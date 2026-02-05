import { NgModule } from '@angular/core';
import { EdiComponent } from './components/edi/edi.component';
import { RouterModule, Routes } from '@angular/router';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: EdiComponent,
  },
]

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class EdiRoutingModule { }
