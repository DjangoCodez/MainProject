import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { SharedModule } from '@shared/shared.module';
import { ButtonComponent } from '@ui/button/button/button.component'
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component'
import { IconModule } from '@ui/icon/icon.module'
import { LabelComponent } from '@ui/label/label.component'
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { AccountingRowsComponent } from './accounting-rows/accounting-rows.component';
import { AddAccountDialogModule } from '../add-account-dialog/add-account-dialog.module';

@NgModule({
  declarations: [AccountingRowsComponent],
  imports: [
    CommonModule,
    SharedModule,
    TranslateModule,
    ReactiveFormsModule,
    GridWrapperComponent,
    ButtonComponent,
    LabelComponent,
    IconModule,
    ToolbarComponent,
    AddAccountDialogModule,
  ],
  exports: [AccountingRowsComponent],
})
export class AccountingRowsModule {}
