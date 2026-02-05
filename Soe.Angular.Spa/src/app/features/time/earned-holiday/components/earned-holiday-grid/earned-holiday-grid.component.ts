import { Component, inject, OnInit } from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { CrudActionTypeEnum } from '@shared/enums';
import { ValidationHandler } from '@shared/handlers';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { IEmployeeEarnedHolidayDTO } from '@shared/models/generated-interfaces/EmployeeEarnedHolidayDTO';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import {
  IEarnedHolidayModel,
  IManageTransactionsForEarnedHolidayModel,
} from '@shared/models/generated-interfaces/TimeModels';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { Perform } from '@shared/util/perform.class';
import { GridComponent } from '@ui/grid/grid.component';
import { IMessageboxComponentResponse } from '@ui/dialog/models/messagebox';
import { MessageboxService } from '@ui/dialog/services/messagebox.service';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { Observable, of, take, tap } from 'rxjs';
import { EarnedHolidayForm } from '../../models/earned-holiday-form.model';
import { EarnedHolidayService } from '../../services/earned-holiday.service';

@Component({
  selector: 'soe-earned-holiday-grid',
  standalone: false,
  templateUrl: 'earned-holiday-grid.component.html',
  providers: [FlowHandlerService, ToolbarService],
})
export class EarnedHolidayGridComponent
  extends GridBaseDirective<IEmployeeEarnedHolidayDTO, EarnedHolidayService>
  implements OnInit
{
  service = inject(EarnedHolidayService);
  validationHandler = inject(ValidationHandler);
  progress = new Perform<any[]>(this.progressService);
  messageboxService = inject(MessageboxService);

  holidays: { [year: number]: SmallGenericType[] } = {};
  holidayId: number = 0;
  holidayCount: number = 0;
  years: SmallGenericType[] = [];
  yearId: number = 0;
  selectedYear: SmallGenericType[] = [];
  form: EarnedHolidayForm | undefined;
  employeeEarnedHolidays: IEmployeeEarnedHolidayDTO[] | undefined;

  ngOnInit() {
    super.ngOnInit();
    this.form = this.createForm();

    this.startFlow(Feature.Time_Time_EarnedHoliday, 'Time.Time.EarnedHoliday', {
      skipInitialLoad: true,
      lookups: [this.loadYears(), this.loadholidays()],
    });
  }
  override createGridToolbar(): void {
    super.createGridToolbar({
      hideReload: true,
      hideClearFilters: true,
    });
  }

  override onGridReadyToDefine(grid: GridComponent<IEmployeeEarnedHolidayDTO>) {
    super.onGridReadyToDefine(grid);

    this.translate
      .get([
        'common.name',
        'time.employee.employee.employeenr',
        'time.employee.employee.percent',
        'time.employee.work5daysperweek',
        'time.employee.hasearnedholiday',
        'time.employee.suggestionearnedholiday',
        'common.note',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnNumber('employeeId', terms['common.code'], {
          editable: false,
          hide: true,
        });
        this.grid.addColumnText(
          'employeeNr',
          terms['time.employee.employee.employeenr'],
          {
            flex: 10,
            editable: false,
          }
        );
        this.grid.addColumnText('employeeName', terms['common.name'], {
          flex: 20,
          editable: false,
          enableHiding: true,
        });
        this.grid.addColumnText(
          'employeePercent',
          terms['time.employee.employee.percent'],
          {
            flex: 10,
            editable: false,
            enableHiding: true,
          }
        );
        this.grid.addColumnText(
          'work5DaysPerWeekString',
          terms['time.employee.work5daysperweek'],
          {
            flex: 10,
            editable: false,
            enableHiding: true,
          }
        );
        this.grid.addColumnText(
          'hasTransactionString',
          terms['time.employee.hasearnedholiday'],
          {
            flex: 10,
            editable: false,
            enableHiding: true,
          }
        );
        this.grid.addColumnText(
          'suggestionString',
          terms['time.employee.suggestionearnedholiday'],
          {
            flex: 10,
            editable: false,
            enableHiding: true,
          }
        );
        this.grid.addColumnText('suggestionNote', terms['common.note'], {
          flex: 30,
          editable: false,
          enableHiding: true,
        });
        this.grid.setRowSelection('multiRow');
        super.finalizeInitGrid();

        this.form!.patchValue({
          yearId: this.yearId,
        });
      });
  }
  createForm(element?: EarnedHolidayForm): EarnedHolidayForm {
    return new EarnedHolidayForm({
      validationHandler: this.validationHandler,
      element,
    });
  }

  loadYears(): Observable<SmallGenericType[]> {
    return this.progress.load$(
      this.service.getYears(1).pipe(
        tap(value => {
          this.years = value;
          this.yearId = new Date().getFullYear();
        })
      )
    );
  }

  loadholidays(): Observable<any[]> {
    return this.progress.load$(
      this.service.getHolidays(0, true, true).pipe(
        tap(value => {
          value.forEach((holiday: any) => {
            const year = new Date(holiday.date).getFullYear();
            if (!this.holidays[year]) {
              this.holidays[year] = [];
            }
            this.holidays[year].push(
              new SmallGenericType(holiday.holidayId, `${holiday.name}`)
            );
            this.holidayCount++;
          });
          this.selectedYear = [new SmallGenericType(0, '')].concat(
            this.holidays[this.yearId] || []
          );
        })
      )
    );
  }

  yearChanged(year: any): void {
    this.yearId = year;
    this.form!.patchValue({
      holidayId: 0,
    });
    this.selectedYear = [new SmallGenericType(0, '')].concat(
      this.holidays[this.yearId] || []
    );
  }

  loadGridData(clearContent: boolean = false) {
    if (clearContent) {
      this.employeeEarnedHolidays = undefined;
      this.grid.resetColumns();
      this.grid.setData([]);
      this.progress.inProgress.set(false);
    }

    const model: IEarnedHolidayModel = {
      holidayId: this.form!.value.holidayId,
      loadSuggestions: this.form!.value.loadsuggestions,
      employeeEarnedHolidays: this.employeeEarnedHolidays!,
      year: this.form!.value.yearId,
    };
    return this.progress
      .load$(
        this.service.loadEarnedHolidaysContent(model).pipe(
          tap((value: IEmployeeEarnedHolidayDTO[]) => {
            this.employeeEarnedHolidays = value;
            this.grid.resetColumns();
            this.grid.setData(this.employeeEarnedHolidays);
          })
        )
      )
      .subscribe();
  }
  initDeleteTransaction() {
    const mb = this.messageboxService.warning(
      'time.time.timeearnedholiday.deletetransactions',
      'time.time.timeearnedholiday.deletetransactionsmessage'
    );
    mb.afterClosed().subscribe((response: IMessageboxComponentResponse) => {
      if (response?.result) this.deleteTransactions();
    });
  }
  initSaveTransactions() {
    const mb = this.messageboxService.warning(
      'time.time.timeearnedholiday.savetransactions',
      'time.time.timeearnedholiday.savetransactionsmessage'
    );
    mb.afterClosed().subscribe((response: IMessageboxComponentResponse) => {
      if (response?.result) this.saveTransactions();
    });
  }

  saveTransactions() {
    const model: IManageTransactionsForEarnedHolidayModel = {
      year: this.form!.value.yearId,
      holidayId: this.form!.value.holidayId,
      employeeIds: this.grid.getSelectedRows().map(x => x.employeeId),
    };

    return of(
      this.progress.crud(
        CrudActionTypeEnum.Save,
        this.service.createTransactionsForEarnedHoliday(model).pipe(
          tap((value: any) => {
            if (value.success) {
              setTimeout((): void => {
                this.loadGridData(true);
              }, 100);
            } else {
              this.messageboxService.error(
                'time.time.timeearnedholiday.savetransactions',
                value.message
              );
            }
          })
        )
      )
    ).subscribe();
  }

  deleteTransactions() {
    const model: IManageTransactionsForEarnedHolidayModel = {
      year: this.form!.value.yearId,
      holidayId: this.form!.value.holidayId,
      employeeIds: this.grid.getSelectedRows().map(x => x.employeeId),
    };

    return of(
      this.progress.crud(
        CrudActionTypeEnum.Delete,
        this.service.deleteTransactionsForEarnedHolidayContent(model).pipe(
          tap((value: any) => {
            if (value.success) {
              setTimeout((): void => {
                this.loadGridData(true);
              }, 100);
            } else {
              this.messageboxService.error(
                'time.time.timeearnedholiday.deletetransactions',
                value.message
              );
            }
          })
        )
      )
    ).subscribe();
  }

  selctedEmployees(): number {
    return this.grid != undefined && this.grid.getSelectedRows()
      ? this.grid.getSelectedRows().length
      : 0;
  }

  loadDisabled() {
    return this.form?.value.yearId === 0 || this.form?.value.holidayId === 0;
  }

  deleteDisabled() {
    return (
      this.selctedEmployees() == 0 ||
      this.form?.value.yearId === 0 ||
      this.form?.value.holidayId === 0
    );
  }

  saveDisabled() {
    return (
      this.selctedEmployees() == 0 ||
      this.form?.value.yearId === 0 ||
      this.form?.value.holidayId === 0
    );
  }
}
