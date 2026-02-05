import { ICoreService } from "../../../Core/Services/CoreService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IProjectGridDTO } from "../../../Scripts/TypeLite.Net4";
import { GridEvent } from "../../../Util/SoeGridOptions";
import { SoeGridOptionsEvent } from "../../../Util/Enumerations";
import { ISelectProjectService } from "./SelectProjectService";
import { Feature, SettingMainType, UserSettingType } from "../../../Util/CommonEnumerations";
import { ISoeGridOptionsAg, SoeGridOptionsAg } from "../../../Util/SoeGridOptionsAg";
import { IShortCutService } from "../../../Core/Services/ShortCutService";
import { SettingsUtility } from "../../../Util/SettingsUtility";

export class SelectProjectController {
    private searching = false;
    private timeout = null;
    private name: string;
    private number: string;
    private customerNr: string;
    private customerName: string;
    private managerName: string;
    private orderNr: string;
    private filteredProject: string;
    private allProjects: IProjectGridDTO[];
    private setupFinished = false;
    private showWithoutCustomer: boolean = false;

    private _showHidden: boolean;
    get showHidden(): boolean {
        return this._showHidden;
    }
    set showHidden(item: boolean) {
        this._showHidden = item;
        if (this.setupFinished)
            this.loadProjects();
    }
    private onlyMineLocked: boolean;
    private _loadMine: boolean;
    get loadMine(): boolean {
        return this._loadMine;
    }
    set loadMine(value: boolean) {
        this._loadMine = value;
    }

    // Grid
    private soeGridOptions: ISoeGridOptionsAg;

    //@ngInject
    constructor(private $uibModalInstance,
        private $timeout: ng.ITimeoutService,
        private coreService: ICoreService,
        private translationService: ITranslationService,
        shortCutService: IShortCutService,
        $scope: ng.IScope,
        private selectProjectService: ISelectProjectService,
        private projects: IProjectGridDTO[],
        private customerId: number,
        private showAllProjects: boolean = false,
        private projectsWithoutCustomer: boolean,
        private showFindHidden: boolean,
        private loadHidden: boolean,
        private useDelete?: boolean,
        private currentProjectNr?: string,
        private currentProjectId?: number,
        private excludedProjectId?: number) {
        shortCutService.bindEnterCloseDialog($scope, () => { this.buttonOkClick(); })
    }

    private $onInit() {
        this.soeGridOptions = new SoeGridOptionsAg("common.dialogs.searchprojects", this.$timeout);
        this.setupGrid();
        this.loadPermissions();

        if (this.loadHidden)
            this.showHidden = true;

        if (this.projectsWithoutCustomer) {
            this.getProjectsWithoutCustomerSetting();
        }
        else {
            this.showWithoutCustomer = true;
        }

        this.$timeout(() => {
            this.soeGridOptions.setFilterFocus();
        });
    }

    private onShowWithoutCustomerChanged(val) {
        if (this.setupFinished) {
            this.$timeout(() => {
                this.coreService.saveBoolSetting(SettingMainType.User, UserSettingType.ProjectDefaultExcludeMissingCustomer, this.showWithoutCustomer);
                this.loadProjects();
            }, 100);
        }
    }

    private loadPermissions(): ng.IPromise<any> {
        const featureIds: number[] = [
            Feature.Billing_Project_ProjectsUser
        ];
        return this.coreService.hasModifyPermissions(featureIds).then((x) => {
            this.onlyMineLocked = x[Feature.Billing_Project_ProjectsUser];
            this.loadMine = this.onlyMineLocked ? true : false;
        });
    }

    private getProjectsWithoutCustomerSetting(): ng.IPromise<any> {
        return this.coreService.getUserSettings([UserSettingType.ProjectDefaultExcludeMissingCustomer], false).then(data => {
            this.showWithoutCustomer = SettingsUtility.getBoolUserSetting(data, UserSettingType.ProjectDefaultExcludeMissingCustomer, false);
        })
    }

