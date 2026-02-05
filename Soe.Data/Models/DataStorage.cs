using SoftOne.Communicator.Shared.DTO;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SoftOne.Soe.Data
{
    public partial class DataStorage : ICreatedModified, IState
    {
        public string TimePeriodName
        {
            get
            {
                return this.TimePeriod?.Name ?? string.Empty;
            }
        }
        public DateTime TimePeriodStartDate
        {
            get
            {
                return this.TimePeriod?.StartDate ?? CalendarUtility.DATETIME_DEFAULT;
            }
        }
        public string TimePeriodStartDateString
        {
            get
            {
                return TimePeriodStartDate.ToString(CalendarUtility.SHORTDATEMASK);
            }
        }
        public DateTime TimePeriodStopDate
        {
            get
            {
                return this.TimePeriod?.StopDate ?? CalendarUtility.DATETIME_DEFAULT;
            }
        }
        public string TimePeriodStopDateString
        {
            get
            {
                return TimePeriodStopDate.ToString(CalendarUtility.SHORTDATEMASK);
            }
        }

        public string DownloadURL { get; set; }

    }

    public partial class DataStorageRecord
    {
        public bool NeedsConfirmation { get; set; }
        public bool Confirmed { get; set; }
        public DateTime? ConfirmedDate { get; set; }
    }

    public static partial class EntityExtensions
    {
        #region DataStorage

        public static DataStorageDTO ToDTO(this DataStorage e)
        {
            if (e == null)
                return null;

            DataStorageDTO dto = new DataStorageDTO()
            {
                DataStorageId = e.DataStorageId,
                ActorCompanyId = e.ActorCompanyId,
                ParentDataStorageId = e.ParentDataStorageId,
                EmployeeId = e.EmployeeId,
                TimePeriodId = e.TimePeriodId,
                UserId = e.UserId,
                Type = (SoeDataStorageRecordType)e.Type,
                Description = e.Description,
                Folder = StringUtility.NullToEmpty(e.Folder),
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State,
                OriginType = (SoeDataStorageOriginType)e.OriginType,
                ValidFrom = e.ValidFrom,
                ValidTo = e.ValidTo,
                NeedsConfirmation = e.NeedsConfirmation,
                Xml = e.XML,
                DownloadURL = e.DownloadURL,
            };

            // Extensions
            dto.ExportDate = e.Created;

            if (!e.DataStorageRecord.IsNullOrEmpty())
            {
                StringBuilder sb = new StringBuilder();
                foreach (DataStorageRecord record in e.DataStorageRecord)
                {
                    if (!String.IsNullOrEmpty(record.RecordNumber))
                        sb.Append(record.RecordNumber + ",");
                }
                dto.Information = sb.ToString();

                if (!String.IsNullOrEmpty(dto.Information) && dto.Information.EndsWith(","))
                    dto.Information = dto.Information.Substring(0, dto.Information.Length - 1);
            }

            return dto;
        }

        public static IEnumerable<DataStorageDTO> ToDTOs(this IEnumerable<DataStorage> l)
        {
            var dtos = new List<DataStorageDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static IEnumerable<MessageAttachmentDTO> ToMessageAttachmentDTOs(this IEnumerable<DataStorageRecord> l, bool includeData)
        {
            return l.Select(s => s.ToMessageAttachmentDTO(includeData)).ToList();
        }

        public static MessageAttachmentDTO ToMessageAttachmentDTO(this DataStorageRecord l, bool includeData)
        {
            if (!l.DataStorageReference.IsLoaded)
            {
                l.DataStorageReference.Load();
                DataProjectLogCollector.LogLoadedEntityInExtension("Datastorage.cs e.DataStorageReference");
            }

            var dto = new MessageAttachmentDTO()
            {
                MessageAttachmentId = l.DataStorageRecordId,
                Name = l.DataStorage.Description,
            };

            if (includeData)
            {
                dto.Data = l.DataStorage.Data;
                dto.Filesize = l.DataStorage.FileSize != null ? (long)l.DataStorage.FileSize : 0;
            }

            return dto;
        }

        public static DataStorageRecordDTO ToDTO(this DataStorageRecord e)
        {
            if (e == null)
                return null;

            DataStorageRecordDTO dto = new DataStorageRecordDTO()
            {
                DataStorageRecordId = e.DataStorageRecordId,
                RecordId = e.RecordId,
                Entity = (SoeEntityType)e.Entity,
                AttestStateId = e.AttestStateId,
                CurrentAttestUsers = e.CurrentAttestUsers,
                AttestStatus = (TermGroup_DataStorageRecordAttestStatus)e.AttestStatus,
                Type = (SoeDataStorageRecordType)e.Type,
                Data = e.DataStorage?.Data
            };

            // Extensions
            if (e.AttestState != null)
            {
                dto.AttestStateName = e.AttestState.Name;
                dto.AttestStateColor = e.AttestState.Color;
            }

            // Permissions
            if (!e.DataStorageRecordRolePermission.IsNullOrEmpty())
                dto.RoleIds = (from p in e.DataStorageRecordRolePermission where p.State == (int)SoeEntityState.Active select p.RoleId).ToList();

            return dto;
        }

        public static List<DataStorageRecordDTO> ToDTOs(this IEnumerable<DataStorageRecord> l)
        {
            List<DataStorageRecordDTO> dtos = new List<DataStorageRecordDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }


        public static DataStorageRecipientDTO ToDTO(this DataStorageRecipient e)
        {
            if (e == null)
                return null;

            return new DataStorageRecipientDTO()
            {
                DataStorageRecipientId = e.DataStorageRecipientId,
                DataStorageId = e.DataStorageId,
                UserId = e.UserId,
                ReadDate = e.ReadDate,
                ConfirmedDate = e.ConfirmedDate,
                UserName = e.User?.LoginName,
                State = (SoeEntityState)e.State
            };
        }

        public static List<DataStorageRecipientDTO> ToDTOs(this IEnumerable<DataStorageRecipient> l)
        {
            List<DataStorageRecipientDTO> dtos = new List<DataStorageRecipientDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static ImagesDTO ToImagesDTO(this DataStorageRecord e, bool includeFile, string connectedTypeName = null, bool canDelete = true, bool fileNameAsDescription = true, bool filterAttachmentsOnRecordId = false, int? parentInvoiceId = null)
        {
            if (!e.DataStorageReference.IsLoaded)
            {
                e.DataStorageReference.Load();
                DataProjectLogCollector.LogLoadedEntityInExtension("Datastorage.cs e.DataStorageReference");
            }

            ImagesDTO dto = new ImagesDTO
            {
                Description = string.IsNullOrEmpty(e.DataStorage.Description) /*&& fileNameAsDescription*/ ? e.DataStorage.FileName : e.DataStorage.Description,
                ConnectedTypeName = connectedTypeName.HasValue() ? connectedTypeName : string.Empty,
                FileName = e.DataStorage.FileName?.RemoveNewLine(),
                FormatType = ImageFormatType.NONE,
                ImageId = e.DataStorageRecordId,
                Created = e.DataStorage.Created,
                NeedsConfirmation = e.NeedsConfirmation,
                Confirmed = e.Confirmed,
                ConfirmedDate = e.ConfirmedDate,
                CanDelete = canDelete,
                DataStorageRecordType = (SoeDataStorageRecordType)e.Type,
                SourceType = InvoiceAttachmentSourceType.DataStorage,
                AttestStateId = e.AttestStateId,
                AttestStateName = e.AttestState != null ? e.AttestState.Name : string.Empty,
                AttestStateColor = e.AttestState != null ? e.AttestState.Color : string.Empty,
                CurrentAttestUsers = e.CurrentAttestUsers,
                AttestStatus = (TermGroup_DataStorageRecordAttestStatus)e.AttestStatus
            };

            if (dto.FileName != null && dto.FileName.EndsWith(".jpg", true, null))
            {
                dto.FormatType = ImageFormatType.JPG;
            }

            //DataStoregeType to ImageType
            switch (dto.DataStorageRecordType)
            {
                case SoeDataStorageRecordType.ChecklistHeadRecordSignature:
                    dto.Type = SoeEntityImageType.ChecklistHeadRecordSignature;
                    break;
                case SoeDataStorageRecordType.ChecklistHeadRecordSignatureExecutor:
                    dto.Type = SoeEntityImageType.ChecklistHeadRecordSignatureExecutor;
                    break;
                case SoeDataStorageRecordType.OrderInvoiceSignature:
                    dto.Type = SoeEntityImageType.OrderInvoiceSignature;
                    break;
                case SoeDataStorageRecordType.EmployeePortrait:
                    dto.Type = SoeEntityImageType.EmployeePortrait;
                    break;
                case SoeDataStorageRecordType.OrderInvoiceFileAttachment:
                    dto.Type = SoeEntityImageType.OrderInvoice;
                    break;
            }

            if (e.InvoiceAttachment != null)
            {
                InvoiceAttachment attachment;
                if (parentInvoiceId.HasValue)
                    attachment = e.InvoiceAttachment.FirstOrDefault(a => a.InvoiceId == parentInvoiceId);
                else
                    attachment = filterAttachmentsOnRecordId ? e.InvoiceAttachment.FirstOrDefault(a => a.InvoiceId == e.RecordId) : e.InvoiceAttachment.FirstOrDefault();

                if (attachment != null)
                {
                    dto.InvoiceAttachmentId = attachment.InvoiceAttachmentId;
                    dto.IncludeWhenTransfered = attachment.AddAttachmentsOnTransfer;
                    dto.IncludeWhenDistributed = attachment.AddAttachmentsOnEInvoice;
                }
                else
                {
                    dto.IncludeWhenTransfered = true;
                }
            }

            if (includeFile)
                dto.Image = e.DataStorage.Data;

            return dto;
        }

        public static IEnumerable<ImagesDTO> ToImagesDTOs(this IEnumerable<DataStorageRecord> e, bool includeFile, string connectedTypeName = null, bool canDelete = true, bool fileNameAsDescription = true, bool filterAttachmentsOnRecordId = false, int? parentInvoiceId = null)
        {
            return e.Select(s => s.ToImagesDTO(includeFile, connectedTypeName, canDelete, fileNameAsDescription, filterAttachmentsOnRecordId, parentInvoiceId)).ToList();
        }

        public static FileRecordDTO ToFileRecordDTO(this DataStorageRecord e, bool includeFileData = false)
        {
            if (!e.DataStorageReference.IsLoaded)
            {
                e.DataStorageReference.Load();
                DataProjectLogCollector.LogLoadedEntityInExtension("Datastorage.cs e.DataStorageReference");
            }

            FileRecordDTO dto = new FileRecordDTO
            {
                Description = string.IsNullOrEmpty(e.DataStorage.Description) ? e.DataStorage.FileName : e.DataStorage.Description,
                FileName = e.DataStorage.FileName?.RemoveNewLine(),
                Extension = e.DataStorage.Extension,
                FileId = e.DataStorage.DataStorageId,
                FileSize = e.DataStorage.FileSize,
                Entity = (SoeEntityType)e.Entity,
                FileRecordId = e.DataStorageRecordId,
                RecordId = e.RecordId,
                Created = e.DataStorage.Created,
                CreatedBy = e.DataStorage.CreatedBy,
                Modified = e.DataStorage.Modified,
                ModifiedBy = e.DataStorage.ModifiedBy,
                Type = (SoeDataStorageRecordType)e.Type, 
            };
            if (includeFileData)
                dto.Data = e.DataStorage.Data;

            return dto;
        }

        public static IEnumerable<FileRecordDTO> ToFileRecordDTOs(this IEnumerable<DataStorageRecord> e)
        {
            return e.Select(s => s.ToFileRecordDTO()).ToList();
        }
        #endregion
    }
}
