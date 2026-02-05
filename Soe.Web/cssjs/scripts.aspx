<%@ Page Language="C#" AutoEventWireup="true" %>
<%@ Import Namespace="System.IO"%>
<%@ Import Namespace="System.Threading"%>
<%@ Import Namespace="System.Globalization"%>
<%@ Import Namespace="JavaScriptSupport"%>

<script runat="server">
    protected string decimalSeparator;
	protected CultureInfo currentCulture;
    
    protected void Page_Load(object sender, EventArgs e)
    {
        bool releaseMode = SoftOne.Soe.Common.Util.StringUtility.GetBool(SoftOne.Soe.Web.Util.WebConfigUtility.GetSoeConfigurationSetting(SoftOne.Soe.Common.Util.Constants.SOE_CONFIGURATION_SETTING_RELEASEMODE));
        if (releaseMode)
        {
            Response.Cache.SetExpires(DateTime.Now.AddDays(365));
            Response.Cache.SetCacheability(HttpCacheability.Public);
            Response.Cache.VaryByParams["*"] = true;
        }

        Response.ContentType = "text/javascript";
        
        //currentCulture = Thread.CurrentThread.CurrentCulture;
        // Hardcoded to Swedish to get around bug in javascript
        // when converting decimals between database and GUI
        //currentCulture = CultureInfo.CreateSpecificCulture("sv-SE");
		//decimalSeparator = currentCulture.NumberFormat.NumberDecimalSeparator;
        
        // 23.2.2015 Hardcoding like above won't work in Finland as it screws the calendar's language so 
        // switching back to Current culture. If decimal separator gives problems, hardcode it rather to "," instead. 
        currentCulture = Thread.CurrentThread.CurrentCulture;
		decimalSeparator = currentCulture.NumberFormat.NumberDecimalSeparator;
        
    }

    private void ParseDir(string path)
    {
		foreach (string dirName in Directory.GetDirectories(path))
        {
            //if (!dirName.Contains("x")) //potential source of trouble, whole local path can't contain x
				ParseDir(dirName);
        }   
        foreach (string fileName in Directory.GetFiles(path))
        {
            if (Path.GetExtension(fileName) == ".js")
            {
				if (Request.QueryString.Count == 0)
					JavaScriptMinifier.Minify(File.OpenRead(fileName), Response.OutputStream);
				else
					Response.Write(File.ReadAllText(fileName));
            }
        }        
    }
</script>

//<%=DateTime.Now%>

var SOE = {
	decimalSeparator: '<%=decimalSeparator%>',
	monthNames: [ '<%=String.Join("', '", currentCulture.DateTimeFormat.AbbreviatedMonthNames)%>' ],
	weekDayNames: [ '<%=String.Join("', '", currentCulture.DateTimeFormat.AbbreviatedDayNames)%>' ],
	imgGarbage: '/img/delete.png',
	_uniqueIdCounter: 0,
	
	getID: function(el) {
		if (!el.id)
			el.id = 'uniqueIdCounter__' + SOE._uniqueIdCounter++;
		return el.id;
	},
	
	parseFloat: function(source, decimals) {
        // If comma exists, replace with dot
	    var fl = (String(source).indexOf(SOE.decimalSeparator) == -1) ? source : source.replace(SOE.decimalSeparator, '.');
		// Convert to numeric with a fixed number of decimals
		fl = Number(Number(fl).toFixed(decimals));
		return fl;
	},
	
	floatToString: function(source, decimals) {
		// Convert to numeric with a fixed number of decimals
		var fl = Number(source).toFixed(decimals);
		// Replace dot with comma
		return fl.replace('.', SOE.decimalSeparator);
	},
	
	isNumeric: function(source) {
		var validChars = '0123456789';
		for (i = 0; i < source.length; i++) {
			if (validChars.indexOf(source.charAt(i)) == -1)
				return false; 
		}
		return true;
	}
}

function disableEnterKey(e)
{
     var key;     
     if(window.event)
          key = window.event.keyCode; //IE
     else
          key = e.which; //firefox     

     return (key != 13);
}

var win = $(window);
var doc = $(document);

<%--
This break Angular SPA routing. Not sure why it would be needed, maybe some Silverlight thingy. Sorry if I broke something...
window.onpopstate = function(e){
    if (history.state != null)
        document.location.href = history.state;
};
--%>

function browserSupportsHistory() {
    if (typeof history.pushState === 'undefined') { 
        return false; 
    } 
    else { 
        return true;
    }
}


<%ParseDir(Server.MapPath("merge"));%>
