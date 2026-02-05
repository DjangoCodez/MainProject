import { ValidationHandler } from '@shared/handlers';
import {
  SoeCheckboxFormControl,
  SoeDateFormControl,
  SoeFormGroup,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import {
  StockInventoryHeadDTO,
  StockInventoryRowDTO,
} from './stock-inventory.model';
import { FormArray } from '@angular/forms';
import { StockInventoryRowForm } from './stock-inventory-row-form.model';
import { IStockInventoryRowDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { EventEmitter } from '@angular/core';

interface IStockInventoryHeadForm {
  validationHandler: ValidationHandler;
  element: StockInventoryHeadDTO | undefined;
}

export class StockInventoryHeadForm extends SoeFormGroup {
  thisValidationHandler: ValidationHandler;
  constructor({ validationHandler, element }: IStockInventoryHeadForm) {
    super(validationHandler, {
      stockInventoryHeadId: new SoeTextFormControl(
        element?.stockInventoryHeadId || 0,
        { isIdField: true }
      ),
      headerText: new SoeTextFormControl(
        element?.headerText || '',
        {
          isNameField: true,
          required: true,
        },
        'common.name'
      ),
      stockId: new SoeSelectFormControl(
        element?.stockId || undefined,
        {
          required: true,
        },
        'billing.stock.stocks.stock'
      ),
      inventoryStart: new SoeDateFormControl(
        element?.inventoryStart || undefined
      ),
      inventoryStop: new SoeDateFormControl(
        element?.inventoryStop || undefined
      ),
      inventoryStartStr: new SoeTextFormControl(
        element?.inventoryStartStr || ''
      ),
      inventoryStopStr: new SoeTextFormControl(element?.inventoryStopStr || ''),
      hasGeneratedRows: new SoeCheckboxFormControl(
        undefined,
        { required: true },
        'billing.stock.stockinventory.generate'
      ),
      stockInventoryRows: new FormArray<StockInventoryRowForm>([]),
    });
    this.thisValidationHandler = validationHandler;

    this.inventoryStartStr.disable();
    this.inventoryStopStr.disable();
  }

  get stockInventoryHeadId(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.stockInventoryHeadId;
  }

  get headerText(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.headerText;
  }

  get stockId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.stockId;
  }

  get stockInventoryRows(): FormArray<StockInventoryRowForm> {
    return <FormArray>this.controls.stockInventoryRows;
  }

  get inventoryStart(): SoeDateFormControl {
    return <SoeDateFormControl>this.controls.inventoryStart;
  }

  get inventoryStop(): SoeDateFormControl {
    return <SoeDateFormControl>this.controls.inventoryStop;
  }

  get inventoryStartStr(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.inventoryStartStr;
  }

  get inventoryStopStr(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.inventoryStopStr;
  }

  get hasGeneratedRows(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.hasGeneratedRows;
  }

  customPatchValue(element: StockInventoryHeadDTO) {
    (this.controls.stockInventoryRows as FormArray).clear();

    for (const stockInventoryRow of element.stockInventoryRows) {
      const inventoryRow = new StockInventoryRowForm({
        validationHandler: this.thisValidationHandler,
        element: stockInventoryRow,
      });
      this.stockInventoryRows.push(inventoryRow, { emitEvent: false });
    }
    this.patchValue(element);
    this.hasGeneratedRows.patchValue(true);
    (<EventEmitter<any>>this.stockInventoryRows.statusChanges).emit(
      this.stockInventoryRows.status
    );
    this.markAsPristine();
    this.markAsUntouched();
  }

  addStockInventoryRows(rows: IStockInventoryRowDTO[]): void {
    this.stockInventoryRows.clear();
    rows.forEach(obj => {
      this.addStockInventoryRowForm(undefined, obj as StockInventoryRowDTO);
    });
    (<EventEmitter<any>>this.stockInventoryRows.statusChanges).emit(
      this.stockInventoryRows.status
    );
  }

  addStockInventoryRowForm(
    rowForm: StockInventoryRowForm | undefined,
    stockInventoryRow: StockInventoryRowDTO | undefined
  ) {
    this.stockInventoryRows.push(
      rowForm ??
        new StockInventoryRowForm({
          validationHandler: this.thisValidationHandler,
          element: stockInventoryRow ?? new StockInventoryRowDTO(),
        }),
      { emitEvent: false }
    );
  }
}
