import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { SharedModule } from '@shared/shared.module';
import { ButtonComponent } from '@ui/button/button/button.component'
import { EditFooterComponent } from '@ui/footer/edit-footer/edit-footer.component'
import { ExpansionPanelComponent } from '@ui/expansion-panel/expansion-panel.component'
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component'
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component'
import { SelectComponent } from '@ui/forms/select/select.component'
import { TextboxComponent } from '@ui/forms/textbox/textbox.component'
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { CollectiveAgreementsRoutingModule } from './collective-agreements-routing.module';
import { CollectiveAgreementsEditComponent } from './components/collective-agreements-edit/collective-agreements-edit.component';
import { CollectiveAgreementsGridComponent } from './components/collective-agreements-grid/collective-agreements-grid.component';
import { CollectiveAgreementsComponent } from './components/collective-agreements/collective-agreements.component';

@NgModule({
  declarations: [
    CollectiveAgreementsComponent,
    CollectiveAgreementsGridComponent,
    CollectiveAgreementsEditComponent,
  ],
  imports: [
    SharedModule,
    CommonModule,
    CollectiveAgreementsRoutingModule,
    ButtonComponent,
    EditFooterComponent,
    GridWrapperComponent,
    SelectComponent,
    ReactiveFormsModule,
    MultiTabWrapperComponent,
    TextboxComponent,
    ToolbarComponent,
    ExpansionPanelComponent,
  ],
})
export class CollectiveAgreementsModule {}
