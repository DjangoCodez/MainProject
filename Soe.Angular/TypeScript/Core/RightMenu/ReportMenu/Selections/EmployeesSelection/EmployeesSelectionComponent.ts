import { IReportDataService } from "../../ReportDataService";
import { IEmployeeSelectionDTO, ISmallGenericType } from "../../../../../Scripts/TypeLite.Net4";
import { EmployeeSelectionDTO } from "../../../../../Common/Models/ReportDataSelectionDTO";
import { AllEmployeesDataSource, FilteredEmployeesDataSource, IEmployeeDataSource, IDataSourceFilter, IDataSourceFilterCollection } from "./EmployeesDataSource";
import { ICoreService } from "../../../../Services/CoreService";
import { ITranslationService } from "../../../../Services/TranslationService";
import { SoeReportTemplateType, TermGroup_EmployeeSelectionAccountingType } from "../../../../../Util/CommonEnumerations";

interface EmployeeSelectModel {
    id: number;
    label: string;
}

interface FilterCollectionGroupsModel {
    id: string;
    collections: IDataSourceFilterCollection[];
}

type SingleSelectModel = string;
type MultiSelectModel = { id: string }

interface FilterCollectionModel extends IDataSourceFilterCollection {
    selectedIds: SingleSelectModel | MultiSelectModel[];
}

export class EmployeesSelection {
    public static component(): ng.IComponentOptions {
        const options: ng.IComponentOptions = {
            controller: EmployeesSelection,
            templateUrl: soeConfig.baseUrl + "Core/RightMenu/ReportMenu/Selections/EmployeesSelection/EmployeesSelectionView.html",
            bindings: {
                onSelected: "&",
                labelKey: "@",
                enableFilters: "<",
                fromDate: "<",
                toDate: "<",
                showInactive: "<",
                showOnlyInactive: "<",
                showEnded: "<",
                showVacant: "<",
                showHidden: "<",
                showSecondary: "<",
                hideAccountingType: "<",
                selectableAccountingTypes: "@",
                timePeriodIds: "<",
                reportTemplateType: "<",
                userSelectionInput: "="
            }
        };

        return options;
    }
    public static componentKey = "employeesSelection";

    private dataSource: IEmployeeDataSource;
    private selectedInactive: boolean;
    private selectedOnlyInactive: boolean;
    private selectedEnded: boolean;
    private selectedVacant: boolean;
    private selectedHidden: boolean;
    private selectedSecondary: boolean;
    private selectedAccountingType: TermGroup_EmployeeSelectionAccountingType = TermGroup_EmployeeSelectionAccountingType.EmployeeCategory;
    private availableAccountingTypes: ISmallGenericType[] = [];
    private availableSelectableEmployees: EmployeeSelectModel[] = [];
    private selectedEmployees: EmployeeSelectModel[] = [];
    private filteredAccountIds: number[];
    private filteredEmployeeGroupIds: number[];
    private filteredCategoryIds: number[];
    private filteredVacationGroupIds: number[];
    private filteredPayrollGroupIds: number[];
    private groupedFilterCollections: FilterCollectionGroupsModel[];

    //bindings properties
    private enableFilters: boolean = false;
    private onSelected: (_: { employeeSelection: IEmployeeSelectionDTO }) => void = angular.noop;
    private fromDate: Date = new Date();
    private toDate: Date = new Date();
    private showInactive: boolean = false;
    private showOnlyInactive: boolean = false;
    private showEnded: boolean = false;
    private showVacant: boolean = false;
    private showHidden: boolean = false;
    private showSecondary: boolean = false;
    private hideAccountingType: boolean = false;
    private selectableAccountingTypes: string;
    private timePeriodIds: number[];
    private reportTemplateType: SoeReportTemplateType;
    private userSelectionInput: EmployeeSelectionDTO;

    // Terms
    private terms: { [index: string]: string; };
    private accountingTypeHelpText: string;

    // Flags
    private employeeFilterDirty: boolean = false;
    private populating: boolean = false;
    private delaySetSavedUserSelection: boolean = false;

    private get useAutoFilterOnEmployee(): boolean {
        return this.selectedAccountingType === TermGroup_EmployeeSelectionAccountingType.EmployeeCategory || this.selectedAccountingType === TermGroup_EmployeeSelectionAccountingType.EmployeeAccount;
    }

