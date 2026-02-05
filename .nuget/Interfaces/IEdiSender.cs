using SoftOne.EdiAdmin.Business.Util;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftOne.EdiAdmin.Business.Interfaces
{
    public interface IEdiSender
    {
        IEnumerable<string> ToXmls();
        bool ConvertMessage(string content);
    }
}
