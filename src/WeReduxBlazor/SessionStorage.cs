using Microsoft.JSInterop;

namespace WeReduxBlazor
{
    public sealed class SessionStorage : StorageBase
    {
        protected override string StorageTypeName => nameof(SessionStorage);

        public SessionStorage(IJSRuntime jsRuntime) : base(jsRuntime)
        {
        }
    }

}
