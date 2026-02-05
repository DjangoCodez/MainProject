import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SupplierInvoiceEditCostAllocationChargedToProject } from './supplier-invoice-edit-cost-allocation-charged-to-project';

describe('SupplierInvoiceEditCostAllocationChargedToProject', () => {
  let component: SupplierInvoiceEditCostAllocationChargedToProject;
  let fixture: ComponentFixture<SupplierInvoiceEditCostAllocationChargedToProject>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SupplierInvoiceEditCostAllocationChargedToProject]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SupplierInvoiceEditCostAllocationChargedToProject);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
