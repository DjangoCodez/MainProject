using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Common.DTO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Security;
using System.Security.Authentication;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Core;

internal static class FinvoiceHelper
{
/* Security measures:
  - ✅ DNS rebinding protection (resolve IP, connect directly, set Host header)
  - ✅ Comprehensive private/reserved IP blocking (IPv4 + IPv6 + 6to4 + IPv4-mapped)
  - ✅ HTTPS only, port 443 only
  - ✅ All redirect status codes blocked (301, 302, 303, 307, 308)
  - ✅ Content-Type validation (PDF + Octet-stream only)
  - ✅ TLS 1.2/1.3 only
  - ✅ Redirects disabled on handler (AllowAutoRedirect = false)
  - ✅ Virus scanning
  - ✅ SecurityException throws with descriptive messages for logging

  Resource protection:
  - ✅ 30 second timeout
  - ✅ 25 MB file size limit (header check + streaming check)
  - ✅ Streaming download with early abort */

	private const string UserAgent = "Mozilla/5.0 (Windows NT; Windows NT 10.0; sv-SE) WindowsPowerShell/5.1.26100.7462";
	private static readonly HttpClient _httpClient = CreateHttpClient();

	private static HttpClient CreateHttpClient()
	{
		var handler = new HttpClientHandler
		{
			UseProxy = true,
			Proxy = WebRequest.GetSystemWebProxy(),
			DefaultProxyCredentials = CredentialCache.DefaultCredentials,
			AllowAutoRedirect = false,
			SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13
		};

		var client = new HttpClient(handler)
		{
			Timeout = TimeSpan.FromSeconds(30)
		};
		client.DefaultRequestHeaders.Add("User-Agent", UserAgent);
		return client;
	}

    internal static async Task<byte[]> GetExternalInvoice(string content)
    {
		var invoiceUrl = ExtractFinvoiceUrl(content);
		if (string.IsNullOrWhiteSpace(invoiceUrl))
			return null;

		invoiceUrl = invoiceUrl.Replace("&amp;", "&");
		var (uri, resolvedIp) = await ValidateExternalUrlAsync(invoiceUrl).ConfigureAwait(false);
		var externalInvoice = await DownloadExternalFileAsync(uri, resolvedIp).ConfigureAwait(false);

		if (DefenderUtil.IsVirus(externalInvoice))
			throw new SecurityException($"Virus detected\n{Environment.StackTrace}");

		return externalInvoice;
	}

    internal static IReadOnlyList<XDocument> ParseDocuments(IReadOnlyList<GenericType<string, string>> finvoiceItems, bool importOnlyValidForCompany, string companyAddressIdentifier, bool includesEnvelope)
    {
		var result = new List<XDocument>();

		foreach (var finvoice in finvoiceItems)
		{
			if (!importOnlyValidForCompany || string.IsNullOrWhiteSpace(companyAddressIdentifier))
			{
				result.Add(XDocument.Parse(finvoice.Field2));
				continue;
			}

			if (includesEnvelope)
				result.AddRange(ParseDocumentsFromSoapEnvelope(finvoice, companyAddressIdentifier));
			else
			{
				var doc = TryParseDocumentFromMessage(finvoice, companyAddressIdentifier);
				if (doc is not null)
					result.Add(doc);
			}
		}

		return result;
    }

    private static IReadOnlyList<XDocument> ParseDocumentsFromSoapEnvelope(GenericType<string, string> item, string companyAddressIdentifier)
    {
		var result = new List<XDocument>();
		const string ns = "http://www.oasis-open.org/committees/ebxml-msg/schema/msg-header-2_0.xsd";

		var doc = XDocument.Parse(item.Field1);
		var fromElements = doc.Root.Descendants(ns + "To");
		foreach (var from in fromElements)
		{
			var roleElement = from.Descendants(ns + "Role").FirstOrDefault();
			if (roleElement is null || !roleElement.Value.Equals("receiver", StringComparison.OrdinalIgnoreCase))
				continue;

			var partyElement = from.Descendants(ns + "PartyId").FirstOrDefault();
			if (partyElement is not null && partyElement.Value == companyAddressIdentifier)
				result.Add(XDocument.Parse(item.Field2));
		}

		return result;
	}

    private static XDocument TryParseDocumentFromMessage(GenericType<string, string> item, string companyAddressIdentifier)
    {
		var doc = XDocument.Parse(item.Field2);
		var receiverElement = doc.Root.Descendants("MessageReceiverDetails").FirstOrDefault();
		if (receiverElement is null)
			return doc;

		var identifierElement = receiverElement.Descendants("ToIdentifier").FirstOrDefault();
		if (identifierElement is not null && identifierElement.Value == companyAddressIdentifier)
			return doc;

		return null;
	}

