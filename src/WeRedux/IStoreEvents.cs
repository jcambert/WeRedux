using System;
using System.Collections.Generic;
using System.Text;

namespace WeRedux
{
    public interface IStoreEvents<TState, TAction> :IDispatcher<TAction>
         where TState : new()
        where TAction : IAction
    {
        IObservable<TState> OnInitialStateChanged { get; }
        IObservable<IMutationstate<TState>> OnChanged { get; }
     //   IObservable<IActionState<TState, TAction>> OnAdd { get; }
       // IObservable <TState> OnReduced { get; }
        IObservable<int> OnTravelTo { get; }
       // IObservable<TState> OnTravel { get; }
        //IObservable<string> OnMutation { get; }

//        IObservable<IActionState<TState, TAction>> OnMutated { get; }
    }
}
