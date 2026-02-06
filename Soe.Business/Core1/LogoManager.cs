using System;
using System.IO;
using System.Linq;
using SoftOne.Soe.Data;
using System.Collections.Generic;
using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Common.Util;
using log4net;
using System.Reflection;
using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Business.Util.LogCollector;

namespace SoftOne.Soe.Business.Core
{
    public class LogoManager : ManagerBase
    {
        #region Variables

        // Create a logger for use in this class
        private readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        #endregion

        #region Ctor

        public LogoManager(ParameterObject parameterObject) : base(parameterObject) { }

        #endregion

        #region CompanyLogo

        public List<CompanyLogo> GetCompanyLogos(int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.CompanyLogo.NoTracking();
            return (from logo in entities.CompanyLogo.Include("Company")
                    where (logo.Company.ActorCompanyId == actorCompanyId)
                    select logo).ToList<CompanyLogo>();
        }

        public CompanyLogo GetCompanyLogo(int imageId, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.CompanyLogo.NoTracking();
            return GetCompanyLogo(entities, imageId, actorCompanyId);
        }

        public CompanyLogo GetCompanyLogo(CompEntities entities, int imageId, int actorCompanyId)
        {
            return (from logo in entities.CompanyLogo
                        .Include("Company")
                    where logo.ImageId == imageId &&
                    logo.Company.ActorCompanyId == actorCompanyId
                    select logo).FirstOrDefault();
        }

        public CompanyLogo GetDefaultCompanyLogo(int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.CompanyLogo.NoTracking();
            return GetDefaultCompanyLogo(entities, actorCompanyId);
        }

        public CompanyLogo GetDefaultCompanyLogo(CompEntities entities, int actorCompanyId)
        {
            return (from logo in entities.CompanyLogo
                    where logo.Company.ActorCompanyId == actorCompanyId
                    select logo).FirstOrDefault();
        }

        public ActionResult SaveCompanyLogo(byte[] image, string extension, int? imageId, int actorCompanyId)
        {
            if (DefenderUtil.IsVirus(image))
                return new ActionResult("File could contain virus");

            using (CompEntities entities = new CompEntities())
            {
                CompanyLogo originalCompanyLogo = null;
                if (imageId != null && imageId > 0)
                    originalCompanyLogo = GetCompanyLogo(entities, Convert.ToInt32(imageId), actorCompanyId);

                CompanyLogo companyLogo = new CompanyLogo()
                {
                    Extension = extension,
                    Logo = image,
                };

                companyLogo.Company = CompanyManager.GetCompany(entities, actorCompanyId);
                if (companyLogo.Company == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte"));

                if (originalCompanyLogo == null)
                    return AddEntityItem(entities, companyLogo, "CompanyLogo");
                else
                    return UpdateEntityItem(entities, originalCompanyLogo, companyLogo, "CompanyLogo");
            }
        }

        public ActionResult DeleteCompanyLogo(int companyLogoId, int actorCompanyId)
        {
            using (CompEntities entities = new CompEntities())
            {
                CompanyLogo orginalCompanyLogo = GetCompanyLogo(entities, companyLogoId, actorCompanyId);
                if (orginalCompanyLogo == null)
                    return new ActionResult((int)ActionResultDelete.EntityNotFound, "CompanyLogo");

                return DeleteEntityItem(entities, orginalCompanyLogo);
            }
        }


        public string GetCompanyLogoFilePath(CompEntities entities, int actorCompanyId, bool refreshIfExists)
        {
            string key = $"GetCompanyLogoFilePath{actorCompanyId}#{refreshIfExists}";

            var fromCache = BusinessMemoryCache<string>.Get(key);

            if (!string.IsNullOrEmpty(fromCache))
                return fromCache;

            string pathPhysical = "";
            try
            {
                int imageId = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.CoreCompanyLogo, 0, actorCompanyId, 0);
                if (imageId > 0)
                {
                    CompanyLogo companyLogo = GetCompanyLogo(entities, imageId, actorCompanyId);
                    if (companyLogo != null)
                    {
                        string dirPhysical = StringUtility.GetValidFilePath(ConfigSettings.SOE_SERVER_DIR_TEMP_LOGO_PHYSICAL);
                        pathPhysical = dirPhysical + companyLogo.FileNameWithExtension;
                        bool createFile = true;
                        if (File.Exists(pathPhysical))
                        {
                            if (refreshIfExists)
                                File.Delete(pathPhysical);
                            else
                                createFile = false;
                        }

                        if (createFile)
                        {
                            if (DefenderUtil.IsVirus(pathPhysical))
                            {
                                LogCollector.LogError($"GetCompanyLogoFilePath Virus detected {pathPhysical} {Environment.StackTrace}");
                                if (File.Exists(pathPhysical))
                                    File.Delete(pathPhysical);
                                return "";
                            }

                            var file = new FileStream(pathPhysical, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.Read);
                            file.Write(companyLogo.Logo, 0, companyLogo.Logo.Length);
                            file.Dispose();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                base.LogError(ex, this.log);
                pathPhysical = "";
            }
            if (!string.IsNullOrEmpty(pathPhysical))
                BusinessMemoryCache<string>.Set(key, pathPhysical, 10);
            return pathPhysical;
        }

        #endregion
    }
}

