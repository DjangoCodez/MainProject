import { ITranslationService } from "../../../../../Core/Services/TranslationService";
import { IMessagingService } from "../../../../../Core/Services/MessagingService";
import { ICoreService } from "../../../../../Core/Services/CoreService";
import { IUrlHelperService } from "../../../../../Core/Services/UrlHelperService";
import { StringUtility } from "../../../../../Util/StringUtility";
import { ICommonCustomerService } from "../../../../../Common/Customer/CommonCustomerService";
import { SoeInvoiceMatchingType, SoeOriginType, TermGroup_BillingType, TermGroup_Languages } from "../../../../../Util/CommonEnumerations";
import { NumberUtility } from "../../../../../Util/NumberUtility";
import { Validators } from "../../../../../Core/Validators/Validators";
import { CoreUtility } from "../../../../../Util/CoreUtility";

export class CashSalesController {
    paymentMethods: any[];
    matchCodes: any[];
    payments: any[];

    // Amounts
    remainingAmount: number;
    cashAmount: number;
    changeAmount: number;
    amountPreRounding: number;

    // Values
    selectedMatchCode: any;
    selectedEmail: any;
    emailAddress: string;

    // Flags
    restCodeEnabled = false;
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
        private billingType: TermGroup_BillingType,
        private totalAmount: number,
        private emails: any[],
        private contactEComId: number,
    ) {
        this.payments = [];
        this.$q.all([
            this.loadPaymentMethods(),
            this.loadMatchCodes()]).then(() => {
                this.selectedEmail = _.find(this.emails, (e) => e.id === contactEComId);
                this.remainingAmount = this.totalAmount;
                this.cashAmount = this.totalAmount;
                this.changeAmount = 0;
            });
    }

    private loadPaymentMethods(): ng.IPromise<any> {
        return this.commonCustomerService.getPaymentMethods(SoeOriginType.CustomerPayment, false, false, false, true).then((x) => {
            this.paymentMethods = x;
            _.forEach(this.paymentMethods, (m) => {
                this.payments.push({ paymentMethodId: m.paymentMethodId, paymentMethodName: m.name, inUse: false, amountCurrency: 0, useRounding: m.useRoundingInCashSales });
            });
        });
    }

    private loadMatchCodes(): ng.IPromise<any> {
        return this.commonCustomerService.getMatchCodes(SoeInvoiceMatchingType.CustomerInvoiceMatching, true).then(x => {
            this.matchCodes = x;
            this.selectedMatchCode = this.matchCodes[0];
        });
    }

    private cashChanged() {
        this.$timeout(() => {
            // Set change
            this.changeAmount = (this.cashAmount - this.totalAmount);
        });
    }

    private amountChanged(row) {
        this.$timeout(() => {
            if ((this.billingType === TermGroup_BillingType.Debit && row.amountCurrency < 0) || (this.billingType === TermGroup_BillingType.Credit && row.amountCurrency > 0))
                row.amountCurrency = (row.amountCurrency * -1);

            this.calculateAmounts();
        });
    }

    private inUseChanged(row) {
        this.$timeout(() => {
            this.calculateAmounts(row);
        });
    }

    private matchCodeChanged() {
        this.$timeout(() => {
            this.enableDisable();
        });
    }

    private calculateAmounts(row = undefined) {
        let paidAmount = 0;
        let resetCashAmount = false;
        this.remainingAmount = 0;

        if (row) {
            if (row.inUse) {
                if (row.useRounding && !this.amountPreRounding && _.filter(this.payments, (p) => p.inUse && !p.useRounding).length === 0) {
                    this.amountPreRounding = this.totalAmount;
                    if (CoreUtility.sysCountryId === TermGroup_Languages.Finnish) {
                        this.totalAmount = Math.round(this.totalAmount * 20) / 20;
                    }
                    else {
                        this.totalAmount = Math.abs(this.totalAmount).round(0);
                    }
                }
                else if (this.amountPreRounding && !row.useRounding) {
                    this.totalAmount = this.amountPreRounding;
                    this.amountPreRounding = undefined;
                }

                // Summarize paid amount
                _.forEach(this.payments, (p) => {
                    paidAmount += NumberUtility.parseDecimal(p.amountCurrency);
                });

                // Set remaining after rounding
                this.remainingAmount = (this.totalAmount - paidAmount);

                // Set row amount
                row.amountCurrency = this.remainingAmount;
            }
            else {
                if (this.amountPreRounding && !_.find(this.payments, (p) => p.inUse && p.useRounding)) {
                    this.totalAmount = this.amountPreRounding;
                    this.amountPreRounding = undefined;
                }
                else if (!this.amountPreRounding && _.filter(this.payments, (p) => p.inUse && !p.useRounding).length === 0 && _.filter(this.payments, (p) => p.inUse && p.useRounding).length > 0) {
                    this.amountPreRounding = this.totalAmount;
                    if (CoreUtility.sysCountryId === TermGroup_Languages.Finnish) {
                        this.totalAmount = (Math.ceil(this.totalAmount * 20) / 20).round(2)
                    }
                    else {
                        this.totalAmount = Math.abs(this.totalAmount).round(0);
                    }
                }

                row.amountCurrency = 0;
            }
        }

        // reset
        paidAmount = 0;

        // Summarize paid amount
        _.forEach(this.payments, (p) => {
            paidAmount += NumberUtility.parseDecimal(p.amountCurrency);
        });

        // Cash
        //this.cashAmount = paidAmount;

        // Set remaining;
        this.remainingAmount = (this.totalAmount - paidAmount);

        // Set change
        this.changeAmount = (this.cashAmount - this.totalAmount);

        this.enableDisable();
    }

    private setRoundingAmount(row) {

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
        // Enable/Disable rest code
        this.restCodeEnabled = this.billingType === TermGroup_BillingType.Debit ? (this.remainingAmount < this.totalAmount && this.remainingAmount > 0) : (this.remainingAmount > this.totalAmount && this.remainingAmount < 0);

        // Enable/Disable print
        this.printEnabled = this.remainingAmount === 0 || (this.selectedMatchCode && this.selectedMatchCode.matchCodeId > 0);

        // Enable/Disable email
        this.emailEnabled = (this.remainingAmount === 0 || (this.selectedMatchCode && this.selectedMatchCode.matchCodeId > 0)) && (this.showEmailTextbox ? !this.showEmailError  : (this.selectedEmail && !StringUtility.isEmpty(this.selectedEmail.name)));
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

        this.$uibModalInstance.close({ invoiceId: this.invoiceId, payments: _.filter(this.payments, (p) => p.amountCurrency !== 0), remainingAmount: this.remainingAmount, matchCodeId: this.selectedMatchCode ? this.selectedMatchCode.matchCodeId : undefined, email: email, sendEmail: sendEmail, useRounding: this.amountPreRounding && this.amountPreRounding !== 0 });
    }
}
