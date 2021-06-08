using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;

namespace DataInterfaceConsoleTest.Variants
{
    static class GithubVariantGetter
    {
        private static JSONVariant[] cache = null;
        public static readonly Uri variantsFile = new Uri("https://raw.githubusercontent.com/GHXX/FiveDChessDataInterface/master/DataInterfaceConsoleTest/Resources/JsonVariants.json");

        public static JSONVariant[] GetAllVariants(bool bypassCache = false)
        {
            if (bypassCache || cache == null) // if we bypass the cache, or if the cache is nonexistent, request the file
            {
                var wr = WebRequest.CreateHttp(variantsFile);
                var resp = wr.GetResponse();
                using (var sr = new StreamReader(resp.GetResponseStream()))
                {
                    cache = JsonConvert.DeserializeObject<JSONVariant[]>(sr.ReadToEnd());
                }
            }

            return cache;
        }
    }
}
