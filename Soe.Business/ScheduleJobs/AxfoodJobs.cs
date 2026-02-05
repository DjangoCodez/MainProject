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
    public class AxfoodJobs : ScheduledJobBase, IScheduledJob
    {
        public void Execute(SysScheduledJobDTO scheduledJob, int batchNr)
        {
            #region Init

            base.Init(scheduledJob, batchNr);

            #endregion

            // Check out scheduled job
            ActionResult result = CheckOutScheduledJob();
            if (result.Success)
            {
                try
                {
                    bool? createXml = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "createxml").Select(s => s.BoolData).FirstOrDefault();
                    bool? getedwdata = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "getedwdata").Select(s => s.BoolData).FirstOrDefault();
                    DateTime? fromDate = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "fromdate").Select(s => s.DateData).FirstOrDefault();
                    DateTime? toDate = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "todate").Select(s => s.DateData).FirstOrDefault();
                    bool split800 = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "split800").Select(s => s.BoolData).FirstOrDefault() ?? true;
                    int numberOfDays = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "numberofdays").Select(s => s.IntData).FirstOrDefault() ?? 35;

                    if (getedwdata.HasValue && getedwdata.Value)
                    {
                        List<int> actorCompanyIds = new List<int>();

                        using (CompEntities entities = new CompEntities())
                        {
                            foreach (var license in entities.License.Include("Company").Where(w => w.LicenseId > 8 && w.LicenseNr.StartsWith("8")))
                                license.Company.ToList().ForEach(f => actorCompanyIds.Add(f.ActorCompanyId));
                        }

                        ImportExportManager iem = new ImportExportManager(parameterObject);
                        CreateLogEntry(ScheduledJobLogLevel.Information, String.Format("AxfoodJobs Logg: '{0}'", "TryToSetAccountidOnStaffingneedsFrequency starts"));
                        result = iem.TryToSetAccountidOnStaffingneedsFrequency(actorCompanyIds);
                        CreateLogEntry(ScheduledJobLogLevel.Information, String.Format("TryToSetAccountidOnStaffingneedsFrequency Logg: '{0}'", "ImportStoreDataFromSftp starts " + result.ErrorMessage));
                        result = iem.ImportEDWDataFromSftp();
                        CreateLogEntry(ScheduledJobLogLevel.Information, String.Format("ImportEDWDataFromSftp Logg: '{0}' RemoveDuplicatesFromStaffingNeedFrequencyTables starts", result.ErrorMessage));
                        iem.RemoveDuplicatesFromStaffingNeedFrequencyTables();
                        CreateLogEntry(ScheduledJobLogLevel.Information, String.Format("RemoveDuplicatesFromStaffingNeedFrequencyTables Logg: '{0}'", result.ErrorMessage));
                        iem = null;
                    }

                    if (createXml.HasValue && createXml.Value)
                    {
                        if (1 > 0)
                        {
                            ImportExportManager iem = new ImportExportManager(parameterObject);
                            CreateLogEntry(ScheduledJobLogLevel.Information, "XML-production started");
                            CreateLogEntry(ScheduledJobLogLevel.Information, iem.HandleAxfoodExport(fromDate, toDate, split800, numberOfDays));
                            CreateLogEntry(ScheduledJobLogLevel.Information, "XML-production done");
                            iem = null;
                        }
                    }                    
                }
                catch (Exception ex)
                {
                    result = new ActionResult(ex);
                    CreateLogEntry(ScheduledJobLogLevel.Error, String.Format("Fel vid exekvering av jobb: '{0}'", result.ErrorMessage));
                    base.LogError(ex);
                }

                // Check in scheduled job
                CheckInScheduledJob(result.Success);
            }
        }
    }
}
