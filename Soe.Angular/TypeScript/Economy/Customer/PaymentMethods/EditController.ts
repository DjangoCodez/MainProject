import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ICompositionEditController } from "../../../Core/ICompositionEditController";
import { ICommonCustomerService } from "../../../Common/Customer/CommonCustomerService";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IValidationSummaryHandlerFactory } from "../../../Core/Handlers/ValidationSummaryHandlerFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IDirtyHandlerFactory } from "../../../Core/Handlers/DirtyHandlerFactory";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { Feature, SoeOriginType } from "../../../Util/CommonEnumerations";
import { Constants } from "../../../Util/Constants";

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    // Data
    private paymentMethodId: number;
    paymentMethod: any;

    // Lookups 
    sysPaymentMethods: any;
    paymentInformation: any;
    accountStdsDict: any = [];

    //@ngInject
    constructor(
        private $q: ng.IQService,
        private commonCustomerService: ICommonCustomerService,
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
            .onDoLookUp(() => this.onDoLookups())
    }

    // SETUP
    public onInit(parameters: any) {

        this.paymentMethodId = parameters.id;
        this.guid = parameters.guid;
        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);

        this.flowHandler.start([{ feature: Feature.Economy_Preferences_CustInvoiceSettings_PaymentMethods_Edit, loadReadPermissions: true, loadModifyPermissions: true }]);
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Economy_Preferences_CustInvoiceSettings_PaymentMethods_Edit].readPermission;
        this.modifyPermission = response[Feature.Economy_Preferences_CustInvoiceSettings_PaymentMethods_Edit].modifyPermission;
    }

    private onDoLookups() {
        return this.$q.all([this.loadPaymentInformation(),
        this.loadSysPaymentTypes(),
        this.loadAccountStdsDict()]);
    }

    // LOOKUPS
    private onLoadData(): ng.IPromise<any> {
        if (this.paymentMethodId > 0) {
            return this.commonCustomerService.getPaymentMethod(this.paymentMethodId, true, false).then((x) => {
                this.paymentMethod = x;
                this.isNew = false;
            });
        }
        else {
            this.new();
        }
    }

    private loadAccountStdsDict(): ng.IPromise<any> {
        return this.commonCustomerService.getAccountStdsDict(false).then((x) => {
            this.accountStdsDict = x;

        });

    }

    private loadSysPaymentTypes(): ng.IPromise<any> {
        return this.commonCustomerService.getSysPaymentMethodsDict(SoeOriginType.CustomerPayment, true).then((x) => {
            this.sysPaymentMethods = x;
            //this.lookupLoaded();
        });

    }
    private loadPaymentInformation(): ng.IPromise<any> {
        return this.commonCustomerService.getPaymentInformationViewsDict(true).then((x) => {
            this.paymentInformation = x;
        });
    }

    // ACTIONS
    public save() {
        this.progress.startSaveProgress((completion) => {
            this.commonCustomerService.savePaymentMethod(this.paymentMethod).then((result) => {
                if (result.success) {
                    if (result.integerValue && result.integerValue > 0)
                        this.paymentMethodId = result.integerValue;

                    completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.paymentMethod);
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
            this.commonCustomerService.deletePaymentMethod(this.paymentMethod.paymentMethodId).then((result) => {
                if (result.success) {
                    completion.completed(this.paymentMethod);
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
        this.paymentMethodId = 0;
        this.paymentMethod = {
            paymentType: <number>SoeOriginType.CustomerPayment,
        };
    }

    // VALIDATION
    public showValidationError() {
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
            if (this.paymentMethod) {
                if (!this.paymentMethod.name) {
                    mandatoryFieldKeys.push("common.name");
                }
                if (!this.paymentMethod.accountStd || !this.paymentMethod.accountStd.account || !this.paymentMethod.accountStd.account.accountNr) {
                    mandatoryFieldKeys.push("economy.common.paymentmethods.accountnr");
                }
            }
        });
    }
}