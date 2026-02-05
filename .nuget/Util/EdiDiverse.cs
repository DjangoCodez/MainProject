using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Data;
using System.Drawing;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using SoftOne.EdiAdmin.Business.Util;
using SoftOne.EdiAdmin.Business.Core;
using SoftOne.Soe.Common.Util;


namespace SoftOne.EdiAdmin.Business.Util
{
    public class EdiDiverse
    {
        private EdiDatabasMetoder EdiDatabasMetoderKlass = new EdiDatabasMetoder();
        string ErrorMessage = "";

        public static StreamReader GetStreamReaderFromContentOrFile(string InputFolderFileName, string @WholesaleTempFolder, string fileContent, out bool doFileOperations, out string InputFileName)
        {
            Stream stream;
            if (!string.IsNullOrEmpty(fileContent))
            {
                InputFileName = InputFolderFileName;
                stream = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(fileContent));
                doFileOperations = false;
            }
            else
            {
                InputFileName = InputFolderFileName.Replace(@WholesaleTempFolder + "\\", "");
                stream = new FileStream(InputFileName, FileMode.Open);
                doFileOperations = true;
            }

            var sr = new StreamReader(stream, Encoding.UTF8);
            
            return sr;
        }

        public static StreamReader GetStreamReaderFromContentOrFile(string fileContent, string InputFileName)
        {
            Stream stream;
            if (string.IsNullOrEmpty(fileContent))
                stream = new FileStream(InputFileName, FileMode.Open);
            else
                stream = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(fileContent));

