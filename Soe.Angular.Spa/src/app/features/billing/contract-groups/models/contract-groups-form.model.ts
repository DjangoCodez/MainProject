import {
  SoeFormGroup,
  SoeNumberFormControl,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { ContractGroupDTO } from './contract-groups.model';

interface IContractGroupsForm {
  validationHandler: ValidationHandler;
  element: ContractGroupDTO | undefined;
}
export class ContractGroupsForm extends SoeFormGroup {
  constructor({ validationHandler, element }: IContractGroupsForm) {
    super(validationHandler, {
      contractGroupId: new SoeTextFormControl(element?.contractGroupId || 0, {
        isIdField: true,
      }),
      name: new SoeTextFormControl(
        element?.name || '',
        {
          isNameField: true,
          required: true,
          maxLength: 100,
        },
        'common.name'
      ),
      description: new SoeTextFormControl(element?.description || ''),
      period: new SoeSelectFormControl(
        element?.period || 0,
        {
          required: true,
          zeroNotAllowed: true,
        },
        'common.period'
      ),
      priceManagement: new SoeSelectFormControl(
        element?.priceManagement || 0,
        {
          required: true,
          zeroNotAllowed: true,
        },
        'billing.contract.contractgroups.pricemanagement'
      ),

      interval: new SoeNumberFormControl(
        element?.interval || 1,
        {
          required: true,
          zeroNotAllowed: true,
        },
        'billing.contract.contractgroups.interval'
      ),

      dayInMonth: new SoeNumberFormControl(element?.dayInMonth || 0, {
        decimals: 0,
      }),
      invoiceText: new SoeTextFormControl(element?.invoiceText || ''),
      invoiceTextRow: new SoeTextFormControl(element?.invoiceTextRow || ''),
    });
  }

  get contractGroupId(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.contractGroupId;
  }

  get name(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.name;
  }

  get description(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.description;
  }
}
