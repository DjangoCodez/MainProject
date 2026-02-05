import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ExportStandardDefinitionsComponent } from './export-standard-definitions/components/export-standard-definitions/export-standard-definitions.component';

const routes: Routes = [
  {
    path: 'standarddef/default.aspx',
    component: ExportStandardDefinitionsComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class ExportRoutingModule {}
