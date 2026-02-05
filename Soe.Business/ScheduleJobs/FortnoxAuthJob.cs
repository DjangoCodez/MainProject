using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Interfaces;
using SoftOne.Soe.Business.Util.API.Fortnox;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;

namespace SoftOne.Soe.ScheduledJobs
{
    /// <summary>
    /// Fortnox's refresh tokens gets automatically deactivated after 30 days.
    /// This job will run periodically (every ~14 days), to ensure that tokens don't run out due to temporary inactivity (vacations etc.)
    /// 
    /// If the user wants to deactivate the integration, they cancel it on the Fortnox side.
    /// </summary>
    internal class FortnoxAuthJob : ScheduledJobBase, IScheduledJob

    {
        public void Execute(SysScheduledJobDTO scheduledJob, int batchNr)
        {
            base.Init(scheduledJob, batchNr);

            var result = this.CheckOutScheduledJob();
            var settingManager = new SettingManager(null);
            
            var settings = settingManager.GetAllCompanySettings((int)CompanySettingType.BillingFortnoxRefreshToken);
            var count = 0;


            foreach (var setting in settings)
            {
                if (setting.ActorCompanyId.IsNullOrEmpty() || setting.StrData.IsNullOrEmpty())
                {
                    this.CreateLogEntry(ScheduledJobLogLevel.Information, $"Invalid setting, UserCompanySettingId: {setting.UserCompanySettingId}, ActorCompanyId: {setting.ActorCompanyId}");
                    continue;
                }

                var fortnoxConnector = new FortnoxConnector();
                fortnoxConnector.SetAuthFromRefreshToken(setting.StrData);
                var refreshToken = fortnoxConnector.GetRefreshToken();

                if (refreshToken == null)
                {
                    this.CreateLogEntry(ScheduledJobLogLevel.Information, $"Received null token, UserCompanySettingId: {setting.UserCompanySettingId}, ActorCompanyId: {setting.ActorCompanyId}");
                    continue;
                }

                var updateResult = settingManager.UpdateInsertStringSetting(SettingMainType.Company, (int)CompanySettingType.BillingFortnoxRefreshToken,
                    refreshToken, 0, setting.ActorCompanyId.ToInt(), 0);
                
                if (!updateResult.Success)
                {
                    this.CreateLogEntry(ScheduledJobLogLevel.Error, $"Failed to save new token, ID: {setting.UserCompanySettingId}, token: {refreshToken}");
                    result.Success = false;
                }

                count++;
            }

            this.CreateLogEntry(ScheduledJobLogLevel.Success, $"Updated {count} of {settings.Count} settings.");
            this.CheckInScheduledJob(result.Success);
        }
    }
}
