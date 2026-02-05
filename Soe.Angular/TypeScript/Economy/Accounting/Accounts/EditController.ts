import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2";
import { ICompositionEditController } from "../../../Core/ICompositionEditController";
import { IAccountDTO, ISmallGenericType } from "../../../Scripts/TypeLite.Net4";
import { IAccountingService } from "../../../Shared/Economy/Accounting/AccountingService";
import { ICoreService } from "../../../Core/Services/CoreService";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { IReportService } from "../../../Core/Services/ReportService";
import { ISupplierService } from "../../../Shared/Economy/Supplier/SupplierService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IValidationSummaryHandlerFactory } from "../../../Core/Handlers/ValidationSummaryHandlerFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IDirtyHandlerFactory } from "../../../Core/Handlers/DirtyHandlerFactory";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { ToolBarUtility, ToolBarButton } from "../../../Util/ToolBarUtility";
import { IconLibrary, SOEMessageBoxButtons, SOEMessageBoxImage } from "../../../Util/Enumerations";
import { CoreUtility } from "../../../Util/CoreUtility";
import { TabMessage } from "../../../Core/Controllers/TabsControllerBase1";
import { Feature, TermGroup_AmountStop, TermGroup, CompTermsRecordType, SoeReportTemplateType, TermGroup_SieAccountDim, SoeEntityState, SoeCategoryType, SoeEntityType, CompanySettingType, SoeTimeSalaryExportTarget } from "../../../Util/CommonEnumerations";
import { Constants } from "../../../Util/Constants";
import { AccountDimDTO } from "../../../Common/Models/AccountDimDTO";
import { SmallGenericType } from "../../../Common/Models/SmallGenericType";
import { CategoryAccountDTO } from "../../../Common/Models/Category";
import { ExtraFieldGridDTO, ExtraFieldRecordDTO } from "../../../Common/Models/ExtraFieldDTO";
import { Guid } from "../../../Util/StringUtility";
import { SettingsUtility } from "../../../Util/SettingsUtility";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { IRequestReportService } from "../../../Shared/Reports/RequestReportService";

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    private accountDimId: number;
    private accountId: number;
    private account: any;
    terms: any = [];

    private params: any = {
        "accountDimId": 0,
        "accountId": 0,
    };


    private categories: ISmallGenericType[] = [];
    private selectedCategories: ISmallGenericType[] = [];
    private originalSelectedCategories: ISmallGenericType[] = [];
    private categoryAccounts: CategoryAccountDTO[] = [];
    private extraFieldRecords: ExtraFieldRecordDTO[] = [];

    private result: any;
    private translations = [];
    private status = {}
    private generalLedgerReportUrl: string;
    private isStdAccount: boolean;
    private calcAllAccounts: boolean;
    private categoryAccountsComponentUrl; any;

    hasExtraFieldPermission = false;

    private accountDim: AccountDimDTO;
    private projectAccountDim: AccountDimDTO;
    private shiftTypeAccountDim: AccountDimDTO;
    private languages: any;
    private accountMappings: any;
    private accountTypes: any;
    private amountStopTypes: any;
    private sysVatAccounts: any;
    private sysAccountSruCodes: any;
    private accountBalances: any;
    private attestFlowPermission: boolean;
    private attestGroups: any[];

    // Flags
    private loading = false;
    private isCostPlaceDim = false;
    private showExternalCodes = false;

    //Parent/Children
    private parentAccountDimId: number = 0;
    private parentAccountDimName: string;
    private accounts = [];
    private childrenAccounts: IAccountDTO[] = [];

    private _balanceExpanderInitiallyOpened: boolean;
    set balanceExpanderInitiallyOpened(value: boolean) {
        this._balanceExpanderInitiallyOpened = value;
    }
    get balanceExpanderInitiallyOpened(): boolean {
        return this._balanceExpanderInitiallyOpened !== undefined ? this._balanceExpanderInitiallyOpened : this.isNew;
    }

    private get isProjectAccountDim(): boolean {
        return this.projectAccountDim && this.accountDimId && this.projectAccountDim.accountDimId === this.accountDimId;
    }

    private get isShiftTypeAccountDim(): boolean {
        return this.shiftTypeAccountDim && this.accountDimId && this.shiftTypeAccountDim.accountDimId === this.accountDimId;
    }

    extraFieldsExpanderRendered = false;

    // Extra fields
    private extraFields: ExtraFieldGridDTO[] = [];
    get showExtraFieldsExpander() {
        return this.hasExtraFieldPermission;
    }

    private isPrinting: boolean = false;

    //@ngInject
    constructor(
        private $q: ng.IQService,
        private $scope: ng.IScope,
        private accountingService: IAccountingService,
        private coreService: ICoreService,
        private $filter: ng.IFilterService,
        private messagingService: IMessagingService,
        private urlHelperService: IUrlHelperService,
        private reportService: IReportService,
        private supplierService: ISupplierService,
        private translationService: ITranslationService,
        private $window: ng.IWindowService,
        private notificationService: INotificationService,
        progressHandlerFactory: IProgressHandlerFactory,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        private dirtyHandlerFactory: IDirtyHandlerFactory,
        private $timeout: ng.ITimeoutService,
        private readonly requestReportService: IRequestReportService) {
        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);

        $scope.$on(Constants.EVENT_ON_INIT_MODAL, (e, parameters) => {
            parameters.guid = Guid.newGuid();
        });

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onDoLookUp(() => this.onDoLookups())
            .onLoadData(() => this.onLoadData())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));

        this.loading = true;
    }

    public onInit(parameters: any) {
        this.accountDimId = parameters.accountDimId;
        this.accountId = parameters.id;
        this.isStdAccount = soeConfig.isStdAccount;
        this.accountBalances = [];
        this.accountMappings = [];
        this.guid = parameters.guid;

        this._balanceExpanderInitiallyOpened = false;

        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);
        this.navigatorRecords = parameters.navigatorRecords;

        this.flowHandler.start([
            { feature: Feature.Economy_Accounting_Accounts_Edit, loadReadPermissions: true, loadModifyPermissions: true },
            { feature: Feature.Economy_Supplier_Invoice_AttestFlow, loadReadPermissions: false, loadModifyPermissions: true },
            { feature: Feature.Common_ExtraFields_Account, loadModifyPermissions: true }
        ]);
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Economy_Accounting_Accounts_Edit].readPermission;
        this.modifyPermission = response[Feature.Economy_Accounting_Accounts_Edit].modifyPermission;
        this.attestFlowPermission = response[Feature.Economy_Supplier_Invoice_AttestFlow].modifyPermission;
        this.hasExtraFieldPermission = response[Feature.Common_ExtraFields_Account].modifyPermission;
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultEditToolbar(true, () => { this.onCopy() }, () => this.isNew);

        this.toolbar.addButtonGroup(
            ToolBarUtility.createGroup(
                new ToolBarButton(
                    "economy.accounting.generalledger", 
                    "economy.accounting.generalledger", 
                    IconLibrary.FontAwesome, 
                    "fa-print", 
                    () => {
                        this.printAccount();
                    }, 
                    () => { 
                        return this.isPrinting; 
                    }, 
                    () => { return !this.generalLedgerReportUrl }
                )
            )
        );
        this.toolbar.setupNavigationRecords(this.navigatorRecords, this.accountId, recordId => {
            if (recordId !== this.accountId) {
                this.accountId = recordId;
                this.onLoadData();
            }
        });
    }

    public onDoLookups(): ng.IPromise<any> {
        return this.$q.all([
            this.loadTerms(),
            this.loadCompanySettings(),
            this.loadProjectAccountDim(),
            this.loadShiftTypeAccountDim(),
            this.loadTranslations(),
            this.loadAccountMappings(),
            this.loadAccountBalance(),
            this.loadGeneralLedgerUrl(),
            this.loadSysVatAccounts(),
            this.loadLanguages(),
            this.loadAccountTypes(),
            this.loadAmountStopTypes(),
            this.loadSysAccountSruCodes(),
            this.loadAttestGroups(),
            this.loadAccountDim(),
            this.loadAccountChildrenDict(),
            this.loadCategories()
        ]);
    }

    private loadCompanySettings(): ng.IPromise<any> {
        let settingTypes: number[] = [CompanySettingType.SalaryExportTarget];

        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.showExternalCodes = (SettingsUtility.getIntCompanySetting(x, CompanySettingType.SalaryExportTarget) == SoeTimeSalaryExportTarget.BlueGarden);
        });
    }

    private loadCategories(): ng.IPromise<any> {
        return this.coreService.getCategoriesDict(SoeCategoryType.Employee, false).then((x) => {
            this.categories = x;
        });
    }

    private loadCategoryAccounts() {
        this.selectedCategories = [];
        this.originalSelectedCategories = [];
        this.coreService.getCategoryAccountsByAccount(this.accountId, false).then((x) => {
            _.forEach(x, cAcc => {
                let sc = _.find(this.categories, c => c.id === cAcc.categoryId);
                if (sc) {
                    this.selectedCategories.push(new SmallGenericType(sc.id, sc.name));
                }
            });
            angular.extend(this.originalSelectedCategories, x);
        });
    }

    private onLoadData(): ng.IPromise<any> {
        const deferral = this.$q.defer();
        this.selectedCategories = [];
        if (this.accountId > 0) {
            this.params['accountId'] = this.accountId;
            this.params['accountDimId'] = this.accountDimId;
            this.accountingService.getAccount(this.accountId).then((x) => {
                this.isNew = false;
                this.account = x;
                this.dirtyHandler.clean();
                this.messagingHandler.publishSetTabLabel(this.guid, this.terms["economy.accounting.account"] + ' ' + this.account.accountNr);

                this.sysVatAccountChanged(this.account.sysVatAccountId);
                this.loadCategoryAccounts();

                if (this.extraFieldsExpanderRendered)
                    this.$scope.$broadcast('reloadExtraFields', { guid: this.guid, recordId: this.accountId });

                this.loading = false;
                deferral.resolve();
            });
        } else {
            this.isNew = true;
            this.accountId = 0;
            this.account = {
                accountDimId: this.accountDimId,
                amountStop: TermGroup_AmountStop.Debit,
                rowTextStop: true,
                active: true,
            };

            this.loading = false;
            deferral.resolve();
        }

        return deferral.promise;
    }

    private loadTerms() {
        const keys: string[] = [
            "economy.accounting.account",
            "core.warning"
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
        });
    }

    private loadProjectAccountDim(): ng.IPromise<any> {
        return this.accountingService.getProjectAccountDim().then(x => {
            this.projectAccountDim = x;
        });
    }

    private loadShiftTypeAccountDim(): ng.IPromise<any> {
        return this.accountingService.getShiftTypeAccountDim(false).then(x => {
            this.shiftTypeAccountDim = x;
        });
    }

    private loadLanguages(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.Language, false, false).then((x) => {
            this.languages = x;
        });
    }

    private loadTranslations(): ng.IPromise<any> {
        const deferral = this.$q.defer();

        if (this.accountId) {
            this.coreService.getTranslations(CompTermsRecordType.AccountName, this.accountId, false).then((x) => {
                this.translations = x;

                deferral.resolve();
            });
        } else {
            deferral.resolve();
        }

        return deferral.promise;
    }

    private loadAccountMappings(): ng.IPromise<any> {
        const deferral = this.$q.defer();

        this.accountingService.getAccountMappings(this.accountId ? this.accountId : 0).then((x) => {
            this.accountMappings = x;
            // Add empty rows to internal accounts
            _.forEach(this.accountMappings, accountMapping => {

                if (!accountMapping.accounts) {
                    accountMapping.accounts = [];
                }

                accountMapping.accounts = _.sortBy(accountMapping.accounts, function (obj) {
                    return parseInt(obj.accountNr, 10);
                });

                accountMapping.accounts.splice(0, 0, {
                    accountId: 0, name: '', dimNameNumberAndName:''
                 });
             });

            deferral.resolve();
        });

        return deferral.promise;
    }

    private loadAccountTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.AccountType, false, true).then((x) => {
            this.accountTypes = x;
        });
    }

    private loadAmountStopTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.AmountStop, false, false).then((x) => {
            this.amountStopTypes = x;
        });
    }

    private loadSysVatAccounts(): ng.IPromise<any> {
        return this.accountingService.getSysVatAccounts(CoreUtility.sysCountryId, true).then((x) => {
            this.sysVatAccounts = x;
            if (x[0].name == " ") {
                this.sysVatAccounts.splice(0, 1);
            }
        });
    }

    private loadSysAccountSruCodes(): ng.IPromise<any> {
        return this.accountingService.getSysAccountSruCodes(true).then((x) => {
            this.sysAccountSruCodes = x;  
        });
    }

    private loadAccountBalance(): ng.IPromise<any> {
        const deferral = this.$q.defer();

        if (this.accountId) {
            return this.accountingService.getAccountBalanceByAccount(this.accountId, true).then((x) => {
                this.accountBalances = x;

                // Format balance column
                var filter: Function = this.$filter("amount");
                _.forEach(x, (y) => {
                    y['balanceStr'] = filter(y['balance']);
                });
            });
        } else {
            deferral.resolve();
        }

        return deferral.promise;
    }

    private loadGeneralLedgerUrl(): ng.IPromise<any> {
        const deferral = this.$q.defer();

        if (this.accountId) {
            this.reportService.getReportPrintUrl(SoeReportTemplateType.GeneralLedger, this.accountId).then((x) => {
                this.generalLedgerReportUrl = x;
                deferral.resolve();
            });
        } else {
            deferral.resolve();
        }

        return deferral.promise;
    }

    private printAccount(): void {
        
        this.isPrinting = true;

        this.requestReportService
        .printAccount(this.accountId)
        .then(() => {
            this.isPrinting = false;
        });
    }

    private loadAccountDim(): ng.IPromise<any> {
        return this.accountingService.getAccountDim(this.accountDimId, false, false, false).then((x: AccountDimDTO) => {
            this.accountDim = x;
            if (x.sysSieDimNr == TermGroup_SieAccountDim.CostCentre && this.attestFlowPermission)
                this.isCostPlaceDim = true;

            this.parentAccountDimId = x.parentAccountDimId;
            if (this.parentAccountDimId) {
                this.loadParentAccountDim();
                this.loadAccountsDict();
            }
        });
    }

    private loadParentAccountDim(): ng.IPromise<any> {
        return this.accountingService.getAccountDim(this.parentAccountDimId, false, false, false).then((x: AccountDimDTO) => {
            this.parentAccountDimName = x.name;
        });
    }

    private loadAttestGroups(): ng.IPromise<any> {
        return this.supplierService.getAttestWorkFlowGroupsDict(true).then((x) => {
            this.attestGroups = x;
        });
    }

    private loadAccountsDict(): ng.IPromise<any> {
        const deferral = this.$q.defer();

        if (this.parentAccountDimId) {
            this.accountingService.getAccountDict(this.parentAccountDimId, true).then((x) => {
                this.accounts = x;
                deferral.resolve();
            });
        } else {
            deferral.resolve();
        }

        return deferral.promise;
    }

    private loadAccountChildrenDict(): ng.IPromise<any> {
        const deferral = this.$q.defer();

        if (this.accountId) {
            this.accountingService.getAccountChildren(this.accountId).then((x) => {
                this.childrenAccounts = x;
                deferral.resolve();
            });
        } else {
            deferral.resolve();
        }

        return deferral.promise;
    }

    private loadPayrollExportFileType(): ng.IPromise<any> {
        const deferral = this.$q.defer();

        this.showExternalCodes = true;
        deferral.resolve();

        return deferral.promise;
    }

    private sysVatAccountChanged(sysVatAccountId: number) {
        if (!sysVatAccountId) {
            this.account.sysVatRate = "";
            return;
        }

        this.accountingService.getSysVatRate(sysVatAccountId).then((x) => {
            this.account.sysVatRate = x + " %";
        });
    }

    public clearAccountDimCache() {
        //used from accountdim directive
        this.accountingService.getAccountDimsSmall(false, true, true, false, false);
        //Used from account settings directive
        this.coreService.getAccountDimsSmall(false, false, true, false, true, true, false);
    }

    private onCategorySelectionChanged() {
        this.categoryAccounts = [];
        let modified = false;
        if (this.originalSelectedCategories.length != this.selectedCategories.length) {
            modified = true;
        }
        _.forEach(this.selectedCategories, sCat => {
            let cObj = new CategoryAccountDTO();
            cObj.categoryId = sCat.id;
            this.categoryAccounts.push(cObj);
            if (!modified) {
                let isMatched = false;
                _.forEach(this.originalSelectedCategories, oSCat => {
                    if (sCat.id == oSCat.id) {
                        isMatched = true;
                    }
                });
                if (!isMatched) {
                    modified = true;
                }
            }
        });
        if (modified) {
            this.dirtyHandler.setDirty();
        }
    }

    public validateAccountNr() {
        this.$timeout(() => {
            this.accountingService.validateAccountNr(this.account.accountNr, this.account.accountId ? this.account.accountId : 0, this.accountDimId).then((result) => {
                if (!result.success)
                    this.notificationService.showDialog(this.terms["core.warning"], result.errorMessage, SOEMessageBoxImage.Error, SOEMessageBoxButtons.OK);
            });
        });
    }

    public save() {
        this.$timeout(() => {
            this.progress.startSaveProgress((completion) => {
                this.accountingService.saveAccount(this.account, this.translations, this.accountMappings, this.categoryAccounts, _.filter(this.extraFieldRecords, (r) => r.isModified === true)).then((result) => {
                    if (result.success) {
                        if (result.integerValue && result.integerValue > 0) {
                            if (this.accountId == 0) {
                                if (this.navigatorRecords) {
                                    this.navigatorRecords.push(new SmallGenericType(result.integerValue, this.account.accountNr));
                                    this.toolbar.setSelectedRecord(result.integerValue);
                                } else {
                                    this.reloadNavigationRecords(result.integerValue);
                                }
                            }

                            this.accountId = result.integerValue;
                            this.account.accountId = result.integerValue;
                        }
                        this.clearAccountDimCache();
                        completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.account);
                    } else {
                        completion.failed(result.errorMessage);
                    }
                }, error => {
                    completion.failed(error.message);
                });

            }, this.guid).then(data => {
                this.dirtyHandler.clean();
                this.onLoadData();
            }, error => { })
        });
    }

    private reloadNavigationRecords(selectedRecord) {
        this.navigatorRecords = [];
        this.accountingService.getAccounts(soeConfig.accountDimId, soeConfig.accountYearId, this.accountDim.linkedToShiftType, !soeConfig.isStdAccount, !soeConfig.isStdAccount, false).then(data => {
            _.forEach(data, (row) => {
                if (row.isActive) {
                    this.navigatorRecords.push(new SmallGenericType(row.accountId, row.accountNr));
                }
            });
            this.toolbar.setupNavigationRecords(this.navigatorRecords, selectedRecord, recordId => {
                if (recordId !== this.accountId) {
                    this.accountId = recordId;
                    this.onLoadData();
                }
            });
            this.toolbar.setSelectedRecord(selectedRecord);
        });
    }

    public delete() {
        this.progress.startDeleteProgress((completion) => {
            this.accountingService.deleteAccount(this.accountId).then((result) => {
                if (result.success) {
                    completion.completed(this.account);
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

    private onCopy() {
        this.isNew = true;
        this.accountId = 0;
        this.account.accountId = 0;
        this.account.accountNr = undefined;

        this.translations = [];
        this.dirtyHandler.setDirty();

        // Set tab name to "New voucher"
        this.messagingService.publish(Constants.EVENT_EDIT_NEW, {
            guid: this.guid,
        });
    }

    private onReCalculateBalances() {
        this._balanceExpanderInitiallyOpened = true;

        if (this.calcAllAccounts) {
            this.progress.startWorkProgress((completion) => {
                this.accountingService.calculateAccountBalanceForAccountsAllYears().then((result) => {
                    if (result.success) {
                        completion.completed(this.account, false);
                    } else {
                        completion.failed(result.errorMessage);
                    }

                });
            }).then(data => {
                this.dirtyHandler.clean();
                this.loadAccountBalance();
            }, error => { });

            return;
        }

        this.progress.startWorkProgress((completion) => {
            this.accountingService.calculateAccountBalanceForAccountInAccountYears(this.accountId).then((result) => {
                if (result.success) {
                    completion.completed(this.account, false);
                } else {
                    completion.failed(result.errorMessage);
                }
            });

        }).then(data => {
            this.dirtyHandler.clean();
            this.loadAccountBalance();

        }, error => { });
    }

    private addTranslation() {
        var languageId: any;
        var done: boolean = false;
        _.forEach(this.languages, (lang: any) => {
            if (!done) {
                var langExists: boolean = false;
                _.forEach(this.translations, (translation: any) => {
                    if (lang.id === translation.lang)
                        langExists = true;
                });
                if (!langExists) {
                    languageId = lang.id;
                    done = true;
                }
            }
        });

        if (languageId > 0) {
            this.translations.push({
                'CompTermId ': 0,
                'RecordType': CompTermsRecordType.AccountName,
                'RecordId': this.accountId,
                'lang': languageId,
                'langName': '',
                'name': '',
                'State': SoeEntityState.Active,
            });
        }
    }

    private removeTranslation(translation: any) {
        if (this.translations) {
            this.translations.splice(this.translations.indexOf(translation), 1);
        }
    }

    private openEditAccount(account: IAccountDTO) {
        //const keys: string[] = [
        //    "economy.accounting.account",
        //];

        //this.translationService.translateMany(keys).then((terms) => {
        const message = new TabMessage(
            `${this.terms["economy.accounting.account"]} ${account.accountNr}`,
            account.accountId,
            EditController,
            { id: account.accountId, accountDimId: account.accountDimId },
            this.urlHelperService.getGlobalUrl("/Economy/Accounting/Accounts/Views/edit.html")
        );
        this.messagingService.publish(Constants.EVENT_OPEN_TAB, message);
        //});
    }

    public showValidationError() {
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
            if (this.account) {
                if (!this.account.accountNr) {
                    mandatoryFieldKeys.push("economy.accounting.accountnr");
                }
                if (!this.account.name) {
                    mandatoryFieldKeys.push("common.name");
                }

                if (!this.account.accountTypeSysTermId) {
                    mandatoryFieldKeys.push("economy.accounting.accounttype");
                }
            }
        });
    }

    public onExtraFieldsExpanderOpenClose() {
        this.extraFieldRecords = [];
        this.extraFieldsExpanderRendered = !this.extraFieldsExpanderRendered;
    }
}