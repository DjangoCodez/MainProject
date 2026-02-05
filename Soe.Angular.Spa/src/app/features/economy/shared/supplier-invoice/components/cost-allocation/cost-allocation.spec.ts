import { ComponentFixture, TestBed } from '@angular/core/testing';
import { CostAllocationComponent } from './cost-allocation';

describe('CostAllocation', () => {
  let component: CostAllocationComponent;
  let fixture: ComponentFixture<CostAllocationComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CostAllocationComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(CostAllocationComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
