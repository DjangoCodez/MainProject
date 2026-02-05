using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Linq;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Util;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Business.Core.PaymentIO.BgMax;

namespace SoftOne.Soe.Business.Core
{
    public class ImportExportCacheManager : ManagerBase
    {
        #region Enums

        protected enum ImportType
        {
            Unknown = 0,
            Customer = 1,
            Supplier = 2,
            ContactPerson = 3,
            Product = 4,
            Employee = 5,
            CustomerGroup = 6,
            ProductGroup = 7,
            PayrollStartValue = 8,
            Accounts = 9,
            TaxDeductionContacts = 10,
            Pricelists = 11,
            Agreements = 12,
        }

        protected enum TerminationMessageType
        {
            FieldMissing = 1,
        }

        protected enum ErrorMessageType
        {
            UnkownError = 1,
            MandatoryColumnMissing = 2,
            MandatoryColumnInvalid = 3,
            SaveFailed = 4,
        }

        protected enum WarningMessageType
        {
            AddContactEcomFailed = 1,
            AddContactAddressesFailed = 2,
            AddPaymentInformationFailed = 3,
            AddCategoriesFailed = 4,
            AddClosestRelativeFailed = 5,
            AddEmployeeVacationSEFailed = 6,
            AddEmployeeAccountsFailed = 7,

            SaveContactFailed = 11,
            SaveContactEComFailed = 12,
            SaveContactAddressesFailed = 13,
            SavePaymentInformationFailed = 14,
            SaveCategoriesFailed = 15,
            SaveAccountsFailed = 16,
            SaveUserCompanyRoleFailed = 17,
            SaveAttestRolesFailed = 18,

            OptionalFieldInvalid = 21,
            MandatoryFieldInvalid = 22,

            SaveUserFailedUserAlreadyExists = 31,

            SaveEmploymentPriceTypeFailed = 41,
            SaveEmployeeFactorsFailed = 42,
            SaveEmployeeEcomFailed = 43,
            SaveEmployeeClosestRelativeFailed = 44,
            SaveEmployeeAddressesFailed = 45,
            SaveEmployeeCategoriesPrimarly = 46,
            SaveEmployeeCategoriesSecondary = 47,
            SaveEmploymeePositionFailed = 48,
            SaveEmployeeVacationSEFailed = 49,
            SaveEmployeeAccountsFailed = 50,

            CustomerMissing = 61,
        }

        #endregion

        #region Variables

        // Create a logger for use in this class
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        //Current values
        protected Company company;
        protected int langId;
        protected int baseCurrencyId = 0;
        protected int rowNr;
        protected int itemsAdded;
        protected int itemsUpdated;

        protected List<ImportExportConflictItem> conflicts;

        #endregion

        #region Constructor

        public ImportExportCacheManager(ParameterObject parameterObject) : base(parameterObject) { }

        #endregion

        #region Cache

        //Dictionary's
        protected Dictionary<string, int> sysCurrencyIdDictCache = new Dictionary<string, int>();
        protected Dictionary<string, int> paymentConditionIdDictCache = new Dictionary<string, int>();
        protected Dictionary<string, int> vatCodeIdDictCache = new Dictionary<string, int>();
        protected Dictionary<string, int> deliveryMethodIdDictCache = new Dictionary<string, int>();
        protected Dictionary<string, int> deliveryConditionIdDictCache = new Dictionary<string, int>();
        protected Dictionary<string, int> priceListTypeIdDictCache = new Dictionary<string, int>();
        protected Dictionary<string, int> timeDeviationCauseIdDictCache = new Dictionary<string, int>();
        protected Dictionary<string, int> timeCodeIdDictCache = new Dictionary<string, int>();
        protected Dictionary<int, int> billingInvoiceReportIdDictCache = new Dictionary<int, int>();
        protected Dictionary<int, int> billingOfferReportIdDictCache = new Dictionary<int, int>();
        protected Dictionary<int, int> billingOrderReportIdDictCache = new Dictionary<int, int>();
        protected Dictionary<int, int> billingAgreementReportIdDictCache = new Dictionary<int, int>();
        protected Dictionary<string, AccountStd> accountStdDictCache = new Dictionary<string, AccountStd>();
        protected Dictionary<string, AccountInternal> accountInternalDictCache = new Dictionary<string, AccountInternal>();
        protected Dictionary<string, Category> categoryDictCache = new Dictionary<string, Category>();
        protected Dictionary<string, EmployeeGroup> employeeGroupDictCache = new Dictionary<string, EmployeeGroup>();
        protected Dictionary<string, PayrollGroup> payrollGroupDictCache = new Dictionary<string, PayrollGroup>();
        protected Dictionary<string, VacationGroup> vacationGroupDictCache = new Dictionary<string, VacationGroup>();
        protected Dictionary<string, ProductGroup> productGroupDictCache = new Dictionary<string, ProductGroup>();
        protected Dictionary<string, ProductUnit> productUnitDictCache = new Dictionary<string, ProductUnit>();
        protected Dictionary<string, PayrollPriceType> payrollPriceTypeDictCache = new Dictionary<string, PayrollPriceType>();
        protected Dictionary<string, PayrollLevel> payrollLevelDictCache = new Dictionary<string, PayrollLevel>();
        protected Dictionary<string, PriceListType> priceListTypeDictCache = new Dictionary<string, PriceListType>();
        protected Dictionary<string, Position> positionDict = new Dictionary<string, Position>();
        protected Dictionary<string, Supplier> supplierDictCache = new Dictionary<string, Supplier>();
        protected Dictionary<int, PaymentInformationRow> paymentInformationRowDictCache = new Dictionary<int, PaymentInformationRow>();

        protected List<Company> companiesCache = null;
        protected List<Role> rolesCache = null;
        protected List<AttestRole> attestRolesCache = null;
        private List<Account> companyAccounts = null;

        #endregion

        #region Core

        protected void AddWarning(WarningMessageType errorMessageType, string columnName = "", string identifier = "")
        {
            string message = GetText(5623, "Varning");
            message += ". ";

            switch (errorMessageType)
            {
                case WarningMessageType.AddContactEcomFailed:
                    message += GetText(5626, "Tele/webb-uppgifter kunde inte tolkas, information saknas");
                    break;
                case WarningMessageType.AddContactAddressesFailed:
                    message += GetText(5627, "Addressuppgifter kunde inte tolkas, information saknas");
                    break;
                case WarningMessageType.AddPaymentInformationFailed:
                    message += GetText(5628, "Betalningsinformation kunde inte tolkas, information saknas");
                    break;
                case WarningMessageType.AddCategoriesFailed:
                    message += GetText(5629, "Kategorier kunde inte tolkas, information saknas");
                    break;

                case WarningMessageType.SaveContactFailed:
                    message += GetText(5630, "Kontaktuppgifter kunde inte sparas");
                    break;
                case WarningMessageType.SaveContactEComFailed:
                    message += GetText(5631, "Tele/webb-uppgifter kunde inte sparas");
                    break;
                case WarningMessageType.SaveContactAddressesFailed:
                    message += GetText(5632, "Addressuppgifter kunde inte sparas");
                    break;
                case WarningMessageType.SavePaymentInformationFailed:
                    message += GetText(5633, "Betalningsinformation kunde inte sparas");
                    break;
                case WarningMessageType.SaveCategoriesFailed:
                    message += GetText(5634, "Kategorier kunde inte sparas");
                    break;
                case WarningMessageType.SaveAccountsFailed:
                    message += GetText(5635, "Kontoinformation kunde inte sparas");
                    break;
                case WarningMessageType.SaveUserCompanyRoleFailed:
                    message += GetText(5636, "Roller för användaren kunde inte sparas");
                    break;
                case WarningMessageType.SaveAttestRolesFailed:
                    message += GetText(5637, "Attestroller kunde inte sparas");
                    break;
                case WarningMessageType.SaveEmployeeAccountsFailed:
                    message += GetText(11991, "Tillhörigheter kunde inte sparas");
                    break;

                case WarningMessageType.OptionalFieldInvalid:
                    message += GetText(5642, "Frivillig kolumn har ogilltigt värde");
                    break;
                case WarningMessageType.MandatoryFieldInvalid:
                    message += GetText(10029, "Obligatorisk kolumn saknar eller har ogilltigt värde");
                    break;

                case WarningMessageType.SaveUserFailedUserAlreadyExists:
                    message += GetText(10030, "Användarnamnet finns redan, den anställde kan ej kopplas mot användare");
                    break;

                case WarningMessageType.SaveEmploymentPriceTypeFailed:
                    message += GetText(11758, "Lönetyper kunde inte sparas");
                    break;
                case WarningMessageType.SaveEmployeeFactorsFailed:
                    message += GetText(11759, "Faktorer kunde inte sparas");
                    break;
                case WarningMessageType.SaveEmploymeePositionFailed:
                    message += GetText(11762, "Befattning kunde inte sparas");
                    break;
                case WarningMessageType.SaveEmployeeEcomFailed:
                    message += GetText(11763, "Tele/webb-uppgifter kunde inte sparas");
                    break;
                case WarningMessageType.SaveEmployeeClosestRelativeFailed:
                    message += GetText(11764, "Närmast anhörig kunde inte sparas");
                    break;
                case WarningMessageType.SaveEmployeeAddressesFailed:
                    message += GetText(11765, "Adresser kunde inte sparas");
                    break;
                case WarningMessageType.SaveEmployeeCategoriesPrimarly:
                    message += GetText(11766, "Primära kategories kunde inte sparas");
                    break;
                case WarningMessageType.SaveEmployeeCategoriesSecondary:
                    message += GetText(11767, "Sekundära kategorier kunde inte sparas");
                    break;
                case WarningMessageType.SaveEmployeeVacationSEFailed:
                    message += GetText(10121, "Semesterdagar kunde inte sparas");
                    break;
                case WarningMessageType.CustomerMissing:
                    message += GetText(8292, "Kund kunde inte hittas");
                    break;
            }

            //Add
            AddConflict(message, columnName, identifier);
        }

        protected void AddError(ErrorMessageType errorMessageType, string columnName = "", string identifier = "", Exception exception = null)
        {
            string message = GetText(5624, "Rad ej inläst");
            message += ". ";

            switch (errorMessageType)
            {
                case ErrorMessageType.UnkownError:
                    message += GetText(5638, "Okänt fel uppstod");
                    break;
                case ErrorMessageType.MandatoryColumnMissing:
                    message += GetText(5639, "Obligatorisk kolumn saknas");
                    break;
                case ErrorMessageType.MandatoryColumnInvalid:
                    message += GetText(5640, "Obligatorisk kolumn har ogilltigt värde");
                    break;
                case ErrorMessageType.SaveFailed:
                    message += GetText(5641, "Misslyckades med att spara");
                    break;
            }

            //Add
            AddConflict(message, columnName, identifier);

            //Log
            if (exception != null)
                base.LogError(exception, this.log);
        }

        protected void AddTerminationError(TerminationMessageType terminationMessageType, string columnName = "", string identifier = "", Exception exception = null)
        {
            string message = GetText(5625, "Import avbruten");
            message += ". ";

            switch (terminationMessageType)
            {
                case TerminationMessageType.FieldMissing:
                    message += GetText(8070, "En nödvändig kolumn saknas");
                    break;
            }

            //Add
            AddConflict(message, columnName, identifier);

            //Log
            if (exception != null)
                base.LogError(exception, this.log);
        }

        protected void AddConflict(string message, string columnName, string identifier = "")
        {
            if (this.conflicts == null)
                this.conflicts = new List<ImportExportConflictItem>();

            this.conflicts.Add(new ImportExportConflictItem()
            {
                RowNr = this.rowNr,
                Field = columnName,
                Message = message.NullToEmpty(),
                Identifier = identifier.NullToEmpty(),
            });
        }

        protected bool HasErrors()
        {
            return this.conflicts != null && this.conflicts.Count > 0;
        }

        protected string GetImportSuccededMessage(ImportType importType)
        {
            string message = "";

            switch (importType)
            {
                case ImportType.Customer:
                    message = GetText(8074, "Kundinformation inläst");
                    break;
                case ImportType.Supplier:
                    message = GetText(8072, "Leverantörsinformation inläst");
                    break;
                case ImportType.ContactPerson:
                    message = GetText(8073, "Kontaktpersoner inlästa");
                    break;
                case ImportType.Product:
                    message = GetText(8075, "Artikelinformation inläst");
                    break;
                case ImportType.Employee:
                    message = GetText(5574, "Anställda inlästa");
                    break;
                case ImportType.CustomerGroup:
                    message = GetText(4598, "KundKategorier inlästa");
                    break;
                case ImportType.ProductGroup:
                    message = GetText(4599, "ArtikelGrupper inlästa");
                    break;
                case ImportType.PayrollStartValue:
                    message = GetText(10057, "Startvärden för lön inlästa");
                    break;
                case ImportType.Accounts:
                    message = GetText(4744, "Konton inlästa");
                    break;
                case ImportType.Pricelists:
                    message = GetText(7732, "Prislistor inlästa");
                    break;
                case ImportType.Agreements:
                    message = GetText(7778, "Avtals inlästa");
                    break;
            }
            message += GetItemsAddedUpdatedInformation();

            return message;
        }

        protected string GetImportFailedMessage()
        {
            string message = "";

            message = GetText(8071, "Import misslyckades");
            message += GetItemsAddedUpdatedInformation();

            return message;
        }

        protected string GetItemsAddedUpdatedInformation()
        {
            return String.Format(GetText(5622, ". {0} tillagda. {1} uppdaterade"), itemsAdded, itemsUpdated);
        }

        protected bool IsOkToUpdateValue(bool doNotModifyWithEmpty, object column = null)
        {
            bool doNotUpdate = !StringUtility.HasValue(column) && doNotModifyWithEmpty;
            return !doNotUpdate;
        }

        #endregion

        #region Get common properties

        protected int GetLangId(object obj, int defaultValue = 1)
        {
            if (!StringUtility.HasValue(obj))
                return defaultValue;

            int lang;
            if (Int32.TryParse(obj.ToString(), out lang))
            {
                //Numeric
                if (!Enum.IsDefined(typeof(TermGroup_Languages), lang))
                    lang = defaultValue;
            }
            else
            {
                //String
                var sysLanguage = LanguageManager.GetSysLanguage(obj.ToString());
                if (sysLanguage != null)
                    lang = sysLanguage.SysLanguageId;
                else
                    lang = defaultValue;
            }

            return lang;
        }

        protected int GetBaseCurrencyId(CompEntities entities)
        {
            if (baseCurrencyId == 0)
            {
                //Get from db
                var id = CountryCurrencyManager.GetCompanyBaseCurrency(entities, this.company.ActorCompanyId)?.CurrencyId;

                //Insert to cache
                if (id.GetValueOrDefault() > 0)
                    baseCurrencyId = id.GetValueOrDefault();
            }

            return baseCurrencyId;
        }

        #endregion

        #region Get entity

        private bool TryGetColumnKey(object column, out string key)
        {
            key = StringUtility.HasValue(column) ? column.ToString().ToLower() : null;
            return key != null;
        }

        protected AccountStd GetAccountStd(CompEntities entities, object column)
        {
            if (!TryGetColumnKey(column, out string key))
                return null;
            if (this.accountStdDictCache.ContainsKey(key))
                return this.accountStdDictCache[key];

            AccountStd entity = entities.AccountStd.FirstOrDefault(e => e.Account.ActorCompanyId == this.company.ActorCompanyId && e.Account.AccountNr.ToLower() == key);
            if (entity != null)
                this.accountStdDictCache.Add(key, entity);
            return entity;
        }

