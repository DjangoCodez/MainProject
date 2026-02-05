import { TimeScheduleTemplateHeadSmallDTO } from "../../../../../Common/Models/TimeScheduleTemplateDTOs";
import { ITranslationService } from "../../../../../Core/Services/TranslationService";
import { StringUtility } from "../../../../../Util/StringUtility";

export class SelectTemplateScheduleController {
    private message: string;

    //@ngInject
    constructor(private $uibModalInstance,
        private translationService: ITranslationService,
        private templateHeads: TimeScheduleTemplateHeadSmallDTO) {

        this.loadTerms();
    }

    private loadTerms() {
        this.translationService.translate('time.schedule.planning.selecttemplateschedule.message').then(term => {
            this.message = StringUtility.ToBr(term);
        });
    }

    private selectTemplate(templateHead: TimeScheduleTemplateHeadSmallDTO) {
        this.$uibModalInstance.close({ templateHead: templateHead });
    }

    private cancel() {
        this.$uibModalInstance.dismiss('cancel');
    }
}