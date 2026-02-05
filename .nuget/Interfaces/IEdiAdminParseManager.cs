using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftOne.EdiAdmin.Business.Interfaces
{
    public interface IEdiAdminParseManager : IEdiAdminManager
    {
        ActionResult ParseMessages();
    }
}