        protected AccountInternal GetAccountInternal(CompEntities entities, object column, bool useExternalCode = false, bool useAccountNr = false)
        {
            if (!TryGetColumnKey(column, out string key))
                return null;
            if (this.accountInternalDictCache.ContainsKey(key))
                return this.accountInternalDictCache[key];

            AccountInternal entity = null;
            if (useExternalCode || useAccountNr)
            {
                if (useExternalCode)
                {
                    entity = (from e in entities.AccountInternal
                                .Include("Account")
                              where e.Account.ActorCompanyId == this.company.ActorCompanyId &&
                              e.Account.ExternalCode != null &&
                              e.Account.ExternalCode.ToLower() == key
                              select e).FirstOrDefault();
                }
                if (useAccountNr && entity == null)
                {
                    entity = (from e in entities.AccountInternal
                                .Include("Account")
                              where e.Account.ActorCompanyId == this.company.ActorCompanyId &&
                              e.Account.AccountNr.ToLower() == key
                              select e).FirstOrDefault();
                }
            }
            else
            {
                entity = (from e in entities.AccountInternal
                            .Include("Account.AccountDim")
                          where e.Account.ActorCompanyId == this.company.ActorCompanyId &&
                          e.Account.Name.ToLower() == key
                          select e).FirstOrDefault();
            }

            if (entity != null)
                this.accountInternalDictCache.Add(key, entity);

            return entity;
        }

        protected AccountMapping GetAccountMapping(int accountId, List<AccountDim> accountDims, int accountDimNr, string dimDefault, int accountDimMandatoryLevel, List<AccountInternal> accountInternals)
        {
            AccountDim accountDim = accountDimNr > 0 ? accountDims.FirstOrDefault(i => i.AccountDimNr == accountDimNr) : null;
            if (accountDim == null)
                return null;

            string defaultAccountNr = StringUtility.GetStringValue(dimDefault);
            AccountInternal accountInternal = accountInternals.FirstOrDefault(a => a.Account.AccountNr == defaultAccountNr && a.Account.AccountDimId == accountDim.AccountDimId);
            int? defaultAccountId = accountInternal?.AccountId.ToNullable();
            int mandatoryLevel = StringUtility.GetInt(accountDimMandatoryLevel, 0);

            AccountMapping accountMapping = new AccountMapping()
            {
                AccountId = accountId,
                AccountDimId = accountDim.AccountDimId,
                DefaultAccountId = defaultAccountId,
                MandatoryLevel = mandatoryLevel,
            };

            return accountMapping;
        }

        private AttestRole GetAttestRole(CompEntities entities, string column)
        {
            if (!TryGetColumnKey(column, out string key))
                return null;

            if (this.attestRolesCache == null)
                this.attestRolesCache = AttestManager.GetAttestRoles(entities, this.company.ActorCompanyId);
            return this.attestRolesCache.FirstOrDefault(e => e.Name != null && e.Name.ToLower() == key);
        }

        protected Category GetCategory(CompEntities entities, object column)
        {
            if (!TryGetColumnKey(column, out string key))
                return null;
            if (this.categoryDictCache.ContainsKey(key))
                return this.categoryDictCache[key];

            Category entity = entities.Category.FirstOrDefault(e => e.Company.ActorCompanyId == this.company.ActorCompanyId && e.Code.ToLower() == key);
            if (entity != null)
                this.categoryDictCache.Add(key, entity);
            return entity;
        }

        protected Category GetCategory(CompEntities entities, object column, SoeCategoryType categoryType)
        {
            if (!TryGetColumnKey(column, out string key))
                return null;

            Category entity = entities.Category.FirstOrDefault(e => e.Company.ActorCompanyId == this.company.ActorCompanyId && e.Code.ToLower() == key && e.Type == (int)categoryType);

            if (entity != null)
                this.categoryDictCache.Add(key, entity);
            return entity;
        }

        protected EmployeeGroup GetEmployeeGroup(CompEntities entities, object column)
        {
            if (!TryGetColumnKey(column, out string key))
                return null;
            if (this.employeeGroupDictCache.ContainsKey(key))
                return this.employeeGroupDictCache[key];

            EmployeeGroup entity = entities.EmployeeGroup.FirstOrDefault(e => e.Company.ActorCompanyId == this.company.ActorCompanyId && e.Name.ToLower() == key);
            if (entity != null)
                this.employeeGroupDictCache.Add(key, entity);
            return entity;
        }

        protected PayrollGroup GetPayrollGroup(CompEntities entities, object column)
        {
            if (!TryGetColumnKey(column, out string key))
                return null;
            if (this.payrollGroupDictCache.ContainsKey(key))
                return this.payrollGroupDictCache[key];

            PayrollGroup entity = entities.PayrollGroup.FirstOrDefault(e => e.Company.ActorCompanyId == this.company.ActorCompanyId && e.Name.ToLower() == key);
            if (entity != null)
                this.payrollGroupDictCache.Add(key, entity);
            return entity;
        }

        protected VacationGroup GetVacationGroup(CompEntities entities, object column)
        {
            if (!TryGetColumnKey(column, out string key))
                return null;
            if (this.vacationGroupDictCache.ContainsKey(key))
                return this.vacationGroupDictCache[key];

            VacationGroup entity = entities.VacationGroup.FirstOrDefault(e => e.Company.ActorCompanyId == this.company.ActorCompanyId && e.Name.ToLower() == key);
            if (entity != null)
                this.vacationGroupDictCache.Add(key, entity);
            return entity;
        }

        protected Supplier GetSupplier(CompEntities entities, int actorCompanyId, string column)
        {
            if (!TryGetColumnKey(column, out string key))
                return null;
            if (this.supplierDictCache.ContainsKey(key))
                return this.supplierDictCache[key];

            Supplier supplier = SupplierManager.GetSupplierBySupplierNr(entities, actorCompanyId, key, false);
            if (supplier != null)
                this.supplierDictCache.Add(key, supplier);
            return supplier;
        }

        protected PaymentInformationRow GetDefaultPaymentInformationRow(CompEntities entities, object column)
        {
            if (!TryGetColumnKey(column, out string key))
                return null;
            if (!Int32.TryParse(key, out int actorId) || actorId == 0)
                return null;
            if (this.paymentInformationRowDictCache.ContainsKey(actorId))
                return this.paymentInformationRowDictCache[actorId];

            PaymentInformationRow paymentInformationRow = PaymentManager.GetDefaultPaymentInformationRow(entities, actorId);
            if (paymentInformationRow != null)
                this.paymentInformationRowDictCache.Add(actorId, paymentInformationRow);
            return paymentInformationRow;
        }

        protected PayrollPriceType GetPayrollPriceType(CompEntities entities, object column)
        {
            if (!TryGetColumnKey(column, out string key))
                return null;
            if (this.payrollPriceTypeDictCache.ContainsKey(key))
                return this.payrollPriceTypeDictCache[key];

            PayrollPriceType entity = entities.PayrollPriceType.FirstOrDefault(e => e.Company.ActorCompanyId == this.company.ActorCompanyId && e.Code != null && e.Code.ToLower() == key);
            if (entity != null)
                this.payrollPriceTypeDictCache.Add(key, entity);
            return entity;
        }

        protected PayrollLevel GetPayrollLevel(CompEntities entities, object column)
        {
            if (!TryGetColumnKey(column, out string key))
                return null;
            if (this.payrollLevelDictCache.ContainsKey(key))
                return this.payrollLevelDictCache[key];

            PayrollLevel entity = entities.PayrollLevel.FirstOrDefault(e => e.ActorCompanyId == this.company.ActorCompanyId && e.Code != null && e.Code.ToLower() == key);
            if (entity != null)
                this.payrollLevelDictCache.Add(key, entity);
            return entity;
        }

        protected PriceListType GetPriceListType(CompEntities entities, object column)
        {
            if (!TryGetColumnKey(column, out string key))
                return null;
            if (priceListTypeDictCache.ContainsKey(key))
                return priceListTypeDictCache[key];

            PriceListType priceListType = entities.PriceListType.FirstOrDefault(e => e.Company.ActorCompanyId == this.company.ActorCompanyId && e.Name.ToLower() == key);
            if (priceListType != null)
                priceListTypeDictCache.Add(key, priceListType);
            return priceListType;
        }

        protected Position GetPosition(CompEntities entities, object column)
        {
            if (!TryGetColumnKey(column, out string key))
                return null;
            if (this.positionDict.ContainsKey(key))
                return this.positionDict[key];

            Position entity = entities.Position.FirstOrDefault(e => e.ActorCompanyId == this.company.ActorCompanyId && e.Code.ToLower() == key);
            if (entity != null)
                positionDict.Add(key, entity);
            return entity;
        }

        #endregion

        #region Nullable FK

        protected List<Account> GetCompanyAccountsWithAccountDim(CompEntities entities, int actorCompanyId)
        {
            if (companyAccounts == null)
            {
                //Get from db
                companyAccounts = AccountManager.GetAccountsByCompany(entities, actorCompanyId, loadAccountDim: true);
            }

            return companyAccounts;
        }

        protected int? GetDefaultActorCompanyId(CompEntities entities, object column, int licenseId, int? originalId, bool doNotModifyWithEmpty = false)
        {
            int? id = null;

            if (StringUtility.HasValue(column))
            {
                if (companiesCache == null)
                {
                    //Get from db
                    companiesCache = (from c in entities.Company
                                      where c.LicenseId == licenseId
                                      select c).ToList();
                }

                Company defaultCompany = null;

                //Get by name
                if (StringUtility.HasValue(column))
                {
                    string value = column.ToString().ToLower();
                    defaultCompany = (from c in companiesCache
                                      where c.LicenseId == licenseId &&
                                      c.Name.ToLower() == value
                                      select c).FirstOrDefault();
                }

                //Get first if only one exists
                if (defaultCompany == null && companiesCache.Count == 1)
                    defaultCompany = companiesCache.First();

                id = defaultCompany?.ActorCompanyId;
            }
            else
            {
                if (doNotModifyWithEmpty)
                    id = originalId;
            }

            return id;
        }

        protected int? GetSysWholeSellerId(List<SysWholeseller> sysWholesellers, object column, int? originalId, bool doNotModifyWithEmpty = false)
        {
            int? id = null;

            if (StringUtility.HasValue(column))
            {
                string value = column.ToString().ToLower();
                SysWholeseller sysWholeseller = (from ws in sysWholesellers
                                                 where ws.Name.ToLower() == value
                                                 select ws).FirstOrDefault();

                if (sysWholeseller != null)
                    id = sysWholeseller.SysWholesellerId;
                else
                    id = null;
            }
            else
            {
                if (doNotModifyWithEmpty)
                    id = originalId;
            }

            return id;
        }

        protected int? GetTimeDeviationCauseId(CompEntities entities, object column, int? originalId, bool doNotModifyWithEmpty = false)
        {
            int? id = null;

            if (StringUtility.HasValue(column))
            {
                string value = column.ToString().ToLower();
                if (timeDeviationCauseIdDictCache.ContainsKey(value))
                {
                    //Get from cache
                    id = timeDeviationCauseIdDictCache[value];
                }
                else
                {
                    //Get from db
                    id = (from tdc in entities.TimeDeviationCause
                          where tdc.ActorCompanyId == this.company.ActorCompanyId &&
                          tdc.Name.ToLower() == value
                          select tdc.TimeDeviationCauseId).FirstOrDefault<int>();

                    //Insert to cache
                    if (id.HasValue && id.Value > 0)
                        timeDeviationCauseIdDictCache.Add(value, id.Value);
                    else
                        id = null;
                }
            }
            else
            {
                if (doNotModifyWithEmpty)
                    id = originalId;
            }

            return id;
        }

        protected int? GetTimeCodeId(CompEntities entities, object column, int? originalId, bool doNotModifyWithEmpty = false)
        {
            int? id = null;

            if (StringUtility.HasValue(column))
            {
                string value = column.ToString().ToLower();
                if (timeCodeIdDictCache.ContainsKey(value))
                {
                    //Get from cache
                    id = timeCodeIdDictCache[value];
                }
                else
                {
                    //Get from db
                    id = (from tc in entities.TimeCode
                          where tc.Company.ActorCompanyId == this.company.ActorCompanyId &&
                          tc.Code.ToLower() == value
                          select tc.TimeCodeId).FirstOrDefault<int>();

                    //Insert to cache
                    if (id.HasValue && id.Value > 0)
                        timeCodeIdDictCache.Add(value, id.Value);
                    else
                        id = null;
                }
            }
            else
            {
                if (doNotModifyWithEmpty)
                    id = originalId;
            }

            return id;
        }

        protected int? GetBillingInvoiceReportId(CompEntities entities, object column, int? originalId, bool doNotModifyWithEmpty = false)
        {
            int? id = null;

            if (StringUtility.HasValue(column))
            {
                int value;
                if (Int32.TryParse(column.ToString().ToLower(), out value))
                {
                    if (billingInvoiceReportIdDictCache.ContainsKey(value))
                    {
                        //Get from cache
                        id = billingInvoiceReportIdDictCache[value];
                    }
                    else
                    {
                        //Get from db
                        id = ReportManager.GetReportByNr(entities, this.company.ActorCompanyId, value, SoeReportTemplateType.BillingInvoice)?.ReportId;

                        //Insert to cache
                        if (id.HasValue)
                            billingInvoiceReportIdDictCache.Add(value, id.Value);
                    }
                }
            }
            else
            {
                if (doNotModifyWithEmpty)
                    id = originalId;
            }

            return id;
        }

        protected int? GetBillingOfferReportId(CompEntities entities, object column, int? originalId, bool doNotModifyWithEmpty = false)
        {
            int? id = null;

            if (StringUtility.HasValue(column))
            {
                int value;
                if (Int32.TryParse(column.ToString().ToLower(), out value))
                {
                    if (billingOfferReportIdDictCache.ContainsKey(value))
                    {
                        //Get from cache
                        id = billingOfferReportIdDictCache[value];
                    }
                    else
                    {
                        //Get from db
                        id = ReportManager.GetReportByNr(entities, this.company.ActorCompanyId, value, SoeReportTemplateType.BillingOffer)?.ReportId;

                        //Insert to cache
                        if (id.HasValue)
                            billingOfferReportIdDictCache.Add(value, id.Value);
                    }
                }
            }
            else
            {
                if (doNotModifyWithEmpty)
                    id = originalId;
            }

            return id;
        }

        protected int? GetBillingOrderReportId(CompEntities entities, object column, int? originalId, bool doNotModifyWithEmpty = false)
        {
            int? id = null;

            if (StringUtility.HasValue(column))
            {
                int value;
                if (Int32.TryParse(column.ToString().ToLower(), out value))
                {
                    if (billingOrderReportIdDictCache.ContainsKey(value))
                    {
                        //Get from cache
                        id = billingOrderReportIdDictCache[value];
                    }
                    else
                    {
                        //Get from db
                        id = ReportManager.GetReportByNr(entities, this.company.ActorCompanyId, value, SoeReportTemplateType.BillingOrder)?.ReportId;

                        //Insert to cache
                        if (id.HasValue)
                            billingOrderReportIdDictCache.Add(value, id.Value);
                    }
                }
            }
            else
            {
                if (doNotModifyWithEmpty)
                    id = originalId;
            }

            return id;
        }

