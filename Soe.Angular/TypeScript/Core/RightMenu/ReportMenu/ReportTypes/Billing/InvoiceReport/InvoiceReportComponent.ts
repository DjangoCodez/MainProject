import { ICommonCustomerService } from "../../../../../../Common/Customer/CommonCustomerService";
import { BoolSelectionDTO, DateRangeSelectionDTO, IdSelectionDTO, TextSelectionDTO } from "../../../../../../Common/Models/ReportDataSelectionDTO";
import { ReportUserSelectionDTO } from "../../../../../../Common/Models/ReportDTOs";
import { SmallGenericType } from "../../../../../../Common/Models/SmallGenericType";
import { IBoolSelectionDTO, IIdSelectionDTO, ISmallGenericType, ITextSelectionDTO, IYearAndPeriodSelectionDTO } from "../../../../../../Scripts/TypeLite.Net4";
import { SoeCategoryType, TermGroup } from "../../../../../../Util/CommonEnumerations";
import { Constants } from "../../../../../../Util/Constants";
import { ICoreService } from "../../../../../Services/CoreService";
import { ITranslationService } from "../../../../../Services/TranslationService";
import { IReportDataService } from "../../../ReportDataService";
import { SelectionCollection } from "../../../SelectionCollection";

export class InvoiceReport {
    selectableSorting: ISmallGenericType[];
    private selectableCustomerCategorySorting: ISmallGenericType[];
    selectableInventoryBalanceList: any[];
    selectableCodeList: any[];
    public static component(): ng.IComponentOptions {
        return {
            controller: InvoiceReport,
            templateUrl: soeConfig.baseUrl + "Core/RightMenu/ReportMenu/ReportTypes/Billing/InvoiceReport/InvoiceReportView.html",
            bindings: {
                userSelection: "=",
                selections: "<",
            }
        } as ng.IComponentOptions;
    }

    public static componentKey = "invoiceReport";

    private userSelection: ReportUserSelectionDTO;
    private selections: SelectionCollection;

    private showUnprintedSelected: BoolSelectionDTO;
    private viewCopiesSelected: BoolSelectionDTO;
    private copiesOfOriginalSelected: BoolSelectionDTO;
    private selectedDateRange: DateRangeSelectionDTO;

    private showRange: boolean;
    private selectedSortItem: IdSelectionDTO;
    private selectedCustomerCategorySortItem: IdSelectionDTO;

    private actorNrFrom: SmallGenericType;
    private actorNrTo: SmallGenericType;

    private customerDict: SmallGenericType[];

    private invoiceSeqNrFrom: TextSelectionDTO;
    private invoiceSeqNrTo: TextSelectionDTO;
    private isDisabledShowCopiesAsOriginal = false;
    


