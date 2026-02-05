import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/messaginghandlerfactory";
import { IGridHandlerFactory } from "../../../Core/Handlers/gridhandlerfactory";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { Feature } from "../../../Util/CommonEnumerations";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/controllerflowhandlerfactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { ISystemService } from "../SystemService";
import { IGridHandler } from "../../../Core/Handlers/GridHandler";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { CalendarUtility } from "../../../Util/CalendarUtility";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {

    private fromDate: Date;
    private toDate: Date;

    //@ngInject
    constructor(
        private translationService: ITranslationService,
        private urlHelperService: IUrlHelperService,
        private systemService: ISystemService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory
    ) {
        super(gridHandlerFactory, "Manage.System.VolymInvoiceing", progressHandlerFactory, messagingHandlerFactory);

        this.fromDate = CalendarUtility.getDateToday().beginningOfMonth();
        this.toDate = CalendarUtility.getDateToday().endOfMonth();

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
            .onSetUpGrid(() => this.setupGrid())
            .onLoadGridData(() => this.loadGridData());
    };

    onInit(parameters: any) {
        this.parameters = parameters;
        this.isHomeTab = !!parameters.isHomeTab;

        if (this.isHomeTab) {
            this.messagingHandler.onGridDataReloadRequired(x => { this.loadGridData(); });
        }

        this.flowHandler.start({ feature: Feature.Manage_System, loadReadPermissions: true, loadModifyPermissions: true });
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(<IGridHandler>this.gridAg, () => this.loadGridData());
        
        this.toolbar.addInclude( this.urlHelperService.getGlobalUrl("manage/system/volyminvoicing/views/gridHeader.html") );
    }

    private setupGrid() {
        var translationKeys: string[] = [
            "manage.system.volyminvoicing.wholeseller",
            "manage.system.volyminvoicing.licensenr",
            "common.number",
            "common.name",
            "common.orgnr",
            "common.type"
        ];

        this.translationService.translateMany(translationKeys).then(terms => {

            var columOptions = { enableHiding: false, enableRowGrouping: true }

            this.gridAg.addColumnText("licenseNr", terms["manage.system.volyminvoicing.licensenr"], null, false, columOptions);
            this.gridAg.addColumnText("companyNr", terms["common.number"], null, false, columOptions);
            this.gridAg.addColumnText("companyName", terms["common.name"], null, false, columOptions);
            this.gridAg.addColumnText("companyOrgNr", terms["common.orgnr"], null, false, columOptions);
            this.gridAg.addColumnText("wholesellerName", terms["manage.system.volyminvoicing.wholeseller"], null, false, columOptions);
            this.gridAg.addColumnText("messageTypeName", terms["common.type"], null, false, columOptions);

            this.gridAg.options.useGrouping();
            this.gridAg.finalizeInitGrid("manage.system.volyminvoiceing.volyminvoiceing", true);
        });
    }

    private loadGridData() {
        this.progress.startLoadingProgress([() => {
            return this.systemService.getEdiEntries(this.fromDate, this.toDate).then((x) => {
                this.setData(x);
            });
        }]);
    }

    private decreaseDate() {
        this.fromDate = this.fromDate.addMonths(-1);
        this.toDate = this.fromDate.endOfMonth();
    }

    private increaseDate() {
        this.fromDate = this.fromDate.addMonths(1);
        this.toDate = this.fromDate.endOfMonth();
    }
}