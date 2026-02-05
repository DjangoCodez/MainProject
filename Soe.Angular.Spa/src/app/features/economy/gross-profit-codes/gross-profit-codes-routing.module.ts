import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { GrossProfitCodesComponent } from './components/gross-profit-codes/gross-profit-codes.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: GrossProfitCodesComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class GrossProfitCodesRoutingModule {}
