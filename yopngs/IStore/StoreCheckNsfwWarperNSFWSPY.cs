using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using NsfwSpyNS;

namespace Iimages.IStore
{
    public class StoreCheckNsfwWarperNSFWSPY : IStoreCheck
    {

        private static NsfwSpy g_NsfwSpy;

        static StoreCheckNsfwWarperNSFWSPY()
        {
            g_NsfwSpy = new NsfwSpy();
        }

        public StoreCheckNsfwWarperNSFWSPY(IApplicationBuilder app, IConfiguration config)
        {

        }

        public bool PassSex(byte[] bytes)
        {
            var result = g_NsfwSpy.ClassifyImage(bytes);
            return !result.IsNsfw;
        }
    }
}
