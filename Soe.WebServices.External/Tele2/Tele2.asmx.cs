using System;
using System.Collections.Generic;
using System.Web.Services;
using System.Text;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Data;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Business.Util.Communicator;
using SoftOne.Communicator.Shared.DTO;

namespace Soe.WebServices.External
{

    // This is a copy of an old web service at bravetime. As little as possible is changed in the interface.

    /// <summary>
    /// Summary description for Tele2
    /// </summary>
    [WebService(Namespace = "http://bravetime.se/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    public class Tele2 : System.Web.Services.WebService
    {
        public Tele2()
        {

            //Uncomment the following line if using designed components 
            //InitializeComponent(); 
        }

        [WebMethod]
        public WebServiceResponse exportCommissionRequest(WebServiceAuthentication webserviceAuthentication, CommissionRequest commissionRequest)
        {
            //Set actorcompanyid to Tele2, Will not work for any other company.

            int actorCompanyId = 170244;
            int timeCodeId = 9331;
            int productId = 286705;
            string errorMessage = string.Empty;
            int imported = 0;


            using (CompEntities entities = new CompEntities())
            {
                int h = 0;

                try
                {
                    exportCommissionRequest exp = new exportCommissionRequest(); // (exportCommissionRequest)Deserialize(typeof(exportCommissionRequest), xml);
                    exp.commissionRequest = commissionRequest;
                    exp.webserviceAuthentication = webserviceAuthentication;

                    if (exp.webserviceAuthentication.password != Tele2Control())
                        return ReturnMessage(401);

                    CountryCurrencyManager countryCurrencyManager = new CountryCurrencyManager(null);
                    AttestManager attestManager = new AttestManager(null);
                    EmployeeManager employeeManager = new EmployeeManager(null);
                    AccountManager accountManager = new AccountManager(null);
                    ProductManager productManager = new ProductManager(null);
                    ParameterObject parameterObject = ParameterObject.Empty();
                    SettingManager settingManager = new SettingManager(null);
                    TimeBlockManager timeBlockManager = new TimeBlockManager(null);

                    int accountCostId = settingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.AccountEmployeeGroupCost, 0, actorCompanyId, 0);

                    //AccountStd accountStd = accountManager.GetAccountStdsByCompany(entities, actorCompanyId, false, false, true).Where(a => a.AccountTypeSysTermId == (int)ProductAccountType.Purchase).FirstOrDefault();

                    if (exp != null)
                    {
                        //Date bug
                        if (exp.commissionRequest.header.fromDate.Month < exp.commissionRequest.header.fromDate.AddDays(1).Month)
                            exp.commissionRequest.header.fromDate = exp.commissionRequest.header.fromDate.AddDays(1);

                        //Save to set timecode (id) and payrollproduct (id) (hardcoded)
                        foreach (employee emp in exp.commissionRequest.employee)
                        {
                            Employee employee = employeeManager.GetEmployeeByNr(entities, (!string.IsNullOrEmpty(emp.employeeNumberString) ? emp.employeeNumberString : emp.employeeNumber.ToString()), actorCompanyId, onlyActive: false);
                            if (employee == null)
                                continue;

                            TimeBlockDate timeBlockDate = timeBlockManager.GetTimeBlockDate(entities, actorCompanyId, employee.EmployeeId, exp.commissionRequest.header.fromDate, createfNotExist: true);

                            #region TimeCodeTransaction

                            h++;

                            ExpenseHead expenseHead = new ExpenseHead()
                            {
                                Start = exp.commissionRequest.header.fromDate,
                                Stop = exp.commissionRequest.header.toDate,
                                Comment = "",
                                Accounting = "",
                                State = (int)SoeEntityState.Active,

                                //Set FK
                                ActorCompanyId = actorCompanyId,

                                //Set references
                                EmployeeId = employee.EmployeeId,
                                TimeBlockDateId = timeBlockDate.TimeBlockDateId,
                                CreatedBy = "Job485"
                            };

                            ExpenseRow expenseRow = new ExpenseRow()
                            {

                                Start = CalendarUtility.MergeDateAndTime(CalendarUtility.DATETIME_DEFAULT, expenseHead.Start),
                                Stop = CalendarUtility.MergeDateAndTime(CalendarUtility.DATETIME_DEFAULT, expenseHead.Stop),
                                Quantity = 1,
                                Comment = "WinCash",
                                ExternalComment = "",
                                IsSpecifiedUnitPrice = true,
                                Accounting = "",
                                UnitPrice = 0,
                                UnitPriceCurrency = 0,
                                UnitPriceLedgerCurrency = 0,
                                UnitPriceEntCurrency = 0,
                                Amount = 0,
                                AmountCurrency = 0,
                                AmountLedgerCurrency = 0,
                                AmountEntCurrency = 0,
                                InvoicedAmount = 0,
                                InvoicedAmountCurrency = 0,
                                InvoicedAmountLedgerCurrency = 0,
                                InvoicedAmountEntCurrency = 0,
                                Vat = 0,
                                VatCurrency = 0,
                                VatLedgerCurrency = 0,
                                VatEntCurrency = 0,
                                CreatedBy = expenseHead.CreatedBy,
                                State = (int)SoeEntityState.Active,

                                //Set FK
                                ActorCompanyId = actorCompanyId,

                                EmployeeId = employee.EmployeeId,
                                TimeCodeId = timeCodeId,
                                TimePeriodId = null,
                                ExpenseHead = expenseHead
                            };

                            TimeCodeTransaction timeCodeTransaction = new TimeCodeTransaction()
                            {
                                TimeCodeId = timeCodeId,
                                Vat = 0,
                                Quantity = 1,
                                Type = 1,
                                Comment = "WinCash",
                                Amount = emp.amount,
                                Start = exp.commissionRequest.header.fromDate,
                                Stop = exp.commissionRequest.header.toDate,
                                ExpenseRow = new System.Data.Entity.Core.Objects.DataClasses.EntityCollection<ExpenseRow>() { expenseRow },
                                IsAdditionOrDeduction = true,
                            };

                            //Set currency amounts
                            countryCurrencyManager.SetCurrencyAmounts(entities, actorCompanyId, timeCodeTransaction);

                            #endregion

                            #region TimePayrollTransaction

                            AccountingPrioDTO prioDTO = accountManager.GetPayrollProductAccount(ProductAccountType.Purchase, actorCompanyId, employee, productId, 0, 0, false, (DateTime?)exp.commissionRequest.header.fromDate);
                            int accountId = prioDTO != null && prioDTO.AccountId.HasValue && prioDTO.AccountId.Value != 0 ? prioDTO.AccountId.Value : accountCostId;

                            AccountDim accountDimCostCentre = accountManager.GetAccountDimBySieNr(entities, (int)TermGroup_SieAccountDim.CostCentre, actorCompanyId);
                            List<Account> accountInternals = accountManager.GetAccountsByDim(entities, accountDimCostCentre.AccountDimId, actorCompanyId, true, true, true);

                            TimePayrollTransaction timePayrollTransaction = new TimePayrollTransaction()
                            {
                                Amount = emp.amount,
                                VatAmount = 0,
                                Quantity = 1,
                                IsPreliminary = false,
                                ManuallyAdded = false,
                                Exported = false,
                                AutoAttestFailed = false,
                                Comment = null,
                                IsAdditionOrDeduction = true,
                                Created = DateTime.Now,
                                CreatedBy = "WinCash",

                                //Refereces
                                TimeCodeTransaction = timeCodeTransaction,
                                TimeBlockDate = timeBlockDate,

                                //Set FK
                                ProductId = productId,
                                EmployeeId = employee.EmployeeId,
                                AttestStateId = attestManager.GetInitialAttestStateId(actorCompanyId, TermGroup_AttestEntity.PayrollTime),
                                AccountStdId = accountId,
                                ActorCompanyId = actorCompanyId,

                            };


                            //SysPayrollTypeLevel1
                            PayrollProduct payrollProduct = productManager.GetPayrollProduct(entities, timePayrollTransaction.ProductId);
                            timePayrollTransaction.SysPayrollTypeLevel1 = payrollProduct.SysPayrollTypeLevel1;
                            timePayrollTransaction.SysPayrollTypeLevel2 = payrollProduct.SysPayrollTypeLevel2;
                            timePayrollTransaction.SysPayrollTypeLevel3 = payrollProduct.SysPayrollTypeLevel3;
                            timePayrollTransaction.SysPayrollTypeLevel4 = payrollProduct.SysPayrollTypeLevel4;

                            if (emp.storeNo != null && accountDimCostCentre != null && emp.storeNo != "")
                            {
                                //AccountInternals for PayrollTransactions that not corresponds to a Category (i.e. was converted in GetWtCategories) will be skipped
                                AccountInternal accountInternal = accountInternals.Where(i => i.AccountNr == emp.storeNo).Select(i => i.AccountInternal).FirstOrDefault();
                                if (accountInternal != null)
                                    timePayrollTransaction.AccountInternal.Add(accountInternal);
                            }

                            //Set currency amounts
                            countryCurrencyManager.SetCurrencyAmounts(entities, actorCompanyId, timePayrollTransaction);

                            //Add to Collection
                            timeCodeTransaction.TimePayrollTransaction.Add(timePayrollTransaction);
                            //timeCodeTransactions.Add(timeCodeTransaction);

                            //Save entity
                            entities.TimeCodeTransaction.AddObject(timeCodeTransaction);

                            entities.SaveChanges();
                            imported++;
                        }

                        #endregion

                        //Add to collection

                        //Return OK
                        return ReturnMessage(200);
                    }
                    else
                    {
                        return ReturnMessage(500);
                    }
                }
                catch (Exception ex)
                {
                    errorMessage = $"Error occured. Number of employees with imported values before failure: {imported}";
                    SysLogManager slm = new SysLogManager(null);
                    slm.AddSysLog(ex, log4net.Core.Level.Error);
                    return ReturnMessage(500);
                }
                finally
                {
                    try
                    {
                        MailMessageDTO mailMessageDTO = new MailMessageDTO();
                        List<string> recievers = new List<string>();
                        recievers.Add("Tele2Butiken_IT@tele2.com");
                        mailMessageDTO.SenderEmail = "noreply@softone.se";
                        mailMessageDTO.SenderName = "noreply@softone.se";
                        mailMessageDTO.subject = "SoftOne Endpoint ExportCommissionRequest Executed";
                        mailMessageDTO.recievers = recievers;
                        mailMessageDTO.cc = new List<string>();
                        mailMessageDTO.body = $"SoftOne Endpoint ExportCommissionRequest Executed. " + (string.IsNullOrEmpty(errorMessage) ? $"Number of imported transactions {imported}" : errorMessage);
                        mailMessageDTO.EmailcontentIsHtml = false;
                        mailMessageDTO.MessageAttachmentDTOs = new List<MessageAttachmentDTO>();
                        CommunicatorConnector.SendMailMessage(mailMessageDTO);
                    }
                    catch 
                    {
                        // Do nothing
                        // We don't want to throw exceptions from the finally block
                        // NOSONAR
                    }
                }
            }
        }

        private static string Tele2Control()
        {
            return "2eleT";
        }

        public static object Deserialize(Type TypeToDeserialize, string xmlString)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(xmlString);
            MemoryStream mem = new MemoryStream(bytes);
            System.Xml.Serialization.XmlSerializer ser = new System.Xml.Serialization.XmlSerializer(TypeToDeserialize);
            return ser.Deserialize(mem);
        }

        private WebServiceResponse ReturnMessage(int ResponseCode)
        {
            WebServiceResponse response = new WebServiceResponse();

            switch (ResponseCode)
            {
                case 200:
                    response.responseCode = 200;
                    response.responseMessage = "OK";
                    break;
                case 401:
                    response.responseCode = 401;
                    response.responseMessage = "Unauthorized";
                    break;
                default:
                    response.responseCode = 500;
                    response.responseMessage = "Internal server error";
                    break;
            }
            //Unclear of the meaning of this?
            response.authenticationKey = "1234567890";

            return response;
        }
    }

