import { ReportUserSelectionDTO } from "../../../../../Common/Models/ReportDTOs";
import { SelectionCollection } from "../../SelectionCollection";
import { BoolSelectionDTO, DateSelectionDTO, IdListSelectionDTO, MatrixColumnsSelectionDTO } from "../../../../../Common/Models/ReportDataSelectionDTO";
import { IBoolSelectionDTO, IDateSelectionDTO, IMatrixColumnsSelectionDTO } from "../../../../../Scripts/TypeLite.Net4";
import { Constants } from "../../../../../Util/Constants";
import { SoeModule } from "../../../../../Util/CommonEnumerations";

export class UserReport {
    public static component(): ng.IComponentOptions {
        const options: ng.IComponentOptions = {
            controller: UserReport,
            templateUrl: soeConfig.baseUrl + "Core/RightMenu/ReportMenu/ReportTypes/UserReport/UserReportView.html",
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
    public static componentKey = "userReport";

    //binding fields
    private module: SoeModule;
    private userSelection: ReportUserSelectionDTO;
    private selections: SelectionCollection;
    private date: Date;
    private sysReportTemplateTypeId: number;
    private reportId: number;
    private isAnalysis: boolean;

    //user selections
    private userSelectionInputDate: DateSelectionDTO;
    private userSelectionInputUser: IdListSelectionDTO;
    private userSelectionInputUniqueRow: BoolSelectionDTO;
    private userSelectionInputColumns: MatrixColumnsSelectionDTO;

    //@ngInject
    constructor(
        private $scope: ng.IScope) {
        this.date = new Date();

        this.$scope.$watch(() => this.userSelection, (newVal, oldVal) => {
            if (!newVal)
                return;

            this.userSelectionInputDate = this.userSelection.getDateSelection();
            this.userSelectionInputUser = this.userSelection.getIdListSelection(Constants.REPORTMENU_SELECTION_KEY_USERS);
            this.userSelectionInputUniqueRow = this.userSelection.getBoolSelection(Constants.REPORTMENU_SELECTION_KEY_UNIQUE_ROW_USERROLEATTESTROLE);
            this.userSelectionInputColumns = this.userSelection.getMatrixColumnSelection();

            this.date = this.userSelectionInputDate ? this.userSelectionInputDate.date : new Date();
        });
    }

    public $onInit() {
    }

    public onDateSelectionUpdated(selection: IDateSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_DATE, selection);

        this.date = selection.date;
    }

    public onUserSelectionUpdated(selection: IdListSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_USERS, selection);
    }

    public onBoolSelectionUniqueRowUpdated(selection: IBoolSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_UNIQUE_ROW_USERROLEATTESTROLE, selection);
    }

    public onMatrixColumnSelectionUpdated(selection: IMatrixColumnsSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_MATRIX_COLUMNS, selection);
    }
}
