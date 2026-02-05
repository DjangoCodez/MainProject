import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { BankintegrationComponent } from './components/bankintegration/bankintegration.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: BankintegrationComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class BankintegrationRoutingModule {}

