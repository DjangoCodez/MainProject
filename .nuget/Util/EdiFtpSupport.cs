using System;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Core;
using System.Data.SqlClient;
using System.IO;
using System.Net;

namespace SoftOne.EdiAdmin.Business.Util
{
    public class EdiFtpSupport
    {
        private EdiDatabasMetoder EdiDatabasMetoderKlass = new EdiDatabasMetoder();

        private string ErrorMessage;

        public EdiFtpSupport()
        {
        }

        //Kopiera över filer från adress på FTP-server
        //Returvärde - Felmeddelande
        //Referenser - Mapp på Ftp-server, Användare, Lösenord, Filnamn (om blankt hämtas samtliga filer),
        //             Mapp dit filen/filerna skall kopieras, Borttag efter att den hämtats (true/false)
        public string DownloadFromFtpValues(string FtpAdress, string Anv, string Losen, string Fil, string TillMapp, bool Borttag, out int AntalFiler)
        //"FtpAdress" = mapp på Ftp-server
        //"Anv" = Användarnamn för inloggning
        //"Losen" = Lösenord för inloggning
        //"Fil" = Filnamn om enskild fil skall hämtas. Om blankt hämtas samtliga filer
        //"TillMapp" = Mapp dit filen/filerna skall hämtas
        //"Borttag" = anger om filen skall tas bort efter att den hämtats - true/false
        {

            StreamReader reader = null;
            bool error = false;
            AntalFiler = 0;
            string ReturnMessage = "";

            if (Fil.ToString() == "")
            {
                try
                {
                    ErrorMessage = "";
                    DataSet dsListDir = ListDirectoryDetails(FtpAdress, Anv, Losen);
                    if (ErrorMessage != "") ReturnMessage = ErrorMessage + "\r\n";
                    foreach (DataRow row in dsListDir.Tables["VisaFiler"].Rows)
                    {
                        ErrorMessage = "";
                        error = GetFileFromFtpValues(FtpAdress, Anv, Losen, row["Namn"].ToString(), TillMapp, Borttag);
                        if (error == false) AntalFiler++;
                        if (ErrorMessage != "") ReturnMessage = ReturnMessage + ErrorMessage + "\r\n";
                    }
                }
                catch (UriFormatException errorMessages)
                {
                    ReturnMessage = ReturnMessage + errorMessages.Message.ToString() + "\r\n";
                }
                catch (WebException errorMessages)
                {
                    ReturnMessage = ReturnMessage + errorMessages.Message.ToString() + "\r\n";
                }
                finally
                {
                    if (reader != null) reader.Close();
                }
            }
            else
            {
                ErrorMessage = "";
                error = GetFileFromFtpValues(FtpAdress, Anv, Losen, Fil, TillMapp, Borttag);
                if (error == false) AntalFiler++;
                if (ErrorMessage != "") ReturnMessage = ErrorMessage + "\r\n";
            }

            return ReturnMessage;

        }
        //Kopiera över filer till adress på FTP-server
        //Returvärde - Felmeddelande
        //Referenser - Mapp på Ftp-server, Användare, Lösenord, Mapp varifrån filen skall kopieras, Filnamn för fil som skall kopieras
        public string UploadFileToFtpValues(string FtpAdress, string Anv, string Losen, string Mapp, string Fil, bool FtpAdressIncludesFile = false)
        //"FtpAdress" = mapp på Ftp-server
        //"Anv" = Användarnamn för inloggning
        //"Losen" = Lösenord för inloggning
        //"Mapp" = Mapp varifrån filen skall lämnas
        //"Fil" = Filnamn för fil som skall lämnas
        {

            FileInfo fileInf = new FileInfo(Mapp + "\\" + Fil);
            
            string uri = FtpAdressIncludesFile ? FtpAdress : FtpAdress + "/" + fileInf.Name;
            ErrorMessage = "";
            if (System.IO.File.Exists(@fileInf.ToString()) == false)
            {
                ErrorMessage = "Fil saknas: " + fileInf;
                return ErrorMessage;
            }

            FtpWebRequest FtpRequest = (FtpWebRequest)WebRequest.Create(FtpAdressIncludesFile ? FtpAdress : FtpAdress + "/" + Fil);
            FtpRequest.Method = WebRequestMethods.Ftp.UploadFile;
            FtpRequest.Credentials = new NetworkCredential(Anv.ToString(), Losen.ToString());

            FtpRequest.UseBinary = true;
            FtpRequest.ContentLength = fileInf.Length;

            int buffLength = 1024;
            byte[] buff = new byte[1024];
            int contentLen;

            using (FileStream fs = fileInf.OpenRead())
            {
            try
            {
                    using (Stream strm = FtpRequest.GetRequestStream())
                    {
                contentLen = fs.Read(buff, 0, buffLength);
                while (contentLen != 0)
                {
                    strm.Write(buff, 0, contentLen);
                    contentLen = fs.Read(buff, 0, buffLength);
                }
            }
                }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message.ToString();
                string msg = ex.Message;
                while (ex.InnerException != null)
                {
                    msg += " Inner exception: " + ex.InnerException.Message;
                    ex = ex.InnerException;
                }

                Console.Error.WriteLine(msg);
            }
            }

