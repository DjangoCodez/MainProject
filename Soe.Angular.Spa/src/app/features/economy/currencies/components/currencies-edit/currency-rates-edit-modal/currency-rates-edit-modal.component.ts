import { Component, inject } from '@angular/core';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { DialogComponent } from '@ui/dialog/dialog/dialog.component';
import {
  CurrencyRatesEditDialogData,
  CurrencyRatesForm,
} from './currency-rates-edit-modal.model';
import { ValidationHandler } from '@shared/handlers';
import { TermGroup_CurrencySource } from '@shared/models/generated-interfaces/Enumerations';
import { CurrencyRateDTO } from '@features/economy/currencies/models/currencies.model';

@Component({
  templateUrl: './currency-rates-edit-modal.component.html',
  providers: [FlowHandlerService],
  standalone: false,
})
export class CurrencyRatesEditModal extends DialogComponent<CurrencyRatesEditDialogData> {
  validationHandler = inject(ValidationHandler);

  form: CurrencyRatesForm;

  get rateFromBase(): string {
    return `${this.data.otherCurrencyCode} / ${this.data.baseCurrencyCode}`;
  }

  get rateToBase(): string {
    return `${this.data.baseCurrencyCode} / ${this.data.otherCurrencyCode}`;
  }

  get permissionToSave(): boolean {
    return this.form.sourceType === TermGroup_CurrencySource.Manually;
  }

  constructor() {
    super();
    this.form = new CurrencyRatesForm({
      validationHandler: this.validationHandler,
      element: this.data.rate,
      rows: this.data.rows.filter(r => r !== this.data.rate),
    });
  }

  triggerOk() {
    const row = this.form.getRawValue() as CurrencyRateDTO;
    row.sourceName =
      this.data.sources.find(s => s.id === row.source)?.name || '';
    row.isModified = this.form.dirty;
    this.dialogRef.close(row);
  }

  rateFromBaseChanged(value: number) {
    const rounded = this.round(value);
    this.form.controls.rateFromBase.setValue(rounded);
    this.form.controls.rateToBase.setValue(this.round(1 / value));
  }

  rateToBaseChanged(value: number) {
    const rounded = this.round(value);
    this.form.controls.rateToBase.setValue(rounded);
    this.form.controls.rateFromBase.setValue(this.round(1 / rounded));
  }

  round(value: number) {
    return Math.round(value * 10000) / 10000;
  }
}
