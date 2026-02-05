import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ImportPriceListComponent } from './components/import-price-list/import-price-list.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: ImportPriceListComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class ImportPriceListRoutingModule {}
