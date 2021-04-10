using System;
using System.Collections.Generic;
using MicroS_Common.Actions;
namespace WeRedux
{
    public interface IStore<TState, TAction> : IStoreEvents<TState, TAction>, IDisposable
        where TState : new()
        where TAction : IAction
    {
        TState InitialState { get; }
        TState State { get; }


        IObservable<TAction> On<T>() where T : TAction;
        void TravelTo(int index);
        List<HistoricEntry<TState, TAction>> History { get; }
        string Name { get; }
        bool IsEmpty { get; }
        void Reset();


        bool Travelling { get; }

        void StateChanged<T>(T action) where T : TAction;
    }
}
