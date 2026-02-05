using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Core.TimerConvert;
using SoftOne.Soe.Business.Interfaces;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Linq;

namespace SoftOne.Soe.ScheduledJobs
{
    public enum ConversionJobSourceType
    {
        WebTid = 0,
        Timer = 1,
    }


    public class WTConversionJob : ScheduledJobBase, IScheduledJob
    {
        public void Execute(SysScheduledJobDTO scheduledJob, int batchNr)
        {
            #region Init

            base.Init(scheduledJob, batchNr);

            UserManager um = new UserManager(parameterObject);
            WtConvertManager wtcm = new WtConvertManager(parameterObject);
            TimerConvertManager tcm = new TimerConvertManager(parameterObject);

            ConversionJobSourceType sourceType;

            #endregion

            #region Read settings

            int serviceUserId = 28;
            string wtConnString = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "connectionstring").Select(s => s.StrData).FirstOrDefault();
            string siteName = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "sitename").Select(s => s.StrData).FirstOrDefault();
            bool? firstFirst = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "firstfirst").Select(s => s.BoolData).FirstOrDefault();
            string sourceName = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "sourcename").Select(s => s.StrData).FirstOrDefault();
            int? timerActorCompanyId = 0;

            switch (sourceName)
            {
                case "Timer":
                    sourceType = ConversionJobSourceType.Timer;
                    timerActorCompanyId = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "actorcompanyid").Select(s => s.IntData).FirstOrDefault();
                    break;
                default:
                    sourceType = ConversionJobSourceType.WebTid;
                    break;
            }

            bool noTransactions = false;
            var notransactionsItem = scheduledJob.SysJobSettings.FirstOrDefault(s => s.Name.ToLower() == "notransactions" && s.BoolData.HasValue);
            if (notransactionsItem != null && notransactionsItem.BoolData.HasValue && notransactionsItem.BoolData.Value)
            {
                noTransactions = false;
            }

            bool noSchedules = false;
            var noschedulesItem = scheduledJob.SysJobSettings.FirstOrDefault(s => s.Name.ToLower() == "noschedules" && s.BoolData.HasValue);
            if (noschedulesItem != null && noschedulesItem.BoolData.HasValue && noschedulesItem.BoolData.Value)
            {
                noSchedules = false;
            }

            bool limitHolidays = false;
            var limitholidaysItem = scheduledJob.SysJobSettings.FirstOrDefault(s => s.Name.ToLower() == "limitholidays" && s.BoolData.HasValue);
            if (limitholidaysItem != null && limitholidaysItem.BoolData.HasValue && limitholidaysItem.BoolData.Value)
            {
                limitHolidays = false;
            }

            #endregion

            #region Parse settings

            //NameStandard
            NameStandard nameStandard;
            if (!firstFirst.HasValue || firstFirst.Value)
                nameStandard = NameStandard.FirstNameThenLastName;
            else
                nameStandard = NameStandard.LastNameThenFirstName;

            //WT Company
            WtConvertManager.WebbTidCompanyConvert wtCompany = new WtConvertManager.WebbTidCompanyConvert();
            if (sourceType == ConversionJobSourceType.WebTid)
            {
                wtCompany = wtcm.GetWtCompanyToConvert(wtConnString);
                if (wtCompany.WebbtidCompanyId == 0)
                {
                    CreateLogEntry(ScheduledJobLogLevel.Error, "No WT Companies to convert");
                    return;
                }
            }
            
            //XE User
            User serviceUser = um.GetUser(serviceUserId);
            if (serviceUser == null)
            {
                CreateLogEntry(ScheduledJobLogLevel.Error, "Service User not found");
                return;
            }

            #endregion

            #region Perform job

            // Check out scheduled job
            ActionResult result = CheckOutScheduledJob();
            if (result.Success)
            {
                try
                {
                    if (sourceType == ConversionJobSourceType.Timer)
                        result = tcm.ExecuteTimerConversion(wtConnString, serviceUser, siteName, (int)timerActorCompanyId, batchNr, scheduledJob.SysScheduledJobId, noTransactions, noSchedules, limitHolidays, true, nameStandard);
                    else
                        result = wtcm.ExecuteWTConversion(wtConnString, wtCompany.WebbtidCompanyId, serviceUser, siteName, wtCompany.StartDateTime, wtCompany.EndDateTime, wtCompany.ActorCompanyId, batchNr, scheduledJob.SysScheduledJobId, noTransactions, noSchedules, limitHolidays, true, nameStandard);
                }
                catch (Exception ex)
                {
                    result = new ActionResult(ex);
                    base.LogError(ex);
                }

                if (!result.Success)
                {
                    #region Logging

                    string prefix = "Conversion failed. ";
                    switch (result.ErrorNumber)
                    {
                        case (int)ActionResultSave.EntityNotFound:
                            CreateLogEntry(ScheduledJobLogLevel.Error, prefix + "EntityNotFound [" + result.ErrorMessage + "]");
                            break;
                        case (int)ActionResultSave.Unknown:
                            CreateLogEntry(ScheduledJobLogLevel.Error, prefix + result.ErrorMessage);
                            break;
                    }

                    #endregion
                }

                // Check in scheduled job
                CheckInScheduledJob(result.Success);
            }

            #endregion
        }
    }
}
