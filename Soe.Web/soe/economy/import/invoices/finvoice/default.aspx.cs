using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Business.Util.LogCollector;
using SoftOne.Soe.Common.Util;
using System;
using System.IO;

namespace SoftOne.Soe.Web.soe.economy.import.invoices.finvoice
{
    public partial class _default : PageBase
    {
        #region Variables
        
        protected AccountManager am = null;
        protected int accountYearId;

        //Module specifics       
        protected SoeModule TargetSoeModule = SoeModule.Economy;

        #endregion

        protected override void Page_Init(object sender, EventArgs e)
        {            
            this.Feature = Feature.Economy_Import_Invoices_Finvoice;            
            base.Page_Init(sender, e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            am = new AccountManager(ParameterObject);
            am.GetAccountYearInfo(CurrentAccountYear, out accountYearId, out _);
        }

        #region Action-methods

        private string SaveFileToServer(Stream stream, string fileName)
        {
            var pathOnServer = ConfigSettings.SOE_SERVER_DIR_TEMP_FINVOICE_PHYSICAL + fileName;
            RemoveFileFromServer(pathOnServer);

            var file = new FileStream(pathOnServer, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.Read);

            MemoryStream ms = null;
            try
            {
                //stream not seekable convert to memory stream
                ms = new MemoryStream();
                byte[] buffer = new byte[2048];
                int bytesRead = 0;
                do
                {
                    bytesRead = stream.Read(buffer, 0, buffer.Length);
                    ms.Write(buffer, 0, bytesRead);
                } while (bytesRead != 0);
                stream.Close();

                //reset stream position
                ms.Position = 0;

                //convert to byte[]
                byte[] result = new byte[(int)ms.Length];
                ms.Read(result, 0, (int)ms.Length);
                if (DefenderUtil.IsVirus(pathOnServer))
                    LogCollector.LogError($"Finvoice SaveFileToServer Virus detected {pathOnServer}");
                else
                    file.Write(result, 0, result.Length);
            }
            finally
            {
                ms.Close();
                ms.Dispose();
                file.Close();
            }
            return pathOnServer;
        }

        private void RemoveFileFromServer(string pathOnServer)
        {
            if (System.IO.File.Exists(pathOnServer))
                System.IO.File.Delete(pathOnServer);
        }

        #endregion
    }
}
