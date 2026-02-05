import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TextBlockDialogComponent } from './text-block-dialog.component';
import { ButtonComponent } from '@ui/button/button/button.component'
import { DeleteButtonComponent } from '@ui/button/delete-button/delete-button.component'
import { DialogComponent } from '@ui/dialog/dialog/dialog.component'
import { ExpansionPanelComponent } from '@ui/expansion-panel/expansion-panel.component'
import { SaveButtonComponent } from '@ui/button/save-button/save-button.component'
import { SelectComponent } from '@ui/forms/select/select.component'
import { TextareaComponent } from '@ui/forms/textarea/textarea.component'
import { TextboxComponent } from '@ui/forms/textbox/textbox.component';
import { SharedModule } from '@shared/shared.module';
import { ReactiveFormsModule } from '@angular/forms';

import { LanguageTranslationsModule } from '../../features/language-translations/language-translations.module';

@NgModule({
  declarations: [TextBlockDialogComponent],
  exports: [TextBlockDialogComponent],
  imports: [
    CommonModule,
    DialogComponent,
    ReactiveFormsModule,
    SelectComponent,
    TextboxComponent,
    TextareaComponent,
    ButtonComponent,
    SaveButtonComponent,
    DeleteButtonComponent,
    SharedModule,
    ExpansionPanelComponent,
    LanguageTranslationsModule,
  ],
})
export class TextBlockDialogModule {}
