import { EditControllerBase } from "../../../../../Core/Controllers/EditControllerBase";
import { ISoeGridOptions, SoeGridOptions, GridEvent } from "../../../../../Util/SoeGridOptions";
import { ICoreService } from "../../../../../Core/Services/CoreService";
import { IReportService } from "../../../../../Core/Services/ReportService";
import { ITranslationService } from "../../../../../Core/Services/TranslationService";
import { IMessagingService } from "../../../../../Core/Services/MessagingService";
import { INotificationService } from "../../../../../Core/Services/NotificationService";
import { IUrlHelperService, IUrlHelperServiceProvider } from "../../../../../Core/Services/UrlHelperService";
import { IPayrollService } from "../../../PayrollService";
import { TimePeriodDTO } from "../../../../../Common/Models/TimePeriodDTO";
import { AttestPayrollTransactionDTO } from "../../../../../Common/Models/AttestPayrollTransactionDTO";
import { SoeGridOptionsEvent, SOEMessageBoxButtons, SOEMessageBoxSize, SOEMessageBoxImage } from "../../../../../Util/Enumerations";
import { IAttestPayrollTransactionDTO } from "../../../../../Scripts/TypeLite.Net4";
import { Constants } from "../../../../../Util/Constants";
import { TermGroup_TimePeriodType } from "../../../../../Util/CommonEnumerations";

export class UnhandledTransactionsDialogController extends EditControllerBase {

    // Collections
    termsArray: any;

    isLoading: boolean = false;
    isPopulating: boolean = false;

    title: string = "";

    createNewTimePeriod: boolean;
    newTimePeriodName: string;
    newTimePeriodPaymentDate: Date = null;
    createNewTimePeriodStartDate: Date = null;
    createNewTimePeriodStopDate: Date = null;

    protected gridOptions: ISoeGridOptions;

    private _selectedStartDate: Date;
    get selectedStartDate() {
        return this._selectedStartDate;
    }
    set selectedStartDate(date: Date) {
        if (!date) {
            return;
        }
        this._selectedStartDate = new Date(<any>date.toString());
        this.search();
    }

    //@ngInject
    constructor(private $uibModalInstance,
        private $timeout: ng.ITimeoutService,
        $uibModal,
        private payrollService: IPayrollService,
        translationService: ITranslationService,
        coreService: ICoreService,
        notificationService: INotificationService,
        urlHelperService: IUrlHelperService,
        messagingService: IMessagingService,
        private uiGridConstants: uiGrid.IUiGridConstants,
        private $q: ng.IQService,
        private employeeId,
        private timePeriodId,
        private timePeriods: TimePeriodDTO[],
        private transactions: AttestPayrollTransactionDTO[],
        private isBackwards: boolean,
        startDate: Date,
        private stopDate: Date) {

        super(null, null, $uibModal, translationService, messagingService, coreService, notificationService, urlHelperService);

        this._selectedStartDate = startDate;
        this.readOnlyPermission = true;
        this.modifyPermission = true;
        this.initGrid();
        this.setupGrid();
        this.validate();
        this.lookupLoaded();
    }

    // EVENTS    

    protected cancel() {
        this.$uibModalInstance.dismiss('cancel');
    }

    // LOOKUPS

    private initGrid() {
        this.gridOptions = new SoeGridOptions("Time.Payroll.PayrollCalculation.UnhandledTransactionsDialog", this.$timeout, this.uiGridConstants);
        this.gridOptions.enableGridMenu = false;
        this.gridOptions.showGridFooter = false;
        this.gridOptions.enableRowSelection = true;
        this.gridOptions.setData([]);
    }

