import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { CollectiveAgreementsComponent } from './components/collective-agreements/collective-agreements.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: CollectiveAgreementsComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class CollectiveAgreementsRoutingModule {}
