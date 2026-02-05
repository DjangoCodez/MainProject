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
    public abstract class ManagerBase
    {
        private SOESysEntities sysEntities;
        private SOECompEntities compEntites;

        #region Public ObjectContexts

        public SOESysEntities SysEntities
        {
            get
            {
                if (sysEntities == null)
                    sysEntities = Sys.CreateSOESysEntities(true);

                return sysEntities;
            }
        }

        public SOECompEntities CompEntities
        {
            get
            {
                if (compEntites == null)
                    compEntites = Comp.CreateSOECompEntities(true);

                return compEntites;
            }
        }

        #endregion

        #region Manager lazy loader properties

        private EdiCompManager ediCompManager;
        protected EdiCompManager EdiCompManager
        {
            get
            {
                return ediCompManager ?? (ediCompManager = new EdiCompManager());
            }
        }

        private EdiSysManager ediSysManager;
        protected EdiSysManager EdiSysManager
        {
            get
            {
                return ediSysManager ?? (ediSysManager = new EdiSysManager());
            }
        }

        #endregion

        #region Save

        protected ActionResult SaveChanges(ObjectContext context)
        {
            // Default result is successful
            ActionResult result = new ActionResult();

            try
            {
                result.ObjectsAffected = context.SaveChanges(SaveOptions.AcceptAllChangesAfterSave);
                if (result.ObjectsAffected == 0)
                {
                    // Set result
                    result.Success = true;
                    result.ErrorNumber = (int)ActionResultSave.NothingSaved;
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.GetInnerExceptionMessages().JoinToString(", "));
                result.Exception = ex;
            }

            return result;
        }

        #endregion
    }
}
