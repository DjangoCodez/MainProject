import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { FinnishTaxExportComponent } from './components/finnish-tax-export/finnish-tax-export.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: FinnishTaxExportComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class FinnishTaxExportRoutingModule {}
