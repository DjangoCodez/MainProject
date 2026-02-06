using SoftOne.Soe.Business.Core.RptGen;
using SoftOne.Soe.Business.DTO;
using SoftOne.Soe.Business.Evo.Connectors.Cache;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Business.Util.LogCollector;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity.Validation;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace SoftOne.Soe.Business.Core
{
    public class StatusManager : ManagerBase
    {
        #region Variables

        // Create a logger for use in this class
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private bool success;
        private string errorMessage;

        #endregion

        #region Ctor

        public StatusManager() : base(null)
        {
            this.success = true;
            this.errorMessage = string.Empty;
        }

        #endregion

        #region General

        public SoftOneStatusDTO GetSoftOneStatusDTO(ServiceType serviceType)
        {
            try
            {
                SoftOneStatusDTO statusDTO = new SoftOneStatusDTO();
                DateTime start = DateTime.Now;

                statusDTO.UTC = DateTime.UtcNow;
                statusDTO.RequestStart = statusDTO.UTC;
                statusDTO.DBConnected = DBConnected(serviceType);
                statusDTO.ServiceType = serviceType;
                statusDTO.MachineName = Environment.MachineName;
                statusDTO.MilliSeconds = CalendarUtility.TimeSpanToMilliSeconds(DateTime.Now, start);
                if (statusDTO.ErrorMessage == null)
                    statusDTO.ErrorMessage = string.Empty;
                statusDTO.ErrorMessage += CheckEvoCache(serviceType);

                if (success)
                    statusDTO.Alive = true;

                statusDTO.RequestEnd = DateTime.UtcNow;

                return statusDTO;
            }
            catch (Exception ex)
            {
                LogError(ex, this.log);
                var dto = new SoftOneStatusDTO(ex);
                dto.ServiceType = serviceType;
                dto.MachineName = Environment.MachineName;
                dto.MilliSeconds = 0;
                dto.ErrorMessage = ex.ToString();
                return dto;
            }
        }

        private static Random random = new Random();
        private string CheckEvoCache(ServiceType serviceType)
        {
            // skip 90% of the checks to reduce load
            if (random.Next(1, 11) <= 9)
                return string.Empty;

            var key = $"EvoCache#{Environment.MachineName}#{ConfigurationSetupUtil.GetCurrentSysCompDbId()}#{serviceType}";
            var sentValue = Guid.NewGuid().ToString();

            try
            {
                EvoDistributionCacheConnector.UpsertCachedValue(key, sentValue, TimeSpan.FromSeconds(45), true);
                var returnedValue = EvoDistributionCacheConnector.GetCachedValue<Guid>(key);

                return $"Key: {key} SentValue: {sentValue} returnedValue: {returnedValue}";
            }
            catch (Exception ex)
            {
                LogError(ex, this.log);
                return $"CheckEvoCache Failed {key} {sentValue}";
            }
        }

        public SoftOneStatusDTO GetPrintSoftOneStatusDTO(ServiceType serviceType)
        {
            try
            {
                SoftOneStatusDTO statusDTO = new SoftOneStatusDTO();
                DateTime start = DateTime.Now;
                string errorMessage = string.Empty;

                statusDTO.UTC = DateTime.UtcNow;
                statusDTO.DBConnected = PrintReport(out errorMessage);
                statusDTO.ErrorMessage = errorMessage;
                statusDTO.ServiceType = serviceType;
                statusDTO.MachineName = Environment.MachineName;
                statusDTO.MilliSeconds = CalendarUtility.TimeSpanToMilliSeconds(DateTime.Now, start);

                if (success)
                    statusDTO.Alive = true;

                if (!string.IsNullOrEmpty(this.errorMessage))
                    statusDTO.ErrorMessage = statusDTO.ErrorMessage + this.errorMessage;

                return statusDTO;
            }
            catch (Exception ex)
            {
                LogError(ex, this.log);
                var dto = new SoftOneStatusDTO(ex);
                dto.ServiceType = serviceType;
                dto.MachineName = Environment.MachineName;
                dto.MilliSeconds = 0;
                dto.ErrorMessage = ex.ToString();
                return dto;
            }
        }

        #endregion

        #region Database

        #region Common

        private bool DBConnected(ServiceType serviceType)
        {
            bool connected = false;
            connected = CanReadFromDatabase(serviceType);
            if (connected)
                connected = CanWriteToDatabase(serviceType);

            if (!connected)
            {
                Thread.Sleep(500);
                connected = CanReadFromDatabase(serviceType);

                if (connected)
                    connected = CanWriteToDatabase(serviceType);
                else
                {
                    Thread.Sleep(50);
                    connected = CanWriteToDatabase(serviceType);
                }
            }

            return connected;
        }

        #endregion

        #region Read from database

        private bool CanReadFromDatabase(ServiceType serviceType)
        {
            bool canRead = false;

            try
            {
                canRead = CanReadFromDatabaseCompEntities();

                if (canRead)
                    CanReadFromDatabaseComp();

                if (canRead)
                    CanReadFromDatabaseSysEntities();

                if (canRead)
                    CanReadFromDatabaseSys();

            }
            catch (Exception ex)
            {
                LogError(ex, this.log);
                this.success = false;
                this.errorMessage = ex.ToString();
                return false;
            }

            return canRead;
        }

        private bool CanReadFromDatabaseCompEntities()
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            return entitiesReadOnly.Tag.Take(10).Count() == 10;
        }

        private bool CanReadFromDatabaseComp()
        {
            using (CompEntities entities = new CompEntities())
            {
                return entities.Tag.Take(10).Count() == 10;
            }
        }

        private bool CanReadFromDatabaseSysEntities()
        {
            using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
            return sysEntitiesReadOnly.SysAccountStd.Take(10).Count() == 10;
        }

        private bool CanReadFromDatabaseSys()
        {
            using (SOESysEntities entities = new SOESysEntities())
            {
                return entities.SysAccountStd.Take(10).Count() == 10;
            }
        }

        #endregion

        #region Write to database

        private bool CanWriteToDatabase(ServiceType serviceType)
        {
            bool canWrite = false;

            try
            {
                canWrite = CanWriteToSys(serviceType);

                if (canWrite)
                    canWrite = CanWriteToComp(serviceType);

            }
            catch (Exception ex)
            {
                LogError(ex, this.log);
                this.success = false;
                this.errorMessage = ex.ToString();
                return false;
            }

            return canWrite;
        }

        private bool CanWriteToSys(ServiceType serviceType)
        {
            var machineName = Environment.MachineName;
            int numbers = Convert.ToInt32(new string(machineName.Where(char.IsDigit).ToArray()));
            var dbId = ConfigurationSetupUtil.GetCurrentSysCompDbId() + (int)serviceType + numbers;
            var changedName = "DBTest";
            var articleId = 0;
            bool writeSuccess = false;
            using (SOESysEntities entities = new SOESysEntities())
            {
                SysXEArticle article = entities.SysXEArticle.FirstOrDefault(f => f.SysXEArticleId == dbId);

                if (article == null)
                    article = entities.SysXEArticle.OrderBy(o => o.SysXEArticleId).FirstOrDefault();

                if (article == null)
                    return false;

                articleId = article.SysXEArticleId;
                changedName += article.ModifiedBy ?? "";
                article.ModifiedBy = changedName;
                try
                {
                    entities.SaveChanges();
                }
                catch (DbEntityValidationException ex)
                {
                    List<string> errors = new List<string>();
                    foreach (var validationError in ex.EntityValidationErrors)
                    {
                        errors.Add($"Entity: {validationError.Entry.Entity.GetType().Name}");
                        foreach (var error in validationError.ValidationErrors)
                        {
                            errors.Add($"Property: {error.PropertyName}, Error: {error.ErrorMessage}");
                        }
                    }

                    LogCollector.LogError(ex.ToString() + " errors: " + string.Join(", ", errors));
                }
            }

            using (SOESysEntities entities = new SOESysEntities())
            {
                var article = entities.SysXEArticle.FirstOrDefault(f => f.SysXEArticleId == articleId);

                if (article == null)
                    return false;

                if (article.ModifiedBy == changedName)
                    writeSuccess = true;

                article.ModifiedBy = "DBTest";

                if (article.ModifiedBy.Length > 40)
                    article.ModifiedBy = article.ModifiedBy.Substring(0, 10);

                try
                {
                    entities.SaveChanges();
                }
                catch (DbEntityValidationException ex)
                {
                    List<string> errors = new List<string>();
                    foreach (var validationError in ex.EntityValidationErrors)
                    {
                        errors.Add($"Entity: {validationError.Entry.Entity.GetType().Name}");
                        foreach (var error in validationError.ValidationErrors)
                        {
                            errors.Add($"Property: {error.PropertyName}, Error: {error.ErrorMessage}");
                        }
                    }

                    LogCollector.LogError(ex.ToString() + " errors: " + string.Join(", ", errors));
                }

                return writeSuccess;
            }
        }

        private bool CanWriteToComp(ServiceType serviceType)
        {
            string dbTest = "DBTest";
            string newName = string.Empty;
            bool writeSuccess = false;
            int numbers = Regex.Match(Environment.MachineName, @"\d+").Success ? Convert.ToInt32(Regex.Match(Environment.MachineName, @"\d+").Value) : 0;
            var dbId = ConfigurationSetupUtil.GetCurrentSysCompDbId() + (int)serviceType + numbers;
            var setName = $"SoftOneStatus dbid {ConfigurationSetupUtil.GetCurrentSysCompDbId()} {serviceType}";
            using (CompEntities entities = new CompEntities())
            {
                Tag tag = entities.Tag.FirstOrDefault(f => f.TagId == dbId);

                if (tag == null)
                {
                    List<Tag> tags = new List<Tag>();
                    int maxDbId = ConfigurationSetupUtil.GetSysCompDBDTOs().Max(m => m.SysCompDbId);
                    var currentMax = entities.Tag.Max(m => m.TagId);

                    if (maxDbId < dbId)
                        maxDbId = dbId;

                    for (int i = currentMax + 1; i <= maxDbId; i++)
                    {
                        entities.Tag.AddObject(new Tag() { TagId = i, Name = $"SoftOneStatus code {i}" });
                    }
                    entities.SaveChanges();
                }

                tag = entities.Tag.OrderBy(o => o.TagId).FirstOrDefault(f => f.TagId == dbId);
                string modifiedBy = tag.Name;
                newName = dbTest + modifiedBy;
                tag.Name = newName;
                entities.SaveChanges();
            }

            using (CompEntities entities = new CompEntities())
            {
                Tag tag = entities.Tag.FirstOrDefault(f => f.Name == newName);

                if (tag == null)
                    return false;

                if (tag.Name == newName)
                    writeSuccess = true;

                tag.Name = setName;
                entities.SaveChanges();

                return writeSuccess;
            }
        }

        #endregion

        #endregion

        #region Report

        private bool PrintReport(out string errorMessage)
        {
            errorMessage = string.Empty;

            SysReportTemplate sysReportTemplate = ReportManager.GetFirstSysReportTemplate(SoeReportTemplateType.TimeMonthlyReport);
            if (sysReportTemplate != null)
            {

                byte[] template = sysReportTemplate.Template;
                List<RptGenRequestPicturesDTO> crGenRequestPicturesDTO = new List<RptGenRequestPicturesDTO>();

                RptGenConnector crgen = RptGenConnector.GetConnector(parameterObject, (SoeReportType)sysReportTemplate.SysReportTypeId);
                CultureInfo culture = GetCulture(GetLangId());

                ReportPrintoutDTO dto = new ReportPrintoutDTO();
                dto.ExportType = TermGroup_ReportExportType.Pdf;

                DataSet dataSet = ReportGenManager.GetDefaultXmlDataSet(SoeReportTemplateType.TimeMonthlyReport);

                RptGenResultDTO crGenResult = crgen.GenerateReport(dto.ExportType, template, null, dataSet, crGenRequestPicturesDTO, culture, ReportGenManager.GetXsdFileString(SoeReportTemplateType.TimeMonthlyReport), $"dto status reportid:{dto.ReportId} actorcompanyid: {dto.ActorCompanyId}");
                if (crGenResult != null)
                {
                    dto.Data = crGenResult.GeneratedReport;
                    if (dto.Data != null && crGenResult.Success)
                        return true;

                    LogError(crGenResult.ErrorMessage);
                    errorMessage = crGenResult.ErrorMessage;
                }
            }

            return false;
        }

        #endregion
    }
}
