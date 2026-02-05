import { IReportDataService } from "../../ReportDataService";
import { ISmallGenericType } from "../../../../../Scripts/TypeLite.Net4";
import { ICoreService } from "../../../../Services/CoreService";
import { CompanySettingType, UserSettingType, SoeReportTemplateType, TermGroup, TermGroup_EmployeeSelectionAccountingType } from "../../../../../Util/CommonEnumerations";
import { AccountDimSmallDTO } from "../../../../../Common/Models/AccountDimDTO";
import { AccountDTO } from "../../../../../Common/Models/AccountDTO";
import { ITranslationService } from "../../../../Services/TranslationService";
import { SettingsUtility } from "../../../../../Util/SettingsUtility";
import { CoreUtility } from "../../../../../Util/CoreUtility";

export interface IDataSourceFilterCollection {
    id: string;
    category: string,
    canMultiSelect: boolean;
    availableFilters: IDataSourceFilter[];
    selectedFilters: IDataSourceFilter[];
    sort: number;
    displayLabel: string;
    hidden: boolean;
}

export interface IDataSourceFilter {
    id: string;
    originId: number;
    label: string;
}

export interface IEmployeeDataSource {
    employees: ISmallGenericType[];
    accountingTypes: ISmallGenericType[];
    accountDimensions: AccountDimSmallDTO[];
    filteredAccountIds: number[];
    filteredEmployeeGroupIds: number[];
    filteredCategoryIds: number[];
    filteredVacationGroupIds: number[];
    filteredPayrollGroupIds: number[];
    populate(): ng.IPromise<any>;
    applyFilter(collectionId: string, selectedFilters: IDataSourceFilter[]);
    getSupportedFilters(): ng.IPromise<IDataSourceFilterCollection[]>;
    setAccounts(accountIds: number[]);
    setDateRange(from: Date, to: Date);
    setTimePeriodIds(timePeriodIds: number[]);
    setReportTemplateType(value: SoeReportTemplateType);
    setAccountingType(value: TermGroup_EmployeeSelectionAccountingType);
    setIncludeInactive(value: boolean);
    setOnlyInactive(value: boolean);
    setIncludeEnded(value: boolean);
    setIncludeVacant(value: boolean);
    setIncludeHidden(value: boolean);
    setIncludeSecondary(value: boolean);
}

export class AllEmployeesDataSource implements IEmployeeDataSource {
    private toDate: Date;
    private fromDate: Date;
    private timePeriodIds: number[];
    private soeReportTemplateType: SoeReportTemplateType;
    private accountingType: TermGroup_EmployeeSelectionAccountingType = TermGroup_EmployeeSelectionAccountingType.EmployeeCategory;
    private includeInactive: boolean;
    private onlyInactive: boolean;
    private includeEnded: boolean;
    private includeVacant: boolean;
    private includeHidden: boolean;
    private includeSecondary: boolean;
    private fetchedEmployees: ISmallGenericType[] = [];

    constructor(private $q: ng.IQService, private reportDataService: IReportDataService) {
    }

    public populate(): ng.IPromise<any> {
        return this.reportDataService.getFilteredEmployees(
            this.fromDate,
            this.toDate,
            this.timePeriodIds,
            this.soeReportTemplateType,
            this.accountingType,
            null,
            null,
            null,
            null,
            null,
            this.includeInactive,
            this.includeEnded,
            this.includeVacant,
            this.includeHidden,
            this.includeSecondary)
            .then(employees => this.fetchedEmployees = employees);
    }

    public setAccounts(accountIds: number[]) {
        //Do nothing, cannot apply data source filter.
    }

    public setDateRange(from: Date, to: Date) {
        this.toDate = to;
        this.fromDate = from;
    }

    public setTimePeriodIds(value: number[]) {
        this.timePeriodIds = value;
    }

    public setReportTemplateType(value: SoeReportTemplateType) {
        this.soeReportTemplateType = value;
    }

    public setAccountingType(value: TermGroup_EmployeeSelectionAccountingType) {
        this.accountingType = value;
    }

