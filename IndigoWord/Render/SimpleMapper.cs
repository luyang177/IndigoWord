using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace IndigoWord.Render
{
    public class SimpleMapper : IMapper
    {
        #region Constructor

        public SimpleMapper(IOffset offset)
        {
            if (offset == null)
            {
                throw new ArgumentNullException("IOffset");
            }

            _offset = offset;
        }

        #endregion

        #region Fields

        private readonly IOffset _offset;

        #endregion

        #region Private Methods

        private Matrix ProcessScreen2Origin()
        {
            var matrix = new Matrix();
            matrix.Translate(_offset.HorizontalOffset, _offset.VerticalOffset);

            return matrix;
        }

        #endregion

        #region Implementation of IMapper

        public Point MapScreen2Origin(Point point)
        {
            var matrix = ProcessScreen2Origin();
            return matrix.Transform(point);
        }

        #endregion
    }
}
