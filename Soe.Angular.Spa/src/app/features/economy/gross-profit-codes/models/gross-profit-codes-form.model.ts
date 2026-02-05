import {
  SoeFormGroup,
  SoeNumberFormControl,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { GrossProfitCodeDTO } from './gross-profit-codes.model';

interface IGrossProfitCodesForm {
  validationHandler: ValidationHandler;
  element: GrossProfitCodeDTO | undefined;
}
export class GrossProfitCodesForm extends SoeFormGroup {
  constructor({ validationHandler, element }: IGrossProfitCodesForm) {
    super(validationHandler, {
      grossProfitCodeId: new SoeTextFormControl(
        element?.grossProfitCodeId || 0,
        {
          isIdField: true,
        }
      ),
      name: new SoeTextFormControl(
        element?.name || '',
        { isNameField: true, required: true, maxLength: 50, minLength: 1 },
        'common.name'
      ),
      code: new SoeNumberFormControl(
        element?.code || 0,
        { required: true },
        'common.code'
      ),
      description: new SoeTextFormControl(
        element?.description || '',
        { maxLength: 50 },
        'common.description'
      ),
      accountYearId: new SoeSelectFormControl(
        element?.accountYearId || undefined
      ),
      accountDimId: new SoeSelectFormControl(
        element?.accountDimId || undefined
      ),
      accountId: new SoeSelectFormControl(element?.accountId || undefined),
      openingBalance: new SoeNumberFormControl(
        element?.openingBalance || 100.0,
        { required: true },
        'economy.accounting.grossprofitcode.openingbalance'
      ),
      period1: new SoeNumberFormControl(
        element?.period1 || 0.0,
        {},
        'economy.accounting.grossprofitcode.period1'
      ),
      period2: new SoeNumberFormControl(
        element?.period2 || 0.0,
        {},
        'economy.accounting.grossprofitcode.period2'
      ),
      period3: new SoeNumberFormControl(
        element?.period3 || 0.0,
        {},
        'economy.accounting.grossprofitcode.period3'
      ),
      period4: new SoeNumberFormControl(
        element?.period4 || 0.0,
        {},
        'economy.accounting.grossprofitcode.period4'
      ),
      period5: new SoeNumberFormControl(
        element?.period5 || 0.0,
        {},
        'economy.accounting.grossprofitcode.period5'
      ),
      period6: new SoeNumberFormControl(
        element?.period6 || 0.0,
        {},
        'economy.accounting.grossprofitcode.period6'
      ),
      period7: new SoeNumberFormControl(
        element?.period7 || 0.0,
        {},
        'economy.accounting.grossprofitcode.period7'
      ),
      period8: new SoeNumberFormControl(
        element?.period8 || 0.0,
        {},
        'economy.accounting.grossprofitcode.period8'
      ),
      period9: new SoeNumberFormControl(
        element?.period9 || 0.0,
        {},
        'economy.accounting.grossprofitcode.period9'
      ),
      period10: new SoeNumberFormControl(
        element?.period10 || 0.0,
        {},
        'economy.accounting.grossprofitcode.period10'
      ),
      period11: new SoeNumberFormControl(
        element?.period11 || 0.0,
        {},
        'economy.accounting.grossprofitcode.period11'
      ),
      period12: new SoeNumberFormControl(
        element?.period12 || 0.0,
        {},
        'economy.accounting.grossprofitcode.period12'
      ),
    });
  }

  get grossProfitCodeId(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.grossProfitCodeId;
  }
  get name(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.name;
  }
  get code(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.code;
  }
  get description(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.description;
  }
  get accountYearId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.accountYearId;
  }
  get accountDimId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.accountDimId;
  }
  get accountId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.accountId;
  }
  get openingBalance(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.openingBalance;
  }
  get period1(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.period1;
  }
  get period2(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.period2;
  }
  get period3(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.period3;
  }
  get period4(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.period4;
  }
  get period5(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.period5;
  }
  get period6(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.period6;
  }
  get period7(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.period7;
  }
  get period8(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.period8;
  }
  get period9(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.period9;
  }
  get period10(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.period10;
  }
  get period11(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.period11;
  }
  get period12(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.period12;
  }
}
