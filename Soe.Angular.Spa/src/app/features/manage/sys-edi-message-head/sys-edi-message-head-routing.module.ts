import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { SysEdiMessageHeadComponent } from './components/sys-edi-message-head/sys-edi-message-head.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: SysEdiMessageHeadComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class SysEdiMessageHeadRoutingModule {}
