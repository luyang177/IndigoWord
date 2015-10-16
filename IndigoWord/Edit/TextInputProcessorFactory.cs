using System;
using System.Windows.Input;
using NUnit.Framework;

namespace IndigoWord.Edit
{
    class TextInputProcessorFactory
    {
        private Lazy<GeneralProcessor> GeneralProcessor { get; set; }

        private Lazy<EnterProcessor> EnterProcessor { get; set; }

        private Lazy<BackspaceProcessor> BackspaceProcessor { get; set; }

        private Lazy<DeleteProcessor> DeleteProcessor { get; set; }

        private Lazy<RemoveRangeProcessor> RemoveRangeProcessor { get; set; }

        private Lazy<GeneralWithRangeProcessor> GeneralWithRangeProcessor { get; set; } 

        public TextInputProcessorFactory()
        {
            GeneralProcessor = new Lazy<GeneralProcessor>( () => new GeneralProcessor());
            EnterProcessor = new Lazy<EnterProcessor>( () => new EnterProcessor());
            BackspaceProcessor = new Lazy<BackspaceProcessor>( () => new BackspaceProcessor());
            DeleteProcessor = new Lazy<DeleteProcessor>( () => new DeleteProcessor());
            RemoveRangeProcessor = new Lazy<RemoveRangeProcessor>( () => new RemoveRangeProcessor());
            GeneralWithRangeProcessor = new Lazy<GeneralWithRangeProcessor>( () => new GeneralWithRangeProcessor());
        }

        public TextInputProcessor Get(string text, bool isRange)
        {
            TextInputProcessor processor;

            if (text == "\r" || text == "\n" || text == "\r\n")
            {
                processor = EnterProcessor.Value;
            }
            else if (text == "\b")
            {
                if (isRange)
                {
                    processor = RemoveRangeProcessor.Value;
                }
                else
                {
                    processor = BackspaceProcessor.Value;
                }
            }
            else
            {
                if (isRange)
                {
                    processor = GeneralWithRangeProcessor.Value;
                }
                else
                {
                    processor = GeneralProcessor.Value;
                }
            }

            //Reset it
            processor.Reset();

            return processor;
        }

        public TextInputProcessor Get(Key key, bool isRange)
        {
            TextInputProcessor processor = null;

            if (key == Key.Delete)
            {
                if (isRange)
                {
                    processor = RemoveRangeProcessor.Value;
                }
                else
                {
                    processor = DeleteProcessor.Value;
                }
            }

            if (processor != null)
            {
                //Reset it
                processor.Reset();
            }

            return processor;
        }
    }
}
