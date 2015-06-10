using System;

namespace IndigoWord.Core
{
    /*
     * Represent line and column.
     */
    class TextPosition : IEquatable<TextPosition>
    {
        public TextPosition(int line, int column)
        {
            Line = line;
            Column = column;
        }

        public int Line { get; set; }

        public int Column { get; set; }

        /*
         * start from 1
         */
        public override string ToString()
        {
            return string.Format("Line: {0}, Column: {1}", Line + 1, Column + 1);
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
