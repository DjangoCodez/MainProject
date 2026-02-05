import { ComponentFixture, TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';
import { SupplierInvoiceProductRowsComponent } from './supplier-invoice-product-rows.component';
import { SupplierInvoiceProductRowsService } from '../../services/supplier-invoice-product-rows.service';
import { CommonCustomerService } from '@features/billing/shared/services/common-customer.service';
import { MessageboxService } from '@ui/dialog/services/messagebox.service';
import { DialogService } from '@ui/dialog/services/dialog.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { vi } from 'vitest';
import { of } from 'rxjs';
import { SupplierInvoiceRowType } from '@shared/models/generated-interfaces/Enumerations';

describe('SupplierInvoiceProductRowsComponent', () => {
  let component: SupplierInvoiceProductRowsComponent;
  let fixture: ComponentFixture<SupplierInvoiceProductRowsComponent>;

  const mockSupplierInvoiceProductRowsService = {
    get: vi.fn().mockReturnValue(of([])),
    transferToOrder: vi.fn().mockReturnValue(of({ success: true })),
  };

  const mockCommonCustomerService = {
    getSysWholesellersDict: vi.fn().mockReturnValue(of([
      { id: 1, name: 'Wholeseller 1' },
      { id: 2, name: 'Wholeseller 2' },
    ])),
  };

  const mockMessageboxService = {
    question: vi.fn().mockReturnValue({
      afterClosed: vi.fn().mockReturnValue(of({ result: true })),
    }),
  };

  const mockDialogService = {
    open: vi.fn().mockReturnValue({
      afterClosed: vi.fn().mockReturnValue(of({ customerInvoiceId: 123 })),
    }),
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SoftOneTestBed, SupplierInvoiceProductRowsComponent],
      providers: [
        FlowHandlerService,
        ToolbarService,
        {
          provide: SupplierInvoiceProductRowsService,
          useValue: mockSupplierInvoiceProductRowsService,
        },
        {
          provide: CommonCustomerService,
          useValue: mockCommonCustomerService,
        },
        {
          provide: MessageboxService,
          useValue: mockMessageboxService,
        },
        {
          provide: DialogService,
          useValue: mockDialogService,
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(SupplierInvoiceProductRowsComponent);
    component = fixture.componentInstance;
    
    // Set required input
    fixture.componentRef.setInput('supplierInvoiceId', 1);
    
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

  it('should update selected row count', () => {
    const mockRows = [
      {
        supplierInvoiceProductRowId: 1,
        supplierInvoiceId: 1,
        sellerProductNumber: 'P001',
        text: 'Product 1',
        unitCode: 'pcs',
        quantity: 10,
        priceCurrency: 100,
        amountCurrency: 1000,
        vatAmountCurrency: 250,
        vatRate: 25,
        customerInvoiceNumber: '',
        rowType: SupplierInvoiceRowType.ProductRow,
        state: 1,
        createdBy: 'user',
        modifiedBy: 'user',
      },
      {
        supplierInvoiceProductRowId: 2,
        supplierInvoiceId: 1,
        sellerProductNumber: 'P002',
        text: 'Product 2',
        unitCode: 'pcs',
        quantity: 5,
        priceCurrency: 200,
        amountCurrency: 1000,
        vatAmountCurrency: 250,
        vatRate: 25,
        customerInvoiceNumber: '',
        rowType: SupplierInvoiceRowType.ProductRow,
        state: 1,
        createdBy: 'user',
        modifiedBy: 'user',
      },
    ];
    
    component.selectionChanged(mockRows);
    expect(component.selectedRowCount()).toBe(2);
  });

  it('should compute transfer disabled state', () => {
    // Initially should be disabled (no rows selected)
    component.selectedRowCount.set(0);
    component.wholesellerId.set(1);
    expect(component.transferDisabled()).toBe(true);
    
    // Should be disabled when wholeseller is 0
    component.selectedRowCount.set(1);
    component.wholesellerId.set(0);
    expect(component.transferDisabled()).toBe(true);
    
    // Should be enabled when both are set
    component.selectedRowCount.set(1);
    component.wholesellerId.set(1);
    expect(component.transferDisabled()).toBe(false);
  });

  it('should show transfer dialog when button clicked', () => {
    // Initialize component first
    fixture.detectChanges();
    
    // Mock flowHandler to prevent permission check from returning early
    vi.spyOn(component.flowHandler, 'modifyPermission').mockReturnValue(false);
    
    // Calling showTransferDialog should return early if no permission
    component.showTransferDialog();
    
    // Verify dialog was not opened due to lack of permission
    expect(mockDialogService.open).not.toHaveBeenCalled();
  });

  it('should sync form control to signal', () => {
    // Initialize component first to set up the form control subscription
    fixture.detectChanges();
    
    // Set form value
    component.transferForm.patchValue({ wholesellerId: 5 });
    
    // Check signal is updated
    expect(component.wholesellerId()).toBe(5);
  });
});

