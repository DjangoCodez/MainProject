using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Web;

namespace SoftOne.Soe.Util
{
    public static class UrlUtil
    {
        public static Dictionary<string, string> GetQS(string url)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();

            if (!String.IsNullOrEmpty(url))
            {
                string[] parts = url.Split('&');
                foreach (string qs in parts)
                {
                    int idx = qs.IndexOf('=');
                    if (qs.Length <= idx)
                        continue;

                    string key = qs.Substring(0, idx);
                    string value = qs.Substring(idx + 1);
                    if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
                    {
                        if (!dict.ContainsKey(key))
                            dict.Add(key, value);
                    }
                }
            }

            return dict;
        }

        public static string NameValueCollectionToString(NameValueCollection values)
        {
            string[] parts = new string[values.Count];
            string[] keys = values.AllKeys;
            for (int i = 0; i < keys.Length; i++)
            {
                string key = keys[i];
                string value = values[i];
                if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(value))
                    continue;
                parts[i] = key + "=" + value;
            }
            return "?" + string.Join("&", parts);
        }

        public static string GetFilePath(string baseUrl, string filename, string extension)
        {
            if (string.IsNullOrEmpty(filename))
                return string.Empty;

            if (!extension.StartsWith("."))
                extension = $".{extension}";
            if (!baseUrl.EndsWith("/") && !baseUrl.EndsWith(@"\"))
                baseUrl += @"\";

            return $"{baseUrl}{filename}{extension}";
        }

        public static string GetAbsolutePath(string url)
        {
            if (string.IsNullOrEmpty(url))
                return String.Empty;

            int idx = url.IndexOf('?');

            //Has no query
            if (idx < 0)
                return url;

            return url.Substring(0, idx);
        }

        public static string GetQueryString(string url)
        {
            if (string.IsNullOrEmpty(url))
                return string.Empty;

            int idx = url.IndexOf('?');
            if (idx < 0 || idx == url.Length)
                return string.Empty;

            return url.Substring(idx + 1);
        }

        public static string[] GetPathParts(string url)
        {
            return Path.GetDirectoryName(url).Split(Path.DirectorySeparatorChar);
        }

        public static int GetNoOfPathParts(string url)
        {
            return GetPathParts(url).Count();
        }

        public static bool UrlIsCompanySpecific(string url)
        {
            if (string.IsNullOrEmpty(url))
                return false;

            if (url.Contains("/edit"))
                return true;

            int index = url.IndexOf('?');
            if (url.Length <= index)
                return false;

            int counter = 0;
            string qs = url.Substring(index + 1);
            string[] arr = qs.Split('&');
            foreach (string s in arr)
            {
                if (!string.IsNullOrEmpty(s))
                    counter++;
            }

            return counter > 2;
        }

        public static bool UrlContainsSectionUrl(string url, string sectionUrl)
        {
            if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(sectionUrl))
                return false;

            return url.Contains(sectionUrl);
        }

        public static bool UrlContainsPathAndQuery(string url, string path, string query)
        {
            if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(path) || string.IsNullOrEmpty(query))
                return false;

            //Lowercase
            url = url.ToLower();
            path = path.ToLower();
            query = query.ToLower();

            //Fix default.aspx
            if (!url.Contains("default.aspx") && path.EndsWith("default.aspx"))
                path = path.Remove(path.IndexOf("default.aspx"));

            //Validate path
            string absolutePath = GetAbsolutePath(url);
            if (string.IsNullOrEmpty(absolutePath) || !path.Contains(absolutePath) || (GetNoOfPathParts(path) != GetNoOfPathParts(absolutePath)))
                return false;

            //Validate query. Approve empty query
            string queryString = GetQueryString(url);
            if (!string.IsNullOrEmpty(queryString) && !query.EndsWith(queryString))
                return false;

            return true;
        }

        public static bool UrlHasSameParameterValue(string url1, string url2, string parameter)
        {
            if (url1.Contains("?"))
            {
                int startPos = url1.IndexOf("?") + 1;
                url1 = url1.Substring(startPos, url1.Length - startPos);
            }
            if (url2.Contains("?"))
            {
                int startPos = url2.IndexOf("?") + 1;
                url2 = url2.Substring(startPos, url2.Length - startPos);
            }

            string prefix = "?=" + parameter;

            string value1 = url1.Substring((url1.IndexOf(prefix) + prefix.Length), (url1.Length - prefix.Length));
            string value2 = url2.Substring((url2.IndexOf(prefix) + prefix.Length), (url2.Length - prefix.Length));

            if (string.IsNullOrEmpty(value1) || string.IsNullOrEmpty(value2))
            {
                prefix = "&=" + parameter;

                if (string.IsNullOrEmpty(value1))
                    value1 = url1.Substring((url1.IndexOf(prefix) + prefix.Length), (url1.Length - prefix.Length));
                if (string.IsNullOrEmpty(value2))
                    value2 = url2.Substring((url2.IndexOf(prefix) + prefix.Length), (url2.Length - prefix.Length));
            }

            return value1.Equals(value2);
        }

        public static bool UrlContainsQS(string url, string name)
        {
            if (string.IsNullOrEmpty(url))
                return false;
            string qs1 = "?" + name + "=";
            string qs2 = "&" + name + "=";
            if (url.Contains(qs1) || url.Contains(qs2))
                return true;
            return false;
        }

        public static bool UrlIsExeFile(string url)
        {
            if (string.IsNullOrEmpty(url))
                return false;
            return url.EndsWith(".exe");
        }

        public static string AddQueryStringParameter(Uri url, string name, int? value)
        {
            if (!value.HasValue)
                return url.ToString();
            return AddQueryStringParameter(url.ToString(), name, value.Value.ToString());
        }

        public static string AddQueryStringParameter(Uri url, string name, string value)
        {
            return AddQueryStringParameter(url.ToString(), name, value);
        }

        public static string AddQueryStringParameter(string url, string name, string value)
        {
            if (url == null)
                return string.Empty;

            if (UrlContainsQS(url, name))
                return url;
            if (UrlIsExeFile(url))
                return url;

            string newUrl = "?" + name + "=" + value;

            string[] parts = url.Split('?');
            newUrl = parts[0] + newUrl;
            if (parts.Length > 1)
                newUrl += "&" + String.Join(String.Empty, parts, 1, parts.Length - 1);

            parts = url.Split('#');
            if (parts.Length > 1)
                newUrl += "#" + String.Join(String.Empty, parts, 1, parts.Length - 1);

            return newUrl;
        }

        public static string EnsureTrailingSlash(this Uri uri)
        {
            var uriStr = uri.ToString();
            return uriStr.EndsWith("/") ? uriStr : uriStr + "/";
        }

        public static string RemoveTrailingSlash(this Uri uri)
        {
            var uriStr = uri.ToString();
            return uriStr.EndsWith("/") ? uriStr.TrimEnd('/') : uriStr;
        }

        #region Menu navigation

        public static string GetModuleUrl(string module)
        {
            return Constants.SOE_URL_BASE + module + "/";
        }

        public static string GetSectionUrl(string module, params string[] sections)
        {
            string url = GetModuleUrl(module);
            foreach (string section in sections)
            {
                url += section + "/";
            }
            return url;
        }

        public static string TrimUrl(string url)
        {
            if (url == null)
                return String.Empty;

            url = url.Replace("Default.aspx", string.Empty);
            url = url.Replace("default.aspx", string.Empty);
            if (url.EndsWith("/"))
                url = url.Substring(0, url.Length - 1);
            return url;
        }

        public static string ToValidUrl(string url, string page)
        {
            if (String.IsNullOrEmpty(url))
                return url;
            if (!url.EndsWith("/"))
                url += "/";
            if (!url.Contains(page))
                url += page;
            return url;
        }

        public static string ToValidUrl(string url)
        {
            if (String.IsNullOrEmpty(url))
                return url;
            if (!url.EndsWith("/"))
                url += "/";
            return url;
        }

        #endregion


        #region Redirect

        public static bool HasXForwardedHeaders(HttpRequest request)
        {
            return request.Headers["X-Forwarded-Proto"] != null || request.Headers["X-Forwarded-Host"] != null || request.Headers["X-Forwarded-Port"] != null;
        }

        public static string HandlePotentialXForwarders(string url, HttpRequest request)
        {
            if (!HasXForwardedHeaders(request))
                return url;

            try
            {
                if (!string.IsNullOrEmpty(url) && url.ToLower().Contains("https://") && request?.Url != null && url.ToLower().Contains(request.Url.Host.ToLower()))
                {
                    if (request.Headers["X-Forwarded-Host"] != null && url.ToLower().Contains(request.Headers["X-Forwarded-Host"].ToLower()))
                        return url;

                    //Make info a Uri and replace host schema and port
                    Uri info = new Uri(url);
                    UriBuilder builder = new UriBuilder(info);
                    builder.Scheme = request.Url.Scheme;
                    builder.Host = request.Url.Host;
                    builder.Port = request.Url.Port;
                    return builder.Uri.ToString();
                }
            }
            catch
            {
                // Intentionally ignored, safe to continue
                // NOSONAR
            }
            return url;
        }

        public static string GetCurrentAuthorityUrl(HttpRequest request)
        {
            string scheme = request.Headers["X-Forwarded-Proto"] ?? request.Url.Scheme;
            string host = request.Headers["X-Forwarded-Host"] ?? request.Url.Host;
            string portString = request.Headers["X-Forwarded-Port"] ?? request.Url.Port.ToString();

            // Check if the newHost contains a port
            int colonIndex = host.IndexOf(':');
            if (colonIndex != -1)
            {
                portString = host.Substring(colonIndex + 1);
                host = host.Substring(0, colonIndex);
            }

            if (int.TryParse(portString, out int port))
            {
                UriBuilder builder = new UriBuilder(scheme, host, port);
                return builder.Uri.EnsureTrailingSlash().ToString();
            }
            else
            {
                UriBuilder builder = new UriBuilder(scheme, host);
                return builder.Uri.EnsureTrailingSlash().ToString();
            }
        }

        public static string GetModifiedUrl(HttpRequest request)
        {
            if (!HasXForwardedHeaders(request))
                return request.Url.ToString();

            // Extract the new scheme, host, and port from headers or default values
            string newScheme = request.Headers["X-Forwarded-Proto"] ?? request.Url.Scheme;
            string newHost = request.Headers["X-Forwarded-Host"] ?? request.Url.Host;
            string newPortString = request.Headers["X-Forwarded-Port"] ?? request.Url.Port.ToString();

            // Check if the newHost contains a port
            int colonIndex = newHost.IndexOf(':');
            if (colonIndex != -1)
            {
                newPortString = newHost.Substring(colonIndex + 1);
                newHost = newHost.Substring(0, colonIndex);
            }

            // Use the original URL as the base
            Uri originalUri = request.Url;

            // Create a new UriBuilder based on the original URL
            UriBuilder builder = new UriBuilder(originalUri);

            // Check if the scheme is being updated to a different value
            if (newScheme != builder.Scheme)
            {
                builder.Scheme = newScheme;
                builder.Port = -1;  // Reset the port to the default for the new scheme
            }

            // Update the host
            builder.Host = newHost;

            // If the new port can be parsed, set it
            if (int.TryParse(newPortString, out int newPort))
            {
                builder.Port = newPort;
            }

            return builder.Uri.ToString();
        }

        #endregion
    }
}
