using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Xml.Linq;


namespace SoftOne.Soe.Business.Core
{
    public class HouseholdTaxDeductionManager : ManagerBase
    {
        // Create a logger for use in this class
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region GetHouseholdTaxDeductionRows

        public HouseholdTaxDeductionManager(ParameterObject parameterObject) : base(parameterObject) { }

        public List<HouseholdTaxDeductionRow> GetHouseholdTaxDeductionRows(int invoiceId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.HouseholdTaxDeductionRow.NoTracking();
            return GetHouseholdTaxDeductionRows(entities, invoiceId);
        }

        public List<HouseholdTaxDeductionRow> GetHouseholdTaxDeductionRows(CompEntities entities, int invoiceId)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            entitiesReadOnly.HouseholdTaxDeductionRow.NoTracking();
            var actorCompanyId = base.ActorCompanyId;
            return (from h in entities.HouseholdTaxDeductionRow
                    where h.ActorCompanyId == actorCompanyId &&
                    h.CustomerInvoiceRow.InvoiceId == invoiceId &&
                    h.CustomerInvoiceRow.CustomerInvoice.HasHouseholdTaxDeduction &&
                    h.CustomerInvoiceRow.CustomerInvoice.Origin.Status != (int)SoeOriginStatus.Draft &&
                    h.CustomerInvoiceRow.State == (int)SoeEntityState.Active
                    select h).ToList();
        }

        public List<GenericType<int, decimal>> GetHouseholdTaxDeductionRowsPerInvoiceDict(int invoiceId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.HouseholdTaxDeductionRow.NoTracking();
            return GetHouseholdTaxDeductionRowsPerInvoiceDict(entities, invoiceId);
        }

        public List<GenericType<int, decimal>> GetHouseholdTaxDeductionRowsPerInvoiceDict(CompEntities entities, int invoiceId)
        {
            var actorCompanyId = base.ActorCompanyId;
            return (from h in entities.HouseholdTaxDeductionRow
                    where h.ActorCompanyId == actorCompanyId &&
                    h.CustomerInvoiceRow.InvoiceId == invoiceId &&
                    h.CustomerInvoiceRow.CustomerInvoice.HasHouseholdTaxDeduction &&
                    h.CustomerInvoiceRow.CustomerInvoice.Origin.Status != (int)SoeOriginStatus.Draft &&
                    h.CustomerInvoiceRow.State == (int)SoeEntityState.Active
                    select new GenericType<int, decimal>()
                    {
                        Field1 = h.CustomerInvoiceRowId,
                        Field2 = h.AmountCurrency,
                    }).ToList();
        }

        public List<HouseholdTaxDeductionApplicantDTO> GetHouseholdTaxDeductionRows(int actorCompanyId, int customerId, bool addEmptyRow = false)
        {
            var result = new List<HouseholdTaxDeductionApplicantDTO>();

            if (addEmptyRow)
            {
                result.Add(new HouseholdTaxDeductionApplicantDTO()
                {
                    Name = " ",
                    ApartmentNr = string.Empty,
                    CooperativeOrgNr = string.Empty,
                    Property = string.Empty,
                    SocialSecNr = string.Empty,
                    ShowButton = false,
                    IdentifierString = string.Empty,
                    Hidden = false,
                });
            }

            using (var entities = new CompEntities())
            {
                var householdApplicants = (from h in entities.HouseholdTaxDeductionApplicant
                                           where h.ActorCustomerId == customerId &&
                                           h.State == (int)SoeEntityState.Active
                                           select h);

                List<string> exclude = new List<string>();
                foreach (HouseholdTaxDeductionApplicant app in householdApplicants)
                {
                    result.Add(new HouseholdTaxDeductionApplicantDTO()
                    {
                        HouseholdTaxDeductionApplicantId = app.HouseholdTaxDeductionApplicantId,
                        Property = app.Property,
                        Name = app.Name,
                        CooperativeOrgNr = app.CooperativeOrgNr,
                        ApartmentNr = app.ApartmentNr,
                        SocialSecNr = app.SocialSecNr,
                        Share = app.Share.HasValue ? app.Share.Value : decimal.Zero,
                        ShowButton = true,
                        IdentifierString = app.Name + ";" + app.SocialSecNr,
                        Hidden = false,
                    });

                    exclude.Add(app.SocialSecNr);
                }

                List<string> list = (from h in entities.HouseholdTaxDeductionRow
                                     where h.ActorCompanyId == actorCompanyId &&
                                     h.State == (int)SoeEntityState.Active &&
                                     h.CustomerInvoiceRow.CustomerInvoice.ActorId == customerId &&
                                     h.CustomerInvoiceRow.CustomerInvoice.HasHouseholdTaxDeduction &&
                                     h.CustomerInvoiceRow.State == (int)SoeEntityState.Active
                                     select h.SocialSecNr).Distinct().ToList();

                foreach (var item in list.Where(s => !exclude.Contains(s)))
                {
                    HouseholdTaxDeductionRow row = (from h in entities.HouseholdTaxDeductionRow
                                                    where h.ActorCompanyId == actorCompanyId &&
                                                    h.State == (int)SoeEntityState.Active &&
                                                    h.SocialSecNr == item &&
                                                    !h.Hidden &&
                                                    h.CustomerInvoiceRow.CustomerInvoice.ActorId == customerId &&
                                                    h.CustomerInvoiceRow.CustomerInvoice.HasHouseholdTaxDeduction &&
                                                    h.CustomerInvoiceRow.State == (int)SoeEntityState.Active
                                                    orderby h.CustomerInvoiceRow.Created descending
                                                    select h).FirstOrDefault();

                    if (row != null)
                    {
                        result.Add(new HouseholdTaxDeductionApplicantDTO()
                        {
                            Property = row.Property,
                            Name = row.Name,
                            CooperativeOrgNr = row.CooperativeOrgNr,
                            ApartmentNr = row.ApartmentNr,
                            SocialSecNr = row.SocialSecNr,
                            ShowButton = true,
                            IdentifierString = row.Name + ";" + row.SocialSecNr,
                            Hidden = false,
                        });
                    }
                }
            }

            return result;
        }
        public List<HouseholdTaxDeductionGridViewDTO> GetHouseholdTaxDeductionRows(int actorCompanyId, SoeHouseholdClassificationGroup classificationGroup)
        {
            TermGroup_HouseHoldTaxDeductionType defaultDeductionType = (TermGroup_HouseHoldTaxDeductionType)SettingManager.GetIntSetting(SettingMainType.User, (int)UserSettingType.BillingInvoiceDefaultHouseholdTaxType, base.UserId, actorCompanyId, 0);
            return GetHouseholdTaxDeductionRows_v2(actorCompanyId, classificationGroup, defaultDeductionType);
        }

