using Soe.Sys.Common.DTO;
using SoftOne.Soe.Business.Core.SysService;
using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Business.Util.WebApiInternal;
using SoftOne.Soe.Common.DTO.ClientManagement;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Threading.Tasks;

#nullable enable
namespace SoftOne.Soe.Business.Core.ClientManagement
{
    /// <summary>
    /// Base class providing common functionality for multi-company aggregators.
    /// </summary>
    /// <typeparam name="TFilter">The type of filter/input parameters</typeparam>
    /// <typeparam name="TResult">The type of data being aggregated</typeparam>
    public abstract class MultiCompanyAggregatorBase<TFilter, TResult> : ManagerBase, IMultiCompanyAggregator<TFilter, TResult>
    {
        protected MultiCompanyAggregatorBase(ParameterObject parameterObject) : base(parameterObject)
        {
        }

        /// <summary>
        /// Gets the feature type for permission checking and API routing.
        /// </summary>
        protected abstract ClientManagementResourceType ResourceType { get; }

        /// <summary>
        /// Gets the permission feature required for accessing the data.
        /// </summary>
        protected abstract Feature PermissionFeature { get; }

        /// <summary>
        /// Gets the error message text key for permission denied scenarios.
        /// </summary>
        private int PermissionDeniedTextKey => 6503;

        /// <summary>
        /// Template method for retrieving data for a single company.
        /// </summary>
        /// <param name="actorCompanyId">Company identifier</param>
        /// <param name="filter">Filter criteria, can be null</param>
        /// <returns>List of results for the specified company</returns>
        protected abstract List<TResult> GetDataForCompany(int actorCompanyId, TFilter? filter);

        public virtual MultiCompanyResponseDTO<List<TResult>> GetAggregatedData(Guid mcCompanyGuid, TFilter? filter)
        {
            int currentDbId = CompDbCache.Instance.SysCompDbId;
            var targetCompanies = SysMultiCompanyConnector.GetTargetCompanies(mcCompanyGuid, currentDbId);

            var companiesInThisDb = targetCompanies.Where(x => x.TCDbId == currentDbId).ToList();
            var companiesInOtherDbs = targetCompanies.Except(companiesInThisDb).ToList();

            var dbAggregatedResult = GetAggregatedDataFromDB(companiesInThisDb, filter);
            var otherDbAggregatedResult = GetAggregatedDataFromAPI(companiesInOtherDbs, filter);

            return new MultiCompanyResponseDTO<List<TResult>>
            {
                Value = dbAggregatedResult.Value.Concat(otherDbAggregatedResult.Value).ToList(),
                Errors = dbAggregatedResult.Errors.Concat(otherDbAggregatedResult.Errors).ToList()
            };
        }

        public virtual MultiCompanyResponseDTO<List<TResult>> GetAggregatedDataFromDB(
            List<TargetCompanyDTO> companies,
            TFilter? filter)
        {
            var tasks = new List<Task<MultiCompanyResponseDTO<List<TResult>>>>(companies.Count);

            foreach (var company in companies)
            {
                tasks.Add(Task.Run(() =>
                {
                    var result = new MultiCompanyResponseDTO<List<TResult>>();

                    if (!FeatureManager.HasRolePermission(
                        PermissionFeature,
                        Permission.Readonly,
                        company.TCRoleId,
                        company.TCActorCompanyId.GetValueOrDefault(),
                        company.TCLicenseId.GetValueOrDefault()))
                    {
                        result.Errors.Add(new MultiCompanyErrorDTO
                        {
                            TargetActorCompanyId = company.TCActorCompanyId ?? 0,
                            TargetCompanyName = company.TCActorCompanyId.HasValue
                                ? (base.GetCompanyFromCache(company.TCActorCompanyId.Value)?.Name ?? company.TCName)
                                : company.TCName,
                            ErrorMessage = string.Format(
                                base.GetText(PermissionDeniedTextKey, "Permission denied for company {0}."),
                                company.TCActorCompanyId.HasValue
                                    ? (base.GetCompanyFromCache(company.TCActorCompanyId.Value)?.Name ?? company.TCName)
                                    : company.TCName)
                        });

                        return result;
                    }

                    result.Value = GetDataForCompany(company.TCActorCompanyId.GetValueOrDefault(), filter);
                    return result;
                }));
            }

            var dbResults = Task.WhenAll(tasks).GetAwaiter().GetResult();
            return new MultiCompanyResponseDTO<List<TResult>>
            {
                Value = dbResults.SelectMany(r => r.Value ?? Enumerable.Empty<TResult>()).ToList(),
                Errors = dbResults.SelectMany(r => r.Errors ?? Enumerable.Empty<MultiCompanyErrorDTO>()).ToList()
            };
        }

        public virtual MultiCompanyResponseDTO<List<TResult>> GetAggregatedDataFromAPI(
            List<TargetCompanyDTO> companies,
            TFilter? filter)
        {
            var resultBag = new ConcurrentBag<MultiCompanyApiResponseDTO>();

            Parallel.ForEach(companies.GroupBy(x => x.TCApiUrl), GetDefaultParallelOptions(), companyGroup =>
            {
                var apiResponse = new ClientManagementConnector().ProcessMultiCompanyRequest(
                    companyGroup.Key,
                    new MultiCompanyApiRequestDTO
                    {
                        Feature = ResourceType,
                        Inputs = filter,
                        TargetCompanies = companyGroup.Cast<object>().ToList()
                    });

				if (apiResponse is null)
				{
					// Handle null response
					resultBag.Add(MultiCompanyApiResponseDTO.CreateError("Unable to get api response for company " + companyGroup.First().TCName));
				}
				else
					resultBag.Add(apiResponse);
			});

            return new MultiCompanyResponseDTO<List<TResult>>
            {
                Value = resultBag.SelectMany(x => x.Value?.Select(y => (TResult)y) ?? Enumerable.Empty<TResult>()).ToList(),
                Errors = resultBag.SelectMany(x => x.Errors ?? Enumerable.Empty<MultiCompanyErrorDTO>()).ToList()
                    ?? new List<MultiCompanyErrorDTO>()
            };
        }
    }
}
