import { Component } from '@angular/core';
import { CompanyGroupAdministrationGridComponent } from '../company-group-administration-grid/company-group-administration-grid.component';
import { CompanyGroupAdministrationEditComponent } from '../company-group-administration-edit/company-group-administration-edit.component';
import { CompanyGroupAdministrationForm } from '../../models/company-group-administration-form.model';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';

@Component({
  templateUrl:
    '../../../../../shared/ui-components/tab/multi-tab-wrapper/multi-tab-wrapper-template.html',
  standalone: false,
})
export class CompanyGroupAdministrationComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: CompanyGroupAdministrationGridComponent,
      editComponent: CompanyGroupAdministrationEditComponent,
      FormClass: CompanyGroupAdministrationForm,
      exportFilenameKey: 'economy.accounting.companygroup.companies',
      gridTabLabel: 'economy.accounting.companygroup.companies',
      editTabLabel: 'economy.accounting.companygroup.company',
      createTabLabel: 'economy.accounting.companygroup.newcompany',
    },
  ];
}
