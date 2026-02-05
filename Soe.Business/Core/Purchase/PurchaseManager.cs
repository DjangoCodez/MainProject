using SoftOne.Soe.Data;
using System;
using System.Dynamic;
using System.Collections.Generic;
using System.Linq;
using SoftOne.Soe.Common.DTO;
using System.Data.Entity;
using SoftOne.Soe.Common.Util;
using System.Transactions;
using SoftOne.Soe.Business.Util.Config;
using System.Text;
using System.Linq.Expressions;

namespace SoftOne.Soe.Business.Core
{
    public class PurchaseManager : ManagerBase
    {
        #region Variables

        // Create a logger for use in this class
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        static private readonly string[] purchaseSaveTreat0asNullHead = { "deliverytypeid", "deliveryconditionid" };
        static private readonly string[] purchaseSaveExcludeFields = { "categoryids", "purchasenr", "projectnr", "created", "modified", "createdby", "modifiedby" };
        static private readonly int[] purchaseStatus = { (int)SoeOriginStatus.Origin, (int)SoeOriginStatus.PurchaseDone, (int)SoeOriginStatus.PurchaseSent, (int)SoeOriginStatus.PurchaseAccepted, (int)SoeOriginStatus.PurchasePartlyDelivered, (int)SoeOriginStatus.PurchaseDeliveryCompleted };


        private int? _langId;
        private int LangId
        {
            get
            {
                return _langId ?? (_langId = base.GetLangId()) ?? 0;
            }
        }
        private Dictionary<int, string> _originStatuses;
        private Dictionary<int, string> OriginStatuses
        {
            get
            {
                return _originStatuses ?? (_originStatuses = base.GetTermGroupDict(TermGroup.OriginStatus, LangId));
            }
        }
        public List<SmallGenericType> GetPurchaseStatus()
        {
            List<SmallGenericType> lst = new List<SmallGenericType>();
            try
            {
                purchaseStatus.ToList().ForEach(i =>
                {
                    lst.Add(new SmallGenericType(i, (i != 0 && OriginStatuses.ContainsKey(i)) ? OriginStatuses[i] : OriginStatuses[(int)SoeOriginStatus.Origin]));
                });
            }
            catch (Exception ex)
            {
                log.Error(ex.Message, ex);
            }
            return lst;
        }

        static private readonly string[] purchaseTreat0asNullRow = { "stockid", "vatcodeid", "purchaseunitid", "productid", "orderid" };
        static private List<int> purchaseStatesForDelivery = new List<int> { (int)SoeOriginStatus.PurchaseDone, (int)SoeOriginStatus.PurchaseAccepted, (int)SoeOriginStatus.PurchaseSent, (int)SoeOriginStatus.PurchasePartlyDelivered };

        #endregion

        #region Ctor

        public PurchaseManager(ParameterObject parameterObject) : base(parameterObject) { }

        #endregion

        #region Purchases
        public List<PurchaseGridDTO> GetPurchaseForGrid(int allItemsSelection, int[] status, int actorCompanyId,int? purchaseId= null)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Project.NoTracking();
            return GetPurchaseForGrid(entities, allItemsSelection, status.Select(f=> (SoeOriginStatus)f).ToList(), actorCompanyId, purchaseId);
        }

        public List<PurchaseGridDTO> GetPurchaseForGrid(CompEntities entities, int? allItemsSelection, List<SoeOriginStatus> filterPurchaseStatus, int actorCompanyId, int? purchaseId = null)
        {
            IQueryable<Purchase> query = (
                from
                    p in entities.Purchase
                where
                    p.Origin.ActorCompanyId == actorCompanyId &&
                    p.State == (int)SoeEntityState.Active
                select p);


            if (purchaseId.HasValue)
            {
                query = query.Where(x => x.PurchaseId == purchaseId);
            }
            else
            {
                DateTime? selectionDate = null;
                if (allItemsSelection.HasValue)
                {
                    selectionDate = InvoiceManager.GetAllItemsSelectionDate((TermGroup_ChangeStatusGridAllItemsSelection)allItemsSelection, selectionDate);
                }

                if (selectionDate.HasValue)
                {
                    query = query.Where(p => p.PurchaseDate >= selectionDate.Value);
                }

                if (filterPurchaseStatus.Any())
                {
                    var statusInt = filterPurchaseStatus.Select(x => (int)x).ToList();
                    query = query.Where(i => statusInt.Contains(i.Origin.Status));
                }
            }

            var gridDtos = query.Select(PurchaseExtensions.PurchaseGridDTO).ToList();

            foreach (var dto in gridDtos)
            {
                if (dto.OriginStatus == SoeOriginStatus.PurchaseDeliveryCompleted)
                {
                    dto.DeliveryStatus = PurchaseDeliveryStatus.Delivered;
                }
                else if (dto.OriginStatus == SoeOriginStatus.PurchasePartlyDelivered)
                {
                    dto.DeliveryStatus = PurchaseDeliveryStatus.PartlyDelivered;
                }
                else if (dto.ConfirmedDate.HasValue && dto.ConfirmedDate < DateTime.Now.Date)
                {
                    dto.DeliveryStatus = PurchaseDeliveryStatus.Late;
                }
                else if (dto.OriginStatus == SoeOriginStatus.PurchaseAccepted)
                {
                    dto.DeliveryStatus = PurchaseDeliveryStatus.Accepted;
                }
            }

            return SetPurchaseTexts(gridDtos).OrderByDescending(p => Convert.ToInt32(p.PurchaseNr)).ToList();
        }

        public Dictionary<int, string> GetPurchaseForSelectDict(int actorCompanyId, bool forDelivery)
        {
            Dictionary<int, string> dict = GetPurchaseForSelect(actorCompanyId, forDelivery).ToDictionary(k => k.PurchaseId, v => v.Name);
            return dict;
        }

