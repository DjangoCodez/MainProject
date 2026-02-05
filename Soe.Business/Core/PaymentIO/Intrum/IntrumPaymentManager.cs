using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Transactions;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Util.Exceptions;

namespace SoftOne.Soe.Business.Core.PaymentIO.Intrum
{
    public class IntrumPaymentManager : ManagerBase
    {
        #region Variables

        // Create a logger for use in this class
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #endregion

        #region Ctor

        public IntrumPaymentManager(ParameterObject parameterObject) : base(parameterObject) { }

        #endregion

        #region Import

        public ActionResult Import(StreamReader sr, int actorCompanyId, SoeOriginType originType, string fileName, int paymentMethodId, ref List<string> log, int paymentImportId, int batchId, ImportPaymentType importType)
        {
            var result = new ActionResult(true);
            var files = new List<IntrumPaymentFile>();

            try
            {
                #region Parse

                if (Path.GetExtension(fileName).ToLower() != ".xml")
                {
                    return new ActionResult(7593, GetText(8077, "Importen kunde inte slutföras. Kontrollera att du har valt rätt fil eller betalningsmetod."));
                }

                string incFile = sr.ReadToEnd();
                XDocument xdoc = XDocument.Parse(incFile);

                XElement documentRoot =
                   (from e in xdoc.Elements()
                    where e.Name.LocalName == "SLSPaymentReport"
                    select e).FirstOrDefault();

                if (documentRoot == null)
                {
                    return new ActionResult(8176, "Kan inte läsa från XML fil");
                }

                var invoiceSet = XmlUtil.GetDescendantElement(documentRoot, "InvoiceSet");

                List<XElement> invoices =
                      (from e in invoiceSet.Elements()
                       where e.Name.LocalName == "Invoice"
                       select e).ToList();

                foreach (XElement invoice in invoices)
                {
                    var file = CreateFile(invoice);

                    if (file != null)
                    {
                        files.Add(file);
                    }
                }

                if (files.Any())
                {
                    result = ConvertStreamEntity(files, actorCompanyId, batchId, paymentImportId, originType, importType);
                }
                else
                {
                    return new ActionResult(7598, GetText(7598, "Hittade inga betalningar att importera"));
                }

                #endregion

            }
            catch (XmlException ex)
            {
                base.LogError(ex, this.log);
                return new ActionResult(8176, "Kan inte läsa från XML fil");
            }
            catch (Exception ex)
            {
                base.LogError(ex, this.log);
                result.Exception = ex;
            }

            return result;
        }

        #endregion

        #region Help methods

        private IntrumPaymentFile CreateFile(XElement invoiceElement)
        {

           //G 6590 Godkänt avdrag:                     50 / ApprovedDeductionAmount
           //D 1612 Direktbetalning:                    51 / PaidAmount
           //I 1951 Intrum:                             54 / PaidAmount
           //F 1517 Kundförlust:                        63 / ApprovedDeductionAmount

            string invoiceNr = XmlUtil.GetAttributeStringValue(invoiceElement, "InvoiceNo");

            XElement invoiceHeader =
                (from e in invoiceElement.Elements()
                 where e.Name.LocalName == "InvoiceHeader"
                 select e).FirstOrDefault();

            string customerNr = XmlUtil.GetAttributeStringValue(invoiceHeader, "InvoiceCustomerNo");
            string currenyCode = XmlUtil.GetAttributeStringValue(invoiceHeader, "CurrencyCode");

            XElement payment =
                (from e in invoiceElement.Elements()
                 where e.Name.LocalName == "Payment"
                 select e).FirstOrDefault();

            var reference = XmlUtil.GetAttributeStringValue(payment, "InvoiceGiroRefference");
            var paymentDate = XmlUtil.GetAttributeDateTimeValue(payment, "PaymentDate", "yyyy-MM-dd");
            var paidAmountStr = XmlUtil.GetAttributeStringValue(payment, "PaidAmount");
            var paymentTransactionType = XmlUtil.GetAttributeStringValue(payment, "PaymentTransactionType");

            if (paymentTransactionType == "50" || (paymentTransactionType == "63"))
            {
                paidAmountStr = XmlUtil.GetAttributeStringValue(payment, "ApprovedDeductionAmount");
            }

            paidAmountStr = paidAmountStr?.Replace(".", ",");
            decimal amount = Convert.ToDecimal(paidAmountStr?.Trim());

            if (amount == 0)
                return null;

            return new IntrumPaymentFile(paymentDate, amount, invoiceNr, reference, currenyCode, customerNr, int.Parse(paymentTransactionType) );
        }


