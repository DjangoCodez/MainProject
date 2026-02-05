using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using SoftOne.EdiAdmin.Business.Senders;
using SoftOne.EdiAdmin.Business.Util;
using SoftOne.Soe.Common.Util;
using SoftOne.EdiAdmin.Business.Core;
using System.Collections.Generic;
using SoftOne.Soe.Common.DTO;
using SoftOne.EdiAdmin.Business.Interfaces;

namespace SoftOne.EdiAdmin.Business
{
    public class EdiManager
    {
        #region Enums

        private enum MoveToCustomersResult
        {
            Success = 0,
            UnkownError = 1,
            SetupError = 2,
            Retry = 3,
            WrongEnviroment = 4,
        }

        #endregion

        #region Variables

        private static DateTime removedMsg;
        private string dbName;
        private bool onlyMoveFilesMode = false;
        private int sysScheduledJobId;
        private int batchNr;
        private EdiCompManager EdiCompManagerClass = new EdiCompManager();
        private EdiSysManager EdiSysManagerClass = new EdiSysManager();
        private EdiDiverse EdiDiverseKlass = new EdiDiverse();
        private EdiDatabasMetoder EdiDatabasMetoderKlass = new EdiDatabasMetoder();
        private EdiFtpSupport EdiFtpSupportKlass = new EdiFtpSupport();
        private EdiFactGenerell EdiFactGenerellKlass = new EdiFactGenerell();
        private EdiElektroskandia EdiElektroskandiaKlass = new EdiElektroskandia();
        private EdiLundagrossisten EdiLundagrossistenKlass = new EdiLundagrossisten();
        private EdiMoel EdiMoelKlass = new EdiMoel();
        private EdiSelga EdiSelgaKlass = new EdiSelga();
        private EdiElgrossn EdiElgrossnKlass = new EdiElgrossn();
        private EdiNelfo5 EdiNelfo5Klass = new EdiNelfo5();
        private EdiNelfo40 EdiNelfo40Klass = new EdiNelfo40();
        private EdiLvisNet EdiLvisNetKlass = new EdiLvisNet();

        private Dictionary<string, string> drEdiSettings;
        private DataRow drLoggInfo;

        private DataSet dsMain = new DataSet();
        private DataRow drMain;

        private DataSet dsMeddelande = new DataSet("Messages");
        private DataSet dsStandardMall = new DataSet();

        private string Connection;

        private DateTime DatumReceived;

        private string ErrorMessage;
        private string MailSubject = "";
        private string MailMessage = "";
        private string MailAttachment;

        private static int AntalKontrollBorttagMeddelande = 0;
        private static int AntalKörningar;
        #endregion

        public event EventHandler<EdiAdminStreamWriterEventArgs> OnOutputRecived;

        #region Constructor

        public EdiManager(int sysScheduledJobId = 0, int batchNr = 0)
        {
            this.sysScheduledJobId = sysScheduledJobId;
            this.batchNr = batchNr;
        }

        public bool Setup(bool redirectOutPut, bool? moveFilesMode = null)
        {
            if (redirectOutPut)
            {
                var errorWriter = new EdiAdminStreamWriter(EventLogEntryType.Error);
                var outputWriter = new EdiAdminStreamWriter(EventLogEntryType.Information);
                errorWriter.Output += this.OnOutputRecived;
                outputWriter.Output += this.OnOutputRecived;
                Console.SetError(errorWriter);
                Console.SetOut(outputWriter);
            }

            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

            // Get the connection string from entity framework and rebuild it with only the parts needed for ediAdmin
            var dbConnection = ((System.Data.Entity.Core.EntityClient.EntityConnection)this.EdiCompManagerClass.CompEntities.Connection).StoreConnection;

            var entityBuilder = new System.Data.SqlClient.SqlConnectionStringBuilder(dbConnection.ConnectionString);
            var newConnection = new System.Data.SqlClient.SqlConnectionStringBuilder();
            newConnection.Add("Data Source", entityBuilder.DataSource);
            newConnection.Add("User Id", entityBuilder.UserID); 
            newConnection.Add("password", entityBuilder.Password);
            newConnection.Add("initial catalog", entityBuilder.InitialCatalog);
            this.Connection = newConnection.ConnectionString + ";";
            this.dbName = entityBuilder.InitialCatalog;

            SqlConnection con = new SqlConnection(Connection);
            DataSet ds = new DataSet();
            SqlDataAdapter da = new SqlDataAdapter("SELECT * FROM Company", con);
            da.SelectCommand.CommandText = "SELECT * FROM Company";
            try
            {
                da.Fill(ds, "Company");
            }
            catch
            {
                string msg = "Felaktig databaskoppling till = '" + Connection + "'";
                Console.Error.WriteLine(msg, EventLogEntryType.Error);
                WindowsLog.Instance.WriteEntry(msg, System.Diagnostics.EventLogEntryType.FailureAudit);
                return false;
            }
            //Hämta Inställningar
            drEdiSettings = EdiCompManagerClass.GetApplicationSettingsDict();
            this.onlyMoveFilesMode = moveFilesMode ?? !string.IsNullOrEmpty(drEdiSettings[ApplicationSettingType.OnlyMoveFilesMode.ToString()]) ? Convert.ToBoolean(Convert.ToInt32(drEdiSettings[ApplicationSettingType.OnlyMoveFilesMode.ToString()])) : this.onlyMoveFilesMode;

            if (File.Exists("StandardMall.txt"))
            {
                StreamReader sr = new StreamReader("StandardMall.txt");
                string StandardMall = sr.ReadLine();
                dsStandardMall.ReadXml(StandardMall);
            }
            else if (File.Exists(drEdiSettings["StandardTemplatesFolder"] + "\\StandardMall.xml"))
            {
                dsStandardMall.ReadXml(drEdiSettings["StandardTemplatesFolder"] + "\\StandardMall.xml");
            }

            //Skapa tomma tabeller
            EdiDiverseKlass.HämtaEdiLog(Connection, dsMain);
            EdiDiverseKlass.HämtaEdiTransfer(Connection, dsMain);
            EdiDiverseKlass.HämtaEdiReceived(Connection, dsMain);
            //Hämta Kunder, Grossister, Meddelandetyper, Koppling Grossist-Meddelandetyp
            EdiDiverseKlass.HämtaCompany(Connection, dsMain);
            EdiDiverseKlass.HämtaEdiSender(Connection, dsMain);
            EdiDiverseKlass.HämtaEdiMsg(Connection, dsMain);
            EdiDiverseKlass.HämtaEdiType(Connection, dsMain);
            //Skapa tabeller för visning av Loggningsinformation
            drLoggInfo = EdiDiverseKlass.SkapaLoggInfo(Connection, dsMain);
            //Skapa tabell för fil-redovisning av meddelande skrivna till Ftp för kund/meddelandetyp
            EdiDiverseKlass.SkapaMeddelande(dsMeddelande);

            try
            {
                Console.WriteLine(this.GetStatusMessage());
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Error, could not get status message for service. Ex: " + ex.Message);
            }

            return true;
        }

        #endregion

