import { ReportUserSelectionDTO } from "../../../../../Common/Models/ReportDTOs";
import { SelectionCollection } from "../../SelectionCollection";
import { EmployeeSelectionDTO, IdListSelectionDTO } from "../../../../../Common/Models/ReportDataSelectionDTO";
import { IEmployeeSelectionDTO, IIdListSelectionDTO } from "../../../../../Scripts/TypeLite.Net4";
import { Constants } from "../../../../../Util/Constants";
import { ICoreService } from "../../../../Services/CoreService";

export class IFMetallReport {
    public static component(): ng.IComponentOptions {
        const options: ng.IComponentOptions = {
            controller: IFMetallReport,
            templateUrl: soeConfig.baseUrl + "Core/RightMenu/ReportMenu/ReportTypes/IFMetallReport/IFMetallReportView.html",
            bindings: {
                userSelection: "=",
                selections: "<"
            }
        };

        return options;
    }
    public static componentKey = "iFMetallReport";

    //binding fields
    private userSelection: ReportUserSelectionDTO;
    private selections: SelectionCollection;
    private timePeriodIds: number[];
   
    //user selections
    private userSelectionInputPayrollMonthYear: IdListSelectionDTO;
    private userSelectionInputEmployee: EmployeeSelectionDTO;

    //@ngInject
    constructor(
        private coreService: ICoreService,
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

    public onPeriodSelectionUpdated(selection: IIdListSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_PERIODS, selection);
        this.timePeriodIds = selection.ids;
    }

    public onEmployeeSelectionUpdated(selection: IEmployeeSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_EMPLOYEES, selection);
    }

}
