﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shr2.Models
{
    public class Config
    {
        public string StorageConnectionString { get; set; } = string.Empty;

        public string StorageProvider { get; set; } = "AzTableStorage";

        public string Domain { get; set; } = string.Empty;

        public bool EncodeWithPermissionKey { get; set; }

        public string[] PermissionKeys { get; set; } = Array.Empty<string>();
    }
}