        protected int? GetBillingAgreementReportId(CompEntities entities, object column, int? originalId, bool doNotModifyWithEmpty = false)
        {
            int? id = null;

            if (StringUtility.HasValue(column))
            {
                int value;
                if (Int32.TryParse(column.ToString().ToLower(), out value))
                {
                    if (billingAgreementReportIdDictCache.ContainsKey(value))
                    {
                        //Get from cache
                        id = billingAgreementReportIdDictCache[value];
                    }
                    else
                    {
                        //Get from db
                        id = ReportManager.GetReportByNr(entities, this.company.ActorCompanyId, value, SoeReportTemplateType.BillingContract)?.ReportId;

                        //Insert to cache
                        if (id.HasValue)
                            billingAgreementReportIdDictCache.Add(value, id.Value);
                    }
                }
            }
            else
            {
                if (doNotModifyWithEmpty)
                    id = originalId;
            }

            return id;
        }

        protected int? GetDeliveryTypeId(CompEntities entities, object column, int? originalId)
        {
            int? id = null;

            if (StringUtility.HasValue(column))
            {
                string value = column.ToString().ToLower();
                if (deliveryMethodIdDictCache.ContainsKey(value))
                {
                    //Get from cache
                    id = deliveryMethodIdDictCache[value];
                }
                else
                {
                    //Get from db
                    id = (from dd in entities.DeliveryType
                          where dd.Company.ActorCompanyId == this.company.ActorCompanyId &&
                          dd.Code.ToLower() == value
                          select dd.DeliveryTypeId).FirstOrDefault<int>();

                    //Insert to cache
                    if (id.HasValue && id.Value > 0)
                        deliveryMethodIdDictCache.Add(value, id.Value);
                }
            }
            else
            {
                id = originalId;
            }

            return id.ToNullable();
        }

        protected int? GetDeliveryConditionId(CompEntities entities, object column, int? originalId)
        {
            int? id = null;

            if (StringUtility.HasValue(column))
            {
                string value = column.ToString().ToLower();
                if (deliveryConditionIdDictCache.ContainsKey(value))
                {
                    //Get from cache
                    id = deliveryConditionIdDictCache[value];
                }
                else
                {
                    //Get from db
                    id = (from dc in entities.DeliveryCondition
                          where dc.Company.ActorCompanyId == this.company.ActorCompanyId &&
                          dc.Code.ToLower() == value
                          select dc.DeliveryConditionId).FirstOrDefault<int>();

                    //Insert to cache
                    if (id.HasValue && id.Value > 0)
                        deliveryConditionIdDictCache.Add(value, id.Value);
                }
            }
            else
            {
                id = originalId;
            }

            return id.ToNullable();
        }

        protected int? GetPaymentConditionId(CompEntities entities, object column, int? originalId, bool doNotModifyWithEmpty = false)
        {
            int? id = null;

            if (StringUtility.HasValue(column))
            {
                string value = column.ToString().ToLower();
                if (paymentConditionIdDictCache.ContainsKey(value))
                {
                    //Get from cache
                    id = paymentConditionIdDictCache[value];
                }
                else
                {
                    //Get from db
                    id = (from pc in entities.PaymentCondition
                          where pc.Company.ActorCompanyId == this.company.ActorCompanyId &&
                          pc.Code.ToLower() == value
                          select pc.PaymentConditionId).FirstOrDefault<int>();

                    //Insert to cache
                    if (id.HasValue && id.Value > 0)
                        paymentConditionIdDictCache.Add(value, id.Value);
                }
            }
            else
            {
                id = originalId;
            }

            return id.ToNullable();
        }

        protected int? GetPricelistTypeId(CompEntities entities, object column, int? originalId, bool doNotModifyWithEmpty = false)
        {
            int? id = null;

            if (StringUtility.HasValue(column))
            {
                string value = column.ToString().ToLower();
                if (priceListTypeIdDictCache.ContainsKey(value))
                {
                    //Get from cache
                    id = priceListTypeIdDictCache[value];
                }
                else
                {
                    //Get from db
                    id = (from plt in entities.PriceListType
                          where plt.Company.ActorCompanyId == this.company.ActorCompanyId &&
                          plt.Name.ToLower() == value
                          select plt.PriceListTypeId).FirstOrDefault<int>();

                    //Insert to cache
                    if (id.HasValue && id.Value > 0)
                        priceListTypeIdDictCache.Add(value, id.Value);
                }
            }
            else
            {
                id = originalId;
            }

            return id.ToNullable();
        }

        protected int GetPayingCustomerId(CompEntities entities, object column, int originalId, bool doNotModifyWithEmpty = false)
        {
            int id = 0;

            if (StringUtility.HasValue(column))
            {
                string value = column.ToString();

                Customer payingCustomer = CustomerManager.GetCustomerByNr(entities, this.company.ActorCompanyId, value);
                if (payingCustomer != null)
                    id = payingCustomer.ActorCustomerId;
            }
            else
            {
                id = originalId;
            }

            return id;
        }

        protected int? GetFactoringSupplierId(CompEntities entities, object column, int? originalId, bool doNotModifyWithEmpty = false)
        {
            int? id = null;

            if (StringUtility.HasValue(column))
            {
                string value = column.ToString();

                Supplier factoringSupplier = SupplierManager.GetSupplierBySupplierNr(entities, this.company.ActorCompanyId, value, true);
                if (factoringSupplier != null)
                    id = factoringSupplier.ActorSupplierId;
            }
            else
            {
                id = originalId;
            }

            return id.ToNullable();
        }

        protected int? GetVatCodeId(CompEntities entities, object column, int? originalId, bool doNotModifyWithEmpty = false)
        {
            int? id = null;

            if (StringUtility.HasValue(column))
            {
                string value = column.ToString().ToLower();
                if (vatCodeIdDictCache.ContainsKey(value))
                {
                    //Get from cache
                    id = vatCodeIdDictCache[value];
                }
                else
                {
                    //Get from db
                    id = (from vc in entities.VatCode
                          where vc.Company.ActorCompanyId == this.company.ActorCompanyId &&
                          vc.Code.ToLower() == value &&
                          vc.State == (int)SoeEntityState.Active
                          select vc.VatCodeId).FirstOrDefault<int>();

                    //Insert to cache
                    if (id.HasValue && id.Value > 0)
                        vatCodeIdDictCache.Add(value, id.Value);
                }
            }
            else
            {
                id = originalId;
            }

            return id.ToNullable();
        }

        #endregion

        #region Not nullable FK

        protected int GetIntId(int? newId, int originalId, bool doNotModifyWithEmpty = false)
        {
            int id = 0;

            if (newId.HasValue)
            {
                id = newId.Value;
            }
            else
            {
                if (doNotModifyWithEmpty)
                    id = originalId;
            }

            return id;
        }

        protected int GetCurrencyId(CompEntities entities, object column, bool doNotFallBackOnBaseCurrency = false)
        {
            return GetCurrencyId(entities, column, 0, false, doNotFallBackOnBaseCurrency);
        }

        protected int GetCurrencyId(CompEntities entities, object column, int originalId, bool doNotModifyWithEmpty = false, bool doNotFallBackOnBaseCurrency = false)
        {
            int id = 0;

            if (StringUtility.HasValue(column))
            {
                string value = column.ToString().ToLower();
                if (sysCurrencyIdDictCache.ContainsKey(value))
                {
                    //Get from cache
                    id = sysCurrencyIdDictCache[value];
                }
                else
                {
                    //Get from db
                    var currency = CountryCurrencyManager.HandleCompCurrencies((from c in entities.CompCurrency
                                                                                where c.ActorCompanyId == this.company.ActorCompanyId
                                                                                select c).ToList()).FirstOrDefault(c => c.Code.ToLower() == value);
                    id = currency != null ? currency.CurrencyId : 0;

                    //Insert to cache
                    if (id > 0)
                        sysCurrencyIdDictCache.Add(value, id);
                }
            }
            else
            {
                if (doNotModifyWithEmpty)
                    id = originalId;
            }

            if (id == 0 && !doNotFallBackOnBaseCurrency)
            {
                //Get base currency
                id = GetBaseCurrencyId(entities);
            }

            return id;
        }

        #endregion

        #region Relations

        #region Add relations

        protected bool TryAddSupplierAccountStds(CompEntities entities, Supplier supplier, List<ImportAccount> accounts, bool doNotModifyWithEmpty = false)
        {
            if (supplier == null)
                return false;

            #region Prereq

            if (base.CanEntityLoadReferences(entities, supplier))
            {
                if (!supplier.SupplierAccountStd.IsLoaded)
                    supplier.SupplierAccountStd.Load();

                foreach (SupplierAccountStd supplierAccountStd in supplier.SupplierAccountStd)
                {
                    if (!supplierAccountStd.AccountStdReference.IsLoaded)
                        supplierAccountStd.AccountStdReference.Load();
                    if (!supplierAccountStd.AccountInternal.IsLoaded)
                        supplierAccountStd.AccountInternal.Load();
                }
            }

            List<int> updatedIds = new List<int>();
            List<SupplierAccountStd> supplierAccountStds = supplier.SupplierAccountStd != null ? supplier.SupplierAccountStd.ToList() : new List<SupplierAccountStd>();

            #endregion

            #region Add/Update

            foreach (ImportAccount account in accounts)
            {
                AccountStd accountStd = account.AccountStd;
                List<AccountInternal> accountInternals = account.AccountInternals;
                SupplierAccountStd supplierAccountStd = supplierAccountStds.OrderBy(i => i.SupplierAccountStdId).FirstOrDefault(i => i.Type == account.Type);

                bool allowOnlyAccountInternals = false;
                bool update = supplierAccountStd != null;
                bool valid = IsAccountsValid(accountStd, accountInternals, allowOnlyAccountInternals, update, doNotModifyWithEmpty);
                if (!valid)
                    continue;

                if (supplierAccountStd == null)
                {
                    #region Add

                    supplierAccountStd = new SupplierAccountStd()
                    {
                        Type = account.Type,
                        Percent = (int)account.Percent,

                        //Set references
                        Supplier = supplier,
                    };
                    entities.SupplierAccountStd.AddObject(supplierAccountStd);

                    #endregion
                }
                else
                {
                    #region Update

                    if (accountStd == null && doNotModifyWithEmpty)
                        accountStd = supplierAccountStd.AccountStd;
                    if (accountInternals == null || accountInternals.Count == 0 && doNotModifyWithEmpty)
                        accountInternals = supplierAccountStd.AccountInternal.ToList();

                    updatedIds.Add(supplierAccountStd.SupplierAccountStdId);

                    #endregion
                }

                #region Common

                //Set AccountStd
                supplierAccountStd.AccountStd = accountStd;

                //Set AccountInternal
                if (supplierAccountStd.AccountInternal == null)
                    supplierAccountStd.AccountInternal = new EntityCollection<AccountInternal>();
                supplierAccountStd.AccountInternal.AddRange(accountInternals);

                #endregion
            }

            #endregion

            #region Delete

            for (int i = 0; i < supplierAccountStds.Count; i++)
            {
                SupplierAccountStd supplierAccountStd = supplierAccountStds[i];
                if (supplierAccountStd == null || updatedIds.Contains(supplierAccountStd.SupplierAccountStdId))
                    continue;

                supplierAccountStd.AccountInternal.Clear();
                entities.DeleteObject(supplierAccountStd);
            }

            #endregion

            return true;
        }

        protected bool TryAddCustomerAccountStds(CompEntities entities, Customer customer, List<ImportAccount> accounts, bool doNotModifyWithEmpty = false)
        {
            if (customer == null)
                return false;

            #region Prereq

            if (base.CanEntityLoadReferences(entities, customer))
            {
                if (!customer.CustomerAccountStd.IsLoaded)
                    customer.CustomerAccountStd.Load();

                foreach (CustomerAccountStd customerAccountStd in customer.CustomerAccountStd)
                {
                    if (!customerAccountStd.AccountStdReference.IsLoaded)
                        customerAccountStd.AccountStdReference.Load();
                    if (!customerAccountStd.AccountInternal.IsLoaded)
                        customerAccountStd.AccountInternal.Load();
                }
            }

            List<int> updatedIds = new List<int>();
            List<CustomerAccountStd> customerAccountStds = customer.CustomerAccountStd != null ? customer.CustomerAccountStd.ToList() : new List<CustomerAccountStd>();

            #endregion

            #region Add/Update

            foreach (ImportAccount account in accounts)
            {
                AccountStd accountStd = account.AccountStd;
                List<AccountInternal> accountInternals = account.AccountInternals;
                CustomerAccountStd customerAccountStd = customerAccountStds.OrderBy(i => i.CustomerAccountStdId).FirstOrDefault(i => i.Type == account.Type);

                bool allowOnlyAccountInternals = true;
                bool update = customerAccountStd != null;
                bool valid = IsAccountsValid(accountStd, accountInternals, allowOnlyAccountInternals, update, doNotModifyWithEmpty);
                if (!valid)
                    continue;

                if (customerAccountStd == null)
                {
                    #region Add

                    customerAccountStd = new CustomerAccountStd()
                    {
                        Type = account.Type,
                        Percent = (int)account.Percent,

                        //Set references
                        Customer = customer,
                    };
                    entities.CustomerAccountStd.AddObject(customerAccountStd);

                    #endregion
                }
                else
                {
                    #region Update

                    if (accountStd == null && doNotModifyWithEmpty)
                        accountStd = customerAccountStd.AccountStd;
                    if (accountInternals == null || accountInternals.Count == 0 && doNotModifyWithEmpty)
                        accountInternals = customerAccountStd.AccountInternal.ToList();

                    updatedIds.Add(customerAccountStd.CustomerAccountStdId);

                    #endregion
                }

                #region Common

                //Set AccountStd
                customerAccountStd.AccountStd = accountStd;

                //Set AccountInternal
                if (customerAccountStd.AccountInternal == null)
                    customerAccountStd.AccountInternal = new EntityCollection<AccountInternal>();
                customerAccountStd.AccountInternal.AddRange(accountInternals);

                #endregion
            }

            #endregion

            #region Delete

            for (int i = 0; i < customerAccountStds.Count; i++)
            {
                CustomerAccountStd customerAccountStd = customerAccountStds[i];
                if (customerAccountStd == null || updatedIds.Contains(customerAccountStd.CustomerAccountStdId))
                    continue;

                customerAccountStd.AccountInternal.Clear();
                entities.DeleteObject(customerAccountStd);
            }

            #endregion

            return true;
        }

