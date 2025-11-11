using ISBoxerEVELauncher.Security;
using ISBoxerEVELauncher.Web;
using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;

namespace ISBoxerEVELauncher.Extensions
{
    public static class HttpWebRequestExtension
    {
        public static void SetBody(this HttpWebRequest webRequest, string bodyText)
        {
            webRequest.SetBody(Encoding.UTF8.GetBytes(bodyText));
        }


        public static void SetBody(this HttpWebRequest webRequest, byte[] body)
        {

            webRequest.ContentLength = body.Length;
            App.requestBody = body;
            try
            {
                if (!App.tofCaptcha)
                {
                    using (Stream reqStream = webRequest.GetRequestStream())
                    {
                        reqStream.Write(body, 0, body.Length);
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        public static void SetBody(this HttpWebRequest webRequest, SecureBytesWrapper body)
        {

            webRequest.ContentLength = body.Bytes.Length;

            try
            {
                using (Stream reqStream = webRequest.GetRequestStream())
                {
                    reqStream.Write(body.Bytes, 0, body.Bytes.Length);
                }

            }
            catch (Exception)
            {
            }
        }

        public static void SetCustomheaders(this HttpWebRequest webRequest, WebHeaderCollection webHeaderCollection)
        {
            try
            {
                // Try reflection approach first (legacy compatibility)
                var field = typeof(HttpWebRequest).GetField("_HttpRequestHeaders", BindingFlags.Instance | BindingFlags.NonPublic);
                if (field != null)
                {
                    field.SetValue(webRequest, webHeaderCollection);
                    return;
                }
            }
            catch
            {
                // Reflection failed, fall back to manual header copying
            }

            // Fallback: Copy headers manually (compatible with .NET 9)
            if (webHeaderCollection is CustomWebHeaderCollection customHeaders)
            {
                // Extract custom headers from the CustomWebHeaderCollection
                var customHeadersDict = customHeaders.GetType()
                    .GetField("_customHeaders", BindingFlags.Instance | BindingFlags.NonPublic)
                    ?.GetValue(customHeaders) as System.Collections.Generic.Dictionary<string, string>;

                if (customHeadersDict != null)
                {
                    foreach (var header in customHeadersDict)
                    {
                        try
                        {
                            // Set headers that can be set directly
                            if (header.Key.Equals("User-Agent", StringComparison.OrdinalIgnoreCase))
                            {
                                webRequest.UserAgent = header.Value;
                            }
                            else if (header.Key.Equals("Referer", StringComparison.OrdinalIgnoreCase))
                            {
                                webRequest.Referer = header.Value;
                            }
                            else if (header.Key.Equals("Accept", StringComparison.OrdinalIgnoreCase))
                            {
                                webRequest.Accept = header.Value;
                            }
                            else if (header.Key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase))
                            {
                                webRequest.ContentType = header.Value;
                            }
                            else
                            {
                                // For other headers, add to Headers collection
                                webRequest.Headers.Add(header.Key, header.Value);
                            }
                        }
                        catch
                        {
                            // Skip headers that can't be set
                        }
                    }
                }
            }
            else
            {
                // Copy all headers from the WebHeaderCollection
                foreach (string key in webHeaderCollection.AllKeys)
                {
                    try
                    {
                        string value = webHeaderCollection[key];
                        if (key.Equals("User-Agent", StringComparison.OrdinalIgnoreCase))
                        {
                            webRequest.UserAgent = value;
                        }
                        else if (key.Equals("Referer", StringComparison.OrdinalIgnoreCase))
                        {
                            webRequest.Referer = value;
                        }
                        else if (key.Equals("Accept", StringComparison.OrdinalIgnoreCase))
                        {
                            webRequest.Accept = value;
                        }
                        else if (key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase))
                        {
                            webRequest.ContentType = value;
                        }
                        else
                        {
                            webRequest.Headers.Add(key, value);
                        }
                    }
                    catch
                    {
                        // Skip headers that can't be set
                    }
                }
            }
        }
    }

}

