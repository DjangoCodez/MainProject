using RestSharp;
using SoftOne.Soe.Business.Util.API.AzoraOne.Models;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Business.Util.API.AzoraOne.Connectors
{
    public class FileConnector : AOBaseConnector
    {
        public FileConnector(Guid companyApiKey) : base(companyApiKey) { }
        public string GetEndpoint()
        {
            return $"{CompanyEndpoint}/files";
        }

        public AOResponseWrapper<AOFile> AddFile(string fileId, string fileName, byte[] fileContent, string webhookUrl = null)
        {
            var request = new RestRequest(GetEndpoint(), Method.Post);
            request.AddHeader("Content-Type", "multipart/form-data");
            request.AddFile("file", fileContent, fileName, WebUtil.GetContentType(fileName));
            request.AddParameter("fileId", fileId);
            if (!string.IsNullOrEmpty(webhookUrl))
                request.AddParameter("webhookUrl", webhookUrl);

            var response = this.Client.Execute(request);
            return HandleResponse<AOFile>(request, response);
        }
        public AOResponseWrapper<AOFile> GetFile(string fileId)
        {
            return Get<AOFile>($"{GetEndpoint()}/{fileId}");
        }
        public AOResponseWrapper<AOSupplierInvoice> ExtractSupplierInvoice(string fileId)
        {
            var endpoint = $"{GetEndpoint()}/{fileId}/supplierInvoices?extended=true";
            return Get<AOSupplierInvoice>(endpoint);
        }
        public AOResponseWrapper<AOSupplierInvoice> BookkeepSupplierInvoice(string fileId, AOSupplierInvoice invoice)
        {
            var result = Put<AOSupplierInvoice>($"{GetEndpoint()}/{fileId}/supplierInvoices", invoice);
            if (result.IsSuccess)
                return result;

            if (TryAdjustForBookkeepErrors(invoice, result.Error.Data))
                return Put<AOSupplierInvoice>($"{GetEndpoint()}/{fileId}/supplierInvoices", invoice);

            return result;
        }
        public AOResponseWrapper<AOReceipt> ExtractReceipt(string fileId)
        {
            var endpoint = $"{GetEndpoint()}/{fileId}/receipts?extended=true";
            return Get<AOReceipt>(endpoint);
        }
        public AOResponseWrapper<AOReceipt> BookkeepReceipt(string fileId, AOReceipt receipt)
        {
            var endpoint = $"{GetEndpoint()}/{fileId}/receipts";
            return Put<AOReceipt>(endpoint, receipt);
        }

        public bool TryAdjustForBookkeepErrors(AOSupplierInvoice invoice, List<AOErrorDetails> errors)
        {
            foreach (var error in errors)
            {
                switch ((ConnectorError)error.Code)
                {
                    case ConnectorError.Invoice_OcrInvalid:
                        invoice.OcrNumber = null;
                        break;
                    case ConnectorError.Invoice_DescriptionInvalid:
                        invoice.Description = null;
                        break;
                }
            }
            return true;
        }
    }

    public static class AOInvoiceExtensions
    {
        public static AOSupplierInvoice ToAOInvoice(this SupplierInvoiceDTO invoice, List<AccountingRowDTO> accountingRows)
        {
            var aoInvoice = new AOSupplierInvoice()
            {
                SupplierID = invoice.ActorId.ToString(),
                Description = !string.IsNullOrEmpty(invoice.OriginDescription) ? 
                    invoice.OriginDescription.SubstringToLengthOfString(0, 99) :
                    null,
                InvoiceDate = AzoraOneHelper.DateToString(invoice.InvoiceDate),
                DueDate = AzoraOneHelper.DateToString(invoice.DueDate),
                InvoiceNumber = invoice.InvoiceNr,
                OrderNumber = invoice.OrderNr?.ToString(),
                OcrNumber = invoice.OCR,
                ReferenceNumber = null,
                OurRef = invoice.ReferenceOur,
                YourRef = invoice.ReferenceYour,
                TotalSum = AzoraOneHelper.DecimalToString(invoice.TotalAmountCurrency),
                Vat = AzoraOneHelper.DecimalToString(invoice.VatAmountCurrency),
                Accounts = new List<AOAccountingRow>()
            };

            decimal total = 0;
            decimal accountedVat = 0;

            bool isCreditInvoice = invoice.TotalAmountCurrency < 0;
            bool foundDebtRow = false;
            foreach (var accountingRow in accountingRows)
            {
                /**
                 * AzoraOne only expects the COST accounting rows as they derive supplier debt and VAT from the invoice.
                 * Therefore, we need to remove all none-cost rows. VAT is simple as we have flags for that, the supplier debt account is not as straight forward. 
                 */

                // Remove VAT rows.
                if (accountingRow.IsVatRow || accountingRow.IsContractorVatRow)
                {
                    accountedVat += accountingRow.AmountCurrency;
                    continue;
                }

                // When debit invoice, the accounts payable is accounted on the credit side. And opposite for credit invoice.
                bool accountedAsSupplierDebt = (accountingRow.IsDebitRow && isCreditInvoice) || (accountingRow.IsCreditRow && !isCreditInvoice);

                /**
                 * We assume that the user only has one supplier debt account which should match the invoice's total.
                 * There are actual scenarios where there are more than one debt row. We don't handle that currently.
                 */
                bool rowMatchesDebtAmount = Math.Abs(invoice.TotalAmountCurrency) == Math.Abs(accountingRow.AmountCurrency);

                if (!foundDebtRow && accountedAsSupplierDebt && rowMatchesDebtAmount)
                {
                    foundDebtRow = true;
                    continue;
                }

                //TODO: add support for accruals.
                total += accountingRow.AmountCurrency;
                aoInvoice.Accounts.Add(accountingRow.ToAOAccountingRow());
            }

            if (!foundDebtRow)
            {
                /** 
                 * Here we have a problem, one of these scenarios probably applies:
                 * 1. The user has multiple supplier debt accounts
                 * 2. The debt accounting row diffs from the invoice total
                 */
            }

            // Some companies like to remove cents from VAT rows. If we don't handle that, AzoraOne will complain that Debit/Credit is not balancing.
            var vatCentDiff = invoice.VatAmountCurrency - accountedVat;
            decimal newVat = invoice.VatAmountCurrency - vatCentDiff;
            if (vatCentDiff != 0 && Math.Abs(vatCentDiff) < 1)
            {
                aoInvoice.Vat = AzoraOneHelper.DecimalToString(newVat);
            }

            return aoInvoice;
        }

        public static AOAccountingRow ToAOAccountingRow(this AccountingRowDTO accountingRow)
        {
            return new AOAccountingRow
            {
                Account = accountingRow.Dim1Nr,
                Amount = accountingRow.AmountCurrency, //Not sent to AzoraOne.
                Debit = AzoraOneHelper.DecimalToString(Math.Abs(accountingRow.DebitAmountCurrency)),
                Credit = AzoraOneHelper.DecimalToString(Math.Abs(accountingRow.CreditAmountCurrency)),
                CostBearer = accountingRow.Dim2Nr != null ?
                    new AOInternalAccount
                    {
                        TargetValue = accountingRow.Dim2Nr
                    } : null,
                Project = accountingRow.Dim3Nr != null ?
                    new AOInternalAccount
                    {
                        TargetValue = accountingRow.Dim3Nr
                    } : null,
                ResultsCentre = accountingRow.Dim4Nr != null ? 
                    new AOInternalAccount
                    {
                        TargetValue = accountingRow.Dim4Nr
                    } : null,
            };

        }
    }
}
