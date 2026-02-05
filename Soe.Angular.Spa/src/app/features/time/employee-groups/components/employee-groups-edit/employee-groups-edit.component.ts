import { Component, inject, OnInit, signal } from '@angular/core';
import {
  AccountDims,
  AccountDimsForm,
  SelectedAccounts,
  SelectedAccountsChangeSet,
} from '@shared/components/account-dims/account-dims-form.model';
import { EditBaseDirective } from '@shared/directives/edit-base/edit-base.directive';
import { CrudActionTypeEnum } from '@shared/enums';
import { ValidationHandler } from '@shared/handlers';
import { TermCollection } from '@shared/localization/term-types';
import { SmallGenericType } from '@shared/models/generic-type.model';
import {
  AttestPeriodType,
  CompanySettingType,
  Feature,
  TermGroup,
  TermGroup_TimeReportType,
} from '@shared/models/generated-interfaces/Enumerations';
import {
  IEmployeeGroupAttestTransitionDTO,
  IEmployeeGroupDayTypeDTO,
  IEmployeeGroupDTO,
  IEmployeeGroupRuleWorkTimePeriodDTO,
  IEmployeeGroupTimeDeviationCauseDTO,
  IEmployeeGroupTimeDeviationCauseTimeCodeDTO,
  ITimeAccumulatorEmployeeGroupRuleDTO,
  ITimeDeviationCauseDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ProgressOptions } from '@shared/services/progress';
