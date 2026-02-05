import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TextBlockRoutingModule } from './text-block-routing.module';
import { TextBlockComponent } from './components/text-block/text-block.component';
import { TextBlockGridComponent } from './components/text-block-grid/text-block-grid.component';
import { ReactiveFormsModule } from '@angular/forms';
import { EditDeliveryAddressModule } from '@shared/components/billing/edit-delivery-address/edit-delivery-address.module';
import { SharedModule } from '@shared/shared.module';
import { ButtonComponent } from '@ui/button/button/button.component'
import { CheckboxComponent } from '@ui/forms/checkbox/checkbox.component'
import { DialogComponent } from '@ui/dialog/dialog/dialog.component'
import { EditFooterComponent } from '@ui/footer/edit-footer/edit-footer.component'
import { ExpansionPanelComponent } from '@ui/expansion-panel/expansion-panel.component'
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component'
import { LabelComponent } from '@ui/label/label.component'
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component'
import { SelectComponent } from '@ui/forms/select/select.component'
import { TextareaComponent } from '@ui/forms/textarea/textarea.component'
import { TextboxComponent } from '@ui/forms/textbox/textbox.component'
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { TextBlockEditComponent } from './components/text-block-edit/text-block-edit.component';
import { LanguageTranslationsModule } from '@shared/features/language-translations/language-translations.module';

@NgModule({
  declarations: [
    TextBlockComponent,
    TextBlockGridComponent,
    TextBlockEditComponent,
  ],
  imports: [
    CommonModule,
    ExpansionPanelComponent,
    ButtonComponent,
    GridWrapperComponent,
    EditFooterComponent,
    SelectComponent,
    MultiTabWrapperComponent,
    TextboxComponent,
    ToolbarComponent,
    ReactiveFormsModule,
    SharedModule,
    TextBlockRoutingModule,
    CheckboxComponent,
    DialogComponent,
    TextareaComponent,
    EditDeliveryAddressModule,
    LanguageTranslationsModule,
    LabelComponent,
  ],
})
export class TextBlockModule {}
