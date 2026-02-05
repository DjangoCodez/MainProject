import { Component, inject, OnInit, signal } from '@angular/core';
import { SoeFormGroup } from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { ITimeCodeInvoiceProductDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { IProductTimeCodeDTO } from '@shared/models/generated-interfaces/ProductDTOs';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { DialogComponent } from '@ui/dialog/dialog/dialog.component';
import { DialogData } from '@ui/dialog/models/dialog';
import { TimeCodeInvoiceProductForm } from '../models/time-code-invoice-product-form.model';

export interface ITimeCodeInvoiceProductsDialogData extends DialogData {
  dto: ITimeCodeInvoiceProductDTO;
  invoiceProducts: IProductTimeCodeDTO[];
  factorDecimals: number;
}

@Component({
  templateUrl: './time-code-invoice-products-dialog.component.html',
  providers: [FlowHandlerService],
  standalone: false,
})
export class TimeCodeInvoiceProductsDialogComponent
  extends DialogComponent<ITimeCodeInvoiceProductsDialogData>
  implements OnInit
{
  protected initialFocusOnFactor = signal(true);
  protected form!: TimeCodeInvoiceProductForm;

  private validationHandler = inject(ValidationHandler);

  ngOnInit(): void {
    const dto: ITimeCodeInvoiceProductDTO =
      this.data.dto ??
      ({
        timeCodeInvoiceProductId: 0,
        timeCodeId: 0,
        invoiceProductId: 0,
        factor: 1,
      } as ITimeCodeInvoiceProductDTO);

    this.form = new TimeCodeInvoiceProductForm({
      validationHandler: this.validationHandler,
      element: dto,
    });

    if (!dto.invoiceProductId) {
      this.initialFocusOnFactor.set(false);
    }
  }

  ok(): void {
    const dialogResult = {
      success: true,
      invoiceProductId: this.form.value.invoiceProductId,
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
