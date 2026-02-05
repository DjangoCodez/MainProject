import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { LeisureCodesComponent } from './components/leisure-codes/leisure-codes.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: LeisureCodesComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class LeisureCodesRoutingModule {}
