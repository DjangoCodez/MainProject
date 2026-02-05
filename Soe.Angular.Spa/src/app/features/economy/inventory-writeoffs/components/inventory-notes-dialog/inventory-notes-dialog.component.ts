import { Component, inject, signal } from '@angular/core';
import { DialogComponent } from '@ui/dialog/dialog/dialog.component';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import {
  InventoryNotesDialogData,
  SaveInventoryNotesModel,
} from './models/inventory-notes.model';
import { AccountDistributionEntryDTO } from '../../models/inventory-writeoffs.model';
import { InventoryNotesForm } from './models/inventory-notes-form.model';
import { ValidationHandler } from '@shared/handlers/validation.handler';
import { InventoryWriteoffsService } from '../../services/inventory-writeoffs.service';
import { Perform } from '@shared/util/perform.class';
import { CrudActionTypeEnum } from '@shared/enums';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Component({
  selector: 'soe-inventory-notes-dialog',
  templateUrl: './inventory-notes-dialog.component.html',
  providers: [FlowHandlerService],
  standalone: false,
})
export class InventoryNotesDialogComponent extends DialogComponent<InventoryNotesDialogData> {
  validationHandler = inject(ValidationHandler);
  inventoryWriteoffsService = inject(InventoryWriteoffsService);
  progressService = inject(ProgressService);
  selectedObject = signal<AccountDistributionEntryDTO>(
    new AccountDistributionEntryDTO()
  );
  performSave = new Perform<BackendResponse>(this.progressService);

  private currentIndex: number = 0;

  form: InventoryNotesForm;

  constructor() {
    super();
    this.currentIndex = this.data.rows.indexOf(this.data.selectedRow);
    this.form = new InventoryNotesForm({
      validationHandler: this.validationHandler,
      element: new SaveInventoryNotesModel(
        this.data.selectedRow.inventoryId as number,
        this.data.selectedRow.inventoryName,
        this.data.selectedRow.inventoryNotes,
        this.data.selectedRow.inventoryDescription
      ),
    });
  }

  public isLeftButtonDisabled(): boolean {
    return this.currentIndex == 0;
  }

  public isRightButtonDisabled(): boolean {
    return this.currentIndex == this.data.rows.length - 1;
  }

  public leftButtonClick() {
    this.moveLeft();
  }

  public rightButtonClick() {
    this.moveRight();
  }

  public moveLeft() {
    if (this.currentIndex > 0) {
      this.currentIndex = this.currentIndex - 1;
    }
    this.setRow();
  }

  public moveRight() {
    if (this.currentIndex < this.data.rows.length) {
      this.currentIndex = this.currentIndex + 1;
    }
    this.setRow();
  }

  private setRow() {
    this.save(() => this.performSetRow());
    this.form.markAsPristine();
  }

  private performSetRow() {
    const currentData = this.form.getRawValue();
    this.updateModel(currentData);

    this.data.title = this.data.selectedRow.inventoryName;
    this.form.patchValue(
      SaveInventoryNotesModel.fromAccountDistributionEntryDTO(
        this.data.selectedRow
      )
    );
  }

  private updateModel(currentData: any) {
    const item = this.data.rows.find(
      o => o.inventoryId === currentData.inventoryId
    );
    if (item) {
      item.inventoryNotes = currentData.notes;
      item.inventoryDescription = currentData.description;
      this.inventoryWriteoffsService.setNotesIcon(item);
    }
    this.data.selectedRow = this.data.rows[this.currentIndex];
  }

  private save(callback: () => void) {
    if (this.form.dirty) {
      this.performSave.crud(
        CrudActionTypeEnum.Save,
        this.inventoryWriteoffsService.saveNotesAndDescription(
          this.form.getRawValue()
        ),
        callback
      );
    } else callback();
  }

  public ok() {
    this.save(() => {
      this.performSetRow();
      this.dialogRef.close(this.data.rows);
    });
  }

  public cancel() {
    this.dialogRef.close(this.data.rows);
  }
}