            return ErrorMessage;
        }

        //Skapa mapp på Ftp-server
        //Returvärde - Felmeddelande
        //Referenser - Mapp på Ftp-server, Användare, Lösenord
        public string MakeDirectoryFtp(string FtpAdress, string Anv, string Losen)
        //"FtpAdress" = mapp som skall skapas på Ftp-server
        //"Anv" = Användarnamn för inloggning
        //"Losen" = Lösenord för inloggning
        {

            ErrorMessage = "";

            //Kontroll om överordnade mappar finns. Om inte så skapas dom upp
            try
            {
                for (int i = 0; i < FtpAdress.Length; i++)
                {
                    if (FtpAdress.Substring(i, 1) == "/")
                    {
                        if (FtpAdress.Substring(i, 2) == "//")
                        {
                            i++;
                            continue;
                        }
                        ErrorMessage = ListDirectoryFtpValue(FtpAdress.Substring(0, i), Anv, Losen);
                        if (ErrorMessage != "")
                        {
                            ErrorMessage = MakeDirectory(FtpAdress.Substring(0, i), Anv, Losen);
                            if (ErrorMessage != "") return ErrorMessage;
                        }
                    }
                }
            }
            catch (Exception ex)
            { ErrorMessage = ex.Message.ToString(); }

            ErrorMessage = ListDirectoryFtpValue(FtpAdress, Anv, Losen);
            if (ErrorMessage != "")
            {
                ErrorMessage = MakeDirectory(FtpAdress, Anv, Losen);
                if (ErrorMessage != "") return ErrorMessage;
            }
            return ErrorMessage;

        }

        public List<string> ListDirectoriesOnFtp(string FtpAdress, string Anv, string Losen)
        {
            return this.ListFilesAndDirectoriesOnFtp(FtpAdress, Anv, Losen).Where(f => f.IsDirectory == true).Select(f => f.Name).ToList();
        }

