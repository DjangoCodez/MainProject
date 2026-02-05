import { ITranslationService } from "../../../Core/Services/TranslationService";
import { ICoreService } from "../../../Core/Services/CoreService";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IGridHandlerFactory } from "../../../Core/Handlers/GridHandlerFactory";
import { IGridHandler } from "../../../Core/Handlers/GridHandler";
import { Feature, SoeEntityType, TermGroup } from "../../../Util/CommonEnumerations";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {   
    
    textblockTypes: any[];

    //@ngInject
    constructor(
        private $q: ng.IQService,          
        private translationService: ITranslationService,
        private coreService: ICoreService,                
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory,        
    ) {
        super(gridHandlerFactory, "Billing.Invoices.TextBlocks", progressHandlerFactory, messagingHandlerFactory);        

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

        this.flowHandler.start({ feature: Feature.Billing_Preferences_Textblock, loadReadPermissions: true, loadModifyPermissions: true });                   
        }
    }

    // LOOKUPS
    private onDoLookups() {
        return this.$q.all([
            this.loadTextBlockTypes(),
        ]);
    }

    private loadTextBlockTypes(): ng.IPromise<any> {
        this.textblockTypes = [];
        return this.coreService.getTermGroupContent(TermGroup.TextBlockType, false, true).then((x) => {
            _.forEach(x, (y) => {
                this.textblockTypes.push({ "id": y.id, "value": y.name });
            });            
        });
    }    

    edit(row) {
        // Send message to TabsController        
        if (this.readPermission || this.modifyPermission)
            this.messagingHandler.publishEditRow(row);
    }

    public setupGrid() {

        // Columns
        var keys: string[] = [            
            "common.active",
            "common.name",
            "common.type",
            "common.offer",
            "common.contract",
            "common.order",
            "common.customerinvoice",
            "billing.purchase.list.purchase",
            "billing.invoices.textblocks.textblocks",   
            "core.edit"
        ];

        this.translationService.translateMany(keys).then((terms) => {                        
            this.gridAg.addColumnText("headline", terms["common.name"], null);          
            this.gridAg.addColumnSelect("textBlockTypeName", terms["common.type"], null, { displayField: "textBlockTypeName", selectOptions: null, populateFilterFromGrid: true });
            this.gridAg.addColumnBool("showInOrder", terms["common.order"], null);
            this.gridAg.addColumnBool("showInInvoice", terms["common.customerinvoice"], null);
            this.gridAg.addColumnBool("showInOffer", terms["common.offer"], null);
            this.gridAg.addColumnBool("showInContract", terms["common.contract"], null);
            this.gridAg.addColumnBool("showInPurchase", terms["billing.purchase.list.purchase"], null);
            this.gridAg.addColumnEdit(terms["core.edit"], this.edit.bind(this), false);                       

            this.gridAg.finalizeInitGrid("billing.invoices.textblocks.textblocks", true);
        });
    }
    
    private onCreateToolbar(toolbarFactory: IToolbarFactory) {                
        this.toolbar = toolbarFactory.createDefaultGridToolbar(<IGridHandler>this.gridAg, () => this.reloadData());
    }           

    public loadGridData(useCache: boolean) {                
        this.progress.startLoadingProgress([() => {
            return this.coreService.getTextBlocks(SoeEntityType.CustomerInvoice).then((x) => {           

                _.forEach(x, (row: any) => {
                    var textBlockType = _.find(this.textblockTypes, { id: row.type })
                    row["textBlockTypeName"] = textBlockType ? textBlockType.value : "";
                });

                this.setData(x);               
            });
        }]);
    }       

    private reloadData() {        
        this.loadGridData(false);
    }
    

}