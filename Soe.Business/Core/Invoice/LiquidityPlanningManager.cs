using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace SoftOne.Soe.Business.Core
{
    public class LiquidityPlanningManager : ManagerBase
    {
        #region Variables

        // Create a logger for use in this class
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #endregion

        #region Ctor

        public LiquidityPlanningManager(ParameterObject parameterObject) : base(parameterObject) { }

        #endregion

        public List<LiquidityPlanningDTO> GetLiquidityPlanning(int actorCompanyId, DateTime from, DateTime to, DateTime? exclusionDate, decimal incomingBalance, bool unpaid, bool paidunchecked, bool paidchecked)
        {
            List<LiquidityPlanningDTO> dtos = new List<LiquidityPlanningDTO>();

            if (!unpaid && !paidunchecked && !paidchecked)
                return dtos;

            var balanceTerm = GetText(1391, (int)TermGroup.General, "Ingående balans");
            var customerInvoiceTerm = GetText(4528, (int)TermGroup.General, "Kundfaktura");
            var supplierInvoiceTerm = GetText(31, (int)TermGroup.General, "Leverantörsfaktura");
            var manualTerm = GetText(5611, (int)TermGroup.AngularEconomy, "Manuell transaktion");
            var preliminary = GetText(5981, (int)TermGroup.General, "Preliminär");
            //var totalTerm = "Total";

            using (CompEntities entities = new CompEntities())
            {
                // Set timeout
                entities.CommandTimeout = 300;

                if (entities.Connection.State != ConnectionState.Open)
                    entities.Connection.Open();

                // Get invoices
                var invoices = (from i in entities.Invoice
                                        .Include("Origin")
                                        .Include("Actor.Customer")
                                        .Include("Actor.Supplier")
                                        .Include("PaymentRow")
                                where
                                i.Origin.ActorCompanyId == actorCompanyId &&
                                (i.Origin.Type == (int)SoeOriginType.CustomerInvoice || i.Origin.Type == (int)SoeOriginType.SupplierInvoice) &&
                                (i.DueDate.HasValue && i.DueDate <= to) &&
                                i.Origin.Status != (int)SoeOriginStatus.Cancel &&
                                i.State == (int)SoeEntityState.Active
                                orderby i.DueDate
                                select i).ToList();

                if (unpaid && paidunchecked && !paidchecked)
                {
                    invoices = invoices.Where(i => i.Origin.Type == (int)SoeOriginType.CustomerInvoice ?
                    (!i.FullyPayed || i.PaymentRow.Count == 0) || (i.PaymentRow.Any(p => p.State == (int)SoeEntityState.Active && (p.Status == (int)SoePaymentStatus.ManualPayment || p.Status == (int)SoePaymentStatus.Verified || p.Status == (int)SoePaymentStatus.Error) && p.VoucherHeadId == null))
                    :
                    (!i.FullyPayed || i.PaymentRow.Count == 0) || (i.PaymentRow.Any(p => p.State == (int)SoeEntityState.Active && (p.Status == (int)SoePaymentStatus.Pending || p.Status == (int)SoePaymentStatus.Verified || p.Status == (int)SoePaymentStatus.Error) && p.VoucherHeadId == null))
                    ).ToList();
                }
                else if (unpaid && !paidunchecked && paidchecked)
                {
                    invoices = invoices.Where(i => i.Origin.Type == (int)SoeOriginType.CustomerInvoice ?
                    (!i.FullyPayed || i.PaymentRow.Count == 0) || (i.PaymentRow.All(p => p.State == (int)SoeEntityState.Active && (p.Status == (int)SoePaymentStatus.Checked || p.Status == (int)SoePaymentStatus.Exported) && p.VoucherHeadId != null))
                    :
                    (!i.FullyPayed || i.PaymentRow.Count == 0) || (i.PaymentRow.All(p => p.State == (int)SoeEntityState.Active && (p.Status == (int)SoePaymentStatus.Checked) || (p.Status == (int)SoePaymentStatus.Cancel) && p.VoucherHeadId != null))
                    ).ToList();
                }
                else if (unpaid && !paidunchecked && !paidchecked)
                {
                    invoices = invoices.Where(i => !i.FullyPayed).ToList();
                }
                else if (!unpaid && paidunchecked && paidchecked)
                {
                    invoices = invoices.Where(i => i.FullyPayed).ToList();
                }
                else if (!unpaid && !paidunchecked && paidchecked)
                {
                    invoices = invoices.Where(i => i.Origin.Type == (int)SoeOriginType.CustomerInvoice ?
                    (i.FullyPayed && i.PaymentRow.All(p => p.State == (int)SoeEntityState.Active && (p.Status == (int)SoePaymentStatus.Checked || p.Status == (int)SoePaymentStatus.Exported) && p.VoucherHeadId != null))
                    :
                    (i.FullyPayed && i.PaymentRow.All(p => p.State == (int)SoeEntityState.Active && (p.Status == (int)SoePaymentStatus.Checked) || (p.Status == (int)SoePaymentStatus.Cancel) && p.VoucherHeadId != null))
                    ).ToList();
                }
                else if (!unpaid && paidunchecked && !paidchecked)
                {
                    invoices = invoices.Where(i => i.Origin.Type == (int)SoeOriginType.CustomerInvoice ?
                    (i.FullyPayed && i.PaymentRow.Any(p => p.State == (int)SoeEntityState.Active && (p.Status == (int)SoePaymentStatus.ManualPayment || p.Status == (int)SoePaymentStatus.Verified || p.Status == (int)SoePaymentStatus.Error) && p.VoucherHeadId == null))
                    :
                    (i.FullyPayed && i.PaymentRow.Any(p => p.State == (int)SoeEntityState.Active && (p.Status == (int)SoePaymentStatus.Pending || p.Status == (int)SoePaymentStatus.Verified || p.Status == (int)SoePaymentStatus.Error) && p.VoucherHeadId == null))
                    ).ToList();
                }


                var transactions = (from t in entities.LiquidityPlanningTransaction
                                    where
                                    t.ActorCompanyId == actorCompanyId &&
                                    t.TransactionDate <= to &&
                                    t.State == (int)SoeEntityState.Active
                                    orderby t.TransactionDate
                                    select t).ToList();

                if (exclusionDate.HasValue)
                {
                    var inv = invoices.Where(i => i.DueDate.Value >= exclusionDate.Value.Date && i.DueDate.Value < from.Date);
                    var tra = transactions.Where(t => t.TransactionDate >= exclusionDate.Value && t.TransactionDate < from.Date);
                    var count = inv.Count();
                    if (paidunchecked)
                    {
                        // Remove "closed" invocies without payments
                        inv = inv.Where(i => !i.FullyPayed || i.PaymentRow.Any(p => p.State == (int)SoeEntityState.Active));

                        incomingBalance += (inv.Sum(i => i.Origin.Type == (int)SoeOriginType.CustomerInvoice ? i.TotalAmount - i.PaymentRow.Where(p => p.State == (int)SoeEntityState.Active && (p.Status == (int)SoePaymentStatus.ManualPayment || p.Status == (int)SoePaymentStatus.Verified || p.Status == (int)SoePaymentStatus.Error) && p.VoucherHeadId != null).Sum(p => p.Amount) : ((i.TotalAmount - i.PaymentRow.Where(p => p.State == (int)SoeEntityState.Active && (p.Status == (int)SoePaymentStatus.Pending || p.Status == (int)SoePaymentStatus.Verified || p.Status == (int)SoePaymentStatus.Error) && p.VoucherHeadId != null).Sum(p => p.Amount)) * -1)));
                    }
                    else
                    {
                        incomingBalance += (inv.Sum(i => i.Origin.Type == (int)SoeOriginType.CustomerInvoice ? i.TotalAmount - i.PaidAmount : ((i.TotalAmount - i.PaidAmount) * -1)));
                    }
                }
                else
                {
                    var inv = invoices.Where(i => i.DueDate.Value < from.Date);
                    var tra = transactions.Where(t => t.TransactionDate < from.Date);
                    var count = inv.Count();
                    if (paidunchecked)
                    {
                        // Remove "closed" invocies without payments
                        inv = inv.Where(i => !i.FullyPayed || i.PaymentRow.Any(p => p.State == (int)SoeEntityState.Active));
                        incomingBalance += (inv.Sum(i => i.Origin.Type == (int)SoeOriginType.CustomerInvoice ? i.TotalAmount - i.PaymentRow.Where(p => p.State == (int)SoeEntityState.Active && p.VoucherHeadId != null).Sum(p => p.Amount) : ((i.TotalAmount - i.PaymentRow.Where(p => p.State == (int)SoeEntityState.Active && p.VoucherHeadId != null).Sum(p => p.Amount)) * -1)));
                    }
                    else
                    {
                        incomingBalance += (inv.Sum(i => i.Origin.Type == (int)SoeOriginType.CustomerInvoice ? i.TotalAmount - i.PaymentRow.Where(p => p.State == (int)SoeEntityState.Active && p.VoucherHeadId != null).Sum(p => p.Amount) : ((i.TotalAmount - i.PaymentRow.Where(p => p.State == (int)SoeEntityState.Active && p.VoucherHeadId != null).Sum(p => p.Amount)) * -1)));
                    }
                }

                for (var date = from; date <= to; date = date.AddDays(1))
                {
                    var invoicesByDate = invoices.Where(i => i.DueDate.Value == date.Date);
                    var transactionsByDate = transactions.Where(t => t.TransactionDate == date.Date);

                    if (invoicesByDate.Any() || transactionsByDate.Any())
                    {
                        var balanceRow = new LiquidityPlanningDTO()
                        {
                            InvoiceId = 0,
                            Date = date,
                            OriginType = SoeOriginType.None,
                            TransactionType = LiquidityPlanningTransactionType.IncomingBalance,
                            TransactionTypeName = balanceTerm,
                            Specification = "",
                            Total = incomingBalance,
                            ValueIn = 0,
                            ValueOut = 0,
                        };
                        dtos.Add(balanceRow);

                        decimal balanceIn = 0;
                        decimal balanceOut = 0;
                        foreach (var invoice in invoicesByDate)
                        {
                            var row = new LiquidityPlanningDTO()
                            {
                                InvoiceId = invoice.InvoiceId,
                                InvoiceNr = invoice.InvoiceNr,
                                Date = date,
                                OriginType = (SoeOriginType)invoice.Origin.Type,
                                ValueIn = 0,
                                ValueOut = 0,
                            };

                            if (invoice.Origin.Type == (int)SoeOriginType.CustomerInvoice)
                            {
                                row.TransactionType = LiquidityPlanningTransactionType.CustomerInvoice;
                                row.TransactionTypeName = customerInvoiceTerm;
                                row.Specification = (invoice.SeqNr.HasValue && invoice.SeqNr.Value > 0 ? invoice.SeqNr.Value.ToString() : preliminary) + " - " + invoice.Actor.Customer.CustomerNr + " " + invoice.Actor.Customer.Name;

                                var paidAmount = invoice.PaymentRow.Where(p => (p.Status == (int)SoePaymentStatus.Checked || p.Status == (int)SoePaymentStatus.Exported) && p.VoucherHeadId != null).Sum(p => p.Amount);

                                if (invoice.BillingType == (int)TermGroup_BillingType.Debit)
                                    row.ValueIn = invoice.TotalAmount - paidAmount;
                                else
                                    row.ValueOut = Decimal.Negate(invoice.TotalAmount - paidAmount);

                                row.Total = row.ValueIn - row.ValueOut;
                            }
                            else
                            {
                                row.TransactionType = LiquidityPlanningTransactionType.SupplierInvoice;
                                row.TransactionTypeName = supplierInvoiceTerm;
                                row.Specification = (invoice.SeqNr.HasValue && invoice.SeqNr.Value > 0 ? invoice.SeqNr.Value.ToString() : preliminary) + " - " + invoice.Actor.Supplier.SupplierNr + " " + invoice.Actor.Supplier.Name;

                                var paidAmount = invoice.PaymentRow.Where(p => p.Status == (int)SoePaymentStatus.Checked && p.VoucherHeadId != null).Sum(p => p.Amount);

                                if (invoice.BillingType == (int)TermGroup_BillingType.Debit)
                                    row.ValueOut = invoice.TotalAmount - paidAmount;
                                else
                                    row.ValueIn = Decimal.Negate(invoice.TotalAmount - paidAmount);

                                row.Total = row.ValueIn - row.ValueOut;
                            }
                            dtos.Add(row);

                            incomingBalance += row.Total;
                            balanceIn += row.ValueIn;
                            balanceOut += row.ValueOut;
                        }

                        foreach (var transaction in transactionsByDate)
                        {
                            var row = new LiquidityPlanningDTO()
                            {
                                LiquidityPlanningTransactionId = transaction.LiquidityPlanningTransactionId,
                                InvoiceId = 0,
                                InvoiceNr = "",
                                Date = transaction.TransactionDate,
                                OriginType = SoeOriginType.None,
                                TransactionType = (LiquidityPlanningTransactionType)transaction.TransactionType,
                                TransactionTypeName = manualTerm,
                                Specification = transaction.Description,
                                Created = transaction.Created,
                                CreatedBy = transaction.CreatedBy,
                                Modified = transaction.Modified,
                                ModifiedBy = transaction.ModifiedBy,
                            };

                            if (transaction.Amount > 0)
                            {
                                row.ValueIn = transaction.Amount;
                                row.Total = transaction.Amount;
                            }
                            else
                            {
                                row.ValueOut = Decimal.Negate(transaction.Amount);
                                row.Total = transaction.Amount;
                            }
                            dtos.Add(row);

                            incomingBalance += row.Total;
                            balanceIn += row.ValueIn;
                            balanceOut += row.ValueOut;
                        }
                    }
                }
            }

            return dtos.OrderBy(d => d.Date).ToList();
        }

        public List<LiquidityPlanningDTO> GetLiquidityPlanningv2(int actorCompanyId, DateTime from, DateTime to, DateTime? exclusionDate, decimal incomingBalance, bool unpaid, bool paidunchecked, bool paidchecked)
        {
            List<LiquidityPlanningDTO> dtos = new List<LiquidityPlanningDTO>();

            if (!unpaid && !paidunchecked && !paidchecked)
                return dtos;
            using var entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            var balanceTerm = GetText(1391, (int)TermGroup.General, "Ingående balans");
            var customerInvoiceTerm = GetText(4528, (int)TermGroup.General, "Kundfaktura");
            var supplierInvoiceTerm = GetText(31, (int)TermGroup.General, "Leverantörsfaktura");
            var manualTerm = GetText(5611, (int)TermGroup.AngularEconomy, "Manuell transaktion");
            var preliminary = GetText(5981, (int)TermGroup.General, "Preliminär");

            // Get invoices
            var items = (from i in entitiesReadOnly.LiquidityPlanningView
                         where i.ActorCompanyId == actorCompanyId &&
                         (i.DueDate.HasValue && i.DueDate <= to)
                         orderby i.DueDate
                         select i);

            var invoices = items.Where(i => i.IsTransaction.Value == false);

            if (unpaid && paidunchecked && !paidchecked)
            {
                invoices = invoices.Where(i => !i.FullyPayed.Value || i.HasUncheckedPayments > 0);
            }
            else if (unpaid && !paidunchecked && paidchecked)
            {
                invoices = invoices.Where(i => !i.FullyPayed.Value || (i.HasUncheckedPayments == 0 && i.HasCheckedPayments > 0));
            }
            else if (unpaid && !paidunchecked && !paidchecked)
            {
                invoices = invoices.Where(i => !i.FullyPayed.Value);
            }
            else if (!unpaid && paidunchecked && paidchecked)
            {
                invoices = invoices.Where(i => i.FullyPayed.Value);
            }
            else if (!unpaid && !paidunchecked && paidchecked)
            {
                invoices = invoices.Where(i => i.FullyPayed.Value && i.HasUncheckedPayments == 0 && i.HasCheckedPayments > 0);
            }
            else if (!unpaid && paidunchecked && !paidchecked)
            {
                invoices = invoices.Where(i => i.FullyPayed.Value && i.HasUncheckedPayments > 0);
            }


            var transactions = items.Where(i => i.IsTransaction.HasValue && i.IsTransaction.Value).ToList();

            if (exclusionDate.HasValue)
            {
                var date = exclusionDate.Value.Date;
                var inv = invoices.Where(i => i.DueDate.Value >= date && i.DueDate.Value < from);
                incomingBalance += inv.Count() > 0 ? (inv.Sum(i => i.OriginType == (int)SoeOriginType.CustomerInvoice ? i.TotalAmountCurrency - i.PaidAmount : ((i.TotalAmountCurrency - i.PaidAmount) * -1))/* + tra.Sum(t => t.TotalAmountCurrency)*/) : 0;
            }
            else
            {
                var inv = invoices.Where(i => i.DueDate.Value < from.Date);
                incomingBalance += inv.Count() > 0 ? (inv.Sum(i => i.OriginType == (int)SoeOriginType.CustomerInvoice ? i.TotalAmountCurrency - i.PaidAmount : ((i.TotalAmountCurrency - i.PaidAmount) * -1))/* + tra.Sum(t => t.TotalAmountCurrency)*/) : 0;
            }

            var filteredInvoices = invoices.ToList();

            for (var date = from; date <= to; date = date.AddDays(1))
            {
                var invoicesByDate = filteredInvoices.Where(i => i.DueDate.Value == date.Date);
                var transactionsByDate = transactions.Where(t => t.DueDate == date.Date);

                if (invoicesByDate.Any() || transactionsByDate.Any())
                {
                    var balanceRow = new LiquidityPlanningDTO()
                    {
                        InvoiceId = 0,
                        Date = date,
                        OriginType = SoeOriginType.None,
                        TransactionType = LiquidityPlanningTransactionType.IncomingBalance,
                        TransactionTypeName = balanceTerm,
                        Specification = "",
                        Total = incomingBalance,
                        ValueIn = 0,
                        ValueOut = 0,
                    };
                    dtos.Add(balanceRow);

                    decimal balanceIn = 0;
                    decimal balanceOut = 0;
                    foreach (var invoice in invoicesByDate)
                    {
                        var row = new LiquidityPlanningDTO()
                        {
                            InvoiceId = invoice.InvoiceId,
                            InvoiceNr = invoice.InvoiceNr,
                            Date = date,
                            OriginType = (SoeOriginType)invoice.OriginType,
                            ValueIn = 0,
                            ValueOut = 0,
                        };

                        if (invoice.OriginType == (int)SoeOriginType.CustomerInvoice)
                        {
                            row.TransactionType = LiquidityPlanningTransactionType.CustomerInvoice;
                            row.TransactionTypeName = customerInvoiceTerm;
                            row.Specification = (invoice.SeqNr.HasValue && invoice.SeqNr.Value > 0 ? invoice.SeqNr.Value.ToString() : preliminary) + " - " + invoice.Customer;

                            if (invoice.BillingType == (int)TermGroup_BillingType.Debit)
                                row.ValueIn = invoice.TotalAmountCurrency - invoice.PaidAmount;
                            else
                                row.ValueOut = Decimal.Negate(invoice.TotalAmountCurrency - invoice.PaidAmount);

                            row.Total = row.ValueIn - row.ValueOut;
                        }
                        else
                        {
                            row.TransactionType = LiquidityPlanningTransactionType.SupplierInvoice;
                            row.TransactionTypeName = supplierInvoiceTerm;
                            row.Specification = (invoice.SeqNr.HasValue && invoice.SeqNr.Value > 0 ? invoice.SeqNr.Value.ToString() : preliminary) + " - " + invoice.Supplier;

                            if (invoice.BillingType == (int)TermGroup_BillingType.Debit)
                                row.ValueOut = invoice.TotalAmountCurrency - invoice.PaidAmount;
                            else
                                row.ValueIn = Decimal.Negate(invoice.TotalAmountCurrency - invoice.PaidAmount);

                            row.Total = row.ValueIn - row.ValueOut;
                        }
                        dtos.Add(row);

                        incomingBalance += row.Total;
                        balanceIn += row.ValueIn;
                        balanceOut += row.ValueOut;
                    }

                    foreach (var transaction in transactionsByDate)
                    {
                        var row = new LiquidityPlanningDTO()
                        {
                            LiquidityPlanningTransactionId = transaction.LiquidityPlanningTransactionId,
                            InvoiceId = 0,
                            InvoiceNr = "",
                            Date = transaction.DueDate.Value,
                            OriginType = SoeOriginType.None,
                            TransactionType = (LiquidityPlanningTransactionType)transaction.TransactionType,
                            TransactionTypeName = manualTerm,
                            Specification = transaction.Description,
                            Created = transaction.Created,
                            CreatedBy = transaction.CreatedBy,
                            Modified = transaction.Modified,
                            ModifiedBy = transaction.ModifiedBy,
                        };

                        if (transaction.TotalAmountCurrency > 0)
                        {
                            row.ValueIn = transaction.TotalAmountCurrency;
                            row.Total = transaction.TotalAmountCurrency;
                        }
                        else
                        {
                            row.ValueOut = Decimal.Negate(transaction.TotalAmountCurrency);
                            row.Total = transaction.TotalAmountCurrency;
                        }
                        dtos.Add(row);

                        incomingBalance += row.Total;
                        balanceIn += row.ValueIn;
                        balanceOut += row.ValueOut;
                    }
                }
            }

            return dtos.OrderBy(d => d.Date).ToList();
        }


        public LiquidityPlanningTransaction GetLiquidityPlanningTransaction(int liquidityPlanningTransactionId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.LiquidityPlanningTransaction.NoTracking();
            return GetLiquidityPlanningTransaction(entities, liquidityPlanningTransactionId);
        }

        public LiquidityPlanningTransaction GetLiquidityPlanningTransaction(CompEntities entities, int liquidityPlanningTransactionId)
        {
            return (from lpt in entities.LiquidityPlanningTransaction
                    where lpt.LiquidityPlanningTransactionId == liquidityPlanningTransactionId
                    select lpt).FirstOrDefault();
        }

        public ActionResult SaveLiquidityPlanningTransaction(LiquidityPlanningDTO dto)
        {
            ActionResult result = new ActionResult();

            try
            {
                using (CompEntities entities = new CompEntities())
                {
                    if (entities.Connection.State != ConnectionState.Open)
                        entities.Connection.Open();

                    if (dto.LiquidityPlanningTransactionId.HasValue && dto.LiquidityPlanningTransactionId.Value > 0)
                    {
                        var existing = this.GetLiquidityPlanningTransaction(entities, dto.LiquidityPlanningTransactionId.Value);
                        if (existing == null)
                            return new ActionResult((int)ActionResultDelete.EntityNotFound, "LiquidityPlanningTransaction");

                        existing.TransactionDate = dto.Date;
                        existing.Amount = dto.ValueIn > 0 ? dto.ValueIn : dto.ValueOut;
                        existing.Description = dto.Specification;

                        SetModifiedProperties(existing);
                    }
                    else
                    {
                        var transaction = new LiquidityPlanningTransaction()
                        {
                            ActorCompanyId = base.ActorCompanyId,
                            TransactionDate = dto.Date,
                            TransactionType = (int)LiquidityPlanningTransactionType.Manual,
                            Amount = dto.ValueIn > 0 ? dto.ValueIn : dto.ValueOut,
                            Description = dto.Specification,
                        };

                        SetCreatedProperties(transaction);
                        entities.LiquidityPlanningTransaction.AddObject(transaction);
                    }

                    result = SaveChanges(entities);
                }
            }
            catch (Exception ex)
            {
                result.Exception = ex;
                base.LogError(ex, this.log);
            }
            finally
            {
                if (!result.Success)
                    base.LogTransactionFailed(this.ToString(), this.log);
            }

            return result;
        }

        public ActionResult DeleteLiquidityPlanningTransaction(int liquidityPlanningTransactionId)
        {
            ActionResult result = new ActionResult();

            try
            {
                using (CompEntities entities = new CompEntities())
                {
                    if (entities.Connection.State != ConnectionState.Open)
                        entities.Connection.Open();

                    #region Prereq

                    LiquidityPlanningTransaction existing = this.GetLiquidityPlanningTransaction(entities, liquidityPlanningTransactionId);

                    #endregion

                    #region Perform

                    if (existing == null)
                        return new ActionResult((int)ActionResultDelete.EntityNotFound, "LiquidityPlanningTransaction");

                    //Update
                    ChangeEntityState(existing, SoeEntityState.Deleted);

                    result = SaveChanges(entities);

                    #endregion
                }
            }
            catch (Exception ex)
            {
                result.Exception = ex;
                base.LogError(ex, this.log);
            }
            finally
            {
                if (!result.Success)
                    base.LogTransactionFailed(this.ToString(), this.log);
            }

            return result;
        }
    }
}
