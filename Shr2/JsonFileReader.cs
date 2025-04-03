﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Text.Json;

namespace Shr2
{
    public class JsonFileReader<T>
    {
        private static readonly JsonSerializerOptions _options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };

        public IEnumerable<T> ReadJsonFileasList(string path)
        {
            var fullPath = Path.Combine(AppContext.BaseDirectory, path);
            var jsonText = File.ReadAllText(fullPath);
            return JsonSerializer.Deserialize<List<T>>(jsonText, _options) ?? new List<T>();
        }

        public T? ReadJsonFile(string path)
        {
            var fullPath = Path.Combine(AppContext.BaseDirectory, path);
            var jsonText = File.ReadAllText(fullPath);
            return JsonSerializer.Deserialize<T>(jsonText, _options);
        }
    }
}
