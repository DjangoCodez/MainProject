import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { CoreUtility } from "../../../Util/CoreUtility";
import { IInvoiceService } from "../../../Shared/Billing/Invoices/InvoiceService";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IGridHandlerFactory } from "../../../Core/Handlers/GridHandlerFactory";
import { Feature } from "../../../Util/CommonEnumerations";
import { CompanyWholesellerListDTO } from "../../../Common/Models/CompanyWholesellerListDTO";
import { IGridHandler } from "../../../Core/Handlers/GridHandler";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {

    private companyWholesellers: CompanyWholesellerListDTO[] = [];    

    //modal
    private modalInstance: any;

    //@ngInject
    constructor(private invoiceService: IInvoiceService,
        private translationService: ITranslationService,
        $uibModal,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory,
    ) {
        super(gridHandlerFactory, "Billing.Invoices.WholesellerSettings", progressHandlerFactory, messagingHandlerFactory);

        this.modalInstance = $uibModal;

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onPermissionsLoaded((feature, readOnly, modify) => {                
                this.readPermission = readOnly;
                this.modifyPermission = modify || CoreUtility.isSupportAdmin;
                if (this.modifyPermission) {
                    // Send messages to TabsController
                    this.messagingHandler.publishActivateAddTab();
                }
            })                                    
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
            .onSetUpGrid(() => this.setupGrid())
            .onLoadGridData(() => this.loadGridData());
    }

    onInit(parameters: any) {
        this.parameters = parameters;
        this.flowHandler.start({ feature: Feature.Billing_Preferences_Wholesellers, loadReadPermissions: true, loadModifyPermissions: true });                
    }

    public setupGrid() {

        // Columns
        var keys: string[] = [            
            "common.active",
            "common.name",
            "billing.invoices.wholesellersettings.wholesellers",   
            "core.edit"
        ];

        this.translationService.translateMany(keys).then((terms) => {
            this.gridAg.addColumnBool("active", terms["common.active"], 5, false);
            this.gridAg.addColumnText("name", terms["common.name"], 50);       
            this.gridAg.addColumnEdit(terms["core.edit"], this.edit.bind(this), false);
            
            this.gridAg.finalizeInitGrid("billing.invoices.wholesellersettings.wholesellers",true);
        });
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(<IGridHandler>this.gridAg, () => this.loadGridData());
    }       

    public loadGridData() {                
        this.progress.startLoadingProgress([() => {
            return this.invoiceService.getCompanyWholesellers().then((x) => {                                
                this.setData(x);                
            });
        }]);
    }       
}