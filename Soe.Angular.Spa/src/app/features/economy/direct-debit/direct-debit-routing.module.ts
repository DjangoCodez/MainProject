import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { DirectDebitComponent } from './components/direct-debit/direct-debit.component';

const routes: Routes = [
  {
    path: 'exportcustomerpaymentservices.aspx',
    component: DirectDebitComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class DirectDebitRoutingModule {}
