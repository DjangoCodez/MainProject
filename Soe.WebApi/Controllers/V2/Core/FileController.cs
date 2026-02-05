using Soe.WebApi.Controllers;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using System.Net;
using System.Web.Http;
using SoftOne.Soe.Data;
using SoftOne.Soe.Common.Util;
using System.Net.Http;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static SoftOne.Soe.Util.ZipUtility;

namespace Soe.WebApi.V2.Core
{
    [RoutePrefix("V2/Core/Files")]
    public class FileController : SoeApiController
    {
        #region Variables

        private readonly GeneralManager gm;
        private readonly GraphicsManager grm;
        private readonly InvoiceManager im;
        #endregion


        #region Constructor

        public FileController( GeneralManager gm, GraphicsManager grm, InvoiceManager im)
        {
            this.gm = gm;
            this.grm = grm;
            this.im = im;
        }

        #endregion


        #region Files

        [HttpGet]
        [Route("GetFile/{fileRecordId:int}")]
        public IHttpActionResult GetFileRecord(int fileRecordId)
        {
                var record = gm.GetDataStorageRecord(base.ActorCompanyId, fileRecordId);
                return Content(HttpStatusCode.OK, record.ToFileRecordDTO(true));
        }

        [HttpGet]
        [Route("GetFiles/{entity:int}/{recordId:int}")]
        public IHttpActionResult GetFileRecords(SoeEntityType entity, int recordId)
        {
            List<FileRecordDTO> items = new List<FileRecordDTO>();

            if (entity == SoeEntityType.Employee)
            {
                items.AddRange(gm.GetDataStorageRecords(base.ActorCompanyId, base.RoleId, recordId, entity, loadConfirmationStatus: true, includeDataStorage: true, includeAttestState: true).ToFileRecordDTOs().ToList());
            }
            else
            {
                if (entity == SoeEntityType.Order)
                {
                    // TODO
                }
                else if (entity == SoeEntityType.CustomerInvoice)
                {
                    // TODO
                }
                else if (entity == SoeEntityType.Offer || entity == SoeEntityType.Contract)
                {
                   // TODO
                }
                else if (entity == SoeEntityType.Voucher)
                {
                    items.AddRange(gm.GetDataStorageRecordDTOs(base.ActorCompanyId, base.RoleId, recordId, entity, SoeDataStorageRecordType.VoucherFileAttachment));
                }
                else if (entity == SoeEntityType.Inventory)
                {
                    items.AddRange(gm.GetDataStorageRecordDTOs(base.ActorCompanyId, base.RoleId, recordId, entity, SoeDataStorageRecordType.InventoryFileAttachment));
                }
                else if (entity == SoeEntityType.Supplier)
                {
                    items.AddRange(gm.GetDataStorageRecordDTOs(base.ActorCompanyId, base.RoleId, recordId, entity, SoeDataStorageRecordType.SupplierFileAttachment));
                }
                else if (entity == SoeEntityType.Expense)
                {
                    items.AddRange(gm.GetDataStorageRecordDTOs(base.ActorCompanyId, base.RoleId, recordId, entity, SoeDataStorageRecordType.Expense));
                }
                else if (entity == SoeEntityType.Customer)
                {
                    items.AddRange(gm.GetDataStorageRecordDTOs(base.ActorCompanyId, base.RoleId, recordId, entity, SoeDataStorageRecordType.CustomerFileAttachment));
                }
                else if (entity == SoeEntityType.CustomerCentral)
                {
                    items.AddRange(gm.GetDataStorageRecordDTOsForCustomerCentral(base.ActorCompanyId, base.RoleId, recordId));
                }
                else if (entity == SoeEntityType.TimeProject)
                {
                    items.AddRange(gm.GetDataStorageRecordDTOs(base.ActorCompanyId, base.RoleId, recordId, entity, SoeDataStorageRecordType.ProjectFileAttachment));
                }
                else
                {
                    items.AddRange(gm.GetDataStorageRecordDTOs(base.ActorCompanyId, base.RoleId, recordId, SoeEntityType.None, SoeDataStorageRecordType.OrderInvoiceFileAttachment));
                }
            }

            return Content(HttpStatusCode.OK, items);
        }

