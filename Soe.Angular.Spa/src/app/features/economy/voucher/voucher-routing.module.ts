import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { VoucherComponent } from './components/voucher/voucher.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: VoucherComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class VoucherRoutingModule {}
