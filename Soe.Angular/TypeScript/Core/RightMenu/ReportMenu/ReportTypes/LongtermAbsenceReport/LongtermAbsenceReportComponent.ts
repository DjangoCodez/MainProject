import { ReportUserSelectionDTO } from "../../../../../Common/Models/ReportDTOs";
import { SelectionCollection } from "../../SelectionCollection";
import { BoolSelectionDTO, DateRangeSelectionDTO, EmployeeSelectionDTO, MatrixColumnsSelectionDTO, PayrollProductRowSelectionDTO, TextSelectionDTO } from "../../../../../Common/Models/ReportDataSelectionDTO";
import { IBoolSelectionDTO, IDateRangeSelectionDTO, IEmployeeSelectionDTO, IMatrixColumnsSelectionDTO, IPayrollProductRowSelectionDTO, ITextSelectionDTO } from "../../../../../Scripts/TypeLite.Net4";
import { Constants } from "../../../../../Util/Constants";
import { SoeModule } from "../../../../../Util/CommonEnumerations";


export class LongtermAbsenceReport {
    public static component(): ng.IComponentOptions {
        const options: ng.IComponentOptions = {
            controller: LongtermAbsenceReport,
            templateUrl: soeConfig.baseUrl + "Core/RightMenu/ReportMenu/ReportTypes/LongtermAbsenceReport/LongtermAbsenceReportView.html",
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
    public static componentKey = "longtermAbsenceReport";

    //binding fields
    private module: SoeModule;
    private userSelection: ReportUserSelectionDTO;
    private selections: SelectionCollection;
    private fromDate: Date;
    private toDate: Date;
    private isAnalysis: boolean;
    private sysReportTemplateTypeId: number;
    private reportId: number;

    //user selections
    private userSelectionInputDate: DateRangeSelectionDTO;
    private userSelectionInputEmployee: EmployeeSelectionDTO;
    private userSelectionInputColumns: MatrixColumnsSelectionDTO;
    private userSelectionInputNumberOfDays: TextSelectionDTO;
    private userSelectionInputFilterOnAccounting: BoolSelectionDTO;
    private userSelectionInputPayrollProduct: PayrollProductRowSelectionDTO[];

    //@ngInject
    constructor(
        private $scope: ng.IScope) {

        this.$scope.$watch(() => this.userSelection, (newVal, oldVal) => {
            if (!newVal)
                return;

            this.userSelectionInputDate = this.userSelection.getDateRangeSelection();
            this.userSelectionInputEmployee = this.userSelection.getEmployeeSelection();
            this.userSelectionInputColumns = this.userSelection.getMatrixColumnSelection();
            this.userSelectionInputNumberOfDays = this.userSelection.getTextSelection(Constants.REPORTMENU_SELECTION_KEY_TIME_NUMBEROFDAYS);
            this.userSelectionInputFilterOnAccounting = this.userSelection.getBoolSelection(Constants.REPORTMENU_SELECTION_KEY_FILTER_ON_ACCOUNTING);
            this.userSelectionInputPayrollProduct = this.userSelection.getPayrollProductRowSelections();
            this.fromDate = this.userSelectionInputDate ? this.userSelectionInputDate.from : null;
            this.toDate = this.userSelectionInputDate ? this.userSelectionInputDate.to : null;
        });
    }

    public $onInit() {
        this.fromDate = new Date();
        this.toDate = new Date();

        if (!this.userSelectionInputNumberOfDays)
            this.userSelectionInputNumberOfDays = new TextSelectionDTO("14");
        else
            this.userSelectionInputNumberOfDays.text = "14";
    }

    public onDateRangeSelectionUpdated(selection: IDateRangeSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_DATE_RANGE, selection);

        this.fromDate = selection.from;
        this.toDate = selection.to;
    }

    public onEmployeeSelectionUpdated(selection: IEmployeeSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_EMPLOYEES, selection);
    }

    public onMatrixColumnSelectionUpdated(selection: IMatrixColumnsSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_MATRIX_COLUMNS, selection);
    }

    public onNumberOfDaysChanged(selection: ITextSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_TIME_NUMBEROFDAYS, selection);
    }

    public onBoolSelectionFilterOnAccountingUpdated(selection: IBoolSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_FILTER_ON_ACCOUNTING, selection);
    }

    public onPayrollProductSelectionUpdated(selection: IPayrollProductRowSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_PAYROLL_PRODUCTS, selection);
    }
}
