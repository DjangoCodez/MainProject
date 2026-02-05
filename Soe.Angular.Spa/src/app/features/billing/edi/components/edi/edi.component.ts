import { Component } from '@angular/core';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';
import { EdiGridComponent } from '../edi-grid/edi-grid.component';
import {
  SoeOriginType,
  TermGroup_EDIStatus,
} from '@shared/models/generated-interfaces/Enumerations';

@Component({
  selector: 'soe-edi',
  templateUrl: './edi.component.html',
  standalone: false,
})
export class EdiComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: EdiGridComponent,
      gridTabLabel: 'billing.import.edi.openorders',
      exportFilenameKey: 'billing.import.edi.openorders',
      hideForCreateTabMenu: true,
      additionalGridProps: {
        originType: SoeOriginType.Order,
        ediStatus: TermGroup_EDIStatus.Unprocessed,
      },
      passGridDataOnAdd: true,
    },
    {
      gridComponent: EdiGridComponent,
      gridTabLabel: 'billing.import.edi.openinvoices',
      exportFilenameKey: 'billing.import.edi.openinvoices',
      hideForCreateTabMenu: true,
      additionalGridProps: {
        originType: SoeOriginType.SupplierInvoice,
        ediStatus: TermGroup_EDIStatus.Unprocessed,
      },
      passGridDataOnAdd: true,
    },
    {
      gridComponent: EdiGridComponent,
      gridTabLabel: 'billing.import.edi.closedorders',
      exportFilenameKey: 'billing.import.edi.closedorders',
      hideForCreateTabMenu: true,
      additionalGridProps: {
        originType: SoeOriginType.Order,
        ediStatus: TermGroup_EDIStatus.Processed,
      },
      passGridDataOnAdd: true,
    },
    {
      gridComponent: EdiGridComponent,
      gridTabLabel: 'billing.import.edi.closedinvoices',
      exportFilenameKey: 'billing.import.edi.closedinvoices',
      hideForCreateTabMenu: true,
      additionalGridProps: {
        originType: SoeOriginType.SupplierInvoice,
        ediStatus: TermGroup_EDIStatus.Processed,
      },
    },
  ];
}
