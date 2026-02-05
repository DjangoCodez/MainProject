import { Component } from '@angular/core';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';
import { FinnishTaxExportForm } from '../../models/finnish-tax-export-form.model';
import { FinnishTaxExportEditComponent } from '../finnish-tax-export-edit/finnish-tax-export-edit.component';

@Component({
  selector: 'soe-finnish-tax-export',
  templateUrl: './finnish-tax-export.component.html',
  standalone: false,
})
export class FinnishTaxExportComponent {
  config: MultiTabConfig[] = [
    {
      editComponent: FinnishTaxExportEditComponent,
      FormClass: FinnishTaxExportForm,
      editTabLabel: 'economy.export.finnishtax.periodtaxreturn',
      createTabLabel: 'economy.export.finnishtax.periodtaxreturn',
    },
  ];
}
