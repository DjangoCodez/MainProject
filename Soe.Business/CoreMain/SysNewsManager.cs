using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Core
{
    public class SysNewsManager : ManagerBase
    {
        #region Ctor

        public SysNewsManager(ParameterObject parameterObject) : base(parameterObject) { }

        #endregion

        #region SysNews

        /// <returns></returns>
        /// <summary>
        /// Get all SysNews's
        /// Accessor for SysDbCache
        /// </summary>
        /// <returns></returns>
        public List<SysNews> GetSysNewsAll()
        {
            using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
            return (from sn in sysEntitiesReadOnly.SysNews
                    where sn.State == (int)SoeEntityState.Active
                    orderby sn.PubDate descending
                    select sn).ToList();
        }

        public IEnumerable<SysNews> GetSysNewsAll(int noOfNews)
        {
            int langId = GetLangId();

            //Uses SysDbCache
            return (from sn in SysDbCache.Instance.SysNews
                    where sn.SysLanguageId == langId
                    orderby sn.PubDate descending
                    select sn).Take<SysNews>(noOfNews);
        }

        public SysNews GetSysNews(int sysNewsId)
        {
            using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
            return GetSysNews(sysEntitiesReadOnly, sysNewsId);
        }

        public SysNews GetSysNews(SOESysEntities entities, int sysNewsId)
        {
            return (from sn in entities.SysNews
                    where sn.SysNewsId == sysNewsId
                    select sn).FirstOrDefault<SysNews>();
        }

        public ActionResult AddSysNews(SysNews sysNews)
        {
            if (sysNews == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "SysNews");

            SysDbCache.Instance.FlushSysNews();
            using (SOESysEntities entities = new SOESysEntities())
            {
                entities.SysNews.Add(sysNews);
                return SaveChanges(entities);
            }
        }

        public ActionResult UpdateSysNews(SysNews sysNews)
        {
            if (sysNews == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "SysNews");

            SysDbCache.Instance.FlushSysNews();
            using (SOESysEntities entities = new SOESysEntities())
            {
                var originalSysNews = GetSysNews(entities, sysNews.SysNewsId);
                if (originalSysNews == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "SysNews");

                originalSysNews.SysXEArticleId = sysNews.SysXEArticleId;
                originalSysNews.SysLanguageId = sysNews.SysLanguageId;
                originalSysNews.AttachmentExportType = sysNews.AttachmentExportType;
                originalSysNews.Attachment = sysNews.Attachment;
                originalSysNews.AttachmentFileName = sysNews.AttachmentFileName;
                originalSysNews.AttachmentImageSrc = sysNews.AttachmentImageSrc;
                originalSysNews.Author = sysNews.Author;
                originalSysNews.Description = sysNews.Description;
                originalSysNews.DisplayType = sysNews.DisplayType;
                originalSysNews.Title = sysNews.Title;
                originalSysNews.IsPublic = sysNews.IsPublic;
                originalSysNews.Preview = sysNews.Preview;
                originalSysNews.PubDate = sysNews.PubDate;
                originalSysNews.State = sysNews.State;
                SetModifiedPropertiesOnEntity(originalSysNews);

                return SaveChanges(entities);

            }
        }

        public ActionResult DeleteSysNews(int sysNewsId, UserDTO userDTO)
        {
            SysDbCache.Instance.FlushSysNews();
            using (SOESysEntities entities = new SOESysEntities())
            {
                SysNews originalSysNews = GetSysNews(entities, sysNewsId);
                if (originalSysNews == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "SysNews");

                return ChangeEntityStateOnEntity(entities, originalSysNews, SoeEntityState.Deleted, true, user: userDTO);
            }
        }

        #endregion
    }
}
