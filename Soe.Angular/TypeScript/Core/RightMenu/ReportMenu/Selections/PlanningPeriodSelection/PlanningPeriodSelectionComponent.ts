import { ISmallGenericType, ITimePeriodDTO, ITimePeriodHeadDTO } from "../../../../../Scripts/TypeLite.Net4";
import { CalendarUtility } from "../../../../../Util/CalendarUtility";
import { DateRangeSelectionDTO } from "../../../../../Common/Models/ReportDataSelectionDTO";
import { IReportDataService } from "../../ReportDataService";
import { Constants } from "../../../../../Util/Constants";
import { TermGroup_TimePeriodType } from "../../../../../Util/CommonEnumerations";

export class PlanningPeriodSelection {
    public static component(): ng.IComponentOptions {
        const options: ng.IComponentOptions = {
            controller: PlanningPeriodSelection,
            templateUrl: soeConfig.baseUrl + "Core/RightMenu/ReportMenu/Selections/PlanningPeriodSelection/PlanningPeriodSelectionView.html",
            bindings: {
                onSelected: "&",
                headLabelKey: "@",
                hideHeadLabel: "<",
                labelKey: "@",
                hideLabel: "<",
                userSelectionInput: "="
            }
        };

        return options;
    }

    public static componentKey = "planningPeriodSelection";

    private onSelected: (_: { selection: DateRangeSelectionDTO }) => void = angular.noop;
    private userSelectionInput: DateRangeSelectionDTO;

    private selectedTimePeriodHeadId: number;
    private selectedTimePeriodId: number;
    private availableSelectableTimePeriodHeads: { id: number, label: string, origin: ITimePeriodHeadDTO }[];
    private availableSelectableTimePeriods: { id: number, label: string, origin: ITimePeriodDTO }[];
    private allTimePeriodHeads: Map<number, ITimePeriodHeadDTO> = new Map<number, ITimePeriodHeadDTO>();

    private delaySetSavedUserSelection: boolean = false;

    //@ngInject
    constructor(private $scope: ng.IScope, private reportDataService: IReportDataService) {
        this.reportDataService.getTimePeriodHeads(TermGroup_TimePeriodType.RuleWorkTime, true, true).then(timePeriodHeads => {
            timePeriodHeads.forEach(tp => this.allTimePeriodHeads.set(tp.timePeriodHeadId, tp));
            this.availableSelectableTimePeriodHeads = timePeriodHeads.map(p => { return { id: p.timePeriodHeadId, label: p.name, origin: p } });

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

        if (!this.availableSelectableTimePeriodHeads || this.availableSelectableTimePeriodHeads.length === 0) {
            this.delaySetSavedUserSelection = true;
            return;
        }

        if (!this.userSelectionInput.from || !this.userSelectionInput.to) {
            this.timePeriodHeadChanged(0);
        } else {
            // Find time period by date
            _.forEach(this.availableSelectableTimePeriodHeads, h => {
                let timePeriodHead = this.allTimePeriodHeads.get(h.id);
                if (timePeriodHead) {
                    _.forEach(timePeriodHead.timePeriods, p => {
                        if (p.startDate.isSameDayAs(this.userSelectionInput.from) && p.stopDate.isSameDayAs(this.userSelectionInput.to)) {
                            this.timePeriodHeadChanged(h.id);
                            this.timePeriodChanged(p.timePeriodId);
                            return false;
                        }
                    });
                }
            });
        }
    }

    private timePeriodHeadChanged(timePeriodHeadId: number) {
        this.selectedTimePeriodHeadId = timePeriodHeadId;

        let timePeriodHead = this.allTimePeriodHeads.get(timePeriodHeadId);
        if (timePeriodHead)
            this.availableSelectableTimePeriods = timePeriodHead.timePeriods.map(p => { return { id: p.timePeriodId, label: p.name, origin: p } });

        this.propagateSelection();
    }

    private timePeriodChanged(timePeriodId: number) {
        this.selectedTimePeriodId = timePeriodId;
        this.propagateSelection();
    }

    private propagateSelection() {
        const timePeriodHead = this.allTimePeriodHeads.get(this.selectedTimePeriodHeadId);
        if (timePeriodHead) {
            const timePeriod = _.find(timePeriodHead.timePeriods, p => p.timePeriodId === this.selectedTimePeriodId);
            if (timePeriod)
                this.onSelected({ selection: new DateRangeSelectionDTO(Constants.REPORTMENU_DATERANGESELECTION_TYPE_TIME_PERIOD, timePeriod.startDate, timePeriod.stopDate) });
        }
    }
}