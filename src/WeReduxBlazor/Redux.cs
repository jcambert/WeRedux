using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WeRedux;

namespace WeReduxBlazor
{
    public interface IRedux<TState, TAction> : IStoreEvents<TState, TAction>, IJsReduxInvokable
         where TState : new()
         where TAction : IAction
    {
        string Name { get; }
        Task LoadStoreAsync();

        IStore<TState, TAction> Store { get; }

        bool TravelFromStorage { get; }

        Task LoadFromLocalStorageAsync();

        Task SaveTolocalstorageAsync();

        Task ClearLocalStorageAsync();

        Task ClearAllLocalStorageAsync();

    }
    public class Redux<TState, TAction> : IRedux<TState, TAction>
         where TState : new()
         where TAction : IAction
    {

        public Redux(IStore<TState, TAction> store, LocalStorage localStorage, ILogger<Redux<TState, TAction>> logger, string name)
        {
            Console.WriteLine("Create New Redux");
            this.Store = store;
            this.Storage = localStorage;
            this.Logger = logger;
            this.Name = name;
            OnLoadFromLocalStorage.Subscribe(b =>
            {
                travelFromLocalStorage = b;
            });
            /* new Action(async () =>
             {

             })();*/
        }
        private readonly Subject<bool> _onLoadFromLocalStorage = new Subject<bool>();

        private bool travelFromLocalStorage = false;




        public async Task LoadStoreAsync()
        {

            if (Store.IsEmpty)
                await LoadFromLocalStorageAsync();
            else
                await SaveTolocalstorageAsync();//Save init state

        }

        public TState State => Store.State;
        public IStore<TState, TAction> Store { get; }
        public LocalStorage Storage { get; }

        public ILogger<Redux<TState, TAction>> Logger { get; }
        public string Name { get; }

        public IObservable<IMutationstate<TState>> OnChanged => Store.OnChanged;

        //public IObservable<TState> OnReduced => Store.OnReduced;

        public IObservable<int> OnTravelTo => Store.OnTravelTo;

        //public IObservable<TState> OnTravel => Store.OnTravel;

        public IObservable<TState> OnInitialStateChanged => Store.OnInitialStateChanged;

        //public IObservable<string> OnMutation => Store.OnMutation;

        //public IObservable<IActionState<TState, TAction>> OnAdd => Store.OnAdd;

        public IObservable<bool> OnLoadFromLocalStorage => _onLoadFromLocalStorage.AsObservable();

//        public IObservable<IActionState<TState, TAction>> OnMutated => Store.OnMutated;

        public bool TravelFromStorage => travelFromLocalStorage;

        public async Task SaveTolocalstorageAsync()
        {
            if (travelFromLocalStorage) return;

            try
            {
                await Storage.SetItemAsync(Name, Store.ToJson());
            }
            catch (Exception ex)
            {
#if DEBUG

#endif
            }
        }
        public async Task LoadFromLocalStorageAsync()
        {
            if (Storage == null || travelFromLocalStorage) return;
            //_onLoadFromLocalStorage.OnNext(true);
            travelFromLocalStorage = true;
            var content = await Storage.GetItemAsync(Name);
            if (string.IsNullOrEmpty(content))
            {
                //_onLoadFromLocalStorage.OnNext(false);
                travelFromLocalStorage = false;
                return;
            }
            var history = content.GetHistoryContent();
            List<Task> tasks = new List<Task>();
            foreach (var action in history.Actions)
            {
                Console.WriteLine($"Load From LocalStorage:{action}");
                Logger.LogTrace($"Load From LocalStorage:{action}");
                //tasks.Add(DispatchAsync(action));
                Dispatch(action);


            }
            //    TravelTo(history.Actions.Count - 1);
            //_onLoadFromLocalStorage.OnNext(false);
            //Task.WaitAll(tasks.ToArray());
            TravelTo(history.Actions.Count - 1);
            travelFromLocalStorage = false;
        }

        public async Task ClearAllLocalStorageAsync()
        {

            if (Storage == null || travelFromLocalStorage) return;
            await Storage.ClearAsync();
        }


        public async Task ClearLocalStorageAsync()
        {
            if (Storage == null || travelFromLocalStorage) return;
            await Storage.RemoveItemAsync(Name);
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
        public void Dispatch<T>(T action) where T : TAction
        {
            Store.Dispatch<T>(action);
        }

        public void Dispatch<T>(Action<T> action = null) where T : TAction, new()
        {
            Store.Dispatch<T>(action);
        }

        [JSInvokable("TravelTo")]
        public void TravelTo(int index)
        {
            Console.WriteLine($"Travel Store {Store.Name} to Index {index}");
            Store.TravelTo(index);
        }

        public override string ToString()
        {
            return JsonSerializer.Serialize(State);
        }

       
    }
}
