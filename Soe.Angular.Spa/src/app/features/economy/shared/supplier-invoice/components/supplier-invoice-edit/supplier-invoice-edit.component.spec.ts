import { ComponentFixture, TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';
import { SupplierInvoiceEditComponent } from './supplier-invoice-edit.component';
import { SupplierInvoiceService } from '../../services/supplier-invoice.service';
import { SupplierInvoiceLoaderService } from '../../services/supplier-invoice-loader.service';
import { SupplierInvoiceFeatureService } from '../../services/supplier-invoice-feature.service';
import { InvoiceAccountingRowsService } from '../../domain-services/accounting-rows.service';
import { InvoiceVatService } from '../../domain-services/vat.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { AutoHeightService } from '@shared/directives/auto-height/auto-height.service';
import { InvoicePaymentConditionService } from '../../domain-services/payment-condition.service';
import { CurrencyService } from '@shared/services/currency.service';
import { signal } from '@angular/core';
import { vi } from 'vitest';
import { of } from 'rxjs';

describe('SupplierInvoiceEditComponent', () => {
  let component: SupplierInvoiceEditComponent;
  let fixture: ComponentFixture<SupplierInvoiceEditComponent>;

  const mockSupplierInvoiceService = {
    loadInvoiceByStrategy: vi.fn().mockReturnValue(of([{}, {}])),
  };

  const mockSupplierInvoiceLoaderService = {
    load: vi.fn().mockReturnValue(of([])),
  };

  const mockInvoiceAccountingRowsService = {
    generateAccountingRows: vi.fn().mockReturnValue([]),
  };

  const mockInvoiceVatService = {
    vatRate: vi.fn().mockReturnValue(0),
    purchaseVATAccountId: vi.fn().mockReturnValue(0),
    shouldShowVatAsZero: vi.fn().mockReturnValue(false),
  };

  const mockInvoicePaymentConditionService = {};
  const mockCurrencyService = {};
  const mockAutoHeightService = {};
  const mockSupplierInvoiceFeatureService = {
    hasProductRows: signal(true),
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SoftOneTestBed],
      declarations: [SupplierInvoiceEditComponent],
      providers: [
        FlowHandlerService,
        ToolbarService,
        {
          provide: SupplierInvoiceService,
          useValue: mockSupplierInvoiceService,
        },
        {
          provide: SupplierInvoiceLoaderService,
          useValue: mockSupplierInvoiceLoaderService,
        },
        {
          provide: InvoiceAccountingRowsService,
          useValue: mockInvoiceAccountingRowsService,
        },
        {
          provide: InvoiceVatService,
          useValue: mockInvoiceVatService,
        },
        {
          provide: AutoHeightService,
          useValue: mockAutoHeightService,
        },
        {
          provide: InvoicePaymentConditionService,
          useValue: mockInvoicePaymentConditionService,
        },
        {
          provide: CurrencyService,
          useValue: mockCurrencyService,
        },
        {
          provide: SupplierInvoiceFeatureService,
          useValue: mockSupplierInvoiceFeatureService,
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(SupplierInvoiceEditComponent);
    component = fixture.componentInstance;
    // Don't call detectChanges here to avoid triggering ngOnInit
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize on ngOnInit', () => {
    // Spy on startFlow method before detectChanges triggers ngOnInit
    const startFlowSpy = vi.spyOn(component as any, 'startFlow');
    fixture.detectChanges(); // This triggers ngOnInit
    expect(startFlowSpy).toHaveBeenCalled();
  });

  it('should toggle locked state', () => {
    const initialState = component.isLocked();
    component.toggleLocked();
    expect(component.isLocked()).toBe(!initialState);
  });

  it('should handle file list opened', () => {
    component.fileListOpened(true);
    expect(component.isFileDisplayAccordionOpen()).toBe(true);
  });

  it('should toggle tracing accordion', () => {
    component.toggleTracingOpened(true);
    expect(component.traceRowsRendered()).toBe(true);
    
    component.toggleTracingOpened(false);
    expect(component.traceRowsRendered()).toBe(false);
  });

  it('should toggle product rows accordion', () => {
    component.toggleProductRowsOpened(true);
    expect(component.productRowsRendered()).toBe(true);
    
    component.toggleProductRowsOpened(false);
    expect(component.productRowsRendered()).toBe(false);
  });

  it('should use feature service for product rows permission', () => {
    // First initialize the component
    fixture.detectChanges();
    
    // Verify that the feature service is injected and accessible
    const featureService = (component as any).featureService;
    expect(featureService).toBeDefined();
    expect(featureService.hasProductRows()).toBe(true);
  });
});

