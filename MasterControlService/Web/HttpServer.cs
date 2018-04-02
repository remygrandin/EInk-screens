using System;
using System.IO;
using System.Reflection;
using Nancy;
using Nancy.Responses;

namespace MasterControlService.Web
{
    public class HttpServerStatic : NancyModule
    {
        public static string StaticRoot = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\WebStatic";

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
                            break;
                        }
                    }

                }

                if (!indexFound)
                {
                    response.WithStatusCode(HttpStatusCode.NotFound);

                    response.ReasonPhrase = "FIle Not Found";

                    return response;
                }
            }

            string fileName = Path.GetFileName(fullPath);

            var file = new FileStream(fullPath, FileMode.Open);

            response = new StreamResponse(() => file, MimeTypes.GetMimeType(fileName));

            return response;
        }
    }
}
