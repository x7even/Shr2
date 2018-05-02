using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shr2.Models;
using Shr2.Interfaces;

namespace Shr2
{
    public class JConfig : IConfig
    {
        private static Config config;
        //private static _loaded()

        public JConfig() { }

        public Config GetConfig()
        {
            if (config != null)
                return config;
            else
                return GetConfig("shr2.config.json");
        }

        public Config GetConfig(string path)
        {
            var jresult = new JsonFileReader<Config>().ReadJsonFile(path);
            if (jresult == null)
                throw new FormatException("Unable to read expected json config model in [" + path + "]");
            else
                return config = jresult;
        }
    }
}
