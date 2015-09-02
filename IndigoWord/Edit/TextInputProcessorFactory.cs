using System;

namespace IndigoWord.Edit
{
    class TextInputProcessorFactory
    {
        private Lazy<GeneralProcessor> GeneralProcessor { get; set; }

        private Lazy<EnterProcessor> EnterProcessor { get; set; }

        private Lazy<BackspaceProcessor> BackspaceProcessor { get; set; }

        public TextInputProcessorFactory()
        {
            GeneralProcessor = new Lazy<GeneralProcessor>( () => new GeneralProcessor());
            EnterProcessor = new Lazy<EnterProcessor>( () => new EnterProcessor());
            BackspaceProcessor = new Lazy<BackspaceProcessor>( () => new BackspaceProcessor());
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
    }
}
