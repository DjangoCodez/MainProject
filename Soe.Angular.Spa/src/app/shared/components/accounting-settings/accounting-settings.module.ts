import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { SharedModule } from '@shared/shared.module';
import { ButtonComponent } from '@ui/button/button/button.component'
import { ExpansionPanelComponent } from '@ui/expansion-panel/expansion-panel.component'
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component'
import { IconModule } from '@ui/icon/icon.module'
import { InstructionComponent } from '@ui/instruction/instruction.component'
import { LabelComponent } from '@ui/label/label.component'
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { AccountingSettingsComponent } from './accounting-settings/accounting-settings.component';

@NgModule({
  declarations: [AccountingSettingsComponent],
  exports: [AccountingSettingsComponent],
  imports: [
    CommonModule,
    SharedModule,
    TranslateModule,
    ReactiveFormsModule,
    ExpansionPanelComponent,
    GridWrapperComponent,
    ButtonComponent,
    LabelComponent,
    IconModule,
    InstructionComponent,
    ToolbarComponent,
  ],
})
export class AccountingSettingsModule {}
