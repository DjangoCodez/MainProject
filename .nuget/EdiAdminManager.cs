using SoftOne.EdiAdmin.Business.Core;
using SoftOne.EdiAdmin.Business.Interfaces;
using SoftOne.EdiAdmin.Business.Senders;
using SoftOne.EdiAdmin.Business.Util;
using SoftOne.EdiAdmin.Business.Util.MessageParsers;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Util;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SoftOne.EdiAdmin.Business
{
    public class EdiAdminManager : EdiAdminManagerBase, IEdiAdminParseManager
    {
        SoftOne.Soe.Business.Core.EdiManager soeEdiManager;

        public EdiAdminManager(IMessageParser parser) : base(parser)
        {
            this.soeEdiManager = new SoftOne.Soe.Business.Core.EdiManager(null);
                }

        private bool TryMoveFileToOtherEnviroment(SOECompEntities entities, EdiTransfer transfer)
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
                directories.Add(DbName);
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
                FtpUtility.UploadData(new Uri(notFoundFtpFolder + DbName + "/" + transfer.OutFilename), data, ftpUser, ftpPassword);
                return true;
            }

            return false;
            }

        /// <summary>
        /// Main entrance point when fetching messages from ftp
        /// </summary>
        /// <returns></returns>
        public ActionResult ParseMessages()
        {
            bool hasErrors = false;
            var result = new ActionResult();

            Console.Out.WriteLine("Parsing messages from softone FTP");
            result = this.ParseMessageFromSoftOneFtp();
            if (result.Success)
                Console.Out.WriteLine("{0} messages fetched and {1} messages parsed(converted) from softone FTP", result.IntegerValue, result.IntegerValue2);
            else
                hasErrors = true;

            Console.Out.WriteLine("Parsing messages from external FTP");
            result = this.ParseMessagesFromExternalFtp();
            if (result.Success)
                Console.Out.WriteLine("{0} messages fetched and {1} messages parsed(converted) from external FTP", result.IntegerValue, result.IntegerValue2);
            else
                hasErrors = true;

            Console.Out.WriteLine("Parsing messages with state retry");
            result = this.ParseMessagesWithStateRetry();
            if (result.Success)
                Console.Out.WriteLine("{0} messages fetched and {1} messages parsed(converted) with state retry", result.IntegerValue, result.IntegerValue2);
            else
                hasErrors = true;

            result.Success = !hasErrors;
            return result;
        }

        public ActionResult ParseMessagesWithStateRetry()
        {
            int nrOfProcessed = 0;
            int nrOfFetched = 0;
            bool success = true;
            ActionResult result = new ActionResult();
            try
            {
                using (var entities = new SOECompEntities())
                {
                    var ediTransfers = this.EdiCompManager.GetEdiTransfersByState(EdiTransferState.Retry, entities).ToList();

                    foreach (var item in ediTransfers)
                    {
                        try
                        {
                            nrOfFetched++;
                            // Make sure we don't end up in a loop
                            item.State = (int)EdiTransferState.UnderProgress;

                            // Find the correct xe customer
                            if (!item.ActorCompanyId.HasValue || item.ActorCompanyId == 0)
                                item.ActorCompanyId = this.GetActorCompanyIdFromFile(entities, item);

                            result = this.EdiCompManager.SaveEdiTransferAndEdiEntry(entities, item, item.ActorCompanyId, this.sysScheduledJobId, this.batchNr);
                            if (result.Success)
                                nrOfProcessed++;
                            else
                                success = false;
                        }
                        catch (Exception ex)
                        {
                            item.State = (int)EdiTransferState.Error;
                            item.ErrorMessage = string.Format("Error in ParseMessageWithStateRetry, ediTransferId = {0}", item.EdiTransferId);

                            SharedProperties.LogError(ex, item.ErrorMessage);
                            SaveChanges(entities);
                        }
                    }

                    var ediRecived = this.EdiCompManager.GetEdiRecivedByState(EdiRecivedMsgState.Retry, entities).ToList();

                    foreach (var item in ediRecived)
                    {
                        try
                        {
                            nrOfFetched++;
                            if (!item.SysWholesellerEdiId.HasValue)
                            {
                                item.State = (int)EdiRecivedMsgState.Error;
                                item.ErrorMessage = string.Format("Could not find SysWholesellerEdi, please add it manually on EdiReceivedMsgId = {0}", item.EdiReceivedMsgId); ;
                                SharedProperties.LogError(item.ErrorMessage);
                                continue;
                            }

                            item.State = (int)EdiRecivedMsgState.UnderProgress;
                            var wholeseller = this.EdiSysManager.GetSysWholesellerEDI(item.SysWholesellerEdiId.Value);

                            result = this.messageParser.ParseMessageToEdiTransfer(entities, item, wholeseller);

                            if (result.Success)
                                nrOfProcessed += result.IntegerValue;
                            else
                                success = false;
                        }
                        catch (Exception ex)
                        {
                            item.State = (int)EdiTransferState.Error;
                            item.ErrorMessage = string.Format("Error in ParseMessageWithStateRetry, ediRecivedMsgId = {0}", item.EdiReceivedMsgId);
                            SharedProperties.LogError(ex, item.ErrorMessage);
                            SaveChanges(entities);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                SharedProperties.LogError(ex, "Error when when retrying to parse messages");
                success = false;
            }

            result.IntegerValue = nrOfFetched;
            result.IntegerValue = nrOfProcessed;
            result.Success = success;
            return result;
        }

        private ActionResult ParseMessageFromSoftOneFtp()
        {
            var ftpUser = SharedProperties.DrEdiSettings[ApplicationSettingType.FtpUser];
            var ftpPassword = SharedProperties.DrEdiSettings[ApplicationSettingType.FtpPassword];

            return ParseMessageFromSoftOneFtp(ftpUser, ftpPassword);
        }

        public ActionResult ParseMessageFromSoftOneFtp(string ftpUser, string ftpPassword)
        {
            ActionResult result = new ActionResult();

            var sysWholesellerEdis = this.EdiSysManager.GetSysWholesellerEDIs(SysWholesellerEdiManagerType.EdiAdminManager);
            int nrOfFetched = 0, nrOfProcessed = 0;
            var fetcher = new EdiFetcherFTP(ftpUser, ftpPassword);
            
            // Check wholesellers who send to our ftp
            foreach (var sysWholesellerEdi in sysWholesellerEdis)
            {
                try
                {
                    if (sysWholesellerEdi == null)
                    {
                        Console.Error.WriteLine("WholesellerEdi was null, continuing with next wholeseller");
                        continue;
                    }
                    else if (string.IsNullOrEmpty(sysWholesellerEdi.EdiFolder))
                    {
                        continue;
                    }

                    SysWholesellerEdiIdEnum ediEnum = Enum.IsDefined(typeof(SysWholesellerEdiIdEnum), sysWholesellerEdi.SysWholesellerEdiId) ? (SysWholesellerEdiIdEnum)sysWholesellerEdi.SysWholesellerEdiId : SysWholesellerEdiIdEnum.Unknown;
                    if (ediEnum == SysWholesellerEdiIdEnum.Unknown)
                    {
                        SharedProperties.LogError("Warning, syswholeselleredi did not have a corrensponding enum in SysWholesellerEdiIdEnum. WholesellerEdiId = {0}", sysWholesellerEdi.SysWholesellerEdiId);
                    }

                    var innerResult = this.messageParser.ParseMessageFromWholeseller(fetcher, sysWholesellerEdi.EdiFolder, sysWholesellerEdi.SysWholesellerEdiId, 0);

                    if (innerResult.Success)
                    {
                        nrOfFetched += innerResult.IntegerValue;
                        nrOfProcessed += innerResult.IntegerValue2;
                    }
                    else
                        result = innerResult;
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine("General error inside ParseMessageFromSoftOneFtp(): Exception message {0}", ex.GetInnerExceptionMessages().JoinToString(", Inner: "));
                }       
            }

            // Now check the error folder
            var resultMain = new ActionResult();
            string baseUrl = SharedProperties.DrEdiSettings[ApplicationSettingType.FtpCustomerNotFoundFolder].Trim('/') + '/';

            var folders = FtpUtility.GetFileList(new Uri(baseUrl), ftpUser, ftpPassword, onlyFolders: true);
            folders = folders.Where(f => f.ToUpper() != this.DbName.ToUpper()).ToList();

            result = this.ParseMessagesFromCustomerNotFoundFtp(fetcher, folders);

            result.IntegerValue = nrOfFetched;
            result.IntegerValue2 = nrOfProcessed;

            return result;
                        }

        public ActionResult ParseMessagesFromExternalFtp()
        {
            var result = new ActionResult();
            int noOfFiles = 0;
            int noOfFilesProcessed = 0;

            // Check nelfo and lvis messages from external ftp
            // Find all enums except Unknown and Symbrio
            var enumsToFind = (from entry in Enum.GetValues(typeof(TermGroup_CompanyEdiType)).OfType<TermGroup_CompanyEdiType>()
                               where entry != TermGroup_CompanyEdiType.Unknown && entry != TermGroup_CompanyEdiType.Symbrio
                               select entry).ToArray();

            // Execute job
            var companyEdis = this.soeEdiManager.GetCompanyEdis(enumsToFind).ToDTOs().OrderBy(i => i.CompanyName).ThenBy(i => i.ActorCompanyId).ToList();
            var companyEdiGroups = companyEdis.GroupBy(i => i.ActorCompanyId).ToList();

            if (companyEdiGroups != null && companyEdiGroups.Count > 0)
            {
                var cm = new SoftOne.Soe.Business.Core.CompanyManager(null);
                foreach (var companyEdi in companyEdiGroups)
                {
                    // Company, only fetch from companies that has uses edi via xe:
                    var company = cm.GetCompany(companyEdi.Key, loadEdiConnection: true);
                    if (company == null)
                        continue;
                    else if (company.EdiConnection.IsLoaded && company.EdiConnection.Count == 0)
                        continue;

                    foreach (var externalCompanyEdi in companyEdi)
                    {
                        var innerResult = this.ParseMessageFromExternalEdi(externalCompanyEdi, company);

                        if (innerResult.Success)
                        {
                            noOfFiles = innerResult.IntegerValue;
                            noOfFilesProcessed += innerResult.IntegerValue2;
                        }
                        if (!innerResult.Success)
                            result = innerResult;
                    }
                }
            }

            if (result.Success)
            {
                result.IntegerValue = noOfFiles;
                result.IntegerValue2 = noOfFilesProcessed;
            }
            return result;
        }

        public ActionResult ParseMessageFromExternalEdi(CompanyEdiDTO externalCompanyEdi, Company company)
        {
            if (externalCompanyEdi == null)
                return new ActionResult(false);

            if (string.IsNullOrEmpty(externalCompanyEdi.Source) || string.IsNullOrEmpty(externalCompanyEdi.Username) || string.IsNullOrEmpty(externalCompanyEdi.Password))
            {
                Console.Error.WriteLine("Company is missing password and or username for external ftp, company: " + company.Name);
                return new ActionResult(false);
            }

            // TODO, this is just temporary. SysWholesellerId should be stored in CompanyEdi table
            int sysWholesellerEdiId = 0;
            switch ((TermGroup_CompanyEdiType)externalCompanyEdi.Type)
            {
                case TermGroup_CompanyEdiType.Nelfo:
                    sysWholesellerEdiId = 36;
                    break;
                case TermGroup_CompanyEdiType.LvisNet:
                    sysWholesellerEdiId = 42;
                    break;
                default:
                    return new ActionResult(false, (int)ActionResultSave.Unknown, "ExternalCompanyEdi type not supported");
            }

            // Ignore some files that contains prices, TODO this is a mockup
            string[] toIgnore = { 
                                    "otrapris-nelfo1.txt",
                                    "V4priserNRF.all",
                                    "V4varefil.zip",
                                    "R4rabatt.txt",
                                    "V4priser.all",
                                    "V4priser.kost",
                                };

            var fetcher = new EdiFetcherFTP(externalCompanyEdi.Username, externalCompanyEdi.Password);
            var innerResult = this.messageParser.ParseMessageFromWholeseller(fetcher, externalCompanyEdi.Source, sysWholesellerEdiId, externalCompanyEdi.ActorCompanyId, toIgnore);
            //var sysWholesellerEdi = this.EdiSysManager.GetSysWholesellerEDI(sysWholesellerEdiId);

            //var innerResult = this.ParseMessageFromWholeseller(externalCompanyEdi, sysWholesellerEdi, toIgnore);

            return innerResult;
        }

        private ActionResult ParseMessagesFromCustomerNotFoundFtp(IEdiFetcher fetcher, IEnumerable<string> folders)
        {
            var resultMain = new ActionResult();
            string baseUrl = SharedProperties.DrEdiSettings[ApplicationSettingType.FtpCustomerNotFoundFolder].Trim('/') + '/';
            foreach (var item in folders)
            {
                OnFileFetchedDelegate fileFetched = (fileName, fullPath, data) =>
                {
            using (var entities = new SOECompEntities())
            {
                        bool deleteFile = false;
                        var innerResult = new ActionResult();
                    // Validate that this file has not been downloaded before
                        var ediTransfer = EdiCompManager.GetEdiTransferByFileName(fileName, entities);

                        if (ediTransfer == null)
                    {
                            string fileContent = System.Text.Encoding.UTF8.GetString(data);
                            ediTransfer = new EdiTransfer()
                    {
                        Xml = fileContent,
                            OutFilename = fileName,
                        State = (int)EdiTransferState.Error,
                        };

                        entities.EdiTransfer.AddObject(ediTransfer);
            }

                        innerResult = this.SaveChanges(entities);
                        if (innerResult.Success)
            {
                            // Delete file
                            fetcher.DeleteFile(fullPath);
            }

                        // Try to transfer file
                        int? actorCompanyId = this.GetActorCompanyIdFromFile(entities, ediTransfer);

                        if (actorCompanyId.HasValue)
            {
                            EdiCompManager.SaveEdiTransferAndEdiEntry(entities, ediTransfer, actorCompanyId.Value, this.sysScheduledJobId, this.batchNr);
            }
            }
                };

                fetcher.GetContent(baseUrl + item, fileFetched);
        }

            return resultMain;
        }

        public string GetEdiSetting(ApplicationSettingType settingType)
        {
            if (SharedProperties.DrEdiSettings == null)
                return null;

            return SharedProperties.DrEdiSettings[settingType];
        }
    }
}
