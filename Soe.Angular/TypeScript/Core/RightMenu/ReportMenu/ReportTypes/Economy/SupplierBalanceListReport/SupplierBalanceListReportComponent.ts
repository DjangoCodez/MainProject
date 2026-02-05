import { BoolSelectionDTO, DateRangeSelectionDTO, IdSelectionDTO, TextSelectionDTO } from "../../../../../../Common/Models/ReportDataSelectionDTO";
import { ReportUserSelectionDTO } from "../../../../../../Common/Models/ReportDTOs";
import { IBoolSelectionDTO, IIdSelectionDTO, ISmallGenericType, ITextSelectionDTO } from "../../../../../../Scripts/TypeLite.Net4";
import { TermGroup, TermGroup_ReportLedgerInvoiceSelection, TermGroup_ReportLedgerDateRegard } from "../../../../../../Util/CommonEnumerations";
import { Constants } from "../../../../../../Util/Constants";
import { SelectionCollection } from "../../../SelectionCollection";
import { ICoreService } from "../../../../../Services/CoreService";
import { SmallGenericType } from "../../../../../../Common/Models/SmallGenericType";
import { CalendarUtility } from "../../../../../../Util/CalendarUtility";
import { IMessagingService } from "../../../../../Services/MessagingService";

export class SupplierBalanceListReport {
    public static component(): ng.IComponentOptions {
        return {
            controller: SupplierBalanceListReport,
            templateUrl: soeConfig.baseUrl + "Core/RightMenu/ReportMenu/ReportTypes/Economy/SupplierBalanceListReport/SupplierBalanceListReportView.html",
            bindings: {
                userSelection: "=",
                selections: "<"
            }
        } as ng.IComponentOptions;
    }
    public static componentKey = "supplierBalanceListReport";

    private userSelection: ReportUserSelectionDTO;
    private selections: SelectionCollection;
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
    private showVoucher: BoolSelectionDTO;
    private showPendingPaymentsInReport: BoolSelectionDTO;
    private invoiceSeqNrHandler: boolean = true;
    private supplierDict: SmallGenericType[];
    private isFromDateMandatory = false;
    private isToDateMandatory = true;

    //@ngInject
    constructor(private $scope: ng.IScope, private coreService: ICoreService, protected messagingService: IMessagingService) {

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

            this.invoiceSeqNrHandler = true;
            this.selectedDateRange = this.userSelection.getDateRangeSelection();
            this.userSelectionInputDateRegard = this.userSelection.getIdSelection(Constants.REPORTMENU_SELECTION_KEY_DATE_REGARD);
            this.userSelectionInputSortOrder = this.userSelection.getIdSelection(Constants.REPORTMENU_SELECTION_KEY_SORT_ORDER);
            this.showVoucher = this.userSelection.getBoolSelection(Constants.REPORTMENU_SELECTION_KEY_SHOW_VOUCHER);
            this.showPendingPaymentsInReport = this.userSelection.getBoolSelection(Constants.REPORTMENU_SELECTION_KEY_SHOW_PENDING_PAYMENTS_IN_REPORT);
        });

    }

    public $onInit() {
        this.selectedDateRange = new DateRangeSelectionDTO("daterange", null, new Date());
        this.showPendingPaymentsInReport = new BoolSelectionDTO(true);
        this.getInvoiceSelection();
        this.getDateRegard();
        this.getSortOrder();
        this.getSuppliers();
        this.validate(this.selectedDateRange);
    }

    private getSuppliers() {
        this.coreService.getSuppliersDict(true, false, true).then((x: ISmallGenericType[]) => {
            this.supplierDict = x;
        });
    }

    private getInvoiceSelection() {
        this.selectableInvoiceSelection = [];
        return this.coreService.getTermGroupContent(TermGroup.ReportLedgerInvoiceSelection, false, false, false).then(data => {
            this.selectableInvoiceSelection = data;

            _.forEach(this.selectableInvoiceSelection, (i) => {
                if (i.id == TermGroup_ReportLedgerInvoiceSelection.NotPayedAndPartlyPayed) {
                    this.userSelectionInputInvoiceSelection = new IdSelectionDTO(i.id);
                }
            });
        });
    }

    private getDateRegard() {
        this.selectableDateRegard = [];
        return this.coreService.getTermGroupContent(TermGroup.ReportLedgerDateRegard, false, false, false).then(data => {
            this.selectableDateRegard = data;

            _.forEach(this.selectableInvoiceSelection, (i) => {
                if (i.id == TermGroup_ReportLedgerDateRegard.InvoiceDate) {
                    this.userSelectionInputDateRegard = new IdSelectionDTO(i.id);
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
        this.isFromDateMandatory = (selection.id == TermGroup_ReportLedgerInvoiceSelection.Reconciliation);
        this.validate(this.selectedDateRange);
        
    }

    public onDateRangeSelected(dateRange: DateRangeSelectionDTO) {
        dateRange.useMinMaxIfEmpty = true;
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_DATE_RANGE, dateRange);
        this.validate(dateRange);
    }

    public onDateRegardChanged(selection: IIdSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_DATE_REGARD, selection);
    }

    public onSortOrderChanged(selection: IIdSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_SORT_ORDER, selection);
    }

    public onBoolSelectionShowVoucher(selection: IBoolSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_SHOW_VOUCHER, selection);
    }

    public onBoolSelectionShowPendingPaymentsInReport(selection: IBoolSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_SHOW_PENDING_PAYMENTS_IN_REPORT, selection);
    }

    public onInvoiceSerialNumberFromChanged(selection: ITextSelectionDTO) {
        let id = parseInt(selection.text);
        if (id) {
            if (!this.invoiceSeqNrTo || !this.invoiceSeqNrTo.text) {
                this.invoiceSeqNrTo = selection;
                this.invoiceSeqNrHandler = true;
            }
            this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_INVOICE_NUMBER_FROM, new IdSelectionDTO(id));
        }
    }

    public onInvoiceSerialNumberToChanged(selection: ITextSelectionDTO) {
        if (this.invoiceSeqNrHandler) {
            let id = parseInt(selection.text);
            if (id)
                this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_INVOICE_NUMBER_TO, new IdSelectionDTO(id));
            this.invoiceSeqNrHandler = false;
            this.invoiceSeqNrTo = new TextSelectionDTO(selection.text);
        } else {
            this.invoiceSeqNrHandler = true;
        }
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

    private validate(dateRange: DateRangeSelectionDTO) {
        let invalid = false;
        if (this.isFromDateMandatory) {
            if (!(dateRange.from && CalendarUtility.isValidDate(dateRange.from))) {
                invalid = true;
            }
        }
        if (this.isToDateMandatory) {
            if (!(dateRange.to && CalendarUtility.isValidDate(dateRange.to))) {
                invalid = true;
            }
        }

        this.messagingService.publish(Constants.EVENT_REPORT_VALIDATION_CHANGED, { invalid: invalid });
    }
}

