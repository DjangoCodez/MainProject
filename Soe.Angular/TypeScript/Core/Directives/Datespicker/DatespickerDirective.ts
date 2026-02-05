import { ITranslationService } from "../../Services/TranslationService";
import { IUrlHelperService } from "../../Services/UrlHelperService";
import { CalendarUtility } from "../../../Util/CalendarUtility";

export class DatespickerDirectiveFactory {
    //@ngInject
    public static create(translationService: ITranslationService, urlHelperService: IUrlHelperService): ng.IDirective {

        return {
            templateUrl: urlHelperService.getCoreDirectiveUrl('Datespicker', 'datespicker.html'),
            scope: {
                labelKey: '@',
                dates: '=',
                orderByDesc: '@'
            },
            restrict: 'E',
            replace: true,
            controller: DatespickerController,
            controllerAs: 'ctrl',
            bindToController: true
        };
    }
}

export class DatespickerController {
    private labelKey: string;
    private dates: Date[];
    private orderByDesc: boolean;
    private selectedDate: Date;

    //@ngInject
    constructor(private $timeout: ng.ITimeoutService) {
    }

    private selectedDateChanged() {
        this.$timeout(() => {
            if (!CalendarUtility.includesDate(this.dates, this.selectedDate)) {
                if (!this.dates)
                    this.dates = [];
                this.dates.push(this.selectedDate.date());
            }

            this.selectedDate = undefined;
        });
    }

    private removeDate(date) {
        _.pull(this.dates, date);
    }
}