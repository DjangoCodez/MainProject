import { PayrollImportEmployeeDTO, PayrollImportEmployeeScheduleDTO, PayrollImportEmployeeTransactionDTO } from "../../../../../Common/Models/PayrollImport";
import { PayrollProductGridDTO } from "../../../../../Common/Models/ProductDTOs";
import { SmallGenericType } from "../../../../../Common/Models/SmallGenericType";
import { EmbeddedGridController } from "../../../../../Core/Controllers/EmbeddedGridController";
import { IGridHandlerFactory } from "../../../../../Core/Handlers/gridhandlerfactory";
import { IProgressHandler } from "../../../../../Core/Handlers/ProgressHandler";
import { IProgressHandlerFactory } from "../../../../../Core/Handlers/progresshandlerfactory";
import { ICoreService } from "../../../../../Core/Services/CoreService";
import { INotificationService } from "../../../../../Core/Services/NotificationService";
import { ITranslationService } from "../../../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../../../Core/Services/UrlHelperService";
import { ISmallGenericType } from "../../../../../Scripts/TypeLite.Net4";
import { CalendarUtility } from "../../../../../Util/CalendarUtility";
import { SoeTimeCodeType, TermGroup, TermGroup_PayrollImportEmployeeTransactionType } from "../../../../../Util/CommonEnumerations";
import { IconLibrary, SoeGridOptionsEvent, SOEMessageBoxButtons, SOEMessageBoxImage } from "../../../../../Util/Enumerations";
import { GridEvent } from "../../../../../Util/SoeGridOptions";
import { IColumnAggregate, IColumnAggregations } from "../../../../../Util/SoeGridOptionsAg";
import { ToolBarButton, ToolBarButtonGroup, ToolBarUtility } from "../../../../../Util/ToolBarUtility";
import { ITimeService } from "../../../../Time/timeservice";
import { IPayrollService } from "../../../PayrollService";
import { EditScheduleDialogController } from "../EditSchedule/EditScheduleDialogController";
import { EditTransactionDialogController } from "../EditTransaction/EditTransactionDialogController";

export class EditEmployeeDialogController {

    // Terms
    private terms: { [index: string]: string; };

    // Lookups
    private progress: IProgressHandler;
    private transactionTypes: ISmallGenericType[];
    private transactionStatuses: ISmallGenericType[];
    private scheduleStatuses: ISmallGenericType[];
    private products: PayrollProductGridDTO[];
    private causes: ISmallGenericType[];
    private timeCodes: ISmallGenericType[];
    private accounts: ISmallGenericType[];
    private settingTypes: SmallGenericType[] = [];

    // Grid
    private transactionsButtonGroups = new Array<ToolBarButtonGroup>();
    private scheduleButtonGroups = new Array<ToolBarButtonGroup>();
    private gridHandlerTransactions: EmbeddedGridController;
    private gridHandlerSchedule: EmbeddedGridController;

    // Properties
    private employeeModified: boolean = false;

    //@ngInject
    constructor(private $uibModalInstance,
        private $uibModal,
        private $timeout: ng.ITimeoutService,
        private $q: ng.IQService,
        private payrollService: IPayrollService,
        private timeService: ITimeService,
        private coreService: ICoreService,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        private urlHelperService: IUrlHelperService,
        progressHandlerFactory: IProgressHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory,
        private employee: PayrollImportEmployeeDTO) {

        if (progressHandlerFactory)
            this.progress = progressHandlerFactory.create();

        this.gridHandlerTransactions = new EmbeddedGridController(gridHandlerFactory, "EditEmployeeDialog.Transactions");
        this.gridHandlerSchedule = new EmbeddedGridController(gridHandlerFactory, "EditEmployeeDialog.Schedule");

        this.progress.startLoadingProgress([() => {
            return this.loadLookups().then(() => {
                this.setTypes();
                this.setupGrids();
                this.$timeout(() => {
                    this.refreshTransactionsGrid();
                    this.refreshScheduleGrid();
                }, 100);
            });
        }]);
    }

