import {
  SoeCheckboxFormControl,
  SoeFormGroup,
  SoeNumberFormControl,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { CompanyGroupAdministrationDTO } from './company-group-administration.model';
import { SoeConfigUtil } from '@shared/util/soeconfig-util';

interface ICompanyGroupAdministrationForm {
  validationHandler: ValidationHandler;
  element: CompanyGroupAdministrationDTO | undefined;
}

export class CompanyGroupAdministrationForm extends SoeFormGroup {
  constructor({ validationHandler, element }: ICompanyGroupAdministrationForm) {
    super(validationHandler, {
      companyGroupAdministrationId: new SoeTextFormControl(
        element?.companyGroupAdministrationId || 0,
        {
          isIdField: true,
        }
      ),
      childActorCompanyId: new SoeSelectFormControl(
        element?.childActorCompanyId || 0,
        {
          required: true,
          zeroNotAllowed: true,
        },
        'economy.accounting.companygroup.company'
      ),
      childActorCompanyName: new SoeTextFormControl(
        element?.childActorCompanyName || '',
        {
          isNameField: true,
        }
      ),
      conversionfactor: new SoeNumberFormControl(
        element?.conversionfactor || 1,
        {
          minValue: 0,
          decimals: 4,
          required: true,
        },
        'economy.accounting.companygroup.conversionfactor'
      ),
      companyGroupMappingHeadId: new SoeSelectFormControl(
        element?.companyGroupMappingHeadId || 0,
        {
          required: true,
          zeroNotAllowed: true,
        },
        'economy.accounting.companygroup.mapping'
      ),
      note: new SoeTextFormControl(element?.note || ''),

      matchInternalAccountOnNr: new SoeCheckboxFormControl(
        element?.matchInternalAccountOnNr
      ),

      groupCompanyActorCompanyId: new SoeNumberFormControl(
        element?.groupCompanyActorCompanyId || SoeConfigUtil.actorCompanyId
      ),
    });
  }

  get companyGroupAdministrationId(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.companyGroupAdministrationId;
  }

  get childActorCompanyId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.childActorCompanyId;
  }

  get conversionfactor(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.conversionfactor;
  }

  get companyGroupMappingHeadId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.companyGroupMappingHeadId;
  }

  get note(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.note;
  }

  get matchInternalAccountOnNr() {
    return <SoeCheckboxFormControl>this.controls.matchInternalAccountOnNr;
  }

  get groupCompanyActorCompanyId() {
    return <SoeNumberFormControl>this.controls.groupCompanyActorCompanyId;
  }
}
