using SoftOne.Soe.Common.Util;
using System;
using System.Text;

namespace SoftOne.Soe.Business.Util.PricelistProvider
{
    public class GenericPriceListColumnPositions
    {
        public int? ProductId { get; set; }
        public int? Name { get; set; }
        public int? ArticleGroup { get; set; }
        public int? EAN { get; set; }
        public int? SalesUnit { get; set; }
        public int? Price { get; set; }
        public int? PurchaseUnit { get; set; }
    }

    public class GenericCSVProvider : CSVProviderBase
    {
        private readonly string _wholesellerName;
        private readonly char _delimiter = (char)0;
        private readonly bool _cleanDoubleQutes = false;
        private readonly bool _skipFirstRow = false;
        private readonly GenericPriceListColumnPositions _columnPositions;
        private readonly Encoding _encoding = null;


        public GenericCSVProvider(SoeSysPriceListProvider provider, GenericPriceListColumnPositions columnPositions, char delimiter = (char)0, bool cleanDoubleQutes = false, bool skipFirstRow = false, Encoding encoding = null): base()
        {
            _wholesellerName = Enum.GetName(typeof(SoeSysPriceListProvider), provider);
            _columnPositions = columnPositions;
            _delimiter = delimiter;
            _skipFirstRow = skipFirstRow;
            _cleanDoubleQutes = cleanDoubleQutes;
            _encoding = encoding;
        }

        public GenericCSVProvider(SoeCompPriceListProvider provider, GenericPriceListColumnPositions columnPositions, char delimiter = (char)0, bool cleanDoubleQutes = false, bool skipFirstRow = false)
        {
            _wholesellerName = Enum.GetName(typeof(SoeCompPriceListProvider), provider);
            _columnPositions = columnPositions;
            _delimiter = delimiter;
            _skipFirstRow = skipFirstRow;
            _cleanDoubleQutes = cleanDoubleQutes;
        }
        
        protected override string WholesellerName
        {
            get { return _wholesellerName; }
        }

        protected override char Delimiter
        {
            get { return _delimiter == (char)0 ? base.Delimiter : _delimiter; }
        }

        protected override int SkipRows
        {
            get
            {
                // First line is a header line
                return _skipFirstRow ? 1:0;
            }
        }

        protected override Encoding FileEncoding
        {
            get { return _encoding ?? base.FileEncoding; }
        }

        protected override GenericProduct ToGenericProduct(string[] columns)
        {
            var product = new GenericProduct()
            {
                ProductId = _columnPositions.ProductId != null ? columns[(int)_columnPositions.ProductId].Trim() : null,
                Name = _columnPositions.Name != null ? columns[(int)_columnPositions.Name].Trim() : null,
                Price = _columnPositions.Price != null ? Convert.ToDecimal(columns[(int)_columnPositions.Price].Replace('.', ',').Trim()) : 0,
                Code = _columnPositions.ArticleGroup != null ? columns[(int)_columnPositions.ArticleGroup].Trim() : null,
                SalesUnit = _columnPositions.SalesUnit != null ? columns[(int)_columnPositions.SalesUnit].Trim() : null,
                EAN = _columnPositions.EAN != null ? columns[(int)_columnPositions.EAN].Trim() : null,
                WholesellerName = WholesellerName,
                PurchaseUnit = _columnPositions.PurchaseUnit != null ? columns[(int)_columnPositions.PurchaseUnit].Trim() : null,
            };

            if (_cleanDoubleQutes)
            {
                product.ProductId = product.ProductId?.Replace("\"", "");
                product.Name = product.Name?.Replace("\"", "");
                product.SalesUnit = product.SalesUnit?.Replace("\"", "");
                product.Code = product.Code?.Replace("\"", "");
                product.EAN = product.EAN?.Replace("\"", "");
            }

            return product;
        }
    }
}
