import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ICompositionEditController } from "../../../Core/ICompositionEditController";
import { ISupplierService } from "../../../Shared/Economy/Supplier/SupplierService";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IValidationSummaryHandlerFactory } from "../../../Core/Handlers/ValidationSummaryHandlerFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IDirtyHandlerFactory } from "../../../Core/Handlers/DirtyHandlerFactory";
import { CoreUtility } from "../../../Util/CoreUtility";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { TermGroup_Languages, TermGroup_SysPaymentMethod, Feature, SoeOriginType } from "../../../Util/CommonEnumerations";
import { Constants } from "../../../Util/Constants";

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    // Data
    private paymentMethodId: number;
    paymentMethod: any;
    account: any;

    // Lookups 
    sysPaymentMethods: any;
    paymentInformation: any;
    accountStdsDict: any = [];

    //Hide & show
    showBankId = false;
    showBic = false;

    // Flags
    loading = false;

    // Properties
    private _selectedPaymentInformation: any;
    get selectedPaymentInformation(): any {
        return this._selectedPaymentInformation;
    }
    set selectedPaymentInformation(item: any) {
        this._selectedPaymentInformation = item;
        if (!this.loading) {
            if (this._selectedPaymentInformation)
                this.paymentMethod.paymentInformationRowId = item.id;
            else
                this.paymentMethod.paymentInformationRowId = undefined;
        }
    }

    //@ngInject
    constructor(
        private $q: ng.IQService,
        private supplierService: ISupplierService,
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
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));
    }

    public onInit(parameters: any) {
        this.paymentMethodId = parameters.id;
        this.guid = parameters.guid;
        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);        
        this.flowHandler.start([{ feature: Feature.Economy_Preferences_SuppInvoiceSettings_PaymentMethods_Edit, loadReadPermissions: true, loadModifyPermissions: true }]);
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Economy_Preferences_SuppInvoiceSettings_PaymentMethods_Edit].readPermission;
        this.modifyPermission = response[Feature.Economy_Preferences_SuppInvoiceSettings_PaymentMethods_Edit].modifyPermission;
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultEditToolbar(true, () => this.copy(), () => this.isNew);
    }

    protected onDoLookups() {
        return this.$q.all([this.loadSysPaymentTypes(),
        this.loadPaymentInformation(),
        this.loadAccountStdsDict()]
        )
    }

    // LOOKUPS
    private onLoadData(): ng.IPromise<any> {
        if (this.paymentMethodId > 0) {
            this.loading = true;
            return this.supplierService.getPaymentMethod(this.paymentMethodId, true, true).then((x) => {
                this.paymentMethod = x;
                this.isNew = false;
                this.selectedPaymentInformation = _.find(this.paymentInformation, (p) => p.id == this.paymentMethod.paymentInformationRowId);
                this.updateShowFields(this.paymentMethod.sysPaymentMethodId);

                this.loading = false;
            });
        }
        else {
            this.new();
        }
    }

    private loadAccountStdsDict(): ng.IPromise<any> {
        return this.supplierService.getAccountStdsDict(false).then((x) => {
            this.accountStdsDict = x;
        });

    }
    private loadSysPaymentTypes(): ng.IPromise<any> {
        return this.supplierService.getSysPaymentMethodsDict(SoeOriginType.SupplierPayment, true).then((x) => {
            this.sysPaymentMethods = x;
        });

    }
    private loadPaymentInformation(): ng.IPromise<any> {
        return this.supplierService.getPaymentInformationViewsSmall(true).then((x) => {
            this.paymentInformation = x;            
        });
    }

    // ACTIONS

    private updateShowFields(sysPaymentMethod: any) {
        this.showBankId = (sysPaymentMethod == TermGroup_SysPaymentMethod.NordeaCA || CoreUtility.sysCountryId == TermGroup_Languages.Finnish || sysPaymentMethod == TermGroup_SysPaymentMethod.ISO20022);
        this.showBic = (sysPaymentMethod == TermGroup_SysPaymentMethod.ISO20022);
    }

    
    private exportTypeChanged(item: any) {        
        this.updateShowFields(item);
    }

    public save() {
        this.progress.startSaveProgress((completion) => {
            this.supplierService.savePaymentMethod(this.paymentMethod).then((result) => {
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
            this.supplierService.deletePaymentMethod(this.paymentMethod.paymentMethodId).then((result) => {
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

    public copy() {
        this.isNew = true;
        this.paymentMethodId = this.paymentMethod.paymentMethodId = 0;
        this.dirtyHandler.setDirty();
    }

    private new() {
        this.isNew = true;
        this.paymentMethodId = 0;
        this.paymentMethod = {
            paymentType: <number>SoeOriginType.SupplierPayment,
            paymentMethodId : 0,
        };
    }

    // VALIDATION
    public showValidationError() {
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
            if (this.paymentMethod) {
                if (!this.paymentMethod.name) {
                    mandatoryFieldKeys.push("common.name");
                }
                if (!this.paymentMethod.sysPaymentMethodId) {
                    mandatoryFieldKeys.push("economy.common.paymentmethods.exporttype");
                }
                if (!this.paymentMethod.paymentInformationRowId) {
                    mandatoryFieldKeys.push("economy.common.paymentmethods.paymentnr");
                }
                if (!this.paymentMethod.accountId) {
                    mandatoryFieldKeys.push("economy.common.paymentmethods.accountnr");
                }
            }
        });
    }
}