import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { HolidaysComponent } from './components/holidays/holidays.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: HolidaysComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class HolidaysRoutingModule {}
