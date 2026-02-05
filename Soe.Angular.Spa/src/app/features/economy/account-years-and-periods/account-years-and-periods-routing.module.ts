import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { YearsAndPeriodsComponent } from './components/account-years-and-periods/account-years-and-periods.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: YearsAndPeriodsComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class YearsAndPeriodsRoutingModule {}
