using SoftOne.Soe.Common.Util;
using System.IO;

namespace SoftOne.Soe.Business.Interfaces
{
    public interface IFileValidator<T>
    {
        ActionResult ValidateFile(Stream stream);
        T GetSecondaryProvider();
    }
}
