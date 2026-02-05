import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SelectAccountDistributionPeriodDialogComponent } from './components/select-account-distribution-period-dialog.component';
import { ButtonComponent } from '@ui/button/button/button.component'
import { DialogComponent } from '@ui/dialog/dialog/dialog.component';
import { SharedModule } from '@shared/shared.module';
import { ReactiveFormsModule } from '@angular/forms';

@NgModule({
  declarations: [SelectAccountDistributionPeriodDialogComponent],
  exports: [SelectAccountDistributionPeriodDialogComponent],
  imports: [
    CommonModule,
    SharedModule,
    DialogComponent,
    ButtonComponent,
    ReactiveFormsModule,
  ],
})
export class SelectAccountDistributionPeriodDialogModule {}
