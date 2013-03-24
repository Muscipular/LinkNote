using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LinkNote2.Data.Model
{
    public class DataPath
    {
        public string Id { get; set; }

        public string Parent { get; set; }

        public string Name { get; set; }

        public string Path { get; set; }

        public Content[] Contents { get; set; }
    }
}
