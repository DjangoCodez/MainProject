import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { HalfdaysComponent } from './components/halfdays/halfdays.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: HalfdaysComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class HalfdaysRoutingModule {}
