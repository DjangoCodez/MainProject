using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Util.API.Models;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;


//https://api.inexchange.com/v1/docs/#sendInvoices

namespace SoftOne.Soe.Business.Util.API.InExchange
{

    #region Sofone Inexchange Settings
    static class InexchangeSettings
    {
        public static string urlSend
        {
            get { return "api/documents"; }    
        }

        public static string urlSendLink
        {
            get { return "api/documents/outbound"; } 
        }

        public static string urlRegister
        {
            get { return "api/companies/details"; } 
        }

        public static string urlActivate
        {
            get { return "api/companies/services"; } 
        }

        public static string urlGetToken
        {
            get { return "api/clientTokens/create"; } 
        }

        public static string urlRevokeToken
        {
            get { return "api/clientTokens/revoke"; } 
        }

        public static string urlInvoiceStatusList
        {
            get { return "api/documents/outbound/list"; } 
        }

        public static string urlBuyerLookup
        {
            get { return "api/buyerparties/lookup"; } 
        }
    }

    public class InExchangeApiSendInfo
    {
        public string recipientGLN { set; get; }
        public string recipientEmail { set; get; }
        public string recipientName { set; get; }
        public string recipientOrgNo { set; get; }
        public string senderEmail { set; get; }
        public string senderName { set; get; }
        public string country { set; get; }
        public string inexchangeCompanyId { set; get; }
    }

    #endregion

    public static class InExchangeConnector
    {
        private static string _apikey;

        private static void AddLogg(string msg, bool error)
        {
            SysLogManager slm = new SysLogManager(null);
            if (error)
            {
                slm.AddSysLogErrorMessage(Environment.MachineName, "Inexchange API error", msg);
            }
            else
            {
                slm.AddSysLogInfoMessage(Environment.MachineName, "Inexchange API info", msg);
            }
        }

        public static string getApiKey()
        {
            if (string.IsNullOrEmpty(_apikey))
            {
                _apikey = SoftOne.Common.KeyVault.KeyVaultSecretsFetcher.GetSecret("Inexchange-Apikey");
            }
            return _apikey;
        }

        #region GetMethods

        public static string GetToken(int ActorCompanyId, bool releaseMode = false)
        {
            bool result = true;
            string resultText = string.Empty;

            try
            {
                var tokenUrl = InexchangeSettings.urlGetToken;
                if (string.IsNullOrEmpty(tokenUrl))
                {
                    throw new Exception("GetTokenForCustomer: InExchangeurlGetToken is empty!");
                }

                var httpWebRequest = (HttpWebRequest)WebRequest.Create(getDomainUrl(releaseMode) + tokenUrl);
                httpWebRequest.ContentType = "application/json; charset=utf-8";
                httpWebRequest.Method = "POST";

                var key = getApiKey();
                httpWebRequest.Headers.Add("APIKey:" + key);

                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    string json = "{" +
                                  "\"erpId\":\"" + ActorCompanyId.ToString() + "\"" +
                                  "}";


                    // Do some logging
                    // AddLogg(json.ToString(), false);

                    streamWriter.Write(json);
                    streamWriter.Flush();
                    streamWriter.Close();
                }

                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    resultText = streamReader.ReadToEnd();
                    //Get the token from the resultText
                    //Example result:  {
                    //"token": "eH8VayyH18H0+CqbCCQ4fbw0M5Tkd0vcF4D/xuCoPl4=",
                    //}
                    string token = resultText;
                    token = token.Replace("{\r\n", "");
                    token = token.Replace("\r\n}", "");
                    token = token.Replace("\"token\":", "");
                    token = token.Trim();
                    token = token.Replace("\"", "");
                    resultText = token;
                }
            }
            catch (Exception ex)
            {
                result = false;
                AddLogg("GetTokenForCustomer: Exception:" + ex.Message, true);
            }

