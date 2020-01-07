using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Text.Json;
using WeRedux;
namespace WeReduxBlazor
{
    public partial class ReduxDevTools<TState, TAction> : IStoreEvents<TState, TAction>
        where TState : new()
        where TAction : IAction
    {
        public ReduxDevTools()
        {

        }
        [JSInvokable("DevToolsReady")]
        public void DevToolsReady(string message)
        {
            this.Message = message;
            this.StateHasChanged();
        }

        public string Message { get; set; }

        public void Dispatch(TAction action)
        {
            Store.Dispatch(action);
            
        }

        public void Dispatch<T>() where T : TAction, new()
        {
            Store.Dispatch<T>();
        }

        [JSInvokable("Reset")]
        public void Reset()
        {
            Store.Reset();
        }

        [JSInvokable("Dispatch")]
        public void Dispatch(string action)
        {
            Store.Dispatch(action);
        }

        [JSInvokable("TravelTo")]
        public void TravelTo(int index)
        {
            Store.TravelTo(index);
        }

        private IStore<TState, TAction> _store;
        [Inject]
        public IStore<TState, TAction> Store
        {
            get { return _store; }
            set { _store = value; }
        }
        public TState State => Store.State;
        [Inject] IJSRuntime JSRuntime { get; set; }
        public IObservable<TState> OnChanged => Store.OnChanged;

        public IObservable<TState> OnReduced => Store.OnReduced;

        public IObservable<bool> OnTimeTravel => Store.OnTimeTravel;

        public IObservable<TState> OnInitialStateChanged => Store.OnInitialStateChanged;

        public IObservable<string> OnMutation => Store.OnMutation;

        public override string ToString()
        {
            return JsonSerializer.Serialize(State);
        }
    }
}
