import { Component, inject, OnInit } from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { ValidationHandler } from '@shared/handlers';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import {
  IEmployeeSmallDTO,
  ITimeStampEntryDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { Perform } from '@shared/util/perform.class';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import _ from 'lodash';
import { Observable, take, tap } from 'rxjs';
import { EmployeeService } from '@features/time/services/employee.service';
import { AdjustTimeStampsService } from '../../services/adjust-time-stamps.service';
import { AdjustTimeStampsForm } from '../../models/adjust-time-stamps-form.model';
import { ISearchTimeStampModel } from '@shared/models/generated-interfaces/TimeModels';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { CrudActionTypeEnum } from '@shared/enums';
import { DialogService } from '@ui/dialog/services/dialog.service';
import {
  ITimeStampDetailsDialogData,
  TimeStampDetailsDialogData,
} from '@shared/components/time/time-stamp-details-dialog/time-stamp-details-controller';

@Component({
  selector: 'soe-adjust-time-stamps-grid',
  standalone: false,
  templateUrl: 'adjust-time-stamps-grid.component.html',
  providers: [FlowHandlerService, ToolbarService],
})
export class AdjustTimeStampsGridComponent
  extends GridBaseDirective<any, AdjustTimeStampsService>
  implements OnInit
{
  service = inject(AdjustTimeStampsService);
  employeeService = inject(EmployeeService);
  validationHandler = inject(ValidationHandler);
  dialogService = inject(DialogService);

  form!: AdjustTimeStampsForm;

  progress = new Perform<any[]>(this.progressService);
  performSaveData = new Perform<AdjustTimeStampsService>(this.progressService);
  showGrid = false;
  employeeAdjustTimeStamps: ITimeStampEntryDTO[] = [];
  employees: SmallGenericType[] = [];
  pendingSearchResults: ITimeStampEntryDTO[] | null = null;

  terms: any = {};
  ngOnInit() {
    super.ngOnInit();
    this.form = this.createForm();
    this.startFlow(
      Feature.Time_Time_Attest_AdjustTimeStamps,
      'Time.Time.AdjustTimeStamps',
      {
        skipInitialLoad: true,
        lookups: [this.loadEmployees()],
      }
    );
  }

  override createGridToolbar(): void {
    super.createGridToolbar({
      hideReload: true,
      hideClearFilters: true,
    });
  }

  override onGridReadyToDefine(grid: GridComponent<ITimeStampEntryDTO>) {
    this.grid = grid;
    super.onGridReadyToDefine(grid);
    this.grid.api.updateGridOptions({
      onCellValueChanged: this.onCellChanged.bind(this),
    });
    this.translate
      .get([
        'time.employee.employee.employeenr',
        'common.name',
        'time.time.timeterminal.timeterminal',
        'common.type',
        'time.time.adjusttimestamps.adjusted',
        'common.date',
        'common.time',
        'time.time.adjusttimestamps.belongstodate',
        'common.time.timedeviationcause',
        'common.accounting',
        'core.comment',
        'common.entitylogviewer.changelog',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.terms = terms;

        this.grid.addColumnModified('isModified', {
          editable: false,
          flex: 1,
        });
        this.grid.addColumnText(
          'employeeNr',
          terms['time.employee.employee.employeenr'],
          { editable: false, enableHiding: false, flex: 5 }
        );
        this.grid.addColumnText('employeeName', terms['common.name'], {
          editable: false,
          flex: 20,
          enableHiding: true,
        });
        this.grid.addColumnText(
          'timeTerminalId',
          terms['time.time.timeterminal.timeterminal'],
          { editable: false, enableHiding: true, flex: 20 }
        );
        this.grid.addColumnText('typeName', terms['common.type'], {
          editable: false,
          flex: 5,
          enableHiding: true,
        });
        this.grid.addColumnBool(
          'manuallyAdjusted',
          terms['time.time.adjusttimestamps.adjusted'],
          { editable: false, enableHiding: true, flex: 5 }
        );
        this.grid.addColumnDate('date', terms['common.date'], {
          editable: false,
          enableHiding: true,
          flex: 10,
        });
        this.grid.addColumnTime('adjustedTime', terms['common.time'], {
          editable: true,
          enableHiding: true,
          flex: 5,
        });
        this.grid.addColumnDate(
          'adjustedTimeBlockDateDate',
          terms['time.time.adjusttimestamps.belongstodate'],
          {
            editable: true,
            enableHiding: true,
            flex: 10,
          }
        );
        this.grid.addColumnText(
          'timeDeviationCauseName',
          terms['common.time.timedeviationcause'],
          { editable: false, enableHiding: true, flex: 10 }
        );
        this.grid.addColumnText('accountName', terms['common.accounting'], {
          editable: false,
          enableHiding: true,
          flex: 15,
        });
        this.grid.addColumnText('note', terms['core.comment'], {
          editable: false,
          enableHiding: true,
          flex: 20,
        });

        this.grid.addColumnIcon(null, ' ', {
          iconName: 'history',
          onClick: row => {
            this.showTimeStampDetails(row.timeStampEntryId);
          },
          filter: true,
          showSetFilter: true,
        });
        super.finalizeInitGrid();

        if (this.pendingSearchResults) {
          this.grid.setData(this.pendingSearchResults);
          this.pendingSearchResults = null;
        } else {
          this.grid.setData([]);
        }
      });
  }

  onCellChanged(params: any): void {
    if (params.oldValue !== params.newValue) {
      const oldValue =
        params.oldValue instanceof Date
          ? params.oldValue.toISOString()
          : params.oldValue;
      const newValue =
        params.newValue instanceof Date
          ? params.newValue.toISOString()
          : params.newValue;

      if (oldValue === newValue) {
        return;
      }

      params.data.isModified = true;

      params.api.refreshCells({
        rowNodes: [params.node],
        columns: ['isModified'],
        force: true,
      });

      this.form.markAsDirty();
    }
  }
  createForm(element?: AdjustTimeStampsForm): AdjustTimeStampsForm {
    return new AdjustTimeStampsForm({
      validationHandler: this.validationHandler,
      element,
    });
  }
  checkSelectedEmployees(): boolean {
    return (
      this.form.value.selectedEmployees != null &&
      this.form.value.selectedEmployees.length > 0
    );
  }
  checkModifiedRows(): boolean {
    if (!this.grid) {
      return false;
    }
    const allRows: any[] = [];
    this.grid.api?.forEachNode((node: any) => {
      if (node.data) {
        allRows.push(node.data);
      }
    });

    return allRows.some(row => row.isModified === true);
  }
  search() {
    const employeeIds = this.form?.value.selectedEmployees;

    const model: ISearchTimeStampModel = {
      employeeIds: employeeIds,
      dateFrom: this.form?.value.selectedDateFrom,
      dateTo: this.form?.value.selectedDateTo,
    };
    return this.progress
      .load$(
        this.service.searchTimeStamps(model).pipe(
          tap((x: ITimeStampEntryDTO[]) => {
            this.showGrid = true;
            this.employeeAdjustTimeStamps = x;

            this.employeeAdjustTimeStamps.forEach(y => {
              if (y.accountName && y.accountNr) {
                y.accountName = y.accountNr + ' - ' + y.accountName;
              }
              y.adjustedTime = y.time;
            });

            this.form.patchRows(this.employeeAdjustTimeStamps);
            if (this.grid) {
              this.grid.setData(this.employeeAdjustTimeStamps);
            } else {
              this.pendingSearchResults = this.employeeAdjustTimeStamps;
            }
          })
        )
      )
      .subscribe();
  }

  loadEmployees(): Observable<any[]> {
    const params = {
      dateFrom: new Date(),
      dateTo: new Date(),
      employeeIds: [],
      showInactive: false,
      showEnded: false,
      showNotStarted: false,
      filterOnAnnualLeaveAgreement: false,
    };
    return this.progress.load$(
      this.employeeService.getEmployeesForGridSmall(params).pipe(
        tap((x: IEmployeeSmallDTO[]) => {
          this.employees = x.map(
            (employee: IEmployeeSmallDTO) =>
              ({
                id: employee.employeeId,
                name: employee.employeeNr + ' - ' + employee.name,
              }) as SmallGenericType
          );
        })
      )
    );
  }

  save() {
    const modifiedRows = this.employeeAdjustTimeStamps.filter(
      e => e.isModified
    );

    if (modifiedRows.length === 0) {
      return;
    }

    this.performSaveData.crud(
      CrudActionTypeEnum.Save,
      this.service.saveAdjustedTimeStampEntries(modifiedRows).pipe(
        tap(x => {
          if (x.success) {
            this.search();
          }
        })
      )
    );
  }
  showTimeStampDetails(timeStampEntryId: number) {
    this.dialogService
      .open(TimeStampDetailsDialogData, {
        title: this.translate.instant('time.time.attest.timestamps.details'),
        size: 'large',
        hideFooter: true,
        timeStampEntryId: timeStampEntryId,
      } as unknown as ITimeStampDetailsDialogData)
      .afterClosed()
      .pipe(take(1))
      .subscribe(() => {});
  }
  checkSelectedDates(): boolean {
    return (
      this.form.value.selectedDateFrom != null &&
      this.form.value.selectedDateTo != null &&
      this.form.value.selectedDateTo >= this.form.value.selectedDateFrom
    );
  }
}