        #region Public Methods
        public void HandleMessages()
        {
            drEdiSettings = EdiCompManagerClass.GetApplicationSettingsDict();
            EdiDiverseKlass.HämtaEdiSender(Connection, dsMain);
            EdiDiverseKlass.HämtaEdiMsg(Connection, dsMain);
            EdiDiverseKlass.HämtaCompany(Connection, dsMain);
            dsMeddelande.Tables["Message"].Rows.Clear();

            //Skapa tabell med samtliga upplagda EDI-kopplingar för samtliga kunder
            EdiDiverseKlass.HämtaAllConnections(Connection, dsMain);
            if (dsMain.Tables["AllConnections"].Columns["AntalFilerKund"] == null)
                dsMain.Tables["AllConnections"].Columns.Add("AntalFilerKund", typeof(int));
            foreach (DataRow row in dsMain.Tables["AllConnections"].Rows)
                row["AntalFilerKund"] = 0;

            drLoggInfo["SenasteStartTid"] = DateTime.Now.ToString();
            int nrOfBatches = Convert.ToInt32(drLoggInfo["AntalHämtningar"]) + 1;
            drLoggInfo["AntalHämtningar"] = nrOfBatches;
            Console.Out.WriteLine(string.Format("Starting fetching from FTP nr: {0}", AntalKörningar));

            //Tar bort gamla meddelande (dagens datum - 10) från Temp-mappar efter 500 hämtningar eller då det har gått ett dygn sen senaste gång (statisk variabel)
            AntalKontrollBorttagMeddelande++;
            AntalKörningar++;
            if ((AntalKontrollBorttagMeddelande == 500) ||
                (removedMsg == default(DateTime) || removedMsg < DateTime.Now.AddDays(-1)))
            {
                EdiDiverseKlass.BorttagMeddelande(dsMain);
                AntalKontrollBorttagMeddelande = 0;
                removedMsg = DateTime.Now;
            }

            EdiDiverseKlass.HämtaEdiTransfer(Connection, dsMain);
            DatumReceived = EdiDiverseKlass.HämtaEdiReceived(Connection, dsMain);
            EdiDiverseKlass.HämtaEdiReceivedMsg(Connection, dsMain);

            //Datum = Datum.Replace(":", "-");
            //Datum = Datum.Replace(" ", "+");

            ////Kopiera meddelande (.xml) från MsgErrorFolder till MsgTempFolder och ta bort dom från MsgErrorFolder
            //string[] ErrorFolder = System.IO.Directory.GetFiles(@drEdiSettings["MsgErrorFolder"].ToString());
            //for (int i = 0; i < ErrorFolder.Length; i++)
            //{
            //    if (File.Exists(ErrorFolder[i]))
            //    {
            //        if (ErrorFolder[i].Contains(".xml") == false) continue;
            //        string FileName = ErrorFolder[i].Replace(@drEdiSettings["MsgErrorFolder"].ToString(), @drEdiSettings["MsgTempFolder"].ToString());
            //        File.Copy(ErrorFolder[i], ErrorFolder[i].Replace(@drEdiSettings["MsgErrorFolder"].ToString(), @drEdiSettings["MsgTempFolder"].ToString()), true);
            //        File.Delete(ErrorFolder[i]);
            //    }
            //}

            //Hämta meddelande från grossister
            if (!onlyMoveFilesMode)
            {
                foreach (DataRow rowEdiSender in dsMain.Tables["SysWholesellerEdi"].Rows)
                {
                    if (rowEdiSender["EdiManagerType"] != null)
                    { 
                        var managerType = (SysWholesellerEdiManagerType)((int)rowEdiSender["EdiManagerType"]);
                        if (managerType == SysWholesellerEdiManagerType.EdiAdminManager)
                        { 
                            continue;
                        }
                    }

                    BehandlaGrossistMeddelande(rowEdiSender);
                }
            }

            //Uppdatera EdiReceivedMsg med antal filer som hämtats från Grossistmappar
            if (Convert.ToInt32(dsMain.Tables["EdiReceivedMsg"].Rows.Count) != 0)
            {
                EdiDatabasMetoderKlass.UppdateraTabellDataSet(Connection, "SoeCompV2", dsMain, "EdiReceivedMsg");
                Console.Out.WriteLine("EdiReceivedMsgCount: " + dsMain.Tables["EdiReceivedMsg"].Rows.Count);
            }

            int nrCount;

            //Kopierar bildfiler från Ftp
            #region Only for SOP
            /*
            ErrorMessage = EdiFtpSupportKlass.DownloadFromFtpValues(drEdiSettings["FtpImageInputFolder"].ToString(), drEdiSettings["FtpUser"].ToString(), drEdiSettings["FtpPassword"].ToString(), "", drEdiSettings["ImageTempFolder"].ToString(), true, out nrCount);
            if (ErrorMessage != "")
            {
                ErrorMessage = EdiFtpSupportKlass.DownloadFromFtpValues(drEdiSettings["FtpImageInputFolder"].ToString(), drEdiSettings["FtpUser"].ToString(), drEdiSettings["FtpPassword"].ToString(), "", drEdiSettings["ImageTempFolder"].ToString(), true, out nrCount);
                if (ErrorMessage != "")
                {
                    MailSubject = "[1] Fel vid hämtning av bildfiler från " + drEdiSettings["FtpImageInputFolder"].ToString();
                    MailMessage = ErrorMessage;
                    EdiDiverseKlass.SkickaMailInternt(drEdiSettings["EmailAddress"].ToString(), MailSubject, MailMessage);
                    EdiDiverseKlass.LagraEdiLog(Connection, dsMain, MailSubject, MailMessage);
                    Console.Error.WriteLine(MailSubject + ": " + MailMessage, EventLogEntryType.Error);
                    //this.timer.Interval = Convert.ToInt32(drEdiSettings["IntervalSecond"]) * 1000;
                    //timer.Start();
                    return;
                }
            }

            if (nrCount > 0)
            {
                Console.Out.WriteLine(string.Concat("Moved ", nrCount, " messages from FtpImageInputFolder: ", drEdiSettings["FtpImageInputFolder"].ToString()));
            }
             */
            #endregion Obsolete

            //Kopierar meddelande från Ftp och Symbrio-mappen

            nrCount = 0;
            ErrorMessage = EdiFtpSupportKlass.DownloadFromFtpValues(drEdiSettings["FtpMsgInputFolder"].ToString(), drEdiSettings["FtpUser"].ToString(), drEdiSettings["FtpPassword"].ToString(), "", drEdiSettings["MsgTempFolder"].ToString(), true, out nrCount);
            if (ErrorMessage != "")
            {
                ErrorMessage = EdiFtpSupportKlass.DownloadFromFtpValues(drEdiSettings["FtpMsgInputFolder"].ToString(), drEdiSettings["FtpUser"].ToString(), drEdiSettings["FtpPassword"].ToString(), "", drEdiSettings["MsgTempFolder"].ToString(), true, out nrCount);
                if (ErrorMessage != "")
                {
                    MailSubject = "[2] Fel vid hämtning av grossistmeddelande från " + drEdiSettings["FtpMsgInputFolder"].ToString();
                    MailMessage = ErrorMessage;
                    EdiDiverseKlass.SkickaMailInternt(drEdiSettings["EmailAddress"].ToString(), MailSubject, MailMessage);
                    EdiDiverseKlass.LagraEdiLog(Connection, dsMain, MailSubject, MailMessage);
                    Console.Error.WriteLine(MailSubject + ": " + MailMessage, EventLogEntryType.Error);
                    //this.timer.Interval = Convert.ToInt32(drEdiSettings["IntervalSecond"]) * 1000;
                    //timer.Start();
                    return;
                }
            }

            if (nrCount > 0)
            {
                Console.Out.WriteLine(string.Concat("Moved ", nrCount, " messages from FtpMsgInputFolder: ", drEdiSettings["FtpMsgInputFolder"].ToString()));
            }

            // Fetch messages from other EdiAdmin instances (from ftp)
            nrCount = 0;
            var notFoundFtpFolder = drEdiSettings["FtpCustomerNotFoundFolder"];
            var directories = EdiFtpSupportKlass.ListDirectoriesOnFtp(notFoundFtpFolder, drEdiSettings["FtpUser"].ToString(), drEdiSettings["FtpPassword"].ToString());
            foreach (var dir in directories.Where(d => d.ToLower() != this.dbName))
            {
                string ftpPath = notFoundFtpFolder + "\\" + dir;
                var files = EdiFtpSupportKlass.ListFilesAndDirectoriesOnFtp(ftpPath, drEdiSettings["FtpUser"].ToString(), drEdiSettings["FtpPassword"].ToString()).Where(d => !d.IsDirectory);
                foreach (var f in files)
                {
                    int tmpNrCount = 0;

                    if (f.Name.ToLower().Contains(this.dbName.ToLower()))
                    {
                        bool allTried = directories.All(d => f.Name.ToLower().Contains(d.ToLower()));
                        if ((allTried || directories.Count() == 1) && !this.onlyMoveFilesMode)
                        {
                            // Download in order to move to error folder later
                        }
                        else
                        {
                            // Just leave it on ftp, already tried on this instance
                            continue;
                        }
                    }

                    ErrorMessage = EdiFtpSupportKlass.DownloadFromFtpValues(ftpPath, drEdiSettings["FtpUser"].ToString(), drEdiSettings["FtpPassword"].ToString(), f.Name, drEdiSettings["MsgTempFolder"].ToString(), true, out tmpNrCount);
                    if (!string.IsNullOrEmpty(ErrorMessage))
                        break;

                    nrCount += tmpNrCount;
                }

                if (ErrorMessage != "")
                {
                    MailSubject = "[7] Fel vid hämtning av grossistmeddelande från " + ftpPath;
                    MailMessage = ErrorMessage;
                    EdiDiverseKlass.SkickaMailInternt(drEdiSettings["EmailAddress"].ToString(), MailSubject, MailMessage);
                    EdiDiverseKlass.LagraEdiLog(Connection, dsMain, MailSubject, MailMessage);
                    Console.Error.WriteLine(MailSubject + ": " + MailMessage, EventLogEntryType.Error);
                }
            }

            if (nrCount > 0)
            {
                Console.Out.WriteLine(string.Concat("Fetching ", nrCount, " messages from FtpCustomerNotFoundFolder: ", notFoundFtpFolder));
            }

            //Behandla samtliga Xml-filer i "Meddelande - Temporär mapp" 
            string[] TempFolder = System.IO.Directory.GetFiles(@drEdiSettings["MsgTempFolder"].TrimEnd('\\', '/') + '\\');
            dsMain.Tables["SysEdiReceived"].Rows[0]["ReceivedFiles"] = TempFolder.Length;
            nrCount = 0;
            int wrongEnvCount = 0;
            for (int i = 0; i < TempFolder.Length; i++)
            {
                if (File.Exists(TempFolder[i]))
                {
                    DataSet dsMeddelandeFil = new DataSet();
                    //Läs in meddelande till Datset
                    try
                    {
                        dsMeddelandeFil.ReadXml(TempFolder[i]);
                    }
                    catch
                    {
                        FelaktigtMeddelande(TempFolder[i]);
                        MailSubject = "[3] Fel vid hämtning av meddelande från " + drEdiSettings["FtpMsgInputFolder"].ToString();
                        MailMessage = "Meddelandefilen '" + TempFolder[i] + "' är inte en XML-fil";
                        EdiDiverseKlass.SkickaMailInternt(drEdiSettings["EmailAddress"].ToString(), MailSubject, MailMessage);
                        EdiDiverseKlass.LagraEdiLog(Connection, dsMain, MailSubject, MailMessage);
                        Console.Error.WriteLine(MailSubject + ": " + MailMessage, EventLogEntryType.Error);
                        try
                        { System.IO.File.Delete(TempFolder[i]); }
                        catch
                        {
                            MailSubject = "[4] Fel vid borttag av meddelande";
                            MailMessage = "Meddelandefilen '" + TempFolder[i];
                            EdiDiverseKlass.SkickaMailInternt(drEdiSettings["EmailAddress"].ToString(), MailSubject, MailMessage);
                            EdiDiverseKlass.LagraEdiLog(Connection, dsMain, MailSubject, MailMessage);
                            Console.Error.WriteLine(MailSubject + ": " + MailMessage, EventLogEntryType.Error);
                        }
                        continue;
                    }

                    //Skriv om meddelande med en rad per Tagg (krav för Cobol)
                    dsMeddelandeFil.WriteXml(TempFolder[i], System.Data.XmlWriteMode.IgnoreSchema);
                    //Kontroll och flytta fil till kundmapp på FTP-server
                    MoveToCustomersResult result = FlyttaTillKund(dsMeddelandeFil, TempFolder[i]);
                    if (result == MoveToCustomersResult.Retry)
                        result = FlyttaTillKund(dsMeddelandeFil, TempFolder[i]);

                    if (result == MoveToCustomersResult.WrongEnviroment)
                    {
                        wrongEnvCount++;
                    }
                    else if (result != MoveToCustomersResult.Success)
                    {
                        drMain["State"] = result == MoveToCustomersResult.SetupError ? (int)EdiTransferState.EdiCompanyNotFound : (int)EdiTransferState.Error;
                        MailSubject = "[5] Fel vid 'Flytta meddelande till kund'";
                        MailMessage = ErrorMessage;
                        string toAddress = result == MoveToCustomersResult.SetupError ? drEdiSettings["SetupErrorEmailAddress"].ToString() : drEdiSettings["EmailAddress"].ToString();
                        EdiDiverseKlass.SkickaMailInternt(toAddress, MailSubject, MailMessage, MailAttachment);
                        EdiDiverseKlass.LagraEdiLog(Connection, dsMain, MailSubject, MailMessage);
                        Console.Error.WriteLine(MailSubject + ": " + MailMessage, EventLogEntryType.Error);
                        // Cleanup
                        MailAttachment = MailSubject = MailMessage = string.Empty;
                    }
                    drLoggInfo["AntalMeddelandeLästa"] = Convert.ToInt32(drLoggInfo["AntalMeddelandeLästa"]) + 1;

                    // Ta bort behandlat meddelande
                    try
                    {
                        File.Delete(TempFolder[i]);
                        if (result != MoveToCustomersResult.WrongEnviroment)
                            nrCount++;
                    }
                    catch
                    {
                        try
                        {
                            File.Delete(TempFolder[i]);
                        }
                        catch (Exception ex)
                        {
                            MailSubject = "[6] Fel vid borttag av meddelande";
                            MailMessage = "Meddelandefilen '" + TempFolder[i] + Environment.NewLine + "Exception: " + ex.Message;
                            EdiDiverseKlass.SkickaMailInternt(drEdiSettings["EmailAddress"].ToString(), MailSubject, MailMessage);
                            EdiDiverseKlass.LagraEdiLog(Connection, dsMain, MailSubject, MailMessage);
                            Console.Error.WriteLine(MailSubject + ": " + MailMessage, EventLogEntryType.Error);
                        }
                    }
                }
            }

            Console.Out.WriteLine(string.Concat("Moved ", wrongEnvCount, " messages to CustomerNotFound folder on ftp."));
            Console.Out.WriteLine(string.Concat("Moved ", nrCount, " messages to customer folders."));

            //Uppdatera EdiTransfer med samtliga filer som flyttats till Kundmappar
            EdiDatabasMetoderKlass.UppdateraTabellDataSet(Connection, "SoeCompV2", dsMain, "EdiTransfer");

            //Uppdatera EdiReceivedMsg med antal filer som hämtats från Grossistmappar
            if (Convert.ToInt32(dsMain.Tables["EdiReceivedMsg"].Rows.Count) != 0)
            {
                EdiDatabasMetoderKlass.UppdateraTabellDataSet(Connection, "SoeCompV2", dsMain, "EdiReceivedMsg");
                Console.Out.WriteLine("EdiReceivedMsgCount: " + dsMain.Tables["EdiReceivedMsg"].Rows.Count);
            }

            //Uppdatera SysEdiReceived med antal filer som hämtats från Indatamappen
            if (Convert.ToInt32(dsMain.Tables["SysEdiReceived"].Rows[0]["ReceivedFiles"]) != 0)
            {
                EdiDatabasMetoderKlass.UppdateraTabellDataSet(Connection, "SoeSysV2", dsMain, "SysEdiReceived");
                Console.Out.WriteLine("EdiReceivedCount: " + dsMain.Tables["SysEdiReceived"].Rows.Count);
            }

            //Uppdatera Company med antal meddelande
            EdiDatabasMetoderKlass.UppdateraTabellDataSet(Connection, "SoeCompV2", dsMain, "Company");
            dsMain.Tables["Company"].AcceptChanges();

            //Skapar xml-fil på Ftp-servern i mappen 'ftp.softone.se/Grossister/ReceivedMessages' som visar kunder som fått meddelande
            if (Convert.ToBoolean(Convert.ToInt32(drEdiSettings["ReceivedMessagesFunction"])) == true && dsMeddelande.Tables["Message"].Rows.Count > 0)
            {
                //Kontroll om angiven "FtpReceivedMessagesFolder mapp" finns upplagd på Ftp-servern
                ErrorMessage = EdiFtpSupportKlass.ListDirectoryFtpValue(drEdiSettings["FtpReceivedMessagesFolder"].ToString(), drEdiSettings["FtpUser"].ToString(), drEdiSettings["FtpPassword"].ToString());
                if (ErrorMessage == "")
                {
                    string FilDatum = drLoggInfo["SenasteStartTid"].ToString().Substring(0, 10);
                    string FilTid = drLoggInfo["SenasteStartTid"].ToString().Substring(11, 8);
                    FilTid = FilTid.Replace(":", "");
                    string MeddelandeFileName = "ReceivedMessages_" + FilDatum + "_" + FilTid + ".xml";
                    dsMeddelande.WriteXml(Directory.GetCurrentDirectory() + "\\" + MeddelandeFileName, System.Data.XmlWriteMode.IgnoreSchema);
                    ErrorMessage = EdiFtpSupportKlass.UploadFileToFtpValues(drEdiSettings["FtpReceivedMessagesFolder"].ToString(), drEdiSettings["FtpUser"].ToString(), drEdiSettings["FtpPassword"].ToString(), Directory.GetCurrentDirectory(), MeddelandeFileName);
                    File.Delete(Directory.GetCurrentDirectory() + "\\" + MeddelandeFileName);
                }
            }

            ////Om det finns felaktiga meddelande skrivs e-post
            //if (Convert.ToInt32(dsMain.Tables["SysEdiReceived"].Rows[0]["ReceivedIncorrect"]) != 0)
            //{
            //    MailSubject = "[7] Felaktiga Meddelande-filer vid hämtning från Freesourcing";
            //    MailMessage = "";
            //    EdiDiverseKlass.SkickaMailInternt(drEdiSettings["EmailAddress"].ToString(), MailSubject, MailMessage);
            //    EdiDiverseKlass.LagraEdiLog(Connection, dsMain, MailSubject, MailMessage);
            //}

            //E-post skickas om det finns filer att hämta och om e-postadress finns angivet för kundens meddelandettyp 
            string OrgNr = "";
            string EdiFolder = "";
            int mailSent = 0;
            foreach (DataRow row in dsMain.Tables["AllConnections"].Rows)
            {
                if (row["Email"].ToString() == "") continue;
                if (Convert.ToInt32(row["AntalFilerKund"]) == 0) continue;
                if (row["OrgNr"].ToString() == OrgNr & row["EdiFolder"].ToString() == EdiFolder) continue;
                MailSubject = "SoftOnes EDI-server";
                MailMessage = "Meddelande-filer finns att hämta från Ftp-adress: " + row["EdiFolder"].ToString();
                EdiDiverseKlass.SkickaMailExternt(drEdiSettings["EmailAddress"].ToString(), row["Email"].ToString(), MailSubject, MailMessage);
                OrgNr = row["OrgNr"].ToString();
                EdiFolder = row["EdiFolder"].ToString();
                mailSent++;
            }
            Console.Out.WriteLine(string.Format("{0} notifications email sent out to customers", mailSent));

            //Tom rad skapas i "EdiTransferTotalt" som avskiljare om redovisning redan finns sedan "Starta"
            if (dsMain.Tables["EdiTransferTotalt"].Rows.Count > 0 & dsMain.Tables["EdiTransfer"].Rows.Count > 0)
            {
                drMain = dsMain.Tables["EdiTransferTotalt"].NewRow();
                dsMain.Tables["EdiTransferTotalt"].Rows.Add(drMain);
                drMain["Debiting"] = false;
            }
            //För varje hämtat meddelande skapas en rad i "EdiTransferTotalt"
            foreach (DataRow row in dsMain.Tables["EdiTransfer"].Rows)
            {
                drMain = dsMain.Tables["EdiTransferTotalt"].NewRow();
                dsMain.Tables["EdiTransferTotalt"].Rows.Add(drMain);
                foreach (DataColumn col in dsMain.Tables["EdiTransfer"].Columns)
                    drMain[col.ColumnName] = row[col.ColumnName];
            }

            //Flytta redovisning av meddelande till sista raden i gridden
            //if (dsMain.Tables["EdiTransferTotalt"].Rows.Count > 0)
            //    grdTransfer.Rows[grdTransfer.Rows.Count - 1].Activate();
        }

