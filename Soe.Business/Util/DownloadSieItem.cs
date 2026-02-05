
namespace SoftOne.Soe.Business.Util
{
	public class DownloadSieItem
	{
		/// <summary>The name of the file to download to client. Set by user in GUI</summary>
		public string FileNameOnClient { get; set; }
		/// <summary>The filepath to the file on the server</summary>
		public string FilePathOnServer { get; set; }
	}
}
