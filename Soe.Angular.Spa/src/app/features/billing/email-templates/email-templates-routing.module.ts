import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { EmailTemplatesComponent } from './components/email-templates/email-templates.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: EmailTemplatesComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class EmailTemplatesRoutingModule {}
