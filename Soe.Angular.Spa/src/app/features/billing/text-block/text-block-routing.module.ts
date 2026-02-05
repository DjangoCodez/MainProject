import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { TextBlockComponent } from './components/text-block/text-block.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: TextBlockComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class TextBlockRoutingModule {}
