using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.TextFormatting;

namespace IndigoWord.LowFontApi
{
    class TextFormatterFactory
    {
        private static TextFormatter _textFormatter;

        public static TextFormatter Get()
        {
            return _textFormatter ?? (_textFormatter = TextFormatter.Create());
        }

        public static void Reset()
        {
            _textFormatter.Dispose();
            _textFormatter = null;
        }
    }
}
