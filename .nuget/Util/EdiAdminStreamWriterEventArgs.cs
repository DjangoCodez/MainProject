using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftOne.EdiAdmin.Business.Util
{
    public class EdiAdminStreamWriterEventArgs : EventArgs
    {
        public EdiAdminStreamWriterEventArgs(EventLogEntryType entryType, string message)
        {
            this.EntryType = entryType;
            this.Message = message;
        }

        public EventLogEntryType EntryType { get; set; }
        public string Message { get; set; }
    }
}
