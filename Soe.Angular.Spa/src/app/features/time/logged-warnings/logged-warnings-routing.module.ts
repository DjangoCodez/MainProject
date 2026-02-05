import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { LoggedWarningsComponent } from './components/logged-warnings/logged-warnings.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: LoggedWarningsComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class LoggedWarningsRoutingModule {}
