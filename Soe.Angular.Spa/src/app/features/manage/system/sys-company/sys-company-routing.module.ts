import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { SysCompanyComponent } from './components/sys-company/sys-company.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: SysCompanyComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class SysCompanyRoutingModule {}
