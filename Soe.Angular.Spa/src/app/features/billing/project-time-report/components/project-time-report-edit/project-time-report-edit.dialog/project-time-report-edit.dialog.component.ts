import { Component, inject, signal } from '@angular/core';
import {
  EditTimeReportDialogDTO,
  IEmployeeTimeCodeDTO,
  ProjectTimeBlockDTO,
  ProjectTimeBlockSaveDTO,
  ValidateProjectTimeBlockSaveDTO,
} from '@features/billing/project-time-report/models/project-time-report.model';
import { ProjectTimeReportService } from '@features/billing/project-time-report/services/project-time-report.service';
import { CrudActionTypeEnum } from '@shared/enums';
import { SoeEntityState } from '@shared/models/generated-interfaces/Enumerations';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import {
  IEmployeeProjectInvoiceDTO,
  IProjectInvoiceSmallDTO,
  IProjectSmallDTO,
} from '@shared/models/generated-interfaces/ProjectDTOs';
import {
  IEmployeeScheduleTransactionInfoDTO,
  ITimeDeviationCauseDTO,
  IValidateProjectTimeBlockSaveDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import { Perform } from '@shared/util/perform.class';
import { ProjectTimeRegistrationType } from '@shared/util/Enumerations';
import { DialogComponent } from '@ui/dialog/dialog/dialog.component';
import { MessageboxService } from '@ui/dialog/services/messagebox.service';
import { BehaviorSubject, tap } from 'rxjs';
import { ResponseUtil } from '@shared/util/response-util';

@Component({
  selector: 'soe-project-time-report-edit.dialog',
  templateUrl: './project-time-report-edit.dialog.component.html',
  providers: [FlowHandlerService],
  standalone: false,
})
export class ProjectTimeReportEditDialogComponent extends DialogComponent<EditTimeReportDialogDTO> {
  employeesDict: ISmallGenericType[] = [];
  employees: IEmployeeTimeCodeDTO[] = [];
  causesDict: ISmallGenericType[] = [];
  projects = new BehaviorSubject<IProjectSmallDTO[]>([]);
  projectInvoices!: IEmployeeProjectInvoiceDTO[];
  timeDeviationCauses!: ITimeDeviationCauseDTO[];
  orders = new BehaviorSubject<IProjectInvoiceSmallDTO[]>([]);
  employee!: IEmployeeTimeCodeDTO;
  employeeDaysWithSchedule: IEmployeeScheduleTransactionInfoDTO[] = [];
  rowData = new BehaviorSubject<ProjectTimeBlockDTO[]>([]);
  $addRow = new BehaviorSubject<boolean>(false);
  workTimePermission = false;
  invoiceTimePermission = false;
  invoiceTimeAsWorkTime = false;
  registrationType: ProjectTimeRegistrationType;
  useExtendedTimeRegistration = false;
  defaultTimeCodeId = 0;
  isTimeSheet = false;
  isProjectCentral = false;
  isNew = signal(true);
  isDirty = signal(false);
  employeeId: number = 0;
  projectId: number = 0;

  service = inject(ProjectTimeReportService);
  coreService = inject(CoreService);
  messageBoxService = inject(MessageboxService);

  perform = new Perform<any>(this.progressService);

  constructor(private progressService: ProgressService) {
    super();

    this.employeesDict = this.data.employeesDict;
    this.employees = this.data.employees;
    this.projectInvoices = this.data.projectInvoices;
    this.causesDict = this.data.timeDeviationCauseDict;
    this.projects = this.data.projects;
    this.timeDeviationCauses = this.data.timeDeviationCauses;
    this.orders = this.data.orders;
    this.workTimePermission = this.data.workTimePermission;
    this.invoiceTimePermission = this.data.invoiceTimePermission;
    this.invoiceTimeAsWorkTime = this.data.invoiceTimeAsWorkTime;
    this.useExtendedTimeRegistration = this.data.useExtendedTimeRegistration;
    this.employeeId = this.data.employeeId;
    this.projectId = this.data.projectId;
    this.employee = this.data.employee;
    this.isTimeSheet = this.data.isTimeSheet;
    this.isProjectCentral = this.data.isProjectCentral;
    this.employeeDaysWithSchedule = this.data.employeeDaysWithSchedule;
    this.registrationType =
      this.isTimeSheet || this.isProjectCentral
        ? ProjectTimeRegistrationType.TimeSheet
        : ProjectTimeRegistrationType.Order;
    this.isNew.set(this.data.isNew());

    if (this.data.rows) this.rowData.next(this.data.rows);
  }

  cancel() {
    this.dialogRef.close();
  }

  save() {
    const saveModel: IValidateProjectTimeBlockSaveDTO[] = [];
    let count = 1;

    this.rowData.value.forEach(row => {
      if (row.isModified) {
        const model = new ValidateProjectTimeBlockSaveDTO();
        model.autoGenTimeAndBreakForProject = row.autoGenTimeAndBreakForProject;
        model.employeeId = row.employeeId;

        model.rows.push({
          id: row.projectTimeBlockId || count,
          workDate: row.date,
          startTime: row.startTime,
          stopTime: row.stopTime,
          timeDeviationCauseId: row.timeDeviationCauseId,
          employeeChildId: row.employeeChildId || 0,
        });
        count++;

        saveModel.push(model);
      }
    });

    this.perform.crud(
      CrudActionTypeEnum.Save,
      this.service.validateProjectTimeBlockSaveDTO(saveModel),
      result => {
        const errorMsg = ResponseUtil.getErrorMessage(result);
        const infoMsg = ResponseUtil.getMessageValue(result);
        if (errorMsg && errorMsg.length > 0) {
          this.messageBoxService.warning('core.error', errorMsg);
        } else if (infoMsg && infoMsg.length > 0) {
          const mb = this.messageBoxService.warning('core.warning', infoMsg);
          mb.afterClosed().subscribe(() => {
            this.saveRows();
          });
        } else this.saveRows();
      },
      undefined,
      {
        showToast: false,
        showToastOnComplete: false,
      }
    );
  }

  private saveRows() {
    const saveRows: ProjectTimeBlockDTO[] = [];
    this.rowData.value.forEach(row => {
      if (row.isModified) saveRows.push(row);
    });

    const _model: ProjectTimeBlockSaveDTO[] = [];

    if (saveRows.length > 0) {
      saveRows.forEach(row => {
        const model = new ProjectTimeBlockSaveDTO();

        model.customerInvoiceId = row.customerInvoiceId;
        model.date = row.date;
        model.employeeId = row.employeeId;
        model.from = row.startTime;
        model.to = row.stopTime;
        model.timeDeviationCauseId = row.timeDeviationCauseId;
        model.externalNote = row.externalNote || '';
        model.internalNote = row.internalNote;
        model.invoiceQuantity = row.invoiceQuantity || 0;
        model.isFromTimeSheet = this.isTimeSheet || this.isProjectCentral;
        model.projectId = this.isTimeSheet ? row.projectId : this.projectId;
        model.projectInvoiceDayId = 0;
        model.projectInvoiceWeekId = row.projectInvoiceWeekId;
        model.state = row.isDeleted
          ? SoeEntityState.Deleted
          : SoeEntityState.Active;
        model.timeBlockDateId = row.timeBlockDateId;
        model.timeCodeId = row.timeCodeId;
        model.timePayrollQuantity = row.timePayrollQuantity || 0;
        model.timeSheetWeekId = row.timeSheetWeekId;
        model.autoGenTimeAndBreakForProject = row.autoGenTimeAndBreakForProject;
        model.employeeChildId = row.employeeChildId || 0;
        model.projectTimeBlockId = row.projectTimeBlockId;
        model.isFromTimeSheet = true;

        _model.push(model);
      });

      this.perform.crud(
        CrudActionTypeEnum.Save,
        this.service.saveProjectTimeBlockSaveDTO(_model).pipe(
          tap(saveResult => {
            if (!saveResult.success) {
              this.messageBoxService.error(
                'error.unabletosave_title',
                ResponseUtil.getErrorMessage(saveResult) || ''
              );
            } else {
              this.dialogRef.close(this.rowData.value);
            }
          })
        )
      );
    }
  }

  addRow() {
    this.$addRow.next(true);
  }
}
