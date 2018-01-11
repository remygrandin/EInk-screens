using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using MasterModuleCommon;
using NLog;
using ScreenConnection;

namespace DrawfriendPonyGraphics
{
    public class DrawfriendPonyGraphicsProvider : GraphicProvider
    {
        private string _cachePath;

        private string _drawfrienUrl = @"https://www.equestriadaily.com/search/label/Drawfriend";

        private Logger _logger;

        public override void Init(Logger logger, IList<MasterModuleCommon.KeyValuePair<string, string>> parameters)
        {
            _logger = logger;

            _cachePath = parameters.FirstOrDefault(item => item.Key == "CachePath").Value;
            _cachePath = @"C:\MasterControl\Cache\DrawfriendPony\";

            logger.Info("---- Init Drawfriend Pony Downloader module ----");

            WebClient wc = new WebClient();

            wc.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");

            logger.Info("Locating lastest drawfriend from \"" + _drawfrienUrl + "\"");
            string listingHtml = wc.DownloadString(_drawfrienUrl);

            Regex lastest = new Regex("post-title entry-title[\\s\\S]{0,30}<a href=\"(.*)\".*#(\\d{1,10})<\\/a>");
            var lastestMatch = lastest.Match(listingHtml);

            logger.Info("Lastest is #" + lastestMatch.Groups[2].Value);

            string cacheFolder = Path.Combine(_cachePath, lastestMatch.Groups[2].Value);

            if (!Directory.Exists(cacheFolder))
            {
                logger.Info("No cache folder foud at \"" + cacheFolder + "\", creating it...");
                Directory.CreateDirectory(cacheFolder);
            }

            logger.Info("Downloading lastest page at \"" + lastestMatch.Groups[1].Value + "\"");

            string pageHtml = wc.DownloadString(lastestMatch.Groups[1].Value);

            Regex imageExtractor = new Regex("<img border=\"0\" src=\"(.*)\"");

            MatchCollection matches = imageExtractor.Matches(pageHtml);
            logger.Info("Found " + matches.Count + " picture(s)");

            foreach (Match match in matches)
            {
                string fileName = Path.GetFileName(match.Groups[1].Value);
                string targetPath = Path.Combine(cacheFolder, fileName);

                if (File.Exists(targetPath))
                {
                    logger.Info("File " + fileName + " already downloaded");
                }
                else
                {
                    logger.Info("Downloading \"" + match.Groups[1].Value +"\"");
                    wc.DownloadFile(match.Groups[1].Value, targetPath);
                }



            }
        }

        public override Bitmap GetNextGraphic(Screen target)
        {

            return null;



        }
    }
}
