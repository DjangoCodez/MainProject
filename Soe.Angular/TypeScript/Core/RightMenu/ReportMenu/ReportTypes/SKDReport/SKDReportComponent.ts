import { ReportUserSelectionDTO } from "../../../../../Common/Models/ReportDTOs";
import { SelectionCollection } from "../../SelectionCollection";
import { EmployeeSelectionDTO, IdListSelectionDTO, BoolSelectionDTO } from "../../../../../Common/Models/ReportDataSelectionDTO";
import { IEmployeeSelectionDTO, IBoolSelectionDTO, IIdListSelectionDTO } from "../../../../../Scripts/TypeLite.Net4";
import { Constants } from "../../../../../Util/Constants";

export class SKDReport {
    public static component(): ng.IComponentOptions {
        const options: ng.IComponentOptions = {
            controller: SKDReport,
            templateUrl: soeConfig.baseUrl + "Core/RightMenu/ReportMenu/ReportTypes/SKDReport/SKDReportView.html",
            bindings: {
                userSelection: "=",
                selections: "<"
            }
        };

        return options;
    }
    public static componentKey = "sKDReport";

    //binding fields
    private userSelection: ReportUserSelectionDTO;
    private selections: SelectionCollection;
    private timePeriodIds: number[];

    //user selections
    private userSelectionInputPayrollMonthYear: IdListSelectionDTO;
    private userSelectionInputEmployee: EmployeeSelectionDTO;
    private userSelectionInputRemovePrevSubmittedData: BoolSelectionDTO;

    //@ngInject
    constructor(
        private $scope: ng.IScope) {

        this.$scope.$watch(() => this.userSelection, (newVal, oldVal) => {
            if (!newVal)
                return;

            this.userSelectionInputPayrollMonthYear = this.userSelection.getIdListSelection(Constants.REPORTMENU_SELECTION_KEY_PERIODS);
            this.userSelectionInputEmployee = this.userSelection.getEmployeeSelection();
            this.userSelectionInputRemovePrevSubmittedData = this.userSelection.getBoolSelection(Constants.REPORTMENU_SELECTION_KEY_REMOVE_PREV_SUBMITTED_DATA);

            this.timePeriodIds = this.userSelectionInputPayrollMonthYear ? this.userSelectionInputPayrollMonthYear.ids : null;
        });
    }

    public $onInit() {
    }

    public onPayrollMonthYearSelectionUpdated(selection: IIdListSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_PERIODS, selection);
        this.timePeriodIds = selection.ids;
    }

    public onEmployeeSelectionUpdated(selection: IEmployeeSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_EMPLOYEES, selection);
    }

    public onBoolSelectionRemovePrevSubmittedDataUpdated(selection: IBoolSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_REMOVE_PREV_SUBMITTED_DATA, selection);
    }
}
