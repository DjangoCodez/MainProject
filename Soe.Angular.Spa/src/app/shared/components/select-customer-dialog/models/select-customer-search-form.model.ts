import { ValidationHandler } from "@shared/handlers";
import { SelectCustomerGridFormDTO } from "./select-customer-dialog.model";
import { SoeFormGroup, SoeTextFormControl } from "@shared/extensions";


interface ISelectCustomerSearchForm {
    validationHandler: ValidationHandler;
    element: SelectCustomerGridFormDTO
}

export class SelectCustomerSearchForm extends SoeFormGroup {
    constructor({ validationHandler, element }: ISelectCustomerSearchForm) {
        super(validationHandler, {
            customerId: new SoeTextFormControl(element?.customerId || ''),
        });
    }

    get customerId(): SoeTextFormControl {
        return <SoeTextFormControl>this.controls.customerId;
    }
    
}