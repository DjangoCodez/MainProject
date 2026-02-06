using log4net;
using log4net.Core;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Transactions;

namespace SoftOne.Soe.Business.Core
{
    public class SysLogManager : ManagerBase
    {
        #region Variables

        // Create a logger for use in this class
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #endregion

        #region Ctor

        public SysLogManager(ParameterObject parameterObject) : base(parameterObject) { }

        #endregion

        #region SysLog

        public List<SysLog> SearchLogEntries(SearchSysLogsDTO dto)
        {
            if (!dto.NoOfRecords.HasValue || dto.NoOfRecords.Value <= 0 || dto.NoOfRecords.Value > Constants.SOE_SYSLOG_MAX)
                dto.NoOfRecords = Constants.SOE_SYSLOG_MAX;

            using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
            IQueryable<SysLog> query = sysEntitiesReadOnly.SysLog;

            // Level
            if (!String.IsNullOrEmpty(dto.Level?.Trim()) && dto.Level != "NONE")
                query = query.Where(r => r.Level == dto.Level.Trim());

            // DateFrom
            if (dto.FromDate.HasValue)
            {
                DateTime date = CalendarUtility.GetDateTime(dto.FromDate.Value, dto.FromTime ?? CalendarUtility.DATETIME_DEFAULT);
                query = query.Where(r => r.Date >= date);
            }

            // DateTo
            if (dto.ToDate.HasValue)
            {
                DateTime date = CalendarUtility.GetDateTime(dto.ToDate.Value, dto.ToTime ?? CalendarUtility.DATETIME_DEFAULT);
                query = query.Where(r => r.Date <= date);
            }

            // License
            if (!String.IsNullOrEmpty(dto.LicenseSearch?.Trim()))
            {
                License license = LicenseManager.GetLicenseByNr(dto.LicenseSearch.Trim());
                if (license != null)
                    query = query.Where(r => r.LicenseId != null && r.LicenseId == license.LicenseId.ToString());
            }

            // Companies
            if (!string.IsNullOrEmpty(dto.CompanySearch?.Trim()))
            {
                var companies = CompanyManager.GetCompaniesBySearch(new CompanySearchFilterDTO { NameOrLicense = dto.CompanySearch.Trim() });
                if (!companies.IsNullOrEmpty())
                {
                    List<string> actorCompanyIds = companies.Select(c => c.ActorCompanyId.ToString()).ToList();
                    query = query.Where(p => p.ActorCompanyId != null && actorCompanyIds.Contains(p.ActorCompanyId));
                }
            }

            // Roles
            if (!String.IsNullOrEmpty(dto.RoleSearch?.Trim()))
            {
                List<Role> roles = RoleManager.GetRolesByName(dto.RoleSearch.Trim(), null);
                if (roles.IsNullOrEmpty())
                {
                    List<string> roleIds = roles.Select(c => c.RoleId.ToString()).ToList();
                    query = query.Where(p => p.RoleId != null && roleIds.Contains(p.RoleId));
                }
            }

            // Users
            if (!String.IsNullOrEmpty(dto.UserSearch?.Trim()))
            {
                List<User> users = UserManager.GetUsersByName(dto.UserSearch.Trim());
                if (users != null && users.Count > 0)
                {
                    List<string> userIds = users.Select(c => c.UserId.ToString()).ToList();
                    query = query.Where(p => p.UserId != null && userIds.Contains(p.UserId));
                }
            }

            if (dto.IncMessageSearch != dto.ExlMessageSearch)
            {
                // Include Message search
                if (!String.IsNullOrEmpty(dto.IncMessageSearch?.Trim()))
                {
                    string[] parts = dto.IncMessageSearch.Trim().ToLower().Split(';');
                    foreach (string part in parts)
                    {
                        query = query.Where(p => p.Message.ToLower().Contains(part.Trim()));
                    }
                }

                // Exlude Message search
                if (!string.IsNullOrEmpty(dto.ExlMessageSearch?.Trim()))
                {
                    string[] parts = dto.ExlMessageSearch.Trim().ToLower().Split(';');
                    foreach (string part in parts)
                    {
                        query = query.Where(p => !p.Message.ToLower().Contains(part.Trim()));
                    }
                }
            }

            if (dto.IncExceptionSearch != dto.ExExceptionSearch)
            {
                // Include Exception search
                if (!string.IsNullOrEmpty(dto.IncExceptionSearch?.Trim()))
                {
                    string[] parts = dto.IncExceptionSearch.Trim().ToLower().Split(';');
                    foreach (string part in parts)
                    {
                        query = query.Where(p => p.Exception.ToLower().Contains(part.Trim()));
                    }
                }

                // Exlude Exception search
                if (!string.IsNullOrEmpty(dto.ExExceptionSearch?.Trim()))
                {
                    string[] parts = dto.ExExceptionSearch.Trim().ToLower().Split(';');
                    foreach (string part in parts)
                    {
                        query = query.Where(p => !p.Exception.ToLower().Contains(part.Trim()));
                    }
                }
            }

            // Run query
            List<SysLog> logEntrys = (from p in query
                                      orderby p.Date descending
                                      select p).Take(dto.NoOfRecords.Value).ToList();


            if (dto.ShowUnique)
                logEntrys = logEntrys.GetUnique();
            if (!dto.ShowUnique)
                AddCompanyNameToSysLogs(logEntrys);

            return logEntrys;
        }

