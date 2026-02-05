using SoftOne.Soe.Business.Core.ManagerWrappers;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Soe.Business.Tests.Business.Core.AccountDistribution.Stubs
{
    internal class StubStateUtility : IStateUtility
    {
        public List<object> AddedObjects { get; } = new List<object>();
        public List<object> DeletedObjects { get; } = new List<object>();
        public List<object> DetachedObjects { get; } = new List<object>();

        public bool DetachResultToReturn { get; set; } = true;


        public void AddObject(string table, EntityObject entity)
        {
            AddedObjects.Add(entity);
        }

        public void DeleteObject(object entity)
        {
            DeletedObjects.Add(entity);
        }

        public ActionResult MarkAsDeleted(EntityObject entity)
        {
            DeletedObjects.Add(entity);
            return new ActionResult(true);
        }

        public bool DetachObject(EntityObject entity)
        {
            DetachedObjects.Add(entity);
            return DetachResultToReturn;
        }

        public void SetCreatedProperties(EntityObject entity, IUser user = null, DateTime? created = null)
        {
        }
    }
}
