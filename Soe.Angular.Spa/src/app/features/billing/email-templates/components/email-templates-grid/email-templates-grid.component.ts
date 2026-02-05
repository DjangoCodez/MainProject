import { Component, inject, OnInit } from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import { IEmailTemplateDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { take } from 'rxjs';
import { EmailTemplateService } from '../../services/email-template.service';

@Component({
  selector: 'soe-email-templates-grid',
  templateUrl:
    '../../../../../shared/ui-components/grid/grid-wrapper/grid-wrapper-template.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class EmailTemplatesGridComponent
  extends GridBaseDirective<IEmailTemplateDTO, EmailTemplateService>
  implements OnInit
{
  service = inject(EmailTemplateService);
  types: ISmallGenericType[] = [];

  ngOnInit() {
    super.ngOnInit();

    this.startFlow(
      Feature.Billing_Preferences_EmailTemplate,
      'Billing.Invoices.EmailTemplates'
    );
  }

  override onGridReadyToDefine(grid: GridComponent<IEmailTemplateDTO>) {
    super.onGridReadyToDefine(grid);

    this.translate
      .get([
        'common.name',
        'common.type',
        'core.edit',
        'billing.invoices.emailtemplates',
        'billing.purchase.list.purchase',
        'common.customer.invoices.reminder',
        'common.salestypes',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnText('name', terms['common.name'], {
          flex: 1,
        });
        this.grid.addColumnText('typename', terms['common.type'], {
          flex: 1,
        });
        this.grid.addColumnIconEdit({
          tooltip: terms['core.edit'],
          onClick: row => {
            this.edit(row);
          },
        });
        super.finalizeInitGrid();
      });
  }
}
