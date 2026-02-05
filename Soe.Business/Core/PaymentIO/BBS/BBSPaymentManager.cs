using SoftOne.Soe.Business.Core.PaymentIO.BBS;
using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Util.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Transactions;

namespace SoftOne.Soe.Business.Core.PaymentIO.BBS
{
    public class BBSPaymentManager : ManagerBase
    {
        public BBSPaymentManager(ParameterObject parameterObject) : base(parameterObject) { }

        public ActionResult Import(StreamReader sr, int actorCompanyId, int batchId, int paymentMethodId, int paymentImportId)
        {
            var bbsFile = new BBSFile(sr);
            return ConvertStreamToEntity(bbsFile, actorCompanyId, batchId, paymentMethodId, paymentImportId);
        }

        public ActionResult ConvertStreamToEntity(BBSFile file, int actorCompanyId, int batchId, int paymentMethodId, int paymentImportId)
        {
            var result = new ActionResult(true);
            var paymentImportIOToAdd = new List<PaymentImportIO>();
            int status = 0;
            int state = 0;
            int type = 1;

            using (var entities = new CompEntities())
            {
                var paymentMethod = PaymentManager.GetPaymentMethod(entities, paymentMethodId, actorCompanyId, true);
                var paymentImport = CreateImport(entities, paymentMethod, paymentImportId, actorCompanyId);

                foreach (var row in file.PaymentRows)
                {
                    status = (int)ImportPaymentIOStatus.Unknown;
                    state = (int)ImportPaymentIOState.Open;

                    var invoiceDto = InvoiceManager.GetCustomerInvoiceAmountExtractNumerics(entities, row.InvoiceNumber);

                    if (invoiceDto != null)
                    {
                        status = (int)ImportPaymentIOStatus.Match;
                        state = (int)ImportPaymentIOState.Open;

                        type = invoiceDto.TotalAmount >= 0 ? (int)TermGroup_BillingType.Debit : (int)TermGroup_BillingType.Credit;

                        bool isPartlyPaid = row.Amount < invoiceDto.TotalAmount;
                        if (isPartlyPaid)
                        {
                            status = (int)ImportPaymentIOStatus.PartlyPaid;
                        }

                        bool isRest = row.Amount > invoiceDto.TotalAmount;
                        if (isRest)
                        {
                            status = (int)ImportPaymentIOStatus.Rest;
                        }

                        bool isFullyPayed = invoiceDto.FullyPayed;
                        if (isFullyPayed)
                            status = (int)ImportPaymentIOStatus.Paid;
                    }

                    var paymentImportIO = new PaymentImportIO
                    {
                        ActorCompanyId = actorCompanyId,
                        BatchNr = batchId,
                        Type = type,
                        CustomerId = invoiceDto?.ActorId ?? 0,
                        Customer = invoiceDto != null ? StringUtility.Left(invoiceDto.ActorName, 50) : StringUtility.Left(row.InvoiceNumber, 50),
                        InvoiceId = invoiceDto?.InvoiceId ?? 0,
                        InvoiceNr = invoiceDto?.InvoiceNr ?? row.InvoiceNumber,
                        InvoiceAmount = invoiceDto != null ? invoiceDto.TotalAmount - invoiceDto.PaidAmount : 0,
                        RestAmount = invoiceDto != null ? invoiceDto.TotalAmount - invoiceDto.PaidAmount - row.Amount : 0,
                        PaidAmount = row.Amount,
                        Currency = "NOK",
                        InvoiceDate = invoiceDto?.DueDate ?? null,
                        PaidDate = row.PaymentDate,
                        MatchCodeId = 0,
                        Status = status,
                        State = state,
                        ImportType = (int)ImportPaymentType.CustomerPayment,
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

            return result;
        }
        private PaymentImport CreateImport(CompEntities entities, PaymentMethod method, int paymentImportId, int actorCompanyId)
        {
            var existingImport = PaymentManager.GetPaymentImport(entities, paymentImportId, actorCompanyId);

            if (existingImport.Type == method.PaymentMethodId)
            {
                return existingImport;
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
            return PaymentManager.GetPaymentImport(entities, saveResult.IntegerValue, actorCompanyId);
        }

    }
}
