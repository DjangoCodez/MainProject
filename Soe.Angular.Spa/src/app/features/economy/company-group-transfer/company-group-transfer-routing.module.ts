import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { CompanyGroupTransferComponent } from './components/company-group-transfer/company-group-transfer.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: CompanyGroupTransferComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class CompanyGroupTransferRoutingModule {}
