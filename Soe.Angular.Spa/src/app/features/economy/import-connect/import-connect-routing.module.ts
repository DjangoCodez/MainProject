import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ImportConnectComponent } from './components/import-connect/import-connect.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: ImportConnectComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class ImportConnectRoutingModule {}
