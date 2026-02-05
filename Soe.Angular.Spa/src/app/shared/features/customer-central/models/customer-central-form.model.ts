import { SoeFormGroup, SoeTextFormControl } from "@shared/extensions";
import { ValidationHandler } from "@shared/handlers";
import { CustomerCentralDTO } from "./customer-central.model";

interface ICustomerCentralForm {
    validationHandler: ValidationHandler;
    element: CustomerCentralDTO;
}
export class CustomerCentralForm extends SoeFormGroup {
    thisValidationHandler: ValidationHandler;

    constructor({ validationHandler, element }: ICustomerCentralForm) {
        super(validationHandler, {
            customerId: new SoeTextFormControl(element?.actorCustomerId || 0, { isIdField: true }),
            customerNr: new SoeTextFormControl(element?.customerNr || 0, {}),
            name: new SoeTextFormControl(element?.name || '', { isNameField: true }),
            customer: new SoeTextFormControl((element?.customerNr || '') + ' ' + (element?.name || ''), {}),
            orgNr: new SoeTextFormControl(element?.orgNr || ''),
            blockOrder: new SoeTextFormControl(element?.blockOrder || ''),
            invoiceDeliveryType: new SoeTextFormControl(element?.invoiceDeliveryType || undefined),
            note: new SoeTextFormControl(element?.note || ''),
            billingAddress: new SoeTextFormControl(element?.billingAddress || ''),
            deliveryAddress: new SoeTextFormControl(element?.deliveryAddress || ''),
            phoneNumber: new SoeTextFormControl(element?.phoneNumber || ''),
            blockNote: new SoeTextFormControl(element?.blockNote || ''),
            blockOrderString: new SoeTextFormControl(element?.blockOrderString || ''),
            categoryString: new SoeTextFormControl(element?.categoryString || ''),
            invoiceDeliveryTypeString: new SoeTextFormControl(element?.invoiceDeliveryTypeString || ''),
        });

        this.thisValidationHandler = validationHandler;
    }

    get customerId(): SoeTextFormControl {
        return <SoeTextFormControl>this.controls.customerId;
    }

    get customerNr(): SoeTextFormControl {
        return <SoeTextFormControl>this.controls.customerNr;
    }

    get name(): SoeTextFormControl {
        return <SoeTextFormControl>this.controls.name;
    }

    get customer(): SoeTextFormControl {
        return <SoeTextFormControl>this.controls.customer;
    }

    get orgNr(): SoeTextFormControl {
        return <SoeTextFormControl>this.controls.orgNr;
    }

    get blockOrder(): SoeTextFormControl {
        return <SoeTextFormControl>this.controls.blockOrder;
    }

    get invoiceDeliveryType(): SoeTextFormControl {
        return <SoeTextFormControl>this.controls.invoiceDeliveryType;
    }

    get note(): SoeTextFormControl {
        return <SoeTextFormControl>this.controls.note;
    }
    
    get billingAddress(): SoeTextFormControl {
        return <SoeTextFormControl>this.controls.billingAddress;
    }
    
    get deliveryAddress(): SoeTextFormControl {
        return <SoeTextFormControl>this.controls.deliveryAddress;
    }
    
    get phoneNumber(): SoeTextFormControl {
        return <SoeTextFormControl>this.controls.phoneNumber;
    }
    
    get blockNote(): SoeTextFormControl {
        return <SoeTextFormControl>this.controls.blockNote;
    }
    
    get blockOrderString(): SoeTextFormControl {
        return <SoeTextFormControl>this.controls.blockOrderString;
    }
    
    get categoryString(): SoeTextFormControl {
        return <SoeTextFormControl>this.controls.categoryString;
    }
    
    get invoiceDeliveryTypeString(): SoeTextFormControl {
        return <SoeTextFormControl>this.controls.invoiceDeliveryTypeString;
    }

}