import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TimeScheduleEventsEditComponent } from './time-schedule-events-edit.component';

describe('TimeScheduleEventsEdit', () => {
  let component: TimeScheduleEventsEditComponent;
  let fixture: ComponentFixture<TimeScheduleEventsEditComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [TimeScheduleEventsEditComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(TimeScheduleEventsEditComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