        public string GetStatusMessage()
        {
            return string.Format("EdiAdmin status: connection: {0}, ftpUser: {1}, templates folder: {2}, machine: {3}, EdiAdmin.Business version: {4}, onlyMoveFilesMode: {5}", this.dbName, drEdiSettings["FtpUser"], drEdiSettings["StandardTemplatesFolder"], System.Environment.MachineName, this.GetType().Assembly.GetName().Version.ToString(4), this.onlyMoveFilesMode);
        }
        #endregion

        #region Private Methods
        private void BehandlaGrossistMeddelande(DataRow SenderRow)
        {
            #region Validation
            //Kontroll att grossist lämnar edi-meddelande och att ftp-mapp finns angiven
            if (SenderRow["SenderId"].ToString() == "AH" |
                SenderRow["SenderId"].ToString() == "ES" |
                SenderRow["SenderId"].ToString() == "SO" |
                SenderRow["SenderId"].ToString() == "ST" |
                SenderRow["SenderId"].ToString() == "SE" |
                SenderRow["SenderId"].ToString() == "LG" |
                SenderRow["SenderId"].ToString() == "MO" |
                SenderRow["SenderId"].ToString() == "DA" |
                SenderRow["SenderId"].ToString() == "EG" |
                SenderRow["SenderId"].ToString() == "VC" |
                SenderRow["SenderId"].ToString() == "NELFO" |
                SenderRow["SenderId"].ToString() == "CO" |
                (SenderRow["SenderId"].ToString().Length >= 2 && SenderRow["SenderId"].ToString().Substring(0, 2) == "N5") |
                (SenderRow["SenderId"].ToString().Length >= 3 && SenderRow["SenderId"].ToString().Substring(0, 3) == "N40") |
                (SenderRow["SenderId"].ToString().Length >= 2 && SenderRow["SenderId"].ToString().Substring(0, 2) == "LN"))
            {
                if (Convert.ToBoolean(SenderRow["EdiFolder"].ToString().Contains("ftp://")) != true) return;
            }
            else
            {
                return;
            }

            #endregion

            #region FetchFromFTP

            if (Convert.ToBoolean(SenderRow["EdiFolder"].ToString().Contains("ftp://")) != true) return;

            //Kontroll om mappen drEdiSettings["WholesaleTempFolder"].ToString() + "\\" + SenderRow["SenderName"].ToString() finns annars upplägg
            string WholesaleTempFolder = EdiDiverseKlass.KontrollGrossistMapp(drEdiSettings, SenderRow);
            if (WholesaleTempFolder == "") return;

            //Kopierar filer från "FtpMsgInputFolder" för aktuell grossist på FTP-server till "WholesaleTempFolder"
            //i det lokala nätverket och tar bort filerna från "FtpMsgInputFolder" på FTP-servern
            int AntalFiler = 0;
            ErrorMessage = EdiFtpSupportKlass.DownloadFromFtpValues(SenderRow["EdiFolder"].ToString(), drEdiSettings["FtpUser"].ToString(), drEdiSettings["FtpPassword"].ToString(), "", @WholesaleTempFolder, true, out AntalFiler);
            if (ErrorMessage != "")
            {
                ErrorMessage = EdiFtpSupportKlass.DownloadFromFtpValues(SenderRow["EdiFolder"].ToString(), drEdiSettings["FtpUser"].ToString(), drEdiSettings["FtpPassword"].ToString(), "", @WholesaleTempFolder, true, out AntalFiler);
                if (ErrorMessage != "")
                {
                    MailSubject = "[16] Fel vid hämtning av meddelande från " + SenderRow["EdiFolder"].ToString();
                    MailMessage = ErrorMessage;
                    EdiDiverseKlass.SkickaMailInternt(drEdiSettings["EmailAddress"].ToString(), MailSubject, MailMessage);
                    EdiDiverseKlass.LagraEdiLog(Connection, dsMain, MailSubject, MailMessage);
                    Console.Error.WriteLine(MailSubject + ": " + MailMessage, EventLogEntryType.Error);
                }
            }
            if (string.IsNullOrEmpty(ErrorMessage) && AntalFiler > 0)
            {
                Console.Out.WriteLine(string.Concat("Flyttat ", AntalFiler, " filer från grossisten ", SenderRow["SenderId"].ToString(), ". FTP mapp: ", SenderRow["EdiFolder"].ToString()));
            }

            #endregion FetchFromFTP

            //Kopierar hämtade filer i "WholesaleTempFolder" till filer med nytt namn "SSÅÅ-MM-DD_TTMMSS_grossistnamn_löpnr.xml"
            #region MoveRenameFiles
            string FilDatum = drLoggInfo["SenasteStartTid"].ToString().Substring(0, 10);
            string FilTid = drLoggInfo["SenasteStartTid"].ToString().Substring(11, 8);
            FilTid = FilTid.Replace(":", "");
            int FilNummer = 1;
            string[] InputMappFile = System.IO.Directory.GetFiles(@WholesaleTempFolder);
            for (int i = 0; i < InputMappFile.Length; i++)
            {
                Guid uId = Guid.NewGuid();
                string NewMappFile = InputMappFile[i].Replace(@WholesaleTempFolder + "\\", "");
                if (NewMappFile.Length > 17 && NewMappFile.Substring(10, 1) == "_" & NewMappFile.Substring(17, 1) == "_") continue;
                NewMappFile = @WholesaleTempFolder + "\\" + uId + "_" + FilDatum + "_" + FilTid + "_" + SenderRow["SenderName"] + "_" + FilNummer++ + ".edi";

                drMain = dsMain.Tables["EdiReceivedMsg"].NewRow();
                dsMain.Tables["EdiReceivedMsg"].Rows.Add(drMain);
                drMain["SysWholesellerEdiId"] = SenderRow["SysWholesellerEdiId"];
                drMain["UniqueId"] = uId.ToString();
                drMain["MsgDate"] = DatumReceived;
                drMain["InFilename"] = InputMappFile[i].Replace(@WholesaleTempFolder + "\\", "");
                drMain["OutFilename"] = NewMappFile.Replace(@WholesaleTempFolder + "\\", "");
                StreamReader sr = new StreamReader(InputMappFile[i], Encoding.Default);
                drMain["FileData"] = sr.ReadToEnd();
                drMain["State"] = (int)EdiRecivedMsgState.UnderProgress;

                sr.Close();

                if (SenderRow["SenderId"].ToString() == "ES")
                {
                    using (TextReader reader = new StreamReader(InputMappFile[i], Encoding.UTF8))
                    {
                        using (TextWriter writer = new StreamWriter(NewMappFile, false, Encoding.UTF8))
                        {
                            string text = null;
                            while ((text = reader.ReadLine()) != null)
                            {
                                if (text.Contains("<ns0:Invoices") == true)
                                {
                                    int tkn = text.IndexOf(">", 0) + 1;
                                    text = text.Substring(tkn);
                                    text = text.Replace("</ns0:Invoices>", "");
                                    writer.WriteLine(text);
                                }
                                else
                                    writer.WriteLine(text);
                            }
                            reader.Close();
                            writer.Flush();
                            writer.Close();
                        }
                    }
                }
                else
                {
                    try
                    {
                        System.IO.File.Copy(InputMappFile[i], NewMappFile, true);
                    }
                    catch
                    {
                        MailSubject = "[17] Fel vid kopiering av meddelande";
                        MailMessage = "Från meddelande: " + InputMappFile[i] + "   Till meddelande: " + NewMappFile;
                        EdiDiverseKlass.SkickaMailInternt(drEdiSettings["EmailAddress"].ToString(), MailSubject, MailMessage);
                        EdiDiverseKlass.LagraEdiLog(Connection, dsMain, MailSubject, MailMessage);
                        Console.Error.WriteLine(MailSubject + ": " + MailMessage, EventLogEntryType.Error);
                    }
                }
                try
                {
                    System.IO.File.Delete(InputMappFile[i]);
                }
                catch
                {
                    MailSubject = "[18] Fel vid borttag av meddelande";
                    MailMessage = "Meddelandefilen '" + InputMappFile[i];
                    EdiDiverseKlass.SkickaMailInternt(drEdiSettings["EmailAddress"].ToString(), MailSubject, MailMessage);
                    EdiDiverseKlass.LagraEdiLog(Connection, dsMain, MailSubject, MailMessage);
                    Console.Error.WriteLine(MailSubject + ": " + MailMessage, EventLogEntryType.Error);
                }
            }
            #endregion MoveRenameFiles

            //Behandla samtliga Xml-filer i Temporära mappen för att få meddelande med en rad per tagg (krav för Professional)
            //Kopierar hämtade filer i "WholesaleTempFolder" mappen till "WholesaleSaveFolder" mappen
            //Tar bort hämtade filer i "WholesaleTempFolder" mappen
            #region Parse files/Handle WholesaleTempFolder
            string[] ProcessMappFile = System.IO.Directory.GetFiles(@WholesaleTempFolder);
            for (int i = 0; i < ProcessMappFile.Length; i++)
            {
                bool MsgOk = false;

                if (File.Exists(ProcessMappFile[i]))
                {
                    try
                    {
                        #region ConvertFile

                        drLoggInfo["AntalGrossistLästa"] = Convert.ToInt32(drLoggInfo["AntalGrossistLästa"]) + 1;
                        foreach (DataTable tb in dsStandardMall.Tables) tb.Rows.Clear();
                        MsgOk = true;

                        if (SenderRow["SenderId"].ToString().Contains("N5") == true)
                        {
                            MsgOk = EdiNelfo5Klass.Nelfo5(ProcessMappFile[i], @WholesaleTempFolder, dsStandardMall, drEdiSettings);
                        }
                        else if (SenderRow["SenderId"].ToString().Contains("N40") == true)
                        {
                            MsgOk = EdiNelfo40Klass.Nelfo40(ProcessMappFile[i], @WholesaleTempFolder, dsStandardMall, drEdiSettings);
                        }
                        else if (SenderRow["SenderId"].ToString().Substring(0, 2) == "LN")
                        {
                            List<string> dummy;
                            MsgOk = BehandlaGrossistMeddelandeLvis(ProcessMappFile[i], @WholesaleTempFolder, out dummy);
                        }
                        else if (SenderRow["SenderId"].ToString() == "ST")
                        {
                            ////I och med att Storel är uppköpt av Selga så skickas under en övergång meddelande enligt två format 
                            using (TextReader reader = new StreamReader(ProcessMappFile[i], Encoding.UTF8))
                            {
                                bool isXML = reader.ReadLine().StartsWith("<?xml");
                                reader.Close();
                                if (isXML)
                                {
                                    MsgOk = EdiSelgaKlass.Selga(ProcessMappFile[i], @WholesaleTempFolder, dsStandardMall, drEdiSettings, SenderRow);
                                }
                                else
                                {
                                    MsgOk = EdiFactGenerellKlass.EdiFact(ProcessMappFile[i], @WholesaleTempFolder, dsStandardMall, drEdiSettings, SenderRow);
                                }
                            }
                        }
                        else if (SenderRow["SenderId"].ToString() == "NELFO")
                        {
                            List<string> dummy;
                            MsgOk = BehandlaGrossistMeddelandeNelfo(ProcessMappFile[i], @WholesaleTempFolder, out dummy);
                        }
                        else
                        {
                            EdiSenderBase ediSender = null;

                            switch (SenderRow["SenderId"].ToString())
                            {
                                case "AH":
                                    MsgOk = EdiFactGenerellKlass.EdiFact(ProcessMappFile[i], @WholesaleTempFolder, dsStandardMall, drEdiSettings, SenderRow);
                                    break;
                                case "SO":
                                    MsgOk = EdiFactGenerellKlass.EdiFact(ProcessMappFile[i], @WholesaleTempFolder, dsStandardMall, drEdiSettings, SenderRow);
                                    break;
                                case "DA":
                                    MsgOk = EdiFactGenerellKlass.EdiFact(ProcessMappFile[i], @WholesaleTempFolder, dsStandardMall, drEdiSettings, SenderRow);
                                    break;
                                //case "ST":
                                //    MsgOk = EdiFactGenerellKlass.EdiFact(ProcessMappFile[i], @WholesaleTempFolder, dsStandardMall, drEdiSettings, Connection, dsMain, SenderRow);
                                //    break;
                                case "ES":
                                    MsgOk = EdiElektroskandiaKlass.Elektroskandia(ProcessMappFile[i], @WholesaleTempFolder, dsStandardMall, drEdiSettings, Connection, dsMain);
                                    break;
                                case "LG":
                                    MsgOk = EdiLundagrossistenKlass.Lundagrossisten(ProcessMappFile[i], @WholesaleTempFolder, dsStandardMall, drEdiSettings, Connection, dsMain);
                                    break;
                                case "MO":
                                    MsgOk = EdiMoelKlass.Moel(ProcessMappFile[i], @WholesaleTempFolder, dsStandardMall, drEdiSettings, Connection, dsMain);
                                    break;
                                case "SE":
                                    MsgOk = EdiSelgaKlass.Selga(ProcessMappFile[i], @WholesaleTempFolder, dsStandardMall, drEdiSettings, SenderRow);
                                    break;
                                case "EG":
                                    MsgOk = EdiElgrossnKlass.Elgrossn(ProcessMappFile[i], @WholesaleTempFolder, dsStandardMall, drEdiSettings, Connection, dsMain, SenderRow);
                                    break;
                                case "VC":
                                    MsgOk = EdiFactGenerellKlass.EdiFact(ProcessMappFile[i], @WholesaleTempFolder, dsStandardMall, drEdiSettings, SenderRow);
                                    break;
                                case "CO":
                                    ediSender = new EdiComfort();
                                    break;
                                default:
                                    break;
                            }
                        }

                        #endregion
                    }
                    catch (Exception ex)
                    {
                        MsgOk = false;
                        drMain["State"] = (int)EdiRecivedMsgState.Error;
                        Console.Error.WriteLine("ERROR BG1: Fel vid behandling av fil: " + ProcessMappFile[i] + ". Felmeddelande: " + ex.Message);
                    }

                    if (MsgOk == false)
                    {
                        string ErrorMappFile = ProcessMappFile[i].Replace(@WholesaleTempFolder, @drEdiSettings["WholesaleErrorFolder"].ToString());
                        try
                        {
                            System.IO.File.Copy(ProcessMappFile[i], ErrorMappFile, true);
                        }
                        catch
                        {
                            MailSubject = "[19] Fel vid kopiering av meddelande";
                            MailMessage = "Från meddelande: " + ProcessMappFile[i] + "   Till meddelande: " + ErrorMappFile;
                            EdiDiverseKlass.SkickaMailInternt(drEdiSettings["EmailAddress"].ToString(), MailSubject, MailMessage);
                            EdiDiverseKlass.LagraEdiLog(Connection, dsMain, MailSubject, MailMessage);
                            drMain["State"] = (int)EdiRecivedMsgState.Error;
                            Console.Error.WriteLine(MailSubject + ": " + MailMessage, EventLogEntryType.Error);
                        }
                        drLoggInfo["AntalGrossistAvvisade"] = Convert.ToInt32(drLoggInfo["AntalGrossistAvvisade"]) + 1;
                    }
                    else
                    {
                        drMain["State"] = (int)EdiRecivedMsgState.Transferred;
                        string SaveMappFile = ProcessMappFile[i].Replace(@WholesaleTempFolder, @drEdiSettings["WholesaleSaveFolder"].TrimEnd('\\', '/') + "\\");
                        try
                        {
                            System.IO.File.Copy(ProcessMappFile[i], SaveMappFile, true);
                        }
                        catch
                        {
                            MailSubject = "[20] Fel vid kopiering av meddelande";
                            MailMessage = "Från meddelande: " + ProcessMappFile[i] + "   Till meddelande: " + SaveMappFile;
                            EdiDiverseKlass.SkickaMailInternt(drEdiSettings["EmailAddress"].ToString(), MailSubject, MailMessage);
                            EdiDiverseKlass.LagraEdiLog(Connection, dsMain, MailSubject, MailMessage);
                            drMain["State"] = (int)EdiRecivedMsgState.Error;
                            Console.Error.WriteLine(MailSubject + ": " + MailMessage, EventLogEntryType.Error);
                        }
                        drLoggInfo["AntalGrossistSkrivna"] = Convert.ToInt32(drLoggInfo["AntalGrossistSkrivna"]) + 1;
                    }

                    try
                    {
                        System.IO.File.Delete(ProcessMappFile[i]);
                    }
                    catch
                    {
                        MailSubject = "[21] Fel vid borttag av meddelande";
                        MailMessage = "Meddelandefilen '" + ProcessMappFile[i];
                        EdiDiverseKlass.SkickaMailInternt(drEdiSettings["EmailAddress"].ToString(), MailSubject, MailMessage);
                        EdiDiverseKlass.LagraEdiLog(Connection, dsMain, MailSubject, MailMessage);
                        drMain["State"] = (int)EdiRecivedMsgState.Error;
                        Console.Error.WriteLine(MailSubject + ": " + MailMessage, EventLogEntryType.Error);
                    }
                }
            }
            #endregion
        }

