import { ITranslationService } from "../../../../../Core/Services/TranslationService";
import { IMessagingService } from "../../../../../Core/Services/MessagingService";
import { IUrlHelperService } from "../../../../../Core/Services/UrlHelperService";
import { IInvoiceService } from "../../../../../Shared/Billing/Invoices/InvoiceService";
import { HouseholdTaxDeductionFileRowDTO } from "../../../../../Common/Models/HouseholdTaxDeductionFileRowDTOs";

export class EditHouseholdFileController {
    private loading: boolean = true;

    //@ngInject
    constructor(
        private $uibModalInstance,
        private $window: ng.IWindowService,
        private $timeout: ng.ITimeoutService,
        private translationService: ITranslationService,
        private messagingService: IMessagingService,
        private invoiceService: IInvoiceService,
        private urlHelperService: IUrlHelperService,
        private $q: ng.IQService,
        private rows: HouseholdTaxDeductionFileRowDTO[],
    ) {

    }

    getAccordionLabelValue(index: number): string {
        return this.rows[index].invoiceNr + " - " + this.rows[index].socialSecNr + " " + this.rows[index].name + " " + (this.rows[index].property && this.rows[index].property.length > 0 ? this.rows[index].property : this.rows[index].apartmentNr);
    }

    hideLabel(index: number): boolean {
        return index > 0;
    }

    buttonCancelClick() {
        this.$uibModalInstance.close(null);
    }

    buttonOkClick() {
        this.$uibModalInstance.close(this.rows);
    }
}
