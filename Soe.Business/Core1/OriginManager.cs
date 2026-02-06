using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Linq;
using System.Transactions;

namespace SoftOne.Soe.Business.Core
{
    public class OriginManager : ManagerBase
    {
        #region Variables

        // Create a logger for use in this class
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #endregion

        #region Ctor

        public OriginManager(ParameterObject parameterObject) : base(parameterObject) { }

        #endregion

        #region Origin

        public Origin GetOrigin(int originId, bool loadOriginUser = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Origin.NoTracking();
            return GetOrigin(entities, originId, loadOriginUser);
        }

        public Origin GetOrigin(CompEntities entities, int originId, bool loadOriginUser = false)
        {
            Origin origin = (from o in entities.Origin
                             where o.OriginId == originId
                             select o).FirstOrDefault();

            if (origin != null && loadOriginUser)
            {
                if (!origin.OriginUser.IsLoaded)
                    origin.OriginUser.Load();
                foreach (OriginUser originUser in origin.OriginUser)
                {
                    if (!originUser.UserReference.IsLoaded)
                        originUser.UserReference.Load();
                }
            }

            return origin;
        }

        public SoeOriginStatus GetOriginStatus(int originId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Origin.NoTracking();
            return GetOriginStatus(entities, originId);
        }

        public SoeOriginStatus GetOriginStatus(CompEntities entities, int originId, bool loadOriginUser = false)
        {
            int? status = (from o in entities.Origin
                             where o.OriginId == originId
                             select o.Status).FirstOrDefault();

            return status.HasValue ? (SoeOriginStatus)status : SoeOriginStatus.None;
        }

        public ActionResult UpdateInvoiceOriginStatusFromAngular(List<int> itemsDict, int actorCompanyId, SoeOriginStatus soeOriginStatus = SoeOriginStatus.Origin, DateTime? invoiceDate = null, DateTime? dueDate = null)
        {
            string failedToTransfer = string.Empty;
            return UpdateOriginStatus(itemsDict, soeOriginStatus, actorCompanyId, invoiceDate, dueDate, failedToTransfer);
        }

