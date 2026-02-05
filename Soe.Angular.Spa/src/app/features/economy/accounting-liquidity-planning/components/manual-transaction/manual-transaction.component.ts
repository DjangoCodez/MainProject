import { Component, inject, OnDestroy, OnInit } from '@angular/core';
import { DialogComponent } from '@ui/dialog/dialog/dialog.component';
import {
  LiquidityPlanningDialogData,
  LiquidityPlanningDTO,
} from '../../models/liquidity-planning.model';
import { ManualTransactionDialogForm } from './manual-transaction-dialog.model';
import { ValidationHandler } from '@shared/handlers';
import { Subject, takeUntil, tap } from 'rxjs';

@Component({
  selector: 'soe-manual-transaction',
  templateUrl: './manual-transaction.component.html',
  standalone: false,
})
export class ManualTransactionComponent
  extends DialogComponent<LiquidityPlanningDialogData>
  implements OnInit, OnDestroy
{
  unsubscribed = new Subject<void>();

  validationHandler = inject(ValidationHandler);
  showRemove = false;

  form: ManualTransactionDialogForm = new ManualTransactionDialogForm({
    validationHandler: this.validationHandler,
    element: new LiquidityPlanningDTO(null),
  });
  ngOnInit(): void {
    this.form.total.valueChanges
      .pipe(
        takeUntil(this.unsubscribed),
        tap(total => {
          if (total > 0) {
            this.form.valueIn.setValue(total);
            this.form.valueOut.setValue(0);
          } else {
            this.form.valueOut.setValue(total);
            this.form.valueIn.setValue(0);
          }
        })
      )
      .subscribe();
    this.form.liquidityPlanningTransactionId.setValue(
      this.data.liquidityPlanningTransactionId ?? null
    );
    this.form.specification.setValue(this.data.specification ?? '');
    this.form.date.setValue(this.data.date ?? new Date());
    this.form.total.setValue(this.data.total ?? 0);
    if (this.data.liquidityPlanningTransactionId) {
      this.showRemove = true;
    }
  }

  close() {
    this.dialogRef.close();
  }

  remove() {
    const transaction = this.getManualTransaction();
    this.dialogRef.close({ item: transaction, delete: true });
  }

  save() {
    const transaction = this.getManualTransaction();
    this.dialogRef.close({ item: transaction });
  }

  ngOnDestroy(): void {
    this.unsubscribed.next();
    this.unsubscribed.complete();
  }

  private getManualTransaction(): LiquidityPlanningDTO {
    const transaction = new LiquidityPlanningDTO(this.data);
    transaction.specification = this.form.specification.getRawValue() ?? '';
    transaction.date = this.form.date.getRawValue() ?? new Date();
    transaction.total = this.form.total.getRawValue() ?? 0;
    transaction.valueIn = this.form.valueIn.getRawValue() ?? 0;
    transaction.valueOut = this.form.valueOut.getRawValue() ?? 0;
    return transaction;
  }
}
