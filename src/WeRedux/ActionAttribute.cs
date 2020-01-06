using System;
using System.Collections.Generic;
using System.Text;

namespace WeRedux
{
    [AttributeUsage(AttributeTargets.Class,AllowMultiple =false,Inherited =false)]
    public class ActionAttribute:Attribute
    {
        public ActionAttribute(string name)
        {
            this.Name = name;
        }

        public string Name { get; }
    }
}
