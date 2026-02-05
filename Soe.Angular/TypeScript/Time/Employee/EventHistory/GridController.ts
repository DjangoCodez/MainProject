import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { ICoreService } from "../../../Core/Services/CoreService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { Feature, TermGroup, SoeEntityType } from "../../../Util/CommonEnumerations";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/controllerflowhandlerfactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/messaginghandlerfactory";
import { IGridHandlerFactory } from "../../../Core/Handlers/gridhandlerfactory";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { IGridHandler } from "../../../Core/Handlers/GridHandler";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ISmallGenericType } from "../../../Scripts/TypeLite.Net4";
import { IEmployeeService } from "../EmployeeService";
import { EventHistoryDTO } from "../../../Common/Models/EventHistoryDTO";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { SOEMessageBoxImage, SOEMessageBoxButtons } from "../../../Util/Enumerations";
import { ModalUtility } from "../../../Util/ModalUtility";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {

    // Data
    private types: ISmallGenericType[];
    private employees: ISmallGenericType[];

    private gridHeaderComponentUrl: any;

    // Properties
    private type: number;
    private employee: ISmallGenericType;
    private dateFrom: Date;
    private dateTo: Date;

    //@ngInject
    constructor(
        private coreService: ICoreService,
        private employeeService: IEmployeeService,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        urlHelperService: IUrlHelperService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory) {
        super(gridHandlerFactory, "Time.Employee.EventHistory", progressHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onPermissionsLoaded((feature, readOnly, modify) => {
                this.readPermission = readOnly;
                this.modifyPermission = modify;
                if (this.modifyPermission) {
                    // Send messages to TabsController
                    this.messagingHandler.publishActivateAddTab();
                }
            })
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
            .onBeforeSetUpGrid(() => this.loadTypes())
            .onBeforeSetUpGrid(() => this.loadEmployees())
            .onSetUpGrid(() => this.setupGrid())
            .onLoadGridData(() => this.loadGridData());

        this.gridHeaderComponentUrl = urlHelperService.getViewUrl("gridHeader.html");
    }

    onInit(parameters: any) {
        this.parameters = parameters;
        this.isHomeTab = !!parameters.isHomeTab;

        // Default search values
        this.dateFrom = new Date();
        this.dateTo = new Date();

        if (this.isHomeTab) {
            this.messagingHandler.onGridDataReloadRequired(x => {
                this.reloadData();
            });
        }

        this.flowHandler.start({ feature: Feature.Time_Employee_EventHistory, loadReadPermissions: true, loadModifyPermissions: true });
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(<IGridHandler>this.gridAg, () => this.loadGridData());
    }

    public setupGrid() {
        // Columns
        const keys: string[] = [
            "core.delete",
            "core.edit",
            "common.bool",
            "common.created",
            "common.createdby",
            "common.date",
            "common.decimal",
            "common.int",
            "common.name",
            "common.string",
            "common.type"
        ];

        this.translationService.translateMany(keys).then((terms) => {
            this.gridAg.options.useGrouping(false, false, { keepColumnsAfterGroup: true, selectChildren: false });
            this.gridAg.options.groupHideOpenParents = true;

            this.gridAg.addColumnNumber("batchId", "Batch", 75, { enableRowGrouping: true });
            this.gridAg.addColumnText("typeName", terms["common.type"], 150, false, { enableRowGrouping: true });
            this.gridAg.addColumnText("recordName", terms["common.name"], 150, false, { enableRowGrouping: true });

            this.gridAg.addColumnText("stringValue", terms["common.string"], null, false, { enableRowGrouping: true });
            this.gridAg.addColumnNumber("integerValue", terms["common.int"], 100, { enableRowGrouping: true });
            this.gridAg.addColumnNumber("decimalValue", terms["common.decimal"], 100, { enableRowGrouping: true, decimals: 2 });
            this.gridAg.addColumnBoolEx("booleanValue", terms["common.bool"], 50, { enableRowGrouping: true });
            this.gridAg.addColumnDate("dateValue", terms["common.date"], 150, false, null, { enableRowGrouping: true });

            this.gridAg.addColumnDate("created", terms["common.created"], 150, true, null, { enableRowGrouping: true });
            this.gridAg.addColumnText("createdBy", terms["common.createdby"], 150, true, { enableRowGrouping: true });

            this.gridAg.addColumnDelete(terms["core.delete"], this.initDeleteRow.bind(this));
            this.gridAg.addColumnEdit(terms["core.edit"], this.edit.bind(this), false);

            this.gridAg.options.enableRowSelection = false;
            this.gridAg.finalizeInitGrid("core.eventhistory", true);
        });
    }

    private loadTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.EventHistoryType, true, true).then(x => {
            this.types = x;
        });
    }

    private loadEmployees(): ng.IPromise<any> {
        return this.employeeService.getEmployeesDict(false, true, false, true).then(x => {
            this.employees = x;
        });
    }

    public loadGridData() {
        if ((!this.type && !this.employee) || !this.dateFrom || !this.dateTo)
            return;

        this.gridAg.clearData();

        this.progress.startLoadingProgress([() => {
            return this.coreService.getEventHistories(this.type || 0, SoeEntityType.Employee, (this.employee && this.employee.id) || 0, this.dateFrom, this.dateTo, true).then(x => {
                this.setData(x);
            });
        }]);
    }

    private reloadData() {
        this.loadGridData();
    }

    private initDeleteRow(row: EventHistoryDTO) {
        // Check if batch contains multiple records
        this.coreService.getNbrOfEventsInBatch(row.type, row.batchId).then(nbr => {
            if (nbr === 1)
                this.deleteRow(row, true);
            else if (nbr > 1) {
                this.translationService.translateMany(["core.delete", "core.eventhistory.deletebatch"]).then(terms => {
                    var modal = this.notificationService.showDialogEx(terms["core.delete"], terms["core.eventhistory.deletebatch"].format(nbr.toString()), SOEMessageBoxImage.Warning, SOEMessageBoxButtons.YesNoCancel, { buttonYesLabelKey: "core.eventhistoty.deletebatch.all", buttonNoLabelKey: "core.eventhistoty.deletebatch.current" });
                    modal.result.then(val => {
                        if (val === true)
                            this.deleteRows(row, false);
                        else
                            this.deleteRow(row, false);
                    }, (reason) => { });
                });
            }
        });
    }

    private deleteRow(row: EventHistoryDTO, confirm: boolean) {
        this.progress.startDeleteProgress((completion) => {
            this.coreService.deleteEventHistory(row.eventHistoryId).then((result) => {
                if (result.success) {
                    completion.completed(row, true);
                }
                else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }, null, confirm ? '' : ModalUtility.MODAL_SKIP_CONFIRM).then(x => {
            this.loadGridData();
        });
    }

    private deleteRows(row: EventHistoryDTO, confirm: boolean) {
        this.progress.startDeleteProgress((completion) => {
            this.coreService.deleteEventHistories(row.type, row.batchId).then((result) => {
                if (result.success) {
                    completion.completed(row, true);
                }
                else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }, null, confirm ? '' : ModalUtility.MODAL_SKIP_CONFIRM).then(x => {
            this.loadGridData();
        });
    }
}
