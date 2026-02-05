import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SharedModule } from '@shared/shared.module';
import { ButtonComponent } from '@ui/button/button/button.component'
import { CheckboxComponent } from '@ui/forms/checkbox/checkbox.component'
import { DatepickerComponent } from '@ui/forms/datepicker/datepicker.component'
import { EditFooterComponent } from '@ui/footer/edit-footer/edit-footer.component'
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component'
import { LabelComponent } from '@ui/label/label.component'
import { MultiSelectComponent } from '@ui/forms/select/multi-select/multi-select.component'
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component'
import { SelectComponent } from '@ui/forms/select/select.component'
import { TextboxComponent } from '@ui/forms/textbox/textbox.component'
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { ProductStatisticsRoutingModule } from './product-statistics-routing.module';
import { ProductStatisticsComponent } from './components/product-statistics/product-statistics.component';
import { ProductStatisticsGridComponent } from './components/product-statistics-grid/product-statistics-grid.component';
import { ProductStatisticsGridFilterComponent } from './components/product-statistics-grid-filter/product-statistics-grid-filter.component';
import { ReactiveFormsModule } from '@angular/forms';

@NgModule({
  declarations: [
    ProductStatisticsComponent,
    ProductStatisticsGridComponent,
    ProductStatisticsGridFilterComponent,
  ],
  imports: [
    CommonModule,
    SharedModule,
    ProductStatisticsRoutingModule,
    ButtonComponent,
    CheckboxComponent,
    DatepickerComponent,
    EditFooterComponent,
    GridWrapperComponent,
    SelectComponent,
    MultiSelectComponent,
    MultiTabWrapperComponent,
    TextboxComponent,
    ReactiveFormsModule,
    ToolbarComponent,
    LabelComponent,
  ],
})
export class ProductStatisticsModule {}
