using SoftOne.EdiAdmin.Business.Core;
using SoftOne.EdiAdmin.Business.Interfaces;
using SoftOne.EdiAdmin.Business.Senders;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SoftOne.EdiAdmin.Business.Util.MessageParsers
{
    public class MessageParser : ManagerBase, IMessageParser
    {
        private int filesParsed = 0;
        private int filesFound = 0;

        public MessageParser()
        {
        }

        public ActionResult ParseMessageFromWholeseller(IEdiFetcher fetcher, string source, int sysWholesellerEdiId, int actorCompanyId, params string[] ignoreFilesList)
        {
            ActionResult result;
            result = fetcher.GetContent(source, (fileName, fullPath, data) =>
            {
                this.filesFound++;
                result = this.ParseFetchedMessage(fetcher, source, sysWholesellerEdiId, actorCompanyId, ignoreFilesList, fileName, fullPath, data);
                if(result.Success)
                    this.filesParsed += result.IntegerValue;
            });

            result.IntegerValue = filesFound;
            result.IntegerValue2 = filesParsed;

            return result;
        }

        private ActionResult ParseFetchedMessage(IEdiFetcher fetcher, string source, int sysWholesellerEdiId, int actorCompanyId, string[] ignoreFilesList, string fileName, string fullPath, byte[] data)
        {
            var sysWholesellerEdi = EdiSysManager.GetSysWholesellerEDI(sysWholesellerEdiId, true);
            using (var entities = new SOECompEntities())
            {
                var result = new ActionResult();
                // Validate that this file has not been downloaded before
                bool isDuplicate = SharedProperties.DoNotCheckDuplicates ? false : EdiCompManager.GetEdiRecivedMsgByFileName(fileName, sysWholesellerEdi.SysWholesellerEdiId, actorCompanyId.ToNullable(0)).Any();

                // TODO, encoding should be saved in syswholeselleredi (if not standard encoding)
                string fileContent;
                var encoding = KlerksSoft.TextFileEncodingDetector.DetectTextByteArrayEncoding(data);
                if (encoding == null)
                    encoding = Constants.ENCODING_LATIN1; // Default encoding

                fileContent = encoding.GetString(data);

                Guid uniqueId = Guid.NewGuid();
                string fetcherType = fetcher.GetType().Name;
                string fileNameBase = fetcherType + "_" + uniqueId.ToString();

                // Move to EDIRecived
                var recived = new EdiReceivedMsg()
                {
                    MsgDate = DateTime.Now,
                    InFilename = fileName,
                    SysWholesellerEdiId = sysWholesellerEdi.SysWholesellerEdiId,
                    OutFilename = fileNameBase + ".xml",
                    FileData = fileContent,
                    State = isDuplicate ? (int)EdiRecivedMsgState.Duplicate : (int)EdiRecivedMsgState.UnderProgress,
                    UniqueId = uniqueId.ToString(),
                    ActorCompanyId = actorCompanyId.ToNullable(0),
                    EdiType = fetcherType,
                };

                entities.EdiReceivedMsg.AddObject(recived);
                result = this.SaveChanges(entities);

                if (result.Success)
                {
                    // Moved to edirecived so we can now delete the file
                    fetcher.DeleteFile(fullPath);

                    // Save to file disc as backup (since we don't save byte array to db).
                    if (!SharedProperties.DoNotBackupFilesToDics)
                    { 
                        try
                        {
                            string folder = SharedProperties.DrEdiSettings[ApplicationSettingType.WholesaleSaveFolder];
                            if (!Directory.Exists(folder))
                                Directory.CreateDirectory(folder);
                            string filePath = folder + "/" + fileName;
                            if (File.Exists(fileName))
                                filePath = uniqueId + fileName;

                            File.WriteAllBytes(filePath, data);
                        }
                        catch (Exception)
                        {
                            SharedProperties.LogError("Could not save file to local disc, please check your settings. EdiRecivedMsgId = {0}", recived.EdiReceivedMsgId);
                        }
                    }
                }
                else
                {
                    Console.Error.WriteLine(String.Format("Error when trying to create EdiRecivedMsg: ErrorNr{0} - ErrorMessage:'{1}' Wholeseller({2})", result.ErrorNumber, result.ErrorMessage, sysWholesellerEdi.SenderName));
                    return result;
                }

                // If duplicate this is as far as we want to go
                if (isDuplicate)
                {
                    var message = string.Format("Error, duplicate edi filename when fetching message for actorCompanyId {0}, filename {1}, EdiRecivedMsgId = {2}. File has been removed from server and not transferred to XE", actorCompanyId, fileName, recived.EdiReceivedMsgId);
                    recived.ErrorMessage = message;
                    recived.State = (int)EdiRecivedMsgState.Duplicate;
                    Console.Error.WriteLine(message);
                    SaveChanges(entities);
                    return result;
                }

                // Parse message and map to customer and transfer
                result = this.ParseMessageToEdiTransfer(entities, recived, sysWholesellerEdi);
                if (result.Success)
                    filesParsed += result.IntegerValue;

                return result;
            }
        }

        public ActionResult ParseMessageToEdiTransfer(SOECompEntities entities, EdiReceivedMsg recivedMsg, SysWholesellerEdi sysWholesellerEdi)
        {
            ActionResult result = new ActionResult(false);
            bool success = true;
            string fileNameBase = recivedMsg.OutFilename.Replace(".xml", string.Empty);

            try
            {
                var parsedMessages = this.ParseMessage(recivedMsg.SysWholesellerEdiId, recivedMsg.InFilename, recivedMsg.FileData);

                if (parsedMessages != null && parsedMessages.Count() > 0)
                {
                    int counter = 1;
                    int? actorCompanyId = recivedMsg.ActorCompanyId.ToNullable();

                    foreach (var item in parsedMessages)
                    {
                        string fileName = string.Concat(fileNameBase, (counter > 1 ? "_" + counter : string.Empty), ".xml");

                        // Create the edi transfer
                        var ediTransfer = new EdiTransfer()
                        {
                            EdiReceivedMsg = recivedMsg,
                            ActorCompanyId = actorCompanyId,
                            Xml = item,
                            OutFilename = fileName,
                            SysWholesellerEdiId = sysWholesellerEdi.SysWholesellerEdiId,
                            State = (int)EdiRecivedMsgState.UnderProgress,
                        };
                        entities.EdiTransfer.AddObject(ediTransfer);
                        result = SaveChanges(entities);
                        if (result.Success)
                        {
                            recivedMsg.State = (int)EdiRecivedMsgState.Transferred;
                            filesParsed++;
                        }
                        else
                        {
                            entities.Detach(ediTransfer);
                            recivedMsg.State = (int)EdiRecivedMsgState.Error;
                            recivedMsg.ErrorMessage = string.Format("Could not add ediTransfer for EdiRecivedMsgId {0}, error msg: {1}", recivedMsg.EdiReceivedMsgId, result.ErrorMessage);
                            SharedProperties.LogError(recivedMsg.ErrorMessage);
                            result = SaveChanges(entities);
                            success = false;
                            continue;
                        }

                        result = this.OnMessageParsed(entities, ediTransfer, sysWholesellerEdi);
                    }
                }
                else
                {
                    success = false;
                }
            }
            catch (Exception ex)
            {
                success = false;
                string errMsg = string.Format("Exception when parsing messages from EdiRecivedMsgId {0}", recivedMsg.EdiReceivedMsgId);
                SharedProperties.LogError(ex, errMsg);
                recivedMsg.ErrorMessage = string.Join(errMsg, " Exception: ", ex.Message);
                recivedMsg.State = (int)EdiRecivedMsgState.Error;
                SaveChanges(entities);
            }
            finally
            {
                // Set final result parameters
                result.Success = success;
            }


            if (result.Success)
                result.IntegerValue = filesParsed;


            return result;
        }

        public IEnumerable<string> ParseMessage(int? sysWholesellerEdiId, string fileName, string content)
        {
            if (!sysWholesellerEdiId.HasValue)
                return null;

            bool success = false;
            IEnumerable<string> parsedMessages = null;
            var wholesellerEdi = this.EdiSysManager.GetSysWholesellerEDI(sysWholesellerEdiId.Value);
            IEdiSender sender = null;
            var wholesellerEnum = Enum.IsDefined(typeof(SysWholesellerEdiIdEnum), wholesellerEdi.SysWholesellerEdiId) ?
                (SysWholesellerEdiIdEnum)wholesellerEdi.SysWholesellerEdiId : SysWholesellerEdiIdEnum.Unknown;

            switch (wholesellerEnum)
            {
                case SysWholesellerEdiIdEnum.Selga: // Selga
                case SysWholesellerEdiIdEnum.Storel: // Storel
                    sender = new EdiSelga();
                    break;
                case SysWholesellerEdiIdEnum.NelfoGeneric: // All norwegian
                    var firstLine = content.Substring(0, content.IndexOf(Environment.NewLine));
                    if (firstLine.StartsWith("<?xml"))
                    {
                        //XML is Nelfo 4 or 5. But we assume that this is Nelfo5 since we don't support xml nelfo 4
                        sender = new EdiNelfo5();
                    }
                    else
                    {
                        var columns = firstLine.Split(';');
                        if (columns.Count() > 3 && columns[2] == "4.0")
                        {
                            // Nelfo 4.0 (semicolon seperated)
                            sender = new EdiNelfo40();
                        }
                        //else Nelfo 3.0?, which we don't have support for
                    }
                    break;
                case SysWholesellerEdiIdEnum.LvisNetGeneric: // All finish
                    sender = new EdiLvisNet();
                    break;
                case SysWholesellerEdiIdEnum.Comfort:
                    sender = new EdiComfort();
                    break;
                case SysWholesellerEdiIdEnum.Onninen:
                case SysWholesellerEdiIdEnum.Dahl:
                case SysWholesellerEdiIdEnum.VVSCentrum:
                    sender = new EdiFactGenerell();
                    break;
                default:
                    Console.Error.WriteLine("Error, wholeseller is not supported by EdiAdminManager. Name: {0}, EnumName: {1}", wholesellerEdi.SenderName, wholesellerEnum.ToString());
                    return null;
            }

            if (sender == null)
            {
                Console.Error.WriteLine("Error, sender is NULL, could not find the correct Sender for wholeseller {0}, SysWholesellerEdiIdEnum = {1}", wholesellerEdi.SenderName, wholesellerEnum.ToString());
            }

            if (sender is IEdiSenderOld)
            {
                var senderOld = (IEdiSenderOld)sender;
                senderOld.SetInputParams(new EdiSenderInputParams()
                {
                    dsStandardMall = SharedProperties.DsStandardMall,
                    SenderRow = wholesellerEdi.ToDataRow(),
                    InputFolderFileName = fileName,
                    drEdiSettings = SharedProperties.DrEdiSettingsOld,
                });
            }

            try
            {
                success = sender.ConvertMessage(content);
                if (success)
                    parsedMessages = sender.ToXmls();
            }
            catch (Exception ex)
            {
                string errMsg = string.Format("Error when trying to Convert input message for wholeseller {0}.",
                    wholesellerEdi.SenderName);
                SharedProperties.LogError(ex, errMsg);
                throw ex;
            }


            return parsedMessages;
        }

        public event OnMessageParsedDelegate OnMessageParsed;
    } // END CLASS
}
