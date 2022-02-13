using Amazon.S3;
using Amazon.S3.Model;
using Iimages.IStore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace Iimages.Stores
{
    public class S3Store : StoreBase
    {
        AmazonS3Client Client;

        string Domain;

        public S3Store(string secretID, string secretKey, string region, string domain)
        {
            Domain = domain;
  
            var endPoint = new AmazonS3Config();
            endPoint.ServiceURL = region;
          //  endPoint.AuthenticationRegion = region;
            Client = new AmazonS3Client(secretID, secretKey, endPoint);
   

        }

        public override bool Up(byte[] maps, string localName,  string localUrl, ref string cdnUrl)
        {
           var files= Client.ListBucketsAsync().Result ;
            var putRequest2 = new PutObjectRequest
            {
                BucketName = "xpnas-assets",
                //获取和设置键属性。此键用于标识S3中的对象,上传到s3的路径+文件名，
                //S3上没有文件夹可以创建一个，参考https://www.cnblogs.com/web424/p/6840207.html
                Key = localName,
                //所有者获得完全控制权，匿名主体被授予读访问权。如果
                //此策略用于对象，它可以从浏览器中读取，无需验证
                CannedACL = S3CannedACL.PublicRead,
                //上传的文件路径
                FilePath = @"C:\Users\Administrator\Desktop\favicon.png",
                //为对象设置的标记。标记集必须编码为URL查询参数
                TagSet = new List<Tag>{ new Tag { Key = "Test", Value = "S3Test"} }
                //ContentType = "image/png"
            };
           // putRequest2.Metadata.Add("x-amz-meta-title", "AwsS3Net");
            PutObjectResponse response2 =  Client.PutObjectAsync(putRequest2).Result;
            return false;
        }

    }

    public class S3StoreFactory : IStoreFactory
    {
        public IDataStore[] GetStores(IApplicationBuilder app, IConfiguration configuration)
        {

            var results = new List<IDataStore>();
            var selection = configuration.GetSection("S3Stores");
            foreach (IConfigurationSection section in selection.GetChildren())
            {
                var active = section.GetValue<bool>("active");
                if (active)
                {
                    var secretID = section.GetValue<string>("secretID");
                    var secretKey = section.GetValue<string>("secretKey");
                    var region = section.GetValue<string>("region");
                    var domain = section.GetValue<string>("Domain");

                    var index = section.GetValue<int>("index");
                    var name = section.GetValue<string>("name");
                    var type = section.GetValue<string>("type");


                    var store = new S3Store(secretID, secretKey, region, domain)
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
