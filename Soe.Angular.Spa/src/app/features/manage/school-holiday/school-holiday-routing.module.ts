import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { SchoolHolidayComponent } from './components/school-holiday/school-holiday.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: SchoolHolidayComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class SchoolHolidayRoutingModule {}
