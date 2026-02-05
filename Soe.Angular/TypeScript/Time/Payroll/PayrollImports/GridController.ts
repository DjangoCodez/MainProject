import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { IGridHandlerFactory } from "../../../Core/Handlers/GridHandlerFactory";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IconLibrary, SoeGridOptionsEvent, SOEMessageBoxButtons, SOEMessageBoxImage } from "../../../Util/Enumerations";
import { Feature } from "../../../Util/CommonEnumerations";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { IGridHandler } from "../../../Core/Handlers/GridHandler";
import { IPayrollService } from "../PayrollService";
import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { GridEvent } from "../../../Util/SoeGridOptions";
import { PayrollImportHeadDTO } from "../../../Common/Models/PayrollImport";
import { FileImportDialogController } from "./Dialogs/FileImport/FileImportDialogController";
import { ToolBarButton, ToolBarUtility } from "../../../Util/ToolBarUtility";
import { INotificationService } from "../../../Core/Services/NotificationService";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {

    // Terms:
    private terms: { [index: string]: string; };

    // Data
    private importHeads: PayrollImportHeadDTO[];
    private selectedCount: number;

    //@ngInject
    constructor(
        private $uibModal,
        private payrollService: IPayrollService,
        private translationService: ITranslationService,
        private $timeout: ng.ITimeoutService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory,
        private notificationService: INotificationService,
        private urlHelperService: IUrlHelperService) {
        super(gridHandlerFactory, "Time.Payroll.PayrollImports", progressHandlerFactory, messagingHandlerFactory);
        this.onTabActivetedAndModified(() => this.loadGridData());

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onAllPermissionsLoaded(x => this.onPermissionsLoaded(x))
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
            .onBeforeSetUpGrid(() => this.loadTerms())
            .onSetUpGrid(() => this.setupGrid())
            .onLoadGridData(() => this.loadGridData());
    }

    // SETUP

    public onInit(parameters: any) {
        this.parameters = parameters;
        this.isHomeTab = !!parameters.isHomeTab;
        if (this.isHomeTab) {
            this.messagingHandler.onGridDataReloadRequired(x => { this.loadGridData(); });
        }

        this.flowHandler.start([
            { feature: Feature.Time_Import_PayrollImport, loadReadPermissions: true, loadModifyPermissions: true },
        ]);
    }

    private onPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readPermission = response[Feature.Time_Import_PayrollImport].readPermission;
        this.modifyPermission = response[Feature.Time_Import_PayrollImport].modifyPermission;
    }

    public setupGrid() {
        this.doubleClickToEdit = true;
        this.gridAg.options.enableRowSelection = false;

        const isDeletedFunc = (data: PayrollImportHeadDTO) => data && data.isDeleted;

        this.gridAg.addColumnText("typeName", this.terms["common.type"], 50, true, { strikeThrough: isDeletedFunc });
        this.gridAg.addColumnText("fileTypeName", this.terms["time.payroll.payrollimport.filetype"], 50, true, { strikeThrough: isDeletedFunc });
        this.gridAg.addColumnDate("dateFrom", this.terms["common.from"], 50, false, null, { strikeThrough: isDeletedFunc });
        this.gridAg.addColumnDate("dateTo", this.terms["common.to"], 50, false, null, { strikeThrough: isDeletedFunc });
        this.gridAg.addColumnDate("paymentDate", this.terms["time.payroll.payrollimport.paymentdate"], 50, false, null, { strikeThrough: isDeletedFunc });
        this.gridAg.addColumnNumber("nrOfEmployees", this.terms["common.employees"], 30, { strikeThrough: isDeletedFunc, enableHiding: true });
        this.gridAg.addColumnText("comment", this.terms["common.comment"], null, true, { strikeThrough: isDeletedFunc });
        this.gridAg.addColumnIcon("statusIcon", null, null, { toolTipField: "statusName" });
        this.gridAg.addColumnText("statusName", this.terms["common.status"], 75, true, { strikeThrough: isDeletedFunc });
        if (this.modifyPermission) {
            this.gridAg.addColumnDelete(this.terms["time.payroll.payrollimport.rollback.file"], this.initRollbackFile.bind(this), false);
            this.gridAg.addColumnDelete(this.terms["time.payroll.payrollimport.rollback"], this.initRollback.bind(this), false, () => { return true }, "fal fa-undo iconDelete");
            this.gridAg.addColumnEdit(this.terms["common.edit"], this.edit.bind(this));
        }

        let events: GridEvent[] = [];
        events.push(new GridEvent(SoeGridOptionsEvent.RowSelectionChanged, (row: any) => {
            this.$timeout(() => {
                this.selectedCount = this.gridAg.options.getSelectedCount();
            });
        }));
        events.push(new GridEvent(SoeGridOptionsEvent.RowSelectionChangedBatch, (row: any) => {
            this.$timeout(() => {
                this.selectedCount = this.gridAg.options.getSelectedCount();
            });
        }));
        this.gridAg.options.subscribe(events);

        this.gridAg.finalizeInitGrid("Time.Payroll.PayrollImports", true);
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(<IGridHandler>this.gridAg, () => this.reloadData());

        this.toolbar.addButtonGroup(ToolBarUtility.createGroup(new ToolBarButton("time.payroll.payrollimport.importfile", "time.payroll.payrollimport.importfile", IconLibrary.FontAwesome, "fa-file-download", () => this.importFile())));
    }

    // SERVICE CALLS

    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
            "core.aggrid.totals.filtered",
            "core.aggrid.totals.total",
            "core.info",
            "core.error",
            "common.type",
            "common.from",
            "common.to",
            "common.comment",
            "common.employee",
            "common.employees",
            "common.quantity",
            "common.amount",
            "common.status",
            "time.payroll.payrollimport.filetype",
            "time.payroll.payrollimport.paymentdate",
            "time.payroll.payrollimport.employee.schedule",
            "time.payroll.payrollimport.employee.transactions",
            "time.payroll.payrollimport.employee.rows",
            "time.payroll.payrollimport.employee.schedule.length",
            "time.payroll.payrollimport.employee.schedule.break",
            "time.payroll.payrollimport.rollback",
            "time.payroll.payrollimport.rollback.validate",
            "time.payroll.payrollimport.rollback.file",
            "time.payroll.payrollimport.rollback.file.validate",
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
        });
    }

    public loadGridData() {
        this.progress.startLoadingProgress([() => {
            return this.payrollService.getPayrollImportHeads(false, true, true, true).then(x => {
                this.importHeads = x;
                this.setData(this.importHeads);
            });
        }]);
    }

    private reloadData() {
        this.loadGridData();
    }

    private loadDetailData(params: any) {
        if (!params || params["rowsLoaded"])
            return;

        this.progress.startLoadingProgress([() => {
            return this.payrollService.getPayrollImportEmployees(params.data.payrollImportHeadId, true, true, false, false).then(x => {
                params.data['employees'] = x;
                params.data['rowsLoaded'] = true;
                params.successCallback();
            });
        }]).then(() => {
            params.successCallback(params.data['employees']);
        });
    }

    // ACTIONS

    private importFile() {
        let options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Payroll/PayrollImports/Dialogs/FileImport/fileImportDialog.html"),
            controller: FileImportDialogController,
            controllerAs: "ctrl",
            size: 'sm',
            resolve: {
            }
        }

        this.$uibModal.open(options).result.then(importResult => {
            if (importResult?.result && importResult.result.success) {
                this.loadGridData();
                this.notificationService.showDialog(this.terms["core.info"], importResult?.result?.errorMessage, SOEMessageBoxImage.Information, SOEMessageBoxButtons.OK);
            }                
            else
                this.notificationService.showDialog(this.terms["core.error"], importResult?.result?.errorMessage, SOEMessageBoxImage.Error, SOEMessageBoxButtons.OK);
        });
    }

    private initRollback(row: PayrollImportHeadDTO) {
        this.progress.startDeleteProgress((completion) => {
            this.payrollService.payrollImportExecuteRollback(row.payrollImportHeadId, [], true).then(result => {
                if (result.success) {
                    completion.completed(null, true);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }, () => this.reloadData(), this.terms["time.payroll.payrollimport.rollback.validate"]);
    }

    private initRollbackFile(row: PayrollImportHeadDTO) {
        this.progress.startDeleteProgress((completion) => {
            this.payrollService.payrollImportExecuteRollbackFile(row.payrollImportHeadId, [], true).then(result => {
                if (result.success) {
                    completion.completed(null, true);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }, () => this.reloadData(), this.terms["time.payroll.payrollimport.rollback.file.validate"]);
    }
}