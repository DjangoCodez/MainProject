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
import { Feature, SoeModule, TermGroup_AttestEntity } from "../../../../Util/CommonEnumerations";
import { AttestStateDTO } from "../../../../Common/Models/AttestStateDTO";
import { Constants } from "../../../../Util/Constants";

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    // Data       
    signingState: AttestStateDTO;
    attestEntities: any;    
    signingStateId: number = 0;
    
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
        this.signingStateId = parameters.id;
        this.guid = parameters.guid;

        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);

        this.flowHandler.start([{ feature: Feature.Manage_Signing_Document_States, loadReadPermissions: true, loadModifyPermissions: true }]);
    }


    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Manage_Signing_Document_States].readPermission;
        this.modifyPermission = response[Feature.Manage_Signing_Document_States].modifyPermission;
    }

    // LOOKUPS
    private onDoLookups() {
        return this.progress.startLoadingProgress([            
            () => this.loadAttestEntities(),            
        ]).then(x => {
            if (this.signingStateId > 0) {
                this.load();
            } else {
                this.new();
            }
        });
    }   

    // SERVICE CALLS
    
    public loadAttestEntities(): ng.IPromise<any> {
        return this.signingService.getAttestEntitiesGenericList(false, true, SoeModule.Manage).then((x) => {            
            this.attestEntities = _.filter(x, (ae) => ae.id == TermGroup_AttestEntity.SigningDocument);            
        });
    }

    private load(): ng.IPromise<any> {
        var deferral = this.$q.defer();
        if (this.signingStateId > 0) {
            this.signingService.getAttestState(this.signingStateId)
                .then((x) => {
                    this.signingState = x;
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

            this.signingService.saveAttestState(this.signingState).then((result) => {
                if (result.success) {
                    if (!this.signingStateId) {
                        this.signingStateId = result.integerValue;
                    }
                    completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.signingState);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }, this.guid)
            .then(data => {
                this.dirtyHandler.clean();
            }, error => {

            });
    }

    protected delete() {
        this.progress.startDeleteProgress((completion) => {
            this.signingService.deleteAttestState(this.signingStateId).then((result) => {
                if (result.success) {
                    completion.completed(this.signingState);
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
        this.signingStateId = 0;
        this.signingState = ({} as AttestStateDTO);
        this.signingState.entity = TermGroup_AttestEntity.SigningDocument;
        this.signingState.module = SoeModule.Manage;
    }

    // VALIDATION
    public showValidationError() {
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
            if (this.signingState) {
                if (!this.signingState.name) {
                    mandatoryFieldKeys.push("common.name");
                }                
            }
        });
    }
   
}
