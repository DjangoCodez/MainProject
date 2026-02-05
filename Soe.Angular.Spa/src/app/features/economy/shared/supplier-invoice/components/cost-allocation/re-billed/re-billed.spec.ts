import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ReBilledComponent } from './re-billed';

describe('ReBilledComponent', () => {
  let component: ReBilledComponent;
  let fixture: ComponentFixture<ReBilledComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ReBilledComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(ReBilledComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
