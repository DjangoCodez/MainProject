import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { SharedModule } from '@shared/shared.module';
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component';
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component';
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { AvailabilityRoutingModule } from './availability-routing.module';
import { AvailabilityComponent } from './components/availability/availability.component';
import { AvailabilityGridComponent } from './components/availability-grid/availability-grid.component';

@NgModule({
  declarations: [
    AvailabilityComponent,
    AvailabilityGridComponent,
  ],
  imports: [
    CommonModule,
    SharedModule,
    FormsModule,
    ReactiveFormsModule,
    AvailabilityRoutingModule,
    GridWrapperComponent,
    MultiTabWrapperComponent,
    ToolbarComponent,
  ],
})
export class AvailabilityModule {}
