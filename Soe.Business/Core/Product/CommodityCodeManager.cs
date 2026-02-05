using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;

namespace SoftOne.Soe.Business.Core
{
    public class CommodityCodeManager : ManagerBase
    {
        #region Ctor

        public CommodityCodeManager(ParameterObject parameterObject) : base(parameterObject) { }

        #endregion

        #region CustomerCommodityCodes

        public List<CommodityCodeDTO> GetCustomerCommodityCodes(int actorCompanyId, bool onlyActive, bool ignoreCodeStateCheck = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.IntrastatCode.NoTracking();
            return GetCustomerCommodityCodes(entities, actorCompanyId, onlyActive, ignoreCodeStateCheck);
        }

        public List<SmallGenericType> GetCustomerCommodityCodesDict(int actorCompanyId, bool addEmpty)
        {
            var codesDict = new List<SmallGenericType>();

            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            entitiesReadOnly.IntrastatCode.NoTracking();
            var codes = GetCustomerCommodityCodes(entitiesReadOnly, actorCompanyId, true).OrderBy(c => c.Code);

            if (addEmpty)
                codesDict.Add(new SmallGenericType { Id = 0, Name = " " });

            codesDict.AddRange(codes.Select(c => new SmallGenericType() { Id = c.IntrastatCodeId.Value, Name = (c.Code + " " + c.Text).Length > 100 ? (c.Code + " " + c.Text).Substring(0, 100) + "..." : (c.Code + " " + c.Text) }));

            return codesDict;
        }

        public List<CommodityCodeDTO> GetCustomerCommodityCodes(CompEntities entities, int actorCompanyId, bool onlyActive, bool ignoreCodeStateCheck = false)
        {
            int langId = GetLangId();

            var dtos = new List<CommodityCodeDTO>();
            var query = (from ef in entities.IntrastatCode
                         where ef.ActorCompanyId == actorCompanyId
                         select new
                         {
                             IntrastatCodeId = ef.IntrastatCodeId,
                             SysIntrastatCodeId = ef.SysIntrastatCodeId,
                             State = ef.State,
                         });

            if (!ignoreCodeStateCheck)
                query = query.Where(ef => ef.State == (int)SoeEntityState.Active);

            var codes = query.ToList();

            if (onlyActive)
            {
                foreach(var code in codes)
                {
                    var sysCode = SysDbCache.Instance.SysIntrastatCodes.FirstOrDefault(c => c.SysIntrastatCodeId == code.SysIntrastatCodeId);
                    if (sysCode != null)
                    {
                        var codeTranslation = sysCode.SysIntrastatText.FirstOrDefault(t => t.SysLanguageId == langId);
                        var dto = new CommodityCodeDTO()
                        {
                            SysIntrastatCodeId = sysCode.SysIntrastatCodeId,
                            IntrastatCodeId = code.IntrastatCodeId,
                            Code = sysCode.Code,
                            UseOtherQuantity = sysCode.UseOtherQualifier,
                            StartDate = sysCode.StartDate,
                            EndDate = sysCode.EndDate,
                            IsActive = code.State == (int)SoeEntityState.Active,
                        };

                        if (codeTranslation != null)
                            dto.Text = codeTranslation.Text;
                        else if (sysCode.SysIntrastatText.Any(t => t.SysLanguageId == (int)TermGroup_Languages.Swedish))
                            dto.Text = sysCode.SysIntrastatText.FirstOrDefault(t => t.SysLanguageId == (int)TermGroup_Languages.Swedish).Text;
                        else if (sysCode.SysIntrastatText.Any())
                            dto.Text = sysCode.SysIntrastatText.FirstOrDefault().Text;
                        else
                            dto.Text = "UNDEFINED";

                        dtos.Add(dto);
                    }
                }
            }
            else
            {
                foreach (var sysCode in SysDbCache.Instance.SysIntrastatCodes)
                {
                    var customerCode = codes.FirstOrDefault(c => c.SysIntrastatCodeId == sysCode.SysIntrastatCodeId);
                    var codeTranslation = sysCode.SysIntrastatText.FirstOrDefault(t => t.SysLanguageId == langId);
                    var dto = new CommodityCodeDTO()
                    {
                        SysIntrastatCodeId = sysCode.SysIntrastatCodeId,
                        IntrastatCodeId = customerCode != null ? customerCode.IntrastatCodeId : (int?)null,
                        Code = sysCode.Code,
                        UseOtherQuantity = sysCode.UseOtherQualifier,
                        StartDate = sysCode.StartDate,
                        EndDate = sysCode.EndDate,
                        IsActive = customerCode != null && customerCode.State == (int)SoeEntityState.Active,
                    };

                    if (codeTranslation != null)
                        dto.Text = codeTranslation.Text;
                    else if (sysCode.SysIntrastatText.Any(t => t.SysLanguageId == (int)TermGroup_Languages.Swedish))
                        dto.Text = sysCode.SysIntrastatText.FirstOrDefault(t => t.SysLanguageId == (int)TermGroup_Languages.Swedish).Text;
                    else if (sysCode.SysIntrastatText.Any())
                        dto.Text = sysCode.SysIntrastatText.FirstOrDefault().Text;
                    else
                        dto.Text = "UNDEFINED";

                    dtos.Add(dto);
                }
            }

            return dtos;
        }