        [HttpPost]
        [Route("GetFilesAsZip")]
        public IHttpActionResult GetFilesAsZip(ZipFileRequestDTO model)
        {
            if (model?.Ids == null || !model.Ids.Any())
                return BadRequest();

            ActionResult result = gm.CreateZipFileFromDataStorageRecords(model.Ids, model.prefixName);

            return Ok(result);
        }

        [HttpPost]
        [Route("SendDocumentsAsEmail")]
        public IHttpActionResult SendDocumentsAsEmail(EmailDocumentsRequestDTO model)
        {
            if ((model?.SingleRecipient == null && !model.RecipientUserIds.Any()) || !model.FileRecordIds.Any())
                return BadRequest();

            var result = gm.SendDocumentsAsEmail(base.ActorCompanyId, model);
            return Content(HttpStatusCode.OK, result);
        }

        [HttpPost]
        [Route("{entity:int}/{type:int}/{recordId}")]
        public async Task<IHttpActionResult> UploadInvoiceFile(SoeEntityType entity, SoeEntityImageType type, int recordId, bool extractZip = false)
        {
            var file = await UploadedFileHandler.HandleAsync(Request);
            var actionResults = new List<ActionResult>();
            if (extractZip && IsZipFile(file.Data, file.FileName))
            {
                var extractedFiles = UnzipFilesInZipFile(file.Data);
                foreach (var extractedFile in extractedFiles)
                {
                    var httpFile = new UploadedFileHandler.HttpFile(extractedFile.Key, extractedFile.Value, ImageFormatType.NONE);
                    var actionResult = SaveFileData(entity, type, recordId, httpFile);
                    actionResults.Add(actionResult);
                }
                return Ok(actionResults);
            }
            else
            {
                var actionResult = SaveFileData(entity, type, recordId, file);
                actionResults.Add(actionResult);
                return Ok(actionResults);
            }
        }

        private ActionResult SaveFileData(SoeEntityType entity, SoeEntityImageType type, int recordId, UploadedFileHandler.HttpFile file)
        {
            var record = new DataStorageRecordExtendedDTO
            {
                Data = file.Data,
                Type = GetDataStorageType(file),
                Entity = entity,
                Description = file.FileName,
                RecordNumber = file.FileName,
                RecordId = recordId
            };
            var result = gm.SaveDataStorageRecord(ActorCompanyId, record, false);
            result.StringValue = file.FileName;
            return result;
        }


        [HttpPost]
        [Route("{entity:int}/{type:int}/{recordId:int}/{roles}/{messageGroups}")]
        public async Task<IHttpActionResult> UploadFileWithRolesAndMessageGroups(SoeEntityType entity, SoeEntityImageType type, int recordId, string roles, string messageGroups)
        {
            var file = await UploadedFileHandler.HandleAsync(Request);

            List<int> roleIds = StringUtility.SplitNumericList(roles);
            List<int> messageGroupIds = StringUtility.SplitNumericList(messageGroups);

            DataStorageRecordExtendedDTO record = new DataStorageRecordExtendedDTO
            {
                Data = file.Data,
                Type = SoeDataStorageRecordType.UploadedFile,
                Entity = entity,
                Description = file.FileName,
                RecordNumber = file.FileName,
                RecordId = recordId
            };
            ActionResult result = gm.SaveDataStorageRecord(ActorCompanyId, record, false, roleIds, messageGroupIds);
            result.StringValue = file.FileName;
            return Ok(result);
        }

        [HttpPost]
        [Route("GetArray/")]
        public async Task<IHttpActionResult> GetByteArray()
        {
            var item = await UploadedFileHandler.HandleAsync(Request);
            var result = new ActionResult(item.Data != null);
            result.StringValue = item.FileName;
            result.Value = item.Data;
            return Ok(result);
        }

        private SoeDataStorageRecordType GetDataStorageType(UploadedFileHandler.HttpFile file)
        {
            switch (file.Type)
            {
                case ImageFormatType.JPG:
                case ImageFormatType.PNG:
                    return SoeDataStorageRecordType.InvoiceBitmap;
                case ImageFormatType.PDF:
                    return SoeDataStorageRecordType.InvoicePdf;
                default:
                    return SoeDataStorageRecordType.Unknown;
            }
        }

