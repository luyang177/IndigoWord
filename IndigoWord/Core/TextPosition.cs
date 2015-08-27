using System;

namespace IndigoWord.Core
{
    /*
     * Represent position in document
     * Line and Column are refer to LogiciLine.
     */
    class TextPosition : IEquatable<TextPosition>
    {
        public TextPosition(int line, int column)
        {
            Line = line;
            Column = column;
            IsAtEndOfLine = false;
        }

        public TextPosition(int line, int column, bool isAtEndOfLine)
        {
            Line = line;
            Column = column;
            IsAtEndOfLine = isAtEndOfLine;
        }

        public int Line { get; set; }

        public int Column { get; set; }

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
        public bool IsAtEndOfLine { get; set; }

        public override string ToString()
        {
            return string.Format("Line: {0}, Column: {1}", Line, Column);
        }

        #region Implementation of IEquatable<T>

        public override bool Equals(object obj)
        {
            return Equals(obj as TextPosition);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public bool Equals(TextPosition other)
        {
            // If parameter is null, return false. 
            if (ReferenceEquals(other, null))
            {
                return false;
            }

            // Optimization for a common success case. 
            if (ReferenceEquals(this, other))
            {
                return true;
            }

            if (GetType() != other.GetType())
                return false;

            // Check properties that this class declares. 
            // Note that the base class is not invoked because it is 
            // System.Object, which defines Equals as reference equality. 
            return EqualFields(other);

        }

        /*
         * Let operator == and != continue compare reference
         */
        //public static bool operator ==(Study lhs, Study rhs)
        //public static bool operator !=(Study lhs, Study rhs)

        private bool EqualFields(TextPosition other)
        {
            return Line == other.Line && Column == other.Column;
        }

        #endregion
    }
}
