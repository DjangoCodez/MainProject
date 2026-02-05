import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CostAllocationDialog } from './cost-allocation-dialog';

describe('CostAllocationDialog', () => {
  let component: CostAllocationDialog;
  let fixture: ComponentFixture<CostAllocationDialog>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CostAllocationDialog],
    }).compileComponents();

    fixture = TestBed.createComponent(CostAllocationDialog);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