import { SettingsUtil } from '@shared/util/settings-util';
import { TimeboxValue } from '@ui/forms/timebox/timebox.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { Observable, of, tap } from 'rxjs';
import { EmployeeGroupsForm } from '../../models/employee-groups-form.model';
import { EmployeeGroupsService } from '../../services/employee-groups.service';
@Component({
  selector: 'soe-employee-groups-edit',
  standalone: false,
  templateUrl: './employee-groups-edit.component.html',
  styleUrl: './employee-groups-edit.component.scss',
  providers: [FlowHandlerService, ToolbarService],
})
export class EmployeeGroupsEditComponent
  extends EditBaseDirective<
    IEmployeeGroupDTO,
    EmployeeGroupsService,
    EmployeeGroupsForm
  >
  implements OnInit
{
  readonly service = inject(EmployeeGroupsService);
  readonly coreService = inject(CoreService);

  accountDimsCostForm!: AccountDimsForm;
  accountsDimsCost!: AccountDims;

  accountDimsIncomeForm!: AccountDimsForm;
  accountsDimsIncome!: AccountDims;

  payrollAccountingPriosTermGroup =
    TermGroup.EmployeeGroupPayrollProductAccountingPrio;
  invoiceAccountingPriosTermGroup =
    TermGroup.EmployeeGroupInvoiceProductAccountingPrio;

  useAccountHierarchy = signal(false);
  showNotifyChangeOfDeviations = signal(false);
  showAutogenTimeblocks = signal(false);

  attestStates: SmallGenericType[] = [];
  reminderPeriodTypes: SmallGenericType[] = [];

  initialized = signal(false);

  ngOnInit() {
    super.ngOnInit();

    this.startFlow(Feature.Time_Employee_Groups_Edit, {
      lookups: [
        this.loadDayOfWeeks(),
        this.loadTimeDeviationCauses(),
        this.loadQualifyingDayCalculationRules(),
        this.loadTimeWorkReductionCalculationRule(),
        this.loadTimeCodes(),
        this.loadAttestStates(),
        this.loadTimeDeviationCausesAbsence(),
      ],
    });

    this.accountDimsCostForm = new AccountDimsForm({
      accountDimsValidationHandler: new ValidationHandler(
        this.translate,
        this.messageboxService
      ),
      element: this.accountsDimsCost,
    });

    this.accountDimsIncomeForm = new AccountDimsForm({
      accountDimsValidationHandler: new ValidationHandler(
        this.translate,
        this.messageboxService
      ),
      element: this.accountsDimsIncome,
    });
  }

  override loadData(): Observable<void> {
    return this.performLoadData.load$(
      this.service.get(this.form?.getIdControl()?.value).pipe(
        tap((value: IEmployeeGroupDTO) => {
          this.accountsDimsCost = {
            account1: value.defaultDim1CostAccountId ?? 0,
            account2: value.defaultDim2CostAccountId ?? 0,
            account3: value.defaultDim3CostAccountId ?? 0,
            account4: value.defaultDim4CostAccountId ?? 0,
            account5: value.defaultDim5CostAccountId ?? 0,
            account6: value.defaultDim6CostAccountId ?? 0,
          };

          this.accountsDimsIncome = {
            account1: value.defaultDim1IncomeAccountId ?? 0,
            account2: value.defaultDim2IncomeAccountId ?? 0,
            account3: value.defaultDim3IncomeAccountId ?? 0,
            account4: value.defaultDim4IncomeAccountId ?? 0,
            account5: value.defaultDim5IncomeAccountId ?? 0,
            account6: value.defaultDim6IncomeAccountId ?? 0,
          };

          // Reset the account dims form with the values
          this.accountDimsCostForm.reset(this.accountsDimsCost);
          this.accountDimsIncomeForm.reset(this.accountsDimsIncome);
          this.form?.customPatchValue(value);
          this.setForm();
        })
      )
    );
  }

  setForm(): void {
    this.form?.setInitialFormattedTimeboxValues();
    this.form?.setBreakSettingsFormLogic();
    this.setHiddenFormLogic(this.form?.timeReportType.value);
  }

  override onFinished(): void {
    this.initialized.set(true);
  }

  override newRecord(): Observable<void> {
    if (this.form?.isCopy) {
      this.setForm();
      this.onFinished();
    }
    return of();
  }

  override loadCompanySettings() {
    const settingTypes: number[] = [];
    settingTypes.push(CompanySettingType.UseAccountHierarchy);
    return this.coreService.getCompanySettings(settingTypes).pipe(
      tap(setting => {
        this.useAccountHierarchy.set(
          SettingsUtil.getBoolCompanySetting(
            setting,
            CompanySettingType.UseAccountHierarchy
          )
        );
      })
    );
  }

  override loadTerms(translationsKeys?: string[]): Observable<TermCollection> {
    return super
      .loadTerms([
        'time.employee.employeegroup.theday',
        'time.employee.employeegroup.theweek',
        'time.employee.employeegroup.themonth',
        'time.employee.employeegroup.theperiod',
      ])
      .pipe(
        tap((terms: TermCollection) => {
          // Must be run after terms are retrieved. onFinished doesn't guarantee that terms are loaded before executing.
          this.loadReminderPeriodTypes(terms);
        })
      );
  }

  override performSave(options?: ProgressOptions | undefined): void {
    if (!this.form || !this.service) return;

    // Remapping values to correct DTO format
    const dayTypeIds: number[] = this.form?.dayTypeIds?.value.map(id => {
      return id.id;
    });

    const timeDeviationCauseRequestIds: number[] =
      this.form?.timeDeviationCauseRequestIds?.value.map(id => {
        return id.id;
      });

    const timeDeviationCauseAbsenceAnnouncementIds: number[] =
      this.form?.timeDeviationCauseAbsenceAnnouncementIds?.value.map(id => {
        return id.id;
      });

    const timeCodeIds: number[] = this.form?.timeCodeIds?.value.map(id => {
      return id.id;
    });

    // Remove timePeriodHead extension
    const ruleWorkTimePeriods: IEmployeeGroupRuleWorkTimePeriodDTO[] =
      this.form?.ruleWorkTimePeriods?.value.map(period => {
        const { timePeriodHeadId, ...periodWithoutHead } = period;
        return periodWithoutHead;
      });

    const timeAccumulatorEmployeeGroupRules: ITimeAccumulatorEmployeeGroupRuleDTO[] =
      this.form?.timeAccumulatorEmployeeGroupRules.value; // Not sure why I have to do this, it should work automatically.

    const timeDeviationCauses: ITimeDeviationCauseDTO[] =
      this.form?.timeDeviationCauses.value;

    const employeeGroupDayType: IEmployeeGroupDayTypeDTO[] =
      this.form?.employeeGroupDayType.value;

    const employeeGroupTimeDeviationCauseTimeCode: IEmployeeGroupTimeDeviationCauseTimeCodeDTO[] =
      this.form?.employeeGroupTimeDeviationCauseTimeCode.value;

    const attestTransitions: IEmployeeGroupAttestTransitionDTO[] =
      this.form?.attestTransition.value;

    const dto = this.form.getRawValue();

    dto.dayTypeIds = dayTypeIds;
    dto.timeDeviationCauseRequestIds = timeDeviationCauseRequestIds;
    dto.timeDeviationCauseAbsenceAnnouncementIds =
      timeDeviationCauseAbsenceAnnouncementIds;
    dto.timeCodeIds = timeCodeIds;
    dto.ruleWorkTimePeriods = ruleWorkTimePeriods;

    dto.timeAccumulatorEmployeeGroupRules = timeAccumulatorEmployeeGroupRules;
    dto.timeDeviationCauses = timeDeviationCauses;
    dto.employeeGroupDayType = employeeGroupDayType;
    dto.employeeGroupTimeDeviationCauseTimeCode =
      employeeGroupTimeDeviationCauseTimeCode;
    dto.attestTransition = attestTransitions;

    this.performAction.crud(
      CrudActionTypeEnum.Save,
      this.service.save(dto).pipe(
        tap(value => {
          if (value.success) {
            this.updateFormValueAndEmitChange(value);
            if (value.success) this.triggerCloseDialog(value);
          }
        })
      ),
      undefined,
      undefined,
      options
    );
  }

  //LOAD DATA
  loadDayOfWeeks(): Observable<SmallGenericType[]> {
    return this.service.getDaysOfWeek(false);
  }

  loadTimeDeviationCauses(): Observable<SmallGenericType[]> {
    return this.service.getTimeDeviationCausesDict();
  }

  loadQualifyingDayCalculationRules(): Observable<SmallGenericType[]> {
    return this.service.getQualifyingDayCalculationRule(false, false);
  }

  loadTimeWorkReductionCalculationRule(): Observable<SmallGenericType[]> {
    return this.service.getTimeWorkReductionCalculationRule(false, false);
  }

  loadTimeCodes(): Observable<SmallGenericType[]> {
    return this.service.getTimeCodesDict();
  }

  loadAttestStates(): Observable<SmallGenericType[]> {
    return this.service
      .getAttestStates()
      .pipe(tap(x => (this.attestStates = x)));
  }

  loadReminderPeriodTypes(terms: TermCollection) {
    this.reminderPeriodTypes.push(
      new SmallGenericType(AttestPeriodType.Unknown, '')
    );
    this.reminderPeriodTypes.push(
      new SmallGenericType(
        AttestPeriodType.Day,
        terms['time.employee.employeegroup.theday']
      )
    );
    this.reminderPeriodTypes.push(
      new SmallGenericType(
        AttestPeriodType.Week,
        terms['time.employee.employeegroup.theweek']
      )
    );
    this.reminderPeriodTypes.push(
      new SmallGenericType(
        AttestPeriodType.Month,
        terms['time.employee.employeegroup.themonth']
      )
    );
    this.reminderPeriodTypes.push(
      new SmallGenericType(
        AttestPeriodType.Period,
        terms['time.employee.employeegroup.theperiod']
      )
    );
  }

  loadTimeDeviationCausesAbsence(): Observable<SmallGenericType[]> {
    return this.service.getTimeDeviationCausesAbsenceDict(true);
  }

  // Helper functions
  setHiddenFormLogic(value: number) {
    if (value === TermGroup_TimeReportType.Stamp) {
      this.form?.notifyChangeOfDeviations.patchValue(false);
      this.showNotifyChangeOfDeviations.set(false);
    } else {
      this.showNotifyChangeOfDeviations.set(true);
    }
    if (value === TermGroup_TimeReportType.ERP) {
      this.showAutogenTimeblocks.set(true);
    } else {
      this.showAutogenTimeblocks.set(false);
      this.form?.autoGenTimeAndBreakForProject.patchValue(false);
    }
  }

  //EVENTS
  timeReportTypeChanged(value: number) {
    this.setHiddenFormLogic(value);
  }

  breakSettingChanged(value: boolean) {
    this.form?.setBreakSettingsFormLogic();
  }

  timeboxFieldChanged(formField: string, value: TimeboxValue) {
    this.form?.timeboxFieldChanged(formField);
  }

  accountCostDimsChanged(dimsChanged: SelectedAccountsChangeSet): void {
    this.form?.markAsDirty();
    this.form?.defaultDim2CostAccountId.setValue(
      dimsChanged.selectedAccounts.account2?.accountId ?? 0
    );
    this.form?.defaultDim3CostAccountId.setValue(
      dimsChanged.selectedAccounts.account3?.accountId ?? 0
    );
    this.form?.defaultDim4CostAccountId.setValue(
      dimsChanged.selectedAccounts.account4?.accountId ?? 0
    );
    this.form?.defaultDim5CostAccountId.setValue(
      dimsChanged.selectedAccounts.account5?.accountId ?? 0
    );
    this.form?.defaultDim6CostAccountId.setValue(
      dimsChanged.selectedAccounts.account6?.accountId ?? 0
    );
  }

  accountIncomeDimsChanged(dimsChanged: SelectedAccountsChangeSet): void {
    this.form?.markAsDirty();
    this.form?.defaultDim2IncomeAccountId.setValue(
      dimsChanged.selectedAccounts.account2?.accountId ?? 0
    );
    this.form?.defaultDim3IncomeAccountId.setValue(
      dimsChanged.selectedAccounts.account3?.accountId ?? 0
    );
    this.form?.defaultDim4IncomeAccountId.setValue(
      dimsChanged.selectedAccounts.account4?.accountId ?? 0
    );
    this.form?.defaultDim5IncomeAccountId.setValue(
      dimsChanged.selectedAccounts.account5?.accountId ?? 0
    );
    this.form?.defaultDim6IncomeAccountId.setValue(
      dimsChanged.selectedAccounts.account6?.accountId ?? 0
    );
  }

  populateTimeDeviationCausesGridWhenEmpty(timeDeviationCauseId: number) {
    const timeDeviationCause: IEmployeeGroupTimeDeviationCauseDTO = {
      employeeGroupTimeDeviationCauseId: 0,
      employeeGroupId: this.form?.value.employeeGroupId,
      timeDeviationCauseId: timeDeviationCauseId,
      useInTimeTerminal: false,
    };

    const existingTimeDeviationCauses: IEmployeeGroupTimeDeviationCauseDTO[] =
      this.form?.value.timeDeviationCauses;

    // If no timedeviationcauses in grid, populate it with the standard deciation cause that was selected
    if (existingTimeDeviationCauses.length == 0) {
      this.form?.customTimeDeviationCausesPatchValue([timeDeviationCause]);
    }
  }
}
