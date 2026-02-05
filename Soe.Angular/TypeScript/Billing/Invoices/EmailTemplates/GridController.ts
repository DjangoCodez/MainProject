import { ITranslationService } from "../../../Core/Services/TranslationService";
import { ICoreService } from "../../../Core/Services/CoreService";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IGridHandlerFactory } from "../../../Core/Handlers/GridHandlerFactory";
import { Feature, EmailTemplateType } from "../../../Util/CommonEnumerations";
import { IGridHandler } from "../../../Core/Handlers/GridHandler";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {

    private emailTemplates: any[] = [];   
    private terms: any;

    //modal
    private modalInstance: any;

    //@ngInject
    constructor(
        private $q: ng.IQService,        
        private translationService: ITranslationService,
        private coreService: ICoreService,
        $uibModal,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory
        ) {
        super(gridHandlerFactory, "Billing.Invoices.EmailTemplates", progressHandlerFactory, messagingHandlerFactory);

        this.modalInstance = $uibModal;

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
            .onDoLookUp(() => this.onDoLookups())
            .onSetUpGrid(() => this.setupGrid())
            .onLoadGridData(() => this.loadGridData(false));
    }

    onInit(parameters: any) {
        this.parameters = parameters;
        this.isHomeTab = !!parameters.isHomeTab;

        if (this.isHomeTab) {
            this.messagingHandler.onGridDataReloadRequired(x => { this.reloadData(); });
        }

        this.flowHandler.start({ feature: Feature.Billing_Preferences_EmailTemplate, loadReadPermissions: true, loadModifyPermissions: true });                
    }

    private onDoLookups() {
        return this.$q.all([
            this.loadTerms()
        ]);
    }

    private loadTerms(): ng.IPromise<any> {
        const keys: string[] = [
            "common.name",
            "common.type",
            "core.edit",
            "billing.invoices.emailtemplates",
            "billing.purchase.list.purchase",
            "common.customer.invoices.reminder",
            "common.salestypes"
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
        });
    }

    edit(row) {
        // Send message to TabsController
        if (this.doubleClickToEdit && (this.readPermission || this.modifyPermission))
            this.messagingHandler.publishEditRow(row);
    }

    public setupGrid() {

        // Columns         
        this.gridAg.addColumnText("name", this.terms["common.name"], null);
        this.gridAg.addColumnText("typename", this.terms["common.type"], 200);           
        this.gridAg.addColumnEdit(this.terms["core.edit"], this.edit.bind(this), false);

        this.gridAg.finalizeInitGrid("billing.invoices.emailtemplates", true);
    }    

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {                
        this.toolbar = toolbarFactory.createDefaultGridToolbar(<IGridHandler>this.gridAg, () => this.reloadData());
    }       

    public loadGridData(useCache: boolean) {                        
        return this.coreService.getEmailTemplates().then((x) => { 
            _.forEach(x, (y) => {
                switch (y.type) {
                    case EmailTemplateType.Invoice:
                        y['typename'] = this.terms["common.salestypes"];
                        break;
                    case EmailTemplateType.Reminder:
                        y['typename'] = this.terms["common.customer.invoices.reminder"];
                        break;
                    case EmailTemplateType.PurchaseOrder:
                        y['typename'] = this.terms["billing.purchase.list.purchase"];
                }
            });
            this.setData(x);                
        });        
    }       

    private reloadData() {        
        this.loadGridData(false);
    }
}