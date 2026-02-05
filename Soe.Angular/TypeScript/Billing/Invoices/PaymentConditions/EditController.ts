import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2";
import { ICompositionEditController } from "../../../Core/ICompositionEditController";
import { IInvoiceService } from "../../../Shared/Billing/Invoices/InvoiceService";
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
    paymentCondition: any;
    paymentConditionId: number;

    //@ngInject
    constructor(
        private invoiceService: IInvoiceService,
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
        this.paymentConditionId = parameters.id;
        this.guid = parameters.guid;
        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);

        this.flowHandler.start([{ feature: Feature.Billing_Preferences_PayCondition_Edit, loadReadPermissions: true, loadModifyPermissions: true }]);
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Billing_Preferences_PayCondition_Edit].readPermission;
        this.modifyPermission = response[Feature.Billing_Preferences_PayCondition_Edit].modifyPermission;
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultEditToolbar(true, null, () => this.isNew);
    }

    private onLoadData(): ng.IPromise<any> {
        if (this.paymentConditionId > 0) {
            return this.invoiceService.getPaymentCondition(this.paymentConditionId).then((x) => {
                this.isNew = false;
                this.paymentCondition = x;
            });
        }
        else {
            this.new();
        }
    }

    public save() {
        this.progress.startSaveProgress((completion) => {
            this.invoiceService.savePaymentCondition(this.paymentCondition).then((result) => {
                if (result.success) {
                    if (result.integerValue && result.integerValue > 0)
                        this.paymentConditionId = result.integerValue;

                    completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.paymentCondition);
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
            this.invoiceService.deletePaymentCondition(this.paymentCondition.paymentConditionId).then((result) => {
                if (result.success) {
                    completion.completed(this.paymentCondition);
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

    private new() {
        this.isNew = true;
        this.paymentConditionId = 0;
        this.paymentCondition = {};
    }

    // VALIDATION
    public showValidationError() {
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
            if (this.paymentCondition) {
                if (!this.paymentCondition.code) {
                    mandatoryFieldKeys.push("common.code");
                }
                if (!this.paymentCondition.name) {
                    mandatoryFieldKeys.push("common.name");
                }
                if (!this.paymentCondition.days) {
                    mandatoryFieldKeys.push("common.day");
                }
            }
        });
    }
}