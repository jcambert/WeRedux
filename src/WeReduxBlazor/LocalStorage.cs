using Microsoft.JSInterop;

namespace WeReduxBlazor
{
    public sealed class LocalStorage : StorageBase
    {
        protected override string StorageTypeName => nameof(LocalStorage);

        public LocalStorage(IJSRuntime jsRuntime) : base(jsRuntime)
        {
        }
    }
}
