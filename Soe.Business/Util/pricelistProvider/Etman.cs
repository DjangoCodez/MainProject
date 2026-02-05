using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Util.Exceptions;
using System;

namespace SoftOne.Soe.Business.Util.PricelistProvider
{
    public class Etman : CSVProviderBase
    {
        private readonly string _wholesellerName = Enum.GetName(typeof(SoeCompPriceListProvider), SoeCompPriceListProvider.Etman);
        private enum ETMANColumnPositions
        {
            /// <summary>
            /// Artikelnummer
            /// </summary>
            ProductId = 0,
            /// <summary>
            /// Benämning
            /// </summary>
            Name = 1,
            Price = 2,
            Unit = 3,
            /// <summary>
            /// Bruttopris
            /// </summary>
        }

        private readonly SoeCompPriceListProvider provider;

        public Etman(SoeCompPriceListProvider provider)
        {
            if ( !(provider == SoeCompPriceListProvider.Etman || provider == SoeCompPriceListProvider.EtmanPipe ) )
            {
                throw new Exception("Etman Pricelist provider called with wrong provider: " + provider.ToString());
            }

            this.provider = provider;
            _wholesellerName = Enum.GetName(typeof(SoeCompPriceListProvider), provider);
        }

        protected override int SkipRows
        {
            get
            {
                return 1;
            }
        }

        protected override string WholesellerName
        {
            get { return _wholesellerName; }
        }
        
        protected override GenericProduct ToGenericProduct(string[] columns)
        {
            if (provider == SoeCompPriceListProvider.EtmanPipe)
            {
                var productNr = columns[0].Trim().Replace(" ", "");
                if ( productNr.StartsWith("E") )
                {
                    productNr = productNr.Remove(0, 1);
                }

                return new GenericProduct()
                {
                    ProductId = productNr,
                    Name = columns[1].Trim(),
                    Price = Convert.ToDecimal(columns[2].Replace('.', ',').Trim()),
                    WholesellerName = WholesellerName,
                };
            }
            else
            {
                if (columns.Length < 3)
                {
                    throw new ActionFailedException("Felaktig filtyp");
                }

                return new GenericProduct()
                {
                    ProductId = columns[(int)ETMANColumnPositions.ProductId].Trim(),
                    Name = columns[(int)ETMANColumnPositions.Name].Trim(),
                    Price = Convert.ToDecimal(columns[(int)ETMANColumnPositions.Price].Replace('.', ',').Trim()),
                    //Code = columns[(int)BevegoColumnPositions.ArticleGroup].Trim(),

                    SalesUnit = columns.Length > (int)ETMANColumnPositions.Unit ? columns[(int)ETMANColumnPositions.Unit].Trim() : "",
                    WholesellerName = WholesellerName,
                };
            }
        }
    }
}