            return result ? resultText : "";
        }

        public static bool SendDocumentHandled(List<string> documentIds, int actorCompanyId, bool releaseMode)
        {
            var token = GetToken(actorCompanyId, releaseMode);

            if (string.IsNullOrEmpty(token))
            {
                AddLogg("SendDocumentHandled got empty token", true);
                return false;
            }

            var result = false;
            var json = new StringBuilder();

            try
            {
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(getDomainUrl(releaseMode) + "/api/documents/handled");
                httpWebRequest.ContentType = "application/json; charset=utf-8";
                httpWebRequest.Method = "POST";
                httpWebRequest.Headers.Add("ClientToken:" + token);

                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    json.Append("{");
                    json.Append("\"documents\": [");

                    foreach (var id in documentIds)
                    {
                        json.Append($"\"{id}\",");
                    }
                    json.Append("]}");

                    // Do some logging
                    //AddLogg(json.ToString(), false);

                    streamWriter.Write(json);
                    streamWriter.Flush();
                    streamWriter.Close();
                }

                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                if (httpResponse.StatusCode != HttpStatusCode.OK)
                {
                    AddLogg($"SendDocumentHandled got response status: {httpResponse.StatusCode}", true);
                }

            }
            catch (Exception ex)
            {
                AddLogg($"SendDocumentHandled: Exception:{ex.Message}", true);
            }

            return result;
        }

        public static List<InExchangeApiIncomingDocument> GetIncomingDocuments(int actorCompanyId, bool releaseMode)
        {
            var token = GetToken(actorCompanyId, releaseMode);
            var result = new List<InExchangeApiIncomingDocument>();

            if (string.IsNullOrEmpty(token))
            {
                AddLogg("GetIncomingDocuments got empty token", true);
                return null;
            }

            try
            {
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(getDomainUrl(releaseMode) + "/api/documents/incoming?type=invoice");
                httpWebRequest.ContentType = "application/json; charset=utf-8";
                httpWebRequest.Method = "GET";
                httpWebRequest.Headers.Add("ClientToken:" + token);

                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var resultText = streamReader.ReadToEnd();
                    if (!string.IsNullOrEmpty(resultText))
                    {
                        var response = JsonConvert.DeserializeObject<InExchangeApiIncomingDocumentResponse>(resultText);
                        result = response.documents;
                    }
                }

                foreach (var document in result)
                {
                    document.filedata = GetIncomingDocumentStream(token, document.downloadUrl);
#if DEBUG
                    //File.WriteAllBytes(@"C:\Temp\Inexchange\incoming\" + document.id + ".xml", document.filedata.ToByteArray());
                    //document.filedata.Position = 0;
#endif
                }

                return result;
            }
            catch (Exception ex)
            {
                AddLogg($"GetIncomingDocuments: Exception:{ex.Message}", true);
                result = null;
            }

            return result;
        }

        private static MemoryStream GetIncomingDocumentStream(string token, string downloadUrl)
        {
            try
            {
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(downloadUrl);
                httpWebRequest.ContentType = "application/json; charset=utf-8";
                httpWebRequest.Method = "GET";
                httpWebRequest.Headers.Add("ClientToken:" + token);

                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                var rs = httpResponse.GetResponseStream();
                var ms = new MemoryStream();
                rs.CopyTo(ms);
                ms.Position = 0;
                return ms;
            }
            catch (Exception ex)
            {
                AddLogg($"GetIncomingDocumentStream: DownloadUrl:{downloadUrl} Exception:{ex.Message}", true);
                return null;
            }
        }

        public static List<InExchangeAPIDocumentStatus> GetStatusList(int actorCompanyId, bool releaseMode, DateTime lastCheckedDate)
        {
            bool fetch = true;
            int skip = 0;
            const int take = 100;
            List<InExchangeAPIDocumentStatus> statusList = new List<InExchangeAPIDocumentStatus>();

            var token = GetToken(actorCompanyId, releaseMode);

            if (string.IsNullOrEmpty(token))
            {
                AddLogg("GetStatusList got empty token", true);
                return statusList;
            }

            while (fetch)
            {
                var responseObject = GetStatusList(releaseMode, token, skip, take, lastCheckedDate);
                if (responseObject.documents.Count > 0)
                {
                    statusList.AddRange(responseObject.documents);
                }
                skip = statusList.Count;
                fetch = (statusList.Count < responseObject.totalCount);
            }

            //After sending cancel the token
            RevokeTokenForCustomer(token, releaseMode);

            return statusList;
        }

        private static InExchangeApiDocumentStatusResponse GetStatusList(bool releaseMode, string loginToken, int skip, int take, DateTime lastCheckedDate)
        {
            var result = new InExchangeApiDocumentStatusResponse();

            if (loginToken != "")
            {
                try
                {
                    var documentRequest = new InexchangeDocumentStatusRequest
                    {
                        Skip = skip,
                        Take = take,
                        UpdatedAfter = lastCheckedDate,
                        IncludeFileInfo = false,
                        IncludeErrorInfo = true
                    };

                    var options = new RestClientOptions(new Uri(getDomainUrl(releaseMode)));
                    var client = new GoRestClient(options, configureSerialization: s => s.UseNewtonsoftJson());
                    var request = CreateInexchangeRequest(InexchangeSettings.urlInvoiceStatusList, loginToken, Method.Post, documentRequest);
                    var response = client.Execute(request);

#if DEBUG
                    //File.WriteAllText(@"C:\Temp\Inexchange\InexchangeStatusList.txt", response.Content);
#endif

                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        var data = JsonConvert.DeserializeObject<InExchangeApiDocumentStatusResponse>(response.Content);
                        return data;
                    }
                    else
                    {
                        AddLogg("GetBuyerCompany: Error:" + response.Content, true);
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    AddLogg("GetStatusListInExchangeApi: Exception:" + ex.Message, true);
                    AddLogg("GetStatusListInExchangeApi: TokenURL:" + getDomainUrl(releaseMode) + InexchangeSettings.urlGetToken, false);
                    AddLogg("GetStatusListInExchangeApi: Token:" + loginToken, false);
                }
            }

            //If fails returns empty string
            return result;
        }

        public static ActionResult GetInexchangeBuyerReciverCompanyId(string loginToken, bool releaseMode, int actorCompanyId, InexchangeBuyerPartyLookup lookup)
        {
            if (string.IsNullOrEmpty(loginToken))
                return new ActionResult("Empty token");

            var companiesFoundByGroup = InExchangeConnector.GetBuyerCompany(loginToken, releaseMode, actorCompanyId, lookup)?.Parties.Where(x => x.ReceiveElectronicInvoiceCapability == "ReceivingElectronicInvoices").GroupBy(x => new { x.GLN, x.OrgNo }).ToList();
            if (companiesFoundByGroup.Count == 1)
            {
                return new ActionResult{Success = true, StringValue= companiesFoundByGroup.First()?.First()?.CompanyId};
            }
            else if (companiesFoundByGroup.Count > 1)
            {
                var message = "";
                var count = 0;
                foreach (var companyGroupMatch in companiesFoundByGroup)
                {
                    foreach (var item in companyGroupMatch)
                    {
                        message += $"{item.Name} : {item.OrgNo} : {item.GLN}\n";
                        count++;
                    }
                }
                return new ActionResult{ErrorMessage = message,IntegerValue = count, Success = false};
            }
            else
            {
                return new ActionResult(false);
            }
        }

        public static InexchangeBuyerLookupResponse GetBuyerCompany(string loginToken, bool releaseMode, int actorCompanyId, InexchangeBuyerPartyLookup searchData)
        {
            var options = new RestClientOptions(new Uri(getDomainUrl(releaseMode)));
            var client = new GoRestClient(options, configureSerialization: s => s.UseNewtonsoftJson());
            var request = CreateInexchangeRequest(InexchangeSettings.urlBuyerLookup, loginToken, Method.Post, searchData);
            var response = client.Execute(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var data = JsonConvert.DeserializeObject<InexchangeBuyerLookupResponse>(response.Content);
                return data;
            }
            else
            {
                AddLogg("GetBuyerCompany: Error:" + response.Content, true);
                return null;
            }
        }

        public static List<EInvoiceRecipientSearchResultDTO> GetBuyerCompany(bool releaseMode, int actorCompanyId, EInvoiceRecipientSearchDTO searchData)
        {
            string loginToken = InExchangeConnector.GetToken(actorCompanyId, releaseMode);

            var buyerPartyLookup = new InexchangeBuyerPartyLookup
            {
                PartyId = actorCompanyId.ToString(),
                Name = searchData.Name,
                GLN = searchData.GLN,
                OrgNo = searchData.OrgNo,
                VatNo = searchData.VatNo,
            };

            var response = GetBuyerCompany(loginToken, releaseMode, actorCompanyId, buyerPartyLookup);
            InExchangeConnector.RevokeTokenForCustomer(loginToken, releaseMode);

            if (searchData.ReceiveElectronicInvoiceCapability)
            {
                response.Parties = response.Parties.Where(x => x.ReceiveElectronicInvoiceCapability == "ReceivingElectronicInvoices").ToList();
            }

            var recipientsList = new List<EInvoiceRecipientSearchResultDTO>();

            if (response == null)
            {
                AddLogg("Inexchange.GetBuyerCompany returned null", false);
            }
            else
            {
                foreach (var recipient in response.Parties)
                {
                    var recipientDTO = new EInvoiceRecipientSearchResultDTO()
                    {
                        CompanyId = recipient.CompanyId,
                        Name = recipient.Name,
                        OrgNo = recipient.OrgNo,
                        VatNo = recipient.VatNo,
                        GLN = recipient.GLN
                    };
                    recipientsList.Add(recipientDTO);
                }
            }
            
            return recipientsList;
        }

        private static RestRequest CreateInexchangeRequest(string resource, string token, RestSharp.Method method, object obj = null)
        {
            var request = new RestRequest(resource, method);
            request.RequestFormat = DataFormat.Json;

            request.AddHeader("ClientToken", token);

            if (obj != null)
            {
                request.AddJsonBody(obj);
            }

            return request;
        }

        #endregion

        #region SendMethods

        public static ActionResult SendFinVoiceMessageToBePostedInExchangeApi(int actorCompanyId, int invoiceId, byte[] fileTosend, string format, string token, string filename, string country = "SE", string orgNo = "", string recipientName = "", string address = "", string city = "", string postalCode = "", bool releaseMode = false)
        {
            //Send file to printing service (paper)
            ActionResult result = new ActionResult(false);
            bool resultApi = true;
            string location = string.Empty;
            string resultString = string.Empty;
            var utf8WithoutBom = new System.Text.UTF8Encoding(false);

            if (token != "")
            {
                Guid g = Guid.NewGuid();
                string divider = g.ToString();

                try
                {
                    var httpWebRequest = (HttpWebRequest)WebRequest.Create(getDomainUrl(releaseMode) + InexchangeSettings.urlSendLink);
                    httpWebRequest.ContentType = "multipart/form-data; boundary =" + divider;
                    httpWebRequest.Method = "POST";
                    httpWebRequest.Headers.Add("ClientToken:" + token);

                    using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                    {
                        string json = "--" + divider + Environment.NewLine +
                            "Content-Type: application/json" + Environment.NewLine +
                            "Content-Disposition: form-data" + Environment.NewLine + Environment.NewLine +
                            "{" +
                            "\"sendDocumentAs\":{" +
                                      "\"type\":" + "\"Paper\"," +
                                      "\"paper\":{" +
                                            "\"recipientAddress\":{" +
                                                "\"name\":" + "\"" + recipientName + "\"," +
                                                "\"streetName\":" + "\"" + address + "\"," +
                                                "\"city\":" + "\"" + city + "\"," +
                                                "\"postalZone\":" + "\"" + postalCode + "\"," +
                                                "\"countryCode\":" + "\"" + country + "\"" +
                                            "}," +
                                            "\"postageType\":" + "\"EconomyClass\"," +
                                            "\"colorSettings\":{" +
                                                "\"mode\":" + "\"BlackAndWhite\"" +
                                            "}" +
                                      "}" +
                            "}," +
                            "\"recipientInformation\":" + "{" +
                                "\"orgNo\":" + "\"" + orgNo + "\"," +
                                "\"name\":" + "\"" + recipientName + "\"," +
                                "\"countryCode\":" + "\"" + country + "\"" +
                                 "}," +
                            "\"document\":" + "{" +
                                "\"documentFormat\":\"" + format + "\"," +
                                "\"documentUri\":\"urn:attachment:" + filename + "\"" +
                                 "}" +
                                      "}" +
                                      Environment.NewLine +
                                      "--" + divider + Environment.NewLine +
                                      "Content-Disposition: attachment; filename=" + filename + Environment.NewLine + Environment.NewLine +
                                      utf8WithoutBom.GetString(fileTosend) +
                                      Environment.NewLine +
                                      "--" + divider + "--";
#if DEBUG
                        //File.WriteAllBytes(@"C:\Temp\Inexchange\Finvoice_"+ FixFileName(filename), fileTosend);
#endif
                        // Do some logging
                        SysLogManager sl = new SysLogManager(null);
                        string loggJson = json.ToString();
                        sl.AddSysLogInfoMessage(Environment.MachineName, "API", loggJson);

                        streamWriter.Write(json);
                        streamWriter.Flush();
                        streamWriter.Close();
                    }

                    var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        location = httpResponse != null ? httpResponse.Headers["Location"] : string.Empty;

                        if (location != string.Empty)
                        {
                            resultString = Path.GetFileName(new Uri(location).AbsolutePath);

                            resultApi = true;
                            result.Success = true;
                            result.BooleanValue = true;
                            result.Value = resultString;
                        }
                        else
                        {
                            resultApi = false;
                            result.Success = false;
                            result.BooleanValue = false;
                            result.Value = string.Empty;
                        }
                    }
                }
                catch (Exception ex)
                {
                    resultApi = false;
                    AddLogg("SendFinVoiceMessageToBePostedInExchangeApi: Exception:" + ex.Message, true);
                }

            }
            if (resultApi)
            {
                result.Success = true;
                result.BooleanValue = true;
            }

            return result;
        }

        public static string FixFileName(string fileName)
        {
            // return  Regex.Replace(fileName, "[ÅÄÖåäö ]", "_");  //fileName.Replace("ö", "_").Replace(" ", "_");
            return Regex.Replace(fileName, @"[^\u0000-\u007F]|[(]|[)]|[,]|[\s]|[@]|[/]|[\\]", "_");
        }

        private static InexchangeRecipientInformation GetRecipientInfo(InExchangeApiSendInfo sendInfo)
        {
            return new InexchangeRecipientInformation()
            {
                GLN = sendInfo.recipientGLN,
                OrgNo = sendInfo.recipientOrgNo,
                Name = sendInfo.recipientName,
                CountryCode = sendInfo.country,
            };
        }

        private static InexchangeSendDocumentAs GetSendDocumentAs(string type, InExchangeApiSendInfo sendInfo)
        {
            //empty type means that Inexchange tries to send in the following order, eInvoice, mail, paper
            var sendAs = new InexchangeSendDocumentAs()
            {
                Type = type,
                Pdf = new InexchangePdf()
            };

            //no email if inexchangeCompanyId is set so send will fail if it can not be sent electronically
            if (!string.IsNullOrEmpty(sendInfo.inexchangeCompanyId))
            {
                sendAs.Electronic = new InexchangeElectronic
                {
                    RecipientId = sendInfo.inexchangeCompanyId
                };
            }
            else if (!string.IsNullOrEmpty(sendInfo.recipientEmail))
            {
                sendAs.Pdf.RecipientEmail = sendInfo.recipientEmail;
            }

            return sendAs;
        }

        public static ActionResult SendSveFakturaMessageToBePostedInExchangeApi(int actorCompanyId, int invoiceId, string svefakturaFileName, string invoicePdfFileName, Dictionary<string, byte[]> attachments, string token, bool releaseMode, InExchangeApiSendInfo sendInfo)
        {
            //Send file to printing service (paper)
            ActionResult result = new ActionResult(false);
            bool resultApi = true;
            string location = string.Empty;
            string resultString = string.Empty;
            var utf8WithoutBom = new System.Text.UTF8Encoding(false);
            var sb = new StringBuilder();

            if (string.IsNullOrEmpty(token))
            {
                AddLogg($"SendSveFakturaMessageToBePostedInExchangeApi called with empty token, actorCompanyId:{actorCompanyId} filename:{svefakturaFileName}", true);
                return result;
            }

            Guid g = Guid.NewGuid();
            string divider = g.ToString();

            try
            {
                if (sendInfo == null)
                {
                    throw new Exception("Inexchange sendInfo object is not set!");
                }

                if (string.IsNullOrEmpty(svefakturaFileName))
                {
                    throw new Exception("svefaktura file name is not set!");
                }

                var httpWebRequest = (HttpWebRequest)WebRequest.Create(getDomainUrl(releaseMode) + InexchangeSettings.urlSendLink);
                httpWebRequest.ContentType = "multipart/form-data; boundary =" + divider;
                httpWebRequest.Method = "POST";
                httpWebRequest.Headers.Add("ClientToken:" + token);

                //using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                using (var stream = httpWebRequest.GetRequestStream())
                {
                    sb.Append("--" + divider + Environment.NewLine +
                        "Content-Type: application/json" + Environment.NewLine +
                        "Content-Disposition: form-data" + Environment.NewLine + Environment.NewLine);

                    sb.Append("{");

                    var sendAsObject = GetSendDocumentAs(null, sendInfo);
                    sb.Append("sendDocumentAs:" + JsonConvert.SerializeObject(sendAsObject, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));

                    sb.Append(",");
                    var recipientInfoObject = GetRecipientInfo(sendInfo);
                    sb.Append("recipientInformation:" + JsonConvert.SerializeObject(recipientInfoObject, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));

                    sb.Append(",");

                    string json =
                        "\"document\":" + "{" +
                            "\"documentFormat\":\"" + "svefaktura" + "\"," + Environment.NewLine;

                    json += "\"erpDocumentId\":\"" + invoiceId.ToString() + "\"," + Environment.NewLine;

                    var svefaktura = attachments.FirstOrDefault(x => x.Key == svefakturaFileName);
                    var invoicePdf = attachments.FirstOrDefault(x => x.Key == invoicePdfFileName);

                    if (svefaktura.Value == null)
                    {
                        throw new Exception("svefaktura xml file is missing in attachment list!");
                    }

                    /*
                    foreach (KeyValuePair<string, byte[]> file in attachments)
                    {
                        json += "\"documentUri\":\"urn:attachment:" + file.Key + "\"," + Environment.NewLine;
                    }
                    */

                    json += "\"documentUri\":\"urn:attachment:" + svefaktura.Key + "\"," + Environment.NewLine;

                    if (!string.IsNullOrEmpty(invoicePdf.Key))
                    {
                        json += "\"renderedDocumentFormat\":\"" + "application/pdf" + "\"," + Environment.NewLine;
                        json += "\"renderedDocumentUri\":\"urn:attachment:" + invoicePdf.Key + "\"," + Environment.NewLine;
                    }

                    var attachementsList = attachments.Where(x => x.Key != svefaktura.Key && x.Key != invoicePdf.Key);
                    if (attachementsList.Any())
                    {
                        json += "\"attachments\":[" + Environment.NewLine;

                        int fileCount = attachementsList.Count();
                        foreach (KeyValuePair<string, byte[]> file in attachementsList)
                        {
                            fileCount--;
                            var end = (fileCount > 0) ? "\"," : "\"";

                            json += "\"urn:attachment:" + FixFileName(file.Key) + end + Environment.NewLine;
                        }

                        json += "]" + Environment.NewLine;
                    }

                    json += "}" +
                             "}";

                    sb.Append(json);
#if DEBUG
                    // Do some logging
                    AddLogg($"[token]:{token} [nrOfAttachments]:{attachementsList.Count()} [message]:{sb.ToString()}", false);
                    File.WriteAllText(@"C:\Temp\Inexchange\InExchangeJson.txt", sb.ToString());
#endif

                    var jsonBytes = utf8WithoutBom.GetBytes(sb.ToString());
                    stream.Write(jsonBytes, 0, jsonBytes.Length);

                    var last = attachments.Last();
                    string lastDivider = string.Empty;

                    foreach (KeyValuePair<string, byte[]> file in attachments)
                    {
                        var startString = Environment.NewLine +
                                 "--" + divider + Environment.NewLine +
                                 "Content-Disposition: attachment; filename=" + FixFileName(file.Key) + Environment.NewLine + Environment.NewLine;
                        var startBytes = utf8WithoutBom.GetBytes(startString);
                        stream.Write(startBytes, 0, startBytes.Length);

#if DEBUG
                        //  File.WriteAllBytes(@"C:\Temp\Inexchange\Attachement_"+ FixFileName(file.Key), file.Value);
#endif

                        stream.Write(file.Value, 0, file.Value.Length);

                        if (file.Equals(last))
                        {
                            lastDivider = "--";
                        }

                        var endString = Environment.NewLine +
                                 "--" + divider + lastDivider + Environment.NewLine + Environment.NewLine;
                        var endBytes = utf8WithoutBom.GetBytes(endString);
                        stream.Write(endBytes, 0, endBytes.Length);
                    }

                    stream.Flush();
                    stream.Close();
                }

                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    location = httpResponse != null ? httpResponse.Headers["Location"] : string.Empty;

                    if (location != string.Empty)
                    {
                        resultString = Path.GetFileName(new Uri(location).AbsolutePath);

                        resultApi = true;
                        result.Success = true;
                        result.BooleanValue = true;
                        result.Value = resultString;
                    }
                    else
                    {
                        resultApi = false;
                        result.Success = false;
                        result.BooleanValue = false;
                        result.Value = string.Empty;
                        AddLogg("SendSveFakturaMessageToBePostedInExchangeApi: location failed", true);
                    }
                }
            }
            catch (Exception ex)
            {
                resultApi = false;
                AddLogg("SendSveFakturaMessageToBePostedInExchangeApi: Exception: \n" + ex.ToString(), true);
            }

            if (resultApi)
            {
                result.Success = true;
                result.BooleanValue = true;
            }

            return result;
        }

        #endregion

        #region Help methods
        private static string GetRegisterProcessString(bool sendInvoices, bool reciveInvoices)
        {
            string result = "";

            if (sendInvoices)
                result += "\"SendInvoices\"";

            if (reciveInvoices)
            {
                if (!string.IsNullOrEmpty(result))
                {
                    result += ",\n";
                }
                result += "\"ReceiveInvoices\"";
            }

            return "[" + result + "]";
        }

        public static bool RegisterCompanyToInExchangeAPI(int ActorCompanyId, CompanyDTO comp, string street, string postalCode, string city, string email, bool sendInvoices, bool reciveInvoices, bool releaseMode)
        {
            bool result = true;
            string resultText = "";
            string apiKeySend = getApiKey();
            string country = (comp.SysCountryId.HasValue && (int)comp.SysCountryId == 3) ? "FI" : "SE";

            var currentProtocoll = ServicePointManager.SecurityProtocol;

            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(getDomainUrl(releaseMode) + InexchangeSettings.urlRegister);
                httpWebRequest.ContentType = "application/json; charset=utf-8";
                httpWebRequest.Method = "POST";
                httpWebRequest.Headers.Add("APIKey:" + apiKeySend);

                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    string json = "{\"registrationId\":\"" + ActorCompanyId.ToString() + "\"," +
                                  "\"orgNo\":\"" + comp.OrgNr + "\"," +
                                  "\"vatNo\":\"" + comp.VatNr + "\"," +
                                  "\"name\":\"" + comp.Name + "\"," +
                                  "\"erpProduct\":\"" + "SoftOne" + "\"," +
                                  //"\"department\":\"" + "Development team" + "\"," +
                                  "\"streetName\":\"" + street + "\"," +
                                  "\"postalZone\":\"" + postalCode + "\"," +
                                  "\"city\":\"" + city + "\"," +
                                  "\"countryCode\":\"" + country + "\"," +
                                  "\"email\":\"" + email + "\"," +
                                  "\"isVatRegistered\":" + (comp.VatNr.HasValue()).ToString().ToLower() + "," +
                                  "\"processes\":" + GetRegisterProcessString(sendInvoices, reciveInvoices) +
                                  "}";

                    // Do some logging
                    AddLogg("RegisterCompanyToInExchangeAPI: msg:" + json.ToString(), false);

                    streamWriter.Write(json);
                    streamWriter.Flush();
                    streamWriter.Close();
                }

                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    resultText = streamReader.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                result = false;
                AddLogg("RegisterCompanyToInExchangeAPI: Exception:" + ex.Message, true);
            }

            ServicePointManager.SecurityProtocol = currentProtocoll;

            return result;
        }

        public static bool ActivateCompany_In_InExchangeAPI(int ActorCompanyId, string orgNo, string country = "SE", bool releaseMode = false)
        {
            bool result = true;
            string resultText = "";
            string apiKeySend = getApiKey();

            var currentProtocoll = ServicePointManager.SecurityProtocol;

            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(getDomainUrl(releaseMode) + InexchangeSettings.urlActivate);
                httpWebRequest.ContentType = "application/json; charset=utf-8";
                httpWebRequest.Method = "POST";
                httpWebRequest.Headers.Add("APIKey:" + apiKeySend);

                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    string json = "{\"registrationId\":\"" + ActorCompanyId.ToString() + "\"," +
                                  "\"erpId\":\"" + ActorCompanyId.ToString() + "\"," +
                                  "\"countryCode\":\"" + country + "\"," +
                                  "\"orgNo\":\"" + orgNo + "\"}";

                    // Do some logging
                    SysLogManager sl = new SysLogManager(null);
                    string loggJson = json.ToString();
                    sl.AddSysLogInfoMessage(Environment.MachineName, "API", loggJson);

                    streamWriter.Write(json);
                    streamWriter.Flush();
                    streamWriter.Close();
                }

                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    resultText = streamReader.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                result = false;
                AddLogg("ActivateCompany_In_InExchangeAPI: Exception:" + ex.Message, true);
            }

            ServicePointManager.SecurityProtocol = currentProtocoll;
            return result;
        }

        public static bool RevokeTokenForCustomer(string token, bool releaseMode = false)
        {
            bool result = false;
            string resultText = "";

            try
            {
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(getDomainUrl(releaseMode) + InexchangeSettings.urlRevokeToken);
                httpWebRequest.ContentType = "application/json; charset=utf-8";
                httpWebRequest.Method = "POST";

                var key = getApiKey();
                httpWebRequest.Headers.Add("APIKey:" + key);

                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    string json = "{" +
                                  "\"clientToken\":\"" + token + "\"" +
                                  "}";

                    streamWriter.Write(json);
                    streamWriter.Flush();
                    streamWriter.Close();
                }

                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    resultText = streamReader.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                result = false;
                AddLogg("RevokeTokenForCustomer: Exception:" + ex.Message, true);
            }

            return result;
        }

        private static IEnumerable<JToken> AllChildren(JToken json)
        {
            foreach (var c in json.Children())
            {
                yield return c;
                foreach (var cc in AllChildren(c))
                {
                    yield return cc;
                }
            }
        }

        public static string getDomainUrl(bool releaseMode)
        {
            if (releaseMode)
                return "https://api.inexchange.com/v1/";
            else
                return "https://testapi.inexchange.se/v1/";

        }

        #endregion

    }


    #region Inexchange API objects

    #region Inexchange Sending objects

    public class InexchangeBuyerPartyLookup
{
    public string PartyId { set; get; }
    public string Name { set; get; }
    public string Department { set; get; }
    public string StreetName { set; get; }
    public string PostBox { set; get; }
    public string PostalZone { set; get; }
    public string City { set; get; }
    public string CountryCode { set; get; }
    public string PhoneNo { set; get; }
    public string FaxNo { set; get; }
    public string Email { set; get; }
    public string GLN { set; get; }
    public string OrgNo { set; get; }
    public string VatNo { set; get; }
    public string PeppolParticipantIdentifier { set; get; }
    public string PostalCode { set; get; }
}
public class InexchangeBuyerParty
{
    public string CompanyId { get; set; }
    public string Name { get; set; }
    public string AltName { get; set; }
    public string StreetName { get; set; }
    public string PostBox { get; set; }
    public string City { get; set; }
    public string PostalZone { get; set; }
    public string CountryCode { get; set; }
    public string PhoneNo { get; set; }
    public string OrgNo { get; set; }
    public string VatNo { get; set; }
    public string GLN { get; set; }
    public List<string> PeppolParticipantIdentifiers { get; set; }
    public string Address { get; set; }
    public string Address2 { get; set; }
    public string PostalCode { get; set; }
    public bool Connected { get; set; }
    public string ReceiveElectronicInvoiceCapability { get; set; }
    public string SendsElectronicOrderCapability { get; set; }
}

