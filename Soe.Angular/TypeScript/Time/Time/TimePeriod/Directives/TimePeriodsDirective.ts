import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../../Core/Services/UrlHelperService";
import { GridControllerBase } from "../../../../Core/Controllers/GridControllerBase";
import { TimePeriodDTO } from "../../../../Common/Models/TimePeriodDTO";
import { ICoreService } from "../../../../Core/Services/CoreService";
import { IMessagingService } from "../../../../Core/Services/MessagingService";
import { INotificationService } from "../../../../Core/Services/NotificationService";
import { ToolBarUtility, ToolBarButton } from "../../../../Util/ToolBarUtility";
import { IconLibrary, SOEMessageBoxImage, SOEMessageBoxButtons } from "../../../../Util/Enumerations";
import { Feature, CompanySettingType } from "../../../../Util/CommonEnumerations";
import { Constants } from "../../../../Util/Constants";
import { SettingsUtility } from "../../../../Util/SettingsUtility";
import { CoreUtility } from "../../../../Util/CoreUtility";

export class TimePeriodsDirectiveFactory {
    //@ngInject
    public static create(translationService: ITranslationService, urlHelperService: IUrlHelperService): ng.IDirective {

        return {
            templateUrl: urlHelperService.getGlobalUrl('Time/Time/TimePeriod/Directives/TimePeriods.html'),
            scope: {
                rows: '=',
                timePeriodHeadId: '=',
                readOnly: '=?',
            },
            restrict: 'E',
            replace: true,
            controller: TimePeriodsDirectiveController,
            controllerAs: 'ctrl',
            bindToController: true
        };
    }
}

class TimePeriodsDirectiveController extends GridControllerBase {
    // Setup
    private rows: TimePeriodDTO[];
    private timePeriodHeadId: number;
    private readOnly: boolean;
    private usePayroll: boolean;

    // Collections
    periods: TimePeriodDTO[] = [];
    //Terms
    terms: { [index: string]: string; };

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