        public IEnumerable<FtpFileInfo> ListFilesAndDirectoriesOnFtp(string FtpAdress, string Anv, string Losen)
        {
            string Meddelande = "";
            var returnList = new List<FtpFileInfo>();
            try
            {
                FtpWebRequest FTPRequest = (FtpWebRequest)WebRequest.Create(FtpAdress.ToString());
                FTPRequest.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
                FTPRequest.Credentials = new NetworkCredential(Anv.ToString(), Losen.ToString());
                using (FtpWebResponse FTPResponse = (FtpWebResponse)FTPRequest.GetResponse())
                {
                    using (Stream responseStream = FTPResponse.GetResponseStream())
                    {
                        using (var reader = new StreamReader(FTPResponse.GetResponseStream()))
                        {
                            while (reader.EndOfStream == false)
                            {
                                string fil = reader.ReadLine();

                                string[] c = { " ", "\t" };
                                string[] columns = fil.Split(c, 9, StringSplitOptions.RemoveEmptyEntries);


                                string name = columns[8];
                                string date = columns[5] + columns[6];
                                string timeOrYear = columns[7];
                                DateTime dateTime; // = DateTime.ParseExact(date + timeOrYear, new []{ "MMMddHH:mm", "MMMddYYYY" },  System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat);
                                var formats = new[] { "MMMddHH:mm", "MMMddyyyy", "MMMMddHH:mm", "MMMddyyyy", "MMMdd" };
                                DateTime.TryParseExact(date + timeOrYear, formats, System.Globalization.CultureInfo.CurrentCulture, System.Globalization.DateTimeStyles.AssumeLocal, out dateTime);

                                returnList.Add(new FtpFileInfo() { Name = name.Trim(), IsDirectory = fil.StartsWith("d"), Modified = dateTime });
                            }
                        }
                    }
                }
            }
            catch (UriFormatException errorMessages)
            { Meddelande = errorMessages.ToString(); }
            catch (WebException errorMessages)
            { Meddelande = errorMessages.ToString(); }
            catch (IOException errorMessages)
            { Meddelande = errorMessages.ToString(); }
            catch
            { Meddelande = "Fel"; }
            finally
            {
            }

            return returnList;
        }

        //Visa filer på Ftp-server
        //Returvärde - Felmeddelande
        //Referenser - Mapp på Ftp-server, Användare, Lösenord
        public string ListDirectoryFtpValue(string FtpAdress, string Anv, string Losen)
        {
            string Meddelande = "";
            StreamReader reader = null;

            try
            {
                FtpWebRequest FTPRequest = (FtpWebRequest)WebRequest.Create(FtpAdress.ToString());
                FTPRequest.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
                FTPRequest.Credentials = new NetworkCredential(Anv.ToString(), Losen.ToString());
                FtpWebResponse FTPResponse = (FtpWebResponse)FTPRequest.GetResponse();
                Stream responseStream = FTPResponse.GetResponseStream();
                reader = new StreamReader(FTPResponse.GetResponseStream());
                while (reader.EndOfStream == false)
                {
                    string fil = reader.ReadLine();
                    if (Convert.ToBoolean(fil.Contains("<DIR>")) == true) continue;
                }
            }
            catch (UriFormatException errorMessages)
            { Meddelande = errorMessages.ToString(); }
            catch (WebException errorMessages)
            { Meddelande = errorMessages.ToString(); }
            catch (IOException errorMessages)
            { Meddelande = errorMessages.ToString(); }
            catch
            { Meddelande = "Fel"; }
            finally
            {
                try
                {
                    if (reader != null) reader.Close();
                }
                catch
                { Meddelande = "Fel"; }
            }

            return Meddelande;

        }