public class InexchangeBuyerLookupResponse
{
    public int PotentialMatches { get; set; }
    public bool ExactHit { get; set; }
    public List<InexchangeBuyerParty> Parties { get; set; }
}

public class InexchangeSendDocumentAs
{
    public string Type { get; set; }
    public InexchangePaper Paper { get; set; }
    public InexchangeElectronic Electronic { get; set; }
    public InexchangePdf Pdf { get; set; }
    public InexchangeBusinessToConsumer BusinessToConsumer { get; set; }
}

public class InexchangeRecipientInformation
{
    public string GLN { get; set; }
    public string OrgNo { get; set; }
    public string VatNo { get; set; }
    public string Name { get; set; }
    public string RecipientNo { get; set; }
    public string CountryCode { get; set; }
}

public class InexchangeAddress
{
    public string Name { get; set; }
    public string Department { get; set; }
    public string StreetName { get; set; }
    public string PostBox { get; set; }
    public string PostalZone { get; set; }
    public string City { get; set; }
    public string CountryCode { get; set; }
}

public class InexchangePaper
{
    public InexchangeAddress RecipientAddress { get; set; }
    public InexchangeAddress ReturnAddress { get; set; }
}

public class InexchangeElectronic
{
    //InExchange Company Id of the recipient.
    public string RecipientId { get; set; }
}

