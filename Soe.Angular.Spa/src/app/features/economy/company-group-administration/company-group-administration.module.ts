import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CompanyGroupAdministrationComponent } from './components/company-group-administration/company-group-administration.component';
import { CompanyGroupAdministrationEditComponent } from './components/company-group-administration-edit/company-group-administration-edit.component';
import { CompanyGroupAdministrationGridComponent } from './components/company-group-administration-grid/company-group-administration-grid.component';
import { CompanyGroupAdministrationRoutingModule } from './company-group-administration-routing.module';
import { ButtonComponent } from '@ui/button/button/button.component'
import { CheckboxComponent } from '@ui/forms/checkbox/checkbox.component'
import { EditFooterComponent } from '@ui/footer/edit-footer/edit-footer.component'
import { ExpansionPanelComponent } from '@ui/expansion-panel/expansion-panel.component'
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component'
import { LabelComponent } from '@ui/label/label.component'
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component'
import { NumberboxComponent } from '@ui/forms/numberbox/numberbox.component'
import { SelectComponent } from '@ui/forms/select/select.component'
import { TextboxComponent } from '@ui/forms/textbox/textbox.component'
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { ReactiveFormsModule } from '@angular/forms';
import { SharedModule } from '@shared/shared.module';

@NgModule({
  declarations: [
    CompanyGroupAdministrationComponent,
    CompanyGroupAdministrationEditComponent,
    CompanyGroupAdministrationGridComponent,
  ],
  imports: [
    CommonModule,
    SharedModule,
    CompanyGroupAdministrationRoutingModule,
    ButtonComponent,
    CheckboxComponent,
    GridWrapperComponent,
    SelectComponent,
    ReactiveFormsModule,
    MultiTabWrapperComponent,
    TextboxComponent,
    NumberboxComponent,
    ToolbarComponent,
    EditFooterComponent,
    LabelComponent,
    ExpansionPanelComponent,
  ],
})
export class CompanyGroupAdministrationModule {}
