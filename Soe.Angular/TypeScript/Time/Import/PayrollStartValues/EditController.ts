import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { Feature, SoeEntityState } from "../../../Util/CommonEnumerations";
import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2";
import { ICompositionEditController } from "../../../Core/ICompositionEditController";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/controllerflowhandlerfactory";
import { IValidationSummaryHandlerFactory } from "../../../Core/Handlers/validationsummaryhandlerfactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/messaginghandlerfactory";
import { IDirtyHandlerFactory } from "../../../Core/Handlers/DirtyHandlerFactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IPayrollService } from "../../Payroll/PayrollService";
import { PayrollStartValueHeadDTO, PayrollStartValueRowDTO } from "../../../Common/Models/PayrollImport";
import { ImportFileDialogController } from "./Directives/ImportFileDialog/ImportFileDialogController";
import { ToolBarButton, ToolBarUtility } from "../../../Util/ToolBarUtility";
import { IconLibrary, SoeGridOptionsEvent, SOEMessageBoxButtons, SOEMessageBoxImage } from "../../../Util/Enumerations";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { ModalUtility } from "../../../Util/ModalUtility";
import { EmbeddedGridController } from "../../../Core/Controllers/EmbeddedGridController";
import { IGridHandlerFactory } from "../../../Core/Handlers/gridhandlerfactory";
import { IToolbar } from "../../../Core/Handlers/Toolbar";
import { EditRowDialogController } from "./Directives/EditRowDialog/EditRowDialogController";
import { IActionResult, ISmallGenericType } from "../../../Scripts/TypeLite.Net4";
import { ProductSmallDTO } from "../../../Common/Models/ProductDTOs";
import { SmallGenericType } from "../../../Common/Models/SmallGenericType";
import { GridEvent } from "../../../Util/SoeGridOptions";
import { CoreUtility } from "../../../Util/CoreUtility";

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    // Terms
    private terms: { [index: string]: string; };
    private minutesLabel: string;

    // Data
    private payrollStartValueHead: PayrollStartValueHeadDTO;
    private payrollStartValueHeadId: number;
    private employees: ISmallGenericType[] = [];
    private payrollProducts: ProductSmallDTO[] = [];

    // Grid
    private gridToolbar: IToolbar;
    private gridHandler: EmbeddedGridController;
    private tmpIdCounter: number = 0;

    //@ngInject
    constructor(
        private $q: ng.IQService,
        private $uibModal,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        private urlHelperService: IUrlHelperService,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        private payrollService: IPayrollService,
        progressHandlerFactory: IProgressHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        private dirtyHandlerFactory: IDirtyHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory) {

        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);

        this.gridHandler = new EmbeddedGridController(gridHandlerFactory, "time.import.payrollstartvalue.rows");

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onDoLookUp(() => this.onDoLookUp())
            .onLoadData(() => this.onLoadData())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));
    }

    // SETUP

    public onInit(parameters: any) {
        this.payrollStartValueHeadId = parameters.id;
        this.guid = parameters.guid;

        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);
        this.flowHandler.start([{ feature: Feature.Time_Import_PayrollStartValuesImported, loadReadPermissions: true, loadModifyPermissions: true }]);
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        // Page toolbar
        this.toolbar = toolbarFactory.createDefaultEditToolbar(false);

        this.toolbar.addButtonGroup(ToolBarUtility.createGroup(new ToolBarButton("time.import.payrollstartvalue.deletetransactionsforall", "time.import.payrollstartvalue.deletetransactionsforall", IconLibrary.FontAwesome, "fa-user-times",
            () => { this.deleteTransactionsForImport(); },
            () => { return this.isNew; }
        )));

        this.toolbar.addButtonGroup(ToolBarUtility.createGroup(new ToolBarButton("time.import.payrollstartvalue.savetransactionsforall", "time.import.payrollstartvalue.savetransactionsforall", IconLibrary.FontAwesome, "fa-user-plus",
            () => { this.saveTransactionsForImport(); },
            () => { return this.isNew; }
        )));

        this.toolbar.addButtonGroup(ToolBarUtility.createGroup(new ToolBarButton("time.import.payrollstartvalue.import", "time.import.payrollstartvalue.import", IconLibrary.FontAwesome, "fa-file-import",
            () => { this.openImportFileDialog(); },
            () => { return this.isNew; }
        )));

        // Grid toolbar
        this.gridToolbar = toolbarFactory.createEmpty();
        this.gridToolbar.addButtonGroup(ToolBarUtility.createGroup(new ToolBarButton("common.newrow", "common.newrow", IconLibrary.FontAwesome, "fa-plus",
            () => { this.addRow(); },
            () => { return this.isNew; }
        )));
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Time_Import_PayrollStartValuesImported].readPermission;
        this.modifyPermission = response[Feature.Time_Import_PayrollStartValuesImported].modifyPermission;

        if (this.modifyPermission)
            this.messagingHandler.publishActivateAddTab();
    }

    private setupGrid() {
        this.gridHandler.gridAg.options.enableRowSelection = false;
        this.gridHandler.gridAg.options.enableGridMenu = false;
        this.gridHandler.gridAg.options.enableContextMenu = false;
        this.gridHandler.gridAg.options.enableFiltering = false;
        this.gridHandler.gridAg.options.showAlignedFooterGrid = false;
        this.gridHandler.gridAg.options.setMinRowsToShow(10);

        this.gridHandler.gridAg.addColumnIsModified();

        let colHeaderStartValue = this.gridHandler.gridAg.options.addColumnHeader("startValueRow", this.terms["time.import.payrollstartvalue.payrollstartvalue"]);
        colHeaderStartValue.marryChildren = true;

        let colDefEmployee = this.gridHandler.gridAg.addColumnText("employeeName", this.terms["common.employee"], null, false, null, colHeaderStartValue);
        this.gridHandler.gridAg.addColumnText("appellation", this.terms["common.appellation"], null, true, null, colHeaderStartValue);
        this.gridHandler.gridAg.addColumnText("productNrAndName", this.terms["time.payrollproduct.payrollproduct"], null, true, null, colHeaderStartValue);
        this.gridHandler.gridAg.addColumnDate("date", this.terms["common.date"], 110, true, null, null, colHeaderStartValue);
        this.gridHandler.gridAg.addColumnNumber("quantity", this.terms["common.quantity"], 100, null, colHeaderStartValue);
        this.gridHandler.gridAg.addColumnNumber("amount", this.terms["common.amount"], 100, { decimals: 2 }, colHeaderStartValue);
        this.gridHandler.gridAg.addColumnNumber("scheduleTimeMinutes", this.terms["time.import.payrollstartvalue.scheduletime"], 100, null, colHeaderStartValue);
        this.gridHandler.gridAg.addColumnNumber("absenceTimeMinutes", this.terms["time.import.payrollstartvalue.absencetime"], 100, null, colHeaderStartValue);

        let colHeaderTrans = this.gridHandler.gridAg.options.addColumnHeader("transaction", this.terms["common.transaction"]);
        colHeaderTrans.marryChildren = true;

        this.gridHandler.gridAg.addColumnNumber("timePayrollTransactionId", this.terms["common.id"], 100, { clearZero: true }, colHeaderTrans);
        this.gridHandler.gridAg.addColumnText("transactionProductNrAndName", this.terms["time.payrollproduct.payrollproduct"], null, true, null, colHeaderTrans);
        this.gridHandler.gridAg.addColumnDate("transactionDate", this.terms["common.date"], 110, true, null, null, colHeaderTrans);
        this.gridHandler.gridAg.addColumnNumber("transactionQuantity", this.terms["common.quantity"], 100, { clearZero: true }, colHeaderTrans);
        this.gridHandler.gridAg.addColumnNumber("transactionAmount", this.terms["common.amount"], 100, { decimals: 2, clearZero: true }, colHeaderTrans);
        this.gridHandler.gridAg.addColumnNumber("transactionUnitPrice", this.terms["common.unitprice"], 100, { decimals: 2, clearZero: true }, colHeaderTrans);
        this.gridHandler.gridAg.addColumnText("transactionComment", this.terms["common.comment"], null, true, null, colHeaderTrans);

        if (this.modifyPermission) {
            this.gridHandler.gridAg.addColumnIcon("saveTrans", null, null, { toolTip: this.terms["time.import.payrollstartvalue.savetransactions"], icon: "fal fa-plus-square", onClick: this.saveTransRow.bind(this), getNodeOnClick: true, showIcon: this.showSaveTransIcon.bind(this) }, colHeaderTrans);
            this.gridHandler.gridAg.addColumnIcon("deleteTrans", null, null, { toolTip: this.terms["time.import.payrollstartvalue.deletetransactions"], icon: 'fal fa-times-square iconDelete', onClick: this.deleteTransRow.bind(this), getNodeOnClick: true, showIcon: this.showDeleteTransIcon.bind(this) }, colHeaderTrans);
            this.gridHandler.gridAg.addColumnIcon("delete", null, null, { pinned: 'right', toolTip: this.terms["core.deleterow"], icon: 'fal fa-times iconDelete', onClick: this.deleteRow.bind(this), getNodeOnClick: true });
            this.gridHandler.gridAg.addColumnIcon("edit", null, null, { pinned: 'right', toolTip: this.terms["core.edit"], icon: 'fal fa-pencil iconEdit', onClick: this.editRow.bind(this), getNodeOnClick: true, showIcon: this.showEditIcon.bind(this) });

            let events: GridEvent[] = [];
            events.push(new GridEvent(SoeGridOptionsEvent.RowDoubleClicked, (row) => {
                if (row)
                    this.openEditRowDialog(row);
            }));
            this.gridHandler.gridAg.options.subscribe(events);
        }        
        // Mark transaction cells if no transaction is created
        _.forEach(this.gridHandler.gridAg.options.getColumnDefs(), colDef => {
            if (colDef.field === "timePayrollTransactionId" || colDef.field.startsWith("transaction") || colDef.field === "saveTrans" || colDef.field === "deleteTrans") {
                let cellCls: string = colDef.cellClass ? colDef.cellClass.toString() : "";
                colDef.cellClass = (grid: any) => {                    
                    if (grid.data) {
                        let data: PayrollStartValueRowDTO = grid.data;
                        if (!data.timePayrollTransactionId)
                            cellCls += " errorRowFaded";
                    }

                    return cellCls;
                };
            }
        });

        this.gridHandler.gridAg.options.useGrouping(false, false, { hideGroupPanel: true, keepGroupState: true });
        this.gridHandler.gridAg.options.groupRowsByColumn(colDefEmployee, true);

        this.gridHandler.gridAg.finalizeInitGrid("time.import.payrollstartvalue.payrollstartvalues", false);
    }

    // LOOKUPS

    private onDoLookUp(): ng.IPromise<any> {
        return this.$q.all([
            this.loadTerms(),
            this.loadEmployees(),
            this.loadPayrollProducts()
        ]).then(() => {
            this.setupGrid();
        });
    }

    private loadTerms(): ng.IPromise<any> {
        let keys: string[] = [
            "common.appellation",
            "common.amount",
            "common.comment",
            "common.date",
            "common.employee",
            "common.newrow",
            "common.quantity",
            "common.id",
            "common.unitprice",
            "common.transaction",
            "core.deleterow",
            "core.edit",
            "core.time.minutes",
            "time.import.payrollstartvalue.absencetime",
            "time.import.payrollstartvalue.delete",
            "time.import.payrollstartvalue.delete.message",
            "time.import.payrollstartvalue.deleteemployee",
            "time.import.payrollstartvalue.deleterow",
            "time.import.payrollstartvalue.deletetransactions",
            "time.import.payrollstartvalue.deletetransactions.message",
            "time.import.payrollstartvalue.deletetransactionsforall",
            "time.import.payrollstartvalue.deletetransactionsforall.message",
            "time.import.payrollstartvalue.import",
            "time.import.payrollstartvalue.payrollstartvalue",
            "time.import.payrollstartvalue.savetransactions",
            "time.import.payrollstartvalue.savetransactions.message",
            "time.import.payrollstartvalue.savetransactionsforall",
            "time.import.payrollstartvalue.savetransactionsforall.message",
            "time.import.payrollstartvalue.scheduletime",
            "time.import.payrollstartvalue.validatedirty.title",
            "time.import.payrollstartvalue.validatedirty.message",
            "time.payrollproduct.payrollproduct"
        ];
        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
            this.minutesLabel = this.terms["core.time.minutes"].toLocaleLowerCase();
        });
    }

    private loadEmployees(): ng.IPromise<any> {
        return this.payrollService.getEmployeesDict(false, true, false, false).then(x => {
            this.employees = x;
        });
    }

    private loadPayrollProducts() {
        this.payrollService.getPayrollProductsSmall(false).then(x => {
            this.payrollProducts = x;
        });
    }

    private onLoadData() {
        if (this.payrollStartValueHeadId > 0) {
            return this.progress.startLoadingProgress([
                () => this.load()
            ]);
        } else {
            this.new();
        }
    }

    private load(): ng.IPromise<any> {
        return this.payrollService.getPayrollStartValueHead(this.payrollStartValueHeadId, true, true, true).then(x => {
            this.isNew = false;
            this.payrollStartValueHead = x;
            _.forEach(this.payrollStartValueHead.rows, row => {
                row['employeeName'] = this.employees.find(e => e.id === row.employeeId)?.name;
            });
            this.dirtyHandler.clean();
            this.messagingHandler.publishSetTabLabel(this.guid, this.terms["time.import.payrollstartvalue.payrollstartvalue"] + ' ' + this.payrollStartValueHead.importedFrom);
            this.setGridData();
        });
    }

    private loadRows(employeeId: number): ng.IPromise<any> {
        return this.payrollService.getPayrollStartValueRows(this.payrollStartValueHeadId, employeeId, true, true, true).then(x => {
            _.forEach(x, row => {
                row['employeeName'] = this.employees.find(e => e.id === row.employeeId)?.name;
            });
            // Remove existing rows for employee
            this.payrollStartValueHead.rows = this.payrollStartValueHead.rows.filter(r => r.employeeId !== employeeId);
            // Add loaded rows
            this.payrollStartValueHead.rows.push(...x);
            this.setGridData();
        });
    }

    // ACTIONS

    private addRow() {
        this.openEditRowDialog(null);
    }

    private editRow(node) {
        this.openEditRowDialog(node.data);
    }

    private saveTransRow(node) {
        if (node && node.group && node.key && node.field === 'employeeName') {
            let employee = this.employees.find(e => e.name === node.key);
            if (employee)
                this.saveTransactionsForEmployee(employee);
        }
    }

    private saveTransactionsForEmployee(employee: SmallGenericType) {
        if (!this.validateNotDirtyBeforeAction())
            return;

        this.notificationService.showDialog(this.terms["time.import.payrollstartvalue.savetransactions"], this.terms["time.import.payrollstartvalue.savetransactions.message"].format(employee.name), SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel).result.then(val => {
            if (val) {
                this.progress.startSaveProgress((completion) => {
                    this.payrollService.saveTransactionsForPayrollStartValue(this.payrollStartValueHead.payrollStartValueHeadId, employee.id).then((result) => {
                        if (result.success) {
                            completion.completed(null, null, true);
                        } else {
                            completion.failed(result.errorMessage);
                        }
                    }, error => {
                        completion.failed(error.message);
                    });
                }, this.guid).then(data => {
                    this.dirtyHandler.clean();
                    this.loadRows(employee.id);
                });
            }
        });
    }

    private deleteTransRow(node) {
        if (node && node.group && node.key && node.field === 'employeeName') {
            let employee = this.employees.find(e => e.name === node.key);
            if (employee)
                this.deleteTransactionsForEmployee(employee);
        }
    }

    private deleteTransactionsForEmployee(employee: SmallGenericType) {
        if (!this.validateNotDirtyBeforeAction())
            return;

        this.notificationService.showDialog(this.terms["time.import.payrollstartvalue.deletetransactions"], this.terms["time.import.payrollstartvalue.deletetransactions.message"].format(employee.name), SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel).result.then(val => {
            if (val) {
                this.progress.startDeleteProgress((completion) => {
                    this.payrollService.deleteTransactionsForPayrollStartValue(this.payrollStartValueHead.payrollStartValueHeadId, employee.id).then((result) => {
                        if (result.success) {
                            completion.completed(null, true);
                        } else {
                            completion.failed(result.errorMessage);
                        }
                    }, error => {
                        completion.failed(error.message);
                    });
                }, null, ModalUtility.MODAL_SKIP_CONFIRM).then(x => {
                    this.loadRows(employee.id);
                });
            }
        });
    }

    private deleteRow(node) {
        if (node && node.group && node.key && node.field === 'employeeName') {
            let employee = this.employees.find(e => e.name === node.key);
            if (employee)
                this.deleteAllEmployeeRows(employee);
        } else if (node.data) {
            this.deleteEmployeeRow(node.data);
        }
    }

    private deleteAllEmployeeRows(employee: ISmallGenericType) {
        this.notificationService.showDialog(this.terms["core.deleterow"], this.terms["time.import.payrollstartvalue.deleteemployee"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel).result.then(val => {
            if (val) {
                this.payrollStartValueHead.rows.filter(r => r.employeeId === employee.id).forEach(row => {
                    row.state = SoeEntityState.Deleted;
                    row.isModified = true;
                });
                this.dirtyHandler.setDirty();
                this.setGridData();
            }
        });
    }

    private deleteEmployeeRow(row: PayrollStartValueRowDTO) {
        this.notificationService.showDialog(this.terms["core.deleterow"], this.terms["time.import.payrollstartvalue.deleterow"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel).result.then(val => {
            if (val) {
                row.state = SoeEntityState.Deleted;
                row.isModified = true;
                this.dirtyHandler.setDirty();
                this.setGridData();
            }
        });
    }

    private saveTransactionsForImport() {
        if (!this.validateNotDirtyBeforeAction())
            return;

        this.notificationService.showDialog(this.terms["time.import.payrollstartvalue.savetransactionsforall"], this.terms["time.import.payrollstartvalue.savetransactionsforall.message"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel).result.then(val => {
            if (val) {
                this.progress.startSaveProgress((completion) => {
                    this.payrollService.saveTransactionsForPayrollStartValue(this.payrollStartValueHeadId, 0).then(result => {
                        if (result.success) {
                            completion.completed(null, this.payrollStartValueHead, true);
                        } else {
                            completion.failed(result.errorMessage);
                        }
                    }, error => {
                        completion.failed(error.message);
                    });
                }, this.guid).then(data => {
                    this.onLoadData();
                });
            }
        });
    }

    private deleteTransactionsForImport() {
        if (!this.validateNotDirtyBeforeAction())
            return;

        this.notificationService.showDialog(this.terms["time.import.payrollstartvalue.deletetransactionsforall"], this.terms["time.import.payrollstartvalue.deletetransactionsforall.message"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel).result.then(val => {
            if (val) {
                this.progress.startDeleteProgress((completion) => {
                    this.payrollService.deleteTransactionsForPayrollStartValue(this.payrollStartValueHeadId, 0).then(result => {
                        if (result.success) {
                            completion.completed(this.payrollStartValueHead, true);
                        } else {
                            completion.failed(result.errorMessage);
                        }
                    }, error => {
                        completion.failed(error.message);
                    });
                }, null, ModalUtility.MODAL_SKIP_CONFIRM).then(x => {
                    this.onLoadData();
                });
            }
        });
    }

    private save() {
        this.progress.startSaveProgress((completion) => {
            this.payrollService.savePayrollStartValues(this.payrollStartValueHead.rows.filter(r => r.isModified), this.payrollStartValueHeadId).then(result => {
                if (result.success) {
                    completion.completed(null, this.payrollStartValueHead, true);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }, this.guid).then(data => {
            this.onLoadData();
        });
    }

    private delete() {
        this.notificationService.showDialog(this.terms["time.import.payrollstartvalue.delete"], this.terms["time.import.payrollstartvalue.delete.message"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel).result.then(val => {
            if (val) {
                this.progress.startDeleteProgress((completion) => {
                    this.payrollService.deletePayrollStartValueHead(this.payrollStartValueHeadId).then(result => {
                        if (result.success) {
                            completion.completed(this.payrollStartValueHead, true);
                        } else {
                            completion.failed(result.errorMessage);
                        }
                    }, error => {
                        completion.failed(error.message);
                    });
                }, null, ModalUtility.MODAL_SKIP_CONFIRM).then(x => {
                    this.closeMe(false);
                });
            }
        });
    }

    // HELP-METHODS

    private showSaveTransIcon(node) {
        return (node && node.group && node.key && node.field === 'employeeName');
    }

    private showDeleteTransIcon(node) {
        return (node && node.group && node.key && node.field === 'employeeName');
    }

    private showEditIcon(node) {
        return (node && !node.group && node.data);
    }

    private new() {
        this.isNew = true;
        this.payrollStartValueHeadId = 0;
        this.payrollStartValueHead = new PayrollStartValueHeadDTO();

        this.openImportFileDialog();
    }

    private openImportFileDialog() {
        if (!this.validateNotDirtyBeforeAction())
            return;

        let options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Import/PayrollStartValues/Directives/ImportFileDialog/ImportFileDialog.html"),
            controller: ImportFileDialogController,
            controllerAs: "ctrl",
            size: 'md',
            resolve: {
                payrollStartValueHead: () => { return this.payrollStartValueHead },
                isNew: () => { return this.isNew }
            }
        }

        this.$uibModal.open(options).result.then((result: any) => {
            if (result && result.result) {
                const actionResult: IActionResult = result.result;
                if (actionResult.success) {
                    if (actionResult.integerValue) {
                        this.payrollStartValueHeadId = actionResult.integerValue;
                        this.onLoadData();
                    }
                } else {
                    this.notificationService.showErrorDialog("", actionResult.errorMessage, "");
                }
            } else if (this.isNew) {
                this.closeMe(false);
            }
        });
    }

    private openEditRowDialog(row: PayrollStartValueRowDTO) {
        let isNewRow: boolean = false;

        if (!row) {
            isNewRow = true;
            row = new PayrollStartValueRowDTO();
            row['tmpId'] = ++this.tmpIdCounter;
        }

        let options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Import/PayrollStartValues/Directives/EditRowDialog/EditRowDialog.html"),
            controller: EditRowDialogController,
            controllerAs: "ctrl",
            size: 'md',
            resolve: {
                minutesLabel: () => { return this.minutesLabel },
                employees: () => { return this.employees },
                payrollProducts: () => { return this.payrollProducts },
                isNew: () => { return isNewRow },
                row: () => { return row }
            }
        }

        this.$uibModal.open(options).result.then((result: any) => {
            if (result && result.row) {
                let editRow: PayrollStartValueRowDTO = result.row;
                if (isNewRow) {
                    row.actorCompanyId = CoreUtility.actorCompanyId;
                    row.payrollStartValueHeadId = this.payrollStartValueHeadId;
                    this.payrollStartValueHead.rows.push(row);
                } else {
                    if (editRow.payrollStartValueRowId)
                        row = this.payrollStartValueHead.rows.find(r => r.payrollStartValueRowId === editRow.payrollStartValueRowId);
                    else
                        row = this.payrollStartValueHead.rows.find(r => r['tmpId'] === editRow['tmpId']);
                }

                if (row) {
                    row.isModified = true;
                    row['tmpId'] = editRow['tmpId'];
                    row.absenceTimeMinutes = editRow.absenceTimeMinutes;
                    row.amount = editRow.amount;
                    row.appellation = editRow.appellation;
                    row.date = editRow.date;
                    row.doCreateTransaction = editRow.doCreateTransaction;
                    row.employeeId = editRow.employeeId;
                    row['employeeName'] = this.employees.find(e => e.id === row.employeeId)?.name;
                    row.productId = editRow.productId;
                    let product = this.payrollProducts.find(p => p.productId === row.productId);
                    row.productName = product?.name;
                    row.productNr = product?.number;
                    row.productNrAndName = product?.numberName;
                    row.quantity = editRow.quantity;
                    row.scheduleTimeMinutes = editRow.scheduleTimeMinutes;
                    row.state = editRow.state;
                    row.sysPayrollStartValueId = editRow.sysPayrollStartValueId;
                    row.sysPayrollTypeLevel1 = editRow.sysPayrollTypeLevel1;
                    row.sysPayrollTypeLevel2 = editRow.sysPayrollTypeLevel2;
                    row.sysPayrollTypeLevel3 = editRow.sysPayrollTypeLevel3;
                    row.sysPayrollTypeLevel4 = editRow.sysPayrollTypeLevel4;
                }

                this.dirtyHandler.setDirty();
                this.setGridData();
            }
        });
    }

    private setGridData() {
        this.gridHandler.gridAg.setData(_.orderBy(this.payrollStartValueHead.rows.filter(r => r.state === SoeEntityState.Active), 'employeeName'));
    }

    // VALIDATION

    private validateNotDirtyBeforeAction(): boolean {
        if (!this.dirtyHandler.isDirty)
            return true;

        this.notificationService.showDialogEx(this.terms["time.import.payrollstartvalue.validatedirty.title"], this.terms["time.import.payrollstartvalue.validatedirty.message"], SOEMessageBoxImage.Warning);
        return false;
    }

    protected validate() {
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
            if (this.payrollStartValueHead) {
                if (!this.payrollStartValueHead.dateFrom) {
                    mandatoryFieldKeys.push("common.datefrom");
                }
                if (!this.payrollStartValueHead.dateTo) {
                    mandatoryFieldKeys.push("common.dateto");
                }
                if (!this.payrollStartValueHead.importedFrom) {
                    mandatoryFieldKeys.push("time.import.payrollstartvalue.importedfrom");
                }
            }
        });
    }
}