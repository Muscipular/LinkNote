using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LinkNote2.Data.Model
{
    [DbModel(Table = "Settings")]
    public class SettingItem
    {
        [DbModelField]
        [DbModelKeyField]
        public string Key { get; set; }
        [DbModelField]
        public string Value { get; set; }
    }
}
