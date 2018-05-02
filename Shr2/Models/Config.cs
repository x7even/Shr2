using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shr2.Models
{
    public class Config
    {
        public string StorageConnectionString { get; set; }

        public string StorageProvider { get; set; }

        public string Domain { get; set; }
    }
}
