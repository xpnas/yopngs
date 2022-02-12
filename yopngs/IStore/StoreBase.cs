namespace Iimages.IStore
{
    public abstract class StoreBase : IDataStore
    {
        public int Index { get; set; }

        public string Name { get; set; }

        public string Type { get; set; }

        public abstract bool Up(byte[] maps, string localName, string localUrl, ref string cdnUrl);

    }
}
