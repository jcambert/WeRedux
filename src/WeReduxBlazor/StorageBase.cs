using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using System.Reactive.Linq;
namespace WeReduxBlazor
{
    public abstract class StorageBase:IDisposable
    {
        private readonly IJsRuntimeAccess _jsRuntime;
        private readonly string _fullTypeName;
        private readonly Subject<StorageEventArgs> _storageChanged = new Subject<StorageEventArgs>();
        //private EventHandler<StorageEventArgs> _storageChanged;

        protected abstract string StorageTypeName { get; }

        protected internal StorageBase(IJSRuntime jsRuntime)
        {
            if (jsRuntime is IJSInProcessRuntime rt)
                _jsRuntime = new ClientSideJsRuntimeAccess(rt);
            else _jsRuntime = new ServerSideJsRuntimeAccess(jsRuntime);
            _fullTypeName = GetType().FullName.Replace('.', '_');

            _jsRuntime.JsRuntimeInvokeAsync<object>(
                        $"{_fullTypeName}.AddEventListener",
                        DotNetObjectReference.Create(this)
                    );
        }

        public void Clear()
        {
            _jsRuntime.JsRuntimeInvoke<object>($"{_fullTypeName}.Clear");
        }

        public async ValueTask ClearAsync(CancellationToken cancellationToken = default)
        {
            await _jsRuntime.JsRuntimeInvokeAsync<object>($"{_fullTypeName}.Clear", Enumerable.Empty<object>(), cancellationToken);
        }

        public string GetItem(string key)
        {
            return _jsRuntime.JsRuntimeInvoke<string>($"{_fullTypeName}.GetItem", key);
        }

        public ValueTask<string> GetItemAsync(string key, CancellationToken cancellationToken = default)
        {
            return _jsRuntime.JsRuntimeInvokeAsync<string>($"{_fullTypeName}.GetItem", new object[] { key }, cancellationToken);
        }

        public T GetItem<T>(string key)
        {
            var json = GetItem(key);
            return string.IsNullOrEmpty(json) ? default : JsonSerializer.Deserialize<T>(json);
        }

        public async Task<T> GetItemAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            var json = await GetItemAsync(key, cancellationToken);
            return string.IsNullOrEmpty(json) ? default : JsonSerializer.Deserialize<T>(json);
        }

        public string Key(int index)
        {
            return _jsRuntime.JsRuntimeInvoke<string>($"{_fullTypeName}.Key", index);
        }

        public ValueTask<string> KeyAsync(int index, CancellationToken cancellationToken = default)
        {
            return _jsRuntime.JsRuntimeInvokeAsync<string>($"{_fullTypeName}.Key", new object[] { index }, cancellationToken);
        }

        public int Length => _jsRuntime.JsRuntimeInvoke<int>($"{_fullTypeName}.Length");

        public ValueTask<int> LengthAsync(CancellationToken cancellationToken = default) => _jsRuntime.JsRuntimeInvokeAsync<int>($"{_fullTypeName}.Length", Enumerable.Empty<object>(), cancellationToken);

        public void RemoveItem(string key)
        {
            _jsRuntime.JsRuntimeInvoke<object>($"{_fullTypeName}.RemoveItem", key);
        }

        public async ValueTask RemoveItemAsync(string key, CancellationToken cancellationToken = default)
        {
            await _jsRuntime.JsRuntimeInvokeAsync<object>($"{_fullTypeName}.RemoveItem", new object[] { key }, cancellationToken);
        }

        public void SetItem(string key, string data)
        {
            _jsRuntime.JsRuntimeInvoke<object>($"{_fullTypeName}.SetItem", key, data);
        }

        public async ValueTask SetItemAsync(string key, string data, CancellationToken cancellationToken = default)
        {
            await _jsRuntime.JsRuntimeInvokeAsync<object>($"{_fullTypeName}.SetItem", new object[] { key, data }, cancellationToken);
        }

        public void SetItem(string key, object data)
        {
            SetItem(key, JsonSerializer.Serialize(data));
        }

        public ValueTask SetItemAsync(string key, object data, CancellationToken cancellationToken = default)
        {
            return SetItemAsync(key, JsonSerializer.Serialize(data), cancellationToken);
        }

        public string this[string key]
        {
            get => _jsRuntime.JsRuntimeInvoke<string>($"{_fullTypeName}.GetItemString", key);
            set => _jsRuntime.JsRuntimeInvoke<object>($"{_fullTypeName}.SetItemString", key, value);
        }

        public string this[int index]
        {
            get => _jsRuntime.JsRuntimeInvoke<string>($"{_fullTypeName}.GetItemNumber", index);
            set => _jsRuntime.JsRuntimeInvoke<object>($"{_fullTypeName}.SetItemNumber", index, value);
        }
        public IObservable<StorageEventArgs> OnStorageChanged => _storageChanged.AsObservable();


        [JSInvokable("StorageChangedTo")]
        public virtual void StorageChangedTo(string key, object oldValue, object newValue)
        {
            
            _storageChanged.OnNext(new StorageEventArgs()
            {
                Key = key,
                OldValue = oldValue,
                NewValue = newValue,
            });

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
                    _storageChanged.Dispose();
                }

                // TODO: libérer les ressources non managées (objets non managés) et remplacer un finaliseur ci-dessous.
                // TODO: définir les champs de grande taille avec la valeur Null.

                disposedValue = true;
            }
        }

        // TODO: remplacer un finaliseur seulement si la fonction Dispose(bool disposing) ci-dessus a du code pour libérer les ressources non managées.
        // ~StorageBase()
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
