using SoftOne.Soe.Common.Attributes;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using TypeLite;

namespace SoftOne.Soe.Common.DTO
{
    #region Tables

    #region Dashboard

    #region SysGauge

    public class SysGaugeDTO
    {
        public int SysGaugeId { get; set; }
        public int SysFeatureId { get; set; }
        public int SysTermId { get; set; }
        public string GaugeName { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }

        // Extensions
        public string Name { get; set; }
        public bool IsSelected { get; set; }

        //For compare information
        public PostChange PostChange { get; set; }
    }

    #endregion

    #endregion

    #region Import/Export

    #region Export

    #region SysExportDefinition

    public class SysExportDefinitionDTO
    {
        public SysExportDefinitionDTO()
        {
            SysExportDefinitionLevels = new List<SysExportDefinitionLevelDTO>();
        }

        public int SysExportDefinitionId { get; set; }
        public int SysExportHeadId { get; set; }
        public TermGroup_SysExportDefinitionType Type { get; set; }
        public string Name { get; set; }

        public string Separator { get; set; }
        public string XmlTagHead { get; set; }
        public string SpecialFunctionality { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }

        // Extensions
        public List<SysExportDefinitionLevelDTO> SysExportDefinitionLevels { get; set; }
    }

    #endregion

    #region SysExportDefinitionLevel

    public class SysExportDefinitionLevelDTO
    {
        public int SysExportDefinitionLevelId { get; set; }
        public int SysExportDefinitionId { get; set; }
        public int Level { get; set; }
        public string Xml { get; set; }
    }

    #endregion

    #region SysExportHead

    public class SysExportHeadDTO
    {
        public SysExportHeadDTO()
        {
            SysExportRelations = new List<SysExportRelationDTO>();
            SysExportSelects = new List<SysExportSelectDTO>();
        }
        public int SysExportHeadId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Sortorder { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }

        // Extensions
        public List<SysExportRelationDTO> SysExportRelations { get; set; }
        public List<SysExportSelectDTO> SysExportSelects { get; set; }
    }

    #endregion

    #region SysExportRelation

    public class SysExportRelationDTO
    {
        public int SysExportRelationId { get; set; }
        public int SysExportHeadId { get; set; }

        public int LevelParent { get; set; }
        public int LevelChild { get; set; }
        public string FieldParent { get; set; }
        public string FieldChild { get; set; }
    }

    #endregion

    #region SysExportSelect

    public class SysExportSelectDTO
    {
        public int SysExportSelectId { get; set; }
        public int SysExportHeadId { get; set; }

        public int Level { get; set; }
        public string Name { get; set; }
        public string Select { get; set; }
        public string Where { get; set; }
        public string GroupBy { get; set; }
        public string OrderBy { get; set; }
        public string Settings { get; set; }
    }

    #endregion

    #endregion

    #region Import

    #region SysImportDefinition

    [TSInclude]
    public class SysImportDefinitionDTO
    {
        public int SysImportDefinitionId { get; set; }
        public int? SysImportHeadId { get; set; }
        public TermGroup_SysImportDefinitionType Type { get; set; }
        public string Name { get; set; }
        public SoeModule Module { get; set; }

        public string Separator { get; set; }
        public string XmlTagHead { get; set; }
        public string SpecialFunctionality { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }
        public Guid Guid { get; set; }

        // Extensions
        public List<SysImportDefinitionLevelDTO> SysImportDefinitionLevels { get; set; }
    }

    #endregion

    #region SysImportDefinitionLevel

    [TSInclude]
    public class SysImportDefinitionLevelDTO
    {
        public int SysImportDefinitionLevelId { get; set; }
        public int SysImportDefinitionId { get; set; }
        public int Level { get; set; }
        public string Xml { get; set; }
        public List<SysImportDefinitionLevelColumnSettings> Columns { get; set; }
    }

    #endregion

    #region SysImportHead

    [TSInclude]
    public class SysImportHeadDTO
    {
        public int SysImportHeadId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Sortorder { get; set; }
        public SoeModule Module { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }
        public int? SysImportHeadTypeId { get; set; }

