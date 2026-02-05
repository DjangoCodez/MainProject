import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { ImportPriceListRoutingModule } from './import-price-list-routing.module';
import { ButtonComponent } from '@ui/button/button/button.component'
import { DialogComponent } from '@ui/dialog/dialog/dialog.component'
import { FileUploadComponent } from '@ui/forms/file-upload/file-upload.component'
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component'
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component'
import { SaveButtonComponent } from '@ui/button/save-button/save-button.component'
import { SelectComponent } from '@ui/forms/select/select.component'
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { ImportPriceListComponent } from './components/import-price-list/import-price-list.component';
import { ReactiveFormsModule } from '@angular/forms';
import { SharedModule } from '@shared/shared.module';
import { ImportPriceListGridComponent } from './components/import-price-list-grid/import-price-list-grid.component';
import { ImportPriceListUploadComponent } from './components/import-price-list-upload/import-price-list-upload.component';

@NgModule({
  declarations: [
    ImportPriceListComponent,
    ImportPriceListGridComponent,
    ImportPriceListUploadComponent,
  ],
  imports: [
    CommonModule,
    SharedModule,
    ImportPriceListRoutingModule,
    GridWrapperComponent,
    MultiTabWrapperComponent,
    ToolbarComponent,
    ReactiveFormsModule,
    FileUploadComponent,
    SelectComponent,
    DialogComponent,
    ButtonComponent,
    SaveButtonComponent,
  ],
})
export class ImportPriceListModule {}
