import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AbsenceRequestsComponent } from './components/absence-requests/absence-requests.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: AbsenceRequestsComponent,
  },
];
@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class AbsenceRequestsRoutingModule {}