    private setupGrid() {
        var keys: string[] = [
            //Grid
            "time.payrollproduct.payrollproduct",
            "common.date",
            "common.start",
            "common.stop",
            "common.quantity",
            "common.price",
            "common.amount",
            "common.accounting",
            "time.atteststate.state",
            "time.time.timeperiod.timeperiod",
            "core.info",
            //General
            "time.payroll.payrollcalculation.timeperiodmandatory",
            "time.payroll.payrollcalculation.newtimeperiodnamemandatory",
            "time.payroll.payrollcalculation.newtimeperiodpaymentdatemandatory",
            "common.fromdate",
            "common.todate",
            "time.payroll.payrollcalculation.transactionsmandatory",
        ];

        this.translationService.translateMany(keys).then((terms) => {
            this.termsArray = terms;
            this.gridOptions.addColumnText("payrollProductString", terms["time.payrollproduct.payrollproduct"], "15%");
            this.gridOptions.addColumnDate("date", terms["common.date"], "12%");
            this.gridOptions.addColumnText("startTimeString", terms["common.start"], "8%")
            this.gridOptions.addColumnText("stopTimeString", terms["common.stop"], "8%");
            this.gridOptions.addColumnNumber("quantityString", terms["common.quantity"], "8%");
            this.gridOptions.addColumnNumber("unitPrice", terms["common.price"], "8%");
            this.gridOptions.addColumnNumber("amount", terms["common.amount"], "8%");
            this.gridOptions.addColumnText("accountingShortString", terms["common.accounting"], "10%", false, "accountingLongString");
            this.gridOptions.addColumnShape("attestStateColor", null, null, "", Constants.SHAPE_CIRCLE, "attestStateName",null, "hasAttestState");           
            this.gridOptions.addColumnText("attestStateName", terms["time.atteststate.state"], null);
            this.gridOptions.addColumnText("timePeriodName", terms["time.time.timeperiod.timeperiod"], null);
            this.gridOptions.addColumnIcon(null, "fal fa-info-circle iconEdit", this.termsArray["core.info"], "showProductInformation", "hasInfo", null, null, null, null, false);

            this.gridOptions.subscribe([new GridEvent(SoeGridOptionsEvent.RowSelectionChanged, (row) => {
                this.isDirty = true;
            })]);

            this.gridOptions.subscribe([new GridEvent(SoeGridOptionsEvent.RowSelectionChangedBatch, (rows) => {
                this.isDirty = true;
            })]);


            _.forEach(this.gridOptions.getColumnDefs(), (colDef: uiGrid.IColumnDef) => {
                colDef.enableColumnResizing = true;
                colDef.enableSorting = true;
                colDef.enableColumnMenu = false;
                colDef.enableCellEdit = false;
                colDef.enableFiltering = false;
            });
        });

        this.transactions = _.orderBy(this.transactions, ['dateFrom', 'dateTo', 'payrollProductString', 'accountingShortString'], ['asc']);
        this.gridOptions.setData(this.transactions);
        this.gridOptions.setMinRowsToShow(10);
    }

    protected lookupLoaded() {
        super.lookupLoaded();
        if (this.lookups == 0) {
            this.isLoading = false;
            this.readOnlyPermission = true;
            this.modifyPermission = true;
            this.validate();
        }
    }

    // ACTIONS

    protected validate() {
        if (this.createNewTimePeriod) {
            if (!this.newTimePeriodName || this.newTimePeriodName.length == 0) {
                this.mandatoryFieldKeys.push("time.payroll.payrollcalculation.timeperiodmandatory");
            }
            if (this.newTimePeriodPaymentDate == null) {
                this.mandatoryFieldKeys.push("time.payroll.payrollcalculation.newtimeperiodpaymentdatemandatory");
            }
            if (this.createNewTimePeriodStartDate == null) {
                this.mandatoryFieldKeys.push("common.fromdate");
            }
            if (this.createNewTimePeriodStopDate == null) {
                this.mandatoryFieldKeys.push("common.todate");
            }
        }
        else {
            if (!this.timePeriodId || this.timePeriodId <= 0) {
                this.mandatoryFieldKeys.push("time.payroll.payrollcalculation.timeperiodmandatory");
            }
        }
    }

    protected save() {
        if (this.gridOptions.getSelectedCount() == 0) {
            this.notificationService.showDialog(this.termsArray["core.info"], this.termsArray["time.payroll.payrollcalculation.transactionsmandatory"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OK, SOEMessageBoxSize.Medium);
            return;
        }
        this.startSave();
        var transactionItems: AttestPayrollTransactionDTO[] = [];
        var scheduleTransactionItems: AttestPayrollTransactionDTO[] = [];
        _.forEach(this.gridOptions.getSelectedRows(), (row: AttestPayrollTransactionDTO) => {
            if (row.isScheduleTransaction)
                scheduleTransactionItems.push(row);
            else
                transactionItems.push(row);
            
        });

        var timePeriod = new TimePeriodDTO;
        if (this.createNewTimePeriod) {
            timePeriod.name = this.newTimePeriodName;
            timePeriod.paymentDate = this.newTimePeriodPaymentDate;
            timePeriod.startDate = this.createNewTimePeriodStartDate;
            timePeriod.stopDate = this.createNewTimePeriodStopDate;
            timePeriod.extraPeriod = true;
        }
        else {
            timePeriod = (_.filter(this.timePeriods, { timePeriodId: this.timePeriodId }))[0];
        }

        this.payrollService.assignPayrollTransactionsToTimePeriod(transactionItems, scheduleTransactionItems, timePeriod, TermGroup_TimePeriodType.Payroll, this.employeeId).then((result) => {
            if (result.success) {
                this.completedSave(null);
                this.$uibModalInstance.close(true);
            } else {
                this.failedSave(result.errorMessage);
            }
        }, error => {
            this.failedSave(error.message);
        });
    }

    protected search() {
        this.startLoad();        
        this.payrollService.getUnhandledPayrollTransactions(this.employeeId, this.selectedStartDate, this.stopDate, this.isBackwards).then((result: AttestPayrollTransactionDTO[]) => {
            this.transactions = result;
            this.gridOptions.setData(this.transactions);
            this.stopProgress();
            this.isDirty = false;
        });
    }
}
