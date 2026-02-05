import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { SharedModule } from '@shared/shared.module';
import { ButtonComponent } from '@ui/button/button/button.component'
import { EditFooterComponent } from '@ui/footer/edit-footer/edit-footer.component'
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component'
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component'
import { TextboxComponent } from '@ui/forms/textbox/textbox.component'
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { PayrollLevelsEditComponent } from './components/payroll-levels-edit/payroll-levels-edit.component';
import { PayrollLevelsGridComponent } from './components/payroll-levels-grid/payroll-levels-grid.component';
import { PayrollLevelsComponent } from './components/payroll-levels/payroll-levels.component';
import { PayrollLevelsRoutingModule } from './payroll-levels-routing.module';

@NgModule({
  declarations: [
    PayrollLevelsComponent,
    PayrollLevelsGridComponent,
    PayrollLevelsEditComponent,
  ],
  imports: [
    CommonModule,
    SharedModule,
    ReactiveFormsModule,
    PayrollLevelsRoutingModule,
    MultiTabWrapperComponent,
    GridWrapperComponent,
    ToolbarComponent,
    ButtonComponent,
    TextboxComponent,
    EditFooterComponent,
  ],
})
export class PayrollLevelsModule {}
