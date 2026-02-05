import { ICompositionEditController } from "../../../Core/ICompositionEditController";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IDirtyHandlerFactory } from "../../../Core/Handlers/DirtyHandlerFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { IValidationSummaryHandlerFactory } from "../../../Core/Handlers/ValidationSummaryHandlerFactory";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IRegistryService } from "../RegistryService";
import { CoreUtility } from "../../../Util/CoreUtility";
import { MessageGroupDTO } from "../../../Common/Models/MessageDTOs";
import { Feature } from "../../../Util/CommonEnumerations";
import { Constants } from "../../../Util/Constants";
import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2";
import { ITranslationService } from "../../../Core/Services/TranslationService";

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    // Terms
    private terms: { [index: string]: string; };
    private noUserValidationInfoLabel: string;

    // Data
    private messageGroupId: number;
    private messageGroup: MessageGroupDTO;

    //@ngInject
    constructor(
        urlHelperService: IUrlHelperService,
        progressHandlerFactory: IProgressHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        private translationService: ITranslationService,
        private registryService: IRegistryService,
        private dirtyHandlerFactory: IDirtyHandlerFactory) {
        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);

        this.deleteButtonTemplateUrl = urlHelperService.getCoreComponent("deleteButtonComposition.html");
        this.saveButtonTemplateUrl = urlHelperService.getCoreComponent("saveButtonComposition.html");

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onDoLookUp(() => this.onDoLookups())
            .onLoadData(() => this.onLoadData())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));
    }

    // SETUP

    public onInit(parameters: any) {
        this.messageGroupId = parameters.id;
        this.guid = parameters.guid;

        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);

        this.flowHandler.start([{ feature: Feature.Manage_Preferences_Registry_EventReceiverGroups, loadReadPermissions: true, loadModifyPermissions: true }]);
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Manage_Preferences_Registry_EventReceiverGroups].readPermission;
        this.modifyPermission = response[Feature.Manage_Preferences_Registry_EventReceiverGroups].modifyPermission;
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultEditToolbar(false);
    }

    // SERVICE CALLS

    private onDoLookups(): ng.IPromise<any> {
        return this.loadTerms();
    }

    private onLoadData(): ng.IPromise<any> {
        if (this.messageGroupId) {
            return this.loadData();
        } else {
            this.new();
        }
    }

    private loadTerms(): ng.IPromise<any> {
        let keys: string[] = [
            "manage.registry.receivergroups.nouservalidation.info"
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
            this.noUserValidationInfoLabel = this.terms["manage.registry.receivergroups.nouservalidation.info"];
        });
    }

    private loadData() {
        return this.registryService.getMessageGroup(this.messageGroupId).then((x) => {
            this.messageGroup = x;
            this.isNew = false;
        });
    }

    // ACTIONS

    private new() {
        this.isNew = true;

        this.messageGroup = new MessageGroupDTO;
        this.messageGroup.messageGroupId = this.messageGroupId = 0;
        this.messageGroup.groupMembers = [];
        this.messageGroup.licenseId = CoreUtility.licenseId;
        this.messageGroup.actorCompanyId = CoreUtility.actorCompanyId;
        this.messageGroup.userId = CoreUtility.userId;
    }

    private save() {
        this.progress.startSaveProgress((completion) => {
            this.registryService.saveMessageGroup(this.messageGroup).then((result) => {
                if (result.success) {
                    if (result.integerValue && result.integerValue > 0)
                        this.messageGroupId = result.integerValue;

                    completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.messageGroup);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }, this.guid)
            .then(data => {
                this.dirtyHandler.clean();
                this.loadData();
            }, error => {

            });
    }

    public delete() {
        this.progress.startDeleteProgress((completion) => {
            this.registryService.deleteMessageGroup(this.messageGroupId).then((result) => {
                if (result.success) {
                    completion.completed(this.messageGroupId);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }).then(x => {
            this.closeMe(false);
        });
    }

    // EVENTS

    private setModified() {
        this.dirtyHandler.setDirty();
    }

    // VALIDATE

    public showValidationError() {
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
            if (this.messageGroup) {
                // Mandatory fields
                if (!this.messageGroup.name)
                    mandatoryFieldKeys.push("common.name");
            }
        });
    }
}
