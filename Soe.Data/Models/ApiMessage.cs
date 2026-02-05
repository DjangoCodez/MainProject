using SoftOne.Soe.Common.Interfaces.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Data
{
    public partial class ApiMessage : ICreatedModified
    {

    }

    public static partial class EntityExtensions
    {
        #region ApiMessage

        public static bool ContainsChange(this List<ApiMessageChange> l, int recordId, int fieldType, string toValue, DateTime? fromDate, DateTime? toDate)
        {
            return l?.Any(i => i.RecordId == recordId && i.FieldType == fieldType && i.ToValue == toValue && i.FromDate == fromDate && i.ToDate == toDate) ?? false;
        }

        #endregion
    }
}