public class InexchangePdf
{
    public string RecipientEmail { get; set; }
    public string RecipientName { get; set; }
    public string SenderEmail { get; set; }
    public string SenderName { get; set; }
}

public class InexchangeBusinessToConsumer
{
    public string FMI { get; set; }
    public string SSN { get; set; }
    public string Provider { get; set; }
}

#endregion

#region Inexchange Status objects
public class InexchangeDocumentStatusRequest
{
    public int Take { get; set; }
    public int Skip { get; set; }
    public DateTime? CreatedFrom { get; set; }
    public DateTime? CreatedTo { get; set; }
    public DateTime UpdatedAfter { get; set; }
    public string DocumentType { get; set; }
    public string Status { get; set; }
    public List<string> IgnoreStatuses { get; set; }
    public bool IncludeFileInfo { get; set; }
    public bool IncludeErrorInfo { get; set; }
}

public class InExchangeApiDocumentErrorStatus
{
    public string message { get; set; }
    public string status { get; set; }
}

public class InExchangeApiDocumentCurrentStatus
{
    public string status { get; set; }
    public DateTime time { get; set; }
}

public class InExchangeApiDocumentStatusResponse
{
    public List<InExchangeAPIDocumentStatus> documents { get; set; }
    public int totalCount { get; set; }
}

public class InExchangeAPIDocumentStatus
{
    public string id { get; set; }
    public string documentType { get; set; }
    public string erpDocumentId { get; set; }
    public InExchangeApiDocumentErrorStatus error { get; set; }
    public InExchangeApiDocumentCurrentStatus currentStatus { get; set; }

    public InExchangeStatusType SOEInExchangeStatusType
    {
        get
        {
            switch (currentStatus.status)
            {
                case "PendingInPlatform":
                case "Pending":
                    return InExchangeStatusType.PendingInPlatform;
                case "Sent":
                case "Delivered":
                    return InExchangeStatusType.Sent;
                case "Error":
                    return InExchangeStatusType.Error;
                case "Stopped":
                    return InExchangeStatusType.Stopped;
                default:
                    return InExchangeStatusType.Unknown;
            }
        }
    }
}
    #endregion

    #endregion

}
