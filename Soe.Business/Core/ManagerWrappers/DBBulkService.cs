using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System.Transactions;

namespace SoftOne.Soe.Business.Core.ManagerWrappers
{
    public interface IDBBulkService
    {
        ActionResult SaveChanges();
    }

    public class DBBulkService : IDBBulkService
    {
        ManagerBase _managerBase;
        CompEntities _entities;
        TransactionScope _transaction;
        public DBBulkService(CompEntities entities, ManagerBase managerBase, TransactionScope transaction = null)
        {
            _entities = entities;
            _managerBase = managerBase;
            _transaction = transaction;
        }
        public ActionResult SaveChanges()
        {
            return _managerBase.SaveChanges(_entities, _transaction);
        }
    }
}
