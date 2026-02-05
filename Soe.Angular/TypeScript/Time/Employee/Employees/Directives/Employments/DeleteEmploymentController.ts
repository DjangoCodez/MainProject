import { ITranslationService } from "../../../../../Core/Services/TranslationService";
import { IFocusService } from "../../../../../Core/Services/FocusService";
import { EmploymentDTO } from "../../../../../Common/Models/EmployeeUserDTO";

export class DeleteEmploymentController {

    // Terms
    private terms: { [index: string]: string; };

    // Properties
    private comment: string;

    //@ngInject
    constructor(
        private $uibModalInstance,
        private translationService: ITranslationService,
        private focusService: IFocusService,
        private employment: EmploymentDTO) {

        this.loadTerms();

        this.focusService.focusById("ctrl_comment");
    }

    private loadTerms() {
        var keys: string[] = [
        ];

        this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
        });
    }

    // EVENTS

    private cancel() {
        this.$uibModalInstance.dismiss('cancel');
    }

    private save() {
        this.$uibModalInstance.close({
            success: true,
            comment: this.comment
        });
    }
}