        public ActionResult UpdateOriginStatus(List<CustomerInvoiceGridDTO> items, SoeOriginStatus soeOriginStatus, int actorCompanyId, DateTime? invoiceDate = null, DateTime? dueDate = null, DateTime? voucherDate = null)
        {
            if (items == null || soeOriginStatus == SoeOriginStatus.None)
                return new ActionResult((int)ActionResultSave.EntityIsNull, GetText(176, "Felaktig statusförändring"));

            // Default result is successful
            ActionResult result = new ActionResult();

            using (CompEntities entities = new CompEntities())
            {
                entities.Connection.Open();

                bool saveSeqNumber = items.Count > 1;
                Dictionary<int, int> voucherHeadsDict = null;
                List<Invoice> invoices = new List<Invoice>();
                List<CustomerInvoice> customerInvoices = new List<CustomerInvoice>();

                try
                {
                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        #region Prereq

                        #endregion

                        foreach (var item in items)
                        {
                            #region Invoice

                            Invoice invoice = InvoiceManager.GetInvoice(entities, item.CustomerInvoiceId);
                            if (invoice == null)
                                return new ActionResult((int)ActionResultSave.EntityNotFound, $"{GetText(8082, "Underlaget kunde inte hittas")}: Invoice");

                            invoices.Add(invoice);

                            #region CustomerInvoice

                            CustomerInvoice customerInvoice = invoice as CustomerInvoice;
                            if (customerInvoice == null)
                                return new ActionResult((int)ActionResultSave.EntityNotFound, $"{GetText(8082, "Underlaget kunde inte hittas")}: CustomerInvoice");
                            if (customerInvoice.Origin == null)
                                return new ActionResult((int)ActionResultSave.EntityNotFound, $"{GetText(8082, "Underlaget kunde inte hittas")}: Origin");
                            if (customerInvoice.VoucherHeadId != null)
                                return new ActionResult((int)ActionResultSave.VoucherExists, $"{GetText(176, "Felaktig statusförändring")}: {GetText(1910, "Fakturanr")} {customerInvoice.InvoiceNr}");

                            //Check if customer is blocked
                            if (soeOriginStatus == SoeOriginStatus.Origin)
                            {
                                bool isBlocked = CustomerManager.IsCustomerBlocked(entities, item.ActorCustomerId);
                                if (isBlocked)
                                {
                                    var customer = CustomerManager.GetCustomer(entities, item.ActorCustomerId, true);
                                    return new ActionResult((int)ActionResultSave.CustomerIsBlocked, GetText(8309) + ": " + customer.CustomerNr + " - " + customer.Name);
                                }
                            }

                            //Dates
                            InvoiceManager.SetCustomerInvoiceDateAndDueDateIfMissing(entities, customerInvoice, actorCompanyId, invoiceDate: invoiceDate, dueDate: dueDate);

                            //Currency
                            var currencyUpdated = false;
                            if (soeOriginStatus == SoeOriginStatus.Origin && (customerInvoice.CurrencyDate != customerInvoice.InvoiceDate))
                            {
                                currencyUpdated = InvoiceManager.ChangeCurrencyDate(entities, customerInvoice, customerInvoice.InvoiceDate.Value, true);
                            }

                            //Calculate currency amounts, but could have already been done by ChangeCurrencyDate
                            if (!currencyUpdated)
                            {
                                CountryCurrencyManager.CalculateCurrencyAmounts(entities, actorCompanyId, customerInvoice);
                            }

                            customerInvoices.Add(customerInvoice);

                            #endregion

                            //VoucherDate
                            InvoiceManager.SetVoucherDateIfMissing(invoice, voucherDate);

                            #endregion
                        }

                        #region Sequence number

                        foreach (Invoice invoice in invoices)
                        {
                            bool draftToOrigin = (invoice.Origin.Status == (int)SoeOriginStatus.Draft && soeOriginStatus == SoeOriginStatus.Origin);
                            invoice.Origin.Status = (int)soeOriginStatus;

                            int seqNbr = invoice.SeqNr ?? 0;
                            if (seqNbr != 0)
                            {
                                //if the inovice already has a seqNbr, check that no other invoices has the same seqnumber, if so get next seqnumber
                                List<Invoice> invoicesWithThisSeqNr = InvoiceManager.GetInvoicesBySeqNr(entities, actorCompanyId, seqNbr, (SoeOriginType)invoice.Origin.Type);
                                if (invoicesWithThisSeqNr.Any(item => item.InvoiceId != invoice.InvoiceId))
                                {
                                    seqNbr = 0;
                                }
                            }

                            if (seqNbr == 0)
                            {
                                var seqNbrDto = InvoiceManager.GetNextSequenceNumber(entities, invoice as CustomerInvoice, saveSeqNumber);
                                invoice.SeqNr = seqNbr = seqNbrDto.SeqNr;

                                // Set invoice number to same as sequence number
                                if (string.IsNullOrEmpty(invoice.InvoiceNr))
                                    invoice.InvoiceNr = seqNbr.ToString();

                                //If segnr is given we need to create ocr
                                if (invoice.IsCustomerInvoice)
                                    InvoiceManager.SetCustomerInvoiceOcrIfMissing(entities, invoice as CustomerInvoice, actorCompanyId);
                            }

                            //Stock
                            InvoiceManager.HandleStockOnInvoice(invoice as CustomerInvoice, entities, transaction, actorCompanyId, null, false, draftToOrigin);
                        }

                        result = SaveChanges(entities);

                        if (!result.Success)
                            return result;

                        //Commit transaction
                        if (result.Success)
                            transaction.Complete();
                    }

                    #endregion
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
                        #region Save Voucher

                        if (result.Success)
                        {
                            if (customerInvoices.Count > 0)
                                result = InvoiceManager.TryTransferCustomerInvoicesToVoucher(entities, ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, customerInvoices, actorCompanyId);

                            if (result.Success)
                                voucherHeadsDict = NumberUtility.MergeDictictionary(voucherHeadsDict, result.IdDict);
                        }

                        #endregion

                        //Set success properties
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

        public ActionResult UpdateOriginStatus(CompEntities entities, TransactionScope transaction, CustomerInvoice invoice, SoeOriginStatus soeOriginStatus, int actorCompanyId, DateTime? invoiceDate = null, DateTime? dueDate = null, bool cashSale = false)
        {
            ActionResult result = new ActionResult();

            try
            {
                #region Invoice

                //Check if customer is blocked
                if (soeOriginStatus == SoeOriginStatus.Origin)
                {
                    bool isBlocked = CustomerManager.IsCustomerBlocked(entities, invoice.ActorId);
                    if (isBlocked)
                    {
                        var customer = CustomerManager.GetCustomer(entities, invoice.ActorId.Value, true);
                        return new ActionResult((int)ActionResultSave.CustomerIsBlocked, GetText(8309) + ": " + customer.CustomerNr + " - " + customer.Name);
                    }
                }

                //Dates
                InvoiceManager.SetCustomerInvoiceDateAndDueDateIfMissing(entities, invoice, actorCompanyId, invoiceDate: invoiceDate, dueDate: dueDate);

                //Calculate currency amounts
                CountryCurrencyManager.CalculateCurrencyAmounts(entities, actorCompanyId, invoice);

                //Stock
                bool draftToOrigin = (invoice.Origin.Status == (int)SoeOriginStatus.Draft && soeOriginStatus == SoeOriginStatus.Origin);
                InvoiceManager.HandleStockOnInvoice(invoice, entities, transaction, actorCompanyId, null, false, draftToOrigin);

                //VoucherDate
                InvoiceManager.SetVoucherDateIfMissing(invoice);

                #endregion

                #region Origin

                invoice.Origin.Status = (int)soeOriginStatus;

                #endregion

                #region Sequence number

                int seqNbr = invoice.SeqNr.HasValue ? invoice.SeqNr.Value : 0;
                if (seqNbr != 0)
                {
                    //if the inovice already has a seqNbr, check that no other invoices has the same seqnumber, if so get next seqnumber
                    List<Invoice> invoicesWithThisSeqNr = InvoiceManager.GetInvoicesBySeqNr(entities, actorCompanyId, seqNbr, (SoeOriginType)invoice.Origin.Type);
                    if (invoicesWithThisSeqNr.Any())
                    {
                        seqNbr = 0;
                    }
                }

                if (seqNbr == 0)
                {
                    seqNbr = InvoiceManager.GetNextSequenceNumber(entities, (SoeOriginType)invoice.Origin.Type, soeOriginStatus, (TermGroup_BillingType)invoice.BillingType, invoice.Origin.ActorCompanyId, false, cashSale);

                    invoice.SeqNr = seqNbr;

                    // Set invoice number to same as sequence number
                    if (String.IsNullOrEmpty(invoice.InvoiceNr))
                        invoice.InvoiceNr = seqNbr.ToString();
                    //If segnr is given we need to create ocr
                    if (invoice.IsCustomerInvoice)
                        InvoiceManager.SetCustomerInvoiceOcrIfMissing(entities, invoice, actorCompanyId);
                }

                result = SaveChanges(entities);

                if (!result.Success)
                    return result;

                #endregion
            }
            catch (Exception ex)
            {
                base.LogError(ex, this.log);
                result.Exception = ex;
            }
            finally
            {
                if (!result.Success)
                    base.LogTransactionFailed(this.ToString(), this.log);
            }

            return result;
        }

        public ActionResult UpdateOriginStatus(List<int> ids, SoeOriginStatus soeOriginStatus, int actorCompanyId, DateTime? invoiceDate = null, DateTime? dueDate = null, string failedToTransfer = null)
        {
            if (ids == null || soeOriginStatus == SoeOriginStatus.None)
                return new ActionResult((int)ActionResultSave.EntityIsNull, GetText(176, "Felaktig statusförändring"));

            // Default result is successful
            ActionResult result = new ActionResult();

            bool continueIfError = false;
            //New list of invoices not able to transfer (Angular)
            if (failedToTransfer != null)
                continueIfError = true;

            using (CompEntities entities = new CompEntities())
            {
                entities.Connection.Open();

                bool saveSeqNumber = ids.Count > 1;
                string sequenceNumbers = String.Empty;
                Dictionary<int, int> voucherHeadsDict = null;
                List<Invoice> invoices = new List<Invoice>();
                List<CustomerInvoice> customerInvoices = new List<CustomerInvoice>();
                List<SupplierInvoice> supplierInvoices = new List<SupplierInvoice>();

                try
                {
                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
        
                        foreach (var id in ids)
                        {
                            #region Invoice

                            Invoice invoice = InvoiceManager.GetInvoice(entities, id);
                            if (invoice == null)
                                return new ActionResult((int)ActionResultSave.EntityNotFound, "Invoice");

                            invoices.Add(invoice);

                            if (invoice.IsCustomerInvoice)
                            {
                                #region CustomerInvoice

                                CustomerInvoice customerInvoice = invoice as CustomerInvoice;
                                if (customerInvoice == null)
                                    return new ActionResult((int)ActionResultSave.EntityNotFound, "CustomerInvoice");
                                if (customerInvoice.Origin == null)
                                    return new ActionResult((int)ActionResultSave.EntityNotFound, "Origin");

                                //Check if customer is blocked
                                if (soeOriginStatus == SoeOriginStatus.Origin)
                                {
                                    bool isBlocked = CustomerManager.IsCustomerBlocked(entities, invoice.ActorId);
                                    if (isBlocked)
                                        return new ActionResult((int)ActionResultSave.CustomerIsBlocked);
                                }

                                //Dates
                                InvoiceManager.SetCustomerInvoiceDateAndDueDateIfMissing(entities, customerInvoice, actorCompanyId, invoiceDate: invoiceDate, dueDate: dueDate);

                                //Calculate currency amounts
                                CountryCurrencyManager.CalculateCurrencyAmounts(entities, actorCompanyId, customerInvoice);

                                //Stock
                                bool draftToOrigin = (invoice.Origin.Status == (int)SoeOriginStatus.Draft && soeOriginStatus == SoeOriginStatus.Origin);
                                InvoiceManager.HandleStockOnInvoice(customerInvoice, entities, transaction, actorCompanyId, null, false, draftToOrigin);

                                customerInvoices.Add(customerInvoice);

                                #endregion
                            }
                            else if (invoice.IsSupplierInvoice)
                            {
                                #region SupplierInvoice

                                SupplierInvoice supplierInvoice = invoice as SupplierInvoice;
                                if (supplierInvoice == null)
                                    return new ActionResult((int)ActionResultSave.EntityNotFound, "SupplierInvoice");
                                if (supplierInvoice.Origin == null)
                                    return new ActionResult((int)ActionResultSave.EntityNotFound, "Origin");

                                //Calculate currency amounts
                                CountryCurrencyManager.CalculateCurrencyAmounts(entities, actorCompanyId, supplierInvoice);

                                supplierInvoices.Add(supplierInvoice);

                                #endregion
                            }

                            //VoucherDate
                            InvoiceManager.SetVoucherDateIfMissing(invoice);

                            #endregion

                            #region Origin

                            invoice.Origin.Status = (int)soeOriginStatus;

                            #endregion
                        }

                        #region Sequence number

                        foreach (Invoice invoice in invoices)
                        {
                            int seqNbr = invoice.SeqNr.HasValue ? invoice.SeqNr.Value : 0;
                            if (seqNbr != 0)
                            {
                                //if the inovice already has a seqNbr, check that no other invoices has the same seqnumber, if so get next seqnumber
                                List<Invoice> invoicesWithThisSeqNr = InvoiceManager.GetInvoicesBySeqNr(entities, actorCompanyId, seqNbr, (SoeOriginType)invoice.Origin.Type);
                                if (invoicesWithThisSeqNr.Any(item => item.InvoiceId != invoice.InvoiceId))
                                {
                                    seqNbr = 0;
                                }
                            }

                            if (seqNbr == 0)
                            {
                                seqNbr = InvoiceManager.GetNextSequenceNumber(entities, (SoeOriginType)invoice.Origin.Type, soeOriginStatus, (TermGroup_BillingType)invoice.BillingType, invoice.Origin.ActorCompanyId, saveSeqNumber);

                                invoice.SeqNr = seqNbr;

                                // Set invoice number to same as sequence number
                                if (String.IsNullOrEmpty(invoice.InvoiceNr))
                                    invoice.InvoiceNr = seqNbr.ToString();
                                //If segnr is given we need to create ocr
                                if (invoice.IsCustomerInvoice)
                                    InvoiceManager.SetCustomerInvoiceOcrIfMissing(entities, invoice as CustomerInvoice, actorCompanyId);

                                if (invoice.IsSupplierInvoice)
                                {
                                    EdiEntry ediEntry = EdiManager.GetEdiEntryFromInvoice(entities, invoice.InvoiceId);

                                    if (ediEntry != null && ediEntry.Type == (int)TermGroup_EDISourceType.Finvoice)
                                        ediEntry.SeqNr = seqNbr;
                                }
                            }

                            if (sequenceNumbers != String.Empty)
                                sequenceNumbers += ", " + invoice.SeqNr;
                            else
                                sequenceNumbers += invoice.SeqNr;
                        }

                        #endregion

                        result = SaveChanges(entities);

                        if (!result.Success)
                            return result;

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
                        #region Save Voucher

                        if (result.Success)
                        {
                            if (customerInvoices.Count > 0)
                                result = InvoiceManager.TryTransferCustomerInvoicesToVoucher(entities, ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, customerInvoices, actorCompanyId);
                            else if (supplierInvoices.Count > 0)
                            {
                                if (continueIfError)
                                    result = SupplierInvoiceManager.TryTransferSupplierInvoiceToVoucher(entities, ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, supplierInvoices, actorCompanyId, failedToTransfer);
                                else
                                    result = SupplierInvoiceManager.TryTransferSupplierInvoiceToVoucher(entities, ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, supplierInvoices, actorCompanyId);
                            }
                            if (result.Success)
                                voucherHeadsDict = NumberUtility.MergeDictictionary(voucherHeadsDict, result.IdDict);
                        }

                        #endregion

                        //Set success properties
                        if (voucherHeadsDict != null)
                        {
                            result.IdDict = voucherHeadsDict;
                        }

                        //Set sequence numbers
                        result.StringValue = sequenceNumbers;
                    }
                    else
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }

                return result;
            }
        }

        public int? GetOriginStatusFromCustomerInvoiceRow(int customerInvoiceRow)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            return entitiesReadOnly.GetOriginStatusFromCustomerInvoiceRow(customerInvoiceRow).FirstOrDefault();
        }

