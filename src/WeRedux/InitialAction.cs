using System;
using System.Collections.Generic;
using System.Text;

namespace WeRedux
{
  
    public class InitialAction : IAction, IStaticMutation
    {
        public InitialAction()
        {

        }
        public string Mutation =>"@@INIT";
    }
}
