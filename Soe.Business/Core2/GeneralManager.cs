using SoftOne.Communicator.Shared.DTO;
using SoftOne.Soe.Business.Core.BlobStorage;
using SoftOne.Soe.Business.Core.CrGen;
using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Business.Util.Azure;
using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Business.Util.LogCollector;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Util;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Transactions;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Core
{
    public class GeneralManager : ManagerBase
    {
        #region Variables

        // Create a logger for use in this class
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static ImmutableHashSet<SoeDataStorageRecordType> EntityNoneDataStorageTypes { get; } = ImmutableHashSet.Create(
            SoeDataStorageRecordType.OrderInvoiceSignature,
            SoeDataStorageRecordType.OrderInvoiceFileAttachment
            );

        #endregion

        #region Ctor

        public GeneralManager(ParameterObject parameterObject) : base(parameterObject) { }

        #endregion

        #region Assembly info

        public static string GetAssemblyVersion()
        {
            try
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
                FileInfo fi = new FileInfo(assembly.Location);

                string version = fvi.FileVersion;
                DateTime modifiedDate = fi.LastWriteTime;

                return String.Format("{0} ({1})", version, modifiedDate.ToShortDateShortTimeString());
            }
            catch (Exception)
            {
                return "Unknown";
            }
        }

        public DateTime GetAssemblyDate()
        {
            try
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                FileInfo fi = new FileInfo(assembly.Location);

                return fi.LastWriteTime;
            }
            catch (Exception)
            {
                return new DateTime(1900, 1, 1);
            }
        }

        #endregion

        #region DataStorage

        public ActionResult MoveToEvoDataStorage(CompEntities entities, DataStorage dataStorage, bool saveChanges = true)
        {
            ActionResult result = new ActionResult();

            if (dataStorage == null)
            {
                result.Success = false;
                return result;
            }

            UnCompressDataStorage(dataStorage, false);

            if (dataStorage.ExternalLink == null)
                dataStorage.ExternalLink = Guid.NewGuid().ToString();

            var upsertResult = EvoDataStorageConnector.UpsertDataStorage(dataStorage.Data, dataStorage.XML, dataStorage.DataStorageId, ConfigurationSetupUtil.GetCurrentSysCompDbId(), dataStorage.ActorCompanyId, dataStorage.ExternalLink);
            result.Success = upsertResult.Success;
            if (!result.Success)
                result.ErrorMessage = upsertResult.Message;
            else
                ClearDataOnDataStorage(dataStorage);

            if (result.Success && saveChanges)
                result = SaveChanges(entities);

            return result;
        }

        private void ClearDataOnDataStorage(DataStorage dataStorage)
        {
            dataStorage.Data = null;
            dataStorage.XML = null;
            dataStorage.DataCompressed = null;
            dataStorage.XMLCompressed = null;
        }

        public ActionResult CompressStorage(CompEntities entities, DataStorage dataStorage, bool saveChanges = true, bool AddToBlob = false)
        {
            ActionResult result = new ActionResult();

            if (dataStorage == null)
            {
                result.Success = false;
                return result;
            }

            Byte[] compressedObject = null;
            Guid guid = Guid.NewGuid();
            string key = dataStorage.ExternalLink ?? guid.ToString();
            int gainedData = 0;

            var data = dataStorage.Data;
            if (dataStorage.Data != null)
            {
                compressedObject = CompressData(dataStorage.Data);
                if (compressedObject != null)
                {
                    dataStorage.DataCompressed = compressedObject;
                    gainedData = dataStorage.Data.Length - compressedObject.Length;
                    dataStorage.Data = null;
                }
                else
                    result.Success = false;
            }

            var xml = dataStorage.XML;
            if (dataStorage.XML != null)
            {
                compressedObject = CompressString(dataStorage.XML);
                if (compressedObject != null)
                {
                    dataStorage.XMLCompressed = compressedObject;
                    gainedData = dataStorage.XML.Length - compressedObject.Length;
                    dataStorage.XML = null;
                }
                else
                    result.Success = false;
            }

            var upserted = false;
            if (result.Success && AddToBlob && dataStorage.DataStorageId != 0)
            {
                var upsertResult = EvoDataStorageConnector.UpsertDataStorage(data, xml, dataStorage.DataStorageId, ConfigurationSetupUtil.GetCurrentSysCompDbId(), dataStorage.ActorCompanyId, key);
                result.Success = upsertResult.Success;
                if (!result.Success)
                    result.ErrorMessage = upsertResult.Message;
                upserted = true;
            }

            if (result.Success && saveChanges)
                result = SaveChanges(entities);

            if (!upserted && result.Success && AddToBlob && dataStorage.DataStorageId != 0)
            {
                var upsertResult = EvoDataStorageConnector.UpsertDataStorage(data, xml, dataStorage.DataStorageId, ConfigurationSetupUtil.GetCurrentSysCompDbId(), dataStorage.ActorCompanyId, key);
                result.Success = upsertResult.Success;
            }

            result.IntegerValue = gainedData;
            return result;
        }

        public byte[] CompressData(byte[] byteArray)
        {
            try
            {
                return CompressionUtil.Compress(byteArray);
            }
            catch (Exception ex)
            {
                try
                {
                    LogError(ex, log);
                    var path = $@"{ConfigSettings.SOE_SERVER_DIR_TEMP_PHYSICAL}\{Guid.NewGuid()}";
                    var arr = CompressionUtil.Compress(path);
                    Thread.Sleep(100);
                    File.Delete(path);
                    return arr;
                }
                catch (Exception ex2)
                {
                    LogError(ex2, log);
                    return null;
                }
            }
        }

        public byte[] CompressString(string str)
        {
            try
            {
                return ZipUtility.CompressString(str);
            }
            catch (Exception ex)
            {
                try
                {
                    LogError(ex, log);
                    var path = $@"{ConfigSettings.SOE_SERVER_DIR_TEMP_PHYSICAL}\{Guid.NewGuid()}";
                    File.WriteAllText(path, str);
                    var arr = CompressionUtil.Compress(path);
                    Thread.Sleep(100);
                    File.Delete(path);
                    return arr;
                }
                catch (Exception ex2)
                {
                    LogError(ex2, log);
                    return null;
                }
            }
        }

        private void UnCompressDataStorage(DataStorage dataStorage, bool getFromEvo = true)
        {
            if (dataStorage != null)
            {
                if (dataStorage.DataCompressed != null)
                    dataStorage.Data = CompressionUtil.Decompress(dataStorage.DataCompressed);

                if (dataStorage.XMLCompressed != null)
                    dataStorage.XML = ZipUtility.UnzipString(dataStorage.XMLCompressed);

                if (getFromEvo && dataStorage.Data == null && dataStorage.XML == null)
                {
                    var ds = EvoDataStorageConnector.GetDataStorage(dataStorage.DataStorageId, ConfigurationSetupUtil.GetCurrentSysCompDbId(), dataStorage.ActorCompanyId, dataStorage.ExternalLink);
                    if (ds != null && ds.Base64Xml != null && ds.Base64Xml.Length > 0)
                        dataStorage.XML = ds.GetXml();
                    if (ds != null && ds.Base64File != null && ds.Base64File.Length > 0)
                        dataStorage.Data = ds.GetData();
                }
            }
        }

        public string GetDataStorageExternalLink(CompEntities entities, int actorCompanyId, int dataStorageRecordId)
        {
            return (from d in entities.DataStorageRecord
                    where d.DataStorageRecordId == dataStorageRecordId &&
                    d.DataStorage.ActorCompanyId == actorCompanyId
                    select d.DataStorage.ExternalLink).FirstOrDefault();
        }

        public string GetFileType(SoeDataStorageRecordType type)
        {
            string fileType = string.Empty;

            switch (type)
            {
                case SoeDataStorageRecordType.BillingInvoicePDF:
                    fileType = "pdf";
                    break;
                case SoeDataStorageRecordType.BillingInvoiceXML:
                    fileType = "xml";
                    break;
                case SoeDataStorageRecordType.CustomerFileAttachment:
                    fileType = "file";
                    break;
                case SoeDataStorageRecordType.DiRegnskapCustomerInvoiceExport:
                    fileType = "xml";
                    break;
                case SoeDataStorageRecordType.DnBNorCustomerInvoiceExport:
                    fileType = "txt";
                    break;
                case SoeDataStorageRecordType.FinvoiceCustomerInvoiceExport:
                    fileType = "txt";
                    break;
                case SoeDataStorageRecordType.HelpAttachment:
                    fileType = "pdf";
                    break;
                case SoeDataStorageRecordType.InvoiceBitmap:
                    fileType = "bmp";
                    break;
                case SoeDataStorageRecordType.InvoicePaymentServiceExport:
                    fileType = "txt";
                    break;
                case SoeDataStorageRecordType.InvoicePdf:
                    fileType = "pdf";
                    break;
                case SoeDataStorageRecordType.OrderInvoiceFileAttachment:
                    fileType = "file";
                    break;
                case SoeDataStorageRecordType.OrderInvoiceFileAttachment_Image:
                    fileType = "jpeg";
                    break;
                case SoeDataStorageRecordType.OrderInvoiceFileAttachment_Thumbnail:
                    fileType = "jpeg";
                    break;
                case SoeDataStorageRecordType.PayrollSlipXML:
                    fileType = "xml";
                    break;
                case SoeDataStorageRecordType.SOPCustomerInvoiceExport:
                    fileType = "txt";
                    break;
                case SoeDataStorageRecordType.TimeSalaryExport:
                    fileType = "txt";
                    break;
                case SoeDataStorageRecordType.TimeSalaryExportControlInfo:
                    fileType = "txt";
                    break;
                case SoeDataStorageRecordType.TimeSalaryExportControlInfoEmployee:
                    fileType = "txt";
                    break;
                case SoeDataStorageRecordType.TimeSalaryExportEmployee:
                    fileType = "txt";
                    break;
                case SoeDataStorageRecordType.TimeSalaryExportSaumaPdf:
                    fileType = "pdf";
                    break;
                case SoeDataStorageRecordType.UniMicroCustomerInvoiceExport:
                    fileType = "txt";
                    break;
                case SoeDataStorageRecordType.UploadedFile:
                    fileType = "file";
                    break;
                case SoeDataStorageRecordType.VacationYearEndHead:
                    fileType = "txt";
                    break;
                case SoeDataStorageRecordType.XEMailFileAttachment:
                    fileType = "file";
                    break;
            }

            return fileType;

        }

        public List<DataStorage> GetDataStorages(int actorCompanyId, SoeDataStorageRecordType storageType)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            entitiesReadOnly.DataStorage.NoTracking();
            entitiesReadOnly.DataStorageRecord.NoTracking();

            return (from ds in entitiesReadOnly.DataStorage.Include("DataStorageRecord")
                    where ds.ActorCompanyId == actorCompanyId &&
                    ds.Type == (int)storageType &&
                    ds.State == (int)SoeEntityState.Active
                    orderby ds.Created
                    select ds).ToList();
        }

        public List<DataStorage> GetDataStorages(CompEntities entities, int actorCompanyId, SoeDataStorageRecordType storageType)
        {
            return (from ds in entities.DataStorage.Include("DataStorageRecord")
                    where ds.ActorCompanyId == actorCompanyId &&
                    ds.Type == (int)storageType &&
                    ds.State == (int)SoeEntityState.Active
                    orderby ds.Created
                    select ds).ToList();
        }

        public DataStorage GetDataStorage(int dataStorageId, int actorCompanyId, bool includeDataStorageRecord = true, bool includeDataStorageRecipients = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.DataStorage.NoTracking();
            return GetDataStorage(entities, dataStorageId, actorCompanyId, includeDataStorageRecord, includeDataStorageRecipients);
        }

        public DataStorage GetDataStorageByDataStorageRecordId(CompEntities entities, int dataStorageRecordId, int actorCompanyId, bool includeDataStorageRecord)
        {
            IQueryable<DataStorage> query = entities.DataStorage;

            if (includeDataStorageRecord)
                query = query.Include("DataStorageRecord");

            return (from d in query
                    where d.DataStorageRecord.Any(r => r.DataStorageRecordId == dataStorageRecordId) &&
                    d.ActorCompanyId == actorCompanyId &&
                    d.State == (int)SoeEntityState.Active
                    select d).FirstOrDefault();
        }

        public DataStorage GetDataStorage(CompEntities entities, int dataStorageId, int actorCompanyId, bool includeDataStorageRecord = true, bool includeDataStorageRecipients = false)
        {
            IQueryable<DataStorage> query = entities.DataStorage;

            if (includeDataStorageRecord)
                query = query.Include("DataStorageRecord");

            var dataStorage = (from ds in query
                               where ds.DataStorageId == dataStorageId &&
                                     ds.ActorCompanyId == actorCompanyId &&
                                     ds.State == (int)SoeEntityState.Active
                               select ds).FirstOrDefault();

            if (includeDataStorageRecipients && dataStorage != null)
                dataStorage.DataStorageRecipient.Load();

            if (dataStorage != null)
                UnCompressDataStorage(dataStorage);

            return dataStorage;
        }

        public int GetDataStorageId(SoeDataStorageRecordType type, int timePeriodId, int employeeId, int actorCompanyId)
        {
            DataStorage dataStorage = GetDataStorage(type, timePeriodId, employeeId, actorCompanyId);
            return dataStorage != null ? dataStorage.DataStorageId : 0;
        }

        public DataStorage GetDataStorage(SoeDataStorageRecordType type, int timePeriodId, int employeeId, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.DataStorage.NoTracking();
            return GetDataStorage(entities, type, timePeriodId, employeeId, actorCompanyId);
        }

        public DataStorage GetDataStorage(CompEntities entities, SoeDataStorageRecordType type, int timePeriodId, int employeeId, int actorCompanyId, bool getUrl = false)
        {
            var dataStorage = (from ds in entities.DataStorage
                        .Include("DataStorageRecord")
                               where ds.ActorCompanyId == actorCompanyId &&
                               ds.EmployeeId.HasValue && ds.EmployeeId.Value == employeeId &&
                               ds.TimePeriodId.HasValue && ds.TimePeriodId.Value == timePeriodId &&
                               ds.Type == (int)type &&
                               ds.State == (int)SoeEntityState.Active
                               select ds).FirstOrDefault();

            if (getUrl && dataStorage != null)
            {
                BlobUtil blobUtil = new BlobUtil();
                blobUtil.Init(BlobUtil.CONTAINER_DATASTORAGE);
                var url = blobUtil.GetDownloadLink(dataStorage.ExternalLink);
                dataStorage.DownloadURL = GetUrlForDownload(dataStorage.Data, dataStorage.FileName);
            }

            if (dataStorage != null)
            {
                UnCompressDataStorage(dataStorage, true);

                if (getUrl && dataStorage.Data != null)
                    dataStorage.DownloadURL = GetUrlForDownload(dataStorage.Data, dataStorage.FileName);
            }

            return dataStorage;
        }

        public List<DataStorage> GetDataStorages(CompEntities entities, SoeDataStorageRecordType type, int timePeriodId, int employeeId, int actorCompanyId, bool getUrl = false)
        {
            var dataStorages = (from ds in entities.DataStorage
                                where ds.ActorCompanyId == actorCompanyId &&
                                ds.EmployeeId.HasValue && ds.EmployeeId.Value == employeeId &&
                                ds.TimePeriodId.HasValue && ds.TimePeriodId.Value == timePeriodId &&
                                ds.Type == (int)type &&
                                ds.State == (int)SoeEntityState.Active
                                select ds).ToList();

            foreach (var dataStorage in dataStorages)
            {
                UnCompressDataStorage(dataStorage, true);

                if (getUrl)
                    dataStorage.DownloadURL = GetUrlForDownload(dataStorage.Data, dataStorage.FileName);
            }

            return dataStorages;
        }

        public DataStorage CreateDataStorage(CompEntities entities, SoeDataStorageRecordType type, string xml, byte[] data, int? timePeriodId, int? employeeId, int actorCompanyId, string description = "", int? fileSize = null, string folder = "", DateTime? validFrom = null, DateTime? validTo = null, string fileName = null)
        {
            if (DefenderUtil.IsVirus(data))
            {
                LogCollector.LogInfo($"Virus detected {actorCompanyId} user {base.UserId} file {fileName} employeeId {employeeId} {Environment.StackTrace}");
                return null;
            }

            var dataStorage = new DataStorage
            {
                Type = (int)type,
                XML = xml,
                Data = data,
                Description = description,
                FileSize = fileSize,
                FileName = string.IsNullOrEmpty(fileName) ? description : fileName,
                Folder = folder,
                ValidFrom = validFrom,
                ValidTo = validTo,
                State = (int)SoeEntityState.Active,

                //Set FK
                ActorCompanyId = actorCompanyId,
                EmployeeId = employeeId,
                TimePeriodId = timePeriodId,
                UserId = base.UserId.ToNullable()
            };

            if (!string.IsNullOrEmpty(dataStorage.FileName))
            {
                dataStorage.FileName = dataStorage.FileName.RemoveNewLine();
            }

            if (!string.IsNullOrEmpty(fileName) && fileName.Contains('.'))
                dataStorage.Extension = Path.GetExtension(fileName);
            else if (!string.IsNullOrEmpty(description) && description.Contains('.'))
                dataStorage.Extension = Path.GetExtension(description);

            SetCreatedProperties(dataStorage);
            entities.DataStorage.AddObject(dataStorage);
            CompressStorage(entities, dataStorage, false, false);

            return dataStorage;
        }

        public ActionResult DeleteDataStorage(int dataStorageId, int actorCompanyId)
        {
            ActionResult result = new ActionResult(true);

            using (var entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        DataStorage dataStorage = GetDataStorage(entities, dataStorageId, actorCompanyId);
                        if (dataStorage == null)
                            return new ActionResult((int)ActionResultSave.EntityNotFound, "DataStorage");

                        result = ChangeEntityState(dataStorage, SoeEntityState.Deleted);
                        if (result.Success)
                        {
                            if (!dataStorage.Children.IsLoaded)
                                dataStorage.Children.Load();

                            foreach (DataStorage dataStorageChild in dataStorage.Children)
                            {
                                result = ChangeEntityState(dataStorageChild, SoeEntityState.Deleted);
                                if (!result.Success)
                                    break;
                            }
                        }

                        if (result.Success)
                            result = SaveChanges(entities, transaction);
                        if (result.Success)
                            transaction.Complete();
                    }
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                }
                finally
                {
                    if (!result.Success)
                    {
                        base.LogTransactionFailed(this.ToString(), this.log);
                    }

                    entities.Connection.Close();
                }
            }

            return result;
        }

        public ActionResult UndoDataStorage(int actorCompanyId, int dataStorageId)
        {
            ActionResult result = new ActionResult();

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        if (entities.Connection.State != ConnectionState.Open)
                            entities.Connection.Open();

                        DataStorage dataStorage = GetDataStorage(entities, dataStorageId, actorCompanyId);
                        if (dataStorage == null)
                            return new ActionResult(false);

                        switch ((SoeDataStorageRecordType)dataStorage.Type)
                        {
                            //Supported
                            case SoeDataStorageRecordType.SOPCustomerInvoiceExport:
                                result = UndoSOPCustomerInvoiceExport(entities, dataStorage);
                                break;
                        }

                        if (result.Success)
                        {
                            ChangeEntityState(dataStorage, SoeEntityState.Deleted);

                            result = SaveChanges(entities, transaction);

                            //Commmit transaction
                            if (result.Success)
                                transaction.Complete();
                        }
                    }
                }
                catch (Exception ex)
                {
                    result.Exception = ex;
                    base.LogError(ex, this.log);
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
            }

            return result;
        }

        #endregion

        #region DataStorageRecord

        public bool HasDataStorageRecords(int actorCompanyId, SoeDataStorageRecordType storageType, SoeEntityType entityType, int recordId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.DataStorageRecord.NoTracking();
            return HasDataStorageRecords(entities, actorCompanyId, storageType, entityType, recordId);
        }

        public bool HasDataStorageRecords(CompEntities entities, int actorCompanyId, SoeDataStorageRecordType storageType, SoeEntityType entityType, int recordId)
        {
            return (from dsr in entities.DataStorageRecord
                    where dsr.DataStorage.ActorCompanyId == actorCompanyId &&
                    dsr.DataStorage.Type == (int)storageType &&
                    dsr.Entity == (int)entityType &&
                    dsr.RecordId == recordId &&
                    dsr.DataStorage.State == (int)SoeEntityState.Active
                    select dsr).Any();
        }

        public bool HasDataStorageRecords(CompEntities entities, int actorCompanyId, int recordId, SoeDataStorageRecordType storageType, SoeEntityType entityType, List<int> excludedIds)
        {
            return (from dsr in entities.DataStorageRecord
                    where dsr.DataStorage.ActorCompanyId == actorCompanyId &&
                    dsr.DataStorage.Type == (int)storageType &&
                    dsr.Entity == (int)entityType &&
                    dsr.RecordId == recordId &&
                    !excludedIds.Contains(dsr.DataStorageRecordId) &&
                    dsr.DataStorage.State == (int)SoeEntityState.Active
                    select dsr).Any();
        }

        public Dictionary<int, bool> HasDataStorageRecords(CompEntities entities, int actorCompanyId, List<int> recordIds, SoeDataStorageRecordType storageType, SoeEntityType entityType)
        {
            return (from dsr in entities.DataStorageRecord
                    where dsr.DataStorage.ActorCompanyId == actorCompanyId &&
                    dsr.DataStorage.Type == (int)storageType &&
                    dsr.Entity == (int)entityType &&
                    recordIds.Contains(dsr.RecordId) &&
                    dsr.DataStorage.State == (int)SoeEntityState.Active
                    select dsr).DistinctBy(d => d.RecordId).ToDictionary(k => k.RecordId, v => true);
        }

        public List<DataStorageRecord> GetDataStorageRecords(int actorCompanyId, SoeDataStorageRecordType storageType, bool includeInvoiceAttachment = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.DataStorageRecord.NoTracking();
            return GetDataStorageRecords(entities, actorCompanyId, storageType, includeInvoiceAttachment);
        }

        public List<DataStorageRecord> GetDataStorageRecords(CompEntities entities, int actorCompanyId, SoeDataStorageRecordType storageType, bool includeInvoiceAttachment = false)
        {
            IQueryable<DataStorageRecord> query = entities.DataStorageRecord;

            if (includeInvoiceAttachment)
                query = query.Include("InvoiceAttachment");

            return (from dsr in query
                    where dsr.DataStorage.ActorCompanyId == actorCompanyId &&
                    dsr.DataStorage.Type == (int)storageType &&
                    dsr.DataStorage.State == (int)SoeEntityState.Active
                    orderby dsr.DataStorage.Created
                    select dsr).ToList();
        }

        public List<DataStorageRecord> GetDataStorageRecords(int? actorCompanyId, int? roleId, int recordId, SoeEntityType entityType, SoeDataStorageRecordType? type = null, bool loadConfirmationStatus = false, bool includeDataStorage = false, bool includeInvoiceAttachment = false, bool includeDataStorageRecipient = false, bool skipDecompress = false, bool includeAttestState = false, bool loadData = true)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            return this.GetDataStorageRecords(entitiesReadOnly, actorCompanyId, roleId, recordId, entityType, type, loadConfirmationStatus: loadConfirmationStatus, includeDataStorage: includeDataStorage, includeInvoiceAttachment: includeInvoiceAttachment, includeDataStorageRecipient: includeDataStorageRecipient, skipDecompress, includeAttestState, loadData);
        }

        public List<FileRecordDTO> GetDataStorageRecordDTOs(int actorCompanyId, int roleId, int recordId, SoeEntityType entity, SoeDataStorageRecordType type)
        {
            // Used for displaying File Records attached to certain entities (voucher, supplier, etc.).
            // Added GetDataStorageRecords fetches way too much data unnecessarily.
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            var records = entitiesReadOnly.DataStorageRecord
                .Where(r => r.DataStorage.ActorCompanyId == actorCompanyId &&
                    r.RecordId == recordId &&
                    r.Entity == (int)entity &&
                    r.Type == (int)type &&
                    r.DataStorage.State == (int)SoeEntityState.Active)
                .Select(r => new FileRecordDTO()
                {
                    Description = r.DataStorage.Description,
                    FileName = r.DataStorage.FileName,
                    Extension = r.DataStorage.Extension,
                    FileId = r.DataStorage.DataStorageId,
                    FileSize = r.DataStorage.FileSize,
                    Entity = (SoeEntityType)r.Entity,
                    FileRecordId = r.DataStorageRecordId,
                    RecordId = r.RecordId,
                    Created = r.DataStorage.Created,
                    CreatedBy = r.DataStorage.CreatedBy,
                    Modified = r.DataStorage.Modified,
                    ModifiedBy = r.DataStorage.ModifiedBy,
                    Type = (SoeDataStorageRecordType)r.Type,
                })
                .ToList();
            records.ForEach(r => r.FileName?.RemoveNewLine());
            return records;
        }

        public List<DataStorageRecord> GetDataStorageRecords(CompEntities entities, int? actorCompanyId, int? roleId, int recordId, SoeEntityType entityType, SoeDataStorageRecordType? type = null, bool loadConfirmationStatus = false, bool includeDataStorage = false, bool includeInvoiceAttachment = false, bool includeDataStorageRecipient = false, bool skipDecompress = false, bool includeAttestState = false, bool loadData = true)
        {
            return this.GetDataStorageRecords(entities, actorCompanyId, roleId, recordId, entityType, type == null ? null : new List<SoeDataStorageRecordType> { (SoeDataStorageRecordType)type }, loadConfirmationStatus, includeDataStorage, includeInvoiceAttachment, includeDataStorageRecipient, skipDecompress, includeAttestState, loadData);
        }

        public List<DataStorageRecord> GetDataStorageRecords(int? actorCompanyId, int? roleId, int recordId, SoeEntityType entityType, List<SoeDataStorageRecordType> types, bool loadConfirmationStatus = false, bool includeDataStorage = false, bool includeInvoiceAttachment = false, bool includeDataStorageRecipient = false, bool skipDecompress = false, bool includeAttestState = false, bool loadData = true)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            return this.GetDataStorageRecords(entitiesReadOnly, actorCompanyId, roleId, recordId, entityType, types, loadConfirmationStatus, includeDataStorage, includeInvoiceAttachment, includeDataStorageRecipient, skipDecompress, includeAttestState, loadData);
        }

        public List<DataStorageRecord> GetDataStorageRecords(CompEntities entities, int? actorCompanyId, int? roleId, int recordId, SoeEntityType entityType, List<SoeDataStorageRecordType> types, bool loadConfirmationStatus = false, bool includeDataStorage = false, bool includeInvoiceAttachment = false, bool includeDataStorageRecipient = false, bool skipDecompress = false, bool includeAttestState = false, bool loadData = true)
        {
            if (recordId == 0)
                return new List<DataStorageRecord>();

            IQueryable<DataStorageRecord> query = (from entry in this.GetDataStorageRecordQuery(entities, recordId, entityType, includeInvoiceAttachment, includeDataStorage, includeDataStorageRecipient, includeAttestState) select entry);

            if (types != null && types.Count == 1)
            {
                SoeDataStorageRecordType type = types[0];
                query = query.Where(entry => entry.DataStorage.Type == (int)type);
            }
            else if (types != null && types.Count > 1)
            {
                query = query.Where(entry => types.Contains((SoeDataStorageRecordType)entry.DataStorage.Type));
            }

            if (actorCompanyId.HasValue)
                query = query.Where(entry => entry.DataStorage.ActorCompanyId == actorCompanyId);

            int userId = 0;
            if (roleId.HasValue)
            {
                if (entityType == SoeEntityType.Employee)
                {
                    // If employee is loading documents on himself, do not check roles.
                    // Employee is allowed to se all documents on himself regardless of specified role permissions.
                    userId = UserManager.GetUserIdByEmployeeId(entities, recordId, actorCompanyId.Value);
                    if (userId == base.UserId)
                        roleId = null;
                }

                if (roleId.HasValue)
                    query = query.Where(entry => !entry.DataStorageRecordRolePermission.Any(p => p.State == (int)SoeEntityState.Active) || entry.DataStorageRecordRolePermission.Where(p => p.State == (int)SoeEntityState.Active).Select(p => p.RoleId).Contains(roleId.Value));
            }

            List<DataStorageRecord> records = query.ToList();

            if (loadConfirmationStatus && actorCompanyId.HasValue)
            {
                if (userId == 0)
                    userId = UserManager.GetUserIdByEmployeeId(entities, recordId, actorCompanyId.Value);
                if (userId != 0)
                {
                    foreach (DataStorageRecord record in records)
                    {
                        if (record.DataStorage != null)
                            record.NeedsConfirmation = record.DataStorage.NeedsConfirmation;

                        DataStorageRecord messageRecord = (from d in entities.DataStorageRecord
                                                           where d.DataStorageId == record.DataStorageId &&
                                                           d.DataStorageRecordId != record.DataStorageRecordId &&
                                                           d.Entity == (int)SoeEntityType.XEMail
                                                           select d).FirstOrDefault();
                        if (messageRecord != null)
                        {
                            Message message = (from m in entities.Message.Include("MessageRecipient")
                                               where m.MessageId == messageRecord.RecordId &&
                                               m.Type == (int)TermGroup_MessageType.NeedsConfirmation &&
                                               m.State == (int)SoeEntityState.Active
                                               select m).FirstOrDefault();

                            if (message != null)
                            {
                                MessageRecipient recipient = message.MessageRecipient.FirstOrDefault(r => r.UserId == userId && r.State == (int)SoeEntityState.Active);
                                if (recipient != null)
                                {
                                    record.NeedsConfirmation = true;
                                    record.Confirmed = recipient.AnswerDate.HasValue;
                                    record.ConfirmedDate = recipient.AnswerDate;
                                }
                            }
                        }
                    }
                }
            }

            if (!skipDecompress && includeDataStorage)
            {
                foreach (var dataRecord in records)
                {
                    if (!dataRecord.DataStorageReference.IsLoaded)
                    {
                        dataRecord.DataStorageReference.Load();
                    }
                    if (loadData)
                        UnCompressDataStorage(dataRecord.DataStorage);
                }
            }

            return records;
        }

        public List<ImagesDTO> GetDataStorageRecordsForCustomerInvoice(int? actorCompanyId, int? roleId, int invoiceId, SoeEntityType entityType, List<SoeDataStorageRecordType> types, bool addToDistribution = false, int? parentInvoiceId = null, bool getData = false)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            return this.GetDataStorageRecordsForCustomerInvoice(entitiesReadOnly, actorCompanyId, roleId, invoiceId, entityType, types, addToDistribution, parentInvoiceId, getData);
        }

        public List<ImagesDTO> GetDataStorageRecordsForCustomerInvoice(CompEntities entities, int? actorCompanyId, int? roleId, int invoiceId, SoeEntityType entityType, List<SoeDataStorageRecordType> types, bool addToDistribution = false, int? parentInvoiceId = null, bool getData = false)
        {
            List<ImagesDTO> records = new List<ImagesDTO>();
            if (invoiceId == 0)
                return records;

            IQueryable<DataStorageRecord> query = (from entry in this.GetDataStorageRecordQuery(entities, invoiceId, entityType, true, true) select entry);

            if (types != null && types.Count == 1)
            {
                SoeDataStorageRecordType type = types[0];
                query = query.Where(entry => entry.DataStorage.Type == (int)type);
            }
            else if (types != null && types.Count > 1)
            {
                query = query.Where(entry => types.Contains((SoeDataStorageRecordType)entry.DataStorage.Type));
            }

            if (actorCompanyId.HasValue)
                query = query.Where(entry => entry.DataStorage.ActorCompanyId == actorCompanyId);

            int userId = 0;
            if (roleId.HasValue)
            {
                if (entityType == SoeEntityType.Employee)
                {
                    // If employee is loading documents on himself, do not check roles.
                    // Employee is allowed to se all documents on himself regardless of specified role permissions.
                    userId = UserManager.GetUserIdByEmployeeId(entities, invoiceId, actorCompanyId.Value);
                    if (userId == base.UserId)
                        roleId = null;
                }

                if (roleId.HasValue)
                    query = query.Where(entry => !entry.DataStorageRecordRolePermission.Any(p => p.State == (int)SoeEntityState.Active) || entry.DataStorageRecordRolePermission.Where(p => p.State == (int)SoeEntityState.Active).Select(p => p.RoleId).Contains(roleId.Value));
            }

            // Get texts
            var typeNameGeneral = TermCacheManager.Instance.GetText(7464, 1, "Manuellt tillagd");
            var typeNameSignature = TermCacheManager.Instance.GetText(7465, 1, "Signatur");
            var typeNameSupplierInvoice = TermCacheManager.Instance.GetText(31, 1, "Leverantörsfaktura");
            var typeNameEdi = TermCacheManager.Instance.GetText(7467, 1, "EDI");
            var typeNameChild = TermCacheManager.Instance.GetText(7469, 1, "underorder");

            foreach (var record in query.ToList())
            {
                var dto = new ImagesDTO()
                {
                    Description = string.IsNullOrEmpty(record.DataStorage.Description) /*&& fileNameAsDescription*/ ? record.DataStorage.FileName : record.DataStorage.Description,
                    FileName = record.DataStorage.FileName,
                    FormatType = ImageFormatType.NONE,
                    ImageId = record.DataStorageRecordId,
                    Created = record.DataStorage.Created,
                    NeedsConfirmation = record.NeedsConfirmation,
                    Confirmed = record.Confirmed,
                    ConfirmedDate = record.ConfirmedDate,
                    CanDelete = true,
                    DataStorageRecordType = (SoeDataStorageRecordType)record.Type,
                    SourceType = InvoiceAttachmentSourceType.DataStorage,
                };

                if (dto.FileName != null && dto.FileName.EndsWith(".jpg", true, null))
                {
                    dto.FormatType = ImageFormatType.JPG;
                }

                if (getData)
                {
                    UnCompressDataStorage(record.DataStorage);
                    dto.Image = record.DataStorage.Data;
                }

                if (record.InvoiceAttachment != null)
                {
                    var attachment = parentInvoiceId.HasValue ? record.InvoiceAttachment.FirstOrDefault(a => a.InvoiceId == parentInvoiceId) : record.InvoiceAttachment.FirstOrDefault();
                    if (attachment != null)
                    {
                        dto.InvoiceAttachmentId = attachment.InvoiceAttachmentId;
                        dto.IncludeWhenTransfered = attachment.AddAttachmentsOnTransfer;
                        dto.IncludeWhenDistributed = attachment.AddAttachmentsOnEInvoice;
                        dto.LastSentDate = attachment.LastDistributedDate;
                        dto.ConnectedTypeName = GetAttachmentConnectedTypeName(attachment.AttachedType.HasValue ? (InvoiceAttachmentConnectType)attachment.AttachedType.Value : InvoiceAttachmentConnectType.Manual, (SoeDataStorageRecordType)record.Type, parentInvoiceId, typeNameGeneral, typeNameSupplierInvoice, typeNameEdi, typeNameSignature, typeNameChild);
                    }
                    else
                    {
                        dto.IncludeWhenTransfered = true;
                        dto.IncludeWhenDistributed = addToDistribution;

                        var result = InvoiceAttachmentManager.AddInvoiceAttachment(entities, invoiceId, dto.ImageId, dto.SourceType, InvoiceAttachmentConnectType.Manual, dto.IncludeWhenDistributed.Value, dto.IncludeWhenTransfered.Value);
                        if (result.Success)
                            dto.InvoiceAttachmentId = result.IntegerValue;
                        dto.ConnectedTypeName = GetAttachmentConnectedTypeName(InvoiceAttachmentConnectType.Manual, (SoeDataStorageRecordType)record.Type, parentInvoiceId, typeNameGeneral, typeNameSupplierInvoice, typeNameEdi, typeNameSignature, typeNameChild);
                    }
                }
                else
                {
                    dto.IncludeWhenTransfered = true;
                    dto.IncludeWhenDistributed = addToDistribution;
                    var result = InvoiceAttachmentManager.AddInvoiceAttachment(entities, invoiceId, dto.ImageId, dto.SourceType, InvoiceAttachmentConnectType.Manual, dto.IncludeWhenDistributed.Value, dto.IncludeWhenTransfered.Value);
                    if (result.Success)
                        dto.InvoiceAttachmentId = result.IntegerValue;
                    dto.ConnectedTypeName = GetAttachmentConnectedTypeName(InvoiceAttachmentConnectType.Manual, (SoeDataStorageRecordType)record.Type, parentInvoiceId, typeNameGeneral, typeNameSupplierInvoice, typeNameEdi, typeNameSignature, typeNameChild);
                }

                if (dto.LastSentDate.HasValue)
                    dto.CanDelete = false;

                records.Add(dto);
            }

            return records;
        }

        public List<FileRecordDTO> GetDataStorageRecordDTOsForCustomerCentral(int actorCompanyId, int roleId, int recordId)
        {
            bool hasOfferPermission = FeatureManager.HasRolePermission(Feature.Billing_Offer_Offers_Edit_Images, Permission.Modify, roleId, actorCompanyId);
            bool hasOrderPermission = FeatureManager.HasRolePermission(Feature.Billing_Order_Orders_Edit_Images, Permission.Modify, roleId, actorCompanyId);
            bool hasContractPermission = FeatureManager.HasRolePermission(Feature.Billing_Contract_Contracts_Edit_Images, Permission.Modify, roleId, actorCompanyId);
            bool hasCustomerInvoicePermission = FeatureManager.HasRolePermission(Feature.Billing_Invoice_Invoices_Edit_Images, Permission.Modify, roleId, actorCompanyId);

            List<int> originTypes = new List<int>();
            originTypes.Add(0);
            if (hasOfferPermission)
                originTypes.Add((int)SoeOriginType.Offer);
            if (hasOrderPermission)
                originTypes.Add((int)SoeOriginType.Order);
            if (hasContractPermission)
                originTypes.Add((int)SoeOriginType.Contract);
            if (hasCustomerInvoicePermission)
                originTypes.Add((int)SoeOriginType.CustomerInvoice);

            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            var records = entitiesReadOnly.CustomerOverviewDataStorageView
                .Where(r => r.ActorCompanyId == actorCompanyId &&
                    r.CustomerId == recordId &&
                    originTypes.Contains(r.OriginType))
                .Select(r => new FileRecordDTO()
                {
                    Description = r.Description,
                    FileName = r.FileName,
                    Extension = r.Extension,
                    FileId = r.DataStorageId,
                    FileSize = r.FileSize,
                    FileRecordId = r.DataStorageRecordId,
                    RecordId = r.RecordId,
                    Created = r.Created,
                    CreatedBy = r.CreatedBy,
                    Modified = r.Modified,
                    ModifiedBy = r.ModifiedBy,
                    Type = (SoeDataStorageRecordType)r.DataStorageType,
                    Entity = r.OriginType == (int)SoeOriginType.Contract ? SoeEntityType.Contract : (SoeEntityType)r.OriginType,
                    IdentifierId = r.Identifier,
                    IdentifierNumber = r.IdentifierNumber,
                    IdentifierName = r.IdentifierNumber + " - " + r.IdentifierName,
                })
                .ToList();
            records.ForEach(r =>
            {
                r.FileName?.RemoveNewLine();
                r.EntityTypeName = r.Entity == SoeEntityType.None ? TermCacheManager.Instance.GetText(1710, 1, "Kund") : TermCacheManager.Instance.GetText((int)r.Entity, (int)TermGroup.SoeEntityType, "");
            });
            return records;
        }

        public string GetAttachmentConnectedTypeName(InvoiceAttachmentConnectType connectType, SoeDataStorageRecordType recordType, int? parentInvoiceId, string general, string supplierInvoice, string edi, string signature, string child)
        {
            string name = String.Empty;
            switch (connectType)
            {
                case InvoiceAttachmentConnectType.Manual:
                    if (recordType == SoeDataStorageRecordType.OrderInvoiceSignature)
                        name = signature;
                    else
                        name = general;
                    if (parentInvoiceId.HasValue)
                        name = name + " (" + child + ")";
                    break;
                case InvoiceAttachmentConnectType.Edi:
                    name = edi;
                    if (parentInvoiceId.HasValue)
                        name = name + " (" + child + ")";
                    break;
                case InvoiceAttachmentConnectType.SupplierInvoice:
                    name = supplierInvoice;
                    if (parentInvoiceId.HasValue)
                        name = name + " (" + child + ")";
                    break;
            }
            return name;
        }

        public IQueryable<DataStorageRecord> GetDataStorageRecordQuery(CompEntities entities, int recordId, SoeEntityType entityType, bool includeInvoiceAttachment = false, bool includeDataStorage = false, bool includeDataStorageRecipient = false, bool includeAttestState = false)
        {
            IQueryable<DataStorageRecord> query = entities.DataStorageRecord;

            if (includeInvoiceAttachment)
                query = query.Include("InvoiceAttachment");

            if (includeDataStorage)
                query = query.Include("DataStorage");

            if (includeDataStorageRecipient)
                query = query.Include("DataStorage.DataStorageRecipient");

            if (includeAttestState)
                query = query.Include("AttestState");

            return from entry in query
                   where entry.RecordId == recordId &&
                   entry.Entity == (int)entityType &&
                   entry.DataStorage.State == (int)SoeEntityState.Active
                   select entry;
        }

        public bool HasDataStorageRecord(CompEntities entities, int actorCompanyId, int recordId, SoeEntityType entityType)
        {
            return (from entry in entities.DataStorageRecord
                    where entry.RecordId == recordId &&
                    entry.Entity == (int)entityType &&
                    entry.DataStorage.State == (int)SoeEntityState.Active &&
                    entry.DataStorage.ActorCompanyId == actorCompanyId
                    select entry).Any();
        }

        public DataStorageRecord GetDataStorageRecord(CompEntities entities, int actorCompanyId, int recordId, SoeEntityType entityType, bool includeInvoiceAttachment = false)
        {
            return this.GetDataStorageRecordQuery(entities, recordId, entityType, includeInvoiceAttachment).Where(i => i.DataStorage.ActorCompanyId == actorCompanyId).FirstOrDefault();
        }

        public DataStorageRecord GetDataStorageRecord(int actorCompanyId, int recordId, SoeDataStorageRecordType dataStorageRecordType)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            return this.GetDataStorageRecord(entitiesReadOnly, actorCompanyId, recordId, dataStorageRecordType);
        }
        public DataStorageRecord GetDataStorageRecord(CompEntities entities, int actorCompanyId, int recordId, SoeDataStorageRecordType dataStorageRecordType)
        {
            return (from entry in entities.DataStorageRecord
                    where entry.RecordId == recordId &&
                    entry.DataStorage.ActorCompanyId == actorCompanyId &&
                    entry.Type == (int)dataStorageRecordType &&
                    entry.DataStorage.State == (int)SoeEntityState.Active
                    select entry).FirstOrDefault();
        }
        public DataStorageRecord GetDataStorageRecord(CompEntities entities, int recordId, SoeDataStorageRecordType dataStorageRecordType)
        {
            return (from entry in entities.DataStorageRecord
                    where entry.RecordId == recordId &&
                    entry.Type == (int)dataStorageRecordType &&
                    entry.DataStorage.State == (int)SoeEntityState.Active
                    select entry).FirstOrDefault();
        }
        public DataStorageRecord GetDataStorageRecord(int actorCompanyId, int dataStorageRecordId, int? roleId = null)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetDataStorageRecord(entities, actorCompanyId, dataStorageRecordId, roleId);
        }

        public DataStorageRecord GetDataStorageRecord(CompEntities entities, int actorCompanyId, int dataStorageRecordId, int? roleId = null)
        {
            if (dataStorageRecordId == 0)
                return null;

            List<DataStorageRecord> query = (from entry in entities.DataStorageRecord.Include("DataStorage")
                                             where entry.DataStorageRecordId == dataStorageRecordId &&
                                             entry.DataStorage.ActorCompanyId == actorCompanyId &&
                                             entry.DataStorage.State == (int)SoeEntityState.Active
                                             select entry).ToList();

            var type = !query.IsNullOrEmpty() ? query.FirstOrDefault().Type : 0;

            if (roleId.HasValue)
                query = query.Where(entry => !entry.DataStorageRecordRolePermission.Any(p => p.State == (int)SoeEntityState.Active) || entry.DataStorageRecordRolePermission.Where(p => p.State == (int)SoeEntityState.Active).Select(p => p.RoleId).Contains(roleId.Value)).ToList();

            if ((type == (int)SoeDataStorageRecordType.TimeEmploymentContract || type == (int)SoeDataStorageRecordType.UploadedFile) && query.IsNullOrEmpty() && base.UserId != 0)
            {
                Employee employee = EmployeeManager.GetEmployeeByUser(actorCompanyId, base.UserId);
                if (employee != null)
                {
                    query = (from entry in entities.DataStorageRecord.Include("DataStorage")
                             where entry.DataStorageRecordId == dataStorageRecordId &&
                             entry.RecordId == employee.EmployeeId &&
                             entry.DataStorage.ActorCompanyId == actorCompanyId &&
                             entry.DataStorage.State == (int)SoeEntityState.Active
                             select entry).ToList();
                }
            }

            DataStorageRecord record = query.FirstOrDefault();

            if (record != null && record.DataStorage != null)
            {
                UnCompressDataStorage(record.DataStorage);
            }

            return record;
        }

        public DataStorageRecord GetDataStorageRecordOnly(CompEntities entities, int actorCompanyId, int dataStorageRecordId)
        {
            if (dataStorageRecordId == 0)
                return null;

            return (from d in entities.DataStorageRecord
                    where d.DataStorageRecordId == dataStorageRecordId &&
                    d.DataStorage.ActorCompanyId == actorCompanyId &&
                    d.DataStorage.State == (int)SoeEntityState.Active
                    select d).FirstOrDefault();
        }

        public int? GetDataStorageRecordId(int actorCompanyId, SoeEntityType entityType, int recordId, string fileName)
        {
            using var entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            return (from dsr in entitiesReadOnly.DataStorageRecord
                    where dsr.DataStorage.ActorCompanyId == actorCompanyId &&
                    dsr.DataStorage.State == (int)SoeEntityState.Active &&
                    dsr.DataStorage.FileName == fileName &&
                    dsr.Entity == (int)entityType &&
                    dsr.RecordId == recordId
                    select dsr.DataStorageRecordId).FirstOrDefault();
        }

        public List<string> GetExistingFiles(int actorCompanyId, SoeEntityType entity, IEnumerable<ImportFileDTO> files)
        {
            var fileNames = files.Select(s => s.FileName);
            var dataStorageIds = files.Select(s => s.DataStorageId);
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            return entitiesReadOnly.DataStorage
                .Where(file =>
                    file.ActorCompanyId == actorCompanyId &&
                    file.State != (int)SoeEntityState.Deleted &&
                    file.DataStorageRecord.Any(r => r.Entity == (int)entity) &&
                    fileNames.Contains(file.FileName) &&
                    !dataStorageIds.Contains(file.DataStorageId))
                .Select(s => s.FileName)
                .Distinct()
                .ToList();
            /*Even though each file has its own DataStorageId- it's Ok to exclude all incomming DataStorageIds, 
             * since two members of files list parameter never can have the same file name.
             */
        }

        public DataStorageRecord CreateDataStorageRecord(CompEntities entities, SoeDataStorageRecordType type, int recordId, string recordNumber, SoeEntityType entityType, DataStorage dataStorage = null)
        {
            if (recordNumber != null && recordNumber.Length > 100)
                recordNumber = recordNumber.Substring(0, 100);

            DataStorageRecord dataStorageRecord = new DataStorageRecord()
            {
                Type = (int)type,
                RecordId = recordId,
                RecordNumber = recordNumber,
                Entity = (int)entityType,
            };
            entities.DataStorageRecord.AddObject(dataStorageRecord);

            if (dataStorage != null)
                dataStorage.DataStorageRecord.Add(dataStorageRecord);

            return dataStorageRecord;
        }

        public ActionResult SaveDataStorageRecord(int actorCompanyId, DataStorageRecordDTO dtoToSave, bool deleteExisting = true, List<int> roleIds = null, List<int> messageGroupIds = null)
        {
            using (var entities = new CompEntities())
            {
                return SaveDataStorageRecord(entities, actorCompanyId, dtoToSave, deleteExisting, roleIds, messageGroupIds);
            }
        }

        public ActionResult SaveDataStorageRecord(CompEntities entities, int actorCompanyId, DataStorageRecordDTO dtoToSave, bool deleteExisting = true, List<int> roleIds = null, List<int> messageGroupIds = null)
        {
            if (DefenderUtil.IsVirus(dtoToSave.Data))
                return new ActionResult() { Success = false, ErrorMessage = "Virus detected in file" };

            string description = null, recordNumber = null, fileName = null;

            if (dtoToSave is DataStorageRecordExtendedDTO)
            {
                var extended = dtoToSave as DataStorageRecordExtendedDTO;
                if (extended != null)
                {
                    description = extended.Description;
                    recordNumber = extended.RecordNumber;
                    fileName = extended.FileName;
                }
            }

            if (deleteExisting)
            {
                DataStorageRecord existingDataStorageRecord = this.GetDataStorageRecord(entities, actorCompanyId, dtoToSave.RecordId, dtoToSave.Entity);
                if (existingDataStorageRecord != null)
                {
                    //Existing found so delete this and continue
                    if (!existingDataStorageRecord.DataStorageReference.IsLoaded)
                        existingDataStorageRecord.DataStorageReference.Load();

                    DataStorage existingDataStorage = existingDataStorageRecord.DataStorage;
                    if (existingDataStorage != null)
                    {
                        existingDataStorage.Data = null;
                        existingDataStorage.XML = null;
                        ChangeEntityState(existingDataStorageRecord.DataStorage, SoeEntityState.Deleted);
                    }
                }
            }

            //Insert new
            DataStorage dataStorage = CreateDataStorage(entities, dtoToSave.Type, null, dtoToSave.Data, null, null, actorCompanyId, description, dtoToSave.Data.Length, fileName: fileName);          
            DataStorageRecord dataStorageRecord = CreateDataStorageRecord(entities, dtoToSave.Type, dtoToSave.RecordId, recordNumber, dtoToSave.Entity, dataStorage);

            // Permissions
            if (!roleIds.IsNullOrEmpty())
            {
                foreach (int roleId in roleIds)
                {
                    DataStorageRecordRolePermission perm = new DataStorageRecordRolePermission()
                    {
                        DataStorageRecord = dataStorageRecord,
                        RoleId = roleId,
                        ActorCompanyId = actorCompanyId
                    };
                    SetCreatedProperties(perm);
                }
            }

            // Message groups
            if (!messageGroupIds.IsNullOrEmpty())
            {
                // Message groups is stored as DataStorageRecords
                foreach (int messageGroupId in messageGroupIds)
                {
                    CreateDataStorageRecord(entities, SoeDataStorageRecordType.MessageGroup, messageGroupId, null, dtoToSave.Entity, dataStorage);
                }
            }

            ActionResult result = SaveChanges(entities);
            result.IntegerValue = dataStorageRecord.DataStorageRecordId;
            result.IntegerValue2 = dataStorage.DataStorageId;
            result.Value = description;

            return result;
        }

        public ActionResult UpdateDataStorageRecord(int dataStorageRecordId, int recordId)
        {
            using (var entities = new CompEntities())
            {
                return UpdateDataStorageRecord(entities, dataStorageRecordId, recordId);
            }
        }

        public ActionResult UpdateDataStorageRecord(CompEntities entities, int dataStorageRecordId, int recordId, string description = null)
        {
            var record = entities.DataStorageRecord.SingleOrDefault(f => f.DataStorageRecordId == dataStorageRecordId);
            if (record != null && record.RecordId != recordId)
            {
                if (description != null)
                {
                    if (!record.DataStorageReference.IsLoaded)
                        record.DataStorageReference.Load();

                    record.DataStorage.Description = description;
                }

                record.RecordId = recordId;
                return SaveChanges(entities);
            }
            else if (record != null && record.RecordId == recordId)
            {
                if (description != null)
                {
                    if (!record.DataStorageReference.IsLoaded)
                        record.DataStorageReference.Load();

                    record.DataStorage.Description = description;
                    return SaveChanges(entities);
                }
                else
                {
                    return new ActionResult(true);
                }
            }

            return new ActionResult(false);
        }

        public ActionResult UpdateDataStorageByRecord(int actorCompanyId, int dataStorageRecordId, int recordId, string description, string fileName)
        {
            using (var entities = new CompEntities())
            {
                var dataStorageRecord = GetDataStorageRecord(entities, actorCompanyId, dataStorageRecordId);
                if (dataStorageRecord != null && dataStorageRecord.RecordId == recordId)
                {
                    if (!string.IsNullOrEmpty(description))
                        dataStorageRecord.DataStorage.Description = description;
                    if (!string.IsNullOrEmpty(fileName))
                        dataStorageRecord.DataStorage.FileName = fileName;
                    return SaveChanges(entities);
                }
            }

            return new ActionResult(false);
        }

        public ActionResult UpdateDataStorageFiles(DataStorageRecordExtendedDTO dtoToSave, int dataStorageId)
        {
            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    if (DefenderUtil.IsVirus(dtoToSave.Data))
                        return new ActionResult(false) { Success = false, ErrorMessage = "Virus detected in file" };

                    DataStorage dataStorage = GetDataStorage(entities, dataStorageId, base.ActorCompanyId);
                    if (dataStorage == null)
                        return new ActionResult((int)ActionResultSave.EntityNotFound, "DataStorage");

                    dataStorage.Data = dtoToSave.Data;
                    dataStorage.FileSize = dtoToSave.Data.Length;
                    dataStorage.FileName = dtoToSave.FileName?.RemoveNewLine();
                    dataStorage.Description = dtoToSave.Description?.RemoveNewLine() ?? dataStorage.FileName;

                    var nameForExtension = dataStorage.FileName ?? dataStorage.Description;
                    if (!string.IsNullOrEmpty(nameForExtension) && nameForExtension.Contains('.'))
                        dataStorage.Extension = Path.GetExtension(nameForExtension);

                    SetModifiedProperties(dataStorage);
                    CompressStorage(entities, dataStorage, false, false);

                    ActionResult result = SaveChanges(entities);
                    result.IntegerValue = dataStorage.DataStorageId;
                    result.Value = dataStorage.Description;
                    result.StringValue = dataStorage.FileName;

                    return result;

                } 
                catch (Exception e)
                {
                    LogError(e, log);
                    return new ActionResult(e);
                }
            }
        }

        public ActionResult UpdateFiles(CompEntities entities, IEnumerable<FileUploadDTO> files, int recordId, SoeEntityType? entityType = null)
        {
            try
            {
                foreach (var file in files)
                {
                    if (file.IsDeleted)
                    {
                        DeleteDataStorageRecord(entities, file.Id.Value, false);
                    }
                    else
                    {
                        var record = entities.DataStorageRecord.SingleOrDefault(f => f.DataStorageRecordId == file.Id);
                        if (record == null)
                            continue;
                        //FA record.RecordId > 0 because problem when removing customerinvoice attachmens swiches inluded
                        //supplierinvoice image to this customer invoiceid and can not se a reason for this.

                        if (record.RecordId > 0 && record.RecordId != recordId)
                        {
                            if (!entityType.HasValue)
                                continue;

                            var dataStorageRecord = new DataStorageRecord
                            {
                                Type = record.Type,
                                RecordId = recordId,
                                RecordNumber = record.RecordNumber,
                                Entity = (int)entityType.Value,
                                DataStorageId = record.DataStorageId,
                            };
                            entities.DataStorageRecord.AddObject(dataStorageRecord);
                        }
                        else
                            record.RecordId = recordId;
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

        public ActionResult UpdateInvoiceAttachments(CompEntities entities, IEnumerable<FileUploadDTO> files, int recordId, SoeEntityType? entityType = null, string recordNr = "")
        {
            ActionResult result = new ActionResult();
            try
            {
                foreach (var file in files)
                {
                    if (file.IsDeleted)
                    {
                        if (file.Id.HasValue)
                            DeleteDataStorageRecord(entities, file.Id.Value, false);
                        else if (file.ImageId.HasValue)
                            DeleteDataStorageRecord(entities, file.ImageId.Value, false);
                    }
                    else
                    {
                        switch (file.DataStorageRecordType)
                        {
                            case SoeDataStorageRecordType.InvoicePdf:
                            case SoeDataStorageRecordType.InvoiceBitmap:
                                if (file.SourceType == InvoiceAttachmentSourceType.Edi)
                                {
                                    var ediEntryRecord = EdiManager.GetEdiEntryWithInvoiceAttachments(entities, file.Id.Value, base.ActorCompanyId);
                                    if (ediEntryRecord != null)
                                    {
                                        var invoiceAttachment = entities.InvoiceAttachment.Where(i => i.EdiEntryId == ediEntryRecord.EdiEntryId && i.InvoiceId == recordId).FirstOrDefault();
                                        if (invoiceAttachment != null)
                                        {
                                            // Update existing
                                            invoiceAttachment.AddAttachmentsOnEInvoice = file.IncludeWhenDistributed;
                                            invoiceAttachment.AddAttachmentsOnTransfer = file.IncludeWhenTransfered;

                                            SetModifiedProperties(invoiceAttachment);
                                        }
                                        else
                                        {
                                            result = InvoiceAttachmentManager.AddInvoiceAttachment(entities, recordId, ediEntryRecord.EdiEntryId, InvoiceAttachmentSourceType.Edi, InvoiceAttachmentConnectType.SupplierInvoice, file.IncludeWhenDistributed, file.IncludeWhenTransfered);
                                        }
                                    }
                                }
                                else
                                {
                                    var storageRecord = entities.DataStorageRecord.Include("DataStorage").Where(f => f.DataStorageRecordId == file.Id.Value).FirstOrDefault();
                                    //FA record.RecordId > 0 because problem when removing customerinvoice attachmens swiches inluded
                                    //supplierinvoice image to this customer invoiceid and can not se a reason for this.
                                    if (storageRecord != null)
                                    {
                                        if (storageRecord.RecordId > 0 && storageRecord.RecordId != recordId)
                                        {
                                            if (!entityType.HasValue)
                                                continue;

                                            if (entityType.Value == SoeEntityType.Order && storageRecord.Entity != (int)SoeEntityType.SupplierInvoice)
                                            {
                                                var dataStorageRecord = new DataStorageRecord
                                                {
                                                    Type = storageRecord.Type,
                                                    RecordId = recordId,
                                                    RecordNumber = recordNr,
                                                    Entity = (int)entityType.Value,
                                                    DataStorageId = storageRecord.DataStorageId,
                                                };
                                                entities.DataStorageRecord.AddObject(dataStorageRecord);

                                                result = SaveChanges(entities);

                                                if (!result.Success)
                                                    return result;

                                                storageRecord = dataStorageRecord;
                                            }
                                        }

                                        if (storageRecord.DataStorage != null)
                                        {
                                            if (file.FileName != storageRecord.DataStorage.FileName)
                                                storageRecord.DataStorage.FileName = file.FileName;

                                            if (file.Description != storageRecord.DataStorage.Description)
                                                storageRecord.DataStorage.Description = file.Description;
                                        }

                                        if (!storageRecord.InvoiceAttachment.IsLoaded)
                                            storageRecord.InvoiceAttachment.Load();

                                        var invoiceAttachment = entities.InvoiceAttachment.Where(i => i.DataStorageRecordId == storageRecord.DataStorageRecordId && i.InvoiceId == recordId).FirstOrDefault();
                                        if (invoiceAttachment != null)
                                        {
                                            // Update existing
                                            invoiceAttachment.AddAttachmentsOnEInvoice = file.IncludeWhenDistributed;
                                            invoiceAttachment.AddAttachmentsOnTransfer = file.IncludeWhenTransfered;

                                            SetModifiedProperties(invoiceAttachment);
                                        }
                                        else
                                        {
                                            result = InvoiceAttachmentManager.AddInvoiceAttachment(entities, recordId, storageRecord.DataStorageRecordId, InvoiceAttachmentSourceType.DataStorage, InvoiceAttachmentConnectType.SupplierInvoice, file.IncludeWhenDistributed, file.IncludeWhenTransfered);
                                        }
                                    }
                                }
                                break;
                            case SoeDataStorageRecordType.Unknown:
                            default:
                                var record = file.ImageId.HasValue && file.ImageId.Value > 0 ? entities.DataStorageRecord.Include("DataStorage").Where(f => f.DataStorageRecordId == file.ImageId.Value).FirstOrDefault() : entities.DataStorageRecord.Include("DataStorage").Where(f => f.DataStorageRecordId == file.Id.Value).FirstOrDefault();
                                //FA record.RecordId > 0 because problem when removing customerinvoice attachmens swiches inluded
                                //supplierinvoice image to this customer invoiceid and can not se a reason for this.
                                if (record == null)
                                    continue;
                                else
                                {
                                    if (record.RecordId == 0)
                                    {
                                        record.RecordId = recordId;
                                    }
                                    else if (record.RecordId > 0 && record.RecordId != recordId)
                                    {
                                        if (!entityType.HasValue)
                                            continue;

                                        var dataStorageRecord = new DataStorageRecord
                                        {
                                            Type = record.Type,
                                            RecordId = recordId,
                                            RecordNumber = recordNr,
                                            Entity = (int)entityType.Value,
                                            DataStorageId = record.DataStorageId,
                                        };
                                        entities.DataStorageRecord.AddObject(dataStorageRecord);

                                        result = SaveChanges(entities);

                                        if (!result.Success)
                                            return result;

                                        record = dataStorageRecord;
                                    }

                                    if (record.DataStorage != null)
                                    {
                                        if (file.FileName != record.DataStorage.FileName)
                                            record.DataStorage.FileName = file.FileName;

                                        if (file.Description != record.DataStorage.Description)
                                            record.DataStorage.Description = file.Description;
                                    }

                                    if (!record.InvoiceAttachment.IsLoaded)
                                        record.InvoiceAttachment.Load();

                                    var invoiceAttachment = entities.InvoiceAttachment.FirstOrDefault(i => i.DataStorageRecordId == record.DataStorageRecordId && i.InvoiceId == recordId);
                                    if (invoiceAttachment != null)
                                    {
                                        // Update existing
                                        invoiceAttachment.AddAttachmentsOnEInvoice = file.IncludeWhenDistributed;
                                        invoiceAttachment.AddAttachmentsOnTransfer = file.IncludeWhenTransfered;

                                        SetModifiedProperties(invoiceAttachment);
                                    }
                                    else
                                    {
                                        result = InvoiceAttachmentManager.AddInvoiceAttachment(entities, recordId, record.DataStorageRecordId, InvoiceAttachmentSourceType.DataStorage, InvoiceAttachmentConnectType.Manual, file.IncludeWhenDistributed, file.IncludeWhenTransfered);
                                    }
                                }
                                break;
                        }
                    }
                }

                return result;
            }
            catch (Exception e)
            {
                LogError(e, log);
                return new ActionResult(e);
            }
        }

        public ActionResult DeleteDataStorageRecord(int actorCompanyId, int dataStorageRecordId)
        {
            using (CompEntities entities = new CompEntities())
            {
                var dataStorageRecord = GetDataStorageRecord(entities, actorCompanyId, dataStorageRecordId);
                if (dataStorageRecord != null)
                {
                    return DeleteDataStorageRecord(entities, dataStorageRecord.DataStorageRecordId, true);
                }
            }

            return new ActionResult(GetText(1179, "Filen hittades inte"));
        }

        public ActionResult DeleteDataStorageRecord(int recordId, SoeDataStorageRecordType dataStorageRecordType)
        {
            using (CompEntities entities = new CompEntities())
            {
                var dataStorageRecord = GetDataStorageRecord(entities, recordId, dataStorageRecordType);
                if (dataStorageRecord != null)
                {
                    return DeleteDataStorageRecord(entities, dataStorageRecord.DataStorageRecordId, true);
                }
            }

            return new ActionResult(GetText(1179, "Filen hittades inte"));
        }

        public ActionResult DeleteDataStorageRecord(int actorCompanyId, DataStorageRecord dataStorageRecord)
        {
            using (var entities = new CompEntities())
            {
                if (dataStorageRecord != null)
                {
                    return DeleteDataStorageRecord(entities, dataStorageRecord.DataStorageRecordId, true);
                }
            }

            return new ActionResult(GetText(1179, "Filen hittades inte"));
        }

        public ActionResult DeleteDataStorageRecord(CompEntities entities, int dataStorageRecordId, bool saveChanges = true)
        {
            var dataStorageRecord = (from entry in entities.DataStorageRecord.Include("DataStorage.DataStorageRecord").Include("DataStorageRecordRolePermission").Include("InvoiceAttachment")
                                     where entry.DataStorageRecordId == dataStorageRecordId
                                     select entry).FirstOrDefault();

            if (dataStorageRecord == null)
                return new ActionResult(false, (int)ActionResultSave.EntityIsNull, "DataStorageRecord not found");

            if (!dataStorageRecord.DataStorage.DataStorageRecord.Any(d => d.DataStorageRecordId != dataStorageRecordId))
                ChangeEntityState(dataStorageRecord.DataStorage, SoeEntityState.Deleted);

            if (dataStorageRecord.AttestStateId.HasValue)
            {
                AttestWorkFlowHead head = (from h in entities.AttestWorkFlowHead.Include("AttestWorkFlowRow")
                                           where h.Entity == (int)SoeEntityType.DataStorageRecord &&
                                           h.RecordId == dataStorageRecordId
                                           select h).FirstOrDefault();
                if (head != null)
                {
                    ChangeEntityState(head, SoeEntityState.Deleted);
                    foreach (AttestWorkFlowRow row in head.AttestWorkFlowRow)
                    {
                        ChangeEntityState(row, SoeEntityState.Deleted);
                    }
                }

            }
            foreach (DataStorageRecordRolePermission perm in dataStorageRecord.DataStorageRecordRolePermission.ToList())
            {
                DeleteEntityItem(entities, perm, useBulkSaveChanges: false);
            }
            foreach (var invoiceAttachment in dataStorageRecord.InvoiceAttachment.ToList())
            {
                DeleteEntityItem(entities, invoiceAttachment, useBulkSaveChanges: false);
            }
            DeleteEntityItem(entities, dataStorageRecord, useBulkSaveChanges: false);

            return saveChanges ? SaveChanges(entities) : new ActionResult(true);
        }

        public ActionResult ExtractFilenameAndExtension()
        {
            ActionResult result = new ActionResult();

            using (CompEntities entities = new CompEntities())
            {
                List<DataStorage> records = (from d in entities.DataStorage
                                             where d.Description != null &&
                                             d.Description != string.Empty &&
                                             (d.FileName == null || d.FileName == string.Empty || d.Extension == null || d.Extension == string.Empty)
                                             select d).ToList();

                foreach (DataStorage record in records)
                {
                    if (record.Description.Contains('.'))
                    {
                        if (string.IsNullOrEmpty(record.FileName))
                            record.FileName = record.Description;
                        if (string.IsNullOrEmpty(record.Extension))
                            record.Extension = Path.GetExtension(record.Description);
                    }
                }

                entities.SaveChanges();
            }

            return result;
        }

        #endregion

        #region DataStorageRecordRolePermission

        public List<int> GetDataStorageRecordRoleIds(int dataStorageRecordId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.DataStorageRecordRolePermission.NoTracking();
            return (from d in entities.DataStorageRecordRolePermission
                    where d.DataStorageRecordId == dataStorageRecordId &&
                    d.State == (int)SoeEntityState.Active
                    select d.RoleId).ToList();
        }

        public ActionResult UpdateDataStorageRecordRoleIds(int actorCompanyId, int dataStorageRecordId, List<int> roleIds)
        {
            ActionResult result = new ActionResult();

            using (CompEntities entities = new CompEntities())
            {
                DataStorageRecord record = (from d in entities.DataStorageRecord.Include("DataStorageRecordRolePermission")
                                            where d.DataStorageRecordId == dataStorageRecordId
                                            select d).FirstOrDefault();

                if (record != null)
                {
                    foreach (DataStorageRecordRolePermission perm in record.DataStorageRecordRolePermission)
                    {
                        // Check existing
                        if (roleIds.Contains(perm.RoleId))
                        {
                            // Still exists in input, no change.
                            // Remove from input
                            roleIds.Remove(perm.RoleId);
                        }
                        else
                        {
                            // Not in input, delete
                            ChangeEntityState(perm, SoeEntityState.Deleted);
                        }
                    }

                    // Add new
                    foreach (int roleId in roleIds)
                    {
                        DataStorageRecordRolePermission perm = new DataStorageRecordRolePermission()
                        {
                            DataStorageRecord = record,
                            RoleId = roleId,
                            ActorCompanyId = actorCompanyId
                        };
                        SetCreatedProperties(perm);
                    }

                    entities.SaveChanges();
                }
            }

            return result;
        }

        #endregion

        #region Document

        public bool HasNewCompanyDocuments(int actorCompanyId, DateTime time)
        {
            string key = $"HasNewCompanyDocuments{actorCompanyId}";

            DateTime? createdOrModified = BusinessMemoryCache<DateTime?>.Get(key);
            if (createdOrModified.HasValue && createdOrModified.Value > time)
                return true;

            if (createdOrModified.HasValue && createdOrModified.Value == CalendarUtility.DATETIME_DEFAULT)
                return false;

            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            entitiesReadOnly.DataStorage.NoTracking();
            DateTime now = DateTime.Now;

            List<DataStorageDTO> documents = (from d in entitiesReadOnly.DataStorage
                                              where d.ActorCompanyId == actorCompanyId &&
                                              (d.Created > time || (d.Modified != null && d.Modified > time) || (d.ValidFrom != null && d.ValidFrom.Value <= now && d.ValidFrom.Value > time)) &&
                                              d.State == (int)SoeEntityState.Active &&
                                              (d.ValidFrom == null || d.ValidFrom.Value <= now) &&
                                              (d.ValidTo == null || d.ValidTo.Value >= now)
                                              select new DataStorageDTO() { Created = d.Created, Modified = d.Modified, ValidFrom = d.ValidFrom }).ToList();

            createdOrModified = !documents.IsNullOrEmpty() ? documents.OrderByDescending(i => i.CreatedOrModified).First().CreatedOrModified : CalendarUtility.DATETIME_DEFAULT;
            BusinessMemoryCache<DateTime?>.Set(key, createdOrModified, 90);

            return createdOrModified.Value > time;
        }

        public int GetNbrOfUnreadInformations(int licenseId, int actorCompanyId, int roleId, int userId, bool showInWeb, bool showInMobile, bool showInTerminal, int langId, bool ignoreCache = false)
        {
            var parameters = string.Empty;
            try
            {
                parameters = $"GetNbrOfUnreadInformations failed: licenseId: {licenseId}, actorCompanyId: {actorCompanyId}, roleId: {roleId}, userId: {userId}, showInWeb: {showInWeb}, showInMobile: {showInMobile}, showInTerminal: {showInTerminal}, langId: {langId}, ignoreCache: {ignoreCache}";

                if (ignoreCache)
                    return GetNbrOfUnreadInformationsFromDB(licenseId, actorCompanyId, roleId, userId, showInWeb, showInMobile, showInTerminal, langId);

                string key = $"GetNbrOfUnreadInformations_{licenseId}|{actorCompanyId}|{roleId}|{userId}|{showInWeb}|{showInMobile}|{showInTerminal}|{langId}";
                string previousKey = $"prev{key}";
                string lockKey = $"Lock_{licenseId}_{roleId}";
                // Check if the value exists in the cache
                if (BusinessMemoryCache<int?>.TryGetValue(key, out int? nbrOfUnreadInformations))
                {
                    if (nbrOfUnreadInformations.HasValue)
                        return nbrOfUnreadInformations.Value;
                }
                else
                {
                    var lockObject = BusinessMemoryCache<object>.Get(lockKey);
                    // Use a lock specific to the licenseId to handle concurrent cache updates
                    if (lockObject == null)
                    {
                        // Add lockObject to the cache to prevent other threads from updating the value in the cache
                        BusinessMemoryCache<object>.Set(lockKey, lockKey, 1);

                        // Check if another thread has already updated the value in the cache
                        if (BusinessMemoryCache<int?>.TryGetValue(key, out nbrOfUnreadInformations) && nbrOfUnreadInformations.HasValue)
                            return nbrOfUnreadInformations.Value;

                        using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
                        int? lastInDB = entitiesReadOnly.Information.OrderByDescending(o => o.InformationId).Take(1).Select(s => s.InformationId).FirstOrDefault();
                        string keyLongTerm = $"LongTerm{key}#{lastInDB}";

                        // If nothing has been added to the database since the last time the cache was updated, return the old value
                        if (BusinessMemoryCache<int?>.TryGetValue(keyLongTerm, out nbrOfUnreadInformations) && nbrOfUnreadInformations.HasValue)
                            return nbrOfUnreadInformations.Value;

                        nbrOfUnreadInformations = GetNbrOfUnreadInformationsFromDB(licenseId, actorCompanyId, roleId, userId, showInWeb, showInMobile, showInTerminal, langId);

                        BusinessMemoryCache<int?>.Set(key, nbrOfUnreadInformations, 30);
                        BusinessMemoryCache<int?>.Set(keyLongTerm, nbrOfUnreadInformations, 5 * 60);
                        BusinessMemoryCache<int?>.Set(previousKey, nbrOfUnreadInformations, 60 * 60);

                        return nbrOfUnreadInformations.Value;
                    }
                    // Return old value if another thread is updating the value in the cache
                    else if (BusinessMemoryCache<int?>.TryGetValue(previousKey, out nbrOfUnreadInformations) && nbrOfUnreadInformations.HasValue)
                        return nbrOfUnreadInformations.Value;

                }
            }
            catch (Exception e)
            {
                LogError(new Exception(parameters, e), log);
            }

            return 0; // Return 0 if the value is not found in the cache
        }

        private int GetNbrOfUnreadInformationsFromDB(int licenseId, int actorCompanyId, int roleId, int userId, bool showInWeb, bool showInMobile, bool showInTerminal, int langId)
        {
            List<InformationDTO> informations = GetUnreadInformations(licenseId, actorCompanyId, roleId, userId, showInWeb, showInMobile, showInTerminal, langId, false);
            return informations.Count;
        }

        public int GetNbrOfUnreadCompanyDocuments(int actorCompanyId, int roleId, int userId, bool ignoreCache = false)
        {
            if (ignoreCache)
                return GetNbrOfUnreadCompanyDocumentsFromDb(actorCompanyId, roleId, userId);

            string key = $"GetNbrOfUnreadCompanyDocuments{actorCompanyId}|{roleId}|{userId}";

            int? nbrOfUnreadDocuments = BusinessMemoryCache<int?>.Get(key);

            if (nbrOfUnreadDocuments.HasValue)
                return nbrOfUnreadDocuments.Value;

            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            int? lastInDB = entitiesReadOnly.DataStorage.OrderByDescending(o => o.DataStorageId).Take(1).Select(s => s.DataStorageId).FirstOrDefault();
            string keyLongTerm = $"LongTerm{key}#{lastInDB}";

            nbrOfUnreadDocuments = BusinessMemoryCache<int?>.Get(keyLongTerm);

            if (nbrOfUnreadDocuments.HasValue)
                return nbrOfUnreadDocuments.Value;

            string previousKey = $"prev{key}";
            string lockKey = $"Lock_{actorCompanyId}_{roleId}";

            var lockObject = BusinessMemoryCache<object>.Get(lockKey);
            // Use a lock specific to the licenseId to handle concurrent cache updates
            if (lockObject == null)
            {
                // Add lockObject to the cache to prevent other threads from updating the value in the cache
                BusinessMemoryCache<object>.Set(lockKey, lockKey, 1);

                nbrOfUnreadDocuments = GetNbrOfUnreadCompanyDocumentsFromDb(actorCompanyId, roleId, userId);

                BusinessMemoryCache<int?>.Set(key, nbrOfUnreadDocuments, 60);
                BusinessMemoryCache<int?>.Set(keyLongTerm, nbrOfUnreadDocuments, 60 * 10);
                BusinessMemoryCache<int?>.Set(previousKey, nbrOfUnreadDocuments, 60 * 60);

                return nbrOfUnreadDocuments.Value;
            }
            // Return old value if another thread is updating the value in the cache
            else if (BusinessMemoryCache<int?>.TryGetValue(previousKey, out nbrOfUnreadDocuments) && nbrOfUnreadDocuments.HasValue)
                return nbrOfUnreadDocuments.Value;

            return 0;
        }

        private int GetNbrOfUnreadCompanyDocumentsFromDb(int actorCompanyId, int roleId, int userId)
        {
            int readCount = 0;
            List<DataStorageDTO> dataStorages = GetCompanyDocuments(actorCompanyId, roleId, userId, addDataStorageRecipients: true, addDataStorageRecords: false, includeUserUploaded: true);
            readCount += dataStorages.Count(dataStorage => dataStorage.DataStorageRecipients != null && dataStorage.DataStorageRecipients.Any(d => d.UserId == userId && d.ReadDate.HasValue));

            List<DocumentDTO> documents = GetMyDocuments(actorCompanyId, roleId, userId);

            return dataStorages.Count - readCount + documents.Count(d => !d.ReadDate.HasValue);
        }

        private List<int> GetAllValidDataStorageIds(int actorCompanyId, int roleId, int userId)
        {
            List<int> validDataStorageIds = new List<int>();

            List<MessageGroup> validGroups = CommunicationManager.GetValidMessageGroupsForRecipient(actorCompanyId, userId, roleId);
            List<int> validGroupIds = validGroups.Select(s => s.MessageGroupId).ToList();
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            List<int> allDataStorageIds = entitiesReadOnly.DataStorage.Where(w => w.ActorCompanyId == actorCompanyId && w.Type == (int)SoeDataStorageRecordType.UploadedFile).Select(s => s.DataStorageId).ToList();
            if (allDataStorageIds.Any())
            {
                List<int> allDataStorageIdWithGroups = entitiesReadOnly.DataStorageRecord.Where(w => allDataStorageIds.Contains(w.DataStorageId)).Select(s => s.DataStorageId).ToList();
                List<int> dataStorageIdsWithoutGroups = allDataStorageIds.Where(w => !allDataStorageIdWithGroups.Contains(w)).ToList();
                List<int> validDataStorageIdOnValidGroups = entitiesReadOnly.DataStorageRecord.Where(w => w.DataStorage.ActorCompanyId == actorCompanyId && w.Type == (int)SoeDataStorageRecordType.MessageGroup && validGroupIds.Contains(w.RecordId)).Select(s => s.DataStorageId).ToList();

                validDataStorageIds.AddRange(dataStorageIdsWithoutGroups);
                validDataStorageIds.AddRange(validDataStorageIdOnValidGroups);
            }

            return validDataStorageIds;
        }

        public List<DataStorageDTO> GetCompanyDocuments(int actorCompanyId, int roleId, int userId, bool addDataStorageRecipients = true, bool addDataStorageRecords = true, bool includeUserUploaded = false)
        {
            List<int> validDataStorageIds = GetAllValidDataStorageIds(actorCompanyId, roleId, userId);
            if (!validDataStorageIds.Any())
                return new List<DataStorageDTO>();

            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            int? lastInDB = entitiesReadOnly.DataStorage.OrderByDescending(o => o.DataStorageId).Take(1).Select(s => s.DataStorageId).FirstOrDefault();
            string key = $"GetCompanyDocuments{lastInDB}|{actorCompanyId}|{validDataStorageIds.JoinToString("#")}";

            List<DataStorageDTO> dataStorages = BusinessMemoryCache<List<DataStorageDTO>>.Get(key, BusinessMemoryDistributionSetting.FullyHybridCache);
            DateTime now = DateTime.Now;
            dataStorages = dataStorages ?? (from d in entitiesReadOnly.DataStorage
                                            where d.ActorCompanyId == actorCompanyId &&
                                            d.Type == (int)SoeDataStorageRecordType.UploadedFile &&
                                            d.State == (int)SoeEntityState.Active &&
                                            (d.ValidFrom == null || d.ValidFrom.Value <= now) &&
                                            (d.ValidTo == null || d.ValidTo.Value >= now) &&
                                            validDataStorageIds.Contains(d.DataStorageId)
                                            orderby d.Folder, d.Description, d.Created descending
                                            select new DataStorageDTO()
                                            {
                                                Type = (SoeDataStorageRecordType)d.Type,
                                                Xml = d.XML,
                                                Description = d.Description,
                                                FileSize = d.FileSize ?? 0,
                                                FileName = d.FileName,
                                                Folder = d.Folder,
                                                ValidFrom = d.ValidFrom,
                                                ValidTo = d.ValidTo,
                                                NeedsConfirmation = d.NeedsConfirmation,
                                                State = (SoeEntityState)d.State,
                                                Name = d.Name,
                                                Extension = d.Extension,

                                                //Set FK
                                                ActorCompanyId = d.ActorCompanyId,
                                                EmployeeId = d.EmployeeId,
                                                TimePeriodId = d.TimePeriodId,
                                                UserId = d.UserId,
                                                DataStorageId = d.DataStorageId,

                                                EntityTypes = d.DataStorageRecord.Select(r => (SoeEntityType)r.Entity).ToList()
                                            }).ToList(); // No blobsLoaded

            BusinessMemoryCache<List<DataStorageDTO>>.Set(key, dataStorages.CloneDTOs(), 60 * 5, BusinessMemoryDistributionSetting.FullyHybridCache);
            List<int> dataStorageIds = dataStorages.Select(s => s.DataStorageId).ToList();

            if (includeUserUploaded)
            {
                dataStorages.AddRange((from d in entitiesReadOnly.DataStorage
                                       where d.ActorCompanyId == actorCompanyId &&
                                       d.UserId == userId &&
                                       d.Type == (int)SoeDataStorageRecordType.UploadedFile &&
                                       d.State == (int)SoeEntityState.Active &&
                                       !dataStorageIds.Contains(d.DataStorageId)
                                       orderby d.Folder, d.Description, d.Created descending
                                       select new DataStorageDTO()
                                       {
                                           Type = (SoeDataStorageRecordType)d.Type,
                                           Xml = d.XML,
                                           Description = d.Description,
                                           FileSize = d.FileSize ?? 0,
                                           FileName = d.FileName,
                                           Folder = d.Folder,
                                           ValidFrom = d.ValidFrom,
                                           ValidTo = d.ValidTo,
                                           NeedsConfirmation = d.NeedsConfirmation,
                                           State = (SoeEntityState)d.State,
                                           Name = d.Name,
                                           Extension = d.Extension,

                                           //Set FK
                                           ActorCompanyId = d.ActorCompanyId,
                                           EmployeeId = d.EmployeeId,
                                           TimePeriodId = d.TimePeriodId,
                                           UserId = d.UserId,
                                           DataStorageId = d.DataStorageId,

                                           EntityTypes = d.DataStorageRecord.Select(r => (SoeEntityType)r.Entity).ToList()
                                       }).ToList());

                // Only select documents connected to message groups or with no records at all
                dataStorages = dataStorages.Where(d => !d.EntityTypes.Any(e => e != SoeEntityType.MessageGroup)).ToList();

                dataStorageIds = dataStorages.Select(s => s.DataStorageId).ToList();
            }

            if (addDataStorageRecipients)
            {
                List<DataStorageRecipient> dataStorageRecipients = entitiesReadOnly.DataStorageRecipient.Where(w => dataStorageIds.Contains(w.DataStorageId) && w.State == (int)SoeEntityState.Active).ToList();
                if (!dataStorageRecipients.IsNullOrEmpty())
                {
                    foreach (DataStorageDTO storage in dataStorages)
                    {
                        List<DataStorageRecipient> onStorage = dataStorageRecipients.Where(w => w.DataStorageId == storage.DataStorageId).ToList();
                        if (!onStorage.IsNullOrEmpty())
                            storage.DataStorageRecipients = onStorage.ToDTOs();
                        else
                            storage.DataStorageRecipients = new List<DataStorageRecipientDTO>();
                    }
                }
            }

            if (addDataStorageRecords)
            {
                List<DataStorageRecord> dataStorageRecords = entitiesReadOnly.DataStorageRecord.Where(w => dataStorageIds.Contains(w.DataStorageId)).ToList();
                if (!dataStorageRecords.IsNullOrEmpty())
                {
                    foreach (DataStorageDTO storage in dataStorages)
                    {
                        List<DataStorageRecord> onStorage = dataStorageRecords.Where(w => w.DataStorageId == storage.DataStorageId).ToList();
                        if (!onStorage.IsNullOrEmpty())
                            storage.DataStorageRecords = onStorage.ToDTOs();
                        else
                            storage.DataStorageRecords = new List<DataStorageRecordDTO>();
                    }
                }
            }

            return dataStorages;
        }

        public List<DocumentDTO> GetMyDocuments(int actorCompanyId, int roleId, int userId)
        {
            List<DocumentDTO> documents = new List<DocumentDTO>();

            string folderEmployee = GetText(4066, "Kopplade på anställd");
            string folderMessage = GetText(4067, "Skickade via meddelanden");

            // Documents connected to the employee
            List<DataStorage> employeeDataStorages = new List<DataStorage>();
            if (FeatureManager.HasRolePermission(Feature.Time_Employee_Employees_Edit_MySelf, Permission.Readonly, roleId, actorCompanyId))
            {
                int employeeId = SessionCache.GetEmployeeFromCache(userId, actorCompanyId)?.EmployeeId ?? 0;
                List<DataStorageRecord> employeeRecords = GetDataStorageRecords(actorCompanyId, roleId, employeeId, SoeEntityType.Employee, includeDataStorage: true, includeDataStorageRecipient: true, skipDecompress: true);
                employeeDataStorages = employeeRecords.Select(r => r.DataStorage).ToList();
                foreach (DataStorage dataStorage in employeeDataStorages)
                {
                    dataStorage.Folder = folderEmployee;
                }
            }
            documents.AddRange(employeeDataStorages.ToDocumentDTOs());
            foreach (DocumentDTO doc in documents)
            {
                DataStorageRecipientDTO recipient = doc.Recipients.FirstOrDefault(r => r.UserId == userId);
                if (recipient != null)
                {
                    doc.ReadDate = recipient.ReadDate;
                    doc.ConfirmedDate = recipient.ConfirmedDate;
                }
            }

            // Documents sent in message as needs confirmation
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.MessageRecipient.NoTracking();
            var messages = (from m in entities.MessageRecipient
                            where m.UserId == userId &&
                            m.State == (int)SoeEntityState.Active &&
                            m.Message.Type == (int)TermGroup_MessageType.NeedsConfirmation &&
                            m.Message.State == (int)SoeEntityState.Active
                            select new { m.MessageId, m.Message.Subject, m.Message.Created, m.ReadDate, m.AnswerType, m.AnswerDate }).ToList();

            List<int> messageIds = messages.Select(r => r.MessageId).ToList();

            entities.DataStorageRecord.NoTracking();
            entities.DataStorage.NoTracking();
            List<DataStorageRecord> messageDataStorageRecords = (from r in entities.DataStorageRecord.Include("DataStorage.DataStorageRecipient")
                                                                 where messageIds.Contains(r.RecordId) &&
                                                                 r.Type == (int)SoeDataStorageRecordType.XEMailFileAttachment &&
                                                                 r.DataStorage.FileName.Length > 0
                                                                 select r).ToList();

            foreach (DataStorageRecord dataStorageRecord in messageDataStorageRecords)
            {
                var message = messages.FirstOrDefault(r => r.MessageId == dataStorageRecord.RecordId);
                if (message != null)
                {
                    DocumentDTO document = dataStorageRecord.DataStorage.ToDocumentDTO();
                    document.MessageId = message.MessageId;
                    document.Name = String.Format("{0} ({1})", message.Subject, document.DisplayName);
                    document.Description = message.Subject;
                    document.Folder = folderMessage;
                    document.ReadDate = message.ReadDate;
                    document.NeedsConfirmation = true;
                    document.AnswerType = (XEMailAnswerType)message.AnswerType;
                    document.AnswerDate = message.AnswerDate;
                    document.Created = message.Created;
                    document.AttestStatus = (TermGroup_DataStorageRecordAttestStatus)dataStorageRecord.AttestStatus;
                    document.CurrentAttestUsers = dataStorageRecord.CurrentAttestUsers;
                    document.AttestStateId = dataStorageRecord.AttestStateId;

                    DataStorageRecipientDTO recipient = document.Recipients.FirstOrDefault(r => r.UserId == userId);
                    if (recipient != null && recipient.ConfirmedDate.HasValue)
                        document.ConfirmedDate = recipient.ConfirmedDate;
                    else if (document.AnswerDate.HasValue) //Its needed because the Web client looks only on answerdate, the app looks on confirmeddate.
                        document.ConfirmedDate = document.AnswerDate;

                    documents.Add(document);
                }
            }

            return documents;
        }

        public List<string> GetDocumentFolders(int actorCompanyId)
        {
            // TODO: Check folders that I'm permitted to see?
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            return (from d in entitiesReadOnly.DataStorage
                    where d.ActorCompanyId == actorCompanyId &&
                    d.State == (int)SoeEntityState.Active &&
                    !(d.Folder == null || d.Folder.Equals(""))
                    select d.Folder).Distinct().ToList();
        }

        public string GetDocumentUrl(int dataStorageId, int actorCompanyId)
        {
            string url = String.Empty;

            using (CompEntities entities = new CompEntities())
            {
                // Get DataStorage
                DataStorage dataStorage = GetDataStorage(entities, dataStorageId, actorCompanyId, false);

                if (dataStorage != null && dataStorage.FileName != null)
                {

                    // Create new external link
                    BlobUtil blobUtil = new BlobUtil();
                    blobUtil.Init(BlobUtil.CONTAINER_LONG_TEMP);
                    string mimeType = WebUtil.GetContentType(dataStorage.FileName);

                    Guid.TryParse(dataStorage.ExternalLink ?? "", out Guid guid);

                    if (guid == Guid.Empty)
                        guid = Guid.NewGuid();

                    string externalLink = guid.ToString();
                    ActionResult result = blobUtil.UploadData(guid, (dataStorage.Data == null && dataStorage.DataCompressed != null ? CompressionUtil.Decompress(dataStorage.DataCompressed) : dataStorage.Data), dataStorage.FileName, mimeType);
                    if (result.Success)
                    {
                        url = blobUtil.GetDownloadLink(externalLink, dataStorage.FileName);
                    }
                }
            }

            return url;
        }

        public string GetPDF(string externalLink, int licenseId)
        {
            DataStorage dataStorage = GetDataStorage(externalLink, licenseId);
            if (dataStorage != null && !string.IsNullOrWhiteSpace(dataStorage.Extension) && dataStorage.Extension.ToLower().Equals(".pdf"))
                return Convert.ToBase64String(dataStorage.Data);

            return string.Empty;
        }

        public ActionResult UpdateDataStorage(CommunicatorMessageAttachment communicatorMessageAttachment)
        {
            using (CompEntities entities = new CompEntities())
            {
                DataStorage dataStorage = GetDataStorage(communicatorMessageAttachment.Name, Convert.ToInt32(communicatorMessageAttachment.Filesize));
                dataStorage.Data = communicatorMessageAttachment.DataBase64;
                if (dataStorage.Data != null)
                {
                    byte[] compressedObject = CompressData(dataStorage.Data);
                    if (compressedObject != null)
                    {
                        dataStorage.DataCompressed = compressedObject;
                        dataStorage.Data = null;
                    }
                }
                return SaveChanges(entities);
            }
        }

        public void SetDataStorageData(DataStorage dataStorage, string base64Data)
        {
            if (dataStorage != null)
            {
                dataStorage.Data = Convert.FromBase64String(base64Data);
                if (dataStorage.Data != null)
                {
                    byte[] compressedObject = CompressData(dataStorage.Data);
                    if (compressedObject != null)
                    {
                        dataStorage.DataCompressed = compressedObject;
                        dataStorage.Data = null;
                    }
                }
            }
        }

        public DataStorage GetDataStorageByReference(CompEntities entities, int dataStorageId)
        {
            DataStorage dataStorage = new DataStorage() { DataStorageId = dataStorageId };
            entities.DataStorage.Attach(dataStorage);
            return dataStorage;
        }

        public DataStorage GetDataStorage(string externalLink, int licenseId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetDataStorage(entities, externalLink, licenseId);
        }

        public DataStorage GetDataStorage(CompEntities entities, string externalLink, int licenseId)
        {
            var dataStorage = entities.DataStorage.FirstOrDefault(f => f.State == (int)SoeEntityState.Active && f.ExternalLink != null && f.ExternalLink == externalLink && f.Company.LicenseId == licenseId);

            if (dataStorage != null)
                UnCompressDataStorage(dataStorage);

            return dataStorage;
        }

        public byte[] GetDocumentData(int dataStorageId, int actorCompanyId)
        {
            DataStorage dataStorage = GetDataStorage(dataStorageId, actorCompanyId, false);
            UnCompressDataStorage(dataStorage);

            return dataStorage?.Data;
        }

        public List<DataStorageRecipientDTO> GetDocumentRecipientInfo(int dataStorageId, int actorCompanyId, int roleId, int userId, bool setEmployeename)
        {
            List<DataStorageRecipientDTO> dtos = null;

            using (CompEntities entities = new CompEntities())
            {
                // Get message groups connected to document
                List<int> messageGroupIds = (from d in entities.DataStorageRecord
                                             where d.DataStorageId == dataStorageId &&
                                             d.Type == (int)SoeDataStorageRecordType.MessageGroup
                                             select d.RecordId).ToList();

                // Get existing recipients (users who has read the document)
                List<DataStorageRecipient> recipients = (from d in entities.DataStorageRecipient.Include("User")
                                                         where d.DataStorageId == dataStorageId &&
                                                         d.State == (int)SoeEntityState.Active
                                                         select d).ToList();
                dtos = recipients.ToDTOs();

                // Add all other users permitted to see the document, but has not read it yet (have no DataStorageRecipient)
                List<User> validUsers = CommunicationManager.GetValidUsersForMessageGroups(entities, messageGroupIds, actorCompanyId, roleId, userId);
                foreach (User user in validUsers)
                {
                    if (!dtos.Select(d => d.UserId).ToList().Contains(user.UserId))
                    {
                        dtos.Add(new DataStorageRecipientDTO()
                        {
                            UserId = user.UserId,
                            UserName = user.LoginName
                        });
                    }
                }

                if (setEmployeename)
                {
                    var employees = (from e in entities.Employee.Include("ContactPerson")
                                     where e.UserId.HasValue &&
                                     e.State == (int)SoeEntityState.Active
                                     select new { e.UserId, e.EmployeeNr, e.ContactPerson.FirstName, e.ContactPerson.LastName }).ToList();

                    foreach (DataStorageRecipientDTO dto in dtos)
                    {
                        var employee = employees.FirstOrDefault(e => e.UserId == dto.UserId);
                        if (employee != null)
                            dto.EmployeeNrAndName = String.Format("({0}) {1} {2}", employee.EmployeeNr, employee.FirstName, employee.LastName);
                    }
                }
            }

            return dtos;
        }

        public ActionResult SaveDocument(DocumentDTO documentInput, byte[] fileData, int actorCompanyId)
        {
            #region Prereq

            if (documentInput == null)
                return new ActionResult(false, (int)ActionResultSave.EntityIsNull, GetText(7044, "Felaktig inparameter"));

            #endregion

            ActionResult result = new ActionResult(true);

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        if (entities.Connection.State != ConnectionState.Open)
                            entities.Connection.Open();

                        #region Perform

                        #region DataStorage

                        DataStorage dataStorage = (from d in entities.DataStorage.Include("DataStorageRecord.DataStorageRecordRolePermission")
                                                   where d.DataStorageId == documentInput.DataStorageId &&
                                                   d.ActorCompanyId == actorCompanyId &&
                                                   d.State == (int)SoeEntityState.Active
                                                   select d).FirstOrDefault();

                        if (dataStorage == null)
                        {
                            #region Add

                            dataStorage = new DataStorage()
                            {
                                ActorCompanyId = actorCompanyId,
                                Type = (int)SoeDataStorageRecordType.UploadedFile,
                                State = (int)SoeEntityState.Active,
                                UserId = base.UserId
                            };

                            SetCreatedProperties(dataStorage);
                            entities.DataStorage.AddObject(dataStorage);

                            #endregion
                        }
                        else
                        {
                            #region Update

                            SetModifiedProperties(dataStorage);

                            #endregion
                        }

                        if (string.IsNullOrEmpty(dataStorage.Extension) && !string.IsNullOrEmpty(dataStorage.FileName) && dataStorage.FileName.Contains('.'))
                            dataStorage.Extension = Path.GetExtension(dataStorage.FileName);

                        dataStorage.Name = documentInput.Name;
                        dataStorage.Description = documentInput.Description;
                        dataStorage.FileName = StringUtility.ReplaceNonAscii(documentInput.FileName.RemoveÅÄÖ(), "_");
                        dataStorage.Extension = documentInput.Extension;
                        dataStorage.Folder = documentInput.Folder;

                        dataStorage.ValidFrom = documentInput.ValidFrom;
                        dataStorage.ValidTo = documentInput.ValidTo;
                        dataStorage.NeedsConfirmation = documentInput.NeedsConfirmation;

                        if (fileData != null)
                        {
                            dataStorage.Data = fileData;
                            dataStorage.FileSize = fileData.Length;
                        }

                        CompressStorage(entities, dataStorage, false, false);

                        #endregion

                        #region DataStorageRecord

                        // Get existing
                        List<int> existingMessageGroupIds = new List<int>();
                        if (dataStorage.DataStorageRecord != null)
                            existingMessageGroupIds = dataStorage.DataStorageRecord.Where(r => r.Type == (int)SoeDataStorageRecordType.MessageGroup).Select(r => r.RecordId).ToList();

                        // Delete removed
                        foreach (int messageGroupId in existingMessageGroupIds)
                        {
                            // Still exists
                            if (documentInput.MessageGroupIds != null && documentInput.MessageGroupIds.Contains(messageGroupId))
                                continue;

                            // Removed in input, delete it
                            DataStorageRecord dataStorageRecord = dataStorage.DataStorageRecord.FirstOrDefault(r => r.Type == (int)SoeDataStorageRecordType.MessageGroup && r.RecordId == messageGroupId);
                            if (dataStorageRecord != null)
                            {
                                foreach (DataStorageRecordRolePermission perm in dataStorageRecord.DataStorageRecordRolePermission.ToList())
                                {
                                    result = DeleteEntityItem(entities, perm, useBulkSaveChanges: false);
                                    if (!result.Success)
                                        return result;
                                }
                                result = DeleteEntityItem(entities, dataStorageRecord, useBulkSaveChanges: false);
                                if (!result.Success)
                                    return result;
                            }
                        }

                        // Add new
                        if (documentInput.MessageGroupIds != null)
                        {
                            foreach (int messageGroupId in documentInput.MessageGroupIds)
                            {
                                if (!existingMessageGroupIds.Contains(messageGroupId))
                                    CreateDataStorageRecord(entities, SoeDataStorageRecordType.MessageGroup, messageGroupId, null, SoeEntityType.MessageGroup, dataStorage);
                            }
                        }

                        #endregion

                        result = SaveChanges(entities);
                        if (result.Success)
                            result.IntegerValue = dataStorage.DataStorageId;

                        #endregion

                        if (result.Success)
                            transaction.Complete();
                    }
                }
                catch (Exception ex)
                {
                    result.Exception = ex;
                    base.LogError(ex, this.log);
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
            }

            return result;
        }

        public ActionResult DeleteDocument(int dataStorageId, int actorCompanyId)
        {
            ActionResult result = new ActionResult(true);

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        if (entities.Connection.State != ConnectionState.Open)
                            entities.Connection.Open();

                        #region DataStorage

                        DataStorage dataStorage = (from d in entities.DataStorage
                                                   .Include("Children")
                                                   .Include("DataStorageRecipient")
                                                   .Include("DataStorageRecord.DataStorageRecordRolePermission")
                                                   where d.DataStorageId == dataStorageId &&
                                                   d.ActorCompanyId == actorCompanyId &&
                                                   d.State == (int)SoeEntityState.Active
                                                   select d).FirstOrDefault();
                        if (dataStorage == null)
                            return new ActionResult((int)ActionResultSave.EntityNotFound, "DataStorage");

                        result = ChangeEntityState(dataStorage, SoeEntityState.Deleted);
                        if (result.Success)
                        {
                            foreach (DataStorage child in dataStorage.Children)
                            {
                                result = ChangeEntityState(child, SoeEntityState.Deleted);
                                if (!result.Success)
                                    return result;
                            }
                        }

                        #endregion

                        #region DataStorageRecipient

                        foreach (DataStorageRecipient recipient in dataStorage.DataStorageRecipient)
                        {
                            result = ChangeEntityState(recipient, SoeEntityState.Deleted);
                            if (!result.Success)
                                return result;
                        }

                        #endregion

                        #region DataStorageRecord

                        foreach (DataStorageRecord dataStorageRecord in dataStorage.DataStorageRecord.ToList())
                        {
                            foreach (DataStorageRecordRolePermission perm in dataStorageRecord.DataStorageRecordRolePermission.ToList())
                            {
                                result = DeleteEntityItem(entities, perm, useBulkSaveChanges: false);
                                if (!result.Success)
                                    return result;
                            }

                            result = DeleteEntityItem(entities, dataStorageRecord, useBulkSaveChanges: false);
                            if (!result.Success)
                                return result;
                        }

                        #endregion

                        result = SaveChanges(entities, transaction);
                        if (result.Success)
                            transaction.Complete();
                    }
                }
                catch (Exception ex)
                {
                    result.Exception = ex;
                    base.LogError(ex, this.log);
                }
                finally
                {
                    if (!result.Success)
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }
            }

            return result;
        }

        public ActionResult SetDocumentAsRead(int dataStorageId, int userId, bool confirmed)
        {
            ActionResult result = new ActionResult(true);

            using (CompEntities entities = new CompEntities())
            {

                bool isCreated = false;
                bool isModified = false;

                DataStorageRecipient recipient = (from d in entities.DataStorageRecipient
                                                  where d.DataStorageId == dataStorageId &&
                                                  d.UserId == userId &&
                                                  d.State == (int)SoeEntityState.Active
                                                  select d).FirstOrDefault();

                if (recipient == null)
                {
                    recipient = new DataStorageRecipient()
                    {
                        DataStorageId = dataStorageId,
                        UserId = UserId
                    };
                    SetCreatedProperties(recipient);
                    entities.DataStorageRecipient.AddObject(recipient);
                    isCreated = true;
                }

                DateTime time = DateTime.Now;
                if (!recipient.ReadDate.HasValue)
                {
                    recipient.ReadDate = time;
                    isModified = true;
                }
                if (confirmed && !recipient.ConfirmedDate.HasValue)
                {
                    recipient.ConfirmedDate = time;
                    isModified = true;
                }

                if (isModified && recipient.DataStorageRecipientId != 0)
                    SetModifiedProperties(recipient);

                if (isCreated || isModified)
                    result = SaveChanges(entities);

                result.DateTimeValue = time;
            }

            return result;
        }

        #endregion

        #region CompanyInformation

        public List<Information> GetCompanyInformations(int actorCompanyId, bool includeMessageGroups, bool includeRecipients, bool setNames)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            entitiesReadOnly.Information.NoTracking();
            IQueryable<Information> query = entitiesReadOnly.Information;

            if (includeMessageGroups)
                query = query.Include("InformationMessageGroup");
            if (includeRecipients)
                query = query.Include("InformationRecipient");

            List<Information> informations = (from i in query
                                              where i.ActorCompanyId == actorCompanyId &&
                                              i.State == (int)SoeEntityState.Active
                                              orderby i.Created descending
                                              select i).ToList();

            if (setNames)
            {
                List<GenericType> severities = GetTermGroupContent(TermGroup.InformationSeverity);
                foreach (Information information in informations)
                {
                    information.SeverityName = severities.FirstOrDefault(s => s.Id == information.Severity)?.Name;
                }
            }

            return informations;
        }

        public List<Information> GetCompanyInformationsForSendPushNotificationsJob(CompEntities entities, int? actorCompanyId, DateTime sendTime)
        {
            IQueryable<Information> query = entities.Information.Include("InformationMessageGroup");
            if (actorCompanyId.HasValue)
                query = query.Where(i => i.ActorCompanyId == actorCompanyId.Value && i.ShowInMobile && i.Notify && !i.NotificationSent.HasValue && i.State == (int)SoeEntityState.Active);
            else
                query = query.Where(i => i.ShowInMobile && i.Notify && !i.NotificationSent.HasValue && i.State == (int)SoeEntityState.Active);

            var informations = query.Where(i => i.ValidateSendPush(sendTime)).ToList();
            return informations;
        }

        public Information GetCompanyInformation(int informationId, int actorCompanyId, bool includeMessageGroups, bool includeRecipients)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Information.NoTracking();
            return GetCompanyInformation(entities, informationId, actorCompanyId, includeMessageGroups, includeRecipients);
        }

        public Information GetCompanyInformation(CompEntities entities, int informationId, int actorCompanyId, bool includeMessageGroups, bool includeRecipients)
        {
            IQueryable<Information> query = entities.Information;

            if (includeMessageGroups)
                query = query.Include("InformationMessageGroup");
            if (includeRecipients)
                query = query.Include("InformationRecipient");

            Information information = (from i in query
                                       where i.ActorCompanyId == actorCompanyId &&
                                       i.InformationId == informationId
                                       select i).FirstOrDefault();

            return information;
        }

        public List<string> GetCompanyInformationFolders(int actorCompanyId)
        {
            // TODO: Check folders that I'm permitted to see?

            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Information.NoTracking();
            return (from i in entities.Information
                    where i.ActorCompanyId == actorCompanyId &&
                    i.State == (int)SoeEntityState.Active &&
                    !(i.Folder == null || i.Folder.Equals(""))
                    select i.Folder).Distinct().ToList();
        }

        public bool CompanyInformationHasConfirmations(int informationId, int actorCompanyId)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            return entitiesReadOnly.InformationRecipient.Where(r => r.InformationId == informationId && r.Information.ActorCompanyId == actorCompanyId && r.ConfirmedDate.HasValue).Any();
        }

        public List<InformationRecipientDTO> GetCompanyInformationRecipientInfo(int informationId, int actorCompanyId, int roleId, int userId, bool setEmployeename)
        {
            List<InformationRecipientDTO> dtos = null;

            using (CompEntities entities = new CompEntities())
            {
                // Get message groups connected to information
                List<int> messageGroupIds = (from m in entities.InformationMessageGroup
                                             where m.InformationId == informationId &&
                                             m.State == (int)SoeEntityState.Active
                                             select m.MessageGroupId).ToList();

                // Get existing recipients (users who has read the information)
                List<InformationRecipient> recipients = (from r in entities.InformationRecipient.Include("User")
                                                         where r.InformationId == informationId &&
                                                         r.ReadDate.HasValue
                                                         select r).ToList();
                dtos = recipients.ToDTOs();

                // Add all other users permitted to see the information, but has not read it yet (have no InformationRecipient)
                List<User> validUsers = CommunicationManager.GetValidUsersForMessageGroups(entities, messageGroupIds, actorCompanyId, roleId, userId);
                foreach (User user in validUsers)
                {
                    if (!dtos.Select(d => d.UserId).ToList().Contains(user.UserId))
                    {
                        dtos.Add(new InformationRecipientDTO()
                        {
                            UserId = user.UserId,
                            UserName = user.LoginName
                        });
                    }
                }

                if (setEmployeename)
                {
                    var employees = (from e in entities.Employee
                                     where e.UserId.HasValue &&
                                     e.State == (int)SoeEntityState.Active
                                     select new { e.UserId, e.EmployeeNr, e.ContactPerson.FirstName, e.ContactPerson.LastName }).ToList();

                    foreach (InformationRecipientDTO dto in dtos)
                    {
                        var employee = employees.FirstOrDefault(e => e.UserId == dto.UserId);
                        if (employee != null)
                            dto.EmployeeNrAndName = String.Format("({0}) {1} {2}", employee.EmployeeNr, employee.FirstName, employee.LastName);
                    }
                }
            }

            if (setEmployeename)
                return dtos.OrderBy(d => d.EmployeeNrAndName).ToList();
            else
                return dtos.OrderBy(d => d.UserName).ToList();
        }

        public ActionResult SaveCompanyInformation(InformationDTO informationInput, int actorCompanyId)
        {
            #region Prereq

            if (informationInput == null)
                return new ActionResult(false, (int)ActionResultSave.EntityIsNull, GetText(7044, "Felaktig inparameter"));
            if (informationInput.Subject.IsNullOrEmpty())
                return new ActionResult(false, (int)ActionResultSave.EntityIsNull, GetText(11907, "Nyheten måste ha en titel"));
            if (informationInput.ShortText.IsNullOrEmpty() && informationInput.Text.IsNullOrEmpty())
                return new ActionResult(false, (int)ActionResultSave.EntityIsNull, GetText(11908, "Nyheten måste ha innehåll"));

            #endregion

            ActionResult result = new ActionResult(true);

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    Information information = null;

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        if (entities.Connection.State != ConnectionState.Open)
                            entities.Connection.Open();

                        #region Perform

                        #region Information

                        if (informationInput.InformationId == 0)
                        {
                            information = new Information()
                            {
                                ActorCompanyId = actorCompanyId
                            };
                            SetCreatedProperties(information);
                            entities.Information.AddObject(information);
                        }
                        else
                        {
                            information = GetCompanyInformation(entities, informationInput.InformationId, actorCompanyId, true, false);
                            SetModifiedProperties(information);
                        }

                        information.LicenseId = informationInput.LicenseId;
                        information.SysLanguageId = informationInput.SysLanguageId;
                        information.Type = (int)informationInput.Type;
                        information.Severity = (int)informationInput.Severity;
                        information.Subject = informationInput.Subject.Left(100);
                        information.ShortText = informationInput.ShortText.Left(255);
                        information.Text = informationInput.Text;
                        information.PlainText = StringUtility.HTMLToText(informationInput.Text, true);
                        using (MD5 md5Hash = MD5.Create())
                        {
                            string hashSource = information.Subject + information.ShortText + information.Text;
                            information.TextHash = CryptographyUtility.GetMd5HashAsBytes(md5Hash, hashSource);
                        }
                        information.Folder = informationInput.Folder;
                        information.ValidFrom = informationInput.ValidFrom;
                        information.ValidTo = informationInput.ValidTo;
                        information.StickyType = (int)informationInput.StickyType;
                        information.NeedsConfirmation = informationInput.NeedsConfirmation;
                        information.ShowInWeb = informationInput.ShowInWeb;
                        information.ShowInMobile = informationInput.ShowInMobile;
                        information.ShowInTerminal = informationInput.ShowInTerminal;
                        information.Notify = informationInput.Notify;
                        information.NotificationSent = informationInput.NotificationSent;
                        information.ShowForAllUsers = ((informationInput.MessageGroupIds == null || !informationInput.MessageGroupIds.Any()) && (informationInput.SysFeatureIds == null || !informationInput.SysFeatureIds.Any()));

                        #endregion

                        #region Message groups

                        // Get existing
                        List<int> existingMessageGroupIds = new List<int>();
                        if (information.InformationMessageGroup != null)
                            existingMessageGroupIds = information.InformationMessageGroup.Where(m => m.State == (int)SoeEntityState.Active).Select(m => m.MessageGroupId).ToList();

                        // Delete removed
                        foreach (int messageGroupId in existingMessageGroupIds)
                        {
                            // Still exists
                            if (informationInput.MessageGroupIds != null && informationInput.MessageGroupIds.Contains(messageGroupId))
                                continue;

                            // Removed in input, delete it
                            InformationMessageGroup messageGroup = information.InformationMessageGroup.FirstOrDefault(m => m.State == (int)SoeEntityState.Active && m.MessageGroupId == messageGroupId);
                            if (messageGroup != null)
                            {
                                ChangeEntityState(messageGroup, SoeEntityState.Deleted);
                                SetModifiedProperties(messageGroup);
                            }
                        }

                        // Add new
                        if (informationInput.MessageGroupIds != null)
                        {
                            foreach (int messageGroupId in informationInput.MessageGroupIds)
                            {
                                if (!existingMessageGroupIds.Contains(messageGroupId))
                                {
                                    InformationMessageGroup newMessageGroup = new InformationMessageGroup()
                                    {
                                        MessageGroupId = messageGroupId
                                    };
                                    SetCreatedProperties(newMessageGroup);
                                    information.InformationMessageGroup.Add(newMessageGroup);
                                }
                            }
                        }

                        #endregion

                        result = SaveChanges(entities, transaction);

                        #endregion

                        //Commit transaction
                        if (result.Success)
                        {
                            result.IntegerValue = information.InformationId;
                            transaction.Complete();
                        }
                    }

                    if (result.Success && information != null && information.ValidateSendPush(DateTime.Now))
                    {
                        SendPushNotification(entities, information);
                    }
                }
                catch (Exception ex)
                {
                    result.Exception = ex;
                    base.LogError(ex, this.log);
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
            }

            return result;
        }

        public ActionResult SendPushNotification(CompEntities entities, Information information)
        {
            bool notificationSent = CommunicationManager.SendInformationPushNotification(information, null);
            if (notificationSent)
            {
                information.NotificationSent = DateTime.Now;
                return SaveChanges(entities);
            }
            return new ActionResult(false);
        }

        public ActionResult DeleteCompanyInformation(List<int> informationIds, int actorCompanyId)
        {
            #region Prereq

            if (informationIds == null || informationIds.Count == 0)
                return new ActionResult(false, (int)ActionResultDelete.EntityIsNull, GetText(7044, "Felaktig inparameter"));

            #endregion

            ActionResult result = new ActionResult();

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        if (entities.Connection.State != ConnectionState.Open)
                            entities.Connection.Open();

                        #region Perform

                        foreach (int informationId in informationIds)
                        {
                            Information item = GetCompanyInformation(entities, informationId, actorCompanyId, false, false);
                            if (item != null)
                                ChangeEntityState(entities, item, SoeEntityState.Deleted, false);
                        }

                        SaveChanges(entities, transaction);

                        #endregion

                        //Commit transaction
                        if (result.Success)
                            transaction.Complete();
                    }
                }
                catch (Exception ex)
                {
                    result.Exception = ex;
                    base.LogError(ex, this.log);
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
            }

            return result;
        }

        public ActionResult DeleteCompanyInformationNotificationSent(int informationId, int actorCompanyId)
        {
            ActionResult result = new ActionResult(true);

            using (CompEntities entities = new CompEntities())
            {
                Information information = GetCompanyInformation(entities, informationId, actorCompanyId, false, false);
                if (information != null)
                {
                    information.NotificationSent = null;
                    SetModifiedProperties(information);
                    result = SaveChanges(entities);
                }
            }

            return result;
        }

        #endregion

        #region SysInformation

        public List<SysInformation> GetSysInformationsForSendPushNotificationsJob(SOESysEntities entities, DateTime sendTime)
        {
            List<SysInformation> informations = (from i in entities.SysInformation.Include("SysInformationSysCompDb")
                                                 where i.ShowInMobile && i.Notify && !i.NotificationSent.HasValue &&
                                                 i.State == (int)SoeEntityState.Active
                                                 select i).ToList();

            informations = informations.Where(i => i.ValidateSendPush(sendTime)).ToList();
            return informations;
        }

        public List<SysInformation> GetSysInformations(bool includeFeatures, bool includeSysCompDb, bool setNames)
        {
            using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
            IQueryable<SysInformation> query = sysEntitiesReadOnly.Set<SysInformation>();

            if (includeFeatures)
                query = query.Include("SysInformationFeature");
            if (includeSysCompDb)
                query = query.Include("SysInformationSysCompDb");

            List<SysInformation> informations = (from i in query
                                                 where i.State == (int)SoeEntityState.Active
                                                 orderby i.Created descending
                                                 select i).ToList();

            if (setNames)
            {
                List<GenericType> severities = GetTermGroupContent(TermGroup.InformationSeverity);
                foreach (SysInformation information in informations)
                {
                    information.SeverityName = severities.FirstOrDefault(s => s.Id == information.Severity)?.Name;
                }
            }

            return informations;
        }

        public SysInformation GetSysInformation(int sysInformationId, bool includeFeatures, bool includeSysCompDb, bool setSiteName = false)
        {
            using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
            return GetSysInformation(sysEntitiesReadOnly, sysInformationId, includeFeatures, includeSysCompDb, setSiteName);
        }

        public SysInformation GetSysInformation(SOESysEntities entities, int sysInformationId, bool includeFeatures, bool includeSysCompDb, bool setSiteName = false)
        {
            IQueryable<SysInformation> query = entities.Set<SysInformation>();

            if (includeFeatures)
                query = query.Include("SysInformationFeature");
            if (includeSysCompDb || setSiteName)
                query = query.Include("SysInformationSysCompDb");

            SysInformation information = (from i in query
                                          where i.SysInformationId == sysInformationId
                                          select i).FirstOrDefault();

            if (setSiteName && information.SysInformationSysCompDb != null)
            {
                List<SmallGenericType> sysCompDbs = GetSysInformationSysCompDbs(entities);
                foreach (SysInformationSysCompDb compDb in information.SysInformationSysCompDb)
                {
                    compDb.SiteName = sysCompDbs.FirstOrDefault(c => c.Id == compDb.SysCompDbId)?.Name ?? String.Empty;
                }
            }

            return information;
        }

        public List<string> GetSysInformationFolders()
        {
            using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
            return (from i in sysEntitiesReadOnly.SysInformation
                    where i.State == (int)SoeEntityState.Active &&
                    !(i.Folder == null || i.Folder.Equals(""))
                    select i.Folder).Distinct().ToList();
        }

        public List<SmallGenericType> GetSysInformationSysCompDbs()
        {
            using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
            return GetSysInformationSysCompDbs(sysEntitiesReadOnly);
        }

        public List<SmallGenericType> GetSysInformationSysCompDbs(SOESysEntities entities)
        {
            var sysCompDbsOfSameType = ConfigurationSetupUtil.GetSysCompDbsOfSameType();

            var sites = (from s in sysCompDbsOfSameType
                         orderby s.Name
                         select new
                         {
                             s.SysCompDbId,
                             s.Name,
                             s.Description
                         }).ToList();

            List<SmallGenericType> items = new List<SmallGenericType>();
            foreach (var site in sites)
            {
                string name = site.Name;
                if (!String.IsNullOrEmpty(site.Description))
                    name += String.Format(" ({0})", site.Description);

                items.Add(new SmallGenericType(site.SysCompDbId, name));
            }

            return items;
        }

        public List<SmallGenericType> GetSysInformationSysFeatures()
        {
            List<SmallGenericType> items = new List<SmallGenericType>();

            using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
            List<SysFeature> allSysFeatures = (from f in sysEntitiesReadOnly.SysFeature
                                               where !f.Inactive
                                               select f).ToList();


            // ParentFeatureId does not exist in model, navigation property is SysFeature2
            List<SysFeature> roots = (from f in allSysFeatures
                                      where f.SysFeature2 == null
                                      orderby f.Order
                                      select f).ToList();

            foreach (SysFeature root in roots)
            {
                string name = GetText(root.SysTermId, root.SysTermGroupId);
                items.Add(new SmallGenericType(root.SysFeatureId, name));
                List<SmallGenericType> childItems = GetSysInformationSysFeaturesChildren(allSysFeatures, root, name);
                if (childItems.Any())
                    items.AddRange(childItems);
            }

            return items;
        }

        private List<SmallGenericType> GetSysInformationSysFeaturesChildren(List<SysFeature> allSysFeatures, SysFeature sysFeature, string parentName)
        {
            List<SmallGenericType> items = new List<SmallGenericType>();

            List<SysFeature> children = (from f in allSysFeatures
                                         where f.SysFeature2 != null && f.SysFeature2.SysFeatureId == sysFeature.SysFeatureId
                                         orderby f.Order
                                         select f).ToList();

            foreach (SysFeature child in children)
            {
                string name = parentName + " | " + GetText(child.SysTermId, child.SysTermGroupId);
                items.Add(new SmallGenericType(child.SysFeatureId, name));
                List<SmallGenericType> childItems = GetSysInformationSysFeaturesChildren(allSysFeatures, child, name);
                if (childItems.Any())
                    items.AddRange(childItems);
            }

            return items;
        }

        public bool SysInformationHasConfirmations(int sysInformationId)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            return entitiesReadOnly.InformationRecipient.Where(r => r.SysInformationId == sysInformationId && r.ConfirmedDate.HasValue).Any();
        }

        public List<InformationRecipientDTO> GetSysInformationRecipientInfo(int sysInformationId, int userId, bool setEmployeename)
        {
            int? sysCompDbId = SysServiceManager.GetSysCompDBId();
            List<InformationRecipientDTO> dtos = new List<InformationRecipientDTO>();
            if (!sysCompDbId.HasValue)
                return dtos;

            using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
            List<int> sysCompDbIds = (from m in sysEntitiesReadOnly.SysInformationSysCompDb
                                      where m.SysInformationId == sysInformationId
                                      select m.SysCompDbId).ToList();

            if (sysCompDbIds.Any() && !sysCompDbIds.Contains(sysCompDbId.Value))
                return dtos;

            // Get sys features connected to information
            List<int> sysFeatureIds = (from m in sysEntitiesReadOnly.SysInformationFeature
                                       where m.SysInformationId == sysInformationId
                                       select m.SysFeatureId).ToList();

            using (CompEntities entities = new CompEntities())
            {
                // Get existing recipients (users who has read the information)
                List<InformationRecipient> recipients = (from r in entities.InformationRecipient.Include("User")
                                                         where r.SysInformationId == sysInformationId &&
                                                         r.ReadDate.HasValue
                                                         select r).ToList();
                dtos = recipients.ToDTOs();

                // Add all other users permitted to see the information, but has not read it yet (have no InformationRecipient)
                List<User> users;
                if (sysFeatureIds.Any())
                {
                    List<int> roleIds = (from r in entities.RoleFeature
                                         where sysFeatureIds.Contains(r.SysFeatureId)
                                         select r.RoleId).ToList();

                    DateTime date = DateTime.Now;

                    users = (from ucr in entities.UserCompanyRole
                             where roleIds.Contains(ucr.RoleId) && ucr.User.State == (int)SoeEntityState.Active &&
                             ucr.State == (int)SoeEntityState.Active &&
                            (!ucr.DateFrom.HasValue || ucr.DateFrom <= date) &&
                            (!ucr.DateTo.HasValue || ucr.DateTo >= date)
                             select ucr.User).Distinct().ToList();
                }
                else
                {
                    users = (from u in entities.User
                             where u.State == (int)SoeEntityState.Active
                             select u).ToList();
                }

                foreach (User user in users)
                {
                    dtos.Add(new InformationRecipientDTO()
                    {
                        UserId = user.UserId,
                        UserName = user.LoginName
                    });
                }

                if (setEmployeename)
                {
                    var employees = (from e in entities.Employee
                                     where e.UserId.HasValue &&
                                     e.State == (int)SoeEntityState.Active
                                     select new { CompanyName = e.Company.Name, e.UserId, e.EmployeeNr, e.ContactPerson.FirstName, e.ContactPerson.LastName }).ToList();

                    foreach (InformationRecipientDTO dto in dtos)
                    {
                        var employee = employees.FirstOrDefault(e => e.UserId == dto.UserId);
                        if (employee != null)
                        {
                            dto.CompanyName = employee.CompanyName;
                            dto.EmployeeNrAndName = String.Format("({0}) {1} {2}", employee.EmployeeNr, employee.FirstName, employee.LastName);
                        }
                    }
                }
            }

            if (setEmployeename)
                return dtos.OrderBy(d => d.CompanyName).ThenBy(d => d.EmployeeNrAndName).ToList();
            else
                return dtos.OrderBy(d => d.UserName).ToList();
        }

        public List<User> GetSysInformationRecipients(int sysInformationId)
        {
            int? sysCompDbId = SysServiceManager.GetSysCompDBId();
            if (!sysCompDbId.HasValue)
                return new List<User>();

            using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
            List<int> sysCompDbIds = (from m in sysEntitiesReadOnly.SysInformationSysCompDb
                                      where m.SysInformationId == sysInformationId
                                      select m.SysCompDbId).ToList();

            if (sysCompDbIds.Any() && !sysCompDbIds.Contains(sysCompDbId.Value))
                return new List<User>();

            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            // Get sys features connected to information
            List<int> sysFeatureIds = (from m in sysEntitiesReadOnly.SysInformationFeature
                                       where m.SysInformationId == sysInformationId
                                       select m.SysFeatureId).ToList();

            if (sysFeatureIds.Any())
            {
                List<int> roleIds = (from r in entitiesReadOnly.RoleFeature
                                     where sysFeatureIds.Contains(r.SysFeatureId)
                                     select r.RoleId).ToList();

                DateTime date = DateTime.Today;
                return (from ucr in entitiesReadOnly.UserCompanyRole
                        where roleIds.Contains(ucr.RoleId) &&
                        ucr.State == (int)SoeEntityState.Active &&
                        (!ucr.DateFrom.HasValue || ucr.DateFrom <= date) &&
                        (!ucr.DateTo.HasValue || ucr.DateTo >= date)
                        select ucr.User).Distinct().ToList();
            }
            else
            {
                return (from u in entitiesReadOnly.User
                        where u.State == (int)SoeEntityState.Active
                        select u).ToList();
            }
        }

        public ActionResult SaveSysInformation(InformationDTO informationInput)
        {
            #region Prereq

            if (informationInput == null)
                return new ActionResult(false, (int)ActionResultSave.EntityIsNull, GetText(7044, "Felaktig inparameter"));
            if (informationInput.Subject.IsNullOrEmpty())
                return new ActionResult(false, (int)ActionResultSave.EntityIsNull, GetText(11907, "Nyheten måste ha en titel"));
            if (informationInput.ShortText.IsNullOrEmpty() && informationInput.Text.IsNullOrEmpty())
                return new ActionResult(false, (int)ActionResultSave.EntityIsNull, GetText(11908, "Nyheten måste ha innehåll"));

            #endregion

            ActionResult result = new ActionResult(true);

            using (SOESysEntities entities = new SOESysEntities())
            {
                try
                {
                    SysInformation information = null;
                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        if (entities.Connection.State != ConnectionState.Open)
                            entities.Connection.Open();

                        #region Perform

                        #region SysInformation                        

                        if (informationInput.InformationId == 0)
                        {
                            information = new SysInformation()
                            {
                            };
                            SetCreatedPropertiesOnEntity(information);
                            entities.SysInformation.Add(information);
                        }
                        else
                        {
                            information = GetSysInformation(entities, informationInput.InformationId, true, true);
                            SetModifiedPropertiesOnEntity(information);
                        }

                        information.SysLanguageId = informationInput.SysLanguageId;
                        information.Type = (int)informationInput.Type;
                        information.Severity = (int)informationInput.Severity;
                        information.Subject = informationInput.Subject.Left(100);
                        information.ShortText = informationInput.ShortText.Left(255);
                        information.Text = informationInput.Text;
                        information.PlainText = StringUtility.HTMLToText(informationInput.Text, true);
                        using (MD5 md5Hash = MD5.Create())
                        {
                            string hashSource = information.Subject + information.ShortText + information.Text;
                            information.TextHash = CryptographyUtility.GetMd5HashAsBytes(md5Hash, hashSource);
                        }
                        information.Folder = informationInput.Folder;
                        information.ValidFrom = informationInput.ValidFrom;
                        information.ValidTo = informationInput.ValidTo;
                        information.StickyType = (int)informationInput.StickyType;
                        information.NeedsConfirmation = informationInput.NeedsConfirmation;
                        information.ShowInWeb = informationInput.ShowInWeb;
                        information.ShowInMobile = informationInput.ShowInMobile;
                        information.ShowInTerminal = informationInput.ShowInTerminal;
                        information.Notify = informationInput.Notify;
                        information.NotificationSent = informationInput.NotificationSent;
                        information.ShowOnAllFeatures = (informationInput.SysFeatureIds == null || !informationInput.SysFeatureIds.Any());

                        #endregion

                        #region SysCompDB

                        // Get existing
                        List<int> existingSysCompDbIds = new List<int>();
                        if (information.SysInformationSysCompDb != null)
                            existingSysCompDbIds = information.SysInformationSysCompDb.Select(f => f.SysCompDbId).ToList();

                        // Delete removed
                        foreach (int sysCompDbId in existingSysCompDbIds)
                        {
                            // Still exists
                            if (informationInput.SysCompDbIds != null && informationInput.SysCompDbIds.Contains(sysCompDbId))
                                continue;

                            // Removed in input, delete it
                            SysInformationSysCompDb sysCompDb = information.SysInformationSysCompDb.FirstOrDefault(f => f.SysCompDbId == sysCompDbId);
                            if (sysCompDb != null)
                                entities.SysInformationSysCompDb.Remove(sysCompDb);
                        }

                        // Add new
                        if (informationInput.SysCompDbIds != null)
                        {
                            foreach (int sysCompDbId in informationInput.SysCompDbIds)
                            {
                                if (!existingSysCompDbIds.Contains(sysCompDbId))
                                {
                                    SysInformationSysCompDb newSysCompDb = new SysInformationSysCompDb()
                                    {
                                        SysCompDbId = sysCompDbId
                                    };
                                    information.SysInformationSysCompDb.Add(newSysCompDb);
                                }
                            }
                        }

                        // If no site was selected, add all sites.
                        // Need to have SysInformationSysCompDb entities for all sites to use NotificationSent.
                        List<SmallGenericType> sysCompDbs = GetSysInformationSysCompDbs(entities);
                        if (information.SysInformationSysCompDb == null)
                            information.SysInformationSysCompDb = new EntityCollection<SysInformationSysCompDb>();
                        if (information.SysInformationSysCompDb.Count == 0)
                        {
                            foreach (SmallGenericType sysCompDb in sysCompDbs)
                            {
                                information.SysInformationSysCompDb.Add(new SysInformationSysCompDb() { SysCompDbId = sysCompDb.Id });
                            }
                            information.ShowOnAllSysCompDbs = true;
                        }
                        else
                        {
                            information.ShowOnAllSysCompDbs = information.SysInformationSysCompDb.Count == sysCompDbs.Count;
                        }

                        if (information.NotificationSent.HasValue && information.SysInformationSysCompDb.Any(x => !x.NotificationSent.HasValue))
                            information.NotificationSent = null;

                        #endregion

                        #region SysFeatures

                        // Get existing
                        List<int> existingSysFeatureIds = new List<int>();
                        if (information.SysInformationFeature != null)
                            existingSysFeatureIds = information.SysInformationFeature.Where(f => f.State == (int)SoeEntityState.Active).Select(f => f.SysFeatureId).ToList();

                        // Delete removed
                        foreach (int sysFeatureId in existingSysFeatureIds)
                        {
                            // Still exists
                            if (informationInput.SysFeatureIds != null && informationInput.SysFeatureIds.Contains(sysFeatureId))
                                continue;

                            // Removed in input, delete it
                            SysInformationFeature sysFeature = information.SysInformationFeature.FirstOrDefault(f => f.State == (int)SoeEntityState.Active && f.SysFeatureId == sysFeatureId);
                            if (sysFeature != null)
                            {
                                ChangeEntityStateOnEntity(sysFeature, SoeEntityState.Deleted);
                                SetModifiedPropertiesOnEntity(sysFeature);
                            }
                        }

                        // Add new
                        if (informationInput.SysFeatureIds != null)
                        {
                            foreach (int sysFeatureId in informationInput.SysFeatureIds)
                            {
                                if (!existingSysFeatureIds.Contains(sysFeatureId))
                                {
                                    SysInformationFeature newSysFeature = new SysInformationFeature()
                                    {
                                        SysFeatureId = sysFeatureId
                                    };
                                    SetCreatedPropertiesOnEntity(newSysFeature);
                                    information.SysInformationFeature.Add(newSysFeature);
                                }
                            }
                        }

                        #endregion

                        result = SaveChanges(entities, transaction);

                        #endregion

                        //Commit transaction
                        if (result.Success)
                        {
                            result.IntegerValue = information.SysInformationId;
                            transaction.Complete();
                        }
                    }

                    if (result.Success && information != null && information.ValidateSendPush(DateTime.Now))
                    {
                        SendPushNotification(entities, information);
                    }
                }
                catch (Exception ex)
                {
                    result.Exception = ex;
                    base.LogError(ex, this.log);
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
            }

            return result;
        }

        public ActionResult SendPushNotification(SOESysEntities entities, SysInformation sysInformation)
        {
            DateTime now = DateTime.Now;
            int? sysCompDbId = SysServiceManager.GetSysCompDBId();
            if (sysCompDbId.HasValue)
            {
                SysInformationSysCompDb compDb = sysInformation.SysInformationSysCompDb.FirstOrDefault(x => x.SysCompDbId == sysCompDbId);
                if (compDb != null && !compDb.NotificationSent.HasValue)
                {
                    bool notificationSent = CommunicationManager.SendInformationPushNotification(null, sysInformation);
                    if (notificationSent)
                    {
                        // Notification sent on current site
                        compDb.NotificationSent = now;
                    }
                }
            }

            if (!sysInformation.SysInformationSysCompDb.Any(x => !x.NotificationSent.HasValue))
                sysInformation.NotificationSent = now;

            return SaveChanges(entities);
        }

        public ActionResult DeleteSysInformation(List<int> sysInformationIds)
        {
            #region Prereq

            if (sysInformationIds == null || sysInformationIds.Count == 0)
                return new ActionResult(false, (int)ActionResultDelete.EntityIsNull, GetText(7044, "Felaktig inparameter"));

            #endregion

            ActionResult result = new ActionResult();

            using (SOESysEntities entities = new SOESysEntities())
            {
                try
                {
                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        if (entities.Connection.State != ConnectionState.Open)
                            entities.Connection.Open();

                        #region Perform

                        foreach (int sysInformationId in sysInformationIds)
                        {
                            SysInformation item = GetSysInformation(entities, sysInformationId, false, false);
                            if (item != null)
                                ChangeEntityStateOnEntity(entities, item, SoeEntityState.Deleted, false);
                        }

                        SaveChanges(entities, transaction);

                        #endregion

                        //Commit transaction
                        if (result.Success)
                            transaction.Complete();
                    }
                }
                catch (Exception ex)
                {
                    result.Exception = ex;
                    base.LogError(ex, this.log);
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
            }

            return result;
        }

        public ActionResult DeleteSysInformationNotificationSent(int sysInformationId, int sysCompDbId)
        {
            ActionResult result = new ActionResult(true);

            using (SOESysEntities entities = new SOESysEntities())
            {
                SysInformation information = GetSysInformation(entities, sysInformationId, false, true);
                if (information != null)
                {
                    information.NotificationSent = null;
                    SetModifiedPropertiesOnEntity(information);

                    if (sysCompDbId != 0)
                    {
                        // Delete one
                        SysInformationSysCompDb compDb = information.SysInformationSysCompDb.FirstOrDefault(c => c.SysCompDbId == sysCompDbId);
                        if (compDb != null)
                            compDb.NotificationSent = null;
                    }
                    else
                    {
                        // Delete all
                        foreach (SysInformationSysCompDb compDb in information.SysInformationSysCompDb)
                        {
                            compDb.NotificationSent = null;
                        }
                    }

                    result = SaveChanges(entities);
                }
            }

            return result;
        }

        #endregion

        #region Information

        public bool HasNewInformations(int actorCompanyId, DateTime time)
        {
            return HasNewCompanyInformations(actorCompanyId, time) || HasNewSysInformations(time);
        }

        private bool HasNewCompanyInformations(int actorCompanyId, DateTime time)
        {
            string key = $"HasNewCompanyInformations{actorCompanyId}";

            DateTime? createdOrModified = BusinessMemoryCache<DateTime?>.Get(key);
            if (createdOrModified.HasValue && createdOrModified.Value > time)
                return true;

            if (createdOrModified.HasValue && createdOrModified.Value == CalendarUtility.DATETIME_DEFAULT)
                return false;

            DateTime now = DateTime.Now;
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            entitiesReadOnly.Information.NoTracking();

            List<InformationDTO> informations = (from i in entitiesReadOnly.Information
                                                 where i.ActorCompanyId == actorCompanyId &&
                                                 (i.Created > time || (i.Modified != null && i.Modified > time) || (i.ValidFrom != null && i.ValidFrom.Value <= now && i.ValidFrom.Value > time)) &&
                                                 i.State == (int)SoeEntityState.Active &&
                                                 (i.ValidFrom == null || i.ValidFrom.Value <= now) &&
                                                 (i.ValidTo == null || i.ValidTo.Value >= now)
                                                 select new InformationDTO() { Created = i.Created, Modified = i.Modified, ValidFrom = i.ValidFrom }).ToList();

            createdOrModified = !informations.IsNullOrEmpty() ? informations.OrderByDescending(i => i.CreatedOrModified).First().CreatedOrModified : CalendarUtility.DATETIME_DEFAULT;
            BusinessMemoryCache<DateTime?>.Set(key, createdOrModified, 90);

            return createdOrModified.Value > time;
        }

        private bool HasNewSysInformations(DateTime time)
        {
            string key = $"HasNewSysInformations";

            DateTime? createdOrModified = BusinessMemoryCache<DateTime?>.Get(key);
            if (createdOrModified.HasValue && createdOrModified.Value > time)
                return true;

            if (createdOrModified.HasValue && createdOrModified.Value == CalendarUtility.DATETIME_DEFAULT)
                return false;

            DateTime now = DateTime.Now;

            using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
            List<InformationDTO> informations = (from i in sysEntitiesReadOnly.SysInformation
                                                 where (i.Created > time || (i.Modified != null && i.Modified > time) || (i.ValidFrom != null && i.ValidFrom.Value <= now && i.ValidFrom.Value > time)) &&
                                                 i.State == (int)SoeEntityState.Active &&
                                                 (i.ValidFrom == null || i.ValidFrom.Value <= now) &&
                                                 (i.ValidTo == null || i.ValidTo.Value >= now)
                                                 select new InformationDTO() { Created = i.Created, Modified = i.Modified, ValidFrom = i.ValidFrom }).ToList();

            createdOrModified = !informations.IsNullOrEmpty() ? informations.OrderByDescending(i => i.CreatedOrModified).First().CreatedOrModified : CalendarUtility.DATETIME_DEFAULT;
            BusinessMemoryCache<DateTime?>.Set(key, createdOrModified, 90);

            return createdOrModified.Value > time;
        }

        public List<InformationDTO> GetUnreadInformations(int licenseId, int actorCompanyId, int roleId, int userId, bool showInWeb, bool showInMobile, bool showInTerminal, int langId, bool useCache = true)
        {
            // Sys
            List<InformationDTO> informations = GetSysInformations(licenseId, actorCompanyId, roleId, userId, showInWeb, showInMobile, showInTerminal, langId, useCache);

            // Comp
            informations.AddRange(GetCompanyInformations(actorCompanyId, roleId, userId, showInWeb, showInMobile, showInTerminal, langId, useCache: useCache));

            // Information counts as unread if user has not read it or if marked as emergency, user has not confirmed it.
            return informations.Where(i => !i.ReadDate.HasValue || (i.NeedsConfirmation && !i.AnswerDate.HasValue)).ToList();
        }

        public bool HasSevereUnreadInformation(int licenseId, int actorCompanyId, int roleId, int userId, bool showInWeb, bool showInMobile, bool showInTerminal, int langId)
        {
            List<InformationDTO> informations = GetUnreadInformations(licenseId, actorCompanyId, roleId, userId, showInWeb, showInMobile, showInTerminal, langId);
            return informations.Any(i => i.Severity == TermGroup_InformationSeverity.Emergency);
        }

        public List<InformationDTO> GetCompanyInformations(int actorCompanyId, int roleId, int userId, bool showInWeb, bool showInMobile, bool showInTerminal, int langId, bool useCache = true)
        {
            List<int> validInformationIds = GetAllValidInformationIds(actorCompanyId, roleId, userId);
            if (!validInformationIds.Any())
                return new List<InformationDTO>();

            DateTime now = DateTime.Now;
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            entitiesReadOnly.Information.NoTracking();
            entitiesReadOnly.InformationRecipient.NoTracking();
            int? lastInDB = entitiesReadOnly.Information.OrderByDescending(o => o.InformationId).Take(1).Select(s => s.InformationId).FirstOrDefault();
            string key = $"GetCompanyInformations{lastInDB}|{actorCompanyId}|{validInformationIds.JoinToString("#")}";
            List<InformationDTO> informationDTOs = !useCache ? null : BusinessMemoryCache<List<InformationDTO>>.Get(key);

            if (informationDTOs != null)
                return informationDTOs.CloneDTOs();

            List<Information> informations = (from i in entitiesReadOnly.Information.Include("InformationRecipient")
                                              where i.ActorCompanyId == actorCompanyId &&
                                              (langId == 0 || i.SysLanguageId == langId) &&
                                              i.State == (int)SoeEntityState.Active &&
                                              (i.ValidFrom == null || i.ValidFrom.Value <= now) &&
                                              (i.ValidTo == null || i.ValidTo.Value >= now) &&
                                              ((showInWeb && i.ShowInWeb) || (showInMobile && i.ShowInMobile) || (showInTerminal && i.ShowInTerminal)) &&
                                              validInformationIds.Contains(i.InformationId)
                                              orderby i.Folder, i.Subject, i.Created descending
                                              select i).ToList();


            List<InformationDTO> dtos = informations.ToDTOs(false, userId);

            List<GenericType> severities = GetTermGroupContent(TermGroup.InformationSeverity);
            foreach (InformationDTO dto in dtos)
            {
                dto.SourceType = SoeInformationSourceType.Company;
                dto.SeverityName = severities.FirstOrDefault(s => s.Id == (int)dto.Severity)?.Name;

                // Should only be max one left here
                InformationRecipientDTO recipient = dto.Recipients.FirstOrDefault(r => r.UserId == userId);
                if (recipient != null)
                {
                    dto.ReadDate = recipient.ReadDate;
                    dto.AnswerDate = recipient.ConfirmedDate;
                    dto.AnswerType = recipient.ConfirmedDate.HasValue ? XEMailAnswerType.Yes : XEMailAnswerType.None;
                }
            }

            BusinessMemoryCache<List<InformationDTO>>.Set(key, dtos.CloneDTOs());

            return dtos.CloneDTOs();
        }

        public InformationDTO GetCompanyInformation(int informationId, int actorCompanyId, int userId, bool includeRecipientInfo)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            IQueryable<Information> query = entitiesReadOnly.Information;
            if (includeRecipientInfo)
                query = query.Include("InformationRecipient");

            Information information = (from i in query
                                       where i.ActorCompanyId == actorCompanyId &&
                                       i.State == (int)SoeEntityState.Active &&
                                       i.InformationId == informationId
                                       select i).FirstOrDefault();
            if (information == null)
                return null;

            InformationDTO dto = information.ToDTO(true, userId);

            if (includeRecipientInfo)
            {
                InformationRecipientDTO recipient = dto.Recipients.FirstOrDefault(r => r.UserId == userId);
                if (recipient != null)
                {
                    dto.ReadDate = recipient.ReadDate;
                    dto.AnswerDate = recipient.ConfirmedDate;
                    dto.AnswerType = recipient.ConfirmedDate.HasValue ? XEMailAnswerType.Yes : XEMailAnswerType.None;
                }
            }

            return dto;
        }

        public List<InformationDTO> GetSysInformations(int licenseId, int actorCompanyId, int roleId, int userId, bool showInWeb, bool showInMobile, bool showInTerminal, int langId, bool useCache = false)
        {
            int? sysCompDbId = SysServiceManager.GetSysCompDBId();
            if (!sysCompDbId.HasValue)
                return new List<InformationDTO>();

            DateTime now = DateTime.Now;

            string key = $"GetSysInformations{sysCompDbId.Value}|{langId}|{roleId}|{actorCompanyId}|{licenseId}";

            List<InformationDTO> dtos = !useCache ? null : BusinessMemoryCache<List<InformationDTO>>.Get(key);

            if (dtos == null)
            {
                string keySysInformations = $"GetSysInformationsInformation{sysCompDbId.Value}|{langId}";
                using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
                List<SysInformation> informations = (!useCache ? null : BusinessMemoryCache<List<SysInformation>>.Get(keySysInformations)) ??
                                                    (from i in sysEntitiesReadOnly.SysInformation.Include("SysInformationFeature").Include("SysInformationSysCompDb")
                                                     where i.State == (int)SoeEntityState.Active &&
                                                     (langId == 0 || i.SysLanguageId == langId) &&
                                                     (i.ValidFrom == null || i.ValidFrom.Value <= now) &&
                                                     (i.ValidTo == null || i.ValidTo.Value >= now) &&
                                                     ((showInWeb && i.ShowInWeb) || (showInMobile && i.ShowInMobile) || (showInTerminal && i.ShowInTerminal)) &&
                                                     (i.ShowOnAllSysCompDbs || i.SysInformationSysCompDb.Any(s => s.SysCompDbId == sysCompDbId.Value))
                                                     orderby i.Folder, i.Subject, i.Created descending
                                                     select i).ToList();

                BusinessMemoryCache<List<SysInformation>>.Set(keySysInformations, informations);
                var filteredInformations = informations.ToList();

                // Check permissions
                foreach (SysInformation information in filteredInformations.Where(i => !i.ShowOnAllFeatures).ToList())
                {
                    List<Feature> features = information.SysInformationFeature.Where(f => f.State == (int)SoeEntityState.Active).Select(f => (Feature)f.SysFeatureId).ToList();
                    Dictionary<int, bool> permissions = FeatureManager.HasRolePermissions(features, Permission.Readonly, licenseId, actorCompanyId, roleId);
                    if (!permissions.Any(p => p.Value))
                        filteredInformations.Remove(information);
                }
                dtos = filteredInformations.ToDTOs(false);
                BusinessMemoryCache<List<InformationDTO>>.Set(key, dtos.CloneDTOs());
            }
            List<int> sysInformationIds = dtos.Select(i => i.InformationId).ToList();
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            entitiesReadOnly.InformationRecipient.NoTracking();
            List<InformationRecipient> recipients = (from r in entitiesReadOnly.InformationRecipient
                                                     where r.UserId == userId &&
                                                     r.SysInformationId.HasValue &&
                                                                                              sysInformationIds.Contains(r.SysInformationId.Value)
                                                     select r).ToList();

            List<GenericType> severities = GetTermGroupContent(TermGroup.InformationSeverity);
            foreach (InformationDTO dto in dtos)
            {
                dto.SourceType = SoeInformationSourceType.Sys;
                dto.SeverityName = severities.FirstOrDefault(s => s.Id == (int)dto.Severity)?.Name;

                InformationRecipient recipient = recipients.FirstOrDefault(r => r.SysInformationId == dto.InformationId);
                if (recipient != null)
                {
                    dto.ReadDate = recipient.ReadDate;
                    dto.AnswerDate = recipient.ConfirmedDate;
                    dto.AnswerType = recipient.ConfirmedDate.HasValue ? XEMailAnswerType.Yes : XEMailAnswerType.None;
                }
            }

            return dtos.CloneDTOs();
        }

        public InformationDTO GetSysInformation(int sysInformationId, bool includeRecipientInfo)
        {
            using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
            SysInformation information = (from i in sysEntitiesReadOnly.SysInformation
                                          where i.State == (int)SoeEntityState.Active &&
                                          i.SysInformationId == sysInformationId
                                          select i).FirstOrDefault();

            InformationDTO dto = information.ToDTO(true);

            if (includeRecipientInfo)
            {
                using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
                entitiesReadOnly.InformationRecipient.NoTracking();
                InformationRecipient recipient = (from r in entitiesReadOnly.InformationRecipient
                                                  where r.SysInformationId == sysInformationId &&
                                                  r.UserId == UserId
                                                  select r).FirstOrDefault();
                if (recipient != null)
                {
                    dto.ReadDate = recipient.ReadDate;
                    dto.AnswerDate = recipient.ConfirmedDate;
                    dto.AnswerType = recipient.ConfirmedDate.HasValue ? XEMailAnswerType.Yes : XEMailAnswerType.None;
                }
            }

            return dto;
        }

        private List<int> GetAllValidInformationIds(int actorCompanyId, int roleId, int userId)
        {
            List<int> validInformationIds = new List<int>();

            List<MessageGroup> validGroups = CommunicationManager.GetValidMessageGroupsForRecipient(actorCompanyId, userId, roleId);
            List<int> validGroupIds = validGroups.Select(m => m.MessageGroupId).ToList();
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            List<int> allInformationIds = entitiesReadOnly.Information.Where(i => i.ActorCompanyId == actorCompanyId && i.State == (int)SoeEntityState.Active).Select(i => i.InformationId).ToList();
            if (allInformationIds.Any())
            {
                List<int> allInformationIdWithGroups = entitiesReadOnly.InformationMessageGroup.Where(m => m.State == (int)SoeEntityState.Active && allInformationIds.Contains(m.InformationId)).Select(i => i.InformationId).ToList();
                List<int> informationIdsWithoutGroups = allInformationIds.Where(i => !allInformationIdWithGroups.Contains(i)).ToList();
                List<int> validInformationIdOnValidGroups = entitiesReadOnly.InformationMessageGroup.Where(m => m.State == (int)SoeEntityState.Active && m.Information.ActorCompanyId == actorCompanyId && validGroupIds.Contains(m.MessageGroupId)).Select(i => i.InformationId).ToList();

                validInformationIds.AddRange(informationIdsWithoutGroups);
                validInformationIds.AddRange(validInformationIdOnValidGroups);
            }

            return validInformationIds;
        }

        public ActionResult SetInformationAsRead(int informationId, int sysInformationId, int userId, bool confirmed, bool hidden)
        {
            ActionResult result = new ActionResult(true);

            using (CompEntities entities = new CompEntities())
            {

                bool isCreated = false;
                bool isModified = false;

                InformationRecipient recipient = (from d in entities.InformationRecipient
                                                  where ((informationId != 0 && d.InformationId == informationId) || (sysInformationId != 0 && d.SysInformationId == sysInformationId)) &&
                                                  d.UserId == userId
                                                  select d).FirstOrDefault();

                if (recipient == null)
                {
                    recipient = new InformationRecipient()
                    {
                        UserId = userId
                    };
                    if (informationId != 0)
                        recipient.InformationId = informationId;
                    else if (sysInformationId != 0)
                        recipient.SysInformationId = sysInformationId;

                    entities.InformationRecipient.AddObject(recipient);
                    isCreated = true;
                }

                DateTime time = DateTime.Now;
                if (!recipient.ReadDate.HasValue)
                {
                    recipient.ReadDate = time;
                    isModified = true;
                }
                if (confirmed && !recipient.ConfirmedDate.HasValue)
                {
                    recipient.ConfirmedDate = time;
                    isModified = true;
                }
                if (hidden && !recipient.HideDate.HasValue)
                {
                    recipient.HideDate = time;
                    isModified = true;
                }

                if (isCreated || isModified)
                    result = SaveChanges(entities);

                result.DateTimeValue = time;
            }

            return result;
        }

        #endregion

        #region TimeSalaryExport

        public List<DataStorageSmallDTO> GetTimeSalaryImportsByCompany(int actorCompanyId)
        {
            using var entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            return (from ds in entitiesReadOnly.DataStorage
                    where ds.ActorCompanyId == actorCompanyId &&
                    !ds.ParentDataStorageId.HasValue &&
                    !ds.EmployeeId.HasValue &&
                    (ds.Type == (int)SoeDataStorageRecordType.TimeSalaryExport || ds.Type == (int)SoeDataStorageRecordType.TimeSalaryExportControlInfo || ds.Type == (int)SoeDataStorageRecordType.TimeKU10Export) &&
                    ds.State == (int)SoeEntityState.Active
                    orderby ds.Created descending
                    select new DataStorageSmallDTO
                    {
                        DataStorageId = ds.DataStorageId,
                        ActorCompanyId = ds.ActorCompanyId,
                        ParentDataStorageId = ds.ParentDataStorageId,
                        EmployeeId = ds.EmployeeId,
                        Description = ds.Description,
                        Type = (SoeDataStorageRecordType)ds.Type,
                        TimePeriodId = ds.TimePeriodId,
                        TimePeriodName = ds.TimePeriod != null ? ds.TimePeriod.Name : String.Empty,
                        TimePeriodStartDate = ds.TimePeriod != null ? ds.TimePeriod.StartDate : CalendarUtility.DATETIME_DEFAULT,
                        TimePeriodStopDate = ds.TimePeriod != null ? ds.TimePeriod.StopDate : CalendarUtility.DATETIME_DEFAULT,
                        NoOfChildrens = ds.Children.Count,
                    }).ToList();
        }
        public List<DataStorageSmallDTO> GetEmployeeTimePeriodsForYear(int actorCompanyId, int employeeId, int year)
        {
            List<int> timePeriodsIds = new List<int>();
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            if (year != 0 && year != 9999)
                timePeriodsIds = TimePeriodManager.GetTimePeriodsWithPaymentDatesThisYear(entitiesReadOnly, actorCompanyId, year).Select(s => s.TimePeriodId).ToList();

            List<DataStorageSmallDTO> dtos = GeneralManager.GetTimeSalaryImportsByEmployee(employeeId, actorCompanyId, true, false, timePeriodsIds);

            return dtos;
        }
        public List<DataStorageSmallDTO> GetTimeSalaryImportsByEmployee(int employeeId, int actorCompanyId, bool includeControlInfo, bool includeXML, List<int> timePeriodIds)
        {
            bool hasTimePeriods = !timePeriodIds.IsNullOrEmpty();
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            entitiesReadOnly.EmployeeTimePeriod.NoTracking();

            List<EmployeeTimePeriod> employeeTimePeriods = (from etp in entitiesReadOnly.EmployeeTimePeriod
                                                            .Include("EmployeeTimePeriodValue")
                                                            where etp.EmployeeId == employeeId &&
                                                            (!hasTimePeriods || (timePeriodIds.Contains(etp.TimePeriodId))) &&
                                                            etp.State == (int)SoeEntityState.Active
                                                            select etp).ToList();

            bool publishPayrollSlipWhenLockingPeriod = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.PublishPayrollSlipWhenLockingPeriod, 0, actorCompanyId, 0);
            var dtos = (from ds in entitiesReadOnly.DataStorage
                        where ds.ActorCompanyId == actorCompanyId && (ds.EmployeeId.HasValue && ds.EmployeeId.Value == employeeId) &&
                        (ds.Type == (int)SoeDataStorageRecordType.TimeSalaryExportEmployee || ds.Type == (int)SoeDataStorageRecordType.TimeSalaryExportControlInfoEmployee || ds.Type == (int)SoeDataStorageRecordType.TimeKU10ExportEmployee || ds.Type == (int)SoeDataStorageRecordType.PayrollSlipXML) &&
                        ds.State == (int)SoeEntityState.Active &&
                        (!hasTimePeriods || (ds.TimePeriodId.HasValue && timePeriodIds.Contains(ds.TimePeriodId.Value)))
                        orderby ds.Created descending
                        select new DataStorageSmallDTO
                        {
                            DataStorageId = ds.DataStorageId,
                            ActorCompanyId = ds.ActorCompanyId,
                            ParentDataStorageId = ds.ParentDataStorageId,
                            EmployeeId = ds.EmployeeId,
                            Description = ds.Description,
                            Type = (SoeDataStorageRecordType)ds.Type,
                            TimePeriodId = ds.TimePeriodId,
                            TimePeriodName = ds.TimePeriod != null ? ds.TimePeriod.Name : String.Empty,
                            TimePeriodStartDate = ds.TimePeriod != null ? ds.TimePeriod.StartDate : CalendarUtility.DATETIME_DEFAULT,
                            TimePeriodStopDate = ds.TimePeriod != null ? ds.TimePeriod.StopDate : CalendarUtility.DATETIME_DEFAULT,
                            TimePeriodPaymentDate = ds.TimePeriod != null ? ds.TimePeriod.PaymentDate : null,
                            XML = ds.XML,
                        }).ToList();

            if (!includeControlInfo)
                dtos = dtos.Where(i => i.Type == SoeDataStorageRecordType.TimeSalaryExportEmployee || i.Type == SoeDataStorageRecordType.PayrollSlipXML).ToList();
            List<DataStorageSmallDTO> validDTOs = new List<DataStorageSmallDTO>();

            foreach (var dto in dtos)
            {
                dto.Year = dto.TimePeriodPaymentDate.HasValue && dto.Type == SoeDataStorageRecordType.PayrollSlipXML ? dto.TimePeriodPaymentDate.Value.Year : 9999;

                if (dto.TimePeriodId.HasValue && dto.Type == SoeDataStorageRecordType.PayrollSlipXML)
                {
                    var employeeTimePeriod = employeeTimePeriods.FirstOrDefault(f => f.TimePeriodId == dto.TimePeriodId.Value);
                    if (employeeTimePeriod != null)
                    {
                        if (!publishPayrollSlipWhenLockingPeriod)
                        {
                            if (employeeTimePeriod.Status != (int)SoeEmployeeTimePeriodStatus.Paid)
                                continue;

                            if (employeeTimePeriod.SalarySpecificationPublishDate.HasValue && employeeTimePeriod.SalarySpecificationPublishDate.Value.Date > DateTime.Now.Date)
                                continue;
                        }
                        dto.NetSalary = employeeTimePeriod.GetNetSum();
                    }
                }

                if (includeXML)
                {
                    if (String.IsNullOrEmpty(dto.XML))
                        continue;

                    XDocument xdoc = XDocument.Parse(dto.XML);
                    if (xdoc == null)
                        continue;

                    DateTime? payedDate = TimeSalaryManager.GetPayedDate(xdoc);
                    if (payedDate.HasValue)
                        dto.XMLValue1 = payedDate.Value.ToString(CalendarUtility.SHORTDATEMASK);
                }

                if (String.IsNullOrEmpty(dto.XMLValue1) && (dto.Type == SoeDataStorageRecordType.TimeSalaryExportControlInfoEmployee || dto.Type == SoeDataStorageRecordType.TimeKU10ExportEmployee))
                    dto.XMLValue1 = GetText(5912, "Kontrolluppgift");

                validDTOs.Add(dto);
            }

            return validDTOs;
        }

        public List<DataStorageAllDTO> GetTimeSalaryImportsByEmployee(int employeeId, int actorCompanyId, bool includeControlInfo, bool includeXML, bool includeData)  //1.4. Jukka
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            entitiesReadOnly.EmployeeTimePeriod.NoTracking();
            List<EmployeeTimePeriod> employeeTimePeriods = entitiesReadOnly.EmployeeTimePeriod.Where(w => w.EmployeeId == employeeId && w.State == (int)SoeEntityState.Active).ToList();

            var dtos = (from ds in entitiesReadOnly.DataStorage
                        where ds.ActorCompanyId == actorCompanyId &&
                        (ds.EmployeeId.HasValue && ds.EmployeeId.Value == employeeId) &&
                        (ds.Type == (int)SoeDataStorageRecordType.TimeSalaryExportEmployee || ds.Type == (int)SoeDataStorageRecordType.TimeSalaryExportControlInfoEmployee || ds.Type == (int)SoeDataStorageRecordType.TimeKU10ExportEmployee || ds.Type == (int)SoeDataStorageRecordType.TimeSalaryExportSaumaPdf) &&
                        ds.State == (int)SoeEntityState.Active
                        orderby ds.Created descending
                        select new DataStorageAllDTO
                        {
                            DataStorageId = ds.DataStorageId,
                            ActorCompanyId = ds.ActorCompanyId,
                            ParentDataStorageId = ds.ParentDataStorageId,
                            EmployeeId = ds.EmployeeId,
                            Description = ds.Description,
                            Type = (SoeDataStorageRecordType)ds.Type,
                            TimePeriodId = ds.TimePeriodId,
                            TimePeriodName = ds.TimePeriod != null ? ds.TimePeriod.Name : String.Empty,
                            TimePeriodStartDate = ds.TimePeriod != null ? ds.TimePeriod.StartDate : CalendarUtility.DATETIME_DEFAULT,
                            TimePeriodStopDate = ds.TimePeriod != null ? ds.TimePeriod.StopDate : CalendarUtility.DATETIME_DEFAULT,
                            Data = ds.Data,
                            XML = ds.XML,
                        }).ToList();

            if (!includeControlInfo)
                dtos = dtos.Where(i => i.Type == SoeDataStorageRecordType.TimeSalaryExportEmployee).ToList();

            List<DataStorageAllDTO> validDTOs = new List<DataStorageAllDTO>();

            foreach (var dto in dtos)
            {
                if (includeXML)
                {
                    if (String.IsNullOrEmpty(dto.XML))
                        continue;

                    XDocument xdoc = XDocument.Parse(dto.XML);
                    if (xdoc == null)
                        continue;

                    DateTime? payedDate = TimeSalaryManager.GetPayedDate(xdoc);
                    if (payedDate.HasValue)
                        dto.XMLValue1 = payedDate.Value.ToString(CalendarUtility.SHORTDATEMASK);
                }

                if (!includeData)
                    dto.Data = null;

                if (String.IsNullOrEmpty(dto.XMLValue1) && dto.Type == SoeDataStorageRecordType.TimeSalaryExportControlInfoEmployee)
                    dto.XMLValue1 = GetText(5912, "Kontrolluppgift");

                validDTOs.Add(dto);
            }

            return validDTOs;
        }

        public List<DataStorageAllDTO> GetTimePayrollSlipByEmployee(int employeeId, int actorCompanyId, bool includeXML, bool includeData)  //1.4. Jukka
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            List<EmployeeTimePeriod> employeeTimePeriods = entitiesReadOnly.EmployeeTimePeriod.Where(w => w.EmployeeId == employeeId && w.State == (int)SoeEntityState.Active).ToList();
            bool publishPayrollSlipWhenLockingPeriod = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.PublishPayrollSlipWhenLockingPeriod, 0, actorCompanyId, 0);
            var dtos = (from ds in entitiesReadOnly.DataStorage
                        where ds.ActorCompanyId == actorCompanyId &&
                        (ds.EmployeeId.HasValue && ds.EmployeeId.Value == employeeId) &&
                        (ds.Type == (int)SoeDataStorageRecordType.PayrollSlipXML) &&
                        ds.State == (int)SoeEntityState.Active
                        orderby ds.Created descending
                        select new DataStorageAllDTO
                        {
                            DataStorageId = ds.DataStorageId,
                            ActorCompanyId = ds.ActorCompanyId,
                            ParentDataStorageId = ds.ParentDataStorageId,
                            EmployeeId = ds.EmployeeId,
                            Description = ds.Description,
                            Type = (SoeDataStorageRecordType)ds.Type,
                            TimePeriodId = ds.TimePeriodId,
                            TimePeriodName = ds.TimePeriod != null ? ds.TimePeriod.Name : String.Empty,
                            TimePeriodStartDate = ds.TimePeriod != null ? ds.TimePeriod.StartDate : CalendarUtility.DATETIME_DEFAULT,
                            TimePeriodStopDate = ds.TimePeriod != null ? ds.TimePeriod.StopDate : CalendarUtility.DATETIME_DEFAULT,
                            TimePeriodPaymentDate = ds.TimePeriod != null ? ds.TimePeriod.PaymentDate : null,
                            Data = ds.Data,
                            XML = ds.XML,
                        }).ToList();

            List<DataStorageAllDTO> validDTOs = new List<DataStorageAllDTO>();

            foreach (var dto in dtos)
            {
                if (!publishPayrollSlipWhenLockingPeriod)
                {
                    EmployeeTimePeriod employeeTimePeriod = employeeTimePeriods.FirstOrDefault(f => f.TimePeriodId == dto.TimePeriodId);
                    if (employeeTimePeriod != null)
                    {
                        if (employeeTimePeriod.Status != (int)SoeEmployeeTimePeriodStatus.Paid)
                            continue;

                        if (employeeTimePeriod.SalarySpecificationPublishDate.HasValue && employeeTimePeriod.SalarySpecificationPublishDate.Value > DateTime.Now)
                            continue;
                    }
                }

                if (dto.TimePeriodPaymentDate.HasValue)
                    dto.XMLValue1 = dto.TimePeriodPaymentDate.Value.ToString(CalendarUtility.SHORTDATEMASK);

                if (!includeData)
                {
                    dto.Data = null;
                }

                validDTOs.Add(dto);
            }

            return validDTOs;
        }

        public DataStorage GetTimeSalaryImport(int dataStorageId, int employeeId, int actorCompanyId)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            entitiesReadOnly.DataStorage.NoTracking();
            var dataStorage = (from ds in entitiesReadOnly.DataStorage
                        .Include("TimePeriod")
                               where ds.DataStorageId == dataStorageId &&
                               ds.ActorCompanyId == actorCompanyId &&
                               (ds.EmployeeId.HasValue && ds.EmployeeId.Value == employeeId) &&
                               (ds.Type == (int)SoeDataStorageRecordType.TimeSalaryExportEmployee || ds.Type == (int)SoeDataStorageRecordType.TimeSalaryExportControlInfoEmployee || ds.Type == (int)SoeDataStorageRecordType.TimeKU10ExportEmployee || ds.Type == (int)SoeDataStorageRecordType.PayrollSlipXML) &&
                               ds.State == (int)SoeEntityState.Active
                               orderby ds.Created descending
                               select ds).FirstOrDefault();

            UnCompressDataStorage(dataStorage);

            return dataStorage;
        }

        #endregion

        #region Invoice

        private ActionResult UndoSOPCustomerInvoiceExport(CompEntities entities, DataStorage dataStorage)
        {
            ActionResult result = new ActionResult(true);

            //Make sure DataStorageRecord is loaded
            if (!dataStorage.DataStorageRecord.IsLoaded)
                dataStorage.DataStorageRecord.Load();

            if (dataStorage.DataStorageRecord.Any())
            {
                //All records connect to a dataStorage have the same entitytype
                SoeEntityType entityType = (SoeEntityType)dataStorage.DataStorageRecord.FirstOrDefault().Entity;

                if (entityType == SoeEntityType.CustomerInvoice)
                {
                    foreach (var record in dataStorage.DataStorageRecord)
                    {
                        CustomerInvoice invoice = InvoiceManager.GetCustomerInvoice(entities, record.RecordId);
                        if (invoice.ExportStatus != (int)SoeInvoiceExportStatusType.NotExported)
                        {
                            invoice.ExportStatus = (int)SoeInvoiceExportStatusType.NotExported;
                        }
                    }
                }
            }

            return result;
        }

        #endregion

        #region XEMail

        public MessageAttachmentDTO GetAttachment(int dataStorageRecordId)
        {
            using var entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            DataStorageRecord record = (from dsr in entitiesReadOnly.DataStorageRecord.Include("DataStorage")
                                        where dsr.DataStorageRecordId == dataStorageRecordId
                                        select dsr).FirstOrDefault();

            if (record != null)
                UnCompressDataStorage(record.DataStorage);

            return record?.ToMessageAttachmentDTO(true);
        }

        #endregion

        #region Files

        public string GetUrlForDownload(byte[] data, string fileName)
        {
            if (data != null)
            {
                BlobUtil blobUtil = new BlobUtil();
                blobUtil.Init(BlobUtil.CONTAINER_LONG_TEMP);
                string mimeType = WebUtil.GetContentType(fileName);

                Guid guid = Guid.NewGuid();
                string externalLink = guid.ToString();
                var result = blobUtil.UploadData(guid, data, fileName, mimeType);
                if (!result.Success)
                    return string.Empty;

                return blobUtil.GetDownloadLink(externalLink, fileName);
            }

            return string.Empty;
        }

        public ActionResult CreateZipFileFromDataStorageRecords(List<int> ids, string prefixName)
        {
            ActionResult result = new ActionResult(true);
            List<Tuple<string, byte[]>> files = new List<Tuple<string, byte[]>>();

            try
            {
                if (string.IsNullOrEmpty(prefixName))
                    prefixName = base.GetText(10938, "Documents");

                string dateStr = DateTime.Now.ToString("yyyy-MM-dd");
                result.StringValue = $"{prefixName}_Download_Files_{dateStr}.zip";

                foreach (var id in ids)
                {
                    var dataStorageRecord = this.GetDataStorageRecord(base.ActorCompanyId, id);

                    if (dataStorageRecord?.DataStorage != null)
                    {
                        UnCompressDataStorage(dataStorageRecord.DataStorage);

                        if (dataStorageRecord.DataStorage.Data != null)
                        {
                            string entryName = !string.IsNullOrEmpty(dataStorageRecord.DataStorage.FileName)
                                ? dataStorageRecord.DataStorage.FileName
                                : dataStorageRecord.DataStorage.Description;

                            files.Add(new Tuple<string, byte[]>(entryName, dataStorageRecord.DataStorage.Data));
                        }
                    }
                }

                if (files.Count > 0)
                {
                    string guid = base.UserId + DateTime.Now.Millisecond.ToString();
                    string zippedpath = $@"{ConfigSettings.SOE_SERVER_DIR_TEMP_REPORT_PHYSICAL}\{guid}.zip";

                    if (ZipUtility.ZipFiles(zippedpath, files))
                    {
                        result.Value = File.ReadAllBytes(zippedpath);

                        if (File.Exists(zippedpath))
                            File.Delete(zippedpath);
                    }
                }
                else
                {
                    result.Success = false;
                    result.ErrorNumber = (int)ActionResultSave.EntityNotFound;
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Exception = ex;
                result.ErrorMessage = ex.Message;
                base.LogError(ex, this.log);
            }

            return result;
        }

        public ActionResult SendDocumentsAsEmail(int actorCompanyId, EmailDocumentsRequestDTO model)
        {
            ActionResult result = new ActionResult(true);
            List<KeyValuePair<string, byte[]>> attachments = new List<KeyValuePair<string, byte[]>>();

            foreach (int fileRecordId in model.FileRecordIds)
            {
                using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
                DataStorageRecord record = GetDataStorageRecord(entitiesReadOnly, actorCompanyId, fileRecordId);

                if (record != null && record.DataStorage != null)
                {
                    UnCompressDataStorage(record.DataStorage);

                    attachments.Add(new KeyValuePair<string, byte[]>(
                        record.DataStorage.FileName,
                        record.DataStorage.Data
                    ));
                }
            }

            var emailTemplateDTO = new EmailTemplateDTO
            {
                EmailTemplateId = 0,
                ActorCompanyId = base.ActorCompanyId,
                Subject = model.Subject,
                Body = model.Body,
                Typename = string.Empty,
                BodyIsHTML = false,
            };
            return EmailManager.SendEmailWithAttachment(
                actorCompanyId,
                emailTemplateDTO,
                model.RecipientUserIds,
                attachments,
                model.EmailAddresses.ToList(),
                null
            );
        }
        public ActionResult SaveFile(string physicalPath, string relativePath, byte[] data)
        {
            ActionResult result = new ActionResult(true);

            if (!File.Exists(physicalPath))
            {
                StreamWriter sw = null;

                try
                {
                    result.StackTrace += "path:" + physicalPath;
                    var file = new FileStream(physicalPath, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.Read);

                    BinaryWriter writer = new BinaryWriter(file);
                    writer.Write(data);

                    writer.Close();
                    file.Close();
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    result.Exception = ex;
                    return result;
                }
                finally
                {
                    if (sw != null)
                        sw.Close();
                }
            }

            result.StringValue = relativePath;

            return result;
        }

        #endregion

        #region EntityHistory     

        public EntityHistory GetEntityHistory(CompEntities entities, SoeEntityType entityType, int recordId, int actorCompanyId)
        {
            //Should only exists one item with this combination
            return (from eh in entities.EntityHistory
                    where eh.Entity == (int)entityType &&
                    eh.RecordId == recordId &&
                    eh.LoginName == LoginName &&
                    eh.ActorCompanyId == actorCompanyId
                    select eh).FirstOrDefault();
        }

        public ActionResult SaveEntityHistory(CompEntities entities, SoeEntityType entityType, int recordId, DateTime dateTime, bool setOpened, bool setSaved, bool setClosed, int actorCompanyId)
        {
            EntityHistory entityHistory = GetEntityHistory(entities, entityType, recordId, actorCompanyId);
            if (entityHistory == null)
            {
                //Should already be added, dont add to only set closed
                if (setClosed)
                    return new ActionResult((int)ActionResultSave.EntityIsNull, "EntityHistory");

                #region Add

                entityHistory = new EntityHistory()
                {
                    Entity = (int)entityType,
                    RecordId = recordId,
                    LoginName = base.LoginName,

                    //Set FK
                    ActorCompanyId = actorCompanyId,
                };
                entities.EntityHistory.AddObject(entityHistory);

                #endregion
            }

            #region Update

            if (setOpened)
                entityHistory.LastOpened = dateTime;
            if (setSaved)
                entityHistory.LastSaved = dateTime;
            if (setClosed)
                entityHistory.Closed = true;

            #endregion

            return SaveChanges(entities);
        }

        #endregion

        #region EventHistory

        public int GetEventHistoryBatchNr(CompEntities entities, int actorCompanyId)
        {
            var last = entities.EventHistory.OrderByDescending(o => o.EventHistoryId).FirstOrDefault(w => w.ActorCompanyId == actorCompanyId);

            if (last != null)
                return last.BatchId + new Random().Next(1, 5);
            else
                return 1;
        }

        public List<EventHistory> GetEventHistories(int actorCompanyId, TermGroup_EventHistoryType type, SoeEntityType entity, int recordId, DateTime dateFrom, DateTime dateTo, bool setNames)
        {
            dateFrom = CalendarUtility.GetBeginningOfDay(dateFrom);
            dateTo = CalendarUtility.GetEndOfDay(dateTo);

            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            entitiesReadOnly.EventHistory.NoTracking();
            IQueryable<EventHistory> oQuery = entitiesReadOnly.EventHistory;
            IQueryable<EventHistory> query = (from e in oQuery
                                              where e.ActorCompanyId == actorCompanyId &&
                                              (e.Created >= dateFrom && e.Created <= dateTo) &&
                                              e.State == (int)SoeEntityState.Active
                                              select e);

            if (type != TermGroup_EventHistoryType.Unspecified)
                query = query.Where(e => e.Type == (int)type);

            if (entity != SoeEntityType.None && recordId > 0)
                query = query.Where(e => e.Entity == (int)entity && e.RecordId == recordId);

            List<EventHistory> eventHistories = query.ToList();

            if (setNames)
                SetEventHistoryNames(eventHistories);

            return eventHistories;
        }

        public List<EventHistory> GetEventHistories(CompEntities entities, int actorCompanyId, TermGroup_EventHistoryType type, int batchId, bool setNames)
        {
            List<EventHistory> eventHistories = (from e in entities.EventHistory
                                                 where e.ActorCompanyId == actorCompanyId &&
                                                 e.Type == (int)type &&
                                                 e.BatchId == batchId &&
                                                 e.State == (int)SoeEntityState.Active
                                                 select e).ToList();

            if (setNames)
                SetEventHistoryNames(eventHistories);

            return eventHistories;
        }

        public int GetNbrOfEventsInBatch(int actorCompanyId, TermGroup_EventHistoryType type, int batchId)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetEventHistories(entitiesReadOnly, actorCompanyId, type, batchId, false).Count;
        }

        public EventHistory GetEventHistory(int eventHistoryId, int actorCompanyId, bool setNames = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.EventHistory.NoTracking();
            return GetEventHistory(entities, eventHistoryId, actorCompanyId, setNames);
        }

        public EventHistory GetEventHistory(CompEntities entities, int eventHistoryId, int actorCompanyId, bool setNames = false)
        {
            EventHistory eventHistory = (from u in entities.EventHistory
                                         where u.EventHistoryId == eventHistoryId &&
                                         u.ActorCompanyId == actorCompanyId &&
                                         u.State == (int)SoeEntityState.Active
                                         select u).FirstOrDefault();

            if (setNames)
                SetEventHistoryNames(new List<EventHistory> { eventHistory });

            return eventHistory;
        }

        public EventHistory GetLastEventHistory(CompEntities entities, TermGroup_EventHistoryType type, int recordId, int actorCompanyId)
        {
            return (from u in entities.EventHistory
                    where u.Type == (int)type &&
                    u.ActorCompanyId == actorCompanyId &&
                    u.RecordId == recordId &&
                    u.State == (int)SoeEntityState.Active
                    select u).OrderByDescending(o => o.EventHistoryId).FirstOrDefault();
        }

        private void SetEventHistoryNames(List<EventHistory> eventHistories)
        {
            List<GenericType> types = base.GetTermGroupContent(TermGroup.EventHistoryType);
            List<GenericType> entityTypes = base.GetTermGroupContent(TermGroup.SoeEntityType);
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();

            foreach (EventHistory eventHistory in eventHistories)
            {
                eventHistory.TypeName = types.First(t => t.Id == eventHistory.Type).Name;
                eventHistory.EntityName = entityTypes.First(t => t.Id == eventHistory.Entity).Name;

                // Set record name based on entity (for display in grid)
                switch ((SoeEntityType)eventHistory.Entity)
                {
                    case SoeEntityType.Employee:
                        eventHistory.RecordName = EmployeeManager.GetEmployeeName(entitiesReadOnly, eventHistory.RecordId);
                        break;
                }
            }
        }

        public ActionResult SaveEventHistories(CompEntities entities, List<EventHistoryDTO> eventHistoryInputs, int actorCompanyId)
        {
            if (eventHistoryInputs.IsNullOrEmpty())
                return new ActionResult(true);

            List<TrackChangesDTO> trackChangesItems = new List<TrackChangesDTO>();

            if (!eventHistoryInputs.All(a => a.BatchId != 0))
            {
                var batchId = GetEventHistoryBatchNr(entities, actorCompanyId);
                eventHistoryInputs.ForEach(f => f.BatchId = batchId);
            }

            foreach (var item in eventHistoryInputs)
            {
                SaveEventHistory(entities, item, actorCompanyId, false);
            }

            ActionResult result = SaveChanges(entities);
            if (result.Success && trackChangesItems.Any())
                result = TrackChangesManager.AddTrackChanges(entities, null, trackChangesItems);

            return result;
        }

        public ActionResult SaveEventHistory(EventHistoryDTO eventHistoryInput, int actorCompanyId)
        {
            using (CompEntities entities = new CompEntities())
            {
                return SaveEventHistory(entities, eventHistoryInput, actorCompanyId);
            }
        }

        public ActionResult SaveEventHistory(CompEntities entities, EventHistoryDTO eventHistoryInput, int actorCompanyId, bool save = true, List<TrackChangesDTO> trackChangesItems = null)
        {
            if (eventHistoryInput == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "EventHistory");

            // Default result is successful
            ActionResult result = new ActionResult();

            #region Prereq

            if (trackChangesItems == null)
                trackChangesItems = new List<TrackChangesDTO>();

            int eventHistoryId = eventHistoryInput.EventHistoryId;

            #endregion

            // Get existing
            EventHistory eventHistory = eventHistoryId != 0 ? GetEventHistory(entities, eventHistoryId, actorCompanyId) : null;
            if (eventHistory == null)
            {
                #region Add

                eventHistory = new EventHistory()
                {
                    ActorCompanyId = actorCompanyId,
                    Type = (int)eventHistoryInput.Type,
                    Entity = (int)eventHistoryInput.Entity,
                    RecordId = eventHistoryInput.RecordId,
                    BatchId = eventHistoryInput.BatchId,
                    UserId = eventHistoryInput.UserId,
                    StrData = eventHistoryInput.StringValue,
                    IntData = eventHistoryInput.IntegerValue,
                    DecimalData = eventHistoryInput.DecimalValue,
                    BoolData = eventHistoryInput.BooleanValue,
                    DateData = eventHistoryInput.DateValue,
                    State = (int)SoeEntityState.Active
                };
                SetCreatedProperties(eventHistory);
                entities.EventHistory.AddObject(eventHistory);

                #endregion
            }
            else
            {
                #region Update

                // Only track changes on update
                if (eventHistory.UserId != eventHistoryInput.UserId)
                {
                    trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, actorCompanyId, TermGroup_TrackChangesActionMethod.CommonUpdate, TermGroup_TrackChangesAction.Update, SoeEntityType.EventHistory, eventHistoryId, eventHistoryInput.Entity, eventHistoryInput.RecordId, SettingDataType.Integer, "UserId", TermGroup_TrackChangesColumnType.EventHistory_UserId, eventHistory.UserId.ToValueOrNull(), eventHistoryInput.UserId.ToValueOrNull()));
                    eventHistory.UserId = eventHistoryInput.UserId;
                }
                if (eventHistory.StrData != eventHistoryInput.StringValue)
                {
                    trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, actorCompanyId, TermGroup_TrackChangesActionMethod.CommonUpdate, TermGroup_TrackChangesAction.Update, SoeEntityType.EventHistory, eventHistoryId, eventHistoryInput.Entity, eventHistoryInput.RecordId, SettingDataType.String, "StrData", TermGroup_TrackChangesColumnType.EventHistory_StrData, eventHistory.StrData, eventHistoryInput.StringValue));
                    eventHistory.StrData = eventHistoryInput.StringValue;
                }
                if (eventHistory.IntData != eventHistoryInput.IntegerValue)
                {
                    trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, actorCompanyId, TermGroup_TrackChangesActionMethod.CommonUpdate, TermGroup_TrackChangesAction.Update, SoeEntityType.EventHistory, eventHistoryId, eventHistoryInput.Entity, eventHistoryInput.RecordId, SettingDataType.Integer, "IntData", TermGroup_TrackChangesColumnType.EventHistory_IntData, eventHistory.IntData.ToValueOrNull(), eventHistoryInput.IntegerValue.ToValueOrNull()));
                    eventHistory.IntData = eventHistoryInput.IntegerValue;
                }
                if (eventHistory.DecimalData != eventHistoryInput.DecimalValue)
                {
                    trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, actorCompanyId, TermGroup_TrackChangesActionMethod.CommonUpdate, TermGroup_TrackChangesAction.Update, SoeEntityType.EventHistory, eventHistoryId, eventHistoryInput.Entity, eventHistoryInput.RecordId, SettingDataType.Decimal, "DecimalData", TermGroup_TrackChangesColumnType.EventHistory_DecimalData, eventHistory.DecimalData.ToValueOrNull(true, 2), eventHistoryInput.DecimalValue.ToValueOrNull(true, 2)));
                    eventHistory.DecimalData = eventHistoryInput.DecimalValue;
                }
                if (eventHistory.BoolData != eventHistoryInput.BooleanValue)
                {
                    trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, actorCompanyId, TermGroup_TrackChangesActionMethod.CommonUpdate, TermGroup_TrackChangesAction.Update, SoeEntityType.EventHistory, eventHistoryId, eventHistoryInput.Entity, eventHistoryInput.RecordId, SettingDataType.Boolean, "BoolData", TermGroup_TrackChangesColumnType.EventHistory_BoolData, eventHistory.BoolData.HasValue ? eventHistory.BoolData.Value.ToString() : null, eventHistoryInput.BooleanValue.HasValue ? eventHistoryInput.BooleanValue.Value.ToString() : null));
                    eventHistory.BoolData = eventHistoryInput.BooleanValue;
                }
                if (eventHistory.DateData != eventHistoryInput.DateValue)
                {
                    trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, actorCompanyId, TermGroup_TrackChangesActionMethod.CommonUpdate, TermGroup_TrackChangesAction.Update, SoeEntityType.EventHistory, eventHistoryId, eventHistoryInput.Entity, eventHistoryInput.RecordId, SettingDataType.Date, "DateData", TermGroup_TrackChangesColumnType.EventHistory_DateData, eventHistory.DateData.HasValue ? eventHistory.DateData.Value.ToShortDateString() : null, eventHistoryInput.DateValue.HasValue ? eventHistoryInput.DateValue.Value.ToShortDateString() : null));
                    eventHistory.DateData = eventHistoryInput.DateValue;
                }
                if (trackChangesItems.Any())
                    SetModifiedProperties(eventHistory);

                #endregion
            }

            if (save)
            {
                result = SaveChanges(entities);
                if (result.Success && trackChangesItems.Any())
                    result = TrackChangesManager.AddTrackChanges(entities, null, trackChangesItems);
            }
            return result;
        }

        public ActionResult DeleteEventHistory(int eventHistoryId, int actorCompanyId)
        {
            using (CompEntities entities = new CompEntities())
            {
                EventHistory eventHistory = GetEventHistory(entities, eventHistoryId, actorCompanyId);
                if (eventHistory == null)
                    return new ActionResult((int)ActionResultDelete.EntityNotFound, "EventHistory");

                return ChangeEntityState(entities, eventHistory, SoeEntityState.Deleted, true);
            }
        }

        public ActionResult DeleteEventHistories(TermGroup_EventHistoryType type, int batchId, int actorCompanyId)
        {
            using (CompEntities entities = new CompEntities())
            {
                List<EventHistory> eventHistories = GetEventHistories(entities, actorCompanyId, type, batchId, false);
                foreach (EventHistory eventHistory in eventHistories)
                {
                    ChangeEntityState(entities, eventHistory, SoeEntityState.Deleted, false);
                }
                return SaveChanges(entities);
            }
        }

        #endregion

        #region SimpleTextEditor

        public List<Textblock> GetTextblocks(int entity, int actorCompanyId, int? textBlockId = null)
        {
            using var entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            entitiesReadOnly.Textblock.NoTracking();
            IQueryable<Textblock> textBlokQuery = from dt in entitiesReadOnly.TextblockEntity
                                                  where dt.Textblock.ActorCompanyId == actorCompanyId &&
                                                  dt.Entity == entity &&
                                                  dt.Textblock.State == (int)SoeEntityState.Active
                                                  orderby dt.Textblock.Headline
                                                  select dt.Textblock;
            if (textBlockId.HasValue)
            {
                textBlokQuery = textBlokQuery.Where(x => x.TextblockId == textBlockId.Value);
            }
            return textBlokQuery.ToList();
        }

        public List<Textblock> GetTextblockDictionary(int actorCompanyId, TextBlockDictType dictType)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Textblock.NoTracking();
            return (from tb in entities.Textblock
                    where tb.ActorCompanyId == actorCompanyId &&
                    tb.State == (int)SoeEntityState.Active &&
                    tb.Type == (int)TextBlockType.Dictionary &&
                    tb.SubType == (int)dictType
                    orderby tb.Headline
                    select tb).ToList();
        }

        public Textblock GetTextblock(int textBlockId, bool loadTextblockEntity = true)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TextblockEntity.NoTracking();
            return GetTextblock(entities, textBlockId, loadTextblockEntity);
        }

        public Textblock GetTextblock(CompEntities entities, int textBlockId, bool mustHaveTextBlockEntity = true)
        {
            if (textBlockId <= 0)
                return null;

            IQueryable<Textblock> query = entities.Textblock;
            if (mustHaveTextBlockEntity)
                query = query.Where(i => i.TextblockEntity.Any());

            return (from t in query
                    where t.TextblockId == textBlockId
                    select t).FirstOrDefault();
        }

        public ActionResult SaveTextblock(TextblockDTO textblockInput, int entity, List<CompTermDTO> inputTranslations = null)
        {
            if (textblockInput == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "Textblock");

            ActionResult result;

            using (CompEntities entities = new CompEntities())
            {
                int textblockId = 0;

                if (textblockInput.TextblockId == 0)
                {
                    Textblock textblock = new Textblock()
                    {
                        ActorCompanyId = textblockInput.ActorCompanyId,
                        Headline = textblockInput.Headline,
                        Text = textblockInput.Text,
                        Type = textblockInput.Type,
                        ShowInContract = textblockInput.ShowInContract,
                        ShowInOffer = textblockInput.ShowInOffer,
                        ShowInOrder = textblockInput.ShowInOrder,
                        ShowInInvoice = textblockInput.ShowInInvoice,
                        ShowInPurchase = textblockInput.ShowInPurchase,
                    };
                    if (textblock.TextblockEntity == null)
                        textblock.TextblockEntity = new EntityCollection<TextblockEntity>();
                    textblock.TextblockEntity.Add(new TextblockEntity() { Entity = entity });
                    SetCreatedProperties(textblock);
                    entities.Textblock.AddObject(textblock);

                    result = AddEntityItem(entities, textblock, "Textblock");

                    textblockId = textblock.TextblockId;
                }
                else
                {
                    Textblock originalTextblock = GetTextblock(entities, textblockInput.TextblockId);

                    //Update Textblock
                    originalTextblock.Text = textblockInput.Text;
                    originalTextblock.Headline = textblockInput.Headline;
                    originalTextblock.Type = textblockInput.Type;
                    originalTextblock.ShowInContract = textblockInput.ShowInContract;
                    originalTextblock.ShowInOffer = textblockInput.ShowInOffer;
                    originalTextblock.ShowInOrder = textblockInput.ShowInOrder;
                    originalTextblock.ShowInInvoice = textblockInput.ShowInInvoice;
                    originalTextblock.ShowInPurchase = textblockInput.ShowInPurchase;

                    SetModifiedProperties(originalTextblock);

                    result = SaveChanges(entities);

                    textblockId = textblockInput.TextblockId;
                }


                #region Translations
                if (inputTranslations != null)
                {
                    var langIdsToSave = inputTranslations.Select(i => (int)i.Lang).Distinct().ToList();
                    var existingTranslations = TermManager.GetCompTerms(entities, CompTermsRecordType.Textblock, textblockId);

                    #region Delete existing translations for other languages

                    foreach (var existingTranslation in existingTranslations)
                    {
                        if (langIdsToSave.Contains(existingTranslation.LangId))
                            continue;

                        existingTranslation.State = (int)SoeEntityState.Deleted;
                    }

                    #endregion

                    #region Add or update translations for languages

                    foreach (int langId in langIdsToSave)
                    {
                        CompTerm translation = null;
                        var inputTranslation = inputTranslations.FirstOrDefault(i => (int)i.Lang == langId);

                        var existingTranslationsForLang = existingTranslations.Where(i => i.LangId == langId).ToList();
                        if (existingTranslationsForLang.Count == 0)
                        {
                            #region Add

                            translation = new CompTerm { ActorCompanyId = ActorCompanyId };
                            entities.CompTerm.AddObject(translation);

                            #endregion
                        }
                        else
                        {
                            #region Update

                            for (int i = 0; i < existingTranslationsForLang.Count; i++)
                            {
                                if (i > 0)
                                {
                                    //Remove duplicates
                                    existingTranslationsForLang[i].State = (int)SoeEntityState.Deleted;
                                    continue;
                                }

                                translation = existingTranslationsForLang[i];
                            }

                            #endregion
                        }

                        #region Set values

                        if (translation != null)
                        {
                            translation.RecordType = (int)inputTranslation.RecordType;
                            translation.RecordId = textblockId;
                            translation.LangId = (int)inputTranslation.Lang;
                            translation.Name = inputTranslation.Name;
                            translation.State = (int)SoeEntityState.Active;
                        }

                        #endregion
                    }

                    #endregion

                    result = SaveChanges(entities);
                    if (!result.Success)
                    {
                        result.ErrorNumber = (int)ActionResultSave.TranslationsSaveFailed;
                        return result;
                    }
                }
                #endregion

                result.IntegerValue = textblockId;

                return result;
            }
        }

        public ActionResult DeleteTextblock(int textBlockId)
        {
            using (CompEntities entities = new CompEntities())
            {
                Textblock originalTextblock = GetTextblock(entities, textBlockId);
                return DeleteTextblock(entities, originalTextblock);
            }
        }

        public ActionResult DeleteTextblock(CompEntities entities, Textblock originalTextblock)
        {
            if (originalTextblock == null)
                return null;

            //Remove Textblock
            return ChangeEntityState(entities, originalTextblock, SoeEntityState.Deleted, true);
        }

        #endregion

        #region SysPageStatus

        public IQueryable<SysPageStatus> GetSysPageStatus(SOESysEntities entities, bool includeSysFeature = false)
        {
            var query = entities.SysPageStatus.AsQueryable();

            if (includeSysFeature)
                query = query.Include("SysFeature");

            return query;
        }

        public IEnumerable<SysPageStatus> GetSysPageStatuses(bool setNames)
        {
            List<GenericType> statusTypes = null;
            if (setNames)
                statusTypes = base.GetTermGroupContent(TermGroup.SysPageStatusStatusType);

            using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
            var query = GetSysPageStatus(sysEntitiesReadOnly, true).AsNoTracking();
            List<SysPageStatus> pageStatuses = query.ToList();
            if (setNames)
            {
                foreach (SysPageStatus pageStatus in pageStatuses)
                {
                    pageStatus.PageName = GetText(pageStatus.SysFeature.SysTermId, pageStatus.SysFeature.SysTermGroupId);
                    pageStatus.BetaStatusName = statusTypes.FirstOrDefault(t => t.Id == pageStatus.BetaStatus)?.Name ?? string.Empty;
                    pageStatus.LiveStatusName = statusTypes.FirstOrDefault(t => t.Id == pageStatus.LiveStatus)?.Name ?? string.Empty;
                }
            }

            return pageStatuses.OrderByDescending(p => p.LiveStatus).ThenByDescending(p => p.BetaStatus);
        }

        public IEnumerable<int> GetMigratedFeatures()
        {
            using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
            var query = GetSysPageStatus(sysEntitiesReadOnly, false);

            bool isLive = CompDbCache.Instance.SiteType == TermGroup_SysPageStatusSiteType.Live;
            var spaIsActiveTypes = new int[]
            {
                (int)TermGroup_SysPageStatusStatusType.Active,
                (int)TermGroup_SysPageStatusStatusType.ActiveForCompany,
                (int)TermGroup_SysPageStatusStatusType.AngularJsBlocked,
            };

            if (isLive)
                query = query.Where(s => spaIsActiveTypes.Contains(s.LiveStatus));
            else
                query = query.Where(s => spaIsActiveTypes.Contains(s.BetaStatus));

            return query
                .Select(s => s.SysFeatureId)
                .ToList();
        }

        public List<int> GetMigratedFeaturesFromCache()
        {
            string key = "GetMigratedFeatures";
            var value = BusinessMemoryCache<List<int>>.Get(key);
            if (value == null)
            {
                value = GetMigratedFeatures().ToList();
                BusinessMemoryCache<List<int>>.Set(key, value, 120);
            }
            return value;
        }

        public SysPageStatus GetSysPageStatus(Feature sysFeature, bool cached = false)
        {
            using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
            return GetSysPageStatus(sysEntitiesReadOnly, sysFeature, cached);
        }

        public SysPageStatus GetSysPageStatus(SOESysEntities entities, Feature sysFeature, bool cached = false)
        {
            if (!cached)
                return entities.SysPageStatus.FirstOrDefault(f => f.SysFeatureId == (int)sysFeature);

            string key = "GetSysPageStatus";

            var value = BusinessMemoryCache<Dictionary<Feature, List<SysPageStatus>>>.Get(key);

            if (value == null)
            {
                value = entities.SysPageStatus.GroupBy(g => (Feature)g.SysFeatureId).ToDictionary(k => k.Key, v => v.ToList());
                BusinessMemoryCache<Dictionary<Feature, List<SysPageStatus>>>.Set(key, value);
            }

            return value.ContainsKey(sysFeature) ? value[sysFeature].FirstOrDefault() : null;
        }

        private TermGroup_SysPageStatusStatusType CheckForCompanyAngularSpaStatus(Feature sysFeature, TermGroup_SysPageStatusStatusType currentStatusType)
        {
            var allowedCompanyIds = new List<int>() { 3057691, 3057665, 3057716, 29984 };

            if (sysFeature == Feature.Billing_Project_List && allowedCompanyIds.Contains(ActorCompanyId))
            {
                return TermGroup_SysPageStatusStatusType.ActiveForCompany;
            }
            else if (ActorCompanyId == 7 && !SettingManager.isTest())
            {
                return TermGroup_SysPageStatusStatusType.ActiveForCompany;
            }

            return currentStatusType;
        }

        public bool IsAngularSpaValid(Feature sysFeature, TermGroup_SysPageStatusSiteType siteType, bool currentPage)
        {
            string key = $"s{sysFeature}#st{siteType}#cp{currentPage}";
            bool? isAngularSpaValid = BusinessMemoryCache<bool?>.Get(key);

            if (isAngularSpaValid.HasValue)
                return isAngularSpaValid.Value;

            SysPageStatus sysPageStatus = GetSysPageStatus(sysFeature);
            if (sysPageStatus == null)
            {
                // If no status is registered for this page, return false.
                // This is done on purpose, so the developer does not forget to register the page.
                isAngularSpaValid = false;
            }
            else
            {
                TermGroup_SysPageStatusStatusType statusType = (TermGroup_SysPageStatusStatusType)(siteType == TermGroup_SysPageStatusSiteType.Live ? sysPageStatus.LiveStatus : sysPageStatus.BetaStatus);
                statusType = CheckForCompanyAngularSpaStatus(sysFeature, statusType);

                switch (statusType)
                {
                    case TermGroup_SysPageStatusStatusType.Blocked:
#if DEBUG
                        if (siteType == TermGroup_SysPageStatusSiteType.Test)
                        {
                            // If development always show Angular SPA page if registered
                            isAngularSpaValid = true;
                            break;
                        }
#endif
                        isAngularSpaValid = false;
                        break;
                    case TermGroup_SysPageStatusStatusType.RFT:
                        // Ready for test, only show internally
                        isAngularSpaValid = (siteType == TermGroup_SysPageStatusSiteType.Test);
                        break;
                    case TermGroup_SysPageStatusStatusType.Active:
                        isAngularSpaValid = true;
                        break;
                    case TermGroup_SysPageStatusStatusType.ActiveForCompany:
                        // Not actually used
                        isAngularSpaValid = true;
                        break;
                    case TermGroup_SysPageStatusStatusType.AngularJsBlocked:
                        isAngularSpaValid = true;
                        break;
                    case TermGroup_SysPageStatusStatusType.AngularJsFirst:
                        // CurrentPage is when when checking if AngularJs or Angular SPA should be shown, otherwise when checking if Angular SPA icon should be shown
                        if (currentPage)
                            isAngularSpaValid = false;
                        else
                            isAngularSpaValid = true;
                        break;
                    default:
                        isAngularSpaValid = false;
                        break;
                }
            }

            if (!isAngularSpaValid.HasValue)
                isAngularSpaValid = false;

            BusinessMemoryCache<bool>.Set(key, isAngularSpaValid.Value);

            return isAngularSpaValid.Value;
        }

        public bool CanShowAngularJsPage(Feature sysFeature, TermGroup_SysPageStatusSiteType siteType)
        {
            SysPageStatus sysPageStatus = GetSysPageStatus(sysFeature);
            if (sysPageStatus != null)
            {
                TermGroup_SysPageStatusStatusType statusType = (TermGroup_SysPageStatusStatusType)(siteType == TermGroup_SysPageStatusSiteType.Live ? sysPageStatus.LiveStatus : sysPageStatus.BetaStatus);
                if (statusType == TermGroup_SysPageStatusStatusType.AngularJsBlocked)
                    return false;
            }

            return true;
        }

        public bool ShowAngularJsFirst(Feature sysFeature, TermGroup_SysPageStatusSiteType siteType)
        {
            bool show = false;

            SysPageStatus sysPageStatus = GetSysPageStatus(sysFeature);
            if (sysPageStatus != null)
            {
                TermGroup_SysPageStatusStatusType statusType = (TermGroup_SysPageStatusStatusType)(siteType == TermGroup_SysPageStatusSiteType.Live ? sysPageStatus.LiveStatus : sysPageStatus.BetaStatus);
                if (statusType == TermGroup_SysPageStatusStatusType.AngularJsFirst)
                    show = true;
            }

            return show;
        }

        public ActionResult SetSysPageStatus(Feature sysFeature, TermGroup_SysPageStatusSiteType siteType, TermGroup_SysPageStatusStatusType statusType)
        {
            ActionResult result;

            using (SOESysEntities entities = new SOESysEntities())
            {
                // Get existing status
                SysPageStatus sysPageStatus = GetSysPageStatus(entities, sysFeature);
                if (sysPageStatus == null)
                {
                    #region Add

                    sysPageStatus = new SysPageStatus()
                    {
                        SysFeatureId = (int)sysFeature,
                    };

                    if (siteType == TermGroup_SysPageStatusSiteType.Beta)
                    {
                        sysPageStatus.BetaStatus = (int)statusType;
                        sysPageStatus.LiveStatus = (int)TermGroup_SysPageStatusStatusType.Blocked;
                    }
                    else
                    {
                        sysPageStatus.BetaStatus = (int)TermGroup_SysPageStatusStatusType.Blocked;
                        sysPageStatus.LiveStatus = (int)statusType;
                    }

                    SetCreatedPropertiesOnEntity(sysPageStatus);
                    entities.SysPageStatus.Add(sysPageStatus);

                    #endregion
                }
                else
                {
                    #region Update

                    if (siteType == TermGroup_SysPageStatusSiteType.Beta)
                        sysPageStatus.BetaStatus = (int)statusType;
                    else
                        sysPageStatus.LiveStatus = (int)statusType;
                    SetModifiedPropertiesOnEntity(sysPageStatus);

                    #endregion
                }

                result = SaveChanges(entities);
            }

            return result;
        }

        #endregion

        #region SystemInfo

        public List<SystemInfoLog> GetSystemInfoLogEntries(CompEntities entities, int actorCompanyId, int? take = null)
        {
            var query = (from sil in entities.SystemInfoLog
                         where sil.ActorCompanyId == actorCompanyId &&
                         sil.State == (int)SoeEntityState.Active
                         orderby sil.Date descending
                         select sil);

            if (take.HasValue)
                return query.Take(take.Value).ToList();
            else
                return query.ToList();
        }

        public List<SystemInfoLog> GetSystemInfoLogEntries(int actorCompanyId, SystemInfoType type, int? take = null, bool activeOnly = true)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.SystemInfoLog.NoTracking();
            return GetSystemInfoLogEntries(entities, actorCompanyId, type, take, activeOnly);
        }

        public List<SystemInfoLog> GetSystemInfoLogEntries(CompEntities entities, int actorCompanyId, SystemInfoType type, int? take = null, bool activeOnly = true)
        {
            var query = (from sil in entities.SystemInfoLog
                         where sil.ActorCompanyId == actorCompanyId &&
                         sil.Type == (int)type &&
                         (activeOnly ? sil.State == (int)SoeEntityState.Active : true)
                         orderby sil.Date descending
                         select sil);

            if (take.HasValue)
                return query.Take(take.Value).ToList();
            else
                return query.ToList();
        }

        public List<SystemInfoLog> GetSystemInfoLogEntriesByRole(int actorCompanyId, int roleId, int? take = null)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.SystemInfoLog.NoTracking();
            return GetSystemInfoLogEntriesByRole(entities, actorCompanyId, roleId, take);
        }

        public List<SystemInfoLog> GetSystemInfoLogEntriesByRole(CompEntities entities, int actorCompanyId, int roleId, int? take = null)
        {
            List<int> employeeIds = EmployeeManager.GetEmployeesForUsersAttestRoles(entities, out _, actorCompanyId, base.UserId, roleId, active: true).Select(e => e.EmployeeId).ToList();
            List<SystemInfoLog> systemInfoLogs = new List<SystemInfoLog>();
            var query = from sil in entities.SystemInfoLog
                        where sil.ActorCompanyId == actorCompanyId &&
                        sil.State == (int)SoeEntityState.Active &&
                        (!sil.EmployeeId.HasValue || employeeIds.Contains(sil.EmployeeId.Value))
                        orderby sil.Date descending
                        select sil;

            if (take.HasValue)
                systemInfoLogs = query.Take(take.Value).ToList();
            else
                systemInfoLogs = query.ToList();

            return systemInfoLogs;
            //  var groupedSystemInfoLogs = systemInfoLogs.GroupBy(g => $"{g.EmployeeId}#{g.Date.Date}#{g.Text}#{g.user}");// 80634
        }

        private Dictionary<int, List<Employee>> GetEmployeesWithKeepNbrOfYearsAfterEndWarningsByCompany()
        {
            Dictionary<int, List<Employee>> companyEmployeesWithWarningsDict = new Dictionary<int, List<Employee>>();

            using (CompEntities entities = new CompEntities())
            {
                List<Employee> employees = entities.Employee.Include("Employment").Include("ContactPerson").Where(w => w.State != (int)SoeEntityState.Deleted).ToList();
                Dictionary<int, List<Employee>> employeesByCompany = employees.GroupBy(g => g.ActorCompanyId).ToDictionary(k => k.Key, v => v.ToList());

                List<UserCompanySetting> settings = SettingManager.GetAllCompanySettings((int)CompanySettingType.EmployeeKeepNbrOfYearsAfterEnd);
                foreach (var settingsByCompany in settings.Where(i => i.IntData.HasValue && i.IntData.Value > 0 && i.ActorCompanyId.HasValue).GroupBy(i => i.ActorCompanyId.Value))
                {
                    if (!employeesByCompany.ContainsKey(settingsByCompany.Key))
                        continue;

                    int actorCompanyId = settingsByCompany.Key;

                    List<Employee> employeesWithWarning = new List<Employee>();
                    List<Employee> employeesForCompany = employeesByCompany[actorCompanyId];

                    foreach (UserCompanySetting setting in settingsByCompany)
                    {
                        DateTime firstValidEndDate = DateTime.Now.Date.AddYears(-setting.IntData.Value);

                        foreach (Employee employee in employeesForCompany)
                        {
                            if (employeesWithWarning.Any(i => i.EmployeeId == employee.EmployeeId))
                                continue;

                            DateTime? employmentEndDate = employee.GetLastEmployment()?.GetEndDate();
                            if (employmentEndDate.HasValue && employmentEndDate.Value <= firstValidEndDate)
                                employeesWithWarning.Add(employee);
                        }
                    }

                    if (!companyEmployeesWithWarningsDict.ContainsKey(actorCompanyId))
                        companyEmployeesWithWarningsDict.Add(actorCompanyId, employeesWithWarning);
                }
            }

            return companyEmployeesWithWarningsDict;
        }

        public SystemInfoLog GetSystemInfoLogEntry(CompEntities entities, int systemInfoLogId)
        {
            return (from sil in entities.SystemInfoLog
                    where sil.SystemInfoLogId == systemInfoLogId
                    select sil).FirstOrDefault();
        }

        public ActionResult AddSystemInfoLogEntry(CompEntities entities, SystemInfoLog logEntry)
        {
            try
            {
                #region Perform

                entities.SystemInfoLog.AddObject(logEntry);
                SetCreatedProperties(logEntry);

                return SaveChanges(entities);

                #endregion
            }
            catch (Exception ex)
            {
                base.LogError(ex, this.log);
                return new ActionResult(ex);
            }
        }

        public ActionResult AddSystemInfoLogEntry(TransactionScope transaction, CompEntities entities, SystemInfoLog logEntry)
        {
            ActionResult result = new ActionResult();

            try
            {
                #region Perform

                entities.SystemInfoLog.AddObject(logEntry);
                SetCreatedProperties(logEntry);

                result = SaveChanges(entities, transaction);

                #endregion
            }
            catch (Exception ex)
            {
                result.Exception = ex;
                base.LogError(ex, this.log);
            }

            return result;
        }

        public ActionResult AddSystemInfoLogEntries(List<SystemInfoLog> logEntries)
        {
            ActionResult result = new ActionResult();

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        if (entities.Connection.State != ConnectionState.Open)
                            entities.Connection.Open();

                        #region Prereq

                        //?

                        #endregion

                        #region Perform

                        foreach (SystemInfoLog entry in logEntries)
                        {
                            entities.SystemInfoLog.AddObject(entry);
                            SetCreatedProperties(entry);
                        }

                        result = SaveChanges(entities, transaction);

                        #endregion

                        //Commit transaction
                        if (result.Success)
                            transaction.Complete();
                    }
                }
                catch (Exception ex)
                {
                    result.Exception = ex;
                    base.LogError(ex, this.log);
                }
                finally
                {
                    if (!result.Success)
                    {
                        base.LogTransactionFailed(this.ToString(), this.log);
                    }

                    entities.Connection.Close();
                }
            }

            return result;
        }

        public ActionResult DisableSystemInfoLogEntry(int actorCompanyId, int systemInfoLogId)
        {
            ActionResult result = new ActionResult();

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    SystemInfoLog logEntry = GetSystemInfoLogEntry(entities, systemInfoLogId);

                    if (logEntry != null)
                    {
                        ChangeEntityState(logEntry, SoeEntityState.Inactive);
                        result = SaveChanges(entities);
                    }

                }
                catch (Exception ex)
                {
                    result.Exception = ex;
                    base.LogError(ex, this.log);
                }
                finally
                {
                    entities.Connection.Close();
                }
            }

            return result;
        }

        public ActionResult RemoveEmployeeInfoJob()
        {
            //EmployeeKeepNbrOfYearsAfterEnd
            Dictionary<int, List<Employee>> companyEmployeesWithWarningsDict = GetEmployeesWithKeepNbrOfYearsAfterEndWarningsByCompany();

            if (companyEmployeesWithWarningsDict.IsNullOrEmpty())
                return new ActionResult(true);

            List<SystemInfoLog> systemInfoLogs = new List<SystemInfoLog>();
            foreach (var companyPair in companyEmployeesWithWarningsDict)
            {
                if (companyPair.Value.IsNullOrEmpty())
                    continue;

                ActionResult result = DisableSystemInfoLogEntries(companyPair.Key, SystemInfoType.RemoveEmployee);
                if (!result.Success)
                    return result;

                foreach (Employee employee in companyPair.Value)
                {
                    SystemInfoLog systemInfoLog = new SystemInfoLog()
                    {
                        Type = (int)SystemInfoType.RemoveEmployee,
                        Entity = (int)SoeEntityType.Employee,
                        LogLevel = (int)SystemInfoLogLevel.Warning,
                        RecordId = employee.EmployeeId,
                        Text = employee.EmployeeNrAndName + " {0} " + employee.GetLastEmployment().GetEndDate().ToShortDateString(),
                        Date = DateTime.Now,
                        DeleteManually = false,

                        //Set FK
                        ActorCompanyId = companyPair.Key,
                        EmployeeId = employee.EmployeeId,
                    };

                    systemInfoLogs.Add(systemInfoLog);
                }
            }

            if (!systemInfoLogs.Any())
                return new ActionResult(true);

            return AddSystemInfoLogEntries(systemInfoLogs);
        }

        public ActionResult DisableSystemInfoLogEntries(int actorCompanyId, SystemInfoType type = SystemInfoType.Unknown)
        {
            ActionResult result = new ActionResult();

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        List<SystemInfoLog> systemInfoLogs = GetSystemInfoLogEntries(entities, actorCompanyId);

                        foreach (SystemInfoLog systemInfoLog in systemInfoLogs)
                        {
                            if (type != SystemInfoType.Unknown && (SystemInfoType)systemInfoLog.Type != type)
                                continue;

                            if (!systemInfoLog.DeleteManually)
                                ChangeEntityState(systemInfoLog, SoeEntityState.Inactive);
                        }

                        result = SaveChanges(entities, transaction);

                        //Commit transaction
                        if (result.Success)
                            transaction.Complete();
                    }
                }
                catch (Exception ex)
                {
                    result.Exception = ex;
                    base.LogError(ex, this.log);
                }
                finally
                {
                    if (!result.Success)
                    {
                        base.LogTransactionFailed(this.ToString(), this.log);
                    }

                    entities.Connection.Close();
                }
            }

            return result;
        }

        #endregion
    }
}