    //@ngInject
    constructor(
        private $scope: ng.IScope,
        private $q: ng.IQService,
        private $timeout: ng.ITimeoutService,
        private reportDataService: IReportDataService,
        private coreService: ICoreService,
        private translationService: ITranslationService) {
        
        this.$scope.$watch(() => this.userSelectionInput, () => {
            this.setSavedUserSelection();
        });
    }

    public $onInit() {
        this.loadTerms();
        
        this.dataSource = this.enableFilters
            ? new FilteredEmployeesDataSource(this.$q, this.reportDataService, this.coreService, this.translationService)
            : new AllEmployeesDataSource(this.$q, this.reportDataService);

        // Default if not specified for each report
        if (!this.selectableAccountingTypes)
            this.selectableAccountingTypes = "0,1,2";

        this.dataSource.setDateRange(this.fromDate, this.toDate);
        this.dataSource.setTimePeriodIds(this.timePeriodIds);
        this.dataSource.setReportTemplateType(this.reportTemplateType);

        this.updateFilters().then(() => {
            this.preselectAccountFilter();
            this.initPopulateEmployees(true);
        });
    }

    public $onChanges(objChanged) {
        if (!this.dataSource)
            return;

        this.dataSource.setDateRange(this.fromDate, this.toDate);
        this.dataSource.setTimePeriodIds(this.timePeriodIds);
        this.dataSource.setReportTemplateType(this.reportTemplateType);
        this.dataSource.setAccountingType(this.selectedAccountingType);
        this.dataSource.setIncludeInactive(this.selectedInactive);
        this.dataSource.setOnlyInactive(this.selectedOnlyInactive);
        this.dataSource.setIncludeEnded(this.selectedEnded);
        this.dataSource.setIncludeVacant(this.selectedVacant);
        this.dataSource.setIncludeHidden(this.selectedHidden);
        this.dataSource.setIncludeSecondary(this.selectedSecondary);

        this.availableSelectableEmployees = [];
        this.updateFilters().then(() => {
            this.preselectAccountFilter();
            this.initPopulateEmployees(true);
        });

    }

