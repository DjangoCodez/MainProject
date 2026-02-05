import { IPromise } from "angular";
import { AccountDimSmallDTO } from "../../../../../../Common/Models/AccountDimDTO";
import { AccountFilterSelectionDTO, AccountFilterSelectionsDTO, BoolSelectionDTO, DateRangeSelectionDTO, IdSelectionDTO, TextSelectionDTO } from "../../../../../../Common/Models/ReportDataSelectionDTO";
import { ReportUserSelectionDTO } from "../../../../../../Common/Models/ReportDTOs";
import { SmallGenericType } from "../../../../../../Common/Models/SmallGenericType";
import { IBoolSelectionDTO, IIdSelectionDTO, ISmallGenericType, ITextSelectionDTO } from "../../../../../../Scripts/TypeLite.Net4";
import { SoeCategoryType } from "../../../../../../Util/CommonEnumerations";
import { Constants } from "../../../../../../Util/Constants";
import { ICoreService } from "../../../../../Services/CoreService";
import { IReportDataService } from "../../../ReportDataService";
import { SelectionCollection } from "../../../SelectionCollection";
import { NamedFilterRange } from "../../../Selections/AccountRangeSelection/AccountRangeSelectionComponent";

export class InvoiceStatisticsReport {
    selectableSorting: ISmallGenericType[];
    selectableInventoryBalanceList: any[];
    selectableCodeList: any[];
    private selectableCustomerCategorySorting: ISmallGenericType[];

    public static component(): ng.IComponentOptions {
        return {
            controller: InvoiceStatisticsReport,
            templateUrl: soeConfig.baseUrl + "Core/RightMenu/ReportMenu/ReportTypes/Billing/InvoiceStatisticsReport/InvoiceStatisticsReportView.html",
            bindings: {
                userSelection: "=",
                selections: "<",
            }
        } as ng.IComponentOptions;
    }

    public static componentKey = "invoiceStatisticsReport";

    private userSelection: ReportUserSelectionDTO;
    private selections: SelectionCollection;
    private selectedDateRange: DateRangeSelectionDTO;
    private selectedPaymentDateRange: DateRangeSelectionDTO;
    private selectedCreatedDateRange: DateRangeSelectionDTO;
    private includeInvoicedOrders: BoolSelectionDTO;
    private selectableRangeNames: AccountDimSmallDTO[];
    private rangeFilters: NamedFilterRange[];
    private showRange: boolean;
    private selectedSortItem: IdSelectionDTO;
    private actorNrFrom: SmallGenericType;
    private actorNrTo: SmallGenericType;
    projectsDict: ISmallGenericType[] = [];
    employeesDict: any[] = [];
    projects: any[] = [];
    projectId: number;
    private projectNrFrom: ITextSelectionDTO;
    private projectNrTo: ITextSelectionDTO;
    private articleNumberFrom: ITextSelectionDTO;
    private articleNumberTo: ITextSelectionDTO;
    private projectNrHandler: boolean = true;
    private employeeNrFrom: any;
    private employeeNrTo: any;
    private customerDict: SmallGenericType[];

    //@ngInject
    constructor(private $scope: ng.IScope, private reportDataService: IReportDataService, private coreService: ICoreService) {
        this.rangeFilters = new Array<NamedFilterRange>();
        this.$scope.$watch(() => this.rangeFilters, () => {
            this.saveRangedFilters();
        }, true);

        this.$scope.$watch(() => this.selections, (newVal, oldVal) => {
            if (!newVal)
                return;
        });

        this.projectNrHandler = true;

        this.$scope.$watch(() => this.userSelection, (newVal, oldVal) => {
            if (!newVal)
                return;
            this.setSavedUserFilters(newVal);
        });

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
                    this.projectsDict.push({ id: row.projectId, name: row.projectId + ' ' + row.number + ' ' + row.name });
                }

