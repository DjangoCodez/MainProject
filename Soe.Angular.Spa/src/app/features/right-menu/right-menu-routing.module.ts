import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { FaqComponent } from './pages/faq/faq.component';
import { ReleaseNotesComponent } from './pages/release-notes/release-notes.component';

const routes: Routes = [
  {
    path: 'releasenotes/default.aspx',
    component: ReleaseNotesComponent,
  },
  {
    path: 'faq/default.aspx',
    component: FaqComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class RightMenuRoutingModule {}