            return new StreamReader(stream, Encoding.UTF8);
        }

        //Tar bort Blanktecken till höger i text (CHAR)
        //Returvärde - text utan blanktecken till höger
        //Referenser - text med blanktecken till höger
        public string Blanka(string refText)
        {
            int i = 0;
            for (i = refText.Length; i != 0 && refText.Substring(i - 1, 1) == " "; i--)
            {
            }
            string txt = "";
            if (i != 0) { return txt = refText.Substring(0, i); }
            else
            { return txt; }
        }

        public void HämtaEdiLog(string Connection, DataSet dsMain)
        {
            if (dsMain.Tables["SysEdiLog"] != null) dsMain.Tables["SysEdiLog"].Rows.Clear();
            ErrorMessage = EdiDatabasMetoderKlass.HämtaTabellDataSet(Connection, "SoeSysV2", dsMain, "SysEdiLog", "SELECT * FROM SysEdiLog Where SysEdiLogId = 0");
            if (dsMain.Tables["EdiLogVisa"] != null) dsMain.Tables["EdiLogVisa"].Rows.Clear();
            ErrorMessage = EdiDatabasMetoderKlass.HämtaTabellDataSet(Connection, "SoeSysV2", dsMain, "EdiLogVisa", "SELECT * FROM SysEdiLog Where SysEdiLogId = 0");
        }

        public void LagraEdiLog(string Connection, DataSet dsMain, string Subject, string Message)
        {
            if (dsMain.Tables["SysEdiLog"] != null) dsMain.Tables["SysEdiLog"].Rows.Clear();
            DataRow drEdiLog = dsMain.Tables["SysEdiLog"].NewRow();
            dsMain.Tables["SysEdiLog"].Rows.Add(drEdiLog);
            drEdiLog["LogDateTime"] = DateTime.Now;
            drEdiLog["Subject"] = Subject;
            drEdiLog["Message"] = Message;
            EdiDatabasMetoderKlass.UppdateraTabellDataSet(Connection, "SoeSysV2", dsMain, "SysEdiLog");

            DataRow drEdiLogVisa = dsMain.Tables["EdiLogVisa"].NewRow();
            dsMain.Tables["EdiLogVisa"].Rows.Add(drEdiLogVisa);
            drEdiLogVisa["LogDateTime"] = DateTime.Now;
            drEdiLogVisa["Subject"] = Subject;
            drEdiLogVisa["Message"] = Message;
        }

        public void HämtaEdiTransfer(string Connection, DataSet dsMain)
        {
            if (dsMain.Tables["EdiTransfer"] != null) dsMain.Tables["EdiTransfer"].Rows.Clear();
            ErrorMessage = EdiDatabasMetoderKlass.HämtaTabellDataSet(Connection, "SoeCompV2", dsMain, "EdiTransfer", "SELECT * FROM EdiTransfer Where EdiTransferId = 0");
        }

        public void HämtaEdiReceivedMsg(string Connection, DataSet dsMain)
        {
            if (dsMain.Tables["EdiReceivedMsg"] != null) dsMain.Tables["EdiReceivedMsg"].Rows.Clear();
            ErrorMessage = EdiDatabasMetoderKlass.HämtaTabellDataSet(Connection, "SoeCompV2", dsMain, "EdiReceivedMsg", "SELECT * FROM EdiReceivedMsg Where EdiReceivedMsgId = 0");
        }

        public DateTime HämtaEdiReceived(string Connection, DataSet dsMain)
        {
            if (dsMain.Tables["SysEdiReceived"] != null) dsMain.Tables["SysEdiReceived"].Rows.Clear();
            ErrorMessage = EdiDatabasMetoderKlass.HämtaTabellDataSet(Connection, "SoeSysV2", dsMain, "SysEdiReceived", "SELECT * FROM SysEdiReceived Where SysEdiReceivedId = 0");
            DataRow drMain = dsMain.Tables["SysEdiReceived"].NewRow();
            dsMain.Tables["SysEdiReceived"].Rows.Add(drMain);
            drMain["ReceivedDate"] = DateTime.Now.ToString();
            drMain["ReceivedFiles"] = 0;
            drMain["ReceivedIncorrect"] = 0;
            drMain["ReceivedMsgCreated"] = 0;
            drMain["ReceivedFilesImage"] = 0;
            return Convert.ToDateTime(drMain["ReceivedDate"]);
        }

        public void HämtaCompany(string Connection, DataSet dsMain)
        {
            if (dsMain.Tables["Company"] != null) dsMain.Tables["Company"].Rows.Clear();
            ErrorMessage = EdiDatabasMetoderKlass.HämtaTabellDataSet(Connection, "SoeCompV2", dsMain, "Company", "SELECT * FROM Company");
        }

        public void HämtaEdiSender(string Connection, DataSet dsMain)
        {
            if (dsMain.Tables["EdiSender"] != null) dsMain.Tables["EdiSender"].Rows.Clear();
            ErrorMessage = EdiDatabasMetoderKlass.HämtaTabellDataSet(Connection, "SoeSysV2", dsMain, "SysWholesellerEdi", "SELECT * FROM SysWholesellerEdi");
        }

        public void HämtaEdiType(string Connection, DataSet dsMain)
        {
            if (dsMain.Tables["EdiType"] != null) dsMain.Tables["EdiType"].Rows.Clear();
            ErrorMessage = EdiDatabasMetoderKlass.HämtaTabellDataSet(Connection, "SoeSysV2", dsMain, "SysEdiType", "SELECT * FROM SysEdiType");
        }

        public void HämtaEdiMsg(string Connection, DataSet dsMain)
        {
            if (dsMain.Tables["EdiMsg"] != null) dsMain.Tables["EdiMsg"].Rows.Clear();
            ErrorMessage = EdiDatabasMetoderKlass.HämtaTabellDataSet(Connection, "SoeSysV2", dsMain, "SysEdiMsg", "SELECT * FROM SysEdiMsg");
        }

        public void HämtaAllConnections(string Connection, DataSet dsMain)
        {
            if (dsMain.Tables["AllConnections"] != null) dsMain.Tables["AllConnections"].Rows.Clear();

            string fraga =
@"SELECT SoeCompV2.dbo.Company.ActorCompanyId, 
SoeCompV2.dbo.Company.OrgNr, 
SoeCompV2.dbo.Company.Name, 
SoeCompV2.dbo.Company.State, 
SoeCompV2.dbo.EdiConnection.SysEdiMsgId, 
SoeCompV2.dbo.EdiConnection.BuyerNr, 
SoeCompV2.dbo.EdiConnection.Debiting, 
SoeCompV2.dbo.EdiConnection.EdiFolder, 
SoeCompV2.dbo.EdiConnection.EdiFolderImage, 
SoeCompV2.dbo.EdiConnection.XslRow, 
SoeCompV2.dbo.EdiConnection.Email, 
SoeSysV2.dbo.SysEdiMsg.SysWholesellerEdiId, 
SoeSysV2.dbo.SysEdiMsg.SysEdiTypeId, 
SoeSysV2.dbo.SysEdiMsg.SenderSenderNr, 
SoeSysV2.dbo.SysEdiMsg.SenderType, 
SoeSysV2.dbo.SysWholesellerEdi.SysWholesellerEdiId, 
SoeSysV2.dbo.SysWholesellerEdi.SenderName,  
SoeSysV2.dbo.SysEdiType.SysEdiTypeId, 
SoeSysV2.dbo.SysEdiType.TypeName 
FROM SoeSysV2.dbo.SysEdiMsg 
INNER JOIN SoeCompV2.dbo.EdiConnection ON SoeSysV2.dbo.SysEdiMsg.SysEdiMsgId = SoeCompV2.dbo.EdiConnection.SysEdiMsgId
INNER JOIN SoeCompV2.dbo.Company ON SoeCompV2.dbo.EdiConnection.ActorCompanyId = SoeCompV2.dbo.Company.ActorCompanyId  
INNER JOIN SoeSysV2.dbo.SysWholesellerEdi ON SoeSysV2.dbo.SysEdiMsg.SysWholesellerEdiId = SoeSysV2.dbo.SysWholesellerEdi.SysWholesellerEdiId 
INNER JOIN SoeSysV2.dbo.SysEdiType ON SoeSysV2.dbo.SysEdiMsg.SysEdiTypeId = SoeSysV2.dbo.SysEdiType.SysEdiTypeId
Order By Company.OrgNr, EdiConnection.EdiFolder";
    
            ErrorMessage = EdiDatabasMetoderKlass.HämtaTabellDataSet(Connection, "SoeCompV2", dsMain, "AllConnections", fraga);
        }

        public DataRow SkapaLoggInfo(string Connection, DataSet dsMain)
        {
            dsMain.Tables.Add("LoggInfo");
            dsMain.Tables["LoggInfo"].Columns.Add("ProgramStartTid");
            dsMain.Tables["LoggInfo"].Columns.Add("SenasteStartTid");
            dsMain.Tables["LoggInfo"].Columns.Add("AntalHämtningar", typeof(int));
            dsMain.Tables["LoggInfo"].Columns.Add("AntalGrossistLästa", typeof(int));
            dsMain.Tables["LoggInfo"].Columns.Add("AntalGrossistSkrivna", typeof(int));
            dsMain.Tables["LoggInfo"].Columns.Add("AntalGrossistAvvisade", typeof(int));
            dsMain.Tables["LoggInfo"].Columns.Add("AntalMeddelandeLästa", typeof(int));
            dsMain.Tables["LoggInfo"].Columns.Add("AntalMeddelandeSkrivna", typeof(int));
            dsMain.Tables["LoggInfo"].Columns.Add("AntalMeddelandeAvvisade", typeof(int));
            dsMain.Tables["LoggInfo"].Columns.Add("AntalBildfilLästa", typeof(int));
            dsMain.Tables["LoggInfo"].Columns.Add("AntalBildfilSkrivna", typeof(int));
            dsMain.Tables["LoggInfo"].Columns.Add("AntalBildfilAvvisade", typeof(int));

            DataRow drLoggInfo = dsMain.Tables["LoggInfo"].NewRow();
            dsMain.Tables["LoggInfo"].Rows.Add(drLoggInfo);
            drLoggInfo["ProgramStartTid"] = DateTime.Now.ToString();
            drLoggInfo["AntalHämtningar"] = 0;
            drLoggInfo["AntalGrossistLästa"] = 0;
            drLoggInfo["AntalGrossistSkrivna"] = 0;
            drLoggInfo["AntalGrossistAvvisade"] = 0;
            drLoggInfo["AntalMeddelandeLästa"] = 0;
            drLoggInfo["AntalMeddelandeSkrivna"] = 0;
            drLoggInfo["AntalMeddelandeAvvisade"] = 0;
            drLoggInfo["AntalBildfilLästa"] = 0;
            drLoggInfo["AntalBildfilSkrivna"] = 0;
            drLoggInfo["AntalBildfilAvvisade"] = 0;

            if (dsMain.Tables["EdiTransferTotalt"] != null) dsMain.Tables["EdiTransferTotalt"].Rows.Clear();
            ErrorMessage = EdiDatabasMetoderKlass.HämtaTabellDataSet(Connection, "SoeCompV2", dsMain, "EdiTransferTotalt", "SELECT * FROM EdiTransfer Where EdiTransferId = 0");

            return drLoggInfo;
        }

        public void SkapaMeddelande(DataSet dsMeddelande)
        {
            dsMeddelande.Tables.Add("Message");
            dsMeddelande.Tables["Message"].Columns.Add("CustomerFtpUser");
            dsMeddelande.Tables["Message"].Columns.Add("CustomerName");
            dsMeddelande.Tables["Message"].Columns.Add("FtpEdiFolder");
        }

        public string KontrollGrossistMapp(Dictionary<string, string> drEdiSettings, DataRow SenderRow)
        {
            string WholesaleTempFolder = drEdiSettings["WholesaleTempFolder"].ToString() + "\\" + SenderRow["SenderName"].ToString();
            if (System.IO.Directory.Exists(WholesaleTempFolder) == false)
            {
                System.IO.Directory.CreateDirectory(WholesaleTempFolder);
                if (System.IO.Directory.Exists(WholesaleTempFolder) == false)
                {
                    WindowsLog.Instance.WriteEntry("'Grossist - Temporär mapp' " + WholesaleTempFolder + " kan inte skapas");
                    return "";
                }
            }
            return WholesaleTempFolder;
        }

        public bool KontrollMappar(Dictionary<string, string> drEdiSettings)
        {
            bool resultat = false;

            //Kontroll att "Grossist - Temporär mapp" inte är Null under Inställningar
            if (drEdiSettings["WholesaleTempFolder"].ToString() == "")
                Console.WriteLine("'Grossist - Temporär mapp' finns inte angiven");
            else
                //Kontroll att "Grossist - Temporär mapp" angiven under Inställningar finns - om inte så läggs den upp
                if (System.IO.Directory.Exists(@drEdiSettings["WholesaleTempFolder"].ToString()) == false)
                {
                    System.IO.Directory.CreateDirectory(@drEdiSettings["WholesaleTempFolder"].ToString());
                    if (System.IO.Directory.Exists(@drEdiSettings["WholesaleTempFolder"].ToString()) == false)
                        resultat = WindowsLog.Instance.WriteEntry("'Grossist - Temporär mapp' " + drEdiSettings["WholesaleTempFolder"].ToString() + " kan inte skapas");
                }

            //Kontroll att "Grossist - Fel mapp" inte är Null under Inställningar
            if (drEdiSettings["WholesaleErrorFolder"].ToString() == "")
                resultat = WindowsLog.Instance.WriteEntry("'Grossist - Fel mapp' finns inte angiven");
            else
                //Kontroll att "Meddelande - Fel mapp" angiven under Inställningar finns - om inte så läggs den upp
                if (System.IO.Directory.Exists(@drEdiSettings["WholesaleErrorFolder"].ToString()) == false)
                {
                    System.IO.Directory.CreateDirectory(@drEdiSettings["WholesaleErrorFolder"].ToString());
                    if (System.IO.Directory.Exists(@drEdiSettings["WholesaleErrorFolder"].ToString()) == false)
                        resultat = WindowsLog.Instance.WriteEntry("'Grossist - Fel mapp' " + drEdiSettings["WholesaleErrorFolder"].ToString() + " kan inte skapas");
                }

            //Kontroll att "Grossist - Arkiv mapp" inte är Null under Inställningar
            if (drEdiSettings["WholesaleSaveFolder"].ToString() == "")
                resultat = WindowsLog.Instance.WriteEntry("'Grossist - Arkiv mapp' finns inte angiven");
            else
                //Kontroll att "Meddelande - Arkiv mapp" angiven under Inställningar finns - om inte så läggs den upp
                if (System.IO.Directory.Exists(@drEdiSettings["WholesaleSaveFolder"].ToString()) == false)
                {
                    System.IO.Directory.CreateDirectory(@drEdiSettings["WholesaleSaveFolder"].ToString());
                    if (System.IO.Directory.Exists(@drEdiSettings["WholesaleSaveFolder"].ToString()) == false)
                        resultat = WindowsLog.Instance.WriteEntry("'Grossist - Arkiv mapp' " + drEdiSettings["WholesaleSaveFolder"].ToString() + "' kan inte skapas");
                }


            //Kontroll att "Meddelande - Temporär mapp" inte är Null under Inställningar
            if (drEdiSettings["MsgTempFolder"].ToString() == "")
                resultat = WindowsLog.Instance.WriteEntry("'Meddelande - Temporär mapp' finns inte angiven");
            else
                //Kontroll att "Meddelande - Temporär mapp" angiven under Inställningar finns - om inte så läggs den upp
                if (System.IO.Directory.Exists(@drEdiSettings["MsgTempFolder"].ToString()) == false)
                {
                    System.IO.Directory.CreateDirectory(@drEdiSettings["MsgTempFolder"].ToString());
                    if (System.IO.Directory.Exists(@drEdiSettings["MsgTempFolder"].ToString()) == false)
                        resultat = WindowsLog.Instance.WriteEntry("'Meddelande - Temporär mapp' " + drEdiSettings["MsgTempFolder"].ToString() + " kan inte skapas");
                }

            //Kontroll att "Meddelande - Fel mapp" inte är Null under Inställningar
            if (drEdiSettings["MsgErrorFolder"].ToString() == "")
                resultat = WindowsLog.Instance.WriteEntry("'Meddelande - Fel mapp' finns inte angiven");
            else
                //Kontroll att "Meddelande - Fel mapp" angiven under Inställningar finns - om inte så läggs den upp
                if (System.IO.Directory.Exists(@drEdiSettings["MsgErrorFolder"].ToString()) == false)
                {
                    System.IO.Directory.CreateDirectory(@drEdiSettings["MsgErrorFolder"].ToString());
                    if (System.IO.Directory.Exists(@drEdiSettings["MsgErrorFolder"].ToString()) == false)
                        resultat = WindowsLog.Instance.WriteEntry("'Meddelande - Fel mapp' " + drEdiSettings["MsgErrorFolder"].ToString() + " kan inte skapas");
                }

            //Kontroll att "Meddelande - Arkiv mapp" inte är Null under Inställningar
            if (drEdiSettings["MsgSaveFolder"].ToString() == "")
                resultat = WindowsLog.Instance.WriteEntry("'Meddelande - Arkiv mapp' finns inte angiven");
            else
                //Kontroll att "Meddelande - Arkiv mapp" angiven under Inställningar finns - om inte så läggs den upp
                if (System.IO.Directory.Exists(@drEdiSettings["MsgSaveFolder"].ToString()) == false)
                {
                    System.IO.Directory.CreateDirectory(@drEdiSettings["MsgSaveFolder"].ToString());
                    if (System.IO.Directory.Exists(@drEdiSettings["MsgSaveFolder"].ToString()) == false)
                        resultat = WindowsLog.Instance.WriteEntry("'Meddelande - Arkiv mapp' " + drEdiSettings["MsgSaveFolder"].ToString() + "' kan inte skapas");
                }


            //Kontroll att "Bildfil - Temporär mapp" inte är Null under Inställningar
            if (drEdiSettings["ImageTempFolder"].ToString() == "")
                resultat = WindowsLog.Instance.WriteEntry("'Bildfil - Temporär mapp' finns inte angiven");
            else
                //Kontroll att "Bildfil - Temporär mapp" angiven under Inställningar finns - om inte så läggs den upp
                if (System.IO.Directory.Exists(@drEdiSettings["ImageTempFolder"].ToString()) == false)
                {
                    System.IO.Directory.CreateDirectory(@drEdiSettings["ImageTempFolder"].ToString());
                    if (System.IO.Directory.Exists(@drEdiSettings["ImageTempFolder"].ToString()) == false)
                        resultat = WindowsLog.Instance.WriteEntry("'Bildfil - Temporär mapp' " + drEdiSettings["ImageTempFolder"].ToString() + " kan inte skapas");
                }

            //Kontroll att "Bildfil - Fel mapp" inte är Null under Inställningar
            if (drEdiSettings["ImageErrorFolder"].ToString() == "")
                resultat = WindowsLog.Instance.WriteEntry("'Bildfil - Fel mapp' finns inte angiven");
            else
                //Kontroll att "Bildfil - Fel mapp" angiven under Inställningar finns - om inte så läggs den upp
                if (System.IO.Directory.Exists(@drEdiSettings["ImageErrorFolder"].ToString()) == false)
                {
                    System.IO.Directory.CreateDirectory(@drEdiSettings["ImageErrorFolder"].ToString());
                    if (System.IO.Directory.Exists(@drEdiSettings["ImageErrorFolder"].ToString()) == false)
                        resultat = WindowsLog.Instance.WriteEntry("'Bildfil - Fel mapp' " + drEdiSettings["ImageErrorFolder"].ToString() + " kan inte skapas");
                }

            //Kontroll att "Bildfil - Arkiv mapp" inte är Null under Inställningar
            if (drEdiSettings["ImageSaveFolder"].ToString() == "")
                resultat = WindowsLog.Instance.WriteEntry("'Bildfil - Arkiv mapp' finns inte angiven");
            else
                //Kontroll att "Bildfil - Arkiv mapp" angiven under Inställningar finns - om inte så läggs den upp
                if (System.IO.Directory.Exists(@drEdiSettings["ImageSaveFolder"].ToString()) == false)
                {
                    System.IO.Directory.CreateDirectory(@drEdiSettings["ImageSaveFolder"].ToString());
                    if (System.IO.Directory.Exists(@drEdiSettings["ImageSaveFolder"].ToString()) == false)
                        resultat = WindowsLog.Instance.WriteEntry("'Bildfil - Arkiv mapp' " + drEdiSettings["ImageSaveFolder"].ToString() + "' kan inte skapas");
                }


            //Kontroll att "Mapp för inkommande meddelande" inte är Null under Inställningar
            if (drEdiSettings["FtpMsgInputFolder"].ToString() == "")
                resultat = WindowsLog.Instance.WriteEntry("Indata mappen för meddelande finns inte angiven");
            if (Convert.ToBoolean(drEdiSettings["FtpMsgInputFolder"].ToString().Contains("ftp://")) != true)
                resultat = WindowsLog.Instance.WriteEntry("Indata mappen för meddelande felaktigt angiven");

            //Kontroll att "Mapp för inkommande bildfiler" inte är Null under Inställningar
            if (drEdiSettings["FtpImageInputFolder"].ToString() == "")
                resultat = WindowsLog.Instance.WriteEntry("Indata mappen för bildfiler finns inte angiven");
            if (Convert.ToBoolean(drEdiSettings["FtpImageInputFolder"].ToString().Contains("ftp://")) != true)
                resultat = WindowsLog.Instance.WriteEntry("Indata mappen för bildfiler felaktigt angiven");


            if (resultat)
                return true;
            else
                return false;

        }

        //Lägg till rad för Xsl-fil
        public void LäggTillXsl(string FromFile, string XslRow)
        {
            string line = "";
            string xml = "";
            bool första = true;
            StreamReader sr = new StreamReader(FromFile, Encoding.UTF8);
            while ((line = sr.ReadLine()) != null)
            {
                xml = xml + line + "\r\n";
                if (första == true)
                {
                    xml = xml + XslRow + "\r\n";
                    första = false;
                }
            }
            sr.Close();
            StreamWriter sw = new StreamWriter(FromFile, false, Encoding.UTF8);
            sw.Write(xml);
            sw.Flush();
            sw.Close();
        }

        //Tar bort filer från Save-mappar som är äldre än 10 dagar
        public void BorttagMeddelande(DataSet dsMain)
        {
            var cm = new EdiCompManager();
            Console.WriteLine("Removing 10 days old messages");
            DateTime TomDatum = DateTime.Now.AddDays(-10);

            string[] WholesaleSaveFolder = System.IO.Directory.GetFiles(cm.GetApplicationSetting(ApplicationSettingType.WholesaleSaveFolder)); // System.IO.Directory.GetFiles(@dsMain.Tables["EdiSettings"].Rows[0]["WholesaleSaveFolder"].ToString());
            string[] MsgSaveFolder = System.IO.Directory.GetFiles(cm.GetApplicationSetting(ApplicationSettingType.MsgSaveFolder));
            string[] ImageSaveFolder = System.IO.Directory.GetFiles(cm.GetApplicationSetting(ApplicationSettingType.ImageSaveFolder));

            for (int i = 0; i < WholesaleSaveFolder.Length; i++)
            {
                DateTime FilDatum = File.GetCreationTime(WholesaleSaveFolder[i]);
                if (FilDatum < TomDatum)
                    File.Delete(WholesaleSaveFolder[i]);
            }

            for (int i = 0; i < MsgSaveFolder.Length; i++)
            {
                DateTime FilDatum = File.GetCreationTime(MsgSaveFolder[i]);
                if (FilDatum < TomDatum)
                    File.Delete(MsgSaveFolder[i]);
            }

            for (int i = 0; i < ImageSaveFolder.Length; i++)
            {
                DateTime FilDatum = File.GetCreationTime(ImageSaveFolder[i]);
                if (FilDatum < TomDatum)
                    File.Delete(ImageSaveFolder[i]);
            }

        }

        public void SkickaMailInternt(string MailAddress, string MailSubject, string MailMessage, params string[] mailAttachments)
        {
            // Revert to edi.softone.se
            if (string.IsNullOrEmpty(MailAddress))
            {
                MailMessage = "This message didn't have any recipiment, please check EDIAdmin settings!" + Environment.NewLine + MailMessage;
                MailAddress = "edi@softone.se";
            }

            string MailFrom = Properties.Settings.Default.EmailInternalFrom ?? "edi@softone.se";
            string[] MailTo = MailAddress.Split(';');
            string[] MailAttachments = mailAttachments ?? new string[0];

            string Meddelande = SendMail(MailFrom, MailTo, MailSubject, MailMessage, MailAttachments);
        }

        public void SkickaMailExternt(string MailAddressFrom, string MailAddressTo, string MailSubject, string MailMessage)
        {
            if (MailAddressFrom == "" | MailAddressTo == "") return;

            string MailFrom = "edi@softone.se";
            string[] MailTo = MailAddressTo.Split(';');
            string[] MailAttachments = new string[0];

            string Meddelande = SendMail(MailFrom, MailTo, MailSubject, MailMessage, MailAttachments);
        }

        //Returvärde - Felmeddelande
        //Referenser - Connection-sträng, Från mail-adress, Till mail-adresser, Ämne, Bifogade filer
        public string SendMail(string From, string[] To, string Subject, string Message, string[] Attachments)
        {
            if (Properties.Settings.Default.UseInternalMailServer)
            {
                return SendMailLocalSMTP(From, To, Subject, Message, Attachments);
            }
            else
            {
                return SendMailSMTP(To, Subject, Message, Attachments);
            }
        }

        public string SendMailLocalSMTP(string From, string[] To, string Subject, string Message, string[] Attachments)
        {
            string Server = "SMTP";

            string Meddelande = "";

            for (int i = 0; i < To.Length; i++)
            {
                MailMessage message = new MailMessage(From, To[i], Subject, Message);

                //Kopia
                //message.CC.Add(To[i]);

                SmtpClient client = new SmtpClient(Server);

                for (int j = 0; j < Attachments.Length; j++)
                {
                    if (!File.Exists(Attachments[j])) continue;
                    // Create  the file attachment for this e-mail message.
                    Attachment data = new Attachment(Attachments[j], MediaTypeNames.Application.Octet);
                    // Add time stamp information for the file.
                    ContentDisposition disposition = data.ContentDisposition;
                    disposition.CreationDate = System.IO.File.GetCreationTime(Attachments[j]);
                    disposition.ModificationDate = System.IO.File.GetLastWriteTime(Attachments[j]);
                    disposition.ReadDate = System.IO.File.GetLastAccessTime(Attachments[j]);
                    // Add the file attachment to this e-mail message.
                    message.Attachments.Add(data);
                    // Add credentials if the SMTP server requires them.
                    client.Credentials = CredentialCache.DefaultNetworkCredentials;
                }
                try
                {
                    //Skicka mail
                    client.Send(message);
                }
                catch (FormatException ex)
                {
                    Meddelande = Meddelande + ex.Message;
                }
                catch (SmtpException ex)
                {
                    Meddelande = Meddelande + ex.Message;
                }

                if (!string.IsNullOrEmpty(Meddelande))
                    Console.Error.WriteLine("Could not send internal SMTP email: " + string.Concat("From: ", From, ". To: ", string.Join(",", To), Environment.NewLine, Subject, Environment.NewLine, Message, Environment.NewLine, " Error: ", Meddelande));
            }

            return Meddelande;
        }

        public string SendMailSMTP(string[] To, string Subject, string Message, string[] Attachments, params string[] CC)
        {
            var settings = Properties.Settings.Default;
            string Server = string.IsNullOrEmpty(settings.EmailSMTP) ? "SMTP" : settings.EmailSMTP;
            string Meddelande = "";

            MailMessage message = new MailMessage()
            {
                Subject = Subject,
                Body = Message,
                From = new MailAddress(settings.EmailFrom, settings.EmailFromName),
            };

            string to = string.Join(",", To);
            message.To.Add(to);

            if (CC != null && CC.Length > 0)
            {
                string cc = string.Join(",", CC);
                message.CC.Add(cc);
            }

            SmtpClient client = new SmtpClient(Server);
            if (settings.SMTPPort > 0)
                client.Port = settings.SMTPPort;

            if (settings.SMTPLogon != null && settings.EmailPassword != null)
                client.Credentials = new NetworkCredential(settings.SMTPLogon, settings.EmailPassword);

            client.EnableSsl = settings.SMTPSSL;

            if (Attachments != null)
            {
                for (int j = 0; j < Attachments.Length; j++)
                {
                    if (!File.Exists(Attachments[j])) continue;
                    // Create  the file attachment for this e-mail message.
                    Attachment data = new Attachment(Attachments[j], MediaTypeNames.Application.Octet);
                    // Add time stamp information for the file.
                    ContentDisposition disposition = data.ContentDisposition;
                    disposition.CreationDate = System.IO.File.GetCreationTime(Attachments[j]);
                    disposition.ModificationDate = System.IO.File.GetLastWriteTime(Attachments[j]);
                    disposition.ReadDate = System.IO.File.GetLastAccessTime(Attachments[j]);
                    // Add the file attachment to this e-mail message.
                    message.Attachments.Add(data);
                    // Add credentials if the SMTP server requires them.
                    client.Credentials = CredentialCache.DefaultNetworkCredentials;
                }
            }

            try
            {
                //Skicka mail
                client.Send(message);
            }
            catch (FormatException ex)
            {
                Meddelande = Meddelande + ex.Message;
            }
            catch (SmtpException ex)
            {
                Meddelande = Meddelande + ex.Message;
            }

            if (!string.IsNullOrEmpty(Meddelande))
                Console.Error.WriteLine("Could not send external SMTP email: " + string.Join(",", settings.EmailFrom, settings.SMTPLogon, settings.SMTPPort, message.To) + " Error: " + Meddelande);

            return Meddelande;

        }
    }
}
