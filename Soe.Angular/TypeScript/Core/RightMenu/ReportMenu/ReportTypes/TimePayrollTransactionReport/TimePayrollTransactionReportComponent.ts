import { ReportUserSelectionDTO } from "../../../../../Common/Models/ReportDTOs";
import { SelectionCollection } from "../../SelectionCollection";
import { DateRangeSelectionDTO, EmployeeSelectionDTO, PayrollProductRowSelectionDTO, BoolSelectionDTO, IdListSelectionDTO, MatrixColumnsSelectionDTO } from "../../../../../Common/Models/ReportDataSelectionDTO";
import { IDateRangeSelectionDTO, IEmployeeSelectionDTO, IPayrollProductRowSelectionDTO, IBoolSelectionDTO, IIdListSelectionDTO, IMatrixColumnsSelectionDTO } from "../../../../../Scripts/TypeLite.Net4";
import { Constants } from "../../../../../Util/Constants";
import { SmallGenericType } from "../../../../../Common/Models/SmallGenericType";
import { ICoreService } from "../../../../Services/CoreService";
import { SoeModule, TermGroup_AttestEntity } from "../../../../../Util/CommonEnumerations";
import { AttestStateDTO } from "../../../../../Common/Models/AttestStateDTO";

export class TimePayrollTransactionReport {
    public static component(): ng.IComponentOptions {
        const options: ng.IComponentOptions = {
            controller: TimePayrollTransactionReport,
            templateUrl: soeConfig.baseUrl + "Core/RightMenu/ReportMenu/ReportTypes/TimePayrollTransactionReport/TimePayrollTransactionReportView.html",
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
    public static componentKey = "timePayrollTransactionReport";

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
    private userSelectionInputFilterOnAccounting: BoolSelectionDTO;
    private userSelectionInputPayrollProduct: PayrollProductRowSelectionDTO[];
    private userSelectionInputPreliminary: BoolSelectionDTO;
    private userSelectionInputShowOnlyTotal: BoolSelectionDTO;
    private userSelectionInputSkipTimeScheduleTransactions: BoolSelectionDTO;
    private userSelectionInputAttestState: IdListSelectionDTO;
    private userSelectionInputColumns: MatrixColumnsSelectionDTO;

    //Data
    private attestStates: SmallGenericType[] = [];

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
            this.userSelectionInputFilterOnAccounting = this.userSelection.getBoolSelection(Constants.REPORTMENU_SELECTION_KEY_FILTER_ON_ACCOUNTING);
            this.userSelectionInputPayrollProduct = this.userSelection.getPayrollProductRowSelections();
            this.userSelectionInputAttestState = this.userSelection.getIdListSelection(Constants.REPORTMENU_SELECTION_KEY_ATTESTSTATES);
            this.userSelectionInputPreliminary = this.userSelection.getBoolSelection(Constants.REPORTMENU_SELECTION_KEY_INCLUDE_PRELIMINARY);
            this.userSelectionInputShowOnlyTotal = this.userSelection.getBoolSelection(Constants.REPORTMENU_SELECTION_KEY_SHOW_ONLY_TOTALS);
            this.userSelectionInputColumns = this.userSelection.getMatrixColumnSelection();
        });
    }

    public $onInit() {
        this.loadAttestStates();
    }

    private loadAttestStates() {
        this.coreService.getAttestStates(TermGroup_AttestEntity.PayrollTime, SoeModule.Time, false).then(x => {
            _.forEach(x, (atteststate: AttestStateDTO) => {
                this.attestStates.push(new SmallGenericType(atteststate.attestStateId, atteststate.name));
            })
        });
    }

    public onDateTimeIntervalSelectionUpdated(selection: IDateRangeSelectionDTO) {
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

    public onPayrollProductSelectionUpdated(selection: IPayrollProductRowSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_PAYROLL_PRODUCTS, selection);
    }

    public onAttestStatusSelectionUpdated(selection: IIdListSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_ATTESTSTATES, selection);
    }

    public onBoolSelectionShowOnlyTotalUpdated(selection: IBoolSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_SHOW_ONLY_TOTALS, selection);
    }

    public onBoolSelectionSkipTimeScheduleTransactions(selection: IBoolSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_SKIPTIMESCHEDULETRANSACTIONS, selection);
    }

    public onMatrixColumnSelectionUpdated(selection: IMatrixColumnsSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_MATRIX_COLUMNS, selection);
    }


}
