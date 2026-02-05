using SoftOne.Soe.Business.Core;
using System;

namespace SoftOne.Soe.Web.ajax
{
    public partial class getBaseParameters : JsonBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            int featureId = 0;

            if (!string.IsNullOrEmpty(QS["f"]))
                featureId = GetFeatureId(); 
    
            Result result = new Result();

            if (SoeCompany != null)
                result.CompanyId = SoeCompany.ActorCompanyId;

            if (SoeUser != null)
            {
                result.UserId = UserId;
                result.RoleId = RoleId;
            }
            
            result.FeatureId = featureId;
            ResponseObject = result;
        }

        private int GetFeatureId()
        {
            var path = "~" + QS["f"];
            if (path.Substring(path.Length - 1, 1) == @"/")
                path += "default.aspx";
            return FeatureManager.GetFeatureIdFromPath(path);
        }

        private class Result
        {
            public int CompanyId { get; set; }
            public int UserId { get; set; }
            public int RoleId { get; set; }
            public int FeatureId { get; set; }
        }
    }
}
