import { Component, inject, OnInit, signal } from '@angular/core';
import { EditBaseDirective } from '@shared/directives/edit-base/edit-base.directive';
import {
  CompanySettingType,
  Feature,
} from '@shared/models/generated-interfaces/Enumerations';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { SharedService } from '@shared/services/shared.service';
import { IncomingDeliveryTypesService } from '../../services/incoming-delivery-types.service';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { IIncomingDeliveryTypeDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { Observable, tap, of } from 'rxjs';
import { DateUtil } from '@shared/util/date-util';
import { SettingsUtil } from '@shared/util/settings-util';
import { AccountDTO } from '@shared/models/account.model';
import { SmallGenericType } from '@shared/models/generic-type.model';
import {
  createLengthValidator,
  IncomingDeliveryTypeForm,
} from '../../models/incoming-delivery-types-form.model';

@Component({
  selector: 'soe-incoming-delivery-types-edit',
  templateUrl: './incoming-delivery-types-edit.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class IncomingDeliveryTypesEditComponent
  extends EditBaseDirective<
    IIncomingDeliveryTypeDTO,
    IncomingDeliveryTypesService,
    IncomingDeliveryTypeForm
  >
  implements OnInit
{
  service = inject(IncomingDeliveryTypesService);
  private readonly coreService = inject(CoreService);
  private readonly sharedService = inject(SharedService);

  companySettingMinLength = signal(0);
  minLengthInfoLabel = signal('');

  useAccountsHierarchy = signal(false);
  accounts: SmallGenericType[] = [];

  ngOnInit() {
    super.ngOnInit();
    this.startFlow(
      Feature.Time_Preferences_ScheduleSettings_IncomingDeliveryType
    );
  }

  override loadCompanySettings(): Observable<any> {
    return this.coreService
      .getCompanySettings([
        CompanySettingType.TimeSchedulePlanningDayViewMinorTickLength,
        CompanySettingType.UseAccountHierarchy,
      ])
      .pipe(
        tap(x => {
          this.companySettingMinLength.set(
            SettingsUtil.getIntCompanySetting(
              x,
              CompanySettingType.TimeSchedulePlanningDayViewMinorTickLength
            )
          );
          const term = this.translate.instant('common.min');
          this.minLengthInfoLabel.set(
            '{0} {1}'.format(term, this.companySettingMinLength().toString())
          );

          this.useAccountsHierarchy.set(
            SettingsUtil.getBoolCompanySetting(
              x,
              CompanySettingType.UseAccountHierarchy
            )
          );
          if (this.useAccountsHierarchy()) {
            this.loadAccounts().subscribe(); // Instead of in lookup for timing reasons
          }

          this.addFormValidators();
        })
      );
  }

  private loadAccounts(): Observable<AccountDTO[] | undefined> {
    if (!this.useAccountsHierarchy()) return of(undefined);

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
          this.accounts.unshift(new SmallGenericType(0, ''));
        })
      );
  }

  private addFormValidators() {
    const minLengthString = this.companySettingMinLength().toString();

    this.form?.addValidators([
      createLengthValidator(
        `${this.translate.instant('time.schedule.incomingdeliverytype.validation.lengthminutesislowerthanallowed')} (${minLengthString})`,
        this.companySettingMinLength()
      ),
    ]);
    this.form?.updateValueAndValidity();
  }
}
