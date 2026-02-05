using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.DTO.ClientManagement;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftOne.Soe.Business.Core.ClientManagement.Aggregators
{
    public class SupplierInvoiceSummaryAggregator : MultiCompanyAggregatorBase<object, SupplierInvoiceSummaryDTO>
	{
		public SupplierInvoiceSummaryAggregator(ParameterObject parameterObject) : base(parameterObject)
		{
		}

		protected override ClientManagementResourceType ResourceType =>
			ClientManagementResourceType.GetSupplierInvoicesSummary;

		protected override Feature PermissionFeature =>
			Feature.ClientManagement_Supplier_Invoices;

		protected override List<SupplierInvoiceSummaryDTO> GetDataForCompany(int actorCompanyId, object filter)
        {
            return SupplierInvoiceManager.GetSupplierInvoicesSummary(actorCompanyId);
		}
    }
}
