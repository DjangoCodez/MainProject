using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftOne.EdiAdmin.Business.Interfaces
{
    public delegate ActionResult OnMessageParsedDelegate(SOECompEntities entities, EdiTransfer ediTransfer, SysWholesellerEdi sysWholesellerEdi);
    public interface IMessageParser
    {
        event OnMessageParsedDelegate OnMessageParsed;

        ActionResult ParseMessageFromWholeseller(IEdiFetcher fetcher, string source, int sysWholesellerEdiId, int actorCompanyId, params string[] ignoreFilesList);
        ActionResult ParseMessageToEdiTransfer(Soe.Data.SOECompEntities entities, Soe.Data.EdiReceivedMsg item, Soe.Data.SysWholesellerEdi wholeseller);
    }
}
