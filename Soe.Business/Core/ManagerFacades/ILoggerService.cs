using System;
using System.Threading.Tasks;
using log4net;
using SoftOne.Soe.Common.Util;

namespace SoftOne.Soe.Business.Core.ManagerFacades
{
    public interface ILoggerService
    {
        ActionResult LogPersonalData(object entity, TermGroup_PersonalDataActionType actionType, string url = "");
        Task LogDTO(object entity, TermGroup_PersonalDataActionType actionType, string url = "");
        void LogInfo(Exception ex, ILog log, long? taskWatchLogId = null);
        void LogInfo(string message, long? taskWatchLogId = null);
        void LogWarning(Exception ex, ILog log, long? taskWatchLogId = null);
        void LogWarning(string message, long? taskWatchLogId = null);
        void LogError(string message, long? taskWatchLogId = null);
        void LogError(Exception ex, ILog log, long? taskWatchLogId = null);
        void DebugWrite(string msg);
        void LogTransactionFailed(string source, ILog log);
    }
}
