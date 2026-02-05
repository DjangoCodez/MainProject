using System;
using System.Collections.Generic;

namespace SoftOne.SOE.SSU.DTO
{
    public class SettingDTO
    {
        public int ServerId { get; set; }
        public string ServerName { get; set; }
        public int ServerPrio { get; set; }
        public bool CheckHardware { get; set; }
        public bool CheckNetwork { get; set; }
        public bool CheckUrls { get; set; }
        public bool CheckIIS { get; set; }
        public bool CheckDeploy { get; set; }

        public BackupType BackupType { get; set; }

        public bool BuildRequests { get; set; }
        public bool SendSMS { get; set; }
        public bool SendEmail { get; set; }
        public List<UrlDTO> UrlDTOs { get; set; }
        public List<SiteDTO> SiteDTOs { get; set; }

        public SettingDTO CreateDefaultSettingDTO()
        {
            SettingDTO dto = new SettingDTO();
            dto.CheckHardware = true;
            dto.CheckNetwork = true;
            dto.SendSMS = false;
            dto.SendEmail = false;
            dto.CheckUrls = false;
            dto.CheckIIS = false;
            dto.CheckDeploy = false;
            dto.BackupType = BackupType.None;

            UrlDTO urlDTO = new UrlDTO();
            dto.UrlDTOs = new List<UrlDTO>();
            dto.UrlDTOs.Add(urlDTO.CreateDefaultUrlDTO());

            return dto;
        }
    }

    public class ServerDTO
    {
        public int ServerId { get; set; }
        public string ServerName { get; set; }
        public string XEVersion { get; set; }
        public string ServerBusinessVersion { get; set; }
        public string IPAddress { get; set; }
        public DateTime LatestAlive { get; set; }
        public List<SettingDTO> settingDTOs { get; set; }
    }

    public class EventDTO
    {
        public int EventId { get; set; }
        public int ServerId { get; set; }
        public string ServerName { get; set; }
        public DateTime EventTime { get; set; }
        public EventType Type { get; set; }
        public string Message { get; set; }
        public bool SMSSent { get; set; }
        public bool EmailSent { get; set; }
        public int EventPrio { get; set; }
        public Guid Guid { get; set; }
        public bool isAlreadyReported { get; set; }

        public UrlDTO urlDTO { get; set; }

        public List<ActionDTO> actionDTOs { get; set; }
    }

    public class ActionDTO
    {
        public int ActionEventId { get; set; }
        public ActionType Type { get; set; }
        public DateTime ActionTime { get; set; }
        public int Prio { get; set; }
        public Guid Guid { get; set; }
        public int ServerId { get; set; }
        public int? SiteId { get; set; }
        public bool IsExecuted { get; set; }
        public DateTime? ExecutionTime { get; set; }
        public bool Validated { get; set; }
        public Guid ValidateGuid { get; set; }
        public DateTime? ValidateStopTime { get; set; }
        public string ValidatedBy { get; set; }

        public string Link { get; set; }
    }

    public class BackupRequestDTO
    {
        public int BackupRequestId { get; set; }
        public BackupRequestType BackupRequestType { get; set; }
        public BackupType BackupType { get; set; }
        public int SiteId { get; set; }
        public Guid Guid { get; set; }
        public BackupRequestStatus Status { get; set; }
        public string StringValue { get; set; }
        public string StringValue2 { get; set; }
        public int? IntValue { get; set; }
        public int? IntValue2 { get; set; }
        public string Message { get; set; }
        public DateTime? RequestTime { get; set; }
        public DateTime? Created { get; set; }
        public DateTime? Modified { get; set; }
        public int RepeatHour { get; set; }
        public int State { get; set; }

