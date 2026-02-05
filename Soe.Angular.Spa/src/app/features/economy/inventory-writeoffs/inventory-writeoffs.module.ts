import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ButtonComponent } from '@ui/button/button/button.component'
import { DatepickerComponent } from '@ui/forms/datepicker/datepicker.component'
import { DialogComponent } from '@ui/dialog/dialog/dialog.component'
import { EditFooterComponent } from '@ui/footer/edit-footer/edit-footer.component'
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component'
import { IconButtonComponent } from '@ui/button/icon-button/icon-button.component'
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component'
import { TextareaComponent } from '@ui/forms/textarea/textarea.component'
import { TextboxComponent } from '@ui/forms/textbox/textbox.component'
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { InventoryWriteoffsRoutingModule } from './inventory-writeoffs-routing.module';
import { ReactiveFormsModule } from '@angular/forms';
import { SharedModule } from '@shared/shared.module';
import { InventoryWriteoffsComponent } from './components/inventory-writeoffs/inventory-writeoffs.component';
import { InventoryWriteoffsGridComponent } from './components/inventory-writeoffs-grid/inventory-writeoffs-grid.component';
import { InventoryWriteoffsSearchComponent } from './components/inventory-writeoffs-search/inventory-writeoffs-search.component';
import { InventoryNotesDialogComponent } from './components/inventory-notes-dialog/inventory-notes-dialog.component';
import { VoucherModule } from '../voucher/voucher.module';
import { AccountDistributionUrlParamsService } from '../account-distribution/services/account-distribution-params.service';

@NgModule({
  declarations: [
    InventoryWriteoffsComponent,
    InventoryWriteoffsGridComponent,
    InventoryWriteoffsSearchComponent,
    InventoryNotesDialogComponent,
  ],
  imports: [
    CommonModule,
    InventoryWriteoffsRoutingModule,
    SharedModule,
    MultiTabWrapperComponent,
    ToolbarComponent,
    TextboxComponent,
    ReactiveFormsModule,
    EditFooterComponent,
    GridWrapperComponent,
    ButtonComponent,
    IconButtonComponent,
    DatepickerComponent,
    DialogComponent,
    TextareaComponent,
    VoucherModule,
  ],
  providers: [AccountDistributionUrlParamsService],
})
export class InventoryWriteoffsModule {}
