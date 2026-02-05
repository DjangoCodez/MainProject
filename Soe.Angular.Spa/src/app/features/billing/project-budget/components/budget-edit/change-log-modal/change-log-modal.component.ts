import { Component, inject, signal, ViewChild } from '@angular/core';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { DialogComponent } from '@ui/dialog/dialog/dialog.component';
import { ChangeLogDialogData, ChangeLogForm } from './change-log-modal.model';
import { ValidationHandler } from '@shared/handlers';
import { ChangeLogModalGridComponent } from './change-log-modal-grid/change-log-modal-grid';
import { CommonModule } from '@angular/common';
import { SharedModule } from '@shared/shared.module';
import { ReactiveFormsModule } from '@angular/forms';
import { ButtonComponent } from '@ui/button/button/button.component';
import { TextboxComponent } from '@ui/forms/textbox/textbox.component';
import { BudgetRowProjectChangeLogDTO } from '@features/billing/project-budget/models/project-budget.model';
import { BehaviorSubject } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

@Component({
  templateUrl: './change-log-modal.component.html',
  providers: [FlowHandlerService],
  standalone: true,
  imports: [
    ChangeLogModalGridComponent,
    DialogComponent,
    CommonModule,
    SharedModule,
    ReactiveFormsModule,
    ButtonComponent,
    TextboxComponent,
  ],
})
export class ChangeLogModal extends DialogComponent<ChangeLogDialogData> {
  validationHandler = inject(ValidationHandler);
  form: ChangeLogForm;

  items = new BehaviorSubject<BudgetRowProjectChangeLogDTO[]>([]);

  // Grid Component Reference
  @ViewChild(ChangeLogModalGridComponent)
  changeLogGrid!: ChangeLogModalGridComponent;

  // Signal
  protected okButtonDisabled = signal(true);
  protected isReadOnly = signal(false);

  constructor() {
    super();

    this.form = new ChangeLogForm({
      validationHandler: this.validationHandler,
      generalComment: '',
    });

    this.form?.generalComment.valueChanges
      .pipe(takeUntilDestroyed())
      .subscribe((text?: string) => {
        this.valueChanged();
      });

    this.isReadOnly.set(this.data.isReadOnly);
    this.items.next(this.data?.items || []);
    this.valueChanged();
  }

  triggerOk() {
    if (this.data.isReadOnly) {
      this.dialogRef.close({});
    } else {
      this.changeLogGrid?.grid?.api?.stopEditing();

      setTimeout(() => {
        const rows = this.items.getValue();
        if (rows.some(i => !i.comment?.trim())) {
          rows.forEach(i => {
            if (!i.comment?.trim()) {
              i.comment = this.form.generalComment.value;
            }
          });
        }

        this.dialogRef.close({
          budgetRowChangeLogItems: this.items.value,
        });
      }, 100);
    }
  }

  valueChanged() {
    let disableOk = false;
    const rows = this.items.getValue();
    if (!this.form.generalComment?.value?.trim()) {
      if (rows.some(i => !i.comment?.trim())) {
        disableOk = true;
      }
    }

    this.okButtonDisabled.set(disableOk);
  }
}
