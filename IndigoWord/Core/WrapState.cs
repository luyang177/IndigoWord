using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace IndigoWord.Core
{
    class WrapState
    {
        private static bool _isInstantiated = false;

        public WrapState()
        {
            if (_isInstantiated)
            {
                var error =
                    "WrapState should be singleton now untill we need more than one editor(Not implementation yet)";
                MessageBox.Show(error);
                throw new ArgumentException(error);
            }

            _isInstantiated = true;
        }

        private bool _isWrap;

        public bool IsWrap
        {
            get { return _isWrap; }
            set
            {
                if (_isWrap == value)
                {
                    return;
                }

                _isWrap = value;
                RaiseChanged();
            }
        }

        public event Action Changed;

        private void RaiseChanged()
        {
            var handler = Changed;
            if (handler != null)
            {
                handler();
            }
        }
    }
}
