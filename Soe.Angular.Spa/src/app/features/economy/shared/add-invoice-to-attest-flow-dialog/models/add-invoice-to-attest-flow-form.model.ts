import { ValidationHandler } from '@shared/handlers';
import { SoeFormGroup, SoeNumberFormControl, SoeTextFormControl, SoeCheckboxFormControl } from '@shared/extensions';

interface IAddInvoiceToAttestFlowForm {
  validationHandler: ValidationHandler;
}

export class AddInvoiceToAttestFlowForm extends SoeFormGroup {
  constructor({ validationHandler }: IAddInvoiceToAttestFlowForm) {
    super(validationHandler, {
      attestWorkFlowHeadId: new SoeNumberFormControl(
        0,
        { 
          required: true,
          zeroNotAllowed: true, 
        },
        'economy.supplier.attestgroup.attestgroup'
      ),
      attestWorkFlowTemplateHeadId: new SoeNumberFormControl(
        0,
        {},
        'economy.supplier.attestgroup.choosetemplate'
      ),
      roleOrUser: new SoeNumberFormControl(
        0,
        {},
        'economy.supplier.attestgroup.role_user'
      ),
      numberOfInvoicesText: new SoeTextFormControl(
        '',
        {
          disabled: true,
        },
        'economy.supplier.invoice.nrofinvoices'
      ),
      adminText: new SoeTextFormControl(
        '',
        {},
        'economy.supplier.invoice.attestflowadminmessage'
      ),
      sendMessage: new SoeCheckboxFormControl(
        false,
        {},
        'economy.supplier.invoice.sendattestmessage'
      ),
    });
  }

  get attestWorkFlowHeadId(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.attestWorkFlowHeadId;
  }

  get attestWorkFlowTemplateHeadId(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.attestWorkFlowTemplateHeadId;
  }

  get roleOrUser(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.roleOrUser;
  }

  get numberOfInvoicesText(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.numberOfInvoicesText;
  }

  get adminText(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.adminText;
  }

  get sendMessage(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.sendMessage;
  }
}

