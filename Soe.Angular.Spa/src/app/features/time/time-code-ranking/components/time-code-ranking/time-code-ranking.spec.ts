import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TimeCodeRankingComponent } from './time-code-ranking';

describe('TimeCodeRanking', () => {
  let component: TimeCodeRankingComponent;
  let fixture: ComponentFixture<TimeCodeRankingComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [TimeCodeRankingComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(TimeCodeRankingComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
