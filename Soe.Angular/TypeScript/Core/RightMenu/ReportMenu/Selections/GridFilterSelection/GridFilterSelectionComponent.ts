import { IIdListSelectionDTO, IProjectGridDTO, ISmallGenericType } from "../../../../../Scripts/TypeLite.Net4";
import { BoolSelectionDTO, IdListSelectionDTO } from "../../../../../Common/Models/ReportDataSelectionDTO";
import { SmallGenericType } from "../../../../../Common/Models/SmallGenericType";
import { ICoreService } from "../../../../Services/CoreService";
import { SoeCategoryType, TermGroup } from "../../../../../Util/CommonEnumerations";
import { ISoeGridOptionsAg, SoeGridOptionsAg } from "../../../../../Util/SoeGridOptionsAg";
import { IReportService } from "../../../../Services/reportservice";
import { IProgressHandlerFactory } from "../../../../Handlers/progresshandlerfactory";
import { IProgressHandler } from "../../../../Handlers/ProgressHandler";
import { ITranslationService } from "../../../../Services/TranslationService";
import { Constants } from "../../../../../Util/Constants";
import { SelectionCollection } from "../../SelectionCollection";
import { ReportUserSelectionDTO } from "../../../../../Common/Models/ReportDTOs";
import { GridEvent } from "../../../../../Util/SoeGridOptions";
import { SoeGridOptionsEvent } from "../../../../../Util/Enumerations";

export class GridFilterSelection {
    public static component(): ng.IComponentOptions {
        const options: ng.IComponentOptions = {
            controller: GridFilterSelection,
            templateUrl: soeConfig.baseUrl + "Core/RightMenu/ReportMenu/Selections/GridFilterSelection/GridFilterSelectionView.html",
            bindings: {
                onSelected: "&",
                labelKey: "@",
                hideLabel: "<",
                items: "<",
                selected: "<",
                userSelectionInput: "=",
                userSelection: "=",
                selections: "<",
                projectIds: "=",
                onSearchProjects: "&",
                onSelectedProjectIds: "&"
            }
        };

        return options;
    }

    public static componentKey = "gridFilterSelection";

    //binding properties
    private labelKey: string;    
    private onSelected: (_: { selection: IdListSelectionDTO }) => void = angular.noop;
    private userSelectionInput: IdListSelectionDTO;

    private items: SmallGenericType[];
    private selected: SmallGenericType[] = [];

    private projectStatus: ISmallGenericType[] = [];
    private projectCategory: ISmallGenericType[] = [];

    private soeGridOptions: ISoeGridOptionsAg;
    projectList: IProjectGridDTO[];
    progress: IProgressHandler;

    private selections: SelectionCollection = new SelectionCollection();

    private selectedProjectStatusDict: IdListSelectionDTO;
    private selectedCategoryDict: IdListSelectionDTO;

    private projectStopDate: Date;
    private withoutStopDate = false;
    private withoutEndDate = false;

    private selectedStatusItem: IdListSelectionDTO;
    private selectedProjectCategoryItem: IdListSelectionDTO;

    private statuses: number[] = [];
    private categories: number[] = [];
    private projectIds: number[];

    private onSearchProjects: (_: { projectStatusSelection: GridFilterSelectionObj }) => void = angular.noop;
    private onSelectedProjectIds: (_: { projectIds: IdListSelectionDTO }) => void = angular.noop;
    private projectStatusList: IdListSelectionDTO;
    private projectCategoryList: IdListSelectionDTO;
    private withoutEndDateParam: BoolSelectionDTO;

    private userSelection: ReportUserSelectionDTO;
    //@ngInject
    constructor(private $scope: ng.IScope, private $timeout: ng.ITimeoutService, private coreService: ICoreService, private reportService: IReportService, progressHandlerFactory: IProgressHandlerFactory, private translationService: ITranslationService, private $q: ng.IQService) {

        this.$scope.$watch(() => this.selections, (newVal, oldVal) => {
            if (!newVal)
                return;
        });

        this.$scope.$watch(() => this.userSelection, (newVal, oldVal) => {
            if (!newVal)
                return;
            this.setSavedUserFilters(newVal);
        });

        this.progress = progressHandlerFactory.create();

        this.soeGridOptions = new SoeGridOptionsAg("common.report.selection.projectlist", this.$timeout);
        this.soeGridOptions.translateText = (key, defaultValue) => {
            return this.translationService.translateInstant("core.aggrid." + key);
        }
        this.soeGridOptions.enableGridMenu = false;
        this.soeGridOptions.setMinRowsToShow(15);
        const event: GridEvent[] = [];
        event.push(new GridEvent(SoeGridOptionsEvent.RowSelectionChangedBatch, (row) => this.onSelectionChanged(row)));
        this.soeGridOptions.subscribe(event);
    }

