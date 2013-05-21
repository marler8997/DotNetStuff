using System;
using System.Collections.Generic;

namespace Marler.Net
{
    public static class HttpProtocol
    {
        public const Int32 HttpResponseOK = 200;
        public const Int32 HttpResponseBadRequest = 400;
        public const Int32 HttpResponseNotFound = 404;

        public static Dictionary<Int32, String> responseStatus;
        static HttpProtocol()
        {
            responseStatus = new Dictionary<Int32, String>();

            responseStatus.Add(200, "200 Ok");
            responseStatus.Add(201, "201 Created");
            responseStatus.Add(202, "202 Accepted");
            responseStatus.Add(204, "204 No Content");

            responseStatus.Add(301, "301 Moved Permanently");
            responseStatus.Add(302, "302 Redirection");
            responseStatus.Add(304, "304 Not Modified");

            responseStatus.Add(400, "400 Bad Request");
            responseStatus.Add(401, "401 Unauthorized");
            responseStatus.Add(403, "403 Forbidden");
            responseStatus.Add(404, "404 Not Found");

            responseStatus.Add(500, "500 Internal Server Error");
            responseStatus.Add(501, "501 Not Implemented");
            responseStatus.Add(502, "502 Bad Gateway");
            responseStatus.Add(503, "503 Service Unavailable");
        }
        public static String GetParentUrl(this String url)
        {
            if (url == null || url.Length <= 1) return null;

            Int32 index = url.Length - 1;

            if (url[index] == '/') index--;

            while (true)
            {
                if (index < 0) return null;
                if (url[index] == '/') return url.Substring(0, index + 1);
                index--;
            }
        }
        public static String GetUrlFilename(this String url)
        {
            if (url == null || url.Length <= 0) return null;

            Int32 lengthWithoutSlash = url.Length;
            Int32 index = url.Length - 1;

            if (url[index] == '/')
            {
                lengthWithoutSlash--;
                index--;
            }

            while (true)
            {
                if (index < 0) return (url.Length == lengthWithoutSlash) ? url : url.Substring(0, lengthWithoutSlash);
                if (url[index] == '/') return url.Substring(index + 1, lengthWithoutSlash - index - 1);
                index--;
            }
        }
    }
}
