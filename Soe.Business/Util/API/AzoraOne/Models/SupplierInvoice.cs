using SoftOne.Soe.Common.DTO.Scanning;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Util.API.AzoraOne.Models
{
    public class AOSupplierInvoice
    {
        public string SupplierID { get; set; }
        public string Description { get; set; }
        public string VerificationSeries { get; set; }
        public string InvoiceDate { get; set; }
        public string DueDate { get; set; }
        public string InvoiceNumber { get; set; }
        public string OrderNumber { get; set; }
        public string OcrNumber { get; set; }
        public string ReferenceNumber { get; set; }
        public string OurRef { get; set; }
        public string YourRef { get; set; }
        public string TotalSum { get; set; }
        public string Vat { get; set; }
        public List<AOAccountingRow> Accounts { get; set; }
    }

    public class AOAccountingRow
    {
        public string Account { get; set; }
        public AOPeriodicity Periodicity { get; set; }
        public AOInternalAccount Project { get; set; }
        public AOInternalAccount CostBearer { get; set; }
        public AOInternalAccount ResultsCentre { get; set; }
        public decimal Amount { get; set; }
        public string Debit { get; set; }
        public string Credit { get; set; }
    }

    public class AOPeriodicity
    {
        public string OffsetAccount { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
    }

    public class AOInternalAccount
    {
        public string TargetValue { get; set; }
    }

    public class AOExtended
    {
        public List<string> Dates { get; set; }
        public List<string> Times { get; set; }
        public List<string> Emails { get; set; }
        public List<string> Currencies { get; set; }
        public List<string> CreditCardNumbers { get; set; }
        public List<string> CorporateIdentityNumbers { get; set; }
        public List<string> BankAccountNumbers { get; set; }
        public List<string> PlusGiroNumbers { get; set; }
        public List<string> Ibans { get; set; }
    }

    public static class AOSupplierInvoiceExtensions
    {
        //Most used payment conditions for supplier invoices in soecompv2 per 2024-12-20.
        public static List<int> MostUsedPaymentConditions = new List<int> { 30, 45, 10, 60, 20, 14, 29, 28, 31, 15, 32, 40 };
        public static SupplierInvoiceInterpretationDTO ToSupplierInvoiceInterpretationDTO(this AOResponse<AOSupplierInvoice> response, string rawResponse)
        {
            var aoInvoice = response.Data;
            var extended = response.Extended;

            var interpretation = new SupplierInvoiceInterpretationDTO();
            interpretation.Metadata = new MetadataDTO
            {
                ArrivalTime = AzoraOneHelper.StringToDateTime(response.Time).Value,
                RawResponse = rawResponse, 
                Provider = "AzoraOne",
            };

            interpretation.Context = new ContextDTO();
            //Header fields
            interpretation.InvoiceNumber = InterpretationValueFactory.InterpretedString(aoInvoice.InvoiceNumber);
            interpretation.SupplierId = InterpretationValueFactory.InterpretedInt(aoInvoice.SupplierID);
            interpretation.Description = InterpretationValueFactory.InterpretedString(aoInvoice.Description);

            //Dates
            var invoiceDate = AzoraOneHelper.StringToDate(aoInvoice.InvoiceDate);
            var dueDate = AzoraOneHelper.StringToDate(aoInvoice.DueDate);
            var dates = extended.Dates.Select(d => AzoraOneHelper.StringToDate(d).Value).ToList();

            if (invoiceDate != null)
                interpretation.InvoiceDate = InterpretationValueFactory.InterpretedDate(invoiceDate);
            else if (dueDate != null)
                interpretation.InvoiceDate = GuessInvoiceDateFromDueDate(dates, dueDate.Value);
            else
                interpretation.InvoiceDate = InterpretationValueFactory.EmptyInterpretedDate();

            if (dueDate != null)
                interpretation.DueDate = InterpretationValueFactory.InterpretedDate(dueDate);
            else if (invoiceDate != null)
                interpretation.DueDate = GuessDueDateFromInvoiceDate(dates, invoiceDate.Value);
            else
                interpretation.DueDate = InterpretationValueFactory.EmptyInterpretedDate();

            //References
            interpretation.PaymentReferenceNumber = InterpretationValueFactory.InterpretedString(aoInvoice.OcrNumber);
            interpretation.SellerContactName = InterpretationValueFactory.InterpretedString(aoInvoice.YourRef);
            interpretation.BuyerContactName = InterpretationValueFactory.InterpretedString(aoInvoice.OurRef);
            interpretation.BuyerOrderNumber = InterpretationValueFactory.InterpretedString(aoInvoice.OrderNumber);
            interpretation.BuyerReference = InterpretationValueFactory.InterpretedString(aoInvoice.ReferenceNumber);

            //Amounts
            var totalSum = AzoraOneHelper.StringToDecimal(aoInvoice.TotalSum);
            var vat = AzoraOneHelper.StringToDecimal(aoInvoice.Vat);
            var amountExVat = (totalSum ?? 0) - (vat ?? 0);
            var isCredit = totalSum < 0;
            var vatRate = amountExVat == 0 ? null : (vat / amountExVat) * 100;
            interpretation.IsCreditInvoice = InterpretationValueFactory.DerivedBool(isCredit);
            interpretation.AmountIncVatCurrency = InterpretationValueFactory.InterpretedDecimal(totalSum);
            interpretation.VatAmountCurrency = InterpretationValueFactory.InterpretedDecimal(vat);
            interpretation.AmountExVatCurrency = InterpretationValueFactory.DerivedDecimal(amountExVat);
            interpretation.VatRatePercent = InterpretationValueFactory.DerivedDecimal(vatRate);

            //First match
            interpretation.CurrencyCode = GetFromList(extended.Currencies);
            interpretation.BankAccountBG = GetFromList(extended.BankAccountNumbers);
            interpretation.BankAccountIBAN = GetFromList(extended.Ibans);
            interpretation.BankAccountPG = GetFromList(extended.PlusGiroNumbers);
            interpretation.Email = GetFromList(extended.Emails);
            interpretation.OrgNumber = GetFromList(extended.CorporateIdentityNumbers);
            
            //None handled
            interpretation.DeliveryCost = InterpretationValueFactory.NoneInterpretedDecimal();
            interpretation.AmountRounding = InterpretationValueFactory.NoneInterpretedDecimal();

            //Metadata
            interpretation.SupplierName = InterpretationValueFactory.NoneInterpretedString();

            //Accounting rows
            interpretation.AccountingRows = InterpretationValueFactory.NoneInterpretedAccountingRows();

            return interpretation;
        }

        public static InterpretationValueDTO<string> GetFromList(List<string> options)
        {
            if (options.IsNullOrEmpty())
                return InterpretationValueFactory.EmptyInterpretedString();

            return InterpretationValueFactory.InterpretedString(options.First(), TermGroup_ScanningInterpretation.ValueIsUnsettled);
        }

        public static InterpretationValueDTO<DateTime?> GuessInvoiceDateFromDueDate(List<DateTime> candidates, DateTime dueDate)
        {
            return GuessDate(candidates, dueDate, false);
        }

        public static InterpretationValueDTO<DateTime?> GuessDueDateFromInvoiceDate(List<DateTime> candidates, DateTime invoiceDate)
        {
            return GuessDate(candidates, invoiceDate, true);
        }

        public static InterpretationValueDTO<DateTime?> GuessDate(List<DateTime> candidates, DateTime guessBase, bool findGreaterThan)
        {

            var limitedCandidates = candidates
                .Where(d => findGreaterThan ? d > guessBase : d < guessBase)
                .ToList();

            if (limitedCandidates.IsNullOrEmpty())
                return InterpretationValueFactory.EmptyInterpretedDate();

            foreach (var dayDiff in MostUsedPaymentConditions)
            {
                var guess = findGreaterThan ? guessBase.AddDays(dayDiff) : guessBase.AddDays(-dayDiff);
                if (limitedCandidates.Contains(guess))
                    return InterpretationValueFactory.InterpretedDate(guess, TermGroup_ScanningInterpretation.ValueIsUnsettled);
            }

            return InterpretationValueFactory.InterpretedDate(limitedCandidates.First(), TermGroup_ScanningInterpretation.ValueIsUnsettled);
        }
    }
}
