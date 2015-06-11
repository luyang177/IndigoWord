using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace IndigoWord.Utility
{
    static class SerializerHelper
    {
        public static T LoadFromFile<T>(string filename)
        {
            try
            {
                var serializer = new JsonSerializer();
                //{
                //    NullValueHandling = NullValueHandling.Ignore
                //};
                serializer.Converters.Add(new StringEnumConverter());
                using (var textReader = new StreamReader(filename))
                using (var reader = new JsonTextReader(textReader))
                    return serializer.Deserialize<T>(reader);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }

            return default(T);
        }

        public static void SaveToFile<T>(T obj, string filename)
        {
            try
            {
                var serializer = new JsonSerializer();
                //{
                //    NullValueHandling = NullValueHandling.Ignore
                //};
                serializer.Converters.Add(new StringEnumConverter());
                using (var textWriter = new StreamWriter(filename))
                using (var writer = new JsonTextWriter(textWriter)
                                    {
                                        QuoteName = false,
                                        Formatting = Formatting.Indented
                                    })
                    serializer.Serialize(writer, obj);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

    }
}
