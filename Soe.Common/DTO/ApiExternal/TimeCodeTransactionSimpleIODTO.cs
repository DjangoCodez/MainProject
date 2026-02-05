using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.Common.DTO.ApiExternal
{
    public class TimeCodeTransactionSimpleIODTO
    {
        /// <summary>
        /// The code and key of the TimeCode
        /// </summary>
        public string TimeCodeCode { get; set; }
        /// <summary>
        /// Name of connected TimeCode (readonly)
        /// </summary>BalanceItemDTO
        public string TimeCodeName { get; set; }
        /// <summary>
        /// Connected Project if any.
        /// </summary>
        public string ProjectNr { get; set; }
        /// <summary>
        /// Connected EmployeeNr (key) is any
        /// </summary>
        public string EmployeeNr { get; set; }
        /// <summary>
        /// Type of TimeCode (readonly)
        ///     None = 0,
        ///     Work = 1,
        ///     Absense = 2,
        ///     Break = 3,
        ///     Material = 8,
        ///     AdditionDeduction = 9,
        ///     WorkAndAbsense = 101,
        ///     WorkAndAbsenseAndAdditionDeduction = 102,
        ///     WorkAndMaterial = 103,
        ///     AdditionAndDeduction = 104,        
        /// </summary>
        public SoeTimeCodeType Type { get; set; }
        /// <summary>
        /// Amount
        /// </summary>
        public decimal Amount { get; set; }
        /// <summary>
        /// Amount in the currency 
        /// </summary>
        public decimal AmountCurrency { get; set; }
        /// <summary>
        /// Amount recalculated to company main currency
        /// </summary>
        public decimal AmountEntCurrency { get; set; }
        /// <summary>
        /// Amount for ledger currency
        /// </summary>
        public decimal AmountLedgerCurrency { get; set; }
        /// <summary>
        /// Vat amount on transaction
        /// </summary>
        public decimal Vat { get; set; }
        /// <summary>
        /// Vat amount in the currency 
        /// </summary>
        public decimal VatCurrency { get; set; }
        /// <summary>
        /// Vatamount recalculated to company main currency
        /// </summary>
        public decimal VatEntCurrency { get; set; }
        /// <summary>
        /// Vatamount for ledger currency
        /// </summary>
        public decimal VatLedgerCurrency { get; set; }
        /// <summary>
        /// Quantity (if the transaction is time, add time in minutes)
        /// </summary>
        public decimal Quantity { get; set; }
        /// <summary>
        /// Quantity for billing (if the transaction is time, add time in minutes)
        /// </summary>
        public decimal InvoiceQuantity { get; set; }
        /// <summary>
        /// Time when transactions starts
        /// </summary>
        public DateTime Start { get; set; }
        /// <summary>
        /// Time when transactions ends (Set same as start if there is no time interval)
        /// </summary>
        public DateTime Stop { get; set; }
        /// <summary>
        /// Comment on transaction
        /// </summary>
        public string Comment { get; set; }
        /// <summary>
        /// Comment that can be visible to customer if the transaction is billed
        /// </summary>
        public string ExternalComment { get; set; }
        /// <summary>
        /// Main account nr, for example 7010
        /// </summary>
        public string AccountNr { get; set; }
        /// <summary>
        /// CostCenter
        /// </summary>
        public string AccountNrSieDim1 { get; set; }
        /// <summary>
        /// CostUnit
        /// </summary>
        public string AccountNrSieDim2 { get; set; }
        /// <summary>
        /// Project
        /// </summary>
        public string AccountNrSieDim6 { get; set; }
        /// <summary>
        /// Employee
        /// </summary>
        public string AccountNrSieDim7 { get; set; }
        /// <summary>
        /// Customer
        /// </summary>
        public string AccountNrSieDim8 { get; set; }
        /// <summary>
        /// Supplier
        /// </summary>
        public string AccountNrSieDim9 { get; set; }
        /// <summary>
        /// Invoice
        /// </summary>
        public string AccountNrSieDim10 { get; set; }
        /// <summary>
        /// Region
        /// </summary>
        public string AccountNrSieDim30 { get; set; }
        /// <summary>
        /// Shop
        /// </summary>
        public string AccountNrSieDim40 { get; set; }
        /// <summary>
        /// Department
        /// </summary>
        public string AccountNrSieDim50 { get; set; }
    }
}
