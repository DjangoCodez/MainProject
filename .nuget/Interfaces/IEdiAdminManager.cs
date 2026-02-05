using SoftOne.EdiAdmin.Business.Util;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftOne.EdiAdmin.Business.Interfaces
{
    public interface IEdiAdminManager
    {
        event EventHandler<EdiAdminStreamWriterEventArgs> OnOutputRecived;
        void Setup(bool redirectOutPut, int? sysScheduledJobId = null, int? batchNr = null);
        string GetStatusMessage();
    }
}
