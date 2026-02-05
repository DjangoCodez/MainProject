import { ReportMenuDTO, ReportUserSelectionDTO } from "../../../../../Common/Models/ReportDTOs";
import { SelectionCollection } from "../../SelectionCollection";
import { SelectionDTO, DateSelectionDTO, DateRangeSelectionDTO, EmployeeSelectionDTO, BoolSelectionDTO } from "../../../../../Common/Models/ReportDataSelectionDTO";
import { IDateSelectionDTO, IDateRangeSelectionDTO, IEmployeeSelectionDTO, IBoolSelectionDTO } from "../../../../../Scripts/TypeLite.Net4";
import { Constants } from "../../../../../Util/Constants";

export class TimeMonthlyReport {
    public static component(): ng.IComponentOptions {
        const options: ng.IComponentOptions = {
            controller: TimeMonthlyReport,
            templateUrl: soeConfig.baseUrl + "Core/RightMenu/ReportMenu/ReportTypes/TimeMonthlyReport/TimeMonthlyReportView.html",
            bindings: {
                userSelection: "=",
                selections: "<",
                fromDate: "<",
                toDate: "<"
            }
        };

        return options;
    }
    public static componentKey = "timeMonthlyReport";

    //binding fields
    private userSelection: ReportUserSelectionDTO;
    private selections: SelectionCollection;
    private fromDate: Date;
    private toDate: Date;

    //user selections
    private userSelectionInputDate: DateRangeSelectionDTO;
    private userSelectionInputEmployee: EmployeeSelectionDTO;
    private userSelectionInputFilterOnAccounting: BoolSelectionDTO;
    private userSelectionInputPreliminary: BoolSelectionDTO;

    //@ngInject
    constructor(
        private $scope: ng.IScope) {
        this.fromDate = new Date();
        this.toDate = new Date();

        this.$scope.$watch(() => this.userSelection, (newVal, oldVal) => {
            if (!newVal)
                return;

            this.userSelectionInputDate = this.userSelection.getDateRangeSelection();
            this.userSelectionInputEmployee = this.userSelection.getEmployeeSelection();
            this.userSelectionInputFilterOnAccounting = this.userSelection.getBoolSelection(Constants.REPORTMENU_SELECTION_KEY_FILTER_ON_ACCOUNTING);
            this.userSelectionInputPreliminary = this.userSelection.getBoolSelection(Constants.REPORTMENU_SELECTION_KEY_INCLUDE_PRELIMINARY);

            this.fromDate = this.userSelectionInputDate ? this.userSelectionInputDate.from : null;
            this.toDate = this.userSelectionInputDate ? this.userSelectionInputDate.to : null;
        });
    }

    public $onInit() {
        
    }

    public onDateRangeSelectionUpdated(selection: IDateRangeSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_DATE_RANGE, selection);

        this.fromDate = selection.from;
        this.toDate = selection.to;
    }

    public onEmployeeSelectionUpdated(selection: IEmployeeSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_EMPLOYEES, selection);
    }

    public onBoolSelectionFilterOnAccountingUpdated(selection: IBoolSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_FILTER_ON_ACCOUNTING, selection);
    }

    public onBoolSelectionPreliminaryUpdated(selection: IBoolSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_INCLUDE_PRELIMINARY, selection);
    }
}
