import { ReportUserSelectionDTO } from "../../../../../Common/Models/ReportDTOs";
import { SelectionCollection } from "../../SelectionCollection";
import { DateSelectionDTO, EmployeeSelectionDTO, BoolSelectionDTO } from "../../../../../Common/Models/ReportDataSelectionDTO";
import { IDateSelectionDTO, IEmployeeSelectionDTO, IBoolSelectionDTO } from "../../../../../Scripts/TypeLite.Net4";
import { Constants } from "../../../../../Util/Constants";

export class EmployeeVacationDebtReport {
    public static component(): ng.IComponentOptions {
        const options: ng.IComponentOptions = {
            controller: EmployeeVacationDebtReport,
            templateUrl: soeConfig.baseUrl + "Core/RightMenu/ReportMenu/ReportTypes/EmployeeVacationDebtReport/EmployeeVacationDebtReportView.html",
            bindings: {
                userSelection: "=",
                selections: "<"
            }
        };

        return options;
    }
    public static componentKey = "employeeVacationDebtReport";

    //binding fields
    private userSelection: ReportUserSelectionDTO;
    private selections: SelectionCollection;
    private date: Date;

    //user selections
    private userSelectionInputDate: DateSelectionDTO;
    private userSelectionInputEmployee: EmployeeSelectionDTO;
    private userSelectionInputSetAsFinal: BoolSelectionDTO;
    private userSelectionInputIncludeFinalSalaryProcessed: BoolSelectionDTO;

    //@ngInject
    constructor(
        private $scope: ng.IScope) {
        this.date = new Date();

        this.$scope.$watch(() => this.userSelection, (newVal, oldVal) => {
            if (!newVal)
                return;

            this.userSelectionInputDate = this.userSelection.getDateSelection();
            this.userSelectionInputEmployee = this.userSelection.getEmployeeSelection();
            this.userSelectionInputSetAsFinal = this.userSelection.getBoolSelection(Constants.REPORTMENU_SELECTION_KEY_TIME_SETASFINAL);
            this.userSelectionInputIncludeFinalSalaryProcessed = this.userSelection.getBoolSelection(Constants.REPORTMENU_SELECTION_KEY_TIME_INCLUDE_FINALSALARYPROCESSED);
            this.date = this.userSelectionInputDate ? this.userSelectionInputDate.date : new Date();
        });
    }

    public $onInit() {

    }

    public onDateSelectionUpdated(selection: IDateSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_DATE, selection);

        this.date = selection.date;
    }

    public onEmployeeSelectionUpdated(selection: IEmployeeSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_EMPLOYEES, selection);
    }

    public onBoolSelectionInputSetAsFinal(selection: IBoolSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_TIME_SETASFINAL, selection);
    }

    public onBoolSelectionInputIncludeFinalSalaryProcessed(selection: IBoolSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_TIME_INCLUDE_FINALSALARYPROCESSED, selection);
    }
}