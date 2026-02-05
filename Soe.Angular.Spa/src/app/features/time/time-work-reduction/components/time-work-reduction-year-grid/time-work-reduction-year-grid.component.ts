import {
  Component,
  Input,
  Output,
  EventEmitter,
  OnInit,
  inject,
  OnChanges,
} from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { ITimeWorkReductionReconciliationYearDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { GridComponent } from '@ui/grid/grid.component';
import { Observable, of, take } from 'rxjs';
import { TimeWorkReductionYearService } from '../../services/time-work-reduction-year.service.';

@Component({
  selector: 'soe-time-work-reduction-year-grid',
  templateUrl:
    '../../../../../shared/ui-components/grid/grid-wrapper/grid-wrapper-template.html',
  standalone: false,
})
export class TimeWorkReductionYearGridComponent
  extends GridBaseDirective<
    ITimeWorkReductionReconciliationYearDTO,
    TimeWorkReductionYearService
  >
  implements OnInit, OnChanges
{
  @Input() years: ITimeWorkReductionReconciliationYearDTO[] = [];
  @Output() editEvent =
    new EventEmitter<ITimeWorkReductionReconciliationYearDTO>();
  @Output() delete =
    new EventEmitter<ITimeWorkReductionReconciliationYearDTO>();
  @Input() pensionPayrollProducts: SmallGenericType[] | undefined;
  @Input() directPaymentpayrollProducts: SmallGenericType[] | undefined;
  service = inject(TimeWorkReductionYearService);

  ngOnInit() {
    super.ngOnInit();

    this.startFlow(
      Feature.Time_Time_TimeWorkReduction,
      'Time.Time.TimeWorkReduction',
      {
        lookups: [],
      }
    );
  }
  onFinished(): void {
    console.log(this.years);
  }
  ngOnChanges() {
    if (this.grid) {
      console.log(this.years);
    }
  }
  override loadData(
    id?: number,
    additionalProps?: any
  ): Observable<ITimeWorkReductionReconciliationYearDTO[]> {
    console.log('Loading years:', this.years);
    return of(this.years);
  }
  override onGridReadyToDefine(
    grid: GridComponent<ITimeWorkReductionReconciliationYearDTO>
  ) {
    super.onGridReadyToDefine(grid);

    this.translate
      .get([
        'core.edit',
        'time.payroll.worktimeaccount.employeelastdecideddate',
        'time.time.timeworkreduction.stopdate',
        'time.payroll.worktimeaccount.pensionproduct',
        'time.payroll.worktimeaccount.directpaymentproduct',
        'time.payroll.worktimeaccount.defaultwithdrawalmethod',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnDate(
          'stop',
          terms['time.time.timeworkreduction.stopdate'],
          {
            flex: 12,
            editable: false,
          }
        );
        this.grid.addColumnDate(
          'employeeLastDecidedDate',
          terms['time.payroll.worktimeaccount.employeelastdecideddate'],
          {
            flex: 12,
            editable: false,
          }
        );
        this.grid.addColumnSelect(
          'pensionDepositPayrollProductId',
          terms['time.payroll.worktimeaccount.pensionproduct'],
          this.pensionPayrollProducts || [],
          undefined,
          {
            flex: 12,
            editable: false,
          }
        );
        this.grid.addColumnSelect(
          'directPaymentPayrollProductId',
          terms['time.payroll.worktimeaccount.directpaymentproduct'],
          this.directPaymentpayrollProducts || [],
          undefined,
          {
            flex: 12,
            editable: false,
          }
        );
        this.grid.addColumnIconEdit({
          tooltip: terms['core.edit'],
          onClick: row => {
            this.edit(row);
          },
        });
        super.finalizeInitGrid();
        this.onFinished();
      });
  }
  getPensionDepositName(id: number | undefined): string {
    return this.pensionPayrollProducts?.find(p => p.id === id)?.name || '';
  }
  getDirectPaymentName(id: number | undefined): string {
    return (
      this.directPaymentpayrollProducts?.find(p => p.id === id)?.name || ''
    );
  }

  onEdit(row: ITimeWorkReductionReconciliationYearDTO) {
    this.editEvent.emit(row);
  }

  onDelete(row: ITimeWorkReductionReconciliationYearDTO) {
    this.delete.emit(row);
  }
}
