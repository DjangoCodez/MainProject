import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ButtonComponent } from '@ui/button/button/button.component'
import { EditFooterComponent } from '@ui/footer/edit-footer/edit-footer.component'
import { ExpansionPanelComponent } from '@ui/expansion-panel/expansion-panel.component'
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component'
import { LabelComponent } from '@ui/label/label.component'
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component'
import { SaveButtonComponent } from '@ui/button/save-button/save-button.component'
import { SelectComponent } from '@ui/forms/select/select.component'
import { TextboxComponent } from '@ui/forms/textbox/textbox.component'
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { CompanyGroupTransferEditComponent } from './components/company-group-transfer-edit/company-group-transfer-edit.component';
import { SharedModule } from '@shared/shared.module';
import { ReactiveFormsModule } from '@angular/forms';
import { CompanyGroupTransferRoutingModule } from './company-group-transfer-routing.module';
import { TransferCarriedOutGridComponent } from './components/company-group-transfer-edit/transfer-carried-out-grid/transfer-carried-out-grid.component';
import { CompanyGroupTransferComponent } from './components/company-group-transfer/company-group-transfer.component';
import { VoucherModule } from '../voucher/voucher.module';

@NgModule({
  declarations: [
    CompanyGroupTransferComponent,
    CompanyGroupTransferEditComponent,
    TransferCarriedOutGridComponent,
  ],
  imports: [
    CommonModule,
    CompanyGroupTransferRoutingModule,
    SharedModule,
    MultiTabWrapperComponent,
    ToolbarComponent,
    LabelComponent,
    TextboxComponent,
    ReactiveFormsModule,
    EditFooterComponent,
    GridWrapperComponent,
    ButtonComponent,
    SaveButtonComponent,
    SelectComponent,
    ExpansionPanelComponent,
    VoucherModule,
  ],
})
export class CompanyGroupTransferModule {}