        // Extensions
        public List<SysImportRelationDTO> SysImportRelations { get; set; }
        public List<SysImportSelectDTO> SysImportSelects { get; set; }
    }

    #endregion

    #region SysImportRelation

    [TSInclude]
    public class SysImportRelationDTO
    {
        public int SysImportRelationId { get; set; }
        public int SysImportHeadId { get; set; }

        public string TableParent { get; set; }
        public string TableChild { get; set; }
    }

    #endregion

    #region SysImportSelect

    [TSInclude]
    public class SysImportSelectDTO
    {
        public int SysImportSelectId { get; set; }
        public int SysImportHeadId { get; set; }

        public int Level { get; set; }
        public string Name { get; set; }
        public string Select { get; set; }
        public string Where { get; set; }
        public string GroupBy { get; set; }
        public string OrderBy { get; set; }
        public string Settings { get; set; }

        public List<SysImportSelectColumnSettings> settingObjects { get; set; }
    }

    #endregion

    #endregion

    #endregion

    #region Scheduled jobs

    #region SysJob

    public class SysJobDTO
    {
        public int SysJobId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string AssemblyName { get; set; }
        public string ClassName { get; set; }
        public bool AllowParallelExecution { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }

        // Extensions
        public List<SysJobSettingDTO> SysJobSettings { get; set; }
    }

    #endregion

    #region SysJobSetting

    public class SysJobSettingDTO
    {
        public int SysJobSettingId { get; set; }
        public SysJobSettingType Type { get; set; }

        public SettingDataType DataType { get; set; }
        public string Name { get; set; }
        public string StrData { get; set; }
        public int? IntData { get; set; }
        public decimal? DecimalData { get; set; }
        public bool? BoolData { get; set; }
        public DateTime? DateData { get; set; }
        public DateTime? TimeData { get; set; }
    }

    #endregion

    #region SysScheduledJob

    public class SysScheduledJobDTO
    {
        public int SysScheduledJobId { get; set; }
        public int SysJobId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string DatabaseName { get; set; }

        public DateTime ExecuteTime { get; set; }
        public int ExecuteUserId { get; set; }
        public bool AllowParallelExecution { get; set; }

        public ScheduledJobRecurrenceType RecurrenceType { get; set; }
        public int RecurrenceCount { get; set; }
        public DateTime? RecurrenceDate { get; set; }
        public string RecurrenceInterval { get; set; }

        public ScheduledJobRetryType RetryTypeForInternalError { get; set; }
        public ScheduledJobRetryType RetryTypeForExternalError { get; set; }
        public int RetryCount { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public ScheduledJobState State { get; set; }
        public ScheduledJobType Type { get; set; }

        // Extensions
        public string StateName { get; set; }
        public List<SysJobSettingDTO> SysJobSettings { get; set; }
        public SysJobDTO SysJob { get; set; }

        public string JobStatusMessage { get; set; }
    }

    #endregion

    #region SysScheduledJobLog

    public class SysScheduledJobLogDTO
    {
        public int SysScheduledJobLogId { get; set; }
        public int SysScheduledJobId { get; set; }
        public int BatchNr { get; set; }
        public int LogLevel { get; set; }
        public DateTime Time { get; set; }
        public string Message { get; set; }

        // Extensions
        public string SysScheduledJobName { get; set; }
        public string LogLevelName { get; set; }
    }

    #endregion

    #region Durations

    public class SysScheduledJobDurationDTO
    {
        public int SysScheduledJobId { get; set; }
        public string Name { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime StopTime { get; set; }
        public bool Aborted { get; set; }
    }

    #endregion

    #endregion

    #region SysAccount

    [TSInclude]
    public class SysAccountStdDTO
    {
        public int SysAccountStdId { get; set; }
        public int SysAccountStdTypeId { get; set; }
        public int? SysVatAccountId { get; set; }
        public int AccountTypeSysTermId { get; set; }

        public string AccountNr { get; set; }
        public string Name { get; set; }

        public int AmountStop { get; set; }
        public bool UnitStop { get; set; }
        public string Unit { get; set; }

        // Extensions
        public List<int> SysAccountSruCodeIds { get; set; }
    }