        public List<HouseholdTaxDeductionGridViewDTO> GetHouseholdTaxDeductionRows(int actorCompanyId, SoeHouseholdClassificationGroup classificationGroup, TermGroup_HouseHoldTaxDeductionType taxDeductionType)
        {
            int productId15 = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.ProductGreen15TaxDeduction, 0, actorCompanyId, 0);
            int productId20 = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.ProductGreen20TaxDeduction, 0, actorCompanyId, 0);
            int productId50 = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.ProductGreen50TaxDeduction, 0, actorCompanyId, 0);
            int productROTId50 = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.ProductHousehold50TaxDeduction, 0, actorCompanyId, 0);

            var dtos = GetHouseholdTaxDeductionDTO(actorCompanyId, classificationGroup, taxDeductionType);

            foreach (var dto in dtos)
            {
                dto.HouseHoldTaxDeductionTypeName = GetText(dto.HouseHoldTaxDeductionType, (int)TermGroup.HouseHoldTaxDeductionType);
                if (productId15 != 0 && dto.ProductId == productId15)
                {
                    dto.HouseHoldTaxDeductionPercent = "15%";
                }
                else if (productId20 != 0 && dto.ProductId == productId20)
                {
                    dto.HouseHoldTaxDeductionPercent = "20%";
                }
                else if ((productId50 != 0 && dto.ProductId == productId50) || (productROTId50 != 0 && dto.ProductId == productROTId50))
                {
                    dto.HouseHoldTaxDeductionPercent = "50%";
                }
            }
            return dtos;
        }

        public List<HouseholdTaxDeductionGridViewDTO> GetHouseholdTaxDeductionRows_v2(int actorCompanyId, SoeHouseholdClassificationGroup classificationGroup, TermGroup_HouseHoldTaxDeductionType taxDeductionType)
        {
            int productId15 = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.ProductGreen15TaxDeduction, 0, actorCompanyId, 0);
            int productId20 = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.ProductGreen20TaxDeduction, 0, actorCompanyId, 0);
            int productId50 = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.ProductGreen50TaxDeduction, 0, actorCompanyId, 0);
            int productROTId50 = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.ProductHousehold50TaxDeduction, 0, actorCompanyId, 0);

            var dtos = GetHouseholdTaxDeductionDTO(actorCompanyId, classificationGroup, taxDeductionType).OrderBy(h => h.InvoiceId).ToList();

            foreach (var dto in dtos)
            {
                dto.HouseHoldTaxDeductionTypeName = GetText(dto.HouseHoldTaxDeductionType, (int)TermGroup.HouseHoldTaxDeductionType);
                if (productId15 != 0 && dto.ProductId == productId15)
                {
                    dto.HouseHoldTaxDeductionPercent = "15%";
                }
                else if (productId20 != 0 && dto.ProductId == productId20)
                {
                    dto.HouseHoldTaxDeductionPercent = "20%";
                }
                else if ((productId50 != 0 && dto.ProductId == productId50) || (productROTId50 != 0 && dto.ProductId == productROTId50))
                {
                    dto.HouseHoldTaxDeductionPercent = "50%";
                }

                if(classificationGroup == SoeHouseholdClassificationGroup.Received)
                {
                    if (dto.FullyPayed)
                    {
                        if (dto.Applied)
                        {
                            if (dto.Received)
                            {
                                if (dto.Amount != dto.ApprovedAmount)
                                {
                                    dto.HouseholdStatus = GetText(1580, TermGroup.AngularBilling);
                                    dto.Status = 6;
                                    dto.StatusIcon = "circle";
                                    dto.StatusIconClass = "warning-color";
                                }
                                else
                                {
                                    dto.HouseholdStatus = GetText(1577, TermGroup.AngularBilling);
                                    dto.Status = 4;
                                    dto.StatusIcon = "circle";
                                    dto.StatusIconClass = "success-color";
                                }
                            }
                            else if (dto.Denied)
                            {
                                dto.HouseholdStatus = GetText(1532, TermGroup.AngularBilling);
                                dto.Status = 5;
                                dto.StatusIcon = "circle";
                                dto.StatusIconClass = "error-color";
                            }
                            else
                            {
                                dto.HouseholdStatus = GetText(1528, TermGroup.AngularBilling);
                                dto.Status = 3;
                                dto.StatusIcon = "circle";
                                dto.StatusIconClass = "information-color";
                            }
                        }
                        else
                        {
                            dto.HouseholdStatus = GetText(1530, TermGroup.AngularBilling);
                            dto.Status = 2;
                            dto.StatusIcon = "circle";
                            dto.StatusIconClass = "medium-grey-color";
                        }
                    }
                    else
                    {
                        dto.HouseholdStatus = GetText(1529, TermGroup.AngularBilling);
                        dto.Status = 1;
                        dto.StatusIcon = "circle";
                        dto.StatusIconClass = "medium-grey-color";
                    }
                }
            }
            return dtos;
        }

        public Dictionary<int, string> GetMobileHouseHoldDeductionTypes(bool incRot, bool incRut, bool incGreenTech)
        {
            var result = new Dictionary<int, string>();

            if (incRot)
            {
                result.Add((int)TermGroup_MobileHouseHoldTaxDeductionType.ROT, GetText((int)TermGroup_MobileHouseHoldTaxDeductionType.ROT, (int)TermGroup.MobileHouseHoldTaxDeductionType, "ROT"));
                result.Add((int)TermGroup_MobileHouseHoldTaxDeductionType.ROT_50, GetText((int)TermGroup_MobileHouseHoldTaxDeductionType.ROT_50, (int)TermGroup.MobileHouseHoldTaxDeductionType, "ROT 50%"));
            }

            if (incRut)
            {
                result.Add((int)TermGroup_MobileHouseHoldTaxDeductionType.RUT, "RUT");
            }

            if(incGreenTech)
            {
                result.Add((int)TermGroup_MobileHouseHoldTaxDeductionType.SolarPanels, GetText((int)TermGroup_MobileHouseHoldTaxDeductionType.SolarPanels, (int)TermGroup.MobileHouseHoldTaxDeductionType, "Solcellsystem"));
                result.Add((int)TermGroup_MobileHouseHoldTaxDeductionType.EneryStorage, GetText((int)TermGroup_MobileHouseHoldTaxDeductionType.EneryStorage, (int)TermGroup.MobileHouseHoldTaxDeductionType, "Energilager"));
                result.Add((int)TermGroup_MobileHouseHoldTaxDeductionType.ChargePoint, GetText((int)TermGroup_MobileHouseHoldTaxDeductionType.ChargePoint, (int)TermGroup.MobileHouseHoldTaxDeductionType, "Laddpunkt"));
            }

            return result;
        }

        public ActionResult GetHouseholdTaxDeductionRowInfo(int invoiceId, int customerInvoiceRowId)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            var info = entitiesReadOnly.GetHouseholdTaxDeductionInfo(invoiceId).FirstOrDefault();

            var infoList = new List<string>();
            if (info != null)
            {
                infoList.Add($"{GetText(1910, "Fakturanr")}: " + info.InvoiceNr);
                infoList.Add($"{GetText(3654, "Timmar")}: " + Math.Round(info.WorkQuantity, 2));
                infoList.Add($"{GetText(7505, "Kostnad för installationen ink moms")}: " + info.TotalAmount);
                infoList.Add($"{GetText(7502, "Övrig kostnad ink moms")}: " + info.OtherAmount);
                infoList.Add($"{GetText(7503, "Betalt belopp")}: " + (info.PaidAmount > info.OtherAmount ? info.PaidAmount - info.OtherAmount : info.PaidAmount));
                infoList.Add($"{GetText(7504, "Begärt belopp")}: " + info.TaxAmount);
            }

            return new ActionResult
            {
                Strings = infoList
            };
        }

        #endregion

        #region Applicants
        public List<HouseholdTaxDeductionApplicantDTO> GetHouseholdTaxDeductionApplicants(int actorCompanyId, int customerId, bool addEmptyRow = false, bool showAllApplicants = true)
        {
            var result = new List<HouseholdTaxDeductionApplicantDTO>();

            if (addEmptyRow)
            {
                result.Add(new HouseholdTaxDeductionApplicantDTO
                {
                    Name = " ",
                    ApartmentNr = string.Empty,
                    CooperativeOrgNr = string.Empty,
                    Property = string.Empty,
                    SocialSecNr = string.Empty,
                    ShowButton = false,
                    IdentifierString = string.Empty,
                    Hidden = false,
                });
            }

            using (var entities = new CompEntities())
            {
                var householdApplicants = (from h in entities.HouseholdTaxDeductionApplicant
                                           where h.ActorCustomerId == customerId &&
                                           h.State == (int)SoeEntityState.Active
                                           select h);

                List<string> exclude = new List<string>();
                foreach (HouseholdTaxDeductionApplicant app in householdApplicants)
                {
                    result.Add(new HouseholdTaxDeductionApplicantDTO()
                    {
                        HouseholdTaxDeductionApplicantId = app.HouseholdTaxDeductionApplicantId,
                        Property = app.Property,
                        Name = app.Name,
                        CooperativeOrgNr = app.CooperativeOrgNr,
                        ApartmentNr = app.ApartmentNr,
                        SocialSecNr = app.SocialSecNr,
                        Share = app.Share.HasValue ? app.Share.Value : decimal.Zero,
                        ShowButton = true,
                        IdentifierString = app.Name + ";" + app.SocialSecNr,
                        Hidden = false,
                    });

                    exclude.Add(app.SocialSecNr);
                }

                if (showAllApplicants)
                {
                    var list = (from h in entities.HouseholdTaxDeductionRow
                                where h.ActorCompanyId == actorCompanyId &&
                                h.CustomerInvoiceRow.CustomerInvoice.ActorId == customerId &&
                                h.CustomerInvoiceRow.CustomerInvoice.HasHouseholdTaxDeduction &&
                                h.State == (int)SoeEntityState.Active &&
                                h.CustomerInvoiceRow.State == (int)SoeEntityState.Active &&
                                !h.Hidden &&
                                !h.ContactHidden
                                select new HouseholdTaxDeductionApplicantDTO()
                                {
                                    Property = h.Property.Trim(),
                                    Name = h.Name.Trim(),
                                    CooperativeOrgNr = !String.IsNullOrEmpty(h.CooperativeOrgNr) ? h.CooperativeOrgNr.Trim() : String.Empty,
                                    ApartmentNr = !String.IsNullOrEmpty(h.ApartmentNr) ? h.ApartmentNr.Trim() : String.Empty,
                                    SocialSecNr = h.SocialSecNr.Trim(),
                                    ShowButton = true,
                                    IdentifierString = h.Name + ";" + h.SocialSecNr,
                                    Hidden = false,
                                    CustomerInvoiceRowId = h.CustomerInvoiceRowId,
                                }).ToList();

                    foreach (var item in list)
                    {
                        if (!result.Any(i => i.Property.ToLower() == item.Property.ToLower() && i.Name.ToLower() == item.Name.ToLower() && i.CooperativeOrgNr == item.CooperativeOrgNr && i.ApartmentNr == item.ApartmentNr && i.SocialSecNr == item.SocialSecNr))
                        {
                            result.Add(item);
                        }
                    }
                }
            }

            return result;
        }
        #endregion


        public HouseholdTaxDeductionRow GetHouseholdTaxDeductionRow(int customerInvoiceRowId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.HouseholdTaxDeductionRow.NoTracking();
            return GetHouseholdTaxDeductionRow(entities, customerInvoiceRowId, false, false);
        }

        public HouseholdTaxDeductionRow GetHouseholdTaxDeductionRow(CompEntities entities, int customerInvoiceRowId, bool loadInvoice=false, bool loadInvoiceRow= false)
        {
            IQueryable<HouseholdTaxDeductionRow> query = entities.HouseholdTaxDeductionRow;

            if (loadInvoice)
                query = query.Include("CustomerInvoiceRow.CustomerInvoice");
            else if (loadInvoiceRow)
                query = query.Include("CustomerInvoiceRow");

            return (from r in query
                    where r.CustomerInvoiceRowId == customerInvoiceRowId && r.State == (int)SoeEntityState.Active
                    select r).FirstOrDefault();
        }

        public HouseholdTaxDeductionApplicantDTO GetHouseholdTaxDeductionRowForEdit(int customerInvoiceRowId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.HouseholdTaxDeductionRow.NoTracking();
            return GetHouseholdTaxDeductionRowForEdit(entities, customerInvoiceRowId);
        }

        public HouseholdTaxDeductionApplicantDTO GetHouseholdTaxDeductionRowForEdit(CompEntities entities, int customerInvoiceRowId)
        {
            var item = (from r in entities.HouseholdTaxDeductionRow
                        where r.CustomerInvoiceRowId == customerInvoiceRowId && 
                               r.State == (int)SoeEntityState.Active
                        select r).FirstOrDefault();

            if(item != null)
            {
                return new HouseholdTaxDeductionApplicantDTO()
                {
                    //using applicant property for row id
                    HouseholdTaxDeductionApplicantId = customerInvoiceRowId,
                    Property = item.Property,
                    Name = item.Name,
                    CooperativeOrgNr = item.CooperativeOrgNr,
                    ApartmentNr = item.ApartmentNr,
                    SocialSecNr = item.SocialSecNr,
                    Comment = item.Comment,
                };
            }

            return null;
        }

        public HouseholdTaxDeductionRowForFileDTO GetHouseholdTaxDeductionRowForFile(int customerInvoiceRowId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.HouseholdTaxDeductionRow.NoTracking();
            return GetHouseholdTaxDeductionRowForFile(entities, customerInvoiceRowId);
        }

        public HouseholdTaxDeductionRowForFileDTO GetHouseholdTaxDeductionRowForFile(CompEntities entities, int customerInvoiceRowId)
        {
            var item = (from r in entities.HouseholdTaxDeductionRow
                        .Include("CustomerInvoiceRow.CustomerInvoice")
                        where r.CustomerInvoiceRowId == customerInvoiceRowId &&
                               r.State == (int)SoeEntityState.Active
                        select r).FirstOrDefault();

            if (item != null)
            {
                return new HouseholdTaxDeductionRowForFileDTO()
                {
                    //using applicant property for row id
                    CustomerInvoiceRowId = customerInvoiceRowId,
                    InvoiceId = item.CustomerInvoiceRow.InvoiceId,
                    ProductId = item.CustomerInvoiceRow.ProductId,
                    InvoiceNr = item.CustomerInvoiceRow.CustomerInvoice.InvoiceNr,
                    Amount = item.Amount,
                    TotalAmountCurrency = item.CustomerInvoiceRow.CustomerInvoice.TotalAmountCurrency,
                    Property = item.Property,
                    Name = item.Name,
                    CooperativeOrgNr = item.CooperativeOrgNr,
                    ApartmentNr = item.ApartmentNr,
                    SocialSecNr = item.SocialSecNr,
                    Comment = item.Comment,
                    HouseHoldTaxDeductionType = item.HouseHoldTaxDeductionType,
                };
            }

            return null;
        }


        #region Save/Modify

        /// <summary>
        /// Currently only used from mobile
        /// </summary>
        /// <param name="actorCompanyId"></param>
        /// <param name="orderId"></param>
        /// <param name="rowId"></param>
        /// <param name="productId"></param>
        /// <param name="roleId"></param>
        /// <param name="quantity"></param>
        /// <param name="householdAmount"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public ActionResult SaveHouseholdDeductionRow(int actorCompanyId, int roleId, int orderId, int orderRowId, string propertyLabel, string socialSecNbr, string name, decimal householdAmountCurrency, string apartmentNbr, string cooperativeOrgNbr, bool isHDRut, int mobileDeductionType)
        {
            var result = new ActionResult();
            CustomerInvoiceRow householdCustomerInvoiceRow = null;
            householdAmountCurrency = Math.Round(householdAmountCurrency);

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        #region Prereq

                        CustomerInvoice order = InvoiceManager.GetCustomerInvoiceByType(entities, orderId, SoeOriginType.Order, OrderInvoiceRegistrationType.Order, actorCompanyId, loadInvoiceRow: true, loadInvoiceAccountRow: true, loadProject: true);
                        if (order == null)
                            return new ActionResult((int)ActionResultSave.EntityNotFound, "CustomerInvoice");

                        var customer = CustomerManager.GetCustomerSmall(entities, order.ActorId.Value);
                        if (customer == null)
                            return new ActionResult((int)ActionResultSave.EntityNotFound, "Customer");

                        // Check if order has PriceListType is inclusive VAT
                        if (order.PriceListTypeId != null)
                        {
                            InvoiceManager.SetPriceListTypeInclusiveVat(entities, order, actorCompanyId, true);
                        }

                        // Check if other payment condition id than setting
                        int defaultPaymentConditionHouseholdId = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.CustomerPaymentDefaultPaymentConditionHouseholdDeduction, 0, actorCompanyId, 0);
                        if (defaultPaymentConditionHouseholdId != 0 && order.PaymentConditionId != defaultPaymentConditionHouseholdId)
                            order.PaymentConditionId = defaultPaymentConditionHouseholdId;

                        #endregion

                        SetModifiedProperties(order);

                        #region create/update HouseholdTaxDeductionRow

                        InvoiceProduct householdBaseProduct;

                        HouseholdTaxDeductionRow householdTaxDeductionRow = GetHouseholdTaxDeductionRow(entities, orderRowId, false, true);
                        if (householdTaxDeductionRow == null)
                        {
                            householdTaxDeductionRow = new HouseholdTaxDeductionRow
                            {
                                ActorCompanyId = actorCompanyId,
                                Applied = false,
                                AppliedDate = null,
                                Received = false,
                                ReceivedDate = null,
                                HouseHoldTaxDeductionType = (int)GetHouseHoldTaxDeductionType(isHDRut, (TermGroup_MobileHouseHoldTaxDeductionType)mobileDeductionType)
                            };
                            householdBaseProduct = GetTaxProduct(entities, actorCompanyId, isHDRut, (TermGroup_MobileHouseHoldTaxDeductionType)mobileDeductionType);
                            SetCreatedProperties(householdTaxDeductionRow);
                        }
                        else
                        {
                            householdBaseProduct = ProductManager.GetInvoiceProduct(entities, householdTaxDeductionRow.CustomerInvoiceRow.ProductId.GetValueOrDefault());
                            SetModifiedProperties(householdTaxDeductionRow);
                        }

                        householdTaxDeductionRow.Property = propertyLabel;
                        householdTaxDeductionRow.SocialSecNr = socialSecNbr;
                        householdTaxDeductionRow.Name = name;
                        householdTaxDeductionRow.Amount = CountryCurrencyManager.GetBaseAmountFromCurrencyAmount(householdAmountCurrency, order.CurrencyRate);
                        householdTaxDeductionRow.AmountCurrency = householdAmountCurrency;
                        householdTaxDeductionRow.ApartmentNr = apartmentNbr;
                        householdTaxDeductionRow.CooperativeOrgNr = cooperativeOrgNbr;

                        #endregion

                        #region Get HouseHold Baseproduct

                        if (householdBaseProduct == null)
                        {
                            if (isHDRut)
                            {
                                return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(9200, "Basartikeln för rut-avdrag kunde inte hittas"));
                            }
                            else
                            {

                                return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(9064, "Basartikeln för ROT-avdrag kunde inte hittas"));
                            }
                        }

                        #endregion

                        #region Save household customerinvoicerow

                        result = InvoiceManager.SaveCustomerInvoiceRow(transaction, entities, actorCompanyId, order, orderRowId, householdBaseProduct.ProductId, 1, decimal.Negate(householdAmountCurrency), householdBaseProduct.Name, SoeInvoiceRowType.ProductRow, householdTaxDeductionRow, productRowType:SoeProductRowType.HouseholdTaxDeduction);
                        if (!result.Success)
                            return result;

                        householdCustomerInvoiceRow = result.Value as CustomerInvoiceRow;

                        #endregion

                        #region Save household textrow

                        string text = GetProductRowText(householdTaxDeductionRow);
                        var textRow = InvoiceManager.GetHouseholdDeductionTextRow(entities, householdCustomerInvoiceRow.CustomerInvoiceRowId);
                        if (textRow == null)
                        {
                            result = InvoiceManager.SaveCustomerInvoiceRow(transaction, entities, actorCompanyId, order, 0, 0, 1, 0, text, SoeInvoiceRowType.TextRow, parentRowId: householdCustomerInvoiceRow.CustomerInvoiceRowId);
                            if (!result.Success)
                                return result;

                            textRow = result.Value as CustomerInvoiceRow;

                            if (householdBaseProduct.ShowDescriptionAsTextRow && !string.IsNullOrEmpty(householdBaseProduct.Description))
                            {
                                result = InvoiceManager.SaveCustomerInvoiceRow(transaction, entities, actorCompanyId, order, 0, 0, 1, 0, householdBaseProduct.Description, SoeInvoiceRowType.TextRow, parentRowId: householdCustomerInvoiceRow.CustomerInvoiceRowId);
                                if (!result.Success)
                                    return result;
                            }
                        }
                        else
                            textRow.Text = text;


                        textRow.ParentRow = householdCustomerInvoiceRow;

                        #endregion

                        order.HasHouseholdTaxDeduction = true;

                        #region Save

                        if (result.Success)
                            result = SaveChanges(entities, transaction);

                        if (!result.Success)
                            return result;

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
                    result.IntegerValue = 0;
                }
                finally
                {
                    if (result.Success)
                    {
                        //Set success properties                        
                    }
                    else
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }
                return result;
            }
        }

        public int GetHouseholdTaxDeductionRowsCounter(int actorCompanyId, SoeHouseholdClassificationGroup classificationGroup, TermGroup_HouseHoldTaxDeductionType taxDeductionType)
        {
            return GetHouseholdTaxDeductionDTO(actorCompanyId, classificationGroup, taxDeductionType).Count;
        }

        public ActionResult SaveHouseholdTaxApplied(List<int> ids)
        {
            ActionResult result = new ActionResult(true);
            if (ids == null || ids.Count == 0)
                return result;

            using (CompEntities entities = new CompEntities())
            {
                foreach (int id in ids)
                {
                    HouseholdTaxDeductionRow originalRow = GetHouseholdTaxDeductionRow(entities, id, false,false);
                    if (originalRow == null)
                        return new ActionResult((int)ActionResultSave.EntityNotFound, "HouseholdTaxDeductionRow");

                    originalRow.Applied = true;
                    originalRow.AppliedDate = DateTime.Today;
                    SetModifiedProperties(originalRow);
                }

                result = SaveChanges(entities);
            }

            return result;
        }

        public ActionResult WithdrawHouseholdApplied(List<int> ids)
        {
            ActionResult result = new ActionResult(true);
            if (ids == null || ids.Count == 0)
                return result;

            using (CompEntities entities = new CompEntities())
            {
                foreach (int id in ids)
                {
                    HouseholdTaxDeductionRow originalRow = GetHouseholdTaxDeductionRow(entities, id, false, false);
                    if (originalRow == null)
                        return new ActionResult((int)ActionResultSave.EntityNotFound, "HouseholdTaxDeductionRow");

                    originalRow.Applied = false;
                    originalRow.AppliedDate = null;
                    SetModifiedProperties(originalRow);
                }

                result = SaveChanges(entities);
            }

            return result;
        }

        public ActionResult SaveHouseholdTaxReceived(List<int> ids, DateTime? receivedDate, decimal? amount = null)
        {
            ActionResult result = new ActionResult(true);
            if (ids == null || ids.Count == 0)
                return result;

            using (CompEntities entities = new CompEntities())
            {
                foreach (int id in ids)
                {
                    HouseholdTaxDeductionRow originalRow = GetHouseholdTaxDeductionRow(entities, id, false, false);
                    if (originalRow == null)
                        return new ActionResult((int)ActionResultSave.EntityNotFound, "HouseholdTaxDeductionRow");

                    originalRow.Received = true;
                    originalRow.ReceivedDate = receivedDate != null ? receivedDate : DateTime.Today;

                    if(amount.HasValue && amount.Value != 0)
                        originalRow.ApprovedAmount = originalRow.ApprovedAmountCurrency = amount.Value;

                    SetModifiedProperties(originalRow);
                }

                result = SaveChanges(entities);
                result.Keys = ids;
            }

            return result;
        }

        public ActionResult WithdrawHouseholdTaxReceived(int id)
        {
            ActionResult result = new ActionResult(true);

            using (CompEntities entities = new CompEntities())
            {
                HouseholdTaxDeductionRow originalRow = GetHouseholdTaxDeductionRow(entities, id, false, false);
                if (originalRow == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "HouseholdTaxDeductionRow");

                originalRow.Received = false;
                originalRow.ReceivedDate = null;
                originalRow.ApprovedAmount = null;
                originalRow.ApprovedAmountCurrency = null;

                if (originalRow.VoucherHeadId.HasValue)
                    originalRow.VoucherHeadId = null;

                SetModifiedProperties(originalRow);

                result = SaveChanges(entities);
            }

            return result;
        }

        public ActionResult SaveHouseholdTaxDenied(int invoiceId, int customerInvoiceRowId, DateTime? deniedDate)
        {
            ActionResult result = new ActionResult(true);

            using (CompEntities entities = new CompEntities())
            {
                HouseholdTaxDeductionRow originalRow = GetHouseholdTaxDeductionRow(entities, customerInvoiceRowId, false, false);
                if (originalRow == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "HouseholdTaxDeductionRow");

                originalRow.Denied = true;
                originalRow.DeniedDate = deniedDate.HasValue ? deniedDate : DateTime.Today;
                SetModifiedProperties(originalRow);

                result = SaveChanges(entities);
                result.Value = invoiceId;
                result.IntegerValue = customerInvoiceRowId;
            }

            return result;
        }

        public ActionResult DeleteHouseholdTaxDeductionRow(int rowId)
        {
            using (CompEntities entities = new CompEntities())
            {
                // Get row
                HouseholdTaxDeductionRow originalRow = GetHouseholdTaxDeductionRow(entities, rowId, false, false);
                if (originalRow == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "HouseholdTaxDeductionRow");

                return ChangeEntityState(entities, originalRow, SoeEntityState.Deleted, true);
            }
        }

        #endregion

        public static TermGroup_HouseHoldTaxDeductionType GetHouseHoldTaxDeductionType(bool isHDRut, TermGroup_MobileHouseHoldTaxDeductionType mobileHouseHoldTaxDeductionType)
        {
            switch (mobileHouseHoldTaxDeductionType)
            {
                case TermGroup_MobileHouseHoldTaxDeductionType.ChargePoint:
                case TermGroup_MobileHouseHoldTaxDeductionType.EneryStorage:
                case TermGroup_MobileHouseHoldTaxDeductionType.SolarPanels:
                    return TermGroup_HouseHoldTaxDeductionType.GREEN;
                case TermGroup_MobileHouseHoldTaxDeductionType.ROT:
                case TermGroup_MobileHouseHoldTaxDeductionType.ROT_50:
                    return TermGroup_HouseHoldTaxDeductionType.ROT;
                case TermGroup_MobileHouseHoldTaxDeductionType.RUT:
                    return TermGroup_HouseHoldTaxDeductionType.RUT;
                default:
                    return isHDRut ? TermGroup_HouseHoldTaxDeductionType.RUT : TermGroup_HouseHoldTaxDeductionType.ROT;
            }
        }

        public string GetProductRowText(HouseholdTaxDeductionRow row)
        {
            string brf = !string.IsNullOrEmpty(row.ApartmentNr) || !string.IsNullOrEmpty(row.CooperativeOrgNr) ? string.Format(GetText(9065, " (Brf: {0}, Lgh: {1})"), row.CooperativeOrgNr, row.ApartmentNr) : string.Empty;
            switch ((TermGroup_HouseHoldTaxDeductionType)row.HouseHoldTaxDeductionType)
            {
                case TermGroup_HouseHoldTaxDeductionType.ROT:
                    return string.Format(GetText(9066, "ROT-avdrag för {0}{1}, {2} {3}"), row.Property, brf, row.SocialSecNr, row.Name);
                case TermGroup_HouseHoldTaxDeductionType.RUT:
                    return string.Format(GetText(9201, "RUT-avdrag för {0} {1}"), row.SocialSecNr, row.Name);
                case TermGroup_HouseHoldTaxDeductionType.GREEN:
                    return string.Format(GetText(7511, "Grönteknik avdrag för {0}{1}, {2} {3}"), row.Property, brf, row.SocialSecNr, row.Name);
                default:
                    return "";
            }
        }

        public InvoiceProduct GetTaxProduct(CompEntities entities, int actorCompanyId, bool isHDRut, TermGroup_MobileHouseHoldTaxDeductionType mobileHouseHoldTaxDeductionType)
        {
            CompanySettingType settingType;

            switch (mobileHouseHoldTaxDeductionType)
            {
                case TermGroup_MobileHouseHoldTaxDeductionType.ChargePoint:
                case TermGroup_MobileHouseHoldTaxDeductionType.EneryStorage:
                    settingType = CompanySettingType.ProductGreen50TaxDeduction;
                    break;
                case TermGroup_MobileHouseHoldTaxDeductionType.SolarPanels:
                    settingType = CompanySettingType.ProductGreen20TaxDeduction;
                    break;
                case TermGroup_MobileHouseHoldTaxDeductionType.RUT:
                    settingType = CompanySettingType.ProductRUTTaxDeduction;
                    break;
                case TermGroup_MobileHouseHoldTaxDeductionType.ROT:
                    settingType = CompanySettingType.ProductHouseholdTaxDeduction;
                    break;
                case TermGroup_MobileHouseHoldTaxDeductionType.ROT_50:
                    settingType = CompanySettingType.ProductHousehold50TaxDeduction;
                    break;
                default:
                    settingType = isHDRut ? CompanySettingType.ProductRUTTaxDeduction : CompanySettingType.ProductHouseholdTaxDeduction;
                    break;
            }

            return ProductManager.GetInvoiceProductFromSetting(entities, settingType, actorCompanyId);
        }

        #region Create

        private ActionResult ValidateHouseholdTaxDeductionRow(IDictionary<string, object> invoiceRowInput)
        {
            // Validate
            if (!invoiceRowInput.ContainsKey("householdsocialsecnr") || string.IsNullOrEmpty((string)invoiceRowInput["householdsocialsecnr"]))
                return new ActionResult((int)ActionResultSave.EntityNotCreated, GetText(7637, "Personnummer saknas på avdragsrad"));

            if (!invoiceRowInput.ContainsKey("householdtaxdeductiontype") || (int)invoiceRowInput["householdtaxdeductiontype"] == 0)
                return new ActionResult((int)ActionResultSave.EntityNotCreated, GetText(7638, "Avdragstyp saknas på avdragsrad"));

            if ((int)invoiceRowInput["householdtaxdeductiontype"] != (int)TermGroup_HouseHoldTaxDeductionType.RUT && (!invoiceRowInput.ContainsKey("householdproperty") && string.IsNullOrEmpty((string)invoiceRowInput["householdproperty"])) && (!invoiceRowInput.ContainsKey("householdcooperativeorgnbr") || string.IsNullOrEmpty((string)invoiceRowInput["householdcooperativeorgnbr"])))
                return new ActionResult((int)ActionResultSave.EntityNotCreated, GetText(7639, "Avdragsrad saknar obligatorisk information"));

            return new ActionResult();
        }

        private ActionResult ValidateHouseholdTaxDeductionRow(ProductRowDTO dto)
        {
            // Validate
            if (string.IsNullOrEmpty(dto.HouseholdSocialSecNbr))
                return new ActionResult((int)ActionResultSave.EntityNotCreated, GetText(7637, "Personnummer saknas på avdragsrad"));

            if (dto.HouseholdTaxDeductionType == TermGroup_HouseHoldTaxDeductionType.None)
                return new ActionResult((int)ActionResultSave.EntityNotCreated, GetText(7638, "Avdragstyp saknas på avdragsrad"));

            if (dto.HouseholdTaxDeductionType != TermGroup_HouseHoldTaxDeductionType.RUT && string.IsNullOrEmpty(dto.HouseholdProperty) && string.IsNullOrEmpty(dto.HouseholdCooperativeOrgNbr))
                return new ActionResult((int)ActionResultSave.EntityNotCreated, GetText(7639, "Avdragsrad saknar obligatorisk information"));

            return new ActionResult();
        }

        private ActionResult ValidateHouseholdTaxDeductionRow(CustomerInvoiceRowDTO dto)
        {
            // Validate
            if (string.IsNullOrEmpty(dto.HouseholdSocialSecNbr))
                return new ActionResult((int)ActionResultSave.EntityNotCreated, GetText(7637, "Personnummer saknas på avdragsrad"));

            if (dto.HouseHoldTaxDeductionType == 0)
                return new ActionResult((int)ActionResultSave.EntityNotCreated, GetText(7638, "Avdragstyp saknas på avdragsrad"));

            if (dto.HouseHoldTaxDeductionType != (int)TermGroup_HouseHoldTaxDeductionType.RUT && string.IsNullOrEmpty(dto.HouseholdProperty) && string.IsNullOrEmpty(dto.HouseholdCooperativeOrgNbr))
                return new ActionResult((int)ActionResultSave.EntityNotCreated, GetText(7639, "Avdragsrad saknar obligatorisk information"));

            return new ActionResult();
        }

        public ActionResult CreateHouseholdTaxDeductionRow(CustomerInvoice invoice, CustomerInvoiceRow row, IDictionary<string, object> invoiceRowInput)
        {
            // Validate
            var result = HouseholdTaxDeductionManager.ValidateHouseholdTaxDeductionRow(invoiceRowInput);
            if (!result.Success)
                return result;

            var taxRow = new HouseholdTaxDeductionRow
            {
                ActorCompanyId = base.ActorCompanyId,
                Property = invoiceRowInput.ContainsKey("householdproperty") ? (string)invoiceRowInput["householdproperty"] : string.Empty,
                SocialSecNr = invoiceRowInput.ContainsKey("householdsocialsecnr") ? (string)invoiceRowInput["householdsocialsecnr"] : string.Empty,
                Name = invoiceRowInput.ContainsKey("householdname") ? (string)invoiceRowInput["householdname"] : string.Empty,
                Amount = invoiceRowInput.ContainsKey("householdamount") ? (decimal)invoiceRowInput["householdamount"] : 0,
                AmountCurrency = invoiceRowInput.ContainsKey("householdamountcurrency") ? (decimal)invoiceRowInput["householdamountcurrency"] : 0,
                ApartmentNr = invoiceRowInput.ContainsKey("householdapartmentnbr") ? (string)invoiceRowInput["householdapartmentnbr"] : string.Empty,
                CooperativeOrgNr = invoiceRowInput.ContainsKey("householdcooperativeorgnbr") ? (string)invoiceRowInput["householdcooperativeorgnbr"] : string.Empty,
                HouseHoldTaxDeductionType = invoiceRowInput.ContainsKey("householdtaxdeductiontype") ? (int)invoiceRowInput["householdtaxdeductiontype"] : (int)TermGroup_HouseHoldTaxDeductionType.None,
            };
            SetCreatedProperties(taxRow);

            taxRow.CustomerInvoiceRow = row;
            row.HouseholdTaxDeductionRow = taxRow;
            row.ProductRowType = (int)SoeProductRowType.HouseholdTaxDeduction;

            invoice.HasHouseholdTaxDeduction = true;

            return result;
        }

        public ActionResult CreateHouseholdTaxDeductionRow(CustomerInvoice invoice, CustomerInvoiceRow row, ProductRowDTO dto)
        {
            var result = HouseholdTaxDeductionManager.ValidateHouseholdTaxDeductionRow(dto);
            if (!result.Success)
                return result;

            var taxRow = new HouseholdTaxDeductionRow
            {
                ActorCompanyId = base.ActorCompanyId,
                Property = dto.HouseholdProperty.HasValue() ? dto.HouseholdProperty : string.Empty,
                SocialSecNr = dto.HouseholdSocialSecNbr,
                Name = dto.HouseholdName,
                Amount = dto.HouseholdAmount,
                AmountCurrency = dto.HouseholdAmountCurrency,
                ApartmentNr = dto.HouseholdApartmentNbr,
                CooperativeOrgNr = dto.HouseholdCooperativeOrgNbr,
                HouseHoldTaxDeductionType = (int)dto.HouseholdTaxDeductionType,
            };
            SetCreatedProperties(taxRow);

            taxRow.CustomerInvoiceRow = row;
            row.HouseholdTaxDeductionRow = taxRow;
            row.ProductRowType = (int)SoeProductRowType.HouseholdTaxDeduction;

            invoice.HasHouseholdTaxDeduction = true;

            return result;
        }

        public ActionResult CreateHouseholdTaxDeductionRow(CustomerInvoiceRowDTO dto, CustomerInvoiceRow row)
        {
            var result = HouseholdTaxDeductionManager.ValidateHouseholdTaxDeductionRow(dto);
            if (!result.Success)
            {
                return result;
            }

            var taxRow = new HouseholdTaxDeductionRow
            {
                ActorCompanyId = base.ActorCompanyId,
                Property = dto.HouseholdProperty,
                SocialSecNr = dto.HouseholdSocialSecNbr,
                Name = dto.HouseholdName,
                Amount = dto.HouseholdAmount,
                AmountCurrency = dto.HouseholdAmountCurrency,
                ApartmentNr = dto.HouseholdApartmentNbr,
                CooperativeOrgNr = dto.HouseholdCooperativeOrgNbr,
                Applied = dto.HouseholdApplied,
                AppliedDate = dto.HouseholdAppliedDate,
                Received = dto.HouseholdReceived,
                ReceivedDate = dto.HouseholdReceivedDate,
                HouseHoldTaxDeductionType = dto.HouseHoldTaxDeductionType
            };
            SetCreatedProperties(taxRow);

            taxRow.CustomerInvoiceRow = row;
            row.HouseholdTaxDeductionRow = taxRow;
            row.ProductRowType = (int)SoeProductRowType.HouseholdTaxDeduction;

            return result;
        }

        public void UpdateHouseholdTaxDeductionRow(CustomerInvoiceRow invoiceRow, IDictionary<string, object> invoiceRowInput)
        {
            var taxRow = invoiceRow.HouseholdTaxDeductionRow;

            if (taxRow != null)
            {
                if (invoiceRowInput.ContainsKey("householdproperty") && !String.IsNullOrEmpty((string)invoiceRowInput["householdproperty"]))
                    taxRow.Property = (string)invoiceRowInput["householdproperty"];

                if (invoiceRowInput.ContainsKey("householdsocialsecnbr") && !String.IsNullOrEmpty((string)invoiceRowInput["householdsocialsecnbr"]))
                    taxRow.SocialSecNr = (string)invoiceRowInput["householdsocialsecnbr"];

                if (invoiceRowInput.ContainsKey("householdname") && !String.IsNullOrEmpty((string)invoiceRowInput["householdname"]))
                    taxRow.Name = (string)invoiceRowInput["householdname"];

                if (invoiceRowInput.ContainsKey("householdapartmentnbr") && !String.IsNullOrEmpty((string)invoiceRowInput["householdapartmentnbr"]))
                    taxRow.ApartmentNr = (string)invoiceRowInput["householdapartmentnbr"];

                if (invoiceRowInput.ContainsKey("householdcooperativeorgnbr") && !String.IsNullOrEmpty((string)invoiceRowInput["householdcooperativeorgnbr"]))
                    taxRow.CooperativeOrgNr = (string)invoiceRowInput["householdcooperativeorgnbr"];

                var type = invoiceRowInput.ContainsKey("householdtaxdeductiontype") ? Convert.ToInt32(invoiceRowInput["householdtaxdeductiontype"]) : 0;
                if (type != 0 && type != taxRow.HouseHoldTaxDeductionType)
                    taxRow.HouseHoldTaxDeductionType = type;

                if ((invoiceRowInput.ContainsKey("householdamount") && invoiceRowInput["householdamount"] != null) || (invoiceRowInput.ContainsKey("householdamountcurrency") && invoiceRowInput["householdamountcurrency"] != null))
                {
                    taxRow.Amount = invoiceRowInput.ContainsKey("householdamount") ? Convert.ToDecimal(invoiceRowInput["householdamount"]) : 0;
                    taxRow.AmountCurrency = invoiceRowInput.ContainsKey("householdamountcurrency") ? Convert.ToDecimal(invoiceRowInput["householdamountcurrency"]) : 0;

                    SetModifiedProperties(taxRow);
                }
            }
        }

        public ActionResult UpdateHouseholdTaxDeductionRow(HouseholdTaxDeductionApplicantDTO item)
        {
            ActionResult result = new ActionResult(true);

            using (CompEntities entities = new CompEntities())
            {
                HouseholdTaxDeductionRow originalRow = GetHouseholdTaxDeductionRow(entities, item.HouseholdTaxDeductionApplicantId, false, false);
                if (originalRow == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "HouseholdTaxDeductionRow");

                originalRow.Property = item.Property;
                originalRow.Name = item.Name;
                originalRow.SocialSecNr = item.SocialSecNr;
                originalRow.CooperativeOrgNr = item.CooperativeOrgNr;
                originalRow.ApartmentNr = item.ApartmentNr;
                originalRow.Comment = item.Comment;

                SetModifiedProperties(originalRow);

                result = SaveChanges(entities);
            }

            return result;
        }

        #endregion

        #region File

        public List<HouseholdTaxDeductionFileRowDTO> GetHouseholdTaxDeductionFileForEdit(List<int> householdTaxDeductionRows)
        {
            var dtos = new List<HouseholdTaxDeductionFileRowDTO>();

            #region Prereq

            int defaultHouseholdDeductionType = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.BillingDefaultHouseholdDeductionType, 0, base.ActorCompanyId, 0);
            int householdTaxDeductionProductId = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.ProductHouseholdTaxDeduction, 0, base.ActorCompanyId, 0);
            int householdTaxDeduction50ProductId = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.ProductHousehold50TaxDeduction, 0, base.ActorCompanyId, 0);
            int householdRutTaxDeductionProductId = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.ProductRUTTaxDeduction, 0, base.ActorCompanyId, 0);
            int green15ProductId = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.ProductGreen15TaxDeduction, 0, base.ActorCompanyId, 0);
            int green20ProductId = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.ProductGreen20TaxDeduction, 0, base.ActorCompanyId, 0);
            int green50ProductId = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.ProductGreen50TaxDeduction, 0, base.ActorCompanyId, 0);

            bool IsNotDeductionProduct(int productId)
            {
                return productId != householdTaxDeductionProductId && productId != householdTaxDeduction50ProductId && productId != householdRutTaxDeductionProductId && productId != green15ProductId && productId != green20ProductId && productId != green50ProductId;
            }

            bool IsGreenTech(int productId)
            {
                return productId == green15ProductId || productId == green20ProductId || productId == green50ProductId;
            }

            List<SysHouseholdType> householdDeductionTypes = ProductManager.GetSysHouseholdType(false);

            #endregion

            using (CompEntities entities = new CompEntities())
            {
                #region Content

                foreach (var customerInvoiceRowId in householdTaxDeductionRows)
                {
                    #region Row

                    var row = HouseholdTaxDeductionManager.GetHouseholdTaxDeductionRowForFile(entities, customerInvoiceRowId);
                    if (row == null)
                        continue;

                    var houseHoldDeductionRowsForThisInvoice = HouseholdTaxDeductionManager.GetHouseholdTaxDeductionRowsPerInvoiceDict(entities, row.InvoiceId);

                    #region Calculate Amounts and create type tags

                    List<HouseholdTaxDeductionFileRowTypeDTO> typesList = new List<HouseholdTaxDeductionFileRowTypeDTO>();
                    List<int> householdDeductionTypeIds = householdDeductionTypes.Where(t => t.SysHouseholdTypeClassification == row.HouseHoldTaxDeductionType).Select(t => t.SysHouseholdTypeId).ToList();
                    int nonValidTypeId = householdDeductionTypes.FirstOrDefault(t => t.SysHouseholdTypeClassification == 0)?.SysHouseholdTypeId ?? 0;

                    foreach (SysHouseholdType hht in householdDeductionTypes.Where(t => t.SysHouseholdTypeClassification == row.HouseHoldTaxDeductionType))
                    {
                        typesList.Add(new HouseholdTaxDeductionFileRowTypeDTO() { SysHouseholdTypeId = hht.SysHouseholdTypeId, Amount = Decimal.Zero, Hours = Decimal.Zero, Text = hht.Name, XMLTag = hht.XMLTagName });
                    }

                    #region Calculate Amounts

                    //Calculate Amounts
                    decimal amountTotal = Decimal.Zero;
                    decimal amountTotalNonValid = Decimal.Zero;

                    var customerInvoiceProductRows = InvoiceManager.GetCustomerInvoiceRowsForOrderInvoiceEdit(entities, row.InvoiceId, false, true, false).Where(i => i.Type == (int)SoeInvoiceRowType.ProductRow && !i.IsHouseholdTaxDeductionRow()).ToList();
                    if (!customerInvoiceProductRows.Any())
                        continue;

                    bool noType = !customerInvoiceProductRows.Any(r => r.HouseholdDeductionType.HasValue);
                    foreach (CustomerInvoiceRow customerInvoiceRow in customerInvoiceProductRows)
                    {
                        //Check that Product not is HouseholdTaxDeduction 
                        if (customerInvoiceRow.ProductId.HasValue && IsNotDeductionProduct(customerInvoiceRow.ProductId.Value))
                        {
                            //Check that Product has VatType Service
                            var invoiceProduct = customerInvoiceRow.Product as InvoiceProduct;
                            if (invoiceProduct != null)
                            {
                                if (invoiceProduct.VatType == (int)TermGroup_InvoiceProductVatType.Service || IsGreenTech(row.ProductId.Value))
                                {
                                    if (noType || (customerInvoiceRow.HouseholdDeductionType.HasValue && householdDeductionTypeIds.Contains((int)customerInvoiceRow.HouseholdDeductionType)))
                                    {
                                        //Accumulate Amount and VatAmount
                                        amountTotal += customerInvoiceRow.SumAmount > 0 ? customerInvoiceRow.SumAmount : Decimal.Negate(customerInvoiceRow.SumAmount);
                                        amountTotal += customerInvoiceRow.VatAmount > 0 ? customerInvoiceRow.VatAmount : Decimal.Negate(customerInvoiceRow.VatAmount);

                                        if (!noType)
                                        {
                                            var typeRow = typesList.FirstOrDefault(g => g.SysHouseholdTypeId == (int)customerInvoiceRow.HouseholdDeductionType);
                                            if (typeRow != null)
                                            {
                                                if (!customerInvoiceRow.ProductUnitReference.IsLoaded)
                                                    customerInvoiceRow.ProductUnitReference.Load();

                                                if (IsGreenTech(row.ProductId.Value))
                                                {
                                                    if (customerInvoiceRow.ProductUnit != null)
                                                    {
                                                        if (customerInvoiceRow.ProductUnit.Code.ToLower() == "min" && customerInvoiceRow.Quantity.HasValue)
                                                            typeRow.Hours += (customerInvoiceRow.Quantity.Value / 60);
                                                        else if (customerInvoiceRow.ProductUnit.Code.ToLower() == "tim" && customerInvoiceRow.Quantity.HasValue)
                                                            typeRow.Hours += customerInvoiceRow.Quantity.Value;
                                                        else if (customerInvoiceRow.ProductUnit.Code.ToLower() == "timmar" && customerInvoiceRow.Quantity.HasValue)
                                                            typeRow.Hours += customerInvoiceRow.Quantity.Value;
                                                        typeRow.Amount += customerInvoiceRow.SumAmountCurrency + customerInvoiceRow.VatAmountCurrency;
                                                    }
                                                    else
                                                    {
                                                        if (!invoiceProduct.ProductUnitReference.IsLoaded)
                                                            invoiceProduct.ProductUnitReference.Load();

                                                        if (invoiceProduct.ProductUnit != null && invoiceProduct.ProductUnit.Code.ToLower() == "min" && customerInvoiceRow.Quantity.HasValue)
                                                            typeRow.Hours += (customerInvoiceRow.Quantity.Value / 60);
                                                        else if (invoiceProduct.ProductUnit != null && invoiceProduct.ProductUnit.Code.ToLower() == "tim" && customerInvoiceRow.Quantity.HasValue)
                                                            typeRow.Hours += customerInvoiceRow.Quantity.Value;
                                                        else if (invoiceProduct.ProductUnit != null && invoiceProduct.ProductUnit.Code.ToLower() == "timmar" && customerInvoiceRow.Quantity.HasValue)
                                                            typeRow.Hours += customerInvoiceRow.Quantity.Value;
                                                        typeRow.Amount += customerInvoiceRow.SumAmountCurrency + customerInvoiceRow.VatAmountCurrency;
                                                    }
                                                }
                                                else
                                                {
                                                    if (customerInvoiceRow.ProductUnit != null)
                                                    {
                                                        if (customerInvoiceRow.ProductUnit.Code.ToLower() == "min" && customerInvoiceRow.Quantity.HasValue)
                                                            typeRow.Hours += (customerInvoiceRow.Quantity.Value / 60);
                                                        else if (customerInvoiceRow.ProductUnit.Code.ToLower() == "tim" && customerInvoiceRow.Quantity.HasValue)
                                                            typeRow.Hours += customerInvoiceRow.Quantity.Value;
                                                        else if (customerInvoiceRow.ProductUnit.Code.ToLower() == "timmar" && customerInvoiceRow.Quantity.HasValue)
                                                            typeRow.Hours += customerInvoiceRow.Quantity.Value;
                                                        else
                                                            typeRow.Amount += customerInvoiceRow.SumAmountCurrency + customerInvoiceRow.VatAmountCurrency;
                                                    }
                                                    else
                                                    {
                                                        if (!invoiceProduct.ProductUnitReference.IsLoaded)
                                                            invoiceProduct.ProductUnitReference.Load();

                                                        if (invoiceProduct.ProductUnit != null && invoiceProduct.ProductUnit.Code.ToLower() == "min" && customerInvoiceRow.Quantity.HasValue)
                                                            typeRow.Hours += (customerInvoiceRow.Quantity.Value / 60);
                                                        else if (invoiceProduct.ProductUnit != null && invoiceProduct.ProductUnit.Code.ToLower() == "tim" && customerInvoiceRow.Quantity.HasValue)
                                                            typeRow.Hours += customerInvoiceRow.Quantity.Value;
                                                        else if (invoiceProduct.ProductUnit != null && invoiceProduct.ProductUnit.Code.ToLower() == "timmar" && customerInvoiceRow.Quantity.HasValue)
                                                            typeRow.Hours += customerInvoiceRow.Quantity.Value;
                                                        else
                                                            typeRow.Amount += customerInvoiceRow.SumAmountCurrency + customerInvoiceRow.VatAmountCurrency;
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            HouseholdTaxDeductionFileRowTypeDTO typeRow = null;
                                            if (invoiceProduct.HouseholdDeductionType.HasValue)
                                                typeRow = typesList.FirstOrDefault(g => g.SysHouseholdTypeId == (int)invoiceProduct.HouseholdDeductionType);
                                            else
                                                typeRow = typesList.FirstOrDefault(g => g.SysHouseholdTypeId == defaultHouseholdDeductionType);

                                            if (typeRow != null)
                                            {
                                                if (!customerInvoiceRow.ProductUnitReference.IsLoaded)
                                                    customerInvoiceRow.ProductUnitReference.Load();

                                                if (IsGreenTech(row.ProductId.Value))
                                                {
                                                    if (customerInvoiceRow.ProductUnit != null)
                                                    {
                                                        if (customerInvoiceRow.ProductUnit.Code.ToLower() == "min" && customerInvoiceRow.Quantity.HasValue)
                                                            typeRow.Hours += (customerInvoiceRow.Quantity.Value / 60);
                                                        else if (customerInvoiceRow.ProductUnit.Code.ToLower() == "tim" && customerInvoiceRow.Quantity.HasValue)
                                                            typeRow.Hours += customerInvoiceRow.Quantity.Value;
                                                        else if (customerInvoiceRow.ProductUnit.Code.ToLower() == "timmar" && customerInvoiceRow.Quantity.HasValue)
                                                            typeRow.Hours += customerInvoiceRow.Quantity.Value;
                                                        typeRow.Amount += customerInvoiceRow.SumAmountCurrency + customerInvoiceRow.VatAmountCurrency;
                                                    }
                                                    else
                                                    {
                                                        if (!invoiceProduct.ProductUnitReference.IsLoaded)
                                                            invoiceProduct.ProductUnitReference.Load();

                                                        if (invoiceProduct.ProductUnit != null && invoiceProduct.ProductUnit.Code.ToLower() == "min" && customerInvoiceRow.Quantity.HasValue)
                                                            typeRow.Hours += (customerInvoiceRow.Quantity.Value / 60);
                                                        else if (invoiceProduct.ProductUnit != null && invoiceProduct.ProductUnit.Code.ToLower() == "tim" && customerInvoiceRow.Quantity.HasValue)
                                                            typeRow.Hours += customerInvoiceRow.Quantity.Value;
                                                        else if (invoiceProduct.ProductUnit != null && invoiceProduct.ProductUnit.Code.ToLower() == "timmar" && customerInvoiceRow.Quantity.HasValue)
                                                            typeRow.Hours += customerInvoiceRow.Quantity.Value;
                                                        typeRow.Amount += customerInvoiceRow.SumAmountCurrency + customerInvoiceRow.VatAmountCurrency;
                                                    }
                                                }
                                                else
                                                {
                                                    if (customerInvoiceRow.ProductUnit != null)
                                                    {
                                                        if (customerInvoiceRow.ProductUnit.Code.ToLower() == "min" && customerInvoiceRow.Quantity.HasValue)
                                                            typeRow.Hours += (customerInvoiceRow.Quantity.Value / 60);
                                                        else if (customerInvoiceRow.ProductUnit.Code.ToLower() == "tim" && customerInvoiceRow.Quantity.HasValue)
                                                            typeRow.Hours += customerInvoiceRow.Quantity.Value;
                                                        else if (customerInvoiceRow.ProductUnit.Code.ToLower() == "timmar" && customerInvoiceRow.Quantity.HasValue)
                                                            typeRow.Hours += customerInvoiceRow.Quantity.Value;
                                                        else
                                                            typeRow.Amount += customerInvoiceRow.SumAmountCurrency + customerInvoiceRow.VatAmountCurrency;
                                                    }
                                                    else
                                                    {
                                                        if (!invoiceProduct.ProductUnitReference.IsLoaded)
                                                            invoiceProduct.ProductUnitReference.Load();

                                                        if (invoiceProduct.ProductUnit != null && invoiceProduct.ProductUnit.Code.ToLower() == "min" && customerInvoiceRow.Quantity.HasValue)
                                                            typeRow.Hours += (customerInvoiceRow.Quantity.Value / 60);
                                                        else if (invoiceProduct.ProductUnit != null && invoiceProduct.ProductUnit.Code.ToLower() == "tim" && customerInvoiceRow.Quantity.HasValue)
                                                            typeRow.Hours += customerInvoiceRow.Quantity.Value;
                                                        else if (invoiceProduct.ProductUnit != null && invoiceProduct.ProductUnit.Code.ToLower() == "timmar" && customerInvoiceRow.Quantity.HasValue)
                                                            typeRow.Hours += customerInvoiceRow.Quantity.Value;
                                                        else
                                                            typeRow.Amount += customerInvoiceRow.SumAmountCurrency + customerInvoiceRow.VatAmountCurrency;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    else if (customerInvoiceRow.HouseholdDeductionType.HasValue && customerInvoiceRow.HouseholdDeductionType == nonValidTypeId)
                                    {
                                        amountTotalNonValid += customerInvoiceRow.SumAmount > 0 ? customerInvoiceRow.SumAmount : Decimal.Negate(customerInvoiceRow.SumAmount);
                                        amountTotalNonValid += customerInvoiceRow.VatAmount > 0 ? customerInvoiceRow.VatAmount : Decimal.Negate(customerInvoiceRow.VatAmount);
                                    }
                                }
                                else if (invoiceProduct.VatType == (int)TermGroup_InvoiceProductVatType.Merchandise)
                                {
                                    if (customerInvoiceRow.HouseholdDeductionType.HasValue && householdDeductionTypeIds.Contains((int)customerInvoiceRow.HouseholdDeductionType))
                                    {
                                        var typeRow = typesList.FirstOrDefault(g => g.SysHouseholdTypeId == (int)customerInvoiceRow.HouseholdDeductionType);
                                        if (typeRow != null)
                                        {
                                            typeRow.Amount += customerInvoiceRow.SumAmountCurrency + customerInvoiceRow.VatAmountCurrency;
                                        }
                                    }
                                    else if (customerInvoiceRow.HouseholdDeductionType.HasValue && customerInvoiceRow.HouseholdDeductionType == nonValidTypeId)
                                    {
                                        amountTotalNonValid += customerInvoiceRow.SumAmount > 0 ? customerInvoiceRow.SumAmount : Decimal.Negate(customerInvoiceRow.SumAmount);
                                        amountTotalNonValid += customerInvoiceRow.VatAmount > 0 ? customerInvoiceRow.VatAmount : Decimal.Negate(customerInvoiceRow.VatAmount);
                                    }
                                    else if (noType)
                                    {
                                        var typeRow = typesList.FirstOrDefault(g => g.SysHouseholdTypeId == defaultHouseholdDeductionType);

                                        if (typeRow != null)
                                        {
                                            typeRow.Amount += customerInvoiceRow.SumAmountCurrency + customerInvoiceRow.VatAmountCurrency;
                                        }
                                    }
                                }
                                else if (invoiceProduct.VatType == (int)TermGroup_InvoiceProductVatType.None && customerInvoiceRow.HouseholdDeductionType.HasValue && customerInvoiceRow.HouseholdDeductionType == nonValidTypeId)
                                {
                                    amountTotalNonValid += customerInvoiceRow.SumAmount > 0 ? customerInvoiceRow.SumAmount : Decimal.Negate(customerInvoiceRow.SumAmount);
                                    amountTotalNonValid += customerInvoiceRow.VatAmount > 0 ? customerInvoiceRow.VatAmount : Decimal.Negate(customerInvoiceRow.VatAmount);
                                }
                            }
                        }
                    }

                    #endregion

                    #endregion

                    decimal amountRequest = row.Amount;
                    if (amountRequest == 0)
                        continue;

                    decimal amountPayed = houseHoldDeductionRowsForThisInvoice.Count > 1 ? amountTotal - houseHoldDeductionRowsForThisInvoice.Sum(r => r.Field2) : amountTotal - amountRequest;
                    DateTime? paymentDate = PaymentManager.GetLastPaymentDateForInvoice(entities, row.InvoiceId);

                    string socialSecNr = row.SocialSecNr;

                    //Rensa bort sånt som inte ska vara med enligt skatteverkets mall
                    socialSecNr = socialSecNr.Replace("-", "");
                    socialSecNr = socialSecNr.Replace(" ", "");

                    //De vill ha tolv siffror, så vi får lägga till om det saknas "19" först
                    if (socialSecNr.Length == 10)
                        socialSecNr = "19" + socialSecNr;

                    // Create row for section
                    var dto = new HouseholdTaxDeductionFileRowDTO();
                    dto.CustomerInvoiceRowId = row.CustomerInvoiceRowId;
                    dto.InvoiceNr = row.InvoiceNr;
                    dto.Name = row.Name;
                    dto.SocialSecNr = socialSecNr;
                    dto.Property = row.Property ?? string.Empty;
                    dto.ApartmentNr = row.ApartmentNr ?? string.Empty;
                    dto.CooperativeOrgNr = row.CooperativeOrgNr ?? string.Empty;
                    dto.PaidDate = paymentDate;
                    dto.InvoiceTotalAmount = row.TotalAmountCurrency;
                    dto.WorkAmount = Math.Round(amountTotal);
                    dto.PaidAmount = Math.Round(amountPayed);
                    dto.AppliedAmount = Math.Round(amountRequest);
                    dto.NonValidAmount = Math.Round(amountTotalNonValid);
                    dto.Comment = row.Comment;

                    dto.Types = typesList;

                    dtos.Add(dto);

                    #endregion
                }

                #endregion
            }
                
            return dtos;
        }

        public string GetHouseholdTaxDeductionFile(List<HouseholdTaxDeductionFileRowDTO> applications, int seqNr, TermGroup_HouseHoldTaxDeductionType type)
        {
            XDocument document = null;
            if(type == TermGroup_HouseHoldTaxDeductionType.GREEN)
                document = GetHouseholdTaxDeductionFileGreenTech(applications, seqNr);
            else
                document = GetHouseholdTaxDeductionFile(applications, seqNr, type == TermGroup_HouseHoldTaxDeductionType.RUT);

            // Update latest used sequence number
            if (type == TermGroup_HouseHoldTaxDeductionType.RUT)
                SequenceNumberManager.UpdateSequenceNumber(base.ActorCompanyId, "RutTaxDeduction", seqNr);
            else if (type == TermGroup_HouseHoldTaxDeductionType.GREEN)
                SequenceNumberManager.UpdateSequenceNumber(base.ActorCompanyId, "GreenTaxDeduction", seqNr);
            else
                SequenceNumberManager.UpdateSequenceNumber(base.ActorCompanyId, "HouseholdTaxDeduction", seqNr);

            //Convert XDocument to stream
            var stream = new MemoryStream();
            document.Save(stream);

            return GeneralManager.GetUrlForDownload(stream.ToArray(), "skattereduktionsfil.xml");
        }

        public XDocument GetHouseholdTaxDeductionFile(List<HouseholdTaxDeductionFileRowDTO> applications, int seqNr, bool isRut)
        {
            #region Prereq

            bool useZerosInTaxDeductionFile = SettingManager.GetBoolSetting(SettingMainType.Application, (int)ApplicationSettingType.UseZerosInTaxDeductionFile, 0, 0, 0, false);

            #endregion

            #region Init document

            const string schema_location = "http://xmls.skatteverket.se/se/skatteverket/ht/begaran/6.0 Begaran.xsd";

            XNamespace xsi = XNamespace.Get("http://www.w3.org/2001/XMLSchema-instance");
            XNamespace defaultNamespace = XNamespace.Get("http://xmls.skatteverket.se/se/skatteverket/ht/begaran/6.0");
            XNamespace htko = XNamespace.Get("http://xmls.skatteverket.se/se/skatteverket/ht/komponent/begaran/6.0");
            XElement rootElement = new XElement(
               new XElement(defaultNamespace + "Begaran",
               new XAttribute(XNamespace.Xmlns + "n1", defaultNamespace),
               new XAttribute(XNamespace.Xmlns + "xsi", xsi.NamespaceName),
               new XAttribute(xsi + "schemaLocation", schema_location),
               new XAttribute(XNamespace.Xmlns + "htko", htko)));

            rootElement.Add(new XElement(htko + "NamnPaBegaran", seqNr));

            XElement householdTaxDeductionElement = isRut ? householdTaxDeductionElement = new XElement(htko + "HushallBegaran") : householdTaxDeductionElement = new XElement(htko + "RotBegaran");

            #endregion

            using (CompEntities entities = new CompEntities())
            {

                foreach (var application in applications)
                {
                    HouseholdTaxDeductionRow row = HouseholdTaxDeductionManager.GetHouseholdTaxDeductionRow(entities, application.CustomerInvoiceRowId, true, true);
                    if (row == null || row.CustomerInvoiceRow == null || row.CustomerInvoiceRow.CustomerInvoice == null)
                        continue;

                    XElement housholdTaxDeductionRow = new XElement(htko + "Arenden");

                    if (isRut)
                    {
                        housholdTaxDeductionRow.Add(
                            new XElement(htko + "Kopare", application.SocialSecNr),
                            new XElement(htko + "BetalningsDatum", application.PaidDate.HasValue ? application.PaidDate.Value.ToShortDateString() : CalendarUtility.DATETIME_DEFAULT.ToShortDateString()),
                            new XElement(htko + "PrisForArbete", Math.Round(application.WorkAmount)),
                            new XElement(htko + "BetaltBelopp", Math.Round(application.PaidAmount)),
                            new XElement(htko + "BegartBelopp", Math.Round(application.AppliedAmount)),
                            new XElement(htko + "FakturaNr", application.InvoiceNr),
                            new XElement(htko + "Ovrigkostnad", (int)Math.Round(application.NonValidAmount)));
                    }
                    else
                    {
                        housholdTaxDeductionRow.Add(
                            new XElement(htko + "Kopare", application.SocialSecNr),
                            new XElement(htko + "BetalningsDatum", application.PaidDate.HasValue ? application.PaidDate.Value.ToShortDateString() : CalendarUtility.DATETIME_DEFAULT.ToShortDateString()),
                            new XElement(htko + "PrisForArbete", Math.Round(application.WorkAmount)),
                            new XElement(htko + "BetaltBelopp", Math.Round(application.PaidAmount)),
                            new XElement(htko + "BegartBelopp", Math.Round(application.AppliedAmount)),
                            new XElement(htko + "FakturaNr", application.InvoiceNr),
                            new XElement(htko + "Ovrigkostnad", (int)Math.Round(application.NonValidAmount)));

                        if (application.CooperativeOrgNr != null && application.CooperativeOrgNr.Trim() != string.Empty)
                        {
                            housholdTaxDeductionRow.Add(
                                    new XElement(htko + "LagenhetsNr", application.ApartmentNr),
                                    new XElement(htko + "BrfOrgNr", application.CooperativeOrgNr));
                        }
                        else
                        {
                            housholdTaxDeductionRow.Add(new XElement(htko + "Fastighetsbeteckning", application.Property.Length > 40 ? application.Property.Substring(0, 40) : application.Property));
                        }
                    }

                    XElement workRow = new XElement(htko + "UtfortArbete");

                    foreach (var type in application.Types)
                    {
                        XElement typeRow = new XElement(htko + type.XMLTag);
                        if (type.XMLTag == "TransportTillForsaljning" || type.XMLTag == "TvattVidTvattinrattning")
                        {
                            if (type.Hours != 0)
                            {
                                typeRow.Add(new XElement(htko + "Utfort", 1));
                            }
                            else
                            {
                                typeRow.Add(new XElement(htko + "Utfort", 0));
                            }
                        }
                        else
                        {
                            if (useZerosInTaxDeductionFile)
                            {
                                typeRow.Add(new XElement(htko + "AntalTimmar", ((int)Math.Ceiling(type.Hours)).ToString()),
                                new XElement(htko + "Materialkostnad", ((int)Math.Ceiling(type.Amount)).ToString()));
                            }
                            else
                            {
                                typeRow.Add(new XElement(htko + "AntalTimmar", type.Hours > 0 ? ((int)Math.Ceiling(type.Hours)).ToString() : ""),
                                new XElement(htko + "Materialkostnad", type.Amount > 0 ? ((int)Math.Ceiling(type.Amount)).ToString() : "" )); 
                            }
                            
                        }

                        workRow.Add(typeRow);
                    }

                    housholdTaxDeductionRow.Add(workRow);
                    householdTaxDeductionElement.Add(housholdTaxDeductionRow);

                    row.SeqNr = seqNr;
                }

                // Save row updates
                entities.SaveChanges();
            }

            #region Close document

            rootElement.Add(householdTaxDeductionElement);

            XDocument document = XmlUtil.CreateDocument(Encoding.UTF8, true);
            document.Add(rootElement);
            return document;

            #endregion
        }

        public XDocument GetHouseholdTaxDeductionFileGreenTech(List<HouseholdTaxDeductionFileRowDTO> applications, int seqNr)
        {
            #region Prereq

            int green15ProductId = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.ProductGreen15TaxDeduction, 0, base.ActorCompanyId, 0);
            int green20ProductId = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.ProductGreen20TaxDeduction, 0, base.ActorCompanyId, 0);
            int green50ProductId = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.ProductGreen50TaxDeduction, 0, base.ActorCompanyId, 0);

            #endregion

            #region Init document

            XNamespace xsi = XNamespace.Get("http://www.w3.org/2001/XMLSchema-instance");
            XNamespace defaultNamespace = XNamespace.Get("http://xmls.skatteverket.se/se/skatteverket/skattered/begaran/1.0");
            XNamespace p = XNamespace.Get("http://xmls.skatteverket.se/se/skatteverket/skattered/begaran/1.0");
            XElement rootElement = new XElement(
               new XElement(defaultNamespace + "Begaran",
               new XAttribute(XNamespace.Xmlns + "p", defaultNamespace),
               new XAttribute(XNamespace.Xmlns + "xsi", xsi.NamespaceName)));

            rootElement.Add(new XElement(p + "NamnPaBegaran", seqNr));
            rootElement.Add(new XElement(p + "TypAvBegaran", "GRON_TEKNIK"));

            Company receiverCompany = CompanyManager.GetCompany(base.ActorCompanyId);
            if (receiverCompany == null)
                return null;

            // Handle org nr
            string recieverOrgNr = receiverCompany.OrgNr;
            recieverOrgNr = recieverOrgNr != null ? recieverOrgNr.Replace("-", "") : String.Empty;
            recieverOrgNr = recieverOrgNr.Replace(" ", "");
            if (recieverOrgNr.Length == 10)
                recieverOrgNr = "16" + recieverOrgNr;

            rootElement.Add(new XElement(p + "Utforare", recieverOrgNr));

            #endregion

            using (CompEntities entities = new CompEntities())
            {
                foreach (var application in applications)
                {
                    HouseholdTaxDeductionRow row = HouseholdTaxDeductionManager.GetHouseholdTaxDeductionRow(entities, application.CustomerInvoiceRowId, true, true);
                    if (row == null || row.CustomerInvoiceRow == null || row.CustomerInvoiceRow.CustomerInvoice == null)
                        continue;

                    XElement housholdTaxDeductionRow = new XElement(p + "Arende");
                    housholdTaxDeductionRow.Add(new XElement(p + "FakturaNr", application.InvoiceNr));
                    housholdTaxDeductionRow.Add(new XElement(p + "Kopare", application.SocialSecNr));

                    // Add object info
                    XElement objectElement = new XElement(p + "Fastighet");
                    if (application.CooperativeOrgNr != null && application.CooperativeOrgNr.Trim() != String.Empty)
                    {
                        objectElement.Add(
                                new XElement(p + "LagenhetsNr", application.ApartmentNr),
                                new XElement(p + "BrfOrgNr", application.CooperativeOrgNr));
                    }
                    else
                    {
                        objectElement.Add(new XElement(p + "Fastighetsbeteckning", application.Property.Length > 40 ? application.Property.Substring(0, 40) : application.Property));
                    }
                    housholdTaxDeductionRow.Add(objectElement);

                    HouseholdTaxDeductionFileRowTypeDTO type = null;
                    if (row.CustomerInvoiceRow.ProductId == green15ProductId || row.CustomerInvoiceRow.ProductId == green20ProductId)
                    {
                        // solar cells
                        type = application.Types.FirstOrDefault(t => t.SysHouseholdTypeId == 20);
                    }
                    else
                    {
                        // storage or charging point
                        var hhtStore = application.Types.FirstOrDefault(t => t.SysHouseholdTypeId == 21);
                        var hhtCharge = application.Types.FirstOrDefault(t => t.SysHouseholdTypeId == 22);
                        if (Math.Ceiling(hhtStore.Hours) > 0 && Math.Ceiling(hhtStore.Amount) > 0 && Math.Ceiling(hhtCharge.Hours) > 0 && Math.Ceiling(hhtCharge.Amount) > 0)
                        {
                            // Both storage and charge eligable product rows
                            if (row.CustomerInvoiceRow.HouseholdDeductionType == 21)
                                type = hhtStore;
                            else if (row.CustomerInvoiceRow.HouseholdDeductionType == 22)
                                type = hhtCharge;
                        }
                        else if (Math.Ceiling(hhtStore.Hours) > 0 && Math.Ceiling(hhtStore.Amount) > 0)
                        {
                            // Storage
                            type = hhtStore;
                        }
                        else if (Math.Ceiling(hhtCharge.Hours) > 0 && Math.Ceiling(hhtCharge.Amount) > 0)
                        {
                            // Charge
                            type = hhtCharge;
                        }
                    }

                    if (type != null)
                    {
                        if (Math.Ceiling(type.Hours) == 0 || Math.Ceiling(type.Amount) == 0)
                            continue;

                        string typeStr = String.Empty;
                        switch (type.SysHouseholdTypeId)
                        {
                            case 20:
                                typeStr = "INSTALLATION_SOLCELLER";
                                break;
                            case 21:
                                typeStr = "INSTALLATION_LAGRING";
                                break;
                            case 22:
                                typeStr = "INSTALLATION_LADDPUNKT";
                                break;
                        }

                        XElement workElement = new XElement(p + "UtfortArbete");

                        workElement.Add(
                                    new XElement(p + "TypAvUtfortArbete", typeStr),
                                    new XElement(p + "AntalTimmar", ((int)Math.Ceiling(type.Hours)).ToString()),
                                    new XElement(p + "Kostnad", ((int)Math.Ceiling(type.Amount)).ToString()));

                        housholdTaxDeductionRow.Add(workElement);
                    }

                    housholdTaxDeductionRow.Add(new XElement(p + "OvrigKostnad", (int)Math.Round(application.NonValidAmount)));
                    housholdTaxDeductionRow.Add(new XElement(p + "Betalningsdatum", application.PaidDate.HasValue ? application.PaidDate.ToShortDateString() : CalendarUtility.DATETIME_DEFAULT.ToShortDateString()));
                    housholdTaxDeductionRow.Add(new XElement(p + "BetaltBelopp", Math.Round(application.PaidAmount)));
                    housholdTaxDeductionRow.Add(new XElement(p + "BegartBelopp", Math.Round(application.AppliedAmount)));

                    rootElement.Add(housholdTaxDeductionRow);

                    row.SeqNr = seqNr;
                }

                // Save row updates
                entities.SaveChanges();
            }

            #region Close document

            XDocument document = XmlUtil.CreateDocument(Encoding.UTF8, true);
            document.Add(rootElement);
            return document;

            #endregion
        }

        #endregion

        #region Help-methods

        /*
        private List<HouseholdTaxDeductionGridView> GetHouseholdTaxDeductionGridViews(int actorCompanyId, SoeHouseholdClassificationGroup classificationGroup, TermGroup_HouseHoldTaxDeductionType taxDeductionType)
        {
            using var entities = CompEntitiesFactory.CreateReadOnly();
            entities.HouseholdTaxDeductionGridView.NoTracking();
            var query = (from h in entities.HouseholdTaxDeductionGridView
                         where h.ActorCompanyId == actorCompanyId &&
                         h.OriginStatus != (int)SoeOriginStatus.Draft &&
                         h.OriginStatus != (int)SoeOriginStatus.Cancel &&
                         h.RowState == (int)SoeEntityState.Active &&
                         h.State == (int)SoeEntityState.Active
                         select h);

            if (classificationGroup != SoeHouseholdClassificationGroup.All)
                query = query.Where(h => h.FullyPayed);

            switch (classificationGroup)
            {
                case SoeHouseholdClassificationGroup.Apply:
                    query = query.Where(h => !h.Applied);
                    break;
                case SoeHouseholdClassificationGroup.Applied:
                    query = query.Where(h => h.Applied && !h.Received && !h.Denied);
                    break;
                case SoeHouseholdClassificationGroup.Received:
                    query = query.Where(h => h.Received);
                    break;
                case SoeHouseholdClassificationGroup.Denied:
                    query = query.Where(h => h.Denied);
                    break;
            }


            if (taxDeductionType != TermGroup_HouseHoldTaxDeductionType.None && classificationGroup != SoeHouseholdClassificationGroup.All)
            {
                query = query.Where(h => h.HouseHoldTaxDeductionType == (int)taxDeductionType);
            }

            return query.ToList();
        }
        */

        private List<HouseholdTaxDeductionGridViewDTO> GetHouseholdTaxDeductionDTO(int actorCompanyId, SoeHouseholdClassificationGroup classificationGroup, TermGroup_HouseHoldTaxDeductionType taxDeductionType)
        {
            using var entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();

            var query = (from h in entitiesReadOnly.HouseholdTaxDeductionRow
                         where h.ActorCompanyId == actorCompanyId &&
                               h.State == (int)SoeEntityState.Active &&
                               h.CustomerInvoiceRow.CustomerInvoice.Origin.Status != (int)SoeOriginStatus.Draft &&
                               h.CustomerInvoiceRow.CustomerInvoice.Origin.Status != (int)SoeOriginStatus.Cancel &&
                               h.CustomerInvoiceRow.CustomerInvoice.RegistrationType == (int)OrderInvoiceRegistrationType.Invoice &&
                               h.CustomerInvoiceRow.State == (int)SoeEntityState.Active
                         select h);

            if (classificationGroup != SoeHouseholdClassificationGroup.All)
                query = query.Where(h => h.CustomerInvoiceRow.CustomerInvoice.FullyPayed);

            switch (classificationGroup)
            {
                case SoeHouseholdClassificationGroup.Apply:
                    query = query.Where(h => !h.Applied);
                    break;
                case SoeHouseholdClassificationGroup.Applied:
                    query = query.Where(h => h.Applied && !h.Received && !h.Denied);
                    break;
                case SoeHouseholdClassificationGroup.Received:
                    query = query.Where(h => h.Received);
                    break;
                case SoeHouseholdClassificationGroup.Denied:
                    query = query.Where(h => h.Denied);
                    break;
            }

            if (taxDeductionType != TermGroup_HouseHoldTaxDeductionType.None && classificationGroup != SoeHouseholdClassificationGroup.All)
            {
                query = query.Where(h => h.HouseHoldTaxDeductionType == (int)taxDeductionType);
            }

            var dtos = query.Select(h=> new HouseholdTaxDeductionGridViewDTO
            {
                Amount = h.AmountCurrency,
                CustomerInvoiceRowId = h.CustomerInvoiceRowId,
                VoucherHeadId = h.VoucherHeadId,
                OriginStatus = (SoeOriginStatus)h.CustomerInvoiceRow.CustomerInvoice.Origin.Status,
                BillingType = (TermGroup_BillingType)h.CustomerInvoiceRow.CustomerInvoice.BillingType,
                InvoiceId = h.CustomerInvoiceRow.InvoiceId,
                InvoiceNr = h.CustomerInvoiceRow.CustomerInvoice.InvoiceNr,
                Property = h.Property,
                SocialSecNr = h.SocialSecNr,
                Name = h.Name,
                ApprovedAmount = h.ApprovedAmount,
                PayDate = h.CustomerInvoiceRow.CustomerInvoice.PaymentRow.OrderByDescending(i => i.PayDate).FirstOrDefault().PayDate,
                FullyPayed = h.CustomerInvoiceRow.CustomerInvoice.FullyPayed,
                SeqNr = h.SeqNr,
                HouseholdStatus = "",
                Applied = h.Applied,
                AppliedDate = h.AppliedDate,
                Received = h.Received,
                ReceivedDate = h.ReceivedDate,
                Denied = h.Denied,
                DeniedDate = h.DeniedDate,
                HouseHoldTaxDeductionType = h.HouseHoldTaxDeductionType,
                ProductId = h.CustomerInvoiceRow.ProductId ?? 0,
                State = (SoeEntityState)h.State,
                VoucherNr = h.VoucherHead.VoucherNr.ToString() ?? "",
                Comment = h.Comment,
            }).ToList();

            foreach(var dto in dtos)
            {
                if (!dto.ApprovedAmount.HasValue)
                {
                    dto.ApprovedAmount = (dto.Received ? dto.Amount : (decimal?)null);
                }
            }

            return dtos;
        }

        #endregion
    }
}
