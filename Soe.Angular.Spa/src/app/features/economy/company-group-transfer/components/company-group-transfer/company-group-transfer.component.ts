import { Component } from '@angular/core';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';
import { CompanyGroupTransferEditComponent } from '../company-group-transfer-edit/company-group-transfer-edit.component';
import { CompanyGroupTransferForm } from '../../models/company-group-transfer-form.model';

@Component({
  templateUrl: './company-group-transfer.component.html',
  standalone: false,
})
export class CompanyGroupTransferComponent {
  config: MultiTabConfig[] = [
    {
      editComponent: CompanyGroupTransferEditComponent,
      FormClass: CompanyGroupTransferForm,
      editTabLabel: 'economy.accounting.companygroup.transfer',
      hideForCreateTabMenu: true,
    },
  ];
}
