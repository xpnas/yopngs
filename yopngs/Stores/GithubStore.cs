using Iimages.IStore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Octokit;
using System.Collections.Generic;
using System.IO;

namespace Iimages.Stores
{
    public class GithubStore : StoreBase
    {
        public override bool Up(byte[] maps, string localName, string localUrl, ref string cdnUrl)
        {
            var client = new GitHubClient(new ProductHeaderValue("my-cool-app"));
            var basicAuth = new Credentials("username", "password"); // NOTE: not real credentials
            client.Credentials = basicAuth;

            using (var archiveContents = File.OpenRead("output.zip"))
            { // TODO: better sample
                var assetUpload = new ReleaseAssetUpload()
                {
                    FileName = "my-cool-project-1.0.zip",
                    ContentType = "application/zip",
                    RawData = archiveContents
                };
                var repository = client.Repository.Release.Get("octokit", "octokit.net", 1).Result;
                var asset = client.Repository.Release.UploadAsset(repository, assetUpload).Result;
            }

            return true;
        }
    }
    public class GithubFactory : IStoreFactory
    {
        public IDataStore[] GetStores(IApplicationBuilder app, IConfiguration configuration)
        {
            var results = new List<IDataStore>();
            var selection = configuration.GetSection("OSSStores");
            foreach (IConfigurationSection section in selection.GetChildren())
            {
                //var active = section.GetValue<bool>("active");
                //if (active)
                //{
                //    var acessKeyId = section.GetValue<string>("AccessKeyId");
                //    var acessKeySecret = section.GetValue<string>("AccessKeySecret");
                //    var endPoint = section.GetValue<string>("Endpoint");
                //    var bucket = section.GetValue<string>("Bucket");

                //    var domain = section.GetValue<string>("Domain");
                //    var index = section.GetValue<int>("index");
                //    var name = section.GetValue<string>("name");
                //    var type = section.GetValue<string>("type");


                //    var diskStore = new OSSStore(acessKeyId, acessKeySecret, endPoint, bucket, domain)
                //    {
                //        Name = name,
                //        Type = type,
                //        Index = index
                //    };

                //    results.Add(diskStore);
                //}
            }

            return results.ToArray();
        }
    }
}
