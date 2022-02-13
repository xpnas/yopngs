using Aliyun.OSS;
using Iimages.IStore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;

namespace Iimages.Stores
{
    public class OSSStore : StoreBase
    {
        private OssClient mOssClient;

        private string mDomain;


        public string Bucket { get; private set; }

        public OSSStore(string accessKeyId, string accessKeySecret, string endpoint, string bucket, string domian)
        {
            Bucket = bucket;
            mDomain = domian;
            mOssClient = new OssClient(endpoint, accessKeyId, accessKeySecret);
        }

        public override bool Up(byte[] maps,string localName, string localUrl, ref string cdnUrl)
        {
            using (var stream = new MemoryStream(maps))
            {
                try
                {
                    var cosPath = DateTime.Now.ToString("yyyy/MM/dd/") + localName;
                    mOssClient.PutObject(Bucket, cosPath, stream);
                    cdnUrl = string.Format("{0}/{1}", mDomain, cosPath);
                    return true;
                }
                catch
                {
                    return false;
                }
            }

        }
    }

    public class OSSFactory : IStoreFactory
    {
        public IDataStore[] GetStores(IApplicationBuilder app, IConfiguration configuration)
        {
            var results = new List<IDataStore>();
            var selection = configuration.GetSection("OSSStores");
            foreach (IConfigurationSection section in selection.GetChildren())
            {
                var active = section.GetValue<bool>("active");
                if (active)
                {
                    var acessKeyId = section.GetValue<string>("AccessKeyId");
                    var acessKeySecret = section.GetValue<string>("AccessKeySecret");
                    var endPoint = section.GetValue<string>("Endpoint");
                    var bucket = section.GetValue<string>("Bucket");

                    var domain = section.GetValue<string>("Domain");
                    var index = section.GetValue<int>("index");
                    var name = section.GetValue<string>("name");
                    var type = section.GetValue<string>("type");


                    var store = new OSSStore(acessKeyId, acessKeySecret, endPoint, bucket,domain)
                    {
                        Name = name,
                        Type = type,
                        Index = index
                    };

                    results.Add(store);
                }
            }

            return results.ToArray();
        }
    }
}
