import { GridControllerBase } from "../../../../../Core/Controllers/GridControllerBase";
import { ISmallGenericType, IShiftTypeEmployeeStatisticsTargetDTO } from "../../../../../Scripts/TypeLite.Net4";
import { ICoreService } from "../../../../../Core/Services/CoreService";
import { ITranslationService } from "../../../../../Core/Services/TranslationService";
import { IMessagingService } from "../../../../../Core/Services/MessagingService";
import { INotificationService } from "../../../../../Core/Services/NotificationService";
import { IUrlHelperService } from "../../../../../Core/Services/UrlHelperService";
import { ToolBarUtility, ToolBarButton } from "../../../../../Util/ToolBarUtility";
import { IconLibrary } from "../../../../../Util/Enumerations";
import { Feature, TermGroup, TermGroup_EmployeeStatisticsType } from "../../../../../Util/CommonEnumerations";
import { Constants } from "../../../../../Util/Constants";
import { CalendarUtility } from "../../../../../Util/CalendarUtility";
import { ShiftTypeEmployeeStatisticsTargetDTO } from "../../../../../Common/Models/ShiftTypeDTO";
import { CoreUtility } from "../../../../../Util/CoreUtility";

export class EmployeeStatisticDirectiveFactory {
    //@ngInject
    public static create(translationService: ITranslationService, urlHelperService: IUrlHelperService): ng.IDirective {

        return {
            templateUrl: urlHelperService.getUrl('Directives/EmployeeStatistic/EmployeeStatistic.html'),
            scope: {
                selectedStatisticRows: '=',
                nbrOfRows: '@',
                readOnly: '='
            },
            restrict: 'E',
            replace: true,
            controller: EmployeeStatisticController,
            controllerAs: 'ctrl',
            bindToController: true
        };
    }
}

class EmployeeStatisticController extends GridControllerBase {

    // Setup
    private selectedStatisticRows: IShiftTypeEmployeeStatisticsTargetDTO[];
    private nbrOfRows: number;
    private readOnly: boolean;
    private isTaxiKurir: boolean;

    // Converted init parameters
    private minRowsToShow: number = 8; // Default
    private hideStandardColumn: boolean;

    // Collections
    private terms: any;
    private allRows: any[];
    private statistcTargets: ISmallGenericType[];

    //@ngInject
    constructor($http,
        $templateCache,
        $timeout: ng.ITimeoutService,
        $uibModal,
        private $filter: ng.IFilterService,
        coreService: ICoreService,
        translationService: ITranslationService,
        messagingService: IMessagingService,
        notificationService: INotificationService,
        urlHelperService: IUrlHelperService,
        uiGridConstants: uiGrid.IUiGridConstants,
        private $q: ng.IQService,
        private $scope: ng.IScope) {

        super("Soe.Time.Schedule.ShiftTypes", "", Feature.None, $http, $templateCache, $timeout, $uibModal, coreService, translationService, urlHelperService, messagingService, notificationService, uiGridConstants);

        // Init parameters

        //TODO maybe own feature ?
        this.isTaxiKurir = (CoreUtility.licenseId == 933);

        this.soeGridOptions.enableGridMenu = false;
        this.soeGridOptions.enableColumnMenus = false;
        this.soeGridOptions.enableFiltering = false;
        this.soeGridOptions.enableSorting = false;
        this.soeGridOptions.showGridFooter = false;
        this.soeGridOptions.setMinRowsToShow(this.nbrOfRows ? this.nbrOfRows : 8);
        this.setupTypeAhead();
    }

    protected setupCustomToolBar() {
        this.buttonGroups.push(ToolBarUtility.createGroup(new ToolBarButton("common.newrow", "common.newrow", IconLibrary.FontAwesome, "fa-plus",
            () => { this.addRow(); },
            null,
            () => { return this.readOnly; })));
    }

    public setupGrid() {
        this.startLoad();
        this.$q.all([this.loadTerms(), this.loadStatisticTargets()
        ]).then(() => this.setupGridColumns());
    }

    private setupGridColumns() {
        this.soeGridOptions.addColumnSelect("employeeStatisticsType", this.terms["common.type"], null, this.statistcTargets, false, true, "employeeStatisticsTypeName", "id", "name", "rowChanged");

        //var colDef1 = this.soeGridOptions.addColumnText("employeeStatisticsTypeName", this.terms["common.type"], null);
        //colDef1.enableCellEdit = true;
        var colDef2 = this.soeGridOptions.addColumnNumber("targetValue", this.terms["time.schedule.shifttype.targetvalue"], "15%");
        colDef2.enableCellEdit = true;
        var coldef3 = this.soeGridOptions.addColumnDate("fromDate", this.terms["common.categories.datefrom"], "20%");
        coldef3.enableCellEdit = true;
        this.soeGridOptions.addColumnDelete(this.terms["common.delete"], "initDeleteRow");

        super.gridDataLoaded(this.sortRows());

        this.setupWatchers();
    }

    private setupWatchers() {
        this.$scope.$watch(() => this.selectedStatisticRows, () => {
            super.gridDataLoaded(this.selectedStatisticRows);
        });
    }

    // Lookups

    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
            "common.type",
            "common.categories.selected",
            "time.schedule.shifttype.targetvalue",
            "common.categories.datefrom",
            "core.delete"
        ];

        return this.translationService.translateMany(keys).then(x => {
            this.terms = x;
        });
    }

    private loadStatisticTargets(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.EmployeeStatisticsType, false, false).then(x => {
            this.statistcTargets = [];
            _.forEach(x, (target: any) => {
                if (this.isTaxiKurir) {
                    if (target.id != TermGroup_EmployeeStatisticsType.ArrivalAndGoHome)
                        this.statistcTargets.push(target);
                }
                else {
                    if (target.id != TermGroup_EmployeeStatisticsType.ArrivalAndGoHome && target.id != TermGroup_EmployeeStatisticsType.AnsweredCalls && target.id != TermGroup_EmployeeStatisticsType.CallDuration && target.id != TermGroup_EmployeeStatisticsType.ConnectedTime && target.id != TermGroup_EmployeeStatisticsType.NotAnsweredCalls)
                        this.statistcTargets.push(target);
                }
            });

        });
    }

    // Events

    private selectRow(row) {

    }

    // Actions
    private addRow() {
        var row: ShiftTypeEmployeeStatisticsTargetDTO = new ShiftTypeEmployeeStatisticsTargetDTO();
        var today = new Date();
        row.fromDate = today;

        if (_.size(this.selectedStatisticRows) == 0) {
            this.selectedStatisticRows.push(row);
            super.gridDataLoaded(this.selectedStatisticRows);
        } else {
            this.soeGridOptions.addRow(row);
        }
        this.soeGridOptions.focusRowByRow(row, 0);

    }


    // Help-methods

    private sortRows() {
        return this.selectedStatisticRows
    }

    protected initDeleteRow(row: any) {
        this.soeGridOptions.deleteRow(row);
        this.messagingService.publish(Constants.EVENT_SET_DIRTY, {});
    }
}
