import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { InventoryWriteOffMethodsComponent } from './components/inventory-write-off-methods/Inventory-write-off-methods.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: InventoryWriteOffMethodsComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class InventoryWriteOffMethodsRoutingModule {}
