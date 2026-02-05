import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { MarkupComponent } from './components/markup/markup.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: MarkupComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class MarkupRoutingModule {}
