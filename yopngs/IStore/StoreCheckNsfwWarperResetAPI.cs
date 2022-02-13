using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Iimages.IStore
{
    public class StoreCheckNsfwWarperResetAPI : IStoreCheck
    {
        private readonly string NSFWFLODER;
        private readonly IHttpClientFactory HttpClientFactory;
        private readonly string SERVERHOST;
        private readonly string NSFWHOST;
        private readonly double NSFWCORE;

        public StoreCheckNsfwWarperResetAPI(IApplicationBuilder app, IWebHostEnvironment env, IConfiguration config, IHttpClientFactory httpClientFactory)
        {
            HttpClientFactory = httpClientFactory;
            if (config.GetSection("GLOBAL") != null)
            {
                NSFWCORE = Convert.ToDouble(config["GLOBAL:NSFWCORE"]);
                NSFWHOST = Convert.ToString(config["GLOBAL:NSFWHOST"]);
                SERVERHOST = Convert.ToString(config["GLOBAL:SERVERHOST"]);

                NSFWFLODER = Path.Combine(env.ContentRootPath, "nsfw");
                if (!Directory.Exists(NSFWFLODER))
                {
                    Directory.CreateDirectory(NSFWFLODER);
                }

                app.UseStaticFiles(new StaticFileOptions
                {
                    FileProvider = new PhysicalFileProvider(NSFWFLODER),
                    RequestPath = "/nsfw"
                });
            }
        }

        public bool PassSex(byte[] formFile)
        {

            //保存到鉴黄目录
            var localName = Guid.NewGuid().ToString() + ".png";
            var nsfwFile = string.Format("{0}/{1}", NSFWFLODER, localName);
            File.WriteAllBytes(nsfwFile, formFile);
            try
            {
                var nsfwFileUrl = string.Format("{0}/{1}/{2}", SERVERHOST, "nsfw", localName);
                var url = string.Format("{0}{1}", NSFWHOST, "/?url=" + nsfwFileUrl);
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                var response = HttpClientFactory.CreateClient().SendAsync(request).Result;
                var result = response.Content.ReadAsStringAsync().Result;
                var jsonDoc = System.Text.Json.JsonDocument.Parse(result);
                var jsonScore = jsonDoc.RootElement.GetProperty("score");
                if (jsonScore.GetDouble() < NSFWCORE)
                    return true;
                return false;

            }
            catch
            {
                return false;
            }
            finally
            {
                File.Delete(nsfwFile);
            }
        }
    }
}
