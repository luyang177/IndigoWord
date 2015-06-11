using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IndigoWord.Utility;

namespace IndigoWord.Core
{
    class CommonSetting
    {
        #region Setting Properties

        public string LatestDocPath { get; set; }

        #endregion

        #region Implementation

        private CommonSetting()
        {
            
        }

        private readonly static string Path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "setting.conf");

        private static CommonSetting _instance;

        public static CommonSetting Instance
        {
            get
            {
                if (_instance == null)
                {

                    if (!File.Exists(Path))
                    {
                        _instance = new CommonSetting();
                        Save(_instance, Path);                        
                    }
                    else
                    {
                        _instance = Load(Path);
                    }
                }

                return _instance;
            }
        }

        public static void Save()
        {
            SerializerHelper.SaveToFile(Instance, Path);
        }

        private static CommonSetting Load(string path)
        {
            return SerializerHelper.LoadFromFile<CommonSetting>(path);
        }

        private static void Save(CommonSetting setting, string path)
        {
            SerializerHelper.SaveToFile(setting, path);
        }

        #endregion
    }
}
