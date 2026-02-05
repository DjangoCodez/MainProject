import { ReportUserSelectionDTO } from "../../../../../Common/Models/ReportDTOs";
import { SelectionCollection } from "../../SelectionCollection";
import { EmployeeSelectionDTO, IdListSelectionDTO } from "../../../../../Common/Models/ReportDataSelectionDTO";
import { IEmployeeSelectionDTO, IIdListSelectionDTO } from "../../../../../Scripts/TypeLite.Net4";
import { Constants } from "../../../../../Util/Constants";

export class ForaReport {
    public static component(): ng.IComponentOptions {
        const options: ng.IComponentOptions = {
            controller: ForaReport,
            templateUrl: soeConfig.baseUrl + "Core/RightMenu/ReportMenu/ReportTypes/ForaReport/ForaReportView.html",
            bindings: {
                userSelection: "=",
                selections: "<"
            }
        };

        return options;
    }
    public static componentKey = "foraReport";

    //binding fields
    private userSelection: ReportUserSelectionDTO;
    private selections: SelectionCollection;
    private timePeriodIds: number[];

    //user selections
    private userSelectionInputPayrollMonthYear: IdListSelectionDTO;
    private userSelectionInputEmployee: EmployeeSelectionDTO;

    //@ngInject
    constructor(
        private $scope: ng.IScope) {

        this.$scope.$watch(() => this.userSelection, (newVal, oldVal) => {
            if (!newVal)
                return;

            this.userSelectionInputPayrollMonthYear = this.userSelection.getIdListSelection(Constants.REPORTMENU_SELECTION_KEY_PERIODS);
            this.userSelectionInputEmployee = this.userSelection.getEmployeeSelection();

            this.timePeriodIds = this.userSelectionInputPayrollMonthYear ? this.userSelectionInputPayrollMonthYear.ids : null;
        });
    }

    public $onInit() {
    }

    public onPayrollMonthYearSelectionUpdated(selection: IIdListSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_PERIODS, selection);
        this.timePeriodIds = selection.ids;
        console.log('selection', selection);
    }

    public onEmployeeSelectionUpdated(selection: IEmployeeSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_EMPLOYEES, selection);
    }
}
