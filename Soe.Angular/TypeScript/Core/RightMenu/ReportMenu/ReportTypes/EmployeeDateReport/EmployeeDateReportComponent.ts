import { ReportUserSelectionDTO } from "../../../../../Common/Models/ReportDTOs";
import { SelectionCollection } from "../../SelectionCollection";
import { BoolSelectionDTO, DateRangeSelectionDTO, EmployeeSelectionDTO, MatrixColumnsSelectionDTO } from "../../../../../Common/Models/ReportDataSelectionDTO";
import { IBoolSelectionDTO, IDateRangeSelectionDTO, IEmployeeSelectionDTO, IMatrixColumnsSelectionDTO } from "../../../../../Scripts/TypeLite.Net4";
import { Constants } from "../../../../../Util/Constants";
import { SoeModule } from "../../../../../Util/CommonEnumerations";

export class EmployeeDateReport {
    public static component(): ng.IComponentOptions {
        const options: ng.IComponentOptions = {
            controller: EmployeeDateReport,
            templateUrl: soeConfig.baseUrl + "Core/RightMenu/ReportMenu/ReportTypes/EmployeeDateReport/EmployeeDateReportView.html",
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
    public static componentKey = "employeeDateReport";

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
    private userSelectionOnlyDateWithChanges: BoolSelectionDTO;
    private userSelectionInputColumns: MatrixColumnsSelectionDTO;

    //@ngInject
    constructor(
        private $scope: ng.IScope) {

        this.$scope.$watch(() => this.userSelection, (newVal, oldVal) => {
            if (!newVal)
                return;

            this.userSelectionInputDate = this.userSelection.getDateRangeSelection();
            this.userSelectionInputEmployee = this.userSelection.getEmployeeSelection();
            this.userSelectionOnlyDateWithChanges = this.userSelection.getBoolSelection("onlyDateWithChanges");
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

    public onOnlyDateWithChangesSelectionUpdated(selection: IBoolSelectionDTO) {
        this.selections.upsert("onlyDateWithChanges", selection);
    }

    public onMatrixColumnSelectionUpdated(selection: IMatrixColumnsSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_MATRIX_COLUMNS, selection);
    }
}