    [Serializable()]
    [System.Xml.Serialization.XmlRootAttribute("exportCommissionRequest", Namespace = "http://bravetime.se/")]
    public class exportCommissionRequest
    {
        private CommissionRequest _commissionRequest;
        private WebServiceAuthentication _webserviceAuthentication;

        [XmlElement("commissionRequest")]
        public CommissionRequest commissionRequest
        {
            get
            {
                return _commissionRequest;
            }
            set
            {
                _commissionRequest = value;
            }
        }

        [XmlElement("webserviceAuthentication")]
        public WebServiceAuthentication webserviceAuthentication
        {
            get
            {
                return _webserviceAuthentication;
            }
            set
            {
                _webserviceAuthentication = value;
            }
        }

    }

    [Serializable()]
    [XmlRoot("webserviceAuthentication")]
    public class WebServiceAuthentication
    {
        private int _companyID;
        private string _password;

        [XmlElement("companyID")]
        public int companyID
        {
            get
            {
                return _companyID;
            }
            set
            {
                _companyID = value;
            }
        }

        [XmlElement("password")]
        public string password
        {
            get
            {
                return _password;
            }
            set
            {
                _password = value;
            }
        }
    }

    [Serializable()]
    [XmlRoot("header")]
    public class Header
    {
        private string _intervaceVersion;
        private string _buildNumber;
        private string _senderID;
        private int _messageID;
        private DateTime _fromDate;
        private DateTime _toDate;
        private DateTime _exportDate;
        private int _numberOfEmployees;
        private decimal _totalAmount;

