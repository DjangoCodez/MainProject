import { ComponentFixture, TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';
import { FlowHandlerService } from '@shared/services/flow-handler.service'; // Adjust the path as necessary
import { VoucherEditHistoryGridComponent } from './voucher-edit-history-grid.component';
import { vi } from 'vitest';

describe('VoucherEditHistoryGridComponent', () => {
  let component: VoucherEditHistoryGridComponent;
  let fixture: ComponentFixture<VoucherEditHistoryGridComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SoftOneTestBed],
      declarations: [VoucherEditHistoryGridComponent],
      providers: [
        FlowHandlerService, // Use the real FlowHandlerService
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(VoucherEditHistoryGridComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should call executeForGrid when startFlow is called', () => {
    const flowHandlerService = TestBed.inject(FlowHandlerService);
    const executeForGridSpy = vi.spyOn(flowHandlerService, 'executeForGrid');
    component.ngOnInit(); // Call ngOnInit to trigger startFlow
    expect(executeForGridSpy).toHaveBeenCalled();
    // Optionally, you can add more assertions to verify the behavior of executeForGrid
  });
});
