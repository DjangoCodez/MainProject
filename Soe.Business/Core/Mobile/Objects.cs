using SoftOne.Communicator.Shared.DTO;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Common.Util.Logger;
using SoftOne.Soe.Data;
using SoftOne.Soe.Util;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Core.Mobile.Objects
{
    #region Enumerations

    public enum MobileDisplayMode
    {
        User = 1,
        Admin = 2,
    }

    public enum MobileShiftGUIType
    {
        MyShifts = 1, //Deprecated
        AvailableShifts = 2, //Deprecated
        OtherShifts = 3,    //Deprecated
        AbsenceRequested = 4,//Deprecated
        AbsenceApproved = 5,//Deprecated
        InterestRequested = 6,//Deprecated
        MyShiftFlow = 7,
        MyTemplateShifts = 8,
        MyShiftsNew = 9,
        AvailableShiftsNew = 10,
        OtherShiftsNew = 11,
        SwapShift = 12,
    }

    public enum MobileViewType
    {
        MyTemplateMonth = 1,
        MyScheduleMonth = 2,
        OverviewWeekEmployee = 3,
        OverviewWeekAdmin = 4,
        DayViewAdmin = 5,
        DayViewEmployee = 6,
    }

    public enum MobileModuleType
    {
        Undefined = 0,
        Order = 1,
        TimeUser = 2,
        StaffingUser = 3,
        Customers = 4,
        XEMail = 5,
        InternalNews = 6,
        Preferences = 7,
        FileArchive = 8,
        Collegues = 9,
        SalarySpecifikations = 10,
        SupplierInvoiceAttest = 11,
        StateAnalysis = 12,
        TimeSheet = 13,
        DemoPriceSearch = 14,
        DemoVideos = 15,
        StaffingAdmin = 16,
        TimeAdmin = 17,
        TimeStampAttendance = 18,
        StaffingAdminAbsenceRequests = 19,
        TemplateScheduleEmployee = 20,
        Accumulators = 21,
        TimeStampInApp = 22,
        EvacuationList = 23,
    }

    public enum MobileTask
    {
        Unknown = 0,

        //Generell
        UserCompanyRoleIsValid = 1,
        ChangePWD = 2,
        UpdateSetting = 3,
        GetMessage = 4,
        Update = 5,
        Save = 6,
        Delete = 7,
        GetValue = 8,

        //Order 101-200
        SaveOrder = 101,
        SetOrderReady = 102,
        AddOrderImage = 103,
        DeleteOrderImage = 104,
        EditOrderImage = 105,
        SaveOriginUsers = 106,
        SaveChecklistAnswers = 107,
        AddCheckList = 108,
        SaveWorkingDescription = 109,
        SetOrderRowsAsReady = 110,
        ChangeProjectOnOrder = 111,
        DeleteOrderChecklist = 112,
        SavePlanningData = 113,
        SetOrderIsReady = 114,

        //OrderRow 201-400
        SaveOrderRow = 201,
        DeleteOrderRow = 202,
        SaveHouseholdDeductionRow = 203,
        CopyOrderRow = 204,
        SaveHouseholdDeductionApplicant = 205,
        DeleteHouseholdDeductionApplicant = 206,

        //Expenses 270 ->
        SaveExpense = 271,
        DeleteExpense = 272,

        //TimeRow (and ProjectTimeBlock) 301-400
        SaveTimeRow = 301,
        GetEmployeeFirstEligableTime = 302,
        SaveProjectTimeBlock = 303,
        DeleteProjectTimeBlock = 304,
        MoveProjectTimeBlockToDate = 305,

        //Deviations
        SaveDeviations = 401,
        SaveAbsence = 402,
        SaveBreak = 403,
        SavePresence = 404,
        RestoreToSchedule = 405,

        //Attest
        SaveAttest = 501,


        //Customer
        SaveCustomer = 601,

        //Misc
        SaveMapLocation = 701,
        SaveEmployeeDetails = 702,
        SaveEmployeeUserSettings = 703,

        //Staffing
        SetShiftAsUnWanted = 801,
        SetShiftAsWanted = 802,
        SaveEmployeeRequest = 803,
        DeleteEmployeeRequest = 804,
        UpdateShiftSetUndoWanted = 805,
        UpdateShiftSetUndoUnWanted = 806,
        SaveAbsenceAnnouncement = 807,
        SaveShifts = 808,
        DeleteShift = 809,
        SaveAbsencePlanning = 810,
        AssignAvailableShift = 811,
        SaveAbsenceRequestPlanning = 811,

        //XEMail
        MarkMailAsRead = 901,
        DeleteIncomingMail = 902,
        AnswerShiftRequest = 903,
        SendMail = 904,
        SendNeedsConfirmationAnswer = 905,
        MarkMailAsUnread = 906,

        //SupplierInvoice
        SaveAttestWorkFlowAnswer = 1001,
        SaveAttestAccountRow = 1002,

        //Orderplanning
        SaveOrderShift = 1101,
    }

    public enum MobileInvoiceType
    {
        Unknown = 0,
        SupplierInvoice = 1,
        CustomerInvoice = 2
    };

    public enum MobileOriginType
    {
        //OBS! Changes here also affects SQL-views!

        None = 0,
        SupplierInvoice = 1,
        CustomerInvoice = 2,
        SupplierPayment = 3,
        CustomerPayment = 4,
        Offer = 5,
        Order = 6,
        Contract = 7,
    };

    public enum MobilePresenceAbsenceDetails
    {
        PresenceInsideSchedule = 1,
        PresenceOutsideSchedule = 2,
        Absence = 3,
        Standby = 4,
        Expense = 5,
    }

    #endregion

    #region Public parameters

    /// <summary>
    /// Includes parameters that are mandatory for all service calls
    /// </summary>
    public sealed class MobileParam
    {
        #region Variables

        private readonly int userId;
        public int UserId
        {
            get { return userId; }
        }

        private readonly int roleId;
        public int RoleId
        {
            get { return roleId; }
        }

        private readonly int actorCompanyId;
        public int ActorCompanyId
        {
            get { return actorCompanyId; }
        }

        public bool Validate { private set; get; }

        public string Version { private set; get; }
        public MobileDeviceType MobileDeviceType { private set; get; }

        #endregion

        #region Ctor

        public MobileParam(int userId, int roleId, int actorCompanyId, string version, bool validate = true)
        {
            if (version == null)
                version = "";

            this.userId = userId;
            this.roleId = roleId;
            this.actorCompanyId = actorCompanyId;
            Version = version;
            Validate = validate;
        }

        public MobileParam(int userId, int roleId, int actorCompanyId, string version, MobileDeviceType mobileDeviceType) : this(userId, roleId, actorCompanyId, version)
        {
            MobileDeviceType = mobileDeviceType;
        }

        #endregion
    }

    #endregion

    #region General messages

    public static class MobileMessages
    {
        #region Constants

        public const string ROOTNAME_LOGIN = "Login";
        public const string ROOTNAME_STARTUP = "Startup";
        public const string XML_ELEMENT_USERID = "UserId";
        public const string XML_ELEMENT_ROLEID = "RoleId";
        public const string XML_ELEMENT_COMPANYID = "CompanyId";
        public const string XML_ELEMENT_CHANGEPWD = "ChangePWD";
        public const string XML_ELEMENT_EMAILISMISSING = "EIM";
        public const string XML_ELEMENT_SUCCESS = "Success";
        public const string XML_ELEMENT_URL = "Url";
        public const string XML_ELEMENT_DOMAIN = "Domain";
        public const string XML_ELEMENT_PNID = "PNId";
        public const string XML_ELEMENT_UAH = "UAH";
        public const string XML_ELEMENT_AHSV = "AHSV";
        public const string XML_ELEMENT_MODULEPERMISSIONS = "Modules";

        public const string XML_ELEMENT_ERRORMESSAGE = "ErrorMessage";
        public const string XML_ELEMENT_MESSAGE = "Message";
        public const string XML_ELEMENT_ORDERID = "OrderId";
        public const string XML_ELEMENT_EXPENSEROWID = "ExpenseRowId";

        #endregion

        #region XDocument

        public static XDocument GetLoginMessageDocument(MobileParam param, bool changePWD, string pnid, bool emailIsMissing, string url, bool useAccountHierarchy, string accountHierarchySettingAccountNames, string domain)
        {
            var elements = new List<XElement>()
            {
                new XElement(XML_ELEMENT_USERID, param.UserId.ToString()),
                new XElement(XML_ELEMENT_ROLEID, param.RoleId.ToString()),
                new XElement(XML_ELEMENT_COMPANYID, param.ActorCompanyId.ToString()),
                new XElement(XML_ELEMENT_CHANGEPWD, StringUtility.GetString(changePWD)),
                new XElement(XML_ELEMENT_EMAILISMISSING, StringUtility.GetString(emailIsMissing)),
                new XElement(XML_ELEMENT_URL, url),
                new XElement(XML_ELEMENT_PNID, pnid),
                new XElement(XML_ELEMENT_UAH, StringUtility.GetString(useAccountHierarchy)),                
                new XElement(XML_ELEMENT_AHSV, accountHierarchySettingAccountNames),
                new XElement(XML_ELEMENT_DOMAIN, domain),
            };

            return XmlUtil.CreateDocument(ROOTNAME_LOGIN, elements);
        }

        public static XDocument GetStartUpSuccessDocument(bool success, bool changePWD)
        {
            int successValue = 1;
            if (changePWD)
                successValue = 2;

            return XmlUtil.CreateDocument(XML_ELEMENT_SUCCESS, success ? successValue.ToString() : "0");
        }

        public static XDocument GetStartUpDocument(bool success, bool changePWD, string pnid, bool emailIsMissing, string url, bool useAccountHierarchy, string accountHierarchySettingAccountNames, string domain)
        {
            int successValue = 1;
            if (changePWD)
                successValue = 2;
            if (emailIsMissing)
                successValue = 3;

            var elements = new List<XElement>()
            {
                new XElement(XML_ELEMENT_SUCCESS, success ? successValue.ToString() : "0"),
                new XElement(XML_ELEMENT_URL, url),
                new XElement(XML_ELEMENT_PNID, pnid),
                new XElement(XML_ELEMENT_UAH, StringUtility.GetString(useAccountHierarchy)),
                new XElement(XML_ELEMENT_AHSV, accountHierarchySettingAccountNames),
                new XElement(XML_ELEMENT_DOMAIN, domain),

            };

            return XmlUtil.CreateDocument(ROOTNAME_STARTUP, elements);
        }

        public static XDocument GetSuccessDocument(bool success)
        {
            return XmlUtil.CreateDocument(XML_ELEMENT_SUCCESS, success ? "1" : "0");
        }

        public static XDocument GetErrorMessageDocument(string errorMessage)
        {
            return XmlUtil.CreateDocument(XML_ELEMENT_ERRORMESSAGE, errorMessage);
        }

        public static XDocument GetIntMessageDocument(string element, int value)
        {
            return GetStringMessageDocument(element, value.ToString());
        }
        public static XDocument GetMessageDocument(string value)
        {
            return MobileMessages.GetStringMessageDocument(MobileMessages.XML_ELEMENT_MESSAGE, value);
        }

        public static XDocument GetStringMessageDocument(string element, string value)
        {
            return XmlUtil.CreateDocument(element, value);
        }

        #endregion

        #region String

        public static string GetDefaultLoginXml()
        {
            string xml =
                "<UserId>1</UserId>" +
                "<RoleId>1</RoleId>" +
                "<CompanyId>1</CompanyId>";

            return XmlUtil.CreateXml(ROOTNAME_LOGIN, xml);
        }

        public static string GetDefaultStartupXml()
        {
            string xml =
                "<UserId>1</UserId>" +
                "<RoleId>1</RoleId>" +
                "<CompanyId>1</CompanyId>";

            return XmlUtil.CreateXml(ROOTNAME_LOGIN, xml);
        }

        public static string GetDefaultLogoutXml()
        {
            string xml =
                "<Success>1</Success>";

            return xml;
        }

        #endregion
    }

    #endregion

    #region Base

    /// <summary>
    /// Base class
    /// </summary>
    internal abstract class MobileBase
    {
        #region Constants

        protected const string COLOR_ORANGE = "#FFF1801C";
        protected const string NOT_IMPLEMENTED = "NOT IMPLEMENTED";
        protected const string CDATASTART = "<![CDATA[ ";
        protected const string CDATAEND = " ]]>";

        #endregion

        #region Variables

        readonly public MobileManagerUtil mobileManagerUtil = new MobileManagerUtil();

        #region Collections

        private Dictionary<MobileTask, bool> tasksResults;

        #endregion

        #region Params

        private readonly MobileParam param;
        public MobileParam Param
        {
            get { return param; }
        }
        protected int UserId
        {
            get { return this.Param != null ? this.Param.UserId : 0; }
        }
        protected int ActorCompanyId
        {
            get { return this.Param != null ? this.Param.ActorCompanyId : 0; }
        }
        protected int RoleId
        {
            get { return this.Param != null ? this.Param.RoleId : 0; }
        }

        #endregion

        #region Error

        private readonly string errorMessage;
        public string ErrorMessage
        {
            get { return errorMessage; }
        }

        public bool Failed
        {
            get
            {
                return !String.IsNullOrEmpty(this.ErrorMessage);
            }
        }
        public bool Succeeded
        {
            get
            {
                return !this.Failed;
            }
        }

        #endregion

        #endregion

        #region Ctor

        protected MobileBase()
        {

        }

        /// <summary>Used for failed service calls. (Not logged in)</summary>
        protected MobileBase(string errorMessage)
        {
            this.errorMessage = errorMessage;
        }

        /// <summary>Used to identify user. (Logged in).</summary>
        protected MobileBase(MobileParam param)
        {
            this.param = param;
        }

        /// <summary>Used for failed service calls. (Logged in)</summary>
        protected MobileBase(MobileParam param, string errorMessage)
        {
            this.param = param;
            this.errorMessage = errorMessage;
        }

        #endregion

        #region Public methods

        public void SetTaskResult(MobileTask task, bool result)
        {
            if (tasksResults == null)
                tasksResults = new Dictionary<MobileTask, bool>();

            if (tasksResults.ContainsKey(task))
                tasksResults[task] = result;
            else
                tasksResults.Add(task, result);
        }

        public bool GetTaskResult(MobileTask task)
        {
            if (tasksResults == null)
                return false;

            return tasksResults[task];
        }

        #endregion

        #region Protected methods

        protected XDocument CreateDocument(string rootName, List<XElement> elements, List<XElement> rootElements = null)
        {
            //Init document
            XDocument document = XmlUtil.CreateDocument();

            //Root
            XElement root = new XElement(rootName);
            if (rootElements != null)
            {
                foreach (var element in rootElements)
                {
                    root.Add(element);
                }
            }

            //Content
            if (this.Param == null || this.Failed)
            {
                root.Add(GetErrorElement());
            }
            else
            {
                if (elements != null)
                {
                    foreach (var element in elements)
                    {
                        root.Add(element);
                    }
                }
            }

            //Close document
            document.Add(root);

            return document;
        }

        protected XDocument MergeDocuments(string rootName, List<XDocument> documents, List<XElement> rootElements = null)
        {
            var elements = new List<XElement>();

            int id = 1;
            foreach (var document in documents)
            {
                var root = document.Elements().FirstOrDefault();
                if (root == null)
                    continue;

                root.Add(new XAttribute("id", id.ToString()));
                id++;

                elements.Add(root);
            }

            return CreateDocument(rootName, elements, rootElements);
        }

        protected XElement GetErrorElement()
        {
            return new XElement("ErrorMessage", ErrorMessage);
        }

        protected object GetTextOrCDATA(string value)
        {
            if ( /*param.MobileDeviceType == MobileDeviceType.IOS && */ !string.IsNullOrEmpty(value))
            {
                return new XCData(value.Replace("\x02", "").Replace("\x03", "").ToString().Trim());
            }
            {
                return value;
            }
        }
        #endregion

        #region Virtual methods

        public virtual XDocument ToXDocument()
        {
            return XmlUtil.CreateDocument();
        }

        public virtual XDocument ToXDocument(MobileTask task)
        {
            return XmlUtil.CreateDocument();
        }

        #endregion
    }

    #endregion

    #region Accumulator

    internal class MobileAccumulators : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "Accumulators";

        #endregion

        #region Variables

        #region Collections

        private List<MobileAccumulator> mobileAccumulators;

        #endregion

        #endregion

        #region Ctor

        /// <summary>Used for get Customers</summary>
        public MobileAccumulators(MobileParam param)
            : base(param)
        {
            Init();
        }

        /// <summary> Used for errors</summary>
        public MobileAccumulators(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            this.mobileAccumulators = new List<MobileAccumulator>();
        }

        #endregion

        #region Public methods
        public void AddAccumulators(List<TimeAccumulatorItem> items)
        {
            if (items == null)
                return;

            foreach (var item in items)
            {
                AddAccumulator(new MobileAccumulator(this.Param, item));
            }
        }

        public void AddAccumulator(string name, string value)
        {
            AddAccumulator(new MobileAccumulator(this.Param, name, value));
        }

        public void AddAccumulator(MobileAccumulator item)
        {
            if (item == null)
                return;

            mobileAccumulators.Add(item);
        }

        #endregion

        #region Private methods

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();

            return MergeDocuments(ROOTNAME, mobileAccumulators.Select(i => i.ToXDocument()).ToList(), elements);
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            return XmlUtil.CreateXml(ROOTNAME, MobileCustomer.GetDefaultXml());
        }

        #endregion
    }

    internal class MobileAccumulator : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "Accumulator";

        #endregion

        #region Variables

        private readonly string name;
        private readonly string value;

        #endregion

        #region Ctor

        public MobileAccumulator(MobileParam param)
            : base(param)
        {
        }

        public MobileAccumulator(MobileParam param, TimeAccumulatorItem item)
            : base(param)
        {
            this.name = item.Name;
            this.value = CalendarUtility.FormatTimeSpan(CalendarUtility.MinutesToTimeSpan((int)item.SumAccToday), false, false);
        }

        public MobileAccumulator(MobileParam param, string name, string value)
           : base(param)
        {
            this.name = name;
            this.value = value;
        }

        /// <summary>Used for errors</summary>
        public MobileAccumulator(MobileParam param, string errorMessage)
            : base(param, errorMessage)
        {
        }

        #endregion

        #region Public methods

        #endregion
        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();

            elements.Add(new XElement("Name", this.name));
            elements.Add(new XElement("Value", this.value));

            return CreateDocument(ROOTNAME, elements);
        }
    }

    #endregion

    #region DimAccounts

    internal class MobileDimAccounts : MobileBase
    {
        #region Variables

        public const string ROOTNAME = "Accs";

        private readonly List<MobileDimAccount> accounts = new List<MobileDimAccount>();


        /// <summary>Used for get Customers</summary>
        public MobileDimAccounts(MobileParam param, List<AccountDimSmallDTO> dims) : base(param)
        {
            AddAccounts(dims);
        }

        public MobileDimAccounts(MobileParam param, string errorMessage) : base(param, errorMessage)
        {

        }

        private void AddAccounts(List<AccountDimSmallDTO> dims)
        {
            foreach (var dim in dims)
            {
                foreach (var account in dim.Accounts)
                {
                    accounts.Add(new MobileDimAccount(this.Param, dim.AccountDimNr, account.AccountId, account.AccountNr, account.Name));
                }
            }
        }

        public override XDocument ToXDocument()
        {
            return MergeDocuments(ROOTNAME, accounts.Select(i => i.ToXDocument()).ToList());
        }

        #endregion
    }

    internal class MobileDimAccount : MobileBase
    {
        #region Variables

        public const string ROOTNAME = "Acc";

        private readonly int DimNr;
        private readonly int AccountId;
        private readonly string AccountNr;
        private readonly string AccountName;

        #endregion

        #region Public Methods

        public MobileDimAccount(MobileParam param, int dimNr, int accountId, string accountNr, string accountName) : base(param)
        {
            DimNr = dimNr;
            AccountId = accountId;
            AccountNr = accountNr;
            AccountName = accountName;

        }

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();

            elements.Add(new XElement("DNr", this.DimNr));
            elements.Add(new XElement("AccId", this.AccountId));
            elements.Add(new XElement("AccN", GetTextOrCDATA(this.AccountName)));
            elements.Add(new XElement("AccNr", GetTextOrCDATA(this.AccountNr)));

            return CreateDocument(ROOTNAME, elements);
        }

        #endregion
    }

    #endregion

    #region AccountDim

    internal class MobileAccountDims : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "Dims";

        #endregion

        #region Variables

        private List<MobileAccountDim> mobileAccountDims;

        #endregion

        #region Ctor

        /// <summary>Used for get Customers</summary>
        public MobileAccountDims(MobileParam param, List<AccountDimSmallDTO> dims) : base(param)
        {
            Init();
            AddDims(dims);
        }

        /// <summary> Used for errors</summary>
        public MobileAccountDims(MobileParam param, string errorMessage) : base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            this.mobileAccountDims = new List<MobileAccountDim>();
        }

        #endregion

        #region Public methods
        public void AddDims(List<AccountDimSmallDTO> dims)
        {
            if (dims == null)
                return;

            foreach (var dim in dims)
            {
                AddDims(new MobileAccountDim(this.Param, dim));
            }
        }

        public void AddDims(MobileAccountDim accountDim)
        {
            if (accountDim == null)
                return;

            mobileAccountDims.Add(accountDim);
        }

        public void SetAccountDimAsLocked(List<Account> selectedAccounts)
        {
            if (this.mobileAccountDims == null)
                return;

            foreach (var account in selectedAccounts)
                this.SetAccountDimAsLocked(account);

        }

        public void SetAccountDimAsLocked(Account selectedAccount)
        {
            if (this.mobileAccountDims == null)
                return;

            var accountDim = this.mobileAccountDims.FirstOrDefault(x => x.id == selectedAccount.AccountDimId);
            if (accountDim != null)
                accountDim.SetAsLocked(selectedAccount);
        }

        public void SetSelectedAccounts(List<Account> selectAccounts)
        {
            if (this.mobileAccountDims == null)
                return;

            foreach (var group in selectAccounts.GroupBy(x => x.AccountDimId))
            {
                var accountDim = this.mobileAccountDims.FirstOrDefault(x => x.id == group.Key);
                if (accountDim != null)
                    accountDim.SetSelectedAccounts(group.ToList());
            }
        }

        public List<int> GetValidAccountIds()
        {
            if (this.mobileAccountDims == null)
                return new List<int>();

            List<int> ids = new List<int>();
            foreach (var mobileAccountDim in this.mobileAccountDims)
            {
                if (mobileAccountDim.accounts.Any(x => x.selected))
                    ids.AddRange(mobileAccountDim.accounts.Where(x => x.selected).Select(x => x.id));
                else
                    ids.AddRange(mobileAccountDim.accounts.Select(x => x.id));
            }

            return ids;
        }

        #endregion

        #region Private methods

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();

            return MergeDocuments(ROOTNAME, mobileAccountDims.Where(x => !x.linkedToShiftType).Select(i => i.ToXDocument()).ToList(), elements);
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            return XmlUtil.CreateXml(ROOTNAME, MobileCustomer.GetDefaultXml());
        }

        #endregion
    }

    internal class MobileAccountDim : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "Dim";

        #endregion

        #region Variables

        public int id;
        private readonly int nr;
        private bool locked;
        private string name;
        public bool linkedToShiftType;
        public List<MobileAccount> accounts;

        #endregion

        #region Ctor

        public MobileAccountDim(MobileParam param) : base(param)
        {
            Init();
        }

        public MobileAccountDim(MobileParam param, AccountDimSmallDTO accountDimDTO)
            : base(param)
        {
            Init();

            this.id = accountDimDTO.AccountDimId;
            this.nr = accountDimDTO.AccountDimNr;
            this.linkedToShiftType = accountDimDTO.LinkedToShiftType;
            this.name = accountDimDTO.Name;

            foreach (var account in accountDimDTO.CurrentSelectableAccounts)
            {
                this.accounts.Add(new MobileAccount(param, account.AccountId, account.Name, false));
            }
        }

        /// <summary>Used for errors</summary>
        public MobileAccountDim(MobileParam param, string errorMessage) : base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            this.accounts = new List<MobileAccount>();
        }

        #endregion

        #region Public methods

        public void SetAsLocked(Account selectAccount)
        {
            if (this.accounts == null)
                return; //Init has not been done

            this.locked = true;
            this.name = selectAccount.Name;
            this.accounts.Clear();
            this.accounts.Add(new MobileAccount(this.Param, selectAccount.AccountId, selectAccount.Name, true));

        }

        public void SetSelectedAccounts(List<Account> selectedAccounts)
        {
            foreach (var account in selectedAccounts)
            {
                var mobileAccount = this.accounts.FirstOrDefault(x => x.id == account.AccountId);
                if (mobileAccount != null)
                    mobileAccount.selected = true;
            }

        }


        #endregion
        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();

            elements.Add(new XElement("DId", this.id));
            elements.Add(new XElement("DNr", this.nr));
            elements.Add(new XElement("DL", StringUtility.GetString(this.locked)));
            elements.Add(new XElement("DN", this.name));
            foreach (var account in accounts)
            {
                elements.Add(account.ToXElement());
            }

            return CreateDocument(ROOTNAME, elements);
        }
    }

    #endregion

    #region Customer

    internal class MobileCustomers : MobileBase
    {
        #region Constants

        public const int MAXFETCH = 4000;
        public const string ROOTNAME = "Customers";

        #endregion

        #region Variables

        #region Collections

        private List<MobileCustomer> mobileCustomers;

        #endregion

        #endregion

        #region Ctor

        /// <summary>Used for get Customers</summary>
        public MobileCustomers(MobileParam param, List<CustomerSearchView> customers)
            : base(param)
        {
            Init();
            AddCustomers(customers);
        }

        /// <summary> Used for errors</summary>
        public MobileCustomers(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            this.mobileCustomers = new List<MobileCustomer>();
        }

        #endregion

        #region Public methods

        public void AddCustomers(List<CustomerSearchView> customers)
        {
            foreach (var customerResult in customers.GroupBy(c => c.ActorCustomerId))
            {
                var first = customerResult.First();

                if (first != null)
                {
                    var deliveryAddress = String.Empty;
                    var billingAddress = String.Empty;
                    Helpers.GetCustomerAddresses(customerResult, out deliveryAddress, out billingAddress);

                    AddCustomer(new MobileCustomer(this.Param, first.ActorCustomerId, first.CustomerNr, first.Name, deliveryAddress, billingAddress));
                }
            }
        }

        public void AddCustomer(MobileCustomer mobileCustomer)
        {
            if (mobileCustomers == null || mobileCustomer == null || mobileCustomer.Failed || ContainsCustomer(mobileCustomer.CustomerId))
                return;

            mobileCustomers.Add(mobileCustomer);
        }

        #endregion

        #region Private methods

        private bool ContainsCustomer(int customerId)
        {
            return mobileCustomers.Any(i => i.CustomerId == customerId);
        }

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            return MergeDocuments(ROOTNAME, mobileCustomers.Select(i => i.ToXDocument()).ToList());
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            return XmlUtil.CreateXml(ROOTNAME, MobileCustomer.GetDefaultXml());
        }

        #endregion
    }

    internal class MobileCustomer : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "Customer";

        #endregion

        #region Variables

        #region Field values

        private readonly int customerId;
        public int CustomerId
        {
            get { return customerId; }
        }

        private readonly string customerNr;
        public string CustomerNr
        {
            get { return customerNr; }
        }

        private readonly string name;
        public string Name
        {
            get { return name; }
        }

        private readonly string deliveryAddress;
        private readonly string billingAddress;

        #endregion

        #endregion

        #region Ctor

        /// <summary>Used for get Customer</summary>
        public MobileCustomer(MobileParam param, int customerId, string customerNr, string name, string deliveryAddress, string billingAddress)
            : base(param)
        {
            this.customerId = customerId;
            this.customerNr = customerNr;
            this.name = name;
            this.deliveryAddress = deliveryAddress;
            this.billingAddress = billingAddress;
        }

        /// <summary>Used for errors</summary>
        public MobileCustomer(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
        }

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();

            elements.Add(new XElement("CustomerId", CustomerId.ToString()));
            elements.Add(new XElement("CustomerNr", GetTextOrCDATA(CustomerNr)));
            elements.Add(new XElement("Name", GetTextOrCDATA(Name + " (" + CustomerNr + ")")) );
            elements.Add(new XElement("DeliveryAddress", GetTextOrCDATA(deliveryAddress)));
            elements.Add(new XElement("BillingAddress", GetTextOrCDATA(billingAddress)));

            return CreateDocument(ROOTNAME, elements);
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            string xml =
                "<CustomerId>1</CustomerId>" +
                "<CustomerNr>100</CustomerNr>" +
                "<Name>Demokund</Name>";

            return XmlUtil.CreateXml(ROOTNAME, xml);
        }

        #endregion
    }

    #endregion

    #region UserCompantRoleDelegate

    internal class MobileUserCompanyRoleDelegate : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "Delegate";

        #endregion

        #region Variables

        readonly private int userId;
        readonly private string loginName;
        readonly private string name;
        private List<MobileUserAttestRole> attestRoles;

        #endregion

        #region Ctor

        public MobileUserCompanyRoleDelegate(MobileParam param)
            : base(param)
        {
            Init();
        }

        public MobileUserCompanyRoleDelegate(MobileParam param, UserCompanyRoleDelegateHistoryUserDTO delegateUserDTO)
            : base(param)
        {
            Init();

            this.userId = delegateUserDTO.UserId;
            this.loginName = delegateUserDTO.LoginName;
            this.name = delegateUserDTO.Name;

            foreach (var attestRole in delegateUserDTO.PossibleTargetAttestRoles)
            {
                this.attestRoles.Add(new MobileUserAttestRole(param, attestRole));
            }
        }

        /// <summary>Used for errors</summary>
        public MobileUserCompanyRoleDelegate(MobileParam param, string errorMessage)
            : base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            this.attestRoles = new List<MobileUserAttestRole>();
        }

        #endregion

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();

            elements.Add(new XElement("UId", this.userId));
            elements.Add(new XElement("LName", this.loginName));
            elements.Add(new XElement("Name", this.name));
            foreach (var attestRole in attestRoles)
            {
                elements.Add(attestRole.ToXElement());
            }

            return CreateDocument(ROOTNAME, elements);
        }
    }

    #endregion

    #region UserDelegateHistory

    internal class MobileUserDelegateHistory : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "DelegateHistory";

        #endregion

        #region Variables

        #region Collections

        private List<MobileUserDelegateHistoryRow> mobileUserDelegateHistoryRows;

        #endregion

        #endregion

        #region Ctor

        public MobileUserDelegateHistory(MobileParam param)
            : base(param)
        {
            Init();
        }

        public MobileUserDelegateHistory(MobileParam param, List<UserCompanyRoleDelegateHistoryGridDTO> rows)
            : base(param)
        {
            Init();
            AddMobileUserDelegateHistoryRows(rows);
        }

        /// <summary>Used for errors</summary>
        public MobileUserDelegateHistory(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            this.mobileUserDelegateHistoryRows = new List<MobileUserDelegateHistoryRow>();
        }

        #endregion

        #region Public methods       

        public void AddMobileUserDelegateHistoryRows(List<UserCompanyRoleDelegateHistoryGridDTO> rows)
        {
            if (rows == null)
                return;


            foreach (var row in rows)
            {
                AddMobileUserDelegateHistoryRow(new MobileUserDelegateHistoryRow(this.Param, row, rows.Count(r => r.UserCompanyRoleDelegateHistoryHeadId == row.UserCompanyRoleDelegateHistoryHeadId) == 1));
            }
        }

        public void AddMobileUserDelegateHistoryRow(MobileUserDelegateHistoryRow row)
        {
            if (row == null)
                return;

            mobileUserDelegateHistoryRows.Add(row);
        }

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            return MergeDocuments(ROOTNAME, mobileUserDelegateHistoryRows.Select(i => i.ToXDocument()).ToList());
        }

        #endregion

    }

    internal class MobileUserDelegateHistoryRow : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "DelegateHistoryRow";

        #endregion

        #region Variables

        #region Field values

        readonly private int userCompanyRoleDelegateHistoryHeadId;
        readonly private string fromUserName;
        readonly private string toUserName;
        readonly private string byUserName;
        readonly private DateTime dateFrom;
        readonly private DateTime dateTo;
        readonly private string attestRoleNames;
        readonly private DateTime? created;
        readonly private bool isDeleted;
        readonly private bool singleInBatch;
        readonly private bool showDelete;

        #endregion

        #endregion

        #region Ctor

        public MobileUserDelegateHistoryRow(MobileParam param)
            : base(param)
        {
            Init();

        }

        public MobileUserDelegateHistoryRow(MobileParam param, UserCompanyRoleDelegateHistoryGridDTO gridDTO, bool singleInBatch)
            : base(param)
        {
            Init();

            this.userCompanyRoleDelegateHistoryHeadId = gridDTO.UserCompanyRoleDelegateHistoryHeadId;
            this.fromUserName = gridDTO.FromUserName;
            this.toUserName = gridDTO.ToUserName;
            this.byUserName = gridDTO.ByUserName;
            this.dateFrom = gridDTO.DateFrom;
            this.dateTo = gridDTO.DateTo;
            this.attestRoleNames = gridDTO.AttestRoleNames;
            this.created = gridDTO.Created;
            this.isDeleted = gridDTO.State == SoeEntityState.Deleted;
            this.singleInBatch = singleInBatch;
            this.showDelete = gridDTO.ShowDelete;

        }

        /// <summary>
        /// Used for errors
        /// </summary>
        public MobileUserDelegateHistoryRow(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            //Set default values
        }

        #endregion

        #region Public methods

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>
            {
                new XElement("HeadId", userCompanyRoleDelegateHistoryHeadId),
                new XElement("FUN", fromUserName),
                new XElement("TUN", toUserName),
                new XElement("BUN", byUserName),
                new XElement("DFrom", StringUtility.GetSwedishFormattedDate(dateFrom)),
                new XElement("DTo", StringUtility.GetSwedishFormattedDate(dateTo)),
                new XElement("Created", StringUtility.GetSwedishFormattedDate(created.Value)),
                new XElement("ARN", attestRoleNames),
                new XElement("IsDeleted", StringUtility.GetBool(isDeleted)),
                new XElement("ISIB", StringUtility.GetBool(singleInBatch)),
                new XElement("SD", StringUtility.GetBool(showDelete))
            };


            return CreateDocument(ROOTNAME, elements);
        }

        #endregion        
    }


    #endregion

    #region UserAttestRole

    internal class MobileUserAttestRole : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "UserAttestRole";

        #endregion

        #region Variables

        readonly private int attestRoleUserId;
        readonly private string name;

        #endregion

        #region Ctor

        public MobileUserAttestRole(MobileParam param)
            : base(param)
        {
            Init();
        }

        public MobileUserAttestRole(MobileParam param, UserAttestRoleDTO userAttestRoleDTO)
            : base(param)
        {
            Init();

            this.attestRoleUserId = userAttestRoleDTO.AttestRoleUserId;
            this.name = userAttestRoleDTO.Name;
        }

        /// <summary>Used for errors</summary>
        public MobileUserAttestRole(MobileParam param, string errorMessage)
            : base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            //Do initializing 
        }

        #endregion

        public XElement ToXElement()
        {
            XElement element = new XElement(ROOTNAME);

            element.Add(new XElement("ARUserId", this.attestRoleUserId));
            element.Add(new XElement("ARName", this.name));

            return element;
        }

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();



            return CreateDocument(ROOTNAME, elements);
        }
    }

    #endregion

    #region Product

    #region Internal

    internal class MobileProducts : MobileBase
    {
        #region Constants

        public const int MAXFETCH = 50;
        public const string ROOTNAME = "Products";

        #endregion

        #region Variables

        private readonly bool useExtendSearchInfo;

        #region Collections

        private List<MobileProduct> mobileProducts;

        #endregion

        #endregion

        #region Ctor

        /// <summary>Used for get Products</summary>
        public MobileProducts(MobileParam param, List<InvoiceProductPriceSearchDTO> products, bool useExtendSearchInfo = false) : base(param)
        {
            Init();

            this.useExtendSearchInfo = useExtendSearchInfo;

            AddProducts(products);
        }

        /// <summary>Used for errors</summary>
        public MobileProducts(MobileParam param, string errorMessage) : base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            this.mobileProducts = new List<MobileProduct>();
        }

        #endregion

        #region Public methods

        public void AddProducts(List<InvoiceProductPriceSearchDTO> products)
        {
            if (products == null)
                return;

            foreach (InvoiceProductPriceSearchDTO product in products)
            {
                AddProduct(new MobileProduct(this.Param, product));
            }
        }

        public void AddProducts(List<MobileProduct> mobileProducts)
        {
            if (mobileProducts == null)
                return;

            foreach (MobileProduct mobileProduct in mobileProducts)
            {
                AddProduct(mobileProduct);
            }
        }

        public void AddProduct(MobileProduct mobileProduct)
        {
            if (mobileProduct == null || mobileProduct.Failed || ContainsProduct(mobileProduct.ProductId))
                return;

            mobileProducts.Add(mobileProduct);
        }

        public void SetPriceDisabled(bool disabled = true)
        {
            foreach (var mobileProduct in mobileProducts)
            {
                mobileProduct.SetPriceDisabled(disabled);
            }
        }

        #endregion

        #region Private methods

        private bool ContainsProduct(int productId)
        {
            return mobileProducts.Any(i => i.ProductId == productId);
        }

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();

            elements.Add(new XElement("UseExtendSearchInfo", StringUtility.GetString(this.useExtendSearchInfo)));

            return MergeDocuments(ROOTNAME, mobileProducts.Select(i => i.ToXDocument()).ToList(), elements);
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            return XmlUtil.CreateXml(ROOTNAME, MobileProduct.GetDefaultXml());
        }

        #endregion
    }

    internal class MobileProduct : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "Product";

        #endregion

        #region Variables

        public readonly int ProductId;
        public readonly string ProductNr;
        public readonly string Name;
        public readonly string sysWholesellerName;

        public readonly decimal Price;
        public readonly string Unit;
        public readonly bool Imported;

        private bool priceDisabled = false;
        private readonly string externalUrl;
        private readonly string extendedInfo;
        private readonly string imageFileName;
        private readonly int externalId;

        #endregion

        #region Ctor

        /// <summary>Used for get Product</summary>
        public MobileProduct(MobileParam param, InvoiceProductPriceSearchDTO product) : base(param)
        {
            if (product != null)
            {
                this.ProductId = product.ProductId;
                this.ProductNr = product.ProductNr;
                this.Name = product.ProductName;
                this.Price = product.SalesPrice;
                this.Unit = product.ProductUnitCode;
                this.Imported = product.External;
                this.externalUrl = product.ExternalUrl;
                this.imageFileName = product.ImageFileName;
                this.extendedInfo = product.ExtendedInfo;
                this.externalId = product.ExternalId;
                this.sysWholesellerName = product.SysWholesellerName;
            }
        }

        /// <summary>Used for get Product</summary>
        public MobileProduct(MobileParam param, InvoiceProduct product) : base(param)
        {
            if (product != null)
            {
                this.ProductId = product.ProductId;
                this.ProductNr = product.Number;
                this.Name = product.Name;
                this.Price = product.SalesPrice;
                this.Unit = product.ProductUnit != null ? product.ProductUnit.Code : string.Empty;
                this.Imported = product.ExternalProductId.HasValue;
            }
        }

        public MobileProduct(MobileParam param) : base(param)
        {
            this.ProductId = 0;
            this.ProductNr = string.Empty;
            this.Name = string.Empty;
            this.Price = 0;
            this.Unit = string.Empty;
            this.Imported = false;
        }

        /// <summary>
        /// Used for errors
        /// </summary>
        public MobileProduct(MobileParam param, string errorMessage) : base(param, errorMessage)
        {
        }

        #endregion

        #region Public methods

        public void SetPriceDisabled(bool disabled = true)
        {
            this.priceDisabled = disabled;
        }

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();

            elements.Add(new XElement("ProductId", ProductId.ToString()));
            elements.Add(new XElement("ProductNr", ProductNr));
            elements.Add(new XElement("Name", Name));

            if (!string.IsNullOrEmpty(this.sysWholesellerName))
            {
                elements.Add(new XElement("SysWholesellerName", this.sysWholesellerName));
            }

            if (!this.priceDisabled)
                elements.Add(new XElement("Price", Price.ToString()));

            elements.Add(new XElement("Unit", this.Unit));
            elements.Add(new XElement("Imported", this.Imported));
            elements.Add(new XElement("ExternalId", this.externalId));

            if (!string.IsNullOrEmpty(this.externalUrl))
                elements.Add(new XElement("ExternalUrl", this.externalUrl));

            if (!string.IsNullOrEmpty(this.imageFileName))
                elements.Add(new XElement("ImageFileName", this.imageFileName));

            if (!string.IsNullOrEmpty(this.extendedInfo))
                elements.Add(new XElement("ExtendedInfo", this.extendedInfo));

            return CreateDocument(ROOTNAME, elements);
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            string xml =
                "<ProductId>1</ProductId>" +
                "<ProductNr>100</ProductNr>" +
                "<Name>Demoartikel</Name>" +
                "<Price>100</Price>" +
                "<Unit>St</Unit>" +
                "<Imported>false</Imported>";

            return XmlUtil.CreateXml(ROOTNAME, xml);
        }

        #endregion
    }

    #endregion

    #region External

    internal class MobileExternalProducts : MobileBase
    {
        #region Constants

        public const int MAXFETCH = 50;
        public const string ROOTNAME = "SysProducts";

        #endregion

        #region Variables

        private readonly bool useExtendSearchInfo;
        private readonly List<MobileExternalProduct> mobileExternalProducts = new List<MobileExternalProduct>();

        #endregion

        #region Ctor

        /// <summary>Used for get Products</summary>
        public MobileExternalProducts(MobileParam param, List<InvoiceProductSearchViewDTO> externalProducts, bool useExtendSearchInfo = false) : base(param)
        {
            this.useExtendSearchInfo = useExtendSearchInfo;

            AddExternalProducts(externalProducts);
        }

        /// <summary>Used for errors</summary>
        public MobileExternalProducts(MobileParam param, string errorMessage) : base(param, errorMessage)
        {

        }

        #endregion

        #region Public methods

        public void AddExternalProducts(List<InvoiceProductSearchViewDTO> externalProducts)
        {
            if (externalProducts == null)
                return;

            foreach (var externalProduct in externalProducts)
            {
                AddExternalProduct(new MobileExternalProduct(this.Param, externalProduct));
            }
        }

        public void AddExternalProducts(List<MobileExternalProduct> mobileExternalProducts)
        {
            if (mobileExternalProducts == null)
                return;

            foreach (MobileExternalProduct mobileExternalProduct in mobileExternalProducts)
            {
                AddExternalProduct(mobileExternalProduct);
            }
        }

        public void AddExternalProduct(MobileExternalProduct mobileExternalProduct)
        {
            if (mobileExternalProduct == null || mobileExternalProduct.Failed || ContainsProduct(mobileExternalProduct.SysProductId))
                return;

            mobileExternalProducts.Add(mobileExternalProduct);
        }

        #endregion

        #region Private methods

        private bool ContainsProduct(int sysProductId)
        {
            return mobileExternalProducts.Any(i => i.SysProductId == sysProductId);
        }

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();

            elements.Add(new XElement("UseExtendSearchInfo", StringUtility.GetString(this.useExtendSearchInfo)));

            return MergeDocuments(ROOTNAME, mobileExternalProducts.Select(i => i.ToXDocument()).ToList(), elements);
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            return XmlUtil.CreateXml(ROOTNAME, MobileExternalProduct.GetDefaultXml());
        }

        #endregion
    }

    internal class MobileExternalProduct : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "SysProduct";

        #endregion

        #region Variables

        public readonly int SysProductId;

        public readonly string ProductNr;

        public readonly string Name;

        private readonly string externalUrl;

        private readonly string extendedInfo;

        public readonly int externalId;

        public readonly string imageFileName;


        #endregion

        #region Ctor

        /// <summary>Used for get Product</summary>
        public MobileExternalProduct(MobileParam param, InvoiceProductSearchViewDTO product) : base(param)
        {
            if (product != null)
            {
                SysProductId = product.ProductIds.Any() ? product.ProductIds.First() : 0;
                ProductNr = product.Number;
                Name = product.Name;
                externalUrl = product.ExternalUrl;
                extendedInfo = product.ExtendedInfo;
                externalId = product.ExternalId ?? 0;
                imageFileName = product.ImageUrl;
            }
        }

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();

            elements.Add(new XElement("SysProductId", SysProductId.ToString()));
            elements.Add(new XElement("ProductNr", ProductNr));
            elements.Add(new XElement("Name", Name));
            elements.Add(new XElement("ExternalId", externalId));


            if (!string.IsNullOrEmpty(externalUrl))
                elements.Add(new XElement("ExternalUrl", externalUrl));

            if(!string.IsNullOrEmpty(extendedInfo))
                elements.Add(new XElement("ExtendedInfo", extendedInfo));

            if (!string.IsNullOrEmpty(imageFileName))
                elements.Add(new XElement("ImageFileName", imageFileName));

            return CreateDocument(ROOTNAME, elements);
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            string xml =
                "<SysProductId>1</SysProductId>" +
                "<ProductNr>100</ProductNr>" +
                "<Name>Demoartikel</Name>" +
                "<Unit>St</Unit>";

            return XmlUtil.CreateXml(ROOTNAME, xml);
        }

        #endregion
    }

    #endregion

    #endregion

    #region Prices

    internal class MobileExternalProductPrices : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "SysProductPrices";

        #endregion

        #region Variables

        #region Collections

        private readonly List<MobileExternalProductPrice> mobileExternalProductPrices;

        #endregion

        #endregion

        #region Ctor

        /// <summary>Used for get Products</summary>
        public MobileExternalProductPrices(MobileParam param, List<InvoiceProductPriceSearchViewDTO> externalProductPrices, int? orderSysWholeSellerId) : base(param)
        {
            this.mobileExternalProductPrices = new List<MobileExternalProductPrice>();
            AddExternalProductPrices(externalProductPrices, orderSysWholeSellerId);
        }

        /// <summary>Used for errors</summary>
        public MobileExternalProductPrices(MobileParam param, string errorMessage) : base(param, errorMessage)
        {
            this.mobileExternalProductPrices = new List<MobileExternalProductPrice>();
        }



        #endregion

        #region Public methods

        public void AddExternalProductPrices(List<InvoiceProductPriceSearchViewDTO> externalProductPrices, int? orderSysWholeSellerId)
        {
            if (externalProductPrices == null)
                return;

            foreach (var externalProductPrice in externalProductPrices)
            {
                AddExternalProduct(new MobileExternalProductPrice(this.Param, externalProductPrice, orderSysWholeSellerId));
            }
        }

        public void AddExternalProducts(List<MobileExternalProductPrice> mobileExternalProductPrices)
        {
            if (mobileExternalProductPrices == null)
                return;

            foreach (MobileExternalProductPrice mobileExternalProductPrice in mobileExternalProductPrices)
            {
                AddExternalProduct(mobileExternalProductPrice);
            }
        }

        public void AddExternalProduct(MobileExternalProductPrice mobileExternalProductPrice)
        {
            if (mobileExternalProductPrice == null || mobileExternalProductPrice.Failed)
                return;

            mobileExternalProductPrices.Add(mobileExternalProductPrice);
        }

        public void SetPriceDisabled(bool disabled = true)
        {
            foreach (var mobileExternalProductPrice in mobileExternalProductPrices)
            {
                mobileExternalProductPrice.SetPriceDisabled(disabled);
            }
        }

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            return MergeDocuments(ROOTNAME, mobileExternalProductPrices.OrderBy(x => x.CustomerPrice).Select(i => i.ToXDocument()).ToList());
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            return XmlUtil.CreateXml(ROOTNAME, MobileProduct.GetDefaultXml());
        }

        #endregion
    }

    internal class MobileExternalProductPrice : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "SysProductPrice";

        #endregion

        #region Variables

        #region Field values

        private readonly int sysProductId;
        public int SysProductId
        {
            get { return sysProductId; }
        }

        private readonly String wholeseller;
        public String Wholeseller
        {
            get { return wholeseller; }
        }

        private readonly int wholesellerId;
        public int WholesellerId
        {
            get { return wholesellerId; }
        }

        private readonly decimal gnp;
        public decimal GNP
        {
            get { return gnp; }
        }

        private readonly decimal nettoNettoPrice;
        public decimal NettoNettoPrice
        {
            get { return nettoNettoPrice; }
        }

        private readonly decimal customerPrice;
        public decimal CustomerPrice
        {
            get { return customerPrice; }
        }

        private readonly decimal marginalIncome;
        public decimal MarginalIncome
        {
            get { return marginalIncome; }
        }

        private readonly decimal marginalIncomeRatio;
        public decimal MarginalIncomeRatio
        {
            get { return marginalIncomeRatio; }
        }

        private readonly String purchaseUnit;
        public String PurchaseUnit
        {
            get { return purchaseUnit; }
        }

        private readonly bool defaultWholeSeller;

        #endregion

        #endregion

        #region Permissions

        private bool priceDisabled = false;

        #endregion

        #region Ctor

        /// <summary>Used for get Product</summary>
        public MobileExternalProductPrice(MobileParam param, InvoiceProductPriceSearchViewDTO product, int? orderSysWholeSellerId) : base(param)
        {
            if (product != null)
            {
                this.sysProductId = product.ProductId;
                this.wholeseller = product.Wholeseller;
                this.wholesellerId = product.SysWholesellerId;
                this.gnp = product.GNP;
                this.nettoNettoPrice = product.NettoNettoPrice ?? 0;
                this.customerPrice = product.CustomerPrice ?? 0;
                this.marginalIncome = product.MarginalIncome;
                this.marginalIncomeRatio = product.MarginalIncomeRatio;
                this.purchaseUnit = product.PurchaseUnit;
                this.defaultWholeSeller = orderSysWholeSellerId != null && product.SysWholesellerId == orderSysWholeSellerId;

            }
        }

        /// <summary>
        /// Used for errors
        /// </summary>
        public MobileExternalProductPrice(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
        }

        #endregion

        #region Public methods

        public void SetPriceDisabled(bool disabled = true)
        {
            this.priceDisabled = disabled;
        }
        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();

            elements.Add(new XElement("SysProductId", SysProductId.ToString()));
            elements.Add(new XElement("Wholeseller", Wholeseller));
            elements.Add(new XElement("WholesellerId", WholesellerId));
            elements.Add(new XElement("PurchaseUnit", purchaseUnit));
            elements.Add(new XElement("DefaultWholeSeller", StringUtility.GetString(defaultWholeSeller)));
            
            if (!this.priceDisabled)
            {
                elements.Add(new XElement("GNP", GNP));
                elements.Add(new XElement("NettoNettoPrice", NettoNettoPrice));
                elements.Add(new XElement("CustomerPrice", CustomerPrice));
                elements.Add(new XElement("MarginalIncome", MarginalIncome));
                elements.Add(new XElement("MarginalIncomeRatio", MarginalIncomeRatio));
            }
            return CreateDocument(ROOTNAME, elements);
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            string xml =
                "<SysProductId>1</SysProductId>" +
                "<Wholeseller>Ahlsell</Wholeseller>" +
                "<WholesellerId>1</ProductId>" +
                "<GNP>50</GNP>" +
                "<NettoNettoPrice>40</NettoNettoPrice>" +
                "<CustomerPrice>100</CustomerPrice>" +
                "<MarginalIncome>0</MarginalIncome>" +
                "<MarginalIncomeRatio>0</MarginalIncomeRatio>";

            return XmlUtil.CreateXml(ROOTNAME, xml);
        }

        #endregion
    }

    #endregion

    #region OrderRows/TimeRows

    internal abstract class MobileOrderRowBase : MobileBase
    {
        #region Variables

        #region Field values

        private readonly int orderId;
        public int OrderId
        {
            get { return orderId; }
        }

        #endregion

        #endregion

        #region Ctor

        /// <summary>Used for get rows</summary>
        protected MobileOrderRowBase(MobileParam param, int orderId)
            : base(param)
        {
            this.orderId = orderId;
        }

        /// <summary>Used for errors</summary>
        protected MobileOrderRowBase(MobileParam param, int orderId, string errorMessage) :
            base(param, errorMessage)
        {
            this.orderId = orderId;
        }

        #endregion
    }
    internal class MobileOrderRowSearch : MobileOrderRowBase
    {
        #region Constants

        public const string ROOTNAME = "OrderRow";

        #endregion

        #region Variables

        #region Field values


        public readonly int orderId;
        public readonly string orderNumber;
        public readonly string customerName;
        public readonly string customerNumber;
        public readonly int projectId;
        public readonly string projectName;
        public readonly string projectNumber;

        #endregion

        #endregion

        #region Ctor

        public MobileOrderRowSearch(MobileParam param, int orderId) : base(param, orderId)
        {
        }

        /// <summary>Used for get OrderRows</summary>
        public MobileOrderRowSearch(MobileParam param, int orderId, CustomerInvoiceSearchDTO customerInvoiceRow) : base(param, orderId)
        {
            if (customerInvoiceRow != null)
            {
                this.orderId = customerInvoiceRow.CustomerInvoiceId;
                this.orderNumber = customerInvoiceRow.Number;
                this.customerName = customerInvoiceRow.CustomerName;
                this.customerNumber = customerInvoiceRow.CustomerNr;
                this.projectId = customerInvoiceRow.ProjectId != null ? customerInvoiceRow.ProjectId.Value : 0;
                this.projectName = customerInvoiceRow.ProjectName;
                this.projectNumber = customerInvoiceRow.ProjectNr;
            }
        }

        /// <summary>Used for errors</summary>
        public MobileOrderRowSearch(MobileParam param, int orderId, string errorMessage) : base(param, orderId, errorMessage)
        {
        }

        #endregion

        #region Public methods

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();

            elements.Add(new XElement("OrderId", orderId));
            elements.Add(new XElement("OrderNumber", orderNumber));
            elements.Add(new XElement("CustomerName", customerName));
            elements.Add(new XElement("CustomerNumber", customerNumber));
            elements.Add(new XElement("ProjectId", projectId));
            elements.Add(new XElement("ProjectNumber", projectNumber));
            elements.Add(new XElement("ProjectName", projectName));

            return CreateDocument(ROOTNAME, elements);
        }

        public override XDocument ToXDocument(MobileTask task)
        {
            return base.ToXDocument(task);
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            string xml =
                "<OrderRowId>1</OrderRowId>" +
                "<ProductId>1</ProductId>" +
                "<ProductNr>100</ProductNr>" +
                "<ProductName>Demoartikel</ProductName>" +
                "<Quantity></Quantity>" +
                "<Unit></Unit>" +
                "<Amount></Amount>" +
                "<SumAmount></SumAmount>" +
                "<Text></Text>" +
                "<IsHD>0</IsHD>";

            return XmlUtil.CreateXml(ROOTNAME, xml);
        }

        public static string GetDefaultSaveXml()
        {
            string xml =
                "<OrderRowId>1</OrderRowId>";

            return xml;
        }

        public static string GetDefaultDeleteXml()
        {
            string xml =
                "<Success>1</Success>";

            return xml;
        }

        #endregion
    }
    internal class MobileOrderRow : MobileOrderRowBase
    {
        #region Constants

        public const string ROOTNAME = "OrderRow";

        #endregion

        #region Variables

        #region Field values


        public readonly int OrderRowId;
        public readonly int ProductId;
        public readonly string ProductNr;
        public readonly string ProductName;
        public readonly decimal Quantity;
        private readonly int? parentRowId;
        private readonly string sysWholesellerName;

        private readonly decimal amountCurrency;
        public decimal Amount
        {
            get { return amountCurrency; }
        }

        private readonly decimal sumAmountCurrency;
        public decimal SumAmount
        {
            get { return sumAmountCurrency; }
        }

        private readonly string unit;
        public readonly string Text;

        private readonly int? HouseHoldTaxDeductionType;
        private readonly bool IsHD;
        private readonly bool IsHDRut;
        private readonly bool isTimeRow;
        private readonly bool isExpense;
        private readonly bool isReadOnly;
        private readonly bool isInitial;

        private readonly string attestStateName;
        private readonly string attestStateColor;

        private readonly int stockId;
        private readonly string stockCode;

        //Only professional for now
        private readonly bool showPartDelivery = false;
        private readonly decimal QuantityToInvoice; // Att fakurera
        private readonly bool finalDelivery; //Slutleverans
        private readonly string deliveryNote; //LeveransDatum

        private readonly bool canCopy = false;
        private readonly bool canMove = false;
        private readonly bool showWarnOnReducedQuantity = false;


        #endregion

        #region Permissions

        private bool amountDisabled = false;

        #endregion

        #endregion

        #region Ctor

        public MobileOrderRow(MobileParam param, int orderId) : base(param, orderId)
        {
        }

        /// <summary>Used for get OrderRows</summary>
        public MobileOrderRow(MobileParam param, int orderId, CustomerInvoiceRow customerInvoiceRow, List<ProductUnit> companyProductUnits, bool isReadOnly, bool isInitial, string attestStateName, string attestStateColor, bool showPartDelivery, bool canCopy, bool canMove, bool showWarnOnReducedQuantity, int productMiscId, Stock stock = null) : base(param, orderId)
        {
            if (customerInvoiceRow != null)
            {
                this.OrderRowId = customerInvoiceRow.CustomerInvoiceRowId;
                this.ProductId = customerInvoiceRow.Product?.ProductId ?? 0;
                this.ProductNr = customerInvoiceRow.Product?.Number ?? string.Empty;
                this.ProductName = customerInvoiceRow.Product?.Name ?? string.Empty;
                this.sysWholesellerName = customerInvoiceRow.SysWholesellerName;
                this.parentRowId = customerInvoiceRow.ParentRowId;
                this.Quantity = customerInvoiceRow.Quantity ?? 0;
                this.amountCurrency = customerInvoiceRow.AmountCurrency;
                this.sumAmountCurrency = customerInvoiceRow.SumAmountCurrency;

                if (customerInvoiceRow.ProductUnitId.HasValue)
                {
                    ProductUnit productUnit = companyProductUnits.FirstOrDefault(u => u.ProductUnitId == customerInvoiceRow.ProductUnitId.Value);
                    if (productUnit != null)
                        this.unit = productUnit.Code;
                }
                this.Text = customerInvoiceRow.Text;

                this.HouseHoldTaxDeductionType = customerInvoiceRow?.HouseholdTaxDeductionRow?.HouseHoldTaxDeductionType;
                this.IsHD = HouseHoldTaxDeductionType.HasValue && HouseHoldTaxDeductionType.Value == (int)TermGroup_HouseHoldTaxDeductionType.ROT;
                this.IsHDRut = HouseHoldTaxDeductionType.HasValue && HouseHoldTaxDeductionType.Value == (int)TermGroup_HouseHoldTaxDeductionType.RUT;

                this.isReadOnly = isReadOnly;
                this.isTimeRow = customerInvoiceRow.IsTimeProjectRow;
                this.isExpense = customerInvoiceRow.IsExpense();
                this.isInitial = isInitial;
                this.attestStateName = attestStateName;
                this.attestStateColor = attestStateColor;
                this.canCopy = canCopy;
                this.canMove = canMove;
                this.showWarnOnReducedQuantity = showWarnOnReducedQuantity;

                this.showPartDelivery = showPartDelivery;
                this.QuantityToInvoice = customerInvoiceRow.InvoiceQuantity ?? 0;
                //Only professional for now
                this.finalDelivery = false;
                this.deliveryNote = "";

                //Fix för Misc product
                if (this.ProductId == productMiscId && !string.IsNullOrEmpty(this.Text))
                    this.ProductName = this.Text;

                this.stockId = customerInvoiceRow.StockId.HasValue ? (int)customerInvoiceRow.StockId : 0;

                if (stockId > 0 && stock != null && stock.StockId == stockId)
                {
                    stockCode = stock.Code;
                }
                else if (stockId > 0 && stock == null)
                {
                    if (!customerInvoiceRow.StockReference.IsLoaded)
                    {
                        customerInvoiceRow.StockReference.Load();
                    }

                    stockCode = customerInvoiceRow?.Stock.Code;
                }

            }
        }

        /// <summary>Used for errors</summary>
        public MobileOrderRow(MobileParam param, int orderId, string errorMessage) : base(param, orderId, errorMessage)
        {
        }

        #endregion

        #region Public methods

        public void SetAmountDisabled(bool disabled = true)
        {
            this.amountDisabled = disabled;
        }

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();

            elements.Add(new XElement("OrderRowId", OrderRowId.ToString()));
            elements.Add(new XElement("ProductId", ProductId.ToString()));
            elements.Add(new XElement("ProductNr", ProductNr));
            elements.Add(new XElement("ProductName", ProductName));
            elements.Add(new XElement("ParentRowId", parentRowId.GetValueOrDefault().ToString()));
            elements.Add(new XElement("SysWholesellerName", sysWholesellerName));
            elements.Add(new XElement("Quantity", Quantity.ToString()));
            elements.Add(new XElement("Unit", unit));

            if (!this.amountDisabled)
            {
                elements.Add(new XElement("Amount", Amount.ToString()));
                elements.Add(new XElement("SumAmount", SumAmount.ToString()));
            }
            elements.Add(new XElement("Text", Text));
            elements.Add(new XElement("IsHD", StringUtility.GetString(IsHD)));
            elements.Add(new XElement("IsHDRut", StringUtility.GetString(IsHDRut)));
            elements.Add(new XElement("IsReadOnly", StringUtility.GetString(isReadOnly)));
            elements.Add(new XElement("IsInitial", StringUtility.GetString(isInitial)));
            elements.Add(new XElement("ASName", attestStateName));
            elements.Add(new XElement("ASColor", attestStateColor));

            if (showPartDelivery)
            {
                elements.Add(new XElement("QuantityToInvoice", QuantityToInvoice.ToString()));
                elements.Add(new XElement("FinalDelivery", StringUtility.GetString(finalDelivery)));
                elements.Add(new XElement("DeliveryNote", deliveryNote));
            }

            elements.Add(new XElement("CanCopy", StringUtility.GetString(canCopy)));
            elements.Add(new XElement("CanMove", StringUtility.GetString(canMove)));
            elements.Add(new XElement("SWORQ", StringUtility.GetString(showWarnOnReducedQuantity)));

            if (stockId > 0)
            {
                elements.Add(new XElement("StockId", stockId));
                elements.Add(new XElement("StockCode", stockCode));
            }

            elements.Add(new XElement("IsTimeRow", StringUtility.GetString(isTimeRow)));
            elements.Add(new XElement("IsExpense", StringUtility.GetString(isExpense)));
            elements.Add(new XElement("HHTDType", HouseHoldTaxDeductionType.GetValueOrDefault().ToString()));

            return CreateDocument(ROOTNAME, elements);
        }

        public override XDocument ToXDocument(MobileTask task)
        {
            if (base.Failed)
                return MobileMessages.GetErrorMessageDocument(base.ErrorMessage);

            bool result = base.GetTaskResult(task);
            switch (task)
            {
                case MobileTask.SaveOrderRow:
                    return MobileMessages.GetSuccessDocument(result);
                case MobileTask.DeleteOrderRow:
                    return MobileMessages.GetSuccessDocument(result);
                case MobileTask.CopyOrderRow:
                    return MobileMessages.GetSuccessDocument(result);
                default:
                    return base.ToXDocument(task);
            }
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            string xml =
                "<OrderRowId>1</OrderRowId>" +
                "<ProductId>1</ProductId>" +
                "<ProductNr>100</ProductNr>" +
                "<ProductName>Demoartikel</ProductName>" +
                "<Quantity></Quantity>" +
                "<Unit></Unit>" +
                "<Amount></Amount>" +
                "<SumAmount></SumAmount>" +
                "<Text></Text>" +
                "<IsHD>0</IsHD>";

            return XmlUtil.CreateXml(ROOTNAME, xml);
        }

        public static string GetDefaultSaveXml()
        {
            string xml =
                "<OrderRowId>1</OrderRowId>";

            return xml;
        }

        public static string GetDefaultDeleteXml()
        {
            string xml =
                "<Success>1</Success>";

            return xml;
        }

        #endregion
    }

    internal class MobileTimeRow : MobileOrderRowBase
    {
        #region Constants

        public const string ROOTNAME = "TimeRow";

        #endregion

        #region Variables

        #region Field values

        private readonly DateTime date;
        public DateTime Date
        {
            get { return date; }
        }

        private readonly int invoiceTimeInMinutes;
        public int InvoiceTimeInMinutes
        {
            get { return invoiceTimeInMinutes; }
        }

        private readonly int workTimeInMinutes;
        public int WorkTimeInMinutes
        {
            get { return workTimeInMinutes; }
        }

        private readonly string note;
        public string Note
        {
            get { return note; }
        }

        private readonly int timeCodeId;
        public int TimeCodeId
        {
            get { return timeCodeId; }
        }

        private readonly string timeCodeName;
        public string TimeCodeName
        {
            get { return timeCodeName; }
        }

        private readonly int id;
        public int Id
        {
            get { return id; }
        }

        #endregion

        #region Task values

        #endregion

        #endregion

        #region Ctor

        /// <summary>Used for get Time Rows</summary>
        public MobileTimeRow(MobileParam param, int orderId, ProjectInvoiceDay projectInvoiceDay, int timecodeId, string timeCodeName, bool workedTimePermission, bool invoicedTimePermission) :
            base(param, orderId)
        {
            Init();

            if (projectInvoiceDay != null)
            {
                this.id = projectInvoiceDay.ProjectInvoiceDayId;
                this.date = projectInvoiceDay.Date;
                this.invoiceTimeInMinutes = invoicedTimePermission ? projectInvoiceDay.InvoiceTimeInMinutes : 0;
                this.workTimeInMinutes = workedTimePermission ? projectInvoiceDay.WorkTimeInMinutes : 0;
                this.note = !String.IsNullOrEmpty(projectInvoiceDay.Note) ? projectInvoiceDay.Note : String.Empty;
                this.timeCodeName = timeCodeName;
                this.timeCodeId = timecodeId;
            }
        }

        /// <summary>Used for errors</summary>
        public MobileTimeRow(MobileParam param, int orderId, string errorMessage) :
            base(param, orderId, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            //Set default values
        }

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();

            elements.Add(new XElement("Id", Id));
            elements.Add(new XElement("Date", Date.ToString()));
            elements.Add(new XElement("InvoiceTimeInMinutes", InvoiceTimeInMinutes.ToString()));
            elements.Add(new XElement("WorkTimeInMinutes", WorkTimeInMinutes.ToString()));
            elements.Add(new XElement("Note", GetTextOrCDATA(Note)));
            elements.Add(new XElement("TimeCodeId", TimeCodeId));
            elements.Add(new XElement("TimeCode", GetTextOrCDATA(TimeCodeName)));

            return CreateDocument(ROOTNAME, elements);
        }

        public override XDocument ToXDocument(MobileTask task)
        {
            if (base.Failed)
                return MobileMessages.GetErrorMessageDocument(base.ErrorMessage);

            bool result = base.GetTaskResult(task);
            switch (task)
            {
                case MobileTask.SaveTimeRow:
                    return MobileMessages.GetSuccessDocument(result);
                default:
                    return base.ToXDocument(task);
            }
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            string xml =
                "<Id>1</Id>" +
                "<Date>2011-01-01</Date>" +
                "<InvoiceTimeInMinutes>120</InvoiceTimeInMinutes>" +
                "<WorkTimeInMinutes>60</WorkTimeInMinutes>" +
                "<Note>Arbete</Note>" +
                "<TimeCodeId>2</TimeCodeId>" +
                "<TimeCode>Konsulttid</TimeCode>";

            return XmlUtil.CreateXml(ROOTNAME, xml);
        }

        public static string GetDefaultSaveXml()
        {
            string xml =
                "<Success>1<Success>";

            return xml;
        }

        #endregion
    }
    internal class MobileOrderRowsSearch : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "MobileOrderRowsSearch";

        #endregion

        #region Variables

        #region Collections

        readonly private List<MobileOrderRowSearch> mobileOrderRows = new List<MobileOrderRowSearch>();

        #endregion

        #endregion

        #region Ctor

        public MobileOrderRowsSearch(MobileParam param, List<CustomerInvoiceSearchDTO> orderRows) : base(param)
        {
            Init();
            AddMobileOrderRows(orderRows);
        }

        /// <summary>
        /// Used for errors
        /// </summary>
        public MobileOrderRowsSearch(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            //Set default values
        }

        #endregion

        #region Public methods
        public void AddMobileOrderRows(List<CustomerInvoiceSearchDTO> orderRows)
        {
            if (orderRows == null)
                return;

            foreach (var item in orderRows)
            {
                mobileOrderRows.Add(new MobileOrderRowSearch(this.Param, item.CustomerInvoiceId, item));
            }
        }
        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            return MergeDocuments(ROOTNAME, mobileOrderRows.Select(i => i.ToXDocument()).ToList());
        }
        #endregion
    }
    internal class MobileOrderRows : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "OrderRows";

        #endregion

        #region Variables

        #region Collections

        private List<MobileOrderRow> mobileOrderRows;

        #endregion

        #region Field values

        private readonly int orderId;
        public int OrderId
        {
            get { return orderId; }
        }

        #endregion

        #endregion

        #region Ctor

        /// <summary>Used for get OrderRows</summary>
        public MobileOrderRows(MobileParam param, int orderId, List<CustomerInvoiceRow> customerInvoiceRows, List<ProductUnit> companyProductUnits, List<AttestState> attestStates, AttestState attestStateInitial, int attestStateTransferredOrderToInvoiceId, bool showPartDelivery, bool warnOnReducedQuantity, int productMiscId) : base(param)
        {
            Init();
            this.orderId = orderId;
            AddOrderRows(customerInvoiceRows, companyProductUnits, attestStates, attestStateInitial, attestStateTransferredOrderToInvoiceId, showPartDelivery, warnOnReducedQuantity, productMiscId);
        }

        /// <summary>Used for errors</summary>
        public MobileOrderRows(MobileParam param, int orderId, string errorMessage) : base(param, errorMessage)
        {
            Init();
            this.orderId = orderId;
        }

        private void Init()
        {
            this.mobileOrderRows = new List<MobileOrderRow>();
        }

        #endregion

        #region Public methods

        public void AddOrderRows(List<CustomerInvoiceRow> customerInvoiceRows, List<ProductUnit> companyProductUnits, List<AttestState> attestStates, AttestState attestStateInitial, int attestStateTransferredOrderToInvoiceId, bool showPartDelivery, bool warnOnReducedQuantityPermission, int productMiscId)
        {
            if (customerInvoiceRows == null)
                return;

            foreach (CustomerInvoiceRow customerInvoiceRow in customerInvoiceRows)
            {
                bool isReadOnly = false;
                bool isInitial = attestStateInitial?.AttestStateId == customerInvoiceRow.AttestStateId;
                string attestStateName = string.Empty;
                string attestStateColor = string.Empty;
                string noAttestStateColor = "#FFFFFF"; //white
                bool canCopy = false;
                bool canMove = false;
                bool showWarnOnReducedQuantity = false;

                #region Decide isLocked

                if (attestStateTransferredOrderToInvoiceId == customerInvoiceRow.AttestStateId)
                {
                    isReadOnly = true;
                }
                else if (customerInvoiceRow.AttestStateId.HasValue && attestStates != null)
                {
                    AttestState aState = attestStates.FirstOrDefault(a => a.AttestStateId == customerInvoiceRow.AttestStateId.Value);
                    if (aState != null && aState.Locked)
                        isReadOnly = true;
                }

                #endregion

                #region Decide AttestStateName/AttestStateColor

                AttestState attestState = attestStates?.FirstOrDefault(a => a.AttestStateId == customerInvoiceRow.AttestStateId);
                attestStateName = attestState?.Name ?? string.Empty;
                attestStateColor = attestState?.Color ?? noAttestStateColor;

                #endregion

                #region Decide CanCopy/CanMove

                canCopy = isInitial && !customerInvoiceRow.IsTimeProjectRow;
                canMove = isInitial && !customerInvoiceRow.IsTimeProjectRow;

                #endregion

                #region Decide WarnOnReducedQuantity

                showWarnOnReducedQuantity = warnOnReducedQuantityPermission && customerInvoiceRow.EdiEntryId.HasValue;


                #endregion


                AddOrderRow(new MobileOrderRow(this.Param, this.OrderId, customerInvoiceRow, companyProductUnits, isReadOnly, isInitial, attestStateName, attestStateColor, showPartDelivery, canCopy, canMove, showWarnOnReducedQuantity, productMiscId));
            }
        }

        public void AddOrderRow(MobileOrderRow mobileOrderRow)
        {
            if (mobileOrderRow == null || mobileOrderRow.Failed || ContainsOrderRow(mobileOrderRow.OrderRowId))
                return;

            mobileOrderRows.Add(mobileOrderRow);
        }

        public void SetAmountDisabled(bool disabled = true)
        {
            foreach (var mobileOrderRow in mobileOrderRows)
            {
                mobileOrderRow.SetAmountDisabled(disabled);
            }
        }

        #endregion

        #region Private methods

        private bool ContainsOrderRow(int orderRowId)
        {
            return mobileOrderRows.Any(i => i.OrderRowId == orderRowId);
        }

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            return MergeDocuments(ROOTNAME, mobileOrderRows.Select(i => i.ToXDocument()).ToList());
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            return XmlUtil.CreateXml(ROOTNAME, MobileOrderRow.GetDefaultXml());
        }

        #endregion
    }

    internal class MobileTimeRows : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "TimeRows";

        #endregion

        #region Variables

        #region Collections

        private List<MobileTimeRow> mobileTimeRows;

        #endregion

        #region Field values

        private readonly int orderId;
        public int OrderId
        {
            get { return orderId; }
        }

        #endregion

        #endregion

        #region Ctor

        /// <summary>Used for get TimeRows</summary>
        public MobileTimeRows(MobileParam param, int orderId, List<ProjectInvoiceDay> projectInvoiceDays, List<TimeCode> timecodes, bool workedTimePermission, bool invoicedTimePermission)
            : base(param)
        {
            Init();
            AddTimeRows(projectInvoiceDays, timecodes, workedTimePermission, invoicedTimePermission);

            this.orderId = orderId;
        }

        /// <summary>Used for errors</summary>
        public MobileTimeRows(MobileParam param, int orderId, string errorMessage) :
            base(param, errorMessage)
        {
            Init();

            this.orderId = orderId;
        }

        private void Init()
        {
            this.mobileTimeRows = new List<MobileTimeRow>();
        }

        #endregion

        #region Public methods

        public void AddTimeRows(List<ProjectInvoiceDay> projectInvoiceDays, List<TimeCode> timecodes, bool workedTimePermission, bool invoicedTimePermission)
        {
            if (projectInvoiceDays == null)
                return;

            foreach (ProjectInvoiceDay projectInvoiceDay in projectInvoiceDays)
            {
                TimeCode timeCode = null;
                if (projectInvoiceDay.ProjectInvoiceWeek.TimeCodeId.HasValue)
                {
                    timeCode = timecodes.FirstOrDefault(t => t.TimeCodeId == projectInvoiceDay.ProjectInvoiceWeek.TimeCodeId.Value);
                }
                AddTimeRow(new MobileTimeRow(this.Param, this.OrderId, projectInvoiceDay, timeCode != null ? timeCode.TimeCodeId : 0, timeCode != null ? timeCode.Name : "", workedTimePermission, invoicedTimePermission));
            }
        }

        public void AddTimeRow(MobileTimeRow mobileTimeRow)
        {
            if (mobileTimeRow == null || mobileTimeRow.Failed /*|| ContainsTimeRow(mobileTimeRow.Date)*/)
                return;

            mobileTimeRows.Add(mobileTimeRow);
        }

        public List<MobileTimeRow> ToList()
        {
            return mobileTimeRows.ToList();
        }


        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            return MergeDocuments(ROOTNAME, mobileTimeRows.Select(i => i.ToXDocument()).ToList());
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            return XmlUtil.CreateXml(ROOTNAME, MobileTimeRow.GetDefaultXml());
        }

        #endregion
    }

    [Log]
    internal class MobileHouseholdDeductionRow : MobileOrderRowBase
    {
        #region Constants

        public const string ROOTNAME = "HouseholdDeductionRow";

        #endregion

        #region Variables
        private readonly int OrderRowId;
        private readonly string PropertyLabel;
        private readonly int HouseHoldTaxDeductionType;
        private readonly string HouseHoldTaxDeductionTypeText;
        private readonly decimal Amount;

        [LogSocSec]
        private readonly string socialSecNr;
        private readonly string name;
        private readonly string apartmentNr;
        private readonly string cooperativeOrgNr;

        #endregion

        #region Ctor

        public MobileHouseholdDeductionRow(MobileParam param, int orderId) : base(param, orderId)
        {
            Init();
        }

        public MobileHouseholdDeductionRow(MobileParam param, int orderId, decimal suggestedAmount, TermGroup_HouseHoldTaxDeductionType houseHoldTaxDeductionType, string houseHoldTaxDeductionTypeText) : base(param, orderId)
        {
            Init();
            Amount = suggestedAmount;
            HouseHoldTaxDeductionType = (int)houseHoldTaxDeductionType;
            HouseHoldTaxDeductionTypeText = houseHoldTaxDeductionTypeText;
        }

        public MobileHouseholdDeductionRow(MobileParam param, int orderId, HouseholdTaxDeductionRow deductionRow, string houseHoldTaxDeductionTypeText) : base(param, orderId)
        {
            Init();

            if (deductionRow != null)
            {
                OrderRowId = deductionRow.CustomerInvoiceRowId;
                PropertyLabel = deductionRow.Property;
                socialSecNr = deductionRow.SocialSecNr;
                name = deductionRow.Name;
                Amount = deductionRow.AmountCurrency;
                apartmentNr = deductionRow.ApartmentNr;
                cooperativeOrgNr = deductionRow.CooperativeOrgNr;
                HouseHoldTaxDeductionType = deductionRow.HouseHoldTaxDeductionType;
                HouseHoldTaxDeductionTypeText = houseHoldTaxDeductionTypeText;
            }
        }

        /// <summary>Used for errors</summary>
        public MobileHouseholdDeductionRow(MobileParam param, int orderId, string errorMessage) : base(param, orderId, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            //Set default values
        }

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();

            elements.Add(new XElement("OrderRowId", OrderRowId.ToString()));
            elements.Add(new XElement("PropertyLbl", PropertyLabel));
            elements.Add(new XElement("SocialSecNr", socialSecNr));
            elements.Add(new XElement("Name", name));
            elements.Add(new XElement("Amount", Amount));
            elements.Add(new XElement("ApartmentNr", apartmentNr));
            elements.Add(new XElement("OrgNr", cooperativeOrgNr));
            elements.Add(new XElement("HHTDType", HouseHoldTaxDeductionType.ToString()));
            elements.Add(new XElement("HHTDTypeText", HouseHoldTaxDeductionTypeText));

            return CreateDocument(ROOTNAME, elements);
        }

        public override XDocument ToXDocument(MobileTask task)
        {
            if (base.Failed)
                return MobileMessages.GetErrorMessageDocument(base.ErrorMessage);

            bool result = base.GetTaskResult(task);
            switch (task)
            {
                case MobileTask.SaveHouseholdDeductionRow:
                    return MobileMessages.GetSuccessDocument(result);
                default:
                    return base.ToXDocument(task);
            }
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            string xml =
                "<OrderRowId>1</OrderRowId>";

            return XmlUtil.CreateXml(ROOTNAME, xml);
        }

        public static string GetDefaultSaveXml()
        {
            string xml =
                "<OrderRowId>1</OrderRowId>";

            return xml;
        }
        #endregion
    }

    internal class MobileHouseHoldDeductionTypes : MobileBase
    {
        public const string ROOTNAME = "MobileHouseHoldDeductionTypes";
        public readonly List<MobileHouseHoldDeductionType> Types = new List<MobileHouseHoldDeductionType>();

        public MobileHouseHoldDeductionTypes(MobileParam param, Dictionary<int, string> types) : base(param)
        {
            AddMobileHouseHoldDeductionTypes(types);
        }

        public void AddMobileHouseHoldDeductionTypes(Dictionary<int, string> types)
        {
            if (types == null)
                return;

            foreach (var type in types)
            {
                AddMobileHouseHoldDeductionType(new MobileHouseHoldDeductionType(this.Param, type.Key, type.Value));
            }
        }

        public void AddMobileHouseHoldDeductionType(MobileHouseHoldDeductionType applicant)
        {
            Types.Add(applicant);
        }

        public override XDocument ToXDocument()
        {
            return MergeDocuments(ROOTNAME, Types.Select(i => i.ToXDocument()).ToList());
        }

        public static string GetDefaultXml()
        {
            return XmlUtil.CreateXml(ROOTNAME, MobileHouseHoldDeductionType.GetDefaultXml());
        }
    }

    internal class MobileHouseHoldDeductionType : MobileBase
    {
        public const string ROOTNAME = "MobileHouseHoldDeductionType";

        public readonly int Id;
        public readonly string Text;

        public MobileHouseHoldDeductionType(MobileParam param, int id, string text) : base(param)
        {
            Id = id;
            Text = text;
        }

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();

            elements.Add(new XElement("Id", Id));
            elements.Add(new XElement("Text", Text));

            return CreateDocument(ROOTNAME, elements);
        }

        public static string GetDefaultXml()
        {
            string xml =
                "<Id>0</Id>" +
                "<Text>Text</Text>";
            return XmlUtil.CreateXml(ROOTNAME, xml);
        }
    }
    internal class MobileHouseholdDeductionApplicants : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "HouseholdDeductionApplicants";

        #endregion

        #region Variables

        #region Collections

        private List<MobileHouseholdDeductionApplicant> applicants;

        #endregion

        #endregion

        #region Ctor

        /// <summary>Used for get OrderRows</summary>
        public MobileHouseholdDeductionApplicants(MobileParam param, List<HouseholdTaxDeductionApplicantDTO> applicants) : base(param)
        {
            Init();
            AddHouseholdDeductionApplicants(applicants);
        }

        /// <summary>Used for errors</summary>
        public MobileHouseholdDeductionApplicants(MobileParam param, string errorMessage) : base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            this.applicants = new List<MobileHouseholdDeductionApplicant>();
        }

        #endregion

        #region Public methods

        public void AddHouseholdDeductionApplicants(List<HouseholdTaxDeductionApplicantDTO> applicants)
        {
            if (applicants == null)
                return;

            foreach (HouseholdTaxDeductionApplicantDTO applicant in applicants)
            {
                AddHouseholdDeductionApplicant(new MobileHouseholdDeductionApplicant(this.Param, applicant));
            }
        }

        public void AddHouseholdDeductionApplicant(MobileHouseholdDeductionApplicant applicant)
        {
            applicants.Add(applicant);
        }

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            return MergeDocuments(ROOTNAME, applicants.Select(i => i.ToXDocument()).ToList());
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            return XmlUtil.CreateXml(ROOTNAME, MobileOrderRow.GetDefaultXml());
        }

        #endregion
    }

    [Log]
    internal class MobileHouseholdDeductionApplicant : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "HouseholdDeductionApplicant";

        #endregion

        #region Variables

        [LogHouseholdDeductionApplicantId]
        private readonly int householdDeductionApplicantId;
        private readonly string propertyLabel;
        [LogSocSec]
        private readonly string socialSecNr;
        private readonly string name;
        private readonly string apartmentNr;
        private readonly string cooperativeOrgNr;
        private readonly decimal share;

        #endregion

        #region Ctor

        public MobileHouseholdDeductionApplicant(MobileParam param) :
            base(param)
        {
            Init();
        }

        public MobileHouseholdDeductionApplicant(MobileParam param, HouseholdTaxDeductionApplicantDTO applicant) : base(param)
        {
            Init();

            if (applicant != null)
            {
                householdDeductionApplicantId = applicant.HouseholdTaxDeductionApplicantId;
                propertyLabel = applicant.Property;
                socialSecNr = applicant.SocialSecNr;
                name = applicant.Name;
                apartmentNr = applicant.ApartmentNr;
                cooperativeOrgNr = applicant.CooperativeOrgNr;
                share = applicant.Share;
            }
        }

        /// <summary>Used for errors</summary>
        public MobileHouseholdDeductionApplicant(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            //Set default values
        }

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();

            elements.Add(new XElement("ApplicantId", householdDeductionApplicantId));
            elements.Add(new XElement("PropertyLbl", GetTextOrCDATA(propertyLabel)));
            elements.Add(new XElement("SocialSecNr", GetTextOrCDATA(socialSecNr)));
            elements.Add(new XElement("Name", GetTextOrCDATA(name)));
            elements.Add(new XElement("ApartmentNr", GetTextOrCDATA(apartmentNr)));
            elements.Add(new XElement("OrgNr", GetTextOrCDATA(cooperativeOrgNr)));
            elements.Add(new XElement("Share", share));

            return CreateDocument(ROOTNAME, elements);
        }

        #endregion
    }

    #endregion

    #region OrderShift

    internal class MobileReloadOrderPlanningSchedule : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "ReloadOrderPlanningSchedule";

        #endregion

        #region Variables

        readonly List<TimeSchedulePlanningDayDTO> shifts;
        readonly List<BreakDTO> breaks;

        #endregion

        #region Ctor

        public MobileReloadOrderPlanningSchedule(MobileParam param, List<TimeSchedulePlanningDayDTO> shifts, List<BreakDTO> breaks)
            : base(param)
        {
            Init();
            this.shifts = shifts;
            this.breaks = breaks;
        }

        public MobileReloadOrderPlanningSchedule(MobileParam param)
            : base(param)
        {
            Init();
        }

        /// <summary>
        /// Used for errors
        /// </summary>
        public MobileReloadOrderPlanningSchedule(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            //Set default values
        }

        #endregion

        #region Public methods

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();

            //From order
            if (!this.Failed)
            {
                // Shifts
                if (!this.shifts.IsNullOrEmpty())
                {
                    var shiftsElement = new XElement("ScheduleShifts",
                        this.shifts.Select(b =>
                            new XElement("ScheduleShift",
                                new XElement("ScheduleShiftStart", b.StartTime),
                                new XElement("ScheduleShiftStop", b.StopTime),
                                new XElement("ScheduleShiftTypeName", b.ShiftTypeName == null ? "" : b.ShiftTypeName)
                            )
                        )
                    );
                    elements.Add(shiftsElement);
                }

                // Breaks
                if (!this.breaks.IsNullOrEmpty())
                {
                    var breaksElement = new XElement("ScheduleBreaks",
                        this.breaks.Select(b =>
                            new XElement("ScheduleBreak",
                                new XElement("ScheduleBreakStart", b.StartTime),
                                new XElement("ScheduleBreakStop", b.StopTime)
                            )
                        )
                    );
                    elements.Add(breaksElement);
                }
            }
            return CreateDocument(ROOTNAME, elements);
        }

        public override XDocument ToXDocument(MobileTask task)
        {
            if (base.Failed)
                return MobileMessages.GetErrorMessageDocument(base.ErrorMessage);

            bool result = base.GetTaskResult(task);
            switch (task)
            {
                case MobileTask.SaveOrderShift:
                    return MobileMessages.GetSuccessDocument(result);
                default:
                    return base.ToXDocument(task);
            }
        }

        #endregion        
    }

    internal class MobileOrderShift : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "OrderShift";

        #endregion

        #region Variables

        readonly TimeSchedulePlanningDayDTO shift;
        readonly CustomerInvoice order;
        readonly bool shiftTypeIsMandatory;
        readonly ShiftType shiftType;
        readonly DateTime start;
        readonly DateTime stop;
        

        #endregion

        #region Ctor

        public MobileOrderShift(MobileParam param, TimeSchedulePlanningDayDTO shift, CustomerInvoice order, Employee employee, bool shiftTypeIsMandatory, ShiftType shiftType, DateTime start, DateTime stop)
            : base(param)
        {
            Init();
            this.shift = shift;
            this.shiftType = shiftType;
            this.order = order;
            this.shiftTypeIsMandatory = shiftTypeIsMandatory;
            this.start = start;
            this.stop = stop;
        }

        public MobileOrderShift(MobileParam param)
            : base(param)
        {
            Init();
        }

        /// <summary>
        /// Used for errors
        /// </summary>
        public MobileOrderShift(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            //Set default values
        }

        #endregion

        #region Public methods

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();

            //From order
            if (!this.Failed)
            {
                elements.Add(new XElement("Priority", this.order.Priority.HasValue ? this.order.Priority.Value.ToString() : ""));
                elements.Add(new XElement("OrderId", this.order.InvoiceId));
                elements.Add(new XElement("OrderNr", this.order.InvoiceNr));
                elements.Add(new XElement("Customer", "(" + this.order.ActorNr + ") " + this.order.ActorName));
                elements.Add(new XElement("Project", "(" + this.order.ProjectNr + ") " + this.order.ProjectName));
                elements.Add(new XElement("Desc", this.order.WorkingDescription));
                elements.Add(new XElement("EstimatedTime", this.order.EstimatedTime));
                elements.Add(new XElement("RemainingTime", this.order.RemainingTime));
                elements.Add(new XElement("PlannedStartDate", this.order.PlannedStartDate.HasValue ? StringUtility.GetSwedishFormattedDate(this.order.PlannedStartDate.Value) : ""));
                elements.Add(new XElement("PlannedStopDate", this.order.PlannedStopDate.HasValue ? StringUtility.GetSwedishFormattedDate(this.order.PlannedStopDate.Value) : ""));

                //From shift
                elements.Add(new XElement("ShiftId", this.shift != null ? this.shift.TimeScheduleTemplateBlockId : 0));

                //From shifttype
                elements.Add(new XElement("ShiftTypeId", this.shiftType != null ? this.shiftType.ShiftTypeId : 0));
                elements.Add(new XElement("ShiftTypeName", this.shiftType != null ? this.shiftType.Name : ""));

                elements.Add(new XElement("Start", this.start.ToShortDateShortTimeString()));
                elements.Add(new XElement("Stop", this.stop.ToShortDateShortTimeString()));

                //Settings
                elements.Add(new XElement("ShiftTypeIsMandatory", StringUtility.GetString(this.shiftTypeIsMandatory)));
            }
            return CreateDocument(ROOTNAME, elements);
        }

        public override XDocument ToXDocument(MobileTask task)
        {
            if (base.Failed)
                return MobileMessages.GetErrorMessageDocument(base.ErrorMessage);

            bool result = base.GetTaskResult(task);
            switch (task)
            {
                case MobileTask.SaveOrderShift:
                    return MobileMessages.GetSuccessDocument(result);
                default:
                    return base.ToXDocument(task);
            }
        }

        #endregion        
    }

    #endregion

    #region TimePeriod

    internal class MobileTimePeriods : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "TimePeriods";

        #endregion

        #region Variables

        #region Collections

        private List<MobileTimePeriod> mobileTimePeriods;

        #endregion

        #endregion

        #region Ctor

        public MobileTimePeriods(MobileParam param, List<TimePeriod> timePeriods)
            : base(param)
        {
            Init();
            AddTimePeriods(timePeriods);
        }

        /// <summary>Used for errors</summary>
        public MobileTimePeriods(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            this.mobileTimePeriods = new List<MobileTimePeriod>();
        }

        #endregion

        #region Public methods

        public void AddTimePeriods(List<TimePeriod> timePeriods)
        {
            if (timePeriods == null)
                return;

            foreach (TimePeriod timePeriod in timePeriods)
            {
                AddTimePeriod(new MobileTimePeriod(this.Param, timePeriod));
            }
        }

        public void AddTimePeriod(MobileTimePeriod mobileTimePeriod)
        {
            if (mobileTimePeriod == null || mobileTimePeriod.Failed)
                return;

            mobileTimePeriods.Add(mobileTimePeriod);
        }

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            return MergeDocuments(ROOTNAME, mobileTimePeriods.Select(i => i.ToXDocument()).ToList());
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            return XmlUtil.CreateXml(ROOTNAME, MobileTimePeriod.GetDefaultXml());
        }

        #endregion
    }

    internal class MobileTimePeriod : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "TimePeriod";

        #endregion

        #region Variables

        #region Field values

        private readonly int timePeriodId;
        public int TimePeriodId
        {
            get { return timePeriodId; }
        }

        private readonly DateTime dateStart;
        public DateTime DateStart
        {
            get { return dateStart; }
        }

        private readonly DateTime dateStop;
        public DateTime DateStop
        {
            get { return dateStop; }
        }

        private readonly bool showAsDefault;
        public bool ShowAsDefault
        {
            get { return showAsDefault; }
        }

        private readonly string name;
        public string Name
        {
            get { return name; }
        }

        #endregion

        #endregion

        #region Ctor

        public MobileTimePeriod(MobileParam param, TimePeriod timePeriod)
            : base(param)
        {
            Init();

            if (timePeriod != null)
            {
                this.timePeriodId = timePeriod.TimePeriodId;
                this.dateStart = timePeriod.StartDate;
                this.dateStop = timePeriod.StopDate;
                this.showAsDefault = timePeriod.ShowAsDefault;
                this.name = timePeriod.Name;
            }
        }

        /// <summary>
        /// Used for errors
        /// </summary>
        public MobileTimePeriod(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            //Set default values
        }

        #endregion

        #region Public methods

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();

            elements.Add(new XElement("TimePeriodId", timePeriodId.ToString()));
            elements.Add(new XElement("Name", Name));
            elements.Add(new XElement("ShowAsDefault", ShowAsDefault.ToString()));
            if (mobileManagerUtil.IsCallerExpectedVersionNewerThenGivenVersion(Param.Version, Constants.MOBILE_WS_SUPPORTED_OLDVERSION_35))
            {
                elements.Add(new XElement("DateStart", StringUtility.GetSwedishFormattedDate(DateStart)));
                elements.Add(new XElement("DateStop", StringUtility.GetSwedishFormattedDate(DateStop))); 
            }
            else
            {
                elements.Add(new XElement("DateStart", DateStart.ToShortDateString()));
                elements.Add(new XElement("DateStop", DateStop.ToShortDateString()));
            }
            return CreateDocument(ROOTNAME, elements);
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            string xml =
                "<TimePeriodId>0</TimePeriodId>" +
                "<Name></Name>" +
                "<DateStart>2009-01-01</DateStart>" +
                "<DateStop>2009-01-31</DateStop>" +
                "<ShowAsDefault>true</ShowAsDefault>";

            return XmlUtil.CreateXml(ROOTNAME, xml);
        }

        #endregion
    }

    #endregion

    #region Employee

    internal class MobileEmployee : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "Employee";

        #endregion

        #region Variables

        #region Field values

        private readonly int employeeId;
        public int EmployeeId
        {
            get { return employeeId; }
        }

        private readonly String employeeName;
        public String EmployeeName
        {
            get { return employeeName; }
        }

        private readonly String employeeNr;
        public String EmployeeNr
        {
            get { return employeeNr; }
        }

        private readonly int employeeGroupId;
        public int EmployeeGroupId
        {
            get { return employeeGroupId; }
        }

        #endregion

        #endregion

        #region Ctor

        public MobileEmployee(MobileParam param, Employee employee)
            : base(param)
        {
            Init();

            if (employee != null)
            {
                this.employeeId = employee.EmployeeId;
                this.employeeName = employee.Name;
                this.employeeNr = employee.EmployeeNr;
                this.employeeGroupId = employee.CurrentEmployeeGroupId;
            }
            //else
            //{
            //    //Temp solution, because of bug (GetEmployee is called for all user from MobileApp)
            //    this.employeeId = 0;
            //    this.employeeName = "NA";
            //    this.employeeNr = "0";
            //    this.employeeGroupId = 0;
            //}
        }

        /// <summary>
        /// Used for errors
        /// </summary>
        public MobileEmployee(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            //Set default values
        }

        #endregion

        #region Public methods

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();

            elements.Add(new XElement("EmployeeId", employeeId.ToString()));
            elements.Add(new XElement("EmployeeName", employeeName));
            elements.Add(new XElement("EmployeeNr", employeeNr));
            elements.Add(new XElement("EmployeeGroupId", employeeGroupId.ToString()));

            return CreateDocument(ROOTNAME, elements);
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            string xml =
                "<EmployeeId>0</EmployeeId>" +
                "<EmployeeName>Kalle Svensson</EmployeeName>" +
                "<EmployeeNr>12345</EmployeeNr>" +
                "<EmployeeGroupId>0</EmployeeGroupId>";

            return XmlUtil.CreateXml(ROOTNAME, xml);
        }

        #endregion
    }

    internal class MobileReplaceWithEmployees : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "ReplaceWithEmployees";

        #endregion

        #region Variables

        #region Collections

        private List<MobileReplaceWithEmployee> mobileReplaceWithEmployees;

        #endregion

        #endregion

        #region Ctor

        public MobileReplaceWithEmployees(MobileParam param, Dictionary<int, string> employees)
            : base(param)
        {
            Init();
            AddReplaceWithEmployees(employees);
        }

        /// <summary>Used for errors</summary>
        public MobileReplaceWithEmployees(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            this.mobileReplaceWithEmployees = new List<MobileReplaceWithEmployee>();
        }

        #endregion

        #region Public methods

        public void AddReplaceWithEmployees(Dictionary<int, string> employees)
        {
            if (employees == null)
                return;

            foreach (var employee in employees)
            {
                AddReplaceWithEmployee(new MobileReplaceWithEmployee(this.Param, employee));
            }
        }

        public void AddReplaceWithEmployee(MobileReplaceWithEmployee employee)
        {

            mobileReplaceWithEmployees.Add(employee);
        }

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            return MergeDocuments(ROOTNAME, mobileReplaceWithEmployees.Select(i => i.ToXDocument()).ToList());
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            return XmlUtil.CreateXml(ROOTNAME, MobileReplaceWithEmployee.GetDefaultXml());
        }

        #endregion
    }

    internal class MobileReplaceWithEmployee : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "ReplaceWithEmployee";

        #endregion

        #region Variables

        #region Field values

        private readonly int id;
        private readonly String name;

        #endregion

        #endregion

        #region Ctor

        public MobileReplaceWithEmployee(MobileParam param, KeyValuePair<int, string> employee)
            : base(param)
        {
            Init();

            this.id = employee.Key;
            this.name = employee.Value;
        }

        /// <summary>
        /// Used for errors
        /// </summary>
        public MobileReplaceWithEmployee(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            //Set default values
        }

        #endregion

        #region Public methods

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();

            elements.Add(new XElement("Id", this.id));
            elements.Add(new XElement("Name", this.name));

            return CreateDocument(ROOTNAME, elements);
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            string xml =
                "<Id></Id>" +
                "<Name></Name>";

            return XmlUtil.CreateXml(ROOTNAME, xml);
        }

        #endregion
    }

    #endregion

    #region DeviationCause

    internal class MobileDeviationCauses : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "DeviationCauses";

        #endregion

        #region Variables

        private readonly List<MobileDeviationCause> mobileDeviationCauses = new List<MobileDeviationCause>();

        #endregion

        #region Ctor

        public MobileDeviationCauses(MobileParam param, List<TimeDeviationCause> timeDeviationCauses) : base(param)
        {
            AddDeviationCauses(timeDeviationCauses);
        }

        /// <summary>Used for errors</summary>
        public MobileDeviationCauses(MobileParam param, string errorMessage) : base(param, errorMessage)
        {

        }

        #endregion

        #region Public methods

        public void AddDeviationCauses(List<TimeDeviationCause> timeDeviationCauses)
        {
            if (timeDeviationCauses == null)
                return;

            foreach (TimeDeviationCause timeDeviationCause in timeDeviationCauses)
            {
                AddDeviationCause(new MobileDeviationCause(this.Param, timeDeviationCause));
            }
        }

        public void AddDeviationCause(MobileDeviationCause mobileDeviationCause)
        {
            if (mobileDeviationCause == null || mobileDeviationCause.Failed)
                return;

            mobileDeviationCauses.Add(mobileDeviationCause);
        }

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            return MergeDocuments(ROOTNAME, mobileDeviationCauses.Select(i => i.ToXDocument()).ToList());
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            return XmlUtil.CreateXml(ROOTNAME, MobileDeviationCause.GetDefaultXml());
        }

        #endregion
    }

    internal class MobileDeviationCause : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "DeviationCause";

        #endregion

        #region Variables

        readonly private int deviationCauseId;
        readonly private string deviationCauseName;
        readonly private bool isPresence;
        readonly private bool isAbsence;
        readonly private bool validatePolicy;
        readonly private bool onlyWholeDay;
        readonly private bool specifyChild;
        readonly private bool notChargeable;
        readonly private bool mandatoryNote;
        readonly private bool mandatoryTime;
        readonly private bool calculateAsOtherTimeInSales;
        readonly private int timeCodeId;


        #endregion

        #region Ctor

        public MobileDeviationCause(MobileParam param, TimeDeviationCause deviationCause) : base(param)
        {
            if (deviationCause != null)
            {
                this.deviationCauseId = deviationCause.TimeDeviationCauseId;
                this.deviationCauseName = deviationCause.Name;
                this.isPresence = deviationCause.IsPresence;
                this.isAbsence = deviationCause.IsAbsence;
                this.validatePolicy = deviationCause.EmployeeRequestPolicyNbrOfDaysBefore > 0;
                this.onlyWholeDay = deviationCause.OnlyWholeDay;
                this.specifyChild = deviationCause.SpecifyChild;
                this.notChargeable = deviationCause.NotChargeable;
                this.mandatoryNote = deviationCause.MandatoryNote;
                this.mandatoryTime = deviationCause.MandatoryTime;
                this.calculateAsOtherTimeInSales = deviationCause.CalculateAsOtherTimeInSales;
                this.timeCodeId = deviationCause.TimeCodeId.GetValueOrDefault();
            }
        }

        /// <summary>
        /// Used for errors
        /// </summary>
        public MobileDeviationCause(MobileParam param, string errorMessage) : base(param, errorMessage)
        {

        }

        #endregion

        #region Public methods

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();

            elements.Add(new XElement("DeviationCauseId", this.deviationCauseId.ToString()));
            elements.Add(new XElement("DeviationCauseName", GetTextOrCDATA(this.deviationCauseName)));
            elements.Add(new XElement("VP", StringUtility.GetString(this.validatePolicy)));
            elements.Add(new XElement("OWH", StringUtility.GetString(this.onlyWholeDay)));
            elements.Add(new XElement("SC", StringUtility.GetString(this.specifyChild)));
            elements.Add(new XElement("IsPresence", this.isPresence)); //do not change this to return "0/1" without handling old app versions
            elements.Add(new XElement("IsAbsence", this.isAbsence));//do not change this to return "0/1" without handling old app versions
            elements.Add(new XElement("NotChargeable", this.notChargeable));
            elements.Add(new XElement("MandatoryNote", this.mandatoryNote));
            elements.Add(new XElement("MandatoryTime", this.mandatoryTime));
            elements.Add(new XElement("CAOTIS", StringUtility.GetString(this.calculateAsOtherTimeInSales)));
            elements.Add(new XElement("TimeCodeId", this.timeCodeId));

            return CreateDocument(ROOTNAME, elements);
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            string xml =
                "<DeviationCauseId></DeviationCauseId>" +
                "<DeviationCauseName></DeviationCauseName>" +
                "<IsPresence></IsPresence>" +
                "<IsAbsence></IsAbsence>";

            return XmlUtil.CreateXml(ROOTNAME, xml);
        }

        #endregion
    }



    #endregion

    #region MobileAttestEmployeeDays

    internal class MobileAttestEmployeeDays : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "AttestEmployeeItems"; //TODO: AttestEmployeeDays

        #endregion

        #region Variables

        private readonly MobileDisplayMode displayMode;

        #region Collections

        private List<MobileAttestEmployeeDay> attestEmployeeDays;

        #endregion

        #endregion

        #region Ctor

        public MobileAttestEmployeeDays(MobileParam param, List<AttestEmployeeDayDTO> attestEmployeeDays, EmployeeGroup employeeGroup = null, bool? appVersionIs11OrOlder = null, MobileDisplayMode displayMode = MobileDisplayMode.User, List<EmployeeRequest> requests = null)
            : base(param)
        {
            Init();
            this.displayMode = displayMode;
            AddAttestEmployeeDays(attestEmployeeDays, employeeGroup, appVersionIs11OrOlder, requests);
        }

        /// <summary>Used for errors</summary>
        public MobileAttestEmployeeDays(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            this.attestEmployeeDays = new List<MobileAttestEmployeeDay>();
        }

        #endregion

        #region Public methods

        public void AddAttestEmployeeDays(List<AttestEmployeeDayDTO> attestEmployeeDays, EmployeeGroup employeeGroup = null, bool? appVersionIs11OrOlder = null, List<EmployeeRequest> requests = null)
        {
            if (attestEmployeeDays == null)
                return;

            foreach (AttestEmployeeDayDTO attestEmployeeDay in attestEmployeeDays)
            {
                AddAttestEmployeeDay(new MobileAttestEmployeeDay(this.Param, attestEmployeeDay, employeeGroup, appVersionIs11OrOlder, displayMode, requests));
            }
        }

        public void AddAttestEmployeeDay(MobileAttestEmployeeDay attestEmployeeDay)
        {
            if (attestEmployeeDay == null || attestEmployeeDay.Failed)
                return;

            attestEmployeeDays.Add(attestEmployeeDay);
        }

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();

            if (displayMode != MobileDisplayMode.Admin)
            {
                int totalWorkedTime = 0;
                int totalPayedTime = 0;

                foreach (var item in attestEmployeeDays)
                {
                    if (item.WorkTime.HasValue)
                        totalWorkedTime += (int)item.WorkTime.Value.TotalMinutes;

                    if (item.PayedTime.HasValue)
                        totalPayedTime += (int)item.PayedTime.Value.TotalMinutes;
                }

                elements.Add(new XElement("TOTWT", CalendarUtility.GetHoursAndMinutesString(totalWorkedTime)));
                elements.Add(new XElement("TOTPT", CalendarUtility.GetHoursAndMinutesString(totalPayedTime)));
            }

            return MergeDocuments(ROOTNAME, attestEmployeeDays.Select(i => i.ToXDocument()).ToList(), elements);
        }

        public override XDocument ToXDocument(MobileTask task)
        {
            if (base.Failed)
                return MobileMessages.GetErrorMessageDocument(base.ErrorMessage);

            bool result = base.GetTaskResult(task);
            switch (task)
            {
                case MobileTask.SaveAbsence:
                case MobileTask.SavePresence:
                case MobileTask.SaveAttest:
                    return MobileMessages.GetSuccessDocument(result);
                default:
                    return base.ToXDocument(task);
            }
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            return XmlUtil.CreateXml(ROOTNAME, MobileDeviationCause.GetDefaultXml());
        }

        public static string GetDefaultSaveXml()
        {
            string xml =
                "<Success>1<Success>";

            return xml;
        }
        #endregion
    }

    internal class MobileAttestEmployeeDay : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "AttestEmployeeItem"; //TODO: AttestEmployeeDay

        #endregion

        #region Variables

        private readonly MobileDisplayMode displayMode;

        #region Field values

        private readonly int schedulePeriodId;
        public int SchedulePeriodId
        {
            get { return schedulePeriodId; }
        }

        private readonly DateTime date;
        public DateTime Date
        {
            get { return date; }
        }

        private DateTime? start;
        public DateTime? Start
        {
            get { return start; }
        }

        private DateTime? stop;
        public DateTime? Stop
        {
            get { return stop; }
        }

        private int? breakInMinutes;
        public int? BreakInMinutes
        {
            get { return breakInMinutes; }
        }

        private TimeSpan? workTime;
        public TimeSpan? WorkTime
        {
            get { return workTime; }
        }

        private TimeSpan? payedTime;
        public TimeSpan? PayedTime
        {
            get { return payedTime; }
        }
        private readonly bool isReadOnly;
        public bool IsReadonly
        {
            get { return isReadOnly; }
        }

        private readonly string color;
        public String Color
        {
            get { return color; }
        }

        private readonly string text;
        public String Text
        {
            get { return text; }
        }

        private readonly bool hasAbsenceRequests;
        public bool HasAbsenceRequests
        {
            get { return hasAbsenceRequests; }
        }

        // Admin
        private readonly TimeSpan? scheduleTime;
        private readonly string attestStateColor;
        private readonly bool hasWorkedInsideSchedule;
        private readonly bool hasWorkedOutsideSchedule;
        private readonly bool hasAbsenceTime;
        private readonly bool hasStandbyTime;
        private readonly bool hasExpenseRows;
        private readonly string timeDeviationCauseNames;
        private readonly bool hasWarnings;
        private readonly string warnings;
        private readonly bool isAttested;
        private readonly bool isToBeAttested;
        private readonly bool hasInfo;
        private readonly string info;
        #endregion

        #endregion

        #region Ctor

        public MobileAttestEmployeeDay(MobileParam param, AttestEmployeeDayDTO attestEmployeeDay, EmployeeGroup employeeGroup = null, bool? appVersionIs11OrOlder = null, MobileDisplayMode displayMode = MobileDisplayMode.User, List<EmployeeRequest> requests = null)
            : base(param)
        {
            Init();

            this.displayMode = displayMode;

            if (attestEmployeeDay != null)
            {
                if (displayMode == MobileDisplayMode.Admin)
                {
                    schedulePeriodId = attestEmployeeDay.TimeScheduleTemplatePeriodId;
                    attestStateColor = attestEmployeeDay.AttestStateColor;
                    scheduleTime = attestEmployeeDay.ScheduleTime;
                    hasWorkedInsideSchedule = attestEmployeeDay.HasWorkedInsideSchedule;
                    hasWorkedOutsideSchedule = attestEmployeeDay.HasWorkedOutsideSchedule;
                    hasAbsenceTime = attestEmployeeDay.HasAbsenceTime;
                    hasStandbyTime = attestEmployeeDay.HasStandbyTime;
                    hasExpenseRows = attestEmployeeDay.HasExpense;
                    date = attestEmployeeDay.Date;
                    timeDeviationCauseNames = attestEmployeeDay.SumGrossSalaryAbsenceText;
                    hasWarnings = attestEmployeeDay.HasWarnings;
                    warnings = attestEmployeeDay.Warnings.Select(x => ((int)x).ToString()).JoinToString(",");
                    isAttested = attestEmployeeDay.IsAttested;
                    isToBeAttested = attestEmployeeDay.IsToBeAttested;
                    hasInfo = attestEmployeeDay.HasInformations;
                    info = attestEmployeeDay.Informations.Select(x => ((int)x).ToString()).JoinToString(",");
                }
                else
                {
                    this.schedulePeriodId = attestEmployeeDay.TimeScheduleTemplatePeriodId;
                    this.date = attestEmployeeDay.Date;
                    this.start = attestEmployeeDay.PresenceStartTime;
                    this.stop = attestEmployeeDay.PresenceStopTime;
                    this.breakInMinutes = attestEmployeeDay.PresenceBreakMinutes;
                    this.workTime = attestEmployeeDay.PresenceTime;
                    this.payedTime = attestEmployeeDay.PresencePayedTime;
                    //temp for bessmanet
                    if (attestEmployeeDay.IsPrel || (employeeGroup != null && !employeeGroup.AutogenTimeblocks))
                        this.isReadOnly = true;
                    else if (attestEmployeeDay.IsScheduleZeroDay)
                        this.isReadOnly = false;
                    else
                        this.isReadOnly = (appVersionIs11OrOlder.HasValue && appVersionIs11OrOlder.Value) && (!(attestEmployeeDay.HasSameAttestState && attestEmployeeDay.AttestStates != null && attestEmployeeDay.AttestStates.Count > 0 && attestEmployeeDay.AttestStates.First().Initial));

                    if (attestEmployeeDay.AttestPayrollTransactions.Any() && attestEmployeeDay.AttestPayrollTransactions.Any(w => !w.AttestStateInitial))
                        this.isAttested = true;

                    if (attestEmployeeDay.HasSameAttestState && attestEmployeeDay.AttestPayrollTransactions.FirstOrDefault() != null)
                        this.color = attestEmployeeDay.AttestPayrollTransactions.FirstOrDefault().AttestStateColor;
                    else if (attestEmployeeDay.IsScheduleZeroDay)
                        this.color = String.Empty;
                    else
                        this.color = COLOR_ORANGE;

                    if (attestEmployeeDay.IsWholedayAbsence && attestEmployeeDay.HasTransactions && attestEmployeeDay.AttestPayrollTransactions.FirstOrDefault() != null)
                        text = attestEmployeeDay.AttestPayrollTransactions.FirstOrDefault().PayrollProductShortName;
                    else
                        text = String.Empty;

                    this.hasAbsenceRequests = requests != null && requests.Any(e => date >= e.Start.Date && date <= e.Stop);
                }

            }
        }

        /// <summary>
        /// Used for errors
        /// </summary>
        public MobileAttestEmployeeDay(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            //Set default values
        }

        #endregion

        #region Public methods

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();

            if (displayMode == MobileDisplayMode.Admin)
            {
                elements.Add(new XElement("SchedulePeriodId", SchedulePeriodId.ToString()));

                if (mobileManagerUtil.IsCallerExpectedVersionNewerThenGivenVersion(Param.Version, Constants.MOBILE_WS_SUPPORTED_OLDVERSION_35))
                {
                    if (scheduleTime.HasValue)
                        elements.Add(new XElement("SchT", StringUtility.GetSwedishFormattedTime(scheduleTime.Value)));
                    else
                        elements.Add(new XElement("SchT", StringUtility.GetSwedishFormattedTime(new TimeSpan(0, 0, 0))));

                    elements.Add(new XElement("Date", StringUtility.GetSwedishFormattedDate(Date)));
                }
                else
                {
                    if (scheduleTime.HasValue)
                        elements.Add(new XElement("SchT", scheduleTime.Value.ToShortTimeString()));
                    else
                        elements.Add(new XElement("SchT", new TimeSpan(0, 0, 0).ToShortTimeString()));

                    elements.Add(new XElement("Date", Date.ToShortDateString()));
                }
                elements.Add(new XElement("ASC", attestStateColor));
                elements.Add(new XElement("HWIS", StringUtility.GetString(hasWorkedInsideSchedule)));
                elements.Add(new XElement("HWOS", StringUtility.GetString(hasWorkedOutsideSchedule)));
                elements.Add(new XElement("HAT", StringUtility.GetString(hasAbsenceTime)));
                elements.Add(new XElement("HST", StringUtility.GetString(hasStandbyTime)));
                elements.Add(new XElement("HE", StringUtility.GetString(hasExpenseRows)));
                elements.Add(new XElement("HW", StringUtility.GetString(hasWarnings)));
                elements.Add(new XElement("WS", warnings));
                elements.Add(new XElement("HI", StringUtility.GetString(hasInfo)));
                elements.Add(new XElement("Info", info));
                elements.Add(new XElement("DCN", timeDeviationCauseNames));
                elements.Add(new XElement("IA", StringUtility.GetString(isAttested)));
                elements.Add(new XElement("IATODO", StringUtility.GetString(isToBeAttested)));
            }
            else
            {
                elements.Add(new XElement("SchedulePeriodId", SchedulePeriodId.ToString()));

                if (mobileManagerUtil.IsCallerExpectedVersionNewerThenGivenVersion(Param.Version, Constants.MOBILE_WS_SUPPORTED_OLDVERSION_35))
                {
                    elements.Add(new XElement("Date", StringUtility.GetSwedishFormattedDate(Date)));
                    if (Start.HasValue)
                        elements.Add(new XElement("Start", StringUtility.GetSwedishFormattedTime(Start.Value)));
                    else
                        elements.Add(new XElement("Start", StringUtility.GetSwedishFormattedTime(new TimeSpan(0, 0, 0))));

                    if (Stop.HasValue)
                        elements.Add(new XElement("Stop", StringUtility.GetSwedishFormattedTime(Stop.Value)));
                    else
                        elements.Add(new XElement("Stop", StringUtility.GetSwedishFormattedTime(new TimeSpan(0, 0, 0))));

                    if (WorkTime.HasValue)
                        elements.Add(new XElement("WorkTime", StringUtility.GetSwedishFormattedTime(WorkTime.Value)));
                    else
                        elements.Add(new XElement("WorkTime", StringUtility.GetSwedishFormattedTime(new TimeSpan(0, 0, 0))));

                    if (PayedTime.HasValue)
                        elements.Add(new XElement("PayedTime", StringUtility.GetSwedishFormattedTime(PayedTime.Value)));
                    else
                        elements.Add(new XElement("PayedTime", StringUtility.GetSwedishFormattedTime(new TimeSpan(0, 0, 0))));
                }
                else
                {
                    elements.Add(new XElement("Date", Date.ToShortDateString()));

                    if (Start.HasValue)
                        elements.Add(new XElement("Start", Start.Value.ToShortTimeString()));
                    else
                        elements.Add(new XElement("Start", new TimeSpan(0, 0, 0).ToShortTimeString()));

                    if (Stop.HasValue)
                        elements.Add(new XElement("Stop", Stop.Value.ToShortTimeString()));
                    else
                        elements.Add(new XElement("Stop", new TimeSpan(0, 0, 0).ToShortTimeString()));

                    if (WorkTime.HasValue)
                        elements.Add(new XElement("WorkTime", WorkTime.Value.ToShortTimeString()));
                    else
                        elements.Add(new XElement("WorkTime", new TimeSpan(0, 0, 0).ToShortTimeString()));

                    if (PayedTime.HasValue)
                        elements.Add(new XElement("PayedTime", PayedTime.Value.ToShortTimeString()));
                    else
                        elements.Add(new XElement("PayedTime", new TimeSpan(0, 0, 0).ToShortTimeString()));
                }

                if (BreakInMinutes.HasValue)
                    elements.Add(new XElement("BreakInMin", BreakInMinutes.Value.ToString()));
                else
                    elements.Add(new XElement("BreakInMin", 0));

                elements.Add(new XElement("IsReadOnly", IsReadonly));
                elements.Add(new XElement("Color", Color));
                elements.Add(new XElement("IA", isAttested));
                elements.Add(new XElement("Text", Text));
                elements.Add(new XElement("HAReq", StringUtility.GetString(HasAbsenceRequests)));
            }

            return CreateDocument(ROOTNAME, elements);
        }

        public override XDocument ToXDocument(MobileTask task)
        {
            if (base.Failed)
                return MobileMessages.GetErrorMessageDocument(base.ErrorMessage);

            bool result = base.GetTaskResult(task);
            switch (task)
            {
                case MobileTask.SaveAbsence:
                case MobileTask.SavePresence:
                case MobileTask.SaveAttest:
                    return MobileMessages.GetSuccessDocument(result);
                default:
                    return base.ToXDocument(task);
            }
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            string xml =
                "<SchedulePeriodId></SchedulePeriodId>" +
                "<Date></Date>" +
                "<Start></Start>" +
                "<Stop></Stop" +
                "<BreakInMin></BreakInMin>" +
                "<WorkTime></WorkTime>" +
                "<IsReadOnly></IsReadonly>" +
                "<Color></Color>" +
                "<Text></Text>";

            return XmlUtil.CreateXml(ROOTNAME, xml);
        }

        public static string GetDefaultSaveXml()
        {
            string xml =
                "<Success>1<Success>";

            return xml;
        }
        #endregion
    }

    #endregion

    #region MobileAttestEmployeePeriod

    internal class MobileAttestEmployeePeriods : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "EmpPeriods";

        #endregion

        #region Variables

        private List<MobileAttestEmployeePeriod> mobileAttestEmployeePeriods;

        #endregion

        #region Ctor

        public MobileAttestEmployeePeriods(MobileParam param, List<AttestEmployeePeriodDTO> attestEmployeePeriods)
            : base(param)
        {
            Init();
            AddMobileAttestEmployeePeriods(attestEmployeePeriods);
        }

        /// <summary>Used for errors</summary>
        public MobileAttestEmployeePeriods(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            this.mobileAttestEmployeePeriods = new List<MobileAttestEmployeePeriod>();
        }

        #endregion

        #region Public methods

        public void AddMobileAttestEmployeePeriods(List<AttestEmployeePeriodDTO> attestEmployeePeriods)
        {
            if (attestEmployeePeriods == null)
                return;

            foreach (AttestEmployeePeriodDTO attestEmployeePeriod in attestEmployeePeriods)
            {
                AddMobileAttestEmployeePeriod(new MobileAttestEmployeePeriod(this.Param, attestEmployeePeriod));
            }
        }

        public void AddMobileAttestEmployeePeriod(MobileAttestEmployeePeriod attestEmployeePeriod)
        {
            if (attestEmployeePeriod == null)
                return;

            mobileAttestEmployeePeriods.Add(attestEmployeePeriod);
        }

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            return MergeDocuments(ROOTNAME, mobileAttestEmployeePeriods.Select(i => i.ToXDocument()).ToList());
        }

        public override XDocument ToXDocument(MobileTask task)
        {
            if (base.Failed)
                return MobileMessages.GetErrorMessageDocument(base.ErrorMessage);

            bool result = base.GetTaskResult(task);
            switch (task)
            {

                case MobileTask.SaveAttest:
                    return MobileMessages.GetSuccessDocument(result);
                default:
                    return base.ToXDocument(task);
            }
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            return XmlUtil.CreateXml(ROOTNAME, MobileDeviationCause.GetDefaultXml());
        }

        public static string GetDefaultSaveXml()
        {
            string xml =
                "<Success>1<Success>";

            return xml;
        }
        #endregion
    }

    internal class MobileAttestEmployeePeriod : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "EmpPeriod";

        #endregion

        #region Variables

        private readonly int employeeId;
        private readonly string employeeName;
        private readonly string attestStateColor;
        private readonly bool isAttested;
        private readonly bool isToBeAttested;
        private readonly string warnings;
        private readonly List<int> additionalOnAccountIds;
        private readonly bool hasWorkedInsideSchedule;
        private readonly bool hasWorkedOutsideSchedule;
        private readonly bool hasAbsenceTime;
        private readonly bool hasStandbyTime;
        private readonly bool hasExpense;
        private readonly bool hasWarnings;
        private readonly bool hasInfo;
        private readonly string info;

        #endregion

        #region Ctor

        public MobileAttestEmployeePeriod(MobileParam param, AttestEmployeePeriodDTO attestEmployeePeriod)
            : base(param)
        {
            Init();

            if (attestEmployeePeriod != null)
            {
                this.employeeId = attestEmployeePeriod.EmployeeId;
                this.employeeName = attestEmployeePeriod.EmployeeNrAndName;
                this.additionalOnAccountIds = attestEmployeePeriod.AdditionalOnAccountIds;
                this.hasWorkedInsideSchedule = attestEmployeePeriod.HasWorkedInsideSchedule;
                this.hasWorkedOutsideSchedule = attestEmployeePeriod.HasWorkedOutsideSchedule;
                this.hasAbsenceTime = attestEmployeePeriod.HasAbsenceTime;
                this.hasStandbyTime = attestEmployeePeriod.HasStandbyTime;
                this.hasExpense = attestEmployeePeriod.HasExpense;
                this.hasWarnings = attestEmployeePeriod.HasWarnings;
                this.attestStateColor = attestEmployeePeriod.AttestStateColor;
                this.isAttested = attestEmployeePeriod.IsAttested;
                this.isToBeAttested = attestEmployeePeriod.IsToBeAttested;
                this.warnings = attestEmployeePeriod.Warnings.Select(x => ((int)x).ToString()).JoinToString(",");
                this.hasInfo = attestEmployeePeriod.HasInformations;
                this.info = attestEmployeePeriod.Informations.Select(x => ((int)x).ToString()).JoinToString(",");
            }
        }

        /// <summary>
        /// Used for errors
        /// </summary>
        public MobileAttestEmployeePeriod(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            //Set default values
        }

        #endregion

        #region Public methods

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>
            {
                new XElement("EmpId", this.employeeId),
                new XElement("EN", this.employeeName),
                new XElement("HWIS", StringUtility.GetString(this.hasWorkedInsideSchedule)),
                new XElement("HWOS", StringUtility.GetString(this.hasWorkedOutsideSchedule)),
                new XElement("HAT", StringUtility.GetString(this.hasAbsenceTime)),
                new XElement("HST", StringUtility.GetString(this.hasStandbyTime)),
                new XElement("HE", StringUtility.GetString(this.hasExpense)),
                new XElement("HW", StringUtility.GetString(this.hasWarnings)),
                new XElement("HI", StringUtility.GetString(this.hasInfo)),
                new XElement("ASC", this.attestStateColor),
                new XElement("IA", StringUtility.GetString(this.isAttested)),
                new XElement("IATODO", StringUtility.GetString(this.isToBeAttested)),
                new XElement("WS", this.warnings),
                new XElement("Info", this.info),
                new XElement("AACC", this.additionalOnAccountIds?.JoinToString(",") ?? "")
            };

            return CreateDocument(ROOTNAME, elements);
        }

        public override XDocument ToXDocument(MobileTask task)
        {
            if (base.Failed)
                return MobileMessages.GetErrorMessageDocument(base.ErrorMessage);

            bool result = base.GetTaskResult(task);
            switch (task)
            {
                case MobileTask.SaveAttest:
                    return MobileMessages.GetSuccessDocument(result);
                default:
                    return base.ToXDocument(task);
            }
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {


            return XmlUtil.CreateXml(ROOTNAME, "");
        }

        public static string GetDefaultSaveXml()
        {
            string xml =
                "<Success>1<Success>";

            return xml;
        }
        #endregion
    }

    #endregion

    #region MyTime Overview

    internal class MobileMyTimeOverview : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "MyTimeOverview";
        public const string PRESENCEABSENCEDETAILS = "PAD";

        #endregion

        private TimeSpan presenceInsideScheduleTime;
        private TimeSpan presenceOutsideScheduleTime;
        private TimeSpan absenceTime;
        private TimeSpan standbyTime;
        private readonly MobileList<string, string> presenceAbsenceDetails = null;
        private MobilePeriodOverviewMessages messages = null;
        private readonly int totalExpenseRows;
        private readonly decimal totalExpenseAmount;
        private readonly bool isPeriodAttest;
        private readonly bool showSetPeriodAsReadyButton;
        private readonly int absenceRequests;

        #region Ctor

        public MobileMyTimeOverview(MobileParam param)
            : base(param)
        {
            Init();

        }

        public MobileMyTimeOverview(MobileParam param, AttestEmployeeOverviewDTO overview, bool showSetPeriodAsReadyButton, int absenceRequests)
            : base(param)
        {
            Init();

            this.presenceInsideScheduleTime = overview.PresenceInsideScheduleTime;
            this.presenceOutsideScheduleTime = overview.PresenceOutsideScheduleTime;
            this.absenceTime = overview.AbsenceTime;
            this.standbyTime = overview.StandbyTime;
            this.presenceAbsenceDetails = new MobileList<string, string>(param, PRESENCEABSENCEDETAILS);
            this.presenceAbsenceDetails.AddRows((int)MobilePresenceAbsenceDetails.PresenceInsideSchedule, overview.PresenceInsideScheduleTimeDetails?.ToStringValueDict());
            this.presenceAbsenceDetails.AddRows((int)MobilePresenceAbsenceDetails.PresenceOutsideSchedule, overview.PresenceOutsideScheduleTimeDetails?.ToStringValueDict());
            this.presenceAbsenceDetails.AddRows((int)MobilePresenceAbsenceDetails.Absence, overview.AbsenceTimeDetails?.ToStringValueDict());
            this.presenceAbsenceDetails.AddRows((int)MobilePresenceAbsenceDetails.Standby, overview.StandbyTimeDetails?.ToStringValueDict());
            this.presenceAbsenceDetails.AddRows((int)MobilePresenceAbsenceDetails.Expense, overview.ExpenseDetails);
            this.totalExpenseRows = overview.SumExpeseRows;
            this.totalExpenseAmount = overview.SumExpenseAmount;
            this.isPeriodAttest = overview.IsPeriodAttested;
            this.showSetPeriodAsReadyButton = showSetPeriodAsReadyButton;
            this.absenceRequests = absenceRequests;

            AddMobilePeriodOverviewMessages(overview.Messages);
        }

        /// <summary>
        /// Used for errors
        /// </summary>
        public MobileMyTimeOverview(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            //Set default values
        }

        #endregion

        #region Public methods

        public void AddMobilePeriodOverviewMessages(List<AttestEmployeeOverviewMessage> messages)
        {
            if (messages == null)
                return;

            this.messages = new MobilePeriodOverviewMessages(this.Param, messages);
        }

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();

            if (this.presenceInsideScheduleTime.TotalMinutes != 0)
                elements.Add(new XElement("PIST", CalendarUtility.GetHoursAndMinutesString((int)this.presenceInsideScheduleTime.TotalMinutes)));
            if (this.presenceOutsideScheduleTime.TotalMinutes != 0)
                elements.Add(new XElement("POST", CalendarUtility.GetHoursAndMinutesString((int)this.presenceOutsideScheduleTime.TotalMinutes)));
            if (this.absenceTime.TotalMinutes != 0)
                elements.Add(new XElement("AT", CalendarUtility.GetHoursAndMinutesString((int)this.absenceTime.TotalMinutes)));
            if (this.standbyTime.TotalMinutes != 0)
                elements.Add(new XElement("ST", CalendarUtility.GetHoursAndMinutesString((int)this.standbyTime.TotalMinutes)));
            elements.Add(this.presenceAbsenceDetails.ToXElement());
            elements.Add(new XElement("IsPA", StringUtility.GetString(this.isPeriodAttest)));
            elements.Add(new XElement("SSPRB", StringUtility.GetString(this.showSetPeriodAsReadyButton)));
            elements.Add(new XElement("AReqs", this.absenceRequests));
            elements.Add(new XElement("TER", this.totalExpenseRows));
            elements.Add(new XElement("TEA", this.totalExpenseAmount));

            return MergeDocuments(ROOTNAME, new List<XDocument> { this.messages.ToXDocument() }, elements);
        }

        #endregion

    }

    internal class MobilePeriodOverviewMessages : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "PeriodMessages";

        #endregion

        #region Variables

        private List<MobilePeriodOverviewMessage> mobileMessages = null;

        #endregion

        #region Ctor

        public MobilePeriodOverviewMessages(MobileParam param, List<AttestEmployeeOverviewMessage> messages)
            : base(param)
        {
            Init();
            AddMobilePeriodOverviewMessages(messages);
        }

        /// <summary>
        /// Used for errors
        /// </summary>
        public MobilePeriodOverviewMessages(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            //Set default values
            mobileMessages = new List<MobilePeriodOverviewMessage>();
        }

        #endregion

        #region Public methods

        public void AddMobilePeriodOverviewMessages(List<AttestEmployeeOverviewMessage> messages)
        {
            foreach (var message in messages)
            {
                AddMobilePeriodOverviewMessage(new MobilePeriodOverviewMessage(this.Param, message));
            }
        }

        public void AddMobilePeriodOverviewMessage(MobilePeriodOverviewMessage mobileMessage)
        {
            if (mobileMessage == null)
                return;

            mobileMessages.Add(mobileMessage);
        }
        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            return MergeDocuments(ROOTNAME, this.mobileMessages.Select(i => i.ToXDocument()).ToList());
        }

        #endregion
    }

    internal class MobilePeriodOverviewMessage : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "PeriodMessage";

        #endregion

        #region Variables

        private readonly AttestEmployeeOverviewMessage message;

        #endregion

        #region Ctor

        public MobilePeriodOverviewMessage(MobileParam param, AttestEmployeeOverviewMessage message)
            : base(param)
        {
            Init();

            this.message = message;
        }

        /// <summary>
        /// Used for errors
        /// </summary>
        public MobilePeriodOverviewMessage(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            //Set default values
        }

        #endregion

        #region Public methods

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();

            elements.Add(new XElement("IsWarn", StringUtility.GetString(message.IsWarning)));
            elements.Add(new XElement("Color", message.Color));
            elements.Add(new XElement("Message", message.Message));


            return CreateDocument(ROOTNAME, elements);
        }

        #endregion
    }

    #endregion

    #region DayView

    internal class MobileDayView : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "DayView";

        #endregion

        #region Variables

        #region Field values

        public int SchedulePeriodId;
        public DateTime Date;
        public DateTime ScheduleStart;
        public DateTime ScheduleStop;
        public int ScheduleBreakInMinutes;
        public TimeSpan ScheduleTime;
        public DateTime ActualStart;
        public DateTime ActualStop;
        public int ActualBreakInMinutes;
        public TimeSpan ActualTime;
        public TimeSpan PayedTime;
        public int FirstTimeBlockId;
        public int LastTimeBlockId;
        public int FirstTimeBlockDeviationCauseId;
        public int LastTimeBlockDeviationCauseId;
        public string FirstTimeBlockComment;
        public string LastTimeBlockComment;

        #endregion

        #endregion

        #region Ctor

        public MobileDayView(MobileParam param)
            : base(param)
        {

        }

        public MobileDayView(MobileParam param, int schedulePeriodId, DateTime date, DateTime scheduleIn, DateTime scheduleOut, int scheduleBreakInMinutes, int actualBreakInMinutes,
                            TimeBlock firstTimeBlock, TimeBlock lastTimeBlock)
            : base(param)
        {
            Init();

            this.SchedulePeriodId = schedulePeriodId;
            this.Date = date;
            this.ScheduleStart = date + scheduleIn.TimeOfDay;
            this.ScheduleStop = date.AddDays((scheduleOut.Date - CalendarUtility.DATETIME_DEFAULT).Days) + scheduleOut.TimeOfDay;
            this.ScheduleBreakInMinutes = scheduleBreakInMinutes;
            this.ScheduleTime = new TimeSpan(0, (((int)(scheduleOut - scheduleIn).TotalMinutes) - scheduleBreakInMinutes), 0);
            this.ActualStart = firstTimeBlock != null ? date + firstTimeBlock.StartTime.TimeOfDay : CalendarUtility.DATETIME_DEFAULT;
            this.ActualStop = (lastTimeBlock != null) ? date.AddDays((lastTimeBlock.StopTime.Date - CalendarUtility.DATETIME_DEFAULT).Days) + lastTimeBlock.StopTime.TimeOfDay : CalendarUtility.DATETIME_DEFAULT;
            this.ActualBreakInMinutes = actualBreakInMinutes;
            this.ActualTime = ((int)(ActualStop - ActualStart).TotalMinutes) != 0 ? new TimeSpan(0, (((int)(ActualStop - ActualStart).TotalMinutes) - actualBreakInMinutes), 0) : new TimeSpan(0, 0, 0);
            this.FirstTimeBlockId = firstTimeBlock != null ? firstTimeBlock.TimeBlockId : 0;
            this.LastTimeBlockId = lastTimeBlock != null ? lastTimeBlock.TimeBlockId : 0;
            this.FirstTimeBlockDeviationCauseId = firstTimeBlock != null && firstTimeBlock.TimeDeviationCauseStartId.HasValue ? firstTimeBlock.TimeDeviationCauseStartId.Value : 0;
            this.LastTimeBlockDeviationCauseId = lastTimeBlock != null && lastTimeBlock.TimeDeviationCauseStartId.HasValue ? lastTimeBlock.TimeDeviationCauseStartId.Value : 0;
        }

        public MobileDayView(MobileParam param, AttestEmployeeDayDTO dayDTO)
            : base(param)
        {
            Init();
            
            this.SchedulePeriodId = dayDTO.TimeScheduleTemplatePeriodId;
            this.Date = dayDTO.Date.Date;
            this.ScheduleStart = Date + dayDTO.ScheduleStartTime.TimeOfDay;
            this.ScheduleStop = Date.AddDays((dayDTO.ScheduleStopTime.Date - CalendarUtility.DATETIME_DEFAULT).Days) + dayDTO.ScheduleStopTime.TimeOfDay;
            this.ScheduleBreakInMinutes = dayDTO.ScheduleBreakMinutes;
            this.ScheduleTime = dayDTO.ScheduleTime;
            this.ActualStart = dayDTO.PresenceStartTime.HasValue ? Date + dayDTO.PresenceStartTime.Value.TimeOfDay : CalendarUtility.DATETIME_DEFAULT;
            this.ActualStop = dayDTO.PresenceStopTime.HasValue ? Date.AddDays((dayDTO.PresenceStopTime.Value.Date - CalendarUtility.DATETIME_DEFAULT).Days) + dayDTO.PresenceStopTime.Value.TimeOfDay : CalendarUtility.DATETIME_DEFAULT;
            this.ActualBreakInMinutes = dayDTO.PresenceBreakMinutes.ToInt();
            this.ActualTime = dayDTO.PresenceTime ?? CalendarUtility.DATETIME_DEFAULT.TimeOfDay;
            this.PayedTime = dayDTO.PresencePayedTime ?? CalendarUtility.DATETIME_DEFAULT.TimeOfDay;
            this.FirstTimeBlockId = dayDTO.FirstPresenceTimeBlockId.ToInt();
            this.LastTimeBlockId = dayDTO.LastPresenceTimeBlockId.ToInt();
            this.FirstTimeBlockDeviationCauseId = dayDTO.FirstPresenceTimeBlockDeviationCauseId.ToInt();
            this.LastTimeBlockDeviationCauseId = dayDTO.LastPresenceTimeBlockDeviationCauseId.ToInt();
            this.FirstTimeBlockComment = dayDTO.FirstPresenceTimeBlockComment;
            this.LastTimeBlockComment = dayDTO.LastPresenceTimeBlockComment;

            if (dayDTO.HasStandbyTime && dayDTO.StandByStartTime != CalendarUtility.DATETIME_DEFAULT && dayDTO.StandByStopTime != CalendarUtility.DATETIME_DEFAULT)
            {
                DateTime standByStartTime = Date + dayDTO.StandByStartTime.TimeOfDay;
                DateTime standByStopTime = Date.AddDays((dayDTO.StandByStopTime.Date - CalendarUtility.DATETIME_DEFAULT).Days) + dayDTO.StandByStopTime.TimeOfDay;

                if (this.ScheduleStart.TimeOfDay.Hours == 0 || standByStartTime < this.ScheduleStart)
                    this.ScheduleStart = standByStartTime;
                if (standByStopTime > this.ScheduleStop)
                    this.ScheduleStop = standByStopTime;
            }
        }
        /// <summary>
        /// Used for errors
        /// </summary>
        public MobileDayView(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            //Set default values
        }

        #endregion

        #region Public methods

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>
            {
                new XElement("SchedulePeriodId", SchedulePeriodId),
                new XElement("ScheduleBreakInMin", ScheduleBreakInMinutes),
                new XElement("ActualBreakInMin", ActualBreakInMinutes),
                new XElement("FirstTimeBlockId", FirstTimeBlockId),
                new XElement("LastTimeBlockId", LastTimeBlockId),
                new XElement("FirstTimeBlockDeviationCauseId", FirstTimeBlockDeviationCauseId),
                new XElement("LastTimeBlockDeviationCauseId", LastTimeBlockDeviationCauseId),
                new XElement("FirstTimeBlockComment", FirstTimeBlockComment),
                new XElement("LastTimeBlockComment", LastTimeBlockComment)
            };


            if (mobileManagerUtil.IsCallerExpectedVersionNewerThenGivenVersion(Param.Version, Constants.MOBILE_WS_SUPPORTED_OLDVERSION_34))
            {
                elements.Add(new XElement("Date", StringUtility.GetSwedishFormattedDate(Date)));
                elements.Add(new XElement("ScheduleStart", StringUtility.GetSwedishFormattedDateTime(ScheduleStart)));
                elements.Add(new XElement("ScheduleStop", StringUtility.GetSwedishFormattedDateTime(ScheduleStop)));
                elements.Add(new XElement("ScheduleTime", StringUtility.GetSwedishFormattedTime((ScheduleTime))));
                elements.Add(new XElement("ActualStart", StringUtility.GetSwedishFormattedDateTime(ActualStart)));
                elements.Add(new XElement("ActualStop", StringUtility.GetSwedishFormattedDateTime(ActualStop)));
                elements.Add(new XElement("ActualTime", StringUtility.GetSwedishFormattedTime(ActualTime)));
                elements.Add(new XElement("PayedTime", StringUtility.GetSwedishFormattedTime(PayedTime)));
            }
            else
            {
                elements.Add(new XElement("Date", Date.ToShortDateString()));
                elements.Add(new XElement("ScheduleStart", ScheduleStart));
                elements.Add(new XElement("ScheduleStop", ScheduleStop));
                elements.Add(new XElement("ScheduleTime", ScheduleTime.ToShortTimeString()));
                elements.Add(new XElement("ActualStart", ActualStart));
                elements.Add(new XElement("ActualStop", ActualStop));
                elements.Add(new XElement("ActualTime", ActualTime.ToShortTimeString()));
                elements.Add(new XElement("PayedTime", PayedTime.ToShortTimeString()));

            }

           



            return CreateDocument(ROOTNAME, elements);
        }

        public override XDocument ToXDocument(MobileTask task)
        {
            if (base.Failed)
                return MobileMessages.GetErrorMessageDocument(base.ErrorMessage);

            bool result = base.GetTaskResult(task);
            switch (task)
            {
                case MobileTask.SaveDeviations:
                case MobileTask.RestoreToSchedule:
                    return MobileMessages.GetSuccessDocument(result);
                default:
                    return base.ToXDocument(task);
            }
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            string xml =
                "<SchedulePeriodId></SchedulePeriodId>" +
                "<Date></Date>" +
                "<ScheduleStart></ScheduleStart>" +
                "<ScheduleStop></ScheduleStop>" +
                "<ScheduleBreakInMin></ScheduleBreakInMin>" +
                "<ScheduleTime></ScheduleTime>" +
                "<ActualStart></ActualStart>" +
                "<ActualStop></ActualStop" +
                "<ActualBreakInMin></ActualBreakInMin>" +
                "<ActualTime></ActualTime>" +
                "<FirstTimeBlockId></FirstTimeBlockId>" +
                "<LastTimeBlockId></LastTimeBlockId>" +
                "<FirstTimeBlockDeviationCauseId></FirstTimeBlockDeviationCauseId>" +
                "<LastTimeBlockDeviationCauseId></LastTimeBlockDeviationCauseId>";

            return XmlUtil.CreateXml(ROOTNAME, xml);
        }

        public static string GetDefaultSaveXml()
        {
            string xml =
                "<Success>1<Success>";

            return xml;
        }
        #endregion
    }

    #endregion

    #region Breaks

    internal class MobileBreaks : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "Breaks";

        #endregion

        #region Variables

        #region Collections

        private List<MobileBreak> mobileBreaks;

        #endregion

        #endregion

        #region Ctor

        public MobileBreaks(MobileParam param, List<Tuple<int, TimeCodeBreak, List<TimeBlock>, int>> tuples)
            : base(param)
        {
            Init();
            AddMobileBreaks(tuples);
        }

        /// <summary>Used for errors</summary>
        public MobileBreaks(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            this.mobileBreaks = new List<MobileBreak>();
        }

        #endregion

        #region Public methods

        public void AddMobileBreaks(List<Tuple<int, TimeCodeBreak, List<TimeBlock>, int>> tuples)
        {
            foreach (var tuple in tuples)
            {
                int currentMinutes = 0;
                foreach (var timeBlock in tuple.Item3)
                {
                    currentMinutes += timeBlock.TotalMinutes;
                }
                AddMobileBreak(new MobileBreak(this.Param, tuple.Item1, tuple.Item2, currentMinutes, tuple.Item4));
            }
        }

        public void AddMobileBreak(MobileBreak mobileBreak)
        {
            if (mobileBreak == null)
                return;

            mobileBreaks.Add(mobileBreak);
        }

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            return MergeDocuments(ROOTNAME, mobileBreaks.Select(i => i.ToXDocument()).ToList());
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            return XmlUtil.CreateXml(ROOTNAME, MobileBreak.GetDefaultXml());
        }

        public static string GetDefaultSaveXml()
        {
            string xml =
                "<Success>1<Success>";

            return xml;
        }
        #endregion
    }

    internal class MobileBreak : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "Break";

        #endregion

        #region Variables

        #region Field values

        private readonly int scheduleBlockId;
        public int ScheduleBlockId
        {
            get { return scheduleBlockId; }
        }

        private readonly int timeCodeBreakId;
        public int TimeCodeBreakId
        {
            get { return timeCodeBreakId; }
        }

        private readonly string name;
        public string Name
        {
            get { return name; }
        }

        private readonly int currentMinutes;
        public int CurrentMinutes
        {
            get { return currentMinutes; }
        }

        private readonly int order;
        public int Order
        {
            get { return order; }
        }

        #endregion

        #endregion

        #region Ctor

        public MobileBreak(MobileParam param)
            : base(param)
        {
            Init();
        }

        public MobileBreak(MobileParam param, int scheduleBlockId, TimeCodeBreak timeCodeBreak, int breakMinutes, int order)
            : base(param)
        {
            Init();

            if (timeCodeBreak != null && scheduleBlockId > 0)
            {
                this.scheduleBlockId = scheduleBlockId;
                this.timeCodeBreakId = timeCodeBreak.TimeCodeId;
                this.name = timeCodeBreak.Name;
                this.currentMinutes = breakMinutes;
                this.order = order;
            }
        }

        /// <summary>
        /// Used for errors
        /// </summary>
        public MobileBreak(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            //Set default values
        }

        #endregion

        #region Public methods

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();

            elements.Add(new XElement("Order", Order));
            elements.Add(new XElement("ScheduleBlockId", ScheduleBlockId));
            elements.Add(new XElement("TimeCodeBreakId", TimeCodeBreakId));
            elements.Add(new XElement("Name", Name));
            elements.Add(new XElement("CurrentMinutes", CurrentMinutes));


            return CreateDocument(ROOTNAME, elements);
        }

        public override XDocument ToXDocument(MobileTask task)
        {
            if (base.Failed)
                return MobileMessages.GetErrorMessageDocument(base.ErrorMessage);

            bool result = base.GetTaskResult(task);
            switch (task)
            {
                case MobileTask.SaveBreak:
                    return MobileMessages.GetSuccessDocument(result);
                default:
                    return base.ToXDocument(task);
            }
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            string xml =
                "<Order></Order>" +
                "<ScheduleBlockId></ScheduleBlockId>" +
                "<TimeCodeBreakId></TimeCodeBreakId>" +
                "<Name></Name>" +
                "<CurrentMinutes></CurrentMinutes>";

            return XmlUtil.CreateXml(ROOTNAME, xml);
        }

        public static string GetDefaultSaveXml()
        {
            string xml =
                "<Success>1<Success>";

            return xml;
        }

        #endregion
    }

    #endregion

    #region SoftOneProducts

    internal class SoftOneProducts : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "SoftOneProducts";

        #endregion

        #region Variables

        #region Collections

        private List<SoftOneProduct> softOneProducts;

        #endregion

        #endregion

        #region Ctor

        public SoftOneProducts(MobileParam param)
            : base(param)
        {
            Init();
        }

        private void Init()
        {
            this.softOneProducts = new List<SoftOneProduct>();

            AddSoftOneProduct("XE", SoeMobileType.XE, "https://s1s1d1.softone.se/WSX/Mobile/MobileService.asmx", "", "", "");
            AddSoftOneProduct("XE2", SoeMobileType.XE, "https://s1s1d2.softone.se/WSX/Mobile/MobileService.asmx", "", "", "");
            AddSoftOneProduct("ICA", SoeMobileType.XE, "https://s1s1d7.softone.se/WSX/Mobile/MobileService.asmx", "", "", "");
            // AddSoftOneProduct("Next", SoeMobileType.XE, "http://next.softone.se/WSX/Mobile/MobileService.asmx", "", "", "");
            AddSoftOneProduct("Gratiskonto hantverkare", SoeMobileType.XE, "https://try.softone.se/WSX/Mobile/MobileService.asmx", "101", "Gratiskonto", "Gratis123");
            AddSoftOneProduct("Demo XE Personal", SoeMobileType.XE, "https://try.softone.se/WSX/Mobile/MobileService.asmx", "101", "521", "Demo123");
            AddSoftOneProduct("Demo XE Hantverkare", SoeMobileType.XE, "https://try.softone.se/WSX/Mobile/MobileService.asmx", "101", "Sture", "Demo123");
            AddSoftOneProduct("Skolversion Elektriker", SoeMobileType.XE, "https://skola.softone.se/WSX/Mobile/MobileService.asmx", "", "", "");

            AddSoftOneProduct("Professional", SoeMobileType.Professional, "", "", "", "");
            //  AddSoftOneProduct("Sauma", SoeMobileType.Sauma, "", "", "", "");
        }

        public void AddSoftOneProduct(String name, SoeMobileType type, string url, string license, string userName, String pwd)
        {
            SoftOneProduct softOneProduct = new SoftOneProduct(this.Param, name, type, url, license, userName, pwd);

            softOneProducts.Add(softOneProduct);
        }

        #endregion

        #region Public methods

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            return MergeDocuments(ROOTNAME, softOneProducts.Select(i => i.ToXDocument()).ToList());
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            return XmlUtil.CreateXml(ROOTNAME, SoftOneProduct.GetDefaultXml());
        }

        #endregion
    }

    internal class SoftOneProduct : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "SoftOneProduct";

        #endregion

        #region Variables

        #region Field values

        private readonly string name;
        private readonly SoeMobileType type;
        private readonly string url;
        private readonly string license;
        private readonly string userName;
        private readonly string pwd;

        #endregion

        #endregion

        #region Ctor

        public SoftOneProduct(MobileParam param, String name, SoeMobileType type, String url, string license, string userName, String pwd)
            : base(param)
        {
            Init();

            this.name = name;
            this.type = type;
            this.url = url;
            this.license = license;
            this.userName = userName;
            this.pwd = pwd;

        }

        private void Init()
        {
            //Set default values
        }

        #endregion

        #region Public methods

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();

            elements.Add(new XElement("Name", name));
            elements.Add(new XElement("Type", (int)type));
            elements.Add(new XElement("Url", url));
            elements.Add(new XElement("UL", license));
            elements.Add(new XElement("UN", userName));
            elements.Add(new XElement("UP", pwd));

            return CreateDocument(ROOTNAME, elements);
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            string xml =
                "<Name></Name>" +
                "<Type></Type>" +
                "<Url></Url>" +
                "<UL></UL>" +
                "<UN></UN>" +
                "<UP></UP>";

            return XmlUtil.CreateXml(ROOTNAME, xml);
        }

        #endregion
    }

    #endregion

    #region FieldSettings

    internal class MobileFieldSettings : MobileBase
    {
        #region Variables

        protected bool showAllFields;
        protected List<FieldSetting> fieldSettings;

        private readonly Dictionary<TermGroup_MobileFields, bool> evaluatedFields;

        #endregion

        #region Ctor

        public MobileFieldSettings(MobileParam param)
            : base(param)
        {
            this.showAllFields = false;
            this.fieldSettings = new List<FieldSetting>();
            this.evaluatedFields = new Dictionary<TermGroup_MobileFields, bool>();
        }

        public MobileFieldSettings(MobileParam param, List<FieldSetting> fieldSettings, bool showAllFields = false)
            : base(param)
        {
            this.showAllFields = showAllFields;
            this.fieldSettings = fieldSettings;
            this.evaluatedFields = new Dictionary<TermGroup_MobileFields, bool>();
        }

        /// <summary>Used for errors</summary>
        public MobileFieldSettings(MobileParam param, string errorMessage)
            : base(param, errorMessage)
        {
            this.fieldSettings = new List<FieldSetting>();
            this.evaluatedFields = new Dictionary<TermGroup_MobileFields, bool>();
        }

        #endregion

        #region Protected methods

        protected XElement GetSettingElement(string elementName, bool defaultValueVisible = true)
        {
            XElement element = new XElement(elementName);

            //Add settings
            AddSettingElements(element, defaultValueVisible);

            return element;
        }

        protected XElement GetSettingElement(string elementName, TermGroup_MobileFields mobileField, bool defaultValueVisible = true)
        {
            XElement element = new XElement(elementName);

            //Add settings
            AddSettingElements(element, mobileField, defaultValueVisible);

            return element;
        }

        protected bool DoShowField(TermGroup_MobileFields mobileField, bool defaultValue = true)
        {
            //Show all fields
            if (this.showAllFields)
                return true;

            //Check evaluated fields
            if (this.evaluatedFields.ContainsKey(mobileField))
                return evaluatedFields[mobileField];

            //Check fieldsettings
            bool doShowField = defaultValue;
            List<FieldSetting> fieldSettingsForField = GetFieldsSettingsForField(mobileField);
            foreach (FieldSetting fieldSetting in fieldSettingsForField)
            {
                foreach (FieldSettingDetail fieldSettingDetail in fieldSetting.GetFieldSettingDetails())
                {
                    if (fieldSettingDetail.SysSettingId == (int)SoeSetting.Visible)
                        doShowField = StringUtility.GetBool(fieldSettingDetail.Value);
                }
            }

            //Set field to evaluated
            this.evaluatedFields.Add(mobileField, doShowField);

            return doShowField;
        }

        #endregion

        #region Private methods

        private void AddSettingElements(XElement element, bool defaultValueVisible = true)
        {
            if (element == null)
                return;

            element.Value = StringUtility.GetString(defaultValueVisible);
        }

        private void AddSettingElements(XElement element, TermGroup_MobileFields mobileField, bool defaultValueVisible = true)
        {
            if (element == null)
                return;

            //Visible only supported fieldsetting type for now
            //When more types are supported - add a element for each type of fieldsetting

            bool doShowField = DoShowField(mobileField, defaultValueVisible);
            element.Value = StringUtility.GetString(doShowField);
        }

        private List<FieldSetting> GetFieldsSettingsForField(TermGroup_MobileFields mobileField)
        {
            if (this.fieldSettings == null)
                return new List<FieldSetting>();

            //Get settings for given field. Orderby RoleId so null values (i.e. CompanyFieldSettings) returns last
            return (from fs in fieldSettings
                    where fs.FieldName == ((int)mobileField).ToString()
                    select fs).OrderByDescending(i => i.RoleId).ToList();
        }

        #endregion
    }

    #endregion

    #region Customers Grid/Edit

    internal class MobileCustomerElements : MobileFieldSettings
    {
        #region Variables

        #region XML Elements

        protected const String CustomerIdElement = "CustomerId";
        protected const String CustomerNrElement = "CustomerNr";
        protected const String CustomerNameElement = "CustomerName";
        protected const String OrganisationNrElement = "OrganisationNr";
        protected const String VatNrElement = "VatNr";
        protected const String ReferenceElement = "Reference";
        protected const String ContactPersonElement = "ContactPerson";
        protected const String EmailAddressElement = "EmailAddress";
        protected const String EmailAddressIdElement = "EmailAddressId";
        protected const String HomePhoneElement = "HomePhone";
        protected const String HomePhoneIdElement = "HomePhoneId";
        protected const String WorkPhoneElement = "WorkPhone";
        protected const String WorkPhoneIdElement = "WorkPhoneId";
        protected const String CellPhoneElement = "CellPhone";
        protected const String CellPhoneIdElement = "CellPhoneId";
        protected const String FaxElement = "Fax";
        protected const String FaxIdElement = "FaxId";
        protected const String InvoiceAddressIdElement = "InvoiceAddressId";
        protected const String InvoiceAddressElement = "InvoiceAddress";
        protected const String InvoiceAddressPostalCodeElement = "IAPostalCode";
        protected const String InvoiceAddressPostalAddressElement = "IAPostalAddress";
        protected const String InvoiceAddressCountryElement = "IACountry";
        protected const String InvoiceAddressAddressCOElement = "IAAddressCO";
        protected const String DeliveryAddress1IdElement = "DeliveryAddress1Id";
        protected const String DeliveryAddress1Element = "DeliveryAddress1";
        protected const String DeliveryAddress1PostalCodeElement = "DA1PostalCode";
        protected const String DeliveryAddress1PostalAddressElement = "DA1PostalAddress";
        protected const String DeliveryAddress1NameElement = "DA1Name";
        protected const String DeliveryAddress1CountryElement = "DA1Country";
        protected const String DeliveryAddress1AddressCOElement = "DA1AddressCO";
        protected const String DeliveryAddress2Element = "DeliveryAddress2";
        protected const String VatTypeElement = "VatType";
        protected const String VatTypeIdElement = "VatTypeId";
        protected const String VatTypeNameElement = "VatTypeName";
        protected const String PaymentConditionElement = "PaymentCondition";
        protected const String PaymentConditionIdElement = "PaymentConditionId";
        protected const String PaymentConditionNameElement = "PaymentConditionName";
        protected const String SalesPriceListElement = "SalesPriceList";
        protected const String SalesPriceListIdElement = "SalesPriceListId";
        protected const String SalesPriceListNameElement = "SalesPriceListName";
        protected const String StandardWholeSellerElement = "StandardWholeSeller";
        protected const String StandardWholeSellerIdElement = "StandardWholeSellerId";
        protected const String StandardWholeSellerNameElement = "StandardWholeSellerName";
        protected const String DiscountArticlesElement = "DiscountArticles";
        protected const String DiscountServicesElement = "DiscountServices";
        protected const String CurrencyElement = "Currency";
        protected const String CurrencyIdElement = "CurrencyId";
        protected const String CurrencyNameElement = "CurrencyName";
        protected const String NoteElement = "Note";

        #endregion

        #endregion

        #region Ctor

        public MobileCustomerElements(MobileParam param) : base(param)
        {

        }

        public MobileCustomerElements(MobileParam param, List<FieldSetting> fieldSettings) : base(param, fieldSettings)
        {

        }

        /// <summary>Used for errors</summary>
        public MobileCustomerElements(MobileParam param, string errorMessage) : base(param, errorMessage)
        {
        }

        #endregion
    }

    internal class MobileCustomerEditFieldSettings : MobileCustomerElements
    {
        #region Constants

        public const string ROOTNAME = "CustomerEditFieldSettings";

        #endregion

        #region Variables

        #region Field values

        private readonly bool editCustomerPermission;
        private readonly bool editHHTDApplicantsPermission;

        #endregion

        #endregion

        #region Ctor

        public MobileCustomerEditFieldSettings(MobileParam param, List<FieldSetting> fieldSettings, bool editCustomerPermission, bool editHHTDApplicantsPermission) : base(param, fieldSettings)
        {
            this.editCustomerPermission = editCustomerPermission;
            this.editHHTDApplicantsPermission = editHHTDApplicantsPermission;
        }


        #endregion

        #region Public methods

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();

            elements.Add(GetSettingElement(CustomerNrElement));
            elements.Add(GetSettingElement(CustomerNameElement));
            elements.Add(GetSettingElement(OrganisationNrElement, TermGroup_MobileFields.CustomerEdit_OrganisationNr));
            elements.Add(GetSettingElement(VatNrElement, TermGroup_MobileFields.CustomerEdit_VatNr));
            elements.Add(GetSettingElement(ReferenceElement, TermGroup_MobileFields.CustomerEdit_Reference));
            elements.Add(GetSettingElement(EmailAddressElement, TermGroup_MobileFields.CustomerEdit_EmailAddress));
            elements.Add(GetSettingElement(HomePhoneElement, TermGroup_MobileFields.CustomerEdit_PhoneHome));
            elements.Add(GetSettingElement(WorkPhoneElement, TermGroup_MobileFields.CustomerEdit_PhoneJob));
            elements.Add(GetSettingElement(CellPhoneElement, TermGroup_MobileFields.CustomerEdit_PhoneMobile));
            elements.Add(GetSettingElement(FaxElement, TermGroup_MobileFields.CustomerEdit_Fax));
            elements.Add(GetSettingElement(InvoiceAddressElement, TermGroup_MobileFields.CustomerEdit_InvoiceAddress));
            elements.Add(GetSettingElement(DeliveryAddress1Element, TermGroup_MobileFields.CustomerEdit_DeliveryAddress1));
            elements.Add(GetSettingElement(VatTypeElement, TermGroup_MobileFields.CustomerEdit_VatType));
            elements.Add(GetSettingElement(PaymentConditionElement, TermGroup_MobileFields.CustomerEdit_PaymentCondition));
            elements.Add(GetSettingElement(SalesPriceListElement, TermGroup_MobileFields.CustomerEdit_SalesPriceList));
            elements.Add(GetSettingElement(StandardWholeSellerElement, TermGroup_MobileFields.CustomerEdit_StandardWholeSeller));
            elements.Add(GetSettingElement(DiscountArticlesElement, TermGroup_MobileFields.CustomerEdit_DiscountArticles));
            elements.Add(GetSettingElement(DiscountServicesElement, TermGroup_MobileFields.CustomerEdit_DiscountServices));
            elements.Add(GetSettingElement(CurrencyElement, TermGroup_MobileFields.CustomerEdit_Currency));
            elements.Add(GetSettingElement(NoteElement, TermGroup_MobileFields.CustomerEdit_Note));
            elements.Add(new XElement("ECustomer", StringUtility.GetString(editCustomerPermission)));
            elements.Add(new XElement("EHHTDApplicants", StringUtility.GetString(editHHTDApplicantsPermission)));
            elements.Add(GetSettingElement("InvoiceDeliveryType", TermGroup_MobileFields.CustomerEdit_InvoiceDeliveryType));

            return CreateDocument(ROOTNAME, elements);
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            string xml =
                "<CustomerNr>1</CustomerNr>" +
                "<CustomerName>1</CustomerName>" +
                "<OrganisationNr>1</OrganisationNr>" +
                "<VatNr>1</VatNr>" +
                "<Reference>1</Reference>" +
                "<EmailAddress>1</EmailAddress>" +
                "<HomePhone>1</HomePhone>" +
                "<WorkPhone>1</WorkPhone>" +
                "<CellPhone>1</CellPhone>" +
                "<Fax>1</Fax>" +
                "<InvoiceAddress>1</InvoiceAddress>" +
                "<DeliveryAddress1>1</DeliveryAddress1>" +
                //"<DeliveryAddress2>1</DeliveryAddress2>" +
                "<VatType>1</VatType>" +
                "<PaymentCondition>1</PaymentCondition>" +
                "<SalesPriceList>1</SalesPriceList>" +
                "<StandardWholeSeller>1</StandardWholeSeller>" +
                "<DiscountArticles>1</DiscountArticles>" +
                "<DiscountServices>1</DiscountServices>" +
                "<Currency>1</Currency>" +
                "<Note>1</Note>";

            return XmlUtil.CreateXml(ROOTNAME, xml);
        }

        #endregion
    }

    internal class MobileCustomerGridFieldSettings : MobileCustomerElements
    {
        #region Constants

        public const string ROOTNAME = "CustomerGridFieldSettings";

        #endregion

        #region Ctor

        public MobileCustomerGridFieldSettings(MobileParam param, List<FieldSetting> fieldSettings) : base(param, fieldSettings)
        {

        }

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();

            elements.Add(GetSettingElement(CustomerNrElement));
            elements.Add(GetSettingElement(CustomerNameElement));
            elements.Add(GetSettingElement(OrganisationNrElement, TermGroup_MobileFields.CustomerGrid_OrganisationNr));
            elements.Add(GetSettingElement(VatNrElement, TermGroup_MobileFields.CustomerGrid_VatNr, false));                            //not yet electable as a fieldsetting in XE
            elements.Add(GetSettingElement(ReferenceElement, TermGroup_MobileFields.CustomerGrid_Reference));
            elements.Add(GetSettingElement(EmailAddressElement, TermGroup_MobileFields.CustomerGrid_EmailAddress, false));              //not yet electable as a fieldsetting in XE
            elements.Add(GetSettingElement(HomePhoneElement, TermGroup_MobileFields.CustomerGrid_PhoneHome, false));                    //not yet electable as a fieldsetting in XE
            elements.Add(GetSettingElement(WorkPhoneElement, TermGroup_MobileFields.CustomerGrid_PhoneJob, false));                     //not yet electable as a fieldsetting in XE
            elements.Add(GetSettingElement(CellPhoneElement, TermGroup_MobileFields.CustomerGrid_PhoneMobile, false));                  //not yet electable as a fieldsetting in XE
            elements.Add(GetSettingElement(FaxElement, TermGroup_MobileFields.CustomerGrid_Fax, false));                                //not yet electable as a fieldsetting in XE
            elements.Add(GetSettingElement(InvoiceAddressElement, TermGroup_MobileFields.CustomerGrid_InvoiceAddress, false));
            elements.Add(GetSettingElement(DeliveryAddress1Element, TermGroup_MobileFields.CustomerGrid_DeliveryAddress1, false));
            elements.Add(GetSettingElement(VatTypeElement, TermGroup_MobileFields.CustomerGrid_VatType, false));                        //not yet electable as a fieldsetting in XE
            elements.Add(GetSettingElement(PaymentConditionElement, TermGroup_MobileFields.CustomerGrid_PaymentCondition, false));      //not yet electable as a fieldsetting in XE
            elements.Add(GetSettingElement(SalesPriceListElement, TermGroup_MobileFields.CustomerGrid_SalesPriceList, false));          //not yet electable as a fieldsetting in XE
            elements.Add(GetSettingElement(StandardWholeSellerElement, TermGroup_MobileFields.CustomerGrid_StandardWholeSeller, false));//not yet electable as a fieldsetting in XE
            elements.Add(GetSettingElement(DiscountArticlesElement, TermGroup_MobileFields.CustomerGrid_DiscountArticles, false));      //not yet electable as a fieldsetting in XE
            elements.Add(GetSettingElement(DiscountServicesElement, TermGroup_MobileFields.CustomerGrid_DiscountServices, false));      //not yet electable as a fieldsetting in XE
            elements.Add(GetSettingElement(CurrencyElement, TermGroup_MobileFields.CustomerGrid_Currency, false));                      //not yet electable as a fieldsetting in XE
            elements.Add(GetSettingElement(NoteElement, TermGroup_MobileFields.CustomerGrid_Note, false));                              //not yet electable as a fieldsetting in XE

            return CreateDocument(ROOTNAME, elements);
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            string xml =
                "<CustomerNr>1</CustomerNr>" +
                "<CustomerName>1</CustomerName>" +
                "<OrganisationNr>1</OrganisationNr>" +
                "<VatNr>0</VatNr>" +
                "<Reference>1</Reference>" +
                "<EmailAddress>0</EmailAddress>" +
                "<HomePhone>0</HomePhone>" +
                "<WorkPhone>0</WorkPhone>" +
                "<CellPhone>0</CellPhone>" +
                "<Fax>0</Fax>" +
                "<InvoiceAddress>0</InvoiceAddress>" +
                "<DeliveryAddress1>0</DeliveryAddress1>" +
                //"<DeliveryAddress2>0</DeliveryAddress2>" +
                "<VatType>0</VatType>" +
                "<PaymentCondition>0</PaymentCondition>" +
                "<SalesPriceList>0</SalesPriceList>" +
                "<StandardWholeSeller>0</StandardWholeSeller>" +
                "<DiscountArticles>0</DiscountArticles>" +
                "<DiscountServices>0</DiscountServices>" +
                "<Currency>0</Currency>" +
                "<Note>0</Note>";

            return XmlUtil.CreateXml(ROOTNAME, xml);
        }

        #endregion
    }

    internal class MobileCustomersGrid : MobileCustomerElements
    {
        #region Constants

        public const string ROOTNAME = "MobileCustomersGrid";

        #endregion

        #region Variables

        private readonly List<MobileCustomerGrid> mobileCustomersGrid = new List<MobileCustomerGrid>();

        #endregion

        #region Ctor

        public MobileCustomersGrid(MobileParam param, List<CustomerSearchView> customers, List<FieldSetting> fieldSettings) : base(param, fieldSettings)
        {
            AddMobileCustomersGrid(customers);
        }

        /// <summary>Used for errors</summary>
        public MobileCustomersGrid(MobileParam param, string errorMessage) : base(param, errorMessage)
        {

        }

        #endregion

        #region Public methods

        public void AddMobileCustomersGrid(List<CustomerSearchView> customers)
        {

            foreach (var customerResult in customers.GroupBy(c => c.ActorCustomerId))
            {
                var first = customerResult.First();

                if (first != null)
                {
                    var deliveryAddress = String.Empty;
                    var billingAddress = String.Empty;
                    Helpers.GetCustomerAddresses(customerResult, out deliveryAddress, out billingAddress);
                    AddMobileCustomerGrid(new MobileCustomerGrid(this.Param, first.ActorCustomerId, first.CustomerNr, first.Name, first.OrgNr, first.InvoiceReference, deliveryAddress, billingAddress, fieldSettings));
                }
            }
        }

        public void AddMobileCustomerGrid(MobileCustomerGrid mobileCustomerGrid)
        {
            if (mobileCustomerGrid == null)
                return;

            mobileCustomersGrid.Add(mobileCustomerGrid);
        }

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            return MergeDocuments(ROOTNAME, mobileCustomersGrid.Select(i => i.ToXDocument()).ToList());
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            return XmlUtil.CreateXml(ROOTNAME, MobileCustomerGrid.GetDefaultXml());
        }

        #endregion
    }

    internal class MobileCustomerGrid : MobileCustomerElements
    {
        #region Constants

        public const string ROOTNAME = "MobileCustomerGrid";

        #endregion

        private readonly int ActorCustomerId;
        private readonly string CustomerNr;
        private readonly string Name;
        private readonly string OrgNr;
        private readonly string InvoiceReference;
        private readonly string DeliveryAddress;
        private readonly string BillingAddress;

        #region Ctor
        public MobileCustomerGrid(MobileParam param, int actorCustomerId, string customerNr, string name, string orgNr, string invoiceReference, string deliveryAddress, string billingAddress, List<FieldSetting> fieldSettings) : base(param, fieldSettings)
        {
            this.ActorCustomerId = actorCustomerId;
            this.CustomerNr = customerNr;
            this.Name = name;
            this.OrgNr = orgNr;
            this.InvoiceReference = invoiceReference;
            this.DeliveryAddress = deliveryAddress;
            this.BillingAddress = billingAddress;
        }

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();

            elements.Add(new XElement(CustomerIdElement, ActorCustomerId));
            elements.Add(new XElement(CustomerNrElement, GetTextOrCDATA(CustomerNr)));
            elements.Add(new XElement(CustomerNameElement, GetTextOrCDATA(Name)));
            if (DoShowField(TermGroup_MobileFields.CustomerGrid_OrganisationNr))
            {
                elements.Add(new XElement(OrganisationNrElement, GetTextOrCDATA(OrgNr)));
            }
            if (DoShowField(TermGroup_MobileFields.CustomerGrid_Reference))
            {
                elements.Add(new XElement(ReferenceElement, GetTextOrCDATA(InvoiceReference)));
            }
            if (DoShowField(TermGroup_MobileFields.CustomerGrid_DeliveryAddress1))
            {
                elements.Add(new XElement(DeliveryAddress1Element, GetTextOrCDATA(DeliveryAddress)));
            }
            if (DoShowField(TermGroup_MobileFields.CustomerGrid_InvoiceAddress))
            {
                elements.Add(new XElement(InvoiceAddressElement, GetTextOrCDATA(BillingAddress)));
            }

            return CreateDocument(ROOTNAME, elements);
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            string xml =
                "<CustomerId>1</CustomerId>" +
                "<CustomerNr>123</CustomerNr>" +
                "<CustomerName>Kalle</CustomerName>" +
                "<OrganisationNr>74101-2565</OrganisationNr>" +
                "<VatNr>0</VatNr>" +
                "<Reference>Nisse</Reference>" +
                "<EmailAddress>mail@mail.com</EmailAddress>" +
                "<HomePhone>123456</HomePhone>" +
                "<WorkPhone>7891011</WorkPhone>" +
                "<CellPhone>070-00000000</CellPhone>" +
                "<Fax></Fax>" +
                "<InvoiceAddress>Fakturaddress</InvoiceAddress>" +
                "<DeliveryAddress1>Leveransaddress</DeliveryAddress1>" +
                "<VatTypeName></VatTypeName>" +
                "<PaymentConditionName></PaymentConditionName>" +
                "<SalesPriceListName></SalesPriceListName>" +
                "<StandardWholeSellerName></StandardWholeSellerName>" +
                "<DiscountArticles></DiscountArticles>" +
                "<DiscountServices></DiscountServices>" +
                "<CurrencyName></CurrencyName>" +
                "<Note>Notering</Note>";

            return XmlUtil.CreateXml(ROOTNAME, xml);
        }

        #endregion
    }

    internal class MobileCustomerEdit : MobileCustomerElements
    {
        #region Constants

        public const string ROOTNAME = "MobileCustomerEdit";

        #endregion

        #region Variables
        private readonly Customer customer;
        private readonly int customerId;
        private readonly string vatTypename;
        private readonly string wholeSellerName;
        private readonly string currencyName;
        private readonly string invoiceDeliveryTypeName;

        //Ecom
        private readonly int emailAddressId = 0;
        private readonly string emailAddress = String.Empty;
        private readonly int homePhoneId = 0;
        private readonly string homePhone = String.Empty;
        private readonly int jobPhoneId = 0;
        private readonly string jobPhone = String.Empty;
        private readonly int mobilePhoneId = 0;
        private readonly string mobilePhone = String.Empty;
        private readonly int faxId = 0;
        private readonly string fax = String.Empty;

        //Address
        private readonly int invoiceAddressId = 0;
        private readonly string invoiceAddress = String.Empty;
        private readonly string invoiceAddressPostalCode = String.Empty;
        private readonly string invoiceAddressPostalAddress = String.Empty;
        private readonly string invoiceAddressAddressCO = string.Empty;
        private readonly string invoiceAddressCountry = string.Empty;
        private readonly int deliveryAddressId1 = 0;
        private readonly string deliveryAddress1 = String.Empty;
        private readonly string deliveryAddress1PostalCode = String.Empty;
        private readonly string deliveryAddress1PostalAddress = String.Empty;
        private readonly string deliveryAddress1Name = string.Empty;
        private readonly string deliveryAddress1AddressCO = string.Empty;
        private readonly string deliveryAddress1Country = string.Empty;

        #endregion

        #region Ctor

        public MobileCustomerEdit(MobileParam param) : base(param)
        {

        }

        public MobileCustomerEdit(MobileParam param, int customerId) : base(param)
        {
            this.customerId = customerId;
        }

        public MobileCustomerEdit(MobileParam param, Customer customer, List<ContactECom> contactEcoms, List<ContactAddress> contactAdrress, string vatTypename, string wholeSellerName, string currencyName, string invoiceDeliveryTypeName, List<FieldSetting> fieldSettings) : base(param, fieldSettings)
        {
            this.customerId = customer.ActorCustomerId;
            this.customer = customer;
            this.vatTypename = vatTypename;
            this.wholeSellerName = wholeSellerName;
            this.currencyName = currencyName;
            this.invoiceDeliveryTypeName = invoiceDeliveryTypeName;

            List<ContactECom> emails = contactEcoms.Where(c => c.SysContactEComTypeId == (int)TermGroup_SysContactEComType.Email).ToList();
            List<ContactECom> homePhones = contactEcoms.Where(c => c.SysContactEComTypeId == (int)TermGroup_SysContactEComType.PhoneHome).ToList();
            List<ContactECom> mobilePhones = contactEcoms.Where(c => c.SysContactEComTypeId == (int)TermGroup_SysContactEComType.PhoneMobile).ToList();
            List<ContactECom> jobPhones = contactEcoms.Where(c => c.SysContactEComTypeId == (int)TermGroup_SysContactEComType.PhoneJob).ToList();
            List<ContactECom> faxes = contactEcoms.Where(c => c.SysContactEComTypeId == (int)TermGroup_SysContactEComType.Fax).ToList();
            List<ContactAddress> deliveryAddresses = contactAdrress.Where(c => c.SysContactAddressTypeId == (int)TermGroup_SysContactAddressType.Delivery).ToList();
            List<ContactAddress> billingAddresses = contactAdrress.Where(c => c.SysContactAddressTypeId == (int)TermGroup_SysContactAddressType.Billing).ToList();

            #region Email

            if (emails.Count > 0)
            {
                var lastCreatedEmail = emails.OrderByDescending(o => o.Created).FirstOrDefault();
                var lastModifiedEmail = emails.OrderByDescending(o => o.Modified).FirstOrDefault();
                bool useLastModified = false;

                if (lastModifiedEmail.Modified.HasValue && lastCreatedEmail.Created.HasValue && lastModifiedEmail.Modified.Value > lastCreatedEmail.Created.Value)
                    useLastModified = true;

                if (useLastModified)
                {
                    emailAddressId = lastModifiedEmail.ContactEComId;
                    emailAddress = lastModifiedEmail.Text;
                }
                else
                {
                    emailAddressId = lastCreatedEmail.ContactEComId;
                    emailAddress = lastCreatedEmail.Text;
                }
            }

            #endregion

            #region HomePhone

            if (homePhones.Count > 0)
            {
                var lastCreatedPhoneHome = homePhones.OrderByDescending(o => o.Created).FirstOrDefault();
                var lastModifiedPhomeHome = homePhones.OrderByDescending(o => o.Modified).FirstOrDefault();
                bool useLastModified = false;

                if (lastModifiedPhomeHome.Modified.HasValue && lastCreatedPhoneHome.Created.HasValue && lastModifiedPhomeHome.Modified.Value > lastCreatedPhoneHome.Created.Value)
                    useLastModified = true;

                if (useLastModified)
                {
                    homePhoneId = lastModifiedPhomeHome.ContactEComId;
                    homePhone = lastModifiedPhomeHome.Text;
                }
                else
                {
                    homePhoneId = lastCreatedPhoneHome.ContactEComId;
                    homePhone = lastCreatedPhoneHome.Text;
                }
            }

            #endregion

            #region MobilePhone

            if (mobilePhones.Count > 0)
            {
                var lastCreatedPhoneMobile = mobilePhones.OrderByDescending(o => o.Created).FirstOrDefault();
                var lastModifiedPhomeMobile = mobilePhones.OrderByDescending(o => o.Modified).FirstOrDefault();
                bool useLastModified = false;

                if (lastModifiedPhomeMobile.Modified.HasValue && lastCreatedPhoneMobile.Created.HasValue && lastModifiedPhomeMobile.Modified.Value > lastCreatedPhoneMobile.Created.Value)
                    useLastModified = true;

                if (useLastModified)
                {
                    mobilePhoneId = lastModifiedPhomeMobile.ContactEComId;
                    mobilePhone = lastModifiedPhomeMobile.Text;
                }
                else
                {
                    mobilePhoneId = lastCreatedPhoneMobile.ContactEComId;
                    mobilePhone = lastCreatedPhoneMobile.Text;
                }
            }

            #endregion

            #region JobPhone

            if (jobPhones.Count > 0)
            {
                var lastCreatedPhoneJob = jobPhones.OrderByDescending(o => o.Created).FirstOrDefault();
                var lastModifiedPhomeJob = jobPhones.OrderByDescending(o => o.Modified).FirstOrDefault();
                bool useLastModified = false;

                if (lastModifiedPhomeJob.Modified.HasValue && lastCreatedPhoneJob.Created.HasValue && lastModifiedPhomeJob.Modified.Value > lastCreatedPhoneJob.Created.Value)
                    useLastModified = true;

                if (useLastModified)
                {
                    jobPhoneId = lastModifiedPhomeJob.ContactEComId;
                    jobPhone = lastModifiedPhomeJob.Text;
                }
                else
                {
                    jobPhoneId = lastCreatedPhoneJob.ContactEComId;
                    jobPhone = lastCreatedPhoneJob.Text;
                }
            }

            #endregion

            #region Fax

            if (faxes.Count > 0)
            {
                var lastCreatedFax = faxes.OrderByDescending(o => o.Created).FirstOrDefault();
                var lastModifiedFax = faxes.OrderByDescending(o => o.Modified).FirstOrDefault();
                bool useLastModified = false;

                if (lastModifiedFax.Modified.HasValue && lastCreatedFax.Created.HasValue && lastModifiedFax.Modified.Value > lastCreatedFax.Created.Value)
                    useLastModified = true;
                if (useLastModified)
                {
                    faxId = lastModifiedFax.ContactEComId;
                    fax = lastModifiedFax.Text;
                }
                else
                {
                    faxId = lastCreatedFax.ContactEComId;
                    fax = lastCreatedFax.Text;
                }
            }

            #endregion

            #region DeliverAddress

            if (deliveryAddresses.Count > 0)
            {
                var lastCreatedDeliveryAddress = deliveryAddresses.OrderByDescending(o => o.Created).FirstOrDefault();
                var lastModifiedDeliveryAddress = deliveryAddresses.OrderByDescending(o => o.Modified).FirstOrDefault();
                bool useLastModified = false;

                if (lastModifiedDeliveryAddress.Modified.HasValue && lastCreatedDeliveryAddress.Created.HasValue && lastModifiedDeliveryAddress.Modified.Value > lastCreatedDeliveryAddress.Created.Value)
                    useLastModified = true;

                if (useLastModified)
                {
                    deliveryAddressId1 = lastModifiedDeliveryAddress.ContactAddressId;
                    var addressrow = lastModifiedDeliveryAddress.ContactAddressRow.FirstOrDefault(c => c.SysContactAddressRowTypeId == (int)TermGroup_SysContactAddressRowType.Address);
                    if (addressrow != null)
                        deliveryAddress1 = addressrow.Text;

                    var postalCodeRow = lastModifiedDeliveryAddress.ContactAddressRow.FirstOrDefault(c => c.SysContactAddressRowTypeId == (int)TermGroup_SysContactAddressRowType.PostalCode);
                    if (postalCodeRow != null)
                        deliveryAddress1PostalCode = postalCodeRow.Text;

                    var postalAddressRow = lastModifiedDeliveryAddress.ContactAddressRow.FirstOrDefault(c => c.SysContactAddressRowTypeId == (int)TermGroup_SysContactAddressRowType.PostalAddress);
                    if (postalAddressRow != null)
                        deliveryAddress1PostalAddress = postalAddressRow.Text;

                    var nameRow = lastModifiedDeliveryAddress.ContactAddressRow.FirstOrDefault(c => c.SysContactAddressRowTypeId == (int)TermGroup_SysContactAddressRowType.Name);
                    if (nameRow != null)
                        deliveryAddress1Name = nameRow.Text;

                    var countryRow = lastModifiedDeliveryAddress.ContactAddressRow.FirstOrDefault(c => c.SysContactAddressRowTypeId == (int)TermGroup_SysContactAddressRowType.Country);
                    if (countryRow != null)
                        deliveryAddress1Country = countryRow.Text;

                    var addressCORow = lastModifiedDeliveryAddress.ContactAddressRow.FirstOrDefault(c => c.SysContactAddressRowTypeId == (int)TermGroup_SysContactAddressRowType.AddressCO);
                    if (addressCORow != null)
                        deliveryAddress1AddressCO = addressCORow.Text;
                }
                else
                {
                    deliveryAddressId1 = lastCreatedDeliveryAddress.ContactAddressId;
                    var addressrow = lastCreatedDeliveryAddress.ContactAddressRow.FirstOrDefault(c => c.SysContactAddressRowTypeId == (int)TermGroup_SysContactAddressRowType.Address);
                    if (addressrow != null)
                        deliveryAddress1 = addressrow.Text;

                    var postalCodeRow = lastCreatedDeliveryAddress.ContactAddressRow.FirstOrDefault(c => c.SysContactAddressRowTypeId == (int)TermGroup_SysContactAddressRowType.PostalCode);
                    if (postalCodeRow != null)
                        deliveryAddress1PostalCode = postalCodeRow.Text;

                    var postalAddressRow = lastCreatedDeliveryAddress.ContactAddressRow.FirstOrDefault(c => c.SysContactAddressRowTypeId == (int)TermGroup_SysContactAddressRowType.PostalAddress);
                    if (postalAddressRow != null)
                        deliveryAddress1PostalAddress = postalAddressRow.Text;

                    var nameRow = lastCreatedDeliveryAddress.ContactAddressRow.FirstOrDefault(c => c.SysContactAddressRowTypeId == (int)TermGroup_SysContactAddressRowType.Name);
                    if (nameRow != null)
                        deliveryAddress1Name = nameRow.Text;

                    var countryRow = lastCreatedDeliveryAddress.ContactAddressRow.FirstOrDefault(c => c.SysContactAddressRowTypeId == (int)TermGroup_SysContactAddressRowType.Country);
                    if (countryRow != null)
                        deliveryAddress1Country = countryRow.Text;

                    var addressCORow = lastCreatedDeliveryAddress.ContactAddressRow.FirstOrDefault(c => c.SysContactAddressRowTypeId == (int)TermGroup_SysContactAddressRowType.AddressCO);
                    if (addressCORow != null)
                        deliveryAddress1AddressCO = addressCORow.Text;
                }
            }


            #endregion

            #region BillingAddress

            if (billingAddresses.Count > 0)
            {
                var lastCreatedBillingAddress = billingAddresses.OrderByDescending(o => o.Created).FirstOrDefault();
                var lastModifiedBillingAddress = billingAddresses.OrderByDescending(o => o.Modified).FirstOrDefault();
                bool useLastModified = false;

                if (lastModifiedBillingAddress.Modified.HasValue && lastCreatedBillingAddress.Created.HasValue && lastModifiedBillingAddress.Modified.Value > lastCreatedBillingAddress.Created.Value)
                    useLastModified = true;

                if (useLastModified)
                {
                    invoiceAddressId = lastModifiedBillingAddress.ContactAddressId;
                    var addressrow = lastModifiedBillingAddress.ContactAddressRow.FirstOrDefault(c => c.SysContactAddressRowTypeId == (int)TermGroup_SysContactAddressRowType.Address);
                    if (addressrow != null)
                        invoiceAddress = addressrow.Text;

                    var postalCodeRow = lastModifiedBillingAddress.ContactAddressRow.FirstOrDefault(c => c.SysContactAddressRowTypeId == (int)TermGroup_SysContactAddressRowType.PostalCode);
                    if (postalCodeRow != null)
                        invoiceAddressPostalCode = postalCodeRow.Text;

                    var postalAddressRow = lastModifiedBillingAddress.ContactAddressRow.FirstOrDefault(c => c.SysContactAddressRowTypeId == (int)TermGroup_SysContactAddressRowType.PostalAddress);
                    if (postalAddressRow != null)
                        invoiceAddressPostalAddress = postalAddressRow.Text;

                    var countryRow = lastModifiedBillingAddress.ContactAddressRow.FirstOrDefault(c => c.SysContactAddressRowTypeId == (int)TermGroup_SysContactAddressRowType.Country);
                    if (countryRow != null)
                        invoiceAddressCountry = countryRow.Text;

                    var addressCORow = lastModifiedBillingAddress.ContactAddressRow.FirstOrDefault(c => c.SysContactAddressRowTypeId == (int)TermGroup_SysContactAddressRowType.AddressCO);
                    if (addressCORow != null)
                        invoiceAddressAddressCO = addressCORow.Text;
                }
                else
                {
                    invoiceAddressId = lastCreatedBillingAddress.ContactAddressId;
                    var addressrow = lastCreatedBillingAddress.ContactAddressRow.FirstOrDefault(c => c.SysContactAddressRowTypeId == (int)TermGroup_SysContactAddressRowType.Address);
                    if (addressrow != null)
                        invoiceAddress = addressrow.Text;

                    var postalCodeRow = lastCreatedBillingAddress.ContactAddressRow.FirstOrDefault(c => c.SysContactAddressRowTypeId == (int)TermGroup_SysContactAddressRowType.PostalCode);
                    if (postalCodeRow != null)
                        invoiceAddressPostalCode = postalCodeRow.Text;

                    var postalAddressRow = lastCreatedBillingAddress.ContactAddressRow.FirstOrDefault(c => c.SysContactAddressRowTypeId == (int)TermGroup_SysContactAddressRowType.PostalAddress);
                    if (postalAddressRow != null)
                        invoiceAddressPostalAddress = postalAddressRow.Text;

                    var countryRow = lastCreatedBillingAddress.ContactAddressRow.FirstOrDefault(c => c.SysContactAddressRowTypeId == (int)TermGroup_SysContactAddressRowType.Country);
                    if (countryRow != null)
                        invoiceAddressCountry = countryRow.Text;

                    var addressCORow = lastCreatedBillingAddress.ContactAddressRow.FirstOrDefault(c => c.SysContactAddressRowTypeId == (int)TermGroup_SysContactAddressRowType.AddressCO);
                    if (addressCORow != null)
                        invoiceAddressAddressCO = addressCORow.Text;
                }
            }
            #endregion
        }

        /// <summary>Used for errors</summary>
        public MobileCustomerEdit(MobileParam param, string errorMessage) : base(param, errorMessage)
        {

        }

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {

            var elements = new List<XElement>();

            elements.Add(new XElement(CustomerIdElement, this.customerId));
            elements.Add(new XElement(CustomerNrElement, GetTextOrCDATA(this.customer.CustomerNr)));
            elements.Add(new XElement(CustomerNameElement, GetTextOrCDATA(this.customer.Name)));
            if (DoShowField(TermGroup_MobileFields.CustomerEdit_OrganisationNr))
            {
                elements.Add(new XElement(OrganisationNrElement, GetTextOrCDATA(this.customer.OrgNr)));
            }
            if (DoShowField(TermGroup_MobileFields.CustomerEdit_VatNr))
            {
                elements.Add(new XElement(VatNrElement, GetTextOrCDATA(this.customer.VatNr)));
            }
            if (DoShowField(TermGroup_MobileFields.CustomerEdit_Reference))
            {
                elements.Add(new XElement(ReferenceElement, GetTextOrCDATA(this.customer.InvoiceReference)));
            }
            if (DoShowField(TermGroup_MobileFields.CustomerEdit_EmailAddress))
            {
                elements.Add(new XElement(EmailAddressIdElement, this.emailAddressId));
                elements.Add(new XElement(EmailAddressElement, GetTextOrCDATA(this.emailAddress)));
            }
            if (DoShowField(TermGroup_MobileFields.CustomerEdit_PhoneHome))
            {
                elements.Add(new XElement(HomePhoneIdElement, this.homePhoneId));
                elements.Add(new XElement(HomePhoneElement, GetTextOrCDATA(this.homePhone)));
            }
            if (DoShowField(TermGroup_MobileFields.CustomerEdit_PhoneJob))
            {
                elements.Add(new XElement(WorkPhoneIdElement, this.jobPhoneId));
                elements.Add(new XElement(WorkPhoneElement, GetTextOrCDATA(this.jobPhone)));
            }
            if (DoShowField(TermGroup_MobileFields.CustomerEdit_PhoneMobile))
            {
                elements.Add(new XElement(CellPhoneIdElement, this.mobilePhoneId));
                elements.Add(new XElement(CellPhoneElement, GetTextOrCDATA(this.mobilePhone)));
            }
            if (DoShowField(TermGroup_MobileFields.CustomerEdit_Fax))
            {
                elements.Add(new XElement(FaxIdElement, this.faxId));
                elements.Add(new XElement(FaxElement, GetTextOrCDATA(this.fax)));
            }
            if (DoShowField(TermGroup_MobileFields.CustomerEdit_InvoiceAddress))
            {
                elements.Add(new XElement(InvoiceAddressIdElement, this.invoiceAddressId));
                elements.Add(new XElement(InvoiceAddressElement, GetTextOrCDATA(this.invoiceAddress)));
                elements.Add(new XElement(InvoiceAddressPostalCodeElement, GetTextOrCDATA(this.invoiceAddressPostalCode)));
                elements.Add(new XElement(InvoiceAddressPostalAddressElement, GetTextOrCDATA(this.invoiceAddressPostalAddress)));
                elements.Add(new XElement(InvoiceAddressCountryElement, GetTextOrCDATA(this.invoiceAddressCountry)));
                elements.Add(new XElement(InvoiceAddressAddressCOElement, GetTextOrCDATA(this.invoiceAddressAddressCO)));
            }
            if (DoShowField(TermGroup_MobileFields.CustomerEdit_DeliveryAddress1))
            {
                elements.Add(new XElement(DeliveryAddress1IdElement, this.deliveryAddressId1));
                elements.Add(new XElement(DeliveryAddress1Element, GetTextOrCDATA(this.deliveryAddress1)));
                elements.Add(new XElement(DeliveryAddress1PostalCodeElement, GetTextOrCDATA(this.deliveryAddress1PostalCode)));
                elements.Add(new XElement(DeliveryAddress1PostalAddressElement, GetTextOrCDATA(this.deliveryAddress1PostalAddress)));
                elements.Add(new XElement(DeliveryAddress1NameElement, GetTextOrCDATA(this.deliveryAddress1Name)));
                elements.Add(new XElement(DeliveryAddress1CountryElement, GetTextOrCDATA(this.deliveryAddress1Country)));
                elements.Add(new XElement(DeliveryAddress1AddressCOElement, GetTextOrCDATA(this.deliveryAddress1AddressCO)));
            }
            if (DoShowField(TermGroup_MobileFields.CustomerEdit_VatType))
            {
                elements.Add(new XElement(VatTypeIdElement, this.customer.VatType));
                elements.Add(new XElement(VatTypeNameElement, GetTextOrCDATA(this.vatTypename)));
            }
            if (DoShowField(TermGroup_MobileFields.CustomerEdit_PaymentCondition))
            {
                elements.Add(new XElement(PaymentConditionIdElement, this.customer.PaymentCondition != null ? this.customer.PaymentConditionId.Value : 0));
                elements.Add(new XElement(PaymentConditionNameElement, this.customer.PaymentCondition != null ? GetTextOrCDATA(this.customer.PaymentCondition.Name) : ""));
            }
            if (DoShowField(TermGroup_MobileFields.CustomerEdit_SalesPriceList))
            {
                elements.Add(new XElement(SalesPriceListIdElement, this.customer.PriceListType != null ? this.customer.PriceListTypeId.Value : 0));
                elements.Add(new XElement(SalesPriceListNameElement, this.customer.PriceListType != null ? GetTextOrCDATA(this.customer.PriceListType.Name) : ""));
            }
            if (DoShowField(TermGroup_MobileFields.CustomerEdit_StandardWholeSeller))
            {
                elements.Add(new XElement(StandardWholeSellerIdElement, this.customer.SysWholeSellerId ?? 0));
                elements.Add(new XElement(StandardWholeSellerNameElement, GetTextOrCDATA(this.wholeSellerName)));
            }
            if (DoShowField(TermGroup_MobileFields.CustomerEdit_DiscountArticles))
            {
                elements.Add(new XElement(DiscountArticlesElement, this.customer.DiscountMerchandise));
            }
            if (DoShowField(TermGroup_MobileFields.CustomerEdit_DiscountServices))
            {
                elements.Add(new XElement(DiscountServicesElement, this.customer.DiscountService));
            }
            if (DoShowField(TermGroup_MobileFields.CustomerEdit_Currency))
            {
                elements.Add(new XElement(CurrencyIdElement, this.customer.CurrencyId));
                elements.Add(new XElement(CurrencyNameElement, GetTextOrCDATA(this.currencyName)));
            }
            if (DoShowField(TermGroup_MobileFields.CustomerEdit_Note))
            {
                elements.Add(new XElement(NoteElement, GetTextOrCDATA(this.customer.Note)));
            }
            if(DoShowField(TermGroup_MobileFields.CustomerEdit_InvoiceDeliveryType))
            {
                elements.Add(new XElement("IDTId", this.customer.InvoiceDeliveryType.HasValue ? this.customer.InvoiceDeliveryType : 0));
                elements.Add(new XElement("IDTName", GetTextOrCDATA(this.invoiceDeliveryTypeName)));
            }

            return CreateDocument(ROOTNAME, elements);
        }

        public override XDocument ToXDocument(MobileTask task)
        {
            if (base.Failed)
                return MobileMessages.GetErrorMessageDocument(base.ErrorMessage);

            bool result = base.GetTaskResult(task);
            switch (task)
            {
                case MobileTask.SaveCustomer:
                    return this.customerId > 0 ? MobileMessages.GetIntMessageDocument(CustomerIdElement, this.customerId) : MobileMessages.GetSuccessDocument(result);
                default:
                    return base.ToXDocument(task);
            }
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            string xml =
                "<CustomerId>1</CustomerId>" + //Mandatory
                "<CustomerNr>123</CustomerNr>" + //Mandatory
                "<CustomerName>Kalle</CustomerName>" + //Mandatory
                "<OrganisationNr>74101-2565</OrganisationNr>" +
                "<VatNr>0</VatNr>" +
                "<Reference>Nisse</Reference>" +
                "<EmailAddressId>1</EmailAddressId>" +
                "<EmailAddress>email@hotmail.com</EmailAddress>" +
                "<HomePhoneId>1</HomePhoneId>" +
                "<HomePhone>81475</HomePhone>" +
                "<WorkPhoneId>1</WorkPhoneId>" +
                "<WorkPhone>854541</WorkPhone>" +
                "<CellPhoneId>1</CellPhoneId>" +
                "<CellPhone>184515</CellPhone>" +
                "<Fax>12111</Fax>" +
                "<InvoiceAddressId>1</InvoiceAddressId>" +
                "<InvoiceAddress>skogsvägen 3</InvoiceAddress>" +
                "<IAPostalCode>14570</IAPostalCode>" +
                "<IAPostalAddress>Norsborg</IAPostalAddress>" +
                "<DeliveryAddress1Id>0</DeliveryAddress1Id>" +
                "<DeliveryAddress1>0</DeliveryAddress1>" +
                "<DA1PostalCode>0</DA1PostalCode>" +
                "<DA1PostalAddress>0</DA1PostalAddress>" +
                "<VatTypeId>1</VatTypeId>" +
                "<VatTypeName>vattypename</VatTypeName>" +
                "<PaymentConditionId>1</PaymentConditionId>" +
                "<PaymentConditionName>conditionname</PaymentConditionName>" +
                "<SalesPriceListId>1</SalesPriceListId>" +
                "<SalesPriceListName>pricelistname</SalesPriceListName>" +
                "<StandardWholeSellerId>1</StandardWholeSellerId>" +
                "<StandardWholeSellerName>WholeSellerName</StandardWholeSellerName>" +
                "<DiscountArticles></DiscountArticles>" +
                "<DiscountServices></DiscountServices>" +
                "<CurrencyId>1</CurrencyId>" +
                "<CurrencyName>Currencyname</CurrencyName>" +
                "<Note>Notering</Note>";

            return XmlUtil.CreateXml(ROOTNAME, xml);
        }

        public static string GetDefaultSaveXml()
        {
            string xml =
                "<Success>1<Success>";

            return xml;
        }

        #endregion
    }

    internal class MobileNewCustomer : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "MobileNewCustomer";

        #endregion

        #region Variables

        public string customerNr;

        #endregion

        #region Ctor

        public MobileNewCustomer(MobileParam param) : base(param)
        {

        }

        /// <summary>Used for errors</summary>
        public MobileNewCustomer(MobileParam param, string errorMessage) : base(param, errorMessage)
        {

        }

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();

            elements.Add(new XElement("Nr", this.customerNr));

            return CreateDocument(ROOTNAME, elements);
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            string xml = "<Nr>123</Nr>";
            return XmlUtil.CreateXml(ROOTNAME, xml);
        }

        #endregion
    }

    #endregion

    #region Orders Grid/Edit

    internal class MobileOrderElements : MobileFieldSettings
    {
        #region Variables

        #region XML Elements

        protected const String OrderIdElement = "OrderId";
        protected const String OrderNrElement = "OrderNr";
        protected const String CustomerIdElement = "CustomerId";
        protected const String CustomerElement = "Customer";
        protected const String ReferenceElement = "Reference";
        protected const String YourReferenceElement = "YourReference";
        protected const String InvoiceAddressElement = "InvoiceAddress";
        protected const String InvoiceAddressIdElement = "InvoiceAddressId";
        protected const String DeliveryAddressElement = "DeliveryAddress";
        protected const String DeliveryAddressIdElement = "DeliveryAddressId";
        protected const String VatTypeElement = "VatType";
        protected const String VatTypeIdElement = "VatTypeId";
        protected const String VatTypeNameElement = "VatTypeName";
        protected const String SalesPriceListElement = "SalesPriceList";
        protected const String SalesPriceListIdElement = "SalesPriceListId";
        protected const String SalesPriceListNameElement = "SalesPriceListName";
        protected const String CurrencyElement = "Currency";
        protected const String CurrencyIdElement = "CurrencyId";
        protected const String CurrencyNameElement = "CurrencyName";
        protected const String HouseDeductionElement = "HHDeducton";
        protected const String LabelElement = "Label";
        protected const String HeadTextElement = "HeadText";
        protected const String InternalTextElement = "InternalText";
        protected const String WorkDescriptionElement = "WorkDescr";
        protected const String AmountElement = "Amount";
        protected const String RemaningAmountElement = "ReAmount";
        protected const String OwnersElement = "Owners";
        protected const String OrderDateElement = "ODate";
        protected const String OrderRowStatusColorElement = "ORowStatusColor";
        protected const String OrderRowStatusNamesElement = "ORowStatusNames";
        protected const String OStatusElement = "OStatus";
        protected const String WholeSellerElement = "WSeller";
        protected const String WholeSellerIdElement = "WSellerId";
        protected const String WholeSellerNameElement = "WSellerName";
        protected const String DeliveryDateElement = "DDate";
        protected const String OrderOrderRowSalesPrice = "OORSalesPrice";
        protected const String OrderOrderRowTotalPrice = "OORTotalPrice";

        #endregion

        #region Permissions

        protected bool amountDisabled = false;

        #endregion

        #endregion

        #region Ctor

        public MobileOrderElements(MobileParam param)
            : base(param)
        {

        }

        public MobileOrderElements(MobileParam param, List<FieldSetting> fieldSettings)
            : base(param, fieldSettings)
        {

        }

        /// <summary>Used for errors</summary>
        public MobileOrderElements(MobileParam param, string errorMessage)
            : base(param, errorMessage)
        {
        }

        #endregion
    }

    internal class MobileOrderEditFieldSettings : MobileOrderElements
    {
        #region Constants

        public const string ROOTNAME = "OrderEditFieldSettings";

        #endregion

        #region Ctor

        public MobileOrderEditFieldSettings(MobileParam param, List<FieldSetting> fieldSettings) : base(param, fieldSettings)
        {

        }

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();

            elements.Add(GetSettingElement(CustomerElement));
            elements.Add(GetSettingElement(VatTypeElement, TermGroup_MobileFields.OrderEdit_VatType));
            elements.Add(GetSettingElement(SalesPriceListElement, TermGroup_MobileFields.OrderEdit_SalesPriceList));
            elements.Add(GetSettingElement(HouseDeductionElement, TermGroup_MobileFields.OrderEdit_HHDeduction));
            elements.Add(GetSettingElement(LabelElement, TermGroup_MobileFields.OrderEdit_Label));
            //elements.Add(GetSettingElement(HeadTextElement, TermGroup_MobileFields.OrderEdit_HeadText));
            elements.Add(GetSettingElement(InternalTextElement, TermGroup_MobileFields.OrderEdit_InternalText));
            elements.Add(GetSettingElement(CurrencyElement, TermGroup_MobileFields.OrderEdit_Currency));
            elements.Add(GetSettingElement(DeliveryAddressElement, TermGroup_MobileFields.OrderEdit_DeliveryAddress));
            elements.Add(GetSettingElement(InvoiceAddressElement, TermGroup_MobileFields.OrderEdit_InvoiceAddress));
            elements.Add(GetSettingElement(ReferenceElement, TermGroup_MobileFields.OrderEdit_OurReference));
            elements.Add(GetSettingElement(AmountElement, TermGroup_MobileFields.OrderEdit_Amount));
            elements.Add(GetSettingElement(WholeSellerElement, TermGroup_MobileFields.OrderEdit_WholeSeller));
            elements.Add(GetSettingElement(YourReferenceElement, TermGroup_MobileFields.OrderEdit_YourReference));
            elements.Add(GetSettingElement(DeliveryDateElement, TermGroup_MobileFields.OrderEdit_DDate, false));

            return CreateDocument(ROOTNAME, elements);
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            string xml =
                "<Customer>1</Customer>" +
                "<VatType>1</VatType>" +
                "<SalesPriceList>1</SalesPriceList>" +
                "<HHDeduction>1</HHDeduction>" +
                "<Label>1</Label>" +
                "<HeadText>1</HeadText>" +
                "<InternalText>1</InternalText>" +
                "<Currency>1</Currency>" +
                "<DeliveryAddress>1</DeliveryAddress>" +
                "<InvoiceAddress>1</InvoiceAddress>" +
                "<Reference>1</Reference>" +
                "<Amount>1</Amount>" +
                "<Grossist>1</Grossist>";

            return XmlUtil.CreateXml(ROOTNAME, xml);
        }

        #endregion
    }

    internal class MobileOrderGridFieldSettings : MobileOrderElements
    {
        #region Constants

        public const string ROOTNAME = "OrderGridFieldSettings";

        #endregion

        #region Variables

        readonly private bool hideStatusOrderReady;
        readonly private bool showMyOrders;

        #endregion

        #region Ctor

        public MobileOrderGridFieldSettings(MobileParam param, List<FieldSetting> fieldSettings, bool hideStatusOrderReady, bool showMyOrders)
            : base(param, fieldSettings)
        {
            this.hideStatusOrderReady = hideStatusOrderReady;
            this.showMyOrders = showMyOrders;
        }

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();

            elements.Add(GetSettingElement("HSOR", hideStatusOrderReady));
            elements.Add(GetSettingElement("SMyOrders", showMyOrders));
            elements.Add(GetSettingElement(CustomerElement));
            elements.Add(GetSettingElement(VatTypeElement, TermGroup_MobileFields.OrderGrid_VatType, false));   //not yet electable as a fieldsetting in XE
            elements.Add(GetSettingElement(SalesPriceListElement, TermGroup_MobileFields.OrderGrid_SalesPriceList, false)); //not yet electable as a fieldsetting in XE
            elements.Add(GetSettingElement(HouseDeductionElement, TermGroup_MobileFields.OrderGrid_HHDeduction));
            elements.Add(GetSettingElement(LabelElement, TermGroup_MobileFields.OrderGrid_Label, false));//not yet electable as a fieldsetting in XE
            //elements.Add(GetSettingElement(HeadTextElement, TermGroup_MobileFields.OrderGrid_HeadText));
            elements.Add(GetSettingElement(InternalTextElement, TermGroup_MobileFields.OrderGrid_InternalText));
            elements.Add(GetSettingElement(WorkDescriptionElement, TermGroup_MobileFields.OrderGrid_WorkDescr));
            elements.Add(GetSettingElement(CurrencyElement, TermGroup_MobileFields.OrderGrid_Currency));
            elements.Add(GetSettingElement(DeliveryAddressElement, TermGroup_MobileFields.OrderGrid_DeliveryAddress));
            elements.Add(GetSettingElement(InvoiceAddressElement, TermGroup_MobileFields.OrderGrid_InvoiceAddress, false));//not yet electable as a fieldsetting in XE
            elements.Add(GetSettingElement(ReferenceElement, TermGroup_MobileFields.OrderGrid_Reference, false));//not yet electable as a fieldsetting in XE
            elements.Add(GetSettingElement(AmountElement, TermGroup_MobileFields.OrderGrid_Amount));
            elements.Add(GetSettingElement(RemaningAmountElement, TermGroup_MobileFields.OrderGrid_ReAmount));
            elements.Add(GetSettingElement(OwnersElement, TermGroup_MobileFields.OrderGrid_Owners));
            elements.Add(GetSettingElement(OrderDateElement, TermGroup_MobileFields.OrderGrid_ODate));
            elements.Add(GetSettingElement(OrderRowStatusColorElement, TermGroup_MobileFields.OrderGrid_ORowStatus));
            elements.Add(GetSettingElement(DeliveryDateElement, TermGroup_MobileFields.OrderGrid_DDate, false));

            return CreateDocument(ROOTNAME, elements);
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            string xml =
                "<Customer>1</Customer>" +
                "<VatType>1</VatType>" +
                "<SalesPriceList>1</SalesPriceList>" +
                "<HHDeduction>1</HHDeduction>" +
                "<Label>1</Label>" +
                "<HeadText>1</HeadText>" +
                "<InternalText>1</InternalText>" +
                "<Currency>1</Currency>" +
                "<DeliveryAddress>1</DeliveryAddress>" +
                "<InvoiceAddress>1</InvoiceAddress>" +
                "<Reference>1</Reference>" +
                "<Amount>1</Amount>" +
                "<ReAmount>1</ReAmount>" +
                "<Owners>1</Owners>" +
                "<ODate>1</ODate>";

            return XmlUtil.CreateXml(ROOTNAME, xml);
        }

        #endregion
    }

    internal class MobileOrdersGrid : MobileOrderElements
    {
        #region Constants

        public const string ROOTNAME = "OrdersGrid";

        #endregion

        #region Variables

        readonly private List<MobileOrderGrid> mobileOrdersGrid = new List<MobileOrderGrid>();

        #endregion

        #region Ctor

        public MobileOrdersGrid(MobileParam param, List<CustomerInvoiceGridDTO> orders, string houseHoldLabel, string noOrderRowStatusLabel, List<FieldSetting> fieldSettings) : base(param, fieldSettings)
        {
            AddMobileOrdersGrid(orders, houseHoldLabel, noOrderRowStatusLabel);
        }

        /// <summary>Used for errors</summary>
        public MobileOrdersGrid(MobileParam param, string errorMessage) : base(param, errorMessage)
        {
        }

        #endregion

        #region Public methods

        public void AddMobileOrdersGrid(List<CustomerInvoiceGridDTO> orders, string houseHoldLabel, string noOrderRowStatusLabel)
        {
            foreach (var order in orders)
            {
                AddMobileOrderGrid(new MobileOrderGrid(this.Param, order, houseHoldLabel, noOrderRowStatusLabel, fieldSettings));
            }
        }

        public void AddMobileOrderGrid(MobileOrderGrid mobileOrderGrid)
        {
            if (mobileOrderGrid == null)
                return;

            mobileOrdersGrid.Add(mobileOrderGrid);
        }

        public void SetAmountDisabled()
        {
            foreach (var mobileOrder in mobileOrdersGrid)
            {
                mobileOrder.SetAmountDisabled();
            }
        }

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            return MergeDocuments(ROOTNAME, mobileOrdersGrid.Select(i => i.ToXDocument()).ToList());
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            return XmlUtil.CreateXml(ROOTNAME, MobileOrderGrid.GetDefaultXml());
        }

        #endregion
    }

    internal class MobileOrderGrid : MobileOrderElements
    {
        #region Constants

        public const string ROOTNAME = "OrderGrid";

        #endregion

        #region Variables

        readonly private int orderId;
        readonly private string orderNr;
        readonly private string projectNr;
        readonly private DateTime? orderDate;
        readonly private int customerId;
        readonly private string customerNrAndName;
        readonly private string deliveryAddress;
        readonly private string internaltext;
        readonly private string workDescription;
        readonly private string currencyCode;
        readonly private decimal totalAmountCurrency;
        readonly private decimal remaningAmount;
        readonly private string owners;
        readonly private bool hasHouseHoldDeduction = false;
        readonly private string houseHoldLabel = "";
        readonly private string invoiceHeadText = "";
        readonly private string attestStateNames = "";
        readonly private string attestStateColor = "";
        readonly private string statusName = "";
        readonly private DateTime? deliveryDate;
        readonly private string referenceYour;
        readonly private int userReadyState;
        readonly private string projectName;
        readonly private int statusicon;
        readonly private int myOriginUserStatus;
        #endregion

        #region Ctor

        public MobileOrderGrid(MobileParam param, CustomerInvoiceGridDTO order, string houseHoldLabel, string noOrderRowStatusLabel, List<FieldSetting> fieldSettings) : base(param, fieldSettings)
        {
            if (order != null)
            {
                this.orderId = order.CustomerInvoiceId;
                this.orderNr = order.InvoiceNr;
                this.projectNr = order.ProjectNr;
                this.customerId = order.ActorCustomerId;
                this.customerNrAndName = order.ActorCustomerNrName;
                this.deliveryAddress = order.DeliveryAddress;
                this.internaltext = (string.IsNullOrEmpty(order.InternalText) ? "" : order.InternalText);
                this.workDescription = (string.IsNullOrEmpty(order.WorkDescription) ? "" : order.WorkDescription);
                this.totalAmountCurrency = order.TotalAmountExVat;
                this.currencyCode = order.CurrencyCode;
                this.remaningAmount = order.RemainingAmountExVat;
                this.owners = order.Users;
                this.orderDate = order.InvoiceDate;
                this.referenceYour = order.ReferenceYour;
                this.houseHoldLabel = houseHoldLabel;
                this.hasHouseHoldDeduction = order.HasHouseholdTaxDeduction;
                this.invoiceHeadText = order.InvoiceHeadText;
                this.statusName = order.StatusName;
                this.deliveryDate = order.DeliveryDate;
                this.userReadyState = order.MyReadyState;
                this.projectName = order.ProjectName;
                this.statusicon = order.StatusIcon;
                this.myOriginUserStatus = order.MyOriginUserStatus;

                if (!string.IsNullOrEmpty(order.MainUserName))
                    this.orderNr += " (" + order.MainUserName + ")";

                if (order.AttestStates == null || order.AttestStates.Count == 0)
                {
                    this.attestStateColor = "#FFFFFF"; //white
                    this.attestStateNames = noOrderRowStatusLabel;
                }
                else if (order.AttestStates.Count == 1)
                {
                    this.attestStateColor = order.AttestStates[0].Color;
                    this.attestStateNames = order.AttestStates[0].Name;
                }
                else if (order.AttestStates.Count > 1)
                {
                    this.attestStateColor = COLOR_ORANGE;

                    foreach (var attestState in order.AttestStates)
                    {
                        if (!string.IsNullOrEmpty(this.attestStateNames))
                            this.attestStateNames += ", ";
                        this.attestStateNames += attestState.Name;
                    }
                }
            }
        }

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();

            elements.Add(new XElement(OrderIdElement, this.orderId));
            elements.Add(new XElement(OrderNrElement, GetTextOrCDATA(this.orderNr)));
            elements.Add(new XElement(CustomerIdElement, this.customerId));
            elements.Add(new XElement(CustomerElement, GetTextOrCDATA(this.customerNrAndName)));
            //if (DoShowField(TermGroup_MobileFields.OrderGrid_VatType))                    //Not yet electabled as a fieldsetting in XE
            //{
            //    elements.Add(new XElement(VatTypeNameElement, NOT_IMPLEMENTED));
            //}
            //if (DoShowField(TermGroup_MobileFields.OrderGrid_SalesPriceList))             //Not yet electabled as a fieldsetting in XE
            //{
            //    elements.Add(new XElement(SalesPriceListNameElement, NOT_IMPLEMENTED));
            //}
            if (DoShowField(TermGroup_MobileFields.OrderGrid_HHDeduction) && this.hasHouseHoldDeduction)
            {
                elements.Add(new XElement(HouseDeductionElement, GetTextOrCDATA(this.houseHoldLabel)));
            }
            //if (DoShowField(TermGroup_MobileFields.OrderGrid_Label))                      //Not yet electabled as a fieldsetting in XE
            //{
            //    elements.Add(new XElement(LabelElement, NOT_IMPLEMENTED));
            //}
            //if (DoShowField(TermGroup_MobileFields.OrderGrid_HeadText))
            //{
            //    elements.Add(new XElement(HeadTextElement, this.invoiceHeadText));
            //}
            if (DoShowField(TermGroup_MobileFields.OrderGrid_InternalText))
            {
                elements.Add(new XElement(InternalTextElement, GetTextOrCDATA(this.internaltext)));
            }
            if (DoShowField(TermGroup_MobileFields.OrderGrid_WorkDescr))
            {
                elements.Add(new XElement(WorkDescriptionElement, GetTextOrCDATA(this.workDescription)));
            }
            if (DoShowField(TermGroup_MobileFields.OrderGrid_Currency))
            {
                elements.Add(new XElement(CurrencyElement, this.currencyCode));
            }
            if (DoShowField(TermGroup_MobileFields.OrderGrid_DeliveryAddress))
            {
                elements.Add(new XElement(HeadTextElement, GetTextOrCDATA(this.invoiceHeadText)));
                elements.Add(new XElement(DeliveryAddressElement, GetTextOrCDATA(this.deliveryAddress)));
            }
            //if (DoShowField(TermGroup_MobileFields.OrderGrid_InvoiceAddress))             //Not yet electabled as a fieldsetting in XE
            //{
            //    elements.Add(new XElement(InvoiceAddressElement, NOT_IMPLEMENTED));
            //}

            if (DoShowField(TermGroup_MobileFields.OrderGrid_Reference))
            {
                elements.Add(new XElement(ReferenceElement, GetTextOrCDATA(this.referenceYour)));
            }

            if (DoShowField(TermGroup_MobileFields.OrderGrid_Amount) && !this.amountDisabled)
            {
                elements.Add(new XElement(AmountElement, this.totalAmountCurrency));
            }
            if (DoShowField(TermGroup_MobileFields.OrderGrid_ReAmount) && !this.amountDisabled)
            {
                elements.Add(new XElement(RemaningAmountElement, this.remaningAmount));
            }
            if (DoShowField(TermGroup_MobileFields.OrderGrid_Owners))
            {
                elements.Add(new XElement(OwnersElement, GetTextOrCDATA(this.owners)));
            }
            if (DoShowField(TermGroup_MobileFields.OrderGrid_ODate))
            {
                elements.Add(new XElement(OrderDateElement, this.orderDate.HasValue ? this.orderDate.Value.ToShortDateString() : string.Empty));
            }
            if (DoShowField(TermGroup_MobileFields.OrderGrid_ORowStatus))
            {
                elements.Add(new XElement(OrderRowStatusColorElement, this.attestStateColor));
                elements.Add(new XElement(OrderRowStatusNamesElement, this.attestStateNames));
            }
            elements.Add(new XElement(OStatusElement, this.statusName));
            if (DoShowField(TermGroup_MobileFields.OrderGrid_DDate, false))
            {
                elements.Add(new XElement(DeliveryDateElement, this.deliveryDate.HasValue ? this.deliveryDate.Value.ToShortDateString() : string.Empty));
            }
            elements.Add(new XElement("UserReadyState", this.userReadyState.ToString()));
            elements.Add(new XElement("ProjectNr", GetTextOrCDATA(this.projectNr)));
            elements.Add(new XElement("ProjectName", GetTextOrCDATA(this.projectName)));
            elements.Add(new XElement("SIcon", this.statusicon.ToString()));
            elements.Add(new XElement("MyOriginUserStatus", this.myOriginUserStatus.ToString()));

            return CreateDocument(ROOTNAME, elements);
        }

        #endregion

        #region Public methods

        public void SetAmountDisabled(bool disabled = true)
        {
            this.amountDisabled = disabled;
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            string xml =
                    "<OrderId></OrderId>" + //Mandatory
                    "<OrderNr></OrderNr>" + //Mandatory
                    "<CustomerId></CustomerId>" + //Mandatory
                    "<Customer></Customer>" + //Mandatory
                    "<VatType></VatType>" +
                    "<SalesPriceList></SalesPriceList>" +
                    "<HHDeduction></HHDeduction>" +
                    "<Label></Label>" +
                    "<HeadText></HeadText>" +
                    "<InternalText></InternalText>" +
                    "<Currency></Currency>" +
                    "<DeliveryAddress></DeliveryAddress>" +
                    "<InvoiceAddress></InvoiceAddress>" +
                    "<Reference></Reference>" +
                    "<Amount></Amount>" +
                    "<ReAmount></ReAmount>" +
                    "<Owners></Owners>" +
                    "<ODate></ODate>" +
                    "<ORowStatusColor></ORowStatusColor>" +
                    "<ORowStatusNames></ORowStatusNames>" +
                    "<OStatus></OStatus>";

            return XmlUtil.CreateXml(ROOTNAME, xml);
        }

        #endregion
    }

    internal class MobileOrderEdit : MobileOrderElements
    {
        #region Constants

        public const string ROOTNAME = "OrderEdit";

        #endregion

        #region Variables


        #region Order info

        private readonly int orderId;
        private readonly string orderNr;
        private readonly int customerId;
        private readonly string customerNr;
        private readonly string customerName;
        private readonly int deliveryAddressId;
        private readonly string deliveryAddress;
        private readonly int currencyId;
        private readonly string currencyName;
        private readonly int billingAddressId;
        private readonly string billingAddress;
        private readonly int vatTypeId;
        private readonly string vatTypename;
        private readonly int priceListTypeId;
        private readonly string priceListTypeName;
        private readonly string internaltext;
        private readonly string invoiceHeadText;
        private readonly string label;
        private readonly string referenceOur;
        private readonly string referenceYour;
        private readonly decimal totalAmountCurrency;
        private readonly bool hasHouseHoldDeduction = false;
        private readonly string houseHoldLabel = "";
        private readonly bool hasWorkDesc = false;
        private readonly int wholeSellerId;
        private readonly string wholeSellerName;
        private readonly int orderTypeId;
        private readonly string orderTypeName;
        private readonly int projectId;
        private readonly int? projectCustomerId;
        private readonly bool projectHidden;
        private readonly string dynamicData;
        private readonly string internalDescription;
        private readonly string externalDescription;
        private readonly DateTime? deliveryDate;
        readonly private bool priceListTypeInclusiveVat;
        readonly private string projectNr;
        readonly private bool showChangeCustomer;
        readonly private bool isClosed;

        #endregion

        #region Overview

        private readonly int workedTime = 0;
        private readonly int otherTime = 0;
        private readonly int invoicedTime = 0;
        private readonly int productsCount = 0;
        private readonly decimal productsSumCurrency = 0;

        #endregion

        #region Permissions

        readonly bool showTP = false;
        readonly bool showProducts = false;
        readonly bool showChecklists = false;
        readonly bool showImages = false;
        readonly bool answerChecklists = false;
        readonly bool addChecklists = false;
        readonly bool addProject = false;
        readonly bool changeProject = false;
        readonly bool showPartDelivery = false; //Only professional for now
        readonly bool showAccounts = false; //Only professional for now
        readonly bool copyRows = false;
        readonly bool moveRows = false;
        readonly bool showSalesPrice = false;
        readonly bool showOrderPlanning = false;
        readonly bool showInvoicedTime = false;
        readonly bool showStockPlace = false;
        readonly bool allowMarkReady = true;
        readonly int userReadyState;
        readonly bool showExpense = false;
        readonly bool showHouseHoldDeduction = false;
        readonly bool deleteOrder = false;

        #endregion

        #region Settings

        private readonly bool isLiber = false;
        private readonly bool showExternalProductInfoLink = false;
        private readonly int myOriginUserStatus = 0;
        private readonly bool showCustomerNote = false;

        #endregion

        #endregion

        #region Ctor

        public MobileOrderEdit(MobileParam param, int orderId) : base(param)
        {
            this.orderId = orderId;
        }

        public MobileOrderEdit(MobileParam param, CustomerInvoice order, int workedTime, int otherTime, int invoicedTime, int productsCount, string billingAddress, string deliveryAddress, string currencyName, string vatTypename, String priceListTypeName, String wholeSellerName, string orderTypeName, String houseHoldLabel, bool showTP, bool showProducts, bool showChecklists, bool showImages, bool answerChecklists, bool addChecklists, bool addProject, bool changeProject, bool showPartDelivery, bool showAccounts, List<FieldSetting> fieldSettings, string dynamicData, bool copyRows, bool moveRows, bool isLiber, bool showSalesPrice, bool showOrderPlanning, bool showInvoicedTime, bool showStockPlace, bool allowMarkReady, int userReadyState, bool showExpense, bool showHouseHoldDeduction, bool showChangeCustomer, bool deleteOrder, bool isClosed, bool showExternalProductInfoLink, int myOriginUserStatus, bool showCustomerNote) : base(param, fieldSettings)
        {
            if (order != null && order.Origin != null && order.Actor != null)
            {
                #region Order info

                this.orderId = order.InvoiceId;
                this.orderNr = order.InvoiceNr;
                this.customerId = order.ActorId ?? 0;
                this.customerNr = order.ActorNr;
                this.customerName = order.ActorName;
                this.internaltext = order.Origin.Description;
                this.invoiceHeadText = order.InvoiceHeadText;
                this.label = order.InvoiceLabel;
                this.totalAmountCurrency = order.TotalAmountCurrency - order.VATAmountCurrency;
                this.billingAddressId = order.BillingAddressId;
                this.deliveryAddressId = order.DeliveryAddressId;
                this.referenceOur = order.ReferenceOur;
                this.referenceYour = order.ReferenceYour;
                this.currencyId = order.CurrencyId;
                this.vatTypeId = order.VatType;
                this.priceListTypeId = order.PriceListTypeId ?? 0;
                this.priceListTypeInclusiveVat = order.PriceListTypeInclusiveVat;
                this.wholeSellerId = order.SysWholeSellerId ?? 0;
                this.orderTypeId = order.OrderType;
                this.hasHouseHoldDeduction = order.HasHouseholdTaxDeduction;
                this.billingAddress = billingAddress;
                this.deliveryAddress = deliveryAddress;
                this.currencyName = currencyName;
                this.vatTypename = vatTypename;
                this.priceListTypeName = priceListTypeName;
                this.wholeSellerName = wholeSellerName;
                this.orderTypeName = orderTypeName;
                this.houseHoldLabel = houseHoldLabel;
                this.showChangeCustomer = showChangeCustomer;

                if (!string.IsNullOrEmpty(order.WorkingDescription))
                    this.hasWorkDesc = true;

                this.projectId = order.ProjectId ?? 0;
                this.projectNr = order.Project?.Number;
                this.projectCustomerId = order.Project?.CustomerId;
                this.projectHidden = order.Project != null && order.Project.Status == (int)TermGroup_ProjectStatus.Hidden;

                this.dynamicData = dynamicData;

                this.internalDescription = !string.IsNullOrEmpty(order.InternalDescription) ? order.InternalDescription : string.Empty;
                this.externalDescription = !string.IsNullOrEmpty(order.ExternalDescription) ? order.ExternalDescription : string.Empty;
                this.deliveryDate = order.DeliveryDate;
                this.isClosed = isClosed;

                #endregion

                #region Overview

                this.workedTime = workedTime;
                this.otherTime = otherTime;
                this.invoicedTime = invoicedTime;
                this.productsCount = productsCount;
                if (showProducts)
                {
                    this.productsSumCurrency = order.TotalAmountCurrency - order.VATAmountCurrency;
                }

                #endregion

                #region Permissions

                this.showTP = showTP;
                this.showProducts = showProducts;
                this.showChecklists = showChecklists;
                this.showImages = showImages;
                this.answerChecklists = answerChecklists;
                this.addChecklists = addChecklists;
                this.addProject = addProject;
                this.changeProject = changeProject;
                this.showPartDelivery = showPartDelivery;
                this.showAccounts = showAccounts;
                this.copyRows = copyRows;
                this.moveRows = moveRows;
                this.showSalesPrice = showSalesPrice;
                this.showOrderPlanning = showOrderPlanning;
                this.showInvoicedTime = showInvoicedTime;
                this.showStockPlace = showStockPlace;
                this.allowMarkReady = allowMarkReady;
                this.userReadyState = userReadyState;
                this.showExpense = showExpense;
                this.showHouseHoldDeduction = showHouseHoldDeduction;
                this.deleteOrder = deleteOrder;

                #endregion

                #region Settings

                this.isLiber = isLiber;
                this.showExternalProductInfoLink = showExternalProductInfoLink;
                this.myOriginUserStatus = myOriginUserStatus;
                this.showCustomerNote = showCustomerNote;

                #endregion 

            }
        }

        /// <summary>Used for errors</summary>
        public MobileOrderEdit(MobileParam param, string errorMessage) : base(param, errorMessage)
        {

        }

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();

            elements.Add(new XElement(OrderIdElement, this.orderId));
            elements.Add(new XElement(OrderNrElement, GetTextOrCDATA(this.orderNr)));
            elements.Add(new XElement(CustomerIdElement, this.customerId));
            elements.Add(new XElement(CustomerElement, GetTextOrCDATA(this.customerNr + ", " + this.customerName)));
            if (DoShowField(TermGroup_MobileFields.OrderEdit_VatType))
            {
                elements.Add(new XElement(VatTypeIdElement, this.vatTypeId));
                elements.Add(new XElement(VatTypeNameElement, this.vatTypename));
            }
            if (DoShowField(TermGroup_MobileFields.OrderEdit_SalesPriceList))
            {
                elements.Add(new XElement(SalesPriceListIdElement, this.priceListTypeId));
                elements.Add(new XElement(SalesPriceListNameElement, this.priceListTypeName));
            }
            if (DoShowField(TermGroup_MobileFields.OrderEdit_HHDeduction) && this.hasHouseHoldDeduction)
            {
                elements.Add(new XElement(HouseDeductionElement, GetTextOrCDATA(this.houseHoldLabel)));
            }
            if (DoShowField(TermGroup_MobileFields.OrderEdit_Label))
            {
                elements.Add(new XElement(LabelElement, GetTextOrCDATA(this.label)));
            }

            if (DoShowField(TermGroup_MobileFields.OrderEdit_InternalText))
            {
                elements.Add(new XElement(InternalTextElement, GetTextOrCDATA(this.internaltext)));
            }
            if (DoShowField(TermGroup_MobileFields.OrderEdit_Currency))
            {
                elements.Add(new XElement(CurrencyIdElement, this.currencyId));
                elements.Add(new XElement(CurrencyNameElement, this.currencyName));
            }
            if (DoShowField(TermGroup_MobileFields.OrderEdit_DeliveryAddress))
            {
                elements.Add(new XElement(HeadTextElement, GetTextOrCDATA(this.invoiceHeadText)));
                elements.Add(new XElement(DeliveryAddressIdElement, this.deliveryAddressId));
                elements.Add(new XElement(DeliveryAddressElement, GetTextOrCDATA(this.deliveryAddress)));
            }
            if (DoShowField(TermGroup_MobileFields.OrderEdit_InvoiceAddress))
            {
                elements.Add(new XElement(InvoiceAddressIdElement, this.billingAddressId));
                elements.Add(new XElement(InvoiceAddressElement, GetTextOrCDATA(this.billingAddress)));
            }
            if (DoShowField(TermGroup_MobileFields.OrderEdit_OurReference))
            {
                elements.Add(new XElement(ReferenceElement, GetTextOrCDATA(this.referenceOur)));
            }
            if (DoShowField(TermGroup_MobileFields.OrderEdit_Amount) && !this.amountDisabled)
            {
                elements.Add(new XElement(AmountElement, this.totalAmountCurrency));
            }
            elements.Add(new XElement("STime", StringUtility.GetString(this.showTP)));
            if (showTP)
            {
                elements.Add(new XElement("WorkedTime", this.workedTime));
                elements.Add(new XElement("OtherTime", this.otherTime));
                elements.Add(new XElement("InvoicedTime", this.invoicedTime));
            }
            elements.Add(new XElement("SProducts", StringUtility.GetString(this.showProducts)));
            if (showProducts)
            {
                elements.Add(new XElement("ProductsCount", this.productsCount));
                if (!this.amountDisabled)
                    elements.Add(new XElement("ProductsSum", this.productsSumCurrency));
            }
            elements.Add(new XElement("SChecklist", StringUtility.GetString(this.showChecklists)));
            if (showChecklists)
            {
                elements.Add(new XElement("AChecklist", StringUtility.GetString(this.answerChecklists)));
                elements.Add(new XElement("AddChecklist", StringUtility.GetString(this.addChecklists)));

            }
            elements.Add(new XElement("SImages", StringUtility.GetString(this.showImages)));
            elements.Add(new XElement("HasWorkDesc", StringUtility.GetString(this.hasWorkDesc)));
            if (DoShowField(TermGroup_MobileFields.OrderEdit_WholeSeller))
            {
                elements.Add(new XElement(WholeSellerIdElement, this.wholeSellerId));
                elements.Add(new XElement(WholeSellerNameElement, GetTextOrCDATA(this.wholeSellerName)));
            }
            if (DoShowField(TermGroup_MobileFields.OrderEdit_YourReference))
            {
                elements.Add(new XElement(YourReferenceElement, GetTextOrCDATA(this.referenceYour)));
            }

            elements.Add(new XElement("ProjectId", this.projectId));
            elements.Add(new XElement("ProjectHidden", StringUtility.GetString(this.projectHidden)));
            elements.Add(new XElement("ProjectCustomerId", this.projectCustomerId));
            elements.Add(new XElement("AddProject", StringUtility.GetString(this.addProject)));
            elements.Add(new XElement("ChangeProject", StringUtility.GetString(this.changeProject)));
            elements.Add(new XElement("DynHeader", GetTextOrCDATA(this.dynamicData)));
            elements.Add(new XElement("SPartDelivery", StringUtility.GetString(this.showPartDelivery)));
            elements.Add(new XElement("SAccounts", StringUtility.GetString(this.showAccounts)));
            elements.Add(new XElement("CopyRows", StringUtility.GetString(this.copyRows)));
            elements.Add(new XElement("MoveRows", StringUtility.GetString(this.moveRows)));
            elements.Add(new XElement("DeleteOrder", StringUtility.GetString(this.deleteOrder)));
            elements.Add(new XElement("IsLiber", StringUtility.GetString(this.isLiber)));
            if (this.isLiber)
            {
                if (!string.IsNullOrEmpty(this.internalDescription))
                    elements.Add(new XElement("TaskDesc", GetTextOrCDATA(this.internalDescription)));

                if (!string.IsNullOrEmpty(this.externalDescription))
                    elements.Add(new XElement("ProAdvice", GetTextOrCDATA(this.externalDescription)));
            }
            elements.Add(new XElement("ShowSalesPrice", StringUtility.GetString(this.showSalesPrice)));
            if (DoShowField(TermGroup_MobileFields.OrderEdit_DDate, false))
            {
                elements.Add(new XElement(DeliveryDateElement, this.deliveryDate.HasValue ? this.deliveryDate.Value.ToShortDateString() : string.Empty));
            }
            elements.Add(new XElement("SPlanning", StringUtility.GetString(this.showOrderPlanning)));
            elements.Add(new XElement("SInvoicedTime", StringUtility.GetString(this.showInvoicedTime)));

            elements.Add(new XElement("ShowStockPlace", StringUtility.GetString(this.showStockPlace)));

            elements.Add(new XElement("SMarkReady", StringUtility.GetString(this.allowMarkReady)));
            elements.Add(new XElement("UserReadyState", this.userReadyState.ToString()));
            elements.Add(new XElement("SExpense", StringUtility.GetString(this.showExpense)));
            elements.Add(new XElement("SHouseHoldDeduction", StringUtility.GetString(this.showHouseHoldDeduction)));
            elements.Add(new XElement("SChangeCustomer", StringUtility.GetString(this.showChangeCustomer)));
            elements.Add(new XElement("SExternalUrl", StringUtility.GetString(this.showExternalProductInfoLink)));
            elements.Add(new XElement("SCustomerNote", StringUtility.GetString(this.showCustomerNote)));

            if (this.showExpense)
            {
                elements.Add(new XElement("PriceListInclusiveVat", StringUtility.GetString(this.priceListTypeInclusiveVat)));
            }
            elements.Add(new XElement("ProjectNr", GetTextOrCDATA(this.projectNr)));
            elements.Add(new XElement("OrderTypeId", this.orderTypeId));
            elements.Add(new XElement("OrderTypeName", this.orderTypeName));
            elements.Add(new XElement("Closed", StringUtility.GetString(this.isClosed)));
            elements.Add(GetSettingElement(OrderOrderRowSalesPrice, TermGroup_MobileFields.OrderOrderRow_SalesPrice));
            elements.Add(GetSettingElement(OrderOrderRowTotalPrice, TermGroup_MobileFields.OrderOrderRow_TotalPrice));
            elements.Add(new XElement("MyOriginUserStatus", this.myOriginUserStatus.ToString()));

            return CreateDocument(ROOTNAME, elements);
        }

        public override XDocument ToXDocument(MobileTask task)
        {
            if (base.Failed)
                return MobileMessages.GetErrorMessageDocument(base.ErrorMessage);

            bool result = base.GetTaskResult(task);
            switch (task)
            {
                case MobileTask.SaveOrder:
                    return MobileMessages.GetIntMessageDocument(MobileMessages.XML_ELEMENT_ORDERID, this.orderId);
                case MobileTask.SetOrderReady:
                case MobileTask.SetOrderRowsAsReady:
                    return MobileMessages.GetSuccessDocument(result);
                default:
                    return base.ToXDocument(task);
            }
        }

        #endregion

        #region Public methods

        public void SetAmountDisabled(bool disabled = true)
        {
            this.amountDisabled = disabled;
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            string xml =
                    "<OrderId></OrderId>" + //Mandatory
                    "<OrderNr></OrderNr>" + //Mandatory
                    "<CustomerId></CustomerId>" + //Mandatory
                    "<Customer></Customer>" + //Mandatory
                    "<PriceListInclusiveVat></PriceListInclusiveVat>" +
                    "<VatTypeId></VatTypeId>" +
                    "<VatTypeName></VatTypeName>" +
                    "<SalesPriceListId></SalesPriceListId>" +
                    "<SalesPriceListName></SalesPriceListName>" +
                    "<HHDeduction></HHDeduction>" +
                    "<Label></Label>" +
                    "<HeadText></HeadText>" +
                    "<InternalText></InternalText>" +
                    "<CurrencyId></CurrencyId>" +
                    "<CurrencyName></CurrencyName>" +
                    "<DeliveryAddressId></DeliveryAddressId>" +
                    "<DeliveryAddress></DeliveryAddress>" +
                    "<InvoiceAddressId></InvoiceAddressId>" +
                    "<InvoiceAddress></InvoiceAddress>" +
                    "<Reference></Reference>" +
                    "<Amount></Amount>" +
                    "<STime></STime>" +
                    "<WorkedTime></WorkedTime>" +
                    "<InvoicedTime></InvoicedTime>" +
                    "<SProducts></SProducts>" +
                    "<ProductsCount></ProductsCount>" +
                    "<ProductsSum></ProductsSum>" +
                    "<SChecklist></SChecklist>" +
                    "<AChecklist></AChecklist>" +
                    "<SImages></SImages>" +
                    "<SInvoicedTime></SInvoicedTime>" +
                    "<UserReadyState></UserReadyState>" +
                    "<SMarkReady></SMarkReady>";

            return XmlUtil.CreateXml(ROOTNAME, xml);
        }

        public static string GetDefaultSaveXml()
        {
            string xml =
                "<Success>1<Success>";

            return xml;
        }
        public static string GetDefaultSetReadyXml()
        {
            string xml =
                "<Success>1</Success>";

            return xml;
        }

        #endregion
    }

    internal class MobileOrderTemplateInfo : MobileOrderElements
    {
        #region Constants

        public const string ROOTNAME = "OrderTemplateInfo";

        #endregion

        #region Variables


        private readonly int orderTypeId;
        private readonly string orderTypeName;


        #endregion

        #region Ctor

        public MobileOrderTemplateInfo(MobileParam param, CustomerInvoice order, string orderTypeName = "") : base(param)
        {
            if (order != null)
            {
                this.orderTypeName = orderTypeName;
                this.orderTypeId = order.OrderType;
            }
        }

        /// <summary>Used for errors</summary>
        public MobileOrderTemplateInfo(MobileParam param, string errorMessage) : base(param, errorMessage)
        {

        }

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();
            elements.Add(new XElement("OrderTypeId", this.orderTypeId));
            elements.Add(new XElement("OrderTypeName", this.orderTypeName));
            
            return CreateDocument(ROOTNAME, elements);
        }

        #endregion

        #region Public methods

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            string xml =
                    "<OrderTypeId></OrderTypeId>" +
                    "<OrderTypeName></OrderTypeName>";

            return XmlUtil.CreateXml(ROOTNAME, xml);
        }

        #endregion
    }

    #endregion

    #region Order Type

    internal class MobileOrderTypes : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "MobileOrderTypes";

        #endregion

        #region Variables

        private List<MobileOrderType> mobileOrderTypes;

        #endregion

        #region Ctor

        public MobileOrderTypes(MobileParam param, Dictionary<int, string> orderTypes) : base(param)
        {
            Init();
            AddMobileOrderTypes(orderTypes);
        }

        /// <summary>Used for errors</summary>
        public MobileOrderTypes(MobileParam param, string errorMessage) : base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            mobileOrderTypes = new List<MobileOrderType>();
        }

        #endregion

        #region Public methods

        public void AddMobileOrderTypes(Dictionary<int, string> orderTypes)
        {
            if (orderTypes == null)
                return;

            foreach (var orderType in orderTypes)
            {
                AddMobileOrderType(new MobileOrderType(this.Param, orderType));
            }
        }

        public void AddMobileOrderType(MobileOrderType mobileOrderType)
        {
            if (mobileOrderType == null)
                return;

            mobileOrderTypes.Add(mobileOrderType);
        }

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            return MergeDocuments(ROOTNAME, mobileOrderTypes.Select(i => i.ToXDocument()).ToList());
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            return XmlUtil.CreateXml(ROOTNAME, MobileOrderType.GetDefaultXml());
        }

        #endregion
    }

    internal class MobileOrderType : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "OrderType";

        #endregion

        #region Variables

        private readonly int id;
        private readonly string value;

        #endregion

        #region Ctor

        public MobileOrderType(MobileParam param, KeyValuePair<int, string> orderType) : base(param)
        {
            id = orderType.Key;
            value = orderType.Value;
        }

        /// <summary>
        /// Used for errors
        /// </summary>
        public MobileOrderType(MobileParam param, string errorMessage) : base(param, errorMessage)
        {

        }

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();

            elements.Add(new XElement("OrderTypeId", id));
            elements.Add(new XElement("OrderTypeName", value));

            return CreateDocument(ROOTNAME, elements);
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            string xml =
                "<OrderTypeId>1</OrderTypeId>" +
                "<OrderTypeName>Projekt</OrderTypeName>";
            return XmlUtil.CreateXml(ROOTNAME, xml);
        }

        #endregion
    }

    #endregion

    #region VatType

    internal class MobileVatTypes : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "VatTypes";

        #endregion

        #region Variables

        #region Collections

        private List<MobileVatType> mobileVatTypes;

        #endregion

        #endregion

        #region Ctor

        public MobileVatTypes(MobileParam param, Dictionary<int, string> vattypes)
            : base(param)
        {
            Init();
            AddMobileVatTypes(vattypes);
        }

        /// <summary>Used for errors</summary>
        public MobileVatTypes(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            this.mobileVatTypes = new List<MobileVatType>();
        }

        #endregion

        #region Public methods

        public void AddMobileVatTypes(Dictionary<int, string> vattypes)
        {
            if (vattypes == null)
                return;

            foreach (var vattype in vattypes)
            {
                AddMobileVatType(new MobileVatType(this.Param, vattype));
            }
        }

        public void AddMobileVatType(MobileVatType mobileVatType)
        {
            if (mobileVatType == null)
                return;

            mobileVatTypes.Add(mobileVatType);
        }

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            return MergeDocuments(ROOTNAME, mobileVatTypes.Select(i => i.ToXDocument()).ToList());
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            return XmlUtil.CreateXml(ROOTNAME, MobileVatType.GetDefaultXml());
        }

        #endregion
    }

    internal class MobileVatType : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "VatType";

        #endregion

        #region Variables

        #region Field values

        private readonly int Id;
        private readonly string Value;

        #endregion

        #endregion

        #region Ctor

        public MobileVatType(MobileParam param, KeyValuePair<int, string> vattype) : base(param)
        {
            this.Id = vattype.Key;
            this.Value = vattype.Value;
        }

        /// <summary>
        /// Used for errors
        /// </summary>
        public MobileVatType(MobileParam param, string errorMessage) : base(param, errorMessage)
        {

        }


        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();

            elements.Add(new XElement("VatTypeId", Id.ToString()));
            elements.Add(new XElement("VatTypeName", Value));

            return CreateDocument(ROOTNAME, elements);
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            string xml =
                "<VatTypeId>1</VatTypeId>" +
                "<VatTypeName>Momstyp 1</VatTypeName>";
            return XmlUtil.CreateXml(ROOTNAME, xml);
        }

        #endregion
    }

    #endregion

    #region stock

    internal class MobileStockProductInfo : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "StockProductInfo";

        #endregion

        #region Variables

        private readonly bool defaultStock;
        private readonly int stockId;
        private readonly string stockCode;
        private readonly string stockName;

        private readonly int stockShelfId;
        private readonly string stockShelfCode;
        private readonly string stockShelfName;
        private readonly int quantity;

        #endregion

        #region Ctor

        public MobileStockProductInfo(MobileParam param, StockDTO stockProduct, bool isDefaultStock) : base(param)
        {
            defaultStock = isDefaultStock;
            stockId = stockProduct.StockId;
            stockName = stockProduct.Name;
            stockCode = stockProduct.Code;

            stockShelfId = stockProduct.StockShelfId;
            stockShelfCode = "";
            stockShelfName = stockProduct.StockShelfName;
            quantity = stockProduct.Saldo;
        }

        /// <summary>
        /// Used for errors
        /// </summary>
        public MobileStockProductInfo(MobileParam param, string errorMessage) : base(param, errorMessage)
        {
            //Init();
        }

        #endregion

        #region Public methods

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>
            {
                new XElement("StockId", stockId.ToString()),
                new XElement("StockName", stockName),
                new XElement("StockCode", stockCode),

                new XElement("StockShelfId", stockShelfId.ToString()),
                new XElement("StockShelfCode", stockShelfCode),
                new XElement("StockShelfName", stockShelfName),

                new XElement("Quantity", quantity),

                new XElement("Default", StringUtility.GetString(defaultStock) )
            };

            return CreateDocument(ROOTNAME, elements);
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            string xml =
                "<StockId>1</StockId>" +
                "<StockName>Huvudlager</StockName>" +
                "<StockCode>HL</StockCode>" +
                "<StockShelfId>1</StockShelfId>" +
                "<StockShelfCode>Hylla 1</StockShelfCode>" +
                "<StockShelfName>HYL</StockShelfName>" +
                "<Quantity>1</Quantity>";

            return XmlUtil.CreateXml(ROOTNAME, xml);
        }

        #endregion
    }

    internal class MobileStockProductInfos : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "StockProductInfos";

        #endregion

        #region Variables

        private readonly List<MobileStockProductInfo> mobileStockProductInfos = new List<MobileStockProductInfo>();

        #endregion

        #region Ctor

        public MobileStockProductInfos(MobileParam param, List<StockDTO> stockProducts, int defaultStockId) : base(param)
        {
            AddMobileStockProductInfos(stockProducts, defaultStockId);
        }

        /// <summary>Used for errors</summary>
        public MobileStockProductInfos(MobileParam param, string errorMessage) : base(param, errorMessage)
        {

        }

        #endregion

        #region Public methods

        public void AddMobileStockProductInfos(List<StockDTO> stockProducts, int defaultStockId)
        {
            if (stockProducts != null)
            {
                foreach (var stockProduct in stockProducts)
                {
                    var isDefaultStock = defaultStockId != 0 && defaultStockId == stockProduct.StockId;
                    mobileStockProductInfos.Add(new MobileStockProductInfo(this.Param, stockProduct, isDefaultStock));
                }
            }
        }

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            return MergeDocuments(ROOTNAME, mobileStockProductInfos.Select(i => i.ToXDocument()).ToList());
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            return XmlUtil.CreateXml(ROOTNAME, MobileVatType.GetDefaultXml());
        }

        #endregion
    }
    #endregion

    #region Currency

    internal class MobileCurrencies : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "Currencies";

        #endregion

        #region Variables

        #region Collections

        private readonly List<MobileCurrency> mobileCurrencies = new List<MobileCurrency>();

        #endregion

        #endregion

        #region Ctor

        public MobileCurrencies(MobileParam param, Dictionary<int, string> currencies) : base(param)
        {

            AddMobileCurrencies(currencies);
        }

        /// <summary>Used for errors</summary>
        public MobileCurrencies(MobileParam param, string errorMessage) : base(param, errorMessage)
        {

        }

        #endregion

        #region Public methods

        public void AddMobileCurrencies(Dictionary<int, string> currencies)
        {
            if (currencies == null)
                return;

            foreach (var currency in currencies)
            {
                AddMobileCurrency(new MobileCurrency(this.Param, currency));
            }
        }

        public void AddMobileCurrency(MobileCurrency mobileCurrency)
        {
            if (mobileCurrency == null)
                return;

            mobileCurrencies.Add(mobileCurrency);
        }

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            return MergeDocuments(ROOTNAME, mobileCurrencies.Select(i => i.ToXDocument()).ToList());
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            return XmlUtil.CreateXml(ROOTNAME, MobileCurrency.GetDefaultXml());
        }

        #endregion
    }

    internal class MobileCurrency : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "Currency";

        #endregion

        #region Variables

        private readonly int Id;
        private readonly string Value;

        #endregion

        #region Ctor

        public MobileCurrency(MobileParam param, KeyValuePair<int, string> currency) : base(param)
        {
            this.Id = currency.Key;
            this.Value = currency.Value;

        }

        /// <summary>
        /// Used for errors
        /// </summary>
        public MobileCurrency(MobileParam param, string errorMessage) : base(param, errorMessage)
        {
        }

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();

            elements.Add(new XElement("CurrencyId", Id.ToString()));
            elements.Add(new XElement("CurrencyName", Value));

            return CreateDocument(ROOTNAME, elements);
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            string xml =
                "<CurrencyId>1</CurrencyId>" +
                "<CurrencyName>SEK</CurrencyName>";
            return XmlUtil.CreateXml(ROOTNAME, xml);
        }

        #endregion
    }

    #endregion

    #region Dict

    internal class MobileDicts : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "Dicts";

        #endregion

        #region Variables

        #region Collections

        private List<MobileDict> mobileDicts;

        #endregion

        #endregion

        #region Ctor

        public MobileDicts(MobileParam param, Dictionary<int, string> dict)
            : base(param)
        {
            Init();
            AddMobileDicts(dict);
        }

        public MobileDicts(MobileParam param, Dictionary<string, string> dict)
            : base(param)
        {
            Init();
            AddMobileDicts(dict);
        }

        /// <summary>Used for errors</summary>
        public MobileDicts(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            this.mobileDicts = new List<MobileDict>();
        }

        #endregion

        #region Public methods

        public void AddMobileDicts(Dictionary<int, string> dict)
        {
            if (dict == null)
                return;

            foreach (var pair in dict)
            {
                AddMobileDict(new MobileDict(this.Param, pair));
            }
        }

        public void AddMobileDicts(Dictionary<string, string> dict)
        {
            if (dict == null)
                return;

            foreach (var pair in dict)
            {
                AddMobileDict(new MobileDict(this.Param, pair));
            }
        }

        public void AddMobileDict(MobileDict mobileDict)
        {
            if (mobileDict == null)
                return;

            mobileDicts.Add(mobileDict);
        }

        public void AddProperties(string key, Dictionary<int, bool> employeeValues)
        {
            foreach (var employeeValue in employeeValues)
            {
                var mobileDict = this.mobileDicts.FirstOrDefault(i => i.id == employeeValue.Key.ToString());
                if (mobileDict != null)
                    mobileDict.AddProperty(key, StringUtility.GetString(employeeValue.Value));
            }
        }

        public void AddProperties(string key, Dictionary<int, string> employeeValues)
        {
            foreach (var employeeValue in employeeValues)
            {
                var mobileDict = this.mobileDicts.FirstOrDefault(i => i.id == employeeValue.Key.ToString());
                if (mobileDict != null)
                    mobileDict.AddProperty(key, employeeValue.Value);
            }
        }

        public void AddProperties(string key, Dictionary<int, int> values)
        {
            foreach (var value in values)
            {
                var mobileDict = this.mobileDicts.FirstOrDefault(i => i.id == value.Key.ToString());
                if (mobileDict != null)
                    mobileDict.AddProperty(key, value.Value.ToString());
            }
        }

        public int Size()
        {
            return this.mobileDicts.Count;
        }
        public void SetFirstAsSelected()
        {
            if (this.mobileDicts.Any())
            {
                string id = this.mobileDicts.First().id;
                this.SetSelectedId(id);
            }
        }
        public void SetSelectedIds(List<int> ids)
        {
            foreach (var id in ids)
            {
                this.SetSelectedId(id.ToString());
            }
        }

        public void SetSelectedId(string id)
        {
            var dict = this.mobileDicts.FirstOrDefault(x => x.id == id);
            if (dict != null)
                dict.SetSelected();
        }

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            return MergeDocuments(ROOTNAME, mobileDicts.Select(i => i.ToXDocument()).ToList());
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            return XmlUtil.CreateXml(ROOTNAME, MobileDict.GetDefaultXml());
        }

        #endregion
    }

    internal class MobileDict : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "Dict";

        #endregion

        #region Variables

        #region Field values

        public string id;
        private readonly string value;
        private readonly Dictionary<string, string> properties;
        private bool selected;

        #endregion

        #endregion

        #region Ctor

        public MobileDict(MobileParam param, KeyValuePair<int, string> pair) : base(param)
        {
            Init();

            this.id = pair.Key.ToString();
            this.value = pair.Value;
            this.properties = new Dictionary<string, string>();

        }

        public MobileDict(MobileParam param, KeyValuePair<string, string> pair) : base(param)
        {
            Init();

            this.id = pair.Key;
            this.value = pair.Value;
        }

        /// <summary>
        /// Used for errors
        /// </summary>
        public MobileDict(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            //Set default values
        }

        #endregion

        #region Public methods

        public void AddProperty(string key, string value)
        {
            if (!this.properties.ContainsKey(key))
                this.properties.Add(key, value);
        }

        public void SetSelected()
        {
            this.selected = true;
        }

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();

            elements.Add(new XElement("Id", this.id));
            elements.Add(new XElement("Name", this.value));
            if (this.selected)
                elements.Add(new XElement("Selected", 1));
            if (!this.properties.IsNullOrEmpty())
            {
                foreach (var property in this.properties)
                {
                    elements.Add(new XElement(property.Key, property.Value));
                }
            }

            return CreateDocument(ROOTNAME, elements);
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            string xml =
                "<Id>1</Id>" +
                "<Name>Testmall 1</Name>";
            return XmlUtil.CreateXml(ROOTNAME, xml);
        }

        #endregion
    }

    #endregion

    #region PaymentConditions

    internal class MobilePaymentConditions : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "PaymentConditions";

        #endregion

        #region Variables

        #region Collections

        private readonly List<MobilePaymentCondition> mobilePaymentConditions = new List<MobilePaymentCondition>();

        #endregion

        #endregion

        #region Ctor

        public MobilePaymentConditions(MobileParam param, Dictionary<int, string> paymentConditions) : base(param)
        {
            AddMobilePaymentConditions(paymentConditions);
        }

        /// <summary>Used for errors</summary>
        public MobilePaymentConditions(MobileParam param, string errorMessage) : base(param, errorMessage)
        {

        }

        #endregion

        #region Public methods

        public void AddMobilePaymentConditions(Dictionary<int, string> paymentConditions)
        {
            if (paymentConditions == null)
                return;

            foreach (var paymenycondition in paymentConditions)
            {
                AddMobilePaymentCondition(new MobilePaymentCondition(this.Param, paymenycondition));
            }
        }

        public void AddMobilePaymentCondition(MobilePaymentCondition mobilePaymentCondition)
        {
            if (mobilePaymentCondition == null)
                return;

            mobilePaymentConditions.Add(mobilePaymentCondition);
        }

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            return MergeDocuments(ROOTNAME, mobilePaymentConditions.Select(i => i.ToXDocument()).ToList());
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            return XmlUtil.CreateXml(ROOTNAME, MobilePaymentCondition.GetDefaultXml());
        }

        #endregion
    }

    internal class MobilePaymentCondition : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "PaymentCondition";

        #endregion

        #region Variables

        private readonly int Id;
        private readonly string Value;

        #endregion

        #region Ctor

        public MobilePaymentCondition(MobileParam param, KeyValuePair<int, string> currency) : base(param)
        {
            this.Id = currency.Key;
            this.Value = currency.Value;
        }

        /// <summary>
        /// Used for errors
        /// </summary>
        public MobilePaymentCondition(MobileParam param, string errorMessage) : base(param, errorMessage)
        {

        }

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();

            elements.Add(new XElement("PaymentConditionId", Id.ToString()));
            elements.Add(new XElement("PaymentConditionName", Value));

            return CreateDocument(ROOTNAME, elements);
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            string xml =
                "<PaymentConditionId>1</PaymentConditionId>" +
                "<PaymentConditionName>villkor1</PaymentConditionName>";
            return XmlUtil.CreateXml(ROOTNAME, xml);
        }

        #endregion
    }

    #endregion

    #region WholeSeller

    internal class MobileWholeSellers : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "WholeSellers";

        #endregion

        #region Variables

        private readonly List<MobileWholeSeller> mobileWholeSellers = new List<MobileWholeSeller>();

        #endregion

        #region Ctor

        public MobileWholeSellers(MobileParam param, Dictionary<int, string> wholeSellers) : base(param)
        {
            AddMobileWholeSellers(wholeSellers);
        }

        /// <summary>Used for errors</summary>
        public MobileWholeSellers(MobileParam param, string errorMessage) : base(param, errorMessage)
        {

        }

        #endregion

        #region Public methods

        public void AddMobileWholeSellers(Dictionary<int, string> wholeSellers)
        {
            if (wholeSellers == null)
                return;

            foreach (var wholeSeller in wholeSellers)
            {
                AddMobileWholeSeller(new MobileWholeSeller(this.Param, wholeSeller));
            }
        }

        public void AddMobileWholeSeller(MobileWholeSeller wholeSeller)
        {
            if (wholeSeller == null)
                return;

            mobileWholeSellers.Add(wholeSeller);
        }

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            return MergeDocuments(ROOTNAME, mobileWholeSellers.Select(i => i.ToXDocument()).ToList());
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            return XmlUtil.CreateXml(ROOTNAME, MobileWholeSeller.GetDefaultXml());
        }

        #endregion
    }

    internal class MobileWholeSeller : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "WholeSeller";

        #endregion

        #region Variables

        private readonly int Id;
        private readonly string Value;

        #endregion

        #region Ctor

        public MobileWholeSeller(MobileParam param, KeyValuePair<int, string> wholeSeller) : base(param)
        {
            this.Id = wholeSeller.Key;
            this.Value = wholeSeller.Value;
        }

        /// <summary>
        /// Used for errors
        /// </summary>
        public MobileWholeSeller(MobileParam param, string errorMessage) : base(param, errorMessage)
        {
        }
        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();

            elements.Add(new XElement("WholeSellerId", Id.ToString()));
            elements.Add(new XElement("WholeSellerName", Value));

            return CreateDocument(ROOTNAME, elements);
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            string xml =
                "<WholeSellerId>1</WholeSellerId>" +
                "<WholeSellerName>Elektroskandia</WholeSellerName>";
            return XmlUtil.CreateXml(ROOTNAME, xml);
        }

        #endregion
    }

    #endregion

    #region PriceList

    internal class MobilePriceLists : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "PriceLists";

        #endregion

        #region Variables

        #region Collections

        private List<MobilePriceList> mobilePriceLists;

        #endregion

        #endregion

        #region Ctor

        public MobilePriceLists(MobileParam param, Dictionary<int, string> pricelists)
            : base(param)
        {
            Init();
            AddMobilePriceLists(pricelists);
        }

        /// <summary>Used for errors</summary>
        public MobilePriceLists(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            this.mobilePriceLists = new List<MobilePriceList>();
        }

        #endregion

        #region Public methods

        public void AddMobilePriceLists(Dictionary<int, string> priceLists)
        {
            if (priceLists == null)
                return;

            foreach (var priceList in priceLists)
            {
                AddMobilePriceList(new MobilePriceList(this.Param, priceList));
            }
        }

        public void AddMobilePriceList(MobilePriceList priceList)
        {
            if (priceList == null)
                return;

            mobilePriceLists.Add(priceList);
        }

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            return MergeDocuments(ROOTNAME, mobilePriceLists.Select(i => i.ToXDocument()).ToList());
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            return XmlUtil.CreateXml(ROOTNAME, MobilePriceList.GetDefaultXml());
        }

        #endregion
    }

    internal class MobilePriceList : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "PriceList";

        #endregion

        #region Variables

        #region Field values

        private readonly int id;
        private readonly String value;

        #endregion

        #endregion

        #region Ctor

        public MobilePriceList(MobileParam param, KeyValuePair<int, string> priceList)
            : base(param)
        {
            Init();

            this.id = priceList.Key;
            this.value = priceList.Value;

        }

        /// <summary>
        /// Used for errors
        /// </summary>
        public MobilePriceList(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            //Set default values
        }

        #endregion

        #region Public methods

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();

            elements.Add(new XElement("PriceListId", id.ToString()));
            elements.Add(new XElement("PriceListName", value));

            return CreateDocument(ROOTNAME, elements);
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            string xml =
                "<PriceListId>1</PriceListId>" +
                "<PriceListName>VIP-Kund</PriceListName>";
            return XmlUtil.CreateXml(ROOTNAME, xml);
        }

        #endregion
    }

    #endregion

    #region ContactPerson

    internal class MobileContactPersons : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "Contacts";

        #endregion

        #region Variables

        #region Collections

        private List<MobileContactPerson> mobileContactPersons;

        #endregion

        #endregion

        #region Ctor

        public MobileContactPersons(MobileParam param, List<ContactPerson> contacts)
            : base(param)
        {
            Init();
            AddMobileContactPersons(contacts);
        }

        public MobileContactPersons(MobileParam param, Dictionary<int, string> dict)
            : base(param)
        {
            Init();
            AddMobileContactPersons(dict);
        }

        /// <summary>Used for errors</summary>
        public MobileContactPersons(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            this.mobileContactPersons = new List<MobileContactPerson>();
        }

        #endregion

        #region Public methods

        public void AddMobileContactPersons(Dictionary<int, string> dict)
        {
            if (dict == null)
                return;

            foreach (var item in dict)
            {
                AddMobileContactPerson(new MobileContactPerson(this.Param, item.Key, item.Value));
            }
        }

        public void AddMobileContactPersons(List<ContactPerson> contacts)
        {
            if (contacts == null)
                return;

            foreach (var contactPerson in contacts)
            {
                AddMobileContactPerson(new MobileContactPerson(this.Param, contactPerson.ActorContactPersonId, contactPerson.Name));
            }
        }

        public void AddMobileContactPerson(MobileContactPerson contactPerson)
        {
            if (contactPerson == null)
                return;

            mobileContactPersons.Add(contactPerson);
        }

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            return MergeDocuments(ROOTNAME, mobileContactPersons.Select(i => i.ToXDocument()).ToList());
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            return XmlUtil.CreateXml(ROOTNAME, MobileContactPerson.GetDefaultXml());
        }

        #endregion
    }

    internal class MobileContactPerson : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "Contact";

        #endregion

        #region Variables

        #region Field values

        private readonly int id;
        private readonly String name;
        private readonly String email;
        private readonly String phoneNumber;

        #endregion

        #endregion

        #region Ctor

        public MobileContactPerson(MobileParam param, int id, String name)
            : base(param)
        {
            Init();

            this.id = id;
            this.name = name;
            this.email = "";
            this.phoneNumber = "";

        }

        public MobileContactPerson(MobileParam param, int id, String name, String email, String phoneNumber)
    : base(param)
        {
            Init();

            this.id = id;
            this.name = name;
            this.email = email;
            this.phoneNumber = phoneNumber;

        }

        /// <summary>
        /// Used for errors
        /// </summary>
        public MobileContactPerson(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            //Set default values
        }

        #endregion

        #region Public methods

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();

            elements.Add(new XElement("ContactId", id.ToString()));
            elements.Add(new XElement("ContactName", name));
            elements.Add(new XElement("ContactEmail", email));
            elements.Add(new XElement("ContactPhoneNumber", phoneNumber));

            return CreateDocument(ROOTNAME, elements);
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            string xml =
                "<ContactId>1</ContactId>" +
                "<ContactName>Peter Forsberg</ContactName>";
            return XmlUtil.CreateXml(ROOTNAME, xml);
        }

        #endregion
    }

    #endregion

    #region Image

    internal class MobileImages : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "Images";

        #endregion

        #region Variables

        #region Collections

        private List<MobileImage> mobileImages;

        #endregion

        #endregion

        #region Ctor

        public MobileImages(MobileParam param, List<Tuple<int, String, String, int, String, String>> images)
            : base(param)
        {
            Init();
            AddMobileImages(images);
        }

        /// <summary>Used for errors</summary>
        public MobileImages(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            this.mobileImages = new List<MobileImage>();
        }

        #endregion

        #region Public methods

        public void AddMobileImages(List<Tuple<int, String, String, int, String, String>> images)
        {
            if (images == null)
                return;

            foreach (var image in images)
            {
                AddMobileImage(new MobileImage(this.Param, image.Item1, image.Item2, image.Item3, image.Item4, image.Item5, false, image.Item6));
            }
        }

        public void AddMobileFiles(List<Tuple<int, String, String, int, String, bool>> files)
        {
            if (files == null)
                return;

            foreach (var file in files)
            {
                AddMobileImage(new MobileImage(this.Param, file.Item1, file.Item2, file.Item3, file.Item4, file.Item5, true, "", file.Item6));
            }
        }

        public void AddMobileImage(MobileImage image)
        {
            if (image == null)
                return;

            mobileImages.Add(image);
        }

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            return MergeDocuments(ROOTNAME, mobileImages.Select(i => i.ToXDocument()).ToList());
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            return XmlUtil.CreateXml(ROOTNAME, MobileImage.GetDefaultXml());
        }

        #endregion
    }

    internal class MobileImage : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "Image";

        #endregion

        #region Variables

        #region Field values

        private readonly int id;
        private readonly String path;
        private readonly String description;
        private readonly String extension;
        private readonly String fileName;
        private readonly int type;
        private readonly bool isFile = false;
        private readonly bool canDelete = false;
        #endregion

        #endregion

        #region Ctor

        public MobileImage(MobileParam param, int imageId, String path, String description, int type, String extension, bool isFile = false, String fileName = "", bool canDelete = false)
            : base(param)
        {
            Init();

            this.id = imageId;
            this.path = path;
            this.description = description;
            this.type = type;
            this.extension = extension;
            this.fileName = fileName;
            this.isFile = isFile;
            this.canDelete = canDelete;
        }

        /// <summary>
        /// Used for errors
        /// </summary>
        public MobileImage(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            //Set default values
        }

        #endregion

        #region Public methods

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();

            elements.Add(new XElement("ImageId", id.ToString()));
            elements.Add(new XElement("Path", path));
            elements.Add(new XElement("FileType", extension));
            elements.Add(new XElement("Description", GetTextOrCDATA(description)));
            elements.Add(new XElement("FileName", GetTextOrCDATA(fileName)));
            elements.Add(new XElement("Type", type.ToString())); //type is 0 for file, in future if we want to add files in the app we may need to return a new tag "SoeDataStorageType" that will be sent in the AddImage method            
            elements.Add(new XElement("IsFile", StringUtility.GetString(isFile)));
            elements.Add(new XElement("CanDelete", StringUtility.GetString(canDelete)));

            return CreateDocument(ROOTNAME, elements);
        }

        public override XDocument ToXDocument(MobileTask task)
        {
            if (base.Failed)
                return MobileMessages.GetErrorMessageDocument(base.ErrorMessage);

            bool result = base.GetTaskResult(task);
            switch (task)
            {
                case MobileTask.AddOrderImage:
                case MobileTask.DeleteOrderImage:
                case MobileTask.EditOrderImage:
                    return MobileMessages.GetSuccessDocument(result);
                default:
                    return base.ToXDocument(task);
            }
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            string xml =
                "<ImageId>1</ImageId>" +
                "<Path></Path>" +
                "<Description></Description>" +
                "<Type></Type>";
            return XmlUtil.CreateXml(ROOTNAME, xml);
        }

        public static string GetDefaultSaveXml()
        {
            string xml =
                "<Success>1<Success>";

            return xml;
        }

        #endregion
    }

    #endregion

    #region OrderAddress

    internal class MobileOrderAddressItems : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "AddressItems";

        #endregion

        #region Variables

        #region Collections

        private List<MobileOrderAddressItem> mobileAddressItems;

        #endregion

        #endregion

        #region Ctor

        public MobileOrderAddressItems(MobileParam param, List<ContactAddress> addressItems)
            : base(param)
        {
            Init();
            AddMobileAddressItems(addressItems);
        }

        /// <summary>Used for errors</summary>
        public MobileOrderAddressItems(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            this.mobileAddressItems = new List<MobileOrderAddressItem>();
        }

        #endregion

        #region Public methods

        public void AddMobileAddressItems(List<ContactAddress> addressItems)
        {
            if (addressItems == null)
                return;

            foreach (var addressItem in addressItems)
            {
                AddMobileAddressItem(new MobileOrderAddressItem(this.Param, addressItem));
            }
        }

        public void AddMobileAddressItem(MobileOrderAddressItem image)
        {
            if (image == null)
                return;

            mobileAddressItems.Add(image);
        }

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            return MergeDocuments(ROOTNAME, mobileAddressItems.Select(i => i.ToXDocument()).ToList());
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            return XmlUtil.CreateXml(ROOTNAME, MobileOrderAddressItem.GetDefaultXml());
        }

        #endregion
    }

    internal class MobileOrderAddressItem : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "AddressItem";

        #endregion

        #region Variables

        #region Field values

        private readonly int id;
        private readonly String address;

        #endregion

        #endregion

        #region Ctor

        public MobileOrderAddressItem(MobileParam param, ContactAddress addressItem)
            : base(param)
        {
            Init();

            this.id = addressItem.ContactAddressId;
            this.address = addressItem.Address;
        }

        /// <summary>
        /// Used for errors
        /// </summary>
        public MobileOrderAddressItem(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            //Set default values
        }

        #endregion

        #region Public methods

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();

            elements.Add(new XElement("ContactAddressId", id.ToString()));
            elements.Add(new XElement("Address", address));

            return CreateDocument(ROOTNAME, elements);
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            string xml =
                "<ContactAddressId></ContactAddressId>" +
                "<Address></Address>";
            return XmlUtil.CreateXml(ROOTNAME, xml);
        }

        #endregion
    }

    #endregion

    #region Checklist

    internal class MobileCheckLists : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "CheckLists";

        #endregion

        #region Variables

        #region Collections

        private List<MobileCheckList> mobileCheckLists;

        #endregion

        #endregion

        #region Ctor

        public MobileCheckLists(MobileParam param, List<ChecklistHead> checklists)
            : base(param)
        {
            Init();
            AddMobileCheckLists(checklists);
        }

        public MobileCheckLists(MobileParam param, List<ChecklistHeadRecord> checklists)
            : base(param)
        {
            Init();
            AddMobileCheckLists(checklists);
        }

        /// <summary>Used for errors</summary>
        public MobileCheckLists(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            this.mobileCheckLists = new List<MobileCheckList>();
        }

        #endregion

        #region Public methods

        public void AddMobileCheckLists(List<ChecklistHead> checklists)
        {
            if (checklists == null)
                return;

            foreach (var checklist in checklists)
            {
                AddMobileCheckList(new MobileCheckList(this.Param, checklist));
            }
        }

        public void AddMobileCheckLists(List<ChecklistHeadRecord> checklists)
        {
            if (checklists == null)
                return;

            foreach (var checklist in checklists)
            {
                AddMobileCheckList(new MobileCheckList(this.Param, checklist));
            }
        }

        public void AddMobileCheckList(MobileCheckList checklist)
        {
            if (checklist == null)
                return;

            mobileCheckLists.Add(checklist);
        }

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            return MergeDocuments(ROOTNAME, mobileCheckLists.Select(i => i.ToXDocument()).ToList());
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            return XmlUtil.CreateXml(ROOTNAME, MobileCheckList.GetDefaultXml());
        }

        #endregion
    }

    internal class MobileCheckList : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "CheckList";

        #endregion

        #region Variables

        #region Field values

        private readonly int headId;
        private readonly int headRecordId;
        private readonly String name;
        private readonly String description;

        #endregion

        #endregion

        #region Ctor

        public MobileCheckList(MobileParam param)
            : base(param)
        {
            Init();
        }

        public MobileCheckList(MobileParam param, ChecklistHead checklist)
            : base(param)
        {
            Init();

            this.headId = checklist.ChecklistHeadId;
            this.name = checklist.Name;
            this.description = checklist.Description;
        }

        public MobileCheckList(MobileParam param, ChecklistHeadRecord checklist)
            : base(param)
        {
            Init();

            this.headId = checklist.ChecklistHeadId;
            this.headRecordId = checklist.ChecklistHeadRecordId;
            this.name = checklist.ChecklistHead.Name;
            this.description = checklist.ChecklistHead.Description;
        }

        /// <summary>
        /// Used for errors
        /// </summary>
        public MobileCheckList(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            //Set default values
        }

        #endregion

        #region Public methods

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();

            elements.Add(new XElement("CheckListHeadId", headId.ToString()));
            elements.Add(new XElement("CheckListHeadRecordId", headRecordId.ToString()));
            elements.Add(new XElement("Name", name));
            elements.Add(new XElement("Description", description));

            return CreateDocument(ROOTNAME, elements);
        }

        public override XDocument ToXDocument(MobileTask task)
        {
            if (base.Failed)
                return MobileMessages.GetErrorMessageDocument(base.ErrorMessage);

            bool result = base.GetTaskResult(task);
            switch (task)
            {
                case MobileTask.AddCheckList:
                case MobileTask.DeleteOrderChecklist:
                    return MobileMessages.GetSuccessDocument(result);
                default:
                    return base.ToXDocument(task);
            }
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            string xml =
                "<CheckListHeadId>1</CheckListHeadId>" +
                "<CheckListHeadRecordId>2</CheckListHeadRecordId>" +
                "<Name>Fixa avlopp</Name>" +
                "<Description>Så här gör du:</Description>";
            return XmlUtil.CreateXml(ROOTNAME, xml);
        }

        #endregion
    }

    internal class MobileCheckListRows : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "CheckListRows";

        #endregion

        #region Variables

        #region Collections

        private List<MobileCheckListRow> mobileCheckListRows;

        #endregion

        #endregion

        #region Ctor

        public MobileCheckListRows(MobileParam param)
            : base(param)
        {
            Init();
        }


        public MobileCheckListRows(MobileParam param, List<ChecklistExtendedRowDTO> checklistRows)
            : base(param)
        {
            Init();
            AddMobileCheckListRows(checklistRows);
        }

        /// <summary>Used for errors</summary>
        public MobileCheckListRows(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            this.mobileCheckListRows = new List<MobileCheckListRow>();
        }

        #endregion

        #region Public methods

        public void AddMobileCheckListRows(List<ChecklistExtendedRowDTO> checklistRows)
        {
            if (checklistRows == null)
                return;

            foreach (var checklistRow in checklistRows)
            {
                if (!string.IsNullOrEmpty(checklistRow.Text))
                    AddMobileCheckListRow(new MobileCheckListRow(this.Param, checklistRow));
            }
        }

        public void AddMobileCheckListRow(MobileCheckListRow checklist)
        {
            if (checklist == null)
                return;

            mobileCheckListRows.Add(checklist);
        }

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            return MergeDocuments(ROOTNAME, mobileCheckListRows.Select(i => i.ToXDocument()).ToList());
        }

        public override XDocument ToXDocument(MobileTask task)
        {
            if (base.Failed)
                return MobileMessages.GetErrorMessageDocument(base.ErrorMessage);

            bool result = base.GetTaskResult(task);
            switch (task)
            {
                case MobileTask.SaveChecklistAnswers:
                    return MobileMessages.GetSuccessDocument(result);
                default:
                    return base.ToXDocument(task);
            }
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            return XmlUtil.CreateXml(ROOTNAME, MobileCheckListRow.GetDefaultXml());
        }

        #endregion
    }

    internal class MobileCheckListRow : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "CheckListRow";

        #endregion

        #region Variables

        #region Field values

        private readonly int checkListRowId;
        private readonly int checkListRowRecordId;
        private readonly int rowNr;
        private readonly String text; //i.e question
        private readonly int rowType; //i.e "Yes/No"
        private readonly bool mandatory;
        private readonly int dataType;
        private readonly String value; //Answer
        private readonly String comment;
        private readonly DateTime? date;
        private readonly int multipleChoiceAnswerHeadId;

        #endregion

        #endregion

        #region Ctor

        public MobileCheckListRow(MobileParam param, ChecklistExtendedRowDTO checklistRow)
            : base(param)
        {
            Init();

            this.checkListRowId = checklistRow.RowId;
            this.checkListRowRecordId = checklistRow.RowRecordId;
            this.rowNr = checklistRow.RowNr;
            this.text = checklistRow.Text;
            this.rowType = (int)checklistRow.Type;
            this.mandatory = checklistRow.Mandatory;
            this.dataType = checklistRow.DataTypeId;
            this.comment = checklistRow.Comment;
            this.date = checklistRow.Date;
            this.value = checklistRow.Value;
            this.multipleChoiceAnswerHeadId = checklistRow.CheckListMultipleChoiceAnswerHeadId ?? 0;
            if (this.multipleChoiceAnswerHeadId != 0 && checklistRow.IntData.HasValue)
                this.value = checklistRow.IntData.Value.ToString();

        }

        /// <summary>
        /// Used for errors
        /// </summary>
        public MobileCheckListRow(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            //Set default values
        }

        #endregion

        #region Public methods

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();

            if (!string.IsNullOrEmpty(text))
            {
                elements.Add(new XElement("RowId", this.checkListRowId));
                elements.Add(new XElement("RowRecordId", this.checkListRowRecordId));
                elements.Add(new XElement("RowNr", this.rowNr));
                elements.Add(new XElement("Text", this.text));
                elements.Add(new XElement("RowType", this.rowType)); //TermGroup_ChecklistRowType
                elements.Add(new XElement("Mandatory", this.mandatory));
                elements.Add(new XElement("DataType", this.dataType)); //SettingDataType
                elements.Add(new XElement("Value", this.value));
                elements.Add(new XElement("Comment", this.comment));
                elements.Add(new XElement("Date", this.date.HasValue ? this.date.Value.ToShortDateString() : ""));
                elements.Add(new XElement("MultipleChoiceAnswerHeadId", this.multipleChoiceAnswerHeadId));
            }

            return CreateDocument(ROOTNAME, elements);
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            string xml =
                "<RowId>11</RowId>" +
                "<RowRecordId>1110</RowRecordId>" +
                "<RowNr>1</RowNr>" +
                "<Text>Kontrollerade elcentral</Text>" +
                "<RowType>2</RowType>" +
                "<Mandatory>1</Mandatory>" +
                "<DataType>3</DataType>" +
                "<Value>1</Value>" +
                "<Comment>dubbelkollade oxå</Comment>" +
                "<Date>2011-02-29</Date>" +
                "<MultipleChoiceAnswerHeadId>1110</MultipleChoiceAnswerHeadId>";
            return XmlUtil.CreateXml(ROOTNAME, xml);
        }

        #endregion
    }

    internal class MobileCheckListAnswers
    {
        #region Variables

        //[#] = next element och [##] = next answer

        public List<MobileCheckListAnswer> ParsedAnswers = new List<MobileCheckListAnswer>();
        public bool ParseSucceded = true;

        #endregion

        #region Ctor

        public MobileCheckListAnswers(String answers)
        {
            Parse(answers);
        }

        #endregion

        #region Help-methods

        private void Parse(String answers)
        {
            string[] answerSeparator = new string[1];
            answerSeparator[0] = "[##]";
            string[] elementSeparator = new string[1];
            elementSeparator[0] = "[#]";

            string[] separatedAnswers = answers.Split(answerSeparator, StringSplitOptions.RemoveEmptyEntries);

            foreach (string separatedAnswer in separatedAnswers)
            {
                string[] separatedElements = separatedAnswer.Trim().Split(elementSeparator, StringSplitOptions.None);
                if (separatedElements.Count() != 5)
                {
                    ParseSucceded = false;
                    return;
                }

                string rowIdStr = separatedElements[0].Trim();
                string rowRecordIdStr = separatedElements[1].Trim();
                string value = separatedElements[2].Trim();
                string comment = separatedElements[3].Trim();
                string dateValue = separatedElements[4].Trim();

                if (!Int32.TryParse(rowRecordIdStr, out int rowRecordId) || !Int32.TryParse(rowIdStr, out int rowId))
                {
                    ParseSucceded = false;
                    return;
                }

                DateTime? date = null;
                if (!String.IsNullOrEmpty(dateValue))
                    date = CalendarUtility.GetNullableDateTime(dateValue);

                MobileCheckListAnswer answer = new MobileCheckListAnswer(rowId, rowRecordId, value, comment, date);
                ParsedAnswers.Add(answer);
            }
        }

        #endregion
    }

    internal class MobileCheckListAnswer
    {
        #region Variables

        public int CheckListRowRecordId;
        public int CheckListRowId;
        public String Answer;
        public String Comment;
        public DateTime? Date;

        #endregion

        #region Ctor

        public MobileCheckListAnswer(int checkListRowId, int checkListRowRecordId, String answer, String comment, DateTime? date)
        {
            this.CheckListRowId = checkListRowId;
            this.CheckListRowRecordId = checkListRowRecordId;
            this.Answer = answer;
            this.Comment = comment;
            this.Date = date;
        }

        #endregion
    }

    internal class MobileMultipleChoiceAnswerRows : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "MultipleChoiceAnswerRows";

        #endregion

        #region Variables

        #region Collections

        private List<MobileMultipleChoiceAnswerRow> mobileMultipleChoiceAnswerRows;

        #endregion

        #endregion

        #region Ctor

        public MobileMultipleChoiceAnswerRows(MobileParam param, List<CheckListMultipleChoiceAnswerRow> rows)
            : base(param)
        {
            Init();
            AddMobileMultipleChoiceAnswerRows(rows);
        }

        /// <summary>Used for errors</summary>
        public MobileMultipleChoiceAnswerRows(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            this.mobileMultipleChoiceAnswerRows = new List<MobileMultipleChoiceAnswerRow>();
        }

        #endregion

        #region Public methods

        public void AddMobileMultipleChoiceAnswerRows(List<CheckListMultipleChoiceAnswerRow> rows)
        {
            if (rows == null)
                return;

            //Add empty row
            AddMobileMultipleChoiceAnswerRow(new MobileMultipleChoiceAnswerRow(this.Param, (CheckListMultipleChoiceAnswerRow)null));

            foreach (var row in rows)
            {
                AddMobileMultipleChoiceAnswerRow(new MobileMultipleChoiceAnswerRow(this.Param, row));
            }
        }

        public void AddMobileMultipleChoiceAnswerRow(MobileMultipleChoiceAnswerRow row)
        {
            if (row == null)
                return;

            mobileMultipleChoiceAnswerRows.Add(row);
        }

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            return MergeDocuments(ROOTNAME, mobileMultipleChoiceAnswerRows.Select(i => i.ToXDocument()).ToList());
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            return XmlUtil.CreateXml(ROOTNAME, MobileMultipleChoiceAnswerRow.GetDefaultXml());
        }

        #endregion
    }

    internal class MobileMultipleChoiceAnswerRow : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "MultipleChoiceAnswerRow";

        #endregion

        #region Variables

        #region Field values

        private readonly int rowId;
        private readonly String text;

        #endregion

        #endregion

        #region Ctor

        public MobileMultipleChoiceAnswerRow(MobileParam param)
            : base(param)
        {
            Init();
        }

        public MobileMultipleChoiceAnswerRow(MobileParam param, CheckListMultipleChoiceAnswerRow row)
            : base(param)
        {
            Init();

            if (row == null)
            {
                this.rowId = 0;
                this.text = "";
            }
            else
            {
                this.rowId = row.CheckListMultipleChoiceAnswerRowId;
                this.text = row.Question;
            }

        }

        /// <summary>
        /// Used for errors
        /// </summary>
        public MobileMultipleChoiceAnswerRow(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            //Set default values
        }

        #endregion

        #region Public methods

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();

            elements.Add(new XElement("RowId", this.rowId.ToString()));
            elements.Add(new XElement("text", this.text));

            return CreateDocument(ROOTNAME, elements);
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            string xml =
                "<RowId>1</RowId>" +
                "<Text>källare</Text>";
            return XmlUtil.CreateXml(ROOTNAME, xml);
        }

        #endregion
    }

    internal class MobileTextBlocks : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "TextBlocks";

        #endregion

        #region Variables

        #region Collections

        private List<MobileTextBlock> mobileTextBlocks;

        #endregion

        #endregion

        #region Ctor

        public MobileTextBlocks(MobileParam param, List<Textblock> textBlocks)
            : base(param)
        {
            Init();
            AddMobileTextBlocks(textBlocks);
        }

        /// <summary>Used for errors</summary>
        public MobileTextBlocks(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            this.mobileTextBlocks = new List<MobileTextBlock>();
        }

        #endregion

        #region Public methods

        public void AddMobileTextBlocks(List<Textblock> textBlocks)
        {
            if (textBlocks == null)
                return;

            foreach (var textBlock in textBlocks)
            {
                AddMobileTextBlock(new MobileTextBlock(this.Param, textBlock));
            }
        }

        public void AddMobileTextBlock(MobileTextBlock textBlock)
        {
            if (textBlock == null)
                return;

            mobileTextBlocks.Add(textBlock);
        }

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            return MergeDocuments(ROOTNAME, mobileTextBlocks.Select(i => i.ToXDocument()).ToList());
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            return XmlUtil.CreateXml(ROOTNAME, MobileTextBlock.GetDefaultXml());
        }

        #endregion
    }

    internal class MobileTextBlock : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "TextBlock";

        #endregion

        #region Variables

        #region Field values

        private readonly int id;
        private readonly String text;

        #endregion

        #endregion

        #region Ctor

        public MobileTextBlock(MobileParam param)
            : base(param)
        {
            Init();
        }

        public MobileTextBlock(MobileParam param, Textblock row)
            : base(param)
        {
            Init();

            this.id = row.TextblockId;
            this.text = row.Text;
        }

        /// <summary>
        /// Used for errors
        /// </summary>
        public MobileTextBlock(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            //Set default values
        }

        #endregion

        #region Public methods

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();

            elements.Add(new XElement("Id", this.id.ToString()));
            elements.Add(new XElement("Text", this.text));

            return CreateDocument(ROOTNAME, elements);
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            string xml =
                "<Id>1</Id>" +
                "<Text>källare</Text>";
            return XmlUtil.CreateXml(ROOTNAME, xml);
        }

        #endregion
    }

    #endregion

    #region OrderUsers

    internal class MobileOrderUsers : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "OrderUsers";

        #endregion

        #region Variables

        #region Collections

        private List<MobileOrderUser> mobileOrderUsers;

        #endregion

        #endregion

        #region Ctor

        public MobileOrderUsers(MobileParam param) : base(param)
        {
            Init();
        }

        public MobileOrderUsers(MobileParam param, List<OriginUser> selectedUsers, List<User> companyUsers) : base(param)
        {
            Init();
            AddMobileOrderUsers(selectedUsers, companyUsers);
        }

        public MobileOrderUsers(MobileParam param, List<OriginUserView> users) : base(param)
        {
            Init();
            AddMobileOrderUsers(users);
        }

        /// <summary>Used for errors</summary>
        public MobileOrderUsers(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            this.mobileOrderUsers = new List<MobileOrderUser>();
        }

        #endregion

        #region Public methods

        public void AddMobileOrderUsers(List<OriginUserView> users)
        {
            foreach (var user in users)
            {
                AddMobileOrderUser(new MobileOrderUser(this.Param, user));
            }
        }

        public void AddMobileOrderUsers(List<OriginUser> selectedUsers, List<User> companyUsers)
        {
            if (selectedUsers == null)
                return;

            foreach (var selectedUser in selectedUsers)
            {
                AddMobileOrderUser(new MobileOrderUser(this.Param, selectedUser, true, selectedUser.Main));
            }

            foreach (var companyUser in companyUsers)
            {
                if (selectedUsers.Any(ou => ou.User.UserId == companyUser.UserId))
                    continue;

                AddMobileOrderUser(new MobileOrderUser(this.Param, companyUser));
            }
        }

        private void AddMobileOrderUser(MobileOrderUser user)
        {
            if (user == null)
                return;

            mobileOrderUsers.Add(user);
        }

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            return MergeDocuments(ROOTNAME, mobileOrderUsers.Select(i => i.ToXDocument()).ToList());
        }

        public override XDocument ToXDocument(MobileTask task)
        {
            if (base.Failed)
                return MobileMessages.GetErrorMessageDocument(base.ErrorMessage);

            bool result = base.GetTaskResult(task);
            switch (task)
            {
                case MobileTask.SaveOriginUsers:
                    return MobileMessages.GetSuccessDocument(result);
                default:
                    return base.ToXDocument(task);
            }
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            return XmlUtil.CreateXml(ROOTNAME, MobileOrderUser.GetDefaultXml());
        }

        public static string GetDefaultSaveXml()
        {
            string xml =
                "<Success>1<Success>";

            return xml;
        }
        #endregion
    }

    internal class MobileOrderUser : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "OrderUser";

        #endregion

        #region Variables

        private readonly int userId;
        private readonly string name;
        private readonly bool isSelected;
        private readonly bool isMainUser;
        private readonly DateTime? readyDate;
        private readonly bool outputReadyDate;

        #endregion

        #region Ctor

        public MobileOrderUser(MobileParam param, User user) : base(param)
        {
            this.userId = user.UserId;
            this.name = user.Name;
            this.isSelected = false;
        }

        public MobileOrderUser(MobileParam param, OriginUser oUser, bool isSelected, bool isMainUser) : base(param)
        {
            this.userId = oUser.User.UserId;
            this.name = oUser.User.Name;
            this.isSelected = isSelected;
            this.isMainUser = isMainUser;
        }

        public MobileOrderUser(MobileParam param, OriginUserView user) : base(param)
        {
            userId = user.UserId;
            name = user.Name;
            isMainUser = user.Main;
            readyDate = user.ReadyDate;
            isSelected = true;
            outputReadyDate = true;
        }

        /// <summary>
        /// Used for errors
        /// </summary>
        public MobileOrderUser(MobileParam param, string errorMessage) : base(param, errorMessage)
        {

        }

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();

            elements.Add(new XElement("UserId", userId));
            elements.Add(new XElement("Name", name));
            elements.Add(new XElement("Selected", isSelected));
            elements.Add(new XElement("MainUser", isMainUser));
            if (outputReadyDate)
            {
                elements.Add(new XElement("ReadyDate", readyDate.HasValue ? readyDate.Value.ToShortDateShortTimeString() : ""));
            }

            return CreateDocument(ROOTNAME, elements);
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            string xml =
                "<UserId>1110</UserId>" +
                "<Name>Kalle</Name>" +
                "<Selected>true</Selected>" +
                "<MainUser>true</MainUser>";

            return XmlUtil.CreateXml(ROOTNAME, xml);
        }

        #endregion
    }

    #endregion

    #region Maps

    internal class MobileMapLocation : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "MapLocation";

        #endregion

        #region Ctor

        public MobileMapLocation(MobileParam param)
            : base(param)
        {
            Init();
        }

        /// <summary>Used for errors</summary>
        public MobileMapLocation(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            //Set default values
        }

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument(MobileTask task)
        {
            if (base.Failed)
                return MobileMessages.GetErrorMessageDocument(base.ErrorMessage);

            bool result = base.GetTaskResult(task);
            switch (task)
            {
                case MobileTask.SaveMapLocation:
                    return MobileMessages.GetSuccessDocument(result);
                default:
                    return base.ToXDocument(task);
            }
        }

        #endregion

        #region Static methods

        public static string GetDefaultSaveXml()
        {
            string xml =
                "<Success>1<Success>";

            return xml;
        }

        #endregion
    }

    #endregion

    #region TimeCode

    internal class MobileTimeCodes : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "TimeCodes";

        #endregion

        #region Variables

        #region Collections

        private List<MobileTimeCode> mobileTimeCodes;

        #endregion

        #endregion

        #region Ctor

        public MobileTimeCodes(MobileParam param, List<TimeCode> timecodes, TimeCode defaultTimeCode)
            : base(param)
        {
            Init();
            AddMobileTimeCodes(timecodes, defaultTimeCode);
        }

        /// <summary>Used for errors</summary>
        public MobileTimeCodes(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            this.mobileTimeCodes = new List<MobileTimeCode>();
        }

        #endregion

        #region Public methods

        public void AddMobileTimeCodes(List<TimeCode> timecodes, TimeCode defaultTimeCode)
        {
            if (timecodes == null || defaultTimeCode == null)
                return;


            foreach (var timecode in timecodes)
            {
                AddMobileTimeCode(new MobileTimeCode(this.Param, timecode, (defaultTimeCode.TimeCodeId == timecode.TimeCodeId)));
            }
        }

        public void AddMobileTimeCode(MobileTimeCode mobileTimeCode)
        {
            if (mobileTimeCode == null)
                return;

            mobileTimeCodes.Add(mobileTimeCode);
        }

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            return MergeDocuments(ROOTNAME, mobileTimeCodes.Select(i => i.ToXDocument()).ToList());
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            return XmlUtil.CreateXml(ROOTNAME, MobileTimeCode.GetDefaultXml());
        }

        #endregion
    }

    internal class MobileTimeCode : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "TimeCode";

        #endregion

        #region Variables

        #region Field values

        private readonly int id;
        private readonly String name;
        private readonly bool isDefault;

        #endregion

        #endregion

        #region Ctor

        public MobileTimeCode(MobileParam param, TimeCode timecode, bool isDefault) : base(param)
        {
            Init();

            this.id = timecode.TimeCodeId;
            this.name = timecode.Name;
            this.isDefault = isDefault;

        }

        /// <summary>
        /// Used for errors
        /// </summary>
        public MobileTimeCode(MobileParam param, string errorMessage) : base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            //Set default values
        }

        #endregion

        #region Public methods

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();

            elements.Add(new XElement("TimeCodeId", id.ToString()));
            elements.Add(new XElement("Name", GetTextOrCDATA(name)));
            elements.Add(new XElement("Default", StringUtility.GetString(isDefault)));

            return CreateDocument(ROOTNAME, elements);
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            string xml =
                "<TimeCodeId>22</TimeCodeId>" +
                "<Name>Lärning</Name>" +
                "<Default>1</Default>";
            return XmlUtil.CreateXml(ROOTNAME, xml);
        }

        #endregion
    }

    internal class MobileBreakTimeCodes : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "BreakTimeCodes";

        #endregion

        #region Variables

        #region Collections

        private List<MobileBreakTimeCode> mobileBreakTimeCodes;

        #endregion

        #endregion

        #region Ctor

        public MobileBreakTimeCodes(MobileParam param, List<TimeCodeBreak> timecodes)
            : base(param)
        {
            Init();
            AddMobileBreakTimeCodes(timecodes);
        }

        /// <summary>Used for errors</summary>
        public MobileBreakTimeCodes(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            this.mobileBreakTimeCodes = new List<MobileBreakTimeCode>();
        }

        #endregion

        #region Public methods

        public void AddMobileBreakTimeCodes(List<TimeCodeBreak> timecodes)
        {
            if (timecodes == null)
                return;


            foreach (var timecode in timecodes)
            {
                AddMobileBreakTimeCode(new MobileBreakTimeCode(this.Param, timecode));
            }
        }

        public void AddMobileBreakTimeCode(MobileBreakTimeCode mobileBreakTimeCode)
        {
            if (mobileBreakTimeCode == null)
                return;

            mobileBreakTimeCodes.Add(mobileBreakTimeCode);
        }

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            return MergeDocuments(ROOTNAME, mobileBreakTimeCodes.Select(i => i.ToXDocument()).ToList());
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            return XmlUtil.CreateXml(ROOTNAME, MobileBreakTimeCode.GetDefaultXml());
        }

        #endregion
    }

    internal class MobileBreakTimeCode : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "BreakTimeCode";

        #endregion

        #region Variables

        #region Field values

        private readonly int id;
        private readonly String name;
        private readonly int minutes;

        #endregion

        #endregion

        #region Ctor

        public MobileBreakTimeCode(MobileParam param, TimeCodeBreak timecode)
            : base(param)
        {
            Init();

            this.id = timecode.TimeCodeId;
            this.name = timecode.Name;
            this.minutes = timecode.DefaultMinutes;

        }

        /// <summary>
        /// Used for errors
        /// </summary>
        public MobileBreakTimeCode(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            //Set default values
        }

        #endregion

        #region Public methods

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();

            elements.Add(new XElement("TimeCodeId", id.ToString()));
            elements.Add(new XElement("Name", name));
            elements.Add(new XElement("Minutes", minutes));

            return CreateDocument(ROOTNAME, elements);
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            string xml =
                "<TimeCodeId>22</TimeCodeId>" +
                "<Name>15 min</Name>" +
                "<Minutes>15</Minutes>";
            return XmlUtil.CreateXml(ROOTNAME, xml);
        }

        #endregion
    }

    internal class MobileAdditionDeductionTimeCodes : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "AdditionDeductionTimeCodes";

        #endregion

        #region Variables

        #region Collections

        private List<MobileAdditionDeductionTimeCode> mobileTimeCodes;

        #endregion

        #endregion

        #region Ctor

        public MobileAdditionDeductionTimeCodes(MobileParam param, List<TimeCodeAdditionDeduction> timecodes) : base(param)
        {
            Init();
            AddMobileAdditionDeductionTimeCodes(timecodes);
        }

        /// <summary>Used for errors</summary>
        public MobileAdditionDeductionTimeCodes(MobileParam param, string errorMessage) : base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            this.mobileTimeCodes = new List<MobileAdditionDeductionTimeCode>();
        }

        #endregion

        #region Public methods

        public void AddMobileAdditionDeductionTimeCodes(List<TimeCodeAdditionDeduction> timecodes)
        {
            foreach (var timecode in timecodes)
            {
                AddMobileAdditionDeductionTimeCode(new MobileAdditionDeductionTimeCode(this.Param, timecode));
            }
        }

        public void AddMobileAdditionDeductionTimeCode(MobileAdditionDeductionTimeCode mobileTimeCode)
        {
            if (mobileTimeCode == null)
                return;

            mobileTimeCodes.Add(mobileTimeCode);
        }

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            return MergeDocuments(ROOTNAME, mobileTimeCodes.Select(i => i.ToXDocument()).ToList());
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            return XmlUtil.CreateXml(ROOTNAME, MobileAdditionDeductionTimeCode.GetDefaultXml());
        }

        #endregion
    }

    internal class MobileAdditionDeductionTimeCode : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "AdditionDeductionTimeCode";

        #endregion

        #region Variables

        #region Field values

        private readonly int id;
        private readonly int expenseType;
        private readonly string name;
        private readonly bool hasInvoiceProduct;
        private readonly bool isRegistrationTypeQuantity;
        private readonly bool commentMandatory;
        private readonly decimal salesPrice = 0;

        #endregion

        #endregion

        #region Ctor

        public MobileAdditionDeductionTimeCode(MobileParam param, TimeCodeAdditionDeduction timecode) : base(param)
        {
            Init();

            this.id = timecode.TimeCodeId;
            this.name = timecode.Name;
            this.hasInvoiceProduct = timecode.HasInvoiceProducts;
            this.expenseType = timecode.ExpenseType;
            this.isRegistrationTypeQuantity = timecode.IsRegistrationTypeQuantity;
            this.commentMandatory = timecode.CommentMandatory;
            if (timecode.TimeCodeInvoiceProduct.Any())
            {
                foreach (var timeCodeInvoiceProduct in timecode.TimeCodeInvoiceProduct)
                {
                    if (timeCodeInvoiceProduct.InvoiceProduct != null)
                    {
                        salesPrice += timeCodeInvoiceProduct.Factor * timeCodeInvoiceProduct.InvoiceProduct.PurchasePrice;
                    }
                }
            }
        }

        /// <summary>
        /// Used for errors
        /// </summary>
        public MobileAdditionDeductionTimeCode(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            //Set default values
        }

        #endregion

        #region Public methods

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();

            elements.Add(new XElement("TimeCodeId", id.ToString()));
            elements.Add(new XElement("ExpenseType", expenseType.ToString()));
            elements.Add(new XElement("Name", GetTextOrCDATA(name)));
            elements.Add(new XElement("HasInvoiceProduct", StringUtility.GetString(hasInvoiceProduct)));
            elements.Add(new XElement("SalesPrice", salesPrice));
            elements.Add(new XElement("IsRTQ", StringUtility.GetString(isRegistrationTypeQuantity)));
            elements.Add(new XElement("CommentMandatory", StringUtility.GetString(commentMandatory)));

            return CreateDocument(ROOTNAME, elements);
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            string xml =
                "<TimeCodeId>22</TimeCodeId>" +
                "<ExpenseType>1</ExpenseType>" +
                "<Name>Lärning</Name>" +
                "<HasInvoiceProduct>1</HasInvoiceProduct>";
            return XmlUtil.CreateXml(ROOTNAME, xml);
        }

        #endregion
    }

    #endregion

    #region ShiftType

    internal class MobileShiftTypes : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "ShiftTypes";

        #endregion

        #region Variables

        #region Collections

        private List<MobileShiftType> mobileShiftTypes;

        #endregion

        #endregion

        #region Ctor

        public MobileShiftTypes(MobileParam param, List<ShiftTypeDTO> shiftTypes)
            : base(param)
        {
            Init();
            AddMobileShiftTypes(shiftTypes);
        }

        /// <summary>Used for errors</summary>
        public MobileShiftTypes(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            this.mobileShiftTypes = new List<MobileShiftType>();
        }

        #endregion

        #region Public methods

        public void AddMobileShiftTypes(List<ShiftTypeDTO> shiftTypes)
        {
            if (shiftTypes == null)
                return;


            foreach (var shiftType in shiftTypes)
            {
                AddMobileShiftType(new MobileShiftType(this.Param, shiftType));
            }
        }

        public void AddMobileShiftType(MobileShiftType mobileShiftType)
        {
            if (mobileShiftType == null)
                return;

            mobileShiftTypes.Add(mobileShiftType);
        }

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            return MergeDocuments(ROOTNAME, mobileShiftTypes.Select(i => i.ToXDocument()).ToList());
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            return XmlUtil.CreateXml(ROOTNAME, MobileShiftType.GetDefaultXml());
        }

        #endregion
    }

    internal class MobileShiftType : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "ShiftType";

        #endregion

        #region Variables

        private readonly int id;
        private readonly string name;
        private readonly string scheduleTypeName;
        private readonly int scheduleTypeId;

        #endregion

        #region Ctor

        public MobileShiftType(MobileParam param, ShiftTypeDTO shiftType)
            : base(param)
        {
            Init();

            this.id = shiftType.ShiftTypeId;
            this.name = shiftType.Name;
            this.scheduleTypeName = shiftType.TimeScheduleTypeName;
            this.scheduleTypeId = shiftType.TimeScheduleTypeId ?? 0;
        }

        /// <summary>
        /// Used for errors
        /// </summary>
        public MobileShiftType(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            //Set default values
        }

        #endregion

        #region Public methods

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();

            elements.Add(new XElement("ShiftTypeId", id.ToString()));
            elements.Add(new XElement("Name", name));
            elements.Add(new XElement("ScheduleTypeName", scheduleTypeName));
            elements.Add(new XElement("ScheduleTypeId", scheduleTypeId));

            return CreateDocument(ROOTNAME, elements);
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            string xml =
                "<ShiftTypeId>22</ShiftTypeId>" +
                "<Name>15 min</Name>";
            return XmlUtil.CreateXml(ROOTNAME, xml);
        }

        #endregion
    }

    #endregion

    #region ScheduleType

    internal class MobileSheduleTypes : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "SheduleTypes";

        #endregion

        #region Variables

        #region Collections

        private List<MobileSheduleType> mobileSheduleTypes;

        #endregion

        #endregion

        #region Ctor

        public MobileSheduleTypes(MobileParam param, List<TimeScheduleTypeDTO> timeScheduleTypeDTOs)
            : base(param)
        {
            Init();
            AddMobileSheduleTypes(timeScheduleTypeDTOs);
        }

        /// <summary>Used for errors</summary>
        public MobileSheduleTypes(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            this.mobileSheduleTypes = new List<MobileSheduleType>();
        }

        #endregion

        #region Public methods

        public void AddMobileSheduleTypes(List<TimeScheduleTypeDTO> timeScheduleTypeDTOs)
        {
            if (timeScheduleTypeDTOs == null)
                return;


            foreach (var timeScheduleTypeDTO in timeScheduleTypeDTOs)
            {
                AddMobileSheduleType(new MobileSheduleType(this.Param, timeScheduleTypeDTO));
            }
        }

        public void AddMobileSheduleType(MobileSheduleType mobileScheduleType)
        {
            if (mobileScheduleType == null)
                return;

            mobileSheduleTypes.Add(mobileScheduleType);
        }

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            return MergeDocuments(ROOTNAME, mobileSheduleTypes.Select(i => i.ToXDocument()).ToList());
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            return XmlUtil.CreateXml(ROOTNAME, MobileSheduleType.GetDefaultXml());
        }

        #endregion
    }

    internal class MobileSheduleType : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "SheduleType";

        #endregion

        #region Variables

        #region Field values

        private int id;
        private String name;

        #endregion

        #endregion

        #region Ctor

        public MobileSheduleType(MobileParam param, TimeScheduleTypeDTO timeScheduleTypeDTO)
            : base(param)
        {
            Init();

            this.id = timeScheduleTypeDTO.TimeScheduleTypeId;
            this.name = timeScheduleTypeDTO.Name;
        }

        /// <summary>
        /// Used for errors
        /// </summary>
        public MobileSheduleType(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            //Set default values
        }

        #endregion

        #region Public methods

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();

            elements.Add(new XElement("ScheduleTypeId", id.ToString()));
            elements.Add(new XElement("Name", name));

            return CreateDocument(ROOTNAME, elements);
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            string xml =
                "<ScheduleTypeId>22</ScheduleTypeId>" +
                "<Name>Typ123</Name>";
            return XmlUtil.CreateXml(ROOTNAME, xml);
        }

        #endregion
    }

    #endregion

    #region Shifts

    internal class MobileDayOverviewAdmin : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "DayAdmin";

        #endregion

        #region Variables

        #region Field values

        private readonly bool groupedByEmployee = false;

        #region Common

        private readonly DateTime date;

        #endregion

        #region Grouped by employee

        private readonly int employeeId;
        private readonly string employeeNr;
        private readonly string firstName;
        private readonly string lastName;
        private readonly DateTime scheduleIn;
        private readonly DateTime scheduleOut;
        private readonly bool isPartTimeAbsence;
        private readonly bool isWholeDayAbsence;

        #endregion

        #region Grouped for all employees

        private readonly int availableShiftCounter;
        private readonly int wantedShiftCounter;
        private readonly int unWantedShiftCounter;
        private readonly int absenceShiftCounter;

        #endregion

        #endregion

        #endregion

        #region Ctor

        public MobileDayOverviewAdmin(MobileParam param, DateTime date, int availableShiftCounter, int wantedShiftCounter, int unWantedShiftCounter, int absenceShiftCounter)
            : base(param)
        {
            Init();

            this.groupedByEmployee = false;
            this.date = date;
            this.availableShiftCounter = availableShiftCounter;
            this.wantedShiftCounter = wantedShiftCounter;
            this.unWantedShiftCounter = unWantedShiftCounter;
            this.absenceShiftCounter = absenceShiftCounter;
        }

        public MobileDayOverviewAdmin(MobileParam param, EmployeeListSmallDTO employee, DateTime date, DateTime scheduleIn, DateTime scheduleOut, bool isPartTimeAbsence, bool isWholeDayAbsence)
            : base(param)
        {
            Init();

            this.groupedByEmployee = true;
            this.date = date;
            this.employeeId = employee.EmployeeId;
            this.employeeNr = employee.EmployeeNr;
            this.firstName = employee.FirstName;
            this.lastName = employee.LastName;
            this.scheduleIn = scheduleIn;
            this.scheduleOut = scheduleOut;
            this.isPartTimeAbsence = isPartTimeAbsence;
            this.isWholeDayAbsence = isWholeDayAbsence;
        }

        /// <summary>
        /// Used for errors
        /// </summary>
        public MobileDayOverviewAdmin(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            //Set default values
        }

        #endregion

        #region Public methods

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();

            if (this.groupedByEmployee)
            {
                elements.Add(new XElement("Date", this.date.ToShortDateString()));
                elements.Add(new XElement("EID", this.employeeId));
                elements.Add(new XElement("ENR", this.employeeNr));
                elements.Add(new XElement("FN", this.firstName));
                elements.Add(new XElement("LN", this.lastName));
                elements.Add(new XElement("SI", this.scheduleIn.TimeOfDay.ToShortTimeString()));
                elements.Add(new XElement("SO", this.scheduleOut.TimeOfDay.ToShortTimeString()));
                elements.Add(new XElement("IPTA", StringUtility.GetString(this.isPartTimeAbsence)));
                elements.Add(new XElement("IWDA", StringUtility.GetString(this.isWholeDayAbsence)));
            }
            else
            {
                elements.Add(new XElement("Date", this.date.ToShortDateString()));
                elements.Add(new XElement("ASC", this.availableShiftCounter));
                elements.Add(new XElement("ABSC", this.absenceShiftCounter));
                elements.Add(new XElement("WSC", this.wantedShiftCounter));
                elements.Add(new XElement("UWSC", this.unWantedShiftCounter));
            }

            return CreateDocument(ROOTNAME, elements);
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            string xml =
                "<Date></Date>" +
                "<ASC></ASC>" + //- Available shifts counter
                "<ABSC></ABSC>" + //- absence shifts counter
                "<WSC></WSC>" +    // - wanted shifts counter
                "<UWSC></UWSC>";    // - unwanted shifts counter

            return XmlUtil.CreateXml(ROOTNAME, xml);
        }

        #endregion
    }

    internal class MobileDayOverview : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "Day";

        #endregion

        #region Variables

        #region Field values

        private readonly DateTime date;
        private readonly int availableShiftCounter;
        private readonly int myShiftCounter;
        private readonly bool interestRequestExits;
        private readonly bool nonInterestRequestExits;
        private readonly bool absenceRequestExits;

        #endregion

        #endregion

        #region Ctor

        public MobileDayOverview(MobileParam param, DateTime date, int availableShiftCounter, int myShiftCounter, bool interestRequestExits, bool nonInterestRequestExits, bool absenceRequestExits, DateTime scheduleIn, DateTime scheduleOut, bool isWholeDayAbsence, bool isPartTimeAbsence)
            : base(param)
        {
            Init();

            this.date = date;
            this.availableShiftCounter = availableShiftCounter;
            this.myShiftCounter = myShiftCounter;
            this.interestRequestExits = interestRequestExits;
            this.nonInterestRequestExits = nonInterestRequestExits;
            this.absenceRequestExits = absenceRequestExits;
        }

        /// <summary>
        /// Used for errors
        /// </summary>
        public MobileDayOverview(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            //Set default values
        }

        #endregion

        #region Public methods

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();

            elements.Add(new XElement("Date", date.ToShortDateString()));
            elements.Add(new XElement("ASC", availableShiftCounter));
            elements.Add(new XElement("MSC", myShiftCounter));
            elements.Add(new XElement("IR", StringUtility.GetString(interestRequestExits)));
            elements.Add(new XElement("NIR", StringUtility.GetString(nonInterestRequestExits)));
            elements.Add(new XElement("AR", StringUtility.GetString(absenceRequestExits)));

            return CreateDocument(ROOTNAME, elements);
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            string xml =
                "<Date></Date>" +
                "<ASC></ASC>" + //- Available shifts counter
                "<MSC></MSC>" + //- My shifts counter
                "<IR></IR>" +    // - Interest request exits , 0 or 1
                "<AR></AR>";    // - Absence request exits, 0 or 1

            return XmlUtil.CreateXml(ROOTNAME, xml);
        }

        #endregion
    }

    internal class MobileShifts : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "Shifts";

        #endregion

        #region Variables

        #region Collections

        private List<MobileShift> shifts;

        #endregion

        #endregion

        #region Ctor

        public MobileShifts(MobileParam param)
            : base(param)
        {
            Init();

        }

        /// <summary>Used for errors</summary>
        public MobileShifts(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            this.shifts = new List<MobileShift>();
        }

        #endregion

        #region Public methods

        public void AddMobileShifts(List<TimeSchedulePlanningDayDTO> shifts, MobileShiftGUIType type, bool showQueueCount, string breakLabel, bool includeBreaks, bool showTotalBreakInfo, bool includeDescInShiftType, bool showAccountName, MobileDisplayMode displayMode, List<TimeScheduleSwapRequestDTO> employeeSwapRequests, bool isVersion26OrHigher)
        {
            var shiftsGroupedByDate = shifts.GroupBy(x => x.StartTime.Date).ToList();

            foreach (var shiftsByDate in shiftsGroupedByDate)
            {
                foreach (var shift in shiftsByDate.ToList())
                {
                    int? iniatorRequestId = null;
                    int? swapRequestId = null;
                    var completedSwap = false;

                    if (employeeSwapRequests != null && employeeSwapRequests.Any())
                    {
                        iniatorRequestId = employeeSwapRequests.FirstOrDefault(w => w.IsInitiator(shift.EmployeeId) && w.IsShiftAffectedBySwap(shift))?.TimeScheduleSwapRequestId;
                        swapRequestId = employeeSwapRequests.FirstOrDefault(w => w.IsSwapWith(shift.EmployeeId) && w.IsShiftAffectedBySwap(shift))?.TimeScheduleSwapRequestId;
                        completedSwap = employeeSwapRequests.Any(w=> (w.TimeScheduleSwapRequestId == iniatorRequestId || w.TimeScheduleSwapRequestId == swapRequestId) && w.Status == TermGroup_TimeScheduleSwapRequestStatus.Done);
                    }
                    var mobileShift = new MobileShift(this.Param, shift, type, showQueueCount, includeDescInShiftType, showAccountName, displayMode, iniatorRequestId, swapRequestId, isVersion26OrHigher);
                    mobileShift.HasCompletedSwapShift = completedSwap;

                    AddMobileShift(mobileShift);
                }

                if (includeBreaks)
                {
                    var shift = shiftsByDate.FirstOrDefault();
                    if (showTotalBreakInfo)
                    {
                        string label = shift.GetBreaks().Select(x => x.BreakMinutes).Sum(x => x) + " min " + breakLabel;
                        //set DATETIME_MAXVALUE so that the totalbreak info gets ordered last
                        AddMobileShift(this.Param, 0, CalendarUtility.DATETIME_MAXVALUE, CalendarUtility.DATETIME_MAXVALUE, shift.ActualDate, shift.EmployeeName, "", label, "", false, false, true, false, shift.EmployeeId, null, type, false, true, false, false, false, false);
                    }
                    else
                    {
                        //Only add breaks once!
                        if (shift.Break1Id != 0 && !this.shifts.Any(w => w.id == shift.Break1Id))
                            AddMobileShift(this.Param, shift.Break1Id, shift.Break1StartTime, shift.Break1StartTime.AddMinutes(shift.Break1Minutes), shift.ActualDate, shift.EmployeeName, "", breakLabel, "", false, false, true, false, shift.EmployeeId, null, type, false, false, false, false, false, shiftsByDate.ToList().IsAssociatedShiftLended(shift.Break1Id));
                        if (shift.Break2Id != 0 && !this.shifts.Any(w => w.id == shift.Break2Id))
                            AddMobileShift(this.Param, shift.Break2Id, shift.Break2StartTime, shift.Break2StartTime.AddMinutes(shift.Break2Minutes), shift.ActualDate, shift.EmployeeName, "", breakLabel, "", false, false, true, false, shift.EmployeeId, null, type, false, false, false, false, false, shiftsByDate.ToList().IsAssociatedShiftLended(shift.Break2Id));
                        if (shift.Break3Id != 0 && !this.shifts.Any(w => w.id == shift.Break3Id))
                            AddMobileShift(this.Param, shift.Break3Id, shift.Break3StartTime, shift.Break3StartTime.AddMinutes(shift.Break3Minutes), shift.ActualDate, shift.EmployeeName, "", breakLabel, "", false, false, true, false, shift.EmployeeId, null, type, false, false, false, false, false, shiftsByDate.ToList().IsAssociatedShiftLended(shift.Break3Id));
                        if (shift.Break4Id != 0 && !this.shifts.Any(w => w.id == shift.Break4Id))
                            AddMobileShift(this.Param, shift.Break4Id, shift.Break4StartTime, shift.Break4StartTime.AddMinutes(shift.Break4Minutes), shift.ActualDate, shift.EmployeeName, "", breakLabel, "", false, false, true, false, shift.EmployeeId, null, type, false, false, false, false, false, shiftsByDate.ToList().IsAssociatedShiftLended(shift.Break4Id));
                    }
                }
            }
            this.shifts = this.shifts.OrderBy(x => x.employeeId).ThenBy(x => x.startTime).ToList();
        }

        public void AddMobileShift(MobileParam param, int id, DateTime startTime, DateTime stopTime, DateTime actualDate, string employeeName, string description, string shiftTypeName, string shiftTypeDesc, bool isWanted, bool isUnWanted, bool isBreak, bool isAbsence, int employeeId, int? orderId, MobileShiftGUIType type, bool showQueueCount, bool isTotalBreak, bool includeDescInShiftType, bool hasShiftRequest, bool hasShiftRequestAnswer, bool shiftIslended)
        {
            AddMobileShift(new MobileShift(this.Param, id, startTime, stopTime, actualDate, employeeName, description, shiftTypeName, shiftTypeDesc, isWanted, isUnWanted, isBreak, isAbsence, employeeId, orderId, type, showQueueCount, isTotalBreak, includeDescInShiftType, hasShiftRequest, hasShiftRequestAnswer, shiftIslended));
        }

        public void AddAggregatedShift(MobileParam param, DateTime aggStartTime, DateTime aggStopTime, DateTime actualDate, int employeeId, string employeeName, bool isPartTimeAbsence, bool isWholeDayAbsence, MobileShiftGUIType type, bool hasShiftRequest, bool hasShiftRequestAnswer)
        {
            AddMobileShift(new MobileShift(param, aggStartTime, aggStopTime, actualDate, employeeId, employeeName, isPartTimeAbsence, isWholeDayAbsence, type, hasShiftRequest, hasShiftRequestAnswer));
        }

        public void AddMobileShift(MobileShift shift)
        {
            if (shift == null)
                return;

            shifts.Add(shift);
        }

        public void SetSelectedId(List<int> ids)
        {
            foreach (var id in ids)
            {
                this.SetSelectedId(id);
            }
        }
        public void SetSelectedId(int id)
        {
            var shift = this.shifts.FirstOrDefault(x => x.id == id);
            if (shift != null)
                shift.SetSelected();
        }

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            return MergeDocuments(ROOTNAME, shifts.Select(i => i.ToXDocument()).ToList());
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            return XmlUtil.CreateXml(ROOTNAME, MobileShift.GetDefaultXml());
        }

        #endregion
    }

    internal class MobileShift : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "Shift";

        #endregion

        #region Variables

        #region Field values

        #region Aggregated

        private readonly bool isWholeDayAbsence;
        private readonly bool isPartTimeAbsence;

        #endregion

        public readonly int id;
        public readonly DateTime startTime;
        private readonly DateTime stopTime;
        public readonly int employeeId;
        public readonly string employeeName;
        private readonly DateTime actualDate;
        private readonly string shiftTypeName;
        private readonly bool isWanted;
        private readonly bool isUnWanted;
        private readonly MobileShiftGUIType type;
        private readonly int? userId;
        private readonly bool isBreak;
        public readonly bool isTotalBreak;
        private readonly bool isAbsence;
        private readonly int? orderId;
        private readonly int queueCount = 0;
        private readonly bool showQueueCount;
        private readonly string shiftTypeColor;
        private readonly string shiftInfo;
        private readonly string description;
        private readonly bool hasShiftRequest;
        private readonly bool hasShiftRequestAnswer;
        private readonly bool isVersion26OrHigher;
        private readonly string link;
        private bool selected;
        private readonly bool shiftIsLended;
        private readonly int? iniatorRequestId;
        private readonly int? swapRequestId;
        private readonly bool isOnDuty;
        private readonly bool prevDay = false;
        private readonly bool nextDay = false;
        private bool hasCompletedSwapShift;

        public bool HasCompletedSwapShift
        {
            get { return hasCompletedSwapShift; }
            set { hasCompletedSwapShift = value; }
        }
        #endregion

        #endregion

        #region Ctor

        public MobileShift(MobileParam param)
            : base(param)
        {
            Init();
        }

        public MobileShift(MobileParam param, TimeSchedulePlanningDayDTO shiftDTO, MobileShiftGUIType type, bool showQueueCount, bool includeDescInShiftType, bool showAccountName, MobileDisplayMode displayMode, int? iniatorRequestId, int? swapRequestId, bool isVersion26OrHigher)
            : base(param)
        {
            Init();

            this.type = type;
            this.id = shiftDTO.TimeScheduleTemplateBlockId;
            this.startTime = shiftDTO.StartTime;
            this.stopTime = shiftDTO.StopTime;
            this.userId = shiftDTO.UserId;
            this.employeeId = shiftDTO.EmployeeId;
            this.employeeName = shiftDTO.EmployeeName;
            this.isWanted = shiftDTO.IamInQueue;
            this.isUnWanted = shiftDTO.ShiftUserStatus == TermGroup_TimeScheduleTemplateBlockShiftUserStatus.Unwanted;
            this.isBreak = false;
            this.isAbsence = shiftDTO.TimeDeviationCauseId.HasValue;
            this.actualDate = shiftDTO.ActualDate;
            this.orderId = (shiftDTO.Order != null) ? shiftDTO.Order.OrderId : (int?)null;
            this.queueCount = shiftDTO.NbrOfWantedInQueue;
            this.showQueueCount = showQueueCount;
            this.hasShiftRequest = shiftDTO.HasShiftRequest;
            this.hasShiftRequestAnswer = shiftDTO.HasShiftRequestAnswer;
            this.shiftTypeColor = shiftDTO.ShiftTypeColor;
            this.shiftInfo = shiftDTO.SwapShiftInfo;
            this.link = shiftDTO.Link?.ToString();
            this.isVersion26OrHigher = isVersion26OrHigher;
            this.shiftIsLended = shiftDTO.IsLended;
            this.iniatorRequestId = iniatorRequestId;
            this.swapRequestId = swapRequestId;
            this.isOnDuty = shiftDTO.IsOnDuty();

            if (this.startTime.Date < this.actualDate) 
                prevDay = true;
            else if (this.startTime.Date > this.actualDate)
                nextDay = true;


            this.shiftTypeName = String.Empty;
            if (shiftDTO.Order != null)
                this.shiftTypeName += String.Format("{0} {1} ", shiftDTO.Order.OrderNr, shiftDTO.Order.CustomerName);
            this.shiftTypeName += shiftDTO.ShiftTypeName;
            if (shiftDTO.TimeDeviationCauseId.HasValue && !string.IsNullOrEmpty(shiftDTO.TimeDeviationCauseName) && (displayMode == MobileDisplayMode.Admin || type == MobileShiftGUIType.MyShifts || type == MobileShiftGUIType.MyShiftsNew || type == MobileShiftGUIType.MyTemplateShifts))
                this.shiftTypeName = String.Format("{0} ({1})", shiftDTO.TimeDeviationCauseName, shiftTypeName);

            if (includeDescInShiftType)
            {
                //older apps
                if (!String.IsNullOrEmpty(shiftDTO.Description))
                    this.shiftTypeName += String.Format(" - {0}", shiftDTO.Description);
                if (!String.IsNullOrEmpty(shiftDTO.ShiftTypeDesc))
                    this.shiftTypeName += String.Format(" ({0})", shiftDTO.ShiftTypeDesc);
            }
            else
            {
                if (showAccountName)
                {
                    if (!shiftDTO.AccountName.IsNullOrEmpty())
                        this.description = shiftDTO.AccountName;

                    if (!String.IsNullOrEmpty(shiftDTO.Description))
                        this.description += String.Format(" ({0})", shiftDTO.Description);
                }
                else
                {
                    if (!String.IsNullOrEmpty(shiftDTO.Description))
                        this.description = String.Format("{0}", shiftDTO.Description);
                }
            }
        }

        public MobileShift(MobileParam param, int id, DateTime startTime, DateTime stopTime, DateTime actualDate, string employeeName, string description, string shiftTypeName, string shiftTypeDesc, bool isWanted, bool isUnWanted, bool isBreak, bool isAbsence, int employeeId, int? orderId, MobileShiftGUIType type, bool showQueueCount, bool isTotalBreak, bool includeDescInShiftType, bool hasShiftRequest, bool hasShiftRequestAnswer, bool shiftIslended)
            : base(param)
        {
            Init();

            this.type = type;
            this.id = id;
            this.startTime = startTime;
            this.stopTime = stopTime;
            this.employeeName = employeeName;
            this.shiftTypeName = shiftTypeName;

            if (includeDescInShiftType)
            {
                if (!String.IsNullOrEmpty(description))
                    this.shiftTypeName += String.Format(" - {0}", description);
                if (!String.IsNullOrEmpty(shiftTypeDesc))
                    this.shiftTypeName += String.Format(" ({0})", shiftTypeDesc);
            }
            else
            {
                if (!String.IsNullOrEmpty(description))
                    this.description = String.Format("{0}", description);
                //if (!String.IsNullOrEmpty(shiftTypeDesc))
                //    this.description += String.Format(" ({0})", shiftTypeDesc);
            }


            this.isWanted = isWanted;
            this.isUnWanted = isUnWanted;
            this.isBreak = isBreak;
            this.isTotalBreak = isTotalBreak;
            this.isAbsence = isAbsence;
            this.employeeId = employeeId;
            this.actualDate = actualDate;
            this.orderId = orderId;
            this.queueCount = 0;
            this.showQueueCount = showQueueCount;
            this.hasShiftRequest = hasShiftRequest;
            this.hasShiftRequestAnswer = hasShiftRequestAnswer;
            this.shiftIsLended = shiftIslended;

            if (this.startTime.Date < this.actualDate)
                prevDay = true;
            else if (this.startTime.Date > this.actualDate)
                nextDay = true;
        }

        public MobileShift(MobileParam param, DateTime aggStartTime, DateTime aggStopTime, DateTime actualDate, int employeeId, string employeeName, bool isPartTimeAbsence, bool isWholeDayAbsence, MobileShiftGUIType type, bool hasShiftRequest, bool hasShiftRequestAnswer)
            : base(param)
        {
            Init();

            this.type = type;
            this.startTime = aggStartTime;
            this.stopTime = aggStopTime;
            this.employeeName = employeeName;
            this.employeeId = employeeId;
            this.actualDate = actualDate;
            this.isPartTimeAbsence = isPartTimeAbsence;
            this.isWholeDayAbsence = isWholeDayAbsence;
            this.hasShiftRequest = hasShiftRequest;
            this.hasShiftRequestAnswer = hasShiftRequestAnswer;
        }

        /// <summary>
        /// Used for errors
        /// </summary>
        public MobileShift(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            //Set default values
        }

        #endregion

        #region Public methods

        public void SetSelected()
        {
            this.selected = true;
        }

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();

            if (this.type == MobileShiftGUIType.MyShiftFlow)
            {
                elements.Add(new XElement("ActualDate", this.actualDate.ToShortDateString()));
                elements.Add(new XElement("Time", this.startTime.TimeOfDay.ToShortTimeString() + " - " + this.stopTime.TimeOfDay.ToShortTimeString()));
                elements.Add(new XElement("EmpId", this.employeeId));
                elements.Add(new XElement("IsPTAbsence", StringUtility.GetString(this.isPartTimeAbsence)));
                elements.Add(new XElement("IsWDAbsence", StringUtility.GetString(this.isWholeDayAbsence)));
                elements.Add(new XElement("IOD", StringUtility.GetString(this.isOnDuty)));
            }
            else if (this.type == MobileShiftGUIType.MyTemplateShifts)
            {
                elements.Add(new XElement("Id", this.id));
                elements.Add(new XElement("ActualDate", this.actualDate.ToShortDateString()));
                elements.Add(new XElement("EmpId", this.employeeId));
                elements.Add(new XElement("Time", this.startTime.TimeOfDay.ToShortTimeString() + " - " + this.stopTime.TimeOfDay.ToShortTimeString()));
                elements.Add(new XElement("STN", this.shiftTypeName));
                elements.Add(new XElement("IsAbsence", StringUtility.GetString(this.isAbsence)));
                elements.Add(new XElement("IsBreak", StringUtility.GetString(this.isBreak)));
                elements.Add(new XElement("ITB", StringUtility.GetString(this.isTotalBreak)));
                elements.Add(new XElement("STC", !string.IsNullOrEmpty(this.shiftTypeColor) ? this.shiftTypeColor : "#707070"));
                elements.Add(new XElement("IOD", StringUtility.GetString(this.isOnDuty)));

                if (!string.IsNullOrEmpty(this.description))
                    elements.Add(new XElement("Desc", this.description));
            }
            else if (type == MobileShiftGUIType.MyShiftsNew)
            {
                elements.Add(new XElement("Id", this.id));
                elements.Add(new XElement("ActualDate", this.actualDate.ToShortDateString()));
                elements.Add(new XElement("EmpId", this.employeeId));
                elements.Add(new XElement("Time", this.startTime.TimeOfDay.ToShortTimeString() + " - " + this.stopTime.TimeOfDay.ToShortTimeString()));
                elements.Add(new XElement("STN", shiftTypeName));
                elements.Add(new XElement("IsAbsence", StringUtility.GetString(this.isAbsence)));
                elements.Add(new XElement("IsBreak", StringUtility.GetString(this.isBreak)));
                elements.Add(new XElement("ITB", StringUtility.GetString(this.isTotalBreak)));
                elements.Add(new XElement("IUW", StringUtility.GetString(this.isUnWanted)));
                elements.Add(new XElement("STC", !string.IsNullOrEmpty(this.shiftTypeColor) ? this.shiftTypeColor : "#707070"));
                elements.Add(new XElement("IOD", StringUtility.GetString(this.isOnDuty)));
                elements.Add(new XElement("PrevDay", StringUtility.GetString(prevDay)));
                elements.Add(new XElement("NextDay", StringUtility.GetString(nextDay)));

                if (!string.IsNullOrEmpty(this.link))
                    elements.Add(new XElement("Link", this.link));

                if (this.orderId.HasValue)
                    elements.Add(new XElement("OrderId", this.orderId.Value)); //is used for to open an order

                if (!string.IsNullOrEmpty(this.description))
                    elements.Add(new XElement("Desc", this.description));

                if (iniatorRequestId.HasValue)
                    elements.Add(new XElement("HSSIId", this.iniatorRequestId));
                if (swapRequestId.HasValue)
                    elements.Add(new XElement("HSSRId", this.swapRequestId));
                if (hasCompletedSwapShift)
                    elements.Add(new XElement("HSSC", StringUtility.GetString(this.hasCompletedSwapShift)));
            }
            else if (type == MobileShiftGUIType.AvailableShiftsNew)
            {
                elements.Add(new XElement("Id", this.id));
                elements.Add(new XElement("ActualDate", this.actualDate.ToShortDateString()));
                elements.Add(new XElement("EmpId", this.employeeId));
                elements.Add(new XElement("Time", this.startTime.TimeOfDay.ToShortTimeString() + " - " + this.stopTime.TimeOfDay.ToShortTimeString()));
                elements.Add(new XElement("STN", this.shiftTypeName));
                elements.Add(new XElement("IsAbsence", StringUtility.GetString(this.isAbsence)));
                elements.Add(new XElement("IsBreak", StringUtility.GetString(this.isBreak)));
                elements.Add(new XElement("ITB", StringUtility.GetString(this.isTotalBreak)));
                elements.Add(new XElement("IW", StringUtility.GetString(this.isWanted)));
                elements.Add(new XElement("AS", StringUtility.GetString(true)));
                elements.Add(new XElement("STC", !string.IsNullOrEmpty(this.shiftTypeColor) ? this.shiftTypeColor : "#707070"));
                elements.Add(new XElement("HSR", StringUtility.GetString(this.hasShiftRequest)));
                elements.Add(new XElement("HSRA", StringUtility.GetString(this.hasShiftRequestAnswer)));
                elements.Add(new XElement("IsLended", StringUtility.GetString(this.shiftIsLended)));
                elements.Add(new XElement("IOD", StringUtility.GetString(this.isOnDuty)));
                elements.Add(new XElement("PrevDay", StringUtility.GetString(prevDay)));
                elements.Add(new XElement("NextDay", StringUtility.GetString(nextDay)));

                if (!string.IsNullOrEmpty(this.link))
                    elements.Add(new XElement("Link", this.link));

                if (this.orderId.HasValue)
                    elements.Add(new XElement("OrderId", this.orderId.Value)); //is used for to open an order

                if (this.queueCount > 0 && this.showQueueCount)
                    elements.Add(new XElement("QC", this.queueCount));

                if (!string.IsNullOrEmpty(this.description))
                    elements.Add(new XElement("Desc", this.description));

            }
            else if (type == MobileShiftGUIType.OtherShiftsNew)
            {
                elements.Add(new XElement("Id", this.id));
                elements.Add(new XElement("ActualDate", this.actualDate.ToShortDateString()));
                elements.Add(new XElement("EmpId", this.employeeId));
                elements.Add(new XElement("Time", this.startTime.TimeOfDay.ToShortTimeString() + " - " + this.stopTime.TimeOfDay.ToShortTimeString()));
                elements.Add(new XElement("STN", this.shiftTypeName));
                elements.Add(new XElement("IUW", StringUtility.GetString(this.isUnWanted)));
                elements.Add(new XElement("IsAbsence", StringUtility.GetString(this.isAbsence)));
                elements.Add(new XElement("IsBreak", StringUtility.GetString(this.isBreak)));
                elements.Add(new XElement("ITB", StringUtility.GetString(this.isTotalBreak)));
                elements.Add(new XElement("STC", !string.IsNullOrEmpty(this.shiftTypeColor) ? this.shiftTypeColor : "#707070"));
                elements.Add(new XElement("HSR", StringUtility.GetString(this.hasShiftRequest)));
                elements.Add(new XElement("HSRA", StringUtility.GetString(this.hasShiftRequestAnswer)));
                elements.Add(new XElement("IsLended", StringUtility.GetString(this.shiftIsLended)));
                elements.Add(new XElement("IOD", StringUtility.GetString(this.isOnDuty)));
                elements.Add(new XElement("PrevDay", StringUtility.GetString(prevDay)));
                elements.Add(new XElement("NextDay", StringUtility.GetString(nextDay)));

                if (!string.IsNullOrEmpty(this.link))
                    elements.Add(new XElement("Link", this.link));

                if (this.orderId.HasValue)
                    elements.Add(new XElement("OrderId", this.orderId.Value)); //is used for to open an order         

                if (this.queueCount > 0 && this.showQueueCount)
                    elements.Add(new XElement("QC", this.queueCount));

                if (!string.IsNullOrEmpty(this.description))
                    elements.Add(new XElement("Desc", this.description));

                if (this.isVersion26OrHigher)
                    elements.Add(new XElement("IW", StringUtility.GetString(this.isWanted)));

                if (iniatorRequestId.HasValue)
                    elements.Add(new XElement("HSSIId", this.iniatorRequestId));
                if (swapRequestId.HasValue)
                    elements.Add(new XElement("HSSRId", this.swapRequestId));
                if (hasCompletedSwapShift)
                    elements.Add(new XElement("HSSC", StringUtility.GetString(this.hasCompletedSwapShift)));
            }
            else if (type == MobileShiftGUIType.SwapShift)
            {
                elements.Add(new XElement("Id", this.id));
                elements.Add(new XElement("EmpId", this.employeeId));
                elements.Add(new XElement("Info", this.shiftInfo));
                if (this.selected)
                    elements.Add(new XElement("Selected", 1));
            }
            else
            {
                elements.Add(new XElement("Id", this.id));
                elements.Add(new XElement("ActualDate", this.actualDate.ToShortDateString()));
                elements.Add(new XElement("Date", this.isBreak ? "" : this.startTime.Date.ToShortDateString()));
                elements.Add(new XElement("Time", this.startTime.TimeOfDay.ToShortTimeString() + " - " + this.stopTime.TimeOfDay.ToShortTimeString()));
                elements.Add(new XElement("EN", this.employeeName));
                elements.Add(new XElement("STN", this.shiftTypeName));
                elements.Add(new XElement("IW", StringUtility.GetString(this.isWanted)));
                elements.Add(new XElement("IUW", StringUtility.GetString(this.isUnWanted)));
                elements.Add(new XElement("EmpId", this.employeeId));
                elements.Add(new XElement("IsAbsence", StringUtility.GetString(this.isAbsence)));
                elements.Add(new XElement("IsBreak", StringUtility.GetString(this.isBreak)));
                elements.Add(new XElement("IOD", StringUtility.GetString(this.isOnDuty)));

                if (this.userId.HasValue)
                    elements.Add(new XElement("UserId", this.userId.Value)); //is used for to send xemail to other collegeos

                if (this.orderId.HasValue)
                    elements.Add(new XElement("OrderId", this.orderId.Value)); //is used for to open an order

                if (this.queueCount > 0 && this.showQueueCount)
                    elements.Add(new XElement("QC", this.queueCount));
            }

            return CreateDocument(ROOTNAME, elements);
        }

        public override XDocument ToXDocument(MobileTask task)
        {
            if (base.Failed)
                return MobileMessages.GetErrorMessageDocument(base.ErrorMessage);

            bool result = base.GetTaskResult(task);
            switch (task)
            {
                case MobileTask.SetShiftAsUnWanted:
                case MobileTask.SetShiftAsWanted:
                case MobileTask.UpdateShiftSetUndoUnWanted:
                case MobileTask.UpdateShiftSetUndoWanted:
                case MobileTask.AssignAvailableShift:
                    return MobileMessages.GetSuccessDocument(result);
                default:
                    return base.ToXDocument(task);
            }
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            string xml =
                "<Id></Id>" +
                "<Date></Date>" +
                "<Time></Time>" +
                "<EN></EN>" +    // - Employee name
                "<STN></STN>" +    // - Shift type name
                "<IW></IW>" +   //Is wanted
                "<IUW></IUW>";  //Is unwanted

            return XmlUtil.CreateXml(ROOTNAME, xml);
        }

        #endregion
    }

    internal class MobileExtendedShifts : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "ExtendedShifts";

        #endregion

        #region Variables

        #region Collections

        private List<MobileExtendedShift> shifts;

        #endregion

        #endregion

        #region Ctor

        public MobileExtendedShifts(MobileParam param)
            : base(param)
        {
            Init();

        }

        /// <summary>Used for errors</summary>
        public MobileExtendedShifts(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            this.shifts = new List<MobileExtendedShift>();
        }

        #endregion

        #region Public methods

        public void AddMobileExtendedShifts(List<TimeSchedulePlanningDayDTO> shifts, bool setAbsenceStartAndStop = false)
        {
            foreach (var shift in shifts)
            {
                AddMobileExtendedShift(new MobileExtendedShift(this.Param, shift, setAbsenceStartAndStop));
            }
        }

        public void AddMobileExtendedShiftBreak(MobileParam param, int id, DateTime startTime, DateTime stopTime, bool belongsToPreviousDay, int employeeId, string employeeName, int timeCodeId, string timeCodeName, int scheduleTypeId, string scheduleTypeName, bool isAbsence, bool isLended)
        {
            AddMobileExtendedShift(new MobileExtendedShift(param, id, startTime, stopTime, null, null, belongsToPreviousDay, employeeId, employeeName, timeCodeId, timeCodeName, scheduleTypeId, scheduleTypeName, isAbsence, true, isLended));
        }

        public void AddMobileExtendedShift(MobileExtendedShift shift)
        {
            if (shift == null)
                return;

            shifts.Add(shift);

            shifts = shifts.OrderBy(x => x.startTime).ToList();
        }
        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            return MergeDocuments(ROOTNAME, shifts.Select(i => i.ToXDocument()).ToList());
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            return XmlUtil.CreateXml(ROOTNAME, MobileExtendedShift.GetDefaultXml());
        }

        #endregion
    }

    internal class MobileExtendedShift : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "ExtendedShift";

        #endregion

        #region Variables

        private readonly int id;
        public readonly DateTime startTime;
        private readonly DateTime stopTime;
        private readonly DateTime? absenceStartTime;
        private readonly DateTime? absenceStopTime;
        private readonly int employeeId;
        private readonly string employeeName;
        private readonly int shiftTypeId;
        private readonly string shiftTypeName;
        private readonly string shiftTypeScheduleTypeName;
        private readonly int scheduleTypeId;
        private readonly string scheduleTypeName;
        private readonly int timeCodeId;
        private readonly string timeCodeName;
        private readonly bool isBreak;
        private readonly bool isAbsence;
        private readonly bool belongsToPreviousDay;
        private readonly int approvalTypeId;
        private readonly string shiftTypeColor;
        private readonly int? accountId;
        private readonly bool isExtraShift;
        private readonly bool isSubstituteShift;
        private readonly bool isLended;
        private readonly int type;

        #endregion

        #region Ctor

        public MobileExtendedShift(MobileParam param)
            : base(param)
        {
            Init();
        }
        /// <summary>
        /// Used for errors
        /// </summary>
        public MobileExtendedShift(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        public MobileExtendedShift(MobileParam param, TimeSchedulePlanningDayDTO shiftDTO, bool setAbsenceStartAndStop = false)
            : base(param)
        {
            Init();

            this.id = shiftDTO.TimeScheduleTemplateBlockId;
            this.startTime = shiftDTO.StartTime;
            this.stopTime = shiftDTO.StopTime;
            if (setAbsenceStartAndStop)
            {
                this.absenceStartTime = shiftDTO.AbsenceStartTime;
                this.absenceStopTime = shiftDTO.AbsenceStopTime;
            }
            this.belongsToPreviousDay = shiftDTO.BelongsToPreviousDay;
            this.employeeId = shiftDTO.EmployeeId;
            this.employeeName = shiftDTO.EmployeeName;
            this.shiftTypeId = shiftDTO.ShiftTypeId;
            this.shiftTypeName = shiftDTO.ShiftTypeName;
            this.shiftTypeScheduleTypeName = shiftDTO.ShiftTypeTimeScheduleTypeName;
            this.timeCodeId = 0;
            this.timeCodeName = "";
            this.isBreak = false;
            this.isAbsence = shiftDTO.TimeDeviationCauseId.HasValue;
            this.scheduleTypeId = shiftDTO.TimeScheduleTypeId;
            this.scheduleTypeName = shiftDTO.TimeScheduleTypeName;
            this.approvalTypeId = shiftDTO.ApprovalTypeId;
            this.shiftTypeColor = shiftDTO.ShiftTypeColor;
            this.accountId = shiftDTO.AccountId;
            this.isExtraShift = shiftDTO.ExtraShift;
            this.isSubstituteShift = shiftDTO.SubstituteShift;
            this.isLended = shiftDTO.IsLended;
            this.type = (int)shiftDTO.Type;
        }

        public MobileExtendedShift(MobileParam param, int id, DateTime startTime, DateTime stopTime, DateTime? absenceStartTime, DateTime? absenceStopTime, bool belongsToPreviousDay, int employeeId, string employeeName, int timeCodeId, string timeCodeName, int scheduleTypeId, string scheduleTypeName, bool isAbsence, bool isBreak, bool isLended)
            : base(param)
        {
            this.id = id;
            this.startTime = startTime;
            this.stopTime = stopTime;
            this.absenceStartTime = absenceStartTime;
            this.absenceStopTime = absenceStopTime;
            this.employeeId = employeeId;
            this.employeeName = employeeName;
            this.timeCodeId = timeCodeId;
            this.timeCodeName = timeCodeName;
            this.isBreak = isBreak;
            this.isAbsence = isAbsence;
            this.scheduleTypeId = scheduleTypeId;
            this.scheduleTypeName = scheduleTypeName;
            this.belongsToPreviousDay = belongsToPreviousDay;
            this.approvalTypeId = (int)TermGroup_YesNo.Unknown;
            this.isLended = isLended;
        }
        private void Init()
        {
            //Set default values
        }

        #endregion

        #region Public methods

        public XElement ToXElement()
        {
            XElement element = new XElement(ROOTNAME);
            foreach (var item in this.GetElements())
            {
                element.Add(item);
            }

            return element;
        }

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();

            elements.AddRange(this.GetElements());

            return CreateDocument(ROOTNAME, elements);
        }

        public override XDocument ToXDocument(MobileTask task)
        {
            if (base.Failed)
                return MobileMessages.GetErrorMessageDocument(base.ErrorMessage);

            bool result = base.GetTaskResult(task);
            switch (task)
            {
                case MobileTask.SaveShifts:
                case MobileTask.DeleteShift:
                case MobileTask.SaveAbsencePlanning:
                    return MobileMessages.GetSuccessDocument(result);
                default:
                    return base.ToXDocument(task);
            }
        }

        #endregion

        #region Private Methods

        private List<XElement> GetElements()
        {
            var elements = new List<XElement>();

            elements.Add(new XElement("Id", this.id));
            elements.Add(new XElement("StartTime", StringUtility.GetSwedishFormattedDateTime(startTime)));
            elements.Add(new XElement("StopTime", StringUtility.GetSwedishFormattedDateTime(stopTime)));

            if (this.absenceStartTime.HasValue)
                elements.Add(new XElement("AbsenceStartTime", StringUtility.GetSwedishFormattedDateTime(absenceStartTime.Value)));
            if (this.absenceStopTime.HasValue)
                elements.Add(new XElement("AbsenceStopTime", StringUtility.GetSwedishFormattedDateTime(absenceStopTime.Value)));

            elements.Add(new XElement("StartsAfterMidnight", StringUtility.GetString(this.belongsToPreviousDay)));
            elements.Add(new XElement("EmployeeId", this.employeeId));
            elements.Add(new XElement("EmployeeName", this.employeeName));
            elements.Add(new XElement("ShiftTypeId", this.shiftTypeId));
            elements.Add(new XElement("ShiftTypeName", this.shiftTypeName));
            elements.Add(new XElement("ShiftTypeScheduleTypeName", this.shiftTypeScheduleTypeName));
            elements.Add(new XElement("ScheduleTypeId", this.scheduleTypeId));
            elements.Add(new XElement("ScheduleTypeName", this.scheduleTypeName));
            elements.Add(new XElement("TimeCodeBreakId", this.timeCodeId));
            elements.Add(new XElement("TimeCodeBreakName", this.timeCodeName));
            elements.Add(new XElement("IsBreak", StringUtility.GetString(this.isBreak)));
            elements.Add(new XElement("IsAbsence", StringUtility.GetString(this.isAbsence)));
            elements.Add(new XElement("ApprovalTypeId", this.approvalTypeId));
            elements.Add(new XElement("IsExtraShift", StringUtility.GetString(this.isExtraShift)));
            elements.Add(new XElement("IsSubStituteShift", StringUtility.GetString(this.isSubstituteShift)));
            elements.Add(new XElement("IsLended", StringUtility.GetString(this.isLended)));
            elements.Add(new XElement("Type", this.type));

            if (this.accountId.HasValue)
                elements.Add(new XElement("AccountId", this.accountId.Value));

            if (!string.IsNullOrEmpty(this.shiftTypeColor))
                elements.Add(new XElement("STC", this.shiftTypeColor));
            else
                elements.Add(new XElement("STC", "#707070"));

            return elements;
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            string xml = ""
                ;

            return XmlUtil.CreateXml(ROOTNAME, xml);
        }

        #endregion
    }

    #endregion

    #region SwapShift
    internal class MobileSwapScheduleShifts : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "Shifts";

        #endregion

        #region Variables

        #region Collections

        private List<MobileSwapScheudleShift> shifts;

        #endregion

        #endregion

        #region Ctor

        public MobileSwapScheduleShifts(MobileParam param)
            : base(param)
        {
            Init();

        }

        public MobileSwapScheduleShifts(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            this.shifts = new List<MobileSwapScheudleShift>();
        }

        #endregion

        #region Public methods

        public void AddMobileSwapScheudleShifts(List<TimeScheduleSwapRequestRowDTO> shifts)
        {
            foreach (var shift in shifts)
            {
                AddMobileSwapScheudleShift(shift.Date, shift.ShiftsInfo, shift.SkillsMatch);
            }
        }

        public void AddMobileSwapScheudleShift(DateTime date, string shiftInfo, bool skillsMatch)
        {
            AddMobileSwapScheudleShift(new MobileSwapScheudleShift(this.Param, date, shiftInfo, skillsMatch));
        }
        public void AddMobileSwapScheudleShift(MobileSwapScheudleShift shift)
        {
            if (shift == null)
                return;

            shifts.Add(shift);
        }
        #endregion

        #region Overrided methods

        public XElement ToXElement(string prefix, bool admin, bool includeValidSkill)
        {
            XElement element = new XElement(prefix + "Shifts");
            foreach (var shift in this.shifts)
            {
                element.Add(shift.ToXElement(admin, includeValidSkill));
            }

            return element;
        }
        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            return XmlUtil.CreateXml(ROOTNAME, "");
        }

        #endregion
    }
    internal class MobileSwapScheudleShift : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "Shift";

        #endregion

        #region Variables

        private readonly DateTime date;
        private readonly string shiftInfo;
        private readonly bool skillsMatch;

        #region Collections

        #endregion

        #endregion

        #region Ctor

        public MobileSwapScheudleShift(MobileParam param)
            : base(param)
        {
            Init();

        }

        /// <summary>Used for errors</summary>
        public MobileSwapScheudleShift(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }
        public MobileSwapScheudleShift(MobileParam param, DateTime date, string shiftInfo, bool skillsMatch) :
            base(param)

        {
            Init();

            this.date = date;
            this.shiftInfo = shiftInfo;
            this.skillsMatch = skillsMatch;
        }

        private void Init()
        {
            //Set default values
        }

        #endregion

        #region Public methods

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();

            return CreateDocument(ROOTNAME, elements);

        }

        #endregion

        #region Static methods
        public XElement ToXElement(bool admin, bool includeValidSkill)
        {
            var rootElement = new XElement(ROOTNAME);

            rootElement.Add(new XElement("Date", date));
            rootElement.Add(new XElement("ShiftInfo", GetTextOrCDATA(shiftInfo)));

            if (admin && includeValidSkill)
                rootElement.Add(new XElement("VSkill", StringUtility.GetString(skillsMatch)));

            return rootElement;
        }
        public static string GetDefaultXml()
        {
            return XmlUtil.CreateXml(ROOTNAME, "");
        }

        #endregion
    }
    internal class MobileScheduleSwapApproveView : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "ScheduleSwapApproveView";

        #endregion

        #region Variables

        private readonly int initiatorEmployeeId;
        private readonly string initiatorEmployeeName;
        private readonly string initiatorEmployeeNumber;
        private readonly int swapWithEmployeeId;
        private readonly string swapWithEmployeeName;
        private readonly string swapWithEmployeeNumber;
        private readonly bool isApprovedBySwapWith;
        private readonly bool validSkills;
        private readonly string validSkillsMessage;
        private readonly bool shiftsHasChanged;
        private readonly string comment;
        private readonly TermGroup_TimeScheduleSwapRequestStatus status;
        private readonly SoeEntityState state;
        private readonly bool admin;
        private readonly bool isInitiator;
        private readonly bool differentLength;
        private readonly string differentLengthMessage;

        private MobileSwapScheduleShifts initiatorEmployeeRows;
        private MobileSwapScheduleShifts swapWithEmployeeRows;
        private MobileSwapScheduleShifts currentInitiatorEmployeeRows;
        private MobileSwapScheduleShifts currentSwapWithEmployeeRows;

        private readonly int timeScheduleSwapRequestId;
        #region Collections

        #endregion

        #endregion

        #region Ctor

        public MobileScheduleSwapApproveView(MobileParam param)
            : base(param)
        {
            Init();

        }

        /// <summary>Used for errors</summary>
        public MobileScheduleSwapApproveView(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }
        public MobileScheduleSwapApproveView(MobileParam param, int timeScheduleSwapRequestId, TimeScheduleSwapApproveViewDTO dto, MobileSwapScheduleShifts initiatorEmployeeRows, MobileSwapScheduleShifts swapWithEmployeeRows, MobileSwapScheduleShifts currentInitiatorEmployeeRows, MobileSwapScheduleShifts currentSwapWithEmployeeRows, bool admin, bool isInitiator)
            : base(param)
        {
            Init();
            this.timeScheduleSwapRequestId = timeScheduleSwapRequestId;
            this.initiatorEmployeeId = dto.InitiatorEmployeeId;
            this.initiatorEmployeeName = dto.InitiatorEmployeeName;
            this.initiatorEmployeeNumber = dto.InitiatorEmployeeNumber;
            this.swapWithEmployeeId = dto.SwapWithEmployeeId;
            this.swapWithEmployeeName = dto.SwapWithEmployeeName;
            this.swapWithEmployeeNumber = dto.SwapWithEmployeeNumber;
            this.validSkills = dto.ValidSkills;
            this.validSkillsMessage = dto.ValidSkillsMessage;
            this.comment = dto.Comment;
            this.status = dto.Status;
            this.state = dto.State;
            this.shiftsHasChanged = dto.ShiftsHasChanged;
            this.admin = admin;
            this.initiatorEmployeeRows = initiatorEmployeeRows;
            this.swapWithEmployeeRows = swapWithEmployeeRows;
            this.currentInitiatorEmployeeRows = currentInitiatorEmployeeRows;
            this.currentSwapWithEmployeeRows = currentSwapWithEmployeeRows;
            this.isInitiator = isInitiator;
            this.isApprovedBySwapWith = dto.IsSwapWithEmployeeRowsApprovedByEmployee();
            this.differentLength = dto.DifferentLength;
            this.differentLengthMessage = dto.DifferentLengthMessage;
        }
        private void Init()
        {
            this.initiatorEmployeeRows = new MobileSwapScheduleShifts(base.Param);
            this.swapWithEmployeeRows = new MobileSwapScheduleShifts(base.Param);
            this.currentInitiatorEmployeeRows = new MobileSwapScheduleShifts(base.Param);
            this.currentSwapWithEmployeeRows = new MobileSwapScheduleShifts(base.Param);
        }

        #endregion

        #region Public methods

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();

            elements.Add(new XElement("Id", timeScheduleSwapRequestId));
            elements.Add(new XElement("IEmpId", initiatorEmployeeId));
            elements.Add(new XElement("IEmpName", GetTextOrCDATA(initiatorEmployeeName)));
            elements.Add(new XElement("IEmpNr", GetTextOrCDATA(initiatorEmployeeNumber)));
            elements.Add(new XElement("SEmpId", swapWithEmployeeId));
            elements.Add(new XElement("SEmpName", GetTextOrCDATA(swapWithEmployeeName)));
            elements.Add(new XElement("SEmpNr", GetTextOrCDATA(swapWithEmployeeNumber)));
            elements.Add(new XElement("SApproved", StringUtility.GetString(isApprovedBySwapWith)));
            if (admin)
            {
                elements.Add(new XElement("DiffLength", StringUtility.GetString(differentLength)));
                elements.Add(new XElement("DiffLengthMessage", GetTextOrCDATA(differentLengthMessage)));
                elements.Add(new XElement("VSkill", StringUtility.GetString(validSkills)));
                elements.Add(new XElement("VSkillMessage", GetTextOrCDATA(validSkillsMessage)));
            }
            elements.Add(new XElement("IsInitiator", StringUtility.GetString(isInitiator)));
            elements.Add(new XElement("IsAdmin", StringUtility.GetString(admin)));
            elements.Add(new XElement("Comment", GetTextOrCDATA(comment)));
            elements.Add(new XElement("Status", status));
            elements.Add(new XElement("State", state));
            elements.Add(new XElement("HasChanged", StringUtility.GetString(shiftsHasChanged)));

            elements.Add(this.initiatorEmployeeRows.ToXElement("I", admin, false));
            elements.Add(this.swapWithEmployeeRows.ToXElement("S", admin, false));
            elements.Add(this.currentInitiatorEmployeeRows.ToXElement("CI", admin, true));
            elements.Add(this.currentSwapWithEmployeeRows.ToXElement("CS", admin, true));

            return CreateDocument(ROOTNAME, elements);
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            return XmlUtil.CreateXml(ROOTNAME, "");
        }

        #endregion
    }
    #endregion

    #region ShiftQueue

    internal class MobileShiftQueueList : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "QueueList";

        #endregion

        #region Variables

        #region Collections

        private List<MobileShiftQueueItem> mobileShiftQueueItems;

        #endregion

        #endregion

        #region Ctor

        public MobileShiftQueueList(MobileParam param, List<TimeScheduleShiftQueueDTO> queueItems, bool sortByLas, bool showEmploymentDays, bool showWantExtraShifts)
            : base(param)
        {
            Init();
            AddMobileShiftQueueList(queueItems, sortByLas, showEmploymentDays, showWantExtraShifts);
        }

        /// <summary>Used for errors</summary>
        public MobileShiftQueueList(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            this.mobileShiftQueueItems = new List<MobileShiftQueueItem>();
        }

        #endregion

        #region Public methods

        public void AddMobileShiftQueueList(List<TimeScheduleShiftQueueDTO> queueItems, bool sortByLas, bool showEmploymentDays, bool showWantExtraShifts)
        {
            if (queueItems == null)
                return;

            foreach (var queueItem in queueItems)
            {
                AddMobileShiftQueueItem(new MobileShiftQueueItem(this.Param, queueItem, sortByLas, showEmploymentDays, showWantExtraShifts));
            }
        }

        public void AddMobileShiftQueueItem(MobileShiftQueueItem mobileShiftQueueItem)
        {
            if (mobileShiftQueueItem == null)
                return;

            mobileShiftQueueItems.Add(mobileShiftQueueItem);
        }

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            return MergeDocuments(ROOTNAME, mobileShiftQueueItems.Select(i => i.ToXDocument()).ToList());
        }

        #endregion        
    }

    internal class MobileShiftQueueItem : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "QueueItem";

        #endregion

        #region Variables

        readonly TimeScheduleShiftQueueDTO queueItem;
        readonly bool sortByLas;
        readonly bool showEmploymentDays;
        readonly bool showWantExtraShifts;

        #endregion

        #region Ctor

        public MobileShiftQueueItem(MobileParam param, TimeScheduleShiftQueueDTO queueItem, bool sortByLas, bool showEmploymentDays, bool showWantExtraShifts)
            : base(param)
        {
            Init();
            this.sortByLas = sortByLas;
            this.showEmploymentDays = showEmploymentDays;
            this.showWantExtraShifts = showWantExtraShifts;
            this.queueItem = queueItem;
        }

        /// <summary>
        /// Used for errors
        /// </summary>
        public MobileShiftQueueItem(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            //Set default values
        }

        #endregion

        #region Public methods

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();

            elements.Add(new XElement("Type", this.queueItem.TypeName));
            elements.Add(new XElement("Sort", this.queueItem.Sort));
            elements.Add(new XElement("EmpId", this.queueItem.EmployeeId));
            elements.Add(new XElement("EmpName", this.queueItem.EmployeeName + ((this.sortByLas && !showEmploymentDays) ? " (Anställd " + this.queueItem.EmploymentDays + " dagar)" : "")));
            elements.Add(new XElement("Date", this.queueItem.Date.ToShortDateShortTimeString()));
            if (this.sortByLas && showEmploymentDays)
                elements.Add(new XElement("EmpDays", this.queueItem.EmploymentDays));

            elements.Add(new XElement("SWES", StringUtility.GetString(showWantExtraShifts)));
            if (this.showWantExtraShifts)
                elements.Add(new XElement("WES", StringUtility.GetString(this.queueItem.WantExtraShifts ?? false)));

            return CreateDocument(ROOTNAME, elements);
        }

        #endregion       
    }

    #endregion

    #region ShiftTask

    internal class MobileShiftTasks : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "ShiftTasks";

        #endregion

        #region Variables

        #region Collections

        private List<MobileShiftTask> mobileShiftTasks;

        #endregion

        #endregion

        #region Ctor

        public MobileShiftTasks(MobileParam param, List<TimeScheduleTemplateBlockTaskDTO> shiftTasks)
            : base(param)
        {
            Init();
            AddMobileShiftTasks(shiftTasks);
        }

        /// <summary>Used for errors</summary>
        public MobileShiftTasks(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            this.mobileShiftTasks = new List<MobileShiftTask>();
        }

        #endregion

        #region Public methods

        public void AddMobileShiftTasks(List<TimeScheduleTemplateBlockTaskDTO> shiftTasks)
        {
            if (shiftTasks == null)
                return;

            foreach (var shiftTask in shiftTasks)
            {
                AddMobileShiftTask(new MobileShiftTask(this.Param, shiftTask));
            }
        }

        public void AddMobileShiftTask(MobileShiftTask mobileShiftTask)
        {
            if (mobileShiftTask == null)
                return;

            mobileShiftTasks.Add(mobileShiftTask);
        }

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            return MergeDocuments(ROOTNAME, mobileShiftTasks.Select(i => i.ToXDocument()).ToList());
        }

        #endregion        
    }

    internal class MobileShiftTask : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "ShiftTask";

        #endregion

        #region Variables

        #region Field values

        readonly TimeScheduleTemplateBlockTaskDTO shiftTask;

        #endregion

        #endregion

        #region Ctor

        public MobileShiftTask(MobileParam param, TimeScheduleTemplateBlockTaskDTO shiftTask)
            : base(param)
        {
            Init();

            this.shiftTask = shiftTask;
        }

        /// <summary>
        /// Used for errors
        /// </summary>
        public MobileShiftTask(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            //Set default values
        }

        #endregion

        #region Public methods

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();

            elements.Add(new XElement("Time", this.shiftTask.StartTime.ToShortTimeString() + " - " + this.shiftTask.StopTime.ToShortTimeString()));
            elements.Add(new XElement("Name", this.shiftTask.Name));
            elements.Add(new XElement("Desc", this.shiftTask.Description));

            return CreateDocument(ROOTNAME, elements);
        }

        #endregion       
    }

    #endregion

    #region ShiftRequestStatus

    internal class MobileShiftRequestStatus : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "ShiftRequestStatus";

        #endregion

        #region Variables

        #region Field values

        private readonly int messageId;
        private readonly string text;
        private readonly DateTime? sentDate;
        private readonly string senderName;

        #endregion

        #region Collections

        private List<MobileShiftRequestStatusRecipient> shiftRequestStatusRecipients;

        #endregion

        #endregion

        #region Ctor

        public MobileShiftRequestStatus(MobileParam param, ShiftRequestStatusDTO shiftRequest)
            : base(param)
        {
            Init();

            messageId = shiftRequest.MessageId;
            text = shiftRequest.Text;
            sentDate = shiftRequest.SentDate;
            senderName = shiftRequest.SenderName;

            AddShiftRequestStatusRecipients(shiftRequest.Recipients);
        }

        /// <summary>Used for errors</summary>
        public MobileShiftRequestStatus(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            shiftRequestStatusRecipients = new List<MobileShiftRequestStatusRecipient>();
        }

        #endregion

        #region Public methods

        public void AddShiftRequestStatusRecipients(List<ShiftRequestStatusRecipientDTO> recipients)
        {
            if (recipients == null)
                return;

            foreach (var recipient in recipients)
            {
                AddShiftRequestStatusRecipient(new MobileShiftRequestStatusRecipient(Param, recipient));
            }
        }

        public void AddShiftRequestStatusRecipient(MobileShiftRequestStatusRecipient recipient)
        {
            if (recipient == null)
                return;

            shiftRequestStatusRecipients.Add(recipient);
        }

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();

            elements.Add(new XElement("MID", messageId));
            elements.Add(new XElement("TXT", text));
            elements.Add(new XElement("SN", senderName));
            elements.Add(new XElement("SD", sentDate.HasValue ? sentDate.Value.ToShortDateShortTimeString() : string.Empty));

            return MergeDocuments(ROOTNAME, shiftRequestStatusRecipients.Select(i => i.ToXDocument()).ToList(), elements);
        }

        #endregion        
    }

    internal class MobileShiftRequestStatusRecipient : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "Recipient";

        #endregion

        #region Variables

        #region Field values

        private readonly int userId;
        private readonly string employeeName;
        private readonly string employeeNr;
        private readonly DateTime? readDate;
        private readonly DateTime? answerDate;
        private readonly bool accepted;
        private readonly bool denied;
        private readonly bool deleted;

        #endregion

        #endregion

        #region Ctor

        public MobileShiftRequestStatusRecipient(MobileParam param, ShiftRequestStatusRecipientDTO recipient)
            : base(param)
        {
            Init();

            userId = recipient.UserId;
            employeeName = recipient.EmployeeName;
            employeeNr = recipient.EmployeeNr;
            readDate = recipient.ReadDate;
            answerDate = recipient.AnswerDate;
            accepted = recipient.AnswerType == XEMailAnswerType.Yes;
            denied = recipient.AnswerType == XEMailAnswerType.No;
            deleted = recipient.State == SoeEntityState.Deleted;
        }

        /// <summary>
        /// Used for errors
        /// </summary>
        public MobileShiftRequestStatusRecipient(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            //Set default values
        }

        #endregion

        #region Public methods

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();

            elements.Add(new XElement("UID", userId));
            elements.Add(new XElement("EN", "(" + employeeNr + ") " + employeeName));
            elements.Add(new XElement("RD", readDate.HasValue ? readDate.Value.ToShortDateShortTimeString() : string.Empty));
            elements.Add(new XElement("AD", answerDate.HasValue ? answerDate.Value.ToShortDateShortTimeString() : string.Empty));
            elements.Add(new XElement("IsA", StringUtility.GetString(accepted)));
            elements.Add(new XElement("IsD", StringUtility.GetString(denied)));
            elements.Add(new XElement("IsDel", StringUtility.GetString(deleted)));

            return CreateDocument(ROOTNAME, elements);
        }

        #endregion       
    }

    #endregion    

    #region Employeerequest

    internal class MobileRequests : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "Requests";

        #endregion

        #region Variables

        #region Collections

        private List<MobileRequest> requests;

        #endregion

        #endregion

        #region Ctor

        public MobileRequests(MobileParam param, List<EmployeeRequest> requests, MobileDisplayMode displayMode, bool includeComment = false)
            : base(param)
        {
            Init();
            AddMobileRequests(requests, displayMode, includeComment);
        }

        /// <summary>Used for errors</summary>
        public MobileRequests(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            this.requests = new List<MobileRequest>();
        }

        #endregion

        #region Public methods

        public void AddMobileRequests(List<EmployeeRequest> requests, MobileDisplayMode displayMode, bool includeComment)
        {

            foreach (var request in requests)
            {
                AddMobileRequest(new MobileRequest(this.Param, request, includeComment, displayMode, new List<TimeSchedulePlanningDayDTO>()));
            }
        }

        public void AddMobileRequest(MobileRequest request)
        {
            if (request == null)
                return;

            requests.Add(request);
        }

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            return MergeDocuments(ROOTNAME, requests.Select(i => i.ToXDocument()).ToList());
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            return XmlUtil.CreateXml(ROOTNAME, MobileRequest.GetDefaultXml());
        }

        #endregion
    }

    internal class MobileRequest : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "Request";

        #endregion

        #region Variables

        private readonly EmployeeRequest request;
        private readonly bool includeComment = false;
        private readonly MobileDisplayMode displayMode;
        List<MobileAbsenceRequestShift> shifts;

        #endregion

        #region Ctor

        public MobileRequest(MobileParam param)
            : base(param)
        {
            Init();
        }

        public MobileRequest(MobileParam param, EmployeeRequest request, bool includeComment, MobileDisplayMode displayMode, List<TimeSchedulePlanningDayDTO> affectedShifts)
            : base(param)
        {
            Init();
            this.request = request;
            this.includeComment = includeComment;
            this.displayMode = displayMode;
            foreach (var affectedShift in affectedShifts)
            {
                shifts.Add(new MobileAbsenceRequestShift(this.Param, affectedShift));
            }
        }

        /// <summary>
        /// Used for errors
        /// </summary>
        public MobileRequest(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            shifts = new List<MobileAbsenceRequestShift>();
        }

        #endregion

        #region Public methods

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();

            if (request != null)
            {
                if (this.displayMode == MobileDisplayMode.User)
                {
                    #region Employee

                    elements.Add(new XElement("Id", this.request.EmployeeRequestId));
                    elements.Add(new XElement("Note", includeComment ? this.request.Comment : string.Empty));
                    elements.Add(new XElement("Status", this.request.StatusName));

                    if (this.request.Type == (int)TermGroup_EmployeeRequestType.AbsenceRequest)
                    {
                        elements.Add(new XElement("CauseId", this.request.TimeDeviationCauseId));
                        elements.Add(new XElement("Cause", this.request.TimeDeviationCauseName + (this.request.EmployeeChildId.HasValue ? ("( " + this.request.EmployeeChildFirstName + " )") : "")));
                        elements.Add(new XElement("OWH", StringUtility.GetString(this.request.TimeDeviationCause?.OnlyWholeDay ?? false)));
                        elements.Add(new XElement("ECId", this.request.EmployeeChildId ?? 0));

                        DateTime dateFrom = this.request.Start;
                        DateTime dateTo = this.request.Stop;

                        if (this.request.ExtendedAbsenceSetting != null)
                        {
                            if (this.request.ExtendedAbsenceSetting.AbsenceFirstDayStart.HasValue)
                                dateFrom = CalendarUtility.MergeDateAndTime(dateFrom, this.request.ExtendedAbsenceSetting.AbsenceFirstDayStart.Value);
                            else
                                dateFrom = CalendarUtility.GetBeginningOfDay(this.request.Start); //00:00

                            if (this.request.ExtendedAbsenceSetting.AbsenceLastDayStart.HasValue)
                                dateTo = CalendarUtility.MergeDateAndTime(dateTo, this.request.ExtendedAbsenceSetting.AbsenceLastDayStart.Value);
                            else
                                dateTo = CalendarUtility.GetBeginningOfDay(dateTo); //00:00
                        }
                        if (mobileManagerUtil.IsCallerExpectedVersionNewerThenGivenVersion(Param.Version, Constants.MOBILE_WS_SUPPORTED_OLDVERSION_33))
                        {
                            elements.Add(new XElement("DateF", StringUtility.GetSwedishFormattedDateTime(dateFrom)));
                            elements.Add(new XElement("DateT", StringUtility.GetSwedishFormattedDateTime(dateTo)));

                        }
                        else
                        {
                            elements.Add(new XElement("DateF", dateFrom));
                            elements.Add(new XElement("DateT", dateTo));
                        }
                        elements.Add(new XElement("ReadOnly", StringUtility.GetString(this.request.Status != (int)TermGroup_EmployeeRequestStatus.RequestPending && this.request.Status != (int)TermGroup_EmployeeRequestStatus.Restored)));
                        elements.Add(new XElement("WholeDays", StringUtility.GetString(this.request.ExtendedAbsenceSetting == null)));
                    }
                    else
                    {
                        if (mobileManagerUtil.IsCallerExpectedVersionNewerThenGivenVersion(Param.Version, Constants.MOBILE_WS_SUPPORTED_OLDVERSION_33))
                        {
                            elements.Add(new XElement("DateF", StringUtility.GetSwedishFormattedDateTime(this.request.Start)));
                            elements.Add(new XElement("DateT", StringUtility.GetSwedishFormattedDateTime(this.request.Stop)));
                        }
                        else
                        {
                            elements.Add(new XElement("DateF", this.request.Start));
                            elements.Add(new XElement("DateT", this.request.Stop));
                        }
                        elements.Add(new XElement("Available", ((request.Type == (int)TermGroup_EmployeeRequestType.InterestRequest) ? "1" : "0")));
                        elements.Add(new XElement("ReadOnly", "0"));
                    }

                    #endregion
                }
                else if (this.displayMode == MobileDisplayMode.Admin)
                {
                    #region Admin

                    elements.Add(new XElement("Id", this.request.EmployeeRequestId));
                    elements.Add(new XElement("Note", this.request.Comment));
                    elements.Add(new XElement("Status", this.request.StatusName));

                    if (this.request.Type == (int)TermGroup_EmployeeRequestType.AbsenceRequest)
                    {
                        if (this.request.Employee != null)
                        {
                            elements.Add(new XElement("EID", this.request.EmployeeId));
                            elements.Add(new XElement("ENR", this.request.Employee.EmployeeNr));
                            if (this.request.Employee.ContactPerson != null)
                            {
                                elements.Add(new XElement("FN", this.request.Employee.ContactPerson.FirstName));
                                elements.Add(new XElement("LN", this.request.Employee.ContactPerson.LastName));
                            }
                        }
                        elements.Add(new XElement("CauseId", this.request.TimeDeviationCauseId));
                        elements.Add(new XElement("Cause", this.request.TimeDeviationCauseName + (this.request.EmployeeChildId.HasValue ? ("( " + this.request.EmployeeChildFirstName + " )") : "")));
                        elements.Add(new XElement("OWH", StringUtility.GetString(this.request.TimeDeviationCause?.OnlyWholeDay ?? false)));
                        if (mobileManagerUtil.IsCallerExpectedVersionNewerThenGivenVersion(Param.Version, Constants.MOBILE_WS_SUPPORTED_OLDVERSION_33))
                        {
                            elements.Add(new XElement("DateF", StringUtility.GetSwedishFormattedDateTime(this.request.Start)));
                            elements.Add(new XElement("DateT", StringUtility.GetSwedishFormattedDateTime(this.request.Stop)));
                        }
                        else
                        {
                            elements.Add(new XElement("DateF", this.request.Start.ToShortDateString()));
                            elements.Add(new XElement("DateT", this.request.Stop.ToShortDateString()));
                        }

                        if (request.Status == (int)TermGroup_EmployeeRequestStatus.RequestPending || request.Status == (int)TermGroup_EmployeeRequestStatus.Restored)
                            elements.Add(new XElement("ADelete", StringUtility.GetString(true)));

                        foreach (var shift in shifts)
                        {
                            elements.Add(shift.ToXElement());
                        }
                    }
                    else
                    {
                        if (mobileManagerUtil.IsCallerExpectedVersionNewerThenGivenVersion(Param.Version, Constants.MOBILE_WS_SUPPORTED_OLDVERSION_33))
                        {
                            elements.Add(new XElement("DateF", StringUtility.GetSwedishFormattedDateTime(this.request.Start)));
                            elements.Add(new XElement("DateT", StringUtility.GetSwedishFormattedDateTime(this.request.Stop)));
                        }
                        else
                        {
                            elements.Add(new XElement("DateF", this.request.Start));
                            elements.Add(new XElement("DateT", this.request.Stop));
                        }

                        elements.Add(new XElement("Available", ((request.Type == (int)TermGroup_EmployeeRequestType.InterestRequest) ? "1" : "0")));
                        elements.Add(new XElement("ReadOnly", "0"));
                    }
                    #endregion
                }
            }

            return CreateDocument(ROOTNAME, elements);
        }

        public override XDocument ToXDocument(MobileTask task)
        {
            if (base.Failed)
                return MobileMessages.GetErrorMessageDocument(base.ErrorMessage);

            bool result = base.GetTaskResult(task);
            switch (task)
            {
                case MobileTask.SaveEmployeeRequest:
                case MobileTask.DeleteEmployeeRequest:
                    return MobileMessages.GetSuccessDocument(result);
                default:
                    return base.ToXDocument(task);
            }
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            string xml =
                "<Id></Id>" +
                "<CauseId></CauseId>" +
                "<Cause></Cause>" +
                "<Available></Available>" +
                "<DateF></DateF>" +
                "<DateT></DateT>" +
                "<Note></Note>" +
                "<Status></Status>" +
                "<ReadOnly></ReadOnly>";

            return XmlUtil.CreateXml(ROOTNAME, xml);
        }

        #endregion
    }

    #endregion

    #region Modules

    internal class MobileModules : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "Modules";

        #endregion

        #region Variables

        #region Collections

        private List<MobileModule> modules;

        #endregion

        #endregion

        #region Ctor

        public MobileModules(MobileParam param)
            : base(param)
        {
            Init();
        }

        /// <summary>Used for errors</summary>
        public MobileModules(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            this.modules = new List<MobileModule>();
        }

        #endregion

        #region Public methods

        public void AddMobileModule(MobileModuleType moduleType, int position, int count, bool generateStartPage)
        {
            AddMobileModule(new MobileModule(this.Param, moduleType, position, count, generateStartPage));
        }

        public void AddMobileModule(MobileModule module)
        {
            if (module == null)
                return;

            modules.Add(module);
        }

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            return MergeDocuments(ROOTNAME, modules.Select(i => i.ToXDocument()).ToList());
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            return XmlUtil.CreateXml(ROOTNAME, MobileModule.GetDefaultXml());
        }

        #endregion
    }

    internal class MobileModule : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "Module";

        #endregion

        #region Variables

        #region Field values

        private readonly int position;
        private readonly int count;
        private readonly bool generateStartPage;
        private readonly MobileModuleType moduleType = MobileModuleType.Undefined;

        #endregion

        #endregion

        #region Ctor

        public MobileModule(MobileParam param)
            : base(param)
        {
            Init();
        }

        public MobileModule(MobileParam param, MobileModuleType moduleType, int position, int count, bool generateStartPage)
            : base(param)
        {
            Init();

            this.moduleType = moduleType;
            this.position = position;
            this.count = count;
            this.generateStartPage = generateStartPage;

        }

        /// <summary>
        /// Used for errors
        /// </summary>
        public MobileModule(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            //Set default values
        }

        #endregion

        #region Public methods

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();

            elements.Add(new XElement("Id", (int)moduleType));
            if (generateStartPage)
                elements.Add(new XElement("Pos", position));
            else
                elements.Add(new XElement("Count", count));

            return CreateDocument(ROOTNAME, elements);
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            string xml =
                "<Id></Id>" +
                "<Pos></Pos>" +
                "<Count></Count>";
            return XmlUtil.CreateXml(ROOTNAME, xml);
        }

        #endregion
    }


    #endregion

    #region News

    internal class MobileInternalNews : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "Inews";

        #endregion

        #region Variables

        #region Collections

        private List<MobileNews> newsList;

        #endregion

        #endregion

        #region Ctor

        public MobileInternalNews(MobileParam param)
            : base(param)
        {
            Init();

        }

        /// <summary>Used for errors</summary>
        public MobileInternalNews(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            this.newsList = new List<MobileNews>();
        }

        #endregion

        #region Public methods

        public void AddMobileInternalNews(List<InformationDTO> informations)
        {
            foreach (InformationDTO information in informations)
            {
                string text = "";
                if (!String.IsNullOrEmpty(information.ShortText))
                {
                    text = information.ShortText;
                    if (!String.IsNullOrEmpty(information.Text))
                        text += "<br><br>";
                }
                text += information.Text;

                string plainText = StringUtility.HTMLToText(text, true);
                AddMobileInternalNews(new MobileNews(this.Param, information.InformationId, information.Subject, plainText, information.Created, false));
            }
        }

        public void AddMobileInternalNews(MobileNews news)
        {
            if (news == null)
                return;

            newsList.Add(news);
        }

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            return MergeDocuments(ROOTNAME, newsList.Select(i => i.ToXDocument()).ToList());
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            return XmlUtil.CreateXml(ROOTNAME, MobileNews.GetDefaultXml());
        }

        #endregion
    }

    internal class MobileNews : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "News";

        #endregion

        #region Variables

        #region Field values

        private readonly int id;
        private readonly string title;
        private readonly string text;
        private readonly bool hasAtts; //attachment
        private DateTime? created;

        #endregion

        #endregion

        #region Ctor

        public MobileNews(MobileParam param)
            : base(param)
        {
            Init();
        }

        public MobileNews(MobileParam param, int id, string title, string text, DateTime? created, bool hasAtts)
            : base(param)
        {
            Init();
            this.id = id;
            this.title = title;
            this.text = String.IsNullOrEmpty(text) ? string.Empty : text;
            this.hasAtts = hasAtts;
            this.created = created;
        }

        /// <summary>
        /// Used for errors
        /// </summary>
        public MobileNews(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            //Set default values
        }

        #endregion

        #region Public methods

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();

            elements.Add(new XElement("Id", id));
            elements.Add(new XElement("Subject", title));
            elements.Add(new XElement("Text", text));
            elements.Add(new XElement("Atts", StringUtility.GetString(hasAtts)));
            if (created.HasValue)
                elements.Add(new XElement("Date", created.Value.ToShortDateString()));

            return CreateDocument(ROOTNAME, elements);
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            string xml =
                "<Id></Id>" +
                "<Subject></Subject>" +
                "<Text></Text>" +
                "<Atts></Atts>";

            return XmlUtil.CreateXml(ROOTNAME, xml);
        }

        #endregion
    }

    #endregion

    #region Files

    internal class MobileFiles : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "Files";

        #endregion

        #region Variables

        #region Collections

        private List<MobileFile> files;

        #endregion

        #endregion

        #region Ctor

        public MobileFiles(MobileParam param)
            : base(param)
        {
            Init();
        }

        /// <summary>Used for errors</summary>
        public MobileFiles(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            this.files = new List<MobileFile>();
        }

        #endregion

        #region Public methods

        public void AddMobileFile(int id, string name, string path)
        {
            AddMobileFile(new MobileFile(this.Param, id, name, path, "", null, false, null, ""));
        }

        public void AddMobileFiles(List<MessageAttachmentDTO> attachments)
        {
            foreach (var item in attachments)
            {
                AddMobileFile(new MobileFile(this.Param, item.MessageAttachmentId, item.Name, "", "", null, false, null, ""));
            }
        }

        public void AddMobileFiles(List<DataStorageDTO> documents)
        {
            foreach (var item in documents)
            {
                string name = GetName(item.Name, item.Description, item.FileName);

                var receiver = item.DataStorageRecipients?.FirstOrDefault(x => x.UserId == this.Param.UserId) ?? null;
                AddMobileFile(new MobileFile(this.Param, item.DataStorageId, name, "", item.Folder, receiver?.ReadDate, item.NeedsConfirmation, receiver?.ConfirmedDate, item.FileName));
            }
        }

        public void AddMobileFiles(List<DataStorageSmallDTO> dataStorageFiles)
        {
            foreach (var item in dataStorageFiles)
            {
                AddMobileFile(new MobileFile(this.Param, item.DataStorageId, item.TimePeriodName, "", "", null, false, null, "", item.TimePeriodId, item.Type));
            }
        }

        public void AddMobileFiles(List<DocumentDTO> documents)
        {
            foreach (var item in documents)
            {
                string name = GetName(item.Name, item.Description, item.FileName);

                AddMobileFile(new MobileFile(this.Param, item.DataStorageId, name, "", item.Folder, item.ReadDate, item.NeedsConfirmation, item.ConfirmedDate, ""));
            }
        }

        public void AddMobileFile(MobileFile file)
        {
            if (file == null)
                return;

            files.Add(file);
        }

        #endregion

        #region Private methods
        private string GetName(string name, string description, string fileName)
        {
            if (!string.IsNullOrEmpty(name))
            {
                return name;
            }
            else if (!string.IsNullOrEmpty(description))
            {
                return description;
            }
            else
            {
                return fileName;
            }
        }
        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            return MergeDocuments(ROOTNAME, files.Select(i => i.ToXDocument()).ToList());
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            return XmlUtil.CreateXml(ROOTNAME, MobileFile.GetDefaultXml());
        }

        #endregion
    }

    internal class MobileFile : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "File";

        #endregion

        #region Variables

        #region Field values

        readonly int id;
        readonly string name;
        readonly string fileName;
        readonly string path;
        readonly string folder;
        readonly DateTime? readDate;
        readonly DateTime? confirmedDate;
        readonly bool needsConfirmation;
        readonly int? timePeriodId;
        readonly SoeDataStorageRecordType type;
        #endregion

        #endregion

        #region Ctor

        public MobileFile(MobileParam param)
            : base(param)
        {
            Init();
        }

        public MobileFile(MobileParam param, int id, string name, string path, string folder, DateTime? readDate, bool needsConfirmation, DateTime? confirmedDate, string fileName, int? timePeriodId = null, SoeDataStorageRecordType type = 0)
            : base(param)
        {
            Init();

            this.id = id;
            this.name = name;
            this.fileName = fileName;
            this.path = path;
            this.folder = folder;
            this.readDate = readDate;
            this.confirmedDate = confirmedDate;
            this.needsConfirmation = needsConfirmation;
            this.timePeriodId = timePeriodId;
            this.type = type;
        }

        /// <summary>
        /// Used for errors
        /// </summary>
        public MobileFile(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            //Set default values
        }

        #endregion

        #region Public methods

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();

            elements.Add(new XElement("Id", this.id));
            elements.Add(new XElement("Name", GetTextOrCDATA(this.name)));

            if (this.fileName.IsNullOrEmpty())
                elements.Add(new XElement("FileType", this.name.IsNullOrEmpty() ? "" : Path.GetExtension(this.name)));
            else
                elements.Add(new XElement("FileType", Path.GetExtension(this.fileName)));

            elements.Add(new XElement("Path", this.path));
            elements.Add(new XElement("Folder", GetTextOrCDATA(this.folder)));
            elements.Add(new XElement("NC", StringUtility.GetString(this.needsConfirmation)));

            if (this.readDate.HasValue)
            {
                if (mobileManagerUtil.IsCallerExpectedVersionNewerThenGivenVersion(Param.Version, Constants.MOBILE_WS_SUPPORTED_OLDVERSION_33))
                    elements.Add(new XElement("RDate", StringUtility.GetSwedishFormattedDate(readDate.Value)));
                else
                    elements.Add(new XElement("RDate", this.readDate.Value.ToShortDateString()));
            }
            if (this.confirmedDate.HasValue)
            {
                if (mobileManagerUtil.IsCallerExpectedVersionNewerThenGivenVersion(Param.Version, Constants.MOBILE_WS_SUPPORTED_OLDVERSION_33))
                    elements.Add(new XElement("CDate", StringUtility.GetSwedishFormattedDate(this.confirmedDate.Value)));
                else
                    elements.Add(new XElement("CDate", this.confirmedDate.Value.ToShortDateString()));
            }
            if (!this.timePeriodId.IsNullOrEmpty())
                elements.Add(new XElement("TimePeriodId", timePeriodId));

            if (this.type == SoeDataStorageRecordType.PayrollSlipXML)
                elements.Add(new XElement("IsGOPayrollSlip", 1));

            return CreateDocument(ROOTNAME, elements);
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            string xml =
                "<Id></Id>" +
                "<Name></Name>" +
                "<Path></Path>";

            return XmlUtil.CreateXml(ROOTNAME, xml);
        }

        #endregion
    }

    #endregion

    #region Information

    internal class MobileInformations : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "Infos";
       
        #endregion

        #region Variables

        #region Collections

        private List<MobileInformation> informationList;

        #endregion

        #endregion

        #region Ctor

        public MobileInformations(MobileParam param)
            : base(param)
        {
            Init();

        }

        /// <summary>Used for errors</summary>
        public MobileInformations(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            informationList = new List<MobileInformation>();
        }

        #endregion

        #region Public methods

        public void AddMobileInformations(List<InformationDTO> informations)
        {
            foreach (InformationDTO information in informations)
            {
                AddMobileInformation(new MobileInformation(Param, information.InformationId, (int)information.SourceType, information.Created, (int)information.Severity, information.Subject, information.ShortText, StringUtility.HTMLToText(information.Text, true), information.Folder, information.ReadDate, information.AnswerDate, information.NeedsConfirmation));
            }
        }

        public void AddMobileInformation(MobileInformation information)
        {
            if (information == null)
                return;

            informationList.Add(information);
        }

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            return MergeDocuments(ROOTNAME, informationList.Select(i => i.ToXDocument()).ToList());
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            string xml =
                "<Infos />";

            return XmlUtil.CreateXml(ROOTNAME, xml);
        }

        #endregion
    }

    internal class MobileInformation : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "Info";

        #endregion

        #region Variables

        #region Field values
        
        readonly private int id;
        readonly private int sourceType;
        readonly private DateTime? created;
        readonly private int severity;
        readonly private string title;
        readonly private string shortText;
        readonly private string text;
        readonly private string folder;
        readonly private DateTime? readDate;
        readonly private DateTime? answerDate;
        readonly private bool needsConfirmation;

        #endregion

        #endregion

        #region Ctor

        public MobileInformation(MobileParam param)
            : base(param)
        {
            Init();
        }

        public MobileInformation(MobileParam param, int id, int sourceType, DateTime? created, int severity, string title, string shortText, string text, string folder, DateTime? readDate, DateTime? answerDate, bool needsConfirmation)
            : base(param)
        {
            Init();
            this.id = id;
            this.sourceType = sourceType;
            this.created = created;
            this.severity = severity;
            this.title = title;
            this.shortText = string.IsNullOrEmpty(shortText) ? string.Empty : shortText;
            this.text = string.IsNullOrEmpty(text) ? string.Empty : text;
            this.folder = folder;
            this.readDate = readDate;
            this.answerDate = answerDate;
            this.needsConfirmation = needsConfirmation;
        }

        /// <summary>
        /// Used for errors
        /// </summary>
        public MobileInformation(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            //Set default values
        }

        #endregion

        #region Public methods

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();

            elements.Add(new XElement("Id", id));
            elements.Add(new XElement("ST", sourceType));

            if (created.HasValue) {
                if (mobileManagerUtil.IsCallerExpectedVersionNewerThenGivenVersion(Param.Version, Constants.MOBILE_WS_SUPPORTED_OLDVERSION_31))
                    elements.Add(new XElement("Date", StringUtility.GetSwedishFormattedDate(created.Value)));
                else
                    elements.Add(new XElement("Date", created.Value.ToShortDateString()));
            }

            elements.Add(new XElement("Severity", severity));
            elements.Add(new XElement("Title", title));
            elements.Add(new XElement("SText", shortText));
            elements.Add(new XElement("Text", text));
            elements.Add(new XElement("Folder", folder));

            if (readDate.HasValue)
            {
                if (mobileManagerUtil.IsCallerExpectedVersionNewerThenGivenVersion(Param.Version, Constants.MOBILE_WS_SUPPORTED_OLDVERSION_31))
                    elements.Add(new XElement("RDate", StringUtility.GetSwedishFormattedDate(readDate.Value)));
                else
                    elements.Add(new XElement("RDate", readDate.Value.ToShortDateString())); 
            }
            if (answerDate.HasValue)
            {
                if (mobileManagerUtil.IsCallerExpectedVersionNewerThenGivenVersion(Param.Version, Constants.MOBILE_WS_SUPPORTED_OLDVERSION_31))
                    elements.Add(new XElement("ADate", StringUtility.GetSwedishFormattedDate(answerDate.Value)));
                else
                    elements.Add(new XElement("ADate", answerDate.Value.ToShortDateString()));

            }

            elements.Add(new XElement("NC", needsConfirmation));

            return CreateDocument(ROOTNAME, elements);
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            string xml =
                "<Id></Id>" +
                "<ST></ST>" +
                "<Date></Date>" +
                "<Severity></Severity>" +
                "<Title></Title>" +
                "<SText></SText>" +
                "<Text></Text>" +
                "<Atts></Atts>";

            return XmlUtil.CreateXml(ROOTNAME, xml);
        }

        #endregion
    }

    #endregion

    #region XeMAil

    internal class MobileXeMails : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "XeMail";

        #endregion

        #region Variables

        #region Collections

        private List<MobileXeMail> mailList;

        #endregion

        #endregion

        #region Ctor

        public MobileXeMails(MobileParam param)
            : base(param)
        {
            Init();
        }

        public MobileXeMails(MobileParam param, List<MessageGridDTO> messages, XEMailType mailType)
            : base(param)
        {
            Init();
            AddMobileXeMail(messages, mailType);
        }

        /// <summary>Used for errors</summary>
        public MobileXeMails(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            this.mailList = new List<MobileXeMail>();
        }

        #endregion

        #region Public methods

        public void AddMobileXeMail(List<MessageGridDTO> messages, XEMailType mailType)
        {
            foreach (MessageGridDTO message in messages)
            {
                AddMobileXeMail(new MobileXeMail(this.Param, message, mailType));
            }

        }

        public void AddMobileXeMail(MobileXeMail mail)
        {
            if (mail == null)
                return;

            mailList.Add(mail);
        }

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            return MergeDocuments(ROOTNAME, mailList.Select(i => i.ToXDocument()).ToList());
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            return XmlUtil.CreateXml(ROOTNAME, MobileXeMail.GetDefaultXml());
        }

        #endregion
    }

    internal class MobileXeMail : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "Mail";

        #endregion

        #region Variables

        #region Field values

        readonly private int id;
        readonly private int? parentId;
        readonly private int? recordId;
        readonly private string sender;
        readonly private string receivers;
        readonly private string subject;
        readonly private string text;
        readonly private string firstTextRow;
        readonly private DateTime? date;
        readonly private DateTime? read;
        readonly private DateTime? replyDate;
        readonly private DateTime? forwardDate;
        readonly private DateTime? confirmedDate;
        readonly private bool hasAttachments;
        readonly private bool isRequest;
        readonly private XEMailType mailType;
        readonly private bool populateEdit = false;
        readonly private bool needsConfirmation = false;
        readonly private bool isConfirmed = false;
        readonly private bool isAbsenceRequest = false;
        readonly private bool isSwapRequest = false;
        readonly private bool isPayrollSlip = false;
        readonly private bool isAttestInvoice = false;
        readonly private bool isOrderAssigned = false;
        readonly private bool openAbsenceRequestAsAdmin = false;
        readonly private bool showTimeWorkAccountOption = false;

        #endregion

        #endregion

        #region Ctor

        public MobileXeMail(MobileParam param) : base(param)
        {
            Init();
        }

        public MobileXeMail(MobileParam param, MessageGridDTO message, XEMailType mailType, bool populateEdit = false)
            : base(param)
        {
            Init();
            this.id = message.MessageId;
            this.sender = message.SenderName;
            this.receivers = message.RecieversName;
            this.subject = message.Subject;
            this.date = message.SentDate;
            this.read = message.ReadDate;
            this.replyDate = message.ReplyDate;
            this.forwardDate = message.ForwardDate;
            this.hasAttachments = message.HasAttachment;
            this.isRequest = message.MessageType == TermGroup_MessageType.ShiftRequest;
            this.isSwapRequest = message.MessageType == TermGroup_MessageType.SwapRequest;
            this.isPayrollSlip = message.MessageType == TermGroup_MessageType.PayrollSlip;
            this.isAttestInvoice = message.MessageType == TermGroup_MessageType.AttestInvoice;
            this.isOrderAssigned = message.MessageType == TermGroup_MessageType.OrderAssigned;
            this.showTimeWorkAccountOption = message.MessageType == TermGroup_MessageType.TimeWorkAccountYearEmployeeOption;
            this.mailType = mailType;
            this.populateEdit = populateEdit;
            this.needsConfirmation = message.NeedsConfirmation;
            this.isConfirmed = message.AnswerDate.HasValue;
            this.firstTextRow = StringUtility.HTMLToText(message.FirstTextRow, true);
        }

        public MobileXeMail(MobileParam param, MessageEditDTO message, XEMailType mailType, int userId, bool populateEdit = true, bool hasAbsenceRequestAdminPermission = false) : base(param)
        {
            Init();

            this.populateEdit = populateEdit;
            this.id = message.MessageId;
            this.parentId = message.ParentId;
            this.sender = message.SenderName;
            this.subject = message.Subject;
            this.text = message.ShortText != null ? message.ShortText.Replace("<br>", "\r\n") : "";

            if (mailType == XEMailType.Sent || mailType == XEMailType.Incoming)
                this.date = message.SentDate;

            foreach (var item in message.Recievers)
            {
                this.receivers += item.Name + ", ";

                if (item.UserId == userId)
                {
                    this.replyDate = item.ReplyDate;
                    this.forwardDate = item.ForwardDate;
                }
            }

            this.receivers = this.receivers?.TrimEnd(new char[] { ' ', ',' });

            MessageRecipientDTO userCopy = message.Recievers.FirstOrDefault(x => x.UserId == userId);
            if (userCopy != null)
                this.read = userCopy.ReadDate;

            this.hasAttachments = !message.Attachments.IsNullOrEmpty();
            this.isRequest = message.MessageType == TermGroup_MessageType.ShiftRequest;
            this.isSwapRequest = message.MessageType == TermGroup_MessageType.SwapRequest;
            this.isPayrollSlip = message.MessageType == TermGroup_MessageType.PayrollSlip;
            this.isAttestInvoice = message.MessageType == TermGroup_MessageType.AttestInvoice;
            this.isOrderAssigned = message.MessageType == TermGroup_MessageType.OrderAssigned;
            this.showTimeWorkAccountOption = message.MessageType == TermGroup_MessageType.TimeWorkAccountYearEmployeeOption;
            this.mailType = mailType;
            this.needsConfirmation = message.MessageType == TermGroup_MessageType.NeedsConfirmation;
            this.isConfirmed = this.needsConfirmation && userCopy != null && userCopy.AnswerType == XEMailAnswerType.Yes;
            this.recordId = message.RecordId;

            if (this.isConfirmed && userCopy != null && userCopy.AnswerDate.HasValue)
            {
                this.confirmedDate = userCopy.AnswerDate.Value;
            }

            if (message.MessageType == TermGroup_MessageType.AbsenceRequest && message.RecordId != 0 && (hasAbsenceRequestAdminPermission || (userId == message.AbsenceRequestEmployeeUserId)))
            {
                this.isAbsenceRequest = true;
                this.openAbsenceRequestAsAdmin = hasAbsenceRequestAdminPermission;
            }
        }

        /// <summary>
        /// Used for errors
        /// </summary>
        public MobileXeMail(MobileParam param, string errorMessage) : base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            //Set default values
        }

        #endregion

        #region Public methods

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();

            elements.Add(new XElement("Id", this.id));
            elements.Add(new XElement("ParentId", this.parentId));
            if ((mailType == XEMailType.Incoming || populateEdit) && !this.isPayrollSlip)
                elements.Add(new XElement("Sender", this.sender));

            if (mailType == XEMailType.Sent || populateEdit)
                elements.Add(new XElement("Receivers", this.receivers));

            elements.Add(new XElement("Subject", GetTextOrCDATA(this.subject)));
            elements.Add(new XElement("Text", GetTextOrCDATA(this.text)));
            elements.Add(new XElement("Date", this.date.HasValue ? this.date.Value.ToShortDateShortTimeString() : string.Empty));
            elements.Add(new XElement("Read", this.read.HasValue ? this.read.Value.ToShortDateShortTimeString() : string.Empty));
            elements.Add(new XElement("ReplyDate", this.replyDate.HasValue ? this.replyDate.Value.ToShortDateShortTimeString() : string.Empty));
            elements.Add(new XElement("ForwardDate", this.forwardDate.HasValue ? this.forwardDate.Value.ToShortDateShortTimeString() : string.Empty));
            elements.Add(new XElement("ConfirmedDate", this.confirmedDate.HasValue ? this.confirmedDate.Value.ToShortDateShortTimeString() : string.Empty));
            elements.Add(new XElement("Atts", StringUtility.GetString(this.hasAttachments)));
            elements.Add(new XElement("IsReq", StringUtility.GetString(this.isRequest)));
            elements.Add(new XElement("NC", StringUtility.GetString(this.needsConfirmation)));
            elements.Add(new XElement("IC", StringUtility.GetString(this.isConfirmed)));
            elements.Add(new XElement("Type", (int)this.mailType));
            elements.Add(new XElement("RId", this.recordId));
            elements.Add(new XElement("IAR", StringUtility.GetString(this.isAbsenceRequest)));
            elements.Add(new XElement("ARA", StringUtility.GetString(this.openAbsenceRequestAsAdmin)));
            elements.Add(new XElement("ISR", StringUtility.GetString(this.isSwapRequest)));
            elements.Add(new XElement("IPS", StringUtility.GetString(this.isPayrollSlip)));
            elements.Add(new XElement("IAI", StringUtility.GetString(this.isAttestInvoice)));
            elements.Add(new XElement("IOA", StringUtility.GetString(this.isOrderAssigned)));
            elements.Add(new XElement("STWAO", StringUtility.GetString(this.showTimeWorkAccountOption)));
            elements.Add(new XElement("FTR", GetTextOrCDATA(firstTextRow)));

            return CreateDocument(ROOTNAME, elements);
        }

        public override XDocument ToXDocument(MobileTask task)
        {
            if (base.Failed)
                return MobileMessages.GetErrorMessageDocument(base.ErrorMessage);

            bool result = base.GetTaskResult(task);
            switch (task)
            {
                case MobileTask.MarkMailAsRead:
                case MobileTask.MarkMailAsUnread:
                case MobileTask.DeleteIncomingMail:
                case MobileTask.AnswerShiftRequest:
                case MobileTask.SendNeedsConfirmationAnswer:
                case MobileTask.SendMail:
                    return MobileMessages.GetSuccessDocument(result);
                default:
                    return base.ToXDocument(task);
            }
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            string xml =
                "<Id></Id>" +
                "<ParentId></ParentId>" +
                "<Sender></Sender>" +
                "<Receivers></Receivers>" +
                "<Subject></Subject>" +
                "<Text></Text>" +
                "<Date></Date>" +
                "<Read></Read>" +
                "<Atts></Atts>" +
                "<IsReq></IsReq>" +
                "<Type></Type>";

            return XmlUtil.CreateXml(ROOTNAME, xml);
        }

        #endregion
    }

    #endregion

    #region Receivers/MessageGroups

    internal class MobileReceivers : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "Receivers";

        #endregion

        #region Variables

        #region Collections

        private List<MobileReceiver> receivers;

        #endregion

        #endregion

        #region Ctor

        public MobileReceivers(MobileParam param)
            : base(param)
        {
            Init();
        }

        /// <summary>Used for errors</summary>
        public MobileReceivers(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            this.receivers = new List<MobileReceiver>();
        }

        #endregion

        #region Public methods

        public void AddMobileReceiver(int id, string name, XEMailRecipientType type)
        {
            AddMobileReceiver(new MobileReceiver(this.Param, id, name, type));
        }

        public void AddMobileReceivers(Dictionary<int, string> receivers, XEMailRecipientType type)
        {
            foreach (var receiver in receivers)
            {
                AddMobileReceiver(new MobileReceiver(this.Param, receiver.Key, receiver.Value, type));
            }
        }

        public void AddMobileReceivers(List<User> receivers, XEMailRecipientType type)
        {
            foreach (var receiver in receivers)
            {
                AddMobileReceiver(new MobileReceiver(this.Param, receiver.UserId, receiver.Name, type));
            }
        }

        public void AddMobileReceivers(List<MessageGroupDTO> messageGroups, XEMailRecipientType type)
        {
            foreach (var messageGroup in messageGroups)
            {
                AddMobileReceiver(new MobileReceiver(this.Param, messageGroup.MessageGroupId, messageGroup.Name, type));
            }
        }

        public void AddMobileReceiver(MobileReceiver receiver)
        {
            if (receiver == null)
                return;

            receivers.Add(receiver);
        }

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            return MergeDocuments(ROOTNAME, receivers.Select(i => i.ToXDocument()).ToList());
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            return XmlUtil.CreateXml(ROOTNAME, MobileReceiver.GetDefaultXml());
        }

        #endregion
    }

    internal class MobileReceiver : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "Receiver";

        #endregion

        #region Variables

        #region Field values

        private readonly int id;
        private readonly string name;
        private readonly XEMailRecipientType type;

        #endregion

        #endregion

        #region Ctor

        public MobileReceiver(MobileParam param)
            : base(param)
        {
            Init();
        }

        public MobileReceiver(MobileParam param, int id, string name, XEMailRecipientType type)
            : base(param)
        {
            Init();
            this.id = id;
            this.name = name;
            this.type = type;
        }

        /// <summary>
        /// Used for errors
        /// </summary>
        public MobileReceiver(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            //Set default values
        }

        #endregion

        #region Public methods

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();

            elements.Add(new XElement("Id", this.id));
            elements.Add(new XElement("Type", (int)this.type));
            elements.Add(new XElement("Name", this.name));

            return CreateDocument(ROOTNAME, elements);
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            string xml =
                "<Id></Id>" +
                "<Type></Type>" +
                "<Name></Name>";

            return XmlUtil.CreateXml(ROOTNAME, xml);
        }

        #endregion
    }

    internal class MobileMessageGroups : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "MessageGroups";

        #endregion

        #region Variables
        
        #region Collections

        private List<MobileMessageGroup> messageGroups;

        #endregion

        #endregion

        #region Ctor

        public MobileMessageGroups(MobileParam param)
            : base(param)
        {
            Init();
        }

        /// <summary>Used for errors</summary>
        public MobileMessageGroups(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            this.messageGroups = new List<MobileMessageGroup>();
        }

        #endregion

        #region Public methods

        public void AddMobileMessageGroups(List<MessageGroupDTO> messageGroups)
        {
            foreach (var messageGroup in messageGroups)
            {
                AddMobileMessageGroup(new MobileMessageGroup(this.Param, messageGroup.MessageGroupId, messageGroup.Name));
            }
        }

        public void AddMobileMessageGroup(MobileMessageGroup messageGroup)
        {
            if (messageGroup == null)
                return;

            messageGroups.Add(messageGroup);
        }

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            return MergeDocuments(ROOTNAME, messageGroups.Select(i => i.ToXDocument()).ToList());
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            return XmlUtil.CreateXml(ROOTNAME, MobileMessageGroup.GetDefaultXml());
        }

        #endregion
    }

    internal class MobileMessageGroup : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "MessageGroup";

        #endregion

        #region Variables

        #region Field values

        private readonly int id;
        private readonly string name;

        #endregion

        #endregion

        #region Ctor

        public MobileMessageGroup(MobileParam param)
            : base(param)
        {
            Init();
        }

        public MobileMessageGroup(MobileParam param, int id, string name)
            : base(param)
        {
            Init();
            this.id = id;
            this.name = name;
        }

        /// <summary>
        /// Used for errors
        /// </summary>
        public MobileMessageGroup(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            //Set default values
        }

        #endregion

        #region Public methods

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();

            elements.Add(new XElement("Id", this.id));
            elements.Add(new XElement("Name", this.name));

            return CreateDocument(ROOTNAME, elements);
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            string xml =
                "<Id></Id>" +
                "<Name></Name>";

            return XmlUtil.CreateXml(ROOTNAME, xml);
        }

        #endregion
    }

    #endregion

    #region EmployeeDetails


    [Log]
    internal class MobileEmployeeDetails : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "Emp";

        #endregion

        #region Variables

        #region Field values

        [LogEmployeeId]
        [LogEmployeeUnionFee]
        private readonly int id;
        private readonly string nr;
        private readonly string firstName;
        private readonly string lastName;
        private readonly int addressId;
        [LogEmployeeAddress]
        private readonly string address;
        [LogEmployeeAddress]
        private readonly string postalCode;
        [LogEmployeeAddress]
        private readonly string postalAddress;
        private readonly int[] closestRelativeId;
        [LogEmployeeClosestRelative]
        private readonly string[] closestRelativePhone;
        [LogEmployeeClosestRelative]
        private readonly string[] closestRelativeName;
        [LogEmployeeClosestRelative]
        private readonly string[] closestRelativeRelation;
        private readonly bool[] closestRelativeIsSecret;
        private readonly bool showClosestRelativeCompleteInfo;
        private readonly int mobileId;
        [LogEmployeeEcom]
        private readonly string mobileNr;
        private readonly int emailId;
        [LogEmployeeEcom]
        private readonly string email;
        private readonly int? userid;
        private readonly bool wantsExtraShifts;
        private readonly string name;

        private readonly bool showAsList = false;
        private readonly bool showOnlyIdAndName = false;
        private readonly bool showAsAvailableEmployee = false;

        #endregion

        #endregion

        #region Ctor

        public MobileEmployeeDetails(MobileParam param) : base(param)
        {

        }

        public MobileEmployeeDetails(MobileParam param, Employee employee, ContactPerson contactPerson, List<ContactECom> ecoms, List<ContactAddress> addresses) : base(param)
        {
            this.id = employee.EmployeeId;
            this.nr = employee.EmployeeNr;
            this.firstName = contactPerson != null ? contactPerson.FirstName : "";
            this.lastName = contactPerson != null ? contactPerson.LastName : "";

            List<ContactECom> emails = ecoms.Where(c => c.SysContactEComTypeId == (int)TermGroup_SysContactEComType.Email).ToList();
            List<ContactECom> mobilePhones = ecoms.Where(c => c.SysContactEComTypeId == (int)TermGroup_SysContactEComType.PhoneMobile).ToList();
            List<ContactECom> closestRelatives = ecoms.Where(c => c.SysContactEComTypeId == (int)TermGroup_SysContactEComType.ClosestRelative).ToList().GetLastCreatedOrModifiedList(2);
            List<ContactAddress> distributionAddresses = addresses.Where(c => c.SysContactAddressTypeId == (int)TermGroup_SysContactAddressType.Distribution).ToList();

            ContactECom contactMobile = mobilePhones.GetLastCreatedOrModified();
            if (contactMobile != null)
            {
                mobileId = contactMobile.ContactEComId;
                mobileNr = contactMobile.Text;
            }

            ContactECom contactEmail = emails.GetLastCreatedOrModified();
            if (contactEmail != null)
            {
                emailId = contactEmail.ContactEComId;
                email = contactEmail.Text;
            }

            closestRelativeId = new int[Constants.MOBILE_NROFCLOSESTRELATIVES];
            closestRelativePhone = new string[Constants.MOBILE_NROFCLOSESTRELATIVES];
            closestRelativeName = new string[Constants.MOBILE_NROFCLOSESTRELATIVES];
            closestRelativeRelation = new string[Constants.MOBILE_NROFCLOSESTRELATIVES];
            closestRelativeIsSecret = new bool[Constants.MOBILE_NROFCLOSESTRELATIVES];

            if (closestRelatives != null)
            {
                showClosestRelativeCompleteInfo = true;
                for (int i = 0; i < Constants.MOBILE_NROFCLOSESTRELATIVES; i++)
                {
                    ContactECom closestRelative = closestRelatives.Skip(i).FirstOrDefault();
                    if (closestRelative == null)
                        continue;

                    string[] closestRelativeDescription = closestRelative.Description != null ? closestRelative.Description.Split(';') : new string[0];
                    closestRelativeId[i] = closestRelative.ContactEComId;
                    closestRelativePhone[i] = closestRelative.Text;
                    closestRelativeName[i] = closestRelativeDescription.Length > 0 ? closestRelativeDescription[0] : String.Empty;
                    closestRelativeRelation[i] = closestRelativeDescription.Length > 1 ? closestRelativeDescription[1] : String.Empty;
                    closestRelativeIsSecret[i] = closestRelative.IsSecret;
                }
            }

            ContactAddress contactAddress = distributionAddresses.GetLastCreatedOrModified();
            if (contactAddress != null)
            {
                addressId = contactAddress.ContactAddressId;

                ContactAddressRow addressrow = contactAddress.ContactAddressRow.FirstOrDefault(c => c.SysContactAddressRowTypeId == (int)TermGroup_SysContactAddressRowType.Address);
                if (addressrow != null)
                    address = addressrow.Text;

                ContactAddressRow postalCodeRow = contactAddress.ContactAddressRow.FirstOrDefault(c => c.SysContactAddressRowTypeId == (int)TermGroup_SysContactAddressRowType.PostalCode);
                if (postalCodeRow != null)
                    postalCode = postalCodeRow.Text;

                ContactAddressRow postalAddressRow = contactAddress.ContactAddressRow.FirstOrDefault(c => c.SysContactAddressRowTypeId == (int)TermGroup_SysContactAddressRowType.PostalAddress);
                if (postalAddressRow != null)
                    postalAddress = postalAddressRow.Text;
            }
        }

        public MobileEmployeeDetails(MobileParam param, int employeeId, string firstName, string lastName, string nr, string mobile, string email, int? userid, List<ContactEcomView> closestRelatives) : base(param)
        {
            this.firstName = firstName;
            this.lastName = lastName;
            this.nr = nr;
            this.mobileNr = mobile;
            this.email = email;
            this.userid = userid;
            this.id = employeeId;

            closestRelativeId = new int[Constants.MOBILE_NROFCLOSESTRELATIVES];
            closestRelativePhone = new string[Constants.MOBILE_NROFCLOSESTRELATIVES];

            if (closestRelatives != null)
            {
                showClosestRelativeCompleteInfo = false;
                for (int i = 0; i < Constants.MOBILE_NROFCLOSESTRELATIVES; i++)
                {
                    ContactEcomView closestRelative = closestRelatives.Skip(i).FirstOrDefault();
                    if (closestRelative == null || closestRelative.HideText)
                        continue;

                    closestRelativeId[i] = closestRelative.ContactEComId;
                    closestRelativePhone[i] = closestRelative.Text;
                }
            }

            showAsList = true;
        }

        public MobileEmployeeDetails(MobileParam param, int id, string name, string nr, bool wantsExtraShifts) : base(param)
        {
            this.id = id;
            this.name = name;
            this.nr = nr;
            this.wantsExtraShifts = wantsExtraShifts;

            showAsAvailableEmployee = true;
        }

        /// <summary>
        /// Used for errors
        /// </summary>
        public MobileEmployeeDetails(MobileParam param, string errorMessage) : base(param, errorMessage)
        {

        }

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();

            elements.Add(new XElement("Id", id));

            if (showAsList)
            {
                elements.Add(new XElement("Nr", nr));
                elements.Add(new XElement("Name", firstName + " " + lastName));
                elements.Add(new XElement("FirstName", firstName));
                elements.Add(new XElement("LastName", lastName));
                elements.Add(new XElement("Mobile", mobileNr));
                elements.Add(new XElement("Email", email));
                if (userid.HasValue)
                    elements.Add(new XElement("UserId", userid.Value));
            }
            else if (showOnlyIdAndName)
            {
                elements.Add(new XElement("Name", "(" + nr + ") " + firstName + " " + lastName));
            }
            else if (showAsAvailableEmployee)
            {
                elements.Add(new XElement("Name", "(" + nr + ") " + name));
                elements.Add(new XElement("WES", StringUtility.GetString(this.wantsExtraShifts)));
            }
            else
            {
                elements.Add(new XElement("Nr", nr));
                elements.Add(new XElement("FirstName", firstName));
                elements.Add(new XElement("LastName", lastName));
                elements.Add(new XElement("AddressId", addressId));
                elements.Add(new XElement("Address", address));
                elements.Add(new XElement("PostalCode", postalCode));
                elements.Add(new XElement("PostalAddress", postalAddress));
                elements.Add(new XElement("MobileId", mobileId));
                elements.Add(new XElement("Mobile", mobileNr));
                elements.Add(new XElement("EmailId", emailId));
                elements.Add(new XElement("Email", email));
            }

            if (!showOnlyIdAndName && !showAsAvailableEmployee && closestRelativeId != null)
            {
                for (int pos = 1; pos <= Constants.MOBILE_NROFCLOSESTRELATIVES; pos++)
                {
                    int index = pos - 1;
                    string extension = pos > 1 ? pos.ToString() : String.Empty;

                    elements.Add(new XElement("ClosestRelativeId" + extension, closestRelativeId.Count() >= pos ? closestRelativeId[index] : 0));
                    elements.Add(new XElement("ClosestRelative" + extension, closestRelativePhone.Count() >= pos ? closestRelativePhone[index] : String.Empty));
                    if (showClosestRelativeCompleteInfo)
                    {
                        elements.Add(new XElement("ClosestRelativeName" + extension, closestRelativeName.Count() >= pos ? closestRelativeName[index] : String.Empty));
                        elements.Add(new XElement("ClosestRelativeRelation" + extension, closestRelativeRelation.Count() >= pos ? closestRelativeRelation[index] : String.Empty));
                        elements.Add(new XElement("ClosestRelativeIsSecret" + extension, closestRelativeIsSecret.Count() >= pos ? closestRelativeIsSecret[index].ToInt() : 0));
                    }
                }
            }

            return CreateDocument(ROOTNAME, elements);
        }

        public override XDocument ToXDocument(MobileTask task)
        {
            if (base.Failed)
                return MobileMessages.GetErrorMessageDocument(base.ErrorMessage);

            bool result = base.GetTaskResult(task);
            switch (task)
            {
                case MobileTask.SaveEmployeeDetails:
                    return MobileMessages.GetSuccessDocument(result);
                default:
                    return base.ToXDocument(task);
            }
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            string xml =
                "<Id></Id>" +
                "<Nr></Nr>" +
                "<FirstName></FirstName>" +
                "<LastName></LastName>" +
                "<AddressId></AddressId>" +
                "<Address></Address>" +
                "<PostalCode></PostalCode>" +
                "<PostalAddress></PostalAddress>" +
                "<ClosestRelativeId></ClosestRelativeId>" +
                "<ClosestRelative></ClosestRelative>" +
                "<MobileId></MobileId>" +
                "<Mobile></Mobile>" +
                "<EmailId></EmailId>" +
                "<Email></Email>";
            return XmlUtil.CreateXml(ROOTNAME, xml);
        }

        #endregion
    }

    internal class MobileEmployeeList : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "Emps";

        #endregion

        #region Variables

        private readonly List<MobileEmployeeDetails> employees = new List<MobileEmployeeDetails>();

        #endregion

        #region Ctor

        public MobileEmployeeList(MobileParam param) : base(param)
        {

        }

        public void AddToMobileEmployeeList(List<Employee> employees, List<ContactEcomView> contactEcoms, bool contactModifyPermission)
        {
            foreach (var employee in employees)
            {
                if (employee.ContactPerson == null /* || employee.ContactPerson.Actor == null*/)
                    continue;

                List<ContactEcomView> employeeEcoms = contactEcoms.Where(x => x.EmployeeId == employee.EmployeeId).ToList();

                ContactEcomView employeeEmail = employeeEcoms.Where(c => c.SysContactEComTypeId == (int)TermGroup_SysContactEComType.Email).ToList().GetLastCreatedOrModified();
                string email;
                if (employeeEmail != null)
                    email = (employeeEmail.IsSecret && !contactModifyPermission) ? string.Empty : employeeEmail.Text;
                else
                    email = string.Empty;


                ContactEcomView employeeMobile = employeeEcoms.Where(c => c.SysContactEComTypeId == (int)TermGroup_SysContactEComType.PhoneMobile).ToList().GetLastCreatedOrModified();
                string mobile;
                if (employeeMobile != null)
                    mobile = (employeeMobile.IsSecret && !contactModifyPermission) ? string.Empty : employeeMobile.Text;
                else
                    mobile = string.Empty;

                List<ContactEcomView> employeeClosestRelative = employeeEcoms.Where(c => c.SysContactEComTypeId == (int)TermGroup_SysContactEComType.ClosestRelative).ToList().GetLastCreatedOrModifiedList(2);
                if (employeeClosestRelative != null)
                {
                    foreach (var item in employeeClosestRelative)
                    {
                        if (item.IsSecret && !contactModifyPermission)
                            item.HideText = true;
                    }
                }

                AddMobileEmployeeDetails(new MobileEmployeeDetails(this.Param, employee.EmployeeId, employee.ContactPerson.FirstName, employee.ContactPerson.LastName, employee.EmployeeNr, mobile, email, employee.UserId, employeeClosestRelative));
            }
        }

        public void AddToMobileEmployeeList(List<AvailableEmployeesDTO> employees)
        {
            foreach (var employee in employees)
            {
                AddMobileEmployeeDetails(new MobileEmployeeDetails(this.Param, employee.EmployeeId, employee.EmployeeName, employee.EmployeeNr, employee.WantsExtraShifts));
            }
        }

        public void AddMobileEmployeeDetails(MobileEmployeeDetails empDetails)
        {
            if (empDetails == null)
                return;

            employees.Add(empDetails);
        }

        /// <summary>
        /// Used for errors
        /// </summary>
        public MobileEmployeeList(MobileParam param, string errorMessage) : base(param, errorMessage)
        {

        }

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            return MergeDocuments(ROOTNAME, employees.Select(i => i.ToXDocument()).ToList());
        }
        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            return XmlUtil.CreateXml(ROOTNAME, MobileEmployeeDetails.GetDefaultXml());
        }

        #endregion
    }
    #endregion

    #region EmployeeTimePeriods
    internal class MobileEmployeeTimePeriodYears : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "MobileEmployeeTimePeriodYears";

        #endregion

        #region Variables
        private bool usePayroll;
        private List<MobileEmployeeTimePeriodYear> moblieEmployeeTimePeriodsYears;

        #region Collections

        #endregion

        #endregion

        #region Ctor
        public MobileEmployeeTimePeriodYears(MobileParam param)
            : base(param)
        {
            Init();
        }

        public MobileEmployeeTimePeriodYears(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }
        private void Init()
        {
            this.moblieEmployeeTimePeriodsYears = new List<MobileEmployeeTimePeriodYear>();
        }

        #endregion

        #region Public methods

        public void AddMobileEmployeeTimePeriodsYears(List<DataStorageSmallDTO> dtos, int year, bool usePayroll)
        {
            this.usePayroll = usePayroll;
            if (dtos == null)
                return;

            if (year == 9999)
                dtos = dtos.Where(w => w.Year == 9999).ToList();

            foreach (var odto in dtos.Where(w => !w.Year.IsNullOrEmpty()).GroupBy(g => g.Year))
            {
                AddMobileEmployeeTimePeriodYears(new MobileEmployeeTimePeriodYear(this.Param, odto.Key == year ? odto.ToList() : null, odto.Key.Value));
            }
        }

        public void AddMobileEmployeeTimePeriodYears(MobileEmployeeTimePeriodYear dto)
        {
            if (dto == null)
                return;

            moblieEmployeeTimePeriodsYears.Add(dto);
        }

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>
            {
                new XElement("UsePayroll",StringUtility.GetString(usePayroll)),
            };
            return MergeDocuments(ROOTNAME, moblieEmployeeTimePeriodsYears.Select(i => i.ToXDocument()).ToList(), elements);
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            return XmlUtil.CreateXml(ROOTNAME, MobileProject.GetDefaultXml());
        }

        #endregion
    }
    internal class MobileEmployeeTimePeriodYear : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "MobileEmployeeTimePeriodYear";

        #endregion

        #region Variables
        private readonly int year;
        private List<MobileEmployeeTimePeriod> moblieEmployeeTimePeriodsYear;

        #region Collections

        #endregion

        #endregion

        #region Ctor
        public MobileEmployeeTimePeriodYear(MobileParam param)
            : base(param)
        {
            Init();
        }

        public MobileEmployeeTimePeriodYear(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        public MobileEmployeeTimePeriodYear(MobileParam param, List<DataStorageSmallDTO> dtos, int year) : base(param)
        {
            Init();
            this.year = year;
            this.AddMobileEmployeeTimePeriodsYear(dtos);
        }

        private void Init()
        {
            this.moblieEmployeeTimePeriodsYear = new List<MobileEmployeeTimePeriod>();
        }

        #endregion

        #region Public methods

        public void AddMobileEmployeeTimePeriodsYear(List<DataStorageSmallDTO> dtos)
        {
            if (dtos == null)
                return;

            foreach (var dto in dtos)
            {
                AddMobileEmployeeTimePeriodYear(new MobileEmployeeTimePeriod(this.Param, dto));
            }
        }

        public void AddMobileEmployeeTimePeriodYear(MobileEmployeeTimePeriod dto)
        {
            if (dto == null)
                return;

            moblieEmployeeTimePeriodsYear.Add(dto);
        }

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>
            {
                new XElement("Year", year)
            };

            return MergeDocuments(ROOTNAME, moblieEmployeeTimePeriodsYear.Select(i => i.ToXDocument()).ToList(), elements);
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            return XmlUtil.CreateXml(ROOTNAME, MobileProject.GetDefaultXml());
        }

        #endregion
    }

    internal class MobileEmployeeTimePeriods : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "MobileEmployeeTimePeriods";

        #endregion

        #region Variables

        #region Collections

        private List<MobileEmployeeTimePeriod> mobileEmployeeTimePeriods;

        #endregion

        #endregion

        #region Ctor
        public MobileEmployeeTimePeriods(MobileParam param)
            : base(param)
        {
            Init();
        }

        public MobileEmployeeTimePeriods(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            this.mobileEmployeeTimePeriods = new List<MobileEmployeeTimePeriod>();
        }

        #endregion

        #region Public methods

        public void AddMobileEmployeeTimePeriods(List<DataStorageAllDTO> timePeriod)
        {
            if (timePeriod == null)
                return;

            foreach (var item in timePeriod)
            {
                AddMobileEmployeeTimePeriod(new MobileEmployeeTimePeriod(this.Param, item));
            }
        }

        public void AddMobileEmployeeTimePeriod(MobileEmployeeTimePeriod timePeriod)
        {
            if (timePeriod == null)
                return;

            mobileEmployeeTimePeriods.Add(timePeriod);
        }

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            return MergeDocuments(ROOTNAME, mobileEmployeeTimePeriods.Select(i => i.ToXDocument()).ToList());
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            return XmlUtil.CreateXml(ROOTNAME, MobileProject.GetDefaultXml());
        }

        #endregion
    }

    internal class MobileEmployeeTimePeriod : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "MobileEmployeeTimePeriod";

        #endregion

        #region Variables

        #region Field values

        private readonly int? timePeriodId;
        private readonly String period;
        private readonly DateTime? paymentDate;
        private readonly decimal gross;
        private readonly decimal benefit;
        private readonly decimal tax;
        private readonly decimal compensation;
        private readonly decimal deduction;
        private readonly decimal employmentTaxDebit;
        private readonly decimal net;
        private readonly bool hasDetails = false;
        private readonly DateTime startDate;
        private readonly DateTime stopDate;
        private readonly int payrollSlipId;
        private readonly int? year;
        private readonly SoeDataStorageRecordType type;

        #endregion

        #endregion

        #region Ctor

        public MobileEmployeeTimePeriod(MobileParam param)
            : base(param)
        {
            Init();
        }
        public MobileEmployeeTimePeriod(MobileParam param, DataStorageSmallDTO item) : base(param)
        {
            Init();
            this.timePeriodId = item.TimePeriodId;
            this.period = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(item.TimePeriodStartDate.Month) + ' ' + item.TimePeriodStartDate.Year.ToString();
            this.paymentDate = item.TimePeriodPaymentDate;
            this.startDate = item.TimePeriodStartDate;
            this.stopDate = item.TimePeriodStopDate;
            this.net = item.NetSalary.HasValue ? item.NetSalary.Value : 0;
            this.type = item.Type;
            this.payrollSlipId = item.DataStorageId;

            if (item.Year.HasValue)
                this.year = item.Year.Value;
        }
        public MobileEmployeeTimePeriod(MobileParam param, DataStorageAllDTO item) : base(param)
        {
            Init();
            this.timePeriodId = item.TimePeriodId;
            this.period = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(item.TimePeriodStartDate.Month) + ' ' + item.TimePeriodStartDate.Year.ToString();
            this.paymentDate = item.TimePeriodPaymentDate;
            this.startDate = item.TimePeriodStartDate;
            this.stopDate = item.TimePeriodStopDate;
            this.payrollSlipId = item.DataStorageId;

        }
        public MobileEmployeeTimePeriod(MobileParam param, PayrollCalculationPeriodSumDTO sum, TimePeriod timePeriod, int payrollSlipDataStorageId) : base(param)
        {
            Init();
            this.hasDetails = true;
            this.timePeriodId = timePeriod.TimePeriodId;
            this.period = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(timePeriod.StartDate.Month) + ' ' + timePeriod.StartDate.Year.ToString();
            this.paymentDate = timePeriod.PaymentDate;
            this.startDate = timePeriod.StartDate;
            this.stopDate = timePeriod.StopDate;
            this.gross = sum.Gross;
            this.benefit = sum.BenefitInvertExcluded;
            this.tax = sum.Tax;
            this.compensation = sum.Compensation;
            this.deduction = sum.Deduction;
            this.employmentTaxDebit = sum.EmploymentTaxDebit;
            this.net = sum.Net;
            this.payrollSlipId = payrollSlipDataStorageId;
        }

        /// <summary>
        /// Used for errors
        /// </summary>
        public MobileEmployeeTimePeriod(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            //Set default values
        }

        #endregion

        #region Public methods

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument(MobileTask task)
        {
            if (base.Failed)
                return MobileMessages.GetErrorMessageDocument(base.ErrorMessage);

            bool result = base.GetTaskResult(task);
            switch (task)
            {
                case MobileTask.SaveEmployeeUserSettings:
                    return MobileMessages.GetSuccessDocument(result);
                default:
                    return base.ToXDocument(task);
            }
        }

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>
            {
                new XElement("TimePeriodId", timePeriodId.ToInt()),
                new XElement("Period", GetTextOrCDATA(period)),
                new XElement("PaymentDate", paymentDate.HasValue ? paymentDate.Value.GetSwedishFormattedDate() : ""),
                new XElement("StartDate", startDate.GetSwedishFormattedDate()),
                new XElement("StopDate", stopDate.GetSwedishFormattedDate()),
                new XElement("PayrollSlipId", payrollSlipId),
            };

            if (this.type == SoeDataStorageRecordType.PayrollSlipXML)
                elements.Add(new XElement("IsGOPayrollSlip", 1));

            if (hasDetails)
            {
                elements.Add(new XElement("Gross", gross));
                elements.Add(new XElement("Benefit", benefit));
                elements.Add(new XElement("Tax", tax));
                elements.Add(new XElement("Compensation", compensation));
                elements.Add(new XElement("Deduction", deduction));
                elements.Add(new XElement("EmploymentTaxDebit", employmentTaxDebit));
                elements.Add(new XElement("Net", net));
            }
            else if (year.HasValue)
            {
                elements.Add(new XElement("Year", year));
                elements.Add(new XElement("Net", net));
            }

            return CreateDocument(ROOTNAME, elements);
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            string xml =
                "<TimePeriodID></TimePeriodID>";

            return XmlUtil.CreateXml(ROOTNAME, xml);
        }

        #endregion
    }

    #endregion

    #region EmployeeUserSettings

    internal class MobileEmployeeUserSettings : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "MobileEmployeeUserSettings";

        #endregion

        #region Variables

        #region Field values

        public int EmployeeId;
        public bool WantsExtraShifts;
        public bool ShowWantsExtraShifts;

        #endregion

        #endregion

        #region Ctor

        public MobileEmployeeUserSettings(MobileParam param)
            : base(param)
        {
            Init();
        }

        public MobileEmployeeUserSettings(MobileParam param, int employeeId, bool wantsExtraShifts, bool showWantsExtraShifts) : base(param)
        {
            Init();
            this.EmployeeId = employeeId;
            this.WantsExtraShifts = wantsExtraShifts;
            this.ShowWantsExtraShifts = showWantsExtraShifts;
        }

        /// <summary>
        /// Used for errors
        /// </summary>
        public MobileEmployeeUserSettings(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            //Set default values
        }

        #endregion

        #region Public methods

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument(MobileTask task)
        {
            if (base.Failed)
                return MobileMessages.GetErrorMessageDocument(base.ErrorMessage);

            bool result = base.GetTaskResult(task);
            switch (task)
            {
                case MobileTask.SaveEmployeeUserSettings:
                    return MobileMessages.GetSuccessDocument(result);
                default:
                    return base.ToXDocument(task);
            }
        }

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();

            elements.Add(new XElement("EmployeeId", this.EmployeeId));
            elements.Add(new XElement("UserId", this.UserId));

            elements.Add(new XElement("ShowWantsExtraShifts", this.ShowWantsExtraShifts.ToInt()));
            if (this.ShowWantsExtraShifts)
                elements.Add(new XElement("WantsExtraShifts", this.WantsExtraShifts.ToInt()));

            return CreateDocument(ROOTNAME, elements);
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            string xml =
                "<EmployeeId></EmployeeId>" +
                "<UserId></UserId>" +
                "<WantsExtraShifts></WantsExtraShifts>";

            return XmlUtil.CreateXml(ROOTNAME, xml);
        }

        #endregion
    }

    #endregion

    #region WorkDescription

    internal class MobileWorkDescription : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "WorkDescription";

        #endregion

        #region Variables

        #region Field values

        private readonly int id;
        private readonly string workDescription;

        #endregion

        #endregion

        #region Ctor

        public MobileWorkDescription(MobileParam param)
            : base(param)
        {
            Init();
        }

        public MobileWorkDescription(MobileParam param, int id, string workDescription)
            : base(param)
        {
            Init();
            this.id = id;
            this.workDescription = workDescription;
        }

        /// <summary>
        /// Used for errors
        /// </summary>
        public MobileWorkDescription(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            //Set default values
        }

        #endregion

        #region Public methods

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument(MobileTask task)
        {
            if (base.Failed)
                return MobileMessages.GetErrorMessageDocument(base.ErrorMessage);

            bool result = base.GetTaskResult(task);
            switch (task)
            {
                case MobileTask.SaveWorkingDescription:
                    return MobileMessages.GetSuccessDocument(result);
                default:
                    return base.ToXDocument(task);
            }
        }

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();

            elements.Add(new XElement("OrderId", this.id));
            elements.Add(new XElement("WorkDescriptionText", GetTextOrCDATA(this.workDescription)));

            return CreateDocument(ROOTNAME, elements);
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            string xml =
                "<OrderId></OrderId>" +
                "<WorkDescription></Text>";

            return XmlUtil.CreateXml(ROOTNAME, xml);
        }

        #endregion
    }

    #endregion

    #region UserCompanies

    internal class MobileUserCompanies : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "UserCompanies";

        #endregion

        #region Variables

        #region Collections

        private List<MobileUserCompany> mobileUserCompanies;

        #endregion

        #endregion

        #region Ctor

        public MobileUserCompanies(MobileParam param, Dictionary<int, string> dict)
            : base(param)
        {
            Init();
            AddMobileUserCompanies(dict);
        }

        /// <summary>Used for errors</summary>
        public MobileUserCompanies(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            this.mobileUserCompanies = new List<MobileUserCompany>();
        }

        #endregion

        #region Public methods

        public void AddMobileUserCompanies(Dictionary<int, string> dict)
        {
            if (dict == null)
                return;

            foreach (var item in dict)
            {
                AddMobileContactPerson(new MobileUserCompany(this.Param, item.Key, item.Value));
            }
        }

        public void AddMobileContactPerson(MobileUserCompany userCompany)
        {
            if (userCompany == null)
                return;

            mobileUserCompanies.Add(userCompany);
        }

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            return MergeDocuments(ROOTNAME, mobileUserCompanies.Select(i => i.ToXDocument()).ToList());
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            return XmlUtil.CreateXml(ROOTNAME, MobileUserCompany.GetDefaultXml());
        }

        #endregion
    }

    internal class MobileUserCompany : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "UserCompany";

        #endregion

        #region Variables

        #region Field values

        private readonly int id;
        private readonly String name;

        #endregion

        #endregion

        #region Ctor

        public MobileUserCompany(MobileParam param, int id, String name)
            : base(param)
        {
            Init();

            this.id = id;
            this.name = name;

        }

        /// <summary>
        /// Used for errors
        /// </summary>
        public MobileUserCompany(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            //Set default values
        }

        #endregion

        #region Public methods

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();

            elements.Add(new XElement("CompanyId", id.ToString()));
            elements.Add(new XElement("Name", name));

            return CreateDocument(ROOTNAME, elements);
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            string xml =
                "<CompanyId>1</CompanyId>" +
                "<Name>Peter Forsberg</Name>";
            return XmlUtil.CreateXml(ROOTNAME, xml);
        }

        #endregion
    }

    #endregion

    #region UserCompanyRole

    internal class MobileUserCompanyRoles : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "UserCompanyRoles";

        #endregion

        #region Variables

        #region Collections

        private List<MobileUserCompanyRole> mobileUserCompanyRoles;

        #endregion

        #endregion

        #region Ctor

        public MobileUserCompanyRoles(MobileParam param, Dictionary<int, string> dict)
            : base(param)
        {
            Init();
            AddMobileUserCompanyRoles(dict);
        }

        /// <summary>Used for errors</summary>
        public MobileUserCompanyRoles(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            this.mobileUserCompanyRoles = new List<MobileUserCompanyRole>();
        }

        #endregion

        #region Public methods

        public void AddMobileUserCompanyRoles(Dictionary<int, string> dict)
        {
            if (dict == null)
                return;

            foreach (var item in dict)
            {
                AddMobileUserCompanyRole(new MobileUserCompanyRole(this.Param, item.Key, item.Value));
            }
        }

        public void AddMobileUserCompanyRole(MobileUserCompanyRole userCompanyRole)
        {
            if (userCompanyRole == null)
                return;

            mobileUserCompanyRoles.Add(userCompanyRole);
        }

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            return MergeDocuments(ROOTNAME, mobileUserCompanyRoles.Select(i => i.ToXDocument()).ToList());
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            return XmlUtil.CreateXml(ROOTNAME, MobileUserCompanyRole.GetDefaultXml());
        }

        #endregion
    }

    internal class MobileUserCompanyRole : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "UserCompanyRole";

        #endregion

        #region Variables

        #region Field values

        private readonly int id;
        private readonly String name;

        #endregion

        #endregion

        #region Ctor

        public MobileUserCompanyRole(MobileParam param)
            : base(param)
        {
            Init();

        }

        public MobileUserCompanyRole(MobileParam param, int id, String name)
            : base(param)
        {
            Init();

            this.id = id;
            this.name = name;

        }

        /// <summary>
        /// Used for errors
        /// </summary>
        public MobileUserCompanyRole(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            //Set default values
        }

        #endregion

        #region Public methods

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument(MobileTask task)
        {
            if (base.Failed)
                return MobileMessages.GetErrorMessageDocument(base.ErrorMessage);

            bool result = base.GetTaskResult(task);
            switch (task)
            {
                case MobileTask.UserCompanyRoleIsValid:
                    return MobileMessages.GetSuccessDocument(result);
                default:
                    return base.ToXDocument(task);
            }
        }

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();

            elements.Add(new XElement("RoleId", id.ToString()));
            elements.Add(new XElement("Name", name));

            return CreateDocument(ROOTNAME, elements);
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            string xml =
                "<RoleId>1</RoleId>" +
                "<Name>Peter Forsberg</Name>";
            return XmlUtil.CreateXml(ROOTNAME, xml);
        }

        #endregion
    }

    #endregion

    #region Project

    internal class MobileProjects : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "Projects";

        #endregion

        #region Variables

        #region Collections

        private List<MobileProject> mobileProjects;

        #endregion

        #endregion

        #region Ctor

        public MobileProjects(MobileParam param, List<ProjectTinyDTO> projects) : base(param)
        {
            Init();
            AddMobileProjects(projects);
        }
        public MobileProjects(MobileParam param, List<ProjectSmallDTO> projects) : base(param)
        {
            Init();
            AddMobileProjects(projects);
        }
        public MobileProjects(MobileParam param, List<ProjectSearchResultDTO> projects) : base(param)
        {
            Init();
            AddMobileProjects(projects);
        }

        /// <summary>Used for errors</summary>
        public MobileProjects(MobileParam param, string errorMessage) : base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            mobileProjects = new List<MobileProject>();
        }

        #endregion

        #region Public methods

        public void AddMobileProjects(List<ProjectTinyDTO> projects)
        {
            if (projects == null)
                return;

            foreach (var item in projects)
            {
                AddMobileProject(new MobileProject(this.Param, item.ProjectId, item.Number, item.Name));
            }
        }
        public void AddMobileProjects(List<ProjectSmallDTO> projects)
        {
            if (projects == null)
                return;

            foreach (var item in projects)
            {
                AddMobileProject(new MobileProject(this.Param, item.ProjectId, item.Number, item.Name, item.CustomerNumber, item.CustomerName));
            }
        }
        public void AddMobileProjects(List<ProjectSearchResultDTO> projects)
        {
            if (projects == null)
                return;

            foreach (var item in projects)
            {
                AddMobileProject(new MobileProject(this.Param, item.ProjectId, item.Number, item.Name, item.Number, item.CustomerName));
            }
        }
        public void AddMobileProject(MobileProject mobileProject)
        {
            if (mobileProject == null)
                return;

            mobileProjects.Add(mobileProject);
        }

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            return MergeDocuments(ROOTNAME, mobileProjects.Select(i => i.ToXDocument()).ToList());
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            return XmlUtil.CreateXml(ROOTNAME, MobileProject.GetDefaultXml());
        }

        #endregion
    }

    internal class MobileProject : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "Project";

        #endregion

        #region Variables

        #region Field values

        private readonly int id;
        private readonly string name;
        private readonly string nr;
        private readonly string customerNr;
        private readonly string customerName;

        #endregion

        #endregion

        #region Ctor

        public MobileProject(MobileParam param) : base(param)
        {
            Init();

        }

        public MobileProject(MobileParam param, int id, string nr, string name) : base(param)
        {
            Init();

            this.id = id;
            this.name = name;
            this.nr = nr;

        }
        public MobileProject(MobileParam param, int id, string nr, string name, string customerNumber, string customerName) : base(param)
        {
            Init();

            this.id = id;
            this.name = name;
            this.nr = nr;
            this.customerNr = customerNumber;
            this.customerName = customerName;
        }
        /// <summary>
        /// Used for errors
        /// </summary>
        public MobileProject(MobileParam param, string errorMessage) : base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            //Set default values
        }

        #endregion

        #region Public methods

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument(MobileTask task)
        {
            if (base.Failed)
                return MobileMessages.GetErrorMessageDocument(base.ErrorMessage);

            bool result = base.GetTaskResult(task);
            switch (task)
            {
                case MobileTask.ChangeProjectOnOrder:
                    return MobileMessages.GetSuccessDocument(result);
                default:
                    return base.ToXDocument(task);
            }
        }

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();

            elements.Add(new XElement("ProjectId", id.ToString()));
            elements.Add(new XElement("Nr", nr));
            elements.Add(new XElement("Name", name));
            elements.Add(new XElement("CustomerNr", customerNr));
            elements.Add(new XElement("CustomerName", customerName));

            return CreateDocument(ROOTNAME, elements);
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            string xml =
                "<ProjectId>1</ProjectId>" +
                "<Name>Peter Forsberg</Name>";
            return XmlUtil.CreateXml(ROOTNAME, xml);
        }

        #endregion
    }

    #endregion

    #region PreCreateOrder

    internal class MobilePreCreateOrder : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "PreCreateOrder";

        #endregion

        #region Variables

        #region Field values

        private readonly bool addProject = false;
        private readonly bool addTemplate = false;

        #endregion

        #endregion

        #region Ctor

        public MobilePreCreateOrder(MobileParam param)
            : base(param)
        {
            Init();

        }

        public MobilePreCreateOrder(MobileParam param, bool addProject, bool addTemplate)
            : base(param)
        {
            Init();

            this.addProject = addProject;
            this.addTemplate = addTemplate;

        }

        /// <summary>
        /// Used for errors
        /// </summary>
        public MobilePreCreateOrder(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            //Set default values
        }

        #endregion

        #region Public methods

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();

            elements.Add(new XElement("AddProject", StringUtility.GetString(this.addProject)));
            elements.Add(new XElement("AddTemplate", StringUtility.GetString(this.addTemplate)));

            return CreateDocument(ROOTNAME, elements);
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            string xml =
                "<AddProject>1/0</AddProject>" +
                "<AddTemplate>1/0</AddTemplate>";

            return XmlUtil.CreateXml(ROOTNAME, xml);
        }

        #endregion
    }
    #endregion

    #region PlanningData

    internal class MobilePlanningData : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "PlanningData";

        #endregion

        #region Variables

        #endregion

        readonly CustomerInvoice order;
        readonly string shiftTypeName;

        #region Ctor

        public MobilePlanningData(MobileParam param)
            : base(param)
        {
            Init();

        }

        public MobilePlanningData(MobileParam param, CustomerInvoice order, string shiftTypeName)
            : base(param)
        {
            Init();

            this.order = order;
            this.shiftTypeName = shiftTypeName;
        }

        /// <summary>
        /// Used for errors
        /// </summary>
        public MobilePlanningData(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            //Set default values
        }

        #endregion

        #region Public methods

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument(MobileTask task)
        {
            if (base.Failed)
                return MobileMessages.GetErrorMessageDocument(base.ErrorMessage);

            bool result = base.GetTaskResult(task);
            switch (task)
            {
                case MobileTask.SavePlanningData:
                    return MobileMessages.GetSuccessDocument(result);
                default:
                    return base.ToXDocument(task);
            }
        }

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();
            if (!this.Failed)
            {
                elements.Add(new XElement("ShiftTypeId", this.order.ShiftTypeId.HasValue ? this.order.ShiftTypeId.Value.ToString() : ""));//OBS! can be null
                elements.Add(new XElement("ShiftTypeName", this.shiftTypeName));
                elements.Add(new XElement("StartDate", this.order.PlannedStartDate.HasValue ? this.order.PlannedStartDate.Value.ToShortDateString() : ""));//OBS! can be null
                elements.Add(new XElement("StopDate", this.order.PlannedStopDate.HasValue ? this.order.PlannedStopDate.Value.ToShortDateString() : ""));//OBS! can be null
                elements.Add(new XElement("EstimatedTime", this.order.EstimatedTime));
                elements.Add(new XElement("RemainingTime", this.order.RemainingTime));
                elements.Add(new XElement("Priority", this.order.Priority.HasValue ? this.order.Priority.Value.ToString() : "")); //OBS! can be null
            }
            return CreateDocument(ROOTNAME, elements);
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            string xml = string.Empty;

            return XmlUtil.CreateXml(ROOTNAME, xml);
        }

        #endregion
    }

    #endregion

    #region SupplierInvoices Attest

    internal class MobileAttestInvoices : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "AttestInvoices";

        #endregion

        #region Variables

        #region Collections

        private List<MobileAttestInvoice> mobileAttestInvoices;

        #endregion

        #endregion

        #region Ctor



        public MobileAttestInvoices(MobileParam param, List<AttestWorkFlowOverviewGridDTO> invoices) : base(param)
        {
            Init();
            AddMobileAttestInvoices(invoices);
        }


        /// <summary>Used for errors</summary>
        public MobileAttestInvoices(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            this.mobileAttestInvoices = new List<MobileAttestInvoice>();
        }

        #endregion

        #region Public methods

        public void AddMobileAttestInvoices(List<AttestWorkFlowOverviewGridDTO> invoices)
        {
            if (invoices == null)
                return;

            foreach (var item in invoices)
            {
                AddMobileAttestInvoice(new MobileAttestInvoice(this.Param, item));
            }
        }

        public void AddMobileAttestInvoice(MobileAttestInvoice mobileAttestInvoice)
        {
            if (mobileAttestInvoice == null)
                return;

            mobileAttestInvoices.Add(mobileAttestInvoice);
        }

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            return MergeDocuments(ROOTNAME, mobileAttestInvoices.Select(i => i.ToXDocument()).ToList());
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            return XmlUtil.CreateXml(ROOTNAME, MobileProject.GetDefaultXml());
        }

        #endregion
    }

    internal class MobileAttestInvoice : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "AttestInvoice";

        #endregion

        #region Variables

        private readonly bool isAttestView = false;

        #region Field values

        private readonly int invoiceId;
        private readonly int finvoiceEdiEntryId;

        private readonly string invoiceNr;
        private readonly string supplierName;
        private readonly string text;
        private readonly string currency;
        private readonly string attestComments;
        private readonly decimal amount;
        private readonly decimal vatAmount;
        private DateTime? expiryDate;
        private DateTime? invoiceDate;
        private readonly string fileType;
        private readonly bool blockPayment;
        private readonly string blockReason;

        private readonly int attestWorkFlowHeadId;
        private readonly int attestWorkFlowRowId;
        private readonly string imagePath;
        private readonly bool changeAccountPermission;
        private readonly bool changeAccountInternalPermission;
        private readonly bool linkToOrderPermssion;
        private readonly bool linkToProjectPermission;

        private MobileAttestInvoiceAccountingRows accountingRows;

        #endregion

        #endregion

        #region Ctor

        public MobileAttestInvoice(MobileParam param) : base(param)
        {

        }

        public MobileAttestInvoice(MobileParam param, AttestWorkFlowOverviewGridDTO item) : base(param)
        {
            this.invoiceId = item.InvoiceId;
            this.invoiceNr = item.InvoiceNr;
            this.supplierName = item.SupplierNr + " " + item.SupplierName;
            this.text = item.InternalDescription;
            this.currency = item.Currency;
            this.attestComments = item.AttestComments;
            this.amount = item.TotalAmount;
            this.expiryDate = item.DueDate;
            this.invoiceDate = item.InvoiceDate;
            this.blockPayment = item.BlockPayment;
        }

        public MobileAttestInvoice(MobileParam param, SupplierInvoiceDTO supplierInvoice, List<AccountDim> dims, int attestWorkFlowHeadId, int attestWorkFlowRowId, string imagePath, string fileType, bool changeAccountPermission, bool changeAccountInternalPermission, bool linkToOrderPermission, bool linkToProjectPermission) : base(param)
        {
            this.isAttestView = true;

            this.invoiceId = supplierInvoice.InvoiceId;
            this.amount = supplierInvoice.TotalAmount;
            this.vatAmount = supplierInvoice.VatAmount;
            this.attestWorkFlowHeadId = attestWorkFlowHeadId;
            this.attestWorkFlowRowId = attestWorkFlowRowId;
            this.imagePath = imagePath;
            this.fileType = fileType;
            this.changeAccountPermission = changeAccountPermission;
            this.changeAccountInternalPermission = changeAccountInternalPermission;
            this.linkToOrderPermssion = linkToOrderPermission;
            this.linkToProjectPermission = linkToProjectPermission;
            this.finvoiceEdiEntryId = supplierInvoice.EdiEntryId;
            this.blockPayment = supplierInvoice.BlockPayment;
            this.blockReason = supplierInvoice.BlockReason;

            AddAccountingRows(param, supplierInvoice, dims);
        }

        /// <summary>
        /// Used for errors
        /// </summary>
        public MobileAttestInvoice(MobileParam param, string errorMessage) : base(param, errorMessage)
        {

        }

        #endregion

        #region Public methods

        public void AddAccountingRows(MobileParam param, SupplierInvoiceDTO invoice, List<AccountDim> dims)
        {
            if (invoice != null && invoice.SupplierInvoiceRows != null && dims != null)
            {
                accountingRows = new MobileAttestInvoiceAccountingRows(param);
                foreach (SupplierInvoiceRowDTO invoiceRowDTO in invoice.SupplierInvoiceRows.Where(x => x.State == (int)SoeEntityState.Active))
                {
                    accountingRows.AddMobileAttestInvoiceAccountingRows(invoiceRowDTO.AccountingRows, dims);
                }
            }
        }

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument(MobileTask task)
        {
            if (base.Failed)
                return MobileMessages.GetErrorMessageDocument(base.ErrorMessage);

            bool result = base.GetTaskResult(task);
            switch (task)
            {
                case MobileTask.SaveAttestWorkFlowAnswer:
                    return MobileMessages.GetSuccessDocument(result);
                default:
                    return base.ToXDocument(task);
            }
        }

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();

            elements.Add(new XElement("InvoiceId", this.invoiceId));
            elements.Add(new XElement("BlockPayment", StringUtility.GetString(this.blockPayment)));

            if (finvoiceEdiEntryId > 0)
            {
                elements.Add(new XElement("FEntryId", this.finvoiceEdiEntryId));
            }

            if (isAttestView)
            {

                elements.Add(new XElement("AttestWorkFlowHeadId", this.attestWorkFlowHeadId));
                elements.Add(new XElement("AttestWorkFlowRowId", this.attestWorkFlowRowId));
                elements.Add(new XElement("ImagePath", this.imagePath));
                elements.Add(new XElement("FileType", this.fileType));
                elements.Add(new XElement("AllowCA", this.changeAccountPermission ? "1" : "0"));
                elements.Add(new XElement("AllowCAI", this.changeAccountInternalPermission ? "1" : "0"));
                elements.Add(new XElement("AllowLinkToOrder", this.linkToOrderPermssion ? "1" : "0"));
                elements.Add(new XElement("AllowLinkToProject", this.linkToProjectPermission ? "1" : "0"));
                elements.Add(new XElement("BlockReason", this.blockReason));
                elements.Add(new XElement("Amount", this.amount.ToString()));
                elements.Add(new XElement("VatAmount", this.vatAmount.ToString()));

                if (accountingRows != null)
                {
                    elements.Add(accountingRows.ToXElement());
                }
            }
            else
            {
                elements.Add(new XElement("InvoiceNr", this.invoiceNr));
                elements.Add(new XElement("Supplier", this.supplierName));
                elements.Add(new XElement("Text", this.text));
                elements.Add(new XElement("Currency", this.currency));
                elements.Add(new XElement("AttestComments", this.GetTextOrCDATA(this.attestComments)));
                elements.Add(new XElement("Amount", this.amount.ToString()));
                if (mobileManagerUtil.IsCallerExpectedVersionNewerThenGivenVersion(Param.Version, Constants.MOBILE_WS_SUPPORTED_OLDVERSION_38))
                {
                    elements.Add(new XElement("ExpiryDate", this.expiryDate.HasValue ? StringUtility.GetSwedishFormattedDate(this.expiryDate.Value) : "" ));
                    elements.Add(new XElement("InvoiceDate", this.invoiceDate.HasValue ? StringUtility.GetSwedishFormattedDate(this.invoiceDate.Value) : ""));
                }
                else
                {
                    elements.Add(new XElement("ExpiryDate", this.expiryDate.HasValue ? this.expiryDate.Value.ToShortDateString() : ""));
                    elements.Add(new XElement("InvoiceDate", this.invoiceDate.HasValue ? this.invoiceDate.Value.ToShortDateString() : ""));
                }
            }

            return CreateDocument(ROOTNAME, elements);
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            return XmlUtil.CreateXml(ROOTNAME, MobileProject.GetDefaultXml());
        }

        #endregion
    }

    internal class MobileAttestInvoiceAccountingRows : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "AccountingRows";

        #endregion

        #region Variables

        #region Collections

        readonly private List<MobileAttestInvoiceAccountingRow> mobileAttestInvoiceAccountingRows = new List<MobileAttestInvoiceAccountingRow>();

        #endregion

        #endregion

        #region Ctor

        public MobileAttestInvoiceAccountingRows(MobileParam param) : base(param)
        {

        }

        /// <summary>Used for errors</summary>
        public MobileAttestInvoiceAccountingRows(MobileParam param, string errorMessage) : base(param, errorMessage)
        {

        }


        #endregion

        #region Public methods

        public void AddMobileAttestInvoiceAccountingRows(List<AccountingRowDTO> invoices, List<AccountDim> dims)
        {
            if (invoices == null || dims == null)
                return;

            foreach (var item in invoices)
            {
                AddMobileAttestInvoiceAccountingRow(new MobileAttestInvoiceAccountingRow(Param, item, dims));
            }
        }

        public void AddMobileAttestInvoiceAccountingRow(MobileAttestInvoiceAccountingRow mobileAttestInvoiceAccountingRow)
        {
            // Only add if type is AccountingRow and not deleted
            if (mobileAttestInvoiceAccountingRow == null || mobileAttestInvoiceAccountingRow.isDeleted || mobileAttestInvoiceAccountingRow.type != AccountingRowType.AccountingRow)
                return;

            mobileAttestInvoiceAccountingRows.Add(mobileAttestInvoiceAccountingRow);
        }

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();

            return CreateDocument(ROOTNAME, elements);
        }

        public XElement ToXElement()
        {
            XElement element = new XElement(ROOTNAME);

            decimal totalDebit = 0;
            decimal totalCredit = 0;

            foreach (var accountingRow in mobileAttestInvoiceAccountingRows)
            {
                totalDebit += accountingRow.debitAmount;
                totalCredit += accountingRow.creditAmount;

                element.Add(accountingRow.ToXElement());
            }

            element.Add(new XElement("TotalDebitAmount", totalDebit));
            element.Add(new XElement("TotalCreditAmount", totalCredit));

            return element;
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            return XmlUtil.CreateXml(ROOTNAME, MobileProject.GetDefaultXml());
        }

        #endregion
    }

    internal class MobileAttestInvoiceAccountingRow : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "AccountingRow";

        #endregion

        #region Variables

        #region Field values

        public readonly bool isDeleted;
        public readonly AccountingRowType type;
        public readonly decimal creditAmount;
        public readonly decimal debitAmount;

        private readonly string dim1Caption;
        private readonly string dim2Caption;
        private readonly string dim3Caption;
        private readonly string dim4Caption;
        private readonly string dim5Caption;
        private readonly string dim6Caption;

        private readonly int dim1Id;
        private readonly string dim1Name;
        private readonly string dim1Nr;
        private readonly bool dim1Disabled;
        private readonly bool dim1Mandatory;

        private readonly int dim2Id;
        private readonly string dim2Name;
        private readonly string dim2Nr;
        private readonly bool dim2Disabled;
        private readonly bool dim2Mandatory;

        private readonly int dim3Id;
        private readonly string dim3Name;
        private readonly string dim3Nr;
        private readonly bool dim3Disabled;
        private readonly bool dim3Mandatory;

        private readonly int dim4Id;
        private readonly string dim4Name;
        private readonly string dim4Nr;
        private readonly bool dim4Disabled;
        private readonly bool dim4Mandatory;

        private readonly int dim5Id;
        private readonly string dim5Name;
        private readonly string dim5Nr;
        private readonly bool dim5Disabled;
        private readonly bool dim5Mandatory;

        private readonly int dim6Id;
        private readonly string dim6Name;
        private readonly string dim6Nr;
        private readonly bool dim6Disabled;
        private readonly bool dim6Mandatory;

        private readonly bool isDebitRow;
        private readonly bool isCreditRow;
        private readonly int invoiceAccountRowId;

        #endregion

        #endregion

        #region Ctor

        public MobileAttestInvoiceAccountingRow(MobileParam param, AccountingRowDTO item, List<AccountDim> dims) : base(param)
        {
            invoiceAccountRowId = item.InvoiceAccountRowId;

            dim1Id = item.Dim1Id;
            dim1Name = item.Dim1Name;
            dim1Nr = item.Dim1Nr;
            dim1Disabled = item.Dim1Disabled;
            dim1Mandatory = item.Dim1Mandatory;

            dim2Id = item.Dim2Id;
            dim2Name = item.Dim2Name;
            dim2Nr = item.Dim2Nr;
            dim2Disabled = item.Dim2Disabled;
            dim2Mandatory = item.Dim2Mandatory;

            dim3Id = item.Dim3Id;
            dim3Name = item.Dim3Name;
            dim3Nr = item.Dim3Nr;
            dim3Disabled = item.Dim3Disabled;
            dim3Mandatory = item.Dim3Mandatory;

            dim4Id = item.Dim4Id;
            dim4Name = item.Dim4Name;
            dim4Nr = item.Dim4Nr;
            dim4Disabled = item.Dim4Disabled;
            dim4Mandatory = item.Dim4Mandatory;

            dim5Id = item.Dim5Id;
            dim5Name = item.Dim5Name;
            dim5Nr = item.Dim5Nr;
            dim5Disabled = item.Dim5Disabled;
            dim5Mandatory = item.Dim5Mandatory;

            dim6Id = item.Dim6Id;
            dim6Name = item.Dim6Name;
            dim6Nr = item.Dim6Nr;
            dim6Disabled = item.Dim6Disabled;
            dim6Mandatory = item.Dim6Mandatory;

            creditAmount = Math.Abs(item.CreditAmount);
            debitAmount = Math.Abs(item.DebitAmount);
            isCreditRow = item.IsCreditRow;
            isDebitRow = item.IsDebitRow;
            isDeleted = item.IsDeleted;
            type = item.Type;

            foreach (AccountDim dim in dims)
            {
                switch (dim.AccountDimNr)
                {
                    case 1:
                        dim1Caption = dim.Name;
                        break;
                    case 2:
                        dim2Caption = dim.Name;
                        break;
                    case 3:
                        dim3Caption = dim.Name;
                        break;
                    case 4:
                        dim4Caption = dim.Name;
                        break;
                    case 5:
                        dim5Caption = dim.Name;
                        break;
                    case 6:
                        dim6Caption = dim.Name;
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// Used for errors
        /// </summary>
        public MobileAttestInvoiceAccountingRow(MobileParam param, string errorMessage) : base(param, errorMessage)
        {

        }


        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();

            return CreateDocument(ROOTNAME, elements);
        }

        public XElement ToXElement()
        {
            XElement element = new XElement(ROOTNAME);

            element.Add(new XElement("Dim1Id", dim1Id));
            element.Add(new XElement("Dim1Caption", dim1Caption));
            element.Add(new XElement("Dim1Name", dim1Name));
            element.Add(new XElement("Dim1Nr", dim1Nr));
            element.Add(new XElement("Dim1Disabled", dim1Disabled));
            element.Add(new XElement("Dim1Mandatory", dim1Mandatory));

            element.Add(new XElement("Dim2Id", dim2Id));
            element.Add(new XElement("Dim2Caption", dim2Caption));
            element.Add(new XElement("Dim2Name", dim2Name));
            element.Add(new XElement("Dim2Nr", dim2Nr));
            element.Add(new XElement("Dim2Disabled", dim2Disabled));
            element.Add(new XElement("Dim2Mandatory", dim2Mandatory));

            element.Add(new XElement("Dim3Id", dim3Id));
            element.Add(new XElement("Dim3Caption", dim3Caption));
            element.Add(new XElement("Dim3Name", dim3Name));
            element.Add(new XElement("Dim3Nr", dim3Nr));
            element.Add(new XElement("Dim3Disabled", dim3Disabled));
            element.Add(new XElement("Dim3Mandatory", dim3Mandatory));

            element.Add(new XElement("Dim4Id", dim4Id));
            element.Add(new XElement("Dim4Caption", dim4Caption));
            element.Add(new XElement("Dim4Name", dim4Name));
            element.Add(new XElement("Dim4Nr", dim4Nr));
            element.Add(new XElement("Dim4Disabled", dim4Disabled));
            element.Add(new XElement("Dim4Mandatory", dim4Mandatory));

            element.Add(new XElement("Dim5Id", dim5Id));
            element.Add(new XElement("Dim5Caption", dim5Caption));
            element.Add(new XElement("Dim5Name", dim5Name));
            element.Add(new XElement("Dim5Nr", dim5Nr));
            element.Add(new XElement("Dim5Disabled", dim5Disabled));
            element.Add(new XElement("Dim5Mandatory", dim5Mandatory));

            element.Add(new XElement("Dim6Id", dim6Id));
            element.Add(new XElement("Dim6Caption", dim6Caption));
            element.Add(new XElement("Dim6Name", dim6Name));
            element.Add(new XElement("Dim6Nr", dim6Nr));
            element.Add(new XElement("Dim6Disabled", dim6Disabled));
            element.Add(new XElement("Dim6Mandatory", dim6Mandatory));

            element.Add(new XElement("CreditAmount", creditAmount));
            element.Add(new XElement("DebitAmount", debitAmount));
            element.Add(new XElement("IsCreditRow", isCreditRow));
            element.Add(new XElement("IsDebitRow", isDebitRow));
            element.Add(new XElement("RowId", invoiceAccountRowId));

            return element;
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            string xml =
                "<>1</>" +
                "<>Peter Forsberg</>";
            return XmlUtil.CreateXml(ROOTNAME, xml);
        }

        #endregion
    }
    internal class MobileAttestInvoiceCostTransferRows : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "CostTransferRows";

        #endregion

        #region Variables

        #region Collections

        readonly private List<MobileAttestInvoiceCostTransferRow> mobileAttestInvoiceOrderProjectRowsGrid = new List<MobileAttestInvoiceCostTransferRow>();

        #endregion

        #endregion

        #region Ctor

        public MobileAttestInvoiceCostTransferRows(MobileParam param, List<SupplierInvoiceCostTransferForGridDTO> orderProjectRows) : base(param)
        {
            Init();
            AddMobileOrderProjectRows(orderProjectRows);
        }

        /// <summary>
        /// Used for errors
        /// </summary>
        public MobileAttestInvoiceCostTransferRows(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            //Set default values
        }

        #endregion

        #region Public methods
        public void AddMobileOrderProjectRows(List<SupplierInvoiceCostTransferForGridDTO> orderProjectRows)
        {
            if (orderProjectRows == null)
                return;

            foreach (var item in orderProjectRows)
            {
                mobileAttestInvoiceOrderProjectRowsGrid.Add(new MobileAttestInvoiceCostTransferRow(this.Param, item));
            }
        }
        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            return MergeDocuments(ROOTNAME, mobileAttestInvoiceOrderProjectRowsGrid.Select(i => i.ToXDocument()).ToList());
        }
        #endregion
    }
    internal class MobileAttestInvoiceCostTransferRow : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "CostTransferRow";

        #endregion

        #region Variables

        public readonly int recordType;
        public readonly int recordId;
        public readonly string orderNumber;
        public readonly string orderName;
        public readonly string projectNumber;
        public readonly string projectName;
        public readonly string timeCodeName;
        public readonly decimal amount;
        public readonly decimal sumAmount;
        public readonly decimal supplementCharge;

        #endregion

        #region Ctor

        public MobileAttestInvoiceCostTransferRow(MobileParam param, SupplierInvoiceCostTransferForGridDTO item) : base(param)
        {
            recordType = (int)item.Type;
            recordId = item.RecordId;
            orderNumber = item.OrderNumber;
            orderName = item.OrderName;
            projectNumber = item.ProjectNumber;
            projectName = item.ProjectName;
            timeCodeName = item.TimeCodeName;
            amount = item.Amount;
            sumAmount = item.SumAmount;
            supplementCharge = item.SupplementCharge;
        }

        /// <summary>
        /// Used for errors
        /// </summary>
        public MobileAttestInvoiceCostTransferRow(MobileParam param, string errorMessage) : base(param, errorMessage)
        {

        }


        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();
            elements.Add(new XElement("Type", recordType));
            elements.Add(new XElement("RecordId", recordId));
            elements.Add(new XElement("OrderNumber", orderNumber));
            elements.Add(new XElement("OrderName", orderName));
            elements.Add(new XElement("ProjectNumber", projectNumber));
            elements.Add(new XElement("ProjectName", projectName));
            elements.Add(new XElement("TimeCodeName", timeCodeName));
            elements.Add(new XElement("Amount", amount));
            elements.Add(new XElement("SumAmount", sumAmount));
            elements.Add(new XElement("SupplementCharge", supplementCharge));
            return CreateDocument(ROOTNAME, elements);
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            string xml =
                "<>1</>" +
                "<>Peter Forsberg</>";
            return XmlUtil.CreateXml(ROOTNAME, xml);
        }

        #endregion
    }




    #endregion

    #region SupplierInvoice Cost Transfer
    internal class MobileSupplierInvoiceCostTransfer : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "CostTransfer";

        #endregion

        #region Variables

        public readonly int recordType;
        public readonly int recordId;
        public readonly string orderNumber;
        public readonly string orderName;
        public readonly int orderId;
        public readonly string projectNumber;
        public readonly string projectName;
        public readonly int projectId;
        public readonly string timeCodeName;
        public readonly int timeCodeId;
        public readonly string employeeName;
        public readonly int? employeeId;
        public readonly decimal amount;
        public readonly decimal supplementCharge;
        public readonly decimal sumAmount;
        public readonly bool chargeCostToProject;
        public readonly bool includeSupplierInvoiceImage;

        #endregion

        #region Ctor

        public MobileSupplierInvoiceCostTransfer(MobileParam param, SupplierInvoiceCostTransferDTO item) : base(param)
        {
            recordType = (int)item.Type;
            recordId = item.RecordId;
            orderNumber = item.CustomerInvoiceNr;
            orderName = item.CustomerInvoiceNumberName;
            orderId = item.CustomerInvoiceId;
            projectNumber = item.ProjectNr;
            projectName = item.ProjectName;
            projectId = item.ProjectId;
            timeCodeName = item.TimeCodeName;
            timeCodeId = item.TimeCodeId;
            employeeName = item.EmployeeName;
            employeeId = item.EmployeeId;
            amount = item.Amount;
            supplementCharge = item.SupplementCharge;
            sumAmount = item.SumAmount;
            chargeCostToProject = item.ChargeCostToProject;
            includeSupplierInvoiceImage = item.IncludeSupplierInvoiceImage;
        }

        /// <summary>
        /// Used for errors
        /// </summary>
        public MobileSupplierInvoiceCostTransfer(MobileParam param, string errorMessage) : base(param, errorMessage)
        {

        }

        #endregion

        #region Public methods

        #endregion

        #region Overrided methods
        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();
            elements.Add(new XElement("Type", recordType));
            elements.Add(new XElement("RecordId", recordId));
            elements.Add(new XElement("OrderNumber", orderNumber));
            elements.Add(new XElement("OrderName", orderName));
            elements.Add(new XElement("OrderId", orderId));
            elements.Add(new XElement("ProjectNumber", projectNumber));
            elements.Add(new XElement("ProjectName", projectName));
            elements.Add(new XElement("ProjectId", projectId));
            elements.Add(new XElement("TimeCodeName", timeCodeName));
            elements.Add(new XElement("TimeCodeId", timeCodeId));
            elements.Add(new XElement("EmployeeName", employeeName));
            elements.Add(new XElement("EmployeeId", employeeId));
            elements.Add(new XElement("Amount", amount));
            elements.Add(new XElement("SupplementCharge", supplementCharge));
            elements.Add(new XElement("SumAmount", sumAmount));
            elements.Add(new XElement("ChargeCostToProject", chargeCostToProject));
            elements.Add(new XElement("IncludeSupplierInvoiceImage", includeSupplierInvoiceImage));
            return CreateDocument(ROOTNAME, elements);
        }
        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            string xml =
                "<>1</>" +
                "<>Peter Forsberg</>";
            return XmlUtil.CreateXml(ROOTNAME, xml);
        }

        #endregion
    }

    #endregion

    #region HTMLView

    internal class MobileHTMLView : MobileBase
    {
        #region Constants

        public const string ROOTNAMESTART = "<HTMLView>";
        public const string ROOTNAMEEND = "</HTMLView>";
        public const string CHILDNAMESTART = "<HtmlData>";
        public const string CHILDNAMEEND = "</HtmlData>";
        #endregion

        #region Variables

        private readonly StringBuilder htmlData;

        #endregion

        #region Ctor

        public MobileHTMLView(MobileParam param) : base(param)
        {

        }

        public MobileHTMLView(MobileParam param, StringBuilder htmlData) : base(param)
        {
            this.htmlData = htmlData;
        }

        /// <summary>
        /// Used for errors
        /// </summary>
        public MobileHTMLView(MobileParam param, string errorMessage) : base(param, errorMessage)
        {
            htmlData = new StringBuilder();
        }

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            return null;
        }

        public override string ToString()
        {
            return ROOTNAMESTART + CHILDNAMESTART + CDATASTART + htmlData.ToString() + CDATAEND + CHILDNAMEEND + ROOTNAMEEND;
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            string xml =
                "<HTMLView>" +
                    "<HtmlData><![CDATA[ html code ]]> </HtmlData>" +
                "</HTMLView>";
            return xml;
        }

        #endregion
    }

    #endregion

    #region TimeSheet
    internal class MobileTimeSheetRows : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "TimeSheetRows";

        #endregion

        #region Variables

        private readonly List<MobileTimeSheetRow> mobileTimeSheetRows = new List<MobileTimeSheetRow>();

        #endregion

        #region Ctor

        public MobileTimeSheetRows(MobileParam param, List<TimeSheetDTO> timeSheetDtos, bool invoicedTimePermission) : base(param)
        {
            AddMobileTimeSheetRows(timeSheetDtos, invoicedTimePermission);
        }

        /// <summary>Used for errors</summary>
        public MobileTimeSheetRows(MobileParam param, string errorMessage) : base(param, errorMessage)
        {
        }

        #endregion

        #region Public methods

        public void AddMobileTimeSheetRows(List<TimeSheetDTO> timeSheetDtos, bool invoicedTimePermission)
        {
            if (timeSheetDtos == null)
                return;

            foreach (var timeSheetDto in timeSheetDtos)
            {
                AddMobileTimeSheetRow(new MobileTimeSheetRow(this.Param, timeSheetDto, invoicedTimePermission));
            }
        }

        public void AddMobileTimeSheetRow(MobileTimeSheetRow timeSheetsDto)
        {
            if (timeSheetsDto == null)
                return;

            mobileTimeSheetRows.Add(timeSheetsDto);
        }

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            return MergeDocuments(ROOTNAME, mobileTimeSheetRows.Select(i => i.ToXDocument()).ToList());
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            return XmlUtil.CreateXml(ROOTNAME, MobileTimeSheetRow.GetDefaultXml());
        }

        #endregion
    }

    internal class MobileTimeSheetRow : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "TimeSheet";

        #endregion

        #region Variables

        #region Field values

        private readonly bool invoicedTimePermission = false;

        private readonly int projectId;
        private readonly string projectName;

        private readonly int customerId;
        private readonly string customerName;

        private readonly int invoiceId;
        private readonly string invoiceNr;

        private readonly int timeCodeId;
        private readonly string timeCodeName;

        private readonly int mondayWorkTimeInMinutes;
        private readonly int mondayInvoiceTimeInMinutes;
        private readonly string mondayNoteInternal;
        private readonly string mondayNoteExternal;

        private readonly int tuesdayWorkTimeInMinutes;
        private readonly int tuesdayInvoiceTimeInMinutes;
        private readonly string tuesdayNoteInternal;
        private readonly string tuesdayNoteExternal;

        private readonly int wednesdayWorkTimeInMinutes;
        private readonly int wednesdayInvoiceTimeInMinutes;
        private readonly string wednesdayNoteInternal;
        private readonly string wednesdayNoteExternal;

        private readonly int thursdayWorkTimeInMinutes;
        private readonly int thursdayInvoiceTimeInMinutes;
        private readonly string thursdayNoteInternal;
        private readonly string thursdayNoteExternal;

        private readonly int fridayWorkTimeInMinutes;
        private readonly int fridayInvoiceTimeInMinutes;
        private readonly string fridayNoteInternal;
        private readonly string fridayNoteExternal;

        private readonly int saturdayWorkTimeInMinutes;
        private readonly int saturdayInvoiceTimeInMinutes;
        private readonly string saturdayNoteInternal;
        private readonly string saturdayNoteExternal;

        private readonly int sundayWorkTimeInMinutes;
        private readonly int sundayInvoiceTimeInMinutes;
        private readonly string sundayNoteInternal;
        private readonly string sundayNoteExternal;

        private readonly int weekSumWorkTimeInMinutes;
        private readonly int weekSumInvoiceTimeInMinutes;
        private readonly string weekNoteInternal;
        private readonly string weekNoteExternal;

        private readonly string attestStateColor;
        private readonly string attestStateName;

        #endregion

        #endregion

        #region Ctor

        public MobileTimeSheetRow(MobileParam param) : base(param)
        {
            Init();
        }

        public MobileTimeSheetRow(MobileParam param, TimeSheetDTO timeSheetDto, bool invoicedTimePermission) : base(param)
        {
            Init();

            this.invoicedTimePermission = invoicedTimePermission;

            #region Set data
            //weekNr = CalendarUtility.GetWeekNr(timeSheetDto.WeekStartDate);
            //dateFrom = timeSheetDto.WeekStartDate;
            //dateTo = CalendarUtility.GetLastDateOfWeek(timeSheetDto.WeekStartDate);

            projectId = timeSheetDto.ProjectId;
            projectName = timeSheetDto.ProjectNr + " " + timeSheetDto.ProjectName;

            customerId = timeSheetDto.CustomerId;
            customerName = timeSheetDto.CustomerName;

            invoiceId = timeSheetDto.InvoiceId;
            invoiceNr = timeSheetDto.InvoiceNr;

            timeCodeId = timeSheetDto.TimeCodeId;
            timeCodeName = timeSheetDto.TimeCodeName;

            mondayWorkTimeInMinutes = (int)timeSheetDto.MondayActual.TotalMinutes;
            mondayInvoiceTimeInMinutes = (int)timeSheetDto.Monday.TotalMinutes;
            mondayNoteInternal = timeSheetDto.MondayNote;
            mondayNoteExternal = timeSheetDto.MondayNoteExternal;

            tuesdayWorkTimeInMinutes = (int)timeSheetDto.TuesdayActual.TotalMinutes;
            tuesdayInvoiceTimeInMinutes = (int)timeSheetDto.Tuesday.TotalMinutes;
            tuesdayNoteInternal = timeSheetDto.TuesdayNote;
            tuesdayNoteExternal = timeSheetDto.TuesdayNoteExternal;

            wednesdayWorkTimeInMinutes = (int)timeSheetDto.WednesdayActual.TotalMinutes;
            wednesdayInvoiceTimeInMinutes = (int)timeSheetDto.Wednesday.TotalMinutes;
            wednesdayNoteInternal = timeSheetDto.WednesdayNote;
            wednesdayNoteExternal = timeSheetDto.WednesdayNoteExternal;

            thursdayWorkTimeInMinutes = (int)timeSheetDto.ThursdayActual.TotalMinutes;
            thursdayInvoiceTimeInMinutes = (int)timeSheetDto.Thursday.TotalMinutes;
            thursdayNoteInternal = timeSheetDto.ThursdayNote;
            thursdayNoteExternal = timeSheetDto.ThursdayNoteExternal;

            fridayWorkTimeInMinutes = (int)timeSheetDto.FridayActual.TotalMinutes;
            fridayInvoiceTimeInMinutes = (int)timeSheetDto.Friday.TotalMinutes;
            fridayNoteInternal = timeSheetDto.FridayNote;
            fridayNoteExternal = timeSheetDto.FridayNoteExternal;

            saturdayWorkTimeInMinutes = (int)timeSheetDto.SaturdayActual.TotalMinutes;
            saturdayInvoiceTimeInMinutes = (int)timeSheetDto.Saturday.TotalMinutes;
            saturdayNoteInternal = timeSheetDto.SaturdayNote;
            saturdayNoteExternal = timeSheetDto.SaturdayNoteExternal;

            sundayWorkTimeInMinutes = (int)timeSheetDto.SundayActual.TotalMinutes;
            sundayInvoiceTimeInMinutes = (int)timeSheetDto.Sunday.TotalMinutes;
            sundayNoteInternal = timeSheetDto.SundayNote;
            sundayNoteExternal = timeSheetDto.SundayNoteExternal;

            weekSumWorkTimeInMinutes = (int)timeSheetDto.WeekSumActual.TotalMinutes;
            weekSumInvoiceTimeInMinutes = (int)timeSheetDto.WeekSum.TotalMinutes;
            weekNoteInternal = timeSheetDto.WeekNote;
            weekNoteExternal = timeSheetDto.WeekNoteExternal;

            attestStateColor = timeSheetDto.AttestStateColor;
            attestStateName = timeSheetDto.AttestStateName;

            #endregion

        }

        /// <summary>
        /// Used for errors
        /// </summary>
        public MobileTimeSheetRow(MobileParam param, string errorMessage) : base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            //Set default values
        }

        #endregion

        #region Public methods

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();

            //elements.Add(new XElement("WeekNr", this.weekNr.ToString()));
            //elements.Add(new XElement("DateFrom", this.dateFrom.ToShortDateString()));
            //elements.Add(new XElement("DateTo", this.dateTo.ToShortDateString()));

            elements.Add(new XElement("ProjectId", this.projectId.ToString()));
            elements.Add(new XElement("ProjectName", this.projectName));

            elements.Add(new XElement("CustomerId", this.customerId.ToString()));
            elements.Add(new XElement("CustomerName", this.customerName));

            elements.Add(new XElement("InvoiceId", this.invoiceId.ToString()));
            elements.Add(new XElement("InvoiceNr", this.invoiceNr));

            elements.Add(new XElement("TimeCodeId", this.timeCodeId.ToString()));
            elements.Add(new XElement("TimeCodeName", this.timeCodeName));

            elements.Add(new XElement("MonWork", this.mondayWorkTimeInMinutes.ToString()));
            elements.Add(new XElement("MonInv", invoicedTimePermission ? this.mondayInvoiceTimeInMinutes.ToString() : "0"));
            elements.Add(new XElement("MonNoteInt", this.mondayNoteInternal));
            elements.Add(new XElement("MonNoteExt", this.mondayNoteExternal));

            elements.Add(new XElement("TueWork", this.tuesdayWorkTimeInMinutes.ToString()));
            elements.Add(new XElement("TueInv", invoicedTimePermission ? this.tuesdayInvoiceTimeInMinutes.ToString() : "0"));
            elements.Add(new XElement("TueNoteInt", this.tuesdayNoteInternal));
            elements.Add(new XElement("TueNoteExt", this.tuesdayNoteExternal));

            elements.Add(new XElement("WedWork", this.wednesdayWorkTimeInMinutes.ToString()));
            elements.Add(new XElement("WedInv", invoicedTimePermission ? this.wednesdayInvoiceTimeInMinutes.ToString() : "0"));
            elements.Add(new XElement("WedNoteInt", this.wednesdayNoteInternal));
            elements.Add(new XElement("WedNoteExt", this.wednesdayNoteExternal));

            elements.Add(new XElement("ThuWork", this.thursdayWorkTimeInMinutes.ToString()));
            elements.Add(new XElement("ThuInv", invoicedTimePermission ? this.thursdayInvoiceTimeInMinutes.ToString() : "0"));
            elements.Add(new XElement("ThuNoteInt", this.thursdayNoteInternal));
            elements.Add(new XElement("ThuNoteExt", this.thursdayNoteExternal));

            elements.Add(new XElement("FriWork", this.fridayWorkTimeInMinutes.ToString()));
            elements.Add(new XElement("FriInv", invoicedTimePermission ? this.fridayInvoiceTimeInMinutes.ToString() : "0"));
            elements.Add(new XElement("FriNoteInt", this.fridayNoteInternal));
            elements.Add(new XElement("FriNoteExt", this.fridayNoteExternal));

            elements.Add(new XElement("SatWork", this.saturdayWorkTimeInMinutes.ToString()));
            elements.Add(new XElement("SatInv", invoicedTimePermission ? this.saturdayInvoiceTimeInMinutes.ToString() : "0"));
            elements.Add(new XElement("SatNoteInt", this.saturdayNoteInternal));
            elements.Add(new XElement("SatNoteExt", this.saturdayNoteExternal));

            elements.Add(new XElement("SunWork", this.sundayWorkTimeInMinutes.ToString()));
            elements.Add(new XElement("SunInv", invoicedTimePermission ? this.sundayInvoiceTimeInMinutes.ToString() : "0"));
            elements.Add(new XElement("SunNoteInt", this.sundayNoteInternal));
            elements.Add(new XElement("SunNoteExt", this.sundayNoteExternal));

            elements.Add(new XElement("WeekWork", this.weekSumWorkTimeInMinutes.ToString()));
            elements.Add(new XElement("WeekInv", invoicedTimePermission ? this.weekSumInvoiceTimeInMinutes.ToString() : "0"));
            elements.Add(new XElement("WeekNoteInt", this.weekNoteInternal));
            elements.Add(new XElement("WeekNoteExt", this.weekNoteExternal));

            elements.Add(new XElement("AttestStateColor", this.attestStateColor));
            elements.Add(new XElement("AttestStateName", this.attestStateName));



            return CreateDocument(ROOTNAME, elements);
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            string xml =
                //"<WeekNr>27</WeekNr>" + 
                //"<DateFrom>2013-07-01</DateFrom>" + 
                //"<DateTo>2013-07-07</DateTo>" + 
                "<ProjectId>21</ProjectId>" +
                "<ProjectName>99 Ulla Andersson</ProjectName>" +
                "<CustomerId>21</CustomerId>" +
                "<CustomerName>Ulla Andersson</CustomerName>" +
                "<InvoiceId>418939</InvoiceId>" +
                "<InvoiceNr>1236</InvoiceNr>" +
                "<TimeCodeId>1</TimeCodeId>" +
                "<TimeCodeName>Konsulttid</TimeCodeName>" +
                "<MonWork>60</MonWork>" +
                "<MonInv>120</MonInv>" +
                "<MonNoteInt>MonNoteInt</MonNoteInt>" +
                "<MonNoteExt />" +
                "<TueWork>60</TueWork>" +
                "<TueInv>120</TueInv>" +
                "<TueNoteInt>TueNoteInt</TueNoteInt>" +
                "<TueNoteExt />" +
                "<WedWork>60</WedWork>" +
                "<WedInv>120</WedInv>" +
                "<WedNoteInt>WedNoteInt</WedNoteInt>" +
                "<WedNoteExt />" +
                "<ThuWork>60</ThuWork>" +
                "<ThuInv>120</ThuInv>" +
                "<ThuNoteInt>ThuNoteInt</ThuNoteInt>" +
                "<ThuNoteExt />" +
                "<FriWork>60</FriWork>" +
                "<FriInv>120</FriInv>" +
                "<FriNoteInt>FriNoteInt</FriNoteInt>" +
                "<FriNoteExt />" +
                "<SatWork>60</SatWork>" +
                "<SatInv>120</SatInv>" +
                "<SatNoteInt>SatNotInt</SatNoteInt>" +
                "<SatNoteExt />" +
                "<SunWork>60</SunWork>" +
                "<SunInv>120</SunInv>" +
                "<SunNoteInt>SunNoteInt</SunNoteInt>" +
                "<SunNoteExt />" +
                "<WeekWork>420</WeekWork>" +
                "<WeekInv>840</WeekInv>" +
                "<WeekNoteInt />" +
                "<WeekNoteExt />" +
                "<AttestStateColor></AttestStateColor>" +
                "<AttestStateName>Reg</AttestStateName>";

            return XmlUtil.CreateXml(ROOTNAME, xml);
        }

        #endregion
    }

    internal class MobileTimeSheetInfo : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "TimeSheetInfo";

        #endregion

        #region Variables

        #region Field values

        private readonly bool invoicedTimePermission = false;
        private readonly bool showExpenses = false;
        private readonly int expensesCount;
        private readonly int mondaySumWork;
        private readonly int mondaySumOther;
        private readonly int mondaySumInv;
        private readonly int tuesdaySumWork;
        private readonly int tuesdaySumOther;
        private readonly int tuesdaySumInv;
        private readonly int wednesdaySumWork;
        private readonly int wednesdaySumOther;
        private readonly int wednesdaySumInv;
        private readonly int thursdaySumWork;
        private readonly int thursdaySumOther;
        private readonly int thursdaySumInv;
        private readonly int fridaySumWork;
        private readonly int fridaySumOther;
        private readonly int fridaySumInv;
        private readonly int saturdaySumWork;
        private readonly int saturdaySumOther;
        private readonly int saturdaySumInv;
        private readonly int sundaySumWork;
        private readonly int sundaySumOther;
        private readonly int sundaySumInv;
        private readonly int weekSumWork;
        private readonly int weekSumOther;
        private readonly int weekSumInv;

        #endregion

        #endregion

        #region Ctor

        public MobileTimeSheetInfo(MobileParam param) : base(param)
        {
            Init();
        }

        public MobileTimeSheetInfo(MobileParam param, TimeSheetScheduleDTO schedule, List<TimeSheetDTO> timeSheetDtos, bool invoicedTimePermission, bool showExpenses, int expensesCount) : base(param)
        {
            Init();

            this.invoicedTimePermission = invoicedTimePermission;
            this.showExpenses = showExpenses;
            this.expensesCount = expensesCount;

            var items = timeSheetDtos.Where(r => r.WeekStartDate == schedule.WeekStartDate && !r.IsDeleted);

            //Sum Work
            mondaySumWork = (int)items.Sum(i => i.MondayActual.TotalMinutes);
            mondaySumOther = (int)items.Sum(i => i.MondayOther.TotalMinutes);
            tuesdaySumWork = (int)items.Sum(i => i.TuesdayActual.TotalMinutes);
            tuesdaySumOther = (int)items.Sum(i => i.TuesdayOther.TotalMinutes);
            wednesdaySumWork = (int)items.Sum(i => i.WednesdayActual.TotalMinutes);
            wednesdaySumOther = (int)items.Sum(i => i.WednesdayOther.TotalMinutes);
            thursdaySumWork = (int)items.Sum(i => i.ThursdayActual.TotalMinutes);
            thursdaySumOther = (int)items.Sum(i => i.ThursdayOther.TotalMinutes);
            fridaySumWork = (int)items.Sum(i => i.FridayActual.TotalMinutes);
            fridaySumOther = (int)items.Sum(i => i.FridayOther.TotalMinutes);
            saturdaySumWork = (int)items.Sum(i => i.SaturdayActual.TotalMinutes);
            saturdaySumOther = (int)items.Sum(i => i.SaturdayOther.TotalMinutes);
            sundaySumWork = (int)items.Sum(i => i.SundayActual.TotalMinutes);
            sundaySumOther = (int)items.Sum(i => i.SundayOther.TotalMinutes);
            weekSumWork = (int)items.Sum(i => i.WeekSumActual.TotalMinutes);
            weekSumOther = (int)items.Sum(i => i.WeekSumOther.TotalMinutes);

            //Sum Inv
            mondaySumInv = (int)items.Sum(i => i.Monday.TotalMinutes);
            tuesdaySumInv = (int)items.Sum(i => i.Tuesday.TotalMinutes);
            wednesdaySumInv = (int)items.Sum(i => i.Wednesday.TotalMinutes);
            thursdaySumInv = (int)items.Sum(i => i.Thursday.TotalMinutes);
            fridaySumInv = (int)items.Sum(i => i.Friday.TotalMinutes);
            saturdaySumInv = (int)items.Sum(i => i.Saturday.TotalMinutes);
            sundaySumInv = (int)items.Sum(i => i.Sunday.TotalMinutes);
            weekSumInv = (int)items.Sum(i => i.WeekSum.TotalMinutes);
        }

        /// <summary>
        /// Used for errors
        /// </summary>
        public MobileTimeSheetInfo(MobileParam param, string errorMessage) : base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            //Set default values
        }

        #endregion

        #region Public methods

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();

            elements.Add(new XElement("SExpenses", StringUtility.GetString(this.showExpenses)));
            elements.Add(new XElement("ExpensesCount", this.expensesCount));

            elements.Add(new XElement("MonSumWork", this.mondaySumWork.ToString()));
            elements.Add(new XElement("MonSumOther", this.mondaySumOther.ToString()));
            elements.Add(new XElement("MonSumInv", invoicedTimePermission ? this.mondaySumInv.ToString() : "0"));
            //elements.Add(new XElement("MonSchedule", this.mondaySchedule.ToString()));
            //elements.Add(new XElement("MonDiffWork", this.mondayDiffWork.ToString()));
            //elements.Add(new XElement("MonDiffInv", this.mondayDiffInv.ToString()));

            elements.Add(new XElement("TueSumWork", this.tuesdaySumWork.ToString()));
            elements.Add(new XElement("TueSumOther", this.tuesdaySumOther.ToString()));
            elements.Add(new XElement("TueSumInv", invoicedTimePermission ? this.tuesdaySumInv.ToString() : "0"));
            //elements.Add(new XElement("TueSchedule", this.tuesdaySchedule.ToString()));
            //elements.Add(new XElement("TueDiffWork", this.tuesdayDiffWork.ToString()));
            //elements.Add(new XElement("TueDiffInv", this.tuesdayDiffInv.ToString()));

            elements.Add(new XElement("WedSumWork", this.wednesdaySumWork.ToString()));
            elements.Add(new XElement("WedSumOther", this.wednesdaySumOther.ToString()));
            elements.Add(new XElement("WedSumInv", invoicedTimePermission ? this.wednesdaySumInv.ToString() : "0"));
            //elements.Add(new XElement("WedSchedule", this.wednesdaySchedule.ToString()));
            //elements.Add(new XElement("WedDiffWork", this.wednesdayDiffWork.ToString()));
            //elements.Add(new XElement("WedDiffInv", this.wednesdayDiffInv.ToString()));

            elements.Add(new XElement("ThuSumWork", this.thursdaySumWork.ToString()));
            elements.Add(new XElement("ThuSumOther", this.thursdaySumOther.ToString()));
            elements.Add(new XElement("ThuSumInv", invoicedTimePermission ? this.thursdaySumInv.ToString() : "0"));
            //elements.Add(new XElement("ThuSchedule", this.thursdaySchedule.ToString()));
            //elements.Add(new XElement("ThuDiffWork", this.thursdayDiffWork.ToString()));
            //elements.Add(new XElement("ThuDiffInv", this.thursdayDiffInv.ToString()));


            elements.Add(new XElement("FriSumWork", this.fridaySumWork.ToString()));
            elements.Add(new XElement("FriSumOther", this.fridaySumOther.ToString()));
            elements.Add(new XElement("FriSumInv", invoicedTimePermission ? this.fridaySumInv.ToString() : "0"));
            //elements.Add(new XElement("FriSchedule", this.fridaySchedule.ToString()));
            //elements.Add(new XElement("FriDiffWork", this.fridayDiffWork.ToString()));
            //elements.Add(new XElement("FriDiffInv", this.fridayDiffInv.ToString()));

            elements.Add(new XElement("SatSumWork", this.saturdaySumWork.ToString()));
            elements.Add(new XElement("SatSumOther", this.saturdaySumOther.ToString()));
            elements.Add(new XElement("SatSumInv", invoicedTimePermission ? this.saturdaySumInv.ToString() : "0"));
            //elements.Add(new XElement("SatSchedule", this.saturdaySchedule.ToString()));
            //elements.Add(new XElement("SatDiffWork", this.saturdayDiffWork.ToString()));
            //elements.Add(new XElement("SatDiffInv", this.saturdayDiffInv.ToString()));

            elements.Add(new XElement("SunSumWork", this.sundaySumWork.ToString()));
            elements.Add(new XElement("SunSumOther", this.sundaySumOther.ToString()));
            elements.Add(new XElement("SunSumInv", invoicedTimePermission ? this.sundaySumInv.ToString() : "0"));
            //elements.Add(new XElement("SunSchedule", this.sundaySchedule.ToString()));
            //elements.Add(new XElement("SunDiffWork", this.sundayDiffWork.ToString()));
            //elements.Add(new XElement("SunDiffInv", this.sundayDiffInv.ToString()));

            elements.Add(new XElement("WeekSumWork", this.weekSumWork.ToString()));
            elements.Add(new XElement("WeekSumOther", this.weekSumOther.ToString()));
            elements.Add(new XElement("WeekSumInv", invoicedTimePermission ? this.weekSumInv.ToString() : "0"));
            //elements.Add(new XElement("WeekSumSchedule", this.weekSumSchedule.ToString()));
            //elements.Add(new XElement("WeekSumDiffWork", this.weekSumDiffWork.ToString()));
            //elements.Add(new XElement("WeekSumDiffInv", this.weekSumDiffInv.ToString()));

            return CreateDocument(ROOTNAME, elements);
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            string xml =
                "<MonSumWork>60</MonSumWork>" +
                "<MonSumInv>120</MonSumInv>" +
                "<MonSchedule>0</MonSchedule>" +
                "<MonDiffWork>60</MonDiffWork>" +
                "<MonDiffInv>120</MonDiffInv>" +
                "<TueSumWork>60</TueSumWork>" +
                "<TueSumInv>120</TueSumInv>" +
                "<TueSchedule>0</TueSchedule>" +
                "<TueDiffWork>60</TueDiffWork>" +
                "<TueDiffInv>120</TueDiffInv>" +
                "<WedSumWork>60</WedSumWork>" +
                "<WedSumInv>120</WedSumInv>" +
                "<WedSchedule>0</WedSchedule>" +
                "<WedDiffWork>60</WedDiffWork>" +
                "<WedDiffInv>120</WedDiffInv>" +
                "<ThuSumWork>60</ThuSumWork>" +
                "<ThuSumInv>120</ThuSumInv>" +
                "<ThuSchedule>0</ThuSchedule>" +
                "<ThuDiffWork>60</ThuDiffWork>" +
                "<ThuDiffInv>120</ThuDiffInv>" +
                "<FriSumWork>60</FriSumWork>" +
                "<FriSumInv>120</FriSumInv>" +
                "<FriSchedule>0</FriSchedule>" +
                "<FriDiffWork>60</FriDiffWork>" +
                "<FriDiffInv>120</FriDiffInv>" +
                "<SatSumWork>60</SatSumWork>" +
                "<SatSumInv>120</SatSumInv>" +
                "<SatSchedule>0</SatSchedule>" +
                "<SatDiffWork>60</SatDiffWork>" +
                "<SatDiffInv>120</SatDiffInv>" +
                "<SunSumWork>60</SunSumWork>" +
                "<SunSumInv>120</SunSumInv>" +
                "<SunSchedule>0</SunSchedule>" +
                "<SunDiffWork>60</SunDiffWork>" +
                "<SunDiffInv>120</SunDiffInv>" +
                "<WeekSumWork>420</WeekSumWork>" +
                "<WeekSumInv>840</WeekSumInv>" +
                "<WeekSumSchedule>0</WeekSumSchedule>" +
                "<WeekSumDiffWork>420</WeekSumDiffWork>" +
                "<WeekSumDiffInv>840</WeekSumDiffInv>";

            return XmlUtil.CreateXml(ROOTNAME, xml);
        }

        #endregion
    }

    #endregion

    #region ProjectTimeBlocks

    internal class MobileHasTransactionsOnDateAndTimeCode : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "HasTransactionsOnDateAndTimeCode";

        #endregion

        #region Variables

        #region Field values

        private readonly bool existing;

        #endregion

        #endregion

        #region Ctor

        public MobileHasTransactionsOnDateAndTimeCode(MobileParam param, bool existing)
            : base(param)
        {
            Init();

            this.existing = existing;

        }

        /// <summary>
        /// Used for errors
        /// </summary>
        public MobileHasTransactionsOnDateAndTimeCode(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            //Set default values
        }

        #endregion

        #region Public methods

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();

            elements.Add(new XElement("Value", existing ? "1" : "0"));

            return CreateDocument(ROOTNAME, elements);
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            string xml =
                "<Value></Value>";
            return XmlUtil.CreateXml(ROOTNAME, xml);
        }

        #endregion
    }

    internal class MobileEmployeeFirstEligableTime : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "EmployeeFirstEligableTime";

        #endregion

        #region Variables

        private readonly DateTime time;

        #endregion

        #region Ctor

        public MobileEmployeeFirstEligableTime(MobileParam param, DateTime time) : base(param)
        {
            this.time = time;
        }

        /// <summary>
        /// Used for errors
        /// </summary>
        public MobileEmployeeFirstEligableTime(MobileParam param, string errorMessage) : base(param, errorMessage)
        {
        }

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();

            elements.Add(new XElement("Time", time != null ? time.ToShortTimeString() : ""));

            return CreateDocument(ROOTNAME, elements);
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            string xml =
                "<Time></Time>";
            return XmlUtil.CreateXml(ROOTNAME, xml);
        }

        #endregion
    }

    internal class MobileProjectTimeBlock : MobileOrderRowBase
    {
        #region Constants

        public const string ROOTNAME = "ProjectTimeBlock";

        #endregion

        #region Variables

        #region Field values

        private readonly DateTime Date;
        private readonly DateTime StartTime;
        private readonly DateTime StopTime;
        private readonly int InvoiceTimeInMinutes;
        private readonly int WorkTimeInMinutes;
        private readonly string Note;
        private readonly string InternalNote;
        private readonly int TimeCodeId;
        private readonly string TimeCodeName;
        private readonly int TimeDeviationCauseId;
        private readonly string TimeDeviationCauseName;
        private readonly int Id;
        private readonly bool CalculateAsOtherTimeInSales;
        private readonly bool IsDayAttested;
        private readonly bool IsOrderRowLocked;
        private readonly bool IsPayrollEditable;
        private readonly bool IsEditable;
        private readonly bool ValidationError;

        private readonly int CustomerInvoiceId;
        private readonly string InvoiceNr;
        private readonly bool OrderClosed;
        private readonly string CustomerName;
        private readonly string ProjectName;
        private readonly int EmployeeChildId;
        private readonly string EmployeeChildName;

        private readonly List<XElement> ValidationErrorMessage;


        #endregion

        #endregion

        #region Ctor

        /// <summary>Used for get Time Rows</summary>
        public MobileProjectTimeBlock(MobileParam param, int orderId, ProjectTimeBlockDTO projectTimeBlock, int orderInitialAttestStateId, int payrollInitialAttestStateId, bool workedTimePermission, bool invoicedTimePermission) : base(param, orderId)
        {
            if (projectTimeBlock != null)
            {
                this.Id = projectTimeBlock.ProjectTimeBlockId;
                this.Date = projectTimeBlock.Date;
                this.StartTime = projectTimeBlock.StartTime;
                this.StopTime = projectTimeBlock.StopTime;
                this.InvoiceTimeInMinutes = invoicedTimePermission ? decimal.ToInt32(projectTimeBlock.InvoiceQuantity) : 0;
                this.WorkTimeInMinutes = workedTimePermission ? decimal.ToInt32(projectTimeBlock.TimePayrollQuantity) : 0;
                this.Note = !string.IsNullOrEmpty(projectTimeBlock.ExternalNote) ? projectTimeBlock.ExternalNote : string.Empty;
                this.InternalNote = !string.IsNullOrEmpty(projectTimeBlock.InternalNote) ? projectTimeBlock.InternalNote : string.Empty;
                this.TimeCodeName = projectTimeBlock.TimeCodeName;
                this.TimeCodeId = projectTimeBlock.TimeCodeId;
                this.TimeDeviationCauseId = projectTimeBlock.TimeDeviationCauseId;
                this.TimeDeviationCauseName = projectTimeBlock.TimeDeviationCauseName;
                this.IsOrderRowLocked = (projectTimeBlock.CustomerInvoiceRowAttestStateId != 0 && projectTimeBlock.CustomerInvoiceRowAttestStateId != orderInitialAttestStateId) || !projectTimeBlock.IsEditable;
                this.IsPayrollEditable = projectTimeBlock.IsPayrollEditable;
                this.IsEditable = projectTimeBlock.IsEditable;
                this.IsDayAttested = (projectTimeBlock.TimePayrollAttestStateId != 0 && projectTimeBlock.TimePayrollAttestStateId != payrollInitialAttestStateId);
                this.CalculateAsOtherTimeInSales = projectTimeBlock.AdditionalTime;

                this.CustomerInvoiceId = projectTimeBlock.CustomerInvoiceId;
                this.InvoiceNr = projectTimeBlock.InvoiceNr;
                this.OrderClosed = projectTimeBlock.OrderClosed;
                this.CustomerName = projectTimeBlock.CustomerName;
                this.ProjectName = projectTimeBlock.ProjectName;
                this.EmployeeChildId = projectTimeBlock.EmployeeChildId;
                this.EmployeeChildName = projectTimeBlock.EmployeeChildName;
                this.ValidationError = false;
                this.ValidationErrorMessage = null;
            }
        }

        public MobileProjectTimeBlock(MobileParam param, int orderId, ProjectTimeBlock projectTimeBlock, int timecodeId, string timeCodeName, int timeDeviationCauseId, string timeDeviationCauseName, bool workedTimePermission, bool invoicedTimePermission) : base(param, orderId)
        {
            if (projectTimeBlock != null)
            {
                this.Id = projectTimeBlock.ProjectTimeBlockId;
                this.Date = projectTimeBlock.TimeBlockDate.Date;
                this.StartTime = projectTimeBlock.StartTime;
                this.StopTime = projectTimeBlock.StopTime;
                this.InvoiceTimeInMinutes = invoicedTimePermission ? projectTimeBlock.InvoiceQuantity : 0;
                this.WorkTimeInMinutes = workedTimePermission ? CalendarUtility.TimeSpanToMinutes(projectTimeBlock.StopTime, projectTimeBlock.StartTime) : 0;
                this.Note = !string.IsNullOrEmpty(projectTimeBlock.ExternalNote) ? projectTimeBlock.ExternalNote : string.Empty;
                this.InternalNote = !string.IsNullOrEmpty(projectTimeBlock.InternalNote) ? projectTimeBlock.InternalNote : string.Empty;
                this.TimeCodeName = timeCodeName;
                this.TimeCodeId = timecodeId;
                this.TimeDeviationCauseId = timeDeviationCauseId;
                this.TimeDeviationCauseName = timeDeviationCauseName;
                this.IsOrderRowLocked = false;
                this.IsDayAttested = false;
                this.CustomerInvoiceId = projectTimeBlock.CustomerInvoiceId.GetValueOrDefault();

                this.ValidationError = false;
                this.ValidationErrorMessage = null;
            }
        }

        public MobileProjectTimeBlock(MobileParam param, int orderId, bool validationError, List<XElement> validationErrorMessage) : base(param, orderId)
        {

            this.ValidationError = validationError;
            this.ValidationErrorMessage = validationErrorMessage;
        }

        /// <summary>Used for errors</summary>
        public MobileProjectTimeBlock(MobileParam param, int orderId, string errorMessage) : base(param, orderId, errorMessage)
        {

        }


        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();

            elements.Add(new XElement("Id", Id));
            elements.Add(new XElement("Date", Date.ToString()));
            elements.Add(new XElement("Start", StartTime.ToShortTimeString()));
            elements.Add(new XElement("Stop", StopTime.ToShortTimeString()));
            elements.Add(new XElement("IT", InvoiceTimeInMinutes.ToString()));
            elements.Add(new XElement("WT", WorkTimeInMinutes.ToString()));
            elements.Add(new XElement("CAOTIS", StringUtility.GetString(CalculateAsOtherTimeInSales)));
            elements.Add(new XElement("Note", GetTextOrCDATA(Note)));
            elements.Add(new XElement("InternalNote", GetTextOrCDATA(InternalNote)));
            elements.Add(new XElement("TCId", TimeCodeId));
            elements.Add(new XElement("TC", GetTextOrCDATA(TimeCodeName)));
            elements.Add(new XElement("TDCId", TimeDeviationCauseId));
            elements.Add(new XElement("TDC", GetTextOrCDATA(TimeDeviationCauseName)));
            elements.Add(new XElement("Locked", IsOrderRowLocked ? "1" : "0"));
            elements.Add(new XElement("Attested", IsDayAttested ? "1" : "0"));
            elements.Add(new XElement("ValidationError", ValidationError));
            elements.Add(new XElement("ValidationErrorMessage", ValidationErrorMessage));

            elements.Add(new XElement("InvoiceNr", InvoiceNr));
            elements.Add(new XElement("PName", ProjectName));
            elements.Add(new XElement("CName", CustomerName));
            elements.Add(new XElement("OrderId", CustomerInvoiceId.ToString()));
            elements.Add(new XElement("OrderClosed", StringUtility.GetString(OrderClosed)));
            elements.Add(new XElement("IsPayrollEditable", StringUtility.GetString(IsPayrollEditable)));
            elements.Add(new XElement("IsEditable", StringUtility.GetString(IsEditable)));
            elements.Add(new XElement("ECId", EmployeeChildId.ToString()));
            elements.Add(new XElement("ECName", EmployeeChildName));

            return CreateDocument(ROOTNAME, elements);
        }

        public override XDocument ToXDocument(MobileTask task)
        {
            if (base.Failed)
                return MobileMessages.GetErrorMessageDocument(base.ErrorMessage);

            if (this.ValidationError)
                return CreateDocument(ROOTNAME, this.ValidationErrorMessage);

            bool result = base.GetTaskResult(task);
            switch (task)
            {
                case MobileTask.SaveProjectTimeBlock:
                    return MobileMessages.GetSuccessDocument(result);
                default:
                    return base.ToXDocument(task);
            }
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            string xml =
                "<Id></Id>" +
                "<Date></Date>" +
                "<Start></Start>" +
                "<Stop></Stop>" +
                "<IT></IT>" +
                "<WT></WT>" +
                "<Note>Arbete</Note>" +
                "<TCId></TCd>" +
                "<TC></TC>" +
                "<TDCId></TDCId>" +
                "<TDC></TDC>" +
                "<Locked></Locked>" +
                "<Attested></Attested>";

            return XmlUtil.CreateXml(ROOTNAME, xml);
        }

        public static string GetDefaultSaveXml()
        {
            string xml =
                "<Success>1<Success>";

            return xml;
        }

        #endregion
    }

    internal class MobileProjectTimeBlocks : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "ProjectTimeBlocks";

        #endregion

        #region Variables


        private readonly List<MobileProjectTimeBlock> mobileProjectTimeBlocks = new List<MobileProjectTimeBlock>();
        private readonly int orderId;

        #endregion

        #region Ctor

        /// <summary>Used for get TimeRows</summary>
        public MobileProjectTimeBlocks(MobileParam param, int orderId, List<ProjectTimeBlockDTO> projectTimeBlocks, int orderInitialAttestStateId, int payrollInitialAttestStateId, bool workedTimePermission, bool invoicedTimePermission) : base(param)
        {
            AddTimeRows(projectTimeBlocks, orderInitialAttestStateId, payrollInitialAttestStateId, workedTimePermission, invoicedTimePermission);

            this.orderId = orderId;
        }

        /// <summary>Used for errors</summary>
        public MobileProjectTimeBlocks(MobileParam param, int orderId, string errorMessage) : base(param, errorMessage)
        {
            this.orderId = orderId;
        }

        #endregion

        #region Public methods

        public void AddTimeRows(List<ProjectTimeBlockDTO> projectTimeBlocks, int orderInitialAttestStateId, int payrollInitialAttestStateId, bool workedTimePermission, bool invoicedTimePermission)
        {
            if (projectTimeBlocks == null)
                return;

            foreach (ProjectTimeBlockDTO projectTimeBlock in projectTimeBlocks)
            {
                AddProjectTimeBlock(new MobileProjectTimeBlock(this.Param, this.orderId, projectTimeBlock, orderInitialAttestStateId, payrollInitialAttestStateId, workedTimePermission, invoicedTimePermission));
            }
        }

        public void AddProjectTimeBlock(MobileProjectTimeBlock projectTimeBlock)
        {
            if (projectTimeBlock == null || projectTimeBlock.Failed)
                return;

            mobileProjectTimeBlocks.Add(projectTimeBlock);
        }

        public List<MobileProjectTimeBlock> ToList()
        {
            return mobileProjectTimeBlocks.ToList();
        }

        #endregion

        #region Private methods


        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            return MergeDocuments(ROOTNAME, mobileProjectTimeBlocks.Select(i => i.ToXDocument()).ToList());
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            return XmlUtil.CreateXml(ROOTNAME, MobileProjectTimeBlock.GetDefaultXml());
        }

        #endregion
    }

    internal class MobileProjectTimeBlockValidation : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "ProjectTimeBlockValidation";

        #endregion

        #region Variables

        #region Field values


        // Validation
        private readonly bool validationError;
        public bool ValidationError
        {
            get { return validationError; }
        }

        private readonly List<XElement> validationErrorData;
        public List<XElement> ValidationErrorData
        {
            get { return validationErrorData; }
        }

        private readonly bool validationInfo;
        public bool ValidationInfo
        {
            get { return validationInfo; }
        }

        private readonly string validationInfoMessage;
        public string ValidationInfoMessage
        {
            get { return validationInfoMessage; }
        }

        #endregion

        #region Task values

        #endregion

        #endregion

        #region Ctor

        public MobileProjectTimeBlockValidation(MobileParam param, bool validationError, List<XElement> validationErrorData, bool validationInfo, string validationInfoMessage) :
            base(param)
        {
            this.validationError = validationError;
            this.validationErrorData = validationErrorData;
            this.validationInfo = validationInfo;
            this.validationInfoMessage = validationInfoMessage;
        }

        public MobileProjectTimeBlockValidation(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
        }

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();

            elements.Add(new XElement("ValidationError", validationError));
            elements.Add(new XElement("ValidationErrorData", validationErrorData));
            elements.Add(new XElement("ValidationInfo", validationInfo));
            elements.Add(new XElement("ValidationInfoMessage", validationInfoMessage));

            return CreateDocument(ROOTNAME, elements);
        }

        public override XDocument ToXDocument(MobileTask task)
        {
            if (base.Failed)
                return MobileMessages.GetErrorMessageDocument(base.ErrorMessage);

            return base.ToXDocument(task);
        }

        #endregion
    }

    #endregion

    #region Expense(order)

    internal class MobileExpense : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "Expense";

        #endregion

        #region Variables

        #region Field values

        private readonly int orderId;
        public int OrderId
        {
            get { return orderId; }
        }

        private readonly DateTime startTime;
        public DateTime StartTime
        {
            get { return startTime; }
        }

        private readonly DateTime stopTime;
        public DateTime StopTime
        {
            get { return stopTime; }
        }

        private readonly string comment;
        public string Comment
        {
            get { return comment; }
        }

        private readonly string externalComment;
        public string ExternalComment
        {
            get { return externalComment; }
        }

        private readonly int timeCodeId;
        public int TimeCodeId
        {
            get { return timeCodeId; }
        }

        private readonly string timeCodeName;
        public string TimeCodeName
        {
            get { return timeCodeName; }
        }

        private readonly int timeCodeExpenseType;
        public int TimeCodeExpenseType
        {
            get { return timeCodeExpenseType; }
        }

        private readonly decimal quantity;
        public decimal Quantity
        {
            get { return quantity; }
        }

        private readonly decimal amount;
        public decimal Amount
        {
            get { return amount; }
        }

        private readonly decimal vat;
        public decimal Vat
        {
            get { return vat; }
        }

        private readonly decimal unitPrice;
        public decimal UnitPrice
        {
            get { return unitPrice; }
        }

        private readonly decimal invoicedAmount;
        public decimal InvoicedAmount
        {
            get { return invoicedAmount; }
        }

        private readonly int expenseRowId;
        public int ExpenseRowId
        {
            get { return expenseRowId; }
        }

        private readonly bool isSpecifiedUnitPrice;
        public bool IsSpecifiedUnitPrice
        {
            get { return isSpecifiedUnitPrice; }
        }

        private readonly bool transferToOrder;
        public bool TransferToOrder
        {
            get { return transferToOrder; }
        }

        private readonly bool isDayAttested;
        public bool IsDayAttested
        {
            get { return isDayAttested; }
        }

        private readonly bool isOrderRowLocked;
        public bool IsOrderRowLocked
        {
            get { return isOrderRowLocked; }
        }

        private readonly bool timeCodeHasProduct;
        public bool TimeCodeHasProduct
        {
            get { return timeCodeHasProduct; }
        }

        private readonly bool isHR;
        public bool IsHR
        {
            get { return isHR; }
        }
        private readonly bool hasFiles;
        public bool HasFiles
        {
            get { return hasFiles; }
        }
        private readonly bool priceListInclVat;
        public bool PriceListInclVat
        {
            get { return priceListInclVat; }
        }

        private readonly string invoiceNumber;
        public string InvoiceNumber
        {
            get { return invoiceNumber; }
        }

        // Validation
        private readonly bool validationError;
        public bool ValidationError
        {
            get { return validationError; }
        }

        private readonly List<XElement> validationErrorMessage;
        public List<XElement> ValidationErrorMessage
        {
            get { return validationErrorMessage; }
        }

        #endregion

        #region Task values

        private bool versionIsOld;
        public bool VersionIsOld
        {
            get { return versionIsOld; }
            set { versionIsOld = value; }
        }
        #endregion

        #endregion

        #region Ctor

        /// <summary>Used for get Time Rows</summary>
        public MobileExpense(MobileParam param, ExpenseRowDTO expenseRow, string timeCodeName, bool timeCodeHasProduct) : base(param)
        {
            Init();

            if (expenseRow != null)
            {
                this.expenseRowId = expenseRow.ExpenseRowId;
                this.orderId = expenseRow.CustomerInvoiceRowId;
                this.invoiceNumber = expenseRow.CustomerInvoiceNr;
                this.startTime = expenseRow.Start;
                this.stopTime = expenseRow.Stop;
                this.quantity = expenseRow.Quantity;
                this.amount = expenseRow.Amount;
                this.vat = expenseRow.Vat;
                this.unitPrice = expenseRow.UnitPrice;
                this.invoicedAmount = expenseRow.InvoicedAmount;
                this.comment = !string.IsNullOrEmpty(expenseRow.Comment) ? expenseRow.Comment : string.Empty;
                this.externalComment = !string.IsNullOrEmpty(expenseRow.ExternalComment) ? expenseRow.ExternalComment : string.Empty;
                this.timeCodeName = timeCodeName;
                this.timeCodeId = expenseRow.TimeCodeId;
                this.transferToOrder = expenseRow.TransferToOrder;
                this.isSpecifiedUnitPrice = expenseRow.IsSpecifiedUnitPrice;
                this.isOrderRowLocked = expenseRow.isReadOnly;
                this.isDayAttested = expenseRow.isTimeReadOnly;
                this.timeCodeHasProduct = timeCodeHasProduct;
                this.hasFiles = expenseRow.HasFiles;

                this.validationError = false;
                this.validationErrorMessage = null;
            }
        }

        public MobileExpense(MobileParam param, int orderId, ExpenseRowGridDTO expenseRow, int orderInitialAttestStateId, int payrollInitialAttestStateId, string timeCodeName, bool timeCodeHasProduct, bool forGrid) :
            base(param)
        {
            Init();

            if (expenseRow != null)
            {
                this.expenseRowId = expenseRow.ExpenseRowId;
                this.orderId = orderId;
                this.startTime = expenseRow.From;
                this.quantity = expenseRow.Quantity;
                this.amount = expenseRow.Amount;
                this.vat = expenseRow.Vat;
                this.invoicedAmount = expenseRow.InvoicedAmount;
                this.comment = !string.IsNullOrEmpty(expenseRow.Comment) ? expenseRow.Comment : string.Empty;
                this.externalComment = !string.IsNullOrEmpty(expenseRow.ExternalComment) ? expenseRow.ExternalComment : string.Empty;
                this.timeCodeName = timeCodeName;
                this.timeCodeId = expenseRow.TimeCodeId;
                this.transferToOrder = expenseRow.InvoicedAmountCurrency != 0;
                this.isOrderRowLocked = (expenseRow.InvoiceRowAttestStateId != 0 && expenseRow.InvoiceRowAttestStateId != orderInitialAttestStateId);
                this.isDayAttested = (expenseRow.PayrollAttestStateId != 0 && expenseRow.PayrollAttestStateId != payrollInitialAttestStateId);
                this.timeCodeHasProduct = timeCodeHasProduct;
                this.hasFiles = expenseRow.HasFiles;

                this.validationError = false;
                this.validationErrorMessage = null;
            }
        }

        public MobileExpense(MobileParam param, ExpenseRowDTO expenseRow, int timecodeId, string timeCodeName, bool timeCodeHasProduct) :
            base(param)
        {
            Init();

            if (expenseRow != null)
            {
                this.expenseRowId = expenseRow.ExpenseRowId;
                this.orderId = expenseRow.CustomerInvoiceRowId;
                this.startTime = expenseRow.Start;
                this.stopTime = expenseRow.Stop;
                this.quantity = expenseRow.Quantity;
                this.amount = expenseRow.Amount;
                this.vat = expenseRow.Vat;
                this.unitPrice = expenseRow.UnitPrice;
                this.invoicedAmount = expenseRow.InvoicedAmount;
                this.comment = !string.IsNullOrEmpty(expenseRow.Comment) ? expenseRow.Comment : string.Empty;
                this.externalComment = !string.IsNullOrEmpty(expenseRow.ExternalComment) ? expenseRow.ExternalComment : string.Empty;
                this.timeCodeName = timeCodeName;
                this.timeCodeId = expenseRow.TimeCodeId;
                this.isOrderRowLocked = false;
                this.isDayAttested = false;
                this.timeCodeHasProduct = timeCodeHasProduct;

                this.validationError = false;
                this.validationErrorMessage = null;
            }
        }

        public MobileExpense(MobileParam param, AttestEmployeeAdditionDeductionDTO expenseRow) :
            base(param)
        {
            Init();

            this.isHR = true;

            if (expenseRow != null)
            {
                this.expenseRowId = expenseRow.ExpenseRowId;
                this.orderId = expenseRow.CustomerInvoiceId ?? 0;
                this.startTime = expenseRow.Start;
                this.stopTime = expenseRow.Stop;
                this.quantity = expenseRow.Quantity;
                this.amount = expenseRow.Amount ?? 0;
                this.vat = expenseRow.VatAmount ?? 0;
                this.unitPrice = expenseRow.UnitPrice ?? 0;
                this.comment = !string.IsNullOrEmpty(expenseRow.Comment) ? expenseRow.Comment : string.Empty;
                this.timeCodeName = expenseRow.TimeCodeName;
                this.timeCodeId = expenseRow.TimeCodeId;
                this.timeCodeExpenseType = (int)expenseRow.TimeCodeExpenseType;
                this.priceListInclVat = expenseRow.PriceListInclVat;
                this.hasFiles = expenseRow.HasFiles;
                this.invoiceNumber = expenseRow.InvoiceNumber;

                this.validationError = false;
                this.validationErrorMessage = null;
            }
        }

        public MobileExpense(MobileParam param, bool validationError, List<XElement> validationErrorMessage) :
            base(param)
        {
            Init();

            this.validationError = validationError;
            this.validationErrorMessage = validationErrorMessage;
        }

        /// <summary>Used for errors</summary>
        public MobileExpense(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            //Set default values
        }

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();

            elements.Add(new XElement("Id", ExpenseRowId));
            elements.Add(new XElement("OrderId", OrderId));
            elements.Add(new XElement("OrderNr", InvoiceNumber));
            elements.Add(new XElement("Quantity", Quantity.ToString()));
            elements.Add(new XElement("Amount", amount.ToString()));
            elements.Add(new XElement("Vat", Vat.ToString()));
            elements.Add(new XElement("UnitPrice", UnitPrice.ToString()));
            elements.Add(new XElement("TCId", TimeCodeId));
            elements.Add(new XElement("TC", GetTextOrCDATA(TimeCodeName)));
            elements.Add(new XElement("Comment", GetTextOrCDATA(Comment)));
            elements.Add(new XElement("SpecUnitPrice", IsSpecifiedUnitPrice ? "1" : "0"));
            elements.Add(new XElement("ValidationError", validationError));
            elements.Add(new XElement("ValidationErrorMessage", validationErrorMessage));
            elements.Add(new XElement("HasFiles", HasFiles ? "1" : "0"));

            if (mobileManagerUtil.IsCallerExpectedVersionNewerThenGivenVersion(Param.Version, Constants.MOBILE_WS_SUPPORTED_OLDVERSION_34))
            {
                elements.Add(new XElement("Start",StringUtility.GetSwedishFormattedDateTime(StartTime)));
                elements.Add(new XElement("Stop",StringUtility.GetSwedishFormattedDateTime(StopTime)));
            }
            else
            {
                elements.Add(new XElement("Start", StartTime.ToString()));
                elements.Add(new XElement("Stop", StopTime.ToString()));
            }

            if (IsHR)
            {
                elements.Add(new XElement("TCET", TimeCodeExpenseType));
            }
            else
            {
                elements.Add(new XElement("IAmount", InvoicedAmount.ToString()));
                elements.Add(new XElement("EComment", GetTextOrCDATA(ExternalComment)));
                elements.Add(new XElement("Invoiced", TransferToOrder ? "1" : "0"));
                elements.Add(new XElement("Locked", IsOrderRowLocked ? "1" : "0"));
                elements.Add(new XElement("Attested", IsDayAttested ? "1" : "0"));
            }

            elements.Add(new XElement("InclVat", PriceListInclVat ? "1" : "0"));
            return CreateDocument(ROOTNAME, elements);
        }

        public override XDocument ToXDocument(MobileTask task)
        {
            if (base.Failed)
                return MobileMessages.GetErrorMessageDocument(base.ErrorMessage);

            if (this.ValidationError)
                return CreateDocument(ROOTNAME, this.ValidationErrorMessage);

            bool result = base.GetTaskResult(task);
            switch (task)
            {
                case MobileTask.SaveExpense:
                    return (this.versionIsOld ? MobileMessages.GetSuccessDocument(result) : MobileMessages.GetIntMessageDocument(MobileMessages.XML_ELEMENT_EXPENSEROWID, this.expenseRowId));
                default:
                    return base.ToXDocument(task);
            }
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            string xml =
                "<Id></Id>" +
                "<Date></Date>" +
                "<Amount></Amount>" +
                "<Vat></Vat>" +
                "<IAmount></IAmount>" +
                "<TCId></TCd>" +
                "<TC></TC>" +
                "<Comment></Comment>" +
                "<EComment></EComment>" +
                "<Locked></Locked>" +
                "<Attested></Attested>";

            return XmlUtil.CreateXml(ROOTNAME, xml);
        }

        public static string GetDefaultSaveXml()
        {
            string xml =
                "<Success>1<Success>";

            return xml;
        }

        #endregion
    }

    internal class MobileExpenses : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "Expenses";

        #endregion

        #region Variables

        #region Collections

        private List<MobileExpense> mobileExpenses;

        #endregion

        #region Field values

        #endregion

        #endregion

        #region Ctor

        /// <summary>Used for get TimeRows</summary>
        public MobileExpenses(MobileParam param, IEnumerable<ExpenseRowDTO> expenseRows, int orderInitialAttestStateId, int payrollInitialAttestStateId)
            : base(param)
        {
            Init();
            AddExpenseRows(expenseRows, orderInitialAttestStateId, payrollInitialAttestStateId);
        }

        /// <summary>Used for errors</summary>
        public MobileExpenses(MobileParam param, int orderId, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            this.mobileExpenses = new List<MobileExpense>();
        }

        #endregion

        #region Public methods

        public void AddExpenseRows(IEnumerable<ExpenseRowDTO> expenseRows, int orderInitialAttestStateId, int payrollInitialAttestStateId)
        {
            if (expenseRows == null)
                return;

            foreach (ExpenseRowDTO expenseRow in expenseRows)
            {
                AddExpenseRow(new MobileExpense(this.Param, expenseRow, expenseRow.TimeCodeName, false));
            }
        }

        public void AddExpenseRow(MobileExpense expenseRow)
        {
            if (expenseRow == null)
                return;

            mobileExpenses.Add(expenseRow);
        }

        #endregion

        #region Private methods


        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            return MergeDocuments(ROOTNAME, mobileExpenses.Select(i => i.ToXDocument()).ToList());
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            return XmlUtil.CreateXml(ROOTNAME, MobileProjectTimeBlock.GetDefaultXml());
        }

        #endregion
    }

    #endregion

    #region XeDemos

    internal class MobileXeVideos : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "XeVideos";

        #endregion

        #region Variables

        #region Collections

        private List<MobileXeVideo> mobileXeVideos;

        #endregion

        #endregion

        #region Ctor

        public MobileXeVideos(MobileParam param)
            : base(param)
        {
            Init();
        }

        /// <summary>Used for errors</summary>
        public MobileXeVideos(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            this.mobileXeVideos = new List<MobileXeVideo>();
        }

        #endregion

        #region Public methods

        public void AddMobileXeVideo(string name, string url)
        {
            mobileXeVideos.Add(new MobileXeVideo(this.Param, name, url));
        }

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            return MergeDocuments(ROOTNAME, mobileXeVideos.Select(i => i.ToXDocument()).ToList());
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            return XmlUtil.CreateXml(ROOTNAME, MobileXeVideo.GetDefaultXml());
        }

        #endregion
    }

    internal class MobileXeVideo : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "XeVideo";

        #endregion

        #region Variables

        #region Field values

        private readonly String name;
        private readonly String url;
        private readonly int type;

        #endregion

        #endregion

        #region Ctor

        public MobileXeVideo(MobileParam param)
            : base(param)
        {
            Init();

        }

        public MobileXeVideo(MobileParam param, String name, String url)
            : base(param)
        {
            Init();

            this.name = name;
            this.url = url;
            this.type = 1; // for future use
        }

        /// <summary>
        /// Used for errors
        /// </summary>
        public MobileXeVideo(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            //Set default values
        }

        #endregion

        #region Public methods

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();

            elements.Add(new XElement("Name", this.name));
            elements.Add(new XElement("Url", this.url));
            elements.Add(new XElement("Type", this.type.ToString()));

            return CreateDocument(ROOTNAME, elements);
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            string xml =
                "<Name>skapa order</Name>" +
                "<Url>http://</Url>";
            return XmlUtil.CreateXml(ROOTNAME, xml);
        }

        #endregion
    }

    #endregion

    #region General Feature class

    internal class MobilePermissions : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "Permissions";

        #endregion

        #region Variables

        private readonly List<Tuple<string, int>> permissions = new List<Tuple<string, int>>();

        #endregion

        #region Ctor

        public MobilePermissions(MobileParam param)
            : base(param)
        {
            Init();

        }

        public MobilePermissions(MobileParam param, List<Tuple<string, int>> permissions)
            : base(param)
        {
            Init();

            this.permissions = permissions;

        }

        /// <summary>
        /// Used for errors
        /// </summary>
        public MobilePermissions(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            //Set default values
        }

        #endregion

        #region Public methods

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();

            foreach (var feature in this.permissions)
            {
                elements.Add(new XElement(feature.Item1, feature.Item2.ToString()));
            }

            return CreateDocument(ROOTNAME, elements);
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            string xml =
                "<Permissions1>1</Permissions1>" +
                "<Permissions2>0</Permissions2>";

            return XmlUtil.CreateXml(ROOTNAME, xml);
        }

        #endregion
    }

    #endregion

    #region General Settings class

    internal class MobileSettings : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "Settings";

        #endregion

        #region Variables

        readonly private List<Tuple<string, string>> settings;

        #endregion

        #region Ctor

        public MobileSettings(MobileParam param, List<Tuple<string, string>> settings) : base(param)
        {
            this.settings = settings;
        }

        #endregion

        #region Public methods

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();

            foreach (var setting in this.settings)
            {
                elements.Add(new XElement(setting.Item1, setting.Item2));
            }

            return CreateDocument(ROOTNAME, elements);
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            string xml =
                "<Setting1>1</Setting1>" +
                "<Setting2>0</Setting2>";

            return XmlUtil.CreateXml(ROOTNAME, xml);
        }

        #endregion
    }

    #endregion

    #region MessageBox

    internal class MobileMessageBox : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "MessageBox";

        #endregion

        #region Variables

        private readonly bool success;
        private readonly bool canOverride;
        private readonly bool okButton;
        private readonly bool cancelButton;
        private readonly string title;
        private readonly string message;
        private readonly string data;
        private readonly string overrideData;

        #endregion

        #region Ctor

        public MobileMessageBox(MobileParam param)
            : base(param)
        {
            Init();

        }

        public MobileMessageBox(MobileParam param, bool success, bool canOverride, bool okButton, bool cancelButton, string title, string message, string details = "", string ruleResult = "")
            : base(param)
        {
            Init();

            this.success = success;
            this.canOverride = canOverride;
            this.okButton = okButton;
            this.cancelButton = cancelButton;
            this.title = title;
            this.message = message;
            this.data = details;
            this.overrideData = ruleResult;
        }

        /// <summary>
        /// Used for errors
        /// </summary>
        public MobileMessageBox(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            //Set default values
        }

        #endregion

        #region Public methods

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();

            elements.Add(new XElement("Sucess", StringUtility.GetString(this.success)));
            elements.Add(new XElement("CanOverride", StringUtility.GetString(this.canOverride)));
            elements.Add(new XElement("OKButton", StringUtility.GetString(this.okButton)));
            elements.Add(new XElement("CancelButton", StringUtility.GetString(this.cancelButton)));
            elements.Add(new XElement("Title", this.title));
            elements.Add(new XElement("Message", this.message));
            if (!String.IsNullOrEmpty(this.data))
                elements.Add(new XElement("Data", this.data));
            if (!String.IsNullOrEmpty(overrideData))
                elements.Add(new XElement("overrideData", overrideData));
            

            return CreateDocument(ROOTNAME, elements);
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            string xml =
                "<Sucess>1</Sucess>" +
                "<CanOverride>0</CanOverride>" +
                "<OKButton>0</OKButton>" +
                "<CancelButton>0</CancelButton>" +
                "<Title>0</Title>" +
                "<Message>0</Message>" +
                "<Data></Data>";

            return XmlUtil.CreateXml(ROOTNAME, xml);
        }

        #endregion
    }

    #endregion

    #region AbsenceAnnouncement

    internal class MobileAbsenceAnnouncement : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "AbsenceAnnouncement";

        #endregion

        #region Variables

        public string successMsg;
        #region Field values


        #endregion

        #endregion

        #region Ctor

        public MobileAbsenceAnnouncement(MobileParam param)
            : base(param)
        {
            Init();
        }

        /// <summary>
        /// Used for errors
        /// </summary>
        public MobileAbsenceAnnouncement(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            //Set default values
        }

        #endregion

        #region Public methods

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument(MobileTask task)
        {
            if (base.Failed)
                return MobileMessages.GetErrorMessageDocument(base.ErrorMessage);

            bool result = base.GetTaskResult(task);
            switch (task)
            {
                case MobileTask.SaveAbsenceAnnouncement:
                    return MobileMessages.GetStringMessageDocument(MobileMessages.XML_ELEMENT_MESSAGE, successMsg);
                default:
                    return base.ToXDocument(task);
            }
        }

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();

            return CreateDocument(ROOTNAME, elements);
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            string xml = "";
            return XmlUtil.CreateXml(ROOTNAME, xml);
        }

        #endregion
    }

    #endregion

    #region SaveShift class

    internal class MobileSaveShifts
    {
        #region Variables

        //[#] = next element och [##] = next shift

        public List<MobileSaveShift> ParsedShifts = new List<MobileSaveShift>();
        public bool ParseSucceded = true;
        private readonly int expectedElementCount = 12;

        #endregion

        #region Ctor

        public MobileSaveShifts(String shifts, bool parseAbsenceStartAndStop, bool isVersion17OrHigher, bool isVersion18OrHigher, bool isVersion32OrHigher)
        {
            if (isVersion17OrHigher)
                this.expectedElementCount = 13;
            if (isVersion18OrHigher)
                this.expectedElementCount = 15;
            if(isVersion32OrHigher)
                this.expectedElementCount = 16;

            Parse(shifts, parseAbsenceStartAndStop, isVersion17OrHigher, isVersion18OrHigher, isVersion32OrHigher);
        }

        #endregion

        public bool IsShiftsMissingReplacements()
        {
            return this.ParsedShifts.Any(x => x.EmployeeId == 0);
        }

        #region Help-methods

        private void Parse(String shifts, bool parseAbsenceStartAndStop, bool isVersion17OrHigher, bool isVersion18OrHigher, bool isVersion32OrHigher)
        {
            string[] shiftSeparator = new string[1];
            shiftSeparator[0] = "[##]";
            string[] elementSeparator = new string[1];
            elementSeparator[0] = "[#]";

            string[] separatedAnswers = shifts.Split(shiftSeparator, StringSplitOptions.RemoveEmptyEntries);

            foreach (string separatedAnswer in separatedAnswers)
            {
                string[] separatedElements = separatedAnswer.Trim().Split(elementSeparator, StringSplitOptions.None);
                if (separatedElements.Count() != expectedElementCount)
                {

                    ParseSucceded = false;
                    return;
                }

                string shiftIdStr = separatedElements[0].Trim();
                string employeeIdStr = separatedElements[1].Trim();
                string isBreakStr = separatedElements[2].Trim();
                string startTimeStr = separatedElements[3].Trim();
                string stopTimeStr = separatedElements[4].Trim();
                string absenceStartTimeStr = separatedElements[5].Trim();
                string absenceStopTimeStr = separatedElements[6].Trim();
                string timeCodeBreakIdStr = separatedElements[7].Trim();
                string shiftTypeIdStr = separatedElements[8].Trim();
                string scheduleTypeIdStr = separatedElements[9].Trim();
                string startsAfterMidnightStr = separatedElements[10].Trim();
                string isDeletedStr = separatedElements[11].Trim();
                string accountIdStr = string.Empty;
               
                if (isVersion17OrHigher)
                    accountIdStr = separatedElements[12].Trim();

                string isExtraShiftStr = string.Empty;
                string isSubstituteShiftStr = string.Empty;
                if (isVersion18OrHigher)
                {
                    isExtraShiftStr = separatedElements[13].Trim();
                    isSubstituteShiftStr = separatedElements[14].Trim();
                }

                string typeStr = string.Empty;
                if (isVersion32OrHigher)
                {
                    typeStr = separatedElements[15].Trim();
                }

                #region Parse integers

                if ((!Int32.TryParse(shiftIdStr, out int shiftId)) || (!Int32.TryParse(employeeIdStr, out int employeeId)) || (!Int32.TryParse(timeCodeBreakIdStr, out int timeCodeBreakId)) || (!Int32.TryParse(shiftTypeIdStr, out int shiftTypeId)) || (!Int32.TryParse(scheduleTypeIdStr, out int scheduleTypeId)))
                {
                    ParseSucceded = false;
                    return;
                }

                int accountId = 0;
                if (isVersion17OrHigher && (!Int32.TryParse(accountIdStr, out accountId)))
                {
                    ParseSucceded = false;
                    return;
                }

                int type = 0;
                if (isVersion32OrHigher && (!Int32.TryParse(typeStr, out type)))
                {
                    ParseSucceded = false;
                    return;
                }

                #endregion

                #region Parse datetimes

                DateTime? startTime = null;
                DateTime? stopTime = null;
                DateTime? absenceStartTime = null;
                DateTime? absenceStopTime = null;

                if (!String.IsNullOrEmpty(startTimeStr))
                    startTime = CalendarUtility.GetNullableDateTime(startTimeStr);

                if (!String.IsNullOrEmpty(stopTimeStr))
                    stopTime = CalendarUtility.GetNullableDateTime(stopTimeStr);

                if (!String.IsNullOrEmpty(absenceStartTimeStr))
                    absenceStartTime = CalendarUtility.GetNullableDateTime(absenceStartTimeStr);

                if (!String.IsNullOrEmpty(absenceStopTimeStr))
                    absenceStopTime = CalendarUtility.GetNullableDateTime(absenceStopTimeStr);


                if (!startTime.HasValue || !stopTime.HasValue)
                {
                    ParseSucceded = false;
                    return;
                }

                if (parseAbsenceStartAndStop && (!absenceStartTime.HasValue || !absenceStopTime.HasValue))
                {
                    ParseSucceded = false;
                    return;
                }

                #endregion

                #region Parse bools

                bool? isBreak = null;
                bool? startsAfterMidnight = null;
                bool? isDeleted = null;
                bool? isExtraShift = null;
                bool? isSubstituteShift = null;

                isBreak = StringUtility.GetNullableBool(isBreakStr);
                startsAfterMidnight = StringUtility.GetNullableBool(startsAfterMidnightStr);
                isDeleted = StringUtility.GetNullableBool(isDeletedStr);
                isExtraShift = StringUtility.GetNullableBool(isExtraShiftStr);
                isSubstituteShift = StringUtility.GetNullableBool(isSubstituteShiftStr);

                if (!isBreak.HasValue || !startsAfterMidnight.HasValue || !isDeleted.HasValue)
                {
                    ParseSucceded = false;
                    return;
                }

                if (isVersion18OrHigher && (!isExtraShift.HasValue || !isSubstituteShift.HasValue))
                {
                    ParseSucceded = false;
                    return;
                }

                isExtraShift = isExtraShift ?? false;
                isSubstituteShift = isSubstituteShift ?? false;

                #endregion

                MobileSaveShift shift = new MobileSaveShift(shiftId, employeeId, isBreak.Value, startTime.Value, stopTime.Value, absenceStartTime, absenceStopTime, timeCodeBreakId, shiftTypeId, scheduleTypeId, startsAfterMidnight.Value, isDeleted.Value, accountId, isExtraShift.Value, isSubstituteShift.Value, type);
                ParsedShifts.Add(shift);
            }
        }

        #endregion
    }

    internal class MobileSaveShift
    {
        #region Variables

        public int ShiftId;
        public int EmployeeId;
        public bool IsBreak;
        public DateTime StartTime;
        public DateTime StopTime;
        public DateTime? AbsenceStartTime;
        public DateTime? AbsenceStopTime;
        public int TimeCodeBreakId;
        public int ShiftTypeId;
        public int ScheduleTypeId;
        public bool StartsAfterMidnight;
        public bool IsDeleted;
        public int AccountId;
        public bool isExtraShift;
        public bool isSubStituteShift;
        public int type;

        #endregion

        #region Ctor

        public MobileSaveShift(int shiftId, int employeeId, bool isBreak, DateTime startTime, DateTime stopTime, DateTime? absenceStartTime, DateTime? absenceStopTime, int timeCodeBreakId, int shiftTypeId, int scheduleTypeId, bool startsAfterMidnight, bool isDeleted, int accountId, bool isExtraShift, bool isSubStituteShift, int type)
        {
            this.ShiftId = shiftId;
            this.EmployeeId = employeeId;
            this.IsBreak = isBreak;
            this.StartTime = startTime;
            this.StopTime = stopTime;
            this.AbsenceStartTime = absenceStartTime;
            this.AbsenceStopTime = absenceStopTime;
            this.TimeCodeBreakId = timeCodeBreakId;
            this.ShiftTypeId = shiftTypeId;
            this.ScheduleTypeId = scheduleTypeId;
            this.StartsAfterMidnight = startsAfterMidnight;
            this.IsDeleted = isDeleted;
            this.AccountId = accountId;
            this.isExtraShift = isExtraShift;
            this.isSubStituteShift = isSubStituteShift;
            this.type = type;
        }

        #endregion
    }

    #endregion

    #region ShiftRequestUsers

    internal class MobileShiftRequestUsers : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "ShiftRequestUsers";

        #endregion

        #region Variables

        #region Collections

        private List<MobileShiftRequestUser> mobileShiftRequestUsers;

        #endregion

        #endregion

        #region Ctor

        public MobileShiftRequestUsers(MobileParam param)
            : base(param)
        {
            Init();
        }

        public MobileShiftRequestUsers(MobileParam param, List<MessageRecipientDTO> recipients)
            : base(param)
        {
            Init();
            AddMobileShiftRequestUsers(recipients);
        }

        /// <summary>Used for errors</summary>
        public MobileShiftRequestUsers(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            this.mobileShiftRequestUsers = new List<MobileShiftRequestUser>();
        }

        #endregion

        #region Public methods       

        public void AddMobileShiftRequestUsers(List<MessageRecipientDTO> recipients)
        {
            if (recipients == null)
                return;


            foreach (var recipient in recipients)
            {
                AddMobileShiftRequestUser(new MobileShiftRequestUser(this.Param, recipient));
            }
        }

        public void AddMobileShiftRequestUser(MobileShiftRequestUser recipient)
        {
            if (recipient == null)
                return;

            mobileShiftRequestUsers.Add(recipient);
        }

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            return MergeDocuments(ROOTNAME, mobileShiftRequestUsers.Select(i => i.ToXDocument()).ToList());
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            return XmlUtil.CreateXml(ROOTNAME, MobileShiftRequestUser.GetDefaultXml());
        }

        #endregion
    }

    internal class MobileShiftRequestUser : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "ShiftRequestUser";

        #endregion

        #region Variables

        #region Field values

        private readonly int userId;
        private readonly string name;
        private readonly string color;
        private readonly string userName;

        #endregion

        #endregion

        #region Ctor

        public MobileShiftRequestUser(MobileParam param)
            : base(param)
        {
            Init();

        }

        public MobileShiftRequestUser(MobileParam param, MessageRecipientDTO recipient)
            : base(param)
        {
            Init();

            this.userId = recipient.UserId;
            this.name = recipient.Name;
            this.userName = recipient.UserName;

            #region Set color : See EmployeeRequestTypeColorConverter

            var type = recipient.EmployeeRequestTypeFlags;

            //Remove partly defined 
            var typeInt = (int)(type & ~TermGroup_EmployeeRequestTypeFlags.PartyDefined);

            if (typeInt == (int)TermGroup_EmployeeRequestType.InterestRequest)
            {
                this.color = "#FF008000"; //Green                
            }
            else
            {
                this.color = "#FFFFFFFF"; //White

            }

            #endregion

        }

        /// <summary>
        /// Used for errors
        /// </summary>
        public MobileShiftRequestUser(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            //Set default values
        }

        #endregion

        #region Public methods

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();

            elements.Add(new XElement("UserId", this.userId));
            elements.Add(new XElement("Color", this.color));
            elements.Add(new XElement("UName", this.userName));
            elements.Add(new XElement("Name", this.name));

            return CreateDocument(ROOTNAME, elements);
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            string xml =

                "<UserId></UserId>" +
                "<Color></Color>" +
                "<UName></UName>" +
                "<Name></Name>";
            return XmlUtil.CreateXml(ROOTNAME, xml);
        }

        #endregion
    }

    #endregion

    #region Accounting on orderrow (only professional for now)

    internal class MobileAccountSettings : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "AccountSettings";

        #endregion

        #region Variables

        #region Collections

        private List<MobileAccountSetting> mobileAccountSettings;

        #endregion

        #endregion

        #region Ctor

        public MobileAccountSettings(MobileParam param)
            : base(param)
        {
            Init();
        }

        /// <summary>Used for errors</summary>
        public MobileAccountSettings(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            this.mobileAccountSettings = new List<MobileAccountSetting>();
        }

        #endregion

        #region Public methods

        public void AddMobileAccountSetting(int accountOrder, string heading)
        {
            mobileAccountSettings.Add(new MobileAccountSetting(this.Param, accountOrder, heading));
        }

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            return MergeDocuments(ROOTNAME, mobileAccountSettings.Select(i => i.ToXDocument()).ToList());
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            return XmlUtil.CreateXml(ROOTNAME, MobileAccountSetting.GetDefaultXml());
        }

        #endregion
    }

    internal class MobileAccountSetting : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "AccountSetting";

        #endregion

        #region Variables

        #region Field values

        private readonly int accountOrder;
        private readonly string heading;

        #endregion

        #endregion

        #region Ctor

        public MobileAccountSetting(MobileParam param)
            : base(param)
        {
            Init();

        }

        public MobileAccountSetting(MobileParam param, int accountOrder, String heading)
            : base(param)
        {
            Init();

            this.accountOrder = accountOrder;
            this.heading = heading;
        }

        /// <summary>
        /// Used for errors
        /// </summary>
        public MobileAccountSetting(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            //Set default values
        }

        #endregion

        #region Public methods

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();

            elements.Add(new XElement("AccountOrder", this.accountOrder));
            elements.Add(new XElement("Heading", this.heading));

            return CreateDocument(ROOTNAME, elements);
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            string xml =
                "<Order></Order>" +
                "<Heading></Heading>";
            return XmlUtil.CreateXml(ROOTNAME, xml);
        }

        #endregion
    }

    internal class MobileAccounts : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "Accounts";

        #endregion

        #region Variables

        #region Collections

        private List<MobileAccount> mobileAccounts;

        #endregion

        #endregion

        #region Ctor

        public MobileAccounts(MobileParam param)
            : base(param)
        {
            Init();
        }

        /// <summary>Used for errors</summary>
        public MobileAccounts(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            this.mobileAccounts = new List<MobileAccount>();
        }

        #endregion

        #region Public methods

        public void AddMobileAccount(int accountOrder, int accountId, string name)
        {
            mobileAccounts.Add(new MobileAccount(this.Param, accountOrder, accountId, name));
        }

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            return MergeDocuments(ROOTNAME, mobileAccounts.Select(i => i.ToXDocument()).ToList());
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            return XmlUtil.CreateXml(ROOTNAME, MobileAccount.GetDefaultXml());
        }

        #endregion
    }

    internal class MobileAccount : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "Account";

        #endregion

        #region Variables

        #region Field values

        public int id;
        public bool selected;
        private readonly string name;
        private readonly int accountOrder;


        #endregion

        #endregion

        #region Ctor

        public MobileAccount(MobileParam param)
            : base(param)
        {
            Init();

        }

        public MobileAccount(MobileParam param, int accountOrder, int accountId, string name)
            : base(param)
        {
            Init();

            this.accountOrder = accountOrder;
            this.id = accountId;
            this.name = name;
        }
        public MobileAccount(MobileParam param, int accountId, string name, bool selected)
           : base(param)
        {
            Init();

            this.id = accountId;
            this.name = name;
            this.selected = selected;
        }

        /// <summary>
        /// Used for errors
        /// </summary>
        public MobileAccount(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            //Set default values
        }

        #endregion

        #region Public methods

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();

            elements.Add(new XElement("AccountOrder", this.accountOrder));
            elements.Add(new XElement("AccountId", this.id));
            elements.Add(new XElement("Name", this.name));

            return CreateDocument(ROOTNAME, elements);
        }

        public XElement ToXElement()
        {
            XElement element = new XElement(ROOTNAME);

            element.Add(new XElement("AId", this.id));
            element.Add(new XElement("AN", this.name));
            element.Add(new XElement("AS", StringUtility.GetString(this.selected)));

            return element;
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            string xml =
                "<AccountOrder></AccountOrder>" +
                "<AccountId></AccountId>" +
                "<Name></Name>";
            return XmlUtil.CreateXml(ROOTNAME, xml);
        }

        #endregion
    }

    #endregion

    #region CopyMoveOrder

    internal class MobileCopyMoveOrders : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "CopyMoveOrders";

        #endregion

        #region Variables

        #region Collections

        private List<MobileCopyMoveOrder> mobileCopyMoveOrders;

        #endregion

        #endregion

        #region Ctor

        public MobileCopyMoveOrders(MobileParam param, List<CustomerInvoiceGridDTO> orders)
            : base(param)
        {
            Init();
            AddMobileCopyMoveOrders(orders);
        }

        /// <summary>Used for errors</summary>
        public MobileCopyMoveOrders(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            this.mobileCopyMoveOrders = new List<MobileCopyMoveOrder>();
        }

        #endregion

        #region Public methods

        public void AddMobileCopyMoveOrders(List<CustomerInvoiceGridDTO> orders)
        {
            if (orders == null)
                return;

            foreach (var order in orders)
            {
                AddMobileCopyMoveOrder(new MobileCopyMoveOrder(this.Param, order.CustomerInvoiceId, order.InvoiceNr, order.ActorCustomerName, order.ActorCustomerNr, order.DeliveryAddress, order.InternalText));
            }
        }

        public void AddMobileCopyMoveOrder(MobileCopyMoveOrder mobileCopyMoveOrder)
        {
            if (mobileCopyMoveOrder == null)
                return;

            mobileCopyMoveOrders.Add(mobileCopyMoveOrder);
        }

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            return MergeDocuments(ROOTNAME, mobileCopyMoveOrders.Select(i => i.ToXDocument()).ToList());
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            return XmlUtil.CreateXml(ROOTNAME, MobileCopyMoveOrder.GetDefaultXml());
        }

        #endregion
    }

    internal class MobileCopyMoveOrder : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "CopyMoveOrder";

        #endregion

        #region Variables

        #region Field values

        private readonly int orderId;
        private readonly string orderNumber;
        private readonly string customerName;
        private readonly string customerNumber;
        private readonly string deliveryAddress;
        private readonly string internalText;

        #endregion

        #endregion

        #region Ctor

        public MobileCopyMoveOrder(MobileParam param, int orderId, string orderNumber, string customerName, string customerNumber, string deliveryAddress, string internalText)
            : base(param)
        {
            Init();

            this.orderId = orderId;
            this.orderNumber = orderNumber;
            this.customerName = customerName;
            this.customerNumber = customerNumber;
            this.deliveryAddress = deliveryAddress ?? "";
            this.internalText = internalText ?? "";

        }

        /// <summary>
        /// Used for errors
        /// </summary>
        public MobileCopyMoveOrder(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            //Set default values
        }

        #endregion

        #region Public methods

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();

            elements.Add(new XElement("OrderId", orderId.ToString()));

            if (mobileManagerUtil.IsCallerExpectedVersionNewerThenGivenVersion(Param.Version, Constants.MOBILE_WS_SUPPORTED_OLDVERSION_43))
            {
                elements.Add(new XElement("OrderNumber", GetTextOrCDATA(orderNumber)));
                elements.Add(new XElement("CustomerName", GetTextOrCDATA(customerName)));
                elements.Add(new XElement("CustomerNumber", GetTextOrCDATA(customerNumber)));
                elements.Add(new XElement("DeliveryAddress", GetTextOrCDATA(deliveryAddress)));
                elements.Add(new XElement("InternalText", GetTextOrCDATA(internalText)));
            }
            else
            {
                elements.Add(new XElement("OrderNbr", GetTextOrCDATA(orderNumber + " (" + customerNumber + " " + customerName + ")")));
            }

            return CreateDocument(ROOTNAME, elements);
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            string xml =
                "<OrderId>1</OrderId>" +
                "<OrderNbr>Momstyp 1</OrderNbr>";
            return XmlUtil.CreateXml(ROOTNAME, xml);
        }

        #endregion
    }

    #endregion

    #region MobileChangePWD

    internal class MobileChangePWD : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "PWDPolicies";

        #endregion

        #region Variables

        private readonly List<Tuple<string, string>> policies = new List<Tuple<string, string>>();

        #endregion

        #region Ctor

        public MobileChangePWD(MobileParam param)
            : base(param)
        {
            Init();

        }

        public MobileChangePWD(MobileParam param, List<Tuple<string, string>> policies)
            : base(param)
        {
            Init();

            this.policies = policies;

        }

        /// <summary>
        /// Used for errors
        /// </summary>
        public MobileChangePWD(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            //Set default values
        }

        #endregion

        #region Public methods

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();

            foreach (var policy in this.policies)
            {
                elements.Add(new XElement(policy.Item1, policy.Item2.ToString()));
            }

            return CreateDocument(ROOTNAME, elements);
        }

        public override XDocument ToXDocument(MobileTask task)
        {
            if (base.Failed)
                return MobileMessages.GetErrorMessageDocument(base.ErrorMessage);

            bool result = base.GetTaskResult(task);
            switch (task)
            {
                case MobileTask.ChangePWD:
                    return MobileMessages.GetSuccessDocument(result);
                default:
                    return base.ToXDocument(task);
            }
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            string xml =
                "<Policy1>xxx</Policy1>" +
                "<Policy2>yyy</Policy2>";

            return XmlUtil.CreateXml(ROOTNAME, xml);
        }

        #endregion
    }

    #endregion

    #region MobileTimeTerminal

    internal class MobileTimeTerminals : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "MobileTimeTerminals";

        #endregion

        #region Variables

        private List<MobileTimeTerminal> mobileTimeTerminals { get; set; }

        #endregion

        #region Ctor

        public MobileTimeTerminals(MobileParam param, List<TimeTerminalDTO> timeTerminals)
            : base(param)
        {
            Init();
            AddTimeTerminals(timeTerminals);
        }

        /// <summary>Used for errors</summary>
        public MobileTimeTerminals(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            this.mobileTimeTerminals = new List<MobileTimeTerminal>();
        }

        #endregion

        #region Public methods

        public void AddTimeTerminals(List<TimeTerminalDTO> timeTerminals)
        {
            if (timeTerminals == null)
                return;

            foreach (TimeTerminalDTO timeTerminal in timeTerminals)
            {
                AddTimeTerminal(new MobileTimeTerminal(this.Param, timeTerminal));
            }
        }

        public void AddTimeTerminal(MobileTimeTerminal mobileTimeTerminal)
        {
            if (mobileTimeTerminal == null)
                return;

            mobileTimeTerminals.Add(mobileTimeTerminal);
        }

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            return MergeDocuments(ROOTNAME, this.mobileTimeTerminals.Select(i => i.ToXDocument()).ToList());
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            return XmlUtil.CreateXml(ROOTNAME, MobileTimeTerminal.GetDefaultXml());
        }

        #endregion
    }

    internal class MobileTimeTerminal : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "MobileTimeTerminal";

        #endregion

        #region Variables

        private readonly int timeTerminalId;
        private readonly string name;
        private readonly string uri;

        #endregion

        #region Ctor

        public MobileTimeTerminal(MobileParam param, TimeTerminalDTO timeTerminal)
            : base(param)
        {
            Init();
            this.timeTerminalId = timeTerminal.TimeTerminalId;
            this.name = timeTerminal.Name;
            this.uri = timeTerminal.Uri;
        }

        /// <summary>
        /// Used for errors
        /// </summary>
        public MobileTimeTerminal(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            //Set default values
        }

        #endregion

        #region Public methods

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();
            elements.Add(new XElement("TimeTeminalId", this.timeTerminalId));
            elements.Add(new XElement("Name", this.name));
            elements.Add(new XElement("Uri", this.uri));

            return CreateDocument(ROOTNAME, elements);
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            string xml =
                "<TimeTerminalId></TimeTerminalId>" +
                "<Name></Name>" +
                "<Uri></Uri>";

            return XmlUtil.CreateXml(ROOTNAME, xml);
        }

        #endregion
    }

    #endregion

    #region MobileTimeStampAttendancies

    internal class MobileTimeStampAttendancies : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "TimeStampAttendancies";

        #endregion

        #region Variables

        #region Collections

        private List<MobileTimeStampAttendance> mobileTimeStampAttendancies;

        #endregion

        #endregion

        #region Ctor

        public MobileTimeStampAttendancies(MobileParam param)
            : base(param)
        {
            Init();
        }

        public MobileTimeStampAttendancies(MobileParam param, List<TimeStampAttendanceGaugeDTO> dtos)
            : base(param)
        {
            Init();
            AddMobileTimeStampAttendancies(dtos);
        }

        /// <summary>Used for errors</summary>
        public MobileTimeStampAttendancies(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            this.mobileTimeStampAttendancies = new List<MobileTimeStampAttendance>();
        }

        #endregion

        #region Public methods

        public void AddMobileTimeStampAttendancies(List<TimeStampAttendanceGaugeDTO> attendances)
        {
            if (attendances == null)
                return;

            foreach (TimeStampAttendanceGaugeDTO attendance in attendances)
            {
                AddMobileTimeStampAttendance(new MobileTimeStampAttendance(this.Param, attendance));
            }
        }

        public void AddMobileTimeStampAttendance(MobileTimeStampAttendance attendance)
        {
            if (attendance == null)
                return;

            mobileTimeStampAttendancies.Add(attendance);
        }

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            return MergeDocuments(ROOTNAME, mobileTimeStampAttendancies.Select(i => i.ToXDocument()).ToList());
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            return XmlUtil.CreateXml(ROOTNAME, MobileTimeStampAttendance.GetDefaultXml());
        }

        #endregion
    }

    internal class MobileTimeStampAttendance : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "TimeStampAttendance";

        #endregion

        #region Variables

        #region Field values

        private readonly DateTime time;
        private readonly string typeName;
        private readonly string name;
        private readonly string timeTerminalName;
        private readonly DateTime? scheduleStartTime;
        private readonly bool isMissing;

        #endregion

        #endregion

        #region Ctor

        public MobileTimeStampAttendance(MobileParam param)
            : base(param)
        {
            Init();

        }

        public MobileTimeStampAttendance(MobileParam param, TimeStampAttendanceGaugeDTO dto)
            : base(param)
        {
            Init();

            this.isMissing = dto.IsMissing;
            this.time = dto.Time;
            this.typeName = dto.TypeName;
            this.name = dto.Name;
            this.timeTerminalName = dto.TimeTerminalName;
            this.scheduleStartTime = dto.ScheduleStartTime;

        }

        /// <summary>
        /// Used for errors
        /// </summary>
        public MobileTimeStampAttendance(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            //Set default values
        }

        #endregion

        #region Public methods

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();

            if (this.isMissing)
            {
                elements.Add(new XElement("IsMissing", 1));
                elements.Add(new XElement("Type", this.typeName));
                elements.Add(new XElement("Name", this.name));
                if (mobileManagerUtil.IsCallerExpectedVersionNewerThenGivenVersion(Param.Version, Constants.MOBILE_WS_SUPPORTED_OLDVERSION_36))
                {
                    elements.Add(new XElement("Start", StringUtility.GetSwedishFormattedTime(this.scheduleStartTime.Value)));
                }
                else
                {
                    elements.Add(new XElement("Start", this.scheduleStartTime.Value.ToShortTimeString()));
                }
            }
            else
            {
                elements.Add(new XElement("Type", this.typeName));
                elements.Add(new XElement("Name", this.name));
                elements.Add(new XElement("TTN", this.timeTerminalName));
                if (this.scheduleStartTime.HasValue)
                {
                    if (mobileManagerUtil.IsCallerExpectedVersionNewerThenGivenVersion(Param.Version, Constants.MOBILE_WS_SUPPORTED_OLDVERSION_36))
                    {
                        elements.Add(new XElement("Time", StringUtility.GetSwedishFormattedTime(this.time)));
                        elements.Add(new XElement("Start", StringUtility.GetSwedishFormattedTime(this.scheduleStartTime.Value)));
                    }
                    else
                    {
                        elements.Add(new XElement("Start", this.scheduleStartTime.Value.ToShortTimeString()));
                        elements.Add(new XElement("Time", this.time.ToShortTimeString()));

                    }
                }
            }
            return CreateDocument(ROOTNAME, elements);
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            string xml =
                "<Time></Time>" +
                "<Type></Type>" +
                "<Name></Name>" +
                "<TDCN></TDCN>" +
                "<AN></AN>" +
                "<TTN></TTN>";

            return XmlUtil.CreateXml(ROOTNAME, xml);
        }

        #endregion
    }

    #endregion

    #region EvacuationLists

    internal class EvacuationLists : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "EvacuationLists";

        #endregion

        #region Variables

        #region Collections

        private List<EvacuationList> evacuationLists;
        private string defaultDimName;

        #endregion

        #endregion

        #region Ctor

        public EvacuationLists(MobileParam param)
            : base(param)
        {
            Init();
        }

        /// <summary>Used for errors</summary>
        public EvacuationLists(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            this.evacuationLists = new List<EvacuationList>();
        }

        #endregion

        #region Public methods

        public void AddEvacuationLists(Employee employee, TimeStampEntryDTO attendance, TimeSchedulePlanningAggregatedDayDTO employeeDay, List<ShiftType> shiftTypes, TimeSchedulePlanningDayDTO dayDTO, AccountDim accountDim, List<EvacuationListRowDTO> markings)
        {
            if (employee == null)
                return;

            defaultDimName = accountDim?.Name ?? string.Empty;

            AddEvacuationList(new EvacuationList(this.Param, employee, attendance, employeeDay, shiftTypes, dayDTO, markings));
         
        }

        public void AddEvacuationList(EvacuationList attendance)
        {
            if (attendance == null)
                return;

            evacuationLists.Add(attendance);
        }

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {

            var elements = new List<XElement>
            {
                {new XElement( "DefaultDimName", GetTextOrCDATA(defaultDimName)) }
            };

            return MergeDocuments(ROOTNAME, evacuationLists.Select(i => i.ToXDocument()).ToList(), elements);
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            return XmlUtil.CreateXml(ROOTNAME, EvacuationList.GetDefaultXml());
        }

        #endregion
    }

    internal class EvacuationList : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "EvacuationList";

        #endregion

        #region Variables

        #region Field values

        private readonly int employeeId;
        private readonly string employeeNr;
        private readonly string employeeName;
        private readonly string firstName;
        private readonly string lastName;
        private readonly int scheduleAccountId;
        private readonly string scheduleAccountName;
        private readonly int shiftTypeId;
        private readonly string shiftTypeName;
        private readonly DateTime? scheduleStartTime;
        private readonly DateTime? scheduleStopTime;
        private readonly bool scheduleMissing;
        private readonly string absceneName;
        private readonly int entryType;
        private readonly DateTime? entryTime;
        private readonly string entryAbs;
        private readonly bool entryMissing;
        private readonly string entryAccountName;
        private readonly int entryAccountId;
        private readonly bool isBreak;
        private readonly bool ownMarked;
        private readonly bool otherMarked;

        #endregion

        #endregion

        #region Ctor

        public EvacuationList(MobileParam param)
            : base(param)
        {
            Init();

        }

        public EvacuationList(MobileParam param, Employee employee, TimeStampEntryDTO attendance, TimeSchedulePlanningAggregatedDayDTO employeeDay, List<ShiftType> shiftTypes, TimeSchedulePlanningDayDTO dayDTO, List<EvacuationListRowDTO> markings)
            : base(param)
        {
            Init();
            employeeId = employee.EmployeeId;
            employeeNr = employee.EmployeeNr;
            employeeName = employee.Name;
            firstName = employee.FirstName;
            lastName = employee.LastName;
            scheduleAccountId = dayDTO != null && dayDTO.AccountId.HasValue ? dayDTO.AccountId.Value : 0;
            scheduleAccountName = dayDTO?.AccountName ?? string.Empty;
            shiftTypeId = dayDTO?.ShiftTypeId ?? 0;
            shiftTypeName = dayDTO != null ? shiftTypes.FirstOrDefault(w => w.ShiftTypeId == dayDTO.ShiftTypeId)?.Name : string.Empty;
            absceneName = dayDTO?.TimeDeviationCauseName ?? string.Empty;            
            scheduleStartTime = employeeDay?.ScheduleStartTime ?? null;
            scheduleStopTime = employeeDay?.ScheduleStopTime ?? null;
            scheduleMissing = employeeDay == null;
            entryMissing = !attendance?.Time.IsDateTime() ?? true;
            entryType = (int)(attendance?.Type ?? 0);
            entryTime = attendance?.Time ?? null;
            entryAbs = attendance?.TimeDeviationCauseName ?? string.Empty;
            entryAccountName = attendance?.AccountName ?? string.Empty;
            entryAccountId = attendance?.AccountId ?? 0;
            isBreak = attendance?.IsBreak ?? false;
            ownMarked = markings.Any(w => w.UserId == param.UserId && w.State == SoeEntityState.Active);
            otherMarked = markings.Any(w => w.UserId != param.UserId && w.State == SoeEntityState.Active);
        }

        public EvacuationList(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            //Set default values
        }

        #endregion

        #region Public methods

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>
            {
                new XElement("EmpId", employeeId),
                new XElement("EmpNr", GetTextOrCDATA(employeeNr)),
                new XElement("SchAccId", scheduleAccountId),
                new XElement("SchAccName", GetTextOrCDATA(scheduleAccountName)),
                new XElement("ShiftTypeId", shiftTypeId),
                new XElement("ShiftTypeName", GetTextOrCDATA(shiftTypeName)),
                new XElement("SchStart", scheduleStartTime.HasValue ? StringUtility.GetSwedishFormattedTime(scheduleStartTime.Value) : ""),
                new XElement("SchStop", scheduleStopTime.HasValue ? StringUtility.GetSwedishFormattedTime(scheduleStopTime.Value) : ""),
                new XElement("SchMissing", StringUtility.GetString(scheduleMissing)),
                new XElement("AbsName", GetTextOrCDATA(absceneName)),
                new XElement("EntryMissing", StringUtility.GetString(entryMissing)),
                new XElement("EntryType", entryType),
                new XElement("EntryTime", entryTime.HasValue && !entryMissing ? StringUtility.GetSwedishFormattedTime(entryTime.Value) : ""),
                new XElement("EntryAbs", GetTextOrCDATA(entryAbs)),
                new XElement("EntryAccName", GetTextOrCDATA(entryAccountName)),
                new XElement("EntryAccId", entryAccountId),
                new XElement("IsBreak", StringUtility.GetString(isBreak)),
                new XElement("OWN", StringUtility.GetString(ownMarked)),
                new XElement("Other", StringUtility.GetString(otherMarked)),
            };

            elements.Add(new XElement("FName", GetTextOrCDATA(firstName)));
            elements.Add(new XElement("LName", GetTextOrCDATA(lastName)));
            elements.Add(new XElement("EmpName", GetTextOrCDATA(employeeName)));

            return CreateDocument(ROOTNAME, elements);
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            string xml = "";

            return XmlUtil.CreateXml(ROOTNAME, xml);
        }

        #endregion
    }

    internal class UpdateEvacuationList : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "UpdateEvacuationList";

        #endregion

        #region Variables

        #region Field values

        private readonly bool success;
        private readonly int headId;
        private readonly EvacuationListMarkings evacuationListMarkings;
        
        #endregion

        #endregion

        #region Ctor

        public UpdateEvacuationList(MobileParam param)
            : base(param)
        {
            Init();
            success = true;
        }

        public UpdateEvacuationList(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
            success = false;
        }

        public UpdateEvacuationList(MobileParam param, EvacuationListMarkings markings, List<int> employeeIds, int headId) :
           base(param)
        {
            Init();
            success = true;
            this.headId = headId;
            evacuationListMarkings = markings;
        }

        private void Init()
        {
            //set defualt Values
        }
        #endregion

        #region Public methods

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>
            {
                new XElement("Success", StringUtility.GetString(success)),
                new XElement("HeadId", headId),
            };

            return MergeDocuments(ROOTNAME, new List<XDocument> { this.evacuationListMarkings.ToXDocument() }, elements);
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            string xml = "";

            return XmlUtil.CreateXml(ROOTNAME, xml);
        }

        #endregion
    }

    internal class EvacuationListMarkings : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "EvacuationListMarkings";

        #endregion

        #region Variables

        #region Field values

         List<EvacuationListMarking> evacuationListMarking;

        #endregion

        #endregion

        #region Ctor
        public EvacuationListMarkings(MobileParam param)
             : base(param)
        {
            Init();

        }

        public EvacuationListMarkings(MobileParam param, List<EvacuationListRowDTO> rows , List<int> employeeIds, int headId)
            : base(param)
        {
            Init();
            foreach (var employeeId in employeeIds)
            {
                bool own = rows.Any(w => w.EmployeeId == employeeId && w.State == SoeEntityState.Active && w.EvacuationListHeadId == headId);
                bool other = rows.Any(w => w.EmployeeId == employeeId && w.State == SoeEntityState.Active && w.EvacuationListHeadId != headId);
                AddMarking(new EvacuationListMarking(param, employeeId, own, other));

            }
        }

        public void AddMarking(EvacuationListMarking markng)
        {
            if (markng == null)
                return;

            evacuationListMarking.Add(markng);
        }
        public EvacuationListMarkings(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            this.evacuationListMarking = new List<EvacuationListMarking>();
        }


        #endregion

        #region Public methods

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            return MergeDocuments(ROOTNAME, evacuationListMarking.Select(i => i.ToXDocument()).ToList());
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            string xml = "";

            return XmlUtil.CreateXml(ROOTNAME, xml);
        }

        #endregion
    }

    internal class EvacuationListMarking : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "EvacuationListMarking";

        #endregion

        #region Variables

        #region Field values

        private readonly int employeeId;
        private readonly bool ownMarked;
        private readonly bool otherMarked;
        private readonly bool info;

        #endregion

        #endregion

        #region Ctor

        public EvacuationListMarking(MobileParam param)
            : base(param)
        {
            Init();

        }
        public EvacuationListMarking(MobileParam param, int employeeId, bool own, bool other)
            : base(param)
        {
            Init();

            this.employeeId = employeeId;
            this.ownMarked = own;
            this.otherMarked = other;
            this.info = own || other;
        }

        public EvacuationListMarking(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {

            // Set default values
        }

        #endregion

        #region Public methods

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>
            {
                new XElement("EmpId", employeeId),
                new XElement("OWN", StringUtility.GetString(ownMarked)),
                new XElement("Other", StringUtility.GetString(otherMarked)),
                new XElement("Info", StringUtility.GetString(info)),

            };

            return CreateDocument(ROOTNAME, elements);
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            string xml = "";

            return XmlUtil.CreateXml(ROOTNAME, xml);
        }

        #endregion
    }

    internal class EvacuationListHistory : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "EvacuationListHistory";

        #endregion

        #region Variables

        #region Field values

        private readonly int employeeId;
        private readonly DateTime? created;
        private readonly DateTime? modified;
        private readonly int userId;
        private readonly int state;
        private readonly string headName;

        #endregion

        #endregion

        #region Ctor

        public EvacuationListHistory(MobileParam param)
            : base(param)
        {
            Init();
                
        }

        public EvacuationListHistory(MobileParam param,  EvacuationListRowDTO row)
            : base(param)
        {
            Init();
            employeeId = row.EmployeeId;
            created = row.Created;
            modified = row.Modified;
            userId = row.UserId;
            headName = row.HeadName;
            state = (int)row.State;
        }


        private void Init()
        {
            // Set defualt values
        }

        #endregion

        #region Public methods

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>
            {
                new XElement("EmpId", employeeId),
                new XElement("UserId", userId),
                new XElement("Name",  GetTextOrCDATA(headName)),
                new XElement("Created", created.HasValue ? StringUtility.GetSwedishFormattedDateTime(created.Value) : null),
                new XElement("Modified", modified.HasValue ? StringUtility.GetSwedishFormattedDateTime(modified.Value) : null),
                new XElement("State", state),

            };

            return CreateDocument(ROOTNAME, elements);
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            string xml = "";

            return XmlUtil.CreateXml(ROOTNAME, xml);
        }

        #endregion
    }
    internal class EvacuationListHistorys : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "EvacuationListHistorys";

        #endregion

        #region Variables

        #region Field values

        private readonly int employeeId;
        private List<EvacuationListHistory> evacuationListHistorys;

        #endregion

        #endregion

        #region Ctor

        public EvacuationListHistorys(MobileParam param)
            : base(param)
        {
            Init();

        }
        public EvacuationListHistorys(MobileParam param, string errorMessage) 
            : base(param, errorMessage)
        {
            Init();
        }
        public EvacuationListHistorys(MobileParam param, EvacuationListDTO dto, int employeeId)
          : base(param)
        {
            Init();

            if (dto.EvacuationListRow.Any())
            {
                foreach (var row in dto.EvacuationListRow)
                {
                    this.employeeId = employeeId;
                    if(row.State == SoeEntityState.Active)
                        AddRows(new EvacuationListHistory(param, row));

                }
            
            }
        }

        public void AddRows(EvacuationListHistory row)
        {
            if (row == null)
                return;

            evacuationListHistorys.Add(row);

        }
        private void Init()
        {
            this.evacuationListHistorys = new List<EvacuationListHistory>();
        }

        #endregion

        #region Public methods

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>
            {
                new XElement("EmpId", employeeId),

            };

            return MergeDocuments(ROOTNAME, evacuationListHistorys.Select(i => i.ToXDocument()).ToList(), elements);
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            string xml = "";

            return XmlUtil.CreateXml(ROOTNAME, xml);
        }

        #endregion
    }
    #endregion

    #region TimeWorkAccount

    internal class TimeWorkAccountOptions : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "TimeWorkAccountOptions";

        #endregion

        #region Variables

        #region Collections
        private int selectedWithdrawalMethod;
        private DateTime? selectedDate;
        private string text1;
        private string text2;
        private bool readOnly;
        private List<TimeWorkAccountOption> timeWorkAccountOption;
        
        #endregion

        #endregion

        #region Ctor

        public TimeWorkAccountOptions(MobileParam param)
            : base(param)
        {
            Init();
        }

        /// <summary>Used for errors</summary>
        public TimeWorkAccountOptions(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            this.timeWorkAccountOption = new List<TimeWorkAccountOption>();
        }

        #endregion

        #region Public methods

        public void AddTimeWorkAccountOptions(TimeWorkAccountYearEmployee dto, Dictionary<string,string> textDict)
        {
            if (dto == null)
                return;

            selectedWithdrawalMethod = dto.SelectedWithdrawalMethod;
            selectedDate = dto.SelectedDate;
            text1 = textDict["text1"];
            text2 = textDict["text2"];
            readOnly = (dto.Status != (int)TermGroup_TimeWorkAccountYearEmployeeStatus.Calculated || dto.SentDate == null);

            if (dto.TimeWorkAccount.UsePensionDeposit)
                AddTimeWorkAccountOption(new TimeWorkAccountOption(this.Param, TermGroup_TimeWorkAccountWithdrawalMethod.PensionDeposit, dto.CalculatedPensionDepositAmount, dto.TimeWorkAccountYear.PensionDepositPercent, textDict["currency"], textDict["choice1"]));

            if (dto.TimeWorkAccount.UsePaidLeave)
                AddTimeWorkAccountOption(new TimeWorkAccountOption(this.Param, TermGroup_TimeWorkAccountWithdrawalMethod.PaidLeave, dto.CalculatedPaidLeaveAmount, dto.TimeWorkAccountYear.PaidLeavePercent, textDict["currency"], textDict["choice2"], textDict["choice2_extra"]));
           
            if (dto.TimeWorkAccount.UseDirectPayment)
                AddTimeWorkAccountOption(new TimeWorkAccountOption(this.Param, TermGroup_TimeWorkAccountWithdrawalMethod.DirectPayment, dto.CalculatedDirectPaymentAmount, dto.TimeWorkAccountYear.DirectPaymentPercent, textDict["currency"], textDict["choice3"], textDict["choice3_extra"]));
        }

        public void AddTimeWorkAccountOption(TimeWorkAccountOption option)
        {
            if (option == null)
                return;

            timeWorkAccountOption.Add(option);
        }

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {

            var elements = new List<XElement>
            {
                { new XElement("SelectedId", selectedWithdrawalMethod) },
                new XElement("SelectedDate", selectedDate != null ? StringUtility.GetSwedishFormattedDate(selectedDate.Value) : null),
                new XElement("Text1", GetTextOrCDATA(text1)),
                new XElement("Text2", GetTextOrCDATA(text2)),
                new XElement("ReadOnly",StringUtility.GetString(readOnly)),
            };
                        
            return MergeDocuments(ROOTNAME, timeWorkAccountOption.Select(i => i.ToXDocument()).ToList(), elements);
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            return XmlUtil.CreateXml(ROOTNAME, TimeWorkAccountOption.GetDefaultXml());
        }

        #endregion
    }

    internal class TimeWorkAccountOption : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "TimeWorkAccountOption";

        #endregion

        #region Variables

        #region Field values

        private readonly int choiceId;
        private readonly decimal value;
        private readonly decimal percent;
        private readonly string choiceText;
        private readonly string choiceTextExtra;
        private readonly string currency;

        #endregion

        #endregion

        #region Ctor

        public TimeWorkAccountOption(MobileParam param)
            : base(param)
        {
            Init();

        }

        public TimeWorkAccountOption(MobileParam param, TermGroup_TimeWorkAccountWithdrawalMethod choice, decimal value, decimal? percent, string currency, string choiceText, string choiceTextExtra = "")
            : base(param)
        {
            Init();
            choiceId = (int)choice;
            this.value = value;
            this.percent = percent.HasValue ? percent.Value : Decimal.Zero;
            this.choiceText = choiceText;
            this.choiceTextExtra = choiceTextExtra;
            this.currency = currency;
        }

        public TimeWorkAccountOption(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            //Set default values
        }

        #endregion

        #region Public methods

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>
            {
                new XElement("ChoiceId", choiceId),
                new XElement("Value", value),
                new XElement("Currency", GetTextOrCDATA(currency)),
                new XElement("Percent", percent),
                new XElement("ChoiceText", GetTextOrCDATA(choiceText)),
                new XElement("ChoiceTextExtra", GetTextOrCDATA(choiceTextExtra)),
            };

            return CreateDocument(ROOTNAME, elements);
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            string xml = "";

            return XmlUtil.CreateXml(ROOTNAME, xml);
        }

        #endregion
    }

    #endregion

    #region Scheduleplanning views objects

    internal class MobileScheduleViewMonth : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "Schedule";

        #endregion

        #region Variables

        List<MobileScheduleWeek> weeks;

        #endregion

        #region Ctor

        public MobileScheduleViewMonth(MobileParam param)
            : base(param)
        {
            Init();
        }

        /// <summary>Used for errors</summary>
        public MobileScheduleViewMonth(MobileParam param, string errorMessage)
            : base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            this.weeks = new List<MobileScheduleWeek>();
        }

        #endregion

        #region Public methods

        public void AddWeek(MobileViewType viewType, int weekNr, string weekInfo, List<TimeSchedulePlanningMonthSmallDTO> days, EmployeeListDTO employeeAvailability, int employeeId, List<TimeScheduleSwapRequestDTO> employeeSwapRequests, bool isVersionOlderThen22)
        {
            this.weeks.Add(new MobileScheduleWeek(this.Param, viewType, weekNr, weekInfo, days, employeeAvailability, employeeId, employeeSwapRequests, isVersionOlderThen22));
        }

        #endregion

        public override XDocument ToXDocument()
        {
            return MergeDocuments(ROOTNAME, this.weeks.Select(i => i.ToXDocument()).ToList());
        }
    }

    internal class MobileScheduleViewWeek : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "Schedule";

        #endregion

        #region Variables

        List<MobileScheduleEmployee> employees;

        #endregion

        #region Ctor

        public MobileScheduleViewWeek(MobileParam param)
            : base(param)
        {
            Init();
        }

        /// <summary>Used for errors</summary>
        public MobileScheduleViewWeek(MobileParam param, string errorMessage)
            : base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            this.employees = new List<MobileScheduleEmployee>();
        }

        #endregion

        #region Public methods

        public void AddEmployee(MobileViewType viewType, int employeeId, string employeeNr, string firstName, string lastName, bool isHiddenEmployee, string info, int? userId, List<TimeSchedulePlanningAggregatedDayDTO> days, EmployeeListDTO employeeAvailability, List<TimeScheduleSwapRequestDTO> employeeSwapRequests, DateTime dateFrom, DateTime dateTo, bool isVersionOlderThen22, bool includeOnDuty, bool includeTypeOrder)
        {
            this.employees.Add(new MobileScheduleEmployee(this.Param, viewType, employeeId, employeeNr, firstName, lastName, isHiddenEmployee, info, userId, days, employeeAvailability, employeeSwapRequests, dateFrom, dateTo, isVersionOlderThen22, includeOnDuty, includeTypeOrder));
        }

        #endregion

        public override XDocument ToXDocument()
        {
            return MergeDocuments(ROOTNAME, this.employees.Select(i => i.ToXDocument()).ToList());
        }
    }

    internal class MobileTemplateScheduleViewMonth : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "Template";

        #endregion

        #region Variables

        List<MobileScheduleWeek> weeks;

        #endregion

        #region Ctor

        public MobileTemplateScheduleViewMonth(MobileParam param)
            : base(param)
        {
            Init();
        }


        public MobileTemplateScheduleViewMonth(MobileParam param, MobileViewType viewType, List<TimeSchedulePlanningAggregatedDayDTO> days, bool isVersionOlderThen22)
            : base(param)
        {
            Init();

            foreach (var weekGroup in days.GroupBy(x => x.WeekNr).ToList())
            {
                this.AddWeek(viewType, weekGroup.Key, "", weekGroup.ToList(), isVersionOlderThen22);
            }
        }

        /// <summary>Used for errors</summary>
        public MobileTemplateScheduleViewMonth(MobileParam param, string errorMessage)
            : base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            this.weeks = new List<MobileScheduleWeek>();
        }

        #endregion

        #region Public methods

        public void AddWeek(MobileViewType viewType, int weekNr, string weekInfo, List<TimeSchedulePlanningAggregatedDayDTO> days, bool isVersionOlderThen22)
        {
            this.weeks.Add(new MobileScheduleWeek(this.Param, viewType, weekNr, weekInfo, days, isVersionOlderThen22));
        }

        #endregion

        public override XDocument ToXDocument()
        {
            return MergeDocuments(ROOTNAME, this.weeks.Select(i => i.ToXDocument()).ToList());
        }
    }

    internal class MobileScheduleViewDay : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "Schedule";

        #endregion

        #region Variables

        private string dayViewStartTime;
        private string dayViewEndTime;
        private bool isOverlappingMidnight;
        private List<MobileScheduleEmployee> employees;

        #endregion

        #region Ctor

        public MobileScheduleViewDay(MobileParam param)
            : base(param)
        {
            Init();
        }

        /// <summary>Used for errors</summary>
        public MobileScheduleViewDay(MobileParam param, string errorMessage)
            : base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            this.employees = new List<MobileScheduleEmployee>();
        }

        #endregion

        #region Public methods

        public void SetupTimeAxis(string dayViewStartTime, string dayViewEndTime, bool isOverlappingMidnight)
        {
            this.dayViewStartTime = dayViewStartTime;
            this.dayViewEndTime = dayViewEndTime;
            this.isOverlappingMidnight = isOverlappingMidnight;
        }

        public void AddEmployee(MobileViewType viewType, int employeeId, string employeeNr, string firstName, string lastName, bool isHiddenEmployee, string info, int? userId, DateTime date, TimeSchedulePlanningAggregatedDayDTO day, EmployeeListDTO employeeAvailability, bool includeBreaks, bool showTotalBreakInfo, List<TimeScheduleSwapRequestDTO> employeeSwapRequests, bool includeUnscheduledEmployees = false, bool includeOnDuty = false)
        {
            this.employees.Add(new MobileScheduleEmployee(this.Param, viewType, employeeId, employeeNr, firstName, lastName, isHiddenEmployee, info, userId, date, day, employeeAvailability, includeBreaks, showTotalBreakInfo, includeUnscheduledEmployees, employeeSwapRequests, includeOnDuty));
        }

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();

            elements.Add(new XElement("ST", this.dayViewStartTime));
            elements.Add(new XElement("ET", this.dayViewEndTime));
            elements.Add(new XElement("OM", StringUtility.GetString(this.isOverlappingMidnight)));

            return MergeDocuments(ROOTNAME, this.employees.Select(i => i.ToXDocument()).ToList(), elements);
        }

        #endregion

    }

    internal class MobileScheduleWeek : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "Week";

        #endregion

        #region Variables

        readonly private int weekNr;
        readonly private string weekInfo;
        private List<MobileScheduleDay> days;

        #endregion

        #region Ctor

        public MobileScheduleWeek(MobileParam param)
            : base(param)
        {
            Init();
        }

        public MobileScheduleWeek(MobileParam param, MobileViewType viewType, int weeknr, string weekInfo, List<TimeSchedulePlanningAggregatedDayDTO> days, bool isVersionOlderThen22)
            : base(param)
        {
            Init();

            this.weekNr = weeknr;
            this.weekInfo = weekInfo;

            foreach (var day in days)
            {
                this.days.Add(new MobileScheduleDay(param, viewType, day.Date, day.StartTime, day.StopTime, "", day.IsWholeDayAbsence, day.IsPartTimeAbsence, false, false, false, false, null, day.HasShiftRequest, day.HasDescription, day.HasShiftRequestAnswer, false, false, day.HasOnDuty, isVersionOlderThen22));
            }
        }

        public MobileScheduleWeek(MobileParam param, MobileViewType viewType, int weeknr, string weekInfo, List<TimeSchedulePlanningMonthSmallDTO> days, EmployeeListDTO employeeAvailability, int employeeId, List<TimeScheduleSwapRequestDTO> employeeSwapRequests, bool isVersionOlderThen22)
          : base(param)
        {
            Init();

            this.weekNr = weeknr;
            this.weekInfo = weekInfo;
            List<DateTime> initiatorDates = new List<DateTime>();
            List<DateTime> swapWithDates = new List<DateTime>();
            List<DateTime> completedSwapsDates = new List<DateTime>();

            if (employeeSwapRequests != null)
            {
                foreach (TimeScheduleSwapRequestDTO request in employeeSwapRequests)
                {
                    if (request.Status == TermGroup_TimeScheduleSwapRequestStatus.Initiated)
                    {
                        if (request.Rows.Any(w => w.EmployeeId == employeeId))
                        {
                            if (request.IsInitiator(employeeId))
                                initiatorDates.TryAdd(request.Rows.FirstOrDefault(w => w.EmployeeId == employeeId).Date);
                            else if (request.IsSwapWith(employeeId))
                                swapWithDates.TryAdd(request.Rows.FirstOrDefault(w => w.EmployeeId == employeeId).Date);
                        }
                    }
                    else
                    {
                        foreach (var row in request.Rows.Where(w => w.EmployeeId == employeeId))
                        {
                            completedSwapsDates.TryAdd(row.Date);
                        }
                    }
                }
            }
            foreach (var day in days)
            {
                bool prevDay = day.AssignedScheduleIn.Date != new DateTime().Date && day.AssignedScheduleIn.Date < CalendarUtility.DATETIME_DEFAULT.Date;
                bool nextDay = day.AssignedScheduleOut.Date != new DateTime().Date && day.AssignedScheduleOut.Date > CalendarUtility.DATETIME_DEFAULT.Date;
                bool availableShiftsExists = day.Open > 0;
                bool unWantedShiftsExists = day.Unwanted > 0;
                bool absenceRequestExits = day.AbsenceRequested > 0;
                bool hasIniatorShifts = initiatorDates.Contains(day.Date);
                bool hasSwapShifts = swapWithDates.Contains(day.Date);
                bool hasCompletedSwapShift = completedSwapsDates.Contains(day.Date);
                bool hasOnDuty = day.HasOnDuty;
                bool hasTypeOrder = day.HasTypeOrder;
                string typeOrderColor = day.TypeOrderColor;

                var mobileScheduleDay = new MobileScheduleDay(param, viewType, day.Date, day.AssignedScheduleIn, day.AssignedScheduleOut, "", day.IsWholeDayAbsence, day.IsPartTimeAbsence, availableShiftsExists, absenceRequestExits, unWantedShiftsExists, false, employeeAvailability, false, day.HasDescription, false, hasIniatorShifts, hasSwapShifts, hasOnDuty, isVersionOlderThen22, prevDay, nextDay, hasTypeOrder, typeOrderColor, hasCompletedSwapShift: hasCompletedSwapShift);               
                this.days.Add(mobileScheduleDay);
            }
        }

        /// <summary>Used for errors</summary>
        public MobileScheduleWeek(MobileParam param, string errorMessage)
            : base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            this.days = new List<MobileScheduleDay>();
        }

        #endregion

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();

            elements.Add(new XElement("Nr", this.weekNr));
            elements.Add(new XElement("Info", this.weekInfo));
            foreach (var day in days)
            {
                elements.Add(day.ToXElement());
            }

            return CreateDocument(ROOTNAME, elements);
        }
    }

    internal class MobileScheduleEmployee : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "Emp";

        #endregion

        #region Variables

        readonly private int id;
        readonly private string nr;
        readonly private string firstName;
        readonly private string lastName;
        readonly private string info;
        readonly private bool isHiddenEmployee;
        readonly private int? userId;
        private List<MobileScheduleDay> days;

        #endregion

        #region Ctor

        public MobileScheduleEmployee(MobileParam param)
            : base(param)
        {
            Init();
        }

        public MobileScheduleEmployee(MobileParam param, MobileViewType viewType, int employeeId, string employeeNr, string firstName, string lastName, bool isHiddenEmployee, string info, int? userId, List<TimeSchedulePlanningAggregatedDayDTO> days, EmployeeListDTO employeeAvailability, List<TimeScheduleSwapRequestDTO> employeeSwapRequests, DateTime dateFrom, DateTime dateTo, bool isVersionOlderThen22, bool includeOnDuty, bool includeTypeOrder)
            : base(param)
        {
            Init();

            this.id = employeeId;
            this.nr = employeeNr;
            this.firstName = firstName;
            this.lastName = lastName;
            this.isHiddenEmployee = isHiddenEmployee;
            this.info = info;
            this.userId = userId;

            List<DateTime> dates = CalendarUtility.GetDates(dateFrom, dateTo);
            List<DateTime> initiatorDates = new List<DateTime>();
            List<DateTime> swapWithDates = new List<DateTime>();
            List<DateTime> completedSwapsDates = new List<DateTime>();

            if (employeeSwapRequests != null)
            {
                foreach (TimeScheduleSwapRequestDTO request in employeeSwapRequests)
                {
                    if (request.Status == TermGroup_TimeScheduleSwapRequestStatus.Initiated)
                    {
                        if (request.Rows.Any(w => w.EmployeeId == employeeId))
                        {
                            if (request.IsInitiator(employeeId))
                                initiatorDates.TryAdd(request.Rows.FirstOrDefault(w => w.EmployeeId == employeeId).Date);
                            else if (request.IsSwapWith(employeeId))
                                swapWithDates.TryAdd(request.Rows.FirstOrDefault(w => w.EmployeeId == employeeId).Date);
                        }
                    }
                    else
                    {
                        foreach (var row in request.Rows.Where(w => w.EmployeeId == employeeId))
                        {
                            completedSwapsDates.TryAdd(row.Date);
                        }
                    }
                }
            }
            foreach (var date in dates)
            {
                bool hasIniatorShifts = initiatorDates.Contains(date);
                bool hasSwapShifts = swapWithDates.Contains(date);
                bool hasCompletedSwapShift = completedSwapsDates.Contains(date);
                bool prevDay = false;
                bool nextDay = false;
                
                TimeSchedulePlanningAggregatedDayDTO scheduleDay = days.FirstOrDefault(x => x.Date == date);
                if (scheduleDay != null)
                {
                    if (!includeOnDuty)
                    {
                        scheduleDay.DayDTOs = scheduleDay.DayDTOs.Where(x => !x.IsOnDuty()).ToList();
                    }
                    prevDay = scheduleDay.DayDTOs.Any(w => w.StartTime.Date < w.ActualDate);
                    nextDay = scheduleDay.DayDTOs.Any(w => w.StartTime.Date > w.ActualDate);

                    if (this.isHiddenEmployee)
                    {
                        List<MobileScheduleShiftAggregated> aggShifts = new List<MobileScheduleShiftAggregated>();
                        foreach (var linked in scheduleDay.LinkedShifts)
                        {
                            var shifts = linked.ToList();

                            bool wantedShiftsExists = false;
                            if (viewType == MobileViewType.MyScheduleMonth || viewType == MobileViewType.OverviewWeekEmployee)
                                wantedShiftsExists = shifts.IamInQueue();
                            else if (viewType == MobileViewType.OverviewWeekAdmin)
                                wantedShiftsExists = shifts.WantedShiftsExists();

                            string link = linked.Key.HasValue ? linked.Key.ToString() : "";
                            var mobileShift = new MobileScheduleShiftAggregated(param, date, viewType, shifts.GetScheduleIn(), shifts.GetScheduleOut(), false, false, false, wantedShiftsExists, false, false, employeeAvailability, shifts.HasShiftRequest(), shifts.HasDescription(), shifts.HasShiftRequestAnswer(), hasIniatorShifts, hasSwapShifts, shifts.HasOnDuty(), link, isVersionOlderThen22, prevDay, nextDay);
                            mobileShift.HasCompletedSwapShift = hasCompletedSwapShift;
                            aggShifts.Add(mobileShift);
                        }

                        this.days.Add(new MobileScheduleDay(param, viewType, scheduleDay.Date, aggShifts));
                    }
                    else
                    {
                        bool wantedShiftsExists = false;
                        bool hasTypeOrder = false;
                        string typeOrderColor = "";

                        if (includeTypeOrder && (viewType == MobileViewType.MyScheduleMonth || viewType == MobileViewType.OverviewWeekEmployee || viewType == MobileViewType.OverviewWeekAdmin))
                        {
                            var orderDayDTOs = scheduleDay.DayDTOs.Where(w => w.Type == TermGroup_TimeScheduleTemplateBlockType.Order).ToList();
                            hasTypeOrder = orderDayDTOs.Count > 0;
                            typeOrderColor = orderDayDTOs.Count == 1 ? orderDayDTOs.First().ShiftTypeColor : "";
                        }

                        if (viewType == MobileViewType.MyScheduleMonth || viewType == MobileViewType.OverviewWeekEmployee)
                            wantedShiftsExists = scheduleDay.IamInQueue;
                        else if (viewType == MobileViewType.OverviewWeekAdmin)
                            wantedShiftsExists = scheduleDay.WantedShiftsExists;
                        
                        var mobileScheduleDay = new MobileScheduleDay(param, viewType, scheduleDay.Date, scheduleDay.StartTime, scheduleDay.StopTime, "", scheduleDay.IsWholeDayAbsence, scheduleDay.IsPartTimeAbsence, false, scheduleDay.AbsenceRequestedShiftsExists, scheduleDay.UnWantedShiftsExists, wantedShiftsExists, employeeAvailability, scheduleDay.HasShiftRequest, scheduleDay.HasDescription, scheduleDay.HasShiftRequestAnswer, hasIniatorShifts, hasSwapShifts, scheduleDay.HasOnDuty, isVersionOlderThen22, prevDay, nextDay, hasTypeOrder, typeOrderColor, hasCompletedSwapShift: hasCompletedSwapShift);
                        this.days.Add(mobileScheduleDay);
                    }
                }
                else if ((employeeAvailability?.IsAvailableInRange(date, date) ?? false) || (employeeAvailability?.IsUnavailableInRange(date, date) ?? false))
                {
                    var mobileScheduleDay = new MobileScheduleDay(param, viewType, date, null, null, "", false, false, false, false, false, false, employeeAvailability, false, false, false, hasIniatorShifts, hasSwapShifts, false, isVersionOlderThen22, hasCompletedSwapShift: hasCompletedSwapShift);
                    this.days.Add(mobileScheduleDay);
                }
            }
        }

        public MobileScheduleEmployee(MobileParam param, MobileViewType viewType, int employeeId, string employeeNr, string firstName, string lastName, bool isHiddenEmployee, string info, int? userId, DateTime date, TimeSchedulePlanningAggregatedDayDTO aggDay, EmployeeListDTO employeeAvailability, bool includeBreaks, bool showTotalBreakInfo, bool includeUnscheduledEmployees, List<TimeScheduleSwapRequestDTO> employeeSwapRequests, bool includeOnDuty)
            : base(param)
        {
            Init();

            this.id = employeeId;
            this.nr = employeeNr;
            this.firstName = firstName;
            this.lastName = lastName;
            this.isHiddenEmployee = isHiddenEmployee;
            this.info = info;
            this.userId = userId;

            bool interestRequestExists = employeeAvailability?.IsAvailableInRange(date, date) ?? false;
            bool nonInterestRequestExists = employeeAvailability?.IsUnavailableInRange(date, date) ?? false;
            List<DateTime> initiatorDates = new List<DateTime>();
            List<DateTime> swapWithDates = new List<DateTime>();
            List<DateTime> completedSwapsDates = new List<DateTime>();

            if (employeeSwapRequests != null)
            {
                foreach (TimeScheduleSwapRequestDTO request in employeeSwapRequests)
                {
                    if (request.Status == TermGroup_TimeScheduleSwapRequestStatus.Initiated)
                    {
                        if (request.IsInitiator(employeeId))
                            initiatorDates.TryAdd(request.Rows.FirstOrDefault(w => w.EmployeeId == employeeId).Date);
                        else if (request.IsSwapWith(employeeId))
                            swapWithDates.TryAdd(request.Rows.FirstOrDefault(w => w.EmployeeId == employeeId).Date);
                    }
                    else
                    {
                        foreach (var row in request.Rows.Where(w => w.EmployeeId == employeeId))
                        {
                            completedSwapsDates.TryAdd(row.Date);
                        }
                    }

                }
               
            }
            bool hasIniatorShifts = initiatorDates.Contains(date);
            bool hasSwapShifts = swapWithDates.Contains(date);
            bool hasCompletedSwapShift = completedSwapsDates.Contains(date);

            if (viewType == MobileViewType.DayViewAdmin || viewType == MobileViewType.DayViewEmployee)
            {
                if (interestRequestExists || nonInterestRequestExists)
                {
                    var mobileScheduleDay = new MobileScheduleDay(param, viewType, date, interestRequestExists, nonInterestRequestExists, true, "", hasIniatorShifts, hasSwapShifts, hasCompletedSwapShift: hasCompletedSwapShift);
                    mobileScheduleDay.AddDayViewContainer(date, employeeAvailability);
                    this.days.Add(mobileScheduleDay);
                }

                if (aggDay != null)
                {
                    if(!includeOnDuty)
                    {
                        aggDay.DayDTOs = aggDay.DayDTOs.Where(x => !x.IsOnDuty()).ToList();
                    }

                    if (aggDay.StartTime != aggDay.StopTime)
                    {
                        if (this.isHiddenEmployee)
                        {
                            foreach (var linked in aggDay.LinkedShifts)
                            {
                                string link = linked.Key.HasValue ? linked.Key.ToString() : "";
                                var mobileScheduleDay = new MobileScheduleDay(param, viewType, aggDay.Date, false, false, false, link, hasIniatorShifts, hasSwapShifts, hasCompletedSwapShift: hasCompletedSwapShift);
                                var linkedList = linked.ToList();

                                var linkedShifts = linkedList.Where(x => !x.IsOnDuty()).ToList();
                                mobileScheduleDay.AddDayViewContainer(linkedShifts, includeBreaks, showTotalBreakInfo);
                                this.days.Add(mobileScheduleDay);

                                var linkedShiftsOnDuty = linkedList.Where(x => x.IsOnDuty()).ToList();
                                foreach (var onDuty in linkedShiftsOnDuty)
                                {
                                    var mobileOnDutyScheduleDay = new MobileScheduleDay(param, viewType, aggDay.Date, false, false, false, link, hasIniatorShifts, hasSwapShifts);
                                    mobileOnDutyScheduleDay.AddDayViewContainer((new List<TimeSchedulePlanningDayDTO> { onDuty }), includeBreaks, showTotalBreakInfo);
                                    this.days.Add(mobileOnDutyScheduleDay);
                                }
                            }
                        }
                        else
                        {
                            var mobileScheduleDay = new MobileScheduleDay(param, viewType, aggDay.Date, interestRequestExists, nonInterestRequestExists, false, "", hasIniatorShifts, hasSwapShifts, hasCompletedSwapShift: hasCompletedSwapShift);
                            mobileScheduleDay.AddDayViewContainer(aggDay.DayDTOs.Where(x => !x.IsOnDuty()).ToList(), includeBreaks, showTotalBreakInfo);                        
                            this.days.Add(mobileScheduleDay);

                            var onDutyDayDTOs = aggDay.DayDTOs.Where(x => x.IsOnDuty()).ToList();
                            foreach (var onDuty in onDutyDayDTOs)
                            {
                                var mobileOnDutyScheduleDay = new MobileScheduleDay(param, viewType, aggDay.Date, interestRequestExists, nonInterestRequestExists, false, "", hasIniatorShifts, hasSwapShifts, hasCompletedSwapShift: hasCompletedSwapShift);
                                mobileOnDutyScheduleDay.AddDayViewContainer(new List<TimeSchedulePlanningDayDTO> { onDuty }, includeBreaks, showTotalBreakInfo);
                                this.days.Add(mobileOnDutyScheduleDay);
                            }
                        }
                    }
                }
                else if (includeUnscheduledEmployees)
                {
                    var mobileScheduleDay = new MobileScheduleDay(param, viewType, date, false, false, false, "", hasIniatorShifts, hasSwapShifts, hasCompletedSwapShift: hasCompletedSwapShift);
                    mobileScheduleDay.AddDayViewContainer(date, null);
                    this.days.Add(mobileScheduleDay);
                }
            }
        }

        /// <summary>Used for errors</summary>
        public MobileScheduleEmployee(MobileParam param, string errorMessage)
            : base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            this.days = new List<MobileScheduleDay>();
        }

        #endregion

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();
            elements.Add(new XElement("EID", this.id));
            elements.Add(new XElement("ENR", this.nr));
            elements.Add(new XElement("FN", this.firstName));
            elements.Add(new XElement("LN", this.lastName));

            if (this.isHiddenEmployee)
                elements.Add(new XElement("AE", StringUtility.GetString(this.isHiddenEmployee)));

            if (userId.HasValue && !this.isHiddenEmployee)
                elements.Add(new XElement("UI", this.userId.Value));

            elements.Add(new XElement("Info", this.info));
            foreach (var day in days)
            {
                elements.Add(day.ToXElement());
            }

            return CreateDocument(ROOTNAME, elements);
        }
    }

    internal class MobileScheduleDay : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "Day";

        #endregion

        #region Variables

        readonly private MobileViewType viewType;
        readonly private DateTime date;
        readonly private string link;
        readonly private bool interestRequestExists;
        readonly private bool nonInterestRequestExists;
        readonly private bool isAvailabilityContainer;
        readonly private bool hasShiftSwapInititor;
        readonly private bool hasShiftSwapReciver;
        private bool hasCompletedSwapShift;
        private List<MobileScheduleShiftAggregated> aggShifts;
        private List<MobileScheduleDayContainer> dayViewContainers;

        #endregion

        #region Ctor

        public MobileScheduleDay(MobileParam param)
            : base(param)
        {
            Init();
        }
        /// <summary>
        /// Used only for Dayview
        /// </summary>
        /// <param name="param"></param>
        /// <param name="viewType"></param>
        /// <param name="date"></param>
        /// <param name="employeeAvailability"></param>
        /// <param name="link"></param>
        /// <param name="hasIniatorShifts"></param>      
        /// <param name="hasSwapShifts"></param>
        public MobileScheduleDay(MobileParam param, MobileViewType viewType, DateTime date, bool interestRequestExists, bool nonInterestRequestExists, bool isAvailabilityContainer, string link, bool hasIniatorShifts, bool hasSwapShifts, bool hasCompletedSwapShift = false)
            : base(param)
        {
            Init();

            this.viewType = viewType;
            this.date = date;
            this.interestRequestExists = interestRequestExists;
            this.nonInterestRequestExists = nonInterestRequestExists;
            this.isAvailabilityContainer = isAvailabilityContainer;
            this.link = link;
            this.hasShiftSwapInititor = hasIniatorShifts;
            this.hasShiftSwapReciver = hasSwapShifts;
            this.hasCompletedSwapShift = hasCompletedSwapShift;
        }
        /// <summary>
        /// Used for "Mitt schema - månad" and "Översikt - vecka
        /// </summary>
        /// <param name="param"></param>
        /// <param name="viewType"></param>
        /// <param name="date"></param>
        /// <param name="startTime"></param>
        /// <param name="stopTime"></param>
        /// <param name="link"></param>
        /// <param name="wholeDayAbsence"></param>
        /// <param name="partTimeAbsence"></param>
        /// <param name="availableShiftsExits"></param>
        /// <param name="absenceRequestExits"></param>
        /// <param name="unWantedShiftsExists"></param>
        /// <param name="wantedShiftsExists"></param>
        /// <param name="employeeAvailability"></param>
        /// <param name="hasShiftRequest"></param>
        /// <param name="hasDescription"></param>
        /// <param name="hasShiftRequestAnswer"></param>        
        /// <param name="hasIniatorShifts"></param>      
        /// <param name="hasSwapShifts"></param>
        /// <param name="hasOnDuty"></param>
        public MobileScheduleDay(MobileParam param, MobileViewType viewType, DateTime date, DateTime? startTime, DateTime? stopTime, string link, bool wholeDayAbsence, bool partTimeAbsence, bool availableShiftsExits, bool absenceRequestExits, bool unWantedShiftsExists, bool wantedShiftsExists, EmployeeListDTO employeeAvailability, bool hasShiftRequest, bool hasDescription, bool hasShiftRequestAnswer, bool hasIniatorShifts, bool hasSwapShifts, bool hasOnDuty, bool isVersionOlderThen22, bool prevDay = false, bool nextDay = false, bool hasTypeOrder = false, string typeOrderColor = "", bool hasCompletedSwapShift = false)
            : base(param)
        {
            Init();

            this.viewType = viewType;
            this.date = date;
            this.link = string.Empty;
            this.hasCompletedSwapShift = hasCompletedSwapShift;

            if (wholeDayAbsence) //move down this check to MobileScheduleShiftAggregated
            {
                if (startTime.HasValue && startTime.Value.TimeOfDay == new TimeSpan(0, 0, 0) && stopTime.HasValue && stopTime.Value.TimeOfDay == new TimeSpan(23, 59, 59))
                {
                    var aggShift = new MobileScheduleShiftAggregated(param, date, viewType, null, null, availableShiftsExits, absenceRequestExits, unWantedShiftsExists, wantedShiftsExists, wholeDayAbsence, partTimeAbsence, employeeAvailability, hasShiftRequest, hasDescription, hasShiftRequestAnswer, hasIniatorShifts, hasSwapShifts, hasOnDuty, link, isVersionOlderThen22, prevDay, nextDay, hasTypeOrder, typeOrderColor);
                    aggShift.HasCompletedSwapShift = hasCompletedSwapShift;
                    this.aggShifts.Add(aggShift);
                }
                else
                {
                    var aggShift = new MobileScheduleShiftAggregated(param, date, viewType, startTime, stopTime, availableShiftsExits, absenceRequestExits, unWantedShiftsExists, wantedShiftsExists, wholeDayAbsence, partTimeAbsence, employeeAvailability, hasShiftRequest, hasDescription, hasShiftRequestAnswer, hasIniatorShifts, hasSwapShifts, hasOnDuty, link, isVersionOlderThen22, prevDay, nextDay, hasTypeOrder, typeOrderColor);
                    aggShift.HasCompletedSwapShift = hasCompletedSwapShift;
                    this.aggShifts.Add(aggShift);
                }
            }
            else
            {
                var aggShift = new MobileScheduleShiftAggregated(param, date, viewType, startTime, stopTime, availableShiftsExits, absenceRequestExits, unWantedShiftsExists, wantedShiftsExists, wholeDayAbsence, partTimeAbsence, employeeAvailability, hasShiftRequest, hasDescription, hasShiftRequestAnswer, hasIniatorShifts, hasSwapShifts, hasOnDuty, link, isVersionOlderThen22, prevDay, nextDay, hasTypeOrder, typeOrderColor);
                aggShift.HasCompletedSwapShift = hasCompletedSwapShift;
                this.aggShifts.Add(aggShift);
            }
            
        }

        public MobileScheduleDay(MobileParam param, MobileViewType viewType, DateTime date, List<MobileScheduleShiftAggregated> shifts)
            : base(param)
        {
            Init();

            this.viewType = viewType;
            this.date = date;
            this.link = string.Empty;
            this.aggShifts.AddRange(shifts);
        }

        /// <summary>Used for errors</summary>
        public MobileScheduleDay(MobileParam param, string errorMessage)
            : base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            this.aggShifts = new List<MobileScheduleShiftAggregated>();
            this.dayViewContainers = new List<MobileScheduleDayContainer>();
        }

        #endregion

        #region Public

        public void AddDayViewContainer(List<TimeSchedulePlanningDayDTO> shifts, bool includeBreaks, bool showTotalBreakInfo)
        {

            List<List<TimeSchedulePlanningDayDTO>> coherentShiftGroups = shifts.GetCoherentShifts();
            foreach (var coherentShifts in coherentShiftGroups)
            {
                this.dayViewContainers.Add(new MobileScheduleDayContainer(this.Param, this.viewType, coherentShifts, includeBreaks, showTotalBreakInfo));
            }
        }

        public void AddDayViewContainer(DateTime date, EmployeeListDTO employeeAvailability)
        {
            this.dayViewContainers.Add(new MobileScheduleDayContainer(this.Param, this.viewType, this.date, employeeAvailability));
        }

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();

            return CreateDocument(ROOTNAME, elements);
        }

        public XElement ToXElement()
        {
            XElement element = new XElement(ROOTNAME);

            if (viewType == MobileViewType.DayViewAdmin || viewType == MobileViewType.DayViewEmployee)
            {
                element.Add(new XElement("IR", StringUtility.GetString(this.interestRequestExists)));
                element.Add(new XElement("NIR", StringUtility.GetString(this.nonInterestRequestExists)));
                element.Add(new XElement("IEA", StringUtility.GetString(this.isAvailabilityContainer)));

                if (hasShiftSwapInititor)
                    element.Add(new XElement("HSSI", StringUtility.GetString(this.hasShiftSwapInititor)));
                if (hasShiftSwapReciver)
                    element.Add(new XElement("HSSR", StringUtility.GetString(this.hasShiftSwapReciver)));
                if (hasCompletedSwapShift)
                    element.Add(new XElement("HSSC", StringUtility.GetString(this.hasCompletedSwapShift)));

                if (!string.IsNullOrEmpty(this.link))
                    element.Add(new XElement("Link", this.link));

                foreach (var container in dayViewContainers)
                {
                    element.Add(container.ToXElement());
                }
            }
            else
            {
                element.Add(new XElement("Date", this.date.ToShortDateString()));

                foreach (var aggShift in aggShifts)
                {
                    element.Add(aggShift.ToXElement());
                }
            }

            return element;
        }

        #endregion
    }

    internal class MobileScheduleDayContainer : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "Container";

        #endregion

        #region Variables

        readonly private MobileViewType viewType;
        readonly private DateTime scheduleIn;
        readonly private DateTime scheduleOut;
        readonly private string txt;
        readonly private bool isOverlappingMidnight;
        readonly private bool startsAfterMidnight;
        private List<MobileDayViewShift> dayViewShifts; // used for Day view

        #endregion

        #region Ctor

        public MobileScheduleDayContainer(MobileParam param)
            : base(param)
        {
            Init();
        }

        public MobileScheduleDayContainer(MobileParam param, MobileViewType viewType, List<TimeSchedulePlanningDayDTO> coherentShifts, bool includeBreaks, bool showTotalBreakInfo)
            : base(param)
        {
            Init();

            this.viewType = viewType;
            this.scheduleIn = coherentShifts.GetScheduleIn();
            this.scheduleOut = coherentShifts.GetScheduleOut();
            this.isOverlappingMidnight = this.scheduleOut.Date > this.scheduleIn.Date;

            var firstShift = coherentShifts.GetFirst();
            if (firstShift != null)
            {
                DateTime actualDate = firstShift.ActualDate;
                this.startsAfterMidnight = firstShift.StartTime.Date > actualDate;
            }


            int breakMinutes = 0;

            if (this.viewType == MobileViewType.DayViewAdmin || this.viewType == MobileViewType.DayViewEmployee)
            {
                foreach (var shift in coherentShifts)
                {
                    bool isWanted = false;
                    if (this.viewType == MobileViewType.DayViewEmployee)
                    {
                        isWanted = shift.IamInQueue;
                    }
                    else if (this.viewType == MobileViewType.DayViewAdmin)
                    {
                        isWanted = shift.NbrOfWantedInQueue > 0;
                    }

                    breakMinutes += shift.GetBreakTimeWithinShift();
                    this.dayViewShifts.Add(new MobileDayViewShift(this.Param, shift, isWanted));

                    if (includeBreaks && !showTotalBreakInfo)
                    {
                        List<BreakDTO> shiftBreaks = shift.GetOverlappedBreaks(shift.GetBreaks(), true);
                        foreach (var breakItem in shiftBreaks)
                        {
                            this.dayViewShifts.Add(new MobileDayViewShift(this.Param, breakItem.Id, breakItem.StartTime, breakItem.StartTime.AddMinutes(breakItem.BreakMinutes), shift.ActualDate, isBreak: true, isAbsence: false, hasShiftRequest: false, hasShiftRequestAnswer: false, hasDescription: false));
                        }
                    }
                }
            }

            if (showTotalBreakInfo && breakMinutes > 0)
                txt += breakMinutes + " min rast";
        }

        public MobileScheduleDayContainer(MobileParam param, MobileViewType viewType, DateTime date, EmployeeListDTO employeeAvailability)
            : base(param)
        {
            Init();

            List<EmployeeListAvailabilityDTO> availableRanges = employeeAvailability?.GetAvailableInRange(date, date) ?? new List<EmployeeListAvailabilityDTO>();
            List<EmployeeListAvailabilityDTO> unavailableRanges = employeeAvailability?.GetUnavailableInRange(date, date) ?? new List<EmployeeListAvailabilityDTO>();

            if (availableRanges.Any() || unavailableRanges.Any())
            {

                this.viewType = viewType;

                DateTime? availableStart = availableRanges.Any() ? availableRanges.OrderBy(x => x.Start).First().Start : (DateTime?)null;
                DateTime? availableStop = availableRanges.Any() ? availableRanges.OrderByDescending(x => x.Stop).First().Stop : (DateTime?)null;

                DateTime? unavailableStart = unavailableRanges.Any() ? unavailableRanges.OrderBy(x => x.Start).First().Start : (DateTime?)null;
                DateTime? unavailableStop = unavailableRanges.Any() ? unavailableRanges.OrderByDescending(x => x.Stop).First().Stop : (DateTime?)null;

                this.scheduleIn = CalendarUtility.GetEarliestDate(availableStart, unavailableStart).Value;
                this.scheduleOut = CalendarUtility.GetLatestDate(availableStop, unavailableStop).Value;
                this.isOverlappingMidnight = this.scheduleOut.Date > this.scheduleIn.Date;
                this.startsAfterMidnight = false; // not possible for availability

                foreach (var range in availableRanges)
                {
                    this.dayViewShifts.Add(new MobileDayViewShift(this.Param, date, true, false, range.Start, range.Stop, range.Comment));
                }

                foreach (var range in unavailableRanges)
                {
                    this.dayViewShifts.Add(new MobileDayViewShift(this.Param, date, false, true, range.Start, range.Stop, range.Comment));
                }
            }
        }

        /// <summary>Used for errors</summary>
        public MobileScheduleDayContainer(MobileParam param, string errorMessage)
            : base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            dayViewShifts = new List<MobileDayViewShift>();
        }

        #endregion

        #region Public

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();

            return CreateDocument(ROOTNAME, elements);
        }

        public XElement ToXElement()
        {
            XElement element = new XElement(ROOTNAME);
            element.Add(new XElement("CSI", this.scheduleIn.ToShortTimeString()));
            element.Add(new XElement("CSO", this.scheduleOut.ToShortTimeString()));
            element.Add(new XElement("CSOM", StringUtility.GetString(this.isOverlappingMidnight)));
            element.Add(new XElement("CSAM", StringUtility.GetString(this.startsAfterMidnight)));
            element.Add(new XElement("TXT", this.txt));

            this.dayViewShifts = this.dayViewShifts.OrderBy(x => x.startTime).ToList();
            foreach (var shift in dayViewShifts)
            {
                element.Add(shift.ToXElement());
            }

            return element;
        }

        #endregion
    }
    /// <summary>
    /// Represents ONE cell
    /// </summary>
    internal class MobileScheduleShiftAggregated : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "Shift";

        #endregion

        #region Variables
        readonly private MobileViewType viewType;
        readonly private DateTime? aggStartTime;
        readonly private DateTime? aggStopTime;
        readonly private bool availableShiftsExits;
        readonly private bool absenceRequestExits;
        readonly private bool unWantedShiftsExists;
        readonly private bool wantedShiftsExists;
        readonly private bool wholeDayAbsence;
        readonly private bool partTimeAbsence;
        readonly private bool interestRequestExists;//isFullyAvailable
        readonly private bool nonInterestRequestExists;//isFullyUnAvailable
        readonly private bool isPartlyAvailable;
        readonly private bool isPartlyUnavailable;
        readonly private bool isMixedAvailable;
        readonly private bool hasShiftRequest;
        readonly private bool hasShiftRequestAnswer;
        readonly private bool hasDescription;
        readonly private bool hasAvailabilityComment;
        readonly private bool hasIniatorShifts;
        readonly private bool hasSwapShifts;
        readonly private bool hasOnDuty;
        readonly private bool hasTypeOrder;
        readonly private string typeOrderColor;
        readonly private string link;
        readonly private bool prevDay;
        readonly private bool nextDay;
        private bool hasCompletedSwapShift; 

        public bool HasCompletedSwapShift
        {
            get { return hasCompletedSwapShift; }
            set { hasCompletedSwapShift = value; }
        }

        #endregion

        #region Ctor

        public MobileScheduleShiftAggregated(MobileParam param)
            : base(param)
        {
            Init();
        }


        public MobileScheduleShiftAggregated(MobileParam param, DateTime date, MobileViewType viewType, DateTime? aggStartTime, DateTime? aggStopTime, bool availableShiftsExits, bool absenceRequestExits, bool unWantedShiftsExists, bool wantedShiftsExists, bool wholeDayAbsence, bool partTimeAbsence, EmployeeListDTO employeeAvailability, bool hasShiftRequest, bool hasDescription, bool hasShiftRequestAnswer, bool hasIniatorShifts, bool hasSwapShifts, bool hasOnDuty, string link, bool isVersionOlderThen22, bool prevDay = false, bool nextDay = false, bool hasTypeOrder = false, string typeOrderColor = "")
            : base(param)
        {
            Init();

            this.viewType = viewType;
            this.aggStartTime = aggStartTime;
            this.aggStopTime = aggStopTime;
            this.availableShiftsExits = availableShiftsExits;
            this.absenceRequestExits = absenceRequestExits;
            this.unWantedShiftsExists = unWantedShiftsExists;
            this.wantedShiftsExists = wantedShiftsExists;
            this.wholeDayAbsence = wholeDayAbsence;
            this.partTimeAbsence = partTimeAbsence;
            this.hasIniatorShifts = hasIniatorShifts;
            this.hasSwapShifts = hasSwapShifts;
            this.hasOnDuty = hasOnDuty;
            this.prevDay = prevDay;
            this.nextDay = nextDay;
            this.hasTypeOrder = hasTypeOrder;
            this.typeOrderColor = typeOrderColor;

            if (isVersionOlderThen22)
            {
                this.interestRequestExists = employeeAvailability?.IsAvailableInRange(date, date) ?? false;
                this.nonInterestRequestExists = employeeAvailability?.IsUnavailableInRange(date, date) ?? false;
            }
            else
            {
                this.interestRequestExists = employeeAvailability?.IsFullyAvailableInRange(date, CalendarUtility.GetEndOfDay(date).RemoveSeconds()) ?? false;
                this.nonInterestRequestExists = employeeAvailability?.IsFullyUnavailableInRange(date, CalendarUtility.GetEndOfDay(date).RemoveSeconds()) ?? false;
            }
            this.isPartlyAvailable = employeeAvailability?.IsPartlyAvailableInRange(date, CalendarUtility.GetEndOfDay(date).RemoveSeconds()) ?? false;
            this.isPartlyUnavailable = employeeAvailability?.IsPartlyUnavailableInRange(date, CalendarUtility.GetEndOfDay(date).RemoveSeconds()) ?? false;
            this.isMixedAvailable = employeeAvailability?.IsMixedAvailableInRange(date, CalendarUtility.GetEndOfDay(date).RemoveSeconds()) ?? false;
            this.hasShiftRequest = hasShiftRequest;
            this.hasShiftRequestAnswer = hasShiftRequestAnswer;
            this.hasDescription = hasDescription;
            this.link = link;

            if (interestRequestExists || nonInterestRequestExists || isPartlyAvailable || isPartlyUnavailable || isMixedAvailable)
            {
                hasAvailabilityComment = employeeAvailability?.HasAvailabilityCommentInRange(date, date) ?? false;
            }
        }

        /// <summary>Used for errors</summary>
        public MobileScheduleShiftAggregated(MobileParam param, string errorMessage)
            : base(param, errorMessage)
        {
            Init();
        }

        #endregion

        private void Init()
        {
            //Set default values
        }

        public XElement ToXElement()
        {
            XElement element = new XElement(ROOTNAME);
            element.Add(new XElement("SI", this.aggStartTime.HasValue ? this.aggStartTime.Value.TimeOfDay.ToShortTimeString() : ""));
            element.Add(new XElement("SO", this.aggStopTime.HasValue ? this.aggStopTime.Value.TimeOfDay.ToShortTimeString() : ""));
            element.Add(new XElement("IPTA", StringUtility.GetString(this.partTimeAbsence)));
            element.Add(new XElement("IWDA", StringUtility.GetString(this.wholeDayAbsence)));
            element.Add(new XElement("UW", StringUtility.GetString(this.unWantedShiftsExists)));
            element.Add(new XElement("AR", StringUtility.GetString(this.absenceRequestExits)));

            element.Add(new XElement("IR", StringUtility.GetString(this.interestRequestExists))); //isAvailable
            element.Add(new XElement("NIR", StringUtility.GetString(this.nonInterestRequestExists))); //isUnavailable
            element.Add(new XElement("IPA", StringUtility.GetString(this.isPartlyAvailable)));
            element.Add(new XElement("IPUA", StringUtility.GetString(this.isPartlyUnavailable)));
            element.Add(new XElement("IMA", StringUtility.GetString(this.isMixedAvailable)));
            element.Add(new XElement("HAC", StringUtility.GetString(this.hasAvailabilityComment)));

            element.Add(new XElement("HSR", StringUtility.GetString(this.hasShiftRequest)));
            element.Add(new XElement("HSRA", StringUtility.GetString(this.hasShiftRequestAnswer)));
            element.Add(new XElement("HD", StringUtility.GetString(this.hasDescription)));
            element.Add(new XElement("HOD", StringUtility.GetString(this.hasOnDuty)));
            element.Add(new XElement("PD", StringUtility.GetString(prevDay)));
            element.Add(new XElement("ND", StringUtility.GetString(nextDay)));
            element.Add(new XElement("HTO", StringUtility.GetString(hasTypeOrder)));
            element.Add(new XElement("TOC", typeOrderColor ?? ""));           

            if (hasIniatorShifts)
                element.Add(new XElement("HSSI", StringUtility.GetString(this.hasIniatorShifts)));
            if (hasSwapShifts)
                element.Add(new XElement("HSSR", StringUtility.GetString(this.hasSwapShifts)));
            if (hasCompletedSwapShift)
                element.Add(new XElement("HSSC", StringUtility.GetString(this.hasCompletedSwapShift)));

            if (this.viewType == MobileViewType.MyScheduleMonth)
            {
                element.Add(new XElement("AS", StringUtility.GetString(this.availableShiftsExits)));
            }
            else if (this.viewType == MobileViewType.OverviewWeekEmployee || this.viewType == MobileViewType.OverviewWeekAdmin)
            {
                element.Add(new XElement("WA", StringUtility.GetString(this.wantedShiftsExists)));
            }

            if (!string.IsNullOrEmpty(this.link))
                element.Add(new XElement("Link", this.link));

            return element;
        }

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();

            return CreateDocument(ROOTNAME, elements);
        }
    }

    internal class MobileDayViewShift : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "Shift";

        #endregion

        #region Variables

        #region Field values

        readonly private int id;
        public DateTime startTime;
        readonly private DateTime stopTime;
        readonly private string shiftTypeColor;
        readonly private bool isBreak;
        readonly private bool isAbsence;
        readonly private bool isOverlappingMidnight;
        readonly private bool startsAfterMidnight;
        readonly private bool hasShiftRequest;
        readonly private bool hasShiftRequestAnswer;
        readonly private bool hasDescription;
        readonly private bool hasAvailabilityComment;
        readonly private bool isAvailable;
        readonly private bool isUnavailable;
        readonly private bool isWanted;
        readonly private bool isUnwanted;
        readonly private bool isOnDuty;

        #endregion

        #endregion

        #region Ctor

        public MobileDayViewShift(MobileParam param)
            : base(param)
        {
            Init();
        }

        public MobileDayViewShift(MobileParam param, TimeSchedulePlanningDayDTO shiftDTO, bool isWanted)
            : base(param)
        {
            Init();

            this.id = shiftDTO.TimeScheduleTemplateBlockId;
            this.startTime = shiftDTO.StartTime;
            this.stopTime = shiftDTO.StopTime;
            this.isOverlappingMidnight = shiftDTO.StopTime.Date > shiftDTO.StartTime.Date;
            this.startsAfterMidnight = shiftDTO.StartTime.Date > shiftDTO.ActualDate;
            this.isBreak = false;
            this.isAbsence = shiftDTO.TimeDeviationCauseId.HasValue;
            this.shiftTypeColor = shiftDTO.ShiftTypeColor;
            this.hasShiftRequest = shiftDTO.HasShiftRequest;
            this.hasShiftRequestAnswer = shiftDTO.HasShiftRequestAnswer;
            this.hasDescription = !string.IsNullOrEmpty(shiftDTO.Description);
            this.isWanted = isWanted;
            this.isUnwanted = shiftDTO.ShiftUserStatus == TermGroup_TimeScheduleTemplateBlockShiftUserStatus.Unwanted;
            this.isOnDuty = shiftDTO.IsOnDuty();
        }

        public MobileDayViewShift(MobileParam param, int id, DateTime startTime, DateTime stopTime, DateTime actualDate, bool isBreak, bool isAbsence, bool hasShiftRequest, bool hasShiftRequestAnswer, bool hasDescription)
            : base(param)
        {
            Init();

            this.id = id;
            this.startTime = startTime;
            this.stopTime = stopTime;
            this.isBreak = isBreak;
            this.isAbsence = isAbsence;
            this.isOverlappingMidnight = stopTime.Date > startTime.Date;
            this.startsAfterMidnight = startTime.Date > actualDate;
            this.hasShiftRequest = hasShiftRequest;
            this.hasShiftRequestAnswer = hasShiftRequestAnswer;
            this.hasDescription = hasDescription;
        }

        public MobileDayViewShift(MobileParam param, DateTime date, bool isAvailable, bool isUnavailable, DateTime start, DateTime stop, string comment)
            : base(param)
        {
            Init();

            this.id = 0;
            this.startTime = start;
            this.stopTime = stop;
            this.isBreak = false;
            this.isAbsence = false;
            this.isOverlappingMidnight = stopTime.Date > startTime.Date;
            this.startsAfterMidnight = startTime.Date > date;
            this.hasShiftRequest = false;
            this.hasShiftRequestAnswer = false;
            this.hasDescription = false;
            this.hasAvailabilityComment = !string.IsNullOrEmpty(comment);
            this.isAvailable = isAvailable;
            this.isUnavailable = isUnavailable;
        }
        /// <summary>
        /// Used for errors
        /// </summary>
        public MobileDayViewShift(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            //Set default values
        }

        #endregion

        #region Public methods

        #endregion

        #region Overrided methods

        public XElement ToXElement()
        {
            XElement element = new XElement(ROOTNAME);
            element.Add(new XElement("Id", this.id));
            element.Add(new XElement("SI", this.startTime.TimeOfDay.ToShortTimeString()));
            element.Add(new XElement("SO", this.stopTime.TimeOfDay.ToShortTimeString()));
            element.Add(new XElement("IA", StringUtility.GetString(this.isAbsence)));
            element.Add(new XElement("IB", StringUtility.GetString(this.isBreak)));
            element.Add(new XElement("SOM", StringUtility.GetString(this.isOverlappingMidnight)));
            element.Add(new XElement("SAM", StringUtility.GetString(this.startsAfterMidnight)));
            element.Add(new XElement("HSR", StringUtility.GetString(this.hasShiftRequest)));
            element.Add(new XElement("HSRA", StringUtility.GetString(this.hasShiftRequestAnswer)));
            element.Add(new XElement("HD", StringUtility.GetString(this.hasDescription)));
            element.Add(new XElement("HAC", StringUtility.GetString(this.hasAvailabilityComment)));
            element.Add(new XElement("EAIA", StringUtility.GetString(this.isAvailable)));
            element.Add(new XElement("EAIUA", StringUtility.GetString(this.isUnavailable)));
            element.Add(new XElement("IW", StringUtility.GetString(this.isWanted)));
            element.Add(new XElement("IUW", StringUtility.GetString(this.isUnwanted)));
            element.Add(new XElement("IOD", StringUtility.GetString(this.isOnDuty)));

            string shiftColor = string.IsNullOrEmpty(this.shiftTypeColor) ? "#707070" : this.shiftTypeColor;
            element.Add(new XElement("STC", shiftColor));
            element.Add(new XElement("FC", GraphicsUtil.ForegroundColorByBackgroundBrightness(shiftColor)));

            return element;
        }

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();

            return CreateDocument(ROOTNAME, elements);
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            string xml = "";
            return XmlUtil.CreateXml(ROOTNAME, xml);
        }

        #endregion
    }

    internal class MobileEmployeeAvailability : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "EAva";

        #endregion

        #region Variables

        #region Field values

        readonly private bool isAvailable;
        readonly private bool isUnavailable;
        readonly private DateTime start;
        readonly private DateTime stop;

        #endregion

        #endregion

        #region Ctor

        public MobileEmployeeAvailability(MobileParam param)
            : base(param)
        {
            Init();
        }

        public MobileEmployeeAvailability(MobileParam param, bool isAvailable, bool isUnavailable, DateTime start, DateTime stop)
            : base(param)
        {
            Init();

            this.isAvailable = isAvailable;
            this.isUnavailable = isUnavailable;
            this.start = start;
            this.stop = stop;
        }

        /// <summary>
        /// Used for errors
        /// </summary>
        public MobileEmployeeAvailability(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            //Set default values
        }

        #endregion

        #region Public methods

        #endregion

        #region Overrided methods

        public XElement ToXElement()
        {
            XElement element = new XElement(ROOTNAME);

            element.Add(new XElement("EAIA", StringUtility.GetString(this.isAvailable)));
            element.Add(new XElement("EAIUA", StringUtility.GetString(this.isUnavailable)));
            element.Add(new XElement("EAST", this.start.ToShortTimeString()));
            element.Add(new XElement("EASP", this.stop.ToShortTimeString()));

            return element;
        }

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();

            return CreateDocument(ROOTNAME, elements);
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            string xml = "";
            return XmlUtil.CreateXml(ROOTNAME, xml);
        }

        #endregion
    }

    #endregion

    #region Company holidays

    internal class MobileCompanyHolidays : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "Holidays";

        #endregion

        #region Variables

        #region Collections

        private List<MobileCompanyHoliday> mobileHolidays;

        #endregion

        #endregion

        #region Ctor

        public MobileCompanyHolidays(MobileParam param, List<HolidayDTO> holidays)
            : base(param)
        {
            Init();
            AddMobileHolidays(holidays);
        }

        /// <summary>Used for errors</summary>
        public MobileCompanyHolidays(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            this.mobileHolidays = new List<MobileCompanyHoliday>();
        }

        #endregion

        #region Public methods

        public void AddMobileHolidays(List<HolidayDTO> holidays)
        {
            if (holidays == null)
                return;

            foreach (var holiday in holidays)
            {
                AddMobileHoliday(new MobileCompanyHoliday(this.Param, holiday));
            }
        }

        public void AddMobileHoliday(MobileCompanyHoliday mobileHoliday)
        {
            if (mobileHoliday == null)
                return;

            mobileHolidays.Add(mobileHoliday);
        }

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            return MergeDocuments(ROOTNAME, mobileHolidays.Select(i => i.ToXDocument()).ToList());
        }

        #endregion        
    }

    internal class MobileCompanyHoliday : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "Holiday";

        #endregion

        #region Variables

        #region Field values

        readonly HolidayDTO holiday;

        #endregion

        #endregion

        #region Ctor

        public MobileCompanyHoliday(MobileParam param, HolidayDTO holiday)
            : base(param)
        {
            Init();

            this.holiday = holiday;
        }

        /// <summary>
        /// Used for errors
        /// </summary>
        public MobileCompanyHoliday(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            //Set default values
        }

        #endregion

        #region Public methods

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();

            elements.Add(new XElement("Date", holiday.Date.ToShortDateString()));


            return CreateDocument(ROOTNAME, elements);
        }

        #endregion       
    }

    #endregion

    #region AbsenceRequestShift

    internal class MobileSaveAbsenceRequestShifts
    {
        #region Variables

        //[#] = next element och [##] = next shift

        public List<MobileParsedAbsenceRequestShift> ParsedShifts = new List<MobileParsedAbsenceRequestShift>();
        public bool ParseSucceded = true;
        readonly private int expectedElementCount = 6;
        #endregion

        #region Ctor

        public MobileSaveAbsenceRequestShifts(String shifts)
        {
            Parse(shifts);
        }

        #endregion

        public bool IsShiftsMissingReplacements()
        {
            return this.ParsedShifts.Any(x => x.ApprovalType == TermGroup_YesNo.Yes && x.EmployeeId == 0);
        }

        public bool IsShiftsMissingApproval()
        {
            return this.ParsedShifts.Any(x => x.ApprovalType == TermGroup_YesNo.Unknown && x.EmployeeId != 0);
        }
        public bool NoShiftsAreApporoved()
        {
            return this.ParsedShifts.Count(x => x.ApprovalType == TermGroup_YesNo.Unknown) == this.ParsedShifts.Count;
        }

        public List<MobileParsedAbsenceRequestShift> GetShiftsToValidate()
        {
            return ParsedShifts.Where(x => x.ApprovalType == TermGroup_YesNo.Yes && x.EmployeeId != 0 && x.EmployeeId != Constants.NO_REPLACEMENT_EMPLOYEEID && !x.AlreadyValidated).ToList();
        }

        public List<MobileParsedAbsenceRequestShift> GetShiftsToPlan()
        {
            return ParsedShifts.Where(x => (x.ApprovalType == TermGroup_YesNo.Yes && x.EmployeeId != 0) || x.ApprovalType == TermGroup_YesNo.No).ToList();
        }

        #region Help-methods

        private void Parse(String shifts)
        {
            string[] shiftSeparator = new string[1];
            shiftSeparator[0] = "[##]";
            string[] elementSeparator = new string[1];
            elementSeparator[0] = "[#]";

            string[] separatedAnswers = shifts.Split(shiftSeparator, StringSplitOptions.RemoveEmptyEntries);

            foreach (string separatedAnswer in separatedAnswers)
            {
                string[] separatedElements = separatedAnswer.Trim().Split(elementSeparator, StringSplitOptions.None);
                if (separatedElements.Count() != expectedElementCount)
                {

                    ParseSucceded = false;
                    return;
                }

                string shiftIdStr = separatedElements[0].Trim();
                string absenceStartTimeStr = separatedElements[1].Trim();
                string absenceStopTimeStr = separatedElements[2].Trim();
                string employeeIdStr = separatedElements[3].Trim();
                string approvalTypeIdStr = separatedElements[4].Trim();
                string alreadyValidatedStr = separatedElements[5].Trim();

                #region Parse integers

                if ((!Int32.TryParse(shiftIdStr, out int shiftId)) || (!Int32.TryParse(employeeIdStr, out int employeeId)) || (!Int32.TryParse(approvalTypeIdStr, out int approvalTypeId)))
                {
                    ParseSucceded = false;
                    return;
                }

                #endregion

                if (!Enum.IsDefined(typeof(TermGroup_YesNo), approvalTypeId))
                {
                    ParseSucceded = false;
                    return;
                }

                #region Parse datetimes

                DateTime? absenceStartTime = null;
                DateTime? absenceStopTime = null;

                if (!String.IsNullOrEmpty(absenceStartTimeStr))
                    absenceStartTime = CalendarUtility.GetNullableDateTime(absenceStartTimeStr);

                if (!String.IsNullOrEmpty(absenceStopTimeStr))
                    absenceStopTime = CalendarUtility.GetNullableDateTime(absenceStopTimeStr);


                if (!absenceStartTime.HasValue || !absenceStopTime.HasValue)
                {
                    ParseSucceded = false;
                    return;
                }

                #endregion

                #region Parse bools

                bool? alreadyValidated = StringUtility.GetNullableBool(alreadyValidatedStr);
                if (!alreadyValidated.HasValue)
                {
                    ParseSucceded = false;
                    return;
                }

                #endregion

                MobileParsedAbsenceRequestShift shift = new MobileParsedAbsenceRequestShift(shiftId, absenceStartTime.Value, absenceStopTime.Value, employeeId, (TermGroup_YesNo)approvalTypeId, alreadyValidated.Value);
                ParsedShifts.Add(shift);
            }
        }

        #endregion
    }

    internal class MobileParsedAbsenceRequestShift
    {
        #region Variables

        public int Id;
        public DateTime AbsenceStartTime;
        public DateTime AbsenceStopTime;
        public int EmployeeId;
        public TermGroup_YesNo ApprovalType;
        public bool AlreadyValidated; //If user used search employee, this will be true

        #endregion

        #region Ctor

        public MobileParsedAbsenceRequestShift(int id, DateTime absenceStartTime, DateTime absenceStopTime, int employeeId, TermGroup_YesNo approvalType, bool alreadyValidated)
        {
            this.Id = id;
            this.AbsenceStartTime = absenceStartTime;
            this.AbsenceStopTime = absenceStopTime;
            this.EmployeeId = employeeId;
            this.ApprovalType = approvalType;
            this.AlreadyValidated = alreadyValidated;
        }

        #endregion
    }

    internal class MobileAbsenceRequestShift : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "Shift";

        #endregion

        #region Variables

        public int Id;
        public int EmployeeId;
        readonly private DateTime date;
        public DateTime StartTime;
        readonly private DateTime stopTime;
        public DateTime AbsenceStartTime;
        public DateTime AbsenceStopTime;
        readonly private string shiftTypeName;
        public int approvalTypeId;

        #endregion

        #region Ctor

        public MobileAbsenceRequestShift(MobileParam param)
            : base(param)
        {
            Init();
        }

        public MobileAbsenceRequestShift(MobileParam param, TimeSchedulePlanningDayDTO shiftDTO)
            : base(param)
        {
            Init();

            this.Id = shiftDTO.TimeScheduleTemplateBlockId;
            this.date = shiftDTO.ActualDate;
            this.StartTime = shiftDTO.StartTime;
            this.stopTime = shiftDTO.StopTime;
            this.AbsenceStartTime = shiftDTO.AbsenceStartTime;
            this.AbsenceStopTime = shiftDTO.AbsenceStopTime;
            this.EmployeeId = shiftDTO.EmployeeId;
            this.shiftTypeName = shiftDTO.ShiftTypeName;
            this.approvalTypeId = shiftDTO.ApprovalTypeId;
        }

        public MobileAbsenceRequestShift(MobileParam param, int id, DateTime date, DateTime startTime, DateTime stopTime, DateTime absenceStartTime, DateTime absenceStopTime, int employeeId, string shiftTypeName, int approvalTypeId)
        : base(param)
        {
            Init();

            this.Id = id;
            this.date = date;
            this.StartTime = startTime;
            this.stopTime = stopTime;
            this.AbsenceStartTime = absenceStartTime;
            this.AbsenceStopTime = absenceStopTime;
            this.EmployeeId = employeeId;
            this.shiftTypeName = shiftTypeName;
            this.approvalTypeId = approvalTypeId;
        }

        /// <summary>
        /// Used for errors
        /// </summary>
        public MobileAbsenceRequestShift(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            //Set default values
        }

        #endregion

        #region Public methods

        public XElement ToXElement()
        {
            XElement element = new XElement(ROOTNAME);
            foreach (var item in this.GetElements())
            {
                element.Add(item);
            }

            return element;
        }

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();

            elements.AddRange(this.GetElements());

            return CreateDocument(ROOTNAME, elements);
        }

        public override XDocument ToXDocument(MobileTask task)
        {
            if (base.Failed)
                return MobileMessages.GetErrorMessageDocument(base.ErrorMessage);

            bool result = base.GetTaskResult(task);
            switch (task)
            {
                case MobileTask.SaveAbsenceRequestPlanning:
                    return MobileMessages.GetSuccessDocument(result);
                default:
                    return base.ToXDocument(task);
            }
        }

        #endregion

        #region Private Methods

        private List<XElement> GetElements()
        {

            var elements = new List<XElement>();

            elements.Add(new XElement("Id", Id));
            elements.Add(new XElement("Date", date.ToShortDateString()));
            elements.Add(new XElement("Start", StartTime.ToShortDateShortTimeString()));
            elements.Add(new XElement("Stop", stopTime.ToShortDateShortTimeString()));
            elements.Add(new XElement("AbsenceStart", AbsenceStartTime.ToShortDateShortTimeString()));
            elements.Add(new XElement("AbsenceStop", AbsenceStopTime.ToShortDateShortTimeString()));
            elements.Add(new XElement("ShiftType", shiftTypeName));
            elements.Add(new XElement("EmployeeId", EmployeeId));
            elements.Add(new XElement("ApprovalTypeId", approvalTypeId));

            return elements;
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            string xml = "";

            return XmlUtil.CreateXml(ROOTNAME, xml);
        }

        #endregion
    }


    #endregion

    #region MobileCustomerCreditLimit

    internal class MobileCustomerCreditLimit : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "MobileCustomerCreditLimit";

        #endregion

        #region Variables

        private readonly bool hasCreditLimit;
        private readonly decimal limit;
        private readonly decimal value;

        #endregion

        #region Ctor

        public MobileCustomerCreditLimit(MobileParam param, bool hasCreditLimit, decimal limit, decimal value)
            : base(param)
        {
            Init();

            this.hasCreditLimit = hasCreditLimit;
            this.limit = limit;
            this.value = value;
        }

        /// <summary>
        /// Used for errors
        /// </summary>
        public MobileCustomerCreditLimit(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            //Set default values
        }

        #endregion

        #region Public methods

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>
            {
                new XElement("HasCreditLimit", StringUtility.GetString(hasCreditLimit)),
                new XElement("Limit", limit.ToString()),
                new XElement("Value", value.ToString())
            };

            return CreateDocument(ROOTNAME, elements);
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            string xml = "<DecimalValue>0</DecimalValue>";

            return XmlUtil.CreateXml(ROOTNAME, xml);
        }

        #endregion
    }

    #endregion

    #region MobileValueResult

    internal class MobileValueResult : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "MobileValueResult";

        #endregion

        #region Variables

        private readonly decimal decimalValue;

        #endregion

        #region Ctor

        public MobileValueResult(MobileParam param, decimal decimalValue)
            : base(param)
        {
            Init();
            this.decimalValue = decimalValue;
        }

        /// <summary>
        /// Used for errors
        /// </summary>
        public MobileValueResult(MobileParam param, string errorMessage) :
            base(param, errorMessage)
        {
            Init();
        }

        private void Init()
        {
            //Set default values
        }

        #endregion

        #region Public methods

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();
            elements.Add(new XElement("DecimalValue", this.decimalValue));

            return CreateDocument(ROOTNAME, elements);
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            string xml = "<DecimalValue>0</DecimalValue>";

            return XmlUtil.CreateXml(ROOTNAME, xml);
        }

        #endregion
    }

    #endregion

    #region MobileResult

    internal class MobileResult : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "ActionResult";

        #endregion

        #region Variables
        readonly private bool success;
        readonly private string errorMessage;
        private string message;
        private string successMessage;

        #endregion

        #region Ctor

        public MobileResult(MobileParam param) : base(param)
        {
            this.success = true;
            Init();
        }

        public MobileResult(MobileParam param, ActionResult result) : base(param)
        {
            this.success = result.Success;
            this.errorMessage = result.ErrorMessage;
            Init();
        }

        /// <summary>
        /// Used for errors
        /// </summary>
        public MobileResult(MobileParam param, string errorMessage) : base(param, errorMessage)
        {
            this.success = false;
            this.errorMessage = errorMessage;
            Init();
        }

        private void Init()
        {
            //Set default values
        }

        #endregion

        #region Public methods

        public void SetMessage(string message)
        {
            this.message = message;
        }

        public void SetSuccessMessage(string successMessage)
        {
            this.successMessage = successMessage;
        }

        #endregion

        #region Private methods

        public override XDocument ToXDocument()
        {
            var elements = new List<XElement>();

            elements.Add(new XElement("Success", StringUtility.GetString(this.success)));
            elements.Add(new XElement("ErrorMessage", this.errorMessage));
            elements.Add(new XElement("SuccessMessage", this.successMessage));

            return CreateDocument(ROOTNAME, elements);
        }

        #endregion

        #region Overrided methods

        public override XDocument ToXDocument(MobileTask task)
        {
            if (base.Failed)
                return MobileMessages.GetErrorMessageDocument(base.ErrorMessage);

            bool result = base.GetTaskResult(task);
            switch (task)
            {
                case MobileTask.UpdateSetting:
                case MobileTask.DeleteProjectTimeBlock:
                case MobileTask.DeleteExpense:
                case MobileTask.MoveProjectTimeBlockToDate:
                case MobileTask.SaveAttestAccountRow:
                case MobileTask.Update:
                case MobileTask.Save:
                case MobileTask.Delete:
                    return MobileMessages.GetSuccessDocument(result);
                case MobileTask.SetOrderIsReady:
                    return MobileMessages.GetStringMessageDocument("IsReady", result ? "1" : "0");
                case MobileTask.GetMessage:
                    return MobileMessages.GetMessageDocument(message);
                case MobileTask.GetValue:
                    return MobileMessages.GetStringMessageDocument("Value", message);
                default:
                    return base.ToXDocument(task);
            }
        }

        #endregion
    }

    #endregion

    #region MobileList

    internal class MobileList<T1, T2> : MobileBase
    {
        #region Variables

        private readonly string name;
        private readonly List<MobileListRow<T1, T2>> rows;

        #endregion

        #region Ctor

        public MobileList(MobileParam param, string name)
            : base(param)
        {
            this.name = name;
            this.rows = new List<MobileListRow<T1, T2>>();
        }

        #endregion

        #region Public methods

        public void AddRows(int type, Dictionary<T1, T2> dict)
        {
            if (dict.IsNullOrEmpty())
                return;

            foreach (var pair in dict)
            {
                this.rows.Add(new MobileListRow<T1, T2>(this.Param, type, pair));
            }
        }

        public XElement ToXElement()
        {
            XElement element = new XElement(this.name);
            foreach (var row in this.rows)
            {
                element.Add(row.ToXElement());
            }

            return element;
        }

        #endregion
    }

    internal class MobileListRow<T1, T2> : MobileBase
    {
        #region Constants

        public const string ROOTNAME = "Row";

        #endregion

        #region Variables

        private readonly string id;
        private readonly string value;
        private readonly int type;

        #endregion

        #region Ctor

        public MobileListRow(MobileParam param, int type, KeyValuePair<T1, T2> pair) : base(param)
        {
            Init();

            this.id = pair.Key.ToString();
            this.value = pair.Value.ToString();
            this.type = type;
        }

        private void Init()
        {
            //Set default values
        }

        #endregion

        #region Public methods

        public XElement ToXElement()
        {
            var rootElement = new XElement(ROOTNAME);
            rootElement.Add(new XElement("Id", this.id));
            rootElement.Add(new XElement("Value", this.value));
            rootElement.Add(new XElement("Type", this.type));
            return rootElement;
        }

        #endregion

        #region Static methods

        public static string GetDefaultXml()
        {
            string xml =
                "<Id></Id>" +
                "<Value>0</Name>" +
                "<Type>0</Type>";
            return XmlUtil.CreateXml(ROOTNAME, xml);
        }

        #endregion
    }

    #endregion

    #region Helpers

    static internal class Helpers
    {
        public static void GetCustomerAddresses(IGrouping<int, CustomerSearchView> customer, out string deliveryAddress, out string billingAddress)
        {
            deliveryAddress = String.Empty;
            billingAddress = String.Empty;

            var deliveryAddressItem = customer.FirstOrDefault(c => c.SysContactAddressTypeId == (int)TermGroup_SysContactAddressType.Delivery && c.SysContactAddressRowTypeId == (int)TermGroup_SysContactAddressRowType.Address && !string.IsNullOrEmpty(c.AddressText));
            if (deliveryAddressItem != null)
            {
                deliveryAddress = deliveryAddressItem.AddressText;
                deliveryAddress += " " + customer.FirstOrDefault(c => c.ContactAddressId == deliveryAddressItem.ContactAddressId && c.SysContactAddressTypeId == (int)TermGroup_SysContactAddressType.Delivery && c.SysContactAddressRowTypeId == (int)TermGroup_SysContactAddressRowType.PostalCode && !string.IsNullOrEmpty(c.AddressText))?.AddressText;
                deliveryAddress += " " + customer.FirstOrDefault(c => c.ContactAddressId == deliveryAddressItem.ContactAddressId && c.SysContactAddressTypeId == (int)TermGroup_SysContactAddressType.Delivery && c.SysContactAddressRowTypeId == (int)TermGroup_SysContactAddressRowType.PostalAddress && !string.IsNullOrEmpty(c.AddressText))?.AddressText;
            }
            var billingAddressItem = customer.FirstOrDefault(c => c.SysContactAddressTypeId == (int)TermGroup_SysContactAddressType.Billing && c.SysContactAddressRowTypeId == (int)TermGroup_SysContactAddressRowType.Address && !string.IsNullOrEmpty(c.AddressText));
            if (billingAddressItem != null)
            {
                billingAddress = billingAddressItem.AddressText;
                billingAddress += " " + customer.FirstOrDefault(c => c.ContactAddressId == billingAddressItem.ContactAddressId && c.SysContactAddressTypeId == (int)TermGroup_SysContactAddressType.Billing && c.SysContactAddressRowTypeId == (int)TermGroup_SysContactAddressRowType.PostalCode && !string.IsNullOrEmpty(c.AddressText))?.AddressText;
                billingAddress += " " + customer.FirstOrDefault(c => c.ContactAddressId == billingAddressItem.ContactAddressId && c.SysContactAddressTypeId == (int)TermGroup_SysContactAddressType.Billing && c.SysContactAddressRowTypeId == (int)TermGroup_SysContactAddressRowType.PostalAddress && !string.IsNullOrEmpty(c.AddressText))?.AddressText;
            }
        }
    }

    #endregion
}