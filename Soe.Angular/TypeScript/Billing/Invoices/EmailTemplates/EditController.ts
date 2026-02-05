import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService, IUrlHelperServiceProvider } from "../../../Core/Services/UrlHelperService";
import { ICoreService } from "../../../Core/Services/CoreService";
import { IFocusService } from "../../../Core/Services/FocusService";
import { ICompositionEditController } from "../../../Core/ICompositionEditController";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IValidationSummaryHandlerFactory } from "../../../Core/Handlers/ValidationSummaryHandlerFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IDirtyHandlerFactory } from "../../../Core/Handlers/DirtyHandlerFactory";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { CoreUtility } from "../../../Util/CoreUtility";
import { Feature, EmailTemplateType } from "../../../Util/CommonEnumerations";
import { Constants } from "../../../Util/Constants";
import { TinyMCEUtility } from "../../../Util/TinyMCEUtility";


export class EditController extends EditControllerBase2 implements ICompositionEditController {

    // Data
    emailTemplateId: number;
    emailTemplate: any;    
    private tinyMceOptions: any; 
    types: any[];

    //@ngInject
    constructor(
        private $q: ng.IQService,        
        private translationService: ITranslationService,
        urlHelperService: IUrlHelperService,
        private coreService: ICoreService,
        private focusService: IFocusService,
        progressHandlerFactory: IProgressHandlerFactory,
        private controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        private dirtyHandlerFactory: IDirtyHandlerFactory,) {
        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onDoLookUp(() => this.onDoLookups())
            .onLoadData(() => this.onLoadData())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));
    }

    public onInit(parameters: any) {        
        this.emailTemplateId = parameters.id;
        this.guid = parameters.guid;
        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);

        this.flowHandler.start([{ feature: Feature.Billing_Preferences_EmailTemplate_Edit, loadReadPermissions: true, loadModifyPermissions: true }]);
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Billing_Preferences_EmailTemplate_Edit].readPermission;
        this.modifyPermission = response[Feature.Billing_Preferences_EmailTemplate_Edit].modifyPermission;
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultEditToolbar(true, null, () => this.isNew);
    }

    // LOOKUPS
    private onDoLookups() {
        return this.$q.all([
            this.loadTypes(),
            this.setupTinyMCE()
        ]);            
    }

    private loadTypes(): ng.IPromise<any> {
        var keys: string[] = [
            "common.salestypes",
            "common.customer.invoices.reminder",
            "billing.purchase.list.purchase"
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.types = [];
            this.types.push({ id: EmailTemplateType.Invoice, name: terms["common.salestypes"] });
            this.types.push({ id: EmailTemplateType.Reminder, name: terms["common.customer.invoices.reminder"] });
            this.types.push({ id: EmailTemplateType.PurchaseOrder, name: terms["billing.purchase.list.purchase"] });
        });
    }

    private onLoadData(): ng.IPromise<any> {
        if (this.emailTemplateId > 0) {
            return this.coreService.getEmailTemplate(this.emailTemplateId).then((x) => {
                this.isNew = false;
                this.emailTemplate = x;
            });
        }
        else {
            this.new();
        }
    }

    private setupTinyMCE() {
        this.tinyMceOptions = TinyMCEUtility.setupDefaultOptions();
    }

    //EVENTS   

    public save() {
        this.progress.startSaveProgress((completion) => {

            this.coreService.saveEmailTemplate(this.emailTemplate).then((result) => {
                if (result.success) {
                    if (result.integerValue && result.integerValue > 0)
                        this.emailTemplateId = result.integerValue;

                    completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.emailTemplate);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }, this.guid)
            .then(data => {                                
                this.dirtyHandler.clean();
                this.onLoadData();
            }, error => {

            });
    }

    public delete() {
        this.progress.startDeleteProgress((completion) => {
            this.coreService.deleteEmailTemplate(this.emailTemplateId).then((result) => {
                if (result.success) {
                    completion.completed(this.emailTemplate);
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

    public copy() {        
        if (!this.emailTemplate)
            return;
        this.isNew = true;
        this.emailTemplateId = 0;
        this.emailTemplate.emailTemplateId = 0;
        this.emailTemplate.name = "";
        
        this.dirtyHandler.setDirty();
        this.focusService.focusByName("ctrl_emailTemplate_name");
        this.translationService.translate("billing.invoices.emailtemplate.new").then((term) => {
            this.messagingHandler.publishSetTabLabel(this.guid, term);
        });
    }

    // HELP-METHODS
    private new() {
        this.isNew = true;
        this.emailTemplateId = 0;  
        this.emailTemplate = {};
        this.emailTemplate.type = 0;
    }

    // VALIDATION
    public showValidationError() {
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
            if (this.emailTemplate) {                
                if (!this.emailTemplate.name) {
                    mandatoryFieldKeys.push("common.name");
                }
                if (!this.emailTemplate.subject) {
                    mandatoryFieldKeys.push("billing.invoices.emailtemplate.subject");
                }
                if (!this.emailTemplate.body) {
                    mandatoryFieldKeys.push("billing.invoices.emailtemplate.body");
                }
            }
        });
    }
}
