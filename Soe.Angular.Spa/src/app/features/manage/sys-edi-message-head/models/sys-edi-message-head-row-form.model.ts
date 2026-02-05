import {
  SoeFormGroup,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { SysEdiMessageRowDTO } from './sys-edi-message-head.model';

interface ISysEdiMessageHeadRowForm {
  validationHandler: ValidationHandler;
  element: SysEdiMessageRowDTO | undefined;
}
export class SysEdiMessageHeadRowForm extends SoeFormGroup {
  constructor({ validationHandler, element }: ISysEdiMessageHeadRowForm) {
    super(validationHandler, {
      sysEdiMessageRowId: new SoeTextFormControl(
        element?.sysEdiMessageRowId || 0,
        { isIdField: true }
      ),
      sysEdiMessageHeadId: new SoeTextFormControl(
        element?.sysEdiMessageHeadId || 0
      ),
      rowSellerArticleNumber: new SoeTextFormControl(element?.rowSellerArticleNumber || ''),
      rowSellerArticleDescription1: new SoeTextFormControl(element?.rowSellerArticleDescription1 || ''),
      rowSellerArticleDescription2: new SoeTextFormControl(element?.rowSellerArticleDescription2 || ''),
      rowSellerRowNumber: new SoeTextFormControl(element?.rowSellerRowNumber || ''),
      rowBuyerArticleNumber: new SoeTextFormControl(element?.rowBuyerArticleNumber || ''),
      rowBuyerRowNumber: new SoeTextFormControl(element?.rowBuyerRowNumber || ''),
      rowDeliveryDate: new SoeTextFormControl(element?.rowDeliveryDate || ''),
      rowBuyerReference: new SoeTextFormControl(element?.rowBuyerReference || ''),
      rowBuyerObjectId: new SoeTextFormControl(element?.rowBuyerObjectId || ''),
      rowQuantity: new SoeTextFormControl(element?.rowQuantity || ''),
      rowUnitCode: new SoeTextFormControl(element?.rowUnitCode || ''),
      rowUnitPrice: new SoeTextFormControl(element?.rowUnitPrice || ''),
      rowDiscountPercent: new SoeTextFormControl(element?.rowDiscountPercent || ''),
      rowDiscountAmount: new SoeTextFormControl(element?.rowDiscountAmount || ''),
      rowDiscountPercent1: new SoeTextFormControl(element?.rowDiscountPercent1 || ''),
      rowDiscountAmount1: new SoeTextFormControl(element?.rowDiscountAmount1 || ''),
      rowDiscountPercent2: new SoeTextFormControl(element?.rowDiscountPercent2 || ''),
      rowDiscountAmount2: new SoeTextFormControl(element?.rowDiscountAmount2 || ''),
      rowNetAmount: new SoeTextFormControl(element?.rowNetAmount || ''),
      rowVatAmount: new SoeTextFormControl(element?.rowVatAmount || ''),
      rowVatPercentage: new SoeTextFormControl(element?.rowVatPercentage || ''),
      externalProductId: new SoeTextFormControl(element?.externalProductId || ''),
      stockCode: new SoeTextFormControl(element?.stockCode || ''),
    });
  }
}