        [HttpPost]
        [Route("Invoice/{entity:int}")]
        public async Task<IHttpActionResult> UploadInvoiceFileByEntityType(SoeEntityType entity)
        {
            var file = await UploadedFileHandler.HandleAsync(Request);

            var record = new DataStorageRecordExtendedDTO
            {
                Data = file.Data,
                Type = GetDataStorageType(file),
                Entity = entity,
                Description = file.FileName,
                RecordNumber = file.FileName,
                RecordId = 0
            };
            var result = gm.SaveDataStorageRecord(ActorCompanyId, record, false);
            result.StringValue = file.FileName;
            return Ok(result);
        }

        [HttpPost]
        [Route("CheckForDuplicates")]
        public IHttpActionResult CheckForDuplicates(FilesLookupDTO model)
        {
            return Content(HttpStatusCode.OK, gm.GetExistingFiles(base.ActorCompanyId, model.Entity, model.Files));
        }

        [HttpPost]
        [Route("Upload/{entity:int}/{type:int}")]
        public async Task<IHttpActionResult> UploadFile(SoeEntityType entity, SoeEntityImageType type)
        {
            var file = await UploadedFileHandler.HandleAsync(Request);
            if (
                 (file.Type == ImageFormatType.NONE || file.Type == ImageFormatType.PDF) ||
                 (entity == SoeEntityType.Voucher || entity == SoeEntityType.Inventory || entity == SoeEntityType.SupplierInvoice || entity == SoeEntityType.Supplier || entity == SoeEntityType.Expense || entity == SoeEntityType.Customer || entity == SoeEntityType.TimeProject) ||
                 (entity == SoeEntityType.Offer || entity == SoeEntityType.Order || entity == SoeEntityType.Contract || entity == SoeEntityType.CustomerInvoice) ||
                 (entity == SoeEntityType.Employee && type == SoeEntityImageType.EmployeePortrait)
               )
            {
                var recordEntity = entity;
                var recordType = SoeDataStorageRecordType.OrderInvoiceFileAttachment; //very strange!!
                switch (entity)
                {
                    case SoeEntityType.Voucher:
                        recordType = SoeDataStorageRecordType.VoucherFileAttachment;
                        break;
                    case SoeEntityType.Inventory:
                        recordType = SoeDataStorageRecordType.InventoryFileAttachment;
                        break;
                    case SoeEntityType.Employee:
                        recordType = SoeDataStorageRecordType.EmployeePortrait;
                        break;
                    case SoeEntityType.Supplier:
                        recordType = SoeDataStorageRecordType.SupplierFileAttachment;
                        break;
                    case SoeEntityType.Expense:
                        recordType = SoeDataStorageRecordType.Expense;
                        break;
                    case SoeEntityType.Customer:
                        recordType = SoeDataStorageRecordType.CustomerFileAttachment;
                        break;
                    case SoeEntityType.TimeProject:
                        recordType = SoeDataStorageRecordType.ProjectFileAttachment;
                        break;
                    default:
                        recordEntity = SoeEntityType.None;
                        break;
                }

                var record = new DataStorageRecordExtendedDTO
                {
                    Data = file.Data,
                    Type = recordType,
                    Entity = recordEntity,
                    Description = file.FileName,
                    RecordNumber = file.FileName,
                    RecordId = 0
                };

                var result = gm.SaveDataStorageRecord(ActorCompanyId, record, false);
                result.StringValue = file.FileName;

                if (entity == SoeEntityType.Employee && type == SoeEntityImageType.EmployeePortrait && result.IntegerValue > 0)
                {
                    result.StringValue = "image";
                    result.Value = gm.GetDataStorageRecord(ActorCompanyId, result.IntegerValue).ToImagesDTO(true);
                }

                return Ok(result);
            }

            var imagesDto = new ImagesDTO
            {
                Type = type,
                FileName = file.FileName,
                Description = file.FileName,
                Image = file.Data,
                FormatType = file.Type
            };
            return Ok(grm.SaveImageDTO(imagesDto, entity, ActorCompanyId));
        }