        //Visa filer i mapp på FTP-server
        //Returvärde - DataSet med tabellen "VisaFiler"
        //Referenser - Ftp-adress, Användare, Lösenord
        private DataSet ListDirectoryDetails(string FtpAdress, string Anv, string Losen)
        {
            DataSet dsVisaFiler = new DataSet();
            DataRow drVisaFiler;
            dsVisaFiler.Tables.Add("VisaFiler");
            dsVisaFiler.Tables["VisaFiler"].Columns.Add("Namn");
            dsVisaFiler.Tables["VisaFiler"].Columns.Add("Datum");
            dsVisaFiler.Tables["VisaFiler"].Columns.Add("Tid");
            dsVisaFiler.Tables["VisaFiler"].Columns.Add("Storlek", typeof(int));

            StreamReader readerLDD = null;

            try
            {
                FtpWebRequest listRequestLDD = (FtpWebRequest)WebRequest.Create(FtpAdress.ToString());
                listRequestLDD.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
                listRequestLDD.Credentials = new NetworkCredential(Anv.ToString(), Losen.ToString());
                FtpWebResponse listResponseLDD = (FtpWebResponse)listRequestLDD.GetResponse();
                Stream responseStreamLDD = listResponseLDD.GetResponseStream();
                readerLDD = new StreamReader(listResponseLDD.GetResponseStream());

                while (readerLDD.EndOfStream == false)
                {
                    string fil = readerLDD.ReadLine();
                    if (fil.Length <= 12) continue;
                    if (fil.Substring(12, 1) == ":")
                    {
                        if (Convert.ToBoolean(fil.Contains("<DIR>")) == true) continue;
                        drVisaFiler = dsVisaFiler.Tables["VisaFiler"].NewRow();
                        dsVisaFiler.Tables["VisaFiler"].Rows.Add(drVisaFiler);
                        drVisaFiler["Namn"] = fil.Substring(39);
                        string datum = fil.Substring(6, 2) + "-" + fil.Substring(0, 5) + fil.Substring(8, 9);
                        drVisaFiler["Datum"] = Convert.ToDateTime(datum).ToString().Substring(0, 10);
                        drVisaFiler["Tid"] = Convert.ToDateTime(datum).ToString().Substring(11, 5);
                        drVisaFiler["Storlek"] = Convert.ToInt32(fil.Substring(28, 10));
                    }
                    else
                    {
                        if (fil.Substring(0, 1) == "d") continue;
                        drVisaFiler = dsVisaFiler.Tables["VisaFiler"].NewRow();
                        dsVisaFiler.Tables["VisaFiler"].Rows.Add(drVisaFiler);
                        string år = "";
                        string månad = "";
                        string dag = "";
                        int från = 0;
                        int tkn = 0;
                        int antal = 1;
                        for (int i = 0; i < fil.Length - 1; i++)
                        {
                            if (fil.Substring(i, 1) == " ")
                            {
                                if (antal == 5)
                                    drVisaFiler["Storlek"] = Convert.ToInt32(fil.Substring(från, tkn));
                                else
                                    if (antal == 6)
                                        månad = fil.Substring(från, tkn);
                                    else
                                        if (antal == 7)
                                            dag = fil.Substring(från, tkn);
                                        else
                                            if (antal == 8)
                                            {
                                                if ((bool)fil.Substring(från, tkn).Contains(":") == true)
                                                {
                                                    drVisaFiler["Tid"] = fil.Substring(från, tkn);
                                                    år = DateTime.Now.ToString().Substring(0, 4);
                                                }
                                                else
                                                {
                                                    drVisaFiler["Tid"] = "00:00:00";
                                                    år = fil.Substring(från, tkn);
                                                }
                                            }

                                tkn = 0;
                                antal++;
                                for (int j = i; j < fil.Length - 1; j++)
                                {
                                    if (fil.Substring(j, 1) != " ")
                                    {
                                        i = j - 1;
                                        från = j;
                                        break;
                                    }
                                }
                                if (antal == 9) break;
                            }
                            else
                            {
                                tkn++;
                            }
                        }
                        drVisaFiler["Namn"] = fil.Substring(från);
                        if (månad == "Jan") månad = "01";
                        else
                            if (månad == "Feb") månad = "02";
                            else
                                if (månad == "Mar") månad = "03";
                                else
                                    if (månad == "Apr") månad = "04";
                                    else
                                        if (månad == "May" | månad == "Maj") månad = "05";
                                        else
                                            if (månad == "Jun") månad = "06";
                                            else
                                                if (månad == "Jul") månad = "07";
                                                else
                                                    if (månad == "Aug") månad = "08";
                                                    else
                                                        if (månad == "Sep") månad = "09";
                                                        else
                                                            if (månad == "Oct" | månad == "Okt") månad = "10";
                                                            else
                                                                if (månad == "Nov") månad = "11";
                                                                else
                                                                    if (månad == "Dec") månad = "12";
                        if (dag.Length == 1) dag = "0" + dag;
                        drVisaFiler["Datum"] = år + "-" + månad + "-" + dag;
                    }
                }

            }
            catch (UriFormatException errorMessages)
            {
                ErrorMessage = errorMessages.Message.ToString();
            }
            catch (WebException errorMessages)
            {
                ErrorMessage = errorMessages.Message.ToString();
            }
            finally
            {
                if (readerLDD != null) readerLDD.Close();
            }

            return dsVisaFiler;

        }

