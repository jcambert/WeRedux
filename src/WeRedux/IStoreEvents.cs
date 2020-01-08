using System;
using System.Collections.Generic;
using System.Text;

namespace WeRedux
{
    public interface IStoreEvents<TState, TAction> :IDispatcher<TAction>
    {
        IObservable<TState> OnInitialStateChanged { get; }
        IObservable<TState> OnChanged { get; }
        IObservable<IActionState<TState, TAction>> OnAdd { get; }
        IObservable <TState> OnReduced { get; }
        IObservable<bool> OnTimeTravel { get; }

        IObservable<string> OnMutation { get; }
    }
}
