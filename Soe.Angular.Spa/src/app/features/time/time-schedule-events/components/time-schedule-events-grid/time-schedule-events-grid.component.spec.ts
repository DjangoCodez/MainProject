import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TimeScheduleEventsGridComponent } from './time-schedule-events-grid.component';

describe('TimeScheduleEventsGridComponent', () => {
  let component: TimeScheduleEventsGridComponent;
  let fixture: ComponentFixture<TimeScheduleEventsGridComponent>;
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [TimeScheduleEventsGridComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(TimeScheduleEventsGridComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
