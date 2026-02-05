import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AssetRegisterComponent } from './components/asset-register/asset-register.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: AssetRegisterComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class AssetRegisterRoutingModule {}
