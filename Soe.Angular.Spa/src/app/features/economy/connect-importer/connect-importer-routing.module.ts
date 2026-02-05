import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ConnectImporterComponent } from './components/connect-importer/connect-importer.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: ConnectImporterComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class ConnectImporterRoutingModule {}