	internal static IReadOnlyList<GenericType<string, string>> ParseFinvoiceWithoutEnvelope(string content)
    {
		var result = new List<GenericType<string, string>>();

        int startIndex = FindDocumentStart(content);
        if (startIndex == -1)
            return result;

        int endIndex = content.IndexOf(FinvoiceXmlTags.FinvoiceEnd);
        if (endIndex == -1)
            return result;

        string finvoice = content.Substring(startIndex, endIndex + FinvoiceXmlTags.FinvoiceEnd.Length - startIndex);
        result.Add(new GenericType<string, string> { Field1 = "", Field2 = finvoice });

		return result;
    }

    internal static IReadOnlyList<GenericType<string, string>> ParseFinvoicesWithEnvelope(string content)
    {
		var result = new List<GenericType<string, string>>();

        while (content.Contains(FinvoiceXmlTags.EnvelopeStart))
        {
            int envelopeStart = content.IndexOf(FinvoiceXmlTags.EnvelopeStart);
            int envelopeEnd = content.IndexOf(FinvoiceXmlTags.EnvelopeEnd) + FinvoiceXmlTags.EnvelopeEnd.Length;
            string envelope = content.Substring(envelopeStart, envelopeEnd - envelopeStart);
            content = content.Substring(envelopeEnd);

            int finvoiceStart = content.IndexOf(FinvoiceXmlTags.FinvoiceStart);
            int finvoiceEnd = content.IndexOf(FinvoiceXmlTags.FinvoiceEnd) + FinvoiceXmlTags.FinvoiceEnd.Length;
            string finvoice = content.Substring(finvoiceStart, finvoiceEnd - finvoiceStart);
            content = content.Substring(finvoiceEnd);

            result.Add(new GenericType<string, string> { Field1 = envelope, Field2 = finvoice });
        }

		return result;
    }

    internal static bool ContainsEnvelope(string content)
    {
        return content.Contains(FinvoiceXmlTags.EnvelopeStart);
    }

	internal static string EscapeUnescapedAmpersands(string content)
	{
		if (string.IsNullOrWhiteSpace(content))
			return "";

		return System.Text.RegularExpressions.Regex.Replace(content, @"&(?!(amp|lt|gt|quot|apos|#\d+|#x[0-9a-fA-F]+);)", "&amp;");
	}

	private static int FindDocumentStart(string content)
    {
        int index = content.IndexOf(FinvoiceXmlTags.XmlDocStart);
        return index >= 0 ? index : content.IndexOf(FinvoiceXmlTags.FinvoiceStart);
    }

	private static string ExtractFinvoiceUrl(string content)
	{
		int invoiceUrlTagStartIndex = content.IndexOf(FinvoiceXmlTags.InvoiceUrlStart, StringComparison.OrdinalIgnoreCase);
		if (invoiceUrlTagStartIndex < 0)
			return null;

		int invoiceUrlTagEndIndex = content.IndexOf(FinvoiceXmlTags.InvoiceUrlEnd, StringComparison.OrdinalIgnoreCase);
		if (invoiceUrlTagEndIndex < 0)
			return null;

		invoiceUrlTagStartIndex += FinvoiceXmlTags.InvoiceUrlStart.Length;
		int invoiceUrlLength = invoiceUrlTagEndIndex - invoiceUrlTagStartIndex;
		return content.Substring(invoiceUrlTagStartIndex, invoiceUrlLength);
	}

    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
	{
		"application/pdf",
		"application/octet-stream"
	};

