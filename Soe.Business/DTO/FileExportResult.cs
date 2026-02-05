using Bridge.Shared.Util;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.DTO
{
    public class FileExportResult
    {
        public ActionResult Result { get; set; }
        public string Base64Data { get; set; }
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public byte[] Data { get; set; }

        public string GetFullFilePath(string address)
        {
            if (!string.IsNullOrEmpty(address))
            {

                if (!address.EndsWith("/"))
                    address += "/";

                if (!string.IsNullOrEmpty(FilePath) && !FilePath.EndsWith("/"))
                    FilePath += "/";

                return address + FilePath + FileName;
            }

            return string.Empty;
        }
    }

    public static class FileExportResultExtensions
    {
        public static FileExportResult Merge(this List<FileExportResult> fileExportResults)
        {
            if (fileExportResults.Count <= 1)
                return fileExportResults.FirstOrDefault();

            List<byte[]> arrays = fileExportResults.Where(w => !string.IsNullOrEmpty(w.Base64Data)).Select(s => Base64Util.GetDataFromBase64String(s.Base64Data)).ToList();
            if (arrays.Any())
            {
                var mergeData = arrays.SelectMany(x => x).ToArray();
                fileExportResults.First().Base64Data = Base64Util.GetBase64StringFromData(mergeData);
                return fileExportResults.First();
            }
            return null;
        }
    }
}
