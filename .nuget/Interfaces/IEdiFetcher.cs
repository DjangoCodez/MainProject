using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SoftOne.EdiAdmin.Business.Interfaces
{
    public delegate void OnFileFetchedDelegate(string fileName, string fullPath, byte[] data);
    public interface IEdiFetcher
    {
        ActionResult GetContent(string source, OnFileFetchedDelegate successCallback);
        ActionResult DeleteFile(string fullPath);
    }
}
