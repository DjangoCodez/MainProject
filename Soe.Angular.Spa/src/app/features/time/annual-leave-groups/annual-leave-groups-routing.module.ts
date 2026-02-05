import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AnnualLeaveGroupsComponent } from './components/annual-leave-groups/annual-leave-groups.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: AnnualLeaveGroupsComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class AnnualLeaveGroupsRoutingModule {}
