import { ICoreService } from "../../../Core/Services/CoreService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { IUrlHelperService, IUrlHelperServiceProvider } from "../../../Core/Services/UrlHelperService";
import { IReportService } from "../../../Core/Services/ReportService";
import { ToolBarUtility, ToolBarButton } from "../../../Util/ToolBarUtility";
import { IconLibrary, SOEMessageBoxImage, SOEMessageBoxButtons, SoeGridOptionsEvent } from "../../../Util/Enumerations";
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

    //Terms
    terms: { [index: string]: string; };

    //@ngInject
    constructor($http,
        $templateCache,
        private $timeout: ng.ITimeoutService,
        $uibModal,
        private $filter: ng.IFilterService,
        coreService: ICoreService,
        private translationService: ITranslationService,
        messagingService: IMessagingService,
        private notificationService: INotificationService,
        urlHelperService: IUrlHelperService,
        private reportService: IReportService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory) {

        super(gridHandlerFactory, "common.report.reportheader.reportheaders", progressHandlerFactory, messagingHandlerFactory);

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
            this.deleteSelectedItems();
        }, () => {
            return this.selectedCount === 0;
        })));
    }

    public setupGrid() {
        // Columns
        var keys: string[] = [
            "common.name",
            "common.description",
            "core.edit",
            "common.report.reportheader.reporttype",
            "common.report.reportheader.deleteerror",
        ];

        this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;

            this.gridAg.addColumnText("name", terms["common.name"], null);
            this.gridAg.addColumnText("description", terms["common.description"], null);
            this.gridAg.addColumnText("templateType", terms["common.report.reportheader.reporttype"], null);
            this.gridAg.addColumnEdit(terms["core.edit"], this.edit.bind(this));

            this.gridAg.finalizeInitGrid("common.report.reportheader.reportheaders", true);

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
        // Load data
        this.reportService.getReportHeadersForModule(soeConfig.module, false).then((x) => {
            this.gridAg.setData(x);
        });
    }

    private deleteSelectedItems() {
        var rows = this.gridAg.options.getSelectedRows();
        var ids = [];
        _.forEach(rows, (row) => {
            ids.push(row.reportHeaderId);
        });

        this.progress.startDeleteProgress((completion) => {
            this.reportService.deleteReportHeaders(ids).then((result) => {
                if (result.success) {
                    if (result.errorNumber === 51) {
                        completion.failed(this.terms["common.report.reportheader.deleteerror"]);
                    } else {
                        completion.completed(null);
                    }
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        });
    }
}