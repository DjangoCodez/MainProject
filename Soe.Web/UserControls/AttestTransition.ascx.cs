using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace SoftOne.Soe.Web.UserControls
{
    public partial class AttestTransition : ControlBase
    {
        #region Variabled

        bool initialized = false;
        private AttestManager am;
        private EmployeeManager em;
        public Dictionary<int, int> selected;

        #endregion

        public void InitControl(Controls.Form Form1)
        {
            this.SoeForm = Form1;

            am = new AttestManager(PageBase.ParameterObject);

            initialized = true;
        }

        public void PopulateAttestRoleTransitions(bool repopulate, int actorCompanyId, int attestRoleId, SoeModule module)
        {
            if (!initialized)
                return;

            if (!Populate(repopulate, module))
                return;

            if (actorCompanyId != 0 && attestRoleId != 0)
            {
                AttestRole attestRole = am.GetAttestRole(attestRoleId, actorCompanyId, false, true);
                if (attestRole != null)
                {
                    int pos = 0;
                    foreach (var transition in attestRole.AttestTransition)
                    {
                        AttestTransitions.AddLabelValue(pos, transition.AttestStateFrom.Entity.ToString());
                        AttestTransitions.AddValueFrom(pos, transition.AttestTransitionId.ToString());
                        selected.Add(pos, transition.AttestTransitionId);

                        pos++;
                        if (pos == AttestTransitions.NoOfIntervals)
                            break;
                    }
                }
            }
        }

        public void PopulateEmployeeGroupAttestTransitions(bool repopulate, int actorCompanyId, int employeeGroupId, SoeModule module)
        {
            if (!initialized)
                return;

            if (!Populate(repopulate, module))
                return;

            if (actorCompanyId != 0 && employeeGroupId != 0)
            {
                if (em == null)
                    em = new EmployeeManager(PageBase.ParameterObject);

                EmployeeGroup employeeGroup = em.GetEmployeeGroup(employeeGroupId, true, true, true, false);
                if (employeeGroup != null)
                {
                    int pos = 0;
                    foreach (var transition in employeeGroup.AttestTransition)
                    {
                        AttestTransitions.AddLabelValue(pos, transition.AttestStateFrom.Entity.ToString());
                        AttestTransitions.AddValueFrom(pos, transition.AttestTransitionId.ToString());
                        selected.Add(pos, transition.AttestTransitionId);

                        pos++;
                        if (pos == AttestTransitions.NoOfIntervals)
                            break;
                    }
                }
            }
        }

        private bool Populate(bool repopulate, SoeModule module)
        {
            //Bug: 
            //PageBase.Scripts is null in method Populate() 
            //in a UserControl that lives in a Page that is navigated to from Server.Transfer
            //The script should be added from the calling page in this scenario
            if (PageBase.Scripts != null)
            {
                PageBase.Scripts.Add("/UserControls/AttestTransition.js");
            }

            AttestTransitions.Labels = am.GetAttestEntities(true, true, module).ToDictionary();

            selected = new Dictionary<int, int>();

            if (repopulate && SoeForm.PreviousForm != null)
            {
                AttestTransitions.PreviousForm = SoeForm.PreviousForm;
                return false;
            }

            return true;
        }

        public bool SaveAttestRoleTransitions(NameValueCollection F, int actorCompanyId, int attestRoleId)
        {
            if (am == null)
                am = new AttestManager(PageBase.ParameterObject);
            return am.SaveAttestRoleTransitions(AttestTransitions.GetData(F), actorCompanyId, attestRoleId).Success;
        }

        public bool SaveEmployeeGroupAttestTransitions(NameValueCollection F, int employeeGroupId)
        {
            if (am == null)
                am = new AttestManager(PageBase.ParameterObject);
            return am.SaveEmployeeGroupAttestTransitions(AttestTransitions.GetData(F), employeeGroupId).Success;
        }
    }
}