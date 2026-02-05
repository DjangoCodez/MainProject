using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data.Util;
using System;

namespace SoftOne.Soe.Data
{
    public partial class Product : ICreatedModified, IState
    {
        public string NumberSort
        {
            get
            {
                return StringUtility.IsNumeric(Number) ? this.Number.PadLeft(100, '0') : this.Number;
            }
        }
    }

    public partial class ProductImported : ICreatedModified
    {

    }

    public static partial class EntityExtensions
    {
        #region Product

        public static ProductDTO ToDTO(this Product e)
        {
            if (e == null)
                return null;

            #region Try load

            try
            {
                if (!e.IsAdded() && !e.ProductUnitReference.IsLoaded)
                {
                    e.ProductUnitReference.Load();
                    DataProjectLogCollector.LogLoadedEntityInExtension("Product.cs e.ProductUnitReference");
                }
            }
            catch (InvalidOperationException ex) { ex.ToString(); }

            #endregion

            ProductDTO dto = new ProductDTO()
            {
                ProductId = e.ProductId,
                Number = e.Number,
                Name = e.Name,
                ProductUnitId = e.ProductUnitId,
                ProductGroupId = e.ProductGroupId,
                Type = e.Type,
                Description = e.Description,
                AccountingPrio = e.AccountingPrio,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = e.State
            };

            // Extensions
            dto.ProductUnitCode = e.ProductUnit?.Code ?? string.Empty;

            return dto;
        }

        #endregion
    }
}
