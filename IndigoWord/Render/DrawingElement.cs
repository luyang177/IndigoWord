using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using IndigoWord.Core;

namespace IndigoWord.Render
{
    class DrawingElement
    {
        #region Constructor

        public DrawingElement(LogicLine logicLine)
        {
            if(logicLine == null)
                throw new ArgumentNullException("logicLine");

            LogicLine = logicLine;

            Visual = new DrawingVisual();
        }

        #endregion

        #region Public Properties

        public DrawingVisual Visual { get; set; }

        public double Height { get; set; }

        #endregion

        #region Public Methods

        public bool Exist(LogicLine logicLine)
        {
            return LogicLine == logicLine;
        }

        #endregion

        #region Private Properties and Fields

        /*
         * DrawingElement knows LogicLine, but LogicLine doesn't know DrawingElement at all.
         * Means one LogicLine can render on many DrawingElements
         */
        private LogicLine LogicLine { get; set; }

        #endregion
    }
}
