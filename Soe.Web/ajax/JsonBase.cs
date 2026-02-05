using System;
using Newtonsoft.Json;

namespace SoftOne.Soe.Web.ajax
{
    public class JsonBase : PageBase
    {
        public object ResponseObject { get; set; }

        private PageBase pageBase = null;
        public PageBase PageBase
        {
            get
            {
                if (pageBase == null)
                    pageBase = Page as PageBase;
                if (pageBase == null)
                    pageBase = (PageBase)System.Web.HttpContext.Current.Handler;
                return pageBase;
            }
        }

        protected override void Page_Init(object sender, EventArgs e)
        {
            Response.ContentType = "application/json";
        }

        protected override void OnPreRender(EventArgs e)
        {
            if (ResponseObject != null)
                Response.Write(JsonConvert.SerializeObject(ResponseObject));
        }
    }
}