        //Privat metod för att kopiera över fil från FTP-server utifrån rad i tabellen "FtpHantering"
        //Returvärde - Fel = true/false
        //Referenser - Connection-sträng, Databas, DataSet FtpHantering för tabellen "FtpLogg", 
        //             Aktuell rad för kopiering i tabellen "FtpHantering", Filnamn på Ftp-server, Användare
        private bool GetFileFromFtp(string SOB_Connection, string SOB_FtgDatabas, DataSet dsFtpHantering, DataRow row, string fil, string Användare)
        {

            Stream responseStream = null;
            FileStream fileStream = null;
            StreamReader reader = null;
            string fileName = "";
            bool error = false;
            string FelText = "Fel avser fil: " + row["FtpAdress"].ToString() + "/" + fil + "\r\n" + "Felmeddelande: ";

            try
            {
                FtpWebRequest downloadRequest = (FtpWebRequest)WebRequest.Create(row["FtpAdress"].ToString() + "/" + fil);
                downloadRequest.Method = WebRequestMethods.Ftp.DownloadFile;
                downloadRequest.Credentials = new NetworkCredential(row["Anv"].ToString(), row["Losen"].ToString());
                FtpWebResponse downloadResponse = (FtpWebResponse)downloadRequest.GetResponse();
                responseStream = downloadResponse.GetResponseStream();

                if (row["FtpFil"].ToString() == "")
                    fileName = row["LokalMapp"] + "\\" + Path.GetFileName(downloadRequest.RequestUri.LocalPath);
                else
                    if (row["LokalFil"].ToString() == "")
                        fileName = row["LokalMapp"] + "\\" + Path.GetFileName(downloadRequest.RequestUri.LocalPath);
                    else
                        fileName = row["LokalMapp"].ToString() + "\\" + row["LokalFil"].ToString();

                fileName = fileName.Replace("%20", " ");
                fileName = fileName.Replace("%7B", "{");
                fileName = fileName.Replace("%7D", "}");

                if (fileName.Length == 0)
                {
                    reader = new StreamReader(responseStream);
                }
                else
                {
                    fileStream = File.Create(fileName);
                    byte[] buffer = new byte[1024];
                    int bytesRead;
                    int storlek = 0;
                    while (true)
                    {
                        bytesRead = responseStream.Read(buffer, 0, buffer.Length);
                        if (bytesRead == 0)
                            break;
                        fileStream.Write(buffer, 0, bytesRead);
                        storlek = storlek + bytesRead;
                    }
                    DataRow drFtpHantering = dsFtpHantering.Tables["FtpLogg"].NewRow();
                    dsFtpHantering.Tables["FtpLogg"].Rows.Add(drFtpHantering);
                    drFtpHantering["IdHantering"] = Convert.ToInt32(row["IdHantering"]);
                    drFtpHantering["Fil"] = fileStream.Name.ToString();
                    drFtpHantering["Datum"] = DateTime.Now.Date.ToString().Substring(0, 10);
                    drFtpHantering["Tid"] = DateTime.Now.TimeOfDay.ToString().Substring(0, 8);
                    drFtpHantering["Storlek"] = storlek;
                    drFtpHantering["Borttag"] = row["Borttag"];
                    drFtpHantering["FtpFil"] = downloadResponse.ResponseUri.OriginalString.ToString();
                    drFtpHantering["Anv"] = Användare;
                    EdiDatabasMetoderKlass.UppdateraTabellDataSet(SOB_Connection, SOB_FtgDatabas, dsFtpHantering, "FtpLogg");
                }
            }
            catch (UriFormatException errorMessages)
            {
                ErrorMessage = ErrorMessage + FelText + errorMessages.Message.ToString() + "\r\n";
                error = true;
            }
            catch (WebException errorMessages)
            {
                ErrorMessage = ErrorMessage + FelText + errorMessages.Message.ToString() + "\r\n";
                error = true;
            }
            catch (IOException errorMessages)
            {
                ErrorMessage = ErrorMessage + FelText + errorMessages.Message.ToString() + "\r\n";
                error = true;
            }
            finally
            {
                if (reader != null) reader.Close();
                else
                    if (responseStream != null) responseStream.Close();
                if (fileStream != null) fileStream.Close();
            }

            if (error == false & Convert.ToBoolean(row["Borttag"]) == true)
            {
                try
                {
                    FtpWebRequest downloadRequest = (FtpWebRequest)WebRequest.Create(row["FtpAdress"].ToString() + "/" + fil);
                    downloadRequest.Method = WebRequestMethods.Ftp.DeleteFile;
                    downloadRequest.Credentials = new NetworkCredential(row["Anv"].ToString(), row["Losen"].ToString());
                    FtpWebResponse downloadResponse = (FtpWebResponse)downloadRequest.GetResponse();
                }
                catch (UriFormatException errorMessages)
                {
                    ErrorMessage = ErrorMessage + errorMessages.Message.ToString() + "\r\n";
                    error = true;
                }
                catch (WebException errorMessages)
                {
                    ErrorMessage = ErrorMessage + errorMessages.Message.ToString() + "\r\n";
                    error = true;
                }
                catch (IOException errorMessages)
                {
                    ErrorMessage = ErrorMessage + errorMessages.Message.ToString() + "\r\n";
                    error = true;
                }
                finally
                {
                    if (reader != null) reader.Close();
                    else
                        if (responseStream != null) responseStream.Close();
                    if (fileStream != null) fileStream.Close();
                }
            }

            return error;

        }