    public class SysAccountStdTypeDTO
    {
        public int SysAccountStdTypeId { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }
        public int? SysAccountStdTypeParentId { get; set; }
    }

    public class SysAccountSruCodeDTO
    {
        public int SysAccountSruCodeId { get; set; }
        public string SruCode { get; set; }
        public string Name { get; set; }
    }

    #endregion

    #region SysContact

    public class SysContactTypeDTO
    {
        public TermGroup_SysContactType SysContactTypeId { get; set; }
        public int SysTermId { get; set; }
        public int SysTermGroupId { get; set; }
    }

    public class SysContactAddressTypeDTO
    {
        public TermGroup_SysContactAddressType SysContactAddressTypeId { get; set; }
        public TermGroup_SysContactType SysContactTypeId { get; set; }
        public int SysTermId { get; set; }
        public int SysTermGroupId { get; set; }
    }

    public class SysContactAddressRowTypeDTO
    {
        public TermGroup_SysContactAddressRowType SysContactAddressRowTypeId { get; set; }
        public TermGroup_SysContactAddressType SysContactAddressTypeId { get; set; }
        public int SysContactTypeId { get; set; }
        public int SysTermId { get; set; }
        public int SysTermGroupId { get; set; }
    }

    public class SysContactEComTypeDTO
    {
        public TermGroup_SysContactEComType SysContactEComTypeId { get; set; }
        public TermGroup_SysContactType SysContactTypeId { get; set; }
        public int SysTermId { get; set; }
        public int SysTermGroupId { get; set; }
    }

    #endregion

    #region SysCountry

    public class SysCountryDTO
    {
        public TermGroup_Country SysCountryId { get; set; }
        public string Code { get; set; }
        public int SysTermId { get; set; }
        public int SysTermGroupId { get; set; }
        public int? SysCurrencyId { get; set; }
        public string AreaCode { get; set; }
        public string Name { get; set; }
        public string CultureCode { get; set; }
    }

    #endregion

    #region SysCurrency
    [TSInclude]
    public class SysCurrencyDTO
    {
        public TermGroup_Currency SysCurrencyId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int SysTermId { get; set; }
        public int SysTermGroupId { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
    }

    #endregion  

    #region SysDayType

    public class SysDayTypeDTO
    {
        public int SysDayTypeId { get; set; }
        public int SysTermId { get; set; }
        public int SysTermGroupId { get; set; }
        public int StandardWeekdayFrom { get; set; }
        public int StandardWeekdayTo { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
    }

    #endregion

    #region SysFeature

    public class SysFeatureDTO
    {
        public int SysFeatureId { get; set; }
        public int? ParentFeatureId { get; set; }
        public int SysTermId { get; set; }
        public int SysTermGroupId { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public int Order { get; set; }
        public bool Inactive { get; set; }

        //For compare information
        public PostChange PostChange { get; set; }
    }

    #endregion

    #region SysHelp

    public class SysHelpDTO
    {
        public int SysHelpId { get; set; }
        public int SysLanguageId { get; set; }
        public int SysFeatureId { get; set; }
        public int? VersionNr { get; set; }
        public string Title { get; set; }
        public string Text { get; set; }
        public string PlainText { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }

        // Extensions
        public string Language { get; set; }
    }

    public class SysHelpSmallDTO
    {
        public int SysHelpId { get; set; }
        public string Title { get; set; }
        public string Text { get; set; }
        public string PlainText { get; set; }
        public int SysFeatureId { get; set; }
    }
    public class SysHelpTitleDto
    {
        public Feature Feature { get; set; }
        public string Title { get; set; }
    }

    #endregion

    #region SysHoliday

    public class SysHolidayDTO
    {
        public int SysHolidayId { get; set; }
        public int? SysDayTypeId { get; set; }
        public int? SysHolidayTypeId { get; set; }
        public int SysTermId { get; set; }
        public int SysTermGroupId { get; set; }
        public DateTime Date { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
    }

    public class SysHolidayTypeDTO
    {
        public int SysHolidayTypeId { get; set; }
        public int SysTermId { get; set; }
        public int SysTermGroupId { get; set; }
        public int SysCountryId { get; set; }
        public string Name { get; set; }
    }

