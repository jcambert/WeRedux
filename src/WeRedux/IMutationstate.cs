using System;
using System.Collections.Generic;
using System.Text;

namespace WeRedux
{
    public interface IMutationstate<TState>
    {
        public string  Mutation { get; set; }

        public TState State { get; set; }


    }

    public class MutationState<TState> : IMutationstate<TState>
    {
        public string Mutation { get; set ; }
        public TState State { get; set; }
    }
}
