using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LinkNote2.Data
{
    [DbModel]
    public class DataPath
    {
        [DbModelField]
        public long Id { get; set; }

        [DbModelField]
        public long Parent { get; set; }

        [DbModelField]
        public string Name { get; set; }
    }
}
