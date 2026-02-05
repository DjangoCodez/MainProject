using SoftOne.Soe.Common.Util;
using System;
using System.Text;

namespace SoftOne.Soe.Business.Util.PricelistProvider
{
    public class Trebolit : CSVProviderBase
    {
        private enum TrebolitColumnPositions
        {
            /// <summary>
            /// Artikelnummer
            /// </summary>
            ProductId = 0,
            /// <summary>
            /// Benämning
            /// </summary>
            Name = 1,
            /// <summary>
            /// Produktgrupp
            /// </summary>
            ArticleGroup = 2,
            //3 = Prisdatum
            /// <summary>
            /// Ut Pris
            /// </summary>
            Price = 4,
        }

        protected override string WholesellerName
        {
            get { return Enum.GetName(typeof(SoeCompPriceListProvider), SoeCompPriceListProvider.Trebolit); }
        }

        protected override Encoding FileEncoding
        {
            get
            {
                return Constants.ENCODING_IBM865;
            }
        }

        protected override int SkipRows
        {
            get
            {
                return 2;
            }
        }

        protected override GenericProduct ToGenericProduct(string[] columns)
        {
            return new GenericProduct()
            {
                ProductId = columns[(int)TrebolitColumnPositions.ProductId].Trim(),
                Name = columns[(int)TrebolitColumnPositions.Name].Trim(),
                Price = Convert.ToDecimal(columns[(int)TrebolitColumnPositions.Price].ToLower().Replace("kr", string.Empty).Trim()),
                WholesellerName = WholesellerName,
            };
        }
    }
}
