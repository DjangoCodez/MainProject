import { Component, inject } from '@angular/core';
import { MatDialogRef } from '@angular/material/dialog';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ISupplierGridDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ButtonComponent } from '@ui/button/button/button.component'
import { DialogComponent } from '@ui/dialog/dialog/dialog.component'
import { DialogData } from '@ui/dialog/models/dialog'
import { GridComponent } from '@ui/grid/grid.component'
import { InstructionComponent } from '@ui/instruction/instruction.component'
import { OptionsUtil } from '@ui/grid/util/options-util';
import { IGridFilterModified } from '@ui/grid/interfaces';
import { BehaviorSubject, take } from 'rxjs';
import { SupplierService } from '../../../features/economy/services/supplier.service';

@Component({
  selector: 'soe-select-supplier-modal',
  imports: [
    TranslateModule,
    ButtonComponent,
    InstructionComponent,
    DialogComponent,
    GridComponent,
  ],
  providers: [FlowHandlerService, SupplierService],
  templateUrl: './select-supplier-modal.component.html',
  styleUrls: ['./select-supplier-modal.component.scss'],
})
export class SelectSupplierModalComponent extends DialogComponent<DialogData> {
  translate = inject(TranslateService);
  flowHandler = inject(FlowHandlerService);
  supplierService = inject(SupplierService);
  rows = new BehaviorSubject<ISupplierGridDTO[]>([]);
  grid!: GridComponent<ISupplierGridDTO>;
  dialogRef = inject(MatDialogRef);

  constructor() {
    super();
    this.flowHandler.execute({
      setupGrid: this.setupGrid.bind(this),
    });
  }

  setupGrid(grid: GridComponent<ISupplierGridDTO>) {
    this.grid = grid;
    this._constructGridColumns();
  }

  private _constructGridColumns() {
    this.grid.addColumnText(
      'supplierNr',
      this.translate.instant('common.number'),
      {
        flex: 40,
      }
    );
    this.grid.addColumnText('name', this.translate.instant('common.name'), {
      flex: 60,
    });

    const selection = new OptionsUtil().defaultSelectionOptions;
    selection.mode = 'singleRow';
    selection.enableClickSelection = true;
    selection.checkboxes = false;
    this.grid.selection = selection;

    this.grid.finalizeInitGrid();
    this.grid.api.setFocusedHeader('supplierNr', true);
  }

  selectRow(): void {
    const selectedRow = this.grid.getSelectedRows();
    this.dialogRef.close(selectedRow[0]);
  }

  filterModified(evt: IGridFilterModified): void {
    this.supplierService
      .getSuppliersBySearch({
        supplierNumber: evt['supplierNr']?.filter || '',
        supplierName: evt['name']?.filter || '',
      })
      .pipe(take(1))
      .subscribe(rows => {
        this.rows.next(rows);
      });
  }
}
