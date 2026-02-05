using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Interfaces;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.ScheduledJobs
{
    public class GridStateJob : ScheduledJobBase, IScheduledJob
    {
        public void Execute(SysScheduledJobDTO scheduledJob, int batchNr)
        {
            #region Init

            base.Init(scheduledJob, batchNr);

            CompanyManager cm = new CompanyManager(parameterObject);
            SettingManager sm = new SettingManager(parameterObject);

            #endregion

            int? paramCompanyId = scheduledJob.SysJobSettings.FirstOrDefault(s => s.Name.ToLower() == "actorcompanyid")?.IntData;
            int? paramGridType = scheduledJob.SysJobSettings.FirstOrDefault(s => s.Name.ToLower() == "type")?.IntData;
            bool? paramSys = scheduledJob.SysJobSettings.FirstOrDefault(s => s.Name.ToLower() == "sys")?.BoolData;

            // Check out scheduled job
            ActionResult result = CheckOutScheduledJob();
            if (result.Success)
            {
                try
                {
                    if (paramGridType.HasValue)
                    {
                        var (gridType, gridName) = sm.GetAgGridTypeName(paramGridType.Value);
                        if (gridType != AgGridType.Unknown)
                        {
                            string gridInfo = $"{(int)gridType}.{gridName}";

                            if (paramSys == true)
                            {
                                CreateLogEntry(ScheduledJobLogLevel.Information, $"Startar SysGridState {gridInfo}");
                                result = sm.FormatSysGridState(gridType, gridName, out bool formattedSysGridState);
                                if (formattedSysGridState)
                                    CreateLogEntry(ScheduledJobLogLevel.Information, $"Klar med SysGridState {gridInfo}");
                                else
                                    CreateLogEntry(ScheduledJobLogLevel.Information, $"Klar med SysGridState {gridInfo}. Behövdes ej formatering");
                            }
                            else
                            {
                                CreateLogEntry(ScheduledJobLogLevel.Information, $"Hoppar över SysGridState {gridInfo}. Använd parameter sys=1 för att formatera SysGridState");
                            }

                            if (result.Success)
                            {
                                List<int> companyIds = paramCompanyId.HasValue ? paramCompanyId.Value.ObjToList() : sm.GetCompaniesWithUserStates(gridName);
                                List<Company> companies = cm.GetCompanies(companyIds);
                                CreateLogEntry(ScheduledJobLogLevel.Information, $"Startar formatering av UserGridState för {companies.Count} företag");

                                foreach (Company company in companies)
                                {
                                    CreateLogEntry(ScheduledJobLogLevel.Information, $"Startar UserGridState för företag {company.Name}");
                                    result = sm.FormatUserGridState(gridType, gridName, company.ActorCompanyId, out int total, out int formatted);
                                    if (result.Success)
                                        CreateLogEntry(ScheduledJobLogLevel.Information, $"Klar med UserGridState för företag {company.Name}. Formaterade för {formatted} av {total} användare");
                                    else
                                        CreateLogEntry(ScheduledJobLogLevel.Error, $"Fel vid exekvering av jobb: '{result.ErrorMessage}'");
                                }
                            }
                            else
                            {
                                CreateLogEntry(ScheduledJobLogLevel.Error, $"Fel vid exekvering av jobb: '{result.ErrorMessage}'");
                            }
                        }
                        else
                        {
                            CreateLogEntry(ScheduledJobLogLevel.Error, $"Jobb kunde inte köras. Felaktig parameter type {paramGridType}");
                        }
                    }
                    else
                    {
                        CreateLogEntry(ScheduledJobLogLevel.Error, $"Jobb kunde inte köras. Parameter type saknas");
                    }
                }
                catch (Exception ex)
                {
                    result = new ActionResult(ex);
                    CreateLogEntry(ScheduledJobLogLevel.Error, $"Fel vid exekvering av jobb: '{result.ErrorMessage}'");
                    base.LogError(ex);
                }

                // Check in scheduled job
                CheckInScheduledJob(result.Success);
            }
        }
    }
}
