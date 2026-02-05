import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { SignatoryContractComponent } from './components/signatory-contract/signatory-contract.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: SignatoryContractComponent,
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class SignatoryContractRoutingModule { }
