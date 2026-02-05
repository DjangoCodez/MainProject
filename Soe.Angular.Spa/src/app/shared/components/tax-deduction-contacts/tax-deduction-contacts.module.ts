import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TaxDeductionContactsComponent } from './tax-deduction-contacts/tax-deduction-contacts.component';
import { SharedModule } from '@shared/shared.module';
import { TranslateModule } from '@ngx-translate/core';
import { ButtonComponent } from '@ui/button/button/button.component'
import { CheckboxComponent } from '@ui/forms/checkbox/checkbox.component'
import { DialogComponent } from '@ui/dialog/dialog/dialog.component'
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component'
import { IconModule } from '@ui/icon/icon.module'
import { LabelComponent } from '@ui/label/label.component'
import { TextboxComponent } from '@ui/forms/textbox/textbox.component'
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { ReactiveFormsModule } from '@angular/forms';

@NgModule({
  declarations: [TaxDeductionContactsComponent],
  exports: [TaxDeductionContactsComponent],
  imports: [
    CommonModule,
    SharedModule,
    TranslateModule,
    GridWrapperComponent,
    ButtonComponent,
    LabelComponent,
    TextboxComponent,
    ReactiveFormsModule,
    DialogComponent,
    ToolbarComponent,
    IconModule,
    CheckboxComponent,
  ],
})
export class TaxDeductionContactsModule {}