    public setupGrid() {

        this.soeGridOptions.enableGridMenu = false;
        this.soeGridOptions.enableFiltering = true;
        this.soeGridOptions.enableRowSelection = false;
        this.soeGridOptions.ignoreResetFilterModel = true;
        this.soeGridOptions.enableSingleSelection();
        this.soeGridOptions.setMinRowsToShow(10);

        // Columns
        const keys: string[] = [
            "common.number",
            "common.name",
            "billing.projects.list.status",
            "billing.projects.list.number",
            "billing.projects.list.name",
            "billing.projects.list.info",
            "billing.projects.list.categories",
            "billing.projects.list.customer",
            "billing.projects.list.underproject",
            "billing.projects.list.openprojectcentral",
            "billing.projects.list.customernr",
            "billing.projects.list.leader",
            "billing.projects.list.ordernr",
            "core.edit"
        ];
        this.translationService.translateMany(keys).then((terms) => {
            this.soeGridOptions.addColumnText("number", terms["billing.projects.list.number"], null, { suppressFilter: true });
            this.soeGridOptions.addColumnText("name", terms["billing.projects.list.name"], null, { suppressFilter: true });
            this.soeGridOptions.addColumnText("customerNr", terms["billing.projects.list.customernr"], null, { suppressFilter: true });
            this.soeGridOptions.addColumnText("customerName", terms["billing.projects.list.customer"], null, { suppressFilter: true });
            this.soeGridOptions.addColumnText("managerName", terms["billing.projects.list.leader"], null, { suppressFilter: true });
            this.soeGridOptions.addColumnText("orderNr", terms["billing.projects.list.ordernr"], null, { suppressFilter: true });

            this.soeGridOptions.finalizeInitGrid();

            // Events
            const events: GridEvent[] = [];

            events.push(new GridEvent(SoeGridOptionsEvent.RowDoubleClicked, (row) => { this.edit(row); }));
            this.soeGridOptions.subscribe(events);

            events.push(new GridEvent(SoeGridOptionsEvent.FilterChanged, () => {
                if (!this.searching)
                    this.loadProjectsFromFilter();
            }));

            if (this.customerId && this.customerId > 0 ) {
                this.$timeout(() => {
                    this.loadProjects(this.currentProjectId && this.currentProjectNr && this.useDelete ? this.currentProjectNr : undefined);
                });
            } else if (this.currentProjectId && this.currentProjectNr && this.useDelete) {
                this.$timeout(() => {
                    this.loadProjects(this.currentProjectNr);
                });
            }

            this.setupFinished = true;
        });
    }

    public loadProjectsFromFilter = _.debounce(() => {
        this.loadProjects();
    }, 800, { leading: false, trailing: true });

    private loadProjects(overrideNr: string = undefined): ng.IPromise<any> {
        const filterModels = this.soeGridOptions.getFilterModels();

        if (!filterModels) {
            return;
        }

        this.searching = true;
        this.soeGridOptions.setData([]);
        const columnValueNumber = overrideNr ? overrideNr : filterModels["number"] ? filterModels["number"].filter : "";
        const columnValueName = filterModels["name"] ? filterModels["name"].filter : "";
        const columnValueCustomerNumber = filterModels["customerNr"] ? filterModels["customerNr"].filter : "";
        const columnValueCustomerName = filterModels["customerName"] ? filterModels["customerName"].filter : "";
        const columnValueManagerName = filterModels["managerName"] ? filterModels["managerName"].filter : "";
        const columnValueOrderNr = filterModels["orderNr"] ? filterModels["orderNr"].filter : "";
        if (!columnValueNumber && !columnValueName && !columnValueCustomerNumber && !columnValueCustomerName && !columnValueManagerName && !columnValueOrderNr && !this.customerId) {
            this.searching = false;
            return;
        }

        return this.selectProjectService.getProjectsBySearch(columnValueNumber, columnValueName, columnValueCustomerNumber, columnValueCustomerName, columnValueManagerName, columnValueOrderNr, true, this.showHidden ? this.showHidden : false, this.showWithoutCustomer ? this.showWithoutCustomer : false, this.loadMine, this.showAllProjects, this.customerId && this.customerId > 0 ? this.customerId : undefined).then(x => {
            this.allProjects = this.excludedProjectId && this.excludedProjectId > 0 ? _.filter(x, (p) => p.projectId !== this.excludedProjectId) : x;
            this.soeGridOptions.setData(this.allProjects);
            if (this.currentProjectId)
                this.selectCurrentProject();
            else
                this.selectFirstRow();
            this.selectCurrentProject();
            this.searching = false;
        });
    }

    selectFirstRow() {
        if (this.allProjects.length > 0 && !this.searching) {
            const row: any = this.soeGridOptions.selectRowByVisibleIndex(0)
            if (row) {
                this.soeGridOptions.selectRow(row)
            }
        }
    }

    private selectCurrentProject() {
        this.$timeout(() => {
            const rowToSelect = _.find(this.allProjects, { 'projectId': this.currentProjectId });
            if (rowToSelect)
                this.soeGridOptions.selectRow(rowToSelect);
            else
                this.selectFirstRow();
        });
    }

    protected edit(row) {
        this.buttonOkClick();
    }

    buttonRemoveClick() {
        this.$uibModalInstance.close({ remove: true });
    }

    buttonOkClick() {
        var projects = this.soeGridOptions.getSelectedRows();
        if (projects[0])
            this.$uibModalInstance.close(projects[0]);
    }

    buttonCancelClick() {
        this.$uibModalInstance.close('cancel');
    }
}