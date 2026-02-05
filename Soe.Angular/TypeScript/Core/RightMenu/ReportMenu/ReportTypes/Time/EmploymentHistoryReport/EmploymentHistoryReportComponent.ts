import { ReportUserSelectionDTO } from "../../../../../../Common/Models/ReportDTOs";
import { SelectionCollection } from "../../../SelectionCollection";
import { DateRangeSelectionDTO, EmployeeSelectionDTO, IdListSelectionDTO, MatrixColumnsSelectionDTO } from "../../../../../../Common/Models/ReportDataSelectionDTO";
import { IDateRangeSelectionDTO, IEmployeeSelectionDTO, IIdListSelectionDTO, IMatrixColumnsSelectionDTO } from "../../../../../../Scripts/TypeLite.Net4";
import { Constants } from "../../../../../../Util/Constants";
import { SoeModule, TermGroup } from "../../../../../../Util/CommonEnumerations";
import { SmallGenericType } from "../../../../../../Common/Models/SmallGenericType";
import { ICoreService } from "../../../../../Services/CoreService";

export class EmploymentHistoryReport {
    public static component(): ng.IComponentOptions {
        const options: ng.IComponentOptions = {
            controller: EmploymentHistoryReport,
            templateUrl: soeConfig.baseUrl + "Core/RightMenu/ReportMenu/ReportTypes/Time/EmploymentHistoryReport/EmploymentHistoryReportView.html",
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
    public static componentKey = "employmentHistoryReport";

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
    private userSelectionInputColumns: MatrixColumnsSelectionDTO;
    private userSelectionInputEmploymentTypes: IdListSelectionDTO;
    private employmentTypeIds: number[];
    //Data
    private employmentTypes: SmallGenericType[] = [];
    //@ngInject
    constructor(
        private $scope: ng.IScope,
        private coreService: ICoreService,
         ) {
        this.$scope.$watch(() => this.userSelection, (newVal, oldVal) => {
            if (!newVal)
                return;

            this.userSelectionInputDate = this.userSelection.getDateRangeSelection();
            this.userSelectionInputEmployee = this.userSelection.getEmployeeSelection();
            this.userSelectionInputColumns = this.userSelection.getMatrixColumnSelection();
            this.userSelectionInputEmploymentTypes = this.userSelection.getIdListSelection(Constants.REPORTMENU_SELECTION_KEY_EMPLOYMENT_TYPE);

            this.fromDate = this.userSelectionInputDate ? this.userSelectionInputDate.from : null;
            this.toDate = this.userSelectionInputDate ? this.userSelectionInputDate.to : null;
        });
    }

    public $onInit() {
        this.fromDate = new Date();
        this.toDate = new Date();
        this.loadEmploymentTypes();
    }

    public onDateRangeSelectionUpdated(selection: IDateRangeSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_DATE_RANGE, selection);

        this.fromDate = selection.from;
        this.toDate = selection.to;
    }

    public onEmployeeSelectionUpdated(selection: IEmployeeSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_EMPLOYEES, selection);
    }

    public onMatrixColumnSelectionUpdated(selection: IMatrixColumnsSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_MATRIX_COLUMNS, selection);
    }

    public onEmploymentTypeSelectionUpdated(selection: IIdListSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_EMPLOYMENT_TYPE, selection);
        this.employmentTypeIds = selection.ids;
    }

    public loadEmploymentTypes() {
        return this.coreService.getTermGroupContent(TermGroup.EmploymentType, false, false).then((x) => {
            this.employmentTypes = [];
            _.forEach(x, (row) => {
                this.employmentTypes.push(new SmallGenericType(row.id, row.name));
            });
        });
    }
}
