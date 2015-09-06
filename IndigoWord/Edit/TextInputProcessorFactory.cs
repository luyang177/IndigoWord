using System;
using System.Windows.Input;

namespace IndigoWord.Edit
{
    class TextInputProcessorFactory
    {
        private Lazy<GeneralProcessor> GeneralProcessor { get; set; }

        private Lazy<EnterProcessor> EnterProcessor { get; set; }

        private Lazy<BackspaceProcessor> BackspaceProcessor { get; set; }

        private Lazy<DeleteProcessor> DeleteProcessor { get; set; } 

        public TextInputProcessorFactory()
        {
            GeneralProcessor = new Lazy<GeneralProcessor>( () => new GeneralProcessor());
            EnterProcessor = new Lazy<EnterProcessor>( () => new EnterProcessor());
            BackspaceProcessor = new Lazy<BackspaceProcessor>( () => new BackspaceProcessor());
            DeleteProcessor = new Lazy<DeleteProcessor>( () => new DeleteProcessor());
        }
        
        public TextInputProcessor Get(string text)
        {
            TextInputProcessor processor;

            if (text == "\r" || text == "\n" || text == "\r\n")
            {
                processor = EnterProcessor.Value;
            }
            else if (text == "\b")
            {
                processor = BackspaceProcessor.Value;
            }
            else
            {
                processor = GeneralProcessor.Value;
            }

            //Reset it
            processor.Reset();

            return processor;
        }

        public TextInputProcessor Get(Key key)
        {
            TextInputProcessor processor = null;

            if (key == Key.Delete)
            {
                processor = DeleteProcessor.Value;
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
