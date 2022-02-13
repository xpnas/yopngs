using COSXML;
using COSXML.Auth;
using COSXML.Transfer;
using Iimages.IStore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace Iimages.Stores
{
    public class COSStore : StoreBase
    {

        private CosXml mCosXml;

        private string mBucket, mDomain;

        public COSStore(string bucket, string region, string secretId, string secretKey,string domain)
        {
            long durationSecond = 600;
            var config = new CosXmlConfig.Builder().IsHttps(true).SetRegion(region).SetDebugLog(true).Build();
            QCloudCredentialProvider cosCredentialProvider = new DefaultQCloudCredentialProvider(secretId, secretKey, durationSecond);
            mCosXml = new CosXmlServer(config, cosCredentialProvider);
            mBucket = bucket;
            mDomain = domain;
        }

        public override bool Up(byte[] maps, string localName, string localUrl, ref string cdnUrl)
        {
            try
            { 
                var cosPath = DateTime.Now.ToString("/yyyy/MM/dd/") + localName;
                var putObjectRequest = new COSXML.Model.Object.PutObjectRequest(mBucket, cosPath, maps);
                var uploadTask = new COSXMLUploadTask(new COSXML.Model.Object.PutObjectRequest(mBucket, cosPath, maps));
                COSXML.Model.Object.PutObjectResult result = mCosXml.PutObject(putObjectRequest);
            
                if (result.IsSuccessful())
                {
                    cdnUrl = string.Format("{0}{1}", mDomain, cosPath);
                    return true;
                }
               
            }
            catch (Exception e)
            {
                Console.WriteLine("CosException: " + e);
            }

            return false;
        }
    }

    public class CosStoreFactory : IStoreFactory
    {
        public IDataStore[] GetStores(IApplicationBuilder app, IConfiguration configuration)
        {
            var results = new List<IDataStore>();
            var selection = configuration.GetSection("COSStores");
            foreach (IConfigurationSection section in selection.GetChildren())
            {
                var active = section.GetValue<bool>("active");
                if (active)
                {
                    var region = section.GetValue<string>("region");
                    var bucket = section.GetValue<string>("bucket");

                    var secretId = section.GetValue<string>("SECRET_ID");
                    var secretKey = section.GetValue<string>("SECRET_KEY");

                    var domain = section.GetValue<string>("Domain");

                    var index = section.GetValue<int>("index");
                    var name = section.GetValue<string>("name");
                    var type = section.GetValue<string>("type");


                    var store = new COSStore(bucket, region, secretId, secretKey, domain)
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
