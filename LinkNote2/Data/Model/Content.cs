using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LinkNote2.Data.Model
{
    [DbModel(Table = "Contents")]
    public class Content
    {
        [DbModelField]
        [DbModelKeyField]
        public long Id { get; set; }
        
        [DbModelField]
        public long Parent { get; set; }

        [DbModelField]
        public string Name { get; set; }

        public string Path { get; set; }

        [DbModelField]
        public string Hash { get; set; }

        [DbModelField]
        public byte[] Data { get; set; }

        [DbModelField]
        public bool Encrypted { get; set; }

        [DbModelField]
        public DateTime LastModify { get; set; }
    }
}
