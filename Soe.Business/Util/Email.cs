using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Mail;

namespace SoftOne.Soe.Business.Util
{
    public class Email
    {
        readonly MailMessage mail;
        readonly SmtpClient smtp;

        public Email(string from, string to, string subject, string body, bool contentIsHTML, bool isTest, params string[] cc)
        {
            if (contentIsHTML)
                body = body.ToHTML();

            if (isTest)
            {
                subject = "XETEST: mottagare: " + to + subject;
                to = to.ToLower().Contains("@softone.") ? to : "xeteamet@softone.se";
                cc = null;
            }

            mail = new MailMessage(from, to, subject, body);
            if (cc != null && cc.Length > 0)
                mail.CC.Add(cc.JoinToString(","));

            mail.IsBodyHtml = contentIsHTML;

            #region smtp

            try
            {
                var smtpHost = "smtpout.dnsdrift.net";
                var smtpPort = "25";
                smtp = new SmtpClient(smtpHost, Convert.ToInt32(smtpPort));
                smtp.EnableSsl = false;
            }
            catch
            {
                smtp = new SmtpClient();
            }

            #endregion
        }

        public Email(string from, string to, string subject, string body, bool contentIsHTML, bool convert, bool isTest,params string[] cc)
        {
            if (contentIsHTML && convert)
                body = body.ToHTML();


            if (isTest)
            {
                subject = "XETEST: mottagare: " + to + subject;
                to = to.ToLower().Contains("@softone.") ? to : "xeteamet@softone.se";
                cc = null;
            }

            mail = new MailMessage(from, to, subject, body);
            if (cc != null && cc.Length > 0)
                mail.CC.Add(cc.JoinToString(","));

            mail.IsBodyHtml = contentIsHTML;

            #region smtp

            try
            {
                var smtpHost = "smtpout.dnsdrift.net";
                var smtpPort = "25";
                smtp = new SmtpClient(smtpHost, Convert.ToInt32(smtpPort));
                smtp.EnableSsl = false;
            }
            catch
            {
                smtp = new SmtpClient();
            }

            #endregion
        }

        public void Attach(List<byte[]> attachements, string filename)
        {
            foreach (var attachement in attachements)
            {
                if (attachement != null)
                {
                    Stream stream = new MemoryStream(attachement);
                    mail.Attachments.Add(new Attachment(stream, filename));
                    /*using (Stream stream = new MemoryStream(attachement))
                    {
                        mail.Attachments.Add(new Attachment(stream, filename));
                    }*/
                }
            }
        }

        public void Attach(byte[] attachement, string filename)
        {
            if (attachement != null)
            {
                Stream stream = new MemoryStream(attachement);
                mail.Attachments.Add(new Attachment(stream, filename));
                /*using (Stream stream = new MemoryStream(attachement))
                {
                    mail.Attachments.Add(new Attachment(stream, filename));
                }*/
            }
        }

        public void Attach(List<byte[]> attachements, System.Net.Mime.ContentType contentType)
        {
            foreach (var attachement in attachements)
            {
                if (attachement != null)
                {
                    Stream stream = new MemoryStream(attachement);
                    Attachment attachment = new Attachment(stream, contentType);
                    /*using (Stream stream = new MemoryStream(attachement))
                    {
                        Attachment attachment = new Attachment(stream, contentType);
                    }*/
                }
            }
        }

        public void GetAttachementsFromDisk(List<string> paths, System.Net.Mime.ContentType contentType)
        {
            List<byte[]> attachements = new List<byte[]>();
            foreach (var path in paths)
            {
                using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    var bytes = new byte[stream.Length];
                    stream.Read(bytes, 0, bytes.Length);
                    attachements.Add(bytes);
                }
            }

            Attach(attachements, contentType);
        }

        public void Send()
        {
            smtp.Send(mail);
        }
    }
}
