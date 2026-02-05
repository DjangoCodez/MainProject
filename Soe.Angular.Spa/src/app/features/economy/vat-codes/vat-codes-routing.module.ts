import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { VatCodesComponent } from './components/vat-codes/vat-codes.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: VatCodesComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class VatCodesRoutingModule {}
