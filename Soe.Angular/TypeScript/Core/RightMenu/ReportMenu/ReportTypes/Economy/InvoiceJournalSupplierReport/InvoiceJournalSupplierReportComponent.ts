import { BoolSelectionDTO, DateRangeSelectionDTO, IdSelectionDTO, TextSelectionDTO } from "../../../../../../Common/Models/ReportDataSelectionDTO";
import { ReportUserSelectionDTO } from "../../../../../../Common/Models/ReportDTOs";
import { SmallGenericType } from "../../../../../../Common/Models/SmallGenericType";
import { IBoolSelectionDTO, IIdSelectionDTO, ISmallGenericType, ITextSelectionDTO } from "../../../../../../Scripts/TypeLite.Net4";
import { TermGroup, TermGroup_ReportLedgerDateRegard, TermGroup_ReportLedgerInvoiceSelection } from "../../../../../../Util/CommonEnumerations";
import { Constants } from "../../../../../../Util/Constants";
import { ICoreService } from "../../../../../Services/CoreService";
import { SelectionCollection } from "../../../SelectionCollection";

export class InvoiceJournalSupplierReport {
    public static component(): ng.IComponentOptions {
        return {
            controller: InvoiceJournalSupplierReport,
            templateUrl: soeConfig.baseUrl + "Core/RightMenu/ReportMenu/ReportTypes/Economy/InvoiceJournalSupplierReport/InvoiceJournalSupplierReportView.html",
            bindings: {
                userSelection: "=",
                selections: "<"
            }
        } as ng.IComponentOptions;
    }
    public static componentKey = "invoiceJournalSupplierReport";

    private userSelection: ReportUserSelectionDTO;
    private selections: SelectionCollection;
    private supplierDict: SmallGenericType[];
    private selectableInvoiceSelection: ISmallGenericType[];
    private selectableDateRegard: ISmallGenericType[];
    private selectableSortOrder: ISmallGenericType[];
    private selectedDateRange: DateRangeSelectionDTO;
    private supplierNrFrom: SmallGenericType;
    private supplierNrTo: SmallGenericType;
    private invoiceSeqNrFrom: ITextSelectionDTO;
    private invoiceSeqNrTo: ITextSelectionDTO;
    private userSelectionInputInvoiceSelection: IdSelectionDTO;
    private userSelectionInputDateRegard: IdSelectionDTO;
    private userSelectionInputSortOrder: IdSelectionDTO;
    private showPreliminaryInvoices: BoolSelectionDTO;
    private includeCashSalesInvoices: BoolSelectionDTO;

    //@ngInject
    constructor(private $scope: ng.IScope, private coreService: ICoreService) {
        this.$scope.$watch(() => this.userSelection, (newVal, oldVal) => {
            if (!newVal)
                return;

            this.userSelectionInputInvoiceSelection = this.userSelection.getIdSelection(Constants.REPORTMENU_SELECTION_KEY_INVOICE_SELECTION);

            var invoiceNrFrom = this.userSelection.getIdSelection(Constants.REPORTMENU_SELECTION_KEY_INVOICE_NUMBER_FROM);
            if (invoiceNrFrom != null) {
                this.invoiceSeqNrFrom = new TextSelectionDTO(invoiceNrFrom.id.toString());
            }

            var invoiceNrTo = this.userSelection.getIdSelection(Constants.REPORTMENU_SELECTION_KEY_INVOICE_NUMBER_TO);
            if (invoiceNrTo != null) {
                this.invoiceSeqNrTo = new TextSelectionDTO(invoiceNrTo.id.toString());
            }

            if (this.userSelection.getTextSelection(Constants.REPORTMENU_SELECTION_KEY_ACTOR_NUMBER_FROM) != null) {
                this.supplierNrFrom = _.find(this.supplierDict, d => d.name.split(' ')[0] === this.userSelection.getTextSelection(Constants.REPORTMENU_SELECTION_KEY_ACTOR_NUMBER_FROM).text);
                this.onActorNumberFromChanged(this.supplierNrFrom);
            }

            if (this.userSelection.getTextSelection(Constants.REPORTMENU_SELECTION_KEY_ACTOR_NUMBER_TO) != null) {
                this.supplierNrTo = _.find(this.supplierDict, d => d.name.split(' ')[0] === this.userSelection.getTextSelection(Constants.REPORTMENU_SELECTION_KEY_ACTOR_NUMBER_TO).text);
                this.onActorNumberToChanged(this.supplierNrTo);
            }

            this.selectedDateRange = this.userSelection.getDateRangeSelection();
            this.userSelectionInputDateRegard = this.userSelection.getIdSelection(Constants.REPORTMENU_SELECTION_KEY_DATE_REGARD);
            this.userSelectionInputSortOrder = this.userSelection.getIdSelection(Constants.REPORTMENU_SELECTION_KEY_SORT_ORDER);
            this.includeCashSalesInvoices = this.userSelection.getBoolSelection(Constants.REPORTMENU_SELECTION_KEY_INCLUDE_CASH_SALE_INVOICES);
            this.showPreliminaryInvoices = this.userSelection.getBoolSelection(Constants.REPORTMENU_SELECTION_KEY_SHOW_PRELIMINARY_INVOICES);
        });
    }

