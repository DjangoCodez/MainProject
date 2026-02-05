import { EditControllerBase2 } from "../../../../Core/Controllers/EditControllerBase2";
import { ICompositionEditController } from "../../../../Core/ICompositionEditController";
import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { INotificationService } from "../../../../Core/Services/NotificationService";
import { IUrlHelperService } from "../../../../Core/Services/UrlHelperService";
import { IProgressHandlerFactory } from "../../../../Core/Handlers/ProgressHandlerFactory";
import { IControllerFlowHandlerFactory } from "../../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IMessagingHandlerFactory } from "../../../../Core/Handlers/MessagingHandlerFactory";
import { IValidationSummaryHandlerFactory } from "../../../../Core/Handlers/ValidationSummaryHandlerFactory";
import { IPermissionRetrievalResponse } from "../../../../Core/Handlers/ControllerFlowHandler";
import { IDirtyHandlerFactory } from "../../../../Core/Handlers/DirtyHandlerFactory";
import { ICoreService } from "../../../../Core/Services/CoreService";
import { ISigningService } from "../../SigningService";
import { Feature, SoeModule, } from "../../../../Util/CommonEnumerations";
import { Constants } from "../../../../Util/Constants";
import { IAttestRoleDTO } from "../../../../Scripts/TypeLite.Net4";

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    // Data       
    signingRole: IAttestRoleDTO;    
    signingRoleId: number = 0;
    
    public parameters: any;

    //@ngInject
    constructor(
        private $q: ng.IQService,
        private $scope: ng.IScope,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        urlHelperService: IUrlHelperService,
        progressHandlerFactory: IProgressHandlerFactory,
        private controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        private dirtyHandlerFactory: IDirtyHandlerFactory,        
        private coreService: ICoreService,
        private signingService: ISigningService        
        ) {

        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onDoLookUp(() => this.onDoLookups())                                   
    }

    public onInit(parameters: any) {
        this.signingRoleId = parameters.id;
        this.guid = parameters.guid;

        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);

        this.flowHandler.start([{ feature: Feature.Manage_Signing_Document_Roles, loadReadPermissions: true, loadModifyPermissions: true }]);
    }


    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Manage_Signing_Document_Roles].readPermission;
        this.modifyPermission = response[Feature.Manage_Signing_Document_Roles].modifyPermission;
    }

    // LOOKUPS
    private onDoLookups() {
        return this.progress.startLoadingProgress([
            () => this.load(),            
        ]);       
    }   

    // SERVICE CALLS

    private load(): ng.IPromise<any> {        
        var deferral = this.$q.defer();
        if (this.signingRoleId > 0) {
            this.signingService.getAttestRole(this.signingRoleId)
                .then((x) => {
                    this.signingRole = x;
                    this.isNew = false;
                    deferral.resolve();
                });
        } else {
            this.new();
            deferral.resolve();
        }

        return deferral.promise;
    }

    // ACTIONS

    private save() {
        this.progress.startSaveProgress((completion) => {

            this.signingService.saveAttestRole(this.signingRole).then((result) => {
                if (result.success) {
                    if (!this.signingRoleId) {
                        this.signingRoleId = result.integerValue;
                    }
                    completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.signingRole);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }, this.guid)
            .then(data => {
                this.dirtyHandler.clean();
                this.load();
            }, error => {

            });
    }

    protected delete() {
        this.progress.startDeleteProgress((completion) => {
            this.signingService.deleteAttestRole(this.signingRoleId).then((result) => {
                if (result.success) {
                    completion.completed(this.signingRole);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }).then(x => {
            this.closeMe(true);
        });
    }

    // HELP-METHODS   
    private new() {        
        this.isNew = true;
        this.signingRoleId = 0;
        this.signingRole = ({} as IAttestRoleDTO);       
        this.signingRole.module = SoeModule.Manage;
    }


    private setDirty(force: boolean = false) {        
        if (force) {
            this.$scope.$applyAsync(() => {
                this['edit'].$pristine = false;
                this['edit'].$dirty = true;
            });
        }
        this.dirtyHandler.setDirty();
    }
    // VALIDATION
    public showValidationError() {
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
            if (this.signingRole) {
                if (!this.signingRole.name) {
                    mandatoryFieldKeys.push("common.name");
                }                
            }
        });
    }
   
}
