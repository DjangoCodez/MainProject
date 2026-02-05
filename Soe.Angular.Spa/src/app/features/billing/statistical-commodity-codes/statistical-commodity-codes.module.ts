import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ButtonComponent } from '@ui/button/button/button.component'
import { EditFooterComponent } from '@ui/footer/edit-footer/edit-footer.component'
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component'
import { LabelComponent } from '@ui/label/label.component'
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component'
import { TextboxComponent } from '@ui/forms/textbox/textbox.component'
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { StatisticalCommodityCodesRoutingModule } from './statistical-commodity-codes-routing.module';
import { StatisticalCommodityCodesComponent } from './components/statistical-commodity-codes/statistical-commodity-codes.component';
import { StatisticalCommodityCodesGridComponent } from './components/statistical-commodity-codes-grid/statistical-commodity-codes-grid.component';
import { ReactiveFormsModule } from '@angular/forms';

@NgModule({
  declarations: [
    StatisticalCommodityCodesComponent,
    StatisticalCommodityCodesGridComponent,
  ],
  imports: [
    CommonModule,
    StatisticalCommodityCodesRoutingModule,
    MultiTabWrapperComponent,
    ToolbarComponent,
    LabelComponent,
    TextboxComponent,
    ReactiveFormsModule,
    EditFooterComponent,
    GridWrapperComponent,
    ButtonComponent,
  ],
})
export class StatisticalCommodityCodesModule {}
