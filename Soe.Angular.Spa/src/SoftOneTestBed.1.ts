import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { HttpClientModule, HttpClient } from '@angular/common/http';
import { AgGridAngular } from 'ag-grid-angular';
import { ToastrModule, ToastrService, TOAST_CONFIG } from 'ngx-toastr';
import { NO_ERRORS_SCHEMA, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { ActivatedRoute } from '@angular/router';
import { of } from 'rxjs';
import { MatDialogRefMock, mockToastConfig } from './SoftOneTestBed';

@NgModule({
  imports: [
    CommonModule,
    TranslateModule.forRoot(),
    FontAwesomeModule,
    AgGridAngular,
    HttpClientModule,
    ToastrModule.forRoot(),
    // Add other modules here
  ],
  declarations: [
    // Declare components, directives, and pipes here if needed
  ],
  exports: [CommonModule, TranslateModule, FontAwesomeModule, AgGridAngular],
  providers: [
    {
      provide: MatDialogRef,
      useValue: new MatDialogRefMock(),
    },
    { provide: MAT_DIALOG_DATA, useValue: {} },
    HttpClient,
    { provide: TOAST_CONFIG, useValue: mockToastConfig },
    ToastrService,
    // Add other providers here if needed
    {
      provide: ActivatedRoute,
      useValue: {
        params: of({}), // Mock any params you expect to use
        queryParams: of({}), // Mock any queryParams you expect to use
      },
    },
  ],
  schemas: [CUSTOM_ELEMENTS_SCHEMA, NO_ERRORS_SCHEMA],
})
export class SoftOneTestBed {}
