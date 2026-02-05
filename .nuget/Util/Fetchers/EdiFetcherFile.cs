using SoftOne.EdiAdmin.Business.Interfaces;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftOne.EdiAdmin.Business.Util
{
    public class EdiFetcherFile : Interfaces.IEdiFetcher
    {        
        public ActionResult GetContent(string source, OnFileFetchedDelegate onFileFetched)
        {
            ActionResult result = new ActionResult();
            try
            {
                if (!File.Exists(source))
                    throw new NullReferenceException();

                //Set information to service job
                var data = File.ReadAllBytes(source);
                string fileName = source.Split('/', '\\').LastOrDefault();
                onFileFetched(fileName, source, data);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                result.Success = false;
                result.Exception = ex;
            }

            return result;
        }

        public ActionResult DeleteFile(string fullPath)
        {
            ActionResult result = new ActionResult();

            File.Delete(fullPath);

            return result;
        }
    }
}
