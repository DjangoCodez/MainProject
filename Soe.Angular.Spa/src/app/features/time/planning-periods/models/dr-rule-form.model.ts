import {
  SoeFormGroup,
  SoeNumberFormControl,
  SoeSelectFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { IPayrollProductDistributionRuleDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';

interface IDistributionRulesForm {
  validationHandler: ValidationHandler;
  element: IPayrollProductDistributionRuleDTO | undefined;
}
export class DistributionRulesForm extends SoeFormGroup {
  thisValidationHandler: ValidationHandler | undefined;
  constructor({ validationHandler, element }: IDistributionRulesForm) {
    super(validationHandler, {
      payrollProductDistributionRuleId: new SoeNumberFormControl(
        element?.payrollProductDistributionRuleId || 0,
        { isIdField: true }
      ),
      payrollProductDistributionRuleHeadId: new SoeNumberFormControl(
        element?.payrollProductDistributionRuleHeadId || 0
      ),
      actorCompanyId: new SoeNumberFormControl(element?.actorCompanyId),
      payrollProductId: new SoeSelectFormControl(
        element?.payrollProductId || null,
        { required: true },
        'common.payrollproduct'
      ),
      type: new SoeNumberFormControl(element?.type || 1, {}),

      start: new SoeNumberFormControl(
        element?.start,
        { required: true },
        'common.start'
      ),
      stop: new SoeNumberFormControl(
        element?.stop,
        { required: true },
        'common.stop'
      ),
    });
    this.thisValidationHandler = validationHandler;
  }
  get start(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.start;
  }
  get stop(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.stop;
  }
  get payrollProductId(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.payrollProductId;
  }
}
