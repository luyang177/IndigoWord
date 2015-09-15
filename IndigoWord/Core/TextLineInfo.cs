namespace IndigoWord.Core
{
    class TextLineInfo
    {
        public LogicLine LogicLine { get; set; }

        //vertical top position in its LogicLine
        public double Top { get; set; }

        //absolute vertical top position, include its LogicLine Top
        public double AbsoluteTop
        {
            get { return LogicLine.Top + Top; }
        }

        //start char position in its logic line
        public int StartCharPos { get; set; }

        //end char position in its logic line
        public int EndCharPos { get; set; }

        //is last text line in its logic line
        public bool IsLast { get; set; }
    }
}
