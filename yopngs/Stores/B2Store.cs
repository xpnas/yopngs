using B2Net;
using B2Net.Models;
using Iimages.IStore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Iimages.Stores
{
    public class B2Store : StoreBase
    {

        private bool mPersistBucket, mSafe;
        private string mKeyId, mApplicationKey, mBucketId, mDomain;
 


        public B2Store(string keyId, string applicationKey, string bucketId, bool prsistBucket, string domain,bool safe)
        {
            mKeyId = keyId;
            mApplicationKey = applicationKey;
            mBucketId = bucketId;
            mPersistBucket = prsistBucket;
            mDomain = domain;
            mSafe = safe;
        }

        public override bool Up(byte[] maps, string localName, string localUrl, ref string cdnUrl)
        {
            try
            {
                var mOptions = new B2Options()
                {
                    KeyId = mKeyId,
                    ApplicationKey = mApplicationKey,
                    BucketId = mBucketId,
                    PersistBucket = mPersistBucket
                };
                var mClient = new B2Client(mOptions);
                var result = mClient.Authorize().Result;
                if (result.Authenticated)
                {
                    var timeTag = DateTime.Now.ToString("yyyy/MM/dd/");
                    var uploadUrl = mClient.Files.GetUploadUrl(mBucketId).Result;
                    var fileInfo = mClient.Files.Upload(maps, timeTag + localName, uploadUrl).Result;
                    var fileId = fileInfo.FileId;
                    if (mSafe)
                    {
                        cdnUrl = string.Format("{0}/{1}", mDomain, timeTag + localName);
                      
                    }
                    else
                    {
                        cdnUrl = string.Format("{0}/file/{1}/{2}", mDomain, result.Capabilities.BucketName, timeTag + localName);
                    }

                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }
    }


    public class B2StoreFactory : IStoreFactory
    {
        public IDataStore[] GetStores(IApplicationBuilder app, IConfiguration configuration)
        {
            var results = new List<IDataStore>();
            var selection = configuration.GetSection("B2Stores");
            foreach (IConfigurationSection section in selection.GetChildren())
            {
                var active = section.GetValue<bool>("active");
                if (active)
                {
                    var keyId = section.GetValue<string>("KeyId");
                    var applicationKey = section.GetValue<string>("ApplicationKey");
                    var bucketId = section.GetValue<string>("BucketId");
                    var persistBucket = section.GetValue<bool>("PersistBucket");
                    var domain = section.GetValue<string>("Domain");
                    var safe = section.GetValue<bool>("Safe");

                    var index = section.GetValue<int>("index");
                    var name = section.GetValue<string>("name");
                    var type = section.GetValue<string>("type");
                    var store = new B2Store(keyId, applicationKey, bucketId, persistBucket, domain, safe)
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
