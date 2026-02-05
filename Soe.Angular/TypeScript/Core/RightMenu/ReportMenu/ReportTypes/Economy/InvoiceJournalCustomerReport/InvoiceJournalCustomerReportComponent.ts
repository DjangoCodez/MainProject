import { BoolSelectionDTO, DateRangeSelectionDTO, IdSelectionDTO, TextSelectionDTO } from "../../../../../../Common/Models/ReportDataSelectionDTO";
import { ReportUserSelectionDTO } from "../../../../../../Common/Models/ReportDTOs";
import { SmallGenericType } from "../../../../../../Common/Models/SmallGenericType";
import { IBoolSelectionDTO, IIdSelectionDTO, ISmallGenericType, ITextSelectionDTO } from "../../../../../../Scripts/TypeLite.Net4";
import { TermGroup, TermGroup_ReportLedgerDateRegard } from "../../../../../../Util/CommonEnumerations";
import { Constants } from "../../../../../../Util/Constants";
import { ICoreService } from "../../../../../Services/CoreService";
import { SelectionCollection } from "../../../SelectionCollection";

export class InvoiceJournalCustomerReport {
    public static component(): ng.IComponentOptions {
        return {
            controller: InvoiceJournalCustomerReport,
            templateUrl: soeConfig.baseUrl + "Core/RightMenu/ReportMenu/ReportTypes/Economy/InvoiceJournalCustomerReport/InvoiceJournalCustomerReportView.html",
            bindings: {
                userSelection: "=",
                selections: "<"
            }
        } as ng.IComponentOptions;
    }

    public static componentKey = "invoiceJournalCustomerReport";

    private userSelection: ReportUserSelectionDTO;
    private selections: SelectionCollection;
    private selectableInvoiceSelection: ISmallGenericType[];
    private selectableDateRegard: ISmallGenericType[];
    private selectableSortOrder: ISmallGenericType[];
    private selectedDateRange: DateRangeSelectionDTO;
    private showPreliminaryInvoices: BoolSelectionDTO;
    private includeCashInvoices: BoolSelectionDTO;
    private userSelectionInputInvoiceSelection: IdSelectionDTO;
    private userSelectionInputDateRegard: IdSelectionDTO;
    private userSelectionInputSortOrder: IdSelectionDTO;
    private customerNrFrom: SmallGenericType;
    private customerNrTo: SmallGenericType;
    private invoiceSeqNrFrom: ITextSelectionDTO
    private invoiceSeqNrTo: ITextSelectionDTO;
    private customerDict: SmallGenericType[];

    //@ngInject
    constructor(private $scope: ng.IScope, private coreService: ICoreService,) {

        this.$scope.$watch(() => this.selections, (newVal, oldVal) => {
            if (!newVal)
                return;
        });

        this.$scope.$watch(() => this.userSelection, (newVal, oldVal) => {
            if (!newVal)
                return;
            this.setSavedUserFilters(newVal);
        });
    }

    public $onInit() {
        this.getInvoiceSelection();
        this.getDateRegard();
        this.getSortOrder();
        this.getCustomers();
    }

    private getCustomers() {
        this.customerDict = [];
        this.coreService.getCustomers(true, false, true).then((customers: ISmallGenericType[]) => {
            this.customerDict = customers;
        });
    }

    private setSavedUserFilters(savedValues: ReportUserSelectionDTO) {
        if (this.userSelection.getTextSelection(Constants.REPORTMENU_SELECTION_KEY_ACTOR_NUMBER_FROM) != null) {
            this.customerNrFrom = _.find(this.customerDict, d => d.name.split(' ')[0] === this.userSelection.getTextSelection(Constants.REPORTMENU_SELECTION_KEY_ACTOR_NUMBER_FROM).text);
            this.onActorNumberFromChanged(this.customerNrFrom);
        }

        if (this.userSelection.getTextSelection(Constants.REPORTMENU_SELECTION_KEY_ACTOR_NUMBER_TO) != null) {
            this.customerNrTo = _.find(this.customerDict, d => d.name.split(' ')[0] === this.userSelection.getTextSelection(Constants.REPORTMENU_SELECTION_KEY_ACTOR_NUMBER_TO).text);
            this.onActorNumberToChanged(this.customerNrTo);
        }

        var invoiceNrFrom = this.userSelection.getIdSelection(Constants.REPORTMENU_SELECTION_KEY_INVOICE_NUMBER_FROM);
        if (invoiceNrFrom != null) {
            this.invoiceSeqNrFrom = new TextSelectionDTO(invoiceNrFrom.id.toString());
        }

        var invoiceNrTo = this.userSelection.getIdSelection(Constants.REPORTMENU_SELECTION_KEY_INVOICE_NUMBER_TO);
        if (invoiceNrTo != null) {
            this.invoiceSeqNrTo = new TextSelectionDTO(invoiceNrTo.id.toString());
        }

        this.userSelectionInputInvoiceSelection = this.userSelection.getIdSelection(Constants.REPORTMENU_SELECTION_KEY_INVOICE_SELECTION);
        this.selectedDateRange = this.userSelection.getDateRangeSelection();
        this.userSelectionInputDateRegard = this.userSelection.getIdSelection(Constants.REPORTMENU_SELECTION_KEY_DATE_REGARD);
        this.userSelectionInputSortOrder = this.userSelection.getIdSelection(Constants.REPORTMENU_SELECTION_KEY_SORT_ORDER);
        this.showPreliminaryInvoices = this.userSelection.getBoolSelection(Constants.REPORTMENU_SELECTION_KEY_SHOW_PRELIMINARY_INVOICES);
        this.includeCashInvoices = this.userSelection.getBoolSelection(Constants.REPORTMENU_SELECTION_KEY_INCLUDE_CASH_SALE_INVOICES);
    }

