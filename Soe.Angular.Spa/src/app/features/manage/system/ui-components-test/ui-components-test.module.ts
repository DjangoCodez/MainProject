import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { MatExpansionModule } from '@angular/material/expansion';
import { FileDisplayModule } from '@shared/components/file-display/file-display.module';
import { SharedModule } from '@shared/shared.module';
import { AutocompleteComponent } from '@ui/forms/autocomplete/autocomplete.component';
import { ButtonComponent } from '@ui/button/button/button.component';
import { CheckboxComponent } from '@ui/forms/checkbox/checkbox.component';
import { ColorpickerComponent } from '@ui/forms/colorpicker/colorpicker.component';
import { DatepickerComponent } from '@ui/forms/datepicker/datepicker.component';
import { DaterangepickerComponent } from '@ui/forms/datepicker/daterangepicker/daterangepicker.component';
import { DatespickerComponent } from '@ui/forms/datepicker/datespicker/datespicker.component';
import { EditFooterComponent } from '@ui/footer/edit-footer/edit-footer.component';
import { ExpansionPanelComponent } from '@ui/expansion-panel/expansion-panel.component';
import { FileUploadComponent } from '@ui/forms/file-upload/file-upload.component';
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component';
import { IconButtonComponent } from '@ui/button/icon-button/icon-button.component';
import { InstructionComponent } from '@ui/instruction/instruction.component';
import { LabelComponent } from '@ui/label/label.component';
import { MenuButtonComponent } from '@ui/button/menu-button/menu-button.component';
import { MultiSelectComponent } from '@ui/forms/select/multi-select/multi-select.component';
import { NumberboxComponent } from '@ui/forms/numberbox/numberbox.component';
import { NumberrangeComponent } from '@ui/forms/numberbox/numberrange/numberrange.component';
import { RadioComponent } from '@ui/forms/radio/radio.component';
import { RecordNavigatorComponent } from '@ui/record-navigator/record-navigator.component';
import { ResizeContainerComponent } from '@ui/resize-container/resize-container.component';
import { SelectComponent } from '@ui/forms/select/select.component';
import { SliderComponent } from '@ui/slider/slider.component';
import { SplitContainerComponent } from '@ui/split-container/split-container.component';
import { TextareaComponent } from '@ui/forms/textarea/textarea.component';
import { TextboxComponent } from '@ui/forms/textbox/textbox.component';
import { TexteditorComponent } from '@ui/forms/texteditor/texteditor.component';
import { TimeboxComponent } from '@ui/forms/timebox/timebox.component';
import { TimerangeComponent } from '@ui/forms/timebox/timerange/timerange.component';
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { GridTestComponentsModule } from './components/grid-test-components/grid-test-components.module';
import { UiComponentsTestComponent } from './components/ui-components-test/ui-components-test.component';
import { UiComponentsTestRoutingModule } from './ui-components-test-routing.module';

@NgModule({
  declarations: [UiComponentsTestComponent],
  imports: [
    CommonModule,
    SharedModule,
    GridWrapperComponent,
    ReactiveFormsModule,
    UiComponentsTestRoutingModule,
    EditFooterComponent,
    ButtonComponent,
    IconButtonComponent,
    MenuButtonComponent,
    CheckboxComponent,
    ColorpickerComponent,
    DatepickerComponent,
    DaterangepickerComponent,
    DatespickerComponent,
    MatExpansionModule,
    ExpansionPanelComponent,
    FileUploadComponent,
    InstructionComponent,
    LabelComponent,
    NumberboxComponent,
    NumberrangeComponent,
    RadioComponent,
    ResizeContainerComponent,
    SelectComponent,
    MultiSelectComponent,
    SliderComponent,
    SplitContainerComponent,
    TextareaComponent,
    TextboxComponent,
    TexteditorComponent,
    TimeboxComponent,
    TimerangeComponent,
    ToolbarComponent,
    GridTestComponentsModule,
    RecordNavigatorComponent,
    FileDisplayModule,
    AutocompleteComponent,
  ],
})
export class UiComponentsTestModule {}
