using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Interfaces;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace SoftOne.Soe.ScheduledJobs
{
    public class CompressDataStorage : ScheduledJobBase, IScheduledJob
    {
        public void Execute(SysScheduledJobDTO scheduledJob, int batchNr)
        {
            #region Init

            base.Init(scheduledJob, batchNr);
            GeneralManager gm = new GeneralManager(parameterObject);
            DateTime hoppla = DateTime.Now.AddYears(-5);

            int? paramCompanyId = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "actorcompanyid").Select(s => s.IntData).FirstOrDefault();

            #endregion

            // Check out scheduled job
            ActionResult result = CheckOutScheduledJob();
            if (result.Success)
            {
                try
                {
                    using (CompEntities entities1 = new CompEntities())
                    {
                        var companiesWithDataStorage = paramCompanyId.HasValue ? new List<int>(paramCompanyId.Value) : entities1.DataStorage.Select(s => s.ActorCompanyId).Distinct().ToList();

                        entities1.CommandTimeout = 30000;
                        int count = 0;
                        int total = companiesWithDataStorage.Count;
                        var totalMoved = 0;

                        foreach (var companyId in companiesWithDataStorage.OrderBy(o => o))
                        {
                            count++;
                            var company = entities1.Company.First(f => f.ActorCompanyId == companyId);
                            var dateLimit = DateTime.Now.AddMonths(-3);
                            var datastorageIds = entities1.DataStorage.Where(w => w.Created < dateLimit && w.ActorCompanyId == companyId && (w.DataCompressed != null || w.XMLCompressed != null)).Select(s => s.DataStorageId).ToList();

                            if (!datastorageIds.Any())
                                continue;

                            CreateLogEntry(ScheduledJobLogLevel.Information, $"{company.ActorCompanyId} # {company.Name} started with {datastorageIds.Count} not transferred storage count. {count}/{total}] ");
                            int internalCount = 0;
                            int companyDataSaved = 0;

                            foreach (var id in datastorageIds)
                            {
                                internalCount++;
                                var compressResult = gm.MoveToEvoDataStorage(entities1, entities1.DataStorage.First(f => f.DataStorageId == id), true);
                                companyDataSaved += 1;
                            }

                            Thread.Sleep(internalCount * 10); // slowing down process in order to not overexhausted transactionslogs
                            CreateLogEntry(ScheduledJobLogLevel.Information, $"{company.ActorCompanyId} # {company.Name} done. {companyDataSaved} moved. ");
                            totalMoved += companyDataSaved;
                        }

                        CreateLogEntry(ScheduledJobLogLevel.Information, $"Job done.{totalMoved} moved during job");

                    }

                    using (CompEntities entities = new CompEntities())
                    {
                        var companiesWithDataStorage = paramCompanyId.HasValue ? new List<int>(paramCompanyId.Value) : entities.DataStorage.Select(s => s.ActorCompanyId).Distinct().ToList();

                        entities.CommandTimeout = 30000;
                        int count = 0;
                        int total = companiesWithDataStorage.Count;
                        int totalMBsaved = 0;

                        foreach (var companyId in companiesWithDataStorage.OrderBy(o => o))
                        {
                            count++;
                            var company = entities.Company.First(f => f.ActorCompanyId == companyId);
                            var datastorageIds = entities.DataStorage.Where(w => w.ActorCompanyId == companyId && (w.Data != null || w.XML != null)).Select(s => s.DataStorageId).ToList();

                            if (!datastorageIds.Any())
                                continue;

                            CreateLogEntry(ScheduledJobLogLevel.Information, $"{company.ActorCompanyId} # {company.Name} started with {datastorageIds.Count} storage count. {count}/{total}] ");
                            int internalCount = 0;
                            int companyDataSaved = 0;

                            foreach (var id in datastorageIds)
                            {
                                internalCount++;
                                var compressResult = gm.CompressStorage(entities, entities.DataStorage.First(f => f.DataStorageId == id), true, false);
                                companyDataSaved += compressResult.IntegerValue;
                            }

                            Thread.Sleep(internalCount * 10); // slowing down process in order to not overexhausted transactionslogs
                            totalMBsaved += Convert.ToInt32(decimal.Divide(companyDataSaved, 1024));
                            CreateLogEntry(ScheduledJobLogLevel.Information, $"{company.ActorCompanyId} # {company.Name} done. {Convert.ToInt32(decimal.Divide(companyDataSaved, 1024))} MB gained");
                        }

                        CreateLogEntry(ScheduledJobLogLevel.Information, $"Job done.{totalMBsaved} MB gained during job");

                        var companiesReportPrintout = paramCompanyId.HasValue ? new List<int>(paramCompanyId.Value) : entities.ReportPrintout.Where(w => w.Data != null).Select(s => s.ActorCompanyId).Distinct().ToList();

                        entities.CommandTimeout = 30000;
                        int numberOfCompressed = 0;
                        ReportManager reportManager = new ReportManager(parameterObject);
                        total = companiesReportPrintout.Count;
                        foreach (var companyId in companiesReportPrintout.OrderBy(o => o))
                        {
                            count++;
                            var company = entities.Company.First(f => f.ActorCompanyId == companyId);
                            var reportPrintoutIds = entities.ReportPrintout.Where(w => w.ActorCompanyId == companyId && (w.Data != null || w.XML != null)).Select(s => s.ReportPrintoutId).ToList();
                            CreateLogEntry(ScheduledJobLogLevel.Information, $"{company.ActorCompanyId} # {company.Name} started with {reportPrintoutIds.Count} reportPrintout count. {count}/{total}] ");
                            int internalCount = 0;
                            int companyDataSaved = 0;

                            foreach (var id in reportPrintoutIds)
                            {
                                internalCount++;
                                ReportPrintout reportPrintout = entities.ReportPrintout.First(f => f.ReportPrintoutId == id);
                                reportManager.CompressReportPrintout(entities, reportPrintout);
                                numberOfCompressed++;
                            }

                            Thread.Sleep(internalCount * 2); // slowing down process in order to not overexhausted transactionslogs
                            totalMBsaved += Convert.ToInt32(decimal.Divide(companyDataSaved, 1024));
                            CreateLogEntry(ScheduledJobLogLevel.Information, $"{company.ActorCompanyId} # {company.Name} done compressing reportprintout");
                        }

                        CreateLogEntry(ScheduledJobLogLevel.Information, $"ReportPrintout compression job done.{numberOfCompressed} rows affected");

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
