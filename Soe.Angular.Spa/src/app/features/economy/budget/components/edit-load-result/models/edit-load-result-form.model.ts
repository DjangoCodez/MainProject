import { ValidationHandler } from "@shared/handlers";
import { EditLoadResultDTO } from "./edit-load-result.model";
import { SoeCheckboxFormControl, SoeFormGroup } from "@shared/extensions";

interface IEditLoadResultForm {
    validationHandler: ValidationHandler;
    element: EditLoadResultDTO
}

export class EditLoadResultForm extends SoeFormGroup {
    constructor({ validationHandler, element }: IEditLoadResultForm) {
        super(validationHandler, {
            useDim2: new SoeCheckboxFormControl(element?.useDim2 || false),
            useDim3: new SoeCheckboxFormControl(element?.useDim3 || false),
        });
    }

    get useDim2(): SoeCheckboxFormControl {
        return <SoeCheckboxFormControl>this.controls.useDim2;
    }

    get useDim3(): SoeCheckboxFormControl {
        return <SoeCheckboxFormControl>this.controls.useDim3;
    }
}