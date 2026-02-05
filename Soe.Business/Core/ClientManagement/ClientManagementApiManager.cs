using Soe.Sys.Common.DTO;
using SoftOne.Soe.Business.Core.ClientManagement.Aggregators;
using SoftOne.Soe.Common.DTO.ClientManagement;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Core.ClientManagement
{
    public class ClientManagementApiManager : ManagerBase
    {
        // Strongly-typed handler delegates
        private readonly Dictionary<ClientManagementResourceType, Func<MultiCompanyApiRequestDTO, object>> _handlers;

        public ClientManagementApiManager(ParameterObject parameterObject) : base(parameterObject)
        {
            // Register handlers for each resource type
            _handlers = new Dictionary<ClientManagementResourceType, Func<MultiCompanyApiRequestDTO, object>>
            {
                { ClientManagementResourceType.GetSupplierInvoices, HandleSupplierInvoicesRequest },
                { ClientManagementResourceType.GetSupplierInvoicesSummary, HandleSupplierInvoicesSummaryRequest },

                // Add more handlers here:
                // { ClientManagementResourceType.GetCustomerInvoices, HandleCustomerInvoicesRequest },
                // { ClientManagementResourceType.GetOrders, HandleOrdersRequest }
            };
        }

        public object ProcessMultiCompanyRequest(MultiCompanyApiRequestDTO request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (!_handlers.TryGetValue(request.Feature, out var handler))
            {
                throw new NotSupportedException($"Resource type '{request.Feature}' is not supported.");
            }

            return handler(request);
        }

		#region Private Handlers
		private object HandleSupplierInvoicesRequest(MultiCompanyApiRequestDTO request)
        {
            var aggregator = new SupplierInvoiceAggregator(base.parameterObject);
            var filter = request.Inputs as MCSupplierInvoicesFilterDTO;
            var targetCompanies = request.TargetCompanies.Cast<TargetCompanyDTO>().ToList();

            return aggregator.GetAggregatedDataFromDB(targetCompanies, filter);
        }

		private object HandleSupplierInvoicesSummaryRequest(MultiCompanyApiRequestDTO request)
		{
			var aggregator = new SupplierInvoiceSummaryAggregator(base.parameterObject);
			var targetCompanies = request.TargetCompanies.Cast<TargetCompanyDTO>().ToList();

			return aggregator.GetAggregatedDataFromDB(targetCompanies, null);
		}

		// Example of how to add more handlers:
		/*
        private object HandleCustomerInvoicesRequest(MultiCompanyApiRequestDTO request)
        {
            var aggregator = new CustomerInvoiceAggregator(base.ParameterObject);
            var filter = request.Inputs as MCCustomerInvoicesFilterDTO;
            var targetCompanies = request.TargetCompanies.Cast<TargetCompanyDTO>().ToList();

            return aggregator.GetAggregatedDataFromDB(targetCompanies, filter);
        }
        */

		#endregion
	}
}