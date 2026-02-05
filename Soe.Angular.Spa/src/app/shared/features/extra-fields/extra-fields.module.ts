import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { SharedModule } from '@shared/shared.module';
import { ButtonComponent } from '@ui/button/button/button.component';
import { CheckboxComponent } from '@ui/forms/checkbox/checkbox.component';
import { DatepickerComponent } from '@ui/forms/datepicker/datepicker.component';
import { EditFooterComponent } from '@ui/footer/edit-footer/edit-footer.component';
import { ExpansionPanelComponent } from '@ui/expansion-panel/expansion-panel.component';
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component';
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component';
import { NumberboxComponent } from '@ui/forms/numberbox/numberbox.component';
import { SelectComponent } from '@ui/forms/select/select.component';
import { TextboxComponent } from '@ui/forms/textbox/textbox.component';
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { LanguageTranslationsModule } from '../language-translations/language-translations.module';
import { ExtraFieldsEditValuesGridComponent } from './components/extra-fields-edit/extra-fields-edit-values-grid/extra-fields-edit-values-grid.component';
import { ExtraFieldsEditComponent } from './components/extra-fields-edit/extra-fields-edit.component';
import { ExtraFieldsGridComponent } from './components/extra-fields-grid/extra-fields-grid.component';
import { ExtraFieldsInputComponent } from './components/extra-fields-input/extra-fields-input.component';
import { ExtraFieldsComponent } from './components/extra-fields/extra-fields.component';
import { ExtraFieldsRoutingModule } from './extra-fields-routing.module';
import { ExtraFieldsGridFilterComponent } from './components/extra-fields-grid/extra-fields-grid-filter/extra-fields-grid-filter.component';
@NgModule({
  declarations: [
    ExtraFieldsComponent,
    ExtraFieldsGridComponent,
    ExtraFieldsEditComponent,
    ExtraFieldsEditValuesGridComponent,
    ExtraFieldsInputComponent,
    ExtraFieldsGridFilterComponent,
  ],
  exports: [ExtraFieldsInputComponent],
  imports: [
    ExtraFieldsRoutingModule,
    ExpansionPanelComponent,
    ButtonComponent,
    CommonModule,
    ReactiveFormsModule,
    LanguageTranslationsModule,
    CheckboxComponent,
    DatepickerComponent,
    EditFooterComponent,
    GridWrapperComponent,
    NumberboxComponent,
    SelectComponent,
    SharedModule,
    MultiTabWrapperComponent,
    TextboxComponent,
    ToolbarComponent,
  ],
})
export class ExtraFieldsModule {}
