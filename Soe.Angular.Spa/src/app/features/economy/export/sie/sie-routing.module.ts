import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { SieComponent } from './components/sie/sie.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: SieComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class SieRoutingModule {}
