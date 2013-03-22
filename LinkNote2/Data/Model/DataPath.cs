using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LinkNote2.Data.Model
{
    [DbModel(Table = "Index")]
    public class DataPath
    {
        [DbModelField]
        [DbModelKeyField]
        public long Id { get; set; }

        [DbModelField]
        public long Parent { get; set; }

        [DbModelField]
        public string Name { get; set; }

        [DbModelField]
        public DateTime LastModify { get; set; }
    }
}
