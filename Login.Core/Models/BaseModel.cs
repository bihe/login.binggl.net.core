﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Login.Core.Models
{
    public abstract class BaseModel
    {
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
    }
}