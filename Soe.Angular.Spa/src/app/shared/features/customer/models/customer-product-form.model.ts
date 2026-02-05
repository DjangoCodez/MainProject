import { ValidationHandler } from "@shared/handlers";
import { SoeCheckboxFormControl, SoeFormGroup, SoeNumberFormControl, SoeTextFormControl } from "@shared/extensions";
import { CustomerProductPriceSmallDTO } from "./customer-product.model";

interface ICustomerProductForm {
    validationHandler: ValidationHandler;
    element: CustomerProductPriceSmallDTO | undefined;
}

export class CustomerProductForm extends SoeFormGroup {
   
    constructor({ validationHandler, element }: ICustomerProductForm) {
        super(validationHandler, {
            productRowId: new SoeTextFormControl( element?.productRowId || 0, { isIdField: true}),
            customerProductId: new SoeTextFormControl(element?.customerProductId || 0),
            productId: new SoeTextFormControl(element?.productId || 0),
            number: new SoeTextFormControl(element?.number || 0, { isIdField: true }),
            number2: new SoeTextFormControl(element?.number || ''),
            name: new SoeTextFormControl(element?.name || ''),
            price: new SoeNumberFormControl(element?.price || undefined),
            isDelete: new SoeCheckboxFormControl(element?.isDelete || false),
        });
        
    }

    get productRowId(): SoeTextFormControl {
        return <SoeTextFormControl>this.controls.productRowId;
    }

    get customerProductId(): SoeTextFormControl {
        return <SoeTextFormControl>this.controls.customerProductId;
    }

    get productId(): SoeTextFormControl {
        return <SoeTextFormControl>this.controls.productId;
    }

    get number(): SoeTextFormControl {
        return <SoeTextFormControl>this.controls.number;
    }

    get price(): SoeNumberFormControl {
        return <SoeNumberFormControl>this.controls.price;
    }

    get isDelete(): SoeCheckboxFormControl {
        return <SoeCheckboxFormControl>this.controls.isDelete;
    }
}