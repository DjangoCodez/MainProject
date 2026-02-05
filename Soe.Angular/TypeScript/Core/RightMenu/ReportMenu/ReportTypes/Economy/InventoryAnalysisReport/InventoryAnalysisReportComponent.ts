import { ReportUserSelectionDTO } from "../../../../../../Common/Models/ReportDTOs";
import { SelectionCollection } from "../../../SelectionCollection";
import { DateRangeSelectionDTO, DateSelectionDTO, MatrixColumnsSelectionDTO, BoolSelectionDTO } from "../../../../../../Common/Models/ReportDataSelectionDTO";
import { IDateRangeSelectionDTO, IMatrixColumnsSelectionDTO, IBoolSelectionDTO } from "../../../../../../Scripts/TypeLite.Net4";
import { Constants } from "../../../../../../Util/Constants";
import { SoeModule } from "../../../../../../Util/CommonEnumerations";
import { IReportDataService } from "../../../ReportDataService";

export class InventoryAnalysisReport {
    public static component(): ng.IComponentOptions {
        const options: ng.IComponentOptions = {
            controller: InventoryAnalysisReport,
            templateUrl: soeConfig.baseUrl + "Core/RightMenu/ReportMenu/ReportTypes/Economy/InventoryAnalysisReport/InventoryAnalysisReportView.html",
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
    public static componentKey = "inventoryAnalysisReport";

    //binding fields
    private module: SoeModule;
    private userSelection: ReportUserSelectionDTO;
    private selections: SelectionCollection;
    private sysReportTemplateTypeId: number;
    private reportId: number;
    private isAnalysis: boolean;
    private fromDate: Date;
    private toDate: Date;

    //user selections
    private userSelectionInputColumns: MatrixColumnsSelectionDTO;
    private userSelectionInputIncludeInactive: BoolSelectionDTO;
    private userSelectedDateRange: DateRangeSelectionDTO;

    //@ngInject
    constructor(
        private $scope: ng.IScope, private reportDataService: IReportDataService
    ) {
        this.$scope.$watch(() => this.userSelection, (newVal, oldVal) => {
            if (!newVal)
                return;

            this.userSelectionInputIncludeInactive = this.userSelection.getBoolSelection(Constants.REPORTMENU_SELECTION_KEY_INCLUDE_INACTIVE);
            this.userSelectionInputColumns = this.userSelection.getMatrixColumnSelection();
            this.userSelectedDateRange = this.userSelection.getDateRangeSelection();
            this.fromDate = this.userSelectedDateRange ? this.userSelectedDateRange.from : null;
            this.toDate = this.userSelectedDateRange ? this.userSelectedDateRange.to : null;
        });
    }

    public $onInit() {
        this.getCurrentAccountYear();
    }

    public onBoolSelectionIncludeInactiveUpdated(selection: IBoolSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_INCLUDE_INACTIVE, selection);
    }
    public onMatrixColumnSelectionUpdated(selection: IMatrixColumnsSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_MATRIX_COLUMNS, selection);
    }

    public onDateRangeSelected(dateRange: DateRangeSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_DATE_RANGE, dateRange);
        this.fromDate = dateRange.from;
        this.toDate = dateRange.to;
    }

    private getCurrentAccountYear() {        
        this.reportDataService.getCurrentAccountYear().then(accountYear => {
            this.userSelectedDateRange = new DateRangeSelectionDTO(Constants.REPORTMENU_DATERANGESELECTION_TYPE_DATERANGE, accountYear.from, accountYear.to);
        });
    }

}
