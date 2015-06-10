using System.Windows.Media;

namespace IndigoWord.Render
{
    interface ILayer
    {
        void Add(Visual visual);

        void Remove(Visual visual);

        void Clear();
    }
}