        private bool TransferToTempFolder(string inputFile, string wholesaleTempFolder, string content)
        {
            string OutputFolderFileName = inputFile.Replace(".edi", ".xml");
            if (ErrorMessage == "")
            {
                //dsStandardMall.WriteXml(OutputFolderFileName, System.Data.XmlWriteMode.IgnoreSchema);
                string UploadFileName = OutputFolderFileName.Replace(wholesaleTempFolder + "\\", "");
                try
                {
                    using (var stream = File.CreateText(OutputFolderFileName.Replace(wholesaleTempFolder, drEdiSettings["MsgTempFolder"].ToString())))
                    {
                        stream.Write(content);
                    }
                    //File.Copy(OutputFolderFileName, OutputFolderFileName.Replace(wholesaleTempFolder, drEdiSettings["MsgTempFolder"].ToString()), true);
                }
                catch
                {
                    string MailSubject = "[ES-4] Fel vid överföring från grossist - Meddelandefil: " + inputFile;
                    string MailMessage = "Det gick inte att kopiera filen " + UploadFileName + " till Temporärmappen: " + drEdiSettings["MsgTempFolder"].ToString();
                    EdiDiverseKlass.SkickaMailInternt(drEdiSettings["EmailAddress"].ToString(), MailSubject, MailMessage);
                    EdiDiverseKlass.LagraEdiLog(Connection, dsMain, MailSubject, MailMessage);
                    return false;
                }
            }
            else
            {
                string MailSubject = "[EG-4] Fel vid överföring från grossist - Meddelandefil: " + inputFile;
                string MailMessage = ErrorMessage;
                EdiDiverseKlass.SkickaMailInternt(drEdiSettings["EmailAddress"].ToString(), MailSubject, MailMessage);
                EdiDiverseKlass.LagraEdiLog(Connection, dsMain, MailSubject, MailMessage);
                return false;
            }

            try
            {
                File.Delete(OutputFolderFileName);
            }
            catch
            {
                string MailSubject = "[EG-5] Fel vid borttag av meddelande";
                string MailMessage = "Meddelandefilen '" + OutputFolderFileName;
                EdiDiverseKlass.SkickaMailInternt(drEdiSettings["EmailAddress"].ToString(), MailSubject, MailMessage);
                EdiDiverseKlass.LagraEdiLog(Connection, dsMain, MailSubject, MailMessage);
            }

            return true;
        }

