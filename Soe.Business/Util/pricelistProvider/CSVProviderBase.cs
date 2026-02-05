using System;


namespace SoftOne.Soe.Business.Util.PricelistProvider
{
    public abstract class CSVProviderBase : DelimiterProviderBase
    {
        protected override char Delimiter
        {
            get
            {
                return ';';
            }
        }
    }

    public abstract class CSVProviderBase<T> : DelimiterProviderBase<T> where T : struct, IConvertible 
    {
        protected override char Delimiter
        {
            get
            {
                return ';';
            }
        }
    }
}
