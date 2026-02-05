import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ExcelImportComponent } from './components/excel-import/excel-import.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: ExcelImportComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class ExcelImportRoutingModule {}
