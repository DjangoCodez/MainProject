import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TimeCodeAdditionDeductionRoutingModule } from './time-code-addition-deduction-routing.module';
import { TimeCodeAdditionDeductionComponent } from './components/time-code-addition-deduction/time-code-addition-deduction.component';
import { TimeCodeAdditionDeductionGridComponent } from './components/time-code-addition-deduction-grid/time-code-addition-deduction-grid.component';
import { TimeCodeAdditionDeductionEditComponent } from './components/time-code-addition-deduction-edit/time-code-addition-deduction-edit.component';
import { SharedModule } from '@shared/shared.module';
import { ReactiveFormsModule } from '@angular/forms';
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component';
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component';
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { ExpansionPanelComponent } from '@ui/expansion-panel/expansion-panel.component';
import { ButtonComponent } from '@ui/button/button/button.component';
import { TextboxComponent } from '@ui/forms/textbox/textbox.component';
import { SelectComponent } from '@ui/forms/select/select.component';
import { EditFooterComponent } from '@ui/footer/edit-footer/edit-footer.component';
import { NumberboxComponent } from '@ui/forms/numberbox/numberbox.component';
import { CheckboxComponent } from '@ui/forms/checkbox/checkbox.component';
import { LabelComponent } from '@ui/label/label.component';
import { SaveButtonComponent } from '@ui/button/save-button/save-button.component';
import { TimeCodePayrollProductsGridModule } from '@shared/components/time/time-code-payroll-products-grid/time-code-payroll-products-grid.module';
import { TimeCodeInvoiceProductsGridModule } from '@shared/components/time/time-code-invoice-products-grid/time-code-invoice-products-grid.module';

@NgModule({
  declarations: [
    TimeCodeAdditionDeductionComponent,
    TimeCodeAdditionDeductionGridComponent,
    TimeCodeAdditionDeductionEditComponent,
  ],
  imports: [
    CommonModule,
    SharedModule,
    ReactiveFormsModule,
    MultiTabWrapperComponent,
    GridWrapperComponent,
    ToolbarComponent,
    EditFooterComponent,
    ButtonComponent,
    ExpansionPanelComponent,
    TimeCodeAdditionDeductionRoutingModule,
    TextboxComponent,
    SelectComponent,
    NumberboxComponent,
    CheckboxComponent,
    LabelComponent,
    SaveButtonComponent,
    TimeCodePayrollProductsGridModule,
    TimeCodeInvoiceProductsGridModule,
  ],
})
export class TimeCodeAdditionDeductionModule {}
