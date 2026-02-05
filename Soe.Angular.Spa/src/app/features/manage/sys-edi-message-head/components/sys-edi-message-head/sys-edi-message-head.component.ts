import { Component } from '@angular/core';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';
import { SysEdiMessageHeadForm } from '../../models/sys-edi-message-head-form.model';
import { SysEdiMessageHeadEditComponent } from '../sys-edi-message-head-edit/sys-edi-message-head-edit.component';
import { SysEdiMessageHeadGridComponent } from '../sys-edi-message-head-grid/sys-edi-message-head-grid.component';

@Component({
  templateUrl:
    '../../../../../shared/ui-components/tab/multi-tab-wrapper/multi-tab-wrapper-template.html',
  standalone: false,
})
export class SysEdiMessageHeadComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: SysEdiMessageHeadGridComponent,
      editComponent: SysEdiMessageHeadEditComponent,
      FormClass: SysEdiMessageHeadForm,
      gridTabLabel: 'manage.system.edi.sysedimessageheads',
      editTabLabel: 'manage.system.edi.sysedimessagehead',
      createTabLabel: 'manage.system.edi.new',
    },
  ];
}
