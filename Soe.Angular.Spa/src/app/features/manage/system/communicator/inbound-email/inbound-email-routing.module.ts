import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { InboundEmailComponent } from './components/inbound-email/inbound-email.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: InboundEmailComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class InboundEmailRoutingModule {}
