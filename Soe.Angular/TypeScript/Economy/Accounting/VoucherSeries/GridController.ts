import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IGridHandlerFactory } from "../../../Core/Handlers/GridHandlerFactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { IAccountingService } from "../../../Shared/Economy/Accounting/AccountingService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { Feature } from "../../../Util/CommonEnumerations";
import { TabMessage } from "../../../Core/Controllers/TabsControllerBase1";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { EditController } from "./EditController";
import { Constants } from "../../../Util/Constants";
import { ToolBarButton, ToolBarUtility } from "../../../Util/ToolBarUtility";
import { IconLibrary } from "../../../Util/Enumerations";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {

    // Flags
    setupComplete;

    //@ngInject
    constructor(
        private accountingService: IAccountingService,
        private translationService: ITranslationService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory,
        private urlHelperService: IUrlHelperService,
        private messagingService: IMessagingService) {
        super(gridHandlerFactory, "Economy.Accounting.VoucherSeriesTypes", progressHandlerFactory, messagingHandlerFactory);

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

        this.onTabActivated(() => this.localOnTabActivated());
    }

    onInit(parameters: any) {
        this.parameters = parameters;
        this.isHomeTab = !!parameters.isHomeTab;
        this.guid = parameters.guid;

        if (this.isHomeTab) {
            this.messagingHandler.onGridDataReloadRequired(x => { this.loadGridData(); });
        }
    }

    private localOnTabActivated() {
        if (!this.setupComplete) {
            this.flowHandler.start({ feature: Feature.Economy_Accounting_VoucherSeries, loadReadPermissions: true, loadModifyPermissions: true });
            this.setupComplete = true;
        }
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(this.gridAg, () => this.loadGridData());
        this.toolbar.addButtonGroup(ToolBarUtility.createGroup(new ToolBarButton("economy.accounting.accountyear.createvoucherserie", "economy.accounting.accountyear.createvoucherserie", IconLibrary.FontAwesome, "fa-plus",
            () => { this.edit(null) },
            () => { return !this.modifyPermission })));
    }

    protected setupGrid() {

        // Columns
        const keys: string[] = [
            "economy.accounting.voucherseriestype.voucherseriestypenr",
            "economy.accounting.voucherseriestype.name",
            "economy.accounting.voucherseriestype.startnr",
            "core.edit",
            "core.aggrid.totals.filtered",
            "core.aggrid.totals.total",
        ];

        this.translationService.translateMany(keys).then((terms) => {
            this.gridAg.addColumnNumber("voucherSeriesTypeNr", terms["economy.accounting.voucherseriestype.voucherseriestypenr"], 40);
            this.gridAg.addColumnText("name", terms["economy.accounting.voucherseriestype.name"], null);
            this.gridAg.addColumnNumber("startNr", terms["economy.accounting.voucherseriestype.startnr"], 40);
            this.gridAg.addColumnEdit(terms["core.edit"], this.edit.bind(this));

            this.gridAg.finalizeInitGrid("economy.accounting.voucherseriestypes", true);
        });
    }

    edit(row) {
        // Send message to TabsController
        if (this.doubleClickToEdit && (this.readPermission || this.modifyPermission)) {
            const translationKeys: string[] = [
                "economy.accounting.voucherseriestype",
                "economy.accounting.newvoucherseriestype"
            ];

            this.translationService.translateMany(translationKeys).then((terms) => {
                const ids = _.map(this.gridAg.options.getData(), 'voucherSeriesTypeId');
                const message = new TabMessage(
                    `${row ? terms["economy.accounting.voucherseriestype"] : terms["economy.accounting.newvoucherseriestype"]} ${row ? row.voucherSeriesTypeNr : ""}`,
                    row ? row.voucherSeriesTypeId.toString() : "",
                    EditController,
                    { id: row ? row.voucherSeriesTypeId : undefined, ids: ids },
                    this.urlHelperService.getGlobalUrl("/Economy/Accounting/VoucherSeries/Views/edit.html")
                );
                this.messagingService.publish(Constants.EVENT_OPEN_TAB, message);
            });
        }
    }

    public loadGridData() {
        this.gridAg.clearData();
        this.progress.startLoadingProgress([() => {
            return this.accountingService.getVoucherSeriesTypes(false).then(data => {
                this.setData(data);
            });
        }]);
    }
}


