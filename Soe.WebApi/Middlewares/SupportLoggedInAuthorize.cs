using Soe.WebApi.Controllers;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;

public class SupportUserAuthorizeAttribute : AuthorizeAttribute
{
    protected override bool IsAuthorized(HttpActionContext actionContext)
    {
        var controller = actionContext.ControllerContext.Controller as SoeApiController;
        if (controller == null)
            return false;

        return controller.ParameterObject != null && (controller.ParameterObject.IsSupportLoggedIn || controller.ParameterObject.SoeCompany.LicenseSupport);
    }

    protected override void HandleUnauthorizedRequest(HttpActionContext actionContext)
    {
        // Return 403 instead of 401
        actionContext.Response = actionContext.Request
            .CreateErrorResponse(HttpStatusCode.Forbidden, "Forbidden");
    }
}
