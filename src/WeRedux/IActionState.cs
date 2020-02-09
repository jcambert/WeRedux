using System;
using System.Collections.Generic;
using System.Text;

namespace WeRedux
{
    public interface IActionState<TState, TAction>
    {

        //TState NewState { get; set; }

        TState State { get; set; }

        TAction Action { get; set; }
    }

    public class ActionState<TState, TAction> : IActionState<TState, TAction>
    {

        //public TState NewState { get; set; }

        public TState State { get; set; }

        public TAction Action { get; set; } = default;
    }
}