        public List<SysLog> GetSysLogs(SoeLogType logType, bool showUnique = false, int? licenseId = null, int? actorCompanyId = null, int? roleId = null, int? userId = null, string clientIpNr = "", int? noOfRecords = null)
        {
            using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
            return GetSysLogs(sysEntitiesReadOnly, logType, showUnique, licenseId, actorCompanyId, roleId, userId, clientIpNr, noOfRecords);
        }

        public List<SysLog> GetSysLogs(SOESysEntities entities, SoeLogType logType, bool showUnique = false, int? licenseId = null, int? actorCompanyId = null, int? roleId = null, int? userId = null, string clientIpNr = "", int? noOfRecords = null)
        {
            List<SysLog> logEntrys = null;

            DateTime date = DateTime.Now;
            if (!noOfRecords.HasValue || noOfRecords.Value <= 0 || noOfRecords.Value > Constants.SOE_SYSLOG_MAX)
                noOfRecords = Constants.SOE_SYSLOG_MAX;

            switch (logType)
            {
                case SoeLogType.System_All:
                    #region System_All

                    date = CalendarUtility.GetEndOfDay(DateTime.Today);

                    logEntrys = (from sl in entities.SysLog
                                 where sl.Date <= date
                                 orderby sl.Date descending
                                 select sl).Take(noOfRecords.Value).ToList();

                    #endregion
                    break;
                case SoeLogType.System_All_Today:
                    #region System_All_Today

                    date = DateTime.Today.Date;

                    logEntrys = (from sl in entities.SysLog
                                 where sl.Date.Year == date.Year &&
                                 sl.Date.Month == date.Month &&
                                 sl.Date.Day == date.Day
                                 orderby sl.Date descending
                                 select sl).Take(noOfRecords.Value).ToList();

                    #endregion
                    break;
                case SoeLogType.System_Error:
                    #region System_Error

                    date = CalendarUtility.GetEndOfDay(DateTime.Today);

                    logEntrys = (from sl in entities.SysLog
                                 where sl.Level == Level.Error.Name &&
                                 sl.Date <= date
                                 orderby sl.Date descending
                                 select sl).Take(noOfRecords.Value).ToList();

                    #endregion
                    break;
                case SoeLogType.System_Error_Today:
                    #region System_Error_Today

                    date = DateTime.Today.Date;

                    logEntrys = (from sl in entities.SysLog
                                 where sl.Level == Level.Error.Name &&
                                 sl.Date.Year == date.Year &&
                                 sl.Date.Month == date.Month &&
                                 sl.Date.Day == date.Day
                                 orderby sl.Date descending
                                 select sl).Take(noOfRecords.Value).ToList();

                    #endregion
                    break;
                case SoeLogType.System_Warning:
                    #region System_Warning

                    date = CalendarUtility.GetEndOfDay(DateTime.Today);

                    logEntrys = (from sl in entities.SysLog
                                 where sl.Level == Level.Warn.Name &&
                                 sl.Date <= date
                                 orderby sl.Date descending
                                 select sl).Take(noOfRecords.Value).ToList();

                    #endregion
                    break;
                case SoeLogType.System_Warning_Today:
                    #region System_Warning_Today

                    date = DateTime.Today.Date;

                    logEntrys = (from sl in entities.SysLog
                                 where sl.Level == Level.Warn.Name &&
                                 sl.Date.Year == date.Year &&
                                 sl.Date.Month == date.Month &&
                                 sl.Date.Day == date.Day
                                 orderby sl.Date descending
                                 select sl).Take(noOfRecords.Value).ToList();

                    #endregion
                    break;
                case SoeLogType.System_Information:
                    #region System_Information

                    date = CalendarUtility.GetEndOfDay(DateTime.Today);

                    logEntrys = (from sl in entities.SysLog
                                 where sl.Level == Level.Info.Name &&
                                 sl.Date <= date
                                 orderby sl.Date descending
                                 select sl).Take(noOfRecords.Value).ToList();

                    #endregion
                    break;
                case SoeLogType.System_Information_Today:
                    #region System_Information_Today

                    date = DateTime.Today.Date;

                    logEntrys = (from sl in entities.SysLog
                                 where sl.Level == Level.Info.Name &&
                                 sl.Date.Year == date.Year &&
                                 sl.Date.Month == date.Month &&
                                 sl.Date.Day == date.Day
                                 orderby sl.Date descending
                                 select sl).Take(noOfRecords.Value).ToList();

                    #endregion
                    break;
                case SoeLogType.License:
                    #region License

                    if (licenseId.HasValue)
                    {
                        string idStr = licenseId.ToString();

                        logEntrys = (from sl in entities.SysLog
                                     where sl.LicenseId == idStr
                                     orderby sl.Date descending
                                     select sl).Take(noOfRecords.Value).ToList();
                    }

                    #endregion
                    break;
                case SoeLogType.Company:
                    #region Company

                    if (actorCompanyId.HasValue)
                    {
                        string idStr = actorCompanyId.Value.ToString();

                        logEntrys = (from sl in entities.SysLog
                                     where sl.ActorCompanyId == idStr
                                     orderby sl.Date descending
                                     select sl).Take(noOfRecords.Value).ToList();
                    }

                    #endregion
                    break;
                case SoeLogType.Role:
                    #region Role

                    if (roleId.HasValue)
                    {
                        string idStr = roleId.Value.ToString();

                        logEntrys = (from sl in entities.SysLog
                                     where sl.RoleId == idStr
                                     orderby sl.Date descending
                                     select sl).Take(noOfRecords.Value).ToList();
                    }

                    #endregion
                    break;
                case SoeLogType.User:
                    #region User

                    if (userId.HasValue)
                    {
                        string idStr = roleId.Value.ToString();

                        logEntrys = (from sl in entities.SysLog
                                     where sl.UserId == idStr
                                     orderby sl.Date descending
                                     select sl).Take(noOfRecords.Value).ToList();
                    }

                    #endregion
                    break;
                case SoeLogType.Machine:
                    #region Machine

                    string host = "";
                    if (!String.IsNullOrEmpty(host) && !String.IsNullOrEmpty(clientIpNr))
                    {
                        using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
                        logEntrys = (from sl in sysEntitiesReadOnly.SysLog
                                     where sl.HostName == host &&
                                     sl.IpNr == clientIpNr
                                     orderby sl.Date descending
                                     select sl).Take(noOfRecords.Value).ToList();
                    }

                    #endregion
                    break;
            }

            if (showUnique)
                logEntrys = logEntrys.GetUnique();
            if (!showUnique)
                AddCompanyNameToSysLogs(logEntrys);

            return logEntrys;
        }

