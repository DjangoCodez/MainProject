using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Util;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Dynamic;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;
using System.Transactions;

namespace SoftOne.Soe.Business.Core
{
    public class SupplierInvoiceManager : ManagerBase
    {
        #region Variables

        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #endregion

        #region Ctor

        public SupplierInvoiceManager(ParameterObject parameterObject) : base(parameterObject) { }

        #endregion

        #region SupplierInvoice

        public List<SupplierInvoiceHistoryGridDTO> GetSupplierInvoiceHistory(int supplierId, int records = 20)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            return entitiesReadOnly.Invoice.OfType<SupplierInvoice>()
                .Where(i => i.ActorId == supplierId)
                .OrderByDescending(i => i.InvoiceDate)
                .Take(records)
                .Select(i => new SupplierInvoiceHistoryGridDTO()
                {
					InvoiceId = i.InvoiceId,
					InvoiceNr = i.InvoiceNr,
					InvoiceDate = i.InvoiceDate ?? DateTime.MinValue,
                    DueDate = i.DueDate ?? DateTime.MaxValue,
					PaymentDate = i.PaymentRow.Max(pr => pr.PayDate),
					TotalAmount = i.TotalAmount,
					TotalAmountCurrency = i.TotalAmountCurrency,
					VATAmount = i.VATAmount,
					VATAmountCurrency = i.VATAmountCurrency
				})
                .ToList();
		}

        private List<SupplierPaymentGridDTO> SetSupplierPaymentTexts(List<SupplierPaymentGridDTO> dtos, bool hasCurrencyPermission)
        {
            try
            {
                var langId = base.GetLangId();
                var statusTexts = base.GetTermGroupDict(TermGroup.OriginStatus, langId);
                var billingTypes = base.GetTermGroupDict(TermGroup.InvoiceBillingType, langId);

                dtos.ForEach(i =>
                {
                    i.StatusName = i.Status != 0 ? statusTexts[i.Status] : "";
                    i.BillingTypeName = i.BillingTypeId != 0 ? billingTypes[i.BillingTypeId] : "";

                    if (hasCurrencyPermission)
                    {
                        i.CurrencyCode = CountryCurrencyManager.GetCurrencyCode(i.SysCurrencyId);
                    }
                });

            }
            catch (Exception ex)
            {
                log.Error(ex.Message, ex);
            }
            return dtos;
        }

        private void SetSupplierInvoiceTexts(IReadOnlyList<SupplierInvoiceGridDTO> dtos, bool hasCurrencyPermission)
        {

            try
            {
                var langId = base.GetLangId();
                var statusTexts = base.GetTermGroupDict(TermGroup.OriginStatus, langId);
                var billingTypes = base.GetTermGroupDict(TermGroup.InvoiceBillingType, langId);
                var invoiceTypes = base.GetTermGroupDict(TermGroup.SupplierInvoiceType);

                foreach (var i in dtos)
                {
                    if (string.IsNullOrEmpty(i.StatusName))
                    {
                        i.StatusName = i.Status != 0 ? statusTexts[i.Status] : " ";
                    }
                    i.BillingTypeName = i.BillingTypeId != 0 ? billingTypes[i.BillingTypeId] : "";

                    if (hasCurrencyPermission)
                    {
                        i.CurrencyCode = CountryCurrencyManager.GetCurrencyCode(i.SysCurrencyId);
                    }
                    i.TypeName = i.Type != 0 ? invoiceTypes[i.Type] : "";
                };

            }
            catch (Exception ex)
            {
                log.Error(ex.Message, ex);
            }
        }

        public List<SupplierInvoiceGridDTO> GetSupplierInvoicesGridForProjectCental(int projectId, bool includeChildProjects, DateTime? fromDateIn, DateTime? toDateIn)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            DateTime fromDate = fromDateIn ?? DateTime.MinValue;
            DateTime toDate = toDateIn ?? DateTime.MaxValue;
            var projectIds = includeChildProjects ? 
                ProjectManager.GetProjectIdsFromMain(entities, ActorCompanyId, projectId) : 
                new List<int> { projectId };
            
