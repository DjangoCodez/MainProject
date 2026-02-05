import { Component, inject } from '@angular/core';
import { DialogComponent } from '@ui/dialog/dialog/dialog.component';
import {
  PriceListTypeDialogData,
  PricelistTypeDTO,
} from '../../models/pricelist-type.model';
import { ValidationHandler } from '@shared/handlers';
import { PricelistTypeForm } from '../../models/pricelist-type-form.model';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { tap } from 'rxjs';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import { PricelistTypeDialogService } from '../../services/pricelist-type-dialog.service';

@Component({
  selector: 'soe-pricelist-type-dialog',
  templateUrl: './pricelist-type-dialog.component.html',
  providers: [FlowHandlerService],
  standalone: false,
})
export class PricelistTypeDialogComponent extends DialogComponent<PriceListTypeDialogData> {
  validationHandler = inject(ValidationHandler);
  handler = inject(FlowHandlerService);
  coreService = inject(CoreService);
  service = inject(PricelistTypeDialogService);
  currencies: ISmallGenericType[] = [];

  form: PricelistTypeForm;

  constructor() {
    super();

    const pricelist = new PricelistTypeDTO();
    pricelist.isProjectPriceList = true;

    this.form = new PricelistTypeForm({
      validationHandler: this.validationHandler,
      element: pricelist,
    });

    this.handler.execute({
      lookups: [this.loadCurrencies()],
      onFinished: () => this.loadData(),
    });
  }

  closeDialog(): void {
    this.dialogRef.close();
  }

  protected ok(): void {
    this.dialogRef.close(this.form.value);
  }

  private loadCurrencies() {
    return this.coreService.getCompCurrenciesDict(true).pipe(
      tap(x => {
        this.currencies = x;
        if (this.currencies.length > 1) {
          this.form.patchValue({ currencyId: this.currencies[1].id });
        }
      })
    );
  }

  private loadData(): void {
    if (this.data.priceListTypeId > 0) {
      this.service.getPriceListType(this.data.priceListTypeId).subscribe(x => {
        this.form.reset(x);
      });
    }
  }
}
