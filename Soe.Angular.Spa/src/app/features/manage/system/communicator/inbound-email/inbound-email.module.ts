import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { ReactiveFormsModule } from '@angular/forms';
import { SharedModule } from '@shared/shared.module';
import { ButtonComponent } from '@ui/button/button/button.component'
import { DialogComponent } from '@ui/dialog/dialog/dialog.component'
import { ExpansionPanelComponent } from '@ui/expansion-panel/expansion-panel.component'
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component'
import { IconButtonComponent } from '@ui/button/icon-button/icon-button.component'
import { InstructionComponent } from '@ui/instruction/instruction.component'
import { MultiSelectComponent } from '@ui/forms/select/multi-select/multi-select.component'
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component'
import { NumberboxComponent } from '@ui/forms/numberbox/numberbox.component'
import { SaveButtonComponent } from '@ui/button/save-button/save-button.component'
import { TextareaComponent } from '@ui/forms/textarea/textarea.component'
import { TextboxComponent } from '@ui/forms/textbox/textbox.component'
import { TexteditorComponent } from '@ui/forms/texteditor/texteditor.component'
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { DatePeriodModule } from '@shared/components/date-period/date-period.module';
import { InboundEmailRoutingModule } from './inbound-email-routing.module';
import { InboundEmailComponent } from './components/inbound-email/inbound-email.component';
import { InboundEmailGridComponent } from './components/inbound-email-grid/inbound-email-grid.component';
import { InboundEmailGridFilterComponent } from './components/inbound-email-grid-filter/inbound-email-grid-filter.component';
import { InboundEmailDetailDialogComponent } from './components/inbound-email-detail-dialog/inbound-email-detail-dialog.component';
import { ByteFormatterPipe } from '@shared/pipes';

@NgModule({
  declarations: [
    InboundEmailComponent,
    InboundEmailGridComponent,
    InboundEmailGridFilterComponent,
    InboundEmailDetailDialogComponent,
  ],
  imports: [
    InboundEmailRoutingModule,
    CommonModule,
    SharedModule,
    ButtonComponent,
    ReactiveFormsModule,
    GridWrapperComponent,
    MultiTabWrapperComponent,
    TextboxComponent,
    ToolbarComponent,
    ExpansionPanelComponent,
    DatePeriodModule,
    MultiSelectComponent,
    SaveButtonComponent,
    DialogComponent,
    NumberboxComponent,
    TexteditorComponent,
    TextareaComponent,
    ByteFormatterPipe,
    IconButtonComponent,
    ButtonComponent,
    InstructionComponent,
  ],
})
export class InboundEmailModule {}