    //@ngInject
    constructor(private $scope: ng.IScope, $timeout: ng.ITimeoutService, private translationService: ITranslationService, private reportDataService: IReportDataService, private coreService: ICoreService, private commonCustomerService: ICommonCustomerService) {
        this.showUnprintedSelected = new BoolSelectionDTO(true);
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

    private setSavedUserFilters(savedValues: ReportUserSelectionDTO) {
        this.showUnprintedSelected = this.userSelection.getBoolSelection(Constants.REPORTMENU_SELECTION_KEY_SHOW_ANY_UNPRINTED);
        this.viewCopiesSelected = this.userSelection.getBoolSelection(Constants.REPORTMENU_SELECTION_KEY_SHOW_COPIES);
        this.copiesOfOriginalSelected = this.userSelection.getBoolSelection(Constants.REPORTMENU_SELECTION_KEY_SHOW_COPIES_OF_ORIGINAL);
        this.selectedCustomerCategorySortItem = this.userSelection.getIdSelection(Constants.REPORTMENU_SELECTION_KEY_CUSTOMER_CATEGORY);
        this.selectedDateRange = savedValues.getDateRangeSelection();

        if (this.userSelection.getTextSelection(Constants.REPORTMENU_SELECTION_KEY_ACTOR_NUMBER_FROM) != null) {
            this.actorNrFrom = _.find(this.customerDict, d => d.name.split(' ')[0] === this.userSelection.getTextSelection(Constants.REPORTMENU_SELECTION_KEY_ACTOR_NUMBER_FROM).text);
            this.customerIDChangedFrom(this.actorNrFrom);
        }
        if (this.userSelection.getTextSelection(Constants.REPORTMENU_SELECTION_KEY_ACTOR_NUMBER_TO) != null) {
            this.actorNrTo = _.find(this.customerDict, d => d.name.split(' ')[0] === this.userSelection.getTextSelection(Constants.REPORTMENU_SELECTION_KEY_ACTOR_NUMBER_TO).text);
            this.customerIDChangedTo(this.actorNrTo);
        }

        this.invoiceSeqNrFrom = this.userSelection.getTextSelection(Constants.REPORTMENU_SELECTION_KEY_INVOICE_NUMBER_FROM);
        this.invoiceSeqNrTo = this.userSelection.getTextSelection(Constants.REPORTMENU_SELECTION_KEY_INVOICE_NUMBER_TO);
        this.selectedSortItem = this.userSelection.getIdSelection(Constants.REPORTMENU_SELECTION_KEY_SORT_ORDER);
    }

    public $onInit() {
        this.getCustomerCategory();
        this.getSortOrder();
        this.getCustomers();
    }

    private getCustomers() {
        this.customerDict = [];
        this.coreService.getCustomers(true, false, true).then((customers: ISmallGenericType[]) => {
            this.customerDict = customers;
        });
    }

    private getCustomerCategory() {
        this.selectableCustomerCategorySorting = [];
        return this.coreService.getCategoriesDict(SoeCategoryType.Customer, true).then(data => {
            this.selectableCustomerCategorySorting = data;
            this.selectedCustomerCategorySortItem = new IdSelectionDTO(this.selectableCustomerCategorySorting[0].id);
        });
    }

    private getSortOrder() {
        this.selectableSorting = [];
        return this.coreService.getTermGroupContent(TermGroup.ReportBillingInvoiceSortOrder, false, true).then(data => {
            this.selectableSorting = data;
            this.selectedSortItem = new IdSelectionDTO(this.selectableSorting[0].id);
        });
    }

    public onSortOrderSelectionChanged(selection: IIdSelectionDTO) {
        
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_SORT_ORDER, selection);
    }

    public onShowUnprinted(selection: IBoolSelectionDTO) {
        
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_SHOW_ANY_UNPRINTED, selection);
    }

    public onViewCopiesSelected(selection: IBoolSelectionDTO) {
        
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_SHOW_COPIES, selection);
        if (selection) {
            this.isDisabledShowCopiesAsOriginal = !selection.value;
        }
    }

    public onCopiesOfOriginalSelected(selection: IBoolSelectionDTO) {
        
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_SHOW_COPIES_OF_ORIGINAL, selection);
    }
    
    public onCustomerCategoryOrderSelectionChanged(selection: IIdSelectionDTO) {
        
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_CUSTOMER_CATEGORY, selection);
    }

    public customerIDChangedFrom(selection: SmallGenericType) {

        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_ACTOR_NUMBER_FROM, new TextSelectionDTO(selection.name.split(' ')[0]));

        if (!this.actorNrTo) {
            this.actorNrTo = selection;
            this.customerIDChangedTo(selection);
        }
    }

    private customerIDChangedTo(selection: SmallGenericType) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_ACTOR_NUMBER_TO, new TextSelectionDTO(selection.name.split(' ')[0]));
    }

    public invoiceNumberChangedFrom(selection: ITextSelectionDTO) {
        var selectFrom = new TextSelectionDTO(selection.text);
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_INVOICE_NUMBER_FROM, selection);

        if (!this.invoiceSeqNrTo || !this.invoiceSeqNrTo.text) {
            this.invoiceSeqNrTo = selectFrom;
        }
    }

    private invoiceNumberChangedTo(selection: ITextSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_INVOICE_NUMBER_TO, selection);
    }

    public onDateRangeSelected(dateRange: DateRangeSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_DATE_RANGE, dateRange);
    }

    public onDateSelectionSelected(selection: IBoolSelectionDTO) {
        this.showRange = !selection.value;
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_DATE_SELECTION_CHOOSEN, selection);
    }

    
}