import { ReportUserSelectionDTO } from "../../../../../../Common/Models/ReportDTOs";
import { SelectionCollection } from "../../../SelectionCollection";
import { DateRangeSelectionDTO, DateSelectionDTO, MatrixColumnsSelectionDTO, BoolSelectionDTO } from "../../../../../../Common/Models/ReportDataSelectionDTO";
import { IDateRangeSelectionDTO, IMatrixColumnsSelectionDTO, IBoolSelectionDTO } from "../../../../../../Scripts/TypeLite.Net4";
import { Constants } from "../../../../../../Util/Constants";
import { SoeModule } from "../../../../../../Util/CommonEnumerations";

export class OrderAnalysisReport {
    public static component(): ng.IComponentOptions {
        const options: ng.IComponentOptions = {
            controller: OrderAnalysisReport,
            templateUrl: soeConfig.baseUrl + "Core/RightMenu/ReportMenu/ReportTypes/Billing/OrderAnalysisReport/OrderAnalysisReportView.html",
            bindings: {
                module: "<",
                userSelection: "=",
                selections: "<",
                fromDate: "<",
                toDate: "<",
                sysReportTemplateTypeId: "<",
                reportId: "<",
                isAnalysis: "<"
            }
        };

        return options;
    }
    public static componentKey = "orderAnalysisReport";

    //binding fields
    private module: SoeModule;
    private userSelection: ReportUserSelectionDTO;
    private selections: SelectionCollection;
    private fromDate: Date;
    private toDate: Date;
    private sysReportTemplateTypeId: number;
    private reportId: number;
    private isAnalysis: boolean;

    private showOpen: boolean;
    private showClosed: boolean;
    private viewMy: boolean;

    //user selections
    private userSelectionInputOrderDate: DateRangeSelectionDTO;
    private userSelectionInputDeliveryDate: DateRangeSelectionDTO;
    private userSelectionInputColumns: MatrixColumnsSelectionDTO;
    private userSelectionInputShowOpen: BoolSelectionDTO;
    private userSelectionInputShowClosed: BoolSelectionDTO;
    private userSelectionInputViewMy: BoolSelectionDTO;
    private deliveryDate: DateSelectionDTO;
    private orderDate: DateSelectionDTO;

    //@ngInject
    constructor(
        private $scope: ng.IScope,
    ) {
        this.$scope.$watch(() => this.userSelection, (newVal, oldVal) => {
            if (!newVal)
                return;

            this.setSavedUserFilters(newVal);
        });
    }

    public $onInit() {
        this.userSelectionInputShowOpen = new BoolSelectionDTO(true);
    }

    private setSavedUserFilters(savedValues: ReportUserSelectionDTO) {
        this.userSelectionInputOrderDate = this.userSelection.getDateRangeSelectionFromKey("orderDate");
        this.userSelectionInputDeliveryDate = this.userSelection.getDateRangeSelectionFromKey("deliveryDate");
        this.userSelectionInputColumns = this.userSelection.getMatrixColumnSelection();
        this.userSelectionInputShowOpen = this.userSelection.getBoolSelection(Constants.REPORTMENU_SELECTION_KEY_SHOW_OPEN);
        this.userSelectionInputShowClosed = savedValues.getBoolSelection(Constants.REPORTMENU_SELECTION_KEY_SHOW_CLOSED);
        this.userSelectionInputViewMy = savedValues.getBoolSelection(Constants.REPORTMENU_SELECTION_KEY_VIEW_MY);

    }
    public onMatrixColumnSelectionUpdated(selection: IMatrixColumnsSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_MATRIX_COLUMNS, selection);
    }

    public onBoolSelectionInputShowOpen(selection: IBoolSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_SHOW_OPEN, selection);
    }

    public onBoolSelectionInputShowClosed(selection: IBoolSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_SHOW_CLOSED, selection);
    }

    public onBoolSelectionInputViewMy(selection: IBoolSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_VIEW_MY, selection);
    }

    public onDeliveryDateSelectionUpdated(selection: IDateRangeSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_DELIVERY_DATE, selection);
    }

    public onOrderDateSelectionUpdated(selection: IDateRangeSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_ORDER_DATE, selection);
    }

    public onAccountDimSelectionUpdated(selection: any) {
        if (selection[0].selectionKey == Constants.REPORTMENU_SELECTION_KEY_ACCOUNT_DIM1 && selection[0].accountIds.length) {
            this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_ACCOUNT_DIM1, selection[0]);
        }
        else if (selection[0].selectionKey == Constants.REPORTMENU_SELECTION_KEY_ACCOUNT_DIM2 && selection[0].accountIds.length) {
            this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_ACCOUNT_DIM2, selection[0]);
        }
        if (selection[0].selectionKey == Constants.REPORTMENU_SELECTION_KEY_ACCOUNT_DIM3 && selection[0].accountIds.length) {
            this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_ACCOUNT_DIM3, selection[0]);
        }
        if (selection[0].selectionKey == Constants.REPORTMENU_SELECTION_KEY_ACCOUNT_DIM4 && selection[0].accountIds.length) {
          this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_ACCOUNT_DIM4, selection[0]);
        }
        if (selection[0].selectionKey == Constants.REPORTMENU_SELECTION_KEY_ACCOUNT_DIM5 && selection[0].accountIds.length) {
            this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_ACCOUNT_DIM5, selection[0]);
        }
        if (selection[0].selectionKey == Constants.REPORTMENU_SELECTION_KEY_ACCOUNT_DIM6 && selection[0].accountIds.length) {
             this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_ACCOUNT_DIM6, selection[0]);
        }
    }

}
