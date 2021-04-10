using MicroS_Common.Actions;
using Microsoft.AspNetCore.Components;
namespace WeReduxBlazor
{
    public partial class Storage<TState, TAction, TStorage>
        where TState : new()
        where TAction : IAction
        where TStorage : StorageBase
    {
        [Inject]
        public TStorage StoreSession { get; set; }
    }
}
