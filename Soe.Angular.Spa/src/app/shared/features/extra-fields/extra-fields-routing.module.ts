import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ExtraFieldsComponent } from './components/extra-fields/extra-fields.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: ExtraFieldsComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class ExtraFieldsRoutingModule {}
