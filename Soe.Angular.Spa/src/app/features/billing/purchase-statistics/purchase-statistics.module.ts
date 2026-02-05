import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PurchaseStatisticsRoutingModule } from './purchase-statistics-routing.module';
import { PurchaseStatisticsComponent } from './components/purchase-statistics/purchase-statistics.component';
import { PurchaseStatisticsGridComponent } from './components/purchase-statistics-grid/purchase-statistics-grid.component';
import { PurchaseStatisticsGridFilterComponent } from './components/purchase-statistics-grid-filter/purchase-statistics-grid-filter.component';
import { SharedModule } from '@shared/shared.module';
import { ButtonComponent } from '@ui/button/button/button.component'
import { DatepickerComponent } from '@ui/forms/datepicker/datepicker.component'
import { EditFooterComponent } from '@ui/footer/edit-footer/edit-footer.component'
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component'
import { LabelComponent } from '@ui/label/label.component'
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component'
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { ReactiveFormsModule } from '@angular/forms';

@NgModule({
  declarations: [
    PurchaseStatisticsComponent,
    PurchaseStatisticsGridComponent,
    PurchaseStatisticsGridFilterComponent,
  ],
  imports: [
    CommonModule,
    SharedModule,
    PurchaseStatisticsRoutingModule,
    DatepickerComponent,
    ButtonComponent,
    EditFooterComponent,
    GridWrapperComponent,
    MultiTabWrapperComponent,
    ReactiveFormsModule,
    ToolbarComponent,
    LabelComponent,
  ],
})
export class PurchaseStatisticsModule {}
