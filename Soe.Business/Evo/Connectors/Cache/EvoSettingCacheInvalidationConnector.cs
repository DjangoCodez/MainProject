using SO.Internal.Shared.Api.Cache.Settings;
using SoftOne.Soe.Business.Evo.Connectors;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Business.Util.LogCollector;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SoftOne.Soe.Business.Evo.Cache
{
    public class EvoSettingCacheInvalidationConnector : EvoConnectorBase
    {
        public static void InvalidateUserCompanySettingEditDTOs(List<UserCompanySettingEditDTO> userCompanySettings, int? userId, int? actorCompanyId, int? licenseId)
        {             
            SettingCacheInvalidationRequest request = new SettingCacheInvalidationRequest();
            foreach (var userCompanySetting in userCompanySettings)
            {
                if (userCompanySetting.SettingMainType == SettingMainType.Company && actorCompanyId.HasValue)                
                    request.SettingCacheInvalidationInputs.Add(SettingCacheInvalidationInput.ActorCompany(ConfigurationSetupUtil.GetCurrentSysCompDbId(), actorCompanyId.Value));
                else if (userCompanySetting.SettingMainType == SettingMainType.License && licenseId.HasValue)                
                    request.SettingCacheInvalidationInputs.Add(SettingCacheInvalidationInput.License(ConfigurationSetupUtil.GetCurrentSysCompDbId(), licenseId.Value));
                else if (userCompanySetting.SettingMainType == SettingMainType.User && userId.HasValue)                
                    request.SettingCacheInvalidationInputs.Add(SettingCacheInvalidationInput.User(ConfigurationSetupUtil.GetCurrentSysCompDbId(), userId.Value));
                else if (userCompanySetting.SettingMainType == SettingMainType.Application)                
                    request.SettingCacheInvalidationInputs.Add(SettingCacheInvalidationInput.Application(ConfigurationSetupUtil.GetCurrentSysCompDbId()));
                else if (userCompanySetting.SettingMainType == SettingMainType.UserAndCompany && actorCompanyId.HasValue && userId.HasValue)
                    request.SettingCacheInvalidationInputs.Add(SettingCacheInvalidationInput.UserAndCompany(ConfigurationSetupUtil.GetCurrentSysCompDbId(), userId.Value, actorCompanyId.Value));  
            }
            Task.Run(() => SettingCacheInvalidationClient.InvalidateCache(Url, Token, request));
        }

        public static void InvalidateCacheApplicationSetting(ApplicationSettingType setting)
        {
            SettingCacheInvalidationRequest request = new SettingCacheInvalidationRequest();
            request.SettingCacheInvalidationInputs.Add(SettingCacheInvalidationInput.Application(ConfigurationSetupUtil.GetCurrentSysCompDbId()));
            Task.Run(() => SettingCacheInvalidationClient.InvalidateCache(Url, Token, request));
        }

        public static void InvalidateCacheUserCompanySetting(int? licenseId, int? actorCompanyId, int? userId)
        {
            Task.Run(() => InvalidateCache(licenseId, actorCompanyId, userId));
        }

        public static SettingCacheInvalidationResponse InvalidateCache(int? licenseId, int? actorCompanyId, int? userId)
        {
            SettingCacheInvalidationRequest request = new SettingCacheInvalidationRequest();

            if (userId.HasValue && actorCompanyId.HasValue)
                request.SettingCacheInvalidationInputs.Add(SettingCacheInvalidationInput.UserAndCompany(ConfigurationSetupUtil.GetCurrentSysCompDbId(), userId.Value, actorCompanyId.Value));

            if (licenseId.HasValue)
                request.SettingCacheInvalidationInputs.Add(SettingCacheInvalidationInput.License(ConfigurationSetupUtil.GetCurrentSysCompDbId(), licenseId.Value));

            if (actorCompanyId.HasValue)
                request.SettingCacheInvalidationInputs.Add(SettingCacheInvalidationInput.ActorCompany(ConfigurationSetupUtil.GetCurrentSysCompDbId(), actorCompanyId.Value));

            if (userId.HasValue)
                request.SettingCacheInvalidationInputs.Add(SettingCacheInvalidationInput.User(ConfigurationSetupUtil.GetCurrentSysCompDbId(), userId.Value));

            var response = Task.Run(() => SettingCacheInvalidationClient.InvalidateCacheAsync(Url, Token, request)).GetAwaiter().GetResult();
            response = response ?? new SettingCacheInvalidationResponse(false, "Returned null");

            if (!response.Success)
                LogCollector.LogError(new System.Exception(response.Message), $"EvoSettingCacheInvalidationConnector {Url} l{licenseId}a{actorCompanyId}u{userId}");
            else if (ConfigurationSetupUtil.IsTestBasedOnMachine())
                LogCollector.LogInfo($"EvoSettingCacheInvalidationConnector {Url} l{licenseId}a{actorCompanyId}u{userId}");

            return response;
        }
    }
}