    public setIncludeInactive(value: boolean) {
        this.includeInactive = value;
    }

    public setOnlyInactive(value: boolean) {
        this.onlyInactive = value;
    }

    public setIncludeEnded(value: boolean) {
        this.includeEnded = value;
    }

    public setIncludeVacant(value: boolean) {
        this.includeVacant = value;
    }

    public setIncludeHidden(value: boolean) {
        this.includeHidden = value;
    }

    public setIncludeSecondary(value: boolean) {
        this.includeSecondary = value;
    }

    public get employees(): ISmallGenericType[] {
        return this.fetchedEmployees;
    }

    public get accountingTypes(): ISmallGenericType[] {
        return [];
    }

    public get accountDimensions(): AccountDimSmallDTO[] {
        return [];
    }

    public get filteredAccountIds(): number[] {
        return null;
    }

    public get filteredEmployeeGroupIds(): number[] {
        return null;
    }

    public get filteredCategoryIds(): number[] {
        return null;
    }

    public get filteredVacationGroupIds(): number[] {
        return null;
    }

    public get filteredPayrollGroupIds(): number[] {
        return null;
    }

    public applyFilter(collectionId: string, selectedFilters: IDataSourceFilter[]) {
        //Do nothing, cannot apply data source filter.
    }

    public getSupportedFilters(): ng.IPromise<IDataSourceFilterCollection[]> {
        return this.$q.resolve([]);
    }
}

export class FilteredEmployeesDataSource implements IEmployeeDataSource {
    private toDate: Date;
    private fromDate: Date;
    private timePeriodIds: number[];
    private soeReportTemplateType: SoeReportTemplateType;
    private accountingType: TermGroup_EmployeeSelectionAccountingType;
    public useAccountHierarchy: boolean;
    private accountHierarchyId: string;
    private accountIds: number[];
    private employeeGroupIds: number[];
    private categoryIds: number[];
    private vacationGroupIds: number[];
    private payrollGroupIds: number[];
    private includeInactive: boolean;
    private onlyInactive: boolean;
    private includeEnded: boolean;
    private includeVacant: boolean;
    private includeHidden: boolean;
    private includeSecondary: boolean;
    private fetchedEmployees: ISmallGenericType[] = [];
    private allAccountingTypes: ISmallGenericType[];
    private categories: ISmallGenericType[] = [];
    private employeeGroups: ISmallGenericType[] = [];
    private payrollGroups: ISmallGenericType[] = [];
    private vacationGroups: ISmallGenericType[] = [];
    private accountDims: AccountDimSmallDTO[] = [];

    private filterFetcher: ng.IPromise<any>;
    private filterCollections: IDataSourceFilterCollection[] = [];

    constructor(
        private $q: ng.IQService,
        private reportDataService: IReportDataService,
        private coreService: ICoreService,
        private translationService: ITranslationService) {

        this.filterFetcher = this.$q.all([this.loadCompanySettings(), this.loadUserAndCompanySettings(), this.loadAccountingTypes()]).then(() => {
            return this.$q.all([
                reportDataService.getAccountDims().then(x => this.accountDims = x),
                reportDataService.getEmployeeGroups().then(x => this.employeeGroups = x),
                (this.useAccountHierarchy ? this.$q.resolve([]) : reportDataService.getEmployeeCategories()).then(x => this.categories = x),
                reportDataService.getPayrollGroups().then(x => this.payrollGroups = x),
                reportDataService.getVacationGroups().then(x => this.vacationGroups = x)
            ]).then(() => {
                if (this.useAccountHierarchy)
                    this.preselectAccounts();
                this.buildFiltersCollections();
            });
        });
    }

    public setDateRange(from: Date, to: Date) {
        this.fromDate = from;
        this.toDate = to;
    }

    public setTimePeriodIds(value: number[]) {
        this.timePeriodIds = value;
    }

    public setReportTemplateType(value: SoeReportTemplateType) {
        this.soeReportTemplateType = value;
    }

