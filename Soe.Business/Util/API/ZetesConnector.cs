using Newtonsoft.Json;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using SoftOne.Soe.Business.Util.API.OAuth;
using SoftOne.Soe.Common.DTO;
using System.IO;
using System.Linq;
using SoftOne.Soe.Data;

namespace SoftOne.Soe.Business.Util.API.Zetes
{
    public class ZetesConnector : OAuthConnectorBase
    {
        private const string TestURL = "https://olympus-tpd-qa-api.azurewebsites.net";
        private const string ProdURL = "https://olympus-tpd-prod-api.azurewebsites.net";
        private const string TestLoginURL = "https://olympus-tpd-qa-id.azurewebsites.net";
        private const string ProdLoginURL = "https://olympus-tpd-prod-id.azurewebsites.net/";
        

        //To make call of API’s method you have to get your token first.The token’s life span is 50 minutes, after that time it expires.
        //

        public OAuthLoginConfiguration GetTestOauthConfig(SoftoneZetesConfiguration zetesConfig, bool testMode)
        {
            return new OAuthLoginConfiguration
            {
                ClientID = "TracehubLevel3",
                ClientSecret = "TracehubApiSecret",
                Username = zetesConfig.Username,
                Password = zetesConfig.Password,
                Scope = "tracehub.api",
                GrantType = "password",
                LoginHost = GetLoginUrl(testMode),
                LoginPath = "/connect/token"
            };
        }

        #region Zetes payment
        public ActionResult SendPayment(bool testMode, SoftoneZetesConfiguration zetesConfig, CustomerInvoiceDistributionDTO invoice, CustomerDistributionDTO customerAddresses, List<string> connectedOrders, List<CustomerInvoiceRowSmallDTO> tpdInvoiceRows,bool paymentMatchesInvoice)
        {
            
            var message = "";
            
            if (string.IsNullOrEmpty(CurrentToken))
            {
                message = GetLoginToken(GetTestOauthConfig(zetesConfig, testMode));
            }

            if (string.IsNullOrEmpty(CurrentToken))
            {
                return new ActionResult("Failed getting Zetes login token: " + message);
            }
            
            var zetespayment = CreateZetesPayment(zetesConfig, invoice, customerAddresses, connectedOrders, tpdInvoiceRows, true);
            var json = JsonConvert.SerializeObject(zetespayment, Formatting.Indented);
#if DEBUG
            File.WriteAllText(@"C:\Temp\zetes_payment.json", json);
#endif

            var result = SendObject(GetUrl(testMode), "/api/v1.0/tracehub/level3/submitPayment", json);

            if (!string.IsNullOrEmpty(result.StringValue))
            {
                var responseContent = JsonConvert.DeserializeObject<ZetesResponse>(result.StringValue);

                if (responseContent.ResponseResult.Errors.Any())
                {
                    var messages = responseContent.ResponseResult.Errors.Select(s => s.ErrorMessage).ToList();
                    result.ErrorMessage = string.Join(",", messages);
                }
            }

            return result;
        }

        private ZetesPayment CreateZetesPayment(SoftoneZetesConfiguration zetesConfig, CustomerInvoiceDistributionDTO invoice, CustomerDistributionDTO customerAddresses, List<string> connectedOrders, List<CustomerInvoiceRowSmallDTO> tpdInvoiceRows, bool paymentMatchesInvoice)
        {
            var zetesInvoice = new ZetesPayment
            {
                Reference = CreateZetesPaymentReference(invoice, customerAddresses, tpdInvoiceRows, paymentMatchesInvoice),
                RequestHeader = CreateZetesRequestHeader(zetesConfig),
                Serials = new List<string>(),
                ManufacturingOrders = new List<string>(),
                PurchaseOrders = connectedOrders,
            };

            return zetesInvoice;
        }

