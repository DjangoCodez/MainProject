import { ReportMenuDTO, ReportUserSelectionDTO } from "../../../../../Common/Models/ReportDTOs";
import { SelectionCollection } from "../../SelectionCollection";
import { SelectionDTO, DateSelectionDTO, IdListSelectionDTO, DateRangeSelectionDTO, BoolSelectionDTO } from "../../../../../Common/Models/ReportDataSelectionDTO";
import { IDateSelectionDTO, IDateRangeSelectionDTO, IEmployeeSelectionDTO, IIdListSelectionDTO, IBoolSelectionDTO } from "../../../../../Scripts/TypeLite.Net4";
import { Constants } from "../../../../../Util/Constants";

export class TimeCategoryScheduleReport {
    public static component(): ng.IComponentOptions {
        const options: ng.IComponentOptions = {
            controller: TimeCategoryScheduleReport,
            templateUrl: soeConfig.baseUrl + "Core/RightMenu/ReportMenu/ReportTypes/TimeCategoryScheduleReport/TimeCategoryScheduleReportView.html",
            bindings: {
                userSelection: "=",
                selections: "<"
            }
        };

        return options;
    }
    public static componentKey = "timeCategoryScheduleReport";

    //binding fields
    private userSelection: ReportUserSelectionDTO;
    private selections: SelectionCollection;
    private fromDate: Date;
    private toDate: Date;

    //user selections
    private userSelectionInputDate: DateRangeSelectionDTO;
    private userSelectionInputCategory: IdListSelectionDTO;
    private userSelectionInputInactive: BoolSelectionDTO;

    //@ngInject
    constructor(
        private $scope: ng.IScope) {
        this.fromDate = new Date();
        this.toDate = new Date();

        this.$scope.$watch(() => this.userSelection, (newVal, oldVal) => {
            if (!newVal)
                return;

            this.userSelectionInputDate = this.userSelection.getDateRangeSelection();
            this.userSelectionInputCategory = this.userSelection.getIdListSelection(Constants.REPORTMENU_SELECTION_KEY_CATEGORIES);
            this.userSelectionInputInactive = this.userSelection.getBoolSelection(Constants.REPORTMENU_SELECTION_KEY_INCLUDE_INACTIVE);

            this.fromDate = this.userSelectionInputDate ? this.userSelectionInputDate.from : null;
            this.toDate = this.userSelectionInputDate ? this.userSelectionInputDate.to : null;
        });
    }

    public onDateRangeSelectionUpdated(selection: IDateRangeSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_DATE_RANGE, selection);

        this.fromDate = selection.from;
        this.toDate = selection.to;
    }

    public onCategorySelectionUpdated(selection: IIdListSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_CATEGORIES, selection);
    }

    public onBoolSelectionInactiveUpdated(selection: IBoolSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_INCLUDE_INACTIVE, selection);
    }
}
