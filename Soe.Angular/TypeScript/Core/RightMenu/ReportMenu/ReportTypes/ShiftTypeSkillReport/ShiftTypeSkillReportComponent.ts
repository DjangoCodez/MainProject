import { ReportUserSelectionDTO } from "../../../../../Common/Models/ReportDTOs";
import { SelectionCollection } from "../../SelectionCollection";
import { IdListSelectionDTO, MatrixColumnsSelectionDTO } from "../../../../../Common/Models/ReportDataSelectionDTO";
import { IIdListSelectionDTO, IMatrixColumnsSelectionDTO } from "../../../../../Scripts/TypeLite.Net4";
import { Constants } from "../../../../../Util/Constants";
import { SoeModule } from "../../../../../Util/CommonEnumerations";


export class ShiftTypeSkillReport {
    public static component(): ng.IComponentOptions {
        const options: ng.IComponentOptions = {
            controller: ShiftTypeSkillReport,
            templateUrl: soeConfig.baseUrl + "Core/RightMenu/ReportMenu/ReportTypes/ShiftTypeSkillReport/ShiftTypeSkillReportView.html",
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
    public static componentKey = "shiftTypeSkillReport";

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
  
    private userSelectionInputColumns: MatrixColumnsSelectionDTO;
    private userSelectionInputShiftType: IdListSelectionDTO;
    //@ngInject
    constructor(
        private $scope: ng.IScope) {

        this.$scope.$watch(() => this.userSelection, (newVal, oldVal) => {
            if (!newVal)
                return;
            this.userSelectionInputShiftType = this.userSelection.getIdListSelection(Constants.REPORTMENU_SELECTION_KEY_SHIFT_TYPES);
            this.userSelectionInputColumns = this.userSelection.getMatrixColumnSelection();

        });
    }

    public $onInit() {
    
    }

   

    public onShiftTypeSelectionUpdated(selection: IIdListSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_SHIFT_TYPES, selection);
    }

    public onMatrixColumnSelectionUpdated(selection: IMatrixColumnsSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_MATRIX_COLUMNS, selection);
    }
}