        //Privat metod för att kopiera över fil från FTP-server
        //Returvärde - Fel = true/false
        //Referenser - Mapp på Ftp-server, Användare, Lösenord, Filnamn (om blankt hämtas samtliga filer),
        //             Mapp dit filen/filerna skall kopieras, Borttag efter att den hämtats (true/false)
        private bool GetFileFromFtpValues(string FtpAdress, string Anv, string Losen, string Fil, string TillMapp, bool Borttag)
        {

            Stream responseStream = null;
            FileStream fileStream = null;
            StreamReader reader = null;
            string fileName = "";
            bool error = false;
            string FelHämta = "Fel vid hämtning av fil: " + FtpAdress.ToString() + "/" + Fil + "\r\n" + "Felmeddelande: ";
            string FelBorttag = "Fel vid borttag av fil: " + FtpAdress.ToString() + "/" + Fil + "\r\n" + "Felmeddelande: ";

            try
            {
                FtpWebRequest FtpRequest = (FtpWebRequest)WebRequest.Create(FtpAdress.ToString() + "/" + Fil);
                FtpRequest.Method = WebRequestMethods.Ftp.DownloadFile;
                FtpRequest.Credentials = new NetworkCredential(Anv.ToString(), Losen.ToString());
                FtpWebResponse FtpResponse = (FtpWebResponse)FtpRequest.GetResponse();
                responseStream = FtpResponse.GetResponseStream();

                fileName = TillMapp + "\\" + Path.GetFileName(FtpRequest.RequestUri.LocalPath);

                if (fileName.Length == 0)
                {
                    reader = new StreamReader(responseStream);
                }
                else
                {
                    fileStream = File.Create(fileName);
                    byte[] buffer = new byte[1024];
                    int bytesRead;
                    int storlek = 0;
                    while (true)
                    {
                        bytesRead = responseStream.Read(buffer, 0, buffer.Length);
                        if (bytesRead == 0)
                            break;
                        fileStream.Write(buffer, 0, bytesRead);
                        storlek = storlek + bytesRead;
                    }
                }
            }
            catch (UriFormatException errorMessages)
            {
                ErrorMessage = ErrorMessage + FelHämta + errorMessages.Message.ToString() + "\r\n";
                error = true;
            }
            catch (WebException errorMessages)
            {
                ErrorMessage = ErrorMessage + FelHämta + errorMessages.Message.ToString() + "\r\n";
                error = true;
            }
            catch (IOException errorMessages)
            {
                ErrorMessage = ErrorMessage + FelHämta + errorMessages.Message.ToString() + "\r\n";
                error = true;
            }
            finally
            {
                if (reader != null) reader.Close();
                else
                    if (responseStream != null) responseStream.Close();
                if (fileStream != null) fileStream.Close();
            }

            if (error == false & Borttag == true)
            {
                try
                {
                    FtpWebRequest downloadRequest = (FtpWebRequest)WebRequest.Create(FtpAdress.ToString() + "/" + Fil);
                    downloadRequest.Method = WebRequestMethods.Ftp.DeleteFile;
                    downloadRequest.Credentials = new NetworkCredential(Anv.ToString(), Losen.ToString());
                    FtpWebResponse downloadResponse = (FtpWebResponse)downloadRequest.GetResponse();
                }
                catch (UriFormatException errorMessages)
                {
                    ErrorMessage = ErrorMessage + FelBorttag + errorMessages.Message.ToString() + "\r\n";
                    error = true;
                }
                catch (WebException errorMessages)
                {
                    ErrorMessage = ErrorMessage + FelBorttag + errorMessages.Message.ToString() + "\r\n";
                    error = true;
                }
                catch (IOException errorMessages)
                {
                    ErrorMessage = ErrorMessage + FelBorttag + errorMessages.Message.ToString() + "\r\n";
                    error = true;
                }
                finally
                {
                    if (reader != null)
                    { reader.Close(); }
                    else
                        if (responseStream != null)
                        { responseStream.Close(); }
                    if (fileStream != null)
                    { fileStream.Close(); }
                }
            }

            return error;

        }

