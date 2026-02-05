import { ITranslationService } from "../../../Core/Services/TranslationService";
import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ICompositionEditController } from "../../../Core/ICompositionEditController";
import { SOEMessageBoxImage, SOEMessageBoxButtons } from "../../../Util/Enumerations";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IValidationSummaryHandlerFactory } from "../../../Core/Handlers/ValidationSummaryHandlerFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IDirtyHandlerFactory } from "../../../Core/Handlers/DirtyHandlerFactory";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { CompanySettingType, Feature, ProductAccountType } from "../../../Util/CommonEnumerations";
import { Constants } from "../../../Util/Constants";
import { IStockService } from "../../../Shared/Billing/Stock/StockService";
import { SmallGenericType } from "../../../Common/Models/SmallGenericType";
import { IAccountingSettingsRowDTO } from "../../../Scripts/TypeLite.Net4";
import { ICoreService } from "../../../Core/Services/CoreService";
import { SettingsUtility } from "../../../Util/SettingsUtility";

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    private terms: any;

    // Data
    stockId: number;
    stock: any;
    accountStdsDict: any = [];

    // Account settings
    accountStockIn: number;
    accountStockInChange: number;
    accountStockOut: number;
    accountStockOutChange: number;
    accountStockInventory: number;
    accountStockInventoryChange: number;
    accountStockLoss: number;
    accountStockLossChange: number;

    stockAccountSettingTypes: SmallGenericType[];
    stockAccountingSettings: IAccountingSettingsRowDTO[];
    stockBaseAccounts: SmallGenericType[];

    get isExternalStock(): boolean {
        return this.stock ? this.stock.isExternal : false;
    }

    set isExternalStock(value: boolean) {
        if (!this.isNew && (this.stock.isExternal) && (!value)) {
            var keys: string[] = [
                "billing.stock.stocks.disconnectexternalmsg",
            ];
            this.translationService.translateMany(keys).then((terms) => {
                var modal = this.notificationService.showDialog(terms[""], terms["billing.stock.stocks.disconnectexternalmsg"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
                modal.result.then(
                    (val) => {
                        this.stock.isExternal = value;
                    },
                    (cancel) => {
                        return;
                    })
            });
        }
        else {
            this.stock.isExternal = value;
        }
    }

    //@ngInject
    constructor(
        private $q: ng.IQService,
        private coreService: ICoreService,
        private stockService: IStockService,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        urlHelperService: IUrlHelperService,
        progressHandlerFactory: IProgressHandlerFactory,
        private controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        private dirtyHandlerFactory: IDirtyHandlerFactory,
        private $scope: ng.IScope) {
        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onLoadData(() => this.onLoadData())
            .onDoLookUp(() => this.onDoLookUp())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));
    }

    public onInit(parameters: any) {
        this.stockId = parameters.id;
        this.guid = parameters.guid;
        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);

        this.flowHandler.start([{ feature: Feature.Billing_Stock_Place, loadReadPermissions: true, loadModifyPermissions: true }]);
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Billing_Stock_Place].readPermission;
        this.modifyPermission = response[Feature.Billing_Stock_Place].modifyPermission;
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultEditToolbar(true, null, () => this.isNew);
    }

    private onDoLookUp() {
        return this.$q.all([
            this.loadTerms(),
            this.loadCompanySettings(),
            this.loadAccountStdsDict()]).then(() => {
                this.createSettingTypes();
            });
    }

    private loadTerms(): ng.IPromise<any> {
        const keys: string[] = [
            "billing.products.products.stockaccountsettingtype.stockin",
            "billing.products.products.stockaccountsettingtype.stockinchange",
            "billing.products.products.stockaccountsettingtype.stockout",
            "billing.products.products.stockaccountsettingtype.stockoutchange",
            "billing.products.products.stockaccountsettingtype.stockinv",
            "billing.products.products.stockaccountsettingtype.stockinvchange",
            "billing.products.products.stockaccountsettingtype.stockloss",
            "billing.products.products.stockaccountsettingtype.stocklosschange",
        ];
        return this.translationService.translateMany(keys)
            .then(terms => this.terms = terms);
    }

    private loadCompanySettings(): ng.IPromise<any> {
        const settingTypes: number[] = [
            CompanySettingType.AccountStockIn,
            CompanySettingType.AccountStockInChange,
            CompanySettingType.AccountStockOut,
            CompanySettingType.AccountStockOutChange,
            CompanySettingType.AccountStockInventory,
            CompanySettingType.AccountStockInventoryChange,
            CompanySettingType.AccountStockLoss,
            CompanySettingType.AccountStockLossChange
        ];

        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.stockBaseAccounts = [];
            this.stockBaseAccounts.push(new SmallGenericType(ProductAccountType.StockIn, SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountStockIn).toString()));
            this.stockBaseAccounts.push(new SmallGenericType(ProductAccountType.StockInChange, SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountStockInChange).toString()));
            this.stockBaseAccounts.push(new SmallGenericType(ProductAccountType.StockOut, SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountStockOut).toString()));
            this.stockBaseAccounts.push(new SmallGenericType(ProductAccountType.StockOutChange, SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountStockOutChange).toString()));
            this.stockBaseAccounts.push(new SmallGenericType(ProductAccountType.StockInv, SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountStockInventory).toString()));
            this.stockBaseAccounts.push(new SmallGenericType(ProductAccountType.StockInvChange, SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountStockInventoryChange).toString()));
            this.stockBaseAccounts.push(new SmallGenericType(ProductAccountType.StockLoss, SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountStockLoss).toString()));
            this.stockBaseAccounts.push(new SmallGenericType(ProductAccountType.StockLossChange, SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountStockLossChange).toString()));
        });
    }
    private loadAccountStdsDict(): ng.IPromise<any> {
        return this.stockService.getAccountStdsDict(true).then((x) => {
            this.accountStdsDict = x;
        });
    }

    private onLoadData(): ng.IPromise<any> {
        if (this.stockId > 0) {
            return this.stockService.getStock(this.stockId).then((x) => {
                this.isNew = false;
                this.stock = x;

                this.stockAccountingSettings = [];
                _.forEach(this.stock.accountingSettings, (row) => {
                        this.stockAccountingSettings.push(row);
                });
            });
        }
        else {
            this.new();
        }
    }

    public save() {
        this.$scope.$broadcast('stopEditing', {
            functionComplete: () => {
            this.stock.accountingSettings = this.stockAccountingSettings;
            this.progress.startSaveProgress((completion) => {
                this.stockService.saveStock(this.stock).then((result) => {
                    if (result.success) {
                        if (result.integerValue && result.integerValue > 0)
                            this.stockId = result.integerValue;
                        completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.stock);
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
        });
    }

    public delete() {
        this.progress.startDeleteProgress((completion) => {
            this.stockService.deleteStock(this.stock.stockId).then((result) => {
                if (result.success) {
                    completion.completed(this.stock);
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
        this.stockId = 0;
        this.stock = {};

        this.stockAccountingSettings = [];
    }

    private createSettingTypes() {
        this.stockAccountSettingTypes = [];
        this.stockAccountSettingTypes.push(new SmallGenericType(ProductAccountType.StockIn, this.terms["billing.products.products.stockaccountsettingtype.stockin"]));
        this.stockAccountSettingTypes.push(new SmallGenericType(ProductAccountType.StockInChange, this.terms["billing.products.products.stockaccountsettingtype.stockinchange"]));
        this.stockAccountSettingTypes.push(new SmallGenericType(ProductAccountType.StockOut, this.terms["billing.products.products.stockaccountsettingtype.stockout"]));
        this.stockAccountSettingTypes.push(new SmallGenericType(ProductAccountType.StockOutChange, this.terms["billing.products.products.stockaccountsettingtype.stockoutchange"]));
        this.stockAccountSettingTypes.push(new SmallGenericType(ProductAccountType.StockInv, this.terms["billing.products.products.stockaccountsettingtype.stockinv"]));
        this.stockAccountSettingTypes.push(new SmallGenericType(ProductAccountType.StockInvChange, this.terms["billing.products.products.stockaccountsettingtype.stockinvchange"]));
        this.stockAccountSettingTypes.push(new SmallGenericType(ProductAccountType.StockLoss, this.terms["billing.products.products.stockaccountsettingtype.stockloss"]));
        this.stockAccountSettingTypes.push(new SmallGenericType(ProductAccountType.StockLossChange, this.terms["billing.products.products.stockaccountsettingtype.stocklosschange"]));
    }

    // VALIDATION
    protected showValidationError() {
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
            if (this.stock) {
                if (!this.stock.code) {
                    mandatoryFieldKeys.push("common.code");
                }
                if (!this.stock.name) {
                    mandatoryFieldKeys.push("common.name");
                }
            }
        });
    }
}