    private setupGrids() {
        const timeSpanColumnAggregate = {
            getSeed: () => 0,
            accumulator: (acc, next) => acc + next,
            cellRenderer: this.timeSpanAggregateRenderer.bind(this)
        } as IColumnAggregate;
        const amountColumnAggregate = {
            getSeed: () => 0,
            accumulator: (acc, next) => acc + next,
            cellRenderer: this.amountAggregateRenderer.bind(this)
        } as IColumnAggregate;

        // Transactions
        this.transactionsButtonGroups.push(ToolBarUtility.createGroup(new ToolBarButton("core.newrow", "core.newrow", IconLibrary.FontAwesome, "fa-plus", () => {
            this.editTransaction(null);
        })));

        this.gridHandlerTransactions.gridAg.options.enableGridMenu = false;
        this.gridHandlerTransactions.gridAg.options.setMinRowsToShow(15);
        this.gridHandlerTransactions.gridAg.options.enableRowSelection = false;

        this.gridHandlerTransactions.gridAg.addColumnDate("date", this.terms["common.date"], 60);
        this.gridHandlerTransactions.gridAg.addColumnTime("startTime", this.terms["common.from"], 60);
        this.gridHandlerTransactions.gridAg.addColumnTime("stopTime", this.terms["common.to"], 60);
        this.gridHandlerTransactions.gridAg.addColumnTime("quantityTime", this.terms["common.length"], 60, { minutesToTimeSpan: true, hideDays: true });
        this.gridHandlerTransactions.gridAg.addColumnNumber("quantity", this.terms["common.quantity"], 60);
        this.gridHandlerTransactions.gridAg.addColumnNumber("amount", this.terms["common.amount"], 60, { decimals: 2 });
        this.gridHandlerTransactions.gridAg.addColumnText("typeName", this.terms["common.type"], 100);
        this.gridHandlerTransactions.gridAg.addColumnText("code", this.terms["common.code"], 60);
        this.gridHandlerTransactions.gridAg.addColumnText("typeValue", this.terms["common.name"], null);
        this.gridHandlerTransactions.gridAg.addColumnIcon("statusIcon", null, null, { toolTipField: "statusName" });
        this.gridHandlerTransactions.gridAg.addColumnDelete(this.terms["core.deleterow"], this.initDeleteTransaction.bind(this));
        this.gridHandlerTransactions.gridAg.addColumnEdit(this.terms["core.edit"], this.editTransaction.bind(this));

        const transactionsEvents: GridEvent[] = [];
        transactionsEvents.push(new GridEvent(SoeGridOptionsEvent.RowDoubleClicked, (row) => { this.editTransaction(row); }));
        this.gridHandlerTransactions.gridAg.options.subscribe(transactionsEvents);

        this.gridHandlerTransactions.gridAg.finalizeInitGrid(null, true, "transactions-totals-grid");

        this.$timeout(() => {
            this.gridHandlerTransactions.gridAg.options.addFooterRow("#transactions-totals-grid", {
                "quantityTime": timeSpanColumnAggregate,
                "quantity": "sum",
                "amount": amountColumnAggregate,
            } as IColumnAggregations);
        });

        // Schedule
        this.scheduleButtonGroups.push(ToolBarUtility.createGroup(new ToolBarButton("core.newrow", "core.newrow", IconLibrary.FontAwesome, "fa-plus", () => {
            this.editSchedule(null);
        })));

        this.gridHandlerSchedule.gridAg.options.enableGridMenu = false;
        this.gridHandlerSchedule.gridAg.options.setMinRowsToShow(15);
        this.gridHandlerSchedule.gridAg.options.enableRowSelection = false;

        this.gridHandlerSchedule.gridAg.addColumnDate("date", this.terms["common.date"], 60);
        this.gridHandlerSchedule.gridAg.addColumnTime("startTime", this.terms["common.from"], 60);
        this.gridHandlerSchedule.gridAg.addColumnTime("stopTime", this.terms["common.to"], 60);
        this.gridHandlerSchedule.gridAg.addColumnTime("quantity", this.terms["common.length"], 60, { minutesToTimeSpan: true });
        this.gridHandlerSchedule.gridAg.addColumnIcon(null, null, null, { icon: "fal fa-mug-hot", showIcon: (row) => row.isBreak, toolTip: this.terms["time.schedule.planning.break"] });
        this.gridHandlerSchedule.gridAg.addColumnIcon("statusIcon", null, null, { toolTipField: "statusName" });
        this.gridHandlerSchedule.gridAg.addColumnDelete(this.terms["core.deleterow"], this.initDeleteSchedule.bind(this));
        this.gridHandlerSchedule.gridAg.addColumnEdit(this.terms["core.edit"], this.editSchedule.bind(this));

        const scheduleEvents: GridEvent[] = [];
        scheduleEvents.push(new GridEvent(SoeGridOptionsEvent.RowDoubleClicked, (row) => { this.editSchedule(row); }));
        this.gridHandlerSchedule.gridAg.options.subscribe(scheduleEvents);

        this.gridHandlerSchedule.gridAg.finalizeInitGrid(null, true, "schedule-totals-grid");

        this.$timeout(() => {
            this.gridHandlerSchedule.gridAg.options.addFooterRow("#schedule-totals-grid", {
                "quantity": timeSpanColumnAggregate
            } as IColumnAggregations);
        });
    }

