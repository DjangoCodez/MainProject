import { Component, inject, OnInit } from '@angular/core';
import { EditBaseDirective } from '@shared/directives/edit-base/edit-base.directive';
import { ITimeCodeSaveDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { TimeCodeAdditionDeductionService } from '../../services/time-code-addition-deduction.service';
import { TimeCodeAdditionDeductionForm } from '../../models/time-code-addition-deduction-form.model';
import {
  CompanySettingType,
  Feature,
  TermGroup,
  TermGroup_ExpenseType,
  TermGroup_TimeCodeRegistrationType,
} from '@shared/models/generated-interfaces/Enumerations';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import { Observable, tap } from 'rxjs';
import { CoreService } from '@shared/services/core.service';
import { SettingsUtil } from '@shared/util/settings-util';

@Component({
  standalone: false,
  templateUrl: './time-code-addition-deduction-edit.component.html',
  providers: [FlowHandlerService, ToolbarService],
})
export class TimeCodeAdditionDeductionEditComponent
  extends EditBaseDirective<
    ITimeCodeSaveDTO,
    TimeCodeAdditionDeductionService,
    TimeCodeAdditionDeductionForm
  >
  implements OnInit
{
  service = inject(TimeCodeAdditionDeductionService);
  coreService = inject(CoreService);

  readonly Feature = Feature;

  expenseTypes: ISmallGenericType[] = [];
  registrationTypes: ISmallGenericType[] = [];
  possibilityToRegisterAdditionsInTerminal: boolean = false;

  readonly stopAtDateStopExpenseTypes: TermGroup_ExpenseType[] = [
    TermGroup_ExpenseType.AllowanceDomestic,
    TermGroup_ExpenseType.AllowanceAbroad,
  ];
  readonly stopAtPriceExpenseTypes: TermGroup_ExpenseType[] = [
    TermGroup_ExpenseType.AllowanceAbroad,
    TermGroup_ExpenseType.Expense,
    TermGroup_ExpenseType.TravellingTime,
    TermGroup_ExpenseType.Time,
  ];
  readonly stopAtVatExpenseTypes: TermGroup_ExpenseType[] = [
    TermGroup_ExpenseType.Expense,
    TermGroup_ExpenseType.TravellingTime,
    TermGroup_ExpenseType.Time,
  ];

  ngOnInit() {
    super.ngOnInit();
    this.startFlow(
      Feature.Time_Preferences_TimeSettings_TimeCodeAdditionDeduction_Edit,
      {
        lookups: [this.loadExpenseTypes(), this.loadRegistrationTypes()],
      }
    );
    if (this.form?.isCopy) {
      this.clearNameAndRelationIds();
      this.form.markAsPristine();
      this.form.markAsUntouched();
      let patchDelaySeconds = 1;
      do {
        setTimeout(() => {
          this.form?.patchValue({});
        }, patchDelaySeconds * 1000);
        patchDelaySeconds++;
      } while (patchDelaySeconds <= 4);
    }
  }

  private clearNameAndRelationIds() {
    if (!this.form) {
      return;
    }
    this.form.patchValue({ timeCodeId: 0, name: '' });
    this.form.payrollProducts.controls.forEach(payrollProduct => {
      payrollProduct.patchValue({
        timeCodePayrollProductId: 0,
        timeCodeId: 0,
      });
    });
    this.form.invoiceProducts.controls.forEach(invoiceProduct => {
      invoiceProduct.patchValue({
        timeCodeInvoiceProductId: 0,
        timeCodeId: 0,
      });
    });
  }

  override loadCompanySettings() {
    const settingTypes: CompanySettingType[] = [
      CompanySettingType.PossibilityToRegisterAdditionsInTerminal,
    ];
    return this.coreService.getCompanySettings(settingTypes).pipe(
      tap(settings => {
        this.possibilityToRegisterAdditionsInTerminal =
          SettingsUtil.getBoolCompanySetting(
            settings,
            CompanySettingType.PossibilityToRegisterAdditionsInTerminal
          );
      })
    );
  }

  override loadData(): Observable<void> {
    return this.performLoadData.load$(
      this.service.get(this.form?.getIdControl()?.value).pipe(
        tap((value: ITimeCodeSaveDTO) => {
          this.form?.customPatchValue(value);
          this.form?.markAsPristine();
          this.form?.markAsUntouched();
        })
      )
    );
  }

  private loadExpenseTypes(): Observable<void> {
    return this.performLoadData.load$(
      this.coreService
        .getTermGroupContent(TermGroup.ExpenseType, false, true)
        .pipe(tap(value => (this.expenseTypes = value)))
    );
  }

  private loadRegistrationTypes(): Observable<void> {
    return this.performLoadData.load$(
      this.coreService
        .getTermGroupContent(TermGroup.TimeCodeRegistrationType, false, true)
        .pipe(tap(value => (this.registrationTypes = value)))
    );
  }

  shouldShowFixedQuantityInput(): boolean {
    return (
      this.form?.value.registrationType ===
      TermGroup_TimeCodeRegistrationType.Quantity
    );
  }
}
