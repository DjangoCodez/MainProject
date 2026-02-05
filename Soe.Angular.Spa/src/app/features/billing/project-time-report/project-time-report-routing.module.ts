import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ProjectTimeReportComponent } from './components/project-time-report/project-time-report.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: ProjectTimeReportComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class ProjectTimeReportRoutingModule {}
