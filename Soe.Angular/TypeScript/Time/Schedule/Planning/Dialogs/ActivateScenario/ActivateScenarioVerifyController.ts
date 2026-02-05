import { ITranslationService } from "../../../../../Core/Services/TranslationService";
import { TimeScheduleScenarioHeadDTO } from "../../../../../Common/Models/TimeSchedulePlanningDTOs";

export class ActivateScenarioVerifyController {

    // Terms
    private terms: { [index: string]: string; };
    private infoLabel: string;

    private validationCode: number;
    private get isValidCode(): boolean {
        return this.validationCode === this.scenarioHead.timeScheduleScenarioHeadId;
    }

    private activating: boolean = false;

    //@ngInject
    constructor(
        private $uibModalInstance,
        private $q: ng.IQService,
        private translationService: ITranslationService,
        private scenarioHead: TimeScheduleScenarioHeadDTO) {
    }

    public $onInit() {
        this.$q.all([
            this.loadTerms()
        ]);
    }

    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
            "time.schedule.planning.scenario.activate.verify.inforow1",
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
            this.infoLabel = this.terms["time.schedule.planning.scenario.activate.verify.inforow1"].format(this.scenarioHead.name, this.scenarioHead.dateFrom.toFormattedDate(), this.scenarioHead.dateTo.toFormattedDate());
        });
    }

    private cancel() {
        this.$uibModalInstance.dismiss('cancel');
    }

    private ok() {
        this.activating = true;
        this.$uibModalInstance.close({ success: true });
    }
}