    private getInvoiceSelection() {
        this.selectableInvoiceSelection = [];
        var termGroupId = TermGroup.ReportLedgerInvoiceSelection;
        return this.coreService.getTermGroupContent(termGroupId, false, false).then(data => {
            this.selectableInvoiceSelection = data;

            if (this.selectableInvoiceSelection.length > 0) {
                this.userSelectionInputInvoiceSelection = new IdSelectionDTO(this.selectableInvoiceSelection[1].id);
            }
        });
    }

    private getDateRegard() {
        this.selectableDateRegard = [];
        var termGroupId = TermGroup.ReportLedgerDateRegard;
        return this.coreService.getTermGroupContent(termGroupId, false, false).then(data => {
            this.selectableDateRegard = data;
            _.forEach(this.selectableDateRegard, (i) => {
                if (i.id == TermGroup_ReportLedgerDateRegard.InvoiceDate) {
                    this.userSelectionInputDateRegard = new IdSelectionDTO(i.id);
                }
            });
        });
    }

    private getSortOrder() {
        this.selectableSortOrder = [];
        var termGroupId = TermGroup.ReportCustomerLedgerSortOrder;
        return this.coreService.getTermGroupContent(termGroupId, false, false).then(data => {
            this.selectableSortOrder = data;
            if (this.selectableSortOrder.length > 0) {
                this.userSelectionInputSortOrder = new IdSelectionDTO(this.selectableSortOrder[0].id);
            }
        });
    }

    public onInvoiceSelectionChanged(selection: IIdSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_INVOICE_SELECTION, selection);
    }

    public onDateRegardChanged(selection: IIdSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_DATE_REGARD, selection);
    }

    public onSortOrderChanged(selection: IIdSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_SORT_ORDER, selection);
    }

    public onBoolSelectionShowPreliminaryInvoices(selection: IBoolSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_SHOW_PRELIMINARY_INVOICES, selection);
    }

    public onBoolSelectionIncludeCashInvoices(selection: IBoolSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_INCLUDE_CASH_SALE_INVOICES, selection);
    }

    public onInvoiceSerialNumberFromChanged(selection: ITextSelectionDTO) {
        let id = parseInt(selection.text);
        if (id) {
            if (!this.invoiceSeqNrTo) {
                this.invoiceSeqNrTo = selection;
                this.onInvoiceSerialNumberToChanged(selection);
            }
            this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_INVOICE_NUMBER_FROM, new IdSelectionDTO(id));
        }
    }

    public onInvoiceSerialNumberToChanged(selection: ITextSelectionDTO) {
        let id = parseInt(selection.text);
        if (id)
            this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_INVOICE_NUMBER_TO, new IdSelectionDTO(id));
    }

    public onActorNumberFromChanged(selection: ISmallGenericType) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_ACTOR_NUMBER_FROM, new TextSelectionDTO(selection.name.split(' ')[0]));

        if (!this.customerNrTo) {
            this.customerNrTo = selection;
            this.onActorNumberToChanged(selection);
        }
    }

    public onActorNumberToChanged(selection: ISmallGenericType) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_ACTOR_NUMBER_TO, new TextSelectionDTO(selection.name.split(' ')[0]));
    }

    public onDateRangeSelected(dateRange: DateRangeSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_DATE_RANGE, dateRange);
    }
}