using System;

namespace IndigoWord.Core
{
    /*
     * Represent position in document
     * Line and Column are refer to LogiciLine.
     * Notice: this struct is immutable.
     */
    struct TextPosition : IEquatable<TextPosition>, IComparable<TextPosition>
    {
        public TextPosition(int line, int column)
        {
            _line = line;
            _column = column;
            _isAtEndOfLine = false;
        }

        public TextPosition(int line, int column, bool isAtEndOfLine)
        {
            _line = line;
            _column = column;
            _isAtEndOfLine = isAtEndOfLine;
        }

        private readonly int _line;

        public int Line
        {
            get { return _line; }
        }

        private readonly int _column;
        

        public int Column
        {
            get { return _column; }
        }

        /*
         * When word-wrap is enabled and a line is wrapped at a position where there is no space character;
         * then both the end of the first TextLine and the beginning of the second TextLine
         * refer to the same position in the document, and also have the same Column.
         * In this case, the IsAtEndOfLine property is used to distinguish between the two cases:
         * the value <c>true</c> indicates that the position refers to the end of the previous TextLine;
         * the value <c>false</c> indicates that the position refers to the beginning of the next TextLine.
         * 
         * If this position is not at such a wrapping position, the value of this property has no effect.
         */

        private readonly bool _isAtEndOfLine;

        public bool IsAtEndOfLine
        {
            get { return _isAtEndOfLine; }
        }

        public override string ToString()
        {
            return string.Format("Line: {0}, Column: {1}", Line, Column);
        }

        #region Implementation of IEquatable<T>

        public override bool Equals(object obj)
        {
            return Equals((TextPosition)obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public bool Equals(TextPosition other)
        {
            if (GetType() != other.GetType())
                return false;

            // Check properties that this class declares. 
            // Note that the base class is not invoked because it is 
            // System.Object, which defines Equals as reference equality. 
            return EqualFields(other);

        }

        private bool EqualFields(TextPosition other)
        {
            return Line == other.Line && Column == other.Column;
        }

        public static bool operator ==(TextPosition objA, TextPosition objB)
        {
            return objA.Equals(objB);
        }

        public static bool operator !=(TextPosition objA, TextPosition objB)
        {
            return !objA.Equals(objB);
        }

        #endregion

        #region Implementation of IComparable<T>

        public int CompareTo(TextPosition other)
        {
            if (Line == other.Line)
            {
                return Column.CompareTo(other.Column);
            }
            else
            {
                return Line.CompareTo(other.Line);
            }
        }

        public static bool operator >(TextPosition objA, TextPosition objB)
        {
            return objA.CompareTo(objB) > 0;
        }

        public static bool operator <(TextPosition objA, TextPosition objB)
        {
            return objA.CompareTo(objB) < 0;
        }

        public static bool operator >=(TextPosition objA, TextPosition objB)
        {
            return objA.CompareTo(objB) >= 0;
        }

        public static bool operator <=(TextPosition objA, TextPosition objB)
        {
            return objA.CompareTo(objB) <= 0;
        }

        #endregion
    }
}
