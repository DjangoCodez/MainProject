import { IPromise } from "angular";
import { ICommonCustomerService } from "../../../../../../Common/Customer/CommonCustomerService";
import { AccountDimSmallDTO } from "../../../../../../Common/Models/AccountDimDTO";
import { ProjectDTO } from "../../../../../../Common/Models/ProjectDTO";
import { AccountIntervalSelectionDTO, BoolSelectionDTO, DateRangeSelectionDTO, AccountFilterSelectionDTO, AccountFilterSelectionsDTO, TextSelectionDTO, IdSelectionDTO } from "../../../../../../Common/Models/ReportDataSelectionDTO";
import { ReportUserSelectionDTO } from "../../../../../../Common/Models/ReportDTOs";
import { SmallGenericType } from "../../../../../../Common/Models/SmallGenericType";
import { IBoolSelectionDTO, IIdSelectionDTO, ISmallGenericType, IYearAndPeriodSelectionDTO } from "../../../../../../Scripts/TypeLite.Net4";
import { SoeCategoryType } from "../../../../../../Util/CommonEnumerations";
import { Constants } from "../../../../../../Util/Constants";
import { ICoreService } from "../../../../../Services/CoreService";
import { ITranslationService } from "../../../../../Services/TranslationService";
import { IReportDataService } from "../../../ReportDataService";
import { SelectionCollection } from "../../../SelectionCollection";
import { NamedFilterRange } from "../../../Selections/AccountRangeSelection/AccountRangeSelectionComponent";

export class ProjekttidReport {
    selectedProject: SmallGenericType;
    public static component(): ng.IComponentOptions {
        return {
            controller: ProjekttidReport,
            templateUrl: soeConfig.baseUrl + "Core/RightMenu/ReportMenu/ReportTypes/Billing/ProjekttidReport/ProjekttidReportView.html",
            bindings: {
                userSelection: "=",
                selections: "<"
            }
        } as ng.IComponentOptions;
    }

    public static componentKey = "projekttidReport";

    selectableSorting: ISmallGenericType[];
    selectableInventoryBalanceList: any[];
    private selectableCustomerCategorySorting: ISmallGenericType[];
    selectableCodeList: any[];

    private onSelected: (_: { selection: IYearAndPeriodSelectionDTO }) => void = angular.noop;
    private userSelection: ReportUserSelectionDTO;
    private selections: SelectionCollection;

    private selectableRangeNames: AccountDimSmallDTO[];
    private rangeFilters: NamedFilterRange[];

    private projectSelected: BoolSelectionDTO;
    private separateAccountDimSelected: BoolSelectionDTO;
    private dateIsSelectedDTO: BoolSelectionDTO;
    private selectedDateRange: DateRangeSelectionDTO;
    
    private showRange: boolean;

    private accountPeriodFrom: AccountIntervalSelectionDTO;
    private accountPeriodTo: AccountIntervalSelectionDTO;
    private projectReportTitle = "";

    private selectedSortItem: IdSelectionDTO;

    private actorNrFrom: SmallGenericType;
    private actorNrTo: SmallGenericType;

    projectsDict: ISmallGenericType[] = [];
    employeesDict: any[] = [];
    projects: any[] = [];
    
    private projectNrFrom: any;
    private projectNrTo: any;
    private employeeNrFrom: any;
    private employeeNrTo: any;

    private customerDict: SmallGenericType[];

    //@ngInject
    constructor(private $scope: ng.IScope, $timeout: ng.ITimeoutService, private translationService: ITranslationService, private reportDataService: IReportDataService, private coreService: ICoreService, private commonCustomerService: ICommonCustomerService,) {

        this.rangeFilters = new Array<NamedFilterRange>();
        this.$scope.$watch(() => this.rangeFilters, () => {
            this.saveRangedFilters();
        }, true);

        this.$scope.$watch(() => this.selections, (newVal, oldVal) => {
            if (!newVal)
                return;
        });

        this.$scope.$watch(() => this.userSelection, (newVal, oldVal) => {
            if (!newVal)
                return;
            this.setSavedUserFilters(newVal);
        });

        const keys = [
            "billing.project.central.projectreports",
            "common.report.daterangeselection",
            "common.report.seperatereport",
            "common.report.accountselection",
            "common.report.distributionreport",
            "common.report.standardselection"
        ]

        this.translationService.translateMany(keys).then(terms => {
            this.projectReportTitle = terms["billing.project.central.projectreports"];
        });
    }

