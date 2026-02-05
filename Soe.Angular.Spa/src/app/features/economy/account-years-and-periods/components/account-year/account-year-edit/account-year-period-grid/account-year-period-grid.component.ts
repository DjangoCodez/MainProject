import {
  Component,
  EventEmitter,
  Input,
  OnChanges,
  OnInit,
  Output,
  SimpleChanges,
  inject,
} from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { SoeFormGroup } from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import {
  Feature,
  TermGroup,
  TermGroup_AccountStatus,
  TermGroup_AccountYearStatus,
} from '@shared/models/generated-interfaces/Enumerations';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { GridComponent } from '@ui/grid/grid.component';
import { MessageboxService } from '@ui/dialog/services/messagebox.service';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { BehaviorSubject, Observable, take, tap } from 'rxjs';
import { AccountPeriodRowsForm } from '../../../../models/account-period-rows-form.model';
import { AccountPeriodDTO } from '../../../../models/account-years-and-periods.model';

@Component({
  selector: 'soe-account-year-period-grid',
  templateUrl: './account-year-period-grid.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class AccountYearPeriodGridComponent
  extends GridBaseDirective<AccountPeriodDTO>
  implements OnInit, OnChanges
{
  @Input() parentForm!: SoeFormGroup;
  @Input() rows!: BehaviorSubject<AccountPeriodDTO[]>;
  @Input() isNew!: boolean;
  @Input() from!: Date;
  @Input() to!: Date;
  @Input() status!: number;

  @Output() isDirty = new EventEmitter<boolean>();

  validationHandler = inject(ValidationHandler);
  coreService = inject(CoreService);
  messageBoxService = inject(MessageboxService);

  form = new AccountPeriodRowsForm({
    validationHandler: this.validationHandler,
    element: new AccountPeriodDTO(),
  });

  accountStatuses: ISmallGenericType[] = [];
  private budgetSubTypes: ISmallGenericType[] = [];
  private rowsAreSet = false;

  ngOnInit(): void {
    super.ngOnInit();

    this.startFlow(
      Feature.Economy_Accounting_AccountPeriods,
      'Economy.Accounting.AccountYear.Periods',
      {
        skipInitialLoad: true,
        lookups: [this.loadAccountStatuses(), this.loadBudgetSubTypes()],
      }
    );

    if (this.isNew) this.isDirty.emit(true);
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (
      (this.rowsAreSet || this.isNew) &&
      (changes.to || changes.from) &&
      this.needsToRegeneratePeriods()
    ) {
      this.generateNewPeriods();
    }
  }

  private loadAccountStatuses(): Observable<ISmallGenericType[]> {
    return this.coreService
      .getTermGroupContent(TermGroup.AccountStatus, false, false, true)
      .pipe(
        tap(x => {
          this.accountStatuses = x;
        })
      );
  }

  private loadBudgetSubTypes(): Observable<ISmallGenericType[]> {
    return this.coreService
      .getTermGroupContent(
        TermGroup.AccountingBudgetSubType,
        false,
        false,
        true
      )
      .pipe(
        tap(x => {
          this.budgetSubTypes = x;
        })
      );
  }

  override onGridReadyToDefine(grid: GridComponent<AccountPeriodDTO>) {
    super.onGridReadyToDefine(grid);

    this.translate
      .get([
        'economy.accounting.accountyear',
        'common.status',
        'common.number',
        'common.period',
        'core.time.month',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.enableRowSelection();
        this.grid.addColumnModified('isModified', { columnSeparator: true });
        this.grid.addColumnNumber('periodNr', terms['common.number'], {
          maxWidth: 60,
        });
        this.grid.addColumnText('periodName', terms['common.period'], {
          flex: 1,
        });
        this.grid.addColumnText('monthName', terms['core.time.month'], {
          flex: 1,
        });
        this.grid.addColumnText('statusName', terms['common.status'], {
          flex: 1,
        });
        this.grid.addColumnShape('status', '', {
          flex: 1,
          shape: 'circle',
          colorField: 'statusIcon',
          tooltip: 'statusName',
        });

        this.grid.context.suppressGridMenu = true;
        super.finalizeInitGrid({ hidden: true });
      });
  }

  override onFinished(): void {
    if (!this.isNew) {
      this.rows.asObservable().subscribe(r => {
        this.loadGrid(r);
      });
    } else this.generateNewPeriods();
  }

  loadGrid(row: AccountPeriodDTO[]) {
    row.forEach(r => {
      const periodStatus = this.accountStatuses.find(o => o.id === r.status);
      const monthName = this.budgetSubTypes.find(
        t => t.id === r.from.getMonth() + 1
      );

      if (periodStatus) r.statusName = periodStatus.name;
      if (monthName) r.monthName = monthName.name;
      r.statusIcon = this.getStatusIcon(r.status);
      r.periodName =
        r.from.getFullYear().toString() +
        '-' +
        (r.from.getMonth() + 1).toString();
    });
    this.grid.refreshCells();

    // Used to circumvent the fact that validation will fail due to grid not being initialized before validations run.
    this.rowsAreSet = true;
  }

  statusChange(event: number | undefined) {
    if (event && event == TermGroup_AccountStatus.Locked)
      this.messageBoxService.warning(
        'core.warning',
        this.translate.instant(
          'economy.accounting.accountyear.changestatuslockedwarning'
        )
      );
  }

  private getStatusIcon(status: number): string {
    switch (status) {
      case TermGroup_AccountStatus.New:
        return '#1e1e1e';
      case TermGroup_AccountStatus.Open:
        return '#24a148';
      case TermGroup_AccountStatus.Closed:
        return '#ff832b';
      case TermGroup_AccountStatus.Locked:
        return '#da1e28';
      default:
        return '';
    }
  }

  public changeStatusOnPeriod() {
    const selectedRows = this.grid.getSelectedRows();
    let invalidCount = 0;
    let invalidVouchersCount = 0;
    this.grid.clearSelectedRows();

    selectedRows.forEach((row: AccountPeriodDTO) => {
      switch (row.status) {
        case TermGroup_AccountStatus.New:
          if (this.form.value.status === TermGroup_AccountStatus.Open) {
            if (!row.hasExistingVouchers) {
              row.status = this.form.value.status;
              this.updatePeriodValues(row, true);
            } else {
              invalidVouchersCount++;
            }
          } else {
            invalidCount++;
          }
          break;
        case TermGroup_AccountStatus.Open:
          if (
            this.form.value.status === TermGroup_AccountStatus.New ||
            this.form.value.status === TermGroup_AccountStatus.Closed
          ) {
            row.status = this.form.value.status;
            this.updatePeriodValues(row, true);
          } else {
            invalidCount++;
          }
          break;
        case TermGroup_AccountStatus.Closed:
          if (this.form.value.status === TermGroup_AccountStatus.Open) {
            row.status = this.form.value.status;
            this.updatePeriodValues(row, true);
          } else {
            invalidCount++;
          }
          break;
        case TermGroup_AccountStatus.Locked:
          if (this.form.value.status === TermGroup_AccountStatus.Closed) {
            row.status = this.form.value.status;
            this.updatePeriodValues(row, true);
          } else {
            invalidCount++;
          }
          break;
      }
    });

    if (invalidCount > 0 || invalidVouchersCount > 0) {
      this.showWarningMessages(invalidCount, invalidVouchersCount);
      this.isDirty.emit(false);
    } else this.isDirty.emit(true);

    this.grid.agGrid.api.refreshCells();
  }

  showWarningMessages(invalidCount: number, invalidVouchersCount: number) {
    //invalid count
    if (invalidVouchersCount > 0) {
      invalidVouchersCount === 1
        ? this.messageBoxService.warning(
            'core.warning',
            this.translate
              .instant(
                'economy.accounting.accountyear.changestatusinvalidvouchersingle'
              )
              .replace(
                '{0}',
                this.accountStatuses.find(o => o.id === this.form.value.status)
                  ?.name
              )
          )
        : this.messageBoxService.warning(
            'core.warning',
            invalidCount.toString() +
              ' ' +
              this.translate
                .instant(
                  'economy.accounting.accountyear.changestatusinvalidvouchermultiple'
                )
                .replace(
                  '{0}',
                  this.accountStatuses.find(
                    o => o.id === this.parentForm.value.status
                  )?.name
                )
          );
    }

    //invalid status
    if (invalidCount > 0) {
      invalidCount === 1
        ? this.messageBoxService.warning(
            'core.warning',
            this.translate
              .instant(
                'economy.accounting.accountyear.changestatusinvalidsingle'
              )
              .replace(
                '{0}',
                this.accountStatuses.find(o => o.id === this.form.value.status)
                  ?.name
              )
          )
        : this.messageBoxService.warning(
            'core.warning',
            invalidCount.toString() +
              ' ' +
              this.translate
                .instant(
                  'economy.accounting.accountyear.changestatusinvalidmultiple'
                )
                .replace(
                  '{0}',
                  this.accountStatuses.find(
                    o => o.id === this.form.value.status
                  )?.name
                )
          );
    }
  }

  private updatePeriodValues(item: AccountPeriodDTO, isModified = true) {
    const status = this.accountStatuses.find(o => o.id === item.status);
    if (status) item['statusName'] = status.name;

    const monthName = this.budgetSubTypes.find(
      t => t.id === item.from.getMonth() + 1
    );
    if (monthName) item['monthName'] = monthName.name;

    item['periodName'] =
      item.from.getFullYear().toString() +
      '-' +
      (item.from.getMonth() + 1).toString();
    item['statusIcon'] = this.getStatusIcon(item.status);
    if (isModified) item.isModified = true;
  }

  public generateNewPeriods() {
    if (!this.from || !this.to) return;

    const currentPeriods = this.rows.getValue();
    const periodsToDelete = currentPeriods.length
      ? currentPeriods.filter(p => p.accountPeriodId > 0)
      : [];

    this.rows.next([]);
    const periods = [];

    for (let i = 0; i < this.getMonthsBetweenDates(this.from, this.to); i++) {
      const period = new AccountPeriodDTO();

      period.periodNr = i + 1;
      period.status = TermGroup_AccountStatus.New;
      period.isModified = true;
      period.from = new Date(
        this.from.getFullYear(),
        this.from.getMonth() + i,
        1
      );

      this.updatePeriodValues(period);
      periods.push(period);
    }
    this.rows.next(periods);
    // If we don't push the periods to the parent, the deletion won't be saved.
    this.parentForm.value.periods = periods.concat(periodsToDelete);
  }

  needsToRegeneratePeriods() {
    if (this.status !== TermGroup_AccountYearStatus.NotStarted) return false;

    const activePeriods = this.rows.getValue();
    if (!activePeriods.length) return true;

    const from = new Date(activePeriods[0].from);
    const to = new Date(activePeriods[activePeriods.length - 1].to);

    return !from.isEqual(this.from) || !to.isEqual(this.to);
  }

  public getMonthsBetweenDates(d1: Date, d2: Date): number {
    let months = (d2.getFullYear() - d1.getFullYear()) * 12;
    months -= d1.getMonth();
    months += d2.getMonth();
    return months <= 0 ? 0 : months + 1;
  }
}
