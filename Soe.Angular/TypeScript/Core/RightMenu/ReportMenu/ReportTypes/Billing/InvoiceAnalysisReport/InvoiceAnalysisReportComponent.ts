import { ReportUserSelectionDTO } from "../../../../../../Common/Models/ReportDTOs";
import { SelectionCollection } from "../../../SelectionCollection";
import { DateRangeSelectionDTO, MatrixColumnsSelectionDTO, BoolSelectionDTO } from "../../../../../../Common/Models/ReportDataSelectionDTO";
import { IDateRangeSelectionDTO, IMatrixColumnsSelectionDTO, IBoolSelectionDTO } from "../../../../../../Scripts/TypeLite.Net4";
import { Constants } from "../../../../../../Util/Constants";
import { SoeModule } from "../../../../../../Util/CommonEnumerations";

export class InvoiceAnalysisReport {
    public static component(): ng.IComponentOptions {
        const options: ng.IComponentOptions = {
            controller: InvoiceAnalysisReport,
            templateUrl: soeConfig.baseUrl + "Core/RightMenu/ReportMenu/ReportTypes/Billing/InvoiceAnalysisReport/InvoiceAnalysisReportView.html",
            bindings: {
                module: "<",
                userSelection: "=",
                selections: "<",
                sysReportTemplateTypeId: "<",
                reportId: "<",
                isAnalysis: "<"
            }
        };

        return options;
    }
    public static componentKey = "invoiceAnalysisReport";

    //binding fields
    private module: SoeModule;
    private userSelection: ReportUserSelectionDTO;
    private selections: SelectionCollection;
    private sysReportTemplateTypeId: number;
    private reportId: number;
    private isAnalysis: boolean;

    //user selections
    private userSelectionInputDate: DateRangeSelectionDTO;
    private userSelectionInputInvoiceDate: DateRangeSelectionDTO;
    private userSelectionInputDueDate: DateRangeSelectionDTO;
    private userSelectionInputColumns: MatrixColumnsSelectionDTO;
    private userSelectionInputShowOpen: BoolSelectionDTO;
    private userSelectionInputShowClosed: BoolSelectionDTO;
    private userSelectionInputViewMy: BoolSelectionDTO;
    private userSelectionInputPreliminary: BoolSelectionDTO;

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

    private setSavedUserFilters(savedValues: ReportUserSelectionDTO) {
        this.userSelectionInputInvoiceDate = this.userSelection.getDateRangeSelectionFromKey("invoiceDate");
        this.userSelectionInputDueDate = this.userSelection.getDateRangeSelectionFromKey("dueDate");
        this.userSelectionInputColumns = this.userSelection.getMatrixColumnSelection();
        this.userSelectionInputShowOpen = savedValues.getBoolSelection(Constants.REPORTMENU_SELECTION_KEY_SHOW_OPEN);
        this.userSelectionInputShowClosed = savedValues.getBoolSelection(Constants.REPORTMENU_SELECTION_KEY_SHOW_CLOSED);
        this.userSelectionInputViewMy = savedValues.getBoolSelection(Constants.REPORTMENU_SELECTION_KEY_VIEW_MY);
        this.userSelectionInputPreliminary = savedValues.getBoolSelection(Constants.REPORTMENU_SELECTION_KEY_SHOW_PRELIMINARY_INVOICES);
    }

    public $onInit() {
        this.userSelectionInputShowOpen = new BoolSelectionDTO(true);
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
    public onBoolSelectionInputPreliminary(selection: IBoolSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_SHOW_PRELIMINARY_INVOICES, selection);
    }

    public onDueDateSelectionUpdated(selection: IDateRangeSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_DUE_DATE, selection);
    }

    public onInvoiceDateSelectionUpdated(selection: IDateRangeSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_INVOICE_DATE, selection);
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
