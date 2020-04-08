namespace WeRedux
{
    /*   public abstract class Reducer<TState, TAction>:IReducer<TState,TAction>, IDisposable
           where TState:new()
       {
           private readonly Subject<IActionState<TState, TAction>> reduce = new Subject<IActionState<TState, TAction>>();

           public Reducer()
           {

           }

           public IObservable<IActionState<TState, TAction>> OnReduce => reduce.AsObservable();

           public void Reduce(TState newState, TState state, TAction action)
           {
               reduce.OnNext(new ActionState<TState,TAction>() { Action=action,State=state });
           }
           public abstract void Execute( IActionState<TState, TAction> action);

   #pragma warning disable CA1063 // Implement IDisposable Correctly
           public void Dispose() => reduce.Dispose();
   #pragma warning restore CA1063 // Implement IDisposable Correctly
       }*/
}
