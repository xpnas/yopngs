using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Minimage;

namespace Iimages.IStore
{
    public class StoreCompressPngQuantWarper : IStoreCompress
    {

        private static PngQuant g_PngQuant;


        static StoreCompressPngQuantWarper()
        {
            var options = new PngQuantOptions()
            {
                QualityMinMax = (65, 80),
                IEBug = false,
                Bit = 256
            };
            g_PngQuant = new PngQuant(options);

        }


        public StoreCompressPngQuantWarper(IApplicationBuilder app, IConfiguration config)
        {

        }

        public byte[] Compress(byte[] maps)
        {
            try
            {
                return g_PngQuant.Compress(maps).Result;
            }
            catch
            {
                return maps;
            }
           
        }
    }
}
