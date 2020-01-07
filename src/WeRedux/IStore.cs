using System;

namespace WeRedux
{
    public interface IStore<TState, TAction> :IStoreEvents<TState,TAction>, IDisposable
        where TState:new()
        where TAction :IAction
    {
        TState Initial { get; }
        TState State { get; }
        void AddReducer<TReducer>(TReducer reducer) where TReducer : Reducer<TState, TAction>;
        void AddReducer<TReducer>() where TReducer : Reducer<TState, TAction>,new();
        IObservable<IActionState<TState, TAction>> On<T>() where T : TAction;
        void TravelTo(int index);
    }
}
