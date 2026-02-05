using System;
using SoftOne.Soe.Common.Util;

namespace SoftOne.Soe.Web.soe.time.import.excelimport
{
    public partial class _default : PageBase
    {
        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Time_Import_ExcelImport;

            //Add parameters
            Context.Items["Feature"] = this.Feature;

            Server.Transfer("/soe/common/excelimport/default.aspx");
        }
    }
}