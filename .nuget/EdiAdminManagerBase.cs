using SoftOne.EdiAdmin.Business.Core;
using SoftOne.EdiAdmin.Business.Interfaces;
using SoftOne.EdiAdmin.Business.Util;
using SoftOne.EdiAdmin.Business.Util.MessageParsers;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SoftOne.EdiAdmin.Business
{
    public abstract class EdiAdminManagerBase : ManagerBase, IEdiAdminManager
    {
        public event EventHandler<EdiAdminStreamWriterEventArgs> OnOutputRecived;
        protected int sysScheduledJobId;
        protected int batchNr;

        protected IMessageParser messageParser;

        private string dbName;
        public string DbName
        {
            get
            {
                if (this.dbName == null)
                {
                    // Get the connection string from entity framework 
                    var dbConnection = ((System.Data.Entity.Core.EntityClient.EntityConnection)this.CompEntities.Connection).StoreConnection;
                    var entityBuilder = new System.Data.SqlClient.SqlConnectionStringBuilder(dbConnection.ConnectionString);
                    this.dbName = entityBuilder.InitialCatalog;
                }

                return this.dbName;
            }
        }

        public EdiAdminManagerBase(IMessageParser messageParser)
        {
            this.messageParser = messageParser;
        }

        public virtual void Setup(bool redirectOutPut, int? sysScheduledJobId = null, int? batchNr = null)
        {
            this.messageParser.OnMessageParsed += this.messageParser_OnMessageParsed;
            if (sysScheduledJobId.HasValue)
                this.sysScheduledJobId = sysScheduledJobId.Value;
            if (batchNr.HasValue)
                this.batchNr = batchNr.Value;

            if (redirectOutPut)
            {
                var errorWriter = new EdiAdminStreamWriter(EventLogEntryType.Error);
                var outputWriter = new EdiAdminStreamWriter(EventLogEntryType.Information);
                errorWriter.Output += this.OnOutputRecived;
                outputWriter.Output += this.OnOutputRecived;
                Console.SetError(errorWriter);
                Console.SetOut(outputWriter);
            }

            if (SharedProperties.DrEdiSettings == null)
            { 
                var tmpDict = new Dictionary<ApplicationSettingType, string>();

                //Hämta Inställningar
                foreach (var item in this.EdiCompManager.GetApplicationSettingsDictYield())
                {
                    tmpDict.Add(item.Item1, item.Item2);
                }

                if (!string.IsNullOrEmpty(Constants.SOE_EDI_WHOLESALESAVEFOLDER) && Directory.Exists(Constants.SOE_EDI_WHOLESALESAVEFOLDER))
                    tmpDict[ApplicationSettingType.WholesaleSaveFolder] = Constants.SOE_EDI_WHOLESALESAVEFOLDER;

                SharedProperties.DrEdiSettings = new System.Collections.Concurrent.ConcurrentDictionary<ApplicationSettingType, string>(tmpDict);

                Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

                if (File.Exists("FileDefinitions/StandardMall.xml"))
                {
                    SharedProperties.DsStandardMall.ReadXml("FileDefinitions/StandardMall.xml");
                    SharedProperties.DrEdiSettings[ApplicationSettingType.StandardTemplatesFolder] = AppDomain.CurrentDomain.BaseDirectory + "/FileDefinitions";
                }
                else if (File.Exists(SharedProperties.DrEdiSettings[ApplicationSettingType.StandardTemplatesFolder] + "\\StandardMall.xml"))
                {
                    SharedProperties.DsStandardMall.ReadXml(SharedProperties.DrEdiSettings[ApplicationSettingType.StandardTemplatesFolder] + "\\StandardMall.xml");
                }
                else
                {
                    Console.Error.WriteLine("Could not find standard template StandardMall.xml, will continue without standard template.");
                }
            }

            try
            {
                Console.WriteLine(this.GetStatusMessage());
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Error, could not get status message for service. Ex: " + ex.Message);
            }
        }

        public virtual string GetStatusMessage()
        {
            return string.Format("EdiAdminManager status: Data Source: {0}, ftpUser: {1}, templates folder: {2}, machine: {3}, EdiAdmin.Business version: {4}", this.DbName, SharedProperties.DrEdiSettings[ApplicationSettingType.FtpUser], SharedProperties.DrEdiSettings[ApplicationSettingType.StandardTemplatesFolder], System.Environment.MachineName, this.GetType().Assembly.GetName().Version.ToString(4));
        }

        private ActionResult messageParser_OnMessageParsed(SOECompEntities entities, EdiTransfer ediTransfer, SysWholesellerEdi sysWholesellerEdi)
        {
            int? actorCompanyId = ediTransfer.ActorCompanyId;

            // Find the correct xe customer
            if (!actorCompanyId.HasValue || actorCompanyId == 0)
                actorCompanyId = this.GetActorCompanyIdFromFile(entities, ediTransfer, sysWholesellerEdi);

            var result = this.EdiCompManager.SaveEdiTransferAndEdiEntry(entities, ediTransfer, actorCompanyId, this.sysScheduledJobId, this.batchNr);

            return result;
        }

        protected int? GetActorCompanyIdFromFile(SOECompEntities entities, EdiTransfer ediTransfer, SysWholesellerEdi sysWholesellerEdi = null)
        {
            var msg = GetMessageObjFromXml(ediTransfer.Xml);

            //Validate message
            if (msg.Buyer == null || msg.Buyer.BuyerId == null || msg.MessageInfo.MessageType == null)
            {
                ediTransfer.ErrorMessage = string.Format("Error: Edi-message is missing critical information, missing BuyerId, RecivedMsgId = {0}", ediTransfer.EdiReceivedMsgId);
                Console.Error.WriteLine(ediTransfer.ErrorMessage);
                return null;
            }

            string buyerNr = msg.Buyer.BuyerId.Trim();
            string buyerName = msg.Buyer.BuyerName;
            string messageType = msg.MessageInfo.MessageType.Trim().ToUpper();
            string senderNr = msg.MessageInfo.MessageSenderId.Trim().ToUpper();
            string sellerName = msg.Seller.SellerName;
            string sellerOrderNr = msg.Head.HeadSellerOrderNumber;

            if (sysWholesellerEdi == null)
            {
                if (!ediTransfer.SysWholesellerEdiId.HasValue)
                {
                    // Try to find correct wholeseller by file info
                    sysWholesellerEdi = this.EdiSysManager.GetSysWholesellerEDI(senderNr, sellerName);
                }
                else if (ediTransfer.SysWholesellerEdiId > 0)
                {
                    sysWholesellerEdi = this.EdiSysManager.GetSysWholesellerEDI(ediTransfer.SysWholesellerEdiId.Value);
                }
            }

            if (sysWholesellerEdi == null)
            {
                ediTransfer.ErrorMessage = string.Format("No SysWholesellerEdi was found where EdiTransferId = {0} and EdiRecivedMsg = {1}", ediTransfer.EdiTransferId, ediTransfer.EdiReceivedMsgId);
                Console.Error.WriteLine(ediTransfer.ErrorMessage);
                return null;
            }

            if (!sysWholesellerEdi.SysEdiMsg.IsLoaded)
                sysWholesellerEdi.SysEdiMsg.Load();

            List<int> sysEdiMsgIds = sysWholesellerEdi.SysEdiMsg.Select(m => m.SysEdiMsgId).ToList();
            var query = (from conn in entities.EdiConnection
                         where sysEdiMsgIds.Contains(conn.SysEdiMsgId) &&
                         conn.BuyerNr == buyerNr
                         select conn.ActorCompanyId).Distinct();

            int count = query.Count();

            if (count == 0)
            {
                // Try new enviroment
                bool isInNewEnviroment = this.CustomerNotFound(entities, ediTransfer);

                if (isInNewEnviroment)
                {
                    // Do not log as error
                    ediTransfer.ErrorMessage = string.Format("Customer not found so message moved to new enviroment. EdiRecivedId = {0}, EdiTransferId = {1}", ediTransfer.EdiReceivedMsgId, ediTransfer.EdiTransferId);
                    Console.Out.WriteLine(ediTransfer.ErrorMessage);
                }
                else
                {
                    // TODO, we can always search the customer by name or maybe check the wholeseller sendernr
                    ediTransfer.ErrorMessage = string.Format("Error, customer could not be found, searching for customer nr {0} on sysEdiMsgIds ({1}). Other info: CustomerName {2}, RecivedMsgId: {3}, SysWholeseller: {4}, MessageType: {5}, MessageSenderId:  {6}",
                        buyerNr, sysEdiMsgIds.JoinToString(","), msg.Buyer.BuyerName, ediTransfer.EdiReceivedMsgId, sysWholesellerEdi.SenderName, messageType, senderNr);
                    Console.Error.WriteLine(ediTransfer.ErrorMessage);
                }

            }
            else if (count > 1)
            {
                ediTransfer.ErrorMessage = string.Format("Error, {0} customers found when searching for customer nr {1} on sysEdiMsgIds ({2]). CustomerName in file is {3}. WholesellerName={4}. RecivedMsgId={5}", count, buyerNr, sysEdiMsgIds.JoinToString(","), buyerName, sysWholesellerEdi.SenderName, ediTransfer.EdiReceivedMsgId);
                Console.Error.WriteLine(ediTransfer.ErrorMessage);
            }
            else
            {
                return query.FirstOrDefault();
            }

            // Only for errors
            return null;
        }

        protected bool CustomerNotFound(SOECompEntities entities, EdiTransfer transfer)
        {
            // Move it back to ftp to let other enviroment check if it has the customer in the db.
            var notFoundFtpFolder = SharedProperties.DrEdiSettings[ApplicationSettingType.FtpCustomerNotFoundFolder].TrimEnd('/') + '/';
            var ftpUser = SharedProperties.DrEdiSettings[ApplicationSettingType.FtpUser];
            var ftpPassword = SharedProperties.DrEdiSettings[ApplicationSettingType.FtpPassword];
            Exception ex;
            var originalFileName = transfer.OutFilename;

            // TODO only show directories here
            var directories = FtpUtility.GetFileList(new Uri(notFoundFtpFolder), ftpUser, ftpPassword, onlyFolders: true);

            if (!directories.Any(d => d.ToLower() == this.DbName.ToLower()))
            {
                FtpUtility.MakeDirectory(new Uri(notFoundFtpFolder + this.DbName), ftpUser, ftpPassword, out ex);
                directories.Add(this.DbName);
            }

            bool allTried = directories.All(f => transfer.OutFilename.ToLower().Contains(f.ToLower()));
            if (allTried || directories.Count() == 1)
            {
                transfer.State = (int)EdiTransferState.EdiCompanyNotFound;
                return false;
            }
            else if (!transfer.OutFilename.ToLower().Contains(this.DbName.ToLower()))
            {
                transfer.State = (int)EdiTransferState.EdiCompNotFoundTryingOtherEnv;
                transfer.OutFilename = this.DbName + "_" + originalFileName;
                byte[] data = SerializeUtil.GetBytes(transfer.Xml);
                FtpUtility.UploadData(new Uri(notFoundFtpFolder + this.DbName + "/" + transfer.OutFilename), data, ftpUser, ftpPassword);
                return true;
            }

            return false;
        }

        protected SoftOne.Soe.EdiAdmin.Business.FileDefinitions.Message GetMessageObjFromXml(string content)
        {
            // Parse file to object
            XmlSerializer serializer = new XmlSerializer(typeof(SoftOne.Soe.EdiAdmin.Business.FileDefinitions.Message));
            using (StringReader reader = new StringReader(content))
            {
                var message = (SoftOne.Soe.EdiAdmin.Business.FileDefinitions.Message)(serializer.Deserialize(reader));

                return message;
            }
        }

    }
}
