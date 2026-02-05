using SoftOne.EdiAdmin.Business.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftOne.EdiAdmin.Business.Interfaces
{
    public interface IEdiSenderOld : IEdiSender
    {
        void SetInputParams(EdiSenderInputParams inputParams);
    }
}