        public bool BehandlaGrossistMeddelandeLvis(string inputFile, string wholeSellerTempFolder, out List<string> parsedContent, string fileContent = null)
        {
            bool MsgOk = false;
            parsedContent = null;
            MsgOk = EdiLvisNetKlass.LvisNet(inputFile, wholeSellerTempFolder, dsStandardMall, drEdiSettings, fileContent);
            parsedContent = EdiLvisNetKlass.ToXmls().ToList();

            return MsgOk;
        }

        public bool BehandlaGrossistMeddelandeNelfo(string inputFile, string wholeSellerTempFolder, out List<string> parsedContent, string fileContent = null)
        {
            bool MsgOk = false;
            parsedContent = null;
            // Try to determine if this is nelfo 3, 4 or 5
            bool nelfo4_scv = false, nelfo5_xml = false;
            string firstLine;

            if (string.IsNullOrEmpty(fileContent))
            {
                using (TextReader reader = new StreamReader(inputFile, Encoding.UTF8))
                {
                    firstLine = reader.ReadLine().ToLower();
                }
            }
            else
            {
                firstLine = fileContent.Substring(0, fileContent.IndexOf(Environment.NewLine));
            }

            if (firstLine.StartsWith("<?xml"))
            {
                //XML is Nelfo 4 or 5. But we assume that this is Nelfo5
                nelfo5_xml = true;
            }
            else
            {
                var columns = firstLine.Split(';');
                if (columns.Count() > 3 && columns[2] == "4.0")
                {
                    // Nelfo 4.0 (semicolon seperated)
                    nelfo4_scv = true;
                }
                //else Nelfo 3.0?, which we don't have support for
            }
            

            // Reading afterward in order to free resources from TextReader
            if (nelfo5_xml)
            {
                MsgOk = EdiNelfo5Klass.Nelfo5(inputFile, wholeSellerTempFolder, dsStandardMall, drEdiSettings);
            }
            else if (nelfo4_scv)
            {
                string InputFileName = inputFile.Replace(wholeSellerTempFolder + "\\", "");
                MsgOk = EdiNelfo40Klass.Nelfo40(InputFileName, wholeSellerTempFolder, dsStandardMall, drEdiSettings, Connection);
                parsedContent = EdiNelfo40Klass.ToXmls().ToList();
            }

            return MsgOk;
        }

