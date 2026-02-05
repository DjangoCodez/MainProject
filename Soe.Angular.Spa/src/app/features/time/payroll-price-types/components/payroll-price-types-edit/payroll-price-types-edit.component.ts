import { Component, inject, OnInit } from '@angular/core';
import { EditBaseDirective } from '@shared/directives/edit-base/edit-base.directive';
import {
  IPayrollPriceTypeDTO,
  IPayrollPriceTypePeriodDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { PayrollPriceTypesService } from '../../services/payroll-price-types.service';
import { PayrollPriceTypesForm } from '../../models/payroll-price-types-form.model';
import {
  Feature,
  TermGroup,
} from '@shared/models/generated-interfaces/Enumerations';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import { Observable, of, tap } from 'rxjs';

@Component({
  selector: 'soe-payroll-price-types-edit',
  standalone: false,
  templateUrl: './payroll-price-types-edit.component.html',
  providers: [FlowHandlerService, ToolbarService],
  styles: ['.periodsgrid-container { max-width: 30rem; }'],
})
export class PayrollPriceTypesEditComponent
  extends EditBaseDirective<
    IPayrollPriceTypeDTO,
    PayrollPriceTypesService,
    PayrollPriceTypesForm
  >
  implements OnInit
{
  service = inject(PayrollPriceTypesService);
  coreService = inject(CoreService);

  payrollPriceTypeTypes: ISmallGenericType[] = [];

  payrollPriceTypePeriods: IPayrollPriceTypePeriodDTO[] = [];

  ngOnInit() {
    super.ngOnInit();
    this.startFlow(Feature.Time_Preferences_SalarySettings_PriceType_Edit, {
      lookups: [this.loadPayrollPriceTypeTypes()],
    });
  }

  loadPayrollPriceTypeTypes(): Observable<void> {
    return this.performLoadData.load$(
      this.coreService
        .getTermGroupContent(TermGroup.PayrollPriceTypes, false, false, true)
        .pipe(tap(value => (this.payrollPriceTypeTypes = value)))
    );
  }

  override loadData(): Observable<void> {
    return this.performLoadData.load$(
      this.service.get(this.form?.getIdControl()?.value).pipe(
        tap((value: IPayrollPriceTypeDTO) => {
          this.form?.customPatchValue(value);
        })
      )
    );
  }

  override newRecord(): Observable<void> {
    let clearValues = () => {};

    if (this.form?.isCopy) {
      clearValues = () => {
        this.form?.onDoCopy();
      };
    }

    setTimeout(() => {
      this.form?.patchValue({});
    });

    return of(clearValues());
  }

  onCodeInput(event: Event) {
    const inputElement = event.target as HTMLInputElement;
    inputElement.value = inputElement.value.toUpperCase();
    this.form?.controls.code.patchValue(inputElement.value);
  }
}
