import { NgModule } from '@angular/core';
import { ButtonComponent } from '@ui/button/button/button.component'
import { DialogComponent } from '@ui/dialog/dialog/dialog.component'
import { EditFooterComponent } from '@ui/footer/edit-footer/edit-footer.component'
import { ExpansionPanelComponent } from '@ui/expansion-panel/expansion-panel.component'
import { LabelComponent } from '@ui/label/label.component'
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component'
import { SaveButtonComponent } from '@ui/button/save-button/save-button.component'
import { TextareaComponent } from '@ui/forms/textarea/textarea.component'
import { TextboxComponent } from '@ui/forms/textbox/textbox.component'
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { SharedModule } from '@shared/shared.module';
import { CommonModule } from '@angular/common';
import { HouseholdTaxDeductionDialogComponent } from './household-tax-deduction-dialog.component';
import { TranslateModule } from '@ngx-translate/core';

@NgModule({
  declarations: [HouseholdTaxDeductionDialogComponent],
  imports: [
    CommonModule,
    SharedModule,
    ReactiveFormsModule,
    DialogComponent,
    LabelComponent,
    TextboxComponent,
    ExpansionPanelComponent,
    ButtonComponent,
    SaveButtonComponent,
    MultiTabWrapperComponent,
    ToolbarComponent,
    EditFooterComponent,
    TranslateModule,
    FormsModule,
    TextareaComponent,
  ],
  exports: [HouseholdTaxDeductionDialogComponent],
})
export class HouseholdTaxDeductionDialogModule {}
