using SO.Internal.Shared.Api.Cache.Features;
using SoftOne.Soe.Business.Evo.Connectors;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Business.Util.LogCollector;
using System.Threading.Tasks;

namespace SoftOne.Soe.Business.Evo.Cache
{
    public class EvoFeatureCacheInvalidationConnector : EvoConnectorBase
    {
        public static void InvalidateLicenseCache(int? licenseId) => 
            Task.Run(() => InvalidationResponse(licenseId, null, null));
        public static void InvalidateCompanyCache(int? actorCompanyId) => 
            Task.Run(() => InvalidationResponse(null, actorCompanyId, null));
        public static void InvalidateRoleCache(int? roleId) =>
            Task.Run(() => InvalidationResponse(null, null, roleId));
        public static void InvalidateCache(int? licenseId, int? actorCompanyId, int? roleId) =>
            Task.Run(() => InvalidationResponse(licenseId, actorCompanyId, roleId));

        private static FeatureCacheInvalidationResponse InvalidationResponse(int? licenseId, int? actorCompanyId, int? roleId)
        {
            FeatureCacheInvalidationRequest request = new FeatureCacheInvalidationRequest();

            if (licenseId.HasValue)
                request.FeatureCacheInvalidationInputs.Add(FeatureCacheInvalidationInput.License(ConfigurationSetupUtil.GetCurrentSysCompDbId(), licenseId.Value));

            if (actorCompanyId.HasValue)
                request.FeatureCacheInvalidationInputs.Add(FeatureCacheInvalidationInput.ActorCompany(ConfigurationSetupUtil.GetCurrentSysCompDbId(), actorCompanyId.Value));

            if (roleId.HasValue)
                request.FeatureCacheInvalidationInputs.Add(FeatureCacheInvalidationInput.Role(ConfigurationSetupUtil.GetCurrentSysCompDbId(), roleId.Value));

            var response = Task.Run(() => FeatureInvalidationClient.InvalidateCacheAsync(Url, Token, request)).GetAwaiter().GetResult();

            response = response ?? new FeatureCacheInvalidationResponse(false, "Returned null");

            if (!response.Success)
                LogCollector.LogError(new System.Exception(response.Message), $"EvoFeatureCacheInvalidationConnector {Url} l{licenseId}a{actorCompanyId}t{roleId}");
            else if (ConfigurationSetupUtil.IsTestBasedOnMachine())
                LogCollector.LogInfo($"EvoFeatureCacheInvalidationConnector {Url} l{licenseId}a{actorCompanyId}t{roleId}");

            return response;
        }
    }
}
