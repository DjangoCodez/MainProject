using System;

namespace SoftOne.Soe.Web.UserControls
{
	public partial class SessionTimeout : ControlBase
	{
        protected void Page_Load(object sender, EventArgs e)
		{
            //Add scripts and style sheets
            PageBase.Scripts.Add("/UserControls/SessionTimeout.js");
		}
	}
}