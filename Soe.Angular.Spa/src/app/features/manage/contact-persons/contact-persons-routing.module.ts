import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ContactPersonsComponent } from './components/contact-persons/contact-persons.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: ContactPersonsComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class ContactPersonsRoutingModule {}
