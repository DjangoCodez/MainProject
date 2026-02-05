import { SoeFormGroup, SoeNumberFormControl, SoeSelectFormControl, SoeTextFormControl } from "@shared/extensions";
import { ValidationHandler } from "@shared/handlers";
import { SmallGenericType } from '@shared/models/generic-type.model';
import { SoeSupplierAgreemntCodeType } from "@shared/models/generated-interfaces/Enumerations";
import { ISupplierAgreementDTO } from "@shared/models/generated-interfaces/SupplierAgreementDTOs";

interface ISupplierAgreementForm {
  validationHandler: ValidationHandler;
  element: ISupplierAgreementDTO | undefined;
  wholesellersDict: SmallGenericType[];
}

export class SupplierAgreementForm extends SoeFormGroup {

  get isExisting() {
    return this.controls.rebateListId.value > 0;
  }

  get isCodeTypeGeneric() {
    return this.controls.codeType.value == SoeSupplierAgreemntCodeType.Generic;
  }

  constructor({ validationHandler, element, wholesellersDict }: ISupplierAgreementForm) {
    super(validationHandler, {
      rebateListId: new SoeNumberFormControl(
        element?.rebateListId || 0,
        {
          isIdField: true
        }
      ),
      sysWholesellerId: new SoeSelectFormControl(
        element?.wholesellerName ? wholesellersDict.find(x => x.name === element.wholesellerName)?.id : null,
        {
          required: true
        },
        'common.customer.customer.wholesellername'
      ),
      priceListTypeId: new SoeSelectFormControl(
        element?.priceListTypeId || 0,
        {},
        'billing.order.pricelisttype'
      ),
      codeType: new SoeSelectFormControl(
        element?.codeType || SoeSupplierAgreemntCodeType.Generic,
        {
          required: true
        },
        'common.type'
      ),
      code: new SoeTextFormControl(
        element?.code || '',
        {
          required: true
        },
        'billing.invoices.supplieragreement.materialclassproductnr'
      ),
      discountPercent: new SoeNumberFormControl(
        element?.discountPercent || 0,
        {
          required: true,
          decimals: 2
        },
        'billing.productrows.dialogs.discountpercent'
      )
    });

    if (this.isExisting) {
      this.controls.sysWholesellerId.disable();
      this.controls.priceListTypeId.disable();
    }

    this.toggleCodeType(this.controls.sysWholesellerId.value);
    this.toggleCode(this.controls.codeType.value);

    this.controls.sysWholesellerId.valueChanges.subscribe(sysWholesellerId => {
      this.toggleCodeType(sysWholesellerId);
    });

    this.controls.codeType.valueChanges.subscribe(codeType => {
      this.toggleCode(codeType);
    });
  }

  private toggleCodeType(sysWholesellerId: number) {
    if (this.isExisting || (sysWholesellerId != 62 && sysWholesellerId != 63)) {
      this.controls.codeType.disable();
    } else {
      this.controls.codeType.enable();
    }
  }

  private toggleCode(codeType: number) {
    if (this.isExisting || codeType == SoeSupplierAgreemntCodeType.Generic) {
      this.controls.code.disable();
    } else {
      this.controls.code.enable();
    }
  }
}