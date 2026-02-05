import { IDateRangeSelectionDTO, IDateSelectionDTO } from "../../../../../Scripts/TypeLite.Net4";
import { DateRangeSelectionDTO, DateSelectionDTO } from "../../../../../Common/Models/ReportDataSelectionDTO";
import { Constants } from "../../../../../Util/Constants";
import { CalendarUtility } from "../../../../../Util/CalendarUtility";

type DateTimeIntervalSelectionDTO = DateSelectionDTO | DateRangeSelectionDTO;

export class DateSelection {
    public static component(): ng.IComponentOptions {
        const options: ng.IComponentOptions = {
            controller: DateSelection,
            templateUrl: soeConfig.baseUrl + "Core/RightMenu/ReportMenu/Selections/DateSelection/DateSelectionView.html",
            bindings: {
                onSelected: "&",
                labelKey: "@",
                additionalLabelKey: "@",
                hideLabel: "<",
                showRange: "<",
                asRange: "<",
                date: "<",
                from: "<",
                to: "<",
                isFromDateMandatory: "<",
                isToDateMandatory: "<",
                useMinMaxIfEmpty: "<",
                userSelectionInput: "=",
                option: "<",
                setFirstAndLastDate: "<",
                hideTodayButton:"<"
            }
        };

        return options;
    }

    public static componentKey = "dateSelection";

    //binding properties
    private labelKey: string;
    private additionalLabelKey: string;
    private showRange: boolean;
    private asRange: boolean;
    private onSelected: (_: { selection: DateTimeIntervalSelectionDTO }) => void = angular.noop;
    private date: Date;
    private from: Date;
    private to: Date;
    private isFromDateMandatory: boolean = false;
    private isToDateMandatory: boolean = false;
    private useMinMaxIfEmpty: boolean;
    private invalidateFromTo: boolean = false;
    private userSelectionInput: DateTimeIntervalSelectionDTO;
    private selected: Date;
    private selectedAdditional: Date;
    private fromDateInvalid = false;
    private toDateInvalid = false;
    private option = undefined;
    private setFirstAndLastDate: boolean = false;
    private hideTodayButton: boolean = false;

    //@ngInject
    constructor(private $scope: ng.IScope,) {
        this.$scope.$watch(() => this.isFromDateMandatory, () => {
            this.validate(this.selected, this.selectedAdditional);
        });
        this.$scope.$watch(() => this.isToDateMandatory, () => {
            this.validate(this.selected, this.selectedAdditional);
        });
        this.$scope.$watch(() => this.userSelectionInput, () => {
            this.setSavedUserSelection();
        });
    }

    public $onInit() {
        this.selected = this.date || this.from;
        this.selectedAdditional = this.to;
        this.propagateChange(this.selected, this.selectedAdditional);
    }

    private setSavedUserSelection() {
        if (!this.userSelectionInput)
            return;

        if (this.asRange) {
            var userSelectionInputDateRange = (<DateRangeSelectionDTO>this.userSelectionInput);
            if (userSelectionInputDateRange) {
                this.selected = userSelectionInputDateRange.from;
                this.showRange = userSelectionInputDateRange.rangeType === Constants.REPORTMENU_DATERANGESELECTION_TYPE_DATERANGE;
                if (this.showRange)
                    this.selectedAdditional = userSelectionInputDateRange.to;

                if (this.selected && this.selectedAdditional) {
                    this.invalidateFromTo = (this.selected > this.selectedAdditional);
                }
            }
        }
        else {
            var userSelectionInputDate = (<DateSelectionDTO>this.userSelectionInput);
            if (userSelectionInputDate) {
                this.selected = userSelectionInputDate.date;
                this.showRange = false;
            }
        }

        if (this.showRange)
            this.onDatesChanged(this.selected, this.selectedAdditional);
        else
            this.onDateChange(this.selected);
    }

    private onDateChange(selected: Date) {
        this.invalidateFromTo = false;
        if (selected && this.selectedAdditional) {
            this.invalidateFromTo = (selected > this.selectedAdditional);
        }
        this.propagateChange(selected, this.selectedAdditional);
    }

    private onAdditionalDateChange(selectedAdditional: Date) {
        this.invalidateFromTo = false;

        if (this.selected && selectedAdditional) {
            this.invalidateFromTo = (this.selected > selectedAdditional);
        }
        this.propagateChange(this.selected, selectedAdditional);
    }

    private onDatesChanged(selected: Date, selectedAdditional: Date) {
        this.propagateChange(this.selected, selectedAdditional);
    }

    private propagateChange(selected: Date | undefined, selectedAdditional: Date | undefined) {
        let selection: (IDateSelectionDTO | IDateRangeSelectionDTO);
        if (this.setFirstAndLastDate) {
                selected = selected ? new Date(selected.getFullYear(), selected.getMonth(), 1) : undefined;

            selectedAdditional = selectedAdditional ? new Date(selectedAdditional.getFullYear(), selectedAdditional.getMonth() + 1, 0) : undefined;
        }
        if (this.asRange)
            selection = new DateRangeSelectionDTO(Constants.REPORTMENU_DATERANGESELECTION_TYPE_DATERANGE, selected, selectedAdditional, this.useMinMaxIfEmpty);
        else
            selection = new DateSelectionDTO(selected);
        this.validate(selected, selectedAdditional);
        this.onSelected({ selection: selection });
        if (this.setFirstAndLastDate) {
            if (!this.datesEqual(this.selected, selected)) {
                this.selected = selected;
            }

            if (!this.datesEqual(this.selectedAdditional, selectedAdditional)) {
                this.selectedAdditional = selectedAdditional;
            }
        }
    }
    private datesEqual(date1: Date | null, date2: Date | null): boolean {
        return date1?.getTime() === date2?.getTime();
    }

    private validate(from: Date, to: Date) {
        this.fromDateInvalid = false;
        this.toDateInvalid = false;
        if (this.isFromDateMandatory) {
            if (!(from && CalendarUtility.isValidDate(from))) {
                this.fromDateInvalid = true;
            }
        }
        if (this.isToDateMandatory) {
            if (!(to && CalendarUtility.isValidDate(to))) {
                this.toDateInvalid = true;
            }
        }
    }
}