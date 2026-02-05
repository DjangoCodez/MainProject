using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Business.Util.PricelistProvider
{
    public abstract class DelimiterProviderBase : RowBasedProviderBase
    {
        protected virtual int FirstColumnIndex { get { return 0; } }

        protected override GenericProduct ToGenericProduct(string line)
        {
            if (FirstColumnIndex > 0)
                line = line.AddLeft(FirstColumnIndex, this.Delimiter);


            var columns = line.Split(this.Delimiter);
            return this.ToGenericProduct(columns);
        }

        protected override GenericHeader ToGenericHeader(string header)
        {
            if (FirstColumnIndex > 0)
                header = header.AddLeft(FirstColumnIndex, this.Delimiter);

            var columns = header.Split(this.Delimiter);
            return ToGenericHeader(columns);
        }

        protected virtual GenericHeader ToGenericHeader(string[] columns)
        {
            // override
            return new GenericHeader(DateTime.Now);
        }

        protected abstract GenericProduct ToGenericProduct(string[] columns);
        protected abstract char Delimiter { get; }
    }

    public abstract class DelimiterProviderBase<T> : DelimiterProviderBase where T : struct, IConvertible
    {
        protected override GenericProduct ToGenericProduct(string[] columns)
        {
            var dict = new Dictionary<T, string>();

            foreach (var item in Enum.GetValues(typeof(T)))
            {
                var pos = (int)item;
                if (pos > 0 && pos < columns.Length)
                    dict.Add((T)item, columns[(int)item]);
            }

            return ToGenericProduct(dict);
        }

        protected abstract GenericProduct ToGenericProduct(Dictionary<T, string> dict);
    }
}
