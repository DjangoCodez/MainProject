using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Data
{
    public partial class PayrollImportHead : ICreatedModified, IState
    {
        public string TypeName { get; set; }
        public string FileTypeName { get; set; }
        public TermGroup_PayrollImportHeadStatus Status { get; set; }
        public string StatusName { get; set; }
    }

    public partial class PayrollImportEmployee : IState
    {
        public string EmployeeInfo { get; set; }
        public TermGroup_PayrollImportEmployeeStatus Status { get; set; }
        public string StatusName { get; set; }
    }

    public partial class PayrollImportEmployeeSchedule : IState
{
        public string StatusName { get; set; }
    }

    public partial class PayrollImportEmployeeTransaction : IState
{
        public string TypeName { get; set; }
        public string StatusName { get; set; }
    }

    public static partial class EntityExtensions
    {
        #region PayrollImportHead

        public static PayrollImportHeadDTO ToDTO(this PayrollImportHead e, bool includeFile, bool includeEmployees, string paymentDateLabel)
        {
            if (e == null)
                return null;

            PayrollImportHeadDTO dto = new PayrollImportHeadDTO()
            {
                PayrollImportHeadId = e.PayrollImportHeadId,
                ActorCompanyId = e.ActorCompanyId,
                DateFrom = e.DateFrom,
                DateTo = e.DateTo,
                PaymentDate = e.PaymentDate,
                Type = (TermGroup_PayrollImportHeadType)e.Type,
                TypeName = e.TypeName,
                FileType = (TermGroup_PayrollImportHeadFileType)e.FileType,
                FileTypeName = e.FileTypeName,
                File = includeFile ? e.File : null,
                Comment = e.Comment,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State,
                Checksum = e.Checksum,
                Status = e.Status,
                StatusName = e.StatusName,
            };

            dto.Name = $"{dto.DateFrom.ToShortDateString()} - {dto.DateTo.ToShortDateString()}, {paymentDateLabel}: {(dto.PaymentDate?.ToShortDateString() ?? string.Empty)}";
            if (includeEmployees)
            {
                dto.Employees = e.PayrollImportEmployee?.Where(i => i.State == (int)SoeEntityState.Active).ToDTOs(includeFile, includeEmployees).ToList();
                dto.NrOfEmployees = dto.Employees.Count;
            }

            return dto;
        }

        public static IEnumerable<PayrollImportHeadDTO> ToDTOs(this IEnumerable<PayrollImportHead> l, bool includeFile, bool includeEmployees, string paymentDateLabel)
        {
            var dtos = new List<PayrollImportHeadDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO(includeFile, includeEmployees, paymentDateLabel));
                }
            }
            return dtos;
        }

        public static DateTime? GetDateTo(this PayrollImportEmployeeDTO e, PayrollImportHeadDTO importHead)
        {
            return e?.LastDateSchedule() ?? e?.LastDateTransactions() ?? importHead?.DateTo;
        }

        #endregion

        #region PayrollImportEmployee

        public static PayrollImportEmployeeDTO ToDTO(this PayrollImportEmployee e, bool includeSchedule, bool includeTransactions, List<Account> accounts = null, List<PayrollImportEmployeeSchedule> importEmployeeSchedules = null, List<PayrollImportEmployeeTransaction> importEmployeeTransactions = null)
        {
            if (e == null)
                return null;

            List<PayrollImportEmployeeSchedule> schedule = (importEmployeeSchedules ?? e.PayrollImportEmployeeSchedule?.ToList())?
                .Where(i => i.State == (int)SoeEntityState.Active).ToList() ?? new List<PayrollImportEmployeeSchedule>();
            List<PayrollImportEmployeeTransaction> transactions = (importEmployeeTransactions ?? e.PayrollImportEmployeeTransaction?.ToList())?
                .Where(i => i.State == (int)SoeEntityState.Active).ToList() ?? new List<PayrollImportEmployeeTransaction>();

            PayrollImportEmployeeDTO dto = new PayrollImportEmployeeDTO()
            {
                PayrollImportEmployeeId = e.PayrollImportEmployeeId,
                PayrollImportHeadId = e.PayrollImportHeadId,
                EmployeeId = e.EmployeeId,
                EmployeeInfo = e.EmployeeInfo,
                ScheduleRowCount = schedule.Count,
                ScheduleQuantity = CalendarUtility.MinutesToTimeSpan(Convert.ToInt32(schedule.Sum(i => i.Quantity))),
                ScheduleBreakQuantity = CalendarUtility.MinutesToTimeSpan(Convert.ToInt32(schedule.Where(i => i.IsBreak).Sum(i => i.Quantity))),
                TransactionRowCount = transactions.Count,
                TransactionQuantity = transactions.Sum(i => i.Quantity),
                TransactionAmount = transactions.Sum(i => i.Amount),
                Status = e.Status,
                StatusName = e.StatusName,
                State = (SoeEntityState)e.State,
            };

            if (includeSchedule)
                dto.Schedule = schedule.OrderBy(s => s.Date).ThenBy(s => s.StartTime).ToDTOs().ToList();
            if (includeTransactions)
                dto.Transactions = transactions.OrderBy(t => t.Date).ThenBy(t => t.StartTime).ToDTOs(accounts: accounts).ToList();

            return dto;
        }

        public static void SetStatuses(this PayrollImportEmployeeDTO e, List<GenericType> employeeStatus, ref bool hasUnprocessed, ref bool hasError, ref bool hasProcessed, ref bool hasPartlyProcessed)
        {
            if (e == null)
                return;

            var scheduleStatuses = e.Schedule?.Where(i => i.State == SoeEntityState.Active).Select(t => t.Status).Distinct().ToList() ?? new List<TermGroup_PayrollImportEmployeeScheduleStatus>();
            var transStatuses = e.Transactions?.Where(i => i.State == SoeEntityState.Active).Select(t => t.Status).Distinct().ToList() ?? new List<TermGroup_PayrollImportEmployeeTransactionStatus>();

            bool scheduleHasUnprocessed = scheduleStatuses.Any(t => t == TermGroup_PayrollImportEmployeeScheduleStatus.Unprocessed);
            bool scheduleHasError = scheduleStatuses.Any(t => t == TermGroup_PayrollImportEmployeeScheduleStatus.Error);
            bool scheduleHasProcessed = scheduleStatuses.Any(t => t == TermGroup_PayrollImportEmployeeScheduleStatus.Processed);
            bool transHasUnprocessed = transStatuses.Any(t => t == TermGroup_PayrollImportEmployeeTransactionStatus.Unprocessed);
            bool transHasError = transStatuses.Any(t => t == TermGroup_PayrollImportEmployeeTransactionStatus.Error);
            bool transHasProcessed = transStatuses.Any(t => t == TermGroup_PayrollImportEmployeeTransactionStatus.Processed);

            if (!transStatuses.Any() && !scheduleStatuses.Any())
            {
                // No transactions or schedules
                e.Status = TermGroup_PayrollImportEmployeeStatus.Unprocessed;
                hasUnprocessed = true;
            }
            else
            {
                if (!scheduleStatuses.Any())
                {
                    scheduleHasUnprocessed = transHasUnprocessed;
                    scheduleHasError = transHasError;
                    scheduleHasProcessed = transHasProcessed;
                }

                if (!transStatuses.Any())
                {
                    transHasUnprocessed = scheduleHasUnprocessed;
                    transHasError = scheduleHasError;
                    transHasProcessed = scheduleHasProcessed;
                }

                if (transHasError || scheduleHasError)
                {
                    // At least one error
                    e.Status = TermGroup_PayrollImportEmployeeStatus.Error;
                    hasError = true;
                }
                else if (transHasProcessed && !transHasUnprocessed && scheduleHasProcessed && !scheduleHasUnprocessed)
                {
                    // All transactions and schedules processed
                    e.Status = TermGroup_PayrollImportEmployeeStatus.Processed;
                    hasProcessed = true;
                }
                else if (transHasUnprocessed && !transHasProcessed && scheduleHasUnprocessed && !scheduleHasProcessed)
                {
                    // All transactions and schedules unprocessed
                    e.Status = TermGroup_PayrollImportEmployeeStatus.Unprocessed;
                    hasUnprocessed = true;
                }
                else
                {
                    // Mix of processed and unprocessed
                    e.Status = TermGroup_PayrollImportEmployeeStatus.PartlyProcessed;
                    hasPartlyProcessed = true;
                }
            }

            e.StatusName = employeeStatus?.FirstOrDefault(t => t.Id == (int)e.Status)?.Name;
        }

        public static IEnumerable<PayrollImportEmployeeDTO> ToDTOs(this IEnumerable<PayrollImportEmployee> l, bool includeSchedule, bool includeTransactions)
        {
            var dtos = new List<PayrollImportEmployeeDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO(includeSchedule, includeTransactions));
                }
            }
            return dtos;
        }

        public static List<PayrollImportEmployeeScheduleDTO> GetScheduleToProcess(this PayrollImportEmployeeDTO e)
        {
            return e?.Schedule?.Where(i => i.State == SoeEntityState.Active && i.Status == TermGroup_PayrollImportEmployeeScheduleStatus.Unprocessed).OrderBy(i => i.Date).ToList() ?? new List<PayrollImportEmployeeScheduleDTO>();
        }

        public static List<PayrollImportEmployeeTransactionDTO> GetTransactionsToProcess(this PayrollImportEmployeeDTO e)
        {
            return e?.Transactions?.Where(i => i.State == SoeEntityState.Active && i.Status == TermGroup_PayrollImportEmployeeTransactionStatus.Unprocessed).OrderBy(i => i.Date).ToList() ?? new List<PayrollImportEmployeeTransactionDTO>();
        }

        public static DateTime? GetDateFrom(this PayrollImportEmployeeDTO e, PayrollImportHeadDTO importHead)
        {
            return e?.FirstDateSchedule() ?? e?.FirstDateTransactions() ?? importHead?.DateFrom;
        }

        #endregion

        #region PayrollImportEmployeeSchedule

        public static PayrollImportEmployeeScheduleDTO ToDTO(this PayrollImportEmployeeSchedule e)
        {
            if (e == null)
                return null;

            return new PayrollImportEmployeeScheduleDTO()
            {
                PayrollImportEmployeeScheduleId = e.PayrollImportEmployeeScheduleId,
                PayrollImportEmployeeId = e.PayrollImportEmployeeId,
                Date = e.Date,
                StartTime = e.StartTime,
                StopTime = e.StopTime,
                IsBreak = e.IsBreak,
                Quantity = e.Quantity,
                ErrorMessage = e.ErrorMessage,
                StatusName = e.StatusName,
                Status = (TermGroup_PayrollImportEmployeeScheduleStatus)e.Status,
                State = (SoeEntityState)e.State,
            };
        }

        public static IEnumerable<PayrollImportEmployeeScheduleDTO> ToDTOs(this IEnumerable<PayrollImportEmployeeSchedule> l)
        {
            var dtos = new List<PayrollImportEmployeeScheduleDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        #endregion

        #region PayrollImportEmployeeTransaction

        public static PayrollImportEmployeeTransactionDTO ToDTO(this PayrollImportEmployeeTransaction e, List<PayrollProductDTO> payrollProducts = null, List<TimeDeviationCauseDTO> timeDeviationCauses = null, List<Account> accounts = null)
        {
            if (e == null)
                return null;

            string name = string.Empty;

            if (e.PayrollProductId.HasValue && !payrollProducts.IsNullOrEmpty())
                name = payrollProducts.FirstOrDefault(f => f.ProductId == e.PayrollProductId)?.NumberName;
            else if (e.TimeDeviationCauseId.HasValue && !timeDeviationCauses.IsNullOrEmpty())
                name = timeDeviationCauses.FirstOrDefault(f => f.TimeDeviationCauseId == e.TimeDeviationCauseId)?.Name;

            PayrollImportEmployeeTransactionDTO dto = new PayrollImportEmployeeTransactionDTO()
            {
                PayrollImportEmployeeTransactionId = e.PayrollImportEmployeeTransactionId,
                PayrollImportEmployeeId = e.PayrollImportEmployeeId,
                PayrollProductId = e.PayrollProductId,
                TimeDeviationCauseId = e.TimeDeviationCauseId,
                TimeCodeAdditionDeductionId = e.TimeCodeAdditionDeductionId,
                AccountStdId = e.AccountStdId,
                AccountStdNr = e.AccountStd?.Account?.AccountNr,
                AccountStdName = e.AccountStd?.Account?.Name,
                AccountStdDimNr = e.AccountStd?.Account?.AccountDim?.AccountDimNr,
                Date = e.Date,
                StartTime = e.StartTime,
                StopTime = e.StopTime,
                Quantity = e.Quantity,
                Amount = e.Amount,
                Code = e.Code,
                AccountCode = e.AccountCode,
                Note = e.Note,
                ErrorMessage = e.ErrorMessage,
                TypeName = e.TypeName,
                StatusName = e.StatusName,
                Type = (TermGroup_PayrollImportEmployeeTransactionType)e.Type,
                Status = (TermGroup_PayrollImportEmployeeTransactionStatus)e.Status,
                State = (SoeEntityState)e.State,
                Name = name,
            };

            dto.AccountInternals = e.PayrollImportEmployeeTransactionAccountInternal?.ToDTOs(accounts);
            dto.PayrollImportEmployeeTransactionLinks = e.PayrollImportEmployeeTransactionLink?.ToDTOs();

            return dto;
        }

        public static IEnumerable<PayrollImportEmployeeTransactionDTO> ToDTOs(this IEnumerable<PayrollImportEmployeeTransaction> l, List<PayrollProductDTO> payrollProducts = null, List<TimeDeviationCauseDTO> timeDeviationCauses = null, List<Account> accounts = null)
        {
            var dtos = new List<PayrollImportEmployeeTransactionDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO(payrollProducts, timeDeviationCauses, accounts));
                }
            }
            return dtos;
        }

        public static PayrollImportEmployeeTransactionAccountInternalDTO ToDTO(this PayrollImportEmployeeTransactionAccountInternal e, List<Account> accounts = null)
        {
            if (e == null)
                return null;

            Account accountInternal = null;
            if (e.AccountInternal != null)
                accountInternal = e.AccountInternal?.Account ?? accounts?.FirstOrDefault(a => a.AccountId == e.AccountInternal.AccountId);

            return new PayrollImportEmployeeTransactionAccountInternalDTO()
            {
                AccountSIEDimNr = e.AccountSIEDimNr,
                AccountCode = e.AccountCode,
                AccountId = e.AccountId,
                AccountNr = accountInternal?.AccountNr,
                AccountName = accountInternal?.Name,
                AccountDimNr = accountInternal?.AccountDim.AccountDimNr,
            };
        }

        public static List<PayrollImportEmployeeTransactionAccountInternalDTO> ToDTOs(this ICollection<PayrollImportEmployeeTransactionAccountInternal> l, List<Account> accounts = null)
        {
            var dtos = new List<PayrollImportEmployeeTransactionAccountInternalDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO(accounts));
                }
            }
            return dtos;
        }

        public static List<PayrollImportEmployeeTransactionDTO> GetPresence(this List<PayrollImportEmployeeTransactionDTO> l)
        {
            return l?.Where(w => !w.PayrollProductId.IsNullOrEmpty() && w.Amount == 0).ToList() ?? new List<PayrollImportEmployeeTransactionDTO>();
        }
        public static List<PayrollImportEmployeeTransactionDTO> FixedAmount(this List<PayrollImportEmployeeTransactionDTO> l)
        {
            return l?.Where(w => !w.PayrollProductId.IsNullOrEmpty() && w.Amount > 0).ToList() ?? new List<PayrollImportEmployeeTransactionDTO>();
        }

        public static List<PayrollImportEmployeeTransactionDTO> GetAbsence(this List<PayrollImportEmployeeTransactionDTO> l)
        {
            return l?.Where(w => !w.TimeDeviationCauseId.IsNullOrEmpty()).ToList() ?? new List<PayrollImportEmployeeTransactionDTO>();
        }

        #endregion

        #region PayrollImportEmployeeTransactionLinkDTO

        public static PayrollImportEmployeeTransactionLinkDTO ToDTO(this PayrollImportEmployeeTransactionLink e)
        {
            if (e == null)
                return null;

            return new PayrollImportEmployeeTransactionLinkDTO()
            {
                PayrollImportEmployeeTransactionLinkId = e.PayrollImportEmployeeTransactionLinkId,
                PayrollImportEmployeeTransactionId = e.PayrollImportEmployeeTransactionId,
                TimeBlockId = e.TimeBlockId,
                TimePayrollTransactionId = e.PayrollImportEmployeeTransactionId,
                EmployeeId = e.EmployeeId,
                Date = e.Date,
                State = (SoeEntityState)e.State
            };
        }

        public static List<PayrollImportEmployeeTransactionLinkDTO> ToDTOs(this ICollection<PayrollImportEmployeeTransactionLink> l)
        {
            var dtos = new List<PayrollImportEmployeeTransactionLinkDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        #endregion
    }
}
