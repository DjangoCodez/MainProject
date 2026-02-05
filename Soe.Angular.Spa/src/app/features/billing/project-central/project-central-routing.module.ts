import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ProjectCentralComponent } from './components/project-central/project-central.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: ProjectCentralComponent
  }
];

@NgModule({
  declarations: [],
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class ProjectCentralRoutingModule { }
