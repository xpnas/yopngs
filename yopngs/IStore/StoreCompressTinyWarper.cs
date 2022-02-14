using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Minimage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Iimages.IStore
{
    public class StoreCompressTinyWarper : IStoreCompress
    {

        private readonly IHttpClientFactory HttpClientFactory;



        public StoreCompressTinyWarper(IApplicationBuilder app, IWebHostEnvironment env, IConfiguration config, IHttpClientFactory httpClientFactory)
        {

            HttpClientFactory = httpClientFactory;

        }

        public byte[] Compress(byte[] maps)
        {
            try
            {
                var randomIP = string.Format("{0}.{1},{2},{3}", new Random().Next(1, 254), new Random().Next(1, 254), new Random().Next(1, 254), new Random().Next(1, 254));
                var webRequest = WebRequest.Create(new Uri("https://tinypng.com//web/shrink"));
                webRequest.Method = "Post";
                webRequest.ContentType = "application/x-www-form-urlencoded";
                webRequest.Headers.Add("rejectUnauthorized", "false");
                webRequest.Headers.Add("Postman-Token", DateTime.Now.ToString());
                webRequest.Headers.Add("Cache-Control", "no-cache");
                webRequest.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/56.0.2924.87 Safari/537.36");
                webRequest.Headers.Add("X-Forwarded-For", randomIP);

                using (var postStream = webRequest.GetRequestStream())
                {
                    var requestStream = webRequest.GetRequestStream();
                    requestStream.Write(maps, 0, maps.Length);
                }

                var response = webRequest.GetResponse();
                using (Stream stream = response.GetResponseStream())
                {
                    using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                    {
                        string resuleJson = reader.ReadToEnd();
                        var compressUrl = JsonDocument.Parse(resuleJson).RootElement.GetProperty("output").GetProperty("url").GetString();

                        using (WebClient client = new WebClient())
                        {

                            using (Stream compressStream = client.OpenRead(compressUrl))
                            {
                                int readCount = 0;
                                int bufferSize = 1 << 17;

                                var buffer = new byte[bufferSize];
                                using (var memory = new MemoryStream())
                                {
                                    while ((readCount = compressStream.Read(buffer, 0, bufferSize)) > 0)
                                    {
                                        memory.Write(buffer, 0, readCount);
                                    }

                                    return memory.ToArray();
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                return maps;
            }

        }
    }
}
