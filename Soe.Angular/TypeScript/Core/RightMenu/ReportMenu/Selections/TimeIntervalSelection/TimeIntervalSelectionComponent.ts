import { IReportDataService } from "../../ReportDataService";
import { ISmallGenericType } from "../../../../../Scripts/TypeLite.Net4";
import { DateRangeSelectionDTO } from "../../../../../Common/Models/ReportDataSelectionDTO";
import { Constants } from "../../../../../Util/Constants";

export class TimeIntervalSelection {

    public static component(): ng.IComponentOptions {
        const options: ng.IComponentOptions = {
            controller: TimeIntervalSelection,
            templateUrl: soeConfig.baseUrl + "Core/RightMenu/ReportMenu/Selections/TimeIntervalSelection/TimeIntervalSelectionView.html",
            bindings: {
                onSelected: "&",
                labelKey: "@",
                hideLabel: "<",
                userSelectionInput: "="
            }
        };

        return options;
    }

    public static componentKey = "timeIntervalSelection";

    private onSelected: (_: { selection: DateRangeSelectionDTO }) => void = angular.noop;
    private userSelectionInput: DateRangeSelectionDTO;

    private timeIntervals: ISmallGenericType[] = [];
    private timeIntervalsLoaded: boolean = false;
    private selectedTimeIntervalId: number;
    private selectedRangeFrom: Date;
    private selectedRangeTo: Date;

    private delaySetSavedUserSelection: boolean = false;

    //@ngInject
    constructor(
        private $scope: ng.IScope,
        private $timeout: ng.ITimeoutService,
        private reportDataService: IReportDataService) {

        this.$scope.$watch(() => this.userSelectionInput, () => {
            this.setSavedUserSelection();
        });
    }

    public $onInit() {
        this.reportDataService.getSysTimeIntervals().then(x => {
            this.timeIntervals = x;
            this.timeIntervalsLoaded = true;
            if (this.timeIntervals.length > 0) {
                this.selectedTimeIntervalId = this.timeIntervals[0].id;
                this.onTimeIntervalChanged();
            }

            if (this.delaySetSavedUserSelection)
                this.setSavedUserSelection();
        });
    }

    private setSavedUserSelection() {
        if (!this.userSelectionInput)
            return;

        this.selectedTimeIntervalId = this.userSelectionInput ? this.userSelectionInput.id : 0;

        if (this.selectedTimeIntervalId && !this.timeIntervalsLoaded)
            this.delaySetSavedUserSelection = true;

        this.onTimeIntervalChanged();
    }

    private onTimeIntervalChanged() {
        this.$timeout(() => {
            this.reportDataService.getSysTimeIntervalDateRange(this.selectedTimeIntervalId).then(range => {
                this.selectedRangeFrom = range ? range.start : null;
                this.selectedRangeTo = range ? range.stop : null;

                const selection = new DateRangeSelectionDTO(Constants.REPORTMENU_DATERANGESELECTION_TYPE_DATERANGE, this.selectedRangeFrom, this.selectedRangeTo, false, this.selectedTimeIntervalId);

                this.onSelected({ selection });
            });
        })
    }

    private get isBeginningOfDay(): boolean {
        return this.selectedRangeFrom && this.selectedRangeFrom.isBeginningOfDay();
    }

    private get isEndOfDay(): boolean {
        return this.selectedRangeTo && this.selectedRangeTo.isEndOfDay();
    }
}