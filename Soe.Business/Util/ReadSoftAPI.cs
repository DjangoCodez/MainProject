using ReadSoft.Services;
using ReadSoft.Services.Entities;
using SoftOne.Soe.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Util
{
    public class ReadSoftAPI
    {
        #region ReadMe

        /**
        
        FieldName	        Format	                        Kommentar	                        Type	                        Aktiverad ReadSoft	Aktiverad XE
        Faktura/Kredit	    X(1-20)		                                                        creditinvoice	                JA	                JA
        Fakturanummer	    X(3-15)	                        Nummeriskt, VERSALER, Specialtecken	invoicenumber	                JA	                JA
        Fakturadatum	    YYYYMMDD	                                                        invoicedate	                    JA	                JA
        Förfallodatum	    YYYYMMDD	                                                        invoiceduedate	                JA	                JA
        Ordernummer	        A(0-4)N(3-20)	                Nummeriskt, VERSALER	            invoiceordernumber	            JA	                JA
          **  ErReferens/buyercontactpersonname field is used for projectnumber **
        ErReferens	        A(1-15)X[-](0-1)A(1-15)	        VERSALER, gemena och "-"	        buyercontactpersonname	        JA	                JA                        
        Referensnummer	    N(1-15)		                                                        buyercontactreference	        JA	                JA
	    Emballagekostnad	N(7).NN	                                                            belopp	deliverycostpackaging	NEJ	                NEJ
	    Rabatt	            N(7).NN	                                                            belopp	Invoicediscountamount	NEJ	                NEJ
	    Rabattsats	        N(1-2)X[.,](0-1)N(0-2)X[%](0-1)	rabattsats med eller utan % tecken	Invoicediscountpercent	        NEJ	                NEJ
        Nettobelopp	        N(7).NN	                        belopp	                            invoicetotalvatexcludedamount	JA	                JA
        Momssats	        N(1-2)X[.,](0-1)N(0-2)X[%](0-1)	momssats med eller utan % tecken	invoicetotalvatratepercent	    JA	                JA
        Momsbelopp	        N(7).NN	                        belopp	                            invoicetotalvatamount	        JA	                JA
        Fraktkostnad	    N(7).NN	belopp	                                                    deliverycost	                JA	                JA
        Bruttobelopp	    N(7).NN	belopp	                                                    invoicetotalvatincludedamount	JA	                JA
        Valuta	            A(3)		                                                        invoicecurrency             	JA	                JA
        OCRNummer	        N(3-25)		                                                        paymentreferencenumber	        JA	                JA
        Plusgiro	        N(3-8)		                                                        supplieraccountnumber1	        JA	                JA
        Bankgiro	        N(7-8)		                                                        supplieraccountnumber2	        JA	                JA
        Bankkonto	        N(3-35)		                                                        supplierbankcodenumber1	        NEJ	                JA
        IBAN	            A(2)N(2)X(10-30)		                                            supplieriban1	                JA	                NEJ
        Organisationsnummer	N(10-12)		                                                    suppliertaxnumber1	            JA	                JA
        VATNummer	        A(2)N(10-12)		                                                suppliervatregistrationnumber	JA	                JA
        AmountRounding		                                                                    amountrounding	                JA	                JA
                
        **/

        #endregion

        #region Constants

        //Logins
        private const string userName = "xe.softone"; //"SoftOne";//old solution: Readsoft
        public static string UserName
        {
            get
            {
                return userName;
            }
        }
        private const string dgk = "DGkqieKET.}({p"; //old solution: F_l!P0KF_l!P0K
        public static string DGK
        {
            get
            {
                return dgk;
            }
        }
        public const string applicationKey = "9b350ef826664fdaa1f95750c4aa36c6";
        private const string url = "https://services.readsoftonline.com";
        private const string email = "softonetest-invoices@mail.softone.readsoftonline.com";
        public static string Email
        {
            get
            {
                return email;
            }
        }

        //Formats
        private const string dateFormat = "yyyyMMdd";
        public static string DateFormat
        {
            get
            {
                return dateFormat;
            }
        }

        #endregion

        #region Public methods
        /*
        public static List<ReadSoftMessage> GetAllMessages()
        {
            List<ReadSoftMessage> messages = new List<ReadSoftMessage>();

            var clientConfiguration = CreateClientConfiguration();
            if (clientConfiguration != null)
            {
                var authenticationClient = new AuthenticationServiceClient(clientConfiguration);
                var result = Login(authenticationClient);
                if (result.Status == AuthenticationStatus.Success)
                {
                    var accountClient = new AccountServiceClient(clientConfiguration);
                    var currentAccount = accountClient.GetCurrentAccount();
                    if (currentAccount != null)
                    {
                        var documentClient = new DocumentServiceClient(clientConfiguration);
                        var documentReferenceCollection = documentClient.GetOutputDocumentsByAccount(new Guid(currentAccount.Id));
                        messages = ParseDocument(documentClient, documentReferenceCollection);
                    }
                }

                authenticationClient.SignOut();
            }

            return messages;
        }
        */
        private ClientConfiguration clientConfiguration = null;
        private AuthenticationServiceClient authenticationClient = null;
        public bool Login()
        {
            clientConfiguration = ClientConfiguration.Create(new Guid(applicationKey), new Uri(url));
            if (clientConfiguration != null)
            {
                authenticationClient = new AuthenticationServiceClient(clientConfiguration);
                var loginResult = authenticationClient.Authenticate(new AuthenticationCredentials
                {
                    AuthenticationType = AuthenticationType.SetCookie,
                    UserName = UserName,
                    Password = DGK,
                });

                if (loginResult.Status == AuthenticationStatus.Success)
                {
                    return true;
                }
                else
                {
                    authenticationClient = null;
                    clientConfiguration = null;
                }
            }

            return false;
        }

        public AuthenticationResult Logout()
        {
            if (authenticationClient != null)
            {
                return authenticationClient.SignOut();
            }
            else
            {
                return null;
            }
        }

        public List<ReadSoftMessage> GetMessages(string apiKey, bool getDocumentsByOrganisation = true)
        {
            List<ReadSoftMessage> messages = new List<ReadSoftMessage>();

            if (string.IsNullOrEmpty(apiKey))
                return messages;

            //var clientConfiguration = CreateClientConfiguration();
            if (clientConfiguration != null)
            {
                //var authenticationClient = new AuthenticationServiceClient(clientConfiguration);
                //var loginResult = Login(authenticationClient);
                //if (loginResult.Status == AuthenticationStatus.Success)
                //{
                    var accountClient = new AccountServiceClient(clientConfiguration);
                    var customers = accountClient.GetAllCustomers();

                    if (getDocumentsByOrganisation)
                    {
                        #region Get documents by organisation

                        /*
                         * ReadSoft API does not provide functionality to get documents for a organisation (buyer)
                         * Because of that we need to go throug all Customers, and for each customer check all theire organisations.
                         * When we have more XE customers we need a more optimized way to do this.
                         * 
                         * We only have a API-key per Company (which maps to a organisation in ReadSoft)
                         * If we had one API-key per License we could get documents faster, but will cause more administration in XE.                     * 
                         */
                                                
                        foreach (var customer in customers)
                        {
                            var buyers = accountClient.GetAllBuyersByCustomer(customer.Id);
                            var buyer = buyers.FirstOrDefault(i => i.ExternalId == apiKey);
                            if (buyer != null)
                            {
                                var documentClient = new DocumentServiceClient(clientConfiguration);
                                var documentReferenceCollection = documentClient.GetOutputDocumentsByBuyer(new Guid(buyer.Id));
                                messages = ParseDocument(documentClient, documentReferenceCollection);
                            }
                        }

                        #endregion
                    }
                    else
                    {
                        #region Get documents by customer

                        var customer = customers.FirstOrDefault(i => i.ExternalId == apiKey);
                        if (customer != null)
                        {
                            var documentClient = new DocumentServiceClient(clientConfiguration);
                            var documentReferenceCollection = documentClient.GetOutputDocuments(new Guid(customer.Id));
                            messages = ParseDocument(documentClient, documentReferenceCollection);
                        }

                        #endregion
                    }
                //}

                //authenticationClient.SignOut();
            }

            return messages;
        }
        
        public bool SetDocumentStatus(string documentId, bool success)
        {
            bool result = false;

            //var clientConfiguration = CreateClientConfiguration();
            if (clientConfiguration != null)
            {
              //  var authenticationClient = new AuthenticationServiceClient(clientConfiguration);
               // var loginResult = Login(authenticationClient);
               // if (loginResult.Status == AuthenticationStatus.Success)
               // {
                    var documentClient = new DocumentServiceClient(clientConfiguration);
                    var outputResult = new OutputResult()
                    {
                        CodingLines = new CodingLineCollection(),
                        CorrelationData = null,
                        ExternalId = null,
                        Message = success ? "Posted in the ERP system" : "Failed to post to the ERP system",
                        Status = success ? OutputStatus.Success : OutputStatus.Failure,
                    };
                    var boolValue = documentClient.DocumentStatus(documentId, outputResult);
                    result = boolValue != null && boolValue.Value;
                //}

                //authenticationClient.SignOut();
            }

            return result;
        }

        public bool LearnDocument(string documentId, Document document)
        {
            bool result = false;

            //var clientConfiguration = CreateClientConfiguration();
            if (clientConfiguration != null)
            {
                //var authenticationClient = new AuthenticationServiceClient(clientConfiguration);
                //var loginResult = Login(authenticationClient);
                //if (loginResult.Status == AuthenticationStatus.Success)
                //{
                    var documentClient = new DocumentServiceClient(clientConfiguration);
                    var boolValue = documentClient.LearnDocument(documentId, document);
                    result = boolValue != null && boolValue.Value;
                //}

                //authenticationClient.SignOut();
            }

            return result;
        }

        public bool SendFeedback(string documentId, string feedbackType, string message)
        {
            bool result = false;

            //var clientConfiguration = CreateClientConfiguration();
            if (clientConfiguration != null)
            {
              //  var authenticationClient = new AuthenticationServiceClient(clientConfiguration);
               // var loginResult = Login(authenticationClient);
               // if (loginResult.Status == AuthenticationStatus.Success)
               // {
                    var accountClient = new AccountServiceClient(clientConfiguration);
                    var currentAccount = accountClient.GetCurrentAccount();
                    if (currentAccount != null)
                    {
                        var feedbackClient = new CustomerFeedbackServiceClient(clientConfiguration);
                        BoolValue feedbackResult = feedbackClient.AddFeedback(documentId, feedbackType, message);
                        result = feedbackResult.Value;
                    }
                //}

                //authenticationClient.SignOut();
            }

            return result;

        }

        #endregion

        #region Help methods

        #region Authentication
/*

        private static ClientConfiguration CreateClientConfiguration()
        {
            return ClientConfiguration.Create(new Guid(applicationKey), new Uri(url));
        }

        private static AuthenticationResult Login(AuthenticationServiceClient client)
        {
            if (client == null)
                return new AuthenticationResult() { Status = AuthenticationStatus.Failed };

            return client.Authenticate(new AuthenticationCredentials 
            { 
                AuthenticationType = AuthenticationType.SetCookie,
                UserName = UserName,
                Password = Password,
            });
        }
        */
        #endregion
        
        #region Document

        private List<ReadSoftMessage> ParseDocument(DocumentServiceClient client, DocumentReferenceCollection documentReferenceCollection)
        {
            List<ReadSoftMessage> messages = new List<ReadSoftMessage>();

            if (client == null || documentReferenceCollection == null)
                return messages;

            foreach (var documentReference in documentReferenceCollection)
            {            
                string contentType;
                var image = client.GetDocumentOutputImage(documentReference.DocumentId, out contentType);
                var document = client.GetDocument(documentReference.DocumentId);
                if (document != null)
                {
                    messages.Add(new ReadSoftMessage()
                    {
                        BatchId = documentReference.BatchId,
                        CreatedBy = ReadSoftAPI.Email,
                        ReceiveTime = documentReference.CompletionTime,
                        Document = document,
                        Image = ZipUtility.GetDataFromStream(image),
                    });
                }
            }

            return messages;
        }

        #endregion

        #endregion
    }
}
