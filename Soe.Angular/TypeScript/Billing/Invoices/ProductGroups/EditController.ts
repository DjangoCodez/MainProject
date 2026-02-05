import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ICompositionEditController } from "../../../Core/ICompositionEditController";
import { InvoiceService } from "../../../Shared/Billing/Invoices/InvoiceService";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IValidationSummaryHandlerFactory } from "../../../Core/Handlers/ValidationSummaryHandlerFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IDirtyHandlerFactory } from "../../../Core/Handlers/DirtyHandlerFactory";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { Feature } from "../../../Util/CommonEnumerations";
import { Constants } from "../../../Util/Constants";

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    // Data
    productGroupId: number;
    productGroup: any;

    //@ngInject
    constructor(
        private invoiceService: InvoiceService,
        private translationService: ITranslationService,
        urlHelperService: IUrlHelperService,
        progressHandlerFactory: IProgressHandlerFactory,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        private dirtyHandlerFactory: IDirtyHandlerFactory) {
        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onLoadData(() => this.onLoadData())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));
    }

    public onInit(parameters: any) {
        this.productGroupId = parameters.id;
        this.guid = parameters.guid;
        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);

        this.flowHandler.start([{ feature: Feature.Billing_Preferences_ProductSettings_ProductGroup_Edit, loadReadPermissions: true, loadModifyPermissions: true }]);
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Billing_Preferences_ProductSettings_ProductGroup_Edit].readPermission;
        this.modifyPermission = response[Feature.Billing_Preferences_ProductSettings_ProductGroup_Edit].modifyPermission;
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultEditToolbar(true, null, () => this.isNew);
    }

    // LOOKUPS
    private onLoadData(): ng.IPromise<any> {
        if (this.productGroupId > 0) {
            return this.invoiceService.getProductGroup(this.productGroupId).then((x) => {
                this.isNew = false;
                this.productGroup = x;
            });
        }
        else {
            this.new();
        }
    }

    public save() {
        this.progress.startSaveProgress((completion) => {
            this.invoiceService.saveProductGroup(this.productGroup).then((result) => {
                if (result.success) {
                    if (result.integerValue && result.integerValue > 0)
                        this.productGroupId = result.integerValue;

                    this.updateTabCaption();

                    completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.productGroup);
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
            this.invoiceService.deleteProductGroup(this.productGroup.productGroupId).then((result) => {
                if (result.success) {
                    completion.completed(this.productGroup);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }).then(x => {
            super.closeMe(false);
        });
    }

    // HELP-METHODS
    private new() {
        this.isNew = true;
        this.productGroupId = 0;
        this.productGroup = {};
    }

    private updateTabCaption() {
        this.translationService.translate("billing.invoices.productgroups.productgroup").then((term) => {
            this.messagingHandler.publishSetTabLabel(this.guid, term + " " + this.productGroup.code);
        });
    }

    // VALIDATION
    public showValidationError() {
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
            if (this.productGroup) {
                if (!this.productGroup.code) {
                    mandatoryFieldKeys.push("common.code");
                }
                if (!this.productGroup.name) {
                    mandatoryFieldKeys.push("common.name");
                }
            }
        });
    }
}
