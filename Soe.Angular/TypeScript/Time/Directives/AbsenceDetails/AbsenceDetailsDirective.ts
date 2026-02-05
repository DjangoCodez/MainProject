import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { GridControllerBaseAg } from "../../../Core/Controllers/GridControllerBaseAg";
import { TimeAttestMode, Feature } from "../../../Util/CommonEnumerations";
import { ICoreService } from "../../../Core/Services/CoreService";
import { ITimeService } from "../../Time/TimeService";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { GridEvent } from "../../../Util/SoeGridOptions";
import { SoeGridOptionsEvent, SOEMessageBoxImage, SOEMessageBoxButtons } from "../../../Util/Enumerations";
import { Constants } from "../../../Util/Constants";
import { CalendarUtility } from "../../../Util/CalendarUtility";
import { TimeAbsenceDetailDTO } from "../../../Common/Models/TimeAbsenceDetailDTO";

export class AbsenceDetailsDirectiveFactory {
    //@ngInject
    public static create(translationService: ITranslationService, urlHelperService: IUrlHelperService): ng.IDirective {
        return {
            templateUrl: urlHelperService.getGlobalUrl('Time/Directives/AbsenceDetails/Views/AbsenceDetails.html'),
            scope: {
                registerControl: '&',
                progressBusy: '=?',
                isReadonly: '=?',
                absenceDetails: '=',
            },
            restrict: 'E',
            replace: true,
            controller: AbsenceDetailsController,
            controllerAs: 'directiveCtrl',
            bindToController: true
        };
    }
}

export class AbsenceDetailsController extends GridControllerBaseAg {

    private registerControl: Function;
    private attestMode: TimeAttestMode;
    private terms: any;
    private absenceDetails: TimeAbsenceDetailDTO[];
    private modalInstance: any;
    private get isMyTime(): boolean {
        return this.attestMode == TimeAttestMode.TimeUser;
    }

    // Init parameters
    private showSortButtons: boolean;
    protected gridId: string;

    // Converted init parameters
    private showSortButtonsValue: boolean;

    //ui stuff
    private lastNavigation: { row: any, column: any };
    private gridHeightStyle;

    //@ngInject
    constructor($http,
        $templateCache,
        $timeout: ng.ITimeoutService,
        protected $uibModal,
        protected coreService: ICoreService,
        private timeService: ITimeService,
        translationService: ITranslationService,
        messagingService: IMessagingService,
        notificationService: INotificationService,
        urlHelperService: IUrlHelperService,
        uiGridConstants: uiGrid.IUiGridConstants,
        private $q: ng.IQService,
        private $scope: ng.IScope) {

        super("Common.Directives.AbsenceDetails", "time.time.attest.absencedetails", Feature.None, $http, $templateCache, $timeout, $uibModal, coreService, translationService, urlHelperService, messagingService, notificationService, uiGridConstants, null, null, null, null, null, null, true);
        this.modalInstance = $uibModal;
    }

    // INIT

    public $onInit() {
        //Config parameters
        this.attestMode = soeConfig.attestMode ? soeConfig.attestMode : TimeAttestMode.Time;
        this.showSortButtonsValue = <any>this.showSortButtons === 'true';
        this.initGrid();
        if (this.registerControl)
            this.registerControl({ control: this });
    }

    private initGrid() {
        this.setGridName(false);
        this.soeGridOptions.enableRowSelection = true;
        this.soeGridOptions.enableFiltering = true;

        this.$scope.$on('focusRow', (e, a) => {
            this.soeGridOptions.startEditingCell(a.row - 1, 0);
        });
    }

    private setGridName(restore: boolean) {
        this.soeGridOptions.clearColumnDefs();
        super.setName("Common.Directives.AbsenceDetails")

        if (restore) {
            this.restoreState(true);
            this.soeGridOptions.refreshGrid();
        }
    }

    // SETUP

    public setupGrid() {
        var events: GridEvent[] = [];
        events.push(new GridEvent(SoeGridOptionsEvent.RowSelectionChanged, (row: uiGrid.IGridRow) => { this.gridSelectionChanged(); }));
        events.push(new GridEvent(SoeGridOptionsEvent.RowSelectionChangedBatch, (row: uiGrid.IGridRow) => { this.gridSelectionChanged(); }));
        this.soeGridOptions.subscribe(events);

        this.startLoad();
        this.$q.all([
            this.loadTerms()]).then(() => {
                this.gridAndDataIsReady();
            });
    }

