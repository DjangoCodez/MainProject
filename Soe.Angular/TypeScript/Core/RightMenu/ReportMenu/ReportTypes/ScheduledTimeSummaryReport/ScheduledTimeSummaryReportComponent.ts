import { ReportUserSelectionDTO } from "../../../../../Common/Models/ReportDTOs";
import { SelectionCollection } from "../../SelectionCollection";
import { DateRangeSelectionDTO, EmployeeSelectionDTO, MatrixColumnsSelectionDTO } from "../../../../../Common/Models/ReportDataSelectionDTO";
import { IDateRangeSelectionDTO, IEmployeeSelectionDTO, IMatrixColumnsSelectionDTO } from "../../../../../Scripts/TypeLite.Net4";
import { Constants } from "../../../../../Util/Constants";
import { SoeModule } from "../../../../../Util/CommonEnumerations";

export class ScheduledTimeSummaryReport {
    public static component(): ng.IComponentOptions {
        const options: ng.IComponentOptions = {
            controller: ScheduledTimeSummaryReport,
            templateUrl: soeConfig.baseUrl + "Core/RightMenu/ReportMenu/ReportTypes/ScheduledTimeSummaryReport/ScheduledTimeSummaryReportView.html",
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
    public static componentKey = "scheduledTimeSummaryReport";

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

    //@ngInject
    constructor(
        private $scope: ng.IScope) {

        this.$scope.$watch(() => this.userSelection, (newVal, oldVal) => {
            if (!newVal)
                return;

            this.userSelectionInputDate = this.userSelection.getDateRangeSelection();
            this.userSelectionInputEmployee = this.userSelection.getEmployeeSelection();
            this.userSelectionInputColumns = this.userSelection.getMatrixColumnSelection();
            this.fromDate = this.userSelectionInputDate ? this.userSelectionInputDate.from : null;
            this.toDate = this.userSelectionInputDate ? this.userSelectionInputDate.to : null;
        });
    }

    public $onInit() {
        this.fromDate = new Date();
        this.toDate = new Date();
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
}
