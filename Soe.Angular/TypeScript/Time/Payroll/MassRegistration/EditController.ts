import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2";
import { ICompositionEditController } from "../../../Core/ICompositionEditController";
import { ICoreService } from "../../../Core/Services/CoreService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IValidationSummaryHandlerFactory } from "../../../Core/Handlers/ValidationSummaryHandlerFactory";
import { Feature, TermGroup, TermGroup_MassRegistrationInputType } from "../../../Util/CommonEnumerations";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IDirtyHandlerFactory } from "../../../Core/Handlers/DirtyHandlerFactory";
import { ISmallGenericType, IEmployeeSmallDTO, ISelectableTimePeriodDTO } from "../../../Scripts/TypeLite.Net4";
import { Constants } from "../../../Util/Constants";
import { IFocusService } from "../../../Core/Services/focusservice";
import { ProductSmallDTO } from "../../../Common/Models/ProductDTOs";
import { IPayrollService } from "../PayrollService";
import { SOEMessageBoxImage, SOEMessageBoxButtons, MassRegistrationFunctions, MassRegistrationRowFunctions, SoeGridOptionsEvent } from "../../../Util/Enumerations";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { IReportDataService } from "../../../Core/RightMenu/ReportMenu/ReportDataService";
import { MassRegistrationTemplateHeadDTO, MassRegistrationTemplateRowDTO } from "../../../Common/Models/MassRegistrationDTOs";
import { EmbeddedGridController } from "../../../Core/Controllers/EmbeddedGridController";
import { IGridHandlerFactory } from "../../../Core/Handlers/gridhandlerfactory";
import { AccountDimSmallDTO } from "../../../Common/Models/AccountDimDTO";
import { EmployeeService as SharedEmployeeService } from "../../../Shared/Time/Employee/EmployeeService";
import { EmployeeSmallDTO } from "../../../Common/Models/EmployeeListDTO";
import { AddRowsDialogController } from "./Directives/AddRows/AddRowsDialogController";
import { GridEvent } from "../../../Util/SoeGridOptions";
import { CalendarUtility } from "../../../Util/CalendarUtility";
import { CoreUtility } from "../../../Util/CoreUtility";
import { ImportRowsDialogController } from "./Directives/ImportRows/ImportRowsDialogController";

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    // Terms
    private terms: { [index: string]: string; };

    // Data
    private massRegistrationTemplateHeadId: number;
    private head: MassRegistrationTemplateHeadDTO;

    private inputTypes: ISmallGenericType[];
    private employees: EmployeeSmallDTO[];
    private allTimePeriods: ISelectableTimePeriodDTO[];
    private payrollProducts: ProductSmallDTO[];
    private accountDims: AccountDimSmallDTO[];

    private dim2AccountId: number;
    private dim3AccountId: number;
    private dim4AccountId: number;
    private dim5AccountId: number;
    private dim6AccountId: number;

    // Grid
    private gridHandler: EmbeddedGridController;
    private steppingRules: any;
    private paymentDate: Date;
    private paymentDates: any[] = [];

    // Functions
    private rowFunctions: any = [];
    private buttonFunctions: any = [];
    private loading: boolean = false;

    //@ngInject
    constructor(
        private $q: ng.IQService,
        private $timeout: ng.ITimeoutService,
        private $uibModal,
        private $window,
        private coreService: ICoreService,
        private payrollService: IPayrollService,
        private sharedEmployeeService: SharedEmployeeService,
        private focusService: IFocusService,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        private reportDataService: IReportDataService,
        private urlHelperService: IUrlHelperService,
        progressHandlerFactory: IProgressHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory,
        private controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        private dirtyHandlerFactory: IDirtyHandlerFactory) {
        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onLoadData(() => this.onLoadData())
            .onDoLookUp(() => this.onDoLookups())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));

        this.gridHandler = new EmbeddedGridController(gridHandlerFactory, "MassRegistrationRows");
    }

    public onInit(parameters: any) {
        this.massRegistrationTemplateHeadId = parameters.id;
        this.guid = parameters.guid;
        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);

        this.flowHandler.start([{ feature: Feature.Time_Payroll_MassRegistration, loadReadPermissions: true, loadModifyPermissions: true }]);
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Time_Payroll_MassRegistration].readPermission;
        this.modifyPermission = response[Feature.Time_Payroll_MassRegistration].modifyPermission;
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultEditToolbar(false);
    }

    private setupGrid() {
        this.gridHandler.gridAg.addColumnTypeAhead("employeeNr", this.terms["common.employee"], 200, {
            minWidth: 200,
            maxWidth: 200,
            editable: (data) => this.modifyPermission,
            secondRow: "employeeName",
            hideSecondRowSeparator: true,
            onChanged: (data) => this.onEmployeeChanged(data),
            typeAheadOptions: {
                source: (filter) => this.filterEmployees(filter),
                displayField: "numberAndName",
                dataField: "employeeNr",
                updater: null,
                minLength: 0,
                delay: 0,
                useScroll: true,
                allowNavigationFromTypeAhead: (value) => this.allowNavigationFromEmployee(value)
            }
        });

        this.gridHandler.gridAg.addColumnSelect("paymentDate", this.terms["time.payroll.massregistration.row.paymentdate"], 135, { selectOptions: this.paymentDates, dropdownIdLabel: "date", dropdownValueLabel: "label", displayField: "paymentDateFormatted", editable: this.modifyPermission, suppressSizeToFit: true });

        this.gridHandler.gridAg.addColumnTypeAhead("productNr", this.terms["common.payrollproduct"], 135, {
            suppressSizeToFit: true,
            editable: (data) => this.modifyPermission,
            secondRow: "productName",
            hideSecondRowSeparator: true,
            onChanged: (data) => this.onPayrollProductChanged(data),
            typeAheadOptions: {
                source: (filter) => this.filterPayrollProducts(filter),
                displayField: "numberName",
                dataField: "number",
                updater: null,
                minLength: 0,
                delay: 0,
                useScroll: true,
                allowNavigationFromTypeAhead: (value) => this.allowNavigationFromPayrollProduct(value)
            }
        });

        this.gridHandler.gridAg.addColumnDate("dateFrom", this.terms["common.from"], 135, false, null, { editable: this.modifyPermission, suppressSizeToFit: true });
        this.gridHandler.gridAg.addColumnDate("dateTo", this.terms["common.to"], 135, false, null, { editable: this.modifyPermission, suppressSizeToFit: true });
        this.gridHandler.gridAg.addColumnNumber("quantity", this.terms["common.quantity"], 75, { decimals: 2, editable: true, suppressSizeToFit: true });
        this.gridHandler.gridAg.addColumnBoolEx("isSpecifiedUnitPrice", this.terms["time.payroll.massregistration.row.isspecifiedunitprice"], 80, { enableEdit: this.modifyPermission, suppressSizeToFit: true });
        this.gridHandler.gridAg.addColumnNumber("unitPrice", this.terms["common.price"], 75, { decimals: 2, editable: this.modifyPermission, suppressSizeToFit: true });

        this.accountDims.forEach((ad, i) => {
            let index = i + 1;

            this.gridHandler.gridAg.addColumnTypeAhead(`dim${index}Nr`, ad.name, null, {
                editable: (data) => this.modifyPermission && !data[`dim${index}Disabled`],
                error: `dim${index}Error`,
                secondRow: `dim${index}Name`,
                hideSecondRowSeparator: true,
                onChanged: (data) => this.onAccountChanged(data, index),
                typeAheadOptions: {
                    source: (filter) => this.filterAccounts(i, filter),
                    displayField: "numberName",
                    dataField: "accountNr",
                    updater: null,
                    minLength: 0,
                    delay: 0,
                    useScroll: true,
                    allowNavigationFromTypeAhead: (value) => this.allowNavigationFromAccount(value, index)
                }
            }, {
                dimIndex: index,
            });
        });

        this.gridHandler.gridAg.addColumnDelete(this.terms["core.deleterow"], this.deleteRow.bind(this));

        var events: GridEvent[] = [];
        events.push(new GridEvent(SoeGridOptionsEvent.AfterCellEdit, (entity, colDef, newValue, oldValue) => { this.afterCellEdit(entity, colDef, newValue, oldValue); }));
        this.gridHandler.gridAg.options.subscribe(events);

        this.gridHandler.gridAg.options.setMinRowsToShow(15);
        this.gridHandler.gridAg.finalizeInitGrid("time.payroll.massregistration.rows", true);

        this.setupSteppingRules();
        this.gridHandler.gridAg.options.customTabToCellHandler = (params) => this.handleNavigateToNextCell(params);
    }

    private setupSteppingRules() {
        this.$timeout(() => {
            var mappings =
            {
                employeeNr(row: MassRegistrationTemplateRowDTO) { return true },
                paymentDate(row: MassRegistrationTemplateRowDTO) { return this.head.stopOnPaymentDate },
                productNr(row: MassRegistrationTemplateRowDTO) { return this.head.stopOnProduct },
                dateFrom(row: MassRegistrationTemplateRowDTO) { return this.head.stopOnDateFrom },
                dateTo(row: MassRegistrationTemplateRowDTO) { return this.head.stopOnDateTo },
                quantity(row: MassRegistrationTemplateRowDTO) { return this.head.stopOnQuantity },
                isSpecifiedUnitPrice(row: MassRegistrationTemplateRowDTO) { return this.head.stopOnIsSpecifiedUnitPrice },
                unitPrice(row: MassRegistrationTemplateRowDTO) { return this.head.stopOnUnitPrice },
                dim1Nr(row: MassRegistrationTemplateRowDTO) { return true },
                dim2Nr(row: MassRegistrationTemplateRowDTO) { return true },
                dim3Nr(row: MassRegistrationTemplateRowDTO) { return true },
                dim4Nr(row: MassRegistrationTemplateRowDTO) { return true },
                dim5Nr(row: MassRegistrationTemplateRowDTO) { return true },
                dim6Nr(row: MassRegistrationTemplateRowDTO) { return true }
            };

            this.steppingRules = mappings;
        });
    }

    private setupFunctions() {
        this.rowFunctions.push({ id: MassRegistrationRowFunctions.AddRow, name: this.terms["time.payroll.massregistration.rowfunctions.addrow"] });
        this.rowFunctions.push({ id: MassRegistrationRowFunctions.AddRows, name: this.terms["time.payroll.massregistration.rowfunctions.addrows"] });
        this.rowFunctions.push({});
        this.rowFunctions.push({ id: MassRegistrationRowFunctions.ExportRows, name: this.terms["time.payroll.massregistration.rowfunctions.exportrows"] });
        this.rowFunctions.push({ id: MassRegistrationRowFunctions.ImportRows, name: this.terms["time.payroll.massregistration.rowfunctions.importrows"] });

        this.buttonFunctions.push({ id: MassRegistrationFunctions.CreateTransactions, name: this.terms["time.payroll.massregistration.functions.createtransactions"] });
        this.buttonFunctions.push({});
        this.buttonFunctions.push({ id: MassRegistrationFunctions.DeleteTemplate, name: this.terms["time.payroll.massregistration.functions.deletetemplate"] });
        this.buttonFunctions.push({ id: MassRegistrationFunctions.DeleteTemplateAndTransactions, name: this.terms["time.payroll.massregistration.functions.deletetemplateandtransactions"] });
    }

    private onDoLookups(): ng.IPromise<any> {
        return this.progress.startLoadingProgress([
            () => this.loadTerms(),
            () => this.loadInputTypes(),
            () => this.loadEmployees(),
            () => this.loadPeriods(),
            () => this.loadPayrollProducts(),
            () => this.loadAccounts()
        ]).then(() => {
            this.setupGrid();
            this.setupFunctions();
        });
    }

    private onLoadData(createTransactions: boolean = false): ng.IPromise<any> {
        if (this.massRegistrationTemplateHeadId) {
            return this.progress.startLoadingProgress([ () => this.load(createTransactions)]);
        } else {
            this.new();
        }
    }

    // SERVICE CALLS

    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
            "core.save",
            "core.deleterow",
            "core.deleting",
            "common.from",
            "common.to",
            "common.employee",
            "common.payrollproduct",
            "common.price",
            "common.quantity",
            "time.payroll.massregistration.rowfunctions.addrow",
            "time.payroll.massregistration.rowfunctions.addrows",
            "time.payroll.massregistration.rowfunctions.exportrows",
            "time.payroll.massregistration.rowfunctions.importrows",
            "time.payroll.massregistration.rowfunctions.importrows.clearrows",
            "time.payroll.massregistration.rowfunctions.importrows.error",
            "time.payroll.massregistration.rowfunctions.importrows.type",
            "time.payroll.massregistration.delete.errortitle",
            "time.payroll.massregistration.save.errortitle",
            "time.payroll.massregistration.row.paymentdate",
            "time.payroll.massregistration.row.isspecifiedunitprice",
            "time.payroll.massregistration.functions.createtransactions",
            "time.payroll.massregistration.functions.deletetemplate",
            "time.payroll.massregistration.functions.deletetemplateandtransactions",
            "time.payroll.massregistration.createtransactions.progress",
            "time.payroll.massregistration.createtransactions.success",
            "time.payroll.massregistration.createtransactions.errortitle",
            "time.payroll.massregistration.export.error",
            "time.payroll.massregistration.export.progress"
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
        });
    }

    private loadInputTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.MassRegistrationTemplateInputType, false, true).then(x => {
            this.inputTypes = x;
        });
    }

    private loadEmployees(): ng.IPromise<any> {
        return this.sharedEmployeeService.getEmployeesForGridSmall(true).then(x => {
            this.employees = x;
        });
    }

    private loadPeriods(): ng.IPromise<any> {
        return this.reportDataService.getAllPayrollTimePeriods().then(x => {
            this.allTimePeriods = x;
            // Create a list of distinct dates for the dropdown
            _.forEach(x, y => {
                if (!CalendarUtility.includesDate(_.map(this.paymentDates, p => p.date), y.paymentDate))
                    this.paymentDates.push({ date: y.paymentDate, label: y.paymentDate.toFormattedDate() });
            });
        });
    }

    private loadPayrollProducts(): ng.IPromise<any> {
        return this.payrollService.getPayrollProductsSmall(true).then(x => {
            this.payrollProducts = x;

            let empty = new ProductSmallDTO();
            empty.productId = 0;
            empty.number = '';
            empty.name = '';
            empty.numberName = '';

            this.payrollProducts.splice(0, 0, empty);
        });
    }

    private loadAccounts(): ng.IPromise<any> {
        return this.coreService.getAccountDimsSmall(false, false, true, true, false, false, false).then(x => {
            this.accountDims = x;

            this.accountDims.forEach(ad => {
                if (!ad.accounts)
                    ad.accounts = [];

                if (ad.accounts.length === 0 || ad.accounts[0].accountId !== 0) {
                    (<any[]>ad.accounts).unshift({ accountId: 0, accountNr: '', name: '', numberName: ' ' });
                }
            });
        });
    }

    private load(createTransactions: boolean = false): ng.IPromise<any> {
        this.loading = true;
        return this.payrollService.getMassRegistration(this.massRegistrationTemplateHeadId).then(x => {
            this.isNew = false;
            this.head = x;
            if (this.head.paymentDate)
                this.paymentDate = _.find(this.paymentDates, p => p.date.isSameDayAs(this.head.paymentDate)).date;
            
            this.refreshGrid();
            this.dirtyHandler.clean();
            this.messagingHandler.publishSetTabLabel(this.guid, this.head.name);
            this.loading = false;
            if (createTransactions)
                this.createTransactions();
        });
    }

    private new() {
        this.isNew = true;
        this.massRegistrationTemplateHeadId = 0;
        this.head = new MassRegistrationTemplateHeadDTO();
        this.head.inputType = TermGroup_MassRegistrationInputType.Automatic;
        this.head.isActive = true;
        this.head.rows = [];
    }

    // ACTIONS

    private executeAddRowFunction(option) {
        switch (option.id) {
            case MassRegistrationRowFunctions.AddRow:
                this.addRow(true);
                break;
            case MassRegistrationRowFunctions.AddRows:
                this.addRows();
                break;
            case MassRegistrationRowFunctions.ExportRows:
                this.exportRows();
                break;
            case MassRegistrationRowFunctions.ImportRows:
                this.importRows();
                break;
        }
    }

    private executeFunction(option) {
        switch (option.id) {
            case MassRegistrationFunctions.CreateTransactions:
                this.createTransactions();
                break;
            case MassRegistrationFunctions.DeleteTemplate:
                this.initDelete(false);
                break;
            case MassRegistrationFunctions.DeleteTemplateAndTransactions:
                this.initDelete(true);
                break;
        }
    }

    private initSave(createTransactions: boolean = false) {
        if (this.validateSave())
            this.save(createTransactions);
    }

    private save(createTransactions: boolean = false) {
        this.head.paymentDate = this.paymentDate;
        this.progress.startSaveProgress((completion) => {
            this.payrollService.saveMassRegistration(this.head).then(result => {
                if (result.success) {
                    this.massRegistrationTemplateHeadId = result.integerValue;
                    this.head.massRegistrationTemplateHeadId = this.massRegistrationTemplateHeadId;
                    completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.head);
                    this.dirtyHandler.clean();
                } else {
                    completion.failed("{0}\n\n{1}".format(this.terms["time.payroll.massregistration.save.errortitle"], result.errorMessage));
                }
            }, error => {
                completion.failed(error.message);
            });
        }, this.guid).then(data => {
            this.onLoadData(createTransactions);
        }, error => {
        });
    }

    private createTransactions() {
        if (this.dirtyHandler.isDirty) {
            this.initSave(true);
            return;
        }

        this.progress.startWorkProgress((completion) => {
            this.payrollService.createMassRegistrationTransactions(this.head).then(result => {
                if (result.success) {
                    completion.completed(null, false, this.terms["time.payroll.massregistration.createtransactions.success"]);
                } else {
                    completion.failed("{0}\n\n{1}".format(this.terms["time.payroll.massregistration.createtransactions.errortitle"], result.errorMessage));
                }
            }, error => {
                completion.failed(error.message);
            });
        }, null, this.terms["time.payroll.massregistration.createtransactions.progress"]).then(data => {
            this.onLoadData();
        }, error => {
        });
    }

    private initDelete(deleteTransactions: boolean) {
        var keys: string[] = [
            "core.warning",
            "time.payroll.massregistration.delete.message",
            "time.payroll.massregistration.deletetransactions.message"
        ];

        this.translationService.translateMany(keys).then(terms => {
            var modal = this.notificationService.showDialogEx(terms["core.warning"], deleteTransactions ? terms["time.payroll.massregistration.deletetransactions.message"] : terms["time.payroll.massregistration.delete.message"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
            modal.result.then(val => {
                if (val != null && val === true) {
                    this.delete(deleteTransactions);
                };
            });
        });
    }

    private delete(deleteTransactions: boolean) {
        this.progress.startWorkProgress((completion) => {
            this.payrollService.deleteMassRegistration(this.head.massRegistrationTemplateHeadId, deleteTransactions).then(result => {
                if (result.success) {
                    completion.completed(this.head, true);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }, null, this.terms["core.deleting"]).then(x => {
            super.closeMe(true);
        });
    }

    // EVENTS

    private dateFromChanged() {
        this.$timeout(() => {
            this.head.dateTo = this.head.dateFrom;
        });
    }

    private isSpecifiedUnitPriceChanging() {
        this.$timeout(() => {
            if (!this.head.isSpecifiedUnitPrice)
                this.head.unitPrice = 0;
        });
    }

    private addRow(setFocus: boolean): { rowIndex: number, row: MassRegistrationTemplateRowDTO } {
        let row = new MassRegistrationTemplateRowDTO();
        // Set template values from head
        row.employeeNr = row.employeeName = '';
        row.paymentDate = this.paymentDate;
        let product = this.head.payrollProductId ? _.find(this.payrollProducts, p => p.productId === this.head.payrollProductId) : null;
        row.productId = product ? product.productId : 0;
        row.productNr = product ? product.number : '';
        row.productName = product ? product.name : '';
        row.dateFrom = this.head.dateFrom;
        row.dateTo = this.head.dateTo;
        row.quantity = this.head.quantity;
        row.isSpecifiedUnitPrice = this.head.isSpecifiedUnitPrice;
        row.unitPrice = this.head.unitPrice;

        for (let i = 1; i < 7; i++) {
            this.setRowAccount(row, i);
        }

        const rowIndex = this.gridHandler.gridAg.options.addRow(row, setFocus, this.getFirstColumn());
        this.head.rows.push(row);
        this.setDirty();

        return { rowIndex, row };
    }

    private addRows() {
        var options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Payroll/MassRegistration/Directives/AddRows/AddRowsDialog.html"),
            controller: AddRowsDialogController,
            controllerAs: "ctrl",
            size: 'lg',
            resolve: {
                allTimePeriods: () => { return this.allTimePeriods },
                existingPaymentDates: () => { return this.head.rows.map(r => r.paymentDate) }
            }
        }
        this.$uibModal.open(options).result.then((result: any) => {
            if (result && result.employeeIds.length > 0) {
                this.paymentDate = result.paymentDate;
                _.forEach(result.employeeIds, (employeeId: number) => {
                    let employee = _.find(this.employees, e => e.employeeId === employeeId);
                    if (employee) {
                        let row = this.addRow(false).row;
                        row.employeeId = employeeId;
                        row.employeeNr = employee.employeeNr;
                        row.employeeName = employee.name;
                    }
                });
                this.paymentDate = _.find(this.paymentDates, p => p.date.isSameDayAs(this.head.paymentDate)).date;
            }
        });
    }

    private exportRows() {
        let clone: MassRegistrationTemplateHeadDTO = CoreUtility.cloneDTO(this.head);
        clone.rows = this.gridHandler.gridAg.options.getSelectedRows();

        this.progress.startWorkProgress((completion) => {
            this.payrollService.exportMassRegistration(clone).then((result) => {
                if (result) {
                    completion.completed(null, true);
                    window.location.assign(result);
                } else {
                    completion.failed(this.terms["time.payroll.massregistration.export.error"]);
                }
            }, error => {
                completion.failed(error.message);
            });
        }, null, this.terms["time.payroll.massregistration.export.progress"]);
    }

    private importRows() {
        if (this.head.hasCreatedTransactions) {
            let keys: string[] = [
                "time.payroll.massregistration.rowfunctions.importrows.hastransactions.title",
                "time.payroll.massregistration.rowfunctions.importrows.hastransactions.message"];

            this.translationService.translateMany(keys).then(terms => {
                this.notificationService.showDialogEx(terms["time.payroll.massregistration.rowfunctions.importrows.hastransactions.title"], terms["time.payroll.massregistration.rowfunctions.importrows.hastransactions.message"], SOEMessageBoxImage.Forbidden);
            });
            return;
        }

        var options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Payroll/MassRegistration/Directives/ImportRows/ImportRowsDialog.html"),
            controller: ImportRowsDialogController,
            controllerAs: "ctrl",
            size: 'sm',
            resolve: {
                paymentDates: () => { return this.paymentDates }
            }
        }

        this.$uibModal.open(options).result.then((result: any) => {
            if (result) {
                let rows: MassRegistrationTemplateRowDTO[] = result.rows
                if (!rows)
                    rows = [];
                rows = rows.map(r => {
                    let obj = new MassRegistrationTemplateRowDTO();
                    angular.extend(obj, r);
                    obj.fixDates();
                    obj.setTypes();
                    return obj;
                });

                if (rows.length > 0) {
                    let errorRows = _.filter(rows, r => r.errorMessage);
                    if (errorRows.length > 0) {
                        let msg: string = '';
                        _.forEach(errorRows, row => {
                            msg += row.errorMessage + "\n";
                        });
                        this.notificationService.showDialogEx(this.terms["time.payroll.massregistration.rowfunctions.importrows"], msg, SOEMessageBoxImage.Error);
                    } else {
                        let warningRows = _.filter(rows, r => r.warnings);
                        let msg: string = '';
                        _.forEach(warningRows, row => {
                            msg += row.warnings + "\n";
                        });
                        if (msg !='')
                            this.notificationService.showDialogEx(this.terms["time.payroll.massregistration.rowfunctions.importrows"], msg, SOEMessageBoxImage.Warning);
                        this.head.rows = result.options.clearRows ? rows : this.head.rows.concat(rows);
                        this.refreshGrid();
                        this.setDirty();
                    }
                } else {
                    this.notificationService.showDialogEx(this.terms["time.payroll.massregistration.rowfunctions.importrows"], this.terms["time.payroll.massregistration.rowfunctions.importrows.error"], SOEMessageBoxImage.Error);
                }
            }
        });
    }

    private deleteRow(row) {
        _.pull(this.head.rows, row);
        this.refreshGrid();
        this.setDirty();
    }

    private afterCellEdit(entity, colDef, newValue, oldValue) {
        if (newValue !== oldValue)
            this.setDirty();
    }

    private onEmployeeChanged(data) {
        let row: MassRegistrationTemplateRowDTO = data.data;
        let employee = this.findEmployee(row);

        row.employeeId = employee ? employee.employeeId : 0;
        row.employeeNr = employee ? employee.employeeNr : '';
        row.employeeName = employee ? employee.name : '';
    }

    private onPayrollProductChanged(data) {
        let row: MassRegistrationTemplateRowDTO = data.data;
        let product = this.findPayrollProduct(row);

        row.productId = product ? product.productId : 0;
        row.productName = product ? product.name : '';
    }

    private onAccountChanged(data, dimIndex) {
        let row: MassRegistrationTemplateRowDTO = data.data;
        let account = this.findAccount(row, dimIndex);

        row[`dim${dimIndex}Id`] = account ? account.accountId : 0;
        row[`dim${dimIndex}Name`] = account ? account.name : "";
    }

    // HELP-METHODS

    private setDirty() {
        this.dirtyHandler.setDirty();
    }

    private refreshGrid() {
        this.gridHandler.gridAg.setData(this.head.rows);
    }

    private setRowAccount(row: MassRegistrationTemplateRowDTO, dimIndex: number) {
        row[`dim${dimIndex}Nr`] = '';
        row[`dim${dimIndex}Name`] = '';

        if (this.head[`dim${dimIndex}Id`]) {
            row[`dim${dimIndex}Id`] = this.head[`dim${dimIndex}Id`];
            _.forEach(this.accountDims, dim => {
                let account = _.find(dim.accounts, a => a.accountId === row[`dim${dimIndex}Id`]);
                if (account) {
                    row[`dim${dimIndex}Nr`] = account.accountNr;
                    row[`dim${dimIndex}Name`] = account.name;
                    return false;
                }
            });
        }
    }

    private filterEmployees(filter) {
        return _.orderBy(this.employees.filter(e => {
            if (parseInt(filter))
                return e.employeeNr.startsWithCaseInsensitive(filter);

            return e.name.startsWithCaseInsensitive(filter) || e.name.contains(filter);
        }), 'name');
    }

    private findEmployee(row: MassRegistrationTemplateRowDTO): IEmployeeSmallDTO {
        if (!row.employeeNr)
            return null;

        return this.employees.find(e => e.employeeNr === row.employeeNr);
    }

    private allowNavigationFromEmployee(value): boolean {
        return (!value || this.employees.filter(p => p.employeeNr === value).length > 0);
    }

    private filterPayrollProducts(filter) {
        return _.orderBy(this.payrollProducts.filter(p => {
            return p.numberName.startsWithCaseInsensitive(filter) || p.numberName.contains(filter);
        }), 'numberName');
    }

    private findPayrollProduct(row: MassRegistrationTemplateRowDTO): ProductSmallDTO {
        if (!row.productNr)
            return null;

        return this.payrollProducts.find(p => p.number === row.productNr);
    }

    private allowNavigationFromPayrollProduct(value): boolean {
        return (!value || this.payrollProducts.filter(p => p.number === value).length > 0);
    }

    private filterAccounts(dimIndex, filter) {
        return _.orderBy(this.accountDims[dimIndex].accounts.filter(acc => {
            if (parseInt(filter))
                return acc.accountNr.startsWithCaseInsensitive(filter);

            return acc.accountNr.startsWithCaseInsensitive(filter) || acc.name.contains(filter);
        }), 'accountNr');
    }

    private findAccount(row: MassRegistrationTemplateRowDTO, dimIndex: number) {
        var nrToFind = row[`dim${dimIndex}Nr`];
        if (!nrToFind)
            return null;

        return this.accountDims[dimIndex - 1].accounts.find(a => a.accountNr === nrToFind);
    }

    private allowNavigationFromAccount(value, dimIndex): boolean {
        return (!value || this.accountDims[dimIndex - 1].accounts.filter(acc => acc.accountNr === value).length > 0);
    }

    private handleNavigateToNextCell(params: any): { rowIndex: number, column: any } {
        const { nextCellPosition, previousCellPosition, backwards } = params;
        const nextColumnCaller: (column: any) => any = backwards ? this.gridHandler.gridAg.options.getPreviousVisibleColumn : this.gridHandler.gridAg.options.getNextVisibleColumn;

        let { rowIndex, column } = nextCellPosition;
        let row: MassRegistrationTemplateRowDTO = this.gridHandler.gridAg.options.getVisibleRowByIndex(rowIndex).data;

        while (!!column && !!this.steppingRules) {
            const { colDef } = column;
            if (this.gridHandler.gridAg.options.isCellEditable(row, colDef)) {
                const steppingRule = this.steppingRules[colDef.field];
                const stop = !!steppingRule ? steppingRule.call(this, row) : false;

                if (stop) {
                    return { rowIndex, column };
                }
            }

            column = nextColumnCaller(column);
        }

        column = previousCellPosition.column;

        const nextRowResult = backwards ? this.findPreviousRow(row) : this.findNextRow(row);
        const newRowIndex = nextRowResult ? nextRowResult.rowIndex : this.addRow(true).rowIndex;

        return { rowIndex: newRowIndex, column: backwards ? this.getLastColumn() : this.getFirstColumn() };
    }

    private getFirstColumn() {
        return this.gridHandler.gridAg.options.getColumnByField('employeeNr');
    }

    private getLastColumn() {
        return this.gridHandler.gridAg.options.getColumnByField(`dim${this.accountDims.length}Nr`);
    }

    private findNextRow(row): { rowIndex: number, rowNode: any } {
        const result = this.gridHandler.gridAg.options.getNextRow(row);

        return !!result.rowNode ? result : null;
    }

    private findPreviousRow(row): { rowIndex: number, rowNode: any } {
        const result = this.gridHandler.gridAg.options.getPreviousRow(row);

        return !!result.rowNode ? result : null;
    }

    // VALIDATION

    private validateSave(): boolean {
        return true;
    }

    public showValidationError() {
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
            if (this.head) {
                var errors = this['edit'].$error;

                if (!this.head.name)
                    mandatoryFieldKeys.push("common.name");

                if (this.head.rows && this.head.rows.length > 0) {
                    if (errors['rowEmployee'])
                        validationErrorKeys.push("time.payroll.massregistration.validation.rows.missingemployee");
                    if (errors['rowProduct'])
                        validationErrorKeys.push("time.payroll.massregistration.validation.rows.missingproduct");
                    if (errors['rowPaymentDate'])
                        validationErrorKeys.push("time.payroll.massregistration.validation.rows.missingpaymentdate");
                    if (errors['rowDateInterval'])
                        validationErrorKeys.push("time.payroll.massregistration.validation.rows.missingdateinterval");
                }
            }
        });
    }
}