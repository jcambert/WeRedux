using System;
using System.Collections.Generic;
using System.Text;

namespace WeRedux.Samples
{
    //[Reducer(typeof(CounterState))]
    public class CounterReducer : Reducer<CounterState, IAction>
    {

        

        public override void Execute(IActionState<CounterState, IAction> action)
        {
            switch (action.Action)
            {
                case IncrementCounter a:
                    //Console.WriteLine("Call Increment Counter");
                    action.NewState.Count = action.State.Count +  a.Step;
                    break;
                case DecrementCounter a:
                    //Console.WriteLine("Call Decrement Counter");
                    action.NewState.Count =action.State.Count - a.Step;
                    break;
                default:
                    break;
            }
        }
    }
}
