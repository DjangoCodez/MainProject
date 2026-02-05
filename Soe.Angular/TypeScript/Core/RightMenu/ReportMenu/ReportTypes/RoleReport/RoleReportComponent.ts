import { ReportUserSelectionDTO } from "../../../../../Common/Models/ReportDTOs";
import { SelectionCollection } from "../../SelectionCollection";
import { DateSelectionDTO, EmployeeSelectionDTO } from "../../../../../Common/Models/ReportDataSelectionDTO";
import { IDateSelectionDTO, IEmployeeSelectionDTO } from "../../../../../Scripts/TypeLite.Net4";
import { Constants } from "../../../../../Util/Constants";

export class RoleReport {
    public static component(): ng.IComponentOptions {
        const options: ng.IComponentOptions = {
            controller: RoleReport,
            templateUrl: soeConfig.baseUrl + "Core/RightMenu/ReportMenu/ReportTypes/RoleReport/RoleReportView.html",
            bindings: {
                userSelection: "=",
                selections: "<"
            }
        };

        return options;
    }
    public static componentKey = "roleReport";

    //binding fields
    private userSelection: ReportUserSelectionDTO;
    private selections: SelectionCollection;
    private date: Date;

    //user selections
    private userSelectionInputDate: DateSelectionDTO;

    //@ngInject
    constructor(
        private $scope: ng.IScope) {
        this.date = new Date();

        this.$scope.$watch(() => this.userSelection, (newVal, oldVal) => {
            if (!newVal)
                return;

            this.userSelectionInputDate = this.userSelection.getDateSelection();

            this.date = this.userSelectionInputDate ? this.userSelectionInputDate.date : new Date();
        });
    }

    public $onInit() {

    }

    public onDateSelectionUpdated(selection: IDateSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_DATE, selection);

        this.date = selection.date;
    }
}
