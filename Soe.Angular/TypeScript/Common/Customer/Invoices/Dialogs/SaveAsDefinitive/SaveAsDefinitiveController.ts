import { CustomerInvoiceGridButtonFunctions } from "../../../../../Util/Enumerations";

export class SaveAsDefinitiveController {
    // Properties
    private _mail: boolean = false;
    get Mail() {
        return this._mail;
    }
    set Mail(item: boolean) {
        this._mail = item;
        if (this._mail) {
            this.SendEInvoice = this.CreateEInvoice = false;
        }
    }

    private _sendEInvoice: boolean = false;
    get SendEInvoice() {
        return this._sendEInvoice;
    }
    set SendEInvoice(item: boolean) {
        this._sendEInvoice = item;
        if (this._sendEInvoice) {
            this.Mail = this.CreateEInvoice = false;
        }
    }

    private _createInvoice: boolean = false;
    get CreateEInvoice() {
        return this._createInvoice;
    }
    set CreateEInvoice(item: boolean) {
        this._createInvoice = item;
        if (this._createInvoice) {
            this.Mail = this.SendEInvoice = false;
        }
    }

    private _print: boolean = false;
    get Print() {
        return this._print;
    }
    set Print(item: boolean) {
        this._print = item;
    }

    
    //@ngInject
    constructor(private $uibModalInstance, private infoText: string, private hasSendEInvoicePermission: boolean, private hasDownloadEInvoicePermission, private hasReportPermission: boolean) {
        
    }

    buttonOkClick() {
        let val: CustomerInvoiceGridButtonFunctions = CustomerInvoiceGridButtonFunctions.SaveAsDefinitiv;
        let print: boolean = false;
        if (this.Mail) {
            val = CustomerInvoiceGridButtonFunctions.SaveAsDefinitiveAndSendAsEmail;
            print = this.Print;
        }
        else if (this.CreateEInvoice) {
            val = CustomerInvoiceGridButtonFunctions.SaveAsDefinitiveAndCreateEInvoice;
            print = this.Print;
        }
        else if (this.SendEInvoice) {
            val = CustomerInvoiceGridButtonFunctions.SaveAsDefinitiveAndSendEInvoice;
            print = this.Print;
        }
        else if (this.Print) {
            val = CustomerInvoiceGridButtonFunctions.SaveAsDefinitiveAndPrint;
        }
        this.$uibModalInstance.close({ option: val, print: print });
    }

    buttonCancelClick() {
        this.$uibModalInstance.dismiss('cancel');
    }
}