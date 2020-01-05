using System;

namespace WeRedux
{
    public interface IStore<TState, TAction> :IDispatcher<TAction>, IDisposable
        where TState:new()
        where TAction :IAction
    {
        IObservable<TState> OnReduced { get; }
        void AddReducer<TReducer>(TReducer reducer) where TReducer : Reducer<TState, TAction>;
        void AddReducer<TReducer>() where TReducer : Reducer<TState, TAction>,new();
       // IObservable<IActionState<TState,T>> On<T>() where T:TAction;
    }
}
