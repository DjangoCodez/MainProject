import { Component } from '@angular/core';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';
import { AvailabilityGridComponent } from '../availability-grid/availability-grid.component';

@Component({
  selector: 'soe-availability',
  templateUrl: './availability.component.html',
  standalone: false,
})
export class AvailabilityComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: AvailabilityGridComponent,
      gridTabLabel: 'time.schedule.availability',
    },
  ];
}