        [HttpPost]
        [Route("Upload/{entity:int}/{type:int}/{recordId:int}")]
        public async Task<IHttpActionResult> UploadFileForRecord(SoeEntityType entity, SoeDataStorageRecordType type, int recordId)
        {
            var file = await UploadedFileHandler.HandleAsync(Request);

            var record = new DataStorageRecordExtendedDTO
            {
                Data = file.Data,
                Type = type,
                Entity = entity,
                Description = file.FileName,
                RecordNumber = file.FileName,
                RecordId = recordId
            };
            var result = gm.SaveDataStorageRecord(ActorCompanyId, record, false);
            result.StringValue = file.FileName;
            return Ok(result);
        }

        [HttpPost]
        [Route("Replace/{entity:int}/{type:int}/{dataStorageId:int}")]
        public async Task<IHttpActionResult> UpdateDataStorageFile(SoeEntityType entity, SoeDataStorageRecordType type, int dataStorageId)
        {
            var file = await UploadedFileHandler.HandleAsync(Request);
            var record = new DataStorageRecordExtendedDTO
            {
                Data = file.Data,
                Type = type,
                Entity = entity,
                Description = file.FileName,
                FileName = file.FileName,
            };

            return Ok(gm.UpdateDataStorageFiles(record, dataStorageId));
        }

        [HttpPost]
        [Route("Update/")]
        public IHttpActionResult updateFileRecord(FileRecordDTO fileRecord)
        {
            return Content(HttpStatusCode.OK, gm.UpdateDataStorageByRecord(ActorCompanyId, fileRecord.FileRecordId, fileRecord.RecordId, fileRecord.Description, fileRecord.FileName));
        }

        [HttpDelete]
        [Route("Delete/{dataStorageRecordId:int}")]
        public IHttpActionResult DeleteFileRecord(int dataStorageRecordId)
        {
            return Content(HttpStatusCode.OK, gm.DeleteDataStorageRecord(ActorCompanyId, dataStorageRecordId));
        }

        public class UploadedFileHandler
        {
            private static readonly IDictionary<string, ImageFormatType> Types = new Dictionary<string, ImageFormatType>
            {
                { "image/jpg", ImageFormatType.JPG },
                { "image/jpeg", ImageFormatType.JPG },
                { "image/png", ImageFormatType.PNG },
                { "application/pdf", ImageFormatType.PDF }
            };

            public static async Task<HttpFile> HandleAsync(HttpRequestMessage message)
            {
                if (!message.Content.IsMimeMultipartContent())
                    throw new HttpResponseException(message.CreateResponse(HttpStatusCode.NotAcceptable, "This request is not properly formatted"));

                var multipart = await message.Content.ReadAsMultipartAsync();
                var uploadedFile = multipart.Contents.SingleOrDefault();
                if (uploadedFile == null)
                    throw new HttpResponseException(message.CreateResponse(HttpStatusCode.NotAcceptable, "This content of the file is null"));

                return new HttpFile(
                    uploadedFile.Headers.ContentDisposition.FileName.Trim('"'),
                    await uploadedFile.ReadAsByteArrayAsync(),
                    GetImageType(uploadedFile.Headers.ContentType.MediaType)
                );
            }

            public static async Task<byte[]> HandleAsyncGetByteArray(HttpRequestMessage message)
            {
                if (!message.Content.IsMimeMultipartContent())
                    throw new HttpResponseException(message.CreateResponse(HttpStatusCode.NotAcceptable, "This request is not properly formatted"));

                var multipart = await message.Content.ReadAsMultipartAsync();
                var uploadedFile = multipart.Contents.SingleOrDefault();
                if (uploadedFile == null)
                    throw new HttpResponseException(message.CreateResponse(HttpStatusCode.NotAcceptable, "This content of the file is null"));

                return await uploadedFile.ReadAsByteArrayAsync();
            }

            private static ImageFormatType GetImageType(string mediaType)
            {
                return Types.ContainsKey(mediaType) ? Types[mediaType] : ImageFormatType.NONE;
            }

            public class HttpFile
            {
                public string FileName { get; private set; }
                public byte[] Data { get; private set; }
                public ImageFormatType Type { get; private set; }

                public HttpFile(string fileName, byte[] data, ImageFormatType type)
                {
                    FileName = fileName;
                    Data = data;
                    Type = type;
                }
            }
        }

        #endregion

    }
}