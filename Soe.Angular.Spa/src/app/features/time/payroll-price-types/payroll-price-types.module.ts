import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PayrollPriceTypesRoutingModule } from './payroll-price-types-routing.module';
import { PayrollPriceTypesComponent } from './components/payroll-price-types/payroll-price-types.component';
import { PayrollPriceTypesGridComponent } from './components/payroll-price-types-grid/payroll-price-types-grid.component';
import { PayrollPriceTypesEditComponent } from './components/payroll-price-types-edit/payroll-price-types-edit.component';
import { SharedModule } from '@shared/shared.module';
import { ReactiveFormsModule } from '@angular/forms';
import { EditFooterComponent } from '@ui/footer/edit-footer/edit-footer.component'
import { ExpansionPanelComponent } from '@ui/expansion-panel/expansion-panel.component'
import { GridComponent } from '@ui/grid/grid.component'
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component'
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component'
import { NumberboxComponent } from '@ui/forms/numberbox/numberbox.component'
import { SelectComponent } from '@ui/forms/select/select.component'
import { TextboxComponent } from '@ui/forms/textbox/textbox.component'
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { PayrollPriceTypesEditPeriodsGridComponent } from './components/payroll-price-types-edit/payroll-price-types-edit-periods-grid/payroll-price-types-edit-periods-grid.component';

@NgModule({
  declarations: [
    PayrollPriceTypesComponent,
    PayrollPriceTypesGridComponent,
    PayrollPriceTypesEditComponent,
    PayrollPriceTypesEditPeriodsGridComponent,
  ],
  imports: [
    CommonModule,
    PayrollPriceTypesRoutingModule,
    SharedModule,
    ReactiveFormsModule,
    MultiTabWrapperComponent,
    GridWrapperComponent,
    GridComponent,
    ToolbarComponent,
    SelectComponent,
    TextboxComponent,
    EditFooterComponent,
    ExpansionPanelComponent,
    NumberboxComponent,
  ],
})
export class PayrollPriceTypesModule {}
