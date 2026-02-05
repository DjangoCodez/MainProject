using Common.Util;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Data;
using System.Collections.Concurrent;
using System.Security.Claims;
using System.Threading;

namespace SoftOne.Soe.Business.Util
{
    public static class SessionCache
    {
        private static readonly ConcurrentDictionary<string, SemaphoreSlim> _semaphores = new ConcurrentDictionary<string, SemaphoreSlim>();
        private static SemaphoreSlim GetSemaphoreSlim(string lockKey) => _semaphores.GetOrAdd(lockKey, new SemaphoreSlim(1, 1));
        private static int _cacheSeconds = 60 * 10;
        private static int _waitMilliSeconds = 10000; // Wait up to 10 seconds
        private static string GetUserKey(int userId, int actorCompanyId) => $"GetUserFromCache[{userId}][{actorCompanyId}]";
        private static string GetUserSlimKey(int userId, int actorCompanyId) => $"GetUserFromCacheSlim_[{userId}][{actorCompanyId}]";
        private static string GetCompanyKey(int actorCompanyId) => $"GetCompanyFromCache[{actorCompanyId}]";
        private static string GetCompanySlimKey(int actorCompanyId) => $"GetCompanyFromCacheSlim_[{actorCompanyId}]";
        private static string GetEmployeeKey(int userId, int actorCompanyId) => $"GetEmployeeFromCache[{actorCompanyId}_{userId}]";
        private static string GetEmployeeSlimKey(int userId, int actorCompanyId) => $"GetEmployeeFromCacheSlim_[{actorCompanyId}_{userId}]";
        private static string GetDefaultRoleIdKey(int userId, int actorCompanyId) => $"GetDefaultRoleId[{actorCompanyId}_{userId}]";
        private static string GetDefaultRoleIdSlimKey(int userId, int actorCompanyId) => $"GetDefaultRoleIdSlim_[{actorCompanyId}_{userId}]";
        private static string GetExistUserCompanyRoleMappingKey(int userId, int actorCompanyId, int roleId) => $"ExistUserCompanyRoleMapping[{actorCompanyId}_{userId}_{roleId}]";
        private static string GetExistUserCompanyRoleMappingSlimKey(int userId, int actorCompanyId, int roleId) => $"ExistUserCompanyRoleMappingSlim_[{actorCompanyId}_{userId}_{roleId}]";

        public static void ReloadUser(int userId, int actorCompanyId)
        {
            if (userId != 0 && actorCompanyId != 0)
                GetUserFromDatabaseAndAddToCache(userId, actorCompanyId);
        }

        public static void RemoveUserFromCache(int userId, int actorCompanyId)
        {
            if (userId != 0 && actorCompanyId != 0)
                BusinessMemoryCache<UserDTO>.Delete(GetUserKey(userId, actorCompanyId), BusinessMemoryDistributionSetting.FullyHybridCache);
        }

        public static UserDTO GetUserFromCache(ClaimsIdentity identity)
        {
            if (int.TryParse(identity.FindFirst(SoeClaimTypes.UserId)?.Value, out int userId))
            {
                if (int.TryParse(identity.FindFirst(SoeClaimTypes.ActorCompanyId)?.Value, out int actorCompanyId))
                    return GetUserFromCache(userId, actorCompanyId);
            }
            return null;
        }
        public static UserDTO GetUserFromCache(int userId, int actorCompanyId)
        {
            if (userId == 0)
                return null;

            if (TryGetFromCache(GetUserKey(userId, actorCompanyId), out UserDTO user))
                return user;

            var semaphore = GetSemaphoreSlim(GetUserSlimKey(userId, actorCompanyId));
            if (!semaphore.Wait(_waitMilliSeconds))
                return null;

            try
            {
                if (TryGetFromCache(GetUserKey(userId, actorCompanyId), out user))
                    return user;

                return GetUserFromDatabaseAndAddToCache(userId, actorCompanyId);
            }
            finally
            {
                semaphore.Release();
            }
        }

        public static void ReloadCompany(int actorCompanyId)
        {
            GetCompanyFromDatabaseAndAddToCache(actorCompanyId);
        }

