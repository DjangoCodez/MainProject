import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { IMessagingService } from "../../../../Core/Services/MessagingService";
import { ICoreService } from "../../../../Core/Services/CoreService";
import { IUrlHelperService } from "../../../../Core/Services/UrlHelperService";
import { StringUtility } from "../../../../Util/StringUtility";
import { IFocusService } from "../../../../Core/Services/focusservice";

export class OneTimeCustomerController {
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
        private coreService: ICoreService,
        private urlHelperService: IUrlHelperService,
        uiGridConstants: uiGrid.IUiGridConstants,
        private $q: ng.IQService,
        private name: string,
        private deliveryAddress: string,
        private phone: string,
        private email: string,
        private isFinvoiceCustomer: boolean, 
        private isLocked: boolean, 
        private isEmailMode: boolean,
        private focusService: IFocusService,
    ) {
        if(!this.isEmailMode)
            this.textToAddress();

        this.focusService.focusByName("ctrl_name");

        //Auto focus Ok button after email
        document.addEventListener('focus', () => {
                if (document.activeElement.id === "ctrl_email" && !this.isLocked) {
                    document.addEventListener(
                        'keydown',
                        (keyEvent) => {
                            if (keyEvent.key === 'Tab') {
                                keyEvent.preventDefault();
                               const saveButton = angular.element('.btn-primary');
                                if ((saveButton && saveButton.length))
                                    saveButton.focus();                                
                            }
                        },
                        { once: true }
                    );
                }
            },
            true
        );
    }

    private textToAddress() {
        const address = this.deliveryAddress.split(/\r\n|\r|\n/g);
        for (let i = 0; i < address.length; i++) {
            switch (i) {
                case 0:
                    if(StringUtility.isEmpty(this.name))
                        this.name = address[0];
                    break;
                case 1:
                    this.address = address[1];
                    break;
                case 2:
                    this.postalCode = address[2];
                    break;
                case 3:
                    this.postalAddress = address[3];
                    break;
                case 4:
                    this.country = address[4];
                    break;
            }
        }

        //this.deliveryAddress = "";
    }

    private addressToText(): string {

        let text = "";
        text += this.name;
        text += ("\n") + this.address;
        text += ("\n") + this.postalCode;
        text += ("\n") + this.postalAddress;
        if (this.country !== "") {
            text += ("\n") + this.country;
        }
        this.deliveryAddress = text;

        this.name = "";
        this.address = "";
        this.postalCode = "";
        this.postalAddress = "";
        this.country = "";

        return text;
    }

    buttonCancelClick() {
        this.$uibModalInstance.close(null);
    }

    buttonOkClick() {
        if(this.isEmailMode)
            this.$uibModalInstance.close({ email: this.email });
        else
            this.$uibModalInstance.close({ name: this.name, address: this.addressToText(), phone: this.phone, email: this.email });
    }
}
