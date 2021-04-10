using System;
using MicroS_Common.Actions;
namespace WeRedux
{
    public interface IStoreEvents<TState, TAction> : IDispatcher<TAction>
         where TState : new()
        where TAction : IAction
    {
        IObservable<TState> OnInitialStateChanged { get; }
        IObservable<IMutationstate<TState>> OnChanged { get; }
        IObservable<int> OnTravelTo { get; }
    }
}
