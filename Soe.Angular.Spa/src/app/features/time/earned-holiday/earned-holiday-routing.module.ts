import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { EarnedHolidayComponent } from './components/earned-holiday/earned-holiday.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: EarnedHolidayComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class EarnedHolidayRoutingModule { }
