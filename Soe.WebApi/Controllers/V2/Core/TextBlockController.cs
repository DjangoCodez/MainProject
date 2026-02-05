using Soe.WebApi.Controllers;
using Soe.WebApi.Models;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Data;
using System.Net;
using System.Web.Http;

namespace Soe.WebApi.V2.Core
{
    [RoutePrefix("V2/Core")]
    public class TextBlockDialogController : SoeApiController
    {
        #region Variables
        private readonly GeneralManager gm;
        #endregion

        #region Constructor
        public TextBlockDialogController(GeneralManager gm)
        {
            this.gm = gm;
        }
        #endregion

        #region TextBlock

        [HttpGet]
        [Route("TextBlock/{textBlockId:int}")]
        public IHttpActionResult GetTextBlock(int textBlockId)
        {
            return Content(HttpStatusCode.OK, gm.GetTextblock(textBlockId).ToDTO());
        }

        [HttpGet]
        [Route("TextBlocks/{entity:int}/{textBlockId:int?}")]
        public IHttpActionResult GetTextBlocks(int entity, int? textBlockId = null)
        {
            return Content(HttpStatusCode.OK, gm.GetTextblocks(entity, base.ActorCompanyId, textBlockId).ToDTOs());
        }

        [HttpPost]
        [Route("TextBlock")]
        public IHttpActionResult SaveTextBlock(TextBlockModel textBlockModel)
        {
            if (!ModelState.IsValid)
            {
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            }
            else
            {
                textBlockModel.TextBlock.ActorCompanyId = this.ActorCompanyId;
                return Content(HttpStatusCode.OK, gm.SaveTextblock(textBlockModel.TextBlock, textBlockModel.Entity, textBlockModel.translations));
            }
        }

        [HttpDelete]
        [Route("TextBlock/{textBlockId:int}")]
        public IHttpActionResult DeleteTextBlock(int textBlockId)
        {
            return Content(HttpStatusCode.OK, gm.DeleteTextblock(textBlockId)); 
        }

        #endregion
    }
}