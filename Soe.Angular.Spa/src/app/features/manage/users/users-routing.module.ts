import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

const routes: Routes = [
  // {
  //   path: '**', // Users
  // }
  {
    path: 'service',
    loadChildren: () =>
      import('./service-users/service-users.module').then(
        m => m.ServiceUsersModule
      ),
  },
];

@NgModule({
  imports: [CommonModule, RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class UsersRoutingModule {}
