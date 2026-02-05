import { ITranslationService } from "../../../../../Core/Services/TranslationService";
import { TimeScheduleScenarioHeadDTO } from "../../../../../Common/Models/TimeSchedulePlanningDTOs";

export class CreateTemplateFromScenarioVerifyController {

    // Terms
    private terms: { [index: string]: string; };
    private infoLabel: string;

    private validationCode: number;
    private get isValidCode(): boolean {
        return this.validationCode === this.scenarioHead.timeScheduleScenarioHeadId;
    }

    //@ngInject
    constructor(
        private $uibModalInstance,
        private $q: ng.IQService,
        private translationService: ITranslationService,
        private scenarioHead: TimeScheduleScenarioHeadDTO,
        private dateFrom: Date,
        private dateTo: Date) {
    }

    public $onInit() {
        this.$q.all([
            this.loadTerms()
        ]);
    }

    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
            "time.schedule.planning.scenario.createtemplate.verify.inforow1",
            "time.schedule.planning.scenario.createtemplate.verify.inforow2"
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
            if (this.dateTo)
                this.infoLabel = this.terms["time.schedule.planning.scenario.createtemplate.verify.inforow1"].format(this.scenarioHead.name, this.dateFrom.toFormattedDate(), this.dateTo.toFormattedDate());
            else
                this.infoLabel = this.terms["time.schedule.planning.scenario.createtemplate.verify.inforow2"].format(this.scenarioHead.name, this.dateFrom.toFormattedDate());
        });
    }

    private cancel() {
        this.$uibModalInstance.dismiss('cancel');
    }

    private ok() {
        this.$uibModalInstance.close({ success: true });
    }
}