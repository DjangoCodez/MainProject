import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Routes } from '@angular/router';
import { ContractGroupsComponent } from './components/contract-groups/contract-groups.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: ContractGroupsComponent,
  }
]

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class ContractGroupsRoutingModule { }
