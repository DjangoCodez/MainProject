using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;

namespace SoftOne.Soe.Data
{
    public partial class ProductAccountStd
    {

    }

    public static partial class EntityExtensions
    {
        #region ProductAccountStd

        public static ProductAccountStdDTO ToDTO(this ProductAccountStd e, bool includeInternalAccounts)
        {
            if (e == null)
                return null;

            ProductAccountStdDTO dto = new ProductAccountStdDTO()
            {
                ProductAccountStdId = e.ProductAccountStdId,
                ProductId = e.Product?.ProductId ?? 0,        // Add foreign key to model
                AccountId = e.AccountStd?.AccountId ?? 0,     // Add foreign key to model
                Type = (ProductAccountType)e.Type,
                Percent = e.Percent
            };

            // Extensions
            dto.AccountStd = e.AccountStd?.Account?.ToDTO();
            if (includeInternalAccounts)
                dto.AccountInternals = e.AccountInternal.ToDTOs();

            return dto;
        }

        #endregion
    }
}
