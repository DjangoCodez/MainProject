import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService, IUrlHelperServiceProvider } from "../../../Core/Services/UrlHelperService";
import { ICompositionEditController } from "../../../Core/ICompositionEditController";
import { InventoryService } from "../../../Shared/Economy/Inventory/InventoryService";
import { ICoreService } from "../../../Core/Services/CoreService";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IValidationSummaryHandlerFactory } from "../../../Core/Handlers/ValidationSummaryHandlerFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IDirtyHandlerFactory } from "../../../Core/Handlers/DirtyHandlerFactory";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { SmallGenericType } from "../../../Common/Models/smallgenerictype";
import { AccountingService } from "../../../Shared/Economy/Accounting/AccountingService";
import { SettingsUtility } from "../../../Util/SettingsUtility";
import { Feature, InventoryAccountType, CompanySettingType } from "../../../Util/CommonEnumerations";
import { Constants } from "../../../Util/Constants";
import { AccountingSettingsRowDTO } from "../../../Common/Models/AccountingSettingsRowDTO";

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    // Data
    inventoryWriteOffTemplateId: number;
    inventoryWriteOffTemplate: any;
    inventoryAccountingSettings: AccountingSettingsRowDTO[] = [];

    // Lookups     
    writeOffMethods: any;
    voucherSeries: any;

    // Settings
    inventoryBaseAccounts: SmallGenericType[];
    inventoryAccountSettingTypes: SmallGenericType[];
    defaultVoucherSeriesTypeId: number

    //@ngInject  
    constructor(
        private $q: ng.IQService,
        private inventoryService: InventoryService,
        private accountingService: AccountingService,
        private coreService: ICoreService,
        private translationService: ITranslationService,
        urlHelperService: IUrlHelperService,
        progressHandlerFactory: IProgressHandlerFactory,
        private controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
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
        this.inventoryWriteOffTemplateId = parameters.id;
        this.guid = parameters.guid;
        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);

        this.flowHandler.start([{ feature: Feature.Economy_Inventory_WriteOffTemplates_Edit, loadReadPermissions: true, loadModifyPermissions: true }]);
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Economy_Inventory_WriteOffTemplates_Edit].readPermission;
        this.modifyPermission = response[Feature.Economy_Inventory_WriteOffTemplates_Edit].modifyPermission;
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultEditToolbar(false, null, () => this.isNew);
    }

    // LOOKUPS
    private onDoLookups() {
        return this.$q.all([this.loadCompanySettings(), this.loadSettingTypes(), this.loadWriteOffMethods(), this.loadVoucherSeriesTypes()]);
    }

    private onLoadData(): ng.IPromise<any> {

        if (this.inventoryWriteOffTemplateId > 0) {
            return this.inventoryService.getInventoryWriteOffTemplate(this.inventoryWriteOffTemplateId).then((x) => {
                this.isNew = false;
                this.inventoryWriteOffTemplate = x;
                this.inventoryAccountingSettings = this.ConvertToInventoryAccountingSettings();
            });
        }
        else {
            this.new();
        }
    }

    private ConvertToInventoryAccountingSettings(): AccountingSettingsRowDTO[] {
        var settings: AccountingSettingsRowDTO[] = [];
        _.forEach(this.inventoryAccountSettingTypes, (y) => {

            switch (y.id) {
                case InventoryAccountType.Inventory:
                    var accounts = this.inventoryWriteOffTemplate.inventoryAccounts;
                    break;
                case InventoryAccountType.AccWriteOff:
                    var accounts = this.inventoryWriteOffTemplate.accWriteOffAccounts;
                    break;
                case InventoryAccountType.WriteOff:
                    var accounts = this.inventoryWriteOffTemplate.writeOffAccounts;
                    break;
                case InventoryAccountType.AccOverWriteOff:
                    var accounts = this.inventoryWriteOffTemplate.accOverWriteOffAccounts;
                    break;
                case InventoryAccountType.OverWriteOff:
                    var accounts = this.inventoryWriteOffTemplate.overWriteOffAccounts;
                    break;
                case InventoryAccountType.AccWriteDown:
                    var accounts = this.inventoryWriteOffTemplate.accWriteDownAccounts;
                    break;
                case InventoryAccountType.WriteDown:
                    var accounts = this.inventoryWriteOffTemplate.writeDownAccounts;
                    break;
                case InventoryAccountType.AccWriteUp:
                    var accounts = this.inventoryWriteOffTemplate.accWriteUpAccounts;
                    break;
                case InventoryAccountType.WriteUp:
                    var accounts = this.inventoryWriteOffTemplate.writeUpAccounts;
                    break;
            }

            var row: AccountingSettingsRowDTO = new AccountingSettingsRowDTO(y.id);
            row.typeName = y.name;
            row.account1Id = accounts != null && accounts[1] != null ? accounts[1].accountId : 0;
            row.account1Name = accounts != null && accounts[1] != null ? accounts[1].name : null;
            row.account1Nr = accounts != null && accounts[1] != null ? accounts[1].number : null;
            row.accountDim1Nr = 1;
            row.account2Id = accounts != null && accounts[2] != null ? accounts[2].accountId : 0;
            row.account2Name = accounts != null && accounts[2] != null ? accounts[2].name : null;
            row.account2Nr = accounts != null && accounts[2] != null ? accounts[2].number : null;
            row.accountDim2Nr = 2;
            row.account3Id = accounts != null && accounts[3] != null ? accounts[3].accountId : 0;
            row.account3Name = accounts != null && accounts[3] != null ? accounts[3].name : null;
            row.account3Nr = accounts != null && accounts[3] != null ? accounts[3].number : null;
            row.accountDim3Nr = 3;
            row.account4Id = accounts != null && accounts[4] != null ? accounts[4].accountId : 0;
            row.account4Name = accounts != null && accounts[4] != null ? accounts[4].name : null;
            row.account4Nr = accounts != null && accounts[4] != null ? accounts[4].number : null;
            row.accountDim4Nr = 4;
            row.account5Id = accounts != null && accounts[5] != null ? accounts[5].accountId : 0;
            row.account5Name = accounts != null && accounts[5] != null ? accounts[5].name : null;
            row.account5Nr = accounts != null && accounts[5] != null ? accounts[5].number : null;
            row.accountDim5Nr = 5;
            row.account6Id = accounts != null && accounts[6] != null ? accounts[6].accountId : 0;
            row.account6Name = accounts != null && accounts[6] != null ? accounts[6].name : null;
            row.account6Nr = accounts != null && accounts[6] != null ? accounts[6].number : null;
            row.accountDim6Nr = 6;
            row.baseAccount = null;
            settings.push(row);

        });

        return settings;
    }

    private loadCompanySettings(): ng.IPromise<any> {
        var settingTypes: number[] = [];

        settingTypes.push(CompanySettingType.AccountingVoucherSeriesTypeManual);

        //base accounts
        settingTypes.push(CompanySettingType.AccountInventoryInventories);
        settingTypes.push(CompanySettingType.AccountInventoryAccWriteOff);
        settingTypes.push(CompanySettingType.AccountInventoryWriteOff);
        settingTypes.push(CompanySettingType.AccountInventoryAccOverWriteOff);
        settingTypes.push(CompanySettingType.AccountInventoryOverWriteOff);
        settingTypes.push(CompanySettingType.AccountInventoryAccWriteDown);
        settingTypes.push(CompanySettingType.AccountInventoryWriteDown);
        settingTypes.push(CompanySettingType.AccountInventoryAccWriteUp);
        settingTypes.push(CompanySettingType.AccountInventoryWriteUp);

        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.defaultVoucherSeriesTypeId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountingVoucherSeriesTypeManual, this.defaultVoucherSeriesTypeId);

            // Base accounts
            this.inventoryBaseAccounts = [];
            this.inventoryBaseAccounts.push(new SmallGenericType(InventoryAccountType.Inventory, SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountInventoryInventories).toString()));
            this.inventoryBaseAccounts.push(new SmallGenericType(InventoryAccountType.AccWriteOff, SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountInventoryAccWriteOff).toString()));
            this.inventoryBaseAccounts.push(new SmallGenericType(InventoryAccountType.WriteOff, SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountInventoryWriteOff).toString()));
            this.inventoryBaseAccounts.push(new SmallGenericType(InventoryAccountType.AccOverWriteOff, SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountInventoryAccOverWriteOff).toString()));
            this.inventoryBaseAccounts.push(new SmallGenericType(InventoryAccountType.OverWriteOff, SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountInventoryOverWriteOff).toString()));
            this.inventoryBaseAccounts.push(new SmallGenericType(InventoryAccountType.AccWriteDown, SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountInventoryAccWriteDown).toString()));
            this.inventoryBaseAccounts.push(new SmallGenericType(InventoryAccountType.WriteDown, SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountInventoryWriteDown).toString()));
            this.inventoryBaseAccounts.push(new SmallGenericType(InventoryAccountType.AccWriteUp, SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountInventoryAccWriteUp).toString()));
            this.inventoryBaseAccounts.push(new SmallGenericType(InventoryAccountType.WriteUp, SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountInventoryWriteUp).toString()));
        });
    }

    private loadSettingTypes(): ng.IPromise<any> {
        var keys: string[] = [
            "economy.inventory.inventoryaccountsettingtype.inventory",
            "economy.inventory.inventoryaccountsettingtype.accwriteoff",
            "economy.inventory.inventoryaccountsettingtype.writeoff",
            "economy.inventory.inventoryaccountsettingtype.accoverwriteoff",
            "economy.inventory.inventoryaccountsettingtype.overwriteoff",
            "economy.inventory.inventoryaccountsettingtype.accwritedown",
            "economy.inventory.inventoryaccountsettingtype.writedown",
            "economy.inventory.inventoryaccountsettingtype.accwriteup",
            "economy.inventory.inventoryaccountsettingtype.writeup"
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.inventoryAccountSettingTypes = [];
            this.inventoryAccountSettingTypes.push(new SmallGenericType(InventoryAccountType.Inventory, terms["economy.inventory.inventoryaccountsettingtype.inventory"]));
            this.inventoryAccountSettingTypes.push(new SmallGenericType(InventoryAccountType.AccWriteOff, terms["economy.inventory.inventoryaccountsettingtype.accwriteoff"]));
            this.inventoryAccountSettingTypes.push(new SmallGenericType(InventoryAccountType.WriteOff, terms["economy.inventory.inventoryaccountsettingtype.writeoff"]));
            this.inventoryAccountSettingTypes.push(new SmallGenericType(InventoryAccountType.AccOverWriteOff, terms["economy.inventory.inventoryaccountsettingtype.accoverwriteoff"]));
            this.inventoryAccountSettingTypes.push(new SmallGenericType(InventoryAccountType.OverWriteOff, terms["economy.inventory.inventoryaccountsettingtype.overwriteoff"]));
            this.inventoryAccountSettingTypes.push(new SmallGenericType(InventoryAccountType.AccWriteDown, terms["economy.inventory.inventoryaccountsettingtype.accwritedown"]));
            this.inventoryAccountSettingTypes.push(new SmallGenericType(InventoryAccountType.WriteDown, terms["economy.inventory.inventoryaccountsettingtype.writedown"]));
            this.inventoryAccountSettingTypes.push(new SmallGenericType(InventoryAccountType.AccWriteUp, terms["economy.inventory.inventoryaccountsettingtype.accwriteup"]));
            this.inventoryAccountSettingTypes.push(new SmallGenericType(InventoryAccountType.WriteUp, terms["economy.inventory.inventoryaccountsettingtype.writeup"]));
        });
    }

    private loadWriteOffMethods(): ng.IPromise<any> {
        return this.inventoryService.getInventoryWriteOffMethodsDict().then((x) => {
            this.writeOffMethods = x;
        });
    }

    private loadVoucherSeriesTypes(): ng.IPromise<any> {
        return this.accountingService.getVoucherSeriesTypes().then((x) => {
            this.voucherSeries = x;
        });
    }

    private save() {
        this.progress.startSaveProgress((completion) => {
            this.inventoryService.saveInventoryWriteOffTemplate(this.inventoryWriteOffTemplate, this.inventoryAccountingSettings).then((result) => {
                if (result.success) {
                    if (result.integerValue && result.integerValue > 0)
                        this.inventoryWriteOffTemplateId = result.integerValue;
                    completion.completed(Constants.EVENT_EDIT_SAVED, this.inventoryWriteOffTemplate);
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

    private delete() {
        this.progress.startDeleteProgress((completion) => {
            this.inventoryService.deleteInventoryWriteOffTemplate(this.inventoryWriteOffTemplate.inventoryWriteOffTemplateId).then((result) => {
                if (result.success) {
                    completion.completed(this.inventoryWriteOffTemplate);
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
        this.inventoryWriteOffTemplateId = 0;
        this.inventoryWriteOffTemplate = {};
        this.inventoryAccountingSettings = this.ConvertToInventoryAccountingSettings();
    }

    // VALIDATION

    public showValidationError() {

    }
}