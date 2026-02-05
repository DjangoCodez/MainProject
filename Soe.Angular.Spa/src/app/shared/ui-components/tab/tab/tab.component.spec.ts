import { ComponentFixture, TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { TabComponent } from './tab.component';

describe('TabComponent', () => {
  let component: TabComponent;
  let fixture: ComponentFixture<TabComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SoftOneTestBed, TabComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(TabComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
  describe('setup', () => {
    it('should set default values', () => {
      expect(component.label()).toBe('');
      expect(component.disabled()).toBe(false);
      expect(component.closable()).toBe(true);
      expect(component.isDirty()).toBe(false);
      expect(component.isNew()).toBe(false);
      expect(component.isActive()).toBe(false);
      expect(component.doubleClickCount()).toBe(0);
    });
  });
});
