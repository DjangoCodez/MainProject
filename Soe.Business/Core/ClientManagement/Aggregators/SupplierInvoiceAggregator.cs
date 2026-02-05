using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.DTO.ClientManagement;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System.Collections.Generic;

namespace SoftOne.Soe.Business.Core.ClientManagement
{
    public class SupplierInvoiceAggregator : MultiCompanyAggregatorBase<MCSupplierInvoicesFilterDTO, SupplierInvoiceGridDTO>
    {
        public SupplierInvoiceAggregator(ParameterObject parameterObject) : base(parameterObject)
        {
        }

        protected override ClientManagementResourceType ResourceType =>
            ClientManagementResourceType.GetSupplierInvoices;

        protected override Feature PermissionFeature =>
            Feature.ClientManagement_Supplier_Invoices;

        protected override List<SupplierInvoiceGridDTO> GetDataForCompany(
            int actorCompanyId,
            MCSupplierInvoicesFilterDTO filter)
        {
            return SupplierInvoiceManager.GetSupplierInvoicesForGrid(
                actorCompanyId,
                filter.AllItemsSelection,
                filter.LoadOpen,
                filter.LoadClosed);
        }
    }
}
