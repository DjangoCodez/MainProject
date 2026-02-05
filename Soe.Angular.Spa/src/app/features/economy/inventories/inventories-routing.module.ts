import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { InventoriesComponent } from './components/inventories/inventories.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: InventoriesComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class InventoriesRoutingModule {}