        public IEdiSender GetEdiSender(string senderId, string content)
        {
            switch (senderId)
            {
                case "SE":
                    return new EdiSelga();
                default:
                    return null;
            }
        }


        //Tolkar vilken kund och flytta meddelande till kundens "EdiFolder mapp" på Ftp-servern
        //Ta hand om felaktiga filer
        private MoveToCustomersResult FlyttaTillKund(DataSet dsMeddelandeFil, string FromFile)
        {
            // Start with inserting row
            drMain = dsMain.Tables["EdiTransfer"].NewRow();
            dsMain.Tables["EdiTransfer"].Rows.Add(drMain);

            var fromFileName = FromFile.Replace(drEdiSettings["MsgTempFolder"].ToString() + "\\", "");
            var ediRec = EdiCompManagerClass.GetEdiRecivedMsgFromFileName(fromFileName);
            if (ediRec != null)
            {
                drMain["EdiReceivedMsgId"] = ediRec.EdiReceivedMsgId;
                drMain["SysWholesellerEdiId"] = ediRec.SysWholesellerEdiId;
            }
            drMain["TransferDate"] = Convert.ToDateTime(dsMain.Tables["SysEdiReceived"].Rows[0]["ReceivedDate"]);
            drMain["InFilename"] = fromFileName;
            drMain["State"] = (int)EdiTransferState.UnderProgress;
            using (StreamReader sr = new StreamReader(FromFile, Encoding.UTF8))
            {
                drMain["Xml"] = sr.ReadToEnd();
                sr.Close();
            }

            //Kontroll att obligatoriska Xml-taggar finns
            ErrorMessage = "";
            if (dsMeddelandeFil.Tables["MessageInfo"] == null)
                ErrorMessage = "Xml-taggen 'MessageInfo' saknas i meddelande " + FromFile;
            else if (dsMeddelandeFil.Tables["MessageInfo"].Columns["MessageSenderId"] == null)
                ErrorMessage = "Xml-taggen 'MessageSenderId' saknas i meddelande " + FromFile;
            else if (dsMeddelandeFil.Tables["MessageInfo"].Columns["MessageType"] == null)
                ErrorMessage = "Xml-taggen 'MessageType' saknas i meddelande " + FromFile;
            else if (dsMeddelandeFil.Tables["Buyer"] == null)
                ErrorMessage = "Xml-taggen 'Buyer' saknas i meddelande " + FromFile;
            else if (dsMeddelandeFil.Tables["Buyer"].Columns["BuyerId"] == null)
                ErrorMessage = "Xml-taggen 'BuyerId' saknas i meddelande " + FromFile;

            if (ErrorMessage != "")
            {
                FelaktigtMeddelande(FromFile, drMain);
                return MoveToCustomersResult.UnkownError;
            }

            var senderSenderId = dsMeddelandeFil.Tables["MessageInfo"].Rows[0]["MessageSenderId"].ToString();
            var senderTypeId = dsMeddelandeFil.Tables["MessageInfo"].Rows[0]["MessageType"].ToString();
            var buyerId = dsMeddelandeFil.Tables["Buyer"].Rows[0]["BuyerId"].ToString();
            var buyerName = dsMeddelandeFil.Tables["Buyer"].Columns.Contains("BuyerName") ? dsMeddelandeFil.Tables["Buyer"].Rows[0]["BuyerName"].ToString() : string.Empty;
            string fråga = "SenderSenderNr = '" + senderSenderId +
                "' And SenderType = '" + senderTypeId +
                "' And BuyerNr = '" + buyerId + "'";
            DataRow[] drAllConnections = dsMain.Tables["AllConnections"].Select(fråga);

            var state = CompanyState.Unknown;

            if (drAllConnections.Length == 0)
            {
                #region Handle not found
                fråga = "SenderSenderNr = '" + senderSenderId +
                "' And BuyerNr = '" + buyerId + "'";
                drAllConnections = dsMain.Tables["AllConnections"].Select(fråga);

                if (drAllConnections.Length == 0)
                {
                    // Move it back to ftp to let other enviroment check if it has the customer in the db.
                    var notFoundFtpFolder = drEdiSettings["FtpCustomerNotFoundFolder"];
                    var originalFileName = FromFile.Replace(@drEdiSettings["MsgTempFolder"].ToString() + "\\", "");
                    var fileName = originalFileName.Contains(dbName) ? originalFileName : dbName + "_" + originalFileName;

                    var directories = EdiFtpSupportKlass.ListDirectoriesOnFtp(notFoundFtpFolder, drEdiSettings["FtpUser"].ToString(), drEdiSettings["FtpPassword"].ToString());
                    if (!directories.Contains(dbName))
                    {
                        EdiFtpSupportKlass.MakeDirectoryFtp(notFoundFtpFolder + "/" + dbName, drEdiSettings["FtpUser"].ToString(), drEdiSettings["FtpPassword"].ToString());
                        directories.Add(dbName);
                    }

                    bool allTried = directories.All(f => fileName.ToLower().Contains(f.ToLower()));
                    if ((allTried || directories.Count() == 1) && !this.onlyMoveFilesMode)
                    {
                        // TODO mark with error
                        var errorPath = FelaktigtMeddelande(FromFile, drMain);
                        ErrorMessage = "Det saknas uppgifter för att meddelandet skall kunna kopplas " + FromFile + ". Meddelandet flyttat till " + errorPath + Environment.NewLine +
                                "GrossistId = " + senderSenderId + ", Meddelandetyp = " + senderTypeId + ", Köparens namn = " + buyerName + ", Köparens id = " + buyerId + Environment.NewLine + Environment.NewLine;
                        // Todo: mail attachment is currently not working from the server, some permissions missing with service user?
                        // MailAttachment = FromFile;
                        return MoveToCustomersResult.SetupError;
                    }
                    else
                    {
                        ErrorMessage = EdiFtpSupportKlass.UploadFileToFtpValues(notFoundFtpFolder + "/" + dbName + "/" + fileName, drEdiSettings["FtpUser"].ToString(), drEdiSettings["FtpPassword"].ToString(), @drEdiSettings["MsgTempFolder"].ToString(), originalFileName, FtpAdressIncludesFile: true);
                        //Console.Out.WriteLine("Moved 1 file to other enviroment for customer " + buyerName + ", customerNr: " + buyerId);
                        drMain["State"] = (int)EdiTransferState.EdiCompNotFoundTryingOtherEnv;
                        drMain["ErrorMessage"] = "Company not found for enviroment " + dbName + ". Putting file back to FTP to be tested on other enviroments";
                        return MoveToCustomersResult.WrongEnviroment;
                    }
                }
                else
                {
                    drMain["ActorCompanyId"] = drAllConnections[0]["ActorCompanyId"].ToString();
                    state = (CompanyState)drAllConnections[0]["State"];
                    // connection found but does not contain the correct sendertype, if xe add this.
                    if (state == CompanyState.Active)
                    {
                        var ediMsg = dsMain.Tables["SysEdiMsg"].Select("SenderType = '" + senderTypeId + "' AND SenderSenderNr = '" + drAllConnections[0]["SenderSenderNr"] + "'");
                        if (ediMsg.Length == 0)
                            return MoveToCustomersResult.SetupError;

                        // Add the sendertype to the customer
                        DataSet dsEdiConnection = new DataSet();
                        ErrorMessage = EdiDatabasMetoderKlass.HämtaTabellDataSet(Connection, this.dbName, dsEdiConnection, "EdiConnection", "SELECT * FROM EdiConnection");
                        dsEdiConnection.AcceptChanges();

                        if (dsEdiConnection == null || !string.IsNullOrEmpty(ErrorMessage))
                            return MoveToCustomersResult.UnkownError;

                        drMain = dsEdiConnection.Tables["EdiConnection"].NewRow();
                        drMain["ActorCompanyId"] = drAllConnections[0]["ActorCompanyId"];
                        drMain["SysEdiMsgId"] = ediMsg[0]["SysEdiMsgId"];
                        drMain["BuyerNr"] = drAllConnections[0]["BuyerNr"];
                        drMain["Debiting"] = drAllConnections[0]["Debiting"];
                        drMain["EdiFolder"] = @drEdiSettings["FtpRootOutputFolder"] + "/" + drAllConnections[0]["CustFtpUser"] + "/" + "Leveransbesked";  // ftp://ftp.softone.se/Grossister/kundnr

                        Console.Out.WriteLine("New messagetype of type {0} added to XE customer with customercounter={1}.", senderTypeId, drMain["CustomerCounter"]);

                        dsEdiConnection.Tables["EdiConnection"].Rows.Add(drMain);
                        EdiDatabasMetoderKlass.UppdateraTabellDataSet(Connection, "SoeCompV2", dsEdiConnection, "EdiConnection");

                        EdiDiverseKlass.HämtaAllConnections(Connection, dsMain);

                        return MoveToCustomersResult.Retry;
                    }
                    else
                    {
                        return MoveToCustomersResult.SetupError;
                    }
                }

                #endregion
            }

            state = (CompanyState)drAllConnections[0]["State"];
            string ToFile = FromFile.Replace(@drEdiSettings["MsgTempFolder"].ToString() + "\\", "");

            // Customer found, proceed
            drMain["ActorCompanyId"] = drAllConnections[0]["ActorCompanyId"].ToString();
            drMain["SysWholesellerEdiId"] = drAllConnections[0]["SysWholesellerEdiId"].ToString();
            drMain["SysEdiTypeId"] = drAllConnections[0]["SysEdiTypeId"].ToString();
            drMain["OutFilename"] = drAllConnections[0]["EdiFolder"].ToString() + "/" + ToFile.ToString();
            //drMain["Debiting"] = (bool)drAllConnections[0]["Debiting"];


            if (state == CompanyState.Active)
            {
                #region XE
                // Active = XECustomer    
                var outputFolder = drEdiSettings[ApplicationSettingType.XECustomersFolder.ToString()];
                if (!Directory.Exists(outputFolder))
                    Directory.CreateDirectory(outputFolder);

                int actorCompanyId = int.Parse(drAllConnections[0]["ActorCompanyId"].ToString());
                int sysWholesellerEdiId = int.Parse(drMain["SysWholesellerEdiId"].ToString());
                //var originalFileName = FromFile.Replace(@drEdiSettings["MsgTempFolder"].ToString() + "\\", "");
                //ToFile = outputFolder + "\\" + originalFileName;

                var em = new SoftOne.Soe.Business.Core.EdiManager(null);
                var dto = new CompanyEdiDTO()
                {
                    ActorCompanyId = actorCompanyId,
                    Source = FromFile,
                    SourceType = CompanyEdiDTO.SourceTypeEnum.File,
                };

                var result = em.AddEdiEntrysFromSource(dto, false, sysScheduledJobId: this.sysScheduledJobId, batchNr: this.batchNr, sysWholesellerEdiId: sysWholesellerEdiId);
                if (!result.Success)
                {
                    ErrorMessage = "Fel vid inläsning till XE av fil: " + ToFile + (Enum.IsDefined(typeof(ActionResultSave), result.ErrorNumber) ? ". Felnr: " + ((ActionResultSave)result.ErrorNumber).ToString() : string.Empty) + ". Felmeddelande: " + result.ErrorMessage;
                    FelaktigtMeddelande(FromFile, drMain);
                    return MoveToCustomersResult.UnkownError;
                }

                #endregion
            }
            else if (state == CompanyState.SOPCustomer)
            {
                #region SOP and FTP
                // TODO SOP Customer is not implimented for this version
                if (drAllConnections[0]["EdiFolder"].ToString() == "" | Convert.ToBoolean(drAllConnections[0]["EdiFolder"].ToString().ToLower().Contains("ftp://")) != true)
                {
                    ErrorMessage = "Ingen Ftp-mapp eller är felaktigt angiven för kunden på Ftp-servern " + FromFile;
                    FelaktigtMeddelande(FromFile, drMain);
                    return MoveToCustomersResult.UnkownError;
                }

                //Lägger till rad för XSL-fil
                if (drAllConnections[0]["XslRow"].ToString() != "")
                    EdiDiverseKlass.LäggTillXsl(FromFile, drAllConnections[0]["XslRow"].ToString());

                //Kopierar meddelande till kundens "EdiFolder mapp" på Ftp servern
                ErrorMessage = EdiFtpSupportKlass.UploadFileToFtpValues(drAllConnections[0]["EdiFolder"].ToString(), drEdiSettings["FtpUser"].ToString(), drEdiSettings["FtpPassword"].ToString(), drEdiSettings["MsgTempFolder"].ToString(), ToFile);
                if (ErrorMessage != "")
                {
                    //Kontroll om kundens "EdiFolder mapp" finns upplagd på Ftp-servern
                    ErrorMessage = EdiFtpSupportKlass.ListDirectoryFtpValue(drAllConnections[0]["EdiFolder"].ToString(), drEdiSettings["FtpUser"].ToString(), drEdiSettings["FtpPassword"].ToString());
                    if (ErrorMessage != "")
                    {
                        //Skapar kundens "EdiFolder mapp" på Ftp servern om den inte finns upplagd
                        ErrorMessage = EdiFtpSupportKlass.MakeDirectoryFtp(drAllConnections[0]["EdiFolder"].ToString(), drEdiSettings["FtpUser"].ToString(), drEdiSettings["FtpPassword"].ToString());
                        if (ErrorMessage == "")
                        {
                            //Kopierar meddelande till kundens "EdiFolder mapp" på Ftp servern
                            ErrorMessage = EdiFtpSupportKlass.UploadFileToFtpValues(drAllConnections[0]["EdiFolder"].ToString(), drEdiSettings["FtpUser"].ToString(), drEdiSettings["FtpPassword"].ToString(), drEdiSettings["MsgTempFolder"].ToString(), ToFile);
                            if (ErrorMessage != "")
                            {
                                //Nytt försök
                                ErrorMessage = EdiFtpSupportKlass.UploadFileToFtpValues(drAllConnections[0]["EdiFolder"].ToString(), drEdiSettings["FtpUser"].ToString(), drEdiSettings["FtpPassword"].ToString(), drEdiSettings["MsgTempFolder"].ToString(), ToFile);
                                if (ErrorMessage != "")
                                {
                                    ErrorMessage = "Det gick inte att kopiera meddelande " + FromFile + " till Ftp-mappen: " + drAllConnections[0]["EdiFolder"].ToString();
                                    FelaktigtMeddelande(FromFile, drMain);
                                    return MoveToCustomersResult.UnkownError;
                                }
                            }
                        }
                        else
                        {
                            ErrorMessage = "Det gick inte att skapa Ftp-mappen: " + drAllConnections[0]["EdiFolder"].ToString();
                            FelaktigtMeddelande(FromFile, drMain);
                            return MoveToCustomersResult.UnkownError;
                        }
                    }
                    else
                    {
                        // Se om filen redan finns på ftp:n  (detta borde ej behövas då den namnges efter klockslag)
                        bool fileExists = EdiFtpSupportKlass.CheckIfFileExists(drAllConnections[0]["EdiFolder"].ToString(), drEdiSettings["FtpUser"].ToString(), drEdiSettings["FtpPassword"].ToString(), ToFile);

                        if (fileExists)
                        {
                            if (ErrorMessage == "")
                            {
                                ErrorMessage = "Meddelandet fanns redan på FTP-servern. Meddelande " + FromFile + " till ftp-mappen: " + drAllConnections[0]["EdiFolder"].ToString();
                                FelaktigtMeddelande(FromFile, drMain);
                                return MoveToCustomersResult.UnkownError;
                            }
                        }
                        else
                        {
                            // Kontroll om filen finns
                            ErrorMessage = "Det gick inte att kopiera meddelande " + FromFile + " till Ftp-mappen: " + drAllConnections[0]["EdiFolder"].ToString();
                            FelaktigtMeddelande(FromFile, drMain);
                            return MoveToCustomersResult.UnkownError;
                        }
                    }
                }


                //Kopiera bildfiler till Ftp-server
                if (dsMeddelandeFil.Tables["MessageInfo"].Columns["MessageImageFileName"] != null &&
                    dsMeddelandeFil.Tables["MessageInfo"].Rows[0]["MessageImageFileName"].ToString() != "")
                    KopieraBildfilerTillFtp(drAllConnections[0], dsMeddelandeFil.Tables["MessageInfo"].Rows[0]["MessageImageFileName"].ToString());


                //Skapar rader i tabell för överförda edi-meddelande per kund och typ
                if (Convert.ToBoolean(drEdiSettings["ReceivedMessagesFunction"]) == true)
                {
                    fråga = "CustomerFtpUser = '" + drAllConnections[0]["CustFtpUser"].ToString() + "' And FtpEdiFolder = '" + drAllConnections[0]["EdiFolder"].ToString() + "'";
                    DataRow[] drMeddelande = dsMeddelande.Tables["Message"].Select(fråga);
                    if (drMeddelande.Length == 0)
                    {
                        DataRow dr = dsMeddelande.Tables["Message"].NewRow();
                        dsMeddelande.Tables["Message"].Rows.Add(dr);
                        dr["CustomerFtpUser"] = drAllConnections[0]["CustFtpUser"].ToString();
                        dr["CustomerName"] = drAllConnections[0]["CustName"].ToString();
                        dr["FtpEdiFolder"] = drAllConnections[0]["EdiFolder"].ToString();
                    }
                }
                #endregion
            }

            int antalFiler = 0;
            int.TryParse(drAllConnections[0]["AntalFilerKund"].ToString(), out antalFiler);
            drAllConnections[0]["AntalFilerKund"] = antalFiler + 1;
            drMain["State"] = (int)EdiTransferState.Transferred;
            drLoggInfo["AntalMeddelandeSkrivna"] = Convert.ToInt32(drLoggInfo["AntalMeddelandeSkrivna"]) + 1;
            dsMain.Tables["SysEdiReceived"].Rows[0]["ReceivedMsgCreated"] = Convert.ToInt32(dsMain.Tables["SysEdiReceived"].Rows[0]["ReceivedMsgCreated"]) + 1;

            //if (Convert.ToBoolean(drAllConnections[0]["Debiting"]) == true)
            //{
            //    foreach (DataRow rowCustomer in dsMain.Tables["Company"].Rows)
            //    {
            //        if (Convert.ToInt32(rowCustomer["ActorCompanyId"]) == Convert.ToInt32(drAllConnections[0]["ActorCompanyId"]))
            //        {
            //            rowCustomer["CustMsgUsed"] = (int)rowCustomer["CustMsgUsed"] + 1;
            //            break;
            //        }
            //    }
            //}

            //Korrekta meddelande lagras i "SaveFolder mappen"
            ToFile = @drEdiSettings["MsgSaveFolder"].ToString() + "\\" + ToFile;
            try
            { System.IO.File.Copy(FromFile, ToFile, true); }
            catch
            {
                MailSubject = "[9] Fel vid kopiering av meddelande";
                MailMessage = "Från meddelande: " + FromFile + "   Till meddelande: " + ToFile;
                EdiDiverseKlass.SkickaMailInternt(drEdiSettings["EmailAddress"].ToString(), MailSubject, MailMessage);
                EdiDiverseKlass.LagraEdiLog(Connection, dsMain, MailSubject, MailMessage);
                Console.Error.WriteLine(MailMessage);
                return MoveToCustomersResult.UnkownError;
            }

            return MoveToCustomersResult.Success;
        }

