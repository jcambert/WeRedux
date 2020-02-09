using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
#if DEBUG
using System.Diagnostics;
#endif
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text.Json;
using System.Threading.Tasks;
using WeRedux;
namespace WeReduxBlazor
{
    public partial class ReduxDevTools<TState, TAction>: IJsReduxInvokable,IDisposable
        where TState : new()
        where TAction : IAction
    {

        public ReduxDevTools()
        {

        }


        [Inject]
        public IJSRuntime JSRuntime { get; set; }
        [Inject]
        public IRedux<TState, TAction> Redux { get; set; }

        public IStore<TState, TAction> Store => Redux?.Store;

        IDisposable foo;
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                

            /*    foo=Store.OnTravelTo.Subscribe((timeLaps) =>
                {
                        this.StateHasChanged();
                });
                */

                await JSRuntime.InvokeVoidAsync("window.addStore", DotNetObjectReference.Create(this), Redux.Name, this.ToString());

                Store.OnChanged.Subscribe(async (o) =>
                {

                    await JSRuntime.InvokeVoidAsync($"window.weredux.{Redux.Name.ToLowerInvariant()}.onChanged", o.Mutation, JsonSerializer.Serialize(o.State));
                    await Redux.SaveTolocalstorageAsync();
                });

                Store.OnInitialStateChanged.Subscribe(async (State) =>
                {
                    await Redux.ClearLocalStorageAsync();
                    await JSRuntime.InvokeVoidAsync($"window.weredux.{Redux.Name.ToLowerInvariant()}.init", JsonSerializer.Serialize(State));

                });

                await Redux.LoadStoreAsync();
            }


        }

        [JSInvokable("Reset")]
        public void Reset()
        {
            Console.WriteLine($"Reset Store {Store.Name}");
            Redux.Reset();
        }

        [JSInvokable("Dispatch")]
        public void Dispatch(string action)
        {
            Console.WriteLine($"Disptach action {action} to Store {Store.Name}");
            Redux.Dispatch(action);
        }

        [JSInvokable("TravelTo")]
        public void TravelTo(int index)
        {
            Console.WriteLine($"Travel Store {Store.Name} to Index {index}");
            Redux.TravelTo(index);
        }

        #region IDisposable Support
        private bool disposedValue = false; // Pour détecter les appels redondants

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: supprimer l'état managé (objets managés).
                    Console.WriteLine("ReduxDevTools Dispose Foo");
                    foo?.Dispose();
                }

                // TODO: libérer les ressources non managées (objets non managés) et remplacer un finaliseur ci-dessous.
                // TODO: définir les champs de grande taille avec la valeur Null.

                disposedValue = true;
            }
        }

        // TODO: remplacer un finaliseur seulement si la fonction Dispose(bool disposing) ci-dessus a du code pour libérer les ressources non managées.
        // ~ReduxDevTools()
        // {
        //   // Ne modifiez pas ce code. Placez le code de nettoyage dans Dispose(bool disposing) ci-dessus.
        //   Dispose(false);
        // }

        // Ce code est ajouté pour implémenter correctement le modèle supprimable.
        public void Dispose()
        {
            // Ne modifiez pas ce code. Placez le code de nettoyage dans Dispose(bool disposing) ci-dessus.
            Dispose(true);
            // TODO: supprimer les marques de commentaire pour la ligne suivante si le finaliseur est remplacé ci-dessus.
            // GC.SuppressFinalize(this);
        }
        #endregion

    }
}
