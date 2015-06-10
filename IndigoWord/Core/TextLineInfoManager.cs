using System.Collections.Generic;
using System.Windows.Media.TextFormatting;

namespace IndigoWord.Core
{
    /*
     * TODO consider remove static
     */
    static class TextLineInfoManager
    {
        private readonly static Dictionary<TextLine, TextLineInfo> _infos = new Dictionary<TextLine, TextLineInfo>();

        public static void Add(TextLine textLine, TextLineInfo info)
        {
            _infos.Add(textLine, info);
        }

        public static TextLineInfo Get(TextLine textLine)
        {
            return _infos[textLine];
        }

        public static void Clear()
        {
            _infos.Clear();
        }
    }
}