        private ZetesPaymentReference CreateZetesPaymentReference(CustomerInvoiceDistributionDTO invoice, CustomerDistributionDTO customerAddresses, List<CustomerInvoiceRowSmallDTO> tpdInvoiceRows, bool paymentMatchesInvoice)
        {
            var paymentReference = new ZetesPaymentReference
            {
                PaymentType = 1,
                PayerInEU = invoice.IsEUCountryBased ? 1 : 0,
                PayerID = invoice.ActorSupplierNr,
                PayerName = invoice.ActorName,
                PayerTaxNumber = invoice.ActorVatNr,
                PayerStreetOne = customerAddresses.BillingAddressStreet,
                PayerZipCode = customerAddresses.BillingAddressPostalCode,
                PayerCity = customerAddresses.BillingAddressCity,
                PayerCountryReg = invoice.ActorCountryCode,
                PaymentDate = invoice.InvoiceDate ?? DateTime.Today,
                InvoiceNumber = invoice.InvoiceNr,
                PaymentCurrency = invoice.CurrencyCode,
                PaymentAmount = tpdInvoiceRows.Sum(x => x.SumAmountCurrency) + tpdInvoiceRows.Sum(x => x.VATAmountCurrency), //invoice.TotalAmount,
                PaymentInvoice = paymentMatchesInvoice ? 1: 0
            };

            return paymentReference;
        }
        
        #endregion

        #region Zetes Invoice

        public ActionResult SendInvoice(bool testMode, SoftoneZetesConfiguration zetesConfig, CustomerInvoiceDistributionDTO invoice, CustomerDistributionDTO customerAddresses, List<string> connectedOrders, List<CustomerInvoiceRowSmallDTO> tpdInvoiceRows)
        {
            var message = "";

            if (string.IsNullOrEmpty(CurrentToken))
            {
                message = GetLoginToken(GetTestOauthConfig(zetesConfig, testMode));
            }

            if (string.IsNullOrEmpty(CurrentToken))
            {
                return new ActionResult("Failed getting Zetes login token: " + message);
            }

            var zetesInvoice = CreateZetesInvoice(zetesConfig, invoice, customerAddresses, connectedOrders, tpdInvoiceRows);
            var json = JsonConvert.SerializeObject(zetesInvoice, Formatting.Indented);
#if DEBUG
            File.WriteAllText(@"C:\Temp\zetes_invoice.json", json);
#endif

            var result = SendObject(GetUrl(testMode), "/api/v1.0/tracehub/level3/submitInvoice", json);

            if (!string.IsNullOrEmpty(result.StringValue))
            {
                var responseContent = JsonConvert.DeserializeObject<ZetesResponse>(result.StringValue);

                if (responseContent.ResponseResult.Errors.Any())
                {
                    var messages = responseContent.ResponseResult.Errors.Select(s => s.ErrorMessage).ToList();
                    result.ErrorMessage = string.Join(",", messages);
                }
            }
            return result;
        }

        private ZetesInvoice CreateZetesInvoice(SoftoneZetesConfiguration zetesConfig, CustomerInvoiceDistributionDTO invoice, CustomerDistributionDTO customerAddresses, List<string> connectedOrders, List<CustomerInvoiceRowSmallDTO> tpdInvoiceRows)
        {
            var zetesInvoice = new ZetesInvoice
            {
                Reference = CreateZetesInvoiceReference(invoice, customerAddresses, tpdInvoiceRows),
                RequestHeader = CreateZetesRequestHeader(zetesConfig),
                Serials = new List<string>(),
                ManufacturingOrders = new List<string>(),
                PurchaseOrders = connectedOrders,
            };

            return zetesInvoice;
        }

        private ZetesInvoiceReference CreateZetesInvoiceReference(CustomerInvoiceDistributionDTO invoice, CustomerDistributionDTO customerAddresses, List<CustomerInvoiceRowSmallDTO> tpdInvoiceRows)
        {
            var zetesInvoiceReference = new ZetesInvoiceReference
            {
                InvoiceType = invoice.BillingType == (int)TermGroup_BillingType.Credit ? 2 : 1,
                BuyerInEU = invoice.IsEUCountryBased ? 1 : 0,
                BuyerID = invoice.ActorSupplierNr,
                BuyerName = invoice.ActorName,
                BuyerTaxNumber = invoice.ActorVatNr,
                BuyerStreet = customerAddresses.BillingAddressStreet,
                BuyerStreetOne = invoice.IsEUCountryBased ? "" : customerAddresses.BillingAddressStreet,
                BuyerZipCode = customerAddresses.BillingAddressPostalCode,
                BuyerCity = customerAddresses.BillingAddressCity,
                BuyerCountryReg = invoice.ActorCountryCode,
                InvoiceDate = invoice.InvoiceDate ?? DateTime.Today,
                InvoiceNumber = invoice.InvoiceNr,
                Invoice_Currency = invoice.CurrencyCode,
                Invoice_Net = tpdInvoiceRows.Sum(x=> x.SumAmountCurrency) + tpdInvoiceRows.Sum(x => x.VATAmountCurrency), 
                FirstSellerEU = 0,
            };

            return zetesInvoiceReference;
        }
        
