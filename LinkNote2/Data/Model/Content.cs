using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LinkNote2.Data.Model
{
    public class Content
    {
        public string Name { get; set; }

        public string Path { get; set; }

        public string Hash { get; set; }

        public bool Encrypted { get; set; }
    }
}
