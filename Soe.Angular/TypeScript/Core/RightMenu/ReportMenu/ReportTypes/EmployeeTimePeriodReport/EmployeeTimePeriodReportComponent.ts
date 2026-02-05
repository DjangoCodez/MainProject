import { ReportUserSelectionDTO } from "../../../../../Common/Models/ReportDTOs";
import { SelectionCollection } from "../../SelectionCollection";
import { EmployeeSelectionDTO, PayrollProductRowSelectionDTO, IdListSelectionDTO, BoolSelectionDTO, MatrixColumnsSelectionDTO } from "../../../../../Common/Models/ReportDataSelectionDTO";
import { IEmployeeSelectionDTO, IPayrollProductRowSelectionDTO, IBoolSelectionDTO, IIdListSelectionDTO, IMatrixColumnsSelectionDTO } from "../../../../../Scripts/TypeLite.Net4";
import { Constants } from "../../../../../Util/Constants";
import { SoeModule } from "../../../../../Util/CommonEnumerations";

export class EmployeeTimePeriodReport {
    public static component(): ng.IComponentOptions {
        const options: ng.IComponentOptions = {
            controller: EmployeeTimePeriodReport,
            templateUrl: soeConfig.baseUrl + "Core/RightMenu/ReportMenu/ReportTypes/EmployeeTimePeriodReport/EmployeeTimePeriodReportView.html",
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
    public static componentKey = "employeeTimePeriodReport";

    //binding fields
    private module: SoeModule;
    private userSelection: ReportUserSelectionDTO;
    private selections: SelectionCollection;
    private timePeriodIds: number[];
    private sysReportTemplateTypeId: number;
    private reportId: number;
    private isAnalysis: boolean;


    //user selections
    private userSelectionInputPayrollPeriod: IdListSelectionDTO;
    private userSelectionInputEmployee: EmployeeSelectionDTO;
    private userSelectionInputColumns: MatrixColumnsSelectionDTO;

    //@ngInject
    constructor(
        private $scope: ng.IScope) {

        this.$scope.$watch(() => this.userSelection, (newVal, oldVal) => {
            if (!newVal)
                return;

            this.userSelectionInputPayrollPeriod = this.userSelection.getIdListSelection(Constants.REPORTMENU_SELECTION_KEY_PERIODS);
            this.userSelectionInputEmployee = this.userSelection.getEmployeeSelection();
            this.timePeriodIds = this.userSelectionInputPayrollPeriod ? this.userSelectionInputPayrollPeriod.ids : null;
            this.userSelectionInputColumns = this.userSelection.getMatrixColumnSelection();
        });
    }

    public $onInit() {
    }

    public onPeriodSelectionUpdated(selection: IIdListSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_PERIODS, selection);
        this.timePeriodIds = selection.ids;
    }

    public onEmployeeSelectionUpdated(selection: IEmployeeSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_EMPLOYEES, selection);
    }

    public onMatrixColumnSelectionUpdated(selection: IMatrixColumnsSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_MATRIX_COLUMNS, selection);
    }
}
