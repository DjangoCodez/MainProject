import { Component, inject, OnInit, signal } from '@angular/core';
import { PlacementsService } from '@features/time/placements/services/placements.service';
import { PlanningEmployeeDTO } from '@features/time/schedule-planning/models/employee.model';
import { EditBaseDirective } from '@shared/directives/edit-base/edit-base.directive';
import { ValidationHandler } from '@shared/handlers';
import {
  CompanySettingType,
  Feature,
  TermGroup,
  TermGroup_EmployeeRequestStatus,
  TermGroup_TimeScheduleTemplateBlockShiftUserStatus,
} from '@shared/models/generated-interfaces/Enumerations';
import {
  IEmployeeRequestDTO,
  IShiftHistoryDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { IShiftDTO } from '@shared/models/generated-interfaces/TimeSchedulePlanningDTOs';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { DateUtil } from '@shared/util/date-util';
import { DialogService } from '@ui/dialog/services/dialog.service';
import { ToolbarEditConfig } from '@ui/toolbar/models/toolbar';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { Observable, switchMap, tap } from 'rxjs';
import {
  AbsenceQuickDialogComponent,
  IAbsenceQuickDialogData,
} from '../../dialogs/absence-quick-dialog/absence-quick-dialog.component';
import { AbsenceRequestsForm } from '../../models/absence-requests-form.model';
import { AbsenceService } from '../../services/absence.service';
import { CoreService } from '@shared/services/core.service';
import { SettingsUtil } from '@shared/util/settings-util';

@Component({
  selector: 'soe-absence-requests-edit',
  standalone: false,
  templateUrl: './absence-requests-edit.component.html',
  providers: [FlowHandlerService, ToolbarService],
})
export class AbsenceRequestsEditComponent
  extends EditBaseDirective<
    IEmployeeRequestDTO,
    AbsenceService,
    AbsenceRequestsForm
  >
  implements OnInit
{
  // Services
  readonly service = inject(AbsenceService);
  private readonly placementsService = inject(PlacementsService);
  private readonly dialogService = inject(DialogService);
  private readonly validationHandler = inject(ValidationHandler);
  private readonly coreService = inject(CoreService);

  // Flags
  readonly hiddenEmployeeId = signal(0);
  //TODO: Do i need all?
  readonly sendXEMailOnChangeDefault = signal(false);
  readonly setApprovedYesAsDefault = signal(false);
  readonly onlyNoReplacementIsSelectable = signal(false);
  readonly isScenario = signal(false);

  // Data
  employeeList: PlanningEmployeeDTO[] = [];
  employees: SmallGenericType[] = [];
  timeDeviationCauses: SmallGenericType[] = [];
  shifts: IShiftDTO[] = [];
  approvalTypes: SmallGenericType[] = [];

  /* TODO: 
  Notes / comments are required on save depending on a setting
  */

  ngOnInit() {
    super.ngOnInit();

    this.startFlow(Feature.Time_Schedule_AbsenceRequests, {
      lookups: [
        this.loadHiddenEmployeeId(),
        this.getAbsenceRequestHistory(this.form?.employeeRequestId.value),
        this.loadEmployees(),
        this.loadTimeDeviationCausesAbsenceFromEmployeeId(),
        this.loadApprovalTypes(),
        // this.loadEmployeesForAbsencePlanning(),
        // this.loadAbsenceRequestAffectedShifts(),
      ],
    });

    // console.log(typeof this.form?.getRawValue());
    const formvalue: IEmployeeRequestDTO = this.form?.getRawValue();
    console.log(formvalue);
  }

  override createEditToolbar(config?: Partial<ToolbarEditConfig>): void {
    super.createEditToolbar({
      hideCopy: true,
    });
  }

  override loadData(): Observable<void> {
    return this.performLoadData.load$(
      (<any>this.service).get(this.form?.getIdControl()?.value).pipe(
        tap(value => {
          this.form?.reset(value);
        }),
        switchMap(() => this.loadShifts()),
        tap((shifts: IShiftDTO[]) => {
          this.shifts = shifts;
          this.form?.populateShiftsForm(shifts);
        })
      ),
      { showDialogDelay: 0 }
    );
  }

  override loadCompanySettings() {
    const settingTypes: number[] = [
      CompanySettingType.TimeSetApprovedYesAsDefault,
      CompanySettingType.TimeOnlyNoReplacementIsSelectable,
      CompanySettingType.TimeSchedulePlanningSendXEMailOnChange,
    ];

    return this.coreService.getCompanySettings(settingTypes).pipe(
      tap(setting => {
        // console.log('setting:', setting);
        this.setApprovedYesAsDefault.set(
          SettingsUtil.getBoolCompanySetting(
            setting,
            CompanySettingType.TimeSetApprovedYesAsDefault
          )
        );
        this.onlyNoReplacementIsSelectable.set(
          SettingsUtil.getBoolCompanySetting(
            setting,
            CompanySettingType.TimeOnlyNoReplacementIsSelectable
          )
        );
        this.sendXEMailOnChangeDefault.set(
          SettingsUtil.getBoolCompanySetting(
            setting,
            CompanySettingType.TimeSchedulePlanningSendXEMailOnChange
          )
        );
      })
    );
  }

  private getAbsenceRequestHistory(id: number): Observable<IShiftHistoryDTO> {
    return this.service.getAbsenceRequestHistory(id).pipe(
      tap(x => {
        console.log(x);
      })
    );
  }

  private loadHiddenEmployeeId(): Observable<number> {
    return this.placementsService.getHiddenEmployeeId().pipe(
      tap(x => {
        this.hiddenEmployeeId.set(x);
      })
    );
  }

  private loadEmployees(): Observable<PlanningEmployeeDTO[]> {
    // if / else from angular
    // if (this.isAttestRoleMode() && this.parentMode !== AbsenceRequestParentMode.TimeAttest /*&& this.isEmployeeRequestGuiMode()*/)
    return this.loadEmployeesForAbsencePlanning();
    // else
    // return this.loadEmployee();
  }

  private loadEmployeesForAbsencePlanning(): Observable<PlanningEmployeeDTO[]> {
    console.log(DateUtil.toDateString(this.form?.controls.start.value));
    const employeeId = this.form?.controls.employeeId.value;
    const dateFrom = this.form?.controls.start.value;
    const dateTo = this.form?.controls.stop.value;
    return this.service
      .getEmployeesForAbsencePlanning(
        DateUtil.toDateString(dateFrom),
        DateUtil.toDateString(dateTo),
        employeeId,
        true
      )
      .pipe(
        tap(x => {
          this.employeeList = x;
          this.employees = this.service.formatEmployeeListForDisplay(
            x.filter(emp => {
              const isSpecialEmployee =
                emp.employeeId === this.service.NO_REPLACEMENT_EMPLOYEEID ||
                emp.employeeId === this.hiddenEmployeeId();
              return !isSpecialEmployee;
            }),
            this.hiddenEmployeeId()
          );

          // this.employeeList = this.service.getFilteredReplacementEmployees(
          //   x,
          //   this.onlyNoReplacementIsSelectable(),
          //   employeeId,
          //   this.hiddenEmployeeId(),
          //   dateFrom,
          //   dateTo,
          //   true,
          //   false
          // );
          console.log(this.employeeList);
        })
      );
  }

  private loadApprovalTypes(): Observable<SmallGenericType[]> {
    return this.coreService
      .getTermGroupContent(TermGroup.YesNo, true, false)
      .pipe(tap(x => (this.approvalTypes = x)));
  }

  //   private loadTimeDeviationCauseRequests(): ng.IPromise<any> {
  //     if (this.isEmployeeMode()) {
  //         return this.sharedTimeService.getTimeDeviationCauseRequests(this.employeeId, this.employeeGroupId).then((x) => {
  //             this.deviationCauses = x;
  //         });
  //     }
  //     else {
  //         if (this.employeeId > 0) {
  //             return this.sharedTimeService.getAbsenceTimeDeviationCausesFromEmployeeId(this.employeeId, this.selectedDateFrom ? this.selectedDateFrom : new Date(Date.now()), this.isEmployeeMode()).then((x) => {
  //                 this.deviationCauses = x
  //             });
  //         }
  //         else {
  //             return this.sharedTimeService.getAbsenceTimeDeviationCauses().then((x) => {
  //                 this.deviationCauses = x;
  //             });
  //         }
  //     }
  // }
  // private loadTimeDeviationCauses(): Observable<SmallGenericType[]> {
  //   const employeeId = this.form?.controls.employeeId.value;
  //   if (employeeId > 0) {

  //   }
  //   else {
  //     return this.service.getTimeDeviationCausesAbsenceDict();

  //   }
  // }
  // private loadTimeDeviationCausesAbsenceFromEmployeeId(): Observable<
  //   SmallGenericType[]
  // > {
  //   const isEmployeeMode = false;
  //   return this.service
  //     .getTimeDeviationCausesAbsenceFromEmployeeId(
  //       this.form?.controls.employeeId.value,
  //       DateUtil.toDateString(
  //         this.form?.controls.start.value ?? DateUtil.getToday()
  //       ),
  //       isEmployeeMode
  //     )
  //     .pipe(
  //       map(
  //         causes =>
  //           (this.timeDeviationCauses = causes.map(cause => {
  //             return new SmallGenericType(
  //               cause.timeDeviationCauseId,
  //               cause.name
  //             );
  //           }))
  //       )
  //     );
  // }

  private loadTimeDeviationCausesAbsenceFromEmployeeId(): Observable<
    SmallGenericType[]
  > {
    const isEmployeeMode = false;
    return this.service
      .getTimeDeviationCausesAbsenceFromEmployeeIdSmall(
        this.form?.controls.employeeId.value,
        this.form?.controls.start.value ?? DateUtil.getToday(),
        isEmployeeMode
      )
      .pipe(tap(causes => (this.timeDeviationCauses = causes)));
  }

  private loadShifts(): Observable<IShiftDTO[]> {
    const status = this.form?.value.status;
    if (status === TermGroup_EmployeeRequestStatus.RequestPending)
      return this.loadAbsenceRequestAffectedShifts();
    return this.loadAbsenceAffectedShifts();
  }
  // TODO: SHOULD NOT RUN IF employeeRequest.status === RequestPending ??
  private loadAbsenceRequestAffectedShifts(): Observable<IShiftDTO[]> {
    const request: IEmployeeRequestDTO = this.form?.getRawValue();
    const extendedSettings = request?.extendedSettings;
    const shiftUserStatus =
      TermGroup_TimeScheduleTemplateBlockShiftUserStatus.AbsenceRequested;
    const timeScheduleScenarioHeadId = 0; //TODO: FIX
    return this.service.getAbsenceRequestAffectedShifts(
      request,
      extendedSettings,
      shiftUserStatus,
      timeScheduleScenarioHeadId
    );
  }

  private loadAbsenceAffectedShifts(): Observable<IShiftDTO[]> {
    const request: IEmployeeRequestDTO = this.form?.getRawValue();
    const employeeId = request.employeeId;
    const start = request.start;
    const stop = request.stop.beginningOfDay().addDays(1).addSeconds(-1); //TODO: Make automatic?
    const timeDeviationCauseId = request.timeDeviationCauseId || 0;
    const extendedSettings = request.extendedSettings;
    const includeAlreadyAbsence = false; //TODO: ?
    // const timeScheduleScenarioHeadId = 0; //TODO: Fix
    return this.service.getAbsenceAffectedShifts(
      employeeId,
      start,
      stop,
      timeDeviationCauseId,
      extendedSettings,
      includeAlreadyAbsence
    );
  }

  //#region Events
  // testButton() {
  //   console.log('hej');
  //   const form = this.form?.getRawValue();
  //   const dialogData: IAbsenceQuickDialogData = {
  //     size: 'lg',
  //     title: 'test',
  //     disableClose: true,
  //     employeeId: form.employeeId,
  //     dateFrom: form.start,
  //     dateTo: form.stop,
  //     employeeName: form.employeeName,
  //     shiftIds: [6506398], //emp 104 10/20
  //   };
  //   console.log(
  //     `getting shiftIdss ${dialogData.shiftIds} for ${dialogData.employeeName}`
  //   );
  //   this.dialogService.open(AbsenceQuickDialogComponent, dialogData);
  // }
  testButton() {
    console.log('hej');
    const form = this.form?.getRawValue();
    const dialogData: IAbsenceQuickDialogData = {
      size: 'lg',
      title: 'test',
      disableClose: true,
      employeeId: form.employeeId,
      dateFrom: form.start,
      dateTo: form.stop,
      employeeName: form.employeeName,
      shiftIds: [6506423, 10247601], //emp 104 10/20
    };
    console.log(
      `getting shiftIdss ${dialogData.shiftIds} for ${dialogData.employeeName}`
    );
    this.dialogService.open(AbsenceQuickDialogComponent, dialogData);
  }
}
