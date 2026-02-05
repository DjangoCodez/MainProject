using Newtonsoft.Json;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Core.SoftOneId;
using SoftOne.Soe.Common.DTO;
using System;

namespace SoftOne.Soe.Web
{
    public partial class SoftOneStatus : PageBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string guidstring = QS["guid"];
            string superKey = QS["key"];
            string report = QS["report"];
            Response.Clear();

            if (!String.IsNullOrEmpty(guidstring) && !String.IsNullOrEmpty(superKey))
            {
                Guid guid = Guid.NewGuid();

                if (Guid.TryParse(guidstring, out guid))
                {
                    StatusManager statusManager = new StatusManager();
                    SoftOneStatusDTO softOneStatusDTO = new SoftOneStatusDTO();

                    if (SoftOneIdConnector.ValidateSuperKey(guid, superKey))
                    {
                        if (string.IsNullOrEmpty(report))
                            softOneStatusDTO = statusManager.GetSoftOneStatusDTO(ServiceType.Webforms);
                        else
                            softOneStatusDTO = statusManager.GetPrintSoftOneStatusDTO(ServiceType.ReportFromWeb);

                        if (softOneStatusDTO != null)
                        {
                            string json = JsonConvert.SerializeObject(softOneStatusDTO);
                            Response.ContentType = "application/json; charset=utf-8";
                            Response.Write(json);
                            Response.End();
                        }
                    }
                }
            }
        }
    }
}
