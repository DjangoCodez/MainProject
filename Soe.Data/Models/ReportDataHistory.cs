using SoftOne.Soe.Common.Interfaces.Common;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Data
{
    public partial class ReportDataHistory : ICreatedModified
    {

    }

    public static partial class EntityExtensions
    {
        #region ReportDataHistory

        public static List<ReportDataHistory> Sort(this IEnumerable<ReportDataHistory> l)
        {
            return l?.OrderBy(i => i.HeadTagId).ThenBy(i => i.TagId).ThenBy(i => i.Created).ToList() ?? new List<ReportDataHistory>();
        }

        #endregion
    }
}
