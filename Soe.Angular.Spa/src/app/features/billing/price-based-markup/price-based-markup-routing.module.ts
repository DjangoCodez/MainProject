import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { PriceBasedMarkupComponent } from './components/price-based-markup/price-based-markup.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: PriceBasedMarkupComponent
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class PriceBasedMarkupRoutingModule { }
