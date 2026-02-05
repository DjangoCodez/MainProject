import { ComponentFixture, TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';
import { StatisticalCommodityCodesGridComponent } from './statistical-commodity-codes-grid.component';
import { StatisticalCommodityCodesService } from '../../services/statistical-commodity-codes.service';
import { ProgressService } from '@shared/services/progress/progress.service';

describe('StatisticalCommodityCodesGridComponent', () => {
  let component: StatisticalCommodityCodesGridComponent;
  let fixture: ComponentFixture<StatisticalCommodityCodesGridComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SoftOneTestBed],
      declarations: [StatisticalCommodityCodesGridComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(StatisticalCommodityCodesGridComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize services and properties', () => {
    expect(component.service).toBeInstanceOf(StatisticalCommodityCodesService);
    expect(component.progressService).toBeInstanceOf(ProgressService);
    expect(component.saveButtonDisabled()).toBe(true);
    expect(component.performAction).toBeDefined();
  });
});
