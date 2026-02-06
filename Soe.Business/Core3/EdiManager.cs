using BitMiracle.LibTiff.Classic;
using Converter.Shared.PDF;
using log4net;
using Soe.Edi.Common.DTO;
using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Business.Util.API.AzoraOne;
using SoftOne.Soe.Business.Util.API.AzoraOne.Models;
using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Business.Util.Converter;
using SoftOne.Soe.Business.Util.Finvoice;
using SoftOne.Soe.Business.Util.LogCollector;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.DTO.Scanning;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Shared.DTO.ThirdParty.AzoraOne;
using SoftOne.Soe.Util;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Transactions;
using System.Xml.Linq;
using static Soe.Edi.Common.Enumerations;

namespace SoftOne.Soe.Business.Core
{
    public class EdiManager : ManagerBase
    {
        #region Variables

        // Create a logger for use in this class
        private readonly log4net.ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        #endregion

        #region Constructors

        public EdiManager(ParameterObject parameterObject) : base(parameterObject) { }

        #endregion

        #region CompanyEdi

        public List<CompanyEdi> GetCompanyEdis(params TermGroup_CompanyEdiType[] ediTypes)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.CompanyEdi.NoTracking();
            return GetCompanyEdis(entities, ediTypes);
        }

        public List<CompanyEdi> GetCompanyEdis(CompEntities entities, params TermGroup_CompanyEdiType[] ediTypes)
        {
            if (ediTypes == null || ediTypes.Length == 0)
            {
                return (from ce in entities.CompanyEdi
                            .Include("Company")
                        where ce.State == (int)SoeEntityState.Active
                        select ce).ToList();
            }
            else
            {
                var ediTypesInt = ediTypes.Select(et => (int)et).ToArray();
                return (from ce in entities.CompanyEdi
                            .Include("Company")
                        where ce.State == (int)SoeEntityState.Active &&
                        ediTypesInt.Contains(ce.Type)
                        select ce).ToList();
            }
        }

        public List<CompanyEdi> GetCompanyEdis(int actorCompanyId, params TermGroup_CompanyEdiType[] ediTypes)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.CompanyEdi.NoTracking();
            return GetCompanyEdis(entities, actorCompanyId, ediTypes);
        }

        public List<CompanyEdi> GetCompanyEdis(CompEntities entities, int actorCompanyId, params TermGroup_CompanyEdiType[] ediTypes)
        {
            var ediTypesInt = ediTypes.Select(et => (int)et).ToArray();
            return (from ce in entities.CompanyEdi
                    where ce.State == (int)SoeEntityState.Active &&
                    ce.ActorCompanyId == actorCompanyId &&
                    ediTypesInt.Contains(ce.Type)
                    select ce).ToList();
        }

        public CompanyEdi GetCompanyEdi(int companyEdiId, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.CompanyEdi.NoTracking();
            return GetCompanyEdi(entities, companyEdiId, actorCompanyId);
        }

        public CompanyEdi GetCompanyEdi(CompEntities entities, int companyEdiId, int actorCompanyId)
        {
            return (from ce in entities.CompanyEdi
                        .Include("Company")
                    where ce.CompanyEdiId == companyEdiId &&
                    ce.ActorCompanyId == actorCompanyId
                    select ce).FirstOrDefault();
        }

        public CompanyEdi GetCompanyEdi(int actorCompanyId, TermGroup_CompanyEdiType? type = null, bool onlyActive = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.CompanyEdi.NoTracking();
            return GetCompanyEdi(entities, actorCompanyId, type, onlyActive);
        }

        public CompanyEdi GetCompanyEdi(CompEntities entities, int actorCompanyId, TermGroup_CompanyEdiType? type = null, bool onlyActive = false)
        {
            //Discard state
            return (from ce in entities.CompanyEdi
                        .Include("Company")
                    where ce.ActorCompanyId == actorCompanyId &&
                    (!onlyActive || ce.State == (int)SoeEntityState.Active) &&
                    (!type.HasValue || ce.Type == (int)type.Value)
                    select ce).FirstOrDefault();
        }

        public bool CompanyUsesEdi(int actorCompanyId)
        {
            //EdiConnection check for current edi users
            EdiConnection ediConnection = GetEdiConnection(actorCompanyId);

            //If not current, check companyedi for previous. 
            if (ediConnection == null)
            {
                CompanyEdi companyEdi = GetCompanyEdi(actorCompanyId);
                return companyEdi != null;
            }
            else
            {
                return true;
            }
        }

        public ActionResult AddCompanyEdi(CompanyEdi companyEdi, int actorCompanyId)
        {
            if (companyEdi == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "CompanyEdi");

            using (CompEntities entities = new CompEntities())
            {
                companyEdi.Company = CompanyManager.GetCompany(entities, actorCompanyId);
                if (companyEdi.Company == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte"));

                return AddEntityItem(entities, companyEdi, "CompanyEdi");
            }
        }

        public ActionResult UpdateCompanyEdi(CompanyEdi companyEdi, int actorCompanyId)
        {
            if (companyEdi == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "CompanyEdi");

            using (CompEntities entities = new CompEntities())
            {
                CompanyEdi originalCompanyEdi = GetCompanyEdi(entities, companyEdi.CompanyEdiId, actorCompanyId);
                if (originalCompanyEdi == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "CompanyEdi");

                return UpdateEntityItem(entities, originalCompanyEdi, companyEdi, "CompanyEdi");
            }
        }

        #endregion

        #region EdiConnection

        public EdiConnection GetEdiConnection(int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.EdiConnection.NoTracking();
            return GetEdiConnection(entities, actorCompanyId);
        }

        public EdiConnection GetEdiConnection(CompEntities entities, int actorCompanyId)
        {
            return (from ec in entities.EdiConnection
                    where ec.ActorCompanyId == actorCompanyId
                    select ec).FirstOrDefault();
        }

        #endregion

        #region EdiEntry

        public List<CompanyEdiEntryDTO> GetEdiEntries(DateTime fromDate, DateTime toDate)
        {
            var dtos = new List<CompanyEdiEntryDTO>();
            using (var entities = new CompEntities())
            {
                var entries = entities.GetEdiEntriesForDates(fromDate, toDate);

                foreach (var item in entries)
                {
                    dtos.Add(
                        new CompanyEdiEntryDTO
                        {
                            ActorCompanyId = item.ActorCompanyId,
                            CompanyName = item.Name,
                            CompanyNr = item.CompanyNr,
                            OrgNr = item.OrgNr,
                            SysWholesellerId = item.SysWholesellerId,
                            WholesellerName = item.WholesellerName,
                            LicenseNr = item.LicenseNr,
                            MessageTypeName = item.MessageTypeName
                        }
                    );
                }
            }

            return dtos;
        }

        public EdiEntry GetEdiEntry(int ediEntryId, int actorCompanyId, bool ignoreState = false, bool loadSupplier = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.EdiEntry.NoTracking();
            return GetEdiEntry(entities, ediEntryId, actorCompanyId, ignoreState, loadSupplier);
        }

        public EdiEntry GetEdiEntry(CompEntities entities, int ediEntryId, int actorCompanyId, bool ignoreState = false, bool loadSupplier = false, bool loadScanning = false)
        {
            EdiEntry ediEntry = null;

            if (loadSupplier && loadScanning)
            {
                ediEntry = (from edi in entities.EdiEntry
                                .Include("Supplier")
                                .Include("ScanningEntryInvoice")
                            where (edi.ActorCompanyId == actorCompanyId &&
                            edi.EdiEntryId == ediEntryId) &&
                            (ignoreState || (edi.State == (int)SoeEntityState.Active))
                            select edi).FirstOrDefault();
            }
            else if (loadSupplier)
            {
                ediEntry = (from edi in entities.EdiEntry
                                .Include("Supplier")
                            where (edi.ActorCompanyId == actorCompanyId &&
                            edi.EdiEntryId == ediEntryId) &&
                            (ignoreState || (edi.State == (int)SoeEntityState.Active))
                            select edi).FirstOrDefault();
            }
            else if (loadScanning)
            {
                ediEntry = (from edi in entities.EdiEntry
                                .Include("ScanningEntryInvoice")
                            where (edi.ActorCompanyId == actorCompanyId &&
                            edi.EdiEntryId == ediEntryId) &&
                            (ignoreState || (edi.State == (int)SoeEntityState.Active))
                            select edi).FirstOrDefault();
            }
            else
            {
                ediEntry = (from edi in entities.EdiEntry
                            where (edi.ActorCompanyId == actorCompanyId &&
                            edi.EdiEntryId == ediEntryId) &&
                            (ignoreState || (edi.State == (int)SoeEntityState.Active))
                            select edi).FirstOrDefault();
            }

            return ediEntry;
        }

        public EdiEntry GetEdiEntryWithInvoiceAttachments(CompEntities entities, int ediEntryId, int actorCompanyId)
        {
            return (from e in entities.EdiEntry.Include("InvoiceAttachment")
                    where e.InvoiceId.HasValue &&
                    e.EdiEntryId == ediEntryId && e.ActorCompanyId == actorCompanyId
                    select e).FirstOrDefault();
        }

        public EdiEntry GetEdiEntryByFileName(CompEntities entities, string fileName, int actorCompanyId)
        {
            if (string.IsNullOrEmpty(fileName))
                return null;

            return (from edi in entities.EdiEntry
                    where edi.ActorCompanyId == actorCompanyId &&
                    edi.FileName == fileName
                    select edi).FirstOrDefault();
        }

        public EdiEntry GetEdiEntryByFileSeller(CompEntities entities, string sellerName, string sellerOrderNr, int actorCompanyId, bool onlyActive = true)
        {
            if (string.IsNullOrEmpty(sellerName) || string.IsNullOrEmpty(sellerOrderNr))
                return null;

            var query = from edi in entities.EdiEntry
                        where edi.ActorCompanyId == actorCompanyId &&
                        edi.SellerName == sellerName &&
                        edi.SellerOrderNr == sellerOrderNr &&
                        edi.Type == (int)TermGroup_EDISourceType.EDI &&
                        edi.MessageType != (int)TermGroup_EdiMessageType.SupplierInvoice
                        select edi;

            if (onlyActive)
                query = query.Where(i => i.State == (int)SoeEntityState.Active);

            return query.FirstOrDefault();
        }

        private EdiEntry GetEdiEntryByInvoiceNr(CompEntities entities, string sellerName, string invoiceNr, int actorCompanyId)
        {
            if (string.IsNullOrEmpty(sellerName) || string.IsNullOrEmpty(invoiceNr))
                return null;

            var query = from edi in entities.EdiEntry
                        where edi.ActorCompanyId == actorCompanyId &&
                        edi.SellerName == sellerName &&
                        edi.InvoiceNr == invoiceNr &&
                        edi.Type == (int)TermGroup_EDISourceType.EDI &&
                        edi.MessageType == (int)TermGroup_EdiMessageType.SupplierInvoice
                        select edi;

            return query.FirstOrDefault();
        }

        public EdiEntry GetEdiScanningEntryInvoice(int ediEntryId, int actorCompanyId, bool onlyActive)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.EdiEntry.NoTracking();
            return GetEdiScanningEntryInvoice(entities, ediEntryId, actorCompanyId, onlyActive);
        }

        public EdiEntry GetEdiScanningEntryInvoice(CompEntities entities, int ediEntryId, int actorCompanyId, bool onlyActive)
        {
            return (from edi in entities.EdiEntry
                        .Include("ScanningEntryInvoice.ScanningEntryRow")
                    where (edi.ActorCompanyId == actorCompanyId &&
                    edi.EdiEntryId == ediEntryId) &&
                    (!onlyActive || edi.State == (int)SoeEntityState.Active)
                    select edi).FirstOrDefault();
        }

        public EdiEntry GetEdiEntryFromInvoice(int invoiceId, bool includeEdiEntry = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.EdiEntry.NoTracking();
            return GetEdiEntryFromInvoice(entities, invoiceId, includeEdiEntry);
        }

        public EdiEntry GetEdiEntryFromInvoice(CompEntities entities, int invoiceId, bool includeEdiEntry = false, bool includeInvoiceAttachment = false)
        {
            IQueryable<EdiEntry> query = entities.EdiEntry;

            if (includeInvoiceAttachment)
                query = query.Include("InvoiceAttachment");

            return (from e in query
                    where e.InvoiceId.HasValue &&
                    e.InvoiceId.Value == invoiceId &&
                    (includeEdiEntry || e.Type != (int)TermGroup_EDISourceType.EDI)
                    select e).FirstOrDefault();
        }

        public int GetEdiEntryIdFromInvoice(int invoiceId, int actorCompanyId, TermGroup_EDISourceType type)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.EdiEntry.NoTracking();
            return GetEdiEntryIdFromInvoice(entities, invoiceId, actorCompanyId, type);
        }

        public int GetEdiEntryIdFromInvoice(CompEntities entities, int invoiceId, int actorCompanyId, TermGroup_EDISourceType type)
        {
            return (from e in entities.EdiEntry
                    where e.InvoiceId.HasValue &&
                    e.InvoiceId.Value == invoiceId &&
                    e.ActorCompanyId == actorCompanyId &&
                    e.Type == (int)type
                    select e.EdiEntryId).FirstOrDefault();
        }

        /// <summary>
        /// Get EdiEntry which is not linked with an Invoice
        /// </summary>
        /// <param name="entities"></param>
        /// <param name="actorCompanyId"></param>
        /// <param name="sourceType"></param>
        /// <param name="externalId"></param>
        /// <returns></returns>
        private EdiEntry GetValidEdiEntryByExternalId(CompEntities entities, int actorCompanyId, TermGroup_EDISourceType sourceType, string externalId)
        {
            return (from e in entities.EdiEntry
                    where e.ActorCompanyId == actorCompanyId
                        && !e.InvoiceId.HasValue
                        && e.Type == (int)sourceType
                        && e.ExternalId == externalId
                    select e).FirstOrDefault();
        }

        public bool EdiEntryExists(string fileName, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.EdiEntry.NoTracking();
            return EdiEntryExists(entities, fileName, actorCompanyId);
        }

        public bool EdiEntryExists(CompEntities entities, string fileName, int actorCompanyId)
        {
            if (string.IsNullOrEmpty(fileName))
                return false;

            var ediEntryId = (from edi in entities.EdiEntry
                              where edi.ActorCompanyId == actorCompanyId &&
                              edi.FileName == fileName
                              select edi.EdiEntryId).FirstOrDefault();
            return ediEntryId > 0;
        }

        private void SetEdiEntryOrderStatus(EdiEntry ediEntry, TermGroup_EDIOrderStatus validatedStatus)
        {
            if (ediEntry.Type != (int)TermGroup_EDISourceType.Finvoice && (ediEntry.SysWholesellerId == 0) || String.IsNullOrEmpty(ediEntry.OrderNr))
                ediEntry.OrderStatus = (int)TermGroup_EDIOrderStatus.Error;
            else
                ediEntry.OrderStatus = (int)validatedStatus;
        }

        private void SetEdiEntryInvoiceStatus(EdiEntry ediEntry, TermGroup_EDIInvoiceStatus validatedStatus)
        {
            if (ediEntry.Type == (int)TermGroup_EDISourceType.EDI)
            {
                if (String.IsNullOrEmpty(ediEntry.InvoiceNr) || ediEntry.ActorSupplierId == null)
                    ediEntry.InvoiceStatus = (int)TermGroup_EDIInvoiceStatus.Error;
                else
                    ediEntry.InvoiceStatus = (int)validatedStatus;
            }
            else
            {
                if (string.IsNullOrEmpty(ediEntry.InvoiceNr) || ediEntry.ActorSupplierId == null ||
                    (ediEntry.Sum < 0 && ediEntry.BillingType != (int)TermGroup_BillingType.Credit) ||
                    (ediEntry.Sum >= 0 && ediEntry.BillingType == (int)TermGroup_BillingType.Credit))
                    ediEntry.InvoiceStatus = (int)TermGroup_EDIInvoiceStatus.Error;
                else
                    ediEntry.InvoiceStatus = (int)validatedStatus;
            }
        }

        private bool SetEdiEntryCurrencyAndAmounts(CompEntities entities, EdiEntry ediEntry, string currencyCode, decimal sum, decimal sumVat, decimal? vatRate, int actorCompanyId)
        {
            bool success = true;

            if (ediEntry == null)
                return false;

            //Currency
            var baseCurrency = CountryCurrencyManager.GetCompanyBaseCurrencyDTO(entities, actorCompanyId);

            CompCurrencyDTO currency = null;
            if (!string.IsNullOrEmpty(currencyCode) && baseCurrency.Code != currencyCode)
            {
                currency = CountryCurrencyManager.GetCompCurrencyDTO(entities, currencyCode, actorCompanyId);
            }

            if (currency != null)
            {
                ediEntry.CurrencyId = currency.CurrencyId;
                ediEntry.CurrencyRate = currency.RateToBase;
                ediEntry.CurrencyDate = currency.Date;

                //Amounts
                ediEntry.SumCurrency = (ediEntry.Type == (int)TermGroup_EDISourceType.Scanning && ediEntry.BillingType == (int)TermGroup_BillingType.Credit && sum > 0 ? Decimal.Negate(sum) : sum);
                ediEntry.SumVatCurrency = (ediEntry.Type == (int)TermGroup_EDISourceType.Scanning && ediEntry.BillingType == (int)TermGroup_BillingType.Credit && sumVat > 0 ? Decimal.Negate(sumVat) : sumVat);

                ediEntry.Sum = (ediEntry.Type == (int)TermGroup_EDISourceType.Scanning && ediEntry.BillingType == (int)TermGroup_BillingType.Credit && sum > 0 ? Decimal.Negate(Math.Round(sum * ediEntry.CurrencyRate, 2)) : Math.Round(sum * ediEntry.CurrencyRate, 2));
                ediEntry.SumVat = (ediEntry.Type == (int)TermGroup_EDISourceType.Scanning && ediEntry.BillingType == (int)TermGroup_BillingType.Credit && sumVat > 0 ? Decimal.Negate(Math.Round(sumVat * ediEntry.CurrencyRate, 2)) : Math.Round(sumVat * ediEntry.CurrencyRate, 2));

                ediEntry.VatRate = vatRate;
            }
            else if (baseCurrency != null)
            {
                ediEntry.CurrencyId = baseCurrency.CurrencyId;
                ediEntry.CurrencyRate = 1;
                ediEntry.CurrencyDate = baseCurrency.Date;

                //Amounts
                ediEntry.Sum = ediEntry.SumCurrency = Math.Round((ediEntry.Type == (int)TermGroup_EDISourceType.Scanning && ediEntry.BillingType == (int)TermGroup_BillingType.Credit && sum > 0 ? Decimal.Negate(sum) : sum), 2);
                ediEntry.SumVat = ediEntry.SumVatCurrency = Math.Round((ediEntry.Type == (int)TermGroup_EDISourceType.Scanning && ediEntry.BillingType == (int)TermGroup_BillingType.Credit && sumVat > 0 ? Decimal.Negate(sumVat) : sumVat), 2);

                ediEntry.VatRate = vatRate;
            }
            else
                success = false;

            return success;
        }

        private bool SetEdiEntryWholeseller(EdiEntry ediEntry, int actorCompanyId)
        {
            return SetEdiEntryWholeseller(ediEntry, ediEntry.WholesellerName, actorCompanyId);
        }

        private bool SetEdiEntryWholeseller(EdiEntry ediEntry, string wholesellerName, int actorCompanyId, int sysWholesellerEdiId = 0)
        {
            bool success = true;

            if (ediEntry == null)
                return false;

            ediEntry.WholesellerName = wholesellerName;

            SysWholeseller sysWholeSeller = SysPriceListManager.GetSysWholeSellerFromName(wholesellerName, actorCompanyId, sysWholesellerEdiId: sysWholesellerEdiId);
            if (sysWholeSeller != null)
            {
                ediEntry.SysWholesellerId = sysWholeSeller.SysWholesellerId;
                ediEntry.WholesellerName = sysWholeSeller.Name;
            }

            return success;
        }

        public ActionResult AddEdiEntryFromSysEdiMessageHeadDTO(Guid? companyApiKey, SysEdiMessageHeadDTO sysEdiMessageHeadDTO, bool TransferToOrder = false, bool TransferToSupplierInvoice = false)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();

            entitiesReadOnly.EdiEntry.NoTracking();

            int? actorCompanyId = null;
            if (companyApiKey != null)
            {
                actorCompanyId = CompanyManager.GetActorCompanyIdFromApiKey(companyApiKey.ToString());
                if (!actorCompanyId.HasValue)
                    return new ActionResult(false);
            }

            if (!actorCompanyId.HasValue || actorCompanyId.Value == 0)
                actorCompanyId = SearchActorCompanyId(sysEdiMessageHeadDTO, sysEdiMessageHeadDTO.SysWholesellerId);

            if (!actorCompanyId.HasValue || actorCompanyId.Value == 0)
                return new ActionResult(false);

            var item = SymbrioEdiItem.CreateItem(sysEdiMessageHeadDTO.XDocument, sysEdiMessageHeadDTO.SysEdiMessageHeadGuid.ToString(), (int)TermGroup_EDISourceType.EDI, true, true);
            var result = TryCreateEdiEntryAndImportToSoftOne(item, actorCompanyId.Value, out EdiEntry ediEntry, sysEdiMessageHeadDTO.SysWholesellerId, TransferToOrder: TransferToOrder, TransferToSupplierInvoice: TransferToSupplierInvoice);
            if (result.Success && ediEntry != null)
                result.IntegerValue = ediEntry.EdiEntryId;

            return result;
        }

        private int? SearchActorCompanyId(SysEdiMessageHeadDTO sysEdiMessageHeadDTO, int sysWholesellerId)
        {
            var key = $"{sysEdiMessageHeadDTO.BuyerId}#{sysWholesellerId}#{sysEdiMessageHeadDTO.BuyerOrganisationNumber}";
            int? actorCompanyId = BusinessMemoryCache<int?>.Get(key);

            if (actorCompanyId.HasValue)
                return actorCompanyId;

            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            var actorCompanyIdsWithWholeseller = entitiesReadOnly.CompanySysWholeseller.Where(s => s.SysWholesellerId == sysWholesellerId).Select(i => i.ActorCompanyId).Distinct().ToList();

            #region CustomerNumber

            if (!string.IsNullOrEmpty(sysEdiMessageHeadDTO.BuyerId) && sysEdiMessageHeadDTO.BuyerId.Length > 2)
            {
                string buyerId = sysEdiMessageHeadDTO.BuyerId.Trim().ToLower();
                var onBuyerNr = entitiesReadOnly.EdiConnection.Where(w => actorCompanyIdsWithWholeseller.Contains(w.ActorCompanyId) && !string.IsNullOrEmpty(w.BuyerNr) && w.BuyerNr.Trim().ToLower().Equals(buyerId)).ToList();

                if (onBuyerNr.Count == 1)
                    actorCompanyId = onBuyerNr.First().ActorCompanyId;

                if (!actorCompanyId.HasValue)
                {
                    buyerId = sysEdiMessageHeadDTO.BuyerId.Trim().ToLower().CleanKeyString();
                    onBuyerNr = entitiesReadOnly.EdiConnection.Where(w => actorCompanyIdsWithWholeseller.Contains(w.ActorCompanyId) && !string.IsNullOrEmpty(w.BuyerNr) && w.BuyerNr.Trim().ToLower().Equals(buyerId)).ToList();

                    if (onBuyerNr.Count == 1)
                        actorCompanyId = onBuyerNr.First().ActorCompanyId;
                }
            }
            #endregion

            #region OrganisationNumber

            if (!actorCompanyId.HasValue)
            {
                if (!string.IsNullOrEmpty(sysEdiMessageHeadDTO.BuyerOrganisationNumber) && sysEdiMessageHeadDTO.BuyerOrganisationNumber.Length > 3)
                {
                    string orgNr = sysEdiMessageHeadDTO.BuyerOrganisationNumber.Trim().ToLower();
                    var OnOrgNr = entitiesReadOnly.Company.Where(w => actorCompanyIdsWithWholeseller.Contains(w.ActorCompanyId) && !string.IsNullOrEmpty(w.OrgNr) && w.OrgNr.Trim().ToLower().Equals(orgNr)).ToList();

                    if (OnOrgNr.Count == 1)
                        actorCompanyId = OnOrgNr.First().ActorCompanyId;

                    if (!actorCompanyId.HasValue)
                    {
                        orgNr = sysEdiMessageHeadDTO.BuyerOrganisationNumber.Trim().ToLower().CleanKeyString();
                        OnOrgNr = entitiesReadOnly.Company.Where(w => actorCompanyIdsWithWholeseller.Contains(w.ActorCompanyId) && !string.IsNullOrEmpty(w.OrgNr) && w.OrgNr.Trim().ToLower().Equals(orgNr)).ToList();

                        if (OnOrgNr.Count == 1)
                            actorCompanyId = OnOrgNr.First().ActorCompanyId;
                    }
                }
                if (!actorCompanyId.HasValue)
                {
                    if (!string.IsNullOrEmpty(sysEdiMessageHeadDTO.BuyerId) && sysEdiMessageHeadDTO.BuyerId.Length > 5)
                    {
                        string buyerId = sysEdiMessageHeadDTO.BuyerId.Trim().ToLower();
                        var matchBuyerIdOnOrgNr = entitiesReadOnly.Company.Where(w => actorCompanyIdsWithWholeseller.Contains(w.ActorCompanyId) && !string.IsNullOrEmpty(w.OrgNr) && w.OrgNr.Trim().ToLower().Equals(buyerId)).ToList();

                        if (matchBuyerIdOnOrgNr.Count == 1)
                            actorCompanyId = matchBuyerIdOnOrgNr.First().ActorCompanyId;

                        if (actorCompanyId.HasValue)
                        {
                            buyerId = sysEdiMessageHeadDTO.BuyerId.Trim().ToLower().CleanKeyString();
                            matchBuyerIdOnOrgNr = entitiesReadOnly.Company.Where(w => actorCompanyIdsWithWholeseller.Contains(w.ActorCompanyId) && !string.IsNullOrEmpty(w.OrgNr) && w.OrgNr.Trim().ToLower().Equals(buyerId)).ToList();

                            if (matchBuyerIdOnOrgNr.Count == 1)
                                actorCompanyId = matchBuyerIdOnOrgNr.First().ActorCompanyId;
                        }
                    }
                }
            }

            #endregion

            BusinessMemoryCache<int?>.Set(key, actorCompanyId ?? 0, actorCompanyId.HasValue ? (6 * 60 * 60) : (30 * 60));

            return actorCompanyId;
        }

        public ActionResult AddEdiEntryFromSysEdiMessageHeadDTO(int actorCompanyId, SysEdiMessageHeadDTO sysEdiMessageHeadDTO)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();

            entitiesReadOnly.EdiEntry.NoTracking();
            var item = SymbrioEdiItem.CreateItem(sysEdiMessageHeadDTO.XDocument, sysEdiMessageHeadDTO.SysEdiMessageHeadGuid.ToString(), sysEdiMessageHeadDTO.EDISourceType, true, true, sysEdiMessageHeadDTO);
            return TryCreateEdiEntryAndImportToSoftOne(item, actorCompanyId, out _, sysEdiMessageHeadDTO.SysWholesellerId);
        }

        public ActionResult AddEdiEntrys(TermGroup_EDISourceType ediSourceType, bool isAutoFromFtp)
        {
            CompanyEdiDTO companyEdi = GetCompanyEdi(base.ActorCompanyId).ToDTO();
            if (companyEdi != null)
                return AddEdiEntrys(companyEdi, ediSourceType, isAutoFromFtp);
            else
                return new ActionResult((int)ActionResultSave.EdiInvalidType, "Invalid EDI source type");
        }

        public ActionResult AddEdiEntrys(CompanyEdiDTO companyEdi, TermGroup_EDISourceType ediSourceType, bool isAutoFromFtp, int sysScheduledJobId = 0, int batchNr = 0)
        {
            if (companyEdi == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "CompanyEdi");

            switch (ediSourceType)
            {
                case TermGroup_EDISourceType.EDI:
                    return AddEdiEntrysFromSource(companyEdi, isAutoFromFtp, sysScheduledJobId, batchNr);
                default:
                    return new ActionResult((int)ActionResultSave.EdiInvalidType, "Invalid EDI source type");
            }
        }

        public ActionResult AddScanningEntrys(TermGroup_EDISourceType ediSourceType, int actorCompanyId)
        {
            switch (ediSourceType)
            {
                case TermGroup_EDISourceType.Scanning:
                    return AddScanningEntrysFromWebService(actorCompanyId);
                case TermGroup_EDISourceType.InExchange:
                    var iem = new ElectronicInvoiceMananger(parameterObject);
                    return iem.AddInexchangeInvoices(actorCompanyId, base.UserId);
                default:
                    return new ActionResult((int)ActionResultSave.EdiInvalidType, "Invalid EDI source type");
            }
        }

        #region Transfer EDI to SupplierInvoice


        public ActionResult TransferToSupplierInvoiceFromEdi(List<int> ediEntryIds, int actorCompanyId, bool checkAutoSetting, bool isAutoFromFtp = false)
        {
            if (checkAutoSetting)
            {
                bool createSupplierInvoice = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.BillingEdiTransferToSupplierInvoice, 0, actorCompanyId, 0);
                if (!createSupplierInvoice)
                    return new ActionResult(false);
            }

            return TransferToSupplierInvoicesFromEdi(ediEntryIds, actorCompanyId, 0, isAutoFromFtp);
        }

        public ActionResult TransferToSupplierInvoiceFromScanning(List<int> ediEntryIds, int actorCompanyId)
        {
            bool createSupplierInvoice = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.ScanningTransferToSupplierInvoice, 0, actorCompanyId, 0);
            if (!createSupplierInvoice)
                return new ActionResult(false);

            return TransferToSupplierInvoicesFromEdi(ediEntryIds, actorCompanyId, 0);
        }

        public ActionResult TransferToSupplierInvoiceFromFinvoice(List<int> ediEntryIds, int actorCompanyId)
        {
            bool createSupplierInvoice = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.FinvoiceTransferToSupplierInvoice, 0, actorCompanyId, 0);
            if (!createSupplierInvoice)
                return new ActionResult(false);

            return TransferToSupplierInvoicesFromEdi(ediEntryIds, actorCompanyId, 0);
        }

        public ActionResult TransferToSupplierInvoicesFromEdiDict(List<int> itemsDict, int actorCompanyId, int userId)
        {
            var ediEntrys = new List<int>();

            foreach (int ediEntryId in itemsDict)
            {
                ediEntrys.Add(ediEntryId);
            }

            return TransferToSupplierInvoicesFromEdi(ediEntrys, actorCompanyId, userId);
        }

        public ActionResult TransferToSupplierInvoicesFromEdi(List<int> ediEntryIds, int actorCompanyId, int userId, bool isAutoFromFtp = false, Dictionary<int, int> attestGroupIds = null)
        {
            var result = new ActionResult(true);

            using (var entities = new CompEntities())
            {
                #region Prereq

                int voucherSeriesTypeId = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.SupplierInvoiceVoucherSeriesType, 0, actorCompanyId, 0);

                #endregion

                foreach (int ediEntryId in ediEntryIds)
                {
                    #region EdiEntry

                    if (ediEntryId == 0)
                        continue;

                    EdiEntry ediEntry = GetEdiEntry(entities, ediEntryId, actorCompanyId);
                    if (ediEntry == null)
                        continue;

                    result = TransferToSupplierInvoiceFromEdi(entities, ediEntry, voucherSeriesTypeId, actorCompanyId, userId);
                    if (!result.Success && result.ErrorNumber == (int)ActionResultSave.EdiFailedTransferToInvoiceInvalidData)
                    {
                        ediEntry.ErrorCode = (int)ActionResultSave.EdiFailedTransferToInvoiceInvalidData;
                        ediEntry.ErrorMessage = result.ErrorMessage;
                        entities.SaveChanges();
                    }

                    #endregion
                }
            }

            return result;
        }

        private ActionResult TransferToSupplierInvoiceFromEdi(CompEntities entities, EdiEntry ediEntry, int voucherSeriesTypeId, int actorCompanyId, int userId)
        {
            ActionResult result = new ActionResult(true);

            // Create attest
            bool createAttestOnEdi = SettingManager.GetBoolSetting(entities, SettingMainType.Company, (int)CompanySettingType.CreateAutoAttestFromSupplierOnEDI, 0, actorCompanyId, 0);
            bool useProductUnitConvert = SettingManager.GetBoolSetting(entities, SettingMainType.Company, (int)CompanySettingType.BillingUseProductUnitConvert, 0, actorCompanyId, 0, false);
            bool defaultInternalAccountsFromOrder = SettingManager.GetBoolSetting(entities, SettingMainType.Company, (int)CompanySettingType.SupplierInvoiceGetInternalAccountsFromOrder, 0, actorCompanyId, 0, false);
            bool saveAsPreliminary = SettingManager.GetBoolSetting(entities, SettingMainType.Company, (int)CompanySettingType.SupplierInvoiceDefaultDraft, 0, actorCompanyId, 0);

            bool addSupplierProductRows = SettingManager.GetBoolSetting(entities, SettingMainType.Company, (int)CompanySettingType.SupplierInvoiceProductRowsImport, 0, actorCompanyId, 0);

            SupplierInvoice supplierInvoice = null;
            int customerInvoiceId = 0;

            List<InvoiceRow> supplierInvoiceRows = null;

            try
            {
                #region Prereq

                if (ediEntry == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "EdiEntry");

                result = IsEdiEntryInvoiceValidForInvoice(ediEntry);
                if (!result.Success)
                    return result;

                //Get AccountYear
                AccountYear accountYear = AccountManager.GetAccountYear(entities, ediEntry.InvoiceDate ?? DateTime.Now.Date, actorCompanyId);
                if (accountYear == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "AccountYear");

                //Get VoucherSeries
                VoucherSeries voucherSerie = VoucherManager.GetVoucherSerieByYear(entities, accountYear.AccountYearId, voucherSeriesTypeId);
                if (voucherSerie == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "VoucherSeries");

                //Get Supplier
                Supplier supplier = null;
                if (ediEntry.ActorSupplierId.HasValue)
                    supplier = SupplierManager.GetSupplier(entities, ediEntry.ActorSupplierId.Value);

                #endregion

                entities.Connection.Open();

                using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                {
                    SymbrioEdiItem item = null;
                    if (ediEntry.Type == (int)TermGroup_EDISourceType.EDI || ediEntry.Type == (int)TermGroup_EDISourceType.Scanning)
                    {
                        #region Edi/Scanning

                        #region Scanning

                        if (ediEntry.Type == (int)TermGroup_EDISourceType.Scanning)
                        {
                            if (!ediEntry.ScanningEntryInvoiceReference.IsLoaded)
                                ediEntry.ScanningEntryInvoiceReference.Load();

                            if (!IsScanningEntryValidForInvoice(ediEntry.ScanningEntryInvoice))
                                return new ActionResult((int)ActionResultSave.EdiFailedTransferToInvoiceInvalidStatus);
                        }

                        #endregion

                        item = SymbrioEdiItem.CreateItem(ediEntry.XML, ediEntry.FileName, ediEntry.Type, false, false);
                        if (item != null)
                        {
                            #region Origin

                            // Add Origin
                            Origin origin = new Origin()
                            {
                                Type = (int)SoeOriginType.SupplierInvoice,
                                Status = (int)SoeOriginStatus.Draft,
                                Description = "",

                                //Set FK
                                VoucherSeriesId = voucherSerie.VoucherSeriesId,
                                ActorCompanyId = actorCompanyId,
                            };
                            SetCreatedProperties(origin);

                            #endregion

                            #region SupplierInvoice

                            PaymentInformationRow defaultPaymentInformationRow = ediEntry.ActorSupplierId.HasValue ? PaymentManager.GetDefaultPaymentInformationRow(entities, ediEntry.ActorSupplierId.Value) : null;

                            supplierInvoice = new SupplierInvoice()
                            {
                                // Inherited from Invoice
                                Type = (int)SoeInvoiceType.SupplierInvoice,
                                VatType = (int)TermGroup_InvoiceVatType.Merchandise,
                                InvoiceNr = ediEntry.InvoiceNr,
                                SeqNr = null,
                                InvoiceDate = ediEntry.InvoiceDate,
                                DueDate = ediEntry.DueDate,
                                VoucherDate = ediEntry.InvoiceDate,
                                ReferenceOur = !string.IsNullOrEmpty(ediEntry.BuyerReference) ? ediEntry.BuyerReference : string.Empty,
                                ReferenceYour = item.SellerReference ?? string.Empty,
                                OCR = (supplier != null && supplier.CopyInvoiceNrToOcr) ? ediEntry.InvoiceNr : item.HeadInvoiceOcr,
                                VATAmount = ediEntry.SumVat,
                                VATAmountCurrency = 0,
                                PaidAmount = 0,
                                PaidAmountCurrency = 0,
                                CurrencyRate = ediEntry.CurrencyRate,
                                CurrencyDate = DateTime.Now.Date,
                                SysPaymentTypeId = defaultPaymentInformationRow != null ? defaultPaymentInformationRow.SysPaymentTypeId : (int?)null,
                                PaymentNr = defaultPaymentInformationRow != null ? defaultPaymentInformationRow.PaymentNr : "",
                                FullyPayed = false,
                                OnlyPayment = false,
                                InterimInvoice = false,
                                MultipleDebtRows = false,
                                BlockPayment = false,

                                //Set FK
                                ActorId = ediEntry.ActorSupplierId.Value,
                                CurrencyId = ediEntry.CurrencyId,

                                // Set references
                                Origin = origin,
                            };

                            if (ediEntry.ScanningEntryInvoice != null)
                            {
                                //handle value from row of type ReferenceYour (ReadSoft: buyercontactpersonname - ErReferens)
                                //Value can be separated by using '#'. First part is saved as referenceOur, second part is considered as
                                //order number, billing project number or accounting project number depending on company setting
                                string reference = ediEntry.ScanningEntryInvoice.GetReferenceYour();
                                var splittedReference = reference.Split('#');
                                string referenceOur = splittedReference.Length > 0 ? splittedReference[0] : string.Empty;
                                string referenceCode = splittedReference.Length > 1 ? splittedReference[1] : string.Empty;

                                //first part of ReferenceYour
                                supplierInvoice.ReferenceOur = Regex.Replace(referenceOur, @"[\d-]", string.Empty);

                                //second part of ReferenceYour
                                if (referenceCode != string.Empty)
                                {
                                    int scanningCodeTargetField = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.ScanningCodeTargetField, userId, actorCompanyId, 0);

                                    if (scanningCodeTargetField == (int)TermGroup_ScanningCodeTargetField.OrderNumber)
                                    {
                                        item.HeadBuyerOrderNumber = referenceCode;
                                    }
                                    else if (scanningCodeTargetField == (int)TermGroup_ScanningCodeTargetField.BillingProject)
                                    {
                                        item.HeadBuyerProjectNumber = referenceCode;
                                    }
                                    else if (scanningCodeTargetField == (int)TermGroup_ScanningCodeTargetField.AccountingProject)
                                    {
                                        //get dimNr for project
                                        AccountDim accountDim = AccountManager.GetAccountDimBySieNr(entities, (int)TermGroup_SieAccountDim.Project, actorCompanyId);
                                        int dimNr = accountDim != null ? accountDim.AccountDimNr : 0;

                                        Account project = AccountManager.GetAccountByNr(entities, referenceCode, accountDim.AccountDimId, actorCompanyId);
                                        if (dimNr == 2)
                                            supplierInvoice.DefaultDim2AccountId = project?.AccountId ?? 0;
                                        if (dimNr == 3)
                                            supplierInvoice.DefaultDim3AccountId = project?.AccountId ?? 0;
                                        if (dimNr == 4)
                                            supplierInvoice.DefaultDim4AccountId = project?.AccountId ?? 0;
                                        if (dimNr == 5)
                                            supplierInvoice.DefaultDim5AccountId = project?.AccountId ?? 0;
                                        if (dimNr == 6)
                                            supplierInvoice.DefaultDim6AccountId = project?.AccountId ?? 0;
                                    }
                                    else if (scanningCodeTargetField == (int)TermGroup_ScanningCodeTargetField.Costplace)
                                    {
                                        //get dimNr for costplace
                                        AccountDim accountDim = AccountManager.GetAccountDimBySieNr(entities, (int)TermGroup_SieAccountDim.CostCentre, actorCompanyId);
                                        int dimNr = accountDim != null ? accountDim.AccountDimNr : 0;

                                        Account costPlace = AccountManager.GetAccountByNr(entities, referenceCode, accountDim.AccountDimId, actorCompanyId);
                                        if (dimNr == 2)
                                            supplierInvoice.DefaultDim2AccountId = costPlace?.AccountId ?? 0;
                                        if (dimNr == 3)
                                            supplierInvoice.DefaultDim3AccountId = costPlace?.AccountId ?? 0;
                                        if (dimNr == 4)
                                            supplierInvoice.DefaultDim4AccountId = costPlace?.AccountId ?? 0;
                                        if (dimNr == 5)
                                            supplierInvoice.DefaultDim5AccountId = costPlace?.AccountId ?? 0;
                                        if (dimNr == 6)
                                            supplierInvoice.DefaultDim6AccountId = costPlace?.AccountId ?? 0;
                                    }
                                }


                                //handle value from row of type ReferenceOur (ReadSoft: buyercontactreference - Referensnummer)
                                int scanningReferenceTargetField = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.ScanningReferenceTargetField, userId, actorCompanyId, 0);

                                if (scanningReferenceTargetField == (int)TermGroup_ScanningReferenceTargetField.Costplace)
                                {
                                    //get dimNr for costplace
                                    AccountDim accountDim = AccountManager.GetAccountDimBySieNr(entities, (int)TermGroup_SieAccountDim.CostCentre, actorCompanyId);
                                    int dimNr = accountDim?.AccountDimNr ?? 0;

                                    Account costPlace = AccountManager.GetAccountByNr(entities, ediEntry.ScanningEntryInvoice.GetReferenceOur(), accountDim.AccountDimId, actorCompanyId);
                                    if (dimNr == 2)
                                        supplierInvoice.DefaultDim2AccountId = costPlace?.AccountId ?? 0;
                                    if (dimNr == 3)
                                        supplierInvoice.DefaultDim3AccountId = costPlace?.AccountId ?? 0;
                                    if (dimNr == 4)
                                        supplierInvoice.DefaultDim4AccountId = costPlace?.AccountId ?? 0;
                                    if (dimNr == 5)
                                        supplierInvoice.DefaultDim5AccountId = costPlace?.AccountId ?? 0;
                                    if (dimNr == 6)
                                        supplierInvoice.DefaultDim6AccountId = costPlace?.AccountId ?? 0;
                                }
                                else if (scanningReferenceTargetField == (int)TermGroup_ScanningReferenceTargetField.Project)
                                {
                                    item.HeadBuyerProjectNumber = ediEntry.ScanningEntryInvoice.GetReferenceOur();
                                }
                                else if (scanningReferenceTargetField == (int)TermGroup_ScanningReferenceTargetField.Order)
                                {
                                    item.HeadBuyerOrderNumber = ediEntry.ScanningEntryInvoice.GetReferenceOur();
                                }
                            }

                            //Should be before HeadBuyerProjectNumber since it will set default project from the order
                            if (!string.IsNullOrEmpty(item.HeadBuyerOrderNumber))
                            {
                                SupplierInvoiceManager.SetSupplierOrderNr(entities, supplierInvoice, item.HeadBuyerOrderNumber, actorCompanyId, defaultInternalAccountsFromOrder, out customerInvoiceId);
                            }

                            if (!string.IsNullOrEmpty(item.HeadBuyerProjectNumber) && (item.HeadBuyerOrderNumber != item.HeadBuyerProjectNumber))
                            {
                                var project = ProjectManager.GetProjectByNumber(entities, actorCompanyId, item.HeadBuyerProjectNumber);
                                if (project != null)
                                {
                                    supplierInvoice.ProjectId = project.ProjectId;
                                }
                            }

                            int projectId = supplierInvoice.ProjectId != null ? (int)supplierInvoice.ProjectId : 0;

                            //Define Attest group
                            AttestWorkFlowGroup attestWorkFlowGroup = AttestManager.GetAttestGroupSuggestion(entities, actorCompanyId, ediEntry.ActorSupplierId.Value, projectId, 0, supplierInvoice.ReferenceOur);
                            supplierInvoice.AttestGroupId = attestWorkFlowGroup != null ? (int?)attestWorkFlowGroup.AttestWorkFlowHeadId : null;

                            if (ediEntry.Type == (int)TermGroup_EDISourceType.Scanning && ediEntry.BillingType != null)
                            {
                                supplierInvoice.BillingType = (int)ediEntry.BillingType;

                                if (supplierInvoice.BillingType == (int)TermGroup_BillingType.Credit && ediEntry.Sum > 0)
                                {
                                    supplierInvoice.TotalAmount = supplierInvoice.TotalAmountCurrency = decimal.Negate(ediEntry.Sum);
                                    supplierInvoice.VATAmount = decimal.Negate(ediEntry.SumVat);
                                }
                                else
                                {
                                    supplierInvoice.TotalAmount = supplierInvoice.TotalAmountCurrency = ediEntry.Sum;
                                    supplierInvoice.VATAmount = ediEntry.SumVat;
                                }
                            }
                            else
                            {
                                if (ediEntry.BillingType == (int)TermGroup_BillingType.Credit)
                                {
                                    supplierInvoice.BillingType = (int)TermGroup_BillingType.Credit;
                                    supplierInvoice.TotalAmount = supplierInvoice.TotalAmountCurrency = ediEntry.Sum > 0 ? Decimal.Negate(ediEntry.Sum) : ediEntry.Sum;
                                    supplierInvoice.VATAmount = ediEntry.SumVat > 0 ? Decimal.Negate(ediEntry.SumVat) : ediEntry.SumVat;
                                }
                                else
                                {
                                    supplierInvoice.BillingType = (int)TermGroup_BillingType.Debit;
                                    supplierInvoice.TotalAmount = supplierInvoice.TotalAmountCurrency = ediEntry.Sum;
                                    supplierInvoice.VATAmount = ediEntry.SumVat;
                                }
                            }

                            #endregion

                        }


                        #endregion
                    }
                    else if (ediEntry.Type == (int)TermGroup_EDISourceType.Finvoice)
                    {
                        #region Finvoice                        

                        XDocument xdoc = XDocument.Parse(ediEntry.XML);
                        if (xdoc != null)
                        {
                            FinvoiceEdiItem finvoice = new FinvoiceEdiItem(xdoc, this.parameterObject);
                            if (finvoice != null)
                            {
                                #region Origin

                                // Add Origin
                                Origin origin = new Origin()
                                {
                                    VoucherSeriesId = voucherSerie.VoucherSeriesId,
                                    Type = (int)SoeOriginType.SupplierInvoice,
                                    Status = saveAsPreliminary ? (int)SoeOriginStatus.Draft : (int)SoeOriginStatus.Origin,
                                    Description = "",

                                    //Set FK
                                    ActorCompanyId = actorCompanyId,
                                };
                                SetCreatedProperties(origin);

                                #endregion

                                #region SupplierInvoice

                                supplierInvoice = new SupplierInvoice
                                {
                                    // Inherited from Invoice
                                    Type = (int)SoeInvoiceType.SupplierInvoice,
                                    BillingType = finvoice.invoiceDetails.SoeCompatibleBillingType,
                                    VatType = finvoice.invoiceDetails.VatType,
                                    InvoiceNr = ediEntry.InvoiceNr,
                                    SeqNr = null,
                                    InvoiceDate = ediEntry.InvoiceDate,
                                    DueDate = ediEntry.DueDate,
                                    VoucherDate = ediEntry.InvoiceDate,
                                    ReferenceOur = "",
                                    ReferenceYour = "",
                                    OCR = (supplier != null && supplier.CopyInvoiceNrToOcr) ? ediEntry.InvoiceNr : finvoice.epiDetails.EpiRemittanceInfoIdentifier,
                                    TotalAmount = ediEntry.Sum,
                                    TotalAmountCurrency = ediEntry.SumCurrency,
                                    VATAmount = ediEntry.SumVat,
                                    PaidAmount = 0,
                                    PaidAmountCurrency = 0,
                                    FullyPayed = false,
                                    CurrencyRate = ediEntry.CurrencyRate,
                                    CurrencyDate = DateTime.Now.Date,
                                    OnlyPayment = false,
                                    SysPaymentTypeId = null,
                                    PaymentNr = finvoice.epiDetails.EpiAccountID,
                                    //AttestGroupId = attestWorkFlowHeadId, //(19856 - deactive when EFH ready for test)                    
                                    // SupplierInvoice
                                    InterimInvoice = false,
                                    MultipleDebtRows = false,
                                    BlockPayment = false,
                                    TimeDiscountDate = finvoice.invoiceDetails.TimeDiscountDate,
                                    TimeDiscountPercent = finvoice.invoiceDetails.TimeDiscountPercent,
                                    // Set references
                                    Origin = origin,
                                    ActorId = ediEntry.ActorSupplierId,
                                    CurrencyId = ediEntry.CurrencyId,
                                    ExternalId = finvoice.MessageTransmissionDetails.MessageIdentifier
                                };

                                supplierInvoiceRows = finvoice.invoiceRows;

                                //VatType...
                                if (supplierInvoice.VATAmount == 0 && supplierInvoice.VatType == (int)TermGroup_InvoiceVatType.NoVat)
                                {
                                    var supplierVatType = SupplierManager.GetSupplierVatType(entities, supplierInvoice.ActorId.GetValueOrDefault(), actorCompanyId);
                                    supplierInvoice.VatType = supplierVatType == (int)TermGroup_InvoiceVatType.Contractor ? (int)TermGroup_InvoiceVatType.Contractor : (int)TermGroup_InvoiceVatType.NoVat;
                                }

                                //VatCode
                                VatCode vatCode = AccountManager.GetVatCodeByVateRate(entities, actorCompanyId, finvoice.invoiceDetails.VatRatePercent);
                                if (vatCode != null)
                                {
                                    supplierInvoice.VatCode = vatCode;
                                    supplierInvoice.VatCodeId = vatCode.VatCodeId;
                                }

                                //ProjectId
                                int? projectId = null;

                                //Try to define project from orderNr:
                                if (ediEntry.OrderNr != "" && ediEntry.OrderNr != null)
                                {
                                    projectId = (from p in entities.Invoice
                                                 .Include("Origin")
                                                 where p.InvoiceNr == ediEntry.OrderNr &&
                                                 p.Origin.Type == (int)SoeOriginType.Order
                                                 select p.ProjectId).FirstOrDefault();
                                }

                                //Try to define project from worksitekey:                                
                                string workSiteKey = "";
                                if (finvoice.invoiceDetails.WorkSiteKey != "" && finvoice.invoiceDetails.WorkSiteKey != null)
                                    workSiteKey = finvoice.invoiceDetails.WorkSiteKey;
                                else if (finvoice.DeliverySiteCode != "" && finvoice.DeliverySiteCode != null)
                                    workSiteKey = finvoice.DeliverySiteCode;
                                else if (finvoice.invoiceDetails.BuyerReferenceIdentifier != "" && finvoice.invoiceDetails.BuyerReferenceIdentifier != null)
                                    workSiteKey = finvoice.invoiceDetails.BuyerReferenceIdentifier;

                                if (workSiteKey != "")
                                {
                                    projectId = (from p in entities.Project
                                                 where p.WorkSiteKey == workSiteKey ||
                                                 p.WorkSiteNumber == workSiteKey
                                                 select p.ProjectId).FirstOrDefault();
                                }

                                if (projectId == null)
                                    projectId = 0;

                                if (projectId > 0)
                                {
                                    supplierInvoice.ProjectId = projectId;
                                }

                                //Define Attest group
                                AttestWorkFlowGroup attestWorkFlowGroup = AttestManager.GetAttestGroupSuggestion(entities, ActorCompanyId, supplier.ActorSupplierId, (int)projectId, 0, supplierInvoice.ReferenceOur);
                                supplierInvoice.AttestGroupId = attestWorkFlowGroup != null ? (int?)attestWorkFlowGroup.AttestWorkFlowHeadId : null;

                                #endregion

                                #region Supplier payment information

                                //update payment information
                                XElement EpiDetails = XmlUtil.GetChildElement(xdoc, "EpiDetails");
                                XElement EpiPartyDetails = XmlUtil.GetChildElement(EpiDetails, "EpiPartyDetails");
                                XElement EpiBfiPartyDetails = XmlUtil.GetChildElement(EpiPartyDetails, "EpiBfiPartyDetails");
                                XElement EpiBeneficiaryPartyDetails = XmlUtil.GetChildElement(EpiPartyDetails, "EpiBeneficiaryPartyDetails");

                                string bic = EpiBfiPartyDetails != null ? XmlUtil.GetChildElementValue(EpiBfiPartyDetails, "EpiBfiIdentifier") : string.Empty;
                                string iban = EpiBeneficiaryPartyDetails != null ? XmlUtil.GetChildElementValue(EpiBeneficiaryPartyDetails, "EpiAccountID") : string.Empty;

                                //get existing payment information
                                PaymentInformation paymentInformation = PaymentManager.GetPaymentInformationFromActor(entities, supplier.ActorSupplierId, true, false);

                                //create new payment information if it does not exist
                                if (paymentInformation == null)
                                {
                                    paymentInformation = new PaymentInformation()
                                    {
                                        Actor = supplier.Actor,
                                        DefaultSysPaymentTypeId = (int)TermGroup_SysPaymentType.BIC,
                                    };
                                    SetCreatedProperties(paymentInformation);
                                }

                                //check if row already exists
                                var paymentInformationRow = paymentInformation.ActivePaymentInformationRows.FirstOrDefault(i => i.PaymentNr.Contains(iban));

                                bool setAsDefault = !paymentInformation.ActivePaymentInformationRows.Any(i => i.SysPaymentTypeId == (int)TermGroup_SysPaymentType.BIC && i.Default);

                                //create new row if it does not exist
                                if (paymentInformationRow == null)
                                {
                                    paymentInformationRow = PaymentManager.AddPaymentInformationRowToPaymentInformation(paymentInformation, TermGroup_SysPaymentType.BIC, iban, setAsDefault, bic);
                                }

                                if (paymentInformationRow != null)
                                {
                                    supplierInvoice.SysPaymentTypeId = paymentInformationRow != null ? paymentInformationRow.SysPaymentTypeId : (int?)null;
                                    supplierInvoice.PaymentNr = paymentInformationRow != null ? paymentInformationRow.PaymentNr : "";
                                }

                                //supplierInvoice.paymen
                                #endregion

                                #region Supplier org.number

                                //update missing org.number to supplier
                                XElement SellerPartyDetails = XmlUtil.GetChildElement(xdoc, "SellerPartyDetails");
                                if (!supplier.OrgNr.HasValue())
                                    supplier.OrgNr = XmlUtil.GetChildElementValue(SellerPartyDetails, "SellerPartyIdentifier");
                                if (!supplier.VatNr.HasValue())
                                    supplier.VatNr = XmlUtil.GetChildElementValue(SellerPartyDetails, "SellerOrganisationTaxCode");


                                #endregion
                            }
                        }

                        #endregion
                    }

                    #region SupplierInvoice

                    if (supplierInvoice != null)
                    {
                        #region ProductRows
                        List<SupplierInvoiceProductRowDTO> productRows = new List<SupplierInvoiceProductRowDTO>();
                        if (supplierInvoiceRows != null && addSupplierProductRows)
                        {
                            foreach (InvoiceRow invoiceRow in supplierInvoiceRows)
                            {
                                var rowType = SupplierInvoiceRowType.Unknown;
                                var sellerProductNumber = invoiceRow.ArticleIdentifier;
                                if (invoiceRow.RowVatExcludedAmount != 0 || invoiceRow.RowVatAmount != 0)
                                {
                                    rowType = SupplierInvoiceRowType.ProductRow;
                                    if (sellerProductNumber.IsNullOrEmpty())
                                    {
                                        InvoiceProduct product = ProductManager.GetInvoiceProductFromSetting(entities, CompanySettingType.ProductMisc, actorCompanyId);
                                        sellerProductNumber = product.Number;
                                    }
                                }
                                else if (invoiceRow.RowVatExcludedAmount == 0 || invoiceRow.RowVatAmount == 0)
                                {
                                    rowType = SupplierInvoiceRowType.TextRow;
                                }

                                decimal quantity = invoiceRow.InvoicedQuantity;
                                string unitCode = invoiceRow.InvoicedQuantityUnitCode;
                                if (invoiceRow.InvoicedQuantityUnitCode.IsNullOrEmpty())
                                {
                                    quantity = invoiceRow.DeliveredQuantity;
                                    unitCode = invoiceRow.DeliveredQuantityUnitCode;
                                }

                                decimal vatAmountCurrency = invoiceRow.RowVatAmount;
                                if (vatAmountCurrency == 0)
                                {
                                    vatAmountCurrency = invoiceRow.RowVatExcludedAmount * invoiceRow.RowVatRatePercent / 100;
                                }

                                var productRow = new SupplierInvoiceProductRowDTO()
                                {
                                    SupplierInvoiceId = 0,
                                    SellerProductNumber = sellerProductNumber,
                                    Text = invoiceRow.ArticleName,
                                    UnitCode = unitCode,
                                    Quantity = quantity,
                                    PriceCurrency = invoiceRow.UnitPriceAmount,
                                    AmountCurrency = invoiceRow.RowVatExcludedAmount,
                                    VatAmountCurrency = vatAmountCurrency,
                                    VatRate = invoiceRow.RowVatRatePercent,
                                    State = SoeEntityState.Active,
                                    RowType = rowType,
                                };
                                productRows.Add(productRow);
                            }
                        }
                        #endregion

                        #region InvoiceText
                        SupplierInvoiceManager.AddInvoiceTextActionsFromEdiEntry(entities, ediEntry, supplierInvoice);
                        #endregion

                        //Accounting rows
                        result = SupplierInvoiceManager.AddSupplierInvoiceRows(entities, supplierInvoice, productRows, actorCompanyId);
                        if (!result.Success)
                            return result;

                        //Calculate currency amounts
                        CountryCurrencyManager.CalculateCurrencyAmounts(entities, actorCompanyId, supplierInvoice);

                        result = AddEntityItem(entities, supplierInvoice, "Invoice", transaction);
                        if (result.Success)
                        {
                            #region Update EdiEntry

                            ediEntry.InvoiceId = supplierInvoice.InvoiceId;
                            ediEntry.InvoiceStatus = (int)TermGroup_EDIInvoiceStatus.Processed;
                            SetModifiedProperties(ediEntry);

                            if (ediEntry.Type == (int)TermGroup_EDISourceType.EDI)
                            {
                                if ((CloseEdiConditionIsSupplierInvoice(entities, actorCompanyId)))
                                    ediEntry.State = (int)SoeEntityState.Inactive;
                                if (CloseEdiConditionIsOrderAndSupplierInvoice(entities, actorCompanyId) && ediEntry.InvoiceStatus == (int)TermGroup_EDIInvoiceStatus.Processed && ediEntry.InvoiceId.HasValue && ediEntry.OrderId.HasValue)
                                    ediEntry.State = (int)SoeEntityState.Inactive;
                                else if (CloseEdiConditionIsOrderOrSupplierInvoice(entities, actorCompanyId) && (ediEntry.InvoiceId.HasValue || ediEntry.OrderId.HasValue))
                                    ediEntry.State = (int)SoeEntityState.Inactive;
                            }

                            #endregion

                            #region Save ProductRows

                            if (productRows != null && addSupplierProductRows)
                            {
                                foreach (SupplierInvoiceProductRowDTO productRow in productRows)
                                {
                                    productRow.SupplierInvoiceId = supplierInvoice.InvoiceId;
                                }
                                result = SupplierInvoiceManager.SaveSupplierInvoiceProductRows(entities, ActorCompanyId, supplierInvoice, productRows);
                                if (!result.Success)
                                    return result;
                            }

                            #endregion

                            #region Update ScanningEntry

                            if (!ediEntry.ScanningEntryInvoiceReference.IsLoaded)
                                ediEntry.ScanningEntryInvoiceReference.Load();

                            if (ediEntry.ScanningEntryInvoice != null)
                            {
                                ediEntry.ScanningEntryInvoice.Status = (int)TermGroup_ScanningStatus.Processed;
                                SetModifiedProperties(ediEntry.ScanningEntryInvoice);

                                #region Update EdiEntry

                                if (CloseScanningWhenTransferedToSupplierInvoice(entities, actorCompanyId))
                                    ediEntry.State = (int)SoeEntityState.Inactive;

                                #endregion
                            }

                            #endregion
                        }
                    }

                    #endregion

                    result = SaveChanges(entities, transaction);

                    if (supplierInvoice != null && result.Success)
                    {
                        #region Sequence number

                        if ((supplierInvoice.SeqNr == null || supplierInvoice.SeqNr == 0) && supplierInvoice.Origin.Status != (int)SoeOriginStatus.Draft)
                        {
                            supplierInvoice.SeqNr = InvoiceManager.GetNextSequenceNumber(entities, SoeOriginType.SupplierInvoice, (SoeOriginStatus)supplierInvoice.Origin.Status, (TermGroup_BillingType)supplierInvoice.BillingType, actorCompanyId, false);
                            ediEntry.SeqNr = ediEntry.Type == (int)TermGroup_EDISourceType.Finvoice ? supplierInvoice.SeqNr : null;

                            result = SaveChanges(entities);
                        }
                        #endregion
                    }

                    if (item != null)
                    {
                        var stockItems = item.Rows.Where(x => !string.IsNullOrEmpty(x.StockCode));
                        if (stockItems.Any())
                        {
                            foreach (var stockItem in stockItems)
                            {
                                var stockResult = UpdateStockFromEdiMessage(stockItem, entities, transaction, actorCompanyId, TermGroup_StockTransactionType.Add, 0, supplierInvoice.InvoiceId, "EDI SupplierInvoice:" + supplierInvoice.InvoiceNr, useProductUnitConvert, out _);
                                if ((!stockResult.Success))
                                {
                                    result = stockResult;
                                    return result;
                                }
                            }
                        }
                    }

                    #region Map Attachments
                    if (result.Success)
                    {
                        var dataStorageRecord = GeneralManager.GetDataStorageRecord(entities, actorCompanyId, ediEntry.EdiEntryId, SoeDataStorageRecordType.EdiEntry_Document);
                        if (dataStorageRecord != null)
                        {
                            var dataStorage = GeneralManager.GetDataStorageByReference(entities, dataStorageRecord.DataStorageId);
                            if (dataStorage != null)
                            {
                                dataStorage.Type = (int)SoeDataStorageRecordType.OrderInvoiceFileAttachment;
                                GeneralManager.CreateDataStorageRecord(entities,
                                    type: SoeDataStorageRecordType.OrderInvoiceFileAttachment,
                                    recordId: supplierInvoice.InvoiceId,
                                    recordNumber: supplierInvoice.SeqNr.ToString(),
                                    entityType: SoeEntityType.None,
                                    dataStorage: dataStorage
                                );

                                var attachmentResult = SaveChanges(entities);
                                if (!attachmentResult.Success)
                                {
                                    result = attachmentResult;
                                    return result;
                                }
                            }
                        }
                    }

                    #endregion

                    //Add projectrows....
                    #region TimeCodeTransaction
                    if (result.Success && (supplierInvoice != null) && (supplierInvoice.ProjectId.GetValueOrDefault() != 0))
                    {
                        int standardTimeCodeId = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.ProjectDefaultTimeCodeId, 0, actorCompanyId, 0);
                        bool chargeCostsToProject = SettingManager.GetBoolSetting(entities, SettingMainType.Company, (int)CompanySettingType.ProjectChargeCostsToProject, 0, actorCompanyId, 0);

                        var projectRow = new List<SupplierInvoiceProjectRowDTO>() {
                                    new SupplierInvoiceProjectRowDTO()
                                    {
                                        //General
                                        State = (int)SoeEntityState.Active,
                                        //TimeCodeTransaction
                                        TimeCodeTransactionId = 0,
                                        Amount = supplierInvoice.TotalAmount - supplierInvoice.VATAmount,
                                        AmountCurrency = supplierInvoice.TotalAmountCurrency - supplierInvoice.VATAmountCurrency,
                                        AmountLedgerCurrency = 0,
                                        AmountEntCurrency = 0,
                                        //TimeInvoiceTransaction
                                        TimeInvoiceTransactionId = 0,
                                        //SupplierInvoice
                                        SupplierInvoiceId = supplierInvoice.InvoiceId,
                                        CustomerInvoiceId = customerInvoiceId,
                                        //Project
                                        ProjectId = (int)supplierInvoice.ProjectId,
                                        ProjectNr = string.Empty,
                                        ProjectName = string.Empty,
                                        ProjectDescription = string.Empty,
                                        //TimeCode                                    
                                        TimeCodeId = standardTimeCodeId,
                                        TimeCodeCode = string.Empty,
                                        TimeCodeName = string.Empty,
                                        TimeCodeDescription = string.Empty,
                                        //Employee
                                        EmployeeId = null,
                                        EmployeeNr = string.Empty,
                                        EmployeeName = string.Empty,
                                        EmployeeDescription = string.Empty,
                                        //TimeBlockDate
                                        TimeBlockDateId = null,
                                        Date = DateTime.Today,
                                        ChargeCostToProject = chargeCostsToProject,
                                    }
                            };

                        var resultProjectRows = ProjectManager.SaveSupplierInvoiceProjectRows(entities, transaction, supplierInvoice, projectRow, actorCompanyId, true);
                        if (resultProjectRows.Success && customerInvoiceId > 0 && resultProjectRows.Value != null)
                        {
                            var customerInvoice = InvoiceManager.GetCustomerInvoice(entities, customerInvoiceId, loadInvoiceAttachments: true);
                            if (customerInvoice != null && (customerInvoice.InvoiceAttachment == null || !customerInvoice.InvoiceAttachment.Any(a => a.EdiEntryId == ediEntry.EdiEntryId)))
                                InvoiceAttachmentManager.AddInvoiceAttachment(entities, customerInvoiceId, ediEntry.EdiEntryId, InvoiceAttachmentSourceType.Edi, InvoiceAttachmentConnectType.SupplierInvoice, customerInvoice.AddAttachementsToEInvoice, true);
                        }
                    }

                    #endregion

                    //Commit transaction
                    if (result.Success)
                        transaction.Complete();
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
                    //Set success properties

                    if (supplierInvoice != null && supplierInvoice.ActorId != null)
                    {
                        // Create Attest
                        if (ediEntry.Type == (int)TermGroup_EDISourceType.Scanning || ediEntry.Type == (int)TermGroup_EDISourceType.Finvoice || (ediEntry.Type == (int)TermGroup_EDISourceType.EDI && createAttestOnEdi))
                        {
                            AttestWorkFlowHead supplierTemplateAttest = supplierInvoice.AttestGroupId != null ? AttestManager.GetAttestWorkFlowHead(entities, (int)supplierInvoice.AttestGroupId, true) : null;

                            if (supplierTemplateAttest != null)
                            {
                                if (!supplierTemplateAttest.AttestWorkFlowRow.IsLoaded)
                                    supplierTemplateAttest.AttestWorkFlowRow.Load();

                                AttestWorkFlowHeadDTO dto = supplierTemplateAttest.ToDTO(true, false);
                                dto.AttestWorkFlowHeadId = 0;
                                dto.Entity = SoeEntityType.SupplierInvoice;
                                dto.RecordId = supplierInvoice.InvoiceId;
                                dto.Rows = new List<AttestWorkFlowRowDTO>();

                                foreach (AttestWorkFlowRow row in supplierTemplateAttest.AttestWorkFlowRow)
                                {
                                    AttestWorkFlowRowDTO rowDto = row.ToDTO(false);
                                    rowDto.AttestWorkFlowRowId = 0;
                                    dto.Rows.Add(rowDto);
                                }

                                // If automatic creation of supplier invoice from ftp
                                // userId is 0 and the user of the person creating the template
                                // on the supplier will be set to invoice attest
                                if (userId == 0)
                                {
                                    AttestWorkFlowRowDTO regRow = dto.Rows.FirstOrDefault(r => r.ProcessType == TermGroup_AttestWorkFlowRowProcessType.Registered);

                                    if (regRow != null && regRow.UserId != null)
                                        userId = (int)regRow.UserId;
                                }

                                AttestManager.SaveAttestWorkFlow(dto, dto.Rows, dto.SendMessage, actorCompanyId, userId);
                            }
                        }
                    }
                }
                else
                    base.LogTransactionFailed(this.ToString(), this.log);

                entities.Connection.Close();
            }

            try
            {
                if (ediEntry.Type == (int)TermGroup_EDISourceType.EDI)
                {
                    ReportDataManager.GenerateReportForEdi(new List<int> { ediEntry.EdiEntryId }, actorCompanyId);
                }
                else if (ediEntry.Type == (int)TermGroup_EDISourceType.Finvoice)
                {
                    ReportDataManager.GenerateReportForFinvoice(new List<int> { ediEntry.EdiEntryId }, actorCompanyId);
                }
            }
            catch (Exception ex)
            {
                base.LogError(ex, this.log);
            }

            return result;
        }

        public ActionResult EdiEntrySaveInvoiceTextAction(int ediEntryId, InvoiceTextType type, bool underInvestigation, string reason = null)
        {
            ActionResult result = new ActionResult();

            if (ediEntryId == 0)
                return new ActionResult((int)ActionResultSave.EntityNotFound, "EdiEntryId must be provided");

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    #region Prereq

                    EdiEntry ediEntry = EdiManager.GetEdiEntry(entities, ediEntryId, this.ActorCompanyId);

                    if (ediEntry == null)
                        return new ActionResult((int)ActionResultSave.EntityNotFound, "SupplierInvoice");

                    #endregion

                    #region SupplierInvoice Update

                    if (type == InvoiceTextType.UnderInvestigationReason)
                        ediEntry.UnderInvestigation = underInvestigation;

                    SetModifiedProperties(ediEntry);
                    result = SaveChanges(entities);

                    #endregion

                    #region Reason

                    if (result.Success)
                    {
                        result = SupplierInvoiceManager.UpdateInvoiceText(entities, null, ediEntryId, underInvestigation, InvoiceTextType.UnderInvestigationReason, reason);
                    }

                    if (result.Success)
                        result.Modified = ediEntry.Modified ?? DateTime.Now;

                    #endregion
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    result.Exception = ex;
                }

                return result;
            }

        }

        private ActionResult UpdateStockFromEdiMessage(SymbrioEdiRowItem ediRowItem, CompEntities entities, TransactionScope transaction, int actorCompanyId, TermGroup_StockTransactionType transType, int productId, int? invoiceId, string note, bool useProductUnitConvert, out int stockId)
        {
            stockId = 0;
            InvoiceProduct invoiceProduct = null;
            if (productId == 0)
            {
                invoiceProduct = ProductManager.GetInvoiceProductByProductNr(entities, ediRowItem.RowSellerArticleNumber, actorCompanyId);
                if (invoiceProduct != null)
                {
                    productId = invoiceProduct.ProductId;
                }
            }

            if (productId == 0)
            {
                return new ActionResult { ErrorMessage = $"Supplier invoice from EDI faild finding product: {ediRowItem.RowSellerArticleNumber}", Success = false, ErrorNumber = (int)ActionResultSave.EdiFailedTransferToInvoiceInvalidData };
            }

            var inventoryList = StockManager.GetStocksForInvoiceProduct(entities, actorCompanyId, productId);
            var productStock = inventoryList.FirstOrDefault(x => x.Code == ediRowItem.StockCode);

            if (productStock == null && transType == TermGroup_StockTransactionType.Add)
            {
                //autocreate stockitem for this product if the code is a valid stock ...
                var stock = StockManager.GetStockByCode(entities, actorCompanyId, ediRowItem.StockCode);
                if (stock != null)
                {
                    var stockDto = new StockDTO
                    {
                        StockId = stock.StockId,
                        AvgPrice = 0
                    };

                    StockManager.SaveStockProducts(entities, transaction, new List<StockDTO> { stockDto }, productId, actorCompanyId);
                    inventoryList = StockManager.GetStocksForInvoiceProduct(entities, actorCompanyId, productId);
                    productStock = inventoryList.FirstOrDefault(x => x.Code == ediRowItem.StockCode);

                    if (invoiceProduct != null)
                    {
                        invoiceProduct.IsStockProduct = true;
                    }
                }
            }

            if (productStock != null)
            {
                stockId = productStock.StockId;
                var quantity = ediRowItem.RowQuantity;
                ProductUnitConvert unitConvert = null;

                //do we need to convert from extern unit quantity to products primary unit
                if (useProductUnitConvert && !string.IsNullOrEmpty(ediRowItem.RowUnitCode))
                {
                    unitConvert = ProductManager.GetProductUnitConvert(entities, productId, ediRowItem.RowUnitCode, actorCompanyId);
                }

                var stockTransactionDTO = new StockTransactionDTO
                {
                    StockTransactionId = 0, //allways new
                    StockProductId = (int)productStock.StockProductId,
                    Quantity = quantity,
                    Price = Math.Round((ediRowItem.RowNetAmount / ediRowItem.RowQuantity), 2), //ediRowItem.RowUnitPrice,
                    ActionType = transType,
                    ReservedQuantity = transType == TermGroup_StockTransactionType.Reserve ? ediRowItem.RowQuantity : 0,
                    Note = note,
                    InvoiceId = invoiceId,
                    ProductUnitConvertId = unitConvert == null ? 0 : unitConvert.ProductUnitConvertId
                };

                return StockManager.SaveStockTransaction(entities, transaction, stockTransactionDTO, actorCompanyId, transaction != null);
                // StockManager.UpdateInvoiceProductStockSaldo(entities, null, (int)productStock.StockProductId, stockItem.RowQuantity, 0, 2, stockItem.RowUnitPrice, false);
            }
            else
            { return new ActionResult { Success = false, ErrorMessage = $"Supplier invoice from EDI failed finding stock place {ediRowItem.StockCode} for item {ediRowItem.RowSellerArticleNumber}", ErrorNumber = (int)ActionResultSave.EdiFailedTransferToInvoiceInvalidData }; }
        }

        private ActionResult UpdateStockFromFinvoiceEdiMessage(FinvoiceEdiRowItem ediRowItem, CompEntities entities, TransactionScope transaction, int actorCompanyId, TermGroup_StockTransactionType transType, int productId, int? invoiceId, string note, bool useProductUnitConvert, out int stockId)
        {
            stockId = 0;
            InvoiceProduct invoiceProduct = null;
            if (productId == 0)
            {
                invoiceProduct = ProductManager.GetInvoiceProductByProductNr(entities, ediRowItem.RowSellerArticleNumber, actorCompanyId);
                if (invoiceProduct != null)
                {
                    productId = invoiceProduct.ProductId;
                }
            }

            if (productId == 0)
            {
                return new ActionResult { ErrorMessage = $"Supplier invoice from EDI faild finding product: {ediRowItem.RowSellerArticleNumber}", Success = false, ErrorNumber = (int)ActionResultSave.EdiFailedTransferToInvoiceInvalidData };
            }

            var inventoryList = StockManager.GetStocksForInvoiceProduct(entities, actorCompanyId, productId);
            var productStock = inventoryList.FirstOrDefault(x => x.Code == ediRowItem.StockCode);

            if (productStock == null && transType == TermGroup_StockTransactionType.Add)
            {
                //autocreate stockitem for this product if the code is a valid stock ...
                var stock = StockManager.GetStockByCode(entities, actorCompanyId, ediRowItem.StockCode);
                if (stock != null)
                {
                    var stockDto = new StockDTO
                    {
                        StockId = stock.StockId,
                        AvgPrice = 0
                    };

                    StockManager.SaveStockProducts(entities, transaction, new List<StockDTO> { stockDto }, productId, actorCompanyId);
                    inventoryList = StockManager.GetStocksForInvoiceProduct(entities, actorCompanyId, productId);
                    productStock = inventoryList.FirstOrDefault(x => x.Code == ediRowItem.StockCode);

                    if (invoiceProduct != null)
                    {
                        invoiceProduct.IsStockProduct = true;
                    }
                }
            }

            if (productStock != null)
            {
                stockId = productStock.StockId;
                var quantity = ediRowItem.RowQuantity;
                ProductUnitConvert unitConvert = null;

                //do we need to convert from extern unit quantity to products primary unit
                if (useProductUnitConvert && !string.IsNullOrEmpty(ediRowItem.RowUnitCode))
                {
                    unitConvert = ProductManager.GetProductUnitConvert(entities, productId, ediRowItem.RowUnitCode, actorCompanyId);
                }

                var stockTransactionDTO = new StockTransactionDTO
                {
                    StockTransactionId = 0, //allways new
                    StockProductId = (int)productStock.StockProductId,
                    Quantity = quantity,
                    Price = Math.Round((ediRowItem.RowNetAmount / ediRowItem.RowQuantity), 2), //ediRowItem.RowUnitPrice,
                    ActionType = transType,
                    ReservedQuantity = transType == TermGroup_StockTransactionType.Reserve ? ediRowItem.RowQuantity : 0,
                    Note = note,
                    InvoiceId = invoiceId,
                    ProductUnitConvertId = unitConvert == null ? 0 : unitConvert.ProductUnitConvertId
                };

                return StockManager.SaveStockTransaction(entities, transaction, stockTransactionDTO, actorCompanyId, transaction != null);
            }
            else
            { return new ActionResult { Success = false, ErrorMessage = $"Supplier invoice from EDI failed finding stock place {ediRowItem.StockCode} for item {ediRowItem.RowSellerArticleNumber}", ErrorNumber = (int)ActionResultSave.EdiFailedTransferToInvoiceInvalidData }; }
        }

        private ActionResult IsEdiEntryInvoiceValidForInvoice(EdiEntry ediEntry)
        {
            if (ediEntry == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "EdiEntry");

            //Validate invoicenr
            if (string.IsNullOrEmpty(ediEntry.InvoiceNr))
                return new ActionResult((int)ActionResultSave.EdiFailedTransferEntityNotFound, GetText(5896, "Fakturanummer saknas"));

            //Validate supplier
            if (!ediEntry.ActorSupplierId.HasValue || ediEntry.ActorSupplierId.Value == 0)
                return new ActionResult((int)ActionResultSave.EdiFailedTransferToInvoiceInvalidData, GetText(5897, "Leverantör saknas"));

            //Validate billing type against amount
            if (ediEntry.Type != (int)TermGroup_EDISourceType.EDI)
            {
                if ((ediEntry.Sum < 0 && ediEntry.BillingType != (int)TermGroup_BillingType.Credit) ||
                    (ediEntry.Sum >= 0 && ediEntry.BillingType == (int)TermGroup_BillingType.Credit))
                    return new ActionResult((int)ActionResultSave.EdiFailedTransferToInvoiceInvalidData, GetText(4743, "Fakturabeloppet avstämmer inte med fakturatypen"));
            }

            //Validate amount
            if (ediEntry.Sum == 0 && ediEntry.Type != (int)TermGroup_EDISourceType.Finvoice)
                return new ActionResult((int)ActionResultSave.EdiFailedTransferToInvoiceInvalidData, GetText(5886, "Belopp får inte vara 0"));

            //Validate status
            bool typeValid = (ediEntry.Type == (int)TermGroup_EDISourceType.Scanning || ediEntry.Type == (int)TermGroup_EDISourceType.Finvoice || (ediEntry.Type == (int)TermGroup_EDISourceType.EDI && ediEntry.MessageType == (int)TermGroup_EdiMessageType.SupplierInvoice));
            bool ediStatusValid = (ediEntry.Status == (int)TermGroup_EDIStatus.Processed || ediEntry.Status == (int)TermGroup_EDIStatus.UnderProcessing);
            bool invoiceStatusValid = (ediEntry.InvoiceStatus == (int)TermGroup_EDIInvoiceStatus.Unprocessed || ediEntry.InvoiceStatus == (int)TermGroup_EDIInvoiceStatus.UnderProcessing);
            if (!typeValid || !ediStatusValid || !invoiceStatusValid)
                return new ActionResult((int)ActionResultSave.EdiFailedTransferToInvoiceInvalidStatus,
                    String.Format("({0}:{1}[{2}], {3}:{4}[{5}])",
                    GetText(5891, "Edi-status"),
                    GetText(ediEntry.Status, (int)TermGroup.EDIStatus), ediEntry.Status.ToString(),
                    GetText(5898, "Edi-fakturastatus"),
                    GetText(ediEntry.InvoiceStatus, (int)TermGroup.EDIInvoiceStatus),
                    ediEntry.InvoiceStatus.ToString()));

            return new ActionResult(true);
        }

        private ActionResult IsEdiEntryInvoiceValidForOrder(EdiEntry ediEntry)
        {
            if (ediEntry == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "EdiEntry");

            //Validate ordernr
            if (String.IsNullOrEmpty(ediEntry.OrderNr))
                return new ActionResult((int)ActionResultSave.EdiFailedTransferEntityNotFound, GetText(5890, "Ordernummer saknas"));

            //Validate status
            bool ediStatusValid = ediEntry.Status == (int)TermGroup_EDIStatus.Processed || ediEntry.Status == (int)TermGroup_EDIStatus.UnderProcessing;
            bool orderStatusValid = ediEntry.OrderStatus == (int)TermGroup_EDIOrderStatus.Unprocessed || ediEntry.OrderStatus == (int)TermGroup_EDIOrderStatus.UnderProcessing;
            if (!ediStatusValid || !orderStatusValid)
                return new ActionResult((int)ActionResultSave.EdiFailedTransferToOrderInvalidStatus,
                    String.Format("({0}:{1}[{2}], {3}:{4}[{5}])",
                    GetText(5891, "Edi-status"),
                    GetText(ediEntry.Status, (int)TermGroup.EDIStatus), ediEntry.Status.ToString(),
                    GetText(5889, "Edi-orderstatus"),
                    GetText(ediEntry.OrderStatus, (int)TermGroup.EDIOrderStatus),
                    ediEntry.OrderStatus.ToString()));

            return new ActionResult(true);
        }

        private bool IsScanningEntryValidForInvoice(ScanningEntry scanningEntry)
        {
            if (scanningEntry == null)
                return false;

            if (!scanningEntry.ScanningEntryRow.IsLoaded)
                scanningEntry.ScanningEntryRow.Load();

            bool valid = scanningEntry.IsAllRowsValid();

            return valid;
        }

        #endregion

        #region Transfer EDI to Order

        public ActionResult TransferToOrdersAndSupplierInvoicesFromEdi(int actorCompanyId)
        {
            ActionResult result = new ActionResult();
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            var orderEdiEntryIds = entitiesReadOnly.EdiEntry.Where(w => w.ActorCompanyId == actorCompanyId && !w.OrderId.HasValue && w.Status == (int)TermGroup_EDIStatus.Unprocessed && w.State == (int)SoeEntityState.Active && w.MessageType == (int)TermGroup_EdiMessageType.OrderAcknowledgement).Select(s => s.EdiEntryId).ToList();
            var supplierInvoiceEdiEntryIds = entitiesReadOnly.EdiEntry.Where(w => w.ActorCompanyId == actorCompanyId && !w.InvoiceId.HasValue && w.Status == (int)TermGroup_EDIStatus.Unprocessed && w.State == (int)SoeEntityState.Active && w.MessageType == (int)TermGroup_EdiMessageType.SupplierInvoice).Select(s => s.EdiEntryId).ToList();

            if (orderEdiEntryIds.Any())
            {
                result = TransferToOrdersFromEdi(orderEdiEntryIds, actorCompanyId, true);
                result.IntegerValue = orderEdiEntryIds.Count;
            }

            if (supplierInvoiceEdiEntryIds.Any())
            {
                result = TransferToSupplierInvoiceFromEdi(orderEdiEntryIds, actorCompanyId, true);
                result.IntegerValue = orderEdiEntryIds.Count;
            }

            return result;
        }

        public ActionResult TransferToOrdersFromEdi(List<int> ediEntryIds, int actorCompanyId, bool checkAutoSettings, int productExternalPriceListHeadId = 0)
        {
            var result = new ActionResult(true);

            #region Auto-transfer settings

            IEnumerable<int[]> transferValuesAdvanced = null;
            bool transferValueSimple = false;
            bool transferCreditInvoices = false;

            if (checkAutoSettings)
            {
                transferValuesAdvanced = GetEdiToOrderAdvancedSetting(actorCompanyId);
                transferValueSimple = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.BillingEdiTransferToOrder, 0, actorCompanyId, 0);
                transferCreditInvoices = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.BillingEdiTransferCreditInvoiceToOrder, 0, actorCompanyId, 0);

                //Validate
                if (!IsAutoEdiToOrderValidForCompare(transferValuesAdvanced, transferValueSimple))
                    return new ActionResult((int)ActionResultSave.EdiFailedTransferToOrderInvalidStatus);
            }

            #endregion

            using (var entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    AttestState attestStateInitial = AttestManager.GetInitialAttestState(entities, actorCompanyId, TermGroup_AttestEntity.Order);

                    foreach (int ediEntryId in ediEntryIds)
                    {
                        #region EdiEntry

                        EdiEntry ediEntry = GetEdiEntry(entities, ediEntryId, actorCompanyId);
                        if (ediEntry == null)
                            continue;

                        using (var transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                        {
                            bool transferToOrder = checkAutoSettings ? this.IsAutoEdiToOrderValid(ediEntry, transferValuesAdvanced, transferValueSimple, transferCreditInvoices) : true;
                            if (transferToOrder)
                            {
                                result = TransferToOrderFromEdiEntry(entities, transaction, ediEntry, attestStateInitial, actorCompanyId, productExternalPriceListHeadId);

                                if (result.Success)
                                {
                                    transaction.Complete();
                                }
                            }

                            #endregion
                        }
                    }
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    result.Exception = ex;
                }
            }

            return result;
        }

        private ActionResult TransferToOrderFromEdiEntry(CompEntities entities, TransactionScope transaction, EdiEntry ediEntry, AttestState attestStateInitial, int actorCompanyId, int productExternalPriceListHeadId)
        {
            if (string.IsNullOrEmpty(ediEntry.OrderNr))
                return new ActionResult((int)ActionResultSave.EdiFailedTransferEntityNotFound);

            var result = new ActionResult(true);
            var rowNr = 1;
            var accountRowNr = 2; // Claim row will get number 1
            int rowsAdded = 0;

            #region Prereq

            if (ediEntry == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, "EdiEntry");

            result = IsEdiEntryInvoiceValidForOrder(ediEntry);
            if (!result.Success)
                return result;

            int standardMaterialCodeId = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.BillingStandardMaterialCode, 0, actorCompanyId, 0);
            int defaultInvoiceProductUnitId = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.BillingDefaultInvoiceProductUnit, 0, actorCompanyId, 0);
            int defaultHouseholdDeductionType = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.BillingDefaultHouseholdDeductionType, 0, actorCompanyId, 0);
            bool useProductUnitConvert = SettingManager.GetBoolSetting(entities, SettingMainType.Company, (int)CompanySettingType.BillingUseProductUnitConvert, 0, actorCompanyId, 0, false);

            #endregion

            CustomerInvoice order = null;
            var processedInputInvoiceRows = new List<CustomerInvoiceRow>();

            if (ediEntry.Type == (int)TermGroup_EDISourceType.EDI)
            {
                #region Symbrio

                SymbrioEdiItem item = SymbrioEdiItem.CreateItem(ediEntry.XML, ediEntry.FileName, ediEntry.Type, true, true);
                if (item != null)
                {
                    #region Order

                    order = InvoiceManager.GetCustomerInvoiceByNr(entities, ediEntry.OrderNr, SoeOriginType.Order, OrderInvoiceRegistrationType.Order, actorCompanyId, true);

                    /* revert since there are performance issues with change to fetch many....
                    var orders = InvoiceManager.GetCustomerInvoicesByNr(entities, ediEntry.OrderNr, SoeOriginType.Order, OrderInvoiceRegistrationType.Order, actorCompanyId, true);
                    if (orders.Count == 1)
                    {
                        order = orders.First();
                    }
                    else if (orders.Count > 1)
                    {
                        return new ActionResult((int)ActionResultSave.EdiFailedTransferEntityNotFound, GetText(7783, "Flera ordrar hittades med samma nummer") + ": " + ediEntry.OrderNr);
                    }
                    */

                    if (order == null)
                        return new ActionResult((int)ActionResultSave.EdiFailedTransferEntityNotFound, GetText(5605, "Ordern hittades inte") + ": " + ediEntry.OrderNr);
                    else if (order.Status == (int)SoeOriginStatus.OrderFullyInvoice)
                        return new ActionResult((int)ActionResultSave.EdiFailedTransferToOrderInvalidStatus, string.Format("{0} ({1}:{2}[{3}])", GetText(5893, "Ordern är helt överförd till faktura"), GetText(5892, "Orderstatus"), GetText(order.Status, (int)TermGroup.OriginStatus), order.Status.ToString()));
                    else if (order.Status == (int)SoeOriginStatus.OrderClosed)
                        return new ActionResult((int)ActionResultSave.EdiFailedTransferToOrderInvalidStatus, string.Format("{0} ({1}:{2}[{3}])", GetText(5894, "Ordern är stängd"), GetText(5892, "Orderstatus"), GetText(order.Status, (int)TermGroup.OriginStatus), order.Status.ToString()));
                    else if (order.Status == (int)SoeOriginStatus.Cancel)
                        return new ActionResult((int)ActionResultSave.EdiFailedTransferToOrderInvalidStatus, string.Format("{0} ({1}:{2}[{3}])", GetText(5895, "Ordern är makulerad"), GetText(5892, "Orderstatus"), GetText(order.Status, (int)TermGroup.OriginStatus), order.Status.ToString()));
                    else if (order.EdiTransferMode == (int)TermGroup_OrderEdiTransferMode.Disable)
                        return new ActionResult((int)ActionResultSave.EdiOrderNotAcceptingEdiTransfer, GetText(9325, "Ordern är avstängd för edi-överföring"));

                    result = InvoiceManager.SetPriceListTypeInclusiveVat(entities, order, actorCompanyId);
                    if (!result.Success)
                        return result;

                    Customer customer = null;

                    order.CustomerInvoiceRow.AddRange(InvoiceManager.GetCustomerInvoiceRowsForOrderInvoiceEdit(entities, order.InvoiceId, true));

                    if (order.ActorId.HasValue)
                        customer = CustomerManager.GetCustomer(entities, (int)order.ActorId, loadActor: true);

                    if (customer == null)
                        return new ActionResult((int)ActionResultSave.EdiFailedTransferEntityNotFound, GetText(8292, "Kund kunde inte hittas"));

                    if (!order.ProjectReference.IsLoaded)
                        order.ProjectReference.Load();

                    if (order.Project != null && standardMaterialCodeId <= 0)
                        return new ActionResult((int)ActionResultSave.TimeCodeMaterialStandardMissing);

                    int orderPriceListTypeId = order.PriceListTypeId ?? 0;

                    #region RowNr

                    if (order.CustomerInvoiceRow != null && order.CustomerInvoiceRow.Any(r => r.State == (int)SoeEntityState.Active))
                    {
                        // Get next product row number
                        rowNr = order.CustomerInvoiceRow.Where(r => r.State == (int)SoeEntityState.Active).Max(i => i.RowNr) + 1;

                        // Get next account row number
                        foreach (CustomerInvoiceRow invoiceRow in order.CustomerInvoiceRow.Where(r => r.State == (int)SoeEntityState.Active))
                        {
                            if (invoiceRow.CustomerInvoiceAccountRow != null && invoiceRow.CustomerInvoiceAccountRow.Any(r => r.State == (int)SoeEntityState.Active))
                            {
                                int maxRowNr = invoiceRow.CustomerInvoiceAccountRow.Where(r => r.State == (int)SoeEntityState.Active).Max(i => i.RowNr) + 1;
                                if (maxRowNr > accountRowNr)
                                    accountRowNr = maxRowNr;
                            }
                        }
                    }

                    #endregion

                    #region Rows

                    InvoiceProduct invoiceProductMisc = null;
                    SysWholesellerDTO sysWholeseller = null;
                    if (ediEntry.SysWholesellerId == 65)
                    {
                        sysWholeseller = WholeSellerManager.GetWholesellerFromComfortReference(item.SellerReference);
                    }
                    if (sysWholeseller == null)
                    {
                        sysWholeseller = WholeSellerManager.GetSysWholesellerDTO(ediEntry.SysWholesellerId);
                    }

                    foreach (var row in item.Rows)
                    {
                        #region CustomerInvoiceRow

                        if (row.ActionCode == EDIMessageRowActionCode.Canceled)
                            continue;

                        bool usesMiscProduct = false;
                        decimal ediPurchaseAmount = CountryCurrencyManager.GetBaseAmountFromCurrencyAmount(row.GetPurchaseAmount(), ediEntry.CurrencyRate);

                        #region InvoiceProduct

                        if (ediEntry.WholesellerName.ToLower().StartsWith("elektroskandia"))
                            row.RowSellerArticleNumber = row.RowSellerArticleNumber.ToUpper().TrimStart('E');

                        //Products should be added from sys to comp in AddEdiEntriesFromFtp(), but if we dont have the ordernr at that point the product will not be added
                        InvoiceProduct invoiceProduct = string.IsNullOrEmpty(row.RowSellerArticleNumber) ? null : ProductManager.GetInvoiceProductByProductNr(entities, row.RowSellerArticleNumber, actorCompanyId, productExternalPriceListHeadId);
                        if (invoiceProduct == null)
                        {
                            if (customer != null)
                            {
                                //Use SupplierAgreement discount when price is calculated
                                int sysProductId = 0;
                                invoiceProduct = string.IsNullOrEmpty(row.RowSellerArticleNumber) ? null : ProductManager.CopyExternalInvoiceProductFromSysByProductNr(entities, sysProductId, row.RowSellerArticleNumber, ediPurchaseAmount, sysWholeseller.SysWholesellerId, row.RowUnitCode, 0, sysWholeseller.Name, actorCompanyId, customer.ActorCustomerId, true);
                            }

                            if (invoiceProduct == null)
                            {
                                //Misc product
                                int productMiscId = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.ProductMisc, 0, actorCompanyId, 0);
                                if (productMiscId > 0)
                                {
                                    if (invoiceProductMisc != null)
                                    {
                                        invoiceProduct = invoiceProductMisc;
                                    }
                                    else
                                    {
                                        invoiceProductMisc = ProductManager.GetInvoiceProduct(entities, productMiscId, true, false, false);
                                        invoiceProduct = invoiceProductMisc;
                                    }

                                    usesMiscProduct = true;
                                }
                            }
                        }

                        if (invoiceProduct == null)
                        {
                            return new ActionResult(GetText(8422, "Artikel kunde inte mappas och ingen ströartikel hittades") + $": {row.RowSellerArticleNumber}");
                        }

                        #endregion

                        #region Price

                        //TODO: Call ApplyPriceRule for misc products
                        InvoiceProductPriceResult productPriceResult = null;
                        if (!usesMiscProduct && customer != null)
                            productPriceResult = ProductManager.GetExternalProductPrice(entities, orderPriceListTypeId, invoiceProduct, customer.ActorCustomerId, actorCompanyId, sysWholeseller.SysWholesellerId, false, checkProduct: true);

                        #endregion

                        #region CustomerInvoiceRow

                        string text = "";
                        if (invoiceProduct != null)
                        {
                            text = (usesMiscProduct) ? row.RowSellerArticleDescription1 + " " + row.RowSellerArticleDescription2 : invoiceProduct.Name;
                        }
                        else
                        {
                            //No product imported, and no misc product exists
                            text = row.RowSellerArticleNumber;
                        }

                        decimal amount = 0;
                        decimal purchasePrice = 0;
                        decimal quantity = GetEdiToOrderQuantity(ediEntry, row);
                        int? productUnitId = null;

                        #region Get productUnit

                        //check if we should do any unit converts for the item.....
                        ProductUnitConvert unitConvert = null;
                        if ((useProductUnitConvert) && invoiceProduct != null && invoiceProduct.ProductUnitId != null && (!string.IsNullOrEmpty(row.RowUnitCode)))
                        {
                            unitConvert = ProductManager.GetProductUnitConvert(entities, invoiceProduct.ProductId, row.RowUnitCode, actorCompanyId);
                            if (unitConvert != null)
                            {
                                ediPurchaseAmount = ediPurchaseAmount == 0 ? 0 : ediPurchaseAmount / unitConvert.ConvertFactor;
                                quantity = quantity * unitConvert.ConvertFactor;
                                productUnitId = invoiceProduct.ProductUnitId;
                            }
                        }

                        //Prio 1
                        if (productUnitId == null)
                        {

                            ProductUnit productUnit = null;

                            //Finnish special since some wholeseller sends edi messages with english unit, so take pricelist unit before
                            if (sysWholeseller != null && sysWholeseller.SysCountryId == (int)TermGroup_Languages.Finnish && productPriceResult != null && !string.IsNullOrEmpty(productPriceResult.ProductUnit))
                            {
                                productUnit = ProductManager.GetProductUnit(entities, productPriceResult.ProductUnit, actorCompanyId);
                            }

                            if (productUnit == null)
                            {
                                productUnit = ProductManager.GetProductUnit(entities, row.RowUnitCode, actorCompanyId);
                            }

                            if (productUnit != null)
                            {
                                productUnitId = productUnit.ProductUnitId;
                            }
                            else if (productUnit == null && invoiceProduct != null)
                            {
                                //Prio 2
                                productUnitId = invoiceProduct.ProductUnitId;
                            }

                            if (productUnitId == null)
                            {
                                //Prio 3
                                productUnitId = defaultInvoiceProductUnitId == 0 ? null : (int?)defaultInvoiceProductUnitId;
                            }
                        }

                        #endregion

                        #region PriceRuleSetting
                        var priceSetFromPriceRule = false;
                        var purchasePriceSetFromPriceRule = false;

                        if (usesMiscProduct && ediPurchaseAmount == 0 && productPriceResult == null)
                        {
                            //misc product and price is 0 so nothing to add markups etc on through priceformulas....
                            amount = ediPurchaseAmount;
                            purchasePrice = ediPurchaseAmount;
                        }
                        else if (EdiPriceSettingRuleIsUsePriceRules(entities, actorCompanyId) && productPriceResult != null)
                        {
                            amount = productPriceResult.SalesPrice;
                            purchasePrice = productPriceResult.PurchasePrice;
                            //purchasePrice = invoiceProduct.PurchasePrice;
                            priceSetFromPriceRule = true;
                            purchasePriceSetFromPriceRule = true;
                        }
                        else if (EdiPriceSettingRuleIsUsePriceRulesKeepEDIPurchasePrice(entities, actorCompanyId))
                        {
                            productPriceResult = ProductManager.GetExternalProductPrice(entities, orderPriceListTypeId, invoiceProduct, customer.ActorCustomerId, actorCompanyId, sysWholeseller.SysWholesellerId, false, wholeSellerName: invoiceProduct.SysWholesellerName, checkProduct: true, ignoreWholesellerDiscount: true, ediPurchasePrice: ediPurchaseAmount, usesMisc: usesMiscProduct);

                            if (productPriceResult != null && productPriceResult.SalesPrice != 0)
                            {
                                priceSetFromPriceRule = true;
                                amount = productPriceResult.SalesPrice;
                            }
                            else
                                amount = ediPurchaseAmount;

                            purchasePrice = ediPurchaseAmount;
                        }
                        else if (EdiPriceSettingRuleIsUsePriceRuleAndPurchasePriceFromSysWholeseller(entities, actorCompanyId))
                        {
                            if (!order.ActorReference.IsLoaded)
                                order.ActorReference.Load();

                            if (usesMiscProduct && productPriceResult == null)
                                productPriceResult = ProductManager.GetExternalProductPrice(entities, orderPriceListTypeId, invoiceProduct, order.Actor.ActorId, actorCompanyId, sysWholeseller.SysWholesellerId, false, checkProduct: true, ignoreWholesellerDiscount: true, ediPurchasePrice: ediPurchaseAmount, usesMisc: usesMiscProduct);

                            if (productPriceResult != null)
                            {
                                amount = productPriceResult.SalesPrice;
                                purchasePrice = productPriceResult.PurchasePrice;
                                priceSetFromPriceRule = true;
                                purchasePriceSetFromPriceRule = true;
                            }
                            else
                            {
                                amount = ediPurchaseAmount;
                                purchasePrice = ediPurchaseAmount;
                            }
                        }
                        else
                        {
                            if (!order.ActorReference.IsLoaded)
                                order.ActorReference.Load();

                            if (usesMiscProduct && productPriceResult == null)
                                productPriceResult = ProductManager.GetExternalProductPrice(entities, orderPriceListTypeId, invoiceProduct, order.Actor.ActorId, actorCompanyId, sysWholeseller.SysWholesellerId, false, checkProduct: true, ignoreWholesellerDiscount: true, ediPurchasePrice: ediPurchaseAmount, usesMisc: usesMiscProduct);

                            if (productPriceResult != null)
                            {
                                amount = productPriceResult.SalesPrice;
                                purchasePrice = productPriceResult.PurchasePrice;
                                priceSetFromPriceRule = true;
                                purchasePriceSetFromPriceRule = true;
                            }
                            else
                            {
                                amount = ediPurchaseAmount;
                                purchasePrice = ediPurchaseAmount;
                            }
                            /*amount = ediPurchaseAmount; //TODO: Call ApplyPriceRule for misc products
                            purchasePrice = ediPurchaseAmount;*/
                        }

                        #endregion

                        //Wholseller has sent a item that is not in official pricelist but customer has added it manually to the product list so try to find the customer price according to normal pricelist handling
                        //PBI: 116075, keep or not to keep?
                        if (!priceSetFromPriceRule && !usesMiscProduct && invoiceProduct != null && invoiceProduct.ExternalProductId.GetValueOrDefault() == 0)
                        {
                            var priceResult = ProductManager.GetProductPrice(entities, actorCompanyId, new ProductPriceRequestDTO { PriceListTypeId = order.PriceListTypeId.GetValueOrDefault(), ProductId = invoiceProduct.ProductId, Quantity = quantity, CustomerId = order.ActorId.GetValueOrDefault(), CurrencyId = order.CurrencyId, WholesellerId = sysWholeseller.SysWholesellerId });
                            if (priceResult.Success)
                            {
                                amount = priceResult.SalesPrice;
                            }
                        }

                        //recalculate prices is set from pricerule and priceRule productUnit is not same as EDI message unit
                        if ((priceSetFromPriceRule || purchasePriceSetFromPriceRule) && useProductUnitConvert && productPriceResult != null && invoiceProduct != null)
                        {
                            var priceResultUnit = ProductManager.GetProductUnit(entities, productPriceResult.ProductUnit, actorCompanyId);
                            if (priceResultUnit != null && priceResultUnit.ProductUnitId != productUnitId)
                            {
                                var priceResultUnitConvert = ProductManager.GetProductUnitConvert(entities, invoiceProduct.ProductId, priceResultUnit.ProductUnitId);
                                if (priceResultUnitConvert != null)
                                {
                                    if (priceSetFromPriceRule && amount != 0)
                                    {
                                        amount = amount / priceResultUnitConvert.ConvertFactor;
                                    }
                                    if (purchasePriceSetFromPriceRule && purchasePrice != 0)
                                    {
                                        purchasePrice = purchasePrice / priceResultUnitConvert.ConvertFactor;
                                    }
                                }
                            }
                        }

                        if (order.FixedPriceOrder)
                            amount = 0;

                        if (order.EdiTransferMode == (int)TermGroup_OrderEdiTransferMode.SetPricesToZero)
                        {
                            amount = 0;
                            purchasePrice = 0;
                        }

                        // Fix for negative * negative returns positive
                        if (amount < 0 && quantity < 0)
                        {
                            amount *= -1;

                            if (purchasePrice < 0)
                            {
                                purchasePrice *= -1;
                            }
                        }

                        // Get VAT account
                        var vatAccountId = (int?)null;
                        var vatRate = 0M;
                        if (invoiceProduct != null && order.VatType == (int)TermGroup_InvoiceVatType.Merchandise)
                        {
                            // TODO, should we really fetch VAT account when order.VatType is VatFree?
                            AccountingPrioDTO dto = AccountManager.GetInvoiceProductAccount(entities, actorCompanyId, invoiceProduct.ProductId, 0, customer.ActorCustomerId, 0, ProductAccountType.VAT, (TermGroup_InvoiceVatType)order.VatType, false);
                            int? accountId = null;
                            if (dto != null)
                                accountId = dto.AccountId;

                            if (!accountId.HasValue)
                            {
                                // Get standard VAT account
                                AccountStd accountStd = AccountManager.GetAccountStdFromInvoiceProductOrCompany(entities, invoiceProduct, ProductAccountType.VAT, CompanySettingType.AccountCommonVatPayable1, actorCompanyId);
                                if (accountStd != null)
                                    accountId = accountStd.AccountId;
                            }

                            if (accountId.HasValue)
                            {
                                // Set VAT account
                                vatAccountId = accountId.Value;

                                // Set VAT rate from account
                                vatRate = AccountManager.GetVatRateValue(entities, accountId.Value);

                                // Set VAT amount
                            }
                        }

                        if (order.PriceListTypeInclusiveVat && vatRate > 0)
                        {
                            amount = amount + decimal.Round(amount * vatRate / 100, 2);
                        }

                        decimal sumAmount = amount * quantity;
                        decimal sumPurchasePrice = purchasePrice * quantity;

                        //decimal sumAmountCurrency = sumAmount * order.CurrencyRate; "*" -- ??
                        decimal sumAmountCurrency = CountryCurrencyManager.GetCurrencyAmountFromBaseAmount(sumAmount, order.CurrencyRate);
                        //decimal sumPurchasePriceCurrency = sumPurchasePrice * order.CurrencyRate; "*" -- ??
                        decimal sumPurchasePriceCurrency = CountryCurrencyManager.GetCurrencyAmountFromBaseAmount(sumPurchasePrice, order.CurrencyRate);

                        decimal discountPercent = customer?.DiscountMerchandise ?? 0;
                        decimal discountAmount = decimal.Round(sumAmount * discountPercent / 100, 2);
                        sumAmount -= discountAmount;
                        sumAmountCurrency -= CountryCurrencyManager.GetCurrencyAmountFromBaseAmount(discountAmount, order.CurrencyRate);

                        decimal discountPercent2 = customer?.Discount2Merchandise ?? 0;
                        decimal discountAmount2 = 0;
                        if (discountPercent2 > 0)
                        {
                            discountAmount2 = decimal.Round(sumAmount * discountPercent2 / 100, 2);
                            sumAmount -= discountAmount2;
                            sumAmountCurrency -= CountryCurrencyManager.GetCurrencyAmountFromBaseAmount(discountAmount2, order.CurrencyRate);
                        }

                        //Create one CustomerInvoiceRow for each product row
                        var orderRow = new CustomerInvoiceRow
                        {
                            RowNr = rowNr,
                            Type = (int)SoeInvoiceRowType.ProductRow,
                            Amount = decimal.Round(amount, 2),
                            AmountCurrency = decimal.Round(CountryCurrencyManager.GetCurrencyAmountFromBaseAmount(amount, order.CurrencyRate), 2),
                            Quantity = quantity,

                            DiscountType = (int)SoeInvoiceRowDiscountType.Percent,
                            DiscountPercent = decimal.Round(discountPercent, 2),
                            DiscountAmount = decimal.Round(discountAmount, 2),
                            DiscountAmountCurrency = decimal.Round(CountryCurrencyManager.GetCurrencyAmountFromBaseAmount(discountAmount, order.CurrencyRate), 2),

                            Discount2Type = (int)SoeInvoiceRowDiscountType.Percent,
                            Discount2Percent = decimal.Round(discountPercent2, 2),
                            Discount2Amount = decimal.Round(discountAmount2, 2),
                            Discount2AmountCurrency = decimal.Round(CountryCurrencyManager.GetCurrencyAmountFromBaseAmount(discountAmount2, order.CurrencyRate), 2),

                            SumAmount = decimal.Round(sumAmount, 2),
                            SumAmountCurrency = decimal.Round(sumAmountCurrency, 2),
                            PurchasePrice = decimal.Round(purchasePrice, 2),
                            PurchasePriceCurrency = decimal.Round(CountryCurrencyManager.GetCurrencyAmountFromBaseAmount(purchasePrice, order.CurrencyRate), 2),
                            SysWholesellerName = sysWholeseller?.Name ?? ediEntry.WholesellerName,
                            Text = text,
                            IsFreightAmountRow = false,
                            IsInvoiceFeeRow = false,
                            IsCentRoundingRow = false,
                            IsInterestRow = false,
                            IsReminderRow = false,
                            IsStockRow = invoiceProduct?.IsStockProduct ?? false,
                            HouseholdDeductionType = invoiceProduct != null && invoiceProduct.HouseholdDeductionType.GetValueOrDefault() > 0 ? invoiceProduct.HouseholdDeductionType.Value : defaultHouseholdDeductionType != 0 ? defaultHouseholdDeductionType : (int?)null,

                            //Set FK
                            EdiEntryId = ediEntry.EdiEntryId,
                            AttestStateId = attestStateInitial?.AttestStateId,

                            VatAccountId = vatAccountId,
                            VatRate = vatRate,

                            //Set references
                            Product = invoiceProduct,
                            ProductId = invoiceProduct?.ProductId,
                            //ProductUnit = productUnit,
                            ProductUnitId = productUnitId,
                            Date = item.HeadInvoiceDate ?? row.RowDeliveryDate ?? item.HeadDeliveryDate,
                            ProductRowType = item.IsInvoice() ? (int)(SoeProductRowType.FromEDI | SoeProductRowType.FromSupplierInvoice) : (int)SoeProductRowType.FromEDI
                        };

                        SetCreatedProperties(orderRow);

                        if (orderRow.VatRate > 0)
                        {
                            orderRow.VatAmount = InvoiceManager.CalculateVatAmountFromVatRate(order.PriceListTypeInclusiveVat, orderRow.SumAmount, orderRow.VatRate);
                            orderRow.VatAmountCurrency = decimal.Round(CountryCurrencyManager.GetCurrencyAmountFromBaseAmount(orderRow.VatAmount, order.CurrencyRate), 2);
                        }

                        InvoiceManager.CalculateMarginIncome(order, orderRow);

                        //handling stock.....
                        if ((orderRow.IsStockRow == true) && !string.IsNullOrEmpty(row.StockCode))
                        {
                            var stockUpdateResult = UpdateStockFromEdiMessage(row, entities, null, actorCompanyId, TermGroup_StockTransactionType.Reserve, (int)orderRow.ProductId, null, "EDI Order confirmation:" + order.InvoiceNr, useProductUnitConvert, out int stockId);
                            if (stockUpdateResult.Success && stockId > 0)
                            {
                                orderRow.StockId = stockId;
                            }
                        }

                        if (order.Project != null)
                        {
                            result = TimeTransactionManager.CreateTimeCodeTransaction(entities, actorCompanyId, orderRow, order.Project, standardMaterialCodeId);
                            if (!result.Success)
                                return result;
                        }

                        processedInputInvoiceRows.Add(orderRow);
                        order.CustomerInvoiceRow.Add(orderRow);
                        rowNr++;
                        rowsAdded++;

                        #endregion

                        #region CustomerInvoiceAccountRow

                        InvoiceManager.CreateCustomerInvoiceAccountRow(entities, order, orderRow, (TermGroup_InvoiceVatType)order.VatType, ref accountRowNr, customer.ActorCustomerId, 0, order.ProjectId ?? 0, actorCompanyId);

                        #endregion

                        #endregion
                    }

                    #endregion

                    #endregion
                }

                #endregion
            }
            else if (ediEntry.Type == (int)TermGroup_EDISourceType.Finvoice)
            {
                #region Finvoice

                FinvoiceEdiItem item = FinvoiceEdiItem.CreateItem(ediEntry.XML, ediEntry.FileName, ediEntry.Type, parameterObject);
                if (item != null)
                {
                    #region Order

                    order = InvoiceManager.GetCustomerInvoiceByNr(entities, ediEntry.OrderNr, SoeOriginType.Order, OrderInvoiceRegistrationType.Order, actorCompanyId, true);

                    if (order == null)
                        return new ActionResult((int)ActionResultSave.EdiFailedTransferEntityNotFound);
                    else if (order.Status == (int)SoeOriginStatus.OrderFullyInvoice)
                        return new ActionResult((int)ActionResultSave.EdiFailedTransferToOrderInvalidStatus, String.Format("{0} ({1}:{2}[{3}])", GetText(5893, "Ordern är helt överförd till faktura"), GetText(5892, "Orderstatus"), GetText(order.Status, (int)TermGroup.OriginStatus), order.Status.ToString()));
                    else if (order.Status == (int)SoeOriginStatus.OrderClosed)
                        return new ActionResult((int)ActionResultSave.EdiFailedTransferToOrderInvalidStatus, String.Format("{0} ({1}:{2}[{3}])", GetText(5894, "Ordern är stängd"), GetText(5892, "Orderstatus"), GetText(order.Status, (int)TermGroup.OriginStatus), order.Status.ToString()));
                    else if (order.Status == (int)SoeOriginStatus.Cancel)
                        return new ActionResult((int)ActionResultSave.EdiFailedTransferToOrderInvalidStatus, String.Format("{0} ({1}:{2}[{3}])", GetText(5895, "Ordern är makulerad"), GetText(5892, "Orderstatus"), GetText(order.Status, (int)TermGroup.OriginStatus), order.Status.ToString()));

                    Customer customer = null;

                    order.CustomerInvoiceRow.AddRange(InvoiceManager.GetCustomerInvoiceRowsForOrderInvoiceEdit(entities, order.InvoiceId, true));

                    if (order.ActorId.HasValue)
                        customer = CustomerManager.GetCustomer(entities, (int)order.ActorId, loadActor: true);

                    if (customer == null)
                        return new ActionResult((int)ActionResultSave.EdiFailedTransferEntityNotFound);

                    if (!order.ProjectReference.IsLoaded)
                        order.ProjectReference.Load();

                    if (order.Project != null && standardMaterialCodeId <= 0)
                        return new ActionResult((int)ActionResultSave.TimeCodeMaterialStandardMissing);

                    int orderPriceListTypeId = order.PriceListTypeId ?? 0;

                    #region RowNr

                    if (order.CustomerInvoiceRow != null && order.CustomerInvoiceRow.Any(r => r.State == (int)SoeEntityState.Active))
                    {
                        // Get next product row number
                        rowNr = order.CustomerInvoiceRow.Where(r => r.State == (int)SoeEntityState.Active).Max(i => i.RowNr) + 1;

                        // Get next account row number
                        foreach (CustomerInvoiceRow invoiceRow in order.CustomerInvoiceRow.Where(r => r.State == (int)SoeEntityState.Active))
                        {
                            if (invoiceRow.CustomerInvoiceAccountRow != null && invoiceRow.CustomerInvoiceAccountRow.Any(r => r.State == (int)SoeEntityState.Active))
                            {
                                int maxRowNr = invoiceRow.CustomerInvoiceAccountRow.Where(r => r.State == (int)SoeEntityState.Active).Max(i => i.RowNr) + 1;
                                if (maxRowNr > accountRowNr)
                                    accountRowNr = maxRowNr;
                            }
                        }
                    }

                    #endregion

                    #region Rows

                    InvoiceProduct invoiceProductMisc = null;

                    int sysCurrencyIdAmount = CountryCurrencyManager.GetSysCurrencyId(entities, ediEntry.CurrencyId);
                    int sysCurrencyIdBase = CountryCurrencyManager.GetCompanyBaseSysCurrencyId(entities, actorCompanyId);

                    foreach (var row in item.Rows)
                    {
                        #region CustomerInvoiceRow

                        bool usesMiscProduct = false;
                        decimal ediPurchaseAmount = CountryCurrencyManager.GetBaseAmountFromCurrencyAmount(row.GetPurchaseAmount(), ediEntry.CurrencyRate);

                        #region InvoiceProduct                           

                        //Products should be added from sys to comp in AddEdiEntriesFromFtp(), but if we dont have the ordernr at that point the product will not be added
                        InvoiceProduct invoiceProduct = string.IsNullOrEmpty(row.RowSellerArticleNumber) ? null : ProductManager.GetInvoiceProductByProductNr(entities, row.RowSellerArticleNumber, actorCompanyId);
                        if (invoiceProduct == null)
                        {
                            if (customer != null)
                            {
                                //Use SupplierAgreement discount when price is calculated
                                int sysProductId = 0;
                                invoiceProduct = string.IsNullOrEmpty(row.RowSellerArticleNumber) ? null : ProductManager.CopyExternalInvoiceProductFromSysByProductNr(entities, sysProductId, row.RowSellerArticleNumber, ediPurchaseAmount, ediEntry.SysWholesellerId, row.RowUnitCode, 0, ediEntry.WholesellerName, actorCompanyId, customer.ActorCustomerId, true);
                            }

                            if (invoiceProduct == null)
                            {
                                //Misc product
                                int productMiscId = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.ProductMisc, 0, actorCompanyId, 0);
                                if (productMiscId > 0)
                                {
                                    if (invoiceProductMisc != null)
                                    {
                                        invoiceProduct = invoiceProductMisc;
                                    }
                                    else
                                    {
                                        invoiceProductMisc = ProductManager.GetInvoiceProduct(entities, productMiscId, true, false, false);
                                        invoiceProduct = invoiceProductMisc;
                                    }

                                    usesMiscProduct = true;
                                }
                            }
                        }

                        #endregion

                        #region Price

                        //TODO: Call ApplyPriceRule for misc products
                        InvoiceProductPriceResult productPriceResult = null;
                        if (invoiceProduct != null && !usesMiscProduct && customer != null)
                            productPriceResult = ProductManager.GetExternalProductPrice(entities, orderPriceListTypeId, invoiceProduct, customer.ActorCustomerId, actorCompanyId, 0, false, checkProduct: true);

                        #endregion

                        #region CustomerInvoiceRow

                        string text = "";
                        if (invoiceProduct != null)
                        {
                            text = (usesMiscProduct) ? row.RowSellerArticleDescription1 + " " + row.RowSellerArticleDescription2 : invoiceProduct.Name;
                        }
                        else
                        {
                            //No product imported, and no misc product exists
                            text = row.RowSellerArticleNumber;
                        }

                        decimal amount = 0;
                        decimal purchasePrice = 0;
                        decimal quantity = ediEntry.BillingType.HasValue && ediEntry.BillingType.Value == (int)TermGroup_BillingType.Credit && row.RowQuantity > 0 ? Decimal.Negate(row.RowQuantity) : row.RowQuantity;
                        int? productUnitId = null;

                        #region Get productUnit

                        //check if we should do any unit converts for the item.....
                        ProductUnitConvert unitConvert = null;
                        if ((useProductUnitConvert) && invoiceProduct != null && invoiceProduct.ProductUnitId != null && (!string.IsNullOrEmpty(row.RowUnitCode)))
                        {
                            unitConvert = ProductManager.GetProductUnitConvert(entities, invoiceProduct.ProductId, row.RowUnitCode, actorCompanyId);
                            if (unitConvert != null)
                            {
                                ediPurchaseAmount = ediPurchaseAmount == 0 ? 0 : ediPurchaseAmount / unitConvert.ConvertFactor;
                                quantity = quantity * unitConvert.ConvertFactor;
                                productUnitId = invoiceProduct.ProductUnitId;
                            }
                        }

                        //Prio 1
                        if (productUnitId == null)
                        {
                            var productUnit = ProductManager.GetProductUnit(entities, row.RowUnitCode, actorCompanyId);

                            if (productUnit != null)
                            {
                                productUnitId = productUnit.ProductUnitId;
                            }
                            else if (productUnit == null && invoiceProduct != null)
                            {
                                //Prio 2
                                productUnitId = invoiceProduct.ProductUnitId;
                            }

                            if (productUnitId == null)
                            {
                                //Prio 3
                                productUnitId = defaultInvoiceProductUnitId == 0 ? null : (int?)defaultInvoiceProductUnitId;
                            }
                        }

                        #endregion

                        #region PriceRuleSetting
                        var priceSetFromPriceRule = false;
                        var purchasePriceSetFromPriceRule = false;

                        if (EdiPriceSettingRuleIsUsePriceRules(entities, actorCompanyId) && productPriceResult != null)
                        {
                            amount = productPriceResult.SalesPrice;
                            purchasePrice = productPriceResult.PurchasePrice;
                            priceSetFromPriceRule = true;
                            purchasePriceSetFromPriceRule = true;
                        }
                        else if (EdiPriceSettingRuleIsUsePriceRulesKeepEDIPurchasePrice(entities, actorCompanyId))
                        {
                            productPriceResult = ProductManager.GetExternalProductPrice(entities, orderPriceListTypeId, invoiceProduct, customer.ActorCustomerId, actorCompanyId, ediEntry.SysWholesellerId, false, wholeSellerName: invoiceProduct.SysWholesellerName, checkProduct: true, ignoreWholesellerDiscount: true, ediPurchasePrice: ediPurchaseAmount, usesMisc: usesMiscProduct);

                            if (productPriceResult != null && productPriceResult.SalesPrice != 0)
                            {
                                priceSetFromPriceRule = true;
                                amount = productPriceResult.SalesPrice;
                            }
                            else
                                amount = ediPurchaseAmount;

                            purchasePrice = ediPurchaseAmount;
                        }
                        else if (EdiPriceSettingRuleIsUsePriceRuleAndPurchasePriceFromSysWholeseller(entities, actorCompanyId))
                        {
                            if (!order.ActorReference.IsLoaded)
                                order.ActorReference.Load();

                            if (usesMiscProduct && productPriceResult == null)
                                productPriceResult = ProductManager.GetExternalProductPrice(entities, orderPriceListTypeId, invoiceProduct, order.Actor.ActorId, actorCompanyId, ediEntry.SysWholesellerId, false, checkProduct: true, ignoreWholesellerDiscount: true, ediPurchasePrice: ediPurchaseAmount, usesMisc: usesMiscProduct);

                            if (productPriceResult != null)
                            {
                                amount = productPriceResult.SalesPrice;
                                purchasePrice = productPriceResult.PurchasePrice;
                                priceSetFromPriceRule = true;
                                purchasePriceSetFromPriceRule = true;
                            }
                            else
                            {
                                amount = ediPurchaseAmount;
                                purchasePrice = ediPurchaseAmount;
                            }
                        }
                        else
                        {
                            if (!order.ActorReference.IsLoaded)
                                order.ActorReference.Load();

                            if (usesMiscProduct && productPriceResult == null)
                                productPriceResult = ProductManager.GetExternalProductPrice(entities, orderPriceListTypeId, invoiceProduct, order.Actor.ActorId, actorCompanyId, ediEntry.SysWholesellerId, false, checkProduct: true, ignoreWholesellerDiscount: true, ediPurchasePrice: ediPurchaseAmount, usesMisc: usesMiscProduct);

                            if (productPriceResult != null)
                            {
                                amount = productPriceResult.SalesPrice;
                                purchasePrice = productPriceResult.PurchasePrice;
                                priceSetFromPriceRule = true;
                                purchasePriceSetFromPriceRule = true;
                            }
                            else
                            {
                                amount = ediPurchaseAmount;
                                purchasePrice = ediPurchaseAmount;
                            }
                        }

                        #endregion

                        //recalculate prices is set from pricerule and priceRule productUnit is not same as EDI message unit
                        if ((priceSetFromPriceRule || purchasePriceSetFromPriceRule) && useProductUnitConvert && productPriceResult != null && invoiceProduct != null)
                        {
                            var priceResultUnit = ProductManager.GetProductUnit(entities, productPriceResult.ProductUnit, actorCompanyId);
                            if (priceResultUnit != null && priceResultUnit.ProductUnitId != productUnitId)
                            {
                                var priceResultUnitConvert = ProductManager.GetProductUnitConvert(entities, invoiceProduct.ProductId, priceResultUnit.ProductUnitId);
                                if (priceResultUnitConvert != null)
                                {
                                    if (priceSetFromPriceRule && amount != 0)
                                    {
                                        amount = amount / priceResultUnitConvert.ConvertFactor;
                                    }
                                    if (purchasePriceSetFromPriceRule && purchasePrice != 0)
                                    {
                                        purchasePrice = purchasePrice / priceResultUnitConvert.ConvertFactor;
                                    }
                                }
                            }
                        }

                        if (order.FixedPriceOrder)
                            amount = 0;

                        decimal sumAmount = amount * quantity;
                        decimal sumPurchasePrice = purchasePrice * quantity;

                        decimal sumAmountCurrency = CountryCurrencyManager.GetCurrencyAmountFromBaseAmount(sumAmount, order.CurrencyRate);
                        decimal sumPurchasePriceCurrency = CountryCurrencyManager.GetCurrencyAmountFromBaseAmount(sumPurchasePrice, order.CurrencyRate);

                        decimal discountPercent = customer != null ? customer.DiscountMerchandise : 0;
                        decimal discountAmount = sumAmount * discountPercent / 100;

                        sumAmount -= discountAmount;
                        sumAmountCurrency -= CountryCurrencyManager.GetCurrencyAmountFromBaseAmount(discountAmount, order.CurrencyRate);

                        decimal salesAmountCurrency = sumAmountCurrency;

                        decimal marginalIncome = salesAmountCurrency - (sumPurchasePriceCurrency);
                        decimal marginalIncomeRatio = (salesAmountCurrency != 0 ? marginalIncome / salesAmountCurrency : 1) * 100;
                        if (marginalIncome < 0 && marginalIncomeRatio > 0)
                            marginalIncomeRatio *= -1;

                        // Fix for negative * negative returns positive
                        if (amount < 0 && quantity < 0)
                            sumAmount = sumAmount * -1;

                        //Create one CustomerInvoiceRow for each product row
                        var orderRow = new CustomerInvoiceRow()
                        {
                            RowNr = rowNr,
                            Type = (int)SoeInvoiceRowType.ProductRow,
                            Amount = Decimal.Round(amount, 2),
                            AmountCurrency = Decimal.Round(CountryCurrencyManager.GetCurrencyAmountFromBaseAmount(amount, order.CurrencyRate), 2),
                            Quantity = quantity,
                            DiscountType = (int)SoeInvoiceRowDiscountType.Percent,
                            DiscountPercent = Decimal.Round(discountPercent, 2),
                            DiscountAmount = Decimal.Round(discountAmount, 2),
                            DiscountAmountCurrency = Decimal.Round(CountryCurrencyManager.GetCurrencyAmountFromBaseAmount(discountAmount, order.CurrencyRate), 2),
                            SumAmount = Decimal.Round(sumAmount, 2),
                            SumAmountCurrency = Decimal.Round(sumAmountCurrency, 2),
                            PurchasePrice = Decimal.Round(purchasePrice, 2),
                            PurchasePriceCurrency = Decimal.Round(CountryCurrencyManager.GetCurrencyAmountFromBaseAmount(purchasePrice, order.CurrencyRate), 2),
                            //SysWholesellerName = ediEntry.WholesellerName,
                            MarginalIncome = Decimal.Round(marginalIncome, 2),
                            MarginalIncomeCurrency = Decimal.Round(CountryCurrencyManager.GetCurrencyAmountFromBaseAmount(marginalIncome, order.CurrencyRate), 2),
                            MarginalIncomeRatio = Decimal.Round(marginalIncomeRatio, 2),
                            Text = text,
                            IsFreightAmountRow = false,
                            IsInvoiceFeeRow = false,
                            IsCentRoundingRow = false,
                            IsInterestRow = false,
                            IsReminderRow = false,
                            IsStockRow = invoiceProduct?.IsStockProduct ?? false,
                            HouseholdDeductionType = defaultHouseholdDeductionType.ToNullable(),

                            //Set FK
                            EdiEntryId = ediEntry.EdiEntryId,
                            AttestStateId = attestStateInitial != null ? attestStateInitial.AttestStateId : (int?)null,

                            //Set references
                            Product = invoiceProduct,
                            ProductId = invoiceProduct != null ? invoiceProduct.ProductId : (int?)null,
                            //ProductUnit = productUnit,
                            ProductUnitId = productUnitId,
                            Date = row.RowDeliveryDate,
                        };

                        SetCreatedProperties(orderRow);

                        //handling stock.....
                        if ((orderRow.IsStockRow == true) && !string.IsNullOrEmpty(row.StockCode))
                        {
                            int stockId;
                            var stockUpdateResult = UpdateStockFromFinvoiceEdiMessage(row, entities, null, actorCompanyId, TermGroup_StockTransactionType.Reserve, (int)orderRow.ProductId, null, "EDI Order confirmation:" + order.InvoiceNr, useProductUnitConvert, out stockId);
                            if (stockUpdateResult.Success && stockId > 0)
                            {
                                orderRow.StockId = stockId;
                            }
                        }

                        if (order.Project != null)
                        {
                            result = TimeTransactionManager.CreateTimeCodeTransaction(entities, actorCompanyId, orderRow, order.Project, standardMaterialCodeId);
                            if (!result.Success)
                                return result;
                        }

                        // Get VAT account
                        if (invoiceProduct != null && order.VatType == (int)TermGroup_InvoiceVatType.Merchandise)
                        {
                            // TODO, should we really fetch VAT account when order.VatType is VatFree?
                            AccountingPrioDTO dto = AccountManager.GetInvoiceProductAccount(entities, actorCompanyId, invoiceProduct.ProductId, 0, customer.ActorCustomerId, 0, ProductAccountType.VAT, (TermGroup_InvoiceVatType)order.VatType, false);
                            int? accountId = null;
                            if (dto != null)
                                accountId = dto.AccountId;

                            if (!accountId.HasValue)
                            {
                                // Get standard VAT account
                                AccountStd accountStd = AccountManager.GetAccountStdFromInvoiceProductOrCompany(entities, invoiceProduct, ProductAccountType.VAT, CompanySettingType.AccountCommonVatPayable1, actorCompanyId);
                                if (accountStd != null)
                                    accountId = accountStd.AccountId;
                            }

                            if (accountId.HasValue)
                            {
                                // Set VAT account
                                orderRow.VatAccountId = accountId.Value;

                                // Set VAT rate from account
                                orderRow.VatRate = AccountManager.GetVatRateValue(entities, accountId.Value);

                                // Set VAT amount
                                orderRow.VatAmount = Decimal.Round(sumAmount * (orderRow.VatRate / 100), 2);
                                orderRow.VatAmountCurrency = Decimal.Round(CountryCurrencyManager.GetCurrencyAmountFromBaseAmount(orderRow.VatAmount, order.CurrencyRate), 2);
                            }
                        }

                        processedInputInvoiceRows.Add(orderRow);
                        order.CustomerInvoiceRow.Add(orderRow);
                        rowNr++;
                        rowsAdded++;

                        #endregion

                        #region CustomerInvoiceAccountRow

                        InvoiceManager.CreateCustomerInvoiceAccountRow(entities, order, orderRow, (TermGroup_InvoiceVatType)order.VatType, ref accountRowNr, customer.ActorCustomerId, 0, order.ProjectId ?? 0, actorCompanyId);

                        #endregion

                        #endregion
                    }

                    #endregion

                    #endregion
                }


                #endregion
            }

            #region Update EdiEntry

            if (order != null)
            {
                ediEntry.OrderId = order.InvoiceId;
                ediEntry.OrderStatus = (int)TermGroup_EDIOrderStatus.Processed;
            }

            if (ediEntry.Type == (int)TermGroup_EDISourceType.EDI)
            {
                if (CloseEdiConditionIsOrder(entities, actorCompanyId))
                    ediEntry.State = (int)SoeEntityState.Inactive;
                if (CloseEdiConditionIsOrderAndSupplierInvoice(entities, actorCompanyId) && ediEntry.InvoiceStatus == (int)TermGroup_EDIInvoiceStatus.Processed && ediEntry.InvoiceId.HasValue && ediEntry.OrderId.HasValue)
                    ediEntry.State = (int)SoeEntityState.Inactive;
                else if (CloseEdiConditionIsOrderOrSupplierInvoice(entities, actorCompanyId) && (ediEntry.InvoiceId.HasValue || ediEntry.OrderId.HasValue))
                    ediEntry.State = (int)SoeEntityState.Inactive;
            }

            #endregion

            if (order != null && rowsAdded > 0)
            {
                if (order.Status == (int)SoeOriginStatus.OrderFullyInvoice)
                {
                    order.Status = (int)SoeOriginStatus.OrderPartlyInvoice;
                }

                result = InvoiceManager.UpdateInvoiceAfterRowModification(entities, order, accountRowNr, actorCompanyId, false, false);
                if (!result.Success)
                    return result;
            }

            InvoiceManager.ConvertProductRowsTextUppercase(entities, processedInputInvoiceRows, base.ActorCompanyId);

            return SaveChanges(entities, transaction);
        }

        private static decimal GetEdiToOrderQuantity(EdiEntry ediEntry, SymbrioEdiRowItem row)
        {
            if (ediEntry.BillingType.HasValue && ediEntry.BillingType.Value == (int)TermGroup_BillingType.Credit)
            {
                //Dahl: Standard creditrows is positive but there can be negative rows that are fees for returning goods that then should show up as a cost on the order
                //for other wholeseller standard credit row are usually positive but some uses negative so make all negative
                switch (ediEntry.SysWholesellerId)
                {
                    case (int)SoeWholeseller.Dahl:
                        return row.RowQuantity * -1;
                    default:
                        return row.RowQuantity > 0 ? decimal.Negate(row.RowQuantity) : row.RowQuantity;
                }
            }

            return row.RowQuantity;
        }

        #region SupplierInvoiceProductRows

        public ActionResult TransferSupplierInvoiceRowsToOrder(CompEntities entities, TransactionScope transaction, int actorCompanyId, SupplierInvoice supplierInvoice, List<SupplierInvoiceProductRow> supplierProductRows, CustomerInvoice order, int sysWholeSellerId = 0, bool createProductIfMissing = false, bool useMiscProduct = false)
        {
            if (order == null)
                return new ActionResult(GetText(8321, "Order kunde inte hittas"));
            if (supplierInvoice == null)
                return new ActionResult("Did not find supplier invoice");
            if (supplierProductRows == null)
                return new ActionResult("No rows");

            if (order.ActorId == null)
                return new ActionResult((int)ActionResultSave.EdiFailedTransferEntityNotFound, GetText(8292, "Kund kunde inte hittas"));

            var isEligibleForTransfer = OrderIsEligibleForEdiTransfer(order);
            if (!isEligibleForTransfer.Success)
                return isEligibleForTransfer;

            //bool useExternalPricing = false;
            SysWholesellerDTO wholeseller = null;
            int sysPriceListHeadId = 0;
            if (sysWholeSellerId != 0)
            {
                //EdiTransfer as if the supplier was a specific wholeseller
                sysPriceListHeadId = WholeSellerManager.GetMostRecentCompanyWholesellerPriceListEx(entities, actorCompanyId, ref sysWholeSellerId);
                wholeseller = WholeSellerManager.GetSysWholesellerDTO(sysWholeSellerId);
                if (!(sysPriceListHeadId > 0 && wholeseller != null))
                    return new ActionResult("Wholeseller is missing");
            }

            var transferRows = new List<TransferExternalProductRowDTO>();
            foreach (var supplierProductRow in supplierProductRows)
            {
                if (supplierProductRow.RowType != (int)SupplierInvoiceRowType.TextRow && (supplierProductRow.Quantity == 0 || supplierProductRow.AmountCurrency == 0))
                    continue;

                if (supplierProductRow.RowType == (int)SupplierInvoiceRowType.TextRow && string.IsNullOrEmpty(supplierProductRow.Text))
                    continue;

                var row = new TransferExternalProductRowDTO()
                {
                    ProductName = supplierProductRow.Text,
                    ProductNumber = supplierProductRow.SellerProductNumber,
                    ProductUnitCode = supplierProductRow.UnitCode,
                    PurchaseAmount = supplierProductRow.AmountCurrency,
                    PurchaseQuantity = supplierProductRow.Quantity,
                    PurchasePrice = supplierProductRow.RowType != (int)SupplierInvoiceRowType.TextRow ? supplierProductRow.AmountCurrency / supplierProductRow.Quantity : 0, //To prevent dicsounts etc. off-setting real purchase price
                    RowType = (SupplierInvoiceRowType)supplierProductRow.RowType,
                    SupplierInvoiceRowReference = supplierProductRow,
                };
                transferRows.Add(row);
            }

            var result = CreateCustomerInvoiceRowsForTransfer(entities, actorCompanyId, order, transferRows, wholeseller, supplierInvoice, sysPriceListHeadId, createProductIfMissing, useMiscProduct);
            int accountRowNr = result.IntegerValue;
            if (result.Success)
            {
                foreach (var transferRow in transferRows)
                {
                    //Set foreign key relationship
                    if (transferRow.CustomerInvoiceRowReference != null)
                    {
                        transferRow.SupplierInvoiceRowReference.CustomerInvoiceRow = transferRow.CustomerInvoiceRowReference;
                    }
                }

                result = InvoiceManager.UpdateInvoiceAfterRowModification(entities, order, accountRowNr, actorCompanyId);
            }

            if (result.Success)
            {
                return SaveChanges(entities, transaction);
            }
            return result;
        }

        public ActionResult CreateCustomerInvoiceRowsForTransfer(CompEntities entities, int actorCompanyId, CustomerInvoice order, List<TransferExternalProductRowDTO> rowsToTransfer, SysWholesellerDTO wholeseller, SupplierInvoice supplierInvoice, int externalPriceListHeadId, bool createProductIfMissing, bool useMiscProduct)
        {

            int orderPriceListTypeId = order.PriceListTypeId ?? 0;
            InvoiceProduct miscProduct = GetMiscInvoiceProduct(entities, actorCompanyId);
            int rowsAdded = 0;

            int defaultHouseholdDeductionType = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.BillingDefaultHouseholdDeductionType, 0, actorCompanyId, 0);
            int defaultInvoiceProductUnitId = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.BillingDefaultInvoiceProductUnit, 0, actorCompanyId, 0);
            bool useProductUnitConvert = SettingManager.GetBoolSetting(entities, SettingMainType.Company, (int)CompanySettingType.BillingUseProductUnitConvert, 0, actorCompanyId, 0, false);
            AttestState attestStateInitial = AttestManager.GetInitialAttestState(entities, actorCompanyId, TermGroup_AttestEntity.Order);

            Customer customer = CustomerManager.GetCustomer(entities, order.ActorId.Value, loadActor: true);

            //a cache for performance and not to create same artikelnr multiple times...
            var productCache = new List<InvoiceProduct>();
            var (rowNr, accountRowNr) = GetNextRowNr(entities, order);
            var processedInputInvoiceRows = new List<CustomerInvoiceRow>();

            foreach (var transferRow in rowsToTransfer)
            {
                if (transferRow.RowType == SupplierInvoiceRowType.ProductRow)
                {
                    var priceRow = new TransferPriceRowDTO();
                    priceRow.PurchaseAmount = CountryCurrencyManager.GetBaseAmountFromCurrencyAmount(transferRow.PurchaseAmount, supplierInvoice.CurrencyRate);
                    priceRow.PurchasePrice = CountryCurrencyManager.GetBaseAmountFromCurrencyAmount(transferRow.PurchasePrice, supplierInvoice.CurrencyRate);
                    priceRow.Quantity = transferRow.PurchaseQuantity;

                    InvoiceProduct invoiceProduct = productCache.FirstOrDefault(x => x.Number == transferRow.ProductNumber);

                    if (!useMiscProduct && invoiceProduct == null)
                    {
                        invoiceProduct = string.IsNullOrEmpty(transferRow.ProductNumber) ? null : ProductManager.GetInvoiceProductByProductNr(entities, transferRow.ProductNumber, actorCompanyId, externalPriceListHeadId);
                        if (invoiceProduct == null && wholeseller != null && !string.IsNullOrEmpty(transferRow.ProductNumber)) //check if want to use external or not?
                        {
                            invoiceProduct = ProductManager.CopyExternalInvoiceProductFromSysByProductNr(entities, 0, transferRow.ProductNumber, priceRow.PurchaseAmount, wholeseller.SysWholesellerId, transferRow.ProductUnitCode, 0, wholeseller.Name, actorCompanyId, customer.ActorCustomerId, true);
                        }

                        if (invoiceProduct == null && !string.IsNullOrEmpty(transferRow.ProductNumber) && createProductIfMissing)
                        {
                            var company = CompanyManager.GetCompany(entities, actorCompanyId);
                            if (company == null)
                                return null;

                            var productUnit = ProductManager.GetProductUnit(entities, transferRow.ProductUnitCode, actorCompanyId);

                            invoiceProduct = new InvoiceProduct
                            {
                                Type = (int)SoeProductType.InvoiceProduct,
                                VatType = (int)TermGroup_InvoiceProductVatType.Merchandise,
                                Name = transferRow.ProductName,
                                Number = transferRow.ProductNumber,
                                CalculationType = (int)TermGroup_InvoiceProductCalculationType.Regular,
                                PurchasePrice = transferRow.PurchasePrice,
                                SysWholesellerName = wholeseller?.Name,
                                AccountingPrio = "1=0,2=0,3=0,4=0,5=0,6=0",
                                ExternalPriceListHeadId = externalPriceListHeadId,
                            };

                            //Set references
                            if (productUnit != null)
                                invoiceProduct.ProductUnit = productUnit;

                            SetCreatedProperties(invoiceProduct);
                            entities.Product.AddObject(invoiceProduct);

                            // Map InvoiceProduct to Company
                            invoiceProduct.Company.Add(company);
                        }

                        if (invoiceProduct != null)
                            productCache.Add(invoiceProduct);
                    }

                    if (invoiceProduct == null)
                    {
                        //Use misc
                        invoiceProduct = miscProduct;
                    }

                    bool usesMiscProduct = miscProduct.ProductId == invoiceProduct.ProductId;

                    InvoiceProductPriceResult productPriceResult = null;
                    if (!usesMiscProduct && customer != null)
                        productPriceResult = ProductManager.GetExternalProductPrice(entities, orderPriceListTypeId, invoiceProduct, customer.ActorCustomerId, actorCompanyId, 0, false, checkProduct: true);

                    //Move
                    string text = (usesMiscProduct) ? transferRow.ProductName : invoiceProduct.Name;

                    //decimal amount = 0;
                    //decimal purchasePrice = 0;
                    //decimal quantity = transferRow.PurchaseQuantity;
                    //decimal quantity = ediEntry.BillingType.HasValue && ediEntry.BillingType.Value == (int)TermGroup_BillingType.Credit && row.RowQuantity > 0 ? decimal.Negate(row.RowQuantity) : row.RowQuantity;

                    //check if we should do any unit converts for the item.....
                    int? productUnitId = null;
                    productUnitId = PerformUnitConversion(entities, actorCompanyId, invoiceProduct, transferRow.ProductUnitCode, priceRow);
                    if (productUnitId == null)
                    {
                        productUnitId = GetProductUnitId(entities, actorCompanyId, wholeseller, invoiceProduct, transferRow, defaultInvoiceProductUnitId, productPriceResult);
                    }

                    bool convertPurchase = false;
                    bool convertSales = false;
                    if (wholeseller != null)
                        SetPrices(entities, actorCompanyId, productPriceResult, order, customer, invoiceProduct, wholeseller, supplierInvoice.CurrencyId, priceRow, usesMiscProduct, ref convertPurchase, ref convertSales);
                    else
                    {
                        priceRow.SalesPrice = priceRow.PurchasePrice;
                    }

                    if (useProductUnitConvert && (convertPurchase || convertSales) && productPriceResult != null && invoiceProduct != null)
                    {
                        PriceRowUnitConversion(entities, actorCompanyId, priceRow, productPriceResult.ProductUnit, productUnitId.GetValueOrDefault(), invoiceProduct.ProductId, convertSales, convertPurchase);
                    }

                    if (order.FixedPriceOrder)
                        priceRow.SalesPrice = 0;

                    if (order.EdiTransferMode == (int)TermGroup_OrderEdiTransferMode.SetPricesToZero)
                    {
                        priceRow.SalesPrice = 0;
                        priceRow.PurchasePrice = 0;
                    }

                    // Fix for negative * negative returns positive
                    if (priceRow.SalesPrice < 0 && priceRow.Quantity < 0)
                    {
                        priceRow.SalesPrice *= -1;

                        if (priceRow.PurchasePrice < 0)
                        {
                            priceRow.PurchasePrice *= -1;
                        }
                    }

                    priceRow.SalesAmount = priceRow.SalesPrice * priceRow.Quantity;
                    priceRow.PurchaseAmount = priceRow.PurchasePrice * priceRow.Quantity;

                    //decimal sumAmountCurrency = sumAmount * order.CurrencyRate; "*" -- ??
                    priceRow.SalesAmountCurrency = CountryCurrencyManager.GetCurrencyAmountFromBaseAmount(priceRow.SalesAmount, order.CurrencyRate);
                    //decimal sumPurchasePriceCurrency = sumPurchasePrice * order.CurrencyRate; "*" -- ??
                    priceRow.PurchaseAmountCurrency = CountryCurrencyManager.GetCurrencyAmountFromBaseAmount(priceRow.PurchaseAmount, order.CurrencyRate);

                    priceRow.DiscountPercent = customer != null ? customer.DiscountMerchandise : 0;
                    priceRow.DiscountAmount = priceRow.SalesAmount * priceRow.DiscountPercent / 100;

                    priceRow.SalesAmount -= priceRow.DiscountAmount;
                    priceRow.SalesAmountCurrency -= CountryCurrencyManager.GetCurrencyAmountFromBaseAmount(priceRow.DiscountAmount, order.CurrencyRate);

                    var orderRow = new CustomerInvoiceRow
                    {
                        RowNr = rowNr,
                        Type = (int)SoeInvoiceRowType.ProductRow,
                        Amount = decimal.Round(priceRow.SalesPrice, 2),
                        AmountCurrency = decimal.Round(CountryCurrencyManager.GetCurrencyAmountFromBaseAmount(priceRow.SalesPrice, order.CurrencyRate), 2),
                        Quantity = priceRow.Quantity,
                        DiscountType = (int)SoeInvoiceRowDiscountType.Percent,
                        DiscountPercent = decimal.Round(priceRow.DiscountPercent, 2),
                        DiscountAmount = decimal.Round(priceRow.DiscountAmount, 2),
                        DiscountAmountCurrency = decimal.Round(CountryCurrencyManager.GetCurrencyAmountFromBaseAmount(priceRow.DiscountAmount, order.CurrencyRate), 2),
                        SumAmount = decimal.Round(priceRow.SalesAmount, 2),
                        SumAmountCurrency = decimal.Round(priceRow.SalesAmountCurrency, 2),
                        PurchasePrice = decimal.Round(priceRow.PurchasePrice, 2),
                        PurchasePriceCurrency = decimal.Round(CountryCurrencyManager.GetCurrencyAmountFromBaseAmount(priceRow.PurchasePrice, order.CurrencyRate), 2),
                        SysWholesellerName = wholeseller?.Name,
                        MarginalIncome = decimal.Round(priceRow.MarginalIncome, 2),
                        MarginalIncomeCurrency = decimal.Round(CountryCurrencyManager.GetCurrencyAmountFromBaseAmount(priceRow.MarginalIncome, order.CurrencyRate), 2),
                        MarginalIncomeRatio = decimal.Round(priceRow.MarginalIncomeRatio, 2),
                        Text = text,
                        IsFreightAmountRow = false,
                        IsInvoiceFeeRow = false,
                        IsCentRoundingRow = false,
                        IsInterestRow = false,
                        IsReminderRow = false,
                        IsStockRow = invoiceProduct != null && invoiceProduct.IsStockProduct != null ? invoiceProduct.IsStockProduct : false,
                        HouseholdDeductionType = invoiceProduct != null && invoiceProduct.HouseholdDeductionType.GetValueOrDefault() > 0 ? invoiceProduct.HouseholdDeductionType.Value : defaultHouseholdDeductionType != 0 ? defaultHouseholdDeductionType : (int?)null,

                        //Set FK
                        //EdiEntryId = ediEntry.EdiEntryId, -> OBS! Set by receiver!
                        AttestStateId = attestStateInitial != null ? attestStateInitial.AttestStateId : (int?)null,

                        //Set references
                        Product = invoiceProduct,
                        ProductId = invoiceProduct != null ? invoiceProduct.ProductId : (int?)null,
                        //ProductUnit = productUnit,
                        ProductUnitId = productUnitId,
                        //SupplierInvoiceId = transferRow.SupplierInvoiceRowReference?.SupplierInvoiceId
                        //Date =  -> OBS! Set by receiver!
                        Date = supplierInvoice.InvoiceDate ?? supplierInvoice.VoucherDate ?? supplierInvoice.Created,
                        ProductRowType = (int)SoeProductRowType.FromSupplierInvoice,
                    };
                    SetCreatedProperties(orderRow);
                    //OBS! Set stock info by receiver!

                    //OBS! Receiver should set time code transaction!

                    // Get VAT account
                    if (invoiceProduct != null && order.VatType == (int)TermGroup_InvoiceVatType.Merchandise)
                    {
                        // TODO, should we really fetch VAT account when order.VatType is VatFree?
                        AccountingPrioDTO dto = AccountManager.GetInvoiceProductAccount(entities, actorCompanyId, invoiceProduct.ProductId, 0, customer.ActorCustomerId, 0, ProductAccountType.VAT, (TermGroup_InvoiceVatType)order.VatType, false);
                        int? accountId = null;
                        if (dto != null)
                            accountId = dto.AccountId;

                        if (!accountId.HasValue)
                        {
                            // Get standard VAT account
                            AccountStd accountStd = AccountManager.GetAccountStdFromInvoiceProductOrCompany(entities, invoiceProduct, ProductAccountType.VAT, CompanySettingType.AccountCommonVatPayable1, actorCompanyId);
                            if (accountStd != null)
                                accountId = accountStd.AccountId;
                        }

                        if (accountId.HasValue)
                        {
                            // Set VAT account
                            orderRow.VatAccountId = accountId.Value;

                            // Set VAT rate from account
                            orderRow.VatRate = AccountManager.GetVatRateValue(entities, accountId.Value);

                            // Set VAT amount
                            orderRow.VatAmount = decimal.Round(priceRow.SalesAmount * (orderRow.VatRate / 100), 2);
                            orderRow.VatAmountCurrency = decimal.Round(CountryCurrencyManager.GetCurrencyAmountFromBaseAmount(orderRow.VatAmount, order.CurrencyRate), 2);
                        }
                    }

                    processedInputInvoiceRows.Add(orderRow);
                    order.CustomerInvoiceRow.Add(orderRow);
                    transferRow.CustomerInvoiceRowReference = orderRow;
                    rowNr++;
                    rowsAdded++;

                    InvoiceManager.CreateCustomerInvoiceAccountRow(entities, order, orderRow, (TermGroup_InvoiceVatType)order.VatType, ref accountRowNr, customer.ActorCustomerId, 0, order.ProjectId ?? 0, actorCompanyId);
                }
                else
                {
                    // Add textrow
                    var orderRow = new CustomerInvoiceRow
                    {
                        RowNr = rowNr,
                        Type = (int)SoeInvoiceRowType.TextRow,
                        DiscountType = (int)SoeInvoiceRowDiscountType.Unknown,
                        Text = transferRow.ProductName,
                        IsFreightAmountRow = false,
                        IsInvoiceFeeRow = false,
                        IsCentRoundingRow = false,
                        IsInterestRow = false,
                        IsReminderRow = false,
                        IsStockRow = false,

                        //Set FK
                        AttestStateId = attestStateInitial != null ? attestStateInitial.AttestStateId : (int?)null,

                        Date = supplierInvoice.InvoiceDate ?? supplierInvoice.VoucherDate ?? supplierInvoice.Created,
                        ProductRowType = (int)SoeProductRowType.FromSupplierInvoice,
                    };
                    SetCreatedProperties(orderRow);

                    processedInputInvoiceRows.Add(orderRow);
                    order.CustomerInvoiceRow.Add(orderRow);
                    transferRow.CustomerInvoiceRowReference = orderRow;
                    rowNr++;
                    rowsAdded++;
                }
            }

            InvoiceManager.ConvertProductRowsTextUppercase(entities, processedInputInvoiceRows, base.ActorCompanyId);

            return new ActionResult(true)
            {
                IntegerValue = accountRowNr
            };
        }

        public InvoiceProduct GetMiscInvoiceProduct(CompEntities entities, int actorCompanyId)
        {
            InvoiceProduct miscProduct = null;
            int productMiscId = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.ProductMisc, 0, actorCompanyId, 0);
            if (productMiscId > 0)
            {
                miscProduct = ProductManager.GetInvoiceProduct(entities, productMiscId, true, false, false);
            }
            return miscProduct;
        }

        public class TransferExternalProductRowDTO
        {
            public string ProductName { get; set; }
            public string ProductNumber { get; set; }
            public string ProductUnitCode { get; set; }
            public decimal PurchaseQuantity { get; set; }
            public decimal PurchaseAmount { get; set; }
            public decimal PurchasePrice { get; set; }
            public SupplierInvoiceRowType RowType { get; set; }
            public CustomerInvoiceRow CustomerInvoiceRowReference { get; set; }
            public SupplierInvoiceProductRow SupplierInvoiceRowReference { get; set; }
        }
        public class TransferPriceRowDTO
        {
            public decimal Quantity { get; set; }

            public decimal PurchasePrice { get; set; }
            public decimal PurchaseAmount { get; set; }
            public decimal PurchaseAmountCurrency { get; set; }
            public decimal SalesPrice { get; set; }
            public decimal SalesAmount { get; set; }
            public decimal SalesAmountCurrency { get; set; }
            public decimal DiscountPercent { get; set; }
            public decimal DiscountAmount { get; set; }
            public decimal MarginalIncome
            {
                get
                {
                    return SalesAmount - PurchaseAmount;
                }
            }
            public decimal MarginalIncomeCurrency
            {
                get
                {
                    return SalesAmountCurrency - PurchaseAmountCurrency;
                }
            }

            public decimal MarginalIncomeRatio
            {
                get
                {
                    var val = (SalesAmountCurrency != 0 ? (MarginalIncomeCurrency / SalesAmountCurrency) : 1) * 100;
                    if (val > 0 && MarginalIncomeCurrency < 0)
                        val *= -1;
                    return val;
                }
            }
        }

        public ActionResult OrderIsEligibleForEdiTransfer(CustomerInvoice order)
        {
            if (order == null)
                return new ActionResult((int)ActionResultSave.EdiFailedTransferEntityNotFound, GetText(5605, "Ordern hittades inte"));
            else if (order.Status == (int)SoeOriginStatus.OrderFullyInvoice)
                return new ActionResult((int)ActionResultSave.EdiFailedTransferToOrderInvalidStatus, string.Format("{0} ({1}:{2}[{3}])", GetText(5893, "Ordern är helt överförd till faktura"), GetText(5892, "Orderstatus"), GetText(order.Status, (int)TermGroup.OriginStatus), order.Status.ToString()));
            else if (order.Status == (int)SoeOriginStatus.OrderClosed)
                return new ActionResult((int)ActionResultSave.EdiFailedTransferToOrderInvalidStatus, string.Format("{0} ({1}:{2}[{3}])", GetText(5894, "Ordern är stängd"), GetText(5892, "Orderstatus"), GetText(order.Status, (int)TermGroup.OriginStatus), order.Status.ToString()));
            else if (order.Status == (int)SoeOriginStatus.Cancel)
                return new ActionResult((int)ActionResultSave.EdiFailedTransferToOrderInvalidStatus, string.Format("{0} ({1}:{2}[{3}])", GetText(5895, "Ordern är makulerad"), GetText(5892, "Orderstatus"), GetText(order.Status, (int)TermGroup.OriginStatus), order.Status.ToString()));
            else if (order.EdiTransferMode == (int)TermGroup_OrderEdiTransferMode.Disable)
                return new ActionResult((int)ActionResultSave.EdiOrderNotAcceptingEdiTransfer, GetText(9325, "Ordern är avstängd för edi-överföring"));
            return new ActionResult();
        }

        private (int, int) GetNextRowNr(CompEntities entities, CustomerInvoice order)
        {
            if (order.CustomerInvoiceRow == null || !order.CustomerInvoiceRow.IsLoaded)
            {
                order.CustomerInvoiceRow.AddRange(InvoiceManager.GetCustomerInvoiceRowsForOrderInvoiceEdit(entities, order.InvoiceId, true));
            }

            int rowNr = 1;
            int accountRowNr = 2;

            if (order.CustomerInvoiceRow != null && order.CustomerInvoiceRow.Any(r => r.State == (int)SoeEntityState.Active))
            {
                // Get next product row number
                rowNr = order.CustomerInvoiceRow.Where(r => r.State == (int)SoeEntityState.Active).Max(i => i.RowNr) + 1;

                // Get next account row number
                foreach (CustomerInvoiceRow invoiceRow in order.CustomerInvoiceRow.Where(r => r.State == (int)SoeEntityState.Active))
                {
                    if (invoiceRow.CustomerInvoiceAccountRow != null && invoiceRow.CustomerInvoiceAccountRow.Any(r => r.State == (int)SoeEntityState.Active))
                    {
                        int maxRowNr = invoiceRow.CustomerInvoiceAccountRow.Where(r => r.State == (int)SoeEntityState.Active).Max(i => i.RowNr) + 1;
                        if (maxRowNr > accountRowNr)
                            accountRowNr = maxRowNr;
                    }
                }
            }
            return (rowNr, accountRowNr);
        }

        private int? PerformUnitConversion(CompEntities entities, int actorCompanyId, InvoiceProduct invoiceProduct, string unitCodeFrom, TransferPriceRowDTO priceRow)
        {
            int? productUnitId = null;
            if (invoiceProduct != null && invoiceProduct.ProductUnitId != null && (!string.IsNullOrEmpty(unitCodeFrom)))
            {
                var unitConvert = ProductManager.GetProductUnitConvert(entities, invoiceProduct.ProductId, unitCodeFrom, actorCompanyId);
                if (unitConvert != null)
                {
                    priceRow.PurchaseAmount = priceRow.PurchaseAmount == 0 ? 0 : priceRow.PurchaseAmount / unitConvert.ConvertFactor;
                    priceRow.Quantity = priceRow.Quantity * unitConvert.ConvertFactor;
                    productUnitId = invoiceProduct.ProductUnitId;
                }
            }
            return productUnitId;
        }

        private int? GetProductUnitId(CompEntities entities, int actorCompanyId, SysWholesellerDTO wholeseller, InvoiceProduct invoiceProduct, TransferExternalProductRowDTO transferRow, int defaultUnitId, InvoiceProductPriceResult productPriceResult)
        {
            ProductUnit productUnit = null;
            int? productUnitId = null;

            //Finnish special since some wholeseller sends edi messages with english unit, so take pricelist unit before
            if (wholeseller != null && wholeseller.SysCountryId == (int)TermGroup_Languages.Finnish && productPriceResult != null && !string.IsNullOrEmpty(productPriceResult.ProductUnit))
            {
                productUnit = ProductManager.GetProductUnit(entities, productPriceResult.ProductUnit, actorCompanyId);
            }

            if (productUnit == null)
            {
                productUnit = ProductManager.GetProductUnit(entities, transferRow.ProductUnitCode, actorCompanyId);
            }

            if (productUnit != null)
            {
                productUnitId = productUnit.ProductUnitId;
            }
            else if (productUnit == null && invoiceProduct != null)
            {
                //Prio 2
                productUnitId = invoiceProduct.ProductUnitId;
            }

            if (productUnitId == null)
            {
                //Prio 3
                productUnitId = defaultUnitId == 0 ? null : (int?)defaultUnitId;
            }
            return productUnitId;
        }
        private void SetPrices(CompEntities entities, int actorCompanyId, InvoiceProductPriceResult productPriceResult, CustomerInvoice order, Customer customer, InvoiceProduct invoiceProduct, SysWholesellerDTO wholeseller, int currencyId, TransferPriceRowDTO priceRow, bool usesMiscProduct, ref bool convertPurchase, ref bool convertSales)
        {
            //PurchaseAmount = PurchasePrice * Discount eller PurchasePrice * Quantity?
            var priceSetFromPriceRule = false;
            var purchasePriceSetFromPriceRule = false;
            int orderPriceListTypeId = order.PriceListTypeId ?? 0;

            InvoiceProductPriceResult getPriceResult(string wholesellerName = null)
            {
                return ProductManager.GetExternalProductPrice(entities, orderPriceListTypeId, invoiceProduct, customer.ActorCustomerId, actorCompanyId, wholeseller.SysWholesellerId, false, wholeSellerName: wholesellerName, checkProduct: true, ignoreWholesellerDiscount: true, ediPurchasePrice: priceRow.PurchasePrice, usesMisc: usesMiscProduct);
            }


            if (EdiPriceSettingRuleIsUsePriceRules(entities, actorCompanyId) && productPriceResult != null)
            {
                priceRow.SalesPrice = productPriceResult.SalesPrice;
                priceRow.PurchasePrice = productPriceResult.PurchasePrice;
                //purchasePrice = invoiceProduct.PurchasePrice;
                priceSetFromPriceRule = true;
                purchasePriceSetFromPriceRule = true;
            }
            else if (EdiPriceSettingRuleIsUsePriceRulesKeepEDIPurchasePrice(entities, actorCompanyId))
            {
                productPriceResult = getPriceResult(invoiceProduct.SysWholesellerName);

                if (productPriceResult != null && productPriceResult.SalesPrice != 0)
                {
                    priceSetFromPriceRule = true;
                    priceRow.SalesPrice = productPriceResult.SalesPrice;
                }
                else
                    priceRow.SalesPrice = priceRow.PurchasePrice;
            }
            else
            {
                if (!order.ActorReference.IsLoaded)
                    order.ActorReference.Load();

                if (usesMiscProduct && productPriceResult == null)
                    productPriceResult = getPriceResult();

                if (productPriceResult != null)
                {
                    priceRow.SalesPrice = productPriceResult.SalesPrice;
                    priceRow.PurchasePrice = productPriceResult.PurchasePrice;
                    priceSetFromPriceRule = true;
                    purchasePriceSetFromPriceRule = true;
                }
                else
                {
                    priceRow.SalesPrice = priceRow.PurchasePrice;
                    //priceRow.PurchasePrice = priceRow.PurchasePrice;
                }
                /*amount = ediPurchaseAmount; //TODO: Call ApplyPriceRule for misc products
                purchasePrice = ediPurchaseAmount;*/
            }

            convertPurchase = purchasePriceSetFromPriceRule;
            convertSales = priceSetFromPriceRule;
            //Reference, to see if we need to convert prices (with regards to differing units)
        }

        private void PriceRowUnitConversion(CompEntities entities, int actorCompanyId, TransferPriceRowDTO priceRow, string unitCodeFrom, int unitIdTo, int productId, bool updateSalesPrice, bool updatePurchasePrice)
        {
            var priceResultUnit = ProductManager.GetProductUnit(entities, unitCodeFrom, actorCompanyId);
            //Check if different units
            if (priceResultUnit != null && priceResultUnit.ProductUnitId != unitIdTo)
            {
                //Perform conversion
                var priceResultUnitConvert = ProductManager.GetProductUnitConvert(entities, productId, priceResultUnit.ProductUnitId);
                if (priceResultUnitConvert != null)
                {
                    if (updateSalesPrice && priceRow.SalesPrice != 0)
                    {
                        priceRow.SalesPrice = priceRow.SalesPrice / priceResultUnitConvert.ConvertFactor;
                    }
                    if (updatePurchasePrice && priceRow.PurchasePrice != 0)
                    {
                        priceRow.PurchasePrice = priceRow.PurchasePrice / priceResultUnitConvert.ConvertFactor;
                    }
                }
            }
        }

        #endregion

        public IEnumerable<int[]> GetEdiToOrderAdvancedSetting(int actorCompanyId)
        {
            string settingValue = SettingManager.GetStringSetting(SettingMainType.Company, (int)CompanySettingType.BillingEdiTransferToOrderAdvanced, 0, actorCompanyId, 0);
            return GetEdiToOrderAdvancedSetting(settingValue);
        }

        public static IEnumerable<int[]> GetEdiToOrderAdvancedSetting(string settingValue)
        {
            //If empty, set none
            if (string.IsNullOrEmpty(settingValue))
                settingValue = Constants.EDI_TRANSFERTOORDER_NONE_STRING;

            var settingValues = settingValue.Split(',').Select(i => i.Split(':'));
            return settingValues.Select(i => new int[]
            {
                int.Parse(i[0]),
                int.Parse(i[1])
            });
        }

        public static string FormatEdiToOrderAdvancedSetting(string settingValue)
        {
            //If contains none, discard all other settings
            if (settingValue.Contains(Constants.EDI_TRANSFERTOORDER_NONE_STRING))
                settingValue = Constants.EDI_TRANSFERTOORDER_NONE_STRING;

            return settingValue;
        }

        public static bool IsAutoEdiToOrderValidForCompare(IEnumerable<int[]> advancedSettings, bool simpleSetting)
        {
            //Advanced
            bool advancedValid = !advancedSettings.IsNullOrEmpty();

            //Simple
            bool simpleValid = simpleSetting;

            return advancedValid || simpleValid;
        }

        public bool IsAutoEdiToOrderValid(EdiEntry ediEntry, IEnumerable<int[]> advancedSettings, bool simpleSetting, bool transferCreditInvoices)
        {
            if (ediEntry == null)
                return false;

            if (!transferCreditInvoices && ediEntry.BillingType == (int)TermGroup_BillingType.Credit)
                return false;

            return IsAutoEdiToOrderValid(ediEntry.SysWholesellerId, ediEntry.MessageType, advancedSettings, simpleSetting);
        }

        public bool IsAutoEdiToOrderValid(int sysWholesellerId, int messageType, IEnumerable<int[]> advancedSettings, bool simpleSetting)
        {
            bool hasAdvancedSettings = !advancedSettings.IsNullOrEmpty();
            bool hasSimpleSettings = !hasAdvancedSettings;

            bool? advancedValid = null;
            bool? simpleValid = null;

            #region Advanced settings

            if (hasAdvancedSettings)
            {
                //First, check if any contains NONE
                foreach (int[] setting in advancedSettings)
                {
                    if (setting.Count() < 2)
                        continue; //Invalid

                    int ruleSysWholeSellerId = setting[0];
                    int ruleMessageType = setting[1];

                    //None
                    if (ruleSysWholeSellerId == Constants.EDI_TRANSFERTOORDER_WHOLESELLERS_NONE && ruleMessageType == Constants.EDI_TRANSFERTOORDER_TYPES_NONE)
                    {
                        advancedValid = false;
                        break;
                    }
                }

                if (!advancedValid.HasValue)
                {
                    //Second, check which wholesellers
                    foreach (int[] item in advancedSettings)
                    {
                        if (item.Count() < 2)
                            continue; //Invalid

                        int ruleSysWholeSellerId = item[0];
                        int ruleMessageType = item[1];
                        if (ruleSysWholeSellerId == Constants.EDI_TRANSFERTOORDER_WHOLESELLERS_NONE || ruleMessageType == Constants.EDI_TRANSFERTOORDER_TYPES_NONE)
                            continue; //Invalid

                        //All - All
                        if (ruleSysWholeSellerId == Constants.EDI_TRANSFERTOORDER_WHOLESELLERS_ALL && ruleMessageType == Constants.EDI_TRANSFERTOORDER_TYPES_ALL)
                        {
                            advancedValid = true;
                            break;
                        }
                        //Given - Given
                        if (CompareSysWholeseller(ruleSysWholeSellerId, sysWholesellerId) && ruleMessageType == messageType)
                        {
                            advancedValid = true;
                            break;
                        }
                        //All WholeSellers - Given MessageType
                        if (ruleSysWholeSellerId == Constants.EDI_TRANSFERTOORDER_WHOLESELLERS_ALL && ruleMessageType == messageType)
                        {
                            advancedValid = true;
                            break;
                        }
                        //All MessageTypes - Given WholeSeller
                        if (CompareSysWholeseller(ruleSysWholeSellerId, sysWholesellerId) && ruleMessageType == Constants.EDI_TRANSFERTOORDER_TYPES_ALL)
                        {
                            advancedValid = true;
                            break;
                        }
                    }
                }
            }

            #endregion

            #region Simple settings

            if (hasSimpleSettings)
            {
                simpleValid = simpleSetting;
            }

            #endregion

            return advancedValid == true || simpleValid == true;
        }

        private bool CompareSysWholeseller(int sysWholesellerId, int sysWholesellerIdToCompare)
        {
            var ahlsells = new List<int> { 2, 14, 15 };
            if (sysWholesellerId == sysWholesellerIdToCompare)
            {
                return true;
            }
            else if (ahlsells.Contains(sysWholesellerId) && ahlsells.Contains(sysWholesellerIdToCompare))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion

        #endregion

        #region ScanningEntry

        public ScanningEntry GetScanningEntry(int scanningEntryId, int actorCompanyId, bool ignoreState = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.ScanningEntry.NoTracking();
            return GetScanningEntry(entities, scanningEntryId, actorCompanyId, ignoreState);
        }

        public ScanningEntry GetScanningEntry(CompEntities entities, int scanningEntryId, int actorCompanyId, bool ignoreState = false)
        {
            return (from se in entities.ScanningEntry
                    where (se.ActorCompanyId == actorCompanyId &&
                    se.ScanningEntryId == scanningEntryId) &&
                    (ignoreState || (se.State == (int)SoeEntityState.Active))
                    select se).FirstOrDefault();
        }

        public ScanningEntry GetScanningEntry(string documentId, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.ScanningEntry.NoTracking();
            return GetScanningEntry(entities, documentId, actorCompanyId);
        }

        public ScanningEntry GetScanningEntry(CompEntities entities, string documentId, int actorCompanyId)
        {
            return (from se in entities.ScanningEntry
                    where se.ActorCompanyId == actorCompanyId &&
                    se.DocumentId == documentId &&
                    se.State == (int)SoeEntityState.Active
                    select se).FirstOrDefault();
        }

        public ScanningEntry GetScanningEntryByDocumentId(CompEntities entities, int actorCompanyId, string documentId)
        {
            return entities.ScanningEntry
                .Where(se => se.ActorCompanyId == actorCompanyId && se.DocumentId == documentId && se.State == (int)SoeEntityState.Active)
                .FirstOrDefault();
        }

        public EdiEntry GetScanningEntry(int ediEntryId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.EdiEntry.NoTracking();
            return GetScanningEntry(entities, ediEntryId);
        }

        public EdiEntry GetScanningEntry(CompEntities entities, int ediEntryId, bool includeInvoiceAttachment = false)
        {
            IQueryable<EdiEntry> query = entities.EdiEntry.Include("ScanningEntryInvoice");

            if (includeInvoiceAttachment)
                query = query.Include("InvoiceAttachment");

            return (from edi in query
                    where edi.EdiEntryId == ediEntryId
                    select edi).FirstOrDefault();
        }

        public decimal GetScanningEntryRoundedInterpretation(List<ScanningEntryRowView> rowItems)
        {
            if (rowItems == null)
                return (int)TermGroup_ScanningInterpretation.ValueNotFound;

            List<decimal> interpretations = GetScanningEntryInterpretation(rowItems);
            int noOfValid = interpretations.Count(i => i == (int)TermGroup_ScanningInterpretation.ValueIsValid);
            int noOfNotFound = interpretations.Count(i => i == (int)TermGroup_ScanningInterpretation.ValueNotFound);

            if (noOfValid == interpretations.Count)
                return (int)TermGroup_ScanningInterpretation.ValueIsValid;
            if (noOfNotFound == interpretations.Count)
                return (int)TermGroup_ScanningInterpretation.ValueNotFound;
            else
                return (int)TermGroup_ScanningInterpretation.ValueIsUnsettled;
        }

        public List<decimal> GetScanningEntryInterpretation(List<ScanningEntryRowView> rowItems)
        {
            List<decimal> interpretations = new List<decimal>();
            if (rowItems == null)
                return interpretations;

            //Used in SupplierInvoiceEdit
            List<int> validTypes = new List<int>()
            {
                (int)ScanningEntryRowType.IsCreditInvoice,
                (int)ScanningEntryRowType.InvoiceNr,
                (int)ScanningEntryRowType.InvoiceDate,
                (int)ScanningEntryRowType.DueDate,
                (int)ScanningEntryRowType.OrderNr,
                (int)ScanningEntryRowType.ReferenceYour,
                (int)ScanningEntryRowType.ReferenceOur,
                (int)ScanningEntryRowType.VatAmount,
                (int)ScanningEntryRowType.TotalAmountIncludeVat,
                (int)ScanningEntryRowType.CurrencyCode,
                //(int)ScanningEntryRowType.OCR,
            };

            //Not used in SupplierInvoiceEdit
            List<int> invalidTypes = new List<int>()
            {
                (int)ScanningEntryRowType.TotalAmountExludeVat,
                (int)ScanningEntryRowType.Plusgiro,
                (int)ScanningEntryRowType.Bankgiro,
                (int)ScanningEntryRowType.OrgNr,
                (int)ScanningEntryRowType.IBAN,
                (int)ScanningEntryRowType.VatRate,
                (int)ScanningEntryRowType.VatNr,
                (int)ScanningEntryRowType.FreightAmount,
                (int)ScanningEntryRowType.CentRounding,
            };

            foreach (var row in rowItems)
            {
                //Only validate the values used in SupplierInvoiceEdit
                if (!validTypes.Contains(row.Type))
                    continue;

                TermGroup_ScanningInterpretation interpretation = TermGroup_ScanningInterpretation.ValueNotFound;
                if (!String.IsNullOrEmpty(row.NewText))
                {
                    //Value is changed by user
                    interpretation = TermGroup_ScanningInterpretation.ValueIsValid;
                }
                else
                {
                    //0 - No errors. The value is correct.
                    //1 - Possible interpretation error. For example, the interpretation seems correct, but it does not match a calculated value.
                    //2 - Error. The field has probably not been interpreted correctly.
                    if (row.ValidationError == "0")
                        interpretation = TermGroup_ScanningInterpretation.ValueIsValid;
                    else if (row.ValidationError == "1")
                        interpretation = TermGroup_ScanningInterpretation.ValueIsUnsettled;
                    else if (row.ValidationError == "2")
                        interpretation = TermGroup_ScanningInterpretation.ValueNotFound;
                }

                interpretations.Add((int)interpretation);
            }

            return interpretations;
        }

        public ActionResult UpdateScanningEntryChanges(CompEntities entities, SupplierInvoiceDTO invoiceInput, ScanningEntry scanningEntry, int actorCompanyId)
        {
            ActionResult result = new ActionResult();

            int nrOfChangedRows = 0;

            if (invoiceInput == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "SupplierInvoice");
            if (scanningEntry == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "ScanningEntry");

            #region Prereq

            if (!scanningEntry.IsAdded())
            {
                if (!scanningEntry.ScanningEntryRow.IsLoaded)
                    scanningEntry.ScanningEntryRow.Load();
            }

            var baseCurrency = CountryCurrencyManager.GetCompanyBaseCurrency(entities, actorCompanyId);
            var currency = CountryCurrencyManager.GetCurrency(entities, invoiceInput.CurrencyId);
            bool calcDueDateFromSupplier = SettingManager.GetBoolSetting(entities, SettingMainType.Company, (int)CompanySettingType.ScanningCalcDueDateFromSupplier, 0, actorCompanyId, 0);

            bool foreign = false;
            string currencyCode = "";
            if (baseCurrency != null && currency != null)
            {
                foreign = baseCurrency.CurrencyId != currency.CurrencyId;
                if (foreign)
                    currencyCode = CountryCurrencyManager.GetCurrencyCode(currency.SysCurrencyId);
                else
                    currencyCode = baseCurrency.Code;
            }

            #endregion

            foreach (ScanningEntryRow row in scanningEntry.ScanningEntryRow.Where(i => i.State == (int)SoeEntityState.Active))
            {
                bool isRowChanged = false;

                #region Validate NewText

                ScanningEntryRowType rowType = (ScanningEntryRowType)row.Type;
                bool isCredit = invoiceInput.BillingType == TermGroup_BillingType.Credit;

                switch (rowType)
                {
                    case ScanningEntryRowType.IsCreditInvoice:
                        #region IsCreditInvoice

                        if (scanningEntry.IsBillingTypeChanged(invoiceInput.BillingType))
                        {
                            row.NewText = (invoiceInput.BillingType == TermGroup_BillingType.Credit).ToString();
                            isRowChanged = true;
                        }

                        #endregion
                        break;
                    case ScanningEntryRowType.InvoiceNr:
                        #region InvoiceNr

                        if (scanningEntry.IsInvoiceNrChanged(invoiceInput.InvoiceNr))
                        {
                            row.NewText = invoiceInput.InvoiceNr;
                            isRowChanged = true;
                        }

                        #endregion
                        break;
                    case ScanningEntryRowType.InvoiceDate:
                        #region InvoiceDate

                        if (scanningEntry.IsInvoiceDateChanged(invoiceInput.InvoiceDate))
                        {
                            row.NewText = CalendarUtility.ToDateTime(invoiceInput.InvoiceDate, ReadSoftAPI.DateFormat);
                            isRowChanged = true;
                        }

                        #endregion
                        break;
                    case ScanningEntryRowType.DueDate:
                        #region DueDate

                        if (scanningEntry.IsDueDateChanged(invoiceInput.DueDate) && !calcDueDateFromSupplier)
                        {
                            row.NewText = CalendarUtility.ToDateTime(invoiceInput.DueDate, ReadSoftAPI.DateFormat);
                            isRowChanged = true;
                        }

                        #endregion
                        break;
                    case ScanningEntryRowType.OrderNr:
                        #region OrderNr

                        //Interpretations from ReadSoft not active
                        if (scanningEntry.IsOrderNrChanged(invoiceInput.OrderNr.ToString()))
                        {
                            //Not a text input field, only update newtext if has input else cannot be sure that it should be empty.
                            row.NewText = String.IsNullOrEmpty(invoiceInput.OrderNr.ToString()) ? null : invoiceInput.OrderNr.ToString();
                            isRowChanged = !String.IsNullOrEmpty(row.NewText);
                        }

                        #endregion
                        break;
                    case ScanningEntryRowType.ReferenceYour:
                        #region ReferenceYour

                        //interpretation from ErReferens / buyercontactpersonname is placed on ReferenceOur
                        if (scanningEntry.IsReferenceYourChanged(invoiceInput.ReferenceOur))
                        {
                            row.NewText = invoiceInput.ReferenceOur;
                            isRowChanged = true;
                        }

                        #endregion
                        break;
                    case ScanningEntryRowType.ReferenceOur:
                        #region ReferenceOur

                        //Interpretations from ReadSoft not active                        

                        #endregion
                        break;
                    case ScanningEntryRowType.TotalAmountExludeVat:
                        #region TotalAmountExludeVat       
                        //TODO: this calculation does not account for rounding

                        decimal totalAmountExcludeVat = invoiceInput.TotalAmount - invoiceInput.VatAmount;

                        if (isCredit)
                            totalAmountExcludeVat = decimal.Negate(Math.Abs(totalAmountExcludeVat));

                        if (foreign == false && scanningEntry.IsTotalAmountExludeVatChanged(totalAmountExcludeVat))
                        {
                            row.NewText = totalAmountExcludeVat.ToString("F");
                            isRowChanged = true;
                        }

                        decimal totalAmountExcludeVatCurrency = invoiceInput.TotalAmountCurrency - invoiceInput.VatAmountCurrency;

                        if (isCredit)
                            totalAmountExcludeVatCurrency = decimal.Negate(Math.Abs(totalAmountExcludeVatCurrency));

                        if (foreign == true && scanningEntry.IsTotalAmountExludeVatChanged(totalAmountExcludeVatCurrency))
                        {
                            row.NewText = totalAmountExcludeVatCurrency.ToString("F");
                            isRowChanged = true;
                        }

                        #endregion
                        break;
                    case ScanningEntryRowType.VatAmount:
                        #region VatAmount

                        decimal vatAmount = invoiceInput.VatAmount;
                        if (isCredit)
                            vatAmount = decimal.Negate(Math.Abs(vatAmount));

                        if (!foreign && scanningEntry.IsVatAmountChanged(vatAmount))
                        {
                            row.NewText = vatAmount.ToString("F");
                            isRowChanged = true;
                        }

                        decimal vatAmountCurrency = invoiceInput.VatAmountCurrency;
                        if (isCredit)
                            vatAmountCurrency = decimal.Negate(Math.Abs(vatAmountCurrency));

                        if (foreign && scanningEntry.IsVatAmountChanged(vatAmountCurrency))
                        {
                            row.NewText = vatAmountCurrency.ToString("F");
                            isRowChanged = true;
                        }

                        #endregion
                        break;
                    case ScanningEntryRowType.TotalAmountIncludeVat:
                        #region TotalAmountIncludeVat

                        decimal totalAmount = invoiceInput.TotalAmount;
                        if (isCredit)
                            totalAmount = decimal.Negate(Math.Abs(totalAmount));

                        if (!foreign && scanningEntry.IsTotalAmountIncludeVatChanged(totalAmount))
                        {
                            row.NewText = totalAmount.ToString("F");
                            isRowChanged = true;
                        }

                        decimal totalAmountCurrency = invoiceInput.TotalAmountCurrency;
                        if (isCredit)
                            totalAmountCurrency = decimal.Negate(Math.Abs(totalAmountCurrency));

                        if (foreign && scanningEntry.IsTotalAmountIncludeVatChanged(totalAmountCurrency))
                        {
                            row.NewText = totalAmountCurrency.ToString("F");
                            isRowChanged = true;
                        }

                        #endregion
                        break;
                    case ScanningEntryRowType.CurrencyCode:
                        #region CurrencyCode

                        if (scanningEntry.IsCurrencyCodeChanged(currencyCode))
                        {
                            row.NewText = currencyCode;
                            isRowChanged = true;
                        }

                        #endregion
                        break;
                    case ScanningEntryRowType.OCR:
                        #region OCR

                        if (scanningEntry.IsOCRNrChanged(invoiceInput.OCR))
                        {
                            row.NewText = invoiceInput.OCR;
                            isRowChanged = true;
                        }

                        #endregion
                        break;
                    case ScanningEntryRowType.Plusgiro:
                        #region Plusgiro

                        //Interpretations from ReadSoft not active

                        #endregion
                        break;
                    case ScanningEntryRowType.Bankgiro:
                        #region Bankgiro

                        //Interpretations from ReadSoft not active

                        #endregion
                        break;
                    case ScanningEntryRowType.OrgNr:
                        #region OrgNr

                        //Interpretations from ReadSoft not active

                        #endregion
                        break;
                    case ScanningEntryRowType.IBAN:
                        #region IBAN

                        //Interpretations from ReadSoft not active

                        #endregion
                        break;
                    case ScanningEntryRowType.VatRate:
                        #region VatRate

                        //Interpretations from ReadSoft not active

                        #endregion
                        break;
                    case ScanningEntryRowType.VatNr:
                        #region VatAmount

                        //Interpretations from ReadSoft not active

                        #endregion
                        break;
                    case ScanningEntryRowType.FreightAmount:
                        #region FreightAmount

                        //Interpretations from ReadSoft not active

                        #endregion
                        break;
                    case ScanningEntryRowType.CentRounding:
                        #region CentRounding

                        //Interpretations from ReadSoft not active

                        #endregion
                        break;
                }

                #endregion

                #region Set NewText

                if (isRowChanged)
                {
                    row.NewText = ReadSoftScanningItem.ValidateRowText(row.NewText, row.Text, rowType);
                    row.ValidationError = ((int)TermGroup_ScanningInterpretation.ValueIsValid).ToString();
                    SetModifiedProperties(row);
                    SetModifiedProperties(scanningEntry);
                    nrOfChangedRows++;
                }

                #endregion
            }

            result.IntegerValue = nrOfChangedRows;

            return result;
        }

        #endregion

        #region ScanningEntryRowView

        public List<ScanningEntryRowView> GetScanningEntryRowItemsByCompany(int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.ScanningEntryRowView.NoTracking();
            return GetScanningEntryRowItemsByCompany(entities, actorCompanyId, null);
        }

        public List<ScanningEntryRowView> GetScanningEntryRowItemsByCompany(CompEntities entities, int actorCompanyId, List<int> scanningEntryIds)
        {
            IQueryable<ScanningEntryRowView> query = (from ser in entities.ScanningEntryRowView
                                                      where ser.ActorCompanyId == actorCompanyId
                                                      select ser);

            if (scanningEntryIds != null && scanningEntryIds.Any())
            {
                query = query.Where(e => scanningEntryIds.Contains(e.ScanningEntryId));
            }

            return query.ToList();
        }

        public List<ScanningEntryRowView> GetScanningEntryRowItems(int scanningEntryId, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.ScanningEntryRowView.NoTracking();
            return GetScanningEntryRowItems(entities, scanningEntryId, actorCompanyId);
        }

        public List<ScanningEntryRowView> GetScanningEntryRowItems(CompEntities entities, int scanningEntryId, int actorCompanyId)
        {
            return (from ser in entities.ScanningEntryRowView
                    where ser.ActorCompanyId == actorCompanyId &&
                    ser.ScanningEntryId == scanningEntryId
                    select ser).ToList();
        }
        #endregion

        #region GetEdiEntrysResult

        public List<EdiEntryViewDTO> GetEDIEntryViewDTOS(IEnumerable<EdiEntryView> items)
        {
            List<EdiEntryViewDTO> dtos = new List<EdiEntryViewDTO>();

            int langId = GetLangId();
            var orderStatuses = base.GetTermGroupDict(TermGroup.EDIOrderStatus, langId);
            var invoiceStatuses = base.GetTermGroupDict(TermGroup.EDIInvoiceStatus, langId);
            var ediSourceTypes = base.GetTermGroupDict(TermGroup.EDISourceType, langId);
            var ediStatuses = base.GetTermGroupDict(TermGroup.EDIStatus, langId);
            var ediMessageTypes = base.GetTermGroupDict(TermGroup.EdiMessageType, langId);
            var billingTypes = base.GetTermGroupDict(TermGroup.InvoiceBillingType, langId);

            foreach (var item in items)
            {
                var dto = item.ToDTO();

                dto.CurrencyCode = CountryCurrencyManager.GetCurrencyCode(dto.SysCurrencyId);

                dto.StatusName = dto.Status != 0 ? ediStatuses[(int)dto.Status] : "";
                dto.InvoiceStatusName = dto.InvoiceStatus != 0 ? invoiceStatuses[(int)dto.InvoiceStatus] : "";
                dto.OrderStatusName = dto.OrderStatus != 0 ? orderStatuses[(int)dto.OrderStatus] : "";
                dto.SourceTypeName = dto.Type != 0 ? ediSourceTypes[(int)dto.Type] : "";
                dto.EdiMessageTypeName = dto.EdiMessageType != 0 ? ediMessageTypes[(int)dto.EdiMessageType] : "";
                dto.BillingTypeName = dto.BillingType != 0 ? billingTypes[(int)dto.BillingType] : "";

                dtos.Add(dto);
            }

            return dtos;
        }
        public List<EdiEntryViewDTO> GetFinvoiceEntryViewDTOS(IEnumerable<FinvoiceEntryView> items)
        {
            List<EdiEntryViewDTO> dtos = new List<EdiEntryViewDTO>();

            int langId = GetLangId();
            var orderStatuses = base.GetTermGroupDict(TermGroup.EDIOrderStatus, langId);
            var invoiceStatuses = base.GetTermGroupDict(TermGroup.EDIInvoiceStatus, langId);
            var ediSourceTypes = base.GetTermGroupDict(TermGroup.EDISourceType, langId);
            var ediStatuses = base.GetTermGroupDict(TermGroup.EDIStatus, langId);
            var ediMessageTypes = base.GetTermGroupDict(TermGroup.EdiMessageType, langId);
            var billingTypes = base.GetTermGroupDict(TermGroup.InvoiceBillingType, langId);

            foreach (var item in items)
            {
                var dto = item.ToDTO();
                dto.CurrencyCode = CountryCurrencyManager.GetCurrencyCode(dto.SysCurrencyId);
                dto.StatusName = dto.Status != 0 ? ediStatuses[(int)dto.Status] : "";
                dto.InvoiceStatusName = dto.InvoiceStatus != 0 ? invoiceStatuses[(int)dto.InvoiceStatus] : "";
                dto.OrderStatusName = dto.OrderStatus != 0 ? orderStatuses[(int)dto.OrderStatus] : "";
                dto.SourceTypeName = dto.Type != 0 ? ediSourceTypes[(int)dto.Type] : "";
                dto.EdiMessageTypeName = dto.EdiMessageType != 0 ? ediMessageTypes[(int)dto.EdiMessageType] : "";
                dto.BillingTypeName = dto.BillingType != 0 ? billingTypes[(int)dto.BillingType] : "";
                dtos.Add(dto);
            }

            return dtos;
        }

        public List<EdiEntryViewDTO> GetEdiEntrysWithStateCheck(int state, int originType)
        {
            int actorCompanyId = base.ActorCompanyId;
            List<EdiEntryViewDTO> result = new List<EdiEntryViewDTO>();
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();

            if (state == (int)SoeEntityState.Inactive)
            {
                var items = (from e in entitiesReadOnly.EdiEntryView
                             where e.ActorCompanyId == actorCompanyId &&
                             e.State == state &&
                             (originType == (int)SoeOriginType.Order ? e.MessageType != (int)TermGroup_EdiMessageType.SupplierInvoice : e.MessageType == (int)TermGroup_EdiMessageType.SupplierInvoice)
                             select e);

                result.AddRange(GetEDIEntryViewDTOS(items));

                items = (from e in entitiesReadOnly.EdiEntryView
                         where e.ActorCompanyId == actorCompanyId &&
                         e.State == (int)SoeEntityState.Deleted &&
                         (originType == (int)SoeOriginType.Order ? e.MessageType != (int)TermGroup_EdiMessageType.SupplierInvoice : e.MessageType == (int)TermGroup_EdiMessageType.SupplierInvoice)
                         select e);

                result.AddRange(GetEDIEntryViewDTOS(items));
            }
            else
            {
                var items = (from e in entitiesReadOnly.EdiEntryView
                             where e.ActorCompanyId == actorCompanyId &&
                             e.State == state &&
                             (originType == (int)SoeOriginType.Order ? e.MessageType != (int)TermGroup_EdiMessageType.SupplierInvoice : e.MessageType == (int)TermGroup_EdiMessageType.SupplierInvoice)
                             select e);

                result.AddRange(GetEDIEntryViewDTOS(items));
            }

            return result;
        }

        public List<EdiEntryViewDTO> GetEdiEntrys(int actorCompanyId, SoeEntityState state)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            var items = (from e in entitiesReadOnly.EdiEntryView
                         where e.ActorCompanyId == actorCompanyId &&
                         e.State == (int)state
                         select e);
            return GetEDIEntryViewDTOS(items);
        }

        public List<EdiEntryViewDTO> GetFilteredEdiEntrys(int actorCompanyId, SoeEntityState state, int originType, List<int> billingTypes, string buyerId, DateTime? dueDate, DateTime? invoiceDate, string orderNr, List<int> orderStatuses, string sellerOrderNr, List<int> ediStatuses, decimal sum, string supplierNrName, TermGroup_ChangeStatusGridAllItemsSelection? allItemsSelection = null)
        {
            DateTime? selectionDate = null;
            if (allItemsSelection.HasValue)
            {
                switch (allItemsSelection)
                {
                    case TermGroup_ChangeStatusGridAllItemsSelection.One_Month:
                        selectionDate = DateTime.Today.AddMonths(-1);
                        break;
                    case TermGroup_ChangeStatusGridAllItemsSelection.Tree_Months:
                        selectionDate = DateTime.Today.AddMonths(-3);
                        break;
                    case TermGroup_ChangeStatusGridAllItemsSelection.Six_Months:
                        selectionDate = DateTime.Today.AddMonths(-6);
                        break;
                    case TermGroup_ChangeStatusGridAllItemsSelection.Twelve_Months:
                        selectionDate = DateTime.Today.AddYears(-1);
                        break;
                    case TermGroup_ChangeStatusGridAllItemsSelection.TwentyFour_Months:
                        selectionDate = DateTime.Today.AddYears(-2);
                        break;
                }
            }
            using var entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            var items = (from e in entitiesReadOnly.EdiEntryView
                         where e.ActorCompanyId == actorCompanyId &&
                         (state == SoeEntityState.Inactive ? (e.State == (int)SoeEntityState.Inactive || e.State == (int)SoeEntityState.Deleted) : e.State == (int)state) &&
                         (!selectionDate.HasValue || e.Created > selectionDate.Value) &&
                         (originType == (int)SoeOriginType.Order ? e.MessageType != (int)TermGroup_EdiMessageType.SupplierInvoice : e.MessageType == (int)TermGroup_EdiMessageType.SupplierInvoice)
                         select e);

            if (billingTypes.Count > 0)
                items = items.Where(e => e.BillingType.HasValue && billingTypes.Contains(e.BillingType.Value));
            if (!string.IsNullOrEmpty(buyerId))
                items = items.Where(e => e.BuyerId.Contains(buyerId));
            if (dueDate.HasValue)
                items = items.Where(e => e.DueDate.HasValue && e.DueDate.Value == dueDate.Value);
            if (invoiceDate.HasValue)
                items = items.Where(e => e.InvoiceDate.HasValue && e.InvoiceDate.Value == invoiceDate.Value);
            if (!string.IsNullOrEmpty(orderNr))
                items = items.Where(e => e.OrderNr.Contains(orderNr));
            if (orderStatuses.Count > 0)
                items = items.Where(e => orderStatuses.Contains(e.OrderStatus));
            if (!string.IsNullOrEmpty(sellerOrderNr))
                items = items.Where(e => e.SellerOrderNr.Contains(sellerOrderNr));
            if (ediStatuses.Count > 0)
                items = items.Where(e => ediStatuses.Contains(e.Status));
            if (sum > 0)
                items = items.Where(e => e.Sum.ToString().Contains(sum.ToString()));
            if (!string.IsNullOrEmpty(supplierNrName))
                items = items.Where(e => (e.SupplierNr + " " + e.SupplierName).Contains(supplierNrName));

            return GetEDIEntryViewDTOS(items);
        }

        public List<EdiEntryViewDTO> GetScanningEntrys(int actorCompanyId, SoeEntityState state)
        {
            List<EdiEntryViewDTO> dtos = new List<EdiEntryViewDTO>();
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();

            var entryItems = (from e in entitiesReadOnly.ScanningEntryView
                              where e.ActorCompanyId == actorCompanyId &&
                              e.State == (int)state &&
                              e.MessageType == (int)TermGroup_EdiMessageType.SupplierInvoice
                              select e);

            if (entryItems.Any())
            {
                int langId = GetLangId();
                var orderStatuses = base.GetTermGroupDict(TermGroup.EDIOrderStatus, langId);
                var invoiceStatuses = base.GetTermGroupDict(TermGroup.EDIInvoiceStatus, langId);
                var ediSourceTypes = base.GetTermGroupDict(TermGroup.EDISourceType, langId);
                var ediStatuses = base.GetTermGroupDict(TermGroup.EDIStatus, langId);
                var ediMessageTypes = base.GetTermGroupDict(TermGroup.EdiMessageType, langId);
                var billingTypes = base.GetTermGroupDict(TermGroup.InvoiceBillingType, langId);

                var rowItems = GetScanningEntryRowItemsByCompany(actorCompanyId);
                foreach (var entryItem in entryItems)
                {
                    var dto = entryItem.ToDTO();
                    dto.CurrencyCode = CountryCurrencyManager.GetCurrencyCode(dto.SysCurrencyId);
                    dto.StatusName = dto.Status != 0 ? ediStatuses[(int)dto.Status] : "";
                    dto.InvoiceStatusName = dto.InvoiceStatus != 0 ? invoiceStatuses[(int)dto.InvoiceStatus] : "";
                    dto.OrderStatusName = dto.OrderStatus != 0 ? orderStatuses[(int)dto.OrderStatus] : "";
                    dto.SourceTypeName = dto.Type != 0 ? ediSourceTypes[(int)dto.Type] : "";
                    dto.EdiMessageTypeName = dto.EdiMessageType != 0 ? ediMessageTypes[(int)dto.EdiMessageType] : "";
                    dto.BillingTypeName = dto.BillingType != 0 ? billingTypes[(int)dto.BillingType] : "";

                    var rowItemsForEntry = rowItems.Where(i => i.ScanningEntryId == entryItem.ScanningEntryId).ToList();
                    dto.RoundedInterpretation = GetScanningEntryRoundedInterpretation(rowItemsForEntry);

                    #region attestgroup

                    int attestGroupId = 0;
                    //try get attest group from supplier invoice
                    if (entryItem.InvoiceId != null)
                    {
                        var supplierInvoice = SupplierInvoiceManager.GetSupplierInvoice((int)entryItem.InvoiceId, false, false, false, false, false, false, false, false);
                        if (supplierInvoice != null && supplierInvoice.AttestGroupId != null)
                            attestGroupId = (int)supplierInvoice.AttestGroupId;
                    }

                    //try get attest group from supplier, if not found from supplier invoice
                    if (attestGroupId == 0 && entryItem.SupplierId != null)
                    {
                        if (entryItem.SupplierId > 0)
                        {
                            var supplier = SupplierManager.GetSupplier((int)entryItem.SupplierId, false, false, false, false);
                            if (supplier.AttestWorkFlowGroupId != null)
                                attestGroupId = (int)supplier.AttestWorkFlowGroupId;
                        }
                    }

                    //try get attest group from defaultsetting, if not found from supplier
                    if (attestGroupId == 0)
                    {
                        int defaultAttestGroupId = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.SupplierInvoiceAttestFlowDefaultAttestGroup, 0, actorCompanyId, 0);
                        if (defaultAttestGroupId > 0)
                            attestGroupId = defaultAttestGroupId;
                    }

                    //set attest group to entryItem
                    if (attestGroupId > 0)
                    {
                        var attestgroup = AttestManager.GetAttestWorkFlowGroup(attestGroupId, actorCompanyId, false);
                        if (attestgroup != null)
                        {
                            dto.SupplierAttestGroupId = attestgroup.AttestWorkFlowHeadId;
                            dto.SupplierAttestGroupName = attestgroup.AttestGroupName;
                        }
                    }

                    #endregion

                    dtos.Add(dto);
                }
            }

            return dtos;
        }

        public List<EdiEntryViewDTO> GetFinvoiceEntrys(int actorCompanyId, SoeEntityState state, int? allItemsSelection, bool onlyUnHandled)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            IQueryable<FinvoiceEntryView> items = (from e in entitiesReadOnly.FinvoiceEntryView
                                                   where e.ActorCompanyId == actorCompanyId &&
                                                   e.State == (int)state
                                                   select e);

            if (onlyUnHandled)
            {
                items = items.Where(i => i.InvoiceStatus == (int)TermGroup_EDIStatus.Error || i.InvoiceStatus == (int)TermGroup_EDIStatus.Unprocessed);
            }

            if (allItemsSelection.HasValue)
            {
                var selectionDate = InvoiceManager.GetSelectionDate((TermGroup_GridDateSelectionType)allItemsSelection.Value);
                var nextDate = selectionDate.Date.AddDays(1);
                if ((TermGroup_GridDateSelectionType)allItemsSelection.Value == TermGroup_GridDateSelectionType.One_Day)
                    items = items.Where(w => (w.Date.HasValue && (w.Date.Value >= selectionDate.Date && w.Date.Value < nextDate)));
                else
                    items = items.Where(w => (w.Date.HasValue && w.Date.Value > selectionDate));
            }
            if (items.Any())
                items = items.OrderByDescending(i => i.EdiEntryId);

            return GetFinvoiceEntryViewDTOS(items);
        }


        public ActionResult UpdateEdiEntrys(List<UpdateEdiEntryDTO> entrys, int actorCompanyId)
        {
            if (entrys == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "GetEdiEntrysResult");

            using (CompEntities entities = new CompEntities())
            {
                foreach (var entry in entrys)
                {
                    if (entry.EdiEntryId > 0)
                    {
                        #region Update EdiEntry

                        EdiEntry originalEdiEntry = GetEdiEntry(entities, entry.EdiEntryId, actorCompanyId, ignoreState: true, loadSupplier: true);
                        if (originalEdiEntry == null)
                            return new ActionResult((int)ActionResultSave.EntityNotFound, "EdiEntry");

                        //Update OrderNr
                        originalEdiEntry.OrderNr = entry.OrderNr;

                        //Update Supplier
                        if (entry.SupplierId.HasValue)
                            originalEdiEntry.Supplier = SupplierManager.GetSupplier(entities, entry.SupplierId.Value);
                        else
                            originalEdiEntry.Supplier = null;

                        //Update SysWholesellerId
                        if (originalEdiEntry.SysWholesellerId == 0)
                            SetEdiEntryWholeseller(originalEdiEntry, actorCompanyId);

                        //Update Syswholeseller from supplier
                        if (originalEdiEntry.Type == (int)TermGroup_EDISourceType.Finvoice && originalEdiEntry.SysWholesellerId == 0 && originalEdiEntry.Supplier.SysWholeSellerId.HasValue)
                        {
                            var sysWholeSeller = WholeSellerManager.GetSysWholesellerDTO((int)originalEdiEntry.Supplier.SysWholeSellerId);
                            if (sysWholeSeller != null)
                            {
                                originalEdiEntry.SysWholesellerId = sysWholeSeller.SysWholesellerId;
                                originalEdiEntry.WholesellerName = sysWholeSeller.Name;
                            }
                        }

                        //Update Error Message
                        if (originalEdiEntry.Supplier != null)
                            originalEdiEntry.ErrorMessage = null;

                        //Validate
                        if (originalEdiEntry.OrderStatus != (int)TermGroup_EDIOrderStatus.Processed)
                            SetEdiEntryOrderStatus(originalEdiEntry, TermGroup_EDIOrderStatus.Unprocessed);

                        if (originalEdiEntry.InvoiceStatus != (int)TermGroup_EDIInvoiceStatus.Processed)
                            SetEdiEntryInvoiceStatus(originalEdiEntry, TermGroup_EDIInvoiceStatus.Unprocessed);

                        SetModifiedProperties(originalEdiEntry);

                        #endregion
                    }

                    if (entry.ScanningEntryId.HasValue && entry.ScanningEntryId > 0)
                    {
                        #region Update ScanningEntry

                        ScanningEntry originalScanningEntry = GetScanningEntry(entities, entry.ScanningEntryId.Value, actorCompanyId);
                        if (originalScanningEntry == null)
                            return new ActionResult((int)ActionResultSave.EntityNotFound, "ScanningEntry");

                        SetModifiedProperties(originalScanningEntry);

                        #endregion
                    }
                }

                return SaveChanges(entities);
            }
        }

        public ActionResult ChangeEdiEntriesState(List<int> entrys, int stateTo, int actorCompanyId)
        {
            ActionResult result = new ActionResult();

            if (entrys == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "GetEdiEntrysResult");

            using (CompEntities entities = new CompEntities())
            {
                if (entrys.Count > 0)
                {
                    foreach (var entry in entrys)
                    {
                        if (entry > 0)
                        {
                            #region Delete EdiEntry

                            EdiEntry originalEdiEntry = GetEdiEntry(entities, entry, actorCompanyId, ignoreState: true);
                            if (originalEdiEntry == null)
                            {
                                result.SuccessNumber = (int)ActionResultSave.EntityNotFound;
                                continue;
                            }

                            result = ChangeEntityState(originalEdiEntry, (SoeEntityState)stateTo);
                            if (!result.Success)
                                result.SuccessNumber = (int)ActionResultSave.EntityNotUpdated;

                            #endregion
                        }
                    }

                    result = SaveChanges(entities);
                }
            }

            return result;
        }
        #endregion

        #region Ftp/WebService

        /// <summary>
        /// Get files from external FTP to internal FTP
        /// </summary>
        /// <param name="externalCompanyEdi">CompanyEdi with Type Nelfo or LVIS</param>
        /// <param name="companyEdiSupport">CompanyWdi with Type Symbrio</param>
        /// <param name="sysScheduledJobId">The scheduled job</param>
        /// <param name="batchNr">The scheduled batch</param>
        /// <returns>ActionResult</returns>
        public ActionResult AddEdiFilesToFtp(CompanyEdiDTO externalCompanyEdi, int sysScheduledJobId = 0, int batchNr = 0, params string[] ignoreFilesList)
        {
            ActionResult result = new ActionResult(true);

            try
            {
                #region Prereq

                //Download from Nelfo FTP
                result = ValidateCompanyEdi(externalCompanyEdi, true, out Uri baseUriDownload);
                if (!result.Success)
                    return result;

                // Use support ftp credentials
                var companyEdiSupport = new CompanyEdiDTO
                {
                    SourceType = CompanyEdiDTO.SourceTypeEnum.FTP,
                };

                if (externalCompanyEdi.Type == (int)TermGroup_CompanyEdiType.Nelfo)
                {
                    //Upload to Nelfo folder
                    companyEdiSupport.Source = Constants.SOE_EDI_FTP_NELFO;
                }
                else if (externalCompanyEdi.Type == (int)TermGroup_CompanyEdiType.LvisNet)
                {
                    companyEdiSupport.Source = Constants.SOE_EDI_FTP_LVISNET;
                }
                else
                {
                    return new ActionResult((int)ActionResultSave.EdiInvalidUri, String.Format("The companyEDIType is not supported: {0}", externalCompanyEdi.Type));
                }

                result = ValidateCompanyEdi(companyEdiSupport, false, out Uri baseUriUpload);
                if (!result.Success)
                    return result;

                //Get filenames from external FTP
                result = GetFilesFromFtp(externalCompanyEdi, baseUriDownload, true, out List<string> fileNames, ignoreFilesList);
                if (!result.Success)
                    return result;

                int noOfFilesProcessed = 0;
                int noOfFiles = fileNames.Count;
                if (noOfFiles == 0)
                    return result;

                #endregion

                #region Process

                foreach (string fn in fileNames)
                {
                    try
                    {
                        string fileName = fn;
                        // We want only the filename, the folder should be specified in CompanyEdi
                        if (fileName.Contains('/'))
                            fileName = fileName.Split('/').Last();

                        Uri downloadUri = new Uri(baseUriDownload.ToString().TrimEnd('/') + "/" + fileName);

                        //Download file
                        byte[] downloadedData = FtpUtility.DownloadData(downloadUri, externalCompanyEdi.Username, externalCompanyEdi.Password);

                        if (DefenderUtil.IsVirus(downloadedData))
                        {
                            LogCollector.LogError($"Virus detected {fn} {Environment.StackTrace}");
                            continue;
                        }

                        if (downloadedData != null && downloadedData.Length > 0)
                        {
                            //Upload file
                            Uri uploadUri = new Uri(baseUriUpload.ToString() + fileName);
                            byte[] uploadedData = FtpUtility.UploadData(uploadUri, downloadedData, companyEdiSupport.Username, companyEdiSupport.Password);

                            if (uploadedData != null)
                            {
                                //Save file to local temp folder
                                string physicalPath = ConfigSettings.SOE_SERVER_DIR_TEMP_IMPORT_EDI_PHYSICAL + fileName;
                                string relativePath = ConfigSettings.SOE_SERVER_DIR_TEMP_IMPORT_EDI_RELATIVE + fileName;
                                result = GeneralManager.SaveFile(physicalPath, relativePath, downloadedData);

                                // Delete File (errors are handled inside this method
                                DeleteFileFromFtp(downloadUri, externalCompanyEdi, sysScheduledJobId, batchNr);
                                noOfFilesProcessed++;
                            }
                        }
                        else
                        {
                            SysScheduledJobManager.CreateLogEntry(sysScheduledJobId, batchNr, ScheduledJobLogLevel.Error, "File found but data size was zero, filename: " + fn);
                        }
                    }
                    catch (Exception ex)
                    {
                        string message = "Unhandled exception in AddEdiFilesToFtp(), fileName: " + fn + ", Exception messages: " + ex.GetInnerExceptionMessages().JoinToString(", Inner:");
                        SysScheduledJobManager.CreateLogEntry(sysScheduledJobId, batchNr, ScheduledJobLogLevel.Error, message);
                    }
                }

                #endregion

                #region Finalize

                //Set information to service job
                result.IntegerValue = noOfFiles;
                result.IntegerValue2 = noOfFilesProcessed;

                #endregion
            }
            catch (Exception ex)
            {
                base.LogError(ex, log);
                result.Exception = ex;
            }

            return result;
        }

        /// <summary>
        /// Get files from FTP and add to EdiEntry
        /// </summary>
        /// <param name="companyEdi">The CompanyEdi</param>
        /// <param name="isAutoFromFtp">True if is scheduled to get from ftp, otherwise false</param>
        /// <param name="sysScheduledJobId">The scheduled job</param>
        /// <param name="batchNr">The scheduled batch</param>
        /// <returns>ActionResult</returns>
        public ActionResult AddEdiEntrysFromSource(CompanyEdiDTO companyEdi, bool isAutoFromFtp, int sysScheduledJobId = 0, int batchNr = 0, int sysWholesellerEdiId = 0)
        {
            ActionResult result = new ActionResult(true)
            {
                Keys = new List<int>()
            };
            bool hasDuplicates = false;

            try
            {
                #region Prereq

                result = ValidateCompanyEdi(companyEdi, false, out Uri baseUri, tryValidateUserName: true);
                if (!result.Success)
                    return result;

                //Get filenames from internal FTP
                List<string> fileNames = new List<string>();
                if (companyEdi.SourceType == CompanyEdiDTO.SourceTypeEnum.File)
                {
                    fileNames.Add(companyEdi.Source);
                }
                else if (companyEdi.SourceType == CompanyEdiDTO.SourceTypeEnum.FTP)
                {
                    result = GetFilesFromFtp(companyEdi, baseUri, false, out fileNames);
                    if (!result.Success)
                        return result;
                }
                else if (companyEdi.SourceType == CompanyEdiDTO.SourceTypeEnum.Xml)
                {
                    fileNames.Add(companyEdi.Source);
                }

                int noOfFilesProcessed = 0;
                int noOfFiles = fileNames.Count;
                if (noOfFiles == 0)
                    return result;

                List<int> ediEntryIds = new List<int>();

                #endregion

                #region Process

                using (CompEntities entities = new CompEntities())
                {
                    foreach (string source in fileNames)
                    {
                        #region Process file

                        EdiEntry ediEntry = null;
                        SymbrioEdiItem item = null;
                        Uri uri = null;
                        string xml = null;
                        string fileName;
                        if (companyEdi.SourceType == CompanyEdiDTO.SourceTypeEnum.Xml)
                        {
                            xml = source;
                            fileName = companyEdi.FileName;
                        }
                        else
                        {
                            fileName = source;
                        }

                        if (companyEdi.SourceType == CompanyEdiDTO.SourceTypeEnum.FTP)
                            uri = new Uri(baseUri.ToString() + fileName);

                        bool createEdiEntry = true;

                        try
                        {
                            #region Prereq

                            //Duplicates check 1: Prevent duplicates when for (1) files that couldnt be deleted (2) files with status Unprocessed or Deleted
                            ediEntry = GetEdiEntryByFileName(entities, fileName, companyEdi.ActorCompanyId);
                            if (ediEntry != null)
                            {
                                MarkEdiEntryAsDuplicate(entities, companyEdi, ediEntry, uri, sysScheduledJobId, batchNr, true, false);
                                createEdiEntry = false;
                                hasDuplicates = true;
                                continue;
                            }

                            //Download file
                            byte[] data = null;
                            if (companyEdi.SourceType == CompanyEdiDTO.SourceTypeEnum.File)
                                data = File.ReadAllBytes(companyEdi.Source);
                            else if (companyEdi.SourceType == CompanyEdiDTO.SourceTypeEnum.FTP)
                                data = FtpUtility.DownloadData(uri, companyEdi.Username, companyEdi.Password);

                            if (DefenderUtil.IsVirus(data))
                            {
                                LogCollector.LogError($"AddEdiEntrysFromSource Virus detected {uri} {companyEdi.SourceType}  {Environment.StackTrace}");
                                continue;
                            }

                            #endregion

                            #region EdiEntry

                            if (data != null && data.Length > 0)
                            {
                                xml = Encoding.UTF8.GetString(data);
                            }

                            if (!string.IsNullOrEmpty(xml))
                            {
                                item = SymbrioEdiItem.CreateItem(xml, fileName, 0, true, true);
                                if (item != null)
                                {
                                    if (!String.IsNullOrEmpty(item.SellerName))
                                    {
                                        //Only check for Lunda and Selga (ask Henrik why)
                                        string wsName = item.SellerName.ToLower();
                                        if (wsName.StartsWith("lunda") || wsName.StartsWith("selga"))
                                        {
                                            var msgType = GetMessageTypeFromFileString(item.MessageType);
                                            if (msgType == TermGroup_EdiMessageType.SupplierInvoice)
                                            {
                                                // Duplicates check 2: If invoice, check that the invoicenr has not been imported before
                                                ediEntry = GetEdiEntryByInvoiceNr(entities, item.SellerName, item.HeadInvoiceNumber, companyEdi.ActorCompanyId);
                                            }
                                            else
                                            {
                                                // If lunda only check active items
                                                bool onlyActive = wsName.StartsWith("lunda");

                                                ediEntry = GetEdiEntryByFileSeller(entities, item.SellerName, item.HeadSellerOrderNumber, companyEdi.ActorCompanyId, onlyActive: onlyActive);
                                            }

                                            if (ediEntry != null)
                                            {
                                                MarkEdiEntryAsDuplicate(entities, companyEdi, ediEntry, uri, sysScheduledJobId, batchNr, false, true, item);
                                                createEdiEntry = false;
                                                continue;
                                            }
                                        }
                                    }

                                    if (TryCreateEdiEntry(entities, item, companyEdi.ActorCompanyId, out ediEntry, sysWholesellerEdiId))
                                    {
                                        ediEntry.Status = (int)TermGroup_EDIStatus.UnderProcessing; //Set UnderProcessing, Processed is set after PDF is generated
                                        ediEntry.ErrorCode = 0;
                                    }
                                    else
                                    {
                                        if (ediEntry != null)
                                        {
                                            ediEntry.Status = (int)TermGroup_EDIStatus.Error;
                                            ediEntry.ErrorCode = (int)ActionResultSave.EdiFailedParse;
                                        }
                                    }
                                }
                            }

                            #endregion
                        }
                        catch (Exception ex)
                        {
                            if (ediEntry != null)
                            {
                                ediEntry.Status = (int)TermGroup_EDIStatus.Error;
                                ediEntry.ErrorCode = (int)ActionResultSave.EdiFailedUnknown;
                            }

                            //Add SysLog this way to ensure logging from a Windows service context
                            base.LogError(ex, this.log);
                        }
                        finally
                        {
                            #region Save EdiEntry and delete file on FTP

                            try
                            {
                                if (ediEntry != null && createEdiEntry)
                                {
                                    result = AddEntityItem(entities, ediEntry, "EdiEntry");
                                    if (result.Success)
                                    {
                                        ediEntryIds.Add(ediEntry.EdiEntryId);

                                        #region Delete File

                                        if (companyEdi.SourceType == CompanyEdiDTO.SourceTypeEnum.FTP)
                                            DeleteFileFromFtp(uri, companyEdi, ediEntry, sysScheduledJobId, batchNr);

                                        #endregion

                                        #region Add/Update products

                                        int customerId = CustomerManager.GetCustomerIdByInvoiceNr(entities, ediEntry.OrderNr, SoeOriginType.Order, companyEdi.ActorCompanyId);

                                        if (item != null && item.Rows != null)
                                        {
                                            foreach (var row in item.Rows)
                                            {
                                                decimal purchaseAmount = row.GetPurchaseAmount();
                                                int sysProductId = 0;

                                                //Use SupplierAgreement discount when price is calculated
                                                var invoiceProduct = ProductManager.CopyExternalInvoiceProductFromSysByProductNr(entities, sysProductId, row.RowSellerArticleNumber, purchaseAmount, ediEntry.SysWholesellerId, row.RowUnitCode, 0, ediEntry.WholesellerName, companyEdi.ActorCompanyId, customerId, true);
                                                if (invoiceProduct == null)
                                                {
                                                    //TODO: Handle if product not could be copied from sys?
                                                    string message = String.Format("Artikel kunde inte kopieras från sys. RowSellerArticleNumber [{0}]", row.RowSellerArticleNumber);
                                                    SysScheduledJobManager.CreateLogEntry(sysScheduledJobId, batchNr, ScheduledJobLogLevel.Information, message);
                                                }
                                            }
                                        }

                                        #endregion
                                    }
                                    else
                                    {
                                        //Prevent that the failed entry is saved for each following file, and thus failed again, and thus failing for the new file
                                        base.TryDetachEntity(entities, ediEntry);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                result.Exception = ex;
                                base.LogError(ex, this.log);
                            }

                            #endregion
                        }

                        noOfFilesProcessed++;

                        #endregion
                    }
                }

                #endregion

                #region Transfer to Order/SupplierInvoice

                if (result.Success && ediEntryIds.Count > 0)
                {
                    TransferToOrdersFromEdi(ediEntryIds, companyEdi.ActorCompanyId, true);
                    TransferToSupplierInvoiceFromEdi(ediEntryIds, companyEdi.ActorCompanyId, true, isAutoFromFtp);
                }

                #endregion

                #region Finalize

                //Set information to service job
                result.IntegerValue = noOfFiles;
                result.IntegerValue2 = noOfFilesProcessed;

                result.Keys = ediEntryIds;

                if (hasDuplicates)
                    result.SuccessNumber = (int)ActionResultSave.Duplicate;

                #endregion
            }
            catch (Exception ex)
            {
                base.LogError(ex, log);
                result.Exception = ex;
            }

            return result;
        }

        public ActionResult AddScanningEntrysFromWebService(int actorCompanyId)
        {
            if (SettingManager.isTest() || SettingManager.isDev())
            {
                var licence = LicenseManager.GetLicenseByCompany(actorCompanyId);
                if (licence == null || licence.LicenseNr != "101")
                {
                    return new ActionResult("Readsoft fakturor kan/får inte hämtas i testmiljön!");
                }
            }

            var result = new ActionResult(true)
            {
                Keys = new List<int>()
            };

            var readsoftApi = new ReadSoftAPI();

            try
            {
                #region Prereq
                if (!readsoftApi.Login())
                    return new ActionResult("Readsoft login failed");

                string apiKey = SettingManager.GetStringSetting(SettingMainType.Company, (int)CompanySettingType.CompanyAPIKey, 0, actorCompanyId, 0);
                bool calcDueDateFromSupplier = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.ScanningCalcDueDateFromSupplier, 0, actorCompanyId, 0);

                var messages = readsoftApi.GetMessages(apiKey);
                if (messages == null || messages.Count == 0)
                {
                    return result;
                }

                var scanningEntryIds = new List<int>();
                var ediEntryIds = new List<int>();
                int noOfItems = 0;
                int noOfItemsProcessed = 0;
                int noOfFailedItems = 0;

                #endregion

                #region Process

                using (var entities = new CompEntities())
                {
                    var company = CompanyManager.GetCompanyDTO(entities, actorCompanyId);
                    var hasWholesellerPriceLists = WholeSellerManager.HasCompanyWholesellerPriceLists(entities, actorCompanyId);

                    if (company == null)
                        return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte"));

                    foreach (ReadSoftMessage message in messages)
                    {
                        if (message.Document == null)
                            continue;

                        #region Process message

                        EdiEntry ediEntry = null;

                        try
                        {
                            #region Prereq

                            //Prevent duplicates when for
                            //1) files that are returned more than once from webservice
                            ScanningEntry scanningEntry = GetScanningEntry(entities, message.Document.Id, actorCompanyId);
                            if (ediEntry != null)
                            {
                                //Log
                                base.LogWarning(string.Format("Dokument är redan inläst. Dokument [{0}]. Status [{1}]. Företag [{2}]", scanningEntry.DocumentId, scanningEntry.Status, scanningEntry.ActorCompanyId));
                                continue;
                            }

                            #endregion

                            #region ScanningEntry

                            var item = ReadSoftScanningItem.CreateItem(message);
                            if (item != null)
                            {
                                if (TryCreateScanningEntry(entities, item, message.Image, company, out scanningEntry, out ediEntry, hasWholesellerPriceLists, calcDueDateFromSupplier))
                                {
                                    scanningEntry.Status = (int)TermGroup_ScanningStatus.Processed;
                                    scanningEntry.ErrorCode = 0;
                                    ediEntry.Status = (int)TermGroup_EDIStatus.Processed;
                                    ediEntry.ErrorCode = 0;
                                }
                                else
                                {
                                    if (ediEntry != null)
                                    {
                                        ediEntry.Status = (int)TermGroup_ScanningStatus.Error;
                                        ediEntry.ErrorCode = (int)ActionResultSave.ScanningFailedParse;
                                    }
                                    if (ediEntry != null)
                                    {
                                        ediEntry.Status = (int)TermGroup_EDIStatus.Error;
                                        ediEntry.ErrorCode = (int)ActionResultSave.ScanningFailedParse;
                                    }
                                }
                            }

                            // Save changes
                            result = SaveChanges(entities);

                            // Set statuses
                            if (result.Success)
                            {
                                if (scanningEntry != null)
                                    scanningEntryIds.Add(scanningEntry.ScanningEntryId);
                                if (ediEntry != null)
                                    ediEntryIds.Add(ediEntry.EdiEntryId);

                                if (readsoftApi.SetDocumentStatus(message.Document.Id, result.Success))
                                {
                                    if (scanningEntry != null)
                                    {
                                        scanningEntry.DocumentStatus = true;
                                        SetModifiedProperties(scanningEntry);

                                        result = SaveChanges(entities);
                                    }
                                }
                            }
                            else
                            {
                                //Prevent that the failed entrys is saved for each following message, and thus failed again, and thus failing for the new message
                                base.TryDetachEntity(entities, scanningEntry);
                                base.TryDetachEntity(entities, ediEntry);

                                readsoftApi.SetDocumentStatus(message.Document.Id, result.Success);

                                noOfFailedItems++;
                            }

                            #endregion
                        }
                        catch (Exception ex)
                        {
                            base.LogError(ex, this.log);
                            return new ActionResult((int)ActionResultSave.ScanningFailedUnknown);
                        }

                        noOfItemsProcessed++;

                        #endregion
                    }
                }

                #endregion

                #region Transfer to SupplierInvoice

                if (result.Success && ediEntryIds.Count > 0)
                {
                    TransferToSupplierInvoiceFromScanning(ediEntryIds, actorCompanyId);
                }

                #endregion

                #region Finalize

                //Set information to service job
                result.DecimalValue = noOfItems;
                result.IntegerValue = noOfItemsProcessed;
                result.IntegerValue2 = noOfFailedItems;
                result.Keys = scanningEntryIds;

                #endregion
            }
            catch (Exception ex)
            {
                base.LogError(ex, log);
                result.Exception = ex;
            }
            finally
            {
                readsoftApi.Logout();
            }
            return result;
        }

        public ActionResult LearnScanningDocument(CompEntities entities, ScanningEntry scanningEntry, Supplier supplier, int actorCompanyId)
        {
            ActionResult result = new ActionResult(true);

            if (SettingManager.isTest() || SettingManager.isDev())
            {
                var licence = LicenseManager.GetLicenseByCompany(entities, actorCompanyId);
                if (licence == null || licence.LicenseNr != "101")
                {
                    return new ActionResult("Readsoft fakturor kan/får inte hanteras i testmiljön!");
                }
            }

            if (scanningEntry == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "ScanningEntry");

            if (scanningEntry.LearningLogLevel == (int)ScanningLogLevel.DoNotSend)
            {
                return result;
            }

            #region Prereq

            ReadSoftScanningItem item = null;
            Dictionary<TermGroup_SysPaymentType, string> paymentNrs = PaymentManager.GetPaymentNrs(entities, supplier.ActorSupplierId);

            if (!scanningEntry.IsAdded())
            {
                if (!scanningEntry.ScanningEntryRow.IsLoaded)
                    scanningEntry.ScanningEntryRow.Load();
            }


            #endregion

            var readSoftApi = new ReadSoftAPI();
            if (!readSoftApi.Login())
                return new ActionResult("Readsoft login failed");

            try
            {
                item = ReadSoftScanningItem.CreateItem(scanningEntry, supplier, paymentNrs);

                if (item != null && item.Document != null)
                {
                    if (readSoftApi.LearnDocument(item.Document.Id, item.Document))
                        scanningEntry.LearningLogLevel = (int)ScanningLogLevel.SentWithTrueReturn;
                    else
                        scanningEntry.LearningLogLevel = (int)ScanningLogLevel.SentWithFalseReturn;

                    SetModifiedProperties(scanningEntry);
                }
            }
            catch (Exception ex)
            {
                base.LogError(ex, this.log);

                scanningEntry.LearningLogLevel = (int)ScanningLogLevel.SentWithException;
                SetModifiedProperties(scanningEntry);
            }
            finally
            {
                readSoftApi.Logout();
                if (!result.Success)
                {
                    //string message = "Failed to learn scanning document";
                    //if (item != null)
                    //    message += String.Format(":{0}", item.ToString());

                    //LogInfo(message);
                }
            }

            return result;
        }

        public string GetScanningEntryDocumentId(int scanningEntryInvoiceId, int actorCompanyId)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            entitiesReadOnly.ScanningEntry.NoTracking();
            string documentId = (from entry in entitiesReadOnly.ScanningEntry
                                 where entry.ActorCompanyId == actorCompanyId &&
                                 entry.ScanningEntryId == scanningEntryInvoiceId
                                 select entry.DocumentId).FirstOrDefault();

            if (documentId == null) return "";


            return documentId;
        }

        private bool TryCreateEdiEntry(CompEntities entities, SymbrioEdiItem item, int actorCompanyId, out EdiEntry ediEntry, int sysWholesellerEdiId = 0)
        {
            ediEntry = null;

            if (item == null)
                return false;

            try
            {
                #region EdiEntry

                ediEntry = new EdiEntry()
                {
                    Type = (int)TermGroup_EDISourceType.EDI,
                    Status = (int)TermGroup_EDIStatus.Unprocessed,
                    PDF = null,
                    SeqNr = 0,

                    //Set FK
                    ActorCompanyId = actorCompanyId,
                };
                SetCreatedProperties(ediEntry);
                entities.EdiEntry.AddObject(ediEntry);

                #region Common

                //XML and files
                ediEntry.XML = item.XML;
                ediEntry.FileName = item.FileName;

                //MessageType
                ediEntry.MessageType = (int)GetMessageTypeFromFileString(item.MessageType);

                //BillingType
                // 1: Debit, 2: Credit, 3: ManualDebit, 4: ManualCredit
                if (item.HeadInvoiceType == 2 || item.HeadInvoiceType == 4)
                {
                    ediEntry.BillingType = (int)TermGroup_BillingType.Credit;
                }
                else
                {
                    ediEntry.BillingType = (int)TermGroup_BillingType.Debit;
                }

                //Seller
                ediEntry.SellerName = item.SellerName;
                ediEntry.SellerOrderNr = item.HeadSellerOrderNumber;

                //Currency
                if (!SetEdiEntryCurrencyAndAmounts(entities, ediEntry, item.HeadCurrencyCode, item.HeadInvoiceGrossAmount, item.HeadVatAmount, null, actorCompanyId))
                    return false;

                //Dates
                ediEntry.Date = item.MessageDate;
                ediEntry.InvoiceDate = item.HeadInvoiceDate;
                ediEntry.DueDate = item.HeadInvoiceDueDate;

                //Bank
                ediEntry.PostalGiro = item.HeadPostalGiro;
                ediEntry.BankGiro = item.HeadBankGiro;
                ediEntry.OCR = item.HeadInvoiceOcr;
                ediEntry.IBAN = item.HeadIbanNumber;

                #endregion

                #region Order

                //Ordernr (assumes that we only have 1 order in each EdiXml)
                if (!String.IsNullOrEmpty(item.HeadBuyerOrderNumber))
                {
                    ediEntry.OrderNr = item.HeadBuyerOrderNumber;
                }
                else
                {
                    SymbrioEdiRowItem rowItem = item.Rows.FirstOrDefault();
                    if (rowItem != null)
                        ediEntry.OrderNr = rowItem.RowBuyerReference;

                }

                //SysWholeseller
                SetEdiEntryWholeseller(ediEntry, item.SellerName, actorCompanyId, sysWholesellerEdiId);

                //Buyer
                ediEntry.BuyerReference = item.BuyerReference;
                ediEntry.BuyerId = item.BuyerId;

                //Validate
                SetEdiEntryOrderStatus(ediEntry, TermGroup_EDIOrderStatus.Unprocessed);

                #endregion

                #region Invoice

                //InvoiceNr
                ediEntry.InvoiceNr = item.HeadInvoiceNumber;

                //Supplier
                ediEntry.Supplier = SupplierManager.GetSupplierByPrio(entities, ediEntry.ActorCompanyId, item.SellerName, item.SellerOrganisationNumber, item.HeadBankGiro, item.HeadPostalGiro, item.HeadIbanNumber, item.HeadBicAddress);

                //Validate
                SetEdiEntryInvoiceStatus(ediEntry, TermGroup_EDIInvoiceStatus.Unprocessed);

                #endregion

                #endregion

                return true;
            }
            catch (Exception ex)
            {
                base.LogError(ex, this.log);
                return false;
            }
        }


        private ActionResult TryCreateEdiEntryAndImportToSoftOne(SymbrioEdiItem item, int actorCompanyId, out EdiEntry ediEntry, int sysWholesellerId, bool TransferToOrder = true, bool TransferToSupplierInvoice = true)
        {
            var result = new ActionResult(false);
            ediEntry = null;
            string sysWholesellerName = string.Empty;

            List<int> ediEntryIds = new List<int>();

            if (item == null)
            {
                result.ErrorMessage = "item is null";
                return result;
            }

            int supplierId = 0;
            var wholeseller = WholeSellerManager.GetSysWholesellerDTO(sysWholesellerId);
            if (wholeseller != null)
            {
                sysWholesellerName = wholeseller.Name;
                supplierId = SupplierManager.GetSupplierIdBySysWholeseller(actorCompanyId, wholeseller.SysWholesellerId).GetValueOrDefault();
            }

            if (supplierId == 0)
            {
                var supplier = SupplierManager.GetSupplierByPrio(actorCompanyId, item.SellerName, item.SellerOrganisationNumber, item.HeadBankGiro, item.HeadPostalGiro, item.HeadIbanNumber, item.HeadBicAddress);
                if (supplier != null)
                    supplierId = supplier.ActorSupplierId;
            }

            // Do some duplicate checks
            var ediEntryExist = EdiEntryExists(item.FileName, actorCompanyId);
            if (ediEntryExist)
            {
                result.ErrorMessage = "Duplicate found!";
                return result;
            }

            using (CompEntities entities = new CompEntities())
            {
                entities.Connection.Open();

                try
                {
                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        #region EdiEntry

                        ediEntry = new EdiEntry()
                        {
                            Type = (int)TermGroup_EDISourceType.EDI,
                            Status = (int)TermGroup_EDIStatus.Unprocessed,
                            PDF = null,
                            SeqNr = 0,

                            //Set FK
                            ActorCompanyId = actorCompanyId,
                        };
                        SetCreatedProperties(ediEntry);
                        entities.EdiEntry.AddObject(ediEntry);

                        #region Common

                        ediEntry.XML = item.XML;
                        ediEntry.FileName = item.FileName;

                        //MessageType
                        ediEntry.MessageType = (int)GetMessageTypeFromFileString(item.MessageType);

                        //BillingType
                        // 1: Debit, 2: Credit, 3: ManualDebit, 4: ManualCredit
                        if (item.HeadInvoiceType == 2 || item.HeadInvoiceType == 4)
                        {
                            ediEntry.BillingType = (int)TermGroup_BillingType.Credit;
                        }
                        else
                        {
                            ediEntry.BillingType = (int)TermGroup_BillingType.Debit;
                        }

                        //Seller
                        ediEntry.SellerName = item.SellerName;
                        ediEntry.SellerOrderNr = item.HeadSellerOrderNumber;

                        //Currency
                        if (!SetEdiEntryCurrencyAndAmounts(entities, ediEntry, item.HeadCurrencyCode, item.HeadInvoiceGrossAmount, item.HeadVatAmount, null, actorCompanyId))
                        {
                            ediEntry.Status = (int)TermGroup_EDIStatus.Error;
                            ediEntry.ErrorCode = (int)ActionResultSave.EdiFailedParse;
                        }

                        //Dates
                        ediEntry.Date = item.MessageDate == DateTime.MinValue ? DateTime.Today : item.MessageDate;
                        ediEntry.InvoiceDate = item.HeadInvoiceDate;
                        ediEntry.DueDate = item.HeadInvoiceDueDate;

                        //Bank
                        ediEntry.PostalGiro = item.HeadPostalGiro;
                        ediEntry.BankGiro = item.HeadBankGiro;
                        ediEntry.OCR = item.HeadInvoiceOcr;
                        ediEntry.IBAN = item.HeadIbanNumber;

                        #endregion

                        #region Order

                        //Ordernr (assumes that we only have 1 order in each EdiXml)
                        if (!string.IsNullOrEmpty(item.HeadBuyerOrderNumber))
                        {
                            ediEntry.OrderNr = item.HeadBuyerOrderNumber;
                        }
                        else
                        {
                            SymbrioEdiRowItem rowItem = item.Rows.FirstOrDefault();
                            if (rowItem != null)
                                ediEntry.OrderNr = rowItem.RowBuyerReference;
                        }

                        if (ediEntry.OrderNr != null && (ediEntry.OrderNr.Length > 50))
                        {
                            ediEntry.OrderNr = ediEntry.OrderNr.Substring(0, 50);
                        }
                        //SysWholeseller
                        ediEntry.SysWholesellerId = sysWholesellerId;
                        ediEntry.WholesellerName = sysWholesellerName;

                        //Buyer
                        ediEntry.BuyerReference = item.BuyerReference;
                        ediEntry.BuyerId = item.BuyerId;

                        //Validate
                        SetEdiEntryOrderStatus(ediEntry, TermGroup_EDIOrderStatus.Unprocessed);

                        #endregion

                        #region Invoice

                        //InvoiceNr
                        ediEntry.InvoiceNr = item.HeadInvoiceNumber;

                        //Supplier
                        ediEntry.ActorSupplierId = supplierId != 0 ? supplierId : (int?)null;


                        //Validate
                        SetEdiEntryInvoiceStatus(ediEntry, TermGroup_EDIInvoiceStatus.Unprocessed);

                        #endregion

                        ediEntry.Status = (int)TermGroup_EDIStatus.UnderProcessing;
                        ediEntry.ErrorCode = 0;

                        result = SaveChanges(entities, transaction);

                        #endregion

                        if (result.Success)
                        {
                            ediEntryIds.Add(ediEntry.EdiEntryId);

                            //Commit transaction
                            transaction.Complete();
                        }
                        else
                        {
                            result.ErrorMessage = "";
                            return result;
                        }
                    }
                }
                catch (Exception ex)
                {
                    base.LogError(ex, log);
                    result.Exception = ex;
                    result.Success = false;
                    result.ErrorMessage = ex.ToString();
                }
                finally
                {
                    if (result.Success)
                    {
                        entities.Connection.Close();
                    }
                }
            }

            #region PostHandling

            try
            {

                using (CompEntities entities = new CompEntities())
                {
                    int sysPriceListHeadId, priceListImportedHeadId;
                    CopyExternalProducts(item, actorCompanyId, ediEntry, wholeseller, entities, out sysPriceListHeadId, out priceListImportedHeadId);
                    #region Process to SoftOne

                    if (ediEntryIds.Count > 0)
                    {
                        if (TransferToOrder)
                        {
                            var productExternalPriceListHeadId = sysPriceListHeadId > 0 ? sysPriceListHeadId : priceListImportedHeadId;
                            TransferToOrdersFromEdi(ediEntryIds, actorCompanyId, true, productExternalPriceListHeadId);
                        }

                        if (TransferToSupplierInvoice)
                            TransferToSupplierInvoiceFromEdi(ediEntryIds, actorCompanyId, true, false);
                    }

                    #endregion

                    result = SaveChanges(entities);
                }
            }
            catch (Exception ex)
            {
                base.LogError(ex, log);
                result.Exception = ex;
                result.Success = false;
                result.ErrorMessage = ex.ToString();
            }

            #endregion

            return result;
        }

        private void CopyExternalProducts(SymbrioEdiItem item, int actorCompanyId, EdiEntry ediEntry, SysWholesellerDTO wholeseller, CompEntities entities, out int sysPriceListHeadId, out int priceListImportedHeadId)
        {
            #region Add/Update products

            int customerId = CustomerManager.GetCustomerIdByInvoiceNr(entities, ediEntry.OrderNr, SoeOriginType.Order, actorCompanyId);

            int sysWholesellerId = ediEntry.SysWholesellerId;
            if (sysWholesellerId == 65)
            {
                sysWholesellerId = WholeSellerManager.GetWholesellerFromComfortReference(item.SellerReference)?.SysWholesellerId ?? ediEntry.SysWholesellerId;
            }

            int sysWholesellerIdEx = sysWholesellerId;
            sysPriceListHeadId = WholeSellerManager.GetMostRecentCompanyWholesellerPriceListEx(entities, actorCompanyId, ref sysWholesellerIdEx);

            priceListImportedHeadId = 0;
            if (item != null && item.Rows != null)
            {
                foreach (var row in item.Rows)
                {
                    if (string.IsNullOrEmpty(row.RowSellerArticleNumber) && row.ExternalProductId == 0)
                    {
                        continue;
                    }
                    decimal purchaseAmount = row.GetPurchaseAmount();

                    //check if invoice product already exists with same pricelist??
                    if (row.ExternalProductId > 0)
                    {
                        var existingInvoiceProduct = ProductManager.GetInvoiceProduct(entities, row.ExternalProductId, actorCompanyId, PriceListOrigin.SysDbPriceList, sysPriceListHeadId);
                        if (existingInvoiceProduct != null)
                        {
                            continue;
                        }
                    }

                    var unitCode = row.RowUnitCode;
                    if (wholeseller?.SysCountryId == (int)TermGroup_Languages.Finnish && sysPriceListHeadId > 0)
                    {
                        //finnish special since some wholeseller sent there unit name in english in edi messages
                        var sysPriceList = SysPriceListManager.GetSysPriceList(row.ExternalProductId, sysPriceListHeadId);
                        if (sysPriceList != null && !string.IsNullOrEmpty(sysPriceList.PurchaseUnit))
                        {
                            unitCode = sysPriceList.PurchaseUnit;
                        }
                    }


                    //Use SupplierAgreement discount when price is calculated
                    var invoiceProduct = ProductManager.CopyExternalInvoiceProductFromSysByProductNr(entities, row.ExternalProductId, row.RowSellerArticleNumber, purchaseAmount, sysWholesellerId, unitCode, sysPriceListHeadId, ediEntry.WholesellerName, actorCompanyId, customerId, true);
                    if (invoiceProduct == null)
                    {
                        //Perhaps it is a company pricelist...
                        invoiceProduct = ProductManager.CopyExternalInvoiceProductFromCompPriceListByProductNr(entities, row.RowSellerArticleNumber, purchaseAmount, sysWholesellerId, unitCode, ediEntry.WholesellerName, actorCompanyId, customerId);
                        if (invoiceProduct == null)
                        {
                            //TODO: Handle if product not could be copied from sys?
                            string message = string.Format("Artikel kunde inte kopieras från sys. RowSellerArticleNumber [{0}]", row.RowSellerArticleNumber);
                            //SysScheduledJobManager.CreateLogEntry(sysScheduledJobId, batchNr, ScheduledJobLogLevel.Information, message);
                        }
                        else if (priceListImportedHeadId == 0 && sysPriceListHeadId == 0)
                        {
                            priceListImportedHeadId = invoiceProduct.ExternalPriceListHeadId.GetValueOrDefault();
                        }
                    }
                }
            }

            #endregion
        }

        private TermGroup_EdiMessageType GetMessageTypeFromFileString(string messageTypeString)
        {
            if (messageTypeString.ToLower().Contains("invoice"))
                return TermGroup_EdiMessageType.SupplierInvoice;
            else if (messageTypeString.ToLower().Contains("order"))
                return TermGroup_EdiMessageType.OrderAcknowledgement;
            else if (messageTypeString.ToLower().Contains("leverans") || messageTypeString.ToLower().Contains("levbesked"))
                return TermGroup_EdiMessageType.DeliveryNotification;
            else
                return TermGroup_EdiMessageType.Unknown;
        }

        private bool TryCreateScanningEntry(CompEntities entities, ReadSoftScanningItem item, byte[] image, CompanyDTO company, out ScanningEntry scanningEntry, out EdiEntry ediEntry, bool setWholeSellerFromPriceLists, bool calcDueDateFromSupplier)
        {
            bool success = false;
            scanningEntry = null;
            ediEntry = null;

            if (item == null || item.Parties == null || item.HeaderFields == null || company == null)
                return false;

            ScanningLogLevel logLevel = ScanningLogLevel.NotSent;

            var fileName = item.OriginalFileName?.ToLower();
            if (!string.IsNullOrEmpty(fileName) && fileName.StartsWith("efaktura") && fileName.EndsWith(".xml"))
            {
                logLevel = ScanningLogLevel.DoNotSend;
            }

            try
            {
                #region ScanningEntry

                scanningEntry = new ScanningEntry()
                {
                    Type = (int)TermGroup_EDISourceType.Scanning,
                    MessageType = (int)TermGroup_ScanningMessageType.SupplierInvoice,
                    Status = (int)TermGroup_ScanningStatus.Unprocessed,
                    XML = item.ToXDocument().ToString(),
                    Image = image,
                    BatchId = item.BatchId,
                    DocumentId = item.DocumentId,
                    ReceiveTime = item.ReceiveTime,
                    OperatorMessage = string.Empty,
                    CompanyId = "0", //Set below
                    SupplierId = "0", //Set below
                    LearningLogLevel = (int)logLevel,
                    Provider = (int)ScanningProvider.ReadSoft,
                    //Set FK
                    ActorCompanyId = company.ActorCompanyId,
                };


                SetCreatedProperties(scanningEntry);
                entities.ScanningEntry.AddObject(scanningEntry);

                #region ScanningEntryRow

                foreach (var headerField in item.HeaderFields)
                {
                    ScanningEntryRowType rowType = ReadSoftScanningItem.GetScanningEntryRowType(headerField.Type);
                    if (rowType == ScanningEntryRowType.Unknown)
                    {
                        LogError(string.Format("Scannat dokument innehåller felaktiga format eller fält som inte stöds. Dokument:{0} Företag:{1}.{2} Typ: {3}", item.DocumentId, company.ActorCompanyId, company.Name, headerField.Type));
                        return false;
                    }

                    var scanningEntryRow = new ScanningEntryRow
                    {
                        Type = (int)rowType,
                        TypeName = headerField.Type,
                        Name = StringUtility.NullToEmpty(headerField.Name),
                        Text = StringUtility.NullToEmpty(headerField.Text),
                        Format = StringUtility.NullToEmpty(headerField.Format),
                        ValidationError = StringUtility.NullToEmpty(headerField.ValidationError),
                        Position = StringUtility.NullToEmpty(headerField.Position),
                        PageNumber = StringUtility.NullToEmpty(headerField.PageNumber),
                    };

                    SetCreatedProperties(scanningEntryRow);
                    entities.ScanningEntryRow.AddObject(scanningEntryRow);

                    scanningEntry.ScanningEntryRow.Add(scanningEntryRow);
                }

                #endregion

                #endregion

                #region EdiEntry

                ediEntry = new EdiEntry
                {
                    Type = (int)TermGroup_EDISourceType.Scanning,
                    MessageType = (int)TermGroup_EdiMessageType.SupplierInvoice,
                    Status = (int)TermGroup_EDIStatus.Unprocessed,
                    PDF = null,
                    XML = item.ToXDocument().ToString(),
                    FileName = item.OriginalFileName,
                    Date = item.ReceiveTime,
                    SeqNr = 0,

                    //Set FK
                    ActorCompanyId = company.ActorCompanyId,

                    //Set references
                    ScanningEntryInvoice = scanningEntry,
                };
                SetCreatedProperties(ediEntry);

                if (image.Length > 0)
                {
                    var stream = new MemoryStream(image);
                    var imageType = ImageUtil.GetFileImageTypeFromHeader(stream);
                    if (imageType != ImageUtil.ImageType.TIFF)
                    {
                        ediEntry.PDF = image;
                        ediEntry.ImageStorageType = (int)SoeInvoiceImageStorageType.StoredInEdiEntry;
                    }
                }

                entities.EdiEntry.AddObject(ediEntry);

                //BillingType
                ediEntry.BillingType = (int)scanningEntry.GetBillingType(defaultBillingType: TermGroup_BillingType.Debit);

                //Currency
                if (!SetEdiEntryCurrencyAndAmounts(entities, ediEntry, scanningEntry.GetCurrencyCode(), scanningEntry.GetTotalAmountIncludeVat(), scanningEntry.GetVatAmount(), scanningEntry.GetVatRate(), company.ActorCompanyId))
                    success = false;

                //Dates
                ediEntry.InvoiceDate = scanningEntry.GetInvoiceDate();
                ediEntry.DueDate = scanningEntry.GetDueDate();

                //Bank
                ediEntry.PostalGiro = scanningEntry.GetPlusgiro();
                ediEntry.BankGiro = scanningEntry.GetBankgiro();
                ediEntry.OCR = scanningEntry.GetOCRNr();
                ediEntry.IBAN = scanningEntry.GetIBAN();

                //Buyer
                ediEntry.BuyerReference = scanningEntry.GetReferenceYour();

                #endregion

                #region Parties

                var partyBuyer = item.GetBuyer();
                if (partyBuyer != null)
                {
                    #region Buyer

                    //Name and external id
                    scanningEntry.CompanyId = String.Format("{0} ({1})", partyBuyer.Name, partyBuyer.ExternalId);

                    //Customernr at wholeseller
                    ediEntry.BuyerId = partyBuyer.Name;

                    #endregion
                }

                var partySupplier = item.GetSupplier();
                if (partySupplier != null)
                {
                    #region Supplier

                    //Buyer
                    scanningEntry.SupplierId = partySupplier.Name;

                    //SysWholeseller
                    if (setWholeSellerFromPriceLists)
                    {
                        SetEdiEntryWholeseller(ediEntry, partySupplier.Name, company.ActorCompanyId);
                    }
                    else
                    {
                        ediEntry.WholesellerName = partySupplier.Name;
                    }

                    //Supplier
                    ediEntry.Supplier = SupplierManager.GetSupplierByPrio(entities, ediEntry.ActorCompanyId, partySupplier.Name, scanningEntry.GetOrgNr(), scanningEntry.GetBankgiro(), scanningEntry.GetPlusgiro(), scanningEntry.GetIBAN(), string.Empty);
                    if (ediEntry.Supplier != null && calcDueDateFromSupplier && ediEntry.InvoiceDate.HasValue)
                    {
                        var paymentConditionId = ediEntry.Supplier.PaymentConditionId ?? SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.SupplierPaymentDefaultPaymentCondition, 0, company.ActorCompanyId, 0);
                        if (paymentConditionId > 0)
                        {
                            ediEntry.DueDate = ediEntry.InvoiceDate.Value.AddDays(PaymentManager.GetPaymentConditionDays(paymentConditionId, company.ActorCompanyId));
                        }
                    }

                    #endregion
                }

                #endregion

                #region Order

                ediEntry.OrderNr = string.Empty;
                ediEntry.BuyerReference = scanningEntry.GetReferenceYour();

                //Validate
                SetEdiEntryOrderStatus(ediEntry, TermGroup_EDIOrderStatus.Unprocessed);

                #endregion

                #region Invoice

                ediEntry.InvoiceNr = scanningEntry.GetInvoiceNr();

                //Validate
                SetEdiEntryInvoiceStatus(ediEntry, TermGroup_EDIInvoiceStatus.Unprocessed);

                #endregion

                success = true;
            }
            catch (Exception ex)
            {
                base.LogError(ex, this.log);
                success = false;
            }

            return success;
        }

        private string GetCompanyEdiTypeName(CompanyEdiDTO companyEdi)
        {
            string typeName = "";
            if (companyEdi != null)
            {
                switch (companyEdi.Type)
                {
                    case (int)TermGroup_CompanyEdiType.Symbrio:
                        typeName = ""; //Standard
                        break;
                    case (int)TermGroup_CompanyEdiType.Nelfo:
                        typeName = "[Nelfo]";
                        break;
                }
            }
            return typeName;
        }

        public ActionResult MarkEdiEntryAsDuplicate(CompEntities entities, CompanyEdiDTO companyEdi, EdiEntry ediEntry, Uri uri, int sysScheduledJobId, int batchNr, bool duplicateByFileName, bool duplicateBySeller, SymbrioEdiItem item = null)
        {
            ActionResult result = new ActionResult();

            //Log
            string reason = "";
            if (duplicateByFileName)
                reason = "med samma filnamn";
            else if (duplicateBySeller)
                reason = "med samma säljare och säljarordernr";
            string message = String.Format("Fil {0} är redan inläst. Fil [{1}]. Status [{2}]. Företag [{3}]", reason, ediEntry.FileName, ediEntry.Status, ediEntry.ActorCompanyId);
            SysScheduledJobManager.CreateLogEntry(sysScheduledJobId, batchNr, ScheduledJobLogLevel.Information, message);
            base.LogWarning(message);

            if (item != null && this.TryCreateEdiEntry(entities, item, companyEdi.ActorCompanyId, out ediEntry))
            {
                ediEntry.State = (int)SoeEntityState.Deleted;
                ediEntry.Status = (int)TermGroup_EDIStatus.Duplicate;

                result = SaveChanges(entities);
                if (result.Success)
                {
                    //Delete file
                    if (companyEdi.SourceType == CompanyEdiDTO.SourceTypeEnum.File)
                        File.Delete(companyEdi.Source);
                    else if (companyEdi.SourceType == CompanyEdiDTO.SourceTypeEnum.FTP)
                        DeleteFileFromFtp(uri, companyEdi, ediEntry, sysScheduledJobId, batchNr);

                    SysScheduledJobManager.CreateLogEntry(sysScheduledJobId, batchNr, ScheduledJobLogLevel.Information, "Duplikat är inlagt som ett duplikat med state deleted samt borttagen från servern");
                }
            }
            else
                base.TryDetachEntity(entities, ediEntry);

            return result;
        }

        private void DeleteFileFromFtp(Uri fileUri, CompanyEdiDTO companyEdi, EdiEntry ediEntry, int sysScheduledJobId, int batchNr)
        {
            DeleteFileFromFtp(fileUri, companyEdi, sysScheduledJobId, batchNr, ediEntry.FileName, ediEntry.Status);
        }

        private void DeleteFileFromFtp(Uri fileUri, CompanyEdiDTO companyEdi, int sysScheduledJobId, int batchNr, string fileName = null, int? status = null)
        {
            string message;

            if (String.IsNullOrEmpty(fileName))
                fileName = fileUri.ToString();

            //Only delete files that are UnderProcessing or Processed or Duplicates
            bool fileDeleted = false;
            if (status.HasValue == false || (status == (int)TermGroup_EDIStatus.UnderProcessing || status == (int)TermGroup_EDIStatus.Processed || status == (int)TermGroup_EDIStatus.Duplicate))
            {
                //Delete file
                if (FtpUtility.DeleteFile(fileUri, companyEdi.Username, companyEdi.Password, out Exception ex))
                {
                    fileDeleted = true;
                }
                else
                {
                    if (ex != null)
                        base.LogError(ex, this.log);

                    //Log to SysScheduledJobLog
                    message = String.Format("Fil kunde inte tas bort och orsakade exception. Fil [{0}]. Status [{1}]. Företag [{2}]", fileName, status, companyEdi.ActorCompanyId);
                    SysScheduledJobManager.CreateLogEntry(sysScheduledJobId, batchNr, ScheduledJobLogLevel.Information, message);
                }
            }
            else
            {
                //Log to SysScheduledJobLog
                message = String.Format("Fil har felaktig status för att tas bort. Fil [{0}]. Status [{1}]. Företag [{2}]", fileName, status, companyEdi.ActorCompanyId);
                SysScheduledJobManager.CreateLogEntry(sysScheduledJobId, batchNr, ScheduledJobLogLevel.Information, message);
            }

            if (fileDeleted)
            {
                //Log to SysScheduledJobLog
                message = String.Format("Fil borttagen från FTP-servern. Fil [{0}]. Status [{1}]. Företag [{2}]", fileName, status, companyEdi.ActorCompanyId);
                SysScheduledJobManager.CreateLogEntry(sysScheduledJobId, batchNr, ScheduledJobLogLevel.Information, message);
            }
            else
            {
                //Log to SysScheduledJobLog
                message = String.Format("Fil kunde inte tas bort från FTP-servern. Fil [{0}]. Status [{1}]. Företag [{2}]", fileName, status, companyEdi.ActorCompanyId);
                SysScheduledJobManager.CreateLogEntry(sysScheduledJobId, batchNr, ScheduledJobLogLevel.Information, message);
            }
        }

        private ActionResult GetFilesFromFtp(CompanyEdiDTO companyEdi, Uri uri, bool ignoreFolders, out List<string> fileNames, params string[] ignoreFiles)
        {
            string typeName = GetCompanyEdiTypeName(companyEdi);

            try
            {
                fileNames = FtpUtility.GetFileList(uri, companyEdi.Username, companyEdi.Password, ignoreFolders, ignoreFilesStartingWithDot: true, ignoreFilesList: ignoreFiles);
            }
            catch (Exception ex)
            {
                fileNames = new List<string>();
                base.LogError(ex, this.log);
                return new ActionResult((int)ActionResultSave.EdiFailedFileListing, String.Format("Failed to list files on FTP {0}", typeName));
            }

            return new ActionResult(true);
        }

        private ActionResult ValidateCompanyEdi(CompanyEdiDTO companyEdi, bool allowEmptyCredentials, out Uri uri, bool tryValidateUserName = false)
        {
            uri = null;
            string typeName = GetCompanyEdiTypeName(companyEdi);

            if (companyEdi == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, String.Format("CompanyEdi {0}", typeName));

            if (!allowEmptyCredentials && companyEdi.SourceType == CompanyEdiDTO.SourceTypeEnum.FTP)
            {
                //Validate username and password
                if ((String.IsNullOrEmpty(companyEdi.Username)) || (String.IsNullOrEmpty(companyEdi.Password)))
                    return new ActionResult((int)ActionResultSave.EdiInvalidUri, String.Format("Invalid username or password {0}", typeName));
            }

            //Validate address
            if (String.IsNullOrEmpty(companyEdi.Source))
                return new ActionResult((int)ActionResultSave.EdiInvalidUri, String.Format("No FTP URI found {0}", typeName));

            //Validate uri or folder
            if (companyEdi.SourceType == CompanyEdiDTO.SourceTypeEnum.File)
            {
                if (!File.Exists(companyEdi.Source))
                    return new ActionResult((int)ActionResultSave.EdiFailedFileListing, string.Format("File does not exist: {0}", companyEdi.Source));
            }
            else if (companyEdi.SourceType == CompanyEdiDTO.SourceTypeEnum.FTP)
            {
                uri = new Uri(companyEdi.Source);
                if (uri == null || uri.Scheme != Uri.UriSchemeFtp)
                    return new ActionResult((int)ActionResultSave.EdiInvalidUri, String.Format("Invalid FTP URI {0}", typeName));
            }

            //Validate if correct user
            if (companyEdi != null && tryValidateUserName)
            {
                if (companyEdi.Username != null && companyEdi.Username.ToLower() != "test" && companyEdi.Username.ToLower() != "xe-test")
                {
                    if (companyEdi.CreatedBy != null && companyEdi.CreatedBy == companyEdi.Username)
                        return new ActionResult((int)ActionResultSave.EdiInvalidUri, String.Format("Invalid Username Chrome {0}", companyEdi.Username));
                    if (!long.TryParse(companyEdi.Username.Trim(), out _))
                        return new ActionResult((int)ActionResultSave.EdiInvalidUri, String.Format("Invalid Username Not Numbers {0}", companyEdi.Username));
                }
            }

            return new ActionResult(true);

        }

        #endregion

        #region Import from file        

        public ActionResult AddFinvoiceAttachment(string filename, int actorCompanyId, Stream content)
        {
            var finvoiceAttachment = new FinvoiceAttachmentItem();

            var result = finvoiceAttachment.Parse(content);

            if (!result.Success)
            {
                LogError("AddFinvoiceAttachment: " + result.ErrorMessage);
                return new ActionResult($" {GetText(8176, "Kan inte läsa från XML fil")}: {filename}");
            }

            var messageList = new List<string>();
            var errorList = new List<string>();

            if (result.Success)
            {
                using (var entities = new CompEntities())
                {

                    var supplierInvoice = SupplierInvoiceManager.GetSupplierInvoiceSmallByExternalId(entities, actorCompanyId, finvoiceAttachment.RefToMessageIdentifier);

                    if (supplierInvoice == null)
                    {
                        var ediEntry = this.GetValidEdiEntryByExternalId(entities, actorCompanyId, TermGroup_EDISourceType.Finvoice, finvoiceAttachment.RefToMessageIdentifier);
                        if (ediEntry == null)
                            return new ActionResult($"{GetText(7567, "Ingen leveranstörsfaktura hittades för")}: {filename}");
                        else
                        {
                            foreach (var file in finvoiceAttachment.AttachmentDetails)
                            {
                                var fileDto = new DataStorageRecordExtendedDTO
                                {
                                    RecordId = ediEntry.EdiEntryId,
                                    Data = file.AttachmentContent,
                                    FileName = file.AttachmentName,
                                    Type = SoeDataStorageRecordType.EdiEntry_Document

                                };
                                var fileResult = GeneralManager.SaveDataStorageRecord(entities, actorCompanyId, fileDto, false);
                                if (fileResult.Success)
                                {
                                    messageList.Add($"{GetText(1830, "Faktura")}: {ediEntry.InvoiceNr}, {GetText(11840, "Dokument")}: {file.AttachmentName}");
                                }
                                else
                                {
                                    errorList.Add($"{GetText(1830, "Faktura")}: {ediEntry.InvoiceNr}, {GetText(11840, "Dokument")}: {file.AttachmentName}");
                                }
                            }
                        }
                    }
                    else
                    {
                        foreach (var file in finvoiceAttachment.AttachmentDetails)
                        {
                            var saveResult = InvoiceManager.SaveOrderInvoiceAttachment(entities, supplierInvoice.InvoiceId, new DataStorageRecordExtendedDTO
                            {
                                RecordId = supplierInvoice.InvoiceId,
                                Data = file.AttachmentContent,
                                FileName = file.AttachmentName,
                                Type = SoeDataStorageRecordType.OrderInvoiceFileAttachment

                            }, actorCompanyId);

                            if (saveResult.Success)
                            {
                                messageList.Add($"{GetText(1830, "Faktura")}: {supplierInvoice.SeqNr} {supplierInvoice.SupplierName}, {GetText(11840, "Dokument")}: {file.AttachmentName}");
                            }
                            else
                            {
                                errorList.Add($"{GetText(1830, "Faktura")}: {supplierInvoice.SeqNr} {supplierInvoice.SupplierName}, {GetText(11840, "Dokument")}: {file.AttachmentName}");
                            }
                        }


                    }
                }
            }

            if (messageList.Any())
            {
                messageList.Insert(0, GetText(7566, "Följande bilagor lades till på fakturor"));
                result.InfoMessage = string.Join("\n", messageList.ToArray());
            }

            if (errorList.Any())
            {
                result.ErrorMessage = string.Join("\n", errorList.ToArray());
                result.Success = false;
            }

            return result;
        }

		public async Task<ActionResult> AddFinvoiceFromFileImportAsync(string pathOnServer, string fileName, int actorCompanyId, string content, bool isBankIntegration = false)
        {
			if (content is null && pathOnServer is null)
				return new ActionResult();

            int numberOfImported = 0;
			List<EdiEntry> savedEdiEntries = new();

			var contentString = content ?? File.ReadAllText(pathOnServer, Encoding.GetEncoding("ISO-8859-15"));
            bool includesEnvelope = FinvoiceHelper.ContainsEnvelope(contentString);
			contentString = FinvoiceHelper.EscapeUnescapedAmpersands(contentString);
			var finvoiceItems = includesEnvelope
                ? FinvoiceHelper.ParseFinvoicesWithEnvelope(contentString)
                : FinvoiceHelper.ParseFinvoiceWithoutEnvelope(contentString);

            if (finvoiceItems.Count == 0)
                return new ActionResult();

            ActionResult result;
            using var entities = new CompEntities();
            try
            {
                bool importOnlyValidForCompany = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.FinvoiceImportOnlyForCompany, 0, actorCompanyId, 0);
                string companyAddressIdentifier = SettingManager.GetStringSetting(SettingMainType.Company, (int)CompanySettingType.BillingFinvoiceAddress, 0, actorCompanyId, 0);
                var xdocs = FinvoiceHelper.ParseDocuments(finvoiceItems, importOnlyValidForCompany, companyAddressIdentifier, includesEnvelope);
                result = CreateFinvoiceEdiEntries(entities, actorCompanyId, xdocs, savedEdiEntries, fileName, isBankIntegration);
				numberOfImported = result.IntegerValue;
            }
            catch (Exception ex)
            {
                base.LogError(ex, this.log);
                return new ActionResult((int)ActionResultSave.Unknown, GetText(8188, "Okänt fel") + ": " + ex.Message);
            }

            if (!result.Success || savedEdiEntries.Count == 0)
                return new ActionResult();

            await GeneratePdfsForEntries(savedEdiEntries, actorCompanyId);
            result = SaveChanges(entities);

            if (!result.Success)
                return new ActionResult();
            
            result = TransferToSupplierInvoiceFromFinvoice(savedEdiEntries.Select(x => x.EdiEntryId).ToList(), actorCompanyId);
            result.IntegerValue = numberOfImported;
            result.IntegerValue2 = finvoiceItems.Count;

            return result;
        }

		private async Task GeneratePdfsForEntries(List<EdiEntry> entries, int actorCompanyId)
		{
			foreach (var entry in entries)
			{
				await TryGetExternalPdf(entry);

				if (entry.PDF is null)
					TryGeneratePdf(entry);

				//Create pdf files, old ways if above failed...should probably be removed in the future...
				if (entry.PDF is null)
					TryGeneratePdfLegacy(entry, actorCompanyId);

				if (entry.PDF is not null)
					entry.ImageStorageType = (int)SoeInvoiceImageStorageType.StoredInEdiEntry;
			}
		}

		private async Task TryGetExternalPdf(EdiEntry entry)
		{
			try {
				entry.PDF = await FinvoiceHelper.GetExternalInvoice(entry.XML).ConfigureAwait(false);
			}
			catch (Exception ex) {
				base.LogError(ex, this.log);
			}
		}

		private void TryGeneratePdf(EdiEntry entry)
		{
			try {
				entry.PDF = FInvoiceFileGen.GetPdf(entry);
			}
			catch (Exception ex) {
				base.LogError(ex, this.log);
			}
		}

		private void TryGeneratePdfLegacy(EdiEntry entry, int actorCompanyId)
		{
			try {
				ReportDataManager.GenerateReportForFinvoice(new List<int> { entry.EdiEntryId }, actorCompanyId);
			}
			catch (Exception ex) {
				base.LogError(ex, this.log);
			}
		}

        private ActionResult CreateFinvoiceEdiEntries(CompEntities entities, int actorCompanyId, IReadOnlyList<XDocument> xdocs, List<EdiEntry> savedEdiEntries, string fileName, bool isBankIntegration)
        {
            foreach (var item in xdocs)
            {
                var finvoice = new FinvoiceEdiItem(item, parameterObject);
                if (finvoice is null)
                    return new ActionResult((int)ActionResultSave.EdiFailedParse, GetText(8177, "Kan inte parsa XML fil"));

                EdiEntry ediEntry = new()
                {
                    Type = (int)TermGroup_EDISourceType.Finvoice,
                    FileName = fileName,
                    Status = (int)TermGroup_EDIStatus.Unprocessed,
                    BillingType = finvoice.invoiceDetails.SoeCompatibleBillingType,
                    Source = isBankIntegration ? (int)EdiImportSource.BankIntegration : (int)EdiImportSource.FileImport,
                };

                if (!TryParseFinvoiceXml(entities, ediEntry, finvoice, actorCompanyId))
                    return new ActionResult((int)ActionResultSave.EdiFailedParse, GetText(8176, "Kan inte läsa från XML fil"));

                if (SupplierInvoiceExists(entities, ediEntry))
                {
                    ediEntry.ErrorCode = (int)ActionResultSave.Duplicate;
                    ediEntry.ErrorMessage = GetText(4887, "Faktura finns redan");
                    ediEntry.Status = (int)TermGroup_EDIStatus.Duplicate;
                    ediEntry.InvoiceStatus = (int)TermGroup_EDIInvoiceStatus.Error;
                }
                else
                {
                    ediEntry.Status = (int)TermGroup_EDIStatus.Processed;
                    ediEntry.ErrorCode = 0;
                }

                SetCreatedProperties(ediEntry);
                entities.EdiEntry.AddObject(ediEntry);

                var result = SaveChanges(entities);
                if (!result.Success)
                    return new ActionResult((int)ActionResultSave.NothingSaved);

                savedEdiEntries.Add(ediEntry);
            }
            return new ActionResult() { Success = true, IntegerValue = savedEdiEntries.Count };
        }

        private bool TryParseFinvoiceXml(CompEntities entities, EdiEntry entry, FinvoiceEdiItem finvoice, int actorCompanyId)
        {
            #region Init

            bool result = false;
            if (entry == null)
                return result;

            //Mandatory fields
            entry.ActorCompanyId = actorCompanyId;
            entry.PDF = null;
            entry.CurrencyId = 0;

            #endregion

            try
            {
                #region Set data

                //Common
                entry.XML = Regex.Replace(finvoice.xdoc.ToString(), "<!DOCTYPE.+?>", string.Empty);
                entry.Date = DateTime.Now;
                entry.SeqNr = 0;

                //Currency
                CompCurrency baseCurrency = CountryCurrencyManager.GetCompanyBaseCurrency(entities, actorCompanyId);
                if (baseCurrency == null)
                    return false;

                CompCurrency currency = null;
                if (String.IsNullOrEmpty(finvoice.invoiceDetails.VatIncludedAmountCurrencyIdentifier) || baseCurrency.Code == finvoice.invoiceDetails.VatIncludedAmountCurrencyIdentifier)
                {
                    //Use base currency
                    currency = baseCurrency;
                }
                else
                {
                    //Try find foreign currency
                    currency = CountryCurrencyManager.GetCompCurrency(entities, finvoice.invoiceDetails.VatIncludedAmountCurrencyIdentifier, actorCompanyId);
                    if (currency == null)
                    {
                        //Foreign currency not found. Use base currency
                        currency = baseCurrency;
                    }
                }

                entry.CurrencyId = currency.CurrencyId;
                entry.CurrencyRate = currency.BaseCurrency ? 1 : currency.RateToBase;
                entry.CurrencyDate = currency.Date;

                //Amounts
                entry.Sum = finvoice.invoiceDetails.InvoiceTotalVatIncludedAmount;
                entry.SumVat = finvoice.invoiceDetails.InvoiceTotalVatAmount;

                //Set currency amounts
                CountryCurrencyManager.SetCurrencyAmounts(entities, actorCompanyId, entry);

                //Dates
                entry.InvoiceDate = finvoice.invoiceDetails.InvoiceDate;
                entry.DueDate = finvoice.invoiceDetails.InvoiceDueDate ?? finvoice.HeadInvoiceDueDate;

                //Bank
                entry.PostalGiro = finvoice.invoiceDetails.PostalGiro;
                entry.BankGiro = finvoice.invoiceDetails.BankGiro;
                entry.OCR = finvoice.epiDetails.EpiRemittanceInfoIdentifier;
                entry.IBAN = finvoice.epiDetails.EpiAccountID;

                //MessageType                
                entry.MessageType = (int)TermGroup_EdiMessageType.SupplierInvoice;

                #region Invoice

                //InvoiceNr
                entry.InvoiceNr = finvoice.invoiceDetails.InvoiceNumber;

                //Supplier
                string orgNr = finvoice.sellerPartyDetails.SellerPartyIdentifier;
                string orgUnit = finvoice.SellerOrganisationUnitNumber;
                string Iban = finvoice.SellerIban;
                string Bic = finvoice.SellerBic;
                string vatNr = finvoice.sellerPartyDetails.SellerOrganisationTaxCode;
                string paymentNr = Iban;
                if (orgUnit.Length > 0 && orgUnit.StartsWith("0037"))
                    orgUnit = orgUnit.Remove(0, 4);

                if (orgUnit.Length == 8)
                    orgUnit = orgUnit.Substring(0, 7) + "-" + orgUnit.SubstringFromEnd(1);

                entry.Supplier = SupplierManager.GetSupplierForFinvoiceByPrio(entities, entry.ActorCompanyId, orgNr, orgUnit, vatNr, paymentNr);

                //Order status
                entry.OrderStatus = (int)TermGroup_EDIOrderStatus.Unprocessed;

                //ExternalId
                entry.ExternalId = finvoice.MessageTransmissionDetails.MessageIdentifier;

                //Validate                
                SetEdiEntryInvoiceStatus(entry, TermGroup_EDIInvoiceStatus.Unprocessed);

                #endregion

                result = true;

                #endregion
            }
            catch (Exception ex)
            {
                base.LogError(ex, this.log);
                result = false;
            }
            return result;
        }

        private bool SupplierInvoiceExists(CompEntities entities, EdiEntry ediEntry)
        {
            bool invoiceOrEdiExists = false;

            //search from invoices
            var invoice = (from p in entities.Invoice.Include("Origin")
                           where p.InvoiceNr == ediEntry.InvoiceNr &&
                           p.ActorId == ediEntry.ActorSupplierId &&
                           p.InvoiceDate == ediEntry.InvoiceDate &&
                           p.TotalAmount == ediEntry.Sum &&
                           p.Origin.Type == (int)SoeOriginType.SupplierInvoice &&
                           p.State == (int)SoeEntityState.Active
                           select p).FirstOrDefault();

            invoiceOrEdiExists = invoice != null;

            //if not found from invoices, search from ediEntry
            if (!invoiceOrEdiExists)
            {
                var edi = (from e in entities.EdiEntry
                           where e.ActorSupplierId == ediEntry.ActorSupplierId &&
                           e.InvoiceNr == ediEntry.InvoiceNr &&
                           e.InvoiceDate == ediEntry.InvoiceDate &&
                           e.Sum == ediEntry.Sum &&
                           e.Type == ediEntry.Type &&
                           e.State == (int)SoeEntityState.Active
                           select e).FirstOrDefault();

                invoiceOrEdiExists = edi != null;
            }

            return invoiceOrEdiExists;
        }

        public ActionResult CreateSupplierFromFinvoice(int ediEntryId, int actorCompanyId)
        {
            ActionResult result = new ActionResult(true);

            using (CompEntities entities = new CompEntities())
            {

                using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                {

                    int actorId = 0;
                    string errormessage = string.Empty;

                    EdiEntry ediEntry = GetEdiEntry(entities, ediEntryId, actorCompanyId);

                    XDocument xdoc = null;
                    xdoc = XDocument.Parse(XmlUtil.FormatXml(ediEntry.XML));

                    if (xdoc != null)
                    {
                        #region Supplier

                        XElement SellerPartyDetails = XmlUtil.GetChildElement(xdoc, "SellerPartyDetails");

                        // check if supplier with same name exists
                        string supplierName = XmlUtil.GetChildElementValue(SellerPartyDetails, "SellerOrganisationName");
                        List<Supplier> suppliers = SupplierManager.GetSuppliersByCompany(actorCompanyId, true);
                        bool supplierExists = suppliers.Where(i => i.Name == supplierName).ToList().Count > 0;

                        if (supplierExists)
                        {
                            result.Success = false;
                            result.ErrorNumber = (int)ActionResultSave.SupplierNotSaved;
                            result.ErrorMessage = String.Format(GetText(4891, "Leverantör {0} finns redan.\nSkapande av leverantör avbryts."), supplierName);
                            return result;
                        }

                        // Add Supplier
                        Supplier supplier = new Supplier()
                        {
                            ActorCompanyId = actorCompanyId,
                            SupplierNr = SupplierManager.GetNextSupplierNr(actorCompanyId),
                            Name = XmlUtil.GetChildElementValue(SellerPartyDetails, "SellerOrganisationName"),
                            State = (int)SoeEntityState.Active,
                            OrgNr = XmlUtil.GetChildElementValue(SellerPartyDetails, "SellerPartyIdentifier"),
                            VatNr = XmlUtil.GetChildElementValue(SellerPartyDetails, "SellerOrganisationTaxCode"),
                            IsPrivatePerson = false,

                            //FK
                            CurrencyId = ediEntry.CurrencyId,
                        };
                        SetCreatedProperties(supplier);

                        Actor actor = new Actor()
                        {
                            ActorType = (int)SoeActorType.Supplier,

                            //Set references
                            Supplier = supplier,
                        };

                        result = AddEntityItem(entities, actor, "Actor");
                        if (!result.Success)
                        {
                            result.ErrorNumber = (int)ActionResultSave.SupplierNotSaved;
                            result.ErrorMessage = GetText(11009, "Leverantör kunde inte sparas");
                            return result;
                        }

                        actorId = supplier.ActorSupplierId;
                        actor = ActorManager.GetActor(entities, actorId, false);

                        #endregion

                        #region Payment information

                        XElement EpiDetails = XmlUtil.GetChildElement(xdoc, "EpiDetails");
                        XElement EpiPartyDetails = XmlUtil.GetChildElement(EpiDetails, "EpiPartyDetails");
                        XElement EpiBfiPartyDetails = XmlUtil.GetChildElement(EpiPartyDetails, "EpiBfiPartyDetails");
                        XElement EpiBeneficiaryPartyDetails = XmlUtil.GetChildElement(EpiPartyDetails, "EpiBeneficiaryPartyDetails");

                        string bic = EpiBfiPartyDetails != null ? XmlUtil.GetChildElementValue(EpiBfiPartyDetails, "EpiBfiIdentifier") : string.Empty;
                        string iban = EpiBeneficiaryPartyDetails != null ? XmlUtil.GetChildElementValue(EpiBeneficiaryPartyDetails, "EpiAccountID") : string.Empty;

                        PaymentInformation paymentInformation = new PaymentInformation()
                        {
                            Actor = actor,
                            DefaultSysPaymentTypeId = (int)TermGroup_SysPaymentType.BIC,
                        };
                        SetCreatedProperties(paymentInformation);

                        PaymentManager.AddPaymentInformationRowToPaymentInformation(paymentInformation, TermGroup_SysPaymentType.BIC, iban, true, bic);

                        result = SaveChanges(entities, transaction);
                        if (!result.Success)
                            errormessage = GetText(4890, "Betalningsuppgifter kunde inte sparas");

                        #endregion

                        #region Contacts                                                                      

                        Contact contact = new Contact()
                        {
                            Actor = actor,
                            SysContactTypeId = (int)TermGroup_SysContactType.Company,
                        };
                        SetCreatedProperties(contact);

                        result = AddEntityItem(entities, contact, "Contact");
                        if (!result.Success)
                        {
                            result.IntegerValue = actorId;

                            if (errormessage.Length > 0)
                                errormessage += "\n";

                            result.ErrorMessage = errormessage + GetText(11011, "Alla kontakt- och tele/webb-uppgifter kunde inte sparas");

                            return result;
                        }

                        #region Addresses

                        XElement SellerPostalAddressDetails = XmlUtil.GetChildElement(SellerPartyDetails, "SellerPostalAddressDetails");

                        string streetName = XmlUtil.GetChildElementValue(SellerPostalAddressDetails, "SellerStreetName");
                        string postalCode = XmlUtil.GetChildElementValue(SellerPostalAddressDetails, "SellerPostCodeIdentifier");
                        string postalAddress = XmlUtil.GetChildElementValue(SellerPostalAddressDetails, "SellerTownName");
                        string country = XmlUtil.GetChildElementValue(SellerPostalAddressDetails, "CountryName");

                        if (streetName.HasValue() || postalCode.HasValue() || postalAddress.HasValue() || country.HasValue())
                        {
                            ContactAddress contactAddressDistribution = new ContactAddress()
                            {
                                Contact = contact,
                                Name = GetText((int)TermGroup_SysContactAddressType.Distribution, (int)TermGroup.SysContactAddressType),
                                SysContactAddressTypeId = (int)TermGroup_SysContactAddressType.Distribution,
                            };
                            SetCreatedProperties(contactAddressDistribution);

                            ContactManager.AddContactAddressRowToContactAddress(contactAddressDistribution, TermGroup_SysContactAddressRowType.Address, String.Format(streetName));
                            ContactManager.AddContactAddressRowToContactAddress(contactAddressDistribution, TermGroup_SysContactAddressRowType.PostalCode, String.Format(postalCode));
                            ContactManager.AddContactAddressRowToContactAddress(contactAddressDistribution, TermGroup_SysContactAddressRowType.PostalAddress, String.Format(postalAddress));
                            ContactManager.AddContactAddressRowToContactAddress(contactAddressDistribution, TermGroup_SysContactAddressRowType.Country, String.Format(country));
                        }
                        #endregion

                        #region Ecom

                        XElement SellerCommunicationDetails = XmlUtil.GetChildElement(xdoc, "SellerCommunicationDetails");
                        XElement SellerInformationDetails = XmlUtil.GetChildElement(xdoc, "SellerInformationDetails");

                        string phoneJob = XmlUtil.GetChildElementValue(SellerInformationDetails, "SellerPhoneNumber");
                        string fax = XmlUtil.GetChildElementValue(SellerInformationDetails, "SellerFaxNumber");
                        string email = XmlUtil.GetChildElementValue(SellerCommunicationDetails, "SellerEmailaddressIdentifier");
                        string web = XmlUtil.GetChildElementValue(SellerInformationDetails, "SellerWebaddressIdentifier");

                        if (phoneJob.HasValue())
                        {
                            ContactECom contactEComPhoneJob = new ContactECom()
                            {
                                Contact = contact,
                                Name = GetText((int)TermGroup_SysContactEComType.PhoneJob, (int)TermGroup.SysContactEComType),
                                SysContactEComTypeId = (int)TermGroup_SysContactEComType.PhoneJob,
                                Text = phoneJob,
                            };
                            SetCreatedProperties(contactEComPhoneJob);
                        }

                        if (fax.HasValue())
                        {
                            ContactECom contactEComFax = new ContactECom()
                            {
                                Contact = contact,
                                Name = GetText((int)TermGroup_SysContactEComType.Fax, (int)TermGroup.SysContactEComType),
                                SysContactEComTypeId = (int)TermGroup_SysContactEComType.Fax,
                                Text = fax,
                            };
                            SetCreatedProperties(contactEComFax);
                        }

                        if (email.HasValue())
                        {
                            ContactECom contactEComEmail = new ContactECom()
                            {
                                Contact = contact,
                                Name = GetText((int)TermGroup_SysContactEComType.Email, (int)TermGroup.SysContactEComType),
                                SysContactEComTypeId = (int)TermGroup_SysContactEComType.Email,
                                Text = email,
                            };
                            SetCreatedProperties(contactEComEmail);
                        }

                        if (web.HasValue())
                        {
                            ContactECom contactEComEmail = new ContactECom()
                            {
                                Contact = contact,
                                Name = GetText((int)TermGroup_SysContactEComType.Web, (int)TermGroup.SysContactEComType),
                                SysContactEComTypeId = (int)TermGroup_SysContactEComType.Web,
                                Text = web,
                            };
                            SetCreatedProperties(contactEComEmail);
                        }

                        result = SaveChanges(entities, transaction);
                        if (!result.Success)
                        {
                            if (errormessage.Length > 0)
                                errormessage += "\n";

                            errormessage += GetText(11011, "Alla kontakt- och tele/webb-uppgifter kunde inte sparas");
                        }

                        #endregion

                        #endregion                        

                    }

                    //save supplierId to edientry
                    if (actorId > 0)
                    {
                        ediEntry.ActorSupplierId = actorId;
                        ediEntry.ErrorMessage = null;
                        ediEntry.InvoiceStatus = (int)TermGroup_EDIInvoiceStatus.Unprocessed;
                        ediEntry.OrderStatus = (int)TermGroup_EDIOrderStatus.Unprocessed;
                        result = SaveChanges(entities, transaction);
                    }

                    //Commit transaction
                    if (result.Success)
                        transaction.Complete();

                    result.ErrorMessage = errormessage;
                    result.IntegerValue = actorId;
                }

            }

            return result;
        }

        #endregion

        #region Image

        public ImageViewerItemDTO GetInvoiceImage(int ediEntryId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.EdiEntry.NoTracking();
            return GetInvoiceImage(entities, ediEntryId);
        }

        public ImageViewerItemDTO GetInvoiceImage(CompEntities entities, int ediEntryId)
        {
            Dictionary<int, byte[]> pages;

            try
            {
                EdiEntry ediEntry = GetScanningEntry(entities, ediEntryId, false);
                if (ediEntry == null || ediEntry.ScanningEntryInvoice == null)
                    return null;

                byte[] tiffImage = ediEntry.ScanningEntryInvoice.Image;
                MemoryStream tiffMemStream = new MemoryStream(tiffImage);
                tiffMemStream.Position = 0;

                pages = GetPngPagesFromImage(tiffMemStream);

                ImageViewerItemDTO imageViewerItem = new ImageViewerItemDTO
                {
                    NrOfPages = pages.Count,
                    pages = pages,
                    ScanningEntryId = ediEntry.ScanningEntryInvoice.ScanningEntryId,
                };

                return imageViewerItem;
            }
            catch (Exception exp)
            {
                base.LogError(exp, this.log);
                return null;
            }
        }

        public GenericImageDTO GetInvoiceImageAndAttachments(CompEntities entities, int ediEntryId, int actorCompanyId, bool includeInvoiceAttachment = false, bool loadAll = false)
        {
            try
            {
                EdiEntry ediEntry = GetScanningEntry(entities, ediEntryId, includeInvoiceAttachment);
                if (ediEntry == null || ediEntry.ScanningEntryInvoice == null)
                    return null;

                byte[] tiffImage = ediEntry.ScanningEntryInvoice.Image;
                MemoryStream tiffMemStream = new MemoryStream(tiffImage);
                tiffMemStream.Position = 0;

                Dictionary<int, byte[]> pages = GetPngPagesFromImage(tiffMemStream);

                if (pages.Count > 0 && loadAll)
                {
                    var imageDTO = new GenericImageDTO() { Id = ediEntry.EdiEntryId, Image = pages.Values.First(), Description = ediEntry.FileName, ImageFormatType = SoeDataStorageRecordType.InvoiceBitmap, Images = new List<byte[]>(), SourceType = InvoiceAttachmentSourceType.Edi, InvoiceAttachments = ediEntry.InvoiceAttachment.Count > 0 ? ediEntry.InvoiceAttachment.ToDTOs().ToList() : new List<InvoiceAttachmentDTO>() };
                    foreach (var array in pages.Values)
                    {
                        imageDTO.Images.Add(array);
                    }
                    return imageDTO;
                }
                else
                    return new GenericImageDTO() { Id = ediEntry.EdiEntryId, Image = pages.Values.First(), Description = ediEntry.FileName, ImageFormatType = SoeDataStorageRecordType.InvoiceBitmap, SourceType = InvoiceAttachmentSourceType.Edi, InvoiceAttachments = ediEntry.InvoiceAttachment.Count > 0 ? ediEntry.InvoiceAttachment.ToDTOs().ToList() : new List<InvoiceAttachmentDTO>() };
            }
            catch (Exception exp)
            {
                base.LogError(exp, this.log);
                return null;
            }
        }

        public bool HasInvoiceImageAsPDF(CompEntities entities, int ediEntryId, int actorCompanyId, bool includeInvoiceAttachment = false)
        {
            try
            {
                string destinationFileName = ConfigSettings.SOE_SERVER_DIR_TEMP_REPORT_PHYSICAL + Guid.NewGuid().ToString() + Constants.SOE_SERVER_FILE_PDF_SUFFIX;

                EdiEntry ediEntry = GetScanningEntry(entities, ediEntryId, includeInvoiceAttachment: includeInvoiceAttachment);
                if (ediEntry == null || ediEntry.ScanningEntryInvoice == null)
                    return false;

                return ediEntry.ScanningEntryInvoice.Image.Length > 0;
            }
            catch
            {
                return false;
            }
        }

        public GenericImageDTO GetInvoiceImageAsPDF(CompEntities entities, int ediEntryId, int actorCompanyId, bool includeInvoiceAttachment = false)
        {
            try
            {
                string destinationFileName = ConfigSettings.SOE_SERVER_DIR_TEMP_REPORT_PHYSICAL + Guid.NewGuid().ToString() + Constants.SOE_SERVER_FILE_PDF_SUFFIX;

                EdiEntry ediEntry = GetScanningEntry(entities, ediEntryId, includeInvoiceAttachment: includeInvoiceAttachment);
                if (ediEntry == null || ediEntry.ScanningEntryInvoice == null)
                    return null;

                byte[] tiffImage = ediEntry.ScanningEntryInvoice.Image;

                if (tiffImage == null)
                    return null;


                byte[] pdf = PDFUtility.CreatePdfFromTif(tiffImage, destinationFileName, true);

#if DEBUG
                //File.WriteAllBytes(@"C:\Temp\invoice\"+ ediEntryId.ToString() + ".tif", tiffImage);
                //File.WriteAllBytes(@"C:\Temp\invoice\" + ediEntryId.ToString() + ".pdf", pdf);
#endif



                if (pdf == null)
                    return null;

                return new GenericImageDTO() { Id = ediEntry.EdiEntryId, Image = pdf, Description = ediEntry.FileName, ImageFormatType = SoeDataStorageRecordType.InvoicePdf, InvoiceAttachments = ediEntry.InvoiceAttachment.Count > 0 ? ediEntry.InvoiceAttachment.ToDTOs().ToList() : new List<InvoiceAttachmentDTO>(), SourceType = InvoiceAttachmentSourceType.Edi };

            }
            catch
            {
                return null;
            }
        }

        public GenericImageDTO GetEdiInvoiceImageFromDataStorage(int actorCompanyId, int ediEntryId)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            var dataStorageRecord = GeneralManager.GetDataStorageRecord(entitiesReadOnly, actorCompanyId, ediEntryId, SoeDataStorageRecordType.EdiEntry_Document);
            var dataStorage = GeneralManager.GetDataStorage(entitiesReadOnly, dataStorageRecord.DataStorageId, actorCompanyId,
                includeDataStorageRecipients: false,
                includeDataStorageRecord: false);

            return new GenericImageDTO { Image = dataStorage.Data, ImageFormatType = (SoeDataStorageRecordType)dataStorage.Type, Filename = dataStorage.FileName };
        }
        public GenericImageDTO GetInvoiceImageFromEdi(int ediEntryId, int actorCompanyId)
        {
            GenericImageDTO imageDTO = null;

            try
            {
                EdiEntry ediEntry = GetScanningEntry(ediEntryId);


                if (ediEntry.UsesDataStorage)
                    return GetEdiInvoiceImageFromDataStorage(actorCompanyId, ediEntry.EdiEntryId);

                if (ediEntry == null)
                    return null;

                if (ediEntry.PDF != null)
                    imageDTO = new GenericImageDTO() { Image = ediEntry.PDF, ImageFormatType = SoeDataStorageRecordType.InvoicePdf };

                if (imageDTO == null)
                {
                    var image = EdiManager.GetInvoiceImage(ediEntry.EdiEntryId);
                    if (image != null)
                    {
                        if (image.NrOfPages > 0)
                        {
                            imageDTO = new GenericImageDTO() { Image = image.pages.Values.First(), ImageFormatType = SoeDataStorageRecordType.InvoiceBitmap, Images = new List<byte[]>() };
                            foreach (var array in image.pages.Values)
                            {
                                imageDTO.Images.Add(array);
                            }
                        }
                        else
                            imageDTO = new GenericImageDTO() { Image = image.pages.Values.First(), ImageFormatType = SoeDataStorageRecordType.InvoiceBitmap };
                    }
                }

                return imageDTO;
            }
            catch (Exception exp)
            {
                base.LogError(exp, this.log);
                return null;
            }
        }

        public Dictionary<int, byte[]> GetPngPagesFromImage(MemoryStream tiffMemStream)
        {
            Dictionary<int, byte[]> pages = new Dictionary<int, byte[]>();

            using (Image image = Image.FromStream(tiffMemStream))
            {
                int numberOfPages = image.GetFrameCount(FrameDimension.Page);
                MemoryStream pngMemStream = new MemoryStream();

                for (int pageIndex = 0; pageIndex < numberOfPages; pageIndex++)
                {
                    // Choose page to work with
                    image.SelectActiveFrame(FrameDimension.Page, pageIndex);
                    //..save the page to returnStream 
                    image.Save(pngMemStream, System.Drawing.Imaging.ImageFormat.Png);

                    pngMemStream.Position = 0;
                    pages.Add(pageIndex, pngMemStream.ToArray());
                }
            }

            return pages;
        }

        public Dictionary<int, byte[]> GetPngPagesFromImage2(MemoryStream tiffMemStream)
        {
            Dictionary<int, byte[]> pages = new Dictionary<int, byte[]>();

            MemoryStream ms = new MemoryStream();
            TiffStream stm = new TiffStream();

            using (Tiff tiff = Tiff.ClientOpen("", "w", ms, stm))
            {
                short numberOfPages = tiff.NumberOfDirectories();

            }

            return pages;
        }

        public ActionResult CreateFinvoiceImage(int ediEntryId, int actorCompanyId)
        {
            using (var entities = new CompEntities())
            {
                var entry = GetEdiEntry(entities, ediEntryId, actorCompanyId);

                try
                {
                    var pdf = FInvoiceFileGen.GetPdf(entry);
                    if (pdf != null)
                    {
                        entry.PDF = pdf;
                        entry.ImageStorageType = (int)SoeInvoiceImageStorageType.StoredInEdiEntry;
                    }
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    return new ActionResult(ex.Message);
                }

                return SaveChanges(entities);
            }
        }

        #endregion

        #region Settings

        #region CloseEdiEntryCondition

        private bool CloseEdiConditionIsOrder(CompEntities entities, int actorCompanyId)
        {
            return (GetCloseEdiCondition(entities, actorCompanyId) == TermGroup_CloseEdiEntryCondition.WhenTransferedToOrder);
        }

        private bool CloseEdiConditionIsSupplierInvoice(CompEntities entities, int actorCompanyId)
        {
            return (GetCloseEdiCondition(entities, actorCompanyId) == TermGroup_CloseEdiEntryCondition.WhenTransferedToSupplierInvoice);
        }

        private bool CloseEdiConditionIsOrderAndSupplierInvoice(CompEntities entities, int actorCompanyId)
        {
            return (GetCloseEdiCondition(entities, actorCompanyId) == TermGroup_CloseEdiEntryCondition.WhenTransferedToOrderAndSupplierInvoice);
        }

        private bool CloseEdiConditionIsOrderOrSupplierInvoice(CompEntities entities, int actorCompanyId)
        {
            return (GetCloseEdiCondition(entities, actorCompanyId) == TermGroup_CloseEdiEntryCondition.WhenTransferedToOrderOrSupplierInvoice);
        }

        private TermGroup_CloseEdiEntryCondition GetCloseEdiCondition(CompEntities entities, int actorCompanyId)
        {
            TermGroup_CloseEdiEntryCondition closeCondition = (TermGroup_CloseEdiEntryCondition)SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.BillingCloseEdiEntryCondition, 0, actorCompanyId, 0);

            return closeCondition;
        }

        public bool CloseScanningWhenTransferedToSupplierInvoice(CompEntities entities, int actorCompanyId)
        {
            return SettingManager.GetBoolSetting(entities, SettingMainType.Company, (int)CompanySettingType.ScanningCloseWhenTransferedToSupplierInvoice, 0, actorCompanyId, 0);
        }

        #endregion

        #region EDIPriceSettingRule

        private bool EdiPriceSettingRuleIsUsePriceRules(CompEntities entities, int actorCompanyId)
        {
            return (GetEdiPriceSettingRule(entities, actorCompanyId) == TermGroup_EDIPriceSettingRule.UsePriceRules);
        }

        private bool EdiPriceSettingRuleIsUsePriceRulesKeepEDIPurchasePrice(CompEntities entities, int actorCompanyId)
        {
            return (GetEdiPriceSettingRule(entities, actorCompanyId) == TermGroup_EDIPriceSettingRule.UsePriceRulesKeepEDIPurchasePrice);
        }

        private bool EdiPriceSettingRuleIsUsePriceRuleAndPurchasePriceFromSysWholeseller(CompEntities entities, int actorCompanyId)
        {
            return (GetEdiPriceSettingRule(entities, actorCompanyId) == TermGroup_EDIPriceSettingRule.UsePriceRulesAndPurchasePriceFromPriceList);
        }

        private TermGroup_EDIPriceSettingRule GetEdiPriceSettingRule(CompEntities entities, int actorCompanyId)
        {
            TermGroup_EDIPriceSettingRule setting = (TermGroup_EDIPriceSettingRule)SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.BillingEDIPriceSettingRule, 0, actorCompanyId, 0);

            return setting;
        }

        #endregion

        #endregion

        #region AzoraOne
        public ActionResult ActivateAzoraOneWithoutAlternative(int actorCompanyId, bool doSyncSuppliers)
        {
            return ActivateAzoraOneIntegration(actorCompanyId, AzoraOneStatus.ActivatedWithoutAlternative, doSyncSuppliers);
        }

        public ActionResult ActivateAzoraOneWithAlternative(int actorCompanyId, bool doSyncSuppliers)
        {
            return ActivateAzoraOneIntegration(actorCompanyId, AzoraOneStatus.ActivatedWithAlternative, doSyncSuppliers);
        }
        public ActionResult ActivateAzoraOneInBackground(int actorCompanyId, bool doSyncSuppliers)
        {
            return ActivateAzoraOneIntegration(actorCompanyId, AzoraOneStatus.ActivatedInBackground, doSyncSuppliers);
        }

        private ActionResult ActivateAzoraOneIntegration(int actorCompanyId, AzoraOneStatus status, bool doSyncSuppliers = true)
        {
            var company = CompanyManager.GetCompany(actorCompanyId, loadLicense: true, loadEdiConnection: false, loadActorAndContact: true);

            if (company == null || company.CompanyGuid == Guid.Empty)
                return new ActionResult((int)ActionResultSave.NothingSaved);


            var azoraOneManager = new AzoraOneManager(company.CompanyGuid);
            var result = azoraOneManager.SaveCompany(company.ToCompanyDTO());

            if (result.Success)
                SettingManager.UpdateInsertIntSetting(SettingMainType.Company, (int)CompanySettingType.ScanningUsesAzoraOne, (int)status, 0, actorCompanyId, 0);

            //In a separate thread, start syncing all suppliers.
            if (doSyncSuppliers)
                Task.Run(() => CompEntitiesProvider.RunWithTaskScopedReadOnlyEntities(() => SyncAllSuppliersWithAzoraOne(actorCompanyId)));
            return result;
        }

        public ActionResult DeactivateAzoraOneIntegration(int actorCompanyId)
        {

            /* Deactivated currently means activated in the background.
             * It's because I want the sync to be active at all times for these companies,
             * to prevent other problems that could come by users turning it off without knowing what it is.
             * In the future we will change this to actually turn it off. 
             */
            Company company = CompanyManager.GetCompany(actorCompanyId, loadLicense: true, loadEdiConnection: false, loadActorAndContact: false);
            if (company == null)
                return new ActionResult((int)ActionResultSave.NothingSaved);

            //var azoraOneManager = new AzoraOneManager(company.CompanyGuid);
            //return azoraOneManager.DeactivateCompany();

            return SettingManager.UpdateInsertIntSetting(SettingMainType.Company, (int)CompanySettingType.ScanningUsesAzoraOne, (int)AzoraOneStatus.ActivatedInBackground, 0, actorCompanyId, 0);
        }

        public ActionResult SyncAllSuppliersWithAzoraOne(int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            var suppliers = SupplierManager.GetSuppliers(entities, actorCompanyId,
                loadActor: true,
                loadAccount: false,
                loadContactAddresses: false,
                loadCategories: false,
                loadPaymentInformation: true,
                loadTemplateAttestHead: false)
                .ToDistributionDTOs();

            string apiKey = CompanyManager.GetCompanyGuid(actorCompanyId);
            var azoraOneManager = new AzoraOneManager(apiKey);

            var result = azoraOneManager.SyncSuppliers(suppliers);
            if (!result.Success)
                base.LogWarning($"AzoraOne: Failed to sync suppliers. ActorCompanyId: {actorCompanyId}, Errors: \n{result.ErrorMessage}");

            return result;
        }

        private bool CompanyUsesAzoraOne(Guid companyGuid)
        {
            var client = new AzoraOneManager(companyGuid);
            return client.CompanyExists();
        }

        public ActionResult SyncSupplierWithAzoraOne(int actorCompanyId, Supplier supplierIn)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            var supplier = supplierIn.ToDistributionDTO();

            string apiKey = CompanyManager.GetCompanyGuid(actorCompanyId);
            var azoraOneManager = new AzoraOneManager(apiKey);

            var result = azoraOneManager.SyncSupplier(supplier);
            if (!result.Success)
                base.LogWarning($"AzoraOne: Failed to sync supplier. ActorCompanyId: {actorCompanyId}, Errors: \n{result.ErrorMessage}");

            return result;
        }
        private ActionResult CreateScanningEntryFromDocUpload(int actorCompanyId, int dataStorageId, out ScanningEntryWrapper value)
        {
            using (var entites = new CompEntities())
            {
                var batchId = Guid.NewGuid();
                value = null;
                var result = CreateScanningEntriesFromDocUpload(entites, actorCompanyId, new List<int> { dataStorageId }, batchId, out List<ScanningEntryWrapper> values);

                if (!result.Success)
                    return result;

                value = values.FirstOrDefault();
                return result;
            }
        }
        public ActionResult SendDocumentsForScanning(int actorCompanyId, List<int> dataStorageIds, bool usePolling)
        {
            using (var entities = new CompEntities())
            {
                var batchId = Guid.NewGuid();
                return SendDocumentsForScanning(entities, actorCompanyId, batchId, dataStorageIds, usePolling);
            }
        }

        public ActionResult SendDocumentsForScanning(CompEntities entities, int actorCompanyId, Guid batchId, List<int> dataStorageIds, bool usePolling)
        {
            var company = CompanyManager.GetCompany(entities, actorCompanyId, loadEdiConnection: false, loadActorAndContact: false);

            if (company == null || !CompanyUsesAzoraOne(company.CompanyGuid))
                return new ActionResult((int)ActionResultSave.ScanningFailed_NotActivatedAtProvider, $"Company {actorCompanyId} is not activated in AzoraOne");

            var result = CreateScanningEntriesFromDocUpload(entities, actorCompanyId, dataStorageIds, batchId, out List<ScanningEntryWrapper> values);
            if (!result.Success)
            {
                base.LogError($"AzoraOne: SendDocumentsForScanning, batchId: {batchId}, error: {result.ErrorMessage}");
                return result;
            }

            return CreateEdiEntries(entities, company, values, usePolling);
        }

        public ActionResult CreateEdiEntries(CompEntities entities, Company company, List<ScanningEntryWrapper> entries, bool usePolling)
        {
            var aoFileUploader = new AzoraOneDocumentUploader(company.CompanyGuid);

            //TODO: Handling if this step fails?
            UploadDocumentsToAzoraOne(entities, company.ActorCompanyId, aoFileUploader, entries);

            var expectedCount = entries.Count;
            var errors = new List<string>();
            if (usePolling)
            {
                foreach (var (scanningEntryId, aoResponse) in aoFileUploader.GetUploadedInvoice())
                {
                    var handleResponse = HandleAzoraOneInvoiceResponse(entities, company, scanningEntryId, aoResponse);
                    if (!aoResponse.IsSuccess || !handleResponse.Success)
                        errors.Add($"AzoraOne: ScanningEntryId: {scanningEntryId}, AoResponse: {aoResponse.ToActionResult().ErrorMessage}, HandleResponse: {handleResponse.ErrorMessage}");
                }
                foreach (var (scanningEntryId, errorMessage) in aoFileUploader.GetFailedEntryIds())
                {
                    var handleResponse = HandleAzoraOneInvoiceResponseError(entities, company, scanningEntryId, errorMessage);
                    if (!handleResponse.Success)
                        errors.Add($"AzoraOne: ScanningEntryId: {scanningEntryId}, ErrorMessage: {errorMessage}, HandleResponse: {handleResponse.ErrorMessage}");
                }
            }
            if (errors.Count() > 0)
            {
                var interpretationResult = new ActionResult($"AzraOne: Failed to upload {errors.Count()} documents, reason:\n {errors.JoinToString("\n")}");
                var errorType = errors.Count() == expectedCount ?
                    ActionResultSave.ScanningFailed_ExtractInterpretationAll :
                    ActionResultSave.ScanningFailed_ExtractInterpretationPartial;
                interpretationResult.ErrorNumber = (int)errorType;

                base.LogError($"AzoraOne: UploadDocumentToAzoraOne, error: {interpretationResult.ErrorMessage}");
                return interpretationResult;
            }
            return new ActionResult();
        }

        private ActionResult CreateScanningEntriesFromDocUpload(CompEntities entities, int actorCompanyId, List<int> dataStorageIds, Guid batchId, out List<ScanningEntryWrapper> values)
        {
            // Create scanning entry
            // Create link to data storage
            // Request interpretation
            // return
            var result = new ActionResult();
            var records = new List<ScanningEntryWrapper>();
            values = null;
            try
            {
                entities.Connection.Open();

                using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                {
                    var dataStorageToScanningEntry = new Dictionary<int, ScanningEntry>();
                    foreach (var dataStorageId in dataStorageIds)
                    {
                        dataStorageToScanningEntry.Add(dataStorageId, CreateScanningEntry(entities, actorCompanyId, batchId));
                    }

                    result = SaveChanges(entities);
                    if (!result.Success)
                        throw new Exception($"batchId: {batchId}, error: {result.ErrorMessage}");

                    foreach (var dataStorageId in dataStorageIds)
                    {
                        var dataStorage = GeneralManager.GetDataStorage(entities, dataStorageId, actorCompanyId);
                        var scanningEntry = dataStorageToScanningEntry[dataStorageId];
                        var dsRecord = CreateDocumentDataStorageRecord(entities, scanningEntry, dataStorage);
                        records.Add(new ScanningEntryWrapper(scanningEntry, dataStorage));
                    }

                    result = SaveChanges(entities);
                    if (!result.Success)
                        throw new Exception($"batchId: {batchId}, error: {result.ErrorMessage}");

                    transaction.Complete();
                    values = records;
                }
            }
            catch (Exception ex)
            {
                base.LogError(ex, this.log);
                result.Exception = ex;
                result.ErrorNumber = (int)ActionResultSave.ScanningFailed_CreateScanningEntry;
            }
            finally
            {
                if (!result.Success)
                    base.LogTransactionFailed($"AzoraOne: {this}", this.log);
            }

            return result;
        }

        private ActionResult UploadDocumentsToAzoraOne(CompEntities entities, int actorCompanyId, AzoraOneDocumentUploader docUploader, List<ScanningEntryWrapper> documents)
        {
            var result = new ActionResult();
            try
            {
                var company = CompanyManager.GetCompany(entities, actorCompanyId, loadEdiConnection: false, loadActorAndContact: false);
                var siteType = SettingManager.SiteType;
                var compDbId = SysServiceManager.GetSysCompDBId();
                var azoraOneManager = new AzoraOneManager(company.CompanyGuid);

                foreach (var doc in documents)
                {
                    long maxLimitInBytes = 6 * 1024 * 1024;
                    byte[] docData = doc.GetData();
                    if (docData.Length > maxLimitInBytes)
                    {
                        List<int> pageIndices = [0, -1];
                        PdfResponse extractedPdf = PdfConvertConnector.ExtractPages(docData, doc.GetFileName(), pageIndices);
                        if (extractedPdf.PdfData != null) docData = extractedPdf.PdfData;
                    }
                    var uploadResult = docUploader.UploadDocument(
                        entityPK: doc.ScanningEntry.ScanningEntryId,
                        fileId: doc.GetDocumentId(),
                        fileName: doc.GetFileName(),
                        fileContent: docData,
                        webhookUrl: !docUploader.UsesPolling() ? doc.GetCallbackUrl(siteType, compDbId) : null);

                    if (!uploadResult.Success)
                    {
                        var scanningEntry = doc.ScanningEntry;
                        scanningEntry.OperatorMessage = uploadResult.ErrorMessage;
                        scanningEntry.Status = (int)TermGroup_ScanningStatus.Error;
                        scanningEntry.ReceiveTime = DateTime.Now;
                        result = uploadResult;
                    }
                }
                result = SaveChanges(entities);
            }
            catch (Exception ex)
            {
                base.LogError(ex, this.log);
                result.Exception = ex;
                result.ErrorNumber = (int)ActionResultSave.ScanningFailed_UploadToProvider;
            }
            finally
            {
                if (!result.Success)
                    base.LogTransactionFailed(this.ToString(), this.log);
            }

            return result;
        }

        public ActionResult AzoraOneDocumentReadyForExtraction(CompEntities entities, int actorCompanyId, int scanningEntryId, AzoraOneFileDTO file)
        {
            var result = new ActionResult();
            try
            {
                var company = CompanyManager.GetCompany(entities, actorCompanyId);
                var aoManager = new AzoraOneManager(company.CompanyGuid);
                var aoInvoice = aoManager.ExtractInvoice(file.FileID);
                result = HandleAzoraOneInvoiceResponse(entities, company, scanningEntryId, aoInvoice);
            }
            catch (Exception ex)
            {
                base.LogError(ex, this.log);
                result.Exception = ex;
                result.ErrorNumber = (int)ActionResultSave.ScanningFailed_HandleProviderResponse;
            }
            finally
            {
                if (!result.Success)
                    base.LogTransactionFailed(this.ToString(), this.log);
            }
            return result;
        }

        private ActionResult HandleAzoraOneInvoiceResponse(CompEntities entities, Company company, string documentId, AOResponseWrapper<AOSupplierInvoice> aoResponse)
        {
            var scanningEntry = GetScanningEntryByDocumentId(entities, company.ActorCompanyId, documentId);

            if (scanningEntry == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, $"AzoraOne: Could not find ScanningEntry, documentId: {documentId}");

            return HandleAzoraOneInvoiceResponse(entities, company, scanningEntry, aoResponse);
        }

        public ActionResult HandleAzoraOneInvoiceResponse(CompEntities entities, Company company, int scanningEntryId, AOResponseWrapper<AOSupplierInvoice> aoResponse)
        {
            var scanningEntry = GetScanningEntry(entities, scanningEntryId, company.ActorCompanyId);

            if (scanningEntry == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, $"AzoraOne: Could not find ScanningEntry, scanningEntryId: {scanningEntryId}");

            return HandleAzoraOneInvoiceResponse(entities, company, scanningEntry, aoResponse);
        }

        public ActionResult HandleAzoraOneInvoiceResponseError(CompEntities entities, Company company, int scanningEntryId, ActionResult result)
        {
            var scanningEntry = GetScanningEntry(entities, scanningEntryId, company.ActorCompanyId);

            if (scanningEntry == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, $"AzoraOne: Could not find ScanningEntry, scanningEntryId: {scanningEntryId}");

            return HandleAzoraOneInvoiceResponseError(entities, company, scanningEntry, result);
        }

        private ActionResult HandleAzoraOneInvoiceResponseError(CompEntities entities, Company company, ScanningEntry scanningEntry, ActionResult extractionResult)
        {
            var result = new ActionResult();
            try
            {
                using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                {

                    var ediEntry = CreateEdiEntryError(entities, company, scanningEntry, extractionResult);
                    result = SaveChanges(entities);

                    if (result.Success)
                    {
                        AddDataStorageFromScanning(entities, scanningEntry, ediEntry);
                        result = SaveChanges(entities);
                    }

                    if (result.Success)
                        transaction.Complete();
                }
            }
            catch (Exception ex)
            {
                base.LogError(ex, this.log);
                result.Exception = ex;
                result.ErrorNumber = (int)ActionResultSave.ScanningFailed_HandleProviderResponse;
            }
            finally
            {
                if (!result.Success)
                    base.LogTransactionFailed(this.ToString(), this.log);
            }
            return result;
        }
        private ActionResult HandleAzoraOneInvoiceResponse(CompEntities entities, Company company, ScanningEntry scanningEntry, AOResponseWrapper<AOSupplierInvoice> aoResponse)
        {
            var result = new ActionResult();
            try
            {
                using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                {
                    string content = aoResponse.RawResponse;

                    var createdDataStorage = CreateDataStorageForScanning(entities, company.ActorCompanyId,
                        scanningEntry: scanningEntry,
                        type: SoeDataStorageRecordType.ScanningEntry_RawData,
                        content: Encoding.UTF8.GetBytes(content));

                    var ediEntry = aoResponse.IsSuccess ?
                        CreateEdiEntry(entities, company, scanningEntry, aoResponse.Response, aoResponse.RawResponse) :
                        CreateEdiEntryError(entities, company, scanningEntry, aoResponse.ToActionResult());

                    result = SaveChanges(entities);

                    if (result.Success)
                    {
                        AddDataStorageFromScanning(entities, scanningEntry, ediEntry);
                        result = SaveChanges(entities);
                    }

                    if (result.Success)
                        transaction.Complete();
                }
            }
            catch (Exception ex)
            {
                base.LogError(ex, this.log);
                result.Exception = ex;
                result.ErrorNumber = (int)ActionResultSave.ScanningFailed_HandleProviderResponse;
            }
            finally
            {
                if (!result.Success)
                    base.LogTransactionFailed(this.ToString(), this.log);
            }
            return result;
        }

        private ActionResult CreateDataStorageForScanning(CompEntities entities, int actorCompanyId, ScanningEntry scanningEntry, SoeDataStorageRecordType type, byte[] content)
        {
            if (scanningEntry == null || scanningEntry.ScanningEntryId == 0)
                throw new ArgumentException("AzoraOne: Scanning entry is not valid");

            var dataStorageRecord = new DataStorageRecordDTO
            {
                RecordId = scanningEntry.ScanningEntryId,
                Entity = SoeEntityType.ScanningEntry,
                Data = content,
                Type = type
            };
            return GeneralManager.SaveDataStorageRecord(entities, actorCompanyId, dataStorageRecord, deleteExisting: false);
        }

        private SupplierInvoiceInterpretationDTO SupplierInvoiceInterpretationFromAzoraOne(CompEntities entities, byte[] jsonData)
        {
            var (invoice, rawResponse) = AzoraOneHelper.InvoiceFromByteArray(jsonData);
            return invoice.ToSupplierInvoiceInterpretationDTO(rawResponse);
        }


        private SupplierInvoiceInterpretationDTO ExtendSupplierInvoiceInterpretationFromAzoraOne(SupplierInvoiceInterpretationDTO dto, EdiEntry ediEntry)
        {
            //Set values dependant on calculations, e.g. specific due date or currency amounts.
            SetContextData(dto, ediEntry);
            SetAmounts(dto, ediEntry);
            SetDueDate(dto, ediEntry);
            return dto;
        }

        private SupplierInvoiceInterpretationDTO SupplierInvoiceInterpretationFromScanningRows(CompEntities entities, Company company, EdiEntry ediEntry)
        {
            var initial = ediEntry.ToSupplierInterpretationDTO();
            SetContextData(initial, ediEntry);
            SetDueDate(initial, ediEntry);
            return initial;
        }

        private void SetContextData(SupplierInvoiceInterpretationDTO dto, EdiEntry entry)
        {
            dto.Context.EdiEntryId = entry.EdiEntryId;
            dto.Context.ScanningEntryId = entry.ScanningEntryInvoiceId;
        }

        private void SetAmounts(SupplierInvoiceInterpretationDTO dto, EdiEntry ediEntry)
        {
            dto.CurrencyId = InterpretationValueFactory.InterpretedInt(ediEntry.CurrencyId, dto.CurrencyCode.ConfidenceLevel);
            dto.CurrencyDate = InterpretationValueFactory.DerivedDate(ediEntry.CurrencyDate);
            dto.CurrencyRate = InterpretationValueFactory.DerivedDecimal(ediEntry.CurrencyRate);
            dto.AmountExVat = InterpretationValueFactory.DerivedDecimal(ediEntry.Sum - ediEntry.SumVat);
            dto.AmountIncVat = InterpretationValueFactory.InterpretedDecimal(ediEntry.Sum, dto.AmountIncVatCurrency.ConfidenceLevel);
            dto.VatAmount = InterpretationValueFactory.InterpretedDecimal(ediEntry.SumVat, dto.VatAmountCurrency.ConfidenceLevel);
        }

        private void SetDueDate(SupplierInvoiceInterpretationDTO dto, EdiEntry ediEntry)
        {
            //EDIEntry carries the due date since it has been calculated (if necessary) on arrival.
            var current = dto.DueDate.Value;
            var edi = ediEntry.DueDate;

            if (edi == current)
                return;

            dto.DueDate = InterpretationValueFactory.InterpretedDate(ediEntry.DueDate, TermGroup_ScanningInterpretation.ValueIsBusinessRuleDerived);
        }

        private void SetDueDateOnEdiEntry(CompEntities entities, EdiEntry ediEntry, Company company)
        {
            bool setDueDateFromSetting = SettingManager.GetBoolSetting(entities, SettingMainType.Company, (int)CompanySettingType.ScanningCalcDueDateFromSupplier, 0, company.ActorCompanyId, 0);
            if (!setDueDateFromSetting)
                return;

            Supplier supplier = LoadSupplier(entities, ediEntry.ActorSupplierId, company.ActorCompanyId);

            if (supplier != null && setDueDateFromSetting && ediEntry.InvoiceDate.HasValue)
            {
                var paymentConditionId = supplier.PaymentConditionId ??
                    SettingManager.GetCompanyIntSetting(entities, CompanySettingType.SupplierPaymentDefaultPaymentCondition);
                if (paymentConditionId > 0)
                {
                    var addDays = PaymentManager.GetPaymentConditionDays(entities, paymentConditionId, company.ActorCompanyId);
                    var dueDate = ((DateTime)ediEntry.InvoiceDate.Value).AddDays(addDays);
                    ediEntry.DueDate = dueDate;
                }
            }
        }

        private void AddDataStorageFromScanning(CompEntities entities, ScanningEntry scanning, EdiEntry edi)
        {
            if (!scanning.UsesDataStorage)
                return;

            var dataDSRecord = GeneralManager.GetDataStorageRecord(entities, scanning.ActorCompanyId, scanning.ScanningEntryId, SoeDataStorageRecordType.ScanningEntry_RawData);
            if (dataDSRecord != null)
            {
                GeneralManager.CreateDataStorageRecord(entities,
                    type: SoeDataStorageRecordType.EdiEntry_RawData,
                    recordId: edi.EdiEntryId,
                    recordNumber: string.Empty,
                    entityType: SoeEntityType.EdiEntry,
                    dataStorage: dataDSRecord.DataStorage);
            }
            var documentDSRecord = GeneralManager.GetDataStorageRecord(entities, scanning.ActorCompanyId, scanning.ScanningEntryId, SoeDataStorageRecordType.ScanningEntry_Document);
            if (documentDSRecord != null)
            {
                var result = GeneralManager.CreateDataStorageRecord(entities,
                    type: SoeDataStorageRecordType.EdiEntry_Document,
                    recordId: edi.EdiEntryId,
                    recordNumber: string.Empty,
                    entityType: SoeEntityType.EdiEntry,
                    dataStorage: documentDSRecord.DataStorage);
                if (result.RecordId > 0)
                {
                    edi.ImageStorageType = (int)SoeInvoiceImageStorageType.StoredInEdiDataStorage;
                }
            }
        }

        private Supplier LoadSupplier(CompEntities entities, int? supplierId, int actorCompanyId)
        {
            if (!supplierId.HasValue)
                return null;
            return SupplierManager.GetSupplier(entities, (int)supplierId,
                loadAccount: false,
                loadContactAddresses: false,
                loadCategories: true,
                actorCompanyId: actorCompanyId);
        }

        private ScanningEntry CreateScanningEntry(CompEntities entities, int actorCompanyId, Guid batchId)
        {
            var scanningEntry = new ScanningEntry()
            {
                Type = (int)TermGroup_EDISourceType.Scanning,
                MessageType = (int)TermGroup_ScanningMessageType.SupplierInvoice,
                Status = (int)TermGroup_ScanningStatus.Unprocessed,
                XML = null,
                Image = null,
                BatchId = batchId.ToString(),
                DocumentId = Guid.NewGuid().ToString(),
                ReceiveTime = CalendarUtility.DATETIME_MINVALUE,
                OperatorMessage = string.Empty,
                CompanyId = "0",
                SupplierId = "0",
                LearningLogLevel = null,
                Provider = (int)ScanningProvider.AzoraOne,
                //Set FK
                ActorCompanyId = actorCompanyId,
                UsesDataStorage = true,
            };
            SetCreatedProperties(scanningEntry);
            entities.ScanningEntry.AddObject(scanningEntry);
            return scanningEntry;
        }

        private void UpdateScanningEntryWithResult(ScanningEntry scanningEntry, SupplierInvoiceInterpretationDTO interpretation, Company company)
        {
            //Buyer
            scanningEntry.CompanyId = company.CompanyGuid.ToString();
            scanningEntry.SupplierId = interpretation.SupplierId.HasValue ?
                interpretation.SupplierId.Value.ToString() :
                string.Empty;
            scanningEntry.ReceiveTime = interpretation.Metadata.ArrivalTime;
            scanningEntry.DocumentStatus = true;
            scanningEntry.Status = (int)TermGroup_ScanningStatus.Processed;
        }

        private EdiEntry CreateEdiEntryError(CompEntities entities, Company company, ScanningEntry scanningEntry, ActionResult extractionResult)
        {
            var ediEntry = new EdiEntry
            {
                Type = (int)TermGroup_EDISourceType.Scanning,
                MessageType = (int)TermGroup_EdiMessageType.SupplierInvoice,
                Status = (int)TermGroup_EDIStatus.Error,
                State = (int)SoeEntityState.Hidden,

                PDF = null,
                Date = DateTime.Now,
                SeqNr = 0,

                ActorSupplierId = null,


                //Dates
                InvoiceDate = null,
                DueDate = null,

                //Bank
                PostalGiro = string.Empty,
                BankGiro = string.Empty,
                OCR = string.Empty,
                IBAN = string.Empty,

                //BillingType
                BillingType = (int)TermGroup_BillingType.Debit,

                //Buyer
                BuyerReference = string.Empty,
                OrderNr = string.Empty,

                Company = company,
                ScanningEntryInvoice = scanningEntry,
                FileName = string.Empty,

                ErrorCode = extractionResult.ErrorNumber,
                ErrorMessage = extractionResult.ErrorMessage,

                UsesDataStorage = true
            };
            SetCreatedProperties(ediEntry);

            entities.EdiEntry.AddObject(ediEntry);

            //Currency
            SetEdiEntryCurrencyAndAmounts(entities, ediEntry,
                string.Empty,
                0,
                0,
                0,
                company.ActorCompanyId);
            SetDueDateOnEdiEntry(entities, ediEntry, company);
            SetEdiEntryOrderStatus(ediEntry, TermGroup_EDIOrderStatus.Error);
            SetEdiEntryInvoiceStatus(ediEntry, TermGroup_EDIInvoiceStatus.Error);

            return ediEntry;
        }

        private EdiEntry CreateEdiEntry(CompEntities entities, Company company, ScanningEntry scanningEntry, AOResponse<AOSupplierInvoice> response, string rawResponse)
        {
            var interpretationResult = response.ToSupplierInvoiceInterpretationDTO(rawResponse);
            UpdateScanningEntryWithResult(scanningEntry, interpretationResult, company);

            var ediEntry = new EdiEntry
            {
                Type = (int)TermGroup_EDISourceType.Scanning,
                MessageType = (int)TermGroup_EdiMessageType.SupplierInvoice,
                Status = (int)TermGroup_EDIStatus.Unprocessed,
                PDF = null,
                Date = interpretationResult.Metadata.ArrivalTime,
                SeqNr = 0,
                InvoiceNr = interpretationResult.InvoiceNumber.Value ?? null,


                ActorSupplierId = interpretationResult.SupplierId.HasValue ?
                    interpretationResult.SupplierId.Value :
                    null,


                //Dates
                InvoiceDate = interpretationResult.InvoiceDate.HasValue ?
                    interpretationResult.InvoiceDate.Value :
                    null,
                DueDate = interpretationResult.DueDate.HasValue ?
                    interpretationResult.DueDate.Value :
                    null,

                //Bank
                PostalGiro = interpretationResult.BankAccountPG.Value ?? string.Empty,
                BankGiro = interpretationResult.BankAccountBG.Value ?? string.Empty,
                OCR = interpretationResult.PaymentReferenceNumber.Value ?? string.Empty,
                IBAN = interpretationResult.BankAccountIBAN.Value ?? string.Empty,

                //BillingType
                BillingType = interpretationResult.IsCreditInvoice.Value == true ?
                    (int)TermGroup_BillingType.Credit :
                    (int)TermGroup_BillingType.Debit,

                //Buyer
                BuyerReference = interpretationResult.BuyerContactName.Value ?? string.Empty,
                OrderNr = interpretationResult.BuyerOrderNumber.Value ?? string.Empty,

                Company = company,
                ScanningEntryInvoice = scanningEntry,
                FileName = string.Empty,
                ImageStorageType = scanningEntry.Image != null ? (int)SoeInvoiceImageStorageType.StoredInScanningEntry : (int)SoeInvoiceImageStorageType.NoImage,
                UsesDataStorage = true,
                Created = DateTime.Now,
                CreatedBy = "SoftOne (Scanning)"
            };

            entities.EdiEntry.AddObject(ediEntry);

            //Currency
            SetEdiEntryCurrencyAndAmounts(entities, ediEntry,
                interpretationResult.CurrencyCode.Value,
                interpretationResult.AmountIncVatCurrency.Value ?? 0,
                interpretationResult.VatAmountCurrency.Value ?? 0,
                interpretationResult.VatRatePercent.Value ?? 0,
                company.ActorCompanyId);
            SetDueDateOnEdiEntry(entities, ediEntry, company);
            SetEdiEntryOrderStatus(ediEntry, TermGroup_EDIOrderStatus.Error);
            SetEdiEntryInvoiceStatus(ediEntry, TermGroup_EDIInvoiceStatus.Processed);

            ExtendSupplierInvoiceInterpretationFromAzoraOne(interpretationResult, ediEntry);

            return ediEntry;
        }

        public void StartTrainAzoraInterpretor(int actorCompanyId)
        {
            Task.Run(() => CompEntitiesProvider.RunWithTaskScopedReadOnlyEntities(() => TrainAzoraOneInterpretor(actorCompanyId)));
        }

        public ActionResult TrainAzoraOneInterpretor(int actorCompanyId)
        {
            var trainingResult = new ActionResult();
            var runGuid = Guid.NewGuid();
            var startTime = DateTime.Now;

            string GetFileId(Guid companyGuid, int invoiceId) => $"{companyGuid}_{invoiceId}";
            int ElapsedSeconds() => (int)(DateTime.Now - startTime).TotalSeconds;
            void LogProgressMessage(string message) => base.LogInfo($"[{runGuid}] [{ElapsedSeconds()}] {message}");

            base.LogInfo($"[{runGuid}] [{ElapsedSeconds()}] Training interpretor (step 0): For actorCompanyId: {actorCompanyId};");

            try
            {
                using (var entities = new CompEntities())
                {
                    var company = CompanyManager.GetCompany(entities, actorCompanyId, loadEdiConnection: false, loadActorAndContact: false);
                    var azoraOneManager = new AzoraOneManager(company.CompanyGuid);
                    var fileUploader = new AzoraOneDocumentUploader(company.CompanyGuid);

                    var invoices = SupplierInvoiceManager.GetSupplierInvoicesForTrainingInterpretor(entities, actorCompanyId,
                        takePerSupplier: 3,
                        monthsBack: 12);
                    LogProgressMessage($"Training interpretor (step 1): Fetched {invoices.Count} invoices; elapsedSeconds {ElapsedSeconds()};");

                    int invoicesWithoutImage = 0;
                    int successfullyUploaded = 0;
                    foreach (var (invoice, rows) in invoices)
                    {
                        var documentId = GetFileId(company.CompanyGuid, invoice.InvoiceId);
                        var invoiceImage = SupplierInvoiceManager.GetSupplierInvoiceImage(entities,
                            actorCompanyId: actorCompanyId,
                            invoiceId: invoice.InvoiceId,
                            loadAll: false);

                        if (invoiceImage is null || invoiceImage.Image is null)
                        {
                            invoicesWithoutImage++;
                            continue;
                        }

                        var result = fileUploader.UploadDocument(
                            entityPK: invoice.InvoiceId,
                            fileId: GetFileId(company.CompanyGuid, invoice.InvoiceId),
                            fileName: invoiceImage.Filename,
                            fileContent: invoiceImage.Image,
                            webhookUrl: null
                        );

                        if (result.Success)
                            successfullyUploaded++;
                    }
                    LogProgressMessage($"Training interpretor (step 2): Total {invoices.Count}; Found images for {invoices.Count - invoicesWithoutImage}; Successfully uploaded {successfullyUploaded};");

                    int successfullyBookkept = 0;
                    string failedRows = string.Empty;
                    foreach (var (invoiceId, documentId) in fileUploader.GetDocumentsReadyForExtraction())
                    {
                        var (invoice, accountingRows) = invoices.FirstOrDefault(i => i.Item1.InvoiceId == invoiceId);
                        var result = azoraOneManager.BookkeepInvoice(documentId, invoice, accountingRows);
                        if (result.Success)
                            successfullyBookkept++;
                        else
                            failedRows += $"\nInvoiceId: {invoiceId}, DocumentId: {documentId}, Error: {result.ErrorMessage}";
                    }
                    trainingResult.ObjectsAffected = successfullyBookkept;
                    LogProgressMessage($"Training interpretor (step 3): Successfully uploaded {successfullyUploaded}; Successfully bookkept {successfullyBookkept} {failedRows};");
                }
            }
            catch (Exception ex)
            {
                base.LogError(ex, this.log);
                LogProgressMessage($"Training interpretor (error): Failed with exception: {ex.Message}");
                trainingResult = new ActionResult((int)ActionResultSave.NothingSaved, $"Training interpretor (error): {ex.Message}");
            }
            return trainingResult;
        }

        public void BookKeepInvoice(CompEntities entities, int actorCompanyId, ScanningEntry entry, SupplierInvoiceDTO invoice, List<AccountingRowDTO> accountingRows)
        {
            if (entry == null || invoice == null)
                return;

            try
            {
                var company = CompanyManager.GetCompany(entities, actorCompanyId, loadLicense: false, loadEdiConnection: false, loadActorAndContact: false);
                var client = new AzoraOneManager(company.CompanyGuid);

                var documentId = entry.DocumentId;
                var result = client.BookkeepInvoice(documentId, invoice, accountingRows);
                if (!result.Success)
                    result = RetryBookKeepInvoice(entities, client, result, actorCompanyId, entry, invoice, accountingRows);

                if (result.Success == true)
                    entry.LearningLogLevel = (int)ScanningLogLevel.SentWithTrueReturn;
                else
                    entry.LearningLogLevel = (int)ScanningLogLevel.SentWithFalseReturn;

                if (!result.Success)
                    entry.OperatorMessage = result.ErrorMessage;
            }
            catch (Exception ex)
            {
                base.LogError(ex, this.log);
                entry.LearningLogLevel = (int)ScanningLogLevel.SentWithException;
            }
            finally
            {
                SetModifiedProperties(entry);
            }
        }

        public ActionResult RetryBookKeepInvoice(CompEntities entities, AzoraOneManager client, ActionResult bookKeepResult, int actorCompanyId, ScanningEntry entry, SupplierInvoiceDTO invoice, List<AccountingRowDTO> accountingRows)
        {
            // Retry bookkeeping after trying to fix errors that pop up.
            var documentId = entry.DocumentId;
            if (bookKeepResult.Success == false && !string.IsNullOrEmpty(bookKeepResult.ErrorMessage))
            {
                bool retryBookkeep = false;
                if (bookKeepResult.ErrorMessage.Contains("Supplier ID is missing") && invoice.ActorId.HasValue)
                {
                    // Scenario when initial supplier sync has gone wrong for some reason. It's worth attempting to fix it.
                    var supplier = SupplierManager.GetSuppliers(entities, actorCompanyId,
                        loadActor: true,
                        loadAccount: false,
                        loadContactAddresses: false,
                        loadCategories: false,
                        loadPaymentInformation: true,
                        loadTemplateAttestHead: false,
                        supplierIds: new List<int> { invoice.ActorId.Value })
                        .ToDistributionDTOs()
                        .FirstOrDefault();

                    if (supplier != null)
                    {
                        var secondAttempt = client.SyncSupplier(supplier);
                        retryBookkeep = secondAttempt.Success;
                    }
                }

                if (retryBookkeep)
                    return client.BookkeepInvoice(documentId, invoice, accountingRows);
            }
            return bookKeepResult;
        }

        public SupplierInvoiceInterpretationDTO GetSupplierInvoiceInterpretationDTO(int actorCompanyId, int ediEntryId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            var ediEntry = GetEdiScanningEntryInvoice(entities, ediEntryId, actorCompanyId, false);

            if (ediEntry == null)
                return null;

            var scanningEntry = ediEntry.ScanningEntryInvoice;
            var provider = (ScanningProvider)scanningEntry.Provider;
            if (provider == ScanningProvider.AzoraOne)
            {
                var record = GeneralManager.GetDataStorageRecord(entities, actorCompanyId, ediEntry.EdiEntryId, SoeDataStorageRecordType.EdiEntry_RawData);
                if (record == null) return new SupplierInvoiceInterpretationDTO(scanningEntry.ScanningEntryId, ediEntryId);

                var dataStorage = GeneralManager.GetDataStorage(record.DataStorageId, actorCompanyId, false, false);
                var dto = SupplierInvoiceInterpretationFromAzoraOne(entities, dataStorage.Data);
                return ExtendSupplierInvoiceInterpretationFromAzoraOne(dto, ediEntry);
            }
            if (provider == ScanningProvider.ReadSoft)
            {
                return ediEntry.ToSupplierInterpretationDTO();
            }
            return null;
        }

        private DataStorageRecord CreateDocumentDataStorageRecord(CompEntities entities, ScanningEntry entry, DataStorage dataStorage)
        {
            return GeneralManager.CreateDataStorageRecord(entities,
                SoeDataStorageRecordType.ScanningEntry_Document,
                entry.ScanningEntryId,
                entry.DocumentId,
                SoeEntityType.ScanningEntry,
                dataStorage);
        }

        #endregion

        public ActionResult AddEdiConnections(CompEntities entities, int actorCompanyId, int sysWholesellerEdiId, params string[] buyerNrs)
        {
            var company = CompanyManager.GetCompany(entities, actorCompanyId, loadEdiConnection: true);

            var result = DeleteEdiConnections(entities, company, sysWholesellerEdiId);
            if (!result.Success)
                return result;

            var ediMsgList = GetSysEdiMessages(sysWholesellerEdiId);
            foreach (var item in buyerNrs)
            {
                foreach (var ediMsgId in ediMsgList.Select(x => x.SysEdiMsgId).ToList())
                {
                    var existing = company.EdiConnection.Any(e => e.BuyerNr == item && e.SysEdiMsgId == ediMsgId);
                    if (existing)
                        continue;

                    var connection = new EdiConnection
                    {
                        BuyerNr = item,
                        ActorCompanyId = actorCompanyId,
                        SysEdiMsgId = ediMsgId,
                    };

                    SetCreatedProperties(connection);

                    company.EdiConnection.Add(connection);
                }
            }

            // Set created and modified
            return SaveChanges(entities);
        }

        public ActionResult DeleteEdiConnections(CompEntities entities, Company company, int sysWholesellerEdiId)
        {
            var ediMsgList = GetSysEdiMessages(sysWholesellerEdiId);
            foreach (var item in company.EdiConnection.Where(ec => ediMsgList.Any(m => m.SysEdiMsgId == ec.SysEdiMsgId)).Select(ec => ec.EdiConnectionId).ToList())
            {
                var toDelete = entities.EdiConnection.FirstOrDefault(ec => ec.EdiConnectionId == item);
                entities.EdiConnection.DeleteObject(toDelete);
            }

            return SaveChanges(entities);
        }

        private List<SysEdiMsg> GetSysEdiMessages(int sysWholesellerEdiId)
        {
            using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
            return sysEntitiesReadOnly.SysEdiMsg.Where(msg => msg.SysWholesellerEdiId == sysWholesellerEdiId).ToList();
        }
    }

    public class ScanningEntryWrapper
    {
        public ScanningEntry ScanningEntry { get; set; }
        public DataStorage DataStorage { get; set; }

        public ScanningEntryWrapper(ScanningEntry scanningEntry, DataStorage dataStorage)
        {
            if (scanningEntry == null)
                throw new ArgumentNullException("scanningEntry");

            if (dataStorage == null)
                throw new ArgumentNullException("dataStorage");

            ScanningEntry = scanningEntry;
            DataStorage = dataStorage;
        }

        public byte[] GetData()
        {
            return this.DataStorage.Data;
        }

        public string GetDocumentId()
        {
            return this.ScanningEntry.DocumentId;
        }

        public string GetFileName()
        {
            return this.DataStorage.FileName;
        }

        public string GetCallbackUrl(TermGroup_SysPageStatusSiteType siteType, int? compDbId)
        {
            return AzoraOneHelper.GetCallbackUrl(this.ScanningEntry.ScanningEntryId, siteType, compDbId);
        }
    }
}