    public $onInit() {
        this.getInvoiceSelection();
        this.getDateRegard();
        this.getSortOrder();
        this.getSuppliers();
    }

    private getSuppliers() {
        this.coreService.getSuppliersDict(true, false, true).then((x: ISmallGenericType[]) => {
            this.supplierDict = x;
        });
    }

    private getInvoiceSelection() {
        this.selectableInvoiceSelection = [];
        var termGroupId = TermGroup.ReportLedgerInvoiceSelection;
        return this.coreService.getTermGroupContent(termGroupId, false, false, false).then(data => {
            this.selectableInvoiceSelection = data;
            _.forEach(this.selectableInvoiceSelection, (i) => {
                if (i.id == TermGroup_ReportLedgerInvoiceSelection.NotPayed) {
                    this.userSelectionInputInvoiceSelection = new IdSelectionDTO(i.id);
                }
            });
        });
    }

    private getDateRegard() {
        this.selectableDateRegard = [];
        var termGroupId = TermGroup.ReportLedgerDateRegard;
        return this.coreService.getTermGroupContent(termGroupId, false, false, false).then(data => {
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
        var termGroupId = TermGroup.ReportSupplierLedgerSortOrder;
        return this.coreService.getTermGroupContent(termGroupId, false, false, false).then(data => {
            this.selectableSortOrder = data;
            if (this.selectableSortOrder.length > 0) {
                this.userSelectionInputSortOrder = new IdSelectionDTO(this.selectableSortOrder[0].id);
            }
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

    public onBoolSelectionShowPreliminaryInvoices(selection: IBoolSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_SHOW_PRELIMINARY_INVOICES, selection);
    }

    public onBoolSelectionIncludeCashSalesInvoices(selection: IBoolSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_INCLUDE_CASH_SALE_INVOICES, selection);
    }

    public onInvoiceSerialNumberFromChanged(selection: ITextSelectionDTO) {
        let id = parseInt(selection.text);
        if (id) {
            if (!this.invoiceSeqNrTo)
                this.invoiceSeqNrTo = selection;

            this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_INVOICE_NUMBER_FROM, new IdSelectionDTO(id));
        }
    }

    public onInvoiceSerialNumberToChanged(selection: ITextSelectionDTO) {
        let id = parseInt(selection.text);
        if (id)
            this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_INVOICE_NUMBER_TO, new IdSelectionDTO(id));
    }

    public onActorNumberFromChanged(selection: SmallGenericType) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_ACTOR_NUMBER_FROM, new TextSelectionDTO(selection.name.split(' ')[0]));

        if (!this.supplierNrTo) {
            this.supplierNrTo = selection;
            this.onActorNumberToChanged(selection);
        }
    }

    public onActorNumberToChanged(selection: SmallGenericType) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_ACTOR_NUMBER_TO, new TextSelectionDTO(selection.name.split(' ')[0]));
    }
}