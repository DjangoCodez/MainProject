import { GridControllerBase } from "../../../../Core/Controllers/GridControllerBase";
import { ICoreService } from "../../../../Core/Services/CoreService";
import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { IMessagingService } from "../../../../Core/Services/MessagingService";
import { INotificationService } from "../../../../Core/Services/NotificationService";
import { IUrlHelperService } from "../../../../Core/Services/UrlHelperService";
import { IProjectService } from "../../../../Shared/Billing/Projects/ProjectService";
import { CalendarUtility } from "../../../../Util/CalendarUtility";
import { Feature } from "../../../../Util/CommonEnumerations";

export class ProjectTimeCodesFactory {
    //@ngInject
    public static create(translationService: ITranslationService, urlHelperService: IUrlHelperService): ng.IDirective {

        return {
            templateUrl: urlHelperService.getGlobalUrl('Shared/Billing/Projects/Directives/ProjectTimeCodes.html'),
            scope: {
                projectId: '=',
                readOnly: '=?',
            },
            restrict: 'E',
            replace: true,
            controller: ProjectTimeCodesController,
            controllerAs: 'ctrl',
            bindToController: true
        };
    }
}

class ProjectTimeCodesController extends GridControllerBase {
    // Setup
    private rows: any[] = [];
    private readOnly: boolean;
    private projectId: number;

    // dims
    private dim2Header: any;
    private dim3Header: any;
    private dim4Header: any;
    private dim5Header: any;
    private dim6Header: any;

    //@ngInject
    constructor($http,
        $templateCache,
        $timeout: ng.ITimeoutService,
        $uibModal,
        coreService: ICoreService,
        private projectService: IProjectService,
        translationService: ITranslationService,
        messagingService: IMessagingService,
        notificationService: INotificationService,
        urlHelperService: IUrlHelperService,
        uiGridConstants: uiGrid.IUiGridConstants,
        private $scope: ng.IScope) {

        super("Billing.Projects.List.Directives.ProjectTimeCodes", "", Feature.None, $http, $templateCache, $timeout, $uibModal, coreService, translationService, urlHelperService, messagingService, notificationService, uiGridConstants);

        this.soeGridOptions.enableGridMenu = false;
        this.soeGridOptions.enableColumnMenus = false;
        this.soeGridOptions.enableFiltering = true;
        this.soeGridOptions.enableSorting = false;
        this.soeGridOptions.showGridFooter = false;
        this.soeGridOptions.setMinRowsToShow(8);
        this.setupTypeAhead();
    }

    protected setupCustomToolBar() {

    }

    public setupGrid() {
        this.startLoad();
        this.setupGridColumns();
        this.setupWatchers();
    }

    private setupGridColumns() {
        //TimeCode = 0,
        //TimeRuleName = 1,
        //TimeRuleSort = 2,
        //Start = 3,
        //Stop = 4,
        //Quantity = 5,

        const keys: string[] = [
            "billing.projects.list.timecode",
            "billing.projects.list.timerulename",
            "billing.projects.list.timerulesort",
            "billing.projects.list.start",
            "billing.projects.list.stop",
            "billing.projects.list.quantity"
        ];

        this.translationService.translateMany(keys).then(terms => {

            var colDef1 = this.soeGridOptions.addColumnText("name", terms["billing.projects.list.timecode"], null);
            colDef1.allowCellFocus = false;
            var colDef2 = this.soeGridOptions.addColumnText("timeRuleName", terms["billing.projects.list.timerulename"], null);
            colDef2.allowCellFocus = false;
            var colDef3 = this.soeGridOptions.addColumnText("timeRuleSort", terms["billing.projects.list.timerulesort"], null);
            colDef3.allowCellFocus = false;
            var colDef4 = this.soeGridOptions.addColumnText("start", terms["billing.projects.list.start"], "10%");
            colDef4.allowCellFocus = false;
            var colDef5 = this.soeGridOptions.addColumnText("stop", terms["billing.projects.list.stop"], "10%");
            colDef5.allowCellFocus = false;
            var colDef6 = this.soeGridOptions.addColumnText("quantityspan", terms["billing.projects.list.quantity"], null);
            colDef6.allowCellFocus = false;



        });
    }

    private setupWatchers() {
        this.$scope.$watch(() => this.projectId, () => {
            this.loadTimeCodes();
        });
    }

    protected onBlur(entity, colDef) {

    }

    public filterProducts(filter) {

    }

    // Lookups
    private loadTimeCodes(): ng.IPromise<any> {
        return this.projectService.getTimeCodes(this.projectId).then((x) => {
            this.rows = x;
            _.forEach(x, (row: any) => {
                row.name = row.code + " " + row.codeName;
                var startDate: Date = new Date(row.timeCodeStart);
                row.start = this.formatTimeHHMM(startDate);
                var stopDate: Date = new Date(row.timeCodeStop);
                row.stop = this.formatTimeHHMM(stopDate);
                row.quantityspan = CalendarUtility.minutesToTimeSpan(row.quantity);
            });
            super.gridDataLoaded(this.rows);
        });
    }

    private formatTimeHHMM(d: Date): string {
        function z(n) { return (n < 10 ? '0' : '') + n }
        var h = d.getHours();
        return (z(h) + ':' + z(d.getMinutes())).toString();
    }

    // Actions
    private addRow() {

    }

    protected initDeleteRow(row: any) {

    }
}
