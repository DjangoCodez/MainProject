import { DateRangeSelectionDTO, IdSelectionDTO } from "../../../../../../Common/Models/ReportDataSelectionDTO";
import { ReportUserSelectionDTO } from "../../../../../../Common/Models/ReportDTOs";
import { SmallGenericType } from "../../../../../../Common/Models/SmallGenericType";
import { IBoolSelectionDTO, IIdSelectionDTO, ISmallGenericType, ITextSelectionDTO } from "../../../../../../Scripts/TypeLite.Net4";
import { SoeCategoryType, TermGroup, TermGroup_ReportLedgerDateRegard, TermGroup_ReportLedgerInvoiceSelection } from "../../../../../../Util/CommonEnumerations";
import { Constants } from "../../../../../../Util/Constants";
import { ICoreService } from "../../../../../Services/CoreService";
import { ITranslationService } from "../../../../../Services/TranslationService";
import { SelectionCollection } from "../../../SelectionCollection";

export class TaxReductionReport {
    public static component(): ng.IComponentOptions {
        return {
            controller: TaxReductionReport,
            templateUrl: soeConfig.baseUrl + "Core/RightMenu/ReportMenu/ReportTypes/Billing/TaxReductionReport/TaxReductionReportView.html",
            bindings: {
                userSelection: "=",
                selections: "<"
            }
        } as ng.IComponentOptions;
    }
    public static componentKey = "taxReductionReport";

   
    private userSelection: ReportUserSelectionDTO;
    private selections: SelectionCollection;
    
    private projectReportTitle = "";

    private selectableCustomerGroup: ISmallGenericType[];
    private selectableDateRegard: ISmallGenericType[];
    private selectableSortOrder: ISmallGenericType[];
    private selectedDateRange: DateRangeSelectionDTO;

    private actorNrFrom: ITextSelectionDTO;
    private actorNrTo: ITextSelectionDTO;
    private invoiceSeqNrFrom: ITextSelectionDTO;
    private invoiceSeqNrTo: ITextSelectionDTO;


    private customerGroup: IdSelectionDTO;
    private dateRegard: IdSelectionDTO;
    private sortOrder: IdSelectionDTO;
    private showNotPrinted: IBoolSelectionDTO;
    private showCopy: IBoolSelectionDTO;
    private invoiceCopyAsOriginal: IBoolSelectionDTO;


    //@ngInject
    constructor(private $scope: ng.IScope, private translationService: ITranslationService, private coreService: ICoreService) {
        this.$scope.$watch(() => this.userSelection, (newVal, oldVal) => {
            if (!newVal)
                return;
            
            this.customerGroup = this.userSelection.getIdSelection(Constants.REPORTMENU_SELECTION_KEY_CUSTOMER_CATEGORY);
            this.invoiceSeqNrFrom = this.userSelection.getTextSelection(Constants.REPORTMENU_SELECTION_KEY_INVOICE_NUMBER_FROM);
            this.invoiceSeqNrTo = this.userSelection.getTextSelection(Constants.REPORTMENU_SELECTION_KEY_INVOICE_NUMBER_TO);
            this.actorNrFrom = this.userSelection.getTextSelection(Constants.REPORTMENU_SELECTION_KEY_ACTOR_NUMBER_FROM);
            this.actorNrTo = this.userSelection.getTextSelection(Constants.REPORTMENU_SELECTION_KEY_ACTOR_NUMBER_TO);

            this.selectedDateRange = this.userSelection.getDateRangeSelection();

            this.dateRegard = this.userSelection.getIdSelection(Constants.REPORTMENU_SELECTION_KEY_DATE_REGARD);
            this.sortOrder = this.userSelection.getIdSelection(Constants.REPORTMENU_SELECTION_KEY_SORT_ORDER);

            this.showNotPrinted = this.userSelection.getBoolSelection(Constants.REPORTMENU_SELECTION_KEY_SHOW_ANY_UNPRINTED);
            this.showCopy = this.userSelection.getBoolSelection(Constants.REPORTMENU_SELECTION_KEY_SHOW_COPIES);
            this.invoiceCopyAsOriginal = this.userSelection.getBoolSelection(Constants.REPORTMENU_SELECTION_KEY_SHOW_COPIES_OF_ORIGINAL);
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

    public $onInit() {
        this.selectedDateRange = new DateRangeSelectionDTO("daterange", null, null);
        this.getCustomerGroup();
        this.getSortOrder();
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
            this.sortOrder = new IdSelectionDTO(this.selectableSortOrder[0].id);
        });
    }

    public onCustomerGroupChanged(selection: IIdSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_CUSTOMER_CATEGORY, selection);
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

    public onInvoiceSerialNumberFromChanged(selection: ITextSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_INVOICE_NUMBER_FROM, selection);
    }

    public onInvoiceSerialNumberToChanged(selection: ITextSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_INVOICE_NUMBER_TO, selection);
    }

    public onActorNumberFromChanged(selection: ITextSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_ACTOR_NUMBER_FROM, selection);
    }

    public onActorNumberToChanged(selection: ITextSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_ACTOR_NUMBER_TO, selection);
        if (!this.actorNrTo || !this.actorNrTo.text) {
            this.actorNrTo = selection;
        }
    }

    public onBoolSelectionShowNotPrinted(selection: IBoolSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_SHOW_ANY_UNPRINTED, selection);
    }

    public onBoolSelectionShowCopy(selection: IBoolSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_SHOW_COPIES, selection);
    }

    public onBoolSelectionInvoiceCopyAsOriginal(selection: IBoolSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_SHOW_COPIES_OF_ORIGINAL, selection);
    }
}

