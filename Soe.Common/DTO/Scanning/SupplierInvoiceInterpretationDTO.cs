using SoftOne.Soe.Common.Attributes;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Common.DTO.Scanning
{
    public class ReceiptInterpretationDTO
    {
        //...
    }
    public class VoucherInterpretationDTO
    {
        //...
    }
    [TSInclude]
    public class SupplierInvoiceInterpretationDTO
    {
        public InterpretationValueDTO<bool?> IsCreditInvoice { get; set; }
        public InterpretationValueDTO<int?> SupplierId { get; set; }
        public InterpretationValueDTO<string> SupplierName { get; set; }
        public InterpretationValueDTO<string> InvoiceNumber { get; set; }
        public InterpretationValueDTO<DateTime?> InvoiceDate { get; set; }
        public InterpretationValueDTO<DateTime?> DueDate { get; set; }
        public InterpretationValueDTO<string> Description { get; set; }
        public InterpretationValueDTO<string> CurrencyCode { get; set; }
        public InterpretationValueDTO<int?> CurrencyId { get; set; }
        public InterpretationValueDTO<DateTime?> CurrencyDate { get; set; } 
        public InterpretationValueDTO<decimal?> CurrencyRate { get; set; }
        public InterpretationValueDTO<string> BuyerOrderNumber { get; set; }
        public InterpretationValueDTO<string> BuyerContactName { get; set; }
        public InterpretationValueDTO<string> BuyerReference { get; set; }
        public InterpretationValueDTO<string> SellerContactName { get; set; }
        public InterpretationValueDTO<decimal?> AmountExVat { get; set; }
        public InterpretationValueDTO<decimal?> AmountExVatCurrency { get; set; }
        public InterpretationValueDTO<decimal?> AmountIncVat { get; set; }
        public InterpretationValueDTO<decimal?> AmountIncVatCurrency { get; set; }
        public InterpretationValueDTO<decimal?> VatRatePercent { get; set; }
        public InterpretationValueDTO<decimal?> VatAmount { get; set; }
        public InterpretationValueDTO<decimal?> VatAmountCurrency { get; set; }
        public InterpretationValueDTO<string> PaymentReferenceNumber { get; set; }
        public InterpretationValueDTO<string> BankAccountPG { get; set; }
        public InterpretationValueDTO<string> BankAccountBG { get; set; }
        public InterpretationValueDTO<string> BankAccountIBAN { get; set; }
        public InterpretationValueDTO<string> OrgNumber { get; set; }
        public InterpretationValueDTO<string> VATRegistrationNumber { get; set; }
        public InterpretationValueDTO<decimal?> DeliveryCost { get; set; }
        public InterpretationValueDTO<decimal?> AmountRounding { get; set; }
        public InterpretationValueDTO<string> Email { get; set; }
        public InterpretationValueDTO<List<InterpretationAccountingRowDTO>> AccountingRows { get; set; }
        public MetadataDTO Metadata { get; set; }
        public ContextDTO Context { get; set; }

        public SupplierInvoiceInterpretationDTO() { }

        public SupplierInvoiceInterpretationDTO(int scanningEntryId, int ediEntryId)
        {
            Context = new ContextDTO { ScanningEntryId = scanningEntryId, EdiEntryId = ediEntryId };
            Metadata = new MetadataDTO { ArrivalTime = DateTime.UtcNow };
        }
    }

    public static class InterpretationValueFactory
    {
        #region Date
        public static InterpretationValueDTO<DateTime?> NoneInterpretedDate() => new InterpretationValueDTO<DateTime?>(null, TermGroup_ScanningInterpretation.ValueIsNotInterpreted);
        public static InterpretationValueDTO<DateTime?> DerivedDate(DateTime? value) => new InterpretationValueDTO<DateTime?>(value, TermGroup_ScanningInterpretation.ValueIsInterpretationDerived);
        public static InterpretationValueDTO<DateTime?> EmptyInterpretedDate() => new InterpretationValueDTO<DateTime?>(null, TermGroup_ScanningInterpretation.ValueNotFound);
        public static InterpretationValueDTO<DateTime?> InterpretedDate(DateTime? value, TermGroup_ScanningInterpretation confidenceLevel = TermGroup_ScanningInterpretation.ValueIsValid) {
            if (value == null)
                return EmptyInterpretedDate();

            return new InterpretationValueDTO<DateTime?>(value, confidenceLevel);
        }
        #endregion

        #region Decimal
        public static InterpretationValueDTO<decimal?> NoneInterpretedDecimal() => new InterpretationValueDTO<decimal?>(null, TermGroup_ScanningInterpretation.ValueIsNotInterpreted);
        public static InterpretationValueDTO<decimal?> EmptyInterpretedDecimal() => new InterpretationValueDTO<decimal?>(null, TermGroup_ScanningInterpretation.ValueNotFound);
        public static InterpretationValueDTO<decimal?> DerivedDecimal(decimal? value) => new InterpretationValueDTO<decimal?>(value, TermGroup_ScanningInterpretation.ValueIsInterpretationDerived);

        public static InterpretationValueDTO<decimal?> InterpretedDecimal(decimal? value)
        {
            if (value == null)
                return EmptyInterpretedDecimal();

            return new InterpretationValueDTO<decimal?>(value, TermGroup_ScanningInterpretation.ValueIsValid);
        }
        public static InterpretationValueDTO<decimal?> InterpretedDecimal(decimal? value, TermGroup_ScanningInterpretation confidence)
        {
            if (value == null)
                return EmptyInterpretedDecimal();

            return new InterpretationValueDTO<decimal?>(value, confidence);
        }
        #endregion

        #region String
        public static InterpretationValueDTO<string> NoneInterpretedString() => new InterpretationValueDTO<string>(null, TermGroup_ScanningInterpretation.ValueIsNotInterpreted);
        public static InterpretationValueDTO<string> EmptyInterpretedString() => new InterpretationValueDTO<string>(null, TermGroup_ScanningInterpretation.ValueNotFound);
        public static InterpretationValueDTO<string> InterpretedString(string value, TermGroup_ScanningInterpretation? confidenceLevel = TermGroup_ScanningInterpretation.ValueIsValid) {
            if (string.IsNullOrWhiteSpace(value) && confidenceLevel != TermGroup_ScanningInterpretation.ValueIsValid)
                return EmptyInterpretedString();

            return new InterpretationValueDTO<string>(value, confidenceLevel ?? TermGroup_ScanningInterpretation.ValueNotFound);
        }
        #endregion

        #region Int
        public static InterpretationValueDTO<int?> NoneInterpretedInt() => new InterpretationValueDTO<int?>(null, TermGroup_ScanningInterpretation.ValueIsNotInterpreted);
        public static InterpretationValueDTO<int?> EmptyInterpretedInt() => new InterpretationValueDTO<int?>(null, TermGroup_ScanningInterpretation.ValueNotFound);
        public static InterpretationValueDTO<int?> InterpretedInt(string value)
        {
            if (!int.TryParse(value, out int output))
                return EmptyInterpretedInt();

            return new InterpretationValueDTO<int?>(output.ToNullable(), TermGroup_ScanningInterpretation.ValueIsNotInterpreted);
        }
        public static InterpretationValueDTO<int?> InterpretedInt(int? value, TermGroup_ScanningInterpretation? confidence)
        {
            if (value == 0 || value == null)
                return EmptyInterpretedInt();

            return new InterpretationValueDTO<int?>(value, confidence ?? TermGroup_ScanningInterpretation.ValueNotFound);
        }
        #endregion

        #region Bool
        public static InterpretationValueDTO<bool?> NoneInterpretedBool() => new InterpretationValueDTO<bool?>(null, TermGroup_ScanningInterpretation.ValueIsNotInterpreted);
        public static InterpretationValueDTO<bool?> EmptyInterpretedBool() => new InterpretationValueDTO<bool?>(null, TermGroup_ScanningInterpretation.ValueNotFound);
        public static InterpretationValueDTO<bool?> DerivedBool(bool? value) => new InterpretationValueDTO<bool?>(value, TermGroup_ScanningInterpretation.ValueIsInterpretationDerived);

        public static InterpretationValueDTO<bool?> InterpretedBool(bool? value, TermGroup_ScanningInterpretation confidence)
        {
            if (value == null)
                return EmptyInterpretedBool();

            return new InterpretationValueDTO<bool?>(value, confidence);
        }
        #endregion

        #region Accounting
        public static InterpretationValueDTO<List<InterpretationAccountingRowDTO>> NoneInterpretedAccountingRows() => new InterpretationValueDTO<List<InterpretationAccountingRowDTO>>(null, TermGroup_ScanningInterpretation.ValueIsNotInterpreted);

        #endregion

        public static TermGroup_ScanningInterpretation GetLowest(TermGroup_ScanningInterpretation a, TermGroup_ScanningInterpretation b) => (TermGroup_ScanningInterpretation)Math.Min((int)a, (int)b);
        public static TermGroup_ScanningInterpretation GetHighest(TermGroup_ScanningInterpretation a, TermGroup_ScanningInterpretation b) => (TermGroup_ScanningInterpretation)Math.Max((int)a, (int)b);
    }
    [TSInclude]
    public class InterpretationValueDTO<T>
    {
        public T Value { get; set; }
        public bool HasValue => Value != null;
        public TermGroup_ScanningInterpretation ConfidenceLevel { get; set; }
        public InterpretationValueDTO(T value, TermGroup_ScanningInterpretation confidenceLevel) {
            Value = value;
            ConfidenceLevel = confidenceLevel;
        }
    }

    [TSInclude]
    public class InterpretationAccountingRowDTO
    {
        public string AccountNumber { get; set; }
        public string ProjectNumber { get; set; }
        public string CostCenterNumber { get; set; }
        public string CostBearerNumber { get; set; }
        public AccrualDTO Accrual { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }

    }
    [TSInclude]
    public class AccrualDTO
    {
        public string OffsetAccountNumber { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
    [TSInclude]
    public class MetadataDTO
    {
        public DateTime ArrivalTime { get; set; }
        public string Provider { get; set; }
        public string RawResponse { get; set; }
    }
    [TSInclude]
    public class ContextDTO
    {
        public int? ScanningEntryId { get; set; }
        public int? EdiEntryId { get; set; }
    }
}
