import { ITranslationService } from "../../../../../Core/Services/TranslationService";
import { IMessagingService } from "../../../../../Core/Services/MessagingService";
import { IUrlHelperService } from "../../../../../Core/Services/UrlHelperService";
import { IInvoiceService } from "../../../../../Shared/Billing/Invoices/InvoiceService";
import { HouseholdTaxDeductionApplicantDTO } from "../../../../../Common/Models/HouseholdTaxDeductionApplicantDTO";

export class EditHouseholdDataController {
    // Terms
    private terms: any;

    // Search
    private address = "";
    private postalCode = "";
    private postalAddress = "";
    private country = "";

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
        private row: HouseholdTaxDeductionApplicantDTO,
    ) {

    }

    buttonCancelClick() {
        this.$uibModalInstance.close(null);
    }

    buttonOkClick() {
        this.invoiceService.saveHouseHoldTaxRowForEdit(this.row).then((result) => {
            this.$uibModalInstance.close({ success: result.success });
        });
    }
}
