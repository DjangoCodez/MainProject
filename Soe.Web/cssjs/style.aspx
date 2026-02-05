<%@ Page Language="C#" AutoEventWireup="true" %>
<%@ Import Namespace="System.IO"%>

<script runat="server">
	protected void Page_Load(object sender, EventArgs e)
	{
        bool releaseMode = SoftOne.Soe.Common.Util.StringUtility.GetBool(SoftOne.Soe.Web.Util.WebConfigUtility.GetSoeConfigurationSetting(SoftOne.Soe.Common.Util.Constants.SOE_CONFIGURATION_SETTING_RELEASEMODE));
        if (releaseMode)
        {
            Response.Cache.SetExpires(DateTime.Now.AddDays(365));
            Response.Cache.SetCacheability(HttpCacheability.Public);
            Response.Cache.VaryByParams["*"] = true;
        }
        
        Response.ContentType = "text/css";
    }

	private void ParseDir(string path)
	{
		foreach (string dirName in Directory.GetDirectories(path))
		{
			ParseDir(dirName);
		}		
		foreach (String fileName in Directory.GetFiles(path))
		{
            if (Path.GetExtension(fileName) == ".css" && Path.GetFileName(fileName) != "ie.css" && (Path.GetFileName(fileName) != "zie.css" || Request.Browser.Browser == "IE"))
			{
				Response.Write(File.ReadAllText(fileName));
			}
		}
	}
</script>

* { margin: 0; padding: 0; }

<%ParseDir(Server.MapPath("merge"));%>

<% %>
