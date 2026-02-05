import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { LeisureCodeTypesComponent } from './components/leisure-code-types/leisure-code-types.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: LeisureCodeTypesComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class LeisureCodeTypesRoutingModule {}