    private saveRangedFilters() {
        const transformed = new Array<AccountFilterSelectionDTO>();
        this.rangeFilters.forEach(filter => {
            if (filter.selectedSelection) {

                if (filter.selectedSelection.accounts.length == 0) {
                    filter.selectionFrom = (_.isEmpty(filter.accountFrom) ? '' : filter.accountFrom).toString();
                    filter.selectionTo = (_.isEmpty(filter.accountTo) ? '' : filter.accountTo).toString();
                }

                const filterDTO = new AccountFilterSelectionDTO(filter.selectedSelection.accountDimId, filter.selectionFrom, filter.selectionTo);
                transformed.push(filterDTO);
            }
        });
        const filtersDTO = new AccountFilterSelectionsDTO(transformed);

        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_NAMED_FILTER_RANGES, filtersDTO);
    }

    private setSavedUserFilters(savedValues: ReportUserSelectionDTO) {
        let namedFilters = savedValues.getNamedRangeSelection();
        if (namedFilters.length > 0) {
            if (!this.selectableRangeNames || this.selectableRangeNames.length == 0) {
                this.getAvailableRanges().then(accountDims => {
                    this.selectableRangeNames = accountDims;
                    this.handleSavedRanges(namedFilters);
                })
            }
            else {
                this.handleSavedRanges(namedFilters);
            }
        } else {
            if (!this.selectableRangeNames || this.selectableRangeNames.length == 0) {
                this.getAvailableRanges().then(accountDims => {
                    this.selectableRangeNames = accountDims;
                    this.addFilter();
                })
            }
        }

        if (this.projectsDict.length == 0) {
            this.reportDataService.getProjects(true, true, false, true, false, 0).then((x) => {
                this.projects = x;
                this.projectsDict.push({ id: 0, name: "" });
                for (var i = 0; i < this.projects.length; i++) {
                    var row = this.projects[i];
                    //if ((!this.projectId || row.projectId !== this.projectId) && row.status < 4)
                    this.projectsDict.push({ id: row.projectId, name: row.projectId + ' ' + row.number + ' ' + row.name });
                }

                this.projectNrFrom = this.userSelection.getIdSelection(Constants.REPORTMENU_SELECTION_KEY_PROJECT_NUMBER_FROM) != null ? _.find(this.projectsDict, d => d.id === this.userSelection.getIdSelection(Constants.REPORTMENU_SELECTION_KEY_PROJECT_NUMBER_FROM).id) : null;
                if (this.projectNrFrom != null)
                    this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_PROJECT_NUMBER_FROM, new IdSelectionDTO(this.projectNrFrom.id));
                this.projectNrTo = this.userSelection.getIdSelection(Constants.REPORTMENU_SELECTION_KEY_PROJECT_NUMBER_TO) != null ? _.find(this.projectsDict, d => d.id === this.userSelection.getIdSelection(Constants.REPORTMENU_SELECTION_KEY_PROJECT_NUMBER_TO).id) : null;
                if (this.projectNrTo != null)
                    this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_PROJECT_NUMBER_TO, new IdSelectionDTO(this.projectNrTo.id));
            });

        }
        else {
            this.projectNrFrom = this.userSelection.getIdSelection(Constants.REPORTMENU_SELECTION_KEY_PROJECT_NUMBER_FROM) != null ? _.find(this.projectsDict, d => d.id === this.userSelection.getIdSelection(Constants.REPORTMENU_SELECTION_KEY_PROJECT_NUMBER_FROM).id) : null;
            if (this.projectNrFrom != null)
                this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_PROJECT_NUMBER_FROM, new IdSelectionDTO(this.projectNrFrom.id));
            this.projectNrTo = this.userSelection.getIdSelection(Constants.REPORTMENU_SELECTION_KEY_PROJECT_NUMBER_TO) != null ? _.find(this.projectsDict, d => d.id === this.userSelection.getIdSelection(Constants.REPORTMENU_SELECTION_KEY_PROJECT_NUMBER_TO).id) : null;
            if (this.projectNrTo != null)
                this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_PROJECT_NUMBER_TO, new IdSelectionDTO(this.projectNrTo.id));
        }

        if (this.employeesDict.length == 0) {
            this.reportDataService.getEmployeesDict(true, true, false, true).then((result) => {
                _.forEach(result, (employee: any) => {
                    var substrings = employee.name.split(')');
                    this.employeesDict.push({ id: employee.id, name: employee.name, value: substrings[0].substring(1) });
                });

                this.employeeNrFrom = this.userSelection.getTextSelection(Constants.REPORTMENU_SELECTION_KEY_EMPLOYEE_NUMBER_FROM) != null ? _.find(this.employeesDict, d => d.value === this.userSelection.getTextSelection(Constants.REPORTMENU_SELECTION_KEY_EMPLOYEE_NUMBER_FROM).text) : null;
                if (this.employeeNrFrom != null)
                    this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_EMPLOYEE_NUMBER_FROM, new TextSelectionDTO(this.employeeNrFrom.value));
                this.employeeNrTo = this.userSelection.getTextSelection(Constants.REPORTMENU_SELECTION_KEY_EMPLOYEE_NUMBER_TO) ? _.find(this.employeesDict, d => d.value === this.userSelection.getTextSelection(Constants.REPORTMENU_SELECTION_KEY_EMPLOYEE_NUMBER_TO).text) : null;
                if (this.employeeNrTo != null)
                    this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_EMPLOYEE_NUMBER_TO, new TextSelectionDTO(this.employeeNrTo.value));
            });

        }
        else {
            this.employeeNrFrom = this.userSelection.getTextSelection(Constants.REPORTMENU_SELECTION_KEY_EMPLOYEE_NUMBER_FROM) != null ? _.find(this.employeesDict, d => d.value === this.userSelection.getTextSelection(Constants.REPORTMENU_SELECTION_KEY_EMPLOYEE_NUMBER_FROM).text) : null;
            if (this.employeeNrFrom != null)
                this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_EMPLOYEE_NUMBER_FROM, new TextSelectionDTO(this.employeeNrFrom.value));
            this.employeeNrTo = this.userSelection.getTextSelection(Constants.REPORTMENU_SELECTION_KEY_EMPLOYEE_NUMBER_TO) ? _.find(this.employeesDict, d => d.value === this.userSelection.getTextSelection(Constants.REPORTMENU_SELECTION_KEY_EMPLOYEE_NUMBER_TO).text) : null;
            if (this.employeeNrTo != null)
                this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_EMPLOYEE_NUMBER_TO, new TextSelectionDTO(this.employeeNrTo.value));
        }

        this.selectedSortItem = this.userSelection.getIdSelection(Constants.REPORTMENU_SELECTION_KEY_CUSTOMER_CATEGORY);
        this.selectedDateRange = savedValues.getDateRangeSelection();
        
        if (this.userSelection.getTextSelection(Constants.REPORTMENU_SELECTION_KEY_ACTOR_NUMBER_FROM) != null)
            this.actorNrFrom = _.find(this.customerDict, d => d.name.split(' ')[0] === this.userSelection.getTextSelection(Constants.REPORTMENU_SELECTION_KEY_ACTOR_NUMBER_FROM).text);

        if (this.userSelection.getTextSelection(Constants.REPORTMENU_SELECTION_KEY_ACTOR_NUMBER_TO) != null)
            this.actorNrTo = _.find(this.customerDict, d => d.name.split(' ')[0] === this.userSelection.getTextSelection(Constants.REPORTMENU_SELECTION_KEY_ACTOR_NUMBER_TO).text);

    }

    private handleSavedRanges(namedFilters: AccountFilterSelectionDTO[]) {
        const savedFilters = new Array<NamedFilterRange>();
        namedFilters.forEach(x => {
            const filter = new NamedFilterRange(this.selectableRangeNames);
            if (this.selectableRangeNames) {
                filter.selectedSelection = this.selectableRangeNames.find(y => y.accountDimId === x.id);
            }
            filter.selectionFrom = x.from;
            filter.selectionTo = x.to;
            filter.accountFrom = _.find(filter.selectedSelection.accounts, (account) => account.accountNr == x.from);
            filter.accountTo = _.find(filter.selectedSelection.accounts, (account) => account.accountNr == x.to);
            savedFilters.push(filter);

        });
        this.rangeFilters = savedFilters;
    }

    public $onInit() {
        this.handleAvailableRanges();
        this.loadProjects();
        this.getCustomerCategory();
        this.getCustomers();
    }

    private getCustomers() {
        this.customerDict = [];
        this.coreService.getCustomers(true, false, true).then((customers: ISmallGenericType[]) => {
            this.customerDict = customers;
        });
    }

    private handleAvailableRanges() {
        this.getAvailableRanges().then(accountDims => {

            this.selectableRangeNames = accountDims;
            this.addFilter();
        })
    }

    private getAvailableRanges(): IPromise<AccountDimSmallDTO[]> {
        return this.reportDataService.getAccountDimsSmall(false, false, true, false);
    }

    private addFilter() {
        this.rangeFilters = [];
        const row = new NamedFilterRange(this.selectableRangeNames);
        row.selectedSelection = this.selectableRangeNames[0];
        this.rangeFilters.push(row);
        if (this.rangeFilters.length === 1) {
            this.rangeFilters[0].selectedSelection.accounts = _.sortBy(this.rangeFilters[0].selectedSelection.accounts, 'accountNr');
            _.forEach(this.rangeFilters[0].selectedSelection.accounts, (a) => {
                a.name = a.numberName;
            });
        }
    }

    private removeFilter(selection: number) {
        this.rangeFilters.splice(selection, 1);
    }

    public onSeparateAccountDimSelected(selection: IBoolSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_SEPARATE_ACCOUNT_DIM, selection);
    }


    private getCustomerCategory() {
        this.selectableCustomerCategorySorting = [];
        return this.coreService.getCategoriesDict(SoeCategoryType.Customer, true).then(data => {
            this.selectableCustomerCategorySorting = data;
        });
    }

    public onCustomerCategoryOrderSelectionChanged(selection: IIdSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_CUSTOMER_CATEGORY, selection);
    }

    public customerIDChangedFrom(selection: SmallGenericType) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_ACTOR_NUMBER_FROM, new TextSelectionDTO(selection.name.split(' ')[0]));

        if (!this.actorNrTo) {
            this.actorNrTo = selection;
            this.customerIDChangedTo(selection);
        }
    }

    public customerIDChangedTo(selection: SmallGenericType) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_ACTOR_NUMBER_TO, new TextSelectionDTO(selection.name.split(' ')[0]));
    }

    public onDateRangeSelected(dateRange: DateRangeSelectionDTO) {
        dateRange.useMinMaxIfEmpty = true;
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_DATE_RANGE, dateRange);
    }

    public onDateSelectionSelected(selection: IBoolSelectionDTO) {
        this.showRange = !selection.value;
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_DATE_SELECTION_CHOOSEN, selection);
    }

    public onProjectFromSelected(selection: any) {

        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_PROJECT_NUMBER_FROM, new TextSelectionDTO(selection.value));

        if (!this.projectNrTo) {
            this.projectNrTo = selection;
            this.onProjectToSelected(selection);
        }
    }

    public onProjectToSelected(selection: any) {

        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_PROJECT_NUMBER_TO, new TextSelectionDTO(selection.value));
    }

    public onEmployeeFromSelected(selection: any) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_EMPLOYEE_NUMBER_FROM, new TextSelectionDTO(selection.value));

        if (!this.employeeNrTo) {
            this.employeeNrTo = selection;
            this.onEmployeeToSelected(selection);
        }
    }

    public onEmployeeToSelected(selection: any) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_EMPLOYEE_NUMBER_TO, new TextSelectionDTO(selection.value));
    }

    private loadProjects() {

        this.projectsDict = [];
        return this.reportDataService.getProjects(true, true, false, true, false, 0).then((x) => {

            this.projects = x;
            this.projectsDict.push({ id: 0, name: "" });
            for (var i = 0; i < this.projects.length; i++) {
                var row = this.projects[i];
                //if ((!this.projectId || row.projectId !== this.projectId) && row.status < 4)
                this.projectsDict.push({ id: row.projectId, name: row.projectId + ' ' + row.number + ' ' + row.name });
            }
        });
    }

}
