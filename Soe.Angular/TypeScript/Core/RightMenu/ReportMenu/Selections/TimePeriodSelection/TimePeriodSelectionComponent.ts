import { ISmallGenericType, ITimePeriodDTO } from "../../../../../Scripts/TypeLite.Net4";
import { CalendarUtility } from "../../../../../Util/CalendarUtility";
import { DateRangeSelectionDTO } from "../../../../../Common/Models/ReportDataSelectionDTO";
import { IReportDataService } from "../../ReportDataService";
import { Constants } from "../../../../../Util/Constants";

export class TimePeriodSelection {
    public static component(): ng.IComponentOptions {
        const options: ng.IComponentOptions = {
            controller: TimePeriodSelection,
            templateUrl: soeConfig.baseUrl + "Core/RightMenu/ReportMenu/Selections/TimePeriodSelection/TimePeriodSelectionView.html",
            bindings: {
                onSelected: "&",
                labelKey: "@",
                hideLabel: "<",
                userSelectionInput: "="
            }
        };

        return options;
    }

    public static componentKey = "timePeriodSelection";

    private onSelected: (_: { selection: DateRangeSelectionDTO }) => void = angular.noop;
    private userSelectionInput: DateRangeSelectionDTO;

    private selectedTimePeriodId: number;
    private availableSelectableTimePeriods: { id: number, label: string, origin: ITimePeriodDTO }[];
    private allTimePeriods: Map<number, ITimePeriodDTO> = new Map<number, ITimePeriodDTO>();

    private delaySetSavedUserSelection: boolean = false;

    //@ngInject
    constructor(private $scope: ng.IScope, private reportDataService: IReportDataService) {
        this.reportDataService.getDefaultTimePeriods().then(timePeriods => {
            timePeriods.forEach(tp => this.allTimePeriods.set(tp.timePeriodId, tp));
            this.availableSelectableTimePeriods = timePeriods.map(p => { return { id: p.timePeriodId, label: p.name, origin: p } });
            this.timePeriodChanged(0);

            if (this.delaySetSavedUserSelection)
                this.setSavedUserSelection();
        });

        this.$scope.$watch(() => this.userSelectionInput, () => {
            this.setSavedUserSelection();
        });
    }

    private setSavedUserSelection() {
        if (!this.userSelectionInput)
            return;

        if (!this.availableSelectableTimePeriods || this.availableSelectableTimePeriods.length === 0) {
            this.delaySetSavedUserSelection = true;
            return;
        }

        if (!this.userSelectionInput.from || !this.userSelectionInput.to) {
            this.timePeriodChanged(0);
        } else {
            _.forEach(this.availableSelectableTimePeriods, p => {
                let timePeriod = this.allTimePeriods.get(p.id);
                if (timePeriod.startDate.isSameDayAs(this.userSelectionInput.from) && timePeriod.stopDate.isSameDayAs(this.userSelectionInput.to)) {
                    this.timePeriodChanged(p.id);
                    return false;
                }
            });
        }
    }

    private timePeriodChanged(timePeriodId: number) {
        if (!timePeriodId) {
            let today: Date = CalendarUtility.getDateToday();
            this.allTimePeriods.forEach((value: ITimePeriodDTO, key: number) => {
                if (value.startDate.isSameOrBeforeOnDay(today) && value.stopDate.isSameOrAfterOnDay(today)) {
                    timePeriodId = key;
                    return false;
                }
            });
        }

        this.selectedTimePeriodId = timePeriodId;
        this.propagateSelection();
    }

    private propagateSelection() {
        const timePeriod = this.allTimePeriods.get(this.selectedTimePeriodId);
        if (timePeriod)
            this.onSelected({ selection: new DateRangeSelectionDTO(Constants.REPORTMENU_DATERANGESELECTION_TYPE_TIME_PERIOD, timePeriod.startDate, timePeriod.stopDate) });
    }
}