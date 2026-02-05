using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using System;
using System.Collections.Specialized;

namespace SoftOne.Soe.Web.UserControls
{
    public partial class DistributionSelectionUser : ControlBase
    {
        #region Variables

        public NameValueCollection F { get; set; }
        private int userId;

        #endregion

        public void Populate(bool repopulate)
        {
            #region Init

            UserManager um = new UserManager(PageBase.ParameterObject);

            #endregion

            #region Populate

            User.ConnectDataSource(um.GetUsersByCompanyDict(PageBase.SoeCompany.ActorCompanyId, PageBase.RoleId, PageBase.UserId, true, true, false, false));

            #endregion

            #region Set data

            if (repopulate && SoeForm != null && SoeForm.PreviousForm != null)
            {
                User.Value = SoeForm.PreviousForm["User"];
            }

            #endregion
        }

        public bool Evaluate(SelectionUser s, EvaluatedSelection es)
        {
            if (F == null || s == null || es == null)
                return false;

            #region 1. Validate input and 2. Read interval into SelectionStd

            if (Int32.TryParse(F["User"], out userId))
            {
                s.UserId = userId;
            }

            #endregion

            #region 2. Evaluate interval

            if (s.UserId.HasValue && s.UserId.Value > 0)
            {
                es.SU_UserId = s.UserId.Value;
                es.SU_HasUser = true;
            }

            #endregion

            //Set as evaluated
            es.SU_IsEvaluated = true;

            return true;
        }
    }
}