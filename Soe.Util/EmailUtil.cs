using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;

namespace SoftOne.Soe.Util
{
    public static class EmailUtil
    {
        public static string SendMailSMTP(string From, string To, string Subject, string Message, string[] Attachments, params string[] CC)
        {
            return SendMailSMTP(From, new string[]{ To }, Subject, Message, Attachments, CC);
        }

        //Returvärde - Felmeddelande
        //Referenser - Connection-sträng, Från mail-adress, Till mail-adresser, Ämne, Bifogade filer
        public static string SendMailSMTP(string From, string[] To, string Subject, string Message, string[] Attachments, params string[] CC)
        {
            string Server = "SMTP";
            string Meddelande = "";

            MailMessage message = new MailMessage()
            {
                Subject = Subject,
                Body = Message,
                From = new MailAddress(From),
            };

            string to = string.Join(",", To);
            message.To.Add(to);

            if (CC != null && CC.Length > 0)
            {
                string cc = string.Join(",", CC);
                message.CC.Add(cc);
            }

            SmtpClient client = new SmtpClient(Server);

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

            return Meddelande;

        }
    }
}
