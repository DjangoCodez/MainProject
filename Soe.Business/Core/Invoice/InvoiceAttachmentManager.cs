using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Linq;

namespace SoftOne.Soe.Business.Core
{
    public class InvoiceAttachmentManager : ManagerBase
    {
        #region Ctor

        public InvoiceAttachmentManager(ParameterObject parameterObject) : base(parameterObject) { }

        #endregion

        public InvoiceAttachment GetInvoiceAttachment(int invoiceAttachmentId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.InvoiceAttachment.NoTracking();
            return GetInvoiceAttachment(entities, invoiceAttachmentId);
        }

        public InvoiceAttachment GetInvoiceAttachment(CompEntities entities, int invoiceAttachmentId)
        {
            return (from ia in entities.InvoiceAttachment
                    where ia.InvoiceAttachmentId == invoiceAttachmentId
                    select ia).FirstOrDefault();
        }

        public ActionResult AddInvoiceAttachment(int invoiceId, int id, InvoiceAttachmentSourceType sourceType, InvoiceAttachmentConnectType connectType, bool addToDistribution, bool addToTransfer)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.InvoiceAttachment.NoTracking();
            return AddInvoiceAttachment(entities, invoiceId, id, sourceType, connectType, addToDistribution, addToTransfer);
        }

        public ActionResult AddInvoiceAttachment(CompEntities entities, int invoiceId, int id, InvoiceAttachmentSourceType sourceType, InvoiceAttachmentConnectType connectType, bool addToDistribution, bool addToTransfer)
        {
            var attachment = new InvoiceAttachment()
            {
                InvoiceId = invoiceId,
                DataStorageRecordId = sourceType == InvoiceAttachmentSourceType.DataStorage ? id : (int?)null,
                EdiEntryId = sourceType == InvoiceAttachmentSourceType.Edi ? id : (int?)null,
                AddAttachmentsOnEInvoice = addToDistribution,
                AddAttachmentsOnTransfer = addToTransfer,
            };

            SetCreatedProperties(attachment);
            entities.InvoiceAttachment.AddObject(attachment);

            ActionResult result = SaveChanges(entities);
            if (result.Success)
                result.IntegerValue = attachment.InvoiceAttachmentId;

            return result;
        }

        public ActionResult AddInvoiceAttachmentToDataStorageRecord(CustomerInvoice invoice, DataStorageRecord dataStorageRecord, InvoiceAttachmentConnectType connectType, bool addToDistribution, bool addToTransfer, bool saveChanges = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.InvoiceAttachment.NoTracking();
            return AddInvoiceAttachmentToDataStorageRecord(entities, invoice, dataStorageRecord, connectType, addToDistribution, addToTransfer, saveChanges);
        }

        public ActionResult AddInvoiceAttachmentToDataStorageRecord(CompEntities entities, CustomerInvoice invoice, DataStorageRecord dataStorageRecord, InvoiceAttachmentConnectType connectType, bool addToDistribution, bool addToTransfer, bool saveChanges = false)
        {
            ActionResult result = new ActionResult();
            var attachment = new InvoiceAttachment()
            {
                CustomerInvoice = invoice,
                DataStorageRecord = dataStorageRecord,
                AddAttachmentsOnEInvoice = addToDistribution,
                AddAttachmentsOnTransfer = addToTransfer,
                AttachedType = (int)connectType,
            };

            SetCreatedProperties(attachment);
            entities.InvoiceAttachment.AddObject(attachment);

            dataStorageRecord.InvoiceAttachment.Add(attachment);

            if (saveChanges)
            {
                result = SaveChanges(entities);
                if (result.Success)
                    result.IntegerValue = attachment.InvoiceAttachmentId;
            }

            return result;
        }

        public ActionResult AddInvoiceAttachmentToEdiEntry(CustomerInvoice invoice, EdiEntry ediEntry, InvoiceAttachmentConnectType connectType, bool addToDistribution, bool addToTransfer, bool saveChanges = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.InvoiceAttachment.NoTracking();
            return AddInvoiceAttachmentToEdiEntry(entities, invoice, ediEntry, connectType, addToDistribution, addToTransfer, saveChanges);
        }

        public ActionResult AddInvoiceAttachmentToEdiEntry(CompEntities entities, CustomerInvoice invoice, EdiEntry ediEntry, InvoiceAttachmentConnectType connectType, bool addToDistribution, bool addToTransfer, bool saveChanges = false)
        {
            ActionResult result = new ActionResult();
            var attachment = new InvoiceAttachment()
            {
                CustomerInvoice = invoice,
                EdiEntry = ediEntry,
                AddAttachmentsOnEInvoice = addToDistribution,
                AddAttachmentsOnTransfer = addToTransfer,
                AttachedType = (int)connectType,
            };

            SetCreatedProperties(attachment);
            entities.InvoiceAttachment.AddObject(attachment);

            if (saveChanges)
            {
                result = SaveChanges(entities);
                if (result.Success)
                    result.IntegerValue = attachment.InvoiceAttachmentId;
            }

            return result;
        }

        public ActionResult ConnectInvoiceAttachmentToDistribution(int invoiceAttachmentId, int invoiceDistributionId, bool saveChanges = false)
        {
            ActionResult result = new ActionResult();
            using (CompEntities entities = new CompEntities())
            {
                var invoiceAttachment = GetInvoiceAttachment(entities, invoiceAttachmentId);
                if (invoiceAttachment != null)
                {
                    invoiceAttachment.LastDistributedDate = DateTime.Now;
                    SetModifiedProperties(invoiceAttachment);

                    var distributionAttachment = new InvoiceDistributionAttachment()
                    {
                        InvoiceAttachmentId = invoiceAttachment.InvoiceAttachmentId,
                        InvoiceDistributionId = invoiceDistributionId,
                    };

                    SetCreatedProperties(distributionAttachment);
                    entities.InvoiceDistributionAttachment.AddObject(distributionAttachment);

                    if (saveChanges)
                        result = SaveChanges(entities);
                }
            }

            return result;
        }
    }
}
