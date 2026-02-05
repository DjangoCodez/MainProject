using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Interfaces;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Util
{
    public class ReadSoftScanningItem : ISoeImportItem
    {
        #region Variables

        private ReportDataManager rdm;

        private readonly ReadSoftMessage message;
        public ReadSoftMessage Message
        {
            get
            {
                return message;
            }
        }
        private ReadSoft.Services.Entities.Document document;
        public ReadSoft.Services.Entities.Document Document
        {
            get
            {
                return document;
            }
        }
        private readonly List<ReadSoftScanningPartyItem> parties;
        public List<ReadSoftScanningPartyItem> Parties
        {
            get
            {
                return parties;
            }
        }
        private readonly List<ReadSoftScanningHeaderFieldItem> headerFields;
        public List<ReadSoftScanningHeaderFieldItem> HeaderFields
        {
            get
            {
                return headerFields;
            }
        }
        private readonly List<ReadSoftScanningHistoryItem> histories;
        public List<ReadSoftScanningHistoryItem> Histories
        {
            get
            {
                return histories;
            }
        }
        private readonly ScanningEntry scanningEntry;
        public ScanningEntry ScanningEntry
        {
            get
            {
                return scanningEntry;
            }
        }
        private readonly Supplier supplier;
        public Supplier Supplier
        {
            get
            {
                return supplier;
            }
        }
        private Dictionary<TermGroup_SysPaymentType, string> supplierPaymentNrs;
        public Dictionary<TermGroup_SysPaymentType, string> PaymentNrs
        {
            get
            {
                return supplierPaymentNrs;
            }
        }
        private string batchId;
        public string BatchId
        {
            get
            {
                return batchId;
            }
        }
        public string CreatedBy { get; set; }
        public DateTime ReceiveTime { get; set; }
        private string documentId;
        public string DocumentId
        {
            get
            {
                return documentId;
            }
        }
        private string type;
        public string Type
        {
            get
            {
                return type;
            }
        }
        private string originalFileName;
        public string OriginalFileName
        {
            get
            {
                return originalFileName;
            }
        }


        #endregion

        #region Ctor

        public ReadSoftScanningItem(ReadSoftMessage message)
        {
            this.message = message;
            this.document = message.Document != null ? message.Document : null;
            this.scanningEntry = null;
            this.supplierPaymentNrs = new Dictionary<TermGroup_SysPaymentType, string>();
            this.parties = new List<ReadSoftScanningPartyItem>();
            this.headerFields = new List<ReadSoftScanningHeaderFieldItem>();
            this.histories = new List<ReadSoftScanningHistoryItem>();

            Parse(message);
        }

        public ReadSoftScanningItem(ScanningEntry scanningEntry, Supplier supplier, Dictionary<TermGroup_SysPaymentType, string> supplierPaymentNrs)
        {
            this.message = null;
            this.document = null;
            this.scanningEntry = scanningEntry;
            this.supplier = supplier;
            this.supplierPaymentNrs = supplierPaymentNrs;
            this.parties = new List<ReadSoftScanningPartyItem>();
            this.headerFields = new List<ReadSoftScanningHeaderFieldItem>();
            this.histories = new List<ReadSoftScanningHistoryItem>();

            Parse(scanningEntry);
        }

        #endregion

        #region Static methods

        public static ReadSoftScanningItem CreateItem(ReadSoftMessage message)
        {
            if (message == null)
                return null;

            return new ReadSoftScanningItem(message);
        }

        public static ReadSoftScanningItem CreateItem(ScanningEntry scanningEntry, Supplier supplier, Dictionary<TermGroup_SysPaymentType, string> supplierPaymentNrs)
        {
            if (scanningEntry == null)
                return null;

            return new ReadSoftScanningItem(scanningEntry, supplier, supplierPaymentNrs);
        }

        public static ScanningEntryRowType GetScanningEntryRowType(string name)
        {
            switch (name.ToLower())
            {
                case Constants.READSOFT_HEADERFIELD_ISCREDITINVOICE:
                    return ScanningEntryRowType.IsCreditInvoice;
                case Constants.READSOFT_HEADERFIELD_INVOICENUMBER:
                    return ScanningEntryRowType.InvoiceNr;
                case Constants.READSOFT_HEADERFIELD_INVOICEDATE:
                    return ScanningEntryRowType.InvoiceDate;
                case Constants.READSOFT_HEADERFIELD_INVOICEDUEDATE:
                    return ScanningEntryRowType.DueDate;
                case Constants.READSOFT_HEADERFIELD_INVOICEORDERNR:
                    return ScanningEntryRowType.OrderNr;
                case Constants.READSOFT_HEADERFIELD_YOURREFERENCE:
                    return ScanningEntryRowType.ReferenceYour;
                case Constants.READSOFT_HEADERFIELD_REFEERENCENR:
                    return ScanningEntryRowType.ReferenceOur;
                case Constants.READSOFT_HEADERFIELD_TOTALAMOUNTEXLUDEDVAT:
                    return ScanningEntryRowType.TotalAmountExludeVat;
                case Constants.READSOFT_HEADERFIELD_TOTALAMOUNTVAT:
                    return ScanningEntryRowType.VatAmount;
                case Constants.READSOFT_HEADERFIELD_TOTALAMOUNTINCUDEDVAT:
                    return ScanningEntryRowType.TotalAmountIncludeVat;
                case Constants.READSOFT_HEADERFIELD_CURRENCY:
                    return ScanningEntryRowType.CurrencyCode;
                case Constants.READSOFT_HEADERFIELD_OCRNR:
                    return ScanningEntryRowType.OCR;
                case Constants.READSOFT_HEADERFIELD_PLUSGIRO:
                    return ScanningEntryRowType.Plusgiro;
                case Constants.READSOFT_HEADERFIELD_BANKGIRO:
                    return ScanningEntryRowType.Bankgiro;
                case Constants.READSOFT_HEADERFIELD_BANKNR:
                    return ScanningEntryRowType.BankNr;
                case Constants.READSOFT_HEADERFIELD_ORGNR:
                    return ScanningEntryRowType.OrgNr;
                case Constants.READSOFT_HEADERFIELD_IBAN:
                    return ScanningEntryRowType.IBAN;
                case Constants.READSOFT_HEADERFIELD_VATRATE:
                    return ScanningEntryRowType.VatRate;
                case Constants.READSOFT_HEADERFIELD_VATNR:
                    return ScanningEntryRowType.VatNr;
                case Constants.READSOFT_HEADERFIELD_FREIGHTAMOUNT:
                    return ScanningEntryRowType.FreightAmount;
                case Constants.READSOFT_HEADERFIELD_CENTROUNDING:
                    return ScanningEntryRowType.CentRounding;
                case Constants.READSOFT_HEADERFIELD_VATREGNUMBER_FIN:
                    return ScanningEntryRowType.VatRegNumberFin;
                case Constants.READSOFT_HEADERFIELD_SUPPLIERBANKCODENUMBER1:
                    return ScanningEntryRowType.SupplierBankCodeNumber1;
            }

            return ScanningEntryRowType.Unknown;
        }

        public static bool isNewAndOldValueTheSame(string newValue, string oldValue)
        {
            if (newValue == null || oldValue == null)
                return false;

            if (newValue.Equals(oldValue)) //Should never have made it this far.
                return true;

            if ((oldValue.Length > 0 && newValue.Length == 0) || (newValue.Length > 0 && oldValue.Length == 0))
                return false;

            string lowerNewValue = newValue.ToLower();
            string lowerOldValue = oldValue.ToLower();

            if (lowerNewValue.Equals(lowerOldValue))
                return true;
            else if (lowerNewValue.Replace(",", ".").Equals(lowerOldValue))
                return true;
            else if (lowerNewValue.Replace("-", "").Equals(lowerOldValue))
                return true;
            else if (lowerNewValue.Trim().Equals(lowerOldValue))
                return true;
            else if (lowerOldValue.Replace(",", ".").Equals(lowerNewValue))
                return true;
            else if (lowerOldValue.Replace("-", "").Equals(lowerNewValue))
                return true;
            else if (lowerOldValue.Trim().Equals(lowerNewValue))
                return true;
            else if (RemoveDiacritics(lowerOldValue).Equals(RemoveDiacritics(lowerNewValue)))
                return true;

            return false;
        }


        public static string ValidateRowText(string newValue, string oldValue, ScanningEntryRowType rowType)
        {

            if (isNewAndOldValueTheSame(newValue, oldValue))
                return oldValue;
            else if (newValue == null)
                newValue = "";
            else
                newValue = newValue.Replace(",", "."); //ReadSoft appear to class "," in amout fields as incorrect format

            switch (rowType)
            {
                case ScanningEntryRowType.IsCreditInvoice:
                    break;
                case ScanningEntryRowType.InvoiceNr:
                    break;
                case ScanningEntryRowType.InvoiceDate:
                    break;
                case ScanningEntryRowType.DueDate:
                    break;
                case ScanningEntryRowType.OrderNr:
                    break;
                case ScanningEntryRowType.ReferenceYour:
                    break;
                case ScanningEntryRowType.ReferenceOur:
                    break;
                case ScanningEntryRowType.TotalAmountExludeVat:
                    break;
                case ScanningEntryRowType.VatAmount:
                    break;
                case ScanningEntryRowType.TotalAmountIncludeVat:
                    break;
                case ScanningEntryRowType.CurrencyCode:
                    break;
                case ScanningEntryRowType.OCR:
                    break;
                case ScanningEntryRowType.Plusgiro:
                    newValue = newValue.RemoveWhiteSpaceAndHyphen();
                    break;
                case ScanningEntryRowType.Bankgiro:
                    newValue = newValue.RemoveWhiteSpaceAndHyphen();
                    break;
                case ScanningEntryRowType.OrgNr:
                    newValue = newValue.RemoveWhiteSpaceAndHyphen();
                    break;
                case ScanningEntryRowType.IBAN:
                    newValue = newValue.RemoveWhiteSpaceAndHyphen();
                    break;
                case ScanningEntryRowType.VatRate:
                    break;
                case ScanningEntryRowType.VatNr:
                    newValue = newValue.RemoveWhiteSpaceAndHyphen();
                    break;
                case ScanningEntryRowType.FreightAmount:
                    break;
                case ScanningEntryRowType.CentRounding:
                    break;
            }

            return newValue;
        }


        public static string ValidateRowText(string text, ScanningEntryRowType rowType)
        {
            text = text.Replace(",", "."); //ReadSoft appear to class "," in amout fields as incorrect format

            switch (rowType)
            {
                case ScanningEntryRowType.IsCreditInvoice:
                    break;
                case ScanningEntryRowType.InvoiceNr:
                    break;
                case ScanningEntryRowType.InvoiceDate:
                    break;
                case ScanningEntryRowType.DueDate:
                    break;
                case ScanningEntryRowType.OrderNr:
                    break;
                case ScanningEntryRowType.ReferenceYour:
                    break;
                case ScanningEntryRowType.ReferenceOur:
                    break;
                case ScanningEntryRowType.TotalAmountExludeVat:
                    break;
                case ScanningEntryRowType.VatAmount:
                    break;
                case ScanningEntryRowType.TotalAmountIncludeVat:
                    break;
                case ScanningEntryRowType.CurrencyCode:
                    break;
                case ScanningEntryRowType.OCR:
                    break;
                case ScanningEntryRowType.Plusgiro:
                    text = text.RemoveWhiteSpaceAndHyphen();
                    break;
                case ScanningEntryRowType.Bankgiro:
                    text = text.RemoveWhiteSpaceAndHyphen();
                    break;
                case ScanningEntryRowType.OrgNr:
                    text = text.RemoveWhiteSpaceAndHyphen();
                    break;
                case ScanningEntryRowType.IBAN:
                    text = text.RemoveWhiteSpaceAndHyphen();
                    break;
                case ScanningEntryRowType.VatRate:
                    break;
                case ScanningEntryRowType.VatNr:
                    text = text.RemoveWhiteSpaceAndHyphen();
                    break;
                case ScanningEntryRowType.FreightAmount:
                    break;
                case ScanningEntryRowType.CentRounding:
                    break;
            }

            return text;
        }

        #endregion

        #region Public methods

        public ReadSoftScanningPartyItem GetBuyer()
        {
            if (Parties == null)
                return null;

            return Parties.FirstOrDefault(i => i.Type == Constants.READSOFT_PARTY_BUYER);
        }

        public ReadSoftScanningPartyItem GetSupplier()
        {
            if (Parties == null)
                return null;

            return Parties.FirstOrDefault(i => i.Type == Constants.READSOFT_PARTY_SUPPLIER);
        }

        public override string ToString()
        {
            string content = "";

            content += "<Document>";

            content += String.Format("<{0}>{1}</{0}>", "Id", this.Document.Id);
            content += String.Format("<{0}>{1}</{0}>", "Version", this.Document.Version);
            content += String.Format("<{0}>{1}</{0}>", "Type", this.Document.Type);
            content += String.Format("<{0}>{1}</{0}>", "OriginalFilename", this.Document.OriginalFilename);

            content += "<Parties>";
            foreach (var party in document.Parties)
            {
                content += "<Party>";
                content += String.Format("<{0}>{1}</{0}>", "Type", party.Type);
                content += String.Format("<{0}>{1}</{0}>", "Name", party.Name);
                content += String.Format("<{0}>{1}</{0}>", "Id", party.Id);
                content += String.Format("<{0}>{1}</{0}>", "ExternalId", party.ExternalId);
                content += "</Party>";
            }
            content += "</Parties>";

            content += "<HeaderFields>";
            foreach (var headerField in document.HeaderFields)
            {
                content += "<HeaderField>";
                content += String.Format("<{0}>{1}</{0}>", "Name", headerField.Name);
                content += String.Format("<{0}>{1}</{0}>", "Type", headerField.Type);
                content += String.Format("<{0}>{1}</{0}>", "Format", headerField.Format);
                content += String.Format("<{0}>{1}</{0}>", "Text", headerField.Text);
                content += String.Format("<{0}>{1}</{0}>", "ValidationError", headerField.ValidationError);
                content += String.Format("<{0}>{1}</{0}>", "Position", headerField.Position);
                content += String.Format("<{0}>{1}</{0}>", "PageNumber", headerField.PageNumber);
                content += "</HeaderField>";
            }
            content += "</HeaderFields>";

            content += "</Document>";

            return content;
        }

        #endregion

        #region Help methods

        private void Parse(ReadSoftMessage message)
        {
            if (message == null || message.Document == null)
                return;

            batchId = message.BatchId;
            CreatedBy = message.CreatedBy;
            ReceiveTime = message.ReceiveTime;
            documentId = message.Document.Id;
            type = message.Document.Type;
            originalFileName = message.Document.OriginalFilename;

            foreach (var party in message.Document.Parties)
            {
                this.parties.Add(new ReadSoftScanningPartyItem()
                {
                    Type = party.Type,
                    Name = party.Name,
                    Id = party.Id,
                    ExternalId = party.ExternalId,
                });
            }

            foreach (var party in this.Parties)
            {
                CheckTextValue(party);
            }

            foreach (var headerField in message.Document.HeaderFields)
            {
                this.headerFields.Add(new ReadSoftScanningHeaderFieldItem()
                {
                    Type = headerField.Type,
                    Name = headerField.Name,
                    Format = headerField.Format,
                    Text = headerField.Text,
                    ValidationError = headerField.ValidationError,
                    Position = headerField.Position,
                    PageNumber = headerField.PageNumber,
                });
            }

            foreach (var headerField in this.HeaderFields)
            {
                CheckTextValue(headerField);
            }

            foreach (var history in message.Document.History)
            {
                this.histories.Add(new ReadSoftScanningHistoryItem()
                {
                    TimeStamp = history.Timestamp,
                    UserFullName = history.UserFullName,
                    Status = history.Status.ToString(),
                    Comment = history.Comment,
                    EntryType = history.EntryType.ToString(),
                    Changes = history.Changes,
                });
            }
        }

        static string RemoveDiacritics(string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }

        private void Parse(ScanningEntry scanningEntry)
        {
            if (scanningEntry == null || scanningEntry.ScanningEntryRow == null)
                return;

            XDocument xdoc = XDocument.Parse(scanningEntry.XML);
            if (xdoc == null)
                return;

            this.document = new ReadSoft.Services.Entities.Document();
            document.Id = scanningEntry.DocumentId;
            document.Version = XmlUtil.GetChildElementValue(xdoc, "Version");
            document.Type = XmlUtil.GetChildElementValue(xdoc, "Type");
            document.OriginalFilename = XmlUtil.GetChildElementValue(xdoc, "OriginalFilename");

            XElement elementParties = XmlUtil.GetChildElement(xdoc, "Parties");
            if (elementParties != null)
            {
                document.Parties = new ReadSoft.Services.Entities.PartyCollection();

                var elementsParty = XmlUtil.GetChildElements(elementParties, "Party");
                foreach (var element in elementsParty)
                {
                    string partyType = XmlUtil.GetChildElementValue(element, "Type");
                    string partyName = XmlUtil.GetChildElementValue(element, "Name");
                    string partyId = XmlUtil.GetChildElementValue(element, "Id");
                    string partyExternalId = XmlUtil.GetChildElementValue(element, "ExternalId");

                    if (partyType == Constants.READSOFT_PARTY_BUYER)
                    {
                        //Buyer specific
                    }
                    if (partyType == Constants.READSOFT_PARTY_SUPPLIER && this.supplier != null)
                    {
                        //Supplier specific
                        partyName = this.supplier.Name;
                    }

                    document.Parties.Add(new ReadSoft.Services.Entities.Party()
                    {
                        Type = partyType,
                        Name = partyName,
                        Id = partyId,
                        ExternalId = partyExternalId,
                    });
                }
            }

            XElement elementHeaderFields = XmlUtil.GetChildElement(xdoc, "HeaderFields");
            if (elementHeaderFields != null)
            {
                document.HeaderFields = new ReadSoft.Services.Entities.HeaderFieldCollection();

                var elementsHeaderField = XmlUtil.GetChildElements(elementHeaderFields, "HeaderField");
                foreach (var element in elementsHeaderField)
                {
                    ScanningEntryRowType rowType = ReadSoftScanningItem.GetScanningEntryRowType(XmlUtil.GetChildElementValue(element, "Type"));
                    ScanningEntryRow row = scanningEntry.ScanningEntryRow.FirstOrDefault(i => i.State == (int)SoeEntityState.Active && i.Type == (int)rowType);
                    if (row == null)
                        continue;
                    
                    string text = row.NewText ?? row.Text;

                    #region Complement with values from Supplier

                    if (this.supplier != null)
                    {
                        if (this.supplierPaymentNrs == null)
                            this.supplierPaymentNrs = new Dictionary<TermGroup_SysPaymentType, string>();

                        string supplierValue = "";

                        switch (rowType)
                        {
                            case ScanningEntryRowType.Plusgiro:
                                if (this.supplierPaymentNrs.ContainsKey(TermGroup_SysPaymentType.PG))
                                    supplierValue = this.supplierPaymentNrs[TermGroup_SysPaymentType.PG].Trim();
                                break;
                            case ScanningEntryRowType.Bankgiro:
                                if (this.supplierPaymentNrs.ContainsKey(TermGroup_SysPaymentType.BG))
                                    supplierValue = this.supplierPaymentNrs[TermGroup_SysPaymentType.BG].Trim();
                                break;
                            case ScanningEntryRowType.OrgNr:
                                supplierValue = this.supplier.OrgNr;
                                break;
                            case ScanningEntryRowType.IBAN:
                                //TODO
                                break;
                            case ScanningEntryRowType.VatNr:
                                supplierValue = this.supplier.VatNr;
                                break;
                        }

                        //Only change the value of any value is found on the Supplier
                        if (!string.IsNullOrEmpty(supplierValue))
                            text = supplierValue;
                    }

                    #endregion

                    #region Validate text

                    text = ValidateRowText(text, row.Text, rowType);

                    switch (rowType)
                    {
                        case ScanningEntryRowType.InvoiceNr:
                            if (text.Length > 15 || text.Length < 3)
                                continue;
                            break;
                        case ScanningEntryRowType.OrgNr:
                            if (text.Length > 12 || text.Length < 10)
                                continue;
                            break;
                        case ScanningEntryRowType.VatNr:
                            if (text.Length > 12 || text.Length < 10)
                                continue;
                            break;
                    }


                    #endregion

                    //All fields should be sent back, event if we dont change it so Readsoft can learn....
                    document.HeaderFields.Add(new ReadSoft.Services.Entities.HeaderField()
                    {
                        Name = XmlUtil.GetChildElementValue(element, "Name"),
                        Type = XmlUtil.GetChildElementValue(element, "Type"),
                        Format = XmlUtil.GetChildElementValue(element, "Format"),
                        Text = text,
                        ValidationError = XmlUtil.GetChildElementValue(element, "ValidationError"),
                        Position = "0,0,0,0",
                        PageNumber = XmlUtil.GetChildElementValue(element, "PageNumber"),
                    });
                }
            }
        }

        private void CheckTextValue(ReadSoftScanningPartyItem party)
        {
            if (party == null)
                return;

            try
            {
                bool valid = true;

                switch (party.Type)
                {
                    case Constants.READSOFT_PARTY_BUYER:
                    case Constants.READSOFT_PARTY_SUPPLIER:
                        if (party.Name != null && party.Name.Length > 256)
                            valid = false;
                        break;
                }

                //Set Name empty if value is not valid (no validation field exists for Parties)
                if (!valid)
                    party.Name = String.Empty;
            }
            catch (Exception ex)
            {
                ex.ToString(); //prevent compiler warning
            }
        }

        private void CheckTextValue(ReadSoftScanningHeaderFieldItem headerField)
        {
            if (headerField == null)
                return;

            try
            {
                bool valid = true;

                switch (headerField.Type)
                {
                    case Constants.READSOFT_HEADERFIELD_ISCREDITINVOICE:
                        //Rule: N/A. No validation required
                        break;
                    case Constants.READSOFT_HEADERFIELD_INVOICENUMBER:
                        //Rule: Max 50 chars (EdiEntry.InvoiceNr[50])
                        if (headerField.Text != null && headerField.Text.Length > 50)
                            valid = false;
                        break;
                    case Constants.READSOFT_HEADERFIELD_INVOICEDATE:
                        //Rule: N/A. No validation required
                        break;
                    case Constants.READSOFT_HEADERFIELD_INVOICEDUEDATE:
                        //Rule: N/A. No validation required
                        break;
                    case Constants.READSOFT_HEADERFIELD_INVOICEORDERNR:
                        //Rule: Max 50 chars (EdiEntry.OrderNr[50])
                        if (headerField.Text != null && headerField.Text.Length > 50)
                            valid = false;
                        break;
                    case Constants.READSOFT_HEADERFIELD_YOURREFERENCE:
                        //Rule: Max 200 chars (EdiEntry.BuyerReference[256] and Invoice.ReferenceOur[256])
                        if (headerField.Text != null && headerField.Text.Length > 200)
                            valid = false;
                        break;
                    case Constants.READSOFT_HEADERFIELD_REFEERENCENR:
                        //Rule: Max 200 chars Invoice.ReferenceOur[200]
                        if (headerField.Text != null && headerField.Text.Length > 200)
                            valid = false;
                        break;
                    case Constants.READSOFT_HEADERFIELD_TOTALAMOUNTEXLUDEDVAT:
                        //Rule: N/A. Field not used
                        break;
                    case Constants.READSOFT_HEADERFIELD_TOTALAMOUNTVAT:
                        //Rule: Max 10.2 decimal (EdiEntry.SumVat[10.2] and Invoice.TotalAmount[10.2])
                        if (headerField.Text != null && !NumberUtility.IsValidNumber(headerField.Text, "10.2"))
                            valid = false;
                        break;
                    case Constants.READSOFT_HEADERFIELD_TOTALAMOUNTINCUDEDVAT:
                        //Rule: Max 10.2 decimal (EdiEntry.Sum[10.2] and Invoice.VATAmount[10.2])
                        if (headerField.Text != null && !NumberUtility.IsValidNumber(headerField.Text, "10.2"))
                            valid = false;
                        break;
                    case Constants.READSOFT_HEADERFIELD_CURRENCY:
                        //Rule: Max 10 chars (CompCurrency.Code[10])
                        if (headerField.Text != null && headerField.Text.Length > 10)
                            valid = false;
                        break;
                    case Constants.READSOFT_HEADERFIELD_OCRNR:
                        //Rule: Max 100 chars (EdiEntry.OCR[100])
                        if (headerField.Text != null && headerField.Text.Length > 100)
                            valid = false;
                        break;
                    case Constants.READSOFT_HEADERFIELD_PLUSGIRO:
                        //Rule: Max 100 chars (EdiEntry.PostalGiro[100] and PaymentInformationRow.PaymentNr[100])
                        if (headerField.Text != null && headerField.Text.Length > 100)
                            valid = false;
                        break;
                    case Constants.READSOFT_HEADERFIELD_BANKGIRO:
                        //Rule: Max 100 chars (EdiEntry.BankGiro[100] and PaymentInformationRow.PaymentNr[100])
                        if (headerField.Text != null && headerField.Text.Length > 100)
                            valid = false;
                        break;
                    case Constants.READSOFT_HEADERFIELD_ORGNR:
                        //Rule: Max 50 chars (Supplier.OrgNr[50])
                        if (headerField.Text != null && headerField.Text.Length > 50)
                            valid = false;
                        break;
                    case Constants.READSOFT_HEADERFIELD_IBAN:
                        //Rule: Max 100 chars (EdiEntry.IBAN and PaymentInformationRow.PaymentNr[100])
                        if (headerField.Text != null && headerField.Text.Length > 100)
                            valid = false;
                        break;
                    case Constants.READSOFT_HEADERFIELD_VATRATE:
                        //Rule: Max 18.2 decimal (EdiEntry.VatRate[18.2])
                        if (headerField.Text != null && !NumberUtility.IsValidNumber(headerField.Text, "18.2"))
                            valid = false;
                        break;
                    case Constants.READSOFT_HEADERFIELD_VATNR:
                        //Rule: N/A. Field not used
                        break;
                    case Constants.READSOFT_HEADERFIELD_FREIGHTAMOUNT:
                        //Rule: N/A. Field not used
                        break;
                    case Constants.READSOFT_HEADERFIELD_CENTROUNDING:
                        //Rule: N/A. Field not used
                        break;
                    case Constants.READSOFT_HEADERFIELD_VATREGNUMBER_FIN:
                        //Rule: N/A. Field not used
                        break;
                    case Constants.READSOFT_HEADERFIELD_SUPPLIERBANKCODENUMBER1:
                        //Rule: N/A. Field not used
                        break;
                    case Constants.READSOFT_HEADERFIELD_BANKNR:
                        //Rule: N/A. Field not used
                        break;
                }

                //Set Text empty if value is not valid, and set ValidationError to not found
                if (!valid)
                {
                    headerField.Text = String.Empty;
                    headerField.ValidationError = ((int)TermGroup_ScanningInterpretation.ValueNotFound).ToString();
                }
            }
            catch (Exception ex)
            {
                ex.ToString(); //prevent compiler warning
            }
        }

        #endregion

        #region ISoeImportItem implementation

        public DataSet ToDataSet()
        {
            if (rdm == null)
                rdm = new ReportDataManager(null);

            return rdm.CreateReadSoftScanningData(this);
        }

        private XDocument xdocument = null;
        public XDocument ToXDocument()
        {
            if (xdocument == null)
            {
                if (rdm == null)
                    rdm = new ReportDataManager(null);

                xdocument = rdm.CreateReadSoftScanningDataDocument(this);
            }
            return xdocument;
        }

        #endregion
    }

    public class ReadSoftScanningPartyItem
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public string Id { get; set; }
        public string ExternalId { get; set; }
    }

    public class ReadSoftScanningHeaderFieldItem
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public string Format { get; set; }
        public string Text { get; set; }
        public string ValidationError { get; set; }
        public string Position { get; set; }
        public string PageNumber { get; set; }
    }

    public class ReadSoftScanningHistoryItem
    {
        public DateTime TimeStamp { get; set; }
        public string UserFullName { get; set; }
        public string Status { get; set; }
        public string Comment { get; set; }
        public string EntryType { get; set; }
        public string Changes { get; set; }
    }
}