            return new List<SupplierInvoiceGridDTO>();
        } 
        public List<SupplierInvoiceGridDTO> GetSupplierInvoicesGridForProjectCentral(int actorCompanyId, int projectId, bool includeChildProjects, DateTime? fromDateIn = null, DateTime? toDateIn = null)
        {
            bool hasSupplierInvoicesPermission = FeatureManager.HasRolePermission(Feature.Economy_Supplier_Invoice_Status, Permission.Readonly, base.RoleId, actorCompanyId);
            if (!hasSupplierInvoicesPermission)
                return new List<SupplierInvoiceGridDTO>();

            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            List<int> projectIds = includeChildProjects ? 
                ProjectManager.GetProjectIdsFromMain(entities, ActorCompanyId, projectId) : 
                new List<int> { projectId };

            int baseSysCurrencyId = CountryCurrencyManager.GetCompanyBaseSysCurrencyId(entities, actorCompanyId);
            bool hideVatWarning = SettingManager.GetCompanyBoolSetting(CompanySettingType.BillingHideVatWarnings);
            bool closeInvoicesWhenTransferredToVoucher = SettingManager.GetCompanyBoolSetting(CompanySettingType.SupplierCloseInvoicesWhenTransferredToVoucher);
            bool hasCurrencyPermission = FeatureManager.HasRolePermission(Feature.Economy_Supplier_Invoice_Status_Foreign, Permission.Readonly, base.RoleId, actorCompanyId);

            DateTime fromDate = fromDateIn ?? CalendarUtility.DATETIME_MINVALUE;
            DateTime toDate = toDateIn ?? CalendarUtility.DATETIME_MAXVALUE;

            string projectIdsStr = string.Join(",", projectIds);
            var supplierInvoices = entities.GetSupplierInvoicesForProject(actorCompanyId, projectIdsStr, fromDate, toDate);

            var dtos = new List<SupplierInvoiceGridDTO>();

            foreach (var supplierInvoice in supplierInvoices)
            {
                bool isWithRate = supplierInvoice.InvoiceTotalAmount != supplierInvoice.InvoiceTotalAmountCurrency;
                var dto = supplierInvoice.ToGridDTO(hideVatWarning, true, closeInvoicesWhenTransferredToVoucher, baseSysCurrencyId != 0 && isWithRate);
                dtos.Add(dto);
            }

            AddSupplierInvoiceAttestStatesConverted(entities, dtos);
            AddSupplierInvoiceAttestGroupsConverted(dtos);
            SetSupplierInvoiceTexts(dtos, hasCurrencyPermission);

            return dtos;
        }

        public List<SupplierInvoiceGridDTO> GetSupplierInvoicesForGrid(TermGroup_ChangeStatusGridAllItemsSelection allItemsSelection, bool loadOpen, bool loadClosed)
        {
			if (!loadOpen && !loadClosed)
				return new List<SupplierInvoiceGridDTO>();

            DateTime? fromDate = null;
            if (allItemsSelection != TermGroup_ChangeStatusGridAllItemsSelection.All)
                fromDate = DateTime.Today.AddMonths(-(int)allItemsSelection);

			using (var entities = new CompEntities())
            {
                return GetSupplierInvoicesForGrid(entities, base.ActorCompanyId, loadOpen, loadClosed, fromDate);
            }
		}

        public List<SupplierInvoiceSummaryDTO> GetSupplierInvoicesSummary(int actorCompanyId)
        {
			using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
			return this.GetSupplierInvoicesSummary(entitiesReadOnly, actorCompanyId);
		}

        public List<SupplierInvoiceSummaryDTO> GetSupplierInvoicesSummary(CompEntities entities, int actorCompanyId)
        {
            // EDI Entries summary
            using var entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            var ediEntriesCount = (from e in entitiesReadOnly.EdiEntryView
									 where e.ActorCompanyId == actorCompanyId &&
										e.Type == (int)TermGroup_EdiMessageType.SupplierInvoice &&
									   !String.IsNullOrEmpty(e.InvoiceNr) &&
									   !e.InvoiceId.HasValue &&
									   e.SupplierId.HasValue && e.SupplierId.Value > 0 &&
									   e.MessageType == (int)TermGroup_EdiMessageType.SupplierInvoice &&
									   e.InvoiceStatus == (int)TermGroup_EDIInvoiceStatus.Unprocessed &&
									   (e.Status == (int)TermGroup_EDIStatus.UnderProcessing || e.Status == (int)TermGroup_EDIStatus.Processed) &&
									   e.State == (int)SoeEntityState.Active
									 select new
									 {
										 e.EdiEntryId
									 }).LongCount();

            entities.CommandTimeout = 600;

            int[] attesingOriginStatus = { (int)SoeOriginStatus.Draft, (int)SoeOriginStatus.Origin, (int)SoeOriginStatus.Voucher };

			// Supplier Invoices summary
			List<SupplierInvoiceSummaryDTO> supplierInvoiceSummaryDTOs = entities.Invoice.OfType<SupplierInvoice>()
							.Where(inv => inv.Origin.ActorCompanyId == actorCompanyId   )
							.Where(inv => inv.State == (int)SoeEntityState.Active && inv.IsTemplate != true && inv.BillingType > 0)
							.Where(inv => inv.Origin.Status != (int)SoeOriginStatus.Cancel)
							.GroupBy(i => new { ActorCompanyId = i.Origin.ActorCompanyId, CompanyName = i.Origin.Company.Name })
							.Select(g => new SupplierInvoiceSummaryDTO
							{
								ActorCompanyId = g.Key.ActorCompanyId,
								ActorCompanyName = g.Key.CompanyName,
								UnhandledInvoiceCount = g.LongCount(i => i.Origin.Status == (int)SoeOriginStatus.Draft),
								AttestingInvoiceCount = g.LongCount(i => !i.AttestState.Closed && i.Origin.Type == 1 && attesingOriginStatus.Contains(i.Origin.Status) && !(i.FullyPayed == true && i.VoucherHead != null && i.SysPaymentTypeId != 6)),
								PaymentReadyInvoiceCount = g.LongCount(i => i.AttestState.Closed && i.FullyPayed != true && i.Type == 1)
							}).ToList();

            //Add EDI entries to supplier invoices summary and result conatains only one company
            supplierInvoiceSummaryDTOs.ForEach(i => i.UnhandledInvoiceCount += ediEntriesCount);

			return supplierInvoiceSummaryDTOs;
        }

		public List<SupplierInvoiceGridDTO> GetSupplierInvoicesForGrid(int actorCompanyId, TermGroup_ChangeStatusGridAllItemsSelection allItemsSelection, bool loadOpen, bool loadClosed)
        {
            if (!loadOpen && !loadClosed)
                return new List<SupplierInvoiceGridDTO>();

            DateTime? fromDate = null;
            if (allItemsSelection != TermGroup_ChangeStatusGridAllItemsSelection.All)
                fromDate = DateTime.Today.AddMonths(-(int)allItemsSelection);

            using (var entities = new CompEntities())
            {
                return GetSupplierInvoicesForGrid(entities, actorCompanyId, loadOpen, loadClosed, fromDate);
            }
        }

        public List<SupplierInvoiceGridDTO> GetSupplierInvoicesForGrid(
            CompEntities entities,
            int actorCompanyId,
            bool loadOpen, 
            bool loadClosed,
            DateTime? fromDate)
        {
            #region PreReq
			int? baseSysCurrencyId = CountryCurrencyManager.GetCompanyBaseSysCurrencyId(entities, actorCompanyId);
			if (!baseSysCurrencyId.HasValue)
				baseSysCurrencyId = 0;
			bool hideVatWarning = SettingManager.GetBoolSetting(entities, SettingMainType.Company, (int)CompanySettingType.BillingHideVatWarnings, 0, actorCompanyId, 0);
			bool closeInvoicesWhenTransferredToVoucher = SettingManager.GetBoolSetting(entities, SettingMainType.Company, (int)CompanySettingType.SupplierCloseInvoicesWhenTransferredToVoucher, 0, actorCompanyId, 0);
			bool hasCurrencyPermission = FeatureManager.HasRolePermission(Feature.Economy_Supplier_Invoice_Status_Foreign, Permission.Readonly, base.RoleId, actorCompanyId);
            #endregion

            var invoices = entities.GetSupplierInvoicesForGrid(
                actorCompanyId,
                loadOpen,
                loadClosed,
                closeInvoicesWhenTransferredToVoucher,
                fromDate ?? DateTime.MinValue).ToList();
            var dtos = new List<SupplierInvoiceGridDTO>();

			if (!hasCurrencyPermission && baseSysCurrencyId != 0)
				invoices = invoices.Where(i => i.SysCurrencyId == baseSysCurrencyId).ToList();

			foreach (var result in invoices)
			{
				SupplierInvoiceGridDTO dto = result.ToGridDTO(hideVatWarning, true, closeInvoicesWhenTransferredToVoucher, (baseSysCurrencyId != 0 && result.SysCurrencyId != baseSysCurrencyId));
				dto.Type = (int)TermGroup_SupplierInvoiceType.Invoice;
				dtos.Add(dto);
			}

			AddSupplierInvoiceAttestStatesConverted(entities, dtos);
			AddSupplierInvoiceAttestGroupsConverted(dtos);

            if (loadOpen)
            {
                var preProcessedInvoices = GetPreProcessedSupplierIinvoices(hideVatWarning, baseSysCurrencyId);
                dtos.AddRange(preProcessedInvoices);
			}

			SetSupplierInvoiceTexts(dtos, hasCurrencyPermission);
			AddIdentification(dtos);

			return dtos;
		}

		public List<SupplierInvoiceGridDTO> GetSupplierInvoicesForGrid(bool loadOpen, bool loadClosed, TermGroup_ChangeStatusGridAllItemsSelection? allItemsSelection, int supplierId = 0, bool skipEdiAndScanning = false, int? projectId = null, bool includeChildProjects = false, List<int> invoiceIds = null)
        {
            using (var entities = new CompEntities())
            {
                return GetSupplierInvoicesForGrid(entities, loadOpen, loadClosed, allItemsSelection, supplierId, skipEdiAndScanning, projectId, includeChildProjects, invoiceIds);
            }
        }
        public List<SupplierInvoiceGridDTO> GetSupplierInvoicesForGrid(CompEntities entities, bool loadOpen, bool loadClosed, TermGroup_ChangeStatusGridAllItemsSelection? allItemsSelection, int supplierId = 0, bool skipEdiAndScanning = false, int? projectId = null, bool includeChildProjects = false, List<int> invoiceIds = null, SoeOriginStatusClassification originStatusClassification = SoeOriginStatusClassification.None, bool addAttestInfo = true, List<int> projectIds = null)
        {
            var dtos = new List<SupplierInvoiceGridDTO>();

            #region prereq

            using var entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            int actorCompanyId = base.ActorCompanyId;
            int? baseSysCurrencyId = CountryCurrencyManager.GetCompanyBaseSysCurrencyId(entities, actorCompanyId);
            if (!baseSysCurrencyId.HasValue)
                baseSysCurrencyId = 0;
            bool hideVatWarning = SettingManager.GetBoolSetting(entities, SettingMainType.Company, (int)CompanySettingType.BillingHideVatWarnings, 0, actorCompanyId, 0);
            bool closeInvoicesWhenTransferredToVoucher = SettingManager.GetBoolSetting(entities, SettingMainType.Company, (int)CompanySettingType.SupplierCloseInvoicesWhenTransferredToVoucher, 0, actorCompanyId, 0);
            bool hasCurrencyPermission = FeatureManager.HasRolePermission(Feature.Economy_Supplier_Invoice_Status_Foreign, Permission.Readonly, base.RoleId, actorCompanyId);

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

            #endregion

            List<GetSupplierInvoices_Result> invoices;

            if (!loadOpen && !loadClosed)
            {
                invoices = new List<GetSupplierInvoices_Result>();
            }
            else if (closeInvoicesWhenTransferredToVoucher && !(loadOpen && loadClosed))
            {
                invoices = entities.GetSupplierInvoices(base.ActorCompanyId, supplierId != 0 ? supplierId.ToNullable() : null, loadOpen, loadClosed, closeInvoicesWhenTransferredToVoucher).OrderBy(i => i.InvoiceSeqNr.HasValue ? i.InvoiceSeqNr : 0).ToList();
            }
            else
            {
                invoices = entities.GetSupplierInvoices(base.ActorCompanyId, supplierId != 0 ? supplierId.ToNullable() : null, loadOpen, loadClosed, false).OrderBy(i => i.InvoiceSeqNr.HasValue ? i.InvoiceSeqNr : 0).ToList();

                if (loadClosed && !loadOpen)
                {
                    //Load only closed invoices
                    invoices = (from i in invoices
                                where (i.Status == (int)SoeOriginStatus.Cancel ||
                                (i.FullyPayed == true && (i.Status == (int)SoeOriginStatus.Voucher || i.Status == (int)SoeOriginStatus.Origin)))
                                //orderby i.InvoiceSeqNr.HasValue, i.InvoiceSeqNr.HasValue ? i.InvoiceSeqNr.Value : 0
                                select i).ToList();
                }
                else if (loadOpen && !loadClosed)
                {
                    invoices = (from i in invoices
                                where (i.Status != (int)SoeOriginStatus.Cancel &&
                                !(i.FullyPayed == true && (i.Status == (int)SoeOriginStatus.Voucher || i.Status == (int)SoeOriginStatus.Origin)))
                                //orderby i.InvoiceSeqNr.HasValue, i.InvoiceSeqNr.HasValue ? i.InvoiceSeqNr.Value : 0
                                select i).ToList();
                }
            }

            switch (originStatusClassification)
            {
                case SoeOriginStatusClassification.SupplierPaymentsSupplierCentralPayed:
                    invoices = invoices.Where(i => i.FullyPayed).ToList();
                    break;
                case SoeOriginStatusClassification.SupplierInvoicesOverdue:
                    invoices = invoices.Where(i => !i.FullyPayed && i.DueDate < DateTime.Now).ToList();
                    break;
                case SoeOriginStatusClassification.SupplierPaymentsUnpayed:
                    invoices = invoices.Where(i => (i.Status == (int)SoeOriginStatus.Origin || i.Status == (int)SoeOriginStatus.Voucher) && !i.FullyPayed && !i.BlockPayment && !i.SupplierBlockPayment).ToList();
                    break;
            }


            if (invoiceIds != null)
            {
                invoices = invoices.Where(i => invoiceIds.Contains(i.InvoiceId)).ToList();
            }
            else if (projectId != null && projectId > 0)
            {
                if (includeChildProjects)
                {
                    List<int> ids = projectIds != null ? projectIds : ProjectManager.GetChildProjectsIds(entities, base.ActorCompanyId, projectId.Value);
                    invoices = invoices.Where(i => ids.Contains(i.ProjectId)).ToList();
                }
                else
                {
                    invoices = invoices.Where(i => i.ProjectId == projectId.Value).ToList();
                }
            }

            //Filter currency
            if (!hasCurrencyPermission && baseSysCurrencyId != 0)
                invoices = invoices.Where(i => i.SysCurrencyId == baseSysCurrencyId).ToList();

            foreach (GetSupplierInvoices_Result result in invoices)
            {
                SupplierInvoiceGridDTO dto = result.ToGridDTO(hideVatWarning, true, closeInvoicesWhenTransferredToVoucher, (baseSysCurrencyId != 0 && result.SysCurrencyId != baseSysCurrencyId));
                dto.Type = (int)TermGroup_SupplierInvoiceType.Invoice;
                dtos.Add(dto);
            }

            //Add attest states (and attestant name later)
            if (addAttestInfo)
            {
                AddSupplierInvoiceAttestStatesConverted(entities, dtos);

                //Add attest groups
                AddSupplierInvoiceAttestGroupsConverted(dtos);
            }
            if (loadOpen)
            {
                if (!skipEdiAndScanning && (projectId == null || projectId == 0))
                {
                    #region Prereq

                    var langId = GetLangId();
                    var ediSourceTypes = base.GetTermGroupDict(TermGroup.EDISourceType, langId);
                    var ediMessageTypes = base.GetTermGroupDict(TermGroup.EdiMessageType, langId);
                    var invoiceStatuses = base.GetTermGroupDict(TermGroup.EDIInvoiceStatus, langId);

                    #endregion
                    //Finvoice Should not be fetched where since we have special import grid for this..
                    #region Scanning

                    var scanningItems = (from e in entitiesReadOnly.ScanningEntryView
                                         where e.ActorCompanyId == actorCompanyId &&
                                         e.State == (int)SoeEntityState.Active &&
                                         e.MessageType == (int)TermGroup_ScanningMessageType.SupplierInvoice &&
                                         e.InvoiceId == null
                                         select e);

                    if (scanningItems.Any())
                    {
                        var rowItems = EdiManager.GetScanningEntryRowItemsByCompany(entities, base.ActorCompanyId, scanningItems.Select(s => s.ScanningEntryId).ToList());

                        foreach (var entry in scanningItems)
                        {
                            // Check interpretation
                            var rowItemsForEntry = rowItems.Where(i => i.ScanningEntryId == entry.ScanningEntryId).ToList();

                            SupplierInvoiceGridDTO dto = entry.ToConvertedScanningSupplierGridDTO(hideVatWarning, true, (baseSysCurrencyId != 0 && entry.SysCurrencyId != baseSysCurrencyId));

                            if ((ScanningProvider)entry.Provider == ScanningProvider.ReadSoft)
                            {
                                dto.RoundedInterpretation = EdiManager.GetScanningEntryRoundedInterpretation(rowItemsForEntry);
                                dto.EdiMessageProviderName = "ReadSoft";
                            }
                            else if ((ScanningProvider)entry.Provider == ScanningProvider.AzoraOne)
                            {
                                dto.RoundedInterpretation = (int)TermGroup_ScanningInterpretation.ValueIsValid;
                                dto.EdiMessageProviderName = "AzoraOne";
                            }
                            dto.SourceTypeName = dto.Type != 0 ? ediSourceTypes[dto.Type] : "";
                            dto.EdiMessageTypeName = dto.EdiMessageType != 0 ? ediMessageTypes[dto.EdiMessageType] : "";
                            dto.StatusName = dto.Status != 0 ? invoiceStatuses[dto.Status] : "";
                            dto.Type = (int)TermGroup_SupplierInvoiceType.Scanning;

                            dtos.Add(dto);
                        }
                    }
                    #endregion

                    #region EDI
                    var items = (from e in entitiesReadOnly.EdiEntryView
                                 where e.ActorCompanyId == actorCompanyId &&
                                 e.Type == (int)TermGroup_EdiMessageType.SupplierInvoice &&
                                !String.IsNullOrEmpty(e.InvoiceNr) &&
                                !e.InvoiceId.HasValue &&
                                e.SupplierId.HasValue && e.SupplierId.Value > 0 &&
                                e.MessageType == (int)TermGroup_EdiMessageType.SupplierInvoice &&
                                e.InvoiceStatus == (int)TermGroup_EDIInvoiceStatus.Unprocessed &&
                                (e.Status == (int)TermGroup_EDIStatus.UnderProcessing || e.Status == (int)TermGroup_EDIStatus.Processed) &&
                                e.State == (int)SoeEntityState.Active
                                 select e);

                    foreach (var entry in items)
                    {
                        SupplierInvoiceGridDTO dto = entry.ToConvertedEdiSupplierGridDTO(hideVatWarning, true, (baseSysCurrencyId != 0 && entry.SysCurrencyId != baseSysCurrencyId));
                        dto.Type = (int)TermGroup_SupplierInvoiceType.EDI;

                        dto.CurrencyCode = CountryCurrencyManager.GetCurrencyCode(dto.SysCurrencyId);
                        dto.SourceTypeName = dto.Type != 0 ? ediSourceTypes[dto.Type] : "";
                        dto.EdiMessageTypeName = dto.EdiMessageType != 0 ? ediMessageTypes[dto.EdiMessageType] : "";
                        dto.StatusName = dto.Status != 0 ? invoiceStatuses[dto.Status] : "";

                        dtos.Add(dto);
                    }
                    #endregion

                    #region Uploaded images                    

                    var uploaded = (from i in entitiesReadOnly.Invoice.OfType<SupplierInvoice>()
                                    where i.Origin.ActorCompanyId == ActorCompanyId &&
                                        i.IsTemplate == false &&
                                        i.State == (int)SoeEntityState.Active &&
                                        i.Type == (int)TermGroup_SupplierInvoiceType.Uploaded
                                    select new SupplierInvoiceGridDTO
                                    {
                                        SupplierInvoiceId = i.InvoiceId,
                                        Type = i.Type,
                                        OwnerActorId = i.ActorId ?? 0,
                                        SeqNr = i.SeqNr.ToString() ?? "",
                                        InvoiceNr = i.InvoiceNr,
                                        BillingTypeId = i.BillingType,
                                        Status = 0,
                                        SupplierName = i.Actor.Supplier.SupplierNr + " " + i.Actor.Supplier.Name,
                                        SupplierId = i.ActorId ?? 0,
                                        TotalAmount = i.TotalAmount,
                                        TotalAmountText = i.TotalAmount.ToString(),
                                        TotalAmountCurrency = 0,
                                        VATAmount = 0,
                                        VATAmountCurrency = 0,
                                        PayAmount = 0,
                                        PayAmountCurrency = 0,
                                        PaidAmount = 0,
                                        PaidAmountCurrency = 0,
                                        VatType = i.VatType,
                                        SysCurrencyId = i.Currency.SysCurrencyId,
                                        InvoiceDate = i.InvoiceDate,
                                        DueDate = i.DueDate,
                                        PayDate = null,
                                        AttestStateId = null,
                                        AttestGroupId = 0,
                                        FullyPaid = i.FullyPayed,
                                        StatusIcon = i.StatusIcon,
                                        CurrencyRate = i.CurrencyRate,
                                        InternalText = i.InternalDescription,
                                        TimeDiscountDate = i.TimeDiscountDate,
                                        TimeDiscountPercent = i.TimeDiscountPercent
                                    }).ToList();

                    
                    foreach (var item in uploaded)
                    {
                        item.PaymentStatuses = string.Empty;
                        item.CurrentAttestUserName = string.Empty;
                        item.PayAmountCurrencyText = string.Empty;
                        item.TotalAmountCurrencyText = string.Empty;
                        item.PayAmountText = string.Empty;
                        item.HasVoucher = false;
                    }

                    if (uploaded.Any())
                    {
                        dtos.AddRange(uploaded);
                    }
                    
                    #endregion

                }
            }

            SetSupplierInvoiceTexts(dtos, hasCurrencyPermission);

            AddIdentification(dtos);
            //Filter on date
            if (selectionDate != null)
                dtos = dtos.Where(i => i.InvoiceDate == null || i.InvoiceDate >= selectionDate).ToList();

            return dtos;
        }

        public List<SupplierInvoiceIncomingHallGridDTO> GetSupplierInvoiceIncomingHallGrid(int actorCompanyId)
        {
            using (var entities = new CompEntities())
            {
                return this.GetSupplierInvoiceIncomingHallGrid(entities, actorCompanyId);
            }
        }

        public List<SupplierInvoiceIncomingHallGridDTO> GetSupplierInvoiceIncomingHallGrid(CompEntities entities, int actorCompanyId)
        {
            List<SupplierInvoiceIncomingHallGridDTO> dtos = new List<SupplierInvoiceIncomingHallGridDTO>();

			#region Prereq
			bool hasPermission = FeatureManager.HasRolePermission(Feature.Economy_Supplier_Invoice_Incoming, Permission.Readonly, base.RoleId, actorCompanyId);
            if (!hasPermission)
                return new List<SupplierInvoiceIncomingHallGridDTO>();

			int? baseSysCurrencyId = CountryCurrencyManager.GetCompanyBaseSysCurrencyId(entities, actorCompanyId);
            if (!baseSysCurrencyId.HasValue)
                baseSysCurrencyId = 0;

            int langId = GetLangId();
            var supplierInvoiceHallSource = base.GetTermGroupDict(TermGroup.SupplierInvoiceSource, langId);
            var supplierInvoiceHallState = base.GetTermGroupDict(TermGroup.SupplierInvoiceState, langId);
            bool supplierInvoiceHallSourcesExists = supplierInvoiceHallSource.Count > 0;
			bool supplierInvoiceHallStatesExists = supplierInvoiceHallState.Count > 0;

			bool hasCurrencyPermission = FeatureManager.HasRolePermission(Feature.Economy_Supplier_Invoice_Status_Foreign, Permission.Readonly, base.RoleId, actorCompanyId);
            bool hasAttestFlowPermission = FeatureManager.HasRolePermission(
                Feature.Economy_Supplier_Invoice_AttestFlow, 
                Permission.Readonly, 
                base.RoleId, 
                actorCompanyId);
            #endregion

            #region Scanning
            var scannedInvoicesQuery = (from e in entities.ScanningEntryView
                                       join s in this.GetSupplierInvoiceQuery(entities, false, true) on e.InvoiceId equals s.InvoiceId into siJoin
                                       from si in siJoin.DefaultIfEmpty()
                                       where e.ActorCompanyId == actorCompanyId &&
                                       e.State == (int)SoeEntityState.Active &&
                                       e.MessageType == (int)TermGroup_ScanningMessageType.SupplierInvoice
                                       select new SupplierInvoiceIncomingHallGridDTO
                                       {
                                           InvoiceId = e.InvoiceId ?? 0,
                                           InvoiceNr = e.InvoiceNr,
                                           BillingTypeId = (TermGroup_BillingType)(si != null ? si.BillingType : e.BillingType),
                                           SupplierNr = (si != null ? si.Actor.Supplier.SupplierNr : e.SupplierNr),
                                           SupplierName = (si != null ? si.Actor.Supplier.SupplierNr : e.SupplierName),
                                           SupplierId = (si != null ? si.Actor.Supplier.ActorSupplierId : e.SupplierId ?? 0),
                                           TotalAmount = (si != null ? si.TotalAmount : e.Sum),
                                           TotalAmountCurrency = (si != null ? si.TotalAmountCurrency : e.SumCurrency),
                                           VatAmount = (si != null ? si.VATAmount : e.SumVat),
										   VatAmountCurrency = (si != null ? si.VATAmountCurrency : e.SumVatCurrency),
										   InternalText = (si != null ? si.Origin.Description : ""),
                                           InvoiceDate = (si != null ? si.InvoiceDate : e.InvoiceDate ?? null),
                                           DueDate = (si != null ? si.DueDate : e.DueDate ?? null),
                                           Created = (si != null ? si.Created : e.Created ?? null),
                                           EdiEntryId = e.EdiEntryId,
                                           EdiType = e.Type,
                                           HasPDF = e.HasPdf || e.UsesDataStorage,
                                           ScanningEntryId = e.ScanningEntryId,
                                           InvoiceState = (si != null ? TermGroup_SupplierInvoiceStatus.InProgress : TermGroup_SupplierInvoiceStatus.New),
                                           BlockPayment = si.BlockPayment,
                                           UnderInvestigation = (si != null ? si.UnderInvestigation : e.UnderInvestigation),
                                           SysCurrencyId = e.SysCurrencyId,
                                           AttestGroupId = si.AttestGroupId,
                                           SupplierInvoiceType = TermGroup_SupplierInvoiceType.Scanning,
                                           OriginStatus = SoeOriginStatus.None,
                                       });
            var scannedInvoices = scannedInvoicesQuery.ToList<SupplierInvoiceIncomingHallGridDTO>();

            scannedInvoices.ForEach(r =>
            {
                r.InvoiceSource = TermGroup_SupplierInvoiceSource.Interpreted;
                r.InvoiceSourceName = supplierInvoiceHallSourcesExists ? supplierInvoiceHallSource[(int)TermGroup_SupplierInvoiceSource.Interpreted] : "";
                r.InvoiceStateName = supplierInvoiceHallStatesExists ? supplierInvoiceHallState[(int)r.InvoiceState] : "";
            });
            dtos.AddRange(scannedInvoices);

            #endregion

            #region EDI

            var ediInvoices = (from e in entities.EdiEntryView
                               join s in this.GetSupplierInvoiceQuery(entities, false, true) on e.InvoiceId equals s.InvoiceId into siJoin
                               from si in siJoin.DefaultIfEmpty()
                               where e.ActorCompanyId == actorCompanyId &&
                                    e.Type == (int)TermGroup_EdiMessageType.SupplierInvoice &&
                                   !String.IsNullOrEmpty(e.InvoiceNr) &&
                                   !e.InvoiceId.HasValue &&
                                   e.SupplierId.HasValue && e.SupplierId.Value > 0 &&
                                   e.MessageType == (int)TermGroup_EdiMessageType.SupplierInvoice &&
                                   e.InvoiceStatus == (int)TermGroup_EDIInvoiceStatus.Unprocessed &&
                                   (e.Status == (int)TermGroup_EDIStatus.UnderProcessing || e.Status == (int)TermGroup_EDIStatus.Processed) &&
                                   e.State == (int)SoeEntityState.Active
                               select new SupplierInvoiceIncomingHallGridDTO
                               {
                                   InvoiceId = e.InvoiceId ?? 0,
                                   InvoiceNr = e.InvoiceNr,
                                   BillingTypeId = (TermGroup_BillingType)(si != null ? si.BillingType : e.BillingType),
                                   SupplierNr = (si != null ? si.Actor.Supplier.SupplierNr : e.SupplierNr),
                                   SupplierName = (si != null ? si.Actor.Supplier.Name : e.SupplierName),
                                   SupplierId = (si != null ? si.Actor.Supplier.ActorSupplierId : e.SupplierId ?? 0),
                                   TotalAmount = (si != null ? si.TotalAmount : e.Sum),
                                   TotalAmountCurrency = (si != null ? si.TotalAmountCurrency : e.SumCurrency),
								   VatAmount = (si != null ? si.VATAmount : e.SumVat),
								   VatAmountCurrency = (si != null ? si.VATAmountCurrency : e.SumVatCurrency),
								   InternalText = (si != null ? si.Origin.Description : ""),
                                   InvoiceDate = (si != null ? si.InvoiceDate : e.InvoiceDate ?? null),
                                   DueDate = (si != null ? si.DueDate : e.DueDate ?? null),
                                   Created = (si != null ? si.Created : e.Created ?? null),                                   
                                   EdiEntryId = e.EdiEntryId,
                                   EdiType = e.Type,
                                   HasPDF = e.HasPdf,
                                   ScanningEntryId = 0,
                                   InvoiceState = (si != null ? TermGroup_SupplierInvoiceStatus.InProgress : TermGroup_SupplierInvoiceStatus.New),
                                   BlockPayment = si.BlockPayment,
                                   SysCurrencyId = e.SysCurrencyId,
                                   AttestGroupId = si.AttestGroupId,
                                   SupplierInvoiceType = TermGroup_SupplierInvoiceType.EDI,
                                   OriginStatus = SoeOriginStatus.None,
                               }).ToList<SupplierInvoiceIncomingHallGridDTO>();

            ediInvoices.ForEach(r => 
            {
                r.InvoiceSource = TermGroup_SupplierInvoiceSource.EDI;
                r.InvoiceSourceName = supplierInvoiceHallSourcesExists ? supplierInvoiceHallSource[(int)TermGroup_SupplierInvoiceSource.EDI] : "";
                r.InvoiceStateName = supplierInvoiceHallStatesExists ? supplierInvoiceHallState[(int)r.InvoiceState] : "";
            });
            dtos.AddRange(ediInvoices);
            #endregion

            #region FInvoice

            var fInvoices = (from e in entities.FinvoiceEntryView
                             join s in this.GetSupplierInvoiceQuery(entities, false, true) on e.InvoiceId equals s.InvoiceId into siJoin
                             from si in siJoin.DefaultIfEmpty()
                             where e.ActorCompanyId == actorCompanyId &&
                                    e.State == (int)SoeEntityState.Active &&
                                    e.MessageType == (int)TermGroup_EdiMessageType.SupplierInvoice &&
                                     e.InvoiceStatus == (int)TermGroup_EDIInvoiceStatus.Unprocessed &&
                                   (e.Status == (int)TermGroup_EDIStatus.UnderProcessing || e.Status == (int)TermGroup_EDIStatus.Processed) &&
                                   e.State == (int)SoeEntityState.Active
                             select new SupplierInvoiceIncomingHallGridDTO
                             {
                                 InvoiceId = e.InvoiceId ?? 0,
                                 InvoiceNr = e.InvoiceNr,
                                 BillingTypeId = (TermGroup_BillingType)(si != null ? si.BillingType : e.BillingType),
                                 SupplierNr = (si != null ? si.Actor.Supplier.SupplierNr : e.SupplierNr),
                                 SupplierName = (si != null ? si.Actor.Supplier.Name : e.SupplierName),
                                 SupplierId = (si != null ? si.Actor.Supplier.ActorSupplierId : e.SupplierId ?? 0),
                                 TotalAmount = (si != null ? si.TotalAmount : e.Sum),
                                 TotalAmountCurrency = (si != null ? si.TotalAmountCurrency : e.SumCurrency),
								 VatAmount = (si != null ? si.VATAmount : e.SumVat),
								 VatAmountCurrency = (si != null ? si.VATAmountCurrency : e.SumVatCurrency),
								 InternalText = (si != null ? si.Origin.Description : ""),
                                 InvoiceDate = (si != null ? si.InvoiceDate : e.InvoiceDate ?? null),
                                 DueDate = (si != null ? si.DueDate : e.DueDate ?? null),
                                 Created = (si != null ? si.Created : e.Created ?? null),
                                 EdiEntryId = e.EdiEntryId,
                                 EdiType = e.Type,
                                 HasPDF = e.HasPdf,
                                 ScanningEntryId = 0,
                                 InvoiceState = (si != null ? TermGroup_SupplierInvoiceStatus.InProgress : TermGroup_SupplierInvoiceStatus.New),
                                 BlockPayment = si.BlockPayment,
                                 UnderInvestigation = (si != null ? si.UnderInvestigation : e.UnderInvestigation),
                                 SysCurrencyId = e.SysCurrencyId,
                                 AttestGroupId = si.AttestGroupId,
                                 SupplierInvoiceType = TermGroup_SupplierInvoiceType.Finvoice,
                                 OriginStatus = SoeOriginStatus.None,
                             }).ToList<SupplierInvoiceIncomingHallGridDTO>();

            fInvoices.ForEach(r =>
            {
                r.InvoiceSource = TermGroup_SupplierInvoiceSource.FInvoice;
                r.InvoiceSourceName = supplierInvoiceHallSourcesExists ? supplierInvoiceHallSource[(int)TermGroup_SupplierInvoiceSource.FInvoice] : "";
                r.InvoiceStateName = supplierInvoiceHallStatesExists ? supplierInvoiceHallState[(int)r.InvoiceState]  : "";
            });
            dtos.AddRange(fInvoices);

            #endregion

            #region E-Invoice
            var eInvoices = (from io in entities.SupplierInvoiceHeadIO
                             join s in this.GetSupplierInvoiceQuery(entities, false, true) on io.InvoiceId equals s.InvoiceId into siJoin
                             join su in entities.Supplier on io.SupplierId equals su.ActorSupplierId into supJoin
                             from si in siJoin.DefaultIfEmpty()
                             from sup in supJoin.DefaultIfEmpty()
                             where io.ActorCompanyId == actorCompanyId &&
                                 io.Type == (int)TermGroup_IOType.Inexchange &&
                                 io.State == (int)SoeEntityState.Active
                             select new SupplierInvoiceIncomingHallGridDTO
                             {
                                 InvoiceId = io.InvoiceId ?? 0,
                                 InvoiceNr = (si != null ? si.InvoiceNr : io.SupplierInvoiceNr),
                                 BillingTypeId = (TermGroup_BillingType)(si != null ? si.BillingType : io.BillingType??0),
                                 SupplierNr = (si != null ? si.Actor.Supplier.SupplierNr : io.SupplierNr),
                                 SupplierName = (si != null ? si.Actor.Supplier.Name : (sup != null ? sup.Name : "")),
                                 SupplierId = (si != null && si.Actor != null ? si.Actor.Supplier.ActorSupplierId : io.SupplierId ?? 0),
                                 TotalAmount = (si != null ? si.TotalAmount : io.TotalAmount ?? 0.0m),
                                 TotalAmountCurrency = (si != null ? si.TotalAmountCurrency : io.TotalAmountCurrency ?? 0.0m),
								 VatAmount = (si != null ? si.VATAmount : 0.0m),
								 VatAmountCurrency = (si != null ? si.VATAmountCurrency : 0.0m),
								 InternalText = (si != null ? si.Origin.Description : ""),
                                 InvoiceDate = (si != null ? si.InvoiceDate : io.InvoiceDate ?? null),
                                 DueDate = (si != null ? si.DueDate : io.DueDate ?? null),
                                 Created = (si != null ? si.Created : io.Created ?? null),
                                 EdiEntryId = 0,
                                 EdiType = 0,
                                 HasPDF = false,
                                 ScanningEntryId = 0,
                                 SupplierInvoiceHeadIOId = io.SupplierInvoiceHeadIOId,
                                 InvoiceState = (si != null ? TermGroup_SupplierInvoiceStatus.InProgress : TermGroup_SupplierInvoiceStatus.New),
                                 BlockPayment = si.BlockPayment,
                                 UnderInvestigation = (si != null ? si.UnderInvestigation : false),
                                 SysCurrencyId = si.Currency.SysCurrencyId,
                                 AttestGroupId = si.AttestGroupId,
                                 SupplierInvoiceType = si != null ? (TermGroup_SupplierInvoiceType)si.Type : TermGroup_SupplierInvoiceType.None,
                                 OriginStatus = SoeOriginStatus.None,
                             }).ToList<SupplierInvoiceIncomingHallGridDTO>();

            eInvoices.ForEach(r =>
            {
                r.InvoiceSource = TermGroup_SupplierInvoiceSource.EInvoice;
                r.InvoiceSourceName = supplierInvoiceHallSourcesExists ? supplierInvoiceHallSource[(int)TermGroup_SupplierInvoiceSource.EInvoice] : "";
                r.InvoiceStateName = supplierInvoiceHallSourcesExists ? supplierInvoiceHallState[(int)r.InvoiceState] : "";
            });
            dtos.AddRange(eInvoices);
            #endregion

            List<SupplierInvoiceIncomingHallGridDTO> result = dtos
                .Distinct().OrderByDescending(r => r.Created).ToList();


            AddSupplierInvoiceAttestGroupsConverted(
                result, hasAttestFlowPermission, actorCompanyId);

            result.ForEach((r) =>
            {
                if (hasCurrencyPermission && r.SysCurrencyId.HasValue)
                {
                    r.CurrencyCode = CountryCurrencyManager.GetCurrencyCode(
                        r.SysCurrencyId.Value);
                }
                else
                {
                    r.SysCurrencyId = null;
                }

            });

            return result;
        }

        public List<SupplierInvoiceGridDTO> GetFilteredSupplierInvoices(ExpandoObject filterModels)
        {
            List<SupplierInvoiceGridDTO> dtos = new List<SupplierInvoiceGridDTO>();
            var values = (IDictionary<string, object>)filterModels;
            bool loadOpen = values.ContainsKey("loadopen") ? (bool)values["loadopen"] : true;
            bool loadClosed = values.ContainsKey("loadclosed") ? (bool)values["loadclosed"] : false;
            using var entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();

            using (CompEntities entities = new CompEntities())
            {
                #region prereq

                int langId = GetLangId();
                int actorCompanyId = base.ActorCompanyId;
                int? baseSysCurrencyId = CountryCurrencyManager.GetCompanyBaseSysCurrencyId(entities, base.ActorCompanyId);
                if (!baseSysCurrencyId.HasValue)
                    baseSysCurrencyId = 0;
                bool hideVatWarning = SettingManager.GetBoolSetting(entities, SettingMainType.Company, (int)CompanySettingType.BillingHideVatWarnings, 0, base.ActorCompanyId, 0);
                bool closeInvoicesWhenTransferredToVoucher = SettingManager.GetBoolSetting(entities, SettingMainType.Company, (int)CompanySettingType.SupplierCloseInvoicesWhenTransferredToVoucher, 0, base.ActorCompanyId, 0);
                bool hasCurrencyPermission = FeatureManager.HasRolePermission(Feature.Economy_Supplier_Invoice_Status_Foreign, Permission.Readonly, base.RoleId, base.ActorCompanyId);

                List<GenericType> terms = base.GetTermGroupContent(TermGroup.SupplierInvoiceType);

                bool loadInvoices = true;
                bool loadScanning = true;
                bool loadEdi = true;
                bool loadUploaded = true;

                if (values.ContainsKey("typeName"))
                {
                    var types = values["typeName"] as List<object>;

                    if (types != null)
                    {
                        var typesInt = types.Select(x => Convert.ToInt32(x)).ToList();
                        loadInvoices = typesInt.Contains((int)TermGroup_SupplierInvoiceType.Invoice);
                        loadScanning = typesInt.Contains((int)TermGroup_SupplierInvoiceType.Scanning);
                        loadEdi = typesInt.Contains((int)TermGroup_SupplierInvoiceType.EDI);
                        loadUploaded = typesInt.Contains((int)TermGroup_SupplierInvoiceType.Uploaded);
                    }
                }

                #region Prereq

                var ediSourceTypes = base.GetTermGroupDict(TermGroup.EDISourceType, langId);
                var ediMessageTypes = base.GetTermGroupDict(TermGroup.EdiMessageType, langId);
                var billingTypes = base.GetTermGroupDict(TermGroup.InvoiceBillingType, langId);
                var invoiceStatuses = base.GetTermGroupDict(TermGroup.EDIInvoiceStatus, langId);

                #endregion

                #endregion

                if (loadInvoices)
                {
                    List<GetSupplierInvoices_Result> invoices;

                    if (!loadOpen && !loadClosed)
                    {
                        invoices = new List<GetSupplierInvoices_Result>();
                    }
                    else if (closeInvoicesWhenTransferredToVoucher && !(loadOpen && loadClosed))
                    {
                        invoices = entities.GetSupplierInvoices(base.ActorCompanyId, null, loadOpen, loadClosed, closeInvoicesWhenTransferredToVoucher).ToList();
                    }
                    else
                    {
                        invoices = entities.GetSupplierInvoices(base.ActorCompanyId, null, loadOpen, loadClosed, false).OrderBy(i => i.InvoiceSeqNr.HasValue ? i.InvoiceSeqNr : 0).ToList();

                        if (loadClosed && !loadOpen)
                        {
                            //Load only closed invoices
                            invoices = (from i in invoices
                                        where (i.Status == (int)SoeOriginStatus.Cancel ||
                                        (i.FullyPayed == true && (i.Status == (int)SoeOriginStatus.Voucher || i.Status == (int)SoeOriginStatus.Origin)))
                                        //orderby i.InvoiceSeqNr.HasValue, i.InvoiceSeqNr.HasValue ? i.InvoiceSeqNr.Value : 0
                                        select i).ToList();
                        }
                        else if (loadOpen && !loadClosed)
                        {
                            invoices = (from i in invoices
                                        where (i.Status != (int)SoeOriginStatus.Cancel &&
                                        !(i.FullyPayed == true && (i.Status == (int)SoeOriginStatus.Voucher || i.Status == (int)SoeOriginStatus.Origin)))
                                        //orderby i.InvoiceSeqNr.HasValue, i.InvoiceSeqNr.HasValue ? i.InvoiceSeqNr.Value : 0
                                        select i).ToList();
                        }
                    }

                    //Filter - billingtyp
                    if (values.ContainsKey("billingType"))
                    {
                        int billingType;
                        if (int.TryParse(values["billingType"].ToString(), out billingType))
                        {
                            invoices = invoices.Where(i => i.BillingTypeId == billingType).ToList();
                        }
                    }

                    //Filter currency
                    if (!hasCurrencyPermission && baseSysCurrencyId != 0)
                        invoices = invoices.Where(i => i.SysCurrencyId == baseSysCurrencyId).ToList();

                    //Filter - strings
                    if (values.ContainsKey("currentAttestUserName"))
                    {
                        var filterObject = values["currentAttestUserName"] as IDictionary<string, object>;
                        invoices = invoices.Where(i => i.CurrentAttestUsers.ToLower().Contains(filterObject["filter"].ToString().ToLower())).ToList();
                    }
                    if (values.ContainsKey("internalText"))
                    {
                        var filterObject = values["internalText"] as IDictionary<string, object>;
                        invoices = invoices.Where(i => i.InternalText.ToLower().Contains(filterObject["filter"].ToString().ToLower())).ToList();
                    }
                    if (values.ContainsKey("invoiceNr"))
                    {
                        var filterObject = values["invoiceNr"] as IDictionary<string, object>;
                        invoices = invoices.Where(i => i.InvoiceNr.ToLower().Contains(filterObject["filter"].ToString().ToLower())).ToList();
                    }
                    if (values.ContainsKey("seqNr"))
                    {
                        var filterObject = values["seqNr"] as IDictionary<string, object>;
                        invoices = invoices.Where(i => i.InvoiceSeqNr.HasValue && i.InvoiceSeqNr.ToString().StartsWith(filterObject["filter"].ToString().ToLower())).ToList();
                    }
                    if (values.ContainsKey("supplierName"))
                    {
                        var filterObject = values["supplierName"] as IDictionary<string, object>;
                        var supplierName = filterObject["filter"].ToString().ToLower();
                        invoices = invoices.Where(i => i.ActorNr.ToLower().Contains(supplierName) || i.ActorName.ToLower().Contains(supplierName)).ToList();
                    }

                    //Filter - collections
                    if (values.ContainsKey("attestGroupName"))
                    {
                        List<int> attestGroupIds = new List<int>();
                        foreach (object nr in values["attestGroupName"] as List<object>)
                        {
                            attestGroupIds.Add(Convert.ToInt32(nr));
                        }
                        invoices = invoices.Where(i => i.SupplierInvoiceAttestGroupId.HasValue && attestGroupIds.Contains(i.SupplierInvoiceAttestGroupId.Value)).ToList();

                    }
                    if (values.ContainsKey("attestStateName"))
                    {
                        bool noAttest = false;
                        bool rejected = false;
                        List<int> attestStateIds = new List<int>();
                        foreach (object nr in values["attestStateName"] as List<object>)
                        {
                            int attestStateId = Convert.ToInt32(nr);
                            if (attestStateId == -100)
                                noAttest = true;
                            else if (attestStateId == -200)
                                rejected = true;
                            else
                                attestStateIds.Add(Convert.ToInt32(nr));
                        }
                        invoices = invoices.Where(i => (i.SupplierInvoiceAttestStateId.HasValue && attestStateIds.Contains(i.SupplierInvoiceAttestStateId.Value)) || (noAttest ? !i.SupplierInvoiceAttestStateId.HasValue : false) || (rejected ? i.IsAttestRejected : false)).ToList();
                    }

                    if (values.ContainsKey("currencyCode"))
                    {
                        List<int> currencyCodeIds = new List<int>();
                        foreach (object nr in values["currencyCode"] as List<object>)
                        {
                            currencyCodeIds.Add(Convert.ToInt32(nr));
                        }
                        invoices = invoices.Where(i => currencyCodeIds.Contains(i.SysCurrencyId)).ToList();
                    }
                    if (values.ContainsKey("statusName"))
                    {
                        List<int> statuses = new List<int>();
                        foreach (object nr in values["statusName"] as List<object>)
                        {
                            statuses.Add(Convert.ToInt32(nr));
                        }
                        invoices = invoices.Where(i => statuses.Contains(i.Status)).ToList();
                    }

                    //Filter - dates
                    if (values.ContainsKey("dueDate"))
                    {
                        var item = (IDictionary<string, object>)values["dueDate"];
                        var type = item["type"].ToString();
                        var fromDate = item["dateFrom"] != null ? Convert.ToDateTime(item["dateFrom"]) : (DateTime?)null;
                        var toDate = item["dateTo"] != null ? Convert.ToDateTime(item["dateTo"]) : (DateTime?)null;
                        invoices = invoices.Where(i => i.DueDate != null && FilterOnDate(i.DueDate.Value, type, fromDate, toDate)).ToList();
                    }

                    if (values.ContainsKey("invoiceDate"))
                    {
                        var item = (IDictionary<string, object>)values["invoiceDate"];
                        var type = item["type"].ToString();
                        var fromDate = item["dateFrom"] != null ? Convert.ToDateTime(item["dateFrom"]) : (DateTime?)null;
                        var toDate = item["dateTo"] != null ? Convert.ToDateTime(item["dateTo"]) : (DateTime?)null;
                        invoices = invoices.Where(i => i.InvoiceDate != null && FilterOnDate(i.InvoiceDate.Value, type, fromDate, toDate)).ToList();
                    }

                    if (values.ContainsKey("payDate"))
                    {
                        var item = (IDictionary<string, object>)values["payDate"];
                        var type = item["type"].ToString();
                        var fromDate = item["dateFrom"] != null ? Convert.ToDateTime(item["dateFrom"]) : (DateTime?)null;
                        var toDate = item["dateTo"] != null ? Convert.ToDateTime(item["dateTo"]) : (DateTime?)null;
                        invoices = invoices.Where(i => i.PayDate != null && FilterOnDate(i.PayDate.Value, type, fromDate, toDate)).ToList();
                    }

                    //Filter - amounts
                    if (values.ContainsKey("payAmount"))
                    {
                        var item = (IDictionary<string, object>)values["payAmount"];
                        var type = item["type"].ToString();
                        invoices = invoices.Where(i => FilterOnAmount(i.InvoicePayAmount, type, Convert.ToDecimal(item["filter"]))).ToList();
                    }

                    if (values.ContainsKey("payAmountCurrency"))
                    {
                        var item = (IDictionary<string, object>)values["payAmountCurrency"];
                        var type = item["type"].ToString();
                        invoices = invoices.Where(i => FilterOnAmount(i.InvoicePayAmountCurrency, type, Convert.ToDecimal(item["filter"]))).ToList();
                    }

                    if (values.ContainsKey("totalAmount"))
                    {
                        var item = (IDictionary<string, object>)values["totalAmount"];
                        var type = item["type"].ToString();
                        invoices = invoices.Where(i => FilterOnAmount(i.InvoiceTotalAmount, type, Convert.ToDecimal(item["filter"]))).ToList();
                    }

                    if (values.ContainsKey("totalAmountCurrency"))
                    {
                        var item = (IDictionary<string, object>)values["totalAmountCurrency"];
                        var type = item["type"].ToString();
                        invoices = invoices.Where(i => FilterOnAmount(i.InvoiceTotalAmountCurrency, type, Convert.ToDecimal(item["filter"]))).ToList();
                    }

                    if (values.ContainsKey("totalAmountExVat"))
                    {
                        var item = (IDictionary<string, object>)values["totalAmountExVat"];
                        var type = item["type"].ToString();
                        invoices = invoices.Where(i => FilterOnAmount((i.InvoiceTotalAmount - i.VATAmount), type, Convert.ToDecimal(item["filter"]))).ToList();
                    }

                    foreach (GetSupplierInvoices_Result result in invoices)
                    {
                        SupplierInvoiceGridDTO dto = result.ToGridDTO(hideVatWarning, true, closeInvoicesWhenTransferredToVoucher, (baseSysCurrencyId != 0 && result.SysCurrencyId != baseSysCurrencyId));
                        dto.Type = (int)TermGroup_SupplierInvoiceType.Invoice;

                        GenericType type = terms.FirstOrDefault(t => t.Id == (int)TermGroup_SupplierInvoiceType.Invoice);
                        if (type != null)
                            dto.TypeName = type.Name;

                        dtos.Add(dto);
                    }

                    SetSupplierInvoiceTexts(dtos, hasCurrencyPermission);

                    //Add attest states (and attestant name later)
                    AddSupplierInvoiceAttestStatesConverted(entities, dtos);

                    //Add attest groups
                    AddSupplierInvoiceAttestGroupsConverted(dtos);
                }

                //Do not load finvoice where since there are special grid for it...

                if (loadScanning)
                {
                    #region Scanning

                    var scanningItems = (from e in entitiesReadOnly.ScanningEntryView
                                         where e.ActorCompanyId == actorCompanyId &&
                                         e.State == (int)SoeEntityState.Active &&
                                         e.MessageType == (int)TermGroup_EdiMessageType.SupplierInvoice &&
                                         e.InvoiceId == null
                                         select e);

                    if (scanningItems.Any())
                    {
                        var rowItems = EdiManager.GetScanningEntryRowItemsByCompany(entities, base.ActorCompanyId, scanningItems.Select(s => s.ScanningEntryId).ToList());

                        foreach (var entry in scanningItems)
                        {
                            // Check interpretation
                            var rowItemsForEntry = rowItems.Where(i => i.ScanningEntryId == entry.ScanningEntryId).ToList();

                            SupplierInvoiceGridDTO dto = entry.ToConvertedScanningSupplierGridDTO(hideVatWarning, true, (baseSysCurrencyId != 0 && entry.SysCurrencyId != baseSysCurrencyId));
                            dto.RoundedInterpretation = EdiManager.GetScanningEntryRoundedInterpretation(rowItemsForEntry);
                            dto.CurrencyCode = CountryCurrencyManager.GetCurrencyCode(dto.SysCurrencyId);
                            dto.SourceTypeName = dto.Type != 0 ? ediSourceTypes[dto.Type] : "";
                            dto.EdiMessageTypeName = dto.EdiMessageType != 0 ? ediMessageTypes[dto.EdiMessageType] : "";
                            dto.BillingTypeName = dto.BillingTypeId != 0 ? billingTypes[dto.BillingTypeId] : "";
                            dto.StatusName = dto.Status != 0 ? invoiceStatuses[dto.Status] : "";
                            dto.Type = (int)TermGroup_SupplierInvoiceType.Scanning;

                            GenericType type = terms.FirstOrDefault(t => t.Id == (int)TermGroup_SupplierInvoiceType.Scanning);
                            if (type != null)
                                dto.TypeName = type.Name;

                            dtos.Add(dto);
                        }
                    }
                    #endregion
                }

                if (loadEdi)
                {
                    #region EDI

                    var items = (from e in entitiesReadOnly.EdiEntryView
                                 where e.ActorCompanyId == actorCompanyId &&
                                 e.Type == (int)TermGroup_EdiMessageType.SupplierInvoice &&
                                !String.IsNullOrEmpty(e.InvoiceNr) &&
                                !e.InvoiceId.HasValue &&
                                e.SupplierId.HasValue && e.SupplierId.Value > 0 &&
                                e.MessageType == (int)TermGroup_EdiMessageType.SupplierInvoice &&
                                e.InvoiceStatus == (int)TermGroup_EDIInvoiceStatus.Unprocessed &&
                                (e.Status == (int)TermGroup_EDIStatus.UnderProcessing || e.Status == (int)TermGroup_EDIStatus.Processed)
                                 select e);

                    foreach (var entry in items)
                    {
                        SupplierInvoiceGridDTO dto = entry.ToConvertedEdiSupplierGridDTO(hideVatWarning, true, (baseSysCurrencyId != 0 && entry.SysCurrencyId != baseSysCurrencyId));
                        dto.Type = (int)TermGroup_SupplierInvoiceType.EDI;
                        dto.CurrencyCode = CountryCurrencyManager.GetCurrencyCode(dto.SysCurrencyId);
                        dto.SourceTypeName = dto.Type != 0 ? ediSourceTypes[dto.Type] : "";
                        dto.EdiMessageTypeName = dto.EdiMessageType != 0 ? ediMessageTypes[dto.EdiMessageType] : "";
                        dto.BillingTypeName = dto.BillingTypeId != 0 ? billingTypes[dto.BillingTypeId] : "";
                        dto.StatusName = dto.Status != 0 ? invoiceStatuses[dto.Status] : "";

                        GenericType type = terms.FirstOrDefault(t => t.Id == (int)TermGroup_SupplierInvoiceType.EDI);
                        if (type != null)
                            dto.TypeName = type.Name;

                        dtos.Add(dto);
                    }
                    #endregion
                }

                if (loadUploaded)
                {
                    #region Uploaded images                    

                    var uploaded = (from i in entitiesReadOnly.Invoice.OfType<SupplierInvoice>()
                                .Include("Origin")
                                .Include("Currency")
                                    where i.Origin.ActorCompanyId == ActorCompanyId &&
                                    i.IsTemplate == false &&
                                    i.State == (int)SoeEntityState.Active &&
                                    i.Type == (int)TermGroup_SupplierInvoiceType.Uploaded
                                    select i).ToList();

                    foreach (var item in uploaded)
                    {
                        SupplierInvoiceGridDTO dto = new SupplierInvoiceGridDTO()
                        {
                            SupplierInvoiceId = item.InvoiceId,
                            Type = (int)TermGroup_SupplierInvoiceType.Uploaded,
                            OwnerActorId = item.ActorId != null ? (int)item.ActorId : 0,
                            SeqNr = item.SeqNr.HasValue ? item.SeqNr.Value.ToString() : String.Empty,
                            InvoiceNr = item.InvoiceNr,
                            BillingTypeId = item.BillingType,
                            Status = item.Status,
                            StatusName = item.StatusName,
                            SupplierName = item.ActorNr + " " + item.ActorName,
                            SupplierId = item.ActorId != null ? (int)item.ActorId : 0,
                            TotalAmount = item.TotalAmount,
                            TotalAmountText = item.TotalAmount.ToString(),
                            TotalAmountCurrency = 0,
                            TotalAmountCurrencyText = String.Empty,
                            VATAmount = 0,
                            VATAmountCurrency = 0,
                            PayAmount = 0,
                            PayAmountText = string.Empty,
                            PayAmountCurrency = 0,
                            PayAmountCurrencyText = String.Empty,
                            PaidAmount = 0,
                            PaidAmountCurrency = 0,
                            VatType = item.VatType,
                            SysCurrencyId = item.CurrencyId,
                            CurrencyCode = item.Currency.Code,
                            InvoiceDate = item.InvoiceDate,
                            DueDate = item.DueDate,
                            PayDate = null,
                            CurrentAttestUserName = string.Empty,
                            AttestStateId = null,
                            AttestGroupId = 0,
                            FullyPaid = item.FullyPayed,
                            PaymentStatuses = string.Empty,
                            StatusIcon = item.StatusIcon,
                            HasVoucher = false,
                            CurrencyRate = item.CurrencyRate,
                            InternalText = item.InternalDescription,
                            TimeDiscountDate = item.TimeDiscountDate,
                            TimeDiscountPercent = item.TimeDiscountPercent
                        };

                        GenericType invoiceType = terms.FirstOrDefault(t => t.Id == (int)TermGroup_SupplierInvoiceType.Uploaded);
                        if (invoiceType != null)
                            dto.TypeName = invoiceType.Name;

                        dtos.Add(dto);
                    }
                    #endregion
                }


                AddIdentification(dtos);

                return dtos;

            }
        }

        public bool FilterOnDate(DateTime dateToFilterOn, string type, DateTime? fromDate, DateTime? toDate)
        {
            //Filter
            switch (type)
            {
                case "equals":
                    return dateToFilterOn == fromDate;
                case "greatherThan":
                    return dateToFilterOn > fromDate;
                case "lessThan":
                    return dateToFilterOn < fromDate;
                case "notEqual":
                    return dateToFilterOn != fromDate;
                case "inRange":
                    return fromDate <= dateToFilterOn && toDate >= dateToFilterOn;
                default:
                    return false;
            }
        }

        public bool FilterOnAmount(decimal amountToFilterOn, string type, decimal compareAmount)
        {
            //Filter
            switch (type)
            {
                case "equals":
                    return amountToFilterOn == compareAmount;
                case "notEqual":
                    return amountToFilterOn != compareAmount;
                case "startsWith":
                    return amountToFilterOn.ToString().StartsWith(compareAmount.ToString());
                case "endsWith":
                    return amountToFilterOn.ToString().EndsWith(compareAmount.ToString());
                case "contains":
                    return amountToFilterOn.ToString().Contains(compareAmount.ToString());
                case "notContains":
                    return !amountToFilterOn.ToString().Contains(compareAmount.ToString());
                default:
                    return false;
            }
        }

        public int GetScanningUnprocessedCount()
        {
            int count = 0;
            using (var entities = new CompEntities())
            {
                var date = DateTime.Now.AddHours(-3);
                count = (from e in entities.ScanningEntryView
                         where e.ActorCompanyId == ActorCompanyId &&
                                 e.State == (int)SoeEntityState.Active &&
                                 e.Status == (int)TermGroup_ScanningStatus.Unprocessed &&
                                 e.Created < date
                         select e).Count();
            }
            return count;
        }
        public List<SupplierPaymentGridDTO> GetSupplierPaymentsForGrid(SoeOriginStatusClassification classification, TermGroup_ChangeStatusGridAllItemsSelection? allItemsSelection)
        {
            return GetSupplierPaymentsForGrid(classification, allItemsSelection, base.ActorCompanyId);
        }

        public List<SupplierPaymentGridDTO> GetSupplierPaymentsForGrid(SoeOriginStatusClassification classification, TermGroup_ChangeStatusGridAllItemsSelection? allItemsSelection, int actorCompanyId)
        {
            var dtos = new List<SupplierPaymentGridDTO>();
            string statusList;

            using (var entities = new CompEntities())
            {
                #region prereq

                //Variables
                int? baseSysCurrencyId = CountryCurrencyManager.GetCompanyBaseSysCurrencyId(entities, actorCompanyId);
                if (!baseSysCurrencyId.HasValue)
                    baseSysCurrencyId = 0;

                bool hasCurrencyPermission = FeatureManager.HasRolePermission(Feature.Economy_Supplier_Invoice_Status_Foreign, Permission.Readonly, base.RoleId, actorCompanyId);
                IEnumerable<int> closedAttestStates = AttestManager.GetClosedAttestStatesIds(entities, actorCompanyId, TermGroup_AttestEntity.SupplierInvoice);
                var showOnlyAttested = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.SupplierInvoicesShowOnlyAttestedAtUnpayed, 0, actorCompanyId, 0);

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

                var langId = base.GetLangId();
                var paymentStatuses = base.GetTermGroupDict(TermGroup.PaymentStatus, langId);
                var billingTypes = base.GetTermGroupDict(TermGroup.InvoiceBillingType, langId);

                #endregion
                /*
                CurrencyCode = e.CurrencyCode,
                StatusName = e.StatusName,
                BillingTypeName = e.BillingTypeName,*/
                switch (classification)
                {
                    case SoeOriginStatusClassification.SupplierPaymentsUnpayed:
                    case SoeOriginStatusClassification.SupplierPaymentsUnpayedForeign:
                        #region Unpaid

                        var invoices = entities.GetSupplierInvoices(actorCompanyId, null, false, false, false).ToList();

                        invoices = (from i in invoices
                                    where
                                    (i.Status == (int)SoeOriginStatus.Origin || i.Status == (int)SoeOriginStatus.Voucher) &&
                                    (i.FullyPayed == false) && (i.BlockPayment == false) && (i.SupplierBlockPayment == false) &&
                                    (i.InvoiceTotalAmount != 0)
                                    orderby i.InvoiceSeqNr.HasValue, i.InvoiceSeqNr.HasValue ? i.InvoiceSeqNr.Value : 0
                                    select i).ToList();

                        if (showOnlyAttested)
                        {
                            invoices = invoices.Where(i => i.SupplierInvoiceAttestStateId.HasValue && closedAttestStates.Contains((int)i.SupplierInvoiceAttestStateId)).ToList();
                        }

                        //Filter on date
                        if (selectionDate != null)
                            invoices = invoices.Where(i => i.InvoiceDate == null || i.InvoiceDate >= selectionDate).ToList();

                        //Filter currency
                        if (!hasCurrencyPermission && baseSysCurrencyId != 0)
                            invoices = invoices.Where(i => i.SysCurrencyId == baseSysCurrencyId).ToList();

                        dtos.AddRange(invoices.ToPaymentGridDTOs(false));
                        dtos = SetSupplierPaymentTexts(dtos, hasCurrencyPermission);
                        #endregion
                        break;
                    case SoeOriginStatusClassification.SupplierPaymentSuggestions:
                    case SoeOriginStatusClassification.SupplierPaymentSuggestionsForeign:
                        #region Payment suggestion
                        statusList = $"{(int)SoePaymentStatus.Pending},{(int)SoePaymentStatus.Verified},{(int)SoePaymentStatus.Error}";
                        var paymentsSuggestion = entities.GetSupplierPayments(actorCompanyId, (int)SoeOriginType.SupplierPayment, statusList).Where(i => i.IsInvoiceTemplate == false);

                        paymentsSuggestion = (from i in paymentsSuggestion
                                              where
                                                (i.HasVoucher == false) &&
                                                (i.PaymentIsSuggestion)
                                              orderby i.InvoiceSeqNr.HasValue, i.InvoiceSeqNr.HasValue ? i.InvoiceSeqNr.Value : 0
                                              select i);

                        //Filter on date - since filter is removed in grid this filtering shouldn't be done
                        /*if (selectionDate != null)
                            paymentsSuggestion = paymentsSuggestion.Where(i => i.InvoiceDate == null || i.InvoiceDate >= selectionDate).ToList();*/

                        //Filter currency
                        if (!hasCurrencyPermission && baseSysCurrencyId != 0)
                            paymentsSuggestion = paymentsSuggestion.Where(i => i.SysCurrencyId == baseSysCurrencyId);

                        foreach (var payment in paymentsSuggestion)
                        {
                            var dto = payment.ToGridDTO(false);

                            dto.BillingTypeName = dto.BillingTypeId != 0 ? billingTypes[dto.BillingTypeId] : "";
                            dto.StatusName = dto.Status != 0 ? paymentStatuses[dto.Status] : "";
                            dto.CurrencyCode = CountryCurrencyManager.GetCurrencyCode(dto.SysCurrencyId);

                            dtos.Add(dto);
                        }

                        #endregion
                        break;
                    case SoeOriginStatusClassification.SupplierPaymentsPayed:
                    case SoeOriginStatusClassification.SupplierPaymentsPayedForeign:
                        #region Paid

                        statusList = $"{(int)SoePaymentStatus.Pending},{(int)SoePaymentStatus.Verified},{(int)SoePaymentStatus.Error}";
                        var paymentsPaid = entities.GetSupplierPayments(actorCompanyId, (int)SoeOriginType.SupplierPayment, statusList).Where(i => i.IsInvoiceTemplate == false);

                        paymentsPaid = (from i in paymentsPaid
                                        where
                                        (i.HasVoucher == false) &&
                                        (i.PaymentIsSuggestion == false)
                                        orderby i.InvoiceSeqNr.HasValue, i.InvoiceSeqNr.HasValue ? i.InvoiceSeqNr.Value : 0
                                        select i);

                        //Filter on date
                        if (selectionDate != null)
                            paymentsPaid = paymentsPaid.Where(i => i.InvoiceDate == null || i.InvoiceDate >= selectionDate).ToList();

                        //Filter currency
                        if (!hasCurrencyPermission && baseSysCurrencyId != 0)
                            paymentsPaid = paymentsPaid.Where(i => i.SysCurrencyId == baseSysCurrencyId);

                        foreach (var payment in paymentsPaid)
                        {
                            var dto = payment.ToGridDTO(false);

                            dto.BillingTypeName = dto.BillingTypeId != 0 ? billingTypes[dto.BillingTypeId] : "";
                            dto.StatusName = dto.Status != 0 ? paymentStatuses[dto.Status] : "";
                            dto.CurrencyCode = CountryCurrencyManager.GetCurrencyCode(dto.SysCurrencyId);

                            dtos.Add(dto);
                        }

                        #endregion
                        break;
                    case SoeOriginStatusClassification.SupplierPaymentsVoucher:
                    case SoeOriginStatusClassification.SupplierPaymentsVoucherForeign:
                        #region Voucher
                        statusList = $"{(int)SoePaymentStatus.Checked},{(int)SoePaymentStatus.Cancel}";
                        var paymentsVoucher = entities.GetSupplierPayments(actorCompanyId, (int)SoeOriginType.SupplierPayment, statusList).Where(i => i.IsInvoiceTemplate == false);

                        paymentsVoucher = (from i in paymentsVoucher
                                           where (i.OriginType == (int)SoeOriginType.SupplierPayment) &&
                                           ((i.HasVoucher == true && i.Status == (int)SoePaymentStatus.Checked) || (i.Status == (int)SoePaymentStatus.Cancel))
                                           orderby i.InvoiceSeqNr.HasValue, i.InvoiceSeqNr.HasValue ? i.InvoiceSeqNr.Value : 0
                                           select i);

                        //Filter on date
                        if (selectionDate != null)
                            paymentsVoucher = paymentsVoucher.Where(i => i.InvoiceDate == null || i.InvoiceDate >= selectionDate).ToList();

                        //Filter currency
                        if (!hasCurrencyPermission && baseSysCurrencyId != 0)
                            paymentsVoucher = paymentsVoucher.Where(i => i.SysCurrencyId == baseSysCurrencyId);

                        foreach (var payment in paymentsVoucher)
                        {
                            var dto = payment.ToGridDTO(false);

                            dto.BillingTypeName = dto.BillingTypeId != 0 ? billingTypes[dto.BillingTypeId] : "";
                            dto.StatusName = dto.Status != 0 ? paymentStatuses[dto.Status] : "";
                            dto.CurrencyCode = CountryCurrencyManager.GetCurrencyCode(dto.SysCurrencyId);

                            dtos.Add(dto);
                        }

                        #endregion
                        break;
                }


                AddIdentification(dtos);

                return dtos;
            }
        }

        public List<AttestWorkFlowOverviewGridDTO> GetAttestWorkFlowOverview(SoeOriginStatusClassification classification, TermGroup_ChangeStatusGridAllItemsSelection? allItemsSelection)
        {
            List<AttestWorkFlowOverviewGridDTO> dtos = new List<AttestWorkFlowOverviewGridDTO>();

            #region Prereq

            List<CompCurrency> currencies = CountryCurrencyManager.GetCompCurrencies(ActorCompanyId, false);

            DateTime now = DateTime.Now.Date;

            #endregion

            DateTime selectionDate = new DateTime(1999, 1, 1);
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

            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            var invoices = entitiesReadOnly.GetSupplierInvoicesForAttest(ActorCompanyId, (int)classification, selectionDate);

            foreach (var invoice in invoices)
            {
                // Create dto
                var dto = new AttestWorkFlowOverviewGridDTO
                {
                    OwnerActorId = invoice.OwnerActorId,
                    InvoiceId = invoice.InvoiceId,
                    InvoiceNr = invoice.InvoiceNr,
                    SeqNr = invoice.SeqNr,
                    SupplierName = invoice.SupplierName,
                    SupplierNr = invoice.SupplierNr,
                    ProjectNr = invoice.ProjectNr,
                    ReferenceOur = invoice.ReferenceOur,
                    TotalAmount = invoice.TotalAmountCurrency,
                    TotalAmountExVat = invoice.TotalAmountCurrency - invoice.VATAmountCurrency,
                    FullyPaid = invoice.FullyPayed,
                    InvoiceDate = invoice.InvoiceDate,
                    DueDate = invoice.DueDate,
                    VoucherDate = invoice.VoucherDate,
                    CostCentreId = invoice.DefaultDim2AccountId,
                    AttestStateId = invoice.AttestStateId,
                    AttestStateName = invoice.AttestStateName,
                    OrderNr = invoice.OrderNr,
                    PaidAmount = invoice.PaidAmount,
                    InternalDescription = invoice.OriginDescription,
                    Currency = currencies.Where(c => c.CurrencyId == invoice.CurrencyId).Select(c => c.Code).FirstOrDefault(),
                    //Internal accounts
                    DefaultDim2Id = invoice.DefaultDim2AccountId,
                    DefaultDim3Id = invoice.DefaultDim3AccountId,
                    DefaultDim4Id = invoice.DefaultDim4AccountId,
                    DefaultDim5Id = invoice.DefaultDim5AccountId,
                    DefaultDim6Id = invoice.DefaultDim6AccountId,
                    hasPicture = invoice.HasImage.HasValue && invoice.HasImage.Value == 1 ? true : false,
                    BlockPayment = invoice.InvoiceBlockPayment,
                    BlockReason = invoice.BlockReason,
                    LastPaymentDate = invoice.LastPaymentDate,
                };

                if (invoice.TimeDiscountDate.HasValue && invoice.TimeDiscountDate.Value > DateTime.Today)
                    dto.PayDate = invoice.TimeDiscountDate.Value;
                else
                    dto.PayDate = dto.DueDate;

                if (classification == SoeOriginStatusClassification.SupplierInvoicesAttestFlowMyClosed)
                {
                    dto.DefaultDim2Name = invoice.DefaultDim2NR + " " + invoice.DefaultDim2Name;
                    dto.DefaultDim3Name = invoice.DefaultDim3NR + " " + invoice.DefaultDim3Name;
                    dto.DefaultDim4Name = invoice.DefaultDim4NR + " " + invoice.DefaultDim4Name;
                    dto.DefaultDim5Name = invoice.DefaultDim5NR + " " + invoice.DefaultDim5Name;
                    dto.DefaultDim6Name = invoice.DefaultDim6NR + " " + invoice.DefaultDim6Name;
                }

                dtos.Add(dto);
            }

            // Filter items
            if (classification == SoeOriginStatusClassification.SupplierInvoicesAttestFlowMyActive)
            {
                var repUsers = (from ur in entitiesReadOnly.UserReplacement
                                where ur.ReplacementUserId == parameterObject.UserId &&
                                ur.StartDate <= now &&
                                ur.StopDate >= now &&
                                ur.Type == (int)UserReplacementType.AttestFlow &&
                                ur.State == (int)SoeEntityState.Active
                                select ur);

                List<int> replacementIds = repUsers.Select(x => x.OriginUserId).Distinct().ToList();
                dtos = FilterStatusClassificationSupplierInvoicesAttestFlow(dtos, replacementIds, 
                        filterMyActive: true, 
                        filterUserIsInRows: false,
                        filterUserHasHandled: false)
                    .ToList();
            }
            else if (classification == SoeOriginStatusClassification.SupplierInvoicesAttestFlowMyClosed)
            {
                bool showUnattestedInvoices = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.SupplierInvoicesShowNonAttestedInvoices, 0, ActorCompanyId, 0);

                AttestStateDTO attestStateClosed = AttestManager.GetAttestStateClosed(ActorCompanyId, TermGroup_AttestEntity.SupplierInvoice, SoeModule.Economy);
                int attestStateClosedId = attestStateClosed != null ? attestStateClosed.AttestStateId : 0;

                if (showUnattestedInvoices)
                {
                    /*
                     * Show unattested invoices means that invoices that have been attested by the user should be shown,
                     * even though the invoice hasn't been fully attested (e.g. there might be steps left that are unhandled).
                     */
                    List<int> nonAttested = FilterStatusClassificationSupplierInvoicesAttestFlow(dtos, new List<int>(), 
                            filterMyActive: true, 
                            filterUserIsInRows: false,
                            filterUserHasHandled: true)
                        .Where(v => v.AttestStateId != attestStateClosedId)
                        .Select(v => v.InvoiceId)
                        .ToList();
                    dtos = dtos.Where(v => !nonAttested.Contains(v.InvoiceId)).ToList();
                }
                else
                {
                    dtos = dtos.Where(v => v.AttestStateId == attestStateClosedId).ToList();
                }

                dtos = FilterStatusClassificationSupplierInvoicesAttestFlow(dtos, new List<int>(), 
                        filterMyActive: false, 
                        filterUserIsInRows: true,
                        filterUserHasHandled: true)
                    .ToList();
            }



            // Set overdue flag
            foreach (var groupCompany in dtos.GroupBy(i => i.OwnerActorId))
            {
                // Company setting must be fetched per company (when company groups are used)
                int attestFlowDueDays = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.SupplierInvoiceAttestFlowDueDays, 0, groupCompany.Key, 0);

                foreach (var item in groupCompany)
                {
                    if (item.DueDate.HasValue)
                        item.AttestFlowOverdued = DateTime.Today.AddDays(-attestFlowDueDays) >= item.DueDate.Value;
                }
            }

            return dtos.OrderBy(i => i.DueDate).ThenBy(i => i.SupplierName).ThenBy(i => i.InvoiceNr).ToList();

        }

        public SupplierInvoice GetSupplierInvoiceForPayment(int invoiceId, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Invoice.NoTracking();
            return (from i in entities.Invoice.OfType<SupplierInvoice>()
                    .Include("Currency")
                    .Include("Actor.Supplier")
                    .Include("Origin.OriginUser")
                    .Include("SupplierInvoiceRow.SupplierInvoiceAccountRow.AccountStd.Account.AccountMapping")
                    .Include("SupplierInvoiceRow.SupplierInvoiceAccountRow.AccountStd.Account")
                    .Include("SupplierInvoiceRow.SupplierInvoiceAccountRow.AccountInternal.Account.AccountDim")
                    where i.InvoiceId == invoiceId && 
                          i.Origin.ActorCompanyId == actorCompanyId
                    select i).FirstOrDefault();
        }

        public SupplierInvoiceSmallDTO GetSupplierInvoiceSmallByExternalId(CompEntities entities, int actorCompanyId, string externalId)
        {
            return (from i in entities.Invoice.OfType<SupplierInvoice>()
                    where i.Origin.ActorCompanyId == actorCompanyId && 
                          i.Origin.Type == (int)SoeOriginType.SupplierInvoice && 
                          i.ExternalId == externalId &&
                          i.State == (int)SoeEntityState.Active
                    select i)
                       .Select(EntityExtensions.GetSupplierInvoiceSmallDTO)
                       .FirstOrDefault();
        }

        public SupplierInvoiceSmallDTO GetSupplierInvoiceSmall(CompEntities entities, int invoiceId)
        {
            return (from i in entities.Invoice.OfType<SupplierInvoice>()
                    where 
                          i.Origin.Type == (int)SoeOriginType.SupplierInvoice &&
                          i.InvoiceId == invoiceId &&
                          i.State == (int)SoeEntityState.Active
                    select i)
                       .Select(EntityExtensions.GetSupplierInvoiceSmallDTO)
                       .FirstOrDefault();
        }

        public InvoiceTinyDTO GetSupplierInvoiceTiny(CompEntities entities, int invoiceId)
        {
            return (from i in entities.Invoice.OfType<SupplierInvoice>()
                    where
                          i.Origin.Type == (int)SoeOriginType.SupplierInvoice &&
                          i.InvoiceId == invoiceId &&
                          i.State == (int)SoeEntityState.Active
                    select i)
                       .Select(EntityExtensions.GetSupplierInvoiceTinyDTO)
                       .FirstOrDefault();
        }

        private IQueryable<SupplierInvoice> GetSupplierInvoiceQuery(CompEntities entities, bool loadOrigin = false, bool loadActor = false, bool loadVoucher = false, bool loadVoucherSeries = false, bool loadPaymentMethod = false, bool loadRows = false, bool loadProject = false)
        {
            // Always load Origin and Currency. Flag loadOrigin determines if entities related to Origin should be loaded
            IQueryable<SupplierInvoice> query = entities.Invoice.OfType<SupplierInvoice>();
            query = query.Include("Origin").Include("Currency");

            if (loadOrigin)
            {
                query = query.Include("Origin.OriginUser");
                query = query.Include("Origin.VoucherSeries");
                query = query.Include("Origin.Company");
            }

            if (loadActor)
            {
                query = query.Include("Actor.Supplier");
                query = query.Include("Actor.Contact");
            }

            if (loadVoucher)
            {
                query = query.Include("VoucherHead.VoucherRow");
                if (loadVoucherSeries)
                    query = query.Include("VoucherHead.VoucherSeries.VoucherSeriesType");
            }

            if (loadPaymentMethod)
                query = query.Include("PaymentMethod");

            if (loadRows)
            {
                query = query.Include("SupplierInvoiceRow.SupplierInvoiceAccountRow.AccountStd.Account.AccountMapping");
                query = query.Include("SupplierInvoiceRow.SupplierInvoiceAccountRow.AccountStd.Account");
                query = query.Include("SupplierInvoiceRow.SupplierInvoiceAccountRow.AccountStd.AccountBalance");
                query = query.Include("SupplierInvoiceRow.SupplierInvoiceAccountRow.AccountInternal.Account.AccountDim");
                query = query.Include("SupplierInvoiceRow.SupplierInvoiceAccountRow.User");
            }

            if (loadProject)
            {
                query = query.Include("Project");
            }


            return query;
        }

        public SupplierInvoice GetSupplierInvoice(int invoiceId, bool loadOrigin = false, bool loadActor = false, bool loadVoucher = false, bool loadVoucherSeries = false, bool loadPaymentMethod = false, bool loadInvoiceRow = false, bool loadInvoiceAccountRow = false, bool setAttestStateName = false, bool loadProjectRows = false, bool loadOrderRows = false, bool loadProject = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Invoice.NoTracking();
            return GetSupplierInvoice(entities, invoiceId, loadOrigin, loadActor, loadVoucher, loadVoucherSeries, loadPaymentMethod, loadInvoiceRow, loadInvoiceAccountRow, setAttestStateName, loadProjectRows, loadOrderRows, loadProject);
        }

        public SupplierInvoice GetSupplierInvoice(CompEntities entities, int invoiceId, bool loadOrigin = false, bool loadActor = false, bool loadVoucher = false, bool loadVoucherSeries = false, bool loadPaymentMethod = false, bool loadInvoiceRow = false, bool loadInvoiceAccountRow = false, bool setAttestStateName = false, bool loadProjectRows = false, bool loadOrderRows = false, bool loadProject = false)
        {
            IQueryable<SupplierInvoice> query = GetSupplierInvoiceQuery(entities, loadOrigin, loadActor, loadVoucher, loadVoucherSeries, loadPaymentMethod, loadInvoiceRow || loadInvoiceAccountRow, loadProject);

            SupplierInvoice invoice = (from i in query
                                       where i.InvoiceId == invoiceId &&
                                       i.State == (int)SoeEntityState.Active
                                       select i).FirstOrDefault();

            if (invoice == null || invoice.Origin.ActorCompanyId != ActorCompanyId)
                return null;


            // Set StatusName
            if (invoice.Origin != null)
                invoice.StatusName = GetText(invoice.Origin.Status, (int)TermGroup.OriginStatus);

            // Set AttestStateName
            if (setAttestStateName && invoice.AttestStateId.HasValue)
                invoice.AttestStateName = AttestManager.GetAttestStateName(entities, invoice.AttestStateId.Value);

            int actorCompanyId = base.ActorCompanyId;

            if (loadProjectRows)
                invoice.SupplierInvoiceProjectRows = ProjectManager.GetSupplierInvoiceProjectRows(entities, invoiceId, actorCompanyId);

            if (loadOrderRows)
                invoice.SupplierInvoiceOrderRows = GetSupplierInvoiceOrderRows(entities, actorCompanyId, invoiceId);

            if (invoice.OrderNr.HasValue && invoice.OrderNr.Value > 0)
            {
                invoice.Order = (from i in entities.Invoice.OfType<CustomerInvoice>()
                                    .Include("Origin")
                                    .Include("Actor.Customer")
                                 where i.SeqNr == invoice.OrderNr.Value &&
                                i.Origin.Type == (int)SoeOriginType.Order &&
                                i.Origin.ActorCompanyId == actorCompanyId &&
                                i.State == (int)SoeEntityState.Active
                                 select i).FirstOrDefault();
            }

            if (invoice.BlockPayment)
            {
                var invoiceText = (from t in entities.InvoiceText
                                   where t.InvoiceId == invoiceId &&
                                   t.Type == (int)InvoiceTextType.SupplierInvoiceBlockReason &&
                                   t.State == (int)SoeEntityState.Active
                                   orderby t.Created descending
                                   select t).FirstOrDefault();

                if (invoiceText != null)
                {
                    invoice.BlockReasonTextId = invoiceText.InvoiceTextId;
                    invoice.BlockReason = invoiceText.Text;
                }
            }

            return invoice;
        }

        public List<SupplierInvoiceOrderGridDTO> GetSupplierInvoicesLinkedToOrder(int invoiceId, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.CustomerInvoiceRow.NoTracking();
            return GetSupplierInvoicesLinkedToOrder(entities, invoiceId, actorCompanyId);
        }

        public List<SupplierInvoiceOrderGridDTO> GetSupplierInvoicesLinkedToOrder(CompEntities entities, int invoiceId, int actorCompanyId)
        {
            List<SupplierInvoiceOrderGridDTO> items = new List<SupplierInvoiceOrderGridDTO>();

            var rows = (from r in entities.CustomerInvoiceRow
                                             where r.SupplierInvoiceId.HasValue &&
                                                 r.CustomerInvoice.InvoiceId == invoiceId &&
                                                 r.CustomerInvoice.Origin.ActorCompanyId == actorCompanyId &&
                                                 r.CustomerInvoice.Origin.Type == (int)SoeOriginType.Order &&
                                                 r.State == (int)SoeEntityState.Active
                                             select r).Select(EntityExtensions.CustomerInvoiceRowToSupplierInvoiceOrderGridDTO).ToList();

            foreach (var row in rows)
            {
                var existing = items.FirstOrDefault(i => i.SupplierInvoiceId == row.SupplierInvoiceId);
                if (existing != null)
                {
                    existing.Amount += row.Amount;
                    if (!existing.IncludeImageOnInvoice && row.IncludeImageOnInvoice)
                        existing.IncludeImageOnInvoice = true;
                }
                else
                {
                    items.Add(row);
                }
            }

            return items;
        }

        public List<SupplierInvoiceOrderGridDTO> GetSupplierInvoiceRowsLinkedToOrder(CompEntities entities, int invoiceId, int actorCompanyId)
        {
            List<SupplierInvoiceOrderGridDTO> items = new List<SupplierInvoiceOrderGridDTO>();

            var rows = (from r in entities.SupplierInvoiceProductRow
                        where 
                            r.CustomerInvoiceRow.InvoiceId == invoiceId &&
                            r.SupplierInvoice.Origin.ActorCompanyId == actorCompanyId &&
                            r.State == (int)SoeEntityState.Active
                        select r).Select(EntityExtensions.SupplierInvoiceProductRowToSupplierInvoiceOrderGridDTO).ToList();

            foreach(var grouprow in rows.GroupBy(x => x.SupplierInvoiceId))
            {
                var first = grouprow.First();
                first.Amount = grouprow.Sum(x => x.Amount);
                first.IncludeImageOnInvoice = grouprow.Any(x => x.IncludeImageOnInvoice);
                items.Add(first);
            }

            return items;
        }

        public List<SupplierInvoiceOrderGridDTO> GetSupplierInvoicesLinkedToProject(int invoiceId, int projectId, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TimeCodeTransaction.NoTracking();
            return GetSupplierInvoicesLinkedToProject(entities, invoiceId, projectId, actorCompanyId);
        }

        public List<SupplierInvoiceOrderGridDTO> GetSupplierInvoicesLinkedToProject(CompEntities entities, int invoiceId, int projectId, int actorCompanyId)
        {
            List<SupplierInvoiceOrderGridDTO> items = new List<SupplierInvoiceOrderGridDTO>();

            var rows = (from tct in entities.TimeCodeTransaction
                        where tct.SupplierInvoiceId.HasValue &&
                            tct.CustomerInvoiceId == invoiceId &&
                            tct.ProjectId == projectId &&
                            //tct.TimeInvoiceTransaction != null && 
                            tct.Project.ActorCompanyId == actorCompanyId &&
                            tct.State == (int)SoeEntityState.Active
                        select tct).Select(EntityExtensions.TimeCodeTransactionToSupplierInvoiceOrderGridDTO);

            #region OLD 
            /*List<TimeCodeTransaction> timeCodeTransactions = (from tct in entitiesReadOnly.TimeCodeTransaction
                                                              .Include("TimeInvoiceTransaction.Employee.ContactPerson")
                                                              .Include("TimeInvoiceTransaction.TimeBlockDate")
                                                              .Include("TimeInvoiceTransaction.CustomerInvoiceRow")
                                                              .Include("SupplierInvoice.Origin")
                                                              .Include("SupplierInvoice.Actor.Supplier")
                                                              .Include("Project.Invoice")
                                                              where tct.SupplierInvoiceId.HasValue &&
                                                              tct.CustomerInvoiceId == invoiceId &&
                                                              //(!tct.CustomerInvoiceId.HasValue || tct.CustomerInvoiceId == invoiceId) && // Invoices only linked to project and not order excluded after discussion on 2020-03-23
                                                              tct.ProjectId == projectId &&
                                                              tct.Project.ActorCompanyId == actorCompanyId &&
                                                              tct.State == (int)SoeEntityState.Active
                                                              select tct).ToList();*/
            #endregion

            foreach (var row in rows)
            {
                var existing = items.FirstOrDefault(i => i.SupplierInvoiceId == row.SupplierInvoiceId);
                if (existing != null)
                {
                    if (!existing.IncludeImageOnInvoice && row.IncludeImageOnInvoice)
                        existing.IncludeImageOnInvoice = true;
                }
                else
                {
                    items.Add(row);
                }
            }

            return items;
        }

        public List<SupplierInvoiceOrderGridDTO> GetSupplierInvoicesTransferedToOrder(int invoiceId, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.CustomerInvoiceRow.NoTracking();
            return GetSupplierInvoicesTransferedToOrder(entities, invoiceId, actorCompanyId);
        }

        public List<SupplierInvoiceOrderGridDTO> GetSupplierInvoicesTransferedToOrder(CompEntities entities, int invoiceId, int actorCompanyId)
        {
            List<SupplierInvoiceOrderGridDTO> items = new List<SupplierInvoiceOrderGridDTO>();

            var rows = (from r in entities.CustomerInvoiceRow
                        where r.EdiEntryId.HasValue &&
                            r.CustomerInvoice.InvoiceId == invoiceId &&
                            r.CustomerInvoice.Origin.ActorCompanyId == actorCompanyId &&
                            r.CustomerInvoice.Origin.Type == (int)SoeOriginType.Order &&
                            r.State == (int)SoeEntityState.Active
                        select r).Select(EntityExtensions.CustomerInvoiceRowToSupplierInvoiceOrderGridDTO).ToList();

            var ediEntryIds = rows.Where(i => i.EdiEntryId.HasValue).Select(i => i.EdiEntryId);
            var entrys = (from e in entities.EdiEntry
                                    .Include("Invoice.Origin")
                                    .Include("Supplier")
                          where e.ActorCompanyId == actorCompanyId &&
                             e.MessageType == (int)TermGroup_EdiMessageType.SupplierInvoice &&
                             e.InvoiceId.HasValue &&
                             ediEntryIds.Contains(e.EdiEntryId)
                          select e).Select(e => new
                          {
                              EdiEntryId = e.EdiEntryId,
                              InvoiceId = e.InvoiceId.Value,
                              BillingType = e.Invoice.BillingType,
                              SupplierNr = e.Supplier.SupplierNr,
                              SupplierName = e.Supplier.Name,
                              InvoiceNr = e.Invoice.InvoiceNr,
                              SeqNr = e.Invoice.SeqNr,
                              InvoiceDate = e.Invoice.InvoiceDate,
                              IsSupplierInvoice = e.Invoice.Origin.Type == (int)SoeOriginType.SupplierInvoice,
                              EdiAmount = e.Invoice.TotalAmountCurrency - e.Invoice.VATAmountCurrency
                          }).ToList();

            foreach (var row in rows)
            {
                var connectedEdiEntry = entrys.FirstOrDefault(e => e.EdiEntryId == row.EdiEntryId.Value);
                if (connectedEdiEntry == null)
                    continue;

                var existing = items.FirstOrDefault(i => i.SupplierInvoiceId == connectedEdiEntry.InvoiceId);
                if (existing != null)
                {
                    existing.Amount += row.Amount;
                    if (!existing.IncludeImageOnInvoice && row.IncludeImageOnInvoice)
                        existing.IncludeImageOnInvoice = true;
                }
                else
                {
                    row.SupplierInvoiceId = connectedEdiEntry.InvoiceId;
                    row.BillingType = (TermGroup_BillingType)connectedEdiEntry.BillingType;
                    row.SupplierNr = connectedEdiEntry.SupplierNr;
                    row.SupplierName = connectedEdiEntry.SupplierName;
                    row.InvoiceNr = connectedEdiEntry.InvoiceNr;
                    row.SeqNr = connectedEdiEntry.SeqNr;
                    row.SupplierInvoiceOrderLinkType = SupplierInvoiceOrderLinkType.Transfered;
                    row.InvoiceDate = connectedEdiEntry.InvoiceDate;

                     if (row.InvoiceAmountExVat == 0 && connectedEdiEntry.IsSupplierInvoice)
                        row.InvoiceAmountExVat = connectedEdiEntry.EdiAmount;

                    items.Add(row);
                }
            }

            return items;
        }

        public List<SupplierInvoiceOrderGridDTO> GetSupplierInvoiceItemsForOrder(int invoiceId, int projectId)
        {
            List<SupplierInvoiceOrderGridDTO> items = new List<SupplierInvoiceOrderGridDTO>();

            using (CompEntities entities = new CompEntities())
            {
                // Linked to order
                items.AddRange(this.GetSupplierInvoicesLinkedToOrder(entities, invoiceId, base.ActorCompanyId));
                items.AddRange(this.GetSupplierInvoiceRowsLinkedToOrder(entities, invoiceId, base.ActorCompanyId));

                // Linked to project
                if (projectId > 0)
                {
                    var itemsLinkedToProject = this.GetSupplierInvoicesLinkedToProject(entities, invoiceId, projectId, base.ActorCompanyId);
                    foreach (var item in itemsLinkedToProject)
                    {
                        if (!items.Any(i => i.SupplierInvoiceId == item.SupplierInvoiceId))
                            items.Add(item);
                    }
                }

                // Transfered to order
                var ediItems = this.GetSupplierInvoicesTransferedToOrder(entities, invoiceId, base.ActorCompanyId);
                foreach (var item in ediItems)
                {
                    if (!items.Any(i => i.SupplierInvoiceId == item.SupplierInvoiceId))
                        items.Add(item);
                }

                foreach (var item in items)
                {
                    item.HasImage = HasSupplierInvoiceImage(entities, base.ActorCompanyId, item.SupplierInvoiceId);
                }
            }

            return items;
        }

        public ActionResult UpdateSupplierInvoiceImageOnOrder(int id, SupplierInvoiceOrderLinkType type, bool include)
        {
            ActionResult result = new ActionResult();
            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    if (type == SupplierInvoiceOrderLinkType.LinkToProject)
                    {
                        var timeCodeTransaction = (from tct in entities.TimeCodeTransaction
                                                   where tct.TimeCodeTransactionId == id &&
                                                   tct.State == (int)SoeEntityState.Active
                                                   select tct).FirstOrDefault();

                        if (timeCodeTransaction != null)
                        {
                            timeCodeTransaction.IncludeSupplierInvoiceImage = include;

                            SetModifiedProperties(timeCodeTransaction);
                        }
                    }
                    else
                    {
                        var customerInvoiceRow = (from r in entities.CustomerInvoiceRow
                                                  where r.CustomerInvoiceRowId == id &&
                                                  r.State == (int)SoeEntityState.Active
                                                  select r).FirstOrDefault();

                        if (customerInvoiceRow != null)
                        {
                            customerInvoiceRow.IncludeSupplierInvoiceImage = include;

                            SetModifiedProperties(customerInvoiceRow);
                        }
                    }

                    result = SaveChanges(entities);
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    result.Exception = ex;
                }

                return result;
            }
        }

        public int GetSupplierInvoiceIdByNr(CompEntities entities, string invoiceNr, int actorCompanyId, TermGroup_BillingType? billingType = null)
        {
            int supplierInvoiceId = (from i in entities.Invoice.OfType<SupplierInvoice>()
                                     where i.InvoiceNr == invoiceNr &&
                                     i.Origin.ActorCompanyId == actorCompanyId &&
                                     i.Origin.Type == (int)SoeOriginType.SupplierInvoice &&
                                     i.State == (int)SoeEntityState.Active &&
                                     (!billingType.HasValue || i.BillingType == (int)billingType.Value)
                                     select i.InvoiceId).FirstOrDefault();

            return supplierInvoiceId;
        }

        public int GetSupplierInvoiceIdByNr(CompEntities entities, string invoiceNr, int supplierId, int actorCompanyId, TermGroup_BillingType? billingType = null, int? seqNr = null)
        {
            if (seqNr == null)
            {
                int supplierInvoiceId = (from i in entities.Invoice.OfType<SupplierInvoice>()
                                         where i.InvoiceNr == invoiceNr &&
                                         i.Origin.ActorCompanyId == actorCompanyId &&
                                         i.ActorId == supplierId &&
                                         i.Origin.Type == (int)SoeOriginType.SupplierInvoice &&
                                         i.State == (int)SoeEntityState.Active &&
                                         (!billingType.HasValue || i.BillingType == (int)billingType.Value)
                                         select i.InvoiceId).FirstOrDefault();

                return supplierInvoiceId;
            }
            else
            {
                int supplierInvoiceId = (from i in entities.Invoice.OfType<SupplierInvoice>()
                                         where i.InvoiceNr == invoiceNr &&
                                         i.Origin.ActorCompanyId == actorCompanyId &&
                                         i.ActorId == supplierId &&
                                         i.SeqNr == seqNr &&
                                         i.Origin.Type == (int)SoeOriginType.SupplierInvoice &&
                                         i.State == (int)SoeEntityState.Active &&
                                         (!billingType.HasValue || i.BillingType == (int)billingType.Value)
                                         select i.InvoiceId).FirstOrDefault();

                return supplierInvoiceId;
            }

        }

        public bool IsSupplierInvoiceValidForVoucher(int originStatus)
        {
            return originStatus == (int)SoeOriginStatus.Origin;
        }

        public GenericImageDTO GetSupplierInvoiceImageByFileId(int actorCompanyId, int fileId)
        {
            using (var entities = new CompEntities())
            {
                var dataStorageRecord = GeneralManager.GetDataStorageRecord(entities, actorCompanyId, fileId);
                if (dataStorageRecord != null)
                {
                    if (!dataStorageRecord.DataStorageReference.IsLoaded)
                        dataStorageRecord.DataStorageReference.Load();

                    if (dataStorageRecord.DataStorage.Data != null && (SoeDataStorageRecordType)dataStorageRecord.DataStorage.Type == SoeDataStorageRecordType.Unknown)
                    {
                        try
                        {
                            string destinationFileName = ConfigSettings.SOE_SERVER_DIR_TEMP_REPORT_PHYSICAL + Guid.NewGuid().ToString() + Constants.SOE_SERVER_FILE_PDF_SUFFIX;
                            byte[] pdf = PDFUtility.CreatePdfFromTif(dataStorageRecord.DataStorage.Data, destinationFileName, true);

                            if (pdf != null)
                            {
                                return new GenericImageDTO() { Id = dataStorageRecord.DataStorageRecordId, Image = pdf, ImageFormatType = SoeDataStorageRecordType.InvoicePdf };
                            }
                        }
                        catch (Exception ex)
                        {
                            LogError(ex, this.log);
                        }
                    }

                    return new GenericImageDTO()
                    {
                        Id = dataStorageRecord.DataStorageRecordId,
                        Image = dataStorageRecord.DataStorage.Data,
                        ImageFormatType = (SoeDataStorageRecordType)dataStorageRecord.DataStorage.Type,
                    };
                }
            }
            return null;
        }

        public GenericImageDTO GetSupplierInvoiceImage(int actorCompanyId, int invoiceId, bool loadAll = false, int? ediEntryId = null)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.DataStorageRecord.NoTracking();
            return GetSupplierInvoiceImage(entities, actorCompanyId, invoiceId, loadAll, ediEntryId: ediEntryId);
        }

        public List<GenericImageDTO> GetSupplierInvoiceImages(CompEntities entities, int actorCompanyId, List<int> invoiceIds, bool loadAll = false, string ediTerm = null, string dataStorageTerm = null, bool includeInvoiceAttachment = false)
        {
            List<GenericImageDTO> images = new List<GenericImageDTO>();

            foreach (var id in invoiceIds)
            {
                var image = GetSupplierInvoiceImage(entities, actorCompanyId, id, loadAll, ediTerm, dataStorageTerm, true);
                if (image != null)
                    images.Add(image);
            }

            return images;
        }

        public GenericImageDTO GetSupplierInvoiceImage(CompEntities entities, int actorCompanyId, int invoiceId, bool loadAll = false, string ediTerm = null, string dataStorageTerm = null, bool includeInvoiceAttachment = false, int? ediEntryId = null)
        {
            if (invoiceId <= 0 && ediEntryId.GetValueOrDefault() == 0)
            {
                return null;
            }

            ImageFormatType format;
            string filename = null;
            GenericImageDTO imageDTO = null;
                
            if (invoiceId > 0)
            {
                imageDTO = GetInvoiceImageFromDataStorage(entities, actorCompanyId, invoiceId, includeInvoiceAttachment, dataStorageTerm);
                if (imageDTO != null) return imageDTO;
            }

            var ediEntryRecord = ediEntryId.HasValue ? EdiManager.GetEdiEntry(entities, ediEntryId.Value, actorCompanyId, true) : EdiManager.GetEdiEntryFromInvoice(entities, invoiceId, includeEdiEntry: true, includeInvoiceAttachment: includeInvoiceAttachment);
            if (ediEntryRecord != null)
            {
                if (ediEntryRecord.PDF != null)
                    imageDTO = new GenericImageDTO() { Id = ediEntryRecord.EdiEntryId, Image = ediEntryRecord.PDF, Description = ediEntryRecord.FileName, ImageFormatType = SoeDataStorageRecordType.InvoicePdf, ConnectedTypeName = ediTerm.HasValue() ? ediTerm : String.Empty, InvoiceAttachments = ediEntryRecord.InvoiceAttachment.Count > 0 ? ediEntryRecord.InvoiceAttachment.Where(i => i.EntityState == (int)SoeEntityState.Active).ToDTOs().ToList() : new List<InvoiceAttachmentDTO>(), SourceType = InvoiceAttachmentSourceType.Edi };

                if (imageDTO != null)
                {
                    GetImageFormatAndName(entities, imageDTO, invoiceId, out format, out filename);
                    imageDTO.Format = format;
                    imageDTO.Filename = filename;
                    return imageDTO;
                }

                if (ediEntryRecord.UsesDataStorage)
                    return GetImageFromEdiEntry(entities, actorCompanyId, ediEntryRecord.EdiEntryId, ediTerm);

                imageDTO = EdiManager.GetInvoiceImageAsPDF(entities, ediEntryRecord.EdiEntryId, actorCompanyId, includeInvoiceAttachment);
                if (imageDTO != null)
                {
                    GetImageFormatAndName(entities, imageDTO, invoiceId, out format, out filename);
                    imageDTO.Format = format;
                    imageDTO.Filename = filename;
                    imageDTO.ConnectedTypeName = ediTerm.HasValue() ? ediTerm : string.Empty;
                    return imageDTO;
                }

                imageDTO = EdiManager.GetInvoiceImageAndAttachments(entities, ediEntryRecord.EdiEntryId, actorCompanyId, includeInvoiceAttachment, loadAll);
                if (imageDTO != null)
                {
                    GetImageFormatAndName(entities, imageDTO, invoiceId, out format, out filename);
                    imageDTO.Format = format;
                    imageDTO.Filename = filename;
                    imageDTO.ConnectedTypeName = ediTerm.HasValue() ? ediTerm : string.Empty;
                    return imageDTO;
                }
            }

            return imageDTO;
        }

        private ImageFormatType GetImageFormatType(string extension, SoeDataStorageRecordType type)
        {
            if (extension != null)
            {
                switch (extension.ToLower())
                {
                    case ".jpg":
                    case ".jpeg":
                    case ".png":
                    case ".bmp":
                        return ImageFormatType.JPG;
                    case ".pdf":
                        return ImageFormatType.PDF;
                    default:
                        break;
                }
            }

            switch (type)
            {
                case SoeDataStorageRecordType.OrderInvoiceFileAttachment_Image:
                case SoeDataStorageRecordType.InvoiceBitmap:
                    return ImageFormatType.JPG;
                case SoeDataStorageRecordType.InvoicePdf:
                default:
                    return ImageFormatType.PDF;
            }
        }
        private GenericImageDTO GetInvoiceImageFromDataStorage(CompEntities entities, int actorCompanyId, int invoiceId, bool includeInvoiceAttachment = false, string dataStorageTerm = null)
        {
            var dataStorageRecord = GeneralManager.GetDataStorageRecord(entities, actorCompanyId, invoiceId, SoeEntityType.SupplierInvoice, includeInvoiceAttachment);
            if (dataStorageRecord != null)
            {
                var dataStorage = GeneralManager.GetDataStorage(entities, dataStorageRecord.DataStorageId, actorCompanyId, false);
                if (dataStorage == null) return null;

                var imageDTO = new GenericImageDTO()
                {
                    Id = dataStorageRecord.DataStorageRecordId,
                    Image = dataStorage.Data,
                    ImageFormatType = (SoeDataStorageRecordType)dataStorage.Type,
                    Format = GetImageFormatType(dataStorage.Extension, (SoeDataStorageRecordType)dataStorage.Type),
                    Description = dataStorage.Description,
                    ConnectedTypeName = dataStorageTerm.HasValue() ? dataStorageTerm : string.Empty,
                    SourceType = InvoiceAttachmentSourceType.DataStorage,
                };

                GetImageFormatAndName(entities, imageDTO, invoiceId, out ImageFormatType format, out string filename);
                imageDTO.Format = format;
                imageDTO.Filename = filename;

                if (includeInvoiceAttachment)
                {
                    imageDTO.InvoiceAttachments = dataStorageRecord.InvoiceAttachment.Count > 0 ? dataStorageRecord.InvoiceAttachment.ToDTOs().ToList() : new List<InvoiceAttachmentDTO>();
                }
                else
                {
                    imageDTO.InvoiceAttachments = new List<InvoiceAttachmentDTO>();
                }
                return imageDTO;
            }
            return null;
        }

        private GenericImageDTO GetImageFromEdiEntry(CompEntities entities, int actorCompanyId, int ediEntryId, string dataStorageTerm = null)
        {
            var dataStorageRecord = GeneralManager.GetDataStorageRecord(entities, actorCompanyId, ediEntryId, SoeDataStorageRecordType.EdiEntry_Document);
            if (dataStorageRecord != null)
            {
                var dataStorage = GeneralManager.GetDataStorage(entities, dataStorageRecord.DataStorageId, actorCompanyId, false);
                if (dataStorage == null) return null;

                var imageDTO = new GenericImageDTO()
                {
                    Id = dataStorageRecord.DataStorageRecordId,
                    Image = dataStorage.Data,
                    ImageFormatType = (SoeDataStorageRecordType)dataStorage.Type,
                    Format = GetImageFormatType(dataStorage.Extension, (SoeDataStorageRecordType)dataStorage.Type),
                    Filename = dataStorage.FileName,
                    Description = dataStorage.Description,
                    ConnectedTypeName = dataStorageTerm.HasValue() ? dataStorageTerm : string.Empty,
                    SourceType = InvoiceAttachmentSourceType.DataStorage,
                    InvoiceAttachments = new List<InvoiceAttachmentDTO>()
                };
                return imageDTO;
            }
            return null;
        }



        public bool HasSupplierInvoiceImage(int actorCompanyId, int invoiceId, bool loadAll = false, int? ediEntryId = null)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.DataStorageRecord.NoTracking();
            return HasSupplierInvoiceImage(entities, actorCompanyId, invoiceId, loadAll, ediEntryId: ediEntryId);
        }

        public bool HasSupplierInvoiceImage(CompEntities entities, int actorCompanyId, int invoiceId, bool loadAll = false, string ediTerm = null, string dataStorageTerm = null, bool includeInvoiceAttachment = false, int? ediEntryId = null)
        {
            if (invoiceId <= 0 && ediEntryId.GetValueOrDefault() == 0)
            {
                return false;
            }

            var ediEntryRecord = ediEntryId.HasValue ? EdiManager.GetEdiEntry(entities, ediEntryId.Value, actorCompanyId, true) : EdiManager.GetEdiEntryFromInvoice(entities, invoiceId, includeEdiEntry: true, includeInvoiceAttachment: includeInvoiceAttachment);
            if (ediEntryRecord != null)
            {
                if (ediEntryRecord.PDF != null)
                    return true;

                if(EdiManager.HasInvoiceImageAsPDF(entities, ediEntryRecord.EdiEntryId, actorCompanyId, includeInvoiceAttachment))
                    return true;
            }

            if (GeneralManager.HasDataStorageRecord(entities, actorCompanyId, invoiceId, SoeEntityType.SupplierInvoice))
                return true;

            return false;
        }

        public void GetImageFormatAndName(CompEntities entities, GenericImageDTO image, int invoiceId, out ImageFormatType format, out string fileName)
        {
            var invoiceNr = InvoiceManager.GetInvoiceNr(entities, invoiceId);
            format = ImageFormatType.NONE;
            fileName = !string.IsNullOrEmpty(invoiceNr) ? GetText(73, (int)TermGroup.Report, "Lev.faktura") + " " + invoiceNr : GetText(1020, (int)TermGroup.AngularCommon, "Leverantörsfaktura");
            switch (image.ImageFormatType)
            {
                case SoeDataStorageRecordType.OrderInvoiceFileAttachment_Image:
                case SoeDataStorageRecordType.InvoiceBitmap:
                    format = ImageFormatType.JPG;
                    fileName += ".jpg";
                    break;
                case SoeDataStorageRecordType.InvoicePdf:
                    format = ImageFormatType.PDF;
                    fileName += ".pdf";
                    break;
                default:
                    break;
            }
        }

        public List<(SupplierInvoiceDTO, List<AccountingRowDTO>)> GetSupplierInvoicesForTrainingInterpretor(CompEntities entities, int actorCompanyId, int takePerSupplier, int monthsBack)
        {
            var from = DateTime.UtcNow.AddMonths(-monthsBack);
            var allInvoices = entities.Invoice.OfType<SupplierInvoice>()
                .Where(i => i.Origin.ActorCompanyId == actorCompanyId && i.State == (int)SoeEntityState.Active)
                .Where(i => i.Origin.Status == (int)SoeOriginStatus.Origin || i.Origin.Status == (int)SoeOriginStatus.Voucher)
                .Where(i => i.InvoiceDate >= from)
                .OrderByDescending(i => i.InvoiceDate)
                .Select(i => (new SupplierInvoiceDTO()
                {
                    InvoiceId = i.InvoiceId,
                    ActorId = i.ActorId,
                    OriginDescription = i.Origin.Description,
                    InvoiceNr = i.InvoiceNr,
                    InvoiceDate = i.InvoiceDate,
                    DueDate = i.DueDate,
                    OrderNr = i.OrderNr,
                    OCR = i.OCR,
                    ReferenceOur = i.ReferenceOur,
                    ReferenceYour = i.ReferenceYour,
                    TotalAmountCurrency = i.TotalAmountCurrency,
                    VatAmountCurrency = i.VATAmountCurrency,
                }))
                .ToList();

            var supplierPerInvoiceCounter = new Dictionary<int, int>();
            var filteredInvoices = new List<SupplierInvoiceDTO>();

            foreach (var invoice in allInvoices)
            {
                if (!invoice.ActorId.HasValue) continue;
                var actorId = invoice.ActorId.Value;

                if (supplierPerInvoiceCounter.ContainsKey(actorId))
                {
                    supplierPerInvoiceCounter[actorId]++;
                }
                else
                {
                    supplierPerInvoiceCounter.Add(actorId, 1);
                }
                if (supplierPerInvoiceCounter[actorId] <= takePerSupplier)
                {
                    filteredInvoices.Add(invoice);
                }
            }
            var invoiceIds = filteredInvoices.Select(i => i.InvoiceId).ToList();
            var allAccountingRows = entities.SupplierInvoiceRow
                .Where(r => invoiceIds.Contains(r.InvoiceId))
                .Where(r => r.State == (int)SoeEntityState.Active)
                .Include("SupplierInvoiceAccountRow.AccountStd")
                .Include("SupplierInvoiceAccountRow.AccountStd.Account")
                .Include("SupplierInvoiceAccountRow.AccountStd.Account.AccountMapping")
                .Include("SupplierInvoiceAccountRow.AccountInternal")
                .Include("SupplierInvoiceAccountRow.AccountInternal.Account")
                .Include("SupplierInvoiceAccountRow.AccountInternal.Account.AccountDim")
                .ToList()
                .GroupBy(r => r.InvoiceId);

            var accountDims = AccountManager.GetAccountDimsByCompany(entities, actorCompanyId, 
                onlyStandard: false, 
                onlyInternal: false, 
                active: false,
                loadAccounts: false,
                loadInternalAccounts: false
            );

            var result = new List<(SupplierInvoiceDTO, List<AccountingRowDTO>)>();

            foreach (var invoice in filteredInvoices)
            {
                var grouping = allAccountingRows.FirstOrDefault(g => g.Key == invoice.InvoiceId);

                if (grouping == null)
                {
                    result.Add((invoice, new List<AccountingRowDTO>()));
                    continue;
                }

                var rows = grouping.ToList();
                var accountingRows = rows
                    .SelectMany(r => r.SupplierInvoiceAccountRow.ToAccountingRowDTO(accountDims))
                    .Where(r => r.State == (int)SoeEntityState.Active)
                    .ToList();
                result.Add((invoice, accountingRows));
            }

            return result;
        }


        public List<SupplierInvoiceCostTransferForGridDTO> GetSupplierInvoiceCostTransfersForGrid(int actorCompanyId, int roleId, int supplierInvoiceId)
        {
            using (var entities = new CompEntities())
            {
                bool linkToProjectPermission = FeatureManager.HasRolePermission(Feature.Economy_Supplier_Invoice_Project, Permission.Modify, roleId, this.ActorCompanyId);
                bool linkToOrderPermission = FeatureManager.HasRolePermission(Feature.Billing_Order_Orders_Edit, Permission.Modify, roleId, this.ActorCompanyId);

                List<SupplierInvoiceCostTransferForGridDTO> result = new List<SupplierInvoiceCostTransferForGridDTO>();
                List<SupplierInvoiceOrderRowDTO> orderRows = linkToOrderPermission ? GetSupplierInvoiceOrderRows(entities, actorCompanyId, supplierInvoiceId) : new List<SupplierInvoiceOrderRowDTO>();
                List<SupplierInvoiceProjectRowDTO> projectRows = linkToProjectPermission ? ProjectManager.GetSupplierInvoiceProjectRows(supplierInvoiceId, actorCompanyId) : new List<SupplierInvoiceProjectRowDTO>();
                foreach (var row in orderRows)
                {
                    result.Add(
                            new SupplierInvoiceCostTransferForGridDTO()
                            {
                                Type = SupplierInvoiceCostLinkType.OrderRow,
                                RecordId = row.CustomerInvoiceRowId,
                                OrderNumber = row.CustomerInvoiceNr,
                                OrderName = row.CustomerInvoiceCustomerName,
                                ProjectNumber = row.ProjectNr,
                                ProjectName = row.ProjectName,
                                TimeCodeName = null,
                                Amount = row.Amount,
                                SupplementCharge = row.SupplementCharge ?? 0m,
                                SumAmount = row.SumAmount
                            }
                        );
                }
                foreach (var row in projectRows)
                {
                    result.Add(
                            new SupplierInvoiceCostTransferForGridDTO()
                            {
                                Type = SupplierInvoiceCostLinkType.ProjectRow,
                                RecordId = row.TimeCodeTransactionId,
                                OrderNumber = row.CustomerInvoiceNr,
                                OrderName = row.CustomerInvoiceNumberName,
                                ProjectNumber = row.ProjectNr,
                                ProjectName = row.ProjectName,
                                TimeCodeName = row.TimeCodeName,
                                Amount = row.Amount
                            }
                        );
                }
                return result;
            }
        }
        public SupplierInvoiceCostTransferDTO GetSupplierInvoiceCostTransfer(int actorCompanyId, int type, int recordId)
        {
            using (var entities = new CompEntities())
            {
                SupplierInvoiceCostLinkType costType = (SupplierInvoiceCostLinkType)type;
                SupplierInvoiceCostTransferDTO result = null;
                switch (costType)
                {
                    case SupplierInvoiceCostLinkType.OrderRow:
                        result = GetSupplierInvoiceOrderRow(entities, recordId);
                        break;
                    case SupplierInvoiceCostLinkType.ProjectRow:
                        result = ProjectManager.GetSupplierInvoiceProjectRow(entities, recordId);
                        break;
                }
                return result;
            }
        }
        public ActionResult SaveSupplierInvoiceCostTransfer(int actorCompanyId, int roleId, SupplierInvoiceCostTransferDTO dto)
        {
            ActionResult actionResult = new ActionResult();

            if (dto.State == SoeEntityState.Deleted)
            {
                if (dto.RecordId == 0)
                {
                    return actionResult;
                }
                else
                {
                    //prevent validation of data if transaction is being removed
                    dto = GetSupplierInvoiceCostTransfer(actorCompanyId, (int)dto.Type, dto.RecordId);
                    dto.State = SoeEntityState.Deleted;
                }
            }

            using (var entities = new CompEntities())
            {
                using (var transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                {
                    var supplierInvoice = GetSupplierInvoice(dto.SupplierInvoiceId, false, true, false, false, false, false, false, false, true, true, false);
                    switch (dto.Type)
                    {
                        case SupplierInvoiceCostLinkType.OrderRow:
                            var linkToOrderPermission = FeatureManager.HasRolePermission(Feature.Billing_Order_Orders_Edit, Permission.Modify, roleId, actorCompanyId);
                            if (!linkToOrderPermission)
                            {
                                actionResult.Success = false;
                                break;
                            }
                            var supplierInvoiceOrderRowDTOs = new List<SupplierInvoiceOrderRowDTO>() { dto.ToSupplierInvoiceOrderRowDTO() };

                            string description = null;
                            if(supplierInvoice != null)
                            {
                                description = " - " + GetText(7712, "från leverantörsfaktura") + " " + supplierInvoice.InvoiceNr + " " + (supplierInvoice.ActorName ?? "");
                            }

                            actionResult = InvoiceManager.SaveOrderRowFromSupplierInvoice(transaction, entities, supplierInvoice, supplierInvoiceOrderRowDTOs, actorCompanyId, roleId, description, fromApp: true);
                            break;
                        case SupplierInvoiceCostLinkType.ProjectRow:
                            var linkToProjectPermission = FeatureManager.HasRolePermission(Feature.Economy_Supplier_Invoice_Project, Permission.Modify, roleId, actorCompanyId);
                            if (!linkToProjectPermission)
                            {
                                actionResult.Success = false;
                                break;
                            }
                            var supplierInvoiceProjectRowDTOs = new List<SupplierInvoiceProjectRowDTO>() { dto.ToSupplierInvoiceProjectRowDTO() };
                            actionResult = ProjectManager.SaveSupplierInvoiceProjectRows(entities, transaction, supplierInvoice, supplierInvoiceProjectRowDTOs, actorCompanyId, true, true, true);
                            break;
                        default:
                            break;
                    }
                    if (actionResult.Success)
                        transaction.Complete();
                }
            }
            return actionResult;
        }
        public SupplierInvoiceCostTransferDTO GetSupplierInvoiceOrderRow(CompEntities entities, int customerInvoiceRowId)
        {
            CustomerInvoiceRow row = (from r in entities.CustomerInvoiceRow
                                     .Include("CustomerInvoice.Origin")
                                     .Include("CustomerInvoice.Project")
                                     .Include("CustomerInvoice.Actor.Customer")
                                      where r.CustomerInvoice.Origin.ActorCompanyId == ActorCompanyId &&
                                      r.CustomerInvoice.Origin.Type == (int)SoeOriginType.Order &&
                                      r.State == (int)SoeEntityState.Active &&
                                      r.CustomerInvoiceRowId == customerInvoiceRowId
                                      select r).FirstOrDefault();

            SupplierInvoiceCostTransferDTO orderRow = new SupplierInvoiceCostTransferDTO()
            {
                //General
                Type = SupplierInvoiceCostLinkType.OrderRow,
                RecordId = row.CustomerInvoiceRowId,
                State = SoeEntityState.Active,


                Amount = row.Amount,
                AmountCurrency = row.AmountCurrency,
                AmountLedgerCurrency = row.AmountLedgerCurrency,
                AmountEntCurrency = row.AmountEntCurrency,

                SumAmount = row.SumAmount,
                SumAmountCurrency = row.SumAmountCurrency,
                SumAmountEntCurrency = row.SumAmountEntCurrency,
                SumAmountLedgerCurrency = row.SumAmountLedgerCurrency,

                IncludeSupplierInvoiceImage = row.IncludeSupplierInvoiceImage.HasValue ? row.IncludeSupplierInvoiceImage.Value : false,

                //SupplierInvoice
                SupplierInvoiceId = (int)row.SupplierInvoiceId,

                //Customer Invoice
                CustomerInvoiceId = row.InvoiceId,
                CustomerInvoiceNr = row.CustomerInvoice.InvoiceNr,
                CustomerInvoiceRowId = row.CustomerInvoiceRowId,
                CustomerInvoiceCustomerName = row.CustomerInvoice.Actor != null && row.CustomerInvoice.Actor.Customer != null ? row.CustomerInvoice.InvoiceNr + " " + row.CustomerInvoice.Actor.Customer.Name : "",
                CustomerInvoiceNumberName = row.CustomerInvoice.InvoiceNr + (row.CustomerInvoice.Actor != null && row.CustomerInvoice.Actor.Customer != null ? " " + row.CustomerInvoice.Actor.Customer.Name : ""),
                CustomerInvoiceDescription = String.Empty,

                //Project
                ProjectId = row.CustomerInvoice.Project != null ? row.CustomerInvoice.Project.ProjectId : 0,
                ProjectNr = row.CustomerInvoice.Project != null ? row.CustomerInvoice.Project.Number : null,
                ProjectName = row.CustomerInvoice.Project != null ? row.CustomerInvoice.Project.Number + " " + row.CustomerInvoice.Project.Name : null,
                ProjectDescription = String.Empty,
            };

            if (row.PurchasePriceCurrency > 1)
            {
                orderRow.SupplementCharge = ((row.SumAmountCurrency / row.PurchasePriceCurrency) - 1) * 100;

                orderRow.Amount = row.PurchasePrice;
                orderRow.AmountCurrency = row.PurchasePriceCurrency;
                orderRow.AmountLedgerCurrency = row.PurchasePriceLedgerCurrency;
                orderRow.AmountEntCurrency = row.PurchasePriceEntCurrency;
            }
            else
            {
                orderRow.SupplementCharge = row.AmountCurrency != 0 ? (((row.SumAmountCurrency / row.AmountCurrency) - 1) * 100) : 0;

                orderRow.Amount = row.Amount;
                orderRow.AmountCurrency = row.AmountCurrency;
                orderRow.AmountLedgerCurrency = row.AmountLedgerCurrency;
                orderRow.AmountEntCurrency = row.AmountEntCurrency;
            }
            return orderRow;
        }

        public List<PurchaseDeliveryInvoiceDTO> GetSupplierPurchaseDeliveryInvoices(int actorCompanyId, int supplierInvoiceId)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            entitiesReadOnly.PurchaseDeliveryInvoice.NoTracking();
            return this.GetSupplierPurchaseDeliveryInvoices(entitiesReadOnly, actorCompanyId, supplierInvoiceId);
        }
        public List<PurchaseDeliveryInvoiceDTO> GetSupplierPurchaseDeliveryInvoices(CompEntities entities, int actorCompanyId, int supplierInvoiceId)
        {
            if (supplierInvoiceId == 0)
            {
                return new List<PurchaseDeliveryInvoiceDTO>();
            }

            var rows = entities.PurchaseDeliveryInvoice
                .Where(i => i.SupplierInvoice.Origin.ActorCompanyId == actorCompanyId && i.SupplierinvoiceId == supplierInvoiceId)
                .Select(i => new PurchaseDeliveryInvoiceDTO
                {
                    PurchaseDeliveryInvoiceId = i.PurchaseDeliveryInvoiceId,
                    SupplierinvoiceId = i.SupplierinvoiceId,
                    PurchaseRowId = i.PurchaseRowId,
                    Price = i.Price,
                    Quantity = i.Quantity,

                    PurchaseId = i.PurchaseRow.PurchaseId,
                    PurchaseNr = i.PurchaseRow.Purchase.PurchaseNr,
                    PurchaseRowNr = i.PurchaseRow.RowNr,
                    Text = i.PurchaseRow.Text,
                    AskedPrice = i.PurchaseRow.PurchasePriceCurrency,
                    PurchaseQuantity = i.PurchaseRow.Quantity,
                    DeliveredQuantity = i.PurchaseRow.DeliveredQuantity ?? 0,

                    SupplierProductId = i.PurchaseRow.SupplierProductId,
                    SupplierProductNr = i.PurchaseRow.SupplierProduct.SupplierProductNr,
                    SupplierProductName = i.PurchaseRow.SupplierProduct.Name,

                    ProductId = i.PurchaseRow.ProductId,
                    ProductNumber = i.PurchaseRow.Product.Number,
                    ProductName = i.PurchaseRow.Product.Name,
                    SupplierInvoiceSeqNr = i.SupplierInvoice.SeqNr
                })
                .ToList();

            rows.ForEach(r =>
            {
                r.Text = !string.IsNullOrEmpty(r.Text) && r.Text.Length > 20 ? $"{r.Text.Substring(0, 20)}..." : $"{r.Text}";
                if (r.SupplierProductId > 0) r.PurchaseRowDisplayName = $"{r.PurchaseRowNr} - {r.SupplierProductNr} - {r.SupplierProductName} ({r.Text})";
                else r.PurchaseRowDisplayName = $"{r.PurchaseRowNr} - {r.ProductNumber} - {r.ProductName} ({r.Text})";
            });

            return rows;
        }
        public List<SupplierInvoiceOrderRowDTO> GetSupplierInvoiceOrderRows(int actorCompanyId, int supplierInvoiceId)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            entitiesReadOnly.CustomerInvoiceRow.NoTracking();
            return this.GetSupplierInvoiceOrderRows(entitiesReadOnly, actorCompanyId, supplierInvoiceId);
        }
        public List<SupplierInvoiceOrderRowDTO> GetSupplierInvoiceOrderRows(CompEntities entities, int actorCompanyId, int supplierInvoiceId)
        {
            List<AttestState> attestStates = AttestManager.GetAttestStates(entities, actorCompanyId, TermGroup_AttestEntity.Order, SoeModule.None);
            AttestState initialAttestState = attestStates.FirstOrDefault(a => a.Initial);

            var rows = (from r in entities.CustomerInvoiceRow
                        where
                           r.SupplierInvoiceId == supplierInvoiceId &&
                           r.State == (int)SoeEntityState.Active &&
                           r.CustomerInvoice.Origin.ActorCompanyId == actorCompanyId &&
                           r.CustomerInvoice.Origin.Type == (int)SoeOriginType.Order
                        select new SupplierInvoiceOrderRowDTO
                        {
                            AttestStateId = r.AttestStateId ?? 0,
                            State = (SoeEntityState)r.State,
                            Amount = r.PurchasePriceCurrency > 1 ? r.PurchasePrice : r.Amount,
                            AmountCurrency = r.PurchasePriceCurrency > 1 ? r.PurchasePriceCurrency : r.AmountCurrency,
                            AmountLedgerCurrency = r.PurchasePriceCurrency > 1 ? r.PurchasePriceLedgerCurrency : r.AmountLedgerCurrency,
                            AmountEntCurrency = r.PurchasePriceCurrency > 1 ? r.PurchasePriceEntCurrency : r.AmountEntCurrency,

                            SumAmount = r.SumAmount,
                            SumAmountCurrency = r.SumAmountCurrency,
                            SumAmountEntCurrency = r.SumAmountEntCurrency,
                            SumAmountLedgerCurrency = r.SumAmountLedgerCurrency,

                            IncludeSupplierInvoiceImage = r.IncludeSupplierInvoiceImage ?? false,

                            //SupplierInvoice
                            SupplierInvoiceId = r.SupplierInvoiceId ?? 0,

                            //Customer Invoice
                            CustomerInvoiceId = r.InvoiceId,
                            CustomerInvoiceNr = r.CustomerInvoice.InvoiceNr,
                            CustomerInvoiceRowId = r.CustomerInvoiceRowId,
                            CustomerInvoiceCustomerName = r.CustomerInvoice.Actor.Customer.Name,

                            //Project
                            ProjectId = r.CustomerInvoice.ProjectId ?? 0,
                            ProjectNr = r.CustomerInvoice.Project.Number,
                            ProjectName = r.CustomerInvoice.Project.Name,
                        }).ToList();

            foreach (var row in rows)
            {
                var attestState = attestStates.FirstOrDefault(a => a.AttestStateId == row.AttestStateId);

                row.IsReadOnly = row.AttestStateId != initialAttestState.AttestStateId;
                row.AttestStateName = attestState?.Name ?? string.Empty;
                row.AttestStateColor = attestState?.Color ?? string.Empty;

                row.CustomerInvoiceNumberName = row.CustomerInvoiceNr + (!string.IsNullOrEmpty(row.CustomerInvoiceCustomerName) ? " " + row.CustomerInvoiceCustomerName : "");
                row.CustomerInvoiceCustomerName = !string.IsNullOrEmpty(row.CustomerInvoiceCustomerName) ? row.CustomerInvoiceNr + " " + row.CustomerInvoiceCustomerName : "";
                row.CustomerInvoiceDescription = "";

                row.ProjectName = !string.IsNullOrEmpty(row.ProjectName) ? row.ProjectNr + " " + row.ProjectName : "";
                row.ProjectDescription = string.Empty;
                row.SupplementCharge = row.AmountCurrency != 0 ? ((row.SumAmountCurrency / row.AmountCurrency) - 1) * 100 : 0;
            }

            return rows;
        }

        public List<SupplierInvoiceCostAllocationDTO> GetSupplierInvoiceCostAllocationRows(int actorCompanyId, int supplierInvoiceId)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            entitiesReadOnly.CustomerInvoiceRow.NoTracking();
            return this.GetSupplierInvoiceCostAllocationRows(entitiesReadOnly, actorCompanyId, supplierInvoiceId);
        }

        public List<SupplierInvoiceCostAllocationDTO> GetSupplierInvoiceCostAllocationRows(CompEntities entities, int actorCompanyId, int supplierInvoiceId)
        {
            List<AttestState> attestStates = AttestManager.GetAttestStates(entities, actorCompanyId, TermGroup_AttestEntity.Order, SoeModule.None);
            AttestState initialAttestState = attestStates.FirstOrDefault(a => a.Initial);

            var rows = (from r in entities.SupplierInvoiceCostAllocationView
                        where
                           r.SupplierInvoiceId == supplierInvoiceId
                        select new SupplierInvoiceCostAllocationDTO
                        {
                            CustomerInvoiceRowId = r.CustomerInvoiceRowId,
                            TimeCodeTransactionId = r.TimeCodeTransactionId,
                            SupplierInvoiceId = r.SupplierInvoiceId ?? 0,                            
                            ProjectId = r.ProjectId,
                            OrderId = r.OrderId,
                            AttestStateId = r.AttestStateId ?? 0,
                            TimeInvoiceTransactionId = r.TimeInvoiceTransactionId,
                            State = (SoeEntityState)r.State,

                            ProjectAmount = r.ProjectAmount ?? 0,
                            ProjectAmountCurrency = r.ProjectAmountCurrency ?? 0,
                            RowAmount = r.RowAmount,
                            RowAmountCurrency = r.RowAmountCurrency,
                            OrderAmount = r.OrderAmount,
                            OrderAmountCurrency = r.OrderAmountCurrency,

                            ChargeCostToProject = r.DoNotChargeProject.HasValue ? !r.DoNotChargeProject.Value : false,
                            IncludeSupplierInvoiceImage = r.IncludeSupplierInvoiceImage ?? false,

                            // Product
                            ProductId = r.ProductId,
                            ProductNr = r.ProductNr,
                            ProductName = r.ProductName,

                            // Timecode
                            TimeCodeId = r.TimeCodeId,
                            TimeCodeCode = r.TimeCode,
                            TimeCodeName = r.TimeCodeName,
                            TimeCodeDescription = r.TimeCodeDescription,

                            //Customer Invoice
                            OrderNr = r.OrderNr,
                            CustomerInvoiceNumberName = r.CustomerInvoiceNumberName,

                            //Project
                            ProjectNr = r.Number,
                            ProjectName = r.Number + " " + r.ProjectName,

                            // Employee
                            EmployeeId = r.EmployeeId,
                            EmployeeNr = r.EmployeeNr,
                            EmployeeName = r.EmployeeName,
                            EmployeeDescription = r.EmployeeNr + " " + r.EmployeeName,

                            IsTransferToOrderRow = r.CustomerInvoiceRowId > 0,
                            IsConnectToProjectRow = r.TimeCodeTransactionId > 0,
                        }).ToList();

            decimal supplierInvoiceCurrencyRate = entities.Invoice
                .Where(i => i.Origin.ActorCompanyId == actorCompanyId 
                    && i.InvoiceId == supplierInvoiceId)
                .Select(i => i.CurrencyRate)
                .FirstOrDefault();

            foreach (var row in rows)
            {

                // Customer Invoices currency values are saved from its currency rate,
                // so we need to convert back to supplier invoice currency rate for supplier invoice page.
                if (supplierInvoiceCurrencyRate > 0 && row.CustomerInvoiceRowId > 0)
                {
                    row.RowAmountCurrency = CountryCurrencyManager.GetCurrencyAmountFromBaseAmount(
                        row.RowAmount, supplierInvoiceCurrencyRate, 4);

                    row.OrderAmountCurrency = CountryCurrencyManager.GetCurrencyAmountFromBaseAmount(
                        row.OrderAmount, supplierInvoiceCurrencyRate, 4);
                }


                var attestState = attestStates.FirstOrDefault(a => a.AttestStateId == row.AttestStateId);

                row.IsReadOnly = row.AttestStateId > 0 ?  row.AttestStateId != initialAttestState.AttestStateId : false;
                row.AttestStateName = attestState?.Name ?? string.Empty;
                row.AttestStateColor = attestState?.Color ?? string.Empty;

                row.SupplementCharge = row.RowAmountCurrency != 0 ? ((row.OrderAmountCurrency / row.RowAmountCurrency) - 1) * 100 : 0;
            }

            return rows;
        }


        public ActionResult SaveSupplierInvoiceCostAllocationRows(List<SupplierInvoiceCostAllocationDTO> costAllocationRows, int supplierInvoiceId, int projectId, int customerInvoiceId, int OrderSeqNr, int actorCompanyId)
        {
            ActionResult result = new ActionResult();

            using (var entities = new CompEntities())
            {
                try 
                {
                    using (var transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        List<int> connectedOrders = new List<int>();

                        var supplierInvoice = GetSupplierInvoice(entities, supplierInvoiceId, false, false, false, false, false, false, false, false, true, true, false);

                        supplierInvoice.ProjectId = projectId.ToNullable();
                        supplierInvoice.OrderNr = OrderSeqNr.ToNullable();

                        var costToProjectRows = costAllocationRows.Where(r => r.TimeCodeTransactionId != 0 || r.TimeCodeId != 0);

                        if (!SupplierInvoiceCostAllocationHelper.AmountsAreValid(supplierInvoice.TotalAmountCurrency, supplierInvoice.VATAmountCurrency, costAllocationRows))
                        {
                            return new ActionResult((int)ActionResultSave.ProjectRowsAmountInvalid, GetText(5741, "Belopp för projektrader får ej överstiga fakturans totalbelopp"));
                        }

                        if (costToProjectRows.Count() > 0)
                        {
                            var prows = new List<SupplierInvoiceProjectRowDTO>();

                            foreach (var row in costToProjectRows)
                            {
                                var prow = new SupplierInvoiceProjectRowDTO()
                                {
                                    State = row.State,
                                    TimeCodeTransactionId = row.TimeCodeTransactionId,
                                    Amount = row.ProjectAmount,
                                    AmountCurrency = row.ProjectAmountCurrency,
                                    TimeInvoiceTransactionId = row.TimeInvoiceTransactionId ?? 0,
                                    SupplierInvoiceId = row.SupplierInvoiceId > 0 ? row.SupplierInvoiceId : supplierInvoice.InvoiceId,
                                    TimeCodeId = row.TimeCodeId,
                                    EmployeeId = row.EmployeeId,
                                    ChargeCostToProject = row.ChargeCostToProject,
                                    IncludeSupplierInvoiceImage = row.IncludeSupplierInvoiceImage,
                                    ProjectId = row.ProjectId,
                                    CustomerInvoiceId = row.OrderId,
                                };
                                prows.Add(prow);
                            }

                            result = ProjectManager.SaveSupplierInvoiceProjectRows(entities, transaction, supplierInvoice, prows, actorCompanyId, false);
                            if (!result.Success)
                                return result;
                            else
                                connectedOrders.AddRange(result.Value as List<int>);
                        }

                        var costToOrderRows = costAllocationRows.Where(r => (r.CustomerInvoiceRowId != 0 || r.OrderId != 0) && r.TimeCodeId == 0 && (r.EmployeeId == 0 || r.EmployeeId == null));
                        if (costToOrderRows.Any())
                        {
                            var miscProduct = ProductManager.GetInvoiceProductFromSetting(entities, CompanySettingType.ProductMisc, actorCompanyId, false, true);
                            if (miscProduct == null && costToOrderRows.Any(r => !r.ProductId.HasValue || r.ProductId.Value == 0))
                                return new ActionResult(null, GetText(9144, "Ingen ströartikel hittad på företaget"));

                            var supplierName = SupplierManager.GetSupplierName(entities, supplierInvoice.ActorId.Value, actorCompanyId);
                            var description = " - " + GetText(7712, "från leverantörsfaktura") + " " + supplierInvoice.InvoiceNr + " " + (supplierName ?? "");

                            var orows = new List<SupplierInvoiceOrderRowDTO>();

                            foreach (var row in costToOrderRows)
                            {
                                var orow = row.ToSupplierInvoiceOrderRow((miscProduct.ProductId, miscProduct.Name, supplierInvoiceId));

                                var existingRow = supplierInvoice.SupplierInvoiceOrderRows.FirstOrDefault(r => r.CustomerInvoiceRowId == row.CustomerInvoiceRowId && r.State != SoeEntityState.Deleted);
                                if (existingRow != null && existingRow.CustomerInvoiceId != row.OrderId)
                                {
                                    // We are changing orderId on an row, to handle that, we remove the existing row and create a new one.
                                    orow.CustomerInvoiceRowId = 0;
                                    var deleteRow = row.ToSupplierInvoiceOrderRow((miscProduct.ProductId, miscProduct.Name, supplierInvoiceId));
                                    deleteRow.State = SoeEntityState.Deleted;
                                    orows.Add(deleteRow);
                                }

                                orows.Add(orow);
                            }

                            result = InvoiceManager.SaveOrderRowFromSupplierInvoice(
                                transaction, 
                                entities, 
                                supplierInvoice, 
                                orows, 
                                actorCompanyId, 
                                base.RoleId, 
                                description, 
                                false, 
                                isCostAllocation: true,
                                fromBaseCurrency: true);
                            if (!result.Success)
                            {
                                return result;
                            }
                            else
                            {
                                foreach (var item in result.Value as List<int>)
                                {
                                    if (!connectedOrders.Contains(item))
                                        connectedOrders.Add(item);
                                }
                            }
                        }

                        var loadedOrders = new List<CustomerInvoice>();
                        if (connectedOrders != null && connectedOrders.Count > 0)
                        {
                            var image = SupplierInvoiceManager.GetSupplierInvoiceImage(entities, actorCompanyId, supplierInvoiceId, false);
                            if (image != null && image.SourceType != InvoiceAttachmentSourceType.None)
                            {
                                foreach (var orderId in connectedOrders)
                                {
                                    CustomerInvoice order = loadedOrders.FirstOrDefault(o => o.InvoiceId == orderId);
                                    if (order == null)
                                    {
                                        order = InvoiceManager.GetCustomerInvoice(orderId, loadInvoiceAttachments: true);
                                        if (order != null)
                                            loadedOrders.Add(order);
                                        else
                                            continue;
                                    }

                                    if (order.InvoiceAttachment == null || (image.SourceType == InvoiceAttachmentSourceType.Edi ? !order.InvoiceAttachment.Any(i => i.EdiEntryId == image.Id) : !order.InvoiceAttachment.Any(i => i.DataStorageRecordId == image.Id)))
                                        result = InvoiceAttachmentManager.AddInvoiceAttachment(entities, orderId, image.Id, image.SourceType, InvoiceAttachmentConnectType.SupplierInvoice, order.AddSupplierInvoicesToEInvoices, true);
                                }
                            }
                        }

                        result = SaveChanges(entities, transaction);
                        if (!result.Success)
                            return result;

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
                    entities.Connection.Close();
                }
            }

            return result;
        }

        /// <summary>
        /// Check if specified invoice number already exist on another invoice for the specified actor (supplier/customer).
        /// Used to warn if a possible duplicate invoice is beeing registered.
        /// </summary>
        /// <param name="actorId">The actors id</param>
        /// <param name="invoiceNr">The invoice number</param>
        /// <returns>ActionResult with success = true if invoice found. Sequence number of existing invoice as IntegerValue</returns>
        public ActionResult SupplierInvoiceNumberExist(int supplierId, int exceptInvoiceId, string invoiceNr)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            entitiesReadOnly.Invoice.NoTracking();
            return this.SupplierInvoiceNumberExist(entitiesReadOnly, supplierId, exceptInvoiceId, invoiceNr);
        }

        public ActionResult SupplierInvoiceNumberExist(CompEntities entities, int supplierId, int exceptInvoiceId, string invoiceNr)
        {
            return InvoiceManager.InvoiceNumberExist(entities, SoeInvoiceType.SupplierInvoice, base.ActorCompanyId, exceptInvoiceId, invoiceNr, supplierId);
        }

        private bool IsValidInvoiceNbr(string invoiceNr)
        {
            return Regex.IsMatch(invoiceNr, @"^[a-zåäöA-ZåÄÖ0-9_/+,.\*\-\\]*$");

        }

        public void SavePurchaseDeliveryInvoice(CompEntities entities, SupplierInvoice invoice, List<PurchaseDeliveryInvoiceDTO> purchaseDeliveryInvoicesInput, int actorCompanyId)
        {
            foreach (var row in purchaseDeliveryInvoicesInput.Where(r => r.SupplierinvoiceId.HasValue))
            {
                //foreach (var row in invoiceGrp)
                //{
                    var disconnect = row.SupplierinvoiceId.GetValueOrDefault() == 0 && !row.LinkToInvoice;

                    PurchaseDeliveryInvoice purchaseDeliveryInvoice = GetPurchaseDeliveryInvoice(entities, row.PurchaseDeliveryInvoiceId, actorCompanyId);
                    if (disconnect && purchaseDeliveryInvoice != null)
                    {
                        entities.DeleteObject(purchaseDeliveryInvoice);
                    }
                    else if (purchaseDeliveryInvoice == null)
                    {
                        // add purchaseDeliveryInvoice
                        purchaseDeliveryInvoice = new PurchaseDeliveryInvoice
                        {
                            Price = row.Price,
                            Quantity = row.Quantity,
                            PurchaseRowId = row.PurchaseRowId,
                            SupplierInvoice = invoice,
                        };
                        invoice.PurchaseDeliveryInvoice.Add(purchaseDeliveryInvoice);
                    }
                    else 
                    {
                        purchaseDeliveryInvoice.Price = row.Price;
                        purchaseDeliveryInvoice.Quantity = row.Quantity;
                    }
                //}
            }
        }

        /// <summary>
        /// Adds/updates a SupplierInvoice with its SupplierInvoiceRows and SupplierInvoiceAccountRows
        /// </summary>
        /// <param name="invoiceInput">The SupplierInvoice to save</param>
        /// <param name="accountingRowsInput">Collection of AccountingRowDTOs to convert to SupplierInvoiceRow</param>
        /// <param name="projectRowsInput">Collection of SupplierInvoiceProjectRowDTO to convert to TimeCodeTransactions</param>
        /// <param name="actorCompanyId">The Company that owns the Invoice</param>
        /// <returns>ActionResult</returns>
        public ActionResult SaveSupplierInvoice(
            SupplierInvoiceDTO invoiceInput, 
            List<PurchaseDeliveryInvoiceDTO> purchaseInvoiceRows, 
            List<AccountingRowDTO> accountingRowsInput, 
            List<SupplierInvoiceProjectRowDTO> projectRowsInput, 
            List<SupplierInvoiceOrderRowDTO> orderRows, 
            List<SupplierInvoiceCostAllocationDTO> costAllocationRows, 
            int actorCompanyId, 
            int roleId, 
            bool createAttestVoucher, 
            bool forceUpdateRows, 
            bool ignoreInvoiceNrCheck = false, 
            bool disregardConcurrencyCheck = false)
        {
            if (invoiceInput == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "SupplierInvoice");

            // Default result is successful
            var result = new ActionResult();

            #region Init

            int invoiceId = invoiceInput.InvoiceId;
            int? seqNbr = invoiceInput.SeqNr;

            int accountYearId = 0;
            Dictionary<int, int> voucherHeadsDict = null;
            bool newScanningInvoice = false;

            List<int> connectedOrders = new List<int>();
            var sourceType = InvoiceAttachmentSourceType.DataStorage;
            int connectToId = 0;
            bool autoTransferToPayment = false;
            bool validateFIBankPaymentReference = false;
            DataStorage invoiceImageDataStorage = null;
            #endregion

            using (var entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    DateTime? prevDueDate = null;
                    SupplierInvoice invoice = null;
                    bool voucherDateChanged = false;
                    bool checkDuplicates = false;
                    bool allowEditAccountingRowsWhenVoucher = false;

                    #region Concurrency check

                    DateTime? modified = null;
                    string modifiedBy = null;
                    if (!disregardConcurrencyCheck)
                    {
                        if (base.IsEntityModified(entities, invoiceInput.InvoiceId, SoeEntityType.SupplierInvoice, invoiceInput.Modified, out modified, out modifiedBy))
                            return new ActionResult((int)ActionResultSave.EntityIsModifiedByOtherUser, string.Format(GetText(7739, "Fakturan är uppdaterad {0} av {1}. Om du sparar kommer den användarens ändringar att skrivas över.\nVill du fortsätta?"), modified, modifiedBy));
                    }

                    #endregion

                    using (var transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        //Get existing loaded SupplierInvoice
                        if (invoiceInput.InvoiceId != 0)
                        {
                            invoice = GetSupplierInvoice(entities, invoiceInput.InvoiceId, true, true, true, true, true, true, true, false);
                            voucherDateChanged = invoice.VoucherDate != invoiceInput.VoucherDate;
                            checkDuplicates = invoice.Type == (int)TermGroup_SupplierInvoiceType.Uploaded;
                        }

                        #region Prereq

                        autoTransferToPayment = SettingManager.GetBoolSetting(entities, SettingMainType.Company, (int)CompanySettingType.SupplierInvoiceAutoTransferAutogiroInvoicesToPayment, base.UserId, actorCompanyId, 0);
                        allowEditAccountingRowsWhenVoucher = SettingManager.GetBoolSetting(entities, SettingMainType.Company, (int)CompanySettingType.SupplierInvoiceAllowEditAccountingRows, base.UserId, actorCompanyId, 0);
                        if (invoiceInput.PaymentMethodId.HasValue && invoiceInput.PaymentMethodId.Value == 0)
                            invoiceInput.PaymentMethodId = null;

                        accountYearId = SettingManager.GetIntSetting(entities, SettingMainType.UserAndCompany, (int)UserSettingType.AccountingAccountYear, base.UserId, actorCompanyId, 0);
                        if (!AccountManager.IsDateWithinCurrentAccountYear(entities, actorCompanyId, invoiceInput.VoucherDate.HasValue ? invoiceInput.VoucherDate.Value : DateTime.Now))
                        {
                            if (invoiceInput.VoucherDate.HasValue)
                                // Date not matched, try to get accountyearid for voucherdate (Has to be open account year)
                                accountYearId = AccountManager.GetAccountYearId(entities, invoiceInput.VoucherDate.HasValue ? invoiceInput.VoucherDate.Value : DateTime.Now, actorCompanyId);
                        }

                        AccountPeriod accountPeriod = AccountManager.GetAccountPeriod(entities, invoiceInput.VoucherDate.HasValue ? invoiceInput.VoucherDate.Value : DateTime.Now, ActorCompanyId);
                        if (accountPeriod.Status != (int)TermGroup_AccountStatus.Open && (invoice == null || voucherDateChanged || (invoice.Origin.Status == (int)SoeOriginStatus.Draft || invoice.Origin.Status == (int)SoeOriginStatus.Origin)))
                            return new ActionResult((int)ActionResultSave.EntityNotCreated, GetText(4787, "Angiven redovisningsperiod är inte öppen"));

                        if (invoiceInput.ActorId == 0)
                            return new ActionResult((int)ActionResultSave.EntityNotCreated, GetText(2290, "Försök att spara om igen"));

                        validateFIBankPaymentReference = SettingManager.GetBoolSetting(entities, SettingMainType.Company, (int)CompanySettingType.FISupplierInvoiceOCRCheckReference, base.UserId, actorCompanyId, 0);

                        #endregion

                        #region ScanningEntry/EdiEntry

                        bool learnScanningDocument = false;
                        ScanningEntry scanningEntry = null;
                        EdiEntry ediEntry = null;

                        if (invoice == null || invoice.IsDraftOrOrigin())
                        {
                            if (invoiceInput.EdiEntryId != 0)
                            {
                                ediEntry = EdiManager.GetEdiEntry(entities, invoiceInput.EdiEntryId, actorCompanyId, loadScanning: true);
                                if (ediEntry != null)
                                {
                                    scanningEntry = ediEntry.ScanningEntryInvoice;
                                    var provider = (ScanningProvider)scanningEntry.Provider;
                                    if (scanningEntry != null && provider == ScanningProvider.ReadSoft)
                                    {
                                        result = EdiManager.UpdateScanningEntryChanges(entities, invoiceInput, scanningEntry, actorCompanyId);
                                        if (result.Success && result.IntegerValue > 0)
                                            learnScanningDocument = true;

                                        if (EdiManager.CloseScanningWhenTransferedToSupplierInvoice(entities, actorCompanyId))
                                            ediEntry.State = (int)SoeEntityState.Inactive;

                                        newScanningInvoice = invoiceInput.InvoiceId == 0;

                                        AddInvoiceTextActionsFromEdiEntry(entities, ediEntry, invoice);
                                    }
                                    else if (scanningEntry != null && provider == ScanningProvider.AzoraOne)
                                    {
                                        learnScanningDocument = true;
                                        var imageDataStorageRecord = GeneralManager.GetDataStorageRecord(entities, actorCompanyId, ediEntry.EdiEntryId, SoeDataStorageRecordType.EdiEntry_Document); 
                                        if (imageDataStorageRecord != null)
                                            invoiceImageDataStorage = GeneralManager.GetDataStorageByReference(entities, imageDataStorageRecord.DataStorageId);

                                        if (EdiManager.CloseScanningWhenTransferedToSupplierInvoice(entities, actorCompanyId))
                                            ediEntry.State = (int)SoeEntityState.Inactive;
                                    }
                                    else
                                    {
                                        sourceType = InvoiceAttachmentSourceType.Edi;
                                        connectToId = ediEntry.EdiEntryId;
                                    }
                                }
                            }
                        }
                        #endregion

                        #region Origin

                        Origin origin = invoice != null ? invoice.Origin : null;
                        if (origin == null)
                        {
                            origin = new Origin()
                            {
                                ActorCompanyId = actorCompanyId,
                                Type = (int)SoeOriginType.SupplierInvoice,
                            };
                            SetCreatedProperties(origin);
                            entities.Origin.AddObject(origin);
                        }
                        else
                        {
                            // Cannot change to Draft from Origin
                            if (invoice.Origin.Status == (int)SoeOriginStatus.Origin && invoiceInput.OriginStatus == SoeOriginStatus.Draft)
                                return new ActionResult((int)ActionResultSave.InvalidStateTransition);

                            SetModifiedProperties(invoice.Origin);
                        }

                        var voucherSeries = VoucherManager.GetVoucherSerie(entities, invoiceInput.VoucherSeriesId, actorCompanyId, true);
                        if (voucherSeries == null)
                            return new ActionResult(null, GetText(8403, "Verifikatserie saknas"));


                        origin.VoucherSeriesId = voucherSeries.VoucherSeriesId;
                        origin.VoucherSeriesTypeId = invoiceInput.VoucherSeriesTypeId;

                        if ((int)invoiceInput.OriginStatus > origin.Status)
                            origin.Status = (int)invoiceInput.OriginStatus;

                        origin.Description = invoiceInput.OriginDescription;

                        #endregion

                        #region Set SupplierInvoice values

                        if (invoice == null)
                        {
                            invoice = new SupplierInvoice
                            {
                                Type = (int)SoeInvoiceType.SupplierInvoice,
                                OnlyPayment = false,

                                //Set references
                                Origin = origin,
                            };
                            SetCreatedProperties(invoice);
                            entities.Invoice.AddObject(invoice);

                            // Set EdiEntry
                            if (ediEntry != null)
                            {
                                invoice.ImageStorageType = ediEntry.ImageStorageType;
                                ediEntry.Invoice = invoice;
                                ediEntry.InvoiceStatus = (int)TermGroup_EDIInvoiceStatus.Processed;
                                SetModifiedProperties(ediEntry);
                                learnScanningDocument = true;
                            }

                            #region previous invoice
                            if (invoiceInput.PrevInvoiceId != 0)
                            {
                                // Add mapping from originating invoice (used when credit)
                                OriginInvoiceMapping oimap = new OriginInvoiceMapping()
                                {
                                    Origin = invoice.Origin,
                                    Type = (int)InvoiceManager.GetOriginInvoiceMappingType(invoice),
                                    InvoiceId = invoiceInput.PrevInvoiceId
                                };
                                entities.OriginInvoiceMapping.AddObject(oimap);
                            }

                            #endregion

                            #region Inventory Link

                            foreach (var row in accountingRowsInput.Where(r => r.InventoryId != 0))
                            {
                                Inventory inventory = InventoryManager.GetInventory(entities, row.InventoryId, actorCompanyId);
                                if (inventory != null)
                                {
                                    invoice.Inventory.Add(inventory);

                                    // Update purchase log (link invoice)
                                    IEnumerable<InventoryLog> logs = InventoryManager.GetInventoryLogs(entities, row.InventoryId, TermGroup_InventoryLogType.Purchase, true, false);
                                    if (logs.Any())
                                        logs.First().Invoice = invoice;
                                }
                            }

                            #endregion
                        }
                        else
                        {
                            prevDueDate = invoice.DueDate;
                            SetModifiedProperties(invoice);
                        }

                        // Double check seqnr 
                        if (invoice.SeqNr != null && invoice.SeqNr != 0)
                            seqNbr = invoice.SeqNr;

                        if (!string.IsNullOrEmpty(invoiceInput.OCR))
                        {
                            if (!IsValidInvoiceNbr(invoiceInput.OCR))
                                return new ActionResult(7420, GetText(7420, "OCR numret innehåller ogiltiga tecken"));

                            if (validateFIBankPaymentReference && !ValidateFinnishBankPaymentReference(invoiceInput.OCR))
                                return new ActionResult(7556, GetText(7556, "Ogiltigt OCR nummer. Var god redigera och försök spara igen."));
                        }

                        invoice.ReferenceOur = invoiceInput.ReferenceOur;
                        invoice.ReferenceYour = invoiceInput.ReferenceYour;
                        invoice.VoucherDate = invoiceInput.VoucherDate;

                        if (!invoice.FullyPayed)
                        {
                            invoice.DueDate = invoiceInput.DueDate;
                            invoice.PaymentNr = invoiceInput.PaymentNr;
                            invoice.PaymentMethodId = invoiceInput.PaymentMethodId;
                            invoice.TimeDiscountDate = invoiceInput.TimeDiscountDate;
                            invoice.TimeDiscountPercent = invoiceInput.TimeDiscountPercent;
                            invoice.SysPaymentTypeId = invoiceInput.SysPaymentTypeId;

                            if (invoice.PaidAmount == 0 && (invoice.Origin.Status == (int)SoeOriginStatus.Draft || invoice.Origin.Status == (int)SoeOriginStatus.Origin || invoice.Origin.Status == (int)SoeOriginStatus.Voucher)) 
                            {
                                invoice.OCR = invoiceInput.OCR;
                                invoice.InvoiceNr = invoiceInput.InvoiceNr;
                            }
                        }

                        if (!invoice.FullyPayed)
                        {
                            if (invoice.PaidAmount == 0 && (invoice.Origin.Status == (int)SoeOriginStatus.Draft || invoice.Origin.Status == (int)SoeOriginStatus.Origin || invoice.Origin.Status == (int)SoeOriginStatus.Voucher))
                            {
                                // Check that invoice nbr has no invalid characters...
                                if (!string.IsNullOrEmpty(invoiceInput.InvoiceNr) && !IsValidInvoiceNbr(invoiceInput.InvoiceNr))
                                {
                                    return new ActionResult(7419, GetText(7419, "Fakturanumret innehåller ogiltiga tecken"));
                                }
                            }
                        }

                        if (invoice.IsDraftOrOrigin())
                        {
                            CompCurrency currency = (invoiceInput.CurrencyId == 0) ? CountryCurrencyManager.GetCompanyBaseCurrency(entities, actorCompanyId) :
                                                      CountryCurrencyManager.GetCompanyCurrency(entities, invoiceInput.CurrencyId, actorCompanyId);

                            if (currency == null)
                                return new ActionResult((int)ActionResultSave.EntityNotCreated, GetText(8417, "Valuta kunde inte mappas/hittas"));

                            invoice.ActorId = invoiceInput.ActorId;
                            invoice.BillingType = (int)invoiceInput.BillingType;
                            invoice.VatType = (int)invoiceInput.VatType;
                            invoice.SeqNr = seqNbr;
                            invoice.InvoiceNr = invoiceInput.InvoiceNr;
                            invoice.OCR = invoiceInput.OCR;
                            invoice.InvoiceDate = invoiceInput.InvoiceDate;

                            invoice.CurrencyId = currency.CurrencyId;
                            invoice.CurrencyRate = invoiceInput.CurrencyRate;
                            invoice.CurrencyDate = invoiceInput.CurrencyDate;
                            invoice.OrderNr = invoiceInput.OrderNr;
                            invoice.ProjectId = invoiceInput.ProjectId;
                            invoice.TotalAmount = invoiceInput.TotalAmount;
                            invoice.TotalAmountCurrency = invoiceInput.TotalAmountCurrency;
                            invoice.VATAmount = invoiceInput.VatAmount;
                            invoice.VATAmountCurrency = invoiceInput.VatAmountCurrency;
                            //invoice.PaidAmount = invoiceInput.PaidAmount;
                            //invoice.PaidAmountCurrency = invoiceInput.PaidAmountCurrency;
                            //invoice.FullyPayed = invoiceInput.FullyPayed;
                            invoice.InterimInvoice = invoiceInput.InterimInvoice;
                            invoice.VatDeductionAccountId = invoiceInput.VatDeductionAccountId;
                            invoice.VatDeductionPercent = invoiceInput.VatDeductionPercent;
                            invoice.VatDeductionType = (int)invoiceInput.VatDeductionType;
                            invoice.MultipleDebtRows = false; // Will be calculated
                            invoice.BlockPayment = invoiceInput.BlockPayment;
                            invoice.StatusIcon = (int)invoiceInput.StatusIcon;
                            invoice.DefaultDim1AccountId = invoiceInput.DefaultDim1AccountId != 0 ? invoiceInput.DefaultDim1AccountId : null;
                            invoice.DefaultDim2AccountId = invoiceInput.DefaultDim2AccountId != 0 ? invoiceInput.DefaultDim2AccountId : null;
                            invoice.DefaultDim3AccountId = invoiceInput.DefaultDim3AccountId != 0 ? invoiceInput.DefaultDim3AccountId : null;
                            invoice.DefaultDim4AccountId = invoiceInput.DefaultDim4AccountId != 0 ? invoiceInput.DefaultDim4AccountId : null;
                            invoice.DefaultDim5AccountId = invoiceInput.DefaultDim5AccountId != 0 ? invoiceInput.DefaultDim5AccountId : null;
                            invoice.DefaultDim6AccountId = invoiceInput.DefaultDim6AccountId != 0 ? invoiceInput.DefaultDim6AccountId : null;
                            invoice.VatCodeId = invoiceInput.VatCodeId > 0 ? invoiceInput.VatCodeId : null;

                            if (invoice.Type == (int)TermGroup_SupplierInvoiceType.Uploaded && (int)invoiceInput.Type == (int)TermGroup_SupplierInvoiceType.Invoice)
                                invoice.Type = (int)invoiceInput.Type;

                            if (invoiceInput.AttestGroupId == null || invoiceInput.AttestGroupId == 0)
                                invoice.AttestGroupId = GetAttestGroupId(entities, actorCompanyId, invoiceInput);
                            else
                                invoice.AttestGroupId = invoiceInput.AttestGroupId;
                        }

                        if (invoice.IsDraftOrOrigin() || allowEditAccountingRowsWhenVoucher)
                        {
                            result = SaveSupplierInvoiceAccountingRows(entities, invoice, accountingRowsInput, actorCompanyId, forceUpdateRows);
                            if (!result.Success)
                            {
                                return result;
                            }
                        }
                        #endregion

                        #region Purchase

                        if (!purchaseInvoiceRows.IsNullOrEmpty())
                        {
                            this.SavePurchaseDeliveryInvoice(entities, invoice, purchaseInvoiceRows, actorCompanyId);
                        }

                        #endregion

                        //check that no one has saved an invoice with same invoice number while creating this one
                        if ((invoice.InvoiceId == 0 || checkDuplicates) && !ignoreInvoiceNrCheck)
                        {
                            var supplierInvoiceNrExist = (from i in entities.Invoice
                                                          where i.Origin.Type == (int)SoeOriginType.SupplierInvoice &&
                                                                i.Origin.ActorCompanyId == actorCompanyId &&
                                                                i.State == (int)SoeEntityState.Active &&
                                                                i.ActorId == invoiceInput.ActorId &&
                                                                i.InvoiceNr == invoiceInput.InvoiceNr
                                                          orderby i.SeqNr
                                                          select i.InvoiceId).Any();

                            if (supplierInvoiceNrExist)
                            {
                                return new ActionResult((int)ActionResultSave.Duplicate, GetText(4863, 1));
                            }
                        }


                        if (result.Success)
                            result = SaveChanges(entities, transaction);

                        if (!result.Success)
                            return result;

                        #region Cost allocation rows

                        if (!costAllocationRows.IsNullOrEmpty())
                        {
                            if (!SupplierInvoiceCostAllocationHelper.AmountsAreValid(invoice.TotalAmountCurrency, invoice.VATAmountCurrency, costAllocationRows))
                            {
                                return new ActionResult((int)ActionResultSave.ProjectRowsAmountInvalid, GetText(5741, "Belopp för projektrader får ej överstiga fakturans totalbelopp"));
                            }

                            var costToProjectRows = costAllocationRows.Where(r => r.TimeCodeTransactionId != 0 || r.TimeCodeId != 0);
                            if (costToProjectRows.Count() > 0)
                            {
                                var prows = new List<SupplierInvoiceProjectRowDTO>();

                                foreach (var row in costToProjectRows)
                                {
                                    var prow = new SupplierInvoiceProjectRowDTO()
                                    {
                                        State = row.State,
                                        TimeCodeTransactionId = row.TimeCodeTransactionId,
                                        Amount = row.ProjectAmount,
                                        AmountCurrency = row.ProjectAmountCurrency,
                                        TimeInvoiceTransactionId = row.TimeInvoiceTransactionId ?? 0,
                                        SupplierInvoiceId = row.SupplierInvoiceId > 0 ? row.SupplierInvoiceId : invoice.InvoiceId,
                                        TimeCodeId = row.TimeCodeId,
                                        EmployeeId = row.EmployeeId,
                                        ChargeCostToProject = row.ChargeCostToProject,
                                        IncludeSupplierInvoiceImage = row.IncludeSupplierInvoiceImage,
                                        ProjectId = row.ProjectId,
                                        CustomerInvoiceId = row.OrderId,
                                    };
                                    prows.Add(prow);
                                }

                                result = ProjectManager.SaveSupplierInvoiceProjectRows(entities, transaction, invoice, prows, actorCompanyId, false);
                                if (!result.Success)
                                    return result;
                                else
                                    connectedOrders.AddRange(result.Value as List<int>);
                            }

                            var costToOrderRows = costAllocationRows.Where(r => (r.CustomerInvoiceRowId != 0 || r.OrderId != 0) && r.TimeCodeId == 0 && (r.EmployeeId == 0 || r.EmployeeId == null));
                            if (costToOrderRows.Any())
                            {
                                var miscProduct = ProductManager.GetInvoiceProductFromSetting(entities, CompanySettingType.ProductMisc, actorCompanyId, false, true);
                                if (miscProduct == null && costToOrderRows.Any(r => !r.ProductId.HasValue || r.ProductId.Value == 0))
                                    return new ActionResult(null, GetText(9144, "Ingen ströartikel hittad på företaget"));

                                var supplierName = SupplierManager.GetSupplierName(entities, invoiceInput.ActorId.Value, actorCompanyId);
                                var description = " - " + GetText(7712, "från leverantörsfaktura") + " " + invoiceInput.InvoiceNr + " " + (supplierName ?? "");

                                var orows = new List<SupplierInvoiceOrderRowDTO>();

                                foreach (var row in costToOrderRows)
                                {
                                    var orow = new SupplierInvoiceOrderRowDTO()
                                    {
                                        State = row.State,
                                        InvoiceProductId = row.ProductId.HasValue && row.ProductId != 0 ? row.ProductId.Value : miscProduct.ProductId,
                                        InvoiceProductName = row.ProductId.HasValue && row.ProductId != 0 ? row.ProductName : miscProduct.Name,
                                        SupplierInvoiceId = row.SupplierInvoiceId > 0 ? row.SupplierInvoiceId : invoice.InvoiceId,
                                        Amount = row.RowAmount,
                                        AmountCurrency = row.RowAmountCurrency,
                                        SumAmount = row.OrderAmount,
                                        SumAmountCurrency = row.OrderAmountCurrency,
                                        SupplementCharge = row.SupplementCharge,
                                        CustomerInvoiceId = row.OrderId,
                                        CustomerInvoiceRowId = row.CustomerInvoiceRowId,
                                        IncludeSupplierInvoiceImage = row.IncludeSupplierInvoiceImage,
                                        ProjectId = row.ProjectId,
                                    };
                                    orows.Add(orow);
                                }

                                result = InvoiceManager.SaveOrderRowFromSupplierInvoice(
                                    transaction, 
                                    entities, 
                                    invoice, 
                                    orows, 
                                    actorCompanyId, 
                                    roleId, 
                                    description, 
                                    voucherDateChanged, 
                                    isCostAllocation: true, 
                                    skipAmountRecalculation: true,
                                    fromBaseCurrency: true);
                                if (!result.Success)
                                {
                                    return result;
                                }
                                else
                                {
                                    foreach (var item in result.Value as List<int>)
                                    {
                                        if (!connectedOrders.Contains(item))
                                            connectedOrders.Add(item);
                                    }
                                }
                            }
                        }

                        #endregion

                        #region OLD Project/Order rows
                        /*#region Project rows

                        if (projectRowsInput != null)
                        {
                            result = ProjectManager.SaveSupplierInvoiceProjectRows(entities, transaction, invoice, projectRowsInput, actorCompanyId, false);
                            if (!result.Success)
                                return result;
                            else
                                connectedOrders.AddRange(result.Value as List<int>);
                        }

                        #endregion

                        #region Supplier invoice order rows

                        if (orderRows != null && orderRows.Count > 0)
                        {
                            #region New

                            foreach (SupplierInvoiceOrderRowDTO orderRow in orderRows)
                            {
                                if (orderRow.SupplierInvoiceId == 0)
                                    orderRow.SupplierInvoiceId = invoice.InvoiceId;
                            }

                            #endregion

                            var supplierName = SupplierManager.GetSupplierName(entities, invoiceInput.ActorId.Value, actorCompanyId);

                            string description = " - " + GetText(168, (int)TermGroup.SupplierInvoiceEdit, "från levfaktura") + " " + invoiceInput.InvoiceNr + " " + (supplierName ?? "");
                            result = InvoiceManager.SaveOrderRowFromSupplierInvoice(transaction, entities, invoice, orderRows, actorCompanyId, roleId, description, voucherDateChanged);
                            if (!result.Success)
                            {
                                return result;
                            }
                            else
                            {
                                foreach (var item in result.Value as List<int>)
                                {
                                    if (!connectedOrders.Contains(item))
                                        connectedOrders.Add(item);
                                }
                            }
                        }

                        #endregion*/
                        #endregion

                        if (invoice.IsDraftOrOrigin())
                        {
                            #region Calculate currency amounts

                            //Calculate currency amounts
                            CountryCurrencyManager.CalculateCurrencyAmounts(entities, actorCompanyId, invoice);

                            #endregion

                            #region ScanningEntry

                            if (scanningEntry != null && learnScanningDocument)
                            {
                                if (scanningEntry.Provider == (int)ScanningProvider.AzoraOne)
                                    EdiManager.BookKeepInvoice(entities, actorCompanyId, scanningEntry, invoiceInput, accountingRowsInput);
                                else
                                {
                                    Supplier supplier;
                                    if (invoice.Actor == null || invoice.Actor.Supplier == null)
                                        supplier = entities.Supplier.FirstOrDefault(s => invoiceInput.ActorId.Value == s.ActorSupplierId && s.ActorCompanyId == actorCompanyId);
                                    else
                                        supplier = invoice.Actor.Supplier;

                                    EdiManager.LearnScanningDocument(entities, scanningEntry, supplier, actorCompanyId);
                                }
                            }

                            #endregion

                            #region Validate invoice accounting rows

                            if (ValidateSupplierInvoiceAccountingRowsDiff(invoice))
                                return new ActionResult((int)ActionResultSave.HasUnbalancedAccountingRows, GetText(200, 33, "Debet och kredit balanserar inte. Kontrollera konteringsrader och spara igen."));

                            #endregion
                        }

                        #region Save

                        if (result.Success)
                            result = SaveChanges(entities, transaction);

                        if (result.Success)
                        {
                            invoiceId = invoice.Origin.OriginId;

                            //Commit transaction
                            transaction.Complete();
                        }

                        #endregion
                    }

                    if (invoice != null && result.Success)
                    {
                        #region Sequence number

                        if ((seqNbr == null || seqNbr == 0) && invoice.Origin.Status != (int)SoeOriginStatus.Draft)
                        {
                            seqNbr = InvoiceManager.GetNextSequenceNumber(entities, SoeOriginType.SupplierInvoice, (SoeOriginStatus)invoice.Origin.Status, (TermGroup_BillingType)invoice.BillingType, actorCompanyId, false);
                            invoice.SeqNr = seqNbr;

                            EdiEntry ediEntry = EdiManager.GetEdiEntryFromInvoice(entities, invoice.InvoiceId);

                            if (ediEntry != null && ediEntry.Type == (int)TermGroup_EDISourceType.Finvoice)
                                ediEntry.SeqNr = seqNbr;

                            result = SaveChanges(entities);
                        }


                        #endregion

                        #region Files

                        // New scanning invoice
                        if (newScanningInvoice && invoiceInput.ScanningImage != null)
                        {
                            var image = invoiceInput.ScanningImage;
                            var dto = new DataStorageRecordDTO()
                            {
                                RecordId = invoiceId,
                                Data = image.Image,
                                Entity = SoeEntityType.SupplierInvoice,
                                Type = image.ImageFormatType,
                                DataStorageRecordId = invoiceId,
                            };

                            result = GeneralManager.SaveDataStorageRecord(actorCompanyId, dto);
                            if (result.Success)
                            {
                                sourceType = InvoiceAttachmentSourceType.DataStorage;
                                connectToId = result.IntegerValue;
                                invoice.ImageStorageType = (int)SoeInvoiceImageStorageType.StoredInInvoiceDataStorage;
                            }
                        }
                        else if (invoiceImageDataStorage != null)
                        {
                            GeneralManager.CreateDataStorageRecord(entities,
                                type: SoeDataStorageRecordType.InvoiceBitmap,
                                recordId: invoiceId,
                                recordNumber: invoice.InvoiceNr,
                                entityType: SoeEntityType.SupplierInvoice,
                                dataStorage: invoiceImageDataStorage
                            );
                        }


                        if (invoiceInput.SupplierInvoiceFiles != null)
                        {
                            var images = invoiceInput.SupplierInvoiceFiles.Where(f => f.ImageId.HasValue);
                            GraphicsManager.UpdateImages(entities, images, invoiceId);
                            invoice.StatusIcon = GetStatusIcon(invoice.StatusIcon, images, SoeStatusIcon.Image);

                            var files = invoiceInput.SupplierInvoiceFiles.Where(f => f.Id.HasValue);
                            GeneralManager.UpdateFiles(entities, files, invoiceId);
                            invoice.StatusIcon = GetStatusIcon(invoice.StatusIcon, files, SoeStatusIcon.Attachment);
                            var invoiceFile = invoiceInput.SupplierInvoiceFiles.SingleOrDefault(f => f.IsSupplierInvoice);
                            if (invoiceFile != null)
                            {
                                sourceType = invoiceFile.SourceType.HasValue && invoiceFile.SourceType.Value != InvoiceAttachmentSourceType.None ? invoiceFile.SourceType.Value : InvoiceAttachmentSourceType.DataStorage;
                                connectToId = invoiceFile.Id.Value;

                                var dataStorageRecords = entities.DataStorageRecord.Where(d => d.Entity == (int)SoeEntityType.SupplierInvoice && d.RecordId == invoiceId && d.DataStorageRecordId != invoiceFile.Id).ToList();
                                foreach (var dataRecord in dataStorageRecords)
                                {
                                    GeneralManager.DeleteDataStorageRecord(entities, dataRecord.DataStorageRecordId, false);
                                }
                            }

                            entities.SaveChanges();
                        }

                        #endregion

                        #region InvoiceAttachments

                        var loadedOrders = new List<CustomerInvoice>();
                        if (connectedOrders != null && connectedOrders.Count > 0 && sourceType != InvoiceAttachmentSourceType.None && connectToId > 0)
                        {
                            foreach (var orderId in connectedOrders)
                            {
                                CustomerInvoice order = loadedOrders.FirstOrDefault(o => o.InvoiceId == orderId);
                                if (order == null)
                                {
                                    order = InvoiceManager.GetCustomerInvoice(orderId, loadInvoiceAttachments: true);
                                    if (order != null)
                                        loadedOrders.Add(order);
                                    else
                                        continue;
                                }

                                if (order.InvoiceAttachment == null || (sourceType == InvoiceAttachmentSourceType.Edi ? !order.InvoiceAttachment.Any(i => i.EdiEntryId == connectToId) : !order.InvoiceAttachment.Any(i => i.DataStorageRecordId == connectToId)))
                                    result = InvoiceAttachmentManager.AddInvoiceAttachment(entities, orderId, connectToId, sourceType, InvoiceAttachmentConnectType.SupplierInvoice, order.AddSupplierInvoicesToEInvoices, true);
                            }
                        }

                        #endregion

                        //voucher successfully saved, create account distribution entries
                        #region AccountDistributionEntry
                        if (invoice.IsDraftOrOrigin() || allowEditAccountingRowsWhenVoucher)
                        {
                            bool saveChanges = false;

                            //remove existing entries if voucher row deleted
                            foreach (var deletedRow in invoice.SupplierInvoiceRow.Where(i => i.State == (int)SoeEntityState.Deleted))
                            {
                                int rowId = 0;
                                var deletedAccRow = deletedRow.SupplierInvoiceAccountRow.FirstOrDefault();
                                if (deletedAccRow != null && deletedAccRow.AccountDistributionHeadId != 0)
                                    rowId = deletedAccRow.SupplierInvoiceAccountRowId;

                                //set previously created entries to state deleted
                                List<AccountDistributionEntry> existingEntries = AccountDistributionManager.GetAccountDistributionEntriesForSourceRow(entities, actorCompanyId, (int)TermGroup_AccountDistributionRegistrationType.SupplierInvoice, invoice.InvoiceId, rowId).ToList();
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

                            //create new entries
                            for (int i = 0; i < accountingRowsInput.Count; i++)
                            {
                                if (accountingRowsInput[i].AccountDistributionHeadId != 0 && accountingRowsInput[i].AccountDistributionNbrOfPeriods != 0)
                                {
                                    int rowId = invoice.SupplierInvoiceRow.ElementAt(i).SupplierInvoiceAccountRow.FirstOrDefault().SupplierInvoiceAccountRowId;

                                    //set previously created entries to state deleted
                                    List<AccountDistributionEntry> existingEntries = AccountDistributionManager.GetAccountDistributionEntriesForSourceRow(entities, actorCompanyId, (int)TermGroup_AccountDistributionRegistrationType.SupplierInvoice, invoice.InvoiceId, rowId).ToList();
                                    foreach (var entry in existingEntries)
                                    {
                                        entry.State = (int)SoeEntityState.Deleted;
                                        SetModifiedProperties(entry);
                                        saveChanges = true;
                                    }

                                    //create new entries
                                    List<AccountDistributionEntry> entries = AccountDistributionManager.CreateAccountDistributionEntriesFromAccountingRowDTO(entities, accountingRowsInput[i], rowId, actorCompanyId, (int)TermGroup_AccountDistributionRegistrationType.SupplierInvoice, invoice.InvoiceId);
                                    foreach (var entry in entries)
                                    {
                                        SetCreatedProperties(entry);
                                        entities.AccountDistributionEntry.AddObject(entry);
                                        saveChanges = true;
                                    }
                                }
                            }

                            //update invoice sequence number to account distribution's name
                            int headId = accountingRowsInput.Where(r => r.AccountDistributionHeadId != 0).Select(r => r.AccountDistributionHeadId).FirstOrDefault();
                            AccountDistributionHead accountDistributionHead = AccountDistributionManager.GetAccountDistributionHead(entities, headId);

                            if (accountDistributionHead != null && accountDistributionHead.Name.Contains("[") && accountDistributionHead.Name.Contains("]"))
                            {
                                int startIndex = accountDistributionHead.Name.IndexOf("[");
                                int length = accountDistributionHead.Name.IndexOf("]") - startIndex;
                                string term = accountDistributionHead.Name.Substring(startIndex, length + 1);
                                accountDistributionHead.Name = accountDistributionHead.Name.Replace(term, invoice.SeqNr.ToString());
                            }

                            if (saveChanges)
                                result = SaveChanges(entities);
                        }
                        #endregion

                        if (invoice.IsDraftOrOrigin())
                        {
                            #region Voucher

                            if (result.Success)
                            {
                                result = TryTransferSupplierInvoiceToVoucher(entities, ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, invoice, actorCompanyId);
                                if (result.Success)
                                    voucherHeadsDict = NumberUtility.MergeDictictionary(voucherHeadsDict, result.IdDict);

                                result = SaveChanges(entities);
                            }

                            #endregion

                            #region Attest voucher

                            if (result.Success && createAttestVoucher)
                            {
                                result = VoucherManager.SaveVoucherFromSupplierInvoiceAttestRows(entities, ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, invoice, actorCompanyId);
                                if (result.Success)
                                    voucherHeadsDict = NumberUtility.MergeDictictionary(voucherHeadsDict, result.IdDict);

                                result = SaveChanges(entities);
                            }

                            #endregion
                        }

                        #region Auto create payment

                        if (autoTransferToPayment && invoice.SysPaymentTypeId.HasValue && invoice.SysPaymentTypeId.Value == (int)TermGroup_SysPaymentType.Autogiro && (invoice.Origin.Status == (int)SoeOriginStatus.Origin || invoice.Origin.Status == (int)SoeOriginStatus.Voucher))
                        {
                            if (invoice.IsCredit ? invoice.TotalAmount - invoice.PaidAmount < 0 : invoice.TotalAmount - invoice.PaidAmount > 0)
                            {
                                var paymentMethods = PaymentManager.GetPaymentMethods(entities, actorCompanyId);
                                if (paymentMethods.Count > 0)
                                {
                                    var autogiroPaymentMethod = paymentMethods.FirstOrDefault(p => p.SysPaymentMethodId == (int)TermGroup_SysPaymentMethod.Autogiro);
                                    if (autogiroPaymentMethod != null)
                                        PaymentManager.SavePaymentFromSupplierInvoice(entities, invoice, autogiroPaymentMethod, TermGroup_SysPaymentType.Autogiro, SoeOriginStatusChange.SupplierInvoice_OriginToPayment, accountYearId, actorCompanyId, true, true, true);
                                }
                            }

                            if (prevDueDate.HasValue && prevDueDate != invoiceInput.DueDate)
                            {
                                // Change payment date on existing payment rows of type autogiro
                                var paymentRows = PaymentManager.GetPaymentRowsByInvoice(entities, invoice.InvoiceId);
                                foreach (var pRow in paymentRows.Where(p => p.SysPaymentTypeId == (int)TermGroup_SysPaymentType.Autogiro && p.Status != (int)SoePaymentStatus.Checked))
                                {
                                    pRow.PayDate = invoice.DueDate.Value;
                                    SetModifiedProperties(pRow);
                                }
                            }
                        }

                        #endregion
                    }
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    result.Exception = ex;
                    result.IntegerValue = 0;
                    seqNbr = 0;
                }
                finally
                {
                    if (result.Success)
                    {
                        //Set success properties
                        result.IntegerValue = invoiceId;
                        result.StringValue = invoiceInput.InvoiceNr;
                        result.Value = seqNbr;
                        if (voucherHeadsDict != null)
                        {
                            result.IdDict = voucherHeadsDict;
                        }
                    }
                    else
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }

                return result;
            }
        }

        /// <summary>
        /// Looks at all accounting dimension for invoice, extracts cost place id and finds mapped attest group if one exists.
        /// </summary>
        /// <param name="entities">DB context</param>
        /// <param name="actorCompanyId">Company unique id</param>
        /// <param name="invoiceInput">Supplier invoice DTO</param>
        /// <returns>AttestGroupId or Null if no match</returns>
        private int? GetAttestGroupId(CompEntities entities, int actorCompanyId, SupplierInvoiceDTO invoiceInput)
        {
            var costplaceAccountIdOptions = new List<int>()
            {
                invoiceInput.DefaultDim1AccountId ?? 0,
                invoiceInput.DefaultDim2AccountId ?? 0,
                invoiceInput.DefaultDim3AccountId ?? 0,
                invoiceInput.DefaultDim4AccountId ?? 0,
                invoiceInput.DefaultDim5AccountId ?? 0,
                invoiceInput.DefaultDim6AccountId ?? 0
            };
            costplaceAccountIdOptions.RemoveAll(o => o == 0);

            int? costplaceId = null;
			if (costplaceAccountIdOptions.Count > 0)
            {
				int? costCenterDimId = AccountManager.GetAccountDimBySieNr(entities, (int)TermGroup_SieAccountDim.CostCentre, actorCompanyId)?.AccountDimId;
				if (costCenterDimId != null)
                {
				    // Extract cost place id from the list of accounting members. Solution required for scenarios where an accounting dimension has been deleted.
				    costplaceId = entities.Account.FirstOrDefault(a => 
                        a.ActorCompanyId == actorCompanyId && 
                        a.AccountDimId == costCenterDimId && 
                        costplaceAccountIdOptions.Contains(a.AccountId))?.AccountId;
				}
			}

			AttestWorkFlowGroup attestWorkFlowGroup = AttestManager.GetAttestGroupSuggestion(
                entities,
                actorCompanyId, 
                invoiceInput.ActorId != null ? (int)invoiceInput.ActorId : 0, 
                invoiceInput.ProjectId != null ? (int)invoiceInput.ProjectId : 0, 
                costplaceId ?? 0, 
                invoiceInput.ReferenceOur);
            
            return attestWorkFlowGroup?.AttestWorkFlowHeadId;
        }

        public PurchaseDeliveryInvoice GetPurchaseDeliveryInvoice(CompEntities entities, int PurchaseDeliveryInvoiceId, int actorCompanyId)
        {
            if (PurchaseDeliveryInvoiceId == 0)
                return null;

            var obj = (
                from
                    r in entities.PurchaseDeliveryInvoice.Include("SupplierInvoice").Include("SupplierInvoice.Origin")
                where
                    r.PurchaseDeliveryInvoiceId == PurchaseDeliveryInvoiceId && r.SupplierInvoice.Origin.ActorCompanyId == actorCompanyId
                select r).FirstOrDefault();
            return obj;
        }

        public ActionResult SaveSupplierInvoiceAccountingRows(CompEntities entities, SupplierInvoice invoice, List<AccountingRowDTO> accountingRowsInput, int actorCompanyId, bool forceUpdateRows)
        {
            List<AccountInternal> accountInternals = AccountManager.GetAccountInternals(entities, actorCompanyId, true);

            // Convert collection of AccountingRowDTOs to collection of SupplierInvoiceRow with SupplierInvoiceAccountRows
            List<SupplierInvoiceRow> invoiceRowsInput = ConvertToSupplierInvoiceRows(entities, accountingRowsInput, accountInternals, actorCompanyId);

            if (!invoiceRowsInput.Any() || ((invoiceRowsInput.Sum(x => Math.Abs(x.Amount)) == 0) && invoice.TotalAmount != 0))
            {
                return new ActionResult((int)ActionResultSave.EntityNotCreated, GetText(7405, "Summan av konteringsraderna får inte vara 0"));
            }

            #region SupplierInvoiceRows

            int noOfDebtRows = 0;

            #region SupplierInvoiceRow Update/Delete

            // Update or Delete existing SupplierInvoiceRows
            foreach (SupplierInvoiceRow invoiceRow in invoice.ActiveSupplierInvoiceRows)
            {
                #region SupplierInvoiceRow

                // Skip attest rows
                if (invoiceRow.SupplierInvoiceAccountRow.Any(r => r.Type == (int)AccountingRowType.SupplierInvoiceAttestRow))
                    continue;

                // Try get SupplierInvoiceRow from input
                SupplierInvoiceRow invoiceRowInput = (from r in invoiceRowsInput
                                                      where r.SupplierInvoiceRowId == invoiceRow.SupplierInvoiceRowId
                                                      select r).FirstOrDefault();

                if (invoiceRowInput != null)
                {
                    // Update existing SupplierInvoiceRow
                    invoiceRow.Amount = invoiceRowInput.Amount;
                    invoiceRow.AmountCurrency = invoiceRowInput.AmountCurrency;
                    invoiceRow.AmountEntCurrency = invoiceRowInput.AmountEntCurrency;
                    invoiceRow.AmountLedgerCurrency = invoiceRowInput.AmountLedgerCurrency;
                    invoiceRow.Quantity = invoiceRowInput.Quantity;
                    invoiceRow.State = invoiceRowInput.State;
                    SetModifiedProperties(invoiceRow);

                    foreach (SupplierInvoiceAccountRow invoiceAccountRow in invoiceRow.ActiveSupplierInvoiceAccountRows)
                    {
                        #region SupplierInvoiceAccountRow

                        // Count debt rows
                        if (invoiceAccountRow.ContractorVatRow == false && IsSupplierInvoiceDebtRow(invoice.BillingType, invoiceAccountRow.DebitRow, invoiceAccountRow.CreditRow))
                            noOfDebtRows++;

                        // Try get SupplierInvoiceAccountRow from input
                        SupplierInvoiceAccountRow invoiceAccountRowInput = (from r in invoiceRowInput.SupplierInvoiceAccountRow
                                                                            where r.SupplierInvoiceAccountRowId == invoiceAccountRow.SupplierInvoiceAccountRowId
                                                                            select r).FirstOrDefault();

                        if (invoiceAccountRowInput != null)
                        {
                            // Update existing SupplierInvoiceAccountRow
                            invoiceAccountRow.Type = invoiceAccountRowInput.Type;
                            invoiceAccountRow.RowNr = invoiceAccountRowInput.RowNr;
                            invoiceAccountRow.SplitType = invoiceAccountRowInput.SplitType;
                            invoiceAccountRow.SplitPercent = invoiceAccountRowInput.SplitPercent;
                            invoiceAccountRow.Amount = invoiceAccountRowInput.Amount;
                            invoiceAccountRow.AmountCurrency = invoiceAccountRowInput.AmountCurrency;
                            invoiceAccountRow.AmountEntCurrency = invoiceAccountRowInput.AmountEntCurrency;
                            invoiceAccountRow.AmountLedgerCurrency = invoiceAccountRowInput.AmountLedgerCurrency;
                            invoiceAccountRow.Quantity = invoiceAccountRowInput.Quantity;
                            invoiceAccountRow.Text = invoiceAccountRowInput.Text;
                            invoiceAccountRow.InterimRow = invoiceAccountRowInput.InterimRow;
                            invoiceAccountRow.VatRow = invoiceAccountRowInput.VatRow;
                            invoiceAccountRow.ContractorVatRow = invoiceAccountRowInput.ContractorVatRow;
                            invoiceAccountRow.CreditRow = invoiceAccountRowInput.CreditRow;
                            invoiceAccountRow.DebitRow = invoiceAccountRowInput.DebitRow;
                            invoiceAccountRow.AttestStatus = invoiceAccountRowInput.AttestStatus;
                            invoiceAccountRow.AttestUserId = invoiceAccountRowInput.AttestUserId;
                            invoiceAccountRow.State = invoiceAccountRowInput.State;
                            invoiceAccountRow.AccountDistributionHeadId = invoiceAccountRowInput.AccountDistributionHeadId;
                            invoiceAccountRow.StartDate = invoiceAccountRowInput.StartDate;
                            invoiceAccountRow.NumberOfPeriods = invoiceAccountRowInput.NumberOfPeriods;

                            // AccountStd
                            invoiceAccountRow.AccountStd = invoiceAccountRowInput.AccountStd;

                            // AccountInternal
                            if (invoiceAccountRowInput.AccountInternal != null)
                            {
                                // Clear all
                                invoiceAccountRow.AccountInternal.Clear();

                                foreach (AccountInternal accountInternalInput in invoiceAccountRowInput.AccountInternal)
                                {
                                    // Add AccountInternal
                                    AccountInternal accountInternal = accountInternals.FirstOrDefault(a => a.AccountId == accountInternalInput.AccountId);
                                    if (accountInternal != null)
                                        invoiceAccountRow.AccountInternal.Add(accountInternal);
                                }
                            }

                            // Detach the input row to prevent adding a new
                            base.TryDetachEntity(entities, invoiceAccountRowInput);
                        }
                        else
                        {
                            // Delete existing SupplierInvoiceAccountRow
                            // Can only delete when Origin has status Draft or Origin
                            if (invoice.Origin.Status == (int)SoeOriginStatus.Draft || invoice.Origin.Status == (int)SoeOriginStatus.Origin)
                                ChangeEntityState(invoiceAccountRow, SoeEntityState.Deleted);
                        }

                        #endregion
                    }

                    // Detach the input row to prevent adding a new
                    base.TryDetachEntity(entities, invoiceRowInput);
                }
                else
                {
                    // Can only delete when Origin has status Draft or Origin
                    if (invoice.Origin.Status == (int)SoeOriginStatus.Draft || invoice.Origin.Status == (int)SoeOriginStatus.Origin || forceUpdateRows)
                    {
                        // Delete existing SupplierInvoiceRow
                        ChangeEntityState(invoiceRow, SoeEntityState.Deleted);

                        foreach (SupplierInvoiceAccountRow invoiceAccountRow in invoiceRow.ActiveSupplierInvoiceAccountRows)
                        {
                            // Delete existing SupplierInvoiceAccountRow
                            ChangeEntityState(invoiceAccountRow, SoeEntityState.Deleted);
                        }
                    }
                }

                #endregion
            }

            #endregion

            #region SupplierInvoiceRow Add

            // Get new SupplierInvoiceRows
            IEnumerable<SupplierInvoiceRow> invoiceRowsToAdd = (from r in invoiceRowsInput
                                                                where r.SupplierInvoiceRowId == 0
                                                                select r).ToList();

            foreach (SupplierInvoiceRow invoiceRowToAdd in invoiceRowsToAdd)
            {
                foreach (SupplierInvoiceAccountRow invoiceAccountRowToAdd in invoiceRowToAdd.SupplierInvoiceAccountRow)
                {
                    // Count debt rows
                    if (invoiceAccountRowToAdd.ContractorVatRow == false && IsSupplierInvoiceDebtRow(invoice.BillingType, invoiceAccountRowToAdd.DebitRow, invoiceAccountRowToAdd.CreditRow))
                        noOfDebtRows++;
                }

                // Add SupplierInvoiceRow to SupplierInvoice
                invoice.SupplierInvoiceRow.Add(invoiceRowToAdd);
            }

            #endregion

            // Check for multiple debt rows
            if (noOfDebtRows > 1)
                invoice.MultipleDebtRows = true;

            #endregion

            return new ActionResult(true);
        }

        public ActionResult SaveSupplierInvoiceAccountingRows(int invoiceId, List<AccountingRowDTO> accountingRowsInput, Dictionary<string, string> currentDimIds, int actorCompanyId)
        {
            var result = new ActionResult();

            using (var entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        SupplierInvoice invoice = GetSupplierInvoice(entities, invoiceId, loadInvoiceRow: true, loadInvoiceAccountRow: true);
                        if (invoice == null)
                            return new ActionResult((int)ActionResultSave.EntityNotFound, "SupplierInvoice");

                        if (currentDimIds != null && currentDimIds.Any())
                        {
                            foreach (var key in currentDimIds.Keys)
                            {
                                switch (key)
                                {
                                    case "dim2Id":
                                        invoice.DefaultDim2AccountId = !string.IsNullOrEmpty(currentDimIds["dim2Id"]) ? Convert.ToInt32(currentDimIds["dim2Id"]) : (int?)null;
                                        break;
                                    case "dim3Id":
                                        invoice.DefaultDim3AccountId = !string.IsNullOrEmpty(currentDimIds["dim3Id"]) ? Convert.ToInt32(currentDimIds["dim3Id"]) : (int?)null;
                                        break;
                                    case "dim4Id":
                                        invoice.DefaultDim4AccountId = !string.IsNullOrEmpty(currentDimIds["dim4Id"]) ? Convert.ToInt32(currentDimIds["dim4Id"]) : (int?)null;
                                        break;
                                    case "dim5Id":
                                        invoice.DefaultDim5AccountId = !string.IsNullOrEmpty(currentDimIds["dim5Id"]) ? Convert.ToInt32(currentDimIds["dim5Id"]) : (int?)null;
                                        break;
                                    case "dim6Id":
                                        invoice.DefaultDim6AccountId = !string.IsNullOrEmpty(currentDimIds["dim6Id"]) ? Convert.ToInt32(currentDimIds["dim6Id"]) : (int?)null;
                                        break;
                                }
                            }
                        }

                        result = SaveSupplierInvoiceAccountingRows(entities, invoice, accountingRowsInput, actorCompanyId, false);
                        if (!result.Success)
                            return result;

                        result = SaveChanges(entities, transaction);
                        if (result.Success)
                        {
                            //Commit transaction
                            transaction.Complete();
                        }
                    }

                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    result.Exception = ex;
                }
                finally
                {

                    entities.Connection.Close();
                }
            }

            return result;
        }

        public ActionResult SaveSupplierInvoiceAttestRows(int invoiceId, List<AccountingRowDTO> accountingRowDTOsInput, int actorCompanyId)
        {
            // Default result is successful
            ActionResult result = new ActionResult();

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        #region Prereq

                        // Get invoice
                        SupplierInvoice invoice = GetSupplierInvoice(entities, invoiceId, loadInvoiceRow: true, loadInvoiceAccountRow: true);
                        if (invoice == null)
                            return new ActionResult((int)ActionResultSave.EntityNotFound, "SupplierInvoice");

                        List<AccountInternal> accountInternals = AccountManager.GetAccountInternals(entities, actorCompanyId, true);

                        #endregion

                        #region Convert

                        // Convert collection of AccountingRowDTOs to collection of SupplierInvoiceRow with SupplierInvoiceAccountRows
                        List<SupplierInvoiceRow> invoiceRowsInput = ConvertToSupplierInvoiceRows(entities, accountingRowDTOsInput, accountInternals, actorCompanyId);

                        #endregion

                        #region SupplierInvoiceRows

                        #region SupplierInvoiceRow Update/Delete

                        // Update or Delete existing SupplierInvoiceRows (of type SupplierInvoiceAttestRow)
                        foreach (SupplierInvoiceRow invoiceRow in invoice.ActiveSupplierInvoiceRows)
                        {
                            if (!invoiceRow.SupplierInvoiceAccountRow.Any(r => r.Type == (int)AccountingRowType.SupplierInvoiceAttestRow))
                                continue;

                            #region SupplierInvoiceRow

                            // Try get SupplierInvoiceRow from input
                            SupplierInvoiceRow invoiceRowInput = (from r in invoiceRowsInput
                                                                  where r.SupplierInvoiceRowId == invoiceRow.SupplierInvoiceRowId
                                                                  select r).FirstOrDefault();

                            if (invoiceRowInput != null)
                            {


                                // Update existing SupplierInvoiceRow
                                // TODO: invoiceRowInput is in Added state, ApplyPropertyChanges will crash
                                // Detaching the entity will detach all its relations too, and we don't want that
                                invoiceRow.Amount = invoiceRowInput.Amount;
                                invoiceRow.AmountCurrency = invoiceRowInput.AmountCurrency;
                                invoiceRow.AmountEntCurrency = invoiceRowInput.AmountEntCurrency;
                                invoiceRow.AmountLedgerCurrency = invoiceRowInput.AmountLedgerCurrency;
                                invoiceRow.Quantity = invoiceRowInput.Quantity;
                                invoiceRow.State = invoiceRowInput.State;
                                SetModifiedProperties(invoiceRow);

                                foreach (SupplierInvoiceAccountRow invoiceAccountRow in invoiceRow.ActiveSupplierInvoiceAttestAccountRows)
                                {
                                    #region SupplierInvoiceAccountRow

                                    // Try get SupplierInvoiceAccountRow from input
                                    SupplierInvoiceAccountRow invoiceAccountRowInput = (from r in invoiceRowInput.SupplierInvoiceAccountRow
                                                                                        where r.SupplierInvoiceAccountRowId == invoiceAccountRow.SupplierInvoiceAccountRowId
                                                                                        select r).FirstOrDefault();

                                    if (invoiceAccountRowInput != null)
                                    {
                                        // Update existing SupplierInvoiceAccountRow
                                        // TODO: invoiceAccountRowInput is in Added state, ApplyPropertyChanges will crash
                                        // Detaching the entity will detach all its relations too, and we don't want that

                                        invoiceAccountRow.Type = invoiceAccountRowInput.Type;
                                        invoiceAccountRow.RowNr = invoiceAccountRowInput.RowNr;
                                        invoiceAccountRow.SplitType = invoiceAccountRowInput.SplitType;
                                        invoiceAccountRow.SplitPercent = invoiceAccountRowInput.SplitPercent;
                                        invoiceAccountRow.Amount = invoiceAccountRowInput.Amount;
                                        invoiceAccountRow.AmountCurrency = invoiceAccountRowInput.AmountCurrency;
                                        invoiceAccountRow.AmountEntCurrency = invoiceAccountRowInput.AmountEntCurrency;
                                        invoiceAccountRow.AmountLedgerCurrency = invoiceAccountRowInput.AmountLedgerCurrency;
                                        invoiceAccountRow.Quantity = invoiceAccountRowInput.Quantity;
                                        invoiceAccountRow.Text = invoiceAccountRowInput.Text;
                                        invoiceAccountRow.InterimRow = invoiceAccountRowInput.InterimRow;
                                        invoiceAccountRow.VatRow = invoiceAccountRowInput.VatRow;
                                        invoiceAccountRow.ContractorVatRow = invoiceAccountRowInput.ContractorVatRow;
                                        invoiceAccountRow.CreditRow = invoiceAccountRowInput.CreditRow;
                                        invoiceAccountRow.DebitRow = invoiceAccountRowInput.DebitRow;
                                        invoiceAccountRow.AttestStatus = invoiceAccountRowInput.AttestStatus;
                                        invoiceAccountRow.AttestUserId = invoiceAccountRowInput.AttestUserId;
                                        invoiceAccountRow.State = invoiceAccountRowInput.State;

                                        // AccountStd
                                        invoiceAccountRow.AccountStd = invoiceAccountRowInput.AccountStd;

                                        // AccountInternal
                                        if (invoiceAccountRowInput.AccountInternal != null)
                                        {
                                            // Clear all
                                            invoiceAccountRow.AccountInternal.Clear();

                                            foreach (AccountInternal accountInternalInput in invoiceAccountRowInput.AccountInternal)
                                            {
                                                // Add AccountInternal
                                                AccountInternal accountInternal = accountInternals.FirstOrDefault(a => a.AccountId == accountInternalInput.AccountId);
                                                if (accountInternal != null)
                                                    invoiceAccountRow.AccountInternal.Add(accountInternal);
                                            }
                                        }

                                        // Detach the input row to prevent adding a new
                                        // TODO: Remove this when ApplyPropertyChanges above works
                                        base.TryDetachEntity(entities, invoiceAccountRowInput);
                                    }
                                    else
                                    {
                                        // Delete existing SupplierInvoiceAccountRow
                                        // Can only delete when Origin has status Draft or Origin
                                        if (invoice.Origin.Status == (int)SoeOriginStatus.Draft || invoice.Origin.Status == (int)SoeOriginStatus.Origin)
                                            ChangeEntityState(invoiceAccountRow, SoeEntityState.Deleted);
                                    }

                                    #endregion
                                }

                                // Detach the input row to prevent adding a new
                                // TODO: Remove this when ApplyPropertyChanges above works
                                base.TryDetachEntity(entities, invoiceRowInput);
                            }
                            else
                            {
                                // Can only delete when Origin has status Draft or Origin
                                if (invoice.Origin.Status == (int)SoeOriginStatus.Draft ||
                                   invoice.Origin.Status == (int)SoeOriginStatus.Origin)
                                {
                                    // Delete existing SupplierInvoiceRow
                                    ChangeEntityState(invoiceRow, SoeEntityState.Deleted);

                                    foreach (SupplierInvoiceAccountRow invoiceAccountRow in invoiceRow.ActiveSupplierInvoiceAttestAccountRows)
                                    {
                                        // Delete existing SupplierInvoiceAccountRow
                                        ChangeEntityState(invoiceAccountRow, SoeEntityState.Deleted);
                                    }
                                }
                            }

                            #endregion
                        }

                        #endregion

                        #region SupplierInvoiceRow Add

                        // Get new SupplierInvoiceRows
                        IEnumerable<SupplierInvoiceRow> invoiceRowsToAdd = (from r in invoiceRowsInput
                                                                            where r.SupplierInvoiceRowId == 0
                                                                            select r).ToList();

                        foreach (SupplierInvoiceRow invoiceRowToAdd in invoiceRowsToAdd)
                        {
                            // Add SupplierInvoiceRow to SupplierInvoice
                            SetCreatedProperties(invoiceRowToAdd);
                            invoice.SupplierInvoiceRow.Add(invoiceRowToAdd);
                        }

                        #endregion

                        #endregion

                        #region Save

                        result = SaveChanges(entities, transaction);
                        if (result.Success)
                        {
                            //Commit transaction
                            transaction.Complete();
                        }

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
                        //Set success properties
                    }
                    else
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }

                return result;
            }
        }

        private static int GetStatusIcon(int currentStatusIcon, IEnumerable<FileUploadDTO> files, SoeStatusIcon icon)
        {
            if (files.Any(i => !i.IsDeleted && !i.IsSupplierInvoice))
                currentStatusIcon |= (int)icon;
            else
                currentStatusIcon &= ~(int)icon;
            return currentStatusIcon;
        }

        public ActionResult SaveSupplierInvoiceChangedAttestGroupId(int invoiceId, int attestGroupId)
        {
            // Default result is successful
            ActionResult result = new ActionResult();

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {


                        // Get invoice
                        SupplierInvoice invoice = GetSupplierInvoice(entities, invoiceId, loadInvoiceRow: false, loadInvoiceAccountRow: false);
                        if (invoice == null)
                            return new ActionResult((int)ActionResultSave.EntityNotFound, "SupplierInvoice");



                        invoice.AttestGroupId = attestGroupId > 0 ? attestGroupId : (int?)null;
                        SetModifiedProperties(invoice);

                        result = SaveChanges(entities, transaction);
                        if (result.Success)
                        {
                            //Commit transaction
                            transaction.Complete();
                        }


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
                    }
                    else
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }

                return result;
            }
        }

        public ActionResult TransferCustomerInvoicesToVoucherFromAngular(List<int> itemsDict, int accountYearId, int actorCompanyId)
        {
            ActionResult result = new ActionResult();
            using (CompEntities entities = new CompEntities())
            {
                List<CustomerInvoice> customerInvoices = new List<CustomerInvoice>();

                foreach (int invoiceId in itemsDict)
                {
                    CustomerInvoice customerInvoice = InvoiceManager.GetCustomerInvoice(entities, invoiceId, true, true, true, false, true, true, true, false);
                    if (customerInvoice == null)
                        return new ActionResult((int)ActionResultSave.EntityNotFound, "CustomerInvoice Id:" + invoiceId.ToString());

                    customerInvoices.Add(customerInvoice);
                }
                result = VoucherManager.SaveVoucherFromCustomerInvoices(entities, ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, customerInvoices, actorCompanyId);
            }

            if (!result.Success)
                result.ErrorMessage = InvoiceManager.GetErrorMessage(result.ErrorNumber, result.StringValue);

            return result;
        }

        public ActionResult TransferSupplierInvoicesToVoucherFromAngular(List<int> itemsDict, int actorCompanyId)
        {
            ActionResult result = new ActionResult();
            using (CompEntities entities = new CompEntities())
            {
                var supplierInvoices = (from i in entities.Invoice.OfType<SupplierInvoice>()
                                        .Include("Origin")
                                        .Include("Actor.Supplier")
                                        .Include("SupplierInvoiceRow.SupplierInvoiceAccountRow.AccountStd")
                                        .Include("SupplierInvoiceRow.SupplierInvoiceAccountRow.AccountInternal")
                                        where i.Origin.ActorCompanyId == actorCompanyId &&
                                        itemsDict.Contains(i.InvoiceId) &&
                                        i.State == (int)SoeEntityState.Active
                                        select i).ToList();

                string notTransferred = string.Empty;
                result = VoucherManager.SaveVoucherFromSupplierInvoices(entities, ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, supplierInvoices, actorCompanyId, notTransferred);
            }

            if (!result.Success)
                result.ErrorMessage = InvoiceManager.GetErrorMessage(result.ErrorNumber, result.StringValue);

            return result;
        }

        public void TransferSupplierInvoicesToVoucherFromAngularWithPolling(List<int> itemsDict, int actorCompanyId, ref SoeProgressInfo info, SoeMonitor monitor)
        {
            ActionResult result = new ActionResult();
            using (CompEntities entities = new CompEntities())
            {
                info.Message = GetText(9329, "Hämtar leverantörsfakturor");
                var supplierInvoices = (from i in entities.Invoice.OfType<SupplierInvoice>()
                                        .Include("Origin")
                                        .Include("Actor.Supplier")
                                        .Include("SupplierInvoiceRow.SupplierInvoiceAccountRow.AccountStd")
                                        .Include("SupplierInvoiceRow.SupplierInvoiceAccountRow.AccountInternal")
                                        where i.Origin.ActorCompanyId == actorCompanyId &&
                                        itemsDict.Contains(i.InvoiceId) &&
                                        i.State == (int)SoeEntityState.Active
                                        select i).ToList();

                string notTransferred = string.Empty;
                result = VoucherManager.SaveVoucherFromSupplierInvoicesWithPolling(entities, ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, supplierInvoices, actorCompanyId, notTransferred, ref info);
            }

            info.Abort = true;
            if (result.Success)
            {
                monitor.AddResult(info.PollingKey, result);
            }
            else
            {
                result.ErrorMessage = InvoiceManager.GetErrorMessage(result.ErrorNumber, result.StringValue);
                monitor.AddResult(info.PollingKey, result);

            }
        }

        public ActionResult TryTransferSupplierInvoiceToVoucherAcceptedAttest(CompEntities entities, List<int> invoiceIds, int actorCompanyId)
        {
            ActionResult result = new ActionResult(true);

            bool transferToVoucher = SettingManager.GetBoolSetting(entities, SettingMainType.Company, (int)CompanySettingType.SupplierInvoiceTransferToVoucherOnAcceptedAttest, 0, actorCompanyId, 0);
            if (!transferToVoucher)
                return result;

            List<SupplierInvoice> supplierInvoices = new List<SupplierInvoice>();

            foreach (int invoiceId in invoiceIds)
            {
                SupplierInvoice supplierInvoice = GetSupplierInvoice(entities, invoiceId, true, true, true, false, true, true, true, false);
                if (supplierInvoice == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "SupplierInvoice");

                supplierInvoices.Add(supplierInvoice);
            }

            List<SupplierInvoice> supplierInvoicesToTransfer = new List<SupplierInvoice>();

            foreach (SupplierInvoice supplierInvoice in supplierInvoices)
            {
                // Make sure VoucherHead is loaded
                if (!supplierInvoice.IsAdded() && !supplierInvoice.VoucherHeadReference.IsLoaded)
                {
                    supplierInvoice.VoucherHeadReference.Load();
                }

                //Check if already transferred
                if (supplierInvoice.VoucherHead != null)
                    continue;

                //Check if valid to be transferred
                bool isValid = IsSupplierInvoiceValidForVoucher(supplierInvoice.Origin.Status);
                if (!isValid)
                    continue;

                LoadSupplierInvoiceRows(supplierInvoice, true);

                supplierInvoicesToTransfer.Add(supplierInvoice);
            }

            if (supplierInvoicesToTransfer.Count > 0)
                result = VoucherManager.SaveVoucherFromSupplierInvoices(entities, ConfigSettings.TRANSACTIONSCOPEOPTION_ATTACH, supplierInvoicesToTransfer, actorCompanyId);

            return result;
        }

        public ActionResult TryTransferSupplierInvoiceToVoucher(CompEntities entities, TransactionScopeOption transactionScopeOption, SupplierInvoice supplierInvoice, int actorCompanyId, string failedToTransfer = null)
        {
            if (supplierInvoice == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, "SupplierInvoice");

            List<SupplierInvoice> supplierInvoices = new List<SupplierInvoice>();
            supplierInvoices.Add(supplierInvoice);

            return TryTransferSupplierInvoiceToVoucher(entities, transactionScopeOption, supplierInvoices, actorCompanyId, failedToTransfer);
        }

        public ActionResult TryTransferSupplierInvoiceToVoucher(CompEntities entities, TransactionScopeOption transactionScopeOption, List<SupplierInvoice> supplierInvoices, int actorCompanyId, string failedToTransfer = null)
        {
            ActionResult result = new ActionResult(true);

            bool transferToVoucher = SettingManager.GetBoolSetting(entities, SettingMainType.Company, (int)CompanySettingType.SupplierInvoiceTransferToVoucher, 0, actorCompanyId, 0);

            if (!transferToVoucher)
                return result;

            List<SupplierInvoice> supplierInvoicesToTransfer = new List<SupplierInvoice>();

            foreach (SupplierInvoice supplierInvoice in supplierInvoices)
            {
                // Make sure VoucherHead is loaded
                if (!supplierInvoice.IsAdded() && !supplierInvoice.VoucherHeadReference.IsLoaded)
                {
                    supplierInvoice.VoucherHeadReference.Load();
                }

                //Check if already transferred
                if (supplierInvoice.VoucherHead != null)
                    continue;

                //Check if valid to be transferred
                bool isValid = IsSupplierInvoiceValidForVoucher(supplierInvoice.Origin.Status);
                if (!isValid)
                    continue;

                LoadSupplierInvoiceRows(supplierInvoice, true);

                supplierInvoicesToTransfer.Add(supplierInvoice);
            }

            if (supplierInvoicesToTransfer.Count > 0)
                result = VoucherManager.SaveVoucherFromSupplierInvoices(entities, transactionScopeOption, supplierInvoicesToTransfer, actorCompanyId, failedToTransfer);

            return result;
        }

        public IQueryable<InvoiceText> GetInvoiceTexts(CompEntities entities, int actorCompanyId)
        {

            return (from t in entities.InvoiceText
                    where (
                        t.Invoice.Origin.ActorCompanyId == actorCompanyId || 
                        t.EdiEntry.ActorCompanyId == actorCompanyId
                    ) &&
                    t.State == (int)SoeEntityState.Active
                    select t);
        }

        public List<InvoiceText> GetSupplierInvoiceTextsByEdiEntry(CompEntities entities, int actorCompanyId, int ediEntryId)
        {
            return GetInvoiceTexts(entities, actorCompanyId)
                .Where(t => t.EdiEntryId == ediEntryId)
                .ToList()
                .OrderByDescending(t => t.Created)
                .ToList();
        }

        public InvoiceText GetSupplierInvoiceText(CompEntities entities, int actorCompanyId, int? invoiceId, int? ediEntryId, InvoiceTextType type)
        {
            var query = GetInvoiceTexts(entities, actorCompanyId);

            if (invoiceId.GetValueOrDefault() > 0)
                query = query.Where(t => t.InvoiceId == invoiceId);
            else if (ediEntryId.GetValueOrDefault() > 0)
                query = query.Where(t => t.EdiEntryId == ediEntryId);
            else
                return null;

            return query.Where(t => t.Type == (int)type && t.State == (int)SoeEntityState.Active)
                        .OrderByDescending(t => t.Created)
                        .FirstOrDefault();
        }

        public InvoiceText GetSupplierInvoiceText(int actorCompanyId, int? invoiceId, int? ediEntryId, InvoiceTextType type)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetSupplierInvoiceText(entities, actorCompanyId, invoiceId, ediEntryId, type);
        }

        public ActionResult SupplierInvoiceSaveInvoiceTextAction(int invoiceId, InvoiceTextType type, bool apply, string reason = null)
        {
            // Default result is successful
            ActionResult result = new ActionResult();

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    #region Prereq

                    // Get SupplierInvoice
                    SupplierInvoice invoice = GetSupplierInvoice(entities, invoiceId);
                    if (invoice == null)
                        return new ActionResult((int)ActionResultSave.EntityNotFound, "SupplierInvoice");

                    #endregion

                    #region SupplierInvoice Update

                    switch (type)
                    {
                        case InvoiceTextType.SupplierInvoiceBlockReason:
                            invoice.BlockPayment = apply;
                            break;
                        case InvoiceTextType.UnderInvestigationReason:
                            invoice.UnderInvestigation = apply;
                            break;
                        default:
                            return new ActionResult((int)ActionResultSave.NothingSaved, "Invalid InvoiceTextType");
                    }
                    SetModifiedProperties(invoice);
                    result = SaveChanges(entities);

                    #endregion

                    #region Reason

                    if (result.Success)
                        result = UpdateInvoiceText(entities, invoiceId, null, apply, type, reason);

                    if (result.Success)
                        result.Modified = invoice.Modified.HasValue ? invoice.Modified.Value : DateTime.Now;

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

        // No saving, edits in place.
        public void AddInvoiceTextActionsFromEdiEntry(CompEntities entities, EdiEntry ediEntry, SupplierInvoice invoice)
        {
            var texts = GetSupplierInvoiceTextsByEdiEntry(entities, ediEntry.ActorCompanyId, ediEntry.EdiEntryId);
            texts.ForEach(t => invoice.InvoiceText1.Add(t));

            // Bools
            invoice.UnderInvestigation = ediEntry.UnderInvestigation;
        }

        public ActionResult UpdateInvoiceText(CompEntities entities, int? invoiceId, int? ediEntryId, bool apply, InvoiceTextType type, string reason = null)
        {
            if ((!invoiceId.HasValue || invoiceId.Value == 0) && (!ediEntryId.HasValue || ediEntryId.Value == 0))
                return new ActionResult((int)ActionResultSave.NothingSaved, "InvoiceId and EdiEntryId cannot both be null");

            if (apply && reason != null)
            {
                var invoiceText = new InvoiceText()
                {
                    InvoiceId = invoiceId,
                    EdiEntryId = ediEntryId,
                    Text = reason,
                    Type = (int)type,
                };

                SetCreatedProperties(invoiceText);
                entities.InvoiceText.AddObject(invoiceText);
                return SaveChanges(entities);
            }
            else if (!apply)
            {
                InvoiceText invoiceText = GetSupplierInvoiceText(entities, this.ActorCompanyId,
                    invoiceId: invoiceId,
                    ediEntryId: ediEntryId,
                    type: type);

                if (invoiceText != null)
                {
                    ChangeEntityState(invoiceText, SoeEntityState.Deleted);
                    return SaveEntityItem(entities, invoiceText);
                }
            }
            return new ActionResult();
        }

        public ActionResult SaveSupplierInvoicesForUploadedImages(List<int> dataStorageIds, int actorCompanyId)
        {
            // Default result is successful
            ActionResult result = new ActionResult();

            var azoraOneStatus = SettingManager.GetCompanyIntSetting(CompanySettingType.ScanningUsesAzoraOne);
            if (azoraOneStatus > (int)AzoraOneStatus.ActivatedInBackground)
            {
                result = EdiManager.SendDocumentsForScanning(actorCompanyId, dataStorageIds, true);
                result.BooleanValue = true; // Set to true to indicate that the result is by using interpretation.
                return result;
            }


            using (CompEntities entities = new CompEntities())
            {

                entities.Connection.Open();

                CompCurrency currency = CountryCurrencyManager.GetCompanyBaseCurrency(entities, actorCompanyId);
                int voucherSeriesTypeId = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.SupplierInvoiceVoucherSeriesType, 0, actorCompanyId, 0);

                //Get AccountYear                    
                AccountYear accountYear = AccountManager.GetAccountYear(entities, DateTime.Now.Date, actorCompanyId);
                result = AccountManager.ValidateAccountYear(accountYear);
                if (!result.Success)
                {
                    return result;
                }

                //Get VoucherSeries
                VoucherSeries voucherSerie = VoucherManager.GetVoucherSerieByYear(entities, accountYear.AccountYearId, voucherSeriesTypeId);
                if (voucherSerie == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(8403, "Verifikatserie saknas"));

                using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                {
                    foreach (var dataStorageId in dataStorageIds)
                    {
                        //origin
                        Origin origin = new Origin()
                        {
                            Type = (int)SoeOriginType.SupplierInvoice,
                            Status = (int)SoeOriginStatus.Draft,

                            //Set FK
                            VoucherSeriesId = voucherSerie.VoucherSeriesId,
                            ActorCompanyId = actorCompanyId,

                        };
                        SetCreatedProperties(origin);
                        entities.Origin.AddObject(origin);

                        //invoice                       
                        SupplierInvoice invoice = new SupplierInvoice()
                        {
                            Type = (int)TermGroup_SupplierInvoiceType.Uploaded,
                            InvoiceNr = "0",
                            OnlyPayment = false,
                            BillingType = 0,
                            CurrencyId = currency.CurrencyId,
                            InvoiceDate = DateTime.Now.Date,
                            VoucherDate = DateTime.Now.Date,
                            CurrencyDate = DateTime.Now.Date,

                            //Set references
                            Origin = origin,
                        };
                        SetCreatedProperties(invoice);
                        entities.Invoice.AddObject(invoice);

                        result = SaveChanges(entities, transaction);

                        if (result.Success)
                        {


                            //update dataStorageRecord
                            DataStorage dataStorage = GeneralManager.GetDataStorage(entities, dataStorageId, actorCompanyId);
                            DataStorageRecord dataStorageRecord = dataStorage.DataStorageRecord.FirstOrDefault();

                            dataStorageRecord.RecordId = invoice.Origin.OriginId;
                            result = SaveChanges(entities, transaction);
                        }
                    }
                    transaction.Complete();
                }

                entities.Connection.Close();


                return result;
            }
        }

        public void SetSupplierOrderNr(CompEntities entities, SupplierInvoice supplierInvoice, string orderNr, int actorCompanyId, bool defaultInternalAccountsFromOrder, out int customerInvoiceId)
        {
            int orderNrInt;
            customerInvoiceId = 0;
            if (int.TryParse(orderNr, out orderNrInt))
            {
                var orderList = InvoiceManager.GetInvoicesSearchInvoiceNr(entities, actorCompanyId, orderNr, 2, SoeOriginType.Order, false);
                if (orderList.Count == 1)
                {
                    var order = orderList[0];
                    supplierInvoice.OrderNr = orderNrInt;
                    customerInvoiceId = order.InvoiceId;
                    //Try to define project from orderNr:

                    if (supplierInvoice.ProjectId == 0 || supplierInvoice.ProjectId == null)
                    {
                        supplierInvoice.ProjectId = order.ProjectId;
                    }

                    if (defaultInternalAccountsFromOrder)
                    {
                        if (order.DefaultDim2AccountId.HasValue)
                        {
                            supplierInvoice.DefaultDim2AccountId = order.DefaultDim2AccountId;
                        }
                        if (order.DefaultDim3AccountId.HasValue)
                        {
                            supplierInvoice.DefaultDim3AccountId = order.DefaultDim3AccountId;
                        }
                        if (order.DefaultDim4AccountId.HasValue)
                        {
                            supplierInvoice.DefaultDim4AccountId = order.DefaultDim4AccountId;
                        }
                        if (order.DefaultDim5AccountId.HasValue)
                        {
                            supplierInvoice.DefaultDim5AccountId = order.DefaultDim5AccountId;
                        }
                        if (order.DefaultDim6AccountId.HasValue)
                        {
                            supplierInvoice.DefaultDim6AccountId = order.DefaultDim6AccountId;
                        }
                    }
                }
            }
        }

        public List<SupplierInvoiceCostOverviewDTO> GetSupplierInvoiceCostOverView(bool notLinked, bool partiallyLinked, bool linked, TermGroup_ChangeStatusGridAllItemsSelection? allItemsSelection, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.SupplierInvoiceCostOverviewView.NoTracking();
            return GetSupplierInvoiceCostOverView(entities, notLinked, partiallyLinked, linked, allItemsSelection, actorCompanyId);
        }

        public List<SupplierInvoiceCostOverviewDTO> GetSupplierInvoiceCostOverView(CompEntities entities, bool notLinked, bool partiallyLinked, bool linked, TermGroup_ChangeStatusGridAllItemsSelection? allItemsSelection, int actorCompanyId)
        {
            if (!notLinked && !partiallyLinked && !linked)
                return new List<SupplierInvoiceCostOverviewDTO>();

            int langId = GetLangId();
            var originStatuses = base.GetTermGroupDict(TermGroup.OriginStatus, langId);

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

            var attestGroups = AttestManager.GetAttestWorkFlowGroups(actorCompanyId).ToList();

            var invoices = entities.GetSupplierInvoiceCostOverview(actorCompanyId).ToList();

            if (selectionDate != null)
                invoices = invoices.Where(i => i.SupplierInvoiceDate == null || i.SupplierInvoiceDate >= selectionDate).ToList();

            List<SupplierInvoiceCostOverviewDTO> dtos = new List<SupplierInvoiceCostOverviewDTO>();

            foreach (var invoice in invoices)
            {
                var dto = new SupplierInvoiceCostOverviewDTO()
                {
                    SupplierInvoiceId = invoice.SupplierInvoiceId,
                    SeqNr = invoice.SupplierInvoiceSeqNr.HasValue ? invoice.SupplierInvoiceSeqNr.Value.ToString() : "",
                    InvoiceNr = invoice.SupplierInvoiceNr,
                    InvoiceDate = invoice.SupplierInvoiceDate,
                    DueDate = invoice.SupplierInoiceDueDate,
                    Status = invoice.SupplierInvoiceStatus,
                    SupplierNr = invoice.SupplierNr,
                    SupplierName = invoice.SupplierName,
                    TotalAmountCurrency = invoice.TotalAmountCurrency,
                    TotalAmountExVat = invoice.TotalAmountCurrency - invoice.VATAmountCurrency,
                    VATAmountCurrency = invoice.VATAmountCurrency,
                    DiffAmount = (invoice.TotalAmountCurrency - invoice.VATAmountCurrency) - (invoice.OrderAmount.HasValue ? invoice.OrderAmount.Value : 0) - invoice.ProjectAmount - (invoice.SupplierInvoiceOrderAmount ?? 0),
                    OrderAmount = invoice.OrderAmount + (invoice.SupplierInvoiceOrderAmount ?? 0),
                    ProjectAmount = invoice.ProjectAmount,
                    InternalText = invoice.InternalText,
                };
                
                if (dto.DiffAmount != 0 && dto.TotalAmountExVat != 0)
                    dto.DiffPercent = (dto.DiffAmount / dto.TotalAmountExVat) * 100;
                else
                    dto.DiffPercent = 0;

                List<string> orderNbrs = !string.IsNullOrEmpty(invoice.OrderNrs) ? invoice.OrderNrs.Split(',').Where(s => s.Trim() != string.Empty).Distinct().ToList() : new List<string>();

                if (!string.IsNullOrEmpty(invoice.EdiOrderNrs))
                {
                    var ediNumbers = invoice.EdiOrderNrs.Split(',').Where(s => s.Trim() != string.Empty).Distinct().ToList();
                    if (ediNumbers.Any())
                    {
                        orderNbrs = orderNbrs.Union(ediNumbers).ToList();
                    }
                }

                if (!string.IsNullOrEmpty(invoice.SupplierInvoiceOrderNrs))
                {
                    var supplierInvoiceOrderNumbers = invoice.SupplierInvoiceOrderNrs.Split(',').Where(s => s.Trim() != string.Empty).Distinct().ToList();
                    if (supplierInvoiceOrderNumbers.Any())
                    {
                        orderNbrs = orderNbrs.Union(supplierInvoiceOrderNumbers).ToList();
                    }
                }

                dto.OrderNr = string.Join(", ", orderNbrs);

                List<string> projectNbrs = !string.IsNullOrEmpty(invoice.ProjectNrs) ? invoice.ProjectNrs.Split(',').Where(s => s.Trim() != string.Empty).Distinct().ToList() : new List<string>();

                if (!string.IsNullOrEmpty(invoice.SupplierInvoiceProjectNrs))
                {
                    var supplierInvoiceProjectNumbers = invoice.SupplierInvoiceProjectNrs.Split(',').Where(s => s.Trim() != string.Empty).Distinct().ToList();
                    if (supplierInvoiceProjectNumbers.Any())
                    {
                        projectNbrs = orderNbrs.Union(supplierInvoiceProjectNumbers).ToList();
                    }
                }

                dto.ProjectNr = string.Join(", ", projectNbrs);

                dto.StatusName = dto.Status != 0 ? originStatuses[dto.Status] : "";

                if (invoice.AttestGroupId.HasValue && invoice.AttestGroupId > 0)
                {
                    var attestGroup = attestGroups.FirstOrDefault(i => i.AttestWorkFlowHeadId == invoice.AttestGroupId.Value);
                    if (attestGroup != null)
                        dto.AttestGroupName = attestGroup.Name;
                }

                if (notLinked && dto.TotalAmountExVat != 0 && dto.TotalAmountExVat == dto.DiffAmount)
                    dtos.Add(dto);
                else if (partiallyLinked && (dto.TotalAmountExVat > 0 ? dto.DiffAmount > 0 : dto.DiffAmount < 0) && dto.TotalAmountExVat != dto.DiffAmount)
                    dtos.Add(dto);
                else if (linked && (dto.TotalAmountExVat == 0 || (dto.TotalAmountExVat > 0 ? dto.DiffAmount <= 0 : dto.DiffAmount >= 0)))
                    dtos.Add(dto);
            }

            return dtos;
        }

        public ActionResult TransferSupplierInvoicesToOrder(int actorCompanyId, List<GenericType<int, decimal>> items, bool transferSupplierInvoiceRows, bool useMiscProduct)
        {
            ActionResult result = new ActionResult();

            using (CompEntities entities = new CompEntities())
            {
                entities.Connection.Open();

                #region Prereq

                var company = CompanyManager.GetCompany(entities, actorCompanyId);

                var orderTemplateId = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.SupplierInvoiceBatchOnwardInvoicingOrderTemplate, 0, actorCompanyId, 0);
                if (orderTemplateId == 0)
                    return new ActionResult((int)ActionResultSave.NothingSaved, GetText(7666, "Inställning för ordermall för vidarefakturering saknas under inställningar leverantörsreskontra"));

                var orderTemplate = InvoiceManager.GetCustomerInvoice(entities, orderTemplateId, loadOrigin: true, loadActor: true, loadPaymentCondition: true, loadInvoiceRow: true, loadInvoiceAccountRow: true, loadIntrastatTransaction: true, loadProject: true);
                if (orderTemplate == null)
                    return new ActionResult(null, GetText(7667, "Order template could not be found"));

                var miscProduct = ProductManager.GetInvoiceProductFromSetting(entities, CompanySettingType.ProductMisc, actorCompanyId, false, true);
                if (miscProduct == null)
                    return new ActionResult(null, GetText(9144, "Ingen ströartikel hittad på företaget")); 

                var accountYearId = AccountManager.GetAccountYearId(entities, DateTime.Today, actorCompanyId);
                var voucherSeries = VoucherManager.GetDefaultVoucherSeries(entities, accountYearId, CompanySettingType.CustomerInvoiceVoucherSeriesType, actorCompanyId);
                if (voucherSeries == null)
                    return new ActionResult(null, GetText(8403, "Verifikatserie saknas"));

                // Cent rounding
                bool useCentRounding = SettingManager.GetBoolSetting(entities, SettingMainType.Company, (int)CompanySettingType.BillingUseCentRounding, 0, actorCompanyId, 0);

                // Project
                bool autoGenerateProject = SettingManager.GetBoolSetting(entities, SettingMainType.Company, (int)CompanySettingType.ProjectAutoGenerateOnNewInvoice, 0, actorCompanyId, 0);
                bool useCustomerNameAsProjectName = SettingManager.GetBoolSetting(entities, SettingMainType.Company, (int)CompanySettingType.ProjectUseCustomerNameAsProjectName, 0, actorCompanyId, 0, true); 
                bool includeTimeReport = SettingManager.GetBoolSetting(entities, SettingMainType.Company, (int)CompanySettingType.ProjectIncludeTimeProjectReport, 0, actorCompanyId, 0);

                // Include image
                bool includeSupplierInvoiceImage = SettingManager.GetBoolSetting(entities, SettingMainType.Company, (int)CompanySettingType.SupplierInvoiceBatchOnwardInvoicingAttachImage, 0, actorCompanyId, 0);

                // Supplier invoice items
                //var invoices = entities.GetSupplierInvoiceCostOverview(actorCompanyId).ToList();

                #endregion

                using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                {
                    foreach(var item in items)
                    {
                        var supplierInvoice = transferSupplierInvoiceRows ? entities.Invoice.OfType<SupplierInvoice>().Include("SupplierInvoiceProductRow").FirstOrDefault(i => i.InvoiceId == item.Field1 && i.Origin.ActorCompanyId == actorCompanyId) : entities.Invoice.OfType<SupplierInvoice>().FirstOrDefault(i => i.InvoiceId == item.Field1 && i.Origin.ActorCompanyId == actorCompanyId);
                        if (supplierInvoice == null)
                            return new ActionResult((int)ActionResultSave.EntityNotFound);

                        var amount = item.Field2;
                        if (amount == 0)
                            continue;

                        #region Create order

                        var clone = InvoiceManager.CopyCustomerInvoice(entities, orderTemplate, SoeOriginType.Order, false);
                        clone.PrintTimeReport = orderTemplate.PrintTimeReport;
                        clone.IncludeOnlyInvoicedTime = orderTemplate.IncludeOnlyInvoicedTime;

                        #region Reset values

                        clone.InvoiceNr = "";
                        clone.SeqNr = null;
                        clone.PaidAmount = 0;
                        clone.PaidAmountCurrency = 0;
                        clone.FullyPayed = false;
                        clone.BillingInvoicePrinted = false;
                        clone.RemainingAmount = 0;
                        clone.OCR = "";
                        clone.IsTemplate = false;

                        // Order planning
                        clone.ShiftTypeId = null;
                        clone.PlannedStartDate = null;
                        clone.PlannedStopDate = null;
                        clone.EstimatedTime = 0;
                        clone.RemainingTime = 0;
                        clone.KeepAsPlanned = false;
                        clone.Priority = null;

                        #endregion

                        #region Set new values

                        //Status
                        clone.Origin.Status = (int)SoeOriginStatus.Origin;
                        clone.InvoiceDate = DateTime.Today;
                        clone.OrderDate = DateTime.Today;
                        clone.VoucherDate = DateTime.Today;

                        if (orderTemplate.PaymentCondition != null)
                            clone.DueDate = clone.InvoiceDate.Value.AddDays(orderTemplate.PaymentCondition.Days);

                        #endregion

                        #region Sequence numbers

                        // Get next sequence number
                        int seqNbr = InvoiceManager.GetNextSequenceNumber(entities, SoeOriginType.Order, (SoeOriginStatus)clone.Origin.Status, (TermGroup_BillingType)clone.BillingType, actorCompanyId, false);

                        clone.SeqNr = seqNbr;

                        // Set invoice number to same as sequence number (padded to a specified number of digits)
                        var invoiceNumberLength = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.BillingInvoiceNumberLength, 0, actorCompanyId, 0);
                        clone.InvoiceNr = seqNbr.ToString().PadLeft(invoiceNumberLength, '0');

                        if (company != null && company.SysCountryId != null && company.SysCountryId == (int)TermGroup_Country.FI)
                        {
                            string tmpReference = orderTemplate.Actor.Customer.CustomerNr.ToString() + "0000" + clone.InvoiceNr.ToString();
                            if (orderTemplate.InvoiceNr != null && orderTemplate.Actor.Customer.CustomerNr != null)
                            {
                                if (SettingManager.GetBoolSetting(entities, SettingMainType.Company, (int)CompanySettingType.BillingFormFIReferenceNumberToOCR, 0, actorCompanyId, 0))
                                {
                                    clone.OCR = InvoiceUtility.GetCheckedBankPaymentReference(tmpReference);  // Normal Finnish Bank Reference 
                                }
                                else
                                {
                                    clone.OCR = InvoiceUtility.GetISO11649(InvoiceUtility.GetCheckedBankPaymentReference(tmpReference));
                                }
                            }
                            else
                            {
                                string errorCheck = "";
                                if (orderTemplate.InvoiceNr == null) errorCheck += "A=null";
                                if (orderTemplate.Actor.Customer.CustomerNr == null) errorCheck += "B=null";
                                clone.OCR = "Virhe(1):" + errorCheck;
                            }
                        }
                        else if (company != null && company.SysCountryId != null && company.SysCountryId == (int)TermGroup_Country.NO)
                        {
                            clone.OCR = InvoiceUtility.GetNorwegianKIDNumber(clone.InvoiceNr.ToString(), orderTemplate.Actor.Customer.CustomerNr.ToString(), clone.InvoiceDate.HasValue ? clone.InvoiceDate.Value : (DateTime)orderTemplate.InvoiceDate, 0);
                        }
                        else
                        {
                            //Sweden and the rest
                            //Since we don't have invoiceid on the not yet created invoice, we use the id from the order. Will still be unique.
                            if (SettingManager.GetBoolSetting(entities, SettingMainType.Company, (int)CompanySettingType.BillingFormReferenceNumberToOCR, 0, actorCompanyId, 0))
                            {
                                clone.OCR = InvoiceUtility.GetSwedishOCRNumber(orderTemplate.InvoiceId.ToString() + clone.SeqNr);
                            }
                            else
                            {
                                clone.OCR = string.Empty;
                            }
                        }

                        #endregion

                        #region OriginUser

                        foreach (var prototypeOriginUser in orderTemplate.Origin.OriginUser.Where(o => o.State == (int)SoeEntityState.Active))
                        {
                            if (!prototypeOriginUser.UserReference.IsLoaded)
                                prototypeOriginUser.UserReference.Load();
                            if (!prototypeOriginUser.RoleReference.IsLoaded)
                                prototypeOriginUser.RoleReference.Load();

                            OriginUser cloneOriginUser = new OriginUser
                            {
                                Origin = clone.Origin,
                                User = prototypeOriginUser.User,
                                Role = prototypeOriginUser.Role,
                                Main = prototypeOriginUser.Main,
                            };
                            SetCreatedProperties(cloneOriginUser);
                            clone.Origin.OriginUser.Add(cloneOriginUser);
                        }

                        #endregion

                        #endregion

                        #region Project

                        if (orderTemplate.Project == null)
                        {
                            if (autoGenerateProject)
                            {
                                var numbers = ProjectManager.GetAllProjectNumbers(entities, actorCompanyId);
                                var projectNr = ProjectManager.GetProjectNumberCheckExisting(numbers, clone.InvoiceNr, actorCompanyId);

                                TimeProject project = new TimeProject()
                                {
                                    Number = projectNr,
                                    Name = !useCustomerNameAsProjectName || String.IsNullOrEmpty(orderTemplate.ActorName) ? DateTime.Now.ToString("yyyyMMddhhmm") : orderTemplate.ActorName.Replace("\n", "").Replace("\r", ""),
                                    Type = (int)TermGroup_ProjectType.TimeProject,
                                    InvoiceProductAccountingPrio = "0,0,0,0,0",
                                    PayrollProductAccountingPrio = "0,0,0,0,0",
                                    Description = string.Empty,
                                    Status = (int)TermGroup_ProjectStatus.Hidden, //Auto created projects gets status Hidden
                                    UseAccounting = true,
                                    AllocationType = (int)TermGroup_ProjectAllocationType.External,
                                    //Set FK
                                    ActorCompanyId = actorCompanyId,
                                    CustomerId = clone.ActorId,
                                };

                                SetCreatedProperties(project);

                                clone.Project = project;

                                clone.PrintTimeReport = true;
                            }
                        }
                        else if (orderTemplate.ProjectId > 0 && !orderTemplate.IsTemplate)
                        {
                            //Bind to invoice
                            clone.Project = orderTemplate.Project;
                        }

                        #endregion

                        #region Handle template rows

                        int accountRowNr = 0;
                        foreach (var row in orderTemplate.ActiveCustomerInvoiceRows)
                        {
                            var cloneRow = InvoiceManager.CopyCustomerInvoiceRow(entities, row);

                            if (cloneRow.IntrastatTransaction != null)
                                cloneRow.IntrastatTransaction.Origin = clone.Origin;

                            foreach (CustomerInvoiceAccountRow prototypeAccountRow in row.ActiveCustomerInvoiceAccountRows)
                            {
                                #region CustomerInvoiceAccountRow

                                //Clone
                                var cloneAccountRow = InvoiceManager.CopyCustomerInvoiceAccountRow(prototypeAccountRow);

                                //Set new values
                                cloneAccountRow.RowNr = ++accountRowNr;

                                cloneRow.CustomerInvoiceAccountRow.Add(cloneAccountRow);

                                #endregion
                            }

                            clone.CustomerInvoiceRow.Add(cloneRow);
                        }

                        #endregion

                        #region Final calculation and add clone

                        #region Calculate totals

                        InvoiceManager.CalculateAmountsByCurrency(entities, clone, useCentRounding, actorCompanyId);

                        decimal sum = 0;
                        foreach (CustomerInvoiceRow row in clone.CustomerInvoiceRow.Where(r => r.Type == (int)SoeInvoiceRowType.ProductRow || r.Type == (int)SoeInvoiceRowType.SubTotalRow).OrderBy(r => r.RowNr))
                        {
                            if (row.Type == (int)SoeInvoiceRowType.SubTotalRow)
                            {
                                row.SumAmountCurrency = sum;
                                sum = 0;
                            }
                            else
                            {
                                sum += row.SumAmountCurrency;
                            }
                        }

                        #endregion

                        #region Add clone

                        entities.Invoice.AddObject(clone);

                        #endregion

                        #region CustomerInvoiceRow - Claim row

                        CustomerInvoiceRow productClaimRow = clone.CustomerInvoiceRow.FirstOrDefault(r => r.Type == (int)SoeInvoiceRowType.AccountingRow && r.Product == null && !r.IsCentRoundingRow);
                        if (productClaimRow == null)
                        {
                            #region Add

                            #region CustomerInvoiceRow

                            productClaimRow = new CustomerInvoiceRow()
                            {
                                Type = (int)SoeInvoiceRowType.AccountingRow,
                                DiscountType = (int)SoeInvoiceRowDiscountType.Percent,
                                Amount = clone.TotalAmount,
                                AmountCurrency = clone.TotalAmountCurrency,
                                SumAmount = clone.TotalAmount,
                                SumAmountCurrency = clone.TotalAmountCurrency,
                            };
                            SetCreatedProperties(productClaimRow);
                            clone.CustomerInvoiceRow.Add(productClaimRow);

                            #endregion

                            #region CustomerInvoiceAccountRow

                            CustomerInvoiceAccountRow productClaimAccountRow = null;
                            AccountingPrioDTO prioDTO = AccountManager.GetInvoiceProductAccount(entities, actorCompanyId, 0, 0, orderTemplate.ActorId.Value, 0, ProductAccountType.Purchase, (TermGroup_InvoiceVatType)clone.VatType, false);
                            if (prioDTO != null && prioDTO.AccountId.HasValue)
                            {
                                var isCredit = (clone.TotalAmount < 0);

                                productClaimAccountRow = new CustomerInvoiceAccountRow()
                                {
                                    RowNr = 1,
                                    AccountId = prioDTO.AccountId.Value,
                                    Amount = clone.TotalAmount,
                                    AmountCurrency = clone.TotalAmountCurrency,
                                    DebitRow = !isCredit,
                                    CreditRow = isCredit,
                                };
                                productClaimRow.CustomerInvoiceAccountRow.Add(productClaimAccountRow);
                            }

                            #endregion

                            #region Cent rounding row

                            if (useCentRounding)
                            {
                                decimal centAmount = clone.CentRounding;

                                // Get cent rounding product row
                                CustomerInvoiceRow productCentRow = clone.CustomerInvoiceRow.FirstOrDefault(r => r.IsCentRoundingRow && r.State == (int)SoeEntityState.Active);
                                if (productCentRow != null && productCentRow.Product != null)
                                {
                                    // Add a cent rounding account row
                                    prioDTO = AccountManager.GetInvoiceProductAccount(entities, actorCompanyId, productCentRow.Product.ProductId, 0, 0, 0, ProductAccountType.Sales, (TermGroup_InvoiceVatType)clone.VatType, false);
                                    if (prioDTO != null && prioDTO.AccountId.HasValue)
                                    {
                                        //get currency amount for cent rounding row
                                        decimal centAmountCurrency = productCentRow.AmountCurrency;

                                        // A positive cent amount from the product row will be a credit accounting row
                                        bool isCreditRow = centAmount > 0;
                                        centAmount = Math.Abs(centAmount);
                                        centAmountCurrency = Math.Abs(centAmountCurrency);
                                        if (isCreditRow)
                                        {
                                            centAmount = -centAmount;
                                            centAmountCurrency = -centAmountCurrency;
                                        }

                                        accountRowNr++;

                                        CustomerInvoiceAccountRow centRow = new CustomerInvoiceAccountRow()
                                        {
                                            RowNr = accountRowNr,
                                            AccountId = prioDTO.AccountId.Value,
                                            Quantity = 1,
                                            Amount = centAmount,
                                            AmountCurrency = centAmountCurrency,
                                            DebitRow = !isCreditRow,
                                            CreditRow = isCreditRow
                                        };

                                        productCentRow.CustomerInvoiceAccountRow.Add(centRow);
                                    }
                                }
                            }

                            // Update amount on claim row after calculation
                            if (productClaimAccountRow != null)
                            {
                                productClaimAccountRow.Amount = clone.TotalAmount;
                                productClaimAccountRow.AmountCurrency = clone.TotalAmountCurrency;
                            }

                            #endregion

                            #endregion
                        }
                        else
                        {
                            #region Update

                            #region CustomerInvoiceRow

                            productClaimRow.Amount = clone.TotalAmount;
                            productClaimRow.SumAmount = clone.TotalAmount;
                            productClaimRow.SumAmountCurrency = clone.TotalAmountCurrency;
                            SetModifiedProperties(productClaimRow);

                            #endregion

                            #region CustomerInvoiceAccountRow

                            CustomerInvoiceAccountRow productClaimAccountRow = productClaimRow.CustomerInvoiceAccountRow.FirstOrDefault(r => r.RowNr == 1);
                            AccountingPrioDTO prioDTO = AccountManager.GetInvoiceProductAccount(entities, actorCompanyId, 0, 0, orderTemplate.ActorId.Value, 0, ProductAccountType.Purchase, (TermGroup_InvoiceVatType)clone.VatType, false);
                            if (prioDTO != null && prioDTO.AccountId.HasValue && productClaimAccountRow == null)
                            {
                                var isCredit = (clone.TotalAmount < 0);

                                productClaimAccountRow = new CustomerInvoiceAccountRow
                                {
                                    RowNr = 1,
                                    AccountId = prioDTO.AccountId.Value,
                                    Amount = clone.TotalAmount,
                                    AmountCurrency = clone.TotalAmountCurrency,
                                    DebitRow = !isCredit,
                                    CreditRow = isCredit,
                                };
                                productClaimRow.CustomerInvoiceAccountRow.Add(productClaimAccountRow);
                            }

                            #endregion

                            #region Cent rounding row

                            if (useCentRounding)
                            {
                                decimal centAmount = clone.CentRounding;

                                // Get cent rounding product row
                                CustomerInvoiceRow productCentRow = clone.CustomerInvoiceRow.FirstOrDefault(r => r.IsCentRoundingRow && r.State == (int)SoeEntityState.Active);
                                if (productCentRow != null && productCentRow.ProductId.HasValue)
                                {
                                    // Add a cent rounding account row
                                    prioDTO = AccountManager.GetInvoiceProductAccount(entities, actorCompanyId, productCentRow.ProductId.Value, 0, 0, 0, ProductAccountType.Sales, (TermGroup_InvoiceVatType)clone.VatType, false);
                                    if (prioDTO.AccountId.HasValue)
                                    {
                                        // A positive cent amount from the product row will be a credit accounting row
                                        bool isCreditRow = centAmount > 0;
                                        centAmount = Math.Abs(centAmount);
                                        if (isCreditRow)
                                            centAmount = -centAmount;

                                        accountRowNr++;

                                        CustomerInvoiceAccountRow centRow = new CustomerInvoiceAccountRow()
                                        {
                                            RowNr = accountRowNr,
                                            AccountId = prioDTO.AccountId.Value,
                                            Quantity = 1,
                                            Amount = centAmount,
                                            DebitRow = !isCreditRow,
                                            CreditRow = isCreditRow
                                        };

                                        productCentRow.CustomerInvoiceAccountRow.Add(centRow);
                                    }
                                }
                            }

                            // Update amount on claim row after calculation
                            if (productClaimAccountRow != null)
                            {
                                productClaimAccountRow.Amount = clone.TotalAmount;
                                productClaimAccountRow.AmountCurrency = clone.TotalAmountCurrency;
                            }

                            #endregion

                            #endregion
                        }

                        #endregion

                        #endregion

                        //Save changes
                        result = SaveChanges(entities, transaction);

                        if (!result.Success)
                            return result;

                        if (transferSupplierInvoiceRows && supplierInvoice.SupplierInvoiceProductRow.Any())
                        {
                            var rows = supplierInvoice.SupplierInvoiceProductRow.Where(r => r.CustomerInvoiceRowId == null).ToList();

                            if (rows.Count > 0)
                            {
                                result = EdiManager.TransferSupplierInvoiceRowsToOrder(entities, null, actorCompanyId, supplierInvoice, rows, clone, clone.SysWholeSellerId ?? 0, true,useMiscProduct);
                            }
                        }
                        else
                        {
                            #region Create onward invocing row

                            var orderRow = new SupplierInvoiceOrderRowDTO()
                            {
                                InvoiceProductId = miscProduct.ProductId,
                                SupplierInvoiceId = supplierInvoice.InvoiceId,
                                SumAmountCurrency = amount,
                                CustomerInvoiceId = clone.InvoiceId,
                                IncludeSupplierInvoiceImage = includeSupplierInvoiceImage,
                            };

                            if (clone.ProjectId.HasValue)
                                orderRow.ProjectId = clone.ProjectId.Value;

                            string description = " - " + GetText(7712, "från leverantörsfaktura") + " " + supplierInvoice.InvoiceNr + " " + (supplierInvoice.ActorName ?? "");
                            result = InvoiceManager.SaveOrderRowFromSupplierInvoice(transaction, entities, null, new List<SupplierInvoiceOrderRowDTO>() { orderRow }, actorCompanyId, base.RoleId, description, false);
                            if (!result.Success)
                                return result;

                            #endregion
                        }
                    }

                    //Complete transaction
                    transaction.Complete();
                }

                entities.Connection.Close();


                return result;
            }
        }

        #endregion

        #region SupplierInvoiceRow

        private void LoadSupplierInvoiceRows(SupplierInvoice invoice, bool loadInvoiceAccountRow)
        {
            if (!invoice.SupplierInvoiceRow.IsLoaded)
                invoice.SupplierInvoiceRow.Load();

            if (loadInvoiceAccountRow)
            {
                foreach (SupplierInvoiceRow invoiceRow in invoice.ActiveSupplierInvoiceRows)
                {
                    #region InvoiceAccountRow

                    // SupplerInvoiceAccountRow
                    if (!invoiceRow.SupplierInvoiceAccountRow.IsLoaded)
                        invoiceRow.SupplierInvoiceAccountRow.Load();

                    foreach (SupplierInvoiceAccountRow invoiceAccountRow in invoiceRow.SupplierInvoiceAccountRow.Where(r => r.State == (int)SoeEntityState.Active))
                    {
                        // AccountStd
                        if (!invoiceAccountRow.AccountStdReference.IsLoaded)
                            invoiceAccountRow.AccountStdReference.Load();
                        if (invoiceAccountRow.AccountStd != null)
                        {
                            if (!invoiceAccountRow.AccountStd.AccountReference.IsLoaded)
                                invoiceAccountRow.AccountStd.AccountReference.Load();
                            if (!invoiceAccountRow.AccountStd.AccountBalance.IsLoaded)
                                invoiceAccountRow.AccountStd.AccountBalance.Load();
                        }
                        // AccountInternal
                        if (!invoiceAccountRow.AccountInternal.IsLoaded)
                            invoiceAccountRow.AccountInternal.Load();

                        foreach (AccountInternal accountInternal in invoiceAccountRow.AccountInternal)
                        {
                            if (!accountInternal.AccountReference.IsLoaded)
                                accountInternal.AccountReference.Load();
                            if (!accountInternal.Account.AccountDimReference.IsLoaded)
                                accountInternal.Account.AccountDimReference.Load();
                        }

                        // Attest user
                        if (!invoiceAccountRow.UserReference.IsLoaded)
                            invoiceAccountRow.UserReference.Load();
                    }

                    #endregion
                }
            }
        }

        private List<SupplierInvoiceRow> ConvertToSupplierInvoiceRows(CompEntities entities, List<AccountingRowDTO> accountingRowDTOs, List<AccountInternal> accountInternals, int actorCompanyId)
        {
            List<SupplierInvoiceRow> rows = new List<SupplierInvoiceRow>();

            foreach (AccountingRowDTO item in accountingRowDTOs)
            {
                // Do not include empty rows
                if (item.Dim1Id == 0 && item.Amount == 0 && item.AmountCurrency == 0 && item.AmountEntCurrency == 0 && item.AmountLedgerCurrency == 0)
                    continue;

                SupplierInvoiceRow row = new SupplierInvoiceRow()
                {
                    SupplierInvoiceRowId = item.InvoiceRowId,
                    Amount = item.Amount,
                    AmountCurrency = item.AmountCurrency,
                    AmountEntCurrency = item.AmountEntCurrency,
                    AmountLedgerCurrency = item.AmountLedgerCurrency,
                    Quantity = item.Quantity,
                    State = (int)item.State,
                };
                SetCreatedProperties(row);

                row.SupplierInvoiceAccountRow.Add(ConvertToSupplierInvoiceAccountRow(entities, item, actorCompanyId, accountInternals));
                rows.Add(row);
            }

            return rows;
        }

        public SupplierInvoiceRow ConvertToSupplierInvoiceRow(CompEntities entities, SupplierInvoiceAccountingRowIO accountingRowIO, int actorCompanyId, List<AccountInternal> accountInternals, int rowNr, out ActionResult result)
        {
            SupplierInvoiceRow row = new SupplierInvoiceRow()
            {
                SupplierInvoiceRowId = 0,
                Amount = accountingRowIO.Amount.HasValue ? accountingRowIO.Amount.Value : 0,
                AmountCurrency = accountingRowIO.AmountCurrency.HasValue ? accountingRowIO.AmountCurrency.Value : 0,
                AmountEntCurrency = 0,
                AmountLedgerCurrency = 0,
                Quantity = accountingRowIO.Quantity,
                State = (int)SoeEntityState.Active,
            };
            SetCreatedProperties(row);

            var ar = ConvertToSupplierInvoiceAccountRow(entities, accountingRowIO, actorCompanyId, accountInternals, rowNr, out result);
            if (ar == null || !result.Success)
                return null;

            row.SupplierInvoiceAccountRow.Add(ar);

            return row;
        }

        public SupplierInvoiceRow GetSupplierInvoiceRow(CompEntities entities, int supplierInvoiceRowId)
        {
            return (from r in entities.SupplierInvoiceRow
                    where r.SupplierInvoiceRowId == supplierInvoiceRowId
                    select r).FirstOrDefault();
        }

        public bool IsSupplierInvoiceDebtRow(int billingType, bool isDebitRow, bool isCreditRow)
        {
            bool isDebtRow = false;

            if (billingType == (int)TermGroup_BillingType.Credit)
                isDebtRow = isDebitRow;
            else
                isDebtRow = isCreditRow;

            return isDebtRow;
        }

        public bool IsSupplierInvoiceDebtAccount(CompEntities entities, int accountId)
        {
            return AccountManager.GetAccountType(entities, accountId) == (int)TermGroup_AccountType.Debt;
        }

        public ActionResult AddSupplierInvoiceRows(CompEntities entities, SupplierInvoice supplierInvoice, List<SupplierInvoiceProductRowDTO> productRows, int actorCompanyId)
        {
            ActionResult result = new ActionResult(true);

            if (supplierInvoice == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "SupplierInvoice");

            #region Accounts

            bool useInternalAccountsWithBalanceSheetAccounts = SettingManager.GetBoolSetting(entities, SettingMainType.Company, (int)CompanySettingType.UseInternalAccountsWithBalanceSheetAccounts, 0, actorCompanyId, 0);

            //Internal accounts
            var supplierDimIds = new List<int>() {
                                                   supplierInvoice.DefaultDim2AccountId.GetValueOrDefault(),
                                                   supplierInvoice.DefaultDim3AccountId.GetValueOrDefault(),
                                                   supplierInvoice.DefaultDim4AccountId.GetValueOrDefault(),
                                                   supplierInvoice.DefaultDim5AccountId.GetValueOrDefault(),
                                                   supplierInvoice.DefaultDim6AccountId.GetValueOrDefault()
                                                }.Where(i => i > 0).ToList();

            List<AccountInternal> supplierAccountInternals = new List<AccountInternal>();
            if (supplierDimIds.Any())
            {
                List<AccountInternal> accountInternals = AccountManager.GetAccountInternals(entities, actorCompanyId, true);
                supplierAccountInternals.AddRange(accountInternals.Where(d => supplierDimIds.Contains(d.AccountId)).ToList());
            }


            //Supplier Accounts
            Dictionary<string, SupplierAccountStd> supplierAccountsDict;
            if (supplierInvoice.ActorId.HasValue)
                supplierAccountsDict = SupplierManager.GetSupplierAccountStdDict(entities, supplierInvoice.ActorId.Value);
            else if (supplierInvoice.Actor != null)
                supplierAccountsDict = SupplierManager.GetSupplierAccountStdDict(entities, supplierInvoice.Actor.ActorId);
            else
                supplierAccountsDict = new Dictionary<string, SupplierAccountStd>();

            #endregion

            #region Rows

            int rowNr = 1;

            //Credit row
            int creditAccountId = 0;
            SupplierAccountStd supplierAccountCredit = null;
            if (supplierAccountsDict.TryGetValue("credit", out supplierAccountCredit))
                creditAccountId = supplierAccountsDict["credit"].AccountStd.AccountId;
            else
                creditAccountId = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.AccountSupplierDebt, 0, actorCompanyId, 0);

            if (creditAccountId == 0)
            {
                return new ActionResult(7532, GetText(7532, "Inställning för standardkonto leverantörsskuld saknas"));
            }
            decimal creditAmount = supplierInvoice.TotalAmount;
            supplierInvoice.SupplierInvoiceRow.Add(CreateSupplierInvoiceRow(entities, actorCompanyId, creditAccountId, creditAmount, supplierAccountCredit != null && supplierAccountCredit.AccountInternal.Any() ? supplierAccountCredit.AccountInternal.ToList() : supplierAccountInternals, useInternalAccountsWithBalanceSheetAccounts, supplierInvoice.IsCredit, false, false, false, false, ref rowNr));

            //Vat row
            int interimAccountId = 0;
            if (supplierAccountsDict.ContainsKey("interim"))
                interimAccountId = supplierAccountsDict["interim"].AccountStd.AccountId;
            else
                interimAccountId = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.AccountSupplierInterim, 0, actorCompanyId, 0);

            int vatAccountId = 0;
            if (supplierAccountsDict.ContainsKey("vat"))
                vatAccountId = supplierAccountsDict["vat"].AccountStd.AccountId;
            else
                vatAccountId = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.AccountCommonVatReceivable, 0, actorCompanyId, 0);

            decimal vatAmount = supplierInvoice.IsCredit ? (supplierInvoice.VATAmount > 0 ? -supplierInvoice.VATAmount : supplierInvoice.VATAmount) : supplierInvoice.VATAmount;
            if (supplierInvoice.VatType == (int)TermGroup_InvoiceVatType.Contractor)
            {
                // Check vat
                if(vatAmount == 0 && supplierInvoice.ActorId.HasValue)
                {
                    decimal vatRate = CompanyManager.GetDefaultVatRate(entities, actorCompanyId);                    
                    Supplier supplier = SupplierManager.GetSupplier(entities, supplierInvoice.ActorId.Value);
                    if (supplier != null && supplier.VatCodeId.HasValue)
                    {
                        VatCode vatCode = AccountManager.GetVatCode(entities, supplier.VatCodeId.Value);
                        if (vatCode != null && vatCode.Percent > 0)
                            vatRate = vatCode.Percent / 100;
                    }

                    vatAmount = (supplierInvoice.TotalAmount * vatRate);
                }

                //Contractor
                int contractorVatAccountCreditId = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.AccountCommonVatPayable1Reversed, 0, actorCompanyId, 0);
                int contractorVatAccountDebitId = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.AccountCommonVatReceivableReversed, 0, actorCompanyId, 0);

                supplierInvoice.SupplierInvoiceRow.Add(CreateSupplierInvoiceRow(entities, actorCompanyId, contractorVatAccountCreditId, vatAmount, supplierAccountInternals, useInternalAccountsWithBalanceSheetAccounts, supplierInvoice.IsCredit, false, false, true, supplierInvoice.InterimInvoice, ref rowNr));
                supplierInvoice.SupplierInvoiceRow.Add(CreateSupplierInvoiceRow(entities, actorCompanyId, contractorVatAccountDebitId, vatAmount, supplierAccountInternals, useInternalAccountsWithBalanceSheetAccounts, supplierInvoice.IsCredit, true, false, true, supplierInvoice.InterimInvoice, ref rowNr));

                // 'Contractor' invoices does not have any regular VAT
                vatAmount = 0;
            }
            else if (supplierInvoice.VatType == (int)TermGroup_InvoiceVatType.NoVat)
            {
                // 'No VAT' invoices does not have any VAT at all
                vatAmount = 0;
            }
            else
            {
                // VAT row
                supplierInvoice.SupplierInvoiceRow.Add(CreateSupplierInvoiceRow(entities, actorCompanyId, supplierInvoice.InterimInvoice ? interimAccountId : vatAccountId, vatAmount, supplierAccountInternals, useInternalAccountsWithBalanceSheetAccounts, supplierInvoice.IsCredit, true, true, false, false, ref rowNr));
            }

            bool addCodingRowsBasedOnProductRows = SettingManager.GetBoolSetting(entities, SettingMainType.Company, (int)CompanySettingType.SupplierInvoiceDetailedCodingRowsBasedOnProductRows, 0, actorCompanyId, 0);
            int debitAccountId = 0;
            if (supplierAccountsDict.TryGetValue("debit", out SupplierAccountStd supplierAccountDebet))
                debitAccountId = supplierAccountDebet.AccountStd.AccountId;
            else
                debitAccountId = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.AccountSupplierPurchase, 0, actorCompanyId, 0);

            if (debitAccountId == 0)
            {
                return new ActionResult(5738, GetText(5738, "Inställning för standardkonto levreskontra inköp saknas"));
            }

            if (productRows != null && addCodingRowsBasedOnProductRows)
            {
                //CodingRows Based On ProductRows
                foreach (SupplierInvoiceProductRowDTO productRow in productRows)
                {
                    if (productRow.RowType != SupplierInvoiceRowType.ProductRow)
                        continue;

                    string text = productRow.SellerProductNumber + " " + productRow.Text;
                    decimal productRowAmount = CountryCurrencyManager.GetBaseAmountFromCurrencyAmount(productRow.AmountCurrency, supplierInvoice.CurrencyRate);
                    bool isCreditRow = productRowAmount < 0;

                    if (isCreditRow)
                        productRowAmount = -productRowAmount;

                    supplierInvoice.SupplierInvoiceRow.Add(CreateSupplierInvoiceRow(entities, actorCompanyId, debitAccountId, productRowAmount, supplierAccountDebet != null && supplierAccountDebet.AccountInternal.Any() ? supplierAccountDebet.AccountInternal.ToList() : supplierAccountInternals, useInternalAccountsWithBalanceSheetAccounts, supplierInvoice.IsCredit, !isCreditRow, false, false, supplierInvoice.InterimInvoice, ref rowNr, text));
                }
            } 
            else 
            {
                //Debit row
                decimal debitAmount = creditAmount - vatAmount;
                supplierInvoice.SupplierInvoiceRow.Add(CreateSupplierInvoiceRow(entities, actorCompanyId, debitAccountId, debitAmount, supplierAccountDebet != null && supplierAccountDebet.AccountInternal.Any() ? supplierAccountDebet.AccountInternal.ToList() : supplierAccountInternals, useInternalAccountsWithBalanceSheetAccounts, supplierInvoice.IsCredit, true, false, false, supplierInvoice.InterimInvoice, ref rowNr));
            }

            #endregion

            return result;
        }

        public SupplierInvoiceRow CreateSupplierInvoiceRow(CompEntities entities, int actorCompanyId, int accountId, decimal amount, List<AccountInternal> supplierInvoiceRowInternals, bool useInternalAccountsWithBalanceSheetAccounts, bool isCreditInvoice, bool isDebitRow, bool isVatRow, bool isContractorVatRow, bool isInterimRow, ref int rowNr, string text = "")
        {
            decimal setAmount = isDebitRow ? amount : -amount;

            var supplierInvoiceRow = new SupplierInvoiceRow
            {
                Amount = setAmount,
                Quantity = null,
            };
            SetCreatedProperties(supplierInvoiceRow);
            entities.SupplierInvoiceRow.AddObject(supplierInvoiceRow);

            // Credit invoice, negate isDebitRow
            if (isCreditInvoice)
                isDebitRow = !isDebitRow;

            var supplierInvoiceAccountRow = new SupplierInvoiceAccountRow
            {
                RowNr = rowNr,
                SplitType = (int)SoeInvoiceRowDiscountType.Unknown,
                SplitPercent = 0,
                Amount = setAmount,
                Quantity = null,
                Text = text,
                InterimRow = isInterimRow,
                VatRow = isVatRow,
                ContractorVatRow = isContractorVatRow,
                CreditRow = !isDebitRow,
                DebitRow = isDebitRow,

                //Set FK
                AccountId = accountId,
            };

            #region internal accounts (Dim2-6)

            if (useInternalAccountsWithBalanceSheetAccounts || (!isVatRow && !isContractorVatRow && ((!isCreditInvoice && isDebitRow) || (isCreditInvoice && !isDebitRow))))
            {
                if (supplierInvoiceRowInternals.Any())
                {
                    supplierInvoiceAccountRow.AccountInternal.AddRange(supplierInvoiceRowInternals);
                }
            }
            #endregion


            SetCreatedProperties(supplierInvoiceAccountRow);
            entities.SupplierInvoiceAccountRow.AddObject(supplierInvoiceAccountRow);

            supplierInvoiceRow.SupplierInvoiceAccountRow.Add(supplierInvoiceAccountRow);
            rowNr++;

            return supplierInvoiceRow;
        }

        #endregion

        #region SupplierInvoiceAccountRow

        private SupplierInvoiceAccountRow ConvertToSupplierInvoiceAccountRow(CompEntities entities, SupplierInvoiceAccountingRowIO accountingRowIO, int actorCompanyId, List<AccountInternal> accountInternals, int rowNr, out ActionResult result)
        {
            result = new ActionResult();
            decimal amount = accountingRowIO.Amount.HasValue ? accountingRowIO.Amount.Value : 0;
            decimal amountCurrency = accountingRowIO.AmountCurrency.HasValue ? accountingRowIO.AmountCurrency.Value : 0;
            // Create new account row, note that DebitRow and CreditRow must be set

            SupplierInvoiceAccountRow row = new SupplierInvoiceAccountRow()
            {
                Type = (int)AccountingRowType.AccountingRow,
                SupplierInvoiceAccountRowId = 0,
                RowNr = rowNr,
                SplitType = (int)SoeInvoiceRowDiscountType.Percent,
                SplitPercent = 0,
                Amount = accountingRowIO.DebitRow ? Math.Abs(amount) : -Math.Abs(amount),
                AmountCurrency = accountingRowIO.DebitRow ? Math.Abs(amountCurrency) : -Math.Abs(amountCurrency),
                Quantity = accountingRowIO.Quantity,
                Text = accountingRowIO.Text,
                InterimRow = accountingRowIO.InterimRow,
                VatRow = accountingRowIO.VatRow,
                ContractorVatRow = accountingRowIO.ContractorVatRow,
                CreditRow = accountingRowIO.CreditRow,
                DebitRow = accountingRowIO.DebitRow,
                State = (int)SoeEntityState.Active
            };

            // Get standard account
            row.AccountStd = AccountManager.GetAccountStd(entities, accountingRowIO.Dim1Id, actorCompanyId, true, false);

            if (row.AccountStd == null)
            {
                result = new ActionResult((int)ActionResultSave.AccountDimRuleNotFulfilled, "Accountingrow is missing a standard account");
                return null;
            }

            #region internal accounts (Dim2-6)

            if (accountingRowIO.Dim2Id != 0)
            {
                AccountInternal accountInternal = accountInternals.FirstOrDefault(a => a.AccountId == accountingRowIO.Dim2Id);
                if (accountInternal != null)
                    row.AccountInternal.Add(accountInternal);
            }
            if (accountingRowIO.Dim3Id != 0)
            {
                AccountInternal accountInternal = accountInternals.FirstOrDefault(a => a.AccountId == accountingRowIO.Dim3Id);
                if (accountInternal != null)
                    row.AccountInternal.Add(accountInternal);
            }
            if (accountingRowIO.Dim4Id != 0)
            {
                AccountInternal accountInternal = accountInternals.FirstOrDefault(a => a.AccountId == accountingRowIO.Dim4Id);
                if (accountInternal != null)
                    row.AccountInternal.Add(accountInternal);
            }
            if (accountingRowIO.Dim5Id != 0)
            {
                AccountInternal accountInternal = accountInternals.FirstOrDefault(a => a.AccountId == accountingRowIO.Dim5Id);
                if (accountInternal != null)
                    row.AccountInternal.Add(accountInternal);
            }
            if (accountingRowIO.Dim6Id != 0)
            {
                AccountInternal accountInternal = accountInternals.FirstOrDefault(a => a.AccountId == accountingRowIO.Dim6Id);
                if (accountInternal != null)
                    row.AccountInternal.Add(accountInternal);
            }
            #endregion

            return row;
        }

        private SupplierInvoiceAccountRow ConvertToSupplierInvoiceAccountRow(CompEntities entities, AccountingRowDTO item, int actorCompanyId, List<AccountInternal> accountInternals)
        {
            // Create new account row
            SupplierInvoiceAccountRow row = new SupplierInvoiceAccountRow()
            {
                Type = (int)item.Type,
                SupplierInvoiceAccountRowId = item.InvoiceAccountRowId,
                RowNr = item.RowNr,
                SplitType = item.SplitType,
                SplitPercent = item.SplitPercent,
                Amount = item.Amount,
                AmountCurrency = item.AmountCurrency,
                AmountEntCurrency = item.AmountEntCurrency,
                AmountLedgerCurrency = item.AmountLedgerCurrency,
                Quantity = item.Quantity,
                Text = item.Text,
                InterimRow = item.IsInterimRow,
                VatRow = item.IsVatRow,
                ContractorVatRow = item.IsContractorVatRow,
                CreditRow = item.IsCreditRow,
                DebitRow = item.IsDebitRow,
                AttestStatus = item.AttestStatus,
                State = (int)item.State,
                AccountDistributionHeadId = item.AccountDistributionHeadId,
                StartDate = item.StartDate,
                NumberOfPeriods = item.NumberOfPeriods
            };

            if (item.AttestUserId != 0)
                row.AttestUserId = item.AttestUserId;
            else
                row.User = null;

            // Get standard account
            row.AccountStd = AccountManager.GetAccountStd(entities, item.Dim1Id, actorCompanyId, true, false, true);

            // Get internal accounts (Dim2-6)
            if (item.Dim2Id != 0)
            {
                AccountInternal accountInternal = accountInternals.FirstOrDefault(a => a.AccountId == item.Dim2Id);
                if (accountInternal != null)
                    row.AccountInternal.Add(accountInternal);
            }
            if (item.Dim3Id != 0)
            {
                AccountInternal accountInternal = accountInternals.FirstOrDefault(a => a.AccountId == item.Dim3Id);
                if (accountInternal != null)
                    row.AccountInternal.Add(accountInternal);
            }
            if (item.Dim4Id != 0)
            {
                AccountInternal accountInternal = accountInternals.FirstOrDefault(a => a.AccountId == item.Dim4Id);
                if (accountInternal != null)
                    row.AccountInternal.Add(accountInternal);
            }
            if (item.Dim5Id != 0)
            {
                AccountInternal accountInternal = accountInternals.FirstOrDefault(a => a.AccountId == item.Dim5Id);
                if (accountInternal != null)
                    row.AccountInternal.Add(accountInternal);
            }
            if (item.Dim6Id != 0)
            {
                AccountInternal accountInternal = accountInternals.FirstOrDefault(a => a.AccountId == item.Dim6Id);
                if (accountInternal != null)
                    row.AccountInternal.Add(accountInternal);
            }

            return row;
        }

        public SupplierInvoiceAccountRow GetSupplierInvoiceAccountRow(CompEntities entities, int supplierInvoiceAccountRowId, bool includeAccountInternal)
        {
            IQueryable<SupplierInvoiceAccountRow> query = entities.SupplierInvoiceAccountRow;

            if (includeAccountInternal)
            {
                query = query.Include("AccountInternal");
            }

            return (from r in query
                    where r.SupplierInvoiceAccountRowId == supplierInvoiceAccountRowId
                    select r).FirstOrDefault();
        }

        public List<SupplierInvoiceAccountRow> GetSupplierInvoiceAccountRows(int invoiceId)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            IQueryable<SupplierInvoiceAccountRow> query = entitiesReadOnly.SupplierInvoiceAccountRow
                .Include("AccountStd.Account")
                .Include("AccountInternal.Account.AccountDim")
                .Include("SupplierInvoiceRow");

            return (from ar in query
                    where ar.SupplierInvoiceRow.InvoiceId == invoiceId &&
                    ar.State == (int)SoeEntityState.Active &&
                    ar.SupplierInvoiceRow.State == (int)SoeEntityState.Active
                    select ar).ToList();
        }

        #endregion

        #region Helpers
        private void AddSupplierInvoiceAttestStatesConverted(CompEntities entities, List<SupplierInvoiceGridDTO> items)
        {
            if (items == null)
                return;

            foreach (var groupCompany in items.GroupBy(i => i.OwnerActorId))
            {
                int actorCompanyId = groupCompany.Key;

                var attestStates = AttestManager.GetAttestStateDTOs(entities, actorCompanyId, TermGroup_AttestEntity.SupplierInvoice, SoeModule.Economy, false);
                if (attestStates.Count > 0)
                {
                    foreach (var item in groupCompany)
                    {
                        var attestState = attestStates.FirstOrDefault(i => i.AttestStateId == item.AttestStateId);
                        if (attestState != null)
                            item.AttestStateName = attestState.Name;
                    }
                }
            }
        }

        private void AddSupplierInvoiceAttestGroupsConverted(List<SupplierInvoiceGridDTO> items)
        {
            if (items == null)
                return;

            foreach (var groupCompany in items.GroupBy(i => i.OwnerActorId))
            {
                int actorCompanyId = groupCompany.Key;

                var attestGroups = AttestManager.GetAttestWorkFlowGroups(actorCompanyId).ToList();
                if (attestGroups.Count > 0)
                {
                    foreach (var item in groupCompany)
                    {
                        if (item.AttestGroupId > 0)
                        {
                            var attestGroup = attestGroups.FirstOrDefault(i => i.AttestWorkFlowHeadId == item.AttestGroupId);
                            if (attestGroup != null)
                                item.AttestGroupName = attestGroup.Name;
                        }
                    }
                }
            }
        }


        private void AddSupplierInvoiceAttestGroupsConverted(
            List<SupplierInvoiceIncomingHallGridDTO> items, 
            bool hasAttestFlowPermission, 
            int actorCompanyId)
        {
            if (hasAttestFlowPermission)
            {

                List<AttestGroupGridDTO> attestGroups = AttestManager
                    .GetAttestWorkFlowGroups(actorCompanyId).ToList();

                foreach (SupplierInvoiceIncomingHallGridDTO item in items)
                {

                    AttestGroupGridDTO attestGroup =
                        attestGroups.FirstOrDefault(
                            i => i.AttestWorkFlowHeadId == item.AttestGroupId);

                    if (attestGroup == null)
                    {
                        // Attest group is not active.
                        // Clear the AttestGroupId to hide
                        // it from the grid filter.
                        item.AttestGroupId = null;
                    }
                    else
                    {
                        item.AttestGroupName = attestGroup?.Name;
                    }
                }


            }
            else 
            {
                foreach (SupplierInvoiceIncomingHallGridDTO item in items)
                {
                    item.AttestGroupId = null;
                }

            }

        }

        private void AddEdiInfo(List<SupplierInvoiceGridDTO> entries)
        {
			var langId = GetLangId();
			var ediSourceTypes = base.GetTermGroupDict(TermGroup.EDISourceType, langId);
			var ediMessageTypes = base.GetTermGroupDict(TermGroup.EdiMessageType, langId);
			var invoiceStatuses = base.GetTermGroupDict(TermGroup.EDIInvoiceStatus, langId);

            foreach(var entry in entries)
            {

				entry.SourceTypeName = entry.Type != 0 ? ediSourceTypes[entry.Type] : "";
				entry.EdiMessageTypeName = entry.EdiMessageType != 0 ? ediMessageTypes[entry.EdiMessageType] : "";
				entry.StatusName = entry.Status != 0 ? invoiceStatuses[entry.Status] : "";
            }
		}

		private List<SupplierInvoiceGridDTO> GetSupplierInvoiceScanningEntries(bool hideVatWarning, int? baseSysCurrencyId)
        {
            using var entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            var entries = new List<SupplierInvoiceGridDTO>();
			var scanningItems = (from e in entitiesReadOnly.ScanningEntryView
									where e.ActorCompanyId == ActorCompanyId &&
									e.State == (int)SoeEntityState.Active &&
									e.MessageType == (int)TermGroup_ScanningMessageType.SupplierInvoice &&
									e.InvoiceId == null
									select e);

            if (scanningItems.Count() == 0)
                return entries;

			var rowItems = EdiManager.GetScanningEntryRowItemsByCompany(entitiesReadOnly, 
                base.ActorCompanyId, 
                scanningItems.Select(s => s.ScanningEntryId).ToList()
            );

			foreach (var entry in scanningItems)
			{
				// Check interpretation
				var rowItemsForEntry = rowItems.Where(i => i.ScanningEntryId == entry.ScanningEntryId).ToList();

				SupplierInvoiceGridDTO dto = entry.ToConvertedScanningSupplierGridDTO(
                    hideVatWarning, 
                    setClosedStyle: true, 
                    foreign: (baseSysCurrencyId != 0 && entry.SysCurrencyId != baseSysCurrencyId));

				if ((ScanningProvider)entry.Provider == ScanningProvider.ReadSoft)
				{
					dto.RoundedInterpretation = EdiManager.GetScanningEntryRoundedInterpretation(rowItemsForEntry);
					dto.EdiMessageProviderName = "ReadSoft";
				}
				else if ((ScanningProvider)entry.Provider == ScanningProvider.AzoraOne)
				{
					dto.RoundedInterpretation = (int)TermGroup_ScanningInterpretation.ValueIsValid;
					dto.EdiMessageProviderName = "AzoraOne";
				}
				dto.Type = (int)TermGroup_SupplierInvoiceType.Scanning;

				entries.Add(dto);
			}

            return entries;
		}

        private List<SupplierInvoiceGridDTO> GetSupplierInvoiceEdiEntries(bool hideVatWarning, int? baseSysCurrencyId)
        {
            var entries = new List<SupplierInvoiceGridDTO>();
            using var entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            var items = (
                from e in entitiesReadOnly.EdiEntryView
				where e.ActorCompanyId == ActorCompanyId &&
				    e.Type == (int)TermGroup_EdiMessageType.SupplierInvoice &&
			        !String.IsNullOrEmpty(e.InvoiceNr) &&
					!e.InvoiceId.HasValue &&
					e.SupplierId.HasValue && e.SupplierId.Value > 0 &&
					e.MessageType == (int)TermGroup_EdiMessageType.SupplierInvoice &&
					e.InvoiceStatus == (int)TermGroup_EDIInvoiceStatus.Unprocessed &&
					(e.Status == (int)TermGroup_EDIStatus.UnderProcessing || e.Status == (int)TermGroup_EDIStatus.Processed) &&
					e.State == (int)SoeEntityState.Active
				select e);

			foreach (var entry in items)
			{
				SupplierInvoiceGridDTO dto = entry.ToConvertedEdiSupplierGridDTO(
                    hideVatWarning, 
                    setClosedStyle: true, 
                    foreign: (baseSysCurrencyId != 0 && entry.SysCurrencyId != baseSysCurrencyId)
                );
				dto.Type = (int)TermGroup_SupplierInvoiceType.EDI;
               
                entries.Add(dto);
			}

            return entries;
		}

        private List<SupplierInvoiceGridDTO> GetSupplierInvoiceUploads(bool hideVatWarning)
        {
            using var entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            var uploaded = (from i in entitiesReadOnly.Invoice.OfType<SupplierInvoice>()
							where i.Origin.ActorCompanyId == ActorCompanyId &&
								i.IsTemplate == false &&
								i.State == (int)SoeEntityState.Active &&
								i.Type == (int)TermGroup_SupplierInvoiceType.Uploaded
							select new SupplierInvoiceGridDTO
							{
								SupplierInvoiceId = i.InvoiceId,
								Type = i.Type,
								OwnerActorId = i.ActorId ?? 0,
								SeqNr = i.SeqNr.ToString() ?? "",
								InvoiceNr = i.InvoiceNr,
								BillingTypeId = i.BillingType,
								Status = 0,
								SupplierName = i.Actor.Supplier.SupplierNr + " " + i.Actor.Supplier.Name,
								SupplierId = i.ActorId ?? 0,
								TotalAmount = i.TotalAmount,
								TotalAmountText = i.TotalAmount.ToString(),
								TotalAmountCurrency = 0,
								VATAmount = 0,
								VATAmountCurrency = 0,
								PayAmount = 0,
								PayAmountCurrency = 0,
								PaidAmount = 0,
								PaidAmountCurrency = 0,
								VatType = i.VatType,
								SysCurrencyId = i.Currency.SysCurrencyId,
								InvoiceDate = i.InvoiceDate,
								DueDate = i.DueDate,
								PayDate = null,
								AttestStateId = null,
								AttestGroupId = 0,
								FullyPaid = i.FullyPayed,
								StatusIcon = i.StatusIcon,
								CurrencyRate = i.CurrencyRate,
								InternalText = i.InternalDescription,
								TimeDiscountDate = i.TimeDiscountDate,
								TimeDiscountPercent = i.TimeDiscountPercent
							}
            ).ToList();

			foreach (var item in uploaded)
			{
				item.PaymentStatuses = string.Empty;
				item.CurrentAttestUserName = string.Empty;
				item.PayAmountCurrencyText = string.Empty;
				item.TotalAmountCurrencyText = string.Empty;
				item.PayAmountText = string.Empty;
				item.HasVoucher = false;
			}

            return uploaded;
		}

        private List<SupplierInvoiceGridDTO> GetPreProcessedSupplierIinvoices(bool hideVatWarning, int? baseSysCurrencyId)
        {
            var preProcessed = new List<SupplierInvoiceGridDTO>();

            //Finvoice Should not be fetched where since we have special import grid for this..
			var scanningEntries = GetSupplierInvoiceScanningEntries(hideVatWarning, baseSysCurrencyId);
			preProcessed.AddRange(scanningEntries);

			var ediEntries = GetSupplierInvoiceEdiEntries(hideVatWarning, baseSysCurrencyId);
			preProcessed.AddRange(ediEntries);

			AddEdiInfo(preProcessed);

			var uploadEntries = GetSupplierInvoiceUploads(hideVatWarning);
			preProcessed.AddRange(uploadEntries);

            return preProcessed;
		}

		private void AddIdentification(List<SupplierInvoiceGridDTO> items)
        {
            //Set Guid to be able to identify each item
            items.ForEach(i => i.Guid = Guid.NewGuid());
        }

        private void AddIdentification(List<SupplierPaymentGridDTO> items)
        {
            //Set Guid to be able to identify each item
            items.ForEach(i => i.Guid = Guid.NewGuid());
        }

        #endregion

        #region Status filter

        public List<SupplierInvoiceGridDTO> FilterSupplierInvoices(SoeOriginStatusClassification classification, List<SupplierInvoiceGridDTO> invoices)
        {
            switch (classification)
            {
                case SoeOriginStatusClassification.SupplierInvoicesOverdue:
                    invoices = invoices.Where(i => (i.Status == (int)SoeOriginStatus.Origin || i.Status == (int)SoeOriginStatus.Voucher) && !i.FullyPaid && i.DueDate < DateTime.Now).ToList();
                    break;
                case SoeOriginStatusClassification.SupplierPaymentsPayed:
                    invoices = invoices.Where(i => (i.Status == (int)SoeOriginStatus.Origin || i.Status == (int)SoeOriginStatus.Voucher) && i.FullyPaid).ToList();
                    break;
                case SoeOriginStatusClassification.SupplierPaymentsUnpayed:
                    invoices = invoices.Where(i => (i.Status == (int)SoeOriginStatus.Origin || i.Status == (int)SoeOriginStatus.Voucher) && !i.FullyPaid && !i.BlockPayment).ToList();
                    break;
                case SoeOriginStatusClassification.SupplierInvoicesAttestFlowHandled:
                case SoeOriginStatusClassification.SupplierInvoicesAttestFlowHandledForeign:
                    {
                        var attestStateClosed = AttestManager.GetAttestStateClosed(base.ActorCompanyId, TermGroup_AttestEntity.SupplierInvoice, SoeModule.Economy);
                        int closedStateId = attestStateClosed != null ? attestStateClosed.AttestStateId : 0;
                        invoices = invoices.Where(i => i.AttestStateId.GetValueOrDefault() != 0 && i.AttestStateId.Value != closedStateId).ToList();
                        break;
                    }
            }

            return invoices;
        }

        public List<SupplierPaymentGridDTO> FilterSupplierPayments(SoeOriginStatusClassification classification, List<SupplierPaymentGridDTO> payments)
        {
            switch (classification)
            {
                case SoeOriginStatusClassification.SupplierInvoicesOverdue:
                    payments = payments.Where(i => (i.Status == (int)SoeOriginStatus.Origin || i.Status == (int)SoeOriginStatus.Voucher) && !i.FullyPaid && i.DueDate < DateTime.Now).ToList();
                    break;
                case SoeOriginStatusClassification.SupplierPaymentsUnpayed:
                    payments = payments.Where(i => (i.Status == (int)SoeOriginStatus.Origin || i.Status == (int)SoeOriginStatus.Voucher) && !i.FullyPaid && !i.BlockPayment).ToList();
                    break;
                case SoeOriginStatusClassification.SupplierPaymentsPayed:
                    payments = payments.Where(i => (i.Status == (int)SoeOriginStatus.Origin || i.Status == (int)SoeOriginStatus.Voucher) && i.FullyPaid).ToList();
                    break;
            }

            return payments;
        }


        private bool AttestFlowIsActiveForUser(List<AttestRowStateView> attestRows, List<int> replacementIds, AttestState attestState, out bool containsRejected)
        {
            var attestWorkFlowRows = new List<AttestRowStateView>();
            containsRejected = false;

            if (!attestRows.Any())
                return false;

            foreach (AttestTransition attestTransitionFrom in attestState.AttestTransitionFrom)
            {
                foreach (AttestRowStateView row in attestRows.Where(r => r.AttestTransitionId == attestTransitionFrom.AttestTransitionId && r.ProcessType != (int)TermGroup_AttestWorkFlowRowProcessType.Registered))
                {
                    if (!attestWorkFlowRows.Any(r => r.AttestWorkFlowRowId == row.AttestWorkFlowRowId))
                    {
                        attestWorkFlowRows.Add(row);
                    }
                }
            }

            if (attestWorkFlowRows.Count > 0)
            {
                containsRejected = attestRows.Any(r => r.Answer == false);
                if (containsRejected)
                {
                    return false;
                }

                if (attestWorkFlowRows[0].Type == (int)TermGroup_AttestWorkFlowType.Any)
                {
                    var rows = attestRows.Where(r => (r.ProcessType == (int)TermGroup_AttestWorkFlowRowProcessType.WaitingForProcess || r.ProcessType == (int)TermGroup_AttestWorkFlowRowProcessType.Returned) && (r.UserId == base.UserId || (r.UserId.HasValue && replacementIds.Contains(r.UserId.Value)))).ToList();
                    if (rows.Count == 0)
                    {
                        return false;
                    }
                }
                else if (attestWorkFlowRows[0].Type == (int)TermGroup_AttestWorkFlowType.All)
                {
                    var rows = attestRows.Where(r => (r.ProcessType == (int)TermGroup_AttestWorkFlowRowProcessType.WaitingForProcess || r.ProcessType == (int)TermGroup_AttestWorkFlowRowProcessType.Returned) && (r.UserId == base.UserId || (r.UserId.HasValue && replacementIds.Contains(r.UserId.Value))) && !r.Answer.HasValue).ToList();

                    if (rows.Count == 0)
                    {
                        return false;
                    }
                }

                return true;
            }

            return true;
        }

        private List<AttestWorkFlowOverviewGridDTO> FilterStatusClassificationSupplierInvoicesAttestFlow(List<AttestWorkFlowOverviewGridDTO> items, List<int> replacementIds, bool filterMyActive, bool filterUserIsInRows, bool filterUserHasHandled)
        {
            //Angular
            List<AttestWorkFlowOverviewGridDTO> validItems = new List<AttestWorkFlowOverviewGridDTO>();
            List<AttestState> cachedAttestStates = new List<AttestState>();
            List<int> forCurrentUserAndNotAttested = new List<int>();
            List<int> userInvoiceIds = new List<int>();
            using var entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            if (filterUserIsInRows)
            {
                /*
                 * This method gets the attest rows (which user, if they have approved or not).
                 * When we filter on userHasHandled, it means that we only want rows where the user has done an action.
                 * This is because we don't we don't want the invoice to turn up as "Mine attested" before it has reached, and been handled, by them.
                 */
                userInvoiceIds = (from r in entitiesReadOnly.AttestWorkFlowRow.AsNoTracking()
                                  where r.ProcessType != (int)TermGroup_AttestWorkFlowRowProcessType.Registered &&
                                  (r.UserId == parameterObject.UserId || (r.UserId.HasValue && replacementIds.Contains(r.UserId.Value))) &&
                                  (filterUserHasHandled && r.Answer != null) &&
                                  r.AttestWorkFlowHead.State == (int)SoeEntityState.Active
                                  select r.AttestWorkFlowHead.RecordId).Distinct().ToList();
            }

            // Get current user with attest roles
            User user = (from u in entitiesReadOnly.User.Include("AttestRoleUser").AsNoTracking()
                         where u.UserId == UserId
                         select u).FirstOrDefault();

            // Get rows
            var actorCompanyId = base.ActorCompanyId;
            var invoiceIds = items.Select(i => i.InvoiceId).ToList();
            IQueryable<AttestRowStateView> attestRowQuery = (from r in entitiesReadOnly.AttestRowStateView.AsNoTracking()
                                                             where r.ActorCompanyId == actorCompanyId &&
                                                              r.State != (int)SoeEntityState.Deleted &&
                                                              r.Entity == (int)SoeEntityType.SupplierInvoice
                                                             select r);

            //SQL Server dosent like contains/in with thousand of rows so use ToList() before where if > 1000
            var attestRows = invoiceIds.Count < 1000 ? attestRowQuery.Where(x => invoiceIds.Contains(x.RecordId)).ToList() : attestRowQuery.ToList().Where(x => invoiceIds.Contains(x.RecordId)).ToList();

            foreach (var item in items)
            {
                if (filterUserIsInRows && !userInvoiceIds.Contains(item.InvoiceId))
                    continue;

                #region AttestState

                if (!item.AttestStateId.HasValue)
                    continue;

                //Get from cache
                AttestState attestState = cachedAttestStates.FirstOrDefault(i => i.AttestStateId == item.AttestStateId.Value);
                if (attestState == null)
                {
                    //Get from db
                    attestState = AttestManager.GetAttestState(item.AttestStateId.Value, true);
                    if (attestState == null)
                        continue;

                    //Add to cache
                    cachedAttestStates.Add(attestState);
                }

                //Any closed rows are for sure not active for user
                if (attestState.Closed && filterMyActive)
                    continue;

                #endregion

                #region AttestWorkFlowHead

                /*AttestWorkFlowHead attestWorkFlowHead = AttestManager.GetAttestWorkFlowHeadFromInvoiceId(item.InvoiceId, false, false, true);
                if (attestWorkFlowHead == null)
                    continue;*/

                #endregion

                bool containsRejected = false;

                // Validate
                var attestRowsForInvoice = attestRows.Where(r => r.RecordId == item.InvoiceId).ToList();
                var isActiveForUser = AttestFlowIsActiveForUser(attestRowsForInvoice, replacementIds, attestState, out containsRejected);

                if (filterMyActive && !isActiveForUser)
                {
                    continue;
                }

                /*
                if ( showUnattestedInvoices && isActiveForUser)
                {
                    forCurrentUserAndNotAttested.Add(item.InvoiceId);
                }
                */

                if (containsRejected)
                    continue;

                item.ShowAttestCommentIcon = attestRowsForInvoice.Any(r => r.Comment != null && r.Comment.Trim() != String.Empty);
                if (item.ShowAttestCommentIcon)
                    item.AttestComments = attestRowsForInvoice.Where(r => !string.IsNullOrEmpty(r.Comment)).Select(r => r.Comment).JoinToString(Environment.NewLine);

                validItems.Add(item);
            }
            /*
            if (showUnattestedInvoices && forCurrentUserAndNotAttested.Any())
            {
                validItems = validItems.Where(v => !forCurrentUserAndNotAttested.Contains(v.InvoiceId) ).ToList();
            }
            */

            return validItems;
        }

        #endregion

        #region Validation
        public bool ValidateFinnishBankPaymentReference(string reference)
        {
            if (reference.Substring(0, 2).ToLower() == "rf")
            {
                var numericString = reference.Substring(4, reference.Length - 4) + "2715" + reference.Substring(2, 2);
                if (BigInteger.TryParse(numericString, out BigInteger refNumber))
                    return refNumber % 97 == 1;
                else
                    return false;
            }
            else
            {
                var formedFIReference = "";

                /// <summary>
                /// FI - We need reference number with checksum. Minimum length is 4 characters. 
                /// Valid reference consists of a number which length is 3-19 characters + 1 checksum that is calculated here and added into
                /// reference. Banks are using reference to check validity of previous numbers in reference. 
                /// We copy all the characters, except last one and create reference based on copied value. If it's identical with reference 
                /// passed here, we can say it's a valid one. 
                /// </summary>

                if (reference.Length < 4)  // First check that it's long enough
                {
                    return false;
                }

                // Original ref without checksum
                var baseReference = reference.Substring(0, reference.Length - 1);
                var linelength = baseReference.Length;

                var sum = 0;
                var multiplier = 7;  // Starting right, weighted by 7,2,1,7,3,1 etc...
                while (linelength > 0)
                {
                    if (!Int32.TryParse(baseReference[linelength - 1].ToString(), out int character))
                        return false;

                    sum += multiplier * (character);//-48
                    switch (multiplier)
                    {
                        case 7:
                            multiplier = 3;
                            break;
                        case 3:
                            multiplier = 1;
                            break;
                        case 1:
                            multiplier = 7;
                            break;
                    }
                    sum %= 10;

                    linelength -= 1;
                }

                sum = 10 - sum;

                if (sum != 10)
                    formedFIReference = baseReference + sum.ToString();
                else
                    formedFIReference = baseReference + "0";

                return formedFIReference == reference;
            }
        }

        public bool ValidateSupplierInvoiceAccountingRowsDiff(SupplierInvoice invoice)
        {
            decimal totalDebitAmount = 0;
            decimal totalCreditAmount = 0;

            foreach (SupplierInvoiceRow invoiceRow in invoice.ActiveSupplierInvoiceRows)
            {
                if (!invoiceRow.SupplierInvoiceAccountRow.Any(r => r.Type == (int)AccountingRowType.SupplierInvoiceAttestRow))
                    continue;

                foreach (SupplierInvoiceAccountRow invoiceAccountRow in invoiceRow.ActiveSupplierInvoiceAccountRows)
                {
                    if (invoiceAccountRow.DebitRow)
                        totalDebitAmount += invoiceAccountRow.AmountCurrency;
                    else
                        totalCreditAmount -= invoiceAccountRow.AmountCurrency;
                }
            }

            return (totalDebitAmount - totalCreditAmount != 0);
        }
        #endregion

        #region Analyzis

        public Dictionary<int, List<SupplierPaymentGridDTO>> GetSupplierPaymentsDictForStateAnalysis(CompEntities entities)
        {
            var counters = new Dictionary<int, List<SupplierPaymentGridDTO>>();
            var supplierPayments = GetSupplierPaymentsForGrid(SoeOriginStatusClassification.SupplierPaymentsUnpayed, TermGroup_ChangeStatusGridAllItemsSelection.All);
            supplierPayments.AddRange(GetSupplierPaymentsForGrid(SoeOriginStatusClassification.SupplierPaymentsPayed, TermGroup_ChangeStatusGridAllItemsSelection.All));

            //SupplierPayment
            counters.Add((int)SoeOriginStatusClassification.SupplierPaymentsUnpayed, FilterSupplierPayments(SoeOriginStatusClassification.SupplierPaymentsUnpayed, supplierPayments));
            counters.Add((int)SoeOriginStatusClassification.SupplierInvoicesOverdue, FilterSupplierPayments(SoeOriginStatusClassification.SupplierInvoicesOverdue, supplierPayments));
            counters.Add((int)SoeOriginStatusClassification.SupplierPaymentsPayed, FilterSupplierPayments(SoeOriginStatusClassification.SupplierPaymentsPayed, supplierPayments));

            return counters;
        }
        #endregion

        #region SupplierProductRow
        public ActionResult TransferSupplierInvoiceProductRows(int actorCompanyId, int customerInvoiceId, int supplierInvoiceId, List<int> supplierInvoiceProductRowIds, int wholesellerId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return TransferSupplierInvoiceProductRows(entities, actorCompanyId, customerInvoiceId, supplierInvoiceId, supplierInvoiceProductRowIds, wholesellerId);
        }
        public ActionResult TransferSupplierInvoiceProductRows(CompEntities entities, int actorCompanyId, int customerInvoiceId, int supplierInvoiceId, List<int> supplierInvoiceProductRowIds, int wholesellerId)
        {
            var rows = entities.SupplierInvoiceProductRow
                .Where(r => 
                    supplierInvoiceProductRowIds.Contains(r.SupplierInvoiceProductRowId) && 
                    r.SupplierInvoice.Origin.ActorCompanyId == actorCompanyId && 
                    r.SupplierInvoiceId == supplierInvoiceId)
                .ToList();

            if (rows.Any(r => r.CustomerInvoiceRowId != null))
            {
                return new ActionResult(GetText(143, "Felaktig statusförändring"));
            }

            var supplierInvoice = entities.Invoice.OfType<SupplierInvoice>().FirstOrDefault(i => i.InvoiceId == supplierInvoiceId && i.Origin.ActorCompanyId == actorCompanyId);
            var order = entities.Invoice.OfType<CustomerInvoice>().FirstOrDefault(i => i.InvoiceId == customerInvoiceId && i.Origin.ActorCompanyId == actorCompanyId);

            var result = EdiManager.TransferSupplierInvoiceRowsToOrder(entities, null, actorCompanyId, supplierInvoice, rows, order, wholesellerId);

            return result;
        }
        public List<SupplierInvoiceProductRowDTO> GetSupplierInvoiceProductRows(int actorCompanyId, int supplierInvoiceId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetSupplierInvoiceProductRows(entities, actorCompanyId, supplierInvoiceId);
        }
        public List<SupplierInvoiceProductRowDTO> GetSupplierInvoiceProductRows(CompEntities entities, int actorCompanyId, int supplierInvoiceId)
        {
            var rows = entities.SupplierInvoiceProductRow
                .Where(r => r.SupplierInvoiceId == supplierInvoiceId && r.SupplierInvoice.Origin.ActorCompanyId == actorCompanyId)
                .Select(r => new SupplierInvoiceProductRowDTO
                {
                    SupplierInvoiceId = r.SupplierInvoiceId,
                    SupplierInvoiceProductRowId = r.SupplierInvoiceProductRowId,
                    CustomerInvoiceRowId = r.CustomerInvoiceRowId,
                    CustomerInvoiceNumber = r.CustomerInvoiceRow.CustomerInvoice.InvoiceNr,
                    CustomerInvoiceId = r.CustomerInvoiceRow.CustomerInvoice.InvoiceId,
                    SellerProductNumber = r.SellerProductNumber,
                    Text = r.Text,
                    UnitCode = r.UnitCode,
                    Quantity = r.Quantity,
                    PriceCurrency = r.PriceCurrency,
                    AmountCurrency = r.AmountCurrency,
                    VatAmountCurrency = r.VatAmountCurrency,
                    VatRate = r.VatRate,
                    State = (SoeEntityState)r.State,
                    Created = r.Created,
                    CreatedBy = r.CreatedBy,
                    Modified = r.Modified,
                    ModifiedBy = r.ModifiedBy,
                    RowType = (SupplierInvoiceRowType)r.RowType
                })
                .ToList();
            return rows;
        }
        public ActionResult SaveSupplierInvoiceProductRows(CompEntities entities, int actorCompanyId, SupplierInvoice invoice, List<SupplierInvoiceProductRowDTO> productRows)
        {
            ActionResult result = new ActionResult();
            foreach (var row in productRows)
            {
                result = AddSupplierInvoiceProductRow(entities, actorCompanyId, invoice, row);
                if (!result.Success)
                    break;
            }
            if (result.Success)
            {
                result = SaveChanges(entities);
            }
            return result;
        }
        public ActionResult AddSupplierInvoiceProductRow(CompEntities entities, int actorCompanyId, SupplierInvoice invoice, SupplierInvoiceProductRowDTO productRow)
        {
            if (productRow.SupplierInvoiceId == 0)
            {
                return new ActionResult("Missing supplier invoice reference");
            }

            SupplierInvoiceProductRow row;
            if (productRow.SupplierInvoiceProductRowId > 0)
            {
                row = entities.SupplierInvoiceProductRow.FirstOrDefault(r => r.SupplierInvoiceProductRowId == productRow.SupplierInvoiceProductRowId && r.SupplierInvoice.Origin.ActorCompanyId == actorCompanyId);
                SetModifiedProperties(row);
            }
            else
            {
                row = new SupplierInvoiceProductRow();
                entities.SupplierInvoiceProductRow.AddObject(row);
                row.SupplierInvoiceId = productRow.SupplierInvoiceId;
                SetCreatedProperties(row);
            }

            if (row == null)
            {
                return new ActionResult("SupplierInvoiceRow is null");
            }

            row.Text = productRow.Text;
            row.SellerProductNumber = productRow.SellerProductNumber;
            row.PriceCurrency = productRow.PriceCurrency;
            row.Quantity = invoice.BillingType == (int)TermGroup_BillingType.Credit && productRow.Quantity > 0 ? decimal.Negate(productRow.Quantity) : productRow.Quantity;
            row.VatAmountCurrency = invoice.BillingType == (int)TermGroup_BillingType.Credit && productRow.VatAmountCurrency > 0 ? decimal.Negate(productRow.VatAmountCurrency) : productRow.VatAmountCurrency;
            row.AmountCurrency = invoice.BillingType == (int)TermGroup_BillingType.Credit && productRow.AmountCurrency > 0 ? decimal.Negate(productRow.AmountCurrency) : productRow.AmountCurrency;
            row.VatRate = productRow.VatRate;
            row.UnitCode = productRow.UnitCode;
            row.RowType = (int)productRow.RowType;
            row.State = productRow.State == SoeEntityState.Active ? 0 : 2;
            
            return new ActionResult();
        }
        #endregion
    }
}
