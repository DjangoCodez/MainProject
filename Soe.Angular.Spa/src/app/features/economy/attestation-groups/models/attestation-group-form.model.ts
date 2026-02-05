import {
  SoeCheckboxFormControl,
  SoeFormGroup,
  SoeNumberFormControl,
  SoeRadioFormControl,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { AttestWorkFlowHeadDTO } from './attestation-groups.model';
import { ValidationHandler } from '@shared/handlers';
interface IAttestationGroupForm {
  validationHandler: ValidationHandler;
  element: AttestWorkFlowHeadDTO | undefined;
}

export class AttestationGroupForm extends SoeFormGroup {
  attestationGroupValidationHandler: ValidationHandler;
  constructor({ validationHandler, element }: IAttestationGroupForm) {
    super(validationHandler, {
      actorCompanyId: new SoeNumberFormControl(
        element?.actorCompanyId || 0,
        undefined
      ),
      attestWorkFlowHeadId: new SoeNumberFormControl(
        element?.attestWorkFlowHeadId || 0,
        { isIdField: true }
      ),
      attestGroupName: new SoeTextFormControl(
        element?.attestGroupName || '',
        {
          required: true,
          isNameField: true,
        },
        'common.name'
      ),

      attestGroupCode: new SoeTextFormControl(
        element?.attestGroupCode || '',
        {
          required: true,
        },
        'common.code'
      ),

      sendMessage: new SoeCheckboxFormControl(
        element?.sendMessage || false,
        undefined,
        'economy.supplier.invoice.sendattestmessage'
      ),

      attestWorkFlowTemplateHeadId: new SoeSelectFormControl(
        element?.attestWorkFlowTemplateHeadId || null,
        {
          required: true,
        },
        'economy.supplier.attestgroup.choosetemplate'
      ),
    });

    this.attestationGroupValidationHandler = validationHandler;
  }

  get actorCompanyId(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.actorCompanyId;
  }

  get attestWorkFlowHeadId(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.attestWorkFlowHeadId;
  }

  get attestGroupName(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.attestGroupName;
  }

  get attestGroupCode(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.attestGroupCode;
  }

  get isAttestGroup(): SoeRadioFormControl {
    return <SoeRadioFormControl>this.controls.isAttestGroup;
  }

  get sendMessage(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.sendMessage;
  }

  get attestWorkFlowTemplateHeadId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.attestWorkFlowTemplateHeadId;
  }
}
