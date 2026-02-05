import { Component, inject, OnInit, signal } from '@angular/core';
import { SoeFormGroup } from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { ITimeCodePayrollProductDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { IProductTimeCodeDTO } from '@shared/models/generated-interfaces/ProductDTOs';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { DialogComponent } from '@ui/dialog/dialog/dialog.component';
import { DialogData } from '@ui/dialog/models/dialog';
import { TimeCodePayrollProductForm } from '../models/time-code-payroll-product-form.model';

export interface ITimeCodePayrollProductsDialogData extends DialogData {
  dto: ITimeCodePayrollProductDTO;
  payrollProducts: IProductTimeCodeDTO[];
  factorDecimals: number;
}

@Component({
  templateUrl: './time-code-payroll-products-dialog.component.html',
  providers: [FlowHandlerService],
  standalone: false,
})
export class TimeCodePayrollProductsDialogComponent
  extends DialogComponent<ITimeCodePayrollProductsDialogData>
  implements OnInit
{
  protected initialFocusOnFactor = signal(true);
  protected form!: TimeCodePayrollProductForm;

  private validationHandler = inject(ValidationHandler);

  ngOnInit(): void {
    const dto: ITimeCodePayrollProductDTO =
      this.data.dto ??
      ({
        timeCodePayrollProductId: 0,
        timeCodeId: 0,
        payrollProductId: 0,
        factor: 1,
      } as ITimeCodePayrollProductDTO);

    this.form = new TimeCodePayrollProductForm({
      validationHandler: this.validationHandler,
      element: dto,
    });

    if (!dto.payrollProductId) {
      this.initialFocusOnFactor.set(false);
    }
  }

  ok(): void {
    const dialogResult = {
      success: true,
      payrollProductId: this.form.value.payrollProductId,
      factor: this.roundFactor(this.form.value.factor),
    };
    this.dialogRef.close(dialogResult);
  }

  cancel(): void {
    this.dialogRef.close({
      success: false,
    });
  }

  openFormValidationErrors(): void {
    this.validationHandler.showFormValidationErrors(this.form as SoeFormGroup);
  }

  private roundFactor(factor: number): number {
    const precision = Math.pow(10, this.data.factorDecimals);
    return Math.round(factor * precision) / precision;
  }
}
