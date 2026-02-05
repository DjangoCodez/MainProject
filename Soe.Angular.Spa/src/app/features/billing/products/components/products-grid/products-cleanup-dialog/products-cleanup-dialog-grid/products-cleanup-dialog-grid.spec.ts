import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ProductsCleanupDialogGrid } from './products-cleanup-dialog-grid';

describe('ProductsCleanupDialogGrid', () => {
  let component: ProductsCleanupDialogGrid;
  let fixture: ComponentFixture<ProductsCleanupDialogGrid>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ProductsCleanupDialogGrid]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ProductsCleanupDialogGrid);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
