import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { SharedModule } from '@shared/shared.module';
import { ButtonComponent } from '@ui/button/button/button.component'
import { DatepickerComponent } from '@ui/forms/datepicker/datepicker.component'
import { DialogComponent } from '@ui/dialog/dialog/dialog.component'
import { FileUploadComponent } from '@ui/forms/file-upload/file-upload.component'
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component'
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component'
import { SaveButtonComponent } from '@ui/button/save-button/save-button.component'
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { CommodityCodesRoutingModule } from './commodity-codes-routing.module';
import { CommodityCodesGridComponent } from './components/commodity-codes-grid/commodity-codes-grid/commodity-codes-grid.component';
import { CommodityCodesUploadComponent } from './components/commodity-codes-upload/commodity-codes-upload.component';
import { CommodityCodesComponent } from './components/commodity-codes/commodity-codes/commodity-codes.component';

@NgModule({
  declarations: [
    CommodityCodesComponent,
    CommodityCodesGridComponent,
    CommodityCodesUploadComponent,
  ],
  imports: [
    CommonModule,
    SharedModule,
    CommodityCodesRoutingModule,
    GridWrapperComponent,
    MultiTabWrapperComponent,
    ToolbarComponent,
    DatepickerComponent,
    FileUploadComponent,
    ReactiveFormsModule,
    ButtonComponent,
    SaveButtonComponent,
    DialogComponent,
  ],
})
export class CommodityCodesModule {}
