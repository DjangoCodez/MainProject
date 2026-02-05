import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { CompanyGroupAdministrationComponent } from './components/company-group-administration/company-group-administration.component';

const routes: Routes = [{
  path: 'default.aspx',
  component: CompanyGroupAdministrationComponent
}]

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class CompanyGroupAdministrationRoutingModule { }
