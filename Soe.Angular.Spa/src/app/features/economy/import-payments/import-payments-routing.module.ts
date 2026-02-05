import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ImportPaymentsComponent } from './components/import-payments/import-payments.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: ImportPaymentsComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class ImportPaymentsRoutingModule {}