    public setAccountingType(value: TermGroup_EmployeeSelectionAccountingType) {
        if (this.accountingType !== value) {
            this.accountingType = value;
            this.preselectAccounts();
            this.createMultiSelectionFilterForAccount(this.accountDims);
        }
    }

    public setIncludeInactive(value: boolean) {
        this.includeInactive = value;
    }

    public setOnlyInactive(value: boolean) {
        this.onlyInactive = value;
    }

    public setIncludeEnded(value: boolean) {
        this.includeEnded = value;
    }

    public setIncludeVacant(value: boolean) {
        this.includeVacant = value;
    }

    public setIncludeHidden(value: boolean) {
        this.includeHidden = value;
    }

    public setIncludeSecondary(value: boolean) {
        this.includeSecondary = value;
    }

    public populate(): ng.IPromise<any> {
        this.employeeGroupIds = this.getMultiSelectedOrNull('employeeGroups');
        this.categoryIds = this.getMultiSelectedOrNull('categories');
        this.vacationGroupIds = this.getMultiSelectedOrNull('vacationGroups');
        this.payrollGroupIds = this.getMultiSelectedOrNull('payrollGroups');
        this.accountIds = this.getSelectedAccounts();
        
        return this.reportDataService.getFilteredEmployees(
            this.fromDate,
            this.toDate,
            this.timePeriodIds,
            this.soeReportTemplateType,
            this.accountingType,
            this.accountIds,
            this.categoryIds,
            this.employeeGroupIds,
            this.payrollGroupIds,
            this.vacationGroupIds,
            this.includeInactive,
            this.onlyInactive,
            this.includeEnded,
            this.includeVacant,
            this.includeHidden,
            this.includeSecondary)
            .then(employees => this.fetchedEmployees = employees);
    }

    public get accountingTypes(): ISmallGenericType[] {
        return this.allAccountingTypes;
    }

    public get employees(): ISmallGenericType[] {
        return this.fetchedEmployees;
    }

    public get accountDimensions(): AccountDimSmallDTO[] {
        return this.accountDims;
    }

    public get filteredAccountIds(): number[] {
        return this.accountIds;
    }

    public get filteredEmployeeGroupIds(): number[] {
        return this.employeeGroupIds;
    }

    public get filteredCategoryIds(): number[] {
        return this.categoryIds;
    }

    public get filteredVacationGroupIds(): number[] {
        return this.vacationGroupIds;
    }

    public get filteredPayrollGroupIds(): number[] {
        return this.payrollGroupIds;
    }

    public applyFilter(collectionId: string, selectedFilters: IDataSourceFilter[]): boolean {
        const collection = this.filterCollections.find(c => c.id === collectionId);
        collection.selectedFilters = selectedFilters;

        if (collectionId.startsWith("acc")) {
            this.updatedSelectableAccounts(collection, selectedFilters);
            return true;
        }

        return false;
    }

    public getSupportedFilters(): ng.IPromise<IDataSourceFilterCollection[]> {
        return this.filterFetcher.then(() => this.filterCollections)
    }

