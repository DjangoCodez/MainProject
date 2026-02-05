using SoftOne.Soe.Business.Interfaces;
using SoftOne.Soe.Common.Util;
using System.IO;

namespace SoftOne.Soe.Business.Util
{
    public class ValidationItem
    {
        public TextEntryValidation Validation { get; set; }
        public int InvalidAlertTermID { get; set; }
        public string InvalidAlertDefaultTerm { get; set; }
    }

    public static class ValidationUtils
    {
        public static ActionResult ValidateFile<T>(Stream stream, ref T provider)
        {
            if (provider == null || stream == null)
                return new ActionResult(false, (int)ActionResultSelect.EntityIsNull, "provider");

            var result = new ActionResult();
            if (provider is IFileValidator<T>)
            {
                result.Success = false;
                while (!result.Success)
                {
                    var validator = provider as IFileValidator<T>;
                    if (validator == null)
                    {
                        result.Success = true;
                        break;
                    }

                    // Copy stream to not affect the first stream
                    Stream tmpStream = new MemoryStream();
                    stream.CopyTo(tmpStream);
                    stream.Position = tmpStream.Position = 0;
                    result = validator.ValidateFile(tmpStream);
                    if (!result.Success)
                    {
                        provider = validator.GetSecondaryProvider();
                        if (provider == null)
                            return new ActionResult(null, "Error. The file did not validate correctly. " + result.ErrorMessage ?? string.Empty);
                    }
                }
            }

            return new ActionResult();
        }
    }
}
