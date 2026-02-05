import { ReportUserSelectionDTO } from "../../../../../Common/Models/ReportDTOs";
import { SelectionCollection } from "../../SelectionCollection";
import { EmployeeSelectionDTO, IdListSelectionDTO, BoolSelectionDTO } from "../../../../../Common/Models/ReportDataSelectionDTO";
import { IEmployeeSelectionDTO, IIdListSelectionDTO, IBoolSelectionDTO } from "../../../../../Scripts/TypeLite.Net4";
import { Constants } from "../../../../../Util/Constants";

export class BygglosenReport {
    public static component(): ng.IComponentOptions {
        const options: ng.IComponentOptions = {
            controller: BygglosenReport,
            templateUrl: soeConfig.baseUrl + "Core/RightMenu/ReportMenu/ReportTypes/BygglosenReport/BygglosenReportView.html",
            bindings: {
                userSelection: "=",
                selections: "<"
            }
        };

        return options;
    }
    public static componentKey = "bygglosenReport";

    //binding fields
    private userSelection: ReportUserSelectionDTO;
    private selections: SelectionCollection;
    private timePeriodIds: number[];

    //user selections
    private userSelectionInputPayrollPeriod: IdListSelectionDTO;
    private userSelectionInputEmployee: EmployeeSelectionDTO;
    private userSelectionInputSetAsFinal: BoolSelectionDTO;
    private userSelectionInputRemovePrevSubmittedData: BoolSelectionDTO;

    //@ngInject
    constructor(
        private $scope: ng.IScope) {

        this.$scope.$watch(() => this.userSelection, (newVal, oldVal) => {
            if (!newVal)
                return;

            this.userSelectionInputPayrollPeriod = this.userSelection.getIdListSelection(Constants.REPORTMENU_SELECTION_KEY_PERIODS);
            this.userSelectionInputEmployee = this.userSelection.getEmployeeSelection();
            this.userSelectionInputSetAsFinal =  this.userSelection.getBoolSelection(Constants.REPORTMENU_SELECTION_KEY_TIME_SETASFINAL);
            this.timePeriodIds = this.userSelectionInputPayrollPeriod ? this.userSelectionInputPayrollPeriod.ids : null;
            this.userSelectionInputRemovePrevSubmittedData = this.userSelection.getBoolSelection(Constants.REPORTMENU_SELECTION_KEY_REMOVE_PREV_SUBMITTED_DATA);
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

    public onBoolSelectionInputSetAsFinal(selection: IBoolSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_TIME_SETASFINAL, selection);
    }

    public onBoolSelectionRemovePrevSubmittedDataUpdated(selection: IBoolSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_REMOVE_PREV_SUBMITTED_DATA, selection);
    }
}
