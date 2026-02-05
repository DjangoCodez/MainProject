import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AccountDimsComponent } from './account-dims/account-dims.component';
import { SharedModule } from '@shared/shared.module';
import { AutocompleteComponent } from '@ui/forms/autocomplete/autocomplete.component';
import { ReactiveFormsModule } from '@angular/forms';

@NgModule({
  declarations: [AccountDimsComponent],
  imports: [
    CommonModule,
    ReactiveFormsModule,
    SharedModule,
    AutocompleteComponent,
  ],
  exports: [AccountDimsComponent],
})
export class AccountDimsModule {}
