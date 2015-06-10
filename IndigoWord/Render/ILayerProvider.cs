namespace IndigoWord.Render
{
    interface ILayerProvider
    {
        void Register(string key, ILayer layer);
        ILayer Get(string key);
    }
}
