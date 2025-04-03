﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Shr2.Models
{
    public class GStyleRequest
    {
        [Required]
        [Url]
        public string LongUrl { get; set; } = string.Empty;
    }
}
