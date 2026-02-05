import { ValidationHandler } from "@shared/handlers";
import { SoeCheckboxFormControl, SoeFormGroup, SoeTextFormControl } from "@shared/extensions";
import { ICustomerUserDTO } from "@shared/models/generated-interfaces/SOECompModelDTOs";

interface IOriginUserForm {
    validationHandler: ValidationHandler;
    element: ICustomerUserDTO | undefined;
}

export class CustomerOriginUserForm extends SoeFormGroup {
    constructor({ validationHandler, element }: IOriginUserForm) {
        super(validationHandler, {
            userId: new SoeTextFormControl(element?.userId || 0),
            name: new SoeTextFormControl(element?.name || ''),
            main: new SoeCheckboxFormControl(element?.main || false),
        });
    }

    get originUserId(): SoeTextFormControl {
        return <SoeTextFormControl>this.controls.originUserId;
      }
      get userId(): SoeTextFormControl {
        return <SoeTextFormControl>this.controls.userId;
      }
      get name(): SoeTextFormControl {
        return <SoeTextFormControl>this.controls.name;
      }
      get main(): SoeCheckboxFormControl {
        return <SoeCheckboxFormControl>this.controls.main;
      }
      get isReady(): SoeCheckboxFormControl {
        return <SoeCheckboxFormControl>this.controls.isReady;
      }
}