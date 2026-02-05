import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { DrillDownReportsComponent } from './components/drill-down-reports/drill-down-reports.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: DrillDownReportsComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class DrillDownReportsRoutingModule {}
