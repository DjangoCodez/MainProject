import { Component, inject, Input, Output, signal } from '@angular/core';
import {
  GetProjectTimeBlocksForMatrixModel,
  ProjectWeekReportGridHeaderDTO,
} from '@features/billing/project-time-report/models/project-time-report.model';
import { ProjectWeekReportGridHeaderForm } from '@features/billing/project-time-report/models/project-week-report-grid-header-form.model';
import { ProjectTimeReportService } from '@features/billing/project-time-report/services/project-time-report.service';
import { TranslateService } from '@ngx-translate/core';
import { ValidationHandler } from '@shared/handlers';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import { TimeProjectButtonFunctions } from '@shared/util/Enumerations';
import { MenuButtonItem } from '@ui/button/menu-button/menu-button.component';
import { Observable, take, tap } from 'rxjs';
import { EventEmitter } from '@angular/core';
import { ProjectWeekReportService } from '@features/billing/project-time-report/services/project-week-report.service';
import { DateUtil } from '@shared/util/date-util';
import { SoeConfigUtil } from '@shared/util/soeconfig-util';

@Component({
  selector: 'soe-project-week-report-grid-header',
  templateUrl: './project-week-report-grid-header.component.html',
  standalone: false,
})
export class ProjectWeekReportGridHeaderComponent {
  @Output() employeeChange = new EventEmitter();
  @Output() addNewRow = new EventEmitter();
  @Output() deleteRow = new EventEmitter();
  @Output() copyWeek = new EventEmitter();
  @Output() onSearchClick =
    new EventEmitter<GetProjectTimeBlocksForMatrixModel>();
  @Output() onWeekendClick = new EventEmitter<boolean>();

  @Input() modifyOtherEmployeesPermission = false;
  @Input() useExtendedTimeRegistration = false;
  @Input() employeesDict: ISmallGenericType[] = [];
  @Input() isDeleteDisable = signal(true);
  @Input() isCopyDisable = signal(true);
  @Input() timeProjectFrom!: Date;
  @Input()
  set showWeekend(value: boolean) {
    if (this.formHeader?.showWeekend) {
      this.formHeader.showWeekend.patchValue(value, { emitEvent: false });
    }
  }

  validationHandler = inject(ValidationHandler);
  projectService = inject(ProjectTimeReportService);
  weekReportService = inject(ProjectWeekReportService);

  addRowList: MenuButtonItem[] = [];
  currentSelectedDate!: Date;
  employeeId = SoeConfigUtil.employeeId;
  isDataFetched = signal(false);

  formHeader: ProjectWeekReportGridHeaderForm =
    new ProjectWeekReportGridHeaderForm({
      validationHandler: this.validationHandler,
      element: new ProjectWeekReportGridHeaderDTO(),
    });

  constructor(private translate: TranslateService) {
    this.searchFunctionList().subscribe();

    this.formHeader.weekNr.disable();
    this.formHeader.employeeId.patchValue(this.employeeId);
    this.currentSelectedDate = DateUtil.getDateFirstInWeek(new Date());
  }

  performFunctionAction(event: MenuButtonItem): void {
    switch (event.id) {
      case TimeProjectButtonFunctions.AddRow:
        this.addRow();
        break;
      case TimeProjectButtonFunctions.DeleteRow:
        this.deleteRows();
        break;
      case TimeProjectButtonFunctions.CopyLastWeek:
        this.copyLastWeek();
        break;
    }
  }

  dateChanged(event: Date | undefined): void {
    //right arrow
    event = event ? event : DateUtil.getDateFirstInWeek(new Date());

    if (event > this.currentSelectedDate) {
      const nextWeekFirstDay = DateUtil.getDateLastInWeek(event).addDays(1);
      this.formHeader.timeProjectFrom.patchValue(
        DateUtil.getDateFirstInWeek(nextWeekFirstDay)
      );
    }
    //left arrow
    else {
      this.formHeader.timeProjectFrom.patchValue(
        DateUtil.getDateFirstInWeek(event)
      );
    }
    this.currentSelectedDate = this.formHeader.timeProjectFrom.value;
    this.formHeader.weekNr.patchValue(
      DateUtil.getWeekNumber(this.currentSelectedDate)
    );
  }

  addRow(): void {
    this.addNewRow.emit();
  }

  deleteRows(): void {
    this.deleteRow.emit();
  }

  copyLastWeek(): void {
    this.copyWeek.emit();
  }

  employeeChanged(event: any) {
    this.isDataFetched.set(false);
    this.employeeChange.emit(event);
  }

  searchFunctionList(): Observable<unknown> {
    return this.translate
      .get([
        'core.save',
        'common.newrow',
        'core.deleterow',
        'billing.project.timesheet.copylastweek',
      ])
      .pipe(
        take(1),
        tap((terms: { [key: string]: string }) => {
          this.addRowList.push({
            id: TimeProjectButtonFunctions.AddRow,
            label: terms['common.newrow'],
            icon: 'plus',
          });
          this.addRowList.push({
            id: TimeProjectButtonFunctions.DeleteRow,
            label: this.translate.instant('core.deleterow'),
            icon: 'remove',
            disabled: this.isDeleteDisable,
          });
          this.addRowList.push({
            id: TimeProjectButtonFunctions.CopyLastWeek,
            label: this.translate.instant(
              'billing.project.timesheet.copylastweek'
            ),
            icon: 'arrow-right',
            disabled: this.isCopyDisable,
          });
        })
      );
  }

  loadGridData() {
    this.isDataFetched.set(true);
    this.onSearchClick.emit({
      selectedEmp: this.formHeader.employeeId.value,
      from: this.formHeader.timeProjectFrom.value,
    });
  }

  clickWeekendCheckbox(event: boolean | undefined) {
    this.onWeekendClick.emit(event);
  }
}