    private setupGridColumns() {
        super.addColumnDate("date", this.terms["common.date"], 100, true, null, null, { suppressMovable: true, enableHiding: true, cellClassRules: { "excelDate": () => true } });
        var colDefDayName = super.addColumnText("dayName", this.terms["time.time.attest.dayname"], 80, { suppressMovable: true, enableHiding: true, clearZero: true, alignLeft: true })
        if (colDefDayName) {
            colDefDayName.cellRenderer = function (params) {
                if (params.data['holidayName'])
                    return '<span style="color:red">' + params.data['holidayName'] + '</span';
                else
                    return '<span>' + params.value + '</span';
            };
            colDefDayName.cellClassRules = {
                "indiscreet": (params) => (params.data.date).isSameDayAs((new Date()).beginningOfDay()),
            };
        }
        super.addColumnText("weekNr", this.terms["common.week"], 60, { suppressMovable: true, enableHiding: true });
        super.addColumnText("sysPayrollTypeLevel3Name", this.terms["time.time.attest.absencedetails.timedeviationcausename"], null, { enableHiding: true, clearZero: true });
        super.addColumnText("timeDeviationCauseName", this.terms["time.time.attest.absencedetails.syspayrolltypelevel3name"], null, { enableHiding: true, clearZero: true });
        super.addColumnText("ratioText", this.terms["time.time.attest.absencedetails.ratio"], 100, { enableHiding: true });
        super.addColumnBool("manuallyAdjusted", this.terms["time.time.attest.absencedetails.manuallyadjusted"], 80);
        super.addColumnDate("modified", this.terms["common.modified"], 80, true, null, null, { enableHiding: true });
        super.addColumnText("modifiedBy", this.terms["common.modifiedby"], 100, { enableHiding: true });
        super.addColumnIcon("info", ' ', null, { icon: "fal fa-info-circle infoColor", toolTip: this.terms["core.info"], onClick: this.showInfo.bind(this), showIcon: this.showInfoIcon.bind(this), pinned: "right", enableHiding: false, enableResizing: false, suppressExport: true });

        this.finalizeGrid();
    }

    private finalizeGrid() {
        this.soeGridOptions.finalizeInitGrid();
        this.restoreState(true);
    }

    private setupWatchers() {
        if (!this.absenceDetails)
            this.absenceDetails = [];

        this.$scope.$watch(() => this.absenceDetails, () => {

            super.gridDataLoaded(this.absenceDetails);
            if (this.absenceDetails) {
                //Need to let the UI-thread purge some work before updating the grid height.
                setTimeout(() => {
                    this.soeGridOptions.updateGridHeightBasedOnActualRows();
                }, 100);
            }
        });
    }

    // DIALOGS

    private showInfoIcon(row: TimeAbsenceDetailDTO): boolean {
        if (row) {
            var message = this.getInfoMessage(row);
            if (message && message.length > 0)
                return true;
        }
        return false;
    }

    private showInfo(row: TimeAbsenceDetailDTO) {
        var message = this.getInfoMessage(row);
        this.notificationService.showDialog(this.terms["core.info"], message, SOEMessageBoxImage.Information, SOEMessageBoxButtons.OK);
    }

    // LOOKUPS

    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
            //Columns
            "core.info",
            "core.edit",
            "core.showinfo",
            "core.functions",
            "common.date",
            "common.week",
            "common.description",
            "common.id",
            "common.created",
            "common.createdby",
            "common.modified",
            "common.modifiedby",
            "time.time.attest.dayname",
            "time.time.attest.absencedetails.timedeviationcausename",
            "time.time.attest.absencedetails.syspayrolltypelevel3name",
            "time.time.attest.absencedetails.ratio",
            "time.time.attest.absencedetails.manuallyadjusted",

            //Dialogs
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
        });
    }

    // ACTIONS

    // EVENTS

    private afterCellEdit(entity, colDef) {

    }

    // HELP-METHODS

    private gridAndDataIsReady() {
        this.setupGridColumns();
        this.setupWatchers();
    }

    private gridSelectionChanged() {
        this.$scope.$applyAsync(() => {
            this.messagingService.publish(Constants.EVENT_ABSENCEDETAILS_ROWS_SELECTED, this.soeGridOptions.getSelectedRows());
        });
    }

    private getInfoMessage(row: TimeAbsenceDetailDTO): string {
        var message: string = '';
        if (row) {
            message += this.terms["common.id"] + " : " + row.timeBlockDateDetailId + "\n";
            message += this.terms["common.created"] + " : " + (row.created ? CalendarUtility.toFormattedDate(row.created) : '') + "\n";
            message += this.terms["common.createdby"] + " : " + (row.createdBy ? row.createdBy : '') + "\n";
            message += this.terms["common.modified"] + " : " + (row.modified ? CalendarUtility.toFormattedDate(row.modified) : '') + "\n";
            message += this.terms["common.modifiedby"] + " : " + (row.modifiedBy ? row.modifiedBy : '') + "\n";
        }
        return message;
    }
}