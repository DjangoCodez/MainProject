import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { PriceBasedMarkupRoutingModule } from './price-based-markup-routing.module';
import { SharedModule } from '@shared/shared.module';
import { PriceBasedMarkupComponent } from './components/price-based-markup/price-based-markup.component';
import { ButtonComponent } from '@ui/button/button/button.component'
import { EditFooterComponent } from '@ui/footer/edit-footer/edit-footer.component'
import { ExpansionPanelComponent } from '@ui/expansion-panel/expansion-panel.component'
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component'
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component'
import { TextboxComponent } from '@ui/forms/textbox/textbox.component'
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { PriceBasedMarkupGridComponent } from './components/price-based-markup-grid/price-based-markup-grid.component';
import { ReactiveFormsModule } from '@angular/forms';

@NgModule({
  declarations: [PriceBasedMarkupComponent, PriceBasedMarkupGridComponent],
  imports: [
    SharedModule,
    CommonModule,
    PriceBasedMarkupRoutingModule,
    ReactiveFormsModule,
    ButtonComponent,
    GridWrapperComponent,
    TextboxComponent,
    ToolbarComponent,
    MultiTabWrapperComponent,
    EditFooterComponent,
    ExpansionPanelComponent,
  ],
})
export class PriceBasedMarkupModule {}
