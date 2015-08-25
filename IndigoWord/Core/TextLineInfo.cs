namespace IndigoWord.Core
{
    class TextLineInfo
    {
        //vertical top position in its logic line
        public double Top { get; set; }

        //start char position in its logic line
        public int StartCharPos { get; set; }

        //end char position in its logic line
        public int EndCharPos { get; set; }

        //is last text line in its logic line
        public bool IsLast { get; set; }
    }
}
