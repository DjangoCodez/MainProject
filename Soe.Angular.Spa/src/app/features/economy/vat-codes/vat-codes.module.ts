import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { SharedModule } from '@shared/shared.module';
import { AutocompleteComponent } from '@ui/forms/autocomplete/autocomplete.component'
import { ButtonComponent } from '@ui/button/button/button.component'
import { EditFooterComponent } from '@ui/footer/edit-footer/edit-footer.component'
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component'
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component'
import { NumberboxComponent } from '@ui/forms/numberbox/numberbox.component'
import { TextboxComponent } from '@ui/forms/textbox/textbox.component'
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { VatCodesEditComponent } from './components/vat-codes-edit/vat-codes-edit.component';
import { VatCodesGridComponent } from './components/vat-codes-grid/vat-codes-grid.component';
import { VatCodesComponent } from './components/vat-codes/vat-codes.component';
import { VatCodesRoutingModule } from './vat-codes-routing.module';

@NgModule({
  declarations: [
    VatCodesComponent,
    VatCodesEditComponent,
    VatCodesGridComponent,
  ],
  imports: [
    VatCodesRoutingModule,
    MultiTabWrapperComponent,
    GridWrapperComponent,
    ToolbarComponent,
    TextboxComponent,
    AutocompleteComponent,
    NumberboxComponent,
    ReactiveFormsModule,
    EditFooterComponent,
    ButtonComponent,
    CommonModule,
    SharedModule,
  ],
})
export class VatCodesModule {}
