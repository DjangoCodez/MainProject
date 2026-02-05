import { Component } from '@angular/core';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';
import { SignatoryContractGridComponent } from '../signatory-contract-grid/signatory-contract-grid.component';
import { SignatoryContractEditComponent } from '../signatory-contract-edit/signatory-contract-edit.component';
import { SignatoryContractForm } from '../../models/signatory-contract-form.model';
import { SoeConfigUtil } from '@shared/util/soeconfig-util';
@Component({
  selector: 'soe-signatory-contract',
  standalone: false,
  templateUrl:
    './signatory-contract.component.html',
})
export class SignatoryContractComponent {
  private readonly supportUserId = SoeConfigUtil.supportUserId;

  config: MultiTabConfig[] = [
    {
      gridComponent: SignatoryContractGridComponent,
      editComponent: SignatoryContractEditComponent,
      FormClass: SignatoryContractForm,
      gridTabLabel: 'manage.registry.signatorycontract.signatorycontract',
      editTabLabel: 'manage.registry.signatorycontract.signatorycontract',
      createTabLabel: 'manage.registry.signatorycontract.new_signatorycontract',
      exportFilenameKey: 'manage.registry.signatorycontract.signatorycontract',
    },
  ];

  protected get addPermission(): boolean {
    return !!this.supportUserId;
  }
}
