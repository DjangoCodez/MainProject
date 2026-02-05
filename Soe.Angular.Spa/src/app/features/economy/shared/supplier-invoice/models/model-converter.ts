import { SupplierInvoicesArrivalHallDTO } from '@features/economy/supplier-invoices-arrival-hall/models/supplier-invoices-arrival-hall.model';
import { InvoiceImageFile, SupplierInvoiceDTO } from './supplier-invoice.model';
import { ISupplierInvoiceInterpretationDTO } from '@shared/models/generated-interfaces/SupplierInvoiceInterpretationDTO';
import {
  SoeDataStorageRecordType,
  TermGroup_BillingType,
  TermGroup_ScanningInterpretation,
  TermGroup_SupplierInvoiceSource,
  TermGroup_SupplierInvoiceStatus,
} from '@shared/models/generated-interfaces/Enumerations';
import { InvoiceFieldClasses } from './utility-models';
import { IEdiEntryDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { IGenericImageDTO } from '@shared/models/generated-interfaces/GenericImageDTO';
import { SupplierEditInputParameters } from '@features/economy/suppliers/models/edit-parameters.model';

/***
 * Utility functions to convert between different DTOs.
 * We don't want to clutter the components with these functions.
 */

export const arrivalHallToInvoiceDTO = (
  dto: SupplierInvoiceDTO | SupplierInvoicesArrivalHallDTO | undefined
): SupplierInvoiceDTO => {
  if (!dto) return new SupplierInvoiceDTO();
  if (isSupplierInvoiceDTO(dto)) return dto;
  return {
    ...new SupplierInvoiceDTO(),
    ...dto,
    source: dto.invoiceSource,
    actorId: dto.supplierId,
    status: dto.invoiceState,
    billingType: dto.billingTypeId,
    blockPayment: dto.blockPayment,
  };
};

export const finvoiceEdiEntryToInvoiceDTO = (dto: IEdiEntryDTO) => {
  const invoiceDTO = {
    ...new SupplierInvoiceDTO(),
    source: TermGroup_SupplierInvoiceSource.FInvoice,
    status: TermGroup_SupplierInvoiceStatus.New,
    ediEntryId: dto.ediEntryId,
    actorId: dto.actorSupplierId,
    invoiceNr: dto.invoiceNr,
    ocr: dto.ocr,
    invoiceDate: dto.invoiceDate,
    dueDate: dto.dueDate,
    billingType: dto.billingType,
    totalAmount: dto.sum,
    totalAmountCurrency: dto.sumCurrency,
    vatAmount: dto.sumVat,
    vatAmountCurrency: dto.sumVatCurrency,
    currencyRate: dto.currencyRate,
    paymentNr: dto.iban,
  };

  if (dto.vatRate && dto?.vatRate > 0) {
    //Set vat rate?
  }

  if (dto.currencyId) {
    invoiceDTO.currencyDate = dto.currencyDate;
    invoiceDTO.currencyId = dto.currencyId;
    invoiceDTO.currencyRate = dto.currencyRate;
  }
  return invoiceDTO as SupplierInvoiceDTO;
};

export const scanningToSupplierData = (
  dto: ISupplierInvoiceInterpretationDTO
): SupplierEditInputParameters => {
  return {
    ...new SupplierEditInputParameters(),
    orgNumber: dto.orgNumber.value || undefined,
    bankAccounts: {
      iban: dto.bankAccountIBAN.value || undefined,
      bg: dto.bankAccountBG.value || undefined,
      pg: dto.bankAccountPG.value || undefined,
    },
  };
};

export const scanningToInvoiceDTO = (
  dto: ISupplierInvoiceInterpretationDTO
): SupplierInvoiceDTO => {
  const invoiceDTO = {
    ...new SupplierInvoiceDTO(),
    source: TermGroup_SupplierInvoiceSource.Interpreted,
    status: TermGroup_SupplierInvoiceStatus.New,
    ediEntryId: dto.context?.ediEntryId,
    scanningEntryId: dto.context.scanningEntryId,
    actorId: dto.supplierId.value,
    invoiceNr: dto.invoiceNumber.value,
    ocr: dto.paymentReferenceNumber.value,
    referenceOur: dto.buyerReference.value,
    referenceYour: dto.sellerContactName.value,
    invoiceDate: dto.invoiceDate.value,
    accountingDate: dto.invoiceDate.value,
    dueDate: dto.dueDate.value,
    billingType: dto.isCreditInvoice.value
      ? TermGroup_BillingType.Credit
      : TermGroup_BillingType.Debit,
    totalAmount: dto.amountIncVat.value,
    totalAmountCurrency: dto.amountIncVatCurrency.value,
    vatAmount: dto.vatAmount.value,
    vatAmountCurrency: dto.vatAmountCurrency.value,
    currencyId: dto.currencyId.value,
    currencyRate: dto.currencyRate.value,
    currencyDate: dto.currencyDate.value,
    orderNr: Number(dto.buyerOrderNumber.value) || undefined,
  };

  if (invoiceDTO.scanningEntryId) {
    const vatRate = dto.vatRatePercent;
    if (
      vatRate.hasValue &&
      (vatRate.value > 0 ||
        vatRate.confidenceLevel ===
          TermGroup_ScanningInterpretation.ValueIsValid)
    ) {
      invoiceDTO.vatCodeId = vatRate.value;
    }
  }
  return invoiceDTO as SupplierInvoiceDTO;
};

export const scanningToInterpretationClasses = (
  dto: ISupplierInvoiceInterpretationDTO
) => {
  const classes: InvoiceFieldClasses = {};
  for (const key in dto) {
    const field = (dto as any)[key];

    if (field.hasValue) {
      classes[key] = {
        'value-is-valid':
          field.confidenceLevel ===
          TermGroup_ScanningInterpretation.ValueIsValid,
        'value-is-unsettled':
          field.confidenceLevel ===
          TermGroup_ScanningInterpretation.ValueIsUnsettled,
        'value-is-not-found':
          field.confidenceLevel ===
          TermGroup_ScanningInterpretation.ValueNotFound,
      };
    }
  }
  return classes;
};

const isSupplierInvoiceDTO = (
  dto: SupplierInvoiceDTO | SupplierInvoicesArrivalHallDTO
): dto is SupplierInvoiceDTO => {
  return (dto as SupplierInvoiceDTO)?.invoiceId > 0;
};

export const toSimpleInvoiceFileDTO = (
  file: IGenericImageDTO
): InvoiceImageFile => {
  return {
    dataStorageRecordId: file.id || undefined,
    fileName: file.filename,
    data: file.image as unknown as string, // It's a base64 string
    extension: getFileExtension(file),
  };
};

const getFileExtension = (file: IGenericImageDTO) => {
  const fileName = file.filename;
  const lastDotIndex = fileName?.lastIndexOf('.');
  if (fileName && lastDotIndex !== -1) {
    return fileName.substring(lastDotIndex).toLowerCase(); // Return the extension in lowercase
  }

  if (lastDotIndex === -1) {
    switch (file.imageFormatType) {
      case SoeDataStorageRecordType.InvoicePdf:
        return '.pdf';
      case SoeDataStorageRecordType.InvoiceBitmap:
        return '.jpeg';
    }
  }
  return '';
};
