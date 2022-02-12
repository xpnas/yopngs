using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using NsfwSpyNS;

namespace Iimages.IStore
{
    public class StoreCheckNsfwWarper : IStoreCheck
    {

        private static NsfwSpy g_NsfwSpy;

        static StoreCheckNsfwWarper()
        {
            g_NsfwSpy = new NsfwSpy();
        }

        public StoreCheckNsfwWarper(IApplicationBuilder app, IConfiguration config)
        {

        }

        public bool PassSex(byte[] bytes)
        {
            var result = g_NsfwSpy.ClassifyImage(bytes);
            return !result.IsNsfw;
        }
    }
}