        private void KopieraBildfilerTillFtp(DataRow drAllConnections, string ImageFileName)
        {
            ErrorMessage = "";

            drLoggInfo["AntalBildfilLästa"] = Convert.ToInt32(drLoggInfo["AntalBildfilLästa"]) + 1;

            //Kontroll att kundens "EdiFolderImage mapp" finns angiven och innehåller "ftp://"
            if (drAllConnections["EdiFolderImage"].ToString() == "" | Convert.ToBoolean(drAllConnections["EdiFolderImage"].ToString().ToLower().Contains("ftp://")) != true)
            {
                ErrorMessage = "Mapp för bildfil för kunden finns inte angiven eller är felaktigt angiven för \r\n";
            }
            else
                if (!File.Exists(drEdiSettings["ImageTempFolder"].ToString() + "\\" + ImageFileName))
                {
                    ErrorMessage = "Angiven Bildfil i meddelandet finns inte i 'ImageTempFolder - mappen' \r\n";
                }
                else
                {
                    //Kopierar bildfil till kundens "EdiFolderImage mapp" på Ftp servern
                    ErrorMessage = EdiFtpSupportKlass.UploadFileToFtpValues(drAllConnections["EdiFolderImage"].ToString(), drEdiSettings["FtpUser"].ToString(), drEdiSettings["FtpPassword"].ToString(), drEdiSettings["ImageTempFolder"].ToString(), ImageFileName);
                    if (ErrorMessage != "")
                    {
                        //Kontroll om kundens "EdiFolderImage mapp" finns upplagd på Ftp-servern
                        ErrorMessage = EdiFtpSupportKlass.ListDirectoryFtpValue(drAllConnections["EdiFolderImage"].ToString(), drEdiSettings["FtpUser"].ToString(), drEdiSettings["FtpPassword"].ToString());
                        if (ErrorMessage != "")
                        {
                            //Skapar kundens "EdiFolderImage mapp" på Ftp servern om den inte finns upplagd
                            ErrorMessage = EdiFtpSupportKlass.MakeDirectoryFtp(drAllConnections["EdiFolderImage"].ToString(), drEdiSettings["FtpUser"].ToString(), drEdiSettings["FtpPassword"].ToString());
                            if (ErrorMessage == "")
                            {
                                ErrorMessage = EdiFtpSupportKlass.UploadFileToFtpValues(drAllConnections["EdiFolderImage"].ToString(), drEdiSettings["FtpUser"].ToString(), drEdiSettings["FtpPassword"].ToString(), drEdiSettings["ImageTempFolder"].ToString(), ImageFileName);
                                if (ErrorMessage != "")
                                {
                                    ErrorMessage = "Det gick inte att kopiera meddelande " + ImageFileName + " till Ftp-mappen: " + drAllConnections["EdiFolderImage"].ToString() + "\r\n";
                                }
                            }
                            else
                            {
                                ErrorMessage = "Det gick inte att skapa Ftp-mappen: " + drAllConnections["EdiFolderImage"].ToString() + "\r\n";
                            }
                        }
                        else
                        {
                            ErrorMessage = "Det gick inte att kopiera meddelande " + ImageFileName + " till Ftp-mappen: " + drAllConnections["EdiFolderImage"].ToString() + "\r\n";
                        }
                    }
                }

            if (ErrorMessage == "")
            {
                //Korrekta bildfiler lagras i "ImageSaveFolder mappen"
                string FromFile = drEdiSettings["ImageTempFolder"].ToString() + "\\" + ImageFileName;
                string ToFile = drEdiSettings["ImageSaveFolder"].ToString() + "\\" + ImageFileName;
                try
                { System.IO.File.Copy(FromFile, ToFile, true); }
                catch
                {
                    MailSubject = "[12] Fel vid kopiering av meddelande";
                    MailMessage = "Från meddelande: " + FromFile + "   Till meddelande: " + ToFile;
                    EdiDiverseKlass.SkickaMailInternt(drEdiSettings["EmailAddress"].ToString(), MailSubject, MailMessage);
                    EdiDiverseKlass.LagraEdiLog(Connection, dsMain, MailSubject, MailMessage);
                }
                drLoggInfo["AntalBildfilSkrivna"] = Convert.ToInt32(drLoggInfo["AntalBildfilSkrivna"]) + 1;
            }
            else
            {
                //Felaktiga meddelande lagras i "ImageErrorFolder mappen"
                string FromFile = drEdiSettings["ImageTempFolder"].ToString() + "\\" + ImageFileName;
                string ToFile = drEdiSettings["ImageErrorFolder"].ToString() + "\\" + ImageFileName;
                try
                { System.IO.File.Copy(FromFile, ToFile, true); }
                catch
                {
                    MailSubject = "[13] Fel vid kopiering av meddelande";
                    MailMessage = "Från meddelande: " + FromFile + "   Till meddelande: " + ToFile;
                    EdiDiverseKlass.SkickaMailInternt(drEdiSettings["EmailAddress"].ToString(), MailSubject, MailMessage);
                    EdiDiverseKlass.LagraEdiLog(Connection, dsMain, MailSubject, MailMessage);
                    Console.Error.WriteLine(MailMessage);
                }
                drLoggInfo["AntalBildfilAvvisade"] = Convert.ToInt32(drLoggInfo["AntalBildfilAvvisade"]) + 1;
                MailSubject = "[14] Edi felmeddelande";
                MailMessage = ErrorMessage +
                    "Kund " + drAllConnections["OrgNr"] + " " + drAllConnections["CustName"] + "\r\n" +
                    "Avsändare " + drAllConnections["SenderId"] + " " + drAllConnections["SenderName"] + "\r\n" +
                    "Avsändare " + drAllConnections["SenderId"] + " " + drAllConnections["SenderName"] + "\r\n" +
                    "Meddelande " + drAllConnections["TypeName"] + " " + ImageFileName;
                EdiDiverseKlass.SkickaMailInternt(drEdiSettings["EmailAddress"].ToString(), MailSubject, MailMessage);
                EdiDiverseKlass.LagraEdiLog(Connection, dsMain, MailSubject, MailMessage);
            }

            //Borttag av bildfil från "ImageTempFolder mappen"
            try
            { System.IO.File.Delete(drEdiSettings["ImageTempFolder"].ToString() + "\\" + ImageFileName); }
            catch
            {
                MailSubject = "[15] Fel vid borttag av meddelande";
                MailMessage = "Meddelandefilen '" + drEdiSettings["ImageTempFolder"].ToString() + "\\" + ImageFileName;
                EdiDiverseKlass.SkickaMailInternt(drEdiSettings["EmailAddress"].ToString(), MailSubject, MailMessage);
                EdiDiverseKlass.LagraEdiLog(Connection, dsMain, MailSubject, MailMessage);
            }

        }

