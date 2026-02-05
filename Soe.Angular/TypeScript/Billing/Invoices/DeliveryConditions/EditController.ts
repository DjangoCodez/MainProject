import { ICompositionEditController } from "../../../Core/ICompositionEditController";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { IValidationSummaryHandlerFactory } from "../../../Core/Handlers/ValidationSummaryHandlerFactory";
import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2";
import { IInvoiceService } from "../../../Shared/Billing/Invoices/InvoiceService";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IDirtyHandlerFactory } from "../../../Core/Handlers/DirtyHandlerFactory";
import { Feature } from "../../../Util/CommonEnumerations";
import { Constants } from "../../../Util/Constants";

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    private deliveryConditionId: number;
    deliveryCondition: any;

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
            .onDoLookUp(() => this.doLookups())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));
    }

    public onInit(parameters: any) {
        this.deliveryConditionId = parameters.id;
        this.guid = parameters.guid;

        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);

        this.flowHandler.start([{ feature: Feature.Billing_Preferences_DeliveryCondition_Edit, loadReadPermissions: true, loadModifyPermissions: true }]);
    }

    public save() {
        this.progress.startSaveProgress((completion) => {
            this.invoiceService.saveDeliveryCondition(this.deliveryCondition).then((result) => {
                if (result.success) {
                    if (result.integerValue && result.integerValue > 0)
                        this.deliveryConditionId = result.integerValue;

                    completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.deliveryCondition);
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
            this.invoiceService.deleteDeliveryCondition(this.deliveryCondition.deliveryConditionId).then((result) => {
                if (result.success) {
                    completion.completed(this.deliveryCondition);
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

    public showValidationError() {
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
            if (this.deliveryCondition) {
                if (!this.deliveryCondition.code) {
                    mandatoryFieldKeys.push("common.code");
                }
                if (!this.deliveryCondition.name) {
                    mandatoryFieldKeys.push("common.name");
                }
            }
        });
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Billing_Preferences_DeliveryCondition_Edit].readPermission;
        this.modifyPermission = response[Feature.Billing_Preferences_DeliveryCondition_Edit].modifyPermission;
    }

    private doLookups() {
        if (this.deliveryConditionId > 0) {
            return this.progress.startLoadingProgress([() => this.loadData()])
        } else {
            this.isNew = true;
            this.deliveryConditionId = 0;
            this.deliveryCondition = {};
        }
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultEditToolbar(true, null, () => this.isNew);
    }

    private loadData(): ng.IPromise<any> {
        return this.invoiceService.getDeliveryCondition(this.deliveryConditionId).then((x) => {
            this.isNew = false;
            this.deliveryCondition = x;
        });
    }
}