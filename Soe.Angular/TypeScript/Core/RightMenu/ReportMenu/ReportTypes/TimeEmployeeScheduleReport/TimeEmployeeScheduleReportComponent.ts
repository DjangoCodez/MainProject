import { ReportUserSelectionDTO } from "../../../../../Common/Models/ReportDTOs";
import { SelectionCollection } from "../../SelectionCollection";
import { BoolSelectionDTO, DateRangeSelectionDTO, EmployeeSelectionDTO, IdListSelectionDTO, MatrixColumnsSelectionDTO } from "../../../../../Common/Models/ReportDataSelectionDTO";
import { IBoolSelectionDTO, IDateRangeSelectionDTO, IEmployeeSelectionDTO, IIdListSelectionDTO, IMatrixColumnsSelectionDTO } from "../../../../../Scripts/TypeLite.Net4";
import { Constants } from "../../../../../Util/Constants";
import { SoeModule } from "../../../../../Util/CommonEnumerations";

export class TimeEmployeeScheduleReport {
    public static component(): ng.IComponentOptions {
        const options: ng.IComponentOptions = {
            controller: TimeEmployeeScheduleReport,
            templateUrl: soeConfig.baseUrl + "Core/RightMenu/ReportMenu/ReportTypes/TimeEmployeeScheduleReport/TimeEmployeeScheduleReportView.html",
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
    public static componentKey = "timeEmployeeScheduleReport";

    //binding fields
    private module: SoeModule;
    private userSelection: ReportUserSelectionDTO;
    private selections: SelectionCollection;
    private fromDate: Date;
    private toDate: Date;
    private sysReportTemplateTypeId: number;
    private reportId: number;
    private isAnalysis: boolean;

    //user selections
    private userSelectionInputDate: DateRangeSelectionDTO;
    private userSelectionInputEmployee: EmployeeSelectionDTO;
    private userSelectionInputShiftType: IdListSelectionDTO;
    private userSelectionInputFilterOnAccounting: BoolSelectionDTO;
    private userSelectionInputExcludeFreeDays: BoolSelectionDTO;
    private userSelectionInputIncludeBreaks: BoolSelectionDTO;
    private userSelectionInputColumns: MatrixColumnsSelectionDTO;
    private userSelectionInputExcludeAbsence: BoolSelectionDTO;

    //@ngInject
    constructor(
        private $scope: ng.IScope) {

        this.$scope.$watch(() => this.userSelection, (newVal, oldVal) => {
            if (!newVal)
                return;

            this.userSelectionInputDate = this.userSelection.getDateRangeSelection();
            this.userSelectionInputEmployee = this.userSelection.getEmployeeSelection();
            this.userSelectionInputShiftType = this.userSelection.getIdListSelection(Constants.REPORTMENU_SELECTION_KEY_SHIFT_TYPES);
            this.userSelectionInputFilterOnAccounting = this.userSelection.getBoolSelection(Constants.REPORTMENU_SELECTION_KEY_FILTER_ON_ACCOUNTING);
            this.userSelectionInputExcludeFreeDays = this.userSelection.getBoolSelection(Constants.REPORTMENU_SELECTION_KEY_EXCLUDE_FREE_DAYS);
            this.userSelectionInputIncludeBreaks = this.userSelection.getBoolSelection(Constants.REPORTMENU_SELECTION_KEY_INCLUDE_BREAKS);
            this.userSelectionInputExcludeAbsence = this.userSelection.getBoolSelection(Constants.REPORTMENU_SELECTION_KEY_EXCLUDE_ABSENCE);
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

    public onShiftTypeSelectionUpdated(selection: IIdListSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_SHIFT_TYPES, selection);
    }

    public onEmployeeSelectionUpdated(selection: IEmployeeSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_EMPLOYEES, selection);
    }

    public onBoolSelectionFilterOnAccountingUpdated(selection: IBoolSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_FILTER_ON_ACCOUNTING, selection);
    }

    public onBoolSelectionExcludeFreeDaysUpdated(selection: IBoolSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_EXCLUDE_FREE_DAYS, selection);
    }

    public onBoolSelectionIncludeBreaksUpdated(selection: IBoolSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_INCLUDE_BREAKS, selection);
    }

    public onBoolSelectionExcludeAbsenceUpdated(selection: IBoolSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_EXCLUDE_ABSENCE, selection);
    }

    public onMatrixColumnSelectionUpdated(selection: IMatrixColumnsSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_MATRIX_COLUMNS, selection);
    }    
}
