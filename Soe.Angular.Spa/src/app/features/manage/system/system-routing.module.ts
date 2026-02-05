import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

const routes: Routes = [
  {
    path: 'bankintegration',
    loadChildren: () =>
      import('./bankintegration/bankintegration.module').then(
        m => m.BankintegrationModule
      ),
  },
  {
    path: 'communicator/incomingemail',
    loadChildren: () =>
      import('./communicator/inbound-email/inbound-email.module').then(
        m => m.InboundEmailModule
      ),
  },
  {
    path: 'admin/uicomponents',
    loadChildren: () =>
      import('./ui-components-test/ui-components-test.module').then(
        m => m.UiComponentsTestModule
      ),
  },
  {
    path: 'syscompany/syscompany',
    loadChildren: () =>
      import('./sys-company/sys-company.module').then(m => m.SysCompanyModule),
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class SystemRoutingModule {}
