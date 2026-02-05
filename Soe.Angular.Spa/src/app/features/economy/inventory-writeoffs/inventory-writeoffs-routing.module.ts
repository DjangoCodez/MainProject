import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { InventoryWriteoffsComponent } from './components/inventory-writeoffs/inventory-writeoffs.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: InventoryWriteoffsComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class InventoryWriteoffsRoutingModule {}
