import {
  SoeCheckboxFormControl,
  SoeFormGroup,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { StockDTO, StockShelfDTO } from './stock-warehouse.model';
import { FormArray, FormControl } from '@angular/forms';
import { WarehouseCodeShelfForm } from './stock-warehouse-shelf-form.model';
import {
  IAccountingSettingsRowDTO,
  IStockShelfDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { AccountingSettingsForm } from '@shared/components/accounting-settings/accounting-settings/accounting-settings-form.model';
import { StockProductDTO } from '@features/billing/stock-balance/models/stock-balance.model';
import { arrayToFormArray } from '@shared/util/form-util';

interface IWarehouseCodeForm {
  validationHandler: ValidationHandler;
  element: StockDTO | undefined;
}
export class StockWarehouseForm extends SoeFormGroup {
  warehouseCodeValidationHandler: ValidationHandler;

  constructor({ validationHandler, element }: IWarehouseCodeForm) {
    super(validationHandler, {
      stockId: new SoeTextFormControl(element?.stockId || 0, {
        isIdField: true,
      }),
      name: new SoeTextFormControl(
        element?.name || '',
        {
          isNameField: true,
          required: true,
        },
        'billing.stock.stocks.name'
      ),
      code: new SoeTextFormControl(
        element?.code || '',
        { required: true },
        'billing.stock.stocks.code'
      ),
      isExternal: new SoeCheckboxFormControl(element?.isExternal || false),
      stockShelves: new FormArray<WarehouseCodeShelfForm>([]),
      accountingSettings: new FormArray<AccountingSettingsForm>([]),
      deliveryAddressId: new SoeSelectFormControl(
        element?.deliveryAddressId || 0
      ),
      stockProducts: arrayToFormArray(element?.stockProducts || []),
    });

    this.warehouseCodeValidationHandler = validationHandler;
    this.customAccountingSettingsPathValue(element?.accountingSettings ?? []);
    this.customStockShelfPatchValue(element?.stockShelves ?? []);
  }

  get stockId(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.stockId;
  }

  get name(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.name;
  }

  get code(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.code;
  }

  get isExternal(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.isExternal;
  }

  get stockShelves(): FormArray<WarehouseCodeShelfForm> {
    return <FormArray>this.controls.stockShelves;
  }

  get accountingSettings(): FormArray<AccountingSettingsForm> {
    return <FormArray>this.controls.accountingSettings;
  }

  get deliveryAddressId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.deliveryAddressId;
  }

  get stockProducts(): FormArray<FormControl<StockProductDTO>> {
    return <FormArray<FormControl<StockProductDTO>>>this.controls.stockProducts;
  }

  onDoCopy(): void {
    const formArray = this.stockShelves;
    if (formArray) {
      const elementArray: StockShelfDTO[] = [];
      for (const stockShelf of formArray.controls) {
        elementArray.push(
          new StockShelfDTO(
            0,
            0,
            stockShelf.value.code,
            stockShelf.value.name,
            stockShelf.value.stockName,
            stockShelf.value.isDelete
          )
        );
      }
      this.customStockShelfPatchValue(elementArray, true);
    }
  }

  setDirtyOnstockShelvesChange(shelf: StockShelfDTO) {
    this.stockShelves.controls.forEach(x => {
      if (x.stockShelfId.value === shelf.stockShelfId) {
        x.patchValue({ code: shelf.code, name: shelf.name });
      }
    });
    this.markAsDirty();
    this.stockShelves.markAsDirty();
    this.stockShelves.markAsTouched();
  }

  setDirtyOnProductStockChange(stockProduct: StockProductDTO) {
    this.stockProducts.controls.forEach(x => {
      if (
        x.value.stockProductId === stockProduct.stockProductId &&
        stockProduct.isModified
      ) {
        x.patchValue(stockProduct);
      }
    });
    this.markAsDirty();
    this.stockProducts.markAsDirty();
    this.stockProducts.markAsTouched();
  }

  customStockShelfPatchValue(stockShelves: StockShelfDTO[], isCopy = false) {
    this.stockShelves.clear({
      emitEvent: false,
    });
    if (stockShelves) {
      for (const stockShelf of stockShelves) {
        if (isCopy) stockShelf.stockShelfId = this.getNewShelfId();
        if (
          stockShelf.stockShelfId !== null &&
          stockShelf.stockShelfId !== undefined &&
          typeof stockShelf.stockShelfId === 'number'
        ) {
          const row = new WarehouseCodeShelfForm({
            validationHandler: this.warehouseCodeValidationHandler,
            element: stockShelf,
          });
          if (stockShelf.isDelete) row.disable();
          this.stockShelves.push(row, {
            emitEvent: false,
          });
        }
      }
      this.stockShelves.updateValueAndValidity();
    }
    return <StockShelfDTO[]>this.stockShelves.value;
  }

  customAccountingSettingsPathValue(rows: IAccountingSettingsRowDTO[]) {
    this.patchAccountingSettingsRows(rows);
  }

  customPatch(element: StockDTO) {
    this.reset(element);
    this.stockShelves.clear({ emitEvent: false });
    element.stockShelves.forEach(s => {
      this.stockShelves.push(
        new WarehouseCodeShelfForm({
          validationHandler: this.warehouseCodeValidationHandler,
          element: s,
        }),
        { emitEvent: false }
      );
    });
    this.stockShelves.markAsUntouched({ onlySelf: true });
    this.stockShelves.markAsPristine({ onlySelf: true });
    this.stockShelves.updateValueAndValidity();

    this.customAccountingSettingsPathValue(element.accountingSettings);
  }

  public addStockShelfvesValidators(validator: any) {
    this.stockShelves.addAsyncValidators(validator.validateStockShelfs());
  }
  public removeEmptyStockShelfs(): void {
    const dto = <StockDTO>this.getRawValue();
    if (dto.stockShelves) {
      const shelfs: IStockShelfDTO[] = [];
      dto.stockShelves.forEach(s => {
        if (
          !(
            (s.stockShelfId <= 0 && s.isDelete) ||
            (s.stockShelfId <= 0 &&
              (s.code === null || s.code === '') &&
              (s.name === null || s.name === ''))
          )
        ) {
          shelfs.push(s);
        }
      });
      //dto.stockShelves = shelfs;
      this.customStockShelfPatchValue(shelfs);
    }
    //return dto;
  }

  public filterModifiedWarehouseProducts() {
    const dto = <StockDTO>this.getRawValue();
    if (dto.stockProducts) {
      const stockProducts: StockProductDTO[] = [];
      dto.stockProducts.forEach(p => {
        if (p.isModified) {
          stockProducts.push(p);
        }
      });
      this.customWarehouseProductsPatchValue(stockProducts);
    }
  }

  private patchAccountingSettingsRows(
    rows: IAccountingSettingsRowDTO[] | undefined
  ) {
    this.accountingSettings?.clear({ emitEvent: false });
    if (rows && rows.length > 0) {
      rows.forEach(r => {
        this.accountingSettings.push(
          new AccountingSettingsForm({
            validationHandler: this.warehouseCodeValidationHandler,
            element: r,
          }),
          { emitEvent: false }
        );
      });
      this.accountingSettings.updateValueAndValidity();
    }
  }

  addShelf(): StockShelfDTO {
    const stockShelf = new StockShelfDTO(
      this.getNewShelfId(),
      this.stockId.value,
      '',
      '',
      this.name.value,
      false
    );
    const row = new WarehouseCodeShelfForm({
      validationHandler: this.warehouseCodeValidationHandler,
      element: stockShelf,
    });
    this.stockShelves.push(row, { emitEvent: false });

    this.markAsDirty();
    this.markAsTouched();
    this.stockShelves.markAsDirty();
    this.stockShelves.updateValueAndValidity();
    return stockShelf;
  }

  private getNewShelfId(): number {
    let minId = 0;

    if (this.stockShelves.value.length > 0) {
      minId = (<StockShelfDTO[]>this.stockShelves.value)
        .map(x => x.stockShelfId)
        .reduce((a, b) => Math.min(a, b));
    }

    return minId > 0 ? 0 : --minId;
  }

  deleteShelf(row: StockShelfDTO) {
    const shelves = <StockShelfDTO[]>this.stockShelves.getRawValue();
    shelves.forEach((item, idx) => {
      if (item.stockShelfId == row.stockShelfId) {
        item.isDelete = true;
        this.markAsDirty();
        this.stockShelves.markAsDirty();
      }
    });
    this.customStockShelfPatchValue(shelves);
  }

  resetShelves(): StockShelfDTO[] {
    const shelves = <StockShelfDTO[]>this.stockShelves.getRawValue();
    shelves.forEach((item, idx) => {
      item.isDelete = false;
    });

    this.stockShelves.markAsUntouched({ onlySelf: true });
    this.stockShelves.markAsPristine({ onlySelf: true });

    return this.customStockShelfPatchValue(shelves);
  }

  customWarehouseProductsPatchValue(products: StockProductDTO[] | undefined) {
    this.stockProducts.clear({ emitEvent: false });
    if (products && products.length > 0) {
      products.forEach(p => {
        this.stockProducts.push(
          new FormControl<StockProductDTO>(p, { nonNullable: true }),
          { emitEvent: false }
        );
      });
      this.stockProducts.updateValueAndValidity();
    }
  }
}