        protected bool TryAddProductAccountStds(CompEntities entities, Product product, List<ImportAccount> accounts, bool doNotModifyWithEmpty = false)
        {
            if (product == null)
                return false;

            #region Prereq

            if (base.CanEntityLoadReferences(entities, product))
            {
                if (!product.ProductAccountStd.IsLoaded)
                    product.ProductAccountStd.Load();

                foreach (ProductAccountStd productAccountStd in product.ProductAccountStd)
                {
                    if (!productAccountStd.AccountStdReference.IsLoaded)
                        productAccountStd.AccountStdReference.Load();
                    if (!productAccountStd.AccountInternal.IsLoaded)
                        productAccountStd.AccountInternal.Load();
                }
            }

            List<int> updatedIds = new List<int>();
            List<ProductAccountStd> productAccountStds = product.ProductAccountStd != null ? product.ProductAccountStd.ToList() : new List<ProductAccountStd>();

            #endregion

            #region Add/Update

            foreach (ImportAccount account in accounts)
            {
                AccountStd accountStd = account.AccountStd;
                List<AccountInternal> accountInternals = account.AccountInternals;
                ProductAccountStd productAccountStd = productAccountStds.OrderBy(i => i.ProductAccountStdId).FirstOrDefault(i => i.Type == account.Type);

                bool allowOnlyAccountInternals = true;
                bool update = productAccountStd != null;
                bool valid = IsAccountsValid(accountStd, accountInternals, allowOnlyAccountInternals, update, doNotModifyWithEmpty);
                if (!valid)
                    continue;

                if (productAccountStd == null)
                {
                    #region Add

                    productAccountStd = new ProductAccountStd()
                    {
                        Type = account.Type,
                        Percent = (int)account.Percent,

                        //Set references
                        Product = product,
                    };
                    entities.ProductAccountStd.AddObject(productAccountStd);

                    #endregion
                }
                else
                {
                    #region Update

                    if (accountStd == null && doNotModifyWithEmpty)
                        accountStd = productAccountStd.AccountStd;
                    if (accountInternals == null || accountInternals.Count == 0 && doNotModifyWithEmpty)
                        accountInternals = productAccountStd.AccountInternal.ToList();

                    updatedIds.Add(productAccountStd.ProductAccountStdId);

                    #endregion
                }

                #region Common

                //Set AccountStd
                productAccountStd.AccountStd = accountStd;

                //Set AccountInternal
                if (productAccountStd.AccountInternal == null)
                    productAccountStd.AccountInternal = new EntityCollection<AccountInternal>();
                productAccountStd.AccountInternal.AddRange(accountInternals);

                #endregion
            }

            #endregion

            #region Delete

            for (int i = 0; i < productAccountStds.Count; i++)
            {
                ProductAccountStd productAccountStd = productAccountStds[i];
                if (productAccountStd == null || updatedIds.Contains(productAccountStd.ProductAccountStdId))
                    continue;

                productAccountStd.AccountInternal.Clear();
                entities.DeleteObject(productAccountStd);
            }

            #endregion

            return true;
        }

        protected bool TrySaveEmploymentAccountStds(CompEntities entities, Employment employment, List<ImportAccount> accounts, List<AccountDim> accountDimInternals, bool doNotModifyWithEmpty = false)
        {
            if (employment == null)
                return false;

            #region Prereq

            if (base.CanEntityLoadReferences(entities, employment))
            {
                if (!employment.EmploymentAccountStd.IsLoaded)
                    employment.EmploymentAccountStd.Load();

                foreach (EmploymentAccountStd employmentAccountStd in employment.EmploymentAccountStd)
                {
                    if (!employmentAccountStd.AccountStdReference.IsLoaded)
                        employmentAccountStd.AccountStdReference.Load();
                    if (!employmentAccountStd.AccountInternal.IsLoaded)
                        employmentAccountStd.AccountInternal.Load();

                    foreach (var accountInternal in employmentAccountStd.AccountInternal)
                    {
                        if (!accountInternal.AccountReference.IsLoaded)
                            accountInternal.AccountReference.Load();
                    }
                }
            }

            List<int> updatedIds = new List<int>();
            List<EmploymentAccountStd> employmentAccountStds = employment.EmploymentAccountStd != null ? employment.EmploymentAccountStd.ToList() : new List<EmploymentAccountStd>();

            #endregion

            #region Add/Update

            foreach (ImportAccount account in accounts)
            {
                AccountStd accountStd = account.AccountStd;
                List<AccountInternal> accountInternals = account.AccountInternals;
                EmploymentAccountStd employmentAccountStd = employmentAccountStds.OrderBy(i => i.EmploymentAccountStdId).FirstOrDefault(i => i.Type == account.Type);

                bool allowOnlyAccountInternals = true;
                bool update = employmentAccountStd != null;

                if (update)
                    updatedIds.Add(employmentAccountStd.EmploymentAccountStdId);

                if (!IsAccountsValid(accountStd, accountInternals, allowOnlyAccountInternals, update, doNotModifyWithEmpty))
                    continue;

                if (employmentAccountStd == null)
                {
                    #region Add

                    employmentAccountStd = new EmploymentAccountStd()
                    {
                        Type = account.Type,
                        Percent = account.Percent,

                        //Set references
                        Employment = employment,
                    };
                    entities.EmploymentAccountStd.AddObject(employmentAccountStd);

                    #endregion
                }
                else
                {
                    #region Update

                    if (accountStd == null && doNotModifyWithEmpty)
                        accountStd = employmentAccountStd.AccountStd;
                    if (accountInternals == null || accountInternals.Count == 0 && doNotModifyWithEmpty)
                        accountInternals = employmentAccountStd.AccountInternal.ToList();

                    #endregion
                }

                #region Common

                //Set AccountStd
                employmentAccountStd.AccountStd = accountStd;

                //Set AccountInternal
                if (employmentAccountStd.AccountInternal == null)
                    employmentAccountStd.AccountInternal = new EntityCollection<AccountInternal>();

                if (employmentAccountStd.AccountInternal.Count == 0 || !doNotModifyWithEmpty)
                {
                    employmentAccountStd.AccountInternal.Clear();
                    employmentAccountStd.AccountInternal.AddRange(accountInternals);
                }
                else
                {
                    if (accountDimInternals != null)
                    {
                        List<AccountInternal> accountInternalsToAdd = new List<AccountInternal>();
                        foreach (AccountDim accountDimInternal in accountDimInternals)
                        {
                            if (accountDimInternal.AccountDimNr == Constants.ACCOUNTDIM_STANDARD)
                                continue;

                            AccountInternal accountInternal = accountInternals.FirstOrDefault(i => i.Account.AccountDimId == accountDimInternal.AccountDimId);
                            if (accountInternal == null)
                                accountInternal = employmentAccountStd.AccountInternal.FirstOrDefault(i => i.Account.AccountDimId == accountDimInternal.AccountDimId);
                            if (accountInternal != null)
                                accountInternalsToAdd.Add(accountInternal);
                        }

                        employmentAccountStd.AccountInternal.Clear();
                        employmentAccountStd.AccountInternal.AddRange(accountInternalsToAdd);
                    }
                }

                #endregion
            }

            #endregion

            #region Delete

            for (int i = 0; i < employmentAccountStds.Count; i++)
            {
                EmploymentAccountStd employmentAccountStd = employmentAccountStds[i];
                if (employmentAccountStd == null || updatedIds.Contains(employmentAccountStd.EmploymentAccountStdId))
                    continue;
                if (employmentAccountStd.Type != (int)EmploymentAccountType.Cost && employmentAccountStd.Type != (int)EmploymentAccountType.Income)
                    continue;

                employmentAccountStd.AccountInternal.Clear();
                entities.DeleteObject(employmentAccountStd);
            }

            #endregion

            return true;
        }

        protected bool TryAddEmploymentPriceType(CompEntities entities, Employment employment, ImportEmploymentPriceType importEmploymentPriceType)
        {
            if (employment == null)
                return false;
            if (importEmploymentPriceType == null)
                return true; //Nothing to save

            #region Prereq

            if (base.CanEntityLoadReferences(entities, employment) && !employment.EmploymentPriceType.IsLoaded)
                employment.EmploymentPriceType.Load();

            PayrollPriceType payrollPriceType = GetPayrollPriceType(entities, importEmploymentPriceType.PriceTypeCode);
            if (payrollPriceType == null)
                return false;

            PayrollLevel payrollLevel = GetPayrollLevel(entities, importEmploymentPriceType.PayrollLevelCode);

            #endregion

            EmploymentPriceType employmentPriceType = employment.GetPriceType(payrollPriceType.PayrollPriceTypeId);
            if (employmentPriceType == null)
            {
                employmentPriceType = new EmploymentPriceType()
                {
                    //Set FK
                    EmploymentId = employment.EmploymentId,
                    PayrollPriceTypeId = payrollPriceType.PayrollPriceTypeId,
                };
                SetCreatedProperties(employmentPriceType);
                entities.EmploymentPriceType.AddObject(employmentPriceType);

                EmploymentPriceTypePeriod employmentPriceTypePeriod = new EmploymentPriceTypePeriod()
                {
                    Amount = importEmploymentPriceType.Amount,
                    FromDate = importEmploymentPriceType.FromDate,
                    PayrollLevelId = payrollLevel?.PayrollLevelId,

                    //Set references
                    EmploymentPriceType = employmentPriceType,
                };
                SetCreatedProperties(employmentPriceTypePeriod);
                entities.EmploymentPriceTypePeriod.AddObject(employmentPriceTypePeriod);

                if (employmentPriceType.EmploymentPriceTypePeriod == null)
                    employmentPriceType.EmploymentPriceTypePeriod = new EntityCollection<EmploymentPriceTypePeriod>();
                employmentPriceType.EmploymentPriceTypePeriod.Add(employmentPriceTypePeriod);
            }
            else if (importEmploymentPriceType.FromDate.HasValue && payrollLevel != null)
            {
                if (base.CanEntityLoadReferences(entities, employment) && !employmentPriceType.EmploymentPriceTypePeriod.IsLoaded)
                    employmentPriceType.EmploymentPriceTypePeriod.Load();

                EmploymentPriceTypePeriod period = employmentPriceType.GetPeriod(importEmploymentPriceType.FromDate.Value);
                if (period != null && !period.PayrollLevelId.HasValue)
                {
                    period.PayrollLevelId = payrollLevel.PayrollLevelId;
                    SetModifiedProperties(period);
                }
            }

            return true;
        }

        protected bool TryAddEmployeeFactors(CompEntities entities, Employee employee, ImportEmployeeFactor importEmployeeFactor)
        {
            if (employee == null)
                return false;
            if (importEmployeeFactor == null)
                return true; //Nothing to save

            #region Prereq

            if (base.CanEntityLoadReferences(entities, employee) && !employee.EmployeeFactor.IsLoaded)
                employee.EmployeeFactor.Load();

            #endregion

            EmployeeFactor existingEmployeeFactor = employee.EmployeeFactor.FirstOrDefault(i => i.Type == (int)importEmployeeFactor.Type);
            if (existingEmployeeFactor == null)
            {
                existingEmployeeFactor = new EmployeeFactor()
                {
                    Type = (int)importEmployeeFactor.Type,
                    FromDate = importEmployeeFactor.FromDate,
                    Factor = importEmployeeFactor.Factor,

                    //Set FK
                    VacationGroupId = null,

                    //Set references
                    Employee = employee,
                };
                SetCreatedProperties(existingEmployeeFactor);
                entities.EmployeeFactor.AddObject(existingEmployeeFactor);
            }

            return true;
        }

        protected bool TryAddEmployeePosition(CompEntities entities, Employee employee, string employeePositionCode)
        {
            if (employee == null)
                return false;
            if (String.IsNullOrEmpty(employeePositionCode))
                return true; //Nothing to save

            #region Prereq

            if (base.CanEntityLoadReferences(entities, employee) && !employee.EmployeePosition.IsLoaded)
                employee.EmployeePosition.Load();

            Position position = GetPosition(entities, employeePositionCode);
            if (position == null)
                return false;

            #endregion

            EmployeePosition existingEmployeePosition = employee.EmployeePosition.FirstOrDefault(i => i.PositionId == position.PositionId);
            if (existingEmployeePosition == null)
            {
                existingEmployeePosition = new EmployeePosition()
                {
                    Default = true,

                    //Set FK
                    EmployeeId = employee.EmployeeId,
                    PositionId = position.PositionId,
                };
                SetCreatedProperties(existingEmployeePosition);
                entities.EmployeePosition.AddObject(existingEmployeePosition);
            }

            return true;
        }

        protected bool TryAddUserCompanyRoles(CompEntities entities, User user, bool doNotModifyWithEmpty = false, params string[] names)
        {
            if (user == null)
                return false;

            #region Prereq

            bool hasValues = names.Any(name => !string.IsNullOrEmpty(name));
            if (!hasValues && doNotModifyWithEmpty)
                return true;

            if (this.rolesCache == null)
            {
                this.rolesCache = RoleManager.GetRolesByCompany(entities, company.ActorCompanyId);
                foreach (Role role in this.rolesCache)
                {
                    role.ActualName = RoleManager.GetRoleNameText(role);
                }
            }

            #endregion

            #region Delete

            if (base.CanEntityLoadReferences(entities, user) && !user.UserCompanyRole.IsLoaded)
                user.UserCompanyRole.Load();

            if (user.UserCompanyRole != null)
                user.UserCompanyRole
                    .Where(ucr => ucr.State == (int)SoeEntityState.Active)
                    .ToList()
                    .ForEach(ucr => ChangeEntityState(entities, ucr, SoeEntityState.Deleted, saveChanges: false));
            else
                user.UserCompanyRole = new EntityCollection<UserCompanyRole>();

            #endregion

            #region Add

            bool setDefaultRole = true;

            foreach (string roleName in names)
            {
                if (string.IsNullOrEmpty(roleName))
                    continue;

                Role role = this.rolesCache.FirstOrDefault(r => r.ActualName != null && r.ActualName.ToLower() == roleName.ToLower());
                if (role != null)
                {
                    UserCompanyRole userCompanyRole = new UserCompanyRole()
                    {
                        //Set references
                        User = user,
                        Company = this.company,
                        Role = role,
                        DateFrom = null,
                        DateTo = null,
                        Default = setDefaultRole,
                    };
                    SetCreatedProperties(userCompanyRole);
                    entities.UserCompanyRole.AddObject(userCompanyRole);
                    setDefaultRole = false;
                }
            }

            #endregion

            return true;
        }

        protected bool TryAddUserAttestRoles(CompEntities entities, User user, List<ImportAttestRole> importAttestRoles, bool doNotModifyWithEmpty = false)
        {
            if (user == null)
                return false;

            importAttestRoles = importAttestRoles?.Where(i => i.AttestRole != null).ToList();
            if (importAttestRoles.IsNullOrEmpty())
                return true; //Nothing to save

            #region Delete

            if (base.CanEntityLoadReferences(entities, user) && !user.AttestRoleUser.IsLoaded)
                user.AttestRoleUser.Load();

            if (user.AttestRoleUser != null)
                user.AttestRoleUser
                    .Where(aru => aru.State == (int)SoeEntityState.Active)
                    .ToList()
                    .ForEach(aru => ChangeEntityState(entities, aru, SoeEntityState.Deleted, saveChanges: false));
            else
                user.AttestRoleUser = new EntityCollection<AttestRoleUser>();

            #endregion

            #region Add

            if (importAttestRoles != null)
            {
                foreach (ImportAttestRole importAttestRole in importAttestRoles)
                {
                    AttestRoleUser attestRoleUser = new AttestRoleUser()
                    {
                        User = user,
                        MaxAmount = 0,

                        //Set FK
                        AttestRoleId = importAttestRole.AttestRole.AttestRoleId,
                        UserId = user.UserId,
                        AccountId = importAttestRole.AccountInternal?.AccountId,
                    };
                    SetCreatedProperties(attestRoleUser);
                    entities.AttestRoleUser.AddObject(attestRoleUser);
                }
            }

            #endregion

            return true;
        }

