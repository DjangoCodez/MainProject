import { Component } from '@angular/core';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';
import { PayrollPriceTypesGridComponent } from '../payroll-price-types-grid/payroll-price-types-grid.component';
import { PayrollPriceTypesEditComponent } from '../payroll-price-types-edit/payroll-price-types-edit.component';
import { PayrollPriceTypesForm } from '../../models/payroll-price-types-form.model';

@Component({
  templateUrl:
    '../../../../../shared/ui-components/tab/multi-tab-wrapper/multi-tab-wrapper-template.html',
  standalone: false,
})
export class PayrollPriceTypesComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: PayrollPriceTypesGridComponent,
      editComponent: PayrollPriceTypesEditComponent,
      FormClass: PayrollPriceTypesForm,
      gridTabLabel: 'time.payroll.payrollpricetype.payrollpricetypes',
      editTabLabel: 'time.payroll.payrollpricetype.payrollpricetype',
      createTabLabel: 'time.payroll.payrollpricetype.new',
    },
  ];
}
