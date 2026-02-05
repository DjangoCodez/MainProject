import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ProductsCleanupDialogComponent } from './products-cleanup-dialog-component';

describe('ProductsCleanupDialogComponent', () => {
  let component: ProductsCleanupDialogComponent;
  let fixture: ComponentFixture<ProductsCleanupDialogComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ProductsCleanupDialogComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ProductsCleanupDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
