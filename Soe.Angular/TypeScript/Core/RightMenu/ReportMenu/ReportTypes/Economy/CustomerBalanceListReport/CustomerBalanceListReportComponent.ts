import { ICommonCustomerService } from "../../../../../../Common/Customer/CommonCustomerService";
import { DateRangeSelectionDTO, IdSelectionDTO, TextSelectionDTO } from "../../../../../../Common/Models/ReportDataSelectionDTO";
import { ReportUserSelectionDTO } from "../../../../../../Common/Models/ReportDTOs";
import { SmallGenericType } from "../../../../../../Common/Models/SmallGenericType";
import { IBoolSelectionDTO, IIdSelectionDTO, ISmallGenericType, ITextSelectionDTO } from "../../../../../../Scripts/TypeLite.Net4";
import { CalendarUtility } from "../../../../../../Util/CalendarUtility";
import { TermGroup, TermGroup_ReportLedgerDateRegard, TermGroup_ReportLedgerInvoiceSelection } from "../../../../../../Util/CommonEnumerations";
import { Constants } from "../../../../../../Util/Constants";
import { ICoreService } from "../../../../../Services/CoreService";
import { IMessagingService } from "../../../../../Services/MessagingService";
import { SelectionCollection } from "../../../SelectionCollection";

export class CustomerBalanceListReport {
    public static component(): ng.IComponentOptions {
        return {
            controller: CustomerBalanceListReport,
            templateUrl: soeConfig.baseUrl + "Core/RightMenu/ReportMenu/ReportTypes/Economy/CustomerBalanceListReport/CustomerBalanceListReportView.html",
            bindings: {
                userSelection: "=",
                selections: "<"
            }
        } as ng.IComponentOptions;
    }
    public static componentKey = "customerBalanceListReport";

    private userSelection: ReportUserSelectionDTO;
    private selections: SelectionCollection;
    private selectableInvoiceSelection: ISmallGenericType[];
    private selectableDateRegard: ISmallGenericType[];
    private selectableSortOrder: ISmallGenericType[];
    private selectedDateRange: DateRangeSelectionDTO;
    private actorNrFrom: SmallGenericType;
    private actorNrTo: SmallGenericType;
    private invoiceSeqNrFrom: ITextSelectionDTO
    private invoiceSeqNrTo: ITextSelectionDTO;
    private invoiceSelection: IdSelectionDTO;
    private dateRegard: IdSelectionDTO;
    private sortOrder: IdSelectionDTO;
    private showVoucher: IBoolSelectionDTO;
    private customerDict: SmallGenericType[];
    private isFromDateMandatory = false;
    private isToDateMandatory = true;
    

    //@ngInject
    constructor(private $scope: ng.IScope, private coreService: ICoreService, private commonCustomerService: ICommonCustomerService, protected messagingService: IMessagingService,) {
        this.$scope.$watch(() => this.userSelection, (newVal, oldVal) => {
            if (!newVal)
                return;

            this.invoiceSelection = this.userSelection.getIdSelection(Constants.REPORTMENU_SELECTION_KEY_INVOICE_SELECTION);
            var invoiceNrFrom = this.userSelection.getIdSelection(Constants.REPORTMENU_SELECTION_KEY_INVOICE_NUMBER_FROM);
            if (invoiceNrFrom != null) {
                this.invoiceSeqNrFrom = new TextSelectionDTO(invoiceNrFrom.id.toString());
            }

            var invoiceNrTo = this.userSelection.getIdSelection(Constants.REPORTMENU_SELECTION_KEY_INVOICE_NUMBER_TO);
            if (invoiceNrTo != null) {
                this.invoiceSeqNrTo = new TextSelectionDTO(invoiceNrTo.id.toString());
            }

            if (this.userSelection.getTextSelection(Constants.REPORTMENU_SELECTION_KEY_ACTOR_NUMBER_FROM) != null) {
                this.actorNrFrom = _.find(this.customerDict, d => d.name.split(' ')[0] === this.userSelection.getTextSelection(Constants.REPORTMENU_SELECTION_KEY_ACTOR_NUMBER_FROM).text);
                this.onActorNumberFromChanged(this.actorNrFrom);
            }

            if (this.userSelection.getTextSelection(Constants.REPORTMENU_SELECTION_KEY_ACTOR_NUMBER_TO) != null) {
                this.actorNrTo = _.find(this.customerDict, d => d.name.split(' ')[0] === this.userSelection.getTextSelection(Constants.REPORTMENU_SELECTION_KEY_ACTOR_NUMBER_TO).text);
                this.onActorNumberToChanged(this.actorNrTo);
            }

            this.selectedDateRange = this.userSelection.getDateRangeSelection();
            this.dateRegard = this.userSelection.getIdSelection(Constants.REPORTMENU_SELECTION_KEY_DATE_REGARD);
            this.sortOrder = this.userSelection.getIdSelection(Constants.REPORTMENU_SELECTION_KEY_SORT_ORDER);
            this.showVoucher = this.userSelection.getBoolSelection(Constants.REPORTMENU_SELECTION_KEY_SHOW_VOUCHER);
        });

    }

    public $onInit() {
        this.selectedDateRange = new DateRangeSelectionDTO("daterange", null, new Date());
        this.getInvoiceSelection();
        this.getDateRegard();
        this.getSortOrder();
        this.getCustomers();
        this.validate(this.selectedDateRange);
    }

    private getCustomers() {
        this.customerDict = [];
        this.coreService.getCustomers(true, false, true).then((customers: ISmallGenericType[]) => {
            this.customerDict = customers;
        });
    }

    private getInvoiceSelection() {
        this.selectableInvoiceSelection = [];
        return this.coreService.getTermGroupContent(TermGroup.ReportLedgerInvoiceSelection, false, false, false).then(data => {
            this.selectableInvoiceSelection = data;
            _.forEach(this.selectableInvoiceSelection, (i) => {
                if (i.id == TermGroup_ReportLedgerInvoiceSelection.NotPayedAndPartlyPayed) {
                    this.invoiceSelection = new IdSelectionDTO(i.id);
                }
            });
        });
    }

    private getDateRegard() {
        this.selectableDateRegard = [];
        return this.coreService.getTermGroupContent(TermGroup.ReportLedgerDateRegard, false, false, false).then(data => {
            this.selectableDateRegard = data;
            _.forEach(this.selectableDateRegard, (i) => {
                if (i.id == TermGroup_ReportLedgerDateRegard.InvoiceDate) {
                    this.dateRegard = new IdSelectionDTO(i.id);
                }
            });
        });
    }

    private getSortOrder() {
        this.selectableSortOrder = [];
        return this.coreService.getTermGroupContent(TermGroup.ReportCustomerLedgerSortOrder, false, false, false).then(data => {
            this.selectableSortOrder = data;
            this.sortOrder = new IdSelectionDTO(this.selectableSortOrder[0].id);
        });
    }

    public onInvoiceSelectionChanged(selection: IIdSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_INVOICE_SELECTION, selection);
        this.isFromDateMandatory = (selection.id == TermGroup_ReportLedgerInvoiceSelection.Reconciliation);
        this.validate(this.selectedDateRange);
    }

    public onDateRangeSelected(dateRange: DateRangeSelectionDTO) {
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
        if(id)
          this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_INVOICE_NUMBER_TO, new IdSelectionDTO(id));
    }

    public onActorNumberFromChanged(selection: SmallGenericType) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_ACTOR_NUMBER_FROM, new TextSelectionDTO(selection.name.split(' ')[0]));

        if (!this.actorNrTo) {
            this.actorNrTo = selection;
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

