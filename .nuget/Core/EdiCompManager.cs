using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftOne.EdiAdmin.Business.Core
{
    public class EdiCompManager : ManagerBase
    {
        SoftOne.Soe.Business.Core.EdiManager emBusiness;

        public string GetApplicationSetting(ApplicationSettingType type)
        {
            return (from setting in CompEntities.UserCompanySetting
                    where setting.ActorCompanyId == null &&
                    setting.UserId == null &&
                    setting.DataTypeId == (int)SettingDataType.String &&
                    setting.SettingTypeId == (int)type
                    select setting.StrData).FirstOrDefault();
        }

        public Dictionary<ApplicationSettingType, string> GetApplicationSettingsDictEnum()
        {
            return this.GetApplicationSettingsDictYield().ToDictionary(k => k.Item1, v => v.Item2);
        }

        public Dictionary<string, string> GetApplicationSettingsDict()
        {
            return this.GetApplicationSettingsDictYield().ToDictionary(k => k.Item1.ToString(), v => v.Item2);
        }


        public IEnumerable<Tuple<ApplicationSettingType, string>> GetApplicationSettingsDictYield()
        {
            var dict = new Dictionary<object, string>();
            var ediSettings = new List<ApplicationSettingType>()
            {
                ApplicationSettingType.WholesaleTempFolder,
                ApplicationSettingType.WholesaleErrorFolder,
                ApplicationSettingType.WholesaleSaveFolder,
                ApplicationSettingType.WholesaleRecreateFolder,
                ApplicationSettingType.MsgTempFolder,
                ApplicationSettingType.MsgErrorFolder,
                ApplicationSettingType.MsgSaveFolder,
                ApplicationSettingType.MsgRecreateFolder,
                ApplicationSettingType.ImageTempFolder,
                ApplicationSettingType.ImageErrorFolder,
                ApplicationSettingType.ImageSaveFolder,
                ApplicationSettingType.FtpMsgInputFolder,
                ApplicationSettingType.FtpImageInputFolder,
                ApplicationSettingType.FtpRootOutputFolder,
                ApplicationSettingType.ReceivedMessagesFunction,
                ApplicationSettingType.FtpReceivedMessagesFolder,
                ApplicationSettingType.FileZillaFolder,
                ApplicationSettingType.FtpUser,
                ApplicationSettingType.FtpPassword,
                ApplicationSettingType.EmailAddress,
                ApplicationSettingType.IntervalSecond,
                ApplicationSettingType.StandardTemplatesFolder,
                ApplicationSettingType.SetupErrorEmailAddress,
                ApplicationSettingType.FtpCustomerNotFoundFolder,
                ApplicationSettingType.OnlyMoveFilesMode,
            };
            var settingsInt = ediSettings.Cast<int>();

            var settings = (from setting in CompEntities.UserCompanySetting
                            where setting.ActorCompanyId == null &&
                            setting.UserId == null &&
                            setting.DataTypeId == (int)SettingDataType.String &&
                            setting.SettingTypeId >= 100 &&
                            setting.SettingTypeId <= 150
                            select new { setting.SettingTypeId, setting.StrData }).ToDictionary(k => k.SettingTypeId, v => v.StrData);

            foreach (var item in Enum.GetValues(typeof(ApplicationSettingType)).OfType<ApplicationSettingType>())
            {
                if ((int)item < 100 || (int)item > 150)
                    continue;

                yield return new Tuple<ApplicationSettingType, string>(item, settings.ContainsKey((int)item) ? settings[(int)item] : string.Empty);
            }
        }

        public int GetEdiRecivedMsgIdFromFileName(string fileName)
        {
            var query = (from entry in CompEntities.EdiReceivedMsg
                         where fileName.Contains(entry.UniqueId)
                         select entry.EdiReceivedMsgId).FirstOrDefault();

            return query;
        }

        public EdiReceivedMsg GetEdiRecivedMsgFromFileName(string fileName)
        {
            return (from entry in CompEntities.EdiReceivedMsg
                    where fileName.Contains(entry.UniqueId)
                    select entry).FirstOrDefault();
        }

        public IQueryable<EdiReceivedMsg> GetEdiRecivedMsgByFileName(string fileName, int sysWholesellerEdiId, int? actorCompanyId)
        {
            if (actorCompanyId == 0)
                actorCompanyId = null;

            return (from entry in CompEntities.EdiTransfer
                    where
                    (!actorCompanyId.HasValue || entry.ActorCompanyId == actorCompanyId.Value) &&
                    entry.EdiReceivedMsg.InFilename == fileName &&
                    entry.SysWholesellerEdiId == sysWholesellerEdiId
                    select entry.EdiReceivedMsg);
        }

        public void DeleteEdiTransfer(IEnumerable<int> ediTransferId, bool alsoRemoveRecivedMsg)
        {
            using (var entities = new SOECompEntities())
            {
                var toRemove = entities.EdiTransfer.Where(et => ediTransferId.Contains(et.EdiTransferId)).ToList();
                int ediReceivedMsgId = 0;
                foreach (var item in toRemove)
                {
                    if (item.EdiReceivedMsgId.HasValue)
                        ediReceivedMsgId = item.EdiReceivedMsgId.Value;

                    entities.EdiTransfer.DeleteObject(item);
                }

                if (ediReceivedMsgId > 0 && alsoRemoveRecivedMsg)
                {
                    var ediRecived = entities.EdiReceivedMsg.Where(r => r.EdiReceivedMsgId == ediReceivedMsgId).FirstOrDefault();
                    entities.EdiReceivedMsg.DeleteObject(ediRecived);
                }

                SaveChanges(entities);
            }
        }

        public ActionResult SaveEdiTransferAndEdiEntry(SOECompEntities entities, EdiTransfer ediTransfer, int? actorCompanyId, int sysScheduledJobId, int batchNr)
        {
            ActionResult result = new ActionResult();
            if (actorCompanyId == 0)
                actorCompanyId = null;

            if (emBusiness == null)
                emBusiness = new Soe.Business.Core.EdiManager(null);

            if (ediTransfer.State == (int)EdiTransferState.Unknown)
                ediTransfer.State = (int)EdiTransferState.UnderProgress;

            ediTransfer.TransferDate = DateTime.Now;
            ediTransfer.ActorCompanyId = actorCompanyId;

            result = SaveChanges(entities);
            if (!result.Success)
                return result;

            result.IntegerValue = ediTransfer.EdiTransferId;
            if (!actorCompanyId.HasValue)
            {
                if (ediTransfer.State != (int)EdiTransferState.EdiCompNotFoundTryingOtherEnv)
                    Console.Error.WriteLine("Error: SaveEdiTransferAndEdiEntry, Edi message, customer not found. EdiTransferId = {0}", ediTransfer.EdiTransferId);
                if (ediTransfer.State == (int)EdiTransferState.Unknown || ediTransfer.State == (int)EdiTransferState.UnderProgress)
                    ediTransfer.State = (int)EdiTransferState.EdiCompanyNotFound;

                result = SaveChanges(entities);
                return result;
            }
            // Transfer to EdiEntry
            var dto = new CompanyEdiDTO()
            {
                ActorCompanyId = actorCompanyId.Value,
                Source = ediTransfer.Xml,
                SourceType = CompanyEdiDTO.SourceTypeEnum.Xml,
                Created = DateTime.Now,
                FileName = ediTransfer.OutFilename,
            };

            result = emBusiness.AddEdiEntrysFromSource(dto, false, sysScheduledJobId: sysScheduledJobId, batchNr: batchNr);
            if (!result.Success)
            {
                ediTransfer.State = (int)EdiTransferState.Error;
                string msg = "Fel vid inläsning till XE av fil: " + ediTransfer.OutFilename + (Enum.IsDefined(typeof(ActionResultSave), result.ErrorNumber) ? ". Felnr: " + ((ActionResultSave)result.ErrorNumber).ToString() : string.Empty) + ". Felmeddelande: " + result.ErrorMessage;
                Console.Error.WriteLine(msg);
            }
            else
            {
                if (result.SuccessNumber == (int)ActionResultSave.Duplicate)
                {
                    ediTransfer.State = (int)EdiTransferState.Duplicate;
                    Console.Error.WriteLine("Duplicate entry found, no EdiEntry created from EdiTransferId = {0}, ActorCompanyId = {1}", ediTransfer.EdiTransferId, actorCompanyId);
                }
                else
                {
                    ediTransfer.State = (int)EdiTransferState.Transferred;
                }
            }

            result = SaveChanges(entities);

            return result;
        }


        internal EdiTransfer GetEdiTransferByFileName(string fileName, SOECompEntities entities = null)
        {
            if (entities == null)
                entities = CompEntities;

            return (from entry in entities.EdiTransfer.Include("EdiReceivedMsg")
                    where entry.EdiReceivedMsg != null && fileName.Contains(entry.EdiReceivedMsg.UniqueId) && entry.State != (int)EdiTransferState.Transferred
                    select entry).FirstOrDefault();
        }

        internal IEnumerable<EdiTransfer> GetEdiTransfersByState(EdiTransferState state, SOECompEntities entities = null)
        {
            if (entities == null)
                entities = CompEntities;

            return (from entry in entities.EdiTransfer.Include("EdiReceivedMsg")
                    where entry.EdiReceivedMsg != null && entry.State == (int)state
                    select entry);
        }


        internal IEnumerable<EdiReceivedMsg> GetEdiRecivedByState(EdiRecivedMsgState ediRecivedMsgState, SOECompEntities entities = null)
        {
            if (entities == null)
                entities = CompEntities;

            return (from entry in entities.EdiReceivedMsg.Include("EdiTransfer")
                    where entry.State == (int)ediRecivedMsgState && entry.EdiTransfer.Count == 0
                    select entry);
        }
    }
}
