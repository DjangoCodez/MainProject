using Soe.Sys.Common.DTO;
using SoftOne.Soe.Common.DTO.ClientManagement;
using System.Collections.Generic;
using System;

#nullable enable
namespace SoftOne.Soe.Business.Core.ClientManagement
{
    /// <summary>
    /// Generic interface for multi-company data aggregation operations.
    /// Supports retrieving and aggregating data from multiple companies across different databases.
    /// </summary>
    /// <typeparam name="TFilter">The type of filter/input parameters for data retrieval</typeparam>
    /// <typeparam name="TResult">The type of data being retrieved and aggregated</typeparam>
    public interface IMultiCompanyAggregator<TFilter, TResult>
    {
        /// <summary>
        /// Retrieves aggregated data from multiple companies in the multi-company group.
        /// </summary>
        /// <param name="multiCompanyId">The identifier of the multi-company group</param>
        /// <param name="filter">Filter criteria for data retrieval (can be null)</param>
        /// <returns>Aggregated response containing results and errors</returns>
        MultiCompanyResponseDTO<List<TResult>> GetAggregatedData(Guid mcCompanyGuid, TFilter? filter);

        /// <summary>
        /// Aggregates data from companies in the current database.
        /// </summary>
        /// <param name="companies">List of target companies in the current database</param>
        /// <param name="filter">Filter criteria for data retrieval (can be null)</param>
        /// <returns>Aggregated response from database operations</returns>
        MultiCompanyResponseDTO<List<TResult>> GetAggregatedDataFromDB(List<TargetCompanyDTO> companies, TFilter? filter);

        /// <summary>
        /// Aggregates data from companies in external databases via API calls.
        /// </summary>
        /// <param name="companies">List of target companies in external databases</param>
        /// <param name="filter">Filter criteria for data retrieval (can be null)</param>
        /// <returns>Aggregated response from API operations</returns>
        MultiCompanyResponseDTO<List<TResult>> GetAggregatedDataFromAPI(List<TargetCompanyDTO> companies, TFilter? filter);
    }
}
