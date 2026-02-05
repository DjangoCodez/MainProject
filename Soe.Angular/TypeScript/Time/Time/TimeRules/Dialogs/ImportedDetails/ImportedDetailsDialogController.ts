import { ModalUtility } from "../../../../../Util/ModalUtility";
import { ITranslationService } from "../../../../../Core/Services/TranslationService";
import { ITimeService } from "../../../Timeservice";
import { TimeRuleImportedDetailsDTO } from "../../../../../Common/Models/TimeRuleDTOs";

export class ImportedDetailsDialogController {

    // Terms
    private terms: { [index: string]: string; };

    // Data
    private details: TimeRuleImportedDetailsDTO;

    // Flags
    private showOriginalJson: boolean = false;

    //@ngInject
    constructor(
        private $q: ng.IQService,
        private $timeout: ng.ITimeoutService,
        private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
        private translationService: ITranslationService,
        private timeService: ITimeService,
        private timeRuleId: number) {

        this.loadTerms().then(() => {
            this.load();
        });
    }

    // SERVICE CALLS

    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
        });
    }

    private load(): ng.IPromise<any> {
        return this.timeService.getTimeRuleImportedDetails(this.timeRuleId, true).then(x => {
            this.details = x;
        });
    }

    // EVENTS

    private showExportedFile(value: boolean) {
        this.showOriginalJson = value;
    }

    private copyToClipboard() {
        var elem = document.getElementById(this.showOriginalJson ? 'originalJson' : 'json');
        var range = document.createRange();
        range.selectNodeContents(elem);
        var sel = window.getSelection();
        sel.removeAllRanges();
        sel.addRange(range);
        document.execCommand('copy');
        sel.removeAllRanges();
    }

    private close() {
        this.$uibModalInstance.dismiss(ModalUtility.MODAL_CANCEL);
    }
}
