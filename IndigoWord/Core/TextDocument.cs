using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.TextFormatting;
using System.Windows.Shapes;
using IndigoWord.Utility;
using Microsoft.Win32;
using NUnit.Framework;

namespace IndigoWord.Core
{
    class TextDocument : IDisposable
    {
        #region Constructor

        public TextDocument(IEnumerable<LogicLine> lines)
        {
            Debug.Assert(_lines.Count == 0);

            foreach (var line in lines)
            {
                AddLogicLine(line);
            }

            Debug.Assert(_lines.Count != 0);
        }

        public static TextDocument Empty()
        {
            var doc = new TextDocument();

            Debug.Assert(doc._lines.Count == 0);

            var line = new LogicLine(0, Environment.NewLine);
            doc.AddLogicLine(line);

            Debug.Assert(doc._lines.Count == 1);

            return doc;
        }

        public static TextDocument Open(string path)
        {
            if (!File.Exists(path))
            {
                MessageBox.Show("File: {0} isn't exist", path);
                return null;
            }

            var lines = new List<LogicLine>();
            using (var sr = File.OpenText(path))
            {
                var s = "";
                int line = 0;
                while ((s = sr.ReadLine()) != null)
                {
                    //ensure caret can reach the right of the last character
                    s += Environment.NewLine;

                    var logicLine = new LogicLine(line, s);
                    lines.Add(logicLine);

                    line++;
                }
            }

            CommonSetting.Instance.LatestDocPath = path;
            CommonSetting.Save();

            if (lines.Count == 0)
            {
                return Empty();
            }
            else
            {
                var document = new TextDocument(lines);
                return document;
            }            
        }

        public static TextDocument Open(IList<string> stringData)
        {
            if (stringData == null)
            {
                throw new ArgumentNullException("stringData");
            }

            if (!stringData.Any())
            {
                return Empty();
            }

            var lines = new List<LogicLine>();
            int line = 0;
            foreach (var str in stringData)
            {
                var s = str + Environment.NewLine;
                var logicLine = new LogicLine(line, s);
                lines.Add(logicLine);

                line++;
                
            }

            var document = new TextDocument(lines);
            return document;                     
        }

        private TextDocument()
        {
            
        }

        #endregion

        #region Public Properties

        public IList<LogicLine> Lines
        {
            get { return _lines.Select(li => li.Value).ToList(); }
        }

        public TextPosition FirstPosition
        {
            get
            {
                return new TextPosition(0, 0);
            }
        }

        public TextPosition LastPosition
        {
            get { return new TextPosition(_lines.Last().Key, _lines.Last().Value.GetLength() - 1); }
        }

        #endregion

        #region Public Methods

        public bool Contains(int line)
        {
            return _lines.ContainsKey(line);
        }

        public bool Contains(LogicLine logicLine)
        {
            return _lines.ContainsValue(logicLine);
        }

        public LogicLine FindLogicLine(int line)
        {
            return _lines[line];
        }

        public LogicLine FindLogicLine(Point point)
        {
            var lastLine = Lines.Last();
            if (point.Y >= lastLine.Bottom)
            {
                return lastLine;
            }

            var findLine = Lines.FirstOrDefault(line => (point.Y >= line.Top) && (point.Y <= line.Bottom));
            if (findLine == null)
            {
                throw new NullReferenceException("findLine, perhaps the TextLines which in this Lines count is 0, cause the line.Top == line.Bottom");
            }

            return findLine;
        }

        /*
         * Guarantee the return TextPosition is always valid.
         */
        public TextPosition GetPreviousTextPosition(TextPosition position)
        {
            var previous = DoGetPreviousTextPosition(position);
            Debug.Assert(VerifyTextPosition(previous));
            return previous;
        }

        /*
         * Guarantee the return TextPosition is always valid.
         */
        public TextPosition GetNextTextPosition(TextPosition position)
        {
            var next = DoGetNextTextPosition(position);
            Debug.Assert(VerifyTextPosition(next));
            return next;
        }

        /*
         * Guarantee the return TextPosition is always valid.
         */
        public TextPosition GetDownLineTextPosition(TextPosition position, Rect caretRect)
        {
            var pos = DoGetVerticalMoveTextPosition(position, caretRect, PositionMoveType.Down);
            Debug.Assert(VerifyTextPosition(pos));
            return pos;
        }

        /*
         * Guarantee the return TextPosition is always valid.
         */
        public TextPosition GetUpLineTextPosition(TextPosition position, Rect caretRect)
        {
            var pos = DoGetVerticalMoveTextPosition(position, caretRect, PositionMoveType.Up);
            Debug.Assert(VerifyTextPosition(pos));
            return pos;
        }

        public bool VerifyTextPosition(TextPosition position)
        {
            if (!Contains(position.Line))
                return false;

            var logicLine = FindLogicLine(position.Line);
            return logicLine.Exist(position);
        }

