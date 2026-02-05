import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { HomeComponent } from '@core/components/home/home.component';

const routes: Routes = [
  { path: '', component: HomeComponent, pathMatch: 'full' },
  {
    path: 'soe/common/rightmenu/helpmenu',
    loadChildren: () =>
      import('./features/right-menu/right-menu.module').then(
        m => m.RightMenuModule
      ),
  },
  {
    path: 'soe/manage',
    loadChildren: () =>
      import('./features/manage/manage.module').then(m => m.ManageModule),
  },
  {
    path: 'soe/billing',
    loadChildren: () =>
      import('./features/billing/billing.module').then(m => m.BillingModule),
  },
  {
    path: 'soe/economy',
    loadChildren: () =>
      import('./features/economy/economy.module').then(m => m.EconomyModule),
  },
  {
    path: 'soe/time',
    loadChildren: () =>
      import('./features/time/time.module').then(m => m.TimeModule),
  },
  {
    path: 'soe/clientmanagement',
    loadChildren: () =>
      import('./features/client-management/client-management.module').then(
        m => m.ClientManagementModule
      ),
  },
  // { path: '**', component: HomeComponent, pathMatch: 'full' }, // Removed to get SPA navigation working.
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule],
})
export class AppRoutingModule {}