        private string FelaktigtMeddelande(string FromFile, DataRow dataRowEdiTransfer = null)
        {
            //Felaktiga meddelande lagras i "ErrorFolder mappen"
            string ToFile = FromFile.Replace(@drEdiSettings["MsgTempFolder"].ToString() + "\\", "");
            ToFile = @drEdiSettings["MsgErrorFolder"].ToString() + "\\" + ToFile;
            try
            {
                System.IO.File.Copy(FromFile, ToFile, true);
            }
            catch
            {
                MailSubject = "[10] Fel vid kopiering av meddelande";
                MailMessage = "Från meddelande: " + FromFile + "   Till meddelande: " + ToFile;
                EdiDiverseKlass.SkickaMailInternt(drEdiSettings["EmailAddress"].ToString(), MailSubject, MailMessage);
                EdiDiverseKlass.LagraEdiLog(Connection, dsMain, MailSubject, MailMessage);
            }
            drLoggInfo["AntalMeddelandeAvvisade"] = Convert.ToInt32(drLoggInfo["AntalMeddelandeAvvisade"]) + 1;
            dsMain.Tables["SysEdiReceived"].Rows[0]["ReceivedIncorrect"] = Convert.ToInt32(dsMain.Tables["SysEdiReceived"].Rows[0]["ReceivedIncorrect"]) + 1;
            MailSubject = "[11] Edi felmeddelande";
            MailMessage = MailMessage + " \r\n" + "Felaktig Avsändare, Typ eller Kund i Meddelande-filen '" + ToFile + "'";
            if (dataRowEdiTransfer != null)
                dataRowEdiTransfer["State"] = (int)EdiTransferState.EdiCompanyNotFound;

            return ToFile;
        }

        #endregion
    }
}
