import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ButtonComponent } from '@ui/button/button/button.component'
import { EditFooterComponent } from '@ui/footer/edit-footer/edit-footer.component'
import { ExpansionPanelComponent } from '@ui/expansion-panel/expansion-panel.component'
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component'
import { InstructionComponent } from '@ui/instruction/instruction.component'
import { LabelComponent } from '@ui/label/label.component'
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component'
import { TextboxComponent } from '@ui/forms/textbox/textbox.component'
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { AccountDistributionAutoRoutingModule } from './account-distribution-auto-routing.module';
import { SharedModule } from '@shared/shared.module';
import { ReactiveFormsModule } from '@angular/forms';
import { AccountDistributionAutoComponent } from './components/account-distribution-auto.component';
import { AccountDistributionModule } from '../account-distribution/account-distribution.module';

@NgModule({
  declarations: [AccountDistributionAutoComponent],
  imports: [
    CommonModule,
    AccountDistributionAutoRoutingModule,
    SharedModule,
    MultiTabWrapperComponent,
    ToolbarComponent,
    TextboxComponent,
    ReactiveFormsModule,
    EditFooterComponent,
    GridWrapperComponent,
    ButtonComponent,
    ExpansionPanelComponent,
    LabelComponent,
    InstructionComponent,
    AccountDistributionModule,
  ],
})
export class AccountDistributionAutoModule {}
