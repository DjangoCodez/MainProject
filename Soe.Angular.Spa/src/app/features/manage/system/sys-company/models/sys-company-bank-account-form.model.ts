import { ValidationHandler } from '@shared/handlers';
import { SysCompanyBankAccountDTO } from '../../../models/sysCompany.model';
import {
  SoeFormGroup,
  SoeNumberFormControl,
  SoeTextFormControl,
} from '@shared/extensions';

interface ISysCompanyBankAccountDTO {
  validationHandler: ValidationHandler;
  element: SysCompanyBankAccountDTO | undefined;
}

export class SysCompanyBankAccountForm extends SoeFormGroup {
  constructor({ validationHandler, element }: ISysCompanyBankAccountDTO) {
    super(validationHandler, {
      sysCompanyBankAccountId: new SoeTextFormControl(
        element?.sysCompanyBankAccountId || 0,
        { isIdField: true }
      ),
      sysBankId: new SoeTextFormControl(
        element?.sysBankId || undefined,
        { required: true },
        'manage.system.syscompany.sysbank'
      ),
      sysCompanyId: new SoeTextFormControl(element?.sysCompanyId || 0),
      accountType: new SoeNumberFormControl(
        element?.accountType || undefined,
        { required: true },
        'manage.system.syscompany.accounttype'
      ),
      paymentNr: new SoeTextFormControl(
        element?.paymentNr || '',
        { required: true },
        'manage.system.syscompany.accountnr'
      ),
    });
  }

  get sysCompanyBankAccountId(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.sysCompanyBankAccountId;
  }

  get sysBankId(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.sysBankId;
  }

  get sysCompanyId(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.sysCompanyId;
  }

  get accountType(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.accountType;
  }

  get paymentNr(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.paymentNr;
  }
}