    private timeSpanAggregateRenderer({ data, colDef }) {
        return CalendarUtility.minutesToTimeSpan(parseFloat(data[colDef.field] || "0"), false);
    }

    private amountAggregateRenderer({ data, colDef }) {
        return parseFloat(data[colDef.field] || "0").toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 });
    }

    // SERVICE CALLS

    private loadLookups(): ng.IPromise<any> {
        let deferral = this.$q.defer<any>();

        this.$q.all([
            this.loadTerms(),
            this.loadTransactionTypes(),
            this.loadTransactionStatuses(),
            this.loadScheduleStatuses(),
            this.loadProducts(),
            this.loadCauses(),
            this.loadTimeCodes()
        ]).then(() => {
            deferral.resolve();
        });

        return deferral.promise;
    }

    private loadTerms(): ng.IPromise<any> {
        let keys: string[] = [
            "core.deleterow",
            "core.edit",
            "common.accountingsettings.account",
            "common.amount",
            "common.code",
            "common.date",
            "common.from",
            "common.length",
            "common.name",
            "common.to",
            "common.type",
            "common.quantity",
            "time.payroll.payrollimport.employee.deleterowwarning",
            "time.schedule.planning.break"
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;

            this.settingTypes.push(new SmallGenericType(0, this.terms["common.accountingsettings.account"]));
        });
    }

    private loadTransactionTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.PayrollImportEmployeeTransactionType, false, false, true).then(x => {
            this.transactionTypes = x;
        });
    }

    private loadTransactionStatuses(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.PayrollImportEmployeeTransactionStatus, false, false, true).then(x => {
            this.transactionStatuses = x;
        });
    }

    private loadScheduleStatuses(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.PayrollImportEmployeeScheduleStatus, false, false, true).then(x => {
            this.scheduleStatuses = x;
        });
    }

    private loadProducts(): ng.IPromise<any> {
        return this.payrollService.getPayrollProductsGrid(true).then(x => {
            this.products = x;

            let empty = new PayrollProductGridDTO();
            empty.productId = 0;
            empty.name = '';
            this.products.splice(0, 0, empty);
        });
    }

    private loadCauses(): ng.IPromise<any> {
        return this.timeService.getTimeDeviationCausesDict(true, false).then(x => {
            this.causes = x;
        });
    }

    private loadTimeCodes(): ng.IPromise<any> {
        return this.timeService.getTimeCodesDict(SoeTimeCodeType.AdditionDeduction, true, false).then(x => {
            this.timeCodes = x;
        });
    }

    // ACTIONS    

    private editTransaction(trans: PayrollImportEmployeeTransactionDTO) {
        let options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Payroll/PayrollImports/Dialogs/EditTransaction/editTransactionDialog.html"),
            controller: EditTransactionDialogController,
            controllerAs: "ctrl",
            size: 'lg',
            resolve: {
                transactionTypes: () => { return this.transactionTypes },
                transactionStatuses: () => { return this.transactionStatuses },
                products: () => { return this.products },
                causes: () => { return this.causes },
                timeCodes: () => { return this.timeCodes },
                accounts: () => { return this.accounts },
                settingTypes: () => { return this.settingTypes },
                payrollImportEmployeeId: () => { return this.employee.payrollImportEmployeeId },
                trans: () => { return trans }
            }
        }
        this.$uibModal.open(options).result.then((result: any) => {
            if (result && result.trans) {
                this.payrollService.getPayrollImportEmployeeTransactions(this.employee.payrollImportEmployeeId, true).then(x => {
                    this.employee.transactions = x;
                    this.setTypes();
                    this.employeeModified = true;
                    this.refreshTransactionsGrid();
                });
            }
        });
    }

    private initDeleteTransaction(trans: PayrollImportEmployeeTransactionDTO) {
        let modal = this.notificationService.showDialogEx(this.terms["core.deleterow"], this.terms["time.payroll.payrollimport.employee.deleterowwarning"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
        modal.result.then(val => {
            if (val)
                this.deleteTransaction(trans);
        });
    }

    private deleteTransaction(trans: PayrollImportEmployeeTransactionDTO) {
        this.payrollService.deletePayrollImportEmployeeTransaction(trans.payrollImportEmployeeTransactionId).then(result => {
            if (result.success) {
                _.pull(this.employee.transactions, trans);
                this.employeeModified = true;
                this.refreshTransactionsGrid();
            } else {
                this.notificationService.showErrorDialog(this.terms["core.deleterow"], result.errorMessage, result.stackTrace);
            }
        });
    }

    private editSchedule(schedule: PayrollImportEmployeeScheduleDTO) {
        let options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Payroll/PayrollImports/Dialogs/EditSchedule/editScheduleDialog.html"),
            controller: EditScheduleDialogController,
            controllerAs: "ctrl",
            size: 'lg',
            resolve: {
                scheduleStatuses: () => { return this.scheduleStatuses },
                payrollImportEmployeeId: () => { return this.employee.payrollImportEmployeeId },
                schedule: () => { return schedule }
            }
        }
        this.$uibModal.open(options).result.then((result: any) => {
            if (result && result.schedule) {
                this.payrollService.getPayrollImportEmployeeSchedules(this.employee.payrollImportEmployeeId).then(x => {
                    this.employee.schedule = x;
                    this.employeeModified = true;
                    this.refreshScheduleGrid();
                });
            }
        });
    }

    private initDeleteSchedule(schedule: PayrollImportEmployeeScheduleDTO) {
        let modal = this.notificationService.showDialogEx(this.terms["core.deleterow"], this.terms["time.payroll.payrollimport.employee.deleterowwarning"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
        modal.result.then(val => {
            if (val)
                this.deleteSchedule(schedule);
        });
    }

    private deleteSchedule(schedule: PayrollImportEmployeeScheduleDTO) {
        this.payrollService.deletePayrollImportEmployeeSchedule(schedule.payrollImportEmployeeScheduleId).then(result => {
            if (result.success) {
                _.pull(this.employee.schedule, schedule);
                this.employeeModified = true;
                this.refreshScheduleGrid();
            } else {
                this.notificationService.showErrorDialog(this.terms["core.deleterow"], result.errorMessage, result.stackTrace);
            }
        });
    }

    // HELP-METHODS

    private setTypes() {
        _.forEach(this.employee.transactions, t => {
            switch (t.type) {
                case TermGroup_PayrollImportEmployeeTransactionType.PayrollProduct:
                case TermGroup_PayrollImportEmployeeTransactionType.SalaryAddition:
                    t.typeValue = _.find(this.products, x => x.productId === t.payrollProductId)?.name;
                    break;
                case TermGroup_PayrollImportEmployeeTransactionType.DeviationCause:
                    t.typeValue = _.find(this.causes, x => x.id === t.timeDeviationCauseId)?.name;
                    break;
                case TermGroup_PayrollImportEmployeeTransactionType.Expense:
                    t.typeValue = _.find(this.timeCodes, x => x.id === t.timeDeviationCauseId)?.name;
                    break;
            }
        });
    }

    private refreshTransactionsGrid() {
        this.gridHandlerTransactions.gridAg.setData(this.employee.transactions);
    }

    private refreshScheduleGrid() {
        this.gridHandlerSchedule.gridAg.setData(this.employee.schedule);
    }

    // EVENTS

    private cancel() {
        this.$uibModalInstance.dismiss(this.employeeModified ? 'reload' : 'cancel');
    }
}
