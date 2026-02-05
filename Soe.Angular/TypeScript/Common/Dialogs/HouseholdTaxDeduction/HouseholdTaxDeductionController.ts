import { HouseholdTaxDeductionApplicantDTO } from "../../Models/householdtaxdeductionapplicantdto";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IValidationSummaryHandlerFactory } from "../../../Core/Handlers/validationsummaryhandlerfactory";
import { IValidationSummaryHandler } from "../../../Core/Handlers/ValidationSummaryHandler";

export class HouseholdTaxDeductionController {
    private errorMessage: string;
    protected validationHandler: IValidationSummaryHandler;
    private socialSecurityNumberFormat = "YYYYMMDD-XXXX";

    //@ngInject
    constructor(
        private $uibModalInstance,
        private $window: ng.IWindowService,
        private translationService: ITranslationService,
        private title: string,
        private applicant: HouseholdTaxDeductionApplicantDTO,
        private validationSummaryHandlerFactory?: IValidationSummaryHandlerFactory) {

        if (validationSummaryHandlerFactory)
            this.validationHandler = validationSummaryHandlerFactory.create();
    }

    buttonCancelClick() {
        this.$uibModalInstance.dismiss('cancel');
    }

    buttonOkClick() {
        this.errorMessage = "";
        if (!((this.applicant.property && this.applicant.property.length > 0) || (this.applicant.apartmentNr && this.applicant.apartmentNr.length > 0 && this.applicant.cooperativeOrgNr && this.applicant.cooperativeOrgNr.length > 0))) {
            this.translationService.translate("billing.productrows.houseorapartmentrequired").then((text) => {
                this.errorMessage = "* " + text;
            });
        }
        else {
            this.$uibModalInstance.close(this.applicant);
        }
    }

    public showValidationError() {
        const errors = this['edit'].$error;
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
            if (this.applicant) {
                if (!this.applicant.name)
                    mandatoryFieldKeys.push("common.name");

                if (errors['socialSecurityNumber']) {
                    _.forEach(errors['socialSecurityNumber'], (value) => {
                        if (value.$name === 'ctrl_applicant_socialSecNr')
                            validationErrorKeys.push("billing.productrows.invalidsocialsecuritynumber");
                        else if (value.$name === 'ctrl_applicant_cooperativeOrgNr')
                            validationErrorKeys.push("billing.productrows.invalidcooperativeorgnumber");
                    });
                }
            }
        });
    }
}