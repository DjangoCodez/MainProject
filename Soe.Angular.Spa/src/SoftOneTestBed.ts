import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { HttpClientModule, HttpClient } from '@angular/common/http';
// Mock AgGridAngular for testing

@Component({
  selector: 'ag-grid-angular',
  template: '<div></div>',
  standalone: true,
})
class MockAgGridAngular {}
import PrototypeArrayExtensions from '@src/app/core/utils/prototype-array-extensions';
import PrototypeDateExtensions from '@src/app/core/utils/prototype-date-extensions';
import PrototypeNumberExtensions from '@core/utils/prototype-number-extensions';
import PrototypeStringExtensions from '@src/app/core/utils/prototype-string-extensions';
import { ToastrModule, ToastrService, TOAST_CONFIG } from 'ngx-toastr';
import {
  NO_ERRORS_SCHEMA,
  CUSTOM_ELEMENTS_SCHEMA,
  Component,
} from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { ActivatedRoute } from '@angular/router';
import { of } from 'rxjs';
import { SoeConfigUtil } from '@shared/util/soeconfig-util';
import { EditBaseDirective } from '@shared/directives/edit-base/edit-base.directive';
import { LabelComponent } from '@ui/label/label.component';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { vi } from 'vitest';
//import { ExtendedWindow } from '@core/services/authentication/authentication.service';

// Ensure prototype extensions are executed
new PrototypeArrayExtensions();
new PrototypeDateExtensions();
new PrototypeNumberExtensions();
new PrototypeStringExtensions();

export class MatDialogRefMock {
  addPanelClass = vi.fn();
  close = vi.fn();
  open = vi.fn();
}

// Mock SoeConfigUtil
const mockConfig = {
  accountDistributionType: 'someValue',
  soeParameters: 'some-soe-parameters',
  termVersionNr: '2024.07.22.1234',
  accountYearId: 2024,
  sysCountryId: 1,
  language: 'en-US',
  licenseId: 12345,
  licenseNr: '12345-67890',
  actorCompanyId: 54321,
  roleId: 1,
  userId: 1,
  loginName: 'testuser',
  token: 'test-token',
  isSupportAdmin: false,
  isSupportSuperAdmin: false,
};

// Replace the config getter with our mock
Object.defineProperty(SoeConfigUtil, 'config', {
  get: () => mockConfig,
  configurable: true,
});

export const mockToastConfig = {
  iconClasses: {
    error: 'toast-error',
    info: 'toast-info',
    success: 'toast-success',
    warning: 'toast-warning',
  },
  positionClass: 'toast-top-right',
  timeOut: 5000,
  closeButton: true,
};

// Mock ToastrService
export class MockToastrService {
  success = vi.fn();
  error = vi.fn();
  info = vi.fn();
  warning = vi.fn();
  show = vi.fn();
  clear = vi.fn();
  remove = vi.fn();
}

@NgModule({
  imports: [
    CommonModule,
    TranslateModule.forRoot(),
    FontAwesomeModule,
    HttpClientModule,
    ToastrModule.forRoot(),
    EditBaseDirective,
    LabelComponent,
    // Add other modules here
  ],
  declarations: [
    // Declare components, directives, and pipes here if needed
  ],
  exports: [CommonModule, TranslateModule, FontAwesomeModule],
  providers: [
    {
      provide: MatDialogRef,
      useValue: new MatDialogRefMock(),
    },
    { provide: MAT_DIALOG_DATA, useValue: {} },
    HttpClient,
    { provide: TOAST_CONFIG, useValue: mockToastConfig },
    { provide: ToastrService, useClass: MockToastrService },
    // Add other providers here if needed
    {
      provide: ActivatedRoute,
      useValue: {
        params: of({}), // Mock any params you expect to use
        queryParams: of({}), // Mock any queryParams you expect to use
        snapshot: {
          queryParamMap: {
            get: (key: string) => {
              const queryParams: { [key: string]: string } = {
                type: 'someType',
              }; // Mock the query parameters you expect
              return queryParams[key];
            },
          },
        },
      },
    },
    {
      provide: FlowHandlerService,
      useValue: {
        execute: vi.fn().mockImplementation(config => {
          // Mock implementation of the execute method
          if (config && config.finished) {
            config.finished();
          }
        }),
      },
    },
  ],
  schemas: [CUSTOM_ELEMENTS_SCHEMA, NO_ERRORS_SCHEMA],
})
export class SoftOneTestBed {}
