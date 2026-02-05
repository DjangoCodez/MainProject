import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2";
import { ICompositionEditController } from "../../../Core/ICompositionEditController";
import { ICoreService } from "../../../Core/Services/CoreService";
import { ITimeService } from "../TimeService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IValidationSummaryHandlerFactory } from "../../../Core/Handlers/ValidationSummaryHandlerFactory";
import { TimeCodeMaterialDTO } from "../../../Common/Models/TimeCode";
import { Feature, SoeTimeCodeType, SoeEntityState } from "../../../Util/CommonEnumerations";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IDirtyHandlerFactory } from "../../../Core/Handlers/DirtyHandlerFactory";
import { Constants } from "../../../Util/Constants";
import { IFocusService } from "../../../Core/Services/focusservice";

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    // Data
    private timeCodeId: number;
    private timeCode: TimeCodeMaterialDTO;

    //@ngInject
    constructor(
        private $q: ng.IQService,
        private timeService: ITimeService,
        private coreService: ICoreService,
        private focusService: IFocusService,
        private translationService: ITranslationService,
        private urlHelperService: IUrlHelperService,
        progressHandlerFactory: IProgressHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        private controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        private dirtyHandlerFactory: IDirtyHandlerFactory) {
        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onLoadData(() => this.onLoadData())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));
    }

    public onInit(parameters: any) {
        this.timeCodeId = parameters.id;
        this.guid = parameters.guid;
        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);

        this.flowHandler.start([{ feature: Feature.Billing_Preferences_ProductSettings_MaterialCode_Edit, loadReadPermissions: true, loadModifyPermissions: true }]);
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Billing_Preferences_ProductSettings_MaterialCode_Edit].readPermission;
        this.modifyPermission = response[Feature.Billing_Preferences_ProductSettings_MaterialCode_Edit].modifyPermission;
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultEditToolbar(true, () => this.copy(), () => !this.timeCodeId);
    }

    private onLoadData(): ng.IPromise<any> {
        if (this.timeCodeId) {
            return this.load();
        } else {
            this.new();
        }
    }

    // SERVICE CALLS

    private load(): ng.IPromise<any> {
        return this.timeService.getTimeCode(SoeTimeCodeType.Material, this.timeCodeId, true).then(x => {
            this.isNew = false;
            this.timeCode = x;
            this.dirtyHandler.clean();
            this.messagingHandler.publishSetTabLabel(this.guid, this.timeCode.name);
        });
    }

    private new() {
        this.isNew = true;
        this.timeCodeId = 0;
        this.timeCode = new TimeCodeMaterialDTO();
        this.timeCode.type = SoeTimeCodeType.Material;
        this.timeCode.state = SoeEntityState.Active;
    }

    // ACTIONS

    protected copy() {
        super.copy();

        this.timeCodeId = this.timeCode.timeCodeId = 0;
        this.timeCode.code = undefined;
        this.timeCode.name = undefined;
        _.forEach(this.timeCode.invoiceProducts, p => {
            p.timeCodeInvoiceProductId = 0;
            p.timeCodeId = 0;
        });
        this.focusService.focusByName("ctrl_timeCode_code");
    }

    private initSave() {
        if (this.validateSave())
            this.save();
    }

    private save() {
        this.progress.startSaveProgress((completion) => {
            this.timeService.saveTimeCode(this.timeCode).then(result => {
                if (result.success) {
                    this.timeCodeId = result.integerValue;
                    this.timeCode.timeCodeId = this.timeCodeId;
                    completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.timeCode);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }, this.guid).then(data => {
            this.onLoadData();
        }, error => {
        });
    }

    private delete() {
        this.progress.startDeleteProgress((completion) => {
            this.timeService.deleteTimeCode(this.timeCode.timeCodeId).then(result => {
                if (result.success) {
                    completion.completed(this.timeCode, true);
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

    private setDirty() {
        this.dirtyHandler.setDirty();
    }

    // VALIDATION

    private validateSave(): boolean {
        return true;
    }

    public showValidationError() {
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
            if (this.timeCode) {
                if (!this.timeCode.code)
                    mandatoryFieldKeys.push("common.code");

                if (!this.timeCode.name)
                    mandatoryFieldKeys.push("common.name");
            }
        });
    }
}