using SoftOne.Soe.Business.Util.SupplierAgreement;
using SoftOne.Soe.Common.Util;
using System.IO;

namespace SoftOne.Soe.Business.Interfaces
{
    public interface ISupplierAgreement
    {
        /// <summary>
        /// Read a file stream to object format
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        ActionResult Read(Stream stream);

        /// <summary>
        /// Converts to general format
        /// </summary>
        /// <returns></returns>
        GenericProvider ToGeneric();

        SoeSupplierAgreementProvider Provider { get; }
    }
}