        public SysLog GetSysLog(int sysLogId)
        {
            using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
            SysLog sysLog = sysEntitiesReadOnly.SysLog.FirstOrDefault<SysLog>(s => s.SysLogId == sysLogId);
            if (sysLog != null)
            {
                if (string.IsNullOrEmpty(sysLog.LicenseNr) && !string.IsNullOrEmpty(sysLog.LicenseId) && Int32.TryParse(sysLog.LicenseId, out int licenseId))
                {
                    License license = LicenseManager.GetLicense(licenseId);
                    if (license != null)
                        sysLog.LicenseNr = license.LicenseNr;
                }
                if (string.IsNullOrEmpty(sysLog.CompanyName) && !string.IsNullOrEmpty(sysLog.ActorCompanyId) && Int32.TryParse(sysLog.ActorCompanyId, out int actorCompanyId))
                {
                    Company company = CompanyManager.GetCompany(actorCompanyId);
                    if (company != null)
                        sysLog.CompanyName = company.Name;
                }
                if (string.IsNullOrEmpty(sysLog.LoginName) && !string.IsNullOrEmpty(sysLog.UserId) && Int32.TryParse(sysLog.UserId, out int userId))
                {
                    User user = UserManager.GetUser(userId);
                    if (user != null)
                        sysLog.LoginName = user.LoginName;
                }
                if (string.IsNullOrEmpty(sysLog.RoleName) && !string.IsNullOrEmpty(sysLog.RoleId) && Int32.TryParse(sysLog.RoleId, out int roleId))
                {
                    Role role = RoleManager.GetRole(roleId);
                    if (role != null)
                        sysLog.RoleName = role.TermId.HasValue && role.TermId.Value != 0 ? GetText(role.TermId.Value, (int)TermGroup.Role) : role.Name;
                }
                if (sysLog.TaskWatchLogId.HasValue)
                {
                    TaskWatchLog taskWatchLog = base.GetTaskWatchLog(sysLog.TaskWatchLogId.Value);
                    if (taskWatchLog != null && sysLog.Date.Date == taskWatchLog.Start.Date) //check date until syscompdb is implemented to prevent reading logs from another db
                    {
                        sysLog.TaskWatchLogStart = taskWatchLog.Start.ToString("yyyy-MM-dd HH:mm:ss");
                        sysLog.TaskWatchLogStop = taskWatchLog.Stop.HasValue ? taskWatchLog.Stop.Value.ToString("yyyy-MM-dd HH:mm:ss") : string.Empty;
                        sysLog.TaskWatchLogName = taskWatchLog.Name;
                        sysLog.TaskWatchLogParameters = taskWatchLog.Parameters;
                    }
                }
            }

            return sysLog;
        }

