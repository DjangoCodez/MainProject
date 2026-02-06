using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Core
{
    public class AnalysisManager : ManagerBase
    {
        #region Variables

        #endregion

        #region Ctor

        public AnalysisManager(ParameterObject parameterObject) : base(parameterObject) { }

        #endregion

        #region StateAnalysis

        public List<StateAnalysisDTO> GetStateAnalysis(List<SoeStatesAnalysis> statesToAnalys, int actorCompanyId, int roleId)
        {
            List<StateAnalysisDTO> dtos = new List<StateAnalysisDTO>();
            List<EdiEntryViewDTO> ediItems = null;
            List<EdiEntryViewDTO> scanningItems = null;
            Dictionary<int, List<SupplierInvoiceGridDTO>> supplierInvoiceStatusItemsDict = null;
            Dictionary<int, List<SupplierPaymentGridDTO>> supplierPaymentStatusItemsDict = null;
            Dictionary<int, List<CustomerInvoiceGridDTO>> customerInvoicesStatusItemsDict = null;

            foreach (SoeStatesAnalysis state in statesToAnalys)
            {
                StateAnalysisDTO dto = new StateAnalysisDTO()
                {
                    State = state,
                };

                bool dtoIsValid = false;

                switch (state)
                {
                    #region General

                    case SoeStatesAnalysis.Role:
                        #region Role
                        {
                            using var entitiesRole = CompEntitiesProvider.LeaseReadOnlyContext();
                            entitiesRole.Role.NoTracking();
                            dto.NoOfItems = (from r in entitiesRole.Role
                                             where r.Company.ActorCompanyId == actorCompanyId &&
                                             r.State == (int)SoeEntityState.Active
                                             select r).Count();

                            dtoIsValid = true;
                        }
                        #endregion
                        break;
                    case SoeStatesAnalysis.User:
                        #region User
                        {
                            DateTime date = DateTime.Today;
                            using var entitiesUser = CompEntitiesProvider.LeaseReadOnlyContext();
                            entitiesUser.User.NoTracking();
                            dto.NoOfItems = (from ucr in entitiesUser.UserCompanyRole
                                             where ucr.Company.ActorCompanyId == actorCompanyId &&
                                            ucr.State == (int)SoeEntityState.Active &&
                                            (!ucr.DateFrom.HasValue || ucr.DateFrom <= date) &&
                                            (!ucr.DateTo.HasValue || ucr.DateTo >= date) &&
                                             ucr.User.State == (int)SoeEntityState.Active
                                             select ucr.User).Distinct().Count();

                            dtoIsValid = true;
                        }
                        #endregion
                        break;
                    case SoeStatesAnalysis.Employee:
                        #region Employee
                        {
                            using var entitiesEmployee = CompEntitiesProvider.LeaseReadOnlyContext();
                            entitiesEmployee.Employee.NoTracking();
                            dto.NoOfItems = (from e in entitiesEmployee.Employee
                                             where e.ActorCompanyId == actorCompanyId &&
                                             e.State == (int)SoeEntityState.Active
                                             select e).Count();

                            dtoIsValid = true;
                        }
                        #endregion
                        break;
                    case SoeStatesAnalysis.Customer:
                        #region Customer
                        {
                            using var entitiesCustomer = CompEntitiesProvider.LeaseReadOnlyContext();
                            entitiesCustomer.Customer.NoTracking();
                            dto.NoOfItems = (from c in entitiesCustomer.Customer
                                                                             where c.ActorCompanyId == actorCompanyId &&
                                                                             c.State == (int)SoeEntityState.Active
                                                                             select c).Count();

                            dtoIsValid = true;
                        }
                        #endregion
                        break;
                    case SoeStatesAnalysis.Supplier:
                        #region Supplier
                        {
                            using var entitiesSupplier = CompEntitiesProvider.LeaseReadOnlyContext();
                            entitiesSupplier.Supplier.NoTracking();
                            dto.NoOfItems = (from e in entitiesSupplier.Supplier
                                             where e.ActorCompanyId == actorCompanyId &&
                                             e.State == (int)SoeEntityState.Active
                                             select e).Count();

                            dtoIsValid = true;
                        }
                        #endregion
                        break;
                    case SoeStatesAnalysis.InvoiceProduct:
                        #region InvoiceProduct
                        {
                            using var entitiesInvoiceProduct = CompEntitiesProvider.LeaseReadOnlyContext();
                            entitiesInvoiceProduct.Product.NoTracking();
                            dto.NoOfItems = (from p in entitiesInvoiceProduct.Product
                                             where p.Company.Any(c => c.ActorCompanyId == actorCompanyId) &&
                                             p.State == (int)SoeEntityState.Active
                                             select p).OfType<InvoiceProduct>().Count();

                            dtoIsValid = true;
                        }
                        #endregion
                        break;
                    case SoeStatesAnalysis.InActiveTerminals:
                        #region InActiveTerminals
                        {
                            DateTime sync = DateTime.Now.AddMinutes(-15);
                            using var entitiesInActiveTerminals = CompEntitiesProvider.LeaseReadOnlyContext();
                            entitiesInActiveTerminals.TimeTerminal.NoTracking();
                            dto.NoOfItems = (from t in entitiesInActiveTerminals.TimeTerminal
                                             where t.ActorCompanyId == actorCompanyId &&
                                             t.State == (int)SoeEntityState.Active &&
                                             t.Registered &&
                                             t.LastSync < sync
                                             select t).OfType<TimeTerminal>().Count();
                            dtoIsValid = true;
                        }
                        #endregion
                        break;

                    #endregion

                    #region Billing

                    case SoeStatesAnalysis.Offer:
                        #region Offer

                        if (FeatureManager.HasRolePermission(Feature.Billing_Offer_Offers, Permission.Readonly, roleId, actorCompanyId))
                        {
                            LoadStatusItemsDictForCustomerInvoices(ref customerInvoicesStatusItemsDict, actorCompanyId);
                            dtoIsValid = GetStatusItemDTOFromCustomerInvoices(customerInvoicesStatusItemsDict, dtos, dto, SoeOriginStatusClassification.OffersOpen, true);
                        }
                        else
                        {
                            LoadStatusItemsDictForCustomerInvoices(ref customerInvoicesStatusItemsDict, actorCompanyId);
                            dtoIsValid = GetStatusItemDTOFromCustomerInvoices(customerInvoicesStatusItemsDict, dtos, dto, SoeOriginStatusClassification.OffersOpenUser, true);
                        }

                        #endregion
                        break;
                    case SoeStatesAnalysis.Contract:
                        #region Contract

                        LoadStatusItemsDictForCustomerInvoices(ref customerInvoicesStatusItemsDict, actorCompanyId);
                        dtoIsValid = GetStatusItemDTOFromCustomerInvoices(customerInvoicesStatusItemsDict, dtos, dto, SoeOriginStatusClassification.ContractsOpen, true);

                        #endregion
                        break;
                    case SoeStatesAnalysis.Order:
                        #region Order

                        if (FeatureManager.HasRolePermission(Feature.Billing_Order_Orders, Permission.Readonly, roleId, actorCompanyId))
                        {
                            LoadStatusItemsDictForCustomerInvoices(ref customerInvoicesStatusItemsDict, actorCompanyId);
                            dtoIsValid = GetStatusItemDTOFromCustomerInvoices(customerInvoicesStatusItemsDict, dtos, dto, SoeOriginStatusClassification.OrdersOpen, true);
                        }
                        else
                        {
                            LoadStatusItemsDictForCustomerInvoices(ref customerInvoicesStatusItemsDict, actorCompanyId);
                            dtoIsValid = GetStatusItemDTOFromCustomerInvoices(customerInvoicesStatusItemsDict, dtos, dto, SoeOriginStatusClassification.OrdersOpenUser, true);
                        }

                        #endregion
                        break;
                    case SoeStatesAnalysis.Invoice:
                        #region Invoice

                        if (FeatureManager.HasRolePermission(Feature.Billing_Invoice_Invoices, Permission.Readonly, roleId, actorCompanyId))
                        {
                            LoadStatusItemsDictForCustomerInvoices(ref customerInvoicesStatusItemsDict, actorCompanyId);
                            dtoIsValid = GetStatusItemDTOFromCustomerInvoices(customerInvoicesStatusItemsDict, dtos, dto, SoeOriginStatusClassification.CustomerInvoicesOpen, true);
                        }
                        else
                        {
                            LoadStatusItemsDictForCustomerInvoices(ref customerInvoicesStatusItemsDict, actorCompanyId);
                            dtoIsValid = GetStatusItemDTOFromCustomerInvoices(customerInvoicesStatusItemsDict, dtos, dto, SoeOriginStatusClassification.CustomerInvoicesOpenUser, true);
                        }

                        #endregion
                        break;
                    case SoeStatesAnalysis.OrderRemaingAmount:
                        #region OrderRemaingAmount

                        LoadStatusItemsDictForCustomerInvoices(ref customerInvoicesStatusItemsDict, actorCompanyId);
                        dtoIsValid = GetStatusItemDTOFromCustomerInvoices(customerInvoicesStatusItemsDict, dtos, dto, SoeOriginStatusClassification.OrdersOpen, true, true);

                        #endregion
                        break;

                    #endregion

                    #region CustomerInvoice

                    case SoeStatesAnalysis.CustomerInvoicesOpen:
                        #region CustomerInvoicesOpen

                        LoadStatusItemsDictForCustomerInvoices(ref customerInvoicesStatusItemsDict, actorCompanyId);
                        dtoIsValid = GetStatusItemDTOFromCustomerInvoices(customerInvoicesStatusItemsDict, dtos, dto, SoeOriginStatusClassification.CustomerInvoicesOpen, false);

                        #endregion
                        break;
                    case SoeStatesAnalysis.CustomerPaymentsUnpayed:
                        #region CustomerPaymentsUnpayed

                        LoadStatusItemsDictForCustomerInvoices(ref customerInvoicesStatusItemsDict, actorCompanyId);
                        dtoIsValid = GetStatusItemDTOFromCustomerInvoices(customerInvoicesStatusItemsDict, dtos, dto, SoeOriginStatusClassification.CustomerPaymentsUnpayed, false, useExAndInclVat: true);

                        #endregion
                        break;
                    case SoeStatesAnalysis.CustomerInvoicesOverdued:
                        #region CustomerInvoicesOverdued


                        LoadStatusItemsDictForCustomerInvoices(ref customerInvoicesStatusItemsDict, actorCompanyId);
                        dtoIsValid = GetStatusItemDTOFromCustomerInvoices(customerInvoicesStatusItemsDict, dtos, dto, SoeOriginStatusClassification.CustomerInvoicesReminder, false, useExAndInclVat: true);

                        #endregion
                        break;

                    #endregion

                    #region SupplierInvoice

                    case SoeStatesAnalysis.SupplierInvoicesOpen:
                        #region SupplierInvoicesOpen

                        LoadStatusItemsDictForSupplierInvoices(ref supplierInvoiceStatusItemsDict);
                        dtoIsValid = LoadStatusItemDTOFromSupplierInvoices(supplierInvoiceStatusItemsDict, dtos, dto, SoeOriginStatusClassification.SupplierInvoicesOpen, false);

                        #endregion
                        break;
                    case SoeStatesAnalysis.SupplierInvoicesUnpayed:
                        #region SupplierInvoicesUnpayed

                        bool showPending = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.SupplierInvoiceReportShowPendingPayments, 0, actorCompanyId, 0);

                        LoadStatusItemsDictForSupplierPayments(ref supplierPaymentStatusItemsDict);
                        dtoIsValid = LoadStatusItemDTOFromSupplierPayments(supplierPaymentStatusItemsDict, dtos, dto, SoeOriginStatusClassification.SupplierPaymentsUnpayed, false, useExAndInclVat: true, showPending: showPending);

                        #endregion
                        break;
                    case SoeStatesAnalysis.SupplierInvoicesOverdued:
                        #region SupplierInvoicesOverdued

                        LoadStatusItemsDictForSupplierPayments(ref supplierPaymentStatusItemsDict);
                        dtoIsValid = LoadStatusItemDTOFromSupplierPayments(supplierPaymentStatusItemsDict, dtos, dto, SoeOriginStatusClassification.SupplierInvoicesOverdue, false, useExAndInclVat: true);

                        #endregion
                        break;

                    #endregion

                    #region EDI

                    case SoeStatesAnalysis.EdiError:
                        #region EdiError

                        LoadEdiItems(ref ediItems, actorCompanyId);

                        var ediItemsError = (from edi in ediItems
                                             where edi.Status == TermGroup_EDIStatus.Error
                                             select edi).ToList();

                        dtoIsValid = LoadEdiDTO(ediItemsError, dtos, dto);

                        #endregion
                        break;
                    case SoeStatesAnalysis.EdiOrderError:
                        #region EdiOrderError

                        LoadEdiItems(ref ediItems, actorCompanyId);

                        var ediItemsOrderError = (from edi in ediItems
                                                  where edi.OrderStatus != TermGroup_EDIOrderStatus.Processed ||
                                                  edi.OrderId == null
                                                  select edi).ToList();

                        dtoIsValid = LoadEdiDTO(ediItemsOrderError, dtos, dto);

                        #endregion
                        break;
                    case SoeStatesAnalysis.EdiInvoicError:
                        #region EdiInvoicError

                        LoadEdiItems(ref ediItems, actorCompanyId);

                        var ediItemsInvoiceError = (from edi in ediItems
                                                    where edi.InvoiceStatus == TermGroup_EDIInvoiceStatus.Error &&
                                                    edi.EdiMessageType == TermGroup_EdiMessageType.SupplierInvoice
                                                    select edi).ToList();

                        dtoIsValid = LoadEdiDTO(ediItemsInvoiceError, dtos, dto);

                        #endregion
                        break;

                    #endregion

                    #region Scanning

                    case SoeStatesAnalysis.ScanningError:
                        #region ScanningError

                        LoadScanningItems(ref scanningItems, actorCompanyId);

                        var scanningItemsError =
                            (from edi in scanningItems
                             where edi.ScanningStatus == TermGroup_ScanningStatus.Error &&
                             edi.ScanningMessageType == TermGroup_ScanningMessageType.SupplierInvoice
                             select edi).ToList();

                        dtoIsValid = LoadScanningDTO(scanningItemsError, dtos, dto);

                        #endregion
                        break;
                    case SoeStatesAnalysis.ScanningInvoiceError:
                        #region ScanningInvoiceError

                        LoadScanningItems(ref scanningItems, actorCompanyId);

                        var scanningItemsInvoiceError =
                            (from edi in scanningItems
                             where edi.InvoiceStatus == TermGroup_EDIInvoiceStatus.Error &&
                             edi.ScanningMessageType == TermGroup_ScanningMessageType.SupplierInvoice
                             select edi).ToList();

                        dtoIsValid = LoadScanningDTO(scanningItemsInvoiceError, dtos, dto);

                        #endregion
                        break;
                    case SoeStatesAnalysis.ScanningUnprocessedArrivals:
                        #region ScanningUnprocessedArrivals

                        LoadScanningItems(ref scanningItems, actorCompanyId);

                        var scanningItemsUnprocessedArrivals =
                            (from edi in scanningItems
                             where edi.NrOfInvoices == 0 &&
                             edi.ScanningMessageType == TermGroup_ScanningMessageType.Arrival
                             select edi).ToList();

                        dtoIsValid = LoadScanningDTO(scanningItemsUnprocessedArrivals, dtos, dto);

                        #endregion
                        break;

                    #endregion

                    #region Communication

                    case SoeStatesAnalysis.NewMessages:
                        #region New_messages

                        int licenseId = Convert.ToInt16(LicenseManager.GetLicenseByCompany(actorCompanyId).LicenseId);
                        dto.NoOfItems = Convert.ToInt16(CommunicationManager.GetIncomingMessagesCount(licenseId, base.UserId));
                        dtoIsValid = true;
                        break;

                        #endregion
                    #endregion

                    #region HouseHoldTaxDeduction

                    case SoeStatesAnalysis.HouseHoldTaxDeductionApply:
                        #region HouseHoldTaxDeductionApply

                        dto.NoOfItems = HouseholdTaxDeductionManager.GetHouseholdTaxDeductionRowsCounter(actorCompanyId, SoeHouseholdClassificationGroup.Apply, TermGroup_HouseHoldTaxDeductionType.None);
                        dtoIsValid = true;

                        #endregion
                        break;
                    case SoeStatesAnalysis.HouseHoldTaxDeductionApplied:
                        #region HouseHoldTaxDeductionApplied

                        dto.NoOfItems = HouseholdTaxDeductionManager.GetHouseholdTaxDeductionRowsCounter(actorCompanyId, SoeHouseholdClassificationGroup.Applied, TermGroup_HouseHoldTaxDeductionType.None);
                        dtoIsValid = true;

                        #endregion
                        break;
                    case SoeStatesAnalysis.HouseHoldTaxDeductionReceived:
                        #region HouseHoldTaxDeductionReceived

                        dto.NoOfItems = HouseholdTaxDeductionManager.GetHouseholdTaxDeductionRowsCounter(actorCompanyId, SoeHouseholdClassificationGroup.Received, TermGroup_HouseHoldTaxDeductionType.None);
                        dtoIsValid = true;

                        #endregion
                        break;
                    case SoeStatesAnalysis.HouseHoldTaxDeductionDenied:
                        #region HouseHoldTaxDeductionDenied

                        dto.NoOfItems = HouseholdTaxDeductionManager.GetHouseholdTaxDeductionRowsCounter(actorCompanyId, SoeHouseholdClassificationGroup.Denied, TermGroup_HouseHoldTaxDeductionType.None);
                        dtoIsValid = true;

                        break;
                        #endregion

                    #endregion
                }

                if (dtoIsValid)
                    dtos.Add(dto);
            }

            return dtos;
        }

        #region Help-methods

        private void LoadEdiItems(ref List<EdiEntryViewDTO> ediItems, int actorCompanyId)
        {
            if (ediItems == null)
                ediItems = EdiManager.GetEdiEntrys(actorCompanyId, SoeEntityState.Active);
        }

        private bool LoadEdiDTO(List<EdiEntryViewDTO> ediItems, List<StateAnalysisDTO> dtos, StateAnalysisDTO dto)
        {
            if (ediItems == null || dtos == null || dto == null)
                return false;

            var items = ediItems;
            dto.NoOfItems = items.Count;
            dto.NoOfActorsForItems = items.Where(i => i.SupplierId.HasValue).Select(i => i.SupplierId.Value).Distinct().Count();

            dto.TotalAmount = items.Sum(i => i.Sum);

            return true;
        }

        private void LoadScanningItems(ref List<EdiEntryViewDTO> scanningItems, int actorCompanyId)
        {
            if (scanningItems == null)
                scanningItems = EdiManager.GetScanningEntrys(actorCompanyId, SoeEntityState.Active);
        }

        private bool LoadScanningDTO(List<EdiEntryViewDTO> scanningItems, List<StateAnalysisDTO> dtos, StateAnalysisDTO dto)
        {
            if (scanningItems == null || dtos == null || dto == null)
                return false;

            var items = scanningItems;
            dto.NoOfItems = items.Count;
            dto.NoOfActorsForItems = items.Where(i => i.SupplierId.HasValue).Select(i => i.SupplierId.Value).Distinct().Count();

            dto.TotalAmount = items.Sum(i => i.Sum);

            return true;
        }

        private void LoadStatusItemsDictForSupplierInvoices(ref Dictionary<int, List<SupplierInvoiceGridDTO>> statusItemsDict)
        {
            if (statusItemsDict == null)
            {
                using (CompEntities entities = new CompEntities())
                {
                    statusItemsDict = InvoiceManager.GetSupplierInvoicesDictForStateAnalysis(entities);
                }
            }
        }

        private void LoadStatusItemsDictForSupplierPayments(ref Dictionary<int, List<SupplierPaymentGridDTO>> statusItemsDict)
        {
            if (statusItemsDict == null)
            {
                using (CompEntities entities = new CompEntities())
                {
                    statusItemsDict = SupplierInvoiceManager.GetSupplierPaymentsDictForStateAnalysis(entities);
                }
            }
        }

        private void LoadStatusItemsDictForCustomerInvoices(ref Dictionary<int, List<CustomerInvoiceGridDTO>> statusItemsDict, int actorCompanyId)
        {
            if (statusItemsDict == null)
            {
                using (CompEntities entities = new CompEntities())
                {
                    statusItemsDict = InvoiceManager.GetCustomerInvoicesDictForStateAnalysis(entities, actorCompanyId);
                }
            }
        }

        private bool LoadStatusItemDTOFromSupplierInvoices(Dictionary<int, List<SupplierInvoiceGridDTO>> itemsDict, List<StateAnalysisDTO> dtos, StateAnalysisDTO dto, SoeOriginStatusClassification originStatusClassification, bool exclVat, bool remaining = false, bool useExAndInclVat = false, bool showPending = false)
        {
            if (itemsDict == null || dtos == null || dto == null || originStatusClassification == SoeOriginStatusClassification.None)
                return false;

            int classification = (int)originStatusClassification;
            if (itemsDict.ContainsKey(classification))
            {
                var items = itemsDict[classification];

                #region show pending

                if (showPending)
                {
                    items.AddRange(itemsDict[(int)SoeOriginStatusClassification.SupplierPaymentsPayed].Where(i => i.PayDate != null && i.PayDate <= DateTime.Today));
                }

                #endregion

                if (remaining)
                {
                    dto.NoOfItems = items.Count;
                    dto.NoOfActorsForItems = items.Select(i => i.SupplierId).Distinct().Count();
                    dto.TotalAmount = items.Sum(i => i.TotalAmount - i.PaidAmount);
                }
                else
                {


                    dto.NoOfItems = items.Count;
                    dto.NoOfActorsForItems = items.Select(i => i.SupplierId).Distinct().Count();
                    dto.TotalAmount = exclVat ? items.Sum(i => i.TotalAmountExVat) : items.Sum(i => i.TotalAmount);

                    if (!exclVat && useExAndInclVat)
                    {
                        dto.TotalAmount2 = items.Sum(i => i.TotalAmountExVat);

                        if (originStatusClassification == SoeOriginStatusClassification.CustomerPaymentsUnpayed || originStatusClassification == SoeOriginStatusClassification.CustomerInvoicesReminder)
                        {
                            dto.TotalAmount3 = items.Sum(i => i.TotalAmount - i.PaidAmount);
                        }
                    }

                }

                return true;
            }

            return false;
        }

        private bool LoadStatusItemDTOFromSupplierPayments(Dictionary<int, List<SupplierPaymentGridDTO>> itemsDict, List<StateAnalysisDTO> dtos, StateAnalysisDTO dto, SoeOriginStatusClassification originStatusClassification, bool exclVat, bool remaining = false, bool useExAndInclVat = false, bool showPending = false)
        {
            if (itemsDict == null || dtos == null || dto == null || originStatusClassification == SoeOriginStatusClassification.None)
                return false;

            int classification = (int)originStatusClassification;
            if (itemsDict.ContainsKey(classification))
            {
                var items = itemsDict[classification];

                #region show pending

                if (showPending)
                {
                    items.AddRange(itemsDict[(int)SoeOriginStatusClassification.SupplierPaymentsPayed].Where(i => i.PayDate != null && i.PayDate <= DateTime.Today));
                }

                #endregion

                if (remaining)
                {
                    dto.NoOfItems = items.Count;
                    dto.NoOfActorsForItems = items.Select(i => i.SupplierId).Distinct().Count();
                    dto.TotalAmount = items.Sum(i => i.TotalAmount - i.PaidAmount);
                }
                else
                {


                    dto.NoOfItems = items.Count;
                    dto.NoOfActorsForItems = items.Select(i => i.SupplierId).Distinct().Count();
                    dto.TotalAmount = exclVat ? items.Sum(i => i.TotalAmountExVat) : items.Sum(i => i.TotalAmount);

                    if (!exclVat && useExAndInclVat)
                        dto.TotalAmount2 = items.Sum(i => i.TotalAmountExVat);

                }

                return true;
            }

            return false;
        }

        private bool GetStatusItemDTOFromCustomerInvoices(Dictionary<int, List<CustomerInvoiceGridDTO>> itemsDict, List<StateAnalysisDTO> dtos, StateAnalysisDTO dto, SoeOriginStatusClassification originStatusClassification, bool exclVat, bool remaining = false, bool useExAndInclVat = false, bool showPending = false)
        {
            if (itemsDict == null || dtos == null || dto == null || originStatusClassification == SoeOriginStatusClassification.None)
                return false;

            int classification = (int)originStatusClassification;
            if (itemsDict.ContainsKey(classification))
            {
                var items = itemsDict[classification];

                #region show pending

                if (showPending)
                {
                    items.AddRange(itemsDict[(int)SoeOriginStatusClassification.SupplierPaymentsPayed].Where(i => i.PayDate != null && i.PayDate <= DateTime.Today));
                }

                #endregion

                if (remaining)
                {
                    dto.NoOfItems = items.Count;
                    dto.NoOfActorsForItems = items.Select(i => i.ActorCustomerId).Distinct().Count();
                    dto.TotalAmount = items.Where(o => o.OrderType != (int)TermGroup_OrderType.Internal).Sum(i => i.RemainingAmountExVat);
                }
                else
                {
                    if (originStatusClassification == SoeOriginStatusClassification.ContractsOpen)
                    {
                        dto.NoOfItems = items.Count;
                        dto.NoOfActorsForItems = items.Select(i => i.ActorCustomerId).Distinct().Count();
                        foreach (CustomerInvoiceGridDTO item in items)
                        {
                            dto.TotalAmount = exclVat ? items.Sum(i => i.ContractYearlyValueExVat) : items.Sum(i => i.ContractYearlyValue);
                        }
                    }
                    else
                    {
                        dto.NoOfItems = items.Count;
                        dto.NoOfActorsForItems = items.Select(i => i.ActorCustomerId).Distinct().Count();
                        dto.TotalAmount = exclVat ? items.Sum(i => i.TotalAmount - i.VATAmount) : items.Sum(i => i.TotalAmount);

                        if (!exclVat && useExAndInclVat)
                        {
                            dto.TotalAmount2 = items.Sum(i => i.TotalAmount - i.VATAmount);

                            if (originStatusClassification == SoeOriginStatusClassification.CustomerPaymentsUnpayed || originStatusClassification == SoeOriginStatusClassification.CustomerInvoicesReminder)
                            {
                                dto.TotalAmount3 = items.Sum(i => i.TotalAmount - i.PaidAmount);
                            }
                        }
                    }
                }

                return true;
            }

            return false;
        }

        #endregion

        #endregion
    }
}
