import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ButtonComponent } from '@ui/button/button/button.component'
import { DialogComponent } from '@ui/dialog/dialog/dialog.component'
import { EditFooterComponent } from '@ui/footer/edit-footer/edit-footer.component'
import { FileUploadComponent } from '@ui/forms/file-upload/file-upload.component'
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component'
import { InstructionComponent } from '@ui/instruction/instruction.component'
import { LabelComponent } from '@ui/label/label.component'
import { MenuButtonComponent } from '@ui/button/menu-button/menu-button.component'
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component'
import { SaveButtonComponent } from '@ui/button/save-button/save-button.component'
import { SelectComponent } from '@ui/forms/select/select.component'
import { TextboxComponent } from '@ui/forms/textbox/textbox.component'
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { DiscountLettersRoutingModule } from './discount-letters-routing.module';
import { DiscountLettersComponent } from './components/discount-letters/discount-letters.component';
import { DiscountLettersGridComponent } from './components/discount-letters-grid/discount-letters-grid.component';
import { ReactiveFormsModule } from '@angular/forms';
import { DiscountLettersGridHeaderComponent } from './components/discount-letters-grid-header/discount-letters-grid-header.component';
import { SupplierAgreementDialogComponent } from './components/discount-letters-grid/supplier-agreement-dialog/supplier-agreement-dialog.component';
import { SharedModule } from '@shared/shared.module';
import { ImportAgreementDialogComponent } from './components/discount-letters-grid/import-agreement-dialog/import-agreement-dialog.component';
import { DeleteAgreementDialogComponent } from './components/discount-letters-grid/delete-agreement-dialog/delete-agreement-dialog.component';
import { NetPricesGridComponent } from './components/net-prices-grid/net-prices-grid.component';
import { NetPricesGridHeaderComponent } from './components/net-prices-grid-header/net-prices-grid-header.component';

@NgModule({
  declarations: [
    DiscountLettersComponent,
    DiscountLettersGridComponent,
    DiscountLettersGridHeaderComponent,
    SupplierAgreementDialogComponent,
    ImportAgreementDialogComponent,
    DeleteAgreementDialogComponent,
    NetPricesGridComponent,
    NetPricesGridHeaderComponent,
  ],
  imports: [
    CommonModule,
    SharedModule,
    DiscountLettersRoutingModule,
    MultiTabWrapperComponent,
    ToolbarComponent,
    LabelComponent,
    TextboxComponent,
    ReactiveFormsModule,
    EditFooterComponent,
    GridWrapperComponent,
    ButtonComponent,
    MenuButtonComponent,
    SaveButtonComponent,
    SelectComponent,
    DialogComponent,
    InstructionComponent,
    FileUploadComponent,
  ],
})
export class DiscountLettersModule {}
