import { Component } from '@angular/core';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';
import { HouseholdTaxDeductionForm } from '../../models/household-tax-deduction-form.model';
import { HouseholdTaxDeductionGridComponent } from '../household-tax-deduction-grid/household-tax-deduction-grid.component';
import { HouseholdTaxDeductionAppliedGridComponent } from '../household-tax-deduction-applied-grid/household-tax-deduction-applied-grid.component';
import { HouseholdTaxDeductionReceivedGridComponent } from '../household-tax-deduction-received-grid/household-tax-deduction-received-grid.component';
import { HouseholdTaxDeductionDeniedGridComponent } from '../household-tax-deduction-denied-grid/household-tax-deduction-denied-grid.component';

@Component({
    selector: 'soe-household-tax-deduction',
    templateUrl: 'household-tax-deduction.component.html',
    standalone: false
})
export class HouseholdTaxDeductionComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: HouseholdTaxDeductionGridComponent,
      FormClass: HouseholdTaxDeductionForm,
      gridTabLabel: 'billing.invoices.householddeduction.applyrot',
      exportFilenameKey: 'billing.invoices.householddeduction.applyrot',
    },
    {
      gridComponent: HouseholdTaxDeductionAppliedGridComponent,
      FormClass: HouseholdTaxDeductionForm,
      gridTabLabel: 'billing.invoices.householddeduction.appliedmany',
      exportFilenameKey: 'billing.invoices.householddeduction.appliedmany',
    },
    {
      gridComponent: HouseholdTaxDeductionReceivedGridComponent,
      FormClass: HouseholdTaxDeductionForm,
      gridTabLabel: 'billing.invoices.householddeduction.approvedmulti',
      exportFilenameKey: 'billing.invoices.householddeduction.approvedmulti',
    },
    {
      gridComponent: HouseholdTaxDeductionDeniedGridComponent,
      FormClass: HouseholdTaxDeductionForm,
      gridTabLabel: 'billing.invoices.householddeduction.deniedmulti',
      exportFilenameKey: 'billing.invoices.householddeduction.deniedmulti',
    },
  ];
}
