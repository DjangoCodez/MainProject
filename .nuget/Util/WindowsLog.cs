using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftOne.EdiAdmin.Business.Util
{
    public class WindowsLog
    {
        #region Singelton
        private static WindowsLog instance;
        public static WindowsLog Instance
        {
            get
            {
                return instance;
            }
        }

        static WindowsLog()
        {
            instance = new WindowsLog();
        }
        #endregion

        private EventLog log;
        private const string LOGNAME = "EdiAdmin.Business";

        public EventLog Log
        {
            get
            {
                return this.log;
            }
        }

        private WindowsLog()
        {
            // Create event log source
            if (!EventLog.SourceExists(LOGNAME))
                EventLog.CreateEventSource(LOGNAME, "Application");
            // Create event log file
            log = new EventLog();
            log.Source = LOGNAME;
            log.Log = "Application";
        }

        public bool WriteEntry(string msg, EventLogEntryType entryType = EventLogEntryType.Warning)
        {
            this.log.WriteEntry(msg, entryType);
            return true;
        }
    }
}