        #endregion

        private ZetesRequestHeader CreateZetesRequestHeader(SoftoneZetesConfiguration zetesConfig)
        {
            var zetesInvoiceRequestHeader = new ZetesRequestHeader
            {
                StakeholderCode = zetesConfig.StakeholderCode,
                ClientCode = zetesConfig.ClientCode,
                OriginCode = "Softone",
                RequestId = Guid.NewGuid().ToString(),
                DateRequest = DateTime.Today.ToString("yyyyMMdd"),
                TimeRequest = DateTime.Now.ToString("HHmmss"),
                Routes = new List<string> {"EU"}
            };

            return zetesInvoiceRequestHeader;
        }

        private string GetUrl(bool testMode)
        {
            return testMode ? TestURL : ProdURL;
        }

        private string GetLoginUrl(bool testMode)
        {
            return testMode ? TestLoginURL : ProdLoginURL;
        }
    }

    #region Helperobjects

    public class SoftoneZetesConfiguration
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string ClientCode { get; set; }
        public string StakeholderCode { get; set; }
    }

    public class ZetesPaymentReference
    {
        public int PaymentType { get; set; }
        public DateTime PaymentDate { get; set; }
        public int PayerInEU { get; set; }
        public string PayerID { get; set; }
        public string PayerName { get; set; }
        public string PayerZipCode { get; set; }
        public string PayerCity { get; set; }
        public string PayerCountryReg { get; set; }
        public string PayerStreetOne { get; set; }
        public string PayerStreetTwo { get; set; }
        public string PayerTaxNumber { get; set; }
        public int PaymentInvoice { get; set; }
        public string InvoiceNumber { get; set; }
        public decimal PaymentAmount { get; set; }
        public string PaymentCurrency { get; set; }
        public string EconomicOperatorId { get; set; }
    }

    public class ZetesInvoiceReference
    {
        public int InvoiceType { get; set; }
        public string TypeOther { get; set; }
        public string InvoiceNumber { get; set; }
        public DateTime InvoiceDate { get; set; }
        public int BuyerInEU { get; set; }
        public string BuyerID { get; set; }
        public string BuyerName { get; set; }
        public string BuyerStreet { get; set; }
        public string BuyerStreetOne { get; set; }
        public string BuyerStreetTwo { get; set; }
        public string BuyerHouseNumber { get; set; }
        public string BuyerZipCode { get; set; }
        public string BuyerCity { get; set; }
        public string BuyerCountryReg { get; set; }
        public string BuyerTaxNumber { get; set; }
        public int FirstSellerEU { get; set; }
        public decimal Invoice_Net { get; set; }
        public string Invoice_Currency { get; set; }
    }

    public class ZetesRequestHeader
    {
        public string ClientCode { get; set; }
        public string StakeholderCode { get; set; }
        public string DateRequest { get; set; }
        public string TimeRequest { get; set; }
        public string RequestId { get; set; }
        public string OriginCode { get; set; }
        public List<string> Routes { get; set; }
    }
    public class ZetesInvoice
    {
        public ZetesInvoiceReference Reference { get; set; }
        public string Comments { get; set; }
        public List<string> Serials { get; set; }
        public List<string> ManufacturingOrders { get; set; }
        public List<string> PurchaseOrders { get; set; }
        public ZetesRequestHeader RequestHeader { get; set; }
    }

    public class ZetesPayment
    {
        public ZetesPaymentReference Reference { get; set; }
        public string Comments { get; set; }
        public List<string> Serials { get; set; }
        public List<string> ManufacturingOrders { get; set; }
        public List<string> PurchaseOrders { get; set; }
        public ZetesRequestHeader RequestHeader { get; set; }
    }

    public class ZetesResponse
    {
        public ZetesResponseReference Reference { get; set; }
        public ZetesResponseResult ResponseResult { get; set; }

    }

    public class ZetesResponseReference
    {
        public string InvoiceNumber { get; set; }
        public string InvoiceDate { get; set; }
    }

    public class ZetesResponseResult
    {
        public string RequestId { get; set; }
        public int Result { get; set; }
        public int ProcessTime { get; set; }
        public string ConfirmationCode { get; set; }
        public int ErrorType { get; set; }
        public string RouterType { get; set; }
        public List<ZetesResponseResultError> Errors { get; set; }
    }

    public class ZetesResponseResultError
    {
        public string ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
    }

    #endregion
}
