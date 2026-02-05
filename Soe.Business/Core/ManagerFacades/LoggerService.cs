using System;
using System.Threading.Tasks;
using log4net;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;

namespace SoftOne.Soe.Business.Core.ManagerFacades
{
    internal class LoggerService : ManagerBase, ILoggerService
    {
        public LoggerService(ParameterObject managerContext) : base(managerContext) { }
        public new ActionResult LogPersonalData(object entity, TermGroup_PersonalDataActionType actionType, string url = "") => base.LogPersonalData(entity, actionType, url);
        public new Task LogDTO(object entity, TermGroup_PersonalDataActionType actionType, string url = "") => base.LogDTO(entity, actionType, url);
        public new void LogInfo(Exception ex, ILog log, long? taskWatchLogId = null) => base.LogInfo(ex, log, taskWatchLogId);
        public new void LogInfo(string message, long? taskWatchLogId = null) => base.LogInfo(message, taskWatchLogId);
        public new void LogWarning(Exception ex, ILog log, long? taskWatchLogId = null) => base.LogWarning(ex, log, taskWatchLogId);
        public new void LogWarning(string message, long? taskWatchLogId = null) => base.LogWarning(message, taskWatchLogId);
        public new void LogError(string message, long? taskWatchLogId = null) => base.LogError(message, taskWatchLogId);
        public new void LogError(Exception ex, ILog log, long? taskWatchLogId = null) => base.LogError(ex, log, taskWatchLogId);
        public new void DebugWrite(string msg) => base.DebugWrite(msg);
        public new void LogTransactionFailed(string source, ILog log) => base.LogTransactionFailed(source, log);
    }
}
