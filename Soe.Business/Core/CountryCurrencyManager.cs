using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Transactions;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Core
{
    public class CountryCurrencyManager : ManagerBase
    {
        #region Variables

        // Create a logger for use in this class
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private const decimal MAX_CURRENCY_DIFF = 0.05M;

        private readonly ConcurrentDictionary<int, Data.Currency> currencyCache = new ConcurrentDictionary<int, Data.Currency>();
        private readonly ConcurrentDictionary<int, Data.Currency> currencyCacheBySysId = new ConcurrentDictionary<int, Data.Currency>();

        #endregion

        #region Ctor

        public CountryCurrencyManager(ParameterObject parameterObject) : base(parameterObject) { }

        #endregion

        #region SysCountry

        /// <summary>
        /// Get all SysCountry's
        /// Accessor for SysDbCache
        /// </summary>
        /// <returns></returns>
        public List<SysCountry> GetSysCountries(bool populateNameField = false)
        {
            using (var sysEntities = new SOESysEntities())
            {
                using (var transaction = new TransactionScope(TransactionScopeOption.Suppress))
                {
                    sysEntities.SysCountry.AsNoTracking();
                    return GetSysCountries(sysEntities, populateNameField);
                }
            }
        }

        /// <summary>
        /// Get all SysCountry's
        /// Accessor for SysDbCache
        /// </summary>
        /// <returns></returns>
        public List<SysCountry> GetSysCountries(SOESysEntities entities, bool populateNameField = false)
        {
            var sysCountrys = entities.SysCountry.Include("SysCurrency").ToList();
            if (!sysCountrys.IsNullOrEmpty() && populateNameField)
            {
                int langId = GetLangId();
                var terms = base.GetTermGroupDict(TermGroup.SysCountry, langId);
                foreach (SysCountry sysCountry in sysCountrys)
                {
                    sysCountry.Name = terms.ContainsKey(sysCountry.SysTermId) ? terms[sysCountry.SysTermId] : string.Empty;
                }
            }
            return sysCountrys;
        }

        public List<int> GetEUSysCountrieIds(DateTime isEuFrom)
        {
            return SysDbCache.Instance.SysCountrys.Where(c => isEuFrom > c.IsEUFrom && (isEuFrom < c.IsEUTo || c.IsEUTo == null)).Select(c2 => c2.SysCountryId).ToList();
        }

        public bool IsEuCountry(int sysCountryId, DateTime date)
        {
            return SysDbCache.Instance.SysCountrys.Any(c => c.SysCountryId == sysCountryId && date > c.IsEUFrom && (date < c.IsEUTo || c.IsEUTo == null));
        }

        public Dictionary<int, string> GetSysCountriesDict(bool addEmptyRow, bool onlyUsedLanguages = false)
        {
            Dictionary<int, string> dict = new Dictionary<int, string>();

            if (addEmptyRow)
                dict.Add(0, " ");

            List<SysCountry> valiSysCountries = new List<SysCountry>();

            //Uses SysDbCache
            foreach (SysCountry sysCountry in SysDbCache.Instance.SysCountrys)
            {
                if (onlyUsedLanguages && !Enum.IsDefined(typeof(TermGroup_Country), sysCountry.SysTermId))
                    continue;


                sysCountry.Name = GetText(sysCountry.SysTermId, (int)TermGroup.SysCountry);
                valiSysCountries.Add(sysCountry);
            }

            foreach (SysCountry sysCountry in valiSysCountries.OrderBy(i => i.Name))
            {
                dict.Add(sysCountry.SysCountryId, sysCountry.Name + " (" + sysCountry.Code + ")");
            }

            return dict;
        }

        public SysCountryDTO GetSysCountry(int sysCountryId)
        {
            string cacheKey = $"GetSysCountry#sysCountryId{sysCountryId}";
            SysCountryDTO sysCountryDTO = BusinessMemoryCache<SysCountryDTO>.Get(cacheKey);

            if (sysCountryDTO == null)
            {
                using (var sysEntities = new SOESysEntities())
                {
                    using (var transaction = new TransactionScope(TransactionScopeOption.Suppress))
                    {
                        SysCountry sysCountry = (from sc in sysEntities.SysCountry.AsNoTracking()
                                                 where sc.SysCountryId == sysCountryId
                                                 select sc).FirstOrDefault();
                        if (sysCountry != null)
                        {
                            sysCountry.Name = GetText(sysCountry.SysTermId, sysCountry.SysTermGroupId);

                            sysCountryDTO = sysCountry.ToDTO();
                            BusinessMemoryCache<SysCountryDTO>.Set(cacheKey, sysCountryDTO, 120);
                        }
                    }
                }
            }

            return sysCountryDTO;
        }

        public SysCountryDTO GetSysCountry(string sysCountryCode)
        {
            if (string.IsNullOrEmpty(sysCountryCode))
                return null;

            string cacheKey = $"GetSysCountry#sysCountryCode{sysCountryCode}";
            SysCountryDTO sysCountryDTO = BusinessMemoryCache<SysCountryDTO>.Get(cacheKey);

            if (sysCountryDTO == null)
            {
                using (var sysEntities = new SOESysEntities())
                {
                    using (var transaction = new TransactionScope(TransactionScopeOption.Suppress))
                    {
                        SysCountry sysCountry = (from sc in sysEntities.SysCountry
                                                 where sc.Code == sysCountryCode
                                                 select sc).FirstOrDefault();
                        if (sysCountry != null)
                        {
                            sysCountry.Name = GetText(sysCountry.SysTermId, sysCountry.SysTermGroupId);

                            sysCountryDTO = sysCountry.ToDTO();
                            BusinessMemoryCache<SysCountryDTO>.Set(cacheKey, sysCountryDTO, 120);
                        }
                    }
                }
            }

            return sysCountryDTO;
        }

        public string GetCountryCode(int sysCountryId)
        {
            return CountryCurrencyManager.GetSysCountry(sysCountryId)?.Code;
        }

        public string GetDateFormatedForCountry(int sysCountryId, DateTime? date, int? defaultSysCountryId = null)
        {
            if (!date.HasValue)
                return "";

            string cultureCode = string.Empty;
            if (sysCountryId > 0)
            {
                cultureCode = GetSysCountry(sysCountryId)?.CultureCode;

            }

            if (string.IsNullOrEmpty(cultureCode) && defaultSysCountryId.GetValueOrDefault() > 0)
            {
                cultureCode = GetSysCountry(defaultSysCountryId.Value)?.CultureCode;
            }

            if (!string.IsNullOrEmpty(cultureCode))
            {
                var culture = new CultureInfo(cultureCode);
                string dateFormatString = culture.DateTimeFormat.ShortDatePattern;
                return date.Value.ToString(dateFormatString);
            }

            return "";
        }

        public int? GetSysCountryIdFromCompany(int actorCompanyId, int? defaultSysCountryId = null)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            entitiesReadOnly.Company.NoTracking();
            int? sysCountryId = (from c in entitiesReadOnly.Company
                                 where c.ActorCompanyId == actorCompanyId &&
                                 c.State == (int)SoeEntityState.Active
                                 select c.SysCountryId).FirstOrDefault();

            if (!sysCountryId.HasValue && defaultSysCountryId.HasValue)
                sysCountryId = defaultSysCountryId;

            return sysCountryId;
        }

        #endregion

        #region SysCurrency

        /// <summary>
        /// Get all SysCurrency's
        /// Accessor for SysDbCache
        /// </summary>
        /// <returns></returns>
        public List<SysCurrency> GetSysCurrencies()
        {
            using (var entities = new SOESysEntities())
            {
                using (var transaction = new TransactionScope(TransactionScopeOption.Suppress))
                {
                    return entities.SysCurrency.AsNoTracking().ToList();
                }
            }
        }

        public List<SysCurrency> GetSysCurrencies(bool loadName)
        {
            //Uses SysDbCache
            var sysCurrencies = (from sc in SysDbCache.Instance.SysCurrencies
                                 orderby sc.Code
                                 select sc).ToList<SysCurrency>();

            if (loadName)
            {
                foreach (SysCurrency sc in sysCurrencies)
                {
                    try
                    {
                        sc.Name = GetText(sc.SysTermId, (int)TermGroup.SysCurrency);
                    }
                    catch (Exception e)
                    {
                        e.ToString(); //prevent compiler warning

                        //Read from database if Cache not is available
                        sc.Name = GetText(sc.SysTermId, (int)TermGroup.SysCurrency);
                    }
                    sc.Description = sc.Name + " (" + sc.Code + ")";
                }
            }

            return sysCurrencies;
        }

        public Dictionary<int, string> GetSysCurrenciesDict(bool addEmptyRow, params TermGroup_Currency[] currencies)
        {
            var dict = new Dictionary<int, string>();

            if (addEmptyRow)
                dict.Add(0, " ");

            //Uses SysDbCache
            foreach (SysCurrency sysCurrency in SysDbCache.Instance.SysCurrencies)
            {
                if (currencies.Any() && !currencies.Contains((TermGroup_Currency)sysCurrency.SysCurrencyId))
                    continue;

                dict.Add(sysCurrency.SysCurrencyId, sysCurrency.Code);
            }
            return dict;
        }

        public SysCurrency GetSysCurrencyByCurrency(int currencyId, bool loadName)
        {
            SysCurrency sysCurrency = null;

            var sysCurrencyId = GetSysCurrencyId(currencyId);
            if (sysCurrencyId > 0)
                sysCurrency = GetSysCurrency(sysCurrencyId, loadName);

            return sysCurrency;
        }

        public SysCurrency GetSysCurrency(int sysCurrencyId, bool loadName)
        {
            using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
            return GetSysCurrency(sysEntitiesReadOnly, sysCurrencyId, loadName);
        }

        public SysCurrency GetSysCurrency(SOESysEntities entities, int sysCurrencyId, bool loadName)
        {
            SysCurrency currency = (from sc in entities.SysCurrency
                                    where sc.SysCurrencyId == sysCurrencyId
                                    select sc).FirstOrDefault();

            if (currency != null && loadName)
            {
                currency.Name = GetText(currency.SysTermId, (int)TermGroup.SysCurrency);
                currency.Description = currency.Name + " (" + currency.Code + ")";
            }

            return currency;
        }

        public SysCurrency GetSysCurrencyCached(int sysCurrencyId, bool loadName)
        {
            SysCurrency currency = (from sc in SysDbCache.Instance.SysCurrencies
                                    where sc.SysCurrencyId == sysCurrencyId
                                    select sc).FirstOrDefault();

            if (currency != null && loadName)
            {
                currency.Name = GetText(currency.SysTermId, (int)TermGroup.SysCurrency);
                currency.Description = currency.Name + " (" + currency.Code + ")";
            }

            return currency;
        }

        public string GetCurrencyCode(int sysCurrencyId)
        {
            string code = "";
            if (Enum.IsDefined(typeof(TermGroup_Currency), sysCurrencyId))
                code = ((TermGroup_Currency)sysCurrencyId).ToString();
            return code;
        }

        public int GetSysCurrencyId(int currencyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetSysCurrencyId(entities, currencyId);
        }

        public int GetSysCurrencyId(CompEntities entities, int currencyId)
        {
            string cacheKey = $"GetSysCurrencyId#currencyid{currencyId}#actorCompanyId{base.ActorCompanyId}";
            int? sysCurrencyId = BusinessMemoryCache<int?>.Get(cacheKey);

            if (sysCurrencyId.GetValueOrDefault() == 0)
            {
                sysCurrencyId = (from c in entities.Currency
                                 where c.CurrencyId == currencyId
                                 select c.SysCurrencyId).FirstOrDefault();

                BusinessMemoryCache<int?>.Set(cacheKey, sysCurrencyId, 360);
            }

            return sysCurrencyId ?? 0;
        }

        #endregion

        #region SysCurrencyRate

        public SysCurrencyRate GetSysCurrencyRate(SOESysEntities entities, TermGroup_Currency currencyFrom, TermGroup_Currency currencyTo, DateTime? date = null)
        {
            // Same currencies return a rate of 1
            if (currencyFrom == currencyTo)
                return new SysCurrencyRate() { Rate = 1, Date = DateTime.Today };

            if (date.HasValue)
                date = date.Value.Date;

            // Get latest rate for specified currencies
            return (from scr in entities.SysCurrencyRate
                    where scr.SysCurrencyFromId == (int)currencyFrom &&
                    scr.SysCurrencyToId == (int)currencyTo &&
                    (!date.HasValue || scr.Date == date.Value)
                    orderby scr.Date descending
                    select scr).FirstOrDefault();
        }

        public ActionResult SaveSysCurrencyRates(bool saveCompCurrencyRates)
        {
            List<string> currencyCodesFilter = GetUsedCurrencyCodes();

            //Add new sources here
            if (!TryGetCurrencyRatesFromECB(out List<SysCurrencyRateDTO> rates, currencyCodesFilter))
                return new ActionResult((int)ActionResultSave.CurrencyGetRatesFailed, "Get Currency rates failed");

            List<SysCurrencyRate> sysCurrencyRates = new List<SysCurrencyRate>();
            TermGroup_CurrencySource source = TermGroup_CurrencySource.ECB;

            using (SOESysEntities entities = new SOESysEntities())
            {
                foreach (SysCurrencyRateDTO rate in rates)
                {
                    SysCurrencyRate sysCurrencyRateTo = CreateSysCurrencyRate(entities, rate.CurrencyFrom, rate.CurrencyTo, rate.Rate, rate.Date);
                    if (sysCurrencyRateTo != null)
                        sysCurrencyRates.Add(sysCurrencyRateTo);
                }

                var result = SaveChanges(entities);
                if (result.Success && saveCompCurrencyRates)
                    result = SaveCurrencyRates(sysCurrencyRates, source);

                return result;
            }
        }

        private List<SysCurrencyRateDTO> ConvertEuroRates(Dictionary<TermGroup_Currency, decimal> ratesFromEuro, DateTime date)
        {
            List<SysCurrencyRateDTO> rates = new List<SysCurrencyRateDTO>();

            //Sort dictionary
            var ratesFromEuroSorted = new Dictionary<TermGroup_Currency, decimal>();
            foreach (KeyValuePair<TermGroup_Currency, decimal> pair in ratesFromEuro.OrderBy(i => i.Key))
            {
                ratesFromEuroSorted.Add(pair.Key, pair.Value);
            }

            foreach (var pairOuter in ratesFromEuroSorted)
            {
                //From EUR - outer
                rates.Add(new SysCurrencyRateDTO()
                {
                    CurrencyFrom = TermGroup_Currency.EUR,
                    CurrencyTo = pairOuter.Key,
                    Rate = pairOuter.Value,
                    Date = date,
                });

                //From outer - EUR
                rates.Add(new SysCurrencyRateDTO()
                {
                    CurrencyFrom = pairOuter.Key,
                    CurrencyTo = TermGroup_Currency.EUR,
                    Rate = 1 / pairOuter.Value,
                    Date = date,
                });

                foreach (var pairInner in ratesFromEuroSorted)
                {
                    if (pairInner.Key == pairOuter.Key || pairInner.Key == TermGroup_Currency.EUR)
                        continue;

                    //From inner - outer
                    rates.Add(new SysCurrencyRateDTO()
                    {
                        CurrencyFrom = pairInner.Key,
                        CurrencyTo = pairOuter.Key,
                        Rate = ConvertRateFromEuro(ratesFromEuroSorted, pairInner.Key, pairOuter.Key),
                        Date = date,
                    });
                }
            }

            return rates;
        }

        private decimal ConvertRateFromEuro(Dictionary<TermGroup_Currency, decimal> ratesFromEuro, TermGroup_Currency currencyFrom, TermGroup_Currency currencyTo)
        {
            return 1 / (ratesFromEuro[currencyFrom] / ratesFromEuro[currencyTo]);
        }

        private SysCurrencyRate CreateSysCurrencyRate(SOESysEntities entities, TermGroup_Currency currencyFrom, TermGroup_Currency currencyTo, decimal rate, DateTime date)
        {
            SysCurrencyRate sysCurrencyRate = GetSysCurrencyRate(entities, currencyFrom, currencyTo, date);
            if (sysCurrencyRate == null)
            {
                sysCurrencyRate = new SysCurrencyRate()
                {
                    Date = date,
                    Rate = rate,

                    //Set FK
                    SysCurrencyFromId = (int)currencyFrom,
                    SysCurrencyToId = (int)currencyTo,
                };
                SetCreatedPropertiesOnEntity(sysCurrencyRate);
                entities.SysCurrencyRate.Add(sysCurrencyRate);
            }

            return sysCurrencyRate;
        }

        #endregion

        #region Currency amounts

        public void SetCurrencyAmounts(CompEntities entities, int actorCompanyId, AccountDistributionEntryRow entity)
        {
            if (entity == null)
                return;

            //Currencies
            int sysCurrencyIdEnt = GetCompanyBaseEntSysCurrencyId(entities, actorCompanyId);

            //CreditAmount
            entity.CreditAmountEntCurrency = GetCurrencyAmount(entities, entity.CreditAmount, sysCurrencyIdEnt, actorCompanyId);

            //DebitAmount
            entity.DebitAmountEntCurrency = GetCurrencyAmount(entities, entity.DebitAmount, sysCurrencyIdEnt, actorCompanyId);
        }

        public void SetCurrencyAmounts(CompEntities entities, int actorCompanyId, AccountYearBalanceHead entity)
        {
            if (entity == null)
                return;

            //Currencies
            int sysCurrencyIdEnt = GetCompanyBaseEntSysCurrencyId(entities, actorCompanyId);

            //Balance
            entity.BalanceEntCurrency = GetCurrencyAmount(entities, entity.Balance, sysCurrencyIdEnt, actorCompanyId);
        }

        public void SetCurrencyAmounts(CompEntities entities, int actorCompanyId, CustomerInvoiceInterest entity, decimal transactionRate, int ledgerActorId)
        {
            if (entity == null)
                return;

            //Currencies
            int sysCurrencyIdLedger = GetLedgerSysCurrencyId(entities, ledgerActorId);
            int sysCurrencyIdEnt = GetCompanyBaseEntSysCurrencyId(entities, actorCompanyId);

            //Amount
            entity.AmountCurrency = GetCurrencyAmountFromBaseAmount(entity.Amount, transactionRate);
            entity.AmountLedgerCurrency = GetCurrencyAmount(entities, entity.Amount, sysCurrencyIdLedger, actorCompanyId);
            entity.AmountEntCurrency = GetCurrencyAmount(entities, entity.Amount, sysCurrencyIdEnt, actorCompanyId);
        }

        public void SetCurrencyAmounts(CompEntities entities, int actorCompanyId, CustomerInvoiceReminder entity, decimal transactionRate, int ledgerActorId)
        {
            if (entity == null)
                return;

            //Currencies
            int sysCurrencyIdLedger = GetLedgerSysCurrencyId(entities, ledgerActorId);
            int sysCurrencyIdEnt = GetCompanyBaseEntSysCurrencyId(entities, actorCompanyId);

            //Amount
            entity.AmountCurrency = GetCurrencyAmountFromBaseAmount(entity.Amount, transactionRate);
            entity.AmountLedgerCurrency = GetCurrencyAmount(entities, entity.Amount, sysCurrencyIdLedger, actorCompanyId);
            entity.AmountEntCurrency = GetCurrencyAmount(entities, entity.Amount, sysCurrencyIdEnt, actorCompanyId);
        }

        public void SetCurrencyAmounts(CompEntities entities, int actorCompanyId, EdiEntry entity)
        {
            if (entity == null)
                return;

            //Sum
            entity.SumCurrency = GetCurrencyAmountFromBaseAmount(entity.Sum, entity.CurrencyRate);

            //Amount
            entity.SumVatCurrency = GetCurrencyAmountFromBaseAmount(entity.SumVat, entity.CurrencyRate);
        }

        public void SetCurrencyAmounts(CompEntities entities, int actorCompanyId, InventoryLog entity)
        {
            if (entity == null)
                return;

            //Currencies
            int sysCurrencyIdEnt = GetCompanyBaseEntSysCurrencyId(entities, actorCompanyId);

            //Amount
            entity.AmountEntCurrency = GetCurrencyAmount(entities, entity.Amount, sysCurrencyIdEnt, actorCompanyId);
        }

        public void SetCurrencyAmounts(CompEntities entities, int actorCompanyId, PaymentRow entity, bool foreign = false)
        {
            if (entity == null)
                return;

            int actorId = entity.Invoice != null && entity.Invoice.ActorId.HasValue ? entity.Invoice.ActorId.Value : 0;

            //Currencies
            int sysCurrencyIdLedger = GetLedgerSysCurrencyId(entities, actorId);
            int sysCurrencyIdEnt = GetCompanyBaseEntSysCurrencyId(entities, actorCompanyId);

            //Amount
            if (foreign)
                entity.Amount = GetBaseAmountFromCurrencyAmount(entity.AmountCurrency, entity.CurrencyRate);
            else
                entity.AmountCurrency = GetCurrencyAmountFromBaseAmount(entity.Amount, entity.CurrencyRate);
            entity.AmountLedgerCurrency = GetCurrencyAmount(entities, entity.Amount, sysCurrencyIdLedger, actorCompanyId);
            entity.AmountEntCurrency = GetCurrencyAmount(entities, entity.Amount, sysCurrencyIdEnt, actorCompanyId);

            //BankFee
            if (foreign)
                entity.BankFee = GetBaseAmountFromCurrencyAmount(entity.BankFeeCurrency, entity.CurrencyRate);
            else
                entity.BankFeeCurrency = GetCurrencyAmountFromBaseAmount(entity.BankFee, entity.CurrencyRate);
            entity.BankFeeLedgerCurrency = GetCurrencyAmount(entities, entity.BankFee, sysCurrencyIdLedger, actorCompanyId);
            entity.BankFeeEntCurrency = GetCurrencyAmount(entities, entity.BankFee, sysCurrencyIdEnt, actorCompanyId);

            //AmountDiff
            if (foreign)
                entity.AmountDiff = GetBaseAmountFromCurrencyAmount(entity.AmountDiffCurrency, entity.CurrencyRate);
            else
                entity.AmountDiffCurrency = GetCurrencyAmountFromBaseAmount(entity.AmountDiff, entity.CurrencyRate);
            entity.AmountDiffLedgerCurrency = GetCurrencyAmount(entities, entity.AmountDiff, sysCurrencyIdLedger, actorCompanyId);
            entity.AmountDiffEntCurrency = GetCurrencyAmount(entities, entity.AmountDiff, sysCurrencyIdEnt, actorCompanyId);
        }

        public void SetCurrencyAmounts(CompEntities entities, int actorCompanyId, PaymentAccountRow entity, decimal transactionRate, int ledgerActorId, DateTime? date = null)
        {
            if (entity == null)
                return;

            //Currencies
            int sysCurrencyIdLedger = GetLedgerSysCurrencyId(entities, ledgerActorId);
            int sysCurrencyIdEnt = GetCompanyBaseEntSysCurrencyId(entities, actorCompanyId);

            //Amount
            entity.AmountCurrency = GetCurrencyAmountFromBaseAmount(entity.Amount, transactionRate);
            entity.AmountLedgerCurrency = GetCurrencyAmount(entities, entity.Amount, sysCurrencyIdLedger, actorCompanyId, date);
            entity.AmountEntCurrency = GetCurrencyAmount(entities, entity.Amount, sysCurrencyIdEnt, actorCompanyId, date);
        }

        public void SetCurrencyAmounts(CompEntities entities, int actorCompanyId, SupplierInvoice entity, int ledgerActorId)
        {
            if (entity == null)
                return;

            //Currencies
            int sysCurrencyIdLedger = GetLedgerSysCurrencyId(entities, ledgerActorId);
            int sysCurrencyIdEnt = GetCompanyBaseEntSysCurrencyId(entities, actorCompanyId);

            //TotalAmount
            entity.TotalAmountCurrency = GetCurrencyAmountFromBaseAmount(entity.TotalAmount, entity.CurrencyRate);
            entity.TotalAmountLedgerCurrency = GetCurrencyAmount(entities, entity.TotalAmount, sysCurrencyIdLedger, actorCompanyId);
            entity.TotalAmountEntCurrency = GetCurrencyAmount(entities, entity.TotalAmount, sysCurrencyIdEnt, actorCompanyId);

            //VATAmount
            entity.VATAmountCurrency = GetCurrencyAmountFromBaseAmount(entity.VATAmount, entity.CurrencyRate);
            entity.VATAmountLedgerCurrency = GetCurrencyAmount(entities, entity.VATAmount, sysCurrencyIdLedger, actorCompanyId);
            entity.VATAmountEntCurrency = GetCurrencyAmount(entities, entity.VATAmount, sysCurrencyIdEnt, actorCompanyId);

            //PaidAmount
            entity.PaidAmountCurrency = GetCurrencyAmountFromBaseAmount(entity.PaidAmount, entity.CurrencyRate);
            entity.PaidAmountLedgerCurrency = GetCurrencyAmount(entities, entity.PaidAmount, sysCurrencyIdLedger, actorCompanyId);
            entity.PaidAmountEntCurrency = GetCurrencyAmount(entities, entity.PaidAmount, sysCurrencyIdEnt, actorCompanyId);
        }

        public void SetCurrencyAmounts(CompEntities entities, int actorCompanyId, TimeCodeTransaction entity)
        {
            if (entity == null)
                return;

            //Currencies
            int sysCurrencyIdEnt = GetCompanyBaseEntSysCurrencyId(entities, actorCompanyId);

            //Amount
            if (entity.Amount.HasValue)
            {
                entity.AmountCurrency = entity.Amount.Value; //No transaction rate
                entity.AmountLedgerCurrency = entity.Amount.Value; //No transaction rate
                entity.AmountEntCurrency = GetCurrencyAmount(entities, entity.Amount.Value, sysCurrencyIdEnt, actorCompanyId);
            }

            //VatAmount
            if (entity.Vat.HasValue)
            {
                entity.VatCurrency = entity.Vat.Value; //No transaction rate
                entity.VatLedgerCurrency = entity.Vat.Value; //No transaction rate
                entity.VatEntCurrency = GetCurrencyAmount(entities, entity.Vat.Value, sysCurrencyIdEnt, actorCompanyId);
            }

            //UnitPrice
            if (entity.UnitPrice.HasValue)
            {
                entity.UnitPriceCurrency = entity.UnitPrice.Value; //No transaction rate
                entity.UnitPriceLedgerCurrency = entity.UnitPrice.Value; //No transaction rate
                entity.UnitPriceEntCurrency = GetCurrencyAmount(entities, entity.UnitPrice.Value, sysCurrencyIdEnt, actorCompanyId);
            }
        }

        public void SetCurrencyAmounts(CompEntities entities, int actorCompanyId, TimeInvoiceTransaction entity)
        {
            if (entity == null)
                return;

            //Currencies
            int sysCurrencyIdEnt = GetCompanyBaseEntSysCurrencyId(entities, actorCompanyId);

            //Amount
            if (entity.Amount.HasValue)
            {
                entity.AmountCurrency = entity.Amount.Value; //No transaction rate
                entity.AmountLedgerCurrency = entity.Amount.Value; //No transaction rate
                entity.AmountEntCurrency = GetCurrencyAmount(entities, entity.Amount.Value, sysCurrencyIdEnt, actorCompanyId);
            }

            //VatAmount
            if (entity.VatAmount.HasValue)
            {
                entity.VatAmountCurrency = entity.VatAmount.Value; //No transaction rate
                entity.VatAmountLedgerCurrency = entity.VatAmount.Value; //No transaction rate
                entity.VatAmountEntCurrency = GetCurrencyAmount(entities, entity.VatAmount.Value, sysCurrencyIdEnt, actorCompanyId);
            }
        }

        public void SetCurrencyAmounts(CompEntities entities, int actorCompanyId, TimeInvoiceTransaction entity, CustomerInvoiceRow customerInvoiceRow)
        {
            if (entity == null)
                return;

            //Currencies
            int sysCurrencyIdEnt = GetCompanyBaseEntSysCurrencyId(entities, actorCompanyId);

            //Amount
            if (entity.Amount.HasValue)
            {
                entity.AmountCurrency = customerInvoiceRow != null && customerInvoiceRow.AmountCurrency > 0 ? customerInvoiceRow.AmountCurrency : entity.Amount.Value;
                entity.AmountLedgerCurrency = customerInvoiceRow != null && customerInvoiceRow.AmountLedgerCurrency > 0 ? customerInvoiceRow.AmountLedgerCurrency : entity.Amount.Value;
                entity.AmountEntCurrency = GetCurrencyAmount(entities, entity.Amount.Value, sysCurrencyIdEnt, actorCompanyId);
            }

            //VatAmount
            if (entity.VatAmount.HasValue)
            {
                entity.VatAmountCurrency = customerInvoiceRow != null && customerInvoiceRow.VatAmountCurrency > 0 ? customerInvoiceRow.VatAmountCurrency : entity.VatAmount.Value;
                entity.VatAmountLedgerCurrency = customerInvoiceRow != null && customerInvoiceRow.VatAmountLedgerCurrency > 0 ? customerInvoiceRow.VatAmountLedgerCurrency : entity.VatAmount.Value;
                entity.VatAmountEntCurrency = GetCurrencyAmount(entities, entity.VatAmount.Value, sysCurrencyIdEnt, actorCompanyId);
            }
        }

        public void SetCurrencyAmounts(CompEntities entities, int actorCompanyId, TimePayrollTransaction entity)
        {
            if (entity == null)
                return;

            //Currencies
            int sysCurrencyIdEnt = GetCompanyBaseEntSysCurrencyId(entities, actorCompanyId);

            //Amount
            if (entity.UnitPrice.HasValue)
            {
                entity.UnitPriceCurrency = entity.UnitPrice.Value; //No transaction rate
                entity.UnitPriceLedgerCurrency = entity.UnitPrice.Value; //No transaction rate
                entity.UnitPriceEntCurrency = GetCurrencyAmount(entities, entity.UnitPrice.Value, sysCurrencyIdEnt, actorCompanyId);
            }

            //Amount
            if (entity.Amount.HasValue)
            {
                entity.AmountCurrency = entity.Amount.Value; //No transaction rate
                entity.AmountLedgerCurrency = entity.Amount.Value; //No transaction rate
                entity.AmountEntCurrency = GetCurrencyAmount(entities, entity.Amount.Value, sysCurrencyIdEnt, actorCompanyId);
            }

            //VatAmount
            if (entity.VatAmount.HasValue)
            {
                entity.VatAmountCurrency = entity.VatAmount.Value; //No transaction rate
                entity.VatAmountLedgerCurrency = entity.VatAmount.Value; //No transaction rate
                entity.VatAmountEntCurrency = GetCurrencyAmount(entities, entity.VatAmount.Value, sysCurrencyIdEnt, actorCompanyId);
            }
        }

        public void SetCurrencyAmounts(CompEntities entities, int actorCompanyId, TimePayrollScheduleTransaction entity)
        {
            if (entity == null)
                return;

            //Currencies
            int sysCurrencyIdEnt = GetCompanyBaseEntSysCurrencyId(entities, actorCompanyId);

            //Amount
            if (entity.UnitPrice.HasValue)
            {
                entity.UnitPriceCurrency = entity.UnitPrice.Value; //No transaction rate
                entity.UnitPriceLedgerCurrency = entity.UnitPrice.Value; //No transaction rate
                entity.UnitPriceEntCurrency = GetCurrencyAmount(entities, entity.UnitPrice.Value, sysCurrencyIdEnt, actorCompanyId);
            }

            //Amount
            if (entity.Amount.HasValue)
            {
                entity.AmountCurrency = entity.Amount.Value; //No transaction rate
                entity.AmountLedgerCurrency = entity.Amount.Value; //No transaction rate
                entity.AmountEntCurrency = GetCurrencyAmount(entities, entity.Amount.Value, sysCurrencyIdEnt, actorCompanyId);
            }

            //VatAmount
            if (entity.VatAmount.HasValue)
            {
                entity.VatAmountCurrency = entity.VatAmount.Value; //No transaction rate
                entity.VatAmountLedgerCurrency = entity.VatAmount.Value; //No transaction rate
                entity.VatAmountEntCurrency = GetCurrencyAmount(entities, entity.VatAmount.Value, sysCurrencyIdEnt, actorCompanyId);
            }
        }

        public void SetCurrencyAmounts(CompEntities entities, int actorCompanyId, VoucherRow entity)
        {
            if (entity == null)
                return;

            //Currencies
            int sysCurrencyIdEnt = GetCompanyBaseEntSysCurrencyId(entities, actorCompanyId);

            //Amount
            entity.AmountEntCurrency = GetCurrencyAmount(entities, entity.Amount, sysCurrencyIdEnt, actorCompanyId, entity.Date);
        }

        public void SetCurrencyAmounts(CompEntities entities, int actorCompanyId, IVoucherRowDTO entity)
        {
            if (entity == null)
                return;

            //Currencies
            int sysCurrencyIdEnt = GetCompanyBaseEntSysCurrencyId(entities, actorCompanyId);

            //Amount
            entity.AmountEntCurrency = GetCurrencyAmount(entities, entity.Amount, sysCurrencyIdEnt, actorCompanyId, entity.Date);
        }

        public void SetCurrencyAmounts(CompEntities entities, int actorCompanyId, VoucherRowHistory entity)
        {
            if (entity == null)
                return;

            //Currencies
            int sysCurrencyIdEnt = GetCompanyBaseEntSysCurrencyId(entities, actorCompanyId);

            //Amount
            if (entity.Amount.HasValue)
            {
                entity.AmountEntCurrency = GetCurrencyAmount(entities, entity.Amount.Value, sysCurrencyIdEnt, actorCompanyId);
            }
        }

        public void CalculateCurrencyAmounts(CompEntities entities, int actorCompanyId, CustomerInvoice entity, bool useCurrencyProp = false, bool calculateForImport = false, bool recalculateTotals = false)
        {
            if (entity == null)
                return;

            #region Prereq

            int actorId = entity.ActorId ?? 0;

            //Currencies
            int sysCurrencyIdInvoice = GetSysCurrencyId(entities, entity.CurrencyId);
            int sysCurrencyIdBase = GetCompanyBaseSysCurrencyId(entities, actorCompanyId);
            int sysCurrencyIdLedger = GetLedgerSysCurrencyId(entities, actorId);
            int sysCurrencyIdEnt = GetCompanyBaseEntSysCurrencyId(entities, actorCompanyId);
            bool foreign = sysCurrencyIdInvoice != sysCurrencyIdBase;

            //Dates
            DateTime ledgerDate = DateTime.Now;
            if (entity.InvoiceDate.HasValue)
            {
                ledgerDate = entity.InvoiceDate.Value;
            }
            else if (entity.VoucherDate.HasValue)
            {
                ledgerDate = entity.VoucherDate.Value;
            }

            //Rates
            decimal transactionRate = entity.CurrencyRate;
            decimal ledgerRate = GetCurrencyRate(entities, sysCurrencyIdLedger, actorCompanyId, ledgerDate);
            decimal entRate = GetCurrencyRate(entities, sysCurrencyIdEnt, actorCompanyId, ledgerDate);

            #endregion

            if (useCurrencyProp)
            {
                #region CustomerInvoice

                #region From Invoice

                //TotalAmount
                //entity.TotalAmount = GetBaseAmountFromCurrencyAmount(entity.TotalAmountCurrency, transactionRate);
                entity.TotalAmountLedgerCurrency = GetCurrencyAmountFromBaseAmount(entity.TotalAmountCurrency, ledgerRate);
                entity.TotalAmountEntCurrency = GetCurrencyAmountFromBaseAmount(entity.TotalAmountCurrency, entRate);

                //VATAmount
                //entity.VATAmount = GetBaseAmountFromCurrencyAmount(entity.VATAmountCurrency, transactionRate);
                entity.VATAmountLedgerCurrency = GetCurrencyAmountFromBaseAmount(entity.VATAmountCurrency, ledgerRate);
                entity.VATAmountEntCurrency = GetCurrencyAmountFromBaseAmount(entity.VATAmountCurrency, entRate);

                //PaidAmount
                //entity.PaidAmount = GetBaseAmountFromCurrencyAmount(entity.PaidAmountCurrency, transactionRate);
                entity.PaidAmountLedgerCurrency = GetCurrencyAmountFromBaseAmount(entity.PaidAmountCurrency, ledgerRate);
                entity.PaidAmountEntCurrency = GetCurrencyAmountFromBaseAmount(entity.PaidAmountCurrency, entRate);

                #endregion

                #region From CustomerInvoice

                //SumAmount
                //entity.SumAmount = GetBaseAmountFromCurrencyAmount(entity.SumAmountCurrency, transactionRate);
                entity.SumAmountLedgerCurrency = GetCurrencyAmountFromBaseAmount(entity.SumAmountCurrency, ledgerRate);
                entity.SumAmountEntCurrency = GetCurrencyAmountFromBaseAmount(entity.SumAmountCurrency, entRate);

                //FreightAmount
                //entity.FreightAmount = GetBaseAmountFromCurrencyAmount(entity.FreightAmountCurrency, transactionRate);
                entity.FreightAmountLedgerCurrency = GetCurrencyAmountFromBaseAmount(entity.FreightAmountCurrency, ledgerRate);
                entity.FreightAmountEntCurrency = GetCurrencyAmountFromBaseAmount(entity.FreightAmountCurrency, entRate);

                //InvoiceFee
                //entity.InvoiceFee = GetBaseAmountFromCurrencyAmount(entity.InvoiceFeeCurrency, transactionRate);
                entity.InvoiceFeeLedgerCurrency = GetCurrencyAmountFromBaseAmount(entity.InvoiceFeeCurrency, ledgerRate);
                entity.InvoiceFeeEntCurrency = GetCurrencyAmountFromBaseAmount(entity.InvoiceFeeCurrency, entRate);

                //MarginalIncome
                //entity.MarginalIncome = GetBaseAmountFromCurrencyAmount(entity.MarginalIncomeCurrency, transactionRate);
                entity.MarginalIncomeLedgerCurrency = GetCurrencyAmountFromBaseAmount(entity.MarginalIncomeCurrency, ledgerRate);
                entity.MarginalIncomeEntCurrency = GetCurrencyAmountFromBaseAmount(entity.MarginalIncomeCurrency, entRate);

                #endregion

                #endregion
            }
            else
            {
                #region CustomerInvoice

                #region From Invoice

                //TotalAmount
                if (recalculateTotals)
                {
                    if (foreign)
                        entity.TotalAmount = GetBaseAmountFromCurrencyAmount(entity.TotalAmountCurrency, transactionRate);
                    else
                        entity.TotalAmountCurrency = GetCurrencyAmountFromBaseAmount(entity.TotalAmount, transactionRate);
                }
                entity.TotalAmountLedgerCurrency = GetCurrencyAmountFromBaseAmount(entity.TotalAmount, ledgerRate);
                entity.TotalAmountEntCurrency = GetCurrencyAmountFromBaseAmount(entity.TotalAmount, entRate);

                //VATAmount
                if (recalculateTotals)
                {
                    if (foreign)
                        entity.VATAmount = GetBaseAmountFromCurrencyAmount(entity.VATAmountCurrency, transactionRate);
                    else
                        entity.VATAmountCurrency = GetCurrencyAmountFromBaseAmount(entity.VATAmount, transactionRate);
                }
                entity.VATAmountLedgerCurrency = GetCurrencyAmountFromBaseAmount(entity.VATAmount, ledgerRate);
                entity.VATAmountEntCurrency = GetCurrencyAmountFromBaseAmount(entity.VATAmount, entRate);

                //PaidAmount
                if (recalculateTotals)
                {
                    if (foreign)
                        entity.PaidAmount = GetBaseAmountFromCurrencyAmount(entity.PaidAmountCurrency, transactionRate);
                    else
                        entity.PaidAmountCurrency = GetCurrencyAmountFromBaseAmount(entity.PaidAmount, transactionRate);
                }
                entity.PaidAmountLedgerCurrency = GetCurrencyAmountFromBaseAmount(entity.PaidAmount, ledgerRate);
                entity.PaidAmountEntCurrency = GetCurrencyAmountFromBaseAmount(entity.PaidAmount, entRate);

                #endregion

                #region From CustomerInvoice

                //SumAmount
                //if (foreign)
                //entity.SumAmount = GetBaseAmountFromCurrencyAmount(entity.SumAmountCurrency, transactionRate);
                //else
                //entity.SumAmountCurrency = GetCurrencyAmountFromBaseAmount(entity.SumAmount, transactionRate);
                entity.SumAmountLedgerCurrency = GetCurrencyAmountFromBaseAmount(entity.SumAmount, ledgerRate);
                entity.SumAmountEntCurrency = GetCurrencyAmountFromBaseAmount(entity.SumAmount, entRate);

                //FreightAmount
                //if (foreign)
                //entity.FreightAmount = GetBaseAmountFromCurrencyAmount(entity.FreightAmountCurrency, transactionRate);
                //else
                //entity.FreightAmountCurrency = GetCurrencyAmountFromBaseAmount(entity.FreightAmount, transactionRate);
                entity.FreightAmountLedgerCurrency = GetCurrencyAmountFromBaseAmount(entity.FreightAmount, ledgerRate);
                entity.FreightAmountEntCurrency = GetCurrencyAmountFromBaseAmount(entity.FreightAmount, entRate);

                //InvoiceFee
                //if (foreign)
                //entity.InvoiceFee = GetBaseAmountFromCurrencyAmount(entity.InvoiceFeeCurrency, transactionRate);
                //else
                //entity.InvoiceFeeCurrency = GetCurrencyAmountFromBaseAmount(entity.InvoiceFee, transactionRate);
                entity.InvoiceFeeLedgerCurrency = GetCurrencyAmountFromBaseAmount(entity.InvoiceFee, ledgerRate);
                entity.InvoiceFeeEntCurrency = GetCurrencyAmountFromBaseAmount(entity.InvoiceFee, entRate);

                //MarginalIncome
                //if (foreign)
                //entity.MarginalIncome = GetBaseAmountFromCurrencyAmount(entity.MarginalIncomeCurrency, transactionRate);
                //else
                //entity.MarginalIncomeCurrency = GetCurrencyAmountFromBaseAmount(entity.MarginalIncome, transactionRate);
                entity.MarginalIncomeLedgerCurrency = GetCurrencyAmountFromBaseAmount(entity.MarginalIncome, ledgerRate);
                entity.MarginalIncomeEntCurrency = GetCurrencyAmountFromBaseAmount(entity.MarginalIncome, entRate);

                #endregion

                #endregion
            }

            #region CustomerInvoiceRow

            decimal diffBC = 0;
            decimal diffTC = 0;
            decimal diffEC = 0;
            decimal diffLC = 0;
            decimal ledger = 0;
            decimal enterprise = 0;

            entity.LoadCustomerInvoiceAccountRows(entities);

            foreach (CustomerInvoiceRow customerInvoiceRow in entity.ActiveCustomerInvoiceRows)
            {
                //Amount
                if (!customerInvoiceRow.IsCentRoundingRow && calculateForImport)
                {
                    if (foreign)
                        customerInvoiceRow.Amount = GetBaseAmountFromCurrencyAmount(customerInvoiceRow.AmountCurrency, transactionRate, 4);
                    else
                        customerInvoiceRow.AmountCurrency = GetCurrencyAmountFromBaseAmount(customerInvoiceRow.Amount, transactionRate, 4);
                }

                ledger = GetCurrencyAmountFromBaseAmount(customerInvoiceRow.Amount, ledgerRate);
                enterprise = GetCurrencyAmountFromBaseAmount(customerInvoiceRow.Amount, entRate);
                if (ledger != customerInvoiceRow.AmountLedgerCurrency)
                {
                    customerInvoiceRow.AmountLedgerCurrency = ledger;
                }
                if (enterprise != customerInvoiceRow.AmountEntCurrency)
                {
                    customerInvoiceRow.AmountEntCurrency = enterprise;
                }
                //customerInvoiceRow.AmountLedgerCurrency = GetCurrencyAmountFromBaseAmount(customerInvoiceRow.Amount, ledgerRate);
                //customerInvoiceRow.AmountEntCurrency = GetCurrencyAmountFromBaseAmount(customerInvoiceRow.Amount, entRate);

                //VatAmount
                if (calculateForImport)
                {
                    if (foreign)
                        customerInvoiceRow.VatAmount = GetBaseAmountFromCurrencyAmount(customerInvoiceRow.VatAmountCurrency, transactionRate);
                    else
                        customerInvoiceRow.VatAmountCurrency = GetCurrencyAmountFromBaseAmount(customerInvoiceRow.VatAmount, transactionRate);
                }

                ledger = GetCurrencyAmountFromBaseAmount(customerInvoiceRow.VatAmount, ledgerRate);
                enterprise = GetCurrencyAmountFromBaseAmount(customerInvoiceRow.VatAmount, entRate);
                if (ledger != customerInvoiceRow.VatAmountLedgerCurrency)
                {
                    customerInvoiceRow.VatAmountLedgerCurrency = ledger;
                }
                if (enterprise != customerInvoiceRow.VatAmountEntCurrency)
                {
                    customerInvoiceRow.VatAmountEntCurrency = enterprise;
                }

                //customerInvoiceRow.VatAmountLedgerCurrency = GetCurrencyAmountFromBaseAmount(customerInvoiceRow.VatAmount, ledgerRate);
                //customerInvoiceRow.VatAmountEntCurrency = GetCurrencyAmountFromBaseAmount(customerInvoiceRow.VatAmount, entRate);

                if (!customerInvoiceRow.IsCentRoundingRow && calculateForImport)
                {
                    //SumAmount
                    if (foreign)
                        customerInvoiceRow.SumAmount = GetBaseAmountFromCurrencyAmount(customerInvoiceRow.SumAmountCurrency, transactionRate);
                    else
                        customerInvoiceRow.SumAmountCurrency = GetCurrencyAmountFromBaseAmount(customerInvoiceRow.SumAmount, transactionRate);
                }

                ledger = GetCurrencyAmountFromBaseAmount(customerInvoiceRow.SumAmount, ledgerRate);
                enterprise = GetCurrencyAmountFromBaseAmount(customerInvoiceRow.SumAmount, entRate);
                if (ledger != customerInvoiceRow.SumAmountLedgerCurrency)
                {
                    customerInvoiceRow.SumAmountLedgerCurrency = ledger;
                }
                if (enterprise != customerInvoiceRow.SumAmountEntCurrency)
                {
                    customerInvoiceRow.SumAmountEntCurrency = enterprise;
                }

                //customerInvoiceRow.SumAmountEntCurrency = GetCurrencyAmountFromBaseAmount(customerInvoiceRow.SumAmount, ledgerRate);
                //customerInvoiceRow.SumAmountLedgerCurrency = GetCurrencyAmountFromBaseAmount(customerInvoiceRow.SumAmount, entRate);

                //DiscountAmount
                if (calculateForImport)
                {
                    if (foreign)
                        customerInvoiceRow.DiscountAmount = GetBaseAmountFromCurrencyAmount(customerInvoiceRow.DiscountAmountCurrency, transactionRate);
                    else
                        customerInvoiceRow.DiscountAmountCurrency = GetCurrencyAmountFromBaseAmount(customerInvoiceRow.DiscountAmount, transactionRate);
                }

                ledger = GetCurrencyAmountFromBaseAmount(customerInvoiceRow.DiscountAmount, ledgerRate);
                enterprise = GetCurrencyAmountFromBaseAmount(customerInvoiceRow.DiscountAmount, entRate);
                if (ledger != customerInvoiceRow.DiscountAmountLedgerCurrency)
                {
                    customerInvoiceRow.DiscountAmountLedgerCurrency = ledger;
                }
                if (enterprise != customerInvoiceRow.DiscountAmountEntCurrency)
                {
                    customerInvoiceRow.DiscountAmountEntCurrency = enterprise;
                }

                //Discount2Amount
                if (calculateForImport)
                {
                    if (foreign)
                        customerInvoiceRow.Discount2Amount = GetBaseAmountFromCurrencyAmount(customerInvoiceRow.Discount2AmountCurrency, transactionRate);
                    else
                        customerInvoiceRow.Discount2AmountCurrency = GetCurrencyAmountFromBaseAmount(customerInvoiceRow.Discount2Amount, transactionRate);
                }

                ledger = GetCurrencyAmountFromBaseAmount(customerInvoiceRow.Discount2Amount, ledgerRate);
                enterprise = GetCurrencyAmountFromBaseAmount(customerInvoiceRow.Discount2Amount, entRate);

                if (ledger != customerInvoiceRow.Discount2AmountLedgerCurrency)
                {
                    customerInvoiceRow.Discount2AmountLedgerCurrency = ledger;
                }
                if (enterprise != customerInvoiceRow.Discount2AmountEntCurrency)
                {
                    customerInvoiceRow.Discount2AmountEntCurrency = enterprise;
                }

                //customerInvoiceRow.DiscountAmountLedgerCurrency = GetCurrencyAmountFromBaseAmount(customerInvoiceRow.DiscountAmount, ledgerRate);
                //customerInvoiceRow.DiscountAmountEntCurrency = GetCurrencyAmountFromBaseAmount(customerInvoiceRow.DiscountAmount, entRate);

                //PurchasePrice
                if (calculateForImport)
                {
                    if (foreign)
                        customerInvoiceRow.PurchasePrice = GetBaseAmountFromCurrencyAmount(customerInvoiceRow.PurchasePriceCurrency, transactionRate);
                    else
                        customerInvoiceRow.PurchasePriceCurrency = GetCurrencyAmountFromBaseAmount(customerInvoiceRow.PurchasePrice, transactionRate);
                }

                ledger = GetCurrencyAmountFromBaseAmount(customerInvoiceRow.PurchasePrice, ledgerRate);
                enterprise = GetCurrencyAmountFromBaseAmount(customerInvoiceRow.PurchasePrice, entRate);
                if (ledger != customerInvoiceRow.PurchasePriceLedgerCurrency)
                {
                    customerInvoiceRow.PurchasePriceLedgerCurrency = ledger;
                }
                if (enterprise != customerInvoiceRow.PurchasePriceEntCurrency)
                {
                    customerInvoiceRow.PurchasePriceEntCurrency = enterprise;
                }

                //customerInvoiceRow.PurchasePriceLedgerCurrency = GetCurrencyAmountFromBaseAmount(customerInvoiceRow.PurchasePrice, ledgerRate);
                //customerInvoiceRow.PurchasePriceEntCurrency = GetCurrencyAmountFromBaseAmount(customerInvoiceRow.PurchasePrice, entRate);

                //MarginalIncome
                if (calculateForImport)
                {
                    if (foreign)
                        customerInvoiceRow.MarginalIncome = GetBaseAmountFromCurrencyAmount(customerInvoiceRow.MarginalIncomeCurrency, transactionRate);
                    else
                        customerInvoiceRow.MarginalIncomeCurrency = GetCurrencyAmountFromBaseAmount(customerInvoiceRow.MarginalIncome, transactionRate);
                }

                ledger = GetCurrencyAmountFromBaseAmount(customerInvoiceRow.MarginalIncome, ledgerRate);
                enterprise = GetCurrencyAmountFromBaseAmount(customerInvoiceRow.MarginalIncome, entRate);
                if (ledger != customerInvoiceRow.MarginalIncomeLedgerCurrency)
                {
                    customerInvoiceRow.MarginalIncomeLedgerCurrency = ledger;
                }
                if (enterprise != customerInvoiceRow.MarginalIncomeEntCurrency)
                {
                    customerInvoiceRow.MarginalIncomeEntCurrency = enterprise;
                }

                //customerInvoiceRow.MarginalIncomeLedgerCurrency = GetCurrencyAmountFromBaseAmount(customerInvoiceRow.MarginalIncome, ledgerRate);
                //customerInvoiceRow.MarginalIncomeEntCurrency = GetCurrencyAmountFromBaseAmount(customerInvoiceRow.MarginalIncome, entRate);

                #region CustomerInvoiceAccountRow

                foreach (CustomerInvoiceAccountRow customerInvoiceAccountRow in customerInvoiceRow.ActiveCustomerInvoiceAccountRows)
                {
                    if (!customerInvoiceRow.IsCentRoundingRow && calculateForImport)
                    {
                        //Amount
                        if (foreign)
                        {
                            var amount = GetBaseAmountFromCurrencyAmount(customerInvoiceAccountRow.AmountCurrency, transactionRate);
                            if (customerInvoiceAccountRow.Amount != amount)
                            {
                                customerInvoiceAccountRow.Amount = amount;
                            }
                        }
                        else
                        {
                            var amountCurrency = GetCurrencyAmountFromBaseAmount(customerInvoiceAccountRow.Amount, transactionRate);
                            if (customerInvoiceAccountRow.AmountCurrency != amountCurrency)
                            {
                                customerInvoiceAccountRow.AmountCurrency = amountCurrency;
                            }
                        }
                    }

                    ledger = GetCurrencyAmountFromBaseAmount(customerInvoiceAccountRow.Amount, ledgerRate);
                    if (ledger != customerInvoiceAccountRow.AmountLedgerCurrency)
                    {
                        customerInvoiceAccountRow.AmountLedgerCurrency = ledger;
                    }

                    enterprise = GetCurrencyAmountFromBaseAmount(customerInvoiceAccountRow.Amount, entRate);
                    if (enterprise != customerInvoiceAccountRow.AmountEntCurrency)
                    {
                        customerInvoiceAccountRow.AmountEntCurrency = enterprise;
                    }
                }

                #endregion
            }
            /*
            //Skip lazy load and fetch only state=0 accountingrows
            var accountingRows = entities.CustomerInvoiceAccountRow.Include("CustomerInvoiceRow").Where(r => r.CustomerInvoiceRow.InvoiceId == entity.InvoiceId && 
                                                                                                        r.CustomerInvoiceRow.State == (int)SoeEntityState.Active && 
                                                                                                        r.State == (int)SoeEntityState.Active).ToList();
            foreach (CustomerInvoiceAccountRow customerInvoiceAccountRow in accountingRows)
            {
                //if (!customerInvoiceRow.IsCentRoundingRow && calculateForImport)
                if (!customerInvoiceAccountRow.CustomerInvoiceRow.IsCentRoundingRow && calculateForImport)
                {
                    //Amount
                    if (foreign)
                        customerInvoiceAccountRow.Amount = GetBaseAmountFromCurrencyAmount(customerInvoiceAccountRow.AmountCurrency, transactionRate);
                    else
                        customerInvoiceAccountRow.AmountCurrency = GetCurrencyAmountFromBaseAmount(customerInvoiceAccountRow.Amount, transactionRate);
                }

                ledger = GetCurrencyAmountFromBaseAmount(customerInvoiceAccountRow.Amount, ledgerRate);
                enterprise = GetCurrencyAmountFromBaseAmount(customerInvoiceAccountRow.Amount, entRate);
                if (ledger != customerInvoiceAccountRow.AmountLedgerCurrency)
                {
                    customerInvoiceAccountRow.AmountLedgerCurrency = ledger;
                }
                if (enterprise != customerInvoiceAccountRow.AmountEntCurrency)
                {
                    customerInvoiceAccountRow.AmountEntCurrency = enterprise;
                }
            }
            */
            #region Currency diff

            if (diffBC != 0 || diffTC != 0 || diffEC != 0 || diffLC != 0)
            {
                // Get claim account from customer or company setting
                int accountId = 0;
                if (entity.ActorId != null)
                {
                    CustomerAccountStd customerAccount = CustomerManager.GetCustomerAccount(entities, entity.ActorId.Value, CustomerAccountType.Debit);
                    if (customerAccount != null)
                        accountId = customerAccount.AccountStd.AccountId;
                }
                if (accountId == 0)
                    accountId = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.AccountCustomerClaim, 0, actorCompanyId, 0);

                // Get customer claim row to put the diff on
                CustomerInvoiceAccountRow accRow = null;
                if (accountId != 0)
                {
                    foreach (CustomerInvoiceRow customerInvoiceRow in entity.ActiveCustomerInvoiceRows)
                    {
                        accRow = customerInvoiceRow.ActiveCustomerInvoiceAccountRows.FirstOrDefault(r => r.AccountStd.AccountId == accountId);
                        if (accRow != null)
                            break;
                    }
                }
                else
                {
                    CustomerInvoiceRow row = entity.ActiveCustomerInvoiceRows.FirstOrDefault();
                    accRow = row?.ActiveCustomerInvoiceAccountRows.FirstOrDefault();
                }

                if (accRow != null)
                {
                    // Fix diffs
                    if (diffBC != 0 && Math.Abs(diffBC) <= MAX_CURRENCY_DIFF)
                    {
                        if (accRow.Amount < 0)
                            accRow.Amount -= diffBC;
                        else
                            accRow.Amount += diffBC;
                    }
                    if (diffTC != 0 && Math.Abs(diffTC) <= MAX_CURRENCY_DIFF)
                    {
                        if (accRow.AmountCurrency < 0)
                            accRow.AmountCurrency -= diffTC;
                        else
                            accRow.AmountCurrency += diffTC;
                    }
                    if (diffEC != 0 && Math.Abs(diffEC) <= MAX_CURRENCY_DIFF)
                    {
                        if (accRow.AmountEntCurrency < 0)
                            accRow.AmountEntCurrency -= diffEC;
                        else
                            accRow.AmountEntCurrency += diffEC;
                    }
                    if (diffLC != 0 && Math.Abs(diffLC) <= MAX_CURRENCY_DIFF)
                    {
                        if (accRow.AmountLedgerCurrency < 0)
                            accRow.AmountLedgerCurrency -= diffLC;
                        else
                            accRow.AmountLedgerCurrency += diffLC;
                    }
                }
            }

            #endregion

            #endregion
        }

        public void CalculateCurrencyAmounts(CompEntities entities, int actorCompanyId, SupplierInvoice entity)
        {
            if (entity == null)
                return;

            #region Prereq

            int actorId = entity.ActorId.HasValue ? entity.ActorId.Value : 0;

            //Currencies
            int sysCurrencyIdInvoice = GetSysCurrencyId(entities, entity.CurrencyId);
            int sysCurrencyIdBase = GetCompanyBaseSysCurrencyId(entities, actorCompanyId);
            int sysCurrencyIdLedger = GetLedgerSysCurrencyId(entities, actorId);
            int sysCurrencyIdEnt = GetCompanyBaseEntSysCurrencyId(entities, actorCompanyId);
            bool foreign = sysCurrencyIdInvoice != sysCurrencyIdBase;

            //Dates
            DateTime ledgerDate = DateTime.Now;
            if (entity.InvoiceDate.HasValue)
            {
                ledgerDate = entity.InvoiceDate.Value;
            }
            else if (entity.VoucherDate.HasValue)
            {
                ledgerDate = entity.VoucherDate.Value;
            }

            //Rates
            decimal transactionRate = entity.CurrencyRate;
            decimal ledgerRate = GetCurrencyRate(entities, sysCurrencyIdLedger, actorCompanyId, ledgerDate);
            decimal entRate = GetCurrencyRate(entities, sysCurrencyIdEnt, actorCompanyId, ledgerDate);

            #endregion

            #region SupplierInvoice

            #region From Invoice

            //TotalAmount
            if (foreign)
                entity.TotalAmount = GetBaseAmountFromCurrencyAmount(entity.TotalAmountCurrency, transactionRate);
            else
                entity.TotalAmountCurrency = GetCurrencyAmountFromBaseAmount(entity.TotalAmount, transactionRate);
            entity.TotalAmountLedgerCurrency = GetCurrencyAmountFromBaseAmount(entity.TotalAmount, ledgerRate);
            entity.TotalAmountEntCurrency = GetCurrencyAmountFromBaseAmount(entity.TotalAmount, entRate);

            //VATAmount
            if (foreign)
                entity.VATAmount = GetBaseAmountFromCurrencyAmount(entity.VATAmountCurrency, transactionRate);
            else
                entity.VATAmountCurrency = GetCurrencyAmountFromBaseAmount(entity.VATAmount, transactionRate);
            entity.VATAmountLedgerCurrency = GetCurrencyAmountFromBaseAmount(entity.VATAmount, ledgerRate);
            entity.VATAmountEntCurrency = GetCurrencyAmountFromBaseAmount(entity.VATAmount, entRate);

            //PaidAmount
            if (foreign)
                entity.PaidAmount = GetBaseAmountFromCurrencyAmount(entity.PaidAmountCurrency, transactionRate);
            else
                entity.PaidAmountCurrency = GetCurrencyAmountFromBaseAmount(entity.PaidAmount, transactionRate);
            entity.PaidAmountLedgerCurrency = GetCurrencyAmountFromBaseAmount(entity.PaidAmount, ledgerRate);
            entity.PaidAmountEntCurrency = GetCurrencyAmountFromBaseAmount(entity.PaidAmount, entRate);

            #endregion

            #region From SupplierInvoice

            #endregion

            #endregion

            #region SupplierInvoiceRow

            decimal diffBC = 0;
            decimal diffTC = 0;
            decimal diffEC = 0;
            decimal diffLC = 0;

            foreach (SupplierInvoiceRow supplierInvoiceRow in entity.ActiveSupplierInvoiceRows)
            {
                //Amount
                if (foreign)
                    supplierInvoiceRow.Amount = GetBaseAmountFromCurrencyAmount(supplierInvoiceRow.AmountCurrency, transactionRate);
                else
                    supplierInvoiceRow.AmountCurrency = GetCurrencyAmountFromBaseAmount(supplierInvoiceRow.Amount, transactionRate);

                //VatAmount
                if (foreign)
                    supplierInvoiceRow.VatAmount = GetBaseAmountFromCurrencyAmount(supplierInvoiceRow.VatAmountCurrency, transactionRate);
                else
                    supplierInvoiceRow.VatAmountCurrency = GetCurrencyAmountFromBaseAmount(supplierInvoiceRow.VatAmount, transactionRate);

                #region SupplierInvoiceAccountRow

                foreach (SupplierInvoiceAccountRow supplierInvoiceAccountRow in supplierInvoiceRow.ActiveSupplierInvoiceAccountRows)
                {
                    if (!supplierInvoiceAccountRow.AccountStdReference.IsLoaded)
                        supplierInvoiceAccountRow.AccountStdReference.Load();

                    //Amount
                    if (foreign)
                        supplierInvoiceAccountRow.Amount = GetBaseAmountFromCurrencyAmount(supplierInvoiceAccountRow.AmountCurrency, transactionRate);
                    else
                        supplierInvoiceAccountRow.AmountCurrency = GetCurrencyAmountFromBaseAmount(supplierInvoiceAccountRow.Amount, transactionRate);
                    supplierInvoiceAccountRow.AmountLedgerCurrency = GetCurrencyAmountFromBaseAmount(supplierInvoiceAccountRow.Amount, ledgerRate);
                    supplierInvoiceAccountRow.AmountEntCurrency = GetCurrencyAmountFromBaseAmount(supplierInvoiceAccountRow.Amount, entRate);

                    // Get diffs
                    diffBC += supplierInvoiceAccountRow.Amount;
                    diffTC += supplierInvoiceAccountRow.AmountCurrency;
                    diffEC += supplierInvoiceAccountRow.AmountEntCurrency;
                    diffLC += supplierInvoiceAccountRow.AmountLedgerCurrency;
                }

                #endregion
            }

            #region Currency diff

            if (diffBC != 0 || diffTC != 0 || diffEC != 0 || diffLC != 0)
            {
                // Get debt account from supplier or company setting
                int accountId = 0;
                if (entity.ActorId != null)
                {
                    SupplierAccountStd supplierAccount = SupplierManager.GetSupplierAccount(entities, entity.ActorId.Value, SupplierAccountType.Credit);
                    if (supplierAccount != null)
                        accountId = supplierAccount.AccountStd.AccountId;
                }
                if (accountId == 0)
                    accountId = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.AccountSupplierDebt, 0, actorCompanyId, 0);

                // Get supplier debt row to put the diff on
                SupplierInvoiceAccountRow accRow = null;
                if (accountId != 0)
                {
                    foreach (SupplierInvoiceRow supplierInvoiceRow in entity.ActiveSupplierInvoiceRows)
                    {
                        accRow = supplierInvoiceRow.ActiveSupplierInvoiceAccountRows.FirstOrDefault(r => r.AccountStd.AccountId == accountId);
                        if (accRow != null)
                            break;
                    }
                }
                else
                {
                    SupplierInvoiceRow row = entity.ActiveSupplierInvoiceRows.FirstOrDefault();
                    accRow = row?.ActiveSupplierInvoiceAccountRows.FirstOrDefault();
                }

                if (accRow != null)
                {
                    // Fix diffs
                    if (diffBC != 0 && Math.Abs(diffBC) <= MAX_CURRENCY_DIFF)
                    {
                        if (accRow.Amount < 0)
                            accRow.Amount -= diffBC;
                        else
                            accRow.Amount += diffBC;
                    }
                    if (diffTC != 0 && Math.Abs(diffTC) <= MAX_CURRENCY_DIFF)
                    {
                        if (accRow.AmountCurrency < 0)
                            accRow.AmountCurrency -= diffTC;
                        else
                            accRow.AmountCurrency += diffTC;
                    }
                    if (diffEC != 0 && Math.Abs(diffEC) <= MAX_CURRENCY_DIFF)
                    {
                        if (accRow.AmountEntCurrency < 0)
                            accRow.AmountEntCurrency -= diffEC;
                        else
                            accRow.AmountEntCurrency += diffEC;
                    }
                    if (diffLC != 0 && Math.Abs(diffLC) <= MAX_CURRENCY_DIFF)
                    {
                        if (accRow.AmountLedgerCurrency < 0)
                            accRow.AmountLedgerCurrency -= diffLC;
                        else
                            accRow.AmountLedgerCurrency += diffLC;
                    }
                }
            }

            #endregion

            #endregion
        }

        public void CalculateCurrencyAmounts(CompEntities entities, int actorCompanyId, Purchase entity)
        {
            if (entity == null)
                return;

            #region Prereq

            int actorId = entity.SupplierId.GetValueOrDefault();

            //Currencies
            int sysCurrencyIdInvoice = GetSysCurrencyId(entities, entity.CurrencyId);
            int sysCurrencyIdBase = GetCompanyBaseSysCurrencyId(entities, actorCompanyId);
            int sysCurrencyIdLedger = GetLedgerSysCurrencyId(entities, actorId);
            int sysCurrencyIdEnt = GetCompanyBaseEntSysCurrencyId(entities, actorCompanyId);

            //Dates
            DateTime ledgerDate = DateTime.Now;
            if (entity.PurchaseDate.HasValue)
            {
                ledgerDate = entity.PurchaseDate.Value;
            }


            //Rates
            decimal transactionRate = entity.CurrencyRate;
            decimal ledgerRate = GetCurrencyRate(entities, sysCurrencyIdLedger, actorCompanyId, ledgerDate);
            decimal entRate = GetCurrencyRate(entities, sysCurrencyIdEnt, actorCompanyId, ledgerDate);

            #endregion

            #region Purchase

            //TotalAmount
            //if (foreign)
            //    entity.TotalAmount = GetBaseAmountFromCurrencyAmount(entity.TotalAmountCurrency, transactionRate);
            //else
            //    entity.TotalAmountCurrency = GetCurrencyAmountFromBaseAmount(entity.TotalAmount, transactionRate);
            entity.TotalAmount = GetBaseAmountFromCurrencyAmount(entity.TotalAmountCurrency, transactionRate);
            entity.TotalAmountLedgerCurrency = GetCurrencyAmountFromBaseAmount(entity.TotalAmount, ledgerRate);
            entity.TotalAmountEntCurrency = GetCurrencyAmountFromBaseAmount(entity.TotalAmount, entRate);

            //VATAmount
            //if (foreign)
            //    entity.VATAmount = GetBaseAmountFromCurrencyAmount(entity.VATAmountCurrency, transactionRate);
            //else
            //    entity.VATAmountCurrency = GetCurrencyAmountFromBaseAmount(entity.VATAmount, transactionRate);
            entity.VATAmount = GetBaseAmountFromCurrencyAmount(entity.VATAmountCurrency, transactionRate);
            entity.VATAmountLedgerCurrency = GetCurrencyAmountFromBaseAmount(entity.VATAmount, ledgerRate);
            entity.VATAmountEntCurrency = GetCurrencyAmountFromBaseAmount(entity.VATAmount, entRate);

            #endregion

            #region PurchaseRow

            foreach (var purchaseRow in entity.PurchaseRow.Where(r => r.State == (int)SoeEntityState.Active))
            {
                //Amount
                /*
                if (foreign)
                    purchaseRow.Amount = GetBaseAmountFromCurrencyAmount(purchaseRow.AmountCurrency, transactionRate, 4);
                else
                    purchaseRow.AmountCurrency = GetCurrencyAmountFromBaseAmount(purchaseRow.Amount, transactionRate, 4);
                purchaseRow.AmountLedgerCurrency = GetCurrencyAmountFromBaseAmount(purchaseRow.Amount, ledgerRate);
                purchaseRow.AmountEntCurrency = GetCurrencyAmountFromBaseAmount(purchaseRow.Amount, entRate);
                */

                //VatAmount
                //if (foreign)
                //    purchaseRow.VatAmount = GetBaseAmountFromCurrencyAmount(purchaseRow.VatAmountCurrency, transactionRate);
                //else
                //    purchaseRow.VatAmountCurrency = GetCurrencyAmountFromBaseAmount(purchaseRow.VatAmount, transactionRate);
                purchaseRow.VatAmount = GetBaseAmountFromCurrencyAmount(purchaseRow.VatAmountCurrency, transactionRate);
                purchaseRow.VatAmountLedgerCurrency = GetCurrencyAmountFromBaseAmount(purchaseRow.VatAmount, ledgerRate);
                purchaseRow.VatAmountEntCurrency = GetCurrencyAmountFromBaseAmount(purchaseRow.VatAmount, entRate);

                //SumAmount
                //if (foreign)
                //    purchaseRow.SumAmount = GetBaseAmountFromCurrencyAmount(purchaseRow.SumAmountCurrency, transactionRate);
                //else
                //    purchaseRow.SumAmountCurrency = GetCurrencyAmountFromBaseAmount(purchaseRow.SumAmount, transactionRate);
                purchaseRow.SumAmount = GetBaseAmountFromCurrencyAmount(purchaseRow.SumAmountCurrency, transactionRate);
                purchaseRow.SumAmountEntCurrency = GetCurrencyAmountFromBaseAmount(purchaseRow.SumAmount, ledgerRate);
                purchaseRow.SumAmountLedgerCurrency = GetCurrencyAmountFromBaseAmount(purchaseRow.SumAmount, entRate);

                //DiscountAmount
                //if (foreign)
                //    purchaseRow.DiscountAmount = GetBaseAmountFromCurrencyAmount(purchaseRow.DiscountAmountCurrency, transactionRate);
                //else
                //    purchaseRow.DiscountAmountCurrency = GetCurrencyAmountFromBaseAmount(purchaseRow.DiscountAmount, transactionRate);
                purchaseRow.DiscountAmount = GetBaseAmountFromCurrencyAmount(purchaseRow.DiscountAmountCurrency, transactionRate);
                purchaseRow.DiscountAmountLedgerCurrency = GetCurrencyAmountFromBaseAmount(purchaseRow.DiscountAmount, ledgerRate);
                purchaseRow.DiscountAmountEntCurrency = GetCurrencyAmountFromBaseAmount(purchaseRow.DiscountAmount, entRate);

                //PurchasePrice
                //if (foreign)
                //    purchaseRow.PurchasePrice = GetBaseAmountFromCurrencyAmount(purchaseRow.PurchasePriceCurrency, transactionRate);
                //else
                //    purchaseRow.PurchasePriceCurrency = GetCurrencyAmountFromBaseAmount(purchaseRow.PurchasePrice, transactionRate);
                purchaseRow.PurchasePrice = GetBaseAmountFromCurrencyAmount(purchaseRow.PurchasePriceCurrency, transactionRate);
                purchaseRow.PurchasePriceLedgerCurrency = GetCurrencyAmountFromBaseAmount(purchaseRow.PurchasePrice, ledgerRate);
                purchaseRow.PurchasePriceEntCurrency = GetCurrencyAmountFromBaseAmount(purchaseRow.PurchasePrice, entRate);
            }

            #endregion
        }
        public void CalculateCurrencyAmountsPurchaseRowsFromOrder(CompEntities entities, List<PurchaseRowDTO> rows, int actorCompanyId, int supplierCurrencyId, decimal transactionRate, bool useForeign)
        {
            int sysCurrencyIdInvoice = GetSysCurrencyId(entities, supplierCurrencyId);
            int sysCurrencyIdBase = GetCompanyBaseSysCurrencyId(entities, actorCompanyId);
            bool foreign = useForeign || sysCurrencyIdInvoice != sysCurrencyIdBase;

            foreach (var row in rows)
            {
                //VatAmount
                if (foreign)
                    row.VatAmount = GetBaseAmountFromCurrencyAmount(row.VatAmountCurrency, transactionRate);
                else
                    row.VatAmountCurrency = GetCurrencyAmountFromBaseAmount(row.VatAmount, transactionRate);

                //SumAmount
                if (foreign)
                    row.SumAmount = GetBaseAmountFromCurrencyAmount(row.SumAmountCurrency, transactionRate);
                else
                    row.SumAmountCurrency = GetCurrencyAmountFromBaseAmount(row.SumAmount, transactionRate);

                //DiscountAmount
                if (foreign)
                    row.DiscountAmount = GetBaseAmountFromCurrencyAmount(row.DiscountAmountCurrency, transactionRate);
                else
                    row.DiscountAmountCurrency = GetCurrencyAmountFromBaseAmount(row.DiscountAmount, transactionRate);

                //PurchasePrice
                if (foreign)
                    row.PurchasePrice = GetBaseAmountFromCurrencyAmount(row.PurchasePriceCurrency, transactionRate);
                else
                    row.PurchasePriceCurrency = GetCurrencyAmountFromBaseAmount(row.PurchasePrice, transactionRate);
            }
        }

        #endregion

        #region CompCurrency

        /// <summary>
        /// This method will return all currencies for a company
        /// </summary>
        /// <param name="actorCompanyId">The ActorCompanyId</param>
        /// <param name="checkForUpdates">True if it should check for new updated rates</param>
        /// <returns>List of CompCurrency's</returns>
        public List<CompCurrency> GetCompCurrencies(int actorCompanyId, bool loadRates)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetCompCurrencies(entities, actorCompanyId, loadRates);
        }

        /// <summary>
        /// This method will return all currencies for a company
        /// </summary>
        /// <param name="entities">The ObjectContext</param>
        /// <param name="actorCompanyId">The ActorCompanyId</param>
        /// <returns>List of CompCurrency's</returns>
        public List<CompCurrency> GetCompCurrencies(CompEntities entities, int actorCompanyId, bool loadRates)
        {
            entities.CompCurrency.NoTracking();
            List<CompCurrency> currencies = (from c in entities.CompCurrency
                                             where c.ActorCompanyId == actorCompanyId
                                             orderby c.BaseCurrency descending
                                             select c).ToList();

            if (loadRates)
            {
                List<CompCurrencyRate> rates = GetCompCurrencyRates(entities, actorCompanyId);
                foreach (CompCurrency currency in currencies)
                {
                    currency.CompCurrencyRates = rates.Where(i => i.CurrencyId == currency.CurrencyId).OrderBy(i => i.Date).ToList();
                    foreach (CompCurrencyRate rate in currency.CompCurrencyRates)
                    {
                        rate.IntervalTypeName = GetText(rate.IntervalType, (int)TermGroup.CurrencyIntervalType);
                        rate.SourceName = GetText(rate.Source, (int)TermGroup.CurrencySource);
                    }
                }
            }

            return HandleCompCurrencies(currencies).ToList();
        }

        public Dictionary<int, string> GetCompCurrenciesDict(int actorCompanyId, bool addEmptyRow, bool nameAsDisplay = true)
        {
            Dictionary<int, string> dict = new Dictionary<int, string>();

            if (addEmptyRow)
                dict.Add(0, " ");

            List<CompCurrency> currencies = GetCompCurrencies(actorCompanyId, false);
            foreach (var currency in currencies)
            {
                dict.Add(currency.CurrencyId, nameAsDisplay ? currency.Name : currency.Code);
            }

            return dict;
        }

        /// <summary>
        /// Get the latest currency for a company
        /// </summary>
        /// <param name="entities">The ObjectContext</param>
        /// <param name="currencyCode">The SysCurrency to get</param>
        /// <param name="actorCompanyId">The ActorCompanyId</param>
        /// <param name="checkForUpdates">True if it should check for new updated rates</param>
        /// <returns>A CompCurrency</returns>
        public CompCurrency GetCompCurrency(CompEntities entities, string code, int actorCompanyId)
        {
            List<CompCurrency> currencys = (from c in entities.CompCurrency
                                            where c.ActorCompanyId == actorCompanyId
                                            select c).ToList();

            return HandleCompCurrencies(currencys).FirstOrDefault(c => c.Code == code);
        }

        public CompCurrencyDTO GetCompCurrencyDTO(CompEntities entities, string code, int actorCompanyId)
        {
            int langId = GetLangId();
            string cacheKey = $"GetCompCurrency#code{code}#actorCompanyId{actorCompanyId}#langId{langId}";
            CompCurrencyDTO compCurrencyDTO = BusinessMemoryCache<CompCurrencyDTO>.Get(cacheKey);

            if (compCurrencyDTO == null)
            {
                var compCurrency = GetCompCurrency(entities, code, actorCompanyId);
                if (compCurrency != null)
                {
                    compCurrencyDTO = compCurrency.ToDTO(false);
                    BusinessMemoryCache<CompCurrencyDTO>.Set(cacheKey, compCurrencyDTO, 120);
                }
            }

            return compCurrencyDTO;
        }

        public CompCurrency GetCompCurrency(int sysCurrencyId, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.CompCurrency.NoTracking();
            return GetCompCurrency(entities, sysCurrencyId, actorCompanyId);
        }

        public CompCurrency GetCompCurrency(CompEntities entities, int sysCurrencyId, int actorCompanyId)
        {
            return HandleCompCurrencies((from c in entities.CompCurrency
                                         where c.ActorCompanyId == actorCompanyId &&
                                         c.SysCurrencyId == sysCurrencyId
                                         orderby c.BaseCurrency descending
                                         select c).ToList()).FirstOrDefault();
        }

        /// <summary>
        /// Get companys base currency
        /// </summary>
        /// <param name="actorCompanyId"></param>
        /// <returns>A CompCurrency</returns>
        public CompCurrency GetCompanyBaseCurrency(int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.CompCurrency.NoTracking();
            return GetCompanyBaseCurrency(entities, actorCompanyId);
        }

        /// <summary>
        /// Get companys base currency
        /// </summary>
        /// <param name="entities">The ObjectContext</param>
        /// <param name="actorCompanyId"></param>
        /// <returns>A CompCurrency</returns>
        public CompCurrency GetCompanyBaseCurrency(CompEntities entities, int actorCompanyId)
        {
            return HandleCompCurrencies((from c in entities.CompCurrency
                                         where c.ActorCompanyId == actorCompanyId &&
                                         c.BaseCurrency
                                         select c).ToList()).FirstOrDefault();
        }

        public CompCurrencyDTO GetCompanyBaseCurrencyDTO(CompEntities entities, int actorCompanyId)
        {
            int langId = GetLangId();
            string cacheKey = $"GetCompanyBaseCurrency#actorCompanyId{actorCompanyId}#langId{langId}";
            CompCurrencyDTO compCurrencyDTO = BusinessMemoryCache<CompCurrencyDTO>.Get(cacheKey);

            if (compCurrencyDTO == null)
            {
                var compCurrency = GetCompanyBaseCurrency(entities, actorCompanyId);
                if (compCurrency != null)
                {
                    compCurrencyDTO = compCurrency.ToDTO(false);
                    BusinessMemoryCache<CompCurrencyDTO>.Set(cacheKey, compCurrencyDTO, 120);
                }
            }

            return compCurrencyDTO;
        }


        /// <summary>
        /// Get given company currency
        /// </summary>
        /// <param name="actorCompanyId"></param>
        /// <returns>A CompCurrency</returns>
        public CompCurrency GetCompanyCurrency(int currencyId, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.CompCurrency.NoTracking();
            return GetCompanyCurrency(entities, currencyId, actorCompanyId);
        }

        /// <summary>
        /// Get given company currency
        /// </summary>
        /// <param name="entities">The ObjectContext</param>
        /// <param name="actorCompanyId"></param>
        /// <returns>A CompCurrency</returns>
        public CompCurrency GetCompanyCurrency(CompEntities entities, int currencyId, int actorCompanyId)
        {
            return HandleCompCurrency((from c in entities.CompCurrency
                                       where c.ActorCompanyId == actorCompanyId &&
                                       c.CurrencyId == currencyId
                                       select c).FirstOrDefault());
        }

        public CompCurrencyDTO GetCompanyCurrencyDTO(CompEntities entities, int currencyId, int actorCompanyId)
        {
            int langId = GetLangId();

            string cacheKey = $"GetCompanyCurrency#actorCompanyId{actorCompanyId}#currencyId{currencyId}#langId{langId}";
            CompCurrencyDTO compCurrencyDTO = BusinessMemoryCache<CompCurrencyDTO>.Get(cacheKey);

            if (compCurrencyDTO == null)
            {
                compCurrencyDTO = HandleCompCurrency((from c in entities.CompCurrency
                                                      where c.ActorCompanyId == actorCompanyId &&
                                                      c.CurrencyId == currencyId
                                                      select c).FirstOrDefault()).ToDTO(false);
                if (compCurrencyDTO != null)
                {
                    BusinessMemoryCache<CompCurrencyDTO>.Set(cacheKey, compCurrencyDTO, 120);
                }
            }
            return compCurrencyDTO;
        }

        public List<CompCurrency> HandleCompCurrencies(List<CompCurrency> currencies)
        {
            int langId = GetLangId();
            var sysCurrencies = base.GetTermGroupDict(TermGroup.SysCurrency, langId);

            foreach (var currency in currencies.Where(c => c.SysCurrencyId > 0))
            {
                currency.Code = CountryCurrencyManager.GetCurrencyCode(currency.SysCurrencyId);
                currency.Name = currency.SysCurrencyId != 0 && sysCurrencies.Keys.Contains(currency.SysCurrencyId) ? sysCurrencies[currency.SysCurrencyId] : "exclude";
            }

            return currencies.Where(c => c.SysCurrencyId > 0 && c.Name != "exclude").OrderByDescending(c => c.BaseCurrency).ThenBy(c => c.Code).ToList();
        }

        public CompCurrency HandleCompCurrency(CompCurrency currency)
        {
            int langId = GetLangId();
            var sysCurrencies = base.GetTermGroupDict(TermGroup.SysCurrency, langId);

            currency.Code = CountryCurrencyManager.GetCurrencyCode(currency.SysCurrencyId);
            currency.Name = currency.SysCurrencyId != 0 && sysCurrencies.Keys.Contains(currency.SysCurrencyId) ? sysCurrencies[currency.SysCurrencyId] : "";

            return currency;
        }

        #region Company base currency

        public Data.Currency GetCurrencyFromType(int id, TermGroup_CurrencyType currencyType)
        {
            int sysCurrId = 0;

            switch (currencyType)
            {
                case TermGroup_CurrencyType.BaseCurrency:
                    sysCurrId = GetCompanyBaseSysCurrencyId(id);
                    break;
                case TermGroup_CurrencyType.TransactionCurrency:
                    sysCurrId = GetCompanyBaseEntSysCurrencyId(id);
                    break;
                case TermGroup_CurrencyType.LedgerCurrency:
                    sysCurrId = GetLedgerSysCurrencyId(id);
                    break;
                case TermGroup_CurrencyType.EnterpriseCurrency:
                    sysCurrId = GetCompanyBaseEntSysCurrencyId(id);
                    break;
            }

            return GetCurrencyFromSysId(sysCurrId);
        }

        public List<string> GetUsedCurrencyCodes()
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            var ids = entitiesReadOnly.CompCurrency.Select(i => i.SysCurrencyId).Distinct().ToList();
            var result = new List<string>();
            ids.ForEach(sysCurrencyId =>
            {
                result.Add(GetCurrencyCode(sysCurrencyId));
            });

            return result.Where(s => !string.IsNullOrEmpty(s)).ToList();
        }

        private readonly Dictionary<int, int> settingCoreBaseCurrencyBydCompany = new Dictionary<int, int>();

        public int GetCompanyBaseSysCurrencyId(int actorCompanyId)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetCompanyBaseSysCurrencyId(entitiesReadOnly, actorCompanyId);
        }
        public int GetCompanyBaseSysCurrencyId(CompEntities entities, int actorCompanyId)
        {
            if (!this.settingCoreBaseCurrencyBydCompany.ContainsKey(actorCompanyId))
                this.settingCoreBaseCurrencyBydCompany.Add(actorCompanyId, SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.CoreBaseCurrency, 0, actorCompanyId, 0));
            return this.settingCoreBaseCurrencyBydCompany[actorCompanyId];
        }

        #endregion

        #region Company base enterprise currency

        private readonly Dictionary<int, int> settingCoreBaseEntCurrency = new Dictionary<int, int>();
        public int GetCompanyBaseEntSysCurrencyId(int actorCompanyId)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetCompanyBaseEntSysCurrencyId(entitiesReadOnly, actorCompanyId);
        }
        public int GetCompanyBaseEntSysCurrencyId(CompEntities entities, int actorCompanyId)
        {
            if (!this.settingCoreBaseEntCurrency.ContainsKey(actorCompanyId))
                this.settingCoreBaseEntCurrency.Add(actorCompanyId, SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.CoreBaseEntCurrency, 0, actorCompanyId, 0));
            return this.settingCoreBaseEntCurrency[actorCompanyId];
        }

        public CompCurrency GetCompanyBaseEntCurrency(int actorCompanyId)
        {
            int sysCurrencyId = GetCompanyBaseEntSysCurrencyId(actorCompanyId);
            return GetCompCurrency(sysCurrencyId, actorCompanyId);
        }

        #endregion

        public CompCurrency GetLedgerCurrency(int actorCompanyId, int actorId)
        {
            int sysCurrencyId = GetLedgerSysCurrencyId(actorId);
            return GetCompCurrency(sysCurrencyId, actorCompanyId);
        }


        public int GetLedgerSysCurrencyId(int actorId)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetLedgerSysCurrencyId(entitiesReadOnly, actorId);
        }
        public int GetLedgerSysCurrencyId(CompEntities entities, int actorId)
        {
            int sysCurrencyId = 0;
            if (actorId > 0)
            {
                Actor actor = ActorManager.GetActor(entities, actorId, true);
                if (actor != null)
                {
                    int currencyId = 0;
                    switch (actor.ActorType)
                    {
                        case (int)SoeActorType.Customer:
                            currencyId = actor.Customer.CurrencyId;
                            break;
                        case (int)SoeActorType.Supplier:
                            currencyId = actor.Supplier.CurrencyId;
                            break;
                    }

                    sysCurrencyId = GetSysCurrencyId(entities, currencyId);
                }
            }

            return sysCurrencyId;
        }


        #endregion

        #region CompCurrencyRate

        public List<CompCurrencyRate> GetCompCurrencyRates(CompEntities entities, int actorCompanyId)
        {
            var sysCurrencies = GetSysCurrencies(true);

            entities.CompCurrencyRate.NoTracking();
            var rates = (from ccr in entities.CompCurrencyRate
                         where ccr.CurrencyRateId.HasValue &&
                         ccr.ActorCompanyId == actorCompanyId
                         select ccr).ToList();

            foreach (var rate in rates)
            {
                var sysCurrency = sysCurrencies.FirstOrDefault(c => c.SysCurrencyId == rate.SysCurrencyId);
                if (sysCurrency != null)
                {
                    rate.Code = sysCurrency.Code;
                    rate.Name = sysCurrency.Name;
                }
            }

            return rates;
        }

        public List<CompCurrencyRate> GetCompCurrencyRates(int actorCompanyId, int currencyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetCompCurrencyRates(entities, actorCompanyId, currencyId);
        }

        public List<CompCurrencyRate> GetCompCurrencyRates(CompEntities entities, int actorCompanyId, int currencyId)
        {
            var sysCurrencies = GetSysCurrencies(true);

            entities.CompCurrencyRate.NoTracking();
            var rates = (from ccr in entities.CompCurrencyRate
                         where ccr.CurrencyRateId.HasValue &&
                         ccr.CurrencyId == currencyId &&
                         ccr.ActorCompanyId == actorCompanyId
                         select ccr).OrderByDescending(x => x.Date).ToList();

            foreach (CompCurrencyRate rate in rates)
            {
                rate.IntervalTypeName = GetText(rate.IntervalType, (int)TermGroup.CurrencyIntervalType);
                rate.SourceName = GetText(rate.Source, (int)TermGroup.CurrencySource);

                var sysCurrency = sysCurrencies.FirstOrDefault(c => c.SysCurrencyId == rate.SysCurrencyId);
                if (sysCurrency != null)
                {
                    rate.Code = sysCurrency.Code;
                    rate.Name = sysCurrency.Name;
                }
            }

            return rates;
        }

        private CompCurrencyRate GetCompCurrencyRate(CompEntities entities, int sysCurrencyId, int actorCompanyId, DateTime? date = null)
        {
            var rate = (from crr in entities.CompCurrencyRate
                        where crr.ActorCompanyId == actorCompanyId &&
                        crr.SysCurrencyId == sysCurrencyId &&
                        (!date.HasValue || crr.Date <= date.Value)
                        orderby crr.Date descending
                        select crr).FirstOrDefault();

            if (rate != null)
            {
                var sysCurrency = GetSysCurrencyCached(rate.SysCurrencyId, true);
                if (sysCurrency != null)
                {
                    rate.Code = sysCurrency.Code;
                    rate.Name = sysCurrency.Name;
                }
            }

            return rate;
        }

        public decimal GetCurrencyRate(int actorCompanyId, int sysCurrencyId, DateTime? date = null, bool rateToBase = true)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.CompCurrencyRate.NoTracking();
            return GetCurrencyRate(entities, sysCurrencyId, actorCompanyId, date, rateToBase);
        }

        public decimal GetCurrencyRate(CompEntities entities, int sysCurrencyId, int actorCompanyId, DateTime? date = null, bool rateToBase = true)
        {
            decimal rate = 1;
            if (sysCurrencyId > 0)
            {
                var currencyRate = GetCompCurrencyRate(entities, sysCurrencyId, actorCompanyId, date);
                if (currencyRate != null)
                    rate = rateToBase ? currencyRate.RateToBase : currencyRate.RateFromBase;
            }
            return rate;
        }

        public decimal GetCurrencyRateFromCurrencyId(int currencyId, int actorCompanyId, DateTime date)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.CompCurrencyRate.NoTracking();
            return GetCurrencyRateFromCurrencyId(entities, currencyId, actorCompanyId, date);
        }

        public decimal GetCurrencyRateFromCurrencyId(CompEntities entities, int currencyId, int actorCompanyId, DateTime date)
        {
            var rate = (from crr in entities.CompCurrencyRate
                        where crr.ActorCompanyId == actorCompanyId &&
                        crr.CurrencyId == currencyId &&
                        crr.Date <= date
                        orderby crr.Date descending
                        select crr).FirstOrDefault();



            return rate?.RateToBase ?? 1;
        }

        public bool CurrencyRateExists(int currencyId, int actorCompanyId, DateTime date)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.CompCurrencyRate.NoTracking();
            return CurrencyRateExists(entities, currencyId, actorCompanyId, date);
        }

        public bool CurrencyRateExists(CompEntities entities, int currencyId, int actorCompanyId, DateTime date)
        {
            return (from crr in entities.CurrencyRate
                    where crr.Currency.ActorCompanyId == actorCompanyId &&
                    crr.CurrencyId == currencyId &&
                    crr.Date == date
                    select crr).Any();
        }

        public decimal GetCurrencyAmount(CompEntities entities, decimal baseAmount, int sysCurrencyId, int actorCompanyId, DateTime? date = null, bool rateToBase = true)
        {
            if (baseAmount == 0)
                return 0;

            decimal rate = GetCurrencyRate(entities, sysCurrencyId, actorCompanyId, date, rateToBase);
            if (rate == 0)
                rate = 1;

            return NumberUtility.GetFormattedDecimalValue(baseAmount / rate, 2);
        }

        public decimal GetCurrencyAmountFromBaseAmount(decimal baseAmount, decimal rate, int round = 2)
        {
            if (rate == 0)
                rate = 1;

            return NumberUtility.GetFormattedDecimalValue(baseAmount / rate, round);
        }

        public decimal GetBaseAmountFromCurrencyAmount(decimal currencyAmount, decimal rate, int round = 2)
        {
            return NumberUtility.GetFormattedDecimalValue(currencyAmount * rate, round);
        }

        #endregion

        #region Currency

        public List<Data.Currency> GetCurrencies(int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Currency.NoTracking();
            return GetCurrencies(entities, actorCompanyId);
        }

        public List<Data.Currency> GetCurrencies(CompEntities entities, int actorCompanyId)
        {
            return (from c in entities.Currency
                    where c.ActorCompanyId == actorCompanyId
                    select c).ToList();
        }

        public List<Data.Currency> GetCurrenciesWithSysCurrency(int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Currency.NoTracking();
            return GetCurrenciesWithSysCurrency(entities, actorCompanyId);
        }

        public List<Data.Currency> GetCurrenciesWithSysCurrency(CompEntities entities, int actorCompanyId)
        {
            var currencies = new List<Data.Currency>();
            var sysCurrencies = GetSysCurrencies(true);

            var currenciesForCompany = (from c in entities.Currency
                                        where c.ActorCompanyId == actorCompanyId
                                        select c).ToList();

            foreach (SysCurrency sysCurrency in sysCurrencies)
            {
                foreach (var currency in currenciesForCompany)
                {
                    if (sysCurrency.SysCurrencyId != currency.SysCurrencyId)
                        continue;

                    currency.Code = sysCurrency.Code;
                    currency.Name = sysCurrency.Name;
                    currencies.Add(currency);
                    break;
                }
            }
            SetIntervalName(currencies);

            //TODO: col.orderby()
            return currencies;
        }

        public Data.Currency GetCurrency(int currencyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Currency.NoTracking();
            return GetCurrency(entities, currencyId);
        }

        public Data.Currency GetCurrency(CompEntities entities, int currencyId)
        {
            if (currencyId == 0)
                return null;

            if (currencyCache.TryGetValue(currencyId, out Data.Currency currency))
                return currency;

            currency = (from c in entities.Currency
                        where c.CurrencyId == currencyId
                        select c).FirstOrDefault();

            currencyCache.TryAdd(currencyId, currency);

            return currency;
        }

        public Data.Currency GetCurrencyWithCode(int currencyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Currency.NoTracking();
            return GetCurrencyWithCode(entities, currencyId);
        }

        public Data.Currency GetCurrencyWithCode(CompEntities entities, int currencyId)
        {
            Data.Currency currency = GetCurrency(entities, currencyId);

            if (currency != null && currency.SysCurrencyId > 0)
            {
                var sysCurrency = GetSysCurrency(currency.SysCurrencyId, true);
                if (sysCurrency != null)
                {
                    currency.Code = sysCurrency.Code;
                    currency.Name = sysCurrency.Name;
                }
            }
            SetIntervalName(currency);

            return currency;
        }

        public Data.Currency GetCurrencyFromSysId(int sysCurrencyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Currency.NoTracking();
            return GetCurrencyFromSysId(entities, sysCurrencyId);
        }

        public Data.Currency GetCurrencyFromSysId(CompEntities entities, int sysCurrencyId)
        {
            if (currencyCacheBySysId.TryGetValue(sysCurrencyId, out Data.Currency currency))
                return currency;

            currency = (from c in entities.Currency
                        where c.SysCurrencyId == sysCurrencyId
                        select c).FirstOrDefault();

            if (currency != null)
            {
                currency.Code = GetCurrencyCode(sysCurrencyId);
                currencyCacheBySysId.TryAdd(sysCurrencyId, currency);
            }
            SetIntervalName(currency);

            return currency;
        }
        private void SetIntervalName(Data.Currency currency)
        {
            if (currency == null)
                return;

            var termsDict = GetTermGroupDict(TermGroup.CurrencyIntervalType);
            currency.IntervalName = termsDict.ContainsKey(currency.IntervalType) ?
                termsDict[currency.IntervalType] : "";
        }
        private void SetIntervalName(IEnumerable<Data.Currency> currency)
        {
            if (currency == null)
                return;

            var termsDict = GetTermGroupDict(TermGroup.CurrencyIntervalType);
            foreach (var c in currency)
            {
                c.IntervalName = termsDict.ContainsKey(c.IntervalType) ? termsDict[c.IntervalType] : "";
            }
        }

        public Data.Currency GetCurrencyAndRateById(int currencyId, int actorCompanyId, bool loadSysCurrency = true)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Currency.NoTracking();
            return GetCurrencyAndRateById(entities, currencyId, actorCompanyId, loadSysCurrency);
        }

        public Data.Currency GetCurrencyAndRateBySysId(int sysCurrencyId, int actorCompanyId, bool loadSysCurrency = true)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Currency.NoTracking();
            return GetCurrencyAndRateBySysId(entities, sysCurrencyId, actorCompanyId, loadSysCurrency);
        }

        public Data.Currency GetCurrencyAndRateById(CompEntities entities, int currencyId, int actorCompanyId, bool loadSysCurrency = true)
        {
            var currency = (from c in entities.Currency
                              .Include("CurrencyRate")
                            where c.ActorCompanyId == actorCompanyId &&
                            c.CurrencyId == currencyId
                            select c).FirstOrDefault();

            if (currency == null)
            {
                return currency;
            }
            SetRateSourceName(currency.CurrencyRate);
            return GetCurrencyAndRateInternal(currency, loadSysCurrency);
        }

        public Data.Currency GetCurrencyAndRateBySysId(CompEntities entities, int sysCurrencyId, int actorCompanyId, bool loadSysCurrency = true)
        {
            var currency = (from c in entities.Currency
                               .Include("CurrencyRate")
                            where c.ActorCompanyId == actorCompanyId &&
                            c.SysCurrencyId == sysCurrencyId
                            select c).FirstOrDefault();

            if (currency == null)
            {
                return currency;
            }
            SetRateSourceName(currency.CurrencyRate);
            return GetCurrencyAndRateInternal(currency, loadSysCurrency);
        }

        public void SetRateSourceName(IEnumerable<Data.CurrencyRate> rates)
        {
            var termsDict = GetTermGroupDict(TermGroup.CurrencySource);
            foreach (var rate in rates)
            {
                rate.SourceName = termsDict.ContainsKey(rate.Source) ? termsDict[rate.Source] : "";
            }
        }

        public Data.Currency GetCurrencyAndRateInternal(Data.Currency currency, bool loadSysCurrency)
        {
            if (loadSysCurrency)
            {
                var sysCurrency = GetSysCurrency(currency.SysCurrencyId, true);
                if (sysCurrency != null)
                {
                    currency.Code = sysCurrency.Code;
                    currency.Name = sysCurrency.Name;
                    currency.SysTermId = sysCurrency.SysTermId;
                }
            }

            return currency;
        }

        public Data.Currency GetCurrencyAndRate(int sysCurrencyId, int actorCompanyId, bool loadSysCurrency = true)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Currency.NoTracking();
            return GetCurrencyAndRate(entities, sysCurrencyId, actorCompanyId, loadSysCurrency);
        }

        public Data.Currency GetCurrencyAndRate(CompEntities entities, int sysCurrencyId, int actorCompanyId, bool loadSysCurrency = true)
        {
            var currency = (from c in entities.Currency
                                .Include("CurrencyRate")
                            where c.ActorCompanyId == actorCompanyId &&
                            c.SysCurrencyId == sysCurrencyId
                            select c).FirstOrDefault();

            if (currency == null)
            {
                return currency;
            }

            if (loadSysCurrency)
            {
                var sysCurrency = GetSysCurrency(currency.SysCurrencyId, true);
                if (sysCurrency != null)
                {
                    currency.Code = sysCurrency.Code;
                    currency.Name = sysCurrency.Name;
                    currency.SysTermId = sysCurrency.SysTermId;
                }
            }

            return currency;
        }

        public Data.Currency GetCurrencyAndRateBySysCurrency(int sysCurrencyId, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Currency.NoTracking();
            return GetCurrencyAndRateBySysCurrency(entities, sysCurrencyId, actorCompanyId);
        }

        public Data.Currency GetCurrencyAndRateBySysCurrency(CompEntities entities, int sysCurrencyId, int actorCompanyId)
        {
            var currency = (from c in entities.Currency
                                .Include("CurrencyRate")
                            where c.ActorCompanyId == actorCompanyId &&
                            c.SysCurrencyId == sysCurrencyId
                            select c).FirstOrDefault();

            if (currency != null)
            {
                var sysCurrency = GetSysCurrency(currency.SysCurrencyId, true);
                if (sysCurrency != null)
                {
                    currency.Code = sysCurrency.Code;
                    currency.Name = sysCurrency.Name;
                    currency.SysTermId = sysCurrency.SysTermId;
                }
            }

            return currency;
        }

        public Data.Currency GetPrevNextCurrency(int currencyId, int actorCompanyId, SoeFormMode mode)
        {
            // Get all currencies
            List<CompCurrency> currencies = GetCompCurrencies(actorCompanyId, false);

            // Get index of current currency
            int i = 0;
            foreach (CompCurrency currency in currencies)
            {
                if (currency.CurrencyId == currencyId)
                    break;
                i++;
            }

            if (mode == SoeFormMode.Next && i < currencies.Count - 1)
                i++;
            else if (mode == SoeFormMode.Prev && i > 0)
                i--;

            return GetCurrencyAndRate(currencies.ElementAt(i).CurrencyId, actorCompanyId);
        }

        public Data.Currency GetCurrencyFromCountry(int actorCompanyId, int sysCountryId)
        {
            // Get SysCurrency
            using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
            SysCurrency sysCurrency = (from sc in sysEntitiesReadOnly.SysCountry
                                       where sc.SysCountryId == sysCountryId
                                       select sc.SysCurrency).FirstOrDefault();

            Data.Currency currency = null;
            if (sysCurrency != null)
            {
                // Get Currency
                using var entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
                entitiesReadOnly.Currency.NoTracking();
                currency = (from c in entitiesReadOnly.Currency
                            where c.ActorCompanyId == actorCompanyId &&
                            c.SysCurrencyId == sysCurrency.SysCurrencyId
                            select c).FirstOrDefault();
            }

            return currency;
        }

        public ActionResult AddCurrency(Data.Currency currency, DateTime date, int actorCompanyId, decimal? rateToBase = null, decimal? rateFromBase = null)
        {
            if (currency == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "Currency");

            using (CompEntities entities = new CompEntities())
            {
                // Get Company
                currency.Company = CompanyManager.GetCompany(entities, actorCompanyId);
                if (currency.Company == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte"));

                if (currency.DefineRateManually && rateToBase.HasValue && rateFromBase.HasValue)
                    CreateCurrencyRate(entities, currency, date, rateFromBase, rateToBase, TermGroup_CurrencySource.Manually, actorCompanyId);

                return AddEntityItem(entities, currency, "Currency");
            }
        }

        public ActionResult AddCurrency(CompEntities entities, Data.Currency currency, DateTime date, int actorCompanyId, decimal? rateToBase = null, decimal? rateFromBase = null)
        {
            if (currency == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "Currency");

            // Get Company
            currency.Company = CompanyManager.GetCompany(entities, actorCompanyId);
            if (currency.Company == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte"));

            if (currency.DefineRateManually && rateToBase.HasValue && rateFromBase.HasValue)
                CreateCurrencyRate(entities, currency, date, rateFromBase, rateToBase, TermGroup_CurrencySource.Manually, actorCompanyId);

            return AddEntityItem(entities, currency, "Currency");
        }

        public ActionResult UpdateCurrency(Data.Currency currency, DateTime date, int actorCompanyId, decimal? rateToBase = null, decimal? rateFromBase = null)
        {
            if (currency == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "Currency");

            using (CompEntities entities = new CompEntities())
            {
                // Get original condition
                var originalCurrency = GetCurrencyAndRateById(entities, currency.CurrencyId, actorCompanyId, false);
                if (originalCurrency == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "Currency");

                originalCurrency.IntervalType = currency.IntervalType;
                originalCurrency.UseSysRate = currency.UseSysRate;

                if (currency.DefineRateManually && rateToBase.HasValue)
                    CreateCurrencyRate(entities, originalCurrency, date.Date, rateFromBase, rateToBase, TermGroup_CurrencySource.Manually, actorCompanyId);

                return SaveEntityItem(entities, originalCurrency);
            }
        }
        public ActionResult SaveCurrency(CurrencyDTO currencyDto)
        {
            ActionResult result = new ActionResult();
            if (currencyDto == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "Currency");

            if (currencyDto.SysCurrencyId == 0)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "Currency.SysCurrencyId");

            using (var entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();
                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        Currency currency = GetCurrencyAndRate(entities, currencyDto.SysCurrencyId, this.ActorCompanyId, false);

                        if (currency == null)
                        {
                            currency = new Currency()
                            {
                                SysCurrencyId = currencyDto.SysCurrencyId,
                                ActorCompanyId = this.ActorCompanyId,
                            };
                            SetCreatedProperties(currency);
                            entities.Currency.AddObject(currency);
                        }
                        else
                        {
                            SetModifiedProperties(currency);
                        }

                        currency.IntervalType = (int)currencyDto.IntervalType;
                        currency.UseSysRate =
                            currencyDto.IntervalType == TermGroup_CurrencyIntervalType.None ||
                            currencyDto.IntervalType == TermGroup_CurrencyIntervalType.Manually ?
                            0 : 1;

                        #region CurrencyRates
                        foreach (var currencyRateDto in currencyDto.CurrencyRates.OrderByDescending(r => r.DoDelete))
                        {
                            CurrencyRate currencyRate = currencyRateDto.CurrencyRateId > 0 ?
                                currency.CurrencyRate.FirstOrDefault(r => r.CurrencyRateId == currencyRateDto.CurrencyRateId) :
                                null;

                            if (currencyRateDto.DoDelete)
                            {
                                if (currencyRate != null)
                                    entities.CurrencyRate.DeleteObject(currencyRate);
                                continue;
                            }

                            if (!currencyRateDto.IsModified)
                                continue;

                            if (currencyRate == null)
                            {
                                if (currencyRateDto.Date == null)
                                    return new ActionResult((int)ActionResultSave.EntityIsNull, "CurrencyRate.Date");

                                var date = currencyRateDto.Date.Date;

                                //Only one rate per day is allowed
                                if (currency.CurrencyRate.Any(r =>
                                    r.EntityState != EntityState.Deleted &&
                                    r.CurrencyRateId != currencyRateDto.CurrencyRateId &&
                                    r.Date != null &&
                                    r.Date.Value.Date == date))
                                    return new ActionResult((int)ActionResultSave.Duplicate, "CurrencyRate.Date");

                                currencyRate = new CurrencyRate()
                                {
                                    CurrencyId = currency.CurrencyId,
                                    Date = date,
                                    Source = (int)currencyRateDto.Source,
                                };
                                currency.CurrencyRate.Add(currencyRate);
                            }
                            else if (currencyRate.Source != (int)TermGroup_CurrencySource.Manually)
                                continue;

                            //We only allow edits of rates, not dates or other fields - that requires a new rate.
                            currencyRate.RateToBase = decimal.Round(
                                currencyRateDto.RateToBase > 0 ? currencyRateDto.RateToBase : 0,
                                4);
                            currencyRate.RateFromBase = decimal.Round(
                                currencyRateDto.RateToBase > 0 ? 1 / currencyRateDto.RateToBase : 0,
                                4);

                        }
                        result = SaveChanges(entities);

                        if (result.Success)
                        {
                            result.IntegerValue = currency.CurrencyId;
                            transaction.Complete();
                        }
                        #endregion
                    }
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    result = new ActionResult(ex);
                    result.IntegerValue = 0;
                }
            }
            return result;
        }

        public CurrencyRate CreateCurrencyRate(CompEntities entities, Data.Currency currency, DateTime date, decimal? rateFromBase, decimal? rateToBase, TermGroup_CurrencySource source, int actorCompanyId, bool ignoreCheck = false)
        {
            DateTime currentDate = date.Date;
            DateTime nextDate = date.Date.AddDays(1);

            CurrencyRate currencyRate = (from cr in entities.CurrencyRate
                                         where cr.Date.HasValue &&
                                         (cr.Date.Value >= currentDate && cr.Date.Value < nextDate) && //Workaround because .Date doesnt work i LINQ
                                         cr.Currency.SysCurrencyId == currency.SysCurrencyId &&
                                         cr.Currency.ActorCompanyId == actorCompanyId
                                         orderby cr.Date descending
                                         select cr).FirstOrDefault();

            if (currencyRate == null)
            {
                currencyRate = new CurrencyRate()
                {
                    Date = date,
                    Source = (int)source,

                    //Set references
                    Currency = currency,
                };
                entities.CurrencyRate.AddObject(currencyRate);
            }

            currencyRate.RateFromBase = rateFromBase;
            currencyRate.RateToBase = rateToBase;

            return currencyRate;
        }

        public ActionResult CreateCurrencyRate(int currencyId, DateTime date, decimal? rateFromBase, decimal? rateToBase, TermGroup_CurrencySource source, int actorCompanyId, bool ignoreCheck = false)
        {
            using (CompEntities entities = new CompEntities())
            {
                // Get original condition
                var originalCurrency = GetCurrencyAndRateById(entities, currencyId, actorCompanyId, false);
                if (originalCurrency == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "Currency");

                CurrencyRate currencyRate = new CurrencyRate()
                {
                    Date = date,
                    Source = (int)source,
                    RateFromBase = rateFromBase,
                    RateToBase = rateToBase,

                    //Set references
                    Currency = originalCurrency,
                };

                entities.CurrencyRate.AddObject(currencyRate);

                return SaveEntityItem(entities, currencyRate);
            }
        }
        public ActionResult DeleteCurrency(int actorCompanyId, int currencyId)
        {
            if (currencyId == 0)
                return new ActionResult((int)ActionResultDelete.EntityIsNull, "Currency");

            using (CompEntities entities = new CompEntities())
            {
                Data.Currency currency = GetCurrency(entities, currencyId);
                if (currency.ActorCompanyId != actorCompanyId)
                    return new ActionResult((int)ActionResultDelete.EntityNotFound, "Company");

                if (currency == null)
                    return new ActionResult((int)ActionResultDelete.EntityNotFound);

                var result = DeleteEntityItem(entities, currency);
                if (!result.Success)
                    return new ActionResult((int)ActionResultDelete.NothingDeleted, GetText(3217, "Valuta kunde inte tas bort"));

                return result;
            }
        }
        public ActionResult DeleteCurrency(Data.Currency currency)
        {
            if (currency == null)
                return new ActionResult((int)ActionResultDelete.EntityIsNull, "Currency");

            using (CompEntities entities = new CompEntities())
            {
                Data.Currency orginalCurrency = GetCurrency(entities, currency.CurrencyId);
                if (orginalCurrency == null)
                    return new ActionResult((int)ActionResultDelete.EntityNotFound);

                return DeleteEntityItem(entities, orginalCurrency);
            }
        }

        #endregion

        #region CurrencyRate

        public ActionResult SaveCurrencyRates(List<SysCurrencyRate> sysCurrencyRates, TermGroup_CurrencySource source)
        {
            ActionResult result = new ActionResult(true);

            if (sysCurrencyRates == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "SysCurrencyRate");

            int nrOfCompanyCurrencies = 0;

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        #region Prereq

                        List<int> handledCompanies = new List<int>();
                        List<CurrencyRate> currencyRates = new List<CurrencyRate>();

                        //Validate all UserCompanySettings
                        List<UserCompanySetting> userCompanySettings = SettingManager.GetAllCompanySettings(entities, (int)CompanySettingType.AccountingCurrencyIntervalType);
                        foreach (UserCompanySetting userCompanySetting in userCompanySettings)
                        {
                            //Mandatory values
                            if (!userCompanySetting.IntData.HasValue || !userCompanySetting.ActorCompanyId.HasValue)
                                continue;

                            int companyIntervalTypeId = userCompanySetting.IntData.Value;
                            int actorCompanyId = userCompanySetting.ActorCompanyId.Value;

                            //Only handle each Company once
                            if (handledCompanies.Contains(actorCompanyId))
                                continue;
                            else
                                handledCompanies.Add(actorCompanyId);

                            int baseCurrencyId = CountryCurrencyManager.GetCompanyBaseSysCurrencyId(entities, actorCompanyId);
                            if (baseCurrencyId > 0)
                            {
                                List<Data.Currency> currencies = GetCurrencies(entities, actorCompanyId);
                                foreach (Data.Currency currency in currencies)
                                {
                                    if (currency.SysCurrencyId == 0 || currency.SysCurrencyId == baseCurrencyId)
                                        continue;

                                    if (IsIntervalTypeValidForUpdate(currency.IntervalType, companyIntervalTypeId))
                                    {
                                        //SysCurrencyRate
                                        SysCurrencyRate sysCurrencyRateFromBase = sysCurrencyRates.FirstOrDefault(i => i.SysCurrencyFromId == baseCurrencyId && i.SysCurrencyToId == currency.SysCurrencyId);
                                        SysCurrencyRate sysCurrencyRateToBase = sysCurrencyRates.FirstOrDefault(i => i.SysCurrencyFromId == currency.SysCurrencyId && i.SysCurrencyToId == baseCurrencyId);
                                        if (sysCurrencyRateFromBase != null && sysCurrencyRateToBase != null)
                                        {
                                            //CurrencyRate
                                            CurrencyRate currencyRate = CreateCurrencyRate(entities, currency, DateTime.Now, sysCurrencyRateFromBase.Rate, sysCurrencyRateToBase.Rate, source, actorCompanyId);
                                            if (currencyRate != null)
                                            {
                                                currencyRates.Add(currencyRate);
                                                nrOfCompanyCurrencies++;
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        #endregion

                        #region Save

                        result = SaveChanges(entities, transaction);

                        //Commit transaction
                        if (result.Success)
                            transaction.Complete();

                        #endregion
                    }
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    result.Exception = ex;
                }
                finally
                {
                    if (result.Success)
                    {
                        result.IntegerValue = nrOfCompanyCurrencies;
                    }
                    else
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }

                return result;
            }
        }

        private bool IsIntervalTypeValidForUpdate(int currencyIntervalTypeId, int companyIntervalTypeId)
        {
            bool valid = false;

            //Take interval type from Currency if exists, otherwise UserCompanySetting
            int intervalType = (int)TermGroup_CurrencyIntervalType.None;
            if (currencyIntervalTypeId != intervalType)
                intervalType = currencyIntervalTypeId;
            else
                intervalType = companyIntervalTypeId;

            switch (intervalType)
            {
                case (int)TermGroup_CurrencyIntervalType.None:
                case (int)TermGroup_CurrencyIntervalType.Manually:
                    valid = false;
                    break;
                case (int)TermGroup_CurrencyIntervalType.FirstDayOfQuarter:
                    valid = CalendarUtility.IsFirstDayOfQuarter();
                    break;
                case (int)TermGroup_CurrencyIntervalType.FirstDayOfMonth:
                    valid = CalendarUtility.IsFirstDayOfMonth();
                    break;
                case (int)TermGroup_CurrencyIntervalType.EveryMonday:
                    valid = CalendarUtility.IsMonday();
                    break;
                case (int)TermGroup_CurrencyIntervalType.EveryDay:
                    valid = true;
                    break;
            }

            return valid;
        }

        #endregion

        #region External Currency

        public bool TryGetCurrencyRatesFromECB(out List<SysCurrencyRateDTO> rates, List<string> currencyCodesFilter)
        {
            bool result = false;
            rates = new List<SysCurrencyRateDTO>();

            XDocument xdoc = XDocument.Load(Constants.CURRENCY_ECB_URI);
            if (xdoc != null)
            {
                ECBCurrencyItem item = new ECBCurrencyItem(xdoc, currencyCodesFilter);
                rates = ConvertEuroRates(item.EuroRates, item.Date);
                result = true;

                if (item.Error != null)
                    this.log.Error($"Error fetching currency rates from ECB: {item.Error}");
            }

            return result;
        }

        #endregion

        #region SysBanker
        public SysBankDTO GetSysBank(string BIC)
        {
            string cacheKey = $"GetSysBank#BIC{BIC}";
            SysBankDTO sysBankDTO = BusinessMemoryCache<SysBankDTO>.Get(cacheKey);

            if (sysBankDTO == null)
            {
                using (var sysEntities = new SOESysEntities())
                {
                    using (var transaction = new TransactionScope(TransactionScopeOption.Suppress))
                    {
                        sysBankDTO = (from sc in sysEntities.SysBank.AsNoTracking()
                                      where sc.BIC == BIC
                                      select new SysBankDTO
                                      {
                                          BIC = sc.BIC,
                                          Name = sc.Name,
                                          SysBankId = sc.SysBankId,
                                          SysCountryId = sc.SysCountryId,
                                          HasIntegration = sc.HasIntegration
                                      }).FirstOrDefault();
                        if (sysBankDTO != null)
                        {
                            BusinessMemoryCache<SysBankDTO>.Set(cacheKey, sysBankDTO, 120);
                        }
                    }
                }
            }

            return sysBankDTO;
        }

        public List<SysBankDTO> GetSysBanksForIntegration()
        {
            using (var sysEntities = new SOESysEntities())
            {
                using (var transaction = new TransactionScope(TransactionScopeOption.Suppress))
                {
                    return (from sc in sysEntities.SysBank.AsNoTracking()
                            where sc.HasIntegration
                            select new SysBankDTO
                            {
                                BIC = sc.BIC,
                                Name = sc.Name,
                                SysBankId = sc.SysBankId,
                                SysCountryId = sc.SysCountryId
                            }).ToList();
                }
            }
        }

        #endregion
    }
}
