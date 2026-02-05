using System;
using System.IO;
using System.Web;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Business.Util;

namespace SoftOne.Soe.Web.soe.economy.export.sie.download
{
	public partial class _default : PageBase
	{
		private DownloadSieItem downloadSieItem;

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.None;
            base.Page_Init(sender, e);
        }

		protected void Page_Load(object sender, EventArgs e)
		{
			downloadSieItem = (DownloadSieItem)Session[Constants.SESSION_DOWNLOAD_SIE_ITEM];

			if (downloadSieItem != null)
			{
				FileStream fs = null;

				try
				{
					//Read file
					fs = File.OpenRead(downloadSieItem.FilePathOnServer);
					byte[] data = new byte[fs.Length];
					fs.Read(data, 0, data.Length);

					HttpContext.Current.Response.ClearContent();
					HttpContext.Current.Response.ClearHeaders();
					HttpContext.Current.Response.ContentType = "text/plain";
					HttpContext.Current.Response.AddHeader("Content-Disposition", "Attachment; Filename=" + downloadSieItem.FileNameOnClient);
					HttpContext.Current.Response.BinaryWrite(data);

                    try
                    {
                        HttpContext.Current.Response.End(); //Causes ThreadAbortException exception
                    }
                    catch (Exception ex)
                    {
                        ex.ToString();
                    }

                    HttpContext.Current.ApplicationInstance.CompleteRequest();					
				}
				catch (Exception ex)
				{
                    ex.ToString(); //prevent compiler warning
				}
				finally
				{
					Session[Constants.SESSION_DOWNLOAD_SIE_ITEM] = null;

					if (fs != null)
					{
						fs.Flush();
						fs.Close();
						fs.Dispose();
					}

					//Cannot delete file because it could be deleted before the user have downloaded it
					//File.Delete(downloadSieItem.FilePathOnServer);
				}
			}
		}
	}
}
