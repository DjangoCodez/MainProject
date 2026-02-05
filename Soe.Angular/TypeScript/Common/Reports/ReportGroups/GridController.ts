import { ICoreService } from "../../../Core/Services/CoreService";
import { IReportService } from "../../../Core/Services/ReportService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { IUrlHelperService, IUrlHelperServiceProvider } from "../../../Core/Services/UrlHelperService";
import { ToolBarUtility, ToolBarButton } from "../../../Util/ToolBarUtility";
import { SOEMessageBoxImage, SOEMessageBoxButtons, IconLibrary, SoeGridOptionsEvent } from "../../../Util/Enumerations";
import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/controllerflowhandlerfactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/messaginghandlerfactory";
import { IGridHandlerFactory } from "../../../Core/Handlers/gridhandlerfactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { GridEvent } from "../../../Util/SoeGridOptions";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {
    private selectedCount: number = 0;

    //@ngInject
    constructor($http,
        $templateCache,
        private $timeout: ng.ITimeoutService,
        $uibModal,
        private $filter: ng.IFilterService,
        coreService: ICoreService,
        private reportService: IReportService,
        private translationService: ITranslationService,
        messagingService: IMessagingService,
        private notificationService: INotificationService,
        private urlHelperService: IUrlHelperService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory) {

        super(gridHandlerFactory, "common.report.reportgroup.reportgroups", progressHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onPermissionsLoaded((feature, readOnly, modify) => {
                this.readPermission = readOnly;
                this.modifyPermission = modify

                if (this.modifyPermission) {
                    // Send messages to TabsController
                    this.messagingHandler.publishActivateAddTab();
                }
            })
            .onSetUpGrid(() => this.setupGrid())
            .onLoadGridData(() => this.loadGridData())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))

    }

    onInit(parameters: any) {
        this.parameters = parameters;
        this.isHomeTab = !!parameters.isHomeTab;

        if (this.isHomeTab) {
            this.messagingHandler.onGridDataReloadRequired(x => { this.loadGridData(); });
        }

        this.flowHandler.start({ feature: soeConfig.feature, loadReadPermissions: true, loadModifyPermissions: true });
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(this.gridAg, () => this.loadGridData());

        this.toolbar.addButtonGroup(ToolBarUtility.createGroup(new ToolBarButton("", "core.delete", IconLibrary.FontAwesome, "fa-times", () => {
            this.initDeleteSelectedItems();
        }, () => {
            return this.selectedCount === 0;
        })));
    }

    public setupGrid() {
        // Columns
        var keys: string[] = [
            "common.name",
            "common.description",
            "common.report.reportgroup.reporttype",
            "core.edit"
        ];

        this.translationService.translateMany(keys).then((terms) => {
            this.gridAg.addColumnText("name", terms["common.name"], null);
            this.gridAg.addColumnText("description", terms["common.description"], null);
            this.gridAg.addColumnText("templateType", terms["common.report.reportgroup.reporttype"], null);
            this.gridAg.addColumnEdit(terms["core.edit"], this.edit.bind(this));
            
            this.gridAg.finalizeInitGrid("common.report.reportgroup.reportgroup", true);

            var events: GridEvent[] = [];
            events.push(new GridEvent(SoeGridOptionsEvent.RowSelectionChangedBatch, (rowNode) => { this.selectionChanged() }));
            events.push(new GridEvent(SoeGridOptionsEvent.RowSelectionChanged, (rowNode) => { this.selectionChanged() }));
            this.gridAg.options.subscribe(events);
        });
    }

    private selectionChanged() {
        this.$timeout(() => {
            this.selectedCount = this.gridAg.options.getSelectedCount();
        });
    }

    public loadGridData() {
        this.reportService.getReportGroupByModule(soeConfig.module).then((x) => {
            this.setData(x);
        });
    }

    private initDeleteSelectedItems() {
        if (this.gridAg.options.getSelectedCount() > 0) {
            // Show verification dialog
            var keys: string[] = [
                "core.warning",
                "common.report.reportgroup.deleteselectedreportgroupswarning"
            ];
            this.translationService.translateMany(keys).then((terms) => {
                var modal = this.notificationService.showDialog(terms["core.warning"], terms["common.report.reportgroup.deleteselectedreportgroupswarning"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
                modal.result.then(val => {
                    if (val) {
                        this.deleteSelectedItems();
                    }
                });
            });
        }
    }

    private deleteSelectedItems() {
        var rows = this.gridAg.options.getSelectedRows();
        var ids = [];
        _.forEach(rows, (row) => {
            ids.push(row.reportGroupId);
        });

        this.progress.startDeleteProgress((completion) => {
            this.reportService.deleteReportGroups(ids).then((result) => {
                if (result.success) {
                    completion.completed(null);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        });
    }
}