import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ProjectCentralComponent } from './components/project-central/project-central.component';
import { ProjectCentralGridComponent } from './components/project-central-grid/project-central-grid.component';
import { SharedModule } from '@shared/shared.module';
import { ProjectCentralRoutingModule } from './project-central-routing.module';
import { ButtonComponent } from '@ui/button/button/button.component'
import { CheckboxComponent } from '@ui/forms/checkbox/checkbox.component'
import { DatepickerComponent } from '@ui/forms/datepicker/datepicker.component'
import { ExpansionPanelComponent } from '@ui/expansion-panel/expansion-panel.component'
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component'
import { IconButtonComponent } from '@ui/button/icon-button/icon-button.component'
import { LabelComponent } from '@ui/label/label.component'
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component'
import { TextboxComponent } from '@ui/forms/textbox/textbox.component'
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { ProjectCentralMainComponent } from './components/project-central-main/project-central-main.component';
import { ReactiveFormsModule } from '@angular/forms';
import { ToolbarButtonComponent } from '@ui/toolbar/toolbar-button/toolbar-button.component';
import { ProjectCentralSupplierInvoicesGridComponent } from './components/project-central-supplier-invoices-grid/project-central-supplier-invoices-grid.component';
import { ProjectCentralDataService } from './services/project-central-data.service';
import { ProjectCentralProductRowsComponent } from './components/project-central-product-rows/project-central-product-rows.component';

@NgModule({
  declarations: [
    ProjectCentralComponent,
    ProjectCentralGridComponent,
    ProjectCentralMainComponent,
    ProjectCentralSupplierInvoicesGridComponent,
    ProjectCentralProductRowsComponent,
  ],
  imports: [
    CommonModule,
    SharedModule,
    ProjectCentralRoutingModule,
    MultiTabWrapperComponent,
    ToolbarComponent,
    GridWrapperComponent,
    ReactiveFormsModule,
    LabelComponent,
    TextboxComponent,
    ExpansionPanelComponent,
    DatepickerComponent,
    CheckboxComponent,
    ButtonComponent,
    IconButtonComponent,
    ToolbarButtonComponent,
  ],
  providers: [ProjectCentralDataService],
})
export class ProjectCentralModule {}
