using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Common.Util
{
    public class NumericComparer : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            if (StringUtility.IsNumeric(x) && StringUtility.IsNumeric(y))
            {
                if (Convert.ToInt32(x) > Convert.ToInt32(y)) return 1;
                if (Convert.ToInt32(x) < Convert.ToInt32(y)) return -1;
                if (Convert.ToInt32(x) == Convert.ToInt32(y)) return 0;
            }

            if (StringUtility.IsNumeric(x) && !StringUtility.IsNumeric(y))
                return -1;

            if (!StringUtility.IsNumeric(x) && StringUtility.IsNumeric(y))
                return 1;

            return String.Compare(x, y);
        }
    }
}