        #endregion

        #region OriginUser

        public List<OriginUserView> GetOriginUsers(int originId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Origin.NoTracking();
            return GetOriginUsers(entities, originId);
        }

        public List<OriginUserView> GetOriginUsers(CompEntities entities, int originId)
        {
            return (from o in entities.OriginUserView
                    where o.OriginId == originId
                    orderby o.Name
                    select o).ToList();
        }

        public ActionResult SaveOriginUsers(CompEntities entities, int originId, List<OriginUserDTO> originUsers)
        {
            ActionResult result = new ActionResult();

            // Get origin
            Origin origin = OriginManager.GetOrigin(entities, originId, true);

            if (originUsers == null)
                originUsers = new List<OriginUserDTO>();
            if (origin.OriginUser == null)
                origin.OriginUser = new EntityCollection<OriginUser>();

            foreach (OriginUserDTO originUser in originUsers)
            {
                OriginUser existingUser = origin.OriginUser.FirstOrDefault(u => u.UserId == originUser.UserId);
                if (existingUser != null)
                {
                    #region Update

                    // User exists on origin
                    if (existingUser.State != (int)SoeEntityState.Active)
                    {
                        // User is deleted or inactive, reactivate it
                        existingUser.State = (int)SoeEntityState.Active;
                        SetModifiedProperties(existingUser);
                    }
                    if (existingUser.Main != originUser.Main)
                    {
                        existingUser.Main = originUser.Main;
                        SetModifiedProperties(existingUser);
                    }

                    #endregion
                }
                else
                {
                    #region Add

                    User newUser = UserManager.GetUser(entities, originUser.UserId);
                    if (newUser != null)
                    {
                        OriginUser newOriginUser = new OriginUser();
                        newOriginUser.Origin = origin;
                        newOriginUser.User = newUser;
                        newOriginUser.Main = originUser.Main;
                        origin.OriginUser.Add(newOriginUser);
                        SetCreatedProperties(newOriginUser);
                    }

                    #endregion
                }
            }

            #region Delete

            // Remove deleted users
            foreach (OriginUser originUser in origin.OriginUser.Where(o => o.State == (int)SoeEntityState.Active))
            {
                if (originUser.User == null || !originUsers.Any(u => u.UserId == originUser.User.UserId && u.State == (int)SoeEntityState.Active))
                {
                    ChangeEntityState(originUser, SoeEntityState.Deleted);
                }
            }

            #endregion


            return result;
        }

        #endregion
    }
}