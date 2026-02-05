using Microsoft.Owin;
using SoftOne.Soe.Business.Util.LogCollector;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;

namespace SoftOne.Soe.Business.Security
{
    public static class TimeOutOwinHelper
    {
        private const double SessionTimeoutHours = 2;
        public const string TimeoutCookieName = "_SOLA";

        public static string CookieSecret { get; set; }

        public static bool TryToSlideTimeoutForward(IOwinContext context)
        {
            if (IsExcludedRoute(context))
                return true; // return all is well but do nothing.

            try
            {
                if (IsUserAuthenticated(context))
                {
                    return HandleAuthenticatedUser(context);
                }
                return HandleUnauthenticatedUser(context);
            }
            catch (Exception ex)
            {
                LogCollector.LogError(ex, "TryToSlideTimeoutForward");
                return false;
            }
        }

        private static bool IsExcludedRoute(IOwinContext context)
        {
            if (context?.Request != null)
            {
                try
                {
                    // Skip updating for certain routes
                    string path = context.Request.Path.Value.ToLower();
                    string[] excludedRoutes = { "core/document/newsince",
                                                "core/document/company/unreadcount",
                                                "core/information/newsince",
                                                "core/information/unreadcount",
                                                "core/information/unread/severe",
                                                "core/xemail/nbrofunreadmessages" };

                    if (Array.Exists(excludedRoutes, route => path.Contains(route)))
                        return true;
                }

                catch (Exception ex)
                {
                    LogCollector.LogError(ex, "IsExcludedRoute");
                    return true;
                }
            }
            return false;
        }

        private static bool IsUserAuthenticated(IOwinContext context)
        {
            return context.Authentication.User?.Identity?.IsAuthenticated ?? false;
        }

        private static bool HandleAuthenticatedUser(IOwinContext context)
        {

            if (!IsValidLastActivityCookie(context, out DateTime lastActivity))
                return false;

            if (IsSessionExpired(lastActivity))
                return false;

            UpdateLastActivityCookie(context);
            return true;
        }

        private static bool HandleUnauthenticatedUser(IOwinContext context)
        {
            UpdateLastActivityCookie(context);
            return true;
        }

        private static bool IsValidLastActivityCookie(IOwinContext context, out DateTime lastActivity)
        {
            string lastActivityCookie = context.Request.Cookies[TimeoutCookieName];
            if (string.IsNullOrEmpty(lastActivityCookie))
            {
                //User is logged in, but do now have any cookie. create a new one.
                UpdateLastActivityCookie(context);
                lastActivity = DateTime.UtcNow;
                return true;
            }

            if (!TryParseAndVerifyCookie(lastActivityCookie, out lastActivity))
            {
                UpdateLastActivityCookie(context);
                return false;
            }

            return true;
        }

        private static bool TryParseAndVerifyCookie(string cookie, out DateTime lastActivity)
        {
            string[] parts = cookie.Split('.');
            if (parts.Length != 2)
            {
                lastActivity = DateTime.MinValue;
                return false;
            }
            string plainTextPart = parts[0];

            if (!CookieSecurity.VerifyAndGetValue(cookie, out string lastActivityValue) ||
                !DateTime.TryParse(lastActivityValue, out lastActivity) ||
                lastActivityValue != plainTextPart)
            {
                lastActivity = DateTime.MinValue;
                return false;
            }

            return true;
        }

        private static bool IsSessionExpired(DateTime lastActivity)
        {
            return DateTime.UtcNow.Subtract(lastActivity).TotalHours > SessionTimeoutHours;
        }

        private static void UpdateLastActivityCookie(IOwinContext context)
        {
            var cookieOptions = new Microsoft.Owin.CookieOptions
            {
                HttpOnly = true,
                Secure = context.Request.IsSecure,  // Set to true if you are sure that you are running over HTTPS
                Expires = DateTime.UtcNow.AddHours(SessionTimeoutHours)
            };

            string currentTime = DateTime.UtcNow.ToString("o").Split('.')[0];

            // Sign the cookie value before storing it
            string signedCookieValue = CookieSecurity.SignCookie(currentTime);

            context.Response.Cookies.Append(TimeoutCookieName, signedCookieValue, cookieOptions);
        }
    }

    public static class CookieSecurity
    {
        public static readonly string secretKey = TimeOutOwinHelper.CookieSecret ?? "78myc8974yv5783my45cm9TGHHJKJ#¤%&/()=4u";  // Store this securely

        public static string SignCookie(string value)
        {
            // Log if secret key is null and add stacktrace to log
            if (string.IsNullOrWhiteSpace(value))
                LogCollector.LogError($"Cookie value is null or empty {Environment.StackTrace}");

            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey)))
            {
                var signature = hmac.ComputeHash(Encoding.UTF8.GetBytes(value));
                var signatureString = Convert.ToBase64String(signature);
                return $"{value}.{signatureString}";
            }
        }

        public static bool VerifyAndGetValue(string signedValue, out string value)
        {
            var parts = signedValue.Split('.');
            if (parts.Length != 2)
            {
                value = null;
                return false;
            }

            var originalValue = parts[0];
            var providedSignature = parts[1];

            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey)))
            {
                var signature = hmac.ComputeHash(Encoding.UTF8.GetBytes(originalValue));
                var signatureString = Convert.ToBase64String(signature);

                if (providedSignature.Equals(signatureString))
                {
                    value = originalValue;
                    return true;
                }
                else
                {
                    value = null;
                    return false;
                }
            }
        }
    }
}
