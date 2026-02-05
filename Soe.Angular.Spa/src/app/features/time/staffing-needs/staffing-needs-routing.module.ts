import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { StaffingNeedsLocationGroupsComponent } from './pages/staffing-needs-location-groups/staffing-needs-location-groups.component';
import { StaffingNeedsLocationsComponent } from './pages/staffing-needs-locations/staffing-needs-locations.component';
import { StaffingNeedsRulesComponent } from './pages/staffing-needs-rules/staffing-needs-rules.component';

const routes: Routes = [
  {
    path: 'locationgroups/default.aspx',
    component: StaffingNeedsLocationGroupsComponent,
  },
  {
    path: 'locations/default.aspx',
    component: StaffingNeedsLocationsComponent,
  },
  {
    path: 'rules/default.aspx',
    component: StaffingNeedsRulesComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class StaffingNeedsRoutingModule {}