        super("Soe.Time.Time.TimePeriod.Directives.TimePeriods", "", Feature.None, $http, $templateCache, $timeout, $uibModal, coreService, translationService, urlHelperService, messagingService, notificationService, uiGridConstants);
    }

    public $onInit() {
        this.setup();
    }

    private setup() {

        if (_.size(this.rows) == 0)
            this.rows = [];

        this.soeGridOptions.enableGridMenu = false;
        this.soeGridOptions.enableColumnMenus = false;
        this.soeGridOptions.enableFiltering = false;
        this.soeGridOptions.enableSorting = false;
        this.soeGridOptions.showGridFooter = false;
        this.soeGridOptions.setMinRowsToShow(8);
        this.setupTypeAhead();
    }

    protected setupCustomToolBar() {
        this.buttonGroups.push(ToolBarUtility.createGroup(new ToolBarButton("time.time.timeperiod.newtimeperiodrow", "time.time.timeperiod.newtimeperiodrow", IconLibrary.FontAwesome, "fa-plus",
            () => { this.addRow(); },
            null,
            () => { return this.readOnly; })));
    }

    private setupWatchers() {
        this.$scope.$watch(() => this.rows, () => {
            super.gridDataLoaded(this.rows);
        });
    }

    public setupGrid() {
        this.loadCompanySettings().then(() => { this.setupGridColumns() });
    }

    public setupGridColumns() {
        var keys: string[] = [
            "core.comment",
            "core.delete",
            "common.name",
            "time.time.timeperiod.extraperiod",
            "time.time.timeperiod.startdate",
            "time.time.timeperiod.stopdate",
            "time.time.timeperiod.payrollstartdate",
            "time.time.timeperiod.payrollstopdate",
            "time.time.timeperiod.paymentdate",
            "time.time.timeperiod.newtimeperiod",
            "time.time.timeperiod.week",
        ];

        this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;

            var colName = this.soeGridOptions.addColumnText("name", terms["common.name"], null);
            colName.enableCellEdit = true;
            var colStartDate = this.soeGridOptions.addColumnDate("startDate", terms["time.time.timeperiod.startdate"], "15%", false);
            colStartDate.enableCellEdit = true;
            var colStopDate = this.soeGridOptions.addColumnDate("stopDate", terms["time.time.timeperiod.stopdate"], "15%", false);
            colStopDate.enableCellEdit = true;
            if (this.usePayroll) {
                var colPayrollStartDate = this.soeGridOptions.addColumnDate("payrollStartDate", terms["time.time.timeperiod.payrollstartdate"], "15%", false);
                colPayrollStartDate.enableCellEdit = true;
                var colPayrollStopDate = this.soeGridOptions.addColumnDate("payrollStopDate", terms["time.time.timeperiod.payrollstopdate"], "15%", false);
                colPayrollStopDate.enableCellEdit = true;
                var colPaymentDate = this.soeGridOptions.addColumnDate("paymentDate", terms["time.time.timeperiod.paymentdate"], "15%", false);
                colPaymentDate.enableCellEdit = true;
                var colExtraPeriod = this.soeGridOptions.addColumnBool("extraPeriod", terms["time.time.timeperiod.extraperiod"], "10%", true);
                colExtraPeriod.enableCellEdit = true;
            }

            this.soeGridOptions.addColumnIcon("icon", null, this.terms["core.comment"], "showComment", null, null, null, null, null, false);
            this.soeGridOptions.addColumnDelete(terms["core.delete"], "initDeleteRow");
            super.gridDataLoaded(this.rows);

            this.setupWatchers();
        });
    }

    private loadCompanySettings(): ng.IPromise<any> {
        var settingTypes: number[] = [];

        settingTypes.push(CompanySettingType.UsePayroll);

        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.usePayroll = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.UsePayroll);
        });
    }

    protected onBlur(entity, colDef) {

    }

    // ACTIONS

    private addRow() {
        var locale = CoreUtility.language;

        var row: TimePeriodDTO = new TimePeriodDTO();
        row.timePeriodHeadId = 0;

        if (_.size(this.rows) == 0) {
            //Propose new period - current month
            var today = new Date();
            row.startDate = new Date(today.getFullYear(), today.getMonth(), 1);
            row.stopDate = new Date(today.getFullYear(), today.getMonth(), today.daysInMonth());
            row.payrollStartDate = null;
            row.payrollStopDate = null;
            row.paymentDate = null;
            row.name = new Date(1900, today.getMonth() + 1, 0).toLocaleString(locale, { month: "long" }) + " " + today.getFullYear().toString();
            row.rowNr = 1;
        }
        else {
            var rowsSorted = this.getRowsSorted();
            if (rowsSorted) {
                //Get last period
                var prevTimePeriodRow = rowsSorted[0];
                if (prevTimePeriodRow) {
                    var prevStopDate = new Date();
                    prevStopDate = moment(prevTimePeriodRow.stopDate).toDate();
                    var prevStartDate = new Date();
                    prevStartDate = moment(prevTimePeriodRow.startDate).toDate();
                    var prevPayrollStartDate = new Date();
                    prevPayrollStartDate = moment(prevTimePeriodRow.payrollStartDate).toDate();
                    var prevPayrollStopDate = new Date();
                    prevPayrollStopDate = moment(prevTimePeriodRow.payrollStopDate).toDate();
                    var prevPaymentDate = new Date();
                    prevPaymentDate = moment(prevTimePeriodRow.paymentDate).toDate();

                    row.startDate = prevStopDate.addDays(1);
                    row.payrollStartDate = prevTimePeriodRow.payrollStopDate != null ? prevPayrollStopDate.addDays(1) : null;
                    row.paymentDate = null;
                    row.rowNr = this.getNextRowNr();

                    var daysInPeriod = prevStopDate.diffDays(prevStartDate);
                    if (daysInPeriod <= 7) {
                        row.stopDate = row.startDate.addDays(daysInPeriod - 1);
                        row.payrollStopDate = row.payrollStartDate ? row.payrollStartDate.addDays(daysInPeriod - 1) : null;
                        row.name = this.terms["time.time.timeperiod.week"] + " " + row.startDate.week().toString() + " " + row.startDate.getFullYear().toString();
                    }
                    else if (daysInPeriod > 7 && daysInPeriod <= 21) {
                        row.stopDate = row.startDate.addDays(daysInPeriod - 1);
                        row.payrollStopDate = row.payrollStartDate ? row.payrollStartDate.addDays(daysInPeriod - 1) : null;
                        row.name = this.terms["time.time.timeperiod.week"] + " " + row.startDate.week().toString() + "-" + row.stopDate.week().toString() + " " + row.startDate.getFullYear().toString();
                    }
                    else if (daysInPeriod >= 21 && daysInPeriod <= 31) {
                        row.stopDate = row.startDate.addMonths(1).addDays(-1);
                        row.payrollStopDate = row.payrollStartDate ? row.payrollStartDate.addMonths(1).addDays(-1) : null;
                        row.name = new Date(1900, (row.startDate.getMonth() + 1), 0).toLocaleString(locale, { month: "long" }) + " " + row.startDate.getFullYear().toString();
                        row.paymentDate = prevPaymentDate ? prevPaymentDate.addMonths(1) : null;
                    }
                    else {
                        row.stopDate = row.startDate.addDays(daysInPeriod);
                        row.payrollStopDate = row.payrollStartDate ? row.payrollStartDate.addDays(daysInPeriod) : null;
                        row.name = this.terms["time.time.timeperiod.newtimeperiod"];
                    }
                }
            }
        }

        this.rows.push(row);
        this.rows = this.getRowsSorted();

        if (_.size(this.rows) == 0) {
            super.gridDataLoaded(this.rows);
        } else {
            this.soeGridOptions.addRow(row);
        }

        this.messagingService.publish(Constants.EVENT_SET_DIRTY, {});
        this.soeGridOptions.focusRowByRow(row, 0);
    }

    protected initDeleteRow(row: any) {
        this.soeGridOptions.deleteRow(row);
        this.messagingService.publish(Constants.EVENT_SET_DIRTY, {});
    }

    //HELP-METHODS

    private getRowsSorted() {
        if (!this.rows)
            this.rows = [];
        else
            return this.rows = _.orderBy(this.rows, ['stopDate'], ['desc'])
    }

    private getNextRowNr() {
        var rowNr = 0;
        var maxRow = _.maxBy(this.rows, 'rowNr');
        if (maxRow)
            rowNr = maxRow.rowNr;
        return rowNr + 1;
    }

    protected allowNavigationFromTypeAhead(entity: TimePeriodDTO, colDef) {
        return true;
    }

    private showComment(timePeriod: TimePeriodDTO) {
        var modal = this.notificationService.showDialogEx(this.terms["core.comment"], '', SOEMessageBoxImage.None, SOEMessageBoxButtons.OKCancel, { showTextBox: true, textBoxValue: timePeriod.comment, textBoxRows: 3 });
        modal.result.then(result => {
            if (result.result) {
                timePeriod.comment = result.textBoxValue;
                this.messagingService.publish(Constants.EVENT_SET_DIRTY, {});
            }
        });
    }

}
