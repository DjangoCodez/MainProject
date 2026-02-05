import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { CurrenciesComponent } from './components/currencies/currencies.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: CurrenciesComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class CurrenciesRoutingModule {}
