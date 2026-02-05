import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/controllerflowhandlerfactory";
import { IDirtyHandlerFactory } from "../../../Core/Handlers/DirtyHandlerFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/messaginghandlerfactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IValidationSummaryHandlerFactory } from "../../../Core/Handlers/validationsummaryhandlerfactory";
import { ICompositionEditController } from "../../../Core/ICompositionEditController";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { Feature } from "../../../Util/CommonEnumerations";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { IPayrollService } from "../PayrollService";
import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2";
import { PayrollImportEmployeeDTO } from "../../../Common/Models/PayrollImport";
import { EmbeddedGridController } from "../../../Core/Controllers/EmbeddedGridController";
import { IGridHandlerFactory } from "../../../Core/Handlers/gridhandlerfactory";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { EditEmployeeDialogController } from "./Dialogs/EditEmployee/EditEmployeeDialogController";
import { GridEvent } from "../../../Util/SoeGridOptions";
import { IconLibrary, SoeGridOptionsEvent, SOEMessageBoxButtons, SOEMessageBoxImage } from "../../../Util/Enumerations";
import { ToolBarButton, ToolBarUtility } from "../../../Util/ToolBarUtility";
import { Constants } from "../../../Util/Constants";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { IColumnAggregate, IColumnAggregations } from "../../../Util/SoeGridOptionsAg";
import { CalendarUtility } from "../../../Util/CalendarUtility";
import { NumberUtility } from "../../../Util/NumberUtility";
import { StringUtility } from "../../../Util/StringUtility";

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    // Terms:
    private terms: { [index: string]: string; };

    // Data
    private payrollImportHeadId: number;
    private payrollImportEmployees: PayrollImportEmployeeDTO[];

    // Grid
    private gridHandler: EmbeddedGridController;
    private nbrOfSelectedRows: number = 0;

    //@ngInject
    constructor(
        private $scope: ng.IScope,
        private $uibModal,
        private $timeout: ng.ITimeoutService,
        private $q: ng.IQService,
        private payrollService: IPayrollService,
        private translationService: ITranslationService,
        private urlHelperService: IUrlHelperService,
        private notificationService: INotificationService,
        progressHandlerFactory: IProgressHandlerFactory,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        private dirtyHandlerFactory: IDirtyHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory) {
        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onDoLookUp(() => this.onDoLookups())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));

        this.gridHandler = new EmbeddedGridController(gridHandlerFactory, "Time.Payroll.PayrollImports.Edit");
    }

    // SETUP

    public onInit(parameters: any) {
        this.payrollImportHeadId = parameters.id;
        this.guid = parameters.guid;
        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);

        this.flowHandler.start([
            { feature: Feature.Time_Import_PayrollImport, loadReadPermissions: true, loadModifyPermissions: true },
        ]);
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultEditToolbar(false);

        this.toolbar.addButtonGroup(ToolBarUtility.createGroup(new ToolBarButton("core.reload_data", "core.reload_data", IconLibrary.FontAwesome, "fa-sync", () => this.loadData())));
        this.toolbar.addButtonGroup(ToolBarUtility.createGroup(new ToolBarButton("time.payroll.payrollimport.rollback.file.employees", "time.payroll.payrollimport.rollback.file.employees", IconLibrary.FontAwesome, "fa-times", () => this.initRollbackFile(), () => this.nbrOfSelectedRows === 0)));
        this.toolbar.addButtonGroup(ToolBarUtility.createGroup(new ToolBarButton("time.payroll.payrollimport.rollback.employees", "time.payroll.payrollimport.rollback.employees", IconLibrary.FontAwesome, "fa-undo", () => this.initRollback(), () => this.nbrOfSelectedRows === 0)));
        this.toolbar.addButtonGroup(ToolBarUtility.createGroup(new ToolBarButton("time.payroll.payrollimport.execute", "time.payroll.payrollimport.execute", IconLibrary.FontAwesome, "fa-download", () => this.initExecute(), () => this.nbrOfSelectedRows === 0)));
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Time_Import_PayrollImport].readPermission;
        this.modifyPermission = response[Feature.Time_Import_PayrollImport].modifyPermission;
    }

    private setupGrid() {
        this.gridHandler.gridAg.options.enableGridMenu = false;
        this.gridHandler.gridAg.options.enableFiltering = true;
        this.gridHandler.gridAg.options.enableRowSelection = true;
        this.gridHandler.gridAg.options.setMinRowsToShow(20);

        const isDeletedFunc = (data: PayrollImportEmployeeDTO) => data && data.isDeleted;
        const timeSpanColumnAggregate = {
            getSeed: () => 0,
            accumulator: (acc, next) => CalendarUtility.sumTimeSpan(acc, next, false),
            cellRenderer: this.timeSpanAggregateRenderer.bind(this)
        } as IColumnAggregate;
        const amountColumnAggregate = {
            getSeed: () => 0,
            accumulator: (acc, next) => acc + next,
            cellRenderer: this.amountAggregateRenderer.bind(this)
        } as IColumnAggregate;

        this.gridHandler.gridAg.addColumnText("employeeInfo", this.terms["common.employee"], 80, false, { strikeThrough: isDeletedFunc });

        let colHeaderSchedule = this.gridHandler.gridAg.options.addColumnHeader("schedule", this.terms["time.payroll.payrollimport.employee.schedule"]);
        colHeaderSchedule.marryChildren = true;
        this.gridHandler.gridAg.addColumnNumber("scheduleRowCount", this.terms["time.payroll.payrollimport.employee.rows"], 50, { strikeThrough: isDeletedFunc }, colHeaderSchedule);
        this.gridHandler.gridAg.addColumnTimeSpan("scheduleQuantity", this.terms["time.payroll.payrollimport.employee.schedule.length"], 50, { strikeThrough: isDeletedFunc, hideDays: true }, colHeaderSchedule);
        this.gridHandler.gridAg.addColumnTimeSpan("scheduleBreakQuantity", this.terms["time.payroll.payrollimport.employee.schedule.break"], 50, { strikeThrough: isDeletedFunc, hideDays: true }, colHeaderSchedule);

        let colHeaderTransactions = this.gridHandler.gridAg.options.addColumnHeader("transactions", this.terms["time.payroll.payrollimport.employee.transactions"]);
        colHeaderTransactions.marryChildren = true;
        this.gridHandler.gridAg.addColumnNumber("transactionRowCount", this.terms["time.payroll.payrollimport.employee.rows"], 50, { strikeThrough: isDeletedFunc }, colHeaderTransactions);
        this.gridHandler.gridAg.addColumnNumber("transactionQuantity", this.terms["common.quantity"], 50, { strikeThrough: isDeletedFunc }, colHeaderTransactions);
        this.gridHandler.gridAg.addColumnNumber("transactionAmount", this.terms["common.amount"], 50, { strikeThrough: isDeletedFunc, decimals: 2 }, colHeaderTransactions);

        this.gridHandler.gridAg.addColumnIcon("statusIcon", null, null, { toolTipField: "statusName" });
        this.gridHandler.gridAg.addColumnText("statusName", this.terms["common.status"], 75, true, { strikeThrough: isDeletedFunc });

        this.gridHandler.gridAg.addColumnEdit(this.terms["core.edit"], this.editEmployee.bind(this));

        // Events
        const events: GridEvent[] = [];
        events.push(new GridEvent(SoeGridOptionsEvent.RowDoubleClicked, (row) => { this.editEmployee(row); }));
        events.push(new GridEvent(SoeGridOptionsEvent.RowSelectionChanged, (row: uiGrid.IGridRow) => { this.gridSelectionChanged(); }));
        events.push(new GridEvent(SoeGridOptionsEvent.RowSelectionChangedBatch, (row: uiGrid.IGridRow) => { this.gridSelectionChanged(); }));
        this.gridHandler.gridAg.options.subscribe(events);

        this.gridHandler.gridAg.finalizeInitGrid("time.payroll.payrollimport.imports", true);

        this.$timeout(() => {
            this.gridHandler.gridAg.options.addFooterRow("#totals-grid", {
                "scheduleRowCount": "sum",
                "scheduleQuantity": timeSpanColumnAggregate,
                "scheduleBreakQuantity": timeSpanColumnAggregate,
                "transactionRowCount": "sum",
                "transactionQuantity": "sum",
                "transactionAmount": amountColumnAggregate
            } as IColumnAggregations);
        });
    }

    private timeSpanAggregateRenderer({ data, colDef }) {
        return data[colDef.field] || "0:00";
    }

    private amountAggregateRenderer({ data, colDef }) {
        return parseFloat(data[colDef.field] || "0").toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 });
    }

    // SERVICE CALLS

    private onDoLookups(): ng.IPromise<any> {
        return this.$q.all([
            this.loadTerms()
        ]).then(() => {
            this.setupGrid();
            this.loadData();
        });
    }

    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
            "core.edit",
            "common.amount",
            "common.employee",
            "common.quantity",
            "common.status",
            "time.payroll.payrollimport.employee.rows",
            "time.payroll.payrollimport.employee.schedule",
            "time.payroll.payrollimport.employee.schedule.break",
            "time.payroll.payrollimport.employee.schedule.length",
            "time.payroll.payrollimport.employee.transactions",
            "time.payroll.payrollimport.rollback.employees",
            "time.payroll.payrollimport.rollback.employees.validate",
            "time.payroll.payrollimport.rollback.file.employees",
            "time.payroll.payrollimport.rollback.file.employees.validate"
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
        });
    }

    private loadData(): ng.IPromise<any> {
        return this.progress.startLoadingProgress([() => {
            return this.payrollService.getPayrollImportEmployees(this.payrollImportHeadId, true, true, false, true).then(x => {
                this.payrollImportEmployees = x;
                this.nbrOfSelectedRows = 0;
                this.gridHandler.gridAg.setData(this.payrollImportEmployees);
            });
        }]);
    }

    // EVENTS

    private gridSelectionChanged() {
        this.$scope.$applyAsync(() => {
            this.nbrOfSelectedRows = this.gridHandler.gridAg.options.getSelectedCount();
        });
    }

    // ACTIONS

    private initExecute() {
        this.progress.startWorkProgress((completion) => {
            this.payrollService.validatePayrollImport(this.payrollImportHeadId, this.getSelectedEmployeeIds()).then(result => {
                completion.completed(null, true);
                if (result.success) {
                    this.execute();
                } else {
                    var image: SOEMessageBoxImage = result.canUserOverride ? SOEMessageBoxImage.Question : SOEMessageBoxImage.Forbidden;
                    var buttons: SOEMessageBoxButtons = result.canUserOverride ? SOEMessageBoxButtons.YesNo : SOEMessageBoxButtons.OK;
                    var modal = this.notificationService.showDialog(this.terms["core.warning"], result.errorMessage, image, buttons);
                    modal.result.then(val => {
                        if (result.canUserOverride && val != null && val === true) {
                            this.execute();
                        }
                    });
                }
            }, error => {
                completion.failed(error.message);
            });
        });
    }

    private execute() {
        this.progress.startSaveProgress((completion) => {
            this.payrollService.payrollImportExecute(this.payrollImportHeadId, this.getSelectedEmployeeIds()).then(result => {
                if (result.success) {
                    completion.completed(Constants.EVENT_EDIT_SAVED);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }, this.guid)
            .then(data => {
                this.dirtyHandler.clean();
                this.loadData();
            });
    }

    private initRollbackFile() {
        this.progress.startDeleteProgress((completion) => {
            this.payrollService.payrollImportExecuteRollbackFile(this.payrollImportHeadId, this.getSelectedEmployeeIds(), false).then(result => {
                if (result.success) {
                    completion.completed(Constants.EVENT_EDIT_DELETED, true);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }, () => this.loadData(), this.terms["time.payroll.payrollimport.rollback.file.employees.validate"]);
    }

    private initRollback() {
        this.progress.startDeleteProgress((completion) => {
            this.payrollService.payrollImportExecuteRollback(this.payrollImportHeadId, this.getSelectedEmployeeIds(), false).then(result => {
                if (result.success) {
                    completion.completed(Constants.EVENT_EDIT_DELETED, true);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }, () => this.loadData(), this.terms["time.payroll.payrollimport.rollback.employees.validate"]);
    }

    private editEmployee(employee: PayrollImportEmployeeDTO) {
        let options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Payroll/PayrollImports/Dialogs/EditEmployee/editEmployeeDialog.html"),
            controller: EditEmployeeDialogController,
            controllerAs: "ctrl",
            bindToController: true,
            backdrop: 'static',
            size: 'xl',
            windowClass: 'fullsize-modal',
            resolve: {
                employee: () => { return employee }
            }
        }

        this.$uibModal.open(options).result.then(result => {

        }, (reason) => {
            // User cancelled dialog
            if (reason === 'reload')
                this.loadData();
        });
    }

    // HELP-METHODS

    private setAsDirty(dirty = true) {
        this.dirtyHandler.isDirty = dirty;
    }

    private getSelectedEmployeeIds(): number[] {
        return this.gridHandler.gridAg.options.getSelectedIds("payrollImportEmployeeId");
    }
}