import { Component, inject, signal } from '@angular/core';
import { DialogComponent } from '@ui/dialog/dialog/dialog.component';
import { DialogData } from '@ui/dialog/models/dialog';
import {
  NoteDialogDTO,
  ProjectTimeBlockDTO,
} from '../../../models/project-time-report.model';
import { EditNoteDialogForm } from '../../../models/edit-note-dialog-form.model';
import { ValidationHandler } from '@shared/handlers';
import { DateUtil } from '@shared/util/date-util';
import { Observable, tap } from 'rxjs';
import { TranslateService } from '@ngx-translate/core';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import { ProjectTimeReportService } from '@features/billing/project-time-report/services/project-time-report.service';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

export interface IEditNoteOutputDialogData extends DialogData {
  rows: any | undefined;
  row: any | undefined;
  workTimePermission: boolean | undefined;
  invoiceTimePermission: boolean | undefined;
  isReadonly: boolean | undefined;
  saveDirect: boolean | false;
}

@Component({
  templateUrl: './edit-note-dialog.component.html',
  providers: [FlowHandlerService],
  standalone: false,
})
export class EditNoteDialogComponent extends DialogComponent<IEditNoteOutputDialogData> {
  validationHandler = inject(ValidationHandler);
  translateService = inject(TranslateService);
  service = inject(ProjectTimeReportService);
  progressService = inject(ProgressService);

  private rowIsModified = false;
  private currentIndex: number = 0;

  isButtonDisable = signal(false);

  form: EditNoteDialogForm = new EditNoteDialogForm({
    validationHandler: this.validationHandler,
    element: new NoteDialogDTO(),
  });

  constructor() {
    super();
    this.changeNoteTitle();
    this.setCurrentIndex();
    this.isReadOnly();
    this.isLeftButtonDisabled();

    this.form.valueChanges.subscribe(() => {
      this.rowIsModified = true;
    });

    this.updateForm();
  }

  cancel(returnValues: boolean = false) {
    if (returnValues) {
      this.dialogRef.close({
        ...this.form.value,
      });
    } else this.dialogRef.close();
  }

  isLeftButtonDisabled(): boolean {
    return this.currentIndex == 0;
  }

  isRightButtonDisabled(): boolean {
    return this.currentIndex == this.data.rows.length - 1;
  }

  leftButtonClick() {
    if (this.rowIsModified) {
      this.save(this.data.row).subscribe(() => {
        this.moveLeft();
      });
    } else this.moveLeft();
  }

  updateForm(row = this.data.row) {
    this.form.patchValue({
      internalNote: row.internalNote,
      externalNote: row.externalNote,
    });
  }

  rightButtonClick() {
    //test this.form.externalNote.enabled
    if (this.rowIsModified) {
      this.save(this.data.row).subscribe(() => {
        this.moveRight();
      });
    } else this.moveRight();
  }

  private moveLeft() {
    if (this.currentIndex > 0) {
      this.currentIndex = this.currentIndex - 1;
    }
    this.setRow();
  }

  private moveRight() {
    if (this.currentIndex < this.data.rows.length) {
      this.currentIndex = this.currentIndex + 1;
    }
    this.setRow();
  }

  private setRow() {
    this.data.row = this.data.rows[this.currentIndex];
    this.updateForm(this.data.row);

    this.rowIsModified = false;

    this.isLeftButtonDisabled();
    this.isRightButtonDisabled();
    this.isReadOnly();

    this.changeNoteTitle(this.data.rows[this.currentIndex]);
  }

  private changeNoteTitle(row = this.data.row) {
    const name = row.employeeName;
    const date = DateUtil.format(row.date, 'yyyy-MM-dd');

    let _title = `${name}, ${date}`;

    if (this.data.workTimePermission) {
      const quantityText =
        this.translateService.instant(
          'billing.project.timesheet.editnote.quantity'
        ) || '';
      const quantity = row.timePayrollQuantityFormatted || '0:00';

      if (quantityText) _title = _title + `, ${quantityText}`;
      if (quantity) _title = _title + ` (${quantity})`;
    }
    if (this.data.invoiceTimePermission) {
      const invoiceQtyText =
        this.translateService.instant(
          'billing.project.timesheet.editnote.invoicequantity'
        ) || '';
      const invoiceQty = row.invoiceQuantityFormatted || '0:00';
      if (invoiceQtyText) _title = _title + `, ${invoiceQtyText}`;
      if (invoiceQty) _title = _title + ` (${invoiceQty})`;
    }

    this.data.title = _title;
  }

  isReadOnly() {
    if (this.data.isReadonly || !this.data.rows[this.currentIndex].isEditable) {
      this.form.externalNote.disable();
      this.form.internalNote.disable();
      this.isButtonDisable.set(
        this.data.isReadonly || !this.data.rows[this.currentIndex].isEditable
      );
    } else {
      this.form.externalNote.enable();
      this.form.internalNote.enable();
    }
  }

  private setCurrentIndex() {
    this.currentIndex = this.data.rows.indexOf(this.data.row)
      ? this.data.rows.indexOf(this.data.row)
      : 0;
  }

  private save(row: ProjectTimeBlockDTO): Observable<BackendResponse> {
    const dto: any = {
      projectTimeBlockId: row.projectTimeBlockId,
      date: row.date,
      externalNote: this.form.externalNote.value,
      internalNote: this.form.internalNote.value,
      projectInvoiceWeekId: row.projectInvoiceWeekId,
    };

    return this.service.saveNotesForProjectTimeBlock(dto).pipe(tap(() => {}));
  }

  protected ok(): void {
    if (this.rowIsModified && this.data.saveDirect) {
      this.save(this.data.row).subscribe(() => {
        this.cancel(true);
      });
    } else {
      this.cancel(true);
    }
  }
}
