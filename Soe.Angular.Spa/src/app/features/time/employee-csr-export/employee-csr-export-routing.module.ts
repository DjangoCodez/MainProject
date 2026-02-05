import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { EmployeeCsrExportComponent } from './components/employee-csr-export/employee-csr-export.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: EmployeeCsrExportComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class EmployeeCsrExportRoutingModule {}
