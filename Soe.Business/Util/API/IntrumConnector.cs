using Newtonsoft.Json;
using RestSharp;
using SoftOne.Soe.Business.Util.API.OAuth;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Util.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SoftOne.Soe.Business.Util.API.Intrum
{
    #region Connector

    //https://anypoint.mulesoft.com/exchange/portals/intrum/64fe4806-309d-4611-90dd-9e6d22542b4d/sls-invoices/minor/2.0/pages/home/

    public class IntrumConnector: OAuthConnectorBase
    {
        public const int MAX_SEND_BATCH_SIZE = 100; //Max 100 per minute
        private const string TestURL = "https://uat-api.intrum.com";
        private const string ProdURL = "https://api.intrum.com";
        private const string LoginURL = "https://kc.intrum.com";

        private readonly string ClientIDTest = "9ec94cc0-b626-4eae-9548-726e71859114"; //intrum special Test, till keyvault 
        private readonly string ClientSecretTest = "c97254b6-1ab9-4302-b6bf-e541e00d491c"; //intrum special Test, till keyvault

        //To make call of API’s method you have to get your token first.The token’s life span is 50 minutes, after that time it expires.

        public ActionResult SendInvoice(bool testMode, SoftoneIntrumConfiguration intrumConfig, CustomerInvoiceDistributionDTO invoice, List<CustomerInvoiceRowSmallDTO> rows, CustomerDistributionDTO customerInfo, List<SysCountry> sysCountries, string invoiceDebetNo)
        {
            var invoiceObject = CreateIntrumInvoice(intrumConfig, invoice, rows, customerInfo, sysCountries, invoiceDebetNo);
            if (string.IsNullOrEmpty(CurrentToken))
            {
                CurrentToken = GetLoginToken(testMode, intrumConfig);
            }

            if (string.IsNullOrEmpty(CurrentToken))
            {
                return new ActionResult("Failed getting intrum login token");
            }

#if DEBUG
            var json = JsonConvert.SerializeObject(invoiceObject, Formatting.Indented);
            File.WriteAllText(@"C:\Temp\intrum\intrumfaktura.json", json);
#endif

            var client = GetRestClientWithNewtonsoftJson(testMode ? TestURL : ProdURL);
            var request = CreateIntrumRequest("/sls-invoices/api/invoices", CurrentToken, Method.Put, invoiceObject);
            var response = client.Execute(request);
            
            ActionResult result;
            switch (response.StatusCode)
            {
                case System.Net.HttpStatusCode.OK:
                case System.Net.HttpStatusCode.Created:
                    Retries = 0;
                    result = new ActionResult(true);
                    break;
                case System.Net.HttpStatusCode.Unauthorized:
                    CurrentToken = null;
                    Retries++;
                    if (Retries > 2)
                    {
                        result = new ActionResult((int)response.StatusCode, "IntrumConnector: " + response.StatusCode + ": " + StringUtility.HTMLToText(response.Content));
                    }
                    else
                    {
                        result = SendInvoice(testMode, intrumConfig, invoice, rows, customerInfo, sysCountries, invoiceDebetNo);
                    }
                    break;
                //429, To many requests....
                case System.Net.HttpStatusCode.ServiceUnavailable:
                case System.Net.HttpStatusCode.InternalServerError:
                case System.Net.HttpStatusCode.Forbidden:
                    result = new ActionResult((int)response.StatusCode, "IntrumConnector: " + response.StatusCode + ": " + StringUtility.HTMLToText(response.Content));
                    break;
                default:
#if DEBUG
                    File.WriteAllText(@"C:\Temp\intrum\intrumfaktura_response.xml", response.Content);
#endif
                    try
                    {
                        var responseContent = JsonConvert.DeserializeObject<IntrumResponse>(response.Content);

                        if (responseContent.messages != null)
                        {
                            var messages = responseContent.messages.Select(s => s.Trim()).ToList();
                            result = new ActionResult((int)response.StatusCode, string.Join(",", messages));
                        }
                        else if (!string.IsNullOrEmpty(responseContent.error))
                        {
                            result = new ActionResult((int)response.StatusCode, responseContent.error);
                        }
                        else
                        {
                            result = new ActionResult((int)response.StatusCode, response.StatusCode.ToString());
                        }
                    }
                    catch
                    {
                        result = new ActionResult((int)response.StatusCode, "IntrumConnector: " + response.StatusCode + ": " + StringUtility.HTMLToText(response.Content));
                    }

                    break;
            }

            return result;
        }

        /*
        public void SendInvoiceTest()
        {
            
            var token = GetLoginToken();
            var json = File.ReadAllText(@"c:\temp\intrum\intrumfaktura.json");
            var client = new GoRestClient(TestURL);

            var request = new RestRequest("/sls-invoices/api/invoices", Method.PUT);

            request.RequestFormat = DataFormat.Json;
            request.JsonSerializer = NewtonsoftJsonSerializer.Default;
            request.AddHeader("Authorization", "Bearer " + token);
            request.AddParameter("application/json; charset=utf-8", json, ParameterType.RequestBody);

            var response = client.Execute(request);
            var content = response.Content;
        }
        */

        private IntrumSLS CreateIntrumInvoice(SoftoneIntrumConfiguration intrumConfig, CustomerInvoiceDistributionDTO invoice, List<CustomerInvoiceRowSmallDTO> rows, CustomerDistributionDTO customerInfo, List<SysCountry> sysCountries, string invoiceDebetNo)
        {
            var intrumInvoice = new IntrumInvoice
            {
                invoiceHeaderType = "I", //"L|I|C|R|D|T"
                invoiceNo = invoice.InvoiceNr,
                invoiceDate = invoice.InvoiceDate.HasValue ? invoice.InvoiceDate.Value.ToString("yyyyMMdd") : "",
                invoiceDueDate = invoice.DueDate.HasValue ? invoice.DueDate.Value.ToString("yyyyMMdd") : "",
                //invoiceAmount = invoice.TotalAmount,
                invoiceCurrency = invoice.CurrencyCode,
                invoiceCustomerNo = invoice.ActorNr,
                invoiceType = "00",
                invoiceReferenceText1 = invoice.ReferenceYour,
                invoiceReferenceText2 = invoice.WorkingDescription,
                invoicePrint = "Y",
                invoiceDebetNo = invoiceDebetNo ?? "",
                invoiceReceiverEmailAddress = customerInfo.Email,
                invoiceRow = CreateIntrumInvoiceRows(rows),
            };

            var intrumCustomer = new IntrumCustomer
            {
                name = invoice.ActorName,
                no = invoice.ActorNr,
                orgNo = invoice.ActorOrgNr,
                vatNo = invoice.ActorVatNr,
                emailAddress = customerInfo.ReminderEmail.EmptyToNull() ?? customerInfo.Email,
                address1 = customerInfo.BillingAddressCO,
                address2 = customerInfo.BillingAddressStreet,
                address3 = customerInfo.VisitorAddressStreet,
                typeCode = "B", //"B|C|A"
                printReminders = "Y", //"Y|N"
                createInterestInvoice = "Y", //"Y|N"
                emailInvoice = "N", //"Y|N"
                ediInvoice = "N",
                countryCode = sysCountries.FirstOrDefault(c => c.SysCountryId == invoice.ActorSysCountryId.GetValueOrDefault())?.Code ?? "SE",
                languageCode = !invoice.ActorSysLanguageId.HasValue || invoice.ActorSysLanguageId.Value == 1 ? "SWE" : "ENG",
                city = customerInfo.BillingAddressCity,
                zipCode = customerInfo.BillingAddressPostalCode,
                category = invoice.InternalDescription,
            };

            //1 = Paper, 5 = Einvoice, 6 = EDI, 7 = Email, 8 = SMS
            switch ((SoeInvoiceDeliveryType)invoice.InvoiceDeliveryType.GetValueOrDefault())
            {
                case SoeInvoiceDeliveryType.Email:
                    intrumInvoice.invoiceDistributionCode = "7";
                    intrumCustomer.emailInvoice = "Y";
                    break;
                case SoeInvoiceDeliveryType.Electronic:
                    intrumInvoice.invoiceDistributionCode = "5";
                    break;
                case SoeInvoiceDeliveryType.EDI:
                    intrumInvoice.invoiceDistributionCode = "6";
                    intrumInvoice.invoiceReferenceText4 = invoice.ReferenceYour;
                    intrumInvoice.invoiceTypeEDI = invoice.BillingType == (int)TermGroup_BillingType.Credit ? "381" : "380";
                    intrumCustomer.ediInvoice = "Y";
                    break;
                default:
                    intrumInvoice.invoiceDistributionCode = "1";
                    break;
            }

            var ledger = new IntrumLedger
            {
                invoice = intrumInvoice,
                customer = intrumCustomer,
                ledgerNo = intrumConfig.LedgerNo,
                productionUnit = "02"
            };

            return new IntrumSLS
            {
                ledger = ledger,
                identification = new IntrumIdentification
                {
                    hubNo = intrumConfig.HubNo,
                    clientNo = intrumConfig.ClientNo,
                    clientBatchNo = intrumConfig.ClientBatchNo,
                    ijBatchNo = intrumConfig.IJBatchNo,
                    clientEmailAddress1 = intrumConfig.ClientEmailAddress1,
                    clientEmailAddress2 = intrumConfig.ClientEmailAddress2
                }
            };
        }

        private List<IntrumInvoiceRow> CreateIntrumInvoiceRows(List<CustomerInvoiceRowSmallDTO> rows)
        {
            var intrumInvoiceRows = new List<IntrumInvoiceRow>();

            foreach (var row in rows.OrderBy(x=> x.RowNr))
            {
                var intrumRow = new IntrumInvoiceRow
                {
                    rowDescription = row.Text.SafeSubstring(0,60),
                };

                if (row.Type == (int)SoeInvoiceRowType.ProductRow)
                {
                    intrumRow.rowDescription2 = row.DeliveryDateText.SafeSubstring(0, 30);
                    intrumRow.rowDescription3 = Math.Round(row.SumAmountCurrency, 2).ToString("F");
                    intrumRow.rowQuantity = Math.Round(row.Quantity, 0);
                    intrumRow.rowAmount = Math.Round(row.SumAmountCurrency, 2);
                    //rowUnitPrice = Math.Round(row.AmountCurrency, 2),
                    //rowVATAmount = Math.Round(row.VATAmountCurrency, 2),

                    switch (row.VatRate)
                    {
                        case 25M:
                            intrumRow.rowVATCode = 1;
                            break;
                        case 12M:
                            intrumRow.rowVATCode = 2;
                            break;
                        case 6M:
                            intrumRow.rowVATCode = 3;
                            break;
                        case 0M:
                            intrumRow.rowVATCode = 9;
                            break;
                    }
                }

                intrumInvoiceRows.Add(intrumRow);
            }

            return intrumInvoiceRows;
        }

        private RestRequest CreateIntrumRequest(string resource, string token, RestSharp.Method method, object obj = null)
        {
            var request = new RestRequest(resource, method);
            request.RequestFormat = DataFormat.Json;

            request.AddHeader("Authorization", "Bearer " + token);

            if (obj != null)
            {
                request.AddJsonBody(obj);
            }

            return request;
        }

        public string GetLoginToken(bool testMode, SoftoneIntrumConfiguration intrumConfig)
        {
            var clientId = testMode ? ClientIDTest : intrumConfig.ClientUser;
            var clientSecret = testMode ? ClientSecretTest : intrumConfig.ClientSecret;

            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
            {
                throw new ActionFailedException("Missing ClientId or ClientSecret");
            }

            var client = new GoRestClient(LoginURL);
            var plainTextBytes = Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}");
            var base64Secret = Convert.ToBase64String(plainTextBytes);

            var request = new RestRequest("/auth/realms/mulesoft/protocol/openid-connect/token", Method.Post);
            request.RequestFormat = DataFormat.Json;
            request.AddHeader("Authorization", $"basic {base64Secret}");
            request.AddParameter("grant_type", "client_credentials");

            var response = client.Execute(request);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var token = JsonConvert.DeserializeObject<OAuthAccessToken>(response.Content);
                return token.Access_token;
            }
            return "";
        }
    }

    #endregion

    #region Intrum API objects
   
    public class IntrumResponse
    {
        public List<string> messages { get; set; }
        public string error { get; set; }
    }

    public class IntrumSLS
    {
        public IntrumIdentification identification { get; set; }
        public IntrumLedger ledger { get; set; }
    }

    public class IntrumIdentification
    {
        public string hubNo { get; set; }
        public int clientNo { get; set; }
        public int clientBatchNo { get; set; }
        public int ijBatchNo { get; set; }
        public string clientEmailAddress1 { get; set; }
        public string clientEmailAddress2 { get; set; }
    }

    public class IntrumLedger
    {
        public string productionUnit { get; set; }
        public int ledgerNo { get; set; }
        public IntrumCustomer customer { get; set; }
        public IntrumInvoice invoice { get; set; }
    }

    public class IntrumCustomer
    {
        public string no { get; set; }
        public string name { get; set; }
        public string address1 { get; set; }
        public string address2 { get; set; }
        public string address3 { get; set; }
        public string zipCode { get; set; }
        public string city { get; set; }
        public string countryCode { get; set; }
        public string languageCode { get; set; }
        public string vatNo { get; set; }
        public string orgNo { get; set; }
        public string emailAddress { get; set; }
        public string category { get; set; }
        public string typeCode { get; set; }
        public string emailInvoice { get; set; }
        public string ediInvoice { get; set; }
        public string printReminders { get; set; }
        public string createInterestInvoice { get; set; }
    }

    public class IntrumInvoice
    {
        public string invoiceNo { get; set; }
        public string invoiceHeaderType { get; set; }
        public string invoiceCustomerNo { get; set; }
        public string invoiceDate { get; set; }
        public string invoiceDueDate { get; set; }
        public string invoiceCurrency { get; set; }
        public string invoiceType { get; set; }
        //public decimal invoiceAmount { get; set; } //Not allowed to be set right now....
        public string invoiceReferenceText1 { get; set; }
        public string invoiceReferenceText2 { get; set; }
        public string invoiceReferenceText3 { get; set; }
        public string invoiceReferenceText4 { get; set; }
        public string invoiceReferenceText5 { get; set; }
        public string invoiceReferenceText6 { get; set; }
        public string invoiceReferenceText7 { get; set; }
        public string invoiceReferenceText8 { get; set; }
        public string invoiceReferenceText9 { get; set; }
        public string invoiceReferenceText10 { get; set; }
        public string invoiceReferenceText11 { get; set; }
        public string invoiceReferenceText12 { get; set; }
        public string invoiceReferenceText13 { get; set; }
        public string invoiceReferenceText14 { get; set; }
        public string invoiceReferenceText15 { get; set; }
        public string invoiceReferenceText16 { get; set; }
        public string invoiceReferenceText17 { get; set; }
        public string invoiceReferenceText18 { get; set; }
        public string invoiceReferenceText19 { get; set; }
        public string invoiceReferenceText20 { get; set; }
        public string invoiceReferenceText21 { get; set; }
        public string invoiceReferenceText22 { get; set; }
        public string clientOCRReference { get; set; }
        public string invoiceDebetNo { get; set; }
        public string invoicePrint { get; set; }
        public string invoiceDistributionCode { get; set; }
        public string invoiceTypeEDI { get; set; }
        public string invoiceReceiverEmailAddress { get; set; }

        public List<IntrumInvoiceRow> invoiceRow { get; set; }
    }

    public class IntrumInvoiceRow
    {
        public string rowDescription { get; set; }
        public string rowDescription2 { get; set; }
        public string rowDescription3 { get; set; }
        public decimal? rowAmount { get; set; }
        public int? rowVATCode { get; set; }
        //public decimal rowVATAmount { get; set; }
        public decimal? rowQuantity { get; set; }
        //public decimal rowUnitPrice { get; set; }
    }

    #endregion

    #region Helperobjects

    public class SoftoneIntrumConfiguration
    {
        public string HubNo { get; set; }
        public int ClientNo { get; set; }
        public int ClientBatchNo { get; set; }
        public int IJBatchNo { get; set; }
        public string ClientEmailAddress1 { get; set; }
        public string ClientEmailAddress2 { get; set; }
        public int LedgerNo { get; set; }
        public string ClientUser { get; set; }
        public string ClientSecret { get; set; }
    }

    #endregion 
}