        public SysLog GetLastLogEntry(int userId = 0, int actorCompanyId = 0, string hostInfo = "", string clientIp = "")
        {
            SysLog sysLog = null;

            //Let database get time to log the last error
            System.Threading.Thread.Sleep(1000);

            //Get only errors from the last 10 seconds
            DateTime dt = DateTime.Now;
            dt -= TimeSpan.Parse("00:00:10");

            if ((!String.IsNullOrEmpty(hostInfo)) && (!String.IsNullOrEmpty(clientIp)))
            {
                using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
                sysLog = (from sl in sysEntitiesReadOnly.SysLog
                          where sl.HostName == hostInfo &&
                          sl.IpNr == clientIp &&
                          sl.Date > dt
                          orderby sl.Date descending
                          select sl).FirstOrDefault<SysLog>();
            }
            else
            {
                if (userId <= 0)
                    userId = parameterObject != null && parameterObject.SoeUser != null ? base.UserId : 0;
                if (actorCompanyId <= 0)
                    userId = parameterObject != null && parameterObject.SoeCompany != null ? base.ActorCompanyId : 0;

                if (userId > 0 && actorCompanyId > 0)
                {
                    using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
                    sysLog = (from sl in sysEntitiesReadOnly.SysLog
                              where sl.UserId == userId.ToString() &&
                              sl.ActorCompanyId == actorCompanyId.ToString() &&
                              sl.Date > dt
                              orderby sl.Date descending
                              select sl).FirstOrDefault<SysLog>();
                }
            }

            return sysLog;
        }

