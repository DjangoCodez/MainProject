import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TimeCodeRankingEdit } from './time-code-ranking-edit';

describe('TimeCodeRankingEdit', () => {
  let component: TimeCodeRankingEdit;
  let fixture: ComponentFixture<TimeCodeRankingEdit>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [TimeCodeRankingEdit],
    }).compileComponents();

    fixture = TestBed.createComponent(TimeCodeRankingEdit);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
