import { Component } from '@angular/core';
import { CompanyGroupMappingsEditComponent } from '../company-group-mappings-edit/company-group-mappings-edit.component';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';
import { CompanyGroupMappingHeadForm } from '../models/company-group-mappings-form.model';
import { CompanyGroupMappingsGridComponent } from '../company-group-mappings-grid/company-group-mappings-grid.component';

@Component({
  selector: 'soe-company-group-mappings',
  templateUrl:
    '../../../../shared/ui-components/tab/multi-tab-wrapper/multi-tab-wrapper-template.html',
  standalone: false,
})
export class CompanyGroupMappingsComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: CompanyGroupMappingsGridComponent,
      editComponent: CompanyGroupMappingsEditComponent,
      FormClass: CompanyGroupMappingHeadForm,
      gridTabLabel: 'economy.accounting.companygroup.mappings',
      editTabLabel: 'economy.accounting.companygroup.mapping',
      createTabLabel: 'economy.accounting.companygroup.newmapping',
      exportFilenameKey: 'economy.accounting.companygroup.mappings',
    },
  ];
}
