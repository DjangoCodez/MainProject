import { ITranslationService } from "../../../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../../../Core/Services/UrlHelperService";
import { EmbeddedGridController } from "../../../../../Core/Controllers/EmbeddedGridController";
import { IGridHandlerFactory } from "../../../../../Core/Handlers/gridhandlerfactory";
import { INotificationService } from "../../../../../Core/Services/NotificationService";
import { IMessagingService } from "../../../../../Core/Services/MessagingService";
import { ICoreService } from "../../../../../Core/Services/CoreService";
import { IPayrollService } from "../../../../Payroll/PayrollService";
import { PayrollCalculationProductDTO } from "../../../../../Common/Models/PayrollCalculationProductDTO";
import { AttestPayrollTransactionDTO } from "../../../../../Common/Models/AttestPayrollTransactionDTO";
import { AttestTransitionLogDTO } from "../../../../../Common/Models/AttestTransitionLogDTO";
import { ISmallGenericType } from "../../../../../Scripts/TypeLite.Net4";
import { GridEvent } from "../../../../../Util/SoeGridOptions";
import { SOEMessageBoxImage, SoeGridOptionsEvent } from "../../../../../Util/Enumerations";
import { TermGroup_PayrollProductTimeUnit, TermGroup, Feature } from "../../../../../Util/CommonEnumerations";
import { Constants } from "../../../../../Util/Constants";
import { CalendarUtility } from "../../../../../Util/CalendarUtility";
import { ChangePayrollProductSettingsPeriodDialogControlller } from "../../Dialogs/PayrollProductSettings/ChangePayrollProductSettingsPeriodDialog";
import { AddedTransactionDialogControlller } from "../../Dialogs/AddedTransaction/AddedTransactionDialogController";

export class PayrollCalculationEmployeeDirectiveFactory {
    //@ngInject
    public static create(translationService: ITranslationService, urlHelperService: IUrlHelperService): ng.IDirective {
        return {
            templateUrl: urlHelperService.getGlobalUrl('Time/Payroll/PayrollCalculation/Directives/PayrollCalculationEmployee/PayrollCalculationEmployee.html'),
            scope: {
                rows: '=',
                timePeriodId: '=',
                employeeId: '=',
                ignoreEmploymentStopDate: '='
            },
            restrict: 'E',
            replace: true,
            controller: PayrollCalculationEmployeeController,
            controllerAs: 'directiveCtrl',
            bindToController: true
        };
    }
}

export class PayrollCalculationEmployeeController {

    // Terms
    private terms: { [index: string]: string; };

    // Init parameters
    private rows: PayrollCalculationProductDTO[];
    private timePeriodId: number;
    private employeeId: number;
    private ignoreEmploymentStopDate: boolean;

    // Lookups
    private payrollProductTimeUnits: ISmallGenericType[];

    private gridHandler: EmbeddedGridController;

    //@ngInject
    constructor(
        private $q: ng.IQService,
        private $uibModal,
        private $scope: ng.IScope,
        gridHandlerFactory: IGridHandlerFactory,
        private urlHelperService: IUrlHelperService,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        private messagingService: IMessagingService,
        private coreService: ICoreService,
        private payrollService: IPayrollService) {

        this.gridHandler = new EmbeddedGridController(gridHandlerFactory, "PayrollCalculationEmployee");
    }

    // INIT

    public $onInit() {
        this.doLookups().then(() => {
            this.setupGrid();
            this.setupWatchers();
        })
    }

    private setupWatchers() {
        if (!this.rows)
            this.rows = [];

        this.$scope.$watch(() => this.rows, () => {
            this.gridHandler.gridAg.setData(this.rows);
        });
    }

