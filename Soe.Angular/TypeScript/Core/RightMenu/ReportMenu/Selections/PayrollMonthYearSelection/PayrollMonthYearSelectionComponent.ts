import { IReportDataService } from "../../ReportDataService";
import { ISmallGenericType, ISelectablePayrollMonthYearDTO } from "../../../../../Scripts/TypeLite.Net4";
import { CalendarUtility } from "../../../../../Util/CalendarUtility";
import { IdListSelectionDTO } from "../../../../../Common/Models/ReportDataSelectionDTO";

interface ISelectableViewModel {
    id: number;
    label: string;
}

export class PayrollMonthYearSelection {
    public static component(): ng.IComponentOptions {
        const options: ng.IComponentOptions = {
            controller: PayrollMonthYearSelection,
            templateUrl: soeConfig.baseUrl + "Core/RightMenu/ReportMenu/Selections/PayrollMonthYearSelection/PayrollMonthYearSelectionView.html",
            bindings: {
                onSelected: "&",
                labelKey: "@",
                hideLabel: "<",
                useYears: "<",
                useMonths: "<",
                userSelectionInput: "="
            }
        };

        return options;
    }

    public static componentKey = "payrollMonthYearSelection";

    private allValues: Map<number, ISelectablePayrollMonthYearDTO> = new Map<number, ISelectablePayrollMonthYearDTO>();
    private selectedValues: ISelectableViewModel[];
    private availableValues: ISelectableViewModel[];

    //bindings properties
    private onSelected: (_: { selections: IdListSelectionDTO }) => void = angular.noop;
    private useYears: boolean = false;
    private useMonths: boolean = false;
    private userSelectionInput: IdListSelectionDTO;

    private delaySetSavedUserSelection: boolean = false;

    //@ngInject
    constructor(private $scope: ng.IScope, private reportDataService: IReportDataService) {
        this.$scope.$watch(() => this.userSelectionInput, () => {
            this.setSavedUserSelection();
        });
    }

    public $onInit() {
        if (this.useYears) {
            this.reportDataService.getPayrollYears().then(values => {
                this.dataLoaded(values);
            });
        } else if (this.useMonths) {
            this.reportDataService.getPayrollMonths().then(values => {
                this.dataLoaded(values);
            });
        }
    }

    private setSavedUserSelection() {
        if (!this.userSelectionInput)
            return;

        if (this.availableValues.length === 0) {
            this.delaySetSavedUserSelection = true;
            return;
        }

        this.selectedValues = [];
        if (this.userSelectionInput.ids && this.userSelectionInput.ids.length > 0) {
            let matched: number[] = [];
            this.allValues.forEach((value: ISelectablePayrollMonthYearDTO, key: number) => {
                if (value.timePeriodIds.every(elem => this.userSelectionInput.ids.indexOf(elem) > -1))
                    matched.push(key);
            });

            if (matched.length > 0)
                this.selectedValues = _.filter(this.availableValues, v => _.includes(matched, v.id));
        }

        this.propagateSelection();
    }

    private dataLoaded(values: ISelectablePayrollMonthYearDTO[]) {
        this.allValues.clear();
        values.forEach(value => this.allValues.set(value.id, value));
        this.availableValues = values.map(t => <ISelectableViewModel>{ id: t.id, label: t.displayName })
        this.selectedValues = [];
        this.propagateSelection();

        if (this.delaySetSavedUserSelection)
            this.setSavedUserSelection();
    }

    private propagateSelection() {
        let selectedIds = this.selectedValues.map(s => s.id)
        let selectedTimePeriodIds: number[] = [];

        _.forEach(selectedIds, (id: number) => {
            let value = this.allValues.get(id);
            _.forEach(value.timePeriodIds, (timePeriodId: number) => {
                selectedTimePeriodIds.push(timePeriodId);
            });
        });

        const selections = new IdListSelectionDTO(selectedTimePeriodIds);
        this.onSelected({ selections });
    }
}