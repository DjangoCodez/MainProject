import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { ButtonComponent } from '@ui/button/button/button.component'
import { DialogComponent } from '@ui/dialog/dialog/dialog.component'
import { IconButtonComponent } from '@ui/button/icon-button/icon-button.component'
import { SaveButtonComponent } from '@ui/button/save-button/save-button.component'
import { SelectComponent } from '@ui/forms/select/select.component'
import { TextboxComponent } from '@ui/forms/textbox/textbox.component';
import { StockBalanceFileImportComponent } from './stock-balance-file-import/stock-balance-file-import.component';
import { SharedModule } from '@shared/shared.module';
import { ReactiveFormsModule } from '@angular/forms';

@NgModule({
  declarations: [StockBalanceFileImportComponent],
  exports: [StockBalanceFileImportComponent],
  imports: [
    CommonModule,
    SharedModule,
    ButtonComponent,
    IconButtonComponent,
    SaveButtonComponent,
    TranslateModule.forChild(),
    TextboxComponent,
    ReactiveFormsModule,
    SelectComponent,
    DialogComponent,
  ],
})
export class StockBalanceFileImportModule {}
