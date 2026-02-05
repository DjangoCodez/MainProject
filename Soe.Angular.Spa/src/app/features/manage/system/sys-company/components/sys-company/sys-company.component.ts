import { Component } from '@angular/core';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';
import { SysCompanyGridComponent } from '../sys-company-grid/sys-company-grid.component';
import { SysCompanyEditComponent } from '../sys-company-edit/sys-company-edit.component';
import { SysCompanyForm } from '../../models/sys-company-form.model';

@Component({
  selector: 'soe-sys-company',
  templateUrl:
    '../../../../../../shared/ui-components/tab/multi-tab-wrapper/multi-tab-wrapper-template.html',
  standalone: false,
})
export class SysCompanyComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: SysCompanyGridComponent,
      editComponent: SysCompanyEditComponent,
      FormClass: SysCompanyForm,
      gridTabLabel: 'manage.system.syscompany.syscompany',
      editTabLabel: 'manage.system.syscompany.syscompany',
      createTabLabel: 'manage.system.syscompany.syscompany_new',
      exportFilenameKey: 'manage.system.sysCompany.sysCompany',
    },
  ];
}
