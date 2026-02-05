using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Data
{
    public partial class RetroactivePayroll : ICreatedModified, IState
    {
        public string StatusName { get; set; }
        public int NrOfEmployees { get; set; }
    }

    public partial class RetroactivePayrollEmployee : ICreatedModified, IState
    {

    }

    public partial class RetroactivePayrollOutcome : ICreatedModified, IState
    {
        public bool CalculatedHasTransactions { get; set; }
    }

    public static partial class EntityExtensions
    {
        #region RetroactivePayroll

        public static RetroactivePayrollDTO ToDTO(this RetroactivePayroll e)
        {
            if (e == null)
                return null;

            RetroactivePayrollDTO dto = new RetroactivePayrollDTO()
            {
                RetroactivePayrollId = e.RetroactivePayrollId,
                ActorCompanyId = e.ActorCompanyId,
                TimePeriodId = e.TimePeriodId,
                DateFrom = e.DateFrom,
                DateTo = e.DateTo,
                Name = e.Name,
                Status = (TermGroup_SoeRetroactivePayrollStatus)e.Status,
                Note = e.Note,
                StatusName = e.StatusName,
                NrOfEmployees = e.NrOfEmployees,
                TimePeriodName = e.TimePeriod?.Name ?? string.Empty,
                TimePeriodPaymentDate = e.TimePeriod?.PaymentDate,
                TimePeriodHeadId = e.TimePeriod?.TimePeriodHead?.TimePeriodHeadId ?? 0,
                TimePeriodHeadName = e.TimePeriod?.TimePeriodHead?.Name ?? string.Empty,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State,
            };

            if (e.RetroactivePayrollEmployee != null)
                dto.RetroactivePayrollEmployees = e.RetroactivePayrollEmployee.ToDTOs().ToList();
            if (e.RetroactivePayrollAccount != null)
                dto.RetroactivePayrollAccounts = e.RetroactivePayrollAccount.ToDTOs().ToList();

            return dto;
        }

        public static IEnumerable<RetroactivePayrollDTO> ToDTOs(this IEnumerable<RetroactivePayroll> l)
        {
            var dtos = new List<RetroactivePayrollDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static TermGroup_SoeRetroactivePayrollStatus GetRetroactivePayrollStatus(this RetroactivePayroll e)
        {
            return e.RetroactivePayrollEmployee.ToList().GetRetroactivePayrollStatus();
        }

        #endregion

        #region RetroactivePayrollAccount

        public static RetroactivePayrollAccountDTO ToDTO(this RetroactivePayrollAccount e)
        {
            if (e == null)
                return null;

            #region Try load

            try
            {
                if (!e.IsAdded() && !e.AccountDimReference.IsLoaded)
                {
                    e.AccountDimReference.Load();
                    DataProjectLogCollector.LogLoadedEntityInExtension("RetroactivePayroll.cs e.AccountDimReference");
                }
            }
            catch (InvalidOperationException ex) { ex.ToString(); }

            #endregion        

            RetroactivePayrollAccountDTO dto = new RetroactivePayrollAccountDTO()
            {
                RetroactivePayrollAccountId = e.RetroactivePayrollAccountId,
                RetroactivePayrollId = e.RetroactivePayrollId,
                AccountDimId = e.AccountDimId,
                AccountId = null,
                Type = (TermGroup_RetroactivePayrollAccountType)e.Type,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State,
            };

            if (e.AccountDim != null)
                dto.AccountId = e.AccountDim.IsStandard ? e.AccountStdId : e.AccountInternalId;

            return dto;
        }

        public static IEnumerable<RetroactivePayrollAccountDTO> ToDTOs(this IEnumerable<RetroactivePayrollAccount> l)
        {
            var dtos = new List<RetroactivePayrollAccountDTO>();
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

        #region RetroactivePayrollEmployee

        public static RetroactivePayrollEmployeeDTO ToDTO(this RetroactivePayrollEmployee e)
        {
            if (e == null)
                return null;

            RetroactivePayrollEmployeeDTO dto = new RetroactivePayrollEmployeeDTO()
            {
                RetroactivePayrollEmployeeId = e.RetroactivePayrollEmployeeId,
                RetroactivePayrollId = e.RetroactivePayrollId,
                ActorCompanyId = e.ActorCompanyId,
                EmployeeId = e.EmployeeId,
                EmployeeNr = e.Employee?.EmployeeNr ?? string.Empty,
                EmployeeName = e.Employee?.ContactPerson?.Name ?? string.Empty,
                Note = e.Note,
                Status = (TermGroup_SoeRetroactivePayrollEmployeeStatus)e.Status,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State,
            };

            if (e.RetroactivePayrollOutcome != null)
                dto.RetroactivePayrollOutcomes = e.RetroactivePayrollOutcome.ToDTOs().ToList();

            return dto;
        }

        public static IEnumerable<RetroactivePayrollEmployeeDTO> ToDTOs(this IEnumerable<RetroactivePayrollEmployee> l)
        {
            var dtos = new List<RetroactivePayrollEmployeeDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static TermGroup_SoeRetroactivePayrollStatus GetRetroactivePayrollStatus(this List<RetroactivePayrollEmployee> l)
        {
            TermGroup_SoeRetroactivePayrollStatus retroPayrollStatus = TermGroup_SoeRetroactivePayrollStatus.Saved;

            List<RetroactivePayrollEmployee> retroEmployees = l.Where(i => i.State == (int)SoeEntityState.Active).ToList();
            int total = retroEmployees.Count;
            if (total > 0)
            {
                int nrOfStatusCalculated = retroEmployees.Count(i => i.Status == (int)TermGroup_SoeRetroactivePayrollEmployeeStatus.Calculated);
                int nrOfStatusPayroll = retroEmployees.Count(i => i.Status == (int)TermGroup_SoeRetroactivePayrollEmployeeStatus.Payroll);
                int nrOfStatusLocked = retroEmployees.Count(i => i.Status == (int)TermGroup_SoeRetroactivePayrollEmployeeStatus.Locked);

                bool statusLocked = nrOfStatusLocked > 0 && nrOfStatusLocked == total;
                bool statusPartlyLocked = nrOfStatusLocked > 0 && nrOfStatusLocked < total;
                bool statusPayroll = nrOfStatusPayroll > 0 && nrOfStatusPayroll == total;
                bool statusPartlyPayroll = nrOfStatusPayroll > 0 && nrOfStatusPayroll < total;
                bool statusCalculated = nrOfStatusCalculated > 0 && nrOfStatusCalculated == total;
                bool statusPartlyCalculated = nrOfStatusCalculated > 0 && nrOfStatusCalculated < total;

                if (Extensions.ExceedsThreshold(2, statusLocked, statusPartlyLocked, statusPayroll, statusPartlyPayroll, statusCalculated, statusPartlyCalculated))
                    retroPayrollStatus = TermGroup_SoeRetroactivePayrollStatus.Multiple;
                else if (statusLocked)
                    retroPayrollStatus = TermGroup_SoeRetroactivePayrollStatus.Locked;
                else if (statusPartlyLocked)
                    retroPayrollStatus = TermGroup_SoeRetroactivePayrollStatus.PartlyLocked;
                else if (statusPayroll)
                    retroPayrollStatus = TermGroup_SoeRetroactivePayrollStatus.Payroll;
                else if (statusPartlyPayroll)
                    retroPayrollStatus = TermGroup_SoeRetroactivePayrollStatus.PartyPayroll;
                else if (statusCalculated)
                    retroPayrollStatus = TermGroup_SoeRetroactivePayrollStatus.Calculated;
                else if (statusPartlyCalculated)
                    retroPayrollStatus = TermGroup_SoeRetroactivePayrollStatus.PartlyCalculated;
            }

            return retroPayrollStatus;
        }

        public static bool IsValidToCreateTransactions(this RetroactivePayrollEmployee rpe)
        {
            return (
                rpe.Status == (int)TermGroup_SoeRetroactivePayrollEmployeeStatus.Calculated &&
                rpe.State == (int)SoeEntityState.Active
                );
        }

        public static bool IsValidToDeleteTransactions(this RetroactivePayrollEmployee rpe)
        {
            return (
                rpe.RetroactivePayrollOutcome != null &&
                rpe.Status == (int)TermGroup_SoeRetroactivePayrollEmployeeStatus.Payroll &&
                rpe.State == (int)SoeEntityState.Active
                );
        }

        public static bool IsValidToDeleteOutcomes(this RetroactivePayrollEmployee rpe)
        {
            return (
                rpe.RetroactivePayrollOutcome != null &&
                rpe.State == (int)SoeEntityState.Active
                );
        }

        #endregion

        #region RetroactivePayrollOutcomeDTO

        public static RetroactivePayrollOutcomeDTO ToDTO(this RetroactivePayrollOutcome e)
        {
            if (e == null)
                return null;

            return new RetroactivePayrollOutcomeDTO()
            {
                RetroactivePayrollOutcomeId = e.RetroactivePayrollOutcomeId,
                RetroactivePayrolIEmployeeId = e.RetroactivePayrolIEmployeeId,
                ActorCompanyId = e.ActorCompanyId,
                EmployeeId = e.EmployeeId,
                ProductId = e.PayrollProductId,
                Quantity = e.Quantity,
                TransactionUnitPrice = e.TransactionUnitPrice,
                RetroUnitPrice = e.RetroUnitPrice,
                SpecifiedUnitPrice = e.SpecifiedUnitPrice,
                Amount = e.Amount,
                IsSpecifiedUnitPrice = e.IsSpecifiedUnitPrice,
                IsRetroCalculated = e.IsRetroCalculated,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State,
                ErrorCode = (TermGroup_SoeRetroactivePayrollOutcomeErrorCode)e.ErrorCode,
                ResultType = (TermGroup_PayrollResultType)e.ResultType,
                IsReversed = e.IsReversed,
                PayrollProductNumber = e.PayrollProduct?.Number ?? string.Empty,
                PayrollProductName = e.PayrollProduct?.Name ?? string.Empty,
            };
        }

        public static IEnumerable<RetroactivePayrollOutcomeDTO> ToDTOs(this IEnumerable<RetroactivePayrollOutcome> l)
        {
            var dtos = new List<RetroactivePayrollOutcomeDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static bool IsValidForTransaction(this RetroactivePayrollOutcome e)
        {
            return (
                e.RetroUnitPrice.HasValue &&
                e.IsRetroCalculated &&
                e.State == (int)SoeEntityState.Active &&
                (e.ErrorCode == 0 || e.IsSpecifiedUnitPrice)
                );
        }

        #endregion
    }
}
