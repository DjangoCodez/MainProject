import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AdjustTimeStampsComponent } from './adjust-time-stamps.component';

describe('AdjustTimeStampsComponent', () => {
  let component: AdjustTimeStampsComponent;
  let fixture: ComponentFixture<AdjustTimeStampsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [AdjustTimeStampsComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AdjustTimeStampsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
