using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

namespace Iimages.IStore
{
    public class SupportType
    {

        public string Type { get; set; }

        public string Name { get; set; }

        public int Index { get; set; }

        public SupportType(IDataStore store)
        {

            Name = store.Name;
            Type = store.Type;
            Index = store.Index;
        }
    }


    public interface IDataStore
    {
        int Index { get; set; }

        string Name { get; set; }

        string Type { get; set; }

        bool Up(byte[] maps, string localName, string localUrl, ref string cdnUrl);

    }

    public interface IStoreFactory
    {
        IDataStore[] GetStores(IApplicationBuilder app, IConfiguration configuration);

    }
}
