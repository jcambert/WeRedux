using System;
using MicroS_Common.Actions;
namespace WeRedux
{
    public class HistoricEntry<TState, TAction>
        where TState : new()
        where TAction : IAction
    {
        public HistoricEntry(IActionState<TState, TAction> actionState)
        {
            ActionState = actionState;
        }

        public IActionState<TState, TAction> ActionState { get; }
        public DateTime Time { get; } = DateTime.UtcNow;

        public string Mutation => ActionState.Action.GetMutation();
        public override string ToString()
        {
            return $"Historic entry with Action {Mutation}";
        }
    }
}
