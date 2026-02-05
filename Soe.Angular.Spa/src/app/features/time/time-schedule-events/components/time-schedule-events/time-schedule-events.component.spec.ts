import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TimeScheduleEventsComponent } from './time-schedule-events.component';

describe('TimeScheduleEventsComponent', () => {
  let component: TimeScheduleEventsComponent;
  let fixture: ComponentFixture<TimeScheduleEventsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [TimeScheduleEventsComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(TimeScheduleEventsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
