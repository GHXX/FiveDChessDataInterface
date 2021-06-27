using Newtonsoft.Json;
using System.IO;
using System.Net;
using System.Reflection;

namespace FiveDChessDataInterface.Variants
{
    public static class GithubVariantGetter
    {
        private static JSONVariant[] cache = null;
        public static readonly string baseUri = "https://raw.githubusercontent.com/GHXX/FiveDChessDataInterface/master/FiveDChessDataInterface";
        public static readonly string variantsFile = "Resources/JsonVariants.json";
        public static bool IsCached => cache != null;

        public static JSONVariant[] GetAllVariants(bool useLocal = false, bool bypassCache = false)
        {
            if (bypassCache || cache == null) // if we bypass the cache, or if the cache is nonexistent, request the file
            {
                if (useLocal)
                {
                    var filePath = Path.Combine(new FileInfo(Assembly.GetExecutingAssembly().Location).Directory.FullName, variantsFile);
                    var text = File.ReadAllText(filePath);
                    cache = JsonConvert.DeserializeObject<JSONVariant[]>(text);
                }
                else
                {
                    var wr = WebRequest.CreateHttp(string.Join("/", new[] { baseUri, variantsFile }));
                    var resp = wr.GetResponse();
                    using (var sr = new StreamReader(resp.GetResponseStream()))
                    {
                        cache = JsonConvert.DeserializeObject<JSONVariant[]>(sr.ReadToEnd());
                    }
                }
            }

            return cache;
        }
    }
}