    private loadTerms(): ng.IPromise<any> {
        let keys: string[] = [
            "common.accountingtype.info.employeecategory",
            "common.accountingtype.info.employeeaccount",
            "common.accountingtype.info.employmentaccountinternal",
            "common.accountingtype.info.timescheduletemplateblock",
            "common.accountingtype.info.timescheduletemplateblockaccount",
            "common.accountingtype.info.timepayrolltransactionaccount"
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
            this.setAccountingTypeHelpText();
        });
    }

    private setSavedUserSelection() {
        if (!this.userSelectionInput)
            return;

        this.selectedInactive = this.userSelectionInput.includeInactive;
        this.selectedOnlyInactive = this.userSelectionInput.onlyInactive;
        this.selectedEnded = this.userSelectionInput.includeEnded;
        this.selectedVacant = this.userSelectionInput.includeVacant;
        this.selectedHidden = this.userSelectionInput.includeHidden;
        this.selectedSecondary = this.userSelectionInput.includeSecondary;

        if (this.availableSelectableEmployees.length === 0) {
            this.delaySetSavedUserSelection = true;
            return;
        }

        this.delaySetSavedUserSelection = false;

        if (this.dataSource) {
            this.dataSource.setDateRange(this.fromDate, this.toDate);
            this.dataSource.setTimePeriodIds(this.timePeriodIds);

            if (this.availableAccountingTypes && this.availableAccountingTypes.length > 0) {
                let accTypeId: number = this.userSelectionInput.accountingType || ((<FilteredEmployeesDataSource>this.dataSource).useAccountHierarchy ? TermGroup_EmployeeSelectionAccountingType.EmployeeAccount : TermGroup_EmployeeSelectionAccountingType.EmployeeCategory);
                if (!this.availableAccountingTypes.find(t => t.id == accTypeId))
                    accTypeId = this.availableAccountingTypes[0].id;
                this.dataSource.setAccountingType(accTypeId);
                this.selectedAccountingType = accTypeId;
                this.setAccountingTypeHelpText();
            }
            this.dataSource.setIncludeInactive(this.userSelectionInput.includeInactive);
            this.selectedInactive = this.userSelectionInput.includeInactive;
            this.dataSource.setOnlyInactive(this.userSelectionInput.onlyInactive);
            this.selectedOnlyInactive = this.userSelectionInput.onlyInactive;
            this.dataSource.setIncludeEnded(this.userSelectionInput.includeEnded);
            this.selectedEnded = this.userSelectionInput.includeEnded;
            this.selectedVacant = this.userSelectionInput.includeVacant;
            this.selectedHidden = this.userSelectionInput.includeHidden;
            this.selectedSecondary = this.userSelectionInput.includeSecondary;

            if (this.enableFilters) {
                if (this.userSelectionInput.accountIds && this.userSelectionInput.accountIds.length > 0) {
                    _.forEach(this.dataSource.accountDimensions, dim => {
                        this.setAccountMultiFilter('account', 'acc-' + dim.accountDimId, this.userSelectionInput.accountIds)
                    });
                }

                if (this.userSelectionInput.employeeGroupIds)
                    this.setMultiFilter('employeeGroups', this.userSelectionInput.employeeGroupIds);
                if (this.userSelectionInput.categoryIds)
                    this.setMultiFilter('categories', this.userSelectionInput.categoryIds);
                if (this.userSelectionInput.vacationGroupIds)
                    this.setMultiFilter('vacationGroups', this.userSelectionInput.vacationGroupIds);
                if (this.userSelectionInput.payrollGroupIds)
                    this.setMultiFilter('payrollGroups', this.userSelectionInput.payrollGroupIds);
            }
        }
    }

    private updateFilters(): ng.IPromise<any> {
        const mapSelectedIds = (c: IDataSourceFilterCollection) => {
            if (c.canMultiSelect)
                return c.selectedFilters.map(f => <MultiSelectModel>{ id: f.id });
            else
                return c.selectedFilters.length > 0 ? c.selectedFilters[0].id : null;
        };

        return this.dataSource.getSupportedFilters().then((filterCollections) => {
            this.availableAccountingTypes = [];
            let useAccountHierarchy: boolean = (<FilteredEmployeesDataSource>this.dataSource).useAccountHierarchy;

            let specifiedTypes: string[] = this.selectableAccountingTypes.split(',');
            _.forEach(specifiedTypes, typeId => {
                let type: ISmallGenericType = _.find(this.dataSource.accountingTypes, t => t.id === parseInt(typeId, 10));
                if (type) {
                    let isValid: boolean = false;
                    switch (type.id) {
                        case TermGroup_EmployeeSelectionAccountingType.EmployeeCategory:
                            isValid = !useAccountHierarchy;
                            break;
                        case TermGroup_EmployeeSelectionAccountingType.EmployeeAccount:
                        case TermGroup_EmployeeSelectionAccountingType.TimeScheduleTemplateBlock:
                            isValid = useAccountHierarchy;
                            break;
                        case TermGroup_EmployeeSelectionAccountingType.EmploymentAccountInternal:
                        case TermGroup_EmployeeSelectionAccountingType.TimeScheduleTemplateBlockAccount:
                        case TermGroup_EmployeeSelectionAccountingType.TimePayrollTransactionAccount:
                            isValid = true;
                            break;
                    }

                    if (isValid)
                        this.availableAccountingTypes.push(type);
                }
            });

            if (this.selectedAccountingType == TermGroup_EmployeeSelectionAccountingType.EmployeeCategory && useAccountHierarchy) {
                this.selectedAccountingType = TermGroup_EmployeeSelectionAccountingType.EmployeeAccount;
                this.accountingTypeChanged();
            }

            this.groupedFilterCollections = _(filterCollections).groupBy((c: IDataSourceFilterCollection) => c.category).map((c, k) => <FilterCollectionGroupsModel>{
                collections: c.map(o => <FilterCollectionModel>Object.assign({ selectedIds: mapSelectedIds(o) }, o)),
                id: k
            }).value();
        });
    }

    private selectFilter(categoryIndex: number, collectionIndex: number, filterId: SingleSelectModel) {
        if (!this.groupedFilterCollections)
            return;

        const filterCollection = this.groupedFilterCollections[categoryIndex].collections[collectionIndex];
        const selectedFilter = filterCollection.availableFilters.find(f => filterId === f.id);

        this.applySelectedFilters(filterCollection.id, selectedFilter ? [selectedFilter] : []);
    }

    private setFilter(categoryIndex: number, collectionId: string, filterId: SingleSelectModel) {
        if (this.groupedFilterCollections) {
            const filterCollection = this.groupedFilterCollections[categoryIndex].collections.find(c => c.id === collectionId);
            if (filterCollection) {
                filterCollection.selectedFilters = [];
                let selectedFilter = _.find(filterCollection.availableFilters, c => c.id === filterId);
                if (selectedFilter) {
                    filterCollection.selectedFilters.push(selectedFilter);
                    filterCollection['selectedIds'] = selectedFilter.id;
                    this.dataSource.applyFilter(collectionId, [selectedFilter]);
                }
            }
        }
    }

    private selectMultiFilter(categoryIndex: number, collectionIndex: number, filterIds: MultiSelectModel[]) {
        const filterCollection = this.groupedFilterCollections[categoryIndex].collections[collectionIndex];
        const selectedFilters = filterCollection.availableFilters.filter(f => filterIds.some(i => i.id === f.id));
        this.applySelectedFilters(filterCollection.id, selectedFilters);
    }

    private setMultiFilter(collectionId: string, filterIds: number[]) {
        let filterCollection;
        _.forEach(this.groupedFilterCollections, group => {
            filterCollection = group.collections.find(c => c.id === collectionId);
            if (filterCollection)
                return false;
        });

        if (filterCollection) {
            const selectedFilters = filterCollection.availableFilters.filter(f => filterIds.some(i => i === f.originId));
            filterCollection.selectedFilters = [];
            filterCollection['selectedIds'] = []
            if (filterIds.length > 0) {
                _.forEach(filterIds, filterId => {
                    let selectedFilter = _.find(filterCollection.availableFilters, c => c.originId === filterId);
                    if (selectedFilter) {
                        filterCollection.selectedFilters.push(selectedFilter);
                        filterCollection['selectedIds'].push(selectedFilter);
                    }
                });
            }
            this.applySelectedFilters(filterCollection.id, selectedFilters);
        }
    }

    private setAccountMultiFilter(groupId: string, collectionId: string, filterIds: number[]) {
        const filterGroup = _.find(this.groupedFilterCollections, c => c.id === groupId);
        if (filterGroup) {
            const filterCollection = filterGroup.collections.find(c => c.id === collectionId);
            const selectedFilters = filterCollection.availableFilters.filter(f => filterIds.some(i => i === f.originId));
            this.applySelectedFilters(filterCollection.id, selectedFilters);
        }
    }

    private applySelectedFilters(collectionId: string, filters: IDataSourceFilter[]) {
        this.availableSelectableEmployees = [];

        let filterFetcher = this.$q.resolve();
        const needToUpdateFilters = this.dataSource.applyFilter(collectionId, filters);
        if (needToUpdateFilters)
            filterFetcher = this.updateFilters();

        filterFetcher.then(() => {
            this.initPopulateEmployees();
        });
    }

    private initPopulateEmployees(forceUpdate: boolean = false) {
        if (this.useAutoFilterOnEmployee)
            this.populateEmployees(forceUpdate);
        else
            this.employeeFilterDirty = true;
    }

    private populateEmployees = _.debounce((forceUpdate: boolean = false) => {
        let deferral = this.$q.defer();

        if (!this.availableSelectableEmployees || this.availableSelectableEmployees.length < 1 || forceUpdate) {
            this.populating = true;
            this.employeeFilterDirty = false;
            this.dataSource.populate().then(() => {
                this.availableSelectableEmployees = this.dataSource.employees.map(e => <EmployeeSelectModel>{ id: e.id, label: e.name });
                this.filteredAccountIds = this.dataSource.filteredAccountIds;
                this.filteredEmployeeGroupIds = this.dataSource.filteredEmployeeGroupIds
                this.filteredCategoryIds = this.dataSource.filteredCategoryIds;
                this.filteredPayrollGroupIds = this.dataSource.filteredPayrollGroupIds;
                this.filteredVacationGroupIds = this.dataSource.filteredVacationGroupIds;
                this.selectedEmployees = this.selectedEmployees.filter(s => this.availableSelectableEmployees.some(a => s.id === a.id));
                if (this.userSelectionInput && this.userSelectionInput.employeeIds.length > 0)
                    this.selectedEmployees = this.availableSelectableEmployees.filter(s => this.userSelectionInput.employeeIds.some(a => s.id === a));
            }).then(() => {
                this.propagateEmployeeSelection();
                this.populating = false;
                if (this.delaySetSavedUserSelection)
                    this.setSavedUserSelection();
                else
                    this.userSelectionInput = null;

                deferral.resolve();
            });
        } else {
            deferral.resolve();
        }

        return deferral.promise;
    }, 200, { leading: false, trailing: true })

    private preselectAccountFilter() {
        let preselectedDims = this.dataSource.accountDimensions.filter(d => d['hidden']);
        if (preselectedDims && preselectedDims.length > 0) {
            let filteredAccountIds = _.last(preselectedDims).filteredAccounts.map(a => a.accountId);
            if (filteredAccountIds.length > 0) {
                let items: MultiSelectModel[] = [];
                _.forEach(filteredAccountIds, accountId => {
                    items.push({ id: 'acc-' + accountId });
                });

                let index = this.dataSource.accountDimensions.findIndex(d => d.accountDimId === _.last(preselectedDims).accountDimId);
                this.selectMultiFilter(0, index, items);
            }
        }
    }

    private boolChanged() {
        this.$timeout(() => {
            this.dataSource.setIncludeInactive(this.selectedInactive);
            this.dataSource.setOnlyInactive(this.selectedOnlyInactive);
            this.dataSource.setIncludeEnded(this.selectedEnded);
            this.dataSource.setIncludeVacant(this.selectedVacant);
            this.dataSource.setIncludeHidden(this.selectedHidden);
            this.dataSource.setIncludeSecondary(this.selectedSecondary);
            this.initPopulateEmployees(true);
        });
    }

    private accountingTypeChanged() {
        this.$timeout(() => {
            this.dataSource.setAccountingType(this.selectedAccountingType);
            this.setAccountingTypeHelpText();
            this.updateFilters();
            this.preselectAccountFilter();
            this.initPopulateEmployees(true);
        });
    }

    private setAccountingTypeHelpText() {
        let labelKey: string = '';

        switch (this.selectedAccountingType) {
            case TermGroup_EmployeeSelectionAccountingType.EmployeeCategory:
                labelKey = "common.accountingtype.info.employeecategory";
                break;
            case TermGroup_EmployeeSelectionAccountingType.EmployeeAccount:
                labelKey = "common.accountingtype.info.employeeaccount";
                break;
            case TermGroup_EmployeeSelectionAccountingType.EmploymentAccountInternal:
                labelKey = "common.accountingtype.info.employmentaccountinternal";
                break;
            case TermGroup_EmployeeSelectionAccountingType.TimeScheduleTemplateBlock:
                labelKey = "common.accountingtype.info.timescheduletemplateblock";
                break;
            case TermGroup_EmployeeSelectionAccountingType.TimeScheduleTemplateBlockAccount:
                labelKey = "common.accountingtype.info.timescheduletemplateblockaccount";
                break;
            case TermGroup_EmployeeSelectionAccountingType.TimePayrollTransactionAccount:
                labelKey = "common.accountingtype.info.timepayrolltransactionaccount";
                break;
        }

        if (labelKey)
            this.accountingTypeHelpText = this.terms[labelKey];
    }

    private propagateEmployeeSelection() {
        const employeeIds = _.map(this.selectedEmployees, m => m.id);

        this.onSelected({
            employeeSelection: new EmployeeSelectionDTO(employeeIds, this.filteredAccountIds, this.filteredCategoryIds, this.filteredEmployeeGroupIds, this.filteredPayrollGroupIds, this.filteredVacationGroupIds, this.selectedInactive, this.selectedOnlyInactive, this.selectedEnded, this.selectedAccountingType, this.selectedVacant, this.selectedHidden, this.selectedSecondary)
        });

        if (this.userSelectionInput && !this.delaySetSavedUserSelection)
            this.userSelectionInput = null;
    }
}