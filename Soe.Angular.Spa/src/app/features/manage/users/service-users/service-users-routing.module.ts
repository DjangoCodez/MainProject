import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ServiceUsersComponent } from './components/service-users/service-users.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: ServiceUsersComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class ServiceUserRoutingModule {}
