import { ReportUserSelectionDTO } from "../../../../../Common/Models/ReportDTOs";
import { SelectionCollection } from "../../SelectionCollection";
import { DateRangeSelectionDTO, EmployeeSelectionDTO, MatrixColumnsSelectionDTO } from "../../../../../Common/Models/ReportDataSelectionDTO";
import { IDateRangeSelectionDTO, IEmployeeSelectionDTO, IMatrixColumnsSelectionDTO } from "../../../../../Scripts/TypeLite.Net4";
import { Constants } from "../../../../../Util/Constants";
import { SoeModule } from "../../../../../Util/CommonEnumerations";
import { ICoreService } from "../../../../Services/CoreService";

export class EmploymentDaysReport {
    public static component(): ng.IComponentOptions {
        const options: ng.IComponentOptions = {
            controller: EmploymentDaysReport,
            templateUrl: soeConfig.baseUrl + "Core/RightMenu/ReportMenu/ReportTypes/EmploymentDaysReport/EmploymentDaysReportView.html",
            bindings: {
                module: "<",
                userSelection: "=",
                selections: "<",
                fromDate: "<",
                toDate: "<",
                sysReportTemplateTypeId: "<",
                reportId: "<",
                isAnalysis: "<"
            }
        };

        return options;
    }
    public static componentKey = "employmentDaysReport";

    //binding fields
    private module: SoeModule;
    private userSelection: ReportUserSelectionDTO;
    private selections: SelectionCollection;
    private fromDate: Date;
    private toDate: Date;
    private sysReportTemplateTypeId: number;
    private reportId: number;
    private isAnalysis: boolean;

    //user selections
    private userSelectionInputDate: DateRangeSelectionDTO;
    private userSelectionInputEmployee: EmployeeSelectionDTO;
    //private userSelectionInputPayrollPriceTypes: IdListSelectionDTO;
    private userSelectionInputColumns: MatrixColumnsSelectionDTO;

    //Data
   // private payrollPriceTypes: SmallGenericType[] = [];

    //@ngInject
    constructor(
        private coreService: ICoreService,
        private $scope: ng.IScope) {
        this.fromDate = new Date();
        this.toDate = new Date();

        this.$scope.$watch(() => this.userSelection, (newVal, oldVal) => {
            if (!newVal)
                return;

            this.userSelectionInputDate = this.userSelection.getDateRangeSelection();
            this.userSelectionInputEmployee = this.userSelection.getEmployeeSelection();
            //this.userSelectionInputPayrollPriceTypes = this.userSelection.getIdListSelection(Constants.REPORTMENU_SELECTION_KEY_PAYROLL_PRICE_TYPES);
            this.userSelectionInputColumns = this.userSelection.getMatrixColumnSelection();

            this.fromDate = this.userSelectionInputDate ? this.userSelectionInputDate.from : null;
            this.toDate = this.userSelectionInputDate ? this.userSelectionInputDate.to : null;
        });
    }

    public $onInit() {
        this.fromDate = new Date();
        this.toDate = new Date();
        //this.loadPayrollPriceTypes();
    }

    /*private loadPayrollPriceTypes() {
        
        return this.coreService.getTermGroupContent(TermGroup.PayrollPriceTypes, false, false).then((x) => {
            this.payrollPriceTypes = [];
            _.forEach(x, (row) => {
                //this.payrollPriceTypes.push({ value: row.id, label: row.name });
                this.payrollPriceTypes.push(new SmallGenericType(row.id, row.name));
            });
        });
    }*/

    public onDateRangeSelectionUpdated(selection: IDateRangeSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_DATE_RANGE, selection);

        this.fromDate = selection.from;
        this.toDate = selection.to;
    }

    public onEmployeeSelectionUpdated(selection: IEmployeeSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_EMPLOYEES, selection);
    }

    /*public onPayrollPriceTypeSelectionUpdated(selection: IIdListSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_PAYROLL_PRICE_TYPES, selection);
    }*/

    public onMatrixColumnSelectionUpdated(selection: IMatrixColumnsSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_MATRIX_COLUMNS, selection);
    }
}
