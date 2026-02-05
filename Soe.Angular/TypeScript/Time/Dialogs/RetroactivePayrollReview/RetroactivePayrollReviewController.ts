import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2";
import { ICompositionEditController } from "../../../Core/ICompositionEditController";
import { RetroactivePayrollDTO, RetroactivePayrollEmployeeDTO, RetroactivePayrollOutcomeDTO } from "../../../Common/Models/RetroactivePayroll";
import { SmallGenericType } from "../../../Common/Models/SmallGenericType";
import { ISoeGridOptions, SoeGridOptions, GridEvent } from "../../../Util/SoeGridOptions";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IPayrollService } from "../../Payroll/PayrollService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IDirtyHandlerFactory } from "../../../Core/Handlers/DirtyHandlerFactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IValidationSummaryHandlerFactory } from "../../../Core/Handlers/ValidationSummaryHandlerFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { Constants } from "../../../Util/Constants";
import { Guid } from "../../../Util/StringUtility";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { SoeGridOptionsEvent, RetroactiveFunctions, SOEMessageBoxImage, SOEMessageBoxButtons } from "../../../Util/Enumerations";
import { NumberUtility } from "../../../Util/NumberUtility";
import { AttestPayrollTransactionDTO } from "../../../Common/Models/AttestPayrollTransactionDTO";
import { AttestPayrollTransactionsView } from "../AttestPayrollTransactionsView/AttestPayrollTransactionsView";
import { TermGroup_SoeRetroactivePayrollEmployeeStatus } from "../../../Util/CommonEnumerations";

export const enum TermGroup_RetroactivePayrollReviewGridColumns {
    Unknown = 0,
    productName = 1,
    transactionUnitPrice = 2,
    retroUnitPrice = 3,
    isSpecifiedUnitPrice = 4,
    specifiedUnitPrice = 5,
    retroDiffFormatted = 6,
    amountFormatted = 7,
    errorCodeText = 8,
    edit = 9,
}

export class RetroactivePayrollReviewController extends EditControllerBase2 implements ICompositionEditController {

    protected soeGridOptions: ISoeGridOptions;

    //Lookups
    private retroactivePayroll: RetroactivePayrollDTO;
    private employeeIds: number[];
    private employees: SmallGenericType[];
    private retroEmployees: RetroactivePayrollEmployeeDTO[];
    private retroOutcomes: RetroactivePayrollOutcomeDTO[];
    private retroactivePayrollId: number = 0;
    private terms: any;
    private isModal: boolean = false;
    private isLoaded: boolean = false;
    private currentEmployeeId: number = 0;
    private isLoadingRetroEmployees: boolean = false;

    //Function
    functions: any = [];
    functionTerms: any = [];
    selectedOption: {};

    // Filters
    amountFilter: any;

    //GUI   
    private _selectedRetroEmployee: RetroactivePayrollEmployeeDTO;
    get selectedRetroEmployee() {
        return this._selectedRetroEmployee;
    }
    set selectedRetroEmployee(item: RetroactivePayrollEmployeeDTO) {
        this._selectedRetroEmployee = item;
    }
    private _selectedEmployee: SmallGenericType;
    get selectedEmployee() {
        return this._selectedEmployee;
    }
    set selectedEmployee(item: SmallGenericType) {
        this._selectedEmployee = item;
        this.selectedRetroEmployee = (_.filter(this.retroEmployees, { employeeId: item != null ? item.id : 0 }))[0];
        this.employeeChanged();
    }
    _showOnlyEmployeesWithoutErrors: boolean = false;
    get showOnlyEmployeesWithoutErrors() {
        return this._showOnlyEmployeesWithoutErrors;
    }
    set showOnlyEmployeesWithoutErrors(showOnlyEmployeesWithoutErrors: boolean) {
        this._showOnlyEmployeesWithoutErrors = showOnlyEmployeesWithoutErrors;
    }
    _hideProductsWithoutRetroAmount: boolean = false;
    get hideProductsWithoutRetroAmount() {
        return this._hideProductsWithoutRetroAmount;
    }
    set hideProductsWithoutRetroAmount(hideProductsWithoutRetroAmount: boolean) {
        this._hideProductsWithoutRetroAmount = hideProductsWithoutRetroAmount;
        this.setRetroOutcomesForEmployee();
    }


