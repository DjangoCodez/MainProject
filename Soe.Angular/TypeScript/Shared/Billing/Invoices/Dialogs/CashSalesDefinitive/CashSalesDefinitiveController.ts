import { ITranslationService } from "../../../../../Core/Services/TranslationService";
import { IMessagingService } from "../../../../../Core/Services/MessagingService";
import { ICoreService } from "../../../../../Core/Services/CoreService";
import { IUrlHelperService } from "../../../../../Core/Services/UrlHelperService";
import { StringUtility } from "../../../../../Util/StringUtility";
import { ICommonCustomerService } from "../../../../../Common/Customer/CommonCustomerService";
import { Validators } from "../../../../../Core/Validators/Validators";

export class CashSalesDefinitiveController {

    // Values
    selectedEmail: any;
    emailAddress: string;

    // Flags
    printEnabled = false;
    emailEnabled = false;
    showEmailTextbox = false;
    showEmailError = false;

    //@ngInject
    constructor(
        private $uibModalInstance,
        private $window: ng.IWindowService,
        private $timeout: ng.ITimeoutService,
        private translationService: ITranslationService,
        private messagingService: IMessagingService,
        private urlHelperService: IUrlHelperService,
        private $q: ng.IQService,
        private commonCustomerService: ICommonCustomerService,
        private invoiceId: number,
        private emails: any[],
        private contactEComId: number,
    ) {
        this.selectedEmail = _.find(this.emails, (e) => e.id === contactEComId);
        this.enableDisable();
    }

    private showEmailChanged() {
        this.$timeout(() => {
            if (!this.showEmailTextbox) {
                this.showEmailError = false;
                this.emailAddress = "";
            }
        });
    }

    private enableDisable() { 
    // Enable/Disable email
        this.emailEnabled = (this.showEmailTextbox ? !this.showEmailError  : (this.selectedEmail && !StringUtility.isEmpty(this.selectedEmail.name)));
    }

    private validateEmail() {
        this.$timeout(() => {
            if (!Validators.isValidEmailAddress(this.emailAddress)) 
                this.showEmailError = true;
            else 
                this.showEmailError = false;

            this.enableDisable();
        });
    }

    buttonCancelClick() {
        this.$uibModalInstance.close(null);
    }

    buttonOkClick(sendEmail: boolean) {
        let email = "";
        if (this.showEmailTextbox)
            email = this.emailAddress && sendEmail ? this.emailAddress : "";
        else
            email = this.selectedEmail && sendEmail ? this.selectedEmail.name : "";

        this.$uibModalInstance.close({ invoiceId: this.invoiceId, email: email, sendEmail: sendEmail });
    }
}