        /*
         * Insert newLines from the index
         */
        public void InsertLines(int index, IList<LogicLine> insertLines)
        {
            if (!insertLines.Any())
            {
                return;
            }

            _lines.InsertAndShift(index, insertLines);

            for (int i = index; i < _lines.Count; i++)
            {
                _lines[i].Line = i;
            }
        }

        public void RemoveLines(int index, int size)
        {
            for (int i = 0; i < size; i++)
            {
                var logicLine = _lines[index];
                logicLine.Dispose();
                TextLineInfoManager.Remove(logicLine);
            }       

            DoRemoveLines(index, size);
        }

        public void Save(string path)
        {
            using (var sw = new StreamWriter(path))
            {
                foreach (var line in _lines.Select(pair => pair.Value))
                {
                    sw.Write(line.Text);
                }
            }

            CommonSetting.Instance.LatestDocPath = path;
            CommonSetting.Save();
        }

        #endregion

        #region Private Methods

        private void AddLogicLine(LogicLine line)
        {
            if (!line.Text.EndsWith("\r\n"))
            {
                throw new Exception("LogicLine must end with \r\n");
            }

            _lines.Add(line.Line, line);
        }

        private TextPosition DoGetPreviousTextPosition(TextPosition position)
        {
            var line = position.Line;
            var previous = new TextPosition(line, position.Column - 1);

            var logicLine = FindLogicLine(line);
            if (logicLine.Exist(previous))
            {
                return previous;
            }
            else
            {
                var newLine = line - 1;
                if (Contains(newLine))
                {
                    //jump to previous line
                    var previousLogicLine = FindLogicLine(newLine);
                    return new TextPosition(newLine, previousLogicLine.GetLength() - 1);
                }
                else
                {
                    //since position is already the first line and the first column
                    return position;
                }
            }
        }

        private TextPosition DoGetNextTextPosition(TextPosition position)
        {
            var line = position.Line;
            var next = new TextPosition(line, position.Column + 1);

            var logicLine = FindLogicLine(line);
            if (logicLine.Exist(next))
            {
                return next;
            }
            else
            {
                var newLine = line + 1;
                if (Contains(newLine))
                {
                    //jump to next line
                    return new TextPosition(newLine, 0);
                }
                else
                {
                    //since position is already the last line and the last column
                    return position;
                }
            }
        }

        private TextPosition DoGetVerticalMoveTextPosition(TextPosition position, Rect caretRect, PositionMoveType type)
        {
            if (position.IsAtEndOfLine)
            {
                position = new TextPosition(position.Line, position.Column - 1, false);
            }

            var currentLogicLine = FindLogicLine(position.Line);

            //VerticalMove can not move to the position which isAtEndOfLine is true
            var currentTextLine = currentLogicLine.FindTextLine(position.Column, false);    

            TextLine nextTextLine;
            if (type == PositionMoveType.Down)
            {
                nextTextLine = currentLogicLine.GetNextTextLine(currentTextLine);
            }
            else if (type == PositionMoveType.Up)
            {
                nextTextLine = currentLogicLine.GetPreviousTextLine(currentTextLine);
            }
            else
            {
                throw new ArgumentException("PositionMoveType");
            }

            int nextLine = position.Line;
            if (nextTextLine == null)
            {
                if (type == PositionMoveType.Down)
                {
                    nextLine = position.Line + 1;
                }
                else if (type == PositionMoveType.Up)
                {
                    nextLine = position.Line - 1;
                }

                if (!Contains(nextLine))
                {
                    //already first or last text line
                    return position;
                }

                var downLogicLine = FindLogicLine(nextLine);

                if (type == PositionMoveType.Down)
                {
                    nextTextLine = downLogicLine.TextLines.First();
                }
                else if (type == PositionMoveType.Up)
                {
                    nextTextLine = downLogicLine.TextLines.Last();
                }
                
            }

            var col = nextTextLine.FindClosestColumn(caretRect, false);
            return new TextPosition(nextLine, col);
        }

        public void DoRemoveLines(int index, int size)
        {
            _lines.RemoveAndShift(index, size);

            for (int i = index; i < _lines.Count; i++)
            {
                var logicLine = _lines[i];
                logicLine.Line = i;
            }
        }

        #endregion

        #region Private Properties and Fields

        private readonly Dictionary<int, LogicLine> _lines = new Dictionary<int, LogicLine>();

        #endregion

        #region Implementation of IDisposable

        public void Dispose()
        {
            foreach (var line in Lines)
            {
                line.Dispose();
            }
        }

        #endregion
    }

    #region Tests

    [TestFixture]
    class TextDocumentTest
    {
        /*
         * Document Lines is below:( notice \r\n )
            "This is not possible with Dictionary<TKey, TValue> \r\n",
            "as it presents it's values in an unordered fashion \r\n",
            "when enumerated. There is SortedDictionary<TKey, TValue>\r\n",
            "which provides ordering but it does so by using an\r\n",
            "IComparer<TKey> against the key value directly.\r\n"
         */

