import {
  SoeFormGroup,
  SoeNumberFormControl,
  SoeRadioFormControl,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import {
  PurchaseCartDTO,
  PurchaseCartRowDTO,
} from './price-optimization.model';
import { TermGroup_PurchaseCartStatus } from '@shared/models/generated-interfaces/Enumerations';
import { FormArray } from '@angular/forms';

interface IPriceOptimizationForm {
  validationHandler: ValidationHandler;
  element: PurchaseCartDTO | undefined;
}
export class PriceOptimizationForm extends SoeFormGroup {
  cartValidationHandler: ValidationHandler;

  constructor({ validationHandler, element }: IPriceOptimizationForm) {
    super(validationHandler, {
      purchaseCartId: new SoeNumberFormControl(element?.purchaseCartId || 0, {
        isIdField: true,
      }),
      name: new SoeTextFormControl(
        element?.name || undefined,
        {
          isNameField: true,
          required: true,
          maxLength: 100,
        },
        'common.name'
      ),
      seqNr: new SoeTextFormControl(element?.seqNr || undefined, {}),
      statusName: new SoeTextFormControl(element?.statusName || '', {
        disabled: true,
      }),
      priceStrategy: new SoeRadioFormControl(element?.priceStrategy || 2),
      selectedWholesellerIds: new SoeNumberFormControl(
        element?.selectedWholesellerIds || []
      ),
      description: new SoeTextFormControl(element?.description || '', {}),
      status: new SoeNumberFormControl(
        element?.status || TermGroup_PurchaseCartStatus.Open,
        {
          disabled: true,
        }
      ),
      purchaseCartRows: new FormArray<PurchaseCartRowForm>([]),
    });
    this.cartValidationHandler = validationHandler;
    this.customRowPatchValue(element?.purchaseCartRows ?? []);
  }

  get purchaseCartId(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.purchaseCartId;
  }

  get name(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.name;
  }

  get statusValue(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.status;
  }

  get statusName(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.statusName;
  }

  get seqNr(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.seqNr;
  }

  get description(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.description;
  }

  get priceStrategy(): SoeRadioFormControl {
    return <SoeRadioFormControl>this.controls.priceStrategy;
  }

  get selectedWholesellerIds(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.selectedWholesellerIds;
  }

  get purchaseCartRows(): FormArray<PurchaseCartRowForm> {
    return <FormArray<PurchaseCartRowForm>>this.controls.purchaseCartRows;
  }

  onDoCopy() {
    const formArray = this.purchaseCartRows;

    if (formArray) {
      const elementArray: PurchaseCartRowDTO[] = [];
      for (const cartRow of formArray.controls) {
        elementArray.push(
          new PurchaseCartRowDTO(
            0,
            0,
            cartRow.value.productInfo,
            cartRow.value.productName,
            cartRow.value.productNr,
            cartRow.value.imageUrl,
            cartRow.value.type,
            cartRow.value.externalId,
            cartRow.value.purchasePrice,
            cartRow.value.quantity,
            cartRow.value.sysWholesellerId,
            cartRow.value.sysProductId,
            true
          )
        );
      }

      this.customRowPatchValue(elementArray);
    }
  }

  customRowPatchValue(purchaseCartRows: PurchaseCartRowDTO[] | undefined) {
    this.purchaseCartRows.clear({ emitEvent: false });

    if (purchaseCartRows && purchaseCartRows.length > 0) {
      for (const purchaseCartRow of purchaseCartRows) {
        const row = new PurchaseCartRowForm({
          validationHandler: this.cartValidationHandler,
          element: purchaseCartRow,
        });
        this.purchaseCartRows.push(row, { emitEvent: false });
      }

      this.purchaseCartRows.updateValueAndValidity();
    }
  }
}

interface IPriceOptimizationRowForm {
  validationHandler: ValidationHandler;
  element: PurchaseCartRowDTO | undefined;
}

export class PurchaseCartRowForm extends SoeFormGroup {
  constructor({ validationHandler, element }: IPriceOptimizationRowForm) {
    super(validationHandler, {
      purchaseCartRowId: new SoeNumberFormControl(
        element?.purchaseCartRowId || 0,
        { isIdField: true }
      ),
      purchaseCartId: new SoeNumberFormControl(
        element?.purchaseCartId || 0,
        {}
      ),
      productName: new SoeTextFormControl(element?.productName || '', {}),
      productNr: new SoeTextFormControl(element?.productNr || '', {}),
      productInfo: new SoeTextFormControl(element?.productInfo || '', {}),
      imageUrl: new SoeTextFormControl(element?.imageUrl || '', {}),
      type: new SoeNumberFormControl(element?.type || 0, {}),
      externalId: new SoeNumberFormControl(element?.externalId || 0, {}),
      sysProductId: new SoeNumberFormControl(element?.sysProductId || 0, {}),
      purchasePrice: new SoeNumberFormControl(element?.purchasePrice || 0, {}),
      sysPricelistHeadId: new SoeNumberFormControl(
        element?.sysPricelistHeadId || 0,
        {}
      ),
      wholesellerNetPriceId: new SoeNumberFormControl(
        element?.wholesellerNetPriceId || 0,
        {}
      ),
      quantity: new SoeNumberFormControl(element?.quantity || 0, {}),
      sysWholesellerId: new SoeNumberFormControl(
        element?.sysWholesellerId || 0,
        {}
      ),
      isModified: new SoeSelectFormControl(element?.isModified || false),
    });
  }
}
