import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SharedModule } from '@shared/shared.module';
import { SalesStatisticsRoutingModule } from './sales-statistics-routing.module';
import { ButtonComponent } from '@ui/button/button/button.component'
import { DatepickerComponent } from '@ui/forms/datepicker/datepicker.component'
import { EditFooterComponent } from '@ui/footer/edit-footer/edit-footer.component'
import { ExpansionPanelComponent } from '@ui/expansion-panel/expansion-panel.component'
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component'
import { LabelComponent } from '@ui/label/label.component'
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component'
import { SelectComponent } from '@ui/forms/select/select.component'
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { ReactiveFormsModule } from '@angular/forms';
import { SalesStatisticsGridComponent } from '../sales-statistics/components/sales-statistics-grid/sales-statistics-grid.component';
import { SalesStatisticsComponent } from './components/sales-statistics/sales-statistics.component';
import { SalesStatisticsGridFilterComponent } from './components/sales-statistics-grid-filter/sales-statistics-grid-filter.component';

@NgModule({
  declarations: [
    SalesStatisticsGridComponent,
    SalesStatisticsComponent,
    SalesStatisticsGridFilterComponent,
  ],
  imports: [
    CommonModule,
    SharedModule,
    SalesStatisticsRoutingModule,
    ExpansionPanelComponent,
    DatepickerComponent,
    ButtonComponent,
    EditFooterComponent,
    GridWrapperComponent,
    SelectComponent,
    MultiTabWrapperComponent,
    ReactiveFormsModule,
    ToolbarComponent,
    LabelComponent,
  ],
})
export class SalesStatisticsModule {}
