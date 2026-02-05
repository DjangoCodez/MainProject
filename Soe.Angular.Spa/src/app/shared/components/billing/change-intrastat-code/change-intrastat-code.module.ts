import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatStepperModule } from '@angular/material/stepper';
import { TranslateModule } from '@ngx-translate/core';
import { SharedModule } from '@shared/shared.module';
import { ButtonComponent } from '@ui/button/button/button.component'
import { DialogComponent } from '@ui/dialog/dialog/dialog.component'
import { GridComponent } from '@ui/grid/grid.component'
import { IconModule } from '@ui/icon/icon.module'
import { InstructionComponent } from '@ui/instruction/instruction.component'
import { LabelComponent } from '@ui/label/label.component'
import { SaveButtonComponent } from '@ui/button/save-button/save-button.component'
import { SelectComponent } from '@ui/forms/select/select.component'
import { TextboxComponent } from '@ui/forms/textbox/textbox.component';
import { ChangeIntrastatCodeGridComponent } from './components/change-intrastat-code-grid/change-intrastat-code-grid.component';
import { ChangeIntrastatCodeComponent } from './components/change-intrastat-code/change-intrastat-code.component';

@NgModule({
  declarations: [
    ChangeIntrastatCodeComponent,
    ChangeIntrastatCodeGridComponent,
  ],
  imports: [
    CommonModule,
    DialogComponent,
    ReactiveFormsModule,
    ButtonComponent,
    SaveButtonComponent,
    LabelComponent,
    MatStepperModule,
    MatIconModule,
    IconModule,
    TranslateModule,
    SelectComponent,
    TextboxComponent,
    InstructionComponent,
    GridComponent,
    SharedModule,
  ],
  exports: [ChangeIntrastatCodeComponent, ChangeIntrastatCodeGridComponent],
})
export class ChangeIntrastatCodeModule {}