    //@ngInject
    constructor(
        private $uibModalInstance,
        protected $uibModal,
        private $scope: ng.IScope,
        private $timeout: ng.ITimeoutService,
        private translationService: ITranslationService,
        private payrollService: IPayrollService,
        private urlHelperService: IUrlHelperService,
        private notificationService: INotificationService,
        private uiGridConstants: uiGrid.IUiGridConstants,
        private $filter: ng.IFilterService,
        private controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        private dirtyHandlerFactory: IDirtyHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        isModal: boolean,
        retroactivePayrollId: number) {

        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);

        this.amountFilter = $filter("amount");

        this.setupGrid();

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onDoLookUp(() => this.doLookups())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));

        if (isModal) {
            this.isModal = true;
            var parameters: any = {};
            parameters.retroactivePayrollId = retroactivePayrollId;
            this.onInit(parameters);
        }
    }

    public onInit(parameters: any) {
        this.retroactivePayrollId = parameters.retroactivePayrollId;
        this.guid = parameters.guid;

        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);
        this.flowHandler.start([{ feature: soeConfig.feature, loadReadPermissions: true, loadModifyPermissions: true }]);

    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        //this.readOnlyPermission = response[soeConfig.feature].readPermission;
        //this.modifyPermission = response[soeConfig.feature].modifyPermission;        
    }

    private setupGrid() {
        this.soeGridOptions = new SoeGridOptions("Soe.Time.Dialogs.RetroactivePayrollReview", this.$timeout, this.uiGridConstants);
        this.soeGridOptions.enableGridMenu = false;
        this.soeGridOptions.enableColumnMenus = false;
        this.soeGridOptions.enableFiltering = false;
        this.soeGridOptions.enableSorting = false;
        this.soeGridOptions.showGridFooter = false;

        this.soeGridOptions.enableExpandable = true;
        this.soeGridOptions.enableExpandableRowHeader = true;
        this.soeGridOptions.expandableRowTemplate = '<div ui-grid="row.entity.subGridOptions.gridOptions" style="height:100%;"></div>';
        this.soeGridOptions.expandableRowHeight = 150;
        this.soeGridOptions.setMinRowsToShow(20);
        this.soeGridOptions.expandableRowScope = {
            showTransactionBasis: (transaction) => {
                this.showTransactionBasis(transaction);
            },
        }
    }

    private doLookups() {
        return this.progress.startLoadingProgress([
            () => this.loadRetro(),
            () => this.loadRetroEmployees(),
            () => this.setupGridColumns()

        ]).then(() => {
            this.lookUpsLoaded();
        });
    }

    private lookUpsLoaded() {
        this.isLoaded = true;
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultEditToolbar(true, null, () => this.isNew);
    }

    private setupGridColumns(): ng.IPromise<any> {
        var keys: string[] = [
            "time.payrollproduct.payrollproduct",
            "time.payroll.retroactive.review.originalunitprice",
            "time.payroll.retroactive.review.retrounitprice",
            "time.payroll.retroactive.review.specifiedunitprice",
            "time.payroll.retroactive.review.specifyunitprice",
            "time.payroll.retroactive.review.retrodiffamount",
            "time.payroll.retroactive.review.amount",
            "time.payroll.retroactive.transactiontype",
            "time.payroll.retroactive.review.showbasis",
            "common.accounting",
            "common.quantity",
            "common.amount",
            "common.price",
            "common.transaction",
            "common.date",
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
            this.soeGridOptions.addColumnText("payrollProductString", terms["time.payrollproduct.payrollproduct"], "15%");
            this.soeGridOptions.addColumnText("quantityString", terms["common.quantity"], "6%");
            this.soeGridOptions.addColumnNumber("transactionUnitPrice", terms["time.payroll.retroactive.review.originalunitprice"], "10%", null, 4);
            this.soeGridOptions.addColumnNumber("retroUnitPrice", terms["time.payroll.retroactive.review.retrounitprice"], "10%", null, 4);
            this.soeGridOptions.addColumnBool("isSpecifiedUnitPrice", terms["time.payroll.retroactive.review.specifyunitprice"], "10%", true, "isSpecifiedUnitPrice_Click", null, "isReadOnly");
            this.soeGridOptions.addColumnNumber("specifiedUnitPrice", terms["time.payroll.retroactive.review.specifiedunitprice"], "10%", null, null, null, null, "specifiedUnitPriceDisabled", "specifiedUnitPrice_Changed");
            this.soeGridOptions.addColumnNumber("retroDiffFormatted", terms["time.payroll.retroactive.review.retrodiffamount"], "10%", null, null);
            this.soeGridOptions.addColumnNumber("amountFormatted", terms["time.payroll.retroactive.review.amount"], "10%", null, null);
            this.soeGridOptions.addColumnText("errorCodeText", "", null, false, "errorCodeText");
            this.soeGridOptions.addColumnIcon(null, "fal fa-info-circle iconEdit", terms["time.payroll.retroactive.review.showbasis"], "showOutcomeBasis", null, null, null, null, null, false);
            //this.soeGridOptions.addColumnText("accountingShortString", terms["common.accounting"], null, false, "accountingLongString");

            _.forEach(this.soeGridOptions.getColumnDefs(), (colDef: uiGrid.IColumnDef) => {
                colDef.enableColumnResizing = true;
                colDef.enableSorting = true;
                colDef.enableColumnMenu = false;
                colDef.enableCellEdit = false;
                colDef.enableFiltering = false;
                if (colDef.field == "isSpecifiedUnitPrice" || colDef.field == "specifiedUnitPrice") {
                    colDef.enableCellEdit = true;
                }
                if (colDef.field === "errorCodeText") {
                    var cellcls: string = colDef.cellClass ? colDef.cellClass.toString() : "";
                    colDef.cellClass = (grid: any, row, col, rowRenderIndex, colRenderIndex) => {
                        return cellcls + (row.entity.errorCode > 0 ? " invalid-cell" : "");
                    };
                }
            });

            // Events
            var events: GridEvent[] = [];

            events.push(new GridEvent(SoeGridOptionsEvent.RowExpanded, (row: uiGrid.IGridRow) => {
                if (!row.entity.subGridOptions.dataLoaded) {
                    this.loadRetroactivePayrollOutcomeTransactions(row);
                }
            }));
            this.soeGridOptions.subscribe(events);
        });
    }

    private setupFunctions(): ng.IPromise<any> {
        this.functions = [];

        var keys: string[] = [
            "core.save",
            "time.payroll.retroactive.calculate",
            "time.payroll.retroactive.createtransactions",
            "time.payroll.retroactive.deletetransactions",
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.functionTerms = terms;
            this.functions.push(this.getFunction(RetroactiveFunctions.Save));
            if (this.selectedRetroEmployee && this.selectedRetroEmployee.status != TermGroup_SoeRetroactivePayrollEmployeeStatus.Locked) {
                this.functions.push(this.getFunction(RetroactiveFunctions.Calculate));
                if (this.showDeleteTransactionsFunction()) {
                    this.functions.push(this.getFunction(RetroactiveFunctions.DeleteTransactions));
                }
                else if (this.showCreateTransactionsFunction()) {
                    this.functions.push(this.getFunction(RetroactiveFunctions.CreateTransactions));
                }
            }
            this.setSelectedOption();
        });
    }

    //LOOKUPS

    private loadRetro(): ng.IPromise<any> {
        return this.payrollService.getRetroactivePayroll(this.retroactivePayrollId).then((result: RetroactivePayrollDTO) => {
            this.retroactivePayroll = result;
        });
    }

    private loadRetroEmployees(setSelectedEmployeeId: number = null): ng.IPromise<any> {
        this.isLoadingRetroEmployees = true;
        return this.payrollService.getRetroactivePayrollReviewEmployees(this.retroactivePayrollId).then((result: RetroactivePayrollEmployeeDTO[]) => {
            this.retroEmployees = result;
            this.employees = [];
            this.employeeIds = [];
            _.forEach(this.retroEmployees, (emp: RetroactivePayrollEmployeeDTO) => {
                var employee = new SmallGenericType(emp.employeeId, emp.employeeNr + ' ' + emp.employeeName);
                this.employees.push(employee);
                this.employeeIds.push(emp.employeeId);
                if (setSelectedEmployeeId && setSelectedEmployeeId == emp.employeeId)
                    this.selectedEmployee = employee;
            });

            this.employees = _.filter(this.employees, function (o: SmallGenericType) {
                return o.name
            });
            if (this.employees && !this.selectedEmployee) {
                this.selectedEmployee = _.head(this.employees);
            }
            this.isLoadingRetroEmployees = false;
        });
    }

    private loadRetroOutcomesForEmployee(employeeId: number) {
        this.retroOutcomes = [];
        return this.payrollService.getRetroactivePayrollOutcomeForEmployee(this.retroactivePayrollId, employeeId).then((result: RetroactivePayrollOutcomeDTO[]) => {
            this.retroOutcomes = result;
            this.setRetroOutcomesForEmployee();
            this.setupFunctions();
            _.forEach(this.retroOutcomes, (retroOutcome: RetroactivePayrollOutcomeDTO) => {
                retroOutcome.specifiedUnitPrice = this.amountFilter(retroOutcome.specifiedUnitPrice, 4);
                this.setRetroOutcomeAmount(retroOutcome, NumberUtility.parseDecimal(retroOutcome.amount.toString()))
                retroOutcome["subGridOptions"] = new SoeGridOptions(retroOutcome.payrollProductString, this.$timeout, this.uiGridConstants);
                retroOutcome["subGridOptions"].enableFiltering = false;
                retroOutcome["subGridOptions"].showGridFooter = false;
                retroOutcome["subGridOptions"].enableGridMenu = false;
            });
        });
    }

    private loadRetroactivePayrollOutcomeTransactions(row: uiGrid.IGridRow) {
        return this.payrollService.getRetroactivePayrollOutcomeTransactions(row.entity.employeeId, row.entity.retroactivePayrollOutcomeId).then((result: AttestPayrollTransactionDTO[]) => {
            row.entity.subGridOptions.enableSorting = false;
            row.entity.subGridOptions.addColumnText("timePayrollTransactionId", this.terms["common.transaction"], "8%");
            row.entity.subGridOptions.addColumnDate("date", this.terms["common.date"], "7%");
            row.entity.subGridOptions.addColumnText("quantityString", this.terms["common.quantity"], "7%");
            row.entity.subGridOptions.addColumnText("retroTransactionType", this.terms["time.payroll.retroactive.transactiontype"], "11%");
            row.entity.subGridOptions.addColumnNumber("unitPrice", this.terms["common.price"], "10%", null, 2);
            row.entity.subGridOptions.addColumnNumber("amount", this.terms["common.amount"], "11%", null, 2);
            row.entity.subGridOptions.addColumnText("accountingShortString", this.terms["common.accounting"], null, false, "accountingLongString");
            row.entity.subGridOptions.addColumnIcon(null, "fal fa-info-circle iconEdit", this.terms["time.payroll.retroactive.review.showbasis"], "showTransactionBasis", null, null, null, null, null, true, "", true);
            row.entity.subGridOptions.setData(result);
            row.entity.subGridOptions.dataLoaded = true;
        });
    }

    private loadRetroactivePayrollBasisForOutcome(row: RetroactivePayrollOutcomeDTO) {
        return this.payrollService.getRetroactivePayrollBasisForOutcome(this.selectedEmployee.id, row.retroactivePayrollOutcomeId).then((result: AttestPayrollTransactionDTO[]) => {
            this.showBasisDialog(result);
        });
    }

    private loadRetroactivePayrollBasisForTransaction(transaction: AttestPayrollTransactionDTO) {
        return this.payrollService.getRetroactivePayrollBasisForTransaction(this.selectedEmployee.id, 0, transaction.isScheduleTransaction ? 0 : transaction.timePayrollTransactionId, transaction.isScheduleTransaction ? transaction.timePayrollTransactionId : 0).then((result: AttestPayrollTransactionDTO[]) => {
            this.showBasisDialog(result);
        });
    }

    //ACTIONS

    hasChanges: boolean = false;
    private save() {
        if (this.selectedEmployee && this.selectedEmployee.id) {

            var outComesToSave: RetroactivePayrollOutcomeDTO[] = [];
            _.forEach(this.retroOutcomes, (outcome: RetroactivePayrollOutcomeDTO) => {
                if (!outcome.isReversed && !outcome.hasTransactions) {
                    var copy = new RetroactivePayrollOutcomeDTO();
                    copy.actorCompanyId = outcome.actorCompanyId;
                    copy.employeeId = outcome.employeeId;
                    copy.isQuantity = outcome.isQuantity;
                    copy.isRetroCalculated = outcome.isRetroCalculated;
                    copy.isReversed = outcome.isReversed;
                    copy.isSpecifiedUnitPrice = outcome.isSpecifiedUnitPrice;
                    copy.productId = outcome.productId;
                    copy.quantity = outcome.quantity;
                    copy.resultType = outcome.resultType;
                    copy.retroactivePayrolIEmployeeId = outcome.retroactivePayrolIEmployeeId;
                    copy.retroactivePayrollOutcomeId = outcome.retroactivePayrollOutcomeId;
                    copy.retroUnitPrice = outcome.retroUnitPrice;
                    copy.transactionUnitPrice = outcome.transactionUnitPrice;
                    copy.specifiedUnitPrice = outcome.specifiedUnitPrice;
                    copy.specifiedUnitPrice = NumberUtility.parseDecimal(copy.specifiedUnitPrice.toString())
                    copy.amount = outcome.amount;
                    outComesToSave.push(copy);
                }
            });

            this.progress.startWorkProgress((completion) => {
                this.payrollService.saveRetroactivePayrollOutcomes(this.retroactivePayrollId, this.selectedEmployee.id, outComesToSave).then((result) => {
                    if (result.success) {
                        this.hasChanges = true;
                        completion.completed(Constants.EVENT_EDIT_SAVED);
                        this.loadRetroOutcomesForEmployee(this.selectedEmployee.id);
                    } else {
                        completion.failed(result.errorMessage);
                    }
                }, error => {
                    completion.failed(error.message);
                });
            });
        }
    }

    private calculate() {
        if (!this.validateCalculate())
            return;

        var employees: number[] = [];
        employees.push(this.selectedEmployee.id);

        this.progress.startWorkProgress((completion) => {
            this.payrollService.calculateRetroactivePayroll(this.retroactivePayroll, false, employees).then((result) => {
                if (result.success) {
                    this.hasChanges = true;
                    this.loadRetroEmployees(this.selectedEmployee.id);
                    completion.completed(result);
                    this.loadRetroOutcomesForEmployee(this.selectedEmployee.id);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        });
    }

    private createTransactions() {
        if (!this.validateCreateTransactions())
            return;

        var employees: number[] = [];
        employees.push(this.selectedEmployee.id);

        this.progress.startWorkProgress((completion) => {
            this.payrollService.createRetroactivePayrollTransactions(this.retroactivePayroll, employees).then((result) => {
                if (result.success) {
                    this.hasChanges = true;
                    this.loadRetroEmployees(this.selectedEmployee.id);
                    completion.completed(result);
                    this.loadRetroOutcomesForEmployee(this.selectedEmployee.id);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        });
    }

    private deleteTransactions() {
        if (!this.validateDeleteTransactions())
            return;

        var employees: number[] = [];
        employees.push(this.selectedEmployee.id);

        this.progress.startWorkProgress((completion) => {
            this.payrollService.deleteRetroactivePayrollTransactions(this.retroactivePayroll, employees).then((result) => {
                if (result.success) {
                    this.hasChanges = true;
                    this.loadRetroEmployees(this.selectedEmployee.id);
                    completion.completed(result);
                    this.loadRetroOutcomesForEmployee(this.selectedEmployee.id);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        });
    }

    //DIALOGS

    private initDeleteTransactions() {
        if (!this.validateDeleteTransactions())
            return;

        // Show verification dialog
        var keys: string[] = [
            "core.warning",
            "time.payroll.retroactive.deletetransactionsquestion",
        ];
        this.translationService.translateMany(keys).then((terms) => {
            var modal = this.notificationService.showDialog(terms["core.warning"], terms["time.payroll.retroactive.deletetransactionsquestion"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
            modal.result.then(val => {
                if (val) {
                    this.deleteTransactions();
                }
            });
        });
    }

    private initCalculate() {
        if (!this.validateCalculate())
            return;

        // Show verification dialog
        var keys: string[] = [
            "core.warning",
            "time.payroll.retroactive.calculatequestion",
        ];
        this.translationService.translateMany(keys).then((terms) => {
            var modal = this.notificationService.showDialog(terms["core.warning"], terms["time.payroll.retroactive.calculatequestion"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
            modal.result.then(val => {
                if (val) {
                    this.calculate();
                }
            });
        });
    }

    private initCreateTransactons() {
        if (!this.validateCreateTransactions())
            return;

        // Show verification dialog
        var keys: string[] = [
            "core.warning",
            "time.payroll.retroactive.createtransactionsquestion",
        ];
        this.translationService.translateMany(keys).then((terms) => {
            var modal = this.notificationService.showDialog(terms["core.warning"], terms["time.payroll.retroactive.createtransactionsquestion"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
            modal.result.then(val => {
                if (val) {
                    this.createTransactions();
                }
            });
        });
    }

    private showBasisDialog(transactions: AttestPayrollTransactionDTO[]) {
        var modal = this.$uibModal.open({
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Dialogs/AttestPayrollTransactionsView/attestPayrollTransactionsView.html"),
            controller: AttestPayrollTransactionsView,
            controllerAs: 'ctrl',
            backdrop: 'static',
            size: 'lg',
            windowClass: 'fullsize-modal',
            scope: this.$scope,
            resolve: {

                transactions: () => { return transactions },
            }
        });

        modal.result.then(result => {

        });
    }

    //EVENTS

    protected showTransactionBasis(transaction: AttestPayrollTransactionDTO) {
        if (!transaction)
            return;

        this.loadRetroactivePayrollBasisForTransaction(transaction);
    }

    protected showOutcomeBasis(row: RetroactivePayrollOutcomeDTO) {
        if (!row)
            return;

        this.loadRetroactivePayrollBasisForOutcome(row);
    }

    protected isSpecifiedUnitPrice_Click(row: RetroactivePayrollOutcomeDTO) {
        this.$timeout(() => {
            if (!row.isSpecifiedUnitPrice)
                row.specifiedUnitPrice = this.amountFilter(0, 4);

            this.calculateAmount(row);

            if (row.isSpecifiedUnitPrice)
                this.soeGridOptions.focusRowByRow(row, TermGroup_RetroactivePayrollReviewGridColumns.specifiedUnitPrice);
        });  
    }

    protected specifiedUnitPriceDisabled(row: RetroactivePayrollOutcomeDTO): boolean {
        return row.isReversed || row.hasTransactions;
    }

    protected specifiedUnitPrice_Changed(row: RetroactivePayrollOutcomeDTO) {
        this.$timeout(() => {
            this.calculateAmount(row);
        });
        
    }

    protected calculateAmount(outcome: RetroactivePayrollOutcomeDTO) {
        if (!outcome.specifiedUnitPrice)
            outcome.specifiedUnitPrice = 0;

        var quantity = outcome.isQuantity ? outcome.quantity : NumberUtility.parseDecimal((outcome.quantity / 60).toString());
        var price = 0;
        if (outcome.isSpecifiedUnitPrice)
            price = NumberUtility.parseDecimal(outcome.specifiedUnitPrice.toString()) - NumberUtility.parseDecimal(outcome.transactionUnitPrice.toString());
        else
            price = outcome.retroUnitPrice ? NumberUtility.parseDecimal(outcome.retroUnitPrice.toString()) - NumberUtility.parseDecimal(outcome.transactionUnitPrice.toString()) : 0;

        this.calcualateRetroOutcomeAmount(outcome, quantity, price);
    }

    protected employeeChanged() {
        if (this.selectedEmployee && this.selectedEmployee.id) {
            this.currentEmployeeId = this.selectedEmployee.id;
            this.loadRetroOutcomesForEmployee(this.selectedEmployee.id)
        }
    }

    protected close() {
        this.$uibModalInstance.close({ hasChanges: this.hasChanges });
    }

    // HELP-METHODS

    private getSelectedOptionLabelKey(option: RetroactiveFunctions): string {
        var labelKey: string = "";
        switch (option) {
            case RetroactiveFunctions.Save:
                labelKey = "core.save";
                break;
            case RetroactiveFunctions.SaveAndCalculate:
                labelKey = "time.payroll.retroactive.saveandcalculate";
                break;
            case RetroactiveFunctions.Calculate:
                labelKey = "time.payroll.retroactive.calculate";
                break;
            case RetroactiveFunctions.CreateTransactions:
                labelKey = "time.payroll.retroactive.createtransactions";
                break;
            case RetroactiveFunctions.DeleteTransactions:
                labelKey = "time.payroll.retroactive.deletetransactions";
                break;
        }
        return labelKey;
    }

    private setSelectedOption() {
        if (this.retroactivePayrollId > 0) {
            this.selectedOption = this.getFunction(RetroactiveFunctions.Calculate);
        }
        else {
            this.selectedOption = this.getFunction(RetroactiveFunctions.Save);
        }
    }

    private getFunction(option: RetroactiveFunctions) {
        return {
            id: option,
            name: this.functionTerms[this.getSelectedOptionLabelKey(option)],
        }
    }

    private executeFunction(option) {
        switch (option.id) {
            case RetroactiveFunctions.Save:
                this.save();
                break;
            case RetroactiveFunctions.Calculate:
                this.initCalculate();
                break;
            case RetroactiveFunctions.CreateTransactions:
                this.initCreateTransactons();
                break;
            case RetroactiveFunctions.DeleteTransactions:
                this.initDeleteTransactions();
                break;
        }
    }

    private getValidRetroEmployees(): RetroactivePayrollEmployeeDTO[] {
        return _.filter(this.retroEmployees, r => r.employeeId > 0);
    }

    private getRetroEmployeesTotalAmount() {
        var totalAmount: number = 0;
        _.each(this.getValidRetroEmployees(), retroEmployee => {
            if (totalAmount === 0)
                totalAmount = retroEmployee.totalAmount;
            else
                totalAmount = totalAmount + retroEmployee.totalAmount;
        });
        totalAmount = NumberUtility.parseDecimal(totalAmount.toString());
        totalAmount = this.amountFilter(totalAmount, 4);
        return totalAmount;
    }

    private getCurrentRetroEmployeeAmount() {
        var totalAmount: number = 0;
        _.each(this.retroOutcomes, retroOutcome => {
            if (retroOutcome.amount && retroOutcome.amount != 0) {
                if (totalAmount === 0)
                    totalAmount = retroOutcome.amount;
                else
                    totalAmount = totalAmount + retroOutcome.amount;
            }
        });
        totalAmount = NumberUtility.parseDecimal(totalAmount.toString());
        totalAmount = this.amountFilter(totalAmount, 4);
        return totalAmount;
    }

    private getRetroEmployeeAmount(retroEmployee: RetroactivePayrollEmployeeDTO) {
        var totalAmount: number = 0;
        totalAmount = NumberUtility.parseDecimal(retroEmployee.totalAmount.toString());
        totalAmount = this.amountFilter(totalAmount, 2);
        return totalAmount;
    }

    private calcualateRetroOutcomeAmount(retroOutcome: RetroactivePayrollOutcomeDTO, quantity: number, price: number) {
        if (!retroOutcome)
            return;

        this.setRetroOutcomeAmount(retroOutcome, quantity * NumberUtility.parseDecimal(price.toString()));
    }

    private setRetroOutcomesForEmployee() {
        this.soeGridOptions.setData(this.filteredRetroOutcomes());
    }

    private filteredRetroOutcomes() {
        if (this.retroOutcomes && this.hideProductsWithoutRetroAmount) {
            return _.filter(this.retroOutcomes, r => r.amount != 0);
        }
        else {
            return this.retroOutcomes;
        }
    }

    private setRetroOutcomeAmount(retroOutcome: RetroactivePayrollOutcomeDTO, amount: number) {
        if (!retroOutcome)
            return;
        retroOutcome.amount = amount;
        retroOutcome.retroDiffFormatted = '';
        if (retroOutcome.isSpecifiedUnitPrice)
            retroOutcome.retroDiffFormatted = this.amountFilter(retroOutcome.specifiedUnitPrice - retroOutcome.transactionUnitPrice, 4);
        else
            retroOutcome.retroDiffFormatted = this.amountFilter(retroOutcome.retroUnitPrice - retroOutcome.transactionUnitPrice, 4);
        retroOutcome.amountFormatted = this.amountFilter(retroOutcome.amount, 4);
    }

    private showCreateTransactionsFunction(): boolean {
        return this.selectedRetroEmployee && this.selectedRetroEmployee.status != TermGroup_SoeRetroactivePayrollEmployeeStatus.Locked && (_.filter(this.retroOutcomes, r => !r.hasTransactions)).length > 0;
    }

    private showDeleteTransactionsFunction(): boolean {
        return this.selectedRetroEmployee && this.selectedRetroEmployee.status != TermGroup_SoeRetroactivePayrollEmployeeStatus.Locked && (_.filter(this.retroOutcomes, r => r.hasTransactions)).length > 0;
    }

    private isRetroEmployeeActive(retroEmployee: RetroactivePayrollEmployeeDTO): boolean {
        return retroEmployee && this.selectedEmployee && retroEmployee.employeeId == this.selectedEmployee.id;
    }

    private isReadonly() {
        return false; //TODO
    }

    protected navigateTo(id: number) {
        var employee = _.find(this.employees, { id: id });
        if (employee)
            this.selectedEmployee = employee;
    }

    //VALIDATION

    private validateCalculate(): boolean {
        return this.selectedRetroEmployee != null;
    }

    private validateCreateTransactions(): boolean {
        return this.selectedRetroEmployee != null;
    }

    private validateDeleteTransactions(): boolean {
        return this.selectedRetroEmployee != null;
    }
}