import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { IMessagingService } from "../../../../Core/Services/MessagingService";
import { ICoreService } from "../../../../Core/Services/CoreService";
import { IUrlHelperService } from "../../../../Core/Services/UrlHelperService";

export class EditDeliveryAddressController {
    // Terms
    private terms: any;

    // Search
    private _showAsAddress: boolean = false;
    private name: string = "";
    private co: string = "";
    private address: string = "";
    private postalCode: string = "";
    private postalAddress: string = "";
    private country: string = "";

    get showAsAddress(): boolean {
        return this._showAsAddress;
    }

    set showAsAddress(value: boolean) {
        if (value !== this._showAsAddress) {
            this._showAsAddress = value;
            this.showAsAddressChanged();
        }
    }

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
        private deliveryAddress: string,
        private isFinvoiceCustomer: boolean, 
        private isLocked: boolean
    ) {
        this.$q.all([
            this.loadTerms()]).then(() => {
                this.populateDeliveryAddress();
            });
    }

    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
            "billing.order.deliveryaddress",
            "common.contactaddresses.addressrow.name",
            "common.contactaddresses.addressrow.address",
            "common.contactaddresses.addressrow.postalcode",
            "common.contactaddresses.addressrow.postaladdress",
            "billing.dialogs.editdeliveryaddress.showasaddress"
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
        });
    }

    private populateDeliveryAddress() {
        if (this.isFinvoiceCustomer) {
            this.showAsAddress = true;
        }
    }

    private showAsAddressChanged() {
        if (this.showAsAddress == true)
            this.textToAddress();
        else
            this.addressToText();
    }

    private textToAddress() {
        var address = this.deliveryAddress.split(/\r\n|\r|\n/g);
        if (address.length === 5) {
            for (var i = 0; i < address.length; i++) {
                switch (i) {
                    case 0:
                        this.name = address[0];
                        break;
                    case 1:
                        this.co = address[1];
                        break;
                    case 2:
                        this.address = address[2];
                        break;
                    /*case 3:                           // Not working due to merging zipcode and city in order for it to show as an address on one row.
                        this.postalCode = address[3];
                        break;
                    case 4:
                        this.postalAddress = address[4];
                        break;*/;
                    case 3:
                        this.postalAddress = address[3];
                        break;
                    case 4:
                        this.country = address[4];
                        break;
                }
            }
        }
        else {
            for (var i = 0; i < address.length; i++) {
                switch (i) {
                    case 0:
                        this.name = address[0];
                        break;
                    case 1:
                        this.address = address[1];
                        break;
                    /*case 2:                           // Not working due to merging zipcode and city in order for it to show as an address on one row.
                        this.postalCode = address[2];
                        break;
                    case 3:
                        this.postalAddress = address[3];
                        break;*/
                    case 2:
                        this.postalAddress = address[2];
                        break;
                    case 3:
                        this.country = address[3];
                        break;
                }
            }
        }

        //this.deliveryAddress = "";
    }

    private addressToText() {

        var text: string = "";
        text += this.name;
        if(this.co)
            text += ("\n") + this.co;
        text += ("\n") + this.address;
        if (this.postalCode)
            text += ("\n") + this.postalCode;
        text += ("\n") + this.postalAddress;
        if (this.country !== "") {
            text += ("\n") + this.country;
        }
        this.deliveryAddress = text;

        this.name = "";
        this.co = "";
        this.address = "";
        this.postalCode = "";
        this.postalAddress = "";
        this.country = "";
    }

    buttonCancelClick() {
        this.close(null);
    }

    buttonOkClick() {
        if (this.showAsAddress == true)
            this.addressToText();
        this.close(this.deliveryAddress);
    }

    close(invoiceHeadText: string) {
        this.$uibModalInstance.close({ deliveryAddress: invoiceHeadText });
    }
}
