using SoftOne.Soe.Common.DTO;

namespace SoftOne.Soe.Data
{
    public partial class SupplierProduct
    {

    }

    public static partial class EntityExtensions
    {
        public static SupplierProductDTO ToDTO(this SupplierProduct e)
        {
            if (e == null)
                return null;

            var dto = new SupplierProductDTO
            {
                SupplierProductId = e.SupplierProductId,
                SupplierProductNr = e.SupplierProductNr,
                SupplierProductName = e.Name,
                SupplierId = e.SupplierId,
                SupplierProductCode = e.Code,
                SupplierProductUnitId = e.SupplierProductUnitId,
                PackSize = e.PackSize,
                DeliveryLeadTimeDays = e.DeliveryLeadTimeDays,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                ProductId = e.ProductId.GetValueOrDefault(),
                SysCountryId = e.SysCountryId.GetValueOrDefault(),
            };

            if(e.Product != null)
            {
                if (e.Product.GetType() == typeof(InvoiceProduct))
                {
                    dto.SysCountryId = ((InvoiceProduct)e.Product).SysCountryId;
                    dto.IntrastatCodeId = ((InvoiceProduct)e.Product).IntrastatCodeId;
                }
            }

            return dto;
        }
    }
}
