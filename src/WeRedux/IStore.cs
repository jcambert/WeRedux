using System;
using System.Collections.Generic;

namespace WeRedux
{
    public interface IStore<TState, TAction> :IStoreEvents<TState,TAction>, IDisposable
        where TState:new()
        where TAction :IAction
    {
        TState InitialState { get; }
        TState State { get; }
        void AddReducer<TReducer>(TReducer reducer) where TReducer : Reducer<TState, TAction>;
        void AddReducer<TReducer>() where TReducer : Reducer<TState, TAction>,new();
        IObservable<IActionState<TState, TAction>> On<T>() where T : TAction;
        void TravelTo(int index);
        List<HistoricEntry<TState, TAction>> History { get; }
        string Name { get; }
        void Reset();
    }
}