        protected bool TryAddEmployeeVacationSE(CompEntities entities, Employee employee, EmployeeIODTO io)
        {
            if (employee == null)
                return false;

            #region Prereq

            if (base.CanEntityLoadReferences(entities, employee) && !employee.EmployeeVacationSE.IsLoaded)
                employee.EmployeeVacationSE.Load();

            EmployeeVacationSE currentEmployeeVacationSE = employee.EmployeeVacationSE.OrderByDescending(i => i.Created).ThenByDescending(i => i.Modified).FirstOrDefault(i => i.State == (int)SoeEntityState.Active);
            foreach (EmployeeVacationSE employeeVacationSE in employee.EmployeeVacationSE.Where(i => i.State == (int)SoeEntityState.Active))
            {
                if (currentEmployeeVacationSE != null && currentEmployeeVacationSE.EmployeeVacationSEId == employeeVacationSE.EmployeeVacationSEId)
                    continue;
                ChangeEntityState(employeeVacationSE, SoeEntityState.Deleted);
            }

            #endregion

            bool doCreateNew = false;
            if (currentEmployeeVacationSE == null)
                doCreateNew = true;
            else if (io.HasAnyVacationFieldsValue() && !currentEmployeeVacationSE.IsSame(io))
                doCreateNew = true;

            if (doCreateNew)
            {
                if (currentEmployeeVacationSE != null)
                    ChangeEntityState(currentEmployeeVacationSE, SoeEntityState.Deleted);

                EmployeeVacationSE newEmployeeVacationSE = new EmployeeVacationSE()
                {
                    EarnedDaysPaid = io.EarnedDaysPaid,
                    EarnedDaysUnpaid = io.EarnedDaysUnpaid,
                    EarnedDaysAdvance = io.EarnedDaysAdvance,

                    SavedDaysYear1 = io.SavedDaysYear1,
                    SavedDaysYear2 = io.SavedDaysYear2,
                    SavedDaysYear3 = io.SavedDaysYear3,
                    SavedDaysYear4 = io.SavedDaysYear4,
                    SavedDaysYear5 = io.SavedDaysYear5,
                    SavedDaysOverdue = io.SavedDaysOverdue,

                    UsedDaysPaid = io.UsedDaysPaid,
                    PaidVacationAllowance = io.PaidVacationAllowance,
                    PaidVacationVariableAllowance = io.PaidVacationVariableAllowance,
                    UsedDaysUnpaid = io.UsedDaysUnpaid,
                    UsedDaysAdvance = io.UsedDaysAdvance,
                    UsedDaysYear1 = io.UsedDaysYear1,
                    UsedDaysYear2 = io.UsedDaysYear2,
                    UsedDaysYear3 = io.UsedDaysYear3,
                    UsedDaysYear4 = io.UsedDaysYear4,
                    UsedDaysYear5 = io.UsedDaysYear5,
                    UsedDaysOverdue = io.UsedDaysOverdue,

                    RemainingDaysPaid = io.RemainingDaysPaid,
                    RemainingDaysUnpaid = io.RemainingDaysUnpaid,
                    RemainingDaysAdvance = io.RemainingDaysAdvance,
                    RemainingDaysYear1 = io.RemainingDaysYear1,
                    RemainingDaysYear2 = io.RemainingDaysYear2,
                    RemainingDaysYear3 = io.RemainingDaysYear3,
                    RemainingDaysYear4 = io.RemainingDaysYear4,
                    RemainingDaysYear5 = io.RemainingDaysYear5,
                    RemainingDaysOverdue = io.RemainingDaysOverdue,

                    EarnedDaysRemainingHoursPaid = 0,
                    EarnedDaysRemainingHoursUnpaid = 0,
                    EarnedDaysRemainingHoursAdvance = 0,
                    EarnedDaysRemainingHoursYear1 = 0,
                    EarnedDaysRemainingHoursYear2 = 0,
                    EarnedDaysRemainingHoursYear3 = 0,
                    EarnedDaysRemainingHoursYear4 = 0,
                    EarnedDaysRemainingHoursYear5 = 0,
                    EarnedDaysRemainingHoursOverdue = 0,

                    EmploymentRatePaid = io.EmploymentRatePaid,
                    EmploymentRateYear1 = io.EmploymentRateYear1,
                    EmploymentRateYear2 = io.EmploymentRateYear2,
                    EmploymentRateYear3 = io.EmploymentRateYear3,
                    EmploymentRateYear4 = io.EmploymentRateYear4,
                    EmploymentRateYear5 = io.EmploymentRateYear5,
                    EmploymentRateOverdue = io.EmploymentRateOverdue,

                    DebtInAdvanceAmount = io.DebtInAdvanceAmount,
                    DebtInAdvanceDueDate = io.DebtInAdvanceDueDate,
                    DebtInAdvanceDelete = false,

                    //Set FK
                    EmployeeId = employee.EmployeeId,
                    PrevEmployeeVacationSEId = currentEmployeeVacationSE != null ? currentEmployeeVacationSE.EmployeeVacationSEId : (int?)null,
                };
                SetCreatedProperties(newEmployeeVacationSE);
                entities.EmployeeVacationSE.AddObject(newEmployeeVacationSE);
            }
            return true;
        }

        protected bool TryAddEmployeeAccounts(CompEntities entities, int employeeId, List<ImportEmployeeAccount> importEmployeeAccounts, bool doNotModifyWithEmpty = false)
        {
            if (employeeId <= 0)
                return false;

            importEmployeeAccounts = importEmployeeAccounts?.Where(i => i.AccountInternal != null).ToList();
            if (importEmployeeAccounts.IsNullOrEmpty())
                return true; //Nothing to save

            #region Prereq

            List<EmployeeAccount> employeeAccounts = EmployeeManager.GetEmployeeAccounts(entities, company.ActorCompanyId, employeeId);

            #endregion

            #region Delete

            if (!employeeAccounts.IsNullOrEmpty())
            {
                foreach (EmployeeAccount employeeAccount in employeeAccounts)
                {
                    base.ChangeEntityState(employeeAccount, SoeEntityState.Deleted);
                }
            }

            #endregion

            #region Add

            if (importEmployeeAccounts != null)
            {
                foreach (ImportEmployeeAccount importEmployeeAccount in importEmployeeAccounts)
                {
                    EmployeeAccount employeeAccount = new EmployeeAccount()
                    {
                        DateFrom = importEmployeeAccount.DateFrom ?? CalendarUtility.DATETIME_DEFAULT,
                        Default = importEmployeeAccount.IsDefault,

                        //Set FK
                        ActorCompanyId = company.ActorCompanyId,
                        EmployeeId = employeeId,
                        AccountId = importEmployeeAccount.AccountInternal.AccountId,
                    };
                    SetCreatedProperties(employeeAccount);
                    entities.EmployeeAccount.AddObject(employeeAccount);
                }
            }

            #endregion

            return true;
        }

        protected bool TryAddCategories(CompEntities entities, int recordId, List<ImportCategoryRecord> importCategoryRecords, SoeCategoryType categoryType, SoeCategoryRecordEntity entity, bool secondary)
        {
            if (recordId <= 0)
                return false;

            importCategoryRecords = importCategoryRecords?.Where(i => i.Category != null).ToList();
            if (importCategoryRecords.IsNullOrEmpty())
                return true; //Nothing to save

            #region Prereq

            //Get existing records
            List<CompanyCategoryRecord> categoryRecords = CategoryManager.GetCompanyCategoryRecords(entities, categoryType, entity, recordId, company.ActorCompanyId);
            if (secondary)
                categoryRecords = categoryRecords.Where(i => !i.Default).ToList();
            else
                categoryRecords = categoryRecords.Where(i => i.Default).ToList();

            #endregion

            #region Delete

            foreach (var record in categoryRecords)
            {
                entities.DeleteObject(record);
            }

            categoryRecords = new List<CompanyCategoryRecord>();

            #endregion

            #region Add

            if (importCategoryRecords != null)
            {
                foreach (ImportCategoryRecord importCategoryRecord in importCategoryRecords)
                {
                    CompanyCategoryRecord record = categoryRecords.FirstOrDefault(r => r.CategoryId == importCategoryRecord.Category.CategoryId);
                    if (record == null)
                    {
                        record = new CompanyCategoryRecord()
                        {
                            RecordId = recordId,
                            Entity = (int)categoryType,
                            Default = !secondary,

                            //Set FK
                            CategoryId = importCategoryRecord.Category.CategoryId,
                            ActorCompanyId = this.company.ActorCompanyId,
                        };
                        entities.CompanyCategoryRecord.AddObject(record);
                        categoryRecords.Add(record);
                    }
                }
            }

            #endregion

            return true;
        }

        protected bool TryAddContactEcoms(CompEntities entities, Contact contact, List<ImportECom> importEcoms, bool doNotModifyWithEmpty = false, bool importGLN = false)
        {
            if (contact == null)
                return false;
            if (importEcoms.IsNullOrEmpty())
                return true; //Nothing to save

            #region Prereq

            if (base.CanEntityLoadReferences(entities, contact) && !contact.ContactECom.IsLoaded)
                contact.ContactECom.Load();

            #endregion


            foreach (var imporEcomsByType in importEcoms.GroupBy(x => x.Type))
            {
                if (imporEcomsByType.Key == (int)TermGroup_SysContactEComType.GlnNumber && !importGLN)
                    continue;

                //Get existing ContactECom
                var existingEComsByType = contact.ContactECom.Where(i => i.SysContactEComTypeId == imporEcomsByType.Key);

                foreach (var importEcom in imporEcomsByType)
                {
                    ContactECom contactECom = existingEComsByType.FirstOrDefault(i => i.ContactEComId > 0 && i.SysContactEComTypeId == importEcom.Type);

                    if (StringUtility.HasValue(importEcom.Text))
                    {
                        if (existingEComsByType.Any(x => x.Text?.ToLower() == importEcom.Text.ToLower()))
                            continue;

                        ContactManager.CreateContactECom(entities, contact, importEcom.Type, importEcom.Text);
                    }
                    else
                    {
                        if (!doNotModifyWithEmpty && contactECom != null)
                        {
                            entities.DeleteObject(contactECom);
                        }
                    }
                }
            }

            /*
            //Not sure why we don't handle GLN per default? 
            foreach (ImportECom importEcom in importEcoms.Where(x => (x.Type != (int)TermGroup_SysContactEComType.GlnNumber || importGLN == true)))
            {
                #region ImportECom

                //Get existing ContactECom
                ContactECom contactECom = contact.ContactECom.FirstOrDefault(i => i.ContactEComId > 0 && i.SysContactEComTypeId == importEcom.Type);

                if (StringUtility.HasValue(importEcom.Text))
                {
                    if (contactECom == null)
                    {
                        ContactManager.CreateContactECom(entities, contact, importEcom.Type, importEcom.Text);
                    }
                    else
                    {
                        contactECom.Text = importEcom.Text;
                        SetModifiedProperties(contactECom);
                    }
                }
                else
                {
                    if (!doNotModifyWithEmpty && contactECom != null)
                    {
                        entities.DeleteObject(contactECom);
                    }
                }
                
                #endregion
         
            }

            */
            return true;
        }

        protected bool TryAddClosestRelatives(CompEntities entities, Contact contact, List<ImportClosestRelative> importClosestRelatives, bool doNotModifyWithEmpty = false)
        {
            if (contact == null || contact.Actor == null)
                return false;
            if (importClosestRelatives.IsNullOrEmpty())
                return true; //Nothing to save

            #region Prereq

            if (base.CanEntityLoadReferences(entities, contact) && !contact.ContactECom.IsLoaded)
                contact.ContactECom.Load();

            #endregion

            List<ContactECom> closestRelatives = contact.ContactECom.Where(i => i.ContactEComId > 0 && i.SysContactEComTypeId == (int)TermGroup_SysContactEComType.ClosestRelative).ToList();

            int counter = 0;
            foreach (ImportClosestRelative importClosestRelative in importClosestRelatives)
            {
                #region ImportClosestRelative

                string text = StringUtility.NullToEmpty(importClosestRelative.Nr);
                string description = ContactManager.MergeClosestRelative(importClosestRelative.Name, importClosestRelative.Relation);

                //Get existing ContactECom
                ContactECom contactECom = closestRelatives.Skip(counter).FirstOrDefault();
                if (contactECom == null)
                {
                    contactECom = new ContactECom()
                    {
                        SysContactEComTypeId = (int)TermGroup_SysContactEComType.ClosestRelative,
                        Name = GetText((int)TermGroup_SysContactEComType.ClosestRelative, (int)TermGroup.SysContactEComType),
                        Text = text,
                        Description = description,
                    };
                    SetCreatedProperties(contact);
                    entities.ContactECom.AddObject(contactECom);

                    if (contact.ContactECom == null)
                        contact.ContactECom = new EntityCollection<ContactECom>();
                    contact.ContactECom.Add(contactECom);
                }
                else if (contactECom.Text != text || contactECom.Description != description)
                {
                    contactECom.Text = StringUtility.ModifyValue(contactECom.Text, text, doNotModifyWithEmpty);
                    contactECom.Description = StringUtility.ModifyValue(contactECom.Description, description, doNotModifyWithEmpty);
                    SetModifiedProperties(contactECom);
                }

                counter++;

                #endregion
            }

            return true;
        }

        protected bool TryAddContactAddresses(CompEntities entities, Contact contact, List<ImportAddress> importAddresses, bool doNotModifyWithEmpty = false)
        {
            if (contact == null)
                return false;
            if (importAddresses.IsNullOrEmpty())
                return true; //Nothing to save

            #region Prereq

            List<ContactAddress> contactAddresses = ContactManager.GetContactAddresses(entities, contact.ContactId);
            List<ContactAddressRow> contactAddressRows = ContactManager.GetContactAddressRows(entities, contact.ContactId);

            #endregion

            foreach (ImportAddress importAddress in importAddresses)
            {
                if (importAddress.SysContactAddressTypeId == 0 || importAddress.SysContactAddressRowTypeId == 0 || importAddress.Value == null)
                    continue;

                #region ContactAddress

                string name = GetText((int)importAddress.SysContactAddressTypeId, (int)TermGroup.SysContactAddressType);

                ContactAddress contactAddress = contactAddresses.FirstOrDefault(i => i.SysContactAddressTypeId == (int)importAddress.SysContactAddressTypeId);
                if (contactAddress == null)
                {
                    #region Add

                    // No need to store empty string if no address object exist
                    if (string.IsNullOrEmpty(importAddress.Value))
                        continue;

                    contactAddress = ContactManager.CreateContactAddress(entities, contact, importAddress.SysContactAddressTypeId, name);

                    contactAddresses.Add(contactAddress);

                    #endregion
                }
                else
                {
                    #region Update

                    if (contactAddress.Name != name)
                    {
                        contactAddress.Name = StringUtility.ModifyValue(contactAddress.Name, name, doNotModifyWithEmpty);
                        SetModifiedProperties(contactAddress);
                    }

                    #endregion
                }

                #endregion

                #region ContactAddressRow

                ContactAddressRow contactAddressRow = contactAddressRows.FirstOrDefault(i => i.ContactAddress.SysContactAddressTypeId == (int)importAddress.SysContactAddressTypeId && i.SysContactAddressRowTypeId == (int)importAddress.SysContactAddressRowTypeId);
                if (contactAddressRow == null)
                {
                    #region Add

                    // No need to store empty string if no address object exist
                    if (string.IsNullOrEmpty(importAddress.Value))
                        continue;

                    contactAddressRow = ContactManager.CreateContactAddressRow(entities, importAddress.SysContactAddressRowTypeId, importAddress.Value);
                    if (contactAddressRow != null)
                        contactAddress.ContactAddressRow.Add(contactAddressRow);

                    #endregion
                }
                else
                {
                    if (string.IsNullOrEmpty(importAddress.Value))
                    {
                        if (!doNotModifyWithEmpty)
                        {
                            #region Delete

                            entities.DeleteObject(contactAddressRow);
                            contactAddressRows.Remove(contactAddressRow);

                            #endregion
                        }
                    }
                    else if (!importAddress.Value.Equals(contactAddressRow.Text))
                    {
                        #region Update

                        string text = StringUtility.ModifyValue(contactAddressRow.Text, importAddress.Value, doNotModifyWithEmpty);
                        if (contactAddressRow.Text != text)
                        {
                            contactAddressRow.Text = text;
                            SetModifiedProperties(contactAddressRow);
                        }

                        #endregion
                    }
                }

                #endregion
            }

            foreach (TermGroup_SysContactAddressType columnAddressType in Enum.GetValues(typeof(TermGroup_SysContactAddressType)))
            {
                ContactAddress contactAddress = contactAddresses.FirstOrDefault(i => i.SysContactAddressTypeId == (int)columnAddressType);
                if (contactAddress == null || contactAddress.ContactAddressRow == null)
                    continue;

                //Delete ContactAddress if it has no ContactAddressRow's
                bool hasRows = contactAddress.ContactAddressRow.Count > 0;
                if (!hasRows)
                    entities.DeleteObject(contactAddress);
            }

            return true;
        }

