import {
  SoeDateFormControl,
  SoeFormGroup,
  SoeNumberFormControl,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { PurchaseDTO } from './purchase.model';
import { FormArray } from '@angular/forms';
import { OriginUserForm } from './purchase-origin-user-form.model';
import { IOriginUserSmallDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { SoeOriginStatus } from '@shared/models/generated-interfaces/Enumerations';
import { PurchaseRowForm } from './purchase-row-form.model';
import { PurchaseRowDTO } from './purchase-rows.model';

interface IPurchaseForm {
  validationHandler: ValidationHandler;
  element: PurchaseDTO | undefined;
}
export class PurchaseForm extends SoeFormGroup {
  purchaseValidationHandler: ValidationHandler;
  constructor({ validationHandler, element }: IPurchaseForm) {
    super(validationHandler, {
      purchaseId: new SoeTextFormControl(element?.purchaseId || 0, {
        isIdField: true,
      }),
      supplierId: new SoeSelectFormControl(element?.supplierId || undefined, {
        required: true,
      }),
      projectId: new SoeTextFormControl(element?.projectId || undefined, {}),
      projectNr: new SoeTextFormControl(element?.projectNr || '', {}),
      purchaseNr: new SoeTextFormControl(element?.purchaseNr || '', {
        isNameField: true,
      }),
      statusName: new SoeTextFormControl(element?.statusName || '', {}),
      purchaseLabel: new SoeTextFormControl(element?.purchaseLabel || '', {}),
      participants: new SoeTextFormControl(element?.participants || '', {}),
      supplierCustomerNr: new SoeTextFormControl(
        element?.supplierCustomerNr || '',
        {}
      ),
      origindescription: new SoeTextFormControl(
        element?.origindescription || '',
        {}
      ),
      referenceOur: new SoeTextFormControl(
        element?.referenceOur || undefined,
        {}
      ),
      referenceOurId: new SoeSelectFormControl(
        element?.referenceOurId || undefined,
        {}
      ),
      referenceYour: new SoeSelectFormControl(
        element?.referenceYour || undefined,
        {}
      ),
      orderId: new SoeNumberFormControl(element?.orderId || undefined),
      orderNr: new SoeTextFormControl(element?.orderNr || '', {}),
      originStatus: new SoeTextFormControl(
        element?.originStatus || SoeOriginStatus.Origin,
        {}
      ),
      confirmedDeliveryDate: new SoeDateFormControl(
        element?.confirmedDeliveryDate || undefined,
        {}
      ),

      deliveryTypeId: new SoeSelectFormControl(
        element?.deliveryTypeId || undefined,
        {}
      ),
      deliveryConditionId: new SoeSelectFormControl(
        element?.deliveryConditionId || undefined,
        {}
      ),
      stockId: new SoeSelectFormControl(element?.stockId || undefined, {}),
      stockCode: new SoeTextFormControl(element?.stockCode || '', {}),
      wantedDeliveryDate: new SoeDateFormControl(
        element?.wantedDeliveryDate || undefined,
        {}
      ),
      purchaseDate: new SoeDateFormControl(element?.purchaseDate || undefined, {
        required: true,
      }),
      vatType: new SoeNumberFormControl(element?.vatType || undefined),

      contactEComId: new SoeSelectFormControl(
        element?.contactEComId || undefined
      ),
      paymentConditionId: new SoeSelectFormControl(
        element?.paymentConditionId || undefined,
        {}
      ),
      deliveryAddressId: new SoeSelectFormControl(
        element?.deliveryAddressId || undefined
      ),
      deliveryAddress: new SoeTextFormControl(
        element?.deliveryAddress || '',
        {}
      ),
      currencyId: new SoeSelectFormControl(element?.currencyId || 0, {}),
      currencyRate: new SoeNumberFormControl(element?.currencyRate || 0.0, {
        decimals: 1,
        minDecimals: 1,
        maxDecimals: 4,
      }),
      currencyDate: new SoeDateFormControl(
        element?.currencyDate || undefined,
        {}
      ),

      totalAmountExVatCurrency: new SoeNumberFormControl(
        element?.totalAmountExVatCurrency || undefined
      ),
      totalAmountCurrency: new SoeNumberFormControl(
        element?.totalAmountCurrency || undefined
      ),
      vatAmountCurrency: new SoeNumberFormControl(
        element?.vatAmountCurrency || undefined
      ),
      supplierEmail: new SoeTextFormControl(element?.supplierEmail || '', {}),

      originUsers: new FormArray<OriginUserForm>([]),
      purchaseRows: new FormArray<PurchaseRowForm>([]),
    });
    this.purchaseValidationHandler = validationHandler;
    if (element?.purchaseRows)
      this.customPurchaseRowsPatchValue(
        element?.purchaseRows as PurchaseRowDTO[]
      );
    if (element?.originUsers)
      this.customOriginUsersPatchValue(element?.originUsers);
  }

  get purchaseId(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.purchaseId;
  }
  get supplierId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.supplierId;
  }
  get projectId(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.projectId;
  }
  get projectNr(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.projectNr;
  }
  get purchaseNr(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.purchaseNr;
  }
  get statusName(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.statusName;
  }
  get participants(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.participants;
  }
  get supplierCustomerNr(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.supplierCustomerNr;
  }
  get purchaseLabel(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.purchaseLabel;
  }
  get referenceOur(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.referenceOur;
  }
  get referenceOurId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.referenceOurId;
  }
  get referenceYour(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.referenceYour;
  }
  get deliveryTypeId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.deliveryTypeId;
  }
  get deliveryConditionId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.deliveryConditionId;
  }
  get stockId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.stockId;
  }
  get stockCode(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.stockCode;
  }

  get origindescription(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.origindescription;
  }
  get orderId(): SoeDateFormControl {
    return <SoeNumberFormControl>this.controls.orderId;
  }
  get orderNr(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.orderNr;
  }
  get originStatus(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.originStatus;
  }
  get confirmedDeliveryDate(): SoeDateFormControl {
    return <SoeDateFormControl>this.controls.confirmedDeliveryDate;
  }
  get wantedDeliveryDate(): SoeDateFormControl {
    return <SoeDateFormControl>this.controls.wantedDeliveryDate;
  }
  get purchaseDate(): SoeDateFormControl {
    return <SoeDateFormControl>this.controls.purchaseDate;
  }

  get vatType(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.vatType;
  }
  get contactEComId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.contactEComId;
  }
  get paymentConditionId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.paymentConditionId;
  }
  get deliveryAddressId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.deliveryAddressId;
  }
  get deliveryAddress(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.deliveryAddress;
  }
  get currencyId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.currencyId;
  }
  get currencyRate(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.currencyRate;
  }
  get currencyDate(): SoeDateFormControl {
    return <SoeDateFormControl>this.controls.currencyDate;
  }

  get totalAmountCurrency(): SoeDateFormControl {
    return <SoeDateFormControl>(
      this.controls.totalAmountotalAmountCurrencyExVatCurrency
    );
  }
  get totalAmountExVatCurrency(): SoeDateFormControl {
    return <SoeDateFormControl>this.controls.vatAmountCurrency;
  }

  get supplierEmail(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.supplierEmail;
  }

  get originUsers(): FormArray<OriginUserForm> {
    return <FormArray>this.controls.originUsers;
  }
  get purchaseRows(): FormArray<PurchaseRowForm> {
    return <FormArray>this.controls.purchaseRows;
  }

  customPurchaseRowsPatchValue(purchaseRows: PurchaseRowDTO[]) {
    if (purchaseRows) {
      (this.controls.purchaseRows as FormArray).clear();

      if (purchaseRows) {
        for (const purchaseRow of purchaseRows) {
          const row = new PurchaseRowForm({
            validationHandler: this.purchaseValidationHandler,
            element: purchaseRow,
          });
          (this.controls.purchaseRows as FormArray).push(row, {
            emitEvent: false,
          });
        }
      }
    }
  }

  customOriginUsersPatchValue(originUsers: IOriginUserSmallDTO[]) {
    if (originUsers) {
      (this.controls.originUsers as FormArray).clear();
      let mainUser = '';
      const users: string[] = [];
      let participant = '';
      if (originUsers) {
        for (const originUser of originUsers) {
          const row = new OriginUserForm({
            validationHandler: this.purchaseValidationHandler,
            element: originUser,
          });
          (this.controls.originUsers as FormArray).push(row, {
            emitEvent: false,
          });
          if (originUser.main) {
            mainUser = originUser.name;
          } else {
            users.push(originUser.name);
          }
        }
        if (mainUser) {
          participant = '<b>' + mainUser + '</b>';
        }
        if (users.length > 0) {
          if (participant) {
            participant = participant + ',';
          }
          participant = participant + users.join(',');
        }
        this.participants.patchValue(participant);
      }
    }
  }
}
