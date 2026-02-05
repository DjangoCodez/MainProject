using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftOne.EdiAdmin.Business.Core
{
    public class EdiSysManager : ManagerBase
    {
        public ActionResult AddSysWholesellerEdi(string senderId, string senderName, string ediFolder, SysWholesellerEdiManagerType type, string username = null, string password = null, int id = 0)
        {
            using (var entities = new SOESysEntities())
            {
                var ws = new SysWholesellerEdi()
                {
                    SenderId = senderId,
                    SenderName = senderName,
                    EdiFolder = ediFolder,
                    FtpUser = username,
                    FtpPassword = password,
                    EdiManagerType = (int)type,
                };

                if (id == 0)
                    ws.SysWholesellerEdiId = entities.SysWholesellerEdi.Select(w => w.SysWholesellerEdiId).OrderByDescending(w => w).FirstOrDefault() + 1;

                entities.SysWholesellerEdi.AddObject(ws);
                var result = SaveChanges(entities);
                result.IntegerValue = ws.SysWholesellerEdiId;
                return result;
            }
        }

        public ActionResult DeleteSysWholesellerEdi(int id)
        {
            using (var entities = new SOESysEntities())
            {
                var ws = this.GetSysWholesellerEDI(id, entities: entities);
                entities.DeleteObject(ws);
                SaveChanges(entities);
            }

            return new ActionResult();
        }

        public List<SysWholesellerEdi> GetSysWholesellerEDIs(SysWholesellerEdiManagerType managerType, bool loadSysEdiMsg = false)
        {
            var query = this.GetSysWholesellerEDIQuery(loadSysEdiMsg);
            return query.Where(w => w.EdiManagerType == (int)managerType).ToList();
        }

        public SysWholesellerEdi GetSysWholesellerEDI(int sysWholesellerEdiId, bool loadSysEdiMsg = false, SOESysEntities entities = null)
        {
            if (entities == null)
                entities = SysEntities;

            var query = GetSysWholesellerEDIQuery(loadSysEdiMsg, entities);
            return query.Where(sw => sw.SysWholesellerEdiId == sysWholesellerEdiId).FirstOrDefault();
        }

        internal SysWholesellerEdi GetSysWholesellerEDI(string senderNr, string senderName)
        {
            ObjectQuery<SysWholesellerEdi> query = SysEntities.SysWholesellerEdi.Include("SysEdiMsg");

            var query2 = (from ws in query
                     where ws.SysEdiMsg.Any(m => m.SenderSenderNr == senderNr)
                     select ws).Distinct();

            int count = query2.Count();
            if (count == 1)
            {
                return query2.FirstOrDefault();
            }
            else if(count > 1 && !string.IsNullOrEmpty(senderName))
            {
                query2 = (from entry in query2
                          where senderName.ToLower().Contains(entry.SenderName.ToLower())
                          select entry);
            }
            else if (count == 0 && !string.IsNullOrEmpty(senderName))
            {
                query2 = (from entry in query
                          where senderName.ToLower().Contains(entry.SenderName.ToLower())
                          select entry);
            }

            count = query2.Count();
            if (count == 1)
                return query2.FirstOrDefault();
            else
                return null;
        }

        private IQueryable<SysWholesellerEdi> GetSysWholesellerEDIQuery(bool loadSysEdiMsg, SOESysEntities entities = null)
        {
            if (entities == null)
                entities = SysEntities;

            ObjectQuery<SysWholesellerEdi> query = entities.SysWholesellerEdi;
            if(loadSysEdiMsg)
                query = query.Include("SysEdiMsg");

            return query;
        }

    }
}