    #endregion

    #region SysHouseholdType
    public class SysHouseholdTypeDTO
    {
        public int SysHouseholdTypeId { get; set; }
        public int SysHouseholdTypeClassification { get; set; }
        public int SysTermId { get; set; }
        public int SysTermGroupId { get; set; }
        public string XMLTagName { get; set; }
    }

    #endregion

    #region Syslanguage

    public class SysLanguageDTO
    {
        public int SysLanguageId { get; set; }
        public string LangCode { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }
        public bool Translated { get; set; }
    }

    #endregion

    #region SysLbError

    public class SysLbErrorDTO
    {
        public int SysErrorId { get; set; }
        public string LbErrorCode { get; set; }
        public int? SysTermId { get; set; }
        public int SysTermGroupId { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
    }

    #endregion

    #region SysLog

    [TSInclude]
    public class SysLogDTO
    {
        //Sorted by prio in download json
        public int SysLogId { get; set; }
        public DateTime Date { get; set; }
        public string Level { get; set; }
        public string Message { get; set; }
        public string Exception { get; set; }

        public string LicenseId { get; set; }
        public string LicenseNr { get; set; }
        public string ActorCompanyId { get; set; }
        public string CompanyName { get; set; }
        public string RoleId { get; set; }
        public string RoleName { get; set; }
        public string UserId { get; set; }
        public string LoginName { get; set; }

        public long? TaskWatchLogId { get; set; }
        public string TaskWatchLogStart { get; set; }
        public string TaskWatchLogStop { get; set; }
        public string TaskWatchLogName { get; set; }
        public string TaskWatchLogParameters { get; set; }
        public int? RecorId { get; set; }

        public string Application { get; set; }
        public string From { get; set; }
        public string HostName { get; set; }
        public string IpNr { get; set; }
        public string LineNumber { get; set; }
        public string LogClass { get; set; }
        public string Logger { get; set; }
        public string ReferUri { get; set; }
        public string RequestUri { get; set; }
        public string Session { get; set; }
        public string Source { get; set; }
        public string TargetSite { get; set; }
        public string Thread { get; set; }   
    }

    [TSInclude]
    public class SysLogGridDTO
    {
        public int SysLogId { get; set; }
        public DateTime Date { get; set; }
        public string Level { get; set; }
        public string Message { get; set; }
        public string StackTrace { get; set; }
        public string CompanyName { get; set; }
        public int UniqueCounter { get; set; }
    }

    public class AddSysLogErrorMessageModel
    {
        public string RequestUri { get; set; }
        public string Message { get; set; }
        public string Exception { get; set; }
        public bool IsWarning { get; set; }
    }

    #endregion

    #region SysMedia

    public class SysMediaDTO
    {
        public int SysMediaId { get; set; }
        public int SysLanguageId { get; set; }
        public TermGroup_SysMediaType Type { get; set; }
        public TermGroup_MediaType MediaType { get; set; }
        public TermGroup_MediaFormat FormatType { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }
        public string Filename { get; set; }
        public string Path { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }

        // Extensions
        public string TypeName { get; set; }
    }

    #endregion

    #region SysNews

    public class SysNewsSmallDTO
    {
        public int SysNewsId { get; set; }
        public DateTime PubDate { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Preview { get; set; }
    }

    public class SysNewsDTO : SysNewsSmallDTO
    {
        public int SysLanguageId { get; set; }
        public int SysXEArticleId { get; set; }

        public string Link { get; set; }
        public string Author { get; set; }
        public bool IsPublic { get; set; }

        public string AttachmentFileName { get; set; }
        public string AttachmentImageSrc { get; set; }
        public int AttachmentExportType { get; set; }

        public SoeEntityState State { get; set; }
    }

    #endregion

    #region SysPageStatus

    public class SysPageStatusDTO
    {
        public int SysPageStatusId { get; set; }
        public int SysFeatureId { get; set; }
        public TermGroup_SysPageStatusStatusType BetaStatus { get; set; }
        public TermGroup_SysPageStatusStatusType LiveStatus { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }

