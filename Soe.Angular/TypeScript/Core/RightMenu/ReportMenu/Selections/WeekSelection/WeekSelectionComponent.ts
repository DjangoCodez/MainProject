import { ISmallGenericType } from "../../../../../Scripts/TypeLite.Net4";
import { CalendarUtility } from "../../../../../Util/CalendarUtility";
import { DateRangeSelectionDTO } from "../../../../../Common/Models/ReportDataSelectionDTO";
import { Constants } from "../../../../../Util/Constants";

interface ISelectableViewModel {
    id: number;
    label: string;
}

export class WeekSelection {
    public static component(): ng.IComponentOptions {
        const options: ng.IComponentOptions = {
            controller: WeekSelection,
            templateUrl: soeConfig.baseUrl + "Core/RightMenu/ReportMenu/Selections/WeekSelection/WeekSelectionView.html",
            bindings: {
                onSelected: "&",
                labelKey: "@",
                additionalLabelKey: "@",
                hideLabel: "<",
                userSelectionInput: "="
            }
        };

        return options;
    }

    public static componentKey = "weekSelection";

    private onSelected: (_: { selection: DateRangeSelectionDTO }) => void = angular.noop;
    private userSelectionInput: DateRangeSelectionDTO;

    private selectedYear: number;
    private selectedWeek: number;

    private availableSelectableYears: ISelectableViewModel[];
    private availableSelectableWeeks: ISelectableViewModel[];

    //@ngInject
    constructor(private $scope: ng.IScope) {
        this.selectedYear = CalendarUtility.getCurrentYear();
        this.selectedWeek = CalendarUtility.getCurrentWeekNr();

        this.availableSelectableYears = _.range(this.selectedYear - 2, this.selectedYear + 2, 1)
            .sort((a, b) => b - a)
            .map(y => <ISelectableViewModel>{ id: y, label: y.toString() });

        this.setAvailableWeeks();

        this.$scope.$watch(() => this.userSelectionInput, () => {
            this.setSavedUserSelection();
        });
    }

    public $onInit() {
        this.propagateSelection();
    }

    private setSavedUserSelection() {
        if (!this.userSelectionInput)
            return;

        this.changeYearAndWeek(this.userSelectionInput.from);
    }

    private changeYear(year: number) {
        this.selectedYear = year;
        this.setAvailableWeeks();
        this.propagateSelection();
    }

    private changeWeek(month: number) {
        this.selectedWeek = month;
        this.propagateSelection();
    }

    private changeYearAndWeek(date: Date) {
        this.selectedYear = date ? date.getFullYear() : 0;
        this.setAvailableWeeks();
        this.selectedWeek = date ? date.week() : 0;
        this.propagateSelection();
    }

    private setAvailableWeeks() {
        this.availableSelectableWeeks = _.range(1, CalendarUtility.getWeeksInYear(this.selectedYear) + 1, 1)
            .map(m => <ISelectableViewModel>{ id: m, label: m.toString() });
    }

    private propagateSelection() {
        const range = CalendarUtility.weekToDateRange(this.selectedYear, this.selectedWeek);
        this.onSelected({ selection: new DateRangeSelectionDTO(Constants.REPORTMENU_DATERANGESELECTION_TYPE_WEEK, range.beginning, range.end) });
    }
}