import { ITranslationService } from "../../../../../Core/Services/TranslationService";
import { ICoreService } from "../../../../../Core/Services/CoreService";
import { TermGroup_HouseHoldTaxDeductionType } from "../../../../../Util/CommonEnumerations";

export class SetSequenceNumberController {

    private infoText: string;
    private selectedSeqNr: number;
    private enableOkButton: boolean;

    //@ngInject
    constructor(
        private $uibModalInstance,
        private translationService: ITranslationService,
        private coreService: ICoreService,
        private $q: ng.IQService,
        private taxDeductionType: TermGroup_HouseHoldTaxDeductionType,
    ) {

        this.$q.all([
            this.loadInfoText(),
            this.getNextSequenceNumber()]).then(() => {
                this.enableOkButton = true;
            });
    }

    private loadInfoText(): ng.IPromise<any> {
        return this.translationService.translate("billing.invoices.householddeduction.setseqnrdialoginfo").then((term) => {
            this.infoText = term;
        });
    }

    private getNextSequenceNumber(): ng.IPromise<any> {
        let entityName: string;
        switch (this.taxDeductionType) {
            case TermGroup_HouseHoldTaxDeductionType.ROT:
                entityName = "HouseholdTaxDeduction";
                break;
            case TermGroup_HouseHoldTaxDeductionType.RUT:
                entityName = "RutTaxDeduction";
                break;
            case TermGroup_HouseHoldTaxDeductionType.GREEN:
                entityName = "GreenTaxDeduction";
                break;
        }

        if (!entityName)
            return;

        return this.coreService.getLastUsedSequenceNumber(entityName).then((x) => {
            this.selectedSeqNr = x ? x + 1 : 1;
        });
    }

    buttonCancelClick() {
        this.close(null);
    }

    buttonOkClick() {
        this.close(this.selectedSeqNr);
    }

    close(result: any) {
        if (!result) {
            this.$uibModalInstance.dismiss('cancel');
        }
        else {
            this.$uibModalInstance.close(result);
        }
    }
}