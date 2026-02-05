import { ReportUserSelectionDTO } from "../../../../../../Common/Models/ReportDTOs";
import { SelectionCollection } from "../../../SelectionCollection";
import { BoolSelectionDTO, MatrixColumnsSelectionDTO } from "../../../../../../Common/Models/ReportDataSelectionDTO";
import { IBoolSelectionDTO, IMatrixColumnsSelectionDTO } from "../../../../../../Scripts/TypeLite.Net4";
import { Constants } from "../../../../../../Util/Constants";
import { SoeModule } from "../../../../../../Util/CommonEnumerations";

export class SupplierReport {
    public static component(): ng.IComponentOptions {
        const options: ng.IComponentOptions = {
            controller: SupplierReport,
            templateUrl: soeConfig.baseUrl + "Core/RightMenu/ReportMenu/ReportTypes/Economy/SupplierReport/SupplierReportView.html",
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
    public static componentKey = "supplierReport";

    //binding fields
    private module: SoeModule;
    private userSelection: ReportUserSelectionDTO;
    private selections: SelectionCollection;
    private sysReportTemplateTypeId: number;
    private reportId: number;
    private isAnalysis: boolean;

    //user selections
    private userSelectionInputColumns: MatrixColumnsSelectionDTO;
    private userSelectionInputIncludeInactive: BoolSelectionDTO;

    //@ngInject
    constructor(
        private $scope: ng.IScope) {
        this.$scope.$watch(() => this.userSelection, (newVal, oldVal) => {
            if (!newVal)
                return;

            this.userSelectionInputIncludeInactive = this.userSelection.getBoolSelection(Constants.REPORTMENU_SELECTION_KEY_INCLUDE_INACTIVE);
            this.userSelectionInputColumns = this.userSelection.getMatrixColumnSelection();
           
        });
    }

    public $onInit() {
    }
    public onBoolSelectionIncludeInactiveUpdated(selection: IBoolSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_INCLUDE_INACTIVE, selection);
    }
    public onMatrixColumnSelectionUpdated(selection: IMatrixColumnsSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_MATRIX_COLUMNS, selection);
    }
}
