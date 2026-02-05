import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AccountingCodingLevelsComponent } from './components/accounting-coding-levels/accounting-coding-levels.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: AccountingCodingLevelsComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class AccountingCodingLevelsRoutingModule {}
