import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

const routes: Routes = [
  {
    path: 'system',
    loadChildren: () =>
      import('./system/system.module').then(m => m.SystemModule),
  },
  {
    path: 'users',
    loadChildren: () => import('./users/users.module').then(m => m.UsersModule),
  },
  {
    path: 'contactpersons',
    loadChildren: () =>
      import('./contact-persons/contact-persons.module').then(
        m => m.ContactPersonsModule
      ),
  },
  {
    path: 'contactpersons/categories',
    loadChildren: () =>
      import('../../shared/features/category/category.module').then(
        m => m.CategoryModule
      ),
  },
  {
    path: 'system/intrastat/commoditycodes',
    loadChildren: () =>
      import('./commodity-codes/commodity-codes.module').then(
        m => m.CommodityCodesModule
      ),
  },
  {
    path: 'system/importpricelist',
    loadChildren: () =>
      import('./import-price-list/import-price-list.module').then(
        m => m.ImportPriceListModule
      ),
  },
  {
    path: 'preferences/fieldsettings',
    loadChildren: () =>
      import('./field-settings/field-settings.module').then(
        m => m.FieldSettingsModule
      ),
  },
  {
    path: 'preferences/registry/openinghours',
    loadChildren: () =>
      import('./opening-hours/opening-hours.module').then(
        m => m.OpeningHoursModule
      ),
  },
  {
    path: 'preferences/registry/positions',
    loadChildren: () =>
      import('./positions/positions.module').then(m => m.PositionsModule),
  },
  {
    path: 'preferences/registry/schoolholiday',
    loadChildren: () =>
      import('./school-holiday/school-holiday.module').then(
        m => m.SchoolHolidayModule
      ),
  },
  {
    path: 'preferences/registry/signatorycontract',
    loadChildren: () =>
      import('./signatory-contract/signatory-contract.module').then(
        m => m.SignatoryContractModule
      ),
  },
  {
    path: 'system/edi/sysedimessagehead',
    loadChildren: () =>
      import('./sys-edi-message-head/sys-edi-message-head.module').then(
        m => m.SysEdiMessageHeadModule
      ),
  },
  {
    path: 'support/logs',
    loadChildren: () =>
      import('./support-logs/support-logs.module').then(
        m => m.SupportLogsModule
      ),
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class ManageRoutingModule {}