        // Extensions
        public string PageName { get; set; }
        public string BetaStatusName { get; set; }
        public string LiveStatusName { get; set; }
    }

    #endregion

    #region SysPaymentMethod

    public class SysPaymentMethodDTO
    {
        public int SysPaymentMethodId { get; set; }
        public int SysTermId { get; set; }
        public int SysTermGroupId { get; set; }
    }

    #endregion

    #region SysPaymentType

    public class SysPaymentTypeDTO
    {
        public int SysPaymentTypeId { get; set; }
        public int SysTermId { get; set; }
        public int SysTermGroupId { get; set; }
    }

    #endregion

    #region SysPayrollPrice

    public class SysPayrollPriceDTO
    {
        public int SysPayrollPriceId { get; set; }
        public int SysCountryId { get; set; }
        public int SysTermId { get; set; }
        public TermGroup_SysPayrollPriceType Type { get; set; }

        public string Code { get; set; }
        public decimal Amount { get; set; }
        public TermGroup_SysPayrollPriceAmountType AmountType { get; set; }
        public DateTime? FromDate { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }

        // Relations
        public List<SysPayrollPriceIntervalDTO> Intervals { get; set; }

        // Extensions
        public string Name { get; set; }
        public string TypeName { get; set; }
        public string AmountTypeName { get; set; }
        public bool IsModified { get; set; }

        //For compare information
        [TsIgnore]
        public PostChange PostChange { get; set; }
    }

    public class SysPayrollPriceSmallDTO
    {
        public int SysTermId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
    }

    public class SysPayrollPriceIntervalDTO
    {
        public int SysPayrollPriceIntervalId { get; set; }
        public int SysPayrollPriceId { get; set; }

        public decimal? FromInterval { get; set; }
        public decimal? ToInterval { get; set; }
        public decimal Amount { get; set; }
        public TermGroup_SysPayrollPriceAmountType AmountType { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }

        // Extensions
        public string AmountTypeName { get; set; }
        public TermGroup_SysPayrollPrice SysPayrollPrice { get; set; }
    }

    #endregion

    #region  SysPayrollType

    public class SysPayrollTypeDTO
    {
        public int SysCountryId { get; set; }
        public int SysTermId { get; set; }
        public int? ParentId { get; set; }

        //For compare information
        public PostChange PostChange { get; set; }

    }

    #endregion

    #region SysPerformanceMonitor

    public class SysPerformanceMonitorDTO
    {
        public int SysPerformanceMonitorId { get; set; }
        public string DatabaseName { get; set; }
        public string HostName { get; set; }
        public TermGroup_SysPerformanceMonitorTask Task { get; set; }
        public int ActorCompanyId { get; set; }
        public int? RecordId { get; set; }
        public DateTime Timestamp { get; set; }
        public int Duration { get; set; }           // Milliseconds
        public int Size { get; set; }               // Bytes fetched
        public int NbrOfRecords { get; set; }       // Number of records fetched
        public int NbrOfSubRecords { get; set; }    // Number of sub records fetched (eg: Invoice/InvoiceRow)
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
    }

    #endregion

    #region SysPermission

    public class SysPermissionDTO
    {
        public int SysPermissionId { get; set; }
        public string Name { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
    }

    #endregion

    #region SysProduct

    public class SysProductDTO
    {
        public int SysProductId { get; set; }
        public string SysProductGroupIdentifier { get; set; }
        public string Name { get; set; }
        public string EAN { get; set; }
        public string ProductId { get; set; }
        public string ExtendedInfo { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public int Type { get; set; }
        public string Manufacturer { get; set; }
        public string ImageFileName { get; set; }
        public int ExternalId { get; set; }
        public DateTime? EndAt { get; set; }
        public TermGroup_Country SysCountryId { get; set; }
    }

    public class ExternalProductSmallDTO
    {
        public int ExternalProductId { get; set; }
        public string Name { get; set; }
        public string ProductId { get; set; }
        public PriceListOrigin PriceListOrigin { get; set; }
        public int Type { get; set; }
        public string Unit { get; set; }
        public string ExtendedInfo { get; set; }
        public string Manufacturer { get; set; }
        public int? ExternalId { get; set; }
        public string ImageUrl  { get; set; }
        public DateTime? EndAt { get; set; }
        public List<SysPriceListSmallDTO> SysPriceListSmallDTOs { get; set; }
    }

