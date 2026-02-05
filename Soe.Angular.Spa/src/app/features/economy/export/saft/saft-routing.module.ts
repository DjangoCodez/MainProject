import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { SaftComponent } from './components/saft/saft.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: SaftComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class SaftRoutingModule {}
