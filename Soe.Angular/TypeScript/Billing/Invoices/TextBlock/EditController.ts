import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ICoreService } from "../../../Core/Services/CoreService";
import { ICompositionEditController } from "../../../Core/ICompositionEditController";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IValidationSummaryHandlerFactory } from "../../../Core/Handlers/ValidationSummaryHandlerFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IDirtyHandlerFactory } from "../../../Core/Handlers/DirtyHandlerFactory";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { Feature, TermGroup, CompTermsRecordType, SoeEntityType } from "../../../Util/CommonEnumerations";
import { Constants } from "../../../Util/Constants";
import { TextBlockDTO } from "../../../Common/Models/InvoiceDTO";
import { CompTermDTO } from "../../../Common/Models/CompTermDTO";

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    // Data
    textBlockId: number;
    textBlock: TextBlockDTO;
    languages: any[];
    textblockTypes: any[];

    // Translations
    compTermRecordType: number = CompTermsRecordType.Textblock;
    compTermRows: CompTermDTO[];

    //@ngInject
    constructor(
        private $q: ng.IQService,                
        urlHelperService: IUrlHelperService,
        private coreService: ICoreService,
        progressHandlerFactory: IProgressHandlerFactory,
        private controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        private dirtyHandlerFactory: IDirtyHandlerFactory) {
        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onDoLookUp(() => this.onDoLookups())
            .onLoadData(() => this.onLoadData())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));
    }

    public onInit(parameters: any) {        
        this.textBlockId = parameters.id;
        if (!this.textBlockId)
            this.textBlockId = 0;
        this.guid = parameters.guid;
        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);

        this.flowHandler.start([{ feature: Feature.Billing_Preferences_Textblock_Edit, loadReadPermissions: true, loadModifyPermissions: true }]);
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Billing_Preferences_Textblock_Edit].readPermission;
        this.modifyPermission = response[Feature.Billing_Preferences_Textblock_Edit].modifyPermission;        
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultEditToolbar(true, null, () => this.isNew);
    }

    // LOOKUPS
    private onDoLookups() {
        return this.$q.all([
            this.loadLanguages(), 
            this.loadTextBlockTypes(),
        ]);            
    }

    private loadLanguages(): ng.IPromise<any> {
        this.languages = [];
        return this.coreService.getTermGroupContent(TermGroup.Language, false, false).then((x) => {
            this.languages = x;
        });
    }    

    private loadTextBlockTypes(): ng.IPromise<any> {
        this.textblockTypes = [];
        return this.coreService.getTermGroupContent(TermGroup.TextBlockType, false, true).then((x) => {            
            this.textblockTypes = x;
        });
    }    

    private onLoadData(): ng.IPromise<any> {        
        if (this.textBlockId > 0) {           
            return this.coreService.getTextBlock(this.textBlockId).then((x) => {
                this.isNew = false;
                this.textBlock = x;
            });
        }
        else {
            this.new();
        }
    }

    //EVENTS
   

    public save() {
        this.progress.startSaveProgress((completion) => {

            this.coreService.saveTextBlock(this.textBlock, SoeEntityType.CustomerInvoice, this.compTermRows).then((result) => {
                if (result.success) {
                    if (result.integerValue && result.integerValue > 0)
                        this.textBlockId = result.integerValue;                    
                    completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.textBlock);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }, this.guid)
            .then(data => {                                
                this.dirtyHandler.clean();
                this.messagingHandler.publishReloadGrid(this.guid);
                this.onLoadData();
            }, error => {

            });
    }

    public delete() {
        this.progress.startDeleteProgress((completion) => {
            this.coreService.deleteTextBlock(this.textBlockId).then((result) => {
                if (result.success) {
                    completion.completed(this.textBlock);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }).then(x => {
            super.closeMe(true);
        });
    }

    // HELP-METHODS
    private new() {
        this.isNew = true;
        this.textBlockId = 0;        
        this.textBlock = new TextBlockDTO();        
    }

    // VALIDATION
    public showValidationError() {
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
            if (this.textBlock) {
                if (!this.textBlock.type) {
                    mandatoryFieldKeys.push("common.type");
                }
                if (!this.textBlock.headline) {
                    mandatoryFieldKeys.push("common.name");
                }
            }
        });
    }
}
