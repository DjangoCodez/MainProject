import { ICommonCustomerService } from "../../../../../../Common/Customer/CommonCustomerService";
import { BoolSelectionDTO, DateRangeSelectionDTO, IdSelectionDTO, TextSelectionDTO } from "../../../../../../Common/Models/ReportDataSelectionDTO";
import { ReportUserSelectionDTO } from "../../../../../../Common/Models/ReportDTOs";
import { SmallGenericType } from "../../../../../../Common/Models/SmallGenericType";
import { IBoolSelectionDTO, IIdSelectionDTO, ISmallGenericType, ITextSelectionDTO } from "../../../../../../Scripts/TypeLite.Net4";
import { SoeCategoryType, TermGroup } from "../../../../../../Util/CommonEnumerations";
import { Constants } from "../../../../../../Util/Constants";
import { ICoreService } from "../../../../../Services/CoreService";
import { ITranslationService } from "../../../../../Services/TranslationService";
import { IReportDataService } from "../../../ReportDataService";
import { SelectionCollection } from "../../../SelectionCollection";

export class InterestInvoiceReport {
    public static component(): ng.IComponentOptions {
        return {
            controller: InterestInvoiceReport,
            templateUrl: soeConfig.baseUrl + "Core/RightMenu/ReportMenu/ReportTypes/Billing/InterestInvoiceReport/InterestInvoiceReportView.html",
            bindings: {
                userSelection: "=",
                selections: "<"
            }
        } as ng.IComponentOptions;
    }
    public static componentKey = "interestInvoiceReport";

    private userSelection: ReportUserSelectionDTO;
    private selections: SelectionCollection;

    private projectReportTitle = "";

    private selectableCustomerGroup: ISmallGenericType[];
    private selectableSortOrder: ISmallGenericType[];

    private customerNrFrom: SmallGenericType;
    private customerNrTo: SmallGenericType;
    private invoiceSeqNrFrom: ITextSelectionDTO;
    private invoiceSeqNrTo: ITextSelectionDTO;

    private userSelectionInputCustomerGroup: IdSelectionDTO;
    private userSelectionInputSortOrder: IdSelectionDTO;
    private selectedDateRange: DateRangeSelectionDTO;
    private includeClosedOrder: BoolSelectionDTO;

    private showNotPrinted: BoolSelectionDTO;
    private showCopies: BoolSelectionDTO;
    private showCopiesAsOriginal: BoolSelectionDTO;

    private customerDict: SmallGenericType[];

    //@ngInject
    constructor(private $scope: ng.IScope, $timeout: ng.ITimeoutService, private translationService: ITranslationService, private reportDataService: IReportDataService, private coreService: ICoreService, private commonCustomerService: ICommonCustomerService,) {

        this.showNotPrinted = new BoolSelectionDTO(true);
        this.$scope.$watch(() => this.invoiceSeqNrFrom, () => {
        }, true);

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

    private setSavedUserFilters(savedValues: ReportUserSelectionDTO) {

        this.userSelectionInputCustomerGroup = this.userSelection.getIdSelection(Constants.REPORTMENU_SELECTION_KEY_CUSTOMER_CATEGORY);
        this.invoiceSeqNrFrom = this.userSelection.getTextSelection(Constants.REPORTMENU_SELECTION_KEY_INVOICE_NUMBER_FROM);
        this.invoiceSeqNrTo = this.userSelection.getTextSelection(Constants.REPORTMENU_SELECTION_KEY_INVOICE_NUMBER_TO);

        if (this.userSelection.getTextSelection(Constants.REPORTMENU_SELECTION_KEY_ACTOR_NUMBER_FROM) != null)
            this.customerNrFrom = _.find(this.customerDict, d => d.name.split(' ')[0] === this.userSelection.getTextSelection(Constants.REPORTMENU_SELECTION_KEY_ACTOR_NUMBER_FROM).text);

        if (this.userSelection.getTextSelection(Constants.REPORTMENU_SELECTION_KEY_ACTOR_NUMBER_TO) != null)
            this.customerNrTo = _.find(this.customerDict, d => d.name.split(' ')[0] === this.userSelection.getTextSelection(Constants.REPORTMENU_SELECTION_KEY_ACTOR_NUMBER_TO).text);

        this.selectedDateRange = this.userSelection.getDateRangeSelection();

        this.userSelectionInputSortOrder = this.userSelection.getIdSelection(Constants.REPORTMENU_SELECTION_KEY_SORT_ORDER);
        this.showNotPrinted = this.userSelection.getBoolSelection(Constants.REPORTMENU_SELECTION_KEY_SHOW_ANY_UNPRINTED);
        this.showCopies = this.userSelection.getBoolSelection(Constants.REPORTMENU_SELECTION_KEY_SHOW_COPIES);
        this.showCopiesAsOriginal = this.userSelection.getBoolSelection(Constants.REPORTMENU_SELECTION_KEY_SHOW_COPIES_OF_ORIGINAL);

    }

    public $onInit() {
        this.selectedDateRange = new DateRangeSelectionDTO("daterange", null, null);
        this.getCustomerGroup();
        this.getSortOrder();
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

    private getSortOrder() {
        this.selectableSortOrder = [];
        return this.coreService.getTermGroupContent(TermGroup.ReportBillingInvoiceSortOrder, false, false, false).then(data => {
            this.selectableSortOrder = data;
            this.userSelectionInputSortOrder = new IdSelectionDTO(this.selectableSortOrder[0].id);
        });
    }

    public onCustomerGroupChanged(selection: IIdSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_CUSTOMER_CATEGORY, selection);
    }

    public onDateRangeSelected(dateRange: DateRangeSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_DATE_RANGE, dateRange);
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
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_ACTOR_NUMBER_FROM, new TextSelectionDTO(selection.name.split(' ')[0]));

        if (!this.customerNrTo) {
            this.customerNrTo = selection;
            this.onActorNumberToChanged(selection);
        }
    }

    public onActorNumberToChanged(selection: SmallGenericType) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_ACTOR_NUMBER_TO, new TextSelectionDTO(selection.name.split(' ')[0]));
    }

    public onBoolSelectionShowNotPrinted(selection) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_SHOW_ANY_UNPRINTED, selection);
    }

    public onBoolSelectionShowCopies(selection) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_SHOW_COPIES, selection);
    }

    public onBoolSelectionShowCopiesAsOriginal(selection) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_SHOW_COPIES_OF_ORIGINAL, selection);
    }
}
