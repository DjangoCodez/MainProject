using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using SoftOne.Soe.Data;

namespace SoftOne.Soe.Business.Util
{
    public class RequestScopeHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var ctx = HttpContext.Current;
            string path = ctx?.Request?.Path;
            SysEntitiesProvider.CreateOnBeginRequest();
            CompEntitiesProvider.CreateOnBeginRequest();

            try
            {
                return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                // Dispose per-request contexts
                try
                {
                    SysEntitiesProvider.DisposeOnRequestEnd();
                    CompEntitiesProvider.DisposeOnRequestEnd();
                }
                catch (Exception ex)
                {
                    LogCollector.LogCollector.LogError(ex, "Error disposing request-scoped entities");
                }
            }
        }
    }
}