        public string intervaceVersion
        {
            get
            {
                return _intervaceVersion;
            }
            set
            {
                _intervaceVersion = value;
            }
        }

        public string buildNumber
        {
            get
            {
                return _buildNumber;
            }
            set
            {
                _buildNumber = value;
            }
        }

        public string senderID
        {
            get
            {
                return _senderID;
            }
            set
            {
                _senderID = value;
            }
        }

        public int messageID
        {
            get
            {
                return _messageID;
            }
            set
            {
                _messageID = value;
            }
        }

        [XmlElement("fromDate")]
        public DateTime fromDate
        {
            get
            {
                return _fromDate;
            }
            set
            {
                _fromDate = value;
            }
        }

        [XmlElement("toDate")]
        public DateTime toDate
        {
            get
            {
                return _toDate;
            }
            set
            {
                _toDate = value;
            }
        }

        [XmlElement("exportDate")]
        public DateTime exportDate
        {
            get
            {
                return _exportDate;
            }
            set
            {
                _exportDate = value;
            }
        }

        public int numberOfEmployees
        {
            get
            {
                return _numberOfEmployees;
            }
            set
            {
                _numberOfEmployees = value;
            }
        }

        public decimal totalAmount
        {
            get
            {
                return _totalAmount;
            }
            set
            {
                _totalAmount = value;
            }
        }

    }

