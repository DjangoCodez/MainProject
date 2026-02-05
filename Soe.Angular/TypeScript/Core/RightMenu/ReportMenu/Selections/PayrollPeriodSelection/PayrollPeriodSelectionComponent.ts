import { IReportDataService } from "../../ReportDataService";
import { ISmallGenericType, ISelectableTimePeriodDTO } from "../../../../../Scripts/TypeLite.Net4";
import { DateRangeSelectionDTO, IdListSelectionDTO, IdSelectionDTO } from "../../../../../Common/Models/ReportDataSelectionDTO";
import { TermGroup_TimePeriodType } from "../../../../../Util/CommonEnumerations";
import { TimePeriodHeadDTO } from "../../../../../Common/Models/TimePeriodHeadDTO";

interface TimePeriodSelectModel {
    id: number;
    label: string;
}

export class PayrollPeriodSelection {

    public static component(): ng.IComponentOptions {
        const options: ng.IComponentOptions = {
            controller: PayrollPeriodSelection,
            templateUrl: soeConfig.baseUrl + "Core/RightMenu/ReportMenu/Selections/PayrollPeriodSelection/PayrollPeriodSelectionView.html",
            bindings: {
                onTimePeriodHeadSelected: "&",
                onSelected: "&",
                labelKey: "@",
                additionalLabelKey: "@",
                hideLabel: "<",
                hideAdditionalLabel: "<",
                showTimePeriodHeadSelector: "@",
                userSelectionInput: "="
            }
        };

        return options;
    }

    public static componentKey = "payrollPeriodSelection";

    private showTimePeriodHeadSelector: boolean;
    private onTimePeriodHeadSelected: (_: { selections: IdSelectionDTO }) => void = angular.noop;
    private onSelected: (_: { selections: IdListSelectionDTO }) => void = angular.noop;
    private userSelectionInput: IdListSelectionDTO;

    private timePeriodHeads: TimePeriodHeadDTO[] = [];
    private selectedTimePeriodHead: TimePeriodHeadDTO;
    private selectedTimePeriodId: number;

    private allTimePeriods: Map<number, ISelectableTimePeriodDTO> = new Map<number, ISelectableTimePeriodDTO>();
    private availableTimePeriods: TimePeriodSelectModel[] = [];
    private selectedTimePeriods: TimePeriodSelectModel[] = [];

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
        if (this.showTimePeriodHeadSelector) {
            this.reportDataService.getTimePeriodHeads(TermGroup_TimePeriodType.Payroll, false, true).then(x => {
                this.timePeriodHeads = x;
                if (this.timePeriodHeads.length > 0) {
                    this.selectedTimePeriodHead = this.timePeriodHeads[0];
                    this.onTimePeriodHeadChanged();
                }

                if (this.delaySetSavedUserSelection)
                    this.setSavedUserSelection();
            });
        } else {
            this.reportDataService.getAllPayrollTimePeriods().then(x => {
                this.allTimePeriods.clear();
                x.forEach(tp => this.allTimePeriods.set(tp.id, tp));
                this.availableTimePeriods = x.map(t => <TimePeriodSelectModel>{ id: t.id, label: t.displayName })

                if (this.delaySetSavedUserSelection)
                    this.setSavedUserSelection();
            });
        }

        this.selectedTimePeriods = [];
    }

    private setSavedUserSelection() {
        if (!this.userSelectionInput)
            return;

        if ((this.showTimePeriodHeadSelector && this.timePeriodHeads.length === 0) ||
            (!this.showTimePeriodHeadSelector && this.availableTimePeriods.length === 0)) {
            this.delaySetSavedUserSelection = true;
            return;
        }

        this.selectedTimePeriods = [];
        if (this.userSelectionInput.ids && this.userSelectionInput.ids.length > 0)
            this.selectedTimePeriods = _.filter(this.availableTimePeriods, p => _.includes(this.userSelectionInput.ids, p.id));

        this.onTimePeriodsSelected();
    }

    private onTimePeriodHeadChanged() {
        this.$timeout(() => {
            const selections = new IdSelectionDTO(this.selectedTimePeriodHead ? this.selectedTimePeriodHead.timePeriodHeadId : 0);

            this.onTimePeriodHeadSelected({ selections });
        })
    }

    private onTimePeriodSelected() {
        this.$timeout(() => {
            const selections = new IdListSelectionDTO([this.selectedTimePeriodId]);

            this.onSelected({ selections });
        });
    }

    private onTimePeriodsSelected() {
        const selections = new IdListSelectionDTO(this.selectedTimePeriods.map(s => s.id));

        this.onSelected({ selections });
    }
}