                this.projectNrFrom = this.userSelection.getTextSelection(Constants.REPORTMENU_SELECTION_KEY_PROJECT_NUMBER_FROM);
                if (this.projectNrFrom != null)
                    this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_PROJECT_NUMBER_FROM, new TextSelectionDTO(this.projectNrFrom.text));
                this.projectNrTo = this.userSelection.getTextSelection(Constants.REPORTMENU_SELECTION_KEY_PROJECT_NUMBER_TO);
                if (this.projectNrTo != null)
                    this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_PROJECT_NUMBER_TO, new TextSelectionDTO(this.projectNrTo.text));
            });

        }
        else {
            this.projectNrFrom = this.userSelection.getTextSelection(Constants.REPORTMENU_SELECTION_KEY_PROJECT_NUMBER_FROM);
            if (this.projectNrFrom != null)
                this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_PROJECT_NUMBER_FROM, new TextSelectionDTO(this.projectNrFrom.text));
            this.projectNrTo = this.userSelection.getTextSelection(Constants.REPORTMENU_SELECTION_KEY_PROJECT_NUMBER_TO);
            if (this.projectNrTo != null)
                this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_PROJECT_NUMBER_TO, new TextSelectionDTO(this.projectNrTo.text));
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

        this.selectedDateRange = savedValues.getDateRangeSelection();
        this.selectedPaymentDateRange = savedValues.getPaymentDateRangeSelection();
        this.selectedCreatedDateRange = savedValues.getCreatedDateRangeSelection();
        this.includeInvoicedOrders = this.userSelection.getBoolSelection(Constants.REPORTMENU_SELECTION_KEY_INCLUDE_INVOICED_ORDERS);

        this.selectedSortItem = this.userSelection.getIdSelection(Constants.REPORTMENU_SELECTION_KEY_CUSTOMER_CATEGORY);

        if (this.userSelection.getTextSelection(Constants.REPORTMENU_SELECTION_KEY_ACTOR_NUMBER_FROM) != null) {
            this.actorNrFrom = _.find(this.customerDict, d => d.name.split(' ')[0] === this.userSelection.getTextSelection(Constants.REPORTMENU_SELECTION_KEY_ACTOR_NUMBER_FROM).text);
            this.customerIDChangedFrom(this.actorNrFrom);
        }
        if (this.userSelection.getTextSelection(Constants.REPORTMENU_SELECTION_KEY_ACTOR_NUMBER_TO) != null) {
            this.actorNrTo = _.find(this.customerDict, d => d.name.split(' ')[0] === this.userSelection.getTextSelection(Constants.REPORTMENU_SELECTION_KEY_ACTOR_NUMBER_TO).text);
            this.customerIDChangedTo(this.actorNrTo);
        }
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
        console.log("Filters has changed");
    }

    private handleSavedRanges(namedFilters: AccountFilterSelectionDTO[]) {
        this.rangeFilters = [];
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

    private loadProjects() {
        this.projectsDict = [];
        return this.reportDataService.getProjects(true, true, false, true, false, 0).then((x) => {
            this.projects = x;
            this.projectsDict.push({ id: 0, name: "" });
            for (var i = 0; i < this.projects.length; i++) {
                var row = this.projects[i];
                this.projectsDict.push({ id: row.projectId, name: row.projectId + ' ' + row.number + ' ' + row.name });
            }
        });
    }

    private getCustomerCategory() {
        this.selectableCustomerCategorySorting = [];
        return this.coreService.getCategoriesDict(SoeCategoryType.Customer, true).then(data => {
            this.selectableCustomerCategorySorting = data;
        });
    }

    public onProjectNrFromChanged(selection: ITextSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_PROJECT_NUMBER_FROM, selection);

        if (!this.projectNrTo || !this.projectNrTo.text) {
            this.projectNrHandler = true;
            this.projectNrTo = new TextSelectionDTO(selection.text);
        }
    }

    public onProjectNrToChanged(selection: ITextSelectionDTO) {
        if (this.projectNrHandler) {
            this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_PROJECT_NUMBER_TO, selection);
            this.projectNrHandler = false;
            this.projectNrTo = new TextSelectionDTO(selection.text);
        } else {
            this.projectNrHandler = true;
        }
    }

    public articleNumberChangedFrom(selection: ITextSelectionDTO) {
        var selectionFrom = new TextSelectionDTO(selection.text);
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_INVOICE_NUMBER_FROM, selection);

        if (!this.articleNumberTo || !this.articleNumberTo.text) {
            this.articleNumberTo = selection;
        }
    }

    private articleNumberChangedTo(selection: ITextSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_INVOICE_NUMBER_TO, selection);
    }

    public onCustomerCategoryOrderSelectionChanged(selection: IIdSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_CUSTOMER_CATEGORY, selection);
    }

    public customerIDChangedFrom(selection: ISmallGenericType) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_ACTOR_NUMBER_FROM, new TextSelectionDTO(selection.name.split(' ')[0]));

        if (!this.actorNrTo) {
            this.actorNrTo = selection;
            this.customerIDChangedTo(selection);
        }
    }

    private customerIDChangedTo(selection: ISmallGenericType) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_ACTOR_NUMBER_TO, new TextSelectionDTO(selection.name.split(' ')[0]));
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

    public onDateRangeSelected(dateRange: DateRangeSelectionDTO) {
        dateRange.useMinMaxIfEmpty = true;
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_DATE_RANGE, dateRange);
    }

    public onPaymentDateRangeSelected(paymentDateRange: DateRangeSelectionDTO) {
        paymentDateRange.useMinMaxIfEmpty = true;
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_PAYMENTDATE_RANGE, paymentDateRange);
    }

    public onCreatedDateRangeSelected(createdDateRange: DateRangeSelectionDTO) {
        createdDateRange.useMinMaxIfEmpty = true;
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_CREATEDDATE_RANGE, createdDateRange);
    }

    public onBoolSelectionIncludeInvoicedOrders(selection: IBoolSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_INCLUDE_INVOICED_ORDERS, selection);
    }

    public onDateSelectionSelected(selection: IBoolSelectionDTO) {
        this.showRange = !selection.value;
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_DATE_SELECTION_CHOOSEN, selection);
    }
}