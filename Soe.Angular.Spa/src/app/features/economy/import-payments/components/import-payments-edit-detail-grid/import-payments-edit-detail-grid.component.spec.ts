import { ComponentFixture, TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';
import { ImportPaymentsEditDetailGridComponent } from './import-payments-edit-detail-grid.component';
import { FlowHandlerService } from '@shared/services/flow-handler.service'; // Adjust the path as necessary
import { vi } from 'vitest';

describe('ImportPaymentsEditDetailGridComponent', () => {
  let component: ImportPaymentsEditDetailGridComponent;
  let fixture: ComponentFixture<ImportPaymentsEditDetailGridComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SoftOneTestBed],
      declarations: [ImportPaymentsEditDetailGridComponent],
      providers: [
        FlowHandlerService, // Use the real FlowHandlerService
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(ImportPaymentsEditDetailGridComponent);
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
