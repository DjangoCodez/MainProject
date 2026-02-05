import { ISmallGenericType } from "../../../../../Scripts/TypeLite.Net4";
import { CalendarUtility } from "../../../../../Util/CalendarUtility";
import { DateRangeSelectionDTO } from "../../../../../Common/Models/ReportDataSelectionDTO";
import { Constants } from "../../../../../Util/Constants";

interface ISelectableViewModel {
    id: number;
    label: string;
}

export class MonthSelection {
    public static component(): ng.IComponentOptions {
        const options: ng.IComponentOptions = {
            controller: MonthSelection,
            templateUrl: soeConfig.baseUrl + "Core/RightMenu/ReportMenu/Selections/MonthSelection/MonthSelectionView.html",
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

    public static componentKey = "monthSelection";

    private onSelected: (_: { selection: DateRangeSelectionDTO }) => void = angular.noop;
    private userSelectionInput: DateRangeSelectionDTO;

    private selectedYear: number;
    private selectedMonth: number;

    private availableSelectableYears: ISelectableViewModel[];
    private availableSelectableMonths: ISelectableViewModel[];

    //@ngInject
    constructor(private $scope: ng.IScope) {
        this.selectedYear = CalendarUtility.getCurrentYear();
        this.selectedMonth = CalendarUtility.getCurrentMonth();

        this.availableSelectableYears = _.range(this.selectedYear - 2, this.selectedYear + 2, 1)
            .sort((a, b) => b - a)
            .map(y => <ISelectableViewModel>{ id: y, label: y.toString() });

        this.availableSelectableMonths = _.range(0, 12, 1)
            .map(m => <ISelectableViewModel>{ id: m, label: CalendarUtility.getMonthName(m) });

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

        this.changeYearAndMonth(this.userSelectionInput.from);
    }

    private changeYear(year: number) {
        this.selectedYear = year;
        this.propagateSelection();
    }

    private changeMonth(month: number) {
        this.selectedMonth = month;
        this.propagateSelection();
    }

    private changeYearAndMonth(date: Date) {
        this.selectedYear = date ? date.getFullYear() : 0;
        this.selectedMonth = date ? date.getMonth() : 0;
        this.propagateSelection();
    }

    private propagateSelection() {
        const range = CalendarUtility.monthToDateRange(this.selectedYear, this.selectedMonth);
        this.onSelected({ selection: new DateRangeSelectionDTO(Constants.REPORTMENU_DATERANGESELECTION_TYPE_MONTH, range.beginning, range.end) });
    }
}