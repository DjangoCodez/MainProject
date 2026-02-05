import { IPromise } from "angular";
import { AccountDimSmallDTO } from "../../../../../../Common/Models/AccountDimDTO";
import { AccountFilterSelectionDTO, AccountFilterSelectionsDTO, BoolSelectionDTO, DateRangeSelectionDTO, IdSelectionDTO, TextSelectionDTO } from "../../../../../../Common/Models/ReportDataSelectionDTO";
import { ReportUserSelectionDTO } from "../../../../../../Common/Models/ReportDTOs";
import { SmallGenericType } from "../../../../../../Common/Models/SmallGenericType";
import { IBoolSelectionDTO, IIdSelectionDTO, ISmallGenericType, ITextSelectionDTO } from "../../../../../../Scripts/TypeLite.Net4";
import { SoeCategoryType, TermGroup, TermGroup_ReportBillingDateRegard } from "../../../../../../Util/CommonEnumerations";
import { Constants } from "../../../../../../Util/Constants";
import { ICoreService } from "../../../../../Services/CoreService";
import { IReportDataService } from "../../../ReportDataService";
import { SelectionCollection } from "../../../SelectionCollection";
import { NamedFilterRange } from "../../../Selections/AccountRangeSelection/AccountRangeSelectionComponent";

export class OrderReport {
    public static component(): ng.IComponentOptions {
        return {
            controller: OrderReport,
            templateUrl: soeConfig.baseUrl + "Core/RightMenu/ReportMenu/ReportTypes/Billing/OrderReport/OrderReportView.html",
            bindings: {
                userSelection: "=",
                selections: "<"
            }
        } as ng.IComponentOptions;
    }
    public static componentKey = "orderReport";
    private userSelection: ReportUserSelectionDTO;
    private selections: SelectionCollection;
    private selectableCustomerGroup: ISmallGenericType[];
    private selectableSortOrder: ISmallGenericType[];
    private selectableDateRegard: ISmallGenericType[];
    private customerNrFrom: SmallGenericType;
    private customerNrTo: SmallGenericType;
    private invoiceSeqNrFrom: ITextSelectionDTO;
    private invoiceSeqNrTo: ITextSelectionDTO;
    private userSelectionInputCustomerGroup: IdSelectionDTO;
    private userSelectionInputSortOrder: IdSelectionDTO;
    private userSelectionInputDateRegard: IdSelectionDTO;
    private selectedDateRange: DateRangeSelectionDTO;
    private includeClosedOrder: BoolSelectionDTO;
    private selectableRangeNames: AccountDimSmallDTO[];
    private rangeFilters: NamedFilterRange[];
    private customerDict: SmallGenericType[];

