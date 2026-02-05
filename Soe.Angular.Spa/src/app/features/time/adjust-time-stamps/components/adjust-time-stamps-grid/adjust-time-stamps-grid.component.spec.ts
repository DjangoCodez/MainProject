import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AdjustTimeStampsGridComponent } from './adjust-time-stamps-grid.component';

describe('AdjustTimeStampsGridComponent', () => {
  let component: AdjustTimeStampsGridComponent;
  let fixture: ComponentFixture<AdjustTimeStampsGridComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [AdjustTimeStampsGridComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AdjustTimeStampsGridComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