        public void AddSysLog(Exception ex, Level level, ILog log = null, long? taskWatchLogId = null)
        {
            if (ex == null)
                return;

            string server = Environment.MachineName;                        //the server machine
            string message = ex.GetExceptionMessage();                      //the error message
            string source = GetExceptionSource(ex);                         //application or object that caused the exception
            string targetSite = GetExceptionTargetSite(ex);                 //the method that throws the current exception
            string lineNumber = GetLineNumber(ex);                          //the error linenumber
            string requestUri = parameterObject?.ExtendedUserParams?.Request;//the URL of the current request
            string referUri = null;                                         //the URL of the client's previous request that linked to the current URL
            string form = "";                                               //the form-variables
            string session = "";                                            //the session-state
            string application = "";                                        //the application-state
            string hostName = "";                                           //the IP-adress and hostname
            string ipNr = parameterObject?.ExtendedUserParams?.IpAddress;   //the IP-adress for client

            if (log != null && log.IsErrorEnabled)
            {
                #region Log4net logging

                SetLog4NetEnvironmentProperties(source, targetSite, lineNumber, requestUri, referUri, form, application, session, hostName, ipNr, taskWatchLogId);
                SetLog4NetUserProperties();
                SaveLog4netLog(server, message, ex, level);

                #endregion
            }
            else
            {
                #region Log4net not accesible (ex: WCF, Mobile, Scheduler, TimeSpot, Test)

                string thread = !String.IsNullOrEmpty(parameterObject?.Thread) ? parameterObject.Thread : "WCF"; //WCF is default

                SysLog sysLog = CreateSysLog(server, thread, message, ex, level, source, targetSite, lineNumber, requestUri, referUri, form, application, session, hostName, ipNr, taskWatchLogId: taskWatchLogId);

                if (sysLog != null)
                    SaveSysLog(sysLog);

                #endregion
            }

            ClearLog4netEnvironmentProperties();
        }

        public void AddSysLogInfoMessage(string server, string thread, string message, int? recordId = null, long? taskWatchLogId = null, SoeSysLogRecordType recordType = SoeSysLogRecordType.Unknown)
        {
            AddSysLogMessage(server, thread, message, null, Level.Info, recordId, taskWatchLogId, recordType);
        }

        public void AddSysLogWarningMessage(string server, string thread, string message, int? recordId = null, long? taskWatchLogId = null, SoeSysLogRecordType recordType = SoeSysLogRecordType.Unknown)
        {
            AddSysLogMessage(server, thread, message, null, Level.Warn, recordId, taskWatchLogId, recordType);
        }

        public void AddSysLogErrorMessage(string server, string thread, string message, int? recordId = null, long? taskWatchLogId = null, SoeSysLogRecordType recordType = SoeSysLogRecordType.Unknown)
        {
            AddSysLogMessage(server, thread, message, null, Level.Error, recordId, taskWatchLogId, recordType);
        }

        public void AddSysLogErrorMessage(string server, string thread, Exception ex, int? recordId = null, long? taskWatchLogId = null, SoeSysLogRecordType recordType = SoeSysLogRecordType.Unknown)
        {
            AddSysLogMessage(server, thread, null, ex, Level.Error, recordId, taskWatchLogId, recordType);
        }

        public void AddSysLogErrorMessage(string server, string thread, string message, string requestUri, long? taskWatchLogId = null)
        {
            SysLog sysLog = CreateSysLog(server, thread, message, null, Level.Error, requestUri: requestUri, taskWatchLogId: taskWatchLogId);
            if (sysLog != null)
                SaveSysLog(sysLog);
        }

