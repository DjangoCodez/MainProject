import { Component, OnInit, inject, signal } from '@angular/core';
import { Observable, of, tap } from 'rxjs';
import { map, mergeMap, take } from 'rxjs/operators';
import { EditBaseDirective } from '@shared/directives/edit-base/edit-base.directive';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ToolbarService } from '@shared/ui-components/toolbar/services/toolbar.service';
import {
  IScheduleCycleDTO,
  IScheduleCycleRuleDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { ScheduleCyclesService } from '../../services/schedule-cycles.service';
import { ScheduleCyclesForm } from '../../models/schedule-cycles-form.model';
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
import { CrudActionTypeEnum } from '@shared/enums/action.enum';

@Component({
  selector: 'soe-schedule-cycles-edit',
  standalone: false,
  templateUrl: './schedule-cycles-edit.component.html',
  providers: [FlowHandlerService, ToolbarService],
})
export class ScheduleCyclesEditComponent
  extends EditBaseDirective<
    IScheduleCycleDTO,
    ScheduleCyclesService,
    ScheduleCyclesForm
  >
  implements OnInit
{
  readonly service = inject(ScheduleCyclesService);
  private coreService = inject(CoreService);
  private sharedService = inject(SharedService);

  useAccountsHierarchy = signal(false);

  accounts: SmallGenericType[] = [];

  initialized = signal(false);

  ngOnInit(): void {
    super.ngOnInit();

    this.startFlow(Feature.Time_Schedule_StaffingNeeds_ScheduleCycle, {
      lookups: [this.loadCompanySettings()],
    });
  }

  override onFinished(): void {
    super.onFinished();
    this.initialized.set(true);
  }

  override loadData(): Observable<void> {
    return this.performLoadData.load$(
      this.service.get(this.form?.getIdControl()?.value).pipe(
        tap((value: IScheduleCycleDTO) => {
          this.form?.customPatchValue(value);
        })
      )
    );
  }

  override newRecord(): Observable<void> {
    if (this.form?.isCopy) {
      this.onFinished();
    }
    return of();
  }

  override performSave(): void {
    if (!this.form || !this.service) return;

    const dto = this.form.getRawValue();

    const scheduleCycleRuleDTOs: IScheduleCycleRuleDTO[] =
      this.form.scheduleCycleRuleDTOs.value;
    dto.scheduleCycleRuleDTOs = scheduleCycleRuleDTOs;

    this.performAction.crud(
      CrudActionTypeEnum.Save,
      this.service.save(dto).pipe(
        tap(value => {
          if (value.success) {
            this.updateFormValueAndEmitChange(value);
            this.triggerCloseDialog(value);
          }
        })
      )
    );
  }

  override copy(): void {
    if (!this.form) return;

    this.form.controls.scheduleCycleId.setValue(0);
    this.form.controls.name.setValue('');
    this.form.controls.accountId.setValue(null);

    const rules = this.form.scheduleCycleRuleDTOs;
    for (let i = 0; i < rules.length; i++) {
      rules.at(i).patchValue({ scheduleCycleRuleId: 0 });
    }

    this.form.markAsDirty();
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

          const scheduleCycleId =
            this.form?.controls.scheduleCycleId?.value || 0;
          if (scheduleCycleId > 0) {
            this.accounts.unshift(new SmallGenericType(0, ''));
          }
        }),
        map(() => undefined)
      );
  }

  onNbrOfWeeksChanged(): void {
    this.form?.markAsDirty();
  }
}