        public static CompanyDTO GetCompanyFromCache(ClaimsIdentity identity)
        {
            if (int.TryParse(identity.FindFirst(SoeClaimTypes.ActorCompanyId)?.Value, out int actorCompanyId))
                return GetCompanyFromCache(actorCompanyId);
            return null;
        }
        public static CompanyDTO GetCompanyFromCache(int actorCompanyId)
        {
            if (actorCompanyId == 0)
                return null;

            if (TryGetFromCache(GetCompanyKey(actorCompanyId), out CompanyDTO company))
                return company;

            var semaphore = GetSemaphoreSlim(GetCompanySlimKey(actorCompanyId));
            if (!semaphore.Wait(_waitMilliSeconds))
                return null;

            try
            {
                if (TryGetFromCache(GetCompanyKey(actorCompanyId), out company))
                    return company;

                return GetCompanyFromDatabaseAndAddToCache(actorCompanyId);
            }
            finally
            {
                semaphore.Release();
            }
        }
        public static EmployeeSmallDTO GetEmployeeFromCache(int userId, int actorCompanyId)
        {
            if (userId == 0 || actorCompanyId == 0)
                return null;

            if (TryGetFromCache(GetEmployeeKey(userId, actorCompanyId), out EmployeeSmallDTO employee))
                return employee;

            var semaphore = GetSemaphoreSlim(GetEmployeeSlimKey(userId, actorCompanyId));
            if (!semaphore.Wait(_waitMilliSeconds))
                return null;

            try
            {
                if (TryGetFromCache(GetEmployeeKey(userId, actorCompanyId), out employee))
                    return employee;

                return GetEmployeeFromDatabaseAndAddToCache(userId, actorCompanyId);
            }
            finally
            {
                semaphore.Release();
            }
        }
        public static int? GetDefaultRoleId(int userId, int actorCompanyId)
        {
            if (userId == 0)
                return null;

            if (TryGetFromCache(GetDefaultRoleIdKey(userId, actorCompanyId), out int? defaultRoleId))
                return defaultRoleId;

            var semaphore = GetSemaphoreSlim(GetDefaultRoleIdSlimKey(userId, actorCompanyId));
            if (!semaphore.Wait(_waitMilliSeconds))
                return null;

            try
            {
                if (TryGetFromCache(GetDefaultRoleIdKey(userId, actorCompanyId), out defaultRoleId))
                    return defaultRoleId;

                return GetDefaultRoleIdFromDatabaseAndAddToCache(userId, actorCompanyId) ?? 0;
            }
            finally
            {
                semaphore.Release();
            }
        }
        public static bool ExistUserCompanyRoleMapping(int userId, int actorCompanyId, int roleId)
        {
            if (TryGetFromCache(GetExistUserCompanyRoleMappingKey(userId, actorCompanyId, roleId), out bool? valid))
                return valid.Value;

            var semaphore = GetSemaphoreSlim(GetExistUserCompanyRoleMappingSlimKey(userId, actorCompanyId, roleId));
            if (!semaphore.Wait(_waitMilliSeconds))
                return false;

            try
            {
                if (TryGetFromCache(GetExistUserCompanyRoleMappingKey(userId, actorCompanyId, roleId), out valid))
                    return valid.Value;

                return ExistUserCompanyRoleMappingFromDatabaseAndAddToCache(userId, actorCompanyId, roleId);
            }
            finally
            {
                semaphore.Release();
            }
        }

        private static UserDTO GetUserFromDatabaseAndAddToCache(int userId, int actorCompanyId)
        {
            UserDTO user = null;
            if (userId != 0)
            {
                var defaultRoleId = GetDefaultRoleIdFromDatabaseAndAddToCache(userId, actorCompanyId) ?? 0;
                user = new UserManager(null).GetUser(userId, loadUserCompanyRole: true, loadLicense: true).ToDTO(defaultRoleId);
                BusinessMemoryCache<UserDTO>.Set(GetUserKey(userId, actorCompanyId), user, _cacheSeconds, BusinessMemoryDistributionSetting.FullyHybridCache);
            }
            return user;
        }
        private static CompanyDTO GetCompanyFromDatabaseAndAddToCache(int actorCompanyId)
        {
            CompanyDTO company = null;
            if (actorCompanyId != 0)
            {
                company = new CompanyManager(null).GetCompany(actorCompanyId, loadLicense: true).ToCompanyDTO();
                BusinessMemoryCache<CompanyDTO>.Set(GetCompanyKey(actorCompanyId), company, _cacheSeconds, BusinessMemoryDistributionSetting.FullyHybridCache);
            }
            return company;
        }
        private static EmployeeSmallDTO GetEmployeeFromDatabaseAndAddToCache(int userId, int actorCompanyId)
        {
            EmployeeSmallDTO employee = null;
            if (userId != 0)
            {
                employee = new EmployeeManager(null).GetEmployeeForUser(userId, actorCompanyId).ToSmallDTO();
                BusinessMemoryCache<EmployeeSmallDTO>.Set(GetEmployeeKey(userId, actorCompanyId), employee ?? new EmployeeSmallDTO(), _cacheSeconds, BusinessMemoryDistributionSetting.FullyHybridCache);
            }
            return employee == null || employee.EmployeeId == 0 ? null : employee;
        }
        private static int? GetDefaultRoleIdFromDatabaseAndAddToCache(int userId, int actorCompanyId)
        {
            int? defaultRoleId = null;
            if (userId != 0 && actorCompanyId != 0)
            {
                defaultRoleId = new UserManager(null).GetDefaultRoleId(actorCompanyId, userId);
                BusinessMemoryCache<int?>.Set(GetDefaultRoleIdKey(userId, actorCompanyId), defaultRoleId, _cacheSeconds, BusinessMemoryDistributionSetting.FullyHybridCache);
            }
            return defaultRoleId != 0 ? defaultRoleId : null;
        }
        private static bool ExistUserCompanyRoleMappingFromDatabaseAndAddToCache(int userId, int actorCompanyId, int roleId)
        {
            var exist = new UserManager(null).ExistUserCompanyRoleMapping(userId, actorCompanyId, roleId);
            BusinessMemoryCache<bool>.Set(GetExistUserCompanyRoleMappingKey(userId, actorCompanyId, roleId), exist, _cacheSeconds, BusinessMemoryDistributionSetting.FullyHybridCache);
            return exist;
        }
        private static bool TryGetFromCache<T>(string key, out T value)
        {
            value = BusinessMemoryCache<T>.Get(key, BusinessMemoryDistributionSetting.FullyHybridCache);
            return value != null;
        }
    }
}
