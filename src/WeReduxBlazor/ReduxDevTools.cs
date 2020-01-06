using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using WeRedux;
namespace WeReduxBlazor
{
    public partial class ReduxDevTools<TState,TAction>
        where TState:new()
        where TAction : IAction
    {
        [JSInvokable("DevToolsReady")]
        public void DevToolsReady(string message)
        {
            this.Message = message;
            this.StateHasChanged();
        }

        [Inject]
        public IStore<TState,TAction> Store { get; set; }
    }
}
