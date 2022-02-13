using B2Net;
using B2Net.Models;
using Iimages.IStore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace Iimages.Stores
{
    public class B2Store : StoreBase
    {

        private B2Client mClient = null;
        private B2Options mOptions;
        private bool mPersistBucket;
        private string mKeyId, mApplicationKey, mBucketId, mDomain;


        public B2Store(string keyId, string applicationKey, string bucketId, bool prsistBucket, string domain)
        {
            mKeyId = keyId;
            mApplicationKey = applicationKey;
            mBucketId = bucketId;
            mPersistBucket = prsistBucket;
            mDomain = domain;

            mOptions = new B2Options()
            {
                KeyId = mKeyId,
                ApplicationKey = mApplicationKey,
                BucketId = mBucketId,
                PersistBucket = mPersistBucket
            };


        }

        public override bool Up(byte[] maps, string localName, string localUrl, ref string cdnUrl)
        {
            lock (mClient)
            {
                try
                {
                    if (mClient == null)
                        mClient = new B2Client(mOptions);

                    return UploadBedAct(maps, localName, ref cdnUrl);
                }
                catch
                {
                    mClient = new B2Client(mOptions);
                    return UploadBedAct(maps, localName, ref cdnUrl);
                }
            }
        }


        private bool UploadBedAct(byte[] maps, string localName, ref string cdnUrl)
        {
            var result = mClient.Authorize().Result;
            if (result.Authenticated)
            {
                var timeTag = DateTime.Now.ToString("yyyy-MM-dd");
                var uploadUrl = mClient.Files.GetUploadUrl(mBucketId).Result;
                var fileInfo = mClient.Files.Upload(maps, timeTag + "_" + localName, uploadUrl).Result;
                var fileId = fileInfo.FileId;
                cdnUrl = string.Format("{0}/b2api/v1/b2_download_file_by_id?fileId={1}", mDomain, fileId);
                return true;
            }
            return false;
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
                    var index = section.GetValue<int>("index");
                    var name = section.GetValue<string>("name");
                    var type = section.GetValue<string>("type");

                    var store = new B2Store(keyId, applicationKey, bucketId, persistBucket, domain)
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
