import { ComponentFixture, TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { CollectiveAgreementsComponent } from './collective-agreements.component';

describe('CollectiveAgreementsComponent', () => {
  let component: CollectiveAgreementsComponent;
  let fixture: ComponentFixture<CollectiveAgreementsComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],
      declarations: [CollectiveAgreementsComponent],
    });
    fixture = TestBed.createComponent(CollectiveAgreementsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should have a valid config', () => {
    expect(component.config).toBeDefined();
    expect(component.config.length).toBe(1);

    const configItem = component.config[0];
    expect(configItem.gridTabLabel).toBe('time.employee.employeecollectiveagreement.employeecollectiveagreements');
    expect(configItem.editTabLabel).toBe('time.employee.employeecollectiveagreement.employeecollectiveagreement');
    expect(configItem.createTabLabel).toBe('time.employee.employeecollectiveagreement.new');
  });
});