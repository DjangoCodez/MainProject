import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { DiscountLettersComponent } from './components/discount-letters/discount-letters.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: DiscountLettersComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class DiscountLettersRoutingModule { }
