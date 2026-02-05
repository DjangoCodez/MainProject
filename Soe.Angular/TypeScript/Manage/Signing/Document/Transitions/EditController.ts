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
import { Constants } from "../../../../Util/Constants";
import { IAttestTransitionDTO } from "../../../../Scripts/TypeLite.Net4";
import { AttestStateDTO } from "../../../../Common/Models/AttestStateDTO";

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    // Data       
    signingTransition: IAttestTransitionDTO;
    attestEntities: any;    
    currentAttestEntity: TermGroup_AttestEntity = TermGroup_AttestEntity.SigningDocument;    
    signingStates: AttestStateDTO[];    
    signingTransitionId: number = 0;
    
    public parameters: any;

    //@ngInject
    constructor(
        private $q: ng.IQService,
        private $timeout: ng.ITimeoutService,
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
        this.signingTransitionId = parameters.id;
        this.guid = parameters.guid;

        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);

        this.flowHandler.start([{ feature: Feature.Manage_Signing_Document_Transitions, loadReadPermissions: true, loadModifyPermissions: true }]);
    }


    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Manage_Signing_Document_Transitions].readPermission;
        this.modifyPermission = response[Feature.Manage_Signing_Document_Transitions].modifyPermission;
    }

    // LOOKUPS
    private onDoLookups() {
        return this.progress.startLoadingProgress([            
            () => this.loadAttestEntities(),         
            () => this.loadAttestStates(),
        ]).then(x => {
            if (this.signingTransitionId > 0) {
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

    public loadAttestStates() {
        return this.signingService.getAttestStates(this.currentAttestEntity, SoeModule.Manage, false).then((x) => {
            this.signingStates = x;
        });
    }

    private load(): ng.IPromise<any> {
        var deferral = this.$q.defer();
        if (this.signingTransitionId > 0) {
            this.signingService.getAttestTransition(this.signingTransitionId)
                .then((x) => {
                    this.signingTransition = x;
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

            this.signingService.saveAttestTransition(this.signingTransition).then((result) => {
                if (result.success) {
                    if (!this.signingTransitionId) {
                        this.signingTransitionId = result.integerValue;
                    }
                    completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.signingTransition);
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
            this.signingService.deleteAttestTransition(this.signingTransitionId).then((result) => {
                if (result.success) {
                    completion.completed(this.signingTransition);
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
        this.signingTransitionId = 0;
        this.signingTransition = ({} as IAttestTransitionDTO);        
        this.signingTransition.module = SoeModule.Manage;
    }

    public suggestTransitionName() {
        this.$timeout(() => {

            if (!this.signingTransition.attestStateFromId || !this.signingTransition.attestStateToId)
                return;

            let stateFrom = _.find(this.signingStates, { attestStateId: this.signingTransition.attestStateFromId });
            if (!stateFrom)
                return;

            let stateTo = _.find(this.signingStates, { attestStateId: this.signingTransition.attestStateToId });
            if (!stateTo)
                return;

            this.signingTransition.name = stateFrom.name + " - " + stateTo.name;
        });
    }

    // VALIDATION
    public showValidationError() {
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
            if (this.signingTransition) {
                if (!this.signingTransition.name) {
                    mandatoryFieldKeys.push("common.name");
                }                
            }
        });
    }
   
}
