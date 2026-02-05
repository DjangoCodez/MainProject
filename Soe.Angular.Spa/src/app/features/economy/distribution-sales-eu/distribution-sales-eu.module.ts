import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';

import { SharedModule } from '@shared/shared.module';
import { ButtonComponent } from '@ui/button/button/button.component';
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component';
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component';
import { RadioComponent } from '@ui/forms/radio/radio.component';
import { SelectComponent } from '@ui/forms/select/select.component';
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { DistributionSalesEuGridFilterComponent } from './components/distribution-sales-eu-grid/distribution-sales-eu-grid-filter/distribution-sales-eu-grid-filter.component';
import { DistributionSalesEuGridComponent } from './components/distribution-sales-eu-grid/distribution-sales-eu-grid.component';
import { DistributionSalesEuComponent } from './components/distribution-sales-eu/distribution-sales-eu.component';
import { DistributionSalesEuRoutingModule } from './distribution-sales-eu-routing.module';
import { ReactiveFormsModule } from '@angular/forms';

@NgModule({
  declarations: [
    DistributionSalesEuComponent,
    DistributionSalesEuGridComponent,
    DistributionSalesEuGridFilterComponent,
  ],
  imports: [
    CommonModule,
    DistributionSalesEuRoutingModule,
    SharedModule,
    ButtonComponent,
    GridWrapperComponent,
    MultiTabWrapperComponent,
    ToolbarComponent,
    SelectComponent,
    ReactiveFormsModule,
    RadioComponent,
  ],
})
export class DistributionSalesEuModule {}
