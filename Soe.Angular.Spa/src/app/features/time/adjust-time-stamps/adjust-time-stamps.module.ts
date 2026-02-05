import { NgModule } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { AdjustTimeStampsRoutingModule } from './adjust-time-stamps-routing.module';
import { AdjustTimeStampsComponent } from './components/adjust-time-stamps/adjust-time-stamps.component';
import { AdjustTimeStampsGridComponent } from './components/adjust-time-stamps-grid/adjust-time-stamps-grid.component';
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component';
import { SelectComponent } from '@ui/forms/select/select.component';
import { ButtonComponent } from '@ui/button/button/button.component';
import { SaveButtonComponent } from '@ui/button/save-button/save-button.component';
import { InstructionComponent } from '@ui/instruction/instruction.component';
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component';
import { ReactiveFormsModule } from '@angular/forms';
import { MultiSelectComponent } from '@ui/forms/select/multi-select/multi-select.component';
import { DatepickerComponent } from '@ui/forms/datepicker/datepicker.component';
import { TimeStampDetailsDialogData } from '@shared/components/time/time-stamp-details-dialog/time-stamp-details-controller';
import { DialogComponent } from '@ui/dialog/dialog/dialog.component';
import { LabelComponent } from '@ui/label/label.component';
import { ExpansionPanelComponent } from '@ui/expansion-panel/expansion-panel.component';
import { CreatedModifiedComponent } from '@ui/created-modified/created-modified.component';
import { TextareaComponent } from '@ui/forms/textarea/textarea.component';
import { TextboxComponent } from '@ui/forms/textbox/textbox.component';
import { TranslateModule } from '@ngx-translate/core';

@NgModule({
  declarations: [
    AdjustTimeStampsComponent,
    AdjustTimeStampsGridComponent,
    TimeStampDetailsDialogData,
  ],
  imports: [
    CommonModule,
    AdjustTimeStampsRoutingModule,
    GridWrapperComponent,
    SelectComponent,
    ButtonComponent,
    SaveButtonComponent,
    InstructionComponent,
    ReactiveFormsModule,
    MultiTabWrapperComponent,
    MultiSelectComponent,
    DatepickerComponent,
    DialogComponent,
    LabelComponent,
    ExpansionPanelComponent,
    CreatedModifiedComponent,
    TextareaComponent,
    TextboxComponent,
    TranslateModule,
  ],
  providers: [DatePipe],
})
export class AdjustTimeStampsModule {}