    private loadCompanySettings(): ng.IPromise<any> {
        let settingTypes: number[] = [];
        settingTypes.push(CompanySettingType.UseAccountHierarchy);

        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.useAccountHierarchy = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.UseAccountHierarchy);
            if (this.useAccountHierarchy && this.accountingType == TermGroup_EmployeeSelectionAccountingType.EmployeeCategory)
                this.setAccountingType(TermGroup_EmployeeSelectionAccountingType.EmployeeAccount);
        });
    }

    private loadUserAndCompanySettings(): ng.IPromise<any> {
        let settingTypes: number[] = [];
        settingTypes.push(UserSettingType.AccountHierarchyId);

        return this.coreService.getUserAndCompanySettings(settingTypes).then(x => {
            this.accountHierarchyId = SettingsUtility.getStringUserSetting(x, UserSettingType.AccountHierarchyId, '0');
        });
    }

    private loadAccountingTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.EmployeeSelectionAccountingType, false, false, true).then(x => {
            this.allAccountingTypes = x
        });
    }

    private getSingleSelectedOrNull(collectionId: string) {
        const collection = this.filterCollections.find(c => c.id === collectionId);
        return collection && collection.selectedFilters && collection.selectedFilters.length
            ? collection.selectedFilters[0].originId
            : null;
    }

    private getMultiSelectedOrNull(collectionId: string) {
        const collection = this.filterCollections.find(c => c.id === collectionId);
        return collection && collection.selectedFilters && collection.selectedFilters.length
            ? collection.selectedFilters.map(f => f.originId)
            : null;
    }

    private preselectAccounts() {
        // Pre filter on account dims based on user setting
        if (this.accountHierarchyId && this.accountDims && this.accountDims.length > 0) {
            var accounts = this.accountHierarchyId.split('-');

            // 'accounts' will contain all account dims.
            // Remove accounts connected to dimensions not specified as UseInSchedulePlanning.
            let firstDimLevel = this.accountDims[0].level;
            if (firstDimLevel > 1) {
                for (let j = 1; j < firstDimLevel; j++) {
                    if (accounts.length > 0)
                        _.pullAt(accounts, 0);
                }
            }

            let hidden: boolean = (this.accountingType === TermGroup_EmployeeSelectionAccountingType.EmployeeAccount || this.accountingType === TermGroup_EmployeeSelectionAccountingType.TimeScheduleTemplateBlock);
            if (accounts.length > 0) {
                for (var i = 0; i < accounts.length && i < this.accountDims.length; i++) {
                    var dim = this.accountDims[i];
                    var account = _.find(dim.accounts, a => a.accountId === parseInt(accounts[i], 10));
                    if (account) {
                        dim['hidden'] = hidden;
                        dim.filteredAccounts = hidden ? [account] : dim.accounts;
                        dim.selectedAccounts = hidden ? [{ id: account.accountId }] : [];
                    }
                }
            }
        }
    }

    public setAccounts(accountIds: number[]) {
        _.forEach(this.accountDims, dim => {
            dim.filteredAccounts = [];
            dim.selectedAccounts = [];

            _.forEach(accountIds, accountId => {
                var account = _.find(dim.accounts, a => a.accountId === accountId);
                if (account) {
                    dim.filteredAccounts.push(account)
                    dim.selectedAccounts.push({ id: accountId });
                }
            });
        });
    }

    private getSelectedAccounts(): number[] {
        return this.filterCollections
            .filter(c => c.id.startsWith("acc"))
            .map(c => c.selectedFilters.map(f => f.originId))
            .reduce((p: number[], c: number[]) => p.concat(c), []);
    }

    private updatedSelectableAccounts(collection: IDataSourceFilterCollection, selectedFilters: IDataSourceFilter[]) {
        const accountDim = this.accountDims.find(a => this.buildAccountCollectionIdFor(a) === collection.id);
        if (!accountDim)
            return;

        const findChild = (accountDimId: number) => this.accountDims.find(d => d.parentAccountDimId === accountDimId);
        
        let currentDim = findChild(accountDim.accountDimId);
        let selectableAccountIds = selectedFilters.map(f => f.originId);
        while (currentDim) {
            const currentCollectionId = this.buildAccountCollectionIdFor(currentDim);
            collection = this.filterCollections.find(c => c.id === currentCollectionId);
            collection.selectedFilters = [];

            if (selectableAccountIds.length > 0) {
                const selectableAccounts = _(selectableAccountIds)
                    .map(id => currentDim.filteredAccounts.filter(a => a.parentAccountId === id || (CoreUtility.isMartinServera && a.hasVirtualParent))) //find child accounts corresponding to the selected.
                    .reduce((p: AccountDTO[], c: AccountDTO[]) => p.concat(c)) //flatten the above list of lists to one single list.
                    .concat(currentDim.filteredAccounts.filter(a => !a.parentAccountId)) //append accounts that should always be selectable.
                    .sort((a, b) => a.name.localeCompare(b.name)); //sort them by name.

                collection.availableFilters = this.buildAvailableFilters(selectableAccounts);
                selectableAccountIds = selectableAccounts.map(a => a.accountId);
            } else {
                collection.availableFilters = this.buildAvailableFilters(currentDim.filteredAccounts);
                selectableAccountIds = [];
            }

            if (collection.availableFilters.length === 1) {
                collection.selectedFilters = [collection.availableFilters[0]];
            }

            currentDim = findChild(currentDim.accountDimId);
        }
    }

    private buildFiltersCollections() {
        this.createMultiSelectionFilterForAccount(this.accountDims);

        this.createMultiSelectionFilter(this.employeeGroups, "employeeGroups", "eg", "common.employeegroups", 2);
        if (!this.useAccountHierarchy)
            this.createMultiSelectionFilter(this.categories, "categories", "ec", "common.categories", 3);
        this.createMultiSelectionFilter(this.vacationGroups, "vacationGroups", "vc", "common.vacationgroups", 4);
        this.createMultiSelectionFilter(this.payrollGroups, "payrollGroups", "pg", "common.payrollgroups", 5);
    }

    private createSingleSelectionFilter(options: ISmallGenericType[], collectionId: string, filterIdPrefix: string, displayLabelTranslationKey: string, sort: number, category: string = ""): void {
        const allFilter: IDataSourceFilter = {
            id: "{0}-{1}".format(filterIdPrefix, "0"),
            originId: 0,
            label: "{0} {1}".format(this.translationService.translateInstant("common.all"), this.translationService.translateInstant(displayLabelTranslationKey).toLocaleLowerCase())
        }

        const filterCollection: IDataSourceFilterCollection = {
            id: collectionId,
            category: category,
            sort: sort,
            canMultiSelect: false,
            availableFilters: [allFilter].concat(options.map(o => {
                return <IDataSourceFilter>{
                    id: "{0}-{1}".format(filterIdPrefix, o.id.toString()),
                    originId: o.id,
                    label: o.name
                }
            })),
            selectedFilters: [allFilter],
            displayLabel: "",
            hidden: false
        }

        this.filterCollections.push(filterCollection);
    }

    private createMultiSelectionFilter(options: ISmallGenericType[], collectionId: string, filterIdPrefix: string, displayLabelTranslationKey: string, sort: number, category: string = ""): void {
        const filterCollection: IDataSourceFilterCollection = {
            id: collectionId,
            category: category,
            sort: sort,
            canMultiSelect: true,
            availableFilters: options.map(o => {
                return <IDataSourceFilter>{
                    id: "{0}-{1}".format(filterIdPrefix, o.id.toString()),
                    originId: o.id,
                    label: o.name
                }
            }),
            selectedFilters: [],
            displayLabel: this.translationService.translateInstant(displayLabelTranslationKey),
            hidden: false
        }

        this.filterCollections.push(filterCollection);
    }

    private createMultiSelectionFilterForAccount(accountDims: AccountDimSmallDTO[]) {
        _.pullAll(this.filterCollections, this.filterCollections.filter(c => c.category === 'account'));
        this.accountDims.map(a => {
            return <IDataSourceFilterCollection>{
                id: this.buildAccountCollectionIdFor(a),
                category: "account",
                sort: 1,
                canMultiSelect: true,
                availableFilters: this.buildAvailableFilters(a.filteredAccounts),
                selectedFilters: [],
                displayLabel: a.name
            }
        }).forEach(c => this.filterCollections.push(c));
        this.filterCollections = _.orderBy(this.filterCollections, c => c.sort);
    }

    private buildAvailableFilters(accounts: AccountDTO[]) {
        // Only include the 3 first levels of accountDims in the filter for M&S
        if (CoreUtility.isMartinServera)
            accounts = accounts.filter(a => a.accountDim.level < 4);

        return accounts.map(x => {
            return <IDataSourceFilter>{
                id: "{0}-{1}".format("acc", x.accountId.toString()),
                originId: x.accountId,
                label: x.name
            }
        })
    }

    private buildAccountCollectionIdFor(accountDim: AccountDimSmallDTO) {
        return "acc-{0}".format(accountDim.accountDimId.toString())
    }
}