import { ReportUserSelectionDTO } from "../../../../../Common/Models/ReportDTOs";
import { SelectionCollection } from "../../SelectionCollection";
import { EmployeeSelectionDTO, DateSelectionDTO } from "../../../../../Common/Models/ReportDataSelectionDTO";
import { IDateSelectionDTO, IEmployeeSelectionDTO } from "../../../../../Scripts/TypeLite.Net4";
import { Constants } from "../../../../../Util/Constants";

export class SCBKSPReport {
    public static component(): ng.IComponentOptions {
        const options: ng.IComponentOptions = {
            controller: SCBKSPReport,
            templateUrl: soeConfig.baseUrl + "Core/RightMenu/ReportMenu/ReportTypes/SCBKSPReport/SCBKSPReportView.html",
            bindings: {
                userSelection: "=",
                selections: "<"
            }
        };

        return options;
    }
    public static componentKey = "sCBKSPReport";

    //binding fields
    private userSelection: ReportUserSelectionDTO;
    private selections: SelectionCollection;
    private date: Date;

    //user selections
    private userSelectionInputDate: DateSelectionDTO;
    private userSelectionInputEmployee: EmployeeSelectionDTO;

    //@ngInject
    constructor(
        private $scope: ng.IScope) {
        this.date = new Date();

        this.$scope.$watch(() => this.userSelection, (newVal, oldVal) => {
            if (!newVal)
                return;

            this.userSelectionInputDate = this.userSelection.getDateSelection();
            this.userSelectionInputEmployee = this.userSelection.getEmployeeSelection();

            this.date = this.userSelectionInputDate ? this.userSelectionInputDate.date : null;
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
}
