import { Component, OnInit, inject, signal } from '@angular/core';
import { map, mergeMap, take, tap } from 'rxjs/operators';
import { Observable, of } from 'rxjs';
import { EditBaseDirective } from '@shared/directives/edit-base/edit-base.directive';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ToolbarService } from '@shared/ui-components/toolbar/services/toolbar.service';
import { IScheduleCycleRuleTypeDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { ScheduleCycleRuleTypesService } from '../../services/schedule-cycle-rule-types.service';
import { ScheduleCycleRuleTypeForm } from '../../models/schedule-cycle-rule-type-form.model';
import {
  Feature,
  CompanySettingType,
} from '@shared/models/generated-interfaces/Enumerations';
import { CoreService } from '@shared/services/core.service';
import { SharedService } from '@shared/services/shared.service';
import { DateUtil } from '@shared/util/date-util';
import {
  SettingsUtil,
  UserCompanySettingCollection,
} from '@shared/util/settings-util';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { ProgressOptions } from '@shared/services/progress/progress-options.class';
import { CrudActionTypeEnum } from '@shared/enums/action.enum';

@Component({
  selector: 'soe-schedule-cycle-rule-types-edit',
  templateUrl: './schedule-cycle-rule-types-edit.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class ScheduleCycleRuleTypesEditComponent
  extends EditBaseDirective<
    IScheduleCycleRuleTypeDTO,
    ScheduleCycleRuleTypesService,
    ScheduleCycleRuleTypeForm
  >
  implements OnInit
{
  service = inject(ScheduleCycleRuleTypesService);
  coreService = inject(CoreService);
  sharedService = inject(SharedService);

  useAccountsHierarchy = signal(false);
  accounts: SmallGenericType[] = [];
  dayOfWeeks: SmallGenericType[] = [];

  ngOnInit(): void {
    super.ngOnInit();

    this.setupDayOfWeeks();

    this.startFlow(Feature.Time_Schedule_StaffingNeeds_ScheduleCycleRuleType);
  }

  override loadData(): Observable<any> {
    return this.performLoadData.load$(
      this.service
        .get(this.form?.controls.scheduleCycleRuleTypeId?.value || 0)
        .pipe(
          tap(value => {
            this.form?.patchValue(value);
          })
        ),
      { showDialogDelay: 1000 }
    );
  }

  override performSave(
    options?: ProgressOptions,
    skipLoadData?: boolean
  ): void {
    if (!this.form) return;

    if (this.form.controls.dayOfWeekIds?.value) {
      const sortedDays = [...this.form.controls.dayOfWeekIds.value].sort(
        (a, b) => a - b
      );
      this.form.controls.dayOfWeekIds.setValue(sortedDays);
    }

    const formValue = this.form.getRawValue();
    const timeRange = this.form.controls.timeRange.value as [
      Date | number,
      Date | number,
    ];

    if (timeRange) {
      (formValue as any).startTime = this.parseTimeToDate(timeRange[0]);
      (formValue as any).stopTime = this.parseTimeToDate(timeRange[1]);
    }

    this.performAction.crud(
      CrudActionTypeEnum.Save,
      this.service.save(formValue as any).pipe(
        tap(res => {
          this.updateFormValueAndEmitChange(res, skipLoadData);
          if (res.success) this.triggerCloseDialog(res);
        })
      ),
      options?.callback,
      options?.errorCallback,
      options
    );
  }

  override copy(): void {
    if (!this.form) return;

    this.form.controls.scheduleCycleRuleTypeId.setValue(0);
    this.form.controls.name.setValue('');
    this.form.controls.accountId.setValue(undefined);
    this.form.markAsDirty();
  }

  private setupDayOfWeeks(): void {
    this.dayOfWeeks = DateUtil.getDayOfWeekNames(true).map(day => ({
      id: day.id,
      name: day.name.charAt(0).toUpperCase() + day.name.slice(1).toLowerCase(),
    }));
  }

  override loadCompanySettings(): Observable<void> {
    return this.performLoadData.load$(
      this.coreService
        .getCompanySettings([CompanySettingType.UseAccountHierarchy])
        .pipe(
          tap((settings: UserCompanySettingCollection) => {
            this.useAccountsHierarchy.set(
              SettingsUtil.getBoolCompanySetting(
                settings,
                CompanySettingType.UseAccountHierarchy
              )
            );
          }),
          mergeMap(() => this.loadAccounts())
        )
    );
  }

  private loadAccounts(): Observable<void> {
    if (!this.useAccountsHierarchy()) {
      return of(undefined);
    }

    const today = DateUtil.getToday();
    this.accounts = [];
    return this.sharedService
      .getAccountsFromHierarchyByUserSetting(
        today,
        today,
        true,
        false,
        false,
        true
      )
      .pipe(
        tap(accounts => {
          this.accounts = accounts.map(a => ({
            id: a.accountId,
            name: a.name,
          }));

          const scheduleCycleRuleTypeId =
            this.form?.controls.scheduleCycleRuleTypeId?.value || 0;
          if (scheduleCycleRuleTypeId > 0) {
            this.accounts.unshift(new SmallGenericType(0, ''));
          }
        }),
        map(() => undefined)
      );
  }

  onTimeRangeChanged(): void {
    this.form?.markAsDirty();
  }

  private parseTimeToDate(value: Date | number): Date {
    if (value instanceof Date) return value;
    return DateUtil.defaultDateTime();
  }
}
