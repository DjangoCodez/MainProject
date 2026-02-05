import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ConnectImporterRoutingModule } from './connect-importer-routing.module';
import { ConnectImporterGridComponent } from './components/connect-importer-grid/connect-importer-grid.component';
import { ConnectImporterEditComponent } from './components/connect-importer-edit/connect-importer-edit.component';
import { ConnectImporterComponent } from './components/connect-importer/connect-importer.component';
import { ConnectImporterGridFilterComponent } from './components/connect-importer-grid/connect-importer-grid-filter/connect-importer-grid-filter.component';
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component'
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component'
import { SelectComponent } from '@ui/forms/select/select.component'
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { ReactiveFormsModule } from '@angular/forms';
import { SharedModule } from '@shared/shared.module';
import { ImportRowsModule } from '@shared/components/import-rows/import-rows.module';

@NgModule({
  declarations: [
    ConnectImporterComponent,
    ConnectImporterEditComponent,
    ConnectImporterGridComponent,
    ConnectImporterGridFilterComponent,
  ],
  imports: [
    CommonModule,
    SharedModule,
    ConnectImporterRoutingModule,
    GridWrapperComponent,
    MultiTabWrapperComponent,
    ToolbarComponent,
    SelectComponent,
    ReactiveFormsModule,
    ImportRowsModule,
  ],
})
export class ConnectImporterModule {}
