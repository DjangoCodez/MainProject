import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { SkillTypesComponent } from './components/skill-types/skill-types.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: SkillTypesComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class SkillTypesRoutingModule {}