        private TextDocument Document { get; set; }
        private readonly string[] _textData =
        {
            "This is not possible with Dictionary<TKey, TValue> ",
            "as it presents it's values in an unordered fashion ",
            "when enumerated. There is SortedDictionary<TKey, TValue>",
            "which provides ordering but it does so by using an",
            "IComparer<TKey> against the key value directly."
        };

        [SetUp]
        public void Init()
        {
            Document = TextDocument.Open(_textData);
        }

        private void TestMemberLine()
        {
            int n = 0;
            foreach (var logicLine in Document.Lines)
            {
                Assert.AreEqual(n, logicLine.Line);
                n++;
            }
        }

        [Test]
        public void RemoveLines_1()
        {
            Document.DoRemoveLines(0, 1);

            string[] expectedResults =
            {
                "as it presents it's values in an unordered fashion \r\n",
                "when enumerated. There is SortedDictionary<TKey, TValue>\r\n",
                "which provides ordering but it does so by using an\r\n",
                "IComparer<TKey> against the key value directly.\r\n"
            };

            Assert.AreEqual(4, Document.Lines.Count);
            Assert.AreEqual(expectedResults[0], Document.Lines[0].Text);
            Assert.AreEqual(expectedResults[1], Document.Lines[1].Text);
            Assert.AreEqual(expectedResults[2], Document.Lines[2].Text);
            Assert.AreEqual(expectedResults[3], Document.Lines[3].Text);

            TestMemberLine();
        }

        [Test]
        public void RemoveLines_2()
        {
            Document.DoRemoveLines(1, 1);

            string[] expectedResults =
            {
                "This is not possible with Dictionary<TKey, TValue> \r\n",
                "when enumerated. There is SortedDictionary<TKey, TValue>\r\n",
                "which provides ordering but it does so by using an\r\n",
                "IComparer<TKey> against the key value directly.\r\n"
            };

            Assert.AreEqual(4, Document.Lines.Count);
            Assert.AreEqual(expectedResults[0], Document.Lines[0].Text);
            Assert.AreEqual(expectedResults[1], Document.Lines[1].Text);
            Assert.AreEqual(expectedResults[2], Document.Lines[2].Text);
            Assert.AreEqual(expectedResults[3], Document.Lines[3].Text);

            TestMemberLine();
        }

        [Test]
        public void RemoveLines_3()
        {
            Document.DoRemoveLines(2, 1);

            string[] expectedResults =
            {
                "This is not possible with Dictionary<TKey, TValue> \r\n",
                "as it presents it's values in an unordered fashion \r\n",
                "which provides ordering but it does so by using an\r\n",
                "IComparer<TKey> against the key value directly.\r\n"
            };

            Assert.AreEqual(4, Document.Lines.Count);
            Assert.AreEqual(expectedResults[0], Document.Lines[0].Text);
            Assert.AreEqual(expectedResults[1], Document.Lines[1].Text);
            Assert.AreEqual(expectedResults[2], Document.Lines[2].Text);
            Assert.AreEqual(expectedResults[3], Document.Lines[3].Text);

            TestMemberLine();
        }

        [Test]
        public void RemoveLines_4()
        {
            Document.DoRemoveLines(4, 1);

            string[] expectedResults =
            {
                "This is not possible with Dictionary<TKey, TValue> \r\n",
                "as it presents it's values in an unordered fashion \r\n",
                "when enumerated. There is SortedDictionary<TKey, TValue>\r\n",
                "which provides ordering but it does so by using an\r\n",
            };

            Assert.AreEqual(4, Document.Lines.Count);
            Assert.AreEqual(expectedResults[0], Document.Lines[0].Text);
            Assert.AreEqual(expectedResults[1], Document.Lines[1].Text);
            Assert.AreEqual(expectedResults[2], Document.Lines[2].Text);
            Assert.AreEqual(expectedResults[3], Document.Lines[3].Text);

            TestMemberLine();
        }

        [Test]
        public void RemoveLines_5()
        {
            Document.DoRemoveLines(0, 3);

            string[] expectedResults =
            {
                "which provides ordering but it does so by using an\r\n",
                "IComparer<TKey> against the key value directly.\r\n"
            };

            Assert.AreEqual(2, Document.Lines.Count);
            Assert.AreEqual(expectedResults[0], Document.Lines[0].Text);
            Assert.AreEqual(expectedResults[1], Document.Lines[1].Text);

            TestMemberLine();
        }

        [Test]
        public void RemoveLines_6()
        {
            Document.DoRemoveLines(2, 3);

            string[] expectedResults =
            {
                "This is not possible with Dictionary<TKey, TValue> \r\n",
                "as it presents it's values in an unordered fashion \r\n",
            };

            Assert.AreEqual(2, Document.Lines.Count);
            Assert.AreEqual(expectedResults[0], Document.Lines[0].Text);
            Assert.AreEqual(expectedResults[1], Document.Lines[1].Text);

            TestMemberLine();
        }

        [Test]
        public void RemoveLines_7()
        {
            Document.DoRemoveLines(0, 5);

            Assert.AreEqual(0, Document.Lines.Count);
        }
    }

    #endregion
}
