import { ReportUserSelectionDTO } from "../../../../../../Common/Models/ReportDTOs";
import { SelectionCollection } from "../../../SelectionCollection";
import { MatrixColumnsSelectionDTO, DateSelectionDTO, IdSelectionDTO, IdListSelectionDTO } from "../../../../../../Common/Models/ReportDataSelectionDTO";
import { IMatrixColumnsSelectionDTO, ISmallGenericType } from "../../../../../../Scripts/TypeLite.Net4";
import { Constants } from "../../../../../../Util/Constants";
import { SoeModule } from "../../../../../../Util/CommonEnumerations";
import { IReportDataService } from "../../../ReportDataService";
import { CoreService } from '../../../../../Services/CoreService';

export class DepreciationAnalysisReport {
    public static component(): ng.IComponentOptions {
        const options: ng.IComponentOptions = {
            controller: DepreciationAnalysisReport,
            templateUrl: soeConfig.baseUrl + "Core/RightMenu/ReportMenu/ReportTypes/Economy/DepreciationAnalysisReport/DepreciationAnalysisReportView.html",
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
    public static componentKey = "depreciationAnalysisReport";

    //binding fields
    private module: SoeModule;
    private userSelection: ReportUserSelectionDTO;
    private selections: SelectionCollection;
    private sysReportTemplateTypeId: number;
    private reportId: number;
    private isAnalysis: boolean;

    //options
    private availablePeriods: ISmallGenericType[] = Array.from({ length: 15 }, (_, i) => ({ id: i + 1, name: (i + 1).toString() }));

    //user selections
    private selectionInputColumns: MatrixColumnsSelectionDTO;
    private selectedDate: DateSelectionDTO;
    private selectedPrognoseType: IdSelectionDTO;
    private selectedPeriods: IdSelectionDTO;
    private selectedStatuses: IdListSelectionDTO;
    private selectedCategories: IdListSelectionDTO;

    //@ngInject
    constructor(
        private $scope: ng.IScope,
        private reportDataService: IReportDataService,
        private coreService: CoreService
    ) {
        this.$scope.$watch(() => this.userSelection, (newVal, oldVal) => {
            if (!newVal)
                return;

            this.selectionInputColumns = newVal.getMatrixColumnSelection();
            this.selectedDate = newVal.getDateSelection();
            this.selectedPeriods = newVal.getIdSelection(Constants.REPORTMENU_SELECTION_KEY_PERIODS);
            setTimeout(() => {
                this.selectedPrognoseType = newVal.getIdSelection(Constants.REPORTMENU_SELECTION_KEY_PROGNOSE_TYPE);
            }, 100); // Gets written over if set ealier, double timeout if debugging backend.
            setTimeout(() => {
                this.selectedCategories = newVal.getIdListSelection(Constants.REPORTMENU_SELECTION_KEY_CATEGORIES);
                this.selectedStatuses = newVal.getIdListSelection(Constants.REPORTMENU_SELECTION_KEY_STATUSES);
			}, 500); // Gets written over if set ealier, double timeout if debugging backend.
        });
    }

    public onMatrixColumnSelectionUpdated(selection: IMatrixColumnsSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_MATRIX_COLUMNS, selection);
    }

    public onDateSelected(selection: DateSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_DATE, selection);
    }

    public onPeriodsSelected(selection: IdSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_PERIODS, selection);
    }

    public onPrognoseTypeSelected(selection: IdSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_PROGNOSE_TYPE, selection);
    }

    public onCategoriesSelected(selection: IdListSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_CATEGORIES, selection);
    }

    public onStatusSelected(selection: IdListSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_STATUSES, selection);
    }
}