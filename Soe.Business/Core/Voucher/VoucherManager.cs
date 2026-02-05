using SoftOne.Soe.Business.Core.Reporting;
using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Util;
using SoftOne.Soe.Util.Exceptions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Transactions;
using Microsoft.Data.OData.Query.SemanticAst;
using static SoftOne.Soe.Business.Core.Reporting.EconomyReportDataManager;

namespace SoftOne.Soe.Business.Core
{
    public class VoucherManager : ManagerBase
    {
        #region Variables

        // Create a logger for use in this class
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #endregion

        #region Ctor

        public VoucherManager(ParameterObject parameterObject) : base(parameterObject) { }

        #endregion

        #region Reconciliation

        public List<ReconciliationRowDTO> GetReconciliationPerAccount(int actorCompanyId, int accountId, DateTime? fromDate, DateTime? toDate)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.ReconciliationRowView.NoTracking();
            return GetReconciliationPerAccount(entities, actorCompanyId, accountId, fromDate, toDate);
        }

        private List<ReconciliationRowDTO> GetReconciliationPerAccount(CompEntities entities, int actorCompanyId, int accountId, DateTime? fromDate, DateTime? toDate)
        {
            List<ReconciliationRowDTO> rows = new List<ReconciliationRowDTO>();

            IQueryable<ReconciliationRowView> query = from rr in entities.ReconciliationRowView
                                                      where rr.ActorCompanyId == actorCompanyId &&
                                                      rr.accountid == accountId
                                                      select rr;

            Account acc = AccountManager.GetAccount(actorCompanyId, accountId, onlyActive: false);

            if (fromDate != null)
            {
                query = query.Where(r => r.Date >= ((DateTime)fromDate).Date);
            }

            if (toDate != null)
            {
                query = query.Where(r => r.Date <= ((DateTime)toDate).Date);
            }

            List<ReconciliationRowView> view = query.ToList();

            var allRows = view.Where(r => r.OriginType == (int)SoeOriginType.CustomerInvoice && r.OriginStatus == (int)SoeOriginStatus.Voucher).ToList();
            allRows.AddRange(view.Where(r => r.OriginType == (int)SoeOriginType.SupplierInvoice && r.OriginStatus == (int)SoeOriginStatus.Voucher).ToList());
            allRows.AddRange(view.Where(r => (r.OriginStatus == (int)SoeOriginStatus.Origin || r.OriginStatus == (int)SoeOriginStatus.Payment) && (r.OriginType == (int)SoeOriginType.SupplierPayment || r.OriginType == (int)SoeOriginType.CustomerPayment)).ToList());
            allRows.AddRange(view.Where(r => r.OriginType == 0).ToList());

            foreach (var row in allRows)
            {
                int status = 3;

                switch (row.Type)
                {
                    case (int)ReconciliationRowType.Voucher:
                        List<ReconciliationRowView> vouRows = allRows.Where(r => r.OriginType != 0 && r.VoucherHeadId == row.AssociatedId).ToList();

                        if (vouRows != null && vouRows.Count > 0)
                        {
                            decimal sum = vouRows.Sum(r => r.Amount);

                            if (row.Amount == sum)
                            {
                                status = 1;
                            }
                            else
                            {
                                ReconciliationRowView rv = vouRows.FirstOrDefault(r => r.Amount == row.Amount);

                                if (rv != null)
                                    status = 1;
                            }
                        }
                        break;
                    case (int)ReconciliationRowType.CustomerInvoice:
                    case (int)ReconciliationRowType.SupplierInvoice:
                    case (int)ReconciliationRowType.Payment:
                        ReconciliationRowView v = allRows.FirstOrDefault(r => r.AssociatedId == row.VoucherHeadId && r.OriginType == 0 && r.Amount == row.Amount);

                        if (v != null)
                        {
                            status = 1;
                        }
                        break;
                }

                rows.Add(new ReconciliationRowDTO()
                {
                    AccountId = acc.AccountId,
                    Account = acc.AccountNr + " " + acc.Name,
                    ActorCompanyId = actorCompanyId,
                    CustomerAmount = row.Amount, //Using as amount in details grid
                    SupplierAmount = 0,
                    PaymentAmount = 0,
                    LedgerAmount = 0,
                    DiffAmount = 0,
                    FromDate = (DateTime)fromDate,
                    ToDate = (DateTime)toDate,
                    Date = (DateTime)row.Date,
                    Type = row.Type,
                    Name = row.Name,
                    AssociatedId = row.AssociatedId,
                    Number = row.Number,
                    RowStatus = status,
                    VoucherSeriesId = row.VoucherSeriesId,
                    OriginType = row.OriginType,
                });
            }

            return rows;
        }

        public List<ReconciliationRowDTO> GetReconciliationRows(int actorCompanyId, int dim1Id, string fromDim1, string toDim1, DateTime? fromDate, DateTime? toDate)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.ReconciliationRowView.NoTracking();
            return GetReconciliationRows(entities, actorCompanyId, dim1Id, fromDim1, toDim1, fromDate, toDate);
        }

        private List<ReconciliationRowDTO> GetReconciliationRows(CompEntities entities, int actorCompanyId, int dim1Id, string fromDim1, string toDim1, DateTime? fromDate, DateTime? toDate)
        {
            List<ReconciliationRowDTO> rows = new List<ReconciliationRowDTO>();

            IQueryable<ReconciliationRowView> query = from rr in entities.ReconciliationRowView
                                                      where rr.ActorCompanyId == actorCompanyId
                                                      select rr;

            if (fromDate != null)
                query = query.Where(r => r.Date >= ((DateTime)fromDate).Date);
            if (toDate != null)
                query = query.Where(r => r.Date <= ((DateTime)toDate).Date);

            List<Account> accounts = new List<Account>();

            if (dim1Id != 0)
            {
                using var entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
                var accs = (from a in entitiesReadOnly.Account
                                    .Include("AccountStd")
                            where a.AccountDimId == dim1Id &&
                            a.ActorCompanyId == actorCompanyId &&
                            a.State == (int)SoeEntityState.Active
                            orderby a.AccountNr
                            select a).ToList();

                if (String.IsNullOrEmpty(fromDim1))
                    fromDim1 = accs.First().AccountNr;

                if (String.IsNullOrEmpty(toDim1))
                    toDim1 = accs.Last().AccountNr;

                accounts.AddRange(accs.Where(a => Validator.IsAccountInInterval(a.AccountNr, dim1Id, new AccountIntervalDTO() { AccountDimId = dim1Id, AccountNrFrom = fromDim1, AccountNrTo = toDim1 })).ToList());

                List<int> accountIds = accounts.Select(a => a.AccountId).ToList();

                query = query.Where(r => accountIds.Contains(r.accountid));
            }

            List<ReconciliationRowView> view = query.ToList();

            foreach (Account acc in accounts)
            {
                decimal cusInvSum = 0;
                decimal supInvSum = 0;
                decimal paySum = 0;
                decimal vouSum = 0;

                var cusRows = view.Where(r => r.accountid == acc.AccountId && r.OriginType == (int)SoeOriginType.CustomerInvoice && r.OriginStatus == (int)SoeOriginStatus.Voucher).ToList();
                if (!cusRows.IsNullOrEmpty())
                    cusInvSum = cusRows.Sum(r => r.Amount);

                var supRows = view.Where(r => r.accountid == acc.AccountId && r.OriginType == (int)SoeOriginType.SupplierInvoice && r.OriginStatus == (int)SoeOriginStatus.Voucher).ToList();
                if (!supRows.IsNullOrEmpty())
                    supInvSum = supRows.Sum(r => r.Amount);

                // manually saved payments are having 'origin' as origin status
                var payRows = view.Where(r => r.accountid == acc.AccountId && (r.OriginStatus == (int)SoeOriginStatus.Origin || r.OriginStatus == (int)SoeOriginStatus.Payment) && (r.OriginType == (int)SoeOriginType.SupplierPayment || r.OriginType == (int)SoeOriginType.CustomerPayment)).ToList();
                if (!payRows.IsNullOrEmpty())
                    paySum = payRows.Sum(r => r.Amount);

                var vouRows = view.Where(r => r.accountid == acc.AccountId && r.OriginType == 0).ToList();
                if (!vouRows.IsNullOrEmpty())
                    vouSum = vouRows.Sum(r => r.Amount);

                if (cusInvSum != 0 || supInvSum != 0 || paySum != 0 || vouSum != 0)
                {
                    rows.Add(new ReconciliationRowDTO()
                    {
                        AccountId = acc.AccountId,
                        Account = acc.AccountNr + " " + acc.Name,
                        ActorCompanyId = actorCompanyId,
                        CustomerAmount = cusInvSum,
                        SupplierAmount = supInvSum,
                        PaymentAmount = paySum,
                        LedgerAmount = vouSum,
                        DiffAmount = cusInvSum != 0 || supInvSum != 0 ? ((cusInvSum + supInvSum) + paySum) - (vouSum) : vouSum - paySum,
                        FromDate = (DateTime)fromDate,
                        ToDate = (DateTime)toDate,
                    });
                }
            }

            return rows;
        }

        #endregion

        #region VoucherSeriesType

        public List<VoucherSeriesType> GetVoucherSeriesTypes(int actorCompanyId, bool includeTemplate, int? voucherSeriesTypeId = null)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.VoucherSeriesType.NoTracking();
            return GetVoucherSeriesTypes(entities, actorCompanyId, includeTemplate, voucherSeriesTypeId);
        }

        public List<VoucherSeriesType> GetVoucherSeriesTypes(CompEntities entities, int actorCompanyId, bool includeTemplate, int? voucherSeriesTypeId = null)
        {

            IQueryable<VoucherSeriesType> query = (from vst in entities.VoucherSeriesType
                                                   where vst.ActorCompanyId == actorCompanyId &&
                                                   vst.State == (int)SoeEntityState.Active &&
                                                   (includeTemplate || !vst.Template)
                                                   orderby vst.VoucherSeriesTypeNr ascending
                                                   select vst);

            if (voucherSeriesTypeId.HasValue)
                query = query.Where(t => t.VoucherSeriesTypeId == voucherSeriesTypeId);

            List<VoucherSeriesType> VoucherSeriesType = query.ToList();
            return VoucherSeriesType.ToList();
        }

        public Dictionary<int, string> GetVoucherSeriesTypesDict(int actorCompanyId, bool includeTemplate, bool? addEmptyRow = false, bool? nameOnly = false)
        {
            Dictionary<int, string> dict = new Dictionary<int, string>();
            if (addEmptyRow.HasValue && (bool)addEmptyRow)
                dict.Add(0, " ");

            List<VoucherSeriesType> voucherSerieTypes = GetVoucherSeriesTypes(actorCompanyId, includeTemplate);
            foreach (VoucherSeriesType voucherSeriesType in voucherSerieTypes)
            {
                if (nameOnly.HasValue && (bool)nameOnly)
                    dict.Add(voucherSeriesType.VoucherSeriesTypeId, voucherSeriesType.Name);
                else
                    dict.Add(voucherSeriesType.VoucherSeriesTypeId, voucherSeriesType.VoucherSeriesTypeNr + ". " + voucherSeriesType.Name);
            }

            return dict;
        }

        public VoucherSeriesType GetVoucherSeriesType(int voucherSeriesTypeId, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.VoucherSeriesType.NoTracking();
            return GetVoucherSeriesType(entities, voucherSeriesTypeId, actorCompanyId);
        }

        public VoucherSeriesType GetVoucherSeriesType(CompEntities entities, int voucherSeriesTypeId, int actorCompanyId, bool loadVoucherSeries = false)
        {
            if (loadVoucherSeries)
            {
                return (from vst in entities.VoucherSeriesType
                            .Include("VoucherSeries")
                        where vst.VoucherSeriesTypeId == voucherSeriesTypeId &&
                        vst.ActorCompanyId == actorCompanyId &&
                        vst.State == (int)SoeEntityState.Active
                        select vst).FirstOrDefault();
            }
            else
            {
                return (from vst in entities.VoucherSeriesType
                        where vst.VoucherSeriesTypeId == voucherSeriesTypeId &&
                        vst.ActorCompanyId == actorCompanyId &&
                        vst.State == (int)SoeEntityState.Active
                        select vst).FirstOrDefault();
            }
        }

        public VoucherSeriesType GetVoucherSeriesTypeByName(string name, int actorCompanyId, int? voucherSeriesTypeNr = null)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.VoucherSeriesType.NoTracking();
            return GetVoucherSeriesTypeByName(entities, name, actorCompanyId, voucherSeriesTypeNr);
        }

        public VoucherSeriesType GetVoucherSeriesTypeByName(CompEntities entities, string name, int actorCompanyId, int? voucherSeriesTypeNr = null)
        {
            var query = (from vst in entities.VoucherSeriesType
                         where vst.Name == name &&
                         vst.ActorCompanyId == actorCompanyId &&
                         vst.State == (int)SoeEntityState.Active
                         select vst);
            if (voucherSeriesTypeNr.HasValue)
            {
                query = query.Where(x => x.VoucherSeriesTypeNr == voucherSeriesTypeNr.Value);
            }
            return query.FirstOrDefault();
        }

        public List<VoucherSeriesType> GetVoucherSeriesTypeByNameOrNr(int voucherSeriesTypeNr, string name, int actorCompanyId, int? excludeVoucherSeriesTypeId = null)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.VoucherSeriesType.NoTracking();
            return GetVoucherSeriesTypeByNameOrNr(entities, voucherSeriesTypeNr, name, actorCompanyId, excludeVoucherSeriesTypeId);
        }

        public List<VoucherSeriesType> GetVoucherSeriesTypeByNameOrNr(CompEntities entities, int voucherSeriesTypeNr, string name, int actorCompanyId, int? excludeVoucherSeriesTypeId = null)
        {
            return (from vst in entities.VoucherSeriesType
                    where vst.ActorCompanyId == actorCompanyId &&
                    (vst.VoucherSeriesTypeNr == voucherSeriesTypeNr || vst.Name == name) &&
                    (!excludeVoucherSeriesTypeId.HasValue || excludeVoucherSeriesTypeId.Value != vst.VoucherSeriesTypeId) &&
                    vst.State == (int)SoeEntityState.Active
                    select vst).ToList();
        }

        public VoucherSeriesType GetTemplateVoucherSeriesType(int actorCompanyId, bool addIfNotExists)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.VoucherSeriesType.NoTracking();
            VoucherSeriesType voucherSeriesType = (from vst in entities.VoucherSeriesType
                                                   where vst.ActorCompanyId == actorCompanyId &&
                                                   vst.Template &&
                                                   vst.State == (int)SoeEntityState.Active
                                                   select vst).FirstOrDefault();

            if (voucherSeriesType == null && addIfNotExists)
            {
                var result = AddTemplateVoucherSeriesType(actorCompanyId);
                if (result.Success)
                    voucherSeriesType = GetTemplateVoucherSeriesType(actorCompanyId, false); //prevent recursive
            }

            return voucherSeriesType;
        }

        public VoucherSeriesType GetPrevNextVoucherSeriesType(int voucherSeriesTypeId, int actorCompanyId, SoeFormMode mode)
        {
            VoucherSeriesType voucherSeriesType = null;
            using var entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            entitiesReadOnly.VoucherSeriesType.NoTracking();
            if (mode == SoeFormMode.Next)
            {
                voucherSeriesType = (from vst in entitiesReadOnly.VoucherSeriesType
                                     where vst.VoucherSeriesTypeId > voucherSeriesTypeId &&
                                     vst.ActorCompanyId == actorCompanyId &&
                                     vst.State == (int)SoeEntityState.Active
                                     orderby vst.VoucherSeriesTypeId ascending
                                     select vst).FirstOrDefault();
            }
            else if (mode == SoeFormMode.Prev)
            {
                voucherSeriesType = (from vst in entitiesReadOnly.VoucherSeriesType
                                     where vst.VoucherSeriesTypeId < voucherSeriesTypeId &&
                                     vst.ActorCompanyId == actorCompanyId &&
                                     vst.State == (int)SoeEntityState.Active
                                     orderby vst.VoucherSeriesTypeId descending
                                     select vst).FirstOrDefault();
            }

            return voucherSeriesType;
        }

        public bool VoucherSeriesTypeExist(int voucherSeriesTypeNr, string name, int actorCompanyId, int? excludeVoucherSeriesTypeId = null)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.VoucherSeriesType.NoTracking();
            return VoucherSeriesTypeExist(entities, voucherSeriesTypeNr, name, actorCompanyId, excludeVoucherSeriesTypeId);
        }

        public bool VoucherSeriesTypeExist(CompEntities entities, int voucherSeriesTypeNr, string name, int actorCompanyId, int? excludeVoucherSeriesTypeId = null)
        {
            List<VoucherSeriesType> voucherSeriesType = GetVoucherSeriesTypeByNameOrNr(entities, voucherSeriesTypeNr, name, actorCompanyId, excludeVoucherSeriesTypeId);
            return voucherSeriesType.Count != 0;
        }

        public ActionResult UpdateVoucherTemplateRow(VoucherRow row)
        {
            if (row == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "VoucherRow");

            using (CompEntities entities = new CompEntities())
            {
                // Get original MatchCode
                VoucherRow originalVoucherRow = GetVoucherRow(entities, row.VoucherRowId);
                if (originalVoucherRow == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "VoucherRow");

                return SaveEntityItem(entities, row);
            }
        }

        public VoucherRow GetVoucherRow(CompEntities entities, int voucherRowId)
        {
            return (from vr in entities.VoucherRow
                    where vr.VoucherRowId == voucherRowId
                    select vr).FirstOrDefault<VoucherRow>();
        }



        public ActionResult AddVoucherTemplateRow(VoucherRow row, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.VoucherRow.NoTracking();
            return AddVoucherTemplateRow(entities, row, actorCompanyId);
        }
        public ActionResult AddVoucherTemplateRow(CompEntities entities, VoucherRow row, int actorCompanyId)
        {
            if (row == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "VoucherRow");

            return AddEntityItem(entities, row, "VoucherRow");
        }

        public ActionResult AddVoucherTemplate(VoucherHead voucher, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.VoucherHead.NoTracking();
            return AddVoucherTemplate(entities, voucher, actorCompanyId);
        }

        public ActionResult AddVoucherTemplate(CompEntities entities, VoucherHead voucher, int actorCompanyId)
        {
            if (voucher == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "VoucherHead");

            return AddEntityItem(entities, voucher, "VoucherHead");
        }

        public ActionResult UpdateVoucherTemplate(VoucherHead voucherTemplate, int actorCompanyId)
        {
            if (voucherTemplate == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "VoucherHead");

            using (CompEntities entities = new CompEntities())
            {
                VoucherHead orginalVoucherTemplate = GetVoucherHead(entities, voucherTemplate.VoucherHeadId, false, true);
                if (orginalVoucherTemplate == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "VoucherHead");

                return UpdateEntityItem(entities, orginalVoucherTemplate, voucherTemplate, "VoucherHead");
            }
        }

        public ActionResult AddVoucherSeriesType(VoucherSeriesType voucherSeriesType, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.VoucherSeriesType.NoTracking();
            return AddVoucherSeriesType(entities, voucherSeriesType, actorCompanyId);
        }

        public ActionResult AddVoucherSeriesType(CompEntities entities, VoucherSeriesType voucherSeriesType, int actorCompanyId)
        {
            if (voucherSeriesType == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "VoucherSeriesType");

            voucherSeriesType.Company = CompanyManager.GetCompany(entities, actorCompanyId);
            if (voucherSeriesType.Company == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte"));

            return AddEntityItem(entities, voucherSeriesType, "VoucherSeriesType");
        }

        public ActionResult UpdateVoucherSeriesType(VoucherSeriesType voucherSeriesType, int actorCompanyId)
        {
            if (voucherSeriesType == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "VoucherSeriesType");

            using (CompEntities entities = new CompEntities())
            {
                VoucherSeriesType orginalVoucherSeriesType = GetVoucherSeriesType(entities, voucherSeriesType.VoucherSeriesTypeId, actorCompanyId);
                if (orginalVoucherSeriesType == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "VoucherSeriesType");

                return UpdateEntityItem(entities, orginalVoucherSeriesType, voucherSeriesType, "VoucherSeriesType");
            }
        }

        public ActionResult SaveVoucherSeriesType(VoucherSeriesTypeDTO voucherSeriesTypeInput, int actorCompanyId)
        {
            ActionResult result = new ActionResult();

            if (voucherSeriesTypeInput == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "VoucherSeriesTypeDTO");

            using (CompEntities entities = new CompEntities())
            {
                Company company = CompanyManager.GetCompany(entities, actorCompanyId);
                // Get original voucherseriestype
                VoucherSeriesType voucherSeriesType = GetVoucherSeriesType(entities, voucherSeriesTypeInput.VoucherSeriesTypeId, actorCompanyId);
                if (voucherSeriesType == null)
                {
                    List<VoucherSeriesType> voucherSeriesTypeByNameOrNr = GetVoucherSeriesTypeByNameOrNr(voucherSeriesTypeInput.VoucherSeriesTypeNr, voucherSeriesTypeInput.Name, actorCompanyId);
                    if (voucherSeriesTypeByNameOrNr.Count != 0)
                    {
                        string errorMessage = "";
                        if (voucherSeriesTypeByNameOrNr.Exists(vs => vs.VoucherSeriesTypeNr == voucherSeriesTypeInput.VoucherSeriesTypeNr))
                            errorMessage = GetText(13007, "Serienumret existerar redan") + "\n";
                        if (voucherSeriesTypeByNameOrNr.Exists(vs => vs.Name == voucherSeriesTypeInput.Name))
                            errorMessage += GetText(13008, "Benämning existerar redan");

                        return new ActionResult((int)ActionResultSave.EntityExists, errorMessage);
                    }

                    voucherSeriesType = new VoucherSeriesType()
                    {
                        Company = company,
                    };

                    SetCreatedProperties(voucherSeriesType);
                    entities.VoucherSeriesType.AddObject(voucherSeriesType);
                }
                else
                {
                    SetModifiedProperties(voucherSeriesType);
                }

                // Modify it
                voucherSeriesType.VoucherSeriesTypeNr = voucherSeriesTypeInput.VoucherSeriesTypeNr;
                voucherSeriesType.Name = voucherSeriesTypeInput.Name;
                voucherSeriesType.StartNr = voucherSeriesTypeInput.StartNr;
                voucherSeriesType.YearEndSerie = voucherSeriesTypeInput.YearEndSerie;
                voucherSeriesType.ExternalSerie = voucherSeriesTypeInput.ExternalSerie;

                result = SaveChanges(entities);
                if (!result.Success)
                    return result;

                result.IntegerValue = voucherSeriesType.VoucherSeriesTypeId;
            }

            return result;
        }

        public ActionResult DeleteVoucherSeriesType(int voucherSeriesTypeId, int actorCompanyId)
        {
            using (CompEntities entities = new CompEntities())
            {
                VoucherSeriesType orginalVoucherSeriesType = GetVoucherSeriesType(entities, voucherSeriesTypeId, actorCompanyId, loadVoucherSeries: true);
                if (orginalVoucherSeriesType == null)
                    return new ActionResult((int)ActionResultDelete.EntityNotFound, "VoucherSeriesType");

                //Check relation dependencies
                if (orginalVoucherSeriesType.VoucherSeries != null && orginalVoucherSeriesType.VoucherSeries.Count > 0)
                    return new ActionResult((int)ActionResultDelete.VoucherSeriesTypeHasVoucherSeries, GetText(1278, "Verifikatserie kunde inte tas bort, kontrollera att den inte används"));

                return ChangeEntityState(entities, orginalVoucherSeriesType, SoeEntityState.Deleted, true);
            }
        }

        public ActionResult AddTemplateVoucherSeriesType(int actorCompanyId)
        {
            using (CompEntities entities = new CompEntities())
            {
                VoucherSeriesType voucherSeriesType = new VoucherSeriesType()
                {
                    StartNr = 1,
                    Name = GetText(1707, "Mallar"),
                    VoucherSeriesTypeNr = 0,
                    Template = true,

                    //Set FK
                    ActorCompanyId = actorCompanyId,
                };

                return AddEntityItem(entities, voucherSeriesType, "VoucherSeriesType");
            }
        }

        public ActionResult AddCompanyGroupVoucherSeries(int accountYearId, int actorCompanyId)
        {
            ActionResult result = new ActionResult(true);

            try
            {
                using (CompEntities entities = new CompEntities())
                {
                    int voucherTypeId = 0;

                    VoucherSeriesType voucherSeriesType = GetVoucherSeriesTypeByName(GetText(9284, "Koncernverifikat"), actorCompanyId);

                    if (voucherSeriesType == null)
                    {
                        voucherSeriesType = new VoucherSeriesType()
                        {
                            StartNr = 1,
                            Name = GetText(9284, "Koncernverifikat"),
                            VoucherSeriesTypeNr = 0,
                            Template = false,

                            //Set FK
                            ActorCompanyId = actorCompanyId,
                        };

                        AddEntityItem(entities, voucherSeriesType, "VoucherSeriesType");
                    }

                    voucherTypeId = voucherSeriesType.VoucherSeriesTypeId;

                    VoucherSeries voucherSerie = new VoucherSeries()
                    {
                        Status = (int)SoeEntityState.Active,
                        VoucherDateLatest = null,
                        VoucherNrLatest = 0,
                        VoucherSeriesTypeId = voucherTypeId
                    };

                    result = AddVoucherSeries(entities, voucherSerie, actorCompanyId, accountYearId, voucherTypeId);
                    if (!result.Success) return null;

                    result.Value = voucherSerie.ToDTO();

                }

            }
            catch (Exception ex)
            {
                base.LogError(ex, this.log);
                return null;
            }

            return result;
        }

        #endregion

        #region VoucherSeries

        public List<VoucherSeries> GetVoucherSeriesByYear(int accountYearId, int actorCompanyId, bool includeTemplate, bool loadAccountYear = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.VoucherSeries.NoTracking();
            return GetVoucherSeriesByYear(entities, accountYearId, actorCompanyId, includeTemplate, loadAccountYear);
        }

        public List<VoucherSeries> GetVoucherSeriesByYear(CompEntities entities, int accountYearId, int actorCompanyId, bool includeTemplate, bool loadAccountYear = false)
        {
            if (loadAccountYear)
            {
                return (from vs in entities.VoucherSeries
                            .Include("VoucherSeriesType")
                            .Include("AccountYear")
                        where vs.AccountYearId == accountYearId &&
                        vs.VoucherSeriesType.ActorCompanyId == actorCompanyId &&
                        vs.VoucherSeriesType.State == (int)SoeEntityState.Active &&
                        (includeTemplate || !vs.VoucherSeriesType.Template)
                        orderby vs.VoucherSeriesType.VoucherSeriesTypeNr
                        select vs).ToList();
            }
            else
            {
                return (from vs in entities.VoucherSeries
                            .Include("VoucherSeriesType")
                        where vs.AccountYearId == accountYearId &&
                        vs.VoucherSeriesType.ActorCompanyId == actorCompanyId &&
                        vs.VoucherSeriesType.State == (int)SoeEntityState.Active &&
                        (includeTemplate || !vs.VoucherSeriesType.Template)
                        orderby vs.VoucherSeriesType.VoucherSeriesTypeNr
                        select vs).ToList();
            }
        }

        public List<VoucherSeries> GetVoucherSeriesByYear(int accountYearIdFrom, int accountYearIdTo, int actorCompanyId, bool includeTemplate, bool loadAccountYear = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.VoucherSeries.NoTracking();
            return GetVoucherSeriesByYear(entities, accountYearIdFrom, accountYearIdTo, actorCompanyId, includeTemplate, loadAccountYear);
        }

        public List<VoucherSeries> GetVoucherSeriesByYear(CompEntities entities, int accountYearIdFrom, int accountYearIdTo, int actorCompanyId, bool includeTemplate, bool loadAccountYear = false)
        {
            List<VoucherSeries> voucherSeries = new List<VoucherSeries>();

            AccountYear accountYearFrom = AccountManager.GetAccountYear(accountYearIdFrom);
            if (accountYearFrom == null)
                return voucherSeries;
            AccountYear accountYearTo = AccountManager.GetAccountYear(accountYearIdTo);
            if (accountYearTo == null)
                return voucherSeries;

            if (loadAccountYear)
            {
                return (from vs in entities.VoucherSeries
                            .Include("VoucherSeriesType")
                            .Include("AccountYear")
                        where vs.AccountYear.ActorCompanyId == actorCompanyId &&
                        vs.AccountYear.From >= accountYearFrom.From &&
                        vs.AccountYear.To <= accountYearTo.To &&
                        vs.VoucherSeriesType.State == (int)SoeEntityState.Active &&
                        (includeTemplate || !vs.VoucherSeriesType.Template)
                        orderby vs.VoucherSeriesType.VoucherSeriesTypeNr
                        select vs).ToList();
            }
            else
            {
                return (from vs in entities.VoucherSeries
                            .Include("VoucherSeriesType")
                        where vs.AccountYear.ActorCompanyId == actorCompanyId &&
                        vs.AccountYear.From >= accountYearFrom.From &&
                        vs.AccountYear.To <= accountYearTo.To &&
                        vs.VoucherSeriesType.State == (int)SoeEntityState.Active &&
                        (includeTemplate || !vs.VoucherSeriesType.Template)
                        orderby vs.VoucherSeriesType.VoucherSeriesTypeNr
                        select vs).ToList();
            }
        }

        public Dictionary<int, string> GetVoucherSeriesByYearDict(int accountYearId, int actorCompanyId, bool addEmptyRow, bool includeTemplate)
        {
            Dictionary<int, string> dict = new Dictionary<int, string>();
            if (addEmptyRow)
                dict.Add(0, " ");

            List<VoucherSeries> voucherSeries = GetVoucherSeriesByYear(accountYearId, actorCompanyId, includeTemplate);
            foreach (VoucherSeries voucherSerie in voucherSeries.OrderBy(i => i.VoucherSeriesType.VoucherSeriesTypeNr))
            {
                if (!includeTemplate && voucherSerie.VoucherSeriesType.Template)
                    continue;

                dict.Add(voucherSerie.VoucherSeriesId, voucherSerie.VoucherSeriesType.Name);
            }

            return dict;
        }

        public Dictionary<int, string> GetVoucherSeriesByYearDict(int accountYearIdFrom, int accountYearIdTo, int actorCompanyId, bool includeTemplate, bool addEmptyRow)
        {
            Dictionary<int, string> dict = new Dictionary<int, string>();
            if (addEmptyRow)
                dict.Add(0, " ");

            List<VoucherSeries> voucherSeries = GetVoucherSeriesByYear(accountYearIdFrom, accountYearIdTo, actorCompanyId, includeTemplate);
            foreach (VoucherSeries voucherSerie in voucherSeries.OrderBy(i => i.VoucherSeriesType.VoucherSeriesTypeNr))
            {
                if (!includeTemplate && voucherSerie.VoucherSeriesType.Template)
                    continue;

                int typeNr = voucherSerie.VoucherSeriesType.VoucherSeriesTypeNr;
                string name = voucherSerie.VoucherSeriesType.Name;
                if (!dict.ContainsKey(typeNr))
                    dict.Add(typeNr, typeNr + ". " + name);
            }

            return dict;
        }

        public VoucherSeries GetVoucherSerie(int voucherSeriesId, int actorCompanyId, bool loadVoucherSeriesType = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.VoucherSeries.NoTracking();
            return GetVoucherSerie(entities, voucherSeriesId, actorCompanyId, loadVoucherSeriesType);
        }

        public VoucherSeries GetVoucherSerie(CompEntities entities, int voucherSeriesId, int actorCompanyId, bool loadVoucherSeriesType = false)
        {
            if (loadVoucherSeriesType)
            {
                return (from vs in entities.VoucherSeries
                            .Include("VoucherSeriesType")
                        where vs.VoucherSeriesId == voucherSeriesId &&
                        vs.VoucherSeriesType.ActorCompanyId == actorCompanyId
                        select vs).FirstOrDefault();
            }
            else
            {
                return (from vs in entities.VoucherSeries
                        where vs.VoucherSeriesId == voucherSeriesId &&
                        vs.VoucherSeriesType.ActorCompanyId == actorCompanyId
                        select vs).FirstOrDefault();
            }
        }

		public VoucherSeries GetVoucherSerie(CompEntities entities, int voucherSeriesTypeId, DateTime date, int actorCompanyId)
		{
			return (from vs in entities.VoucherSeries
					where vs.VoucherSeriesTypeId == voucherSeriesTypeId &&
					vs.VoucherSeriesType.ActorCompanyId == actorCompanyId &&
                    vs.AccountYear.ActorCompanyId == actorCompanyId &&
                    vs.AccountYear.From <= date &&
                    vs.AccountYear.To >= date
					select vs).SingleOrDefault();
		}

		public List<VoucherSeries> GetVoucherSeries(int actorCompanyId, bool loadVoucherSeriesType = false, bool? active = true)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.VoucherSeries.NoTracking();
            return GetVoucherSeries(entities, actorCompanyId, loadVoucherSeriesType, active);
        }

        public List<VoucherSeries> GetVoucherSeries(CompEntities entities, int actorCompanyId, bool loadVoucherSeriesType = false, bool? active = true)
        {
            IQueryable<VoucherSeries> query = (from vs in entities.VoucherSeries
                                               where vs.VoucherSeriesType.ActorCompanyId == actorCompanyId
                                               select vs);
            if (loadVoucherSeriesType)
                query = query.Include("VoucherSeriesType");

            List<VoucherSeries> voucherSeries = null;
            if (active == true)
                voucherSeries = query.Where(a => a.VoucherSeriesType.State == (int)SoeEntityState.Active).ToList();
            else if (active == false)
                voucherSeries = query.Where(a => a.VoucherSeriesType.State == (int)SoeEntityState.Inactive).ToList();
            else
            {
                voucherSeries = query.Where(a => a.VoucherSeriesType.State != (int)SoeEntityState.Deleted).ToList();
            }

            return voucherSeries;
        }

        public VoucherSeries GetVoucherSerieByType(int voucherSeriesTypeId, int accountYearId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.VoucherSeries.NoTracking();
            return GetVoucherSerieByType(entities, voucherSeriesTypeId, accountYearId);
        }

        public VoucherSeries GetVoucherSerieByType(CompEntities entities, int voucherSeriesTypeId, int accountYearId)
        {
            return (from vs in entities.VoucherSeries
                        .Include("VoucherSeriesType")
                    where vs.AccountYearId == accountYearId &&
                    vs.VoucherSeriesType.VoucherSeriesTypeId == voucherSeriesTypeId &&
                    vs.VoucherSeriesType.State == (int)SoeEntityState.Active
                    select vs).FirstOrDefault();
        }

        public VoucherSeries GetVoucherSerieByTypeNr(int voucherSeriesTypeNr, int accountYearId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.VoucherSeries.NoTracking();
            return GetVoucherSerieByTypeNr(entities, voucherSeriesTypeNr, accountYearId);
        }

        public VoucherSeries GetVoucherSerieByTypeNr(CompEntities entities, int voucherSeriesTypeNr, int accountYearId)
        {
            return (from vs in entities.VoucherSeries
                        .Include("VoucherSeriesType")
                    where vs.AccountYearId == accountYearId &&
                    vs.VoucherSeriesType.VoucherSeriesTypeNr == voucherSeriesTypeNr &&
                    vs.VoucherSeriesType.State == (int)SoeEntityState.Active
                    select vs).FirstOrDefault();
        }

        public VoucherSeries GetVoucherSerieByYear(CompEntities entities, int accountYearId, int voucherSeriesTypeId)
        {
            return (from vs in entities.VoucherSeries
                    where vs.AccountYearId == accountYearId &&
                    vs.VoucherSeriesType.VoucherSeriesTypeId == voucherSeriesTypeId &&
                    vs.VoucherSeriesType.State == (int)SoeEntityState.Active
                    select vs).FirstOrDefault();
        }

        public VoucherSeries GetTemplateVoucherSerie(int accountYearId, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.VoucherSeries.NoTracking();
            return GetTemplateVoucherSerie(entities, accountYearId, actorCompanyId);
        }

        public VoucherSeries GetTemplateVoucherSerie(CompEntities entities, int accountYearId, int actorCompanyId)
        {
            return (from vs in entities.VoucherSeries
                        .Include("VoucherSeriesType")
                    where vs.AccountYearId == accountYearId &&
                    vs.VoucherSeriesType.ActorCompanyId == actorCompanyId &&
                    vs.VoucherSeriesType.Template &&
                    vs.VoucherSeriesType.State == (int)SoeEntityState.Active
                    select vs).FirstOrDefault();
        }

        public int GetDefaultVoucherSeriesId(int accountYearId, CompanySettingType type, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.VoucherSeries.NoTracking();
            return GetDefaultVoucherSeriesId(entities, accountYearId, type, actorCompanyId);
        }

        public int GetDefaultVoucherSeriesId(CompEntities entities, int accountYearId, CompanySettingType type, int actorCompanyId)
        {
            VoucherSeries voucherSeries = GetDefaultVoucherSeries(entities, accountYearId, type, actorCompanyId);
            if (voucherSeries == null) return 0;
            return voucherSeries.VoucherSeriesId;
        }

        public VoucherSeries GetDefaultVoucherSeries(int accountYearId, CompanySettingType type, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.VoucherSeries.NoTracking();
            return GetDefaultVoucherSeries(entities, accountYearId, type, actorCompanyId);
        }

        public VoucherSeries GetDefaultVoucherSeries(CompEntities entities, int accountYearId, CompanySettingType type, int actorCompanyId)
        {
            int voucherSeriesTypeId = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)type, 0, actorCompanyId, 0);
            VoucherSeries voucherSeries = GetVoucherSerieByType(entities, voucherSeriesTypeId, accountYearId);
            return voucherSeries;
        }

        public bool VoucherExists(DateTime voucherDate, int voucherSeriesTypeId, TermGroup_VoucherHeadSourceType sourceType, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.VoucherHead.NoTracking();
            return VoucherExists(entities, voucherDate, voucherSeriesTypeId, sourceType, actorCompanyId);
        }

        public bool VoucherExists(CompEntities entities, DateTime voucherDate, int voucherSeriesTypeId, TermGroup_VoucherHeadSourceType sourceType, int actorCompanyId)
        {
            return (from v in entities.VoucherHead
                    where v.ActorCompanyId == actorCompanyId &&
                    v.Date == voucherDate &&
                    v.SourceType == (int)sourceType &&
                    v.VoucherSeries.VoucherSeriesTypeId == voucherSeriesTypeId
                    select v).Any();
        }

        public bool VoucherSerieExists(int voucherSeriesTypeId, int accountYearId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.VoucherSeries.NoTracking();
            return VoucherSerieExists(entities, voucherSeriesTypeId, accountYearId);
        }

        public bool VoucherSerieExists(CompEntities entities, int voucherSeriesTypeId, int accountYearId)
        {
            VoucherSeries voucherSerie = GetVoucherSerieByType(entities, voucherSeriesTypeId, accountYearId);
            return voucherSerie != null;
        }

        public bool VoucherSeriesHasVoucherHeads(CompEntities entities, int voucherSeriesTypeId, int accountYearId)
        {
            int counter = (from vh in entities.VoucherHead
                           where vh.VoucherSeries.VoucherSeriesTypeId == voucherSeriesTypeId &&
                           vh.VoucherSeries.AccountYearId == accountYearId
                           select vh).Count();

            return counter > 0;
        }

        public ActionResult AddVoucherSeries(VoucherSeries voucherSeries, int actorCompanyId, int accountYearId, int voucherSeriesTypeId, int voucherSeriesId = 0, bool ClearSeqNr = false)
        {
            if (voucherSeries == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "VoucherSeries");

            using (CompEntities entities = new CompEntities())
            {
                return AddVoucherSeries(entities, voucherSeries, actorCompanyId, accountYearId, voucherSeriesTypeId, voucherSeriesId, ClearSeqNr);
            }
        }

        public ActionResult AddVoucherSeries(CompEntities entities, VoucherSeries voucherSeries, int actorCompanyId, int accountYearId, int voucherSeriesTypeId, int voucherSeriesId = 0, bool ClearSeqNr = false)
        {
            voucherSeries.AccountYear = AccountManager.GetAccountYear(entities, accountYearId, true);
            if (voucherSeries.AccountYear == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, "AccountYear");

            voucherSeries.VoucherSeriesType = GetVoucherSeriesType(entities, voucherSeriesTypeId, actorCompanyId, true);
            if (voucherSeries.VoucherSeriesType == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, "VoucherSeriesType");

            // Load existing VoucherSerie
            VoucherSeries existingVoucherSeries = GetVoucherSerie(entities, voucherSeriesId, actorCompanyId, true);

            // Need to subtract with one in order to get correct startnumber
            if (ClearSeqNr)
            {
                voucherSeries.VoucherNrLatest = 0;
                voucherSeries.VoucherSeriesType.StartNr = 0;
            }
            else
            {
                voucherSeries.VoucherNrLatest = existingVoucherSeries != null ? existingVoucherSeries.VoucherNrLatest : voucherSeries.VoucherSeriesType.StartNr - 1;
            }

            return AddEntityItem(entities, voucherSeries, "VoucherSeries");
        }

        public ActionResult UpdateVoucherSeries(VoucherSeries voucherSeries, int actorCompanyId)
        {
            if (voucherSeries == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "VoucherSeries");

            using (CompEntities entities = new CompEntities())
            {
                VoucherSeries originalVoucherSerie = GetVoucherSerie(entities, voucherSeries.VoucherSeriesId, actorCompanyId);
                if (originalVoucherSerie == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "VoucherSeries");

                return this.UpdateEntityItem(entities, originalVoucherSerie, voucherSeries, "VoucherSeries");
            }
        }

        public ActionResult DeleteVoucherSeries(int voucherSeriesId, int actorCompanyId)
        {
            using (CompEntities entities = new CompEntities())
            {
                VoucherSeries voucherSerie = GetVoucherSerie(entities, voucherSeriesId, actorCompanyId);
                if (voucherSerie == null)
                    return new ActionResult((int)ActionResultDelete.EntityNotFound, "VoucherSeries");

                //Check relation dependencies
                if (VoucherSeriesHasVoucherHeads(entities, voucherSerie.VoucherSeriesTypeId, voucherSerie.AccountYearId))
                    return new ActionResult((int)ActionResultDelete.VoucherSeriesHasVoucherSeries);

                return DeleteEntityItem(entities, voucherSerie);
            }
        }

        public ActionResult DeleteVoucherSeriesForYear(int accountYearId, int actorCompanyId)
        {
            using (CompEntities entities = new CompEntities())
            {
                ActionResult result = new ActionResult(true);

                // Get all voucher series for specified account year
                List<VoucherSeries> voucherSeries = GetVoucherSeriesByYear(accountYearId, actorCompanyId, true);

                // Loop through voucher series
                foreach (VoucherSeries voucherSerie in voucherSeries)
                {
                    // Delete voucher serie
                    result = DeleteVoucherSeries(voucherSerie.VoucherSeriesId, actorCompanyId);
                    if (!result.Success)
                        return result;
                }

                return result;
            }
        }

        public ActionResult CopyVoucherSeries(int actorCompanyId, int accountYearIdFrom, int accountYearIdTo, bool ClearSeqNr)
        {
            ActionResult result = new ActionResult(true);

            try
            {
                using (CompEntities entities = new CompEntities())
                {
                    List<VoucherSeries> voucherSeries = GetVoucherSeriesByYear(entities, accountYearIdFrom, accountYearIdTo, actorCompanyId, true);
                    foreach (VoucherSeries voucherSerie in voucherSeries)
                    {
                        if (VoucherSerieExists(entities, voucherSerie.VoucherSeriesTypeId, accountYearIdTo))
                        {
                            continue;
                        }

                        VoucherSeries newVoucherSerie = new VoucherSeries()
                        {
                            Status = (int)SoeEntityState.Active,
                            VoucherDateLatest = null,
                            VoucherNrLatest = 0,
                        };

                        result = AddVoucherSeries(newVoucherSerie, actorCompanyId, accountYearIdTo, voucherSerie.VoucherSeriesTypeId, voucherSerie.VoucherSeriesId, ClearSeqNr);
                    }
                }
            }
            catch (Exception ex)
            {
                base.LogError(ex, this.log);
                return new ActionResult(false);
            }

            return result;
        }

        #endregion

        #region VoucherHead

        public List<CompanyGroupTransferHeadDTO> GetCompanyGroupTransferHistoryResult(int accountYearId, int actorCompanyId, int transferType)
        {
            var dtos = new List<CompanyGroupTransferHeadDTO>();
            using var entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            var items = (from view in entitiesReadOnly.CompanyGroupTransferHistoryView
                         where view.ActorCompanyId == actorCompanyId &&
                         view.TransferType == transferType
                         orderby view.TransferDate descending
                         select view);

            foreach (var view in items)
            {
                if (!view.OnlyVoucher)
                {
                    var dto = dtos.FirstOrDefault(t => t.CompanyGroupTransferHeadId == view.CompanyGroupTransferHeadId && !t.IsOnlyVoucher);
                    if (dto == null)
                    {
                        dto = new CompanyGroupTransferHeadDTO()
                        {
                            CompanyGroupTransferHeadId = view.CompanyGroupTransferHeadId,
                            ActorCompanyId = view.ActorCompanyId,
                            AccountYearId = view.AccountYearId,
                            AccountYearText = view.AccountYearFrom.ToShortDateString() + " - " + view.AccountYearTo.ToShortDateString(),
                            FromAccountPeriodId = view.FromAccountPeriodId,
                            FromAccountPeriodText = view.FromAccountPeriodStart.HasValue ? view.FromAccountPeriodStart.Value.Year.ToString() + " - " + view.FromAccountPeriodStart.Value.Month.ToString() : "",
                            ToAccountPeriodId = view.ToAccountPeriodId,
                            ToAccountPeriodText = view.ToAccountPeriodStart.HasValue ? view.FromAccountPeriodStart.Value.Year.ToString() + " - " + view.ToAccountPeriodStart.Value.Month.ToString() : "",
                            TransferDate = view.TransferDate,
                            TransferType = view.TransferType.HasValue ? (CompanyGroupTransferType)view.TransferType.Value : CompanyGroupTransferType.None,
                            TransferStatus = view.TransferStatus.HasValue ? (CompanyGroupTransferStatus)view.TransferStatus.Value : CompanyGroupTransferStatus.None,
                            IsOnlyVoucher = false,
                            CompanyGroupTransferRows = new List<CompanyGroupTransferRowDTO>(),
                        };

                        dto.TransferTypeName = dto.TransferType == CompanyGroupTransferType.Consolidation ? GetText(2303, "Utfall") : GetText(7148, "Budget");

                        if (dto.TransferStatus == CompanyGroupTransferStatus.Transfered)
                            dto.TransferStatusName = GetText(8847, "Överförd");
                        else if (dto.TransferStatus == CompanyGroupTransferStatus.PartlyDeleted)
                            dto.TransferStatusName = GetText(8848, "Delvis borttagen");
                        else
                            dto.TransferStatusName = GetText(2244, "Borttagen");

                        dtos.Add(dto);
                    }

                    var row = new CompanyGroupTransferRowDTO()
                    {
                        CompanyGroupTransferRowId = view.CompanyGroupTransferRowId,
                        //ChildActorCompanyId = view.co
                        ChildActorCompanyNrName = view.CompanyNr.HasValue ? view.CompanyNr.Value.ToString() + " - " + view.CompanyName : view.CompanyName,
                        Status = view.VoucherHeadId == 0 ? GetText(2244, "Borttagen") : GetText(8847, "Överförd"),
                        VoucherHeadId = view.VoucherHeadId,
                        VoucherNr = view.VoucherNr,
                        VoucherText = view.Text,
                        VoucherSeriesId = view.VoucherSeriesId,
                        VoucherSeriesName = view.VoucherSeriesName,
                        ConversionFactor = view.ConversionFactor,
                        Created = view.TransferDate.Value,
                        AccountPeriodText = view.AccountPeriodFrom.HasValue ? view.AccountPeriodFrom.Value.Year.ToString() + " - " + view.AccountPeriodFrom.Value.Month.ToString() : " ",
                    };

                    dto.CompanyGroupTransferRows.Add(row);
                }
                else
                {
                    CompanyGroupTransferHeadDTO dto = new CompanyGroupTransferHeadDTO()
                    {
                        CompanyGroupTransferHeadId = view.CompanyGroupTransferHeadId,
                        ActorCompanyId = view.ActorCompanyId,
                        AccountYearId = view.AccountYearId,
                        AccountYearText = view.AccountYearFrom.ToShortDateString() + " - " + view.AccountYearTo.ToShortDateString(),
                        FromAccountPeriodId = view.FromAccountPeriodId,
                        FromAccountPeriodText = view.AccountPeriodFrom.HasValue ? view.AccountPeriodFrom.Value.Year.ToString() + " - " + view.AccountPeriodFrom.Value.Month.ToString() : "",
                        ToAccountPeriodId = view.ToAccountPeriodId,
                        ToAccountPeriodText = view.AccountPeriodTo.HasValue ? view.AccountPeriodTo.Value.Year.ToString() + " - " + view.AccountPeriodTo.Value.Month.ToString() : "",
                        TransferType = view.TransferType.HasValue ? (CompanyGroupTransferType)view.TransferType.Value : CompanyGroupTransferType.None,
                        TransferTypeName = GetText(2303, "Utfall"),
                        TransferStatus = view.TransferStatus.HasValue ? (CompanyGroupTransferStatus)view.TransferStatus.Value : CompanyGroupTransferStatus.None,
                        TransferStatusName = GetText(8847, "Överförd"),
                        //TransferDate = view.Tr
                        IsOnlyVoucher = false,
                        CompanyGroupTransferRows = new List<CompanyGroupTransferRowDTO>(),
                    };

                    var row = new CompanyGroupTransferRowDTO()
                    {
                        CompanyGroupTransferRowId = view.CompanyGroupTransferRowId,
                        //ChildActorCompanyId = view.co
                        ChildActorCompanyNrName = view.CompanyNr.Value.ToString() + " - " + view.CompanyName,
                        Status = GetText(8847, "Överförd"),//view.Status == (int)SoeEntityState.Deleted ? GetText(2244, (int)TermGroup.General, "Borttagen") : GetText(8847, (int)TermGroup.General, "Överförd"),
                        VoucherHeadId = view.VoucherHeadId,
                        VoucherNr = view.VoucherNr,
                        VoucherText = view.Text,
                        VoucherSeriesId = view.VoucherSeriesId,
                        VoucherSeriesName = view.VoucherSeriesName,
                        ConversionFactor = view.ConversionFactor,
                        Created = view.TransferDate.Value,
                    };

                    dto.CompanyGroupTransferRows.Add(row);

                    dtos.Add(dto);
                }

            }

            return dtos;
        }

        public List<VoucherHead> GetVoucherHeads(int accountYearId, int actorCompanyId, bool includeTemplate, bool loadVoucherSeries = false, int voucherSeriesTypeId = 0)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();

            entitiesReadOnly.VoucherHead.NoTracking();
            IQueryable<VoucherHead> query = entitiesReadOnly.VoucherHead;
            if (loadVoucherSeries)
                query = query.Include("VoucherSeries.VoucherSeriesType");

            if (voucherSeriesTypeId > 0)
            {
                return (from vh in query
                        where vh.ActorCompanyId == actorCompanyId &&
                        vh.AccountPeriod.AccountYearId == accountYearId &&
                        (includeTemplate || !vh.Template) &&
                        vh.VoucherSeries.VoucherSeriesTypeId == voucherSeriesTypeId
                        orderby vh.VoucherSeries.VoucherSeriesType.Name, vh.VoucherNr descending
                        select vh).ToList();
            }
            else
            {
                return (from vh in query
                        where vh.ActorCompanyId == actorCompanyId &&
                        vh.AccountPeriod.AccountYearId == accountYearId &&
                        (includeTemplate || !vh.Template)
                        orderby vh.VoucherSeries.VoucherSeriesType.Name, vh.VoucherNr descending
                        select vh).ToList();
            }
        }

        public List<VoucherGridDTO> GetVoucherHeadsForGrid(int accountYearId, int actorCompanyId, bool includeTemplate, int voucherSeriesTypeId, int? voucherHeadId = null)
        {
            using var entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            IQueryable<VoucherHeadGridView> query = (from vh in entitiesReadOnly.VoucherHeadGridView
                                                     where vh.ActorCompanyId == actorCompanyId &&
                                                           vh.AccountYearId == accountYearId
                                                     orderby vh.VoucherSeriesTypeName, vh.VoucherNr descending
                                                     select vh);
            if (!includeTemplate)
                query = query.Where(x => !x.Template);

            if (voucherSeriesTypeId > 0)
                query = query.Where(x => x.VoucherSeriesTypeId == voucherSeriesTypeId);
            if (voucherHeadId.HasValue)
            {
                query = query.Where(x => x.VoucherHeadId == voucherHeadId.Value);
            }

            List<VoucherGridDTO> dtos = query.Select(e => new VoucherGridDTO
            {
                VoucherHeadId = e.VoucherHeadId,
                VoucherNr = e.VoucherNr,
                Date = e.Date,
                Text = e.Text,
                VatVoucher = e.VatVoucher,
                VoucherSeriesTypeId = e.VoucherSeriesTypeId,
                VoucherSeriesTypeName = e.VoucherSeriesTypeName,
                SourceType = e.SourceType,
                Modified = e.Modified,
                HasDocuments = e.HasDocuments == 1,
                HasHistoryRows = e.HasHistoryRows == 1,
                HasNoRows = !e.NbrOfRows.HasValue || e.NbrOfRows == 0,
                HasUnbalancedRows = e.SumOfRows.HasValue && e.SumOfRows != 0
            }).ToList();

            List<GenericType> sourceTypes = GetTermGroupContent(TermGroup.VoucherHeadSourceType, skipUnknown: true);
            foreach (VoucherGridDTO dto in dtos)
            {
                GenericType sourceType = sourceTypes.FirstOrDefault(s => s.Id == dto.SourceType);
                if (sourceType != null)
                    dto.SourceTypeName = sourceType.Name;
            }

            return dtos;
        }

        public List<VoucherHead> GetVoucherHeadsByPeriod(CompEntities entities, int accountPeriodId, int actorCompanyId)
        {
            return (from vh in entities.VoucherHead
                    where vh.AccountPeriodId == accountPeriodId &&
                    vh.ActorCompanyId == actorCompanyId
                    select vh).ToList();
        }

        public List<VoucherHead> GetVoucherHeadsByCompany(int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.VoucherHead.NoTracking();
            return GetVoucherHeadsByCompany(entities, actorCompanyId);
        }

        public List<VoucherHead> GetVoucherHeadsByCompany(CompEntities entities, int actorCompanyId)
        {
            return (from vh in entities.VoucherHead
                    .Include("AccountPeriod.AccountYear")
                    where vh.ActorCompanyId == actorCompanyId
                    select vh).ToList();
        }

        public List<VoucherHeadDTO> GetVoucherHeadDTOs(int actorCompanyId, DateTime dateFrom, DateTime dateTo, VoucherHeadFilter filter = null)
        {
            #region Prereq

            List<AccountDim> accountDims = AccountManager.GetAccountDimInternalsByCompany(actorCompanyId, true);

            int accounDim2Id = 0;
            int accounDim3Id = 0;
            int accounDim4Id = 0;
            int accounDim5Id = 0;
            int accounDim6Id = 0;
            int accountDimCounter = 2;

            //Number the AccountDims
            if (accountDims.Any())
            {
                accountDims = accountDims.OrderBy(a => a.AccountDimNr).ToList();

                foreach (var accountDimId in accountDims.OrderBy(a => a.AccountDimNr).Select(x => x.AccountDimId).ToList())
                {
                    if (accountDimCounter == 2) accounDim2Id = accountDimId;
                    if (accountDimCounter == 3) accounDim3Id = accountDimId;
                    if (accountDimCounter == 4) accounDim4Id = accountDimId;
                    if (accountDimCounter == 5) accounDim5Id = accountDimId;
                    if (accountDimCounter == 6) accounDim6Id = accountDimId;

                    accountDimCounter++;
                }
            }

            #endregion

            using (CompEntities entities = new CompEntities())
            {
                entities.CommandTimeout = 200;

                IQueryable<VoucherHead> voucherHeadsQuery = (from vh in entities.VoucherHead
                                                             where vh.ActorCompanyId == actorCompanyId &&
                                                                   vh.Date >= dateFrom && vh.Date <= dateTo
                                                             select vh);

                IQueryable<VoucherRowAccountView> voucherRowsQuery = (from vr in entities.VoucherRowAccountView
                                                                      where vr.ActorCompanyId == actorCompanyId &&
                                                                            vr.HDate >= dateFrom && vr.HDate <= dateTo &&
                                                                            vr.State == (int)SoeEntityState.Active
                                                                      select vr);

                if (filter != null)
                {
                    if (filter.VoucherNrFrom.HasValue && filter.VoucherNrTo.HasValue)
                    {
                        voucherHeadsQuery = voucherHeadsQuery.Where(vh => vh.VoucherNr >= filter.VoucherNrFrom.Value && vh.VoucherNr <= filter.VoucherNrTo.Value);
                        voucherRowsQuery = voucherRowsQuery.Where(vh => vh.VoucherNr >= filter.VoucherNrFrom.Value && vh.VoucherNr <= filter.VoucherNrTo.Value);
                    }

                    if (filter.VoucherHeadId.HasValue)
                    {
                        voucherHeadsQuery = voucherHeadsQuery.Where(vh => vh.VoucherHeadId == filter.VoucherHeadId);
                        voucherRowsQuery = voucherRowsQuery.Where(vh => vh.VoucherHeadId == filter.VoucherHeadId);
                    }

                    if (filter.VoucherSeriesId.HasValue)
                    {
                        voucherHeadsQuery = voucherHeadsQuery.Where(vh => vh.VoucherSeriesId == filter.VoucherSeriesId);
                    }

                    if (filter.AccountYearId.HasValue)
                    {
                        voucherHeadsQuery = voucherHeadsQuery.Where(vh => vh.AccountPeriod.AccountYearId == filter.AccountYearId);
                    }
                }

                var voucherHeads = voucherHeadsQuery.Select(vh => new VoucherHeadDTO
                {
                    ActorCompanyId = actorCompanyId,
                    VoucherHeadId = vh.VoucherHeadId,
                    AccountPeriodId = vh.AccountPeriodId,
                    AccountYearId = vh.AccountPeriod.AccountYearId,
                    VoucherNr = vh.VoucherNr,
                    Date = vh.Date,
                    Note = vh.Note,
                    Text = vh.Text,
                    CompanyGroupVoucher = vh.CompanyGroupVoucher ?? false,
                    Template = vh.Template,
                    TypeBalance = vh.TypeBalance,
                    VatVoucher = vh.VatVoucher ?? false,
                    Status = (TermGroup_AccountStatus)vh.Status,
                    Created = vh.Created,
                    CreatedBy = vh.CreatedBy,
                    Modified = vh.Modified,
                    ModifiedBy = vh.ModifiedBy,
                    VoucherSeriesId = vh.VoucherSeriesId,
                    VoucherSeriesTypeId = vh.VoucherSeries.VoucherSeriesTypeId,
                    VoucherSeriesTypeNr = vh.VoucherSeries.VoucherSeriesType.VoucherSeriesTypeNr,
                    VoucherSeriesTypeName = vh.VoucherSeries.VoucherSeriesType.Name,
                }).ToLookup(p => new { p.VoucherHeadId });

                var voucherRows = voucherRowsQuery.ToList();

                foreach (var rowsByHead in voucherRows.GroupBy(x => x.VoucherHeadId))
                {
                    var headRow = rowsByHead.First();
                    var headDto = voucherHeads[new { headRow.VoucherHeadId }].FirstOrDefault(); //.FirstOrDefault(h=> h.VoucherHeadId == headRow.VoucherHeadId);
                    if (headDto == null)
                    {
                        continue;
                    }
                    headDto.Rows = new List<VoucherRowDTO>();

                    foreach (var rowsByRow in rowsByHead.GroupBy(x => x.VoucherRowId))
                    {
                        var row = rowsByRow.First();

                        var rowDTO = new VoucherRowDTO
                        {
                            VoucherRowId = row.VoucherRowId,
                            VoucherHeadId = row.VoucherHeadId,
                            ParentRowId = row.ParentRowId,
                            AccountDistributionHeadId = row.AccountDistributionHeadId,
                            Date = row.RDate ?? headDto.Date,
                            Text = string.IsNullOrEmpty(row.RText) ? headDto.Text : row.RText,
                            Quantity = row.Quantity,
                            Amount = row.Amount,
                            AmountEntCurrency = row.AmountEntCurrency,
                            Merged = row.Merged,
                            State = SoeEntityState.Active,
                            VoucherNr = headDto.VoucherNr,
                            VoucherSeriesTypeNr = headDto.VoucherSeriesTypeNr,
                            VoucherSeriesTypeName = headDto.VoucherSeriesTypeName,
                            Dim1Id = row.Dim1Id,
                            Dim1Nr = row.Dim1Nr,
                            Dim1Name = row.Dim1Name,
                            Dim1UnitStop = row.Dim1UnitStop,
                            Dim1AmountStop = row.Dim1AmountStop,
                            RowNr = row.RowNr,
                            SysVatAccountId = row.SysVatAccountId,
                            Dim1AccountType = row.Dim1AccountType,
                        };

                        var accountInternalDTOs = new List<AccountInternalDTO>();

                        //Add Internal Accounts
                        foreach (var accountTrans in rowsByRow)
                        {
                            if (accountTrans.AccountInternalId == 0)
                                continue;

                            var accountInternalDTO = new AccountInternalDTO();

                            accountInternalDTO.AccountNr = accountTrans.AccountInternalNr;
                            accountInternalDTO.AccountId = accountTrans.AccountInternalId;
                            accountInternalDTO.Name = accountTrans.AccountInternalName;

                            if (accountTrans.AccountInternalDimId == accounDim2Id)
                            {
                                rowDTO.Dim2Id = accountTrans.AccountInternalId;
                                rowDTO.Dim2Nr = accountTrans.AccountInternalNr;
                                rowDTO.Dim2Name = accountTrans.AccountInternalName;
                                accountInternalDTO.AccountDimId = accounDim2Id;
                                accountInternalDTO.AccountDimNr = accountDims.FirstOrDefault(d => d.AccountDimId == accounDim2Id).AccountDimNr;
                                accountInternalDTO.SysSieDimNr = accountDims.FirstOrDefault(d => d.AccountDimId == accounDim2Id).SysSieDimNr;
                            }

                            if (accountTrans.AccountInternalDimId == accounDim3Id)
                            {
                                rowDTO.Dim3Id = accountTrans.AccountInternalId;
                                rowDTO.Dim3Nr = accountTrans.AccountInternalNr;
                                rowDTO.Dim3Name = accountTrans.AccountInternalName;
                                accountInternalDTO.AccountDimId = accounDim3Id;
                                accountInternalDTO.AccountDimNr = accountDims.FirstOrDefault(d => d.AccountDimId == accounDim3Id).AccountDimNr;
                                accountInternalDTO.SysSieDimNr = accountDims.FirstOrDefault(d => d.AccountDimId == accounDim3Id).SysSieDimNr;
                            }

                            if (accountTrans.AccountInternalDimId == accounDim4Id)
                            {
                                rowDTO.Dim4Id = accountTrans.AccountInternalId;
                                rowDTO.Dim4Nr = accountTrans.AccountInternalNr;
                                rowDTO.Dim4Name = accountTrans.AccountInternalName;
                                accountInternalDTO.AccountDimId = accounDim4Id;
                                accountInternalDTO.AccountDimNr = accountDims.FirstOrDefault(d => d.AccountDimId == accounDim4Id).AccountDimNr;
                                accountInternalDTO.SysSieDimNr = accountDims.FirstOrDefault(d => d.AccountDimId == accounDim4Id).SysSieDimNr;
                            }

                            if (accountTrans.AccountInternalDimId == accounDim5Id)
                            {
                                rowDTO.Dim5Id = accountTrans.AccountInternalId;
                                rowDTO.Dim5Nr = accountTrans.AccountInternalNr;
                                rowDTO.Dim5Name = accountTrans.AccountInternalName;
                                accountInternalDTO.AccountDimId = accounDim5Id;
                                accountInternalDTO.AccountDimNr = accountDims.FirstOrDefault(d => d.AccountDimId == accounDim5Id).AccountDimNr;
                                accountInternalDTO.SysSieDimNr = accountDims.FirstOrDefault(d => d.AccountDimId == accounDim5Id).SysSieDimNr;
                            }

                            if (accountTrans.AccountInternalDimId == accounDim6Id)
                            {
                                rowDTO.Dim6Id = accountTrans.AccountInternalId;
                                rowDTO.Dim6Nr = accountTrans.AccountInternalNr;
                                rowDTO.Dim6Name = accountTrans.AccountInternalName;
                                accountInternalDTO.AccountDimId = accounDim6Id;
                                accountInternalDTO.AccountDimNr = accountDims.FirstOrDefault(d => d.AccountDimId == accounDim6Id).AccountDimNr;
                                accountInternalDTO.SysSieDimNr = accountDims.FirstOrDefault(d => d.AccountDimId == accounDim6Id).SysSieDimNr;
                            }

                            //dum fix for errors when we have saved multiple of same dimension on one voucherrow..
                            //the voucher reg page will show the last so lets used the last here also..
                            var existing = accountInternalDTOs.FirstOrDefault(x => x.AccountDimId == accountInternalDTO.AccountDimId);
                            if (existing != null)
                            {
                                accountInternalDTOs.Remove(existing);
                            }
                            accountInternalDTOs.Add(accountInternalDTO);
                        }

                        rowDTO.AccountInternalDTO_forReports = accountInternalDTOs;

                        headDto.Rows.Add(rowDTO);
                    }
                }
                return voucherHeads.SelectMany(x => x.Select(y => y)).Where(x => x.Rows != null).ToList();
                //return voucherHeads.Where(x=> x.Rows != null).ToList();
            }
        }

        public List<VoucherHeadDTO> GetVoucherHeadDTOsFromSelection(EvaluatedSelection es, AccountDimDTO accountDimStd = null, bool orderByVoucherNr = false)
        {
            List<VoucherHeadDTO> validVoucherHeads = new List<VoucherHeadDTO>();
            List<VoucherSeries> voucherSeries = VoucherManager.GetVoucherSeries(es.ActorCompanyId, false);

            #region Prereq

            if (accountDimStd == null)
                accountDimStd = AccountManager.GetAccountDimStd(es.ActorCompanyId).ToDTO();
            if (accountDimStd == null)
                return validVoucherHeads;


            DateTime dateFrom = es.DateFrom;
            DateTime dateTo = es.DateTo != CalendarUtility.DATETIME_DEFAULT ? es.DateTo : DateTime.Now.AddYears(100);

            List<VoucherHeadDTO> allVoucherHeads = new List<VoucherHeadDTO>();

            if (es.SV_HasVoucherHeadIds)
            {

                foreach (int voucherHeadId in es.SV_VoucherHeadIds)
                {
                    allVoucherHeads.AddRange(VoucherManager.GetVoucherHeadDTOs(es.ActorCompanyId, dateFrom, dateTo, new VoucherHeadFilter { VoucherHeadId = voucherHeadId }));
                }
            }
            else
            {
                if (!es.SV_HasVoucherNrInterval)
                    allVoucherHeads = VoucherManager.GetVoucherHeadDTOs(es.ActorCompanyId, dateFrom, dateTo);
                else
                    allVoucherHeads = VoucherManager.GetVoucherHeadDTOs(es.ActorCompanyId, dateFrom, dateTo, new VoucherHeadFilter { VoucherNrFrom = es.SV_VoucherNrFrom, VoucherNrTo = es.SV_VoucherNrTo });
            }

            #endregion

            foreach (VoucherHeadDTO voucherHead in allVoucherHeads)
            {
                #region Validate VoucherHead

                if (es.SV_HasVoucherHeadIds)
                {
                    bool voucherHeadValid = es.SV_VoucherHeadIds.Contains(voucherHead.VoucherHeadId);
                    if (!voucherHeadValid)
                        continue;
                }

                #endregion

                #region Validate VoucherSerie

                if (es.SV_HasVoucherSeriesTypeNrInterval)
                {
                    bool voucherSerieValid = voucherHead.Template == false && voucherSeries.Any(s => s.VoucherSeriesId == voucherHead.VoucherSeriesId && (s.Status == (int)TermGroup_AccountStatus.Open || s.Status == null || s.Status == 0)) &&
                                             voucherHead.VoucherSeriesTypeNr >= es.SV_VoucherSeriesTypeNrFrom && voucherHead.VoucherSeriesTypeNr <= es.SV_VoucherSeriesTypeNrTo;
                    if (!voucherSerieValid)
                        continue;
                }

                #endregion

                #region Validate Accounts

                if (es.SA_HasAccountInterval)
                {
                    //Selection AccountInterval: Continue if there is no interval, or no VoucherRow have AccountStd/AccountInternal in interval
                    bool accountsValid = VoucherHeadDTOContainsAccounts(voucherHead, es.SA_AccountIntervals, accountDimStd.AccountDimId);
                    if (!accountsValid)
                        continue;
                }

                #endregion

                #region Validate VoucherInterval
                /*SV_HasVoucherInterval must never be true when SV_HasVoucherSeriesTypeNrInterval or SV_HasVoucherHeadIds is true.
				 * SV_HasVoucherInterval is used for new SIE export, the others are used for the old one.
				 */
                if (es.SV_HasVoucherInterval)
                {
                    bool voucherHeadValid = es.SV_VoucherIntervals.Any(i =>
                        i.VoucherNoFrom <= voucherHead.VoucherNr &&
                        i.VoucherNoTo >= voucherHead.VoucherNr &&
                        i.VoucherSerieId == voucherHead.VoucherSeriesId
                    );
                    if (!voucherHeadValid)
                        continue;
                }
                #endregion

                validVoucherHeads.Add(voucherHead);
            }

            #region Sort

            if (orderByVoucherNr)
            {
                validVoucherHeads = (from vh in validVoucherHeads
                                     orderby vh.VoucherSeriesTypeNr ascending, vh.VoucherNr ascending
                                     select vh).ToList();
            }
            else
            {
                validVoucherHeads = (from vh in validVoucherHeads
                                     orderby vh.VoucherSeriesTypeNr ascending, vh.Date ascending, vh.VoucherNr ascending
                                     select vh).ToList();
            }

            #endregion

            return validVoucherHeads;
        }

        public List<VoucherHeadDTO> GetVoucherHeadDTOsFromSelection(CreateReportResult reportResult, EconomyReportParamsDTO reportParams, AccountDimDTO accountDimStd = null, bool orderByVoucherNr = false)
        {
            List<VoucherHeadDTO> validVoucherHeads = new List<VoucherHeadDTO>();
            List<VoucherSeries> voucherSeries = VoucherManager.GetVoucherSeries(reportResult.ActorCompanyId, false);

            #region Prereq

            if (accountDimStd == null)
                accountDimStd = AccountManager.GetAccountDimStd(reportResult.ActorCompanyId).ToDTO();
            if (accountDimStd == null)
                return validVoucherHeads;


            DateTime dateFrom = reportParams.DateFrom;
            DateTime dateTo = reportParams.DateTo != CalendarUtility.DATETIME_DEFAULT ? reportParams.DateTo : DateTime.Now.AddYears(100);

            List<VoucherHeadDTO> allVoucherHeads = new List<VoucherHeadDTO>();

            if (reportParams.SV_HasVoucherHeadIds)
            {
                foreach (int voucherHeadId in reportParams.SV_VoucherHeadIds)
                {
                    allVoucherHeads.AddRange(VoucherManager.GetVoucherHeadDTOs(reportResult.ActorCompanyId, dateFrom, dateTo, new VoucherHeadFilter { VoucherHeadId = voucherHeadId }));
                }
            }
            else
            {
                if (!reportParams.SV_HasVoucherNrInterval)
                    allVoucherHeads = VoucherManager.GetVoucherHeadDTOs(reportResult.ActorCompanyId, dateFrom, dateTo);
                else
                    allVoucherHeads = VoucherManager.GetVoucherHeadDTOs(reportResult.ActorCompanyId, dateFrom, dateTo, new VoucherHeadFilter { VoucherNrFrom = reportParams.SV_VoucherNrFrom, VoucherNrTo = reportParams.SV_VoucherNrTo });
            }

            #endregion

            foreach (VoucherHeadDTO voucherHead in allVoucherHeads)
            {
                #region Validate VoucherHead

                if (reportParams.SV_HasVoucherHeadIds)
                {
                    bool voucherHeadValid = reportParams.SV_VoucherHeadIds.Contains(voucherHead.VoucherHeadId);
                    if (!voucherHeadValid)
                        continue;
                }

                #endregion

                #region Validate VoucherSerie

                if (reportParams.SV_HasVoucherSeriesTypeNrInterval)
                {
                    bool voucherSerieValid = voucherHead.Template == false && voucherSeries.Any(s => s.VoucherSeriesId == voucherHead.VoucherSeriesId && (s.Status == (int)TermGroup_AccountStatus.Open || s.Status == null || s.Status == 0)) &&
                                             voucherHead.VoucherSeriesTypeNr >= reportParams.SV_VoucherSeriesTypeNrFrom && voucherHead.VoucherSeriesTypeNr <= reportParams.SV_VoucherSeriesTypeNrTo;
                    if (!voucherSerieValid)
                        continue;
                }

                #endregion

                #region Validate Accounts

                if (reportParams.SA_HasAccountInterval)
                {
                    //Selection AccountInterval: Continue if there is no interval, or no VoucherRow have AccountStd/AccountInternal in interval
                    bool accountsValid = VoucherHeadDTOContainsAccounts(voucherHead, reportParams.SA_AccountIntervals, accountDimStd.AccountDimId);
                    if (!accountsValid)
                        continue;
                }

                #endregion

                #region Load VoucherRows

                #endregion

                validVoucherHeads.Add(voucherHead);
            }

            #region Sort

            if (orderByVoucherNr)
            {
                validVoucherHeads = (from vh in validVoucherHeads
                                     orderby vh.VoucherSeriesTypeNr ascending, vh.VoucherNr ascending
                                     select vh).ToList();
            }
            else
            {
                validVoucherHeads = (from vh in validVoucherHeads
                                     orderby vh.VoucherSeriesTypeNr ascending, vh.Date ascending, vh.VoucherNr ascending
                                     select vh).ToList();
            }

            #endregion

            return validVoucherHeads;
        }

        public List<VoucherHead> GetVoucherTemplates(int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.VoucherHead.NoTracking();
            return GetVoucherTemplates(entities, actorCompanyId);
        }

        public List<VoucherHead> GetVoucherTemplates(CompEntities entities, int actorCompanyId)
        {
            return (from vh in entities.VoucherHead
                    .Include("AccountPeriod")
                    .Include("VoucherRow")
                    .Include("VoucherRow.AccountDistributionHead")
                    .Include("VoucherRow.AccountStd")
                    .Include("VoucherRow.AccountStd.Account")
                    .Include("VoucherSeries.VoucherSeriesType")
                    .Include("VoucherSeries.AccountYear")
                    where vh.ActorCompanyId == actorCompanyId &&
                          vh.VoucherSeries.VoucherSeriesType.State == (int)SoeEntityState.Active &&
                          vh.Template
                    orderby vh.VoucherNr ascending
                    select vh).ToList();
        }

        public IQueryable<VoucherHead> GetVoucherTemplatesByYear(int accountYearId, int actorCompanyId, bool loadVoucherSeries = false, bool loadVoucherRow = false, int? voucherHeadId = null)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.VoucherHead.NoTracking();
            return GetVoucherTemplatesByYear(entities, accountYearId, actorCompanyId, loadVoucherSeries, loadVoucherRow, voucherHeadId);
        }

        public IQueryable<VoucherHead> GetVoucherTemplatesByYear(CompEntities entities, int accountYearId, int actorCompanyId, bool loadVoucherSeries = false, bool loadVoucherRow = false, int? voucherHeadId = null)
        {
            IQueryable<VoucherHead> query = (from vh in entities.VoucherHead
                                             where vh.AccountPeriod.AccountYearId == accountYearId &&
                                             vh.VoucherSeries.VoucherSeriesType.ActorCompanyId == actorCompanyId &&
                                             vh.VoucherSeries.VoucherSeriesType.State == (int)SoeEntityState.Active &&
                                             vh.Template
                                             select vh);

            if (loadVoucherSeries)
                query = query.Include("VoucherSeries.VoucherSeriesType");

            if (loadVoucherRow)
                query = query.Include("VoucherRow");

            if (voucherHeadId.HasValue)
                query = query.Where(x => x.VoucherHeadId == voucherHeadId.Value);

            return query;
        }

        public Dictionary<int, string> GetVoucherTemplatesByCompanyDict(int accountYearId, int actorCompanyId)
        {
            return GetVoucherTemplatesByYear(accountYearId, actorCompanyId, false, false).ToDictionary(v => v.VoucherHeadId, v => v.Text);
        }

        //TODO...int to long
        public Dictionary<int, int> GetVouchersDict(List<VoucherHead> voucherHeads)
        {
            var dict = new Dictionary<int, int>();

            if (voucherHeads == null)
                return dict;

            foreach (VoucherHead voucherHead in voucherHeads)
            {
                if (!dict.ContainsKey(voucherHead.VoucherHeadId))
                    dict.Add(voucherHead.VoucherHeadId, Convert.ToInt32(voucherHead.VoucherNr));
            }

            return dict;
        }

        public VoucherHead GetVoucherHead(int voucherHeadId, bool loadVoucherSeries = false, bool loadVoucherRow = false, bool loadVoucherRowAccounts = false, bool loadAccountBalance = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.VoucherHead.NoTracking();
            return GetVoucherHead(entities, voucherHeadId, loadVoucherSeries, loadVoucherRow, loadVoucherRowAccounts, loadAccountBalance);
        }

        public VoucherHead GetVoucherHead(CompEntities entities, int voucherHeadId, bool loadVoucherSeries = false, bool loadVoucherRow = false, bool loadVoucherRowAccounts = false, bool loadAccountBalance = false)
        {
            if (voucherHeadId == 0)
                return null;

            VoucherHead voucherHead = null;
            if (loadVoucherSeries && loadVoucherRow && loadVoucherRowAccounts)
            {
                voucherHead = (from vh in entities.VoucherHead
                                .Include("VoucherSeries.VoucherSeriesType")
                                .Include("VoucherRow.AccountStd.Account")
                                .Include("VoucherRow.AccountInternal.Account.AccountDim")
                               where vh.VoucherHeadId == voucherHeadId && vh.ActorCompanyId == ActorCompanyId
                               select vh).FirstOrDefault();
            }
            else if (loadVoucherSeries)
            {
                voucherHead = (from vh in entities.VoucherHead
                                .Include("VoucherSeries.VoucherSeriesType")
                               where vh.VoucherHeadId == voucherHeadId && vh.ActorCompanyId == ActorCompanyId
                               select vh).FirstOrDefault();
            }
            else if (loadVoucherRow)
            {
                if (loadVoucherRowAccounts)
                {
                    voucherHead = (from vh in entities.VoucherHead
                                    .Include("VoucherSeries.VoucherSeriesType")
                                    .Include("VoucherRow.AccountStd.Account")
                                    .Include("VoucherRow.AccountInternal.Account.AccountDim")
                                   where vh.VoucherHeadId == voucherHeadId && vh.ActorCompanyId == ActorCompanyId
                                   select vh).FirstOrDefault();
                }
                else
                {
                    voucherHead = (from vh in entities.VoucherHead
                                    .Include("VoucherSeries.VoucherSeriesType")
                                    .Include("VoucherRow")
                                   where vh.VoucherHeadId == voucherHeadId && vh.ActorCompanyId == ActorCompanyId
                                   select vh).FirstOrDefault();
                }
            }
            else
            {
                voucherHead = (from vh in entities.VoucherHead
                               .Include("VoucherSeries.VoucherSeriesType")
                               where vh.VoucherHeadId == voucherHeadId && vh.ActorCompanyId == ActorCompanyId
                               select vh).FirstOrDefault();
            }


            // Cannot make include on AccountBalance since EF will make an inner join,
            // In that case the row itself will not be returned if it has no balance.
            if (voucherHead != null && voucherHead.VoucherRow != null && loadAccountBalance)
            {
                LoadAccountBalance(voucherHead);
            }

            if (voucherHead != null && voucherHead.SourceType > 0)
                voucherHead.SourceTypeName = TermCacheManager.Instance.GetText(voucherHead.SourceType, (int)TermGroup.VoucherHeadSourceType, string.Empty);

            return voucherHead;
        }

        public VoucherHead GetVoucherHeadByNr(int voucherNr, int voucherSeriesId, int actorCompanyId, bool loadVoucherSeries = false, bool loadVoucherRow = false, bool loadVoucherRowAccounts = false, bool loadAccountBalance = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.VoucherHead.NoTracking();
            return GetVoucherHeadByNr(entities, voucherNr, voucherSeriesId, actorCompanyId, loadVoucherSeries, loadVoucherRow, loadVoucherRowAccounts, loadAccountBalance);
        }

        public VoucherHead GetVoucherHeadByNr(CompEntities entities, int voucherNr, int voucherSeriesId, int actorCompanyId, bool loadVoucherSeries = false, bool loadVoucherRow = false, bool loadVoucherRowAccounts = false, bool loadAccountBalance = false)
        {
            VoucherHead voucherHead = null;
            if (loadVoucherSeries && loadVoucherRow && loadVoucherRowAccounts)
            {
                voucherHead = (from vh in entities.VoucherHead
                                .Include("VoucherSeries.VoucherSeriesType")
                                .Include("VoucherRow.AccountStd.Account")
                                .Include("VoucherRow.AccountInternal.Account.AccountDim")
                               where vh.VoucherNr == voucherNr &&
                               vh.ActorCompanyId == actorCompanyId &&
                               vh.VoucherSeriesId == voucherSeriesId
                               select vh).FirstOrDefault();
            }
            else if (loadVoucherSeries)
            {
                voucherHead = (from vh in entities.VoucherHead
                                .Include("VoucherSeries.VoucherSeriesType")
                               where vh.VoucherNr == voucherNr &&
                               vh.ActorCompanyId == actorCompanyId &&
                               vh.VoucherSeriesId == voucherSeriesId
                               select vh).FirstOrDefault();
            }
            else if (loadVoucherRow)
            {
                if (loadVoucherRowAccounts)
                {
                    voucherHead = (from vh in entities.VoucherHead
                                    .Include("VoucherRow.AccountStd.Account")
                                    .Include("VoucherRow.AccountInternal.Account.AccountDim")
                                   where vh.VoucherNr == voucherNr &&
                                   vh.ActorCompanyId == actorCompanyId &&
                                   vh.VoucherSeriesId == voucherSeriesId
                                   select vh).FirstOrDefault();
                }
                else
                {
                    voucherHead = (from vh in entities.VoucherHead
                                    .Include("VoucherRow")
                                   where vh.VoucherNr == voucherNr &&
                                   vh.ActorCompanyId == actorCompanyId &&
                                   vh.VoucherSeriesId == voucherSeriesId
                                   select vh).FirstOrDefault();
                }
            }
            else
            {
                voucherHead = (from vh in entities.VoucherHead
                               where vh.VoucherNr == voucherNr &&
                               vh.ActorCompanyId == actorCompanyId &&
                               vh.VoucherSeriesId == voucherSeriesId
                               select vh).FirstOrDefault();
            }

            if (loadAccountBalance)
                LoadAccountBalance(voucherHead);

            return voucherHead;
        }

        public VoucherHead GetVatVoucherHead(CompEntities entities, int voucherHeadId, int actorCompanyId)
        {
            return (from vh in entities.VoucherHead
                    where vh.VoucherHeadId == voucherHeadId &&
                    vh.ActorCompanyId == actorCompanyId &&
                    vh.VatVoucher == true
                    select vh).FirstOrDefault();
        }

        public VoucherHead GetVatVoucherHeadByPeriod(int accountPeriodId, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.VoucherHead.NoTracking();
            return GetVatVoucherHeadByPeriod(entities, accountPeriodId, actorCompanyId);
        }

        public VoucherHead GetVatVoucherHeadByPeriod(CompEntities entities, int accountPeriodId, int actorCompanyId)
        {
            return (from vh in entities.VoucherHead
                    where vh.AccountPeriodId == accountPeriodId &&
                    vh.ActorCompanyId == actorCompanyId &&
                    vh.VatVoucher == true
                    select vh).FirstOrDefault();
        }

        public VoucherHead GetVatVoucherHeadLaterThanPeriod(int accountPeriodId, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.VoucherHead.NoTracking();
            return GetVatVoucherHeadLaterThanPeriod(entities, accountPeriodId, actorCompanyId);
        }

        public VoucherHead GetVatVoucherHeadLaterThanPeriod(CompEntities entities, int accountPeriodId, int actorCompanyId)
        {
            AccountPeriod accountPeriod = AccountManager.GetAccountPeriod(entities, accountPeriodId);
            if (accountPeriod == null)
                return null;

            return (from vh in entities.VoucherHead
                    where vh.ActorCompanyId == actorCompanyId &&
                    vh.Date > accountPeriod.To &&
                    vh.VatVoucher == true
                    orderby vh.Date
                    select vh).FirstOrDefault();
        }

        public List<VoucherHead> GetVoucherHeadPeriodFromPeriodTo(CompEntities entities, DateTime? dateFrom, DateTime? dateTo, int actorCompanyId)
        {
            return (from vh in entities.VoucherHead
                                .Include("VoucherRow.AccountStd.Account")
                                .Include("VoucherRow.AccountInternal.Account.AccountDim")
                    where vh.ActorCompanyId == actorCompanyId &&
                    vh.Date >= dateFrom.Value &&
                    vh.Date <= dateTo.Value &&
                    vh.Template == false
                    orderby vh.Date
                    select vh).ToList();
        }

        public bool VatVoucherExists(CompEntities entities, int accountPeriodId, int actorCompanyId)
        {
            VoucherHead voucherHead = GetVatVoucherHeadByPeriod(entities, accountPeriodId, actorCompanyId);
            return voucherHead != null;
        }

        public bool VoucherHeadDTOContainsAccounts(VoucherHeadDTO voucherHead, List<AccountIntervalDTO> accountIntervals, int accountDimStdId)
        {
            //Approve all if no filter given
            if (accountIntervals == null || accountIntervals.Count == 0)
                return true;

            foreach (AccountIntervalDTO accountInterval in accountIntervals)
            {
                foreach (VoucherRowDTO voucherRow in voucherHead.Rows)
                {
                    if (VoucherRowDTOContainsAccount(voucherRow, accountInterval, accountDimStdId))
                        return true;
                }
            }

            return false;
        }

        public void LoadAccountBalance(VoucherHead voucherHead)
        {
            // Cannot make include on AccountBalance since EF will make an inner join,
            // In that case the row itself will not be returned if it has no balance.
            if (voucherHead != null && voucherHead.VoucherRow != null)
            {
                foreach (VoucherRow voucherRow in voucherHead.VoucherRow)
                {
                    if (!voucherRow.AccountStdReference.IsLoaded)
                        voucherRow.AccountStdReference.Load();

                    if (!voucherRow.AccountStd.AccountBalance.IsLoaded)
                        voucherRow.AccountStd.AccountBalance.Load();
                }
            }
        }

        public ActionResult SaveVoucher(VoucherHeadDTO voucherInput, List<AccountingRowDTO> accountingRowDTOsInput, List<int> householdRowIds, int? revertVatVoucherId, int actorCompanyId, bool isImport = false, bool useAccountDistribution = false)
        {
            return SaveVoucher(voucherInput, accountingRowDTOsInput, householdRowIds, Enumerable.Empty<FileUploadDTO>(), revertVatVoucherId, actorCompanyId, isImport, useAccountDistribution);
        }

        public ActionResult SaveVoucher(VoucherHeadDTO voucherInput, List<AccountingRowDTO> accountingRowDTOsInput, List<int> householdRowIds, IEnumerable<FileUploadDTO> attachments, int? revertVatVoucherId, int actorCompanyId, bool isImport = false, bool useAccountDistribution = false)
        {
            if (voucherInput == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "VoucherHead");

            var result = new ActionResult(true);

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    if (entities.Connection.State != ConnectionState.Open)
                        entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        result = SaveVoucher(entities, transaction, voucherInput, accountingRowDTOsInput, householdRowIds, revertVatVoucherId, actorCompanyId, isImport, false, useAccountDistribution, useVoucherLock: true);

                        if (result.Success)
                        {
                            var voucherHeadId = result.IntegerValue;
                            var resultAttachments = SaveVoucherAttachements(entities, voucherHeadId, attachments);
                            if (!resultAttachments.Success)
                            {
                                result = resultAttachments;
                            }
                        }

                        //Commit transaction
                        if (result.Success)
                            transaction.Complete();
                    }
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    result.Exception = ex;
                    result.IntegerValue = 0;
                    result.Value = 0;
                }
                finally
                {
                    if (result.Success)
                    {
                        //Set success properties - already set
                    }
                    else
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }

                return result;
            }
        }

        private ActionResult SaveVoucherAttachements(CompEntities entities, int voucherHeadId, IEnumerable<FileUploadDTO> files)
        {
            files = files ?? Enumerable.Empty<FileUploadDTO>();
            if (files.Any())
            {
                GeneralManager.UpdateFiles(entities, files, voucherHeadId);
                return SaveChanges(entities);
            }

            return new ActionResult(true);
        }

        public ActionResult SaveVoucher(CompEntities entities, TransactionScope transaction, VoucherHeadDTO voucherInput, List<AccountingRowDTO> accountingRowDTOsInput, List<int> householdRowIds, int? revertVatVoucherId, int actorCompanyId, bool isImport, bool useRowsFromHead = false, bool useAccountDistribution = false, bool useVoucherLock = false)
        {
            // Default result is successful
            bool newVoucher = false;

            long seqNbr = 0;

            bool voucherDateChanged = false, voucherTextChanged = false;
            DateTime voucherDateOld = DateTime.Now, voucherDateNew = DateTime.Now;
            string voucherTextOld = "", voucherTextNew = "";
            // Get internal accounts (Dim2-6)
            var accountInternals = AccountManager.GetAccountInternals(entities, actorCompanyId, true);

            #region Prereq

            // Get VoucherSeries
            VoucherSeries voucherSeries = GetVoucherSerie(entities, voucherInput.VoucherSeriesId, actorCompanyId, true);
            if (voucherSeries == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, "VoucherSeries");

            // Get AccountPeriod
            AccountPeriod accountPeriod = AccountManager.GetAccountPeriod(entities, voucherInput.AccountPeriodId);
            if (accountPeriod == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, "AccountPeriod");

            int accountYearId = accountPeriod.AccountYearId;
            bool useQuantityInVoucher = SettingManager.GetBoolSetting(entities, SettingMainType.Company, (int)CompanySettingType.AccountingUseQuantityInVoucher, 0, actorCompanyId, 0);
            bool allowUnbalancedVouchers = SettingManager.GetBoolSetting(entities, SettingMainType.Company, (int)CompanySettingType.AccountingAllowUnbalancedVoucher, 0, actorCompanyId, 0);

            List<AccountDim> accountDims = AccountManager.GetAccountDimsByCompany(entities, actorCompanyId);
            List<AccountStd> accountStds = AccountManager.GetAccountStdsByCompany(entities, actorCompanyId, null, true, false);

            #endregion

            #region Validate accounting rows

            if (!isImport && !allowUnbalancedVouchers && ValidateAccountingRows(accountingRowDTOsInput))
            {
                return new ActionResult((int)ActionResultSave.HasUnbalancedAccountingRows, GetText(7406, "Debet och kredit balanserar inte. Kontrollera konteringsrader och spara igen."));
            }

            #endregion

            #region Convert

            // Convert collection of AccountingRowDTOs to collection of VoucherRows
            List<VoucherRow> voucherRowsInput = useRowsFromHead ?
                CreateVoucherRows(entities, voucherInput, base.ActorCompanyId, accountStds, accountInternals).ToList() :
                ConvertToVoucherRows(entities, accountingRowDTOsInput, voucherInput, actorCompanyId, accountStds, accountInternals).ToList();

            #endregion

            if (isImport && useAccountDistribution)
                voucherRowsInput = VoucherManager.ApplyAutomaticAccountDistribution(entities, voucherRowsInput, accountStds, accountDims, accountInternals, actorCompanyId, true, null);

            #region VatVoucher

            //When reverting it should not be possible to save voucher as VAT voucher
            if (revertVatVoucherId.HasValue && revertVatVoucherId.Value > 0)
            {
                VoucherHead vatVoucherHead = GetVatVoucherHead(entities, revertVatVoucherId.Value, actorCompanyId);
                if (vatVoucherHead != null)
                {
                    vatVoucherHead.VatVoucher = false;
                    SetModifiedProperties(vatVoucherHead);
                }

                voucherInput.VatVoucher = false;
            }

            #endregion

            #region VoucherHead

            // Get existing Voucher
            VoucherHead voucherHead = GetVoucherHead(entities, voucherInput.VoucherHeadId, false, true, true, false);
            if (voucherHead == null)
            {
                #region VoucherHead Add

                newVoucher = true;

                if (voucherInput.Template)
                {
                    // Templates are stored against the first period found in current account year
                    accountPeriod = AccountManager.GetFirstAccountPeriod(entities, accountYearId, actorCompanyId, true);
                    voucherSeries = GetTemplateVoucherSerie(entities, accountYearId, actorCompanyId);
                }

                // Always get the next available sequence number
                // A check will be made in the GUI if the number has changed from the proposed one
                seqNbr = voucherSeries.VoucherNrLatest.GetValueOrDefault() + 1;

                //if imported voucher, voucherNr will be used
                if (voucherInput.VoucherNr != 0 && isImport)
                    seqNbr = voucherInput.VoucherNr;

                voucherHead = new VoucherHead
                {
                    VoucherNr = seqNbr,
                    Date = voucherInput.Date.Date,
                    Text = voucherInput.Text,
                    Template = voucherInput.Template,
                    VatVoucher = voucherInput.VatVoucher,
                    Status = (int)TermGroup_AccountStatus.Open,
                    SourceType = (int)voucherInput.SourceType,

                    //Set FK
                    ActorCompanyId = actorCompanyId,

                    //Set referenecs
                    AccountPeriod = accountPeriod,
                    VoucherSeries = voucherSeries,

                    //Note
                    Note = voucherInput.Note,
                };
                SetCreatedProperties(voucherHead);
                entities.VoucherHead.AddObject(voucherHead);

                // Update voucher serie with last voucher nr and date
                voucherSeries.VoucherNrLatest = seqNbr;
                voucherSeries.VoucherDateLatest = voucherInput.Date;

                #endregion
            }
            else
            {
                #region VoucherHead Update

                //Check if voucher date or voucher text is changed               
                if (voucherHead.Date != voucherInput.Date)
                {
                    voucherDateChanged = true;
                    voucherDateOld = voucherHead.Date;
                    voucherDateNew = voucherInput.Date;
                    accountPeriod = AccountManager.GetAccountPeriod(entities, voucherDateNew, ActorCompanyId);
                }
                if (voucherHead.Text != voucherInput.Text)
                {
                    voucherTextChanged = true;
                    voucherTextOld = voucherHead.Text;
                    voucherTextNew = voucherInput.Text;
                }

                //We have change accounting year and ned new SeqNr and create a "placeholder" for the old number
                if (voucherDateChanged && accountPeriod.AccountYearId != voucherHead.AccountPeriod.AccountYearId)
                {
                    var accountYear = AccountManager.GetAccountYear(entities, accountPeriod.AccountYearId);
                    seqNbr = (voucherSeries.VoucherNrLatest.HasValue ? voucherSeries.VoucherNrLatest.Value : 0) + 1;
                    var placeHolderVoucherHead = new VoucherHead
                    {
                        VoucherNr = voucherHead.VoucherNr,
                        Date = voucherHead.Date,
                        Text = string.Format(GetText(7518, "Verifikatet har flyttats till redovisningsår {0}, vernr {1}"), accountYear.GetFromToShortString(), seqNbr),
                        VatVoucher = voucherHead.VatVoucher,
                        Status = (int)TermGroup_AccountStatus.Open,
                        SourceType = voucherHead.SourceType,

                        //Set FK
                        ActorCompanyId = actorCompanyId,

                        //Set referenecs
                        AccountPeriod = voucherHead.AccountPeriod,
                        VoucherSeries = voucherHead.VoucherSeries,
                        VoucherSeriesId = voucherHead.VoucherSeriesId
                    };
                    SetCreatedProperties(placeHolderVoucherHead);
                    entities.VoucherHead.AddObject(placeHolderVoucherHead);

                    voucherHead.VoucherNr = seqNbr;
                    voucherHead.VoucherSeriesId = voucherSeries.VoucherSeriesId;
                    voucherHead.VoucherSeries = voucherSeries;

                    // Update voucher serie with last voucher nr and date
                    voucherSeries.VoucherNrLatest = seqNbr;
                    voucherSeries.VoucherDateLatest = voucherInput.Date;
                }

                newVoucher = false;
                voucherHead.Date = voucherInput.Date.Date;
                voucherHead.AccountPeriodId = accountPeriod.AccountPeriodId;
                voucherHead.Text = voucherInput.Text;
                voucherHead.SourceType = (int)voucherInput.SourceType;
                voucherHead.VatVoucher = voucherInput.VatVoucher;
                voucherHead.Note = voucherInput.Note;
                SetModifiedProperties(voucherHead);

                seqNbr = voucherHead.VoucherNr;

                #endregion
            }

            if (voucherHead.Template)
            {
                var nameIsTaken = GetVoucherTemplatesByYear(entities, accountYearId, actorCompanyId)
                    .Any(v => v.Text == voucherHead.Text && v.VoucherHeadId != voucherHead.VoucherHeadId);

                if (nameIsTaken)
                {
                    return new ActionResult((int)ActionResultSave.VoucherExists, GetText(7401, "Det finns redan en verifikatmall med den texten.")); // There is already a template with that text...
                }
            }

            #endregion

            #region VoucherRow

            #region VoucherRow Update/Delete

            // Update or Delete existing VoucherRows
            foreach (VoucherRow voucherRow in voucherHead.ActiveVoucherRows)
            {
                // Try get VoucherRow from input
                VoucherRow voucherRowInput = (from r in voucherRowsInput
                                              where r.VoucherRowId == voucherRow.VoucherRowId
                                              select r).FirstOrDefault();


                if (voucherRowInput != null)
                {
                    #region VoucherRow Update

                    if (!voucherHead.Template)
                    {
                        // Update voucher row history
                        AddVoucherRowHistory(entities, voucherRow, voucherRowInput, TermGroup_VoucherRowHistoryEvent.Modified, actorCompanyId);

                        //Add history row also when voucher date is changed
                        if (voucherDateChanged)
                        {
                            var voucherRowHistory = new VoucherRowHistory
                            {
                                Date = DateTime.Now,
                                UserId = base.UserId,
                                AccountStd = voucherRow.AccountStd,
                                EventType = (int)TermGroup_VoucherRowHistoryEvent.Modified,
                                VoucherHeadIdModified = voucherHead.VoucherHeadId,
                                FieldModified = (int)TermGroup_VoucherRowHistoryField.VoucherDate,
                                EventText = voucherDateOld.ToShortDateString() + " --> " + voucherDateNew.ToShortDateString(),
                            };

                            voucherRow.VoucherRowHistory.Add(voucherRowHistory);

                            voucherDateChanged = false;
                        }

                        //Add history row also when voucher text is changed
                        if (voucherTextChanged)
                        {
                            string textFrom = voucherTextOld == string.Empty ? "<" + GetText(3003, "blankt") + "> " : voucherTextOld;
                            string textTo = voucherTextNew == string.Empty ? " <" + GetText(3003, "blankt") + ">" : voucherTextNew;

                            VoucherRowHistory voucherRowHistory = new VoucherRowHistory()
                            {
                                Date = DateTime.Now,
                                UserId = base.UserId,
                                AccountStd = voucherRow.AccountStd,
                                EventType = (int)TermGroup_VoucherRowHistoryEvent.Modified,
                                VoucherHeadIdModified = voucherHead.VoucherHeadId,
                                FieldModified = (int)TermGroup_VoucherRowHistoryField.VoucherText,
                                EventText = textFrom + " --> " + textTo,
                            };
                            voucherRow.VoucherRowHistory.Add(voucherRowHistory);

                            voucherTextChanged = false;
                        }

                    }

                    // Update existing voucher row
                    voucherRow.Date = voucherRowInput.Date.HasValue ? voucherRowInput.Date.Value.Date : (DateTime?)null;
                    voucherRow.AccountStd = voucherRowInput.AccountStd;
                    voucherRow.Text = voucherRowInput.Text;
                    voucherRow.Quantity = voucherRowInput.Quantity;
                    voucherRow.Amount = voucherRowInput.Amount;
                    voucherRow.AmountEntCurrency = voucherRowInput.AmountEntCurrency;
                    voucherRow.AccountDistributionHeadId = voucherRowInput.AccountDistributionHeadId;
                    voucherRow.RowNr = voucherRowInput.RowNr;
                    voucherRow.NumberOfPeriods = voucherRowInput.NumberOfPeriods;
                    voucherRow.StartDate = voucherRowInput.StartDate;

                    // Update AccountInternal
                    voucherRow.AccountInternal.Clear();
                    foreach (AccountInternal accountInternal in voucherRowInput.AccountInternal)
                    {
                        voucherRow.AccountInternal.Add(accountInternal);
                    }

                    // Detach the input row to prevent adding a new
                    base.TryDetachEntity(entities, voucherRowInput);

                    #endregion
                }
                else
                {
                    #region VoucherRow Delete

                    // Delete existing VoucherRow
                    if (voucherRow.State != (int)SoeEntityState.Deleted)
                    {
                        ChangeEntityState(voucherRow, SoeEntityState.Deleted);

                        // Add VoucherRowHistory
                        AddVoucherRowHistory(entities, voucherRow, voucherRowInput, TermGroup_VoucherRowHistoryEvent.Removed, actorCompanyId);
                    }

                    #endregion
                }
            }

            #endregion

            #region VoucherRow Add

            // Get new VoucherRows
            IEnumerable<VoucherRow> voucherRowsToAdd = (from r in voucherRowsInput
                                                        where r.VoucherRowId == 0
                                                        select r).ToList();

            foreach (VoucherRow voucherRowToAdd in voucherRowsToAdd)
            {
                // Add VoucherRow to VoucherHead
                voucherHead.VoucherRow.Add(voucherRowToAdd);

                if (!voucherHead.Template && !newVoucher)
                {
                    // Add VoucherRowHistory
                    AddVoucherRowHistory(entities, voucherRowToAdd, voucherRowToAdd, TermGroup_VoucherRowHistoryEvent.New, actorCompanyId);
                }
            }

            #endregion

            #endregion            

            #region HouseholdTaxDeductionRow

            // Create relation to HouseholdTaxDeductionRow
            if (householdRowIds != null && householdRowIds.Any())
            {
                foreach (int rowId in householdRowIds)
                {
                    HouseholdTaxDeductionRow row = HouseholdTaxDeductionManager.GetHouseholdTaxDeductionRow(entities, rowId);
                    if (row != null)
                    {
                        row.VoucherHead = voucherHead;
                        SetModifiedProperties(row);
                    }
                }
            }

            #endregion

            #region validate voucher rows

            if (!isImport && !allowUnbalancedVouchers && ValidateVoucherRows(voucherHead.ActiveVoucherRows.ToList()))
            {
                return new ActionResult((int)ActionResultSave.HasUnbalancedAccountingRows, "Voucher");
            }

            #endregion

            if (newVoucher && useVoucherLock)
            {
                //Use the locking mechanism to make sure the voucher number is unique
                //This should be done before any writes are done to the VoucherSeries table.
                new VoucherNumberLock(entities)
                    .AddVoucher(voucherHead)
                    .SetVoucherNumbers();

                seqNbr = voucherHead.VoucherNr;
            }

            var result = SaveChanges(entities, transaction);
            if (result.Success)
            {
                //voucher successfully saved, create account distribution entries
                #region AccountDistributionEntry

                bool saveChanges = false;

                //remove existing entries if voucher row deleted
                List<VoucherRow> deletedRows = voucherHead.VoucherRow.Where(i => i.State == (int)SoeEntityState.Deleted && i.AccountDistributionHeadId != 0).ToList();
                foreach (var deletedRow in deletedRows)
                {
                    //set previously created entries to state deleted
                    List<AccountDistributionEntry> existingEntries = AccountDistributionManager.GetAccountDistributionEntriesForSourceRow(entities, actorCompanyId, (int)TermGroup_AccountDistributionRegistrationType.Voucher, voucherHead.VoucherHeadId, deletedRow.VoucherRowId).ToList();
                    bool transferredEntries = existingEntries.Where(i => i.VoucherHeadId != null).ToList().Count > 0;
                    if (!transferredEntries)
                    {
                        foreach (var entry in existingEntries)
                        {
                            entry.State = (int)SoeEntityState.Deleted;
                            SetModifiedProperties(entry);
                            saveChanges = true;
                        }
                    }
                }

                var voucherSeriesType = GetVoucherSeriesType(entities, voucherSeries.VoucherSeriesTypeId, actorCompanyId);
                string voucherSeriesTypeName = voucherSeriesType != null ? voucherSeriesType.Name : "";

                string accrualName = $"{GetText(1152, "Verifikat")} {voucherHead?.VoucherNr}, {voucherSeriesTypeName}, {DateTime.Today.ToString()}";
                saveChanges = AccountDistributionManager.CreateAccrualsForAccountingRows(entities, transaction, actorCompanyId, voucherHead.VoucherHeadId, TermGroup_AccountDistributionRegistrationType.Voucher, accrualName).Success;
                if (saveChanges)
                    result = SaveChanges(entities, transaction);

                #endregion


                //Set success properties
                result.IntegerValue = voucherHead.VoucherHeadId;
                result.Value = seqNbr;
                result.StringValue = voucherHead.Text;

            }

            return result;
        }

        public ActionResult SaveVoucherFromCustomerInvoices(CompEntities entities, TransactionScopeOption transactionScopeOption, List<CustomerInvoice> invoices, int actorCompanyId)
        {
            if (invoices == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, GetText(8082, "Underlaget kunde inte hittas"));

            // Default result is successful
            ActionResult result = new ActionResult();

            // Handle unbalanced
            bool hasInvalidInvoices = false;
            Dictionary<int, string> invalidInvoices = new Dictionary<int, string>();
            List<VoucherHead> voucherHeads = new List<VoucherHead>();

            try
            {
                if (entities.Connection.State != ConnectionState.Open)
                    entities.Connection.Open();

                #region Settings

                //Quantity
                bool useQuantityInVoucher = SettingManager.GetBoolSetting(entities, SettingMainType.Company, (int)CompanySettingType.AccountingUseQuantityInVoucher, 0, actorCompanyId, 0);

                //VoucherSeries
                int customerInvoiceSeriesTypeId = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.CustomerInvoiceVoucherSeriesType, 0, actorCompanyId, 0);

                //Merge Invoice to VoucherHead
                int invoiceToVoucherHeadType = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.CustomerInvoiceToVoucherHeadType, 0, actorCompanyId, 0);
                if (invoiceToVoucherHeadType == (int)SoeInvoiceToVoucherHeadType.None)
                    invoiceToVoucherHeadType = (int)SoeInvoiceToVoucherHeadType.VoucherPerInvoice;

                //Merge Invoice to VoucherRow
                int invoiceToVoucherRowType = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.CustomerInvoiceToVoucherRowType, 0, actorCompanyId, 0);
                if (invoiceToVoucherRowType == (int)SoeInvoiceToVoucherRowType.None)
                    invoiceToVoucherRowType = (int)SoeInvoiceToVoucherRowType.VoucherRowPerInvoiceRow;

                bool useAccountDistribution = SettingManager.GetBoolSetting(entities, SettingMainType.Company, (int)CompanySettingType.CustomerInvoiceUseAutoAccountDistributionOnVoucher, 0, actorCompanyId, 0);

                #endregion

                // Possible to include this method in a running Transaction
                using (TransactionScope transaction = new TransactionScope(transactionScopeOption, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                {
                    #region Prereq
                    AccountYear accountYear = null;
                    VoucherSeries voucherSeries = null;
                    AccountPeriod accountPeriod = null;

                    List<AccountDim> accountDims = useAccountDistribution ? AccountManager.GetAccountDimsByCompany(entities, actorCompanyId) : null;
                    List<AccountStd> accountStds = useAccountDistribution ? AccountManager.GetAccountStdsByCompany(entities, actorCompanyId, null, true, false) : null;
                    List<AccountInternal> accountInternals = useAccountDistribution ? AccountManager.GetAccountInternals(entities, actorCompanyId, true) : null;

                    int voucherHeadCounter = 0;

                    #endregion

                    foreach (CustomerInvoice invoice in invoices.OrderBy(x => x.VoucherDate))
                    {
                        #region Prereq

                        var voucherDate = invoice.VoucherDate.HasValue ? invoice.VoucherDate.Value : DateTime.Now;
                        var invoiceVoucherSeriesTypeId = invoice.Origin.VoucherSeriesTypeId ?? customerInvoiceSeriesTypeId;

                        if (!AccountManager.ValidateAccountYear(accountYear, voucherDate).Success)
                        {
                            //Validate AccountYear
                            accountYear = AccountManager.GetAccountYear(entities, voucherDate, actorCompanyId);
                            result = AccountManager.ValidateAccountYear(accountYear);
                            if (!result.Success)
                            {
                                result.ErrorMessage = InvoiceManager.GetErrorMessage(result.ErrorNumber, accountYear.From.ToShortDateString() + "-" + accountYear.To.ToShortDateString());
                                return result;
                            }

                            //Get VoucherSerie for CustomerInvoice for current AccountYear
                            voucherSeries = VoucherManager.GetVoucherSerieByType(entities, invoiceVoucherSeriesTypeId, accountYear.AccountYearId);

                            if (voucherSeries == null)
                                return new ActionResult((int)ActionResultSave.EntityNotFound, "VoucherSeries");

                            //Validate AccountYear
                            if (accountYear.Status != (int)TermGroup_AccountStatus.Open)
                            {
                                result.Success = false;
                                result.ErrorMessage = GetText(1994, "Året stängt") + " " + accountYear.From.ToShortDateString() + "-" + accountYear.To.ToShortDateString();
                                result.ErrorNumber = (int)ActionResultSave.AccountYearNotOpen;
                                return result;
                            }

                            //Validate AccountPeriod
                            if (!AccountManager.ValidateAccountPeriod(accountPeriod, voucherDate).Success)
                            {
                                accountPeriod = AccountManager.GetAccountPeriod(entities, accountYear.AccountYearId, voucherDate, actorCompanyId);
                                result = AccountManager.ValidateAccountPeriod(accountPeriod, voucherDate);
                                if (!result.Success)
                                    return result;
                            }
                        }

                        //Can only get status Voucher from Origin.
                        if (invoice.Origin.Status != (int)SoeOriginStatus.Origin || invoice.VoucherHeadId != null)
                        {
                            result.Success = false;
                            result.ErrorNumber = (int)ActionResultSave.InvalidStateTransition;
                            result.ErrorMessage = $"{GetText(176, "Felaktig statusförändring")}: {(string.IsNullOrEmpty(invoice.InvoiceNr) ? invoice.CustomerName : invoice.InvoiceNr)}";
                            return result;
                        }

                        Dictionary<int, List<int>> seqNbrDict = new Dictionary<int, List<int>>();
                        int seqNbr = invoice.SeqNr.Value;

                        #endregion

                        #region Validate 

                        if (InvoiceManager.ValidateCustomerInvoiceAccountingRowsDiff(invoice))
                        {
                            hasInvalidInvoices = true;
                            invalidInvoices.Add(invoice.InvoiceId, invoice.InvoiceNr);
                            continue;
                        }

                        #endregion

                        #region VoucherHead

                        /*
                         * Verifikathuvudtexten som inte är sammanslagna ska vara på formatet Kundfakt. (Faktnr), Kund, ev fakturatext ska inte läggas ut
                         *Sammanslagna på formatet  Kundfakt. (Faktnr), (Faktnr), (Faktnr)
                         */

                        VoucherHead voucherHead = null;

                        voucherHeadCounter++;
                        //bool foundExistingVoucherHead = false;

                        #region MergeVoucherOnInvoiceDate

                        if (invoiceToVoucherHeadType == (int)SoeInvoiceToVoucherHeadType.MergeVoucherOnVoucherDate && voucherHeads.Count > 0)
                        {
                            voucherHead = voucherHeads.FirstOrDefault(i => i.Date == voucherDate);
                            if (voucherHead != null)
                            {
                                //Text
                                voucherHead.Text += ", " + invoice.InvoiceNr;

                                //foundExistingVoucherHead = true;
                            }
                        }

                        #endregion

                        #region VoucherPerInvoice (or no matching Voucher found)

                        if (invoiceToVoucherHeadType == (int)SoeInvoiceToVoucherHeadType.VoucherPerInvoice || voucherHead == null)
                        {
                            //Text
                            string voucherHeadText = GetText(1818, "Kundfakt.") + " " + invoice.InvoiceNr;
                            if (invoiceToVoucherHeadType == (int)SoeInvoiceToVoucherHeadType.VoucherPerInvoice || invoices.Count == 1)
                            {
                                //Add additional info if not merging
                                if (!String.IsNullOrEmpty(invoice.ActorName))
                                    voucherHeadText += ", " + invoice.ActorName;
                            }

                            // invoice.Origin.Description can be to big for voucherheadtext, so trim this down.
                            if (voucherHeadText.Length > 375)
                                voucherHeadText = voucherHeadText.Substring(0, 375);

                            //Create VoucherHead
                            voucherHead = new VoucherHead()
                            {
                                VoucherNr = voucherSeries.VoucherNrLatest.HasValue ? voucherSeries.VoucherNrLatest.Value + 1 : 1,
                                Date = invoice.VoucherDate.HasValue ? invoice.VoucherDate.Value.Date : DateTime.Today,
                                Text = voucherHeadText,

                                //Set FK
                                ActorCompanyId = actorCompanyId,

                                //Set references
                                VoucherSeries = voucherSeries,
                                AccountPeriod = accountPeriod,
                            };
                            SetCreatedProperties(voucherHead);

                            //Update VoucherSeries
                            voucherSeries.VoucherNrLatest = voucherHead.VoucherNr;
                            voucherSeries.VoucherDateLatest = voucherHead.Date;

                            //Update VoucherDate on Invoice
                            invoice.VoucherDate = voucherHead.Date;

                            //Set status on VoucherHead to the same as its AccountPeriod
                            voucherHead.Status = voucherHead.AccountPeriod.Status;

                            voucherHeads.Add(voucherHead);
                        }

                        #endregion

                        #endregion

                        int vocherRowNr = 1;

                        var voucherRows = new List<VoucherRow>();
                        foreach (CustomerInvoiceRow invoiceRow in invoice.ActiveCustomerInvoiceRows)
                        {
                            #region CustomerInvoiceRow

                            foreach (CustomerInvoiceAccountRow invoiceAccountRow in invoiceRow.ActiveCustomerInvoiceAccountRows.OrderBy(o => o.RowNr))
                            {
                                #region CustomerInvoiceAccountRow

                                #region Prereq

                                //Make sure AccountStd is loaded
                                if (!invoiceAccountRow.AccountStdReference.IsLoaded)
                                    invoiceAccountRow.AccountStdReference.Load();

                                //Make sure Account is loaded
                                if (!invoiceAccountRow.AccountStd.AccountReference.IsLoaded)
                                    invoiceAccountRow.AccountStd.AccountReference.Load();

                                VoucherRow voucherRow = null;
                                int accountId = invoiceAccountRow.AccountStd.AccountId;

                                if (invoiceToVoucherRowType == (int)SoeInvoiceToVoucherRowType.MergeVoucherRowsOnAccount && string.IsNullOrEmpty(invoiceAccountRow.Text))
                                {
                                    //Find existing VoucherRow on VoucherHead
                                    voucherRow = GetVoucherRowFromCustomerInvoiceAccountRow(voucherHead, invoiceAccountRow);
                                    if (voucherRow != null)
                                    {
                                        //Update VoucherRow
                                        voucherRow.Amount += invoiceAccountRow.Amount;
                                        voucherRow.AmountEntCurrency += invoiceAccountRow.AmountEntCurrency;
                                        voucherRow.Quantity += invoiceAccountRow.Quantity;
                                        voucherRow.Merged = true;

                                        //Only add the same sequence number once
                                        if (!seqNbrDict.ContainsKey(accountId) || !seqNbrDict[accountId].Contains(seqNbr) && voucherRow.Text.Length < 500)
                                            voucherRow.Text += ", " + seqNbr.ToString();
                                    }
                                }

                                #endregion

                                if (voucherRow == null)
                                {
                                    #region VoucherRow

                                    /*
                                     * Rad texten på formatet Kundfakt. (Faktnr), ev kund, ev text från radkontering alt text från kundfakthuvudet
                                     */

                                    //Text
                                    string text = GetText(1818, "Kundfakt.") + " " + invoice.InvoiceNr.ToString();
                                    //Add customer if no merging
                                    if ((invoiceToVoucherHeadType == (int)SoeInvoiceToVoucherHeadType.VoucherPerInvoice || invoices.Count == 1) && !string.IsNullOrEmpty(invoice.ActorName))
                                        text += ", " + invoice.ActorName;
                                    //Add text from row, or description from origin
                                    if (!string.IsNullOrEmpty(invoiceAccountRow.Text))
                                        text += ", " + invoiceAccountRow.Text;
                                    //else if ((invoiceToVoucherHeadType == (int)SoeInvoiceToVoucherHeadType.VoucherPerInvoice || invoices.Count() == 1) && !String.IsNullOrEmpty(invoice.Origin.Description))
                                    //    text += ", " + (invoice.Origin.Description.Length > 375 ? invoice.Origin.Description.Substring(0, 375) : invoice.Origin.Description);

                                    //Create VoucherRow
                                    voucherRow = new VoucherRow()
                                    {
                                        Text = text,
                                        Amount = invoiceAccountRow.Amount,
                                        AmountEntCurrency = invoiceAccountRow.AmountEntCurrency,
                                        Quantity = invoiceAccountRow.Quantity.HasValue ? decimal.Round(invoiceAccountRow.Quantity.Value, 6) : invoiceAccountRow.Quantity,
                                        RowNr = vocherRowNr,

                                        //Set references
                                        AccountStd = invoiceAccountRow.AccountStd,
                                    };

                                    vocherRowNr++;

                                    //Set currency amounts
                                    CountryCurrencyManager.SetCurrencyAmounts(entities, actorCompanyId, voucherRow);

                                    if (!invoiceRow.IsAdded() && !invoiceAccountRow.AccountInternal.IsLoaded)
                                        invoiceAccountRow.AccountInternal.Load();

                                    //Set AccountInternals
                                    foreach (AccountInternal accountInternal in invoiceAccountRow.AccountInternal)
                                    {
                                        voucherRow.AccountInternal.Add(accountInternal);
                                    }

                                    //Add CustomerInvoiceAccountRow
                                    if (!useAccountDistribution)
                                        invoiceAccountRow.VoucherRow = voucherRow;

                                    voucherRow.Date = voucherRow.Date.HasValue ? voucherRow.Date : voucherHead.Date;

                                    //Add VoucherRow
                                    voucherHead.VoucherRow.Add(voucherRow);

                                    #endregion

                                    #region AccountBalance

                                    // Update balance on account
                                    //if (voucherHead.Template == false && voucherRow.AccountStd != null)
                                    //{
                                    //Edit NI 100217: Update AccountBalance for all accounts from Silverlight in Complete method
                                    //Update AccountBalance
                                    //abm.SetAccountBalance(entities, actorCompanyId, voucherRow.AccountStd.AccountId, accountYear.AccountYearId, voucherRow.Amount, true, false, accountBalances);
                                    //}

                                    #endregion

                                    #region CuswtomerInvoiceAccountRow

                                    // Set voucher number on accounting row
                                    invoiceAccountRow.Text = String.Format("{0} {1}", GetText(3887, "Ver:"), voucherHead.VoucherNr) + (invoiceAccountRow.Text != null && invoiceAccountRow.Text.Trim() != String.Empty ? " - " + invoiceAccountRow.Text : String.Empty);

                                    #endregion
                                }

                                // Remember current invoice sequence number to prevent multiple numbers on the same voucher row.
                                if (!seqNbrDict.ContainsKey(accountId))
                                    seqNbrDict[accountId] = new List<int>();
                                seqNbrDict[accountId].Add(seqNbr);

                                #endregion
                            }

                            #endregion
                        }

                        #region Status

                        //Update Origin status
                        invoice.Origin.Status = (int)SoeOriginStatus.Voucher;

                        #endregion

                        #region Connect CustomerInvoice to VoucherHead

                        //Add VoucherHead to CustomerInvoice
                        invoice.VoucherHead = voucherHead;

                        //Check if VoucherHead should be added
                        // Add Voucher if:
                        // 1) Setting SoeInvoiceToVoucherHeadType.VoucherPerInvoice
                        // 2) Setting SoeInvoiceToVoucherHeadType.MergeVoucherOnInvoiceDate, but no matching VoucherHead found
                        // 3) Setting SoeInvoiceToVoucherHeadType.MergeVoucherOnInvoiceDate, but is last VoucherHead
                        /*if (!foundExistingVoucherHead || voucherHeadCounter == invoices.Count)
                        {
                            voucherHeads.Add(voucherHead);
                        }*/

                        #endregion
                    }

                    #region Account distribution

                    foreach (var head in voucherHeads)
                    {
                        if (useAccountDistribution)
                        {
                            // Get distributed
                            var distributedRows = VoucherManager.ApplyAutomaticAccountDistribution(entities, head.VoucherRow.ToList(), accountStds, accountDims, accountInternals, actorCompanyId, null, true, invoiceToVoucherRowType == (int)SoeInvoiceToVoucherRowType.MergeVoucherRowsOnAccount);

                            // Empty
                            head.VoucherRow.Clear();

                            // Add distributed
                            head.VoucherRow.AddRange(distributedRows);
                        }

                        //Add VoucherHead
                        entities.VoucherHead.AddObject(head);
                    }

                    #endregion

                    //Lock voucher numbers
                    new VoucherNumberLock(entities)
                        .AddVouchers(voucherHeads)
                        .SetVoucherNumbers();

                    if (result.Success)
                        result = SaveChanges(entities, transaction);

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
                    result.IdDict = GetVouchersDict(voucherHeads);
                    //result.IntegerValue = voucherHeads.Count;

                    if (hasInvalidInvoices)
                    {
                        var count = invalidInvoices.Count;
                        var message = count == 1 ? count + " " + GetText(2302, "faktura kunde inte föras över till verifikat.") : count + " " + GetText(2302, "fakturor kunde inte föras över till verifikat.");
                        message += "\n" + GetText(7406, "Debet och kredit balanserar inte. Kontrollera konteringsrader och spara igen.") + "\n" + GetText(1809, "Fakturor") + ": " + String.Join(", ", invalidInvoices.Select(i => i.Value));
                        result.IntegerValue = invalidInvoices.Count;
                        result.InfoMessage = message;
                    }
                }
                else
                    base.LogTransactionFailed(this.ToString(), this.log);

                if (transactionScopeOption != ConfigSettings.TRANSACTIONSCOPEOPTION_ATTACH)
                    entities.Connection.Close();
            }

            return result;
        }

        public ActionResult SaveVoucherFromSupplierInvoices(CompEntities entities, TransactionScopeOption transactionScopeOption, List<SupplierInvoice> invoices, int actorCompanyId, string failedToTransfer = null)
        {
            if (invoices == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "SupplierInvoice");

            // Default result is successful
            var result = new ActionResult();

            List<VoucherHead> voucherHeads = new List<VoucherHead>();

            bool continueIfError = false;
            int previusAccountYearId = 0;

            //New list of invoices not able to transfer (Angular)
            if (failedToTransfer != null)
                continueIfError = true;

            try
            {
                if (entities.Connection.State != ConnectionState.Open)
                    entities.Connection.Open();

                bool useAccountDistribution = SettingManager.GetBoolSetting(entities, SettingMainType.Company, (int)CompanySettingType.SupplierInvoiceUseAutoAccountDistributionOnVoucher, 0, actorCompanyId, 0);

                // Possible to include this method in a running Transaction
                using (TransactionScope transaction = new TransactionScope(transactionScopeOption, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                {
                    #region Settings

                    //Merge Invoice to VoucherHead
                    int invoiceToVoucherHeadType = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.SupplierInvoiceToVoucherHeadType, 0, actorCompanyId, 0);
                    if (invoiceToVoucherHeadType == (int)SoeInvoiceToVoucherHeadType.None)
                        invoiceToVoucherHeadType = (int)SoeInvoiceToVoucherHeadType.VoucherPerInvoice;

                    //Merge Invoice to VoucherRow
                    int invoiceToVoucherRowType = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.SupplierInvoiceToVoucherRowType, 0, actorCompanyId, 0);
                    if (invoiceToVoucherRowType == (int)SoeInvoiceToVoucherRowType.None)
                        invoiceToVoucherRowType = (int)SoeInvoiceToVoucherRowType.VoucherRowPerInvoiceRow;

                    //VoucherSeries
                    int supplierInvoiceSeriesTypeId = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.SupplierInvoiceVoucherSeriesType, 0, actorCompanyId, 0);

                    #endregion

                    #region Prereq

                    int voucherHeadCounter = 0;

                    List<AccountDim> accountDims = useAccountDistribution ? AccountManager.GetAccountDimsByCompany(entities, actorCompanyId) : null;
                    List<AccountStd> accountStds = useAccountDistribution ? AccountManager.GetAccountStdsByCompany(entities, actorCompanyId, null, true, false) : null;
                    List<AccountInternal> accountInternals = useAccountDistribution ? AccountManager.GetAccountInternals(entities, actorCompanyId, true) : null;

                    #endregion

                    foreach (var invoice in invoices)
                    {
                        #region Prereq

                        //Can only get status Voucher from Origin
                        if (invoice.Origin.Status != (int)SoeOriginStatus.Origin)
                        {
                            if (continueIfError)
                            {
                                failedToTransfer = failedToTransfer + invoice.InvoiceNr + Environment.NewLine;
                                continue;
                            }
                            else
                            {
                                result.Success = false;
                                result.ErrorNumber = (int)ActionResultSave.InvalidStateTransition;
                                return result;
                            }
                        }

                        var invoiceVoucherSeriesTypeId = 0;
                        if (invoice.Origin.VoucherSeriesTypeId != null)
                            invoiceVoucherSeriesTypeId = invoice.Origin.VoucherSeriesTypeId.GetValueOrDefault(0);
                        else
                            invoiceVoucherSeriesTypeId = supplierInvoiceSeriesTypeId;

                        Dictionary<int, List<int>> seqNbrDict = new Dictionary<int, List<int>>();

                        #endregion

                        #region AccountYear

                        var voucherDate = invoice.VoucherDate.HasValue ? invoice.VoucherDate.Value : DateTime.Today;

                        AccountYear accountYear = AccountManager.GetAccountYear(entities, voucherDate, actorCompanyId);
                        result = AccountManager.ValidateAccountYear(accountYear);
                        if (!result.Success)
                            return result;

                        VoucherSeries voucherSeries = null;
                        if (voucherSeries == null || previusAccountYearId != accountYear.AccountYearId || voucherSeries.VoucherSeriesId != invoice.Origin.VoucherSeriesId)
                        {
                            //Get VoucherSerie for SupplierInvoice for current AccountYear
                            voucherSeries = VoucherManager.GetVoucherSerieByType(entities, invoiceVoucherSeriesTypeId, accountYear.AccountYearId);
                        }

                        if (voucherSeries == null)
                            return new ActionResult(8403, GetText(8403, "Verifikatserie saknas"));
                        #endregion

                        #region Validate invoice accounting rows

                        if (SupplierInvoiceManager.ValidateSupplierInvoiceAccountingRowsDiff(invoice))
                        {
                            if (continueIfError)
                            {
                                failedToTransfer = failedToTransfer + invoice.InvoiceNr + Environment.NewLine;
                                continue;
                            }
                            else
                            {
                                return new ActionResult((int)ActionResultSave.HasUnbalancedAccountingRows, "SupplierInvoice");
                            }
                        }

                        #endregion

                        #region VoucherHead

                        /*
                         * Verifikathuvudtexten som inte är sammanslagna ska vara på formatet Lev.fakt. (Löpnr), Leverantör, ev fakturatext skall ej ut i vertext 
                         * Sammanslagna på formatet  Lev.fakt. (Löpnr), (Löpnr), (Löpnr)
                         */

                        VoucherHead voucherHead = null;

                        voucherHeadCounter++;
                        //bool foundExistingVoucherHead = false;

                        #region MergeVoucherOnInvoiceDate

                        if (invoiceToVoucherHeadType == (int)SoeInvoiceToVoucherHeadType.MergeVoucherOnVoucherDate && voucherHeads.Count > 0)
                        {
                            DateTime date = invoice.VoucherDate.HasValue ? invoice.VoucherDate.Value : DateTime.Today;

                            voucherHead = voucherHeads.FirstOrDefault(i => i.Date == date);
                            if (voucherHead != null)
                            {
                                //Text
                                voucherHead.Text += ", " + invoice.SeqNr.Value;

                                //foundExistingVoucherHead = true;
                            }
                        }

                        #endregion

                        #region VoucherPerInvoice (or no matching Voucher found)

                        if (invoiceToVoucherHeadType == (int)SoeInvoiceToVoucherHeadType.VoucherPerInvoice || voucherHead == null)
                        {
                            //Validate AccountPeriod
                            var period = AccountManager.GetAccountPeriod(entities, accountYear.AccountYearId, voucherDate, actorCompanyId);
                            result = AccountManager.ValidateAccountPeriod(period, voucherDate);
                            if (!result.Success)
                            {
                                if (continueIfError)
                                {
                                    failedToTransfer = failedToTransfer + invoice.InvoiceNr + Environment.NewLine;
                                    continue;
                                }
                                else
                                {
                                    return result;
                                }
                            }

                            //Text
                            string voucherHeadText = GetText(1806, "Lev.fakt.") + " " + invoice.SeqNr.Value;
                            if (invoiceToVoucherHeadType == (int)SoeInvoiceToVoucherHeadType.VoucherPerInvoice || invoices.Count == 1)
                            {
                                //Add additional info if not merging
                                if (!string.IsNullOrEmpty(invoice.ActorName))
                                    voucherHeadText += ", " + invoice.ActorName;
                            }

                            //Create VoucherHead
                            voucherHead = new VoucherHead
                            {
                                VoucherNr = voucherSeries.VoucherNrLatest.HasValue ? voucherSeries.VoucherNrLatest.Value + 1 : 1,
                                Date = voucherDate,
                                Text = voucherHeadText,

                                //Set FK
                                ActorCompanyId = actorCompanyId,

                                //Set references
                                VoucherSeries = voucherSeries,

                                // Account period
                                AccountPeriod = period,
                            };
                            SetCreatedProperties(voucherHead);

                            //Update VoucherSeries
                            voucherSeries.VoucherNrLatest = voucherHead.VoucherNr;
                            voucherSeries.VoucherDateLatest = voucherHead.Date;

                            //Set status on VoucherHead to the same as its AccountPeriod
                            voucherHead.Status = voucherHead.AccountPeriod.Status;

                            voucherHeads.Add(voucherHead);
                        }

                        #endregion

                        #endregion
                        var rowNr = 1;
                        var voucherRows = new List<VoucherRow>();
                        foreach (SupplierInvoiceRow invoiceRow in invoice.ActiveSupplierInvoiceRows)
                        {
                            #region SupplierInvoiceRow

                            foreach (SupplierInvoiceAccountRow invoiceAccountRow in invoiceRow.SupplierInvoiceAccountRow.Where(r => r.State == (int)SoeEntityState.Active && r.Type == (int)AccountingRowType.AccountingRow))
                            {
                                #region SupplierInvoiceAccountRow

                                #region Prereq

                                VoucherRow voucherRow = null;
                                int accountId = invoiceAccountRow.AccountStd.AccountId;

                                if (invoiceToVoucherRowType == (int)SoeInvoiceToVoucherRowType.MergeVoucherRowsOnAccount && string.IsNullOrEmpty(invoiceAccountRow.Text))
                                {
                                    //(Bug 1248: Quantity should aways be null in Supplier ledger)
                                    if (invoiceAccountRow.Quantity == 0)
                                        invoiceAccountRow.Quantity = null;

                                    //Find existing VoucherRow on VoucherHead
                                    voucherRow = GetVoucherRowFromSupplierInvoiceAccountRow(voucherHead, invoiceAccountRow);
                                    if (voucherRow != null)
                                    {
                                        //Update VoucherRow
                                        voucherRow.Amount += invoiceAccountRow.Amount;
                                        voucherRow.Quantity += invoiceAccountRow.Quantity;
                                        voucherRow.Merged = true;

                                        //Only add the same sequence number once
                                        if (!seqNbrDict.ContainsKey(accountId) || !seqNbrDict[accountId].Contains(invoice.SeqNr.Value) && voucherRow.Text.Length < 500)
                                            voucherRow.Text += ", " + invoice.SeqNr.Value.ToString();

                                        //Set currency amounts
                                        CountryCurrencyManager.SetCurrencyAmounts(entities, actorCompanyId, voucherRow);
                                    }
                                }

                                #endregion

                                if (voucherRow == null)
                                {
                                    #region VoucherRow

                                    /*
                                     * Rad texten på formatet Kundfakt. (Faktnr), ev leverantör, ev text från radkontering alt text från kundfakthuvudet
                                     */

                                    //Text
                                    string text = GetText(1806, "Lev.fakt.") + " " + invoice.SeqNr.Value.ToString();

                                    //Add customer if no merging
                                    if ((invoiceToVoucherHeadType == (int)SoeInvoiceToVoucherHeadType.VoucherPerInvoice || invoices.Count == 1) && !string.IsNullOrEmpty(invoice.ActorName))
                                        text += ", " + invoice.ActorName;

                                    //Add text from row, or description from origin
                                    if (!string.IsNullOrEmpty(invoiceAccountRow.Text))
                                        text += ", " + invoiceAccountRow.Text;

                                    //else if ((invoiceToVoucherHeadType == (int)SoeInvoiceToVoucherHeadType.VoucherPerInvoice || invoices.Count() == 1) && !String.IsNullOrEmpty(invoice.Origin.Description))
                                    //    text += ", " + invoice.Origin.Description;
                                    if (text.Length > 512)
                                        text = text.Left(512);

                                    //Create VoucherRow
                                    voucherRow = new VoucherRow
                                    {
                                        Text = text,
                                        Amount = invoiceAccountRow.Amount,
                                        Quantity = invoiceAccountRow.Quantity.HasValue ? decimal.Round(invoiceAccountRow.Quantity.Value, 6) : invoiceAccountRow.Quantity,
                                        RowNr = rowNr,
                                        //Set references
                                        AccountStd = invoiceAccountRow.AccountStd
                                    };

                                    rowNr++;

                                    //Set currency amounts
                                    CountryCurrencyManager.SetCurrencyAmounts(entities, actorCompanyId, voucherRow);

                                    //Set AccountInternals
                                    foreach (AccountInternal accountInternal in invoiceAccountRow.AccountInternal)
                                    {
                                        voucherRow.AccountInternal.Add(accountInternal);
                                    }

                                    //Add SupplierInvoiceAccountRow
                                    if (!useAccountDistribution)
                                        invoiceAccountRow.VoucherRow = voucherRow;

                                    voucherRow.Date = voucherRow.Date.HasValue ? voucherRow.Date : voucherHead.Date;

                                    //Add VoucherRow
                                    voucherHead.VoucherRow.Add(voucherRow);

                                    #endregion

                                    #region SupplierInvoiceAccountRow

                                    // Set voucher number on accounting row
                                    invoiceAccountRow.Text = string.Format("{0} {1}", GetText(3887, "Ver:"), voucherHead.VoucherNr) + (invoiceAccountRow.Text != null && invoiceAccountRow.Text.Trim() != String.Empty ? " - " + invoiceAccountRow.Text : String.Empty);

                                    #endregion
                                }

                                // Remember current invoice sequence number to prevent multiple numbers on the same voucher row.
                                if (!seqNbrDict.ContainsKey(accountId))
                                    seqNbrDict[accountId] = new List<int>();
                                seqNbrDict[accountId].Add(invoice.SeqNr.Value);

                                #endregion
                            }

                            #endregion
                        }

                        #region Status

                        //Update Origin status
                        invoice.Origin.Status = (int)SoeOriginStatus.Voucher;

                        #endregion

                        #region Connect SupplierInvoice to VoucherHead

                        //Add VoucherHead to SupplierInvoice
                        invoice.VoucherHead = voucherHead;

                        //Check if VoucherHead should be added
                        // Add Voucher if:
                        // 1) Setting SoeInvoiceToVoucherHeadType.VoucherPerInvoice
                        // 2) Setting SoeInvoiceToVoucherHeadType.MergeVoucherOnInvoiceDate, but no matching VoucherHead found
                        // 3) Setting SoeInvoiceToVoucherHeadType.MergeVoucherOnInvoiceDate, but is last VoucherHead
                        //if (!foundExistingVoucherHead || voucherHeadCounter == invoices.Count)
                        //{
                        //    //Add VoucherHead
                        //    entities.VoucherHead.AddObject(voucherHead);
                        //}

                        //Its possible to have invoice with 0, so when transfered to voucher they are marked as fully paid...
                        if (invoice.TotalAmount == 0)
                        {
                            invoice.FullyPayed = true;
                        }

                        #endregion
                    }

                    #region Account distribution

                    foreach (var head in voucherHeads)
                    {
                        if (useAccountDistribution)
                        {
                            // Get distributed
                            var distributedRows = VoucherManager.ApplyAutomaticAccountDistribution(entities, head.VoucherRow.ToList(), accountStds, accountDims, accountInternals, actorCompanyId, null, true, invoiceToVoucherRowType == (int)SoeInvoiceToVoucherRowType.MergeVoucherRowsOnAccount);

                            // Empty
                            head.VoucherRow.Clear();

                            // Add distributed
                            head.VoucherRow.AddRange(distributedRows);
                        }

                        //Add VoucherHead
                        entities.VoucherHead.AddObject(head);
                    }

                    #endregion

                    new VoucherNumberLock(entities)
                        .AddVouchers(voucherHeads)
                        .SetVoucherNumbers();

                    if (result.Success)
                        result = SaveChanges(entities, transaction);

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
                    result.IdDict = GetVouchersDict(voucherHeads);
                    //result.IntegerValue = voucherHeads.Count;
                    //Return not transferred invoicenumbers
                    if (continueIfError && failedToTransfer != null)
                        if (failedToTransfer != string.Empty)
                            result.StringValue = failedToTransfer;
                }
                else
                    base.LogTransactionFailed(this.ToString(), this.log);

                if (transactionScopeOption != ConfigSettings.TRANSACTIONSCOPEOPTION_ATTACH)
                    entities.Connection.Close();
            }

            return result;
        }

        public ActionResult SaveVoucherFromSupplierInvoicesWithPolling(CompEntities entities, TransactionScopeOption transactionScopeOption, List<SupplierInvoice> invoices, int actorCompanyId, string failedToTransfer, ref SoeProgressInfo info)
        {
            if (invoices == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "SupplierInvoice");

            // Default result is successful
            ActionResult result = new ActionResult();

            var voucherHeads = new List<VoucherHead>();
            var accountYearsToReturn = new HashSet<int>();

            bool continueIfError = false;
            //New list of invoices not able to transfer (Angular)
            if (failedToTransfer != null)
                continueIfError = true;

            try
            {
                if (entities.Connection.State != ConnectionState.Open)
                    entities.Connection.Open();

                info.Message = GetText(9331, "Kontrollerar förutsättningar");

                #region Settings

                //Merge Invoice to VoucherHead
                int invoiceToVoucherHeadType = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.SupplierInvoiceToVoucherHeadType, 0, actorCompanyId, 0);
                if (invoiceToVoucherHeadType == (int)SoeInvoiceToVoucherHeadType.None)
                    invoiceToVoucherHeadType = (int)SoeInvoiceToVoucherHeadType.VoucherPerInvoice;

                //Merge Invoice to VoucherRow
                int invoiceToVoucherRowType = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.SupplierInvoiceToVoucherRowType, 0, actorCompanyId, 0);
                if (invoiceToVoucherRowType == (int)SoeInvoiceToVoucherRowType.None)
                    invoiceToVoucherRowType = (int)SoeInvoiceToVoucherRowType.VoucherRowPerInvoiceRow;

                bool useAccountDistribution = SettingManager.GetBoolSetting(entities, SettingMainType.Company, (int)CompanySettingType.SupplierInvoiceUseAutoAccountDistributionOnVoucher, 0, actorCompanyId, 0);
                //VoucherSeries
                int supplierInvoiceSeriesTypeId = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.SupplierInvoiceVoucherSeriesType, 0, actorCompanyId, 0);
                #endregion

                #region Prereq

                int voucherHeadCounter = 0;
                AccountYear accountYear = null;
                
                AccountPeriod accountPeriod = null;

                List<AccountDim> accountDims = useAccountDistribution ? AccountManager.GetAccountDimsByCompany(entities, actorCompanyId) : null;
                List<AccountStd> accountStds = useAccountDistribution ? AccountManager.GetAccountStdsByCompany(entities, actorCompanyId, null, true, false) : null;
                List<AccountInternal> accountInternals = useAccountDistribution ? AccountManager.GetAccountInternals(entities, actorCompanyId, true) : null;

                #endregion
                int numberOfHandled = 0;

                // Possible to include this method in a running Transaction
                using (TransactionScope transaction = new TransactionScope(transactionScopeOption, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                {
                    foreach (var invoice in invoices.OrderBy(x => x.VoucherDate))
                    {
						VoucherSeries voucherSerie = null;
						DateTime voucherDate = invoice.VoucherDate.HasValue ? invoice.VoucherDate.Value : DateTime.Today;
                        int? invoiceVoucherSeriesTypeId = null;
                        invoiceVoucherSeriesTypeId = invoice.Origin.VoucherSeriesTypeId;
                        invoiceVoucherSeriesTypeId ??= supplierInvoiceSeriesTypeId;
                        invoiceVoucherSeriesTypeId ??= 0;

                        if (invoiceVoucherSeriesTypeId != 0)
                            voucherSerie = VoucherManager.GetVoucherSerie(entities, invoiceVoucherSeriesTypeId.Value, voucherDate, actorCompanyId);

						//Validate AccountYear
						var needsNewVoucher = false;

                        if (!AccountManager.ValidateAccountYear(accountYear, voucherDate).Success)
                        {
                            needsNewVoucher = accountYear != null;
                            accountYear = AccountManager.GetAccountYear(entities, voucherDate, actorCompanyId);

                            result = AccountManager.ValidateAccountYear(accountYear);
                            if (!result.Success)
                                return result;

                            //Get default VoucherSerie for SupplierInvoice for current AccountYear
                            voucherSerie ??= VoucherManager.GetVoucherSerie(entities, invoice.Origin.VoucherSeriesId, actorCompanyId);

                            if (voucherSerie == null)
                                return new ActionResult(GetText(8403, "Verifikatserie saknas"));

                            accountYearsToReturn.Add(accountYear.AccountYearId);
                        }

                        //Validate AccountPeriod
                        if (!AccountManager.ValidateAccountPeriod(accountPeriod, voucherDate).Success)
                        {
                            needsNewVoucher = accountPeriod != null;
                            accountPeriod = AccountManager.GetAccountPeriod(entities, accountYear.AccountYearId, voucherDate, actorCompanyId);
                            result = AccountManager.ValidateAccountPeriod(accountPeriod, voucherDate);
                            if (!result.Success)
                                return result;
                        }

                        if (voucherSerie == null)
                            return new ActionResult(GetText(8403, "Verifikatserie saknas"));

                        #region Prereq

                        //Can only get status Voucher from Origin
                        if (invoice.Origin.Status != (int)SoeOriginStatus.Origin || invoice.VoucherHeadId.HasValue)
                        {
                            if (continueIfError)
                            {
                                failedToTransfer = failedToTransfer + invoice.InvoiceNr + Environment.NewLine;
                                continue;
                            }
                            else
                            {
                                result.Success = false;
                                result.ErrorNumber = (int)ActionResultSave.InvalidStateTransition;
                                return result;
                            }
                        }

                        Dictionary<int, List<int>> seqNbrDict = new Dictionary<int, List<int>>();

                        #endregion

                        #region Validate invoice accounting rows

                        if (SupplierInvoiceManager.ValidateSupplierInvoiceAccountingRowsDiff(invoice))
                        {
                            if (continueIfError)
                            {
                                failedToTransfer = failedToTransfer + invoice.InvoiceNr + Environment.NewLine;
                                continue;
                            }
                            else
                            {
                                return new ActionResult((int)ActionResultSave.HasUnbalancedAccountingRows, "SupplierInvoice");
                            }
                        }

                        #endregion

                        #region VoucherHead

                        /*
                         * Verifikathuvudtexten som inte är sammanslagna ska vara på formatet Lev.fakt. (Löpnr), Leverantör, ev fakturatext skall ej ut i vertext 
                         * Sammanslagna på formatet  Lev.fakt. (Löpnr), (Löpnr), (Löpnr)
                         */

                        numberOfHandled = numberOfHandled + 1;
                        info.Message = string.Format(GetText(9330, "Skapar verifikat för faktura {0} av {1}"), numberOfHandled.ToString(), invoices.Count);

                        voucherHeadCounter++;
                        //bool foundExistingVoucherHead = false;
                        VoucherHead voucherHead = null;

                        #region MergeVoucherOnInvoiceDate

                        if (invoiceToVoucherHeadType == (int)SoeInvoiceToVoucherHeadType.MergeVoucherOnVoucherDate && voucherHeads.Any() && !needsNewVoucher)
                        {
                            voucherHead = voucherHeads.FirstOrDefault(i => i.Date == voucherDate);
                            if (voucherHead != null)
                            {
                                //Text
                                voucherHead.Text += ", " + invoice.SeqNr.Value;
                                //foundExistingVoucherHead = true;
                            }
                        }

                        #endregion

                        #region VoucherPerInvoice (or no matching Voucher found)

                        if (invoiceToVoucherHeadType == (int)SoeInvoiceToVoucherHeadType.VoucherPerInvoice || voucherHead == null)
                        {
                            //Text
                            string voucherHeadText = GetText(1806, "Lev.fakt.") + " " + invoice.SeqNr.Value;
                            if (invoiceToVoucherHeadType == (int)SoeInvoiceToVoucherHeadType.VoucherPerInvoice || invoices.Count == 1)
                            {
                                //Add additional info if not merging
                                if (!string.IsNullOrEmpty(invoice.ActorName))
                                    voucherHeadText += ", " + invoice.ActorName;
                            }

                            //Create VoucherHead
                            voucherHead = new VoucherHead()
                            {
                                VoucherNr = voucherSerie.VoucherNrLatest.HasValue ? voucherSerie.VoucherNrLatest.Value + 1 : 1,
                                Date = voucherDate,
                                Text = voucherHeadText,

                                //Set FK
                                ActorCompanyId = actorCompanyId,

                                //Set references
                                VoucherSeries = voucherSerie,

                                // Account period
                                AccountPeriod = accountPeriod,
                            };
                            SetCreatedProperties(voucherHead);

                            //Update VoucherSeries
                            voucherSerie.VoucherNrLatest = voucherHead.VoucherNr;
                            voucherSerie.VoucherDateLatest = voucherHead.Date;

                            //Set status on VoucherHead to the same as its AccountPeriod
                            voucherHead.Status = voucherHead.AccountPeriod.Status;

                            voucherHeads.Add(voucherHead);
                        }

                        #endregion

                        #endregion
                        var rowNr = 1;
                        var voucherRows = new List<VoucherRow>();
                        foreach (SupplierInvoiceRow invoiceRow in invoice.ActiveSupplierInvoiceRows)
                        {
                            #region SupplierInvoiceRow

                            foreach (SupplierInvoiceAccountRow invoiceAccountRow in invoiceRow.SupplierInvoiceAccountRow.Where(r => r.State == (int)SoeEntityState.Active && r.Type == (int)AccountingRowType.AccountingRow))
                            {
                                #region SupplierInvoiceAccountRow

                                #region Prereq

                                VoucherRow voucherRow = null;
                                int accountId = invoiceAccountRow.AccountStd.AccountId;

                                if (invoiceToVoucherRowType == (int)SoeInvoiceToVoucherRowType.MergeVoucherRowsOnAccount && string.IsNullOrEmpty(invoiceAccountRow.Text))
                                {
                                    //Find existing VoucherRow on VoucherHead
                                    voucherRow = GetVoucherRowFromSupplierInvoiceAccountRow(voucherHead, invoiceAccountRow);
                                    if (voucherRow != null)
                                    {
                                        //Update VoucherRow
                                        voucherRow.Amount += invoiceAccountRow.Amount;
                                        voucherRow.Quantity += invoiceAccountRow.Quantity;
                                        voucherRow.Merged = true;

                                        //Only add the same sequence number once
                                        if (!seqNbrDict.ContainsKey(accountId) || !seqNbrDict[accountId].Contains(invoice.SeqNr.Value) && voucherRow.Text.Length < 500)
                                            voucherRow.Text += ", " + invoice.SeqNr.Value.ToString();

                                        //Set currency amounts
                                        CountryCurrencyManager.SetCurrencyAmounts(entities, actorCompanyId, voucherRow);
                                    }
                                }

                                #endregion

                                if (voucherRow == null)
                                {
                                    #region VoucherRow

                                    /*
                                     * Rad texten på formatet Kundfakt. (Faktnr), ev leverantör, ev text från radkontering alt text från kundfakthuvudet
                                     */

                                    //Text
                                    string text = GetText(1806, "Lev.fakt.") + " " + invoice.SeqNr.Value.ToString();
                                    //Add customer if no merging
                                    if ((invoiceToVoucherHeadType == (int)SoeInvoiceToVoucherHeadType.VoucherPerInvoice || invoices.Count == 1) && !string.IsNullOrEmpty(invoice.ActorName))
                                        text += ", " + invoice.ActorName;
                                    //Add text from row, or description from origin
                                    if (!string.IsNullOrEmpty(invoiceAccountRow.Text))
                                        text += ", " + invoiceAccountRow.Text;
                                    //else if ((invoiceToVoucherHeadType == (int)SoeInvoiceToVoucherHeadType.VoucherPerInvoice || invoices.Count() == 1) && !String.IsNullOrEmpty(invoice.Origin.Description))
                                    //    text += ", " + invoice.Origin.Description;
                                    if (text.Length > 512)
                                        text = text.Left(512);
                                    //Create VoucherRow
                                    voucherRow = new VoucherRow()
                                    {
                                        Text = text,
                                        Amount = invoiceAccountRow.Amount,
                                        Quantity = invoiceAccountRow.Quantity.HasValue ? Decimal.Round(invoiceAccountRow.Quantity.Value, 6) : invoiceAccountRow.Quantity,
                                        RowNr = rowNr,
                                        //Set references
                                        AccountStd = invoiceAccountRow.AccountStd
                                    };

                                    rowNr++;

                                    //Set currency amounts
                                    CountryCurrencyManager.SetCurrencyAmounts(entities, actorCompanyId, voucherRow);

                                    //Set AccountInternals
                                    foreach (AccountInternal accountInternal in invoiceAccountRow.AccountInternal)
                                    {
                                        voucherRow.AccountInternal.Add(accountInternal);
                                    }

                                    //Add SupplierInvoiceAccountRow
                                    if (!useAccountDistribution)
                                        invoiceAccountRow.VoucherRow = voucherRow;

                                    //Add VoucherRow
                                    voucherHead.VoucherRow.Add(voucherRow);

                                    #endregion

                                    #region AccountBalance

                                    // Update balance on account
                                    if (voucherHead.Template == false && voucherRow.AccountStd != null)
                                    {
                                        //Edit NI 100217: Update AccountBalance for all accounts from Silverlight in Complete method
                                        //Update AccountBalance
                                        //abm.SetAccountBalance(entities, actorCompanyId, voucherRow.AccountStd.AccountId, accountYearId, voucherRow.Amount, true, false, accountBalances);
                                    }

                                    #endregion

                                    #region SupplierInvoiceAccountRow

                                    // Set voucher number on accounting row
                                    invoiceAccountRow.Text = string.Format("{0} {1}", GetText(3887, "Ver:"), voucherHead.VoucherNr) + (invoiceAccountRow.Text != null && invoiceAccountRow.Text.Trim() != String.Empty ? " - " + invoiceAccountRow.Text : String.Empty);

                                    #endregion
                                }

                                // Remember current invoice sequence number to prevent multiple numbers on the same voucher row.
                                if (!seqNbrDict.ContainsKey(accountId))
                                    seqNbrDict[accountId] = new List<int>();
                                seqNbrDict[accountId].Add(invoice.SeqNr.Value);

                                #endregion
                            }

                            #endregion
                        }

                        #region Status

                        //Update Origin status
                        invoice.Origin.Status = (int)SoeOriginStatus.Voucher;

                        #endregion

                        #region Connect SupplierInvoice to VoucherHead

                        //Add VoucherHead to SupplierInvoice
                        invoice.VoucherHead = voucherHead;

                        //Check if VoucherHead should be added
                        // Add Voucher if:
                        // 1) Setting SoeInvoiceToVoucherHeadType.VoucherPerInvoice
                        // 2) Setting SoeInvoiceToVoucherHeadType.MergeVoucherOnInvoiceDate, but no matching VoucherHead found

                        //if (!foundExistingVoucherHead)
                        //{
                        //    //Add VoucherHead
                        //    entities.VoucherHead.AddObject(voucherHead);
                        //}

                        //Its possible to have invoice with 0, so when transfered to voucher they are marked as fully paid...
                        if (invoice.TotalAmount == 0)
                        {
                            invoice.FullyPayed = true;
                        }

                        #endregion
                    }

                    #region Account distribution

                    foreach (var head in voucherHeads)
                    {
                        if (useAccountDistribution)
                        {
                            // Get distributed
                            var distributedRows = VoucherManager.ApplyAutomaticAccountDistribution(entities, head.VoucherRow.ToList(), accountStds, accountDims, accountInternals, actorCompanyId, null, true, invoiceToVoucherRowType == (int)SoeInvoiceToVoucherRowType.MergeVoucherRowsOnAccount);

                            // Empty
                            head.VoucherRow.Clear();

                            // Add distributed
                            head.VoucherRow.AddRange(distributedRows);
                        }

                        //Add VoucherHead
                        entities.VoucherHead.AddObject(head);
                    }

                    #endregion

                    info.Message = GetText(9332, "Sparar verifikat");

                    new VoucherNumberLock(entities)
                        .AddVouchers(voucherHeads)
                        .SetVoucherNumbers();

                    if (result.Success)
                        result = SaveChanges(entities, transaction);

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
                    result.IdDict = GetVouchersDict(voucherHeads);
                    result.IntDict = accountYearsToReturn.ToDictionary(k => k, k => k);

                    //Return not transferred invoicenumbers
                    if (continueIfError && failedToTransfer != null)
                        if (failedToTransfer != string.Empty)
                            result.StringValue = failedToTransfer;
                }
                else
                    base.LogTransactionFailed(this.ToString(), this.log);

                if (transactionScopeOption != ConfigSettings.TRANSACTIONSCOPEOPTION_ATTACH)
                    entities.Connection.Close();
            }

            return result;
        }

        public ActionResult SaveVoucherFromPayment(CompEntities entities, TransactionScopeOption transactionScopeOption, List<PaymentRow> paymentRows, SoeOriginType originType, bool foreign, int accountYearId, int actorCompanyId, bool alwaysMerge = false, DateTime? date = null, bool usePayDateAndValidate = false, List<SysHoliday> holidays = null)
        {
            if (paymentRows == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "PaymentRow");

            // Default result is successful
            ActionResult result = new ActionResult();

            List<VoucherHead> voucherHeads = new List<VoucherHead>();
            var addedInvoiceNr = new HashSet<string>();

            #region
            try
            {
                if (entities.Connection.State != ConnectionState.Open)
                    entities.Connection.Open();

                // Possible to include this method in a running Transaction
                using (TransactionScope transaction = new TransactionScope(transactionScopeOption, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                {
                    #region Settings

                    //Customer/Supplier dependent settings
                    int paymentSeriesTypeId = 0;
                    int invoiceToVoucherHeadType = (int)SoeInvoiceToVoucherHeadType.None;
                    int invoiceToVoucherRowType = (int)SoeInvoiceToVoucherRowType.None;
                    bool useAccountDistribution = false;
                    if (originType == SoeOriginType.SupplierPayment)
                    {
                        //VoucherSeries
                        paymentSeriesTypeId = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.SupplierPaymentVoucherSeriesType, 0, actorCompanyId, 0);

                        //Merge Payment to VoucherHead
                        invoiceToVoucherHeadType = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.SupplierPaymentToVoucherHeadType, 0, actorCompanyId, 0);
                        if (invoiceToVoucherHeadType == (int)SoeInvoiceToVoucherHeadType.None)
                            invoiceToVoucherHeadType = (int)SoeInvoiceToVoucherHeadType.VoucherPerInvoice;

                        if (alwaysMerge)
                            invoiceToVoucherHeadType = (int)SoeInvoiceToVoucherHeadType.MergeAllInvoices;

                        //Merge Payment to VoucherRow
                        invoiceToVoucherRowType = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.SupplierPaymentToVoucherRowType, 0, actorCompanyId, 0);
                        if (invoiceToVoucherRowType == (int)SoeInvoiceToVoucherRowType.None)
                            invoiceToVoucherRowType = (int)SoeInvoiceToVoucherRowType.VoucherRowPerInvoiceRow;
                        useAccountDistribution = SettingManager.GetBoolSetting(entities, SettingMainType.Company, (int)CompanySettingType.SupplierInvoiceUseAutoAccountDistributionOnVoucher, 0, actorCompanyId, 0);
                    }
                    else if (originType == SoeOriginType.CustomerPayment)
                    {
                        //VoucherSeries
                        paymentSeriesTypeId = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.CustomerPaymentVoucherSeriesType, 0, actorCompanyId, 0);

                        //Merge Payment to VoucherHead
                        invoiceToVoucherHeadType = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.CustomerPaymentToVoucherHeadType, 0, actorCompanyId, 0);
                        if (invoiceToVoucherHeadType == (int)SoeInvoiceToVoucherHeadType.None)
                            invoiceToVoucherHeadType = (int)SoeInvoiceToVoucherHeadType.VoucherPerInvoice;

                        //Merge Payment to VoucherRow
                        invoiceToVoucherRowType = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.CustomerPaymentToVoucherRowType, 0, actorCompanyId, 0);
                        if (invoiceToVoucherRowType == (int)SoeInvoiceToVoucherRowType.None)
                            invoiceToVoucherRowType = (int)SoeInvoiceToVoucherRowType.VoucherRowPerInvoiceRow;
                        useAccountDistribution = SettingManager.GetBoolSetting(entities, SettingMainType.Company, (int)CompanySettingType.CustomerInvoiceUseAutoAccountDistributionOnVoucher, 0, actorCompanyId, 0);
                    }
                    else
                    {
                        result.Success = false;
                        result.ErrorNumber = (int)ActionResultSave.InvalidStateTransition;
                        result.ErrorMessage = "InvalidStateTransition originType: " + originType;
                        return result;
                    }

                    #endregion

                    #region Prereq

                    //Validate AccountYear
                    AccountYear accountYear = AccountManager.GetAccountYear(entities, accountYearId);
                    result = AccountManager.ValidateAccountYear(accountYear);
                    if (!result.Success)
                        return result;

                    //Get default VoucherSerie for SupplierInvoice payments and current AccountYear
                    VoucherSeries voucherSeries = VoucherManager.GetVoucherSerieByType(entities, paymentSeriesTypeId, accountYear.AccountYearId);
                    if (voucherSeries == null)
                        return new ActionResult((int)ActionResultSave.EntityNotFound, "VoucherSeries");

                    //Cache VoucherSeries
                    Dictionary<int, VoucherSeries> voucherSeriesDict = new Dictionary<int, VoucherSeries>();
                    voucherSeriesDict.Add(voucherSeries.VoucherSeriesId, voucherSeries);

                    int voucherHeadCounter = 0;

                    List<AccountDim> accountDims = useAccountDistribution ? AccountManager.GetAccountDimsByCompany(entities, actorCompanyId) : null;
                    List<AccountStd> accountStds = useAccountDistribution ? AccountManager.GetAccountStdsByCompany(entities, actorCompanyId, null, true, false) : null;
                    List<AccountInternal> accountInternals = useAccountDistribution ? AccountManager.GetAccountInternals(entities, actorCompanyId, true) : null;

                    // Get company
                    var company = CompanyManager.GetCompany(entities, actorCompanyId);

                    #endregion

                    foreach (PaymentRow paymentRow in paymentRows)
                    {
                        #region Prereq

                        //Can only transfer Payment to Voucher once

                        if (paymentRow.VoucherHead != null)
                            return new ActionResult((int)ActionResultSave.InvalidStateTransition, string.Format(GetText(7460, "Verifikat finns redan för faktura {0} betalning {1} "), paymentRow.Invoice?.SeqNr, paymentRow.SeqNr));

                        //Can only transfer Payment to Voucher if status is Verified, Pending or ManualPayment
                        if (paymentRow.Status != (int)SoePaymentStatus.Verified && paymentRow.Status != (int)SoePaymentStatus.Pending && paymentRow.Status != (int)SoePaymentStatus.ManualPayment)
                            return new ActionResult((int)ActionResultSave.InvalidStateTransition, "PaymentRow");

                        //Prio 1: Use current AccountYear and VoucherSeries
                        AccountYear currentAccountYear = accountYear;
                        VoucherSeries currentVoucherSeries = voucherSeries;

                        // Check payment voucher series
                        VoucherSeries paymentVoucherSeries = null;
                        if (paymentRow.Payment?.Origin?.VoucherSeriesId != currentVoucherSeries.VoucherSeriesId && (invoiceToVoucherHeadType == (int)SoeInvoiceToVoucherHeadType.VoucherPerInvoice || paymentRows.Count == 1))
                            paymentVoucherSeries = VoucherManager.GetVoucherSerie(entities, paymentRow.Payment.Origin.VoucherSeriesId, actorCompanyId, true);

                        // Validate serie is in correct year
                        if (paymentVoucherSeries != null && paymentVoucherSeries.AccountYearId != currentAccountYear.AccountYearId)
                            paymentVoucherSeries = VoucherManager.GetVoucherSerieByType(entities, paymentVoucherSeries.VoucherSeriesTypeId, currentAccountYear.AccountYearId);

                        if (paymentVoucherSeries != null)
                        {
                            if (voucherSeriesDict.ContainsKey(paymentVoucherSeries.VoucherSeriesId))
                            {
                                //Prio 2: Use cached VoucherSeries, no preserve changed properties as LastVoucherNr etc
                                currentVoucherSeries = voucherSeriesDict[paymentVoucherSeries.VoucherSeriesId];
                            }
                            else
                            {
                                //Prio 3: Use fetched VoucherSeries, and cache them for next loop
                                currentVoucherSeries = paymentVoucherSeries;
                                voucherSeriesDict.Add(paymentVoucherSeries.VoucherSeriesId, paymentVoucherSeries);
                            }
                        }

                        // Handle date
                        var voucherDate = date == null ? paymentRow.PayDate.Date : date.Value.Date;
                        if (usePayDateAndValidate)
                        {
                            voucherDate = paymentRow.PayDate;
                            if (holidays == null)
                            {
                                while (voucherDate.DayOfWeek != DayOfWeek.Saturday || voucherDate.DayOfWeek != DayOfWeek.Sunday)
                                {
                                    voucherDate = voucherDate.AddDays(1);
                                }
                            }
                            else
                            {
                                while (holidays.Any(s => (s.SysHolidayType == null || s.SysHolidayType.SysCountryId == company.SysCountryId) && s.Date == voucherDate) || voucherDate.DayOfWeek == DayOfWeek.Saturday || voucherDate.DayOfWeek == DayOfWeek.Sunday)
                                {
                                    voucherDate = voucherDate.AddDays(1);
                                }
                            }
                        }

                        //Check if payment is in current year
                        if (!CalendarUtility.IsDateInRange(voucherDate, accountYear.From, accountYear.To))
                        {
                            //Validate AccountYear
                            currentAccountYear = AccountManager.GetAccountYear(entities, voucherDate, actorCompanyId);
                            result = AccountManager.ValidateAccountYear(currentAccountYear);
                            if (!result.Success)
                            {
                                result.ErrorMessage = String.Format(GetText(2233, "Du står i fel redovisningsår") + " {0}", accountYear.From.ToShortDateString() + "-" + accountYear.To.ToShortDateString());
                                result.ErrorNumber = (int)ActionResultSave.AccountYearNotOpen;
                                return result;
                            }

                            //Get VoucherSeries
                            VoucherSeries voucherSeriesTemp = VoucherManager.GetVoucherSerieByType(entities, paymentSeriesTypeId, currentAccountYear.AccountYearId);
                            if (voucherSeriesTemp == null)
                                return new ActionResult((int)ActionResultSave.EntityNotFound, "VoucherSeries");

                            paymentVoucherSeries = null;
                            if (paymentRow.Payment?.Origin?.VoucherSeriesId != voucherSeriesTemp.VoucherSeriesId && (invoiceToVoucherHeadType == (int)SoeInvoiceToVoucherHeadType.VoucherPerInvoice || paymentRows.Count == 1))
                                paymentVoucherSeries = VoucherManager.GetVoucherSerie(entities, paymentRow.Payment.Origin.VoucherSeriesId, actorCompanyId, true);

                            // Validate serie is in correct year
                            if (paymentVoucherSeries != null && paymentVoucherSeries.AccountYearId != currentAccountYear.AccountYearId)
                                paymentVoucherSeries = VoucherManager.GetVoucherSerieByType(entities, paymentVoucherSeries.VoucherSeriesTypeId, currentAccountYear.AccountYearId);

                            if (paymentVoucherSeries != null)
                            {
                                if (voucherSeriesDict.ContainsKey(paymentVoucherSeries.VoucherSeriesId))
                                {
                                    //Prio 2: Use cached VoucherSeries, no preserve changed properties as LastVoucherNr etc
                                    currentVoucherSeries = voucherSeriesDict[paymentVoucherSeries.VoucherSeriesId];
                                }
                                else
                                {
                                    //Prio 3: Use fetched VoucherSeries, and cache them for next loop
                                    currentVoucherSeries = paymentVoucherSeries;
                                    voucherSeriesDict.Add(paymentVoucherSeries.VoucherSeriesId, paymentVoucherSeries);
                                }
                            }
                            else
                            {
                                if (voucherSeriesDict.ContainsKey(voucherSeriesTemp.VoucherSeriesId))
                                {
                                    //Prio 2: Use cached VoucherSeries, no preserve changed properties as LastVoucherNr etc
                                    currentVoucherSeries = voucherSeriesDict[voucherSeriesTemp.VoucherSeriesId];
                                }
                                else
                                {
                                    //Prio 3: Use fetched VoucherSeries, and cache them for next loop
                                    currentVoucherSeries = voucherSeriesTemp;
                                    voucherSeriesDict.Add(voucherSeriesTemp.VoucherSeriesId, voucherSeriesTemp);
                                }
                            }
                        }

                        //Make sure Origin is loaded
                        if (!paymentRow.InvoiceReference.IsLoaded)
                            paymentRow.InvoiceReference.Load();

                        if (!paymentRow.Invoice.OriginReference.IsLoaded)
                            paymentRow.Invoice.OriginReference.Load();

                        #endregion

                        #region Voucher

                        /*
                         * Verifikathuvudtexten som inte är sammanslagna ska vara på formatet Betalning för fakt. (Faktnr), Aktör, ev fakturatext 
                         * Sammanslagna på formatet  Betalning för fakt. (Faktnr), (Faktnr), (Faktnr)
                         */

                        VoucherHead voucherHead = null;
                        voucherHeadCounter++;
                        //bool foundExistingVoucherHead = false;

                        #region MergeVoucherOnInvoiceDate

                        if (invoiceToVoucherHeadType == (int)SoeInvoiceToVoucherHeadType.MergeVoucherOnVoucherDate)
                        {
                            voucherHead = voucherHeads.FirstOrDefault(i => i.Date == voucherDate);

                            if (voucherHead != null)
                            {
                                if (paymentRow.Invoice.OnlyPayment || !addedInvoiceNr.Contains(paymentRow.Invoice.InvoiceNr))
                                {
                                    //Text
                                    voucherHead.Text += ", " + (!paymentRow.Invoice.OnlyPayment ? (" " + paymentRow.Invoice.InvoiceNr) : "*");
                                    if (!string.IsNullOrEmpty(paymentRow.Invoice.InvoiceNr))
                                    {
                                        addedInvoiceNr.Add(paymentRow.Invoice.InvoiceNr);
                                    }
                                }

                                //foundExistingVoucherHead = true;
                            }
                        }

                        if (invoiceToVoucherHeadType == (int)SoeInvoiceToVoucherHeadType.MergeAllInvoices)
                        {
                            voucherHead = voucherHeads.FirstOrDefault();
                            if (voucherHead != null)
                            {
                                //Text
                                voucherHead.Text += ", " + (!paymentRow.Invoice.OnlyPayment ? (" " + paymentRow.Invoice.InvoiceNr) : "*");

                                //foundExistingVoucherHead = true;
                            }
                        }

                        #endregion

                        #region VoucherPerInvoice (or no matching Voucher found)

                        if (invoiceToVoucherHeadType == (int)SoeInvoiceToVoucherHeadType.VoucherPerInvoice || voucherHeadCounter == 1 || voucherHead == null)
                        {
                            if (!paymentRow.Invoice.OnlyPayment)
                            {
                                addedInvoiceNr.Clear();
                                if (!string.IsNullOrEmpty(paymentRow.Invoice.InvoiceNr))
                                {
                                    addedInvoiceNr.Add(paymentRow.Invoice.InvoiceNr);
                                }
                            }


                            //Text
                            string voucherHeadText = $"{GetText(2294, "Betalning för faktura")} {(!paymentRow.Invoice.OnlyPayment ? (" " + paymentRow.Invoice.InvoiceNr) : " *")}";
                            if (paymentRow.Payment?.Origin?.Status == (int)SoeOriginStatus.Matched)
                            {
                                voucherHeadText = $"{GetText(7449, "Utjämning")}: {voucherHeadText}";
                            }

                            if ((invoiceToVoucherHeadType == (int)SoeInvoiceToVoucherHeadType.VoucherPerInvoice || paymentRows.Count == 1) ||
                                (invoiceToVoucherHeadType == (int)SoeInvoiceToVoucherHeadType.MergeVoucherOnVoucherDate && !paymentRows.Any(r => r.PaymentRowId != paymentRow.PaymentRowId && r.PayDate == paymentRow.PayDate)))
                            {
                                //Add additional info if not merging
                                if (!String.IsNullOrEmpty(paymentRow.Invoice.ActorName))
                                    voucherHeadText += ", " + paymentRow.Invoice.ActorName;
                            }

                            // Handle date
                            /*var voucherDate = date == null ? paymentRow.PayDate.Date : (DateTime)date.Value.Date;
                            if (usePayDateAndValidate)
                            {
                                voucherDate = paymentRow.PayDate;
                                if (holidays == null)
                                {
                                    while (voucherDate.DayOfWeek != DayOfWeek.Saturday || voucherDate.DayOfWeek != DayOfWeek.Sunday)
                                    {
                                        voucherDate = voucherDate.AddDays(1);
                                    }
                                }
                                else
                                {
                                    while (holidays.Any(s => (s.SysHolidayType == null || s.SysHolidayType.SysCountryId == company.SysCountryId) && s.Date == voucherDate) || voucherDate.DayOfWeek == DayOfWeek.Saturday || voucherDate.DayOfWeek == DayOfWeek.Sunday)
                                    {
                                        voucherDate = voucherDate.AddDays(1);
                                    }
                                }
                            }*/

                            //Create VoucherHead
                            voucherHead = new VoucherHead()
                            {
                                VoucherNr = currentVoucherSeries.VoucherNrLatest.HasValue ? currentVoucherSeries.VoucherNrLatest.Value + 1 : 1,
                                Date = voucherDate,
                                Text = voucherHeadText,

                                //Set FK
                                ActorCompanyId = actorCompanyId,

                                //Set references
                                VoucherSeries = currentVoucherSeries,
                            };
                            SetCreatedProperties(voucherHead);

                            //Update VoucherSeries
                            currentVoucherSeries.VoucherNrLatest = voucherHead.VoucherNr;
                            currentVoucherSeries.VoucherDateLatest = voucherHead.Date;

                            //Validate AccountYear
                            result = AccountManager.ValidateAccountYear(voucherHead.Date, currentAccountYear.From, currentAccountYear.To);
                            if (!result.Success)
                                return result;

                            //Validate AccountPeriod
                            voucherHead.AccountPeriod = AccountManager.GetAccountPeriod(entities, currentAccountYear.AccountYearId, voucherHead.Date, actorCompanyId);
                            result = AccountManager.ValidateAccountPeriod(voucherHead.AccountPeriod, voucherHead.Date);
                            if (!result.Success)
                                return result;

                            voucherHead.Status = voucherHead.AccountPeriod.Status;

                            voucherHeads.Add(voucherHead);
                        }

                        #endregion

                        int rowNr = 1;
                        var voucherRows = new List<VoucherRow>();
                        foreach (PaymentAccountRow paymentAccountRow in paymentRow.PaymentAccountRow.Where(r => r.State == (int)SoeEntityState.Active))
                        {
                            #region PaymentAccountRow

                            VoucherRow voucherRow = null;

                            if (invoiceToVoucherRowType == (int)SoeInvoiceToVoucherRowType.MergeVoucherRowsOnAccount)
                            {
                                //Find existing VoucherRow on VoucherHead
                                voucherRow = GetVoucherRowFromPaymentAccountRow(voucherHead, paymentAccountRow);
                                if (voucherRow != null)
                                {
                                    //Update VoucherRow
                                    if (voucherRow.Text.Length < 500)
                                        voucherRow.Text += ", " + paymentRow.SeqNr.ToString();
                                    voucherRow.Quantity = null;
                                    voucherRow.Amount += paymentAccountRow.Amount;
                                    voucherRow.AmountEntCurrency += paymentAccountRow.AmountEntCurrency;

                                    //Set currency amounts
                                    CountryCurrencyManager.SetCurrencyAmounts(entities, actorCompanyId, voucherRow);
                                }
                            }

                            if (voucherRow == null)
                            {
                                #region VoucherRow

                                /*
                                 * Rad texten på formatet Bet. (Löpnr), fakt. (Faktnr), ev aktör, ev text från radkontering alt text från fakthuvudet
                                 */

                                //Text
                                string text = GetText(2293, "Bet.") + " " + paymentRow.SeqNr.ToString();
                                //Add invoices
                                if (invoiceToVoucherHeadType == (int)SoeInvoiceToVoucherHeadType.VoucherPerInvoice || paymentRows.Count == 1)
                                    text += GetText(2295, ", fakt.") + " " + paymentRow.Invoice.InvoiceNr;
                                //Add actor if no merging
                                if (((invoiceToVoucherHeadType == (int)SoeInvoiceToVoucherHeadType.VoucherPerInvoice || paymentRows.Count == 1) ||
                                    invoiceToVoucherRowType != (int)SoeInvoiceToVoucherRowType.MergeVoucherRowsOnAccount) && !string.IsNullOrEmpty(paymentRow.Invoice.ActorName))
                                    text += ", " + paymentRow.Invoice.ActorName;
                                //Add text from row, or description from origin
                                if (!string.IsNullOrEmpty(paymentAccountRow.Text))
                                    text += ", " + paymentAccountRow.Text;
                                //else if ((invoiceToVoucherHeadType == (int)SoeInvoiceToVoucherHeadType.VoucherPerInvoice || paymentRows.Count() == 1) && !String.IsNullOrEmpty(paymentRow.Invoice.Origin.Description))
                                //    text += ", " + paymentRow.Invoice.Origin.Description;
                                if (text.Length > 512)
                                    text = text.Left(512);
                                //Create VoucherRow
                                voucherRow = new VoucherRow()
                                {
                                    Text = text,
                                    Amount = paymentAccountRow.Amount,
                                    AmountEntCurrency = paymentAccountRow.AmountEntCurrency,
                                    Quantity = null,
                                    RowNr = rowNr,

                                    //Set references
                                    AccountStd = paymentAccountRow.AccountStd,
                                };
                                rowNr++;
                                //Set currency amounts
                                CountryCurrencyManager.SetCurrencyAmounts(entities, actorCompanyId, voucherRow);

                                //Set AccountInternals
                                foreach (AccountInternal accountInternal in paymentAccountRow.AccountInternal)
                                {
                                    voucherRow.AccountInternal.Add(accountInternal);
                                }

                                //if (voucherRow.AccountNr == "2617")
                                //{
                                //    string err = "Nu är något fel!";
                                //}

                                //Add SupplierInvoiceAccountRow
                                if (!useAccountDistribution)
                                    paymentAccountRow.VoucherRow = voucherRow;

                                //Add VoucherRow
                                voucherHead.VoucherRow.Add(voucherRow);

                                #endregion

                                #region AccountBalance

                                // Update balance on account
                                if (voucherHead.Template == false && voucherRow.AccountStd != null)
                                {
                                    //Edit NI 100217: Update AccountBalance for all accounts from Silverlight in Complete method
                                    //Update AccountBalance
                                    //abm.SetAccountBalance(entities, actorCompanyId, voucherRow.AccountStd.AccountId, currentAccountYear.AccountYearId, voucherRow.Amount, true, false, accountBalances);
                                }

                                #endregion

                                #region PaymentAccountRow

                                // Set voucher number on accounting row
                                paymentAccountRow.Text = String.Format("{0} {1}", GetText(3887, "Ver:"), voucherHead.VoucherNr) + (paymentAccountRow.Text != null && paymentAccountRow.Text.Trim() != String.Empty ? " - " + paymentAccountRow.Text : String.Empty);

                                #endregion
                            }

                            #endregion
                        }

                        #region Status

                        //Set PaymentRow to status Checked
                        paymentRow.Status = (int)SoePaymentStatus.Checked;

                        #endregion

                        #region Connect PaymentRow to VoucherHead

                        //Set VoucherHead on PaymentRow
                        paymentRow.VoucherHead = voucherHead;

                        if (usePayDateAndValidate && paymentRow.PayDate != voucherDate)
                            paymentRow.PayDate = voucherDate;

                        //Check if VoucherHead should be added
                        // Add Voucher if:
                        // 1) Setting SoeInvoiceToVoucherHeadType.VoucherPerInvoice
                        // 2) Setting SoeInvoiceToVoucherHeadType.MergeVoucherOnInvoiceDate, but no matching VoucherHead found
                        // 3) Setting SoeInvoiceToVoucherHeadType.MergeVoucherOnInvoiceDate, but is last VoucherHead
                        //if (!foundExistingVoucherHead || voucherHeadCounter == paymentRows.Count)
                        //{
                        //    //Add VoucherHead
                        //    entities.VoucherHead.AddObject(voucherHead);
                        //}

                        #endregion

                        #endregion
                    }

                    #region Account distribution
                    foreach (var head in voucherHeads)
                    {
                        if (useAccountDistribution)
                        {
                            // Get distributed
                            var distributedRows = VoucherManager.ApplyAutomaticAccountDistribution(entities, head.VoucherRow.ToList(), accountStds, accountDims, accountInternals, actorCompanyId, null, true, invoiceToVoucherRowType == (int)SoeInvoiceToVoucherRowType.MergeVoucherRowsOnAccount);
                            // Empty
                            head.VoucherRow.Clear();
                            // Add distributed
                            head.VoucherRow.AddRange(distributedRows);
                        }
                        //Add VoucherHead
                        entities.VoucherHead.AddObject(head);
                    }
                    #endregion

                    //Reapply voucher numbers and lock while saving
                    new VoucherNumberLock(entities)
                        .AddVouchers(voucherHeads)
                        .SetVoucherNumbers();

                    if (result.Success)
                        result = SaveChanges(entities, transaction);

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
                    result.IdDict = GetVouchersDict(voucherHeads);
                    //result.IntegerValue = voucherHeads.Count;
                }
                else
                    base.LogTransactionFailed(this.ToString(), this.log);

                if (transactionScopeOption != ConfigSettings.TRANSACTIONSCOPEOPTION_ATTACH)
                    entities.Connection.Close();
            }
            #endregion

            return result;
        }

        public ActionResult SaveVoucherFromSupplierInvoiceAttestRows(CompEntities entities, TransactionScopeOption transactionScopeOption, SupplierInvoice invoice, int actorCompanyId)
        {
            if (invoice == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "SupplierInvoice");

            // Default result is successful
            ActionResult result = new ActionResult();

            List<VoucherHead> voucherHeads = new List<VoucherHead>();

            try
            {
                if (entities.Connection.State != ConnectionState.Open)
                    entities.Connection.Open();

                // Possible to include this method in a running Transaction
                using (TransactionScope transaction = new TransactionScope(transactionScopeOption, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                {
                    #region Prereq

                    // Get AccountYear
                    AccountYear accountYear = AccountManager.GetAccountYear(entities, DateTime.Today, actorCompanyId);
                    if (accountYear == null)
                        return new ActionResult((int)ActionResultSave.EntityNotFound, "AccountYear");

                    // Get voucher series
                    VoucherSeries voucherSeries = null;
                    if (invoice.VoucherHead != null && invoice.VoucherHead.VoucherSeries.AccountYearId == accountYear.AccountYearId)
                    {
                        // Use same voucher series as existing voucher
                        voucherSeries = invoice.VoucherHead.VoucherSeries;
                    }
                    else
                    {
                        // Use voucher series in company setting
                        int supplierInvoiceSeriesTypeId = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.SupplierInvoiceVoucherSeriesType, 0, actorCompanyId, 0);
                        voucherSeries = VoucherManager.GetVoucherSerieByType(entities, supplierInvoiceSeriesTypeId, accountYear.AccountYearId);
                    }
                    if (voucherSeries == null)
                        return new ActionResult((int)ActionResultSave.EntityNotFound, "VoucherSeries");
                    long voucherNr = (voucherSeries.VoucherNrLatest.HasValue ? voucherSeries.VoucherNrLatest.Value : 0) + 1;

                    // Get AccountPeriod from date
                    AccountPeriod accountPeriod = AccountManager.GetAccountPeriod(entities, accountYear.AccountYearId, DateTime.Today, actorCompanyId);
                    if (accountPeriod == null)
                        return new ActionResult((int)ActionResultSave.EntityNotFound, "AccountPeriod");
                    if (accountPeriod.Status != (int)TermGroup_AccountStatus.Open)
                        return new ActionResult((int)ActionResultSave.AccountYearNotOpen, accountPeriod.From + "-" + accountPeriod.To);

                    // Get all internal accounts once
                    List<AccountInternal> accountInternals = AccountManager.GetAccountInternals(entities, actorCompanyId, true);

                    #endregion

                    #region VoucherHead

                    // Text
                    string voucherHeadText = GetText(1806, "Lev.fakt.") + " " + invoice.SeqNr.Value;
                    if (!String.IsNullOrEmpty(invoice.ActorName))
                        voucherHeadText += ", " + invoice.ActorName;
                    //if (!String.IsNullOrEmpty(invoice.Origin.Description))
                    //    voucherHeadText += ", " + invoice.Origin.Description;

                    VoucherHead voucherHead = new VoucherHead()
                    {
                        VoucherNr = voucherNr,
                        Date = DateTime.Today,
                        Text = voucherHeadText,
                        Status = accountPeriod.Status,
                        VatVoucher = false,
                        Template = false,

                        //Set FK
                        ActorCompanyId = actorCompanyId,

                        // References
                        AccountPeriod = accountPeriod,
                        VoucherSeries = voucherSeries,
                    };
                    SetCreatedProperties(voucherHead);
                    entities.VoucherHead.AddObject(voucherHead);

                    // Update voucher series with last voucher nr and date
                    voucherSeries.VoucherNrLatest = voucherHead.VoucherNr;
                    voucherSeries.VoucherDateLatest = voucherHead.Date;

                    #endregion

                    #region VoucherRows

                    // Invert rows on existing voucher
                    if (invoice.VoucherHead != null)
                    {
                        foreach (var row in invoice.VoucherHead.VoucherRow.Where(r => r.State == (int)SoeEntityState.Active))
                        {
                            VoucherRow newRow = new VoucherRow()
                            {
                                Amount = -row.Amount,
                                AmountEntCurrency = -row.AmountEntCurrency,
                                AccountStd = row.AccountStd,
                                Date = voucherHead.Date,
                                Text = String.Format("{0} {1}", GetText(3887, "Ver:"), invoice.VoucherHead.VoucherNr)
                            };
                            foreach (var accRow in row.AccountInternal)
                            {
                                AccountInternal accountInternal = accountInternals.FirstOrDefault(a => a.AccountId == accRow.AccountId);
                                if (accountInternal != null)
                                    newRow.AccountInternal.Add(accountInternal);
                            }
                            voucherHead.VoucherRow.Add(newRow);
                        }
                    }

                    // Add rows from supplier invoice
                    foreach (var suppRow in invoice.SupplierInvoiceRow.Where(r => r.State == (int)SoeEntityState.Active))
                    {
                        foreach (var suppAccRow in suppRow.SupplierInvoiceAccountRow.Where(r => r.State == (int)SoeEntityState.Active && r.Type == (int)AccountingRowType.AccountingRow))
                        {
                            VoucherRow newRow = new VoucherRow()
                            {
                                Amount = suppAccRow.Amount,
                                AmountEntCurrency = suppAccRow.AmountEntCurrency,
                                AccountStd = suppAccRow.AccountStd,
                                Date = voucherHead.Date,
                            };
                            foreach (var accRow in suppAccRow.AccountInternal)
                            {
                                AccountInternal accountInternal = accountInternals.FirstOrDefault(a => a.AccountId == accRow.AccountId);
                                if (accountInternal != null)
                                    newRow.AccountInternal.Add(accountInternal);
                            }
                            voucherHead.VoucherRow.Add(newRow);
                        }
                    }

                    #endregion

                    #region Invoice

                    if (invoice.VoucherHead == null)
                        invoice.VoucherHead = voucherHead;
                    else
                        invoice.VoucherHead2 = voucherHead;

                    voucherHeads.Add(voucherHead);

                    // Update Origin status
                    invoice.Origin.Status = (int)SoeOriginStatus.Voucher;

                    #endregion

                    if (result.Success)
                        result = SaveChanges(entities, transaction);

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
                    result.IdDict = GetVouchersDict(voucherHeads);
                    //result.IntegerValue = voucherHeads.Count;
                }
                else
                    base.LogTransactionFailed(this.ToString(), this.log);

                if (transactionScopeOption != ConfigSettings.TRANSACTIONSCOPEOPTION_ATTACH)
                    entities.Connection.Close();
            }

            return result;
        }

        public ActionResult SaveVoucherFromCompanyGroup(CompEntities entities, TransactionScope transaction, VoucherHeadDTO voucherInput, List<VoucherRow> voucherRows, int voucherSeriesId, int accountPeriodId, int actorCompanyId, bool alwaysMerge = true, string loggText = "", bool updateSeqNr = true)
        {
            bool newVoucher = false;
            long seqNbr = 0;

            bool voucherDateChanged = false, voucherTextChanged = false;
            DateTime voucherDateOld = DateTime.Now, voucherDateNew = DateTime.Now;
            string voucherTextOld = "", voucherTextNew = "";

            #region Prereq

            VoucherSeries voucherSeries = GetVoucherSerie(entities, voucherSeriesId, actorCompanyId, true);
            if (voucherSeries == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, "VoucherSeries");

            AccountPeriod accountPeriod = AccountManager.GetAccountPeriod(entities, accountPeriodId);
            if (accountPeriod == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, "AccountPeriod");

            int accountYearId = accountPeriod.AccountYearId;

            #endregion

            #region VoucherHead

            // Get existing Voucher
            VoucherHead voucherHead = GetVoucherHead(entities, voucherInput.VoucherHeadId, false, true, true, false);
            if (voucherHead == null)
            {
                #region VoucherHead Add

                newVoucher = true;

                if (voucherInput.Template)
                {
                    // Templates are stored against the first period found in current account year
                    accountPeriod = AccountManager.GetFirstAccountPeriod(entities, accountYearId, actorCompanyId, true);
                    voucherSeries = GetTemplateVoucherSerie(entities, accountYearId, actorCompanyId);
                }

                // Always get the next available sequence number
                // A check will be made in the GUI if the number has changed from the proposed one
                seqNbr = voucherSeries.VoucherNrLatest.GetValueOrDefault() + 1;

                voucherHead = new VoucherHead()
                {
                    VoucherNr = updateSeqNr ? seqNbr : voucherInput.VoucherNr,
                    Date = voucherInput.Date.Date,
                    Text = voucherInput.Text,
                    Template = voucherInput.Template,
                    VatVoucher = voucherInput.VatVoucher,
                    Status = (int)TermGroup_AccountStatus.Open,
                    CompanyGroupVoucher = true,
                    SourceType = (int)voucherInput.SourceType,

                    //Set FK
                    ActorCompanyId = actorCompanyId,

                    //Set referenecs
                    AccountPeriod = accountPeriod,
                    VoucherSeries = voucherSeries,

                    //Note
                    Note = voucherInput.Note,
                };
                SetCreatedProperties(voucherHead);
                entities.VoucherHead.AddObject(voucherHead);

                // Update voucher serie with last voucher nr and date
                if (updateSeqNr)
                {
                    voucherSeries.VoucherNrLatest = seqNbr;
                    voucherSeries.VoucherDateLatest = voucherInput.Date;
                }

                #endregion
            }
            else
            {

                #region VoucherHead Update

                //Check if voucher date or voucher text is changed               
                if (voucherHead.Date != voucherInput.Date)
                {
                    voucherDateChanged = true;
                    voucherDateOld = voucherHead.Date;
                    voucherDateNew = voucherInput.Date;
                }
                if (voucherHead.Text != voucherInput.Text)
                {
                    voucherTextChanged = true;
                    voucherTextOld = voucherHead.Text;
                    voucherTextNew = voucherInput.Text;
                }

                newVoucher = false;
                voucherHead.Date = voucherInput.Date.Date;
                voucherHead.Text = voucherInput.Text;
                voucherHead.VatVoucher = voucherInput.VatVoucher;
                voucherHead.Note = voucherInput.Note;
                voucherHead.SourceType = (int)voucherInput.SourceType;
                SetModifiedProperties(voucherHead);

                seqNbr = voucherHead.VoucherNr;

                #endregion
            }

            #endregion

            #region VoucherRow

            #region VoucherRow Update/Delete

            // Update or Delete existing VoucherRows
            foreach (VoucherRow voucherRow in voucherHead.ActiveVoucherRows)
            {
                // Try get VoucherRow from input
                VoucherRow voucherRowInput = (from r in voucherRows
                                              where r.VoucherRowId == voucherRow.VoucherRowId
                                              select r).FirstOrDefault();


                if (voucherRowInput != null)
                {
                    #region VoucherRow Update

                    if (!voucherHead.Template)
                    {
                        // Update voucher row history
                        AddVoucherRowHistory(entities, voucherRow, voucherRowInput, TermGroup_VoucherRowHistoryEvent.Modified, actorCompanyId);

                        //Add history row also when voucher date is changed
                        if (voucherDateChanged)
                        {
                            VoucherRowHistory voucherRowHistory = new VoucherRowHistory()
                            {
                                Date = DateTime.Now,
                                UserId = base.UserId,
                                AccountStd = voucherRow.AccountStd,
                                EventType = (int)TermGroup_VoucherRowHistoryEvent.Modified,
                                VoucherHeadIdModified = voucherHead.VoucherHeadId,
                                FieldModified = (int)TermGroup_VoucherRowHistoryField.VoucherDate,
                                EventText = voucherDateOld.ToShortDateString() + " --> " + voucherDateNew.ToShortDateString(),
                            };

                            voucherRow.VoucherRowHistory.Add(voucherRowHistory);

                            voucherDateChanged = false;
                        }

                        //Add history row also when voucher text is changed
                        if (voucherTextChanged)
                        {
                            string textFrom = voucherTextOld == string.Empty ? "<" + GetText(3003, "blankt") + "> " : voucherTextOld;
                            string textTo = voucherTextNew == string.Empty ? " <" + GetText(3003, "blankt") + ">" : voucherTextNew;

                            VoucherRowHistory voucherRowHistory = new VoucherRowHistory()
                            {
                                Date = DateTime.Now,
                                UserId = base.UserId,
                                AccountStd = voucherRow.AccountStd,
                                EventType = (int)TermGroup_VoucherRowHistoryEvent.Modified,
                                VoucherHeadIdModified = voucherHead.VoucherHeadId,
                                FieldModified = (int)TermGroup_VoucherRowHistoryField.VoucherText,
                                EventText = textFrom + " --> " + textTo,
                            };
                            voucherRow.VoucherRowHistory.Add(voucherRowHistory);

                            voucherTextChanged = false;
                        }

                    }

                    // Update existing voucher row
                    voucherRow.Date = voucherRowInput.Date.HasValue ? voucherRowInput.Date.Value.Date : (DateTime?)null;
                    voucherRow.AccountStd = voucherRowInput.AccountStd;
                    voucherRow.Text = voucherRowInput.Text;
                    voucherRow.Quantity = voucherRowInput.Quantity;
                    voucherRow.Amount = voucherRowInput.Amount;

                    // Update AccountInternal
                    voucherRow.AccountInternal.Clear();
                    foreach (AccountInternal accountInternal in voucherRowInput.AccountInternal)
                    {
                        voucherRow.AccountInternal.Add(accountInternal);
                    }

                    // Detach the input row to prevent adding a new
                    base.TryDetachEntity(entities, voucherRowInput);

                    #endregion
                }
                else
                {
                    #region VoucherRow Delete

                    // Delete existing VoucherRow
                    if (voucherRow.State != (int)SoeEntityState.Deleted)
                    {
                        ChangeEntityState(voucherRow, SoeEntityState.Deleted);

                        // Add VoucherRowHistory
                        AddVoucherRowHistory(entities, voucherRow, voucherRowInput, TermGroup_VoucherRowHistoryEvent.Removed, actorCompanyId);
                    }

                    #endregion
                }
            }

            #endregion

            #region VoucherRow Add

            // Get new VoucherRows
            IEnumerable<VoucherRow> voucherRowsToAdd = (from r in voucherRows
                                                        where r.VoucherRowId == 0
                                                        select r).ToList();

            foreach (VoucherRow voucherRowToAdd in voucherRowsToAdd)
            {
                // Add VoucherRow to VoucherHead
                voucherHead.VoucherRow.Add(voucherRowToAdd);

                if (!voucherHead.Template && !newVoucher)
                {
                    // Add VoucherRowHistory
                    AddVoucherRowHistory(entities, voucherRowToAdd, voucherRowToAdd, TermGroup_VoucherRowHistoryEvent.New, actorCompanyId);
                }
            }

            #endregion

            #endregion

            var result = SaveChanges(entities, transaction);
            if (result.Success)
            {
                //Set success properties
                result.IntegerValue = voucherHead.VoucherHeadId;
                result.Value = seqNbr;
                result.StringValue = voucherHead.Text;
                result.InfoMessage = loggText;
            }

            return result;
        }

        public ActionResult CreateVatVoucher(DateTime dateFrom, DateTime dateTo, int actorCompanyId)
        {
            // Default result is successful
            ActionResult result = new ActionResult(true);

            #region Init

            //Used to save AccountBalance after VatVoucher has been saved
            int accountYearId = 0;

            #endregion

            using (var entities = new CompEntities())
            {
                try
                {
                    if (entities.Connection.State != ConnectionState.Open)
                        entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        // Settings
                        int voucherSeriesTypeId = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.AccountingVoucherSeriesTypeVat, 0, actorCompanyId, 0);

                        var accountYears = AccountManager.GetAccountYears(entities, dateFrom, dateTo, actorCompanyId);
                        var currentAccountYear = accountYears.OrderBy(y => y.From).Last();
                        result = AccountManager.ValidateAccountYear(currentAccountYear, dateTo);
                        if (!result.Success)
                            return result;

                        // Get VoucherSeries
                        VoucherSeries voucherSeries = GetVoucherSerieByYear(entities, currentAccountYear.AccountYearId, voucherSeriesTypeId);
                        if (voucherSeries == null)
                            return new ActionResult((int)ActionResultSave.EntityNotFound, "VoucherSeries");

                        // Get AccountPeriod
                        AccountPeriod accountPeriod = AccountManager.GetAccountPeriod(entities, currentAccountYear.AccountYearId, dateTo, actorCompanyId);
                        result = AccountManager.ValidateAccountPeriod(accountPeriod, dateTo);
                        if (!result.Success)
                            return result;

                        // Check that a VAT Voucher dont already exists for given period
                        if (VatVoucherExists(entities, accountPeriod.AccountPeriodId, actorCompanyId))
                        {
                            result = new ActionResult((int)ActionResultSave.VatVoucherExists, GetText(5535, "Det finns ett momsavräkningsverifikat för vald period. Det måste tas bort eller motbokas innan nytt kan skapas"));
                            return result;
                        }

                        long voucherNr = (voucherSeries.VoucherNrLatest.HasValue ? voucherSeries.VoucherNrLatest.Value : 0) + 1;

                        VoucherHead voucherHead = new VoucherHead()
                        {
                            VoucherNr = voucherNr,
                            Date = dateTo.Date,
                            Text = GetText(5506, "Momsavräkning") + " " + dateFrom.ToString("yyyyMMdd") + "-" + dateTo.ToString("yyyyMMdd"),
                            VatVoucher = true,
                            Template = false,
                            Status = (int)TermGroup_AccountStatus.Open,

                            //Set FK
                            ActorCompanyId = actorCompanyId,

                            //References
                            AccountPeriod = accountPeriod,
                            VoucherSeries = voucherSeries,
                        };
                        SetCreatedProperties(voucherHead);
                        entities.VoucherHead.AddObject(voucherHead);

                        // Update voucher serie with last voucher nr and date
                        voucherSeries.VoucherNrLatest = voucherHead.VoucherNr;
                        voucherSeries.VoucherDateLatest = voucherHead.Date;

                        result = CreateVatVoucherRows(entities, voucherHead, accountYears, dateFrom, dateTo, actorCompanyId);

                        new VoucherNumberLock(entities)
                            .AddVoucher(voucherHead)
                            .SetVoucherNumbers();

                        if (result.Success)
                            result = SaveChanges(entities, transaction);

                        if (result.Success)
                            transaction.Complete();
                    }
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    result = new ActionResult(GetText(5638, "Okänt fel uppstod"));
                }
                finally
                {
                    if (result.Success)
                    {
                        //Set success properties
                        AccountBalanceManager(actorCompanyId).CalculateAccountBalanceForAccountsFromVoucher(actorCompanyId, accountYearId, 3);
                    }
                    else
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }

                return result;
            }
        }

        private ActionResult CreateVatVoucherRows(CompEntities entities, VoucherHead voucherHead, List<AccountYear> accountYears, DateTime dateFrom, DateTime dateTo, int actorCompanyId)
        {
            ActionResult result = new ActionResult(true);

            #region Init

            //Settings
            bool useQuantityInVoucher = SettingManager.GetBoolSetting(entities, SettingMainType.Company, (int)CompanySettingType.AccountingUseQuantityInVoucher, 0, actorCompanyId, 0);

            //Cent rounding
            AccountStd accountStdCent = AccountManager.GetAccountStdFromCompanySetting(entities, CompanySettingType.AccountCommonCentRounding, actorCompanyId);

            //Handle theese sysvataccountid's   
            List<int> sysVatAccountIds = new List<int>();
            sysVatAccountIds.Add(49);  //VatNr1 => 10, SysVatAccountId => 49  (Utgående skatt 25% försäljning)
            sysVatAccountIds.Add(50);  //VatNr1 => 11, SysVatAccountId => 50  (Utgående skatt 12% försäljning)
            sysVatAccountIds.Add(51);  //VatNr1 => 12, SysVatAccountId => 51  (Utgående skatt 6% försäljning)
            sysVatAccountIds.Add(57);  //VatNr1 => 30, SysVatAccountId => 57  (Utgående skatt 25% inköp eller Ingående moms omvänd)
            sysVatAccountIds.Add(58);  //VatNr1 => 31, SysVatAccountId => 58  (Utgående skatt 12% inköp)
            sysVatAccountIds.Add(59);  //VatNr1 => 32, SysVatAccountId => 59  (Utgående skatt 6% inköp)
            sysVatAccountIds.Add(123); //VatNr1 => 60, SysVatAccountId => 123 (Utgående skatt 25% inköp import)
            sysVatAccountIds.Add(124); //VatNr1 => 61, SysVatAccountId => 124 (Utgående skatt 12% inköp import)
            sysVatAccountIds.Add(125); //VatNr1 => 62, SysVatAccountId => 125 (Utgående skatt 6% inköp import)
            sysVatAccountIds.Add(68);  //VatNr1 => 48, SysVatAccountId => 68  (Ingående moms att dra av)

            List<AccountStd> accountStds = AccountManager.GetAccountStdsByCompany(entities, actorCompanyId, true);
            List<AccountStd> vatAccounts = accountStds.Where(r => r.SysVatAccountId.HasValue && sysVatAccountIds.Contains(r.SysVatAccountId.Value)).ToList();

            AccountStd accountCommonVatAccountingKredit = AccountManager.GetAccountStdFromCompanySetting(entities, CompanySettingType.AccountCommonVatAccountingKredit, actorCompanyId);
            if (accountCommonVatAccountingKredit == null)
            {
                return new ActionResult { Success = false, ErrorMessage = GetText(7486, "Momsredovisningskonto för kredit saknas") };
            }
            vatAccounts.Add(accountCommonVatAccountingKredit); //Accounting (must be added last because dependent on above) 
            AccountStd accountCommonVatAccountingDebet = AccountManager.GetAccountStdFromCompanySetting(entities, CompanySettingType.AccountCommonVatAccountingDebet, actorCompanyId);

            #endregion

            // Something like this:
            // { [SysVatAccountId: int] = (isPayable: bool, sum: decimal) };
            // Check comment further down to understand why these helper methods are needed.
            var sumBySysVatAccount = new Dictionary<int, (bool, decimal)>();
            void addSysVatAccountSum(bool isPayableSysVatAccount, int sysVatAccountId, decimal amount)
            {
                if (sumBySysVatAccount.ContainsKey(sysVatAccountId))
                {
                    var sum = sumBySysVatAccount[sysVatAccountId];
                    sumBySysVatAccount[sysVatAccountId] = (isPayableSysVatAccount, sum.Item2 + amount);
                }
                else
                {
                    sumBySysVatAccount[sysVatAccountId] = (isPayableSysVatAccount, amount);
                }
            }

            (decimal, decimal) getSysVatAccountSum(bool forPayableSysVatAccount)
            {
                decimal cent = 0;
                decimal total = 0;
                foreach (var sumPart in sumBySysVatAccount.Values.Where(s => s.Item1 == forPayableSysVatAccount))
                {
                    var amount = sumPart.Item2;
                    cent += amount - decimal.Truncate(amount);
                    total += decimal.Truncate(amount);
                }
                return (total, cent);
            }

            bool isPayable = false;
            bool isReceivable = false;
            bool isAccounting = false;
            int voucherRowNr = 1;

            foreach (AccountStd accountStd in vatAccounts)
            {
                isPayable = false;
                isReceivable = false;
                isAccounting = false;

                if (accountCommonVatAccountingKredit.AccountId != accountStd.AccountId)
                {
                    switch (accountStd.SysVatAccountId)
                    {
                        case 49:
                        case 50:
                        case 51:
                        case 57:
                        case 58:
                        case 59:
                        case 123:
                        case 124:
                        case 125:
                            isPayable = true;
                            break;
                        case 68:
                            isReceivable = true;
                            break;
                        default:
                            continue;
                    }
                }

                #region VoucherRow

                decimal amount = 0;
                decimal cent = 0;
                decimal? quantity = null;

                if (isPayable || isReceivable)
                {
                    decimal quantityAggr = 0;
                    foreach (var accountYear in accountYears)
                    {
                        BalanceItemDTO balanceItem = AccountBalanceManager(actorCompanyId).GetBalanceChange(entities, accountYear, dateFrom, dateTo, accountStd, null, actorCompanyId);
                        if (balanceItem.Balance != 0)
                        {
                            addSysVatAccountSum(isPayable, accountStd.SysVatAccountId.Value, balanceItem.Balance);

                            amount += -balanceItem.Balance;
                            quantityAggr += balanceItem.Quantity.HasValue ? balanceItem.Quantity.Value : 0;
                        }
                    }

                    amount = decimal.Round(amount, 2);
                    quantity = quantityAggr == 0 ? null : decimal.Round(quantityAggr, 6);
                }
                else
                {
                    #region Accounting

                    /**
                     * We will end up here as a last iteration over the relevant accounts. This is the amount that is owed to or should be received by the tax authority.
                     * 
                     * The amount in the voucher should mimic the final sum that is represented in the VAT report (i.e. check CreateTaxAuditData method in EconomyReportDataManager).
                     * In the report, the sum for each "cell" is truncated, where each cell represent a specific SysVatAccount. Therefore, we need to deal with the amounts similarily here.
                     * 
                     * If we truncate the amount per account, we will end up with a rounding difference since the report truncates the sum of the accounts for a specific type.
                     * Therefore, we truncate the sums per sysVatAccount and then add them together, in the helper methods.
                     */

                    isAccounting = true;

                    var (payableSumTruncated, payableCent) = getSysVatAccountSum(true);
                    var (receivableSumTruncated, receivableCent) = getSysVatAccountSum(false);

                    amount = payableSumTruncated + receivableSumTruncated;
                    cent = payableCent + receivableCent;

                    quantity = null;

                    if (accountStdCent == null)
                    {
                        amount += cent;
                        cent = 0;
                    }
                    #endregion
                }

                if (amount == 0 && cent == 0 && (quantity == null || quantity == 0))
                    continue;

                var voucherRow = new VoucherRow
                {
                    Text = "",
                    Date = voucherHead.Date,
                    Amount = amount,
                    Quantity = quantity,
                    RowNr = voucherRowNr,
                    //Set references
                    AccountStd = isAccounting && amount > 0 && accountCommonVatAccountingDebet != null ? accountCommonVatAccountingDebet : accountStd,
                };

                voucherRowNr++;
                //Set currency amounts
                CountryCurrencyManager.SetCurrencyAmounts(entities, actorCompanyId, voucherRow);

                //Add to VoucherHead
                voucherHead.VoucherRow.Add(voucherRow);

                if (cent != decimal.Zero)
                {
                    #region Cent rounding row

                    //if (payable + receivable + amount + cent != 0)
                    //    cent = -cent;

                    var centVoucherRow = new VoucherRow
                    {
                        Text = "",
                        Date = voucherHead.Date,
                        Amount = cent,
                        Quantity = null,
                        RowNr = voucherRowNr,
                        //Set references
                        AccountStd = accountStdCent,
                    };
                    voucherRowNr++;
                    //Set currency amounts
                    CountryCurrencyManager.SetCurrencyAmounts(entities, actorCompanyId, centVoucherRow);

                    //Add to VoucherHead
                    voucherHead.VoucherRow.Add(centVoucherRow);

                    #endregion
                }

                #endregion
            }

            return result;
        }

        /// <summary>
        /// Only support inserts.
        /// Inserts VoucherHead + VoucherRows + VoucherRowAccounts efficiently in three steps.
        /// 
        /// Known limitations: 
        /// - The entity context is not updated and the vouchers will be de-attached from the context.
        /// - The method expects VoucherRow.RowNr to have been set by the caller, with unique numbering per VoucherHead.
        ///
        /// </summary>
        public ActionResult BulkInsertVouchers(CompEntities entities, int actorCompanyId, int accountYearId, List<VoucherHead> vouchers, HashSet<int> alreadyExistingVoucherHeadIds)
        {
            try
            {
                using (var transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                {
                    var saveVoucherSeries = BulkUpdate(entities,
                        vouchers.Select(v => v.VoucherSeries)
                                .DistinctBy(v => v.VoucherSeriesId)
                                .ToList(),
                        transaction);

                    if (!saveVoucherSeries.Success)
                        return saveVoucherSeries;

                    var insertVouchersResult = BulkInsert(entities, vouchers, transaction);

                    if (!insertVouchersResult.Success)
                        return insertVouchersResult;

                    // IF this fails, most likely due to the user trying violate unique nr/series/year constraint.
                    var voucherHeadLookup = entities.VoucherHead.Where(vh => vh.ActorCompanyId == actorCompanyId && vh.AccountPeriod.AccountYearId == accountYearId)
                        .Select(vh => new
                        {
                            vh.VoucherHeadId,
                            vh.VoucherNr,
                            vh.VoucherSeriesId,
                            vh.AccountPeriodId,
                        })
                        .ToList()
                        .Where(vh => !alreadyExistingVoucherHeadIds.Contains(vh.VoucherHeadId))
                        .ToDictionary(vh => (vh.VoucherNr, vh.VoucherSeriesId, vh.AccountPeriodId));

                    var voucherRowAccountMapping = new Dictionary<(int, int), List<int>>();
                    var voucherRows = new List<VoucherRow>();
                    int minVoucherHeadId = -1;
                    foreach (var voucher in vouchers)
                    {
                        // Map PK of voucher heads
                        var idItem = voucherHeadLookup[(voucher.VoucherNr, voucher.VoucherSeriesId, voucher.AccountPeriodId)];
                        voucher.VoucherHeadId = idItem.VoucherHeadId;
                        minVoucherHeadId = Math.Min(minVoucherHeadId, voucher.VoucherHeadId);

                        int rowNr = 1;
                        foreach (var voucherRow in voucher.VoucherRow)
                        {
                            // Map FK row -> head
                            voucherRow.VoucherHeadId = voucher.VoucherHeadId;
                            voucherRow.RowNr = rowNr++;
                            voucherRows.Add(voucherRow);
                            foreach (var accountInternal in voucherRow.AccountInternal)
                            {
                                // Setup mapping for account internals
                                if (voucherRowAccountMapping.TryGetValue((voucher.VoucherHeadId, voucherRow.RowNr.Value), out var accountIds))
                                {
                                    voucherRowAccountMapping[(voucher.VoucherHeadId, voucherRow.RowNr.Value)].Add(accountInternal.AccountId);
                                }
                                else
                                {
                                    accountIds = new List<int> { accountInternal.AccountId };
                                    voucherRowAccountMapping[(voucher.VoucherHeadId, voucherRow.RowNr.Value)] = accountIds;
                                }
                            }
                        }
                        TryDetachEntity(entities, voucher);
                    }

                    var insertVoucherRowsResult = BulkInsert(entities, voucherRows, transaction);

                    if (!insertVoucherRowsResult.Success)
                        return insertVoucherRowsResult;

                    // Get inserted voucher row ids so we can map them with the account internals
                    var insertedVoucherRowIds = entities.VoucherRow.Where(
                             vr => vr.VoucherHead.ActorCompanyId == actorCompanyId &&
                             vr.VoucherHead.AccountPeriod.AccountYearId == accountYearId &&
                             vr.VoucherHeadId >= minVoucherHeadId // Limit to the inserted vouchers
                         )
                        .Select(vr => new
                        {
                            vr.VoucherRowId,
                            vr.VoucherHeadId,
                            RowNr = vr.RowNr ?? 0,
                        })
                        .ToList();

                    var accountMappings = new List<VoucherRowAccount>();
                    foreach (var insertedVoucherRowId in insertedVoucherRowIds)
                    {
                        if (voucherRowAccountMapping.TryGetValue((insertedVoucherRowId.VoucherHeadId, insertedVoucherRowId.RowNr), out var accountIds))
                            foreach (var accountId in accountIds)
                                accountMappings.Add(new VoucherRowAccount { VoucherRowId = insertedVoucherRowId.VoucherRowId, AccountId = accountId });
                    }

                    var insertMappingsResult = InsertVoucherRowAccount(entities, accountMappings);
                    if (!insertMappingsResult.Success)
                        return insertMappingsResult;

                    transaction.Complete();
                    return insertMappingsResult;
                }
            }
            catch (Exception ex)
            {
                this.LogError(ex, this.log);
                return new ActionResult(ex);
            }
        }

        /// <summary>
        /// Bulk insert VoucherRowAccount in batches.
        /// Raw SQL is not good, but in this case the trade-offs for using EF were not considered worth it as that would require us
        /// to update the model to declare a mapping table for VoucherRowAccount which does not exist today.
        /// </summary>
        /// <param name="entities"></param>
        /// <param name="rows"></param>
        /// <returns></returns>
        private ActionResult InsertVoucherRowAccount(CompEntities entities, List<VoucherRowAccount> rows)
        {
            int maxParameters = 2098;
            int maxRows = 999;
            int maxRowsPerBatch = Math.Min(maxParameters / VoucherRowAccount.ColumnCount, maxRows);

            int handled = 0;
            while (handled < rows.Count)
            {
                int batchCount = Math.Min(maxRowsPerBatch, rows.Count - handled);

                var sb = new StringBuilder();
                var parameters = new List<SqlParameter>();

                sb.AppendLine("INSERT INTO VoucherRowAccount (VoucherRowId, AccountId) VALUES");
                for (int i = 0; i < batchCount; i++)
                {
                    int paramIndexVoucherRow = i * VoucherRowAccount.ColumnCount;
                    int paramIndexAccount = paramIndexVoucherRow + 1;
                    sb.AppendLine($"(@p{paramIndexVoucherRow}, @p{paramIndexAccount})");
                    sb.Append(i < batchCount - 1 ? ',' : ';');

                    var row = rows[handled + i];
                    parameters.Add(new SqlParameter($"@p{paramIndexVoucherRow}", row.VoucherRowId));
                    parameters.Add(new SqlParameter($"@p{paramIndexAccount}", row.AccountId));
                }

                var operation = sb.ToString();
                int objectsAffected = entities.ExecuteStoreCommand(operation, parameters.ToArray());
                var result = new ActionResult(objectsAffected == batchCount);
                if (!result.Success)
                    return result;

                handled += batchCount;
            }
            return new ActionResult();
        }

        private Collection<VoucherRow> ConvertToVoucherRows(CompEntities entities, List<AccountingRowDTO> accountingRowDTOs, VoucherHeadDTO voucherHeadDTO, int actorCompanyId, List<AccountStd> accountStds, List<AccountInternal> accountInternals)
        {
            Collection<VoucherRow> rows = new Collection<VoucherRow>();
            Dictionary<int, VoucherRow> parentRowDict = new Dictionary<int, VoucherRow>();

            AccountInternal accountInternal;
            // Diff can occur 
            decimal amountEntSum = 0;

            foreach (AccountingRowDTO accountingRowDTO in accountingRowDTOs.OrderBy(r => r.RowNr))
            {
                // Do not include empty rows
                if (accountingRowDTO.Dim1Id == 0 && accountingRowDTO.DebitAmount - accountingRowDTO.CreditAmount == 0)
                    continue;

                var voucherRow = new VoucherRow
                {
                    VoucherRowId = accountingRowDTO.InvoiceRowId,
                    Text = accountingRowDTO.Text,
                    Amount = accountingRowDTO.DebitAmount - accountingRowDTO.CreditAmount,
                    Quantity = accountingRowDTO.Quantity,
                    Date = voucherHeadDTO != null ? voucherHeadDTO.Date : accountingRowDTO.Date,
                    State = (int)accountingRowDTO.State,
                    RowNr = accountingRowDTO.RowNr,
                    StartDate = accountingRowDTO.StartDate,
                    NumberOfPeriods = accountingRowDTO.NumberOfPeriods,
                };

                //Set currency amounts
                CountryCurrencyManager.SetCurrencyAmounts(entities, actorCompanyId, voucherRow);

                amountEntSum += voucherRow.AmountEntCurrency;


                #region Accounts

                // Get standard account
                //voucherRow.AccountStd = AccountManager.GetAccountStd(entities, accountingRowDTO.Dim1Id, actorCompanyId, true, false, true);
                voucherRow.AccountStd = accountStds.FirstOrDefault(a => a.AccountId == accountingRowDTO.Dim1Id);
                //voucherRow.AccountId = accountingRowDTO.Dim1Id;

                if (accountingRowDTO.Dim2Id != 0 && (accountInternal = accountInternals.FirstOrDefault(a => a.AccountId == accountingRowDTO.Dim2Id)) != null)
                    voucherRow.AccountInternal.Add(accountInternal);
                if (accountingRowDTO.Dim3Id != 0 && (accountInternal = accountInternals.FirstOrDefault(a => a.AccountId == accountingRowDTO.Dim3Id)) != null)
                    voucherRow.AccountInternal.Add(accountInternal);
                if (accountingRowDTO.Dim4Id != 0 && (accountInternal = accountInternals.FirstOrDefault(a => a.AccountId == accountingRowDTO.Dim4Id)) != null)
                    voucherRow.AccountInternal.Add(accountInternal);
                if (accountingRowDTO.Dim5Id != 0 && (accountInternal = accountInternals.FirstOrDefault(a => a.AccountId == accountingRowDTO.Dim5Id)) != null)
                    voucherRow.AccountInternal.Add(accountInternal);
                if (accountingRowDTO.Dim6Id != 0 && (accountInternal = accountInternals.FirstOrDefault(a => a.AccountId == accountingRowDTO.Dim6Id)) != null)
                    voucherRow.AccountInternal.Add(accountInternal);

                #endregion

                #region Account distribution

                // Set parent row
                if (accountingRowDTO.ParentRowId != 0 && parentRowDict.ContainsKey(accountingRowDTO.ParentRowId))
                    voucherRow.Parent = parentRowDict[accountingRowDTO.ParentRowId];

                if (accountingRowDTO.AccountDistributionHeadId != 0)
                    voucherRow.AccountDistributionHead = AccountDistributionManager.GetAccountDistributionHead(entities, accountingRowDTO.AccountDistributionHeadId);

                // If a child row refers to this row, add it to a dictionary
                // so we can get hold of it when adding the child row
                if (accountingRowDTOs.Any(r => r.ParentRowId == accountingRowDTO.TempInvoiceRowId) && !parentRowDict.ContainsKey(accountingRowDTO.TempInvoiceRowId))
                    parentRowDict.Add(accountingRowDTO.TempInvoiceRowId, voucherRow);

                #endregion

                rows.Add(voucherRow);

            }
            // take care of diff
            VoucherRow lastRow = rows.LastOrDefault();
            if (lastRow != null && amountEntSum != 0)
                lastRow.AmountEntCurrency -= amountEntSum;

            return rows;
        }

        private Collection<VoucherRow> CreateVoucherRows(CompEntities entities, VoucherHeadDTO voucherHeadDTO, int actorCompanyId, List<AccountStd> accountStds, List<AccountInternal> accountInternals)
        {
            Collection<VoucherRow> rows = new Collection<VoucherRow>();
            Dictionary<int, VoucherRow> parentRowDict = new Dictionary<int, VoucherRow>();

            // Get internal accounts (Dim2-6)
            AccountInternal accountInternal;
            // Diff can occur 
            decimal amountEntSum = 0;

            foreach (var row in voucherHeadDTO.Rows)
            {
                VoucherRow voucherRow = new VoucherRow()
                {
                    Text = row.Text,
                    Amount = row.Amount,
                    Quantity = row.Quantity,
                    Date = voucherHeadDTO.Date,
                    State = (int)row.State,
                    StartDate = row.StartDate,
                    NumberOfPeriods = row.NumberOfPeriods
                };

                if (voucherRow.VoucherRowId > 0)
                    voucherRow.VoucherRowId = voucherRow.VoucherRowId;

                //Set currency amounts
                CountryCurrencyManager.SetCurrencyAmounts(entities, actorCompanyId, voucherRow);

                amountEntSum += voucherRow.AmountEntCurrency;


                #region Accounts

                // Get standard account
                voucherRow.AccountStd = AccountManager.GetAccountStd(entities, row.Dim1Id, actorCompanyId, true, false);

                if (row.Dim2Id != 0 && (accountInternal = accountInternals.FirstOrDefault(a => a.AccountId == row.Dim2Id)) != null)
                    voucherRow.AccountInternal.Add(accountInternal);
                if (row.Dim3Id != 0 && (accountInternal = accountInternals.FirstOrDefault(a => a.AccountId == row.Dim3Id)) != null)
                    voucherRow.AccountInternal.Add(accountInternal);
                if (row.Dim4Id != 0 && (accountInternal = accountInternals.FirstOrDefault(a => a.AccountId == row.Dim4Id)) != null)
                    voucherRow.AccountInternal.Add(accountInternal);
                if (row.Dim5Id != 0 && (accountInternal = accountInternals.FirstOrDefault(a => a.AccountId == row.Dim5Id)) != null)
                    voucherRow.AccountInternal.Add(accountInternal);
                if (row.Dim6Id != 0 && (accountInternal = accountInternals.FirstOrDefault(a => a.AccountId == row.Dim6Id)) != null)
                    voucherRow.AccountInternal.Add(accountInternal);

                #endregion

                #region Account distribution

                // Set parent row
                if (row.ParentRowId.HasValue && row.ParentRowId.Value != 0 && parentRowDict.ContainsKey(row.ParentRowId.Value))
                    voucherRow.Parent = parentRowDict[row.ParentRowId.Value];

                if (row.AccountDistributionHeadId.HasValue && row.AccountDistributionHeadId != 0)
                    voucherRow.AccountDistributionHead = AccountDistributionManager.GetAccountDistributionHead(entities, row.AccountDistributionHeadId.Value);

                /*if (row.AccountDistributionHeadId.HasValue && row.AccountDistributionHeadId != 0) {
                    var accountDistributionHead = AccountDistributionManager.GetAccountDistributionHead(entities, row.AccountDistributionHeadId.Value);
                    if(accountDistributionHead != null)
                    {
                        // Create cloned head
                        var cloneHead = accountDistributionHead.CloneDTO();
                        cloneHead.AccountDistributionHeadId = 0;

                        foreach(var accRow in accountDistributionHead.AccountDistributionRow)
                        {
                            // Create cloned row
                            var cloneRow = accRow.CloneDTO();
                            cloneRow.AccountDistributionHeadId = 0;
                            cloneRow.AccountDistributionRowId = 0;

                            // Add to head
                            cloneHead.AccountDistributionRow.Add(cloneRow);
                        }

                        voucherRow.AccountDistributionHead = cloneHead;
                    }
                }*/

                // If a child row refers to this row, add it to a dictionary
                // so we can get hold of it when adding the child row
                if (voucherHeadDTO.Rows.Any(r => r.ParentRowId == row.TempRowId) && !parentRowDict.ContainsKey(row.TempRowId))
                    parentRowDict.Add(row.TempRowId, voucherRow);

                #endregion

                rows.Add(voucherRow);

            }

            // take care of diff
            VoucherRow lastRow = rows.LastOrDefault();
            if (lastRow != null && amountEntSum != 0)
                lastRow.AmountEntCurrency -= amountEntSum;

            return rows;
        }

        public ActionResult DeleteVoucher(int voucherHeadId)
        {
            using (CompEntities entities = new CompEntities())
            {
                // Get voucher head
                VoucherHead voucherHead = GetVoucherHead(entities, voucherHeadId, false, true, false, false);
                if (voucherHead == null)
                    return new ActionResult((int)ActionResultDelete.EntityNotFound, "VoucherHead");

                // Only templates can be removed
                if (!voucherHead.Template)
                    return new ActionResult((int)ActionResultDelete.OnlyTemplateCanBeRemoved, GetText(11004, "Endast mallar kan tas bort"));

                if (!voucherHead.VoucherRow.IsLoaded)
                    voucherHead.VoucherRow.Load();

                // Remove the rows
                foreach (VoucherRow voucherRow in voucherHead.VoucherRow.ToList())
                {

                    //Make sure voucher Row History is loaded
                    if (!voucherRow.VoucherRowHistory.IsLoaded)
                        voucherRow.VoucherRowHistory.Load();

                    //Make sure AccountInternal is loaded
                    if (!voucherRow.AccountInternal.IsLoaded)
                        voucherRow.AccountInternal.Load();

                    // Remove mapping records between row and account
                    voucherRow.AccountInternal.Clear();

                    //Delete Voucher Row History
                    foreach (VoucherRowHistory history in voucherRow.VoucherRowHistory.ToList())
                    {
                        if (!DeleteEntityItem(entities, history).Success)
                            return new ActionResult((int)ActionResultDelete.VoucherRowHistoryNotDeleted);
                    }
                    // Remove records between row and history
                    voucherRow.VoucherRowHistory.Clear();

                    if (!SaveEntityItem(entities, voucherRow).Success)
                        return new ActionResult((int)ActionResultDelete.VoucherRowAccountNotDeleted);

                    if (!DeleteEntityItem(entities, voucherRow).Success)
                        return new ActionResult((int)ActionResultDelete.VoucherRowAccountNotDeleted);
                }

                // Remove voucher
                return DeleteEntityItem(entities, voucherHead);
            }
        }

        public ActionResult DeleteVouchersOnlySuperSupport(List<int> voucherHeadIds)
        {
            ActionResult result = new ActionResult();
            var successList = new List<long>();
            var failingList = new List<long>();

            foreach (int voucherHeadId in voucherHeadIds)
            {
                VoucherHead head = GetVoucherHead(voucherHeadId);
                if (head == null)
                    continue;

                if (DeleteVoucherOnlySuperSupport(voucherHeadId).Success)
                    successList.Add(head.VoucherNr);
                else
                    failingList.Add(head.VoucherNr);
            }

            string msg = "";
            if (successList.Count > 0)
            {
                msg = GetText(11001, "Tagit bort verifikat med verifikatnummer") + ":";
                foreach (var nbr in successList)
                {
                    msg += " " + nbr.ToString() + ",";
                }
                msg = msg.Substring(0, msg.Length - 1);
            }

            if (failingList.Count > 0)
            {
                if (msg != "") msg += "\n";
                msg += GetText(11002, "Misslyckades att ta bort verifikat med verifikatnummer") + ":";
                foreach (var nbr in failingList)
                {
                    msg += " " + nbr.ToString() + ",";
                }
                msg = msg.Substring(0, msg.Length - 1);
            }

            result.StringValue = msg;

            return result;
        }

        public ActionResult DeleteVoucherOnlySuperSupport(int voucherHeadId, bool checkTransfer = false)
        {
            ActionResult result = new ActionResult();
            result.Success = false;

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    if (checkTransfer)
                    {
                        var transfer = (from t in entities.CompanyGroupTransferRow
                                        .Include("CompanyGroupTransferHead")
                                        where t.VoucherHeadId == voucherHeadId
                                        select t).FirstOrDefault();

                        if (transfer != null)
                        {
                            var noOfExistingRows = (from r in entities.CompanyGroupTransferRow
                                                    where r.CompanyGroupTransferHeadId == transfer.CompanyGroupTransferHeadId &&
                                                    r.VoucherHead != null
                                                    select r).Count();

                            transfer.VoucherHeadId = null;
                            transfer.CompanyGroupTransferHead.Status = noOfExistingRows > 1 ? (int)CompanyGroupTransferStatus.PartlyDeleted : (int)CompanyGroupTransferStatus.Deleted;

                            SetModifiedProperties(transfer);
                            SetModifiedProperties(transfer.CompanyGroupTransferHead);

                            result = SaveChanges(entities);
                            if (!result.Success)
                                return result;
                        }
                    }

                    int voucherDeleted = entities.DeleteVoucherSuperSupport(voucherHeadId);
                    if (voucherDeleted > 0)
                        result.Success = true;
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                }
            }

            return result;
        }

        /// <summary>
        /// Update voucher number, is not recommeded to use except for supersupportadmin 
        /// </summary>        
        /// <param name="voucherHeadId"></param>
        /// <param name="newVoucherNr"></param>
        /// <returns></returns>
        public ActionResult UpdateVoucherNumberOnlySuperSupport(int voucherHeadId, int newVoucherNr)
        {
            using (CompEntities entities = new CompEntities())
            {
                VoucherHead voucherHead = GetVoucherHead(entities, voucherHeadId, false, true, false, false);
                if (voucherHead == null)
                    return new ActionResult((int)ActionResultDelete.EntityNotFound, "VoucherHead");

                voucherHead.VoucherNr = newVoucherNr;
                SetModifiedProperties(voucherHead);

                return SaveChanges(entities);
            }
        }

        public ActionResult UpdateVoucherHeadsStatus(int actorCompanyId, int accountPeriodId, int status)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.VoucherHead.NoTracking();
            return UpdateVoucherHeadsStatus(entities, actorCompanyId, accountPeriodId, status);
        }

        public ActionResult UpdateVoucherHeadsStatus(CompEntities entities, int actorCompanyId, int accountPeriodId, int status)
        {
            List<VoucherHead> voucherHeads = GetVoucherHeadsByPeriod(entities, accountPeriodId, actorCompanyId);
            foreach (VoucherHead voucherHead in voucherHeads)
            {
                voucherHead.Status = status;
            }

            return SaveChanges(entities);
        }

        public ActionResult CopyVoucherTemplatesCheckExisting(int actorCompanyId, int accountYearId)
        {
            ActionResult result = new ActionResult();

            try
            {
                using (CompEntities entities = new CompEntities())
                {
                    AccountYear currentAccountYear = AccountManager.GetAccountYear(entities, accountYearId, false);
                    if (currentAccountYear == null)
                        return new ActionResult((int)ActionResultDelete.EntityNotFound, "AccountYear");

                    AccountYear previousAccountYear = AccountManager.GetPreviousAccountYear(entities, currentAccountYear.From, currentAccountYear.ActorCompanyId, false);

                    AccountPeriod accountPeriod = AccountManager.GetFirstAccountPeriod(entities, accountYearId, actorCompanyId, true);
                    if (accountPeriod == null)
                        return new ActionResult((int)ActionResultDelete.EntityNotFound, "AccountPeriod");

                    var voucherSerie = entities.VoucherSeries
                        .Where(vs => vs.AccountYearId == currentAccountYear.AccountYearId)
                        .Where(vs => vs.VoucherSeriesType.Template && vs.VoucherSeriesType.ActorCompanyId == actorCompanyId)
                        .FirstOrDefault();

                    if (voucherSerie == null)
                        return new ActionResult((int)ActionResultSave.EntityNotFound, "Template VoucherSeries");

                    List<VoucherHead> existingVoucherTemplates = GetVoucherTemplatesByYear(entities, currentAccountYear.AccountYearId, actorCompanyId, true, true)
                        .ToList()
                        .OrderBy(vh => vh.VoucherNr).ToList();

                    List<VoucherHead> voucherTemplates = previousAccountYear != null ? GetVoucherTemplatesByYear(entities, previousAccountYear.AccountYearId, actorCompanyId, true, true)
                        .ToList()
                        .OrderBy(vh => vh.VoucherNr).ToList()
                        : new List<VoucherHead>();

                    voucherSerie.VoucherNrLatest = existingVoucherTemplates.IsNullOrEmpty() ? 0 : existingVoucherTemplates.Max(t => t.VoucherNr);

                    foreach (VoucherHead voucherTemplate in voucherTemplates)
                    {
                        // Check for duplicates - just matching name
                        if (existingVoucherTemplates.Any(h => h.Text == voucherTemplate.Text))
                            continue;

                        if (voucherTemplate.BatchId == null)
                            voucherTemplate.BatchId = Guid.NewGuid();

                        voucherSerie.VoucherNrLatest++;

                        VoucherHead newVoucherHead = new VoucherHead()
                        {
                            Date = voucherTemplate.Date.Date,
                            Status = voucherTemplate.Status,
                            Template = voucherTemplate.Template,
                            Text = voucherTemplate.Text,
                            TypeBalance = voucherTemplate.TypeBalance,
                            VatVoucher = voucherTemplate.VatVoucher,
                            VoucherNr = voucherSerie.VoucherNrLatest.Value,
                            BatchId = voucherTemplate.BatchId,

                            //Set FK
                            ActorCompanyId = actorCompanyId,

                            //Set references
                            AccountPeriodId = accountPeriod.AccountPeriodId,
                            VoucherSeriesId = voucherSerie.VoucherSeriesId,
                        };

                        voucherSerie.VoucherDateLatest = newVoucherHead.Date;

                        if (!voucherTemplate.VoucherRow.IsLoaded)
                            voucherTemplate.VoucherRow.Load();

                        foreach (VoucherRow voucherRowTemplate in voucherTemplate.VoucherRow)
                        {
                            if (!voucherRowTemplate.AccountStdReference.IsLoaded)
                                voucherRowTemplate.AccountStdReference.Load();

                            var newVoucherRow = new VoucherRow
                            {
                                Amount = voucherRowTemplate.Amount,
                                AmountEntCurrency = voucherRowTemplate.AmountEntCurrency,
                                Date = voucherRowTemplate.Date,
                                Merged = voucherRowTemplate.Merged,
                                Quantity = voucherRowTemplate.Quantity,
                                State = voucherRowTemplate.State,
                                Text = voucherRowTemplate.Text,
                                RowNr = voucherRowTemplate.RowNr,
                                //Set references
                                AccountId = voucherRowTemplate.AccountStd.AccountId,
                            };

                            if (!voucherRowTemplate.AccountInternal.IsLoaded)
                                voucherRowTemplate.AccountInternal.Load();

                            foreach (AccountInternal accountInternal in voucherRowTemplate.AccountInternal)
                            {
                                newVoucherRow.AccountInternal.Add(accountInternal);
                            }

                            newVoucherHead.VoucherRow.Add(newVoucherRow);
                        }

                        existingVoucherTemplates.Add(newVoucherHead);
                        //Add item
                        result = AddEntityItem(entities, newVoucherHead, "VoucherHead");
                        if (!result.Success)
                            return result;
                    }

                    result = SaveChanges(entities);
                }
            }
            catch (Exception ex)
            {
                base.LogError(ex, this.log);
                return new ActionResult(false);
            }
            return result;
        }

        #endregion

        #region VoucherRow

        public IQueryable<VoucherRow> GetVoucherRowsQuery(CompEntities entities, int actorCompanyId, int accountYearId)
        {
            return from vr in entities.VoucherRow
                   where vr.VoucherHead.ActorCompanyId == actorCompanyId &&
                   vr.VoucherHead.AccountPeriod.AccountYearId == accountYearId &&
                   vr.State == (int)SoeEntityState.Active
                   select vr;
        }

        public List<VoucherRow> GetVoucherRows(int voucherHeadId, bool loadAccountInternal = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.VoucherRow.NoTracking();
            return GetVoucherRows(entities, voucherHeadId, loadAccountInternal);
        }

        public List<VoucherRow> GetVoucherRows(CompEntities entities, int voucherHeadId, bool loadAccountInternal = false)
        {
            var actorCompanyId = base.ActorCompanyId;
            List<VoucherRow> voucherRows = (from vr in entities.VoucherRow.
                                                Include("AccountStd.Account")
                                            where vr.VoucherHead.VoucherHeadId == voucherHeadId &&
                                            vr.State == (int)SoeEntityState.Active &&
                                            vr.VoucherHead.ActorCompanyId == actorCompanyId
                                            select vr).ToList();

            foreach (VoucherRow voucherRow in voucherRows)
            {
                if (loadAccountInternal)
                {
                    //AccountInternal
                    if (!voucherRow.AccountInternal.IsLoaded)
                        voucherRow.AccountInternal.Load();

                    foreach (AccountInternal accountInternal in voucherRow.AccountInternal)
                    {
                        if (!accountInternal.AccountReference.IsLoaded)
                            accountInternal.AccountReference.Load();
                        if (!accountInternal.Account.AccountDimReference.IsLoaded)
                            accountInternal.Account.AccountDimReference.Load();
                    }
                }
            }

            return voucherRows;
        }

        public List<VoucherRow> GetVoucherRows(CompEntities entities, int actorCompanyId, DateTime dateFrom, DateTime dateTo)
        {
            List<VoucherRow> voucherRows = (from vr in entities.VoucherRow.
                                                Include("VoucherHead")
                                            where vr.VoucherHead.ActorCompanyId == actorCompanyId &&
                                            vr.State == (int)SoeEntityState.Active &&
                                            vr.VoucherHead.Date >= dateFrom &&
                                            vr.VoucherHead.Date <= dateTo
                                            select vr).ToList();

            return voucherRows;
        }

        public List<AccountDateBalanceDTO> GetAccountDateBalances(CompEntities entities, int actorCompanyId, DateTime dateFrom, DateTime dateTo)
        {
            return entities.VoucherRow
                .Where(vr => vr.VoucherHead.ActorCompanyId == actorCompanyId && vr.State == 0 && vr.VoucherHead.Date >= dateFrom && vr.VoucherHead.Date <= dateTo)
                .GroupBy(vr => new { vr.AccountId, vr.VoucherHead.Date })
                .Select(g => new AccountDateBalanceDTO
                {
                    AccountId = g.Key.AccountId,
                    Date = g.Key.Date,
                    Amount = g.Sum(vr => vr.Amount),
                    RowCount = g.Count()
                }).ToList();
        }

        public List<VoucherRowDTO> GetVoucherRowsDTOFromSelection(CompEntities entities, int actorCompanyId, List<AccountIntervalDTO> accountIntervals, bool projectReport, DateTime? dateFromParam, DateTime? dateToParam, int? voucherNrFromParam, int? voucherNrToParam, int? voucherSeriesTypeNrFromParam, int? voucherSeriesTypeNrToParam, bool hasVoucherSeriesTypeSelection, List<int> voucherSeriesTypeSelection)
        {
            List<VoucherRowDTO> voucherRows = new List<VoucherRowDTO>();

            AccountDim accountDimStd = AccountManager.GetAccountDimStd(actorCompanyId);
            List<AccountDim> internalDims = AccountManager.GetAccountDimsByCompany(actorCompanyId, false, true, true);
            if (accountDimStd == null)
                return voucherRows;

            //Check EvaluatedSelection
            DateTime dateFrom = dateFromParam.HasValue && !projectReport ? dateFromParam.Value : new DateTime(1900, 1, 1);
            DateTime dateTo = (dateToParam.HasValue
                && dateToParam.Value != CalendarUtility.DATETIME_DEFAULT)
                ? dateToParam.Value : DateTime.Now;
            int voucherNrFrom = voucherNrFromParam.HasValue ? voucherNrFromParam.Value : 0;
            int voucherNrTo = voucherNrToParam.HasValue ? voucherNrToParam.Value : Int32.MaxValue;
            int voucherSeriesTypeNrFrom = voucherSeriesTypeNrFromParam.HasValue ? voucherSeriesTypeNrFromParam.Value : 0;
            int voucherSeriesTypeNrTo = voucherSeriesTypeNrToParam.HasValue ? voucherSeriesTypeNrToParam.Value : Int32.MaxValue;

            List<VoucherHeadDTO> voucherHeads = GetVoucherHeadDTOs(actorCompanyId, dateFrom, dateTo);

            foreach (VoucherHeadDTO voucherHead in voucherHeads)
            {
                if ((voucherHead.Date >= dateFrom && voucherHead.Date <= dateTo) &&
                   (voucherHead.VoucherNr >= voucherNrFrom && voucherHead.VoucherNr <= voucherNrTo) &&
                   (!hasVoucherSeriesTypeSelection || voucherSeriesTypeSelection.Contains(voucherHead.VoucherSeriesTypeId)) &&
                   (!voucherHead.Template))
                {
                    foreach (VoucherRowDTO rowDTO in voucherHead.Rows)
                    {
                        if ((rowDTO.VoucherSeriesTypeNr >= voucherSeriesTypeNrFrom && rowDTO.VoucherSeriesTypeNr <= voucherSeriesTypeNrTo) &&
                           (VoucherRowDTOContainsAccounts(rowDTO, accountIntervals, internalDims, accountDimStd.AccountDimId)))
                            voucherRows.Add(rowDTO);
                    }
                }
            }

            return (from vr in voucherRows
                    orderby vr.VoucherNr ascending, vr.Date ascending, vr.VoucherRowId ascending
                    select vr).ToList();
        }

        public List<VoucherRowDTO> GetVoucherRowsDTOFromSelection(CompEntities entities, EvaluatedSelection es)
        {
            List<VoucherRowDTO> voucherRows = new List<VoucherRowDTO>();

            AccountDim accountDimStd = AccountManager.GetAccountDimStd(es.ActorCompanyId);
            List<AccountDim> internalDims = AccountManager.GetAccountDimsByCompany(es.ActorCompanyId, false, true, true);
            if (accountDimStd == null)
                return voucherRows;

            //Check EvaluatedSelection
            DateTime dateFrom = es.HasDateInterval && !es.SSTD_ProjectReport ? es.DateFrom : new DateTime(1900, 1, 1);
            DateTime dateTo = es.HasDateInterval ? es.DateTo : DateTime.Now;
            int voucherNrFrom = es.SV_HasVoucherNrInterval ? es.SV_VoucherNrFrom : 0;
            int voucherNrTo = es.SV_HasVoucherNrInterval ? es.SV_VoucherNrTo : Int32.MaxValue;
            int voucherSeriesTypeNrFrom = es.SV_HasVoucherSeriesTypeNrInterval ? es.SV_VoucherSeriesTypeNrFrom : es.SV_VoucherSeriesTypeNrTo;
            int voucherSeriesTypeNrTo = es.SV_HasVoucherSeriesTypeNrInterval ? es.SV_VoucherSeriesTypeNrTo : Int32.MaxValue;

            List<VoucherHeadDTO> voucherHeads = GetVoucherHeadDTOs(es.ActorCompanyId, dateFrom, dateTo);

            foreach (VoucherHeadDTO voucherHead in voucherHeads)
            {
                if ((voucherHead.Date >= dateFrom && voucherHead.Date <= dateTo) &&
                   (voucherHead.VoucherNr >= voucherNrFrom && voucherHead.VoucherNr <= voucherNrTo) &&
                   (!voucherHead.Template))
                {
                    foreach (VoucherRowDTO rowDTO in voucherHead.Rows)
                    {
                        if ((rowDTO.VoucherSeriesTypeNr >= voucherSeriesTypeNrFrom && rowDTO.VoucherSeriesTypeNr <= voucherSeriesTypeNrTo) &&
                           (VoucherRowDTOContainsAccounts(rowDTO, es.SA_AccountIntervals, internalDims, accountDimStd.AccountDimId)))
                            voucherRows.Add(rowDTO);
                    }
                }
            }

            return (from vr in voucherRows
                    orderby vr.VoucherNr ascending, vr.Date ascending, vr.VoucherRowId ascending
                    select vr).ToList();
        }

        public List<SearchVoucherRowDTO> SearchVoucherRowsDto(int actorCompanyId, SearchVoucherRowsAngDTO dto)
        {
            //Build search from search dto
            DateTime? from = dto.VoucherDateFrom;
            DateTime? to = dto.VoucherDateTo;
            int sFrom = dto.VoucherSeriesIdFrom;
            int sTo = dto.VoucherSeriesIdTo;
            decimal dFrom = dto.DebitFrom;
            decimal dTo = dto.DebitTo;
            decimal kFrom = dto.CreditFrom;
            decimal kTo = dto.CreditTo;
            decimal aFrom = dto.AmountFrom;
            decimal aTo = dto.AmountTo;
            string text = dto.VoucherText;
            DateTime? createdFrom = dto.CreatedFrom;
            DateTime? createdTo = dto.CreatedTo;
            string createdBy = dto.CreatedBy;
            int d1Id = dto.Dim1AccountId;
            string d1From = dto.Dim1AccountFr;
            string d1To = dto.Dim1AccountTo;
            int d2Id = dto.Dim2AccountId;
            string d2From = dto.Dim2AccountFr;
            string d2To = dto.Dim2AccountTo;
            int d3Id = dto.Dim3AccountId;
            string d3From = dto.Dim3AccountFr;
            string d3To = dto.Dim3AccountTo;
            int d4Id = dto.Dim4AccountId;
            string d4From = dto.Dim4AccountFr;
            string d4To = dto.Dim4AccountTo;
            int d5Id = dto.Dim5AccountId;
            string d5From = dto.Dim5AccountFr;
            string d5To = dto.Dim5AccountTo;
            int d6Id = dto.Dim6AccountId;
            string d6From = dto.Dim6AccountFr;
            string d6To = dto.Dim6AccountTo;
            int[] voucherSeriesTypeIds = dto.VoucherSeriesTypeIds ?? Array.Empty<int>();


            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.VoucherRow.NoTracking();
            return SearchVoucherRows(entities, actorCompanyId, from, to, sFrom, sTo, dFrom, dTo, kFrom, kTo, aFrom, aTo, text, createdFrom, createdTo, createdBy, d1Id, d1From, d1To, d2Id, d2From, d2To, d3Id, d3From, d3To, d4Id, d4From, d4To, d5Id, d5From, d5To, d6Id, d6From, d6To, voucherSeriesTypeIds);
        }

        private List<SearchVoucherRowDTO> SearchVoucherRows(CompEntities entities, int actorCompanyId, DateTime? from, DateTime? to, int sFrom, int sTo, decimal dFrom, decimal dTo, decimal kFrom, decimal kTo, decimal aFrom, decimal aTo, string text, DateTime? createdFrom, DateTime? createdTo, string createdBy, int d1Id, string d1From, string d1To, int d2Id, string d2From, string d2To, int d3Id, string d3From, string d3To, int d4Id, string d4From, string d4To, int d5Id, string d5From, string d5To, int d6Id, string d6From, string d6To, int[] voucherSeriesTypeIds)
        {
            List<SearchVoucherRowDTO> dtoList = new List<SearchVoucherRowDTO>();

            bool checkDim2 = false;
            bool checkDim3 = false;
            bool checkDim4 = false;
            bool checkDim5 = false;
            bool checkDim6 = false;

            using var entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            entitiesReadOnly.VoucherRow.NoTracking();
            entitiesReadOnly.VoucherHead.NoTracking();
            entitiesReadOnly.Invoice.NoTracking();
            IQueryable<VoucherRow> query = from vr in entitiesReadOnly.VoucherRow
                                            .Include("VoucherHead.VoucherSeries.VoucherSeriesType")
                                            .Include("VoucherHead.Invoice.Origin")
                                            .Include("AccountStd.Account")
                                            .Include("AccountInternal.Account")
                                           where vr.VoucherHead.ActorCompanyId == actorCompanyId &&
                                           !vr.VoucherHead.Template &&
                                           vr.State == 0
                                           select vr;

            #region Dates

            if (from != null && to != null)
            {
                if (from == to)
                {
                    DateTime fromDate = new DateTime(((DateTime)from).Year, ((DateTime)from).Month, ((DateTime)from).Day);
                    query = query.Where(r => r.VoucherHead.Date == fromDate);
                }
                else
                {
                    //Both are not null, but have different value
                    DateTime fromDate = new DateTime(((DateTime)from).Year, ((DateTime)from).Month, ((DateTime)from).Day);
                    query = query.Where(r => r.VoucherHead.Date >= fromDate);
                    DateTime toDate = new DateTime(((DateTime)to).Year, ((DateTime)to).Month, ((DateTime)to).Day).AddDays(1);
                    query = query.Where(r => r.VoucherHead.Date < toDate);
                }
            }
            else
            {
                if (from != null)
                {
                    DateTime fromDate = new DateTime(((DateTime)from).Year, ((DateTime)from).Month, ((DateTime)from).Day);
                    query = query.Where(r => r.VoucherHead.Date >= fromDate);
                }

                if (to != null)
                {
                    DateTime toDate = new DateTime(((DateTime)to).Year, ((DateTime)to).Month, ((DateTime)to).Day).AddDays(1);
                    query = query.Where(r => r.VoucherHead.Date < toDate);
                }
            }

            #endregion

            #region VoucherSeriesId

            if (sFrom != 0 && voucherSeriesTypeIds.Length == 0)
            {
                List<int> voucherSeriesIds = new List<int>();

                if (sTo != 0)
                {
                    if (sFrom == sTo)
                    {
                        voucherSeriesIds = (from vs in entities.VoucherSeries
                                            where vs.VoucherSeriesType.ActorCompanyId == actorCompanyId &&
                                            vs.VoucherSeriesTypeId == sFrom &&
                                            vs.VoucherSeriesType.State == (int)SoeEntityState.Active
                                            orderby vs.VoucherSeriesType.VoucherSeriesTypeNr
                                            select vs).Select(r => r.VoucherSeriesId).ToList();
                    }
                    else
                    {
                        voucherSeriesIds = (from vs in entities.VoucherSeries
                                            where vs.VoucherSeriesType.ActorCompanyId == actorCompanyId &&
                                            vs.VoucherSeriesTypeId >= sFrom &&
                                            vs.VoucherSeriesTypeId <= sTo &&
                                            vs.VoucherSeriesType.State == (int)SoeEntityState.Active
                                            orderby vs.VoucherSeriesType.VoucherSeriesTypeNr
                                            select vs).Select(r => r.VoucherSeriesId).ToList();
                    }
                }
                else
                {
                    voucherSeriesIds = (from vs in entities.VoucherSeries
                                        where vs.VoucherSeriesType.ActorCompanyId == actorCompanyId &&
                                        vs.VoucherSeriesTypeId >= sFrom &&
                                        vs.VoucherSeriesType.State == (int)SoeEntityState.Active
                                        orderby vs.VoucherSeriesType.VoucherSeriesTypeNr
                                        select vs).Select(r => r.VoucherSeriesId).ToList();
                }

                query = query.Where(r => voucherSeriesIds.Contains(r.VoucherHead.VoucherSeriesId));
            }
            else if (voucherSeriesTypeIds.Length > 0)
            {
                List<int> voucherSeriesIds = new List<int>();

                voucherSeriesIds = (from vs in entities.VoucherSeries
                                    where vs.VoucherSeriesType.ActorCompanyId == actorCompanyId &&
                                    voucherSeriesTypeIds.Contains(vs.VoucherSeriesTypeId) &&
                                    vs.VoucherSeriesType.State == (int)SoeEntityState.Active
                                    orderby vs.VoucherSeriesType.VoucherSeriesTypeNr
                                    select vs).Select(r => r.VoucherSeriesId).ToList();

                query = query.Where(r => voucherSeriesIds.Contains(r.VoucherHead.VoucherSeriesId));
            }

            #endregion

            #region Amount

            if (dFrom != 0)
            {
                query = query.Where(r => r.Amount >= dFrom);
            }

            if (dTo != 0)
            {
                query = query.Where(r => r.Amount <= dTo);
            }

            if (kFrom != 0)
            {
                kFrom = kFrom > 0 ? Decimal.Negate(kFrom) : kFrom;
                query = query.Where(r => r.Amount <= kFrom);
            }

            if (kTo != 0)
            {
                kTo = kTo > 0 ? Decimal.Negate(kTo) : kTo;
                query = query.Where(r => r.Amount >= kTo);
            }

            if (aFrom != 0)
            {
                query = query.Where(r => r.Amount >= aFrom);
            }

            if (aTo != 0)
            {
                query = query.Where(r => r.Amount <= aTo);
            }

            #endregion

            #region Text

            if (!String.IsNullOrEmpty(text))
            {
                query = query.Where(r => (r.Text.Contains(text.Trim()) || r.VoucherHead.Text.Contains(text.Trim())));
            }

            #endregion

            #region Created

            if (createdFrom != null)
            {
                DateTime check = createdFrom.Value.Date;
                query = query.Where(r => r.VoucherHead.Created.HasValue && r.VoucherHead.Created.Value >= check);
            }

            if (createdTo != null)
            {
                DateTime check = CalendarUtility.GetEndOfDay(createdTo.Value);
                query = query.Where(r => r.VoucherHead.Created.HasValue && r.VoucherHead.Created.Value <= check);
            }

            if (!String.IsNullOrEmpty(createdBy.Trim()))
            {
                query = query.Where(r => (r.VoucherHead.CreatedBy.Trim().ToLower().Equals(createdBy.Trim().ToLower())));
            }

            #endregion

            #region Accounts

            List<int> accountIds = new List<int>();

            if (d1Id != 0)
            {
                if (!String.IsNullOrEmpty(d1From) || !String.IsNullOrEmpty(d1To))
                {
                    var accounts = (from a in entitiesReadOnly.Account
                                        .Include("AccountStd")
                                    where a.AccountDimId == d1Id &&
                                    a.ActorCompanyId == actorCompanyId &&
                                    a.State == (int)SoeEntityState.Active
                                    orderby a.AccountNr
                                    select a).ToList();

                    if (String.IsNullOrEmpty(d1From))
                    {
                        d1From = accounts.First().AccountNr;
                    }

                    if (String.IsNullOrEmpty(d1To))
                    {
                        d1To = accounts.Last().AccountNr;
                    }

                    accountIds.AddRange(accounts.Where(a => Validator.IsAccountInInterval(a.AccountNr, d1Id, new AccountIntervalDTO() { AccountDimId = d1Id, AccountNrFrom = d1From, AccountNrTo = d1To })).Select(a => a.AccountId).ToList());

                    query = query.Where(r => accountIds.Contains(r.AccountStd.AccountId));
                }
            }

            if (d2Id != 0)
            {
                if (!String.IsNullOrEmpty(d2From) || !String.IsNullOrEmpty(d2To))
                {
                    checkDim2 = true;

                    var accounts = (from a in entitiesReadOnly.Account
                                    where a.AccountDimId == d2Id &&
                                    a.ActorCompanyId == actorCompanyId &&
                                    a.State == (int)SoeEntityState.Active
                                    orderby a.AccountNr
                                    select a).ToList();

                    if (String.IsNullOrEmpty(d2From))
                    {
                        d2From = accounts.First().AccountNr;
                    }

                    if (String.IsNullOrEmpty(d2To))
                    {
                        d2To = accounts.Last().AccountNr;
                    }

                    accountIds.AddRange(accounts.Where(a => Validator.IsAccountInInterval(a.AccountNr, d2Id, new AccountIntervalDTO() { AccountDimId = d2Id, AccountNrFrom = d2From, AccountNrTo = d2To })).Select(a => a.AccountId).ToList());
                }
            }

            if (d3Id != 0)
            {
                if (!String.IsNullOrEmpty(d3From) || !String.IsNullOrEmpty(d3To))
                {
                    checkDim3 = true;

                    var accounts = (from a in entitiesReadOnly.Account
                                    where a.AccountDimId == d3Id &&
                                    a.ActorCompanyId == actorCompanyId &&
                                    a.State == (int)SoeEntityState.Active
                                    orderby a.AccountNr
                                    select a).ToList();

                    if (String.IsNullOrEmpty(d3From))
                    {
                        d3From = accounts.First().AccountNr;
                    }

                    if (String.IsNullOrEmpty(d3To))
                    {
                        d3To = accounts.Last().AccountNr;
                    }

                    accountIds.AddRange(accounts.Where(a => Validator.IsAccountInInterval(a.AccountNr, d3Id, new AccountIntervalDTO() { AccountDimId = d3Id, AccountNrFrom = d3From, AccountNrTo = d3To })).Select(a => a.AccountId).ToList());
                }
            }

            if (d4Id != 0)
            {
                if (!String.IsNullOrEmpty(d4From) || !String.IsNullOrEmpty(d4To))
                {
                    checkDim4 = true;

                    var accounts = (from a in entitiesReadOnly.Account
                                    where a.AccountDimId == d4Id &&
                                    a.ActorCompanyId == actorCompanyId &&
                                    a.State == (int)SoeEntityState.Active
                                    orderby a.AccountNr
                                    select a).ToList();

                    if (String.IsNullOrEmpty(d4From))
                    {
                        d4From = accounts.First().AccountNr;
                    }

                    if (String.IsNullOrEmpty(d4To))
                    {
                        d4To = accounts.Last().AccountNr;
                    }

                    accountIds.AddRange(accounts.Where(a => Validator.IsAccountInInterval(a.AccountNr, d4Id, new AccountIntervalDTO() { AccountDimId = d4Id, AccountNrFrom = d4From, AccountNrTo = d4To })).Select(a => a.AccountId).ToList());
                }
            }

            if (d5Id != 0)
            {
                if (!String.IsNullOrEmpty(d5From) || !String.IsNullOrEmpty(d5To))
                {
                    checkDim5 = true;

                    var accounts = (from a in entitiesReadOnly.Account
                                    where a.AccountDimId == d5Id &&
                                    a.ActorCompanyId == actorCompanyId &&
                                    a.State == (int)SoeEntityState.Active
                                    orderby a.AccountNr
                                    select a).ToList();

                    if (String.IsNullOrEmpty(d5From))
                    {
                        d5From = accounts.First().AccountNr;
                    }

                    if (String.IsNullOrEmpty(d5To))
                    {
                        d5To = accounts.Last().AccountNr;
                    }

                    accountIds.AddRange(accounts.Where(a => Validator.IsAccountInInterval(a.AccountNr, d5Id, new AccountIntervalDTO() { AccountDimId = d5Id, AccountNrFrom = d5From, AccountNrTo = d5To })).Select(a => a.AccountId).ToList());
                }
            }

            if (d6Id != 0)
            {
                if (!String.IsNullOrEmpty(d6From) || !String.IsNullOrEmpty(d6To))
                {
                    checkDim6 = true;

                    var accounts = (from a in entitiesReadOnly.Account
                                    where a.AccountDimId == d6Id &&
                                    a.ActorCompanyId == actorCompanyId &&
                                    a.State == (int)SoeEntityState.Active
                                    orderby a.AccountNr
                                    select a).ToList();

                    if (String.IsNullOrEmpty(d6From))
                    {
                        d6From = accounts.First().AccountNr;
                    }

                    if (String.IsNullOrEmpty(d6To))
                    {
                        d6To = accounts.Last().AccountNr;
                    }

                    accountIds.AddRange(accounts.Where(a => Validator.IsAccountInInterval(a.AccountNr, d6Id, new AccountIntervalDTO() { AccountDimId = d6Id, AccountNrFrom = d6From, AccountNrTo = d6To })).Select(a => a.AccountId).ToList());
                }
            }

            #endregion

            List<VoucherRow> voucherRows = query.ToList();

            //Loop list, match to accounts and create dto´s
            foreach (VoucherRow row in voucherRows)
            {
                bool add = true;

                /*
                var head = (from a in CompEntities.VoucherHead.Include("VoucherSeries.VoucherSeriesType")
                            where (a.VoucherHeadId == row.VoucherHead.VoucherHeadId)
                            select a).FirstOrDefault();
                */

                SearchVoucherRowDTO dto = new SearchVoucherRowDTO()
                {
                    VoucherHeadId = row.VoucherHead.VoucherHeadId,
                    VoucherRowId = row.VoucherRowId,
                    VoucherNr = row.VoucherHead.VoucherNr,
                    VoucherText = row.VoucherHead.Text != String.Empty ? row.VoucherHead.Text : row.Text,
                    VoucherSeriesName = row.VoucherHead.VoucherSeries.VoucherSeriesType.Name,
                    VoucherDate = row.VoucherHead.Date,
                    VoucherHead = row.VoucherHead.ToDTO(false, false),
                    Created = row.VoucherHead.Created,
                    CreatedBy = row.VoucherHead.CreatedBy,
                    ActorCompanyId = row.VoucherHead.ActorCompanyId,
                };

                if (row.VoucherHead.Invoice != null && row.VoucherHead.Invoice.Count == 1)
                {
                    //Exactly one invoice attached to the voucher
                    var invoice = row.VoucherHead.Invoice.FirstOrDefault();
                    dto.InvoiceId = invoice.InvoiceId;
                    dto.InvoiceNr = invoice.InvoiceNr;
                    dto.InvoiceOriginType = (SoeOriginType)invoice.Origin.Type;
                    if (invoice.IsCustomerInvoice)
                        dto.InvoiceRegistrationType = (OrderInvoiceRegistrationType)((CustomerInvoice)invoice).RegistrationType;
                }

                if (row.Amount > 0)
                {
                    dto.Debit = row.Amount;
                }
                else
                {
                    dto.Credit = row.Amount;
                }

                if (row.AccountStd != null && row.AccountStd.Account != null && row.AccountStd.Account.AccountDimId == d1Id) //&& accIds.Contains(row.AccountStd.AccountId) 
                {
                    dto.Dim1AccountId = row.AccountStd.AccountId;
                    dto.Dim1AccountName = row.AccountStd.Account.Name;
                    dto.Dim1AccountNr = row.AccountStd.Account.AccountNr;
                }

                AccountInternal dim2 = row.AccountInternal.FirstOrDefault(r => r.Account.AccountDimId == d2Id);

                if (dim2 != null)
                {
                    if (checkDim2 && !accountIds.Contains(dim2.AccountId))
                    {
                        add = false;
                    }

                    dto.Dim2AccountId = dim2.AccountId;
                    dto.Dim2AccountName = dim2.Account.Name;
                    dto.Dim2AccountNr = dim2.Account.AccountNr;
                }
                else
                {
                    if (checkDim2)
                    {
                        add = false;
                    }
                }

                AccountInternal dim3 = row.AccountInternal.FirstOrDefault(r => r.Account.AccountDimId == d3Id);

                if (dim3 != null)
                {
                    if (checkDim3 && !accountIds.Contains(dim3.AccountId))
                    {
                        add = false;
                    }

                    dto.Dim3AccountId = dim3.AccountId;
                    dto.Dim3AccountName = dim3.Account.Name;
                    dto.Dim3AccountNr = dim3.Account.AccountNr;
                }
                else
                {
                    if (checkDim3)
                    {
                        add = false;
                    }
                }

                AccountInternal dim4 = row.AccountInternal.FirstOrDefault(r => r.Account.AccountDimId == d4Id);

                if (dim4 != null)
                {
                    if (checkDim4 && !accountIds.Contains(dim4.AccountId))
                    {
                        add = false;
                    }

                    dto.Dim4AccountId = dim4.AccountId;
                    dto.Dim4AccountName = dim4.Account.Name;
                    dto.Dim4AccountNr = dim4.Account.AccountNr;
                }
                else
                {
                    if (checkDim4)
                    {
                        add = false;
                    }
                }

                AccountInternal dim5 = row.AccountInternal.FirstOrDefault(r => r.Account.AccountDimId == d5Id);

                if (dim5 != null)
                {
                    if (checkDim5 && !accountIds.Contains(dim5.AccountId))
                    {
                        add = false;
                    }

                    dto.Dim5AccountId = dim5.AccountId;
                    dto.Dim5AccountName = dim5.Account.Name;
                    dto.Dim5AccountNr = dim5.Account.AccountNr;
                }
                else
                {
                    if (checkDim5)
                    {
                        add = false;
                    }
                }

                AccountInternal dim6 = row.AccountInternal.FirstOrDefault(r => r.Account.AccountDimId == d6Id);

                if (dim6 != null)
                {
                    if (checkDim6 && !accountIds.Contains(dim6.AccountId))
                    {
                        add = false;
                    }

                    dto.Dim6AccountId = dim6.AccountId;
                    dto.Dim6AccountName = dim6.Account.Name;
                    dto.Dim6AccountNr = dim6.Account.AccountNr;
                }
                else
                {
                    if (checkDim6)
                    {
                        add = false;
                    }
                }

                if (add)
                    dtoList.Add(dto);
            }

            return dtoList;
        }

        public List<SearchVoucherRowDTO> GetVoucherTransactions(int accountId, int accountYearId, int accountPeriodIdFrom, int accountPeriodIdTo)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.VoucherRow.NoTracking();
            return GetVoucherTransactions(entities, accountId, accountYearId, accountPeriodIdFrom, accountPeriodIdTo);
        }

        public List<SearchVoucherRowDTO> GetVoucherTransactions(CompEntities entities, int accountId, int accountYearId, int accountPeriodIdFrom, int accountPeriodIdTo)
        {
            DateTime? dateFrom = null;
            DateTime? dateTo = null;
            List<SearchVoucherRowDTO> items = new List<SearchVoucherRowDTO>();

            #region prereq

            int actorCompanyId = base.ActorCompanyId;
            List<AccountDim> accountDims = AccountManager.GetAccountDimsByCompany(actorCompanyId, loadAccounts: true);

            if (accountYearId != 0)
            {
                var accountYear = AccountManager.GetAccountYear(accountYearId, true);
                if (accountYear != null)
                {
                    if (accountPeriodIdFrom != 0)
                    {
                        var period = accountYear.AccountPeriod.FirstOrDefault(p => p.AccountPeriodId == accountPeriodIdFrom);
                        if (period != null)
                            dateFrom = period.From;
                    }

                    if (accountPeriodIdTo != 0)
                    {
                        var period = accountYear.AccountPeriod.FirstOrDefault(p => p.AccountPeriodId == accountPeriodIdTo);
                        if (period != null)
                            dateTo = period.To;
                    }

                    if (dateFrom == null && dateTo == null)
                    {
                        dateFrom = accountYear.From;
                        dateTo = accountYear.To;
                    }
                }
            }

            #endregion
            using var entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            entitiesReadOnly.VoucherRow.NoTracking();
            entitiesReadOnly.VoucherHead.NoTracking();
            IQueryable<VoucherRow> query = from vr in entitiesReadOnly.VoucherRow
                                            .Include("VoucherHead.VoucherSeries.VoucherSeriesType")
                                            .Include("AccountStd.Account")
                                            .Include("AccountInternal.Account")
                                           where vr.VoucherHead.ActorCompanyId == actorCompanyId &&
                                           vr.AccountStd.AccountId == accountId &&
                                           vr.State == 0
                                           select vr;

            if (dateFrom.HasValue)
            {
                query = query.Where(r => r.VoucherHead.Date >= dateFrom.Value);
            }

            if (dateTo.HasValue)
            {
                query = query.Where(r => r.VoucherHead.Date < dateTo.Value);
            }

            List<VoucherRow> voucherRows = query.ToList();

            //Loop list, match to accounts and create dto´s
            foreach (VoucherRow row in voucherRows)
            {
                bool add = true;

                SearchVoucherRowDTO dto = new SearchVoucherRowDTO()
                {
                    VoucherHeadId = row.VoucherHead.VoucherHeadId,
                    VoucherRowId = row.VoucherRowId,
                    VoucherNr = row.VoucherHead.VoucherNr,
                    VoucherText = row.Text != string.Empty ? row.Text : row.VoucherHead.Text,
                    VoucherSeriesName = row.VoucherHead.VoucherSeries.VoucherSeriesType.Name,
                    VoucherDate = row.VoucherHead.Date,
                    VoucherHead = row.VoucherHead.ToDTO(false, false),
                    Created = row.VoucherHead.Created,
                    CreatedBy = row.VoucherHead.CreatedBy,
                    ActorCompanyId = row.VoucherHead.ActorCompanyId,
                };

                if (row.Amount > 0)
                {
                    dto.Debit = row.Amount;
                }
                else
                {
                    dto.Credit = row.Amount < 0 ? Decimal.Negate(row.Amount) : row.Amount;
                }

                if (row.AccountStd != null && row.AccountStd.Account != null)
                {
                    dto.Dim1AccountId = row.AccountStd.AccountId;
                    dto.Dim1AccountName = row.AccountStd.Account.AccountNr + " - " + row.AccountStd.Account.Name;
                    dto.Dim1AccountNr = row.AccountStd.Account.AccountNr;
                }

                var type = dto.GetType();
                for (int i = 2; i <= accountDims.Count; i++)
                {
                    var accountInternal = row.AccountInternal.FirstOrDefault(r => r.Account.AccountDimId == accountDims[i - 1].AccountDimId);
                    if (accountInternal != null)
                    {
                        type.GetProperty("Dim" + i.ToString() + "AccountId").SetValue(dto, accountInternal.AccountId);
                        type.GetProperty("Dim" + i.ToString() + "AccountName").SetValue(dto, accountInternal.Account.AccountNr + " - " + accountInternal.Account.Name);
                        type.GetProperty("Dim" + i.ToString() + "AccountNr").SetValue(dto, accountInternal.Account.AccountNr);
                    }
                }

                if (add)
                    items.Add(dto);
            }

            return items;
        }

        public List<VatVerificationVoucherRowDTO> GetVatVerifyVoucherRows(int actorCompanyId, DateTime? from, DateTime? to, decimal excludeDiffAmountLimit)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.VoucherRow.NoTracking();
            return GetVatVerifyVoucherRows(entities, actorCompanyId, from, to, excludeDiffAmountLimit);
        }

        public List<VatVerificationVoucherRowDTO> GetVatVerifyVoucherRows(CompEntities entities, int actorCompanyId, DateTime? from, DateTime? to, decimal excludeDiffAmountLimit)
        {
            List<VatVerificationVoucherRowDTO> dtoList = new List<VatVerificationVoucherRowDTO>();
            using var entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            IQueryable<VoucherRow> query = from vr in entitiesReadOnly.VoucherRow
                                            .Include("VoucherHead.VoucherSeries.VoucherSeriesType")
                                            .Include("AccountStd.Account")
                                            .Include("AccountInternal.Account")
                                           where vr.VoucherHead.ActorCompanyId == actorCompanyId && vr.State == 0
                                           select vr;

            #region Period

            if (from != null)
            {
                DateTime fromDate = new DateTime(((DateTime)from).Year, ((DateTime)from).Month, ((DateTime)from).Day);
                query = query.Where(r => r.VoucherHead.Date >= fromDate);
            }

            if (to != null)
            {
                DateTime toDate = new DateTime(((DateTime)to).Year, ((DateTime)to).Month, ((DateTime)to).Day);
                query = query.Where(r => r.VoucherHead.Date <= toDate);
            }

            #endregion

            #region VatAccounts

            List<int> vatAccountIds = new List<int>();
            vatAccountIds.Add(45);  //VatNr1 => 5, SysVatAccountId => 45 (Momspliktig försäljning)
            vatAccountIds.Add(49);  //VatNr1 => 10, SysVatAccountId => 49 (Utgående skatt 25% försäljning)
            vatAccountIds.Add(50);  //VatNr1 => 11, SysVatAccountId => 50 (Utgående skatt 12% försäljning)
            vatAccountIds.Add(51);  //VatNr1 => 12, SysVatAccountId => 51 (Utgående skatt 6% försäljning)
            vatAccountIds.Add(57);  //VatNr1 => 30, SysVatAccountId => 57 (Utgående skatt 25% inköp)
            vatAccountIds.Add(58);  //VatNr1 => 31, SysVatAccountId => 58 (Utgående skatt 12% inköp)
            vatAccountIds.Add(59);  //VatNr1 => 32, SysVatAccountId => 59 (Utgående skatt 6% inköp)

            List<int> vatSalesAccountIds = new List<int>();
            vatSalesAccountIds.Add(45); //VatNr1 => 5, SysVatAccountId => 45 (Momspliktig försäljning)
            vatSalesAccountIds.Add(56); //VatNr1 => 24, SysVatAccountId => 56 (Inköp av tjänster i Sverige)

            #endregion

            IQueryable<VoucherRow> queryVatSales = query;

            //
            query = query.Where(r => vatAccountIds.Contains(r.AccountStd.SysVatAccountId.Value));
            List<VoucherRow> voucherRows = query.ToList(); //All vouchers that is directly connected to any of theese vataccounts 10,11,12,30,31,32,48            
            //
            queryVatSales = queryVatSales.Where(r => vatSalesAccountIds.Contains(r.AccountStd.SysVatAccountId.Value));
            List<VoucherRow> voucherRowsVatSales = queryVatSales.ToList(); //All vouchers that is directly connected to sysvataccount "momspliktig försäljning"
            //

            var excludeVerificationVoucherNrList = new List<long>();

            bool bVatSalesAccount = false;

            foreach (VoucherRow voucherRow in voucherRows)
            {
                bVatSalesAccount = false;

                if (voucherRow.VoucherHead.VatVoucher.HasValue && voucherRow.VoucherHead.VatVoucher.Value) //Skip all vouchers that is "momsavräkningverifikat"
                    continue;

                //Check if update existing (if same vouchernr and voucherserie) or add new to the dtoList                                
                int updateDtoIndex = dtoList.FindIndex(item => item.VoucherNr == voucherRow.VoucherNr && item.VoucherSeriesName == voucherRow.VoucherHead.VoucherSeries.VoucherSeriesType.Name);

                //PeriodNumber
                int periodNumber = AccountManager.GetAccountPeriod(voucherRow.VoucherHead.AccountPeriodId).PeriodNr;

                //PeriodMonthYear
                string periodMonthYear = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(voucherRow.VoucherHead.Date.Month).Substring(0, 3) + " " + voucherRow.VoucherHead.Date.Year.ToString().Substring(2, 2);

                if (voucherRow.AccountStd.ExcludeVatVerification.HasValue && voucherRow.AccountStd.ExcludeVatVerification.Value)
                {
                    excludeVerificationVoucherNrList.Add(voucherRow.VoucherNr);
                }

                var dto = new VatVerificationVoucherRowDTO
                {
                    VoucherHeadId = voucherRow.VoucherHead.VoucherHeadId,
                    VoucherHead = voucherRow.VoucherHead.ToDTO(false, false),
                    VoucherRowId = voucherRow.VoucherRowId,
                    VoucherNr = voucherRow.VoucherHead.VoucherNr,
                    PeriodNr = periodNumber,
                    PeriodMonthYear = periodMonthYear,
                    VoucherText = voucherRow.VoucherHead.Text != string.Empty ? voucherRow.VoucherHead.Text : voucherRow.Text,
                    VoucherSeriesName = voucherRow.VoucherHead.VoucherSeries.VoucherSeriesType.Name,
                    VoucherDate = voucherRow.VoucherHead.Date,
                    Created = voucherRow.VoucherHead.Created,
                    CreatedBy = voucherRow.VoucherHead.CreatedBy
                };

                switch (voucherRow.AccountStd.SysVatAccountId)
                {
                    case (45):
                    case (56):
                        bVatSalesAccount = true;
                        break;
                    case (49):
                        dto.Vat25Amount = voucherRow.Amount;
                        dto.TaxSalesDueVATAmount = voucherRow.Amount / 0.25m;
                        break;
                    case (50):
                        dto.Vat12Amount = voucherRow.Amount;
                        dto.TaxSalesDueVATAmount = voucherRow.Amount / 0.12m;
                        break;
                    case (51):
                        dto.Vat6Amount = voucherRow.Amount;
                        dto.TaxSalesDueVATAmount = voucherRow.Amount / 0.06m;
                        break;
                    case (57):
                        dto.Tax25Amount = voucherRow.Amount;
                        dto.TaxSalesDueVATAmount = voucherRow.Amount / 0.25m;
                        break;
                    case (58):
                        dto.Tax12Amount = voucherRow.Amount;
                        dto.TaxSalesDueVATAmount = voucherRow.Amount / 0.12m;
                        break;
                    case (59):
                        dto.Tax6Amount = voucherRow.Amount;
                        dto.TaxSalesDueVATAmount = voucherRow.Amount / 0.06m;
                        break;
                }

                //Calculate diff and VATSalesSumAmount                                                
                if (updateDtoIndex >= 0) // Update dtoList
                {
                    dtoList[updateDtoIndex].Vat25Amount += dto.Vat25Amount;
                    dtoList[updateDtoIndex].Vat12Amount += dto.Vat12Amount;
                    dtoList[updateDtoIndex].Vat6Amount += dto.Vat6Amount;
                    dtoList[updateDtoIndex].Tax25Amount += dto.Tax25Amount;
                    dtoList[updateDtoIndex].Tax12Amount += dto.Tax12Amount;
                    dtoList[updateDtoIndex].Tax6Amount += dto.Tax6Amount;
                    dtoList[updateDtoIndex].IncomingVATAmount += dto.IncomingVATAmount;
                    dtoList[updateDtoIndex].TaxSalesDueVATAmount += dto.TaxSalesDueVATAmount;
                    if (!bVatSalesAccount)
                        dtoList[updateDtoIndex].DiffAmount = dtoList[updateDtoIndex].DiffAmount - dto.TaxSalesDueVATAmount;

                }
                else // Add new dto to dtoList (and calculate diff and vatsales only on add new)
                {
                    foreach (VoucherRow rowVatSales in voucherRowsVatSales) //Vouchers with "momspliktig försäljning" or "inköp av tjänster i sverige" account with this vouchernr
                    {
                        if (rowVatSales.VoucherNr == dto.VoucherNr && rowVatSales.VoucherSeriesTypeName == dto.VoucherSeriesName)
                        {
                            dto.VATSalesSumAmount += rowVatSales.Amount;
                        }
                    }

                    dto.DiffAmount = dto.VATSalesSumAmount - dto.TaxSalesDueVATAmount;

                    dtoList.Add(dto);
                }
            }

            if (excludeVerificationVoucherNrList.Count > 0)
                dtoList.RemoveAll(i => excludeVerificationVoucherNrList.Contains(i.VoucherNr));

            dtoList = dtoList.Where(x => Math.Abs(x.DiffAmount) >= excludeDiffAmountLimit).ToList();

            foreach (var item in dtoList)
            {
                item.Vat25Amount = decimal.Negate(item.Vat25Amount);
                item.Vat12Amount = decimal.Negate(item.Vat12Amount);
                item.Vat6Amount = decimal.Negate(item.Vat6Amount);
                item.Tax25Amount = decimal.Negate(item.Tax25Amount);
                item.Tax12Amount = decimal.Negate(item.Tax12Amount);
                item.Tax6Amount = decimal.Negate(item.Tax6Amount);
                item.IncomingVATAmount = decimal.Negate(item.IncomingVATAmount);
                item.TaxSalesDueVATAmount = decimal.Negate(item.TaxSalesDueVATAmount);
                item.VATSalesSumAmount = decimal.Negate(item.VATSalesSumAmount);
                item.DiffAmount = decimal.Negate(item.DiffAmount);
                //dtoList[i].DiffAmount = Math.Abs(dtoList[i].DiffAmount);
            }

            return dtoList;
        }

        /// <summary>
        /// Get VoucherRow with matching accounts. Matches AccountStd and AccountInternals.
        /// </summary>
        /// <param name="voucherHead">The VoucherHead to get VoucherRow from</param>
        /// <param name="invoiceAccountRow">The row to match</param>
        /// <returns>A VoucherRow</returns>
        private List<VoucherRow> GetVoucherRowsWithMatchingAccounts(List<VoucherRow> voucherRows, VoucherRow matchAgainstVoucherRow)
        {
            if (voucherRows == null || matchAgainstVoucherRow == null || matchAgainstVoucherRow.AccountStd == null || matchAgainstVoucherRow.AccountInternal == null)
                return null;

            List<VoucherRow> matchingRows = new List<VoucherRow>();

            //Find VoucherRow with same AccountStd
            var rows = (from vr in voucherRows
                        where vr.AccountId == matchAgainstVoucherRow.AccountStd.AccountId &&
                        vr.AccountInternal.Count == matchAgainstVoucherRow.AccountInternal.Count &&
                        vr.RowNr != matchAgainstVoucherRow.RowNr
                        select vr);

            //Must contain same AccountInternals
            if (rows != null)
            {
                foreach (var voucherRow in rows)
                {
                    bool identical = AccountManager.IsAccountInternalsCollectionIdentical(matchAgainstVoucherRow.AccountInternal.ToList(), voucherRow.AccountInternal.ToList());
                    if (identical)
                        matchingRows.Add(voucherRow);
                }
            }

            return matchingRows;
        }

        /// <summary>
        /// Get VoucherRow from CustomerInvoiceAccountRow. Matches AccountStd and AccountInternals.
        /// </summary>
        /// <param name="voucherHead">The VoucherHead to get VoucherRow from</param>
        /// <param name="invoiceAccountRow">The row to match</param>
        /// <returns>A VoucherRow</returns>
        private VoucherRow GetVoucherRowFromCustomerInvoiceAccountRow(VoucherHead voucherHead, CustomerInvoiceAccountRow invoiceAccountRow)
        {
            if (voucherHead.VoucherRow == null || invoiceAccountRow == null || invoiceAccountRow.AccountStd == null || invoiceAccountRow.AccountInternal == null)
                return null;

            //Find VoucherRow with same AccountStd
            VoucherRow voucherRow = (from vr in voucherHead.VoucherRow
                                     where vr.AccountId == invoiceAccountRow.AccountStd.AccountId &&
                                     vr.AccountInternal.Count == invoiceAccountRow.AccountInternal.Count
                                     select vr).FirstOrDefault();

            //Must contain same AccountInternals
            if (voucherRow != null)
            {
                bool identical = AccountManager.IsAccountInternalsCollectionIdentical(invoiceAccountRow.AccountInternal.ToList(), voucherRow.AccountInternal.ToList());
                if (!identical)
                    voucherRow = null;
            }

            return voucherRow;
        }

        /// <summary>
        /// Get VoucherRow from SupplierInvoiceAccountRow. Matches AccountStd and AccountInternals.
        /// </summary>
        /// <param name="voucherHead">The VoucherHead to get VoucherRow from</param>
        /// <param name="invoiceAccountRow">The row to match</param>
        /// <returns>A VoucherRow</returns>
        private VoucherRow GetVoucherRowFromSupplierInvoiceAccountRow(VoucherHead voucherHead, SupplierInvoiceAccountRow invoiceAccountRow)
        {
            if (voucherHead.VoucherRow == null || invoiceAccountRow == null || invoiceAccountRow.AccountStd == null || invoiceAccountRow.AccountInternal == null)
                return null;

            //Find VoucherRow with same AccountStd
            VoucherRow voucherRow = (from vr in voucherHead.VoucherRow
                                     where vr.AccountId == invoiceAccountRow.AccountStd.AccountId &&
                                     vr.AccountInternal.Count == invoiceAccountRow.AccountInternal.Count
                                     select vr).FirstOrDefault();

            //Must contain same AccountInternals
            if (voucherRow != null)
            {
                bool identical = AccountManager.IsAccountInternalsCollectionIdentical(invoiceAccountRow.AccountInternal.ToList(), voucherRow.AccountInternal.ToList());
                if (!identical)
                    voucherRow = null;
            }

            return voucherRow;
        }

        /// <summary>
        /// Get VoucherRow from PaymentAccountRow. Matches AccountStd and AccountInternals.
        /// </summary>
        /// <param name="voucherHead">The VoucherHead to get VoucherRow from</param>
        /// <param name="paymentAccountRow">The row to match</param>
        /// <returns>A VoucherRow</returns>
        private VoucherRow GetVoucherRowFromPaymentAccountRow(VoucherHead voucherHead, PaymentAccountRow paymentAccountRow)
        {
            if (voucherHead == null || voucherHead.VoucherRow == null || paymentAccountRow == null || paymentAccountRow.AccountStd == null || paymentAccountRow.AccountInternal == null)
                return null;

            //Find VoucherRow with same AccountStd
            VoucherRow voucherRow = (from vr in voucherHead.VoucherRow
                                     where vr.AccountId == paymentAccountRow.AccountStd.AccountId &&
                                     vr.AccountInternal.Count == paymentAccountRow.AccountInternal.Count
                                     select vr).FirstOrDefault();

            //Must contain same AccountInternals
            if (voucherRow != null)
            {
                bool identical = AccountManager.IsAccountInternalsCollectionIdentical(paymentAccountRow.AccountInternal.ToList(), voucherRow.AccountInternal.ToList());
                if (!identical)
                    voucherRow = null;
            }

            return voucherRow;
        }

        /// <summary>
        /// Checks if the given VoucherRow contains the Accounts given in the AccountInterval list.
        /// Also loads the VoucherRow with properties from its VoucherHead
        /// </summary>
        /// <param name="voucherRow">The VoucherRow to check</param>
        /// <param name="accountIntervals">The AccountIntervals the VoucherRow must contain</param>
        /// <param name="accountDimStdId">The AccountDim standard id</param>
        /// <returns>True if the VoucherRow contains ALL Accounts in the given Interval</returns>
        public bool VoucherRowContainsAccounts(VoucherRow voucherRow, List<AccountIntervalDTO> accountIntervals, int accountDimStdId)
        {
            if (voucherRow == null)
                return false;

            //Approve all if no filter given
            if (accountIntervals == null || accountIntervals.Count == 0)
                return true;

            //Validate each AccountInterval (all must be validated to return true)
            foreach (AccountIntervalDTO accountInterval in accountIntervals)
            {
                if (VoucherRowContainsAccount(voucherRow, accountInterval, accountDimStdId))
                    return true;
            }

            return false;
        }

        public bool VoucherRowDTOContainsAccounts(VoucherRowDTO voucherRowDTO, List<AccountIntervalDTO> accountIntervals, List<AccountDim> accountDims, int accountDimStdId)
        {
            if (voucherRowDTO == null || accountDimStdId == 0)
                return false;

            //Approve all if no filter given
            if (accountIntervals == null || accountIntervals.Count == 0)
                return true;

            //Validate each AccountInterval (all must be validated to return true)
            foreach (AccountIntervalDTO accountInterval in accountIntervals)
            {
                if (VoucherRowDTOContainsAccount(voucherRowDTO, accountInterval, accountDimStdId))
                    return true;
            }

            return false;
        }


        /// <summary>
        /// Checks if the given VoucherRow contains the given AccountInterval.
        /// Also loads the VoucherRow with properties from its VoucherHead
        /// </summary>
        /// <param name="voucherRow">The VoucherRow to check</param>
        /// <param name="accountInterval">The AccountInterval the VoucherRow must contain</param>
        /// <param name="accountDimStdId">The AccountDim standard id</param>
        /// <returns>True if the VoucherRow contains ALL Accounts in the given Interval</returns>
        public bool VoucherRowContainsAccount(VoucherRow voucherRow, AccountIntervalDTO accountInterval, int accountDimStdId)
        {
            if (voucherRow == null || accountInterval == null)
                return false;

            bool isAccountStd = accountInterval.AccountDimId == accountDimStdId;
            if (isAccountStd)
            {
                #region AccountStd

                //AccountStd
                if (!voucherRow.AccountStdReference.IsLoaded)
                    voucherRow.AccountStdReference.Load();

                //Account
                if (!voucherRow.AccountStd.AccountReference.IsLoaded)
                    voucherRow.AccountStd.AccountReference.Load();

                if (Validator.IsAccountInInterval(voucherRow.AccountStd.Account.AccountNr, voucherRow.AccountStd.Account.AccountDimId, accountInterval))
                    return true;

                #endregion
            }
            else
            {
                #region AccountInternal

                //AccountInternal
                if (!voucherRow.AccountInternal.IsLoaded)
                    voucherRow.AccountInternal.Load();

                foreach (AccountInternal accountInternal in voucherRow.AccountInternal)
                {
                    //Account
                    if (!accountInternal.AccountReference.IsLoaded)
                        accountInternal.AccountReference.Load();

                    if (accountInternal.Account.AccountDimId == accountInterval.AccountDimId && Validator.IsAccountInInterval(accountInternal.Account.AccountNr, accountInternal.Account.AccountDimId, accountInterval))
                        return true;
                }

                #endregion
            }

            return false;
        }

        public bool VoucherRowDTOContainsAccount(VoucherRowDTO voucherRow, AccountIntervalDTO accountInterval, int accountDimStdId)
        {
            if (voucherRow == null || accountInterval == null)
                return false;

            bool isAccountStd = accountInterval.AccountDimId == accountDimStdId;
            if (isAccountStd)
            {
                #region AccountStd

                if (Validator.IsAccountInInterval(voucherRow.Dim1Nr, accountDimStdId, accountInterval))
                    return true;

                #endregion
            }
            else
            {
                #region AccountInternal


                foreach (AccountInternalDTO accountInternal in voucherRow.AccountInternalDTO_forReports)
                {
                    if (accountInternal.AccountDimId == accountInterval.AccountDimId && Validator.IsAccountInInterval(accountInternal.AccountNr, accountInternal.AccountDimId, accountInterval))
                        return true;
                }

                #endregion
            }

            return false;
        }

        public bool VoucherRowDTOContainsAccountInternals(VoucherRowDTO voucherRowDTO, List<AccountInternalDTO> accountInternals)
        {
            if (voucherRowDTO == null)
                return false;

            //Approve all if no filter given
            if (accountInternals == null || accountInternals.Count == 0)
                return true;

            List<int> accountDimIds = accountInternals.Select(i => i.AccountDimId).Distinct().ToList();

            //VoucherRow must contain all dimensions
            if (accountDimIds.Count > voucherRowDTO.AccountInternalDTO_forReports.Count)
                return false;

            //Validate each AccountDim
            foreach (int accountDimId in accountDimIds)
            {
                List<AccountInternalDTO> accountInternalsForDim = accountInternals.Where(i => i.AccountDimId == accountDimId).ToList();
                bool valid = accountInternalsForDim.Any(accountInternal => VoucherManager.VoucherRowDTOContainsAccountInternal(voucherRowDTO, accountInternal));
                if (!valid)
                    return false;
            }

            return true;
        }

        public bool VoucherRowDTOContainsAccountInternal(VoucherRowDTO voucherRowDTO, AccountInternalDTO accountInternal)
        {
            if (voucherRowDTO == null || accountInternal == null)
                return false;

            return VoucherRowDTOContainsAccountInternal(voucherRowDTO, accountInternal.AccountNr, accountInternal.AccountDimId);
        }

        public bool VoucherRowDTOContainsAccountInternal(VoucherRowDTO voucherRow, string accountNr, int accountDimId)
        {
            if (voucherRow == null)
                return false;

            AccountInternalDTO accountInternal = (from ai in voucherRow.AccountInternalDTO_forReports
                                                  where ai.AccountNr == accountNr &&
                                                  ai.AccountDimId == accountDimId
                                                  select ai).FirstOrDefault();

            return accountInternal != null;
        }

        #endregion

        #region VoucherRowHistory

        public IEnumerable<VoucherRowHistory> GetVoucherRowHistoryFromVoucher(int actorCompanyId, int voucherHeadId)
        {
            using (CompEntities entities = new CompEntities())
            {
                List<VoucherRowHistory> voucherRowHistories = (from vrh in entities.VoucherRowHistory
                                                                .Include("VoucherRow")
                                                               where vrh.VoucherRow.VoucherHead.VoucherHeadId == voucherHeadId &&
                                                               vrh.VoucherRow.VoucherHead.ActorCompanyId == actorCompanyId
                                                               select vrh).ToList();

                foreach (VoucherRowHistory voucherRowHistory in voucherRowHistories)
                {
                    //Load references
                    LoadVoucherRowHistory(voucherRowHistory);
                }

                return (from vrh in voucherRowHistories
                        orderby vrh.RegDate descending, vrh.VoucherRowHistoryId ascending
                        select vrh).ToList();
            }
        }

        public List<VoucherRowHistoryViewDTO> GetVoucherRowHistoryDTO(int actorCompanyId, int voucherHeadId)
        {
            List<VoucherRowHistoryViewDTO> dtos = new List<VoucherRowHistoryViewDTO>();
            using (CompEntities entities = new CompEntities())
            {
                #region Prereq

                List<GenericType> events = base.GetTermGroupContent(TermGroup.VoucherRowHistoryEvent);
                List<GenericType> fields = base.GetTermGroupContent(TermGroup.VoucherRowHistoryField);

                #endregion

                #region VoucherRowHistory

                List<VoucherRowHistory> voucherRowHistories = (from vrh in entities.VoucherRowHistory
                                                                .Include("VoucherRow").Include("User")
                                                               where vrh.VoucherRow.VoucherHead.VoucherHeadId == voucherHeadId &&
                                                               vrh.VoucherRow.VoucherHead.ActorCompanyId == actorCompanyId
                                                               select vrh).ToList();

                foreach (VoucherRowHistory item in voucherRowHistories)
                {
                    VoucherRowHistoryViewDTO dto = new VoucherRowHistoryViewDTO()
                    {
                        EventText = item.EventText,
                        Text = item.Text,
                        Date = item.Date.Value.ToShortDateString(),
                        Time = item.Date.Value.ToShortTimeString(),
                        DateTime = item.Date.Value,
                        UserName = item.User?.LoginName ?? string.Empty,
                        EventType = events?.FirstOrDefault(t => t.Id == item.EventType)?.Name,
                        FieldModified = fields?.FirstOrDefault(t => t.Id == item.FieldModified)?.Name,
                    };
                    dtos.Add(dto);
                }

                #endregion
            }
            return dtos;
        }

        public List<VoucherRowHistory> GetVoucherRowHistoryFromSelection(EvaluatedSelection es, TermGroup_VoucherRowHistorySortField sortField, TermGroup_VoucherRowHistorySortOrder sortOrder, int maxRows, out int rowsFromDB)
        {
            using (CompEntities entities = new CompEntities())
            {
                rowsFromDB = 0;

                List<VoucherRowHistory> voucherRowHistories = new List<VoucherRowHistory>();

                AccountDim accountDimStd = AccountManager.GetAccountDimStd(es.ActorCompanyId);
                if (accountDimStd == null || es == null)
                    return voucherRowHistories;

                var allVoucherRowHistories = (from vrh in entities.VoucherRowHistory
                                                .Include("VoucherRow")
                                              where ((vrh.VoucherRow.VoucherHead.ActorCompanyId == es.ActorCompanyId) &&
                                              (!vrh.VoucherRow.VoucherHead.VoucherSeries.VoucherSeriesType.Template) &&
                                              (vrh.VoucherRow.VoucherHead.VoucherSeries.VoucherSeriesType.State == (int)SoeEntityState.Active) &&
                                              (!es.SV_HasVoucherSeriesTypeNrInterval || (vrh.VoucherRow.VoucherHead.VoucherSeries.VoucherSeriesType.VoucherSeriesTypeNr >= es.SV_VoucherSeriesTypeNrFrom && vrh.VoucherRow.VoucherHead.VoucherSeries.VoucherSeriesType.VoucherSeriesTypeNr <= es.SV_VoucherSeriesTypeNrTo)) &&
                                              (!es.SV_HasVoucherNrInterval || (vrh.VoucherRow.VoucherHead.VoucherNr >= es.SV_VoucherNrFrom && vrh.VoucherRow.VoucherHead.VoucherNr <= es.SV_VoucherNrTo)) &&
                                              (!es.HasDateInterval || (vrh.VoucherRow.VoucherHead.Date >= es.DateFrom && vrh.VoucherRow.VoucherHead.Date <= es.DateTo)) &&
                                              (!es.SU_HasUser || (vrh.UserId >= es.SU_UserId && vrh.UserId <= es.SU_UserId)))
                                              select vrh).ToList();

                rowsFromDB = allVoucherRowHistories.Count;
                int counter = 1;
                foreach (VoucherRowHistory voucherRowHistory in allVoucherRowHistories)
                {
                    if (!VoucherRowContainsAccounts(voucherRowHistory.VoucherRow, es.SA_AccountIntervals, accountDimStd.AccountDimId))
                        continue;
                    if (counter > maxRows)
                        break;

                    //Load references
                    LoadVoucherRowHistory(voucherRowHistory);

                    voucherRowHistories.Add(voucherRowHistory);
                    counter++;
                }

                //Sort collection
                switch (sortField)
                {
                    case TermGroup_VoucherRowHistorySortField.VoucherNr:
                        #region VoucherNr

                        if (sortOrder == TermGroup_VoucherRowHistorySortOrder.Descending)
                        {
                            return (from vrh in voucherRowHistories
                                    orderby vrh.VoucherNr descending, vrh.RegDate descending, vrh.AccountNr descending
                                    select vrh).ToList();
                        }
                        else
                        {
                            return (from vrh in voucherRowHistories
                                    orderby vrh.VoucherNr ascending, vrh.RegDate ascending, vrh.AccountNr ascending
                                    select vrh).ToList();
                        }

                    #endregion
                    case TermGroup_VoucherRowHistorySortField.AccountNr:
                        #region AccountNr

                        if (sortOrder == TermGroup_VoucherRowHistorySortOrder.Descending)
                        {
                            return (from vrh in voucherRowHistories
                                    orderby vrh.AccountNr descending, vrh.VoucherNr descending, vrh.RegDate descending
                                    select vrh).ToList();
                        }
                        else
                        {
                            return (from vrh in voucherRowHistories
                                    orderby vrh.AccountNr ascending, vrh.VoucherNr ascending, vrh.RegDate ascending
                                    select vrh).ToList();
                        }

                    #endregion
                    case TermGroup_VoucherRowHistorySortField.RegDate:
                        #region RegDate

                        if (sortOrder == TermGroup_VoucherRowHistorySortOrder.Descending)
                        {
                            return (from vrh in voucherRowHistories
                                    orderby vrh.RegDate descending, vrh.VoucherNr descending, vrh.AccountNr descending
                                    select vrh).ToList();
                        }
                        else
                        {
                            return (from vrh in voucherRowHistories
                                    orderby vrh.RegDate ascending, vrh.VoucherNr ascending, vrh.AccountNr ascending
                                    select vrh).ToList();
                        }
                        #endregion
                }

                return voucherRowHistories;
            }
        }

        private void LoadVoucherRowHistory(VoucherRowHistory voucherRowHistory)
        {
            if (voucherRowHistory == null)
                return;

            //User
            if (!voucherRowHistory.UserReference.IsLoaded)
                voucherRowHistory.UserReference.Load();

            voucherRowHistory.LoginName = voucherRowHistory.User.LoginName;

            //Account
            if (!voucherRowHistory.AccountStdReference.IsLoaded)
                voucherRowHistory.AccountStdReference.Load();
            if (!voucherRowHistory.AccountStd.AccountReference.IsLoaded)
                voucherRowHistory.AccountStd.AccountReference.Load();

            voucherRowHistory.AccountNr = voucherRowHistory.AccountStd.Account.AccountNr;

            //VoucherHead
            if (!voucherRowHistory.VoucherRow.VoucherHeadReference.IsLoaded)
                voucherRowHistory.VoucherRow.VoucherHeadReference.Load();

            voucherRowHistory.VoucherHeadId = voucherRowHistory.VoucherRow.VoucherHead.VoucherHeadId;
            voucherRowHistory.VoucherNr = voucherRowHistory.VoucherRow.VoucherHead.VoucherNr;
            voucherRowHistory.RegDate = voucherRowHistory.Date.HasValue ? voucherRowHistory.Date.Value.ToShortDateLongTimeString() : voucherRowHistory.RegDate = voucherRowHistory.VoucherRow.VoucherHead.Date.ToShortDateLongTimeString();

            //Quantity
            if (voucherRowHistory.Quantity.HasValue)
                voucherRowHistory.Quantity = Decimal.Round(voucherRowHistory.Quantity.Value, 2);

            //VoucherSeries
            if (!voucherRowHistory.VoucherRow.VoucherHead.VoucherSeriesReference.IsLoaded)
                voucherRowHistory.VoucherRow.VoucherHead.VoucherSeriesReference.Load();

            voucherRowHistory.VoucherSeriesId = voucherRowHistory.VoucherRow.VoucherHead.VoucherSeriesId;

            //VoucherSeriesType
            if (!voucherRowHistory.VoucherRow.VoucherHead.VoucherSeries.VoucherSeriesTypeReference.IsLoaded)
                voucherRowHistory.VoucherRow.VoucherHead.VoucherSeries.VoucherSeriesTypeReference.Load();

            voucherRowHistory.VoucherSeriesTypeNr = voucherRowHistory.VoucherRow.VoucherHead.VoucherSeries.VoucherSeriesType.VoucherSeriesTypeNr;
            voucherRowHistory.VoucherSeriesTypeName = voucherRowHistory.VoucherRow.VoucherHead.VoucherSeries.VoucherSeriesType.Name;

            //AccountPeriod
            if (!voucherRowHistory.VoucherRow.VoucherHead.AccountPeriodReference.IsLoaded)
                voucherRowHistory.VoucherRow.VoucherHead.AccountPeriodReference.Load();

            voucherRowHistory.PeriodNr = voucherRowHistory.VoucherRow.VoucherHead.AccountPeriod.PeriodNr;

            //AccountYear - From, Tom
            if (!voucherRowHistory.VoucherRow.VoucherHead.AccountPeriod.AccountYearReference.IsLoaded)
                voucherRowHistory.VoucherRow.VoucherHead.AccountPeriod.AccountYearReference.Load();

            voucherRowHistory.YearFrom = voucherRowHistory.VoucherRow.VoucherHead.AccountPeriod.AccountYear.From.ToShortDateString();
            voucherRowHistory.YearTo = voucherRowHistory.VoucherRow.VoucherHead.AccountPeriod.AccountYear.To.ToShortDateString();
        }

        public ActionResult AddVoucherRowHistory(CompEntities entities, VoucherRow voucherRow, decimal? amount, decimal? quantity, bool useQuantityInVoucher, bool save, int actorCompanyId)
        {
            var result = new ActionResult(true);

            #region Init

            if (voucherRow == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "VoucherRow");
            if (voucherRow.AccountStd == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "AccountStd");
            if (voucherRow.AccountStd.Account == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "Account");

            #endregion

            #region VoucherRow

            VoucherRowHistory voucherRowHistory = new VoucherRowHistory()
            {
                Text = voucherRow.Text,
                Amount = amount,
                Quantity = quantity,
                Date = DateTime.Now,

                //Set FK
                UserId = base.UserId,

                //Set references
                AccountStd = voucherRow.AccountStd,
            };

            //Set currency amounts
            CountryCurrencyManager.SetCurrencyAmounts(entities, actorCompanyId, voucherRowHistory);

            #region EventText

            string delimiter = Constants.VOUCHERROWHISTORY_EVENTTEXT_DELIMETER.ToString();

            //Date
            if (voucherRow.Date.HasValue)
                voucherRowHistory.EventText = String.Format("{0} [{1}]", GetText(3284, "Ursprungligt datum"), voucherRow.Date.Value.ToShortDateString());

            //AccountStd
            voucherRowHistory.EventText += (!String.IsNullOrEmpty(voucherRowHistory.EventText) ? delimiter : String.Empty) + String.Format("{0} [{1}]", GetText(3285, "Ursprungligt konto"), voucherRow.AccountStd.Account.AccountNr);

            //Text
            if (!String.IsNullOrEmpty(voucherRow.Text))
                voucherRowHistory.EventText += (!String.IsNullOrEmpty(voucherRowHistory.EventText) ? delimiter : String.Empty) + String.Format("{0} [{1}]", GetText(1722, "Ursprunglig text"), voucherRow.Text);

            //Quantity
            if (useQuantityInVoucher && voucherRow.Quantity != null)
                voucherRowHistory.EventText += (!String.IsNullOrEmpty(voucherRowHistory.EventText) ? delimiter : String.Empty) + String.Format("{0} [{1}]", GetText(1718, "Ursprunglig kvantitet"), NumberUtility.GetFormattedDecimalStringValue(voucherRow.Quantity, 2));

            //Amount
            voucherRowHistory.EventText += (!String.IsNullOrEmpty(voucherRowHistory.EventText) ? delimiter : String.Empty) + String.Format("{0} [{1}]", GetText(1717, "Ursprungligt belopp"), NumberUtility.GetFormattedDecimalStringValue(voucherRow.Amount, 2));

            //Merged
            if (voucherRow.Merged)
                voucherRowHistory.EventText += (!String.IsNullOrEmpty(voucherRowHistory.EventText) ? delimiter : String.Empty) + GetText(1805, "Hopslagen");

            #endregion

            voucherRow.VoucherRowHistory.Add(voucherRowHistory);

            #endregion

            if (save)
                result = SaveChanges(entities);

            return result;
        }

        public ActionResult AddVoucherRowHistory(CompEntities entities, VoucherRow voucherRow, VoucherRow voucherRowInput, TermGroup_VoucherRowHistoryEvent historyEvent, int actorCompanyId)
        {
            ActionResult result = new ActionResult(true);

            #region Init

            if (voucherRow == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "VoucherRow");
            if (voucherRow.AccountStd == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "AccountStd");
            if (voucherRow.AccountStd.Account == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "Account");

            #endregion

            #region VoucherRow          

            string text = string.Empty;
            if (voucherRowInput != null)
            {
                text = GetText(4801, "Konto: ") + voucherRowInput.AccountNr;
                text += ", " + GetText(4802, "Text: ") + voucherRowInput.Text;
                text += ", " + GetText(4804, "Belopp: ") + voucherRowInput.Amount;
            }
            else
            {
                text = GetText(4801, "Konto: ") + voucherRow.AccountNr;
                text += ", " + GetText(4802, "Text: ") + voucherRow.Text;
                text += ", " + GetText(4804, "Belopp: ") + voucherRow.Amount;
            }

            //New voucher row added to existing voucher
            if (historyEvent == TermGroup_VoucherRowHistoryEvent.New)
            {
                string eventText = GetText(3001, "Ny rad");
                AddVoucherRowHistory(voucherRow, (int)historyEvent, (int)TermGroup_VoucherRowHistoryField.Unknown, eventText, text);
            }
            //Voucher row removed from existing voucher
            else if (historyEvent == TermGroup_VoucherRowHistoryEvent.Removed)
            {
                string eventText = GetText(3002, "Raderad rad");
                AddVoucherRowHistory(voucherRow, (int)historyEvent, (int)TermGroup_VoucherRowHistoryField.Unknown, eventText, text);
            }
            //Voucher row changed on existing voucher
            else if (historyEvent == TermGroup_VoucherRowHistoryEvent.Modified && voucherRowInput != null)
            {
                //account changed
                if (voucherRow.AccountId != voucherRowInput.AccountId)
                {
                    string eventText = voucherRow.AccountNr + " --> " + voucherRowInput.AccountNr;
                    AddVoucherRowHistory(voucherRow, (int)historyEvent, (int)TermGroup_VoucherRowHistoryField.Account, eventText, text);
                }

                //row text changed
                if (voucherRow.Text != voucherRowInput.Text)
                {
                    string textFrom = voucherRow.Text == string.Empty ? "<" + GetText(3003, "blankt") + "> " : voucherRow.Text;
                    string textTo = voucherRowInput.Text == string.Empty ? " <" + GetText(3003, "blankt") + ">" : voucherRowInput.Text;
                    string eventText = textFrom + " --> " + textTo;

                    AddVoucherRowHistory(voucherRow, (int)historyEvent, (int)TermGroup_VoucherRowHistoryField.RowText, eventText, text);
                }

                //row amount changed
                if (voucherRow.Amount != voucherRowInput.Amount)
                {
                    string eventText = voucherRow.Amount + " --> " + voucherRowInput.Amount;
                    AddVoucherRowHistory(voucherRow, (int)historyEvent, (int)TermGroup_VoucherRowHistoryField.Amount, eventText, text);
                }

                //row quantity changed
                if (voucherRow.Quantity != voucherRowInput.Quantity)
                {
                    string eventText = voucherRow.Quantity + " --> " + voucherRowInput.Quantity;
                    AddVoucherRowHistory(voucherRow, (int)historyEvent, (int)TermGroup_VoucherRowHistoryField.Quantity, eventText, text);
                }

                //internal account changed
                List<AccountInternal> internalAccounts = voucherRow.AccountInternal.ToList();
                List<AccountInternal> internalAccountsInput = voucherRowInput.AccountInternal.ToList();

                foreach (var internalAccount in internalAccounts)
                {
                    AccountInternal internalAccountInput = (from iAI in internalAccountsInput
                                                            where iAI.Account.AccountDimId == internalAccount.Account.AccountDimId
                                                            select iAI).FirstOrDefault();

                    string textFrom = "<" + GetText(3003, "blankt") + "> ";
                    string textTo = "<" + GetText(3003, "blankt") + "> ";
                    int? accountDimId = null;

                    if (internalAccount != null && internalAccount.Account != null)
                    {
                        textFrom = internalAccount.Account.AccountNr + " " + internalAccount.Account.Name;
                        accountDimId = internalAccount.Account.AccountDimId;
                    }

                    if (internalAccountInput != null && internalAccountInput.Account != null)
                    {
                        textTo = internalAccountInput.Account.AccountNr + " " + internalAccountInput.Account.Name;
                        accountDimId = internalAccountInput.Account.AccountDimId;
                    }

                    if (textFrom != textTo)
                    {
                        string eventText = textFrom + " --> " + textTo;
                        AddVoucherRowHistory(voucherRow, (int)historyEvent, (int)TermGroup_VoucherRowHistoryField.InternalAccount, eventText, text, accountDimId);
                    }
                }

                //internal account added
                foreach (var internalAccountInput in internalAccountsInput)
                {
                    AccountInternal internalAccount = null;
                    if (internalAccounts.Count > 0)
                    {
                        internalAccount = (from iAI in internalAccounts
                                           where iAI.Account.AccountDimId == internalAccountInput.Account.AccountDimId
                                           select iAI).FirstOrDefault();
                    }
                    if (internalAccount == null)
                    {
                        string textFrom = "<" + GetText(3003, "blankt") + "> ";
                        string textTo = internalAccountInput.Account.AccountNr + " " + internalAccountInput.Account.Name;
                        string eventText = textFrom + " --> " + textTo;
                        int? accountDimId = internalAccountInput.Account.AccountDimId;
                        AddVoucherRowHistory(voucherRow, (int)historyEvent, (int)TermGroup_VoucherRowHistoryField.InternalAccount, eventText, text, accountDimId);
                    }
                }
            }

            #endregion

            return result;
        }

        public ActionResult AddVoucherRowHistory(VoucherRow voucherRow, int eventType, int fieldModified, string eventText, string text, int? accountDimId = null)
        {
            ActionResult result = new ActionResult(true);

            VoucherRowHistory voucherRowHistory = new VoucherRowHistory()
            {
                Date = DateTime.Now,
                UserId = base.UserId,
                AccountStd = voucherRow.AccountStd,
                EventType = eventType,
                VoucherRowId = voucherRow.VoucherRowId,
                FieldModified = fieldModified,
                EventText = eventText,
                Text = text,
                AccountDimId = accountDimId,
            };

            voucherRow.VoucherRowHistory.Add(voucherRowHistory);

            return result;
        }

        #endregion

        #region VoucherTraceView

        public List<VoucherTraceViewDTO> GetVoucherTraceViews(int voucherHeadId, int baseSysCurrencyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.VoucherTraceView.NoTracking();
            return GetVoucherTraceViews(entities, voucherHeadId, baseSysCurrencyId);
        }

        public List<VoucherTraceViewDTO> GetVoucherTraceViews(CompEntities entities, int voucherHeadId, int baseSysCurrencyId)
        {
            List<VoucherTraceViewDTO> dtos = new List<VoucherTraceViewDTO>();

            var items = (from v in entities.VoucherTraceView
                         where v.VoucherHeadId == voucherHeadId
                         select v).ToList();

            if (!items.IsNullOrEmpty())
            {
                int langId = GetLangId();
                var originTypes = base.GetTermGroupDict(TermGroup.OriginType, langId);
                var originStatuses = base.GetTermGroupDict(TermGroup.OriginStatus, langId);
                var paymentStatuses = base.GetTermGroupDict(TermGroup.PaymentStatus, langId);
                var inventoryStatuses = base.GetTermGroupDict(TermGroup.InventoryStatus, langId);

                foreach (var item in items)
                {
                    var dto = item.ToDTO();

                    dto.InventoryTypeName = GetText(2304, "Inventarie");
                    dto.Foreign = item.SysCurrencyId != baseSysCurrencyId && !item.IsInventory;
                    dto.CurrencyCode = CountryCurrencyManager.GetCurrencyCode(dto.SysCurrencyId);
                    dto.OriginTypeName = dto.IsInventory ? GetText(2304, "Inventarie") : (dto.OriginType != 0 ? originTypes[(int)dto.OriginType] : "");
                    dto.OriginStatusName = dto.IsInventory ? (dto.InventoryStatusId != 0 ? inventoryStatuses[(int)dto.InventoryStatusId] : "") : (dto.OriginStatus != 0 ? originStatuses[(int)dto.OriginStatus] : "");
                    dto.PaymentStatusName = dto.PaymentStatus != 0 ? paymentStatuses[(int)dto.PaymentStatus] : "";
                    dto.InventoryStatusName = dto.InventoryStatusId != 0 ? inventoryStatuses[(int)dto.InventoryStatusId] : "";
                    //dto.?? = dto.PaymentStatus != 0 ? paymentStatuses[(int)dto.PaymentStatus] : ""; Doesn't seem to be used in the dto

                    dtos.Add(dto);
                }
            }

            return dtos;
        }

        #endregion

        #region Validation

        public bool ValidateAccountingRows(List<AccountingRowDTO> accountingRows)
        {
            decimal totalDebitAmount = 0;
            decimal totalCreditAmount = 0;

            foreach (AccountingRowDTO accountRow in accountingRows)
            {
                if (accountRow.IsDebitRow)
                    totalDebitAmount += accountRow.Amount;
                else
                    totalCreditAmount -= accountRow.Amount;
            }

            return (totalDebitAmount - totalCreditAmount != 0);
        }

        public bool ValidateVoucherRows(List<VoucherRow> VoucherRow)
        {
            decimal totalDebitAmount = 0;
            decimal totalCreditAmount = 0;

            foreach (VoucherRow voucherRow in VoucherRow)
            {
                if (voucherRow.Amount > 0)
                    totalDebitAmount += voucherRow.Amount;
                else
                    totalCreditAmount -= voucherRow.Amount;
            }

            return (totalDebitAmount - totalCreditAmount != 0);
        }

        #endregion

        #region Automatic account distribution

        public List<VoucherRow> ApplyAutomaticAccountDistribution(CompEntities entities, List<VoucherRow> defaultRows, List<AccountStd> accountStds, List<AccountDim> dims, List<AccountInternal> accountInternals, int actorCompanyId, bool? useInImport, bool? useInVoucher, bool mergeOnAccount = false)
        {
            var voucherRows = new List<VoucherRow>();
            var rowsToRemove = new List<VoucherRow>();

            var adm = AccountDistributionManager;
            List<AccountDistributionHeadSmallDTO> accountDistributions = adm.GetAccountDistributionHeadsUsedIn(entities, actorCompanyId, SoeAccountDistributionType.Auto, useInImport: useInImport, useInVoucher: useInVoucher).ToSmallDTOs(true).ToList();

            if (accountDistributions.Count > 0)
            {
                var distributed = false;
                foreach (var voucherRow in defaultRows)
                {
                    var entries = new List<AccountDistributionHeadSmallDTO>();
                    entries.AddRange(accountDistributions);

                    var entry = GetMatchingAccountDistributionEntry(entries, dims, voucherRow);
                    if (entry != null)
                    {
                        var distributedRows = ApplyAutomaticAccountDistributionOnRow(entities, entry, voucherRow, accountStds, accountInternals);
                        if (distributedRows.Count > 0)
                        {
                            if (!entry.KeepRow)
                                entities.Detach(voucherRow);
                            else
                                voucherRows.Add(voucherRow);

                            voucherRows.AddRange(distributedRows);
                        }
                        else
                            voucherRows.Add(voucherRow);

                        distributed = true;
                    }
                    else
                    {
                        voucherRows.Add(voucherRow);
                    }
                }

                int rowNbr = 1;
                foreach (var voucherRow in voucherRows)
                {
                    voucherRow.RowNr = rowNbr;
                    rowNbr++;
                }

                if (mergeOnAccount && distributed)
                {
                    var mergedRows = new List<VoucherRow>();

                    int counter = 1;
                    foreach (var voucherRow in voucherRows)
                    {
                        if (voucherRow.IsDetached())
                            continue;

                        voucherRow.RowNr = counter;

                        var matchingRows = GetVoucherRowsWithMatchingAccounts(voucherRows.Where(r => !r.IsDetached()).ToList(), voucherRow);
                        if (matchingRows != null)
                        {
                            foreach (var matchingRow in matchingRows)
                            {
                                voucherRow.Amount += matchingRow.Amount;
                                voucherRow.AmountEntCurrency += matchingRow.AmountEntCurrency;
                                voucherRow.Quantity += matchingRow.Quantity;
                                voucherRow.Merged = true;

                                if (voucherRow.Text.Length < 500)
                                    voucherRow.Text += ", " + matchingRow.Text;

                                entities.Detach(matchingRow);
                            }
                        }

                        mergedRows.Add(voucherRow);
                        counter++;
                    }

                    voucherRows = mergedRows;
                }

                return voucherRows;
            }
            else
            {
                return defaultRows;
            }
        }

        public void ApplyAutomaticAccountDistributionOnPayrollVoucherHead(CompEntities entities, int actorCompanyId, PayrollVoucherHeadDTO payrollVoucherHead, List<AccountDTO> accountStds, List<AccountDimDTO> dims, List<AccountInternalDTO> accountInternals, bool isPayrollVacation = false)
        {
            var voucherRows = new List<PayrollVoucherRowDTO>();
            var adm = AccountDistributionManager;
            var useInPayrollVacation = isPayrollVacation ? true : (bool?)null;
            var useInPayroll = !isPayrollVacation ? true : (bool?)null;
            var accountDTOs = base.GetAccountInternalsFromCache(entities, CacheConfig.Company(actorCompanyId));
            List<AccountDistributionHeadSmallDTO> accountDistributions = adm.GetAccountDistributionHeadsUsedIn(actorCompanyId, useInPayrollVoucher: useInPayroll, useInPayrollVacationVoucher: useInPayrollVacation).ToSmallDTOs(true).ToList();

            if (accountDistributions.Count > 0)
            {
                foreach (var voucherRow in payrollVoucherHead.Rows)
                {
                    if (!voucherRow.AccountInternals.IsNullOrEmpty())
                        voucherRow.AccountInternals.ToList().ForEach(f => f.Account = accountDTOs.FirstOrDefault(ff => ff.AccountId == f.AccountId));

                    var entries = new List<AccountDistributionHeadSmallDTO>();
                    entries.AddRange(accountDistributions);

                    var matchingEntries = GetMatchingAccountDistributionEntries(entries, dims, voucherRow);
                    if (matchingEntries != null)
                    {
                        foreach (var entry in matchingEntries)
                        {
                            var distributedRows = ApplyAutomaticAccountDistributionOnRow(entities, entry, voucherRow, accountStds, accountInternals);
                            if (distributedRows.Count > 0)
                            {
                                if (entry.KeepRow && !voucherRows.Contains(voucherRow))
                                    voucherRows.Add(voucherRow);
                                else
                                    voucherRow.State = SoeEntityState.Deleted;

                                foreach (var item in distributedRows)
                                {
                                    voucherRows.Add(item);
                                }
                            }
                            else if (!voucherRows.Contains(voucherRow))
                                voucherRows.Add(voucherRow);
                        }
                    }
                    else if (!voucherRows.Contains(voucherRow))
                    {
                        voucherRows.Add(voucherRow);
                    }
                }

                int rowNbr = 1;
                foreach (var voucherRow in voucherRows.Where(w => w.State == SoeEntityState.Active))
                {
                    voucherRow.RowNr = rowNbr;
                    rowNbr++;
                }

                payrollVoucherHead.Rows = voucherRows;
            }
        }

        private List<PayrollVoucherRowDTO> ApplyAutomaticAccountDistributionOnRow(CompEntities entities, AccountDistributionHeadSmallDTO distribution, PayrollVoucherRowDTO voucherRow, List<AccountDTO> accountStds, List<AccountInternalDTO> accountInternals)
        {

            if (voucherRow.Amount == 0 && voucherRow.Quantity == 0)
                return voucherRow.ObjToList();

            var voucherRows = new List<PayrollVoucherRowDTO>();

            var distributionRows = AccountDistributionManager.GetDistributionRows(distribution.AccountDistributionHeadId);

            decimal sourceAmount = voucherRow.Amount;
            bool isDebitAmount = sourceAmount > 0;

            foreach (var row in distributionRows)
            {
                var rowItem = new PayrollVoucherRowDTO();
                rowItem.Text = voucherRow.Text;

                #region AccountStd

                var account = accountStds.FirstOrDefault(a => a.AccountId == row.AccountId);
                rowItem.AccountDistributionHeadId = row.AccountDistributionHeadId;
                rowItem.AccountDistributionName = row.AccountDistributionHead.Name;
                rowItem.Dim1Id = account?.AccountId ?? 0;
                rowItem.Dim1Name = account?.Name ?? "";
                rowItem.Dim1Nr = account?.AccountNr ?? "";
                rowItem.Quantity = voucherRow.Quantity;
                rowItem.AccountDistributionName = distribution.Name;

                #endregion

                #region AccountInternals

                foreach (var distributionAccount in row.AccountDistributionRowAccount)
                {

                    if (distributionAccount.KeepSourceRowAccount)
                    {
                        var accountInternal = voucherRow.AccountInternals.FirstOrDefault(a => a.Account.AccountDimNr == distributionAccount.DimNr);
                        if (accountInternal != null)
                            rowItem.AccountInternals.Add(accountInternal);
                    }
                    else
                    {
                        var accountInternal = accountInternals.FirstOrDefault(a => a.AccountId == distributionAccount.AccountId);
                        if (accountInternal != null)
                            rowItem.AccountInternals.Add(accountInternal);
                    }
                }

                #endregion

                rowItem.AccountDistributionHeadId = row.AccountDistributionHeadId;

                // Set amount
                switch (distribution.CalculationType)
                {
                    case TermGroup_AccountDistributionCalculationType.Percent:
                        rowItem.Amount = (row.SameBalance != 0 ? sourceAmount * row.SameBalance : (sourceAmount * -1) * row.OppositeBalance) / 100;
                        break;
                    case TermGroup_AccountDistributionCalculationType.Amount:
                        rowItem.Amount = row.SameBalance != 0 ? row.SameBalance : row.OppositeBalance;
                        break;
                    case TermGroup_AccountDistributionCalculationType.TotalAmount:
                        // TODO: Implement
                        break;
                }

                CountryCurrencyManager.SetCurrencyAmounts(entities, base.ActorCompanyId, rowItem);

                voucherRows.Add(rowItem);

            }

            return voucherRows;
        }

        private List<VoucherRow> ApplyAutomaticAccountDistributionOnRow(CompEntities entities, AccountDistributionHeadSmallDTO distribution, VoucherRow voucherRow, List<AccountStd> accountStds, List<AccountInternal> accountInternals)
        {
            var voucherRows = new List<VoucherRow>();

            var distributionRows = AccountDistributionManager.GetDistributionRows(entities, distribution.AccountDistributionHeadId);

            decimal sourceAmount = voucherRow.Amount;
            bool isDebitAmount = sourceAmount > 0;

            foreach (var row in distributionRows)
            {
                var rowItem = new VoucherRow();
                rowItem.Text = voucherRow.Text;

                #region AccountStd

                rowItem.AccountStd = accountStds.FirstOrDefault(a => a.AccountId == row.AccountId);

                #endregion

                #region AccountInternals

                foreach (var distributionAccount in row.AccountDistributionRowAccount)
                {
                    if (distributionAccount.KeepSourceRowAccount)
                    {
                        var accountInternal = voucherRow.AccountInternal.FirstOrDefault(a => a.Account.AccountDim.AccountDimNr == distributionAccount.DimNr);
                        if (accountInternal != null)
                            rowItem.AccountInternal.Add(accountInternal);
                    }
                    else
                    {
                        var accountInternal = accountInternals.FirstOrDefault(a => a.AccountId == distributionAccount.AccountId);
                        if (accountInternal != null)
                            rowItem.AccountInternal.Add(accountInternal);
                    }
                }

                #endregion

                rowItem.AccountDistributionHeadId = row.AccountDistributionHeadId;

                // Set amount
                switch (distribution.CalculationType)
                {
                    case TermGroup_AccountDistributionCalculationType.Percent:
                        rowItem.Amount = decimal.Round(((row.SameBalance != 0 ? sourceAmount * row.SameBalance : (sourceAmount * -1) * row.OppositeBalance) / 100), 2);
                        break;
                    case TermGroup_AccountDistributionCalculationType.Amount:
                        rowItem.Amount = row.SameBalance != 0 ? row.SameBalance : row.OppositeBalance;
                        break;
                    case TermGroup_AccountDistributionCalculationType.TotalAmount:
                        // TODO: Implement
                        break;
                }

                CountryCurrencyManager.SetCurrencyAmounts(entities, base.ActorCompanyId, rowItem);

                if (rowItem.AccountStd == null)
                {
                    throw new SoeGeneralException(string.Format(GetText(7734, "Automatkontering {0} misslyckades: Konto saknas"), distribution.Name + " (Rad=" + row.RowNbr + ")"), this.ToString());
                }

                voucherRows.Add(rowItem);
            }

            return voucherRows;
        }
        private AccountDistributionHeadSmallDTO GetMatchingAccountDistributionEntry(List<AccountDistributionHeadSmallDTO> accountDistributions, List<AccountDim> dims, VoucherRow row)
        {
            return GetMatchingAccountDistributionEntry(accountDistributions, dims.ToDTOs().ToList(), row.Date, row.AccountId, row.AccountNr, row.Amount, row.AccountInternal.ToDTOs());
        }
        private AccountDistributionHeadSmallDTO GetMatchingAccountDistributionEntry(List<AccountDistributionHeadSmallDTO> accountDistributions, List<AccountDimDTO> dims, DateTime? date, int accountId, string accountNr, decimal amount, List<AccountInternalDTO> accountInternals)
        {
            var entries = GetMatchingAccountDistributionEntries(accountDistributions, dims, date, accountId, accountNr, amount, accountInternals);
            return entries != null ? entries.FirstOrDefault() : null;
        }
        private List<AccountDistributionHeadSmallDTO> GetMatchingAccountDistributionEntries(List<AccountDistributionHeadSmallDTO> accountDistributions, List<AccountDimDTO> dims, PayrollVoucherRowDTO dto)
        {
            return GetMatchingAccountDistributionEntries(accountDistributions, dims, dto.Date, dto.Dim1Id, dto.Dim1Nr, dto.Amount, dto.AccountInternals);
        }
        private List<AccountDistributionHeadSmallDTO> GetMatchingAccountDistributionEntries(List<AccountDistributionHeadSmallDTO> accountDistributions, List<AccountDimDTO> dims, DateTime? date, int accountId, string accountNr, decimal amount, List<AccountInternalDTO> accountInternals)
        {
            // Sort
            dims = dims.OrderBy(d => d.AccountDimNr).ToList();

            // Match on date
            if (date.HasValue)
            {
                accountDistributions = accountDistributions.Where(a => (a.StartDate == null || a.StartDate.Value <= date.Value.Date) &&
                                             (a.EndDate == null || a.EndDate.Value >= date.Value.Date)).ToList();
                if (accountDistributions.Count == 0)
                    return null;
            }

            var tempDistributions = accountDistributions;
            // Match on accounts
            foreach (var match in accountDistributions.ToList())
            {
                // Dim 1
                if (!MatchAccount(match.Dim1Expression, accountNr))
                {
                    tempDistributions.Remove(match);
                }
                // Dim 2
                if (match.Dim2Expression != null && dims.Count > 1)
                {
                    var dim2 = dims[1];
                    var accountInternal = accountInternals.FirstOrDefault(a => a.AccountDimId == dim2.AccountDimId);
                    if (!MatchAccount(match.Dim2Expression, accountInternal != null ? accountInternal.AccountNr : String.Empty))
                    {
                        tempDistributions.Remove(match);
                    }
                }
                // Dim 3
                if (match.Dim3Expression != null && dims.Count > 2)
                {
                    var dim3 = dims[2];
                    var accountInternal = accountInternals.FirstOrDefault(a => a.AccountDimId == dim3.AccountDimId);
                    if (!MatchAccount(match.Dim3Expression, accountInternal != null ? accountInternal.AccountNr : String.Empty))
                    {
                        tempDistributions.Remove(match);
                    }
                }
                // Dim 4
                if (match.Dim4Expression != null && dims.Count > 3)
                {
                    var dim4 = dims[3];
                    var accountInternal = accountInternals.FirstOrDefault(a => a.AccountDimId == dim4.AccountDimId);
                    if (!MatchAccount(match.Dim4Expression, accountInternal != null ? accountInternal.AccountNr : String.Empty))
                    {
                        tempDistributions.Remove(match);
                    }
                }
                // Dim 5
                if (match.Dim5Expression != null && dims.Count > 4)
                {
                    var dim5 = dims[4];
                    var accountInternal = accountInternals.FirstOrDefault(a => a.AccountDimId == dim5.AccountDimId);
                    if (!MatchAccount(match.Dim5Expression, accountInternal != null ? accountInternal.AccountNr : String.Empty))
                    {
                        tempDistributions.Remove(match);
                    }
                }
                // Dim 6
                if (match.Dim6Expression != null && dims.Count > 5)
                {
                    var dim6 = dims[5];
                    var accountInternal = accountInternals.FirstOrDefault(a => a.AccountDimId == dim6.AccountDimId);
                    if (!MatchAccount(match.Dim6Expression, accountInternal != null ? accountInternal.AccountNr : String.Empty))
                    {
                        tempDistributions.Remove(match);
                    }
                }
            }

            if (tempDistributions.Count == 0)
                return null;

            // Loop through remaining matches where an amount condition is specified
            foreach (var match in accountDistributions.Where(a => a.Amount != 0).ToList())
            {
                switch ((WildCard)match.AmountOperator)
                {
                    case WildCard.LessThan:
                        if (amount >= match.Amount)
                            tempDistributions.Remove(match);
                        break;
                    case WildCard.LessThanOrEquals:
                        if (amount > match.Amount)
                            tempDistributions.Remove(match);
                        break;
                    case WildCard.Equals:
                        if (amount != match.Amount)
                            tempDistributions.Remove(match);
                        break;
                    case WildCard.GreaterThan:
                        if (amount <= match.Amount)
                            tempDistributions.Remove(match);
                        break;
                    case WildCard.GreaterThanOrEquals:
                        if (amount < match.Amount)
                            tempDistributions.Remove(match);
                        break;
                    case WildCard.NotEquals:
                        if (amount == match.Amount)
                            tempDistributions.Remove(match);
                        break;
                }
            }

            if (tempDistributions.Count == 0)
                return null;

            return tempDistributions.Where(match => match.Type == (int)SoeAccountDistributionType.Auto).ToList();
        }

        private bool MatchAccount(string expression, string accountNr)
        {
            // If expression is empty, entered value must also be empty
            if (String.IsNullOrEmpty(expression) && !String.IsNullOrEmpty(accountNr))
                return false;

            if (string.IsNullOrEmpty(accountNr))
                return false;

            Regex regEx = new Regex(StringUtility.WildCardToRegEx(expression));
            return regEx.IsMatch(accountNr);
        }
        #endregion
    }
}