        public static BackupRequestDTO CloneDTO(BackupRequestDTO backupRequestDTO)
        {
            BackupRequestDTO backupRequest = new BackupRequestDTO();

            backupRequest.BackupRequestId = backupRequestDTO.BackupRequestId;
            backupRequest.BackupRequestType = backupRequestDTO.BackupRequestType;
            backupRequest.BackupType = backupRequestDTO.BackupType;
            backupRequest.Guid = backupRequestDTO.Guid;
            backupRequest.IntValue = backupRequestDTO.IntValue;
            backupRequest.Message = backupRequestDTO.Message;
            backupRequest.SiteId = backupRequestDTO.SiteId;
            backupRequest.Status = backupRequestDTO.Status;
            backupRequest.StringValue = backupRequestDTO.StringValue;
            backupRequest.StringValue2 = backupRequestDTO.StringValue2;
            backupRequest.Created = backupRequestDTO.Created;
            backupRequest.Modified = backupRequestDTO.Modified;
            backupRequest.State = backupRequestDTO.State;
            backupRequest.RequestTime = backupRequestDTO.RequestTime;
            backupRequest.RepeatHour = backupRequestDTO.RepeatHour;

            return backupRequest;
        }
    }

    public class SiteDTO
    {
        public int SiteId { get; set; }
        public SiteType SiteType { get; set; }
        public string SiteTypeName { get; set; }
        public int ServerId { get; set; }
        public string URL { get; set; }
        public string MainFolder { get; set; }
        public bool IsChecked { get; set; }
        public bool IsExternal { get; set; }
        public int? ServiceId { get; set; }

        public List<DeployDTO> DeployDTOs { get; set; }

        public List<SiteSettingDTO> SiteSettingDTOs { get; set; }
    }

    public class SiteSettingDTO
    {
        public int SiteSettingId { get; set; }
        public int SiteId { get; set; }
        public SiteSettingType SiteSettingType { get; set; }
        public string SiteSettingTypeName { get; set; }
        public bool? BoolValue { get; set; }
        public string StringValue { get; set; }
        public Decimal? DecimalValue { get; set; }
        public int? IntValue { get; set; }
        public int State { get; set; }

    }

    public class SiteSettingValues
    {
        public string SiteSettingTypeName { get; set; }
        public bool BoolValue { get; set; }
        public string StringValue { get; set; }
        public Decimal DecimalValue { get; set; }
        public int IntValue { get; set; }
    }

    public class PerformanceResultDTO
    {
        public int PerformanceResultId { get; set; }
        public PerformanceType PerformanceType { get; set; }
        public string PerformanceTypeName { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int Time { get; set; }
        public int Size { get; set; }
        public int Quantity { get; set; }
        public int PerformanceSettingId { get; set; }
        public string Message { get; set; }
        public string Information { get; set; }
        public string PerformanceSettingName { get; set; }
        public string PerformanceSettingDescription { get; set; }
    }

    public class PerformanceResultGridDTO
    {
        public int PerformanceResultId { get; set; }
        public PerformanceType PerformanceType { get; set; }
        public string PerformanceTypeName { get; set; }
        public string PerformanceSettingName { get; set; }
        public DateTime StartTime { get; set; }
        public int Time { get; set; }
        public string Message { get; set; }
    }

    public class PerformanceSettingDTO
    {
        public int PerformanceSettingId { get; set; }
        public PerformanceType PerformanceType { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<PerformanceSettingParameterDTO> PerformanceSettingParameterDTOs { get; set; }
    }

    public class PerformanceSettingParameterDTO
    {
        public int PerformanceSettingParameterId { get; set; }
        public PerformanceSettingParameterType PerformanceSettingParameterType { get; set; }
        public string Stringvalue { get; set; }
        public decimal? Decimalvalue { get; set; }
        public int? Intvalue { get; set; }

    }