    #endregion

    #region SysProductGroup
    [TSInclude]
    public class SysProductGroupSmallDTO
    {
        public int SysProductGroupId { get; set; }
        public int? ParentSysProductGroupId { get; set; }
        public string Identifier { get; set; }
        public string Name { get; set; }
    }

    #endregion

    #region SysPosition
    [TSInclude]
    public class SysPositionDTO
    {
        public int SysPositionId { get; set; }
        public int SysCountryId { get; set; }
        public int SysLanguageId { get; set; }

        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }

        //Extensions
        public string SysCountryCode { get; set; }
        public string SysLanguageCode { get; set; }
    }

    [TSInclude]
    public class SysPositionGridDTO
    {
        public int SysPositionId { get; set; }

        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        // Extensions
        public string SysCountryCode { get; set; }
        public string SysLanguageCode { get; set; }
        public bool IsLinked { get; set; }
        public bool Selected { get; set; }
    }

    #endregion

    #region SysPriceList

    [TSInclude]
    public class SysPricelistProviderDTO {
        public int Id { get; set; }
        public string Name { get; set; }
    }
    public class SysPriceListDTO
    {
        public int SysPriceListId { get; set; }
        public int SysPriceListHeadId { get; set; }
        public int SysProductId { get; set; }
        public decimal GNP { get; set; }
        public string PurchaseUnit { get; set; }
        public string SalesUnit { get; set; }
        public bool EnvironmentFee { get; set; }
        public bool Storage { get; set; }
        public string ReplacesProduct { get; set; }
        public decimal? PackageSizeMin { get; set; }
        public decimal? PackageSize { get; set; }
        public string ProductLink { get; set; }
        public DateTime? PriceChangeDate { get; set; }
        public int SysWholesellerId { get; set; }
        public int PriceStatus { get; set; }
        public string Code { get; set; }
    }

    #region SysPriceListHead

    public class SysPriceListHeadDTO
    {
        public int SysPriceListHeadId { get; set; }
        public DateTime Date { get; set; }
        public int? Version { get; set; }
        public int Provider { get; set; }
        public int SysWholesellerId { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
    }
    [TSInclude]
    public class SysPriceListHeadGridDTO
    {
        public int Provider { get; set; }
        public string ProviderName { get; set; }
        public int SysWholesellerId { get; set; }
        public string SysWholesellerName { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
    }
    [TSInclude]
    public class SysPriceListImportDTO 
    {
        public int Provider { get; set; }
        public FileDTO File { get; set; }
    }

    public class SysPriceListSmallDTO
    {
        public int SysPriceListId { get; set; }
        public int SysPriceListHeadId { get; set; }
        public int SysWholesellerId { get; set; }
        public int SysProductId { get; set; }
    }
    #endregion

    #region SysReportTemplate

        public class SysReportTemplateDTO
    {
        public int SysReportTemplateId { get; set; }
        public string FileName { get; set; }
        public byte[] Template { get; set; }
        public int SysReportTypeId { get; set; }
        public int SysTemplateTypeId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public int? SysCountryId { get; set; }
        public int GroupByLevel1 { get; set; }
        public int GroupByLevel2 { get; set; }
        public int GroupByLevel3 { get; set; }
        public int GroupByLevel4 { get; set; }
        public int SortByLevel1 { get; set; }
        public int SortByLevel2 { get; set; }
        public int SortByLevel3 { get; set; }
        public int SortByLevel4 { get; set; }
        public bool IsSortAscending { get; set; }
        public string Special { get; set; }
    }

    #region SysReportTemplateType

    public class SysReportTemplateTypeDTO
    {
        public int SysReportTemplateTypeId { get; set; }
        public int SysReportTermId { get; set; }
        public int SelectionType { get; set; }
        public bool GroupMapping { get; set; }
        public int Module { get; set; }
        public int Group { get; set; }

        //For compare information
        public PostChange PostChange { get; set; }

    }