    public $onInit() {
        if (!this.selected)
            this.selected = [];
        this.$q.all([
            this.loadStatus(),
            this.loadCategories(),
            this.setupProjectGrid()]);
        this.selections = new SelectionCollection();
    }

    private loadStatus(): ng.IPromise<any> {
        if (this.projectStatus.length === 0) {
            return this.coreService.getTermGroupContent(TermGroup.ProjectStatus, false, false).then((data: any[]) => {
                data.forEach((x) => {
                    this.projectStatus.push({ id: x.id, name: x.name });
                });
            });
        }
    }

    private loadCategories(): ng.IPromise<any> {
        if (this.projectCategory.length === 0) {
            return this.coreService.getCategories(SoeCategoryType.Project, false, false, false, true).then((categories: any[]) => {
                categories.forEach((c) => {
                    this.projectCategory.push({ id: c.categoryId, name: c.name });
                })
            });
        }
    }

    private setSavedUserFilters(savedValues: ReportUserSelectionDTO) {
        this.withoutStopDate = this.userSelection.getBoolSelection(Constants.REPORTMENU_SELECTION_KEY_WITHOUT_STOP_DATE).value;
    }

    private setupProjectGrid(): ng.IPromise<any> {
        const keys: string[] = [
            "common.report.selection.projectnr",
            "common.report.selection.projectname",
            "common.report.selection.projectleader",
            "common.report.selection.customernr",
            "common.report.selection.customername",
            "common.report.selection.projectstatus",
            "common.stopdate",
            "core.aggrid.totals.filtered",
            "core.aggrid.totals.total",
            "core.aggrid.totals.selected"
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.soeGridOptions.addColumnText("statusName", terms['common.report.selection.projectstatus'], null);
            this.soeGridOptions.addColumnText("number", terms['common.report.selection.projectnr'], null);
            this.soeGridOptions.addColumnText("name", terms['common.report.selection.projectname'], null);
            this.soeGridOptions.addColumnText("managerName", terms['common.report.selection.projectleader'], null);
            this.soeGridOptions.addColumnText("customerNr", terms["common.report.selection.customernr"], null);
            this.soeGridOptions.addColumnText("customerName", terms['common.report.selection.customername'], null);
            this.soeGridOptions.addColumnDate("stopDate", terms['common.stopdate'], null);

            

            this.$timeout(() => {
                this.soeGridOptions.addTotalRow("#totals-grid", {
                    filtered: terms["core.aggrid.totals.filtered"],
                    total: terms["core.aggrid.totals.total"],
                    selected: terms["core.aggrid.totals.selected"]
                });
            });

            this.soeGridOptions.finalizeInitGrid();
        });
    }

    private onSelectionChanged(row) {
        this.projectIds = [];
        row.forEach(o => {
            this.projectIds.push(o.projectId);
        });

        this.onSelectedProjectIds({ projectIds: new IdListSelectionDTO(this.projectIds) });
    }

    private searchProjects(): ng.IPromise<any> {
        var gridSelections = new GridFilterSelectionObj();
        gridSelections.projectStatus = new IdListSelectionDTO(this.statuses);
        gridSelections.projectCategory = new IdListSelectionDTO(this.categories);
        gridSelections.withoutStop = new BoolSelectionDTO(this.withoutEndDate);

        this.onSearchProjects({ projectStatusSelection: gridSelections });

        return this.progress.startLoadingProgress([
            () => this.reportService.getProjectsBySearch(true, this.statuses, this.categories, this.projectStopDate, this.withoutEndDate).then((x) => {
                this.projectList = x;
                this.soeGridOptions.setData(this.projectList);
            })
        ]);

    }

    private onProjectStatusSelectionChanged(selection: IIdListSelectionDTO) {
        this.statuses = [];
        selection.ids.forEach(o => {
            this.statuses.push(o);
        });
    }

    private onProjectCategorySelectionChanged(selection: IIdListSelectionDTO) {
        this.categories = [];
        selection.ids.forEach(o => {
            this.categories.push(o);
        });
    }

    private onWithoutStopDateSelected(selection) {
        this.withoutEndDate = selection.value;
    }

}

export class GridFilterSelectionObj {

    constructor() {
        this.projectStatus = new IdListSelectionDTO([]);
        this.projectCategory = new IdListSelectionDTO([]);
        this.withoutStop = new BoolSelectionDTO(false);
    }

    public projectStatus: IdListSelectionDTO;
    public projectCategory: IdListSelectionDTO;
    public withoutStop: BoolSelectionDTO;
}