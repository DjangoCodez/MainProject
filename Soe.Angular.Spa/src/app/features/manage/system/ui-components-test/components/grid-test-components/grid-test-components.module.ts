import { NgModule } from '@angular/core';
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component';
import { EditableGridTestComponent } from './editable-grid.component';
import { GroupedHeadersGridTestComponent } from './grouped-headers-grid.component';
import { MasterDetailGridTestComponent } from './master-detail-grid.component';

@NgModule({
  declarations: [
    MasterDetailGridTestComponent,
    GroupedHeadersGridTestComponent,
    EditableGridTestComponent,
  ],
  imports: [GridWrapperComponent],
  exports: [
    MasterDetailGridTestComponent,
    GroupedHeadersGridTestComponent,
    EditableGridTestComponent,
  ],
})
export class GridTestComponentsModule {}