        protected bool TryAddPaymentInformation(CompEntities entities, SupplierIO io, Actor actor, bool doNotModifyWithEmpty = false)
        {
            if (io == null || actor == null)
                return false;

            #region Prereq

            // Make sure PaymentInformation is loaded
            if (!actor.PaymentInformation.IsLoaded)
                actor.PaymentInformation.Load();

            #endregion

            #region PaymentInformation

            object standardPaymentType = io.StandardPaymentType.HasValue ? io.StandardPaymentType.Value.ToString() : null;
            if (standardPaymentType == null)
            {
                if (io.BIC.HasValue())
                    standardPaymentType = TermGroup_SysPaymentType.BIC;
                if (io.BankNr.HasValue())
                    standardPaymentType = TermGroup_SysPaymentType.Bank;
                if (io.PlusGiroNr.HasValue())
                    standardPaymentType = TermGroup_SysPaymentType.PG;
                if (io.BankGiroNr.HasValue())
                    standardPaymentType = TermGroup_SysPaymentType.BG;
            }

            int defaultSysPaymentTypeId = PaymentManager.GetStandardPaymentType(standardPaymentType);

            PaymentInformation paymentInformation = actor.PaymentInformation.FirstOrDefault(i => i.State == (int)SoeEntityState.Active);
            if (paymentInformation == null)
            {
                #region Add

                paymentInformation = new PaymentInformation()
                {
                    DefaultSysPaymentTypeId = defaultSysPaymentTypeId,
                    Actor = actor,
                };
                SetCreatedProperties(paymentInformation);

                #endregion
            }
            else
            {
                #region Update

                if (IsOkToUpdateValue(doNotModifyWithEmpty, standardPaymentType) && paymentInformation.DefaultSysPaymentTypeId != defaultSysPaymentTypeId)
                {
                    paymentInformation.DefaultSysPaymentTypeId = defaultSysPaymentTypeId;
                    SetModifiedProperties(paymentInformation);
                }

                // Make sure PaymentInformationRow is loaded
                if (!paymentInformation.PaymentInformationRow.IsLoaded)
                    paymentInformation.PaymentInformationRow.Load();

                #endregion
            }

            #endregion

            #region PaymentInformationRow

            TryAddPaymentInformationRow(entities, paymentInformation, defaultSysPaymentTypeId, TermGroup_SysPaymentType.BG, io.BankGiroNr, doNotModifyWithEmpty);
            TryAddPaymentInformationRow(entities, paymentInformation, defaultSysPaymentTypeId, TermGroup_SysPaymentType.PG, io.PlusGiroNr, doNotModifyWithEmpty);
            TryAddPaymentInformationRow(entities, paymentInformation, defaultSysPaymentTypeId, TermGroup_SysPaymentType.Bank, io.BankNr, doNotModifyWithEmpty);
            TryAddPaymentInformationRow(entities, paymentInformation, defaultSysPaymentTypeId, TermGroup_SysPaymentType.BIC, io.BIC + "/" + io.IBAN, doNotModifyWithEmpty);

            #endregion

            return true;
        }

        protected bool TryAddPaymentInformationRow(CompEntities entities, PaymentInformation paymentInformation, int defaultSysPaymentTypeId, TermGroup_SysPaymentType sysPaymentType, string paymentNr, bool doNotModifyWithEmpty = false)
        {
            if (String.IsNullOrEmpty(paymentNr))
                return true;

            bool isDefault = (int)sysPaymentType == defaultSysPaymentTypeId;
            string bic = string.Empty;
            bool isBICOrSEPA = (sysPaymentType == TermGroup_SysPaymentType.BIC || sysPaymentType == TermGroup_SysPaymentType.SEPA) && StringUtility.CountChars(paymentNr, '/') == 1;
            if (isBICOrSEPA)
            {
                string[] bicIban = paymentNr.Split('/');
                bic = bicIban[0];
                paymentNr = bicIban[1];
            }

            if (sysPaymentType != TermGroup_SysPaymentType.BIC || (sysPaymentType == TermGroup_SysPaymentType.BIC && paymentNr.Length > 1))
            {
                // Get existing PaymentInformationRow
                PaymentInformationRow paymentInformationRow = paymentInformation.PaymentInformationRow.FirstOrDefault(i => i.PaymentInformationRowId > 0 && i.SysPaymentTypeId == (int)sysPaymentType && i.PaymentNr == paymentNr);

                if (paymentInformationRow == null)
                {
                    #region Add

                    // Payment info may not contain /, however we add that in the method call to separate BIC from IBAN within the same parameter.
                    if (isBICOrSEPA)
                    {
                        paymentInformationRow = new PaymentInformationRow()
                        {
                            SysPaymentTypeId = (int)sysPaymentType,
                            BIC = bic,
                            PaymentNr = paymentNr,
                            Default = isDefault
                        };
                    }
                    else
                    {
                        paymentInformationRow = new PaymentInformationRow()
                        {
                            SysPaymentTypeId = (int)sysPaymentType,
                            PaymentNr = paymentNr,
                            Default = isDefault,
                        };
                    }
                    SetCreatedProperties(paymentInformationRow);
                    entities.PaymentInformationRow.AddObject(paymentInformationRow);
                    paymentInformation.PaymentInformationRow.Add(paymentInformationRow);

                    #endregion
                }
                else
                {
                    if (paymentInformationRow.State != (int)SoeEntityState.Active)
                    {
                        paymentInformationRow.State = (int)SoeEntityState.Active;
                        SetModifiedProperties(paymentInformationRow);
                    }
                }
            }
            return true;
        }

        protected void TryAddProductGroup(CompEntities entities, object column, InvoiceProduct invoiceProduct)
        {
            if (StringUtility.HasValue(column))
            {
                string value = column.ToString().ToLower();
                if (productGroupDictCache.ContainsKey(column.ToString()))
                {
                    //Get from cache
                    invoiceProduct.ProductGroup = productGroupDictCache[column.ToString()];
                }
                else
                {
                    //Get from db
                    invoiceProduct.ProductGroup = (from p in entities.ProductGroup
                                                   where p.Company.ActorCompanyId == this.company.ActorCompanyId &&
                                                   p.Code.ToLower() == value
                                                   select p).FirstOrDefault();

                    //Add to cache
                    if (invoiceProduct.ProductGroup != null)
                        productGroupDictCache.Add(column.ToString(), invoiceProduct.ProductGroup);
                }
            }
            else
            {
                //doNotModifyWithEmpty is not relevant since reference is mandatory
            }
        }

        protected void TryAddProductUnit(CompEntities entities, object column, InvoiceProduct invoiceProduct)
        {
            if (StringUtility.HasValue(column))
            {
                string value = column.ToString().ToLower();
                if (productUnitDictCache.ContainsKey(column.ToString()))
                {
                    //Get from cache
                    invoiceProduct.ProductUnit = productUnitDictCache[column.ToString()];
                }
                else
                {
                    //Get from db
                    invoiceProduct.ProductUnit = (from pu in entities.ProductUnit
                                                  where pu.Company.ActorCompanyId == this.company.ActorCompanyId &&
                                                  pu.Code.ToLower() == value
                                                  select pu).FirstOrDefault();

                    //Add to cache
                    if (invoiceProduct.ProductUnit != null)
                        productUnitDictCache.Add(column.ToString(), invoiceProduct.ProductUnit);
                }
            }
            else
            {
                //doNotModifyWithEmpty is not relevant since reference is mandatory
            }
        }

        protected void TryAddPriceList(CompEntities entities, object columnPriceListType, object columnPrice, InvoiceProduct invoiceProduct, bool doNotModifyWithEmpty = false)
        {
            if (StringUtility.HasValue(columnPriceListType))
            {
                PriceListType priceListType = GetPriceListType(entities, columnPriceListType);
                if (priceListType != null)
                {
                    decimal price = 0;
                    if (columnPrice != null)
                        decimal.TryParse(columnPrice.ToString(), out price);

                    PriceList priceList = invoiceProduct?.PriceList?.FirstOrDefault(i => i.PriceListTypeId == priceListType.PriceListTypeId);
                    if (priceList == null)
                    {
                        #region Add

                        var pricelist = new PriceList
                        {
                            Price = price,
                            PriceListType = priceListType,
                            StartDate = new DateTime(1901, 1, 1),
                            StopDate = new DateTime(9999, 1, 1)
                        };
                        SetCreatedProperties(priceList);

                        //Add to InvoiceProduct
                        if (invoiceProduct != null)
                            invoiceProduct.PriceList.Add(pricelist);

                        #endregion
                    }
                    else
                    {
                        #region Update

                        if (IsOkToUpdateValue(doNotModifyWithEmpty, columnPrice) && priceList.Price != price)
                        {
                            priceList.Price = price;
                            SetModifiedProperties(priceList);
                        }

                        #endregion
                    }
                }
            }
        }

        #endregion

        #region Save relations

        protected bool TrySaveContact(CompEntities entities, int actorId, TermGroup_SysContactType sysContactType, ref Contact contact)
        {
            ActionResult result = ContactManager.SaveContact(entities, actorId, sysContactType, false);
            if (result.Success)
                contact = result.Value as Contact;
            else
                AddWarning(WarningMessageType.SaveContactFailed);

            return result.Success;
        }

        protected ActionResult TrySaveContactEcom(CompEntities entities, Contact contact, List<ImportECom> importEcoms, bool doNotModifyWithEmpty = false, bool importGLN = false)
        {
            //Add
            if (!TryAddContactEcoms(entities, contact, importEcoms, doNotModifyWithEmpty, importGLN))
                AddWarning(WarningMessageType.AddContactEcomFailed);

            //Save
            var result = SaveChanges(entities);
            if (!result.Success)
                AddWarning(WarningMessageType.SaveContactEComFailed);

            return result;
        }

        protected ActionResult TrySaveContactEcom(CompEntities entities, Contact contact, ImportECom ecom)
        {
            if (ecom == null)
                return new ActionResult();

            ActionResult result = new ActionResult();

            //Add
            ContactECom contactECom = contact.ContactECom.FirstOrDefault(i => i.ContactEComId > 0 && i.SysContactEComTypeId == ecom.Type && i.Text.ToLower() == ecom.Text.ToLower());
            if (contactECom == null)
            {
                contactECom = ContactManager.CreateContactECom(entities, contact, ecom.Type, ecom.Text);

                //Save
                result = SaveChanges(entities);
                if (!result.Success)
                    AddWarning(WarningMessageType.SaveContactEComFailed);
                else
                    result.IntegerValue = contactECom.ContactEComId;
            }
            else
            { result.IntegerValue = contactECom.ContactEComId; }

            return result;
        }

        protected ActionResult TrySaveContactEcom(CompEntities entities, Contact contact, List<ContactEComIODTO> ecoms, TermGroup_SysContactEComType type)
        {
            if (ecoms == null)
                return new ActionResult();

            //Add
            foreach (var ecom in ecoms)
            {
                //only add for the moment....
                ContactECom contactECom = contact.ContactECom.FirstOrDefault(i => i.ContactEComId > 0 && i.SysContactEComTypeId == (int)type && i.Text.ToLower() == ecom.Text.ToLower());
                if (contactECom == null)
                {
                    ContactManager.CreateContactECom(entities, contact, (int)type, ecom.Text, ecom.Name);
                }
            }

            //Save
            var result = SaveChanges(entities);
            if (!result.Success)
                AddWarning(WarningMessageType.SaveContactEComFailed);

            return result;
        }

        protected ActionResult TrySaveClosestRelative(CompEntities entities, Contact contact, List<ImportClosestRelative> importClosestRelative, bool doNotModifyWithEmpty = false)
        {
            if (!TryAddClosestRelatives(entities, contact, importClosestRelative, doNotModifyWithEmpty))
                AddWarning(WarningMessageType.AddClosestRelativeFailed);

            ActionResult result = SaveChanges(entities);
            if (!result.Success)
                AddWarning(WarningMessageType.SaveEmployeeClosestRelativeFailed);

            return result;
        }

        protected ActionResult TrySaveContactAddresses(CompEntities entities, Contact contact, List<ImportAddress> importAddresses, bool doNotModifyWithEmpty = false)
        {
            if (!TryAddContactAddresses(entities, contact, importAddresses, doNotModifyWithEmpty))
                AddWarning(WarningMessageType.AddContactAddressesFailed);

            ActionResult result = SaveChanges(entities);
            if (!result.Success)
                AddWarning(WarningMessageType.SaveContactAddressesFailed);

            return result;
        }

        protected ActionResult TrySaveEmployeeVacationSE(CompEntities entities, Employee employee, EmployeeIODTO io)
        {
            if (!TryAddEmployeeVacationSE(entities, employee, io))
                AddWarning(WarningMessageType.AddEmployeeVacationSEFailed);

            ActionResult result = SaveChanges(entities);
            if (!result.Success)
                AddWarning(WarningMessageType.SaveEmployeeVacationSEFailed);

            return result;
        }

        protected ActionResult TrySaveCategories(CompEntities entities, int recordId, List<ImportCategoryRecord> importCategoryRecords, SoeCategoryType categoryType, SoeCategoryRecordEntity entity, bool secondary)
        {
            if (!TryAddCategories(entities, recordId, importCategoryRecords, categoryType, entity, secondary))
                AddWarning(WarningMessageType.AddCategoriesFailed);

            ActionResult result = SaveChanges(entities);
            if (!result.Success)
                AddWarning(WarningMessageType.SaveCategoriesFailed);

            return result;
        }

        protected ActionResult TrySaveEmployeeAccounts(CompEntities entities, int employeeId, List<ImportEmployeeAccount> importEmployeeAccounts, bool doNotModifyWithEmpty = false)
        {
            if (!TryAddEmployeeAccounts(entities, employeeId, importEmployeeAccounts, doNotModifyWithEmpty))
                AddWarning(WarningMessageType.AddEmployeeAccountsFailed);

            ActionResult result = SaveChanges(entities);
            if (!result.Success)
                AddWarning(WarningMessageType.SaveEmployeeAccountsFailed);

            return result;
        }

        protected ActionResult TrySavePaymentInformation(CompEntities entities, SupplierIO io, Actor actor, bool doNotModifyWithEmpty = false)
        {
            //Add
            if (!TryAddPaymentInformation(entities, io, actor, doNotModifyWithEmpty))
                AddWarning(WarningMessageType.AddPaymentInformationFailed);

            //Save
            ActionResult result = SaveChanges(entities);
            if (!result.Success)
                AddWarning(WarningMessageType.SavePaymentInformationFailed);

            return result;
        }

