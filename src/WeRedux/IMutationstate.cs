using System;

namespace WeRedux
{
    public interface IMutationstate<TState>
    {
        public string Mutation { get; set; }

        public TState State { get; set; }

        public Type Action { get; set; }
    }

    public class MutationState<TState> : IMutationstate<TState>
    {
        public string Mutation { get; set; }
        public TState State { get; set; }
        public Type Action{ get; set; }
    }
}