    [Serializable()]
    public class employee
    {
        private string _employeeNumberString;
        private int _employeeNumber;
        private decimal _amount;
        private string _storeNo;

        [XmlElement("employeeNumberString")]
        public string employeeNumberString
        {
            get
            {
                return _employeeNumberString;
            }
            set
            {
                _employeeNumberString = value;
            }
        }

        [XmlElement("employeeNumber")]
        public int employeeNumber
        {
            get
            {
                return _employeeNumber;
            }
            set
            {
                _employeeNumber = value;
            }
        }

        [XmlElement("amount")]
        public decimal amount
        {
            get
            {
                return _amount;
            }
            set
            {
                _amount = value;
            }
        }

        [XmlElement("storeNo")]
        public string storeNo
        {
            get
            {
                return _storeNo;
            }
            set
            {
                _storeNo = value;
            }
        }

    }

    [Serializable()]
    [XmlRoot("commissionRequest")]
    public class CommissionRequest
    {
        private employee[] _employee;
        private Header _header;

        [XmlArray("employees")]
        [XmlArrayItem("employee", typeof(employee))]
        public employee[] employee
        {
            get
            {
                return _employee;
            }
            set
            {
                _employee = value;
            }
        }

        [XmlElement("header")]
        public Header header
        {
            get
            {
                return _header;
            }
            set
            {
                _header = value;
            }
        }
    }

    public class WebServiceResponse
    {
        private int _responseCode;
        private string _responseMessage;
        private string _authenticationKey;

        public int responseCode
        {
            get
            {
                return _responseCode;
            }
            set
            {
                _responseCode = value;
            }
        }

        public string responseMessage
        {
            get
            {
                return _responseMessage;
            }
            set
            {
                _responseMessage = value;
            }
        }

        public string authenticationKey
        {
            get
            {
                return _authenticationKey;
            }
            set
            {
                _authenticationKey = value;
            }
        }

    }
}