        #endregion

        #endregion

        #region Help-methods

        #region ImportAddress

        protected List<ImportAddress> GetImportAddresses(EmployeeIODTO dto)
        {
            return new List<ImportAddress>
            {
                // Distribution
                CreateImportAddress(TermGroup_SysContactAddressType.Distribution, TermGroup_SysContactAddressRowType.Address, dto.DistributionAddress),
                CreateImportAddress(TermGroup_SysContactAddressType.Distribution, TermGroup_SysContactAddressRowType.AddressCO, dto.DistributionCoAddress),
                CreateImportAddress(TermGroup_SysContactAddressType.Distribution, TermGroup_SysContactAddressRowType.PostalCode, dto.DistributionPostalCode),
                CreateImportAddress(TermGroup_SysContactAddressType.Distribution, TermGroup_SysContactAddressRowType.PostalAddress, dto.DistributionPostalAddress),
                CreateImportAddress(TermGroup_SysContactAddressType.Distribution, TermGroup_SysContactAddressRowType.Country, dto.DistributionCountry)
            };
        }

        protected List<ImportAddress> GetImportAddresses(SupplierIO io)
        {
            return new List<ImportAddress>
            { 
                // Distribution
                CreateImportAddress(TermGroup_SysContactAddressType.Distribution, TermGroup_SysContactAddressRowType.Address, io.DistributionAddress),
                CreateImportAddress(TermGroup_SysContactAddressType.Distribution, TermGroup_SysContactAddressRowType.AddressCO, io.DistributionCoAddress),
                CreateImportAddress(TermGroup_SysContactAddressType.Distribution, TermGroup_SysContactAddressRowType.PostalCode, io.DistributionPostalCode),
                CreateImportAddress(TermGroup_SysContactAddressType.Distribution, TermGroup_SysContactAddressRowType.PostalAddress, io.DistributionPostalAddress),
                CreateImportAddress(TermGroup_SysContactAddressType.Distribution, TermGroup_SysContactAddressRowType.Country, io.DistributionCountry),

                // Billing
                CreateImportAddress(TermGroup_SysContactAddressType.Billing, TermGroup_SysContactAddressRowType.Address, io.BillingAddress),
                CreateImportAddress(TermGroup_SysContactAddressType.Billing, TermGroup_SysContactAddressRowType.AddressCO, io.BillingCoAddress),
                CreateImportAddress(TermGroup_SysContactAddressType.Billing, TermGroup_SysContactAddressRowType.PostalCode, io.BillingPostalCode),
                CreateImportAddress(TermGroup_SysContactAddressType.Billing, TermGroup_SysContactAddressRowType.PostalAddress, io.BillingPostalAddress),
                CreateImportAddress(TermGroup_SysContactAddressType.Billing, TermGroup_SysContactAddressRowType.Country, io.BillingCountry),

                // Board HQ
                CreateImportAddress(TermGroup_SysContactAddressType.BoardHQ, TermGroup_SysContactAddressRowType.Address, io.BoardHQAddress),
                CreateImportAddress(TermGroup_SysContactAddressType.BoardHQ, TermGroup_SysContactAddressRowType.Country, io.BoardHQCountry),

                // Visiting
                CreateImportAddress(TermGroup_SysContactAddressType.Visiting, TermGroup_SysContactAddressRowType.StreetAddress, io.VisitingAddress),
                CreateImportAddress(TermGroup_SysContactAddressType.Visiting, TermGroup_SysContactAddressRowType.AddressCO, io.VisitingCoAddress),
                CreateImportAddress(TermGroup_SysContactAddressType.Visiting, TermGroup_SysContactAddressRowType.PostalCode, io.VisitingPostalCode),
                CreateImportAddress(TermGroup_SysContactAddressType.Visiting, TermGroup_SysContactAddressRowType.PostalAddress, io.VisitingPostalAddress),
                CreateImportAddress(TermGroup_SysContactAddressType.Visiting, TermGroup_SysContactAddressRowType.Country, io.VisitingCountry),

                // Delivery
                CreateImportAddress(TermGroup_SysContactAddressType.Delivery, TermGroup_SysContactAddressRowType.Address, io.DeliveryAddress),
                CreateImportAddress(TermGroup_SysContactAddressType.Delivery, TermGroup_SysContactAddressRowType.AddressCO, io.DeliveryCoAddress),
                CreateImportAddress(TermGroup_SysContactAddressType.Delivery, TermGroup_SysContactAddressRowType.PostalCode, io.DeliveryPostalCode),
                CreateImportAddress(TermGroup_SysContactAddressType.Delivery, TermGroup_SysContactAddressRowType.PostalAddress, io.DeliveryPostalAddress),
                CreateImportAddress(TermGroup_SysContactAddressType.Delivery, TermGroup_SysContactAddressRowType.Country, io.DeliveryCountry)
            };
        }

        protected List<ImportAddress> GetImportAddresses(CustomerIO io)
        {
            return new List<ImportAddress>
            { 
                // Distribution
                CreateImportAddress(TermGroup_SysContactAddressType.Distribution, TermGroup_SysContactAddressRowType.Address, io.DistributionAddress),
                CreateImportAddress(TermGroup_SysContactAddressType.Distribution, TermGroup_SysContactAddressRowType.AddressCO, io.DistributionCoAddress),
                CreateImportAddress(TermGroup_SysContactAddressType.Distribution, TermGroup_SysContactAddressRowType.PostalCode, io.DistributionPostalCode),
                CreateImportAddress(TermGroup_SysContactAddressType.Distribution, TermGroup_SysContactAddressRowType.PostalAddress, io.DistributionPostalAddress),
                CreateImportAddress(TermGroup_SysContactAddressType.Distribution, TermGroup_SysContactAddressRowType.Country, io.DistributionCountry),

                // Billing
                CreateImportAddress(TermGroup_SysContactAddressType.Billing, TermGroup_SysContactAddressRowType.Address, io.BillingAddress),
                CreateImportAddress(TermGroup_SysContactAddressType.Billing, TermGroup_SysContactAddressRowType.AddressCO, io.BillingCoAddress),
                CreateImportAddress(TermGroup_SysContactAddressType.Billing, TermGroup_SysContactAddressRowType.PostalCode, io.BillingPostalCode),
                CreateImportAddress(TermGroup_SysContactAddressType.Billing, TermGroup_SysContactAddressRowType.PostalAddress, io.BillingPostalAddress),
                CreateImportAddress(TermGroup_SysContactAddressType.Billing, TermGroup_SysContactAddressRowType.Country, io.BillingCountry),

                // Board HQ
                CreateImportAddress(TermGroup_SysContactAddressType.BoardHQ, TermGroup_SysContactAddressRowType.PostalAddress, io.BoardHQAddress),
                CreateImportAddress(TermGroup_SysContactAddressType.BoardHQ, TermGroup_SysContactAddressRowType.Country, io.BoardHQCountry),

                // Visiting
                CreateImportAddress(TermGroup_SysContactAddressType.Visiting, TermGroup_SysContactAddressRowType.StreetAddress, io.VisitingAddress),
                CreateImportAddress(TermGroup_SysContactAddressType.Visiting, TermGroup_SysContactAddressRowType.AddressCO, io.VisitingCoAddress),
                CreateImportAddress(TermGroup_SysContactAddressType.Visiting, TermGroup_SysContactAddressRowType.PostalCode, io.VisitingPostalCode),
                CreateImportAddress(TermGroup_SysContactAddressType.Visiting, TermGroup_SysContactAddressRowType.PostalAddress, io.VisitingPostalAddress),
                CreateImportAddress(TermGroup_SysContactAddressType.Visiting, TermGroup_SysContactAddressRowType.Country, io.VisitingCountry),

                // Delivery
                CreateImportAddress(TermGroup_SysContactAddressType.Delivery, TermGroup_SysContactAddressRowType.Address, io.DeliveryAddress),
                CreateImportAddress(TermGroup_SysContactAddressType.Delivery, TermGroup_SysContactAddressRowType.AddressCO, io.DeliveryCoAddress),
                CreateImportAddress(TermGroup_SysContactAddressType.Delivery, TermGroup_SysContactAddressRowType.PostalCode, io.DeliveryPostalCode),
                CreateImportAddress(TermGroup_SysContactAddressType.Delivery, TermGroup_SysContactAddressRowType.PostalAddress, io.DeliveryPostalAddress),
                CreateImportAddress(TermGroup_SysContactAddressType.Delivery, TermGroup_SysContactAddressRowType.Country, io.DeliveryCountry)
            };
        }

        protected ImportAddress CreateImportAddress(TermGroup_SysContactAddressType addressType, TermGroup_SysContactAddressRowType addressRowType, object obj)
        {
            return new ImportAddress(addressType, addressRowType, StringUtility.GetValue(obj));
        }

        #endregion

        #region ImportAccount

        protected bool IsAccountsValid(AccountStd accountStd, List<AccountInternal> accountInternals, bool allowOnlyAccountInternals, bool update, bool doNotModifyWithEmpty)
        {
            if (update && doNotModifyWithEmpty)
                return true;

            if (allowOnlyAccountInternals)
                return accountStd != null || !accountInternals.IsNullOrEmpty();
            else
                return accountStd != null;
        }

        protected List<ImportAccount> GetImportEmployeeAccounts(CompEntities entities, EmployeeIODTO io, bool useAccountInternalNr = false)
        {
            return new List<ImportAccount>
            {
                CreateImportAccount(entities, (int)EmploymentAccountType.Cost, io.CostAccountStd, useAccountInternalNr, io.CostAccountInternal1, io.CostAccountInternal2, io.CostAccountInternal3, io.CostAccountInternal4, io.CostAccountInternal5),
                CreateImportAccount(entities, (int)EmploymentAccountType.Income, io.IncomeAccountStd, useAccountInternalNr, io.IncomeAccountInternal1, io.IncomeAccountInternal2, io.IncomeAccountInternal3, io.IncomeAccountInternal4, io.IncomeAccountInternal5),
            };
        }

        protected List<ImportAccount> GetSupplierImportAccounts(CompEntities entities, SupplierIO io, int actorCompanyId)
        {
            List<AccountDim> accountDims = AccountManager.GetAccountDimsByCompany(entities, actorCompanyId, onlyInternal: true);

            return new List<ImportAccount>()
            {
                CreateImportAccount(entities, actorCompanyId, accountDims, (int)SupplierAccountType.Credit, io.AccountsPayableAccountNr, io.AccountsPayableAccountInternal1, io.AccountsPayableAccountInternal2, io.AccountsPayableAccountInternal3, io.AccountsPayableAccountInternal4, io.AccountsPayableAccountInternal5),
                CreateImportAccount(entities, actorCompanyId, accountDims, (int)SupplierAccountType.Debit, io.PurchaseAccountNr, io.PurchaseAccountInternal1, io.PurchaseAccountInternal2, io.PurchaseAccountInternal3, io.PurchaseAccountInternal4, io.PurchaseAccountInternal5),
                CreateImportAccount(entities, actorCompanyId, accountDims, (int)SupplierAccountType.VAT, io.VATAccountNr, "","","","",""),
            };
        }

        protected List<ImportAccount> GetCustomerImportAccounts(CompEntities entities, CustomerIO io)
        {
            return new List<ImportAccount>
            {
                CreateImportAccount(entities, (int)CustomerAccountType.Credit, io.AccountsReceivableAccountNr, true, io.AccountsReceivableAccountInternal1, io.AccountsReceivableAccountInternal2, io.AccountsReceivableAccountInternal3, io.AccountsReceivableAccountInternal4, io.AccountsReceivableAccountInternal5),
                CreateImportAccount(entities, (int)CustomerAccountType.Debit, io.SalesAccountNr, true, io.SalesAccountInternal1, io.SalesAccountInternal2, io.SalesAccountInternal3, io.SalesAccountInternal4, io.SalesAccountInternal5),
                CreateImportAccount(entities, (int)CustomerAccountType.VAT, io.VATAccountNr, false),
            };
        }

        protected ImportAccount CreateImportAccount(CompEntities entities, int type, string accountStdNr, bool useAccountInternalNr, params string[] accountInternalNames)
        {
            ImportAccount importAccount = new ImportAccount(type);

            //Get AccountStd
            if (!String.IsNullOrEmpty(accountStdNr))
                importAccount.AccountStd = GetAccountStd(entities, accountStdNr);

            //Get AccountInternals
            if (accountInternalNames != null)
            {
                foreach (string accountInternalName in accountInternalNames)
                {
                    if (string.IsNullOrEmpty(accountInternalName))
                        continue;

                    AccountInternal accountInternal = GetAccountInternal(entities, accountInternalName, useAccountNr: useAccountInternalNr);
                    if (accountInternal != null && !importAccount.AccountInternals.Any(ai => ai.AccountId == accountInternal.AccountId))
                        importAccount.AccountInternals.Add(accountInternal);
                }
            }

            return importAccount;
        }

        protected ImportAccount CreateImportAccount(CompEntities entities, int actorCompanyId, List<AccountDim> accountDims, int type, string accountStdNr, string dim2, string dim3, string dim4, string dim5, string dim6)
        {
            ImportAccount importAccount = new ImportAccount(type);

            //Get AccountStd
            if (!String.IsNullOrEmpty(accountStdNr))
            {
                AccountStd accountStd = GetAccountStd(entities, accountStdNr);
                if (accountStd != null)
                    importAccount.AccountStd = GetAccountStd(entities, accountStdNr);
            }

            #region AccountDims

            int accountDimNr = 2;
            foreach (AccountDim accountDim in accountDims.GetInternals().OrderBy(a => a.AccountDimNr))
            {
                TryAddAccount();
                accountDimNr++;

                void TryAddAccount()
                {
                    Account account = null;
                    if (accountDimNr == 2)
                        account = GetAccount(dim2);
                    if (accountDimNr == 3)
                        account = GetAccount(dim3);
                    if (accountDimNr == 4)
                        account = GetAccount(dim4);
                    if (accountDimNr == 5)
                        account = GetAccount(dim5);
                    if (accountDimNr == 6)
                        account = GetAccount(dim6);
                    if (account?.AccountInternal != null)
                        importAccount.AccountInternals.Add(account.AccountInternal);
                }
                Account GetAccount(string accountNr) => AccountManager.GetAccountByNr(entities, accountNr, accountDim.AccountDimId, actorCompanyId, onlyActive: false, loadAccount: true);
            }

            #endregion

            return importAccount;
        }

        #endregion

        #region ImportAttestRole

        protected List<ImportAttestRole> GetImportAttestRoles(CompEntities entities, EmployeeIODTO io)
        {
            return new List<ImportAttestRole>
            {
                CreateImportAttestRole(entities, io.AttestRoleName1, io.AttestRoleAccount1),
                CreateImportAttestRole(entities, io.AttestRoleName2, io.AttestRoleAccount2),
                CreateImportAttestRole(entities, io.AttestRoleName3, io.AttestRoleAccount3),
                CreateImportAttestRole(entities, io.AttestRoleName4, io.AttestRoleAccount4),
                CreateImportAttestRole(entities, io.AttestRoleName5, io.AttestRoleAccount5)
            };
        }

        private ImportAttestRole CreateImportAttestRole(CompEntities entities, string attestRole, string account)
        {
            return new ImportAttestRole(GetAttestRole(entities, attestRole), GetAccountInternal(entities, account, useExternalCode: true, useAccountNr: true));
        }

