import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { CompanyGroupMappingsComponent } from './company-group-mappings/company-group-mappings.component';

const routes: Routes = [
  { path: 'default.aspx', component: CompanyGroupMappingsComponent },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class CompanyGroupMappingsRoutingModule {}
