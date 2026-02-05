import { Component } from '@angular/core';
import { StaffingNeedsLocationsForm } from '../../models/staffing-needs-location-form.model';
import { StaffingNeedsLocationsGridComponent } from '../../components/staffing-needs-loactions-grid/staffing-needs-locations-grid.component';
import { StaffingNeedsLocationsEditComponent } from '../../components/staffing-needs-locations-edit/staffing-needs-locations-edit.component';
import { StaffingNeedsLocationGroupsForm } from '../../models/staffing-needs-location-group-form.model';
import { StaffingNeedsLocationGroupsEditComponent } from '../../components/staffing-needs-location-groups-edit/staffing-needs-location-groups-edit.component';
import { StaffingNeedsLocationGroupsGridComponent } from '../../components/staffing-needs-location-groups-grid/staffing-needs-location-groups-grid.component';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';

@Component({
  selector: 'staffing-needs-locations',
  templateUrl:
    '../../../../../shared/ui-components/tab/multi-tab-wrapper/multi-tab-wrapper-template.html',
  standalone: false,
})
export class StaffingNeedsLocationsComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: StaffingNeedsLocationsGridComponent,
      editComponent: StaffingNeedsLocationsEditComponent,
      FormClass: StaffingNeedsLocationsForm,
      gridTabLabel:
        'time.schedule.staffingneedslocation.staffingneedslocations',
      editTabLabel: 'time.schedule.staffingneedslocation.staffingneedslocation',
      createTabLabel: 'time.schedule.staffingneedslocation.new',
    },
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
