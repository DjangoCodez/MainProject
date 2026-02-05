import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HouseholdTaxDeductionComponent } from './components/household-tax-deduction/household-tax-deduction.component';
import { HouseholdTaxDeductionGridComponent } from './components/household-tax-deduction-grid/household-tax-deduction-grid.component';
import { SharedModule } from '@shared/shared.module';
import { ReactiveFormsModule } from '@angular/forms';
import { HouseholdTaxDeductionRoutingModule } from './household-tax-deduction-routing.module';
import { ButtonComponent } from '@ui/button/button/button.component'
import { CheckboxComponent } from '@ui/forms/checkbox/checkbox.component'
import { DatepickerComponent } from '@ui/forms/datepicker/datepicker.component'
import { DialogComponent } from '@ui/dialog/dialog/dialog.component'
import { EditFooterComponent } from '@ui/footer/edit-footer/edit-footer.component'
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component'
import { InstructionComponent } from '@ui/instruction/instruction.component'
import { LabelComponent } from '@ui/label/label.component'
import { MenuButtonComponent } from '@ui/button/menu-button/menu-button.component'
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component'
import { NumberboxComponent } from '@ui/forms/numberbox/numberbox.component'
import { SelectComponent } from '@ui/forms/select/select.component';
import { HouseholdTaxDeductionAppliedGridComponent } from './components/household-tax-deduction-applied-grid/household-tax-deduction-applied-grid.component';
import { HouseholdTaxDeductionReceivedGridComponent } from './components/household-tax-deduction-received-grid/household-tax-deduction-received-grid.component';
import { HouseholdTaxDeductionDeniedGridComponent } from './components/household-tax-deduction-denied-grid/household-tax-deduction-denied-grid.component';
import { HouseholdSequenceNumberModal } from './components/household-tax-deduction-applied-grid/household-sequence-number-modal/household-sequence-number-modal.component';
import { HouseholdPartialAmountModal } from './components/household-tax-deduction-applied-grid/household-partial-amount-modal/household-partial-amount-modal.component';
import { VoucherModule } from '@features/economy/voucher/voucher.module';

@NgModule({
  declarations: [
    HouseholdTaxDeductionComponent,
    HouseholdTaxDeductionGridComponent,
    HouseholdTaxDeductionAppliedGridComponent,
    HouseholdTaxDeductionReceivedGridComponent,
    HouseholdTaxDeductionDeniedGridComponent,
    HouseholdSequenceNumberModal,
    HouseholdPartialAmountModal,
  ],
  imports: [
    CommonModule,
    SharedModule,
    ReactiveFormsModule,
    HouseholdTaxDeductionRoutingModule,
    MultiTabWrapperComponent,
    GridWrapperComponent,
    LabelComponent,
    SelectComponent,
    EditFooterComponent,
    ButtonComponent,
    MenuButtonComponent,
    DatepickerComponent,
    NumberboxComponent,
    DialogComponent,
    InstructionComponent,
    CheckboxComponent,
    VoucherModule,
  ],
})
export class HouseholdTaxDeductionModule {}
