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
import { Feature, TermGroup_AttestEntity } from "../../../../Util/CommonEnumerations";
import { Constants } from "../../../../Util/Constants";
import { AttestWorkFlowTemplateHeadDTO, AttestWorkFlowTemplateRowDTO } from "../../../../Common/Models/AttestWorkFlowDTOs";

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    // Data       
    attestWorkFlowTemplateHead: AttestWorkFlowTemplateHeadDTO;
    attestWorkFlowTemplateHeadId: number = 0;
    attestWorkFlowTemplateRows: AttestWorkFlowTemplateRowDTO[]
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
        private signingService: ISigningService) {
        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onDoLookUp(() => this.onDoLookups())
    }

    // SETUP

    public onInit(parameters: any) {
        this.attestWorkFlowTemplateHeadId = parameters.id;
        this.guid = parameters.guid;

        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);

        this.flowHandler.start([{ feature: Feature.Manage_Signing_Document_Templates, loadReadPermissions: true, loadModifyPermissions: true }]);
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Manage_Signing_Document_Templates].readPermission;
        this.modifyPermission = response[Feature.Manage_Signing_Document_Templates].modifyPermission;
    }

    private onDoLookups() {
        return this.progress.startLoadingProgress([
            () => this.load(),
        ]);
    }

    // SERVICE CALLS

    private load(): ng.IPromise<any> {
        var deferral = this.$q.defer();
        if (this.attestWorkFlowTemplateHeadId > 0) {
            this.signingService.getAttestWorkFlowTemplate(this.attestWorkFlowTemplateHeadId)
                .then((x) => {
                    this.attestWorkFlowTemplateHead = x;
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
            this.signingService.saveAttestWorkFlowTemplate(this.attestWorkFlowTemplateHead).then(result => {
                if (result.success) {
                    if (!this.attestWorkFlowTemplateHeadId) {
                        this.attestWorkFlowTemplateHead.attestWorkFlowTemplateHeadId = this.attestWorkFlowTemplateHeadId = result.integerValue;
                    }

                    let rows = this.attestWorkFlowTemplateRows.filter(item => item.checked).map((x, index) => {
                        x.sort = index + 1;
                        return x;
                    });

                    this.signingService.saveAttestWorkFlowTemplateRows(rows, this.attestWorkFlowTemplateHeadId).then(resultRows => {
                        if (!resultRows.success)
                            completion.failed(resultRows.errorMessage);
                        else
                            completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.attestWorkFlowTemplateHead);
                    });
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
            this.signingService.deleteAttestWorkFlowTemplate(this.attestWorkFlowTemplateHeadId).then((result) => {
                if (result.success) {
                    completion.completed(this.attestWorkFlowTemplateHead, true);
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
        this.attestWorkFlowTemplateHeadId = 0;
        this.attestWorkFlowTemplateHead = new AttestWorkFlowTemplateHeadDTO;
        this.attestWorkFlowTemplateHead.attestEntity = TermGroup_AttestEntity.SigningDocument;
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
        var errors = this['edit'].$error;
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
            if (this.attestWorkFlowTemplateHead) {
                if (!this.attestWorkFlowTemplateHead.name)
                    mandatoryFieldKeys.push("common.name");

                if (errors['rowsSelected'])
                    validationErrorKeys.push("manage.attest.supplier.attestworkflowtemplate.notransitions");
            }
        });
    }
}
