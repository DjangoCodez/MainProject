
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Data.Entity.Core.Objects.DataClasses;

namespace SoftOne.Soe.Business.Core.ManagerWrappers
{
    // Please keep this interface minimal - only methods/properties needed by multiple managers should be here
    // Only add utility methods that do not depend on specific manager functionality, such as bulksave, setcreatedproperties, etc.
    public interface IStateUtility {
        void SetCreatedProperties(EntityObject entity, IUser user = null, DateTime? created = null);
        ActionResult MarkAsDeleted(EntityObject entity);
        void AddObject(string table, EntityObject entity);
        void DeleteObject(object entity);
        bool DetachObject(EntityObject entity);
    }

    public class StateUtility : IStateUtility
    {
        ManagerBase _managerBase;
        CompEntities _entities;
        public StateUtility(CompEntities entities, ManagerBase managerBase)
        {
            _entities = entities;
            _managerBase = managerBase;
        }
        public void AddObject(string table, EntityObject entity)
        {
            _entities.AddObject(table, entity);
        }
        public void DeleteObject(object entity)
        {
            _entities.DeleteObject(entity);
        }
        public bool DetachObject(EntityObject entity)
        {
            return _managerBase.TryDetachEntity(_entities, entity);
        }
        public void SetCreatedProperties(EntityObject entity, IUser user = null, DateTime? created = null)
        {
            _managerBase.SetCreatedProperties(entity, user, created);

        }
        public ActionResult MarkAsDeleted(EntityObject entity)
        {
            return _managerBase.ChangeEntityState(_entities, entity, SoeEntityState.Deleted, false);
        }
    }

}
