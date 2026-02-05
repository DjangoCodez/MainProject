using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace SoftOne.Soe.Business.Util
{
    #region Container

    public abstract class SieContainerBase
    {
        #region Collections

        public List<SieSyntaxItem> SyntaxItems = new List<SieSyntaxItem>();
        public List<SieAccountDimItem> AccountDimItems = new List<SieAccountDimItem>();
        public List<SieAccountStdItem> AccountStdItems = new List<SieAccountStdItem>();
        public List<SieAccountInternalItem> AccountInternalItems = new List<SieAccountInternalItem>();
        public List<SieAccountStdInBalanceItem> AccountStdInBalanceItems = new List<SieAccountStdInBalanceItem>();
        public List<SieAccountStdOutBalanceItem> AccountStdOutBalanceItems = new List<SieAccountStdOutBalanceItem>();
        public List<SieAccountStdYearBalanceItem> AccountStdYearBalanceItems = new List<SieAccountStdYearBalanceItem>();
        public List<SieAccountPeriodBalanceItem> AccountPeriodBalanceItems = new List<SieAccountPeriodBalanceItem>();
        public List<SieAccountPeriodBudgetBalanceItem> AccountPeriodBudgetBalanceItems = new List<SieAccountPeriodBudgetBalanceItem>();
        public List<SieAccountInternalInBalanceItem> AccountInternalInBalanceItems = new List<SieAccountInternalInBalanceItem>();
        public List<SieAccountInternalOutBalanceItem> AccountInternalOutBalanceItems = new List<SieAccountInternalOutBalanceItem>();
        public List<SieVoucherItem> VoucherItems = new List<SieVoucherItem>();
        public List<SieAccountYearItem> AccountYearItems = new List<SieAccountYearItem>();


        #endregion

        #region Conflicts

        public virtual int NoOfConflicts() { return 0; }
        public virtual List<SieImportItemBase> GetConflicts() { return null; }
        public string Message { get; set; }
        public bool MessageRequireRepopulate { get; set; }

        public bool HasConflicts()
        {
            return NoOfConflicts() > 0;
        }

        public int NoOfSyntaxConflicts()
        {
            return (from si in SyntaxItems
                    select si.NoOfConflicts()).Sum();
        }

        public int NoOfAccountDimConflicts()
        {
            return (from adi in AccountDimItems
                    select adi.NoOfConflicts()).Sum();
        }

        public int NoOfAccountStdConflicts()
        {
            return (from asi in AccountStdItems
                    select asi.NoOfConflicts()).Sum();
        }

        public int NoOfAccountInternalConflicts()
        {
            return (from aii in AccountInternalItems
                    select aii.NoOfConflicts()).Sum();
        }

        public int NoOfAccountStdInBalanceConflicts()
        {
            return (from aibi in AccountStdInBalanceItems
                    select aibi.NoOfConflicts()).Sum();
        }

        public int NoOfAccountStdOutBalanceConflicts()
        {
            return (from aobi in AccountStdOutBalanceItems
                    select aobi.NoOfConflicts()).Sum();
        }

        public int NoOfAccountStdYearBalanceConflicts()
        {
            return (from aybi in AccountStdYearBalanceItems
                    select aybi.NoOfConflicts()).Sum();
        }

        public int NoOfAccountStdPeriodBalanceConflicts()
        {
            return (from apbi in AccountPeriodBalanceItems
                    select apbi.NoOfConflicts()).Sum();
        }

        public int NoOfAccountInternalInBalanceConflicts()
        {
            return (from aiibi in AccountInternalInBalanceItems
                    select aiibi.NoOfConflicts()).Sum();
        }

        public int NoOfAccountInternalOutBalanceConflicts()
        {
            return (from aiobi in AccountInternalOutBalanceItems
                    select aiobi.NoOfConflicts()).Sum();
        }

        public int NoOfVoucherConflicts()
        {
            return (from vi in VoucherItems
                    select vi.NoOfConflicts()).Sum();
        }

        public void GetSyntaxConflicts(List<SieImportItemBase> conflictList)
        {
            if (conflictList == null)
                return;

            var query = from si in SyntaxItems
                        where si.NoOfConflicts() > 0
                        select si;

            foreach (SieSyntaxItem syntaxItem in query)
                AddItemToList(conflictList, syntaxItem);
        }

        public void GetAccountInternalConflicts(List<SieImportItemBase> conflictList)
        {
            if (conflictList == null)
                return;

            var query = from ait in AccountInternalItems
                        where ait.NoOfConflicts() > 0
                        select ait;

            foreach (SieAccountInternalItem accountInternalItem in query)
                AddItemToList(conflictList, accountInternalItem);
        }

        public void GetAccountDimConflicts(List<SieImportItemBase> conflictList)
        {
            if (conflictList == null)
                return;

            var query = from adi in AccountDimItems
                        where adi.NoOfConflicts() > 0
                        select adi;

            foreach (SieAccountDimItem accountDimItem in query)
                AddItemToList(conflictList, accountDimItem);
        }

        public void GetAccountStdConflicts(List<SieImportItemBase> conflictList)
        {
            if (conflictList == null)
                return;

            var query = from asi in AccountStdItems
                        where asi.NoOfConflicts() > 0
                        select asi;

            foreach (SieAccountStdItem accountStdItem in query)
                AddItemToList(conflictList, accountStdItem);
        }

        public void GetAccountStdInBalanceConflicts(List<SieImportItemBase> conflictList)
        {
            if (conflictList == null)
                return;

            var query = from aibi in AccountStdInBalanceItems
                        where aibi.NoOfConflicts() > 0
                        select aibi;

            foreach (SieAccountStdInBalanceItem accountStdInBalanceItem in query)
                AddItemToList(conflictList, accountStdInBalanceItem);
        }

        public void GetAccountStdOutBalanceConflicts(List<SieImportItemBase> conflictList)
        {
            if (conflictList == null)
                return;

            var query = from aobi in AccountStdOutBalanceItems
                        where aobi.NoOfConflicts() > 0
                        select aobi;

            foreach (SieAccountStdOutBalanceItem accountStdOutBalanceItem in query)
                AddItemToList(conflictList, accountStdOutBalanceItem);
        }

        public void GetAccountStdYearBalanceConflicts(List<SieImportItemBase> conflictList)
        {
            if (conflictList == null)
                return;

            var query = from aybi in AccountStdYearBalanceItems
                        where aybi.NoOfConflicts() > 0
                        select aybi;

            foreach (SieAccountStdYearBalanceItem accountStdYearBalanceItem in query)
                AddItemToList(conflictList, accountStdYearBalanceItem);
        }

        public void GetAccountStdPeriodBalanceConflicts(List<SieImportItemBase> conflictList)
        {
            if (conflictList == null)
                return;

            var query = from apbi in AccountPeriodBalanceItems
                        where apbi.NoOfConflicts() > 0
                        select apbi;

            foreach (SieAccountPeriodBalanceItem accountStdPeriodBalanceItem in query)
                AddItemToList(conflictList, accountStdPeriodBalanceItem);
        }

        public void GetAccountInternalInBalanceConflicts(List<SieImportItemBase> conflictList)
        {
            if (conflictList == null)
                return;

            var query = from aiibi in AccountInternalInBalanceItems
                        where aiibi.NoOfConflicts() > 0
                        select aiibi;

            foreach (SieAccountInternalInBalanceItem accountInternalInBalanceItem in query)
                AddItemToList(conflictList, accountInternalInBalanceItem);
        }

        public void GetAccountInternalOutBalanceConflicts(List<SieImportItemBase> conflictList)
        {
            if (conflictList == null)
                return;

            var query = from aiobi in AccountInternalOutBalanceItems
                        where aiobi.NoOfConflicts() > 0
                        select aiobi;

            foreach (SieAccountInternalOutBalanceItem accountInternalOutBalanceItem in query)
                AddItemToList(conflictList, accountInternalOutBalanceItem);
        }

        public void GetVoucherConflicts(List<SieImportItemBase> conflictList)
        {
            if (conflictList == null)
                return;

            var query = from si in VoucherItems
                        where si.NoOfConflicts() > 0
                        select si;

            foreach (SieVoucherItem voucherItem in query)
                AddItemToList(conflictList, voucherItem);
        }

        #endregion

        #region General

        public void AddItemToList(List<SieImportItemBase> conflictList, Object obj)
        {
            if (obj is SieImportItemBase sie)
                conflictList.Add(sie);
        }

        #endregion
    }

    public class SieImportContainer : SieContainerBase
    {
        #region Properties

        //Standard
        public int UserId { get; set; }
        public int ActorCompanyId { get; set; }
        public SieImportType ImportType { get; set; }
        public StreamReader StreamReader { get; set; }

        public StreamReader StreamReaderAccount { get; set; }
        public StreamReader StreamReaderVoucher { get; set; }
        public StreamReader StreamReaderAccountBalance { get; set; }

        public bool AllowNotOpenAccountYear { get; set; }
        
        //Account Voucher AccountBalance        
        public bool AvabSuccessImportAccount { get; set; }
        public bool AvabSuccessImportVoucher { get; set; }
        public bool AvabSuccessImportAccountBalance { get; set; }
        public bool AvabFailedImportAccount { get; set; }
        public bool AvabFiledImportVoucher { get; set; }
        public bool AvabFailedImportAccountBalance { get; set; }        
        public string AvabErrorMessageGeneral { get; set; }
        
        //Account import
        public bool OverwriteNameConflicts { get; set; }
        public bool ApproveEmptyAccountNames { get; set; }
        public bool ImportAccountStd { get; set; }
        public bool ImportAccountInternal { get; set; }
        public bool ImportAsUtf8 { get; set; }
        public string EmptyAccountName { get; set; }

        //AccountBalance import
        public bool OverrideAccountBalance { get; set; }
        public bool UseUBInsteadOfIB { get; set; }

        //Voucher import
        public int? DefaultVoucherSeriesId { get; set; }
        public bool OverrideVoucherSeries { get; set; }
        public Dictionary<string, int> VoucherSeriesTypesMappingDict { get; set; }
        public bool HasVoucherSeriesTypesMapping
        {
            get
            {
                return VoucherSeriesTypesMappingDict != null && VoucherSeriesTypesMappingDict.Count > 0;
            }
        }
        public bool OverrideVoucherDeletes { get; set; }
        public bool SkipAlreadyExistingVouchers { get; set; }
        public Dictionary<int, bool> VoucherSeriesDeleteDict { get; set; }
        public bool HasVoucherSeriesDeletes
        {
            get
            {
                return VoucherSeriesDeleteDict != null && VoucherSeriesDeleteDict.Count > 0;
            }
        }
        public bool TakeVoucherNrFromSeries { get; set; }
        //AccountYear
        public int AccountYearId { get; set; }
        public AccountYear AccountYear { get; set; }

        //Calculated
        public string SieKpTyp { get; set; }
        #endregion

        #region Conflicts

        public override int NoOfConflicts()
        {
            int noOfConflicts = 0;

            switch (ImportType)
            {
                case SieImportType.Account:
                    noOfConflicts =
                    NoOfSyntaxConflicts() +
                    NoOfAccountDimConflicts() +
                    NoOfAccountInternalConflicts() +
                    NoOfAccountStdConflicts();
                    break;
                case SieImportType.Voucher:
                    noOfConflicts =
                    NoOfSyntaxConflicts() +
                    NoOfVoucherConflicts();
                    break;
                case SieImportType.AccountBalance:
                    noOfConflicts =
                    NoOfSyntaxConflicts() +
                    NoOfAccountStdInBalanceConflicts();
                    break;
                case SieImportType.Account_Voucher_AccountBalance:
                    noOfConflicts =
                    NoOfSyntaxConflicts() +
                    NoOfAccountDimConflicts() +
                    NoOfAccountInternalConflicts() +
                    NoOfAccountStdConflicts() +
                    NoOfVoucherConflicts() +
                    NoOfAccountStdInBalanceConflicts();
                    break;

            }

            return noOfConflicts;
        }


        public List<SieImportItemBase> GetAllConflicts()
        {
            List<SieImportItemBase> conflictList = new List<SieImportItemBase>();
            GetSyntaxConflicts(conflictList);
            GetAccountDimConflicts(conflictList);
            GetAccountStdConflicts(conflictList);
            GetAccountInternalConflicts(conflictList);
            GetVoucherConflicts(conflictList);
            GetAccountStdInBalanceConflicts(conflictList);
            return conflictList
                .OrderBy(c => c.LineNr)
                .ToList();
        }

        public override List<SieImportItemBase> GetConflicts()
        {
            List<SieImportItemBase> conflictList = new List<SieImportItemBase>();

            //SyntaxItems
            GetSyntaxConflicts(conflictList);

            switch (ImportType)
            {
                case SieImportType.Account:
                    GetAccountDimConflicts(conflictList);
                    GetAccountStdConflicts(conflictList);
                    GetAccountInternalConflicts(conflictList);
                    break;
                case SieImportType.Voucher:
                    GetVoucherConflicts(conflictList);
                    break;
                case SieImportType.AccountBalance:
                    GetAccountStdInBalanceConflicts(conflictList);
                    break;
                case SieImportType.Account_Voucher_AccountBalance:                    
                    GetAccountDimConflicts(conflictList);
                    GetAccountStdConflicts(conflictList);
                    GetAccountInternalConflicts(conflictList);
                    GetVoucherConflicts(conflictList);
                    GetAccountStdInBalanceConflicts(conflictList);
                    break;
            }

            return conflictList;
        }

        #endregion

        #region Reader

        private int lineNr;
        public int LineNr
        {
            get { return lineNr; }
        }

        public void ResetReadLine()
        {
            lineNr = 0;
        }

        public string ReadLine()
        {
            lineNr++;

            string line = StreamReader.ReadLine();
            if (line == null)
                return null;

            return line.Trim();
        }

        public void CloseStream()
        {
            if (StreamReader != null)
            {
                StreamReader.Close();
            }
        }

        public string ReadLineAccount()
        {
            lineNr++;

            string line = StreamReaderAccount.ReadLine();
            if (line == null)
                return null;

            return line.Trim();
        }

        public void CloseStreamAccount()
        {
            if (StreamReaderAccount != null)
            {
                StreamReaderAccount.Close();
            }
        }

        public string ReadLineVoucher()
        {
            lineNr++;

            string line = StreamReaderVoucher.ReadLine();
            if (line == null)
                return null;

            return line.Trim();
        }

        public void CloseStreamVoucher()
        {
            if (StreamReaderVoucher != null)
            {
                StreamReaderVoucher.Close();
            }
        }

        public string ReadLineAccountBalance()
        {
            lineNr++;

            string line = StreamReaderAccountBalance.ReadLine();
            if (line == null)
                return null;

            return line.Trim();
        }

        public void CloseStreamAccountBalance()
        {
            if (StreamReaderAccountBalance != null)
            {
                StreamReaderAccountBalance.Close();
            }
        }

        #endregion
    }

    public class SieExportContainer : SieContainerBase
    {
        #region Properties

        //Standard
        public string LoginName { get; set; }
        public int ActorCompanyId { get; set; }
        public SieExportType ExportType { get; set; }
        public TextWriter StreamReader { get; set; }
        public EvaluatedSelection Es { get; set; }

        //Export
        public string Program { get; set; }
        public string Version { get; set; }
        public string Comment { get; set; }

        //Company
        public string CompanyName { get; set; }
        public string OrgNr { get; set; }
        public string ContactName { get; set; }
        public string Address { get; set; }
        public string PostalCode { get; set; }
        public string PostalAddress { get; set; }
        public string Phone { get; set; }

        //AccountYear
        public AccountYear AccountYear { get; set; }
        public AccountYear PreviousAccountYear { get; set; }
        public int AccountYearNr { get; set; } //0 = current, -1 = previous and so on

        //Account
        public int NoOfDimensions { get; set; }
        public bool ExportPreviousYear { get; set; }
        public bool ExportObject { get; set; }
        public bool ExportAccount { get; set; }
        public bool ExportAccountType { get; set; }
        public bool ExportSruCodes { get; set; }

        //Sort
        public TermGroup_SieExportVoucherSort VoucherSortBy { get; set; } = TermGroup_SieExportVoucherSort.Unknown;

        #endregion

        #region Conflicts

        public override int NoOfConflicts()
        {
            int noOfConflicts = 0;

            switch (ExportType)
            {
                case SieExportType.Type1:
                    noOfConflicts =
                    NoOfAccountStdConflicts() +
                    NoOfAccountStdInBalanceConflicts() +
                    NoOfAccountStdOutBalanceConflicts() +
                    NoOfAccountStdYearBalanceConflicts();
                    break;
                case SieExportType.Type2:
                    noOfConflicts =
                    NoOfAccountStdConflicts() +
                    NoOfAccountStdInBalanceConflicts() +
                    NoOfAccountStdOutBalanceConflicts() +
                    NoOfAccountStdYearBalanceConflicts() +
                    NoOfAccountStdPeriodBalanceConflicts();
                    break;
                case SieExportType.Type3:
                    noOfConflicts =
                    NoOfAccountDimConflicts() +
                    NoOfAccountInternalConflicts() +
                    NoOfAccountStdConflicts() +
                    NoOfAccountStdInBalanceConflicts() +
                    NoOfAccountStdOutBalanceConflicts() +
                    NoOfAccountStdYearBalanceConflicts() +
                    NoOfAccountStdPeriodBalanceConflicts();
                    break;
                case SieExportType.Type4:
                    noOfConflicts =
                    NoOfAccountDimConflicts() +
                    NoOfAccountInternalConflicts() +
                    NoOfAccountStdConflicts() +
                    NoOfAccountStdOutBalanceConflicts() +
                    NoOfAccountStdYearBalanceConflicts() +
                    NoOfAccountStdPeriodBalanceConflicts() +
                    NoOfVoucherConflicts();
                    break;
            }

            return noOfConflicts;
        }

        public override List<SieImportItemBase> GetConflicts()
        {
            List<SieImportItemBase> conflictList = new List<SieImportItemBase>();

            switch (ExportType)
            {
                case SieExportType.Type1:
                    //AccountStdItems
                    GetAccountStdConflicts(conflictList);
                    GetAccountStdInBalanceConflicts(conflictList);
                    GetAccountStdOutBalanceConflicts(conflictList);
                    GetAccountStdYearBalanceConflicts(conflictList);
                    break;
                case SieExportType.Type2:
                    //AccountStdItems
                    GetAccountStdConflicts(conflictList);
                    GetAccountStdInBalanceConflicts(conflictList);
                    GetAccountStdOutBalanceConflicts(conflictList);
                    GetAccountStdYearBalanceConflicts(conflictList);
                    GetAccountStdPeriodBalanceConflicts(conflictList);
                    break;
                case SieExportType.Type3:
                    //AccountStdItems
                    GetAccountDimConflicts(conflictList);
                    GetAccountInternalConflicts(conflictList);
                    GetAccountStdConflicts(conflictList);
                    GetAccountStdInBalanceConflicts(conflictList);
                    GetAccountStdOutBalanceConflicts(conflictList);
                    GetAccountStdYearBalanceConflicts(conflictList);
                    GetAccountStdPeriodBalanceConflicts(conflictList);
                    break;
                case SieExportType.Type4:
                    //AccountStdItems
                    GetAccountDimConflicts(conflictList);
                    GetAccountInternalConflicts(conflictList);
                    GetAccountStdConflicts(conflictList);
                    GetAccountStdOutBalanceConflicts(conflictList);
                    GetAccountStdYearBalanceConflicts(conflictList);
                    GetAccountStdPeriodBalanceConflicts(conflictList);
                    GetVoucherConflicts(conflictList);
                    break;
            }

            return conflictList;
        }

        #endregion

        #region Writer

        private int lineNr;
        public int LineNr
        {
            get { return lineNr; }
        }

        public void WriteLine(string line)
        {
            lineNr++;
            StreamReader.Write(line);
        }

        public void CloseWriter()
        {
            if (StreamReader != null)
            {
                StreamReader.Close();
            }
        }

        #endregion
    }

    #endregion

    #region Interfaces

    public interface ILabelItem
    {
        string Label { get; set; }
    }

    public interface INameItem
    {
        string Name { get; set; }
        string OriginalName { get; set; }
    }

    #endregion

    #region SIE    

    public class SieConflictItem
    {
        public SieConflict Conflict { get; set; }
        public string Line { get; set; }
        public int LineNr { get; set; }
        public string StrData { get; set; }
        public int? IntData { get; set; }
        public decimal? DecData { get; set; }
        public DateTime? DateData { get; set; }
    }    

    public class SieImportItemBase : ILabelItem
    {
        public string Label { get; set; }
        public string Line { get; set; }
        public int LineNr { get; set; }
        public bool Invalid { get; set; }

        public SieImportItemBase(string label, string line, int lineNr)
        {
            this.Line = line;
            this.LineNr = lineNr;
            this.Label = label;
        }

        private readonly List<SieConflictItem> conflicts = new List<SieConflictItem>();
        public List<SieConflictItem> Conflicts
        {
            get { return conflicts; }
        }

        public void AddConflict(SieConflict conflict, decimal? value)
        {
            AddConflict(conflict, decData: value);
        }

        public void AddConflict(SieConflict conflict, DateTime? value)
        {
            AddConflict(conflict, dateData: value);
        }

        public void AddConflict(SieConflict conflict, int? value)
        {
            AddConflict(conflict, intData: value);
        }

        public void AddConflict(SieConflict conflict, string value)
        {
            AddConflict(conflict, strData: value);
        }

        public void AddConflict(SieConflict conflict, decimal? decData = null, DateTime? dateData = null, int? intData = null, string strData = "")
        {
            AddConflict(new SieConflictItem()
            {
                Conflict = conflict,
                DecData = decData,
                DateData = dateData,
                IntData = intData,
                StrData = strData,
            });
        }

        public void AddConflicts(List<SieConflictItem> conflicts)
        {
            foreach (var conflict in conflicts)
            {
                AddConflict(conflict);
            }
        }

        public void AddConflict(SieConflictItem conflict)
        {
            if (String.IsNullOrEmpty(conflict.Line))
                conflict.Line = this.Line;
            if (conflict.LineNr == 0)
                conflict.LineNr = this.LineNr;

            if (conflict.Conflict != SieConflict.Import_NameConflict)
                Invalid = true;
            conflicts.Add(conflict);
        }

        public int NoOfConflicts()
        {
            return Conflicts.Count;
        }
    }

    public class SieAccountYearItem : SieImportItemBase
    {
        public SieAccountYearItem(string line, int lineNr) : base(Constants.SIE_LABEL_RAR, line, lineNr) { }
        public int AccountYear { get; set; } // 0 = Current year, 1 = Previous year
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public bool IsCurrentYear { get { return AccountYear == 0; } }
    }
    public class SieSyntaxItem : SieImportItemBase
    {
        public SieSyntaxItem(string line, int lineNr) : base(String.Empty, line, lineNr) { }
    }

    public abstract class SieAccountBalanceItem : SieImportItemBase
    {
        protected SieAccountBalanceItem(string label, string line, int lineNr) : base(label, line, lineNr) { }
        protected SieAccountBalanceItem(string label, int lineNr) : base(label, String.Empty, lineNr) { }

        public int AccountYear { get; set; } //0 = Current year, 1 = Previous year
        public string AccountNr { get; set; }

        private readonly List<SieObjectItem> objItems = new List<SieObjectItem>();
        public List<SieObjectItem> ObjectItems
        {
            get { return objItems; }
        }

        public void AddObjectItem(SieObjectItem objItem)
        {
            if (objItem == null)
                return;

            objItems.Add(objItem);
        }
        public string ObjectItemsString
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                foreach (SieObjectItem objectItem in this.ObjectItems)
                {
                    sb.Append(objectItem.SieDimension);
                    sb.Append(StringUtility.GetAsciiSpace()); 
                    sb.Append(objectItem.ObjectCode);
                    sb.Append(StringUtility.GetAsciiSpace());
                }
                return sb.Length > 0 ? sb.ToString().Trim() : " ";
            }
        }

        public decimal Balance { get; set; }
        public decimal? Quantity { get; set; }
        public string QuantityString
        {
            get
            {
                return Quantity.HasValue ? Quantity.Value.ToString("0.0#####", CultureInfo.InvariantCulture) : string.Empty;
            }
        }

        public bool UseUBInsteadOfIB { get; set; }

        public string ToStringYear(AccountYear accountYear)
        {
            string result = AccountNr;
            if (accountYear != null)
                result += " " + accountYear.GetFromToShortString();
            return result;
        }
    }

    public abstract class SieAccountInternalBalanceItem : SieAccountBalanceItem
    {
        protected SieAccountInternalBalanceItem(string label, string line, int lineNr) : base(label, line, lineNr) { }
        protected SieAccountInternalBalanceItem(string label, int lineNr) : base(label, String.Empty, lineNr) { }
    }

    /// <summary>#DIM</summary>
    public class SieAccountDimItem : SieImportItemBase, INameItem
    {
        public SieAccountDimItem(string line, int lineNr) : base(Constants.SIE_LABEL_DIM, line, lineNr) { }
        public SieAccountDimItem(int lineNr) : base(Constants.SIE_LABEL_DIM, String.Empty, lineNr) { }

        public string Name { get; set; }
        public string OriginalName { get; set; }

        public int? AccountDimNr { get; set; }

        public override string ToString()
        {
            return AccountDimNr + ". " + Name;
        }
    }

    /// <summary>#OBJEKT</summary>
    public class SieAccountInternalItem : SieImportItemBase, INameItem
    {
        public SieAccountInternalItem(string line, int lineNr) : base(Constants.SIE_LABEL_OBJEKT, line, lineNr) { }
        public SieAccountInternalItem(int lineNr) : base(Constants.SIE_LABEL_OBJEKT, String.Empty, lineNr) { }

        public string Name { get; set; }
        public string OriginalName { get; set; }

        public string ObjectCode { get; set; }
        public int? AccountDimNr { get; set; }

        public override string ToString()
        {
            return ObjectCode + ". " + Name;
        }
    }

    /// <summary>#KONTO</summary>
    public class SieAccountStdItem : SieImportItemBase, INameItem
    {
        public SieAccountStdItem(string line, int lineNr) : base(Constants.SIE_LABEL_KONTO, line, lineNr) { }
        public SieAccountStdItem(int lineNr) : base(Constants.SIE_LABEL_KONTO, String.Empty, lineNr) { }

        public string Name { get; set; }
        public string OriginalName { get; set; }

        public string AccountNr { get; set; }
        public int? AccountDimNr { get; set; }
        public int? AccountType { get; set; }
        public string AccountTypeString
        {
            get
            {
                string accountType = "";
                switch (Convert.ToInt32(AccountType))
                {
                    case (int)TermGroup_AccountType.Asset:
                        accountType = "T";
                        break;
                    case (int)TermGroup_AccountType.Debt:
                        accountType = "S";
                        break;
                    case (int)TermGroup_AccountType.Income:
                        accountType = "I";
                        break;
                    case (int)TermGroup_AccountType.Cost:
                        accountType = "K";
                        break;
                }
                return accountType;
            }
        }

        public decimal Balance { get; set; }
        public string SruCode { get; set; }

        public string ToStringSru()
        {
            return AccountNr + ". SRU " + SruCode;
        }
        public override string ToString()
        {
            return AccountNr + ". " + Name;
        }
    }

    /// <summary>#IB</summary>
    public class SieAccountStdInBalanceItem : SieAccountBalanceItem
    {
        public SieAccountStdInBalanceItem(string line, int lineNr) : base(Constants.SIE_LABEL_IB, line, lineNr) { }
        public SieAccountStdInBalanceItem(int lineNr) : base(Constants.SIE_LABEL_IB, String.Empty, lineNr) { }
    }

    /// <summary>#UB</summary>
    public class SieAccountStdOutBalanceItem : SieAccountBalanceItem
    {
        public SieAccountStdOutBalanceItem(string line, int lineNr) : base(Constants.SIE_LABEL_UB, line, lineNr) { }
        public SieAccountStdOutBalanceItem(int lineNr) : base(Constants.SIE_LABEL_UB, String.Empty, lineNr) { }
    }

    /// <summary>#RES</summary>
    public class SieAccountStdYearBalanceItem : SieAccountBalanceItem
    {
        public SieAccountStdYearBalanceItem(string line, int lineNr) : base(Constants.SIE_LABEL_RES, line, lineNr) { }
        public SieAccountStdYearBalanceItem(int lineNr) : base(Constants.SIE_LABEL_RES, String.Empty, lineNr) { }
    }

    /// <summary>#PSALDO</summary>
    public class SieAccountPeriodBalanceItem : SieAccountBalanceItem
    {
        public SieAccountPeriodBalanceItem(string line, int lineNr) : base(Constants.SIE_LABEL_PSALDO, line, lineNr) { }
        public SieAccountPeriodBalanceItem(int lineNr) : base(Constants.SIE_LABEL_PSALDO, String.Empty, lineNr) { }

        public DateTime Period { get; set; }
        public string PeriodString
        {
            get
            {
                return Period.ToString("yyyyMM");
            }
        }
    }

    /// <summary>#PBUDGET</summary>
    public class SieAccountPeriodBudgetBalanceItem : SieAccountBalanceItem
    {
        public SieAccountPeriodBudgetBalanceItem(string line, int lineNr) : base(Constants.SIE_LABEL_PBUDGET, line, lineNr) { }
        public SieAccountPeriodBudgetBalanceItem(int lineNr) : base(Constants.SIE_LABEL_PBUDGET, String.Empty, lineNr) { }

        public DateTime Period { get; set; }
        public string PeriodString
        {
            get
            {
                return Period.ToString("yyyyMM");
            }
        }
    }

    /// <summary>#OIB</summary>
    public class SieAccountInternalInBalanceItem : SieAccountInternalBalanceItem
    {
        public SieAccountInternalInBalanceItem(string line, int lineNr) : base(Constants.SIE_LABEL_OIB, line, lineNr) { }
        public SieAccountInternalInBalanceItem(int lineNr) : base(Constants.SIE_LABEL_OIB, String.Empty, lineNr) { }
    }

    /// <summary>#OUB</summary>
    public  class SieAccountInternalOutBalanceItem : SieAccountInternalBalanceItem
    {
        public SieAccountInternalOutBalanceItem(string line, int lineNr) : base(Constants.SIE_LABEL_OUB, line, lineNr) { }
        public SieAccountInternalOutBalanceItem(int lineNr) : base(Constants.SIE_LABEL_OUB, String.Empty, lineNr) { }
    }

    /// <summary>#VER</summary>
    public class SieVoucherItem : SieImportItemBase
    {
        public SieVoucherItem(string line, int lineNr) : base(Constants.SIE_LABEL_VER, line, lineNr) { }
        public SieVoucherItem(int lineNr) : base(Constants.SIE_LABEL_VER, String.Empty, lineNr) { }

        //From/To SIE file
        public long? VoucherNr { get; set; }
        public string Text { get; set; }
        public string VoucherSeriesTypeNr { get; set; }
        public DateTime VoucherDate { get; set; }
        public string VoucherDateString
        {
            get
            {
                return VoucherDate.ToString("yyyyMMdd");
            }
        }
        public DateTime? RegDate { get; set; }
        public string RegDateString
        {
            get
            {
                return RegDate.HasValue ? RegDate.Value.ToString("yyyyMMdd") : StringUtility.GetAsciiDoubleQoute();
            }
        }
        public Decimal Balance { get; set; }

        //From pre-req check
        public AccountYear AccountYear { get; set; }
        public AccountPeriod AccountPeriod { get; set; }
        public VoucherSeriesType VoucherSeriesType { get; set; }
        public VoucherSeries VoucherSeries { get; set; }
        public bool IsDefaultVoucherSeries { get; set; }
        public bool AddVoucherSeries { get; set; }

        private readonly List<SieTransactionItem> transactionItems = new List<SieTransactionItem>();
        public List<SieTransactionItem> GetTransactionItems()
        {
            //Dont return #BTRANS and #RTRANS
            return transactionItems.Where(i => !i.IsAdded && !i.IsRemoved).ToList();
        }
        public void AddTransactionItem(SieTransactionItem transactionItem)
        {
            transactionItems.Add(transactionItem);
        }

        public SieTransactionItem GetRemovedTransaction(int lineNr)
        {
            return transactionItems.FirstOrDefault(i => i.LineNr == lineNr && i.IsRemoved);
        }
        public SieTransactionItem GetAddedTransaction(int lineNr)
        {
            return transactionItems.FirstOrDefault(i => i.LineNr == lineNr && i.IsAdded);
        }

        public bool HasInvalidTransactions()
        {
            return transactionItems.Any(ti => ti.Invalid);
        }

        public string ToStringTransaction(SieTransactionItem transactionItem)
        {
            string result = "";
            if (VoucherNr.HasValue)
                result = VoucherNr.Value.ToString();
            if (transactionItem != null)
            {
                if (!String.IsNullOrEmpty(result))
                    result += ". ";
                result += transactionItem.AccountNr.ToString();
            }
            return result;
        }
    }

    /// <summary>#TRANS</summary>
    public class SieTransactionItem : SieImportItemBase
    {
        public SieTransactionItem(string line, int lineNr) : base(Constants.SIE_LABEL_TRANS, line, lineNr) { }
        public SieTransactionItem(int lineNr) : base(Constants.SIE_LABEL_TRANS, String.Empty, lineNr) { }

        //From/To SIE file
        public string AccountNr { get; set; }
        public DateTime? TransactionDate { get; set; }
        public string Text { get; set; }
        public decimal Amount { get; set; }
        public decimal? Quantity { get; set; }

        //Flags
        public bool IsRemoved { get; set; }
        public bool IsAdded { get; set; }

        public SieTransactionItem RelatedAddedTransaction { get; set; }

        private readonly List<SieObjectItem> objItems = new List<SieObjectItem>();
        public List<SieObjectItem> ObjectItems
        {
            get { return objItems; }
        }
        public void AddObjectItem(SieObjectItem objectItem)
        {
            objItems.Add(objectItem);
        }
        public string ObjectItemsString
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                foreach (SieObjectItem objectItem in this.ObjectItems)
                {
                    sb.Append(objectItem.SieDimension);
                    sb.Append(StringUtility.GetAsciiSpace());
                    sb.Append(objectItem.ObjectCode);
                    sb.Append(StringUtility.GetAsciiSpace());
                }
                return sb.Length > 0 ? sb.ToString().Trim() : " ";
            }
        }
    }

    /// <summary>{ objectlist }</summary>
    public class SieObjectItem
    {
        public int SieDimension { get; set; }
        public string ObjectCode { get; set; }        
    }

    #endregion
}