    private static async Task<byte[]> DownloadExternalFileAsync(Uri uri, IPAddress resolvedIp)
    {
        const int MaxFileSize = 25 * 1024 * 1024; // 25 MB

		var requestUri = new UriBuilder(uri) { Host = resolvedIp.ToString() }.Uri;
		using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
		request.Headers.Host = uri.Host;

		using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
		if (response.StatusCode == HttpStatusCode.MovedPermanently ||
			response.StatusCode == HttpStatusCode.Redirect ||
			response.StatusCode == HttpStatusCode.SeeOther ||
			response.StatusCode == HttpStatusCode.TemporaryRedirect ||
			(int)response.StatusCode == 308) // PermanentRedirect
			throw new SecurityException($"Redirects not allowed. Status: {response.StatusCode}");

		response.EnsureSuccessStatusCode();

		var contentType = response.Content.Headers.ContentType?.MediaType;
		if (contentType is null || !AllowedContentTypes.Contains(contentType))
			throw new SecurityException($"Invalid content type: {contentType}. Expected: application/pdf");

		var contentLength = response.Content.Headers.ContentLength;
		if (contentLength.HasValue && contentLength.Value > MaxFileSize)
			throw new InvalidOperationException($"File size ({contentLength.Value} bytes) exceeds maximum allowed ({MaxFileSize} bytes)");

		using var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
		using var memoryStream = new MemoryStream();
		var buffer = new byte[8192];
		int bytesRead;
		int totalBytesRead = 0;

		while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false)) > 0)
		{
			totalBytesRead += bytesRead;
			if (totalBytesRead > MaxFileSize)
				throw new InvalidOperationException($"File size exceeds maximum allowed ({MaxFileSize} bytes)");

			await memoryStream.WriteAsync(buffer, 0, bytesRead);
		}

		return memoryStream.ToArray();
    }

	private static async Task<(Uri uri, IPAddress resolvedIp)> ValidateExternalUrlAsync(string url)
	{
		if (!Uri.TryCreate(url, UriKind.Absolute, out Uri uri))
			throw new SecurityException($"Invalid URL format: {url}");

		if (uri.Scheme != Uri.UriSchemeHttps)
			throw new SecurityException($"Only HTTPS allowed. Scheme: {uri.Scheme}");

		if (uri.Port != 443 && uri.Port != -1)
			throw new SecurityException($"Invalid port: {uri.Port}. Only port 443 allowed.");

		IPAddress[] addresses;
		try
		{
			addresses = await Dns.GetHostAddressesAsync(uri.Host).ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			throw new SecurityException($"DNS resolution failed for host: {uri.Host}", ex);
		}

		var privateIp = addresses.FirstOrDefault(ip => IsPrivateOrReservedIp(ip));
		if (privateIp is not null)
			throw new SecurityException($"URL resolves to private/reserved IP: {privateIp}. Host: {uri.Host}");

		var preferredIp = addresses.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork) ?? addresses[0];
		return (uri, preferredIp);
	}

	private static bool IsPrivateOrReservedIp(IPAddress ip)
	{
		if (IPAddress.IsLoopback(ip))
			return true;

		byte[] bytes = ip.GetAddressBytes();

		return ip.AddressFamily switch
		{
			AddressFamily.InterNetwork => IsPrivateOrReservedIPv4(bytes),
			AddressFamily.InterNetworkV6 => IsPrivateOrReservedIPv6(bytes, ip),
			_ => true
		};
	}

	private static bool IsPrivateOrReservedIPv4(byte[] bytes)
	{
		return bytes[0] == 0 ||                                          // 0.0.0.0/8 (current network)
			bytes[0] == 10 ||                                            // 10.0.0.0/8 (private)
			bytes[0] == 127 ||                                           // 127.0.0.0/8 (loopback)
			(bytes[0] == 100 && bytes[1] >= 64 && bytes[1] <= 127) ||    // 100.64.0.0/10 (carrier-grade NAT)
			(bytes[0] == 169 && bytes[1] == 254) ||                      // 169.254.0.0/16 (link-local, metadata)
			(bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31) ||     // 172.16.0.0/12 (private)
			(bytes[0] == 192 && bytes[1] == 0 && bytes[2] == 0) ||       // 192.0.0.0/24 (IETF protocol assignments)
			(bytes[0] == 192 && bytes[1] == 0 && bytes[2] == 2) ||       // 192.0.2.0/24 (documentation)
			(bytes[0] == 192 && bytes[1] == 168) ||                      // 192.168.0.0/16 (private)
			(bytes[0] == 198 && bytes[1] == 51 && bytes[2] == 100) ||    // 198.51.100.0/24 (documentation)
			(bytes[0] == 203 && bytes[1] == 0 && bytes[2] == 113) ||     // 203.0.113.0/24 (documentation)
			bytes[0] >= 224;                                             // 224.0.0.0/4 multicast + 240.0.0.0/4 reserved
	}

	private static bool IsPrivateOrReservedIPv6(byte[] bytes, IPAddress ip)
	{
		return ip.IsIPv6LinkLocal ||
			ip.IsIPv6SiteLocal ||
			(bytes[0] & 0xfe) == 0xfc ||                                                   // fc00::/7 (unique local)
			bytes[0] == 0xff ||                                                            // ff00::/8 (multicast)
			(ip.IsIPv4MappedToIPv6 && IsPrivateOrReservedIp(ip.MapToIPv4())) ||            // ::ffff:0:0/96 (IPv4-mapped)
			(bytes[0] == 0x20 && bytes[1] == 0x02 &&                                       // 2002::/16 (6to4)
				IsPrivateOrReservedIPv4(new[] { bytes[2], bytes[3], bytes[4], bytes[5] })) ||
			(bytes[0] == 0x20 && bytes[1] == 0x01 &&                                       // 2001:db8::/32 (documentation)
				bytes[2] == 0x0d && bytes[3] == 0xb8);
	}

	private static class FinvoiceXmlTags
    {
        public const string XmlDocStart = @"<?xml version=";
        public const string EnvelopeStart = @"<SOAP-ENV:Envelope ";
        public const string EnvelopeEnd = @"</SOAP-ENV:Envelope>";
        public const string FinvoiceStart = @"<Finvoice ";
        public const string FinvoiceEnd = @"</Finvoice>";
        public const string InvoiceUrlStart = @"<InvoiceUrlText>";
        public const string InvoiceUrlEnd = @"</InvoiceUrlText>";
	}
}
