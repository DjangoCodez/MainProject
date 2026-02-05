import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AvailabilityComponent } from './components/availability/availability.component';

const routes: Routes = [
  { path: 'default.aspx', component: AvailabilityComponent },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class AvailabilityRoutingModule {}
