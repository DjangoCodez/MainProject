import { Component } from '@angular/core';
import { EmailTemplatesGridComponent } from '../email-templates-grid/email-templates-grid.component';
import { EmailTemplateEditComponent } from '../email-template-edit/email-template-edit.component';
import { EmailTemplateForm } from '../../models/email-template-form.model';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';

@Component({
  selector: 'soe-email-templates',
  templateUrl:
    '../../../../../shared/ui-components/tab/multi-tab-wrapper/multi-tab-wrapper-template.html',
  standalone: false,
})
export class EmailTemplatesComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: EmailTemplatesGridComponent,
      editComponent: EmailTemplateEditComponent,
      FormClass: EmailTemplateForm,
      gridTabLabel: 'billing.invoices.emailtemplates',
      editTabLabel: 'billing.invoices.emailtemplate',
      createTabLabel: 'billing.invoices.emailtemplate.new',
    },
  ];
}
