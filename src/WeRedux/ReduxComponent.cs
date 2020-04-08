namespace WeRedux
{
    public class ReduxComponent<TState, TAction>
        where TState : new()
        where TAction : IAction
    {
        protected IStore<TState, TAction> Store { get; private set; }
        public ReduxComponent(IStore<TState, TAction> store)
        {
            Store = store;
        }
    }
}