    //@ngInject
    constructor(private $scope: ng.IScope, private reportDataService: IReportDataService, private coreService: ICoreService,) {
        this.rangeFilters = new Array<NamedFilterRange>();
        this.$scope.$watch(() => this.rangeFilters, () => {
            this.saveRangedFilters();
        }, true);
        
        this.$scope.$watch(() => this.userSelection, (newVal, oldVal) => {
            if (!newVal)
                return;
            this.setSavedUserFilters(newVal);
        });

        this.$scope.$watch(() => this.customerNrFrom, (newVal, oldVal) => {
            if (newVal == null && oldVal != null)
                this.onActorNumberFromChanged(this.customerNrFrom);
        });

        this.$scope.$watch(() => this.customerNrTo, (newVal, oldVal) => {
            if (newVal == null && oldVal != null)
                this.onActorNumberToChanged(this.customerNrTo);
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

        this.userSelectionInputCustomerGroup = this.userSelection.getIdSelection(Constants.REPORTMENU_SELECTION_KEY_CUSTOMER_CATEGORY);
        this.invoiceSeqNrFrom = this.userSelection.getTextSelection(Constants.REPORTMENU_SELECTION_KEY_INVOICE_NUMBER_FROM);
        this.invoiceSeqNrTo = this.userSelection.getTextSelection(Constants.REPORTMENU_SELECTION_KEY_INVOICE_NUMBER_TO);

        if (this.userSelection.getTextSelection(Constants.REPORTMENU_SELECTION_KEY_ACTOR_NUMBER_FROM) != null) {
            this.customerNrFrom = _.find(this.customerDict, d => d.name.split(' ')[0] === this.userSelection.getTextSelection(Constants.REPORTMENU_SELECTION_KEY_ACTOR_NUMBER_FROM).text);
            this.onActorNumberFromChanged(this.customerNrFrom);
        }
        if (this.userSelection.getTextSelection(Constants.REPORTMENU_SELECTION_KEY_ACTOR_NUMBER_TO) != null) {
            this.customerNrTo = _.find(this.customerDict, d => d.name.split(' ')[0] === this.userSelection.getTextSelection(Constants.REPORTMENU_SELECTION_KEY_ACTOR_NUMBER_TO).text);
            this.onActorNumberToChanged(this.customerNrTo);
        }

        this.selectedDateRange = this.userSelection.getDateRangeSelection();
        this.userSelectionInputDateRegard = this.userSelection.getIdSelection(Constants.REPORTMENU_SELECTION_KEY_DATE_REGARD);
        this.userSelectionInputSortOrder = this.userSelection.getIdSelection(Constants.REPORTMENU_SELECTION_KEY_SORT_ORDER);
        this.includeClosedOrder = this.userSelection.getBoolSelection(Constants.REPORTMENU_SELECTION_KEY_INCLUDE_CLOSED_ORDER);
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
        this.selectedDateRange = new DateRangeSelectionDTO("daterange", null, null);
        this.getCustomerGroup();
        this.getDateRegard();
        this.getSortOrder();
        this.handleAvailableRanges();
        this.getCustomers();
    }

    private getCustomers() {
        this.customerDict = [];
        this.coreService.getCustomers(true, false, true).then((customers: ISmallGenericType[]) => {
            this.customerDict = customers;
        });
    }

    private getCustomerGroup() {
        this.selectableCustomerGroup = [];
        return this.coreService.getCategories(SoeCategoryType.Customer, false, false, false, false).then(categories => {
            categories.forEach(category => {
                this.selectableCustomerGroup.push(new SmallGenericType(category.categoryId, category.code + " - " + category.name));
            });
        });
    }

    private getDateRegard() {
        this.selectableDateRegard = [];
        var termGroupId = TermGroup.ReportBillingDateRegard;
        return this.coreService.getTermGroupContent(termGroupId, false, false, false).then(data => {
            this.selectableDateRegard = data;
            _.forEach(this.selectableDateRegard, (i) => {
                if (i.id == TermGroup_ReportBillingDateRegard.OrderDate) {
                    this.userSelectionInputDateRegard = new IdSelectionDTO(i.id);
                }
            });
        });
    }

    private getSortOrder() {
        this.selectableSortOrder = [];
        return this.coreService.getTermGroupContent(TermGroup.ReportBillingOrderSortOrder, false, false, false).then(data => {
            this.selectableSortOrder = data;
            this.userSelectionInputSortOrder = new IdSelectionDTO(this.selectableSortOrder[0].id);
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
        if (this.rangeFilters.length === 0) {
            row.selectedSelection = this.selectableRangeNames[0];
        }
        this.rangeFilters.push(row);
        if (this.rangeFilters.length === 1) {
            this.rangeFilters[0].selectedSelection.accounts = _.sortBy(this.rangeFilters[0].selectedSelection.accounts, 'accountNr');
            _.forEach(this.rangeFilters[0].selectedSelection.accounts, (a) => {
                a.name = a.numberName;
            });
        }
    }

    public onCustomerGroupChanged(selection: IIdSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_CUSTOMER_CATEGORY, selection);
    }

    public onDateRangeSelected(dateRange: DateRangeSelectionDTO) {
        dateRange.useMinMaxIfEmpty = true;
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_DATE_RANGE, dateRange);
    }

    public onDateRegardChanged(selection: IIdSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_DATE_REGARD, selection);
    }

    public onSortOrderChanged(selection: IIdSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_SORT_ORDER, selection);
    }

    public onBoolSelectionIncludeClosedOrder(selection: IBoolSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_INCLUDE_CLOSED_ORDER, selection);
    }

    public onInvoiceSerialNumberFromChanged(selection: ITextSelectionDTO) {

        var selectFrom = new TextSelectionDTO(selection.text);
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_INVOICE_NUMBER_FROM, selection);

        if (!this.invoiceSeqNrTo || !this.invoiceSeqNrTo.text) {
            this.invoiceSeqNrTo = selectFrom;
        }
    }

    public onInvoiceSerialNumberToChanged(selection: ITextSelectionDTO) {

        if (selection.text == "") {
            selection.text = null;
            this.invoiceSeqNrTo = selection
        }

        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_INVOICE_NUMBER_TO, selection);
    }
    public onActorNumberFromChanged(selection: SmallGenericType) {
        var text = selection != null ? selection.name.split(' ')[0] : "";
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_ACTOR_NUMBER_FROM, new TextSelectionDTO(text));

        if (!this.customerNrTo) {
            this.customerNrTo = selection;
            this.onActorNumberToChanged(selection);
        }
    }

    public onActorNumberToChanged(selection: SmallGenericType) {
        var text = selection != null ? selection.name.split(' ')[0] : "";
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_ACTOR_NUMBER_TO, new TextSelectionDTO(text));
    }
}
