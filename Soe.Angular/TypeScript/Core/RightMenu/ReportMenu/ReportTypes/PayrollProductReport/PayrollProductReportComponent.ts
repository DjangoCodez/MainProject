import { IPayrollProductRowSelectionDTO } from "../../../../../Scripts/TypeLite.Net4";
import { SelectionCollection } from "../../SelectionCollection";
import { EmployeeSelectionDTO, PayrollProductRowSelectionDTO } from "../../../../../Common/Models/ReportDataSelectionDTO";
import { ReportUserSelectionDTO } from "../../../../../Common/Models/ReportDTOs";
import { Constants } from "../../../../../Util/Constants";

export class PayrollProductReport {
    public static component(): ng.IComponentOptions {
        const options: ng.IComponentOptions = {
            controller: PayrollProductReport,
            templateUrl: soeConfig.baseUrl + "Core/RightMenu/ReportMenu/ReportTypes/PayrollProductReport/PayrollProductReportView.html",
            bindings: {
                userSelection: "=",
                selections: "<"
            }
        };

        return options;
    }
    public static componentKey = "payrollProductReport";

    //binding fields
    private userSelection: ReportUserSelectionDTO;
    private selections: SelectionCollection;

    private userSelectionInputPayrollProduct: PayrollProductRowSelectionDTO[];

    //@ngInject
    constructor(
        private $scope: ng.IScope) {

        this.$scope.$watch(() => this.userSelection, (newVal, oldVal) => {
            if (!newVal)
                return;

            this.userSelectionInputPayrollProduct = this.userSelection.getPayrollProductRowSelections();
        });
    }

    public $onInit() {
    }

    public onPayrollProductSelectionUpdated(selection: IPayrollProductRowSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_PAYROLL_PRODUCTS, selection);
    }
}
