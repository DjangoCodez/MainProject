using Microsoft.Owin;
using SoftOne.Soe.Business.Util;
using System;
using System.Threading.Tasks;
using System.Web;

namespace SoftOne.Soe.Business.Web.Middleware
{
    public class CorrelationIdMiddleware : OwinMiddleware
    {
        private const string CorrelationIdHeader = "X-Correlation-Id";
        private const string TraceParentHeader = "traceparent";

        public CorrelationIdMiddleware(OwinMiddleware next)
            : base(next)
        {
        }

        public override async Task Invoke(IOwinContext context)
        {
            var correlationId = ResolveCorrelationId(context);

            context.Set(AppInsightUtil.CorrelationIdItemKey, correlationId);

            if (HttpContext.Current != null)
                HttpContext.Current.Items[AppInsightUtil.CorrelationIdItemKey] = correlationId;

            context.Response.Headers.Set(CorrelationIdHeader, correlationId);

            await Next.Invoke(context);
        }

        private static string ResolveCorrelationId(IOwinContext context)
        {
            // 1. Prefer traceparent -> extract trace-id
            var traceParent = context?.Request?.Headers?.Get(TraceParentHeader);
            var traceId = ExtractTraceId(traceParent);
            if (!string.IsNullOrWhiteSpace(traceId))
                return traceId;

            // 2. Fallback: X-Correlation-Id (GUID or 32-hex)
            var headerValue = context?.Request?.Headers?.Get(CorrelationIdHeader);
            if (!string.IsNullOrWhiteSpace(headerValue) && IsValidCorrelationId(headerValue))
                return Normalize(headerValue);

            // 3. Generate new
            return Guid.NewGuid().ToString("N");
        }

        private static string ExtractTraceId(string traceParent)
        {
            // Expected: version-traceid-spanid-flags
            if (string.IsNullOrWhiteSpace(traceParent))
                return null;

            var parts = traceParent.Split('-');
            if (parts.Length != 4)
                return null;

            var traceId = parts[1];
            return IsHex32(traceId) ? traceId : null;
        }

        private static string Normalize(string value)
        {
            if (Guid.TryParse(value, out var g))
                return g.ToString("N");

            return value;
        }

        private static bool IsHex32(string value)
        {
            if (value == null || value.Length != 32)
                return false;

            for (int i = 0; i < value.Length; i++)
            {
                var ch = value[i];
                var isHex = (ch >= '0' && ch <= '9')
                            || (ch >= 'a' && ch <= 'f')
                            || (ch >= 'A' && ch <= 'F');
                if (!isHex)
                    return false;
            }
            return true;
        }

        private static bool IsValidCorrelationId(string value)
        {
            if (Guid.TryParse(value, out _))
                return true;

            if (value.Length != 32)
                return false;

            for (int i = 0; i < value.Length; i++)
            {
                var ch = value[i];
                var isHex = (ch >= '0' && ch <= '9')
                    || (ch >= 'a' && ch <= 'f')
                    || (ch >= 'A' && ch <= 'F');

                if (!isHex)
                    return false;
            }

            return true;
        }
    }
}
