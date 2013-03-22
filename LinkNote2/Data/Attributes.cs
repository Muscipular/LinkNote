using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LinkNote2.Data
{
    class DbModelAttribute : Attribute
    {
        public string Table { get; set; }
    } 
    
    class DbModelFieldAttribute : Attribute
    {
        public string FieldName { get; set; }
    }
    
    class DbModelKeyFieldAttribute : Attribute
    {
    }
}
