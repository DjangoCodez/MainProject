using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Data
{
    public partial class ProductUnit : ICreatedModified
    {
        public string CodeAndName
        {
            get
            {
                if (String.IsNullOrEmpty(this.Code) || String.IsNullOrEmpty(this.Name))
                    return String.Empty;
                return $"{this.Code}. {this.Name}";
            }
        }
    }

    public static partial class EntityExtensions
    {
        #region ProductUnit

        public static ProductUnitDTO ToDTO(this ProductUnit e)
        {
            if (e == null)
                return null;

            return new ProductUnitDTO()
            {
                ProductUnitId = e.ProductUnitId,
                ActorCompanyId = e.Company?.ActorCompanyId ?? 0,  // Add foreign key to model
                Code = e.Code,
                Name = e.Name,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy
            };
        }

        public static IEnumerable<ProductUnitDTO> ToDTOs(this IEnumerable<ProductUnit> l)
        {
            var dtos = new List<ProductUnitDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static ProductUnitSmallDTO ToSmallDTO(this ProductUnit e)
        {
            if (e == null)
                return null;

            return new ProductUnitSmallDTO()
            {
                ProductUnitId = e.ProductUnitId,
                Code = e.Code,
                Name = e.Name,
            };
        }

        public static IEnumerable<ProductUnitSmallDTO> ToSmallDTOs(this IEnumerable<ProductUnit> l)
        {
            var dtos = new List<ProductUnitSmallDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToSmallDTO());
                }
            }
            return dtos;
        }

        public static IEnumerable<ProductUnitConvertDTO> ToDTOs(this IEnumerable<ProductUnitConvert> l)
        {
            var dtos = new List<ProductUnitConvertDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static ProductUnitConvertDTO ToDTO(this ProductUnitConvert e)
        {
            if (e == null)
                return null;

            return new ProductUnitConvertDTO()
            {
                ProductUnitConvertId = e.ProductUnitConvertId,
                ProductUnitId = e.ProductUnitId,
                ProductId = e.InvoiceProductId,
                ConvertFactor = e.ConvertFactor,
                ProductUnitName = e.ProductUnit?.Name ?? String.Empty,
            };
        }

        #endregion
    }
}