    public class BuildRequestDTO
    {
        public int BuildRequestId { get; set; }
        public int ServerId { get; set; }
        public Guid Guid { get; set; }
        public string Version { get; set; }
        public BuildType BuildType { get; set; }
        public string TempFolder { get; set; }
        public string ZipFolder { get; set; }
        public DateTime DeployTime { get; set; }
        public bool RemoveConfigFiles { get; set; }
        public int BuildDefinitionId { get; set; }
        public int SiteId { get; set; }
        public BuildStatus Status { get; set; }
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public int State { get; set; }
        public string XapName { get; set; }
        public int RepeatHour { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? BuildStarted { get; set; }
        public string Message { get; set; }
        public int BuildId { get; set; }
        public string User { get; set; }
        public List<int> SiteIds { get; set; }
        public List<int> ServiceIds { get; set; }
        public bool UseDacpac { get; set; }
        public string Email { get; set; }
        public bool UseUpdateSysterm { get; set; }
        public bool IsExternal { get; set; }
        public string Password { get; set; }
    }

    public class DeploySmallDTO
    {
        public Guid Guid { get; set; }
        public string Version { get; set; }
        public DateTime UTCDeployTime { get; set; }
        public string BuildDefintionId { get; set; }
    }

    public class UrlDTO
    {
        public int UrlId { get; set; }
        public int URLPrio { get; set; }
        public string URL { get; set; }
        public string Status { get; set; }
        public DateTime Time { get; set; }
        public string ErrorMessage { get; set; }
        public int SiteId { get; set; }
        public int ServerId { get; set; }
        public string ServerName { get; set; }

        public UrlDTO CreateDefaultUrlDTO()
        {
            UrlDTO dto = new UrlDTO();

            dto.Status = "";
            dto.URL = "https://xe.softone.se";
            dto.URLPrio = 3;

            return dto;
        }
    }

    public class DeployDTO
    {
        public int DeployId { get; set; }
        public DeployType DeployType { get; set; }
        public int SiteId { get; set; }
        public DateTime UTCDeployTime { get; set; }
        public string ExtractLocation { get; set; }
        public string Version { get; set; }
        public string URL { get; set; }
        public bool IsDeployed { get; set; }
        public bool isZipValidated { get; set; }
        public string ZipLocation { get; set; }
        public string DeploySettingDTOXML { get; set; }
        public Guid Guid { get; set; }
        public bool DownloadSucessful { get; set; }

    }

    public class DeploySettingDTO
    {
        public int DeploySettingDTOType { get; set; }
        public string setting { get; set; }
    }

    public enum ActionResultSave
    {
        // Common errors
        Unknown = 0,
        EntityIsNull = 1,
        EntityNotFound = 2,
        EntityNotActive = 3,
        NothingSaved = 4,
        EntityNotUpdated = 5,
        EntityNotCreated = 6,
        MaximumLengthExceed = 7,
        InsufficientInput = 8,
        Duplicate = 9,
        IncorrectInput = 10,
        EntityExists = 11,

        // SMS 
        SendSMSNoSender = 1701,
        SendSMSNoReceivers = 1702,
        SendSMSNoMessage = 1703,
        SendSMSResponseIsNull = 1704,
        SendSMSResponseReturnedFailed = 1705,
        SMSNoProviderExists = 1706,
        AreaCodeNotFound = 1707,
        SMSInsuficientData = 1708,
        SMSCompanyLimitReached = 1709,
    }

    public enum EventType
    {
        Unknown = 0,
        NoAliveSignal = 1,
        IISnotRunning = 2,
        LowDisk = 3,
        HighCPU = 4,
        HighMemory = 5,
        UrlCheckFailed = 6,
        AliveFailed = 7,
        LocalhostNotResponing = 8,
        DiskIsFillingUpFast = 9,
        ActionExecuted = 10,
        DailyInformationMailSent = 11,
        BuildRequestFailed = 12,

        WinEventlogInformation = 50,
        WinEventlogError = 51,

