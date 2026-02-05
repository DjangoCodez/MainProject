import { DateRangeSelectionDTO } from "../../../../../Common/Models/ReportDataSelectionDTO";
import { ITranslationService } from "../../../../Services/TranslationService";
import { Guid } from "../../../../../Util/StringUtility";
import { Constants } from "../../../../../Util/Constants";

interface MultiSelectViewModel {
    id: string;
    label: string;
}

export class DateTimeIntervalSelection {
    public static component(): ng.IComponentOptions {
        const options: ng.IComponentOptions = {
            controller: DateTimeIntervalSelection,
            templateUrl: soeConfig.baseUrl + "Core/RightMenu/ReportMenu/Selections/DateTimeIntervalSelection/DateTimeIntervalSelectionView.html",
            bindings: {
                selectableIntervals: "@",
                onTimePeriodChanged: "&",
                onSelected: "&",
                labelKey: "@",
                hideLabel: "<",
                selectedInterval: "@",
                useMinMaxIfEmpty: "<",
                userSelectionInput: "="
            }
        };

        return options;
    }

    public static componentKey = "dateTimeIntervalSelection";
    private static supportedIntervals = [
        { key: Constants.REPORTMENU_DATERANGESELECTION_TYPE_DATERANGE, termId: "common.daterange" },
        { key: Constants.REPORTMENU_DATERANGESELECTION_TYPE_DATE, termId: "common.date" },
        { key: Constants.REPORTMENU_DATERANGESELECTION_TYPE_WEEK, termId: "common.week" },
        { key: Constants.REPORTMENU_DATERANGESELECTION_TYPE_MONTH, termId: "common.month" },
        { key: Constants.REPORTMENU_DATERANGESELECTION_TYPE_TIME_INTERVAL, termId: "common.timeintervals" },
        { key: Constants.REPORTMENU_DATERANGESELECTION_TYPE_TIME_PERIOD, termId: "common.period" },
        { key: Constants.REPORTMENU_DATERANGESELECTION_TYPE_PLANNING_PERIOD, termId: "common.planningperiod" }
    ];

    private onTimePeriodChanged: (_: { item: string }) => void = angular.noop;
    private onSelected: (_: { selection: DateRangeSelectionDTO }) => void = angular.noop;
    private selectableIntervals: string;
    private availableSelectableIntervals: MultiSelectViewModel[];
    private selectedInterval: string;
    private useMinMaxIfEmpty: boolean;
    private userSelectionInput: DateRangeSelectionDTO;

    private termsFetcher: ng.IPromise<{ [index: string]: string }>;
    private terms: { [index: string]: string };
    private sharedClientKey: string;

    //@ngInject
    constructor(private $scope: ng.IScope, private $timeout: ng.ITimeoutService, private translationService: ITranslationService) {
        this.termsFetcher = this.translationService.translateMany(DateTimeIntervalSelection.supportedIntervals.map(x => x.termId));
        this.sharedClientKey = Guid.newGuid();

        this.$scope.$watch(() => this.userSelectionInput, () => {
            this.setSavedUserSelection();
        });
    }

    public $onInit() {
        this.termsFetcher.then((terms) => {
            this.terms = terms;
            this.parseSelectableIntervals();
        });

        if (this.selectableIntervals == null) {
            this.selectableIntervals = DateTimeIntervalSelection.supportedIntervals.map(x => x.key).join(",");
        }
    }

    private setSavedUserSelection() {
        if (!this.userSelectionInput)
            return;

        this.selectedInterval = this.userSelectionInput.rangeType;

        this.$timeout(() => {
            this.userSelectionInput = null;
        }, 1000);
    }

    private timePeriodChanged(item) {
        this.onTimePeriodChanged({ item });
    }

    private propagateSelection(selection: DateRangeSelectionDTO) {
        this.onSelected({ selection: new DateRangeSelectionDTO(this.selectedInterval, selection.from, selection.to, this.useMinMaxIfEmpty, selection.id) });
    }

    private parseSelectableIntervals() {
        this.availableSelectableIntervals = DateTimeIntervalSelection.supportedIntervals
            .filter(i => this.selectableIntervals.contains(i.key))
            .map(value => <MultiSelectViewModel>{ id: value.key, label: this.terms[value.termId] });
    }
}