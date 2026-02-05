using System;
using System.Collections.Generic;
using System.Web.UI;

namespace SoftOne.Soe.Web
{
    public abstract class MasterPageBase : MasterPage
    {
        private PageBase pageBase = null;

        public PageBase PageBase
        {
            get
            {
                if (pageBase == null)
                {
                    pageBase = (PageBase)this.Page;
                }
                return pageBase;
            }
        }
    }
}