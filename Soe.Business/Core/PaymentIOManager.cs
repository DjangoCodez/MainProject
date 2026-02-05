using SoftOne.Soe.Business.Core.PaymentIO;
using SoftOne.Soe.Business.Core.PaymentIO.BBS;
using SoftOne.Soe.Business.Core.PaymentIO.BgMax;
using SoftOne.Soe.Business.Core.PaymentIO.Cfp;
using SoftOne.Soe.Business.Core.PaymentIO.Intrum;
using SoftOne.Soe.Business.Core.PaymentIO.Lb;
using SoftOne.Soe.Business.Core.PaymentIO.Nets;
using SoftOne.Soe.Business.Core.PaymentIO.Pg;
using SoftOne.Soe.Business.Core.PaymentIO.SEPA;
using SoftOne.Soe.Business.Core.PaymentIO.SEPAV3;
using SoftOne.Soe.Business.Core.PaymentIO.SOP;
using SoftOne.Soe.Business.Core.PaymentIO.TotalIn;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Core
{
    /// <summary>
    /// Controls access to import/export of payment files and access of payment artifacts
    /// </summary>
    public class PaymentIOManager : ManagerBase
    {
        #region Variables

        // Create a logger for use in this class
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #endregion

        #region Ctor

        public PaymentIOManager(ParameterObject parameterObject) : base(parameterObject) { }

        #endregion

        #region Import

        public ActionResult Import(TermGroup_SysPaymentMethod paymentIOType, int paymentMethodId, Stream stream, string fileName, int actorCompanyId, int userId, int batchId, int paymentImportId, ImportPaymentType importType, bool isCAMT53 = false)
        {
            List<string> paymentLog = new List<string>();
            Dictionary<string, decimal> notFound = new Dictionary<string, decimal>();

            var result = new ActionResult(true);

            List<Origin> origins = null;
            List<Payment> payments = null;
            List<PaymentRow> paymentRows = null;
            Origin origin = null;
            bool templateIsSuggestion = false;
            var encoding = paymentIOType == TermGroup_SysPaymentMethod.BGMax || paymentIOType == TermGroup_SysPaymentMethod.LB ? System.Text.Encoding.GetEncoding("ISO8859-1") : System.Text.Encoding.UTF8;
            using (var sr = new StreamReader(stream, encoding))
            {
                switch (paymentIOType)
                {
                    case TermGroup_SysPaymentMethod.LB:
                        payments = FindPayment(sr, paymentIOType, actorCompanyId, ref paymentLog, ref notFound);
                        break;
                    case TermGroup_SysPaymentMethod.Cfp:
                        paymentRows = FindPaymentCfp(sr, actorCompanyId, ref paymentLog);
                        break;
                    case TermGroup_SysPaymentMethod.BGMax:
                    case TermGroup_SysPaymentMethod.SOP:
                    case TermGroup_SysPaymentMethod.TOTALIN:
                        origins = FindOrigins(sr, paymentIOType, actorCompanyId);
                        break;
                    case TermGroup_SysPaymentMethod.PG:
                        origin = FindOrigin(sr, paymentIOType, actorCompanyId);
                        break;
                    default:
                        break;
                }
                sr.DiscardBufferedData();
                sr.BaseStream.Position = 0; //imp. reset the stream

                switch (paymentIOType)
                {
                    case TermGroup_SysPaymentMethod.LB:
                        if (payments.IsNullOrEmpty() && notFound.Count == 0)
                            return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(8107, "Betalningen som filen anger kunde inte hittas"));

                        result = LbManager.Import(sr, actorCompanyId, fileName, payments, notFound, paymentMethodId, ref paymentLog, userId, batchId, paymentImportId, importType);
                        break;
                    case TermGroup_SysPaymentMethod.Cfp:
                        if (paymentRows.IsNullOrEmpty() && notFound.Count == 0)
                            return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(8107, "Betalningen som filen anger kunde inte hittas"));

                        result = CfpManager.Import(sr, actorCompanyId, fileName, paymentRows, notFound, paymentMethodId, ref paymentLog, userId, batchId, paymentImportId, importType);
                        break;
                    case TermGroup_SysPaymentMethod.BGMax:
                        if (origins == null)
                            return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(8082, "Underlaget kunde inte hittas"));

                        result = BgMaxManager.Import(sr, actorCompanyId, fileName, origins, templateIsSuggestion, paymentMethodId, ref paymentLog, userId, batchId, paymentImportId, importType);
                        break;
                    case TermGroup_SysPaymentMethod.TOTALIN:
                        if (origins == null)
                            return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(8082, "Underlaget kunde inte hittas"));
                        result = TotalInManager.Import(sr, actorCompanyId, templateIsSuggestion, paymentMethodId, ref paymentLog, userId, batchId, paymentImportId, importType);
                        break;
                    case TermGroup_SysPaymentMethod.SOP:
                        if (origins == null)
                            return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(8082, "Underlaget kunde inte hittas"));

                        result = SOPManager.Import(sr, actorCompanyId, fileName, origins, templateIsSuggestion, paymentMethodId, ref paymentLog);
                        break;
                    case TermGroup_SysPaymentMethod.PG:
                        if (origin == null)
                            return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(8082, "Underlaget kunde inte hittas"));

                        if (HasExistingPayments(origin))
                            return new ActionResult(false, (int)ActionResultSave.EntityIsNull, GetText(1947, "Betalning redan registrerad"));

                        result = PgManager.Import(sr, actorCompanyId, fileName, origin, templateIsSuggestion, paymentMethodId, ref paymentLog, userId, batchId, paymentImportId, importType);
                        break;
                    case TermGroup_SysPaymentMethod.ISO20022:
                        if (isCAMT53)
                            result = SEPAV3Manager.ImportCAMT53Extended(sr, actorCompanyId, fileName, paymentMethodId, ref paymentLog, paymentImportId, batchId, importType);
                        else
                            result = SEPAV3Manager.Import(sr, actorCompanyId, fileName, paymentMethodId, ref paymentLog, paymentImportId, batchId, importType);
                        break;
                    case TermGroup_SysPaymentMethod.Intrum:
                        var intrum = new IntrumPaymentManager(this.parameterObject);
                        result = intrum.Import(sr, actorCompanyId, SoeOriginType.CustomerPayment, fileName, paymentMethodId, ref paymentLog, paymentImportId, batchId, importType);
                        break;
                    case TermGroup_SysPaymentMethod.SEPA:
                        #region Clean Sepa file (can be combined)
                        const string xmlDocStartSearchTag = "<?xml version=";
                        const string SepaEndtag = "</Document>";
                        string importedFileAsString = sr.ReadToEnd();
                        var sepaFiles = new List<string>();

                        if (importedFileAsString.IndexOf(xmlDocStartSearchTag) < 0)
                            return new ActionResult((int)ActionResultSave.EdiFailedParse, GetText(8175, "Kan inte hitta XML version"));

                        while (importedFileAsString.IndexOf(xmlDocStartSearchTag) >= 0)
                        {
                            int startIndex = importedFileAsString.IndexOf(xmlDocStartSearchTag);
                            importedFileAsString = importedFileAsString.Substring(startIndex);
                            int stopIndex = importedFileAsString.IndexOf(SepaEndtag) + SepaEndtag.Length;
                            sepaFiles.Add(importedFileAsString.Substring(0, stopIndex));
                            importedFileAsString = importedFileAsString.Substring(stopIndex);
                        }

                        #endregion
                        int paymentMethodXmlId = 0;
                        var reportXDocument = new XDocument();
                        var reportXRoot = new XElement("PaymentMethods");
                        reportXDocument.Add(reportXRoot);

                        foreach (var item in sepaFiles)
                        {
                            paymentMethodXmlId++;
                            if (paymentImportId == 0)
                                result = SEPAManager.Import(reportXRoot, item, actorCompanyId, fileName, templateIsSuggestion, paymentMethodId, paymentMethodXmlId, ref paymentLog);
                            else
                                result = SEPAManager.ImportIO(item, actorCompanyId, templateIsSuggestion, paymentMethodId, ref paymentLog, userId, batchId, paymentImportId, importType);
                        }

                        //old way....
                        if (paymentImportId == 0)
                        {
                            var reportXMLString = reportXDocument.ToString();
                            SEPAManager.UpdateDataStorage(actorCompanyId, reportXMLString);
                        }
                        break;

                    case TermGroup_SysPaymentMethod.BBSOCR:
                        var bbs = new BBSPaymentManager(this.parameterObject);
                        result = bbs.Import(sr, actorCompanyId, batchId, paymentMethodId, paymentImportId);
                        break;

                    default:
                        result.Success = false;
                        result.ErrorNumber = (int)ActionResultSave.PaymentIncorrectPaymentType;
                        result.ErrorMessage = GetText(8083, "Filens betalningstyp kan inte importeras");
                        return result;
                }
            }

            //Log for reading in was not workin anymore for finnish customers Kai
            if ((int)paymentIOType == (int)TermGroup_SysPaymentMethod.SEPA)
                result.Value = paymentLog;

            return result;
        }

        public ActionResult ImportCAMT53(string bic, string bankAccontNr, int actorCompanyId, string fileContent)
        {
            var sepa = new SEPAV3Manager(this.parameterObject);

            bool hasCustomerPayments;
            bool hasSupplierPayments;

            var result = sepa.Camt53Contains(fileContent, out hasCustomerPayments, out hasSupplierPayments);
            if (result.Success)
            {
                if (hasCustomerPayments)
                {
                    result = ImportCAMTFile(bic, bankAccontNr, actorCompanyId, ImportPaymentType.CustomerPayment, fileContent, true);
                    if (!result.Success)
                    {
                        return result;
                    }
                }

                if (hasSupplierPayments)
                {
                    result = ImportCAMTFile(bic, bankAccontNr, actorCompanyId, ImportPaymentType.SupplierPayment, fileContent, true);
                }
            }

            return result;
        }
        

        public ActionResult ImportCAMTFile(string bic, string bankAccontNr, int actorCompanyId, ImportPaymentType importType, string fileContent, bool isCAMT53=false)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            var companyCountryId = GetCompanySysCountryIdFromCache(entitiesReadOnly, actorCompanyId);
            var paymentMethodTerm = companyCountryId == (int)TermGroup_Country.FI ? TermGroup_SysPaymentMethod.SEPA : TermGroup_SysPaymentMethod.ISO20022;

            var paymentType = importType == ImportPaymentType.SupplierPayment ? SoeOriginType.SupplierPayment : SoeOriginType.CustomerPayment;
            
            var paymentMethod = PaymentManager.GetPaymentMethodByBankAccountNr(actorCompanyId, bic, bankAccontNr, paymentType, paymentMethodTerm);
            if (paymentMethod == null)
            {
                paymentMethod = PaymentManager.GetPaymentMethodByBankAccountNr(actorCompanyId, bic, bankAccontNr, paymentType, TermGroup_SysPaymentMethod.None);
            }

            string fileName;

            if (isCAMT53)
            {
                fileName = "Camt53Ext.xml";
            }
            else if (importType == ImportPaymentType.SupplierPayment)
            {
                fileName = "Camt54d.xml";
            }
            else
            {
                fileName = "Camt54c.xml";
            }

            //Precheck for SEPA needs XML tag so make sure its there
            if (paymentMethodTerm == TermGroup_SysPaymentMethod.SEPA && !fileContent.StartsWith("<?xml version="))
            {
                fileContent = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" + fileContent;
            }

            var importDto = new Common.DTO.PaymentImportDTO
            {
                ActorCompanyId = actorCompanyId,
                ImportDate = DateTime.Today,
                ImportType = importType,
                Type = paymentMethod != null ? paymentMethod.PaymentMethodId : 0,
                Filename = fileName,
                SysPaymentTypeId = paymentMethod != null ? (TermGroup_SysPaymentType)paymentMethod.PaymentInformationRow.SysPaymentTypeId : TermGroup_SysPaymentType.Unknown,
                TransferStatus = (int)TermGroup_PaymentTransferStatus.Transfered,
            };

            if (paymentMethod == null)
            {
                importDto.TransferStatus = (int)TermGroup_PaymentTransferStatus.SoftoneError;
                importDto.StatusName = $"{GetText(7644, "Betalningsmetod saknas för kontonr")}:{bic}/{bankAccontNr}";
            }

            var saveHeaderResult = PaymentManager.SavePaymentImportHeader(actorCompanyId, importDto);

            if (saveHeaderResult.Success)
            {
                var paymentMethodId = paymentMethod?.PaymentMethodId ?? 0;
                var fileContentBytes = Encoding.UTF8.GetBytes(fileContent);
                Stream stream = new MemoryStream(fileContentBytes, true);
                return this.Import(paymentMethodTerm, paymentMethodId, stream, fileName, actorCompanyId, 0, Convert.ToInt32(saveHeaderResult.Value), saveHeaderResult.IntegerValue, importType, isCAMT53);
            }
            else
            {
                return saveHeaderResult;
            }
        }

        public ActionResult ImportPain002(string fileContent)
        {
            XDocument xdoc = XDocument.Parse(fileContent);

            if (SEPAV3Manager.IsPain002_V3(xdoc))
                return SEPAV3Manager.ImportPain002(xdoc);
            else if (SEPAManager.IsPain002_V2(xdoc))
                return SEPAManager.ImportPain002(xdoc);
            else
                return new ActionResult(8176, "Kan inte läsa från XML fil");
        }
        #endregion

        #region Export

        public ActionResult Export(CompEntities entities, List<SysCountry> sysCountries, List<SysCurrency> sysCurrencies, TransactionScope transaction, Payment payment, int sysPaymentMethodId, int actorCompanyId, TermGroup_PaymentTransferStatus initialTransferStatus, bool sendPaymentFile)
        {
            ActionResult result = new ActionResult(true);

            List<PaymentRow> paymentRows = PaymentManager.GetPaymentRowsForExport(payment);
            if ( paymentRows.Any() )
            {
                switch ( sysPaymentMethodId )
                {
                    case (int)TermGroup_SysPaymentMethod.LB:
                        result = LbManager.Export(entities, sysCountries, transaction, payment.PaymentMethod, paymentRows, payment.PaymentId, actorCompanyId);
                        break;
                    case (int)TermGroup_SysPaymentMethod.PG:
                        result = PgManager.Export(entities, transaction, payment.PaymentMethod, paymentRows, payment.PaymentId, actorCompanyId);
                        break;
                    case (int)TermGroup_SysPaymentMethod.Cfp:
                        result = CfpManager.Export(entities, transaction, payment.PaymentMethod, paymentRows, payment.PaymentId, actorCompanyId);
                        break;
                    case (int)TermGroup_SysPaymentMethod.SEPA:
                        result = SEPAManager.Export(entities, sysCountries, sysCurrencies, transaction, payment.PaymentMethod, paymentRows, payment.PaymentId, actorCompanyId, initialTransferStatus);
                        break;
                    case (int)TermGroup_SysPaymentMethod.NordeaCA:
                    case (int)TermGroup_SysPaymentMethod.ISO20022:
                        result = SEPAV3Manager.Export(entities, sysCountries, sysCurrencies, transaction, payment.PaymentMethod, paymentRows, actorCompanyId, initialTransferStatus, sendPaymentFile);
                        break;
                    case (int)TermGroup_SysPaymentMethod.Nets:
                        result = NetsManager.Export(entities, transaction, payment.PaymentMethod, paymentRows, payment.PaymentId, actorCompanyId);
                        break;
                    case (int)TermGroup_SysPaymentMethod.Autogiro:
                    case (int)TermGroup_SysPaymentMethod.Cash:
                        result = DoNotExport();
                        break;
                    default:
                        result.Success = false;
                        result.ErrorNumber = (int)ActionResultSave.PaymentIncorrectPaymentType;
                        result.ErrorMessage = GetText(8083, "Filens betalningstyp kan inte importeras");
                        break;
                }
                if (result.Success && result.Value is PaymentExport)
                {
                    payment.PaymentExport = result.Value as PaymentExport;
                }
            }
            else
            {
                result = DoNotExport();
            }
            return result;
        }

        public ActionResult UpdateExportStatus(bool success, string msgId, int status, string message)
        {
            using (var entities = new CompEntities())
            {
                var export = entities.PaymentExport.FirstOrDefault(p => p.MsgId == msgId);
                if (export != null)
                {
                    TermGroup_PaymentTransferStatus exportStatus = (TermGroup_PaymentTransferStatus)status;

                    if (exportStatus != TermGroup_PaymentTransferStatus.None && export.TransferStatus != (int)exportStatus)
                    {
                        export.TransferMsg = message;
                        export.TransferStatus = (int)exportStatus;

                        return SaveChanges(entities);
                    }

                    return new ActionResult {Success=true, InfoMessage= "Payment found but with wrong status:" + msgId };
                }
            }

            return new ActionResult("Payment export not found:" + msgId);
        }

        private ActionResult DoNotExport()
        {
            ActionResult result = new ActionResult(true);
            result.SuccessNumber = (int)ActionResultSave.PaymentsNotExported;
            return result;
        }

        #endregion

        #region Artifact access


        private PaymentRow GetPaymentRowFromInvoiceCfp(int seqNr, decimal amount, int actorCompanyId)
        {
            using (var entities = new CompEntities())
            {

                var invoice = (from i in entities.Invoice.Include("Origin")
                          .Include("Actor.Supplier")
                               where ((i.SeqNr == seqNr) &&
                               (i.Origin.Company.ActorCompanyId == actorCompanyId) &&
                               (i.State == (int)SoeEntityState.Active))
                               select i).FirstOrDefault();



                return GetPaymentFromInvoiceCfp(invoice);
            }
        }

        private PaymentRow GetPaymentFromInvoiceCfp(Invoice invoice)
        {
            if (invoice == null)
                return null;

            if (!invoice.PaymentRow.IsLoaded)
                invoice.PaymentRow.Load();

            return invoice.PaymentRow?.FirstOrDefault(p => p.Status != (int)SoePaymentStatus.Cancel);
        }

        private Payment GetPaymentFromInvoice(string invoiceNr, decimal amount, int actorCompanyId, List<Payment> payments, bool creditPartPayment, int? invoiceSeqNr, SoeOriginType originType)
        {
            using (var entities = new CompEntities())
            {

                IQueryable<Invoice> matchinginvoiceQuery = (from i in entities.Invoice.Include("Origin")
                                                              .Include("Actor.Supplier")
                                                              .Include("Actor.Customer")
                                                            where
                                                            i.Origin.Company.ActorCompanyId == actorCompanyId &&
                                                            i.State == (int)SoeEntityState.Active
                                                            select i);
                if (originType != SoeOriginType.None)
                {
                    matchinginvoiceQuery = matchinginvoiceQuery.Where(i => i.Origin.Type == (int)originType);
                }

                if (invoiceSeqNr.GetValueOrDefault() > 0)
                    matchinginvoiceQuery = matchinginvoiceQuery.Where(i => (i.SeqNr == invoiceSeqNr || i.OCR == invoiceNr || i.InvoiceNr == invoiceNr));
                else
                    matchinginvoiceQuery = matchinginvoiceQuery.Where(i => (i.OCR == invoiceNr) || (i.InvoiceNr == invoiceNr));

                var matchinginvoiceList = matchinginvoiceQuery.ToList();

                Invoice invoice = null;
                if (invoiceSeqNr.HasValue)
                    invoice = matchinginvoiceList.FirstOrDefault(i => i.SeqNr == invoiceSeqNr);
                if (invoice == null)
                    invoice = matchinginvoiceList.FirstOrDefault(i => i.OCR == invoiceNr);
                if (invoice == null)
                    invoice = matchinginvoiceList.FirstOrDefault(i => i.InvoiceNr == invoiceNr);

                return GetPaymentFromInvoice(invoice, payments, creditPartPayment);
            }
        }
        private Payment GetPaymentFromInvoice(Invoice invoice, List<Payment> payments, bool creditPartPayment = false)
        {
            if (invoice == null)
                return null;

            if (!invoice.PaymentRow.IsLoaded)
                invoice.PaymentRow.Load();

            PaymentRow payrow =creditPartPayment ? invoice.PaymentRow.FirstOrDefault(p => p.Status == (int)SoePaymentStatus.Pending) : invoice.PaymentRow.FirstOrDefault(p => p.Status != (int)SoePaymentStatus.Cancel);
            if (payrow == null)
                return null;

            if (!payrow.PaymentReference.IsLoaded)
                payrow.PaymentReference.Load();

            if (payrow.Payment != null &&!payments.Any(p => p.PaymentId == payrow.Payment.PaymentId))
            {
                if (!payrow.Payment.PaymentRow.IsLoaded)
                    payrow.Payment.PaymentRow.Load();

                foreach (PaymentRow row in payrow.Payment.PaymentRow)
                {
                    if (!row.InvoiceReference.IsLoaded)
                        row.InvoiceReference.Load();

                    if (!row.Invoice.ActorReference.IsLoaded)
                        row.Invoice.ActorReference.Load();

                    if (!row.Invoice.Actor.SupplierReference.IsLoaded)
                        row.Invoice.Actor.SupplierReference.Load();
                }
            }

            return payrow.Payment;
        }

        private Origin GetOriginFromInvoice(int actorCompanyId, string invoiceNr, DateTime duedate, decimal amount)
        {
            using (var entities = new CompEntities())
            {
                var invoice = (from i in entities.Invoice.Include("Origin.OriginInvoiceMapping")
                               where i.Origin.ActorCompanyId == actorCompanyId &&
                              ((i.InvoiceNr == invoiceNr) &&
                              (i.DueDate == duedate) && (i.TotalAmount == amount) &&
                              (i.State == (int)SoeEntityState.Active))
                               select i).FirstOrDefault();

                return GetOriginFromInvoice(invoice);
            }
        }
        private Origin GetOriginFromInvoice(int actorCompanyId, string invoiceNr, decimal amount)
        {
            using (var entities = new CompEntities())
            {
                var invoice = (from i in entities.Invoice.Include("Origin.OriginInvoiceMapping")
                               where i.Origin.ActorCompanyId == actorCompanyId &&
                               ((i.InvoiceNr == invoiceNr) &&
                               (i.TotalAmount == amount) &&
                               (i.State == (int)SoeEntityState.Active))
                               select i).FirstOrDefault();

                if (invoice == null)
                    return null;

                return GetOriginFromInvoice(invoice);
            }
        }

        private Origin GetOriginFromInvoice(int actorCompanyId, string invoiceNr, bool checkAlsoOCR = false)
        {
            using (var entities = new CompEntities())
            {
                var invoice = (from i in entities.Invoice.Include("Origin.OriginInvoiceMapping")
                               where i.Origin.ActorCompanyId == actorCompanyId &&
                               ((i.InvoiceNr == invoiceNr) &&
                               (i.Origin.Type == (int)SoeOriginType.CustomerInvoice) &&
                               (i.State == (int)SoeEntityState.Active))
                               select i).FirstOrDefault();

                if (invoice == null && checkAlsoOCR)
                {
                    invoice = (from i in entities.Invoice.Include("Origin.OriginInvoiceMapping")
                               where i.Origin.ActorCompanyId == actorCompanyId &&
                               ((i.OCR == invoiceNr) &&
                               (i.Origin.Type == (int)SoeOriginType.CustomerInvoice) &&
                               (i.State == (int)SoeEntityState.Active))
                               select i).FirstOrDefault();
                }

                if (invoice == null)
                    return null;

                return GetOriginFromInvoice(invoice);
            }
        }

        private Origin GetOriginFromInvoiceSOP(string invoiceNr, int actorCompanyId)
        {

            using (var entities = new CompEntities())
            {
                List<Invoice> invoices = (from i in entities.Invoice.Include("Origin.OriginInvoiceMapping")
                                          where (i.InvoiceNr.EndsWith(invoiceNr) &&
                                          (i.Origin.Company.ActorCompanyId == actorCompanyId) &&
                                          (i.Origin.Type == (int)SoeOriginType.CustomerInvoice) &&
                                          (i.State == (int)SoeEntityState.Active))
                                          select i).ToList();
                if (invoices == null)
                    return null;
                Invoice invoice = null;
                foreach (Invoice listInvoice in invoices)
                {
                    int invoiceEnt = Convert.ToInt32(listInvoice.InvoiceNr);
                    int invoicePar = Convert.ToInt32(invoiceNr);
                    if (invoiceEnt != invoicePar)
                        continue;
                    invoice = listInvoice;
                }
                if (invoice == null)
                    return null;

                return GetOriginFromInvoice(invoice);
            }
        }
        private Origin GetOriginFromInvoice(string invoiceNr, decimal amount, string customerNr)
        {
            using (var entities = new CompEntities())
            {
                var invoice = (from i in entities.Invoice.Include("Origin.OriginInvoiceMapping")
                               where ((i.InvoiceNr == invoiceNr) &&
                               (i.TotalAmount == amount) && (i.Actor.Customer.CustomerNr == customerNr) &&
                               (i.State == (int)SoeEntityState.Active))
                               select i).FirstOrDefault();
                if (invoice == null)
                    return null;

                return GetOriginFromInvoice(invoice);
            }
        }

        private Origin GetOriginFromInvoice(Invoice invoice)
        {
            if (invoice.Origin == null)
                return null;

            Origin origin = invoice.Origin;

            if (!origin.InvoiceReference.IsLoaded)
                origin.InvoiceReference.Load();

            return origin;
        }
        #endregion

        #region File info

        public TermGroup_SysPaymentMethod ParseFileType(StreamReader sr)
        {
            sr.DiscardBufferedData();
            sr.BaseStream.Position = 0;
            TermGroup_SysPaymentMethod returnValue = TermGroup_SysPaymentMethod.None;
            while (!sr.EndOfStream)
            {
                string ln = sr.ReadLine();
                if (!string.IsNullOrEmpty(ln))
                {
                    if (ln.Contains(Utilities.POST_PRODUCT_NAME_LB))
                        returnValue = TermGroup_SysPaymentMethod.LB;
                    else if (ln.Contains(Utilities.POST_PRODUCT_NAME_BGC))
                        returnValue = TermGroup_SysPaymentMethod.BGMax;
                }
            }
            return returnValue;
        }

        private Origin FindOrigin(StreamReader sr, TermGroup_SysPaymentMethod type, int actorCompanyId)
        {
            sr.DiscardBufferedData();
            sr.BaseStream.Position = 0;
            return FindOriginByIdentifier(sr, type, actorCompanyId);
        }

        private List<Origin> FindOrigins(StreamReader sr, TermGroup_SysPaymentMethod type, int actorCompanyId)
        {
            sr.DiscardBufferedData();
            sr.BaseStream.Position = 0;
            return FindOriginsByIdentifier(sr, type, actorCompanyId);
        }

        private List<Payment> FindPayment(StreamReader sr, TermGroup_SysPaymentMethod type, int actorCompanyId, ref List<string> log, ref Dictionary<string, decimal> notFound)
        {
            List<Payment> payments = null;

            sr.DiscardBufferedData();
            sr.BaseStream.Position = 0;

            switch (type)
            {
                case TermGroup_SysPaymentMethod.BGMax:
                case TermGroup_SysPaymentMethod.LB:
                    #region BGMax, LB

                    payments = FindPaymentByIdentifier(sr, type, actorCompanyId, ref log, ref notFound);

                    #endregion
                    break;
                case TermGroup_SysPaymentMethod.PG:

                    #region PG

                    payments = new List<Payment>();

                    List<int> paymentIds = FindPaymentIdByReference(sr);
                    foreach (int paymentId in paymentIds)
                    {
                        Payment payment = PaymentManager.GetPayment(paymentId);
                        if (payment != null && !payments.Contains(payment))
                            payments.Add(payment);
                        else
                            log.Add(GetText(7076, "Kunde inte hitta betalning för fakturanr") + ": " + paymentId.ToString());
                    }

                    #endregion
                    break;
                case TermGroup_SysPaymentMethod.Cfp:
                    payments = new List<Payment>();

                    List<int> paymentSeqNrs = FindPaymentByReferenceCfp(sr);
                    foreach (int seqNr in paymentSeqNrs)
                    {
                        PaymentRow paymentRow = PaymentManager.GetPaymentBySeqNr(actorCompanyId, seqNr);
                        if (paymentRow != null && !payments.Contains(paymentRow.Payment))
                            payments.Add(paymentRow.Payment);
                        else
                            log.Add(GetText(7076, "Kunde inte hitta betalning för fakturanr") + ": " + seqNr.ToString());
                    }


                    break;
            }

            return payments;
        }
        private List<PaymentRow> FindPaymentCfp(StreamReader sr, int actorCompanyId, ref List<string> log)
        {
            sr.DiscardBufferedData();
            sr.BaseStream.Position = 0;

            List<PaymentRow> paymentRows = FindPaymentByIdentifierCfp(sr, actorCompanyId, ref log);
            return paymentRows;
        }


        private List<int> FindPaymentIdByReference(StreamReader sr)
        {
            List<int> list = new List<int>();
            try
            {
                while (!sr.EndOfStream)
                {
                    var ln = sr.ReadLine();
                    if (string.IsNullOrEmpty(ln)) continue;
                    var lnTc = ln.Substring(0, 1);
                    int tc;
                    int.TryParse(lnTc, out tc);
                    if (tc < 1)
                        continue;

                    switch (tc)
                    {
                        case (int)PgPostType.MessagePost:
                            var tmp = ln.Substring(1, 30).Trim();
                            if (!string.IsNullOrEmpty(tmp))
                                list.Add(Convert.ToInt32(tmp));
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                base.LogError(ex, this.log);
            }

            return list;
        }
        private List<int> FindPaymentByReferenceCfp(StreamReader sr)
        {
            List<int> list = new List<int>();
            try
            {
                while (!sr.EndOfStream)
                {
                    var ln = sr.ReadLine();
                    ln = Utilities.AddPadding(ln, Utilities.BGC_LINE_MAX_LENGTH);
                    if (string.IsNullOrEmpty(ln)) continue;
                    string lnTc = ln.Substring(0, 4);


                    switch (lnTc)
                    {
                        case Utilities.DA1_SENDERNOTES_RECORD_TYPE:
                            {
                                var tmp = ln.Substring(31, 30).Trim();
                                int seq;
                                bool ok = Int32.TryParse(tmp, out seq);
                                if (ok && !string.IsNullOrEmpty(tmp))
                                    list.Add(seq);
                                break;
                            }
                        case Utilities.DA1_DEBIT_RECORD_TYPE:
                        case Utilities.DA1_CREDIT_RECORD_TYPE:
                            {
                                var tmp = ln.Substring(52, 25).Trim();
                                int seq;
                                bool ok = Int32.TryParse(tmp, out seq);
                                if (ok && !string.IsNullOrEmpty(tmp))
                                    list.Add(seq);
                                break;
                            }
                    }
                }
            }
            catch (Exception ex)
            {
                base.LogError(ex, this.log);
            }

            return list;
        }

        private List<Payment> FindPaymentByIdentifier(StreamReader sr, TermGroup_SysPaymentMethod type, int actorCompanyId, ref List<string> log, ref Dictionary<string, decimal> notFound)
        {
            List<Payment> payments = new List<Payment>();
            string invoiceNum = string.Empty;
            string invoiceSeqNrStr = string.Empty;
            int invoiceSeqNr = 0;

            Payment payment = new Payment();

            int lineNr = 0;
            while (!sr.EndOfStream)
            {
                try
                {
                    lineNr++;

                    var line = sr.ReadLine();
                    if (string.IsNullOrEmpty(line))
                        continue;

                    if (line.Length < 2)
                        continue;

                    int.TryParse(line.Substring(0, 2), out int tc);
                    if (tc < 1)
                        continue;

                    decimal invoiceAmount;
                    //string invoiceNum;

                    switch ((int)type)
                    {
                        case (int)TermGroup_SysPaymentMethod.LB:
                            #region LB

                            switch (tc)
                            {
                                case (int)LbTransactionCodeDomestic.PaymentPost:
                                case (int)LbTransactionCodeDomestic.ReductionPost:
                                case (int)LbTransactionCodeDomestic.CreditInvoiceObservationCompletePost:
                                case (int)LbTransactionCodeDomestic.CreditInvoiceObservationPost:
                                case (int)LbTransactionCodeDomestic.PlusgiroPost:
                                    #region PaymentPost, ReductionPost, CreditInvoiceObservationCompletePost, CreditInvoiceObservationPost

                                    invoiceNum = line.SafeSubstring(12, 25).Trim();
                                    invoiceAmount = Utilities.GetAmount(Utilities.GetNumeric(Utilities.RemoveLeadingZeros(line), 37, 12));
                                    invoiceSeqNrStr = line.SafeSubstring( 60, 20).Trim();
                                    
                                    if (!string.IsNullOrEmpty(invoiceSeqNrStr))
                                    {
                                        var pos = invoiceSeqNrStr.IndexOf(",");
                                        if (pos > 0)
                                        {
                                            invoiceSeqNrStr = invoiceSeqNrStr.Remove(invoiceSeqNrStr.IndexOf(","));
                                        }
                                        int.TryParse(invoiceSeqNrStr, out invoiceSeqNr);
                                    }

                                    if (tc == (int)LbTransactionCodeDomestic.CreditInvoiceObservationCompletePost || tc == (int)LbTransactionCodeDomestic.CreditInvoiceObservationPost)
                                    {
                                        invoiceAmount = invoiceAmount > 0 ? decimal.Negate(invoiceAmount) : invoiceAmount;

                                        payment = GetPaymentFromInvoice(invoiceNum, invoiceAmount, actorCompanyId, payments, (tc == (int)LbTransactionCodeDomestic.CreditInvoiceObservationPost), invoiceSeqNr > 0 ? invoiceSeqNr : (int?)null, SoeOriginType.SupplierInvoice);
                                        if (payment != null && !payments.Contains(payment))
                                        {
                                            if (!payments.Any(p => p.PaymentId == payment.PaymentId))
                                                payments.Add(payment);
                                        }
                                        else
                                        {
                                            // Create new empty payment
                                            if (!notFound.ContainsKey(invoiceNum))
                                                notFound.Add(invoiceNum, invoiceAmount);

                                            log.Add(GetText(7077, "Kunde inte hitta betalning för fakturanr") + ": " + invoiceNum);
                                        }
                                    }
                                    else
                                    {
                                        payment = GetPaymentFromInvoice(invoiceNum, invoiceAmount, actorCompanyId, payments, false, invoiceSeqNr > 0 ? invoiceSeqNr : (int?)null, SoeOriginType.SupplierInvoice);
                                        if (payment != null && !payments.Contains(payment))
                                        {
                                            if (!payments.Any(p => p.PaymentId == payment.PaymentId))
                                                payments.Add(payment);
                                        }
                                        else
                                        {
                                            // Create new empty payment
                                            if (!notFound.ContainsKey(invoiceNum))
                                                notFound.Add(invoiceNum, invoiceAmount);

                                            log.Add(GetText(7077, "Kunde inte hitta betalning för fakturanr") + ": " + invoiceNum);
                                        }
                                    }

                                    #endregion
                                    break;

                                case (int)LbTransactionCodeDomestic.CreditInvoiceRestPost:
                                    #region CreditInvoiceRestPost

                                    invoiceAmount = Utilities.GetAmount(Utilities.GetNumeric(Utilities.RemoveLeadingZeros(line), 18, 12));
                                    invoiceAmount = invoiceAmount > 0 ? Decimal.Negate(invoiceAmount) : invoiceAmount;

                                    payment = GetPaymentFromInvoice(invoiceNum, invoiceAmount, actorCompanyId, payments, true, null, SoeOriginType.SupplierInvoice);
                                    if (payment != null && !payments.Contains(payment))
                                    {
                                        if (!payments.Any(p => p.PaymentId == payment.PaymentId))
                                            payments.Add(payment);
                                    }
                                    else
                                    {
                                        // Create new empty payment, can alreday been added by above case 
                                        if (!notFound.ContainsKey(invoiceNum))
                                            notFound.Add(invoiceNum, invoiceAmount);

                                        log.Add(GetText(7077, "Kunde inte hitta betalning för fakturanr") + ": " + invoiceNum);
                                    }

                                    #endregion
                                    break;
                            }

                            break;
                        #endregion
                        case (int)TermGroup_SysPaymentMethod.BGMax:
                            #region BGMax

                            switch (tc)
                            {
                                case (int)BgMaxTransactionCodes.PaymentPost:
                                case (int)BgMaxTransactionCodes.PaymentReductionPost:
                                    #region PaymentPost, PaymentReductionPost

                                    //improve this logic
                                    invoiceNum = Utilities.RemoveLeadingZeros(line.Substring(57, 12));
                                    invoiceAmount = Utilities.GetAmount(Utilities.RemoveLeadingZeros(line.Substring(37, 18)));

                                    payment = GetPaymentFromInvoice(invoiceNum, invoiceAmount, actorCompanyId, payments, false, null, SoeOriginType.CustomerInvoice);
                                    if (payment != null && !payments.Contains(payment))
                                    {
                                        if (!payments.Any(p => p.PaymentId == payment.PaymentId))
                                            payments.Add(payment);
                                    }
                                    else
                                    {
                                        log.Add(GetText(7077, "Kunde inte hitta betalning för fakturanr") + ": " + invoiceNum);
                                    }

                                    #endregion
                                    break;
                            }

                            #endregion
                            break;
                    }
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    log.Add(String.Format(GetText(91555, "Kunde inte läsa rad {0}"), lineNr));
                }
            }

            return payments;
        }
        private List<PaymentRow> FindPaymentByIdentifierCfp(StreamReader sr, int actorCompanyId, ref List<string> log)
        {
            List<PaymentRow> paymentRows = new List<PaymentRow>();

            List<Tuple<int, decimal>> seqList = new List<Tuple<int, decimal>>();
            bool complete = true;
            int lineNr = 0;
            try
            {
                decimal invoiceAmount = 0;
                int invoiceNum = 0;
                while (!sr.EndOfStream)
                {
                    lineNr++;

                    var line = sr.ReadLine();
                    line = line.AddTrailingBlanks(Utilities.BGC_LINE_MAX_LENGTH);
                    if (String.IsNullOrEmpty(line))
                        continue;

                    string tc = line.Substring(0, 4);
                    switch (tc)
                    {
                        case Utilities.DA1_PAYMENT_RECORD_TYPE:
                            if (!complete)
                            {
                                seqList.Add(new Tuple<int, decimal>(invoiceNum, invoiceAmount));
                                invoiceNum = 0;
                                complete = true;
                            }
                            invoiceAmount = Utilities.GetAmount(Utilities.GetNumeric(Utilities.RemoveLeadingZeros(line), 22, 13));
                            break;
                        case Utilities.DA1_SENDERNOTES_RECORD_TYPE:
                            {
                                var tmp = line.Substring(31, 30).Trim();
                                int seq;
                                bool ok = Int32.TryParse(tmp, out seq);
                                if (ok && !string.IsNullOrEmpty(tmp))
                                    invoiceNum = seq;
                                complete = false;
                                break;
                            }
                        case Utilities.DA1_RECEIVER_RECORD_TYPE:
                            complete = false;
                            break;
                        case Utilities.DA1_DEBIT_RECORD_TYPE:
                        case Utilities.DA1_CREDIT_RECORD_TYPE:
                            {
                                var tmp = line.Substring(52, 25).Trim();
                                int seq;
                                bool ok = Int32.TryParse(tmp, out seq);
                                if (ok && !string.IsNullOrEmpty(tmp))
                                    invoiceNum = seq;
                                invoiceAmount = Utilities.GetAmount(Utilities.GetNumeric(Utilities.RemoveLeadingZeros(line), 39, 13));
                                if (tc == Utilities.DA1_CREDIT_RECORD_TYPE)
                                    invoiceAmount = Decimal.Negate(invoiceAmount);
                                seqList.Add(new Tuple<int, decimal>(invoiceNum, invoiceAmount));
                                invoiceNum = 0;
                                complete = true;
                                break;
                            }
                        case Utilities.DA1_CLOSING_RECORD_TYPE:
                            //pgFile.Sections.Last().SummaryPost = new SummaryPost(line);
                            //newSection = true;
                            if (!complete)
                            {
                                seqList.Add(new Tuple<int, decimal>(invoiceNum, invoiceAmount));
                                complete = true;
                                invoiceNum = 0;
                            }
                            break;
                    }
                }

                foreach (Tuple<int, decimal> pair in seqList)
                {
                    PaymentRow paymentRow = GetPaymentRowFromInvoiceCfp(pair.Item1, pair.Item2, actorCompanyId);
                    if (paymentRow != null && !paymentRows.Contains(paymentRow))
                    {
                        if (!paymentRow.InvoiceReference.IsLoaded)
                            paymentRow.InvoiceReference.Load();
                        if (!paymentRows.Any(p => p.PaymentRowId == paymentRow.PaymentRowId))
                            paymentRows.Add(paymentRow);
                    }
                    /*else
                    {
                        // Create new empty payment
                        notFound.Add(pair.Item1.ToString(), pair.Item2);

                        log.Add(GetText(7077, "Kunde inte hitta betalning för fakturanr") + ": " + pair.Item1);
                    }*/

                }


            }


            catch (Exception ex)
            {
                base.LogError(ex, this.log);
                log.Add(String.Format(GetText(91555, "Kunde inte läsa rad {0}"), lineNr));
            }
            return paymentRows;
        }

        private Origin FindOriginByIdentifier(StreamReader sr, TermGroup_SysPaymentMethod type, int actorCompanyId)
        {
            Origin origin = null;
            List<int> suggestedIds = null;
            int invoiceNrLength = 0;

            try
            {
                if (type == TermGroup_SysPaymentMethod.PG)
                {
                    suggestedIds = new List<int>();
                    invoiceNrLength = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.BillingInvoiceNumberLength, 0, actorCompanyId, 0);
                }

                string invoiceNum = string.Empty;
                decimal invoiceAmount = 0M;

                while (!sr.EndOfStream && origin == null)
                {
                    var ln = sr.ReadLine();
                    if (string.IsNullOrEmpty(ln)) continue;
                    var lnTc = ln.Substring(0, 2);
                    int tc;
                    int.TryParse(lnTc, out tc);
                    if (tc < 1)
                        continue;

                    switch ((int)type)
                    {
                        case (int)TermGroup_SysPaymentMethod.LB:

                            switch (tc)
                            {
                                case (int)LbTransactionCodeDomestic.PaymentPost:
                                case (int)LbTransactionCodeDomestic.ReductionPost:
                                case (int)LbTransactionCodeDomestic.CreditInvoiceObservationCompletePost:
                                case (int)LbTransactionCodeDomestic.CreditInvoiceObservationPost:
                                case (int)LbTransactionCodeDomestic.PlusgiroPost:
                                    invoiceNum = ln.Substring(12, 25);
                                    invoiceNum = invoiceNum.Trim();
                                    invoiceAmount = Utilities.GetAmount(Utilities.GetNumeric(Utilities.RemoveLeadingZeros(ln), 37, 12));

                                    if (tc == (int)LbTransactionCodeDomestic.CreditInvoiceObservationCompletePost || tc == (int)LbTransactionCodeDomestic.CreditInvoiceObservationPost)
                                    {
                                        DateTime invoiceDueDate = Utilities.GetDateTime(TermGroup_SysPaymentMethod.LB, ln.Substring(49, 6));
                                        origin = GetOriginFromInvoice(actorCompanyId, invoiceNum, invoiceDueDate, invoiceAmount);
                                    }
                                    else
                                    {
                                        origin = GetOriginFromInvoice(actorCompanyId, invoiceNum, invoiceAmount);
                                    }
                                    break;
                            }
                            break;

                        case (int)TermGroup_SysPaymentMethod.BGMax:
                            switch (tc)
                            {
                                case (int)BgMaxTransactionCodes.PaymentPost:
                                case (int)BgMaxTransactionCodes.PaymentReductionPost:
                                    //invoiceNum = Utilities.RemoveLeadingZeros(ln.Substring(57, 12));
                                    invoiceNum = ln.Substring(12, 24).Trim();
                                    invoiceAmount = Utilities.GetAmount(Utilities.RemoveLeadingZeros(ln.Substring(37, 18)));
                                    origin = GetOriginFromInvoice(actorCompanyId, invoiceNum, invoiceAmount);
                                    break;
                            }
                            break;

                        case (int)TermGroup_SysPaymentMethod.PG:
                            string lnPost = ln.Substring(0, 1);
                            int postCode;
                            int.TryParse(lnPost, out postCode);

                            switch (postCode)
                            {
                                case (int)PgPostType.DebitAmountPost:
                                    invoiceAmount = Utilities.GetAmount(Utilities.RemoveLeadingZeros(ln.Substring(38, 9)));
                                    break;

                                case (int)PgPostType.MessagePost:
                                    if (ln.Length >= 22)
                                        invoiceNum = ln.Substring(2, 22).Trim();
                                    break;
                            }

                            if (invoiceAmount > 0M && !string.IsNullOrEmpty(invoiceNum))
                            {
                                List<Customer> customers = CustomerManager.GetCustomersByCompany(actorCompanyId, onlyActive: true);
                                foreach (Customer customer in customers)
                                {
                                    if (invoiceNum.Length > invoiceNrLength)
                                        continue;

                                    suggestedIds = Utilities.GetInvoiceNrSuggestions(invoiceNum, invoiceNrLength);
                                    foreach (int suggestedId in suggestedIds)
                                    {
                                        origin = GetOriginFromInvoice(suggestedId.ToString(), invoiceAmount, customer.CustomerNr);
                                        if (origin != null)
                                            break;
                                    }

                                    if (origin == null)
                                        suggestedIds.Clear();
                                    else
                                        break;
                                }
                            }
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                base.LogError(ex, this.log);
            }
            return origin;
        }

        private List<Origin> FindOriginsByIdentifier(StreamReader sr, TermGroup_SysPaymentMethod type, int actorCompanyId)
        {
            Origin origin = null;
            List<Origin> origins = new List<Origin>();
            List<int> suggestedIds = null;
            int invoiceNrLength = 0;

            try
            {
                if (type == TermGroup_SysPaymentMethod.PG)
                {
                    suggestedIds = new List<int>();
                    invoiceNrLength = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.BillingInvoiceNumberLength, 0, actorCompanyId, 0);
                }

                string invoiceNum = string.Empty;
                decimal invoiceAmount = 0M;

                while (!sr.EndOfStream)
                {
                    origin = null;
                    var ln = sr.ReadLine();
                    if (string.IsNullOrEmpty(ln)) continue;
                    var lnTc = ln.Substring(0, 2);
                    int tc;
                    int.TryParse(lnTc, out tc);
                    if (type != TermGroup_SysPaymentMethod.SOP && tc < 1)
                        continue;

                    switch ((int)type)
                    {
                        case (int)TermGroup_SysPaymentMethod.LB:

                            switch (tc)
                            {
                                case (int)LbTransactionCodeDomestic.PaymentPost:
                                case (int)LbTransactionCodeDomestic.ReductionPost:
                                case (int)LbTransactionCodeDomestic.CreditInvoiceObservationCompletePost:
                                case (int)LbTransactionCodeDomestic.CreditInvoiceObservationPost:
                                case (int)LbTransactionCodeDomestic.PlusgiroPost:
                                    invoiceNum = ln.Substring(12, 25);
                                    invoiceNum = invoiceNum.Trim();
                                    invoiceAmount = Utilities.GetAmount(Utilities.GetNumeric(Utilities.RemoveLeadingZeros(ln), 37, 12));

                                    if (tc == (int)LbTransactionCodeDomestic.CreditInvoiceObservationCompletePost || tc == (int)LbTransactionCodeDomestic.CreditInvoiceObservationPost)
                                    {
                                        DateTime invoiceDueDate = Utilities.GetDateTime(TermGroup_SysPaymentMethod.LB, ln.Substring(49, 6));
                                        origin = GetOriginFromInvoice(actorCompanyId, invoiceNum, invoiceDueDate, invoiceAmount);
                                    }
                                    else
                                    {
                                        origin = GetOriginFromInvoice(actorCompanyId, invoiceNum, invoiceAmount);
                                    }
                                    break;
                            }
                            break;

                        case (int)TermGroup_SysPaymentMethod.BGMax:
                            switch (tc)
                            {
                                case (int)BgMaxTransactionCodes.PaymentPost:
                                case (int)BgMaxTransactionCodes.PaymentReductionPost:
                                    //invoiceNum = Utilities.RemoveLeadingZeros(ln.Substring(57, 12));
                                    invoiceNum = ln.Substring(12, 24).Trim();
                                    invoiceAmount = Utilities.GetAmount(Utilities.RemoveLeadingZeros(ln.Substring(37, 18)));
                                    if (tc == (int)BgMaxTransactionCodes.PaymentReductionPost)
                                        invoiceAmount = Decimal.Negate(invoiceAmount);

                                    origin = GetOriginFromInvoice(actorCompanyId, invoiceNum, true);
                                    break;
                            }
                            break;
                        case (int)TermGroup_SysPaymentMethod.TOTALIN:
                            ln = ln.AddTrailingBlanks(Utilities.BGC_LINE_MAX_LENGTH);
                            switch (tc)
                            {
                                case (int)TotalinTransactionCodes.PaymentPost:
                                case (int)TotalinTransactionCodes.PaymentReductionPost:

                                    //invoiceNum = Utilities.RemoveLeadingZeros(ln.Substring(57, 12));

                                    invoiceNum = ln.Substring(2, 35).Trim();
                                    invoiceAmount = Utilities.GetAmount(Utilities.RemoveLeadingZeros(ln.Substring(37, 15)));
                                    if (!string.IsNullOrEmpty(invoiceNum))
                                    {
                                        origin = GetOriginFromInvoice(actorCompanyId, invoiceNum, true);
                                    }
                                    break;

                                case (int)TotalinTransactionCodes.ReferenceNumberPost:
                                case (int)TotalinTransactionCodes.InformationPost:

                                    invoiceNum = ln.Substring(2, 35).Trim();

                                    if (!string.IsNullOrEmpty(invoiceNum))
                                    {
                                        origin = GetOriginFromInvoice(actorCompanyId, invoiceNum, true);
                                    }
                                    break;
                            }
                            break;
                        case (int)TermGroup_SysPaymentMethod.SOP:
                            string[] parts = ln.Split(new char[] { ';' });

                            invoiceNum = Utilities.RemoveLeadingZeros(parts[1]);
                            invoiceAmount = Convert.ToDecimal(Utilities.RemoveLeadingZeros(parts[4]));
                            origin = GetOriginFromInvoiceSOP(invoiceNum, actorCompanyId);
                            break;

                        case (int)TermGroup_SysPaymentMethod.PG:
                            string lnPost = ln.Substring(0, 1);
                            int postCode;
                            int.TryParse(lnPost, out postCode);

                            switch (postCode)
                            {
                                case (int)PgPostType.DebitAmountPost:
                                    invoiceAmount = Utilities.GetAmount(Utilities.RemoveLeadingZeros(ln.Substring(38, 9)));
                                    break;

                                case (int)PgPostType.MessagePost:
                                    if (ln.Length >= 22)
                                        invoiceNum = ln.Substring(2, 22).Trim();
                                    break;
                            }

                            if (invoiceAmount > 0M && !string.IsNullOrEmpty(invoiceNum))
                            {
                                List<Customer> customers = CustomerManager.GetCustomersByCompany(actorCompanyId, onlyActive: true);
                                foreach (Customer customer in customers)
                                {
                                    if (invoiceNum.Length > invoiceNrLength)
                                        continue;

                                    suggestedIds = Utilities.GetInvoiceNrSuggestions(invoiceNum, invoiceNrLength);
                                    foreach (int suggestedId in suggestedIds)
                                    {
                                        origin = GetOriginFromInvoice(suggestedId.ToString(), invoiceAmount, customer.CustomerNr);
                                        if (origin != null)
                                            break;
                                    }

                                    if (origin == null)
                                        suggestedIds.Clear();
                                    else
                                        break;
                                }
                            }
                            break;
                    }

                    if (origin != null && !origins.Any(o => o.OriginId == origin.OriginId))
                        origins.Add(origin);
                }
            }
            catch (Exception ex)
            {
                base.LogError(ex, this.log);
            }
            return origins;
        }

        #endregion

        #region Help methods

        private bool HasExistingPayments(Origin origin)
        {
            using (CompEntities entities = new CompEntities())
            {
                Origin originalOrigin = OriginManager.GetOrigin(entities, origin.OriginId);
                if (originalOrigin == null)
                    return false;

                if (!originalOrigin.PaymentReference.IsLoaded)
                    originalOrigin.PaymentReference.Load();

                if (originalOrigin.Payment != null)
                    return true;
            }

            return false;
        }

        #endregion
    }
}
