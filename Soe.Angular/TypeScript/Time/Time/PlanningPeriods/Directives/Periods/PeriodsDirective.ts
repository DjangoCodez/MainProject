import { ITranslationService } from "../../../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../../../Core/Services/UrlHelperService";
import { TimePeriodDTO } from "../../../../../Common/Models/TimePeriodDTO";
import { PeriodsDialogController } from "./PeriodsDialogController";

export class PeriodsDirectiveFactory {
    //@ngInject
    public static create(translationService: ITranslationService, urlHelperService: IUrlHelperService): ng.IDirective {

        return {
            templateUrl: urlHelperService.getGlobalUrl('Time/Time/PlanningPeriods/Directives/Periods/Views/Periods.html'),
            scope: {
                periods: '=',
                readOnly: '=?',
                onChange: '&'
            },
            restrict: 'E',
            replace: true,
            controller: PeriodsDirectiveController,
            controllerAs: 'ctrl',
            bindToController: true
        };
    }
}

class PeriodsDirectiveController {

    // Terms
    terms: { [index: string]: string; };

    // Init parameters
    private periods: TimePeriodDTO[];
    private readOnly: boolean;
    private onChange: Function;

    private selectedTimePeriod: TimePeriodDTO;

    //@ngInject
    constructor(private $uibModal,
        private urlHelperService: IUrlHelperService) {
    }

    public $onInit() {
    }

    // ACTIONS

    private editPeriod(timePeriod: TimePeriodDTO) {

        let lastDate: Date;
        if (!timePeriod && this.periods.length > 0)
            lastDate = _.maxBy(this.periods, 'stopDate').stopDate;

        var options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Time/PlanningPeriods/Directives/Periods/Views/PeriodsDialog.html"),
            controller: PeriodsDialogController,
            controllerAs: "ctrl",
            size: 'md',
            resolve: {
                timePeriod: () => { return timePeriod },
                lastDate: () => { return lastDate }
            }
        }
        this.$uibModal.open(options).result.then((result: any) => {
            if (result && result.timePeriod) {
                if (timePeriod) {
                    // Update original period
                    this.setTimePeriodDataFromResult(timePeriod, result.timePeriod);
                } else {
                    // Add new period to the original collection
                    timePeriod = new TimePeriodDTO();
                    this.setTimePeriodDataFromResult(timePeriod, result.timePeriod);
                    if (!this.periods)
                        this.periods = [];
                    this.periods.push(timePeriod);
                }

                this.selectedTimePeriod = timePeriod;

                if (this.onChange)
                    this.onChange();
            }
        });
    }

    private deletePeriod(timePeriod: TimePeriodDTO) {
        _.pull(this.periods, timePeriod);

        if (this.onChange)
            this.onChange();
    }

    // HELP-METHODS

    private setTimePeriodDataFromResult(timePeriod: TimePeriodDTO, resultTimePeriod: TimePeriodDTO) {
        timePeriod.name = resultTimePeriod.name;
        timePeriod.startDate = resultTimePeriod.startDate;
        timePeriod.stopDate = resultTimePeriod.stopDate;
        if (!timePeriod.rowNr)
            timePeriod.rowNr = this.getNextRowNr();
    }

    private getNextRowNr() {
        var rowNr = 0;
        var maxRow = _.maxBy(this.periods, 'rowNr');
        if (maxRow)
            rowNr = maxRow.rowNr;
        return rowNr + 1;
    }

}
