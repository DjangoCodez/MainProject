import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { IAccountingService } from "../../../Shared/Economy/Accounting/AccountingService";
import { ICompositionEditController } from "../../../Core/ICompositionEditController";
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

    private vatCodeId: number;
    vatCode: any;

    // Lookups 
    accountStdsDict: any = [];

    // Properties
    private _selectedAccount: any;
    get selectedAccount() {
        return this._selectedAccount;
    }
    set selectedAccount(item: any) {
        this._selectedAccount = item;

        if (this.vatCode) {
            var id = 0;
            if (item)
                id = item.id;
            this.vatCode.accountId = id;
            this.loadAccountSysVatRate();
        }
    }

    private _selectedPurchaseVATAccount: any;
    get selectedPurchaseVATAccount() {
        return this._selectedPurchaseVATAccount;
    }
    set selectedPurchaseVATAccount(item: any) {
        this._selectedPurchaseVATAccount = item;

        if (this.vatCode) {
            var id = 0;
            if (item)
                id = item.id;
            this.vatCode.purchaseVATAccountId = id;
            this.loadPurchaseVATAccountSysVatRate();
        }
    }

    get purchaseVATAccountId() {
        if (this.vatCode && this.vatCode.purchaseVATAccountId) {
            return this.vatCode.purchaseVATAccountId;
        } else {
            return 0;
        }
    }
    set purchaseVATAccountId(value: number) {
        if (!this.vatCode)
            return;

        this.vatCode.purchaseVATAccountId = value;
        this.loadPurchaseVATAccountSysVatRate();
    }

    //@ngInject
    constructor(
        private $q: ng.IQService,
        private accountingService: IAccountingService,
        urlHelperService: IUrlHelperService,
        progressHandlerFactory: IProgressHandlerFactory,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        private dirtyHandlerFactory: IDirtyHandlerFactory) {
        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onLoadData(() => this.onLoadData()) //this.doLookups())
            .onDoLookUp(() => this.onDoLookups()) //this.doLookups())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));
    }


    public onInit(parameters: any) {
        this.vatCodeId = parameters.id;
        this.guid = parameters.guid;
        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);

        this.flowHandler.start([{ feature: Feature.Economy_Preferences_VoucherSettings_VatCodes_Edit, loadReadPermissions: true, loadModifyPermissions: true }]);
    }

    public save() {
        this.progress.startSaveProgress((completion) => {
            this.accountingService.saveVatCode(this.vatCode).then((result) => {
                if (result.success) {
                    if (result.integerValue && result.integerValue > 0)
                        this.vatCodeId = result.integerValue;

                    completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.vatCode);
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
            this.accountingService.deleteVatCode(this.vatCode.vatCodeId).then((result) => {
                if (result.success) {
                    completion.completed(this.vatCode);
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

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Economy_Preferences_VoucherSettings_VatCodes_Edit].readPermission;
        this.modifyPermission = response[Feature.Economy_Preferences_VoucherSettings_VatCodes_Edit].modifyPermission;
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultEditToolbar(true, null, () => this.isNew);
    }
    private onDoLookups() {
        return this.loadAccountStds();
    }

    private onLoadData(): ng.IPromise<any> {
        if (this.vatCodeId > 0) {
            return this.accountingService.getVatCode(this.vatCodeId).then((x) => {
                this.isNew = false;
                this.vatCode = x;
                if (this.vatCode) {
                    this.selectedAccount = _.find(this.accountStdsDict, { id: this.vatCode.accountId });
                    this.selectedPurchaseVATAccount = _.find(this.accountStdsDict, { id: this.vatCode.purchaseVATAccountId });
                }
            });
        }
        else {
            this.new();
        }
    }

    public showValidationError() {
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
            if (this.vatCode) {
                if (!this.vatCode.code) {
                    mandatoryFieldKeys.push("common.code");
                }
                if (!this.vatCode.percentage) {
                    mandatoryFieldKeys.push("common.percentage");
                }
                if (!this.vatCode.accountId) {
                    mandatoryFieldKeys.push("economy.accounting.vatcode.account");
                }
                if (!this.vatCode.name) {
                    mandatoryFieldKeys.push("common.name")
                }
            }
        });
    }

    private loadAccountStds(): ng.IPromise<any> {
        return this.accountingService.getAccountStdsDict(false).then(x => {
            this.accountStdsDict = x;
        });
    }

    private loadAccountStdsDict() {
        this.accountingService.getAccountStdsDict(false).then((x) => {
            this.accountStdsDict = x;
        });
    }
    private loadAccountSysVatRate() {
        this.accountingService.getAccountSysVatRate(this.vatCode.accountId).then((x) => {
            this.vatCode.accountSysVatRate = x + " %";
        });
    }

    private loadPurchaseVATAccountSysVatRate() {
        this.accountingService.getAccountSysVatRate(this.vatCode.purchaseVATAccountId).then((x) => {
            this.vatCode.purchaseVATAccountSysVatRate = x + " %";
        });
    }
    private new() {
        this.isNew = true;
        this.vatCodeId = 0;
        this.vatCode = {};
    }
}