        #endregion

        #region ImportCategoryRecord

        protected List<ImportCategoryRecord> GetImportCategoryRecords(CompEntities entities, EmployeeIODTO io)
        {
            return new List<ImportCategoryRecord>
            {
                CreateImportCategoryRecord(entities, io.CategoryCode1),
                CreateImportCategoryRecord(entities, io.CategoryCode2),
                CreateImportCategoryRecord(entities, io.CategoryCode3),
                CreateImportCategoryRecord(entities, io.CategoryCode4),
                CreateImportCategoryRecord(entities, io.CategoryCode5),
            };
        }

        protected List<ImportCategoryRecord> GetImportCategoryRecords(CompEntities entities, params string[] categoryCodes)
        {
            List<ImportCategoryRecord> importCategoryRecords = new List<ImportCategoryRecord>();
            foreach (string categoryCode in categoryCodes)
            {
                importCategoryRecords.Add(CreateImportCategoryRecord(entities, categoryCode));
            }
            return importCategoryRecords;
        }

        protected List<ImportCategoryRecord> GetImportCategoryRecords(CompEntities entities, SoeCategoryType categoryType, params string[] categoryCodes)
        {
            List<ImportCategoryRecord> importCategoryRecords = new List<ImportCategoryRecord>();
            foreach (string categoryCode in categoryCodes)
            {
                importCategoryRecords.Add(CreateImportCategoryRecord(entities, categoryCode, categoryType));
            }
            return importCategoryRecords;
        }

        private ImportCategoryRecord CreateImportCategoryRecord(CompEntities entities, string categoryCode, SoeCategoryType categoryType = SoeCategoryType.Unknown)
        {
            if (categoryType == SoeCategoryType.Unknown)
                return new ImportCategoryRecord(GetCategory(entities, categoryCode));

            return new ImportCategoryRecord(GetCategory(entities, categoryCode, categoryType));
        }

        #endregion

        #region ImportClosestRelative

        protected List<ImportClosestRelative> GetImportClosestRelatives(EmployeeIODTO io)
        {
            List<ImportClosestRelative> closestRelatives = new List<ImportClosestRelative>();

            if (StringUtility.HasAnyValue(io.ClosestRelativeName, io.ClosestRelativeNr, io.ClosestRelativeRelation))
            {
                closestRelatives.Add(new ImportClosestRelative()
                {
                    Name = io.ClosestRelativeName,
                    Nr = io.ClosestRelativeNr,
                    Relation = io.ClosestRelativeRelation,
                });
            }

            if (StringUtility.HasAnyValue(io.ClosestRelativeName2, io.ClosestRelativeNr2, io.ClosestRelativeRelation2))
            {
                closestRelatives.Add(new ImportClosestRelative()
                {
                    Name = io.ClosestRelativeName2,
                    Nr = io.ClosestRelativeNr2,
                    Relation = io.ClosestRelativeRelation2,
                });
            }

            return closestRelatives;
        }

        #endregion

        #region ImportECom

        protected List<ImportECom> GetImportEComs(EmployeeIODTO io)
        {
            return new List<ImportECom>
            {
               CreateImportECom(TermGroup_SysContactEComType.Email, io.Email),
               CreateImportECom(TermGroup_SysContactEComType.PhoneHome, io.PhoneHome),
               CreateImportECom(TermGroup_SysContactEComType.PhoneJob, io.PhoneJob),
               CreateImportECom(TermGroup_SysContactEComType.PhoneMobile, io.PhoneMobile),
            };
        }

        protected List<ImportECom> GetImportEComs(SupplierIO io)
        {
            return new List<ImportECom>
            {
               CreateImportECom(TermGroup_SysContactEComType.Email, io.Email1),
               CreateImportECom(TermGroup_SysContactEComType.Email, io.Email2),
               CreateImportECom(TermGroup_SysContactEComType.PhoneHome, io.PhoneHome),
               CreateImportECom(TermGroup_SysContactEComType.PhoneJob, io.PhoneJob),
               CreateImportECom(TermGroup_SysContactEComType.PhoneMobile, io.PhoneMobile),
               CreateImportECom(TermGroup_SysContactEComType.Fax, io.Fax),
               CreateImportECom(TermGroup_SysContactEComType.Web, io.Webpage)
            };
        }

        protected List<ImportECom> GetImportEComs(CustomerIO io)
        {
            var emailAdded = false;
            var result = new List<ImportECom>();
            
            if (!string.IsNullOrEmpty(io.Email2))
            {
                result.Add(CreateImportECom(TermGroup_SysContactEComType.Email, io.Email2));
                emailAdded = true;
            }

            if (!string.IsNullOrEmpty(io.InvoiceDeliveryEmail) && io.InvoiceDeliveryEmail.ToLower() != io.Email1?.ToLower() && io.InvoiceDeliveryEmail != io.Email2?.ToLower())
            {
                result.Add(CreateImportECom(TermGroup_SysContactEComType.Email, io.InvoiceDeliveryEmail));
                emailAdded = true;
            }

            if (!emailAdded || !string.IsNullOrEmpty(io.Email1))
            {
                result.Insert(0, (CreateImportECom(TermGroup_SysContactEComType.Email, io.Email1)));
            }

            result.Add(CreateImportECom(TermGroup_SysContactEComType.PhoneHome, io.PhoneHome));
            result.Add(CreateImportECom(TermGroup_SysContactEComType.PhoneJob, io.PhoneJob));
            result.Add(CreateImportECom(TermGroup_SysContactEComType.PhoneMobile, io.PhoneMobile));
            result.Add(CreateImportECom(TermGroup_SysContactEComType.Fax, io.Fax));
            result.Add(CreateImportECom(TermGroup_SysContactEComType.Web, io.Webpage));
            return result;
        }

        protected ImportECom CreateImportECom(TermGroup_SysContactEComType type, object obj)
        {
            return new ImportECom((int)type, StringUtility.GetValue(obj));
        }

        #endregion

        #region ImportEmployeeAccount

        protected List<ImportEmployeeAccount> GetEmployeeAccounts(CompEntities entities, EmployeeIODTO io)
        {
            return new List<ImportEmployeeAccount>
            {
                CreateImportEmployeeAccount(entities, io.EmployeeAccount1, io.EmployeeAccountStartDate1, io.EmployeeAccountDefault1),
                CreateImportEmployeeAccount(entities, io.EmployeeAccount2, io.EmployeeAccountStartDate2, io.EmployeeAccountDefault2),
                CreateImportEmployeeAccount(entities, io.EmployeeAccount3, io.EmployeeAccountStartDate3, io.EmployeeAccountDefault3),
            };
        }

        private ImportEmployeeAccount CreateImportEmployeeAccount(CompEntities entities, string employeeAccount, DateTime? startDate, bool? isDefault)
        {
            return new ImportEmployeeAccount(GetAccountInternalForEmployeeAccount(entities, employeeAccount), startDate, isDefault);
        }

        private AccountInternal GetAccountInternalForEmployeeAccount(CompEntities entities, string employeeAccount)
        {
            if (string.IsNullOrEmpty(employeeAccount))
                return null;
            return GetAccountInternal(entities, employeeAccount, useExternalCode: true, useAccountNr: true);
        }

        #endregion

        #region ImportEmployeeFactor

        protected ImportEmployeeFactor GetImportEmployeeFactor(EmployeeIODTO io)
        {
            ImportEmployeeFactor importEmployeeFactor = null;
            if (io != null && io.EmployeeFactorType.HasValue && io.EmployeeFactorFactor.HasValue)
            {
                int type;
                if (EnumUtility.GetValue(io.EmployeeFactorType, out type, typeof(TermGroup_EmployeeFactorType)))
                    importEmployeeFactor = new ImportEmployeeFactor((TermGroup_EmployeeFactorType)type, io.EmployeeFactorFromDate, io.EmployeeFactorFactor.Value);
            }
            return importEmployeeFactor;
        }

        #endregion

        #region ImportEmploymentPriceType

        protected ImportEmploymentPriceType GetImportEmploymentPriceType(EmployeeIODTO io)
        {
            ImportEmploymentPriceType importEmployeePriceType = null;
            if (io != null && !String.IsNullOrEmpty(io.EmploymentPriceTypeCode) && io.EmploymentPriceTypeAmount.HasValue)
                importEmployeePriceType = new ImportEmploymentPriceType(io.EmploymentPriceTypeCode, io.EmploymentPayrollLevelCode, io.EmploymentPriceTypeFromDate, io.EmploymentPriceTypeAmount.Value);
            return importEmployeePriceType;
        }

        #endregion

        #endregion
    }

    #region Help-classes

    public class ImportAddress
    {
        public TermGroup_SysContactAddressType SysContactAddressTypeId { get; set; }
        public TermGroup_SysContactAddressRowType SysContactAddressRowTypeId { get; set; }
        public string Value { get; set; }

        public ImportAddress(TermGroup_SysContactAddressType sysContactAddressTypeId, TermGroup_SysContactAddressRowType sysContactAddressRowTypeId, string value)
        {
            SysContactAddressTypeId = sysContactAddressTypeId;
            SysContactAddressRowTypeId = sysContactAddressRowTypeId;
            Value = value;
        }
    }

    public class ImportAccount
    {
        public int Type { get; set; }
        public decimal Percent { get; set; }
        public AccountStd AccountStd { get; set; }
        public List<AccountInternal> AccountInternals { get; set; }

        public ImportAccount(int type)
        {
            this.Type = type;
            this.Percent = 100;
            this.AccountStd = null;
            this.AccountInternals = new List<AccountInternal>();
        }
    }

    public class ImportAttestRole
    {
        public AttestRole AttestRole { get; set; }
        public AccountInternal AccountInternal { get; set; }

        public ImportAttestRole(AttestRole attestRole, AccountInternal accountInternal)
        {
            this.AttestRole = attestRole;
            this.AccountInternal = accountInternal;
        }
    }

    public class ImportCategoryRecord
    {
        public Category Category { get; set; }

        public ImportCategoryRecord(Category category)
        {
            this.Category = category;
        }
    }

    public class ImportClosestRelative
    {
        public string Nr { get; set; }
        public string Name { get; set; }
        public string Relation { get; set; }
    }

    public class ImportECom
    {
        public int Type { get; set; }
        public string Text { get; set; }

        public ImportECom(int type, string text)
        {
            this.Type = type;
            this.Text = text;
        }
    }

    public class ImportEmployeeAccount
    {
        public AccountInternal AccountInternal { get; set; }
        public DateTime? DateFrom { get; set; }
        public bool IsDefault { get; set; }

        public ImportEmployeeAccount(AccountInternal accountInternal, DateTime? dateFrom, bool? isDefault)
        {
            this.AccountInternal = accountInternal;
            this.DateFrom = dateFrom;
            this.IsDefault = isDefault == true;
        }
    }

    public class ImportEmployeeFactor
    {
        public TermGroup_EmployeeFactorType Type { get; set; }
        public DateTime? FromDate { get; set; }
        public decimal Factor { get; set; }

        public ImportEmployeeFactor(TermGroup_EmployeeFactorType type, DateTime? fromDate, decimal factor)
        {
            this.Type = type;
            this.FromDate = fromDate;
            this.Factor = factor;
        }
    }

    public class ImportEmploymentPriceType
    {
        public string PriceTypeCode { get; set; }
        public string PayrollLevelCode { get; set; }
        public DateTime? FromDate { get; set; }
        public decimal Amount { get; set; }

        public ImportEmploymentPriceType(string priceTypeCode, string payrollLevelCode, DateTime? fromdate, decimal amount)
        {
            this.PriceTypeCode = priceTypeCode;
            this.PayrollLevelCode = payrollLevelCode;
            this.FromDate = fromdate;
            this.Amount = amount;
        }
    }

    public class ImportEmployeeVacationSE
    {
        public decimal? EarnedDaysPaid { get; set; }
        public decimal? EarnedDaysUnpaid { get; set; }

        public decimal? SavedDaysYear1 { get; set; }
        public decimal? SavedDaysYear2 { get; set; }
        public decimal? SavedDaysYear3 { get; set; }
        public decimal? SavedDaysYear4 { get; set; }
        public decimal? SavedDaysYear5 { get; set; }
        public decimal? SavedDaysOverdue { get; set; }

        public decimal? UsedDaysPaid { get; set; }
        public decimal? PaidVacationAllowance { get; set; }
        public decimal? PaidVacationVariableAllowance { get; set; }
        public decimal? UsedDaysUnpaid { get; set; }
        public decimal? UsedDaysAdvance { get; set; }
        public decimal? UsedDaysYear1 { get; set; }
        public decimal? UsedDaysYear2 { get; set; }
        public decimal? UsedDaysYear3 { get; set; }
        public decimal? UsedDaysYear4 { get; set; }
        public decimal? UsedDaysYear5 { get; set; }
        public decimal? UsedDaysOverdue { get; set; }

        public decimal? RemainingDaysPaid { get; set; }
        public decimal? RemainingDaysUnpaid { get; set; }
        public decimal? RemainingDaysAdvance { get; set; }
        public decimal? RemainingDaysYear1 { get; set; }
        public decimal? RemainingDaysYear2 { get; set; }
        public decimal? RemainingDaysYear3 { get; set; }
        public decimal? RemainingDaysYear4 { get; set; }
        public decimal? RemainingDaysYear5 { get; set; }
        public decimal? RemainingDaysOverdue { get; set; }

        public decimal? EarnedDaysRemainingHoursPaid { get; set; }
        public decimal? EarnedDaysRemainingHoursUnpaid { get; set; }
        public decimal? EarnedDaysRemainingHoursAdvance { get; set; }
        public decimal? EarnedDaysRemainingHoursYear1 { get; set; }
        public decimal? EarnedDaysRemainingHoursYear2 { get; set; }
        public decimal? EarnedDaysRemainingHoursYear3 { get; set; }
        public decimal? EarnedDaysRemainingHoursYear4 { get; set; }
        public decimal? EarnedDaysRemainingHoursYear5 { get; set; }
        public decimal? EarnedDaysRemainingHoursOverdue { get; set; }

        public decimal? EmploymentRatePaid { get; set; }
        public decimal? EmploymentRateYear1 { get; set; }
        public decimal? EmploymentRateYear2 { get; set; }
        public decimal? EmploymentRateYear3 { get; set; }
        public decimal? EmploymentRateYear4 { get; set; }
        public decimal? EmploymentRateYear5 { get; set; }
        public decimal? EmploymentRateOverdue { get; set; }

        public decimal? DebtInAdvanceAmount { get; set; }
        public decimal? DebtInAdvanceDueDate { get; set; }
        public decimal? DebtInAdvanceDelete { get; set; }
    }

    public class ImportPriceList
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Currency { get; set; }
        public string ProductNr { get; set; }
        public string ProductName { get; set; }
        public string Price { get; set; }
        public string Quantity { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
    }

    public class ImportAgreement
    {
        public string CustomerNr { get; set; }
        public string AgreementGroup { get; set; }
        public string InternalText { get; set; }
        public string InvoiceLabel { get; set; }
        public string AgreementNr { get; set; }
        public string DeliveryName { get; set; }
        public string DeliveryStreet { get; set; }
        public string DeliveryPostalCode { get; set; }
        public string DeliveryPostalAddress { get; set; }
        public string NextInvoiceDate { get; set; }
        public string[] Categories { get; set; }
        public string StartDate { get; set; }
    }

    #endregion
}
