import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { PayrollPriceTypesComponent } from './components/payroll-price-types/payroll-price-types.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: PayrollPriceTypesComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class PayrollPriceTypesRoutingModule {}
