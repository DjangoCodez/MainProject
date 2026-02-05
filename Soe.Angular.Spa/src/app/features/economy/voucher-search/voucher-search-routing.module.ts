import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { VoucherSearchComponent } from './components/voucher-search/voucher-search.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: VoucherSearchComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class VoucherSearchRoutingModule {}