        public int? EnsureCustomerCommodityCode(int actorCompanyId, string code)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.IntrastatCode.NoTracking();
            return EnsureCustomerCommodityCode(entities, actorCompanyId, code);
        }

        public int? EnsureCustomerCommodityCode(CompEntities entities, int actorCompanyId, string code)
        {
            int langId = GetLangId();
            var sysCode = SysDbCache.Instance.SysIntrastatCodes.FirstOrDefault(c => c.Code == code);
            if (sysCode == null)
                return null;

            var intrastatCode = (from ef in entities.IntrastatCode
                                    where ef.ActorCompanyId == actorCompanyId &&
                                    ef.SysIntrastatCodeId == sysCode.SysIntrastatCodeId &&
                                    ef.State == (int)SoeEntityState.Active
                                    select ef).FirstOrDefault();

            if (intrastatCode == null)
            {
                intrastatCode = new IntrastatCode()
                {
                    SysIntrastatCodeId = sysCode.SysIntrastatCodeId,
                    ActorCompanyId = actorCompanyId,
                    State = (int)SoeEntityState.Active
                };
                SetCreatedProperties(intrastatCode);

                entities.IntrastatCode.AddObject(intrastatCode);
                var saveResult = SaveChanges(entities);
            }

            return intrastatCode.IntrastatCodeId;
        }

        public ActionResult SaveCustomerCommodityCodes(Dictionary<int, bool> codes, int actorCompanyId)
        {
            ActionResult result = new ActionResult();

            using (CompEntities entities = new CompEntities())
            {
                var existingCodes = (from ef in entities.IntrastatCode
                         where ef.ActorCompanyId == actorCompanyId
                         select ef);

                foreach(var code in codes)
                {
                    var existing = existingCodes.FirstOrDefault(c => c.SysIntrastatCodeId == code.Key && c.ActorCompanyId == actorCompanyId);
                    if (existing != null)
                    {
                        existing.State = code.Value == true ? (int)SoeEntityState.Active : (int)SoeEntityState.Deleted;

                        SetModifiedProperties(existing);
                    }
                    else
                    {
                        existing = new IntrastatCode()
                        {
                            SysIntrastatCodeId = code.Key,
                            ActorCompanyId = actorCompanyId,

                            State = (int)SoeEntityState.Active
                        };

                        SetCreatedProperties(existing);
                        entities.IntrastatCode.AddObject(existing);
                    }

                    result = SaveChanges(entities);
                }

                return result;
            }
        }

        #endregion

        #region IntrastatTransactions

        public List<IntrastatTransactionDTO> GetIntrastatTransactions(int originId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.IntrastatTransaction.NoTracking();
            return GetIntrastatTransactions(entities, originId);
        }

