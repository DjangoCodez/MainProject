import { enableProdMode } from '@angular/core';
import { platformBrowserDynamic } from '@angular/platform-browser-dynamic';

import { AppModule } from './app/app.module';
import { environment } from './environments/environment';

import { LicenseManager, AllEnterpriseModule } from 'ag-grid-enterprise';
import { ModuleRegistry, provideGlobalGridOptions } from 'ag-grid-community';

export function getBaseUrl() {
  return document.getElementsByTagName('base')[0].href;
}

const providers = [{ provide: 'BASE_URL', useFactory: getBaseUrl, deps: [] }];

if (environment.production) {
  enableProdMode();
}

// AG grid license
LicenseManager.setLicenseKey(environment.agGridLicenceKey);

// Mark all grids as using legacy themes
provideGlobalGridOptions({ theme: 'legacy' });

// AG grid module registration
// https://ag-grid.com/angular-data-grid/modules/
ModuleRegistry.registerModules([AllEnterpriseModule]);

platformBrowserDynamic(providers)
  .bootstrapModule(AppModule)
  .catch(err => console.log(err));