        public List<PurchaseSmallDTO> GetPurchaseForSelect(int actorCompanyId, bool forDelivery)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Project.NoTracking();
            return GetOpenPurchasesForSelect(entities, actorCompanyId, forDelivery);
        }

        public List<PurchaseSmallDTO> GetOpenPurchasesForSelect(CompEntities entities, int actorCompanyId, bool forDelivery)
        {
            IQueryable<Purchase> query = from p in entities.Purchase
                                         where
                                             p.Origin.ActorCompanyId == actorCompanyId &&
                                             p.State == (int)SoeEntityState.Active
                                         select p;

            if (forDelivery)
                query = query.Where(p => purchaseStatesForDelivery.Contains(p.Origin.Status));
            else
                query = query.Where(p => p.Origin.Status < (int)SoeOriginStatus.PurchasePartlyDelivered);

            var purchases = query
                .Select(PurchaseExtensions.GetPurchaseSmallDTO)
                .ToList();

            purchases.ForEach(p =>
            {
                var statusText = OriginStatuses[p.Status];
                p.SetDisplayNameWithSupplier(statusText);
            });

            return purchases;
        }
        public List<PurchaseSmallDTO> GetPurchasesForSelectBySupplier(int actorCompanyId, int supplierId, SoeOriginStatus minStatus)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Purchase.NoTracking();
            return GetPurchasesForSelectBySupplier(entities, actorCompanyId, supplierId, minStatus);
        }
        public List<PurchaseSmallDTO> GetPurchasesForSelectBySupplier(CompEntities entities, int actorCompanyId, int supplierId, SoeOriginStatus? minStatus)
        {
            var query = entities.Purchase.Where(p => p.Origin.ActorCompanyId == actorCompanyId && p.SupplierId == supplierId && p.State == 0);
            if (minStatus != null) query = query.Where(p => p.Origin.Status >= (int)minStatus);

            var purchases = query
                .Select(PurchaseExtensions.GetPurchaseSmallDTO)
                .ToList();

            purchases.ForEach(p =>
            {
                var statusText = OriginStatuses[p.Status];
                p.SetDisplayNameWithDescription(statusText);
            });

            return purchases;
        }
        #endregion

        #region PurchaseRows
        public List<PurchaseRowSmallDTO> GetPurchaseRowsByPurchase(int actorCompanyId, int purchaseId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.PurchaseRow.NoTracking();
            return GetPurchaseRowsByPurchase(entities, actorCompanyId, purchaseId);
        }
        public List<PurchaseRowSmallDTO> GetPurchaseRowsByPurchase(CompEntities entities, int actorCompanyId, int purchaseId)
        {

            var purchaseRows = entities.PurchaseRow
                .Where(p => p.PurchaseId == purchaseId && p.Purchase.Origin.ActorCompanyId == actorCompanyId && p.Type == (int)PurchaseRowType.PurchaseRow)
                .Select(PurchaseExtensions.PurchaseRowSmallDTO)
                .OrderBy(r => r.PurchaseRowNr)
                .ToList();

            purchaseRows.ForEach(r =>
            {
                r.Text = !string.IsNullOrEmpty(r.Text) && r.Text.Length > 20 ? $"{r.Text.Substring(0, 20)}..." : $"{r.Text}";
            });

            return purchaseRows;
        }

        public List<PurchaseStatisticsDTO> GetPurchaseStatistics(DateTime fromDate, DateTime toDate)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Purchase.NoTracking();
            return GetPurchaseStatistics(entities, fromDate, toDate);
        }

        public List<PurchaseStatisticsDTO> GetPurchaseStatistics(CompEntities entities, DateTime fromDate, DateTime toDate)
        {
            var actorCompanyId = base.ActorCompanyId;

            if (FeatureManager.HasRolePermission(Feature.Billing_Statistics_Purchase, Permission.Readonly, base.RoleId, actorCompanyId))
            {
                return GetPurchaseStatisticsRows(entities, actorCompanyId, fromDate, toDate);
            }

            return new List<PurchaseStatisticsDTO>();
        }

        public List<PurchaseRowDTO> GetPurchaseRowsFromSupplier(CompEntities entities, int supplierId, int actorCompanyId)
        {
            var rows = (
                from
                    e in entities.PurchaseRow
                join o in entities.Origin
                    on new { p1 = e.PurchaseId } equals new { p1 = o.OriginId }
                where
                    e.Purchase.SupplierId == supplierId &&
                    o.Status != (int)SoeOriginStatus.PurchaseDeliveryCompleted &&
                    e.Purchase.Origin.ActorCompanyId == actorCompanyId &&
                    e.State == (int)SoeEntityState.Active
                select e)
                .Select(PurchaseExtensions.GetPurchaseRowDTO)
                .ToList();

            return SetPurchaseTexts(rows);
        }
        public List<PurchaseStatisticsDTO> GetPurchaseStatisticsRows(CompEntities entities, int actorCompanyId, DateTime fromDate, DateTime toDate)
        {
            var rows = (
                    from
                        e in entities.PurchaseRow
                    where
                        e.Purchase.Origin.ActorCompanyId == actorCompanyId &&
                        e.Purchase.State == (int)SoeEntityState.Active &&
                        e.State == (int)SoeEntityState.Active &&
                        e.Purchase.PurchaseDate >= fromDate && e.Purchase.PurchaseDate <= toDate
                    select e)
                    .Select(PurchaseExtensions.GetPurchaseStatisticsRowDTO)
                    .ToList();

            return SetPurchaseRowTexts(rows);
        }

        public List<PurchaseRowOrderMappingDTO> GetPurchaseRowsForOrder(int invoiceId, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.CustomerInvoiceRow.NoTracking();
            return GetPurchaseRowsForOrder(entities, invoiceId, actorCompanyId);
        }
        public List<PurchaseRowOrderMappingDTO> GetPurchaseRowsForOrder(CompEntities entities, int invoiceId, int actorCompanyId)
        {
            var rows = (
                from
                    e in entities.CustomerInvoiceRow
                where
                    e.State == (int)SoeEntityState.Active &&
                    e.InvoiceId == invoiceId &&
                    e.PurchaseRow.Any()
                select new PurchaseRowOrderMappingDTO
                {
                    CustomerInvoiceRowId = e.CustomerInvoiceRowId,
                    PurchaseRowId = e.PurchaseRow.FirstOrDefault().PurchaseRowId,
                    Status = e.PurchaseRow.FirstOrDefault().Status ?? 0,
                    PurchaseNr = e.PurchaseRow.FirstOrDefault().Purchase.PurchaseNr,
                    PurchaseId = e.PurchaseRow.FirstOrDefault().Purchase.PurchaseId
                })
                .ToList();

            return SetPurchaseTexts(rows);
        }

        #endregion

        public List<PurchaseDeliveryInvoiceDTO> GetPurchaseDeliveryInvoiceFromPurchase(int actorCompanyId, List<int> purchaseIds, bool getOnlyDelivered, bool getAlreadyConnected)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.PurchaseRow.NoTracking();
            return GetPurchaseDeliveryInvoiceFromPurchase(entities, actorCompanyId, purchaseIds, getOnlyDelivered, getAlreadyConnected);
        }
        public List<PurchaseDeliveryInvoiceDTO> GetPurchaseDeliveryInvoiceFromPurchase(CompEntities entities, int actorCompanyId, List<int> purchaseIds, bool getOnlyDelivered, bool getAlreadyConnected)
        {
            IQueryable<PurchaseRow> query = entities.PurchaseRow
                .Where(p => purchaseIds.Contains(p.PurchaseId) &&
                        p.Purchase.Origin.ActorCompanyId == actorCompanyId &&
                        p.State == (int)SoeEntityState.Active &&
                        p.Type == (int)PurchaseRowType.PurchaseRow);

            if (getOnlyDelivered)
            {
                List<int> statusToFetch = new List<int>() { (int)SoeOriginStatus.PurchasePartlyDelivered, (int)SoeOriginStatus.PurchaseDeliveryCompleted };
                query = query.Where(p => statusToFetch.Contains(p.Status ?? 0));
            }

            if (!getAlreadyConnected)
            {
                query = query.Where(p => !p.PurchaseDeliveryInvoice.Any());
            }

            var purchaseRows = query.Select(PurchaseExtensions.PurchaseDeliveryInvoiceDTO).OrderBy(r => r.PurchaseNr).ThenBy(r => r.PurchaseRowNr).ToList();
            return purchaseRows;
        }

        public Purchase GetPurchase(int purchaseId, bool loadRows, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Purchase.NoTracking();
            return GetPurchase(entities, purchaseId, loadRows, actorCompanyId);
        }

        public Purchase GetPurchase(CompEntities entities, int purchaseId, bool loadRows, int actorCompanyId)
        {
            IQueryable<Purchase> query = (
                from i in entities.Purchase
                    .Include("Origin.OriginUser.User")
                    .Include("Project")
                    .Include("CustomerInvoice")
                    .Include("Supplier")
                where
                    i.PurchaseId == purchaseId &&
                    i.State == (int)SoeEntityState.Active
                select i
            );

            if (loadRows)
            {
                query = query.Include("PurchaseRow");
            }

            var purchase = query.FirstOrDefault();

            if (purchase == null || purchase.Origin.ActorCompanyId != actorCompanyId)
                return null;

            purchase.StatusName = GetText(purchase.Origin.Status, (int)TermGroup.OriginStatus);

            return purchase;
        }

        public PurchaseSmallDTO GetPurchaseSmallDTO(int purchaseId, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Purchase.NoTracking();
            return GetPurchaseSmallDTO(entities, purchaseId, actorCompanyId);

        }
        public PurchaseSmallDTO GetPurchaseSmallDTO(CompEntities entities, int purchaseId, int actorCompanyId)
        {
            IQueryable<Purchase> query = from p in entities.Purchase
                                         where
                                             p.Origin.ActorCompanyId == actorCompanyId &&
                                             p.State == (int)SoeEntityState.Active &&
                                             p.PurchaseId == purchaseId
                                         select p;

            return query
                .Select(PurchaseExtensions.GetPurchaseSmallDTO)
                .FirstOrDefault();
        }

        public List<PurchaseRowDTO> GetPurchaseRows(int purchaseId, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.PurchaseRow.NoTracking();
            return GetPurchaseRows(entities, purchaseId, actorCompanyId);
        }

        public List<PurchaseRowDTO> GetPurchaseRows(CompEntities entities, int purchaseId, int actorCompanyId)
        {
            var rows = entities.PurchaseRow.Include("SupplierProduct.Product")
                .Where(e => e.Purchase.PurchaseId == purchaseId &&
                    e.Purchase.Origin.ActorCompanyId == actorCompanyId &&
                    e.State == (int)SoeEntityState.Active)
                .Select(PurchaseExtensions.GetPurchaseRowDTO)
                .ToList();

            return SetPurchaseTexts(rows);
        }

        #region Purchase

        public ActionResult SavePurchaseStatus(int purchaseId, SoeOriginStatus status, DateTime? deliveryDate = null)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return SavePurchaseStatus(entities, purchaseId, status, deliveryDate);
        }
        public ActionResult SavePurchaseStatus(CompEntities entities, int purchaseId, SoeOriginStatus status, DateTime? deliveryDate = null)
        {
            var result = new ActionResult();

            try
            {
                Purchase purchase = purchaseId != 0 ? GetPurchase(entities, purchaseId, true, base.ActorCompanyId) : null;
                if (purchase != null && (purchase.Origin.Status != (int)status) )
                {

                    if (status == SoeOriginStatus.PurchaseDeliveryCompleted)
                    {
                        purchase.DeliveryDate = deliveryDate ?? DateTime.Today;
                    }

                    SetModifiedProperties(purchase);
                    SetModifiedProperties(purchase.Origin);
                    purchase.Origin.Status = (int)status;
                    entities.SaveChanges();
                    result.IntegerValue = purchase.PurchaseId;
                }
            }
            catch (Exception ex)
            {
                base.LogError(ex, this.log);
                result.Exception = ex;
                result.IntegerValue = 0;
            }

            return result;
        }
        public ActionResult SavePurchase(dynamic modifiedFields, List<OriginUserDTO> originUsers, List<PurchaseRowDTO> newRows, List<ExpandoObject> modifiedRows)
        {
            var result = new ActionResult();

            var values = (IDictionary<string, object>)modifiedFields;
            int purchaseId = values.ContainsKey("purchaseid") ? Convert.ToInt32(values["purchaseid"]) : 0;
            string purchaseNr = "";

            var modifiedPurchaseRows = new Dictionary<int, PurchaseRow>();

            using (var entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    int voucherSeriesTypeId = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.CustomerInvoiceVoucherSeriesType, 0, base.ActorCompanyId, 0);

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        Purchase purchase = purchaseId != 0 ? GetPurchase(entities, purchaseId, true, base.ActorCompanyId) : null;
                        //Origin origin = purchaseId != 0 ? entities.Origin.FirstOrDefault(o => o.OriginId == purchaseId && o.ActorCompanyId == ActorCompanyId) : null;

                        if (purchase == null)
                        {
                            #region Origin

                            // Create new Origin, fields will be set in update below
                            var origin = new Origin
                            {
                                VoucherSeriesId = voucherSeriesTypeId,
                                VoucherSeriesTypeId = voucherSeriesTypeId,

                                Type = Convert.ToInt32(SoeOriginType.Purchase),
                                Status = Convert.ToInt32(values["originstatus"]),

                                //Set FK
                                ActorCompanyId = base.ActorCompanyId,
                            };

                            SetCreatedProperties(origin);
                            entities.Origin.AddObject(origin);

                            #endregion

                            #region OriginUser

                            // Add current user to origin
                            var originUser = new OriginUser
                            {
                                Main = true,

                                //Set FK
                                UserId = base.UserId,

                                //Set references
                                Origin = origin,
                            };
                            origin.OriginUser.Add(originUser);
                            SetCreatedProperties(originUser);

                            #endregion

                            #region Purchase 
                            var copyDeliveryAddress = false;
                            bool.TryParse(values["copyDeliveryAddress"].ToString(), out copyDeliveryAddress);

                            if (copyDeliveryAddress)
                            {
                                purchase = new Purchase
                                {
                                    Origin = origin,
                                    DeliveryAddressId = Convert.ToInt32(values["deliveryAddressId"]),
                                    DeliveryAddress = values["deliveryAddress"].ToString()
                                };
                            }
                            else
                            {
                                purchase = new Purchase
                                {
                                    Origin = origin,
                                };
                            }

                            entities.Purchase.AddObject(purchase);
                            SetCreatedProperties(purchase);

                            // Get next sequence number
                            var startNbr = 1;
                            var entityName = Enum.GetName(typeof(SoeOriginType), origin.Type);
                            // Get sequence number
                            purchaseNr = SequenceNumberManager.GetNextSequenceNumber(entities, base.ActorCompanyId, entityName, startNbr, false).ToString();

                            // Set invoice number to same as sequence number (padded to a specified number of digits)
                            purchase.PurchaseNr = purchaseNr;

                            #endregion
                        }
                        else
                        {
                            purchaseNr = purchase.PurchaseNr;
                            SetModifiedProperties(purchase);
                        }

                        //handle origin fields
                        if (values.ContainsKey("originstatus"))
                        {
                            var value = Convert.ToInt32(values["originstatus"]);
                            purchase.Origin.Status = value;
                        }

                        if (values.ContainsKey("origindescription"))
                        {
                            purchase.Origin.Description = (string)values["origindescription"];
                            values.Remove("origindescription");
                        }

                        var props = purchase.GetType().GetProperties();
                        foreach (var prop in props)
                        {
                            string lcName = prop.Name.ToLower();

                                if (values.ContainsKey(lcName) && !purchaseSaveExcludeFields.Contains(lcName))
                                {
                                    var value = values[lcName];

                                    if (prop.PropertyType == typeof(decimal) || prop.PropertyType == typeof(decimal?))
                                        prop.SetValue(purchase, Convert.ToDecimal(value));
                                    else if (prop.PropertyType == typeof(int) || prop.PropertyType == typeof(int?))
                                    {
                                        if (purchaseSaveTreat0asNullHead.Contains(lcName) && value != null && Convert.ToInt32(value) == 0)
                                        {
                                            value = null;
                                        }
                                        prop.SetValue(purchase, value == null ? (int?)null : Convert.ToInt32(value));
                                    }
                                    else if ((prop.PropertyType == typeof(DateTime) || prop.PropertyType == typeof(DateTime?)) && value != null)
                                    {
                                        DateTime dt = (DateTime)value;
                                        if (dt.Year < 1900)
                                            prop.SetValue(purchase, new DateTime(1900, dt.Month, dt.Day));
                                        else
                                            prop.SetValue(purchase, value);
                                    }
                                    else
                                        prop.SetValue(purchase, value);
                                }
                            }

                        #region Rows

                        #region Modified rows

                        if (modifiedRows.Any())
                        {
                            foreach (var purchaseRow in purchase.PurchaseRow.Where(r => r.State == (int)SoeEntityState.Active))
                            {
                                // Try get CustomerInvoiceRow from input
                                var purchaseRowInput = (from r in modifiedRows
                                                        where (long)((IDictionary<string, object>)r)["purchaserowid"] == purchaseRow.PurchaseRowId
                                                        select r).FirstOrDefault() as IDictionary<string, object>;

                                if (purchaseRowInput == null)
                                {
                                    continue;
                                }

                                if (Convert.ToInt32(purchaseRowInput["state"]) == (int)SoeEntityState.Deleted)
                                {
                                    // Delete existing CustomerInvoiceRow
                                    ChangeEntityState(purchaseRow, SoeEntityState.Deleted);
                                }
                                else
                                {
                                    var rowProps = purchaseRow.GetType().GetProperties();
                                    foreach (var prop in rowProps)
                                    {
                                        string lcName = prop.Name.ToLower();
                                        if (purchaseRowInput.ContainsKey(lcName))
                                        {
                                            if (prop.PropertyType == typeof(decimal) || prop.PropertyType == typeof(decimal?))
                                                prop.SetValue(purchaseRow, Convert.ToDecimal(purchaseRowInput[lcName]));
                                            else if (prop.PropertyType == typeof(int) || prop.PropertyType == typeof(int?))
                                            {
                                                if (purchaseTreat0asNullRow.Contains(lcName) && (purchaseRowInput[lcName] == null || Convert.ToInt32(purchaseRowInput[lcName]) == 0))
                                                {
                                                    purchaseRowInput[lcName] = null;
                                                    prop.SetValue(purchaseRow, purchaseRowInput[lcName]);
                                                }
                                                else
                                                    prop.SetValue(purchaseRow, Convert.ToInt32(purchaseRowInput[lcName]));
                                            }
                                            else
                                                prop.SetValue(purchaseRow, purchaseRowInput[lcName]);
                                        }
                                    }

                                    SetModifiedProperties(purchaseRow);
                                }
                            }
                        }

                        #endregion

                        #region New rows

                        foreach (var rowDTO in newRows)
                        {
                            PurchaseRow purchaseRow = null;
                            switch (rowDTO.Type)
                            {
                                case PurchaseRowType.PurchaseRow:
                                    purchaseRow = new PurchaseRow
                                    {
                                        TempRowId = rowDTO.TempRowId,
                                        ProductId = rowDTO.ProductId.GetValueOrDefault() > 0 ? rowDTO.ProductId : null,
                                        PurchaseUnitId = rowDTO.PurchaseUnitId,
                                        PurchasePrice = rowDTO.PurchasePrice,
                                        PurchasePriceCurrency = rowDTO.PurchasePriceCurrency,
                                        Quantity = rowDTO.Quantity,
                                        RowNr = rowDTO.RowNr,
                                        StockId = rowDTO.StockId.GetValueOrDefault() > 0 ? rowDTO.StockId : null,
                                        VatRate = rowDTO.VatRate,
                                        VatCodeId = rowDTO.VatCodeId,
                                        VatAmount = rowDTO.VatAmount,
                                        VatAmountCurrency = rowDTO.VatAmountCurrency,
                                        SumAmount = rowDTO.SumAmount,
                                        SumAmountCurrency = rowDTO.SumAmountCurrency,
                                        Text = rowDTO.Text,
                                        DiscountType = rowDTO.DiscountType,
                                        DiscountPercent = rowDTO.DiscountPercent,
                                        DiscountAmount = rowDTO.DiscountAmount,
                                        DiscountAmountCurrency = rowDTO.DiscountPercent,
                                        WantedDeliveryDate = rowDTO.WantedDeliveryDate,
                                        AccDeliveryDate = rowDTO.AccDeliveryDate,
                                        OrderId = rowDTO.OrderId,
                                        Status = 2,
                                        Type = (int)rowDTO.Type,
                                        SupplierProductId = rowDTO.SupplierProductId.GetValueOrDefault() > 0 ? rowDTO.SupplierProductId.Value : (int?)null
                                    };
                                    break;
                                case PurchaseRowType.TextRow:
                                    purchaseRow = new PurchaseRow
                                    {
                                        TempRowId = rowDTO.TempRowId,
                                        PurchasePrice = 0,
                                        PurchasePriceCurrency = 0,
                                        Quantity = 0,
                                        RowNr = rowDTO.RowNr,
                                        VatRate = 0,
                                        VatAmount = 0,
                                        VatAmountCurrency = 0,
                                        SumAmount = 0,
                                        SumAmountCurrency = 0,
                                        Text = rowDTO.Text,
                                        DiscountType = rowDTO.DiscountType,
                                        DiscountPercent = 0,
                                        DiscountAmount = 0,
                                        DiscountAmountCurrency = 0,
                                        OrderId = rowDTO.OrderId,
                                        Status = 2,
                                        Type = (int)rowDTO.Type,
                                    };
                                    break;
                                default:
                                    return new ActionResult("Purchase row has unknown row typ");
                            }
                            
                            SetCreatedProperties(purchaseRow);

                            //Add mapping to
                            if (!rowDTO.CustomerInvoiceRowIds.IsNullOrEmpty())
                            {
                                foreach (var customerInvoiceRowId in rowDTO.CustomerInvoiceRowIds)
                                {
                                    var customerInvoiceRow = entities.CustomerInvoiceRow.Where(r => r.CustomerInvoiceRowId == customerInvoiceRowId).FirstOrDefault();
                                    if (customerInvoiceRow != null)
                                    {
                                        purchaseRow.CustomerInvoiceRow.Add(customerInvoiceRow);
                                    }
                                }
                            }

                            // Set parent row
                            if (rowDTO.ParentRowId.HasValue)
                            {
                                purchaseRow.ParentRow = purchase.PurchaseRow.FirstOrDefault(r => r.PurchaseRowId == rowDTO.ParentRowId.Value);
                                if (purchaseRow.ParentRow == null)
                                {
                                    purchaseRow.ParentRow = purchase.PurchaseRow.FirstOrDefault(r => r.TempRowId == rowDTO.ParentRowId.Value);
                                }
                            }

                            purchase.PurchaseRow.Add(purchaseRow);

                            // Add to modified to get id
                            modifiedPurchaseRows.Add(rowDTO.TempRowId, purchaseRow);
                        }
                        #endregion

                        #endregion


                        if (result.Success)
                        {
                            this.CalculateAmounts(purchase);
                            //Calculate currency amounts
                            CountryCurrencyManager.CalculateCurrencyAmounts(entities, base.ActorCompanyId, purchase);
                            result = SaveChanges(entities, transaction);
                        }

                        if (result.Success)
                        {
                            //Set id
                            purchaseId = purchase.PurchaseId;
                            var originUserResult = OriginManager.SaveOriginUsers(entities, purchaseId, originUsers);

                            if (originUserResult.Success)
                            {
                                result = SaveChanges(entities, transaction);
                            }

                            if (result.Success)
                            {
                                //Commit transaction
                                transaction.Complete();

                                var modifiedRowIds = new Dictionary<int, int>();
                                foreach (int tempRowId in modifiedPurchaseRows.Keys)
                                {
                                    modifiedRowIds.Add(tempRowId, modifiedPurchaseRows[tempRowId].PurchaseRowId);
                                }

                                result.IntegerValue = purchaseId;
                                result.StringValue = purchaseNr;

                                result.IntDict = modifiedRowIds;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    result.Exception = ex;
                    result.IntegerValue = 0;
                }
            }

            return result;
        }

        private decimal GetVatRate(int actorCompanyId, out int sysCountryId)
        {
            var company = CompanyManager.GetCompany(actorCompanyId);

            sysCountryId = company.SysCountryId ?? 0;
            switch (sysCountryId)
            {
                case (int)TermGroup_Country.FI:
                    return 0.24M;
                case (int)TermGroup_Country.SE:
                default:
                    return 0.25M;
            }
        }

        public Dictionary<int, SmallGenericType> CreatePurchaseFromStock(int actorCompanyId, List<PurchaseRowFromStockDTO> rows)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            var results = new Dictionary<int, SmallGenericType>();

            var groups = rows.GroupBy(r => r.SupplierId);

            Dictionary<int, VatCode> vatCodes = null;

            decimal defaultVatRate = GetVatRate(actorCompanyId, out int sysCountryId);

            foreach (var rowsBySupplier in groups)
            {
                int supplierId = rowsBySupplier.FirstOrDefault().SupplierId;
                Supplier supplier = SupplierManager.GetSupplier(entities, supplierId);

                foreach (var row in rows)
                {
                    if (supplier.SysCountryId != sysCountryId)
                    {
                        row.VatAmount = 0;
                        row.VatRate = 0;
                    }
                    else if (row.VatCodeId > 0)
                    {
                        if (vatCodes == null)
                        {
                            vatCodes = AccountManager.GetVatCodes(entities, actorCompanyId).ToDictionary(r => r.VatCodeId);
                        }

                        if (vatCodes.TryGetValue(row.VatCodeId, out VatCode vatCode))
                        {
                            row.VatRate = vatCode.Percent;
                            row.VatAmount = (row.VatRate / 100) * row.Sum;
                        }
                    }
                    else
                    {
                        row.VatRate = defaultVatRate * 100;
                        row.VatAmount = defaultVatRate * row.Sum;
                    }
                }

                dynamic purchaseHead = new ExpandoObject();
                PurchaseExpandoHelpers.SetValuesFromSupplier(purchaseHead, supplier);
                purchaseHead.currencyrate = CountryCurrencyManager.GetCurrencyRate(
                    this.ActorCompanyId,
                    CountryCurrencyManager.GetSysCurrencyId(entities, supplier.CurrencyId),
                    purchaseHead.currencydate
                    );

                purchaseHead.copyDeliveryAddress = false;
                purchaseHead.deliveryAddressId = 0;
                purchaseHead.referenceour = rowsBySupplier.FirstOrDefault().ReferenceOur;


                CreateForExclusiveRows(ref results, purchaseHead, rowsBySupplier.Where(r => r.ExclusivePurchase).ToList());
                CreateForGroupedRows(ref results, purchaseHead, rowsBySupplier.Where(r => !r.ExclusivePurchase).ToList());
            }

            return results;
        }

        public void CreateForGroupedRows(ref Dictionary<int, SmallGenericType> results, dynamic template, List<PurchaseRowFromStockDTO> rows)
        {
            foreach (var group in rows.GroupBy(r => r.DeliveryAddress))
            {
                template.deliveryaddress = group.Key;
                CreatePurchase(ref results, template, group.ToList());
            }
        }

        public void CreateForExclusiveRows(ref Dictionary<int, SmallGenericType> results, dynamic template, List<PurchaseRowFromStockDTO> rows)
        {
            foreach (var row in rows)
            {
                CreatePurchase(ref results, template, new List<PurchaseRowFromStockDTO>() { row });
            }
        }

        public void CreatePurchase(ref Dictionary<int, SmallGenericType> results, dynamic purchaseHead, List<PurchaseRowFromStockDTO> rows)
        {
            dynamic clone = PurchaseExpandoHelpers.DeepCopy(purchaseHead);

            var purchaseRows = new List<PurchaseRowDTO>();
            var rowNr = 0;
            foreach (var row in rows)
            {
                rowNr++;
                var purchaseRow = ConvertToPurchaseRowDTO(row);
                purchaseRow.RowNr = rowNr;
                purchaseRows.Add(purchaseRow);
            }

            var actionResult = SavePurchase(clone, new List<OriginUserDTO>(), purchaseRows, new List<ExpandoObject>());

            foreach (var row in rows)
            {
                results.Add(row.TempId, new SmallGenericType(actionResult.IntegerValue, actionResult.StringValue));
            }
        }

        private PurchaseRowDTO ConvertToPurchaseRowDTO(PurchaseRowFromStockDTO dto)
        {
            decimal discountPercentage = ((100 - dto.DiscountPercentage) / 100);
            return new PurchaseRowDTO()
            {
                TempRowId = dto.TempId,
                ProductId = dto.ProductId,
                Text = dto.ProductName,
                PurchaseUnitId = dto.SupplierProductId != 0 ? dto.SupplierUnitId : dto.UnitId,
                PurchasePrice = dto.Price,
                PurchasePriceCurrency = dto.Price,
                Quantity = dto.Quantity,
                StockId = dto.StockId,
                VatRate = dto.VatRate,
                VatAmountCurrency = dto.VatAmount,
                SumAmountCurrency = dto.Sum,
                DiscountType = (int)SoeInvoiceRowDiscountType.Percent,
                DiscountPercent = dto.DiscountPercentage,
                DiscountAmount = discountPercentage * dto.Sum,
                DiscountAmountCurrency = discountPercentage,
                WantedDeliveryDate = dto.RequestedDeliveryDate,
                Status = 2,
                Type = PurchaseRowType.PurchaseRow,
                SupplierProductId = dto.SupplierProductId,
            };
        }

        public ActionResult UpdatePurchaseFromOrder(PurchaseFromOrderDTO dto)
        {
            var result = new ActionResult();

            using (var entities = new CompEntities())
            {
                if (dto.OrderId == 0)
                {
                    return new ActionResult(5605, GetText(5605, "Ordern hittades inte"));
                }

                void setPurchasePrice(PurchaseRowDTO row, int supplierId, DateTime currencyDate, int currencyId)
                {
                    if (row.PurchasePriceCurrency == 0)
                    {
                        SupplierProduct product = SupplierProductManager.GetSupplierProductByInvoiceProduct(entities, row.ProductId.Value, supplierId, ActorCompanyId);
                        if (product != null)
                        {
                            SupplierProductPriceDTO price = SupplierProductManager.GetSupplierProductPrice(entities, product.SupplierProductId, currencyDate, row.Quantity, currencyId);
                            if (price != null)
                                row.PurchasePriceCurrency = price.Price;
                        }
                    }
                    row.SumAmountCurrency = row.PurchasePriceCurrency * row.Quantity;
                }

                if (dto.CreateNewPurchase)
                {
                    if (dto.SupplierId == 0)
                    {
                        return new ActionResult(1275, GetText(1275, "Leverantör hittades inte"));
                    }

                    dynamic purchaseHead = new ExpandoObject();

                    var supplier = SupplierManager.GetSupplier(entities, dto.SupplierId);
                    purchaseHead.supplierid = dto.SupplierId;
                    purchaseHead.originstatus = (int)SoeOriginStatus.Origin;
                    purchaseHead.orderid = dto.OrderId;
                    purchaseHead.contactecomid = supplier.ContactEcomId;
                    purchaseHead.deliveryconditionid = supplier.DeliveryConditionId;
                    purchaseHead.deliverytypeid = supplier.DeliveryTypeId;
                    purchaseHead.paymentconditionid = supplier.PaymentConditionId;
                    purchaseHead.vattype = supplier.VatType == 0 ? (int)TermGroup_InvoiceVatType.Merchandise : supplier.VatType;

                    purchaseHead.currencyid = supplier.CurrencyId;
                    purchaseHead.currencydate = DateTime.Now.Date;
                    purchaseHead.currencyrate = CountryCurrencyManager.GetCurrencyRate(this.ActorCompanyId, CountryCurrencyManager.GetSysCurrencyId(entities, supplier.CurrencyId), purchaseHead.currencydate);
                    purchaseHead.purchasedate = DateTime.Now.Date;


                    var order = InvoiceManager.GetInvoice(entities, dto.OrderId);

                    CustomerInvoice customerInvoice = order.InvoiceId != 0 ? InvoiceManager.GetCustomerInvoice(order.InvoiceId) : null;

                    purchaseHead.copyDeliveryAddress = dto.CopyDeliveryAddress;
                    if (dto.CopyDeliveryAddress)
                    {
                        purchaseHead.deliveryAddressId = 0;
                        purchaseHead.deliveryAddress = string.Empty;
                        if (customerInvoice.DeliveryAddressId == 0)
                        {
                            purchaseHead.deliveryAddress = customerInvoice.InvoiceHeadText;
                        }
                        else
                        {
                            string address = string.Empty;
                            var contactAddress = ContactManager.GetContactAddress(entities, customerInvoice.DeliveryAddressId, false, true);

                            if (contactAddress != null)
                            {
                                address = ParseDeliveryAddress(contactAddress);
                            }
                            purchaseHead.deliveryAddress = address;
                        }
                    }

                    if (dto.CopyProject || dto.CopyInternalAccounts)
                    {
                        if (order != null && dto.CopyInternalAccounts)
                        {
                            purchaseHead.defaultdim1accountid = order.DefaultDim1AccountId;
                            purchaseHead.defaultdim2accountid = order.DefaultDim2AccountId;
                            purchaseHead.defaultdim3accountid = order.DefaultDim3AccountId;
                            purchaseHead.defaultdim4accountid = order.DefaultDim4AccountId;
                            purchaseHead.defaultdim5accountid = order.DefaultDim5AccountId;
                            purchaseHead.defaultdim6accountid = order.DefaultDim6AccountId;
                        }
                        if (order != null && dto.CopyProject)
                        {
                            purchaseHead.projectid = order.ProjectId;
                        }
                    }
                    int n = 1;
                    foreach (var row in dto.PurchaseRows)
                    {
                        if (row.ProductId == null) continue;

                        if (supplier.VatType != (int)TermGroup_InvoiceVatType.Merchandise)
                            row.VatRate = decimal.Zero;
                        setPurchasePrice(row, supplier.ActorSupplierId, purchaseHead.currencydate, purchaseHead.currencyid);

                        row.RowNr = n;
                        row.TempRowId = n;
                        n++;
                    }
                    CountryCurrencyManager.CalculateCurrencyAmountsPurchaseRowsFromOrder(entities, dto.PurchaseRows, this.ActorCompanyId, supplier.CurrencyId, purchaseHead.currencyrate, true);
                    result = SavePurchase(purchaseHead, new List<OriginUserDTO>(), dto.PurchaseRows, new List<ExpandoObject>());
                }
                else
                {
                    var rows = new Dictionary<int, List<PurchaseRowDTO>>();
                    dto.PurchaseRows.ForEach(r =>
                    {
                        if (r.PurchaseId > 0)
                        {
                            if (rows.ContainsKey(r.PurchaseId))
                            {
                                rows[r.PurchaseId].Add(r);
                            }
                            else
                            {
                                rows[r.PurchaseId] = new List<PurchaseRowDTO>() { r };
                            }
                        }
                    });
                    foreach (var purchaseId in rows.GetKeys())
                    {
                        dynamic purchaseHead = new ExpandoObject();
                        purchaseHead.purchaseid = purchaseId;
                        var purchase = entities.Purchase.FirstOrDefault(p => p.PurchaseId == purchaseId);
                        int n = GetLastPurchaseRowNr(entities, purchaseId) + 1;
                        foreach (var row in rows[purchaseId])
                        {
                            if (purchase.VatType != (int)TermGroup_InvoiceVatType.Merchandise)
                                row.VatRate = decimal.Zero;

                            setPurchasePrice(row, purchase.SupplierId ?? 0, purchase.CurrencyDate, purchase.CurrencyId);

                            row.TempRowId = n;
                            row.RowNr = n;
                            n++;
                        }
                        CountryCurrencyManager.CalculateCurrencyAmountsPurchaseRowsFromOrder(entities, rows[purchaseId], this.ActorCompanyId, purchase.CurrencyId, purchase.CurrencyRate, true);
                        result = SavePurchase(purchaseHead, new List<OriginUserDTO>(), rows[purchaseId], new List<ExpandoObject>());
                        if (!result.Success)
                            return result;
                    }
                }
            }
            return result;
        }
        private int GetLastPurchaseRowNr(CompEntities entities, int purchaseId)
        {
            return entities.PurchaseRow.Where(r => r.PurchaseId == purchaseId && r.State == (int)SoeEntityState.Active).Max(r => (int?)r.RowNr) ?? 0;
        }

        public ActionResult DeletePurchase(int purchaseId, int actorCompanyId)
        {
            ActionResult result;
            using (var entities = new CompEntities())
            {
                var purchase = GetPurchase(entities, purchaseId, false, actorCompanyId);
                if (purchase != null)
                {
                    purchase.State = (int)SoeEntityState.Deleted;
                    SetModifiedProperties(purchase);
                    result = SaveChanges(entities);
                }
                else
                {
                    result = new ActionResult(GetText(7542, "Beställningen hittades inte"));
                }
            }

            return result;
        }
        public List<string> GetContactAddresesForPurchase(int customerOrderId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Invoice.NoTracking();
            return GetContactAddresesForPurchase(entities, customerOrderId);
        }
        public List<string> GetContactAddresesForPurchase(CompEntities entities, int customerOrderId)
        {
            var addresses = new List<string>();
            var customerOrder = InvoiceManager.GetCustomerInvoice(entities, customerOrderId);

            if (customerOrder == null)
                return addresses;
            
            if (!string.IsNullOrEmpty(customerOrder.InvoiceHeadText))
            {
                addresses.Add(customerOrder.InvoiceHeadText);
            }

            if (customerOrder.ActorId > 0)
            {
                int contactId = ContactManager.GetContactIdFromActorId(entities, customerOrder.ActorId.GetValueOrDefault());
                var customerAddresses = ContactManager.GetContactAddresses(entities, contactId, TermGroup_SysContactAddressType.Delivery, false, true);

                foreach (var address in customerAddresses)
                {
                    addresses.Add(ParseDeliveryAddress(address));
                }
            }

            return addresses;
        }

        #endregion

        #region Email
        public ActionResult SendPurchasesAsEmail(int actorCompanyId, List<int> purchaseIds, int? emailTemplateId, int? langId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Project.NoTracking();
            return SendPurchasesAsEmail(entities, actorCompanyId, purchaseIds, emailTemplateId, langId);
        }
        public ActionResult SendPurchaseAsEmail(int actorCompanyId, int purchaseId, int reportId, int emailTemplateId, int? langId, List<int> recipients, string singleRecipient)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Project.NoTracking();
            return SendPurchaseAsEmail(entities, actorCompanyId, purchaseId, emailTemplateId, reportId, langId, recipients, singleRecipient);
        }
        private ActionResult SendPurchaseAsEmail(CompEntities entities, int actorCompanyId, int purchaseId, int emailTemplateId, int reportId, int? langId, List<int> recipients, string singleRecipient)
        {
            var result = new ActionResult();

            int langIdCulture = GetLangId();

            #region EmailTemplate
            EmailTemplate emailTemplate = emailTemplateId > 0 ?
                EmailManager.GetEmailTemplate(entities, emailTemplateId, actorCompanyId) :
                EmailManager.GetEmailTemplates(entities, actorCompanyId).FirstOrDefault(t => t.Type == (int)EmailTemplateType.PurchaseOrder);

            if (emailTemplate == null)
            {
                return new ActionResult(false);
            }
            #endregion

            #region Report
            reportId = reportId == 0 ? SettingManager.GetCompanyIntSetting(CompanySettingType.BillingDefaultPurchaseOrderReportTemplate) : reportId;
            var report = ReportManager.GetReport(reportId, actorCompanyId).ToDTO();
            if (report == null)
            {
                return new ActionResult(false);
            }
            #endregion

            var purchase = entities.Purchase
                .Where(p => p.Origin.ActorCompanyId == actorCompanyId)
                .Where(p => p.PurchaseId == purchaseId)
                .Select(PurchaseExtensions.GetPurchaseReportDTOSmall)
                .FirstOrDefault();

            if (purchase == null)
            {
                return new ActionResult(GetText(7542, "Beställningen hittades inte"));
            }

            var invoiceDistributionId = InvoiceDistributionManager.EnsureDistributionItem(purchase.PurchaseId, TermGroup_EDistributionType.Email, TermGroup_EDistributionStatusType.PendingInPlatform, null, true);

            ReportPrintoutDTO dto = SendPurchaseEmail( actorCompanyId, invoiceDistributionId, purchase, recipients, langId, emailTemplate, report, singleRecipient);
            if (dto.Status == (int)TermGroup_ReportPrintoutStatus.Error || dto.Status == (int)TermGroup_ReportPrintoutStatus.SentFailed)
            {
                result.ErrorMessage = GetText(7408, "Skicka epost misslyckades") + ": " + dto.EmailMessage;
                result.Success = false;
                InvoiceDistributionManager.UpdatePurchaseEmailStatus(this, purchase.PurchaseId, (int)TermGroup_ReportPrintoutStatus.Error, result.ErrorMessage, invoiceDistributionId);
            }
            else
            {
                var origin = entities.Origin.FirstOrDefault(o => o.OriginId == purchase.PurchaseId);
                var status = (SoeOriginStatus)origin.Status;
                if (status == SoeOriginStatus.PurchaseDone || status == SoeOriginStatus.Origin)
                {
                    origin.Status = (int)SoeOriginStatus.PurchaseSent;
                    SetModifiedProperties(origin);
                }
                entities.SaveChanges();
            }

            return result;
        }
        private ActionResult SendPurchasesAsEmail(CompEntities entities, int actorCompanyId, List<int> purchaseIds, int? emailTemplateId, int? langId)
        {
            var result = new ActionResult();

            #region EmailTemplate
            EmailTemplate emailTemplate = emailTemplateId.GetValueOrDefault() > 0 ?
                EmailManager.GetEmailTemplate(entities, emailTemplateId.Value, actorCompanyId) :
                EmailManager.GetEmailTemplates(entities, actorCompanyId).FirstOrDefault(t => t.Type == (int)EmailTemplateType.PurchaseOrder);

            if (emailTemplate == null)
            {
                return new ActionResult(false);
            }
            #endregion

            #region Report
            int reportId = SettingManager.GetCompanyIntSetting(CompanySettingType.BillingDefaultPurchaseOrderReportTemplate);
            var report = ReportManager.GetReport(reportId, actorCompanyId).ToDTO();
            if (report == null)
            {
                return new ActionResult(false);
            }
            #endregion

            var purchases = entities.Purchase
                .Where(p => p.Origin.ActorCompanyId == actorCompanyId)
                .Where(p => purchaseIds.Contains(p.PurchaseId))
                .Select(PurchaseExtensions.GetPurchaseReportDTOSmall)
                .ToList();

            var origins = entities.Origin
                .Where(o => o.ActorCompanyId == actorCompanyId)
                .Where(o => purchaseIds.Contains(o.OriginId))
                .ToList();

            foreach (var purchase in purchases)
            {
                if (purchase.ContactEComId.GetValueOrDefault() == 0 && string.IsNullOrEmpty(purchase.SupplierEmail))
                {
                    //return error?
                    continue;
                }

                var invoiceDistributionId = InvoiceDistributionManager.EnsureDistributionItem(purchase.PurchaseId, TermGroup_EDistributionType.Email, TermGroup_EDistributionStatusType.PendingInPlatform, null, true);

                var recipients = purchase.ContactEComId.HasValue ? new List<int>() { purchase.ContactEComId.Value } : new List<int>();
                ReportPrintoutDTO dto = SendPurchaseEmail( actorCompanyId, invoiceDistributionId, purchase, recipients, langId, emailTemplate, report, string.IsNullOrEmpty(purchase.SupplierEmail) ? " ": purchase.SupplierEmail);

                if (dto.Status == (int)TermGroup_ReportPrintoutStatus.Error || dto.Status == (int)TermGroup_ReportPrintoutStatus.SentFailed)
                {
                    result.ErrorMessage = GetText(7408, "Skicka epost misslyckades") + ": " + dto.EmailMessage;
                    result.Success = false;
                    InvoiceDistributionManager.UpdatePurchaseEmailStatus(this, purchase.PurchaseId, (int)TermGroup_ReportPrintoutStatus.Error, result.ErrorMessage, invoiceDistributionId);
                }
                else
                {
                    var origin = origins.FirstOrDefault(o => o.OriginId == purchase.PurchaseId);
                    var status = (SoeOriginStatus)purchase.OriginStatus;
                    if (origin != null && (status == SoeOriginStatus.PurchaseDone || status == SoeOriginStatus.Origin) )
                    {
                        origin.Status = (int)SoeOriginStatus.PurchaseSent;
                        SetModifiedProperties(origin);
                    }
                }
            }
            entities.SaveChanges();

            return result;
        }
        private ReportPrintoutDTO SendPurchaseEmail( int actorCompanyId,int invoiceDistributionId, PurchaseReportDTO purchase, List<int> recipients, int? langId, EmailTemplate emailTemplate, ReportDTO report, string singleRecipient = "")
        {

            langId = langId.HasValue ? langId.Value : this.GetLangId();

            PurchaseOrderReportDTO reportItem = new PurchaseOrderReportDTO(actorCompanyId, new List<int>() { purchase.PurchaseId }, report.ReportId, langId.Value, (int)SoeReportTemplateType.PurchaseOrder, (int)TermGroup_ReportExportType.Pdf, purchase.PurchaseNr, recipients, emailTemplate.EmailTemplateId, singleRecipient?.Trim());

            Selection selection = new Selection(actorCompanyId, this.UserId, this.RoleId, this.LoginName, report, exportType: (int)TermGroup_ReportExportType.Pdf);
            selection.Evaluate(reportItem, null);
            selection.Evaluated.EmailTemplate = emailTemplate.ToDTO();
            selection.Evaluated.EmailTemplate.Body = ReplaceVarsInText(selection.Evaluated.EmailTemplate.Body, purchase);
            selection.Evaluated.EmailTemplate.Subject = ReplaceVarsInText(selection.Evaluated.EmailTemplate.Subject, purchase);
            selection.Evaluated.InvoiceDistributionId = invoiceDistributionId;

            ReportPrintoutDTO printout = ReportDataManager.PrintReportDTO(selection.Evaluated, true);
            printout.DeliveryType = TermGroup_ReportPrintoutDeliveryType.Email;

            return printout;
        }
        #endregion

        #region Helpers
        private string ReplaceVarsInText(string text, PurchaseReportDTO purchase)
        {
            const string startToken = "[[";
            const string endToken = "]]";
            var sb = new StringBuilder(text);
            sb.Replace(startToken + "PurchaseNr" + endToken, purchase.PurchaseNr);
            sb.Replace(startToken + "OurReference" + endToken, string.IsNullOrEmpty(purchase.ReferenceOur) ? "" : purchase.ReferenceOur);
            sb.Replace(startToken + "YourReference" + endToken, string.IsNullOrEmpty(purchase.ReferenceYour) ? "" : purchase.ReferenceYour);
            sb.Replace(startToken + "Label" + endToken, string.IsNullOrEmpty(purchase.PurchaseLabel) ? "" : purchase.PurchaseLabel);
            sb.Replace(startToken + "SupplierName" + endToken, string.IsNullOrEmpty(purchase.SupplierName) ? "" : purchase.SupplierName);
            sb.Replace(startToken + "SupplierNr" + endToken, string.IsNullOrEmpty(purchase.SupplierNr) ? "" : purchase.SupplierNr);
            return sb.ToString();
        }
        public void CalculateAmounts(Purchase purchase)
        {
            decimal totalAmountCurrency = 0;
            decimal vatAmountCurrency = 0;

            //decimal sumAmount = 0;

            // Check VAT type
            bool noVAT = purchase.VatType == (int)TermGroup_InvoiceVatType.NoVat;

            foreach (var row in purchase.PurchaseRow.Where(r => r.State == (int)SoeEntityState.Active))
            {
                //Fix row 
                // Add row sum to total amount
                totalAmountCurrency += row.SumAmountCurrency;
                // VAT amount
                var rowVatAmountCurrency = noVAT ? decimal.Zero : row.VatAmountCurrency;
                if (rowVatAmountCurrency > 0)
                {
                    totalAmountCurrency += rowVatAmountCurrency; //CountryCurrencyManager.GetCurrencyAmountFromBaseAmount(rowVatAmountCurrency, purchase.CurrencyRate);
                    vatAmountCurrency += rowVatAmountCurrency;
                }
            }

            //Set new values
            purchase.TotalAmountCurrency = decimal.Round(totalAmountCurrency, 2);
            purchase.TotalAmount = decimal.Round(CountryCurrencyManager.GetBaseAmountFromCurrencyAmount(totalAmountCurrency, purchase.CurrencyRate), 2);

            //purchase.SumAmount = Decimal.Round(sumAmount, 2);
            //purchase.SumAmountCurrency = Decimal.Round(CountryCurrencyManager.GetCurrencyAmountFromBaseAmount(sumAmount, purchase.CurrencyRate), 2);

            purchase.VATAmountCurrency = vatAmountCurrency;
            purchase.VATAmount = decimal.Round(CountryCurrencyManager.GetBaseAmountFromCurrencyAmount(vatAmountCurrency, purchase.CurrencyRate), 2);

        }
        private List<PurchaseGridDTO> SetPurchaseTexts(List<PurchaseGridDTO> dtos)
        {
            if (!dtos.IsNullOrEmpty())
            {
                dtos.ForEach(i =>
                {
                    i.StatusName = i.OriginStatus != 0 && OriginStatuses.ContainsKey((int)i.OriginStatus) ? OriginStatuses[(int)i.OriginStatus] : OriginStatuses[(int)SoeOriginStatus.Origin];
                    i.CurrencyCode = CountryCurrencyManager.GetCurrencyCode(i.SysCurrencyId);
                });
            }
            return dtos;
        }
        private List<T> SetPurchaseTexts<T>(List<T> dtos) where T : IPurchaseStatusName
        {
            if (!dtos.IsNullOrEmpty())
            {
                dtos.ForEach(i =>
                {
                    i.StatusName = i.Status != 0 && OriginStatuses.ContainsKey(i.Status) ? OriginStatuses[i.Status] : "";
                });
            }
            return dtos;
        }
        private List<T> SetPurchaseRowTexts<T>(List<T> dtos) where T : PurchaseStatisticsDTO
        {
           var currencies = CountryCurrencyManager.GetSysCurrenciesDict(false);
            if (!dtos.IsNullOrEmpty())
            {
                dtos.ForEach(i =>
                {
                    i.CurrencyCode = currencies[i.SysCurrencyId];
                    i.StatusName = i.Status != 0 && OriginStatuses.ContainsKey(i.Status) ? OriginStatuses[i.Status] : "";
                    i.RowStatusName = i.RowStatus != 0 && OriginStatuses.ContainsKey(i.RowStatus) ? OriginStatuses[i.RowStatus] : "";
                });
            }
            return dtos;
        }

        private string ParseDeliveryAddress(ContactAddress contactAddress)
        {
            var address = string.Empty;
            ContactManager.ParseContactAddress(contactAddress, TermGroup_SysContactAddressType.Delivery,
                out string addressName,
                out string addressAddress,
                out string addressCo,
                out string addressPostNr,
                out string addressCity,
                out string addressCountry);

            if (!string.IsNullOrEmpty(addressName))
                address += $"{addressName}\n";

            if (!string.IsNullOrEmpty(addressCo))
                address += $"{addressCo}\n";

            if (!string.IsNullOrEmpty(addressAddress) || !string.IsNullOrEmpty(addressPostNr) || !string.IsNullOrEmpty(addressCity))
            {
                if (!string.IsNullOrEmpty(addressAddress))
                    address += $"{addressAddress}, "; 
                if (!string.IsNullOrEmpty(addressPostNr))
                    address += $"{addressPostNr} ";
                if (!string.IsNullOrEmpty(addressCity))
                    address += $"{addressCity}\n";
            }

            if (!string.IsNullOrEmpty(addressCountry))
                address += $"{addressCountry}\n";

            return address;
        }

        #endregion

        #region CustomerInvoiceRow
        public List<CustomerInvoiceRowPurchaseDTO> GetCustomerInvoiceRows(int actorCompanyId, int viewTypeIn, int id)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            entitiesReadOnly.CustomerInvoiceRow.NoTracking();
            var viewType = (PurchaseCustomerInvoiceViewType)viewTypeIn;
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            switch (viewType)
            {
                case PurchaseCustomerInvoiceViewType.FromPurchase:
                    return GetCustomerInvoiceRowsByPurchase(entities, actorCompanyId, id);
                case PurchaseCustomerInvoiceViewType.FromPurchaseDelivery:
                    return GetCustomerInvoiceRowsByDelivery(entities, actorCompanyId, id);
                case PurchaseCustomerInvoiceViewType.FromCustomerInvoice:
                    return GetCustomerInvoiceRowsByCustomerInvoice(entities, actorCompanyId, id);
                case PurchaseCustomerInvoiceViewType.FromCustomerInvoiceRow:
                    return GetCustomerInvoiceRowsByCustomerInvoiceRowId(entities, actorCompanyId, id);
                default:
                    return new List<CustomerInvoiceRowPurchaseDTO>();
            }
        }
        public List<CustomerInvoiceRowPurchaseDTO> GetCustomerInvoiceRowsByCustomerInvoice(CompEntities entities, int actorCompanyId, int customerInvoiceId)
        {
            Expression<Func<CustomerInvoiceRow, bool>> whereClause = r => r.InvoiceId == customerInvoiceId;
            return GetCustomerInvoiceRows(entities, actorCompanyId, whereClause);
        }
        public List<CustomerInvoiceRowPurchaseDTO> GetCustomerInvoiceRowsByCustomerInvoiceRowId(CompEntities entities, int actorCompanyId, int customerInvoiceRowId)
        {
            Expression<Func<CustomerInvoiceRow, bool>> whereClause = r => r.CustomerInvoiceRowId == customerInvoiceRowId;
            return GetCustomerInvoiceRows(entities, actorCompanyId, whereClause);
        }
        public List<CustomerInvoiceRowPurchaseDTO> GetCustomerInvoiceRowsByPurchase(CompEntities entities, int actorCompanyId, int purchaseId)
        {
            Expression<Func<CustomerInvoiceRow, bool>> whereClause = r => r.PurchaseRow.Any(p => p.PurchaseId == purchaseId && p.State == 0);
            return GetCustomerInvoiceRows(entities, actorCompanyId, whereClause);
        }

        public List<CustomerInvoiceRowPurchaseDTO> GetCustomerInvoiceRowsByDelivery(CompEntities entities, int actorCompanyId, int purchaseDeliveryId)
        {
            Expression<Func<CustomerInvoiceRow, bool>> whereClause = r => r.PurchaseRow.Any(p => p.PurchaseDeliveryRow.Any(d => d.State == 0 && d.PurchaseDeliveryId == purchaseDeliveryId) && p.State == 0);
            return GetCustomerInvoiceRows(entities, actorCompanyId, whereClause);
        }
        private List<CustomerInvoiceRowPurchaseDTO> GetCustomerInvoiceRows(CompEntities entities, int actorCompanyId, Expression<Func<CustomerInvoiceRow, bool>> whereClause)
        {
            if (whereClause == null)
            {
                return new List<CustomerInvoiceRowPurchaseDTO>();
            }

            var rows = entities.CustomerInvoiceRow
                .Where(r => r.CustomerInvoice.Origin.ActorCompanyId == actorCompanyId && r.State == 0 && r.CustomerInvoice.State == 0)
                .Where(whereClause)
                .Select(PurchaseExtensions.GetCustomerInvoiceRowPurchaseDTO)
                .ToList();

            foreach (var row in rows)
            {
                decimal deliveredPurchaseQuantity = 0;
                foreach (var purchaseRow in row.PurchaseRows)
                {
                    purchaseRow.PurchaseStatusName = OriginStatuses[purchaseRow.PurchaseStatus];
                    purchaseRow.RowStatusName = OriginStatuses[purchaseRow.RowStatus];
                    deliveredPurchaseQuantity += purchaseRow.DeliveredQuantity;
                }
                row.DeliveredPurchaseQuantity = deliveredPurchaseQuantity;
            }

            return rows;
        }

        #endregion

        #region TraceViews

        public List<PurchaseTraceViewDTO> GetPurchaseTraceViews(int purchaseId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.OfferTraceView.NoTracking();
            return GetPurchaseTraceViews(entities, purchaseId);
        }

        public List<PurchaseTraceViewDTO> GetPurchaseTraceViews(CompEntities entities, int purchaseId)
        {
            List<PurchaseTraceViewDTO> dtos = new List<PurchaseTraceViewDTO>();

            if (purchaseId == 0)
                return dtos;

            var items = (from v in entities.PurchaseTraceView
                         where v.PurchaseId == purchaseId
                         select v).ToList();

            if (!items.IsNullOrEmpty())
            {
                var originTypes = base.GetTermGroupDict(TermGroup.OriginType, LangId);

                foreach (var item in items)
                {
                    var dto = item.ToDTO();
                    if (dto.IsDelivery)
                    {
                        dto.OriginTypeName = GetText(7563, "Inleverans");
                        dto.OriginStatusName = GetText(7702, "Levererad");
                    }
                    else
                    {
                        dto.OriginTypeName = originTypes[(int)SoeOriginType.Order];
                        dto.OriginStatusName = dto.OriginStatus != 0 ? OriginStatuses[(int)dto.OriginStatus] : "";
                    }

                    dtos.Add(dto);
                }
            }

            return dtos;
        }

        #endregion
    }

    public static class PurchaseExpandoHelpers
    {
        public static ExpandoObject ShallowCopy(ExpandoObject original)
        {
            var clone = new ExpandoObject();

            var _original = (IDictionary<string, object>)original;
            var _clone = (IDictionary<string, object>)clone;

            foreach (var kvp in _original)
                _clone.Add(kvp);

            return clone;
        }
        public static ExpandoObject DeepCopy(ExpandoObject original)
        {
            var clone = new ExpandoObject();

            var _original = (IDictionary<string, object>)original;
            var _clone = (IDictionary<string, object>)clone;

            foreach (var kvp in _original)
                _clone.Add(kvp.Key, kvp.Value is ExpandoObject ? DeepCopy((ExpandoObject)kvp.Value) : kvp.Value);

            return clone;
        }
        public static void SetValuesFromSupplier(dynamic purchase, Supplier supplier)
        {
            purchase.supplierid = supplier.ActorSupplierId;
            purchase.originstatus = (int)SoeOriginStatus.Origin;
            purchase.contactecomid = supplier.ContactEcomId;
            purchase.deliveryconditionid = supplier.DeliveryConditionId;
            purchase.deliverytypeid = supplier.DeliveryTypeId;
            purchase.paymentconditionid = supplier.PaymentConditionId;
            purchase.vattype = supplier.VatType == 0 ? (int)TermGroup_InvoiceVatType.Merchandise : supplier.VatType;
            purchase.currencyid = supplier.CurrencyId;
            purchase.currencydate = DateTime.Now.Date;
            purchase.purchasedate = DateTime.Now.Date;
        }
    }

    public static class PurchaseExtensions
    {
        public static Expression<Func<Purchase, PurchaseReportDTO>> GetPurchaseReportDTOSmall =
            p => new PurchaseReportDTO
            {
                PurchaseId = p.PurchaseId,
                PurchaseNr = p.PurchaseNr,
                ContactEComId = p.ContactEComId,
                SupplierName = p.Supplier.Name,
                SupplierNr = p.Supplier.SupplierNr,
                ReferenceOur = p.ReferenceOur,
                ReferenceYour = p.ReferenceYour,
                Attention = p.Attention,
                PurchaseLabel = p.PurchaseLabel,
                SupplierEmail = p.SupplierEmail
            };

        public static Expression<Func<CustomerInvoiceRow, CustomerInvoiceRowPurchaseDTO>> GetCustomerInvoiceRowPurchaseDTO =
            r => new CustomerInvoiceRowPurchaseDTO
            {
                CustomerInvoiceRowId = r.CustomerInvoiceRowId,
                ProductId = r.ProductId ?? 0,
                ProductNr = r.Product.Number,
                Text = r.Text,
                Unit = r.ProductUnit.Code,
                InvoiceId = r.InvoiceId,
                InvoiceSeqNr = r.CustomerInvoice.SeqNr,
                InvoiceStatus = r.CustomerInvoice.Origin.Status,
                AttestStateId = r.AttestStateId ?? 0,
                AttestStateColor = r.AttestState.Color,
                Quantity = r.Quantity ?? 0,
                InvoiceQuantity = r.InvoiceQuantity ?? 0,
                InvoicedQuantity = r.PreviouslyInvoicedQuantity ?? 0,
                DeliveryDate = r.CustomerInvoice.DeliveryDate,
                PurchaseRows = r.PurchaseRow
                        .Select(p => new PurchaseRowGridDTO
                        {
                            PurchaseRowId = p.PurchaseRowId,
                            PurchaseId = p.PurchaseId,
                            ProductNr = p.Product.Number,
                            PurchaseStatus = p.Purchase.Origin.Status,
                            Text = p.Text,
                            Unit = p.ProductUnit.Code,
                            PurchaseNr = p.Purchase.PurchaseNr,
                            SupplierName = p.Purchase.Supplier.Name,
                            SupplierNr = p.Purchase.Supplier.SupplierNr,
                            RowStatus = p.Status ?? 0,
                            PurchaseQuantity = p.Quantity,
                            DeliveredQuantity = p.DeliveredQuantity ?? 0,
                            RequestedDate = p.WantedDeliveryDate,
                            ConfirmedDate = p.AccDeliveryDate,
                            DeliveryDate = p.DeliveryDate,
                            CustomerInvoiceRowCount = p.CustomerInvoiceRow.Count,
                        })
                        .ToList()
            };

        public static Expression<Func<Purchase, PurchaseReportDTO>> GetPurchaseReportDTOFull
        {
            get
            {
                return p => new PurchaseReportDTO
                {
                    PurchaseId = p.PurchaseId,
                    PurchaseNr = p.PurchaseNr,
                    SupplierId = p.SupplierId,
                    SupplierName = p.Supplier.Name,
                    SupplierNr = p.Supplier.SupplierNr,
                    SupplierOrgNr = p.Supplier.OrgNr,
                    SupplierVatNr = p.Supplier.VatNr,
                    SysLanguageId = p.Supplier.SysLanguageId,
                    OurCustomerNr = p.Supplier.OurCustomerNr,
                    VatType = p.VatType,
                    TotalAmount = p.TotalAmount,
                    VatAmount = p.VATAmount,
                    SumAmount = p.TotalAmount - p.VATAmount,
                    TotalAmountCurrency = p.TotalAmountCurrency,
                    VatAmountCurrency = p.VATAmountCurrency,
                    SumAmountCurrency = p.TotalAmountCurrency - p.VATAmountCurrency,
                    CurrencyId = p.CurrencyId,
                    CurrencyRate = p.CurrencyRate,
                    CurrencyDate = p.CurrencyDate,
                    PurchaseDate = p.PurchaseDate,
                    ReferenceOur = p.ReferenceOur,
                    ReferenceYour = p.ReferenceYour,
                    PurchaseLabel = p.PurchaseLabel,
                    Attention = p.Attention,
                    PaymentConditionName = p.PaymentCondition.Name,
                    PaymentConditioCode = p.PaymentCondition.Code,
                    DeliveryConditionName = p.DeliveryCondition.Name,
                    DeliveryConditionCode = p.DeliveryCondition.Code,
                    DeliveryTypeName = p.DeliveryType.Name,
                    DeliveryTypeCode = p.DeliveryType.Code,
                    DeliveryAddressId = p.DeliveryAddressId ?? 0,
                    DeliveryAddress = p.DeliveryAddress,
                    WantedDeliveryDate = p.WantedDeliveryDate,
                    State = p.State,
                    OriginStatus = p.Origin.Status,
                    Created = p.Created,
                    CreatedBy = p.CreatedBy,
                    Modified = p.Modified,
                    ModifiedBy = p.ModifiedBy,
                    PurchaseRows = p.PurchaseRow
                        .Where(r => r.State == (int)SoeEntityState.Active)
                        .Select(r => new PurchaseRowReportDTO
                        {
                            Type = (PurchaseRowType)r.Type,
                            PurchaseRowId = r.PurchaseRowId,
                            ProductName = r.Product.Name,
                            RowNr = r.RowNr,
                            ProductNr = r.Product.Number,
                            PurchaseUnitName = r.ProductUnit.Name,
                            PurchaseUnitCode = r.ProductUnit.Code,
                            Quantity = r.Quantity,
                            DeliveredQuantity = r.DeliveredQuantity,
                            PurchasePrice = r.PurchasePrice,
                            PurchasePriceCurrency = r.PurchasePriceCurrency,
                            SumAmount = r.SumAmount,
                            SumAmountCurrency = r.SumAmountCurrency,
                            VatAmount = r.VatAmount,
                            VatAmountCurrency = r.VatAmountCurrency,
                            DiscountAmount = r.DiscountAmount,
                            DiscountAmountCurrency = r.DiscountAmountCurrency,
                            DiscountPercent = r.DiscountPercent,
                            DiscountType = r.DiscountType,
                            Text = r.Text,
                            VatRate = r.VatRate,
                            AccDeliveryDate = r.AccDeliveryDate,
                            WantedDeliveryDate = r.WantedDeliveryDate,
                            DeliveryDate = r.DeliveryDate,
                            Status = r.Status ?? 0,
                            SupplierProductNr = r.SupplierProduct != null ? r.SupplierProduct.SupplierProductNr : string.Empty,
                        }).ToList(),
                };
            }
        }
        public static Expression<Func<PurchaseRow, PurchaseRowDTO>> GetPurchaseRowDTO
        {
            get
            {
                return e => new PurchaseRowDTO
                {
                    PurchaseRowId = e.PurchaseRowId,
                    ProductId = e.ProductId,
                    ProductNr = e.Product.Number,
                    ProductName = e.Product.Name,
                    Quantity = e.Quantity,
                    DeliveredQuantity = e.DeliveredQuantity,
                    DeliveryDate = e.DeliveryDate,
                    PurchasePriceCurrency = e.PurchasePriceCurrency,
                    PurchasePrice = e.PurchasePrice,
                    PurchaseNr = e.Purchase.PurchaseNr,
                    VatAmountCurrency = e.VatAmountCurrency,

                    VatRate = e.VatRate,
                    VatCodeId = e.VatCodeId,
                    RowNr = e.RowNr,
                    AccDeliveryDate = e.AccDeliveryDate,
                    PurchaseUnitId = e.PurchaseUnitId,
                    StockId = e.StockId,
                    StockCode = e.Stock.Code,
                    ParentRowId = e.ParentRowId,
                    Text = e.Text,
                    WantedDeliveryDate = e.WantedDeliveryDate,

                    OrderId = e.OrderId,
                    OrderNr = e.CustomerInvoice.InvoiceNr,

                    DiscountType = e.DiscountType,
                    DiscountAmount = e.DiscountAmount,
                    DiscountAmountCurrency = e.DiscountAmountCurrency,
                    DiscountPercent = e.DiscountPercent,
                    Type = (PurchaseRowType)e.Type,

                    Status = e.Status ?? 0,
                    State = (SoeEntityState)e.State,

                    SupplierProductId = e.SupplierProductId,
                    SupplierProductNr = e.SupplierProduct != null ? e.SupplierProduct.SupplierProductNr : string.Empty,

                    IntrastatTransactionId = e.IntrastatTransactionId,
                };
            }
        }
        public static Expression<Func<PurchaseRow, PurchaseStatisticsDTO>> GetPurchaseStatisticsRowDTO
        { 
            get
            {
                return row => new PurchaseStatisticsDTO
                {
                    ProductNumber = row.Product.Number,
                    ProductName = row.Product.Name,
                    Quantity = row.Quantity,
                    DeliveredQuantity = row.DeliveredQuantity,
                    DeliveryDate = row.DeliveryDate,
                    PurchasePriceCurrency = row.PurchasePriceCurrency,
                    PurchasePrice = row.PurchasePrice,
                    PurchaseNr = row.Purchase.PurchaseNr,
                    AcknowledgeDeliveryDate = row.AccDeliveryDate,
                    WantedDeliveryDate = row.WantedDeliveryDate,
                    CustomerOrderNumber = row.CustomerInvoice.SeqNr.HasValue ? row.CustomerInvoice.SeqNr.Value.ToString() : "",
                    SumAmount = row.SumAmount,
                    SumAmountCurrency = row.SumAmountCurrency,
                    DiscountAmount = row.DiscountAmount,
                    DiscountAmountCurrency = row.DiscountAmountCurrency,
                    Status = row.Purchase.Origin.Status,
                    SysCurrencyId = row.Purchase.Currency.SysCurrencyId,
                    SupplierItemNumber = row.SupplierProduct.SupplierProductNr,
                    SupplierItemName = row.SupplierProduct.Name,
                    SupplierItemCode = row.SupplierProduct.Code,
                    ProjectNumber = row.Purchase.Project.Number,
                    PurchaseDate = row.Purchase.PurchaseDate,
                    StockPlace = row.Stock.Name,
                    RowStatus = row.Status ?? 0,
                    SupplierNr = row.Purchase.Supplier.SupplierNr,
                    SupplierName = row.Purchase.Supplier.Name,
                    ProductUnitCode = row.ProductUnit.Code,
                    Unit = row.ProductUnit.Code,
                    Code = row.SupplierProduct.Code,
                };
            }
        }
        public static Expression<Func<Purchase, PurchaseSmallDTO>> GetPurchaseSmallDTO
        {
            get
            {
                return p => new PurchaseSmallDTO
                {
                    PurchaseId = p.PurchaseId,
                    PurchaseNr = p.PurchaseNr,
                    SupplierId = p.SupplierId,
                    SupplierNr = p.Supplier.SupplierNr,
                    SupplierName = p.Supplier.Name,
                    OriginDescription = p.Origin.Description ?? "",
                    Status = p.Origin.Status,
                };
            }
        }
        public static readonly Expression<Func<Purchase, PurchaseGridDTO>> PurchaseGridDTO =
            p => new PurchaseGridDTO
            {
                PurchaseId = p.PurchaseId,
                PurchaseNr = p.PurchaseNr,
                PurchaseDate = p.PurchaseDate,
                TotalAmount = p.TotalAmount,
                TotalAmountExVat = p.TotalAmount - p.VATAmount,
                TotalAmountExVatCurrency = p.TotalAmountCurrency - p.VATAmountCurrency,
                OriginStatus = p.Origin == null ? SoeOriginStatus.None : (SoeOriginStatus)p.Origin.Status,
                SupplierName = p.Supplier.Name,
                SupplierNr = p.Supplier.SupplierNr,
                SysCurrencyId = p.Currency.SysCurrencyId,
                ProjectNr = p.Project.Number ?? "",
                Origindescription = p.Origin.Description,
                ConfirmedDate = p.ConfirmedDeliveryDate,
                DeliveryDate = p.DeliveryDate,
                StatusIcon = p.StatusIcon
            };
        public static readonly Expression<Func<PurchaseRow, PurchaseRowSmallDTO>> PurchaseRowSmallDTO =
            p => new PurchaseRowSmallDTO
            {
                PurchaseRowId = p.PurchaseRowId,
                PurchaseRowNr = p.RowNr,
                SupplierProductId = p.SupplierProductId,
                SupplierProductNr = p.SupplierProduct.SupplierProductNr ?? string.Empty,
                SupplierProductName = p.SupplierProduct.Name ?? string.Empty,
                ProductId = p.ProductId,
                ProductName = p.Product.Name,
                ProductNumber = p.Product.Number,
                Price = p.PurchasePriceCurrency,
                DeliveredQuantity = p.DeliveredQuantity ?? 0,
                Text = p.Text,
            };

        public static readonly Expression<Func<PurchaseRow, PurchaseDeliveryInvoiceDTO>> PurchaseDeliveryInvoiceDTO =
           p => new PurchaseDeliveryInvoiceDTO
           {
               PurchaseId = p.PurchaseId,
               PurchaseRowId = p.PurchaseRowId,
               PurchaseRowNr = p.RowNr,
               PurchaseNr = p.Purchase.PurchaseNr,
               SupplierProductId = p.SupplierProductId,
               SupplierProductNr = p.SupplierProduct.SupplierProductNr ?? string.Empty,
               SupplierProductName = p.SupplierProduct.Name ?? string.Empty,
               ProductId = p.ProductId,
               ProductName = p.Product.Name,
               ProductNumber = p.Product.Number,
               AskedPrice = p.PurchasePriceCurrency,
               Price = p.PurchasePriceCurrency,
               DeliveredQuantity = p.DeliveredQuantity ?? 0,
               Quantity = p.DeliveredQuantity ?? 0,
               PurchaseQuantity = p.Quantity,
               SupplierinvoiceId = p.PurchaseDeliveryInvoice.FirstOrDefault().SupplierinvoiceId,
               SupplierInvoiceSeqNr = p.PurchaseDeliveryInvoice.FirstOrDefault().SupplierInvoice.SeqNr
           };
    }
}
