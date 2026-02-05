import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { InventoryWriteOffTemplatesComponent } from './components/inventory-write-off-templates/inventory-write-off-templates.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: InventoryWriteOffTemplatesComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class InventoryWriteOffTemplatesRoutingModule {}
