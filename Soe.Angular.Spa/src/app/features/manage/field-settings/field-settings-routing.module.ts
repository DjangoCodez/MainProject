import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { FieldSettingsComponent } from './components/field-settings/field-settings.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: FieldSettingsComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class FieldSettingsRoutingModule {}
