using Soe.WebApi.Controllers;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using System.Net;
using System.Web.Http;
using SoftOne.Soe.Data;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;
using System.Linq;

namespace Soe.WebApi.V2.Core
{
    [RoutePrefix("V2/Core/Image")]
    public class ImageController : SoeApiController
    {
        #region Variables

        private readonly GraphicsManager grm;
        private readonly GeneralManager gm;
        private readonly InvoiceManager im;


        #endregion

        #region Constructor

        public ImageController(GraphicsManager grm, GeneralManager gm, InvoiceManager im)
        {
            this.grm = grm;
            this.gm = gm;
            this.im = im;
        }

        #endregion

        #region Images

        [HttpGet]
        [Route("{imageType:int}/{entity:int}/{recordId:int}/{useThumbnails:bool}/{projectId:int}")]
        public IHttpActionResult GetImages(SoeEntityImageType imageType, SoeEntityType entity, int recordId, bool useThumbnails, int projectId)
        {
            List<ImagesDTO> items;

            if (entity == SoeEntityType.Employee)
            {
                items = grm.GetImages(base.ActorCompanyId, imageType, entity, recordId).ToDTOs(useThumbnails).ToList();

                if (imageType == SoeEntityImageType.EmployeePortrait)
                {
                    // Just one employee portrait, do not fetch any more images
                    if (!items.Any())
                    {
                        items.AddRange(gm.GetDataStorageRecords(base.ActorCompanyId, base.RoleId, recordId, entity, SoeDataStorageRecordType.EmployeePortrait, false, true).ToImagesDTOs(true));
                    }
                }
                else if (imageType == SoeEntityImageType.EmployeeFile)
                {
                    items.AddRange(gm.GetDataStorageRecords(base.ActorCompanyId, base.RoleId, recordId, entity, loadConfirmationStatus: true, includeDataStorage: true, includeAttestState: true).ToImagesDTOs(false));
                }
                else
                {
                    items.AddRange(gm.GetDataStorageRecords(base.ActorCompanyId, base.RoleId, recordId, entity, loadConfirmationStatus: true, includeDataStorage: true, includeAttestState: true).ToImagesDTOs(false));
                }
            }
            else
            {
                var typeNameGeneral = TermCacheManager.Instance.GetText(7464, 1, "Manuellt tillagd");
                var typeNameSignature = TermCacheManager.Instance.GetText(7465, 1, "Signatur");
                var typeNameSupplierInvoice = TermCacheManager.Instance.GetText(31, 1, "Leverantörsfaktura");
                var typeNameEdi = TermCacheManager.Instance.GetText(7467, 1, "EDI");

                items = grm.GetImages(base.ActorCompanyId, imageType, entity, recordId).ToDTOs(useThumbnails, typeNameGeneral, true).ToList();
                if (entity == SoeEntityType.Order)
                {
                    var invoice = im.GetCustomerInvoice(recordId);

                    // Add head
                    var signatures = grm.GetImages(base.ActorCompanyId, SoeEntityImageType.OrderInvoiceSignature, entity, recordId).ToDTOs(useThumbnails, typeNameSignature, true).ToList();
                    foreach (var signature in signatures)
                    {
                        if (!items.Any(r => r.ImageId == signature.ImageId && r.SourceType == signature.SourceType))
                            items.Add(signature);
                    }

                    var rows = grm.GetImagesFromOrderRows(base.ActorCompanyId, recordId, false, null, null, false, addToDistribution: invoice?.AddSupplierInvoicesToEInvoices ?? false);
                    foreach (var row in rows)
                    {
                        if (!items.Any(r => r.ImageId == row.ImageId && r.SourceType == row.SourceType))
                        {
                            if (row.IncludeWhenDistributed == null && invoice != null)
                                row.IncludeWhenDistributed = invoice.AddSupplierInvoicesToEInvoices;
                            row.ConnectedTypeName = typeNameSupplierInvoice;
                            items.Add(row);
                        }
                    }

                    var links = grm.GetImagesFromLinkToProject(base.ActorCompanyId, recordId, projectId, false, null, false, addToDistribution: invoice?.AddSupplierInvoicesToEInvoices ?? false);
                    foreach (var link in links)
                    {
                        if (!items.Any(r => r.ImageId == link.ImageId && r.SourceType == link.SourceType))
                        {
                            if (link.IncludeWhenDistributed == null && invoice != null)
                                link.IncludeWhenDistributed = invoice.AddSupplierInvoicesToEInvoices;
                            link.ConnectedTypeName = typeNameSupplierInvoice;
                            items.Add(link);
                        }
                    }

                    var records = gm.GetDataStorageRecordsForCustomerInvoice(base.ActorCompanyId, base.RoleId, recordId, SoeEntityType.None, new List<SoeDataStorageRecordType> { SoeDataStorageRecordType.OrderInvoiceFileAttachment, SoeDataStorageRecordType.OrderInvoiceSignature }, invoice?.AddSupplierInvoicesToEInvoices ?? false);
                    foreach (var record in records)
                    {
                        if (!items.Any(r => r.ImageId == record.ImageId && r.SourceType == record.SourceType))
                        {
                            if (record.IncludeWhenDistributed == null && invoice != null)
                                record.IncludeWhenDistributed = invoice.AddAttachementsToEInvoice;
                            items.Add(record);
                        }
                    }

                    // Add children
                    var mappings = im.GetChildInvoices(recordId);
                    foreach (var mapping in mappings)
                    {
                        var typeNameChild = TermCacheManager.Instance.GetText(7469, 1, "underorder");

                        var childImages = grm.GetImages(base.ActorCompanyId, imageType, entity, mapping.ChildInvoiceId).ToDTOs(useThumbnails, typeNameGeneral + " (" + typeNameChild + ")", true).ToList();
                        foreach (var images in childImages)
                        {
                            if (!items.Any(r => r.ImageId == images.ImageId && r.SourceType == images.SourceType))
                                items.Add(images);
                        }

                        var childSignatures = grm.GetImages(base.ActorCompanyId, SoeEntityImageType.OrderInvoiceSignature, entity, mapping.ChildInvoiceId).ToDTOs(useThumbnails, typeNameSignature + " (" + typeNameChild + ")", true).ToList();
                        foreach (var signature in childSignatures)
                        {
                            if (!items.Any(r => r.ImageId == signature.ImageId && r.SourceType == signature.SourceType))
                                items.Add(signature);
                        }

                        var childRows = grm.GetImagesFromOrderRows(base.ActorCompanyId, mapping.ChildInvoiceId, false, typeNameEdi + " (" + typeNameChild + ")", null, false, mapping.MainInvoiceId);
                        foreach (var row in childRows)
                        {
                            if (!items.Any(r => r.ImageId == row.ImageId && r.SourceType == row.SourceType))
                            {
                                if (row.IncludeWhenDistributed == null)
                                    row.IncludeWhenDistributed = invoice.AddSupplierInvoicesToEInvoices;
                                row.ConnectedTypeName = typeNameSupplierInvoice + " (" + typeNameChild + ")";
                                items.Add(row);
                            }
                        }

                        var childLinks = grm.GetImagesFromLinkToProject(base.ActorCompanyId, mapping.ChildInvoiceId, projectId, false, null, false, mapping.MainInvoiceId);
                        foreach (var link in childLinks)
                        {
                            if (!items.Any(r => r.ImageId == link.ImageId && r.SourceType == link.SourceType))
                            {
                                if (link.IncludeWhenDistributed == null)
                                    link.IncludeWhenDistributed = invoice.AddSupplierInvoicesToEInvoices;
                                link.ConnectedTypeName = typeNameSupplierInvoice + " (" + typeNameChild + ")";
                                items.Add(link);
                            }
                        }

                        var childRecords = gm.GetDataStorageRecordsForCustomerInvoice(base.ActorCompanyId, base.RoleId, mapping.ChildInvoiceId, SoeEntityType.None, new List<SoeDataStorageRecordType> { SoeDataStorageRecordType.OrderInvoiceFileAttachment, SoeDataStorageRecordType.OrderInvoiceSignature }, invoice?.AddSupplierInvoicesToEInvoices ?? false, mapping.MainInvoiceId);
                        foreach (var record in childRecords)
                        {
                            if (!items.Any(r => r.ImageId == record.ImageId && r.SourceType == record.SourceType))
                            {
                                if (record.IncludeWhenDistributed == null)
                                    record.IncludeWhenDistributed = invoice.AddAttachementsToEInvoice;
                                items.Add(record);
                            }
                        }
                    }
                }
                else if (entity == SoeEntityType.CustomerInvoice)
                {
                    var invoice = im.GetCustomerInvoice(recordId);

                    var records = gm.GetDataStorageRecordsForCustomerInvoice(base.ActorCompanyId, base.RoleId, recordId, SoeEntityType.None, new List<SoeDataStorageRecordType> { SoeDataStorageRecordType.OrderInvoiceFileAttachment, SoeDataStorageRecordType.InvoicePdf, SoeDataStorageRecordType.InvoiceBitmap, SoeDataStorageRecordType.OrderInvoiceSignature }, invoice?.AddSupplierInvoicesToEInvoices ?? false);
                    foreach (var record in records)
                    {
                        if (!items.Any(r => r.ImageId == record.ImageId && r.SourceType == record.SourceType))
                        {
                            if (record.IncludeWhenDistributed == null && invoice != null)
                                record.IncludeWhenDistributed = invoice.AddAttachementsToEInvoice;
                            items.Add(record);
                        }
                    }
                }
                else if (entity == SoeEntityType.Offer || entity == SoeEntityType.Contract)
                {
                    var invoice = im.GetCustomerInvoice(recordId);

                    var records = gm.GetDataStorageRecordsForCustomerInvoice(base.ActorCompanyId, base.RoleId, recordId, SoeEntityType.None, new List<SoeDataStorageRecordType> { SoeDataStorageRecordType.OrderInvoiceFileAttachment }, invoice?.AddSupplierInvoicesToEInvoices ?? false);
                    foreach (var record in records)
                    {
                        if (!items.Any(r => r.ImageId == record.ImageId && r.SourceType == record.SourceType))
                        {
                            if (record.IncludeWhenDistributed == null && invoice != null)
                                record.IncludeWhenDistributed = invoice.AddAttachementsToEInvoice;
                            items.Add(record);
                        }
                    }
                }
                else if (entity == SoeEntityType.Voucher)
                {
                    items.AddRange(gm.GetDataStorageRecords(base.ActorCompanyId, base.RoleId, recordId, entity, SoeDataStorageRecordType.VoucherFileAttachment, includeInvoiceAttachment: true).ToImagesDTOs(false));
                }
                else if (entity == SoeEntityType.Inventory)
                {
                    items.AddRange(gm.GetDataStorageRecords(base.ActorCompanyId, base.RoleId, recordId, entity, SoeDataStorageRecordType.InventoryFileAttachment).ToImagesDTOs(false));
                }
                else if (entity == SoeEntityType.Supplier)
                {
                    items.AddRange(gm.GetDataStorageRecords(base.ActorCompanyId, base.RoleId, recordId, entity, SoeDataStorageRecordType.SupplierFileAttachment).ToImagesDTOs(false));
                }
                else if (entity == SoeEntityType.Expense)
                {
                    items.AddRange(gm.GetDataStorageRecords(base.ActorCompanyId, base.RoleId, recordId, entity, SoeDataStorageRecordType.Expense).ToImagesDTOs(false));
                }
                else if (entity == SoeEntityType.Customer)
                {
                    items.AddRange(gm.GetDataStorageRecords(base.ActorCompanyId, base.RoleId, recordId, entity, SoeDataStorageRecordType.CustomerFileAttachment).ToImagesDTOs(false));
                }
                else
                {
                    items.AddRange(gm.GetDataStorageRecords(base.ActorCompanyId, base.RoleId, recordId, SoeEntityType.None, SoeDataStorageRecordType.OrderInvoiceFileAttachment).ToImagesDTOs(false));
                }
            }

            return Content(HttpStatusCode.OK, items);
        }

        [HttpGet]
        [Route("{imageId}")]
        public IHttpActionResult GetImage(int imageId)
        {
            var image = grm.GetImage(imageId);
            if (image != null)
            {
                return Content(HttpStatusCode.OK, image.ToDTO(false));
            }
            else
            {
                var record = gm.GetDataStorageRecord(base.ActorCompanyId, imageId);
                return Content(HttpStatusCode.OK, record.ToImagesDTO(true));
            }
        }

        #endregion

    }
}