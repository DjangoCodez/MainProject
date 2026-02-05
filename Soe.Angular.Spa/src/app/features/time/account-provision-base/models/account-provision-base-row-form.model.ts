
import { SoeFormGroup, SoeSelectFormControl} from "@shared/extensions";
import { ValidationHandler } from "@shared/handlers";
import { IAccountProvisionBaseDTO } from "@shared/models/generated-interfaces/SOECompModelDTOs";

interface IAccountProvisionBaseRowForm {
    validationHandler: ValidationHandler;
    element: IAccountProvisionBaseDTO | undefined;
  }
  export class AccountProvisionBaseRowForm extends SoeFormGroup {
   
    constructor({ validationHandler, element }: IAccountProvisionBaseRowForm) {
      super(validationHandler, {
        accountId: new SoeSelectFormControl(
          element?.accountId || 0,
          {},
          ''
        ),
        accountName: new SoeSelectFormControl(
          element?.accountName || '',
          {},
          ''
        ),
        accountNr: new SoeSelectFormControl(
          element?.accountNr || '',
          {},
          ''
        ),
        isLocked: new SoeSelectFormControl(
          element?.isLocked || false,
          {},
          ''
        ),
        isModified: new SoeSelectFormControl(
          element?.isModified || false,
          {},
          ''
        ),
        period1Value: new SoeSelectFormControl(
          element?.period1Value || 0,
          {},
          ''
        ),
        period2Value: new SoeSelectFormControl(
          element?.period2Value || 0,
          {},
          ''
        ),
        period3Value: new SoeSelectFormControl(
          element?.period3Value || 0,
          {},
          ''
        ),
        period4Value: new SoeSelectFormControl(
          element?.period4Value || 0,
          {},
          ''
        ),
        period5Value: new SoeSelectFormControl(
          element?.period5Value || 0,
          {},
          ''
        ),
        period6Value: new SoeSelectFormControl(
          element?.period6Value || 0,
          {},
          ''
        ),
        period7Value: new SoeSelectFormControl(
          element?.period7Value || 0,
          {},
          ''
        ),
        period8Value: new SoeSelectFormControl(
          element?.period8Value || 0,
          {},
          ''
        ),
        period9Value: new SoeSelectFormControl(
          element?.period9Value || 0,
          {},
          ''
        ),
        period10Value: new SoeSelectFormControl(
          element?.period10Value || 0,
          {},
          ''
        ),
        period11Value: new SoeSelectFormControl(
          element?.period11Value || 0,
          {},
          ''
        ),
        period12Value: new SoeSelectFormControl(
          element?.period12Value || 0,
          {},
          '',
        ),
        timePeriodAccountValueId: new SoeSelectFormControl(
          element?.timePeriodAccountValueId || 0,
          {},
          ''
        ),
        timePeriodId: new SoeSelectFormControl(
          element?.timePeriodId || 0,
          {},
          ''
        ),
      });

    }
 
  }    
 