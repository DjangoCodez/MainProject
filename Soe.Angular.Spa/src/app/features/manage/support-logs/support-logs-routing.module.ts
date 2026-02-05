import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { SupportLogsComponent } from './components/support-logs/support-logs.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: SupportLogsComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class SupportLogsRoutingModule {}