    private setupGrid() {
        this.gridHandler.gridAg.options.setMinRowsToShow(20);

        // Master
        let colDef = this.gridHandler.gridAg.addColumnText("payrollProductString", this.terms["time.payrollproduct.payrollproduct"], null, true);
        colDef.cellRenderer = 'agGroupCellRenderer';
        this.gridHandler.gridAg.addColumnDate("dateFrom", this.terms["common.fromdate"], 136, true, null, { suppressSizeToFit: true });
        this.gridHandler.gridAg.addColumnDate("dateTo", this.terms["common.todate"], 136, true, null, { suppressSizeToFit: true });
        this.gridHandler.gridAg.addColumnText("quantityString", this.terms["common.quantity"], 50, true, { suppressSizeToFit: true, cellClassRules: { "text-right": (row) => true } });
        this.gridHandler.gridAg.addColumnText("quantity_HH_DD_String", this.terms["time.payroll.payrollcalculation.quantityhhdd"], 30, true, { suppressSizeToFit: false, cellClassRules: { "text-right": (row) => true } });
        this.gridHandler.gridAg.addColumnText("unitPriceString", this.terms["common.price"], 60, true, { suppressSizeToFit: true, cellClassRules: { "text-right": (row) => true } });
        this.gridHandler.gridAg.addColumnNumber("amount", this.terms["common.amount"], 70, { suppressSizeToFit: true, decimals: 2, cellClassRules: { "errorColor": (row) => { return row.data.amount < 0 } } });
        this.gridHandler.gridAg.addColumnText("accountingShortString", this.terms["common.accounting"], 136, true, { suppressSizeToFit: true, toolTipField: "accountingLongString" });
        this.gridHandler.gridAg.addColumnShape("attestStateColor", null, 22, { enableHiding: true, shape: Constants.SHAPE_CIRCLE, toolTipField: "attestStateName", showIconField: "hasAttestStates", suppressExport: true });
        this.gridHandler.gridAg.addColumnText("attestStateName", this.terms["time.atteststate.state"], 136, true, { suppressSizeToFit: true });
        this.gridHandler.gridAg.addColumnEdit(this.terms["core.edit"], this.editPayrollProductPeriodSettings.bind(this));
        this.gridHandler.gridAg.addColumnIcon(null, null, null, { enableHiding: false, icon: "fal fa-comment-dots iconEdit", toolTip: this.terms["core.comment"], showIcon: (row: PayrollCalculationProductDTO) => row.hasComment, onClick: this.showProductComment.bind(this), suppressFilter: true });
        this.gridHandler.gridAg.addColumnIcon(null, null, null, { enableHiding: false, icon: "fal fa-info-circle infoColor", toolTip: this.terms["core.info"], onClick: this.showProductInformation.bind(this), suppressFilter: true });

        var events: GridEvent[] = [];
        events.push(new GridEvent(SoeGridOptionsEvent.RowSelectionChanged, (row) => { this.gridSelectionChanged(); }));
        events.push(new GridEvent(SoeGridOptionsEvent.RowSelectionChangedBatch, (row) => { this.gridSelectionChanged(); }));
        events.push(new GridEvent(SoeGridOptionsEvent.IsRowMaster, (row: PayrollCalculationProductDTO) => { return row.attestPayrollTransactions.length > 0 }));
        this.gridHandler.gridAg.options.subscribe(events);

        this.gridHandler.gridAg.finalizeInitGrid("time.payroll.payrollcalculation.calculation", true);

        // Details
        this.gridHandler.gridAg.enableMasterDetail(false, null, 'attestPayrollTransactions');
        this.gridHandler.gridAg.detailOptions.enableFiltering = false;
        this.gridHandler.gridAg.options.setDetailCellDataCallback((params) => {
            // Hide selection row, since enableRowSelection does not seem to work for detail grids
            let gridName = (params.node && params.node.detailGridInfo ? params.node.detailGridInfo.id : null);
            if (gridName)
                this.gridHandler.gridAg.options.setChildGridColumnVisibility(gridName, 'soe-row-selection', false);

            // Return data
            params.successCallback(params.data['attestPayrollTransactions']);
        });
        this.gridHandler.gridAg.options.setTooltipDelay = 0;
        this.gridHandler.gridAg.detailOptions.addColumnText("timePayrollTransactionId", this.terms["common.transaction"], null);
        this.gridHandler.gridAg.detailOptions.addColumnDate("date", this.terms["common.date"], 136, false, null, null, { suppressSizeToFit: true });
        this.gridHandler.gridAg.detailOptions.addColumnText("startTimeString", this.terms["common.start"], 68, { suppressSizeToFit: true });
        this.gridHandler.gridAg.detailOptions.addColumnText("stopTimeString", this.terms["common.end"], 68, { suppressSizeToFit: true });
        this.gridHandler.gridAg.detailOptions.addColumnText("quantityString", this.terms["common.quantity"], 50, { suppressSizeToFit: true, cellClassRules: { "text-right": (row) => true } });
        this.gridHandler.gridAg.detailOptions.addColumnNumber("unitPrice", this.terms["common.price"], 60, { suppressSizeToFit: true, decimals: 2 });
        this.gridHandler.gridAg.detailOptions.addColumnNumber("amount", this.terms["common.amount"], 70, { suppressSizeToFit: true, decimals: 2 });
        this.gridHandler.gridAg.detailOptions.addColumnText("accountingShortString", this.terms["common.accounting"], 136, { suppressSizeToFit: true, toolTipField: "accountingLongString" });
        this.gridHandler.gridAg.detailOptions.addColumnShape("attestStateColor", null, 22, { enableHiding: false, shape: Constants.SHAPE_CIRCLE, toolTipField: "attestStateName", showIconField: "hasAttestState" });
        this.gridHandler.gridAg.detailOptions.addColumnText("attestStateName", this.terms["time.atteststate.state"], 135, { suppressSizeToFit: true });
        this.gridHandler.gridAg.detailOptions.addColumnEdit(this.terms["core.edit"], this.openAddedTransactionDialog.bind(this), true, (row: AttestPayrollTransactionDTO) => { return row.isAdded });
        this.gridHandler.gridAg.detailOptions.addColumnIcon(null, null, null, { enableHiding: false, icon: "fal fa-comment-dots iconEdit", toolTip: this.terms["core.comment"], showIcon: (row: AttestPayrollTransactionDTO) => row.hasComment, onClick: this.showTransactionComment.bind(this) });
        this.gridHandler.gridAg.detailOptions.addColumnIcon(null, null, null, { enableHiding: false, icon: "fal fa-info-circle infoColor", toolTip: this.terms["core.info"], onClick: this.showTransactionInformation.bind(this) });
        this.gridHandler.gridAg.detailOptions.finalizeInitGrid();
    }

