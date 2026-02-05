import { ReportUserSelectionDTO } from "../../../../../Common/Models/ReportDTOs";
import { SelectionCollection } from "../../SelectionCollection";
import { EmployeeSelectionDTO, PayrollProductRowSelectionDTO, IdListSelectionDTO, BoolSelectionDTO, MatrixColumnsSelectionDTO } from "../../../../../Common/Models/ReportDataSelectionDTO";
import { IEmployeeSelectionDTO, IPayrollProductRowSelectionDTO, IBoolSelectionDTO, IIdListSelectionDTO, IMatrixColumnsSelectionDTO } from "../../../../../Scripts/TypeLite.Net4";
import { Constants } from "../../../../../Util/Constants";
import { SoeModule } from "../../../../../Util/CommonEnumerations";

export class PayrollTransactionStatisticsReport {
    public static component(): ng.IComponentOptions {
        const options: ng.IComponentOptions = {
            controller: PayrollTransactionStatisticsReport,
            templateUrl: soeConfig.baseUrl + "Core/RightMenu/ReportMenu/ReportTypes/PayrollTransactionStatisticsReport/PayrollTransactionStatisticsReportView.html",
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
    public static componentKey = "payrollTransactionStatisticsReport";

    //binding fields
    private module: SoeModule;
    private userSelection: ReportUserSelectionDTO;
    private selections: SelectionCollection;
    private timePeriodIds: number[];
    private sysReportTemplateTypeId: number;
    private reportId: number;
    private isAnalysis: boolean;

    //user selections
    private userSelectionInputPayrollPeriod: IdListSelectionDTO;
    private userSelectionInputEmployee: EmployeeSelectionDTO;
    private userSelectionInputFilterOnAccounting: BoolSelectionDTO;
    private userSelectionInputPayrollProduct: PayrollProductRowSelectionDTO[];
    private userSelectionInputIncludeStartValues: BoolSelectionDTO;
    private userSelectionInputIgnoreAccounting: BoolSelectionDTO; 
    private userSelectionInputShowOnlyTotal: BoolSelectionDTO;
    private userSelectionInputColumns: MatrixColumnsSelectionDTO;

    //@ngInject
    constructor(
        private $scope: ng.IScope) {

        this.$scope.$watch(() => this.userSelection, (newVal, oldVal) => {
            if (!newVal)
                return;

            this.userSelectionInputPayrollPeriod = this.userSelection.getIdListSelection(Constants.REPORTMENU_SELECTION_KEY_PERIODS);
            this.userSelectionInputEmployee = this.userSelection.getEmployeeSelection();
            this.userSelectionInputFilterOnAccounting = this.userSelection.getBoolSelection(Constants.REPORTMENU_SELECTION_KEY_FILTER_ON_ACCOUNTING);
            this.userSelectionInputPayrollProduct = this.userSelection.getPayrollProductRowSelections();
            this.userSelectionInputIncludeStartValues = this.userSelection.getBoolSelection(Constants.REPORTMENU_SELECTION_KEY_INCLUDE_START_VALUES);
            this.userSelectionInputIgnoreAccounting = this.userSelection.getBoolSelection(Constants.REPORTMENU_SELECTION_KEY_IGNORE_ACCOUNTING);
            this.userSelectionInputShowOnlyTotal = this.userSelection.getBoolSelection(Constants.REPORTMENU_SELECTION_KEY_SHOW_ONLY_TOTALS);
            this.userSelectionInputColumns = this.userSelection.getMatrixColumnSelection();

            this.timePeriodIds = this.userSelectionInputPayrollPeriod ? this.userSelectionInputPayrollPeriod.ids : null;
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

    public onBoolSelectionFilterOnAccountingUpdated(selection: IBoolSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_FILTER_ON_ACCOUNTING, selection);
    }

    public onPayrollProductSelectionUpdated(selection: IPayrollProductRowSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_PAYROLL_PRODUCTS, selection);
    }

    public onBoolSelectionIncludeStartValuesUpdated(selection: IBoolSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_INCLUDE_START_VALUES, selection);
    }

    public onBoolSelectionShowOnlyTotalUpdated(selection: IBoolSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_SHOW_ONLY_TOTALS, selection);
    }

    public onBoolSelectionIgnoreAccountingUpdated(selection: IBoolSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_IGNORE_ACCOUNTING, selection);
    }

    public onMatrixColumnSelectionUpdated(selection: IMatrixColumnsSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_MATRIX_COLUMNS, selection);
    }
}
