using Azure.Storage.Blobs.Models;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Shared.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftOne.Soe.Business.Core.Bridge.Visma
{
    public static class VismaHrPlusAbsence
    {
        public static ActionResult ExportAbsence(
                        List<ScheduledJobSetting> scheduledJobSettings,
                        CompEntities entities,
                        BridgeManager bridgeManager,
                        ImportExportManager importExportManager,
                        ActorManager actorManager,
                        AccountManager accountManager,
                        TimeSalaryManager timeSalaryManager,
                        int actorCompanyId,
                        int scheduledJobHeadId,
                        int scheduledJobRowId,
                        int batchNr,
                        Action<CompEntities, int?, int?, int, string> infoLog,
                        Action<int?, int?, int, string> errorLog)
        {
            infoLog(entities, scheduledJobHeadId, scheduledJobRowId, batchNr, "SendVismaHrplusAbsence started");

            bool isInitialTransfer = scheduledJobSettings.FirstOrDefault(a => a.Type == (int)TermGroup_ScheduledJobSettingType.ExportIsPreliminary && a.DataType == (int)SettingDataType.Boolean && a.BoolData.HasValue && a.State == (int)SoeEntityState.Active)?.BoolData ?? false;

            DateTime dateFrom;
            if (isInitialTransfer)
            {
                dateFrom = new DateTime(2023, 1, 1);
            }
            else
            {
                dateFrom = timeSalaryManager.GetTimeSalaryExportDTOs(actorCompanyId, true)
                    .Where(exp => exp.StopInterval <= DateTime.Today.AddDays(-7))
                    .OrderBy(o => o.StopInterval)
                    .LastOrDefault()?.StopInterval.AddDays(1) ?? DateTime.Today.AddDays(-46);

                if (dateFrom < DateTime.Today.AddDays(-46) || dateFrom > DateTime.Today)
                    dateFrom = DateTime.Today.AddDays(-46);
            }

            var externalCodes = actorManager.GetCompanyExternalCodes(entities, TermGroup_CompanyExternalCodeEntity.AccountHierachyPayrollExport, actorCompanyId);
            var externalCodeUnits = actorManager.GetCompanyExternalCodes(entities, TermGroup_CompanyExternalCodeEntity.AccountHierachyPayrollExportUnit, actorCompanyId);

            var dimsWithAccount = accountManager.GetAccountDimsByCompany(entities, actorCompanyId, onlyInternal: true, loadAccounts: true, loadInternalAccounts: true);

            foreach (var externalCode in externalCodes)
            {
                foreach (var dim in dimsWithAccount)
                {
                    foreach (var acc in dim.Account.Where(w => w.AccountId == externalCode.RecordId))
                    {
                        acc.AccountHierachyPayrollExportExternalCode = externalCode.ExternalCode;
                    }
                }
            }

            foreach (var externalCode in externalCodeUnits)
            {
                foreach (var dim in dimsWithAccount)
                {
                    foreach (var acc in dim.Account.Where(w => w.AccountId == externalCode.RecordId))
                    {
                        acc.AccountHierachyPayrollExportUnitExternalCode = externalCode.ExternalCode;
                    }
                }
            }


            var longTermAbsenceOutput = importExportManager.GetLongTermAbsence(new LongTermAbsenceInput
            {
                CalculateRatio = false,
                DateFrom = dateFrom,
                DateTo = DateTime.Today.AddDays(-1),
                PayrollProductInputs = new List<LongTermAbsencePayrollProductInput>
                {
                    new LongTermAbsencePayrollProductInput
                    {
                        SysPayrollTypeLevel1 = TermGroup_SysPayrollType.SE_GrossSalary,
                        SysPayrollTypeLevel2 = TermGroup_SysPayrollType.SE_GrossSalary_Absence,
                        SysPayrollTypeLevel3 = TermGroup_SysPayrollType.SE_GrossSalary_Absence_Sick,
                    }
                }
            }, actorCompanyId);

            // Set external Code and unit external code
            if (longTermAbsenceOutput?.Rows != null)
            {
                DateTime minEndDate = new DateTime(2025, 1, 1); // the minimum end date agreed with Coop & Visma in a meeting on 2025-06-25
                longTermAbsenceOutput.Rows = longTermAbsenceOutput.Rows
                    .Where(r => r.StopDate >= minEndDate)
                    .ToList();

                DateTime maxDate = DateTime.Today.AddDays(-1);
                longTermAbsenceOutput.Rows = longTermAbsenceOutput.Rows
                    .Where(r => r.StartDate <= maxDate)
                    .ToList();

                foreach (var absenceRow in longTermAbsenceOutput.Rows)
                {
                    if (!string.IsNullOrEmpty(absenceRow.SocialSec))
                        absenceRow.SocialSec = StringUtility.SocialSecYYMMDDXXXX(absenceRow.SocialSec);

                    if (absenceRow.StopDate > maxDate)
                        absenceRow.StopDate = maxDate;

                    foreach (var account in absenceRow.EmployeeAccounts)
                    {
                        foreach (var dim in dimsWithAccount.Where(w => w.AccountDimNr == account.AccountDimNr || account.AccountDimNr == 0))
                        {
                            var acc = dim.Account.FirstOrDefault(w => w.AccountNr == account.AccountNr);
                            if (acc == null)
                                continue;

                            if (!string.IsNullOrEmpty(acc.AccountHierachyPayrollExportExternalCode))
                                account.ExportExternalCode = acc.AccountHierachyPayrollExportExternalCode;
                            if (!string.IsNullOrEmpty(acc.AccountHierachyPayrollExportUnitExternalCode))
                                account.UnitExternalCode = acc.AccountHierachyPayrollExportUnitExternalCode;
                        }
                    }
                }
            }

            if (longTermAbsenceOutput == null)
            {
                infoLog(entities, scheduledJobHeadId, scheduledJobRowId, batchNr, "SendVismaHrplusAbsence no data");
                return new ActionResult(false);
            }
            else if (!string.IsNullOrEmpty(longTermAbsenceOutput.ErrorMessage))
            {
                errorLog(scheduledJobHeadId, scheduledJobRowId, batchNr, "SendVismaHrplusAbsence failed: " + longTermAbsenceOutput.ErrorMessage);
                return new ActionResult(false);
            }
            else
            {
                infoLog(entities, scheduledJobHeadId, scheduledJobRowId, batchNr, $"SendVismaHrplusAbsence fetched information. Rows {longTermAbsenceOutput.Rows?.Count ?? 0}");
            }

            var result = bridgeManager.SendVismaHrplusAbsence(scheduledJobSettings, longTermAbsenceOutput, actorCompanyId);
            if (result.Success)
            {
                infoLog(entities, scheduledJobHeadId, scheduledJobRowId, batchNr, $"SendVismaHrplusAbsence sent. {result.InfoMessage}");
            }
            else
            {
                errorLog(scheduledJobHeadId, scheduledJobRowId, batchNr, $"SendVismaHrplusAbsence failed: {result.ErrorMessage}");
            }
            infoLog(entities, scheduledJobHeadId, scheduledJobRowId, batchNr, "SendVismaHrplusAbsence finished");

            return result;
        }
    }
}
