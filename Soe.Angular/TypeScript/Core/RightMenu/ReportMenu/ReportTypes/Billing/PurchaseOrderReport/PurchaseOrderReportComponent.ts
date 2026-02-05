import { DateRangeSelectionDTO, IdSelectionDTO } from "../../../../../../Common/Models/ReportDataSelectionDTO";
import { ReportUserSelectionDTO } from "../../../../../../Common/Models/ReportDTOs";
import { IBoolSelectionDTO, IDateRangeSelectionDTO, IIdSelectionDTO, ISmallGenericType, ITextSelectionDTO } from "../../../../../../Scripts/TypeLite.Net4";
import { TermGroup } from "../../../../../../Util/CommonEnumerations";
import { Constants } from "../../../../../../Util/Constants";
import { ICoreService } from "../../../../../Services/CoreService";
import { ITranslationService } from "../../../../../Services/TranslationService";
import { IReportDataService } from "../../../ReportDataService";
import { SelectionCollection } from "../../../SelectionCollection";

export class PurchaseOrderReport {
    public static component(): ng.IComponentOptions {
        return {
            controller: PurchaseOrderReport,
            templateUrl: soeConfig.baseUrl + "Core/RightMenu/ReportMenu/ReportTypes/Billing/PurchaseOrderReport/PurchaseOrderReportView.html",
            bindings: {
                userSelection: "=",
                selections: "<"
            }
        } as ng.IComponentOptions;
    }

    public static componentKey = "purchaseOrderReport";

    private userSelection: ReportUserSelectionDTO;
    private selections: SelectionCollection;

    private selectableSortOrder: ISmallGenericType[];

    private showRange: boolean;

    private supplierNrFrom: ITextSelectionDTO;
    private supplierNrTo: ITextSelectionDTO;
    private purchaseNrFrom: ITextSelectionDTO;
    private purchaseNrTo: ITextSelectionDTO;

    private userSelectionInputSortOrder: IdSelectionDTO;


    //@ngInject
    constructor(private $scope: ng.IScope, $timeout: ng.ITimeoutService, private translationService: ITranslationService, private reportDataService: IReportDataService, private coreService: ICoreService) {
        this.$scope.$watch(() => this.userSelection, (newVal, oldVal) => {
            if (!newVal)
                return;

            this.purchaseNrFrom = this.userSelection.getTextSelection(Constants.REPORTMENU_SELECTION_KEY_INVOICE_PURCHASE_NUMBER_FROM);
            this.purchaseNrTo = this.userSelection.getTextSelection(Constants.REPORTMENU_SELECTION_KEY_INVOICE_PURCHASE_NUMBER_TO);
            this.supplierNrFrom = this.userSelection.getTextSelection(Constants.REPORTMENU_SELECTION_KEY_ACTOR_NUMBER_FROM);
            this.supplierNrTo = this.userSelection.getTextSelection(Constants.REPORTMENU_SELECTION_KEY_ACTOR_NUMBER_TO);
            this.userSelectionInputSortOrder = this.userSelection.getIdSelection(Constants.REPORTMENU_SELECTION_KEY_SORT_ORDER);
        });
    }

    public $onInit() {
        this.getSortOrder();
    }

    private getSortOrder() {
        this.selectableSortOrder = [];
        return this.coreService.getTermGroupContent(TermGroup.PurchaseOrderSortOrder, false, false, false).then(data => {
            this.selectableSortOrder = data;
            this.userSelectionInputSortOrder = new IdSelectionDTO(this.selectableSortOrder[0].id);
        });
    }

    public onSortOrderChanged(selection: IIdSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_SORT_ORDER, selection);
    }

    public onPurchaseNumberFromChanged(selection: ITextSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_INVOICE_PURCHASE_NUMBER_FROM, selection);
        if (!this.purchaseNrTo || !this.purchaseNrTo.text)
        {
            this.purchaseNrTo = selection;
    }
    }

    public onPurchaseNumberToChanged(selection: ITextSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_INVOICE_PURCHASE_NUMBER_TO, selection);
    }

    public onActorNumberFromChanged(selection: ITextSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_ACTOR_NUMBER_FROM, selection);
        if (!this.supplierNrTo || !this.supplierNrTo.text)
        {
            this.supplierNrTo = selection;
        }
    }

    public onActorNumberToChanged(selection: ITextSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_ACTOR_NUMBER_TO, selection);
    }

    public onDateRangeSelected(dateRange: DateRangeSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_DATE_RANGE, dateRange);
    }

    public onDateSelectionSelected(selection: IBoolSelectionDTO) {
        this.showRange = !selection.value;
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_DATE_SELECTION_CHOOSEN, selection);
    }

}