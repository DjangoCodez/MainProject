using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.Business.Util.PricelistProvider
{
    public class Rexel : CSVProviderBase
    {
        private enum RexelFileTypes
        {
            SysGNP = 1,
            Netto = 2,
            NettoAssemblin = 3,
            NettoAssemblin2 = 4,
            NettoInstalco = 5,
        }

        public enum RexelPricelistColumnPositions
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
            /// Enhet
            /// </summary>
            Unit = 2,
            /// <summary>
            /// Artikelgrupp
            /// </summary>
            ArticleGroup = 3,
            /// <summary>
            /// Nettopris
            /// </summary>
            PurchasePrice = 4,
            /// <summary>
            /// Prisdatum
            /// </summary>
            PriceDate = 5,
        }

        //Enummer;Benämning;Enhet;Rexel_MG;Rexel_GN;IC20;Lagerförd
        private enum RexelNettoPricelistColumnPositions
        {
            ProductId = 0,
            Name = 1,
            PurchaseUnit = 2,
            ArticleGroup = 3,
            PurchasePrice = 5,
        }

        //Enummer;Benämning;Enhet;Materialgrupp;GN;Leverantör;Nettopris;Lagerförd
        private enum RexelInstalcoNettoPricelistColumnPositions
        {
            ProductId = 0,
            Name = 1,
            PurchaseUnit = 2,
            ArticleGroup = 3,
            PurchasePrice = 6,
        }

        //Enummer;Benämning;Lista;Leverantör;Rexel_GN;Rexel_MG;Enhet;Pris;NettoRabatt;Lagerförd
        private enum RexelAssemblinNettoPricelistColumnPositions
        {
            ProductId = 0,
            Name = 1,
            ArticleGroup = 5,
            PurchaseUnit = 6,
            PurchasePrice = 7,
        }

        //Enummer;Benämning;Enhet;Rexel_MG;Rexel_GN;Prisdatum;ALEM;Lagerförd;Status;NettoRabatt;NettoPris
        //Vet inte om den ovan uttgått så behåller den också...
        private enum RexelAssemblin2NettoPricelistColumnPositions
        {
            ProductId = 0,
            Name = 1,
            PurchaseUnit = 2,
            ArticleGroup = 3,
            PurchasePrice = 10,
        }

        private readonly string _wholeSeller;
        private RexelFileTypes _fileType;
        private bool _firstRow = true;

        protected override string WholesellerName
        {
            get { return _wholeSeller; }
        }

        public Rexel(bool isCompRexelNetto=false)
        {
            _fileType = isCompRexelNetto ? RexelFileTypes.Netto : RexelFileTypes.SysGNP;
            _wholeSeller = Enum.GetName(typeof(SoeSysPriceListProvider), SoeSysPriceListProvider.Rexel);
        }

        protected override GenericProduct ToGenericProduct(string[] columns)
        {
            GenericProduct product;

            if (_fileType !=  RexelFileTypes.SysGNP)
            {
                if (_firstRow)
                {
                    if (columns.Length == 10 && columns[(int)RexelAssemblinNettoPricelistColumnPositions.PurchaseUnit] == "Enhet")
                    {
                        _fileType = RexelFileTypes.NettoAssemblin;
                        _firstRow = false;
                        return null;
                    }
                    else if (columns.Length == 11 && columns[(int)RexelAssemblin2NettoPricelistColumnPositions.PurchaseUnit] == "Enhet")
                    {

                        _fileType = RexelFileTypes.NettoAssemblin2;
                        _firstRow = false;
                        return null;
                    }
                    else if (columns.Length == 8 && columns[(int)RexelInstalcoNettoPricelistColumnPositions.PurchasePrice] == "Nettopris")
                    {

                        _fileType = RexelFileTypes.NettoInstalco;
                        _firstRow = false;
                        return null;
                    }
                    else if (columns.Length == 7 && columns[(int)RexelNettoPricelistColumnPositions.PurchaseUnit] == "Enhet")
                    {
                        _firstRow = false;
                        return null;
                    }
                }

                if (_fileType == RexelFileTypes.NettoAssemblin)
                {
                    product = new GenericProduct
                    {
                        ProductId = columns[(int)RexelAssemblinNettoPricelistColumnPositions.ProductId].Trim(),
                        Name = columns[(int)RexelAssemblinNettoPricelistColumnPositions.Name].Trim(),
                        Price = Convert.ToDecimal(columns[(int)RexelAssemblinNettoPricelistColumnPositions.PurchasePrice].Trim().Replace('.', ',')),
                        Code = columns[(int)RexelAssemblinNettoPricelistColumnPositions.ArticleGroup].Trim(),
                        PurchaseUnit = columns[(int)RexelAssemblinNettoPricelistColumnPositions.PurchaseUnit].Trim(),
                        WholesellerName = WholesellerName,
                    };
                }
                else if (_fileType == RexelFileTypes.NettoAssemblin2)
                {
                    product = new GenericProduct
                    {
                        ProductId = columns[(int)RexelAssemblin2NettoPricelistColumnPositions.ProductId].Trim(),
                        Name = columns[(int)RexelAssemblin2NettoPricelistColumnPositions.Name].Trim(),
                        Price = Convert.ToDecimal(columns[(int)RexelAssemblin2NettoPricelistColumnPositions.PurchasePrice].Trim().Replace('.', ',')),
                        Code = columns[(int)RexelAssemblin2NettoPricelistColumnPositions.ArticleGroup].Trim(),
                        PurchaseUnit = columns[(int)RexelAssemblin2NettoPricelistColumnPositions.PurchaseUnit].Trim(),
                        WholesellerName = WholesellerName,
                    };
                }
                else if (_fileType == RexelFileTypes.NettoInstalco)
                {
                    product = new GenericProduct
                    {
                        ProductId = columns[(int)RexelInstalcoNettoPricelistColumnPositions.ProductId].Trim(),
                        Name = columns[(int)RexelInstalcoNettoPricelistColumnPositions.Name].Trim(),
                        Price = Convert.ToDecimal(columns[(int)RexelInstalcoNettoPricelistColumnPositions.PurchasePrice].Trim().Replace('.', ',')),
                        Code = columns[(int)RexelInstalcoNettoPricelistColumnPositions.ArticleGroup].Trim(),
                        PurchaseUnit = columns[(int)RexelInstalcoNettoPricelistColumnPositions.PurchaseUnit].Trim(),
                        WholesellerName = WholesellerName,
                    };
                }
                else
                {
                    product = new GenericProduct
                    {
                        ProductId = columns[(int)RexelNettoPricelistColumnPositions.ProductId].Trim(),
                        Name = columns[(int)RexelNettoPricelistColumnPositions.Name].Trim(),
                        Price = Convert.ToDecimal(columns[(int)RexelNettoPricelistColumnPositions.PurchasePrice].Trim().Replace('.', ',')),
                        Code = columns[(int)RexelNettoPricelistColumnPositions.ArticleGroup].Trim(),
                        PurchaseUnit = columns[(int)RexelNettoPricelistColumnPositions.PurchaseUnit].Trim(),
                        WholesellerName = WholesellerName,
                    };
                }
                return product;
            }
            else
            {
                DateTime priceDate;
                product = new GenericProduct
                {
                    ProductId = columns[(int)RexelPricelistColumnPositions.ProductId].Trim(),
                    Name = columns[(int)RexelPricelistColumnPositions.Name].Trim(),
                    // Fetch from purchase price since discount is added to that price.
                    Price = Convert.ToDecimal(columns[(int)RexelPricelistColumnPositions.PurchasePrice].Trim().Replace('.', ',')),
                    Code = columns[(int)RexelPricelistColumnPositions.ArticleGroup].Trim(),
                    PurchaseUnit = columns[(int)RexelPricelistColumnPositions.Unit].Trim(),
                    SalesUnit = columns[(int)RexelPricelistColumnPositions.Unit].Trim(),
                    WholesellerName = WholesellerName,
                };

                if (columns[(int)RexelPricelistColumnPositions.PriceDate].Trim().Length == 8)
                {
                    string date = columns[(int)RexelPricelistColumnPositions.PriceDate].Trim();
                    date = date.Insert(4, "-").Insert(7, "-");
                    if (DateTime.TryParse(date, out priceDate))
                        product.PriceChangeDate = priceDate;
                }
            }
            
            _firstRow = false;
            return product;
        }

        #region RexelPost
        public class RexelPost
        {
            #region Members
            public string ProductId { get; set; }
            public string Name { get; set; }
            public string MaterialCode { get; set; }
            public decimal Price { get; set; }
            public DateTime PriceChangeDate { get; set; }
            public string Unit { get; set; }
            #endregion

            #region Constructors

            /// <summary>
            /// Used to mock in unit test
            /// </summary>
            public RexelPost()
            {
                //Empty constructor
            }


            public RexelPost(string item)
            {
                try
                {
                    ProductId = item.Substring(0, 7);
                    Name = item.Substring(7, 25).Trim();
                    //MaterialCode = item.Substring(32, 8).Trim();
                    //Price = GetAmount(item.Substring(40, 4));
                    Price = GetAmount(item.Substring(32, 8));
                    MaterialCode = item.Substring(40, 4).Trim();
                    Unit = item.Substring(44, 2).Trim();
                    PriceChangeDate = GetDate(item.Substring(44, 6));
                }
                catch (Exception ex)
                {
                    ex.ToString(); //prevent compiler warning
                }
            }
            #endregion

            #region Help methods
            private int GetEan(string item)
            {
                int result = 0;
                item = item.Trim();
                if (!string.IsNullOrEmpty(item))
                    int.TryParse(item, out result);
                return result;
            }
            private decimal GetAmount(string item)
            {
                if (String.IsNullOrEmpty(item.Trim()))
                    return 0M;
                return Convert.ToDecimal(Convert.ToInt32(item) / 100M);
            }
            public static DateTime GetDate(string date)
            {
                int year = Convert.ToInt32(DateTime.Now.Year.ToString().Substring(0, 2) + date.Substring(0, 2));
                int month = Convert.ToInt32(date.Substring(2, 2));
                int day = Convert.ToInt32(date.Substring(4, 2));
                return new DateTime(year, month, day);
            }
            #endregion
        }
        #endregion
    }
}