    #endregion


    public class SysReportTypeDTO
    {
        public int SysReportTypeId { get; set; }
        public int SysTermId { get; set; }
        public int SysTermGroupId { get; set; }
        public string FileExtension { get; set; }

        //For compare information
        public PostChange PostChange { get; set; }
    }

    #endregion

    #region SysSetting

    public class SysSettingDTO
    {
        public int SysSettingId { get; set; }
        public int SysTermId { get; set; }
        public int SysTermGroupId { get; set; }
        public int SysSettingTypeId { get; set; }
    }

    #region SysSettingType

    public class SysSettingTypeDTO
    {
        public int SysSettingTypeId { get; set; }
        public int SysTermId { get; set; }
        public int SysTermGroupId { get; set; }
    }

    #endregion

    #endregion

    #region SysTerm

    public class SysTermDTO
    {
        public int SysTermId { get; set; }
        public int SysTermGroupId { get; set; }
        public int LangId { get; set; }
        public string Name { get; set; }
        [TsIgnore]
        public string Description { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }

        public string TranslationKey { get; set; }

        //For compare information
        public PostChange PostChange { get; set; }

        public bool IsEqualTo(SysTermDTO other)
        {
            return
                this.SysTermId == other.SysTermId &&
                this.SysTermGroupId == other.SysTermGroupId &&
                this.LangId == other.LangId &&
                this.Name.Equals(other.Name) &&
                this.Created == other.Created &&
                this.CreatedBy.Equals(other.CreatedBy) &&
                this.Modified == other.Modified &&
                this.ModifiedBy.Equals(other.ModifiedBy);
        }
    }

    public class SysTermJsonDTO
    {
        public int Id { get; set; }
        public int LId { get; set; }
        public int GId { get; set; }
        public string GName { get; set; }
        public string Name { get; set; }
    }

    #endregion

    #region SysTermGroup

    public class SysTermGroupDTO
    {
        public int SysTermGroupId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public PostChange PostChange { get; set; }

        public bool IsEqualTo(SysTermGroupDTO other)
        {
            return
                this.SysTermGroupId == other.SysTermGroupId &&
                this.Name.Equals(other.Name) &&
                this.Description.Equals(other.Description);
        }
    }

    #endregion

    #region SysVehicleType

    public class SysVehicleTypeDTO
    {
        public int SysVehicleTypeId { get; set; }
        public string Filename { get; set; }
        public int ManufacturingYear { get; set; }
        public string XML { get; set; }
        public DateTime? DateFrom { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }
    }

    public class SysVehicleTypeGridDTO
    {
        public int SysVehicleTypeId { get; set; }
        public string Filename { get; set; }
        public int ManufacturingYear { get; set; }
        public DateTime? DateFrom { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
    }

    public class SysVehicleDTO
    {
        public int ManufacturingYear { get; set; }
        public string ModelCode { get; set; }
        public string VehicleMake { get; set; }
        public string VehicleModel { get; set; }
        public decimal Price { get; set; }
        public TermGroup_SysVehicleFuelType FuelType { get; set; }
        public string CodeForComparableModel { get; set; }
        public decimal PriceAdjustment { get; set; }
        public decimal priceAfterReduction { get; set; }
        public SysVehicleDTO ComparableModel { get; set; }
    }

    #endregion

    #region SysVatAccount

    public class SysVatAccountDTO
    {
        public int SysVatAccountId { get; set; }
        public string AccountCode { get; set; }
        public int? VatCode { get; set; }
        public int? LangId { get; set; }
        public int? VatNr1 { get; set; }
        public int? VatNr2 { get; set; }
        public string Name { get; set; }
    }

    #endregion

    #region SysVatRAte

    public class SysVatRateDTO
    {
        public int SysVatAccountId { get; set; }
        public decimal VatRate { get; set; }
        public DateTime Date { get; set; }
        public int IsActive { get; set; }
    }

    #endregion

    #region SysWholeseller

