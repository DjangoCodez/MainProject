import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { AccountingRowsModule } from '@shared/components/accounting-rows/accounting-rows.module';
import { FileDisplayModule } from '@shared/components/file-display/file-display.module';
import { TraceRowsModule } from '@shared/components/trace-rows/trace-rows.module';
import { SharedModule } from '@shared/shared.module';
import { ButtonComponent } from '@ui/button/button/button.component'
import { CheckboxComponent } from '@ui/forms/checkbox/checkbox.component'
import { CreatedModifiedComponent } from '@ui/created-modified/created-modified.component'
import { DatepickerComponent } from '@ui/forms/datepicker/datepicker.component'
import { DeleteButtonComponent } from '@ui/button/delete-button/delete-button.component'
import { ExpansionPanelComponent } from '@ui/expansion-panel/expansion-panel.component'
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component'
import { IconButtonComponent } from '@ui/button/icon-button/icon-button.component'
import { MenuButtonComponent } from '@ui/button/menu-button/menu-button.component'
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component'
import { SaveButtonComponent } from '@ui/button/save-button/save-button.component'
import { SelectComponent } from '@ui/forms/select/select.component'
import { TextareaComponent } from '@ui/forms/textarea/textarea.component'
import { TextboxComponent } from '@ui/forms/textbox/textbox.component'
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { VoucherEditHistoryGridComponent } from './components/voucher-edit/voucher-edit-history-grid/voucher-edit-history-grid.component';
import { VoucherEditComponent } from './components/voucher-edit/voucher-edit.component';
import { VoucherGridFilterComponent } from './components/voucher-grid/voucher-grid-filter/voucher-grid-filter.component';
import { VoucherGridComponent } from './components/voucher-grid/voucher-grid.component';
import { VoucherComponent } from './components/voucher/voucher.component';
import { VoucherRoutingModule } from './voucher-routing.module';
import { VoucherParamsService } from './services/voucher-params.service';

@NgModule({
  declarations: [
    VoucherComponent,
    VoucherEditComponent,
    VoucherGridComponent,
    VoucherGridFilterComponent,
    VoucherEditHistoryGridComponent,
  ],
  imports: [
    CommonModule,
    SharedModule,
    VoucherRoutingModule,
    CreatedModifiedComponent,
    SelectComponent,
    ReactiveFormsModule,
    GridWrapperComponent,
    ToolbarComponent,
    MultiTabWrapperComponent,
    ExpansionPanelComponent,
    TextareaComponent,
    ButtonComponent,
    DeleteButtonComponent,
    IconButtonComponent,
    MenuButtonComponent,
    CheckboxComponent,
    TextboxComponent,
    FileDisplayModule,
    TraceRowsModule,
    AccountingRowsModule,
    DatepickerComponent,
    SaveButtonComponent,
  ],
  providers: [VoucherParamsService],
})
export class VoucherModule {}
