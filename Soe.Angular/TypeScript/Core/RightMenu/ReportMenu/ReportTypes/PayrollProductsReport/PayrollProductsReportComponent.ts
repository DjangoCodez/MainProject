import { ReportUserSelectionDTO } from "../../../../../Common/Models/ReportDTOs";
import { SelectionCollection } from "../../SelectionCollection";
import { MatrixColumnsSelectionDTO, PayrollProductRowSelectionDTO} from "../../../../../Common/Models/ReportDataSelectionDTO";
import { IMatrixColumnsSelectionDTO, IPayrollProductRowSelectionDTO  } from "../../../../../Scripts/TypeLite.Net4";
import { Constants } from "../../../../../Util/Constants";
import { SoeModule } from "../../../../../Util/CommonEnumerations";

export class PayrollProductsReport {
    public static component(): ng.IComponentOptions {
        const options: ng.IComponentOptions = {
            controller: PayrollProductsReport,
            templateUrl: soeConfig.baseUrl + "Core/RightMenu/ReportMenu/ReportTypes/PayrollProductsReport/PayrollProductsReportView.html",
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
    public static componentKey = "payrollProductsReport";

    //binding fields
    private module: SoeModule;
    private userSelection: ReportUserSelectionDTO;
    private selections: SelectionCollection;
    private sysReportTemplateTypeId: number;
    private reportId: number;
    private isAnalysis: boolean;

    //user selections
    private userSelectionInputColumns: MatrixColumnsSelectionDTO;
    private userSelectionInputPayrollProduct: PayrollProductRowSelectionDTO[];

    //@ngInject
    constructor(
        private $scope: ng.IScope) {

        this.$scope.$watch(() => this.userSelection, (newVal, oldVal) => {
            if (!newVal)
                return;

            this.userSelectionInputColumns = this.userSelection.getMatrixColumnSelection();
            this.userSelectionInputPayrollProduct = this.userSelection.getPayrollProductRowSelections();
        });
    }

    public $onInit() {

    }

    public onMatrixColumnSelectionUpdated(selection: IMatrixColumnsSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_MATRIX_COLUMNS, selection);
    }
    public onPayrollProductSelectionUpdated(selection: IPayrollProductRowSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_PAYROLL_PRODUCTS, selection);
    }
}
   