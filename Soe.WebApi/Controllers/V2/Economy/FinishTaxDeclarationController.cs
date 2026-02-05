using Soe.WebApi.Controllers;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using System.Net;
using System.Web.Http;

namespace Soe.WebApi.V2.Economy
{
    [RoutePrefix("V2/Economy/Export/FinnishTax")]
    public class FinishTaxDeclarationController: SoeApiController
    {
        #region Variables
        private readonly FI_TaxDeclarationManager tdm;
        #endregion

        #region Constructor
        public FinishTaxDeclarationController(FI_TaxDeclarationManager tdm)
        {
            this.tdm = tdm;
        }
        #endregion

        #region Export
        [HttpPost]
        [Route("ExportVat")]
        public IHttpActionResult ExportVatFile(FinnishTaxExportDTO exportDTO)
        {
            return Content(HttpStatusCode.OK, this.tdm.Export(exportDTO, base.ActorCompanyId));
        }
        #endregion
    }
}