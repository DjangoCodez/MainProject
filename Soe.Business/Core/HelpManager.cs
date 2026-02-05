using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Core
{
    public class HelpManager : ManagerBase
    {
        #region Variables

        // Create a logger for use in this class
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #endregion

        #region Ctor

        public HelpManager(ParameterObject parameterObject) : base(parameterObject) { }

        #endregion

        #region Public methods

        // TODO: Quick fix for help in angular forms
        public HelpText GetFormHelp(string formName)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.SLForm.NoTracking();
            SLForm form = (from f in entities.SLForm.Include("Help")
                           where f.Name.ToLower() == formName.ToLower()
                           select f).FirstOrDefault();

            if (form != null)
            {
                int langId = GetLangId();
                entities.HelpText.NoTracking();
                return (from ht in entities.HelpText
                        where ht.Help.HelpId == form.HelpId &&
                        ht.SysLanguageId == langId &&
                        ht.Role == null &&
                        ht.Company == null &&
                        ht.State == (int)SoeEntityState.Active
                        select ht).FirstOrDefault();
            }

            return null;
        }

        #endregion

        #region SysHelp

        public IList<SysHelpTitleDto> GetSysHelpTitles(string language)
        {
            int langId = LanguageManager.GetSysLanguageId(language);
            using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
            return (from s in sysEntitiesReadOnly.SysHelp
                    where s.SysLanguageId == langId &&
                    s.State == (int)SoeEntityState.Active
                    select s)
                                .Select(s => new SysHelpTitleDto
                                {
                                    Title = s.Title,
                                    Feature = (Feature)s.SysFeatureId
                                }).ToList();
        }

        public bool HasSysHelp(Feature feature, string language)
        {
            int langId = LanguageManager.GetSysLanguageId(language);

            using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
            return (from s in sysEntitiesReadOnly.SysHelp
                    where s.SysFeatureId == (int)feature &&
                    s.SysLanguageId == langId &&
                    s.State == (int)SoeEntityState.Active
                    select s).Any();
        }

        public SysHelp GetSysHelp(Feature feature, string language)
        {
            int langId = LanguageManager.GetSysLanguageId(language);

            using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
            return (from s in sysEntitiesReadOnly.SysHelp
                    where s.SysFeatureId == (int)feature &&
                    s.SysLanguageId == langId &&
                    s.State == (int)SoeEntityState.Active
                    select s).FirstOrDefault();
        }

        public SysHelp GetSysHelp(SOESysEntities entities, int sysHelpId)
        {
            return (from s in entities.SysHelp
                    where s.SysHelpId == sysHelpId
                    select s).FirstOrDefault();
        }

        public ActionResult SaveSysHelp(SysHelpDTO helpInput)
        {
            ActionResult result = new ActionResult();

            #region Prereq

            if (helpInput == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "SysHelpDTO");

            if (helpInput.SysLanguageId == 0)
            {
                if (String.IsNullOrEmpty(helpInput.Language))
                    return new ActionResult((int)ActionResultSave.IncorrectInput, "Language");

                helpInput.SysLanguageId = LanguageManager.GetSysLanguageId(helpInput.Language);
                if (helpInput.SysLanguageId == 0)
                    return new ActionResult((int)ActionResultSave.IncorrectInput, "Language");
            }

            #endregion

            using (SOESysEntities entities = new SOESysEntities())
            {
                // Get original help
                SysHelp sysHelp = GetSysHelp(entities, helpInput.SysHelpId);
                if (sysHelp == null)
                {
                    sysHelp = new SysHelp();
                    SetCreatedPropertiesOnEntity(sysHelp);
                    entities.SysHelp.Add(sysHelp);
                }
                else
                {
                    SetModifiedPropertiesOnEntity(sysHelp);
                }

                // Set values
                sysHelp.SysLanguageId = helpInput.SysLanguageId;
                sysHelp.SysFeatureId = helpInput.SysFeatureId;
                sysHelp.VersionNr = helpInput.VersionNr;
                sysHelp.Title = helpInput.Title;
                sysHelp.Text = helpInput.Text;
                sysHelp.PlainText = helpInput.PlainText;

                // Save
                result = SaveChanges(entities);
                if (!result.Success)
                    return result;

                result.IntegerValue = sysHelp.SysHelpId;
            }

            return result;
        }

        #endregion
    }
}
