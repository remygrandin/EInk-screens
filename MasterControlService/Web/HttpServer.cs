using System;
using System.IO;
using System.Reflection;
using Nancy;
using Nancy.Responses;
using NLog;

namespace MasterControlService.Web
{
    /// <summary>
    /// Deliver all static files
    /// </summary>
    public class HttpServerStatic : NancyModule
    {
        public static string StaticRoot = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\WebStatic";
        public static Logger WebLogger;

        public HttpServerStatic()
        {
            Get["/{Path*}"] = DefaultStatic;
            Get["/"] = DefaultStatic;
        }

        private dynamic DefaultStatic(dynamic parameters)
        {
            Response response = new Response();

            string fullPath = Path.GetFullPath(Path.Combine(StaticRoot, ((string)parameters.Path ?? "").Replace("/", "\\")));

            if (!fullPath.Contains(StaticRoot))
            {
                response.WithStatusCode(HttpStatusCode.Forbidden);

                response.ReasonPhrase = "Illegal Path";
                WebLogger.Error("[403] Client " + this.Request.UserHostAddress + " tried to access illegal path : " + fullPath);
                return response;
            }

            if (!File.Exists(fullPath))
            {
                bool indexFound = false;
                if (Context.Request.Url.Path.EndsWith(@"/") && Directory.Exists(fullPath))
                {

                    foreach (string indexFile in new[] { "index.html", "index.htm" })
                    {
                        if (File.Exists(Path.Combine(fullPath, indexFile)))
                        {
                            indexFound = true;
                            fullPath = Path.Combine(fullPath, indexFile);
                            WebLogger.Info("[200] Client " + this.Request.UserHostAddress + " requested  : " + (string)parameters.Path);
                            break;
                        }
                    }

                }

                if (!indexFound)
                {
                    response.WithStatusCode(HttpStatusCode.NotFound);

                    response.ReasonPhrase = "FIle Not Found";
                    WebLogger.Warn("[404] Client " + this.Request.UserHostAddress + " requested unknown file : " + (string)parameters.Path);
                    return response;
                }
            }

            string fileName = Path.GetFileName(fullPath);

            var file = new FileStream(fullPath, FileMode.Open);

            response = new StreamResponse(() => file, MimeTypes.GetMimeType(fileName));

            WebLogger.Info("[200] Client " + this.Request.UserHostAddress + " requested  : " + (string)parameters.Path);

            return response;
        }
    }
}
