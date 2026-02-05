import { ComponentFixture, TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';
import { ChangeIntrastatCodeGridComponent } from './change-intrastat-code-grid.component';
import { FlowHandlerService } from '@shared/services/flow-handler.service'; // Adjust the path as necessary
import { vi } from 'vitest';

describe('ChangeIntrastatCodeGridComponent', () => {
  let component: ChangeIntrastatCodeGridComponent;
  let fixture: ComponentFixture<ChangeIntrastatCodeGridComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SoftOneTestBed],
      declarations: [ChangeIntrastatCodeGridComponent],
      providers: [
        FlowHandlerService, // Use the real FlowHandlerService
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(ChangeIntrastatCodeGridComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
