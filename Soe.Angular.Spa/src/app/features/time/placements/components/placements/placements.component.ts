import { Component } from '@angular/core';
import { PlacementsGridComponent } from '../placements-grid/placements-grid.component';
import { PlacementsForm } from '../../models/placements-form.model';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';

@Component({
  templateUrl: './placements.component.html',
  standalone: false,
})
export class PlacementsComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: PlacementsGridComponent,
      FormClass: PlacementsForm,
      gridTabLabel: 'time.schedule.activate',
    },
  ];
}
