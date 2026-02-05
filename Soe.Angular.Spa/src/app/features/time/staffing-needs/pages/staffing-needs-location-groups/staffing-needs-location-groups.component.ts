import { Component } from '@angular/core';
import { StaffingNeedsLocationGroupsForm } from '../../models/staffing-needs-location-group-form.model';
import { StaffingNeedsLocationGroupsGridComponent } from '../../components/staffing-needs-location-groups-grid/staffing-needs-location-groups-grid.component';
import { StaffingNeedsLocationGroupsEditComponent } from '../../components/staffing-needs-location-groups-edit/staffing-needs-location-groups-edit.component';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';

@Component({
  selector: 'staffing-needs-location-groups',
  templateUrl:
    '../../../../../shared/ui-components/tab/multi-tab-wrapper/multi-tab-wrapper-template.html',
  standalone: false,
})
export class StaffingNeedsLocationGroupsComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: StaffingNeedsLocationGroupsGridComponent,
      editComponent: StaffingNeedsLocationGroupsEditComponent,
      FormClass: StaffingNeedsLocationGroupsForm,
      gridTabLabel:
        'time.schedule.staffingneedslocationgroup.staffingneedslocationgroups',
      editTabLabel:
        'time.schedule.staffingneedslocationgroup.staffingneedslocationgroup',
      createTabLabel: 'time.schedule.staffingneedslocationgroup.new',
    },
  ];
}
