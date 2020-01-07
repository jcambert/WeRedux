using System;

namespace WeRedux
{
    public class HistoricEntry<TState, TAction>
        where TState:new()
        where TAction:IAction
    {
        public HistoricEntry(IActionState<TState, TAction> actionState )
        {
            ActionState = actionState;
        }

        public IActionState<TState, TAction> ActionState { get; }
        public DateTime Time { get; } = DateTime.UtcNow;
    }
}
