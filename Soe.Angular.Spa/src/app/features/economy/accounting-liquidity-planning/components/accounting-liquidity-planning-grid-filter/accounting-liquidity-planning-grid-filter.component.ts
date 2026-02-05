import {
  Component,
  EventEmitter,
  inject,
  OnInit,
  Output,
  signal,
} from '@angular/core';
import { GetLiquidityPlanningModel } from '../../models/liquidity-planning.model';
import { ValidationHandler } from '@shared/handlers';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { LiquidityPlanningFilterForm } from '../../models/liquidity-planning-filter-form.model';
import { CoreService } from '@shared/services/core.service';
import {
  SettingMainType,
  UserSettingType,
} from '@shared/models/generated-interfaces/Enumerations';
import { SaveUserCompanySettingModel } from '@shared/components/select-project-dialog/models/select-project-dialog.model';
import { TranslateService } from '@ngx-translate/core';
import { forkJoin, Observable, take } from 'rxjs';
import { SettingsUtil, UserCompanySettingCollection } from '@shared/util/settings-util';
import { TermCollection } from '@shared/localization/term-types';

@Component({
  selector: 'soe-accounting-liquidity-planning-grid-filter',
  templateUrl: './accounting-liquidity-planning-grid-filter.component.html',
  standalone: false,
})
export class AccountingLiquidityPlanningGridFilterComponent implements OnInit {
  paymentStatuses = signal<SmallGenericType[]>([]);
  userSettingSelection: UserCompanySettingCollection = [];

  @Output() searchClick = new EventEmitter<GetLiquidityPlanningModel>();

  coreService = inject(CoreService);
  translate = inject(TranslateService);
  validationHandler = inject(ValidationHandler);
  formFilter: LiquidityPlanningFilterForm = new LiquidityPlanningFilterForm({
    validationHandler: this.validationHandler,
    element: new GetLiquidityPlanningModel(),
  });

  ngOnInit(): void {
    const today = new Date();
    this.formFilter.from.setValue(
      new Date(today.getFullYear(), today.getMonth(), 1)
    );
    this.formFilter.to.setValue(
      new Date(today.getFullYear(), today.getMonth() + 1, 0)
    );

    forkJoin({
      paymentStatusTypes: this.getPaymentStatusTypes(),
      userSettings: this.loadUserSettings(),
    }).subscribe(({ paymentStatusTypes, userSettings }) => {
      // Set payment Types
      this.paymentStatuses.set([
        new SmallGenericType(
          +UserSettingType.LiquidityPlanningPreSelectUnpaid,
          paymentStatusTypes['economy.supplier.suppliercentral.unpaiedinvoices']
        ),
        new SmallGenericType(
          +UserSettingType.LiquidityPlanningPreSelectPaidUnchecked,
          paymentStatusTypes[
            'economy.accounting.liquidityplanning.paidunchecked'
          ]
        ),
      ]);

      // Set user settings
      this.userSettingSelection = userSettings;
      this.formFilter.selectedPaymentStatuses.setValue([
        ...(this.paymentStatuses()
          .filter(s =>
            SettingsUtil.getBoolUserSetting(
              this.userSettingSelection,
              s.id,
              false
            )
          )
          .map(s => s.id) ?? []),
      ]);
    });
  }

  getPaymentStatusTypes(): Observable<TermCollection> {
    return this.translate
      .get([
        'economy.supplier.suppliercentral.unpaiedinvoices',
        'economy.accounting.liquidityplanning.paidunchecked',
      ])
      .pipe(take(1));
  }

  private loadUserSettings(): Observable<UserCompanySettingCollection> {
    const settingTypes: UserSettingType[] = [
      UserSettingType.LiquidityPlanningPreSelectUnpaid,
      UserSettingType.LiquidityPlanningPreSelectPaidUnchecked,
    ];

    return this.coreService.getUserSettings(settingTypes);
  }

  saveSelection() {
    const selectedPaymentStatuses =
      this.formFilter.selectedPaymentStatuses.getRawValue() as number[];
    this.paymentStatuses().forEach(s => {
      //Update only if changed
      if (
        this.userSettingSelection[s.id] !==
        selectedPaymentStatuses.some(x => x === s.id)
      ) {
        this.userSettingSelection[s.id] = selectedPaymentStatuses.some(
          x => x === s.id
        );

        const settingModel = new SaveUserCompanySettingModel(
          SettingMainType.User,
          s.id,
          selectedPaymentStatuses.some(x => x === s.id)
        );
        this.coreService.saveBoolSetting(settingModel).subscribe();
      }
    });
  }

  search() {
    this.searchClick.emit(this.getFilter());
  }

  getFilter(): GetLiquidityPlanningModel {
    const selectedPaymentStatuses =
      this.formFilter.selectedPaymentStatuses.getRawValue() as number[];
    const filter: GetLiquidityPlanningModel = {
      from: this.formFilter.from.getRawValue(),
      to: this.formFilter.to.getRawValue(),
      exclusion: this.formFilter.exclusion.getRawValue() ?? undefined,
      balance: this.formFilter.balance.getRawValue(),
      selectedPaymentStatuses:
        this.formFilter.selectedPaymentStatuses.getRawValue(),
      unpaid: selectedPaymentStatuses.some(
        x => x === UserSettingType.LiquidityPlanningPreSelectUnpaid
      ),
      paidUnchecked: selectedPaymentStatuses.some(
        x => x === UserSettingType.LiquidityPlanningPreSelectPaidUnchecked
      ),
      paidChecked: selectedPaymentStatuses.some(
        x => x === UserSettingType.LiquidityPlanningPreSelectPaidChecked
      ),
    };

    return filter;
  }
}
