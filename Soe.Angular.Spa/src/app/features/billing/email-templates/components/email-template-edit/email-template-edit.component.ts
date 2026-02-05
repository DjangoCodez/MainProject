import { Component, inject, OnInit } from '@angular/core';
import { EmailTemplateDTO } from '../../models/email-template.model';
import { EmailTemplateService } from '../../services/email-template.service';
import { EditBaseDirective } from '@shared/directives/edit-base/edit-base.directive';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import { TranslateService } from '@ngx-translate/core';
import { Observable, tap } from 'rxjs';
import { SoeFormControl } from '@shared/extensions';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';

@Component({
  selector: 'soe-email-template-edit',
  templateUrl: './email-template-edit.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class EmailTemplateEditComponent
  extends EditBaseDirective<EmailTemplateDTO, EmailTemplateService>
  implements OnInit
{
  service = inject(EmailTemplateService);
  translate = inject(TranslateService);

  emailTemplateTypes: ISmallGenericType[] = [];
  ngOnInit(): void {
    super.ngOnInit();
    this.startFlow(Feature.Billing_Preferences_EmailTemplate_Edit, {
      lookups: [this.loadEmailTemplateTypes()],
    });
    this.initializeForm();
  }

  initializeForm(): void {
    if (this.form?.isCopy) {
      this.form?.clearIfExists(<SoeFormControl>this.form.controls.name);
    }
  }

  loadEmailTemplateTypes(): Observable<ISmallGenericType[]> {
    return this.service
      .loadEmailTemplateTypes()
      .pipe(tap(types => (this.emailTemplateTypes = types)));
  }
}
