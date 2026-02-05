import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ButtonComponent } from '@ui/button/button/button.component'
import { EditFooterComponent } from '@ui/footer/edit-footer/edit-footer.component'
import { ExpansionPanelComponent } from '@ui/expansion-panel/expansion-panel.component'
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component'
import { LabelComponent } from '@ui/label/label.component'
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component'
import { NumberboxComponent } from '@ui/forms/numberbox/numberbox.component'
import { TextboxComponent } from '@ui/forms/textbox/textbox.component'
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { CompanyGroupMappingsRoutingModule } from './company-group-mappings-routing.module';
import { CompanyGroupMappingsEditComponent } from './company-group-mappings-edit/company-group-mappings-edit.component';
import { CompanyGroupMappingsComponent } from './company-group-mappings/company-group-mappings.component';
import { CompanyGroupMappingsGridComponent } from './company-group-mappings-grid/company-group-mappings-grid.component';
import { SharedModule } from '@shared/shared.module';
import { ReactiveFormsModule } from '@angular/forms';
import { CompanyGroupMappingRowsComponent } from './company-group-mappings-edit/company-group-mapping-rows/company-group-mapping-rows.component';

@NgModule({
  declarations: [
    CompanyGroupMappingsComponent,
    CompanyGroupMappingsEditComponent,
    CompanyGroupMappingsGridComponent,
    CompanyGroupMappingRowsComponent,
  ],
  imports: [
    CommonModule,
    CompanyGroupMappingsRoutingModule,
    SharedModule,
    CommonModule,
    MultiTabWrapperComponent,
    ToolbarComponent,
    LabelComponent,
    TextboxComponent,
    ReactiveFormsModule,
    NumberboxComponent,
    EditFooterComponent,
    GridWrapperComponent,
    ButtonComponent,
    ExpansionPanelComponent,
  ],
})
export class CompanyGroupMappingsModule {}
