import { BoolSelectionDTO, DateRangeSelectionDTO, IdSelectionDTO, TextSelectionDTO } from "../../../../../../Common/Models/ReportDataSelectionDTO";
import { ReportUserSelectionDTO } from "../../../../../../Common/Models/ReportDTOs";
import { IBoolSelectionDTO, IIdSelectionDTO, ISmallGenericType } from "../../../../../../Scripts/TypeLite.Net4";
import { TermGroup } from "../../../../../../Util/CommonEnumerations";
import { Constants } from "../../../../../../Util/Constants";
import { ICoreService } from "../../../../../Services/CoreService";
import { ITranslationService } from "../../../../../Services/TranslationService";
import { SelectionCollection } from "../../../SelectionCollection";

export class CustomerPaymentJournalReport {
    public static component(): ng.IComponentOptions {
        return {
            controller: CustomerPaymentJournalReport,
            templateUrl: soeConfig.baseUrl + "Core/RightMenu/ReportMenu/ReportTypes/Economy/CustomerPaymentJournalReport/CustomerPaymentJournalReportView.html",
            bindings: {
                userSelection: "=",
                selections: "<"
            }
        } as ng.IComponentOptions;
    }
    public static componentKey = "customerPaymentJournalReport";

    private userSelection: ReportUserSelectionDTO;
    private selections: SelectionCollection;

    private projectReportTitle = "";

    private selectableInvoiceSelection: ISmallGenericType[];
    private selectableDateRegard: ISmallGenericType[];
    private selectableSortOrder: ISmallGenericType[];
    private selectedDateRange: DateRangeSelectionDTO;

    private customerNrFrom: string = "";
    private customerNrTo: string = "";

    private userSelectionInputInvoiceSelection: IdSelectionDTO;
    private userSelectionInputDateRegard: IdSelectionDTO;
    private userSelectionInputSortOrder: IdSelectionDTO;
    private includeCashSalesInvoices: BoolSelectionDTO;
    private includeCashInvoices: BoolSelectionDTO;

    //@ngInject
    constructor(private $scope: ng.IScope, private translationService: ITranslationService, private coreService: ICoreService) {

        this.$scope.$watch(() => this.userSelection, (newVal, oldVal) => {
            if (!newVal)
                return;
            this.setSavedUserFilters(newVal);
        });

        this.$scope.$watch(() => this.customerNrFrom, () => {
            this.onActorNumberFromChanged();
        });
        this.$scope.$watch(() => this.customerNrTo, () => {
            this.onActorNumberToChanged();
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

        this.customerNrFrom = this.userSelection.getTextSelection(Constants.REPORTMENU_SELECTION_KEY_ACTOR_NUMBER_FROM) != null ? this.userSelection.getTextSelection(Constants.REPORTMENU_SELECTION_KEY_ACTOR_NUMBER_FROM).text : null;
        this.customerNrTo = this.userSelection.getTextSelection(Constants.REPORTMENU_SELECTION_KEY_ACTOR_NUMBER_TO) != null ? this.userSelection.getTextSelection(Constants.REPORTMENU_SELECTION_KEY_ACTOR_NUMBER_TO).text : null;
        this.userSelectionInputInvoiceSelection = this.userSelection.getIdSelection(Constants.REPORTMENU_SELECTION_KEY_INVOICE_SELECTION);
        this.selectedDateRange = this.userSelection.getDateRangeSelection();
        this.userSelectionInputDateRegard = this.userSelection.getIdSelection(Constants.REPORTMENU_SELECTION_KEY_DATE_REGARD);
        this.userSelectionInputSortOrder = this.userSelection.getIdSelection(Constants.REPORTMENU_SELECTION_KEY_SORT_ORDER);
        this.includeCashInvoices = this.userSelection.getBoolSelection(Constants.REPORTMENU_SELECTION_KEY_INCLUDE_CASH_SALE_INVOICES);
    }

    public $onInit() {
        this.getInvoiceSelection();
        this.getDateRegard();
        this.getSortOrder();
    }

    private getInvoiceSelection() {
        this.selectableInvoiceSelection = [];
        const termGroupId = TermGroup.ReportLedgerInvoiceSelection;
        return this.coreService.getTermGroupContent(termGroupId, false, false).then(data => {
            this.selectableInvoiceSelection = data.filter((x) => !((x.id == 1) || (x.id == 4) || (x.id == 5)));
            this.userSelectionInputInvoiceSelection = new IdSelectionDTO(this.selectableInvoiceSelection[1].id);
        });
    }

    private getDateRegard() {
        this.selectableDateRegard = [];
        const termGroupId = TermGroup.ReportLedgerDateRegard;
        return this.coreService.getTermGroupContent(termGroupId, false, true).then(data => {
            this.selectableDateRegard = data.filter((x) => (x.id == 4));
            this.userSelectionInputDateRegard = new IdSelectionDTO(this.selectableDateRegard[0].id);
        });
    }

    private getSortOrder() {
        this.selectableSortOrder = [];
        const termGroupId = TermGroup.ReportCustomerLedgerSortOrder;
        return this.coreService.getTermGroupContent(termGroupId, false, true).then(data => {
            this.selectableSortOrder = data;
            this.userSelectionInputSortOrder = new IdSelectionDTO(this.selectableSortOrder[0].id);
        });
    }

    public onInvoiceSelectionChanged(selection: IIdSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_INVOICE_SELECTION, selection);
    }

    public onDateRangeSelected(dateRange: DateRangeSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_DATE_RANGE, dateRange);
    }

    public onDateRegardChanged(selection: IIdSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_DATE_REGARD, selection);
    }

    public onSortOrderChanged(selection: IIdSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_SORT_ORDER, selection);
    }

    public onBoolSelectionIncludeCashSalesInvoices(selection: IBoolSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_INCLUDE_CASH_SALE_INVOICES, selection);
    }

    public onActorNumberFromChanged() {
        var selection = new TextSelectionDTO(this.customerNrFrom);
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_ACTOR_NUMBER_FROM, selection);
    }

    public onActorNumberToChanged() {
        var selection = new TextSelectionDTO(this.customerNrTo);
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_ACTOR_NUMBER_TO, selection);
    }
}