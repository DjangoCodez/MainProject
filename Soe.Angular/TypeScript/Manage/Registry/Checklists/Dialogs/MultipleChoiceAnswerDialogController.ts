import { NotificationService } from "../../../../Core/Services/NotificationService";
import { TranslationService } from "../../../../Core/Services/TranslationService";
import { CheckListMultipleChoiceAnswerRowDTO } from "../../../../Common/Models/CheckListMultipleChoiceAnswerRowDTO";

//import { SOEMessageBoxImage, SOEMessageBoxButtons } from "../../../../Util/Enumerations";

export class MultipleChoiceAnswerDialogController {

    private answerRow: CheckListMultipleChoiceAnswerRowDTO;    

    //@ngInject
    constructor(
        private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
        private translationService: TranslationService,
        private notificationService: NotificationService,
        answerRow: CheckListMultipleChoiceAnswerRowDTO) {        

        this.answerRow = new CheckListMultipleChoiceAnswerRowDTO();

        angular.extend(this.answerRow, answerRow);
    }

    public cancel() {
        this.$uibModalInstance.close();
    }

    public ok() {
        this.$uibModalInstance.close({ answerRow: this.answerRow });
    }

}
