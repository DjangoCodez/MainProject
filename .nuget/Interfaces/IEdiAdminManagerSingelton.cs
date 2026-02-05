using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftOne.EdiAdmin.Business.Interfaces
{
    public interface IEdiAdminManagerSingelton : IEdiAdminManager
    {
        void StartWatch(int sysScheduledJobId);
    }
}
