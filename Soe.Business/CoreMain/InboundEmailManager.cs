using SoftOne.Communicator.Shared.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftOne.Soe.Business.Core
{
    public class InboundEmailManager : ManagerBase
    {
        public InboundEmailManager(ParameterObject parameterObject) : base(parameterObject) { }
        
        public ActionResult HandleInboundEmail(int actorCompanyId, CommunicatorMessage communicatorMessage)
        {
            using (CompEntities entities = new CompEntities())
            {
                return HandleInboundEmail(entities, actorCompanyId, communicatorMessage);
            }
        }
        public ActionResult HandleInboundEmail(CompEntities entities, int actorCompanyId, CommunicatorMessage communicatorMessage)
        {
            var result = ImportEmailAttachmentsToScanningEntry(entities, actorCompanyId, communicatorMessage);
            if (!result.Success)
                SetRedirectEmailOnResult(entities, actorCompanyId, result);

            return result;
        }

        public void SetRedirectEmailOnResult(CompEntities entities, int actorCompanyId, ActionResult result)
        {
            bool isNotificationEnabled = !SettingManager.GetBoolSetting(entities, 
                SettingMainType.Company, 
                (int)CompanySettingType.DisableMessageOnInboundEmailError, 
                userId: 0, actorCompanyId: actorCompanyId, licenseId: 0);

            if (isNotificationEnabled)
            {
                var adminEmail = ContactManager.GetContactEComsFromActor(entities, actorCompanyId, 
                    loadContact: false, 
                    type: TermGroup_SysContactEComType.CompanyAdminEmail)
                    .FirstOrDefault();

                if (adminEmail != null && !string.IsNullOrEmpty(adminEmail.Text))
                    result.StringValue = adminEmail.Text;
            }
        }

        public ActionResult ImportEmailAttachmentsToScanningEntry(CompEntities entities, int actorCompanyId, CommunicatorMessage communicatorMessage)
        {
            // Upload attachments to data storage and then handle through EdiManager.

            var dataStorages = new List<DataStorage>();
            try
            {
                foreach (var attachment in communicatorMessage.MessageAttachments)
                {
                    var dataRecordType = ImageUtil.IsImageBitMapExtension(attachment.Name) ?
                        SoeDataStorageRecordType.InvoiceBitmap :
                        SoeDataStorageRecordType.InvoicePdf;

                    var rawData = attachment.DataBase64;
                    var subject = communicatorMessage.Subject;
                    var dataStorage = GeneralManager.CreateDataStorage(entities,
                        dataRecordType,
                        actorCompanyId: actorCompanyId,
                        data: rawData,
                        description: communicatorMessage.CommunicatorMessageId.ToString(),
                        fileName: attachment.Name,
                        fileSize: rawData.Length,
                        timePeriodId: null,
                        xml: null,
                        employeeId: null);
                    dataStorages.Add(dataStorage);
                }
                entities.SaveChanges();
            }
            catch (Exception ex)
            {
                var result = new ActionResult(ex, "Error when uploading attachments to data storage.");
                result.ErrorNumber = (int)ActionResultSave.ScanningFailed_UploadToDataStorage;
                return result;
            }
            var dataStorageIds = dataStorages.Select(x => x.DataStorageId).ToList();
            if (dataStorageIds.Count == 0)
            {
                var result = new ActionResult("No attachments found in email.");
                result.ErrorNumber = (int)ActionResultSave.NothingSaved;
                return result;
            }
            var communicatorGuid = communicatorMessage.CommunicatorMessageId;
            var batchId = communicatorGuid != null && communicatorGuid != Guid.Empty ? communicatorGuid : Guid.NewGuid();
            return this.EdiManager.SendDocumentsForScanning(entities, actorCompanyId, batchId, dataStorageIds, true);
        }

    }
}