    public class SysWholesellerDTO
    {
        public int SysWholesellerId { get; set; }
        public string Name { get; set; }
        public int Type { get; set; }
        public int SysCountryId { get; set; }
        public int SysCurrencyId { get; set; }
        public bool IsOnlyInComp { get; set; }
        public string MessageTypes { get; set; }
        public int? SysWholesellerEdiId { get; set; }
        public bool HasEdiFeature { get; set; }

        public List<SysWholeSellerSettingDTO> SysWholeSellerSettingDTOs { get; set; }
    }

    #region SysWholeSellerSetting

    public class SysWholeSellerSettingDTO
    {
        public int SysWholesellerSettingId { get; set; }
        public int SysWholesellerId { get; set; }
        public int SettingType { get; set; }
        public string StringValue { get; set; }
        public int? IntValue { get; set; }
        public bool? Boolvalue { get; set; }
        public decimal? DecimalValue { get; set; }
    }

    #endregion

    #endregion

    #region SysXEArticle

    public class SysXEArticleDTO
    {
        public int SysXEArticleId { get; set; }
        public string Name { get; set; }
        public string ArticleNr { get; set; }
        public string ArticleNrC { get; set; }
        public string Description { get; set; }
        public bool Inactive { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public string ModuleGroup { get; set; }
        public string ArticleNrYear1 { get; set; }
        public string ArticleNrYear2 { get; set; }
        public decimal? StartPrice { get; set; }
        public decimal? MonthlyPrice { get; set; }
        public int SortOrder { get; set; }
        public List<SysXEArticleFeatureDTO> SysXEArticleFeatures { get; set; }
    }

    #region SysXEArticleFeature

    public class SysXEArticleFeatureDTO
    {
        public int SysXEArticleId { get; set; }
        public int SysFeatureId { get; set; }
        public int SysPermissionId { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
    }

    #endregion

    #endregion

    #endregion

    #region SysServer

    public class SysServerDTO
    {
        public int SysServerId { get; set; }
        public string Url { get; set; }
        public bool UseLoadBalancer { get; set; }
    }

    #endregion

    #region SysTimeInterval

    public class SysTimeIntervalDTO
    {
        public int SysTimeIntervalId { get; set; }
        public int SysTermId { get; set; }
        public TermGroup_TimeIntervalPeriod Period { get; set; }
        public TermGroup_TimeIntervalStart Start { get; set; }
        public int StartOffset { get; set; }
        public TermGroup_TimeIntervalStop Stop { get; set; }
        public int StopOffset { get; set; }
        public int Sort { get; set; }

        // Extensions
        public string Name { get; set; }
    }

    #endregion

    #region SysBank

    [TSInclude]
    public class SysBankDTO
    {
        public int SysBankId { get; set; }
        public int SysCountryId { get; set; }
        public string Name { get; set; }
        public string BIC { get; set; }
        public bool HasIntegration { get; set; }
    }

    #endregion

    #endregion

    #region Views

    #region SysPayrollTypeView

    public class SysPayrollTypeViewDTO
    {
        public int SysTermId { get; set; }
        public int? ParentId { get; set; }
        public string Name { get; set; }
    }

    #endregion

    #region SysReportTemplateView

    public class SysReportTemplateViewDTO
    {
        public int SysReportTemplateId { get; set; }
        public int SysReportTypeId { get; set; }
        public int SysTemplateTypeId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int SysReportTermId { get; set; }
        public int SelectionType { get; set; }
        public bool GroupMapping { get; set; }
        public SoeModule Module { get; set; }
        //Extensions
        public string SysReportTemplateTypeName { get; set; }

    }

    public class SysReportTemplateViewGridDTO
    {
        public int SysReportTemplateId { get; set; }
        public int? ReportNr { get; set; }
        public string GroupName { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<int> SysCountryIds { get; set; }
        public bool IsSystemReport { get; set; }

        //Extensions
        public string SysReportTemplateTypeName { get; set; }
    }


    public class SysLinkDTO
    {
        public int SysLinkTableRecordType { get; set; }
        public int SysLinkTableKeyItemId { get; set; }
        public int SysLinkTableIntegerValueType { get; set; }
        public int SysLinkTableIntegerValue { get; set; }

        //Extensions
        public string SysLinkTableIntegerValueName { get; set; }
    }
    #endregion


    #endregion
}
