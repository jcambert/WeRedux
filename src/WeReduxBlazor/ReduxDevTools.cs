using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Text.Json;
using System.Threading.Tasks;
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
            Console.WriteLine($"Reset Store {Store.Name}");
            Store.Reset();
        }

        [JSInvokable("Dispatch")]
        public void Dispatch(string action)
        {
            Console.WriteLine($"Disptach action {action} to Store {Store.Name}");
            Store.Dispatch(action);
        }

        [JSInvokable("TravelTo")]
        public void TravelTo(int index)
        {
            Console.WriteLine($"Travel Store {Store.Name} to Index {index}");
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

        public IObservable<IActionState<TState, TAction>> OnAdd => Store.OnAdd;

        public async Task SaveTolocalstorageAsync()
        {
            if (!UseLocalStorage || LocalStorage == null) return;

            await LocalStorage.SetItemAsync(Name, Store.ToJson());
        }
        public async Task LoadFromLocalStorageAsync()
        {
            if (!UseLocalStorage || LocalStorage == null) return;
            var content=await LocalStorage.GetItemAsync(Name);
            if (string.IsNullOrEmpty( content)) return;
            var history= content.GetHistoryContent();
            foreach (var action in history.Actions)
            {
                Dispatch(action);
            }
            TravelTo(history.Actions.Count-1);
        }

        public async Task ClearLocalStorageAsync()
        {
            if (!UseLocalStorage || LocalStorage == null) return;
            await LocalStorage.ClearAsync();
        }

        public override string ToString()
        {
            return JsonSerializer.Serialize(State);
        }
    }
}
