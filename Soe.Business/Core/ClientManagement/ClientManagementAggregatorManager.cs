using SoftOne.Soe.Business.Core.ClientManagement.Aggregators;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.DTO.ClientManagement;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftOne.Soe.Business.Core
{
    public class ClientManagementAggregatorManager : ManagerBase
    {
        #region Lazy Load Aggregators

        private SupplierInvoiceSummaryAggregator supplierInvoiceSummaryAggregator;
        private SupplierInvoiceSummaryAggregator SupplierInvoiceSummaryAggregator
        {
            get 
            {
                return supplierInvoiceSummaryAggregator ?? (supplierInvoiceSummaryAggregator = 
                    new SupplierInvoiceSummaryAggregator(base.parameterObject));
			}
		}

		#endregion

		public ClientManagementAggregatorManager(ParameterObject parameterObject) 
            : base(parameterObject)
        {
        }

        public MultiCompanyResponseDTO<List<SupplierInvoiceSummaryDTO>> GetSupplierInvoiceSummaryAggregatedData(Guid mcCompanyGuid)
        {
            return SupplierInvoiceSummaryAggregator.GetAggregatedData(mcCompanyGuid, null);
		}
	}
}
