using SoftOne.Soe.Common.Interfaces.Common;
using System;

namespace SoftOne.Soe.Data
{
    public partial class ProjectTimeBlock : ICreatedModified, IState
    {
        public Guid? Guid { get; set; }
    }

    public static partial class EntityExtensions
    {
        #region ProjectTimeBlock

        #endregion
    }
}