        public void AddSysLogErrorMessage(string server, string thread, string message, Exception ex, string requestUri, long? taskWatchLogId = null)
        {
            SysLog sysLog = CreateSysLog(server, thread, message, ex, Level.Error, requestUri: requestUri, taskWatchLogId: taskWatchLogId);
            if (sysLog != null)
                SaveSysLog(sysLog);
        }

        public void AddSysLogMessage(string server, string thread, string message, Exception ex, Level level, int? recordId = null, long? taskWatchLogId = null, SoeSysLogRecordType recordType = SoeSysLogRecordType.Unknown)
        {
            SysLog sysLog = CreateSysLog(server, thread, message, ex, level, recordId: recordId, recordType: recordType, taskWatchLogId: taskWatchLogId);
            if (sysLog != null)
                SaveSysLog(sysLog);
        }

        #endregion

        #region Log4net

        public void SetLog4NetEnvironmentProperties(string source, string targetSite, string lineNumber, string requestUri, string referUri, string form, string application, string session, string hostName, string ipNr, long? taskWatchLogId)
        {
            //Set exception and environment information in log4net MDC
            ThreadContext.Properties["lineNumber"] = lineNumber;
            ThreadContext.Properties["source"] = source;
            ThreadContext.Properties["targetSite"] = targetSite;
            ThreadContext.Properties["requestUri"] = requestUri;
            ThreadContext.Properties["referUri"] = referUri;
            ThreadContext.Properties["form"] = form;
            ThreadContext.Properties["application"] = application;
            ThreadContext.Properties["session"] = session;
            ThreadContext.Properties["hostName"] = hostName;
            ThreadContext.Properties["ipNr"] = ipNr;
            //ThreadContext.Properties["taskWatchLogId"] = taskWatchLogId;
        }

        public void SetLog4NetUserProperties()
        {
            if (parameterObject != null)
            {
                ThreadContext.Properties["licenseId"] = parameterObject.LicenseId;
                ThreadContext.Properties["actorCompanyId"] = parameterObject.ActorCompanyId;
                ThreadContext.Properties["roleId"] = parameterObject.RoleId;
                ThreadContext.Properties["userId"] = parameterObject.UserId;
            }
        }

        public static void ClearLog4netProperties()
        {
            ClearLog4netEnvironmentProperties();
            ClearLog4netUserProperties();
        }

        public static void ClearLog4netEnvironmentProperties()
        {
            ThreadContext.Properties["lineNumber"] = DBNull.Value;
            ThreadContext.Properties["source"] = DBNull.Value;
            ThreadContext.Properties["targetSite"] = DBNull.Value;
            ThreadContext.Properties["requestUri"] = DBNull.Value;
            ThreadContext.Properties["referUri"] = DBNull.Value;
            ThreadContext.Properties["form"] = DBNull.Value;
            ThreadContext.Properties["application"] = DBNull.Value;
            ThreadContext.Properties["session"] = DBNull.Value;
            ThreadContext.Properties["hostName"] = DBNull.Value;
            ThreadContext.Properties["ipNr"] = DBNull.Value;
        }

        public static void ClearLog4netUserProperties()
        {
            ThreadContext.Properties["licenseId"] = DBNull.Value;
            ThreadContext.Properties["actorCompanyId"] = DBNull.Value;
            ThreadContext.Properties["roleId"] = DBNull.Value;
            ThreadContext.Properties["userId"] = DBNull.Value;
        }

        #endregion

        #region Help-methods

