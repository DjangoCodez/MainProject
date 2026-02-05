using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Util.Finvoice
{
    public class FinvoiceAttachmentItem : FinvoiceBase
    {
        public string RefToMessageIdentifier { get; set; }
        
        private readonly List<FinvoiceAttachmentDetails> attachmentDetails =  new List<FinvoiceAttachmentDetails>();
        public IReadOnlyList<FinvoiceAttachmentDetails> AttachmentDetails => attachmentDetails;

        static private readonly string[] AllowedFinvoiceAttachmentTypes = { ".pdf", ".jpeg", ".jpg", ".png" };

        #region Constructors

        /// <summary>
        /// Constructor for preparing for creation of finvoice (Export).
        /// Call ToXml to create the file.
        /// </summary>        
        public FinvoiceAttachmentItem(CustomerInvoice invoice, List<Tuple<int, string, byte[]>> attachments, string messageIdentifier)
        {
            Populate(invoice, attachments, messageIdentifier);
        }

        public FinvoiceAttachmentItem()
        {
            
        }

        #endregion

        public static ActionResult CheckForInvalidAttachments(ManagerBase parentManger, string invoiceNr, List<Tuple<int, string, byte[]>> attachments)
        {
            if (attachments.Count > 10)
            {
                return new ActionResult(parentManger.GetText(7523,1, "Max antal bilagor är 10"));
            }

            var totalLength = attachments.Select(i => i.Item3.Length).Sum();
            if (totalLength > 1 * 1024000)
                return new ActionResult(string.Format(parentManger.GetText(7728, 1, "Den totala storleken på bifogade filer i faktura {0} är för stor. Maximal totalstorlek för alla bilagor är 1 Mb."), invoiceNr));

            foreach (var item in attachments)
            {
                var fileName = item.Item2;
                var fileExt = Path.GetExtension(fileName).ToLower();
                if (!AllowedFinvoiceAttachmentTypes.Contains(fileExt))
                {
                    return new ActionResult(string.Format(parentManger.GetText(7524, 1, "Attached file {0} in the invoice {0} has wrong type. Permitted file types are jpeg, png and pdf (PDF/A)"), fileName, invoiceNr));
                }
            }

            return new ActionResult();
        }

        private void Populate(CustomerInvoice invoice, List<Tuple<int, string, byte[]>> attachments, string messageIdentifier)
        {
            foreach(var item in attachments)
            {
                string fileExt = Path.GetExtension(item.Item2)?.ToLower();
                string mime;
                switch (fileExt)
                {
                    case ".png":
                        mime = "image/png";
                        break;
                    case ".jpeg":
                    case ".jpg":
                        mime = "image/jpeg";
                        break;
                    case ".pdf":
                        mime = "application/pdf";
                        break;
                    default:
                        continue;
                }

                attachmentDetails.Add(new FinvoiceAttachmentDetails
                {
                    AttachmentMimeType = mime,
                    AttachmentName = item.Item2,
                    AttachmentContent = item.Item3,
                    AttachmentSecurityClass = "SEI01",
                    AttachmentIdentifier = messageIdentifier + "::attachments::"
                });
            }
        }

        public void ToXml(MessageTransmissionDetails messageTransmissionDetails, ref XDocument document)
        {
            XNamespace xsi = "http://www.w3.org/2001/XMLSchema-instance";
            var rootElement = new XElement("FinvoiceAttachments", new XAttribute(XNamespace.Xmlns + "xsi", xsi.NamespaceName), new XAttribute(xsi + "noNamespaceSchemaLocation", "FinvoiceAttachments.xsd"), new XAttribute("Version", "1.0"));
            messageTransmissionDetails.AddNode(ref rootElement, true);
            foreach(var detail in this.attachmentDetails)
            {
                detail.AddNode(ref rootElement);
            }

            document.Add(rootElement);
        }

        public ActionResult Parse(Stream content)
        {
            var result = new ActionResult(true);

            try
            {
                using (content)
                {
                    XDocument xmlDoc = XDocument.Load(content);

                    var refToMessageIdentifier = XmlUtil.GetDescendantElement(xmlDoc, "MessageTransmissionDetails", "RefToMessageIdentifier");
                    if (refToMessageIdentifier == null)
                    {
                        return new ActionResult("RefToMessageIdentifier was not found");
                    }

                    RefToMessageIdentifier = refToMessageIdentifier.Value;

                    var attachmentDetailsXML = XmlUtil.GetChildElements(xmlDoc, "AttachmentDetails");

                    foreach (var item in attachmentDetailsXML)
                    {
                        var attachmentDetail = new FinvoiceAttachmentDetails
                        {
                            AttachmentName = XmlUtil.GetChildElement(item, "AttachmentName")?.Value,
                            AttachmentIdentifier = XmlUtil.GetChildElement(item, "AttachmentIdentifier")?.Value,
                            AttachmentMimeType = XmlUtil.GetChildElement(item, "AttachmentMimeType")?.Value
                        };

                        var contentString = XmlUtil.GetChildElement(item, "AttachmentContent")?.Value;
                        if (!string.IsNullOrEmpty(contentString))
                        {
                            attachmentDetail.AttachmentContent = Convert.FromBase64String(contentString);
                        }

#if DEBUG
                        //File.WriteAllBytes(@"C:\Temp\finvoice\incoming\" + attachmentDetail.AttachmentName, attachmentDetail.AttachmentContent);
#endif
                        this.attachmentDetails.Add(attachmentDetail);
                    }
                }
            }
            catch (Exception ex)
            {
                return new ActionResult(ex);
            }

            return result;
        }

    }


    public class FinvoiceAttachmentDetails : FinvoiceBase
    {
        public string AttachmentIdentifier { get; set; }
        public string AttachmentName { get; set; }
        public string AttachmentSecurityClass { get; set; }
        public string AttachmentMimeType { get; set; }

        public byte[] AttachmentContent { get; set; }

        public void AddNode(ref XElement root)
        {
            XElement attachmentDetails = new XElement("AttachmentDetails");
            root.Add(attachmentDetails);

            var content = Convert.ToBase64String(AttachmentContent);

            string attachmentSecureHash;

            using (var sha1Hash = SHA1.Create())
            {
                //From String to byte array
                byte[] sourceBytes = Encoding.UTF8.GetBytes(content);
                byte[] hashBytes = sha1Hash.ComputeHash(sourceBytes);
                attachmentSecureHash = BitConverter.ToString(hashBytes).Replace("-", string.Empty).ToLower();
            }

            parseToNodes(attachmentDetails, "AttachmentIdentifier", GetString(AttachmentIdentifier+ attachmentSecureHash), 0, 1500,1);
            parseToNodes(attachmentDetails, "AttachmentContent", content, 0, content.Length + 1, 1);
            parseToNodes(attachmentDetails, "AttachmentName", GetString(AttachmentName), 0, 50, 1);
            //parseToNodes(attachmentDetails, "AttachmentSecurityClass", GetString(AttachmentSecurityClass), 0, 500, 1); //Not mandatory
            parseToNodes(attachmentDetails, "AttachmentMimeType", GetString(AttachmentMimeType), 0, 50, 1);

            parseToNodes(attachmentDetails, "AttachmentSecureHash", GetString(attachmentSecureHash), 0, attachmentSecureHash.Length+1, 1);
        }
    }
}
