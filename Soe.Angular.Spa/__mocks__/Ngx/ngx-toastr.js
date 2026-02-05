import { TestBed } from '@angular/core/testing';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { HttpClientModule, HttpClient } from '@angular/common/http';
import { ToastrModule, ToastrService, TOAST_CONFIG } from 'ngx-toastr';
import { NO_ERRORS_SCHEMA, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { AgGridAngular } from 'ag-grid-angular';
import { vi } from 'vitest';

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

export class SoftOneTestBed {
    static init() {
        TestBed.configureTestingModule({
            imports: [
                CommonModule,
                TranslateModule.forRoot(),
                FontAwesomeModule,
                AgGridAngular,
                HttpClientModule,
                ToastrModule.forRoot(),
            ],
            declarations: [],
            providers: [
                HttpClient,
                { provide: TOAST_CONFIG, useValue: mockToastConfig },
                ToastrService,
            ],
            schemas: [CUSTOM_ELEMENTS_SCHEMA, NO_ERRORS_SCHEMA],
        });
    }

    static addToasterServiceMock() {
        vi.mock('ngx-toastr', () => {
            return {
                ToastrService: vi.fn().mockImplementation(() => ({
                    success: vi.fn(),
                    error: vi.fn(),
                    info: vi.fn(),
                    warning: vi.fn(),
                })),
            };
        });
    }

    static addCustomProvider(provider) {
        TestBed.overrideProvider(provider.provide, { useValue: provider.useValue });
    }
}
