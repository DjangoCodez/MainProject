import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IRegistryService } from "../RegistryService";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IGridHandlerFactory } from "../../../Core/Handlers/GridHandlerFactory";
import { IGridHandler } from "../../../Core/Handlers/GridHandler";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { Feature } from "../../../Util/CommonEnumerations";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {

    private terms: { [index: string]: string; };
    private yesNoDict: any[] = [];    

    //@ngInject
    constructor(        
        private readonly translationService: ITranslationService,
        private readonly registryService: IRegistryService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory) {
        super(gridHandlerFactory, "Manage.Registry.Checklists", progressHandlerFactory, messagingHandlerFactory);
        
        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onPermissionsLoaded((feature, readOnly, modify) => {                                
                this.readPermission = readOnly;
                this.modifyPermission = modify;
                
                if (this.modifyPermission) {
                    // Send messages to TabsController
                    this.messagingHandler.publishActivateAddTab();
                }
            })      
            .onBeforeSetUpGrid(() => this.loadTerms())
            .onSetUpGrid(() => this.setUpGrid())
            .onLoadGridData(() => this.loadGridData())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));
    }

    public onInit(parameters: any) {
        this.parameters = parameters;
        this.isHomeTab = !!parameters.isHomeTab;
        this.guid = this.parameters.guid;
        
        if (this.isHomeTab) {
            this.messagingHandler.onGridDataReloadRequired(x => { this.loadGridData(); });
        }

        this.flowHandler.start({ feature: Feature.Manage_Preferences_Registry_Checklists, loadReadPermissions: true, loadModifyPermissions: true });                         
    }    

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {        
        this.toolbar = toolbarFactory.createDefaultGridToolbar(<IGridHandler>this.gridAg, () => this.loadGridData());
    }

    private loadTerms(): ng.IPromise<any> {        
        const keys: string[] = [            
            "core.yes",
            "core.no",
            "core.edit",
            "common.active",
            "manage.registry.checklists.name",
            "manage.registry.checklists.description",
            "manage.registry.checklists.checklists", 
            "manage.registry.checklists.showdefaultinorder",
            "manage.registry.checklists.type",
            "manage.registry.checklists.addattachementstoeinvoice"
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
            
            this.yesNoDict.push({ id: 1, name: this.terms["core.yes"] })
            this.yesNoDict.push({ id: 2, name: this.terms["core.no"] })
        });
    }

    public loadGridData() {        
        this.progress.startLoadingProgress([() => {
            return this.registryService.getChecklistHeads(false).then((x) => {                
                _.forEach(x, (item) => {                                        
                    item["defaultInOrderName"] = item.defaultInOrder ? this.yesNoDict[0].name : this.yesNoDict[1].name;                    
                })
                this.setData(x);
            });
        }]);
    }

    private setUpGrid() {
        this.gridAg.addColumnActive("isActive", this.terms["common.active"], 60);
        //(params) => this.selectedItemsService.CellChanged(params)
        this.gridAg.addColumnText("name", this.terms["manage.registry.checklists.name"], null);
        this.gridAg.addColumnText("description", this.terms["manage.registry.checklists.description"], null);
        this.gridAg.addColumnText("typeName", this.terms["manage.registry.checklists.type"], 100);
        this.gridAg.addColumnText("defaultInOrderName", this.terms["manage.registry.checklists.showdefaultinorder"], 100);        
        this.gridAg.addColumnIcon(null, null, null, { icon: "fal fa-paperclip", showIcon: this.showIcon.bind(this), toolTip: this.terms["manage.registry.checklists.addattachementstoeinvoice"] });
        this.gridAg.addColumnEdit(this.terms["core.edit"], this.edit.bind(this))
        
        this.gridAg.finalizeInitGrid("manage.registry.checklists.checklists", true, undefined,true);
    }    

    protected showIcon(row) {        
        if (row.addAttachementsToEInvoice === true)
            return true;
        else
            return false;    
    }
    
}