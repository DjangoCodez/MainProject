using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace SoftOne.Soe.Web.UserControls
{
    public partial class TimeCodeTimeDeviationCause : ControlBase
    {
        #region Variabled

        bool initialized = false;
        private EmployeeManager em;
        private TimeCodeManager tcm;
        private TimeDeviationCauseManager tdcm;

        public Dictionary<int, int> selectedFrom;
        public Dictionary<int, int> selectedTo;

        #endregion

        public void InitControl(Controls.Form Form1)
        {
            this.SoeForm = Form1;

            em = new EmployeeManager(PageBase.ParameterObject);
            tcm = new TimeCodeManager(PageBase.ParameterObject);
            tdcm = new TimeDeviationCauseManager(PageBase.ParameterObject);

            initialized = true;
        }

        #region Generic

        private bool Populate(bool repopulate, int actorCompanyId)
        {
            Mappings.DataSourceFrom = tdcm.GetTimeDeviationCausesDict(actorCompanyId, true);
            Mappings.DataSourceTo = tcm.GetTimeCodesDict(actorCompanyId, true, false, false);

            selectedFrom = new Dictionary<int, int>();
            selectedTo = new Dictionary<int, int>();

            if (repopulate && SoeForm.PreviousForm != null)
            {
                Mappings.PreviousForm = SoeForm.PreviousForm;
                return false;
            }

            return true;
        }

        #endregion

        #region EmployeeGroup

        public void PopulateEmployeeGroupMapping(bool repopulate, int actorCompanyId, int employeeGroupId)
        {
            if (!initialized)
                return;

            if (!Populate(repopulate, actorCompanyId))
                return;

            if (actorCompanyId != 0 && employeeGroupId != 0)
            {
                // Get attest role
                EmployeeGroup employeeGroup = em.GetEmployeeGroup(employeeGroupId, false, true, true, false);
                if (employeeGroup != null)
                {
                    if (!employeeGroup.EmployeeGroupTimeDeviationCauseTimeCode.IsLoaded)
                        employeeGroup.EmployeeGroupTimeDeviationCauseTimeCode.Load();
                    if (employeeGroup.EmployeeGroupTimeDeviationCauseTimeCode != null)
                    {
                        int pos = 0;
                        foreach (var mapping in employeeGroup.EmployeeGroupTimeDeviationCauseTimeCode)
                        {
                            if (!mapping.TimeCodeReference.IsLoaded)
                                mapping.TimeCodeReference.Load();
                            if (mapping.TimeCode == null) 
                                continue;

                            if (!mapping.TimeDeviationCauseReference.IsLoaded)
                                mapping.TimeDeviationCauseReference.Load();
                            if (mapping.TimeDeviationCause == null)
                                continue;

                            Mappings.AddValueTo(pos, mapping.TimeCode.TimeCodeId.ToString());
                            Mappings.AddValueFrom(pos, mapping.TimeDeviationCauseId.ToString());

                            selectedTo.Add(pos, mapping.TimeCodeId);
                            selectedFrom.Add(pos, mapping.TimeDeviationCauseId);

                            pos++;
                            if (pos == Mappings.NoOfIntervals)
                                break;
                        }
                    }
                }
            }
        }

        public bool SaveEmployeeGroupMappings(NameValueCollection F, int employeeGroupId, int actorCompanyId, int userId)
        {
            if (em == null)
                em = new EmployeeManager(PageBase.ParameterObject);
            return em.SaveEmployeeGroupTimeCodeTimeDeviationCauseMappings(Mappings.GetData(F), employeeGroupId, actorCompanyId).Success;
        }

        #endregion

        #region TimeCodeBreak

        public void PopulateTimeCodeBreakMapping(bool repopulate, int actorCompanyId, int timeCodeId)
        {
            if (!initialized)
                return;

            if (!Populate(repopulate, actorCompanyId))
                return;

            if (actorCompanyId != 0 && timeCodeId != 0)
            {
                // Get attest role
                TimeCodeBreak timeCodeBreak = tcm.GetTimeCodeBreak(timeCodeId, actorCompanyId, false, false);
                if (timeCodeBreak != null)
                {
                    if (!timeCodeBreak.TimeCodeBreakTimeCodeDeviationCauses.IsLoaded)
                        timeCodeBreak.TimeCodeBreakTimeCodeDeviationCauses.Load();
                    if (timeCodeBreak.TimeCodeBreakTimeCodeDeviationCauses != null)
                    {
                        int pos = 0;
                        foreach (var mapping in timeCodeBreak.TimeCodeBreakTimeCodeDeviationCauses)
                        {
                            if (!mapping.TimeCodeReference.IsLoaded)
                                mapping.TimeCodeReference.Load();
                            if (mapping.TimeCode == null)
                                continue;

                            if (!mapping.TimeDeviationCauseReference.IsLoaded)
                                mapping.TimeDeviationCauseReference.Load();
                            if (mapping.TimeDeviationCause == null)
                                continue;

                            Mappings.AddValueTo(pos, mapping.TimeCode.TimeCodeId.ToString());
                            Mappings.AddValueFrom(pos, mapping.TimeDeviationCause.TimeDeviationCauseId.ToString());

                            selectedTo.Add(pos, mapping.TimeCode.TimeCodeId);
                            selectedFrom.Add(pos, mapping.TimeDeviationCause.TimeDeviationCauseId);

                            pos++;
                            if (pos == Mappings.NoOfIntervals)
                                break;
                        }
                    }
                }
            }
        }

        #endregion
    }
}