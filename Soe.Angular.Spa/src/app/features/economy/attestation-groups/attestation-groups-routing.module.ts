import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AttestationGroupsComponent } from './components/attestation-groups/attestation-groups.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: AttestationGroupsComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class AttestationGroupsRoutingModule {}
