import { FormArray } from '@angular/forms';
import {
  SoeFormGroup,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { SysEdiMessageHeadRowForm } from './sys-edi-message-head-row-form.model';
import { SysEdiMessageHeadDTO } from './sys-edi-message-head.model';

interface ISysEdiMessageHeadForm {
  validationHandler: ValidationHandler;
  element: SysEdiMessageHeadDTO | undefined;
}
export class SysEdiMessageHeadForm extends SoeFormGroup {
  sysEdiMessageHeadValidationHandler: ValidationHandler;
  constructor({ validationHandler, element }: ISysEdiMessageHeadForm) {
    super(validationHandler, {
      sysEdiMessageHeadId: new SoeTextFormControl(
        element?.sysEdiMessageHeadId || 0,
        {
          isIdField: true,
        },
        'sysEdiMessageHeadId'
      ),
      sysEdiMessageHeadGuid: new SoeTextFormControl(
        element?.sysEdiMessageHeadGuid || '',
        {},
        'sysEdiMessageHeadGuid'
      ),

      sysCompanyId: new SoeSelectFormControl(
        element?.sysCompanyId || 0,
        {},
        'sysCompanyId'
      ),

      sysEdiMessageHeadStatus: new SoeTextFormControl(
        element?.sysEdiMessageHeadStatus || '',
        {},
        'sysEdiMessageHeadStatus'
      ),

      sysEdiMessageRawId: new SoeTextFormControl(
        element?.sysEdiMessageRawId || '',
        {},
        'sysEdiMessageRawId'
      ),
      ediSourceType: new SoeTextFormControl(
        element?.ediSourceType || '',
        { maxLength: 100 },
        'eDISourceType'
      ),
      sysEdiType: new SoeTextFormControl(
        element?.sysEdiType || '',
        { maxLength: 100 },
        'sysEdiType'
      ),
      sysEdiMsgId: new SoeTextFormControl(
        element?.sysEdiMsgId || '',
        { maxLength: 100 },
        'sysEdiMsgId'
      ),
      sysWholesellerId: new SoeTextFormControl(
        element?.sysWholesellerId || '',
        { maxLength: 100 },
        'sysWholesellerId'
      ),
      messageSenderId: new SoeTextFormControl(
        element?.messageSenderId || '',
        { maxLength: 100 },
        'messageSenderId'
      ),
      messageType: new SoeTextFormControl(
        element?.messageType || '',
        { maxLength: 100 },
        'messageType'
      ),
      messageDate: new SoeTextFormControl(
        element?.messageDate || '',
        { maxLength: 100 },
        'messageDate'
      ),
      headInvoiceNumber: new SoeTextFormControl(
        element?.headInvoiceNumber || '',
        { maxLength: 100 },
        'headInvoiceNumber'
      ),
      headInvoiceOcr: new SoeTextFormControl(
        element?.headInvoiceOcr || '',
        { maxLength: 100 },
        'headInvoiceOcr'
      ),
      headInvoiceType: new SoeTextFormControl(
        element?.headInvoiceType || '',
        { maxLength: 100 },
        'headInvoiceType'
      ),
      headInvoiceDate: new SoeTextFormControl(
        element?.headInvoiceDate || '',
        { maxLength: 100 },
        'headInvoiceDate'
      ),
      headInvoiceDueDate: new SoeTextFormControl(
        element?.headInvoiceDueDate || '',
        { maxLength: 100 },
        'headInvoiceDueDate'
      ),
      headDeliveryDate: new SoeTextFormControl(
        element?.headDeliveryDate || '',
        { maxLength: 100 },
        'headDeliveryDate'
      ),
      headBuyerOrderNumber: new SoeTextFormControl(
        element?.headBuyerOrderNumber || '',
        { maxLength: 100 },
        'headBuyerOrderNumber'
      ),
      headBuyerProjectNumber: new SoeTextFormControl(
        element?.headBuyerProjectNumber || '',
        { maxLength: 100 },
        'headBuyerProjectNumber'
      ),
      headBuyerInstallationNumber: new SoeTextFormControl(
        element?.headBuyerInstallationNumber || '',
        { maxLength: 100 },
        'headBuyerInstallationNumber'
      ),
      headSellerOrderNumber: new SoeTextFormControl(
        element?.headSellerOrderNumber || '',
        { maxLength: 100 },
        'headSellerOrderNumber'
      ),
      headPostalGiro: new SoeTextFormControl(
        element?.headPostalGiro || '',
        { maxLength: 100 },
        'headPostalGiro'
      ),
      headBankGiro: new SoeTextFormControl(
        element?.headBankGiro || '',
        { maxLength: 100 },
        'headBankGiro'
      ),
      headBank: new SoeTextFormControl(
        element?.headBank || '',
        { maxLength: 100 },
        'headBank'
      ),
      headBicAddress: new SoeTextFormControl(
        element?.headBicAddress || '',
        { maxLength: 100 },
        'headBicAddress'
      ),
      headIbanNumber: new SoeTextFormControl(
        element?.headIbanNumber || '',
        { maxLength: 100 },
        'headIbanNumber'
      ),
      headCurrencyCode: new SoeTextFormControl(
        element?.headCurrencyCode || '',
        { maxLength: 100 },
        'headCurrencyCode'
      ),
      headVatPercentage: new SoeTextFormControl(
        element?.headVatPercentage || '',
        { maxLength: 100 },
        'headVatPercentage'
      ),
      headPaymentConditionDays: new SoeTextFormControl(
        element?.headPaymentConditionDays || '',
        { maxLength: 100 },
        'headPaymentConditionDays'
      ),
      headPaymentConditionText: new SoeTextFormControl(
        element?.headPaymentConditionText || '',
        { maxLength: 100 },
        'headPaymentConditionText'
      ),
      headInterestPaymentPercent: new SoeTextFormControl(
        element?.headInterestPaymentPercent || '',
        { maxLength: 100 },
        'headInterestPaymentPercent'
      ),
      headInterestPaymentText: new SoeTextFormControl(
        element?.headInterestPaymentText || '',
        { maxLength: 100 },
        'headInterestPaymentText'
      ),
      headInvoiceGrossAmount: new SoeTextFormControl(
        element?.headInvoiceGrossAmount || '',
        { maxLength: 100 },
        'headInvoiceGrossAmount'
      ),
      headInvoiceNetAmount: new SoeTextFormControl(
        element?.headInvoiceNetAmount || '',
        { maxLength: 100 },
        'headInvoiceNetAmount'
      ),
      headVatAmount: new SoeTextFormControl(
        element?.headVatAmount || '',
        { maxLength: 100 },
        'headVatAmount'
      ),
      headVatBasisAmount: new SoeTextFormControl(
        element?.headVatBasisAmount || '',
        { maxLength: 100 },
        'headVatBasisAmount'
      ),
      headFreightFeeAmount: new SoeTextFormControl(
        element?.headFreightFeeAmount || '',
        { maxLength: 100 },
        'headFreightFeeAmount'
      ),
      headHandlingChargeFeeAmount: new SoeTextFormControl(
        element?.headHandlingChargeFeeAmount || '',
        { maxLength: 100 },
        'headHandlingChargeFeeAmount'
      ),
      headInsuranceFeeAmount: new SoeTextFormControl(
        element?.headInsuranceFeeAmount || '',
        { maxLength: 100 },
        'headInsuranceFeeAmount'
      ),
      headRemainingFeeAmount: new SoeTextFormControl(
        element?.headRemainingFeeAmount || '',
        { maxLength: 100 },
        'headRemainingFeeAmount'
      ),
      headDiscountAmount: new SoeTextFormControl(
        element?.headDiscountAmount || '',
        { maxLength: 100 },
        'headDiscountAmount'
      ),
      headRoundingAmount: new SoeTextFormControl(
        element?.headRoundingAmount || '',
        { maxLength: 100 },
        'headRoundingAmount'
      ),
      headBonusAmount: new SoeTextFormControl(
        element?.headBonusAmount || '',
        { maxLength: 100 },
        'headBonusAmount'
      ),
      headInvoiceArrival: new SoeTextFormControl(
        element?.headInvoiceArrival || '',
        { maxLength: 100 },
        'headInvoiceArrival'
      ),
      headInvoiceAuthorized: new SoeTextFormControl(
        element?.headInvoiceAuthorized || '',
        { maxLength: 100 },
        'headInvoiceAuthorized'
      ),
      headInvoiceAuthorizedBy: new SoeTextFormControl(
        element?.headInvoiceAuthorizedBy || '',
        { maxLength: 100 },
        'headInvoiceAuthorizedBy'
      ),
      sellerId: new SoeTextFormControl(
        element?.sellerId || '',
        { maxLength: 100 },
        'sellerId'
      ),
      sellerOrganisationNumber: new SoeTextFormControl(
        element?.sellerOrganisationNumber || '',
        { maxLength: 100 },
        'sellerOrganisationNumber'
      ),
      sellerVatNumber: new SoeTextFormControl(
        element?.sellerVatNumber || '',
        { maxLength: 100 },
        'sellerVatNumber'
      ),
      sellerName: new SoeTextFormControl(
        element?.sellerName || '',
        { maxLength: 100, isNameField: true },
        'sellerName'
      ),
      sellerAddress: new SoeTextFormControl(
        element?.sellerAddress || '',
        { maxLength: 100 },
        'sellerAddress'
      ),
      sellerPostalCode: new SoeTextFormControl(
        element?.sellerPostalCode || '',
        { maxLength: 100 },
        'sellerPostalCode'
      ),
      sellerPostalAddress: new SoeTextFormControl(
        element?.sellerPostalAddress || '',
        { maxLength: 100 },
        'sellerPostalAddress'
      ),
      sellerCountryCode: new SoeTextFormControl(
        element?.sellerCountryCode || '',
        { maxLength: 100 },
        'sellerCountryCode'
      ),
      sellerPhone: new SoeTextFormControl(
        element?.sellerPhone || '',
        { maxLength: 100 },
        'sellerPhone'
      ),
      sellerFax: new SoeTextFormControl(
        element?.sellerFax || '',
        { maxLength: 100 },
        'sellerFax'
      ),
      sellerReference: new SoeTextFormControl(
        element?.sellerReference || '',
        { maxLength: 100 },
        'sellerReference'
      ),
      sellerReferencePhone: new SoeTextFormControl(
        element?.sellerReferencePhone || '',
        { maxLength: 100 },
        'sellerReferencePhone'
      ),
      buyerId: new SoeTextFormControl(
        element?.buyerId || '',
        { maxLength: 100 },
        'buyerId'
      ),
      buyerOrganisationNumber: new SoeTextFormControl(
        element?.buyerOrganisationNumber || '',
        { maxLength: 100 },
        'buyerOrganisationNumber'
      ),
      buyerVatNumber: new SoeTextFormControl(
        element?.buyerVatNumber || '',
        { maxLength: 100 },
        'buyerVatNumber'
      ),
      buyerName: new SoeTextFormControl(
        element?.buyerName || '',
        { maxLength: 100 },
        'buyerName'
      ),
      buyerAddress: new SoeTextFormControl(
        element?.buyerAddress || '',
        { maxLength: 100 },
        'buyerAddress'
      ),
      buyerPostalCode: new SoeTextFormControl(
        element?.buyerPostalCode || '',
        { maxLength: 100 },
        'buyerPostalCode'
      ),
      buyerPostalAddress: new SoeTextFormControl(
        element?.buyerPostalAddress || '',
        { maxLength: 100 },
        'buyerPostalAddress'
      ),
      buyerCountryCode: new SoeTextFormControl(
        element?.buyerCountryCode || '',
        { maxLength: 100 },
        'buyerCountryCode'
      ),
      buyerReference: new SoeTextFormControl(
        element?.buyerReference || '',
        { maxLength: 100 },
        'buyerReference'
      ),
      buyerPhone: new SoeTextFormControl(
        element?.buyerPhone || '',
        { maxLength: 100 },
        'buyerPhone'
      ),
      buyerFax: new SoeTextFormControl(
        element?.buyerFax || '',
        { maxLength: 100 },
        'buyerFax'
      ),
      buyerEmailAddress: new SoeTextFormControl(
        element?.buyerEmailAddress || '',
        { maxLength: 100 },
        'buyerEmailAddress'
      ),
      buyerDeliveryName: new SoeTextFormControl(
        element?.buyerDeliveryName || '',
        { maxLength: 100 },
        'buyerDeliveryName'
      ),
      buyerDeliveryCoAddress: new SoeTextFormControl(
        element?.buyerDeliveryCoAddress || '',
        { maxLength: 100 },
        'buyerDeliveryCoAddress'
      ),
      buyerDeliveryAddress: new SoeTextFormControl(
        element?.buyerDeliveryAddress || '',
        { maxLength: 100 },
        'buyerDeliveryAddress'
      ),
      buyerDeliveryPostalCode: new SoeTextFormControl(
        element?.buyerDeliveryPostalCode || '',
        { maxLength: 100 },
        'buyerDeliveryPostalCode'
      ),
      buyerDeliveryPostalAddress: new SoeTextFormControl(
        element?.buyerDeliveryPostalAddress || '',
        { maxLength: 100 },
        'buyerDeliveryPostalAddress'
      ),
      buyerDeliveryCountryCode: new SoeTextFormControl(
        element?.buyerDeliveryCountryCode || '',
        { maxLength: 100 },
        'buyerDeliveryCountryCode'
      ),
      buyerDeliveryNoteText: new SoeTextFormControl(
        element?.buyerDeliveryNoteText || '',
        { maxLength: 100 },
        'buyerDeliveryNoteText'
      ),
      buyerDeliveryGoodsMarking: new SoeTextFormControl(
        element?.buyerDeliveryGoodsMarking || '',
        { maxLength: 100 },
        'buyerDeliveryGoodsMarking'
      ),
      sysEdiEdiMessageRowDTOs: new FormArray<SysEdiMessageHeadRowForm>([]),
    });
    this.sysEdiMessageHeadValidationHandler = validationHandler;
  }

  get sysEdiMessageHeadId(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.sysEdiMessageHeadId;
  }

  get sysEdiMessageHeadGuid(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.sysEdiMessageHeadGuid;
  }

  get sysEdiMessageHeadStatus(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.sysEdiMessageHeadStatus;
  }
  get sysEdiMessageRawId(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.sysEdiMessageRawId;
  }

  get ediSourceType(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.ediSourceType;
  }

  get sysEdiType(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.sysEdiType;
  }
  get sysEdiMsgId(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.sysEdiMsgId;
  }

  get sysWholesellerId(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.sysWholesellerId;
  }

  get messageSenderId(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.messageSenderId;
  }
  get messageType(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.messageType;
  }

  get messageDate(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.messageDate;
  }

  get headInvoiceNumber(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.headInvoiceNumber;
  }
  get headInvoiceOcr(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.headInvoiceOcr;
  }

  get headInvoiceType(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.headInvoiceType;
  }

  get headInvoiceDate(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.headInvoiceDate;
  }
  get headInvoiceDueDate(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.headInvoiceDueDate;
  }

  get headDeliveryDate(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.headDeliveryDate;
  }

  get headBuyerOrderNumber(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.headBuyerOrderNumber;
  }
  get headBuyerProjectNumber(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.headBuyerProjectNumber;
  }

  get headBuyerInstallationNumber(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.headBuyerInstallationNumber;
  }

  get headSellerOrderNumber(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.headSellerOrderNumber;
  }
  get headPostalGiro(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.headPostalGiro;
  }

  get headBankGiro(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.headBankGiro;
  }

  get headBank(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.headBank;
  }
  get headBicAddress(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.headBicAddress;
  }

  get headIbanNumber(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.headIbanNumber;
  }

  get headCurrencyCode(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.headCurrencyCode;
  }
  get headVatPercentage(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.headVatPercentage;
  }

  get headPaymentConditionDays(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.headPaymentConditionDays;
  }

  get headPaymentConditionText(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.headPaymentConditionText;
  }
  get headInterestPaymentPercent(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.headInterestPaymentPercent;
  }

  get headInvoiceGrossAmount(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.headInvoiceGrossAmount;
  }

  get headInvoiceNetAmount(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.headInvoiceNetAmount;
  }
  get headVatBasisAmount(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.headVatBasisAmount;
  }

  get headVatAmount(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.headVatAmount;
  }

  get headFreightFeeAmount(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.headFreightFeeAmount;
  }
  get headHandlingChargeFeeAmount(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.headHandlingChargeFeeAmount;
  }

  get headInsuranceFeeAmount(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.headInsuranceFeeAmount;
  }

  get headRemainingFeeAmount(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.headRemainingFeeAmount;
  }
  get headDiscountAmount(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.headDiscountAmount;
  }

  get headRoundingAmount(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.headRoundingAmount;
  }

  get headBonusAmount(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.headBonusAmount;
  }
  get headInvoiceArrival(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.headInvoiceArrival;
  }

  get headInvoiceAuthorized(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.headInvoiceAuthorized;
  }

  get headInvoiceAuthorizedBy(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.headInvoiceAuthorizedBy;
  }
  get sellerId(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.sellerId;
  }

  get sellerOrganisationNumber(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.sellerOrganisationNumber;
  }

  get sellerVatNumber(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.sellerVatNumber;
  }
  get sellerName(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.sellerName;
  }

  get sellerAddress(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.sellerAddress;
  }

  get sellerPostalCode(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.sellerPostalCode;
  }
  get sellerPostalAddress(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.sellerPostalAddress;
  }

  get sellerCountryCode(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.sellerCountryCode;
  }

  get sellerPhone(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.sellerPhone;
  }
  get sellerFax(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.sellerFax;
  }

  get sellerReference(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.sellerReference;
  }

  get sellerReferencePhone(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.sellerReferencePhone;
  }
  get buyerId(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.buyerId;
  }

  get buyerOrganisationNumber(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.buyerOrganisationNumber;
  }

  get buyerVatNumber(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.buyerVatNumber;
  }
  get buyerName(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.buyerName;
  }

  get buyerAddress(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.buyerAddress;
  }

  get buyerPostalCode(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.buyerPostalCode;
  }
  get buyerPostalAddress(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.buyerPostalAddress;
  }

  get buyerCountryCode(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.buyerCountryCode;
  }

  get buyerReference(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.buyerReference;
  }
  get buyerPhone(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.buyerPhone;
  }

  get buyerFax(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.buyerFax;
  }

  get buyerEmailAddress(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.buyerEmailAddress;
  }
  get buyerDeliveryName(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.buyerDeliveryName;
  }

  get buyerDeliveryCoAddress(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.buyerDeliveryCoAddress;
  }

  get buyerDeliveryPostalCode(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.buyerDeliveryPostalCode;
  }
  get buyerDeliveryPostalAddress(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.buyerDeliveryPostalAddress;
  }
  get buyerDeliveryCountryCode(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.buyerDeliveryCountryCode;
  }
  get buyerDeliveryNoteText(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.buyerDeliveryNoteText;
  }
  get buyerDeliveryGoodsMarking(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.buyerDeliveryGoodsMarking;
  }

  get sysEdiEdiMessageRowDTOs(): FormArray<SysEdiMessageHeadRowForm> {
    return <FormArray>this.controls.sysEdiEdiMessageRowDTOs;
  }

  setDirtyOnEdiMessageRowChange(rowId: number) {
    this.markAsDirty();
  }

  customPatchValue(element: SysEdiMessageHeadDTO) {
    (this.controls.sysEdiEdiMessageRowDTOs as FormArray).clear();

    for (const sysEdiEdiMessageRow of element.sysEdiEdiMessageRowDTOs) {
      const row = new SysEdiMessageHeadRowForm({
        validationHandler: this.sysEdiMessageHeadValidationHandler,
        element: sysEdiEdiMessageRow,
      });
      (this.controls.sysEdiEdiMessageRowDTOs as FormArray).push(row);
    }
  }
}
