import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ButtonComponent } from '@ui/button/button/button.component';
import { DeleteButtonComponent } from '@ui/button/delete-button/delete-button.component';
import { EditFooterComponent } from '@ui/footer/edit-footer/edit-footer.component';
import { ExpansionPanelComponent } from '@ui/expansion-panel/expansion-panel.component';
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component';
import { MenuButtonComponent } from '@ui/button/menu-button/menu-button.component';
import { MultiSelectComponent } from '@ui/forms/select/multi-select/multi-select.component';
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component';
import { RadioComponent } from '@ui/forms/radio/radio.component';
import { SelectComponent } from '@ui/forms/select/select.component';
import { TextboxComponent } from '@ui/forms/textbox/textbox.component';
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { ReactiveFormsModule } from '@angular/forms';
import { PriceOptimizationRoutingModule } from './price-optimization-routing.module';
import { PriceOptimizationComponent } from './components/price-optimization/price-optimization.component';
import { PriceOptimizationGridComponent } from './components/price-optimization-grid/price-optimization-grid.component';
import { PriceOptimizationGridHeaderComponent } from './components/price-optimization-grid-header/price-optimization-grid-header.component';
import { PriceOptimizationEditComponent } from './components/price-optimization-edit/price-optimization-edit.component';
import { SharedModule } from '@shared/shared.module';
import { PriceOptimizationEditItemGridComponent } from './components/price-optimization-edit/price-optimization-edit-item-grid/price-optimization-edit-item-grid.component';
import { InstructionComponent } from '@ui/instruction/instruction.component';
import { TraceRowsModule } from '@shared/components/trace-rows/trace-rows.module';
import { CheckboxComponent } from '@ui/forms/checkbox/checkbox.component';

@NgModule({
  declarations: [
    PriceOptimizationComponent,
    PriceOptimizationGridComponent,
    PriceOptimizationGridHeaderComponent,
    PriceOptimizationEditComponent,
    PriceOptimizationEditItemGridComponent,
  ],
  imports: [
    CommonModule,
    PriceOptimizationRoutingModule,
    GridWrapperComponent,
    MultiTabWrapperComponent,
    ToolbarComponent,
    ReactiveFormsModule,
    MultiSelectComponent,
    MenuButtonComponent,
    SelectComponent,
    ButtonComponent,
    EditFooterComponent,
    CheckboxComponent,
    TextboxComponent,
    ExpansionPanelComponent,
    DeleteButtonComponent,
    RadioComponent,
    SharedModule,
    TraceRowsModule,
    InstructionComponent,
  ],
})
export class PriceOptimizationModule {}
