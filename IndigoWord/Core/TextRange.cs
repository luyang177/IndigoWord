using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IndigoWord.Core
{
    /*
     * Range is [x,y)
     * 
     * TextRange is a mutable struct, using property is a trap
     * because we will call getter to get a copy(remember this is a struct), and change the copy, not the real one
     * so use field instead.
     */
    struct TextRange
    {
        public TextRange(TextPosition pos)
        {
            _anchor = pos;
            _secondPos = pos;
        }

        public TextRange(TextPosition start, TextPosition end)
        {
            _anchor = start;
            _secondPos = end;
        }

        private readonly TextPosition _anchor;
        private TextPosition _secondPos;

        public TextPosition Start
        {
            get { return _anchor <= _secondPos ? _anchor : _secondPos; }
        }

        public TextPosition End
        {
            get { return _anchor > _secondPos ? _anchor : _secondPos; }
        }        

        /*
         * Update range use the given pos
         */
        public void Change(TextPosition pos)
        {
            _secondPos = pos;
        }

        public bool IsRange
        {
            get { return !Start.Equals(End); }
        }

        public IEnumerable<int> Lines
        {
            get
            {
                for (int i = Start.Line; i <= End.Line; i++)
                {
                    yield return i;
                }
            }
        }

        public bool IsInRange(TextPosition pos)
        {
            return pos >= Start && pos <= End;
        }
    }
}
