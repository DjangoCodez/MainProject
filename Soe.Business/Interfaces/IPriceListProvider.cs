using SoftOne.Soe.Business.Util.PricelistProvider;
using SoftOne.Soe.Common.Util;
using System.IO;

namespace SoftOne.Soe.Business.Interfaces
{
    public interface IPriceListProvider
    {
        /// <summary>
        /// Read a file stream to object format
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        ActionResult Read(Stream stream, string fileName=null);

        /// <summary>
        /// Converts to general format
        /// </summary>
        /// <returns></returns>
        GenericProvider ToGeneric();
        
    }
}