        private SysLog CreateSysLog(string server, string thread, string message, Exception ex, Level level, string source = "", string targetSite = null, string lineNumber = null, string requestUri = null, string referUri = null, string form = null, string application = null, string session = null, string hostName = null, string ipNr = null, int? recordId = null, long? taskWatchLogId = null, SoeSysLogRecordType recordType = SoeSysLogRecordType.Unknown)
        {
            string currentLineNumber = this.ToString() + ":" + GetLineNumber(new StackTrace(new StackFrame(true)));

            if (String.IsNullOrEmpty(message) && ex != null)
                message = ex.GetExceptionMessage();
            message = $"[{server}:{thread}] {message}";

            var exceptionMessage = ex?.ToString() ?? "";

            var correlationId = AppInsightUtil.GetCorrelationId() ?? "";
            if (correlationId.Length > 0)
                exceptionMessage += $"### [Server: {server}] [CorrelationId: {correlationId}]";

            return new SysLog()
            {
                Date = DateTime.Now,
                Level = level.ToString(),
                Thread = thread,
                Message = message,
                Exception = exceptionMessage,
                LogClass = currentLineNumber,
                Source = source,
                TargetSite = targetSite,
                LineNumber = lineNumber,
                RequestUri = requestUri != null && requestUri.Length > 255 ? requestUri.Left(255) : requestUri,
                ReferUri = referUri != null && referUri.Length > 255 ? referUri.Left(255) : referUri,
                Form = form,
                Application = application,
                Session = session,
                HostName = hostName,
                IpNr = ipNr,
                Logger = "",
                RecordId = recordId,
                RecordType = (int)recordType,
                LicenseId = parameterObject?.LicenseId.ToString(),
                ActorCompanyId = parameterObject?.ActorCompanyId.ToString(),
                RoleId = parameterObject?.RoleId.ToString(),
                UserId = parameterObject?.UserId.ToString(),
                TaskWatchLogId = taskWatchLogId,
            };
        }

        private void SaveSysLog(SysLog sysLog)
        {
            if (sysLog == null)
                return;

            using (SOESysEntities entities = new SOESysEntities())
            {
                using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_SUPPRESS, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                {
                    entities.SysLog.Add(sysLog);
                    SaveChanges(entities);
                }
            }
        }

        private void SaveLog4netLog(string server, string message, Exception ex, Level level)
        {
            message = $"--log4net-- [{server}] {message}";

            if (level == Level.Info && log.IsInfoEnabled)
                log.Info(message, ex);
            else if (level == Level.Warn && log.IsWarnEnabled)
                log.Warn(message, ex);
            else if (level == Level.Error && log.IsErrorEnabled)
                log.Error(message, ex);
        }

        private string GetExceptionSource(Exception ex)
        {
            return ex?.TargetSite?.ReflectedType?.FullName ?? "";
        }

        private string GetExceptionTargetSite(Exception ex)
        {
            return ex?.TargetSite?.ToString() ?? "";
        }

        private string GetLineNumber(Exception ex)
        {
            return ex != null ? GetLineNumber(new StackTrace(ex, true)) : "";
        }

        private string GetLineNumber(StackTrace trace)
        {
            if (trace == null || trace.FrameCount == 0)
                return string.Empty;

            StackFrame frame = trace.GetFrame(0);
            if (frame == null)
                return string.Empty;

            return frame.GetFileLineNumber() + ":" + frame.GetFileColumnNumber();
        }

        private void AddCompanyNameToSysLogs(List<SysLog> sysLogs)
        {
            if (sysLogs.IsNullOrEmpty())
                return;

            foreach (var sysLogsByCompany in sysLogs.Where(i => !String.IsNullOrEmpty(i.ActorCompanyId)).GroupBy(i => i.ActorCompanyId))
            {
                if (!Int32.TryParse(sysLogsByCompany.Key, out int actorCompanyId))
                    continue;

                Company company = CompanyManager.GetCompany(actorCompanyId);
                if (company == null)
                    continue;

                foreach (SysLog sysLog in sysLogsByCompany)
                {
                    sysLog.CompanyName = company.Name;
                }
            }
        }

        #endregion

        #region Static methods

        public static void ParseUserEnvironmentInfo(string environmentInfo, out string hostIp, out string hostName, out string clientIp)
        {
            hostIp = "";
            hostName = "";
            clientIp = "";

            if (!String.IsNullOrEmpty(environmentInfo))
            {
                var parts = environmentInfo.Split(Constants.SOE_ENVIRONMENT_CONFIGURATION_SEPARATOR);
                if (parts.Count() == 3)
                {
                    hostIp = parts[0];
                    hostName = parts[1];
                    clientIp = parts[2];
                }
            }
        }

        #endregion
    }
}
