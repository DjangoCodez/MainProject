import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { IntrastatExportComponent } from './components/intrastat-export/intrastat-export.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: IntrastatExportComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class IntrastatExportRoutingModule {}