    // SERVICE CALLS

    private doLookups(): ng.IPromise<any> {
        return this.$q.all([
            this.loadTerms(),
            this.loadPayrollProductTimeUnits()
        ]);
    }

    private loadTerms(): ng.IPromise<any> {
        let keys: string[] = [
            "core.comment",
            "core.edit",
            "core.info",
            "common.accounting",
            "common.amount",
            "common.comment",
            "common.createdbyat",
            "common.modifiedbyat",
            "common.date",
            "common.end",
            "common.fromdate",
            "common.by",
            "common.price",
            "common.quantity",
            "common.start",
            "common.to",
            "common.todate",
            "common.transaction",
            "time.atteststate.state",
            "time.payroll.payrollcalculation.calculation",
            "time.payroll.payrollcalculation.attesttransitionlogs",
            "time.payroll.payrollcalculation.productcomments",
            "time.payroll.payrollcalculation.producthaschanges",
            "time.payroll.payrollcalculation.quantityhhdd",
            "time.payrollproduct.payrollproduct",
            "time.time.timeunit"
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
        });
    }

    private loadPayrollProductTimeUnits() {
        this.coreService.getTermGroupContent(TermGroup.PayrollProductTimeUnit, false, false).then(x => {
            this.payrollProductTimeUnits = x;
        });
    }

    // DIALOGS

    private editPayrollProductPeriodSettings(payrollCalculationProduct: PayrollCalculationProductDTO) {
        if (!payrollCalculationProduct)
            return;

        var modal = this.$uibModal.open({
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Payroll/PayrollCalculation/Dialogs/PayrollProductSettings/ChangePayrollProductSettingsPeriodDialog.html"),
            controller: ChangePayrollProductSettingsPeriodDialogControlller,
            controllerAs: 'ctrl',
            backdrop: 'static',
            size: 'md',

            resolve: {
                employeeId: () => { return this.employeeId },
                timePeriodId: () => { return this.timePeriodId },
                productName: () => { return payrollCalculationProduct.payrollProductName },
                payrollProductId: () => { return payrollCalculationProduct.payrollProductId }
            }
        });

        modal.result.then(val => {
            if (val === true) {
                this.messagingService.publish(Constants.EVENT_PAYROLL_CALCULATION_RECALCULATE_PERIOD, false);
            }
        });
    }

    private showProductComment(payrollCalculationProduct: PayrollCalculationProductDTO) {
        if (!payrollCalculationProduct || !payrollCalculationProduct.attestPayrollTransactions || payrollCalculationProduct.attestPayrollTransactions.length === 0)
            return;

        let message: string = "";
        _.forEach(payrollCalculationProduct.attestPayrollTransactions, (transaction: AttestPayrollTransactionDTO) => {
            if (transaction.hasComment) {
                message += "<b>" + this.terms["common.date"] + ": </b>" + new Date(<any>transaction.date).toLocaleDateString() + " <b>" + this.terms["common.quantity"] + ": </b>" + transaction.quantityString + "<br />";
                message += "<b>" + this.terms["common.comment"] + ": </b>" + transaction.comment + "<br /><br />";
            }
        });

        this.notificationService.showDialogEx(this.terms["time.payroll.payrollcalculation.productcomments"], message, SOEMessageBoxImage.Information);
    }

    private showProductInformation(payrollCalculationProduct: PayrollCalculationProductDTO) {
        if (!payrollCalculationProduct)
            return;

        let message: string = payrollCalculationProduct.sysPayrollTypeLevel1Name;
        if (payrollCalculationProduct.sysPayrollTypeLevel2 && payrollCalculationProduct.sysPayrollTypeLevel2Name)
            message += " - {0}".format(payrollCalculationProduct.sysPayrollTypeLevel2Name);
        if (payrollCalculationProduct.sysPayrollTypeLevel3 && payrollCalculationProduct.sysPayrollTypeLevel3Name)
            message += " - {0}".format(payrollCalculationProduct.sysPayrollTypeLevel3Name);
        if (payrollCalculationProduct.sysPayrollTypeLevel4 && payrollCalculationProduct.sysPayrollTypeLevel4Name)
            message += " - {0}".format(payrollCalculationProduct.sysPayrollTypeLevel4Name);

        if (payrollCalculationProduct.hasInfo) {
            message += "<br />";
            message += this.terms["time.payroll.payrollcalculation.producthaschanges"];
        }

        this.notificationService.showDialogEx(this.terms["core.info"], message, SOEMessageBoxImage.Information);
    }

    private openAddedTransactionDialog(transaction: AttestPayrollTransactionDTO) {
        this.messagingService.publish(Constants.EVENT_PAYROLL_CALCULATION_EMPLOYEE_EDIT_ADDED_TRANSACTION, transaction);
    }

    private showTransactionComment(transaction: AttestPayrollTransactionDTO) {
        if (!transaction || !transaction.hasComment)
            return;

        this.notificationService.showDialogEx(this.terms["core.comment"], transaction.comment, SOEMessageBoxImage.Information);
    }

    protected showTransactionInformation(trans: AttestPayrollTransactionDTO) {
        if (!trans)
            return;

        this.payrollService.getAttestTransitionLogs(trans.timeBlockDateId, this.employeeId, trans.timePayrollTransactionId).then((result: AttestTransitionLogDTO[]) => {
            trans.attestTransitionLogs = result;

            var message: string = "{0} ID: {1}\n{2} ID: {3}\n\n".format(
                this.terms["common.transaction"],
                trans.timePayrollTransactionId.toString(),
                this.terms["time.payrollproduct.payrollproduct"],
                trans.payrollProductId.toString());
            
            if (trans.created) {
                message += this.terms["common.createdbyat"].format(trans.createdBy || '', trans.created.toFormattedDateTime()) + "\n";
                if (trans.modified)
                    message += this.terms["common.modifiedbyat"].format(trans.modifiedBy || '', trans.modified.toFormattedDateTime()) + "\n";
                message += "<br />";
            }    

            if (trans.formulaNames || trans.formulaPlain || trans.formulaExtracted) {
                message += "<b>" + this.terms["time.payroll.payrollcalculation.calculation"] + "</b>";
                message += "<br />";
                if (trans.formulaNames) {
                    message += trans.formulaNames;
                    message += "<br />";
                }
                if (trans.formulaPlain) {
                    message += trans.formulaPlain;
                    message += "<br />";
                }
                if (trans.formulaExtracted) {
                    message += trans.formulaExtracted;
                    message += "<br />";
                }
                message += "<br />";
            }

            if (trans.attestTransitionLogs && trans.attestTransitionLogs.length > 0) {
                message += "<b>" + this.terms["time.payroll.payrollcalculation.attesttransitionlogs"] + "</b>";
                message += "<br />";
                _.forEach(trans.attestTransitionLogs, (attestTransitionLog: any) => {
                    message += attestTransitionLog.attestStateFromName + " " + this.terms["common.to"].toLowerCase() + " " + attestTransitionLog.attestStateToName + " " + this.terms["common.by"] + " " + attestTransitionLog.attestTransitionUserName + " " + CalendarUtility.toFormattedDateAndTime(attestTransitionLog.attestTransitionDate);
                    message += "<br />";
                });
                message += "<br /><br />";
            }
            if (this.payrollProductTimeUnits && this.payrollProductTimeUnits.length > 0) {
                message += "<b>" + this.terms["time.time.timeunit"] + "</b>";
                message += "<br />";
                var timeUnitWorkDays = _.find(this.payrollProductTimeUnits, { id: TermGroup_PayrollProductTimeUnit.WorkDays });
                if (timeUnitWorkDays) {
                    message += timeUnitWorkDays["name"] + ": " + trans.quantityWorkDays;
                    message += "<br />";
                }
                var timeUnitCalenderDays = _.find(this.payrollProductTimeUnits, { id: TermGroup_PayrollProductTimeUnit.CalenderDays });
                if (timeUnitCalenderDays) {
                    message += timeUnitCalenderDays["name"] + ": " + trans.quantityCalendarDays;
                    message += "<br />";
                }
                var timeUnitCalenderDayFactor = _.find(this.payrollProductTimeUnits, { id: TermGroup_PayrollProductTimeUnit.CalenderDayFactor });
                if (timeUnitCalenderDayFactor) {
                    message += timeUnitCalenderDayFactor["name"] + ": " + trans.calenderDayFactor;
                    message += "<br />";
                }
                message += "<br /><br />";
            }

            this.notificationService.showDialogEx(this.terms["core.info"], message, SOEMessageBoxImage.Information);
        });
    }

    // EVENTS

    private gridSelectionChanged() {
        this.$scope.$applyAsync(() => {
            this.messagingService.publish(Constants.EVENT_PAYROLL_CALCULATION_EMPLOYEE_ROWS_SELECTED, this.gridHandler.gridAg.options.getSelectedRows());
        });
    }
}