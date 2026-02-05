using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace SoftOne.Soe.Business.Core
{
    public class GraphicsManager : ManagerBase
    {
        #region Variables

        // Create a logger for use in this class
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #endregion

        #region Constructor

        public GraphicsManager(ParameterObject parameterObject) : base(parameterObject) { }

        #endregion

        #region Comp Images

        #region Public methods

        #region ImagesDTO

        public ActionResult UpdateImages(CompEntities entities, IEnumerable<FileUploadDTO> images, int recordId)
        {
            try
            {
                foreach (var file in images)
                {
                    var image = entities.Images.SingleOrDefault(f => f.ImageId == file.ImageId && f.ActorCompanyId == ActorCompanyId);
                    if (image == null)
                        continue;

                    if (file.IsDeleted)
                    {
                        var result = DeleteImage(entities, image, saveChanges: false);
                        if (!result.Success)
                            return result;
                    }
                    else
                    {
                        image.RecordId = recordId;
                        image.Description = file.Description;
                    }
                }
                return new ActionResult(true);
            }
            catch (Exception e)
            {
                LogError(e, log);
                return new ActionResult(e);
            }
        }

        public ActionResult SaveImageDTO(ImagesDTO image, SoeEntityType type, int actorCompanyId)
        {
            using (var entities = new CompEntities())
            {
                var images = CreateImage(actorCompanyId, image.Type, type, 0, image.FormatType, image.Image, image.Description);
                entities.Images.AddObject(images);
                entities.SaveChanges();
                return new ActionResult
                {
                    Value = images.ToDTO(true),
                    StringValue = "image",
                    IntegerValue = images.ImageId
                };
            }
        }

        public ActionResult SaveImagesDTO(SaveImagesDTO imagesInput)
        {
            ActionResult result = new ActionResult();
            bool saveChanges = false;

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    #region Deleted images

                    if (imagesInput.DeletedImages != null && imagesInput.DeletedImages.Count > 0)
                    {
                        foreach (int imageId in imagesInput.DeletedImages)
                        {
                            Images originalImage = GetImage(entities, imageId);
                            if (originalImage != null)
                            {
                                result = DeleteImage(entities, originalImage, saveChanges: false);
                                if (!result.Success)
                                    return result;

                                saveChanges = true;
                            }
                        }
                    }

                    #endregion

                    #region Updated images (description)

                    if (imagesInput.UpdatedDescriptions != null && imagesInput.UpdatedDescriptions.Count > 0)
                    {
                        foreach (KeyValuePair<int, string> kvp in imagesInput.UpdatedDescriptions)
                        {
                            Images originalImage = GetImage(entities, kvp.Key);
                            if (originalImage != null)
                            {
                                originalImage.Description = kvp.Value;
                                saveChanges = true;
                            }
                        }
                    }

                    #endregion

                    #region New images

                    if (imagesInput.NewImages != null && imagesInput.NewImages.Count > 0)
                    {
                        foreach (ImagesDTO image in imagesInput.NewImages)
                        {
                            var images = CreateImage(imagesInput.ActorCompanyId, imagesInput.Type, imagesInput.Entity, imagesInput.RecordId, image.FormatType, image.Image, image.Description);
                            entities.Images.AddObject(images);
                            saveChanges = true;
                        }
                    }

                    #endregion

                    if (saveChanges)
                    {
                        SaveChanges(entities);
                    }
                }
                catch (Exception ex)
                {
                    result.Success = false;
                    result.ErrorMessage = ex.Message;

                    base.LogError(ex, this.log);
                }
            }

            return result;
        }

        #endregion

        #region Images

        public Images GetImage(int imageId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Images.NoTracking();
            return GetImage(entities, imageId);
        }

        public Images GetImage(CompEntities entities, int imageId)
        {
            return (from i in entities.Images
                    where i.ImageId == imageId
                    select i).FirstOrDefault();
        }

        public Images GetImage(int actorCompanyId, SoeEntityImageType imageType, SoeEntityType entity, int recordId, bool createThumbnailIfNotExists)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Images.NoTracking();
            return GetImage(entities, actorCompanyId, imageType, entity, recordId, createThumbnailIfNotExists);
        }

        public Images GetImage(CompEntities entities, int actorCompanyId, SoeEntityImageType imageType, SoeEntityType entity, int recordId, bool createThumbnailIfNotExists)
        {
            Images image = (from i in entities.Images
                            where i.ActorCompanyId == actorCompanyId &&
                                  i.Type == (int)imageType &&
                                  i.Entity == (int)entity &&
                                  i.RecordId == recordId
                            select i).FirstOrDefault();

            if (createThumbnailIfNotExists && image != null && image.Thumbnail == null && image.Image != null)
                image.Thumbnail = CompressImage(image.Image, (ImageFormatType)image.FormatType, 160, 30);

            return image;
        }

        public IEnumerable<Images> GetImages(int actorCompanyId, SoeEntityImageType imageType, SoeEntityType entity, int recordId)
        {
            return this.GetImages(actorCompanyId, entity, recordId).Where(i => i.Type == (int)imageType);
        }

        public IEnumerable<Images> GetImages(CompEntities entities, int actorCompanyId, SoeEntityImageType imageType, SoeEntityType entity, int recordId)
        {
            return this.GetImages(entities, actorCompanyId, entity, recordId).Where(i => i.Type == (int)imageType);
        }

        public IEnumerable<Images> GetImages(int actorCompanyId, SoeEntityType entity, int recordId, params SoeEntityImageType[] imageTypes)
        {
            var intArray = imageTypes.Cast<int>();
            return this.GetImages(actorCompanyId, entity, recordId).Where(i => intArray.Contains(i.Type));
        }

        private List<Images> GetImages(int actorCompanyId, SoeEntityType entity, int recordId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Images.NoTracking();
            return GetImages(entities, actorCompanyId, entity, recordId).ToList();
        }

        private IEnumerable<Images> GetImages(CompEntities entities, int actorCompanyId, SoeEntityType entity, int recordId)
        {
            return (from i in entities.Images
                    where i.ActorCompanyId == actorCompanyId &&
                          i.Entity == (int)entity &&
                          i.RecordId == recordId
                    orderby i.Description
                    select i);
        }

        public List<int> GetImagesIds(int actorCompanyId, SoeEntityImageType imageType, SoeEntityType entity, int recordId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Images.NoTracking();
            return (from i in entities.Images
                    where i.ActorCompanyId == actorCompanyId &&
                          i.Entity == (int)entity &&
                          i.RecordId == recordId &&
                          i.Type == (int)imageType
                    orderby i.Description
                    select i.ImageId).ToList();
        }

        public List<ImagesDTO> GetImagesFromOrderRows(int actorCompanyId, int invoiceId, bool setData = false, string ediTerm = null, string dataStorageTerm = null, bool canDelete = true, int? parentInvoiceId = null, bool addToDistribution = false, bool hasSupplierInvoicesPermission = true)
        {
            var images = new List<ImagesDTO>();
            using (var entities = new CompEntities())
            {
                try
                {
                    List<int> idsWithEdi = (from r in entities.CustomerInvoiceRow
                                            where r.InvoiceId == invoiceId &&
                                            r.IncludeSupplierInvoiceImage != null && r.IncludeSupplierInvoiceImage == true &&
                                            r.State == (int)SoeEntityState.Active &&
                                            r.SupplierInvoiceId != null &&
                                            (r.EdiEntryId.HasValue && r.EdiEntryId.Value > 0)
                                            select r.SupplierInvoiceId.Value).Distinct().ToList();

                    foreach (int id in idsWithEdi)
                    {
                        var image = GetImageFromOrderRows(entities, actorCompanyId, invoiceId, id, setData, ediTerm, canDelete, parentInvoiceId, addToDistribution);
                        if (image != null)
                            images.Add(image);
                    }

                    if (hasSupplierInvoicesPermission)
                    {
                        List<int> idsWithoutEdi = (from r in entities.CustomerInvoiceRow
                                                   where r.InvoiceId == invoiceId &&
                                                   r.IncludeSupplierInvoiceImage != null && r.IncludeSupplierInvoiceImage == true &&
                                                   r.State == (int)SoeEntityState.Active &&
                                                   r.SupplierInvoiceId != null &&
                                                   !r.EdiEntryId.HasValue
                                                   select r.SupplierInvoiceId.Value).Distinct().ToList();

                        foreach (int id in idsWithoutEdi)
                        {
                            var image = GetImageFromOrderRows(entities, actorCompanyId, invoiceId, id, setData, dataStorageTerm, canDelete, parentInvoiceId, addToDistribution);
                            if (image != null)
                                images.Add(image);
                        }
                    }
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                }
            }
            return images;
        }

        public ImagesDTO GetImageFromOrderRows(CompEntities entities, int actorCompanyId, int invoiceId, int id, bool setData = false, string term = null, bool canDelete = true, int? parentInvoiceId = null, bool addToDistribution = false, bool isEdi = false)
        {
            ImagesDTO dto = null;
            var image = SupplierInvoiceManager.GetSupplierInvoiceImage(entities, actorCompanyId, id, false, null, null, true);
            if (image != null)
            {
                image.ConnectedTypeName = term;

                dto = new ImagesDTO { ImageId = image.Id != 0 ? image.Id : id, Image = setData ? image.Image : null, FormatType = image.Format, FileName = image.Filename, Description = image.Description, ConnectedTypeName = image.ConnectedTypeName, Type = SoeEntityImageType.SupplierInvoice, CanDelete = canDelete, DataStorageRecordType = image.ImageFormatType, SourceType = image.SourceType };
                if (image.InvoiceAttachments != null)
                {
                    // Get attachment connected to order
                    var attachment = image.InvoiceAttachments.FirstOrDefault(a => parentInvoiceId.HasValue ? a.InvoiceId == parentInvoiceId.Value : a.InvoiceId == invoiceId);
                    if (attachment != null)
                    {
                        dto.InvoiceAttachmentId = attachment.InvoiceAttachmentId;
                        dto.IncludeWhenTransfered = attachment.AddAttachmentsOnTransfer;
                        dto.IncludeWhenDistributed = attachment.AddAttachmentsOnEInvoice;
                        dto.LastSentDate = attachment.LastDistributedDate;
                    }
                    else
                    {
                        dto.IncludeWhenTransfered = true;
                        dto.IncludeWhenDistributed = addToDistribution;

                        var result = InvoiceAttachmentManager.AddInvoiceAttachment(entities, invoiceId, dto.ImageId, dto.SourceType, InvoiceAttachmentConnectType.SupplierInvoice, dto.IncludeWhenDistributed.Value, dto.IncludeWhenTransfered.Value);
                        if (result.Success)
                            dto.InvoiceAttachmentId = result.IntegerValue;
                    }
                }
                else
                {
                    dto.IncludeWhenTransfered = true;
                    dto.IncludeWhenDistributed = addToDistribution;

                    var result = InvoiceAttachmentManager.AddInvoiceAttachment(entities, invoiceId, dto.ImageId, dto.SourceType, InvoiceAttachmentConnectType.SupplierInvoice, dto.IncludeWhenDistributed.Value, dto.IncludeWhenTransfered.Value);
                    if (result.Success)
                        dto.InvoiceAttachmentId = result.IntegerValue;
                }
            }
            return dto;
        }

        public List<ImagesDTO> GetImagesFromLinkToProject(int actorCompanyId, int invoiceId, int projectId, bool setData = false, string overrideDesc = null, bool canDelete = true, int? parentInvoiceId = null, bool addToDistribution = false)
        {
            List<ImagesDTO> images = new List<ImagesDTO>();
            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    var ids = (from tct in entities.TimeCodeTransaction
                               where tct.SupplierInvoiceId.HasValue &&
                               (tct.CustomerInvoiceId.HasValue && tct.CustomerInvoiceId == invoiceId) &&
                               tct.ProjectId == projectId &&
                               tct.State == (int)SoeEntityState.Active &&
                               tct.IncludeSupplierInvoiceImage
                               select tct.SupplierInvoiceId.Value).Distinct().ToList();

                    foreach (int id in ids)
                    {
                        var image = SupplierInvoiceManager.GetSupplierInvoiceImage(entities, actorCompanyId, id, true, null, null, true);
                        if (image != null)
                        {
                            var dto = new ImagesDTO { ImageId = image.Id != 0 ? image.Id : id, Image = setData ? image.Image : null, FormatType = image.Format, FileName = image.Filename, Description = image.Description, ConnectedTypeName = (overrideDesc.HasValue() ? overrideDesc : String.Empty), Type = SoeEntityImageType.SupplierInvoice, CanDelete = canDelete, DataStorageRecordType = image.ImageFormatType, SourceType = image.SourceType };
                            if (image.InvoiceAttachments != null)
                            {
                                // Get attachment connected to order
                                var attachment = image.InvoiceAttachments.FirstOrDefault(a => parentInvoiceId.HasValue ? a.InvoiceId == parentInvoiceId.Value : a.InvoiceId == invoiceId);
                                if (attachment != null)
                                {
                                    dto.InvoiceAttachmentId = attachment.InvoiceAttachmentId;
                                    dto.IncludeWhenTransfered = attachment.AddAttachmentsOnTransfer;
                                    dto.IncludeWhenDistributed = attachment.AddAttachmentsOnEInvoice;
                                    dto.LastSentDate = attachment.LastDistributedDate;
                                }
                                else
                                {
                                    dto.IncludeWhenTransfered = true;
                                    dto.IncludeWhenDistributed = addToDistribution;

                                    var result = InvoiceAttachmentManager.AddInvoiceAttachment(entities, invoiceId, dto.ImageId, dto.SourceType, InvoiceAttachmentConnectType.SupplierInvoice, dto.IncludeWhenDistributed.Value, dto.IncludeWhenTransfered.Value);
                                    if (result.Success)
                                        dto.InvoiceAttachmentId = result.IntegerValue;
                                }
                            }
                            else
                            {
                                dto.IncludeWhenTransfered = true;
                                dto.IncludeWhenDistributed = addToDistribution;

                                var result = InvoiceAttachmentManager.AddInvoiceAttachment(entities, invoiceId, dto.ImageId, dto.SourceType, InvoiceAttachmentConnectType.SupplierInvoice, dto.IncludeWhenDistributed.Value, dto.IncludeWhenTransfered.Value);
                                if (result.Success)
                                    dto.InvoiceAttachmentId = result.IntegerValue;
                            }
                            images.Add(dto);
                        }
                    }
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                }
            }
            return images;
        }

        public string GetImageFilePath(ImagesDTO imageDTO, int actorCompanyID, bool saveToDisk = true)
        {
            string pathPhysical = "";
            if (imageDTO != null)
            {
                string dirPhysical = ConfigSettings.SOE_SERVER_DIR_TEMP_LOGO_PHYSICAL;
                pathPhysical = dirPhysical + +imageDTO.ImageId + "_" + ActorCompanyId + "_" + "signature.jpeg";

                if (saveToDisk)
                {
                    if (File.Exists(pathPhysical))
                        File.Delete(pathPhysical);

                    var file = new FileStream(pathPhysical, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.Read);
                    file.Write(imageDTO.Image, 0, imageDTO.Image.Length);
                    file.Dispose();
                }
            }
            return pathPhysical;
        }

        public bool HasImage(int actorCompanyId, SoeEntityImageType imageType, SoeEntityType entity, int recordId)
        {
			using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
			return (from i in entitiesReadOnly.Images
                    where i.ActorCompanyId == actorCompanyId &&
                    i.Type == (int)imageType &&
                    i.Entity == (int)entity &&
                    i.RecordId == recordId &&
                    i.Image != null
                    select i).Any();
        }

        #endregion

        #endregion

        #region Private methods

        private Images CreateImage(int actorCompanyId, SoeEntityImageType imageType, SoeEntityType entity, int recordId, ImageFormatType formatType, byte[] image, string description)
        {
            // Reduce file size if neccessary
            image = LimitImageSize(image, imageType, formatType);

            Images images = new Images()
            {
                ActorCompanyId = actorCompanyId,
                Type = (int)imageType,
                Entity = (int)entity,
                RecordId = recordId,
                FormatType = (int)formatType,
                Thumbnail = CompressImage(image, formatType, 160, 30),
                Image = image,
                Description = description
            };

            return images;
        }

        private ActionResult DeleteImage(CompEntities entities, Images originalImage, bool saveChanges = true)
        {
            entities.DeleteObject(originalImage);
            if (saveChanges)
                return SaveChanges(entities);

            return new ActionResult();
        }

        #endregion

        #region Help Methods

        private byte[] LimitImageSize(byte[] image, SoeEntityImageType imageType, ImageFormatType formatType)
        {
            if (image != null)
            {
                if (imageType == SoeEntityImageType.OrderInvoice || imageType == SoeEntityImageType.XEMailAttachment)
                {
                    if (image.Length > 200000)
                        image = CompressImage(image, formatType, 1920, 30);
                }
                else
                {
                    if (image.Length > 50000)
                        image = CompressImage(image, formatType, 800, 30);
                }
            }

            return image;
        }

        public byte[] CompressImage(byte[] image, ImageFormatType formatType, double width, long quality)
        {
            try
            {
                using (var imageStream = Image.FromStream(new MemoryStream(image)))
                {
                    var size = new Size((int)width, (int)(imageStream.Height * (width / imageStream.Width)));
                    using (var bitmap = new Bitmap(imageStream, size))
                    {
                        var parameters = new EncoderParameters(1);
                        parameters.Param[0] = new EncoderParameter(Encoder.Quality, value: quality);
                        var codecInfo = GetCodecInfo(formatType);
                        using (var memStream = new MemoryStream())
                        {
                            bitmap.Save(memStream, codecInfo, parameters);
                            return memStream.ToArray();
                        }
                    }
                }
            }
            catch (Exception exp)
            {
                base.LogError(exp, this.log);
            }

            return null;
        }

        public byte[] ConvertToJpeg(byte[] image, long quality, int maxWidth)
        {
            ImageFormatType formatType = ImageFormatType.JPG;
            try
            {
                using (var imageStream = Image.FromStream(new MemoryStream(image)))
                {
                    double width = imageStream.Width;

                    if (width > maxWidth)
                        width = maxWidth;

                    // Calculate new size while maintaining aspect ratio
                    var size = new Size((int)width, (int)(imageStream.Height * (width / imageStream.Width)));

                    using (var bitmap = new Bitmap(imageStream, size))
                    {
                        var parameters = new EncoderParameters(1);
                        parameters.Param[0] = new EncoderParameter(Encoder.Quality, quality);
                        var jpegCodecInfo = GetCodecInfo(formatType);

                        using (var memStream = new MemoryStream())
                        {
                            bitmap.Save(memStream, jpegCodecInfo, parameters);
                            return memStream.ToArray();
                        }
                    }
                }
            }
            catch (Exception exp)
            {
                // Log error if necessary
                // base.LogError(exp, this.log);
                Console.WriteLine(exp.Message); // Replace with actual logging if needed
            }

            return null;
        }

        private ImageCodecInfo GetJpegCodecInfo()
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();
            foreach (var codec in codecs)
            {
                if (codec.MimeType == "image/jpeg")
                {
                    return codec;
                }
            }
            return null;
        }

        private ImageCodecInfo GetCodecInfo(ImageFormatType formatType)
        {
            String mimeType = ImageUtil.GetMimeType(formatType);

            foreach (ImageCodecInfo encoder in ImageCodecInfo.GetImageEncoders())
            {
                if (encoder.MimeType == mimeType)
                    return encoder;
            }

            return null;
        }

        #endregion

        #endregion

        #region Map

        public IEnumerable<MapLocation> GetMapLocations(int actorCompanyId, MapLocationType locationType, SoeEntityType entity, bool onlyLatest)
        {
			using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
			entitiesReadOnly.MapLocation.NoTracking();

            if (onlyLatest)
            {
                return (from m in entitiesReadOnly.MapLocation
                        where m.ActorCompanyId == actorCompanyId &&
                        m.Type == (int)locationType &&
                        m.Entity == (int)entity
                        group m by m.RecordId into g
                        let MaxTimeStamp = g.Max(x => x.TimeStamp)
                        from m in g
                        where m.TimeStamp == MaxTimeStamp
                        select m);
            }
            else
            {
                return (from m in entitiesReadOnly.MapLocation
                        where m.ActorCompanyId == actorCompanyId &&
                        m.Type == (int)locationType &&
                        m.Entity == (int)entity
                        select m);
            }
        }

        public ActionResult SaveMapLocation(int actorCompanyId, int recordId, MapLocationType type, SoeEntityType entity, decimal longitude, decimal latitude, string description, DateTime timeStamp)
        {
            if (longitude == 0 && latitude == 0)
                return new ActionResult(false);

            using (CompEntities entities = new CompEntities())
            {
                var maplocation = new MapLocation
                {
                    ActorCompanyId = actorCompanyId,
                    RecordId = recordId,
                    Type = (int)type,
                    Entity = (int)entity,
                    Longitude = longitude,
                    Latitude = latitude,
                    Description = description,
                    TimeStamp = timeStamp
                };
                return AddEntityItem(entities, maplocation, "MapLocation");
            }
        }

        #endregion
    }
}