        private ActionResult ConvertStreamEntity(List<IntrumPaymentFile> files, int actorCompanyId, int batchId, int paymentImportId, SoeOriginType originType, ImportPaymentType importType)
        {
            var result = new ActionResult(true);
            var paymentImportIOToAdd = new List<PaymentImportIO>();
            int status = 0;
            int state = 0;
            int type = 1;

            using (var entities = new CompEntities())
            {
                var transactionTypes = files.Select(x => x.PaymentTransactionType).Distinct().ToList();
                var paymentImports  = CreateImportForIntrumTypes(entities, paymentImportId, actorCompanyId, originType, transactionTypes);

                foreach (var filegroup in files.GroupBy(f => f.PaymentTransactionType))
                {
                    paymentImportIOToAdd.Clear();
                    var paymentImport = paymentImports[filegroup.First().PaymentTransactionType];
                    batchId = paymentImport.BatchId;

                    if (paymentImport == null)
                        continue;

                    foreach (var file in filegroup)
                    {
                        status = (int)ImportPaymentIOStatus.Unknown;
                        state = (int)ImportPaymentIOState.Open;

                        var invoiceDto = InvoiceManager.GetCustomerInvoiceAmount(entities, invoiceNr: file.InvoiceNr);

                        if (invoiceDto != null)
                        {
                            status = (int)ImportPaymentIOStatus.Match;
                            state = (int)ImportPaymentIOState.Open;

                            type = invoiceDto.TotalAmount >= 0 ? (int)TermGroup_BillingType.Debit : (int)TermGroup_BillingType.Credit;

                            bool isPartlyPaid = Utilities.GetAmount(file.Amount) < invoiceDto.TotalAmount;
                            if (isPartlyPaid)
                            {
                                status = (int)ImportPaymentIOStatus.PartlyPaid;
                            }

                            bool isRest = Utilities.GetAmount(file.Amount) > invoiceDto.TotalAmount;
                            if (isRest)
                            {
                                status = (int)ImportPaymentIOStatus.Rest;
                            }

                            bool isFullyPayed = invoiceDto.FullyPayed;
                            if (isFullyPayed)
                            {
                                if (importType == ImportPaymentType.CustomerPayment)
                                    status = (int)ImportPaymentIOStatus.Unknown;
                                else
                                    status = (int)ImportPaymentIOStatus.Paid;
                            }
                        }

                        var paymentImportIO = new PaymentImportIO
                        {
                            ActorCompanyId = actorCompanyId,
                            BatchNr = batchId,
                            Type = type,
                            CustomerId = invoiceDto?.ActorId ?? 0,
                            Customer = invoiceDto != null ? StringUtility.Left(invoiceDto.ActorName, 50) : StringUtility.Left(file.Reference, 50),
                            InvoiceId = invoiceDto?.InvoiceId ?? 0,
                            InvoiceNr = invoiceDto?.InvoiceNr ?? file.InvoiceNr,
                            InvoiceAmount = invoiceDto != null ? invoiceDto.TotalAmount - invoiceDto.PaidAmount : 0,
                            RestAmount = invoiceDto != null ? invoiceDto.TotalAmount - invoiceDto.PaidAmount - file.Amount : 0,
                            PaidAmount = file.Amount,
                            Currency = file.CurrencyCode,
                            InvoiceDate = invoiceDto?.DueDate ?? null,
                            PaidDate = file.PaidDate,
                            MatchCodeId = 0,
                            Status = status,
                            State = state,
                            ImportType = (int)importType,
                        };

                        paymentImportIOToAdd.Add(paymentImportIO);
                    }

                    int numberOfPayments = 1;

                    foreach (var paymentIO in paymentImportIOToAdd)
                    {
                        using (var transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                        {
                            if (result.Success)
                            {
                                entities.PaymentImportIO.AddObject(paymentIO);

                                result = SaveEntityItem(entities, paymentIO, transaction);

                                if (result.Success)
                                {
                                    paymentImport = PaymentManager.GetPaymentImport(entities, paymentImport.PaymentImportId, actorCompanyId);

                                    paymentImport.TotalAmount = paymentImport.TotalAmount + paymentIO.PaidAmount.Value;
                                    paymentImport.NumberOfPayments = numberOfPayments++;

                                    result = PaymentManager.UpdatePaymentImportHead(entities, paymentImport, paymentImportId, actorCompanyId);

                                    transaction.Complete();
                                }
                                else
                                {
                                    // Set result
                                    result.ErrorNumber = (int)ActionResultSave.NothingSaved;
                                    result.ErrorMessage = string.Format("Faktura med nr {0} är felaktig, importen är avbruten!", paymentIO.InvoiceNr);
                                }
                            }
                            else
                            {
                                transaction.Complete();
                            }
                        }
                    }
                }

                result.Keys = paymentImports.Values.Select(x => x.PaymentImportId).ToList();
                result.Strings = paymentImports.Values.Select(x => x.BatchId.ToString()).ToList();
            }
            return result;
        }


        private Dictionary<int, PaymentImport> CreateImportForIntrumTypes(CompEntities entities, int paymentImportId, int actorCompanyId, SoeOriginType originType, List<int> usedTransactionTypes)
        {
            var result = new Dictionary<int, PaymentImport>();
            var intrumMethods = PaymentManager.GetPaymentMethodsBySysId(entities, TermGroup_SysPaymentMethod.Intrum, originType, actorCompanyId).Where(x=> usedTransactionTypes.Contains(x.TransactionCode.GetValueOrDefault()) ) ;
            var existingImport = PaymentManager.GetPaymentImport(entities, paymentImportId, actorCompanyId);

            foreach (var method in intrumMethods)
            {
                if (existingImport.Type == method.PaymentMethodId)
                {
                    result.Add(method.TransactionCode.GetValueOrDefault(), existingImport);
                    continue;
                }

                var dto = new PaymentImportDTO
                {
                    ActorCompanyId = existingImport.ActorCompanyId,
                    Filename = existingImport.Filename,
                    ImportType = (ImportPaymentType)existingImport.ImportType,
                    SysPaymentTypeId = (TermGroup_SysPaymentType)existingImport.SysPaymentTypeId,
                    ImportDate = existingImport.ImportDate,
                    PaymentLabel = existingImport.PaymentLabel,
                    Type = method.PaymentMethodId,
                };

                var saveResult = PaymentManager.SavePaymentImportHeader(entities, actorCompanyId, dto);
                if (!saveResult.Success)
                {
                    throw new ActionFailedException(saveResult.ErrorNumber, saveResult.ErrorMessage);
                }
                var savedImport = PaymentManager.GetPaymentImport(entities, saveResult.IntegerValue, actorCompanyId);
                result.Add(method.TransactionCode.GetValueOrDefault(), savedImport);
            }

            return result;
        }
        #endregion
    }
}
