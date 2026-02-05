using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Util.Exceptions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Core
{
    public class SAFTManager : ManagerBase
    {
        // Create a logger for use in this class
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region Namespaces
        
        public readonly XNamespace nl = "urn:StandardAuditFile-Taxation-Financial:NO";
        public readonly XNamespace nl_xsi = "http://www.w3.org/2001/XMLSchema-instance";

        #endregion

        #region Ctor

        public SAFTManager(ParameterObject parameterObject) : base(parameterObject) { }

        #endregion

        public List<SAFTTransactionDTO> GetTransactions(DateTime from, DateTime to, int actorCompanyId)
        {
            return GetVoucherRows(from, to, actorCompanyId);
        }

        public ActionResult Export(DateTime fromDate, DateTime toDate, int actorCompanyId)
        {
            var result = new ActionResult();

            try
            {
                XDocument xdoc = XmlUtil.CreateDocument(Encoding.UTF8, true);

                XElement rootElement = new XElement(nl + "AuditFile",
                                        new XAttribute(XNamespace.Xmlns + "nl", nl),
                                        new XAttribute(XNamespace.Xmlns + "xsi", nl_xsi)
                                    );

                //rootElement.("schemaLocation", "urn:StandardAuditFile-Taxation-Financial:NO Norwegian_SAF-T_Financial_Schema_v_1.10.xsd");
                xdoc.Add(rootElement);

                AddHeader(rootElement);
                AddMasterFiles(rootElement, fromDate, toDate);
                AddGeneralLedgerEntries(rootElement, fromDate, toDate);

                var memory = new MemoryStream();
                xdoc.Save(memory);

                result.StringValue = Encoding.UTF8.GetString(memory.ToArray());
            }
            catch (Exception ex) {
                result.Success = false;
                result.ErrorMessage = ex.Message;
            }

            return result;
        }

        private void AddHeader(XElement root)
        {
            XElement headerNode = new XElement(nl + "Header");
            root.Add(headerNode);

            AddElementNL(headerNode, "AuditFileVersion", "1.0");
            AddElementNL(headerNode, "AuditFileCountry", "NO");
            AddElementNL(headerNode, "AuditFileDateCreated", DateToString(DateTime.Today));
            AddElementNL(headerNode, "SoftwareCompanyName", "Softone AB");
            AddElementNL(headerNode, "SoftwareID", "Softone GO");
            AddElementNL(headerNode, "SoftwareVersion", "1.0");

            AddCompany(headerNode);

            AddElementNL(headerNode, "DefaultCurrencyCode", "NOK");
            AddSelectionCriteria(headerNode, DateTime.Today.AddMonths(-4), DateTime.Today);
            AddElementNL(headerNode, "TaxAccountingBasis", "A");
        }

        private void AddCompany(XElement parent)
        {
            var company = CompanyManager.GetCompany(this.ActorCompanyId);

            var companyNode = AddElementNL(parent, "Company");

            AddElementNL(companyNode, "RegistrationNumber", company.OrgNr);
            AddElementNL(companyNode, "Name", company.Name);

            var contact = ContactManager.GetContactFromActor(company.ActorCompanyId, true, true);

            AddContactAddress(companyNode, contact, $"{company.Name}");
            AddContactInformation(companyNode, contact, true);

            /*
            if (!string.IsNullOrEmpty(company.VatNr))
            {
                AddTaxRegistrationNumber(companyNode, company.VatNr?.ToUpper());
            }
            */
        }

        private void AddSelectionCriteria(XElement parent, DateTime fromDate, DateTime toDate)
        {
            var selectionNode = AddElementNL(parent, "SelectionCriteria");

            AddElementNL(selectionNode, "PeriodStart", fromDate.Month.ToString());
            AddElementNL(selectionNode, "PeriodStartYear", fromDate.Year.ToString());
            AddElementNL(selectionNode, "PeriodEnd", toDate.Month.ToString());
            AddElementNL(selectionNode, "PeriodEndYear", toDate.Year.ToString());
        }

        #region MasterFiles

        private void AddMasterFiles(XElement parent, DateTime fromDate, DateTime toDate)
        {
            var masterNode = AddElementNL(parent, "MasterFiles");

            GeneralLedgerAccounts(masterNode, fromDate, toDate, base.ActorCompanyId);
            AddCustomers(masterNode, fromDate, toDate);
            AddSuppliers(masterNode, fromDate, toDate);
            AddTaxTable(masterNode);
        }

        private void GeneralLedgerAccounts(XElement parent, DateTime fromDate, DateTime toDate, int actorCompanyId)
        {
            var accountsNode = AddElementNL(parent, "GeneralLedgerAccounts");
            var abm = new AccountBalanceManager(this.parameterObject, actorCompanyId);

            using (var entities = new CompEntities())
            {
                //Get AccountDim internals
                var accountStds = AccountManager.GetAccountStdsByCompany(entities, actorCompanyId, true);
                
                var accountYear = AccountManager.GetAccountYear(entities, fromDate, actorCompanyId);
                var accountYearDto = accountYear.ToDTO();
                //Year in
                Dictionary<int, BalanceItemDTO> biYearInBalanceDict = abm.GetYearInBalance(entities, accountYear, accountStds, null, actorCompanyId);
                //Balance change
                Dictionary<int, BalanceItemDTO> biPeriodBalanceDict = abm.GetPeriodOutBalance(entities, accountYear, toDate, accountStds, null, null, biYearInBalanceDict, actorCompanyId);
                Dictionary<int, BalanceItemDTO> fromPeriodBalanceDict = abm.GetPeriodOutBalance(entities, accountYear, fromDate.AddDays(-1), accountStds, null, null, biYearInBalanceDict, actorCompanyId);

                foreach (var account in accountStds)
                {
                    var accountNode = AddElementNL(accountsNode, "Account", null);
                    
                    AddElementNL(accountNode, "AccountID", account.Account.AccountNr);
                    AddElementNL(accountNode, "AccountDescription", account.Account.Name);
                    AddElementNL(accountNode, "StandardAccountID", account.Account.AccountNr.Left(2));
                    AddElementNL(accountNode, "AccountType", "GL");

                    BalanceItemDTO openPeriodBalance = abm.GetAccountBalanceFromDTO(accountYearDto, account.AccountId, biYearInBalanceDict, fromPeriodBalanceDict);

                    //balances
                    AddDebitCreditOpeningBalance(accountNode, openPeriodBalance?.Balance ?? 0);

                    BalanceItemDTO biPeriodBalance = biPeriodBalanceDict.ContainsKey(account.AccountId) ? biPeriodBalanceDict[account.AccountId] : null;
                    AddDebitCreditClosingBalance(accountNode, biPeriodBalance?.Balance ?? 0);
                }
            }
        }
        //Can be limited to the customers representing those parties with transactions for the selection period or with open balances(opening or closing balances not like 0)
        private void AddCustomers(XElement parent, DateTime fromDate, DateTime toDate)
        {
            var customersNode = AddElementNL(parent, "Customers");

            var defaultDebitAccountId = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.AccountCustomerClaim, 0, base.ActorCompanyId, 0);
            var defaultDebitAccountNr = defaultDebitAccountId > 0 ? AccountManager.GetAccount(base.ActorCompanyId, defaultDebitAccountId)?.AccountNr: null;
            var customers = CustomerManager.GetCustomers(base.ActorCompanyId,true, loadContactAddresses:true);
            var customerAccounts = CustomerManager.GetAllCustomerAccounts(base.ActorCompanyId, CustomerAccountType.Debit);
            var customerBalanceList = GetInvoiceBalances(fromDate, toDate, base.ActorCompanyId, SoeOriginType.CustomerInvoice);

            foreach (var customer in customers)
            {
                var contact = customer.Actor.Contact.FirstOrDefault();

                var customerNode = AddElementNL(customersNode, "Customer");
                AddElementNL(customerNode, "RegistrationNumber", customer.OrgNr);
                AddElementNL(customerNode, "Name", customer.Name);

                AddContactAddress(customerNode, contact, $"{customer.CustomerNr}, {customer.Name}");

                AddElementNL(customerNode, "CustomerID", customer.CustomerNr);

                string accountNr;
                if (!customerAccounts.TryGetValue(customer.ActorCustomerId, out accountNr) || string.IsNullOrEmpty(accountNr) )
                {
                    accountNr = defaultDebitAccountNr;
                }

                if (!string.IsNullOrEmpty(accountNr))
                {
                    AddElementNL(customerNode, "AccountID", accountNr);
                }

                var balances = customerBalanceList.FirstOrDefault(x=> x.ActorId == customer.ActorCustomerId);
                AddAmountElement(customerNode, "OpeningDebitBalance", balances?.StartBalance ?? 0);
                AddAmountElement(customerNode, "ClosingDebitBalance", balances?.EndBalance ?? 0);

                //AddTaxRegistrationNumber(customerNode, customer.VatNr);
            }
        }
        //Can be limited to the suppliers representing those parties with transactions for the selection period or with open balances(opening or closing balances not like 0)
        private void AddSuppliers(XElement parent, DateTime fromDate, DateTime toDate)
        {
            var suppliersNode = AddElementNL(parent, "Suppliers");
            var defaultCreditAccountId = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.AccountSupplierDebt, 0, base.ActorCompanyId, 0);
            var defaultCreditAccountNr = defaultCreditAccountId > 0 ? AccountManager.GetAccount(base.ActorCompanyId, defaultCreditAccountId)?.AccountNr : null;
            var supplierAccounts = SupplierManager.GetAllSupplierAccounts(base.ActorCompanyId, SupplierAccountType.Credit);

            var supplierBalanceList = GetInvoiceBalances(fromDate, toDate, base.ActorCompanyId, SoeOriginType.SupplierInvoice);

            var suppliers = SupplierManager.GetSuppliers(base.ActorCompanyId, true, false, true, false, false, false);
            foreach (var supplier in suppliers)
            {
                var contact = supplier.Actor.Contact.FirstOrDefault();

                var supplierNode = AddElementNL(suppliersNode, "Supplier");
                AddElementNL(supplierNode, "RegistrationNumber", supplier.OrgNr);
                AddElementNL(supplierNode, "Name", supplier.Name);
                
                AddContactAddress(supplierNode, contact, $"{supplier.SupplierNr}, {supplier.Name}");

                AddElementNL(supplierNode, "SupplierID", supplier.SupplierNr);

                string accountNr;
                if (!supplierAccounts.TryGetValue(supplier.ActorSupplierId, out accountNr) || string.IsNullOrEmpty(accountNr))
                {
                    accountNr = defaultCreditAccountNr;
                }

                if (!string.IsNullOrEmpty(accountNr))
                {
                    AddElementNL(supplierNode, "AccountID", accountNr);
                }

                var balances = supplierBalanceList.FirstOrDefault(x => x.ActorId == supplier.ActorSupplierId);
                AddAmountElement(supplierNode, "OpeningDebitBalance", balances?.StartBalance ?? 0);
                AddAmountElement(supplierNode, "ClosingDebitBalance", balances?.EndBalance ?? 0);

                //AddTaxRegistrationNumber(supplierNode, supplier.VatNr);
            }
        }

        private void AddContactInformation(XElement parent, Contact contact, bool addUser)
        {
            var contactNode = AddElementNL(parent, "Contact");

            if (addUser)
            {
                var contactPersonNode = AddElementNL(contactNode, "ContactPerson");
                var user = UserManager.GetUser(UserId);
                if (string.IsNullOrEmpty(user.Name))
                {
                    throw new ActionFailedException("Användaren saknar namn");
                }
                AddElementNL(contactPersonNode, "FirstName", user.Name.Trim());
                AddElementNL(contactPersonNode, "LastName", user.Name.Trim());
            }

            var telephone = contact.ContactECom.FirstOrDefault(x => x.SysContactEComTypeId == (int)TermGroup_SysContactEComType.PhoneJob);
            if (telephone != null)
            {
                AddElementNL(contactNode, "Telephone", telephone.Text);
            }

            var email = contact.ContactECom.FirstOrDefault(x => x.SysContactEComTypeId == (int)TermGroup_SysContactEComType.Email);
            if (email != null)
            {
                AddElementNL(contactNode, "Email", email.Text);
            }
        }

        private void AddContactAddress(XElement parent, Contact contact, string logInfo)
        {
            var adresses = contact.ContactAddress.Where(x=> x.SysContactAddressTypeId == (int)TermGroup_SysContactAddressType.Visiting).ToList();
            if (!adresses.Any())
            {
                adresses = contact.ContactAddress.Where(x => x.SysContactAddressTypeId == (int)TermGroup_SysContactAddressType.Distribution).ToList();
                if (!adresses.Any())
                {
                    adresses = contact.ContactAddress.Where(x => x.SysContactAddressTypeId == (int)TermGroup_SysContactAddressType.Delivery).ToList();

                    if (!adresses.Any())
                    {
                        adresses = contact.ContactAddress.Where(x => x.SysContactAddressTypeId == (int)TermGroup_SysContactAddressType.Billing).ToList();
                    }
                }
            }
            
            if (!adresses.Any())
            {
                throw new ActionFailedException(GetText(7737, "Adress måste anges på företaget") + ": " + logInfo);
                /*
                    var address = AddElementNL(parent, "Address");
                    AddElementNL(address, "StreetName", "StreetName");
                    AddElementNL(address, "City", "City");
                    AddElementNL(address, "PostalCode", "PostalCode");
                    AddElementNL(address, "Country", "NO");

                    */
            }


            foreach (var address in adresses)
            {
                var adressType = (TermGroup_SysContactAddressType)address.SysContactAddressTypeId;
                var addressNode = AddElementNL(parent, "Address");

                var city = ContactManager.GetContactAddressRowText(address, TermGroup_SysContactAddressRowType.PostalAddress);
                var postalCode = ContactManager.GetContactAddressRowText(address, TermGroup_SysContactAddressRowType.PostalCode);
                var street = ContactManager.GetContactAddressRowText(address, TermGroup_SysContactAddressRowType.StreetAddress);

                if (string.IsNullOrEmpty(street))
                {
                    street = ContactManager.GetContactAddressRowText(address, TermGroup_SysContactAddressRowType.Address);
                }

                if (!string.IsNullOrEmpty(street))
                {
                    AddElementNL(addressNode, "StreetName", street);
                }

                if (!string.IsNullOrEmpty(city))
                {
                    AddElementNL(addressNode, "City", city);
                }

                if (!string.IsNullOrEmpty(postalCode))
                {
                    AddElementNL(addressNode, "PostalCode", postalCode);
                }

                /*
                if (contact.Actor.ActorType == (int)SoeActorType.Company)
                {
                    AddElementNL(addressNode, "Country", "NO");
                }
                */
                //Field to differentiate between multiple addresses and to indicate the type of address. Choose from the predefined enumerations: 
                //StreetAddress, PostalAddress, BillingAddress, 
                //ShipToAddress, ShipFromAddress.

                switch (adressType)
                {
                    case TermGroup_SysContactAddressType.Billing:
                        AddElementNL(addressNode, "AddressType", "BillingAddress");
                        break;
                    case TermGroup_SysContactAddressType.Delivery:
                        AddElementNL(addressNode, "AddressType", "PostalAddress");
                        break;
                    case TermGroup_SysContactAddressType.Distribution:
                        AddElementNL(addressNode, "AddressType", "ShipToAddress");
                        break;
                }
            }
        }
        private void AddTaxTable(XElement parent)
        {
            var taxNode = AddElementNL(parent, "TaxTable");

            var vatCodes = AccountManager.GetVatCodes(base.ActorCompanyId);

            foreach(var vatCode in vatCodes.Where(v=> v.AccountStd.SysVatAccountId.HasValue))
            {
                var sysVatAccount = GetSysVatAccount(vatCode.AccountStd.SysVatAccountId.Value);

                if (sysVatAccount != null)
                {
                    var entry = AddElementNL(taxNode, "TaxTableEntry");
                    AddElementNL(entry, "TaxType", "MVA");
                    AddElementNL(entry, "Description", "Merverdiavgift");

                    var details = AddElementNL(entry, "TaxCodeDetails");
                    AddElementNL(details, "TaxCode", vatCode.Code);
                    AddElementNL(details, "Description", vatCode.Name);
                    AddElementNL(details, "TaxPercentage", vatCode.Percent.ToString("N0"));
                    AddElementNL(details, "Country", "NO");
                    AddElementNL(details, "StandardTaxCode", sysVatAccount.AccountCode);
                    AddElementNL(details, "BaseRate", "100");
                }
            }
        }

        #endregion

        #region GeneralLedger
        //Always show all accounts that exists in the company’s chart of accounts, inclusive historical accounts. No limitations

        private void AddGeneralLedgerEntries(XElement parent, DateTime fromDate, DateTime toDate)
        {
            var generalLedgerEntriesNode = AddElementNL(parent, "GeneralLedgerEntries");

            decimal totalDebit = 0;
            decimal totalCredit = 0;

            //Grouping mechanism for journals.Please use the examples when appropriate:
            //GL = General Ledger Journals
            //AR = Accounts Receivable Journals
            //AP = Accounts Payable Journals
            //A = Assorted journals
            int supplierInvoiceSeriesTypeId = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.SupplierInvoiceVoucherSeriesType, 0, base.ActorCompanyId, 0);
            int customerInvoiceSeriesTypeId = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.CustomerInvoiceVoucherSeriesType, 0, base.ActorCompanyId, 0);

            var voucherRows = GetVoucherRows(fromDate, toDate, base.ActorCompanyId);

            var supplierInvoiceVouchers = voucherRows.Where(x => x.VoucherSeriesTypeId == supplierInvoiceSeriesTypeId).ToList();
            if (supplierInvoiceVouchers.Any())
            {
                AddJournal(generalLedgerEntriesNode, "AP", supplierInvoiceVouchers, fromDate, toDate, ref totalDebit, ref totalCredit);
            }
            var customerInvoiceVouchers = voucherRows.Where(x => x.VoucherSeriesTypeId == customerInvoiceSeriesTypeId).ToList();
            if (customerInvoiceVouchers.Any())
            {
                AddJournal(generalLedgerEntriesNode, "AR", customerInvoiceVouchers, fromDate, toDate, ref totalDebit, ref totalCredit);
            }
            var generalVouchers = voucherRows.Where(x => (x.VoucherSeriesTypeId != supplierInvoiceSeriesTypeId) && (x.VoucherSeriesTypeId != customerInvoiceSeriesTypeId) ).ToList();
            if (generalVouchers.Any())
            {
                AddJournal(generalLedgerEntriesNode, "GL", generalVouchers, fromDate, toDate, ref totalDebit, ref totalCredit);
            }

            AddAmountElement(generalLedgerEntriesNode, "TotalCredit", totalCredit, true);
            AddAmountElement(generalLedgerEntriesNode, "TotalDebit", totalDebit, true);
            AddElementNL(generalLedgerEntriesNode, "NumberOfEntries", voucherRows.Count.ToString(), true);
        }

        private void AddJournal(XElement parent,string type, List<SAFTTransactionDTO> voucherRows, DateTime fromDate, DateTime toDate, ref decimal totalDebit, ref decimal totalCredit)
        {
            var journal = AddElementNL(null, "Journal");
            AddElementNL(journal, "JournalID", "123ABC");
            AddElementNL(journal, "Description", "Ver register");
            AddElementNL(journal, "Type", type);

            var customerInvoices = GetCustomerinvoices(fromDate, toDate, base.ActorCompanyId);
            var supplierInvoices = GetSupplierinvoices(fromDate, toDate, base.ActorCompanyId);

            foreach (var head in voucherRows.GroupBy(g => g.VoucherHeadId))
            {
                var customerInvoicePerVoucherHead = customerInvoices.Where(i => i.VoucherHeadId == head.Key).ToList();
                var supplierInvoicePerVoucherHead = supplierInvoices.Where(i => i.VoucherHeadId == head.Key).ToList();
                var customerId = customerInvoicePerVoucherHead.Count == 1 ? customerInvoicePerVoucherHead.First().ActorId.ToString() : "";
                var supplierId = supplierInvoicePerVoucherHead.Count == 1 ? supplierInvoicePerVoucherHead.First().SupplierId.ToString() : "";

                AddTransaction(journal, head, customerId, supplierId, ref totalDebit, ref totalCredit);
            }

            parent.Add(journal);
        }

        private void AddTransaction(XElement parent, IGrouping<int, SAFTTransactionDTO> voucherRows, string customerId, string supplierId, ref decimal totalDebit, ref decimal totalCredit)
        {
            var transaction = AddElementNL(parent, "Transaction");

            var first = voucherRows.First();

            AddElementNL(transaction, "TransactionID", first.VoucherNr.ToString());
            AddElementNL(transaction, "Period", first.Date.Month.ToString());
            AddElementNL(transaction, "PeriodYear", first.Date.Year.ToString());
            AddElementNL(transaction, "TransactionDate", DateToString(first.Date));
            AddElementNL(transaction, "TransactionType", "Normal");
            AddElementNL(transaction, "Description", first.VoucherText);
            AddElementNL(transaction, "SystemEntryDate", DateToString(first.CreatedDate ?? first.Date));
            AddElementNL(transaction, "GLPostingDate", DateToString(first.Date));

            foreach (var voucherRow in voucherRows)
            {
                var line = AddElementNL(transaction, "Line", null);
                AddElementNL(line, "RecordID", voucherRow.RowNr.ToString());
                AddElementNL(line, "AccountID", voucherRow.AccountNr);
                
                if (!string.IsNullOrEmpty(customerId))
                {
                    AddElementNL(line, "CustomerID", customerId);
                }
                if (!string.IsNullOrEmpty(supplierId))
                {
                    AddElementNL(line, "SupplierID", supplierId);
                }

                AddElementNL(line, "Description", first.VoucherText);

                AddDebitCreditAmount(line, voucherRow.Amount);
                if (voucherRow.Amount >= 0)
                {
                    totalDebit += voucherRow.Amount;
                }
                else
                {
                    totalCredit += Math.Abs(voucherRow.Amount);
                }

                AddTaxInformation(line, voucherRow);
            }
        }


        #endregion

        #region Helpers


        private string DateToString(DateTime date)
        {
            return date.ToString("yyyy-MM-dd");
        }

        public void AddTaxInformation(XElement parent, SAFTTransactionDTO dto)
        {
            
            var taxInformationElem = AddElementNL(parent, "TaxInformation");
            //AddAmountElement(taxInformationElem, "TaxBase", totalAmount);

            AddElementNL(taxInformationElem, "TaxType", "MVA");
            AddElementNL(taxInformationElem, "TaxCode", dto.AccountCode);
            AddAmountElement(taxInformationElem, "TaxPercentage", dto.VatRate.GetValueOrDefault());
            
            var taxAmountElem = AddElementNL(taxInformationElem, "TaxAmount");
            AddMonetaryElement(taxAmountElem, "Amount", dto.TaxAmount);
        }

        public void AddTaxRegistrationNumber(XElement parent, string number)
        {
            var taxNode = AddElementNL(parent, "TaxRegistration");
            AddElementNL(taxNode, "TaxRegistrationNumber", number);
        }
        
        private void AddDebitCreditAmount(XElement line, decimal amount)
        {
            var debitCredit = new XElement(nl + (amount >= 0 ? "DebitAmount" : "CreditAmount") );
            line.Add(debitCredit);
            AddAmountElement(debitCredit, "Amount", Math.Abs(amount));
        }
        private void AddDebitCreditOpeningBalance(XElement parent, decimal amount)
        {
            AddAmountElement(parent, (amount >= 0 ? "OpeningDebitBalance" : "OpeningCreditBalance"), amount);
        }
        private void AddDebitCreditClosingBalance(XElement parent, decimal amount)
        {
            AddAmountElement(parent, (amount >= 0 ? "ClosingDebitBalance" : "ClosingCreditBalance"), amount);
        }

        private XElement AddAmountElement(XElement parent, string tag, decimal amount, bool addFirst=false)
        {
            return AddElementNL(parent, tag, amount.ToString("0.00##", CultureInfo.InvariantCulture), addFirst);
        }

        private XElement AddMonetaryElement(XElement parent, string tag, decimal amount, bool addFirst = false)
        {
            return AddElementNL(parent, tag, amount.ToString("0.00", CultureInfo.InvariantCulture), addFirst);
        }

        private XElement AddElementNL(XElement parent, string newTag, string value = null, bool addFirst = false)
        {
            var elem = new XElement(nl + newTag, value);
            if (parent != null)
            {
                if (addFirst)
                    parent.AddFirst(elem);
                else
                    parent.Add(elem);
            }
            
            return elem;
        }

        private SysVatAccount GetSysVatAccount(int sysVatAccountId)
        {
            return SysDbCache.Instance.SysVatAccounts.FirstOrDefault(x=> x.SysVatAccountId == sysVatAccountId);
        }

        private SysVatRate GetSysVatRate(int sysVatAccountId)
        {
            return SysDbCache.Instance.SysVatRates.FirstOrDefault(x => x.SysVatAccountId == sysVatAccountId && x.IsActive == 1);
        }

        private List<SAFTTransactionDTO> GetVoucherRows(DateTime fromDate, DateTime toDate, int actorCompanyId)
        {
            var result = new List<SAFTTransactionDTO>();

            var voucherHeads = VoucherManager.GetVoucherHeadDTOs(actorCompanyId, fromDate, toDate);

            var customerInvoices = GetCustomerinvoices(fromDate, toDate, actorCompanyId);
            var supplierInvoices = GetSupplierinvoices(fromDate, toDate, actorCompanyId);

            foreach (var head in voucherHeads)
            {
                var supplierNr = "";
                var customerNr = "";
                var actorName = "";

                var customerInvoicesPerVoucherHead = customerInvoices.Where(i => i.VoucherHeadId == head.VoucherHeadId).ToList();
                var supplierInvoicePerVoucherHead = supplierInvoices.Where(i => i.VoucherHeadId == head.VoucherHeadId).ToList();
                if (customerInvoicesPerVoucherHead.Count == 1)
                {
                    var customer = customerInvoicesPerVoucherHead.First();
                    customerNr = customer.CustomerNr;
                    actorName = customer.CustomerName;
                }

                if (supplierInvoicePerVoucherHead.Count == 1)
                {
                    var supplier = supplierInvoicePerVoucherHead.First();
                    supplierNr = supplier.SupplierNr;
                    actorName = supplier.SupplierName;
                }

                foreach (var row in head.Rows)
                {
                    var dto = new SAFTTransactionDTO
                    {
                        VoucherNr = row.VoucherNr,
                        AccountNr = row.Dim1Nr,
                        AccountName = row.Dim1Name,
                        Date = head.Date,
                        VoucherText = row.Text,
                        DebetAmount = row.Amount >= 0 ? row.Amount : 0,
                        CreditAmount = row.Amount < 0 ? Math.Abs(row.Amount) : 0,
                        Amount = row.Amount,
                        VoucherSeriesTypeId = head.VoucherSeriesTypeId,
                        VoucherHeadId = row.VoucherHeadId,
                        RowNr = row.RowNr ?? 0,
                        CreatedDate = head.Created,
                    };

                    if (!string.IsNullOrEmpty(customerNr))
                    {
                        dto.CustomerId = customerNr;
                    }

                    if (!string.IsNullOrEmpty(supplierNr))
                    {
                        dto.SupplierId = supplierNr;
                    }

                    dto.SupplierCustomerName = actorName;


                    if (row.SysVatAccountId.GetValueOrDefault() > 0 && (row.Dim1AccountType == (int)TermGroup_AccountType.Income || row.Dim1AccountType == (int)TermGroup_AccountType.Cost))
                    {
                        var sysVatAccount = GetSysVatAccount(row.SysVatAccountId.Value);
                        var sysVatRate = GetSysVatRate(row.SysVatAccountId.Value);
                        var vatRate = sysVatRate?.VatRate ?? 0M;
                        dto.TaxAmount = vatRate > 0 ? row.Amount * (vatRate / 100) : 0;
                        dto.VatRate = vatRate;
                        dto.VatCode = sysVatAccount.VatNr1.ToString();
                        dto.AccountCode =  sysVatAccount.AccountCode;
                    }

                    result.Add(dto);
                }
            }

            return result;
        }

        private List<CustomerInvoiceActorDTO> GetCustomerinvoices(DateTime fromDate, DateTime toDate, int actorCompanyId)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            entitiesReadOnly.Invoice.NoTracking();
            return (from i in entitiesReadOnly.Invoice.OfType<CustomerInvoice>()
                                    where //(vr.VoucherHead.Date >= from) && 
                                          (i.VoucherHead.Date >= fromDate) &&
                                          (i.VoucherHead.Date <= toDate) &&
                                          i.VoucherHead.ActorCompanyId == actorCompanyId &&
                                          (i.Origin.Type == (int)SoeOriginType.CustomerInvoice)
                                    select i)
                                        .Select(EntityExtensions.GetCustomerInvoiceActorDTO)
                                        .ToList();
        }

        private List<SupplierInvoiceSmallDTO> GetSupplierinvoices(DateTime fromDate, DateTime toDate, int actorCompanyId)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            return (from i in entitiesReadOnly.Invoice.OfType<SupplierInvoice>()
                    where //(vr.VoucherHead.Date >= from) && 
                          (i.VoucherHead.Date >= fromDate) &&
                          (i.VoucherHead.Date <= toDate) &&
                          i.VoucherHead.ActorCompanyId == actorCompanyId &&
                          (i.Origin.Type == (int)SoeOriginType.SupplierInvoice)
                    select i)
                          .Select(EntityExtensions.GetSupplierInvoiceSmallDTO)
                          .ToList();
        }

        public List<SAFTActorBalanceDTO> GetInvoiceBalances(DateTime fromDate, DateTime toDate, int actorCompanyId, SoeOriginType invoiceType)
        {
            var result = new List<SAFTActorBalanceDTO>();

            var paymentType = invoiceType == SoeOriginType.SupplierInvoice ? SoeOriginType.SupplierPayment : SoeOriginType.CustomerPayment;
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            var data = entitiesReadOnly.GetReportBalanceListItems(actorCompanyId, (int)invoiceType, (int)paymentType).ToList();
            //Remove non relevant data..
            data = data.Where(x => ( !(x.PayDate < fromDate && x.FullyPayed) ) && x.InvoiceDate <= toDate && !x.OnlyPayment).ToList();

            foreach(var customerGroup in data.GroupBy(x=> x.ActorId)) 
            { 
                var first = customerGroup.First();
                var startBalance = customerGroup.Where(x => x.InvoiceDate <= fromDate && (!x.PayDate.HasValue || x.PayDate >= fromDate) ).Sum(s => s.InvoiceTotalAmount);
                var endBalance = customerGroup.Where(x => !x.PayDate.HasValue || x.PayDate >= toDate).Sum(s => s.InvoiceTotalAmount);

                var balance = new SAFTActorBalanceDTO
                {
                    ActorId = first.ActorId.GetValueOrDefault(),
                    StartBalance = startBalance,
                    EndBalance = endBalance
                };

                result.Add(balance);
            }

            return result;
        }

        #endregion


    }
}
