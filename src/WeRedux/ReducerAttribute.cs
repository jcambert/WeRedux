using System;
using System.Collections.Generic;
using System.Text;

namespace WeRedux
{
    [AttributeUsage(AttributeTargets.Class,AllowMultiple =true,Inherited =true)]
    public class ReducerAttribute : Attribute
    {
        public ReducerAttribute(Type t)
        {

        }
    }
}
