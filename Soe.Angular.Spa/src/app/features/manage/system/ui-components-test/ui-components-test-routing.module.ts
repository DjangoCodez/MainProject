import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { UiComponentsTestComponent } from './components/ui-components-test/ui-components-test.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: UiComponentsTestComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class UiComponentsTestRoutingModule {}