        NoAliveSignalNoLongerCurrent = 101,
        IISnotRunningNoLongerCurrent = 102,
        LowDiskNoLongerCurrent = 103,
        HighCPUNoLongerCurrent = 104,
        HighMemoryNoLongerCurrent = 105,
        UrlCheckFailedNoLongerCurrent = 106,
        AliveFailedNoLongerCurrent = 107,
        LocalhostNotResponingNoLongerCurrent = 108,
        DiskIsFillingUpFastNoLongerCurrent = 109,

    }

    public enum JobType
    {
        Unknown = 0,
        Alive = 1,
        NetworkInformation = 2,
        HardwareInformation = 3,
        IISInformation = 4,
        GetSettings = 5,
        CheckURLs = 6,
        CheckDeploy = 7,
        Deploy = 8,
        BackupXE = 9,

    }

    public enum ContactType
    {
        Unknown = 0,
        FirstLine = 1,
        SecondLine = 2,
    }

    public enum BackupType
    {
        None = 0,
        All = 1,
        OnlyTempFolder = 2,
        OnlyConfigFiles = 3,
        TempFolderAndConfigFiles = 4,
    }

    public enum DeployType
    {
        Files = 0,
        msDeploy = 1,
    }

    public enum BuildType
    {
        unknown = 0,
        deploy = 1,
    }

    public enum BuildStatus
    {
        requested = 0,
        pending = 1,
        success = 2,
        failed = 3,
        closed = 4,
    }

    public enum Overwrite
    {
        Always,
        IfNewer,
        Never
    }

    public enum ArchiveAction
    {
        Merge,
        Replace,
        Error,
        Ignore
    }

    public enum ActionType
    {
        RestartIIS = 1,
        RestartWindowsService = 2,
        RestartServer = 3,
        CleanComputer = 4,
        RestartSSU = 5,
        RecycleApplicationPools = 6,
    }

    public enum IODictionaryType
    {
        Unkown = 0,
        AccountNr = 1,
        AccountInternalDim2Nr = 2,
        AccountInternalDim3Nr = 3,
        AccountInternalDim4Nr = 4,
        AccountInternalDim5Nr = 5,
        AccountInternalDim6Nr = 6,
        InvoiceProduct = 7,
        PayrollProduct = 8,
        EmployeeGroup = 9,
        PayrollGroup = 10,
        VacationGroup = 11,
        EmploymentType = 12,
    }

    public enum SiteType
    {
        Unkown = 0,
        WebServer = 1,
        SqlServer = 2,
        WebAndSqlServer = 3,
    }

    public enum BackupRequestStatus
    {
        None = 0,
        Initiated = 1,
        PhaseOneCompleted = 2,
        PhaseTwoCompleted = 3,
        PhaseThreeCompleted = 4,
        PhaseFiveCompleted = 5,

        Failed = 10,
        Done = 11,

    }

    public enum SiteSettingType
    {
        Unkown = 0,
        IsProduction = 1,
        Folder = 2,
        DatabaseServer = 3,
        DatebaseName = 4,
        DatebaseUser = 5,
        DatebasePassWord = 6,
        SiteId = 7,
        SiteIsBackupOfSiteId = 8,
    }
    public enum BackupRequestType
    {
        None = 0,
        RequestFromChild = 1,
        RequestFromChildRepeat = 2,
    }

    public enum PerformanceType
    {
        Unkown = 0,
        TimeAttendenceView = 1,
        MobileOrderList = 2,
        EmployeeList = 3,
        ScheduleList = 4,
        CustomerList = 5,
    }

    public enum PerformanceSettingParameterType
    {
        Unkown = 0,
        License = 1,
        UserName = 2,
        Password = 3,
        UserId = 4,
        ActorCompanyId = 5,
        EmployeeId = 6,
        InvoiceId = 7,
        TimeTerminalId = 8,
        CustomerId = 9,
        URL = 10,
        Server = 11,
        CompanyAPIKey = 12,
        ConnectAPIKey = 13,
        OrderListTypeMobil = 14,
        CultureCode = 15,
        Version = 16,
        RoleId = 17,
    }
}





