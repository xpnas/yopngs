using Iimages.IStore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using System;
using System.Collections.Generic;
using System.IO;

namespace Iimages.Stores
{

    public class DISKStore : StoreBase
    {

        private string m_diskFloder, m_webFloder, m_host;

        public DISKStore(string diskFloder, string webFloder, string host)
        {
            m_diskFloder = diskFloder;
            m_webFloder = webFloder;
            m_host = host;
        }


        public override bool Up(byte[] maps, string localName, string webServer, ref string cdnUrl)
        {

            if (!Directory.Exists(m_diskFloder))
                Directory.CreateDirectory(m_diskFloder);

            var timeTag = DateTime.Now.ToString("/yyyy/MM/dd/");
            var filePath = m_diskFloder + timeTag + localName;
            var fileDir = Path.GetDirectoryName(filePath);

            if (!Directory.Exists(fileDir))
                Directory.CreateDirectory(fileDir);

            File.WriteAllBytes(filePath,maps);

            cdnUrl = string.Format("{0}{1}{2}{3}", string.IsNullOrEmpty(m_host) ? webServer : m_host, m_webFloder, timeTag, localName);

            return true;
        }
    }


    public class DiskStoreFactory : IStoreFactory
    {
        public IDataStore[] GetStores(IApplicationBuilder app, IConfiguration configuration)
        {
            var results = new List<IDataStore>();
            var selection = configuration.GetSection("DISKStores");
            foreach (IConfigurationSection section in selection.GetChildren())
            {
                var diskfloder = section.GetValue<string>("diskfloder");
                var webfloder = section.GetValue<string>("webfloder");
                var host= section.GetValue<string>("host");
                if (!Directory.Exists(diskfloder)) 
                    Directory.CreateDirectory(diskfloder);
                app.UseStaticFiles(new StaticFileOptions
                {
                    FileProvider = new PhysicalFileProvider(diskfloder),
                    RequestPath = webfloder
                });

                var active = section.GetValue<bool>("active");
                if (active)
                {
                    var name = section.GetValue<string>("name");
                    var type = section.GetValue<string>("type");
                    var index = section.GetValue<int>("index");

                    var diskStore = new DISKStore(diskfloder, webfloder,host)
                    {
                        Name = name,
                        Type = type,
                        Index = index
                    };

                    results.Add(diskStore);
                }
            }
            return results.ToArray();
        }
    }
}