        public List<IntrastatTransactionDTO> GetIntrastatTransactions(CompEntities entities, int originId)
        {
            return (from i in entities.IntrastatTransaction
                    where i.OriginId == originId &&
                    i.State == (int)SoeEntityState.Active
                    select i).Select(map => new IntrastatTransactionDTO()
                    {
                        IntrastatTransactionId = map.IntrastatTransactionId,
                        OriginId = map.OriginId,
                        IntrastatCodeId = map.IntrastatCodeId,
                        IntrastatTransactionType = map.IntrastatTransactionType,
                        ProductUnitId = map.ProductUnitId,
                        SysCountryId = map.SysCountryId,
                        Quantity = map.Quantity,
                        NetWeight = map.NetWeight,
                        OtherQuantity = map.OtherQuantity,
                        NotIntrastat = map.NotIntrastat,
                        Amount = map.Amount,
                        State = (SoeEntityState)map.State, 
                    }).ToList();
        }

        public ActionResult SaveIntrastatTransactions(List<IntrastatTransactionDTO> transactions, int originId, SoeOriginType originType)
        {
            ActionResult result = new ActionResult();

            using (CompEntities entities = new CompEntities())
            {
                var existingTransactions = (from t in entities.IntrastatTransaction
                                     where t.OriginId == originId
                                     select t);

                if (originType == SoeOriginType.Order || originType == SoeOriginType.CustomerInvoice)
                {
                    var invoiceRows = (from t in entities.CustomerInvoiceRow
                                       where t.InvoiceId == originId &&
                                       t.State == (int)SoeEntityState.Active
                                       select t);

                    foreach (var transaction in transactions)
                    {
                        if (transaction.State == SoeEntityState.Deleted)
                        {
                            var existing = existingTransactions.FirstOrDefault(c => c.IntrastatTransactionId == transaction.IntrastatTransactionId);
                            if (existing != null)
                            {
                                existing.State = (int)SoeEntityState.Deleted;
                                SetModifiedProperties(existing);

                                if (transaction.CustomerInvoiceRowId.HasValue)
                                {
                                    var invoiceRow = invoiceRows.FirstOrDefault(r => r.CustomerInvoiceRowId == transaction.CustomerInvoiceRowId.Value);
                                    if (invoiceRow != null)
                                    {
                                        invoiceRow.IntrastatTransaction = null;
                                        SetModifiedProperties(invoiceRow);
                                    }
                                }
                            }
                        }
                        else
                        {
                            var existing = existingTransactions.FirstOrDefault(c => c.IntrastatTransactionId == transaction.IntrastatTransactionId);
                            if (existing != null)
                            {
                                //var invoiceRow = invoiceRows.FirstOrDefault()
                                existing.OriginId = transaction.OriginId;
                                existing.IntrastatCodeId = transaction.IntrastatCodeId;
                                existing.IntrastatTransactionType = transaction.IntrastatTransactionType;
                                existing.ProductUnitId = transaction.ProductUnitId;
                                existing.SysCountryId = transaction.SysCountryId;
                                existing.Quantity = transaction.Quantity;
                                existing.NetWeight = transaction.NetWeight;
                                existing.OtherQuantity = transaction.OtherQuantity.IsNullOrEmpty() ? String.Empty : transaction.OtherQuantity;
                                existing.NotIntrastat = transaction.NotIntrastat;
                                existing.Amount = transaction.Amount;

                                SetModifiedProperties(existing);
                            }
                            else
                            {
                                existing = new IntrastatTransaction()
                                {
                                    OriginId = transaction.OriginId,
                                    IntrastatCodeId = transaction.IntrastatCodeId,
                                    IntrastatTransactionType = transaction.IntrastatTransactionType,
                                    ProductUnitId = transaction.ProductUnitId,
                                    SysCountryId = transaction.SysCountryId,
                                    Quantity = transaction.Quantity,
                                    NetWeight = transaction.NetWeight,
                                    OtherQuantity = transaction.OtherQuantity.IsNullOrEmpty() ? String.Empty : transaction.OtherQuantity,
                                    NotIntrastat = transaction.NotIntrastat,
                                    Amount = transaction.Amount,

                                    State = (int)SoeEntityState.Active
                                };

                                SetCreatedProperties(existing);
                                entities.IntrastatTransaction.AddObject(existing);

                                if (transaction.CustomerInvoiceRowId.HasValue)
                                {
                                    var invoiceRow = invoiceRows.FirstOrDefault(r => r.CustomerInvoiceRowId == transaction.CustomerInvoiceRowId.Value);
                                    if (invoiceRow != null)
                                    {
                                        invoiceRow.IntrastatTransaction = existing;
                                        SetModifiedProperties(invoiceRow);
                                    }
                                }
                            }
                        }
                    }
                }
                else if(originType == SoeOriginType.Purchase)
                {
                    var puchaseRows = (from t in entities.PurchaseRow
                                       where t.PurchaseId == originId &&
                                       t.State == (int)SoeEntityState.Active
                                       select t);

                    foreach (var transaction in transactions)
                    {
                        if (transaction.State == SoeEntityState.Deleted)
                        {
                            var existing = existingTransactions.FirstOrDefault(c => c.IntrastatTransactionId == transaction.IntrastatTransactionId);
                            if (existing != null)
                            {
                                existing.State = (int)SoeEntityState.Deleted;
                                SetModifiedProperties(existing);

                                if (transaction.CustomerInvoiceRowId.HasValue)
                                {
                                    var purchaseRow = puchaseRows.FirstOrDefault(r => r.PurchaseRowId == transaction.CustomerInvoiceRowId.Value);
                                    if (purchaseRow != null)
                                    {
                                        purchaseRow.IntrastatTransaction = null;
                                        SetModifiedProperties(purchaseRow);
                                    }
                                }
                            }
                        }
                        else
                        {
                            var existing = existingTransactions.FirstOrDefault(c => c.IntrastatTransactionId == transaction.IntrastatTransactionId);
                            if (existing != null)
                            {
                                //var invoiceRow = invoiceRows.FirstOrDefault()
                                existing.OriginId = transaction.OriginId;
                                existing.IntrastatCodeId = transaction.IntrastatCodeId;
                                existing.IntrastatTransactionType = transaction.IntrastatTransactionType;
                                existing.ProductUnitId = transaction.ProductUnitId;
                                existing.SysCountryId = transaction.SysCountryId;
                                existing.Quantity = transaction.Quantity;
                                existing.NetWeight = transaction.NetWeight;
                                existing.OtherQuantity = transaction.OtherQuantity.IsNullOrEmpty() ? String.Empty : transaction.OtherQuantity;
                                existing.NotIntrastat = transaction.NotIntrastat;
                                existing.Amount = transaction.Amount;

                                SetModifiedProperties(existing);
                            }
                            else
                            {
                                existing = new IntrastatTransaction()
                                {
                                    OriginId = transaction.OriginId,
                                    IntrastatCodeId = transaction.IntrastatCodeId,
                                    IntrastatTransactionType = transaction.IntrastatTransactionType,
                                    ProductUnitId = transaction.ProductUnitId,
                                    SysCountryId = transaction.SysCountryId,
                                    Quantity = transaction.Quantity,
                                    NetWeight = transaction.NetWeight,
                                    OtherQuantity = transaction.OtherQuantity.IsNullOrEmpty() ? String.Empty : transaction.OtherQuantity,
                                    NotIntrastat = transaction.NotIntrastat,
                                    Amount = transaction.Amount,

                                    State = (int)SoeEntityState.Active
                                };

                                SetCreatedProperties(existing);
                                entities.IntrastatTransaction.AddObject(existing);

                                if (transaction.CustomerInvoiceRowId.HasValue)
                                {
                                    var purchaseRow = puchaseRows.FirstOrDefault(r => r.PurchaseRowId == transaction.CustomerInvoiceRowId.Value);
                                    if (purchaseRow != null)
                                    {
                                        purchaseRow.IntrastatTransaction = existing;
                                        SetModifiedProperties(purchaseRow);
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    foreach (var transaction in transactions)
                    {
                        if (transaction.State == SoeEntityState.Deleted)
                        {
                            var existing = existingTransactions.FirstOrDefault(c => c.IntrastatTransactionId == transaction.IntrastatTransactionId);
                            if (existing != null)
                            {
                                existing.State = (int)SoeEntityState.Deleted;
                                SetModifiedProperties(existing);
                            }
                        }
                        else
                        {
                            var existing = existingTransactions.FirstOrDefault(c => c.IntrastatTransactionId == transaction.IntrastatTransactionId);
                            if (existing != null)
                            {
                                //var invoiceRow = invoiceRows.FirstOrDefault()
                                existing.OriginId = transaction.OriginId;
                                existing.IntrastatCodeId = transaction.IntrastatCodeId;
                                existing.IntrastatTransactionType = transaction.IntrastatTransactionType;
                                existing.ProductUnitId = transaction.ProductUnitId;
                                existing.SysCountryId = transaction.SysCountryId;
                                existing.Quantity = transaction.Quantity;
                                existing.NetWeight = transaction.NetWeight;
                                existing.OtherQuantity = transaction.OtherQuantity.IsNullOrEmpty() ? String.Empty : transaction.OtherQuantity;
                                existing.NotIntrastat = transaction.NotIntrastat;
                                existing.Amount = transaction.Amount;

                                SetModifiedProperties(existing);
                            }
                            else
                            {
                                existing = new IntrastatTransaction()
                                {
                                    OriginId = transaction.OriginId,
                                    IntrastatCodeId = transaction.IntrastatCodeId,
                                    IntrastatTransactionType = transaction.IntrastatTransactionType,
                                    ProductUnitId = transaction.ProductUnitId,
                                    SysCountryId = transaction.SysCountryId,
                                    Quantity = transaction.Quantity,
                                    NetWeight = transaction.NetWeight,
                                    OtherQuantity = transaction.OtherQuantity.IsNullOrEmpty() ? String.Empty : transaction.OtherQuantity,
                                    NotIntrastat = transaction.NotIntrastat,
                                    Amount = transaction.Amount,

                                    State = (int)SoeEntityState.Active
                                };

                                SetCreatedProperties(existing);
                                entities.IntrastatTransaction.AddObject(existing);
                            }
                        }
                    }
                }

                result = SaveChanges(entities);

                return result;
            }
        }

        #endregion

        #region Intrastat Transactions Export

        public List<IntrastatTransactionExportDTO> GetIntrastatTransactionsForExport(IntrastatReportingType reportingType, DateTime fromDate, DateTime toDate, int actorCompanyId)
        {
            List<IntrastatTransactionExportDTO> transactions = new List<IntrastatTransactionExportDTO>();
           
                #region Prereq

                var langId = this.GetLangId();
                var syscountries = CountryCurrencyManager.GetSysCountries();
                var currencyDict = CountryCurrencyManager.GetCompCurrenciesDict(actorCompanyId, false);
                var intrastatCodes = GetCustomerCommodityCodes(actorCompanyId, true, true);
                var transactionTypes = base.GetTermGroupDict(TermGroup.IntrastatTransactionType, langId);
                var originTypeTexts = base.GetTermGroupDict(TermGroup.OriginType, langId);

                #endregion
                using (var entities = new CompEntities())
                {
                    var trans = (from t in entities.IntrastatTransactionView
                                 where t.ActorCompanyId == actorCompanyId &&
                                 (t.DeliveryDate != null ? (t.DeliveryDate >= fromDate && t.DeliveryDate <= toDate) : (t.VoucherDate >= fromDate && t.VoucherDate <= toDate)) && !t.NotIntrastat
                                 select t);

                    if (reportingType == IntrastatReportingType.Export)
                    {
                        transactions = (from t in trans
                                        where t.OriginType == (int)SoeOriginType.CustomerInvoice
                                        select t).Select(t => new IntrastatTransactionExportDTO()
                                        {
                                            IntrastatTransactionId = t.IntrastatTransactionId,
                                            OriginId = t.OriginId,
                                            OriginType = t.OriginType,
                                            IntrastatCodeId = t.IntrastatCodeId,
                                            IntrastatTransactionType = t.IntrastatTransactionType,
                                            ProductUnitId = t.ProductUnitId,
                                            SysCountryId = t.SysCountryId,
                                            NetWeight = t.NetWeight,
                                            OtherQuantity = t.OtherQuantity,
                                            NotIntrastat = t.NotIntrastat,
                                            Amount = t.Amount,

                                            CustomerInvoiceRowId = t.ProductRowId,
                                            ProductNr = t.ProductNr,
                                            ProductName = t.ProductName,
                                            Quantity = t.Quantity,
                                            ProductUnitCode = t.ProductUnitCode,
                                            SeqNr = t.SeqNr,
                                            OriginNr = t.InvoiceNr,
                                            Name = t.ActorName,
                                            VatNr = t.ActorVatNr,
                                            InvoiceDate = t.InvoiceDate,
                                            VoucherDate = t.VoucherDate,
                                            IsPrivatePerson = t.ActorPrivatePerson.HasValue ? t.ActorPrivatePerson.Value : false,
                                            ActorCountryId = t.ActorCountryId.Value,
                                            CurrencyId = t.CurrencyId,

                                        }).ToList();
                    }
                    else if (reportingType == IntrastatReportingType.Import)
                    {
                        transactions = (from t in trans
                                        where (t.OriginType == (int)SoeOriginType.Purchase || t.OriginType == (int)SoeOriginType.SupplierInvoice)
                                        select t).Select(t => new IntrastatTransactionExportDTO()
                                        {
                                            IntrastatTransactionId = t.IntrastatTransactionId,
                                            OriginId = t.OriginId,
                                            OriginType = t.OriginType,
                                            IntrastatCodeId = t.IntrastatCodeId,
                                            IntrastatTransactionType = t.IntrastatTransactionType,
                                            ProductUnitId = t.ProductUnitId,
                                            SysCountryId = t.SysCountryId,
                                            NetWeight = t.NetWeight,
                                            OtherQuantity = t.OtherQuantity,
                                            NotIntrastat = t.NotIntrastat,
                                            Amount = t.Amount,

                                            CustomerInvoiceRowId = t.ProductRowId,
                                            ProductNr = t.ProductNr,
                                            ProductName = t.ProductName,
                                            Quantity = t.Quantity,
                                            ProductUnitCode = t.ProductUnitCode,
                                            SeqNr = t.SeqNr,
                                            OriginNr = t.InvoiceNr,
                                            Name = t.ActorName,
                                            VatNr = t.ActorVatNr,
                                            InvoiceDate = t.InvoiceDate,
                                            VoucherDate = t.VoucherDate,
                                            IsPrivatePerson = t.ActorPrivatePerson.HasValue ? t.ActorPrivatePerson.Value : false,
                                            ActorCountryId = t.ActorCountryId.Value,
                                            CurrencyId = t.CurrencyId,
                                        }).ToList();
                    }
                    else
                    {
                        transactions = (from t in trans
                                        select t).Select(t => new IntrastatTransactionExportDTO()
                                        {
                                            IntrastatTransactionId = t.IntrastatTransactionId,
                                            OriginId = t.OriginId,
                                            OriginType = t.OriginType,
                                            IntrastatCodeId = t.IntrastatCodeId,
                                            IntrastatTransactionType = t.IntrastatTransactionType,
                                            ProductUnitId = t.ProductUnitId,
                                            SysCountryId = t.SysCountryId,
                                            NetWeight = t.NetWeight,
                                            OtherQuantity = t.OtherQuantity,
                                            NotIntrastat = t.NotIntrastat,
                                            Amount = t.Amount,

                                            CustomerInvoiceRowId = t.ProductRowId,
                                            ProductNr = t.ProductNr,
                                            ProductName = t.ProductName,
                                            Quantity = t.Quantity,
                                            ProductUnitCode = t.ProductUnitCode,
                                            SeqNr = t.SeqNr,
                                            OriginNr = t.InvoiceNr,
                                            Name = t.ActorName,
                                            VatNr = t.ActorVatNr,
                                            InvoiceDate = t.InvoiceDate,
                                            VoucherDate = t.VoucherDate,
                                            IsPrivatePerson = t.ActorPrivatePerson.HasValue ? t.ActorPrivatePerson.Value : false,
                                            ActorCountryId = t.ActorCountryId.Value,
                                            CurrencyId = t.CurrencyId,
                                        }).ToList();
                    }
                }

                foreach (var transaction in transactions)
                {
                    transaction.OriginTypeName = transaction.OriginType != 0 ? originTypeTexts[transaction.OriginType] : "";
                    transaction.IntrastatTransactionTypeName = transaction.IntrastatTransactionType != 0 ? transaction.IntrastatTransactionType.ToString() + " " + transactionTypes[transaction.IntrastatTransactionType] : "";

                    if (transaction.VatNr.Trim().IsNullOrEmpty())
                        transaction.VatNr = transaction.IsPrivatePerson ? "QN999999999999" : "QV999999999999";

                    if (currencyDict.TryGetValue(transaction.CurrencyId, out string currencyCode))
                        transaction.CurrencyCode = currencyCode;

                    var originCountry = syscountries.FirstOrDefault(c => c.SysCountryId == transaction.SysCountryId);
                    if (originCountry != null)
                        transaction.OriginCountry = originCountry.Code;

                    var actorCountry = syscountries.FirstOrDefault(c => c.SysCountryId == transaction.ActorCountryId);
                    if (actorCountry != null)
                        transaction.Country = actorCountry.Code;

                    var customerCode = intrastatCodes.FirstOrDefault(c => c.IntrastatCodeId == transaction.IntrastatCodeId);
                    if (customerCode != null)
                    {
                        var sysCode = SysDbCache.Instance.SysIntrastatCodes.FirstOrDefault(c => c.SysIntrastatCodeId == customerCode.SysIntrastatCodeId);
                        if (sysCode != null)
                        {
                            transaction.IntrastatCode = sysCode.Code;

                            var codeTranslation = sysCode.SysIntrastatText.FirstOrDefault(t => t.SysLanguageId == langId);

                            if (codeTranslation != null)
                                transaction.IntrastatCodeName = sysCode.Code + " " + codeTranslation.Text;
                            else if (sysCode.SysIntrastatText.Any(t => t.SysLanguageId == (int)TermGroup_Languages.Swedish))
                                transaction.IntrastatCodeName = sysCode.Code + " " + sysCode.SysIntrastatText.FirstOrDefault(t => t.SysLanguageId == (int)TermGroup_Languages.Swedish).Text;
                            else if (sysCode.SysIntrastatText.Any())
                                transaction.IntrastatCodeName = sysCode.Code + " " + sysCode.SysIntrastatText.FirstOrDefault().Text;
                            else
                                transaction.IntrastatCodeName = "UNDEFINED";
                        }
                    }
                }
            
            return transactions;
        }

        #endregion

        #region SysIntrastatCodes

        public List<SysIntrastatCode> GetSysIntrastatCodes()
        {            
            using (var entities = new SOESysEntities())
            {
                using (var transaction = new TransactionScope(TransactionScopeOption.Suppress))
                {
                    var codes = (from c in entities.SysIntrastatCode.Include("SysIntrastatText")                                
                                 select c).ToList();

                    return codes;
                }
            }
        }

        public SysIntrastatCode GetCommodityCodeByCode(SOESysEntities entities, string code)
        {            
            return (from ic in entities.SysIntrastatCode.Include("SysIntrastatText")
                where ic.State == (int)SoeEntityState.Active && ic.Code == code
                select ic).FirstOrDefault();            
        }
        public void AddCommodityCode(SOESysEntities entities, SysIntrastatCode obj) {            
            obj.Created = DateTime.Now;
            obj.CreatedBy = GetUserDetails();
            entities.SysIntrastatCode.Add(obj);
        }

        public void UpdateCommodityCode(SOESysEntities entities, SysIntrastatCode obj)
        {
            obj.Modified = DateTime.Now;
            obj.ModifedBy = GetUserDetails();
        }

        public List<CommodityCodeDTO> GetSysIntrastatCodesDTOs(int langId)
        {
        
            var lst = GetSysIntrastatCodes();
            List<CommodityCodeDTO> model = new List<CommodityCodeDTO>();
            foreach (var cc in lst)
            {
                foreach (var item in cc.SysIntrastatText)
                {
                    if (item.SysLanguageId == langId) {
                        model.Add(new CommodityCodeDTO {
                            SysIntrastatCodeId = cc.SysIntrastatCodeId,
                            IntrastatCodeId = item.SysIntrastatTextId,
                            Code = cc.Code,
                            Text = item.Text,
                            UseOtherQuantity = cc.UseOtherQualifier,
                            StartDate = cc.StartDate,
                            EndDate = cc.EndDate,
                            IsActive = true
                        });
                    }
                }
            }
            return model;
        }

        #endregion
    }
}
