using System;

namespace WeRedux
{
    public interface IStore<TState, TAction> :IDispatcher<TAction>, IDisposable
        where TState:new()
        where TAction :IAction
    {
        TState Initial { get; }
        IObservable<TState> OnChanged { get; }
        IObservable<TState> OnReduced { get; }
        IObservable<TState> OnTimeTraveled { get; }
        void AddReducer<TReducer>(TReducer reducer) where TReducer : Reducer<TState, TAction>;
        void AddReducer<TReducer>() where TReducer : Reducer<TState, TAction>,new();
       // IObservable<IActionState<TState,T>> On<T>() where T:TAction;
    }
}
