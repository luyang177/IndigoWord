using System.Collections.Generic;

namespace IndigoWord.Render
{
    class SimpleLayerProvider : ILayerProvider
    {
        private readonly Dictionary<string, ILayer> _layers = new Dictionary<string, ILayer>(); 

        #region Implementation of ILayerProvider

        public void Register(string key, ILayer layer)
        {
            if (_layers.ContainsKey(key))
                return;

            _layers.Add(key, layer);
        }

        public ILayer Get(string key)
        {
            if (!_layers.ContainsKey(key))
                throw new KeyNotFoundException(string.Format("can not find key: {0} in LayerProvider", key));

            return _layers[key];
        }

        #endregion
    }
}