        //Privat metod för att skapa mapp på Ftp-server
        //Returvärde - Felmeddelande
        //Referenser - Mapp på Ftp-server, Användare, Lösenord
        private string MakeDirectory(string FtpAdress, string Anv, string Losen)
        {
            string Message = "";

            try
            {
                FtpWebRequest FtpRequest = (FtpWebRequest)FtpWebRequest.Create(FtpAdress);
                FtpRequest.Method = WebRequestMethods.Ftp.MakeDirectory;
                FtpRequest.Credentials = new NetworkCredential(Anv.ToString(), Losen.ToString());
                FtpWebResponse FtpResponse = (FtpWebResponse)FtpRequest.GetResponse();
            }
            catch (UriFormatException errorMessages)
            {
                Message = errorMessages.Message.ToString();
            }
            catch (WebException errorMessages)
            {
                Message = errorMessages.Message.ToString();
            }

            return Message;

        }


        internal bool CheckIfFileExists(string FtpAdress, string Anv, string Losen, string fileName)
        {
            string Meddelande = "";
            StreamReader reader = null;

            try
            {
                FtpWebRequest FTPRequest = (FtpWebRequest)WebRequest.Create(FtpAdress.ToString());
                FTPRequest.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
                FTPRequest.Credentials = new NetworkCredential(Anv.ToString(), Losen.ToString());
                FtpWebResponse FTPResponse = (FtpWebResponse)FTPRequest.GetResponse();
                Stream responseStream = FTPResponse.GetResponseStream();
                reader = new StreamReader(FTPResponse.GetResponseStream());
                while (reader.EndOfStream == false)
                {
                    string fil = reader.ReadLine();
                    string fileNameFTP = fil.Substring(fil.LastIndexOf(" ") + 1);
                    if (fileNameFTP.ToLower() == fileName.ToLower())
                        return true;
                }
            }
            catch (UriFormatException errorMessages)
            { Meddelande = errorMessages.ToString(); }
            catch (WebException errorMessages)
            { Meddelande = errorMessages.ToString(); }
            catch (IOException errorMessages)
            { Meddelande = errorMessages.ToString(); }
            catch
            { Meddelande = "Fel"; }
            finally
            {
                try
                {
                    if (reader != null) reader.Close();
                }
                catch
                { Meddelande = "Fel"; }
            }

            return false;
        }
    }

    public class FtpFileInfo
    {
        public string Name { get; set; }
        public bool IsDirectory { get; set; }

        public DateTime Modified { get; set; }
    }
}
