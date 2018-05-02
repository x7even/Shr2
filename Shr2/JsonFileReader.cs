using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;

namespace Shr2
{
    public class JsonFileReader<T>
    {
        public IEnumerable<T> ReadJsonFileasList(string path)
        {
            var jsonText = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<List<T>>(jsonText);
        }

        public T ReadJsonFile(string path)
        {
            var jsonText = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<T>(jsonText);
        }

    }
}
