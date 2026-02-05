import { BoolSelectionDTO, DateRangeSelectionDTO, IdSelectionDTO, TextSelectionDTO } from "../../../../../../Common/Models/ReportDataSelectionDTO";
import { ReportUserSelectionDTO } from "../../../../../../Common/Models/ReportDTOs";
import { SmallGenericType } from "../../../../../../Common/Models/SmallGenericType";
import { IBoolSelectionDTO, IIdSelectionDTO, ISmallGenericType } from "../../../../../../Scripts/TypeLite.Net4";
import { TermGroup, TermGroup_ReportLedgerDateRegard, TermGroup_ReportLedgerInvoiceSelection } from "../../../../../../Util/CommonEnumerations";
import { Constants } from "../../../../../../Util/Constants";
import { ICoreService } from "../../../../../Services/CoreService";
import { SelectionCollection } from "../../../SelectionCollection";

export class PaymentJournalSupplierReport {
    public static component(): ng.IComponentOptions {
        return {
            controller: PaymentJournalSupplierReport,
            templateUrl: soeConfig.baseUrl + "Core/RightMenu/ReportMenu/ReportTypes/Economy/PaymentJournalSupplierReport/PaymentJournalSupplierReportView.html",
            bindings: {
                userSelection: "=",
                selections: "<"
            }
        } as ng.IComponentOptions;
    }
    public static componentKey = "paymentJournalSupplierReport";

    private userSelection: ReportUserSelectionDTO;
    private selections: SelectionCollection;
    private selectableInvoiceSelection: ISmallGenericType[];
    private selectableDateRegard: ISmallGenericType[];
    private selectableSortOrder: ISmallGenericType[];
    private selectedDateRange: DateRangeSelectionDTO;
    private supplierNrFrom: SmallGenericType;
    private supplierNrTo: SmallGenericType;
    private userSelectionInputInvoiceSelection: IdSelectionDTO;
    private userSelectionInputDateRegard: IdSelectionDTO;
    private userSelectionInputSortOrder: IdSelectionDTO;
    private includeCashSalesInvoices: BoolSelectionDTO;
    private supplierDict: SmallGenericType[];

    //@ngInject
    constructor(private $scope: ng.IScope, private coreService: ICoreService) {
        this.$scope.$watch(() => this.userSelection, (newVal, oldVal) => {
            if (!newVal)
                return;

            if (this.userSelection.getTextSelection(Constants.REPORTMENU_SELECTION_KEY_ACTOR_NUMBER_FROM) != null) {
                this.supplierNrFrom = _.find(this.supplierDict, d => d.name.split(' ')[0] === this.userSelection.getTextSelection(Constants.REPORTMENU_SELECTION_KEY_ACTOR_NUMBER_FROM).text);
                this.onActorNumberFromChanged(this.supplierNrFrom);
            }

            if (this.userSelection.getTextSelection(Constants.REPORTMENU_SELECTION_KEY_ACTOR_NUMBER_TO) != null) {
                this.supplierNrTo = _.find(this.supplierDict, d => d.name.split(' ')[0] === this.userSelection.getTextSelection(Constants.REPORTMENU_SELECTION_KEY_ACTOR_NUMBER_TO).text);
                this.onActorNumberToChanged(this.supplierNrTo);
            }

            this.selectedDateRange = this.userSelection.getDateRangeSelection();
            this.userSelectionInputInvoiceSelection = this.userSelection.getIdSelection(Constants.REPORTMENU_SELECTION_KEY_INVOICE_SELECTION);
            this.userSelectionInputDateRegard = this.userSelection.getIdSelection(Constants.REPORTMENU_SELECTION_KEY_DATE_REGARD);
            this.userSelectionInputSortOrder = this.userSelection.getIdSelection(Constants.REPORTMENU_SELECTION_KEY_SORT_ORDER);
            this.includeCashSalesInvoices = this.userSelection.getBoolSelection(Constants.REPORTMENU_SELECTION_KEY_INCLUDE_CASH_SALE_INVOICES);
        });
    }

    public $onInit() {
        this.selectedDateRange = new DateRangeSelectionDTO("daterange", null, null);
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
        return this.coreService.getTermGroupContent(TermGroup.ReportLedgerInvoiceSelection, false, false, false).then(data => {
            _.forEach(data, (i) => {
                if (!(i.id == TermGroup_ReportLedgerInvoiceSelection.All || i.id == TermGroup_ReportLedgerInvoiceSelection.NotPayed || i.id == TermGroup_ReportLedgerInvoiceSelection.NotPayedAndPartlyPayed)) {
                    if (i.id == TermGroup_ReportLedgerInvoiceSelection.FullyPayedAndPartlyPayed) {
                        this.userSelectionInputInvoiceSelection = new IdSelectionDTO(i.id);
                    }
                    this.selectableInvoiceSelection.push(i);
                }
            });
        });
    }

    private getDateRegard() {
        this.selectableDateRegard = [];
        return this.coreService.getTermGroupContent(TermGroup.ReportLedgerDateRegard, false, false, false).then(data => {
            _.forEach(data, (i) => {
                if (!(i.id == TermGroup_ReportLedgerDateRegard.InvoiceDate || i.id == TermGroup_ReportLedgerDateRegard.VoucherDate || i.id == TermGroup_ReportLedgerDateRegard.DueDate)) {
                    if (i.id == TermGroup_ReportLedgerDateRegard.InvoiceDate) {
                        this.userSelectionInputDateRegard = new IdSelectionDTO(i.id);
                    } else if (i.id == TermGroup_ReportLedgerDateRegard.PaymentDate) {
                        this.userSelectionInputDateRegard = new IdSelectionDTO(i.id);
                    }
                    this.selectableDateRegard.push(i);
                }
            });
        });
    }

    private getSortOrder() {
        this.selectableSortOrder = [];
        return this.coreService.getTermGroupContent(TermGroup.ReportSupplierLedgerSortOrder, false, false, false).then(data => {
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

