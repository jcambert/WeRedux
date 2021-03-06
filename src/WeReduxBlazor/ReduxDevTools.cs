﻿using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Reactive.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using WeRedux;
using MicroS_Common.Actions;
namespace WeReduxBlazor
{
    public partial class ReduxDevTools<TState, TAction> : IJsReduxInvokable, IDisposable
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
        [Parameter]
        public bool UseLocalStorage
        {
            get => Redux.UseLocalStorage;
            set
            {
                Redux.UseLocalStorage = value;
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                Console.WriteLine("Rendering 1st ReduxDevTools");
                await JSRuntime.InvokeVoidAsync("weredux.addStore", DotNetObjectReference.Create(this), Redux.Name, this.ToString());

                Store.OnChanged.Subscribe(async (o) =>
                {
                    try
                    {
                        await JSRuntime.InvokeVoidAsync($"weredux.stores.{Redux.Name.ToLowerInvariant()}.onChanged", o.Mutation, JsonSerializer.Serialize(o.State));

                    }
                    catch  { }
                    try
                    {

                        await Redux.SaveTolocalstorageAsync();
                    }
                    catch{ } 
                });

                Store.OnInitialStateChanged.Subscribe(async (State) =>
                {
                    await Redux.ClearLocalStorageAsync();
                    await JSRuntime.InvokeVoidAsync($"weredux.stores.{Redux.Name.ToLowerInvariant()}.init", JsonSerializer.Serialize(State));

                });

                //await Redux.LoadStoreAsync();
                Console.WriteLine("Rendered 1st ReduxDevTools");
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
                    Console.WriteLine("ReduxDevTools Disposing");
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
